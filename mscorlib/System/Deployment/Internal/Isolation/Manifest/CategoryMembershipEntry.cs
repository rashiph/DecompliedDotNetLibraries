namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class CategoryMembershipEntry
    {
        public IDefinitionIdentity Identity;
        public ISection SubcategoryMembership;
    }
}

