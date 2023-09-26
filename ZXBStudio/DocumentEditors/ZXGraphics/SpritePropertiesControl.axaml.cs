using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
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
                chkExport.IsChecked= SpriteData.Export;

                if (SpriteData.Patterns == null || SpriteData.Patterns.Count == 0)
                {
                    SpriteData.Patterns = new List<Pattern>();
                    SpriteData.Patterns.Add(new Pattern()
                    {
                        Data = new PointData[0],
                        Id = 0,
                        Name = "",
                        Number = ""
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


        /*
        public void ApplySettings(bool askForApply)
        {
            if (SpriteData == null)
            {
                return;
            }
            else
            {
                var sp = SpriteData.Clonar<Sprite>();
                sp.Frames = txtFrames.Value.ToByte();
                sp.GraphicMode = (GraphicsModes)cmbMode.SelectedIndex;
                sp.Height = txtHeight.Value.ToByte();
                sp.Masked = chkMasked.IsChecked.ToBoolean();
                sp.Name = txtName.Text;
                sp.Width = txtWidth.Value.ToByte();

                if (sp.Width != SpriteData.Width || sp.Height != SpriteData.Height)
                {
                    if (!ServiceLayer.SpriteData_Resize(ref sp, SpriteData.Width, SpriteData.Height))
                    {
                        // TODO: Report error
                        return;
                    }
                }
                if (sp.GraphicMode != SpriteData.GraphicMode)
                {
                    if (!ServiceLayer.SpriteData_ChangeMode(ref sp, SpriteData.GraphicMode))
                    {
                        // TODO: Report error
                        return;
                    }
                }
                if (sp.Masked != SpriteData.Masked)
                {
                    if (!ServiceLayer.SpriteData_ChangeMasked(ref sp, SpriteData.Masked))
                    {
                        // TODO: Report error
                        return;
                    }
                }
                if (sp.Frames != SpriteData.Frames)
                {
                    if (!ServiceLayer.SpriteData_ChangeFrames(ref sp, SpriteData.Frames))
                    {
                        // TODO: Report error
                        return;
                    }
                }

                _SpriteData = sp;
                CallBackCommand?.Invoke(this, "UPDATE");
                Refresh();
                newSprite = false;
            }
        }
        */

        #endregion


        #region Private methods

        private void AddNew()
        {
            var sp = new Sprite()
            {
                CurrentFrame = 0,
                DefaultColor = 0,
                Frames = 1,
                GraphicMode = GraphicsModes.Monochrome,
                Height = 8,
                Id = -1,
                Masked = false,
                Name = "",
                Patterns = new List<Pattern>(),
                Width = 8
            };
            sp.Palette = ServiceLayer.GetPalette(sp.GraphicMode);
            sp.Patterns.Add(CreatePattern());
            SpriteData = sp;
            _IsSelected = true;
            Refresh();

            CallBackCommand?.Invoke(this, "ADD");
        }


        private Pattern CreatePattern()
        {
            var pat = new Pattern()
            {
                Id = 0,
                Name = "",
                Number = "0",
                Data = new PointData[SpriteData.Width * SpriteData.Height]
            };
            int dir = 0;
            for (int y = 0; y < SpriteData.Height; y++)
            {
                for (int x = 0; x < SpriteData.Width; x++)
                {
                    pat.Data[dir] = new PointData()
                    {
                        X = x,
                        Y = y,
                        ColorIndex = 0
                    };
                    dir++;
                }
            }
            return pat;
        }


        private void ResizePattern()
        {
            var patterns = new List<Pattern>();
            foreach (var pattern in SpriteData.Patterns)
            {
                var pat = new Pattern()
                {
                    Id = pattern.Id,
                    Name = pattern.Name,
                    Number = pattern.Number,
                    Data = new PointData[SpriteData.Width * SpriteData.Height]
                };
                int dir = 0;
                for (int y = 0; y < SpriteData.Height; y++)
                {
                    for (int x = 0; x < SpriteData.Width; x++)
                    {
                        var oldData = pattern.Data.FirstOrDefault(d => d.X == x && d.Y == y);
                        if (oldData == null)
                        {
                            pat.Data[dir] = new PointData()
                            {
                                X = x,
                                Y = y,
                                ColorIndex = 0
                            };
                        }
                        else
                        {
                            pat.Data[dir] = oldData.Clonar<PointData>();
                        }
                        dir++;
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
                SpriteData.GraphicMode = value;
                Refresh();
                CallBackCommand(this, "REFRESH");
            }
        }


        private void TxtWidth_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            var value = txtWidth.Text.ToInteger();
            if (SpriteData.Width != value)
            {
                SpriteData.Width = value;
                ResizePattern();
                Refresh();
            }
        }

        private void TxtHeight_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            var value = txtHeight.Text.ToInteger();
            if (SpriteData.Height != value)
            {
                SpriteData.Height = value;
                ResizePattern();
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
                CallBackCommand(this, "REFRESH");
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

        #endregion

    }
}
