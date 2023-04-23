using Avalonia.Controls;
using System;

namespace ZXBasicStudio.Controls.DockSystem
{
    public partial class ZXFloatingWindow : Window
    {
        public event EventHandler DockingControlsChanged;
        public IZXDockingContainer DockingContainer { get { return mainDock; } }
        public ZXFloatingWindow()
        {
            InitializeComponent();
            DockingContainer.DockingControlsChanged += DockingContainer_DockingControlsChanged;
        }

        private void DockingContainer_DockingControlsChanged(object? sender, EventArgs e)
        {
            if(DockingControlsChanged != null)
                DockingControlsChanged(this, e);
        }
    }
}
