namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;

    public class SecurityMessageProperty : IMessageProperty, IDisposable
    {
        private bool disposed;
        private ReadOnlyCollection<IAuthorizationPolicy> externalAuthorizationPolicies;
        private Collection<SupportingTokenSpecification> incomingSupportingTokens;
        private SecurityTokenSpecification initiatorToken;
        private Collection<SupportingTokenSpecification> outgoingSupportingTokens;
        private SecurityTokenSpecification protectionToken;
        private SecurityTokenSpecification recipientToken;
        private System.ServiceModel.ServiceSecurityContext securityContext = System.ServiceModel.ServiceSecurityContext.Anonymous;
        private string senderIdPrefix = "_";
        private SecurityTokenSpecification transportToken;

        private void AddAuthorizationPolicies(SecurityTokenSpecification spec, Collection<IAuthorizationPolicy> policies)
        {
            if (((spec != null) && (spec.SecurityTokenPolicies != null)) && (spec.SecurityTokenPolicies.Count > 0))
            {
                for (int i = 0; i < spec.SecurityTokenPolicies.Count; i++)
                {
                    policies.Add(spec.SecurityTokenPolicies[i]);
                }
            }
        }

        public IMessageProperty CreateCopy()
        {
            this.ThrowIfDisposed();
            SecurityMessageProperty property = new SecurityMessageProperty();
            if (this.HasOutgoingSupportingTokens)
            {
                for (int i = 0; i < this.outgoingSupportingTokens.Count; i++)
                {
                    property.OutgoingSupportingTokens.Add(this.outgoingSupportingTokens[i]);
                }
            }
            if (this.HasIncomingSupportingTokens)
            {
                for (int j = 0; j < this.incomingSupportingTokens.Count; j++)
                {
                    property.IncomingSupportingTokens.Add(this.incomingSupportingTokens[j]);
                }
            }
            property.securityContext = this.securityContext;
            property.externalAuthorizationPolicies = this.externalAuthorizationPolicies;
            property.senderIdPrefix = this.senderIdPrefix;
            property.protectionToken = this.protectionToken;
            property.initiatorToken = this.initiatorToken;
            property.recipientToken = this.recipientToken;
            property.transportToken = this.transportToken;
            return property;
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
            }
        }

        internal ReadOnlyCollection<IAuthorizationPolicy> GetInitiatorTokenAuthorizationPolicies()
        {
            return this.GetInitiatorTokenAuthorizationPolicies(true);
        }

        internal ReadOnlyCollection<IAuthorizationPolicy> GetInitiatorTokenAuthorizationPolicies(bool includeTransportToken)
        {
            return this.GetInitiatorTokenAuthorizationPolicies(includeTransportToken, null);
        }

        internal ReadOnlyCollection<IAuthorizationPolicy> GetInitiatorTokenAuthorizationPolicies(bool includeTransportToken, SecurityContextSecurityToken supportingSessionTokenToExclude)
        {
            if (!this.HasIncomingSupportingTokens)
            {
                if (((this.transportToken != null) && (this.initiatorToken == null)) && (this.protectionToken == null))
                {
                    if (includeTransportToken && (this.transportToken.SecurityTokenPolicies != null))
                    {
                        return this.transportToken.SecurityTokenPolicies;
                    }
                    return EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
                }
                if (((this.transportToken == null) && (this.initiatorToken != null)) && (this.protectionToken == null))
                {
                    return (this.initiatorToken.SecurityTokenPolicies ?? EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance);
                }
                if (((this.transportToken == null) && (this.initiatorToken == null)) && (this.protectionToken != null))
                {
                    return (this.protectionToken.SecurityTokenPolicies ?? EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance);
                }
            }
            Collection<IAuthorizationPolicy> policies = new Collection<IAuthorizationPolicy>();
            if (includeTransportToken)
            {
                this.AddAuthorizationPolicies(this.transportToken, policies);
            }
            this.AddAuthorizationPolicies(this.initiatorToken, policies);
            this.AddAuthorizationPolicies(this.protectionToken, policies);
            if (this.HasIncomingSupportingTokens)
            {
                for (int i = 0; i < this.incomingSupportingTokens.Count; i++)
                {
                    if (supportingSessionTokenToExclude != null)
                    {
                        SecurityContextSecurityToken securityToken = this.incomingSupportingTokens[i].SecurityToken as SecurityContextSecurityToken;
                        if ((securityToken != null) && (securityToken.ContextId == supportingSessionTokenToExclude.ContextId))
                        {
                            continue;
                        }
                    }
                    switch (this.incomingSupportingTokens[i].SecurityTokenAttachmentMode)
                    {
                        case SecurityTokenAttachmentMode.Endorsing:
                        case SecurityTokenAttachmentMode.Signed:
                        case SecurityTokenAttachmentMode.SignedEncrypted:
                        case SecurityTokenAttachmentMode.SignedEndorsing:
                            this.AddAuthorizationPolicies(this.incomingSupportingTokens[i], policies);
                            break;
                    }
                }
            }
            return new ReadOnlyCollection<IAuthorizationPolicy>(policies);
        }

        public static SecurityMessageProperty GetOrCreate(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            SecurityMessageProperty security = null;
            if (message.Properties != null)
            {
                security = message.Properties.Security;
            }
            if (security == null)
            {
                security = new SecurityMessageProperty();
                message.Properties.Security = security;
            }
            return security;
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().FullName));
            }
        }

        public ReadOnlyCollection<IAuthorizationPolicy> ExternalAuthorizationPolicies
        {
            get
            {
                return this.externalAuthorizationPolicies;
            }
            set
            {
                this.externalAuthorizationPolicies = value;
            }
        }

        public bool HasIncomingSupportingTokens
        {
            get
            {
                this.ThrowIfDisposed();
                return ((this.incomingSupportingTokens != null) && (this.incomingSupportingTokens.Count > 0));
            }
        }

        internal bool HasOutgoingSupportingTokens
        {
            get
            {
                return ((this.outgoingSupportingTokens != null) && (this.outgoingSupportingTokens.Count > 0));
            }
        }

        public Collection<SupportingTokenSpecification> IncomingSupportingTokens
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.incomingSupportingTokens == null)
                {
                    this.incomingSupportingTokens = new Collection<SupportingTokenSpecification>();
                }
                return this.incomingSupportingTokens;
            }
        }

        public SecurityTokenSpecification InitiatorToken
        {
            get
            {
                this.ThrowIfDisposed();
                return this.initiatorToken;
            }
            set
            {
                this.ThrowIfDisposed();
                this.initiatorToken = value;
            }
        }

        public Collection<SupportingTokenSpecification> OutgoingSupportingTokens
        {
            get
            {
                if (this.outgoingSupportingTokens == null)
                {
                    this.outgoingSupportingTokens = new Collection<SupportingTokenSpecification>();
                }
                return this.outgoingSupportingTokens;
            }
        }

        public SecurityTokenSpecification ProtectionToken
        {
            get
            {
                this.ThrowIfDisposed();
                return this.protectionToken;
            }
            set
            {
                this.ThrowIfDisposed();
                this.protectionToken = value;
            }
        }

        public SecurityTokenSpecification RecipientToken
        {
            get
            {
                this.ThrowIfDisposed();
                return this.recipientToken;
            }
            set
            {
                this.ThrowIfDisposed();
                this.recipientToken = value;
            }
        }

        public string SenderIdPrefix
        {
            get
            {
                return this.senderIdPrefix;
            }
            set
            {
                XmlHelper.ValidateIdPrefix(value);
                this.senderIdPrefix = value;
            }
        }

        public System.ServiceModel.ServiceSecurityContext ServiceSecurityContext
        {
            get
            {
                this.ThrowIfDisposed();
                return this.securityContext;
            }
            set
            {
                this.ThrowIfDisposed();
                this.securityContext = value;
            }
        }

        public SecurityTokenSpecification TransportToken
        {
            get
            {
                this.ThrowIfDisposed();
                return this.transportToken;
            }
            set
            {
                this.ThrowIfDisposed();
                this.transportToken = value;
            }
        }
    }
}

