namespace System.Web.Services.Discovery
{
    using System;
    using System.Runtime;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class DiscoverySearchPattern
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected DiscoverySearchPattern()
        {
        }

        public abstract DiscoveryReference GetDiscoveryReference(string filename);

        public abstract string Pattern { get; }
    }
}

