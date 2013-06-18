namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("e6e21054-a7dc-4378-877d-b7f4a2d7e8ba"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAppDomainFactory
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        object Create([In, MarshalAs(UnmanagedType.BStr)] string module, [In, MarshalAs(UnmanagedType.BStr)] string typeName, [In, MarshalAs(UnmanagedType.BStr)] string appId, [In, MarshalAs(UnmanagedType.BStr)] string appPath, [In, MarshalAs(UnmanagedType.BStr)] string strUrlOfAppOrigin, [In, MarshalAs(UnmanagedType.I4)] int iZone);
    }
}

