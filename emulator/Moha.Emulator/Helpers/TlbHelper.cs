using Moha.Emulator.Moxie;
using System;

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
    }
}
