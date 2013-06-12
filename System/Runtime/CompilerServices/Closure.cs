namespace System.Runtime.CompilerServices
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    [EditorBrowsable(EditorBrowsableState.Never), DebuggerStepThrough]
    public sealed class Closure
    {
        public readonly object[] Constants;
        public readonly object[] Locals;

        public Closure(object[] constants, object[] locals)
        {
            this.Constants = constants;
            this.Locals = locals;
        }
    }
}

