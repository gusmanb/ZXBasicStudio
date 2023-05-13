using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using Svg;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZXBasicStudio.Controls;
using ZXBasicStudio.DocumentEditors.ZXTapeBuilder.Classes;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.Extensions;

namespace ZXBasicStudio.DocumentEditors.ZXTapeBuilder.Controls
{
    public partial class ZXTapeBuilderEditor : ZXDocumentEditorBase
    {
        static readonly ZXColorItem[] _colors = new ZXColorItem[] 
        {
            new ZXColorItem{ ColorName = "Black", ColorBrush = SolidColorBrush.Parse("#000000"), ColorValue = 0 },
            new ZXColorItem{ ColorName = "Blue", ColorBrush = SolidColorBrush.Parse("#0000D7"), ColorValue = 1 },
            new ZXColorItem{ ColorName = "Red", ColorBrush = SolidColorBrush.Parse("#D70000"), ColorValue = 2 },
            new ZXColorItem{ ColorName = "Purple", ColorBrush = SolidColorBrush.Parse("#D700D7"), ColorValue = 3 },
            new ZXColorItem{ ColorName = "Green", ColorBrush = SolidColorBrush.Parse("#00D700"), ColorValue = 4 },
            new ZXColorItem{ ColorName = "Cyan", ColorBrush = SolidColorBrush.Parse("#00D7D7"), ColorValue = 5 },
            new ZXColorItem{ ColorName = "Yellow", ColorBrush = SolidColorBrush.Parse("#D7D700"), ColorValue = 6 },
            new ZXColorItem{ ColorName = "White", ColorBrush = SolidColorBrush.Parse("#D7D7D7"), ColorValue = 7 },
        };

        #region Private variables
        string _docName;
        string _docPath;
        bool _modified;
        bool _internalUpdate;
        ObservableCollection<ZXTapeBuilderDataBlock> _blocks = new ObservableCollection<ZXTapeBuilderDataBlock>();
        #endregion

        #region Events
        public override event EventHandler? DocumentModified;
        public override event EventHandler? DocumentRestored;
        public override event EventHandler? DocumentSaved;
        public override event EventHandler? RequestSaveDocument;
        #endregion

        #region Binding properties
        public ZXColorItem[] Colors => _colors;
        public ObservableCollection<ZXTapeBuilderDataBlock> Blocks => _blocks;
        #endregion

        #region ZXDocumentBase properties
        public override string DocumentName => _docName;
        public override string DocumentPath => _docPath;
        public override bool Modified => _modified;
        #endregion

        public ZXTapeBuilderEditor()
        {
            DataContext = this;
            InitializeComponent();
        }

        public ZXTapeBuilderEditor(string DocumentPath)
        {
            DataContext = this;
            InitializeComponent();

            txtLoaderName.TextChanged += DocumentChanged;
            ckInk.IsCheckedChanged+= DocumentChanged;
            cbInk.SelectionChanged+= DocumentChanged;
            ckPaper.IsCheckedChanged+= DocumentChanged;
            cbPaper.SelectionChanged+= DocumentChanged;
            ckBorder.IsCheckedChanged+= DocumentChanged;
            cbBorder.SelectionChanged+= DocumentChanged;
            txtPokesBefore.TextChanged += DocumentChanged;
            txtPokesAfter.TextChanged += DocumentChanged;
            txtScreenName.TextChanged += DocumentChanged;   
            txtScreenFile.TextChanged += DocumentChanged;

            btnSelectScreen.Click += BtnSelectScreen_Click;
            btnSelectBlock.Click += BtnSelectBlock_Click;
            btnAddBlock.Click += BtnAddBlock_Click;
            btnMoveBlockUp.Click += BtnMoveBlockUp_Click;
            btnMoveBlockDown.Click += BtnMoveBlockDown_Click;
            btnRemoveBlock.Click += BtnRemoveBlock_Click;
            btnSave.Click += BtnSave_Click;
            btnDiscard.Click += BtnDiscard_Click;
            _docPath = DocumentPath;
            _docName = Path.GetFileName(DocumentPath);
            if (!UpdateFileName(DocumentPath))
                throw new Exception("Error opening document");
        }

        #region Event handlers
        private async void BtnDiscard_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!_modified)
                return;

            if (!await Window.GetTopLevel(this).ShowConfirm("Discard changes", "Are you sure you want to discard all changes?"))
                return;

            UpdateFileName(_docPath);

            _modified = false;

