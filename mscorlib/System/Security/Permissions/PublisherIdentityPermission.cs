namespace System.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class PublisherIdentityPermission : CodeAccessPermission, IBuiltInPermission
    {
        private X509Certificate[] m_certs;
        private bool m_unrestricted;

        public PublisherIdentityPermission(X509Certificate certificate)
        {
            this.Certificate = certificate;
        }

        public PublisherIdentityPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.m_unrestricted = true;
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPermissionState"));
                }
                this.m_unrestricted = false;
            }
        }

        private static void CheckCertificate(X509Certificate certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }
            if (certificate.GetRawCertData() == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_UninitializedCertificate"));
            }
        }

        public override IPermission Copy()
        {
            PublisherIdentityPermission permission = new PublisherIdentityPermission(PermissionState.None) {
                m_unrestricted = this.m_unrestricted
            };
            if (this.m_certs != null)
            {
                permission.m_certs = new X509Certificate[this.m_certs.Length];
                for (int i = 0; i < this.m_certs.Length; i++)
                {
                    permission.m_certs[i] = (this.m_certs[i] == null) ? null : new X509Certificate(this.m_certs[i]);
                }
            }
            return permission;
        }

        public override void FromXml(SecurityElement esd)
        {
            this.m_unrestricted = false;
            this.m_certs = null;
            CodeAccessPermission.ValidateElement(esd, this);
            string strA = esd.Attribute("Unrestricted");
            if ((strA != null) && (string.Compare(strA, "true", StringComparison.OrdinalIgnoreCase) == 0))
            {
                this.m_unrestricted = true;
            }
            else
            {
                string hexString = esd.Attribute("X509v3Certificate");
                ArrayList list = new ArrayList();
                if (hexString != null)
                {
                    list.Add(new X509Certificate(Hex.DecodeHexString(hexString)));
                }
                ArrayList children = esd.Children;
                if (children != null)
                {
                    foreach (SecurityElement element in children)
                    {
                        hexString = element.Attribute("X509v3Certificate");
                        if (hexString != null)
                        {
                            list.Add(new X509Certificate(Hex.DecodeHexString(hexString)));
                        }
                    }
                }
                if (list.Count != 0)
                {
                    this.m_certs = (X509Certificate[]) list.ToArray(typeof(X509Certificate));
                }
            }
        }

        internal static int GetTokenIndex()
        {
            return 10;
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            PublisherIdentityPermission permission = target as PublisherIdentityPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            if (this.m_unrestricted && permission.m_unrestricted)
            {
                return new PublisherIdentityPermission(PermissionState.None) { m_unrestricted = true };
            }
            if (this.m_unrestricted)
            {
                return permission.Copy();
            }
            if (permission.m_unrestricted)
            {
                return this.Copy();
            }
            if (((this.m_certs == null) || (permission.m_certs == null)) || ((this.m_certs.Length == 0) || (permission.m_certs.Length == 0)))
            {
                return null;
            }
            ArrayList list = new ArrayList();
            foreach (X509Certificate certificate in this.m_certs)
            {
                foreach (X509Certificate certificate2 in permission.m_certs)
                {
                    if (certificate.Equals(certificate2))
                    {
                        list.Add(new X509Certificate(certificate));
                    }
                }
            }
            if (list.Count == 0)
            {
                return null;
            }
            return new PublisherIdentityPermission(PermissionState.None) { m_certs = (X509Certificate[]) list.ToArray(typeof(X509Certificate)) };
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                if (!this.m_unrestricted)
                {
                    if (this.m_certs == null)
                    {
                        return true;
                    }
                    if (this.m_certs.Length == 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            PublisherIdentityPermission permission = target as PublisherIdentityPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            if (!permission.m_unrestricted)
            {
                if (this.m_unrestricted)
                {
                    return false;
                }
                if (this.m_certs != null)
                {
                    foreach (X509Certificate certificate in this.m_certs)
                    {
                        bool flag = false;
                        if (permission.m_certs != null)
                        {
                            foreach (X509Certificate certificate2 in permission.m_certs)
                            {
                                if (certificate.Equals(certificate2))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (!flag)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.PublisherIdentityPermission");
            if (this.m_unrestricted)
            {
                element.AddAttribute("Unrestricted", "true");
                return element;
            }
            if (this.m_certs != null)
            {
                if (this.m_certs.Length == 1)
                {
                    element.AddAttribute("X509v3Certificate", this.m_certs[0].GetRawCertDataString());
                    return element;
                }
                for (int i = 0; i < this.m_certs.Length; i++)
                {
                    SecurityElement child = new SecurityElement("Cert");
                    child.AddAttribute("X509v3Certificate", this.m_certs[i].GetRawCertDataString());
                    element.AddChild(child);
                }
            }
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                if (((this.m_certs == null) || (this.m_certs.Length == 0)) && !this.m_unrestricted)
                {
                    return null;
                }
                return this.Copy();
            }
            PublisherIdentityPermission permission = target as PublisherIdentityPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            if (this.m_unrestricted || permission.m_unrestricted)
            {
                return new PublisherIdentityPermission(PermissionState.None) { m_unrestricted = true };
            }
            if ((this.m_certs == null) || (this.m_certs.Length == 0))
            {
                if ((permission.m_certs != null) && (permission.m_certs.Length != 0))
                {
                    return permission.Copy();
                }
                return null;
            }
            if ((permission.m_certs == null) || (permission.m_certs.Length == 0))
            {
                return this.Copy();
            }
            ArrayList list = new ArrayList();
            foreach (X509Certificate certificate in this.m_certs)
            {
                list.Add(certificate);
            }
            foreach (X509Certificate certificate2 in permission.m_certs)
            {
                bool flag = false;
                foreach (X509Certificate certificate3 in list)
                {
                    if (certificate2.Equals(certificate3))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    list.Add(certificate2);
                }
            }
            return new PublisherIdentityPermission(PermissionState.None) { m_certs = (X509Certificate[]) list.ToArray(typeof(X509Certificate)) };
        }

        public X509Certificate Certificate
        {
            get
            {
                if ((this.m_certs == null) || (this.m_certs.Length < 1))
                {
                    return null;
                }
                if (this.m_certs.Length > 1)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_AmbiguousIdentity"));
                }
                if (this.m_certs[0] == null)
                {
                    return null;
                }
                return new X509Certificate(this.m_certs[0]);
            }
            set
            {
                CheckCertificate(value);
                this.m_unrestricted = false;
                this.m_certs = new X509Certificate[] { new X509Certificate(value) };
            }
        }
    }
}

