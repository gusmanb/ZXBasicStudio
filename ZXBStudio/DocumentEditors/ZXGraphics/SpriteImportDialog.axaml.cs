using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    public partial class SpriteImportDialog : Window, IDisposable
    {
        /*
        private string fileName = "";


        private FileTypeConfig fileType = null;
        private Pattern[] patterns = null;
        private ExportConfig exportConfig = null;
        */

        private Action<Sprite, string> CallBackCommand = null;
        private IEnumerable<Sprite> sprites = null;
        private Sprite sprite = null;

        private string spriteName = "";
        private GraphicsModes spriteMode = GraphicsModes.Monochrome;
        private int spriteWidth = 16;
        private int spriteHeight = 16;
        private int spriteFrames = 1;
        private int spriteMargin = 0;
        private int spriteZoom = 4;

        private DispatcherTimer tmr = null;


        public SpriteImportDialog()
        {
            InitializeComponent();

            btnFile.Tapped += BtnFile_Tapped;
            sldZoom.PropertyChanged += SldZoom_PropertyChanged;

            grdImport.KeyDown += GrdImport_KeyDown;

            cmbMode.SelectionChanged += CmbMode_SelectionChanged;
            txtWidth.ValueChanged += TxtWidth_ValueChanged;
            txtHeight.ValueChanged += TxtHeight_ValueChanged;
            txtFrames.ValueChanged += TxtFrames_ValueChanged;

            btnCancel.Tapped += BtnCancel_Tapped;
            btnImport.Tapped += BtnImport_Tapped;
        }

        public bool Initialize(string fileName, IEnumerable<Sprite> sprites, Action<Sprite, string> callBackCommand)
        {
            this.CallBackCommand = callBackCommand;
            this.sprites = sprites;

            spriteName = "";
            spriteMode = GraphicsModes.Monochrome;
            spriteWidth = 16;
            spriteHeight = 16;
            spriteFrames = 1;
            spriteMargin = 0;
            spriteZoom = 4;

            txtName.Text = spriteName;
            cmbMode.SelectedIndex = 0;
            txtWidth.Text = spriteWidth.ToString();
            txtHeight.Text = spriteHeight.ToString();
            txtFrames.Text = spriteFrames.ToString();
            txtMargin.Text = spriteMargin.ToString();
            sldZoom.Value = spriteZoom;

            tmr = new DispatcherTimer(TimeSpan.FromMilliseconds(1000), DispatcherPriority.Normal, UpdatePreview);

            return true;
        }

        public void Dispose()
        {
            if (tmr != null)
            {
                tmr.Stop();
                tmr = null;
            }
        }

#if NO
        #region ExportOptions

        private void ExportType_Changed(ExportTypes exportType)
        {
            exportConfig.ExportType = exportType;
            Refresh();
        }


        private void Refresh()
        {
            grdOptions.IsVisible = true;

            chkAuto.IsVisible = true;

            lblOutputFile.IsVisible = true;
            txtOutputFile.IsVisible = true;
            btnOutputFile.IsVisible = true;

            lblLabelName.IsVisible = true;
            txtLabelName.IsVisible = true;

            lblArrayBase.IsVisible = true;
            cmbDataType.IsVisible = true;
            cmbArrayBase.IsVisible = true;

            bool canExport = false;
            if (sprites == null || sprites.Where(d => d != null && d.Export).Count() == 0)
            {
                txtError.IsVisible = true;
            }
            else
            {
                canExport = true;
                txtError.IsVisible = false;
            }

            switch (exportConfig.ExportType)
            {
                case ExportTypes.PutChars:
                    CreateExportPath(".bas");
                    if (canExport)
                    {
                        CreateExample_PutChars();
                    }
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
            }
        }


        #endregion


        #region Examples

        private void GetConfigFromUI()
        {
            if (exportConfig == null)
            {
                exportConfig = new ExportConfig();
            }
            exportConfig.ArrayBase = cmbArrayBase.SelectedIndex.ToInteger();
            exportConfig.AutoExport = chkAuto.IsChecked == true;
            exportConfig.ExportDataType = (ExportDataTypes)cmbDataType.SelectedIndex.ToInteger();
            exportConfig.ExportFilePath = txtOutputFile.Text.ToStringNoNull();
            exportConfig.ExportType = cmbSelectExportType.ExportType;
            exportConfig.LabelName = txtLabelName.Text.ToStringNoNull();
            exportConfig.ZXAddress = 0;
            exportConfig.ZXFileName = "";
            exportConfig.IncludeAttr = chkAttr.IsChecked == true;
        }

        private void CreateExample_PutChars()
        {
            if (sprites == null || sprites.Count() == 0)
            {
                txtCode.Text = "";
            }

            var sb = new StringBuilder();
            switch (exportConfig.ExportDataType)
            {
                case ExportDataTypes.DIM:
                    {
                        sb.AppendLine("'- Includes -----------------------------------------------");
                        sb.AppendLine("#INCLUDE <putchars.bas>");
                        sb.AppendLine("");
                        sb.AppendLine(string.Format("' Can use: #INCLUDE \"{0}\"",
                            Path.GetFileName(exportConfig.ExportFilePath)));
                        sb.AppendLine(ExportManager.Export_Sprite_PutChars(exportConfig, sprites));
                        sb.AppendLine("");
                        sb.AppendLine("'- Draw sprite --------------------------------------------");

                        var sprite = sprites.ElementAt(0);
                        sb.AppendLine(string.Format(
                            "putChars(10,5,{0},{1},@{2}{3}({4}))",
                            sprite.Width / 8,
                            sprite.Height / 8,
                            exportConfig.LabelName,
                            sprite.Name.Replace(" ", "_"),
                            sprite.Frames == 1 ? "0" : "0,0"));
                        sb.AppendLine("");
                    }
                    break;

                case ExportDataTypes.ASM:
                    {
                        sb.AppendLine("'- Includes -----------------------------------------------");
                        sb.AppendLine("#INCLUDE <putchars.bas>");
                        sb.AppendLine("");
                        sb.AppendLine("'- Draw sprite --------------------------------------------");
                        var sprite = sprites.ElementAt(0);
                        sb.AppendLine(string.Format(
                            "putChars(10,5,{0},{1},@{2}{3})",
                            sprite.Width / 8,
                            sprite.Height / 8,
                            exportConfig.LabelName,
                            sprite.Name.Replace(" ", "_")));
                        sb.AppendLine("");
                        sb.AppendLine("' This section must not be executed");
                        sb.AppendLine(string.Format("' Can use: #INCLUDE \"{0}\"",
                            Path.GetFileName(exportConfig.ExportFilePath)));
                        sb.AppendLine(ExportManager.Export_Sprite_PutChars(exportConfig, sprites));
                    }
                    break;

                case ExportDataTypes.BIN:
                    {
                        sb.AppendLine("'- Includes -----------------------------------------------");
                        sb.AppendLine("#INCLUDE <putchars.bas>");
                        sb.AppendLine("");
                        sb.AppendLine("'- Draw sprite --------------------------------------------");
                        var sprite = sprites.ElementAt(0);
                        sb.AppendLine(string.Format(
                            "putChars(10,5,{0},{1},@{2})",
                            sprite.Width / 8,
                            sprite.Height / 8,
                            exportConfig.LabelName));
                        sb.AppendLine("");
                        sb.AppendLine("' This section must not be executed");
                        sb.AppendLine(string.Format(
                            "{0}:",
                            exportConfig.LabelName));
                        sb.AppendLine("ASM");
                        sb.AppendLine(string.Format("\tINCBIN \"{0}\"",
                            Path.GetFileName(exportConfig.ExportFilePath)));
                        sb.AppendLine("END ASM");
                    }
                    break;

                case ExportDataTypes.TAP:
                    {
                        sb.AppendLine("'- Includes -----------------------------------------------");
                        sb.AppendLine("#INCLUDE <putchars.bas>");
                        sb.AppendLine("");
                        sb.AppendLine("' Load .tap data ------------------------------------------");
                        sb.AppendLine("LOAD \"\" CODE");
                        sb.AppendLine("");
                        sb.AppendLine("'- Draw sprite --------------------------------------------");
                        var sprite = sprites.ElementAt(0);
                        sb.AppendLine(string.Format(
                            "putChars(10,5,{0},{1},@{2})",
                            sprite.Width / 8,
                            sprite.Height / 8,
                            exportConfig.LabelName));
                        sb.AppendLine("");
                    }
                    break;
            }

            txtCode.Text = sb.ToString();
        }

        #endregion


        #region Export

        private void Export()
        {
            GetConfigFromUI();
            var em = new ExportManager();
            em.Initialize(FileTypes.Sprite);
            em.ExportSprites(exportConfig, sprites);
        }

        #endregion


        #region Buttons

        private void BtnSave_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            GetConfigFromUI();
            ServiceLayer.Export_SetConfigFile(fileName + ".zbs", exportConfig);
            Export();
            this.Close();
        }

        private async void BtnOutputFile_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            var fileTypes = new FilePickerFileType[2];
            fileTypes[1] = new FilePickerFileType("All files") { Patterns = new[] { "*", "*.*" } };
            fileTypes[0] = new FilePickerFileType("Sprite files") { Patterns = new[] { "*.spr" } };

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
            GetConfigFromUI();
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
            Refresh();
        }


        private void CmbDataType_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var idx = cmbDataType.SelectedIndex;
            exportConfig.ExportDataType = (ExportDataTypes)idx;

            var ext = "";
            switch (exportConfig.ExportDataType)
            {
                case ExportDataTypes.ASM:
                case ExportDataTypes.DIM:
                    ext = ".bas";
                    break;
                case ExportDataTypes.BIN:
                    ext = ".bin";
                    break;
                case ExportDataTypes.TAP:
                    ext = ".tap";
                    break;
                default:
                    return;
            }
            txtOutputFile.Text = Path.Combine(Path.GetDirectoryName(txtOutputFile.Text), Path.GetFileNameWithoutExtension(txtOutputFile.Text) + ext);
            exportConfig.ExportFilePath = txtOutputFile.Text;
            Refresh();
        }


        private void ChkAttr_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            exportConfig.IncludeAttr = chkAttr.IsChecked.ToBoolean();
            Refresh();
        }

        #endregion
