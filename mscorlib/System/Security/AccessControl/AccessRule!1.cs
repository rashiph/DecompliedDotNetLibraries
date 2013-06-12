namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public class AccessRule<T> : AccessRule where T: struct
    {
        public AccessRule(IdentityReference identity, T rights, AccessControlType type) : this(identity, (int) rights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public AccessRule(string identity, T rights, AccessControlType type) : this(new NTAccount(identity), (int) rights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public AccessRule(IdentityReference identity, T rights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : this(identity, (int) rights, false, inheritanceFlags, propagationFlags, type)
        {
        }

        public AccessRule(string identity, T rights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : this(new NTAccount(identity), (int) rights, false, inheritanceFlags, propagationFlags, type)
        {
        }

        internal AccessRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, type)
        {
        }

        public T Rights
        {
            get
            {
                return (T) base.AccessMask;
            }
        }
    }
}

