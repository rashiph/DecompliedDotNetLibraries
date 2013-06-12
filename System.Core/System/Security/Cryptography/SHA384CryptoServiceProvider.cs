namespace System.Security.Cryptography
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class SHA384CryptoServiceProvider : SHA384
    {
        private CapiHashAlgorithm m_hashAlgorithm = new CapiHashAlgorithm("Microsoft Enhanced RSA and AES Cryptographic Provider", CapiNative.ProviderType.RsaAes, CapiNative.AlgorithmId.Sha384);

        [SecurityCritical]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.m_hashAlgorithm.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        [SecurityCritical]
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            this.m_hashAlgorithm.HashCore(array, ibStart, cbSize);
        }

        [SecurityCritical]
        protected override byte[] HashFinal()
        {
            return this.m_hashAlgorithm.HashFinal();
        }

        [SecurityCritical]
        public override void Initialize()
        {
            this.m_hashAlgorithm.Initialize();
        }
    }
}

