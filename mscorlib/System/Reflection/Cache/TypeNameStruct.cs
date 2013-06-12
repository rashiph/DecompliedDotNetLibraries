namespace System.Reflection.Cache
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TypeNameStruct
    {
        internal IntPtr HashKey;
        internal string TypeName;
    }
}

