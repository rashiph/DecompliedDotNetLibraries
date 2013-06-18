namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;

    public sealed class BasicHttpMessageSecurity
    {
        private SecurityAlgorithmSuite algorithmSuite = SecurityAlgorithmSuite.Default;
        private BasicHttpMessageCredentialType clientCredentialType = BasicHttpMessageCredentialType.UserName;
        internal const BasicHttpMessageCredentialType DefaultClientCredentialType = BasicHttpMessageCredentialType.UserName;

        internal SecurityBindingElement CreateMessageSecurity(bool isSecureTransportMode)
        {
            SecurityBindingElement element;
            if (isSecureTransportMode)
            {
                MessageSecurityVersion version = MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;
                switch (this.clientCredentialType)
                {
                    case BasicHttpMessageCredentialType.UserName:
                        element = SecurityBindingElement.CreateUserNameOverTransportBindingElement();
                        element.MessageSecurityVersion = version;
                        goto Label_0077;

                    case BasicHttpMessageCredentialType.Certificate:
                        element = SecurityBindingElement.CreateCertificateOverTransportBindingElement(version);
                        goto Label_0077;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            if (this.clientCredentialType != BasicHttpMessageCredentialType.Certificate)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BasicHttpMessageSecurityRequiresCertificate")));
            }
            element = SecurityBindingElement.CreateMutualCertificateBindingElement(MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10, true);
        Label_0077:
            element.DefaultAlgorithmSuite = this.AlgorithmSuite;
            element.SecurityHeaderLayout = SecurityHeaderLayout.Lax;
            element.SetKeyDerivation(false);
            element.DoNotEmitTrust = true;
            return element;
        }

        internal bool InternalShouldSerialize()
        {
            if (!this.ShouldSerializeAlgorithmSuite())
            {
                return this.ShouldSerializeClientCredentialType();
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeAlgorithmSuite()
        {
            return (this.algorithmSuite.GetType() != SecurityAlgorithmSuite.Default.GetType());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeClientCredentialType()
        {
            return (this.clientCredentialType != BasicHttpMessageCredentialType.UserName);
        }

        internal static bool TryCreate(SecurityBindingElement sbe, out BasicHttpMessageSecurity security, out bool isSecureTransportMode)
        {
            BasicHttpMessageCredentialType userName;
            security = null;
            isSecureTransportMode = false;
            if (!sbe.DoNotEmitTrust)
            {
                return false;
            }
            if (!sbe.IsSetKeyDerivation(false))
            {
                return false;
            }
            if (sbe.SecurityHeaderLayout != SecurityHeaderLayout.Lax)
            {
                return false;
            }
            if (sbe.MessageSecurityVersion != MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10)
            {
                return false;
            }
            if (!SecurityBindingElement.IsMutualCertificateBinding(sbe, true))
            {
                isSecureTransportMode = true;
                if (!SecurityBindingElement.IsCertificateOverTransportBinding(sbe))
                {
                    if (!SecurityBindingElement.IsUserNameOverTransportBinding(sbe))
                    {
                        return false;
                    }
                    userName = BasicHttpMessageCredentialType.UserName;
                }
                else
                {
                    userName = BasicHttpMessageCredentialType.Certificate;
                }
            }
            else
            {
                userName = BasicHttpMessageCredentialType.Certificate;
            }
            security = new BasicHttpMessageSecurity();
            security.ClientCredentialType = userName;
            security.AlgorithmSuite = sbe.DefaultAlgorithmSuite;
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
            }
        }

        public BasicHttpMessageCredentialType ClientCredentialType
        {
            get
            {
                return this.clientCredentialType;
            }
            set
            {
                if (!BasicHttpMessageCredentialTypeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.clientCredentialType = value;
            }
        }
    }
}

