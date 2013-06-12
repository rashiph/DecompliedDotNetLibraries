namespace System.Security.Cryptography
{
    using System;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CngAlgorithmGroup : IEquatable<CngAlgorithmGroup>
    {
        private string m_algorithmGroup;
        private static CngAlgorithmGroup s_dh;
        private static CngAlgorithmGroup s_dsa;
        private static CngAlgorithmGroup s_ecdh;
        private static CngAlgorithmGroup s_ecdsa;
        private static CngAlgorithmGroup s_rsa;

        public CngAlgorithmGroup(string algorithmGroup)
        {
            if (algorithmGroup == null)
            {
                throw new ArgumentNullException("algorithmGroup");
            }
            if (algorithmGroup.Length == 0)
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_InvalidAlgorithmGroup", new object[] { algorithmGroup }), "algorithmGroup");
            }
            this.m_algorithmGroup = algorithmGroup;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as CngAlgorithmGroup);
        }

        public bool Equals(CngAlgorithmGroup other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            return this.m_algorithmGroup.Equals(other.AlgorithmGroup);
        }

        public override int GetHashCode()
        {
            return this.m_algorithmGroup.GetHashCode();
        }

        public static bool operator ==(CngAlgorithmGroup left, CngAlgorithmGroup right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(CngAlgorithmGroup left, CngAlgorithmGroup right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return !object.ReferenceEquals(right, null);
            }
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return this.m_algorithmGroup;
        }

        public string AlgorithmGroup
        {
            get
            {
                return this.m_algorithmGroup;
            }
        }

        public static CngAlgorithmGroup DiffieHellman
        {
            get
            {
                if (s_dh == null)
                {
                    s_dh = new CngAlgorithmGroup("DH");
                }
                return s_dh;
            }
        }

        public static CngAlgorithmGroup Dsa
        {
            get
            {
                if (s_dsa == null)
                {
                    s_dsa = new CngAlgorithmGroup("DSA");
                }
                return s_dsa;
            }
        }

        public static CngAlgorithmGroup ECDiffieHellman
        {
            get
            {
                if (s_ecdh == null)
                {
                    s_ecdh = new CngAlgorithmGroup("ECDH");
                }
                return s_ecdh;
            }
        }

        public static CngAlgorithmGroup ECDsa
        {
            get
            {
                if (s_ecdsa == null)
                {
                    s_ecdsa = new CngAlgorithmGroup("ECDSA");
                }
                return s_ecdsa;
            }
        }

        public static CngAlgorithmGroup Rsa
        {
            get
            {
                if (s_rsa == null)
                {
                    s_rsa = new CngAlgorithmGroup("RSA");
                }
                return s_rsa;
            }
        }
    }
}

