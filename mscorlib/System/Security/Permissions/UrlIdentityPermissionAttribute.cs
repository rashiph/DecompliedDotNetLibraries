namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, ComVisible(true), AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class UrlIdentityPermissionAttribute : CodeAccessSecurityAttribute
    {
        private string m_url;

        public UrlIdentityPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            if (base.m_unrestricted)
            {
                return new UrlIdentityPermission(PermissionState.Unrestricted);
            }
            if (this.m_url == null)
            {
                return new UrlIdentityPermission(PermissionState.None);
            }
            return new UrlIdentityPermission(this.m_url);
        }

        public string Url
        {
            get
            {
                return this.m_url;
            }
            set
            {
                this.m_url = value;
            }
        }
    }
}

