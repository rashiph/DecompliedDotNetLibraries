namespace System.Security.Policy
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class Site : EvidenceBase, IIdentityPermissionFactory
    {
        private SiteString m_name;

        private Site(SiteString name)
        {
            this.m_name = name;
        }

        public Site(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.m_name = new SiteString(name);
        }

        public override EvidenceBase Clone()
        {
            return new Site(this.m_name);
        }

        public object Copy()
        {
            return this.Clone();
        }

        public static Site CreateFromUrl(string url)
        {
            return new Site(ParseSiteFromUrl(url));
        }

        public IPermission CreateIdentityPermission(Evidence evidence)
        {
            return new SiteIdentityPermission(this.Name);
        }

        public override bool Equals(object o)
        {
            Site site = o as Site;
            if (site == null)
            {
                return false;
            }
            return string.Equals(this.Name, site.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        internal SiteString GetSiteString()
        {
            return this.m_name;
        }

        internal object Normalize()
        {
            return this.m_name.ToString().ToUpper(CultureInfo.InvariantCulture);
        }

        private static SiteString ParseSiteFromUrl(string name)
        {
            URLString str = new URLString(name);
            if (string.Compare(str.Scheme, "file", StringComparison.OrdinalIgnoreCase) == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSite"));
            }
            return new SiteString(new URLString(name).Host);
        }

        public override string ToString()
        {
            return this.ToXml().ToString();
        }

        internal SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("System.Security.Policy.Site");
            element.AddAttribute("version", "1");
            if (this.m_name != null)
            {
                element.AddChild(new SecurityElement("Name", this.m_name.ToString()));
            }
            return element;
        }

        public string Name
        {
            get
            {
                return this.m_name.ToString();
            }
        }
    }
}

