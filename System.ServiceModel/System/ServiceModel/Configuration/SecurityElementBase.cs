namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Configuration;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    public class SecurityElementBase : BindingElementExtensionElement
    {
        internal const System.ServiceModel.Configuration.AuthenticationMode defaultAuthenticationMode = System.ServiceModel.Configuration.AuthenticationMode.SspiNegotiated;
        private SecurityBindingElement failedSecurityBindingElement;
        private ConfigurationPropertyCollection properties;
        private SecurityKeyType templateKeyType;
        private bool willX509IssuerReferenceAssertionBeWritten;

        internal SecurityElementBase()
        {
        }

        protected void AddBindingTemplate(Dictionary<System.ServiceModel.Configuration.AuthenticationMode, SecurityBindingElement> bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode mode)
        {
            this.AuthenticationMode = mode;
            try
            {
                bindingTemplates[mode] = (SecurityBindingElement) this.CreateBindingElement(true);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
            }
        }

        protected virtual void AddBindingTemplates(Dictionary<System.ServiceModel.Configuration.AuthenticationMode, SecurityBindingElement> bindingTemplates)
        {
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.AnonymousForCertificate);
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.AnonymousForSslNegotiated);
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.CertificateOverTransport);
            if (this.templateKeyType == SecurityKeyType.SymmetricKey)
            {
                this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.IssuedToken);
            }
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.IssuedTokenForCertificate);
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.IssuedTokenForSslNegotiated);
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.IssuedTokenOverTransport);
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.Kerberos);
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.KerberosOverTransport);
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.MutualCertificate);
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.MutualCertificateDuplex);
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.MutualSslNegotiated);
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.SspiNegotiated);
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.UserNameForCertificate);
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.UserNameForSslNegotiated);
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.UserNameOverTransport);
            this.AddBindingTemplate(bindingTemplates, System.ServiceModel.Configuration.AuthenticationMode.SspiNegotiatedOverTransport);
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            SecurityBindingElement element = (SecurityBindingElement) bindingElement;
            if (base.ElementInformation.Properties["defaultAlgorithmSuite"].ValueOrigin != PropertyValueOrigin.Default)
            {
                element.DefaultAlgorithmSuite = this.DefaultAlgorithmSuite;
            }
            if (base.ElementInformation.Properties["includeTimestamp"].ValueOrigin != PropertyValueOrigin.Default)
            {
                element.IncludeTimestamp = this.IncludeTimestamp;
            }
            if (base.ElementInformation.Properties["messageSecurityVersion"].ValueOrigin != PropertyValueOrigin.Default)
            {
                element.MessageSecurityVersion = this.MessageSecurityVersion;
            }
            if (base.ElementInformation.Properties["keyEntropyMode"].ValueOrigin != PropertyValueOrigin.Default)
            {
                element.KeyEntropyMode = this.KeyEntropyMode;
            }
            if (base.ElementInformation.Properties["securityHeaderLayout"].ValueOrigin != PropertyValueOrigin.Default)
            {
                element.SecurityHeaderLayout = this.SecurityHeaderLayout;
            }
            if (base.ElementInformation.Properties["requireDerivedKeys"].ValueOrigin != PropertyValueOrigin.Default)
            {
                element.SetKeyDerivation(this.RequireDerivedKeys);
            }
            if (base.ElementInformation.Properties["allowInsecureTransport"].ValueOrigin != PropertyValueOrigin.Default)
            {
                element.AllowInsecureTransport = this.AllowInsecureTransport;
            }
            if (base.ElementInformation.Properties["enableUnsecuredResponse"].ValueOrigin != PropertyValueOrigin.Default)
            {
                element.EnableUnsecuredResponse = this.EnableUnsecuredResponse;
            }
            SymmetricSecurityBindingElement element2 = element as SymmetricSecurityBindingElement;
            if (element2 != null)
            {
                if (base.ElementInformation.Properties["messageProtectionOrder"].ValueOrigin != PropertyValueOrigin.Default)
                {
                    element2.MessageProtectionOrder = this.MessageProtectionOrder;
                }
                if (base.ElementInformation.Properties["requireSignatureConfirmation"].ValueOrigin != PropertyValueOrigin.Default)
                {
                    element2.RequireSignatureConfirmation = this.RequireSignatureConfirmation;
                }
                SecureConversationSecurityTokenParameters protectionTokenParameters = element2.ProtectionTokenParameters as SecureConversationSecurityTokenParameters;
                if (protectionTokenParameters != null)
                {
                    protectionTokenParameters.CanRenewSession = this.CanRenewSecurityContextToken;
                }
            }
            AsymmetricSecurityBindingElement element3 = element as AsymmetricSecurityBindingElement;
            if (element3 != null)
            {
                if (base.ElementInformation.Properties["messageProtectionOrder"].ValueOrigin != PropertyValueOrigin.Default)
                {
                    element3.MessageProtectionOrder = this.MessageProtectionOrder;
                }
                if (base.ElementInformation.Properties["requireSignatureConfirmation"].ValueOrigin != PropertyValueOrigin.Default)
                {
                    element3.RequireSignatureConfirmation = this.RequireSignatureConfirmation;
                }
                if (base.ElementInformation.Properties["allowSerializedSigningTokenOnReply"].ValueOrigin != PropertyValueOrigin.Default)
                {
                    element3.AllowSerializedSigningTokenOnReply = this.AllowSerializedSigningTokenOnReply;
                }
            }
            TransportSecurityBindingElement element4 = element as TransportSecurityBindingElement;
            if ((element4 != null) && (element4.EndpointSupportingTokenParameters.Endorsing.Count == 1))
            {
                SecureConversationSecurityTokenParameters parameters2 = element4.EndpointSupportingTokenParameters.Endorsing[0] as SecureConversationSecurityTokenParameters;
                if (parameters2 != null)
                {
                    parameters2.CanRenewSession = this.CanRenewSecurityContextToken;
                }
            }
            if (base.ElementInformation.Properties["localClientSettings"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.LocalClientSettings.ApplyConfiguration(element.LocalClientSettings);
            }
            if (base.ElementInformation.Properties["localServiceSettings"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.LocalServiceSettings.ApplyConfiguration(element.LocalServiceSettings);
            }
        }

        internal static bool AreBindingsMatching(SecurityBindingElement b1, SecurityBindingElement b2)
        {
            return AreBindingsMatching(b1, b2, true);
        }

        internal static bool AreBindingsMatching(SecurityBindingElement b1, SecurityBindingElement b2, bool exactMessageSecurityVersion)
        {
            if ((b1 == null) || (b2 == null))
            {
                return (b1 == b2);
            }
            if (b1.GetType() != b2.GetType())
            {
                return false;
            }
            if (b1.MessageSecurityVersion != b2.MessageSecurityVersion)
            {
                if (exactMessageSecurityVersion)
                {
                    return false;
                }
                if (((b1.MessageSecurityVersion.SecurityVersion != b2.MessageSecurityVersion.SecurityVersion) || (b1.MessageSecurityVersion.TrustVersion != b2.MessageSecurityVersion.TrustVersion)) || ((b1.MessageSecurityVersion.SecureConversationVersion != b2.MessageSecurityVersion.SecureConversationVersion) || (b1.MessageSecurityVersion.SecurityPolicyVersion != b2.MessageSecurityVersion.SecurityPolicyVersion)))
                {
                    return false;
                }
            }
            if (b1.SecurityHeaderLayout != b2.SecurityHeaderLayout)
            {
                return false;
            }
            if (b1.DefaultAlgorithmSuite != b2.DefaultAlgorithmSuite)
            {
                return false;
            }
            if (b1.IncludeTimestamp != b2.IncludeTimestamp)
            {
                return false;
            }
            if (b1.SecurityHeaderLayout != b2.SecurityHeaderLayout)
            {
                return false;
            }
            if (b1.KeyEntropyMode != b2.KeyEntropyMode)
            {
                return false;
            }
            if (!AreTokenParameterCollectionsMatching(b1.EndpointSupportingTokenParameters.Endorsing, b2.EndpointSupportingTokenParameters.Endorsing, exactMessageSecurityVersion))
            {
                return false;
            }
            if (!AreTokenParameterCollectionsMatching(b1.EndpointSupportingTokenParameters.SignedEncrypted, b2.EndpointSupportingTokenParameters.SignedEncrypted, exactMessageSecurityVersion))
            {
                return false;
            }
            if (!AreTokenParameterCollectionsMatching(b1.EndpointSupportingTokenParameters.Signed, b2.EndpointSupportingTokenParameters.Signed, exactMessageSecurityVersion))
            {
                return false;
            }
            if (!AreTokenParameterCollectionsMatching(b1.EndpointSupportingTokenParameters.SignedEndorsing, b2.EndpointSupportingTokenParameters.SignedEndorsing, exactMessageSecurityVersion))
            {
                return false;
            }
            if (b1.OperationSupportingTokenParameters.Count != b2.OperationSupportingTokenParameters.Count)
            {
                return false;
            }
            foreach (KeyValuePair<string, SupportingTokenParameters> pair in b1.OperationSupportingTokenParameters)
            {
                if (!b2.OperationSupportingTokenParameters.ContainsKey(pair.Key))
                {
                    return false;
                }
                SupportingTokenParameters parameters = b2.OperationSupportingTokenParameters[pair.Key];
                if (!AreTokenParameterCollectionsMatching(pair.Value.Endorsing, parameters.Endorsing, exactMessageSecurityVersion))
                {
                    return false;
                }
                if (!AreTokenParameterCollectionsMatching(pair.Value.SignedEncrypted, parameters.SignedEncrypted, exactMessageSecurityVersion))
                {
                    return false;
                }
                if (!AreTokenParameterCollectionsMatching(pair.Value.Signed, parameters.Signed, exactMessageSecurityVersion))
                {
                    return false;
                }
                if (!AreTokenParameterCollectionsMatching(pair.Value.SignedEndorsing, parameters.SignedEndorsing, exactMessageSecurityVersion))
                {
                    return false;
                }
            }
            SymmetricSecurityBindingElement element = b1 as SymmetricSecurityBindingElement;
            if (element != null)
            {
                SymmetricSecurityBindingElement element2 = (SymmetricSecurityBindingElement) b2;
                if (element.MessageProtectionOrder != element2.MessageProtectionOrder)
                {
                    return false;
                }
                if (!AreTokenParametersMatching(element.ProtectionTokenParameters, element2.ProtectionTokenParameters, false, exactMessageSecurityVersion))
                {
                    return false;
                }
            }
            AsymmetricSecurityBindingElement element3 = b1 as AsymmetricSecurityBindingElement;
            if (element3 != null)
            {
                AsymmetricSecurityBindingElement element4 = (AsymmetricSecurityBindingElement) b2;
                if (element3.MessageProtectionOrder != element4.MessageProtectionOrder)
                {
                    return false;
                }
                if (element3.RequireSignatureConfirmation != element4.RequireSignatureConfirmation)
                {
                    return false;
                }
                if (!AreTokenParametersMatching(element3.InitiatorTokenParameters, element4.InitiatorTokenParameters, true, exactMessageSecurityVersion) || !AreTokenParametersMatching(element3.RecipientTokenParameters, element4.RecipientTokenParameters, true, exactMessageSecurityVersion))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool AreTokenParameterCollectionsMatching(Collection<SecurityTokenParameters> c1, Collection<SecurityTokenParameters> c2, bool exactMessageSecurityVersion)
        {
            if (c1.Count != c2.Count)
            {
                return false;
            }
            for (int i = 0; i < c1.Count; i++)
            {
                if (!AreTokenParametersMatching(c1[i], c2[i], true, exactMessageSecurityVersion))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool AreTokenParametersMatching(SecurityTokenParameters p1, SecurityTokenParameters p2, bool skipRequireDerivedKeysComparison, bool exactMessageSecurityVersion)
        {
            if ((p1 == null) || (p2 == null))
            {
                return false;
            }
            if (p1.GetType() != p2.GetType())
            {
                return false;
            }
            if (p1.InclusionMode != p2.InclusionMode)
            {
                return false;
            }
            if (!skipRequireDerivedKeysComparison && (p1.RequireDerivedKeys != p2.RequireDerivedKeys))
            {
                return false;
            }
            if (p1.ReferenceStyle != p2.ReferenceStyle)
            {
                return false;
            }
            if (p1 is SslSecurityTokenParameters)
            {
                if (((SslSecurityTokenParameters) p1).RequireClientCertificate != ((SslSecurityTokenParameters) p2).RequireClientCertificate)
                {
                    return false;
                }
            }
            else if (p1 is SecureConversationSecurityTokenParameters)
            {
                SecureConversationSecurityTokenParameters parameters = (SecureConversationSecurityTokenParameters) p1;
                SecureConversationSecurityTokenParameters parameters2 = (SecureConversationSecurityTokenParameters) p2;
                if (parameters.RequireCancellation != parameters2.RequireCancellation)
                {
                    return false;
                }
                if (parameters.CanRenewSession != parameters2.CanRenewSession)
                {
                    return false;
                }
                if (!AreBindingsMatching(parameters.BootstrapSecurityBindingElement, parameters2.BootstrapSecurityBindingElement, exactMessageSecurityVersion))
                {
                    return false;
                }
            }
            else if ((p1 is IssuedSecurityTokenParameters) && (((IssuedSecurityTokenParameters) p1).KeyType != ((IssuedSecurityTokenParameters) p2).KeyType))
            {
                return false;
            }
            return true;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            SecurityElementBase base2 = (SecurityElementBase) from;
            if (base2.ElementInformation.Properties["allowSerializedSigningTokenOnReply"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.AllowSerializedSigningTokenOnReply = base2.AllowSerializedSigningTokenOnReply;
            }
            if (base2.ElementInformation.Properties["defaultAlgorithmSuite"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.DefaultAlgorithmSuite = base2.DefaultAlgorithmSuite;
            }
            if (base2.ElementInformation.Properties["requireDerivedKeys"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.RequireDerivedKeys = base2.RequireDerivedKeys;
            }
            if (base2.ElementInformation.Properties["includeTimestamp"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.IncludeTimestamp = base2.IncludeTimestamp;
            }
            if (base2.ElementInformation.Properties["issuedTokenParameters"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.IssuedTokenParameters.Copy(base2.IssuedTokenParameters);
            }
            if (base2.ElementInformation.Properties["messageProtectionOrder"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.MessageProtectionOrder = base2.MessageProtectionOrder;
            }
            if (base2.ElementInformation.Properties["messageSecurityVersion"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.MessageSecurityVersion = base2.MessageSecurityVersion;
            }
            if (base2.ElementInformation.Properties["requireSignatureConfirmation"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.RequireSignatureConfirmation = base2.RequireSignatureConfirmation;
            }
            if (base2.ElementInformation.Properties["requireSecurityContextCancellation"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.RequireSecurityContextCancellation = base2.RequireSecurityContextCancellation;
            }
            if (base2.ElementInformation.Properties["canRenewSecurityContextToken"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.CanRenewSecurityContextToken = base2.CanRenewSecurityContextToken;
            }
            if (base2.ElementInformation.Properties["keyEntropyMode"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.KeyEntropyMode = base2.KeyEntropyMode;
            }
            if (base2.ElementInformation.Properties["securityHeaderLayout"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.SecurityHeaderLayout = base2.SecurityHeaderLayout;
            }
            if (base2.ElementInformation.Properties["localClientSettings"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.LocalClientSettings.CopyFrom(base2.LocalClientSettings);
            }
            if (base2.ElementInformation.Properties["localServiceSettings"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.LocalServiceSettings.CopyFrom(base2.LocalServiceSettings);
            }
            this.failedSecurityBindingElement = base2.failedSecurityBindingElement;
            this.willX509IssuerReferenceAssertionBeWritten = base2.willX509IssuerReferenceAssertionBeWritten;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            return this.CreateBindingElement(false);
        }

        protected internal virtual BindingElement CreateBindingElement(bool createTemplateOnly)
        {
            SecurityBindingElement element;
            switch (this.AuthenticationMode)
            {
                case System.ServiceModel.Configuration.AuthenticationMode.AnonymousForCertificate:
                    element = SecurityBindingElement.CreateAnonymousForCertificateBindingElement();
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.AnonymousForSslNegotiated:
                    element = SecurityBindingElement.CreateSslNegotiationBindingElement(false, this.RequireSecurityContextCancellation);
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.CertificateOverTransport:
                    element = SecurityBindingElement.CreateCertificateOverTransportBindingElement(this.MessageSecurityVersion);
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.IssuedToken:
                    element = SecurityBindingElement.CreateIssuedTokenBindingElement(this.IssuedTokenParameters.Create(createTemplateOnly, this.templateKeyType));
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.IssuedTokenForCertificate:
                    element = SecurityBindingElement.CreateIssuedTokenForCertificateBindingElement(this.IssuedTokenParameters.Create(createTemplateOnly, this.templateKeyType));
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.IssuedTokenForSslNegotiated:
                    element = SecurityBindingElement.CreateIssuedTokenForSslBindingElement(this.IssuedTokenParameters.Create(createTemplateOnly, this.templateKeyType), this.RequireSecurityContextCancellation);
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.IssuedTokenOverTransport:
                    element = SecurityBindingElement.CreateIssuedTokenOverTransportBindingElement(this.IssuedTokenParameters.Create(createTemplateOnly, this.templateKeyType));
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.Kerberos:
                    element = SecurityBindingElement.CreateKerberosBindingElement();
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.KerberosOverTransport:
                    element = SecurityBindingElement.CreateKerberosOverTransportBindingElement();
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.MutualCertificate:
                    element = SecurityBindingElement.CreateMutualCertificateBindingElement(this.MessageSecurityVersion);
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.MutualCertificateDuplex:
                    element = SecurityBindingElement.CreateMutualCertificateDuplexBindingElement(this.MessageSecurityVersion);
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.MutualSslNegotiated:
                    element = SecurityBindingElement.CreateSslNegotiationBindingElement(true, this.RequireSecurityContextCancellation);
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.SspiNegotiated:
                    element = SecurityBindingElement.CreateSspiNegotiationBindingElement(this.RequireSecurityContextCancellation);
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.UserNameForCertificate:
                    element = SecurityBindingElement.CreateUserNameForCertificateBindingElement();
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.UserNameForSslNegotiated:
                    element = SecurityBindingElement.CreateUserNameForSslBindingElement(this.RequireSecurityContextCancellation);
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.UserNameOverTransport:
                    element = SecurityBindingElement.CreateUserNameOverTransportBindingElement();
                    break;

                case System.ServiceModel.Configuration.AuthenticationMode.SspiNegotiatedOverTransport:
                    element = SecurityBindingElement.CreateSspiNegotiationOverTransportBindingElement(this.RequireSecurityContextCancellation);
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("AuthenticationMode", (int) this.AuthenticationMode, typeof(System.ServiceModel.Configuration.AuthenticationMode)));
            }
            this.ApplyConfiguration(element);
            return element;
        }

        private bool DoesSecurityBindingElementContainClauseTypeofIssuerSerial(SecurityBindingElement sbe)
        {
            if (sbe == null)
            {
                return false;
            }
            if (sbe is SymmetricSecurityBindingElement)
            {
                X509SecurityTokenParameters protectionTokenParameters = ((SymmetricSecurityBindingElement) sbe).ProtectionTokenParameters as X509SecurityTokenParameters;
                if ((protectionTokenParameters != null) && (protectionTokenParameters.X509ReferenceStyle == X509KeyIdentifierClauseType.IssuerSerial))
                {
                    return true;
                }
            }
            else if (sbe is AsymmetricSecurityBindingElement)
            {
                X509SecurityTokenParameters initiatorTokenParameters = ((AsymmetricSecurityBindingElement) sbe).InitiatorTokenParameters as X509SecurityTokenParameters;
                if ((initiatorTokenParameters != null) && (initiatorTokenParameters.X509ReferenceStyle == X509KeyIdentifierClauseType.IssuerSerial))
                {
                    return true;
                }
                X509SecurityTokenParameters recipientTokenParameters = ((AsymmetricSecurityBindingElement) sbe).RecipientTokenParameters as X509SecurityTokenParameters;
                if ((recipientTokenParameters != null) && (recipientTokenParameters.X509ReferenceStyle == X509KeyIdentifierClauseType.IssuerSerial))
                {
                    return true;
                }
            }
            return (this.DoesX509TokenParametersContainClauseTypeofIssuerSerial(sbe.EndpointSupportingTokenParameters.Endorsing) || (this.DoesX509TokenParametersContainClauseTypeofIssuerSerial(sbe.EndpointSupportingTokenParameters.Signed) || (this.DoesX509TokenParametersContainClauseTypeofIssuerSerial(sbe.EndpointSupportingTokenParameters.SignedEncrypted) || (this.DoesX509TokenParametersContainClauseTypeofIssuerSerial(sbe.EndpointSupportingTokenParameters.SignedEndorsing) || (this.DoesX509TokenParametersContainClauseTypeofIssuerSerial(sbe.OptionalEndpointSupportingTokenParameters.Endorsing) || (this.DoesX509TokenParametersContainClauseTypeofIssuerSerial(sbe.OptionalEndpointSupportingTokenParameters.Signed) || (this.DoesX509TokenParametersContainClauseTypeofIssuerSerial(sbe.OptionalEndpointSupportingTokenParameters.SignedEncrypted) || this.DoesX509TokenParametersContainClauseTypeofIssuerSerial(sbe.OptionalEndpointSupportingTokenParameters.SignedEndorsing))))))));
        }

        private bool DoesX509TokenParametersContainClauseTypeofIssuerSerial(Collection<SecurityTokenParameters> tokenParameters)
        {
            foreach (SecurityTokenParameters parameters in tokenParameters)
            {
                X509SecurityTokenParameters parameters2 = parameters as X509SecurityTokenParameters;
                if ((parameters2 != null) && (parameters2.X509ReferenceStyle == X509KeyIdentifierClauseType.IssuerSerial))
                {
                    return true;
                }
            }
            return false;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            this.InitializeFrom(bindingElement, true);
        }

        internal void InitializeFrom(BindingElement bindingElement, bool initializeNestedBindings)
        {
            if (bindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElement");
            }
            SecurityBindingElement sbe = (SecurityBindingElement) bindingElement;
            this.DefaultAlgorithmSuite = sbe.DefaultAlgorithmSuite;
            this.IncludeTimestamp = sbe.IncludeTimestamp;
            this.MessageSecurityVersion = sbe.MessageSecurityVersion;
            this.KeyEntropyMode = sbe.KeyEntropyMode;
            this.SecurityHeaderLayout = sbe.SecurityHeaderLayout;
            bool? nullable = null;
            if (sbe.EndpointSupportingTokenParameters.Endorsing.Count == 1)
            {
                this.InitializeNestedTokenParameterSettings(sbe.EndpointSupportingTokenParameters.Endorsing[0], initializeNestedBindings);
            }
            else if (sbe.EndpointSupportingTokenParameters.SignedEncrypted.Count == 1)
            {
                this.InitializeNestedTokenParameterSettings(sbe.EndpointSupportingTokenParameters.SignedEncrypted[0], initializeNestedBindings);
            }
            else if (sbe.EndpointSupportingTokenParameters.Signed.Count == 1)
            {
                this.InitializeNestedTokenParameterSettings(sbe.EndpointSupportingTokenParameters.Signed[0], initializeNestedBindings);
            }
            bool flag = false;
            foreach (SecurityTokenParameters parameters in sbe.EndpointSupportingTokenParameters.Endorsing)
            {
                if (!parameters.HasAsymmetricKey)
                {
                    if (nullable.HasValue && (nullable.Value != parameters.RequireDerivedKeys))
                    {
                        flag = true;
                    }
                    else
                    {
                        nullable = new bool?(parameters.RequireDerivedKeys);
                    }
                }
            }
            SymmetricSecurityBindingElement element2 = sbe as SymmetricSecurityBindingElement;
            if (element2 != null)
            {
                this.MessageProtectionOrder = element2.MessageProtectionOrder;
                this.RequireSignatureConfirmation = element2.RequireSignatureConfirmation;
                if (element2.ProtectionTokenParameters != null)
                {
                    this.InitializeNestedTokenParameterSettings(element2.ProtectionTokenParameters, initializeNestedBindings);
                    if (nullable.HasValue && (nullable.Value != element2.ProtectionTokenParameters.RequireDerivedKeys))
                    {
                        flag = true;
                    }
                    else
                    {
                        nullable = new bool?(element2.ProtectionTokenParameters.RequireDerivedKeys);
                    }
                }
            }
            else
            {
                AsymmetricSecurityBindingElement element3 = sbe as AsymmetricSecurityBindingElement;
                if (element3 != null)
                {
                    this.MessageProtectionOrder = element3.MessageProtectionOrder;
                    this.RequireSignatureConfirmation = element3.RequireSignatureConfirmation;
                    if (element3.InitiatorTokenParameters != null)
                    {
                        this.InitializeNestedTokenParameterSettings(element3.InitiatorTokenParameters, initializeNestedBindings);
                        if (nullable.HasValue && (nullable.Value != element3.InitiatorTokenParameters.RequireDerivedKeys))
                        {
                            flag = true;
                        }
                        else
                        {
                            nullable = new bool?(element3.InitiatorTokenParameters.RequireDerivedKeys);
                        }
                    }
                }
            }
            this.willX509IssuerReferenceAssertionBeWritten = this.DoesSecurityBindingElementContainClauseTypeofIssuerSerial(sbe);
            this.RequireDerivedKeys = nullable.GetValueOrDefault(true);
            this.LocalClientSettings.InitializeFrom(sbe.LocalClientSettings);
            this.LocalServiceSettings.InitializeFrom(sbe.LocalServiceSettings);
            if (!flag)
            {
                flag = !this.TryInitializeAuthenticationMode(sbe);
            }
            if (flag)
            {
                this.failedSecurityBindingElement = sbe;
            }
        }

        protected virtual void InitializeNestedTokenParameterSettings(SecurityTokenParameters sp, bool initializeNestedBindings)
        {
            if (sp is SspiSecurityTokenParameters)
            {
                this.RequireSecurityContextCancellation = ((SspiSecurityTokenParameters) sp).RequireCancellation;
            }
            else if (sp is SslSecurityTokenParameters)
            {
                this.RequireSecurityContextCancellation = ((SslSecurityTokenParameters) sp).RequireCancellation;
            }
            else if (sp is IssuedSecurityTokenParameters)
            {
                this.IssuedTokenParameters.InitializeFrom((IssuedSecurityTokenParameters) sp, initializeNestedBindings);
            }
        }

        protected override bool SerializeToXmlElement(XmlWriter writer, string elementName)
        {
            if ((this.failedSecurityBindingElement != null) && (writer != null))
            {
                writer.WriteComment(System.ServiceModel.SR.GetString("ConfigurationSchemaInsuffientForSecurityBindingElementInstance"));
                writer.WriteComment(this.failedSecurityBindingElement.ToString());
                return true;
            }
            if ((writer != null) && this.willX509IssuerReferenceAssertionBeWritten)
            {
                writer.WriteComment(System.ServiceModel.SR.GetString("ConfigurationSchemaContainsX509IssuerSerialReference"));
            }
            return base.SerializeToXmlElement(writer, elementName);
        }

        private void SetIssuedTokenKeyType(SecurityBindingElement sbe)
        {
            if ((sbe.EndpointSupportingTokenParameters.Endorsing.Count > 0) && (sbe.EndpointSupportingTokenParameters.Endorsing[0] is IssuedSecurityTokenParameters))
            {
                this.templateKeyType = ((IssuedSecurityTokenParameters) sbe.EndpointSupportingTokenParameters.Endorsing[0]).KeyType;
            }
            else if ((sbe.EndpointSupportingTokenParameters.Signed.Count > 0) && (sbe.EndpointSupportingTokenParameters.Signed[0] is IssuedSecurityTokenParameters))
            {
                this.templateKeyType = ((IssuedSecurityTokenParameters) sbe.EndpointSupportingTokenParameters.Signed[0]).KeyType;
            }
            else if ((sbe.EndpointSupportingTokenParameters.SignedEncrypted.Count > 0) && (sbe.EndpointSupportingTokenParameters.SignedEncrypted[0] is IssuedSecurityTokenParameters))
            {
                this.templateKeyType = ((IssuedSecurityTokenParameters) sbe.EndpointSupportingTokenParameters.SignedEncrypted[0]).KeyType;
            }
            else
            {
                this.templateKeyType = SecurityKeyType.SymmetricKey;
            }
        }

        private bool TryInitializeAuthenticationMode(SecurityBindingElement sbe)
        {
            if (sbe.OperationSupportingTokenParameters.Count > 0)
            {
                return false;
            }
            this.SetIssuedTokenKeyType(sbe);
            Dictionary<System.ServiceModel.Configuration.AuthenticationMode, SecurityBindingElement> bindingTemplates = new Dictionary<System.ServiceModel.Configuration.AuthenticationMode, SecurityBindingElement>();
            this.AddBindingTemplates(bindingTemplates);
            foreach (System.ServiceModel.Configuration.AuthenticationMode mode in bindingTemplates.Keys)
            {
                SecurityBindingElement element = bindingTemplates[mode];
                if (AreBindingsMatching(sbe, element))
                {
                    this.AuthenticationMode = mode;
                    return true;
                }
            }
            return false;
        }

        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            if (sourceElement is SecurityElementBase)
            {
                this.failedSecurityBindingElement = ((SecurityElementBase) sourceElement).failedSecurityBindingElement;
                this.willX509IssuerReferenceAssertionBeWritten = ((SecurityElementBase) sourceElement).willX509IssuerReferenceAssertionBeWritten;
            }
            base.Unmerge(sourceElement, parentElement, saveMode);
        }

        [ConfigurationProperty("allowInsecureTransport", DefaultValue=false)]
        public bool AllowInsecureTransport
        {
            get
            {
                return (bool) base["allowInsecureTransport"];
            }
            set
            {
                base["allowInsecureTransport"] = value;
            }
        }

        [ConfigurationProperty("allowSerializedSigningTokenOnReply", DefaultValue=false)]
        public bool AllowSerializedSigningTokenOnReply
        {
            get
            {
                return (bool) base["allowSerializedSigningTokenOnReply"];
            }
            set
            {
                base["allowSerializedSigningTokenOnReply"] = value;
            }
        }

        [ServiceModelEnumValidator(typeof(AuthenticationModeHelper)), ConfigurationProperty("authenticationMode", DefaultValue=13)]
        public System.ServiceModel.Configuration.AuthenticationMode AuthenticationMode
        {
            get
            {
                return (System.ServiceModel.Configuration.AuthenticationMode) base["authenticationMode"];
            }
            set
            {
                base["authenticationMode"] = value;
            }
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(SecurityBindingElement);
            }
        }

        [ConfigurationProperty("canRenewSecurityContextToken", DefaultValue=true)]
        public bool CanRenewSecurityContextToken
        {
            get
            {
                return (bool) base["canRenewSecurityContextToken"];
            }
            set
            {
                base["canRenewSecurityContextToken"] = value;
            }
        }

        [TypeConverter(typeof(SecurityAlgorithmSuiteConverter)), ConfigurationProperty("defaultAlgorithmSuite", DefaultValue="Default")]
        public SecurityAlgorithmSuite DefaultAlgorithmSuite
        {
            get
            {
                return (SecurityAlgorithmSuite) base["defaultAlgorithmSuite"];
            }
            set
            {
                base["defaultAlgorithmSuite"] = value;
            }
        }

        [ConfigurationProperty("enableUnsecuredResponse", DefaultValue=false)]
        public bool EnableUnsecuredResponse
        {
            get
            {
                return (bool) base["enableUnsecuredResponse"];
            }
            set
            {
                base["enableUnsecuredResponse"] = value;
            }
        }

        internal bool HasImportFailed
        {
            get
            {
                return (this.failedSecurityBindingElement != null);
            }
        }

        [ConfigurationProperty("includeTimestamp", DefaultValue=true)]
        public bool IncludeTimestamp
        {
            get
            {
                return (bool) base["includeTimestamp"];
            }
            set
            {
                base["includeTimestamp"] = value;
            }
        }

        [ConfigurationProperty("issuedTokenParameters")]
        public IssuedTokenParametersElement IssuedTokenParameters
        {
            get
            {
                return (IssuedTokenParametersElement) base["issuedTokenParameters"];
            }
        }

        [ConfigurationProperty("keyEntropyMode", DefaultValue=2), ServiceModelEnumValidator(typeof(SecurityKeyEntropyModeHelper))]
        public SecurityKeyEntropyMode KeyEntropyMode
        {
            get
            {
                return (SecurityKeyEntropyMode) base["keyEntropyMode"];
            }
            set
            {
                base["keyEntropyMode"] = value;
            }
        }

        [ConfigurationProperty("localClientSettings")]
        public LocalClientSecuritySettingsElement LocalClientSettings
        {
            get
            {
                return (LocalClientSecuritySettingsElement) base["localClientSettings"];
            }
        }

        [ConfigurationProperty("localServiceSettings")]
        public LocalServiceSecuritySettingsElement LocalServiceSettings
        {
            get
            {
                return (LocalServiceSecuritySettingsElement) base["localServiceSettings"];
            }
        }

        [ConfigurationProperty("messageProtectionOrder", DefaultValue=1), ServiceModelEnumValidator(typeof(MessageProtectionOrderHelper))]
        public System.ServiceModel.Security.MessageProtectionOrder MessageProtectionOrder
        {
            get
            {
                return (System.ServiceModel.Security.MessageProtectionOrder) base["messageProtectionOrder"];
            }
            set
            {
                base["messageProtectionOrder"] = value;
            }
        }

        [ConfigurationProperty("messageSecurityVersion", DefaultValue="Default"), TypeConverter(typeof(MessageSecurityVersionConverter))]
        public System.ServiceModel.MessageSecurityVersion MessageSecurityVersion
        {
            get
            {
                return (System.ServiceModel.MessageSecurityVersion) base["messageSecurityVersion"];
            }
            set
            {
                base["messageSecurityVersion"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("defaultAlgorithmSuite", typeof(SecurityAlgorithmSuite), "Default", new SecurityAlgorithmSuiteConverter(), null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("allowSerializedSigningTokenOnReply", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("enableUnsecuredResponse", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("authenticationMode", typeof(System.ServiceModel.Configuration.AuthenticationMode), System.ServiceModel.Configuration.AuthenticationMode.SspiNegotiated, null, new ServiceModelEnumValidator(typeof(AuthenticationModeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("requireDerivedKeys", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("securityHeaderLayout", typeof(System.ServiceModel.Channels.SecurityHeaderLayout), System.ServiceModel.Channels.SecurityHeaderLayout.Strict, null, new ServiceModelEnumValidator(typeof(SecurityHeaderLayoutHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("includeTimestamp", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("allowInsecureTransport", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("keyEntropyMode", typeof(SecurityKeyEntropyMode), SecurityKeyEntropyMode.CombinedEntropy, null, new ServiceModelEnumValidator(typeof(SecurityKeyEntropyModeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("issuedTokenParameters", typeof(IssuedTokenParametersElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("localClientSettings", typeof(LocalClientSecuritySettingsElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("localServiceSettings", typeof(LocalServiceSecuritySettingsElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("messageProtectionOrder", typeof(System.ServiceModel.Security.MessageProtectionOrder), System.ServiceModel.Security.MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature, null, new ServiceModelEnumValidator(typeof(MessageProtectionOrderHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("messageSecurityVersion", typeof(System.ServiceModel.MessageSecurityVersion), "Default", new MessageSecurityVersionConverter(), null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("requireSecurityContextCancellation", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("requireSignatureConfirmation", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("canRenewSecurityContextToken", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("requireDerivedKeys", DefaultValue=true)]
        public bool RequireDerivedKeys
        {
            get
            {
                return (bool) base["requireDerivedKeys"];
            }
            set
            {
                base["requireDerivedKeys"] = value;
            }
        }

        [ConfigurationProperty("requireSecurityContextCancellation", DefaultValue=true)]
        public bool RequireSecurityContextCancellation
        {
            get
            {
                return (bool) base["requireSecurityContextCancellation"];
            }
            set
            {
                base["requireSecurityContextCancellation"] = value;
            }
        }

        [ConfigurationProperty("requireSignatureConfirmation", DefaultValue=false)]
        public bool RequireSignatureConfirmation
        {
            get
            {
                return (bool) base["requireSignatureConfirmation"];
            }
            set
            {
                base["requireSignatureConfirmation"] = value;
            }
        }

        [ServiceModelEnumValidator(typeof(SecurityHeaderLayoutHelper)), ConfigurationProperty("securityHeaderLayout", DefaultValue=0)]
        public System.ServiceModel.Channels.SecurityHeaderLayout SecurityHeaderLayout
        {
            get
            {
                return (System.ServiceModel.Channels.SecurityHeaderLayout) base["securityHeaderLayout"];
            }
            set
            {
                base["securityHeaderLayout"] = value;
            }
        }
    }
}

