namespace System.Net
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public sealed class DnsPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private bool m_noRestriction;

        internal DnsPermission(bool free)
        {
            this.m_noRestriction = free;
        }

        public DnsPermission(PermissionState state)
        {
            this.m_noRestriction = state == PermissionState.Unrestricted;
        }

        public override IPermission Copy()
        {
            return new DnsPermission(this.m_noRestriction);
        }

        public override void FromXml(SecurityElement securityElement)
        {
            if (securityElement == null)
            {
                throw new ArgumentNullException("securityElement");
            }
            if (!securityElement.Tag.Equals("IPermission"))
            {
                throw new ArgumentException(SR.GetString("net_no_classname"), "securityElement");
            }
            string str = securityElement.Attribute("class");
            if (str == null)
            {
                throw new ArgumentException(SR.GetString("net_no_classname"), "securityElement");
            }
            if (str.IndexOf(base.GetType().FullName) < 0)
            {
                throw new ArgumentException(SR.GetString("net_no_typename"), "securityElement");
            }
            string strA = securityElement.Attribute("Unrestricted");
            this.m_noRestriction = (strA != null) ? (0 == string.Compare(strA, "true", StringComparison.OrdinalIgnoreCase)) : false;
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target != null)
            {
                DnsPermission permission = target as DnsPermission;
                if (permission == null)
                {
                    throw new ArgumentException(SR.GetString("net_perm_target"), "target");
                }
                if (this.m_noRestriction && permission.m_noRestriction)
                {
                    return new DnsPermission(true);
                }
            }
            return null;
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return !this.m_noRestriction;
            }
            DnsPermission permission = target as DnsPermission;
            if (permission == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_target"), "target");
            }
            if (this.m_noRestriction)
            {
                return permission.m_noRestriction;
            }
            return true;
        }

        public bool IsUnrestricted()
        {
            return this.m_noRestriction;
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("IPermission");
            element.AddAttribute("class", base.GetType().FullName + ", " + base.GetType().Module.Assembly.FullName.Replace('"', '\''));
            element.AddAttribute("version", "1");
            if (this.m_noRestriction)
            {
                element.AddAttribute("Unrestricted", "true");
            }
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                return this.Copy();
            }
            DnsPermission permission = target as DnsPermission;
            if (permission == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_target"), "target");
            }
            return new DnsPermission(this.m_noRestriction || permission.m_noRestriction);
        }
    }
}

