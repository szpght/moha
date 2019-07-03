using FluentAssertions;
using Moha.Emulator.Helpers;
using Moha.Emulator.Moxie;

namespace Moha.Emulator.Xunit
{
    public class VirtualMemoryTests
    {
        const int MemorySize = 85000;
        readonly Mmu _mmu = new Mmu(MemorySize);

        [ReadableFact("Byte from virtual address with empty TLB can be read")]
        public void ByteFromVirtualAddressWithEmptyTlbCanBeRead()
        {
            uint testDataVirtualAddress = 65536;

            var (_, _, pdEntryForPt) = TlbHelper.CreatePageTableEntry(0, 4096);
            _mmu.CopyToPhysical(0, pdEntryForPt);

            var (_, ptIndex, ptEntry) = TlbHelper.CreatePageTableEntry(testDataVirtualAddress, 8192);
            uint ptEntryAddress =(uint)(4096 + ptIndex * 4);
            _mmu.CopyToPhysical(ptEntryAddress, ptEntry);

            var sampleData = new byte[] { 66, 33 };
            _mmu.CopyToPhysical(8192, sampleData);

            _mmu.GetVirtualByte(testDataVirtualAddress).Should().Be(66);
            _mmu.GetVirtualByte(testDataVirtualAddress + 1).Should().Be(33);
        }
    }
}
