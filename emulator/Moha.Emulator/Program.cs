using ELFSharp.ELF;
using ELFSharp.ELF.Segments;
using Moha.Emulator.Moxie;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Moha.Emulator.Tests")]

namespace Moha.Emulator
{
    class Program
    {
        static void Main(string[] args)
        {
            uint a = 200;
            uint b;
            unchecked
            {
                b = (uint)(sbyte)a;
            }

            const int memorySize = 16 * 1024 * 1024;
            var mmu = new Mmu(memorySize);
            var decoder = new InstructionDecoder();
            var cpu = new Cpu(mmu, decoder);

            var elf = ELFReader.Load<uint>("D:\\memory");
            var startAddress = elf.EntryPoint;
            var loadableSegments = elf.Segments.Where(s => s.Type == SegmentType.Load);
            foreach (var segment in loadableSegments)
            {
                var content = segment.GetMemoryContents();
                var address = segment.PhysicalAddress;
                mmu.CopyToPhysical((int)address, content);
            }

            cpu.Execute((int)startAddress);
        }
    }
}
