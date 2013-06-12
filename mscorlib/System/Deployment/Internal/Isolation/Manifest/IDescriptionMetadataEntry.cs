namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CB73147E-5FC2-4c31-B4E6-58D13DBE1A08")]
    internal interface IDescriptionMetadataEntry
    {
        DescriptionMetadataEntry AllData { [SecurityCritical] get; }
        string Publisher { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string Product { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string SupportUrl { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string IconFile { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string ErrorReportUrl { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string SuiteName { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
    }
}

