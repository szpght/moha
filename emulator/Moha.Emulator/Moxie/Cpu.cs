﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Moha.Emulator.Moxie
{
    partial class Cpu<TMemoryAccessor, TExecutionTracing>
        where TMemoryAccessor : struct, IMemoryAccessor
        where TExecutionTracing : struct, IExecutionTracing
    {
        readonly Mmu _memory;
        readonly InstructionDecoder _decoder;

        long _instructionsExecuted = 0;
        public long InstructionsExecuted => _instructionsExecuted;
        public void Reset() => _instructionsExecuted = 0;

        public Cpu(Mmu mmu, InstructionDecoder decoder)
        {
            _memory = mmu;
            _decoder = decoder;
            _tracing.Initialize();
        }

        int Ip;
        int CompareStatusSigned;
        int CompareStatusUnsigned;

        readonly TMemoryAccessor _memoryAccessor = default;
        readonly TExecutionTracing _tracing = default;

        public void Execute(int startAddress)
        {
            Sp = (uint)(_memory._pageDirectory - 4); // TODO handle this better
            Fp = Sp;
            Ip = startAddress / 2;

            bool run = true;
            while (run)
            {
                run = ExecuteNextInstruction();
            }
        }

        private bool ExecuteNextInstruction()
        {
            var instruction = _decoder.Decode(_memoryAccessor.GetShort(_memory, (uint)Ip * 2));
            _tracing.TraceCall(instruction, Ip);
            Ip++;

            int signedA, signedB;
            uint address;

            ulong ulongResult;
            switch (instruction.Opcode)
            {
                case Opcode.And:
                    Registers[instruction.RegisterA] &= Registers[instruction.RegisterB];
                    break;

                case Opcode.Add:
                    Registers[instruction.RegisterA] += Registers[instruction.RegisterB];
                    break;

                case Opcode.Ashl:
                    Registers[instruction.RegisterA] = (uint)((int)Registers[instruction.RegisterA] << (int)Registers[instruction.RegisterB]);
                    break;

                case Opcode.Ashr:
                    Registers[instruction.RegisterA] = (uint)((int)Registers[instruction.RegisterA] >> (int)Registers[instruction.RegisterB]);
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
                    return false;
                    Break();
                    break;

                case Opcode.Cmp:
                    signedA = (int)Registers[instruction.RegisterA];
                    signedB = (int)Registers[instruction.RegisterB];
                    CompareStatusUnsigned = Registers[instruction.RegisterA].CompareTo(Registers[instruction.RegisterB]);
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
                    JumpToSubroutine(Registers[instruction.RegisterA]);
                    break;

                case Opcode.Jsra:
                    address = GetLongImmediate();
                    JumpToSubroutine(address);
                    break;

                case Opcode.LdB:
                    Registers[instruction.RegisterA] = _memoryAccessor.GetByte(_memory, Registers[instruction.RegisterB]);
                    break;

                case Opcode.LdL:
                    Registers[instruction.RegisterA] = _memoryAccessor.GetLong(_memory, Registers[instruction.RegisterB]);
                    break;

                case Opcode.LdS:
                    Registers[instruction.RegisterA] = _memoryAccessor.GetShort(_memory, Registers[instruction.RegisterB]);
                    break;

                case Opcode.LdaB:
                    Registers[instruction.RegisterA] = _memoryAccessor.GetByte(_memory, GetLongImmediate());
                    Ip += 2;
                    break;

                case Opcode.LdaL:
                    Registers[instruction.RegisterA] = _memoryAccessor.GetLong(_memory, GetLongImmediate());
                    Ip += 2;
                    break;

                case Opcode.LdaS:
                    Registers[instruction.RegisterA] = _memoryAccessor.GetShort(_memory, GetLongImmediate());
                    Ip += 2;
                    break;

                case Opcode.LdiL:
                    Registers[instruction.RegisterA] = GetLongImmediate();
                    break;

                case Opcode.LdiB:
                    Registers[instruction.RegisterA] = GetLongImmediate() & 0xFF;
                    break;

                case Opcode.LdiS:
                    Registers[instruction.RegisterA] = GetLongImmediate() & 0xFFFF;
                    break;

                case Opcode.LdoB:
                    address = (uint)(Registers[instruction.RegisterB] + GetShortSignedImmediate());
                    Registers[instruction.RegisterA] = _memoryAccessor.GetByte(_memory, address);
                    break;

                case Opcode.LdoL:
                    address = (uint)(Registers[instruction.RegisterB] + GetShortSignedImmediate());
                    Registers[instruction.RegisterA] = _memoryAccessor.GetLong(_memory, address);
                    break;

                case Opcode.LdoS:
                    address = (uint)(Registers[instruction.RegisterB] + GetShortSignedImmediate());
                    Registers[instruction.RegisterA] = _memoryAccessor.GetShort(_memory, address);
                    break;

                case Opcode.Lshr:
                    Registers[instruction.RegisterA] = Registers[instruction.RegisterA] >> (int)Registers[instruction.RegisterB];
                    break;

                case Opcode.Mod:
                    signedA = (int)Registers[instruction.RegisterA];
                    signedB = (int)Registers[instruction.RegisterB];
                    Registers[instruction.RegisterA] = (uint)(signedA % signedB);
                    break;

                case Opcode.Mov:
                    Registers[instruction.RegisterA] = Registers[instruction.RegisterB];
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
                    ReturnFromSubroutine();
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
                    _memoryAccessor.StoreByte(_memory, Registers[instruction.RegisterA], (byte)Registers[instruction.RegisterB]);
                    break;

                case Opcode.StL:
                    _memoryAccessor.StoreLong(_memory, Registers[instruction.RegisterA], Registers[instruction.RegisterB]);
                    break;

                case Opcode.StS:
                    _memoryAccessor.StoreShort(_memory, Registers[instruction.RegisterA], (ushort)Registers[instruction.RegisterB]);
                    break;

                case Opcode.StaB:
                    _memoryAccessor.StoreByte(_memory, GetLongImmediate(), (byte)Registers[instruction.RegisterA]);
                    break;

                case Opcode.StaL:
                    _memoryAccessor.StoreLong(_memory, GetLongImmediate(), Registers[instruction.RegisterA]);
                    break;

                case Opcode.StaS:
                    _memoryAccessor.StoreShort(_memory, GetLongImmediate(), (ushort)Registers[instruction.RegisterA]);
                    break;

                case Opcode.StoB:
                    address = (uint)(Registers[instruction.RegisterA] + GetShortSignedImmediate());
                    _memoryAccessor.StoreByte(_memory, address, (byte)Registers[instruction.RegisterB]);
                    break;

                case Opcode.StoL:
                    address = (uint)(Registers[instruction.RegisterA] + GetShortSignedImmediate());
                    _memoryAccessor.StoreLong(_memory, address, Registers[instruction.RegisterB]);
                    break;

                case Opcode.StoS:
                    address = (uint)(Registers[instruction.RegisterA] + GetShortSignedImmediate());
                    _memoryAccessor.StoreShort(_memory, address, (ushort)Registers[instruction.RegisterB]);
                    break;

                case Opcode.Sub:
                    Registers[instruction.RegisterA] = Registers[instruction.RegisterA] - Registers[instruction.RegisterB];
                    break;

                case Opcode.Swi:
                    SwiRequestIndex = GetLongImmediate();
                    Exception(ExceptionReason.SoftwareInterrupt);
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

            _instructionsExecuted += 1;
            return true;
        }

        private void Exception(ExceptionReason reason)
        {
            ExceptionReason = reason;
            throw new NotImplementedException();
        }

        private uint GetLongImmediate()
        {
            uint value = _memoryAccessor.GetShort(_memory, (uint)Ip * 2);
            value |= ((uint)_memoryAccessor.GetShort(_memory, (uint)Ip * 2 + 2)) << 16;
            _tracing.TraceImmediate(value);
            Ip += 2;
            return value;
        }

        private short GetShortSignedImmediate()
        {
            ushort value = _memoryAccessor.GetShort(_memory, (uint)Ip * 2);
            Ip += 1;
            _tracing.TraceImmediate(value);
            return (short)value;
        }

        private void Jump(uint address)
        {
            if (address % 2 != 0)
            {
                ExecutionException.Throw(ExecutionError.JumpToUnalignedAddress);
            }

            Ip = (int)(address / 2);
        }

        private void JumpToSubroutine(uint address)
        {
            Push(SP_REGISTER_INDEX, 0); // empty value for static chain whatever it is
            Push(SP_REGISTER_INDEX, (uint)Ip);
            Push(SP_REGISTER_INDEX, Fp);
            Fp = Sp;
            Jump(address);
        }

        private void ReturnFromSubroutine()
        {
            Sp = Fp;
            Fp = Pop(SP_REGISTER_INDEX);
            Ip = (int)Pop(SP_REGISTER_INDEX);
            Pop(SP_REGISTER_INDEX);
        }

        private void Push(int register, uint value)
        {
            Registers[register] -= 4;
            _memoryAccessor.StoreLong(_memory, Registers[register], value);
        }

        private uint Pop(int register)
        {
            var value = _memoryAccessor.GetLong(_memory, Registers[register]);
            Registers[register] += 4;
            return value;
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
