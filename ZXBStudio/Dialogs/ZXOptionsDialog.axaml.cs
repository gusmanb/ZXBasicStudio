using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.IO;
using ZXBasicStudio.Classes;
using ZXBasicStudio.Extensions;

namespace ZXBasicStudio.Dialogs
{
    public partial class ZXOptionsDialog : ZXWindowBase
    {
        ZXBuildSettings? bsett = null;

        public ZXOptionsDialog()
        {
            InitializeComponent();
            btnAccept.Click += BtnAccept_Click;
            btnCancel.Click += BtnCancel_Click;
            btnSelectZxbc.Click += BtnSelectZxbc_Click;
            btnSelectZxbasm.Click += BtnSelectZxbasm_Click;
            btnDefaultBuildConfig.Click += BtnDefaultBuildConfig_Click;

            txtZxbasm.Text = ZXOptions.Current.ZxbasmPath;
            txtZxbc.Text = ZXOptions.Current.ZxbcPath;
            nudFontSize.Value = (decimal)ZXOptions.Current.EditorFontSize;
            ckDisableAudio.IsChecked = ZXOptions.Current.AudioDisabled;
            ckWordWrap.IsChecked = ZXOptions.Current.WordWrap;
            ckCls.IsChecked = ZXOptions.Current.Cls;
            ckBorderless.IsChecked = ZXOptions.Current.Borderless;
            ckAntiAlias.IsChecked = ZXOptions.Current.AntiAlias;

            btnKeybMap.Click += BtnKeybMap_Click;
        }

        private void BtnKeybMap_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dlg = new ZXKeybMappingDialog();
            dlg.ShowDialog(this);
        }

        private async void BtnDefaultBuildConfig_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dlg = new ZXBuildSettingsDialog();
            if (ZXOptions.Current.DefaultBuildSettings != null)
                dlg.Settings = ZXOptions.Current.DefaultBuildSettings.Clone();

            var result = await dlg.ShowDialog<bool>(this);

            if (!result)
                return;

            bsett = dlg.Settings;
        }

        private async void BtnSelectZxbasm_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var select = await StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions 
            { 
                AllowMultiple = false, 
                Title = "Select ZXBASM path...", 
                FileTypeFilter = new[] 
                { 
                    new FilePickerFileType("ZXBASM executable") { Patterns = new[] { "zxbasm.exe" } },
                    new FilePickerFileType("All files") { Patterns = new[] { "*", "*.*" } }
                } 
            });

            if (select != null && select.Count > 0)
                txtZxbasm.Text = Path.GetFullPath(select[0].Path.LocalPath);
        }

        private async void BtnSelectZxbc_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var select = await StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions 
            { 
                AllowMultiple = false, 
                Title = "Select ZXBC path...", 
                FileTypeFilter = new[] 
                { 
                    new FilePickerFileType("ZXBC executable") { Patterns = new[] { "zxbc.exe" } }, 
                    new FilePickerFileType("All files") { Patterns = new[] { "*", "*.*" } } 
                } 
            });

            if (select != null && select.Count > 0)
                txtZxbc.Text = Path.GetFullPath(select[0].Path.LocalPath);
        }

        private void BtnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close(false);
        }

        private async void BtnAccept_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtZxbc.Text) || !File.Exists(txtZxbc.Text))
            {
                await this.ShowError("Invalid Zxbc.", "The selected path for Zxbc is invalid, check your settings and try again.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtZxbasm.Text) || !File.Exists(txtZxbasm.Text))
            {
                await this.ShowError("Invalid Zxbasm.", "The selected path for Zxbasm is invalid, check your settings and try again.");
                return;
            }

            ZXOptions.Current.ZxbcPath = txtZxbc.Text;
            ZXOptions.Current.ZxbasmPath= txtZxbasm.Text;
            ZXOptions.Current.EditorFontSize = (double)(nudFontSize.Value ?? 16);
            ZXOptions.Current.AudioDisabled = ckDisableAudio.IsChecked ?? false;
            ZXOptions.Current.WordWrap = ckWordWrap.IsChecked ?? false;
            ZXOptions.Current.Cls = ckCls.IsChecked ?? false;
            ZXOptions.Current.Borderless = ckBorderless.IsChecked ?? false;
            ZXOptions.Current.AntiAlias = ckAntiAlias.IsChecked ?? false;

            if (bsett != null)
                ZXOptions.Current.DefaultBuildSettings = bsett;

            ZXOptions.SaveCurrentSettings();
            this.Close(true);
        }
    }
}
