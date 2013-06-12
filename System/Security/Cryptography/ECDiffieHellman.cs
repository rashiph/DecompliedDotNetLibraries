namespace System.Security.Cryptography
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public abstract class ECDiffieHellman : AsymmetricAlgorithm
    {
        protected ECDiffieHellman()
        {
        }

        public static ECDiffieHellman Create()
        {
            return Create(typeof(ECDiffieHellmanCng).FullName);
        }

        public static ECDiffieHellman Create(string algorithm)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException("algorithm");
            }
            return (CryptoConfig.CreateFromName(algorithm) as ECDiffieHellman);
        }

        public abstract byte[] DeriveKeyMaterial(ECDiffieHellmanPublicKey otherPartyPublicKey);

        public override string KeyExchangeAlgorithm
        {
            get
            {
                return "ECDiffieHellman";
            }
        }

        public abstract ECDiffieHellmanPublicKey PublicKey { get; }

        public override string SignatureAlgorithm
        {
            get
            {
                return null;
            }
        }
    }
}

