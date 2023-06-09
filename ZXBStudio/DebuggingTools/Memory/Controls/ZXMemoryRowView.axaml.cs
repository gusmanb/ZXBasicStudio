using Avalonia;
using Avalonia.Controls;
using Konamiman.Z80dotNet;
using ZXBasicStudio.DebuggingTools.Memory.Binding;

namespace ZXBasicStudio.DebuggingTools.Memory.Controls
{
    public partial class ZXMemoryRowView : UserControl
    {
        public static StyledProperty<ZXMemoryRow> RowProperty = StyledProperty<ZXMemoryRow>.Register<ZXMemoryRowView, ZXMemoryRow>("Row");
        public ZXMemoryRow Row 
        {
            get { return GetValue<ZXMemoryRow>(RowProperty); }
            set { SetValue<ZXMemoryRow>(RowProperty, value); }
        }

        public ZXMemoryRowView()
        {
            DataContext = this;
            InitializeComponent();
        }
    }
}
