namespace System.Deployment.Internal.Isolation.Manifest
{
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("a504e5b0-8ccf-4cb4-9902-c9d1b9abd033")]
    internal interface ICMS
    {
        System.Deployment.Internal.Isolation.IDefinitionIdentity Identity { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection FileSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection CategoryMembershipSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection COMRedirectionSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection ProgIdRedirectionSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection CLRSurrogateSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection AssemblyReferenceSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection WindowClassSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection StringSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection EntryPointSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection PermissionSetSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISectionEntry MetadataSectionEntry { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection AssemblyRequestSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection RegistryKeySection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection DirectorySection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection FileAssociationSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection CompatibleFrameworksSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection EventSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection EventMapSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection EventTagSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection CounterSetSection { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection CounterSection { [SecurityCritical] get; }
    }
}

