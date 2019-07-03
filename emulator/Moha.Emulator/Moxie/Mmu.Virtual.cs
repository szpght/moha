﻿using Moha.Emulator.Helpers;

namespace Moha.Emulator.Moxie
{
    partial class Mmu
    {
		private void InitializeTlb()
        {
			// since size of tag is 20 bits, highest 12 bits are always 0
            // so we can signal emtpy slot with one of them being 1
            // this way we check tag and page presentness in one comparison
            for (int i = 0; i < _tlb.Length; i++)
            {
                _tlb[i].tag = uint.MaxValue;
            }

            var (index, entry) = TlbHelper.CreateTlbEntry(4096, 8192);
            _tlb[index] = entry;
        }

        public unsafe byte GetVirtualByte(uint address)
        {
            return *GetMemoryAtVirtual(address, false);
        }

        private unsafe byte* GetMemoryAtVirtual(uint address, bool write)
        {
            var tag = TlbEntry.TagFromAddress(address);
            var index = TlbEntry.IndexFromTag(tag);
            var offset = TlbEntry.OffsetFromAddress(address);
            var tlbEntry = _tlb[index];

            if (tlbEntry.tag != tag)
            {
                tlbEntry = WalkPageTables(address);
            }

			if (write && !tlbEntry.Rw())
            {
                ThrowPageReadOnlyException();
            }

            return NotCheckedGetMemoryAtPhysical(tlbEntry.PagePhysicalAddress() + offset);
        }

		private void ThrowPageReadOnlyException()
        {
            throw new MemoryAccessException(MemoryAccessStatus.PageReadOnly);
        }

        private TlbEntry WalkPageTables(uint address)
        {
            TlbEntry entry = default;

            var pageDirectoryIndex = address >> 22;
            var pageTableIndex = (address >> 12) & 1023;
            var pageDirectoryEntry = GetLongPhysical(_pageDirectory + pageDirectoryIndex * 4);
            if ((pageDirectoryEntry & PageTableFlags.Present) == 0)
            {
                throw new MemoryAccessException(MemoryAccessStatus.PageDirectoryEntryNotPresent);
            }

            // TODO use correct mask for pageDirectoryEntry
            var pageTableEntry = GetLongPhysical((pageDirectoryEntry & 0xFFFFFC00) + pageTableIndex * 4);
            if ((pageTableEntry & PageTableFlags.Present) == 0)
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
            _tlb[TlbEntry.IndexFromTag(entry.tag)] = entry;
            return entry;
        }
    }
}
