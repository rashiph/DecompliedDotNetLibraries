namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1), ComVisible(false)]
    public struct BOID
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
        public byte[] rgb;
    }
}

