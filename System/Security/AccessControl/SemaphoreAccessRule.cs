namespace System.Security.AccessControl
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    [ComVisible(false)]
    public sealed class SemaphoreAccessRule : AccessRule
    {
        public SemaphoreAccessRule(IdentityReference identity, System.Security.AccessControl.SemaphoreRights eventRights, AccessControlType type) : this(identity, (int) eventRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public SemaphoreAccessRule(string identity, System.Security.AccessControl.SemaphoreRights eventRights, AccessControlType type) : this(new NTAccount(identity), (int) eventRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        internal SemaphoreAccessRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, type)
        {
        }

        public System.Security.AccessControl.SemaphoreRights SemaphoreRights
        {
            get
            {
                return (System.Security.AccessControl.SemaphoreRights) base.AccessMask;
            }
        }
    }
}

