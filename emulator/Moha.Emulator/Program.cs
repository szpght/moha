using ELFSharp.ELF;
using ELFSharp.ELF.Segments;
using Moha.Emulator.Moxie;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;

[assembly: InternalsVisibleTo("Moha.Emulator.Tests")]

namespace Moha.Emulator
{
    class Program
    {
        static void Main(string[] args)
        {
            const int memorySize = 16 * 1024 * 1024;
            var mmu = new Mmu(memorySize);
            var decoder = new InstructionDecoder();
            var cpu = new Cpu(mmu, decoder);

            var elf = ELFReader.Load<uint>("D:\\fibo");
            var startAddress = elf.EntryPoint;
            var loadableSegments = elf.Segments.Where(s => s.Type == SegmentType.Load);
            foreach (var segment in loadableSegments)
            {
                var content = segment.GetMemoryContents();
                var address = segment.PhysicalAddress;
                mmu.CopyToPhysical(address, content);
            }

            cpu.Execute((int)startAddress);

            var stoper = Stopwatch.StartNew();
            for (var i = 0; i < 100; ++i)
            {
                cpu.Execute((int)startAddress);
            }
            stoper.Stop();
            Console.WriteLine(stoper.ElapsedMilliseconds);
            Console.WriteLine(cpu.InstructionsExecuted);
            Console.WriteLine($"{(double)cpu.InstructionsExecuted / stoper.ElapsedMilliseconds / 1000} MIPS");
            cpu.Reset();


            stoper = Stopwatch.StartNew();
            for (var i = 0; i < 100; ++i)
            {
                cpu.Execute((int)startAddress);
            }
            stoper.Stop();
            Console.WriteLine(stoper.ElapsedMilliseconds);
            Console.WriteLine(cpu.InstructionsExecuted);
            Console.WriteLine($"{(double)cpu.InstructionsExecuted / stoper.ElapsedMilliseconds / 1000} MIPS");
            cpu.Reset();


            stoper = Stopwatch.StartNew();
            for (var i = 0; i < 100; ++i)
            {
                cpu.Execute((int)startAddress);
            }
            stoper.Stop();
            Console.WriteLine(stoper.ElapsedMilliseconds);
            Console.WriteLine(cpu.InstructionsExecuted);
            Console.WriteLine($"{(double)cpu.InstructionsExecuted / stoper.ElapsedMilliseconds / 1000} MIPS");
        }
    }
}
