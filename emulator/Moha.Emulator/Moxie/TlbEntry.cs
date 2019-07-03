namespace Moha.Emulator.Moxie
{
    struct TlbEntry
    {
        public uint tag;
        public uint entry;

        public uint PagePhysicalAddress() => entry & 0xFFFFFC00;
        public bool Rw() => (entry & 2) != 0;

        public static uint TagFromAddress(uint virtualAddress) => virtualAddress >> 12;
        public static int IndexFromTag(uint tag) => (int)(tag & 1023);
        public static uint OffsetFromAddress(uint address) => address & 1023;
    }
}
