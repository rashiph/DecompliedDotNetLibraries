namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    public class BinarySecretSecurityToken : SecurityToken
    {
        private DateTime effectiveTime;
        private string id;
        private byte[] key;
        private ReadOnlyCollection<SecurityKey> securityKeys;

        public BinarySecretSecurityToken(int keySizeInBits) : this(SecurityUniqueId.Create().Value, keySizeInBits)
        {
        }

        public BinarySecretSecurityToken(byte[] key) : this(SecurityUniqueId.Create().Value, key)
        {
        }

        public BinarySecretSecurityToken(string id, int keySizeInBits) : this(id, keySizeInBits, true)
        {
        }

        public BinarySecretSecurityToken(string id, byte[] key) : this(id, key, true)
        {
        }

        protected BinarySecretSecurityToken(string id, int keySizeInBits, bool allowCrypto)
        {
            if ((keySizeInBits <= 0) || (keySizeInBits >= 0x200))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("keySizeInBits", System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, 0x200 })));
            }
            if ((keySizeInBits % 8) != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("keySizeInBits", System.ServiceModel.SR.GetString("KeyLengthMustBeMultipleOfEight", new object[] { keySizeInBits })));
            }
            this.id = id;
            this.effectiveTime = DateTime.UtcNow;
            this.key = new byte[keySizeInBits / 8];
            CryptoHelper.FillRandomBytes(this.key);
            if (allowCrypto)
            {
                this.securityKeys = System.ServiceModel.Security.SecurityUtils.CreateSymmetricSecurityKeys(this.key);
            }
            else
            {
                this.securityKeys = EmptyReadOnlyCollection<SecurityKey>.Instance;
            }
        }

        protected BinarySecretSecurityToken(string id, byte[] key, bool allowCrypto)
        {
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }
            this.id = id;
            this.effectiveTime = DateTime.UtcNow;
            this.key = new byte[key.Length];
            Buffer.BlockCopy(key, 0, this.key, 0, key.Length);
            if (allowCrypto)
            {
                this.securityKeys = System.ServiceModel.Security.SecurityUtils.CreateSymmetricSecurityKeys(this.key);
            }
            else
            {
                this.securityKeys = EmptyReadOnlyCollection<SecurityKey>.Instance;
            }
        }

        public byte[] GetKeyBytes()
        {
            return System.ServiceModel.Security.SecurityUtils.CloneBuffer(this.key);
        }

        public override string Id
        {
            get
            {
                return this.id;
            }
        }

        public int KeySize
        {
            get
            {
                return (this.key.Length * 8);
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                return this.securityKeys;
            }
        }

        public override DateTime ValidFrom
        {
            get
            {
                return this.effectiveTime;
            }
        }

        public override DateTime ValidTo
        {
            get
            {
                return DateTime.MaxValue;
            }
        }
    }
}

