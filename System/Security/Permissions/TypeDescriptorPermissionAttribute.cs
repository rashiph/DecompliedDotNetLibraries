namespace System.Security.Permissions
{
    using System;
    using System.Security;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class TypeDescriptorPermissionAttribute : CodeAccessSecurityAttribute
    {
        private TypeDescriptorPermissionFlags m_flags;

        public TypeDescriptorPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            if (base.Unrestricted)
            {
                return new TypeDescriptorPermission(PermissionState.Unrestricted);
            }
            return new TypeDescriptorPermission(this.m_flags);
        }

        public TypeDescriptorPermissionFlags Flags
        {
            get
            {
                return this.m_flags;
            }
            set
            {
                TypeDescriptorPermission.VerifyFlags(value);
                this.m_flags = value;
            }
        }

        public bool RestrictedRegistrationAccess
        {
            get
            {
                return ((this.m_flags & TypeDescriptorPermissionFlags.RestrictedRegistrationAccess) != TypeDescriptorPermissionFlags.NoFlags);
            }
            set
            {
                this.m_flags = value ? (this.m_flags | TypeDescriptorPermissionFlags.RestrictedRegistrationAccess) : (this.m_flags & ~TypeDescriptorPermissionFlags.RestrictedRegistrationAccess);
            }
        }
    }
}

