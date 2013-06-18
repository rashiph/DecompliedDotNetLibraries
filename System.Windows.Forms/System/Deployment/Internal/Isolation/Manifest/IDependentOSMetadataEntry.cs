namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("CF168CF4-4E8F-4d92-9D2A-60E5CA21CF85"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDependentOSMetadataEntry
    {
        System.Deployment.Internal.Isolation.Manifest.DependentOSMetadataEntry AllData { [SecurityCritical] get; }
        string SupportUrl { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string Description { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        ushort MajorVersion { [SecurityCritical] get; }
        ushort MinorVersion { [SecurityCritical] get; }
        ushort BuildNumber { [SecurityCritical] get; }
        byte ServicePackMajor { [SecurityCritical] get; }
        byte ServicePackMinor { [SecurityCritical] get; }
    }
}

