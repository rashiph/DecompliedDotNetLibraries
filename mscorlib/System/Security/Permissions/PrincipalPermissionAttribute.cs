namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple=true, Inherited=false), ComVisible(true)]
    public sealed class PrincipalPermissionAttribute : CodeAccessSecurityAttribute
    {
        private bool m_authenticated;
        private string m_name;
        private string m_role;

        public PrincipalPermissionAttribute(SecurityAction action) : base(action)
        {
            this.m_authenticated = true;
        }

        public override IPermission CreatePermission()
        {
            if (base.m_unrestricted)
            {
                return new PrincipalPermission(PermissionState.Unrestricted);
            }
            return new PrincipalPermission(this.m_name, this.m_role, this.m_authenticated);
        }

        public bool Authenticated
        {
            get
            {
                return this.m_authenticated;
            }
            set
            {
                this.m_authenticated = value;
            }
        }

        public string Name
        {
            get
            {
                return this.m_name;
            }
            set
            {
                this.m_name = value;
            }
        }

        public string Role
        {
            get
            {
                return this.m_role;
            }
            set
            {
                this.m_role = value;
            }
        }
    }
}

