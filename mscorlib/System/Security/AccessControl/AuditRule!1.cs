namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public class AuditRule<T> : AuditRule where T: struct
    {
        public AuditRule(IdentityReference identity, T rights, AuditFlags flags) : this(identity, rights, InheritanceFlags.None, PropagationFlags.None, flags)
        {
        }

        public AuditRule(string identity, T rights, AuditFlags flags) : this(new NTAccount(identity), rights, InheritanceFlags.None, PropagationFlags.None, flags)
        {
        }

        public AuditRule(IdentityReference identity, T rights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags) : this(identity, (int) rights, false, inheritanceFlags, propagationFlags, flags)
        {
        }

        public AuditRule(string identity, T rights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags) : this(new NTAccount(identity), (int) rights, false, inheritanceFlags, propagationFlags, flags)
        {
        }

        internal AuditRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, flags)
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

