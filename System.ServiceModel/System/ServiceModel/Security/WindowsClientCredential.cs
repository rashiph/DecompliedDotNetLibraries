namespace System.ServiceModel.Security
{
    using System;
    using System.Net;
    using System.Security.Principal;
    using System.ServiceModel;

    public sealed class WindowsClientCredential
    {
        private TokenImpersonationLevel allowedImpersonationLevel;
        private bool allowNtlm;
        internal const TokenImpersonationLevel DefaultImpersonationLevel = TokenImpersonationLevel.Identification;
        private bool isReadOnly;
        private NetworkCredential windowsCredentials;

        internal WindowsClientCredential()
        {
            this.allowedImpersonationLevel = TokenImpersonationLevel.Identification;
            this.allowNtlm = true;
        }

        internal WindowsClientCredential(WindowsClientCredential other)
        {
            this.allowedImpersonationLevel = TokenImpersonationLevel.Identification;
            this.allowNtlm = true;
            if (other.windowsCredentials != null)
            {
                this.windowsCredentials = System.ServiceModel.Security.SecurityUtils.GetNetworkCredentialsCopy(other.windowsCredentials);
            }
            this.allowedImpersonationLevel = other.allowedImpersonationLevel;
            this.allowNtlm = other.allowNtlm;
            this.isReadOnly = other.isReadOnly;
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        private void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
        }

        public TokenImpersonationLevel AllowedImpersonationLevel
        {
            get
            {
                return this.allowedImpersonationLevel;
            }
            set
            {
                this.ThrowIfImmutable();
                this.allowedImpersonationLevel = value;
            }
        }

        [Obsolete("This property is deprecated and is maintained for backward compatibility only. The local machine policy will be used to determine if NTLM should be used.")]
        public bool AllowNtlm
        {
            get
            {
                return this.allowNtlm;
            }
            set
            {
                this.ThrowIfImmutable();
                this.allowNtlm = value;
            }
        }

        public NetworkCredential ClientCredential
        {
            get
            {
                if (this.windowsCredentials == null)
                {
                    this.windowsCredentials = new NetworkCredential();
                }
                return this.windowsCredentials;
            }
            set
            {
                this.ThrowIfImmutable();
                this.windowsCredentials = value;
            }
        }
    }
}

