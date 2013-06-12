namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum MethodImplOptions
    {
        ForwardRef = 0x10,
        InternalCall = 0x1000,
        NoInlining = 8,
        NoOptimization = 0x40,
        PreserveSig = 0x80,
        Synchronized = 0x20,
        Unmanaged = 4
    }
}

