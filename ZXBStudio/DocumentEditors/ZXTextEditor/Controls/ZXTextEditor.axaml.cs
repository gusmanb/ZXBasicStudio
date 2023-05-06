using Avalonia.Controls;
using AvaloniaEdit.Highlighting.Xshd;
using AvaloniaEdit.Highlighting;
using System;
using ZXBasicStudio.Classes;
using Avalonia.Media;
using System.IO;
using Avalonia.LogicalTree;
using Avalonia;
using Newtonsoft.Json.Linq;
using Avalonia.Input;
using Avalonia.Threading;
using AvaloniaEdit.Folding;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.LanguageDefinitions;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.Folding;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.Breakpoints;
using ZXBasicStudio.DocumentModel.Classes;
using AvaloniaEdit;

namespace ZXBasicStudio.DocumentEditors.ZXTextEditor.Controls
{
    public partial class ZXTextEditor : ZXDocumentEditorBase, IObserver<AvaloniaPropertyChangedEventArgs>
    {
        #region Constants
        const int FONT_MAX_SIZE = 64;
        const int FONT_MIN_SIZE = 7;
        #endregion

        #region Protected properties
        protected virtual bool allowsBreakpoints { get { return false; } }
        protected virtual LanguageDefinitionBase? langDef { get { return null; } }
        protected virtual IBrush? searchMarkerBrush { get { return null; } }
        protected virtual AbstractFoldingStrategy? foldingStrategy { get { return null; } }
        protected virtual Regex? regCancelBreakpoint { get { return null; } }
        protected virtual char? commentChar { get { return null; } }
        #endregion

        #region Private variables
        BreakPointMargin? bpMargin;
        DispatcherTimer? updateFoldingsTimer;
        bool firstRender = false;
        PausedLineBackgroundRender? blRender;
        FoldingManager? fManager;
        string _docPath;
        string _docName;
        #endregion

        #region Events
        public override event EventHandler? DocumentRestored;
        public override event EventHandler? DocumentModified;
        public override event EventHandler? DocumentSaved;
        public override event EventHandler? RequestSaveDocument;
        #endregion

        #region ZXDocumentBase properties
        public override string DocumentName => _docName;
        public override string DocumentPath => _docPath;
        public override bool Modified
        { 
            get 
            { 
                if (_docPath == ZXConstants.DISASSEMBLY_DOC || _docPath == ZXConstants.ROM_DOC) 
                    return false; 
                
                return editor?.IsModified ?? false; 
            } 
        }
        #endregion

        #region Text editor properties
        public bool Readonly { get { return editor.IsReadOnly; } set { editor.IsReadOnly = value; } }
        public string? Text { get { return editor.Document?.Text; } set { if (editor.Document != null) editor.Document.Text = value; } }
        public new double FontSize { get { return editor.FontSize; } set { editor.FontSize = value; } }
        #endregion

        #region Debugging properties
        public int? BreakLine
        {
            get { return blRender?.Line; }
            set
            {
                if (blRender == null)
                    return;

                blRender.Line = value;
                editor.TextArea.TextView.InvalidateVisual();
                if (value != null)
                {
                    editor.TextArea.Caret.Line = value.Value + 1;
                    editor.ScrollTo(value.Value + 1, 1);
                }
            }
        }
        #endregion

        #region Constructors
        public ZXTextEditor() : this("Untitled")
        { 
        }
        public ZXTextEditor(string DocumentPath)
        {
            InitializeComponent();

            editor.FontSize = ZXOptions.Current.EditorFontSize;
            editor.WordWrap = ZXOptions.Current.WordWrap;

            editor.TextArea.LeftMargins.Insert(0, bpMargin);
            editor.TextArea.PointerWheelChanged += Editor_PointerWheelChanged;
            editor.TextArea.SelectionBrush = Brush.Parse("#305da9f6");
            editor.TextArea.SelectionBorder = new Pen(Brushes.White, 1);
            editor.TextArea.SelectionCornerRadius = 2;
            editor.TextArea.KeyUp += TextArea_KeyUp;
            editor.TextArea.LayoutUpdated += TextArea_LayoutUpdated;

            if (langDef != null)
            {
                using var xr = langDef.AsReader;
                editor.SyntaxHighlighting = HighlightingLoader.Load(xr, HighlightingManager.Instance);
            }

            if (allowsBreakpoints)
            {
                bpMargin = new BreakPointMargin(editor);
                bpMargin.BreakpointAdded += BpMargin_BreakpointAdded;
                bpMargin.BreakpointRemoved += BpMargin_BreakpointRemoved;
                blRender = new PausedLineBackgroundRender();
                editor.TextArea.TextView.BackgroundRenderers.Add(blRender);
                editor.TextArea.LeftMargins[0] = bpMargin;
            }

            _docPath = DocumentPath;
            _docName = Path.GetFileName(DocumentPath);

            UpdateFileName(null, _docPath);
            editor.GetPropertyChangedObservable(TextEditor.IsModifiedProperty).Subscribe(this);
            editor.TemplateApplied += Editor_TemplateApplied;

            editor.Options.ConvertTabsToSpaces = true;
            editor.Options.EnableTextDragDrop = true;
            editor.Options.EnableRectangularSelection = true;

            var breaks = BreakpointManager.BreakPoints(DocumentPath);

            foreach (var bp in breaks)
                bpMargin?.Breakpoints.Add(bp);
        }
        #endregion

