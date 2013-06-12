namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class FileDialogPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission
    {
        private FileDialogPermissionAccess access;

        public FileDialogPermission(FileDialogPermissionAccess access)
        {
            VerifyAccess(access);
            this.access = access;
        }

        public FileDialogPermission(PermissionState state)
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
                this.Reset();
            }
        }

        public override IPermission Copy()
        {
            return new FileDialogPermission(this.access);
        }

        public override void FromXml(SecurityElement esd)
        {
            CodeAccessPermission.ValidateElement(esd, this);
            if (XMLUtil.IsUnrestricted(esd))
            {
                this.SetUnrestricted(true);
            }
            else
            {
                this.access = FileDialogPermissionAccess.None;
                string str = esd.Attribute("Access");
                if (str != null)
                {
                    this.access = (FileDialogPermissionAccess) Enum.Parse(typeof(FileDialogPermissionAccess), str);
                }
            }
        }

        internal static int GetTokenIndex()
        {
            return 1;
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
            FileDialogPermission permission = (FileDialogPermission) target;
            FileDialogPermissionAccess access = this.access & permission.Access;
            if (access == FileDialogPermissionAccess.None)
            {
                return null;
            }
            return new FileDialogPermission(access);
        }

        public override bool IsSubsetOf(IPermission target)
        {
            bool flag;
            if (target == null)
            {
                return (this.access == FileDialogPermissionAccess.None);
            }
            try
            {
                FileDialogPermission permission = (FileDialogPermission) target;
                if (permission.IsUnrestricted())
                {
                    return true;
                }
                if (this.IsUnrestricted())
                {
                    return false;
                }
                int num = ((int) this.access) & 1;
                int num2 = ((int) this.access) & 2;
                int num3 = ((int) permission.Access) & 1;
                int num4 = ((int) permission.Access) & 2;
                flag = (num <= num3) && (num2 <= num4);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            return flag;
        }

        public bool IsUnrestricted()
        {
            return (this.access == FileDialogPermissionAccess.OpenSave);
        }

        private void Reset()
        {
            this.access = FileDialogPermissionAccess.None;
        }

        private void SetUnrestricted(bool unrestricted)
        {
            if (unrestricted)
            {
                this.access = FileDialogPermissionAccess.OpenSave;
            }
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.FileDialogPermission");
            if (!this.IsUnrestricted())
            {
                if (this.access != FileDialogPermissionAccess.None)
                {
                    element.AddAttribute("Access", Enum.GetName(typeof(FileDialogPermissionAccess), this.access));
                }
                return element;
            }
            element.AddAttribute("Unrestricted", "true");
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                return this.Copy();
            }
            if (!base.VerifyType(target))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            FileDialogPermission permission = (FileDialogPermission) target;
            return new FileDialogPermission(this.access | permission.Access);
        }

        private static void VerifyAccess(FileDialogPermissionAccess access)
        {
            if ((access & ~FileDialogPermissionAccess.OpenSave) != FileDialogPermissionAccess.None)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) access }));
            }
        }

        public FileDialogPermissionAccess Access
        {
            get
            {
                return this.access;
            }
            set
            {
                VerifyAccess(value);
                this.access = value;
            }
        }
    }
}

