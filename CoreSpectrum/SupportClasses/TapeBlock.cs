using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CoreSpectrum.SupportClasses.TAPFile;

namespace CoreSpectrum.SupportClasses
{
    public class TapeBlock
    {
        TapeStream? _stream;
        byte[]? _data;
        TapeHeader? _header;
        BlockType _blockType;
        public BlockType BlockType { get { return _blockType; } }
        public TapeStream? Stream { get { return _stream; } }
        public byte[]? Data { get { return _data; } }
        public TapeHeader? Header { get { return _header; } }
        public ulong Length { get { return _stream?.Length ?? 0; } }
        public TapeBlock(bool InitialPulseState,  byte[] data)
        {
            LoadBlock(InitialPulseState, data, 0, data.Length);
        }
        public TapeBlock(bool InitialPulseState, byte[] data, int offset, int length)
        {
            LoadBlock(InitialPulseState, data, offset, length);
        }
        public TapeBlock(bool InitialPulseState, byte[] data, int silence)
        {
            LoadBlock(InitialPulseState, data, 0, data.Length, silence);
        }
        public TapeBlock(bool InitialPulseState, TapeStreamTimmings timmings, byte[] data, int silence)
        {
            LoadBlock(InitialPulseState, timmings, data, 0, data.Length, silence);
        }

        public TapeBlock(bool InitialPulseState, TapeStreamTimmings timmings, byte[] data, int silence, byte LastByteBits)
        {
            LoadBlock(InitialPulseState, timmings, data, 0, data.Length, silence, LastByteBits);
        }

        public TapeBlock(TapeStream stream, byte[]? data, BlockType blockType)
        { 
            _stream = stream;
            _data = data;
            _blockType = blockType;

            if (_blockType == BlockType.Header)
                GenerateHeader();
        }
        public TapeBlock(TapeStream stream, BlockType blockType)
        {
            _stream = stream;
            _blockType = blockType;
            _data = new byte[0];
        }

        public TapeBlock(ulong Count)
        {
            _blockType = BlockType.Calibration;
            _stream = new TapeStream(false, Count);
        }

        private void LoadBlock(bool InitialPulseState, byte[] data, int offset, int length, int silence = -1)
        {
            _data = new byte[length];
            Buffer.BlockCopy(data, offset, _data, 0, length);

            bool isHeader = _data[0] == 0;

            if (isHeader)
            {
                _blockType = BlockType.Header;
                GenerateHeader();
                _stream = new TapeStream(InitialPulseState, isHeader, _data, silence > 0 ? silence : 500);
            }
            else
            {
                _blockType = BlockType.Data;
                _stream = new TapeStream(InitialPulseState, isHeader, _data, silence > 0 ? silence : 1000);
            }
        }
        private void LoadBlock(bool InitialPulseState, TapeStreamTimmings timmings, byte[] data, int offset, int length, int silence)
        {
            _data = new byte[length];
            Buffer.BlockCopy(data, offset, _data, 0, length);

            _blockType = BlockType.Special;
            _stream = new TapeStream(InitialPulseState, timmings, _data, silence > 0 ? silence : 1000);
        }
        private void LoadBlock(bool InitialPulseState, TapeStreamTimmings timmings, byte[] data, int offset, int length, int silence, byte LastByteBits)
        {
            _data = new byte[length];
            Buffer.BlockCopy(data, offset, _data, 0, length);

            _blockType = BlockType.Special;
            _stream = new TapeStream(InitialPulseState, timmings, _data, silence > 0 ? silence : 1000, LastByteBits);
        }
        private void GenerateHeader()
        {
            _header = new TapeHeader();
            _header.Type = (DataBlockType)Data[1];
            _header.Name = new byte[10];
            Buffer.BlockCopy(Data, 2, _header.Name, 0, 10);
            _header.Length = Word(Data, 12);
            _header.Param1 = Word(Data, 14);
            _header.Param2 = Word(Data, 16);
            _header.Checksum = Data[18];
        }
        private static ushort Word(byte[] data, int offset)
        {
            return (ushort)(data[offset] | (data[offset + 1] << 8));
        }
    }
    public class TapeHeader
    {
        public DataBlockType Type;
        public byte[] Name;
        public ushort Length;
        public ushort Param1;
        public ushort Param2;
        public byte Checksum;
    }
    public enum BlockType
    {
        Calibration,
        Header,
        Data,
        Special
    }
    public enum DataBlockType : byte
    {
        Program = 0,
        Number = 1,
        Character = 2,
        Code = 3
    }
}
