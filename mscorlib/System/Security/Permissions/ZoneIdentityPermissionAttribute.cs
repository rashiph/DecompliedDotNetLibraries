namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false), ComVisible(true)]
    public sealed class ZoneIdentityPermissionAttribute : CodeAccessSecurityAttribute
    {
        private SecurityZone m_flag;

        public ZoneIdentityPermissionAttribute(SecurityAction action) : base(action)
        {
            this.m_flag = SecurityZone.NoZone;
        }

        public override IPermission CreatePermission()
        {
            if (base.m_unrestricted)
            {
                return new ZoneIdentityPermission(PermissionState.Unrestricted);
            }
            return new ZoneIdentityPermission(this.m_flag);
        }

        public SecurityZone Zone
        {
            get
            {
                return this.m_flag;
            }
            set
            {
                this.m_flag = value;
            }
        }
    }
}

