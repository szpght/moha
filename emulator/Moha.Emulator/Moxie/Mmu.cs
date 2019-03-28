using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
            return GetByteSpan(address)[0];
        }

        public ushort GetShort(uint address)
        {
            return GetShortSpan(address)[0];
        }

        public uint GetLong(uint address)
        {
            return GetLongSpan(address)[0];
        }

        public void StoreByte(uint address, byte value)
        {
            GetByteSpan(address)[0] = value;
        }

        public void StoreShort(uint address, ushort value)
        {
            GetShortSpan(address)[0] = value;
        }

        public void StoreLong(uint address, uint value)
        {
            GetLongSpan(address)[0] = value;
        }

        /// <summary>
        /// This method works on memory aligned to short boundary
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

        private Span<byte> GetByteSpan(uint address)
        {
            var index = (int)(address / 2);
            var ushortMemory = _memory.AsSpan(index);
            var byteMemory = MemoryMarshal.Cast<ushort, byte>(ushortMemory);
            var memoryIndex = address % 2;
            return byteMemory.Slice((int)memoryIndex);
        }

        private Span<ushort> GetShortSpan(uint address)
        {
            return MemoryMarshal.Cast<byte, ushort>(GetByteSpan(address));
        }

        private Span<uint> GetLongSpan(uint address)
        {
            return MemoryMarshal.Cast<byte, uint>(GetByteSpan(address));
        }
    }
}
