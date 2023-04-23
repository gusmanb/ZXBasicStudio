using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus;

namespace ZXBasicStudio.Classes
{
    public static class ZXVariableHelper
    {
        const double fixedPrecission = 0.0000152587890625;

        public static object GetArrayValue(IMemory Memory, ZXArrayDescriptor Descriptor, ZXVariableStorage StorageType, int[] Index)
        {
            int offset = 0;

            for (int buc = 0; buc < Index.Length; buc++)
                offset += Index[buc] * Descriptor.DimensionStrides[buc];

            ushort valueAddress = (ushort)(Descriptor.StorageAddress + offset);
            return ZXVariableHelper.GetValue(Memory, valueAddress, ZXVariableType.Flat, StorageType);
        }

        public static bool SetArrayValue(IMemory Memory, ZXArrayDescriptor Descriptor, ZXVariableStorage StorageType, int[] Index, object Value)
        {
            int offset = 0;

            for (int buc = 0; buc < Index.Length; buc++)
                offset += Index[buc] * Descriptor.DimensionStrides[buc];

            ushort valueAddress = (ushort)(Descriptor.StorageAddress + offset);
            return ZXVariableHelper.SetValue(Memory, valueAddress, ZXVariableType.Flat, StorageType, Value);
        }

        public static ZXArrayDescriptor GetArrayDescriptor(IMemory Memory, ushort PointerAddress, ushort StorageAddress, int StorageSize)
        {

            ZXArrayDescriptor descriptor = new ZXArrayDescriptor();

            byte[] tmpData = Memory.GetContents(PointerAddress, 2);
            ushort descPtr = BitConverter.ToUInt16(tmpData);
            tmpData = Memory.GetContents(StorageAddress, 2);
            ushort storagePtr = BitConverter.ToUInt16(tmpData);
            ushort storageSize = (ushort)StorageSize;

            descriptor.DescriptorAddress = descPtr;
            descriptor.StorageAddress = storagePtr;

            tmpData = Memory.GetContents(descPtr, 2);
            ushort dims = BitConverter.ToUInt16(tmpData);
            int realDims = dims + 1;
            descPtr += 2;

            //Restricted to avoid problems with uninitialized arrays (nobody is going to have more than 65 dimensions on an spectrum array... :P)
            if (dims > 64)
                dims = 64;

            List<ushort> dimSizes = new List<ushort>();
            for (int buc = 0; buc < dims; buc++)
            {
                tmpData = Memory.GetContents(descPtr, 2);
                descPtr += 2;
                ushort dimSize = BitConverter.ToUInt16(tmpData);
                dimSizes.Add(dimSize);
            }
            byte elemSize = Memory.GetContents(descPtr, 1)[0];

            //To avoid crashes with uninitialized arrays
            if (elemSize == 0)
                elemSize = 1;

            var elementCount = storageSize / elemSize;

            foreach (var dimSize in dimSizes)
                elementCount /= dimSize == 0 ? 1 : dimSize; //To avoid crashes with uninitialized arrays

            dimSizes.Insert(0, (ushort)elementCount);
            descriptor.Dimensions = realDims;
            descriptor.DimensionSizes = dimSizes.ToArray();
            descriptor.ElementSize = elemSize;

            List<int> strides = new List<int>();

            for (int buc = 0; buc < dimSizes.Count - 1; buc++)
            {
                int stride = dimSizes[buc + 1];

                for (int innerBuc = buc + 2; innerBuc < dimSizes.Count; innerBuc++)
                    stride *= dimSizes[innerBuc];

                stride *= elemSize;

                strides.Add(stride);
            }

            strides.Add(elemSize);

            descriptor.DimensionStrides = strides.ToArray();

            return descriptor;
        }

