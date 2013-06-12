namespace System.Security.Policy
{
    using System;

    internal sealed class CodeGroupStackFrame
    {
        internal CodeGroup current;
        internal CodeGroupStackFrame parent;
        internal PolicyStatement policy;
    }
}

