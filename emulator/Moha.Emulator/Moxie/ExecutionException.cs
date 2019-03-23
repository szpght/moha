using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Moha.Emulator.Moxie
{
    class ExecutionException : Exception
    {
        private ExecutionException(string message) : base(message)
        {
        }

        public static void Throw(ExecutionError error)
        {
            var description = error.GetType()
                .GetField(error.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .Cast<DescriptionAttribute>()
                .Single()
                .Description;
            throw new ExecutionException(description);
        }
    }
}
