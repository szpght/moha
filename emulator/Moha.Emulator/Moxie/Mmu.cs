using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Moha.Emulator.Moxie
{
    partial class Mmu
    {
        public Mmu(int size)
        {
            CheckAlignment(size, nameof(size));
            CheckMemorySize(size);
            _memory = new ushort[size / 2];
            Size = size;
            SizeMinus2 = size - 2;
            SizeMinus4 = size - 4;
            InitializeTlb();
        }

        public long Size { get; }

        private long SizeMinus2 { get; }
        private long SizeMinus4 { get; }
        readonly ushort[] _memory;
        private uint _pageDirectory;
        internal readonly TlbEntry[] _tlb = new TlbEntry[1024];

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
            if (address >= Size) OutOfRange();
            unsafe
            {
                return *NotCheckedGetMemoryAtPhysical(address);
            }
        }

        public ushort GetShort(uint address)
        {
            if (address > SizeMinus2) OutOfRange();
            unsafe
            {
                return *(ushort*)NotCheckedGetMemoryAtPhysical(address);
            }
        }

        public uint GetLong(uint address)
        {
            if (address > SizeMinus4) OutOfRange();
            unsafe
            {
                return *(uint*)NotCheckedGetMemoryAtPhysical(address);
            }
        }

        public void StoreByte(uint address, byte value)
        {
            if (address >= Size) OutOfRange();
            unsafe
            {
                *NotCheckedGetMemoryAtPhysical(address) = value;
            }
        }

        public void StoreShort(uint address, ushort value)
        {
            if (address > SizeMinus2) OutOfRange();
            unsafe
            {
                *(ushort*)NotCheckedGetMemoryAtPhysical(address) = value;
            }
        }

        public void StoreLong(uint address, uint value)
        {
            if (address > SizeMinus4) OutOfRange();
            unsafe
            {
                *(uint*)NotCheckedGetMemoryAtPhysical(address) = value;
            }
        }

        private void CheckAlignment(long offset, string name)
        {
            if (offset % 2 > 0)
            {
                throw new Exception($"{name} must be even");
            }
        }

        private uint GetLongPhysical(uint address)
        {
            unsafe
            {
                return *(uint*)NotCheckedGetMemoryAtPhysical(address);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe byte* NotCheckedGetMemoryAtPhysical(uint address)
        {
            fixed (ushort* x = &_memory[0])
            {
                return ((byte*)x) + address;
            }
        }

        private void CheckMemorySize(long size)
        {
            if (size < 85000)
            {
                throw new ArgumentOutOfRangeException("Memory size must be at least 85000 bytes so it lands on LOH");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void OutOfRange()
        {
            throw new MemoryAccessException(MemoryAccessStatus.PhysicalAddressOutOfRange);
        }
    }
}
