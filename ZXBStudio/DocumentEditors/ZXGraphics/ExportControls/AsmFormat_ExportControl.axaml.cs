using Avalonia.Controls;
using System;
using System.IO;
using System.Text;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using Avalonia;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics.ExportControls
{
    public partial class AsmFormat_ExportControl : UserControl, IExportControl
    {
        private Action<string> CallBackCommand { get; set; }
        private FileTypeConfig fileType = null;
        private Pattern[] patterns = null;

        public AsmFormat_ExportControl()
        {
            InitializeComponent();

            btnCopy.Tapped += BtnCopy_Tapped;
            btnCancel.Tapped += BtnCancel_Tapped;
            btnExport.Tapped += BtnExport_Tapped;
            btnSave.Tapped += BtnSave_Tapped;
        }


        public bool Initialize(FileTypeConfig fileType, Pattern[] patterns, Action<string> CallBackCommand)
        {
            this.fileType = fileType;
            this.patterns = patterns;
            this.CallBackCommand = CallBackCommand;

            var fileName = fileType.FileName.Replace(".fnt", ".bas").Replace(".gdu", ".bas").Replace(".udg", ".bas");
            txtOutputFile.Text = fileName;

            var spName = Path.GetFileName(fileName).Replace(".bas", "").Replace(" ", "_");
            txtLabelName.Text = spName;

            txtCode.Text = GenerateExample();

            return true;
        }


        private string GenerateExample()
        {
            var sb = new StringBuilder();
            sb.AppendLine("' Example of use of the asm export format");
            sb.AppendLine(string.Format("POKE (uinteger 23606, @{0}-256)", txtLabelName.Text));
            sb.AppendLine("PRINT \"Hello World!\"");
            sb.AppendLine("STOP");
            sb.AppendLine("");

            sb.Append(GenerateExport());
                        
            return sb.ToString();
        }


        private string GenerateExport()
        {
            var sb = new StringBuilder();
            sb.AppendLine(txtLabelName.Text + ":");
            sb.AppendLine("ASM");

            var data = ServiceLayer.Files_CreateBinData_GDUorFont(fileType, patterns);
            int col = 0;

            foreach (var d in data)
            {
                if (col == 0)
                {
                    sb.Append("\tDB ");
                }
                if (col > 0)
                {
                    sb.Append(",");
                }
                var x = string.Format("${0:X2}", d);
                sb.Append(x);

                col++;
                if (col >= 8)
                {
                    sb.AppendLine("");
                    col = 0;
                }
            }

            sb.AppendLine("END ASM");

            return sb.ToString();
        }


        public bool Export()
        {
            var text = GenerateExport();
            var data = Encoding.UTF8.GetBytes(text);
            ServiceLayer.Files_SaveFileData(txtOutputFile.Text, data);

            return true;
        }


        private void BtnCancel_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            CallBackCommand?.Invoke("CLOSE");
        }


        private void BtnSave_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            throw new NotImplementedException();
        }


        private void BtnExport_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            Export();
            CallBackCommand?.Invoke("CLOSE");
        }


        private void BtnCopy_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            Application.Current.Clipboard.SetTextAsync(txtCode.Text).Wait();
        }
    }
}
