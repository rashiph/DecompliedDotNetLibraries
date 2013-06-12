namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false), ComVisible(true)]
    public sealed class SiteIdentityPermissionAttribute : CodeAccessSecurityAttribute
    {
        private string m_site;

        public SiteIdentityPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            if (base.m_unrestricted)
            {
                return new SiteIdentityPermission(PermissionState.Unrestricted);
            }
            if (this.m_site == null)
            {
                return new SiteIdentityPermission(PermissionState.None);
            }
            return new SiteIdentityPermission(this.m_site);
        }

        public string Site
        {
            get
            {
                return this.m_site;
            }
            set
            {
                this.m_site = value;
            }
        }
    }
}

