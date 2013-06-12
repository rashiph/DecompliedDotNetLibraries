namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public sealed class MutexAccessRule : AccessRule
    {
        public MutexAccessRule(IdentityReference identity, System.Security.AccessControl.MutexRights eventRights, AccessControlType type) : this(identity, (int) eventRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public MutexAccessRule(string identity, System.Security.AccessControl.MutexRights eventRights, AccessControlType type) : this(new NTAccount(identity), (int) eventRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        internal MutexAccessRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, type)
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

