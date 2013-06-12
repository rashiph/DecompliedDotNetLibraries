namespace System.Security.Policy
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, ComVisible(true)]
    public sealed class Zone : EvidenceBase, IIdentityPermissionFactory
    {
        [OptionalField(VersionAdded=2)]
        private string m_url;
        private System.Security.SecurityZone m_zone;
        private static readonly string[] s_names = new string[] { "MyComputer", "Intranet", "Trusted", "Internet", "Untrusted", "NoZone" };

        private Zone(Zone zone)
        {
            this.m_url = zone.m_url;
            this.m_zone = zone.m_zone;
        }

        public Zone(System.Security.SecurityZone zone)
        {
            if ((zone < System.Security.SecurityZone.NoZone) || (zone > System.Security.SecurityZone.Untrusted))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IllegalZone"));
            }
            this.m_zone = zone;
        }

        private Zone(string url)
        {
            this.m_url = url;
            this.m_zone = System.Security.SecurityZone.NoZone;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern System.Security.SecurityZone _CreateFromUrl(string url);
        public override EvidenceBase Clone()
        {
            return new Zone(this);
        }

        public object Copy()
        {
            return this.Clone();
        }

        public static Zone CreateFromUrl(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            return new Zone(url);
        }

        public IPermission CreateIdentityPermission(Evidence evidence)
        {
            return new ZoneIdentityPermission(this.SecurityZone);
        }

        public override bool Equals(object o)
        {
            Zone zone = o as Zone;
            if (zone == null)
            {
                return false;
            }
            return (this.SecurityZone == zone.SecurityZone);
        }

        public override int GetHashCode()
        {
            return (int) this.SecurityZone;
        }

        internal object Normalize()
        {
            return s_names[(int) this.SecurityZone];
        }

        public override string ToString()
        {
            return this.ToXml().ToString();
        }

        internal SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("System.Security.Policy.Zone");
            element.AddAttribute("version", "1");
            if (this.SecurityZone != System.Security.SecurityZone.NoZone)
            {
                element.AddChild(new SecurityElement("Zone", s_names[(int) this.SecurityZone]));
                return element;
            }
            element.AddChild(new SecurityElement("Zone", s_names[s_names.Length - 1]));
            return element;
        }

        public System.Security.SecurityZone SecurityZone
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_url != null)
                {
                    this.m_zone = _CreateFromUrl(this.m_url);
                }
                return this.m_zone;
            }
        }
    }
}

