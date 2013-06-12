namespace System.Net.Mail
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class SmtpPermissionAttribute : CodeAccessSecurityAttribute
    {
        private string access;
        private const string strAccess = "Access";

        public SmtpPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            SmtpPermission permission = null;
            if (base.Unrestricted)
            {
                return new SmtpPermission(PermissionState.Unrestricted);
            }
            permission = new SmtpPermission(PermissionState.None);
            if (this.access != null)
            {
                if (string.Compare(this.access, "Connect", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    permission.AddPermission(SmtpAccess.Connect);
                    return permission;
                }
                if (string.Compare(this.access, "ConnectToUnrestrictedPort", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    permission.AddPermission(SmtpAccess.ConnectToUnrestrictedPort);
                    return permission;
                }
                if (string.Compare(this.access, "None", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    throw new ArgumentException(SR.GetString("net_perm_invalid_val", new object[] { "Access", this.access }));
                }
                permission.AddPermission(SmtpAccess.None);
            }
            return permission;
        }

        public string Access
        {
            get
            {
                return this.access;
            }
            set
            {
                this.access = value;
            }
        }
    }
}

