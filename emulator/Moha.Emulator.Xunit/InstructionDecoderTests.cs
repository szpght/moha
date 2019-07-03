using FluentAssertions;
using Moha.Emulator.Moxie;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Moha.Emulator.Xunit
{
    public class InstructionDecoderTests
    {
        readonly Opcode[] FourBitOpcodes = new[]
        {
            Opcode.Dec, Opcode.Gsr, Opcode.Inc, Opcode.Ssr
        };

        readonly Opcode[] SixBitOpcodes = new[]
        {
            Opcode.Beq, Opcode.Bge, Opcode.Bgeu,
            Opcode.Bgt, Opcode.Bgtu, Opcode.Ble, Opcode.Bleu,
            Opcode.Blt, Opcode.Bltu, Opcode.Bne
        };

        readonly Opcode[] EightBitOpcodes = new[]
        {
            Opcode.LdiL, Opcode.Mov, Opcode.Jsra, Opcode.Ret,
            Opcode.Add, Opcode.Push, Opcode.Pop, Opcode.LdaL,
            Opcode.StaL, Opcode.LdL, Opcode.StL, Opcode.LdoL,
            Opcode.StoL, Opcode.Cmp, Opcode.Nop, Opcode.SexB,
            Opcode.SexS, Opcode.ZexB, Opcode.ZexS, Opcode.UmulX,
            Opcode.MulX, Opcode.Jsr, Opcode.Jmpa, Opcode.LdiB,
            Opcode.LdB, Opcode.LdaB, Opcode.StB, Opcode.StaB,
            Opcode.LdiS, Opcode.LdS, Opcode.LdaS, Opcode.StS,
            Opcode.StaS, Opcode.Jmp, Opcode.And, Opcode.Lshr,
            Opcode.Ashl, Opcode.Sub, Opcode.Neg, Opcode.Or,
            Opcode.Not, Opcode.Ashr, Opcode.Xor, Opcode.Mul,
            Opcode.Swi, Opcode.Div, Opcode.Udiv, Opcode.Mod,
            Opcode.Umod, Opcode.Brk, Opcode.LdoB, Opcode.StoB,
            Opcode.LdoS, Opcode.StoS
        };

        [ReadableFact("All instructions are tested")]
        public void AllInstructionsAreTested()
        {
            var testedOpcodesCount = FourBitOpcodes.Length + SixBitOpcodes.Length + EightBitOpcodes.Length;
            var allOpcodesCount = Enum.GetValues(typeof(Opcode)).Length;

            testedOpcodesCount.Should().Be(allOpcodesCount);
        }

        void ForEachOpcode(IEnumerable<Opcode> opcodes, Action<ushort> action)
        {
            foreach (var opcode in opcodes)
            {
                action((ushort)(((ushort)opcode) << 8));
            }
        }

        void TestOpcodeParameter(IEnumerable<Opcode> opcodes, ushort rawValue, int expectedDecodedValue)
        {
            var decoder = new InstructionDecoder();

            ForEachOpcode(opcodes, opcode =>
            {
                var encodedInstruction = (ushort)(opcode | rawValue);
                var decodedInstruction = decoder.Decode(encodedInstruction);
                decodedInstruction.Value.Should().Be(expectedDecodedValue);
            });
        }

        [ReadableTheory("6-bit opcode: 10-bit parameter should be treated as signed")]
        [InlineData(0, 0)]
        [InlineData(511, 511)]
        [InlineData(512, -512)]
        [InlineData(513, -511)]
        [InlineData(1023, -1)]
        public void SixBitInstructionParametersAreSigned(ushort rawValue, int expectedDecodedValue)
        {
            TestOpcodeParameter(SixBitOpcodes, rawValue, expectedDecodedValue);
        }

        [ReadableTheory("4-bit opcode: 8-bit parameter should be treated as unsigned")]
        [InlineData(0, 0)]
        [InlineData(127, 127)]
        [InlineData(255, 255)]
        public void FourBitOpcodeParametersAreUnsigned(ushort rawValue, int expectedDecodedValue)
        {
            TestOpcodeParameter(FourBitOpcodes, rawValue, expectedDecodedValue);
        }

        [ReadableTheory("4-bit opcode: register A encoded in bits 8-11")]
        [InlineData(0)]
        [InlineData(15)]
        public void FourBitOpcodeRegisterAInBits7To11(int register)
        {
            var decoder = new InstructionDecoder();

            ForEachOpcode(FourBitOpcodes, opcode =>
            {
                var encodedInstruction = (ushort)(opcode | (register << 8));
                var decodedInstruction = decoder.Decode(encodedInstruction);
                decodedInstruction.RegisterA.Should().Be(register);
            });
        }

        [ReadableTheory("8-bit opcode: register A encoded in bits 4-7")]
        [InlineData(0)]
        [InlineData(15)]
        public void EightBitOpcodeRegisterAInBits4To7(int register)
        {
            var decoder = new InstructionDecoder();

            ForEachOpcode(EightBitOpcodes, opcode =>
            {
                var encodedInstruction = (ushort)(opcode | (register << 4));
                var decodedInstruction = decoder.Decode(encodedInstruction);
                decodedInstruction.RegisterA.Should().Be(register);
            });
        }

        [ReadableTheory("8-bit opcode: register B encoded in bits 0-3")]
        [InlineData(0)]
        [InlineData(15)]
        public void EightBitOpcodeRegisterBInBits0To3(int register)
        {
            var decoder = new InstructionDecoder();

            ForEachOpcode(EightBitOpcodes, opcode =>
            {
                var encodedInstruction = (ushort)(opcode | (register << 0));
                var decodedInstruction = decoder.Decode(encodedInstruction);
                decodedInstruction.RegisterB.Should().Be(register);
            });
        }
    }
}
