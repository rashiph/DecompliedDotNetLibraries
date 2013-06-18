namespace System.IdentityModel.Tokens
{
    using System;
    using System.IdentityModel;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;

    public abstract class BinaryKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        private readonly byte[] identificationData;

        protected BinaryKeyIdentifierClause(string clauseType, byte[] identificationData, bool cloneBuffer) : this(clauseType, identificationData, cloneBuffer, null, 0)
        {
        }

        protected BinaryKeyIdentifierClause(string clauseType, byte[] identificationData, bool cloneBuffer, byte[] derivationNonce, int derivationLength) : base(clauseType, derivationNonce, derivationLength)
        {
            if (identificationData == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("identificationData"));
            }
            if (identificationData.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("identificationData", System.IdentityModel.SR.GetString("LengthMustBeGreaterThanZero")));
            }
            if (cloneBuffer)
            {
                this.identificationData = System.IdentityModel.SecurityUtils.CloneBuffer(identificationData);
            }
            else
            {
                this.identificationData = identificationData;
            }
        }

        public byte[] GetBuffer()
        {
            return System.IdentityModel.SecurityUtils.CloneBuffer(this.identificationData);
        }

        protected byte[] GetRawBuffer()
        {
            return this.identificationData;
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            BinaryKeyIdentifierClause objB = keyIdentifierClause as BinaryKeyIdentifierClause;
            return (object.ReferenceEquals(this, objB) || ((objB != null) && objB.Matches(this.identificationData)));
        }

        public bool Matches(byte[] data)
        {
            return this.Matches(data, 0);
        }

        public bool Matches(byte[] data, int offset)
        {
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.IdentityModel.SR.GetString("ValueMustBeNonNegative")));
            }
            return System.IdentityModel.SecurityUtils.MatchesBuffer(this.identificationData, 0, data, offset);
        }

        internal string ToBase64String()
        {
            return Convert.ToBase64String(this.identificationData);
        }

        internal string ToHexString()
        {
            return new SoapHexBinary(this.identificationData).ToString();
        }
    }
}

