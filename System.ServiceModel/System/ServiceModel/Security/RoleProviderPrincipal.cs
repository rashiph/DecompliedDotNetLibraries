namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Web.Security;

    internal sealed class RoleProviderPrincipal : IPrincipal
    {
        private object roleProvider;
        private ServiceSecurityContext securityContext;

        public RoleProviderPrincipal(object roleProvider, ServiceSecurityContext securityContext)
        {
            this.roleProvider = roleProvider;
            this.securityContext = securityContext;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool IsInRole(string role)
        {
            RoleProvider provider = (this.roleProvider as RoleProvider) ?? SystemWebHelper.GetDefaultRoleProvider();
            return ((provider != null) && provider.IsUserInRole(this.securityContext.PrimaryIdentity.Name, role));
        }

        public IIdentity Identity
        {
            get
            {
                return this.securityContext.PrimaryIdentity;
            }
        }
    }
}

