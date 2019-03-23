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

            int signedA, signedB;
            uint unsignedA, unsignedB;
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
                        Ip = BranchNewIp(instruction.Value);
                    }
                    break;

                case Opcode.Bge:
                    if (CompareStatusSigned >= 0)
                    {
                        Ip = BranchNewIp(instruction.Value);
                    }
                    break;

                case Opcode.Bgeu:
                    if (CompareStatusUnsigned >= 0)
                    {
                        Ip = BranchNewIp(instruction.Value);
                    }
                    break;

                case Opcode.Bgt:
                    if (CompareStatusSigned > 0)
                    {
                        Ip = BranchNewIp(instruction.Value);
                    }
                    break;

                case Opcode.Bgtu:
                    if (CompareStatusUnsigned > 0)
                    {
                        Ip = BranchNewIp(instruction.Value);
                    }
                    break;

                case Opcode.Ble:
                    if (CompareStatusSigned <= 0)
                    {
                        Ip = BranchNewIp(instruction.Value);
                    }
                    break;

                case Opcode.Bleu:
                    if (CompareStatusUnsigned <= 0)
                    {
                        Ip = BranchNewIp(instruction.Value);
                    }
                    break;

                case Opcode.Blt:
                    if (CompareStatusSigned < 0)
                    {
                        Ip = BranchNewIp(instruction.Value);
                    }
                    break;

                case Opcode.Bltu:
                    if (CompareStatusUnsigned < 0)
                    {
                        Ip = BranchNewIp(instruction.Value);
                    }
                    break;

                case Opcode.Bne:
                    if (CompareStatusUnsigned != 0)
                    {
                        Ip = BranchNewIp(instruction.Value);
                    }
                    break;

                case Opcode.Brk:
                    Console.WriteLine("Breakpoint hit. Press enter key to continue");
                    Console.ReadLine();
                    break;

                case Opcode.Cmp:
                    unsignedA = Registers[instruction.RegisterA];
                    unsignedB = Registers[instruction.RegisterB];
                    signedA = (int)unsignedA;
                    signedB = (int)unsignedB;
                    CompareStatusUnsigned = unsignedA.CompareTo(unsignedB);
                    CompareStatusSigned = signedA.CompareTo(signedB);
                    break;

                case Opcode.Dec:
                    Registers[instruction.RegisterA] = (uint)(Registers[instruction.RegisterA] - instruction.Value);
                    break;

                case Opcode.Div:
                    signedA = (int)Registers[instruction.RegisterA];
                    signedB = (int)Registers[instruction.RegisterB];
                    if (signedB == 0)
                    {
                        ExecutionException.Throw(ExecutionError.DivisionByZero);
                    }
                    else if (signedA == int.MinValue && signedB == -1)
                    {
                        unchecked
                        {
                            Registers[instruction.RegisterA] = (uint)int.MinValue;
                        }
                    }
                    else
                    {
                        Registers[instruction.RegisterA] = (uint)(signedA / signedB);
                    }
                    break;

                case Opcode.Gsr:
                    Registers[instruction.RegisterA] = SpecialRegisters[instruction.Value];
                    break;

                case Opcode.Inc:
                    Registers[instruction.RegisterA] = (uint)(Registers[instruction.RegisterA] + instruction.Value);
                    break;

                case Opcode.Jmp:
                    Jump(Registers[instruction.RegisterA]);
                    break;

                case Opcode.Jmpa:
                    Jump(_memory.GetLong(Ip));
                    break;

                case Opcode.Nop:
                    break;

                case Opcode.Ssr:
                    SpecialRegisters[instruction.Value] = Registers[instruction.RegisterA];
                    break;

                default:
                    ExecutionException.Throw(ExecutionError.IllegalOpcode);
                    break;
            }
        }

        private void Jump(uint address)
        {
            if (address % 2 != 0)
            {
                ExecutionException.Throw(ExecutionError.JumpToUnalignedAddress);
            }

            Ip = (int)(address / 2);
        }

        private int BranchNewIp(int offset)
        {
            return Ip + offset;
        }
    }
}