        #region Editor event handling
        private void Editor_TemplateApplied(object? sender, Avalonia.Controls.Primitives.TemplateAppliedEventArgs e)
        {
            if (searchMarkerBrush != null)
                editor.SearchPanel.SetSearchResultsBrush(searchMarkerBrush);
        }
        private void TextArea_LayoutUpdated(object? sender, EventArgs e)
        {
            if (!firstRender)
            {
                firstRender = true;

                if (blRender?.Line != null)
                {
                    editor.TextArea.Caret.Line = blRender.Line.Value + 1;
                    editor.ScrollTo(blRender.Line.Value + 1, 1);
                }
            }
        }
        private void TextArea_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.S && e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Control) && RequestSaveDocument != null)
                RequestSaveDocument(this, EventArgs.Empty);
        }
        private void Editor_PointerWheelChanged(object? sender, Avalonia.Input.PointerWheelEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Control))
            {
                UpdateFontSize(e.Delta.Y > 0);
                e.Handled = true;
            }

        }
        #endregion

        #region Breakpoint management
        private void BpMargin_BreakpointRemoved(object? sender, BreakPointMargin.BreakpointEventArgs e)
        {
            if (DocumentName == "Untitled")
            {
                e.Cancel = true;
                return;
            }

            BreakpointManager.RemoveBreakpoint(_docPath, e.BreakPoint);
        }
        private void BpMargin_BreakpointAdded(object? sender, BreakPointMargin.BreakpointEventArgs e)
        {
            if (DocumentName == "Untitled")
            {
                e.Cancel = true;
                return;
            }

            var line = editor.Document.Lines[e.BreakPoint.Line - 1];
            var text = editor.Document.GetText(line);

            if (string.IsNullOrWhiteSpace(text))
                e.Cancel = true;

            if (regCancelBreakpoint != null && regCancelBreakpoint.IsMatch(text))
                e.Cancel = true;

            e.BreakPoint.File = _docPath;
            BreakpointManager.AddBreakpoint(e.BreakPoint);

        }
        public void ClearBreakpoints()
        {
            bpMargin?.Breakpoints.Clear();
            InvalidateVisual();
            bpMargin?.InvalidateVisual();
        }
        #endregion

        #region ZXDocumentBase functions

        public override bool SaveDocument(TextWriter OutputLog)
        {
            try
            {
                if (_docPath == ZXConstants.DISASSEMBLY_DOC || _docPath == ZXConstants.ROM_DOC)
                {
                    OutputLog.WriteLine($"This file cannot be saved.");
                    return false;
                }

                using (var str = File.Create(_docPath))
                    editor.Save(str);

                OutputLog.WriteLine($"File {_docPath} saved successfully.");

                return true;
            }
            catch(Exception ex) 
            {
                OutputLog.WriteLine($"Error saving file {_docPath}: {ex.Message}");
                return false;
            }
        }
        public override bool RenameDocument(string NewName, TextWriter OutputLog)
        {
            try 
            {
                UpdateFileName(_docPath, NewName);
                return true;
            }
            catch(Exception ex) 
            {
                OutputLog.WriteLine($"Error internally updating the document name: {ex.Message}");
                return false;
            }
        }
        public override bool CloseDocument(TextWriter OutputLog, bool ForceClose)
        {
            return true;
        }

        #endregion

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
        }

        #region Text editor functions
        public void FocusText()
        {
            editor.TextArea.Focus();
            editor.TextArea.Caret.BringCaretToView();
        }
        public void Collapse()
        {
            if (fManager == null)
                return;

            foreach (var fold in fManager.AllFoldings)
                fold.IsFolded = true;
        }
        public void Expand()
        {
            if (fManager == null)
                return;

            foreach (var fold in fManager.AllFoldings)
                fold.IsFolded = false;
        }
        public void CommentSelection()
        {
            if (editor.IsReadOnly || commentChar == null || editor.TextArea.Selection == null)
                return;
            
            TextDocument document = editor.TextArea.Document;

            if (editor.TextArea.Selection.Length == 0)
            {
                var caretPos = editor.TextArea.Caret.Position;
                var line = document.GetLineByNumber(caretPos.Line);
                document.Insert(line.Offset, $"{commentChar}");
            }
            else
            {
                IEnumerable<SelectionSegment> selectionSegments = editor.TextArea.Selection.Segments;
                
                foreach (SelectionSegment segment in selectionSegments)
                {
                    int lineStart = document.GetLineByOffset(segment.StartOffset).LineNumber;
                    int lineEnd = document.GetLineByOffset(segment.EndOffset).LineNumber;
                    for (int i = lineStart; i <= lineEnd; i++)
                    {
                        document.Insert(document.GetLineByNumber(i).Offset, $"{commentChar}");
                    }
                }
            }
        }
        public void UncommentSelection()
        {
            if (editor.IsReadOnly || commentChar == null || editor.TextArea.Selection == null)
                return;

            TextDocument document = editor.TextArea.Document;
            Regex regComment = new Regex($"^(\\s*?)({commentChar})");

            IEnumerable<SelectionSegment> selectionSegments = editor.TextArea.Selection.Segments;
            if (editor.TextArea.Selection.Length == 0)
            {
                var caretPos = editor.TextArea.Caret.Position;
                var line = document.GetLineByNumber(caretPos.Line);
                var text = document.GetText(line.Offset, line.Length);
                text = regComment.Replace(text, "$1");
                document.Replace(line.Offset, line.Length, text);
            }
            else
            {
                foreach (SelectionSegment segment in selectionSegments)
                {
                    int lineStart = document.GetLineByOffset(segment.StartOffset).LineNumber;
                    int lineEnd = document.GetLineByOffset(segment.EndOffset).LineNumber;
                    for (int i = lineStart; i <= lineEnd; i++)
                    {
                        var line = document.GetLineByNumber(i);
                        var text = document.GetText(line.Offset, line.Length);
                        text = regComment.Replace(text, "$1");
                        document.Replace(line.Offset, line.Length, text);
                    }
                }
            }
        }
        private void UpdateFileName(string? OldName, string NewName)
        {
            if (fManager != null)
            {
                FoldingManager.Uninstall(fManager);
                fManager = null;
            }

            if (updateFoldingsTimer != null)
            {
                updateFoldingsTimer.Stop();
                updateFoldingsTimer = null;
            }

            if (!string.IsNullOrWhiteSpace(OldName) && OldName != "Untitled")
                BreakpointManager.UpdateFileName(OldName, NewName);

            if (!string.IsNullOrWhiteSpace(NewName) && NewName != "Untitled" && NewName != ZXConstants.DISASSEMBLY_DOC && NewName != ZXConstants.ROM_DOC)
                editor.Document = new AvaloniaEdit.Document.TextDocument(File.ReadAllText(NewName));
            else
                editor.Document = new AvaloniaEdit.Document.TextDocument();

            if (NewName == ZXConstants.DISASSEMBLY_DOC || NewName == ZXConstants.ROM_DOC)
                editor.IsReadOnly = true;

            if (foldingStrategy != null)
            {
                fManager = FoldingManager.Install(editor.TextArea);
                updateFoldingsTimer = new DispatcherTimer();
                updateFoldingsTimer.Interval = TimeSpan.FromSeconds(2);
                updateFoldingsTimer.Tick += UpdateFoldingsTimer_Tick;
                updateFoldingsTimer.IsEnabled = true;
            }

            _docPath = NewName;
            _docName = Path.GetFileName(NewName);
        }
        private void UpdateFoldingsTimer_Tick(object? sender, EventArgs e)
        {
            if (updateFoldingsTimer == null || fManager == null || foldingStrategy == null)
                return;

            updateFoldingsTimer.IsEnabled = false;
            foldingStrategy.UpdateFoldings(fManager, editor.Document);
        }
        public void UpdateFontSize(bool increase)
        {
            double currentSize = editor.FontSize;

            if (increase)
            {
                if (currentSize < FONT_MAX_SIZE)
                {
                    double newSize = Math.Min(FONT_MAX_SIZE, currentSize + 1);
                    editor.FontSize = newSize;
                }
            }
            else
            {
                if (currentSize > FONT_MIN_SIZE)
                {
                    double newSize = Math.Max(FONT_MIN_SIZE, currentSize - 1);
                    editor.FontSize = newSize;
                }
            }
        }
        #endregion

        #region IObserver implementation for modified document notifications
        public void OnCompleted()
        {
            
        }

        public void OnError(Exception error)
        {
            
        }

        public void OnNext(AvaloniaPropertyChangedEventArgs value)
        {
            if (_docPath == ZXConstants.DISASSEMBLY_DOC || _docPath == ZXConstants.ROM_DOC)
                return;

            if (value.NewValue != null && (bool)value.NewValue == true)
            {
                if (DocumentModified != null)
                    DocumentModified(this, EventArgs.Empty);
            }
            else if (value.NewValue != null && (bool)value.NewValue == false)
            {
                if (DocumentRestored != null)
                    DocumentRestored(this, EventArgs.Empty);
            }

        }
        #endregion
    }
}
