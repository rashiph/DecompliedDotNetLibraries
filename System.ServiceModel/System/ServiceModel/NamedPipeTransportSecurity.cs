namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Net.Security;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;

    public sealed class NamedPipeTransportSecurity
    {
        internal const System.Net.Security.ProtectionLevel DefaultProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
        private System.Net.Security.ProtectionLevel protectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

        internal WindowsStreamSecurityBindingElement CreateTransportProtectionAndAuthentication()
        {
            return new WindowsStreamSecurityBindingElement { ProtectionLevel = this.protectionLevel };
        }

        internal static bool IsTransportProtectionAndAuthentication(WindowsStreamSecurityBindingElement wssbe, NamedPipeTransportSecurity transportSecurity)
        {
            transportSecurity.protectionLevel = wssbe.ProtectionLevel;
            return true;
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

