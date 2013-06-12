namespace System.Security
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), AttributeUsage(AttributeTargets.Module, AllowMultiple=true, Inherited=false)]
    public sealed class UnverifiableCodeAttribute : Attribute
    {
    }
}

