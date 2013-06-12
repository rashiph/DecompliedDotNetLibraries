namespace System.Diagnostics.Contracts
{
    using System;
    using System.Diagnostics;

    [AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Parameter | AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, AllowMultiple=false, Inherited=true), Conditional("CONTRACTS_FULL")]
    public sealed class PureAttribute : Attribute
    {
    }
}

