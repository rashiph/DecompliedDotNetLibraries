namespace System.EnterpriseServices.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("5AC4CB7E-F89F-429b-926B-C7F940936BF4")]
    public interface ISoapUtility
    {
        [DispId(1)]
        void GetServerPhysicalPath([MarshalAs(UnmanagedType.BStr)] string rootWebServer, [MarshalAs(UnmanagedType.BStr)] string inBaseUrl, [MarshalAs(UnmanagedType.BStr)] string inVirtualRoot, [MarshalAs(UnmanagedType.BStr)] out string physicalPath);
        [DispId(2)]
        void GetServerBinPath([MarshalAs(UnmanagedType.BStr)] string rootWebServer, [MarshalAs(UnmanagedType.BStr)] string inBaseUrl, [MarshalAs(UnmanagedType.BStr)] string inVirtualRoot, [MarshalAs(UnmanagedType.BStr)] out string binPath);
        [DispId(3)]
        void Present();
    }
}

