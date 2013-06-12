namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public sealed class EventWaitHandleAccessRule : AccessRule
    {
        public EventWaitHandleAccessRule(IdentityReference identity, System.Security.AccessControl.EventWaitHandleRights eventRights, AccessControlType type) : this(identity, (int) eventRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public EventWaitHandleAccessRule(string identity, System.Security.AccessControl.EventWaitHandleRights eventRights, AccessControlType type) : this(new NTAccount(identity), (int) eventRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        internal EventWaitHandleAccessRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, type)
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

