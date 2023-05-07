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

            var fileName = fileType.FileName.Replace(".fnt", ".tap").Replace(".gdu", ".tap").Replace(".udg", ".tap");
            txtOutputFile.Text = fileName;
            txtCode.Text = "' Example of use of .tap export format\rLOAD \"\" CODE $c000\rPOKE (uinteger 23606, $c000-256)\rPRINT \"Hello World!\"\rSTOP\r";

            var spName = Path.GetFileName(fileName).Replace(".tap","");
            if (spName.Length > 10)
            {
                spName=spName.Substring(0, 10);
            }
            txtZXFile.Text = spName;

            txtMemoryAddr.Text = "49152";

            return true;
        }


        public bool Export()
        {
            var fileName = txtZXFile.Text;
            var dir = txtMemoryAddr.Text.ToInteger();

            var data=ServiceLayer.Files_CreateBinData_GDUorFont(fileType, patterns);
            data = ServiceLayer.Bin2Tap(fileName,dir, data);
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
