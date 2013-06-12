namespace System.Security
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
    public sealed class SuppressUnmanagedCodeSecurityAttribute : Attribute
    {
    }
}

