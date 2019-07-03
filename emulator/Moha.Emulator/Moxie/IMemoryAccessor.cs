namespace Moha.Emulator.Moxie
{
    interface IMemoryAccessor
    {
        byte GetByte(Mmu mmu, uint address);
        uint GetLong(Mmu mmu, uint address);
        ushort GetShort(Mmu mmu, uint address);
        void StoreByte(Mmu mmu, uint address, byte value);
        void StoreLong(Mmu mmu, uint address, uint value);
        void StoreShort(Mmu mmu, uint address, ushort value);
    }
}