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
            if (address >= Size) throw new IndexOutOfRangeException();
            unsafe
            {
                fixed (ushort* x = &_memory[0])
                {
                    return *((byte*)x + address);
                }
            }
        }

        public ushort GetShort(uint address)
        {
            if (address > Size - 2) throw new IndexOutOfRangeException();
            unsafe
            {
                fixed (ushort* x = &_memory[0])
                {
                    return *(ushort*)((byte*)x + address);
                }
            }
        }

        public uint GetLong(uint address)
        {
            if (address > Size - 4) throw new IndexOutOfRangeException();
            unsafe
            {
                fixed (ushort* x = &_memory[0])
                {
                    return *(uint*)((byte*)x + address);
                }
            }
        }

        public void StoreByte(uint address, byte value)
        {
            if (address >= Size) throw new IndexOutOfRangeException();
            unsafe
            {
                fixed(ushort* x = &_memory[0])
                {
                    *(((byte*)x) + address) = value;
                }
            }
        }

        public void StoreShort(uint address, ushort value)
        {
            if (address > Size - 2) throw new IndexOutOfRangeException();
            unsafe
            {
                fixed (ushort* x = &_memory[0])
                {
                    *(ushort*)(((byte*)x) + address) = value;
                }
            }
        }

        public void StoreLong(uint address, uint value)
        {
            if (address > Size - 4) throw new IndexOutOfRangeException();
            unsafe
            {
                fixed (ushort* x = &_memory[0])
                {
                    *(uint*)(((byte*)x) + address) = value;
                }
            }
        }

        /// <summary>
        /// This method works on memory aligned to short boundary
        /// </summary>
        /// <param name="index">Index in _memory array where requested long starts</param>
        /// <returns></returns>
        public uint GetLongAtIndex(int index)
        {
            return GetLong((uint)index * 2);
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
