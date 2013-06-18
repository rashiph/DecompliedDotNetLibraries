namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("02998279-7175-4d59-aa5a-fb8e44d4ca9d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAppManagerAppDomainFactory
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        object Create([In, MarshalAs(UnmanagedType.BStr)] string appId, [In, MarshalAs(UnmanagedType.BStr)] string appPath);
        void Stop();
    }
}

