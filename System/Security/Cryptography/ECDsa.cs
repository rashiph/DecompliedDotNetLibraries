namespace System.Security.Cryptography
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public abstract class ECDsa : AsymmetricAlgorithm
    {
        protected ECDsa()
        {
        }

        public static ECDsa Create()
        {
            return Create(typeof(ECDsaCng).FullName);
        }

        public static ECDsa Create(string algorithm)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException("algorithm");
            }
            return (CryptoConfig.CreateFromName(algorithm) as ECDsa);
        }

        public abstract byte[] SignHash(byte[] hash);
        public abstract bool VerifyHash(byte[] hash, byte[] signature);

        public override string KeyExchangeAlgorithm
        {
            get
            {
                return null;
            }
        }

        public override string SignatureAlgorithm
        {
            get
            {
                return "ECDsa";
            }
        }
    }
}

