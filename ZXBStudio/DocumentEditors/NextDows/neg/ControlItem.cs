using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.NextDows.neg
{
    public class ControlItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Fixed properties
        public ControlsTypes ControlType { get; set; }
        public bool Visible { get; set; }
        public int ContainerId { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Ink { get; set; }
        public int Paper { get; set; }

        // Other properties
        public List<ControlProperty> Properties { get; set; }
    }
}
