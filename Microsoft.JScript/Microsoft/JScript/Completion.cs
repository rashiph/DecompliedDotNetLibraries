namespace Microsoft.JScript
{
    using System;

    internal sealed class Completion
    {
        internal int Continue = 0;
        internal int Exit = 0;
        internal bool Return = false;
        public object value = null;

        internal Completion()
        {
        }
    }
}

