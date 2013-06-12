namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class ReflectionPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission
    {
        internal const ReflectionPermissionFlag AllFlagsAndMore = (ReflectionPermissionFlag.RestrictedMemberAccess | ReflectionPermissionFlag.AllFlags);
        private ReflectionPermissionFlag m_flags;

        public ReflectionPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.SetUnrestricted(true);
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPermissionState"));
                }
                this.SetUnrestricted(false);
            }
        }

        public ReflectionPermission(ReflectionPermissionFlag flag)
        {
            this.VerifyAccess(flag);
            this.SetUnrestricted(false);
            this.m_flags = flag;
        }

        public override IPermission Copy()
        {
            if (this.IsUnrestricted())
            {
                return new ReflectionPermission(PermissionState.Unrestricted);
            }
            return new ReflectionPermission(this.m_flags);
        }

        public override void FromXml(SecurityElement esd)
        {
            CodeAccessPermission.ValidateElement(esd, this);
            if (XMLUtil.IsUnrestricted(esd))
            {
                this.m_flags = ReflectionPermissionFlag.RestrictedMemberAccess | ReflectionPermissionFlag.AllFlags;
            }
            else
            {
                this.Reset();
                this.SetUnrestricted(false);
                string str = esd.Attribute("Flags");
                if (str != null)
                {
                    this.m_flags = (ReflectionPermissionFlag) Enum.Parse(typeof(ReflectionPermissionFlag), str);
                }
            }
        }

        internal static int GetTokenIndex()
        {
            return 4;
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            if (!base.VerifyType(target))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            ReflectionPermission permission = (ReflectionPermission) target;
            ReflectionPermissionFlag flag = permission.m_flags & this.m_flags;
            if (flag == ReflectionPermissionFlag.NoFlags)
            {
                return null;
            }
            return new ReflectionPermission(flag);
        }

        public override bool IsSubsetOf(IPermission target)
        {
            bool flag;
            if (target == null)
            {
                return (this.m_flags == ReflectionPermissionFlag.NoFlags);
            }
            try
            {
                ReflectionPermission permission = (ReflectionPermission) target;
                if (permission.IsUnrestricted())
                {
                    return true;
                }
                if (this.IsUnrestricted())
                {
                    return false;
                }
                flag = (this.m_flags & ~permission.m_flags) == ReflectionPermissionFlag.NoFlags;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            return flag;
        }

        public bool IsUnrestricted()
        {
            return (this.m_flags == (ReflectionPermissionFlag.RestrictedMemberAccess | ReflectionPermissionFlag.AllFlags));
        }

        private void Reset()
        {
            this.m_flags = ReflectionPermissionFlag.NoFlags;
        }

        private void SetUnrestricted(bool unrestricted)
        {
            if (unrestricted)
            {
                this.m_flags = ReflectionPermissionFlag.RestrictedMemberAccess | ReflectionPermissionFlag.AllFlags;
            }
            else
            {
                this.Reset();
            }
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.ReflectionPermission");
            if (!this.IsUnrestricted())
            {
                element.AddAttribute("Flags", XMLUtil.BitFieldEnumToString(typeof(ReflectionPermissionFlag), this.m_flags));
                return element;
            }
            element.AddAttribute("Unrestricted", "true");
            return element;
        }

        public override IPermission Union(IPermission other)
        {
            if (other == null)
            {
                return this.Copy();
            }
            if (!base.VerifyType(other))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            ReflectionPermission permission = (ReflectionPermission) other;
            if (this.IsUnrestricted() || permission.IsUnrestricted())
            {
                return new ReflectionPermission(PermissionState.Unrestricted);
            }
            return new ReflectionPermission(this.m_flags | permission.m_flags);
        }

        private void VerifyAccess(ReflectionPermissionFlag type)
        {
            if ((type & ~(ReflectionPermissionFlag.RestrictedMemberAccess | ReflectionPermissionFlag.AllFlags)) != ReflectionPermissionFlag.NoFlags)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) type }));
            }
        }

        public ReflectionPermissionFlag Flags
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

