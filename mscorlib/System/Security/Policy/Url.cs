namespace System.Security.Policy
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class Url : EvidenceBase, IIdentityPermissionFactory
    {
        private URLString m_url;

        private Url(Url url)
        {
            this.m_url = url.m_url;
        }

        public Url(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.m_url = new URLString(name);
        }

        internal Url(string name, bool parsed)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.m_url = new URLString(name, parsed);
        }

        public override EvidenceBase Clone()
        {
            return new Url(this);
        }

        public object Copy()
        {
            return this.Clone();
        }

        public IPermission CreateIdentityPermission(Evidence evidence)
        {
            return new UrlIdentityPermission(this.m_url);
        }

        public override bool Equals(object o)
        {
            Url url = o as Url;
            if (url == null)
            {
                return false;
            }
            return url.m_url.Equals(this.m_url);
        }

        public override int GetHashCode()
        {
            return this.m_url.GetHashCode();
        }

        internal URLString GetURLString()
        {
            return this.m_url;
        }

        internal object Normalize()
        {
            return this.m_url.NormalizeUrl();
        }

        public override string ToString()
        {
            return this.ToXml().ToString();
        }

        internal SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("System.Security.Policy.Url");
            element.AddAttribute("version", "1");
            if (this.m_url != null)
            {
                element.AddChild(new SecurityElement("Url", this.m_url.ToString()));
            }
            return element;
        }

        public string Value
        {
            get
            {
                return this.m_url.ToString();
            }
        }
    }
}

