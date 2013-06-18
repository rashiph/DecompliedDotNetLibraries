namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;

    internal class SymmetricSecurityProtocolFactory : MessageSecurityProtocolFactory
    {
        private System.ServiceModel.Security.Tokens.SecurityTokenParameters protectionTokenParameters;
        private SecurityTokenProvider recipientAsymmetricTokenProvider;
        private ReadOnlyCollection<SecurityTokenResolver> recipientOutOfBandTokenResolverList;
        private SecurityTokenAuthenticator recipientSymmetricTokenAuthenticator;
        private System.ServiceModel.Security.Tokens.SecurityTokenParameters tokenParameters;

        public SymmetricSecurityProtocolFactory()
        {
        }

        internal SymmetricSecurityProtocolFactory(MessageSecurityProtocolFactory factory) : base(factory)
        {
        }

        private RecipientServiceModelSecurityTokenRequirement CreateRecipientTokenRequirement()
        {
            RecipientServiceModelSecurityTokenRequirement requirement = base.CreateRecipientSecurityTokenRequirement();
            this.SecurityTokenParameters.InitializeSecurityTokenRequirement(requirement);
            requirement.KeyUsage = this.SecurityTokenParameters.HasAsymmetricKey ? SecurityKeyUsage.Exchange : SecurityKeyUsage.Signature;
            return requirement;
        }

        public override EndpointIdentity GetIdentityOfSelf()
        {
            if (base.SecurityTokenManager is IEndpointIdentityProvider)
            {
                SecurityTokenRequirement requirement = base.CreateRecipientSecurityTokenRequirement();
                this.SecurityTokenParameters.InitializeSecurityTokenRequirement(requirement);
                return ((IEndpointIdentityProvider) base.SecurityTokenManager).GetIdentityOfSelf(requirement);
            }
            return base.GetIdentityOfSelf();
        }

        public override T GetProperty<T>()
        {
            if (!(typeof(T) == typeof(Collection<ISecurityContextSecurityTokenCache>)))
            {
                return base.GetProperty<T>();
            }
            Collection<ISecurityContextSecurityTokenCache> property = base.GetProperty<Collection<ISecurityContextSecurityTokenCache>>();
            if (this.recipientSymmetricTokenAuthenticator is ISecurityContextSecurityTokenCacheProvider)
            {
                property.Add(((ISecurityContextSecurityTokenCacheProvider) this.recipientSymmetricTokenAuthenticator).TokenCache);
            }
            return (T) property;
        }

        internal System.ServiceModel.Security.Tokens.SecurityTokenParameters GetProtectionTokenParameters()
        {
            return this.protectionTokenParameters;
        }

        public override void OnAbort()
        {
            if (!base.ActAsInitiator)
            {
                if (this.recipientSymmetricTokenAuthenticator != null)
                {
                    System.ServiceModel.Security.SecurityUtils.AbortTokenAuthenticatorIfRequired(this.recipientSymmetricTokenAuthenticator);
                }
                if (this.recipientAsymmetricTokenProvider != null)
                {
                    System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(this.recipientAsymmetricTokenProvider);
                }
            }
            base.OnAbort();
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (!base.ActAsInitiator)
            {
                if (this.recipientSymmetricTokenAuthenticator != null)
                {
                    System.ServiceModel.Security.SecurityUtils.CloseTokenAuthenticatorIfRequired(this.recipientSymmetricTokenAuthenticator, helper.RemainingTime());
                }
                if (this.recipientAsymmetricTokenProvider != null)
                {
                    System.ServiceModel.Security.SecurityUtils.CloseTokenProviderIfRequired(this.recipientAsymmetricTokenProvider, helper.RemainingTime());
                }
            }
            base.OnClose(helper.RemainingTime());
        }

        protected override SecurityProtocol OnCreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, TimeSpan timeout)
        {
            return new SymmetricSecurityProtocol(this, target, via);
        }

        public override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnOpen(helper.RemainingTime());
            if (this.tokenParameters == null)
            {
                base.OnPropertySettingsError("SecurityTokenParameters", true);
            }
            if (!base.ActAsInitiator)
            {
                SecurityTokenRequirement tokenRequirement = this.CreateRecipientTokenRequirement();
                SecurityTokenResolver outOfBandTokenResolver = null;
                if (this.SecurityTokenParameters.HasAsymmetricKey)
                {
                    this.recipientAsymmetricTokenProvider = base.SecurityTokenManager.CreateSecurityTokenProvider(tokenRequirement);
                }
                else
                {
                    this.recipientSymmetricTokenAuthenticator = base.SecurityTokenManager.CreateSecurityTokenAuthenticator(tokenRequirement, out outOfBandTokenResolver);
                }
                if ((this.RecipientSymmetricTokenAuthenticator != null) && (this.RecipientAsymmetricTokenProvider != null))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("OnlyOneOfEncryptedKeyOrSymmetricBindingCanBeSelected")));
                }
                if (outOfBandTokenResolver != null)
                {
                    Collection<SecurityTokenResolver> list = new Collection<SecurityTokenResolver> {
                        outOfBandTokenResolver
                    };
                    this.recipientOutOfBandTokenResolverList = new ReadOnlyCollection<SecurityTokenResolver>(list);
                }
                else
                {
                    this.recipientOutOfBandTokenResolverList = EmptyReadOnlyCollection<SecurityTokenResolver>.Instance;
                }
                if (this.RecipientAsymmetricTokenProvider != null)
                {
                    base.Open("RecipientAsymmetricTokenProvider", true, this.RecipientAsymmetricTokenProvider, helper.RemainingTime());
                }
                else
                {
                    base.Open("RecipientSymmetricTokenAuthenticator", true, this.RecipientSymmetricTokenAuthenticator, helper.RemainingTime());
                }
            }
            if (this.tokenParameters.RequireDerivedKeys)
            {
                base.ExpectKeyDerivation = true;
            }
            if (this.tokenParameters.HasAsymmetricKey)
            {
                this.protectionTokenParameters = new WrappedKeySecurityTokenParameters();
                this.protectionTokenParameters.RequireDerivedKeys = this.SecurityTokenParameters.RequireDerivedKeys;
            }
            else
            {
                this.protectionTokenParameters = this.tokenParameters;
            }
        }

        public SecurityTokenProvider RecipientAsymmetricTokenProvider
        {
            get
            {
                return this.recipientAsymmetricTokenProvider;
            }
        }

        public ReadOnlyCollection<SecurityTokenResolver> RecipientOutOfBandTokenResolverList
        {
            get
            {
                return this.recipientOutOfBandTokenResolverList;
            }
        }

        public SecurityTokenAuthenticator RecipientSymmetricTokenAuthenticator
        {
            get
            {
                return this.recipientSymmetricTokenAuthenticator;
            }
        }

        public System.ServiceModel.Security.Tokens.SecurityTokenParameters SecurityTokenParameters
        {
            get
            {
                return this.tokenParameters;
            }
            set
            {
                base.ThrowIfImmutable();
                this.tokenParameters = value;
            }
        }
    }
}

