using System.Diagnostics;

namespace Moha.Emulator.Moxie
{
    [DebuggerDisplay("{ToString(),nq}")]
    readonly struct Instruction
    {
        public readonly Opcode Opcode;
        public readonly int RegisterA;
        public readonly int RegisterB;
        public readonly int Value;

        public Instruction(int opcode, int registerA, int registerB, int value)
        {
            Opcode = (Opcode) opcode;
            RegisterA = registerA & 0xF;
            RegisterB = registerB & 0xF;
            Value = value;
        }

        public Instruction(int opcode, int value) : this(opcode, 0, 0, value)
        { }

        public Instruction(int opcode, int registerA, int registerB) : this(opcode, registerA, registerB, 0)
        { }

        public override string ToString() => $"{Opcode}({(Register)RegisterA}, {(Register)RegisterB}, {Value})";
    }
}
