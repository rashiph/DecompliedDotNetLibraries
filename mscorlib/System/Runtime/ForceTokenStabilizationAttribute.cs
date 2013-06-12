namespace System.Runtime
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    internal sealed class ForceTokenStabilizationAttribute : Attribute
    {
    }
}

