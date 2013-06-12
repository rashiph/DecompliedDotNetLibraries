namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("dc3b0a85-9da7-47e4-ba1b-e27da9db8a1e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IListenerChannelCallback
    {
        void ReportStarted();
        void ReportStopped(int hr);
        void ReportMessageReceived();
        int GetId();
        int GetBlobLength();
        void GetBlob([In, Out, MarshalAs(UnmanagedType.LPArray)] byte[] buffer, ref int bufferSize);
    }
}

