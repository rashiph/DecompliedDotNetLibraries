namespace System.Runtime.Remoting.Proxies
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MessageData
    {
        internal IntPtr pFrame;
        internal IntPtr pMethodDesc;
        internal IntPtr pDelegateMD;
        internal IntPtr pSig;
        internal IntPtr thGoverningType;
        internal int iFlags;
    }
}

