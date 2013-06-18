namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    public sealed class IssuedTokenClientCredential
    {
        private bool cacheIssuedTokens;
        private SecurityKeyEntropyMode defaultKeyEntropyMode;
        private bool isReadOnly;
        private int issuedTokenRenewalThresholdPercentage;
        private Dictionary<Uri, KeyedByTypeCollection<IEndpointBehavior>> issuerChannelBehaviors;
        private EndpointAddress localIssuerAddress;
        private Binding localIssuerBinding;
        private KeyedByTypeCollection<IEndpointBehavior> localIssuerChannelBehaviors;
        private TimeSpan maxIssuedTokenCachingTime;

        internal IssuedTokenClientCredential()
        {
            this.defaultKeyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
            this.cacheIssuedTokens = true;
            this.maxIssuedTokenCachingTime = IssuanceTokenProviderBase<SspiNegotiationTokenProviderState>.DefaultClientMaxTokenCachingTime;
            this.issuedTokenRenewalThresholdPercentage = 60;
        }

        internal IssuedTokenClientCredential(IssuedTokenClientCredential other)
        {
            this.defaultKeyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
            this.cacheIssuedTokens = true;
            this.maxIssuedTokenCachingTime = IssuanceTokenProviderBase<SspiNegotiationTokenProviderState>.DefaultClientMaxTokenCachingTime;
            this.issuedTokenRenewalThresholdPercentage = 60;
            this.defaultKeyEntropyMode = other.defaultKeyEntropyMode;
            this.cacheIssuedTokens = other.cacheIssuedTokens;
            this.issuedTokenRenewalThresholdPercentage = other.issuedTokenRenewalThresholdPercentage;
            this.maxIssuedTokenCachingTime = other.maxIssuedTokenCachingTime;
            this.localIssuerAddress = other.localIssuerAddress;
            this.localIssuerBinding = (other.localIssuerBinding != null) ? new CustomBinding(other.localIssuerBinding) : null;
            if (other.localIssuerChannelBehaviors != null)
            {
                this.localIssuerChannelBehaviors = this.GetBehaviorCollection(other.localIssuerChannelBehaviors);
            }
            if (other.issuerChannelBehaviors != null)
            {
                this.issuerChannelBehaviors = new Dictionary<Uri, KeyedByTypeCollection<IEndpointBehavior>>();
                foreach (Uri uri in other.issuerChannelBehaviors.Keys)
                {
                    this.issuerChannelBehaviors.Add(uri, this.GetBehaviorCollection(other.issuerChannelBehaviors[uri]));
                }
            }
            this.isReadOnly = other.isReadOnly;
        }

        private KeyedByTypeCollection<IEndpointBehavior> GetBehaviorCollection(KeyedByTypeCollection<IEndpointBehavior> behaviors)
        {
            KeyedByTypeCollection<IEndpointBehavior> types = new KeyedByTypeCollection<IEndpointBehavior>();
            foreach (IEndpointBehavior behavior in behaviors)
            {
                types.Add(behavior);
            }
            return types;
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

        public bool CacheIssuedTokens
        {
            get
            {
                return this.cacheIssuedTokens;
            }
            set
            {
                this.ThrowIfImmutable();
                this.cacheIssuedTokens = value;
            }
        }

        public SecurityKeyEntropyMode DefaultKeyEntropyMode
        {
            get
            {
                return this.defaultKeyEntropyMode;
            }
            set
            {
                SecurityKeyEntropyModeHelper.Validate(value);
                this.ThrowIfImmutable();
                this.defaultKeyEntropyMode = value;
            }
        }

        public int IssuedTokenRenewalThresholdPercentage
        {
            get
            {
                return this.issuedTokenRenewalThresholdPercentage;
            }
            set
            {
                this.ThrowIfImmutable();
                this.issuedTokenRenewalThresholdPercentage = value;
            }
        }

        public Dictionary<Uri, KeyedByTypeCollection<IEndpointBehavior>> IssuerChannelBehaviors
        {
            get
            {
                if (this.issuerChannelBehaviors == null)
                {
                    this.issuerChannelBehaviors = new Dictionary<Uri, KeyedByTypeCollection<IEndpointBehavior>>();
                }
                return this.issuerChannelBehaviors;
            }
        }

        public EndpointAddress LocalIssuerAddress
        {
            get
            {
                return this.localIssuerAddress;
            }
            set
            {
                this.ThrowIfImmutable();
                this.localIssuerAddress = value;
            }
        }

        public Binding LocalIssuerBinding
        {
            get
            {
                return this.localIssuerBinding;
            }
            set
            {
                this.ThrowIfImmutable();
                this.localIssuerBinding = value;
            }
        }

        public KeyedByTypeCollection<IEndpointBehavior> LocalIssuerChannelBehaviors
        {
            get
            {
                if (this.localIssuerChannelBehaviors == null)
                {
                    this.localIssuerChannelBehaviors = new KeyedByTypeCollection<IEndpointBehavior>();
                }
                return this.localIssuerChannelBehaviors;
            }
        }

        public TimeSpan MaxIssuedTokenCachingTime
        {
            get
            {
                return this.maxIssuedTokenCachingTime;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.ThrowIfImmutable();
                this.maxIssuedTokenCachingTime = value;
            }
        }
    }
}

