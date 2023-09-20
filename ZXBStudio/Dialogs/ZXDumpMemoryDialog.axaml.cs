using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Konamiman.Z80dotNet;
using System;
using System.IO;
using System.Linq;
using System.Text;
using ZXBasicStudio.Classes;
using ZXBasicStudio.Extensions;

namespace ZXBasicStudio.Dialogs
{
    public partial class ZXDumpMemoryDialog : ZXWindowBase
    {
        IMemory? mem;

        public ZXDumpMemoryDialog()
        {
            InitializeComponent();
            btnSelectOutput.Click += BtnSelectOutput_Click;
            btnAccept.Click += BtnAccept_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        public void Initialize(IMemory Memory)
        {
            mem = Memory;
        }

        private void BtnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close(false);
        }

        private async void BtnAccept_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (mem == null)
            {
                await this.ShowError("Internal error", "Try to open the dialog again and if the error persists inform about this error.");
                return;
            }

            int start = (int)(nudStart?.Value ?? -1);
            int end = (int)(nudEnd?.Value ?? -1);
            if (start > end || start == -1 || end == -1)
            {
                await this.ShowError("Invalid range", "Selected memory range is not valid, start must be lower than end.");
                return;
            }

            if(string.IsNullOrWhiteSpace(txtPath.Text))
            {
                await this.ShowError("Select file", "Select the file where to store the memory dump.");
                return;
            }

            var ext = Path.GetExtension(txtPath.Text).ToLower();

            byte[] data = mem.GetContents(start, end - start + 1);

            try
            {

                if (ext == ".bin")
                {
                    File.WriteAllBytes(txtPath.Text, data);
                    this.Close(true);
                }
                else
                {
                    StringBuilder build = new StringBuilder();
                    for (int buc = 0; buc < data.Length; buc += 16)
                    {
                        var range = data.Skip(buc).Take(16).Select(c => c.ToString("X2"));
                        build.AppendLine(string.Join(", ", range));
                    }

                    File.WriteAllText(txtPath.Text, build.ToString());
                    this.Close(true);
                }
            }
            catch(Exception ex) { await this.ShowError("Unexpected error", $"Unexpected error saving file: {ex.Message} - {ex.StackTrace}"); }
        }

        private async void BtnSelectOutput_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var select = await StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Select output file",
                DefaultExtension = ".bin",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Binary file (*.BIN)") { Patterns = new[] { "*.bin" } },
                    new FilePickerFileType("Hexadecimal file (*.HEX)") { Patterns = new[] { "*.hex" } },
                }
            });

            if (select != null)
           {
                txtPath.Text = Path.GetFullPath(select.Path.LocalPath);
                var ext = Path.GetExtension(txtPath.Text).ToLower();
                if (ext != ".bin" && ext != ".hex")
                {
                    txtPath.Text = "";
                    await this.ShowError("Invalid file type", "Choose a .bin or .hex file.");
                    txtPath.Text = "";
                    return;
                }
            }
        }
    }
}
