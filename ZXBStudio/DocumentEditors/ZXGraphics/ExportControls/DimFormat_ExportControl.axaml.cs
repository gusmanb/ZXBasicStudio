using Avalonia.Controls;
using System;
using System.IO;
using System.Text;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using Avalonia;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics.ExportControls
{
    public partial class DimFormat_ExportControl : UserControl, IExportControl
    {
        private Action<string> CallBackCommand { get; set; }
        private FileTypeConfig fileType = null;
        private Pattern[] patterns = null;

        public DimFormat_ExportControl()
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
            sb.AppendLine("' Example of use of the embedded export format");
            sb.AppendLine(GenerateExport());
            switch (fileType.FileType)
            {
                case FileTypes.GDU:
                    sb.AppendLine(string.Format("POKE (uinteger 23675, @{0})", txtLabelName.Text));
                    break;
                case FileTypes.Font:
                    sb.AppendLine(string.Format("POKE (uinteger 23606, @{0}-256)", txtLabelName.Text));
                    break;
            }
            sb.AppendLine("PRINT \"Hello World!\"");
            sb.AppendLine("STOP");
            sb.AppendLine("");

            return sb.ToString();
        }


        private string GenerateExport()
        {
            int la = 20;
            switch (fileType.FileType)
            {
                case FileTypes.GDU:
                    la = 20;
                    break;
                case FileTypes.Font:
                    la = 95;
                    break;
            }

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("DIM {0}({1},7) AS UBYTE => {{ _", txtLabelName.Text, la.ToString()));

            var data = ServiceLayer.Files_CreateBinData_GDUorFont(fileType, patterns);
            int col = 0;
            int row = 0;
            foreach (var d in data)
            {
                if (col == 0)
                {
                    if (row == 0)
                    {
                        row = 1;
                    }
                    else
                    {
                        sb.AppendLine(", _");
                    }
                    sb.Append("\t{ ");
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
                    sb.Append(" }");
                    col = 0;
                }
            }
            sb.AppendLine(" _");
            sb.AppendLine("}");

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
