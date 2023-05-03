using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Controls.DockSystem
{
    public interface IZXDockingContainer
    {
        event EventHandler? DockingControlsChanged;
        string? DockingGroup { get; }
        IEnumerable<ZXDockingControl> DockingControls { get; }
        void AddToStart(ZXDockingControl Element);
        void AddToEnd(ZXDockingControl Element);
        void InsertAt(int Index, ZXDockingControl Element);
        void Remove(ZXDockingControl Element);
        void Select(ZXDockingControl Element);
    }
}
