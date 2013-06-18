namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("9d98b251-453e-44f6-9cec-8b5aed970129")]
    public interface IProcessHostIdleAndHealthCheck
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsIdle();
        void Ping(IProcessPingCallback callback);
    }
}

