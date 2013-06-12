namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("CB73147E-5FC2-4c31-B4E6-58D13DBE1A08"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDescriptionMetadataEntry
    {
        System.Deployment.Internal.Isolation.Manifest.DescriptionMetadataEntry AllData { [SecurityCritical] get; }
        string Publisher { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string Product { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string SupportUrl { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string IconFile { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string ErrorReportUrl { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string SuiteName { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
    }
}

