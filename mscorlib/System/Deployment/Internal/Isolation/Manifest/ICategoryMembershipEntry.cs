namespace System.Deployment.Internal.Isolation.Manifest
{
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("97FDCA77-B6F2-4718-A1EB-29D0AECE9C03")]
    internal interface ICategoryMembershipEntry
    {
        CategoryMembershipEntry AllData { [SecurityCritical] get; }
        IDefinitionIdentity Identity { [SecurityCritical] get; }
        ISection SubcategoryMembership { [SecurityCritical] get; }
    }
}

