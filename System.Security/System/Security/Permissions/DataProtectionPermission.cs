namespace System.Security.Permissions
{
    using System;
    using System.Globalization;
    using System.Security;

    [Serializable]
    public sealed class DataProtectionPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private DataProtectionPermissionFlags m_flags;

        public DataProtectionPermission(DataProtectionPermissionFlags flag)
        {
            this.Flags = flag;
        }

        public DataProtectionPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.m_flags = DataProtectionPermissionFlags.AllFlags;
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(SecurityResources.GetResourceString("Argument_InvalidPermissionState"));
                }
                this.m_flags = DataProtectionPermissionFlags.NoFlags;
            }
        }

        public override IPermission Copy()
        {
            if (this.Flags == DataProtectionPermissionFlags.NoFlags)
            {
                return null;
            }
            return new DataProtectionPermission(this.m_flags);
        }

        public override void FromXml(SecurityElement securityElement)
        {
            if (securityElement == null)
            {
                throw new ArgumentNullException("securityElement");
            }
            string str = securityElement.Attribute("class");
            if ((str == null) || (str.IndexOf(base.GetType().FullName, StringComparison.Ordinal) == -1))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Argument_InvalidClassAttribute"), "securityElement");
            }
            string strA = securityElement.Attribute("Unrestricted");
            if ((strA != null) && (string.Compare(strA, "true", StringComparison.OrdinalIgnoreCase) == 0))
            {
                this.m_flags = DataProtectionPermissionFlags.AllFlags;
            }
            else
            {
                this.m_flags = DataProtectionPermissionFlags.NoFlags;
                string str3 = securityElement.Attribute("Flags");
                if (str3 != null)
                {
                    DataProtectionPermissionFlags flags = (DataProtectionPermissionFlags) Enum.Parse(typeof(DataProtectionPermissionFlags), str3);
                    VerifyFlags(flags);
                    this.m_flags = flags;
                }
            }
        }

        public override IPermission Intersect(IPermission target)
        {
            IPermission permission2;
            if (target == null)
            {
                return null;
            }
            try
            {
                DataProtectionPermission permission = (DataProtectionPermission) target;
                DataProtectionPermissionFlags flag = permission.m_flags & this.m_flags;
                if (flag == DataProtectionPermissionFlags.NoFlags)
                {
                    return null;
                }
                permission2 = new DataProtectionPermission(flag);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SecurityResources.GetResourceString("Argument_WrongType"), new object[] { base.GetType().FullName }));
            }
            return permission2;
        }

        public override bool IsSubsetOf(IPermission target)
        {
            bool flag;
            if (target == null)
            {
                return (this.m_flags == DataProtectionPermissionFlags.NoFlags);
            }
            try
            {
                DataProtectionPermission permission = (DataProtectionPermission) target;
                DataProtectionPermissionFlags flags = this.m_flags;
                DataProtectionPermissionFlags flags2 = permission.m_flags;
                flag = (flags & flags2) == flags;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SecurityResources.GetResourceString("Argument_WrongType"), new object[] { base.GetType().FullName }));
            }
            return flag;
        }

        public bool IsUnrestricted()
        {
            return (this.m_flags == DataProtectionPermissionFlags.AllFlags);
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("IPermission");
            element.AddAttribute("class", base.GetType().FullName + ", " + base.GetType().Module.Assembly.FullName.Replace('"', '\''));
            element.AddAttribute("version", "1");
            if (!this.IsUnrestricted())
            {
                element.AddAttribute("Flags", this.m_flags.ToString());
                return element;
            }
            element.AddAttribute("Unrestricted", "true");
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            IPermission permission2;
            if (target == null)
            {
                return this.Copy();
            }
            try
            {
                DataProtectionPermission permission = (DataProtectionPermission) target;
                DataProtectionPermissionFlags flag = this.m_flags | permission.m_flags;
                if (flag == DataProtectionPermissionFlags.NoFlags)
                {
                    return null;
                }
                permission2 = new DataProtectionPermission(flag);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SecurityResources.GetResourceString("Argument_WrongType"), new object[] { base.GetType().FullName }));
            }
            return permission2;
        }

        internal static void VerifyFlags(DataProtectionPermissionFlags flags)
        {
            if ((flags & ~DataProtectionPermissionFlags.AllFlags) != DataProtectionPermissionFlags.NoFlags)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SecurityResources.GetResourceString("Arg_EnumIllegalVal"), new object[] { (int) flags }));
            }
        }

        public DataProtectionPermissionFlags Flags
        {
            get
            {
                return this.m_flags;
            }
            set
            {
                VerifyFlags(value);
                this.m_flags = value;
            }
        }
    }
}

