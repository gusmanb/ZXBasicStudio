using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ZXBasicStudio.Common;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    public partial class SpritePatternControl : UserControl
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
        public Action<SpritePatternControl, string> CallBackCommand { get; set; }

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
                if (_IsSelected != value)
                {
                    _IsSelected = value;
                    Refresh();
                }
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

        public SpritePatternControl()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Initializes the control
        /// </summary>
        /// <param name="spriteData">Data of the sprite, if is null, the "Add" icon is visible and no properties are shown</param>
        /// <param name="callBackCommand">CallBak for actions command, line "ADD", "CLONE", "DELETE" or "SELECTED"</param>
        /// <returns></returns>
        public bool Initialize(Sprite spriteData, Action<SpritePatternControl, string> callBackCommand)
        {
            this.SpriteData = spriteData;
            this.CallBackCommand = callBackCommand;

            this.PointerPressed += SpritePropertiesControl_PointerPressed;

            btnNew.Tapped += BtnNew_Tapped;

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
                    pnlPreview.IsVisible = false;
                    return;
                }

                if (_IsSelected)
                {
                    brdMain.BorderBrush = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    brdMain.BorderBrush = new SolidColorBrush(Colors.Gray);
                }

                pnlNew.IsVisible = false;

                pnlPreview.IsVisible = true;
                lblName.Text = SpriteData.Name;

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

                // Delete background
                {
                    var r = new Rectangle();
                    r.Width = cnvPreview.Width;
                    r.Height = cnvPreview.Height;
                    r.Fill = new SolidColorBrush(new Color(255, 0x28, 0x28, 0x28));
                    cnvPreview.Children.Add(r);
                    Canvas.SetTop(r,0);
                    Canvas.SetLeft(r,0);
                }

                cnvPreview.Width = SpriteData.Width * 4;
                cnvPreview.Height = SpriteData.Height * 4;

                cnvPreview.Children.Clear();
                for (int y = 0; y < SpriteData.Height; y++)
                {
                    for (int x = 0; x < SpriteData.Width; x++)
                    {
                        int colorIndex = 0;

                        var frame = SpriteData.Patterns[0]; // SpriteData.CurrentFrame];
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
                        cnvPreview.Children.Add(r);
                        Canvas.SetTop(r, y * 4);
                        Canvas.SetLeft(r, x * 4);
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
                //CallBackCommand?.Invoke(this, "UPDATE");
                //Refresh();
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

        #endregion

    }
}
