using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.Classes;

namespace ZXBasicStudio.Emulator.Classes.Connectors
{
    public interface IEmulatorConnector
    {
        public bool OpenConnector(object ConnectionData);
        public bool LoadImage(byte[] Image, ushort Address);
        public bool AddBreakpoint(Guid Id, ushort Address);
        public bool RemoveBreakpoint(Guid Id);
        public bool Play();
        public bool Stop();
        public bool Pause();
        public bool Resume();
        public bool Step();
        public byte[] ReadMemory(ushort Address, ushort Size);
        public bool WriteMemory(ushort Address, byte[] Data);
        public ushort GetRegister();
    }

    public enum ZXBEmulatorRegister
    {
        AF,
        BC,
        DE,
        HL,
        AFP,
        BCP,
        DEP,
        HLP,
        IX,
        IY,
        PC,
        SP
    }
}
