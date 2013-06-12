namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false), ComVisible(true)]
    public sealed class StrongNameIdentityPermissionAttribute : CodeAccessSecurityAttribute
    {
        private string m_blob;
        private string m_name;
        private string m_version;

        public StrongNameIdentityPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            if (base.m_unrestricted)
            {
                return new StrongNameIdentityPermission(PermissionState.Unrestricted);
            }
            if (((this.m_blob == null) && (this.m_name == null)) && (this.m_version == null))
            {
                return new StrongNameIdentityPermission(PermissionState.None);
            }
            if (this.m_blob == null)
            {
                throw new ArgumentException(Environment.GetResourceString("ArgumentNull_Key"));
            }
            StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob(this.m_blob);
            if ((this.m_version != null) && !this.m_version.Equals(string.Empty))
            {
                return new StrongNameIdentityPermission(blob, this.m_name, new System.Version(this.m_version));
            }
            return new StrongNameIdentityPermission(blob, this.m_name, null);
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

        public string PublicKey
        {
            get
            {
                return this.m_blob;
            }
            set
            {
                this.m_blob = value;
            }
        }

        public string Version
        {
            get
            {
                return this.m_version;
            }
            set
            {
                this.m_version = value;
            }
        }
    }
}

