namespace System.DirectoryServices.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct AdsSearchPreferenceInfo
    {
        public int dwSearchPref;
        internal int pad;
        public AdsValue vValue;
        public int dwStatus;
        internal int pad2;
    }
}

