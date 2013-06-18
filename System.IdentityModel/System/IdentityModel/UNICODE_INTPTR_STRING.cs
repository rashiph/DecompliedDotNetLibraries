namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct UNICODE_INTPTR_STRING
    {
        internal ushort Length;
        internal ushort MaxLength;
        internal IntPtr Buffer;
        internal UNICODE_INTPTR_STRING(int length, int maximumLength, IntPtr buffer)
        {
            this.Length = (ushort) length;
            this.MaxLength = (ushort) maximumLength;
            this.Buffer = buffer;
        }
    }
}

