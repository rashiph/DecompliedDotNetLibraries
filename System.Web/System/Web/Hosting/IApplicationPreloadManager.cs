namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("AE54F424-71BC-4da5-AA2F-8C0CD53496FC"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IApplicationPreloadManager
    {
        void SetApplicationPreloadUtil([In, MarshalAs(UnmanagedType.Interface)] IApplicationPreloadUtil preloadUtil);
        void SetApplicationPreloadState([In, MarshalAs(UnmanagedType.LPWStr)] string context, [In, MarshalAs(UnmanagedType.LPWStr)] string appId, [In, MarshalAs(UnmanagedType.Bool)] bool enabled);
    }
}

