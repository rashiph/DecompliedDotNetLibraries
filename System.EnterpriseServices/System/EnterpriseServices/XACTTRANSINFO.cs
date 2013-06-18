namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4), ComVisible(false)]
    public struct XACTTRANSINFO
    {
        public BOID uow;
        public int isoLevel;
        public int isoFlags;
        public int grfTCSupported;
        public int grfRMSupported;
        public int grfTCSupportedRetaining;
        public int grfRMSupportedRetaining;
    }
}

