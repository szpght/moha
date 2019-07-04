namespace Moha.Emulator.Moxie
{
    readonly struct NoTracing : IExecutionTracing
    {
        public void Initialize() { }

        public void TraceCall(in Instruction instruction, int ip) { }

        public void TraceImmediate(uint value) { }
    }
}
