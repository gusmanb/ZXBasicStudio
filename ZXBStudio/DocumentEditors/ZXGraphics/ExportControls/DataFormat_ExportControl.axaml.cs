using Avalonia.Controls;
using System;
using System.IO;
using System.Text;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using Avalonia;
using ZXBasicStudio.Common;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics.ExportControls
{
    public partial class DataFormat_ExportControl : UserControl, IExportControl
    {
        private Action<string> CallBackCommand { get; set; }
        private FileTypeConfig fileType = null;
        private Pattern[] patterns = null;

        public DataFormat_ExportControl()
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

            ExportConfig exportConfig = ServiceLayer.Export_GetConfigFile(fileType.FileName + ".zbs");
            if (exportConfig == null)
            {
                exportConfig = new ExportConfig();
                exportConfig.ExportType = ExportTypes.Data;
                exportConfig.AutoExport = false;
                exportConfig.ExportFilePath = fileType.FileName.Replace(".fnt", ".bas").Replace(".gdu", ".bas").Replace(".udg", ".bas");
                exportConfig.LabelName = Path.GetFileName(exportConfig.ExportFilePath).Replace(".bas", "").Replace(" ", "_");
                exportConfig.ZXFileName = "";
                exportConfig.ZXAddress = 49152;

            }

            txtCode.Text = GenerateExample();

            return true;
        }


        private string GenerateExample()
        {
            int la = 168;
            switch (fileType.FileType)
            {
                case FileTypes.GDU:
                    la = 168;
                    break;
                case FileTypes.Font:
                    la = 768;
                    break;
            }

            var sb = new StringBuilder();
            sb.AppendLine("' Example of use of the DATA export format");
            sb.AppendLine("DIM dir AS UINTEGER = $c000");
            sb.AppendLine("DIM n AS UINTEGER");
            sb.AppendLine("DIM d AS UBYTE");
            sb.AppendLine("");
            sb.AppendLine("RESTORE " + txtLabelName.Text);
            sb.AppendLine(string.Format("FOR n=0 to {0}", la));
            sb.AppendLine("\tREAD d");
            sb.AppendLine("\tPOKE dir,d");
            sb.AppendLine("\tdir=dir+1");
            sb.AppendLine("NEXT n");
            sb.AppendLine("");
            switch (fileType.FileType)
            {
                case FileTypes.Font:
                    sb.AppendLine("POKE (uinteger 23606, $c000-256)");
                    break;
                case FileTypes.GDU:
                    sb.AppendLine("POKE (uinteger 23675, $c000)");
                    break;
            }
            sb.AppendLine("PRINT \"Hello World!\"");
            sb.AppendLine("STOP");
            sb.AppendLine("");

            sb.AppendLine(GenerateExport());
            return sb.ToString();
        }


        private string GenerateExport()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0}:", txtLabelName.Text));

            var data = ServiceLayer.Files_CreateBinData_GDUorFont(fileType, patterns);
            int col = 0;
            int row = 0;
            foreach (var d in data)
            {
                if (col == 0)
                {
                    sb.Append("DATA ");
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
            var exportConfig = new ExportConfig()
            {
                AutoExport = chkAuto.IsChecked.ToBoolean(),
                ExportFilePath = txtOutputFile.Text.ToStringNoNull(),
                ExportType = ExportTypes.Data,
                LabelName = txtLabelName.ToStringNoNull(),
                ZXAddress = 49152,
                ZXFileName = ""
            };
            ServiceLayer.Export_SetConfigFile(fileType.FileName + ".zbs", exportConfig);
            Export();
            CallBackCommand?.Invoke("CLOSE");
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
