using System;

namespace Moha.Emulator.Moxie
{
    static class PageTableFlags
    {
        public const uint Present = 1;
        public const uint Rw = 2;
        public const uint Supervisor = 4;
    }
}
