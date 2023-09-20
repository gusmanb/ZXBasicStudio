using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using ZXBasicStudio.Common;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    public partial class SpritePreviewControl : UserControl
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
                ApplySettings(false);
            }
        }

        /// <summary>
        /// CallBack for commands: "ADD", "CLONE", "DELETE", "SELECT", "MODE", "SIZE"
        /// </summary>
        public Action<SpritePreviewControl, string> CallBackCommand { get; set; }

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

        /// <summary>
        /// The settings has changed
        /// </summary>
        public bool SettingsChanged
        {
            get
            {
                return _SettingsChanged;
            }
            set
            {
                _SettingsChanged = value;
                RefreshButtons();
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

        public SpritePreviewControl()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Initializes the control
        /// </summary>
        /// <param name="spriteData">Data of the sprite, if is null, the "Add" icon is visible and no properties are shown</param>
        /// <param name="callBackCommand">CallBak for actions command, line "ADD", "CLONE", "DELETE" or "SELECTED"</param>
        /// <returns></returns>
        public bool Initialize(Sprite spriteData, Action<SpritePreviewControl, string> callBackCommand)
        {
            this.SpriteData = spriteData;
            this.CallBackCommand = callBackCommand;

            this.PointerPressed += SpritePropertiesControl_PointerPressed;

            btnApply.Tapped += BtnApply_Tapped;
            btnCancel.Tapped += BtnCancel_Tapped;
            btnClone.Tapped += BtnClone_Tapped;
            btnDelete.Tapped += BtnDelete_Tapped;
            btnNew.Tapped += BtnNew_Tapped;

            txtFrames.ValueChanged += TxtFrames_ValueChanged;
            txtHeight.ValueChanged += TxtHeight_ValueChanged;
            txtName.TextChanged += TxtName_TextChanged;
            txtWidth.ValueChanged += TxtWidth_ValueChanged;
            chkExport.IsCheckedChanged += ChkExport_IsCheckedChanged;
            chkMasked.IsCheckedChanged += ChkMasked_IsCheckedChanged;
            cmbMode.SelectionChanged += CmbMode_SelectionChanged;

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
                RefreshButtons();

                if (SpriteData == null)
                {
                    pnlNew.IsVisible = true;
                    pnlProperties.IsVisible = false;
                    pnlPreview.IsVisible = false;
                    return;
                }

                pnlNew.IsVisible = false;
                if (_IsSelected)
                {
                    pnlProperties.IsVisible = true;
                    pnlPreview.IsVisible = false;

                    txtFrames.Value = SpriteData.Frames;
                    txtHeight.Value = SpriteData.Height;
                    txtId.Text = SpriteData.Id == -1 ? "---" : SpriteData.Id.ToString();
                    txtName.Text = SpriteData.Name;
                    txtWidth.Value = SpriteData.Width;
                    chkMasked.IsCancel = SpriteData.Masked;
                    cmbMode.SelectedIndex = (byte)SpriteData.GraphicMode;
                }
                else
                {
                    pnlProperties.IsVisible = false;
                    pnlPreview.IsVisible = true;
                    lblName.Text = SpriteData.Name;
                }

                if(SpriteData.Patterns==null || SpriteData.Patterns.Count == 0)
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
                if(SpriteData.Palette==null || SpriteData.Palette.Length == 0)
                {
                    SpriteData.Palette = ServiceLayer.GetPalette(SpriteData.GraphicMode);
                }

                for (int y = 0; y < SpriteData.Height; y++)
                {
                    for (int x = 0; x < SpriteData.Width; x++)
                    {
                        int colorIndex = 0;

                        var frame = SpriteData.Patterns[SpriteData.CurrentFrame];
                        var p = frame.Data.FirstOrDefault(d => d.X == x && d.Y == y);
                        if (p != null)
                        {
                            colorIndex = p.ColorIndex;
                        }

                        var r = new Rectangle();
                        r.Width = 4;
                        r.Height = 4;

                        var palette = SpriteData.Palette[colorIndex];
                        r.Fill = new SolidColorBrush(new Color(255, palette.Red, palette.Green, palette.Blue));
                        var r2 = r.Clonar<Rectangle>();

                        cnvPreview.Children.Add(r);
                        Canvas.SetTop(r, y * 4);
                        Canvas.SetLeft(r, x * 4);

                        cnvPoints.Children.Add(r2);
                        Canvas.SetTop(r2, y * 4);
                        Canvas.SetLeft(r2, x * 4);
                    }
                }
            }
            catch { }
            finally
            {
                refreshing = false;
            }
        }


        private void RefreshButtons()
        {
            btnApply.IsVisible = _SettingsChanged;
            btnCancel.IsVisible = _SettingsChanged;
            btnClone.IsVisible = !newSprite;
            btnDelete.IsVisible = !_SettingsChanged;
        }


        public void Select()
        {
            _IsSelected = true;
            Refresh();
            CallBackCommand(this, "SELECTED");
        }


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
                    if(!ServiceLayer.SpriteData_ChangeMasked(ref sp, SpriteData.Masked))
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
                CallBackCommand(this, "UPDATE");
                Refresh();
                newSprite = false;
                SettingsChanged = false;
            }
        }

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
                Data = new PointData[64]
            };
            int dir = 0;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
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


        #endregion


        #region Button events

        private void BtnNew_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            AddNew();
        }


        private void SpritePropertiesControl_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Select();
        }


        private void ChkExport_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SpriteData.Export = chkExport.IsChecked.ToBoolean();
        }


        private void ChkMasked_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SettingsChanged = true;
        }


        private void CmbMode_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            SettingsChanged = true;
        }


        private void TxtWidth_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            SettingsChanged = true;
        }

        private void TxtHeight_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            SettingsChanged = true;
        }

        private void TxtName_TextChanged(object? sender, TextChangedEventArgs e)
        {
            SpriteData.Name = txtName.Text;
        }

        private void TxtFrames_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            SettingsChanged = true;
        }


        private void BtnDelete_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            CallBackCommand(this, "DELETE");
        }


        private void BtnClone_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            CallBackCommand(this, "CLONE");
        }


        private void BtnCancel_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            Refresh();
            SettingsChanged = false;
        }


        private void BtnApply_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            ApplySettings(true);
        }

        #endregion

    }
}
