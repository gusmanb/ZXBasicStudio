using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ZXBasicStudio.DebuggingTools.Flags.Controls
{
    public partial class ZXFlagsView : UserControl
    {
        
        public ZXFlagsView()
        {
            InitializeComponent();
        }

        public void Update()
        {
            flag1.Text = "HH";
            flag2.Text = "HH";
        }

        public void Clear()
        {
            flag1.Text = "**";
            flag2.Text = "**";
        }
    }
}
