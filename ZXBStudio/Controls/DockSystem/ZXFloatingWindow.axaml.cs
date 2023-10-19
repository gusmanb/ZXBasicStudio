using Avalonia;
using Avalonia.Controls;
using System;
using System.Diagnostics;

namespace ZXBasicStudio.Controls.DockSystem
{
    public partial class ZXFloatingWindow : Window
    {
        public event EventHandler DockingControlsChanged;
        public IZXDockingContainer DockingContainer { get { return mainDock; } }

        PixelPoint lastGood = new PixelPoint();
        public PixelPoint LastGoodPosition { get { return lastGood; } }

        public ZXFloatingWindow()
        {
            InitializeComponent();
            DockingContainer.DockingControlsChanged += DockingContainer_DockingControlsChanged;
            this.PositionChanged += ZXFloatingWindow_PositionChanged;
            this.SizeChanged += ZXFloatingWindow_SizeChanged;
        }

        private void ZXFloatingWindow_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            PixelRect rectBounds = new PixelRect(this.Position, new PixelSize((int)this.Width, (int)this.Height));

            var screen = this.Screens.ScreenFromWindow(this);

            if (screen == null)
                return;

            if (screen.WorkingArea.Intersects(rectBounds))
                lastGood = this.Position;
            else
                Debug.Print("NO INTERSECT");
        }

        private void ZXFloatingWindow_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            PixelRect rectBounds = new PixelRect(this.Position, new PixelSize((int)this.Width, (int)this.Height));

            var screen = this.Screens.ScreenFromWindow(this);

            if (screen == null)
                return;

            if (screen.WorkingArea.Intersects(rectBounds))
                lastGood = this.Position;
            else
                Debug.Print("NO INTERSECT");
        }

        private void DockingContainer_DockingControlsChanged(object? sender, EventArgs e)
        {
            if(DockingControlsChanged != null)
                DockingControlsChanged(this, e);
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
        }
    }
}
