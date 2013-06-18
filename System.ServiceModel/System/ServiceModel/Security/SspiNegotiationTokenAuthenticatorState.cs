namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.ServiceModel;

    internal class SspiNegotiationTokenAuthenticatorState : NegotiationTokenAuthenticatorState
    {
        private EndpointAddress appliesTo;
        private DataContractSerializer appliesToSerializer;
        private string context;
        private HashAlgorithm negotiationDigest;
        private int requestedKeySize;
        private ISspiNegotiation sspiNegotiation;

        public SspiNegotiationTokenAuthenticatorState(ISspiNegotiation sspiNegotiation)
        {
            if (sspiNegotiation == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sspiNegotiation");
            }
            this.sspiNegotiation = sspiNegotiation;
            this.negotiationDigest = CryptoHelper.NewSha1HashAlgorithm();
        }

        public override void Dispose()
        {
            try
            {
                lock (base.ThisLock)
                {
                    if (this.sspiNegotiation != null)
                    {
                        this.sspiNegotiation.Dispose();
                    }
                    if (this.negotiationDigest != null)
                    {
                        this.negotiationDigest.Dispose();
                    }
                }
            }
            finally
            {
                base.Dispose();
            }
        }

        public override string GetRemoteIdentityName()
        {
            if ((this.sspiNegotiation != null) && !base.IsNegotiationCompleted)
            {
                return this.sspiNegotiation.GetRemoteIdentityName();
            }
            return base.GetRemoteIdentityName();
        }

        internal EndpointAddress AppliesTo
        {
            get
            {
                return this.appliesTo;
            }
            set
            {
                this.appliesTo = value;
            }
        }

        internal DataContractSerializer AppliesToSerializer
        {
            get
            {
                return this.appliesToSerializer;
            }
            set
            {
                this.appliesToSerializer = value;
            }
        }

        internal string Context
        {
            get
            {
                return this.context;
            }
            set
            {
                this.context = value;
            }
        }

        internal HashAlgorithm NegotiationDigest
        {
            get
            {
                return this.negotiationDigest;
            }
        }

        internal int RequestedKeySize
        {
            get
            {
                return this.requestedKeySize;
            }
            set
            {
                this.requestedKeySize = value;
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

