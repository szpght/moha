using System.Runtime.CompilerServices;

namespace Moha.Emulator.Moxie
{
    readonly struct VirtualMemoryAccessor : IMemoryAccessor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetByte(Mmu mmu, uint address)
            => mmu.GetVirtualByte(address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetLong(Mmu mmu, uint address)
            => mmu.GetVirtualLong(address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetShort(Mmu mmu, uint address)
            => mmu.GetVirtualShort(address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StoreByte(Mmu mmu, uint address, byte value)
            => mmu.StoreVirtualByte(address, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StoreLong(Mmu mmu, uint address, uint value)
            => mmu.StoreVirtualLong(address, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StoreShort(Mmu mmu, uint address, ushort value)
            => mmu.StoreVirtualShort(address, value);
    }
}
