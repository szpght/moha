using FluentAssertions;
using Moha.Emulator.Helpers;
using Moha.Emulator.Moxie;
using System;

namespace Moha.Emulator.Xunit
{
    public class VirtualMemoryTests
    {
        const int MemorySize = 4 * 1024 * 1024;
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

        [ReadableFact("Generated identity mapping allows to read and write values")]
        public void IdentityMaptTest()
        {
            var (location, data) = TlbHelper.PrepareIdentityMap(MemorySize);
            _mmu.CopyToPhysical(location, data);
            _mmu._pageDirectory = location;
            var storeRand = new Random(0);
            var getRand = new Random(0);

            for (uint address = 0; address < location; address += 4)
            {
                var value = (uint)storeRand.Next();
                _mmu.StoreVirtualLong(address, value);
                var gotValue = _mmu.GetVirtualLong(address);
                gotValue.Should().Be(value, $"fail at first pass, address {address}");
            }

            for (uint address = 0; address < location; address += 4)
            {
                var expectedValue = (uint)getRand.Next();
                var value = _mmu.GetVirtualLong(address);
                value.Should().Be(expectedValue, $"fail at second pass, address {address}");
            }
        }
    }
}
