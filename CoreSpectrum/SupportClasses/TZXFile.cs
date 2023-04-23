using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.SupportClasses
{
    public class TZXFile
    {
        public static Tape? Load(string file)
        {
            byte[] data = File.ReadAllBytes(file);

            string id = Encoding.ASCII.GetString(data, 0, 7);

            if (id != "ZXTape!")
                return null;

            int offset = 10;

            List<TZXBlock> blocks = new List<TZXBlock>();
            TZXBlock? block;

            while (ReadBlock(data, ref offset, out block))
            {
                if (block == null)
                    return null;

                blocks.Add(block);
            }

            if (offset != data.Length)
                return null;

            List<TapeBlock> tblocks = new List<TapeBlock>();
            TapeStream? currentStream = null;

            ControlStructure control = new ControlStructure();

            for (int buc = 0; buc < blocks.Count; buc++)
            {
                if (!ProcessBlock(blocks[buc], ref currentStream, tblocks, control))
                    return null;

                if (control.JumpBlock != null)
                {
                    buc = control.JumpBlock.Value - 1;
                    control.JumpBlock = null;
                }
                else if (control.LoopCount != null)
                {
                    int outPos = 0;

                    for (int loop = 0; loop < control.LoopCount; loop++)
                    {
                        if (!ExecuteLoop(buc, blocks, ref currentStream, tblocks, control, out outPos))
                            return null;
                    }

                    control.LoopCount = null;

                    buc = outPos;
                }
                else if (control.CallList != null)
                {
                    for (int call = 0; call < control.CallList.Length; call++)
                    {
                        if (!ExecuteCall(control.CallList[call], blocks, ref currentStream, tblocks, control))
                            return null;

                        control.CallList = null;
                    }
                }
            }


            if (currentStream != null)
            {
                tblocks.Add(new TapeBlock(currentStream, BlockType.Special));
                currentStream = null;
            }

            return new Tape(tblocks.ToArray());
        }

        private static bool ExecuteLoop(int loopStart, List<TZXBlock> blocks, ref TapeStream? currentStream, List<TapeBlock> tblocks, ControlStructure control, out int OutPos)
        {
            OutPos = 0;

            for (int buc = loopStart; buc < blocks.Count; buc++)
            {
                if (blocks[buc].BlockType == 0x25)
                {
                    OutPos = buc;
                    return true;
                }
                if (!ProcessBlock(blocks[buc], ref currentStream, tblocks, control))
                    return false;
            }

            return false;
        }

        private static bool ExecuteCall(int CallAddress, List<TZXBlock> blocks, ref TapeStream? currentStream, List<TapeBlock> tblocks, ControlStructure control)
        {
            for (int buc = CallAddress; buc < blocks.Count; buc++)
            {
                if (blocks[buc].BlockType == 0x27)
                    return true;

                if (!ProcessBlock(blocks[buc], ref currentStream, tblocks, control))
                    return false;
            }

            return false;
        }

        public class ControlStructure
        {
            public int? JumpBlock { get; set; }
            public ushort[]? CallList { get; set; }
            public int? LoopCount { get; set; }
        }

        public static bool ProcessBlock(TZXBlock bblock, ref TapeStream? currentStream, List<TapeBlock> tblocks, ControlStructure control)
        {
            switch (bblock.BlockType)
            {
                case 0x10:

                    if (currentStream != null)
                    {
                        tblocks.Add(new TapeBlock(currentStream, BlockType.Special));
                        currentStream = null;
                    }

                    var dblock = bblock as Block10;
                    tblocks.Add(new TapeBlock(tblocks.LastOrDefault()?.Stream?.LastState ?? false, dblock.Data, dblock.Pause));
                    break;

                case 0x11:

                    if (currentStream != null)
                    {
                        tblocks.Add(new TapeBlock(currentStream, BlockType.Special));
                        currentStream = null;
                    }

                    var tblock = bblock as Block11;
                    tblocks.Add(new TapeBlock(tblocks.LastOrDefault()?.Stream?.LastState ?? false, tblock.GetTimmings(), tblock.Data, tblock.Pause, tblock.LastByteBits));
                    break;

                case 0x12:

                    var ptBlock = bblock as Block12;
                    currentStream.AddPulses(ptBlock.PulseLength, ptBlock.PulseCount);
                    break;

                case 0x13:

                    var psBlock = bblock as Block13;
                    currentStream.AddPulses(psBlock.PulseCount, psBlock.PulseLengths);
                    break;

                case 0x14:

                    var pdBlock = bblock as Block14;
                    currentStream.AddData(pdBlock.Data, pdBlock.PulseZeroLength, pdBlock.PulseOneLength, pdBlock.LastByteBits);
                    break;

                case 0x20:

                    var pBlock = bblock as Block20;
                    currentStream.AddSilence(pBlock.Pause);
                    break;

                case 0x21:

                    if (currentStream != null)
                    {
                        tblocks.Add(new TapeBlock(currentStream, BlockType.Special));
                        currentStream = null;
                    }

                    currentStream = new TapeStream(tblocks.LastOrDefault()?.Stream?.LastState ?? false);

                    break;

                case 0x22:

                    if (currentStream != null)
                    {
                        tblocks.Add(new TapeBlock(currentStream, BlockType.Special));
                        currentStream = null;
                    }

                    break;

                case 0x23:

                    var jBlock = bblock as Block23;
                    control.JumpBlock = jBlock.Jump;

                    break;

                case 0x24:

                    var lBlock = bblock as Block24;
                    control.LoopCount = lBlock.LoopCount;
                    break;

                case 0x25:

                    //Should be processed by loop function,it should never reach here
                    return false;

                case 0x26:

                    var cBlock = bblock as Block26;
                    control.CallList = cBlock.Calls;
                    break;

                case 0x27:

                    //Should be processed by call function,it should never reach here
                    return false;

                case 0x28:
                case 0x2A:
                case 0x2B:
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x35:
                case 0x5A:

                    return true;

                default:
                    return false;
            }

            return true;
        }

        static bool IsHeader(byte[] data)
        {
            return data.Length == 19 && data[0] == 0;
        }
        private static bool ReadBlock(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            if (offset >= data.Length)
                return false;

            byte type = data[offset++];

            switch (type)
            {
                case 0x10:
                    return GenerateBlock10(data, ref offset, out block);
                    break;
                case 0x11:
                    return GenerateBlock11(data, ref offset, out block);
                    break;
                case 0x12:
                    return GenerateBlock12(data, ref offset, out block);
                    break;
                case 0x13:
                    return GenerateBlock13(data, ref offset, out block);
                    break;
                case 0x14:
                    return GenerateBlock14(data, ref offset, out block);
                    break;
                case 0x15:
                    return GenerateBlock15(data, ref offset, out block);
                    break;
                case 0x18:
                    return GenerateBlock18(data, ref offset, out block);
                    break;
                case 0x19:
                    return GenerateBlock19(data, ref offset, out block);
                    break;
                case 0x20:
                    return GenerateBlock20(data, ref offset, out block);
                    break;
                case 0x21:
                    return GenerateBlock21(data, ref offset, out block);
                    break;
                case 0x22:
                    return GenerateBlock22(data, ref offset, out block);
                    break;
                case 0x23:
                    return GenerateBlock23(data, ref offset, out block);
                    break;
                case 0x24:
                    return GenerateBlock24(data, ref offset, out block);
                    break;
                case 0x25:
                    return GenerateBlock25(data, ref offset, out block);
                    break;
                case 0x26:
                    return GenerateBlock26(data, ref offset, out block);
                    break;
                case 0x27:
                    return GenerateBlock27(data, ref offset, out block);
                    break;
                case 0x28:
                    return GenerateBlock28(data, ref offset, out block);
                    break;
                case 0x2A:
                    return GenerateBlock2A(data, ref offset, out block);
                    break;
                case 0x2B:
                    return GenerateBlock2B(data, ref offset, out block);
                    break;
                case 0x30:
                    return GenerateBlock30(data, ref offset, out block);
                    break;
                case 0x31:
                    return GenerateBlock31(data, ref offset, out block);
                    break;
                case 0x32:
                    return GenerateBlock32(data, ref offset, out block);
                    break;
                case 0x33:
                    return GenerateBlock33(data, ref offset, out block);
                    break;
                case 0x35:
                    return GenerateBlock35(data, ref offset, out block);
                    break;
                case 0x5A:
                    return GenerateBlock5A(data, ref offset, out block);
                    break;
                default:
                    block = null;
                    return false;
            }
        }

        private static bool GenerateBlock10(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block10 tblock = new Block10
                {
                    BlockType = 0x10,
                    Pause = Word(data, ref offset),
                    Length = Word(data, ref offset)
                };
                tblock.Data = Slice(data, ref offset, tblock.Length);
                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock11(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block11 tblock = new Block11
                {
                    BlockType = 0x11,
                    PulsePilotLength = Word(data, ref offset),
                    PulseSync1Length = Word(data, ref offset),
                    PulseSync2Length = Word(data, ref offset),
                    PulseZeroLength = Word(data, ref offset),
                    PulseOneLength = Word(data, ref offset),
                    PilotPulseCount = Word(data, ref offset),
                    LastByteBits = data[offset++],
                    Pause = Word(data, ref offset),
                    Length = DWord_24(data, ref offset),
                };
                tblock.Data = Slice(data, ref offset, tblock.Length);
                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock12(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block12 tblock = new Block12
                {
                    BlockType = 0x12,
                    PulseLength = Word(data, ref offset),
                    PulseCount = Word(data, ref offset),
                };

                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock13(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block13 tblock = new Block13
                {
                    BlockType = 0x13,
                    PulseCount = data[offset++]
                };

                tblock.PulseLengths = WordArray(data, ref offset, tblock.PulseCount);
                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock14(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block14 tblock = new Block14
                {
                    BlockType = 0x14,
                    PulseZeroLength = Word(data, ref offset),
                    PulseOneLength = Word(data, ref offset),
                    LastByteBits = data[offset++],
                    Pause = Word(data, ref offset),
                    Length = DWord_24(data, ref offset)

                };

                tblock.Data = Slice(data, ref offset, tblock.Length);
                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock15(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block15 tblock = new Block15
                {
                    BlockType = 0x15,
                    StatesPerSample = Word(data, ref offset),
                    Pause = Word(data, ref offset),
                    LastByteBits = data[offset++]
                };

                tblock.Data = Slice(data, ref offset, tblock.Length);
                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock18(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block18 tblock = new Block18
                {
                    BlockType = 0x18,
                    Length = DWord(data, ref offset)
                };

                tblock.Data = Slice(data, ref offset, tblock.Length);
                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock19(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block19 tblock = new Block19
                {
                    BlockType = 0x19,
                    Length = DWord(data, ref offset)
                };

                tblock.Data = Slice(data, ref offset, tblock.Length);
                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock20(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block20 tblock = new Block20
                {
                    BlockType = 0x20,
                    Pause = Word(data, ref offset)
                };

                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock21(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block21 tblock = new Block21
                {
                    BlockType = 0x21,
                    Length = data[offset++]
                };

                tblock.Data = Slice(data, ref offset, tblock.Length);
                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock22(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block22 tblock = new Block22
                {
                    BlockType = 0x22
                };

                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock23(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block23 tblock = new Block23
                {
                    BlockType = 0x23,
                    Jump = SWord(data, ref offset)
                };

                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock24(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block24 tblock = new Block24
                {
                    BlockType = 0x24,
                    LoopCount = Word(data, ref offset)
                };

                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock25(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block25 tblock = new Block25
                {
                    BlockType = 0x25
                };

                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock26(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block26 tblock = new Block26
                {
                    BlockType = 0x26,
                    CallCount = Word(data, ref offset)
                };

                tblock.Calls = WordArray(data, ref offset, tblock.CallCount);
                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock27(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block27 tblock = new Block27
                {
                    BlockType = 0x27                    
                };

                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock28(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block28 tblock = new Block28
                {
                    BlockType = 0x28,
                    Length = Word(data, ref offset)
                };

                tblock.Data = Slice(data, ref offset, tblock.Length);
                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock2A(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block2A tblock = new Block2A
                {
                    BlockType = 0x2a,
                    Length = Word(data, ref offset)
                };

                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock2B(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block2B tblock = new Block2B
                {
                    BlockType = 0x2b,
                    Length = DWord(data, ref offset),
                    SignalLevel = data[offset++]
                };

                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock30(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block30 tblock = new Block30
                {
                    BlockType = 0x30,
                    Length = data[offset++]
                };

                tblock.Data = Slice(data, ref offset, tblock.Length);
                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock31(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block31 tblock = new Block31
                {
                    BlockType = 0x31,
                    Time = data[offset++],
                    Length = data[offset++]
                };

                tblock.Data = Slice(data, ref offset, tblock.Length);
                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock32(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block32 tblock = new Block32
                {
                    BlockType = 0x32,
                    Length = Word(data, ref offset)
                };

                tblock.Data = Slice(data, ref offset, tblock.Length);
                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock33(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block33 tblock = new Block33
                {
                    MachineCount = data[offset++],
                };

                tblock.Data = Slice(data, ref offset, tblock.MachineCount * 3);
                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock35(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block35 tblock = new Block35
                {
                    BlockType = 0x35
                };

                tblock.Id = Slice(data, ref offset, 10);
                tblock.Length = DWord(data, ref offset);
                tblock.Data = Slice(data, ref offset, tblock.Length);
                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static bool GenerateBlock5A(byte[] data, ref int offset, out TZXBlock? block)
        {
            block = null;

            try
            {
                Block5A tblock = new Block5A
                {
                    BlockType = 0x5A,
                    Data = Slice(data, ref offset, 9)
                };

                block = tblock;
                return true;
            }
            catch { return false; }
        }
        private static ushort Word(byte[] data, ref int offset)
        {
            var value = (ushort)(data[offset] | (data[offset + 1] << 8));
            offset += 2;
            return value;
        }

        private static short SWord(byte[] data, ref int offset)
        {
            var value = (short)(data[offset] | (data[offset + 1] << 8));
            offset += 2;
            return value;
        }

        private static uint DWord(byte[] data, ref int offset)
        {
            var value = (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 1] << 16) | (data[offset + 1] << 24));
            offset += 4;
            return value;
        }

        private static uint DWord_24(byte[] data, ref int offset)
        {
            var value = (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16));
            offset += 3;
            return value;
        }

        private static byte[] Slice(byte[] data, ref int offset, long length)
        {
            byte[] slice = new byte[length];
            Buffer.BlockCopy(data, offset, slice, 0, (int)length);
            offset += (int)length;
            return slice;
        }

        private static ushort[] WordArray(byte[] data, ref int offset, int length)
        {
            ushort[] words = new ushort[length];

            for(int buc = 0; buc < length; buc++)
                words[buc] = Word(data, ref offset);

            return words;
        }

        public class TZXBlock
        {
            public byte BlockType { get; set; }
        }

        /// <summary>
        /// Standard data block
        /// </summary>
        class Block10 : TZXBlock
        {
            public ushort Pause { get; set; }
            public ushort Length { get; set; }
            public byte[] Data { get; set; }
        }

        /// <summary>
        /// Turbo data block
        /// </summary>
        class Block11 : TZXBlock
        {
            public ushort PulsePilotLength { get; set; }
            public ushort PulseSync1Length { get; set; }
            public ushort PulseSync2Length { get; set; }
            public ushort PulseZeroLength { get; set; }
            public ushort PulseOneLength { get; set; }
            public ushort PilotPulseCount { get; set; }
            public byte LastByteBits { get; set; }
            public ushort Pause { get; set; }
            //24 bits (3 bytes)
            public uint Length { get; set; }
            public byte[] Data { get; set; }
            public TapeStreamTimmings GetTimmings()
            {
                TapeStreamTimmings timmings = new TapeStreamTimmings
                {
                    PulsePilotLength = PulsePilotLength,
                    PulseSync1Length = PulseSync1Length,
                    PulseSync2Length = PulseSync2Length,
                    PulseZeroLength = PulseZeroLength,
                    PulseOneLength = PulseOneLength,
                    PilotPulseCount = PilotPulseCount
                };

                return timmings;
            }
        }

        /// <summary>
        /// Pure tone block
        /// </summary>
        class Block12 : TZXBlock
        {
            public ushort PulseLength { get; set; }
            public ushort PulseCount { get; set; }
        }

        /// <summary>
        /// Pulse sequence
        /// </summary>
        class Block13 : TZXBlock
        {
            public byte PulseCount { get; set; }
            public ushort[] PulseLengths { get; set; }
        }

        /// <summary>
        /// Pure data block
        /// </summary>
        class Block14 : TZXBlock
        {
            public ushort PulseZeroLength { get; set; }
            public ushort PulseOneLength { get; set; }
            public byte LastByteBits { get; set; }
            public ushort Pause { get; set; }
            //24 bits (3 bytes)
            public uint Length { get; set; }
            public byte[] Data { get; set; }
        }

        /// <summary>
        /// Direct recording
        /// </summary>
        class Block15 : TZXBlock
        {
            public ushort StatesPerSample { get; set; }
            public ushort Pause { get; set; }
            //24 bits (3 bytes)
            public uint Length { get; set; }
            public byte[] Data { get; set; }
            public byte LastByteBits { get; set; }
        }

        /// <summary>
        /// CSW recording
        /// </summary>
        class Block18 : TZXBlock
        {
            //32 bits
            public uint Length { get; set; }
            public byte[] Data { get; set; }

        }

        /// <summary>
        /// Generalized data, unsupported
        /// </summary>
        class Block19 : TZXBlock
        {
            public uint Length { get; set; }
            public byte[] Data { get; set; }
        }

        /// <summary>
        /// Pause
        /// </summary>
        class Block20 : TZXBlock
        {
            public ushort Pause { get; set; }
        }

        /// <summary>
        /// Begin group
        /// </summary>
        class Block21 : TZXBlock
        {
            public byte Length { get; set; }
            public byte[]? Data { get; set; }
            public string? Name { get { return Data == null ? null : Encoding.ASCII.GetString(Data); } }
        }

        /// <summary>
        /// End group
        /// </summary>
        class Block22 : TZXBlock
        { 
        }

        /// <summary>
        /// Jump to block (relative)
        /// </summary>
        class Block23 : TZXBlock
        {
            public short Jump { get; set; }
        }

        /// <summary>
        /// Loop start
        /// </summary>
        class Block24 : TZXBlock
        {
            public ushort LoopCount { get; set; }
        }

        /// <summary>
        /// Loop end
        /// </summary>
        class Block25 : TZXBlock
        { }

        /// <summary>
        /// Call sequence
        /// </summary>
        class Block26 : TZXBlock
        { 
            public ushort CallCount { get; set; }
            public ushort[] Calls { get; set; }
        }

        /// <summary>
        /// Call return
        /// </summary>
        class Block27 : TZXBlock
        { }

        /// <summary>
        /// Select block, ignored
        /// </summary>
        class Block28 : TZXBlock
        {
            public ushort Length { get; set; }
            public byte[] Data { get; set; }
        }

        /// <summary>
        /// Stop if 48k, ignored
        /// </summary>
        class Block2A : TZXBlock
        {
            public uint Length { get; set; }
        }

        /// <summary>
        /// Set signal level, ignored
        /// </summary>
        class Block2B : TZXBlock
        {
            public uint Length { get; set; }
            public byte SignalLevel { get; set; }
        }

        /// <summary>
        /// Text description
        /// </summary>
        class Block30 : TZXBlock
        {
            public byte Length { get; set; }
            public byte[]? Data { get; set; }
            public string? Text { get { return Data == null ? null : Encoding.ASCII.GetString(Data); } }
        }

        /// <summary>
        /// Message block, ignored
        /// </summary>
        class Block31 : TZXBlock
        {
            public byte Time { get; set; }
            public byte Length { get; set; }
            public byte[]? Data { get; set; }
            public string? Text { get { return Data == null ? null : Encoding.ASCII.GetString(Data); } }
        }

        /// <summary>
        /// Archive info, ignored
        /// </summary>
        class Block32 : TZXBlock
        {
            public ushort Length { get; set; }
            public byte[] Data { get; set; }
        }

        /// <summary>
        /// Hardware type, ignored
        /// </summary>
        class Block33 : TZXBlock
        {
            public byte MachineCount { get; set; }
            public byte[] Data { get; set; } //MachineCount * 3
        }

        /// <summary>
        /// Custom info block, ignored
        /// </summary>
        class Block35 : TZXBlock
        {
            public byte[] Id { get; set; }
            public string? IdString { get { return Id == null ? null : Encoding.ASCII.GetString(Data); } }
            public uint Length { get; set; }
            public byte[] Data { get; set; }
        }

        /// <summary>
        /// Glue block, ignored
        /// </summary>
        class Block5A : TZXBlock
        {
            public byte[] Data { get; set; }
        }
    }
}
