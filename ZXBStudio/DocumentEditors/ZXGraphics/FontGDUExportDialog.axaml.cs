using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using ZXBasicStudio.Common;
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
        private Pattern[] patterns = null;
        private ExportConfig exportConfig = null;

        public FontGDUExportDialog()
        {
            InitializeComponent();

            btnCancel.Tapped += BtnCancel_Tapped;
            btnCopy.Tapped += BtnCopy_Tapped;
            btnExport.Tapped += BtnExport_Tapped;
            btnOutputFile.Tapped += BtnOutputFile_Tapped;
            btnSave.Tapped += BtnSave_Tapped;
        }


        public bool Initialize(FileTypeConfig fileType, Pattern[] patterns)
        {
            this.fileType = fileType;
            this.patterns = patterns;

            this.cmbSelectExportType.Initialize(ExportType_Changed);
            exportConfig = ServiceLayer.Export_GetConfigFile(fileType.FileName + ".zbs");
            if (exportConfig == null)
            {
                exportConfig = new ExportConfig();
                exportConfig.ExportType = ExportTypes.None;
                exportConfig.AutoExport = false;
                exportConfig.ExportFilePath = fileType.FileName.Replace(".fnt", ".bas").Replace(".gdu", ".bas").Replace(".udg", ".bas");
                exportConfig.LabelName = Path.GetFileName(exportConfig.ExportFilePath).Replace(".bas", "").Replace(" ", "_");
                exportConfig.ZXAddress = 49152;
                var spName = Path.GetFileName(exportConfig.ZXFileName).Replace(".tap", "");
                if (spName.Length > 10)
                {
                    exportConfig.ZXFileName = spName.Substring(0, 10);
                }
            }

            txtLabelName.Text = exportConfig.LabelName;
            txtMemoryAddr.Text = exportConfig.ZXAddress.ToString();
            txtOutputFile.Text = exportConfig.ExportFilePath;
            txtZXFile.Text = exportConfig.ZXFileName;
            chkAuto.IsChecked = exportConfig.AutoExport;
            cmbSelectExportType.ExportType = exportConfig.ExportType;

            return true;
        }


        #region ExportOptions

        private void ExportType_Changed(ExportTypes exportType)
        {
            exportConfig.ExportType = exportType;

            grdOptions.IsVisible = true;

            chkAuto.IsVisible = true;

            lblOutputFile.IsVisible = false;
            txtOutputFile.IsVisible = false;
            btnOutputFile.IsVisible = false;

            lblLabelName.IsVisible = false;
            txtLabelName.IsVisible = false;

            lblZXFile.IsVisible = false;
            txtZXFile.IsVisible = false;

            lblMemoryAddr.IsVisible = false;
            txtMemoryAddr.IsVisible = false;

            switch (exportType)
            {
                case ExportTypes.Bin:
                    lblOutputFile.IsVisible = true;
                    txtOutputFile.IsVisible = true;
                    btnOutputFile.IsVisible = true;
                    CreateExample_BIN();
                    break;
                case ExportTypes.Tap:
                    lblOutputFile.IsVisible = true;
                    txtOutputFile.IsVisible = true;
                    btnOutputFile.IsVisible = true;
                    lblZXFile.IsVisible = true;
                    txtZXFile.IsVisible = true;
                    lblMemoryAddr.IsVisible = true;
                    txtMemoryAddr.IsVisible = true;
                    CreateExample_TAP();
                    break;
                case ExportTypes.Asm:
                    lblOutputFile.IsVisible = true;
                    txtOutputFile.IsVisible = true;
                    btnOutputFile.IsVisible = true;
                    lblLabelName.IsVisible = true;
                    txtLabelName.IsVisible = true;
                    CreateExample_ASM();
                    break;
                case ExportTypes.Dim:
                    lblOutputFile.IsVisible = true;
                    txtOutputFile.IsVisible = true;
                    btnOutputFile.IsVisible = true;
                    lblLabelName.IsVisible = true;
                    txtLabelName.IsVisible = true;
                    CreateExample_DIM();
                    break;
                case ExportTypes.Data:
                    lblOutputFile.IsVisible = true;
                    txtOutputFile.IsVisible = true;
                    btnOutputFile.IsVisible = true;
                    lblLabelName.IsVisible = true;
                    txtLabelName.IsVisible = true;
                    CreateExample_DATA();
                    break;
                default:
                    grdOptions.IsVisible = false;
                    break;
            }
        }

        #endregion


        #region Examples

        private void CreateExample_BIN()
        {
            var sb = new StringBuilder();

            switch (fileType.FileType)
            {
                case FileTypes.Font:
                    sb.AppendLine("' Example of use of custom font type");
                    sb.AppendLine("POKE (uinteger 23606, @MyFont-256)");
                    sb.AppendLine("PRINT \"Hello World!\"");
                    sb.AppendLine("STOP");
                    sb.AppendLine("");
                    sb.AppendLine("' Don't let the execution thread bypass the ASM");
                    sb.AppendLine("");
                    sb.AppendLine("MyFont:");
                    sb.AppendLine("ASM");
                    sb.AppendLine("\tincbin \"MyFont.fnt\"");
                    sb.AppendLine("END ASM");
                    sb.AppendLine("");
                    break;
                case FileTypes.UDG:
                    sb.AppendLine("' Example of use of UDG\rPOKE (uinteger 23675, @MyUDG)\rPRINT \"Hello World!\"\rSTOP\r\r' Don't let the execution thread bypass the ASM\r\rMyUGD:\rASM\r\tincbin \"MyUDG.fnt\"\rEND ASM\r");
                    sb.AppendLine("' Example of use of UDG/GDU");
                    sb.AppendLine("POKE (uinteger 23675, @MyUDG)");
                    sb.AppendLine("PRINT \"Hello World!\"");
                    sb.AppendLine("STOP");
                    sb.AppendLine("");
                    sb.AppendLine("' Don't let the execution thread bypass the ASM");
                    sb.AppendLine("");
                    sb.AppendLine("MyUDG:");
                    sb.AppendLine("ASM");
                    sb.AppendLine("\tincbin \"MyUDG.fnt\"");
                    sb.AppendLine("END ASM");
                    sb.AppendLine("");
                    break;
            }
            txtCode.Text = sb.ToString();
        }


        private void CreateExample_TAP()
        {
            var sb = new StringBuilder();
            sb.AppendLine("' Example of use of tap");
            sb.AppendLine("");
            sb.AppendLine("' Load data from .tap");
            sb.AppendLine("load \"\" code 60000");

            switch (fileType.FileType)
            {
                case FileTypes.Font:
                    sb.AppendLine("POKE (uinteger 23606, 60000-256)");
                    sb.AppendLine("PRINT \"Hello World!\"");
                    sb.AppendLine("STOP");
                    sb.AppendLine("");
                    break;
                case FileTypes.UDG:
                    sb.AppendLine("POKE (uinteger 23675, 60000)");
                    sb.AppendLine("PRINT \"Hello World!\"");
                    sb.AppendLine("STOP");
                    sb.AppendLine("");
                    break;
            }
            txtCode.Text = sb.ToString();
        }


        private void CreateExample_ASM()
        {
            var sb = new StringBuilder();
            sb.AppendLine("' Example of use of the asm export format");
            if (fileType.FileType == FileTypes.Font)
            {
                sb.AppendLine(string.Format("POKE (uinteger 23606, @{0}-256)", txtLabelName.Text));
            }
            else
            {
                sb.AppendLine(string.Format("POKE (uinteger 23675, @{0})", txtLabelName.Text));
            }
            sb.AppendLine("PRINT \"Hello World!\"");
            sb.AppendLine("STOP");
            sb.AppendLine("");

            sb.Append(ExportManager.Export_ASM(fileType, patterns, txtLabelName.Text));

            txtCode.Text = sb.ToString();
        }


        private void CreateExample_DIM()
        {
            var sb = new StringBuilder();
            sb.AppendLine("' Example of use of the embedded export format");
            sb.AppendLine(ExportManager.Export_DIM(fileType, patterns, txtLabelName.Text));
            switch (fileType.FileType)
            {
                case FileTypes.UDG:
                    sb.AppendLine(string.Format("POKE (uinteger 23675, @{0})", txtLabelName.Text));
                    break;
                case FileTypes.Font:
                    sb.AppendLine(string.Format("POKE (uinteger 23606, @{0}-256)", txtLabelName.Text));
                    break;
            }
            sb.AppendLine("PRINT \"Hello World!\"");
            sb.AppendLine("STOP");
            sb.AppendLine("");

            txtCode.Text = sb.ToString();
        }


        private void CreateExample_DATA()
        {
            int la = 168;
            switch (fileType.FileType)
            {
                case FileTypes.UDG:
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
                case FileTypes.UDG:
                    sb.AppendLine("POKE (uinteger 23675, $c000)");
                    break;
            }
            sb.AppendLine("PRINT \"Hello World!\"");
            sb.AppendLine("STOP");
            sb.AppendLine("");

            sb.AppendLine(ExportManager.Export_DATA(fileType, patterns, txtLabelName.Text));

            txtCode.Text = sb.ToString();
        }


        private void Export()
        {
            var em = new ExportManager();
            em.Initialize(fileType.FileType);
            em.Export(exportConfig, fileType, patterns);
        }


        private void GetExportOptions()
        {
            exportConfig.AutoExport = chkAuto.IsChecked.ToBoolean();
            exportConfig.ExportFilePath = txtOutputFile.Text;
            exportConfig.LabelName = txtLabelName.Text;
            exportConfig.ZXAddress = txtMemoryAddr.Text.ToInteger();
            exportConfig.ZXFileName = txtZXFile.Text;
        }

        #endregion


        #region Buttons

        private void BtnSave_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            GetExportOptions();
            ServiceLayer.Export_SetConfigFile(fileType.FileName + ".zbs", exportConfig);
            Export();
            this.Close();
        }

        private async void BtnOutputFile_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            var fileTypes = new FilePickerFileType[2];
            fileTypes[1] = new FilePickerFileType("All files") { Patterns = new[] { "*", "*.*" } };
            switch (exportConfig.ExportType)
            {
                case ExportTypes.Asm:
                case ExportTypes.Data:
                case ExportTypes.Dim:
                    fileTypes[0] = new FilePickerFileType("Basic files (*.bas)") { Patterns = new[] { "*.bas", "*.zxbas" } };
                    break;
                case ExportTypes.Bin:
                    fileTypes[0] = new FilePickerFileType("Binary files (*.bin)") { Patterns = new[] { "*.bin" } };
                    break;
                case ExportTypes.Tap:
                    fileTypes[0] = new FilePickerFileType("Tape files (*.bin)") { Patterns = new[] { "*.tap" } };
                    break;
            }

            var select = await StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                ShowOverwritePrompt = true,
                FileTypeChoices=fileTypes,
                Title = "Select Export path...",
            });

            if (select != null)
            {
                txtOutputFile.Text = Path.GetFullPath(select.Path.LocalPath);
            }
        }


        private void BtnExport_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            GetExportOptions();
            Export();
            this.Close();
        }


        private void BtnCopy_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            Application.Current.Clipboard.SetTextAsync(txtCode.Text);
        }


        private void BtnCancel_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.Close();
        }


        #endregion
    }
}
