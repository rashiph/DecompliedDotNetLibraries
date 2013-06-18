namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class CategoryMembershipEntry
    {
        public System.Deployment.Internal.Isolation.IDefinitionIdentity Identity;
        public System.Deployment.Internal.Isolation.ISection SubcategoryMembership;
    }
}

