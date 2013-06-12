namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public sealed class MutexAuditRule : AuditRule
    {
        public MutexAuditRule(IdentityReference identity, System.Security.AccessControl.MutexRights eventRights, AuditFlags flags) : this(identity, (int) eventRights, false, InheritanceFlags.None, PropagationFlags.None, flags)
        {
        }

        internal MutexAuditRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, flags)
        {
        }

        public System.Security.AccessControl.MutexRights MutexRights
        {
            get
            {
                return (System.Security.AccessControl.MutexRights) base.AccessMask;
            }
        }
    }
}

