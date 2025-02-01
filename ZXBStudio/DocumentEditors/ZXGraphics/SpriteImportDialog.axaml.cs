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
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using System.Formats.Tar;

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
        private int cutOff = 5;
        private bool exportInOneSprite = true;
        private DispatcherTimer tmr = null;
        private bool resetFocus = false;


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
            sldCutOff.PropertyChanged += sldCutOff_PropertyChanged;

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

        #region Image source

        private async void BtnFile_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            var fileTypes = new FilePickerFileType[4];
            fileTypes[0] = new FilePickerFileType("All files") { Patterns = new[] { "*", "*.*" } };
            fileTypes[1] = new FilePickerFileType("BMP files") { Patterns = new[] { "*.bmp" } };
            fileTypes[2] = new FilePickerFileType("JPG files") { Patterns = new[] { "*.jpg", "*.jpeg" } };
            fileTypes[3] = new FilePickerFileType("PNG files") { Patterns = new[] { "*.png" } };
            /*fileTypes[4] = new FilePickerFileType("SCR Spectrum screen files") { Patterns = new[] { "*.scr" } };*/

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


        /// <summary>
        /// CutOff changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sldCutOff_PropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
        {
            cutOff = (int)sldCutOff.Value;
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
            if (resetFocus)
            {
                btnFile.Focus();
                resetFocus = false;
            }
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
                var attrsDefault = new AttributeColor[numAttr];

                for (int n = 0; n < numAttr; n++)
                {
                    var attr = new AttributeColor();
                    attr.Paper = 7;
                    attr.Ink = 0;
                    attr.Bright = false;
                    attr.Flash = false;
                    attrsDefault[n] = attr;
                }

                var w = scrPreview.Bounds.Width;
                var h = scrPreview.Bounds.Width;
                double ix = (spriteWidth + 2) * 4;
                double iy = (spriteHeight + 2) * 4;
                int sw = spriteWidth / 8;
                int sh = spriteHeight / 8;
                int px = 0;
                int py = 0;
                int xs = 0;
                int ys = 0;
                int paper = -1;
                int ink = -1;
                int prevX = 0;
                int prevY = 0;
                int anchoPreview = 0;
                int altoPreview = 0;

                pnlPreview.Children.Clear();

                for (int n = 0; n < spriteFrames; n++)
                {
                    var attrs = attrsDefault.Clonar<AttributeColor[]>();
                    var pattern = new Pattern();
                    pattern.Attributes = attrs; // new AttributeColor[(spriteWidth / 8) * (spriteHeight / 8)];
                    pattern.Id = n;
                    pattern.Name = spriteName;
                    pattern.Number = n.ToString();
                    pattern.RawData = new int[spriteWidth * spriteHeight];

                    int dir = 0;
                    for (int cy = 0; cy < sh; cy++)
                    {
                        py = cy * 8;
                        for (int cx = 0; cx < sw; cx++)
                        {
                            ys = cnvSource.OffsetY + py;
                            px = cx * 8;
                            paper = -1;
                            ink = -1;
                            for (int y = 0; y < 8; y++)
                            {
                                xs = (cnvSource.OffsetX + (n * spriteWidth)) + px;
                                dir = (((cy * 8) + y) * spriteWidth) + (cx * 8);
                                for (int x = 0; x < 8; x++)
                                {
                                    if (xs >= 0 && xs < imgData.Width &&
                                        ys >= 0 && ys < imgData.Height)
                                    {
                                        var c = imgData[xs, ys];
                                        var idxAttr = GetColor(c.R, c.G, c.B, s.Palette);
                                        if (spriteMode == GraphicsModes.ZXSpectrum)
                                        {
                                            // Fijar el color
                                            var dirAttr = (cy * sw) + cx;
                                            var attr = pattern.Attributes[dirAttr];
                                            attr.Bright = attr.Bright | (idxAttr > 7);
                                            byte cCol = (byte)(idxAttr & 0b111);
                                            if (paper == -1)
                                            {
                                                attr.Paper = cCol;
                                                paper = cCol;
                                            }
                                            else if (ink == -1 && paper != cCol)
                                            {
                                                attr.Ink = cCol;
                                                ink = cCol;
                                            }
                                            if (cCol == paper)
                                            {
                                                idxAttr = 0;
                                            }
                                            else
                                            {
                                                idxAttr = 7;
                                            }
                                        }
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
                        }
                    }
                    s.Patterns.Add(pattern);

                    var prev = new ZXSpriteImage();
                    var img = new Avalonia.Controls.Image();
                    img.Width = spriteWidth * spriteZoom;
                    img.Height = spriteHeight * spriteZoom;
                    img.Source = prev;
                    pnlPreview.Children.Add(img);

                    Canvas.SetLeft(img, prevX);
                    Canvas.SetTop(img, prevY);
                    prevX += (spriteWidth * 4);

                    if (anchoPreview < prevX)
                    {
                        anchoPreview = prevX;
                    }
                    if ((prevX + sw) > w)
                    {
                        prevY += (spriteHeight * 4);
                        prevX = 0;
                        if (altoPreview < prevY)
                        {
                            altoPreview = prevY;
                        }
                    }

                    prev.RenderSprite(s, n);

                }
                sprite = s;
                pnlPreview.Width = anchoPreview + (spriteWidth * 4);
                pnlPreview.Height = altoPreview + (spriteHeight * 4);
            }
            catch (Exception ex)
            {

            }
        }


        private int GetColor(byte r, byte g, byte b, PaletteColor[] palette)
        {
            if (r > 0)
            {

            }
            byte cr = GetColor_CutOff(r);
            byte cg = GetColor_CutOff(g);
            byte cb = GetColor_CutOff(b);
            PaletteColor targetColor = new PaletteColor()
            {
                Blue = cb,
                Green = cg,
                Red = cr
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


        private byte GetColor_CutOff(byte c)
        {
            if (cutOff == 5)
            {
                return c;
            }

            if (cutOff < 5)
            {
                int ic = c - (25 * (5 - cutOff));
                if (ic < 0)
                {
                    return 0;
                }
                else
                {
                    return ic.ToByte();
                }
            }
            else
            {
                int ic = c + (25 * (cutOff - 5));
                if (ic > 255)
                {
                    return 255;
                }
                else
                {
                    return ic.ToByte();
                }
            }
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
            spriteMode = (GraphicsModes)cmbMode.SelectedIndex;
            exportInOneSprite = chkFrames.IsChecked == true;
        }

        private void CmbMode_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            resetFocus = true;
            //btnFile.Focus();
        }


        private void ChangeMode(GraphicsModes oldMode, GraphicsModes newMode)
        {
            resetFocus = true;
        }


        private void TxtWidth_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            resetFocus = true;
            cnvSource.SpriteWidth = txtWidth.Text.ToInteger();
            cnvSource.Refresh();
        }

        private void TxtHeight_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            resetFocus = true;
            cnvSource.SpriteHeight = txtHeight.Text.ToInteger();
            cnvSource.Refresh();
        }

        private void TxtFrames_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            resetFocus = true;
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
            try
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

                if (exportInOneSprite)
                {
                    // Check if name exists
                    var spr = sprites.FirstOrDefault(d => d != null && d.Name == spriteName);
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
                }
                else
                {
                    string sprName = spriteName;
                    // Check for duplicates
                    {
                        var spr = sprites.FirstOrDefault(d => d != null && d.Name == spriteName);
                        if (spr != null)
                        {
                            if (!await this.ShowConfirm("Confirm overwrite", $"There is already a sprite with the name \"{spriteName}\".\r\nAre you sure you want to overwrite it?"))
                            {
                                return;
                            }
                        }
                        else
                        {
                            spr = sprites.FirstOrDefault(d => d != null && d.Name == spriteName + "_0");
                            if (spr != null)
                            {
                                if (!await this.ShowConfirm("Confirm overwrite", $"There is already a sprite with the name \"{spriteName}_0\".\r\nAre you sure you want to overwrite it?"))
                                {
                                    return;
                                }
                            }
                        }
                    }

                    for (int n = 0; n < sprite.Patterns.Count(); n++)
                    {
                        var spr = sprite.Clonar<Sprite>();
                        spr.Patterns = spr.Patterns.Skip(n).Take(1).ToList();
                        spr.Frames = 1;
                        spr.Name = sprName + "_" + n.ToString();
                        var spr2 = sprites.FirstOrDefault(d => d != null && d.Name == spr.Name);
                        if (spr2 == null)
                        {
                            var id = 0;
                            if (sprites.Count() > 0 && sprites.ElementAt(0) != null)
                            {
                                id = sprites.Where(d => d != null).Max(d => d.Id) + 1;
                            }
                            spr.Id = id;
                            CallBackCommand?.Invoke(spr, "ADD");
                        }
                        else
                        {
                            spr.Id = spr2.Id;
                            CallBackCommand?.Invoke(sprite, "UPDATE");
                        }
                    }
                }
                this.Close();
                this.Dispose();
            }
            catch (Exception ex)
            {
                this.ShowError("ERROR importing image", ex.Message + ex.StackTrace);
            }
        }

        #endregion
    }
}
