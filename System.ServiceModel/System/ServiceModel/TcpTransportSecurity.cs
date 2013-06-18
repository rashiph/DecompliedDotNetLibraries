namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;

    public sealed class TcpTransportSecurity
    {
        private TcpClientCredentialType clientCredentialType = TcpClientCredentialType.Windows;
        internal const TcpClientCredentialType DefaultClientCredentialType = TcpClientCredentialType.Windows;
        internal const System.Net.Security.ProtectionLevel DefaultProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
        private System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy extendedProtectionPolicy = ChannelBindingUtility.DefaultPolicy;
        private System.Net.Security.ProtectionLevel protectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

        private SslStreamSecurityBindingElement CreateSslBindingElement(bool requireClientCertificate)
        {
            if (this.protectionLevel != System.Net.Security.ProtectionLevel.EncryptAndSign)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSslProtectionLevel", new object[] { this.protectionLevel })));
            }
            return new SslStreamSecurityBindingElement { RequireClientCertificate = requireClientCertificate };
        }

        internal BindingElement CreateTransportProtectionAndAuthentication()
        {
            if ((this.clientCredentialType == TcpClientCredentialType.Certificate) || (this.clientCredentialType == TcpClientCredentialType.None))
            {
                return this.CreateSslBindingElement(this.clientCredentialType == TcpClientCredentialType.Certificate);
            }
            return new WindowsStreamSecurityBindingElement { ProtectionLevel = this.protectionLevel };
        }

        internal BindingElement CreateTransportProtectionOnly()
        {
            return this.CreateSslBindingElement(false);
        }

        internal bool InternalShouldSerialize()
        {
            if ((this.ClientCredentialType == TcpClientCredentialType.Windows) && (this.ProtectionLevel == System.Net.Security.ProtectionLevel.EncryptAndSign))
            {
                return this.ShouldSerializeExtendedProtectionPolicy();
            }
            return true;
        }

        private static bool IsSslBindingElement(BindingElement element, TcpTransportSecurity transportSecurity, out bool requireClientCertificate)
        {
            requireClientCertificate = false;
            SslStreamSecurityBindingElement element2 = element as SslStreamSecurityBindingElement;
            if (element2 == null)
            {
                return false;
            }
            transportSecurity.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            requireClientCertificate = element2.RequireClientCertificate;
            return true;
        }

        internal static bool SetTransportProtectionAndAuthentication(BindingElement transport, TcpTransportSecurity transportSecurity)
        {
            bool requireClientCertificate = false;
            if (transport is WindowsStreamSecurityBindingElement)
            {
                transportSecurity.ClientCredentialType = TcpClientCredentialType.Windows;
                transportSecurity.ProtectionLevel = ((WindowsStreamSecurityBindingElement) transport).ProtectionLevel;
                return true;
            }
            if (IsSslBindingElement(transport, transportSecurity, out requireClientCertificate))
            {
                transportSecurity.ClientCredentialType = requireClientCertificate ? TcpClientCredentialType.Certificate : TcpClientCredentialType.None;
                return true;
            }
            return false;
        }

        internal static bool SetTransportProtectionOnly(BindingElement transport, TcpTransportSecurity transportSecurity)
        {
            bool flag;
            return IsSslBindingElement(transport, transportSecurity, out flag);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeExtendedProtectionPolicy()
        {
            return !ChannelBindingUtility.AreEqual(this.ExtendedProtectionPolicy, ChannelBindingUtility.DefaultPolicy);
        }

        [DefaultValue(1)]
        public TcpClientCredentialType ClientCredentialType
        {
            get
            {
                return this.clientCredentialType;
            }
            set
            {
                if (!TcpClientCredentialTypeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.clientCredentialType = value;
            }
        }

        public System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy ExtendedProtectionPolicy
        {
            get
            {
                return this.extendedProtectionPolicy;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if ((value.PolicyEnforcement == PolicyEnforcement.Always) && !System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy.OSSupportsExtendedProtection)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new PlatformNotSupportedException(System.ServiceModel.SR.GetString("ExtendedProtectionNotSupported")));
                }
                this.extendedProtectionPolicy = value;
            }
        }

        [DefaultValue(2)]
        public System.Net.Security.ProtectionLevel ProtectionLevel
        {
            get
            {
                return this.protectionLevel;
            }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.protectionLevel = value;
            }
        }
    }
}

