namespace System.ServiceModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    internal class ServiceAuthenticationManagerWrapper : ServiceAuthenticationManager
    {
        private string[] filteredActionUriCollection;
        private ServiceAuthenticationManager wrappedAuthenticationManager;

        internal ServiceAuthenticationManagerWrapper(ServiceAuthenticationManager wrappedServiceAuthManager, string[] actionUriFilter)
        {
            if (wrappedServiceAuthManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wrappedServiceAuthManager");
            }
            if ((actionUriFilter != null) && (actionUriFilter.Length > 0))
            {
                this.filteredActionUriCollection = new string[actionUriFilter.Length];
                for (int i = 0; i < actionUriFilter.Length; i++)
                {
                    this.filteredActionUriCollection[i] = actionUriFilter[i];
                }
            }
            this.wrappedAuthenticationManager = wrappedServiceAuthManager;
        }

        public override ReadOnlyCollection<IAuthorizationPolicy> Authenticate(ReadOnlyCollection<IAuthorizationPolicy> authPolicy, Uri listenUri, ref Message message)
        {
            if (this.CanSkipAuthentication(message))
            {
                return authPolicy;
            }
            if (this.filteredActionUriCollection != null)
            {
                for (int i = 0; i < this.filteredActionUriCollection.Length; i++)
                {
                    if (((message != null) && (message.Headers != null)) && (!string.IsNullOrEmpty(message.Headers.Action) && (message.Headers.Action == this.filteredActionUriCollection[i])))
                    {
                        return authPolicy;
                    }
                }
            }
            return this.wrappedAuthenticationManager.Authenticate(authPolicy, listenUri, ref message);
        }

        private bool CanSkipAuthentication(Message message)
        {
            if (((message != null) && (message.Properties != null)) && ((message.Properties.Security != null) && (message.Properties.Security.TransportToken == null)))
            {
                if (((message.Properties.Security.ProtectionToken != null) && (message.Properties.Security.ProtectionToken.SecurityToken != null)) && (message.Properties.Security.ProtectionToken.SecurityToken.GetType() == typeof(SecurityContextSecurityToken)))
                {
                    return true;
                }
                if (message.Properties.Security.HasIncomingSupportingTokens)
                {
                    foreach (SupportingTokenSpecification specification in message.Properties.Security.IncomingSupportingTokens)
                    {
                        if ((specification.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing) && (specification.SecurityToken.GetType() == typeof(SecurityContextSecurityToken)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}

