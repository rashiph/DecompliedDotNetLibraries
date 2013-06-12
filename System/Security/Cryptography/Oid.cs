namespace System.Security.Cryptography
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    public sealed class Oid
    {
        private string m_friendlyName;
        private System.Security.Cryptography.OidGroup m_group;
        private string m_value;

        public Oid()
        {
        }

        public Oid(Oid oid)
        {
            if (oid == null)
            {
                throw new ArgumentNullException("oid");
            }
            this.m_value = oid.m_value;
            this.m_friendlyName = oid.m_friendlyName;
            this.m_group = oid.m_group;
        }

        public Oid(string oid) : this(oid, System.Security.Cryptography.OidGroup.AllGroups, true)
        {
        }

        public Oid(string value, string friendlyName)
        {
            this.m_value = value;
            this.m_friendlyName = friendlyName;
        }

        internal Oid(string oid, System.Security.Cryptography.OidGroup group, bool lookupFriendlyName)
        {
            if (lookupFriendlyName)
            {
                string str = System.Security.Cryptography.X509Certificates.X509Utils.FindOidInfo(2, oid, group);
                if (str == null)
                {
                    str = oid;
                }
                this.Value = str;
            }
            else
            {
                this.Value = oid;
            }
            this.m_group = group;
        }

        public string FriendlyName
        {
            get
            {
                if ((this.m_friendlyName == null) && (this.m_value != null))
                {
                    this.m_friendlyName = System.Security.Cryptography.X509Certificates.X509Utils.FindOidInfo(1, this.m_value, this.m_group);
                }
                return this.m_friendlyName;
            }
            set
            {
                this.m_friendlyName = value;
                if (this.m_friendlyName != null)
                {
                    string str = System.Security.Cryptography.X509Certificates.X509Utils.FindOidInfo(2, this.m_friendlyName, this.m_group);
                    if (str != null)
                    {
                        this.m_value = str;
                    }
                }
            }
        }

        public string Value
        {
            get
            {
                return this.m_value;
            }
            set
            {
                this.m_value = value;
            }
        }
    }
}

