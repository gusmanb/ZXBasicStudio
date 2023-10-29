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
using System.Collections;
using static AvaloniaEdit.Document.TextDocumentWeakEventManager;
using ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Text;
using System.Runtime.InteropServices;
using System.Windows.Input;
using FFmpeg.AutoGen;
using System.Reflection;
using AvaloniaEdit.Search;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;

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
        protected virtual string? HelpUrl { get { return null; } }

        internal static Dictionary<string, ZXKeybCommand> keyboardCommands = new Dictionary<string, ZXKeybCommand> {
            { "Save", new ZXKeybCommand{ CommandId = Guid.Parse("87f7d73b-d28a-44f4-ba0c-41baa4de238c"), CommandName = "Save", Key = Key.S, Modifiers = KeyModifiers.Control } },
            { "Copy",new ZXKeybCommand{ CommandId = Guid.Parse("fee014bb-222b-42e3-80f3-048325b70e34"), CommandName = "Copy", Key = Key.C, Modifiers = KeyModifiers.Control } },
            { "Cut",new ZXKeybCommand{ CommandId = Guid.Parse("1edf352f-238b-421e-b69f-613dc63c0e47"), CommandName = "Cut", Key = Key.X, Modifiers = KeyModifiers.Control } },
            { "Paste", new ZXKeybCommand{ CommandId = Guid.Parse("f5d450b0-d126-4f62-885b-b3e28e638542"), CommandName = "Paste", Key = Key.V, Modifiers = KeyModifiers.Control } },
            { "Select", new ZXKeybCommand{ CommandId = Guid.Parse("dc325d2f-be66-4baa-8388-caff14ff75e7"), CommandName = "Select all", Key = Key.A, Modifiers = KeyModifiers.Control } },
            { "Undo", new ZXKeybCommand{ CommandId = Guid.Parse("912c1887-ab37-4c0a-9aee-65d84b4521c7"), CommandName = "Undo", Key = Key.Z, Modifiers = KeyModifiers.Control } },
            { "Redo", new ZXKeybCommand{ CommandId = Guid.Parse("c5c506f0-d5e4-429f-ad21-af8cee7d1d9a"), CommandName = "Redo", Key = Key.Z, Modifiers = KeyModifiers.Control | KeyModifiers.Shift } },
            { "Find", new ZXKeybCommand{ CommandId = Guid.Parse("bdb79400-bbc1-46bb-8f3d-b3d9f0f3a11b"), CommandName = "Find", Key = Key.F, Modifiers = KeyModifiers.Control } },
            { "Replace", new ZXKeybCommand{ CommandId = Guid.Parse("ee825cb5-bbdf-460f-8b28-e40482177a52"), CommandName = "Replace", Key = Key.R, Modifiers = KeyModifiers.Control } },
            { "Collapse", new ZXKeybCommand{ CommandId = Guid.Parse("22778e78-6807-43d6-9a11-70ea66a9784b"), CommandName = "Collapse all", Key = Key.E, Modifiers = KeyModifiers.Control | KeyModifiers.Shift } },
            { "Expand", new ZXKeybCommand{ CommandId = Guid.Parse("bdcfc508-19fe-4f6f-8cc5-5762bc61e93a"), CommandName = "Expand all", Key = Key.E, Modifiers = KeyModifiers.Control } },
            { "Comment", new ZXKeybCommand{ CommandId = Guid.Parse("5d7eac8e-1c73-4f30-ba4a-daafa40eaea0"), CommandName = "Comment selection", Key = Key.M, Modifiers = KeyModifiers.Control } },
            { "Uncomment", new ZXKeybCommand{ CommandId = Guid.Parse("3a14b7b9-8269-43ff-a72a-d475d7f441da"), CommandName = "Uncomment selection", Key = Key.M , Modifiers = KeyModifiers.Control | KeyModifiers.Shift } },
        };

        #endregion

        #region Private variables
        BreakPointMargin? bpMargin;
        DispatcherTimer? updateFoldingsTimer;
        bool firstRender = false;
        PausedLineBackgroundRender? blRender;
        FoldingManager? fManager;
        string _docPath;
        string _docName;
        Guid _docTypeId;

        Dictionary<Guid, Action> _keybCommands = new Dictionary<Guid, Action>();

        CompletionWindow? completionWindow;

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
        public ZXTextEditor() : this("Untitled", ZXTextDocument.Id)
        {
            
        }
        public ZXTextEditor(string DocumentPath, Guid DocumentTypeId)
        {
            InitializeComponent();

            _docTypeId = DocumentTypeId;
            InitializeShortcuts();
            
            editor.DataContext = editor;
            editor.FontSize = ZXOptions.Current.EditorFontSize;
            editor.WordWrap = ZXOptions.Current.WordWrap;

            editor.TextArea.LeftMargins.Insert(0, bpMargin);
            editor.TextArea.PointerWheelChanged += Editor_PointerWheelChanged;
            editor.TextArea.SelectionBrush = Brush.Parse("#305da9f6");
            editor.TextArea.SelectionBorder = new Pen(Brushes.White, 1);
            editor.TextArea.SelectionCornerRadius = 2;
            editor.TextArea.KeyDown += TextArea_Down;
            editor.TextArea.LayoutUpdated += TextArea_LayoutUpdated;
            editor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            editor.TextArea.SelectionChanged += TextArea_SelectionChanged;
            editor.TextArea.KeyDown += TextArea_KeyDown;
            editor.TextArea.TextEntering += TextArea_TextEntering;

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

        #region Completion

        protected virtual IEnumerable<ICompletionData>? ShouldComplete(IDocument Document, int Line, int Column, char? RequestedChar, bool ByRequest)
        {
            return null;
        }

        private void TextArea_TextEntering(object? sender, TextInputEventArgs e)
        {

            if (e.Text == null || e.Text.Length == 0)
                return;

            if (completionWindow == null && !string.IsNullOrWhiteSpace(e.Text) && !char.IsNumber(e.Text[0]))
            {
                var d = (IDocument)editor.Document;
                var completionData = ShouldComplete(editor.Document, editor.TextArea.Caret.Line, editor.TextArea.Caret.Column - 1, e.Text[0], false);

                if (completionData != null)
                    ShowCompletion(completionData, false);
            }
            else if (completionWindow != null && !char.IsLetterOrDigit(e.Text[0]))
                completionWindow.CompletionList.RequestInsertion(e);
             
        }

        private void TextArea_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && e.KeyModifiers == KeyModifiers.Control)
            {
                e.Handled = true;
                var completionData = ShouldComplete(editor.Document, editor.TextArea.Caret.Line, editor.TextArea.Caret.Column - 1, null, true);

                if (completionData != null)
                    ShowCompletion(completionData, true);
            }
        }

        private void ShowCompletion(IEnumerable<ICompletionData> completions, bool requested)
        {
            if (completionWindow == null)
            {
                completionWindow = new CompletionWindow(editor.TextArea);

                ICompletionData? selectedItem = null;

                if (requested)
                {
                    var line = editor.Document.GetLineByNumber(editor.TextArea.Caret.Line);
                    var text = editor.Document.GetText(line);

                    int offset = editor.TextArea.Caret.Offset;

                    int pos = editor.TextArea.Caret.Column - 1;

                    while (pos > 0 && char.IsLetterOrDigit(text[pos - 1]))
                    {
                        offset--;
                        pos--;
                    }

                    completionWindow.StartOffset = offset;
                    completionWindow.EndOffset = editor.TextArea.Caret.Offset;

                    string itemText = editor.Document.GetText(offset, editor.TextArea.Caret.Offset - offset).ToLower();

                    selectedItem = completions.OrderByDescending(c => c.Priority).ThenBy(c => c.Text).FirstOrDefault(c => c.Text.ToLower().StartsWith(itemText));
                }

                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                data.AddRange(completions);
                completionWindow.Show();
                completionWindow.CompletionList.SelectedItem = selectedItem;
                completionWindow.KeyDown += (s, e) =>
                {
                    if (e.Key == Key.F1 && completionWindow.CompletionList.SelectedItem != null)
                    {
                        var item = completionWindow.CompletionList.SelectedItem;
                        var topic = item.Text;
                        OpenHelp(topic);
                        e.Handled = true;
                    }
                };
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
        }

        private void OpenHelp(string Topic)
        {

            if (HelpUrl == null)
                return;

            string url = string.Format(HelpUrl, Topic);

            try
            {
                ProcessStartInfo processInfo = new()
                {
                    FileName = url,
                    UseShellExecute = true
                };

                Process.Start(processInfo);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    return;
                }
            }
        }

        #endregion

        private void TextArea_SelectionChanged(object? sender, EventArgs e)
        {
            if (editor.TextArea.Selection.Length == 0)
                txtSelection.Text = "No text selected";
            else
            {
                txtSelection.Text = $"Selected from [{editor.TextArea.Selection.StartPosition.Line},{editor.TextArea.Selection.StartPosition.Column}] to [{editor.TextArea.Selection.EndPosition.Line},{editor.TextArea.Selection.EndPosition.Column}], {editor.TextArea.Selection.Length} chars";
            }
        }

        private void Caret_PositionChanged(object? sender, EventArgs e)
        {
            txtStatus.Text = $"Line {editor.TextArea.Caret.Line}, Column {editor.TextArea.Caret.Column}";
        }

        #endregion

        #region Shortcut management

        private void InitializeShortcuts()
        {
            DisableCommand(ApplicationCommands.Cut);
            DisableCommand(ApplicationCommands.Copy);
            DisableCommand(ApplicationCommands.Paste);
            DisableCommand(ApplicationCommands.Undo);
            DisableCommand(ApplicationCommands.Redo);
            DisableCommand(ApplicationCommands.Find);
            DisableCommand(ApplicationCommands.Replace);
            DisableCommand(AvaloniaEditCommands.DeleteLine);

            _keybCommands = new Dictionary<Guid, Action>()
            {
                { keyboardCommands["Save"].CommandId, () => { RequestSaveDocument?.Invoke(this, EventArgs.Empty); } },
                { keyboardCommands["Copy"].CommandId, () => { editor.Copy(); } },
                { keyboardCommands["Cut"].CommandId, () => { editor.Cut();} },
                { keyboardCommands["Paste"].CommandId, () => { editor.Paste(); } },
                { keyboardCommands["Select"].CommandId, () => { editor.SelectAll(); } },
                { keyboardCommands["Undo"].CommandId, () => { editor.Undo(); } },
                { keyboardCommands["Redo"].CommandId, () => { editor.Redo(); } },
                { keyboardCommands["Find"].CommandId, () => { ApplicationCommands.Find.Execute(null, editor.TextArea); } },
                { keyboardCommands["Replace"].CommandId, () => { ApplicationCommands.Replace.Execute(null, editor.TextArea); } },
                { keyboardCommands["Collapse"].CommandId, () => { Collapse();  } },
                { keyboardCommands["Expand"].CommandId, () => { Expand();  } },
                { keyboardCommands["Comment"].CommandId, () => { CommentSelection(); } },
                { keyboardCommands["Uncomment"].CommandId, () => { UncommentSelection(); } }
            };
        }

        private void DisableCommand(RoutedCommand cut)
        {
            var field = typeof(RoutedCommand).GetField("<Gesture>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            KeyGesture g = new KeyGesture(Key.None, KeyModifiers.None);
            field.SetValue(cut, g);
        }

        void FindBinding(ICommand Command, ITextAreaInputHandler Handler)
        {
            if (Handler is not TextAreaInputHandler)
                return;

            var handler = (TextAreaInputHandler)Handler;

            foreach (var binding in handler.KeyBindings.ToList())
            {
                var cmd = binding.Command as RoutedCommand;

                if (cmd != null && cmd.Gesture != null && cmd.Gesture.Key == Key.D)
                    cmd = cmd;
            }

            foreach (var binding in handler.CommandBindings.ToList())
            {
                var cmd = binding.Command as RoutedCommand;

                if (cmd != null && cmd.Gesture != null && cmd.Gesture.Key == Key.D)
                    cmd = cmd;
            }

            foreach (var childHandler in handler.NestedInputHandlers)
                FindBinding(Command, childHandler);

        }

        private void TextArea_Down(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            var commandId = ZXKeybMapper.GetCommandId(_docTypeId, e.Key, e.KeyModifiers);

            if (commandId != null && _keybCommands.ContainsKey(commandId.Value))
                _keybCommands[commandId.Value]();
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
                if (!Modified)
                    return true;

                if (_docPath == ZXConstants.DISASSEMBLY_DOC || _docPath == ZXConstants.ROM_DOC)
                {
                    OutputLog.WriteLine($"This file cannot be saved.");
                    return false;
                }

                using (var str = File.Create(_docPath))
                    editor.Save(str);

                OutputLog.WriteLine($"File {_docPath} saved successfully.");

                if (DocumentSaved != null)
                    DocumentSaved(this, EventArgs.Empty);

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

        public override void Dispose()
        {
            DocumentSaved = null;
            DocumentRestored = null;
            DocumentModified = null;
            RequestSaveDocument = null;

            if (fManager != null)
                FoldingManager.Uninstall(fManager);

            fManager = null;

            if (updateFoldingsTimer != null)
            {
                updateFoldingsTimer.Stop();
                updateFoldingsTimer.Tick -= UpdateFoldingsTimer_Tick;
                updateFoldingsTimer = null;
            }

            blRender = null;

            if (bpMargin != null)
            {
                bpMargin.BreakpointAdded -= BpMargin_BreakpointAdded;
                bpMargin.BreakpointRemoved -= BpMargin_BreakpointRemoved;
                bpMargin?.Dispose();
                bpMargin = null;
            }

            editor.TextArea.PointerWheelChanged -= Editor_PointerWheelChanged;
            editor.TextArea.KeyDown -= TextArea_Down;
            editor.TextArea.LayoutUpdated -= TextArea_LayoutUpdated;

            if (allowsBreakpoints)
            {
                editor.Document.Changing -= Document_Changing;
                editor.Document.Changed -= Document_Changed;
            }

            editor.TemplateApplied -= Editor_TemplateApplied;
        }

        #endregion

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
        
        public void FontIncrease()
        {
            editor.FontSize++;
        }
        
        public void FontDecrease()
        {
            editor.FontSize--;
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
                updateFoldingsTimer.Tick -= UpdateFoldingsTimer_Tick;
                updateFoldingsTimer = null;
            }

            if (!string.IsNullOrWhiteSpace(OldName) && OldName != "Untitled")
                BreakpointManager.UpdateFileName(OldName, NewName);

            if (editor.Document != null && allowsBreakpoints)
            {
                editor.Document.Changing -= Document_Changing;
                editor.Document.Changed -= Document_Changed;
            }
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

            if (allowsBreakpoints)
            {
                editor.Document.Changing += Document_Changing;
                editor.Document.Changed += Document_Changed;
            }
        }
        private void Document_Changing(object? sender, DocumentChangeEventArgs e)
        {

            if (!allowsBreakpoints || e.OffsetChangeMap == null || e.OffsetChangeMap.Count == 0)
                return;

            // TODO: DUEFECTU 2023.05.17 - if e.OffsetChangeMap.Count == 0, e.OffsetChangeMap[0].Offset throws a null reference exception
            if (e.OffsetChangeMap.Count == 0)
            {
                return;
            }

            int start = e.OffsetChangeMap[0].Offset;
            int firstLine = editor.Document.GetLineByOffset(start).LineNumber;
            bool changed = false;

            if (e.RemovalLength > 0)
            {
                
                int end = start + e.RemovalLength;
                int linesRemoved = 0;
                int pos = start;
                while(pos < end) 
                {
                    var line = editor.Document.GetLineByOffset(pos);
                    if (pos >= line.EndOffset)
                        firstLine++;

                    
                    string lineText = editor.Document.GetText(line.Offset, line.Length);
                    int removalLength = Math.Min(line.EndOffset - pos, end - pos);
                    string leftText = lineText.Remove(pos - line.Offset, removalLength);


                    if (string.IsNullOrWhiteSpace(leftText))
                        changed |= RemoveBreakpoint(line.LineNumber);

                    pos = line.NextLine?.Offset ?? int.MaxValue;

                    if (pos <= end)
                        linesRemoved++;
                }

                if(linesRemoved > 0)
                    changed |= MoveBreakpoints(firstLine, -linesRemoved);
            }

            if(changed)
                bpMargin?.InvalidateVisual();
        }
        private void Document_Changed(object? sender, DocumentChangeEventArgs e)
        {
            if (!allowsBreakpoints || e.OffsetChangeMap == null || e.OffsetChangeMap.Count == 0)
                return;

            // TODO: DUEFECTU 2023.05.17 - if e.OffsetChangeMap.Count == 0, e.OffsetChangeMap[0].Offset throws a null reference exception
            if (e.OffsetChangeMap.Count == 0)
            {
                return;
            }

            int start = e.OffsetChangeMap[0].Offset;
            var firstLine = editor.Document.GetLineByOffset(start);
            bool changed = false;

            if (e.InsertionLength > 0 && e.InsertedText.Text.Contains("\n"))
            {
                int addedLines = e.InsertedText.Text.Count(c => c == '\n');

                var firstText = editor.Document.GetText(firstLine.Offset, firstLine.Length);

                if(!string.IsNullOrWhiteSpace(firstText))
                    changed = MoveBreakpoints(firstLine.LineNumber + 1, addedLines);
                else
                    changed = MoveBreakpoints(firstLine.LineNumber, addedLines);
            }
        }
        bool RemoveBreakpoint(int LineNumber)
        {
            if (bpMargin == null)
                return false;

            var bp = bpMargin.Breakpoints.Where(b => b.Line == LineNumber).FirstOrDefault();
            if (bp != null)
            {
                bpMargin.Breakpoints.Remove(bp);
                BreakpointManager.RemoveBreakpoint(_docPath, bp);
                return true;
            }

            return false;
        }
        bool MoveBreakpoints(int FirstLine, int LineOffset)
        {
            if (bpMargin == null)
                return false;

            var toMove = bpMargin.Breakpoints.Where(b => b.Line >= FirstLine).ToArray();

            if (toMove.Length > 0)
            {
                foreach (var b in toMove)
                    b.Line += LineOffset;

                return true;
            }

            return false;
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

        public override void Activated()
        {
            editor.TextArea.Focus();
        }

    }
}
