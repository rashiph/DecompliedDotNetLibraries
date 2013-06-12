namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, ComVisible(true)]
    public sealed class GacIdentityPermission : CodeAccessPermission, IBuiltInPermission
    {
        public GacIdentityPermission()
        {
        }

        public GacIdentityPermission(PermissionState state)
        {
            if ((state != PermissionState.Unrestricted) && (state != PermissionState.None))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPermissionState"));
            }
        }

        public override IPermission Copy()
        {
            return new GacIdentityPermission();
        }

        public override void FromXml(SecurityElement securityElement)
        {
            CodeAccessPermission.ValidateElement(securityElement, this);
        }

        internal static int GetTokenIndex()
        {
            return 15;
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            if (!(target is GacIdentityPermission))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            return this.Copy();
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return false;
            }
            if (!(target is GacIdentityPermission))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            return true;
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        public override SecurityElement ToXml()
        {
            return CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.GacIdentityPermission");
        }

        public override IPermission Union(IPermission target)
        {
            if ((target != null) && !(target is GacIdentityPermission))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            return this.Copy();
        }
    }
}

