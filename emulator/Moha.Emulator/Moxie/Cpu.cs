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
            uint address;
            uint valueA = Registers[instruction.RegisterA];
            uint valueB = Registers[instruction.RegisterB];
            ulong ulongResult;
            switch (instruction.Opcode)
            {
                case Opcode.And:
                    valueA = valueA & valueB;
                    break;

                case Opcode.Add:
                    valueA = valueA + valueB;
                    break;

                case Opcode.Ashl:
                    valueA = (uint)((int)valueA << (int)valueB);
                    break;

                case Opcode.Ashr:
                    valueA = (uint)((int)valueA >> (int)valueB);
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
                    signedA = (int)valueA;
                    signedB = (int)valueB;
                    CompareStatusUnsigned = valueA.CompareTo(valueB);
                    CompareStatusSigned = signedA.CompareTo(signedB);
                    break;

                case Opcode.Dec:
                    valueA = (uint)(valueA - instruction.Value);
                    break;

                case Opcode.Div:
                    signedA = (int)valueA;
                    signedB = (int)valueB;
                    if (signedB == 0)
                    {
                        ExecutionException.Throw(ExecutionError.DivisionByZero);
                    }
                    else if (signedA == int.MinValue && signedB == -1)
                    {
                        unchecked
                        {
                            valueA = (uint)int.MinValue;
                        }
                    }
                    else
                    {
                        valueA = (uint)(signedA / signedB);
                    }
                    break;

                case Opcode.Gsr:
                    valueA = SpecialRegisters[instruction.Value];
                    break;

                case Opcode.Inc:
                    valueA = (uint)(valueA + instruction.Value);
                    break;

                case Opcode.Jmp:
                    Jump(valueA);
                    break;

                case Opcode.Jmpa:
                    Jump(GetLongImmediate());
                    break;

                case Opcode.Jsr:
                    Push(SP_REGISTER_INDEX, (uint)Ip);
                    Jump(valueA);
                    break;

                case Opcode.Jsra:
                    Push(SP_REGISTER_INDEX, (uint)Ip + 2);
                    Jump(GetLongImmediate());
                    break;

                case Opcode.LdB:
                    valueA = _memory.GetByte(valueB);
                    break;

                case Opcode.LdL:
                    valueA = _memory.GetLong(valueB);
                    break;

                case Opcode.LdS:
                    valueA = _memory.GetShort(valueB);
                    break;

                case Opcode.LdaB:
                    valueA = _memory.GetByte(GetLongImmediate());
                    Ip += 2;
                    break;

                case Opcode.LdaL:
                    valueA = _memory.GetLong(GetLongImmediate());
                    Ip += 2;
                    break;

                case Opcode.LdaS:
                    valueA = _memory.GetShort(GetLongImmediate());
                    Ip += 2;
                    break;

                case Opcode.LdiL:
                    valueA = GetLongImmediate();
                    break;

                case Opcode.LdiB:
                    valueA = GetLongImmediate() & 0xFF;
                    break;

                case Opcode.LdiS:
                    valueA = GetLongImmediate() & 0xFFFF;
                    break;

                case Opcode.LdoB:
                    address = (uint)(valueB + GetShortSignedImmediate());
                    valueA = _memory.GetByte(address);
                    break;

                case Opcode.LdoL:
                    address = (uint)(valueB + GetShortSignedImmediate());
                    valueA = _memory.GetLong(address);
                    break;

                case Opcode.LdoS:
                    address = (uint)(valueB + GetShortSignedImmediate());
                    valueA = _memory.GetShort(address);
                    break;

                case Opcode.Lshr:
                    valueA = valueA >> (int)valueB;
                    break;

                case Opcode.Mod:
                    signedA = (int)valueA;
                    signedB = (int)valueB;
                    valueA = (uint)(signedA % signedB);
                    break;

                case Opcode.Mul:
                    signedA = (int)valueA;
                    signedB = (int)valueB;
                    ulongResult = (ulong)(signedA * signedB);
                    valueA = (uint)ulongResult;
                    break;

                case Opcode.MulX:
                    signedA = (int)valueA;
                    signedB = (int)valueB;
                    ulongResult = (ulong)(signedA * signedB);
                    ulongResult >>= 32;
                    valueA = (uint)ulongResult;
                    break;

                case Opcode.Neg:
                    signedB = (int)valueB;
                    signedB = -signedB;
                    valueA = (uint)signedB;
                    break;

                case Opcode.Nop:
                    break;

                case Opcode.Not:
                    valueA = ~valueB;
                    break;

                case Opcode.Or:
                    valueA |= valueB;
                    break;

                case Opcode.Pop:
                    valueB = Pop(instruction.RegisterA);
                    break;

                case Opcode.Push:
                    Push(instruction.RegisterA, valueB);
                    break;

                case Opcode.Ret:
                    address = Pop(SP_REGISTER_INDEX) / 2;
                    Ip = (int)address;
                    break;

                case Opcode.SexB:
                    valueA = (uint)(sbyte)valueB;
                    break;

                case Opcode.SexS:
                    valueA = (uint)(short)valueB;
                    break;

                case Opcode.Ssr:
                    SpecialRegisters[instruction.Value] = valueA;
                    break;

                case Opcode.StB:
                    _memory.StoreByte(valueA, (byte)valueB);
                    break;

                case Opcode.StL:
                    _memory.StoreLong(valueA, valueB);
                    break;

                case Opcode.StS:
                    _memory.StoreShort(valueA, (ushort)valueB);
                    break;

                case Opcode.StaB:
                    _memory.StoreByte(GetLongImmediate(), (byte)valueA);
                    break;

                case Opcode.StaL:
                    _memory.StoreLong(GetLongImmediate(), valueA);
                    break;

                case Opcode.StaS:
                    _memory.StoreShort(GetLongImmediate(), (ushort)valueA);
                    break;

                case Opcode.StoB:
                    address = (uint)(valueA + GetShortSignedImmediate());
                    _memory.StoreByte(address, (byte)valueB);
                    break;

                case Opcode.StoL:
                    address = (uint)(valueA + GetShortSignedImmediate());
                    _memory.StoreLong(address, valueB);
                    break;

                case Opcode.StoS:
                    address = (uint)(valueA + GetShortSignedImmediate());
                    _memory.StoreShort(address, (ushort)valueB);
                    break;

                case Opcode.Sub:
                    valueA = valueA - valueB;
                    break;

                case Opcode.Swi:
                    Exception(GetLongImmediate());
                    break;

                case Opcode.Udiv:
                    valueA /= valueB;
                    break;

                case Opcode.Umod:
                    valueA %= valueB;
                    break;

                case Opcode.UmulX:
                    ulongResult = valueA * valueB;
                    ulongResult >>= 32;
                    valueA = (uint)ulongResult;
                    break;

                case Opcode.Xor:
                    valueA ^= valueB;
                    break;

                case Opcode.ZexB:
                    valueA = valueB & 0xFF;
                    break;

                case Opcode.ZexS:
                    valueA = valueB & 0xFFFF;
                    break;

                default:
                    ExecutionException.Throw(ExecutionError.IllegalOpcode);
                    break;
            }

            Registers[instruction.RegisterA] = valueA;
            Registers[instruction.RegisterB] = valueB;
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
