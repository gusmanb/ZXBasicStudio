using Avalonia;
using Konamiman.Z80dotNet;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DebuggingTools.Memory.Binding
{
    public class ZXMemoryBlock : AvaloniaObject
    {
        public static StyledProperty<ZXMemoryRow> Row1Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row1", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row2Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row2", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row3Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row3", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row4Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row4", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row5Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row5", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row6Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row6", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row7Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row7", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row8Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row8", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row9Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row9", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row10Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row10", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row11Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row11", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row12Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row12", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row13Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row13", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row14Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row14", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row15Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row15", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row16Property = AvaloniaProperty.Register<ZXMemoryBlock, ZXMemoryRow>("Row16", new ZXMemoryRow());

        public ZXMemoryRow Row1
        {
            get { return GetValue(Row1Property); }
            set { SetValue(Row1Property, value); }
        }
        public ZXMemoryRow Row2
        {
            get { return GetValue(Row2Property); }
            set { SetValue(Row2Property, value); }
        }
        public ZXMemoryRow Row3
        {
            get { return GetValue(Row3Property); }
            set { SetValue(Row3Property, value); }
        }
        public ZXMemoryRow Row4
        {
            get { return GetValue(Row4Property); }
            set { SetValue(Row4Property, value); }
        }
        public ZXMemoryRow Row5
        {
            get { return GetValue(Row5Property); }
            set { SetValue(Row5Property, value); }
        }
        public ZXMemoryRow Row6
        {
            get { return GetValue(Row6Property); }
            set { SetValue(Row6Property, value); }
        }
        public ZXMemoryRow Row7
        {
            get { return GetValue(Row7Property); }
            set { SetValue(Row7Property, value); }
        }
        public ZXMemoryRow Row8
        {
            get { return GetValue(Row8Property); }
            set { SetValue(Row8Property, value); }
        }
        public ZXMemoryRow Row9
        {
            get { return GetValue(Row9Property); }
            set { SetValue(Row9Property, value); }
        }
        public ZXMemoryRow Row10
        {
            get { return GetValue(Row10Property); }
            set { SetValue(Row10Property, value); }
        }
        public ZXMemoryRow Row11
        {
            get { return GetValue(Row11Property); }
            set { SetValue(Row11Property, value); }
        }
        public ZXMemoryRow Row12
        {
            get { return GetValue(Row12Property); }
            set { SetValue(Row12Property, value); }
        }
        public ZXMemoryRow Row13
        {
            get { return GetValue(Row13Property); }
            set { SetValue(Row13Property, value); }
        }
        public ZXMemoryRow Row14
        {
            get { return GetValue(Row14Property); }
            set { SetValue(Row14Property, value); }
        }
        public ZXMemoryRow Row15
        {
            get { return GetValue(Row15Property); }
            set { SetValue(Row15Property, value); }
        }
        public ZXMemoryRow Row16
        {
            get { return GetValue(Row16Property); }
            set { SetValue(Row16Property, value); }
        }

        public void Update(int FirstRow, IMemory Memory)
        {
            //Max 4080
            ushort startAddress = (ushort)(FirstRow * 16);
            byte[] block = Memory.GetContents(startAddress, 256);
            Row1.Update(startAddress, 0, block);
            Row2.Update((ushort)(startAddress + 1 * 16), 1, block);
            Row3.Update((ushort)(startAddress + 2 * 16), 2, block);
            Row4.Update((ushort)(startAddress + 3 * 16), 3, block);
            Row5.Update((ushort)(startAddress + 4 * 16), 4, block);
            Row6.Update((ushort)(startAddress + 5 * 16), 5, block);
            Row7.Update((ushort)(startAddress + 6 * 16), 6, block);
            Row8.Update((ushort)(startAddress + 7 * 16), 7, block);
            Row9.Update((ushort)(startAddress + 8 * 16), 8, block);
            Row10.Update((ushort)(startAddress + 9 * 16), 9, block);
            Row11.Update((ushort)(startAddress + 10 * 16), 10, block);
            Row12.Update((ushort)(startAddress + 11 * 16), 11, block);
            Row13.Update((ushort)(startAddress + 12 * 16), 12, block);
            Row14.Update((ushort)(startAddress + 13 * 16), 13, block);
            Row15.Update((ushort)(startAddress + 14 * 16), 14, block);
            Row16.Update((ushort)(startAddress + 15 * 16), 15, block);
        }
    }
}
