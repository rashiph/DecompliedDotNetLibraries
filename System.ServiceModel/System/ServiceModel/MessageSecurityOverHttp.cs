namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    public class MessageSecurityOverHttp
    {
        private SecurityAlgorithmSuite algorithmSuite = SecurityAlgorithmSuite.Default;
        private MessageCredentialType clientCredentialType = MessageCredentialType.Windows;
        internal const MessageCredentialType DefaultClientCredentialType = MessageCredentialType.Windows;
        internal const bool DefaultNegotiateServiceCredential = true;
        private bool negotiateServiceCredential = true;
        private bool wasAlgorithmSuiteSet;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal SecurityBindingElement CreateSecurityBindingElement(bool isSecureTransportMode, bool isReliableSession, MessageSecurityVersion version)
        {
            SecurityBindingElement element;
            SecurityBindingElement element2;
            if (isReliableSession && !this.IsSecureConversationEnabled())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecureConversationRequiredByReliableSession")));
            }
            bool flag = false;
            bool emitBspRequiredAttributes = true;
            if (!isSecureTransportMode)
            {
                if (this.negotiateServiceCredential)
                {
                    switch (this.clientCredentialType)
                    {
                        case MessageCredentialType.None:
                            element2 = SecurityBindingElement.CreateSslNegotiationBindingElement(false, true);
                            goto Label_01DA;

                        case MessageCredentialType.Windows:
                            element2 = SecurityBindingElement.CreateSspiNegotiationBindingElement(true);
                            goto Label_01DA;

                        case MessageCredentialType.UserName:
                            element2 = SecurityBindingElement.CreateUserNameForSslBindingElement(true);
                            goto Label_01DA;

                        case MessageCredentialType.Certificate:
                            element2 = SecurityBindingElement.CreateSslNegotiationBindingElement(true, true);
                            goto Label_01DA;

                        case MessageCredentialType.IssuedToken:
                            element2 = SecurityBindingElement.CreateIssuedTokenForSslBindingElement(IssuedSecurityTokenParameters.CreateInfoCardParameters(new SecurityStandardsManager(new WSSecurityTokenSerializer(emitBspRequiredAttributes)), this.algorithmSuite), true);
                            goto Label_01DA;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                switch (this.clientCredentialType)
                {
                    case MessageCredentialType.None:
                        element2 = SecurityBindingElement.CreateAnonymousForCertificateBindingElement();
                        goto Label_01DA;

                    case MessageCredentialType.Windows:
                        element2 = SecurityBindingElement.CreateKerberosBindingElement();
                        flag = true;
                        goto Label_01DA;

                    case MessageCredentialType.UserName:
                        element2 = SecurityBindingElement.CreateUserNameForCertificateBindingElement();
                        goto Label_01DA;

                    case MessageCredentialType.Certificate:
                        element2 = SecurityBindingElement.CreateMutualCertificateBindingElement();
                        goto Label_01DA;

                    case MessageCredentialType.IssuedToken:
                        element2 = SecurityBindingElement.CreateIssuedTokenForCertificateBindingElement(IssuedSecurityTokenParameters.CreateInfoCardParameters(new SecurityStandardsManager(new WSSecurityTokenSerializer(emitBspRequiredAttributes)), this.algorithmSuite));
                        goto Label_01DA;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            switch (this.clientCredentialType)
            {
                case MessageCredentialType.None:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ClientCredentialTypeMustBeSpecifiedForMixedMode")));

                case MessageCredentialType.Windows:
                    element2 = SecurityBindingElement.CreateSspiNegotiationOverTransportBindingElement(true);
                    break;

                case MessageCredentialType.UserName:
                    element2 = SecurityBindingElement.CreateUserNameOverTransportBindingElement();
                    break;

                case MessageCredentialType.Certificate:
                    element2 = SecurityBindingElement.CreateCertificateOverTransportBindingElement();
                    break;

                case MessageCredentialType.IssuedToken:
                    element2 = SecurityBindingElement.CreateIssuedTokenOverTransportBindingElement(IssuedSecurityTokenParameters.CreateInfoCardParameters(new SecurityStandardsManager(new WSSecurityTokenSerializer(emitBspRequiredAttributes)), this.algorithmSuite));
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            if (this.IsSecureConversationEnabled())
            {
                element = SecurityBindingElement.CreateSecureConversationBindingElement(element2, true);
            }
            else
            {
                element = element2;
            }
            goto Label_01EE;
        Label_01DA:
            if (this.IsSecureConversationEnabled())
            {
                element = SecurityBindingElement.CreateSecureConversationBindingElement(element2, true);
            }
            else
            {
                element = element2;
            }
        Label_01EE:
            if (this.wasAlgorithmSuiteSet || !flag)
            {
                element.DefaultAlgorithmSuite = element2.DefaultAlgorithmSuite = this.AlgorithmSuite;
            }
            else if (flag)
            {
                element.DefaultAlgorithmSuite = element2.DefaultAlgorithmSuite = SecurityAlgorithmSuite.KerberosDefault;
            }
            element.IncludeTimestamp = true;
            element2.MessageSecurityVersion = version;
            element.MessageSecurityVersion = version;
            if (!isReliableSession)
            {
                element.LocalServiceSettings.ReconnectTransportOnFailure = false;
                element.LocalClientSettings.ReconnectTransportOnFailure = false;
            }
            else
            {
                element.LocalServiceSettings.ReconnectTransportOnFailure = true;
                element.LocalClientSettings.ReconnectTransportOnFailure = true;
            }
            if (this.IsSecureConversationEnabled())
            {
                element2.LocalServiceSettings.IssuedCookieLifetime = NegotiationTokenAuthenticator<SspiNegotiationTokenAuthenticatorState>.defaultServerIssuedTransitionTokenLifetime;
            }
            return element;
        }

        internal bool InternalShouldSerialize()
        {
            if (!this.ShouldSerializeAlgorithmSuite() && !this.ShouldSerializeClientCredentialType())
            {
                return this.ShouldSerializeNegotiateServiceCredential();
            }
            return true;
        }

        protected virtual bool IsSecureConversationEnabled()
        {
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeAlgorithmSuite()
        {
            return (this.AlgorithmSuite != SecurityAlgorithmSuite.Default);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeClientCredentialType()
        {
            return (this.ClientCredentialType != MessageCredentialType.Windows);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeNegotiateServiceCredential()
        {
            return !this.NegotiateServiceCredential;
        }

        internal static bool TryCreate<TSecurity>(SecurityBindingElement sbe, bool isSecureTransportMode, bool isReliableSession, out TSecurity messageSecurity) where TSecurity: MessageSecurityOverHttp
        {
            MessageCredentialType userName;
            bool flag2;
            SecurityBindingElement element;
            IssuedSecurityTokenParameters parameters;
            messageSecurity = default(TSecurity);
            if (!sbe.IncludeTimestamp)
            {
                return false;
            }
            if (sbe.SecurityHeaderLayout != SecurityHeaderLayout.Strict)
            {
                return false;
            }
            bool flag = true;
            SecurityAlgorithmSuite suite1 = SecurityAlgorithmSuite.Default;
            if (!SecurityBindingElement.IsSecureConversationBinding(sbe, true, out element))
            {
                flag2 = false;
                element = sbe;
            }
            else
            {
                flag2 = true;
            }
            if (!flag2 && typeof(TSecurity).Equals(typeof(MessageSecurityOverHttp)))
            {
                return false;
            }
            if (!flag2 && isReliableSession)
            {
                return false;
            }
            if (isSecureTransportMode && !(element is TransportSecurityBindingElement))
            {
                return false;
            }
            if (isSecureTransportMode)
            {
                if (SecurityBindingElement.IsUserNameOverTransportBinding(element))
                {
                    userName = MessageCredentialType.UserName;
                }
                else if (SecurityBindingElement.IsCertificateOverTransportBinding(element))
                {
                    userName = MessageCredentialType.Certificate;
                }
                else if (SecurityBindingElement.IsSspiNegotiationOverTransportBinding(element, true))
                {
                    userName = MessageCredentialType.Windows;
                }
                else
                {
                    if (!SecurityBindingElement.IsIssuedTokenOverTransportBinding(element, out parameters))
                    {
                        return false;
                    }
                    if (!IssuedSecurityTokenParameters.IsInfoCardParameters(parameters, new SecurityStandardsManager(sbe.MessageSecurityVersion, new WSSecurityTokenSerializer(sbe.MessageSecurityVersion.SecurityVersion, sbe.MessageSecurityVersion.TrustVersion, sbe.MessageSecurityVersion.SecureConversationVersion, true, null, null, null))))
                    {
                        return false;
                    }
                    userName = MessageCredentialType.IssuedToken;
                }
            }
            else if (SecurityBindingElement.IsSslNegotiationBinding(element, false, true))
            {
                flag = true;
                userName = MessageCredentialType.None;
            }
            else if (SecurityBindingElement.IsUserNameForSslBinding(element, true))
            {
                flag = true;
                userName = MessageCredentialType.UserName;
            }
            else if (SecurityBindingElement.IsSslNegotiationBinding(element, true, true))
            {
                flag = true;
                userName = MessageCredentialType.Certificate;
            }
            else if (SecurityBindingElement.IsSspiNegotiationBinding(element, true))
            {
                flag = true;
                userName = MessageCredentialType.Windows;
            }
            else if (SecurityBindingElement.IsIssuedTokenForSslBinding(element, true, out parameters))
            {
                if (!IssuedSecurityTokenParameters.IsInfoCardParameters(parameters, new SecurityStandardsManager(sbe.MessageSecurityVersion, new WSSecurityTokenSerializer(sbe.MessageSecurityVersion.SecurityVersion, sbe.MessageSecurityVersion.TrustVersion, sbe.MessageSecurityVersion.SecureConversationVersion, true, null, null, null))))
                {
                    return false;
                }
                flag = true;
                userName = MessageCredentialType.IssuedToken;
            }
            else if (SecurityBindingElement.IsUserNameForCertificateBinding(element))
            {
                flag = false;
                userName = MessageCredentialType.UserName;
            }
            else if (SecurityBindingElement.IsMutualCertificateBinding(element))
            {
                flag = false;
                userName = MessageCredentialType.Certificate;
            }
            else if (SecurityBindingElement.IsKerberosBinding(element))
            {
                flag = false;
                userName = MessageCredentialType.Windows;
            }
            else if (SecurityBindingElement.IsIssuedTokenForCertificateBinding(element, out parameters))
            {
                if (!IssuedSecurityTokenParameters.IsInfoCardParameters(parameters, new SecurityStandardsManager(sbe.MessageSecurityVersion, new WSSecurityTokenSerializer(sbe.MessageSecurityVersion.SecurityVersion, sbe.MessageSecurityVersion.TrustVersion, sbe.MessageSecurityVersion.SecureConversationVersion, true, null, null, null))))
                {
                    return false;
                }
                flag = false;
                userName = MessageCredentialType.IssuedToken;
            }
            else if (SecurityBindingElement.IsAnonymousForCertificateBinding(element))
            {
                flag = false;
                userName = MessageCredentialType.None;
            }
            else
            {
                return false;
            }
            if (typeof(NonDualMessageSecurityOverHttp).Equals(typeof(TSecurity)))
            {
                messageSecurity = (TSecurity) new NonDualMessageSecurityOverHttp();
                ((NonDualMessageSecurityOverHttp) ((TSecurity) messageSecurity)).EstablishSecurityContext = flag2;
            }
            else
            {
                messageSecurity = (TSecurity) new MessageSecurityOverHttp();
            }
            messageSecurity.ClientCredentialType = userName;
            messageSecurity.NegotiateServiceCredential = flag;
            messageSecurity.AlgorithmSuite = sbe.DefaultAlgorithmSuite;
            return true;
        }

        public SecurityAlgorithmSuite AlgorithmSuite
        {
            get
            {
                return this.algorithmSuite;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.algorithmSuite = value;
                this.wasAlgorithmSuiteSet = true;
            }
        }

        public MessageCredentialType ClientCredentialType
        {
            get
            {
                return this.clientCredentialType;
            }
            set
            {
                if (!MessageCredentialTypeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.clientCredentialType = value;
            }
        }

        public bool NegotiateServiceCredential
        {
            get
            {
                return this.negotiateServiceCredential;
            }
            set
            {
                this.negotiateServiceCredential = value;
            }
        }

        internal bool WasAlgorithmSuiteSet
        {
            get
            {
                return this.wasAlgorithmSuiteSet;
            }
        }
    }
}

