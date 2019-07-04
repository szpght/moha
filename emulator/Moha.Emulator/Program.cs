using ELFSharp.ELF;
using ELFSharp.ELF.Segments;
using Moha.Emulator.Moxie;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moha.Emulator.Helpers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;

[assembly: InternalsVisibleTo("Moha.Emulator.Xunit")]

namespace Moha.Emulator
{
    public class Benchmark
    {
        Cpu<PhysicalMemoryAccessor, NoTracing> cpu1;
        Cpu<PhysicalMemoryAccessor, NoTracing> cpu2;
        uint startAddress;

        public Benchmark()
        {
            const int memorySize = 32 * 1024 * 1024;
            var mmu = new Mmu(memorySize);
            var decoder = new InstructionDecoder();
            cpu1 = new Cpu<PhysicalMemoryAccessor, NoTracing>(mmu, decoder);
            cpu2 = new Cpu<PhysicalMemoryAccessor, NoTracing>(mmu, decoder);
            mmu._pageDirectory = memorySize - 4;

            var elf = ELFReader.Load<uint>("D:\\fibo");
            startAddress = elf.EntryPoint;
            var loadableSegments = elf.Segments.Where(s => s.Type == SegmentType.Load);
            foreach (var segment in loadableSegments)
            {
                var content = segment.GetMemoryContents();
                var address = segment.PhysicalAddress;
                mmu.CopyToPhysical(address, content);
            }


        }

        [Benchmark]
        public void Enabled()
        {
            cpu1.Execute((int)startAddress);
        }

                [Benchmark]
        public void Enabled2()
        {
            cpu1.Execute((int)startAddress);
        }

        [Benchmark]
        public void Disabled()
        {
            cpu2.Execute((int)startAddress);
        }
                [Benchmark]
        public void Disabled2()
        {
            cpu2.Execute((int)startAddress);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            const int memorySize = 32 * 1024 * 1024;
            var mmu = new Mmu(memorySize);
            var decoder = new InstructionDecoder();
            var cpu = new Cpu<PhysicalMemoryAccessor, Tracing>(mmu, decoder);

            if (false /*cpu is Cpu<VirtualMemoryAccessor>*/)
            {
                var (location, data) = TlbHelper.PrepareIdentityMap(memorySize);
                mmu.CopyToPhysical(location, data);
                mmu._pageDirectory = location;
            }
            else
            {
                mmu._pageDirectory = memorySize - 4;
            }
            
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
            for (var i = 0; i < 10; ++i)
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
