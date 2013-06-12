namespace System.Security.AccessControl
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    [ComVisible(false)]
    public sealed class SemaphoreAuditRule : AuditRule
    {
        public SemaphoreAuditRule(IdentityReference identity, System.Security.AccessControl.SemaphoreRights eventRights, AuditFlags flags) : this(identity, (int) eventRights, false, InheritanceFlags.None, PropagationFlags.None, flags)
        {
        }

        internal SemaphoreAuditRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, flags)
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

