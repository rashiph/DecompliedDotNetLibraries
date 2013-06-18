namespace System.IdentityModel.Tokens
{
    using System;
    using System.Globalization;
    using System.IdentityModel;

    public sealed class EncryptedKeyIdentifierClause : BinaryKeyIdentifierClause
    {
        private readonly string carriedKeyName;
        private readonly SecurityKeyIdentifier encryptingKeyIdentifier;
        private readonly string encryptionMethod;

        public EncryptedKeyIdentifierClause(byte[] encryptedKey, string encryptionMethod) : this(encryptedKey, encryptionMethod, null)
        {
        }

        public EncryptedKeyIdentifierClause(byte[] encryptedKey, string encryptionMethod, SecurityKeyIdentifier encryptingKeyIdentifier) : this(encryptedKey, encryptionMethod, encryptingKeyIdentifier, null)
        {
        }

        public EncryptedKeyIdentifierClause(byte[] encryptedKey, string encryptionMethod, SecurityKeyIdentifier encryptingKeyIdentifier, string carriedKeyName) : this(encryptedKey, encryptionMethod, encryptingKeyIdentifier, carriedKeyName, true, null, 0)
        {
        }

        public EncryptedKeyIdentifierClause(byte[] encryptedKey, string encryptionMethod, SecurityKeyIdentifier encryptingKeyIdentifier, string carriedKeyName, byte[] derivationNonce, int derivationLength) : this(encryptedKey, encryptionMethod, encryptingKeyIdentifier, carriedKeyName, true, derivationNonce, derivationLength)
        {
        }

        internal EncryptedKeyIdentifierClause(byte[] encryptedKey, string encryptionMethod, SecurityKeyIdentifier encryptingKeyIdentifier, string carriedKeyName, bool cloneBuffer, byte[] derivationNonce, int derivationLength) : base("http://www.w3.org/2001/04/xmlenc#EncryptedKey", encryptedKey, cloneBuffer, derivationNonce, derivationLength)
        {
            if (encryptionMethod == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encryptionMethod");
            }
            this.carriedKeyName = carriedKeyName;
            this.encryptionMethod = encryptionMethod;
            this.encryptingKeyIdentifier = encryptingKeyIdentifier;
        }

        public byte[] GetEncryptedKey()
        {
            return base.GetBuffer();
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            EncryptedKeyIdentifierClause objB = keyIdentifierClause as EncryptedKeyIdentifierClause;
            return (object.ReferenceEquals(this, objB) || ((objB != null) && objB.Matches(base.GetRawBuffer(), this.encryptionMethod, this.carriedKeyName)));
        }

        public bool Matches(byte[] encryptedKey, string encryptionMethod, string carriedKeyName)
        {
            return ((base.Matches(encryptedKey) && (this.encryptionMethod == encryptionMethod)) && (this.carriedKeyName == carriedKeyName));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "EncryptedKeyIdentifierClause(EncryptedKey = {0}, Method '{1}')", new object[] { Convert.ToBase64String(base.GetRawBuffer()), this.EncryptionMethod });
        }

        public string CarriedKeyName
        {
            get
            {
                return this.carriedKeyName;
            }
        }

        public SecurityKeyIdentifier EncryptingKeyIdentifier
        {
            get
            {
                return this.encryptingKeyIdentifier;
            }
        }

        public string EncryptionMethod
        {
            get
            {
                return this.encryptionMethod;
            }
        }
    }
}

