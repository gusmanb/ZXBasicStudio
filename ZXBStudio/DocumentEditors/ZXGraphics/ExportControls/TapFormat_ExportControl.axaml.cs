using Avalonia;
using Avalonia.Controls;
using System;
using System.IO;
using ZXBasicStudio.Common;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics.ExportControls
{
    public partial class TapFormat_ExportControl : UserControl, IExportControl
    {
        private Action<string> CallBackCommand { get; set; }
        private FileTypeConfig fileType = null;
        private Pattern[] patterns = null;

        public TapFormat_ExportControl()
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
                exportConfig.ExportType = ExportTypes.Dim;
                exportConfig.AutoExport = false;
                exportConfig.ExportFilePath = fileType.FileName.Replace(".fnt", ".tap").Replace(".gdu", ".tap").Replace(".udg", ".tap");
                exportConfig.LabelName = Path.GetFileName(exportConfig.ExportFilePath).Replace(".bas", "").Replace(" ", "_");
                exportConfig.ZXFileName = "";
                exportConfig.ZXAddress = 49152;
                var spName = Path.GetFileName(exportConfig.ZXFileName).Replace(".tap", "");
                if (spName.Length > 10)
                {
                    exportConfig.ZXFileName = spName.Substring(0, 10);
                }
            }

            chkAuto.IsChecked = exportConfig.AutoExport;
            txtOutputFile.Text = exportConfig.ExportFilePath;
            txtZXFile.Text = "";
            txtMemoryAddr.Text = "";
            txtZXFile.Text = exportConfig.ZXFileName;
            txtMemoryAddr.Text = exportConfig.ZXAddress.ToStringNoNull();

            return true;
        }


        public bool Export()
        {
            var fileName = txtZXFile.Text;
            var dir = txtMemoryAddr.Text.ToInteger();

            var data = ServiceLayer.Files_CreateBinData_GDUorFont(fileType, patterns);
            data = ServiceLayer.Bin2Tap(fileName, dir, data);
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
                ExportType = ExportTypes.Asm,
                LabelName = "",
                ZXAddress = txtMemoryAddr.Text.ToInteger(),
                ZXFileName = txtZXFile.Text
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
