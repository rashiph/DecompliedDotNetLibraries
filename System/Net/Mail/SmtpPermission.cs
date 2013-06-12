namespace System.Net.Mail
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public sealed class SmtpPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private SmtpAccess access;
        private bool unrestricted;

        public SmtpPermission(bool unrestricted)
        {
            if (unrestricted)
            {
                this.access = SmtpAccess.ConnectToUnrestrictedPort;
                this.unrestricted = true;
            }
            else
            {
                this.access = SmtpAccess.None;
            }
        }

        public SmtpPermission(SmtpAccess access)
        {
            this.access = access;
        }

        public SmtpPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.access = SmtpAccess.ConnectToUnrestrictedPort;
                this.unrestricted = true;
            }
            else
            {
                this.access = SmtpAccess.None;
            }
        }

        public void AddPermission(SmtpAccess access)
        {
            if (access > this.access)
            {
                this.access = access;
            }
        }

        public override IPermission Copy()
        {
            if (this.unrestricted)
            {
                return new SmtpPermission(true);
            }
            return new SmtpPermission(this.access);
        }

        public override void FromXml(SecurityElement securityElement)
        {
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
                this.access = SmtpAccess.ConnectToUnrestrictedPort;
                this.unrestricted = true;
            }
            else
            {
                strA = securityElement.Attribute("Access");
                if (strA != null)
                {
                    if (string.Compare(strA, "Connect", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.access = SmtpAccess.Connect;
                    }
                    else if (string.Compare(strA, "ConnectToUnrestrictedPort", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.access = SmtpAccess.ConnectToUnrestrictedPort;
                    }
                    else
                    {
                        if (string.Compare(strA, "None", StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            throw new ArgumentException(SR.GetString("net_perm_invalid_val_in_element"), "Access");
                        }
                        this.access = SmtpAccess.None;
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
            SmtpPermission permission = target as SmtpPermission;
            if (permission == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_target"), "target");
            }
            if (this.IsUnrestricted() && permission.IsUnrestricted())
            {
                return new SmtpPermission(true);
            }
            return new SmtpPermission((this.access < permission.access) ? this.access : permission.access);
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return (this.access == SmtpAccess.None);
            }
            SmtpPermission permission = target as SmtpPermission;
            if (permission == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_target"), "target");
            }
            if (this.unrestricted && !permission.IsUnrestricted())
            {
                return false;
            }
            return (permission.access >= this.access);
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
            if (this.access == SmtpAccess.Connect)
            {
                element.AddAttribute("Access", "Connect");
                return element;
            }
            if (this.access == SmtpAccess.ConnectToUnrestrictedPort)
            {
                element.AddAttribute("Access", "ConnectToUnrestrictedPort");
            }
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                return this.Copy();
            }
            SmtpPermission permission = target as SmtpPermission;
            if (permission == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_target"), "target");
            }
            if (this.unrestricted || permission.IsUnrestricted())
            {
                return new SmtpPermission(true);
            }
            return new SmtpPermission((this.access > permission.access) ? this.access : permission.access);
        }

        public SmtpAccess Access
        {
            get
            {
                return this.access;
            }
        }
    }
}

