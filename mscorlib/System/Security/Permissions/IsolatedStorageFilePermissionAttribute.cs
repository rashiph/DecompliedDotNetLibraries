namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, ComVisible(true), AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class IsolatedStorageFilePermissionAttribute : IsolatedStoragePermissionAttribute
    {
        public IsolatedStorageFilePermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            if (base.m_unrestricted)
            {
                return new IsolatedStorageFilePermission(PermissionState.Unrestricted);
            }
            return new IsolatedStorageFilePermission(PermissionState.None) { UserQuota = base.m_userQuota, UsageAllowed = base.m_allowed };
        }
    }
}

