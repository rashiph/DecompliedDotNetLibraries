namespace System.Web.Services.Interop
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity, Guid("C43CC2F3-90AF-4e93-9112-DFB8B36749B5")]
    internal interface INotifySink2
    {
        void OnSyncCallOut([In] CallId callId, out IntPtr out_ppBuffer, [In, Out] ref int inout_pBufferSize);
        void OnSyncCallEnter([In] CallId callId, [In, MarshalAs(UnmanagedType.LPArray)] byte[] in_pBuffer, [In] int in_BufferSize);
        void OnSyncCallReturn([In] CallId callId, [In, MarshalAs(UnmanagedType.LPArray)] byte[] in_pBuffer, [In] int in_BufferSize);
        void OnSyncCallExit([In] CallId callId, out IntPtr out_ppBuffer, [In, Out] ref int inout_pBufferSize);
    }
}

