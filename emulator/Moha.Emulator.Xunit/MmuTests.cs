using FluentAssertions;
using Moha.Emulator.Moxie;
using System;
using Xunit;
using static LanguageExt.Prelude;

namespace Moha.Emulator.Xunit
{
    public class MmuTests
    {
        const int MemorySize = 128 * 1024;
        const uint DestinationOffset = MemorySize / 2;
        const int DestinationIndex = (int)DestinationOffset / 2;
        readonly ReadOnlyMemory<byte> SampleData = new byte[] { 0xFF, 0xFF, 0xFF, 0xDE, 0xAD, 0xBE, 0xEF, 0xFF };
        readonly Mmu _mmu;

        Mmu CreateMmu()
        {
            var mmu = new Mmu(MemorySize);
            mmu.CopyToPhysical(DestinationOffset, SampleData.Span);
            return mmu;
        }

        public MmuTests()
        {
            _mmu = CreateMmu();
        }

        [ReadableFact("GetByte: works")]
        public void GetByteWorks()
        {
            var getByte = fun((uint offset) => _mmu.GetByte(DestinationOffset + offset));

            getByte(3).Should().Be(0xDE);
            getByte(4).Should().Be(0xAD);
            getByte(5).Should().Be(0xBE);
            getByte(6).Should().Be(0xEF);
        }

        [Theory(DisplayName = "GetShort works")]
        [InlineData(3, 0xADDE)]
        [InlineData(4, 0xBEAD)]
        public void GetShortUnaligned(uint offset, ushort result)
        {
            _mmu.GetShort(DestinationOffset + offset).Should().Be(result);
        }

        [ReadableFact("GetShort: throws when range exceeded by 1 byte")]
        public void GetShortThrowsWhenOutOfRange()
        {
            var action = fun(() => _mmu.GetShort(MemorySize - 1));
            action.Should().Throw<MemoryAccessException>();
        }

        [Theory(DisplayName = "GetLong: works")]
        [InlineData(3, 0xEFBEADDE)]
        [InlineData(4, 0xFFEFBEAD)]
        public void GetLongWorks(uint offset, uint result)
        {
            _mmu.GetLong(DestinationOffset + offset).Should().Be(result);
        }

        [Theory(DisplayName = "GetLong: throws when range exceeded by 1-3 bytes")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void GetLongThrowsWhenOutOfRange(uint exceededBy)
        {
            var action = fun(() => _mmu.GetLong(MemorySize - exceededBy));
            action.Should().Throw<MemoryAccessException>();
        }

        const uint ExampleStoreAddress = 259;

        [ReadableFact("StoreByte: stored value can be retrieved")]
        public void StoreByteValueCanBeRetrieved()
        {
            _mmu.StoreByte(ExampleStoreAddress, 0xCA);
            _mmu.StoreByte(ExampleStoreAddress + 1, 0xFE);

            _mmu.GetByte(ExampleStoreAddress).Should().Be(0xCA);
            _mmu.GetByte(ExampleStoreAddress + 1).Should().Be(0xFE);
        }

        [ReadableFact("StoreShort: stored value can be retrieved")]
        public void StoreShortValueCanBeRetrieved()
        {
            _mmu.StoreShort(ExampleStoreAddress, 0xBABE);
            _mmu.GetShort(ExampleStoreAddress);
        }

        [ReadableFact("StoreLong: stored value can be retrieved")]
        public void StoreLongValueCanBeRetrieved()
        {
            _mmu.StoreLong(ExampleStoreAddress, 0xDEADBAAD);
            _mmu.GetLong(ExampleStoreAddress).Should().Be(0xDEADBAAD);
        }
    }
}
