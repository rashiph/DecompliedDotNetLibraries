namespace System.Net.NetworkInformation
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class NetworkInformationPermissionAttribute : CodeAccessSecurityAttribute
    {
        private string access;
        private const string strAccess = "Access";

        public NetworkInformationPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            NetworkInformationPermission permission = null;
            if (base.Unrestricted)
            {
                return new NetworkInformationPermission(PermissionState.Unrestricted);
            }
            permission = new NetworkInformationPermission(PermissionState.None);
            if (this.access != null)
            {
                if (string.Compare(this.access, "Read", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    permission.AddPermission(NetworkInformationAccess.Read);
                    return permission;
                }
                if (string.Compare(this.access, "Ping", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    permission.AddPermission(NetworkInformationAccess.Ping);
                    return permission;
                }
                if (string.Compare(this.access, "None", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    throw new ArgumentException(SR.GetString("net_perm_invalid_val", new object[] { "Access", this.access }));
                }
                permission.AddPermission(NetworkInformationAccess.None);
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

