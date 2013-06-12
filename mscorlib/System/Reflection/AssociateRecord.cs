namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct AssociateRecord
    {
        internal int MethodDefToken;
        internal MethodSemanticsAttributes Semantics;
    }
}