        public static ZXArrayDescriptor GetArrayDescriptor(IMemory Memory, ushort Address)
        {

            ZXArrayDescriptor descriptor = new ZXArrayDescriptor();

            byte[] tmpData = Memory.GetContents(Address, 2);
            ushort descPtr = BitConverter.ToUInt16(tmpData);
            tmpData = Memory.GetContents(Address + 2, 2);
            ushort storagePtr = BitConverter.ToUInt16(tmpData);
            ushort storageSize = (ushort)(descPtr - storagePtr);

            descriptor.DescriptorAddress = descPtr;
            descriptor.StorageAddress = storagePtr;

            tmpData = Memory.GetContents(descPtr, 2);
            ushort dims = BitConverter.ToUInt16(tmpData);
            int realDims = dims + 1;
            descPtr += 2;
            List<ushort> dimSizes = new List<ushort>();
            for(int buc = 0; buc < dims; buc++) 
            {
                tmpData = Memory.GetContents(descPtr, 2);
                descPtr += 2;
                ushort dimSize = BitConverter.ToUInt16(tmpData);
                dimSizes.Add(dimSize);
            }
            byte elemSize = Memory.GetContents(descPtr, 1)[0];

            var elementCount = storageSize / elemSize;

            foreach (var dimSize in dimSizes)
                elementCount /= dimSize;

            dimSizes.Insert(0, (ushort)elementCount);
            descriptor.Dimensions = realDims;
            descriptor.DimensionSizes = dimSizes.ToArray();
            descriptor.ElementSize = elemSize;

            List<int> strides = new List<int>();

            for (int buc = 0; buc < dimSizes.Count - 1; buc++)
            {
                int stride = dimSizes[buc + 1];

                for (int innerBuc = buc + 2; innerBuc < dimSizes.Count; innerBuc++)
                    stride *= dimSizes[innerBuc];

                stride *= elemSize;

                strides.Add(stride);
            }

            strides.Add(elemSize);

            descriptor.DimensionStrides = strides.ToArray();

            return descriptor;
        }
        public static object GetValue(IMemory Memory, ushort Address, ZXVariableType VariableType, ZXVariableStorage StorageType)
        {
            if (VariableType == ZXVariableType.Flat)
            {
                switch (StorageType)
                {
                    case ZXVariableStorage.I8:
                        return (sbyte)Memory[Address];
                    case ZXVariableStorage.U8:
                        return Memory[Address];
                    case ZXVariableStorage.I16:
                        return BitConverter.ToInt16(Memory.GetContents(Address, 2));
                    case ZXVariableStorage.U16:
                        return BitConverter.ToUInt16(Memory.GetContents(Address, 2));
                    case ZXVariableStorage.I32:
                        return BitConverter.ToInt32(Memory.GetContents(Address, 4));
                    case ZXVariableStorage.U32:
                        return BitConverter.ToUInt32(Memory.GetContents(Address, 4));
                    case ZXVariableStorage.F16:
                        var bytes = Memory.GetContents(Address, 4);
                        //TODO: Ask for -0.x case
                        var integral = BitConverter.ToInt16(bytes, 2);
                        var decim = fixedPrecission * BitConverter.ToUInt16(bytes, 0) * Math.Sign(integral);
                        return integral + decim;
                    case ZXVariableStorage.F:
                    case ZXVariableStorage.LAF:
                        return GetFloat40(Memory.GetContents(Address, 5));
                    case ZXVariableStorage.STR:
                        return GetString(BitConverter.ToUInt16(Memory.GetContents(Address, 2)), Memory);
                    case ZXVariableStorage.LA8:
                        return new ZXLocalArrayValue((sbyte)Memory[Address], Memory[Address]);
                    case ZXVariableStorage.LA16:
                        return new ZXLocalArrayValue(BitConverter.ToInt16(Memory.GetContents(Address, 2)), BitConverter.ToUInt16(Memory.GetContents(Address, 2)));
                    case ZXVariableStorage.LA32:
                        return new ZXLocalArrayValue(BitConverter.ToInt32(Memory.GetContents(Address, 4)), BitConverter.ToUInt32(Memory.GetContents(Address, 4)));
                }

                throw new InvalidCastException();
            }
            else
                return BitConverter.ToUInt16(Memory.GetContents(Address, 2));
        }

