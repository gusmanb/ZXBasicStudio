using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Konamiman.Z80dotNet;
using ZXBasicStudio.DebuggingTools.Memory.Binding;

namespace ZXBasicStudio.DebuggingTools.Memory.Controls
{
    public partial class ZXMemoryBlockView : UserControl
    {
        public static StyledProperty<ZXMemoryBlock> BlockProperty = StyledProperty<ZXMemoryBlock>.Register<ZXMemoryBlockView, ZXMemoryBlock>("Block", new ZXMemoryBlock());

        public ZXMemoryBlock Block
        {
            get { return GetValue<ZXMemoryBlock>(BlockProperty); }
            set { SetValue(BlockProperty, value); }
        }
        DispatcherTimer timer;
        IMemory? mem;
        public ZXMemoryBlockView()
        {
            DataContext = this;
            AddHandler(PointerWheelChangedEvent, ScrollWheel, handledEventsToo: true);
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = new System.TimeSpan(0,0,1);
            timer.Tick += Timer_Tick;
            timer.Start(); 
            scrFirstRow.Scroll += ScrFirstRow_Scroll;
            rowView1.Row = Block.Row1;
            rowView2.Row = Block.Row2;
            rowView3.Row = Block.Row3;
            rowView4.Row = Block.Row4;
            rowView5.Row = Block.Row5;
            rowView6.Row = Block.Row6;
            rowView7.Row = Block.Row7;
            rowView8.Row = Block.Row8;
            rowView9.Row = Block.Row9;
            rowView10.Row = Block.Row10;
            rowView11.Row = Block.Row11;
            rowView12.Row = Block.Row12;
            rowView13.Row = Block.Row13;
            rowView14.Row = Block.Row14;
            rowView15.Row = Block.Row15;
            rowView16.Row = Block.Row16;
        }

        public void Initialize(IMemory Memory)
        {
            mem = Memory;
        }

        private void ScrollWheel(object? sender, Avalonia.Input.PointerWheelEventArgs e)
        {
            scrFirstRow.Value -= e.Delta.Y;
            timer.Stop();
            Block.Update((int)scrFirstRow.Value, mem);
            timer.Start();
        }

        private void ScrFirstRow_Scroll(object? sender, Avalonia.Controls.Primitives.ScrollEventArgs e)
        {
            if (mem == null)
                return;

            timer.Stop();
            Block.Update((int)scrFirstRow.Value, mem);
            timer.Start();
        }

        private void Timer_Tick(object? sender, System.EventArgs e)
        {
            if (mem == null)
                return;

            Block.Update((int)scrFirstRow.Value, mem);
        }
    }
}
