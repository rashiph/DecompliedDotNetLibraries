namespace System.Diagnostics
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited=false)]
    public sealed class DebuggerStepperBoundaryAttribute : Attribute
    {
    }
}

