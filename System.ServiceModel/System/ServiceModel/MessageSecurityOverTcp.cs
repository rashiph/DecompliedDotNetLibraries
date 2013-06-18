namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    public sealed class MessageSecurityOverTcp
    {
        private SecurityAlgorithmSuite algorithmSuite = SecurityAlgorithmSuite.Default;
        private MessageCredentialType clientCredentialType = MessageCredentialType.Windows;
        internal const MessageCredentialType DefaultClientCredentialType = MessageCredentialType.Windows;
        private bool wasAlgorithmSuiteSet;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal SecurityBindingElement CreateSecurityBindingElement(bool isSecureTransportMode, bool isReliableSession, BindingElement transportBindingElement)
        {
            SecurityBindingElement element2;
            if (!isSecureTransportMode)
            {
                switch (this.clientCredentialType)
                {
                    case MessageCredentialType.None:
                        element2 = SecurityBindingElement.CreateSslNegotiationBindingElement(false, true);
                        goto Label_00FF;

                    case MessageCredentialType.Windows:
                        element2 = SecurityBindingElement.CreateSspiNegotiationBindingElement(true);
                        goto Label_00FF;

                    case MessageCredentialType.UserName:
                        element2 = SecurityBindingElement.CreateUserNameForSslBindingElement(true);
                        goto Label_00FF;

                    case MessageCredentialType.Certificate:
                        element2 = SecurityBindingElement.CreateSslNegotiationBindingElement(true, true);
                        goto Label_00FF;

                    case MessageCredentialType.IssuedToken:
                        element2 = SecurityBindingElement.CreateIssuedTokenForSslBindingElement(IssuedSecurityTokenParameters.CreateInfoCardParameters(new SecurityStandardsManager(), this.algorithmSuite), true);
                        goto Label_00FF;
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
                    element2 = SecurityBindingElement.CreateIssuedTokenOverTransportBindingElement(IssuedSecurityTokenParameters.CreateInfoCardParameters(new SecurityStandardsManager(), this.algorithmSuite));
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            SecurityBindingElement element = SecurityBindingElement.CreateSecureConversationBindingElement(element2);
            goto Label_0107;
        Label_00FF:
            element = SecurityBindingElement.CreateSecureConversationBindingElement(element2, true);
        Label_0107:
            element.DefaultAlgorithmSuite = element2.DefaultAlgorithmSuite = this.AlgorithmSuite;
            element.IncludeTimestamp = true;
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
            element2.LocalServiceSettings.IssuedCookieLifetime = NegotiationTokenAuthenticator<SspiNegotiationTokenAuthenticatorState>.defaultServerIssuedTransitionTokenLifetime;
            element.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11;
            element2.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11;
            return element;
        }

        internal bool InternalShouldSerialize()
        {
            if (this.ClientCredentialType == MessageCredentialType.Windows)
            {
                return (this.AlgorithmSuite != NetTcpDefaults.MessageSecurityAlgorithmSuite);
            }
            return true;
        }

        internal static bool TryCreate(SecurityBindingElement sbe, bool isReliableSession, BindingElement transportBindingElement, out MessageSecurityOverTcp messageSecurity)
        {
            MessageCredentialType userName;
            SecurityBindingElement element;
            IssuedSecurityTokenParameters parameters;
            messageSecurity = null;
            if (sbe == null)
            {
                return false;
            }
            if (!sbe.IncludeTimestamp)
            {
                return false;
            }
            if ((sbe.MessageSecurityVersion != MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11) && (sbe.MessageSecurityVersion != MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10))
            {
                return false;
            }
            if (sbe.SecurityHeaderLayout != SecurityHeaderLayout.Strict)
            {
                return false;
            }
            if (!SecurityBindingElement.IsSecureConversationBinding(sbe, true, out element))
            {
                return false;
            }
            if (element is TransportSecurityBindingElement)
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
                    if (!IssuedSecurityTokenParameters.IsInfoCardParameters(parameters, new SecurityStandardsManager(element.MessageSecurityVersion, new WSSecurityTokenSerializer(element.MessageSecurityVersion.SecurityVersion, element.MessageSecurityVersion.TrustVersion, element.MessageSecurityVersion.SecureConversationVersion, true, null, null, null))))
                    {
                        return false;
                    }
                    userName = MessageCredentialType.IssuedToken;
                }
            }
            else if (SecurityBindingElement.IsUserNameForSslBinding(element, true))
            {
                userName = MessageCredentialType.UserName;
            }
            else if (SecurityBindingElement.IsSslNegotiationBinding(element, true, true))
            {
                userName = MessageCredentialType.Certificate;
            }
            else if (SecurityBindingElement.IsSspiNegotiationBinding(element, true))
            {
                userName = MessageCredentialType.Windows;
            }
            else if (SecurityBindingElement.IsIssuedTokenForSslBinding(element, true, out parameters))
            {
                if (!IssuedSecurityTokenParameters.IsInfoCardParameters(parameters, new SecurityStandardsManager(element.MessageSecurityVersion, new WSSecurityTokenSerializer(element.MessageSecurityVersion.SecurityVersion, element.MessageSecurityVersion.TrustVersion, element.MessageSecurityVersion.SecureConversationVersion, true, null, null, null))))
                {
                    return false;
                }
                userName = MessageCredentialType.IssuedToken;
            }
            else if (SecurityBindingElement.IsSslNegotiationBinding(element, false, true))
            {
                userName = MessageCredentialType.None;
            }
            else
            {
                return false;
            }
            messageSecurity = new MessageSecurityOverTcp();
            messageSecurity.ClientCredentialType = userName;
            if (userName != MessageCredentialType.IssuedToken)
            {
                messageSecurity.AlgorithmSuite = element.DefaultAlgorithmSuite;
            }
            return true;
        }

        [DefaultValue(typeof(SecurityAlgorithmSuite), "Default")]
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

        [DefaultValue(1)]
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

        internal bool WasAlgorithmSuiteSet
        {
            get
            {
                return this.wasAlgorithmSuiteSet;
            }
        }
    }
}

