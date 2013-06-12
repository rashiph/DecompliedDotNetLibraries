namespace System.Net.NetworkInformation
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public sealed class NetworkInformationPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private NetworkInformationAccess access;
        private bool unrestricted;

        internal NetworkInformationPermission(bool unrestricted)
        {
            if (unrestricted)
            {
                this.access = NetworkInformationAccess.Ping | NetworkInformationAccess.Read;
                unrestricted = true;
            }
            else
            {
                this.access = NetworkInformationAccess.None;
            }
        }

        public NetworkInformationPermission(NetworkInformationAccess access)
        {
            this.access = access;
        }

        public NetworkInformationPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.access = NetworkInformationAccess.Ping | NetworkInformationAccess.Read;
                this.unrestricted = true;
            }
            else
            {
                this.access = NetworkInformationAccess.None;
            }
        }

        public void AddPermission(NetworkInformationAccess access)
        {
            this.access |= access;
        }

        public override IPermission Copy()
        {
            if (this.unrestricted)
            {
                return new NetworkInformationPermission(true);
            }
            return new NetworkInformationPermission(this.access);
        }

        public override void FromXml(SecurityElement securityElement)
        {
            this.access = NetworkInformationAccess.None;
            if (securityElement == null)
            {
                throw new ArgumentNullException("securityElement");
            }
            if (!securityElement.Tag.Equals("IPermission"))
            {
                throw new ArgumentException(SR.GetString("net_not_ipermission"), "securityElement");
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
            if ((strA != null) && (string.Compare(strA, "true", StringComparison.OrdinalIgnoreCase) == 0))
            {
                this.access = NetworkInformationAccess.Ping | NetworkInformationAccess.Read;
                this.unrestricted = true;
            }
            else if (securityElement.Children != null)
            {
                foreach (SecurityElement element in securityElement.Children)
                {
                    strA = element.Attribute("Access");
                    if (string.Compare(strA, "Read", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.access |= NetworkInformationAccess.Read;
                    }
                    else if (string.Compare(strA, "Ping", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.access |= NetworkInformationAccess.Ping;
                    }
                }
            }
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            NetworkInformationPermission permission = target as NetworkInformationPermission;
            if (permission == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_target"), "target");
            }
            if (this.unrestricted && permission.IsUnrestricted())
            {
                return new NetworkInformationPermission(true);
            }
            return new NetworkInformationPermission(this.access & permission.access);
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return (this.access == NetworkInformationAccess.None);
            }
            NetworkInformationPermission permission = target as NetworkInformationPermission;
            if (permission == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_target"), "target");
            }
            if (this.unrestricted && !permission.IsUnrestricted())
            {
                return false;
            }
            return ((this.access & permission.access) == this.access);
        }

        public bool IsUnrestricted()
        {
            return this.unrestricted;
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("IPermission");
            element.AddAttribute("class", base.GetType().FullName + ", " + base.GetType().Module.Assembly.FullName.Replace('"', '\''));
            element.AddAttribute("version", "1");
            if (this.unrestricted)
            {
                element.AddAttribute("Unrestricted", "true");
                return element;
            }
            if ((this.access & NetworkInformationAccess.Read) > NetworkInformationAccess.None)
            {
                SecurityElement child = new SecurityElement("NetworkInformationAccess");
                child.AddAttribute("Access", "Read");
                element.AddChild(child);
            }
            if ((this.access & NetworkInformationAccess.Ping) > NetworkInformationAccess.None)
            {
                SecurityElement element3 = new SecurityElement("NetworkInformationAccess");
                element3.AddAttribute("Access", "Ping");
                element.AddChild(element3);
            }
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                return this.Copy();
            }
            NetworkInformationPermission permission = target as NetworkInformationPermission;
            if (permission == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_target"), "target");
            }
            if (!this.unrestricted && !permission.IsUnrestricted())
            {
                return new NetworkInformationPermission(this.access | permission.access);
            }
            return new NetworkInformationPermission(true);
        }

        public NetworkInformationAccess Access
        {
            get
            {
                return this.access;
            }
        }
    }
}

