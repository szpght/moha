using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Moha.Emulator.Moxie
{
    class Mmu
    {
        public Mmu(int size)
        {
            CheckAlignment(size, nameof(size));
            _memory = new ushort[size / 2];
            Size = size;
        }

        public long Size { get; }
        readonly ushort[] _memory;

        public ushort this[int index] => _memory[index];

        public void CopyToPhysical(uint alignedAddress, ReadOnlySpan<byte> source)
        {
            CheckAlignment(alignedAddress, nameof(alignedAddress));
            CheckAlignment(source.Length, "source length");
            var index = alignedAddress / 2;
            var castedSource = MemoryMarshal.Cast<byte, ushort>(source);
            var destination = _memory.AsSpan((int)index);
            castedSource.CopyTo(destination);
        }

        public byte GetByte(uint address)
        {
            var index = (int)(address / 2);
            var ushortMemory = _memory.AsSpan(index);
            var byteMemory = MemoryMarshal.Cast<ushort, byte>(ushortMemory);
            var memoryIndex = address % 2;
            return byteMemory[(int)memoryIndex];
        }

        public ushort GetShort(uint address)
        {
            throw new NotImplementedException();
        }

        public uint GetLong(uint address)
        {
            throw new NotImplementedException();
        }

        public void StoreByte(uint address, byte value)
        {
            throw new NotImplementedException();
        }

        public void StoreShort(uint address, ushort value)
        {
            throw new NotImplementedException();
        }

        public void StoreLong(uint address, uint value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method works on memory aligned do short boundary
        /// </summary>
        /// <param name="index">Index in _memory array where requested long starts</param>
        /// <returns></returns>
        public uint GetLongAtIndex(int index)
        {
            var memoryAreaWithTheLong = _memory.AsSpan(index, 4);
            var castedMemory = MemoryMarshal.Cast<ushort, uint>(memoryAreaWithTheLong);
            return castedMemory[0];
        }

        public void CheckAlignment(long offset, string name)
        {
            if (offset % 2 > 0)
            {
                throw new Exception($"{name} must be even");
            }
        }
    }
}
