namespace System.Security.Policy
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    [Serializable, ComVisible(true)]
    public sealed class Publisher : EvidenceBase, IIdentityPermissionFactory
    {
        private X509Certificate m_cert;

        public Publisher(X509Certificate cert)
        {
            if (cert == null)
            {
                throw new ArgumentNullException("cert");
            }
            this.m_cert = cert;
        }

        public override EvidenceBase Clone()
        {
            return new Publisher(this.m_cert);
        }

        public object Copy()
        {
            return this.Clone();
        }

        public IPermission CreateIdentityPermission(Evidence evidence)
        {
            return new PublisherIdentityPermission(this.m_cert);
        }

        public override bool Equals(object o)
        {
            Publisher publisher = o as Publisher;
            return ((publisher != null) && PublicKeyEquals(this.m_cert, publisher.m_cert));
        }

        public override int GetHashCode()
        {
            return this.m_cert.GetHashCode();
        }

        internal object Normalize()
        {
            return new MemoryStream(this.m_cert.GetRawCertData()) { Position = 0L };
        }

        internal static bool PublicKeyEquals(X509Certificate cert1, X509Certificate cert2)
        {
            if (cert1 == null)
            {
                return (cert2 == null);
            }
            if (cert2 == null)
            {
                return false;
            }
            byte[] publicKey = cert1.GetPublicKey();
            string keyAlgorithm = cert1.GetKeyAlgorithm();
            byte[] keyAlgorithmParameters = cert1.GetKeyAlgorithmParameters();
            byte[] buffer3 = cert2.GetPublicKey();
            string str2 = cert2.GetKeyAlgorithm();
            byte[] buffer4 = cert2.GetKeyAlgorithmParameters();
            int length = publicKey.Length;
            if (length != buffer3.Length)
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (publicKey[i] != buffer3[i])
                {
                    return false;
                }
            }
            if (!keyAlgorithm.Equals(str2))
            {
                return false;
            }
            length = keyAlgorithmParameters.Length;
            if (buffer4.Length != length)
            {
                return false;
            }
            for (int j = 0; j < length; j++)
            {
                if (keyAlgorithmParameters[j] != buffer4[j])
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            return this.ToXml().ToString();
        }

        internal SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("System.Security.Policy.Publisher");
            element.AddAttribute("version", "1");
            element.AddChild(new SecurityElement("X509v3Certificate", (this.m_cert != null) ? this.m_cert.GetRawCertDataString() : ""));
            return element;
        }

        public X509Certificate Certificate
        {
            get
            {
                return new X509Certificate(this.m_cert);
            }
        }
    }
}

