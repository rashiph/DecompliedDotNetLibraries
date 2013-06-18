namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Security.Principal;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;

    public class ServiceSecurityContext
    {
        private static ServiceSecurityContext anonymous;
        private System.IdentityModel.Policy.AuthorizationContext authorizationContext;
        private ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies;
        private Claim identityClaim;
        private IIdentity primaryIdentity;
        private System.Security.Principal.WindowsIdentity windowsIdentity;

        public ServiceSecurityContext(ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            if (authorizationPolicies == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authorizationPolicies");
            }
            this.authorizationContext = null;
            this.authorizationPolicies = authorizationPolicies;
        }

        public ServiceSecurityContext(System.IdentityModel.Policy.AuthorizationContext authorizationContext) : this(authorizationContext, EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance)
        {
        }

        public ServiceSecurityContext(System.IdentityModel.Policy.AuthorizationContext authorizationContext, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            if (authorizationContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authorizationContext");
            }
            if (authorizationPolicies == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authorizationPolicies");
            }
            this.authorizationContext = authorizationContext;
            this.authorizationPolicies = authorizationPolicies;
        }

        private IList<IIdentity> GetIdentities()
        {
            object obj2;
            System.IdentityModel.Policy.AuthorizationContext authorizationContext = this.AuthorizationContext;
            if ((authorizationContext != null) && authorizationContext.Properties.TryGetValue("Identities", out obj2))
            {
                return (obj2 as IList<IIdentity>);
            }
            return null;
        }

        public static ServiceSecurityContext Anonymous
        {
            get
            {
                if (anonymous == null)
                {
                    anonymous = new ServiceSecurityContext(EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance);
                }
                return anonymous;
            }
        }

        public System.IdentityModel.Policy.AuthorizationContext AuthorizationContext
        {
            get
            {
                if (this.authorizationContext == null)
                {
                    this.authorizationContext = System.IdentityModel.Policy.AuthorizationContext.CreateDefaultAuthorizationContext(this.authorizationPolicies);
                }
                return this.authorizationContext;
            }
        }

        public ReadOnlyCollection<IAuthorizationPolicy> AuthorizationPolicies
        {
            get
            {
                return this.authorizationPolicies;
            }
            set
            {
                this.authorizationPolicies = value;
            }
        }

        public static ServiceSecurityContext Current
        {
            get
            {
                ServiceSecurityContext serviceSecurityContext = null;
                OperationContext current = OperationContext.Current;
                if (current != null)
                {
                    MessageProperties incomingMessageProperties = current.IncomingMessageProperties;
                    if (incomingMessageProperties != null)
                    {
                        SecurityMessageProperty security = incomingMessageProperties.Security;
                        if (security != null)
                        {
                            serviceSecurityContext = security.ServiceSecurityContext;
                        }
                    }
                }
                return serviceSecurityContext;
            }
        }

        internal Claim IdentityClaim
        {
            get
            {
                if (this.identityClaim == null)
                {
                    this.identityClaim = System.ServiceModel.Security.SecurityUtils.GetPrimaryIdentityClaim(this.AuthorizationContext);
                }
                return this.identityClaim;
            }
        }

        public bool IsAnonymous
        {
            get
            {
                if (this != Anonymous)
                {
                    return (this.IdentityClaim == null);
                }
                return true;
            }
        }

        public IIdentity PrimaryIdentity
        {
            get
            {
                if (this.primaryIdentity == null)
                {
                    IIdentity identity = null;
                    IList<IIdentity> identities = this.GetIdentities();
                    if ((identities != null) && (identities.Count == 1))
                    {
                        identity = identities[0];
                    }
                    this.primaryIdentity = identity ?? System.ServiceModel.Security.SecurityUtils.AnonymousIdentity;
                }
                return this.primaryIdentity;
            }
        }

        public System.Security.Principal.WindowsIdentity WindowsIdentity
        {
            get
            {
                if (this.windowsIdentity == null)
                {
                    System.Security.Principal.WindowsIdentity anonymous = null;
                    IList<IIdentity> identities = this.GetIdentities();
                    if (identities != null)
                    {
                        for (int i = 0; i < identities.Count; i++)
                        {
                            System.Security.Principal.WindowsIdentity identity2 = identities[i] as System.Security.Principal.WindowsIdentity;
                            if (identity2 != null)
                            {
                                if (anonymous != null)
                                {
                                    anonymous = System.Security.Principal.WindowsIdentity.GetAnonymous();
                                    break;
                                }
                                anonymous = identity2;
                            }
                        }
                    }
                    this.windowsIdentity = anonymous ?? System.Security.Principal.WindowsIdentity.GetAnonymous();
                }
                return this.windowsIdentity;
            }
        }
    }
}

