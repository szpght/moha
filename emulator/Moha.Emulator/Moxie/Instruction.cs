namespace Moha.Emulator.Moxie
{
    struct Instruction
    {
        public readonly Opcode Opcode;
        public readonly int RegisterA;
        public readonly int RegisterB;
        public readonly int Value;

        public Instruction(int opcode, int registerA, int registerB, int value)
        {
            Opcode = (Opcode) opcode;
            RegisterA = registerA;
            RegisterB = registerB;
            Value = value;
        }

        public Instruction(int opcode, int value) : this(opcode, -1, -1, value)
        { }

        public Instruction(int opcode, int registerA, int registerB) : this(opcode, registerA, registerB, 0)
        { }
    }
}
