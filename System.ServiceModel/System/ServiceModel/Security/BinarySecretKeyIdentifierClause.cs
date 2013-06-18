namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Tokens;

    public class BinarySecretKeyIdentifierClause : BinaryKeyIdentifierClause
    {
        private InMemorySymmetricSecurityKey symmetricKey;

        public BinarySecretKeyIdentifierClause(byte[] key) : this(key, true)
        {
        }

        public BinarySecretKeyIdentifierClause(byte[] key, bool cloneBuffer) : this(key, cloneBuffer, null, 0)
        {
        }

        public BinarySecretKeyIdentifierClause(byte[] key, bool cloneBuffer, byte[] derivationNonce, int derivationLength) : base(XD.TrustFeb2005Dictionary.BinarySecretClauseType.Value, key, cloneBuffer, derivationNonce, derivationLength)
        {
        }

        public override SecurityKey CreateKey()
        {
            if (this.symmetricKey == null)
            {
                this.symmetricKey = new InMemorySymmetricSecurityKey(base.GetBuffer(), false);
            }
            return this.symmetricKey;
        }

        public byte[] GetKeyBytes()
        {
            return base.GetBuffer();
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            BinarySecretKeyIdentifierClause objB = keyIdentifierClause as BinarySecretKeyIdentifierClause;
            return (object.ReferenceEquals(this, objB) || ((objB != null) && objB.Matches(base.GetRawBuffer())));
        }

        public override bool CanCreateKey
        {
            get
            {
                return true;
            }
        }
    }
}

