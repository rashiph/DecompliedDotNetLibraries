namespace System.Web.SessionState
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7297744b-e188-40bf-b7e9-56698d25cf44")]
    public interface IStateRuntime
    {
        void StopProcessing();
        void ProcessRequest([In, MarshalAs(UnmanagedType.SysInt)] IntPtr tracker, [In, MarshalAs(UnmanagedType.I4)] int verb, [In, MarshalAs(UnmanagedType.LPWStr)] string uri, [In, MarshalAs(UnmanagedType.I4)] int exclusive, [In, MarshalAs(UnmanagedType.I4)] int timeout, [In, MarshalAs(UnmanagedType.I4)] int lockCookieExists, [In, MarshalAs(UnmanagedType.I4)] int lockCookie, [In, MarshalAs(UnmanagedType.I4)] int contentLength, [In, MarshalAs(UnmanagedType.SysInt)] IntPtr content);
        void ProcessRequest([In, MarshalAs(UnmanagedType.SysInt)] IntPtr tracker, [In, MarshalAs(UnmanagedType.I4)] int verb, [In, MarshalAs(UnmanagedType.LPWStr)] string uri, [In, MarshalAs(UnmanagedType.I4)] int exclusive, [In, MarshalAs(UnmanagedType.I4)] int extraFlags, [In, MarshalAs(UnmanagedType.I4)] int timeout, [In, MarshalAs(UnmanagedType.I4)] int lockCookieExists, [In, MarshalAs(UnmanagedType.I4)] int lockCookie, [In, MarshalAs(UnmanagedType.I4)] int contentLength, [In, MarshalAs(UnmanagedType.SysInt)] IntPtr content);
    }
}

