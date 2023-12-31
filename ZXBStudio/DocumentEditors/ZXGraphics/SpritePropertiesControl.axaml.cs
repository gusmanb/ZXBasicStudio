using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using ZXBasicStudio.Common;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    public partial class SpritePropertiesControl : UserControl
    {
        #region Public properties

        /// <summary>
        /// Sprite data
        /// </summary>
        public Sprite SpriteData
        {
            get
            {
                return _SpriteData;
            }
            set
            {
                _SpriteData = value;
                IsSelected = true;
                //ApplySettings(false);
            }
        }

        /// <summary>
        /// CallBack for commands: "ADD", "CLONE", "DELETE", "SELECT", "MODE", "SIZE"
        /// </summary>
        public Action<SpritePropertiesControl, string> CallBackCommand { get; set; }

        /// <summary>
        /// True when then control is selected
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                _IsSelected = value;
                Refresh();
            }
        }


        public int PrimaryColor = 1;
        public int SecondaryColor = 0;

        #endregion


        #region Private fields

        private bool _IsSelected = false;
        private bool _SettingsChanged = false;
        private bool refreshing = false;
        private bool newSprite = true;
        private Sprite _SpriteData = null;

        #endregion


        #region Constructor and public methods

        public SpritePropertiesControl()
        {
            InitializeComponent();
            Refresh();
        }


        /// <summary>
        /// Initializes the control
        /// </summary>
        /// <param name="spriteData">Data of the sprite, if is null, the "Add" icon is visible and no properties are shown</param>
        /// <param name="callBackCommand">CallBak for actions command, line "ADD", "CLONE", "DELETE" or "SELECTED"</param>
        /// <returns></returns>
        public bool Initialize(Sprite spriteData, Action<SpritePropertiesControl, string> callBackCommand)
        {
            this.SpriteData = spriteData;
            this.CallBackCommand = callBackCommand;

            btnInsert.Tapped += BtnInsert_Tapped;
            btnClone.Tapped += BtnClone_Tapped;
            btnDelete.Tapped += BtnDelete_Tapped;

            txtName.TextChanged += TxtName_TextChanged;
            cmbMode.SelectionChanged += CmbMode_SelectionChanged;
            txtWidth.ValueChanged += TxtWidth_ValueChanged;
            txtHeight.ValueChanged += TxtHeight_ValueChanged;
            txtFrames.ValueChanged += TxtFrames_ValueChanged;
            chkMasked.IsCheckedChanged += ChkMasked_IsCheckedChanged;
            chkExport.IsCheckedChanged += ChkExport_IsCheckedChanged;

            _SettingsChanged = false;
            newSprite = true;

            Select();

            return true;
        }


        public void Refresh()
        {
            if (refreshing)
            {
                return;
            }
            refreshing = true;

            try
            {
                if (SpriteData == null)
                {
                    pnlProperties.IsVisible = false;
                    return;
                }
                else
                {
                    pnlProperties.IsVisible = true;
                }

                txtFrames.Value = SpriteData.Frames;
                txtHeight.Value = SpriteData.Height;
                txtId.Text = SpriteData.Id == -1 ? "---" : SpriteData.Id.ToString();
                txtName.Text = SpriteData.Name;
                txtWidth.Value = SpriteData.Width;
                chkMasked.IsChecked = SpriteData.Masked;
                cmbMode.SelectedIndex = (byte)SpriteData.GraphicMode;
                chkExport.IsChecked = SpriteData.Export;

                if (SpriteData.Patterns == null || SpriteData.Patterns.Count == 0)
                {
                    SpriteData.Patterns = new List<Pattern>();
                    SpriteData.Patterns.Add(new Pattern()
                    {
                        Data = null,
                        Id = 0,
                        Name = "",
                        Number = "",
                        RawData = new int[64]
                    });
                }
                if (SpriteData.Palette == null || SpriteData.Palette.Length == 0)
                {
                    SpriteData.Palette = ServiceLayer.GetPalette(SpriteData.GraphicMode);
                }
            }
            catch { }
            finally
            {
                refreshing = false;
            }
        }


        public void Select()
        {
            _IsSelected = true;
            Refresh();
            CallBackCommand(this, "SELECTED");
        }

        #endregion


        #region Private methods

        private Pattern CreatePattern()
        {
            var pat = new Pattern()
            {
                Id = 0,
                Name = "",
                Number = "0",
                Data = null,
                RawData = new int[SpriteData.Width * SpriteData.Height],
                Attributes = new AttributeColor[(SpriteData.Width * SpriteData.Height) / 8]
            };
            for (int n = 0; n < pat.Attributes.Length; n++)
            {
                pat.Attributes[n] = new AttributeColor()
                {
                    Paper = SecondaryColor,
                    Ink = PrimaryColor
                };
            }
            return pat;
        }


        private void ResizePattern(int oldWidth, int oldHeight)
        {
            var patterns = new List<Pattern>();
            foreach (var pattern in SpriteData.Patterns)
            {
                var pat = new Pattern()
                {
                    Id = pattern.Id,
                    Name = pattern.Name,
                    Number = pattern.Number,
                    RawData = new int[SpriteData.Width * SpriteData.Height],
                    Attributes = new AttributeColor[(SpriteData.Width / 8) * (SpriteData.Height / 8)]
                };
                for (int n = 0; n < pat.Attributes.Length; n++)
                {
                    pat.Attributes[n] = new AttributeColor()
                    {
                        Paper = SecondaryColor,
                        Ink = PrimaryColor
                    };
                }

                for (int y = 0; y < SpriteData.Height; y++)
                {
                    for (int x = 0; x < SpriteData.Width; x++)
                    {
                        int dir = (y * SpriteData.Width) + x;
                        int colorIndex = 0;
                        if (x < oldWidth && y < oldHeight)
                        {
                            colorIndex = pattern.RawData[(y * oldWidth) + x];
                        }

                        pat.RawData[(y * SpriteData.Width) + x] = colorIndex;
                    }
                }

                patterns.Add(pat);
            }
            SpriteData.Patterns = patterns;
            CallBackCommand(this, "REFRESH");
        }

        #endregion


        #region Button events


        private void ChkMasked_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var value = chkMasked.IsChecked.ToBoolean();
            if (SpriteData.Masked != value)
            {
                SpriteData.Masked = value;
                Refresh();
                CallBackCommand(this, "REFRESH");
            }
        }


        private void ChkExport_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var value = chkExport.IsChecked.ToBoolean();
            if (SpriteData.Export != value)
            {
                SpriteData.Export = value;
            }
        }


        private void CmbMode_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var value = (GraphicsModes)cmbMode.SelectedIndex;
            if (SpriteData.GraphicMode != value)
            {
                ChangeMode(SpriteData.GraphicMode, value);
                SpriteData.GraphicMode = value;
                SpriteData.Palette = ServiceLayer.GetPalette(SpriteData.GraphicMode);
                Refresh();
                CallBackCommand(this, "CHANGEMODE");
            }
        }


        private void ChangeMode(GraphicsModes oldMode, GraphicsModes newMode)
        {
            if (oldMode == newMode)
            {
                return;
            }
            if (oldMode == GraphicsModes.Monochrome)
            {
                foreach (var frame in SpriteData.Patterns)
                {
                    if (frame.Attributes == null)
                    {
                        int cW = SpriteData.Width / 8;
                        int cH = SpriteData.Height / 8;
                        frame.Attributes = new AttributeColor[cW * cH];
                    }

                    for (int n = 0; n < frame.Attributes.Length; n++)
                    {
                        frame.Attributes[n] = new AttributeColor()
                        {
                            Ink = 0,
                            Paper = 7
                        };
                    }

                    for (int n = 0; n < frame.RawData.Length; n++)
                    {
                        var o = frame.RawData[n];
                        if (o == 0)
                        {
                            frame.RawData[n] = 0;
                        }
                        else
                        {
                            frame.RawData[n] = 7;
                        }
                    }

                }
            }
            else
            {
                foreach (var frame in SpriteData.Patterns)
                {
                    if (frame.Attributes == null)
                    {
                        int cW = SpriteData.Width / 8;
                        int cH = SpriteData.Height / 8;
                        frame.Attributes = new AttributeColor[cW * cH];
                    }
                        for (int n = 0; n < frame.Attributes.Length; n++)
                        {
                            frame.Attributes[n] = new AttributeColor()
                            {
                                Ink = 0,
                                Paper = 7
                            };
                        }
                    for (int n = 0; n < frame.RawData.Length; n++)
                    {
                        var o = frame.RawData[n];
                        if (o == 0)
                        {
                            frame.RawData[n] = 0;
                        }
                        else
                        {
                            frame.RawData[n] = 1;
                        }
                    }
                }
            }
        }


        private void TxtWidth_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            var value = txtWidth.Text.ToInteger();
            if (SpriteData.Width != value)
            {
                int oldWidth = SpriteData.Width;
                int oldHeight = SpriteData.Height;
                SpriteData.Width = value;
                ResizePattern(oldWidth, oldHeight);
                Refresh();
            }
        }

        private void TxtHeight_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            var value = txtHeight.Text.ToInteger();
            if (SpriteData.Height != value)
            {
                int oldWidth = SpriteData.Width;
                int oldHeight = SpriteData.Height;
                SpriteData.Height = value;
                ResizePattern(oldWidth, oldHeight);
                Refresh();
            }
        }

        private void TxtName_TextChanged(object? sender, TextChangedEventArgs e)
        {
            var value = txtName.Text;
            if (SpriteData.Name != value)
            {
                SpriteData.Name = value;
                Refresh();
                CallBackCommand(this, "REFRESH");
            }
        }

        private void TxtFrames_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            var value = txtFrames.Text.ToByte();
            if (SpriteData.Frames != value)
            {
                SpriteData.Frames = value;
                while (SpriteData.Patterns.Count < SpriteData.Frames)
                {
                    SpriteData.Patterns.Add(CreatePattern());
                }
                SpriteData.CurrentFrame = (byte)(SpriteData.Frames - 1);
                Refresh();
                CallBackCommand(this, "FRAMEUPDATE");
            }
        }


        private void BtnDelete_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            CallBackCommand(this, "DELETE");
        }


        private void BtnClone_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            CallBackCommand(this, "CLONE");
        }

        private void BtnInsert_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            CallBackCommand(this, "INSERT");
        }

        #endregion

    }
}
