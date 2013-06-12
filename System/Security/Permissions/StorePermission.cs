namespace System.Security.Permissions
{
    using System;
    using System.Globalization;
    using System.Security;

    [Serializable]
    public sealed class StorePermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private StorePermissionFlags m_flags;

        public StorePermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.m_flags = StorePermissionFlags.AllFlags;
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(SR.GetString("Argument_InvalidPermissionState"));
                }
                this.m_flags = StorePermissionFlags.NoFlags;
            }
        }

        public StorePermission(StorePermissionFlags flag)
        {
            VerifyFlags(flag);
            this.m_flags = flag;
        }

        public override IPermission Copy()
        {
            return new StorePermission(this.m_flags);
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
                throw new ArgumentException(SR.GetString("Argument_InvalidClassAttribute"), "securityElement");
            }
            string strA = securityElement.Attribute("Unrestricted");
            if ((strA != null) && (string.Compare(strA, "true", StringComparison.OrdinalIgnoreCase) == 0))
            {
                this.m_flags = StorePermissionFlags.AllFlags;
            }
            else
            {
                this.m_flags = StorePermissionFlags.NoFlags;
                string str3 = securityElement.Attribute("Flags");
                if (str3 != null)
                {
                    StorePermissionFlags flags = (StorePermissionFlags) Enum.Parse(typeof(StorePermissionFlags), str3);
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
                StorePermission permission = (StorePermission) target;
                StorePermissionFlags flag = permission.m_flags & this.m_flags;
                if (flag == StorePermissionFlags.NoFlags)
                {
                    return null;
                }
                permission2 = new StorePermission(flag);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Argument_WrongType"), new object[] { base.GetType().FullName }));
            }
            return permission2;
        }

        public override bool IsSubsetOf(IPermission target)
        {
            bool flag;
            if (target == null)
            {
                return (this.m_flags == StorePermissionFlags.NoFlags);
            }
            try
            {
                StorePermission permission = (StorePermission) target;
                StorePermissionFlags flags = this.m_flags;
                StorePermissionFlags flags2 = permission.m_flags;
                flag = (flags & flags2) == flags;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Argument_WrongType"), new object[] { base.GetType().FullName }));
            }
            return flag;
        }

        public bool IsUnrestricted()
        {
            return (this.m_flags == StorePermissionFlags.AllFlags);
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
                StorePermission permission = (StorePermission) target;
                StorePermissionFlags flag = this.m_flags | permission.m_flags;
                if (flag == StorePermissionFlags.NoFlags)
                {
                    return null;
                }
                permission2 = new StorePermission(flag);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Argument_WrongType"), new object[] { base.GetType().FullName }));
            }
            return permission2;
        }

        internal static void VerifyFlags(StorePermissionFlags flags)
        {
            if ((flags & ~StorePermissionFlags.AllFlags) != StorePermissionFlags.NoFlags)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Arg_EnumIllegalVal"), new object[] { (int) flags }));
            }
        }

        public StorePermissionFlags Flags
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

