using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Moha.Emulator.Moxie
{
    class Cpu
    {
        const int FP_REGISTER_INDEX = 0;
        const int SP_REGISTER_INDEX = 1;
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
            get => Registers[FP_REGISTER_INDEX];
            set => Registers[FP_REGISTER_INDEX] = value;
        }
        uint Sp
        {
            get => Registers[SP_REGISTER_INDEX];
            set => Registers[SP_REGISTER_INDEX] = value;
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
            uint unsignedA, unsignedB, address;
            ulong ulongResult;
            switch (instruction.Opcode)
            {
                case Opcode.And:
                    Registers[instruction.RegisterA] = Registers[instruction.RegisterA] & Registers[instruction.RegisterB];
                    break;

                case Opcode.Add:
                    Registers[instruction.RegisterA] = Registers[instruction.RegisterA] + Registers[instruction.RegisterB];
                    break;

                case Opcode.Ashl:
                    Registers[instruction.RegisterA] = (uint)((int)Registers[instruction.RegisterA] << (int)Registers[instruction.RegisterB]);
                    // TODO test
                    break;

                case Opcode.Ashr:
                    Registers[instruction.RegisterA] = (uint)((int)Registers[instruction.RegisterA] >> (int)Registers[instruction.RegisterB]);
                    // TODO test
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
                    Break();
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
                    Jump(GetLongImmediate());
                    break;

                case Opcode.Jsr:
                    Push(SP_REGISTER_INDEX, (uint)Ip);
                    Jump(Registers[instruction.RegisterA]);
                    break;

                case Opcode.Jsra:
                    Push(SP_REGISTER_INDEX, (uint)Ip + 2);
                    Jump(GetLongImmediate());
                    break;

                case Opcode.LdB:
                    Registers[instruction.RegisterA] = _memory.GetByte(Registers[instruction.RegisterB]);
                    break;

                case Opcode.LdL:
                    Registers[instruction.RegisterA] = _memory.GetLong(Registers[instruction.RegisterB]);
                    break;

                case Opcode.LdS:
                    Registers[instruction.RegisterA] = _memory.GetShort(Registers[instruction.RegisterB]);
                    break;

                case Opcode.LdaB:
                    Registers[instruction.RegisterA] = _memory.GetByte(GetLongImmediate());
                    Ip += 2;
                    break;

                case Opcode.LdaL:
                    Registers[instruction.RegisterA] = _memory.GetLong(GetLongImmediate());
                    Ip += 2;
                    break;

                case Opcode.LdaS:
                    Registers[instruction.RegisterA] = _memory.GetShort(GetLongImmediate());
                    Ip += 2;
                    break;

                case Opcode.LdiL:
                    Registers[instruction.RegisterA] = GetLongImmediate();
                    break;

                case Opcode.LdiB:
                    Registers[instruction.RegisterA] = GetLongImmediate() & 0xFF;
                    Break("ldi.b executed, check if result correct"); // TODO test is this and needed and correct
                    break;

                case Opcode.LdiS:
                    Registers[instruction.RegisterA] = GetLongImmediate() & 0xFFFF;
                    Break("ldi.s executed, check if result correct"); // TODO test is this and needed and correct
                    break;

                case Opcode.LdoB:
                    address = (uint)(Registers[instruction.RegisterB] + GetShortSignedImmediate());
                    Registers[instruction.RegisterA] = _memory.GetByte(address);
                    break;

                case Opcode.LdoL:
                    address = (uint)(Registers[instruction.RegisterB] + GetShortSignedImmediate());
                    Registers[instruction.RegisterA] = _memory.GetLong(address);
                    break;

                case Opcode.LdoS:
                    address = (uint)(Registers[instruction.RegisterB] + GetShortSignedImmediate());
                    Registers[instruction.RegisterA] = _memory.GetShort(address);
                    break;

                case Opcode.Lshr:
                    Registers[instruction.RegisterA] = Registers[instruction.RegisterA] >> (int)Registers[instruction.RegisterB];
                    // TODO test
                    break;

                case Opcode.Mod:
                    signedA = (int)Registers[instruction.RegisterA];
                    signedB = (int)Registers[instruction.RegisterB];
                    Registers[instruction.RegisterA] = (uint)(signedA % signedB);
                    break;

                case Opcode.Mul:
                    signedA = (int)Registers[instruction.RegisterA];
                    signedB = (int)Registers[instruction.RegisterB];
                    ulongResult = (ulong)(signedA * signedB);
                    Registers[instruction.RegisterA] = (uint)ulongResult;
                    break;

                case Opcode.MulX:
                    signedA = (int)Registers[instruction.RegisterA];
                    signedB = (int)Registers[instruction.RegisterB];
                    ulongResult = (ulong)(signedA * signedB);
                    ulongResult >>= 32;
                    Registers[instruction.RegisterA] = (uint)ulongResult;
                    break;

                case Opcode.Neg:
                    signedB = (int)Registers[instruction.RegisterB];
                    signedB = -signedB;
                    Registers[instruction.RegisterA] = (uint)signedB;
                    break;

                case Opcode.Nop:
                    break;

                case Opcode.Not:
                    Registers[instruction.RegisterA] = ~Registers[instruction.RegisterB];
                    break;

                case Opcode.Or:
                    Registers[instruction.RegisterA] |= Registers[instruction.RegisterB];
                    break;

                case Opcode.Pop:
                    Registers[instruction.RegisterB] = Pop(instruction.RegisterA);
                    break;

                case Opcode.Push:
                    Push(instruction.RegisterA, Registers[instruction.RegisterB]);
                    break;

                case Opcode.Ret:
                    address = Pop(SP_REGISTER_INDEX) / 2;
                    Ip = (int)address;
                    break;

                case Opcode.SexB:
                    Registers[instruction.RegisterA] = (uint)(sbyte)Registers[instruction.RegisterB];
                    break;

                case Opcode.SexS:
                    Registers[instruction.RegisterA] = (uint)(short)Registers[instruction.RegisterB];
                    break;

                case Opcode.Ssr:
                    SpecialRegisters[instruction.Value] = Registers[instruction.RegisterA];
                    break;

                case Opcode.StB:
                case Opcode.StL:
                case Opcode.StS:
                case Opcode.StaB:
                case Opcode.StaL:
                case Opcode.StaS:
                case Opcode.StoB:
                case Opcode.StoL:
                case Opcode.StoS:
                    throw new NotImplementedException();

                case Opcode.Sub:
                    Registers[instruction.RegisterA] = Registers[instruction.RegisterA] - Registers[instruction.RegisterB];
                    break;

                case Opcode.Swi:
                    Exception(GetLongImmediate());
                    break;

                case Opcode.Udiv:
                    Registers[instruction.RegisterA] /= Registers[instruction.RegisterB];
                    break;

                case Opcode.Umod:
                    Registers[instruction.RegisterA] %= Registers[instruction.RegisterB];
                    break;

                case Opcode.UmulX:
                    ulongResult = Registers[instruction.RegisterA] * Registers[instruction.RegisterB];
                    ulongResult >>= 32;
                    Registers[instruction.RegisterA] = (uint)ulongResult;
                    break;

                case Opcode.Xor:
                    Registers[instruction.RegisterA] ^= Registers[instruction.RegisterB];
                    break;

                case Opcode.ZexB:
                    Registers[instruction.RegisterA] = Registers[instruction.RegisterB] & 0xFF;
                    break;

                case Opcode.ZexS:
                    Registers[instruction.RegisterA] = Registers[instruction.RegisterB] & 0xFFFF;
                    break;

                default:
                    ExecutionException.Throw(ExecutionError.IllegalOpcode);
                    break;
            }
        }

        private void Exception(uint type)
        {
            throw new NotImplementedException();
        }

        private uint GetLongImmediate()
        {
            var value = _memory.GetLongAtIndex(Ip);
            Ip += 2;
            return value;
        }

        private short GetShortSignedImmediate()
        {
            return (short)_memory[Ip++];
        }

        private void Jump(uint address)
        {
            if (address % 2 != 0)
            {
                ExecutionException.Throw(ExecutionError.JumpToUnalignedAddress);
            }

            Ip = (int)(address / 2);
        }

        private void Push(int register, uint value)
        {
            throw new NotImplementedException();
        }

        private uint Pop(int register)
        {
            throw new NotImplementedException();
        }

        private int BranchNewIp(int offset)
        {
            return Ip + offset;
        }

        private void Break(string message = null)
        {
            Console.WriteLine($"{message ?? "Execution in break mode"}\nPress enter to resume");
            Console.ReadLine();
        }
    }
}
