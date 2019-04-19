namespace Moha.Emulator.Moxie
{
    enum MemoryAccessStatus
    {
        Ok,
        PageDirectoryEntryNotPresent,
        PageNotPresent,
        PageReadOnly,
        PhysicalAddressOutOfRange,
    }
}
