using Xunit;

namespace Moha.Emulator.Xunit
{
    class ReadableFactAttribute : FactAttribute
    {
        public ReadableFactAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
