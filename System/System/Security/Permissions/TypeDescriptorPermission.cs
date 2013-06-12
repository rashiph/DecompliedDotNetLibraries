namespace System.Security.Permissions
{
    using System;
    using System.Globalization;
    using System.Security;

    [Serializable]
    public sealed class TypeDescriptorPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private TypeDescriptorPermissionFlags m_flags;

        public TypeDescriptorPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.SetUnrestricted(true);
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(SR.GetString("Argument_InvalidPermissionState"));
                }
                this.SetUnrestricted(false);
            }
        }

        public TypeDescriptorPermission(TypeDescriptorPermissionFlags flag)
        {
            this.VerifyAccess(flag);
            this.SetUnrestricted(false);
            this.m_flags = flag;
        }

        public override IPermission Copy()
        {
            return new TypeDescriptorPermission(this.m_flags);
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
                this.m_flags = TypeDescriptorPermissionFlags.RestrictedRegistrationAccess;
            }
            else
            {
                this.m_flags = TypeDescriptorPermissionFlags.NoFlags;
                string str3 = securityElement.Attribute("Flags");
                if (str3 != null)
                {
                    TypeDescriptorPermissionFlags flags = (TypeDescriptorPermissionFlags) Enum.Parse(typeof(TypeDescriptorPermissionFlags), str3);
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
                TypeDescriptorPermission permission = (TypeDescriptorPermission) target;
                TypeDescriptorPermissionFlags flag = permission.m_flags & this.m_flags;
                if (flag == TypeDescriptorPermissionFlags.NoFlags)
                {
                    return null;
                }
                permission2 = new TypeDescriptorPermission(flag);
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
                return (this.m_flags == TypeDescriptorPermissionFlags.NoFlags);
            }
            try
            {
                TypeDescriptorPermission permission = (TypeDescriptorPermission) target;
                TypeDescriptorPermissionFlags flags = this.m_flags;
                TypeDescriptorPermissionFlags flags2 = permission.m_flags;
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
            return (this.m_flags == TypeDescriptorPermissionFlags.RestrictedRegistrationAccess);
        }

        private void Reset()
        {
            this.m_flags = TypeDescriptorPermissionFlags.NoFlags;
        }

        private void SetUnrestricted(bool unrestricted)
        {
            if (unrestricted)
            {
                this.m_flags = TypeDescriptorPermissionFlags.RestrictedRegistrationAccess;
            }
            else
            {
                this.Reset();
            }
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
                TypeDescriptorPermission permission = (TypeDescriptorPermission) target;
                TypeDescriptorPermissionFlags flag = this.m_flags | permission.m_flags;
                if (flag == TypeDescriptorPermissionFlags.NoFlags)
                {
                    return null;
                }
                permission2 = new TypeDescriptorPermission(flag);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Argument_WrongType"), new object[] { base.GetType().FullName }));
            }
            return permission2;
        }

        private void VerifyAccess(TypeDescriptorPermissionFlags type)
        {
            if ((type & ~TypeDescriptorPermissionFlags.RestrictedRegistrationAccess) != TypeDescriptorPermissionFlags.NoFlags)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Arg_EnumIllegalVal"), new object[] { (int) type }));
            }
        }

        internal static void VerifyFlags(TypeDescriptorPermissionFlags flags)
        {
            if ((flags & ~TypeDescriptorPermissionFlags.RestrictedRegistrationAccess) != TypeDescriptorPermissionFlags.NoFlags)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.GetString("Arg_EnumIllegalVal"), new object[] { (int) flags }));
            }
        }

        public TypeDescriptorPermissionFlags Flags
        {
            get
            {
                return this.m_flags;
            }
            set
            {
                this.VerifyAccess(value);
                this.m_flags = value;
            }
        }
    }
}

