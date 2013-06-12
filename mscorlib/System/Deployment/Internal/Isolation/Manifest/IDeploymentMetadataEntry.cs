namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CFA3F59F-334D-46bf-A5A5-5D11BB2D7EBC")]
    internal interface IDeploymentMetadataEntry
    {
        DeploymentMetadataEntry AllData { [SecurityCritical] get; }
        string DeploymentProviderCodebase { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string MinimumRequiredVersion { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        ushort MaximumAge { [SecurityCritical] get; }
        byte MaximumAge_Unit { [SecurityCritical] get; }
        uint DeploymentFlags { [SecurityCritical] get; }
    }
}

