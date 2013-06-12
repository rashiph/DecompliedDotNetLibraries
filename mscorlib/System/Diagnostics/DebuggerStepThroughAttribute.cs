namespace System.Diagnostics
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class, Inherited=false), ComVisible(true)]
    public sealed class DebuggerStepThroughAttribute : Attribute
    {
    }
}

