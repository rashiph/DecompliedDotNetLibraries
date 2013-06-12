namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=false)]
    public sealed class AllowReversePInvokeCallsAttribute : Attribute
    {
    }
}