#endif

        #region Image source

        private async void BtnFile_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            var fileTypes = new FilePickerFileType[5];
            fileTypes[0] = new FilePickerFileType("All files") { Patterns = new[] { "*", "*.*" } };
            fileTypes[1] = new FilePickerFileType("BMP files") { Patterns = new[] { "*.bmp" } };
            fileTypes[2] = new FilePickerFileType("JPG files") { Patterns = new[] { "*.jpeg" } };
            fileTypes[3] = new FilePickerFileType("PNG files") { Patterns = new[] { "*.png" } };
            fileTypes[4] = new FilePickerFileType("SCR Spectrum screem files") { Patterns = new[] { "*.scr" } };

            var select = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = fileTypes,
                Title = "Select file to import...",
            });

            if (select != null && select.Count > 0)
            {
                cnvSource.LoadImage(select[0]);
            }
        }


        private int lastZoom = 0;

        /// <summary>
        /// Zoom changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldZoom_PropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
        {
            int z = (int)sldZoom.Value;
            if (z == 0 || z == lastZoom)
            {
                return;
            }
            lastZoom = z;

            txtZoom.Text = "Zoom " + z.ToString() + "x";
            cnvSource.Zoom = z;
        }


        private void GrdImport_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Avalonia.Input.Key.Up:
                    if (e.KeyModifiers == Avalonia.Input.KeyModifiers.Shift)
                    {
                        cnvSource.OffsetY += 8;
                    }
                    else
                    {
                        cnvSource.OffsetY++;
                    }
                    break;
                case Avalonia.Input.Key.Down:
                    if (e.KeyModifiers == Avalonia.Input.KeyModifiers.Shift)
                    {
                        cnvSource.OffsetY -= 8;
                    }
                    else
                    {
                        cnvSource.OffsetY--;
                    }
                    break;
                case Avalonia.Input.Key.Left:
                    if (e.KeyModifiers == Avalonia.Input.KeyModifiers.Shift)
                    {
                        cnvSource.OffsetX += 8;
                    }
                    else
                    {
                        cnvSource.OffsetX++;
                    }
                    break;
                case Avalonia.Input.Key.Right:
                    if (e.KeyModifiers == Avalonia.Input.KeyModifiers.Shift)
                    {
                        cnvSource.OffsetX -= 8;
                    }
                    else
                    {
                        cnvSource.OffsetX--;
                    }
                    break;
            }
        }

        #endregion


        #region Preview


        private void UpdatePreview(object userState, EventArgs e)
        {
            _UpdatePreview();
        }

        private void _UpdatePreview()
        {
            try
            {
                var imgData = cnvSource.imageData;
                if (imgData == null)
                {
                    return;
                }

                GetProperties();

                var s = new Sprite();
                s.CurrentFrame = 0;
                s.DefaultColor = 7;
                s.Export = true;
                s.Frames = spriteFrames.ToByte();
                s.GraphicMode = spriteMode;
                s.Height = spriteHeight;
                s.Id = 0;
                s.Masked = false;
                s.Name = spriteName;
                s.Palette = ServiceLayer.GetPalette(spriteMode);
                s.Patterns = new List<Pattern>();
                s.Width = spriteWidth;

                int numAttr = (spriteWidth / 8) * (spriteHeight / 8);
                var attrs = new AttributeColor[numAttr];
                for (int n = 0; n < numAttr; n++)
                {
                    var attr = new AttributeColor();
                    attr.Paper = 7;
                    attr.Ink = 0;
                    attr.Bright = false;
                    attr.Flash = false;
                    attrs[n] = attr;
                }

                pnlPreview.Children.Clear();
                for (int n = 0; n < spriteFrames; n++)
                {
                    var pattern = new Pattern();
                    pattern.Attributes = new AttributeColor[(spriteWidth / 8) * (spriteHeight / 8)];
                    pattern.Id = n;
                    pattern.Name = spriteName;
                    pattern.Number = n.ToString();
                    pattern.RawData = new int[spriteWidth * spriteHeight];

                    int dir = 0;
                    int ys = cnvSource.OffsetY;
                    for (int y = 0; y < spriteHeight; y++)
                    {
                        int xs = cnvSource.OffsetX;
                        xs += (spriteWidth + spriteMargin) * n;
                        for (int x = 0; x < spriteWidth; x++)
                        {
                            if (xs >= 0 && xs < imgData.Width &&
                                ys > 0 && ys < imgData.Height)
                            {
                                var c = imgData[xs, ys];
                                var idxAttr = GetColor(c.R, c.G, c.B, s.Palette);
                                pattern.RawData[dir] = idxAttr;
                            }
                            else
                            {
                                pattern.RawData[dir] = 0;
                            }
                            dir++;
                            xs++;
                        }
                        ys++;
                    }
                    s.Patterns.Add(pattern);

                    var prev = new ZXSpriteImage();
                    var img = new Avalonia.Controls.Image();
                    img.Width = spriteWidth * spriteZoom;
                    img.Height = spriteHeight * spriteZoom;
                    img.Source = prev;
                    pnlPreview.Children.Add(img);
                    prev.RenderSprite(s, n);
                }
                sprite = s;
            }
            catch (Exception ex)
            {

            }
        }


        private int GetColor(byte r, byte g, byte b, PaletteColor[] palette)
        {
            PaletteColor targetColor = new PaletteColor()
            {
                Blue = b,
                Green = g,
                Red = r
            };
            var palColor = palette.OrderBy(c => GetColorDistance(c, targetColor)).First();
            for (int n = 0; n < palette.Count(); n++)
            {
                var p = palette[n];
                if (palColor.Red == p.Red &&
                    palColor.Green == p.Green &&
                    palColor.Blue == p.Blue)
                {
                    return n;
                }
            }
            return 0;
        }


        private static double GetColorDistance(PaletteColor c1, PaletteColor c2)
        {
            int rDiff = c1.Red - c2.Red;
            int gDiff = c1.Green - c2.Green;
            int bDiff = c1.Blue - c2.Blue;
            return Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
        }


        private void GetProperties()
        {
            spriteName = txtName.Text;
            spriteWidth = txtWidth.Text.ToInteger();
            spriteHeight = txtHeight.Text.ToInteger();
            spriteFrames = txtFrames.Text.ToInteger();
            spriteMargin = txtMargin.Text.ToInteger();
        }

        private void CmbMode_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            btnFile.Focus();
        }


        private void ChangeMode(GraphicsModes oldMode, GraphicsModes newMode)
        {
            btnFile.Focus();
        }


        private void TxtWidth_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            btnFile.Focus();
        }

        private void TxtHeight_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            btnFile.Focus();
        }

        private void TxtFrames_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            btnFile.Focus();
        }

        #endregion


        #region Buttons

        private void BtnImport_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            Import();
        }

        private void BtnCancel_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        #endregion


        #region Import

        private async void Import()
        {
            GetProperties();

            if (string.IsNullOrEmpty(spriteName))
            {
                var box = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
                {
                    ButtonDefinitions = ButtonEnum.Ok,
                    ContentTitle = "Parameter ERROR",
                    ContentMessage = "You must specify a name for the imported sprite.",
                    Icon = MsBox.Avalonia.Enums.Icon.Warning,
                    WindowIcon = ((Window)this.VisualRoot).Icon,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                });
                box.ShowAsPopupAsync(this);
                return;
            }

            // Check if name exists
            var spr = sprites.FirstOrDefault(d => d!=null && d.Name == spriteName);
            if (spr == null)
            {
                if (sprites.Count() > 1)
                {
                    sprite.Id = sprites.Where(d => d != null).Max(d => d.Id) + 1;
                }
                else
                {
                    sprite.Id = 1;
                }
                CallBackCommand?.Invoke(sprite, "ADD");
            }
            else
            {
                if (!await this.ShowConfirm("Confirm overwrite", $"There is already a sprite with the name \"{spriteName}\".\r\nAre you sure you want to overwrite it?"))
                {
                    return;
                }
                sprite.Id = spr.Id;
                CallBackCommand?.Invoke(sprite, "UPDATE");
            }
            this.Close();
            this.Dispose();
        }

        #endregion
    }
}
