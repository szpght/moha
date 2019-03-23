using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Moha.Emulator.Moxie
{
    class Cpu
    {
        readonly Mmu _memory;
        readonly InstructionDecoder _decoder;

        public Cpu(Mmu mmu, InstructionDecoder decoder)
        {
            _memory = mmu;
            _decoder = decoder;
        }

        int Ip;
        int CompareStatusSigned;
        int CompareStatusUnsigned;
        uint Fp
        {
            get => Registers[0];
            set => Registers[0] = value;
        }
        uint Sp
        {
            get => Registers[1];
            set => Registers[1] = value;
        }

        readonly uint[] Registers = new uint[16];
        readonly uint[] SpecialRegisters = new uint[10];

        public void Execute(int startAddress)
        {
            Ip = startAddress;

            while (true)
            {
                ExecuteNextInstruction();
            }
        }

        private void ExecuteNextInstruction()
        {
            var instruction = _decoder.Decode(_memory[Ip]);
            Ip++;

            switch (instruction.Opcode)
            {
                case Opcode.And:
                    Registers[instruction.RegisterA] = Registers[instruction.RegisterA] & Registers[instruction.RegisterB];
                    break;

                case Opcode.Add:
                    Registers[instruction.RegisterA] = Registers[instruction.RegisterA] + Registers[instruction.RegisterB];
                    break;

                case Opcode.Ashl:
                    Registers[instruction.RegisterA] = Registers[instruction.RegisterA] << (int)Registers[instruction.RegisterB];
                    break;

                case Opcode.Ashr:
                    Registers[instruction.RegisterA] = Registers[instruction.RegisterA] >> (int)Registers[instruction.RegisterB];
                    break;

                case Opcode.Beq:
                    if (CompareStatusUnsigned == 0)
                    {
                        Ip += instruction.Value;
                    }
                    break;

                case Opcode.Cmp:
                    var a = Registers[instruction.RegisterA];
                    var b = Registers[instruction.RegisterB];
                    var signedA = (int)a;
                    CompareStatusUnsigned = a.CompareTo(b);
                    CompareStatusSigned = signedA.CompareTo((int)b);
                    break;

                case Opcode.Nop:
                    break;

                case Opcode.Brk:
                    Console.WriteLine("Breakpoint hit. Press enter key to continue");
                    Console.ReadLine();
                    break;

                case Opcode.Inc:
                    Registers[instruction.RegisterA] = (uint)(Registers[instruction.RegisterA] + instruction.Value);
                    break;

                case Opcode.Dec:
                    Registers[instruction.RegisterA] = (uint)(Registers[instruction.RegisterA] - instruction.Value);
                    break;

                case Opcode.Gsr:
                    Registers[instruction.RegisterA] = SpecialRegisters[instruction.Value];
                    break;

                case Opcode.Ssr:
                    SpecialRegisters[instruction.Value] = Registers[instruction.RegisterA];
                    break;

                default:
                    ExecutionException.Throw(ExecutionError.IllegalOpcode);
                    break;
            }
        }
    }
}
