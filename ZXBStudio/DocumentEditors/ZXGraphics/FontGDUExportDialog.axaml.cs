using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using ZXBasicStudio.Classes;
using ZXBasicStudio.Common;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.DocumentModel.Interfaces;
using ZXBasicStudio.Extensions;
using ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Basic;
using static System.Net.Mime.MediaTypeNames;

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
            cmbArrayBase.SelectionChanged += CmbArrayBase_SelectionChanged;
            btnSave.Tapped += BtnSave_Tapped;
        }


        public bool Initialize(FileTypeConfig fileType, Pattern[] patterns)
        {
            this.fileType = fileType;
            this.patterns = patterns;

            this.cmbSelectExportType.Initialize(ExportType_Changed);
            exportConfig = ServiceLayer.Export_GetConfigFile(fileType.FileName + ".zbs");
            if (exportConfig == null)
                exportConfig = ServiceLayer.Export_GetDefaultConfig(fileType.FileName);

            txtLabelName.Text = exportConfig.LabelName;
            txtMemoryAddr.Text = exportConfig.ZXAddress.ToString();
            txtOutputFile.Text = exportConfig.ExportFilePath;
            txtZXFile.Text = exportConfig.ZXFileName;
            chkAuto.IsChecked = exportConfig.AutoExport;
            cmbSelectExportType.ExportType = exportConfig.ExportType;
            cmbArrayBase.SelectedIndex = exportConfig.ArrayBase.ToInteger();
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

            lblArrayBase.IsVisible = false;
            cmbArrayBase.IsVisible = false;

            switch (exportType)
            {
                case ExportTypes.Bin:
                    lblOutputFile.IsVisible = true;
                    txtOutputFile.IsVisible = true;
                    btnOutputFile.IsVisible = true;
                    CreateExportPath(".bin");
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
                    CreateExportPath(".tap");
                    CreateExample_TAP();
                    break;
                case ExportTypes.Asm:
                    lblOutputFile.IsVisible = true;
                    txtOutputFile.IsVisible = true;
                    btnOutputFile.IsVisible = true;
                    lblLabelName.IsVisible = true;
                    txtLabelName.IsVisible = true;
                    CreateExportPath(ZXDocumentProvider.GetDocumentTypeInstance(typeof(ZXBasicDocument)).DocumentExtensions.First());
                    CreateExample_ASM();
                    break;
                case ExportTypes.Dim:
                    lblOutputFile.IsVisible = true;
                    txtOutputFile.IsVisible = true;
                    btnOutputFile.IsVisible = true;
                    lblLabelName.IsVisible = true;
                    txtLabelName.IsVisible = true;
                    lblArrayBase.IsVisible = true;
                    cmbArrayBase.IsVisible = true;
                    CreateExportPath(ZXDocumentProvider.GetDocumentTypeInstance(typeof(ZXBasicDocument)).DocumentExtensions.First());
                    CreateExample_DIM();
                    break;
                case ExportTypes.Data:
                    lblOutputFile.IsVisible = true;
                    txtOutputFile.IsVisible = true;
                    btnOutputFile.IsVisible = true;
                    lblLabelName.IsVisible = true;
                    txtLabelName.IsVisible = true;
                    CreateExportPath(ZXDocumentProvider.GetDocumentTypeInstance(typeof(ZXBasicDocument)).DocumentExtensions.First());
                    CreateExample_DATA();
                    break;
                default:
                    grdOptions.IsVisible = false;
                    break;
            }
        }


        private void CreateExportPath(string extension)
        {
            if (string.IsNullOrEmpty(exportConfig.ExportFilePath))
            {
                txtOutputFile.Text = fileType.FileName + extension;
                var spName = Path.GetFileNameWithoutExtension(fileType.FileName.ToStringNoNull());
                txtLabelName.Text = spName;
                if (spName.Length > 10)
                {
                    spName = spName.Substring(0, 10);
                }
                txtZXFile.Text = spName;
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
                    sb.AppendLine("' Example of use of UDG/GDU");
                    sb.AppendLine("POKE (uinteger 23675, @MyUDG)");
                    sb.AppendLine("PRINT \"UDG/GDU Table\"");
                    sb.AppendLine("FOR n=0 TO 20");
                    sb.AppendLine("     PRINT (144+n);\" - \";CHR(n+65);\": \";CHR(144+n)");
                    sb.AppendLine("NEXT n");
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
                    sb.AppendLine("PRINT \"UDG/GDU Table\"");
                    sb.AppendLine("FOR n=0 TO 20");
                    sb.AppendLine("     PRINT (144+n);\" - \";CHR(n+65);\": \";CHR(144+n)");
                    sb.AppendLine("NEXT n");
                    sb.AppendLine("STOP");
                    sb.AppendLine("");
                    break;
            }
            txtCode.Text = sb.ToString();
        }


        private void CreateExample_ASM()
        {
            var sb = new StringBuilder();
            sb.AppendLine("' Example of use of the asm format");
            if (fileType.FileType == FileTypes.Font)
            {
                sb.AppendLine(string.Format("POKE (uinteger 23606, @{0}-256)", txtLabelName.Text));
                sb.AppendLine("PRINT \"Hello World!\"");
                sb.AppendLine("STOP");
            }
            else
            {
                sb.AppendLine(string.Format("POKE (uinteger 23675, @{0})", txtLabelName.Text));
                sb.AppendLine("PRINT \"UDG/GDU Table\"");
                sb.AppendLine("FOR n=0 TO 20");
                sb.AppendLine("     PRINT (144+n);\" - \";CHR(n+65);\": \";CHR(144+n)");
                sb.AppendLine("NEXT n");
                sb.AppendLine("STOP");
            }
            sb.AppendLine("");

            sb.Append(ExportManager.Export_ASM(fileType, patterns, txtLabelName.Text));

            txtCode.Text = sb.ToString();
        }


        private void CreateExample_DIM()
        {
            var sb = new StringBuilder();
            sb.AppendLine("' Example of use of the embedded format");
            sb.AppendLine("DIM n as ubyte");
            sb.AppendLine(ExportManager.Export_DIM(fileType, patterns, txtLabelName.Text, exportConfig.ArrayBase));
            switch (fileType.FileType)
            {
                case FileTypes.UDG:
                    sb.AppendLine(string.Format("POKE (uinteger 23675, @{0})", txtLabelName.Text));
                    sb.AppendLine("PRINT \"UDG/GDU Table\"");
                    sb.AppendLine("FOR n=0 TO 20");
                    sb.AppendLine("     PRINT (144+n);\" - \";CHR(n+65);\": \";CHR(144+n)");
                    sb.AppendLine("NEXT n");
                    sb.AppendLine("STOP");
                    break;
                case FileTypes.Font:
                    sb.AppendLine(string.Format("POKE (uinteger 23606, @{0}-256)", txtLabelName.Text));
                    sb.AppendLine("PRINT \"Hello World!\"");
                    sb.AppendLine("STOP");
                    break;
            }
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
            sb.AppendLine("' Example of use of the DATA format");
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
                    sb.AppendLine("PRINT \"Hello World!\"");
                    sb.AppendLine("STOP");
                    break;
                case FileTypes.UDG:
                    sb.AppendLine("POKE (uinteger 23675, $c000)");
                    sb.AppendLine("PRINT \"UDG/GDU Table\"");
                    sb.AppendLine("FOR n=0 TO 20");
                    sb.AppendLine("     PRINT (144+n);\" - \";CHR(n+65);\": \";CHR(144+n)");
                    sb.AppendLine("NEXT n");
                    sb.AppendLine("STOP");
                    break;
            }
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
                    fileTypes[0] = new FilePickerFileType("Basic files") { Patterns = ZXDocumentProvider.GetDocumentTypeInstance(typeof(ZXBasicDocument)).DocumentExtensions };
                    break;
                case ExportTypes.Bin:
                    fileTypes[0] = new FilePickerFileType("Binary files") { Patterns = new[] { "*.bin" } };
                    break;
                case ExportTypes.Tap:
                    fileTypes[0] = new FilePickerFileType("Tape files") { Patterns = new[] { "*.tap" } };
                    break;
            }

            var select = await StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                ShowOverwritePrompt = true,
                FileTypeChoices = fileTypes,
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
            TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(txtCode.Text);
        }


        private void BtnCancel_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.Close();
        }



        private void CmbArrayBase_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var idx = cmbArrayBase.SelectedIndex;
            exportConfig.ArrayBase = idx;
            if (exportConfig.ExportType == ExportTypes.Dim)
            {
                CreateExample_DIM();
            }
        }

        #endregion
    }
}
