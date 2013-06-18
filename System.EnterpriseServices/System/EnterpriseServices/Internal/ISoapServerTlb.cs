namespace System.EnterpriseServices.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("1E7BA9F7-21DB-4482-929E-21BDE2DFE51C")]
    public interface ISoapServerTlb
    {
        [DispId(1)]
        void AddServerTlb([MarshalAs(UnmanagedType.BStr)] string progId, [MarshalAs(UnmanagedType.BStr)] string classId, [MarshalAs(UnmanagedType.BStr)] string interfaceId, [MarshalAs(UnmanagedType.BStr)] string srcTlbPath, [MarshalAs(UnmanagedType.BStr)] string rootWebServer, [MarshalAs(UnmanagedType.BStr)] string baseUrl, [MarshalAs(UnmanagedType.BStr)] string virtualRoot, [MarshalAs(UnmanagedType.BStr)] string clientActivated, [MarshalAs(UnmanagedType.BStr)] string wellKnown, [MarshalAs(UnmanagedType.BStr)] string discoFile, [MarshalAs(UnmanagedType.BStr)] string operation, [MarshalAs(UnmanagedType.BStr)] out string assemblyName, [MarshalAs(UnmanagedType.BStr)] out string typeName);
        [DispId(2)]
        void DeleteServerTlb([MarshalAs(UnmanagedType.BStr)] string progId, [MarshalAs(UnmanagedType.BStr)] string classId, [MarshalAs(UnmanagedType.BStr)] string interfaceId, [MarshalAs(UnmanagedType.BStr)] string srcTlbPath, [MarshalAs(UnmanagedType.BStr)] string rootWebServer, [MarshalAs(UnmanagedType.BStr)] string baseUrl, [MarshalAs(UnmanagedType.BStr)] string virtualRoot, [MarshalAs(UnmanagedType.BStr)] string operation, [MarshalAs(UnmanagedType.BStr)] string assemblyName, [MarshalAs(UnmanagedType.BStr)] string typeName);
    }
}

