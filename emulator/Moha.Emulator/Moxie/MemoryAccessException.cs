using System;

namespace Moha.Emulator.Moxie
{
    class MemoryAccessException : Exception
    {
        public MemoryAccessStatus Reason { get; }

        public MemoryAccessException(MemoryAccessStatus reason)
        {
            Reason = reason;
        }
    }
}
