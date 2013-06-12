namespace System.Security
{
    using System;
    using System.Security.Permissions;

    [SecurityCritical, PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public abstract class SecurityState
    {
        protected SecurityState()
        {
        }

        public abstract void EnsureState();
        [SecurityCritical]
        public bool IsStateAvailable()
        {
            AppDomainManager currentAppDomainManager = AppDomainManager.CurrentAppDomainManager;
            if (currentAppDomainManager == null)
            {
                return false;
            }
            return currentAppDomainManager.CheckSecuritySettings(this);
        }
    }
}

