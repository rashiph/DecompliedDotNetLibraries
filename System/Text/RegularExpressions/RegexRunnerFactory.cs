namespace System.Text.RegularExpressions
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class RegexRunnerFactory
    {
        protected RegexRunnerFactory()
        {
        }

        protected internal abstract RegexRunner CreateInstance();
    }
}

