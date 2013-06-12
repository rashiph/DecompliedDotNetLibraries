namespace System.Diagnostics.Contracts
{
    using System;
    using System.Diagnostics;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple=false, Inherited=true), Conditional("CONTRACTS_FULL")]
    public sealed class ContractRuntimeIgnoredAttribute : Attribute
    {
    }
}

