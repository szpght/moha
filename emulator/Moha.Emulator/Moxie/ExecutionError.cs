using System.ComponentModel;

namespace Moha.Emulator.Moxie
{
    enum ExecutionError
    {
        // TODO use separate class for such error with fields with all context
        [Description("Illegal opcode")]
        IllegalOpcode,
    }
}
