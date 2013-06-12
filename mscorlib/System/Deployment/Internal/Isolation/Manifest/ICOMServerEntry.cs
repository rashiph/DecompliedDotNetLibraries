namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("3903B11B-FBE8-477c-825F-DB828B5FD174")]
    internal interface ICOMServerEntry
    {
        COMServerEntry AllData { [SecurityCritical] get; }
        Guid Clsid { [SecurityCritical] get; }
        uint Flags { [SecurityCritical] get; }
        Guid ConfiguredGuid { [SecurityCritical] get; }
        Guid ImplementedClsid { [SecurityCritical] get; }
        Guid TypeLibrary { [SecurityCritical] get; }
        uint ThreadingModel { [SecurityCritical] get; }
        string RuntimeVersion { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string HostFile { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
    }
}

