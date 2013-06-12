namespace System.Web
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, AttributeUsage(AttributeTargets.All, AllowMultiple=true, Inherited=false)]
    public sealed class AspNetHostingPermissionAttribute : CodeAccessSecurityAttribute
    {
        private AspNetHostingPermissionLevel _level;

        public AspNetHostingPermissionAttribute(SecurityAction action) : base(action)
        {
            this._level = AspNetHostingPermissionLevel.None;
        }

        public override IPermission CreatePermission()
        {
            if (base.Unrestricted)
            {
                return new AspNetHostingPermission(PermissionState.Unrestricted);
            }
            return new AspNetHostingPermission(this._level);
        }

        public AspNetHostingPermissionLevel Level
        {
            get
            {
                return this._level;
            }
            set
            {
                AspNetHostingPermission.VerifyAspNetHostingPermissionLevel(value, "Level");
                this._level = value;
            }
        }
    }
}

