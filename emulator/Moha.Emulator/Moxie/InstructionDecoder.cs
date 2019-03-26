namespace Moha.Emulator.Moxie
{
    class InstructionDecoder
    {
        public Instruction Decode(ushort encodedInstruction)
        {
            var highestBits = encodedInstruction & 0xC000;
            if (highestBits == 0xC000)
            {
                var opcode = encodedInstruction & 0xFC00;
                var value = encodedInstruction & 0x3FF;
                if (value > 511)
                {
                    value -= 1024;
                }
                return new Instruction(opcode, value);
            }
            else if (highestBits == 0x8000)
            {
                var opcode = encodedInstruction & 0xF000;
                var register = (encodedInstruction >> 8) & 0xF;
                var value = encodedInstruction & 0xFF;
                return new Instruction(opcode, register, 0, value);
            }
            else
            {
                var opcode = encodedInstruction & 0xFF00;
                var registerA = (encodedInstruction >> 4) & 0xF;
                var registerB = encodedInstruction & 0xF;
                return new Instruction(opcode, registerA, registerB);
            }
        }
    }
}
