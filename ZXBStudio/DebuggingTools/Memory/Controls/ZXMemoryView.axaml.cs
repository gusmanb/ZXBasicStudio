using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Konamiman.Z80dotNet;
using System.Collections.Generic;
using System.Net;
using System.Text;
using ZXBasicStudio.DebuggingTools.Memory.Classes;
using ZXBasicStudio.DebuggingTools.Memory.Dialogs;

namespace ZXBasicStudio.DebuggingTools.Memory.Controls
{
    public partial class ZXMemoryView : UserControl
    {
        TextBlock[] dataBlocks;
        TextBlock[] addressBlocks;
        IMemory? mem;
        DispatcherTimer timer;

        public bool ASCIIMode { get; set; }
        public ZXMemoryRange? HighlightedRange { get; set; }
        public ZXMemoryView()
        {
            AddHandler(PointerWheelChangedEvent, ScrollWheel, handledEventsToo: true);

            InitializeComponent();

            List<TextBlock> dbs = new List<TextBlock>();
            List<TextBlock> abs = new List<TextBlock>();

            for (int y = 0; y < 16; y++)
            {
                TextBlock tbad = new TextBlock();
                tbad.Text = "0000";
                tbad.SetValue(Grid.RowProperty, y + 1);
                grdBlocks.Children.Add(tbad);
                abs.Add(tbad);

                for (int x = 0; x < 16; x++)
                {
                    TextBlock tbd = new TextBlock();
                    tbd.Text = "00";
                    tbd.SetValue(Grid.RowProperty, y + 1);
                    tbd.SetValue(Grid.ColumnProperty, x + + 1);
                    grdBlocks.Children.Add(tbd);
                    dbs.Add(tbd);
                }
            }

            dataBlocks = dbs.ToArray();
            addressBlocks = abs.ToArray();

            scrFirstRow.Scroll += ScrFirstRow_Scroll;
            timer = new DispatcherTimer();
            timer.Interval = new System.TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
            timer.Start();

            mnuAscii.Click += (o, e) => { ASCIIMode = true; Update(); };
            mnuHex.Click += (o, e) => { ASCIIMode = false; Update(); };
            mnuSearch.Click += MnuSearch_Click;
        }

        private async void MnuSearch_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (mem == null)
                return;

            var top = TopLevel.GetTopLevel(this);

            if(top == null) 
                return;

            var win = top as Window;

            if (win == null) 
                return;

            ZXMemorySearchDialog dlg = new ZXMemorySearchDialog();
            dlg.Initialize(mem, this);

            await dlg.ShowDialog(win);
        }

        public void Initialize(IMemory Memory)
        {
            mem = Memory;
        }
        private void ScrollWheel(object? sender, Avalonia.Input.PointerWheelEventArgs e)
        {
            scrFirstRow.Value -= e.Delta.Y;
            timer.Stop();
            Update();
            timer.Start();
        }

        private void ScrFirstRow_Scroll(object? sender, Avalonia.Controls.Primitives.ScrollEventArgs e)
        {
            if (mem == null)
                return;

            timer.Stop();
            Update();
            timer.Start();
        }

        private void Timer_Tick(object? sender, System.EventArgs e)
        {
            if (mem == null)
                return;

            Update();
        }

        public void Update()
        {
            if (mem == null)
                return;

            ushort startAddress = (ushort)((int)(scrFirstRow.Value) * 16);
            byte[] block = mem.GetContents(startAddress, 256);

            for (int y = 0; y < 16; y++)
            {
                ushort rowAddress = (ushort)(y * 16);
                addressBlocks[y].Text = (startAddress + rowAddress).ToString("X4");

                for (int x = 0; x < 16; x++)
                {
                    int byteAddress = rowAddress + x + startAddress;

                    var tb = dataBlocks[x + rowAddress];
                    tb.Text = ASCIIMode ? Encoding.ASCII.GetString(block, x + rowAddress, 1) : block[x + rowAddress].ToString("X2");

                    if (HighlightedRange != null && HighlightedRange.Contains(byteAddress))
                    {
                        if (tb.Background != Brushes.Red)
                            tb.Background = Brushes.Red;
                    }
                    else
                    {
                        if (tb.Background != null)
                            tb.Background = null;
                    }
                }
            }
        }

        public void GoToAddress(ushort Address)
        {
            int rowAddress = (Address & 0xFFF0) >> 4;

            if (rowAddress > 4080)
                rowAddress = 4080;

            scrFirstRow.Value = rowAddress;

            timer.Stop();
            Update();
            timer.Start();
        }

        protected override Size MeasureCore(Size availableSize)
        {
            return base.MeasureCore(availableSize);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            base.MeasureOverride(new Size());
            return base.MeasureOverride(availableSize);
        }

        protected override void OnMeasureInvalidated()
        {
            base.OnMeasureInvalidated();
        }
    }
}