            if(DocumentRestored != null)
                DocumentRestored(this, EventArgs.Empty);
        }

        private void BtnSave_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (RequestSaveDocument != null)
                RequestSaveDocument(this, EventArgs.Empty);
        }

        private void BtnRemoveBlock_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (lstBlocks.SelectedItem != null)
            {
                _blocks.Remove((ZXTapeBuilderDataBlock)lstBlocks.SelectedItem);
                DocumentChanged(this, e);
            }
        }

        private void BtnMoveBlockDown_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (lstBlocks.SelectedItem != null && lstBlocks.SelectedIndex < _blocks.Count - 1)
            {
                _blocks.Move(lstBlocks.SelectedIndex, lstBlocks.SelectedIndex + 1);
                DocumentChanged(this, e);
            }
        }

        private void BtnMoveBlockUp_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (lstBlocks.SelectedItem != null && lstBlocks.SelectedIndex > 0)
            {
                _blocks.Move(lstBlocks.SelectedIndex, lstBlocks.SelectedIndex - 1);
                DocumentChanged(this, e);
            }
        }

        private async void BtnAddBlock_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBlockFile.Text))
            {
                await Window.GetTopLevel(this).ShowError("Missing file", "No file selected for the data block.");
                return;
            }

            if (!File.Exists(txtBlockFile.Text))
            {
                await Window.GetTopLevel(this).ShowError("File not found", "Cannot find the specified file for the block.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBlockName.Text))
            {
                await Window.GetTopLevel(this).ShowError("Missing name", "A name must be specified for the data block.");
                return;
            }

            ZXTapeBuilderDataBlock block = new ZXTapeBuilderDataBlock { BlockFile = txtBlockFile.Text, BlockName = txtBlockName.Text, BlockAddress = (ushort)nudBlockAddress.Value, BasicLoad = ckBasicLoad.IsChecked ?? false };

            _blocks.Add(block);

            DocumentChanged(this, e);
        }

        private async void BtnSelectBlock_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var storage = Window.GetTopLevel(this)?.StorageProvider;

            if (storage == null)
                return;

            var typeTap = new FilePickerFileType("Tape file") { Patterns = new string[] { "*.tap" } };

            var typeBin = new FilePickerFileType("Binary file") { Patterns = new string[] { "*.bin" } };

            var typeAll = new FilePickerFileType("All files") { Patterns = new string[] { "*.*" } };

            var options = new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Select block file",
                FileTypeFilter = new FilePickerFileType[] { typeBin, typeTap, typeAll }
            };

            var res = await storage.OpenFilePickerAsync(options);

            if (res == null || res.Count < 1)
                return;

            var file = res[0].TryGetLocalPath();

            if (string.IsNullOrWhiteSpace(file))
                return;

            txtBlockFile.Text = file;
        }

        private async void BtnSelectScreen_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var storage = Window.GetTopLevel(this)?.StorageProvider;

            if (storage == null)
                return;

            var typeScr = new FilePickerFileType("Screen file") { Patterns = new string[] { "*.scr" } };

            var typeTap = new FilePickerFileType("Tape file") { Patterns = new string[] { "*.tap" } };

            var typeBin = new FilePickerFileType("Binary file") { Patterns = new string[] { "*.bin" } };

            var typeAll = new FilePickerFileType("All files") { Patterns = new string[] { "*.*" } };

            var options = new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Select screen file",
                FileTypeFilter = new FilePickerFileType[] { typeScr, typeBin, typeTap, typeAll }
            };

            var res = await storage.OpenFilePickerAsync(options);

            if (res == null || res.Count < 1)
                return;

            var file = res[0].TryGetLocalPath();

            if (string.IsNullOrWhiteSpace(file))
                return;

            txtScreenFile.Text = file;
        }

        private void DocumentChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_internalUpdate)
                return;

            if (!_modified)
            {
                _modified = true;
                if(DocumentModified != null)
                    DocumentModified(this, e);
            }
        }
        #endregion

        bool UpdateFileName(string NewPath)
        {
            _docPath = NewPath;
            _docName = Path.GetFileName(NewPath);
            _modified = false;
            
            string content = File.ReadAllText(_docPath);
            ZXTapeBuilderFile? fileContent = JsonConvert.DeserializeObject<ZXTapeBuilderFile>(content);

            if (fileContent == null)
                return false;

            _internalUpdate = true;

            //Basic loader
            txtLoaderName.Text = fileContent.ProgramName;
            ckInk.IsChecked = fileContent.UseInk;
            cbInk.SelectedIndex = fileContent.Ink;
            ckPaper.IsChecked = fileContent.UsePaper;
            cbPaper.SelectedIndex = fileContent.Paper;
            ckBorder.IsChecked = fileContent.UseBorder;
            cbBorder.SelectedIndex = fileContent.Border;

            if (fileContent.PokesBeforeLoad != null)
            {
                txtPokesBefore.Text = string.Join(';', fileContent.PokesBeforeLoad.Select(p => p.Address.ToString() + "," + p.Value.ToString()));
            }
            else
                txtPokesBefore.Text = "";

            if (fileContent.PokesAfterLoad != null)
            {
                txtPokesAfter.Text = string.Join(';', fileContent.PokesAfterLoad.Select(p => p.Address.ToString() + "," + p.Value.ToString()));
            }
            else
                txtPokesAfter.Text = "";

            //Load screen
            txtScreenName.Text = fileContent.ScreenName;
            txtScreenFile.Text = fileContent.ScreenFile;

            //Blocks
            _blocks.Clear();
            if (fileContent.DataBlocks != null)
            {
                foreach(var block in fileContent.DataBlocks) 
                    _blocks.Add(block);
            }

            Task.Run(async () => 
            {
                await Task.Delay(100);
                _internalUpdate = false;
            });

            return true;
        }

        ZXTapeBuilderPoke[]? ParsePokeString(string PokeString)
        {
            List<ZXTapeBuilderPoke> pokesList = new List<ZXTapeBuilderPoke>();

            string[] pokeList = PokeString.Split(';');

            foreach (var poke in pokeList)
            {
                string[] pokeValues = poke.Split(",");

                if (pokeValues.Length != 2)
                    return null;

                ushort addr;
                byte val;

                if (!ushort.TryParse(pokeValues[0], out addr))
                    return null;

                if (!byte.TryParse(pokeValues[1], out val))
                    return null;

                pokesList.Add(new ZXTapeBuilderPoke { Address = addr, Value = val });
            }

            if(pokesList.Count == 0)
                return null;

            return pokesList.ToArray();
        }

        #region ZXDocumentBase implementation
        public override bool SaveDocument(TextWriter OutputLog)
        {
            ZXTapeBuilderPoke[]? beforePokes = null;
            ZXTapeBuilderPoke[]? afterPokes = null;

            if (string.IsNullOrWhiteSpace(txtLoaderName.Text))
            {
                OutputLog.WriteLine("Missing loader name, aborting...");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtPokesBefore.Text))
            {
                beforePokes = ParsePokeString(txtPokesBefore.Text);

                if (beforePokes == null)
                {
                    OutputLog.WriteLine("Poke string malformed, aborting...");
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(txtPokesAfter.Text))
            {
                afterPokes = ParsePokeString(txtPokesAfter.Text);

                if (afterPokes == null)
                {
                    OutputLog.WriteLine("Poke string malformed, aborting...");
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(txtScreenName.Text) && string.IsNullOrWhiteSpace(txtScreenFile.Text))
            {
                OutputLog.WriteLine("Screen name specified but no screen file selected, aborting...");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtScreenName.Text) && !string.IsNullOrWhiteSpace(txtScreenFile.Text))
            {
                OutputLog.WriteLine("Screen file specified but no screen name selected, aborting...");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtScreenFile.Text) && !File.Exists(txtScreenFile.Text))
            {
                OutputLog.WriteLine("Cannot find specified screen file, aborting...");
                return false;
            }

            ZXTapeBuilderFile fileContent = new ZXTapeBuilderFile
            {
                ProgramName = txtLoaderName.Text,
                UseInk = ckInk.IsChecked ?? false,
                Ink = cbInk.SelectedIndex,
                UsePaper = ckPaper.IsChecked ?? false,
                Paper = cbPaper.SelectedIndex,
                UseBorder = ckBorder.IsChecked ?? false,
                Border = cbBorder.SelectedIndex,
                PokesBeforeLoad = beforePokes,
                PokesAfterLoad = afterPokes,
                ScreenFile = txtScreenFile.Text,
                ScreenName = txtScreenName.Text,
                DataBlocks = _blocks.ToArray(),
            };

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

        #region Support classes
        public class ZXColorItem
        {
            public required string ColorName { get; set; }
            public required IBrush ColorBrush { get; set; }
            public required int ColorValue { get; set; }
        }
        #endregion
    }
    
}
