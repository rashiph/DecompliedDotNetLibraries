namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class EnvironmentPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission
    {
        private StringExpressionSet m_read;
        private bool m_unrestricted;
        private StringExpressionSet m_write;

        public EnvironmentPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.m_unrestricted = true;
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPermissionState"));
                }
                this.m_unrestricted = false;
            }
        }

        [SecuritySafeCritical]
        public EnvironmentPermission(EnvironmentPermissionAccess flag, string pathList)
        {
            this.SetPathList(flag, pathList);
        }

        [SecuritySafeCritical]
        public void AddPathList(EnvironmentPermissionAccess flag, string pathList)
        {
            this.VerifyFlag(flag);
            if (this.FlagIsSet(flag, EnvironmentPermissionAccess.Read))
            {
                if (this.m_read == null)
                {
                    this.m_read = new EnvironmentStringExpressionSet();
                }
                this.m_read.AddExpressions(pathList);
            }
            if (this.FlagIsSet(flag, EnvironmentPermissionAccess.Write))
            {
                if (this.m_write == null)
                {
                    this.m_write = new EnvironmentStringExpressionSet();
                }
                this.m_write.AddExpressions(pathList);
            }
        }

        public override IPermission Copy()
        {
            EnvironmentPermission permission = new EnvironmentPermission(PermissionState.None);
            if (this.m_unrestricted)
            {
                permission.m_unrestricted = true;
                return permission;
            }
            permission.m_unrestricted = false;
            if (this.m_read != null)
            {
                permission.m_read = this.m_read.Copy();
            }
            if (this.m_write != null)
            {
                permission.m_write = this.m_write.Copy();
            }
            return permission;
        }

        private void ExclusiveFlag(EnvironmentPermissionAccess flag)
        {
            if (flag == EnvironmentPermissionAccess.NoAccess)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumNotSingleFlag"));
            }
            if ((flag & (flag - 1)) != EnvironmentPermissionAccess.NoAccess)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumNotSingleFlag"));
            }
        }

        private bool FlagIsSet(EnvironmentPermissionAccess flag, EnvironmentPermissionAccess question)
        {
            return ((flag & question) != EnvironmentPermissionAccess.NoAccess);
        }

        [SecuritySafeCritical]
        public override void FromXml(SecurityElement esd)
        {
            CodeAccessPermission.ValidateElement(esd, this);
            if (XMLUtil.IsUnrestricted(esd))
            {
                this.m_unrestricted = true;
            }
            else
            {
                this.m_unrestricted = false;
                this.m_read = null;
                this.m_write = null;
                string str = esd.Attribute("Read");
                if (str != null)
                {
                    this.m_read = new EnvironmentStringExpressionSet(str);
                }
                str = esd.Attribute("Write");
                if (str != null)
                {
                    this.m_write = new EnvironmentStringExpressionSet(str);
                }
            }
        }

        public string GetPathList(EnvironmentPermissionAccess flag)
        {
            this.VerifyFlag(flag);
            this.ExclusiveFlag(flag);
            if (this.FlagIsSet(flag, EnvironmentPermissionAccess.Read))
            {
                if (this.m_read == null)
                {
                    return "";
                }
                return this.m_read.ToString();
            }
            if (!this.FlagIsSet(flag, EnvironmentPermissionAccess.Write))
            {
                return "";
            }
            if (this.m_write == null)
            {
                return "";
            }
            return this.m_write.ToString();
        }

        internal static int GetTokenIndex()
        {
            return 0;
        }

        [SecuritySafeCritical]
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
            if (this.IsUnrestricted())
            {
                return target.Copy();
            }
            EnvironmentPermission permission = (EnvironmentPermission) target;
            if (permission.IsUnrestricted())
            {
                return this.Copy();
            }
            StringExpressionSet set = (this.m_read == null) ? null : this.m_read.Intersect(permission.m_read);
            StringExpressionSet set2 = (this.m_write == null) ? null : this.m_write.Intersect(permission.m_write);
            if (((set == null) || set.IsEmpty()) && ((set2 == null) || set2.IsEmpty()))
            {
                return null;
            }
            return new EnvironmentPermission(PermissionState.None) { m_unrestricted = false, m_read = set, m_write = set2 };
        }

        private bool IsEmpty()
        {
            if (this.m_unrestricted || ((this.m_read != null) && !this.m_read.IsEmpty()))
            {
                return false;
            }
            if (this.m_write != null)
            {
                return this.m_write.IsEmpty();
            }
            return true;
        }

        [SecuritySafeCritical]
        public override bool IsSubsetOf(IPermission target)
        {
            bool flag;
            if (target == null)
            {
                return this.IsEmpty();
            }
            try
            {
                EnvironmentPermission permission = (EnvironmentPermission) target;
                if (permission.IsUnrestricted())
                {
                    return true;
                }
                if (this.IsUnrestricted())
                {
                    return false;
                }
                flag = ((this.m_read == null) || this.m_read.IsSubsetOf(permission.m_read)) && ((this.m_write == null) || this.m_write.IsSubsetOf(permission.m_write));
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            return flag;
        }

        public bool IsUnrestricted()
        {
            return this.m_unrestricted;
        }

        [SecuritySafeCritical]
        public void SetPathList(EnvironmentPermissionAccess flag, string pathList)
        {
            this.VerifyFlag(flag);
            this.m_unrestricted = false;
            if ((flag & EnvironmentPermissionAccess.Read) != EnvironmentPermissionAccess.NoAccess)
            {
                this.m_read = null;
            }
            if ((flag & EnvironmentPermissionAccess.Write) != EnvironmentPermissionAccess.NoAccess)
            {
                this.m_write = null;
            }
            this.AddPathList(flag, pathList);
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.EnvironmentPermission");
            if (!this.IsUnrestricted())
            {
                if ((this.m_read != null) && !this.m_read.IsEmpty())
                {
                    element.AddAttribute("Read", SecurityElement.Escape(this.m_read.ToString()));
                }
                if ((this.m_write != null) && !this.m_write.IsEmpty())
                {
                    element.AddAttribute("Write", SecurityElement.Escape(this.m_write.ToString()));
                }
                return element;
            }
            element.AddAttribute("Unrestricted", "true");
            return element;
        }

        [SecuritySafeCritical]
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
            EnvironmentPermission permission = (EnvironmentPermission) other;
            if (this.IsUnrestricted() || permission.IsUnrestricted())
            {
                return new EnvironmentPermission(PermissionState.Unrestricted);
            }
            StringExpressionSet set = (this.m_read == null) ? permission.m_read : this.m_read.Union(permission.m_read);
            StringExpressionSet set2 = (this.m_write == null) ? permission.m_write : this.m_write.Union(permission.m_write);
            if (((set == null) || set.IsEmpty()) && ((set2 == null) || set2.IsEmpty()))
            {
                return null;
            }
            return new EnvironmentPermission(PermissionState.None) { m_unrestricted = false, m_read = set, m_write = set2 };
        }

        private void VerifyFlag(EnvironmentPermissionAccess flag)
        {
            if ((flag & ~EnvironmentPermissionAccess.AllAccess) != EnvironmentPermissionAccess.NoAccess)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) flag }));
            }
        }
    }
}

