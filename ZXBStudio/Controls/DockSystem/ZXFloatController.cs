using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Controls.DockSystem
{
    public static class ZXFloatController
    {
        static List<ZXFloatingWindow> windows = new List<ZXFloatingWindow>();

        public static IEnumerable<ZXFloatingWindow> Windows { get { return windows.ToArray(); } }

        private static bool destroy;

        public static ZXFloatingWindow MakeFloating(ZXDockingControl Control)
        {
            ZXFloatingWindow window = new ZXFloatingWindow();
            var size = Control.Bounds.Size;
            window.Width = size.Width;
            window.Height = size.Height;
            window.Closing += Window_Closing;
            window.Closed += Window_Closed;
            window.DockingControlsChanged += Window_DockingControlsChanged;
            window.Show();
            window.DockingContainer.AddToEnd(Control);
            windows.Add(window);
            return window;
        }

        private static void Window_DockingControlsChanged(object? sender, EventArgs e)
        {
            var window = sender as ZXFloatingWindow;

            if (window == null)
                return;

            if (window.DockingContainer.DockingControls.Count() == 0)
                window.Close();
        }

        private static void Window_Closed(object? sender, EventArgs e)
        {
            if (destroy)
                return;

            var window = sender as ZXFloatingWindow;

            if (window == null)
                return;

            windows.Remove(window);
        }

        internal static void Window_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
        {
            if (destroy)
                return;

            var window = sender as ZXFloatingWindow;

            if (window == null)
                return;

            var controls = window.DockingContainer.DockingControls;

            if (controls.Count() == 0)
                return;

            if (controls.All(c => c.CanClose))
                return;

            e.Cancel = true;
        }

        public static void Dispose()
        {
            destroy = true;
            foreach (var window in Windows)
                window.Close();
        }
    }
}
