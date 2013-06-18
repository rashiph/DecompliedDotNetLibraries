namespace System.DirectoryServices.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    internal struct AdsValue
    {
        [FieldOffset(0)]
        public int dwType;
        [FieldOffset(8)]
        public Ads_Generic generic;
        [FieldOffset(8)]
        public Ads_OctetString octetString;
        [FieldOffset(4)]
        internal int pad;
        [FieldOffset(8)]
        public Ads_Pointer pointer;
    }
}

