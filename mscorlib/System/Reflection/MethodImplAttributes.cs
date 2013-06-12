namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum MethodImplAttributes
    {
        CodeTypeMask = 3,
        ForwardRef = 0x10,
        IL = 0,
        InternalCall = 0x1000,
        Managed = 0,
        ManagedMask = 4,
        MaxMethodImplVal = 0xffff,
        Native = 1,
        NoInlining = 8,
        NoOptimization = 0x40,
        OPTIL = 2,
        PreserveSig = 0x80,
        Runtime = 3,
        Synchronized = 0x20,
        Unmanaged = 4
    }
}

