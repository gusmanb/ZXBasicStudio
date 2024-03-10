using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.BuildSystem
{
    public class ZXVariable
    {
        public required string Name { get; set; }
        public required ZXVariableAddress Address { get; set; }
        public required ZXVariableScope Scope { get; set; }
        public required ZXVariableType VariableType { get; set; }
        public required ZXVariableStorage StorageType { get; set; }
        public bool IsReference { get; set; }
        public bool IsParameter { get; set; }
        public int StorageSize { get; set; }
        public object? GetValue(IMemory Memory, IZ80Registers Registers)
        {

            if (!Scope.InRange(Registers.PC))
                return null;

            ushort realAddress;

            if (Address.AddressType == ZXVariableAddressType.Absolute)
                realAddress = (ushort)Address.AddressValue;
            else
                realAddress = (ushort)(Registers.IX + Address.AddressValue);

            if (IsReference)
                realAddress = BitConverter.ToUInt16(Memory.GetContents(realAddress, 2));

            return ZXVariableHelper.GetValue(Memory, realAddress, VariableType, StorageType);
        }
        public bool SetValue(IMemory Memory, IZ80Registers Registers, object Value)
        {
            if (!Scope.InRange(Registers.PC))
                return false;

            ushort realAddress;

            if (Address.AddressType == ZXVariableAddressType.Absolute)
                realAddress = (ushort)Address.AddressValue;
            else
                realAddress = (ushort)(Registers.IX + Address.AddressValue);

            if (IsReference)
                realAddress = BitConverter.ToUInt16(Memory.GetContents(realAddress, 2));

            ZXVariableHelper.SetValue(Memory, realAddress, VariableType, StorageType, Value);

            return true;
        }
        public ZXArrayDescriptor? GetArrayDescriptor(IMemory Memory, IZ80Registers Registers)
        {
            try
            {
                if (VariableType != ZXVariableType.Array)
                    throw new InvalidCastException();

                if (!Scope.InRange(Registers.PC) || IsReference) //Parameter arrays are unsupported, impossible to retrieve descriptor
                    return null;

                if (Address.AddressType == ZXVariableAddressType.Absolute)
                {
                    ushort realAddress = (ushort)Address.AddressValue;
                    return ZXVariableHelper.GetArrayDescriptor(Memory, realAddress);
                }
                else
                {
                    var ptrAddress = (ushort)((ushort)Registers.IX + Address.AddressValue);
                    var storAddress = (ushort)((ushort)Registers.IX + Address.AddressValue + 2);
                    return ZXVariableHelper.GetArrayDescriptor(Memory, ptrAddress, storAddress, StorageSize);

                }
            }
            catch { return null; }
        }
        public object? GetArrayValue(IMemory Memory, IZ80Registers Registers, int[] Index)
        {
            if (VariableType != ZXVariableType.Array)
                throw new InvalidCastException();

            var descriptor = GetArrayDescriptor(Memory, Registers);

            if (descriptor == null)
                return null;

            if (Index == null || Index.Length != descriptor.Dimensions)
                throw new ArgumentOutOfRangeException();

            return ZXVariableHelper.GetArrayValue(Memory, descriptor, StorageType, Index);
        }
        public object? GetArrayValue(IMemory Memory, ZXArrayDescriptor Descriptor, int[] Index)
        {
            if (VariableType != ZXVariableType.Array)
                throw new InvalidCastException();

            if (Index == null || Index.Length != Descriptor.Dimensions)
                throw new ArgumentOutOfRangeException();

            return ZXVariableHelper.GetArrayValue(Memory, Descriptor, StorageType, Index);
        }
        public bool SetArrayValue(IMemory Memory, IZ80Registers Registers, int[] Index, object Value)
        {
            var descriptor = GetArrayDescriptor(Memory, Registers);

            if (descriptor == null)
                return false;

            ZXVariableHelper.SetArrayValue(Memory, descriptor, StorageType, Index, Value);

            return true;
        }
        public bool SetArrayValue(IMemory Memory, ZXArrayDescriptor Descriptor, int[] Index, object Value)
        {
            ZXVariableHelper.SetArrayValue(Memory, Descriptor, StorageType, Index, Value);

            return true;
        }
    }

    public class ZXVariableScope
    {
        public static ZXVariableScope GlobalScope = new ZXVariableScope { ScopeName = "GLOBAL", StartAddress = 0, EndAddress = 0xFFFF };
        public required string ScopeName { get; set; }
        public required ushort StartAddress { get; set; }
        public required ushort EndAddress { get; set; }

        public bool InRange(ushort Address)
        {
            return Address >= StartAddress && Address <= EndAddress;
        }
    }

    public class ZXVariableAddress
    {
        public required ZXVariableAddressType AddressType { get; set; }
        public required int AddressValue { get; set; }
    }

    public enum ZXVariableAddressType
    {
        Absolute,
        Relative
    }

    public enum ZXVariableType
    {
        Flat,
        Array
    }
    public enum ZXVariableStorage
    {
        I8,
        I16,
        I32,
        U8,
        U16,
        U32,
        F16,
        F,
        STR,
        LA8,
        LA16,
        LA32,
        LAF
    }
}
