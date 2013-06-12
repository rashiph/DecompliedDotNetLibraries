namespace System.Runtime.CompilerServices
{
    using System;

    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class)]
    internal sealed class SuppressMergeCheckAttribute : Attribute
    {
    }
}

