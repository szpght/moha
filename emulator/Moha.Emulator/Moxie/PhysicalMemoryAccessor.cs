using System.Runtime.CompilerServices;

namespace Moha.Emulator.Moxie
{
    readonly struct PhysicalMemoryAccessor : IMemoryAccessor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetByte(Mmu mmu, uint address)
            => mmu.GetByte(address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetLong(Mmu mmu, uint address)
            => mmu.GetLong(address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetShort(Mmu mmu, uint address)
            => mmu.GetShort(address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StoreByte(Mmu mmu, uint address, byte value)
            => mmu.StoreByte(address, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StoreLong(Mmu mmu, uint address, uint value)
            => mmu.StoreLong(address, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StoreShort(Mmu mmu, uint address, ushort value)
            => mmu.StoreShort(address, value);
    }
}
