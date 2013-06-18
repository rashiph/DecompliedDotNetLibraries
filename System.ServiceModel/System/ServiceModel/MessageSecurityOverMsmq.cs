namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    public sealed class MessageSecurityOverMsmq
    {
        private SecurityAlgorithmSuite algorithmSuite = SecurityAlgorithmSuite.Default;
        private MessageCredentialType clientCredentialType = MessageCredentialType.Windows;
        internal const MessageCredentialType DefaultClientCredentialType = MessageCredentialType.Windows;
        private bool wasAlgorithmSuiteSet;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal SecurityBindingElement CreateSecurityBindingElement()
        {
            SymmetricSecurityBindingElement element;
            bool flag = false;
            switch (this.clientCredentialType)
            {
                case MessageCredentialType.None:
                    element = SecurityBindingElement.CreateAnonymousForCertificateBindingElement();
                    break;

                case MessageCredentialType.Windows:
                    element = SecurityBindingElement.CreateKerberosBindingElement();
                    flag = true;
                    break;

                case MessageCredentialType.UserName:
                    element = SecurityBindingElement.CreateUserNameForCertificateBindingElement();
                    break;

                case MessageCredentialType.Certificate:
                    element = (SymmetricSecurityBindingElement) SecurityBindingElement.CreateMutualCertificateBindingElement();
                    break;

                case MessageCredentialType.IssuedToken:
                    element = SecurityBindingElement.CreateIssuedTokenForCertificateBindingElement(IssuedSecurityTokenParameters.CreateInfoCardParameters(new SecurityStandardsManager(), this.algorithmSuite));
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            element.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11;
            if (this.wasAlgorithmSuiteSet || !flag)
            {
                element.DefaultAlgorithmSuite = this.AlgorithmSuite;
            }
            else if (flag)
            {
                element.DefaultAlgorithmSuite = SecurityAlgorithmSuite.KerberosDefault;
            }
            element.IncludeTimestamp = false;
            element.LocalServiceSettings.DetectReplays = false;
            element.LocalClientSettings.DetectReplays = false;
            return element;
        }

        internal static bool TryCreate(SecurityBindingElement sbe, out MessageSecurityOverMsmq messageSecurity)
        {
            MessageCredentialType none;
            messageSecurity = null;
            if (sbe == null)
            {
                return false;
            }
            SymmetricSecurityBindingElement element = sbe as SymmetricSecurityBindingElement;
            if (element == null)
            {
                return false;
            }
            if ((sbe.MessageSecurityVersion != MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10) && (sbe.MessageSecurityVersion != MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11))
            {
                return false;
            }
            if (element.IncludeTimestamp)
            {
                return false;
            }
            bool flag = false;
            if (SecurityBindingElement.IsAnonymousForCertificateBinding(sbe))
            {
                none = MessageCredentialType.None;
            }
            else if (SecurityBindingElement.IsUserNameForCertificateBinding(sbe))
            {
                none = MessageCredentialType.UserName;
            }
            else if (SecurityBindingElement.IsMutualCertificateBinding(sbe))
            {
                none = MessageCredentialType.Certificate;
            }
            else if (SecurityBindingElement.IsKerberosBinding(sbe))
            {
                none = MessageCredentialType.Windows;
                flag = true;
            }
            else
            {
                IssuedSecurityTokenParameters parameters;
                if (!SecurityBindingElement.IsIssuedTokenForCertificateBinding(sbe, out parameters))
                {
                    return false;
                }
                if (!IssuedSecurityTokenParameters.IsInfoCardParameters(parameters, new SecurityStandardsManager(sbe.MessageSecurityVersion, new WSSecurityTokenSerializer(sbe.MessageSecurityVersion.SecurityVersion, sbe.MessageSecurityVersion.TrustVersion, sbe.MessageSecurityVersion.SecureConversationVersion, true, null, null, null))))
                {
                    return false;
                }
                none = MessageCredentialType.IssuedToken;
            }
            messageSecurity = new MessageSecurityOverMsmq();
            messageSecurity.ClientCredentialType = none;
            if ((none != MessageCredentialType.IssuedToken) && !flag)
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

