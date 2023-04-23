using Avalonia.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.Controls.DockSystem;

namespace ZXBasicStudio.Classes
{
    public static class ZXLayoutPersister
    {
        const string LayoutFile = "ZXBasicStudioLayout.json";

        public static void ResetLayout()
        {
            try
            {
                if (File.Exists(LayoutFile))
                    File.Delete(LayoutFile);
            }
            catch { }
        }

        public static void SaveLayout(Grid MainGrid, ZXDockingContainer LeftDock, ZXDockingContainer RightDock, ZXTabDockingContainer BottomDock)
        {
            try
            {
                if (Avalonia.Controls.Design.IsDesignMode)
                    return;

                var windows = ZXFloatController.Windows.Select(w => new ZXLayoutWindow 
                { 
                    Top = w.Position.Y,
                    Left = w.Position.X,
                    Width = w.ClientSize.Width,
                    Height = w.ClientSize.Height,
                    DockedControls = w.DockingContainer.DockingControls
                    .Where(c => c is ZXDockingControl && c.Name != null)
                    .Select(c => c.Name ?? "")
                    .ToArray() ?? new string[0]
                });

                ZXLayout layout = new ZXLayout
                {
                    MainRows = MainGrid.RowDefinitions.ToString(),
                    MainColumns = MainGrid.ColumnDefinitions.ToString(),
                    LeftDockRows = LeftDock.RowDefinitions.ToString(),
                    RightDockRows = RightDock.RowDefinitions.ToString(),
                    LeftDockedControls = LeftDock.Children?
                    .OrderBy(c => c.GetValue(Grid.RowProperty))
                    .Where(c => c is ZXDockingControl && c.Name != null)
                    .Select(c => c.Name ?? "")
                    .ToArray() ?? new string[0],
                    RightDockedControls = RightDock.Children?
                    .OrderBy(c => c.GetValue(Grid.RowProperty))
                    .Where(c => c is ZXDockingControl && c.Name != null)
                    .Select(c => c.Name ?? "")
                    .ToArray() ?? new string[0],
                    BottomDockedControls = BottomDock.DockedControls?
                    .Where(c => c is ZXDockingControl && c.Name != null)
                    .Select(c => c.Name ?? "")
                    .ToArray() ?? new string[0],
                    FloatingWindows = windows.ToArray()
                };

                var content = JsonConvert.SerializeObject(layout, Formatting.Indented);

                File.WriteAllText(LayoutFile, content);

                ZXFloatController.Dispose();
            }
            catch { }
        }

        public static void RestoreLayout(Grid MainGrid, ZXDockingContainer LeftDock, ZXDockingContainer RightDock, ZXTabDockingContainer BottomDock)
        {
            try
            {
                if (Avalonia.Controls.Design.IsDesignMode)
                    return;

                if (!File.Exists(LayoutFile))
                    return;

                ZXLayout? layout = JsonConvert.DeserializeObject<ZXLayout>(File.ReadAllText(LayoutFile));

                if (layout == null)
                    return;

                MainGrid.RowDefinitions = new RowDefinitions(layout.MainRows);
                MainGrid.ColumnDefinitions = new ColumnDefinitions(layout.MainColumns);

                List<ZXDockingControl> controls = new List<ZXDockingControl>();

                var leftControls = LeftDock.Children?.Where(c => c is ZXDockingControl).Cast<ZXDockingControl>().ToArray();
                if (leftControls != null)
                    controls.AddRange(leftControls);
                LeftDock.Children?.Clear();


                var rightControls = RightDock.Children?.Where(c => c is ZXDockingControl).Cast<ZXDockingControl>().ToArray();
                if (rightControls != null)
                    controls.AddRange(rightControls);
                RightDock.Children?.Clear();

                var bottomControls = BottomDock.DockedControls?.Where(c => c is ZXDockingControl).Cast<ZXDockingControl>().ToArray();
                if (bottomControls != null)
                {
                    controls.AddRange(bottomControls);
                    foreach (var ctl in bottomControls)
                        BottomDock.Remove(ctl);
                }

                foreach (var ctrl in layout.LeftDockedControls)
                {
                    var contrl = controls.FirstOrDefault(c => c.Name == ctrl);
                    if (contrl != null)
                        LeftDock.AddToEnd(contrl);
                }

                LeftDock.RowDefinitions = new RowDefinitions(layout.LeftDockRows);

                foreach (var ctrl in layout.RightDockedControls)
                {
                    var contrl = controls.FirstOrDefault(c => c.Name == ctrl);
                    if (contrl != null)
                        RightDock.AddToEnd(contrl);
                }

                RightDock.RowDefinitions = new RowDefinitions(layout.RightDockRows);

                foreach (var ctrl in layout.BottomDockedControls)
                {
                    var contrl = controls.FirstOrDefault(c => c.Name == ctrl);
                    if (contrl != null)
                        BottomDock.AddToEnd(contrl);
                }

                foreach (var window in layout.FloatingWindows)
                {
                    var firstControl = controls.FirstOrDefault(c => c.Name == window.DockedControls[0]);
                    if (firstControl != null)
                    {
                        var wind = ZXFloatController.MakeFloating(firstControl);
                        wind.Position = new Avalonia.PixelPoint(window.Left, window.Top);
                        wind.Width = window.Width;
                        wind.Height = window.Height;

                        foreach (var ctrl in window.DockedControls.Skip(1))
                        {
                            var contrl = controls.FirstOrDefault(c => c.Name == ctrl);
                            if (contrl != null)
                                wind.DockingContainer.AddToEnd(contrl);
                        }
                    }
                }
            }
            catch { }
        }
    }

    public class ZXLayout
    {
        public required string MainRows { get; set; }
        public required string MainColumns { get; set; }
        public required string LeftDockRows { get; set; }
        public required string RightDockRows { get; set; }
        public required string[] LeftDockedControls { get; set; }
        public required string[] RightDockedControls { get; set; }
        public required string[] BottomDockedControls { get; set; }
        public required ZXLayoutWindow[] FloatingWindows { get; set; }
    }

    public class ZXLayoutWindow
    {
        public required int Left { get; set; }
        public required int Top { get; set; }
        public required double Width { get; set; }
        public required double Height { get; set; }
        public required string[] DockedControls { get; set; }

    }
}
