namespace Moha.Emulator.Moxie
{
    partial class Cpu<TMemoryAccessor, TExecutionTracing>
        where TMemoryAccessor : struct, IMemoryAccessor
        where TExecutionTracing : struct, IExecutionTracing
    {
        readonly uint[] Registers = new uint[16];
        readonly uint[] SpecialRegisters = new uint[10];

        const int FP_REGISTER_INDEX = 0;
        const int SP_REGISTER_INDEX = 1;

        const int SR_STATUS_INDEX = 0;
        const int SR_EXCEPTION_HANDLER_INDEX = 1;
        const int SR_EXCEPTION_TRIGGER_INDEX = 2;
        const int SR_SWI_REQUEST_NUMBER_INDEX = 3;
        const int SR_SUPERVISOR_MODE_STACK_INDEX = 4;
        const int SR_EXCEPTION_RETURN_ADDRESS_INDEX = 5;
        const int SR_DEVICE_TREE_POINTER_INDEX = 9;

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

        uint StatusRegister
        {
            get => SpecialRegisters[SR_STATUS_INDEX];
            set => SpecialRegisters[SR_STATUS_INDEX] = value;
        }

        uint ExceptionHandler
        {
            get => SpecialRegisters[SR_EXCEPTION_HANDLER_INDEX];
            set => SpecialRegisters[SR_EXCEPTION_HANDLER_INDEX] = value;
        }

        ExceptionTrigger ExceptionTrigger
        {
            get => (ExceptionTrigger)SpecialRegisters[SR_EXCEPTION_TRIGGER_INDEX];
            set => SpecialRegisters[SR_EXCEPTION_TRIGGER_INDEX] = (uint)value;
        }

        uint SwiRequestIndex
        {
            get => SpecialRegisters[SR_SWI_REQUEST_NUMBER_INDEX];
            set => SpecialRegisters[SR_SWI_REQUEST_NUMBER_INDEX] = value;
        }

        uint SupervisorModeStack
        {
            get => SpecialRegisters[SR_SUPERVISOR_MODE_STACK_INDEX];
            set => SpecialRegisters[SR_SUPERVISOR_MODE_STACK_INDEX] = value;
        }

        uint ExceptionReturnAddress
        {
            get => SpecialRegisters[SR_EXCEPTION_RETURN_ADDRESS_INDEX];
            set => SpecialRegisters[SR_EXCEPTION_RETURN_ADDRESS_INDEX] = value;
        }

        uint DeviceTreePointer
        {
            get => SpecialRegisters[SR_DEVICE_TREE_POINTER_INDEX];
            set => SpecialRegisters[SR_DEVICE_TREE_POINTER_INDEX] = value;
        }
    }
}