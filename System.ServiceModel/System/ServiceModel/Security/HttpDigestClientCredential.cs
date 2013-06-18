namespace System.ServiceModel.Security
{
    using System;
    using System.Net;
    using System.Security.Principal;
    using System.ServiceModel;

    public sealed class HttpDigestClientCredential
    {
        private TokenImpersonationLevel allowedImpersonationLevel;
        private NetworkCredential digestCredentials;
        private bool isReadOnly;

        internal HttpDigestClientCredential()
        {
            this.allowedImpersonationLevel = TokenImpersonationLevel.Identification;
            this.digestCredentials = new NetworkCredential();
        }

        internal HttpDigestClientCredential(HttpDigestClientCredential other)
        {
            this.allowedImpersonationLevel = TokenImpersonationLevel.Identification;
            this.allowedImpersonationLevel = other.allowedImpersonationLevel;
            this.digestCredentials = System.ServiceModel.Security.SecurityUtils.GetNetworkCredentialsCopy(other.digestCredentials);
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

        public NetworkCredential ClientCredential
        {
            get
            {
                return this.digestCredentials;
            }
            set
            {
                this.ThrowIfImmutable();
                this.digestCredentials = value;
            }
        }
    }
}

