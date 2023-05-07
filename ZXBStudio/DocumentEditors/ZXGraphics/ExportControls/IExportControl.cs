using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics.ExportControls
{
    public interface IExportControl
    {
        public bool Initialize(FileTypeConfig fileType, Pattern[] patterns, Action<string> CallBackCommand);

        public bool Export();
    }
}
