namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("AB1ED79F-943E-407d-A80B-0744E3A95B28"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMetadataSectionEntry
    {
        System.Deployment.Internal.Isolation.Manifest.MetadataSectionEntry AllData { [SecurityCritical] get; }
        uint SchemaVersion { [SecurityCritical] get; }
        uint ManifestFlags { [SecurityCritical] get; }
        uint UsagePatterns { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.IDefinitionIdentity CdfIdentity { [SecurityCritical] get; }
        string LocalPath { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        uint HashAlgorithm { [SecurityCritical] get; }
        object ManifestHash { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
        string ContentType { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string RuntimeImageVersion { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        object MvidValue { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.Manifest.IDescriptionMetadataEntry DescriptionData { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.Manifest.IDeploymentMetadataEntry DeploymentData { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.Manifest.IDependentOSMetadataEntry DependentOSData { [SecurityCritical] get; }
        string defaultPermissionSetID { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string RequestedExecutionLevel { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        bool RequestedExecutionLevelUIAccess { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.IReferenceIdentity ResourceTypeResourcesDependency { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.IReferenceIdentity ResourceTypeManifestResourcesDependency { [SecurityCritical] get; }
        string KeyInfoElement { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.Manifest.ICompatibleFrameworksMetadataEntry CompatibleFrameworksData { [SecurityCritical] get; }
    }
}

