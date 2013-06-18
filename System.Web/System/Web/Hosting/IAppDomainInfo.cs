namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("5BC9C234-6CD7-49bf-A07A-6FDB7F22DFFF")]
    public interface IAppDomainInfo
    {
        [return: MarshalAs(UnmanagedType.BStr)]
        string GetId();
        [return: MarshalAs(UnmanagedType.BStr)]
        string GetVirtualPath();
        [return: MarshalAs(UnmanagedType.BStr)]
        string GetPhysicalPath();
        [return: MarshalAs(UnmanagedType.I4)]
        int GetSiteId();
        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsIdle();
    }
}

