using Xunit;

namespace Moha.Emulator.Xunit
{
    public class ReadableTheoryAttribute : TheoryAttribute
    {
        public ReadableTheoryAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