        public static bool SetValue(IMemory Memory, ushort Address, ZXVariableType VariableType, ZXVariableStorage StorageType, object Value)
        {
            if (VariableType == ZXVariableType.Flat)
            {
                switch (StorageType)
                {
                    case ZXVariableStorage.I8:
                        {
                            sbyte value = (sbyte)Value;
                            Memory[Address] = unchecked((byte)value);
                        }
                        break;
                    case ZXVariableStorage.U8:
                    case ZXVariableStorage.LA8:
                        Memory[Address] = (byte)Value;
                        break;
                    case ZXVariableStorage.I16:
                        {
                            short value = (short)Value;
                            byte[] data = BitConverter.GetBytes(value);
                            Memory.SetContents(Address, data);
                        }
                        break;
                    case ZXVariableStorage.U16:
                    case ZXVariableStorage.LA16:
                        {
                            ushort value = (ushort)Value;
                            byte[] data = BitConverter.GetBytes(value);
                            Memory.SetContents(Address, data);
                        }
                        break;
                    case ZXVariableStorage.I32:
                        {
                            int value = (int)Value;
                            byte[] data = BitConverter.GetBytes(value);
                            Memory.SetContents(Address, data);
                        }
                        break;
                    case ZXVariableStorage.U32:
                    case ZXVariableStorage.LA32:
                        {
                            uint value = (uint)Value;
                            byte[] data = BitConverter.GetBytes(value);
                            Memory.SetContents(Address, data);
                        }
                        break;
                    case ZXVariableStorage.F16:
                        {
                            float underVal = (float)Value;

                            short intVal = (short)Math.Truncate(underVal);
                            double decVal = underVal - Math.Truncate(underVal);
                            ushort fixedVal = (ushort)Math.Truncate(underVal);

                            List<byte> buffer = new List<byte>();
                            buffer.AddRange(BitConverter.GetBytes(fixedVal));
                            buffer.AddRange(BitConverter.GetBytes(intVal));

                            Memory.SetContents(Address, buffer.ToArray());
                        }
                        break;
                    case ZXVariableStorage.F:
                    case ZXVariableStorage.LAF:
                        {
                            byte[]? data = GetBytesFloat40((double)Value);
                            if (data == null)
                                return false;
                            Memory.SetContents(Address, data);
                        }
                        break;
                    case ZXVariableStorage.STR:
                        {
                            string val = (string)Value;
                            SetString(Address, Memory, val);
                        }
                        break;
                }

                return false;
            }
            else
            {
                short value = (short)Value;
                byte[] data = BitConverter.GetBytes(value);
                Memory.SetContents(Address, data);
            }

            return true;
        }

        public static double GetFloat40(byte[] Data)
        {
            double result = 0;
            int exp = Data[0] - 0x80;
            bool signed = (Data[1] & 128) != 0;
            uint mantissa = ((((uint)Data[1] & 0x7f) | 128) << 24) | ((uint)Data[2] << 16) | ((uint)Data[3] << 8) | (uint)Data[4];
            result = Math.Pow(2.0, exp) * (mantissa / 4294967296.0);

            if (signed)
                result *= -1;

            return result;
        }

        public static byte[]? GetBytesFloat40(double Value) 
        {

            if (Value == 0)
                return new byte[5];

            var exp = Math.Ceiling(Math.Log2(Math.Abs(Value)));

            if (exp < -128 || exp > 127)
                return null;

            byte sign = Math.Sign(Value) == -1 ? (byte)0x80 : (byte)0;
            var val = Math.Abs(Value);
            var mantissa = (uint)(Math.Truncate((val / Math.Pow(2.0, exp)) * 4294967296.0));
            byte[] data = new byte[5];
            data[0] = (byte)(exp + 0x80);
            data[1] = (byte)(((mantissa >> 24) & 0x7F) | sign);
            data[2] = (byte)((mantissa >> 16) & 0xFF);
            data[3] = (byte)((mantissa >> 8) & 0xFF);
            data[4] = (byte)(mantissa & 0xFF);

            return data;
        }

        public static string GetString(ushort StringAddress, IMemory Memory)
        {
            if (StringAddress == 0)
                return "{null}";

            ushort len = BitConverter.ToUInt16(Memory.GetContents(StringAddress, 2));
            byte[] strData = Memory.GetContents(StringAddress + 2, len);
            return Encoding.ASCII.GetString(strData);
        }

        public static void SetString(ushort StringAddress, IMemory Memory, string NewValue) 
        {
            ushort len = BitConverter.ToUInt16(Memory.GetContents(StringAddress, 2));
            List<byte> tmpBuf = new List<byte>();
            tmpBuf.AddRange(Encoding.ASCII.GetBytes(NewValue));

            if (tmpBuf.Count != len)
            {
                if (tmpBuf.Count < len)
                {
                    byte[] pad = new byte[len - tmpBuf.Count];
                    Array.Fill(pad, (byte)0x20);
                    tmpBuf.AddRange(pad);
                }
                else
                {
                    tmpBuf.RemoveRange(len, tmpBuf.Count - len);
                }
            }

            Memory.SetContents(StringAddress + 2, tmpBuf.ToArray());
        }
    }

    public class ZXArrayDescriptor
    {
        public ushort DescriptorAddress { get; set; }
        public ushort StorageAddress { get; set; }
        public int Dimensions { get; set; }
        public ushort[] DimensionSizes { get; set; }
        public int[] DimensionStrides { get; set; }
        public byte ElementSize { get; set; }
    }

    public class ZXLocalArrayValue
    {
        public ZXLocalArrayValue(object SignedValue, object UnsignedValue) 
        {
            this.SignedValue= SignedValue;
            this.UnsignedValue= UnsignedValue;
        }
        public object SignedValue { get; set; }
        public object UnsignedValue { get; set; }

        public override string ToString()
        {
            return $"{SignedValue} / {UnsignedValue}";
        }
    }
}
