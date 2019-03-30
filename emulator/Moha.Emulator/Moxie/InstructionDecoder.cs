using System.Runtime.CompilerServices;

namespace Moha.Emulator.Moxie
{
    class InstructionDecoder
    {
        public Instruction Decode(ushort encodedInstruction)
        {
            var shiftedForOpcodeExtraction = encodedInstruction >> 8;
            if ((encodedInstruction & 0xC000) == 0)
            {
                var opcode = shiftedForOpcodeExtraction & 0xFF;
                var registerA = encodedInstruction >> 4;
                var registerB = encodedInstruction;
                return new Instruction(opcode, registerA, registerB);
            }
            var highestBits = encodedInstruction & 0xC000;
            if (highestBits == 0xC000)
            {
                var opcode = shiftedForOpcodeExtraction & 0xFC;
                var value = encodedInstruction & 0x3FF;
                if (value > 511)
                {
                    value -= 1024;
                }
                return new Instruction(opcode, value);
            }
            else
            {
                var opcode = shiftedForOpcodeExtraction & 0xF0;
                var register = encodedInstruction >> 8;
                var value = encodedInstruction & 0xFF;
                return new Instruction(opcode, register, 0, value);
            }
        }
    }
}
