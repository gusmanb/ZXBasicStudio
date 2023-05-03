using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CoreSpectrum.Hardware;
using CoreSpectrum.SupportClasses;
using MessageBox.Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.Classes;

namespace ZXBasicStudio.Controls
{
    public partial class ZXTapePlayer : UserControl
    {
        TapePlayer? _dataCorder;
        public TapePlayer? Datacorder 
        {
            get { return _dataCorder; } 
            set { _dataCorder = value; _tapeName = null; _blockIndex = -1; UpdateUI(this, EventArgs.Empty); } 
        }

        DispatcherTimer? _tmrUpdate;
        string? _tapeName = null;
        int _blockIndex = -1;
        List<ListBoxItem> _blocks = new List<ListBoxItem>();
        public ZXTapePlayer()
        {
            InitializeComponent();
            btnEject.Click += BtnEject_Click;
            btnPlay.Click += BtnPlay_Click;
            btnPause.Click += BtnPause_Click;
            btnStop.Click += BtnStop_Click;
            btnFfw.Click += BtnFfw_Click;
            btnRew.Click += BtnRew_Click;
        }

        private void BtnRew_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_dataCorder == null)
                return;

            if (_dataCorder.LoadedTape == null)
                return;

            int idx = _dataCorder.Block;

            if (idx == -1)
                return;

            idx--;

            if (idx > -1)
                _dataCorder.FfwRew(idx);
        }

        private void BtnFfw_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_dataCorder == null)
                return;

            if (_dataCorder.LoadedTape == null)
                return;

            int idx = _dataCorder.Block;

            if (idx == -1)
                return;

            idx++;

            if (idx <= _dataCorder.LoadedTape.Blocks.Length)
                _dataCorder.FfwRew(idx);
        }
        private void BtnPause_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_dataCorder == null)
                return;

            if (_dataCorder.LoadedTape == null)
                return;

            if (_dataCorder.Paused)
                _dataCorder.Resume();
            else
                _dataCorder.Pause();
        }
        private void BtnStop_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _dataCorder?.Stop();
            UpdateUI(null, EventArgs.Empty);
        }

        private void BtnPlay_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_dataCorder == null || _dataCorder.Playing || _dataCorder.Paused)
                return;

            _dataCorder.Play();
        }

        private async void BtnEject_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_dataCorder == null)
                return;

            if (_dataCorder.LoadedTape != null)
            {
                _dataCorder.EjectTape();
                _tapeName = null;
                _blocks.Clear();
                lstBlocks.Items?.Clear();
                txtTape.Text = "Insert tape...";

                return;
            }

            var provider = Window.GetTopLevel(this)?.StorageProvider;

            if (provider == null)
                return;

            var select = await provider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Select tape...",
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Tape files") { Patterns = ZXExtensions.ZXTapeFiles.Select(t => "*" + t).ToArray() },
                }
            });

            string? file = null;

            if (select != null && select.Count > 0)
                file = Path.GetFullPath(select[0].Path.LocalPath);

            if (file == null)
                return;

            Tape? tape = null;

            switch (Path.GetExtension(file).ToLower())
            {
                case ".tap":
                    tape = TAPFile.Load(file);
                    break;
                case ".tzx":
                    tape = TZXFile.Load(file);
                    break;
                default:
                    await ShowError("Unknown format", "Unknown tape file format.");
                    return;
            }

            if (tape == null)
            {
                await ShowError("Error", "Error loading the specified tape file.");
                return;
            }

            _dataCorder.InsertTape(tape);

            _tapeName = Path.GetFileNameWithoutExtension(file);

            foreach (var block in tape.Blocks)
            {
                var item = new ListBoxItem();

                if (block.Block.BlockType == BlockType.Header)
                    item.Content = $"Header: {block.Block.Header.Type.ToString()}, {Encoding.ASCII.GetString(block.Block.Header.Name)}";
                else
                    item.Content = $"Data: {block.Block.Data.Length - 2} bytes";

                _blocks.Add(item);
                lstBlocks.Items.Add(item);
            }

            UpdateUI(null, EventArgs.Empty);
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);

            if (_tmrUpdate != null)
                _tmrUpdate.Stop();

            _tmrUpdate = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, UpdateUI);
            _tmrUpdate.Start();
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);

            if (_tmrUpdate != null)
            {
                _tmrUpdate.Stop();
                _tmrUpdate = null;
            }
        }

        void UpdateUI(object? sender, EventArgs e)
        {
            if (Datacorder == null || Datacorder.LoadedTape == null)
            {
                if (lstBlocks.Items != null && lstBlocks.Items.Count > 0)
                    lstBlocks.Items.Clear();
                _blocks.Clear();
                txtTape.Text = "Insert tape...";
                txtState.Text = "Stopped.";
                txtTime.Text = "--:-- / --:--";
                _tapeName = null;
                return;
            }

            txtState.Text = Datacorder.Playing ? "Playing..." : Datacorder.Paused ? "Paused..." : "Stopped.";
            txtTape.Text = _tapeName ?? "Insert tape...";
            string len = Datacorder.TapeLength?.ToString("mm\\:ss") ?? "--:--";
            string pos = Datacorder.Position?.ToString("mm\\:ss") ?? "--:--";
            txtTime.Text = $"{pos} / {len}";
            int bIdx = Datacorder.Block;
            if (bIdx != _blockIndex && bIdx < (lstBlocks.Items?.Count ?? 0))
            {
                if(_blockIndex != -1)
                    _blocks[_blockIndex].Content = $"{GetBlockText(Datacorder.LoadedTape.Blocks[_blockIndex].Block)}";

                if(bIdx != -1)
                    _blocks[bIdx].Content = $"> {GetBlockText(Datacorder.LoadedTape.Blocks[bIdx].Block)}";

                _blockIndex = bIdx;
            }
        }

        string GetBlockText(TapeBlock Block)
        {
            if (Block.BlockType == BlockType.Header)
                return $"Header: {Block.Header.Type.ToString()}, {Encoding.ASCII.GetString(Block.Header.Name)}";
            else
                return $"Data: {Block.Data.Length - 2} bytes";
        }

        protected async Task ShowError(string Title, string Text)
        {
            var window = (Window.GetTopLevel(this) as Window);

            var box = MessageBoxManager.GetMessageBoxStandardWindow(Title, Text, icon: MessageBox.Avalonia.Enums.Icon.Error);

            var prop = box.GetType().GetField("_window", BindingFlags.Instance | BindingFlags.NonPublic);
            var win = prop.GetValue(box) as Window;

            win.Icon = window.Icon;
            await box.ShowDialog(window);
        }
    }
}
