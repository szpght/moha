using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Moha.Emulator.Moxie
{
    class Mmu
    {
        public Mmu(int size)
        {
            CheckAlignment(size, nameof(size));
            _memory = new ushort[size / 2];
            Size = size;
        }

        public long Size { get; }
        readonly ushort[] _memory;

        public ushort this[int index] => _memory[index];

        public void CopyToPhysical(int destinationOffset, Span<byte> source)
        {
            CheckAlignment(destinationOffset, nameof(destinationOffset));
            var casted = MemoryMarshal.Cast<byte, ushort>(source);
            var destination = _memory.AsSpan(destinationOffset);
            casted.CopyTo(destination);
        }

        public uint GetLong(int index)
        {
            var memoryAreaWithTheLong = _memory.AsSpan(index, 4);
            var castedMemory = MemoryMarshal.Cast<ushort, uint>(memoryAreaWithTheLong);
            return castedMemory[0];
        }

        public void CheckAlignment(long offset, string name)
        {
            if (offset % 2 > 0)
            {
                throw new Exception($"{name} must be even");
            }
        }
    }
}
