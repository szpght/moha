namespace Moha.Emulator.Moxie
{
    interface IExecutionTracing
    {
        void Initialize();
        void TraceCall(in Instruction instruction, int ip);
    }
}
