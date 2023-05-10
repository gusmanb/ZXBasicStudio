using Avalonia;
using Avalonia.Controls;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.DocumentModel.Interfaces;
using ZXBasicStudio.Extensions;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    public partial class FontGDUExportDialog : Window
    {
        private FileTypeConfig fileType = null;
        private PatternControl[] patterns = null;


        public FontGDUExportDialog()
        {
            InitializeComponent();
        }


        public bool Initialize(FileTypeConfig fileType, PatternControl[] patterns)
        {
            this.fileType = fileType;
            this.patterns= patterns;

            this.cmbSelectExportType.Initialize(ExportType_Changed);
            var exportConfig = ServiceLayer.Export_GetConfigFile(fileType.FileName + ".zbs");
            if (exportConfig != null)
            {
                this.cmbSelectExportType.ExportType = exportConfig.ExportType;
            }
            return true;
        }


        #region ExportOptions

        private ExportControls.IExportControl exportControl = null;

        private void ExportType_Changed(ExportTypes exportType)
        {
            grdExportControl.Children.Clear();


            switch (exportType)
            {
                case ExportTypes.Bin:
                    exportControl = new ExportControls.RawData_ExportControl();
                    break;
                case ExportTypes.Tap:
                    exportControl = new ExportControls.TapFormat_ExportControl();
                    break;
                case ExportTypes.Asm:
                    exportControl = new ExportControls.AsmFormat_ExportControl();
                    break;
                case ExportTypes.Dim:
                    exportControl = new ExportControls.DimFormat_ExportControl();
                    break;
                case ExportTypes.Data:
                    exportControl = new ExportControls.DataFormat_ExportControl();
                    break;
            }

            if (exportControl == null)
            {
                return;
            }

            grdExportControl.Children.Add((UserControl)exportControl);
            var data = patterns.Select(d => d.Pattern).ToArray();
            exportControl.Initialize(fileType, data, Export_Commad);
        }


        private void Export_Commad(string command)
        {
            switch (command)
            {
                case "CLOSE":
                    this.Close();
                    break;
            }
        }




        #endregion
    }
}
