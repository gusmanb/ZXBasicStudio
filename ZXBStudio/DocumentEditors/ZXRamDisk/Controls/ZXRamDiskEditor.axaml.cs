using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZXBasicStudio.Classes;
using ZXBasicStudio.DebuggingTools.Registers.Binding;
using ZXBasicStudio.DocumentEditors.ZXRamDisk.Classes;
using ZXBasicStudio.DocumentEditors.ZXTapeBuilder.Classes;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.Extensions;
using static ZXBasicStudio.DocumentEditors.ZXTapeBuilder.Controls.ZXTapeBuilderEditor;

namespace ZXBasicStudio.DocumentEditors.ZXRamDisk.Controls
{
    public partial class ZXRamDiskEditor : ZXDocumentEditorBase
    {
        public static StyledProperty<ObservableCollection<ZXRamDiskContainedFile>> FilesProperty = AvaloniaProperty.Register<ZXRamDiskEditor, ObservableCollection<ZXRamDiskContainedFile>>("Files");

        #region Private variables
        string _docName;
        string _docPath;
        bool _modified;
        bool _internalUpdate;
        ObservableCollection<ZXRamDiskContainedFile>[] _files = new ObservableCollection<ZXRamDiskContainedFile>[5];
        #endregion

        #region Events
        public override event EventHandler? DocumentModified;
        public override event EventHandler? DocumentRestored;
        public override event EventHandler? DocumentSaved;
        public override event EventHandler? RequestSaveDocument;
        #endregion

        #region Binding properties
        public ObservableCollection<ZXRamDiskContainedFile> Files 
        {
            get { return GetValue(FilesProperty); }
            set { SetValue(FilesProperty, value); }
        }
        #endregion

        #region ZXDocumentBase properties
        public override string DocumentName => _docName;
        public override string DocumentPath => _docPath;
        public override bool Modified => _modified;
        #endregion
        public ZXRamDiskEditor()
        {
            for (int buc = 0; buc < 5; buc++)
                _files[buc] = new ObservableCollection<ZXRamDiskContainedFile>();

            //Order: 4, 6, 1, 3, 7
            Files = _files[0];

            DataContext = this;
            InitializeComponent();
        }

        public ZXRamDiskEditor(string DocumentPath)
        {
            for (int buc = 0; buc < 5; buc++)
                _files[buc] = new ObservableCollection<ZXRamDiskContainedFile>();

            //Order: 4, 6, 1, 3, 7
            Files = _files[0];

            DataContext = this;
            InitializeComponent();

            ckIndirect.IsCheckedChanged += DocumentChanged;
            ckRelocate.IsCheckedChanged += DocumentChanged;
            nudIndSize.ValueChanged += DocumentChanged;
            cbBank.SelectionChanged += ChangeSelectedBank;

            btnSelectFile.Click += BtnSelectFile_Click;
            btnAddFile.Click += BtnAddFile_Click;
            btnRemoveFile.Click += BtnRemoveFile_Click;
            btnSave.Click += BtnSave_Click;
            btnDiscard.Click += BtnDiscard_Click;

            _docPath = DocumentPath;
            _docName = Path.GetFileName(DocumentPath);
            if (!UpdateFileName(DocumentPath))
                throw new Exception("Error opening document");
        }

        private void ChangeSelectedBank(object? sender, SelectionChangedEventArgs e)
        {
            int bank = cbBank.SelectedIndex;
            Files = _files[bank];
        }

