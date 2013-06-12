namespace System.Net
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class DnsPermissionAttribute : CodeAccessSecurityAttribute
    {
        public DnsPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            if (base.Unrestricted)
            {
                return new DnsPermission(PermissionState.Unrestricted);
            }
            return new DnsPermission(PermissionState.None);
        }
    }
}

