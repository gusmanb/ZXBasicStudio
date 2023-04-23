using Avalonia;
using Konamiman.Z80dotNet;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public class ZXMemoryBlock : AvaloniaObject
    {
        public static StyledProperty<ZXMemoryRow> Row1Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row1", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row2Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row2", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row3Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row3", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row4Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row4", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row5Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row5", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row6Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row6", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row7Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row7", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row8Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row8", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row9Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row9", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row10Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row10", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row11Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row11", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row12Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row12", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row13Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row13", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row14Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row14", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row15Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row15", new ZXMemoryRow());
        public static StyledProperty<ZXMemoryRow> Row16Property = StyledProperty<ZXMemoryRow>.Register<ZXMemoryBlock, ZXMemoryRow>("Row16", new ZXMemoryRow());

        public ZXMemoryRow Row1
        {
            get { return GetValue<ZXMemoryRow>(Row1Property); }
            set { SetValue<ZXMemoryRow>(Row1Property, value); }
        }
        public ZXMemoryRow Row2
        {
            get { return GetValue<ZXMemoryRow>(Row2Property); }
            set { SetValue<ZXMemoryRow>(Row2Property, value); }
        }
        public ZXMemoryRow Row3
        {
            get { return GetValue<ZXMemoryRow>(Row3Property); }
            set { SetValue<ZXMemoryRow>(Row3Property, value); }
        }
        public ZXMemoryRow Row4
        {
            get { return GetValue<ZXMemoryRow>(Row4Property); }
            set { SetValue<ZXMemoryRow>(Row4Property, value); }
        }
        public ZXMemoryRow Row5
        {
            get { return GetValue<ZXMemoryRow>(Row5Property); }
            set { SetValue<ZXMemoryRow>(Row5Property, value); }
        }
        public ZXMemoryRow Row6
        {
            get { return GetValue<ZXMemoryRow>(Row6Property); }
            set { SetValue<ZXMemoryRow>(Row6Property, value); }
        }
        public ZXMemoryRow Row7
        {
            get { return GetValue<ZXMemoryRow>(Row7Property); }
            set { SetValue<ZXMemoryRow>(Row7Property, value); }
        }
        public ZXMemoryRow Row8
        {
            get { return GetValue<ZXMemoryRow>(Row8Property); }
            set { SetValue<ZXMemoryRow>(Row8Property, value); }
        }
        public ZXMemoryRow Row9
        {
            get { return GetValue<ZXMemoryRow>(Row9Property); }
            set { SetValue<ZXMemoryRow>(Row9Property, value); }
        }
        public ZXMemoryRow Row10
        {
            get { return GetValue<ZXMemoryRow>(Row10Property); }
            set { SetValue<ZXMemoryRow>(Row10Property, value); }
        }
        public ZXMemoryRow Row11
        {
            get { return GetValue<ZXMemoryRow>(Row11Property); }
            set { SetValue<ZXMemoryRow>(Row11Property, value); }
        }
        public ZXMemoryRow Row12
        {
            get { return GetValue<ZXMemoryRow>(Row12Property); }
            set { SetValue<ZXMemoryRow>(Row12Property, value); }
        }
        public ZXMemoryRow Row13
        {
            get { return GetValue<ZXMemoryRow>(Row13Property); }
            set { SetValue<ZXMemoryRow>(Row13Property, value); }
        }
        public ZXMemoryRow Row14
        {
            get { return GetValue<ZXMemoryRow>(Row14Property); }
            set { SetValue<ZXMemoryRow>(Row14Property, value); }
        }
        public ZXMemoryRow Row15
        {
            get { return GetValue<ZXMemoryRow>(Row15Property); }
            set { SetValue<ZXMemoryRow>(Row15Property, value); }
        }
        public ZXMemoryRow Row16
        {
            get { return GetValue<ZXMemoryRow>(Row16Property); }
            set { SetValue<ZXMemoryRow>(Row16Property, value); }
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
