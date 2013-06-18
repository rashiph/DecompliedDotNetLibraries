namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;

    internal class TokenTracker
    {
        private bool allowFirstTokenMismatch;
        public bool AlreadyReadEndorsingSignature;
        public bool IsDerivedFrom;
        public bool IsEncrypted;
        public bool IsEndorsing;
        public bool IsSigned;
        public SupportingTokenAuthenticatorSpecification spec;
        public SecurityToken token;

        public TokenTracker(SupportingTokenAuthenticatorSpecification spec) : this(spec, null, false)
        {
        }

        public TokenTracker(SupportingTokenAuthenticatorSpecification spec, SecurityToken token, bool allowFirstTokenMismatch)
        {
            this.spec = spec;
            this.token = token;
            this.allowFirstTokenMismatch = allowFirstTokenMismatch;
        }

        private static bool AreTokensEqual(SecurityToken outOfBandToken, SecurityToken replyToken)
        {
            if ((outOfBandToken is X509SecurityToken) && (replyToken is X509SecurityToken))
            {
                byte[] certHash = ((X509SecurityToken) outOfBandToken).Certificate.GetCertHash();
                byte[] b = ((X509SecurityToken) replyToken).Certificate.GetCertHash();
                return CryptoHelper.IsEqual(certHash, b);
            }
            return false;
        }

        public void RecordToken(SecurityToken token)
        {
            if (this.token == null)
            {
                this.token = token;
            }
            else if (this.allowFirstTokenMismatch)
            {
                if (!AreTokensEqual(this.token, token))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MismatchInSecurityOperationToken")));
                }
                this.token = token;
                this.allowFirstTokenMismatch = false;
            }
            else if (!object.ReferenceEquals(this.token, token))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MismatchInSecurityOperationToken")));
            }
        }
    }
}

