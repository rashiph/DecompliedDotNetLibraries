namespace System.Runtime.ConstrainedExecution
{
    using System;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited=false)]
    public sealed class PrePrepareMethodAttribute : Attribute
    {
    }
}

