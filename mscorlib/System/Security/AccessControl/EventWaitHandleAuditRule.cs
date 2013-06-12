namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public sealed class EventWaitHandleAuditRule : AuditRule
    {
        public EventWaitHandleAuditRule(IdentityReference identity, System.Security.AccessControl.EventWaitHandleRights eventRights, AuditFlags flags) : this(identity, (int) eventRights, false, InheritanceFlags.None, PropagationFlags.None, flags)
        {
        }

        internal EventWaitHandleAuditRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, flags)
        {
        }

        public System.Security.AccessControl.EventWaitHandleRights EventWaitHandleRights
        {
            get
            {
                return (System.Security.AccessControl.EventWaitHandleRights) base.AccessMask;
            }
        }
    }
}

