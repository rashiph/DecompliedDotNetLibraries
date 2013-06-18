namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Policy;
    using System.ServiceModel.Channels;

    internal class SCTServiceAuthenticationManagerWrapper : ServiceAuthenticationManager
    {
        private ServiceAuthenticationManager wrappedAuthenticationManager;

        internal SCTServiceAuthenticationManagerWrapper(ServiceAuthenticationManager wrappedServiceAuthManager)
        {
            if (wrappedServiceAuthManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wrappedServiceAuthManager");
            }
            this.wrappedAuthenticationManager = wrappedServiceAuthManager;
        }

        public override ReadOnlyCollection<IAuthorizationPolicy> Authenticate(ReadOnlyCollection<IAuthorizationPolicy> authPolicy, Uri listenUri, ref Message message)
        {
            if ((((message != null) && (message.Properties != null)) && ((message.Properties.Security != null) && (message.Properties.Security.TransportToken != null))) && ((message.Properties.Security.ServiceSecurityContext != null) && (message.Properties.Security.ServiceSecurityContext.AuthorizationPolicies != null)))
            {
                List<IAuthorizationPolicy> list = new List<IAuthorizationPolicy>(message.Properties.Security.ServiceSecurityContext.AuthorizationPolicies);
                foreach (IAuthorizationPolicy policy in message.Properties.Security.TransportToken.SecurityTokenPolicies)
                {
                    list.Remove(policy);
                }
                authPolicy = list.AsReadOnly();
            }
            return this.wrappedAuthenticationManager.Authenticate(authPolicy, listenUri, ref message);
        }
    }
}

