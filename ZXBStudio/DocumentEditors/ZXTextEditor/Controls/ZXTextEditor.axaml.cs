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

namespace ZXBasicStudio.DocumentEditors.ZXTextEditor.Controls
{
    public partial class ZXTextEditor : UserControl
    {
        BreakPointMargin? bpMargin;
        protected virtual LanguageDefinitionBase? langDef { get { return null; } }
        protected virtual IBrush? SearchMarkerBrush { get { return null; } }
        protected virtual AbstractFoldingStrategy? FoldingStrategy { get { return null; } }

        protected virtual Regex? RegCancelBreakpoint { get { return null; } }
        protected virtual char? CommentChar { get { return null; } }

        DispatcherTimer? updateFoldingsTimer;
        protected FoldingManager? fManager;

        const int FONT_MAX_SIZE = 64;
        const int FONT_MIN_SIZE = 7;

        public event EventHandler? DocumentModified;
        public event EventHandler? DocumentSaved;

        PausedLineBackgroundRender? blRender;
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

        public bool Modified { get; set; }
        
        public bool Readonly { get { return editor.IsReadOnly; } set { editor.IsReadOnly = value; } }

        string _fileName;
        public string FileName { get { return _fileName; } set { UpdateFileName(_fileName, value); _fileName = value; } }
        
        public string? Text { get { return editor.Document?.Text; } set { if(editor.Document != null) editor.Document.Text = value; } }

        public new double FontSize { get { return editor.FontSize; } set { editor.FontSize = value; } }

        public ZXTextEditor() : this("Untitled")
        { 
        }
        public ZXTextEditor(string FileName)
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
                bpMargin = new BreakPointMargin(editor);
                bpMargin.BreakpointAdded += BpMargin_BreakpointAdded;
                bpMargin.BreakpointRemoved += BpMargin_BreakpointRemoved;
                blRender = new PausedLineBackgroundRender();
                editor.TextArea.TextView.BackgroundRenderers.Add(blRender);
                editor.TextArea.LeftMargins[0] = bpMargin;
            }
            _fileName = FileName;
            UpdateFileName(null, _fileName);
            editor.TextChanged += Editor_TextChanged;
            editor.TemplateApplied += Editor_TemplateApplied;

            editor.Options.ConvertTabsToSpaces = true;
            editor.Options.EnableTextDragDrop = true;
            editor.Options.EnableRectangularSelection = true;

            var breaks = BreakpointManager.BreakPoints(FileName);

            foreach (var bp in breaks)
                bpMargin?.Breakpoints.Add(bp);
        }

        public void FocusText()
        {
            editor.TextArea.Focus();
            editor.TextArea.Caret.BringCaretToView();
        }

        private void Editor_TemplateApplied(object? sender, Avalonia.Controls.Primitives.TemplateAppliedEventArgs e)
        {
            if(SearchMarkerBrush != null)
                editor.SearchPanel.SetSearchResultsBrush(SearchMarkerBrush);
        }

        bool firstRender = false;

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
            if (e.Key == Avalonia.Input.Key.S && e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Control))
                SaveDocument();
        }
        private void Editor_TextChanged(object? sender, EventArgs e)
        {
            if (FileName == ZXConstants.DISASSEMBLY_DOC || FileName == ZXConstants.ROM_DOC)
                return;

            if (!Modified)
            {
                Modified = true;
                if(DocumentModified != null)
                    DocumentModified(this, EventArgs.Empty);
            }

            if (updateFoldingsTimer != null)
            {
                updateFoldingsTimer.IsEnabled = false;
                updateFoldingsTimer.IsEnabled = true;
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

            if (FoldingStrategy != null)
            {
                fManager = FoldingManager.Install(editor.TextArea);
                updateFoldingsTimer = new DispatcherTimer();
                updateFoldingsTimer.Interval = TimeSpan.FromSeconds(2);
                updateFoldingsTimer.Tick += UpdateFoldingsTimer_Tick;
                updateFoldingsTimer.IsEnabled = true;
            }
        }
        private void UpdateFoldingsTimer_Tick(object? sender, EventArgs e)
        {
            if (updateFoldingsTimer == null || fManager == null || FoldingStrategy == null)
                return;

            updateFoldingsTimer.IsEnabled = false;
            FoldingStrategy.UpdateFoldings(fManager, editor.Document);
        }
        private void Editor_PointerWheelChanged(object? sender, Avalonia.Input.PointerWheelEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Control))
            {
                UpdateFontSize(e.Delta.Y > 0);
                e.Handled = true;
            }

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
        public bool SaveDocument()
        {

            if (FileName == ZXConstants.DISASSEMBLY_DOC || FileName == ZXConstants.ROM_DOC)
                return false;

            try
            {
                File.WriteAllText(_fileName, editor.Document.Text);
            }
            catch { return false; }

            Modified = false;
            
            if(DocumentSaved != null)
                DocumentSaved(this, EventArgs.Empty);

            return true;
        }
        private void BpMargin_BreakpointRemoved(object? sender, BreakPointMargin.BreakpointEventArgs e)
        {
            if (FileName == "Untitled")
            {
                e.Cancel = true;
                return;
            }

            BreakpointManager.RemoveBreakpoint(_fileName, e.BreakPoint);
        }
        private void BpMargin_BreakpointAdded(object? sender, BreakPointMargin.BreakpointEventArgs e)
        {
            if (FileName == "Untitled")
            {
                e.Cancel = true;
                return;
            }

            var line = editor.Document.Lines[e.BreakPoint.Line - 1];
            var text = editor.Document.GetText(line);

            if (string.IsNullOrWhiteSpace(text))
                e.Cancel = true;

            if (RegCancelBreakpoint != null && RegCancelBreakpoint.IsMatch(text))
                e.Cancel = true;

            e.BreakPoint.File = _fileName;
            BreakpointManager.AddBreakpoint(e.BreakPoint);
            
        }
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
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
            if (editor.IsReadOnly || CommentChar == null || editor.TextArea.Selection == null)
                return;
            
            TextDocument document = editor.TextArea.Document;

            if (editor.TextArea.Selection.Length == 0)
            {
                var caretPos = editor.TextArea.Caret.Position;
                var line = document.GetLineByNumber(caretPos.Line);
                document.Insert(line.Offset, $"{CommentChar}");
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
                        document.Insert(document.GetLineByNumber(i).Offset, $"{CommentChar}");
                    }
                }
            }
        }
        public void UncommentSelection()
        {
            if (editor.IsReadOnly || CommentChar == null || editor.TextArea.Selection == null)
                return;

            TextDocument document = editor.TextArea.Document;
            Regex regComment = new Regex($"^(\\s*?)({CommentChar})");

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
        public void ClearBreakpoints()
        {
            bpMargin?.Breakpoints.Clear();
            InvalidateVisual();
            bpMargin?.InvalidateVisual();
        }
    }
}
