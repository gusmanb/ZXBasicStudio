using Avalonia.Controls;
using ExCSS;

namespace ZXBasicStudio.DebuggingTools.TStates.Controls
{
    public partial class ZXTStatesView : UserControl
    {
        ulong lastStates = 0;
        public ZXTStatesView()
        {
            InitializeComponent();
        }

        public void Update(ulong NewTStates)
        {
            tbTotal.Text = NewTStates.ToString();
            tbStep.Text = (NewTStates - lastStates).ToString();
            lastStates = NewTStates;
        }

        public void Clear()
        {
            tbTotal.Text = "--";
            tbStep.Text = "--";
            lastStates = 0;
        }
    }
}
