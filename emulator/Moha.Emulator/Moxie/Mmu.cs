﻿using System;
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
        readonly ushort[] _memory;
        private uint _pageDirectory;
        private readonly TlbEntry[] _tlb = new TlbEntry[1024];

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
                return *NotCheckedGetMemoryAtPhysical(address);
            }
        }

        public ushort GetShort(uint address)
        {
            if (address > Size - 2) throw new IndexOutOfRangeException();
            unsafe
            {
                return *(ushort*)NotCheckedGetMemoryAtPhysical(address);
            }
        }

        public uint GetLong(uint address)
        {
            if (address > Size - 4) throw new IndexOutOfRangeException();
            unsafe
            {
                return *(uint*)NotCheckedGetMemoryAtPhysical(address);
            }
        }

        public void StoreByte(uint address, byte value)
        {
            if (address >= Size) throw new IndexOutOfRangeException();
            unsafe
            {
                *NotCheckedGetMemoryAtPhysical(address) = value;
            }
        }

        public void StoreShort(uint address, ushort value)
        {
            if (address > Size - 2) throw new IndexOutOfRangeException();
            unsafe
            {
                *(ushort*)NotCheckedGetMemoryAtPhysical(address) = value;
            }
        }

        public void StoreLong(uint address, uint value)
        {
            if (address > Size - 4) throw new IndexOutOfRangeException();
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
                tlbEntry = WalkPageTables();
            }

            // TODO handle page boundary
            // TODO handle page ro/rw
            _tlb[index] = tlbEntry;
            return NotCheckedGetMemoryAtPhysical(tlbEntry.PagePhysicalAddress + offset);
        }

        private TlbEntry WalkPageTables()
        {
            // TODO walk page tables, fill tlb
            // TODO check if physical address do not exceed range
            throw new IndexOutOfRangeException("Invalid memory access");
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
    }
}
