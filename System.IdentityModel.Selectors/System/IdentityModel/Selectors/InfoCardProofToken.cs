namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;
    using System.Runtime;

    internal class InfoCardProofToken : SecurityToken, IDisposable
    {
        private DateTime m_expiration;
        private string m_id;
        private SecurityKey m_securityKey;
        private ReadOnlyCollection<SecurityKey> m_securityKeys;

        private InfoCardProofToken(DateTime expiration)
        {
            this.m_id = Guid.NewGuid().ToString();
            this.m_expiration = expiration.ToUniversalTime();
        }

        public InfoCardProofToken(AsymmetricCryptoHandle cryptoHandle, DateTime expiration) : this(expiration)
        {
            this.InitCrypto(new InfoCardAsymmetricCrypto(cryptoHandle));
        }

        public InfoCardProofToken(SymmetricCryptoHandle cryptoHandle, DateTime expiration) : this(expiration)
        {
            this.InitCrypto(new InfoCardSymmetricCrypto(cryptoHandle));
        }

        public void Dispose()
        {
            this.m_securityKeys = null;
            ((IDisposable) this.m_securityKey).Dispose();
        }

        private void InitCrypto(SecurityKey securityKey)
        {
            this.m_securityKey = securityKey;
            this.m_securityKeys = new List<SecurityKey>(1) { securityKey }.AsReadOnly();
        }

        public override string Id
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_id;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_securityKeys;
            }
        }

        public override DateTime ValidFrom
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        public override DateTime ValidTo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_expiration;
            }
        }
    }
}

