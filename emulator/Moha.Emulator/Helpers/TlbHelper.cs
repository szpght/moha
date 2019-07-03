using Moha.Emulator.Moxie;
using System;
using System.Linq;

namespace Moha.Emulator.Helpers
{
    static class TlbHelper
    {
        internal static (int index, TlbEntry entry) CreateTlbEntry(uint virtualAddress,
            uint physicalAddress, bool rw = true, bool supervisor = false)
        {
            var entry = physicalAddress & 0xFFFFFC00;
            entry |= rw ? PageTableFlags.Rw : 0;
            entry |= supervisor ? PageTableFlags.Supervisor : 0;
            var tag = TlbEntry.TagFromAddress(virtualAddress);
            var tlbEntry = new TlbEntry
            {
                entry = entry,
                tag = tag,
            };
            var index = TlbEntry.IndexFromTag(tag);
            return (index, tlbEntry);
        }

        internal static (int pageDirectoryIndex, int pageTableindex, byte[] entry) CreatePageTableEntry(
            uint virtualAddress, uint physicalAddress, bool rw = true, bool supervisor = false)
        {
            var pageDirectoryIndex = virtualAddress >> 22;
            var pageTableIndex = (virtualAddress >> 12) & 1023;
            physicalAddress = physicalAddress & 0xFFFFFC00;
            physicalAddress |= PageTableFlags.Present;
            physicalAddress |= rw ? PageTableFlags.Rw : 0;
            physicalAddress |= supervisor ? PageTableFlags.Supervisor : 0;
            return ((int)pageDirectoryIndex, (int)pageTableIndex, BitConverter.GetBytes(physicalAddress));
        }

        internal static (uint location, byte[] data) PrepareIdentityMap(int memorySize)
        {
            // this doesnt work for memory sizes where one page doesnt cover whole memory
            var memoryAreaCoveredByTable = 4096 * 1024;
            var tablesNeeded = memorySize / memoryAreaCoveredByTable + 1;
            var entries = new uint[tablesNeeded * 1024];
            var location = memorySize - 4096 * tablesNeeded;
            for (int i = 0, addr = location + 4096; i < tablesNeeded - 1; i++, addr += 4096)
            {
                entries[i] = (uint) addr | PageTableFlags.Present | PageTableFlags.Rw;
            }

            for (int i = 1024, addr = 0; addr < location; i++, addr += 4096)
            {
                entries[i] = (uint) addr | PageTableFlags.Present | PageTableFlags.Rw;
            }

            var data = entries.Select(BitConverter.GetBytes).SelectMany(x => x).ToArray();
            return ((uint)location, data);
        }
    }
}
