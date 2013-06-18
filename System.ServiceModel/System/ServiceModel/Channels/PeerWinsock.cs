namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;

    internal static class PeerWinsock
    {
        [DllImport("ws2_32.dll", SetLastError=true)]
        internal static extern int WSAIoctl([In] IntPtr socketHandle, [In] int ioControlCode, [In] IntPtr inBuffer, [In] int inBufferSize, [Out] IntPtr outBuffer, [In] int outBufferSize, out int bytesTransferred, [In] IntPtr overlapped, [In] IntPtr completionRoutine);
    }
}

