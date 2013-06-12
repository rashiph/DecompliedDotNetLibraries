namespace System.Diagnostics.Contracts
{
    using System;
    using System.Diagnostics;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=false), Conditional("CONTRACTS_FULL")]
    public sealed class ContractInvariantMethodAttribute : Attribute
    {
    }
}

