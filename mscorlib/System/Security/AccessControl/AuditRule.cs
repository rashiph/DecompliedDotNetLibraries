namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public abstract class AuditRule : AuthorizationRule
    {
        private readonly System.Security.AccessControl.AuditFlags _flags;

        protected AuditRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, System.Security.AccessControl.AuditFlags auditFlags) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags)
        {
            if (auditFlags == System.Security.AccessControl.AuditFlags.None)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumAtLeastOneFlag"), "auditFlags");
            }
            if ((auditFlags & ~(System.Security.AccessControl.AuditFlags.Failure | System.Security.AccessControl.AuditFlags.Success)) != System.Security.AccessControl.AuditFlags.None)
            {
                throw new ArgumentOutOfRangeException("auditFlags", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            this._flags = auditFlags;
        }

        public System.Security.AccessControl.AuditFlags AuditFlags
        {
            get
            {
                return this._flags;
            }
        }
    }
}

