namespace System.Security.Cryptography
{
    using System;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CngAlgorithm : IEquatable<CngAlgorithm>
    {
        private string m_algorithm;
        private static CngAlgorithm s_ecdhp256;
        private static CngAlgorithm s_ecdhp384;
        private static CngAlgorithm s_ecdhp521;
        private static CngAlgorithm s_ecdsap256;
        private static CngAlgorithm s_ecdsap384;
        private static CngAlgorithm s_ecdsap521;
        private static CngAlgorithm s_md5;
        private static CngAlgorithm s_sha1;
        private static CngAlgorithm s_sha256;
        private static CngAlgorithm s_sha384;
        private static CngAlgorithm s_sha512;

        public CngAlgorithm(string algorithm)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException("algorithm");
            }
            if (algorithm.Length == 0)
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_InvalidAlgorithmName", new object[] { algorithm }), "algorithm");
            }
            this.m_algorithm = algorithm;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as CngAlgorithm);
        }

        public bool Equals(CngAlgorithm other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            return this.m_algorithm.Equals(other.Algorithm);
        }

        public override int GetHashCode()
        {
            return this.m_algorithm.GetHashCode();
        }

        public static bool operator ==(CngAlgorithm left, CngAlgorithm right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(CngAlgorithm left, CngAlgorithm right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return !object.ReferenceEquals(right, null);
            }
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return this.m_algorithm;
        }

        public string Algorithm
        {
            get
            {
                return this.m_algorithm;
            }
        }

        public static CngAlgorithm ECDiffieHellmanP256
        {
            get
            {
                if (s_ecdhp256 == null)
                {
                    s_ecdhp256 = new CngAlgorithm("ECDH_P256");
                }
                return s_ecdhp256;
            }
        }

        public static CngAlgorithm ECDiffieHellmanP384
        {
            get
            {
                if (s_ecdhp384 == null)
                {
                    s_ecdhp384 = new CngAlgorithm("ECDH_P384");
                }
                return s_ecdhp384;
            }
        }

        public static CngAlgorithm ECDiffieHellmanP521
        {
            get
            {
                if (s_ecdhp521 == null)
                {
                    s_ecdhp521 = new CngAlgorithm("ECDH_P521");
                }
                return s_ecdhp521;
            }
        }

        public static CngAlgorithm ECDsaP256
        {
            get
            {
                if (s_ecdsap256 == null)
                {
                    s_ecdsap256 = new CngAlgorithm("ECDSA_P256");
                }
                return s_ecdsap256;
            }
        }

        public static CngAlgorithm ECDsaP384
        {
            get
            {
                if (s_ecdsap384 == null)
                {
                    s_ecdsap384 = new CngAlgorithm("ECDSA_P384");
                }
                return s_ecdsap384;
            }
        }

        public static CngAlgorithm ECDsaP521
        {
            get
            {
                if (s_ecdsap521 == null)
                {
                    s_ecdsap521 = new CngAlgorithm("ECDSA_P521");
                }
                return s_ecdsap521;
            }
        }

        public static CngAlgorithm MD5
        {
            get
            {
                if (s_md5 == null)
                {
                    s_md5 = new CngAlgorithm("MD5");
                }
                return s_md5;
            }
        }

        public static CngAlgorithm Sha1
        {
            get
            {
                if (s_sha1 == null)
                {
                    s_sha1 = new CngAlgorithm("SHA1");
                }
                return s_sha1;
            }
        }

        public static CngAlgorithm Sha256
        {
            get
            {
                if (s_sha256 == null)
                {
                    s_sha256 = new CngAlgorithm("SHA256");
                }
                return s_sha256;
            }
        }

        public static CngAlgorithm Sha384
        {
            get
            {
                if (s_sha384 == null)
                {
                    s_sha384 = new CngAlgorithm("SHA384");
                }
                return s_sha384;
            }
        }

        public static CngAlgorithm Sha512
        {
            get
            {
                if (s_sha512 == null)
                {
                    s_sha512 = new CngAlgorithm("SHA512");
                }
                return s_sha512;
            }
        }
    }
}

