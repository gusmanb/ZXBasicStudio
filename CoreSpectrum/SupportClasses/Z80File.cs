using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.SupportClasses
{
    public class Z80File
    {
        public int StartAddress { get; set; } = 16384;
        public int EndAddress { get; set; } = 65536;
        public Z80TransferHeader Header { get; set; } = new Z80TransferHeader();
        public byte[] Data { get; set; } = new byte[64 * 1024];
        public static unsafe Z80File Load(string FileName)
        {
            byte[] data;

            try
            {
                data = File.ReadAllBytes(FileName);
            }
            catch { return null; }

            Z80File file = new Z80File();
            int headerLength = 30;
            bool compressed = true;

            fixed (byte* bData = data)
            {

                Z80Header* head = (Z80Header*)&bData[0];

                if (head->PC == 00)
                {
                    Z80Header2* head2 = (Z80Header2*)&bData[headerLength];
                    headerLength += head2->Size + 2;
                    head->PC = head2->PC;
                }

                compressed = (head->Info1 & 32) != 0;


                file.Header.I = head->I;
                file.Header.HLP = head->HLP;
                file.Header.DEP = head->DEP;
                file.Header.BCP = head->BCP;
                file.Header.AFP = head->AFP;
                file.Header.HL = head->HL;
                file.Header.DE = head->DE;
                file.Header.BC = head->BC;
                file.Header.IY = head->IY;
                file.Header.IX = head->IX;
                file.Header.IFFStatus = head->IFF1;
                file.Header.R = (byte)((head->R & 0x7F) | ((head->Info1 & 0x80) << 7));
                file.Header.AF = head->AF;
                file.Header.SP = head->SP;
                file.Header.IntMode = (byte)(head->Info2 & 3);
                file.Header.BorderColor = (byte)((head->Info1 >> 1) & 7);
                file.Header.PC = head->PC;

            }

            if (headerLength != 30)
            {
                compressed = true;

                int i = headerLength;

                while (i != data.Length)
                {
                    var datalen = Word(data, i);
                    var page = GetPage(data[i + 2]);

                    if (page == 0xFFFF)
                        return null;

                    i = i + 3; // skip block header


                    if (datalen == 0xFFFF)
                    {
                        datalen = 16384;
                        compressed = false;
                    }

                    UnpackMem(page, data, i, i + datalen, compressed, file);

                    i += datalen;
                }
            }
            else
                UnpackMem(0x4000, data, 30, data.Length, compressed, file);

            return file;
        }

        private static ushort Word(byte[] data, int offset)
        {
            return (ushort)(data[offset] | (data[offset + 1] << 8));
        }
        private static ushort GetPage(byte page)
        {
            switch (page)
            {
                case 0: return 0; // rom
                case 4: return 0x8000;
                case 5: return 0xc000;
                case 8: return 0x4000;
                default: return 0xFFFF;
            }
        }

        private static ushort SwapBytes(ushort Value)
        {
            return (ushort)(((Value & 0xFF) << 8) | ((Value & 0xFF00) >> 8));
        }

        private static ushort UnpackMem(ushort offset, byte[] data, int start, int end, bool compressed, Z80File File)
        {
            for (int i = start; i < end; ++i)
            {
                if (compressed &&
                    data[i + 0] == 0x00 &&
                    data[i + 1] == 0xED &&
                    data[i + 2] == 0xED &&
                    data[i + 3] == 0x00)
                    break;

                if (data[i] == 0xED && data[i + 1] == 0xED && compressed)
                {
                    var repeat = data[i + 2];
                    var value = data[i + 3];
                    while (repeat-- > 0)
                    {
                        File.Data[offset++] = value;
                    }

                    i = i + 3;
                }
                else
                {
                    File.Data[offset++] = data[i];
                }
            }
            return offset;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct Z80Header
        {
            [FieldOffset(0)]
            public ushort AF;
            [FieldOffset(2)]
            public ushort BC;
            [FieldOffset(4)]
            public ushort HL;
            [FieldOffset(6)]
            public ushort PC;
            [FieldOffset(8)]
            public ushort SP;
            [FieldOffset(10)]
            public byte I;
            [FieldOffset(11)]
            public byte R;
            [FieldOffset(12)]
            public byte Info1;
            [FieldOffset(13)]
            public ushort DE;
            [FieldOffset(15)]
            public ushort BCP;
            [FieldOffset(17)]
            public ushort DEP;
            [FieldOffset(19)]
            public ushort HLP;
            [FieldOffset(21)]
            public ushort AFP;
            [FieldOffset(23)]
            public ushort IY;
            [FieldOffset(25)]
            public ushort IX;
            [FieldOffset(27)]
            public byte IFF1;
            [FieldOffset(28)]
            public byte IFF2;
            [FieldOffset(29)]
            public byte Info2;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Z80Header2
        {
            public ushort Size;
            public ushort PC;
        }

        [StructLayout(LayoutKind.Explicit)]
        public class Z80TransferHeader
        {
            [FieldOffset(0)]
            public byte I;
            [FieldOffset(1)]
            public ushort HLP;
            [FieldOffset(3)]
            public ushort DEP;
            [FieldOffset(5)]
            public ushort BCP;
            [FieldOffset(7)]
            public ushort AFP;
            [FieldOffset(9)]
            public ushort HL;
            [FieldOffset(11)]
            public ushort DE;
            [FieldOffset(13)]
            public ushort BC;
            [FieldOffset(15)]
            public ushort IY;
            [FieldOffset(17)]
            public ushort IX;
            [FieldOffset(19)]
            public byte IFFStatus;
            [FieldOffset(20)]
            public byte R;
            [FieldOffset(21)]
            public ushort AF;
            [FieldOffset(23)]
            public ushort SP;
            [FieldOffset(25)]
            public byte IntMode;
            [FieldOffset(26)]
            public byte BorderColor;
            [FieldOffset(27)]
            public ushort PC;
        }
    }

}
