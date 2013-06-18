namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct PLAINTEXTKEYBLOBHEADER
    {
        internal byte bType;
        internal byte bVersion;
        internal short reserved;
        internal int aiKeyAlg;
        internal int keyLength;
        internal static readonly int SizeOf;
        static PLAINTEXTKEYBLOBHEADER()
        {
            SizeOf = Marshal.SizeOf(typeof(PLAINTEXTKEYBLOBHEADER));
        }
    }
}