        private async void BtnAddFile_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtFilePath.Text))
            {
                await Window.GetTopLevel(this).ShowError("Missing file", "No file selected.");
                return;
            }

            string path = Path.Combine(ZXProjectManager.Current.ProjectPath, txtFilePath.Text);

            if (!File.Exists(path))
            {
                await Window.GetTopLevel(this).ShowError("File not found", "Cannot find the specified file.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFileName.Text))
            {
                await Window.GetTopLevel(this).ShowError("Missing name", "A name must be specified for the file.");
                return;
            }

            ZXRamDiskContainedFile file = new ZXRamDiskContainedFile { Name = txtFileName.Text, SourcePath = txtFilePath.Text };

            Files.Add(file);

            DocumentChanged(this, e);
        }

        private async void BtnSelectFile_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var storage = Window.GetTopLevel(this)?.StorageProvider;

            if (storage == null)
                return;

            var typeAll = new FilePickerFileType("All files") { Patterns = new string[] { "*" } };

            var options = new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Select file",
                FileTypeFilter = new FilePickerFileType[] { typeAll }
            };

            var res = await storage.OpenFilePickerAsync(options);

            if (res == null || res.Count < 1)
                return;

            var file = res[0].TryGetLocalPath();

            if (string.IsNullOrWhiteSpace(file))
                return;

            file = Path.GetRelativePath(ZXProjectManager.Current.ProjectPath, file);

            txtFilePath.Text = file;

            string fileName = Path.GetFileNameWithoutExtension(txtFilePath.Text);
            fileName = Regex.Replace(fileName, "[^\\w\\d]", "_");
            txtFileName.Text = fileName;
        }

        private void BtnRemoveFile_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (lstFiles.SelectedItem != null)
            {
                Files.Remove((ZXRamDiskContainedFile)lstFiles.SelectedItem);
                DocumentChanged(this, e);
            }
        }
        private void BtnSave_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (RequestSaveDocument != null)
                RequestSaveDocument(this, EventArgs.Empty);
        }
        private async void BtnDiscard_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!_modified)
                return;

            if (!await Window.GetTopLevel(this).ShowConfirm("Discard changes", "Are you sure you want to discard all changes?"))
                return;

            UpdateFileName(_docPath);

            _modified = false;

            if (DocumentRestored != null)
                DocumentRestored(this, EventArgs.Empty);
        }
        private void DocumentChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_internalUpdate)
                return;

            if (!_modified)
            {
                _modified = true;
                if (DocumentModified != null)
                    DocumentModified(this, e);
            }
        }

        bool UpdateFileName(string NewPath)
        {
            _docPath = NewPath;
            _docName = Path.GetFileName(NewPath);
            _modified = false;

            string content = File.ReadAllText(_docPath);
            ZXRamDiskFile? fileContent = JsonConvert.DeserializeObject<ZXRamDiskFile>(content);

            if (fileContent == null)
                return false;

            _internalUpdate = true;

            ckIndirect.IsChecked = fileContent.EnableIndirect;
            ckRelocate.IsChecked = fileContent.RelocateStack;
            nudIndSize.Value = fileContent.IndirectBufferSize;

            for (int buc = 0; buc < 5; buc++)
            {
                _files[buc].Clear();
                _files[buc].AddRange(fileContent.Banks[buc].Files);
            }

            cbBank.SelectedIndex = 0;

            Task.Run(async () =>
            {
                await Task.Delay(100);
                _internalUpdate = false;
            });

            return true;
        }

        #region ZXDocumentBase implementation
        public override bool SaveDocument(TextWriter OutputLog)
        {
            if(_files.Sum(f => f.Count) == 0) 
            {
                OutputLog.WriteLine("No files on disk, aborting...");
                return false;
            }

            if (_files.Any(f => f.Sum(ff => ff.Size) > 16 * 1024))
            {
                OutputLog.WriteLine("Bank size exceeds 16Kb, aborting...");
                return false;
            }

            ZXRamDiskFile fileContent = new ZXRamDiskFile();

            for (int buc = 0; buc < 5; buc++)
                fileContent.Banks[buc].Files.AddRange(_files[buc]);

            fileContent.IndirectBufferSize = (int)(nudIndSize.Value ?? 0);
            fileContent.EnableIndirect = ckIndirect.IsChecked ?? false;
            fileContent.RelocateStack = ckRelocate.IsChecked ?? false;

            string content = JsonConvert.SerializeObject(fileContent, Formatting.Indented);
            try
            {
                File.WriteAllText(_docPath, content);

                if (DocumentSaved != null)
                    DocumentSaved(this, EventArgs.Empty);

                _modified = false;

                return true;
            }
            catch (Exception ex)
            {
                OutputLog.WriteLine($"Unexpected error saving file: {ex.Message}");
                return false;
            }
        }

        public override bool RenameDocument(string NewName, TextWriter OutputLog)
        {
            if (!UpdateFileName(NewName))
            {
                OutputLog.WriteLine("Error reading new file.");
                return false;
            }

            return true;
        }

        public override bool CloseDocument(TextWriter OutputLog, bool ForceClose)
        {
            return true;
        }

        public override void Dispose()
        {
            //Nothing to do, no allocated resources
        }
        #endregion

    }
}
