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
            CheckMemorySize(size);
            _memory = new ushort[size / 2];
            Size = size;
            SizeMinus2 = size - 2;
            SizeMinus4 = size - 4;
            InitializeTlb();
        }

        private void InitializeTlb()
        {
            // highest 12 bits of tag are always zeroed so we can 
            // signal emtpy slot with one of them being set
            // this way we check tag and page presentness in one comparison
            for (int i = 0; i < _tlb.Length; i++)
            {
                _tlb[i].tag = uint.MaxValue;
            }


            _tlb[1].tag = 1;
        }

        struct TlbEntry
        {
            public uint tag;
            public uint entry;

            public uint PagePhysicalAddress => entry & 0xFFFFFC00;
            public uint Rw => entry & 2;
        }

        public long Size { get; }

        private long SizeMinus2 { get; }
        private long SizeMinus4 { get; }
        readonly ushort[] _memory;
        private uint _pageDirectory;
        private readonly TlbEntry[] _tlb = new TlbEntry[1024];

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

        private unsafe byte* GetMemoryAtVirtual(uint address)
        {
            var tag = address >> 12;
            var index = tag & 1023;
            var offset = address & 1023;
            var tlbEntry = _tlb[index];

            if (tlbEntry.tag != tag)
            {
                tlbEntry = WalkPageTables(address);
            }

            // TODO handle page ro/rw

            _tlb[index] = tlbEntry;
            return NotCheckedGetMemoryAtPhysical(tlbEntry.PagePhysicalAddress + offset);
        }

        private TlbEntry WalkPageTables(uint address)
        {
            TlbEntry entry = default;

            var pageDirectoryIndex = address >> 22;
            var pageTableIndex = (address >> 12) & 1023;
            var pageDirectoryEntry = GetLongPhysical(_pageDirectory + pageDirectoryIndex * 4);
            if ((pageDirectoryEntry & 1) == 0)
            {
                throw new MemoryAccessException(MemoryAccessStatus.PageDirectoryEntryNotPresent);
            }

            // TODO use correct mask for pageDirectoryEntry
            var pageTableEntry = GetLongPhysical(pageDirectoryEntry - 1 + pageTableIndex * 4);
            if ((pageTableEntry & 1) == 0)
            {
                throw new MemoryAccessException(MemoryAccessStatus.PageNotPresent);
            }

            var pageTableEntryAddress = pageTableEntry & 0xFFFFFC00;
            if (pageTableEntryAddress >= Size)
            {
                throw new MemoryAccessException(MemoryAccessStatus.PhysicalAddressOutOfRange);
            }

            entry.entry = pageTableEntry;
            entry.tag = address >> 12;
            return entry;
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
            throw new IndexOutOfRangeException();
        }
    }
}
