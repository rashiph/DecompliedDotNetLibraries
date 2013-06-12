namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), AttributeUsage(AttributeTargets.Struct, Inherited=true)]
    public sealed class NativeCppClassAttribute : Attribute
    {
    }
}

