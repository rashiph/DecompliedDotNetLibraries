namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Net.Security;
    using System.ServiceModel.Security;

    public sealed class MsmqTransportSecurity
    {
        private System.ServiceModel.MsmqAuthenticationMode msmqAuthenticationMode;
        private System.ServiceModel.MsmqEncryptionAlgorithm msmqEncryptionAlgorithm;
        private System.ServiceModel.MsmqSecureHashAlgorithm msmqHashAlgorithm;
        private ProtectionLevel msmqProtectionLevel;

        public MsmqTransportSecurity()
        {
            this.msmqAuthenticationMode = System.ServiceModel.MsmqAuthenticationMode.WindowsDomain;
            this.msmqEncryptionAlgorithm = System.ServiceModel.MsmqEncryptionAlgorithm.RC4Stream;
            this.msmqHashAlgorithm = System.ServiceModel.MsmqSecureHashAlgorithm.Sha1;
            this.msmqProtectionLevel = ProtectionLevel.Sign;
        }

        public MsmqTransportSecurity(MsmqTransportSecurity other)
        {
            if (other == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("other");
            }
            this.msmqAuthenticationMode = other.MsmqAuthenticationMode;
            this.msmqEncryptionAlgorithm = other.MsmqEncryptionAlgorithm;
            this.msmqHashAlgorithm = other.MsmqSecureHashAlgorithm;
            this.msmqProtectionLevel = other.MsmqProtectionLevel;
        }

        internal void Disable()
        {
            this.msmqAuthenticationMode = System.ServiceModel.MsmqAuthenticationMode.None;
            this.msmqProtectionLevel = ProtectionLevel.None;
        }

        internal bool Enabled
        {
            get
            {
                return ((this.msmqAuthenticationMode != System.ServiceModel.MsmqAuthenticationMode.None) && (this.msmqProtectionLevel != ProtectionLevel.None));
            }
        }

        [DefaultValue(1)]
        public System.ServiceModel.MsmqAuthenticationMode MsmqAuthenticationMode
        {
            get
            {
                return this.msmqAuthenticationMode;
            }
            set
            {
                if (!MsmqAuthenticationModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.msmqAuthenticationMode = value;
            }
        }

        [DefaultValue(0)]
        public System.ServiceModel.MsmqEncryptionAlgorithm MsmqEncryptionAlgorithm
        {
            get
            {
                return this.msmqEncryptionAlgorithm;
            }
            set
            {
                if (!MsmqEncryptionAlgorithmHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.msmqEncryptionAlgorithm = value;
            }
        }

        [DefaultValue(1)]
        public ProtectionLevel MsmqProtectionLevel
        {
            get
            {
                return this.msmqProtectionLevel;
            }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.msmqProtectionLevel = value;
            }
        }

        [DefaultValue(1)]
        public System.ServiceModel.MsmqSecureHashAlgorithm MsmqSecureHashAlgorithm
        {
            get
            {
                return this.msmqHashAlgorithm;
            }
            set
            {
                if (!MsmqSecureHashAlgorithmHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.msmqHashAlgorithm = value;
            }
        }
    }
}

