namespace System.ServiceModel.Security
{
    using System;
    using System.Security.Cryptography;
    using System.ServiceModel;

    internal class SspiNegotiationTokenProviderState : IssuanceTokenProviderState
    {
        private HashAlgorithm negotiationDigest;
        private ISspiNegotiation sspiNegotiation;

        public SspiNegotiationTokenProviderState(ISspiNegotiation sspiNegotiation)
        {
            if (sspiNegotiation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sspiNegotiation");
            }
            this.sspiNegotiation = sspiNegotiation;
            this.negotiationDigest = CryptoHelper.NewSha1HashAlgorithm();
        }

        public override void Dispose()
        {
            try
            {
                if (this.sspiNegotiation != null)
                {
                    this.sspiNegotiation.Dispose();
                    this.sspiNegotiation = null;
                    this.negotiationDigest.Dispose();
                    this.negotiationDigest = null;
                }
            }
            finally
            {
                base.Dispose();
            }
        }

        internal HashAlgorithm NegotiationDigest
        {
            get
            {
                return this.negotiationDigest;
            }
        }

        public ISspiNegotiation SspiNegotiation
        {
            get
            {
                return this.sspiNegotiation;
            }
        }
    }
}

