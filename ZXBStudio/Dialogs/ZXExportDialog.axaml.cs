using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.IO;
using ZXBasicStudio.Classes;
using ZXBasicStudio.Extensions;

namespace ZXBasicStudio.Dialogs
{
    public partial class ZXExportDialog : ZXWindowBase
    {
        ZXExportOptions? exportOptions;
        public ZXExportOptions? ExportOptions { get { return exportOptions; } set { exportOptions = value; UpdateUI(); } }
        public ZXExportDialog()
        {
            InitializeComponent();
            btnSelectOutput.Click += BtnSelectOutput_Click;
            btnAccept.Click += BtnAccept_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        void UpdateUI()
        {
            if (exportOptions == null)
            {
                txtPath.Text = "";
                ckAutorun.IsChecked = false;
                ckBasic.IsChecked = false;
            }
            else
            {
                txtPath.Text = exportOptions.OutputPath;
                ckAutorun.IsChecked = exportOptions.Autorun;
                ckBasic.IsChecked = exportOptions.AddBasic;
            }
        }

        private void BtnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close(false);
        }

        private async void BtnAccept_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPath.Text))
            {
                await this.ShowError("No output file", "Output file has not been selected, cannot export the program.");
                return;
            }

            ExportOptions = new ZXExportOptions(txtPath.Text, ckBasic.IsChecked ?? false, ckAutorun.IsChecked ?? false);
            this.Close(true);
        }

        private async void BtnSelectOutput_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var select = await StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions 
            { 
                Title = "Select output file",
                DefaultExtension = ".tap",
                FileTypeChoices = new[] 
                { 
                    new FilePickerFileType("TAP file (*.TAP)") { Patterns = new[] { "*.tap" } },
                    new FilePickerFileType("TZX file (*.TZX)") { Patterns = new[] { "*.tzx" } },
                    new FilePickerFileType("Binary file (*.BIN)") { Patterns = new[] { "*.bin" } },
                }
            });

            if (select != null)
            {
                txtPath.Text = Path.GetFullPath(select.Path.LocalPath);
                var ext = Path.GetExtension(txtPath.Text).ToLower();
                if (ext != ".tzx" && ext != ".tap" && ext != ".bin")
                {
                    txtPath.Text = "";
                    await this.ShowError("Invalid file type", "Choose a .tap, .tzx or .bin file.");
                    txtPath.Text = "";
                    return;
                }
            }
        }
    }
}
