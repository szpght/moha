using System;
using System.Collections.Generic;
using System.Text;

namespace Moha.Emulator.Moxie
{
    interface IExecutionTracing
    {
        void Initialize();
        void TraceCall(in Instruction instruction, int ip);
    }
}
