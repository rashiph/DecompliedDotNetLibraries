namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security.Tokens;

    internal class AsymmetricSecurityProtocolFactory : MessageSecurityProtocolFactory
    {
        private bool allowSerializedSigningTokenOnReply;
        private SecurityTokenParameters asymmetricTokenParameters;
        private SecurityTokenParameters cryptoTokenParameters;
        private SecurityTokenProvider recipientAsymmetricTokenProvider;
        private SecurityTokenAuthenticator recipientCryptoTokenAuthenticator;
        private ReadOnlyCollection<SecurityTokenResolver> recipientOutOfBandTokenResolverList;

        public AsymmetricSecurityProtocolFactory()
        {
        }

        internal AsymmetricSecurityProtocolFactory(AsymmetricSecurityProtocolFactory factory) : base(factory)
        {
            this.allowSerializedSigningTokenOnReply = factory.allowSerializedSigningTokenOnReply;
        }

        public override EndpointIdentity GetIdentityOfSelf()
        {
            if ((base.SecurityTokenManager is IEndpointIdentityProvider) && (this.AsymmetricTokenParameters != null))
            {
                SecurityTokenRequirement requirement = base.CreateRecipientSecurityTokenRequirement();
                this.AsymmetricTokenParameters.InitializeSecurityTokenRequirement(requirement);
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
            if (this.recipientCryptoTokenAuthenticator is ISecurityContextSecurityTokenCacheProvider)
            {
                property.Add(((ISecurityContextSecurityTokenCacheProvider) this.recipientCryptoTokenAuthenticator).TokenCache);
            }
            return (T) property;
        }

        public override void OnAbort()
        {
            if (!base.ActAsInitiator)
            {
                if (this.recipientAsymmetricTokenProvider != null)
                {
                    System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(this.recipientAsymmetricTokenProvider);
                }
                if (this.recipientCryptoTokenAuthenticator != null)
                {
                    System.ServiceModel.Security.SecurityUtils.AbortTokenAuthenticatorIfRequired(this.recipientCryptoTokenAuthenticator);
                }
            }
            base.OnAbort();
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (!base.ActAsInitiator)
            {
                if (this.recipientAsymmetricTokenProvider != null)
                {
                    System.ServiceModel.Security.SecurityUtils.CloseTokenProviderIfRequired(this.recipientAsymmetricTokenProvider, helper.RemainingTime());
                }
                if (this.recipientCryptoTokenAuthenticator != null)
                {
                    System.ServiceModel.Security.SecurityUtils.CloseTokenAuthenticatorIfRequired(this.recipientCryptoTokenAuthenticator, helper.RemainingTime());
                }
            }
            base.OnClose(helper.RemainingTime());
        }

        protected override SecurityProtocol OnCreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, TimeSpan timeout)
        {
            return new AsymmetricSecurityProtocol(this, target, via);
        }

        public override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnOpen(helper.RemainingTime());
            if (base.ActAsInitiator)
            {
                if (base.ApplyIntegrity)
                {
                    if (this.CryptoTokenParameters == null)
                    {
                        base.OnPropertySettingsError("CryptoTokenParameters", true);
                    }
                    if (this.CryptoTokenParameters.RequireDerivedKeys)
                    {
                        base.ExpectKeyDerivation = true;
                    }
                }
            }
            else
            {
                if (this.CryptoTokenParameters == null)
                {
                    base.OnPropertySettingsError("CryptoTokenParameters", true);
                }
                if (this.CryptoTokenParameters.RequireDerivedKeys)
                {
                    base.ExpectKeyDerivation = true;
                }
                SecurityTokenResolver outOfBandTokenResolver = null;
                if (base.RequireIntegrity)
                {
                    RecipientServiceModelSecurityTokenRequirement requirement = base.CreateRecipientSecurityTokenRequirement();
                    this.CryptoTokenParameters.InitializeSecurityTokenRequirement(requirement);
                    requirement.KeyUsage = SecurityKeyUsage.Signature;
                    requirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Input;
                    this.recipientCryptoTokenAuthenticator = base.SecurityTokenManager.CreateSecurityTokenAuthenticator(requirement, out outOfBandTokenResolver);
                    base.Open("RecipientCryptoTokenAuthenticator", true, this.recipientCryptoTokenAuthenticator, helper.RemainingTime());
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
            }
            if (this.RequiresAsymmetricTokenProviderForForwardDirection || this.RequiresAsymmetricTokenProviderForReturnDirection)
            {
                if (this.AsymmetricTokenParameters == null)
                {
                    base.OnPropertySettingsError("AsymmetricTokenParameters", this.RequiresAsymmetricTokenProviderForForwardDirection);
                }
                else if (this.AsymmetricTokenParameters.RequireDerivedKeys)
                {
                    base.ExpectKeyDerivation = true;
                }
                if (!base.ActAsInitiator)
                {
                    RecipientServiceModelSecurityTokenRequirement requirement2 = base.CreateRecipientSecurityTokenRequirement();
                    this.AsymmetricTokenParameters.InitializeSecurityTokenRequirement(requirement2);
                    requirement2.KeyUsage = this.RequiresAsymmetricTokenProviderForForwardDirection ? SecurityKeyUsage.Exchange : SecurityKeyUsage.Signature;
                    requirement2.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = this.RequiresAsymmetricTokenProviderForForwardDirection ? MessageDirection.Input : MessageDirection.Output;
                    this.recipientAsymmetricTokenProvider = base.SecurityTokenManager.CreateSecurityTokenProvider(requirement2);
                    base.Open("RecipientAsymmetricTokenProvider", this.RequiresAsymmetricTokenProviderForForwardDirection, this.recipientAsymmetricTokenProvider, helper.RemainingTime());
                }
            }
            if ((base.ActAsInitiator && this.AllowSerializedSigningTokenOnReply) && (base.IdentityVerifier == null))
            {
                base.OnPropertySettingsError("IdentityVerifier", false);
            }
        }

        public bool AllowSerializedSigningTokenOnReply
        {
            get
            {
                return this.allowSerializedSigningTokenOnReply;
            }
            set
            {
                base.ThrowIfImmutable();
                this.allowSerializedSigningTokenOnReply = value;
            }
        }

        public SecurityTokenParameters AsymmetricTokenParameters
        {
            get
            {
                return this.asymmetricTokenParameters;
            }
            set
            {
                base.ThrowIfImmutable();
                this.asymmetricTokenParameters = value;
            }
        }

        public SecurityTokenParameters CryptoTokenParameters
        {
            get
            {
                return this.cryptoTokenParameters;
            }
            set
            {
                base.ThrowIfImmutable();
                this.cryptoTokenParameters = value;
            }
        }

        public SecurityTokenProvider RecipientAsymmetricTokenProvider
        {
            get
            {
                base.CommunicationObject.ThrowIfNotOpened();
                return this.recipientAsymmetricTokenProvider;
            }
        }

        public SecurityTokenAuthenticator RecipientCryptoTokenAuthenticator
        {
            get
            {
                base.CommunicationObject.ThrowIfNotOpened();
                return this.recipientCryptoTokenAuthenticator;
            }
        }

        public ReadOnlyCollection<SecurityTokenResolver> RecipientOutOfBandTokenResolverList
        {
            get
            {
                base.CommunicationObject.ThrowIfNotOpened();
                return this.recipientOutOfBandTokenResolverList;
            }
        }

        private bool RequiresAsymmetricTokenProviderForForwardDirection
        {
            get
            {
                return ((base.ActAsInitiator && base.ApplyConfidentiality) || (!base.ActAsInitiator && base.RequireConfidentiality));
            }
        }

        private bool RequiresAsymmetricTokenProviderForReturnDirection
        {
            get
            {
                return ((base.ActAsInitiator && base.RequireIntegrity) || (!base.ActAsInitiator && base.ApplyIntegrity));
            }
        }
    }
}

