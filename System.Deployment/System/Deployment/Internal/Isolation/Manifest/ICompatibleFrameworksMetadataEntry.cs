namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("4A33D662-2210-463A-BE9F-FBDF1AA554E3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICompatibleFrameworksMetadataEntry
    {
        System.Deployment.Internal.Isolation.Manifest.CompatibleFrameworksMetadataEntry AllData { [SecurityCritical] get; }
        string SupportUrl { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
    }
}

