namespace System.Security.Cryptography
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class MD5Cng : MD5
    {
        private BCryptHashAlgorithm m_hashAlgorithm;

        [SecurityCritical]
        public MD5Cng()
        {
            if (CryptoConfig.AllowOnlyFipsAlgorithms)
            {
                throw new InvalidOperationException(System.SR.GetString("Cryptography_NonCompliantFIPSAlgorithm"));
            }
            this.m_hashAlgorithm = new BCryptHashAlgorithm(CngAlgorithm.MD5, "Microsoft Primitive Provider");
        }

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

