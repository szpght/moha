using System;
using System.Collections.Generic;

namespace Moha.Emulator.Moxie
{
    struct Tracing : IExecutionTracing
    {
        Dictionary<Opcode, long> _opcodeCounts;

        public void Initialize()
        {
            _opcodeCounts = new Dictionary<Opcode, long>();
        }

        public void TraceCall(in Instruction instruction, int ip)
        {
            var opcode = instruction.Opcode;
            long count;
            _opcodeCounts.TryGetValue(opcode, out count);
            count += 1;
            _opcodeCounts[opcode] = count;
            Console.WriteLine($"{ip * 2:X}: {instruction}");
        }

        public void TraceImmediate(uint value)
        {
            Console.WriteLine($"Immediate: {value}");
        }
    }
}
