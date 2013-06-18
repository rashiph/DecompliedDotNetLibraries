namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;

    internal class SendSecurityHeaderElementContainer
    {
        private List<SendSecurityHeaderElement> basicSupportingTokens;
        public SecurityToken DerivedEncryptionToken;
        public SecurityToken DerivedSigningToken;
        private List<SecurityToken> endorsingDerivedSupportingTokens;
        private List<SendSecurityHeaderElement> endorsingSignatures;
        private List<SecurityToken> endorsingSupportingTokens;
        public SecurityToken PrerequisiteToken;
        public SendSecurityHeaderElement PrimarySignature;
        public ISecurityElement ReferenceList;
        private List<SendSecurityHeaderElement> signatureConfirmations;
        private List<SecurityToken> signedEndorsingDerivedSupportingTokens;
        private List<SecurityToken> signedEndorsingSupportingTokens;
        private List<SecurityToken> signedSupportingTokens;
        public SecurityToken SourceEncryptionToken;
        public SecurityToken SourceSigningToken;
        public SecurityTimestamp Timestamp;
        public SecurityToken WrappedEncryptionToken;

        private void Add<T>(ref List<T> list, T item)
        {
            if (list == null)
            {
                list = new List<T>();
            }
            list.Add(item);
        }

        public void AddBasicSupportingToken(SendSecurityHeaderElement tokenElement)
        {
            this.Add<SendSecurityHeaderElement>(ref this.basicSupportingTokens, tokenElement);
        }

        public void AddEndorsingDerivedSupportingToken(SecurityToken token)
        {
            this.Add<SecurityToken>(ref this.endorsingDerivedSupportingTokens, token);
        }

        public void AddEndorsingSignature(SendSecurityHeaderElement signature)
        {
            this.Add<SendSecurityHeaderElement>(ref this.endorsingSignatures, signature);
        }

        public void AddEndorsingSupportingToken(SecurityToken token)
        {
            this.Add<SecurityToken>(ref this.endorsingSupportingTokens, token);
        }

        public void AddSignatureConfirmation(SendSecurityHeaderElement confirmation)
        {
            this.Add<SendSecurityHeaderElement>(ref this.signatureConfirmations, confirmation);
        }

        public void AddSignedEndorsingDerivedSupportingToken(SecurityToken token)
        {
            this.Add<SecurityToken>(ref this.signedEndorsingDerivedSupportingTokens, token);
        }

        public void AddSignedEndorsingSupportingToken(SecurityToken token)
        {
            this.Add<SecurityToken>(ref this.signedEndorsingSupportingTokens, token);
        }

        public void AddSignedSupportingToken(SecurityToken token)
        {
            this.Add<SecurityToken>(ref this.signedSupportingTokens, token);
        }

        public SendSecurityHeaderElement[] GetBasicSupportingTokens()
        {
            if (this.basicSupportingTokens == null)
            {
                return null;
            }
            return this.basicSupportingTokens.ToArray();
        }

        public SecurityToken[] GetEndorsingDerivedSupportingTokens()
        {
            if (this.endorsingDerivedSupportingTokens == null)
            {
                return null;
            }
            return this.endorsingDerivedSupportingTokens.ToArray();
        }

        public SendSecurityHeaderElement[] GetEndorsingSignatures()
        {
            if (this.endorsingSignatures == null)
            {
                return null;
            }
            return this.endorsingSignatures.ToArray();
        }

        public SecurityToken[] GetEndorsingSupportingTokens()
        {
            if (this.endorsingSupportingTokens == null)
            {
                return null;
            }
            return this.endorsingSupportingTokens.ToArray();
        }

        public SendSecurityHeaderElement[] GetSignatureConfirmations()
        {
            if (this.signatureConfirmations == null)
            {
                return null;
            }
            return this.signatureConfirmations.ToArray();
        }

        public SecurityToken[] GetSignedEndorsingDerivedSupportingTokens()
        {
            if (this.signedEndorsingDerivedSupportingTokens == null)
            {
                return null;
            }
            return this.signedEndorsingDerivedSupportingTokens.ToArray();
        }

        public SecurityToken[] GetSignedEndorsingSupportingTokens()
        {
            if (this.signedEndorsingSupportingTokens == null)
            {
                return null;
            }
            return this.signedEndorsingSupportingTokens.ToArray();
        }

        public SecurityToken[] GetSignedSupportingTokens()
        {
            if (this.signedSupportingTokens == null)
            {
                return null;
            }
            return this.signedSupportingTokens.ToArray();
        }

        public List<SecurityToken> EndorsingSupportingTokens
        {
            get
            {
                return this.endorsingSupportingTokens;
            }
        }
    }
}

