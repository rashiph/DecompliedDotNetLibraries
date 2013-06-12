namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class RegistryPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission
    {
        [OptionalField(VersionAdded=2)]
        private StringExpressionSet m_changeAcl;
        private StringExpressionSet m_create;
        private StringExpressionSet m_read;
        private bool m_unrestricted;
        [OptionalField(VersionAdded=2)]
        private StringExpressionSet m_viewAcl;
        private StringExpressionSet m_write;

        public RegistryPermission(PermissionState state)
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
        public RegistryPermission(RegistryPermissionAccess access, string pathList)
        {
            this.SetPathList(access, pathList);
        }

        [SecuritySafeCritical]
        public RegistryPermission(RegistryPermissionAccess access, AccessControlActions control, string pathList)
        {
            this.m_unrestricted = false;
            this.AddPathList(access, control, pathList);
        }

        [SecuritySafeCritical]
        public void AddPathList(RegistryPermissionAccess access, string pathList)
        {
            this.AddPathList(access, AccessControlActions.None, pathList);
        }

        [SecuritySafeCritical]
        public void AddPathList(RegistryPermissionAccess access, AccessControlActions control, string pathList)
        {
            this.VerifyAccess(access);
            if ((access & RegistryPermissionAccess.Read) != RegistryPermissionAccess.NoAccess)
            {
                if (this.m_read == null)
                {
                    this.m_read = new StringExpressionSet();
                }
                this.m_read.AddExpressions(pathList);
            }
            if ((access & RegistryPermissionAccess.Write) != RegistryPermissionAccess.NoAccess)
            {
                if (this.m_write == null)
                {
                    this.m_write = new StringExpressionSet();
                }
                this.m_write.AddExpressions(pathList);
            }
            if ((access & RegistryPermissionAccess.Create) != RegistryPermissionAccess.NoAccess)
            {
                if (this.m_create == null)
                {
                    this.m_create = new StringExpressionSet();
                }
                this.m_create.AddExpressions(pathList);
            }
            if ((control & AccessControlActions.View) != AccessControlActions.None)
            {
                if (this.m_viewAcl == null)
                {
                    this.m_viewAcl = new StringExpressionSet();
                }
                this.m_viewAcl.AddExpressions(pathList);
            }
            if ((control & AccessControlActions.Change) != AccessControlActions.None)
            {
                if (this.m_changeAcl == null)
                {
                    this.m_changeAcl = new StringExpressionSet();
                }
                this.m_changeAcl.AddExpressions(pathList);
            }
        }

        public override IPermission Copy()
        {
            RegistryPermission permission = new RegistryPermission(PermissionState.None);
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
            if (this.m_create != null)
            {
                permission.m_create = this.m_create.Copy();
            }
            if (this.m_viewAcl != null)
            {
                permission.m_viewAcl = this.m_viewAcl.Copy();
            }
            if (this.m_changeAcl != null)
            {
                permission.m_changeAcl = this.m_changeAcl.Copy();
            }
            return permission;
        }

        private void ExclusiveAccess(RegistryPermissionAccess access)
        {
            if (access == RegistryPermissionAccess.NoAccess)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumNotSingleFlag"));
            }
            if ((access & (access - 1)) != RegistryPermissionAccess.NoAccess)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumNotSingleFlag"));
            }
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
                this.m_create = null;
                this.m_viewAcl = null;
                this.m_changeAcl = null;
                string str = esd.Attribute("Read");
                if (str != null)
                {
                    this.m_read = new StringExpressionSet(str);
                }
                str = esd.Attribute("Write");
                if (str != null)
                {
                    this.m_write = new StringExpressionSet(str);
                }
                str = esd.Attribute("Create");
                if (str != null)
                {
                    this.m_create = new StringExpressionSet(str);
                }
                str = esd.Attribute("ViewAccessControl");
                if (str != null)
                {
                    this.m_viewAcl = new StringExpressionSet(str);
                }
                str = esd.Attribute("ChangeAccessControl");
                if (str != null)
                {
                    this.m_changeAcl = new StringExpressionSet(str);
                }
            }
        }

        public string GetPathList(RegistryPermissionAccess access)
        {
            this.VerifyAccess(access);
            this.ExclusiveAccess(access);
            if ((access & RegistryPermissionAccess.Read) != RegistryPermissionAccess.NoAccess)
            {
                if (this.m_read == null)
                {
                    return "";
                }
                return this.m_read.ToString();
            }
            if ((access & RegistryPermissionAccess.Write) != RegistryPermissionAccess.NoAccess)
            {
                if (this.m_write == null)
                {
                    return "";
                }
                return this.m_write.ToString();
            }
            if ((access & RegistryPermissionAccess.Create) == RegistryPermissionAccess.NoAccess)
            {
                return "";
            }
            if (this.m_create == null)
            {
                return "";
            }
            return this.m_create.ToString();
        }

        internal static int GetTokenIndex()
        {
            return 5;
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
            RegistryPermission permission = (RegistryPermission) target;
            if (permission.IsUnrestricted())
            {
                return this.Copy();
            }
            StringExpressionSet set = (this.m_read == null) ? null : this.m_read.Intersect(permission.m_read);
            StringExpressionSet set2 = (this.m_write == null) ? null : this.m_write.Intersect(permission.m_write);
            StringExpressionSet set3 = (this.m_create == null) ? null : this.m_create.Intersect(permission.m_create);
            StringExpressionSet set4 = (this.m_viewAcl == null) ? null : this.m_viewAcl.Intersect(permission.m_viewAcl);
            StringExpressionSet set5 = (this.m_changeAcl == null) ? null : this.m_changeAcl.Intersect(permission.m_changeAcl);
            if (((((set == null) || set.IsEmpty()) && ((set2 == null) || set2.IsEmpty())) && (((set3 == null) || set3.IsEmpty()) && ((set4 == null) || set4.IsEmpty()))) && ((set5 == null) || set5.IsEmpty()))
            {
                return null;
            }
            return new RegistryPermission(PermissionState.None) { m_unrestricted = false, m_read = set, m_write = set2, m_create = set3, m_viewAcl = set4, m_changeAcl = set5 };
        }

        private bool IsEmpty()
        {
            if (((!this.m_unrestricted && ((this.m_read == null) || this.m_read.IsEmpty())) && ((this.m_write == null) || this.m_write.IsEmpty())) && (((this.m_create == null) || this.m_create.IsEmpty()) && ((this.m_viewAcl == null) || this.m_viewAcl.IsEmpty())))
            {
                if (this.m_changeAcl != null)
                {
                    return this.m_changeAcl.IsEmpty();
                }
                return true;
            }
            return false;
        }

        [SecuritySafeCritical]
        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return this.IsEmpty();
            }
            RegistryPermission permission = target as RegistryPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            if (permission.IsUnrestricted())
            {
                return true;
            }
            if (!this.IsUnrestricted() && ((((this.m_read == null) || this.m_read.IsSubsetOf(permission.m_read)) && ((this.m_write == null) || this.m_write.IsSubsetOf(permission.m_write))) && (((this.m_create == null) || this.m_create.IsSubsetOf(permission.m_create)) && ((this.m_viewAcl == null) || this.m_viewAcl.IsSubsetOf(permission.m_viewAcl)))))
            {
                if (this.m_changeAcl != null)
                {
                    return this.m_changeAcl.IsSubsetOf(permission.m_changeAcl);
                }
                return true;
            }
            return false;
        }

        public bool IsUnrestricted()
        {
            return this.m_unrestricted;
        }

        internal void SetPathList(AccessControlActions control, string pathList)
        {
            this.m_unrestricted = false;
            if ((control & AccessControlActions.View) != AccessControlActions.None)
            {
                this.m_viewAcl = null;
            }
            if ((control & AccessControlActions.Change) != AccessControlActions.None)
            {
                this.m_changeAcl = null;
            }
            this.AddPathList(RegistryPermissionAccess.NoAccess, control, pathList);
        }

        [SecuritySafeCritical]
        public void SetPathList(RegistryPermissionAccess access, string pathList)
        {
            this.VerifyAccess(access);
            this.m_unrestricted = false;
            if ((access & RegistryPermissionAccess.Read) != RegistryPermissionAccess.NoAccess)
            {
                this.m_read = null;
            }
            if ((access & RegistryPermissionAccess.Write) != RegistryPermissionAccess.NoAccess)
            {
                this.m_write = null;
            }
            if ((access & RegistryPermissionAccess.Create) != RegistryPermissionAccess.NoAccess)
            {
                this.m_create = null;
            }
            this.AddPathList(access, pathList);
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.RegistryPermission");
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
                if ((this.m_create != null) && !this.m_create.IsEmpty())
                {
                    element.AddAttribute("Create", SecurityElement.Escape(this.m_create.ToString()));
                }
                if ((this.m_viewAcl != null) && !this.m_viewAcl.IsEmpty())
                {
                    element.AddAttribute("ViewAccessControl", SecurityElement.Escape(this.m_viewAcl.ToString()));
                }
                if ((this.m_changeAcl != null) && !this.m_changeAcl.IsEmpty())
                {
                    element.AddAttribute("ChangeAccessControl", SecurityElement.Escape(this.m_changeAcl.ToString()));
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
            RegistryPermission permission = (RegistryPermission) other;
            if (this.IsUnrestricted() || permission.IsUnrestricted())
            {
                return new RegistryPermission(PermissionState.Unrestricted);
            }
            StringExpressionSet set = (this.m_read == null) ? permission.m_read : this.m_read.Union(permission.m_read);
            StringExpressionSet set2 = (this.m_write == null) ? permission.m_write : this.m_write.Union(permission.m_write);
            StringExpressionSet set3 = (this.m_create == null) ? permission.m_create : this.m_create.Union(permission.m_create);
            StringExpressionSet set4 = (this.m_viewAcl == null) ? permission.m_viewAcl : this.m_viewAcl.Union(permission.m_viewAcl);
            StringExpressionSet set5 = (this.m_changeAcl == null) ? permission.m_changeAcl : this.m_changeAcl.Union(permission.m_changeAcl);
            if (((((set == null) || set.IsEmpty()) && ((set2 == null) || set2.IsEmpty())) && (((set3 == null) || set3.IsEmpty()) && ((set4 == null) || set4.IsEmpty()))) && ((set5 == null) || set5.IsEmpty()))
            {
                return null;
            }
            return new RegistryPermission(PermissionState.None) { m_unrestricted = false, m_read = set, m_write = set2, m_create = set3, m_viewAcl = set4, m_changeAcl = set5 };
        }

        private void VerifyAccess(RegistryPermissionAccess access)
        {
            if ((access & ~RegistryPermissionAccess.AllAccess) != RegistryPermissionAccess.NoAccess)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) access }));
            }
        }
    }
}

