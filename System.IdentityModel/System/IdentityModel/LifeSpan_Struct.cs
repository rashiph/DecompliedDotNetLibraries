namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct LifeSpan_Struct
    {
        internal long start;
        internal long end;
        internal static readonly int Size;
        static LifeSpan_Struct()
        {
            Size = Marshal.SizeOf(typeof(LifeSpan_Struct));
        }
    }
}

