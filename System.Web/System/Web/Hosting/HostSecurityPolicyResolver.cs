namespace System.Web.Hosting
{
    using System;
    using System.Security.Permissions;
    using System.Security.Policy;

    [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public class HostSecurityPolicyResolver
    {
        public virtual HostSecurityPolicyResults ResolvePolicy(Evidence evidence)
        {
            return HostSecurityPolicyResults.DefaultPolicy;
        }
    }
}

