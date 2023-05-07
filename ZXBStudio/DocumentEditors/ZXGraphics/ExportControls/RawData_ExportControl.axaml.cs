using Avalonia;
using Avalonia.Controls;
using System;
using System.Linq;
using ZXBasicStudio.Common;
using ZXBasicStudio.DocumentEditors.ZXGraphics.dat;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics.ExportControls
{
    public partial class RawData_ExportControl : UserControl, IExportControl
    {
        private Action<string> CallBackCommand { get; set; }
        private FileTypeConfig fileType = null;
        private Pattern[] patterns = null;


        public RawData_ExportControl()
        {
            InitializeComponent();

            btnClose.Tapped += BtnClose_Tapped;
            btnCopy.Tapped += BtnCopy_Tapped;
        }


        public bool Initialize(FileTypeConfig fileType, Pattern[] patterns, Action<string> CallBackCommand)
        {
            this.fileType = fileType;
            this.patterns = patterns;
            this.CallBackCommand = CallBackCommand;

            txtCode.Text = "' Example of use of custom font type\rPOKE (uinteger 23606, @MyFont-256)\rPRINT \"Hello World!\"\rSTOP\r\r' Don't let the execution thread bypass the ASM\r\rMyFont:\rASM\r\tincbin \"MyFont.fnt\"\rEND ASM\r";
            return true;
        }


        public bool Export()
        {
            return ServiceLayer.Files_Save_GDUorFont(fileType, patterns);
        }

        private void BtnClose_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            CallBackCommand?.Invoke("CLOSE");
        }


        private void BtnCopy_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            Application.Current.Clipboard.SetTextAsync(txtCode.Text).Wait();
        }

    }
}
