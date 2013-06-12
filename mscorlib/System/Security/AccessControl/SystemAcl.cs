namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public sealed class SystemAcl : CommonAcl
    {
        public SystemAcl(bool isContainer, bool isDS, int capacity) : this(isContainer, isDS, isDS ? GenericAcl.AclRevisionDS : GenericAcl.AclRevision, capacity)
        {
        }

        public SystemAcl(bool isContainer, bool isDS, RawAcl rawAcl) : this(isContainer, isDS, rawAcl, false)
        {
        }

        public SystemAcl(bool isContainer, bool isDS, byte revision, int capacity) : base(isContainer, isDS, revision, capacity)
        {
        }

        internal SystemAcl(bool isContainer, bool isDS, RawAcl rawAcl, bool trusted) : base(isContainer, isDS, rawAcl, trusted, false)
        {
        }

        public void AddAudit(AuditFlags auditFlags, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags)
        {
            base.CheckFlags(inheritanceFlags, propagationFlags);
            base.AddQualifiedAce(sid, AceQualifier.SystemAudit, accessMask, (AceFlags) ((byte) (GenericAce.AceFlagsFromAuditFlags(auditFlags) | GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags))), ObjectAceFlags.None, Guid.Empty, Guid.Empty);
        }

        public void AddAudit(AuditFlags auditFlags, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
        {
            if (!base.IsDS)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_OnlyValidForDS"));
            }
            base.CheckFlags(inheritanceFlags, propagationFlags);
            base.AddQualifiedAce(sid, AceQualifier.SystemAudit, accessMask, (AceFlags) ((byte) (GenericAce.AceFlagsFromAuditFlags(auditFlags) | GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags))), objectFlags, objectType, inheritedObjectType);
        }

        public bool RemoveAudit(AuditFlags auditFlags, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags)
        {
            return base.RemoveQualifiedAces(sid, AceQualifier.SystemAudit, accessMask, (AceFlags) ((byte) (GenericAce.AceFlagsFromAuditFlags(auditFlags) | GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags))), true, ObjectAceFlags.None, Guid.Empty, Guid.Empty);
        }

        public bool RemoveAudit(AuditFlags auditFlags, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
        {
            if (!base.IsDS)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_OnlyValidForDS"));
            }
            return base.RemoveQualifiedAces(sid, AceQualifier.SystemAudit, accessMask, (AceFlags) ((byte) (GenericAce.AceFlagsFromAuditFlags(auditFlags) | GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags))), true, objectFlags, objectType, inheritedObjectType);
        }

        public void RemoveAuditSpecific(AuditFlags auditFlags, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags)
        {
            base.RemoveQualifiedAcesSpecific(sid, AceQualifier.SystemAudit, accessMask, (AceFlags) ((byte) (GenericAce.AceFlagsFromAuditFlags(auditFlags) | GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags))), ObjectAceFlags.None, Guid.Empty, Guid.Empty);
        }

        public void RemoveAuditSpecific(AuditFlags auditFlags, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
        {
            if (!base.IsDS)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_OnlyValidForDS"));
            }
            base.RemoveQualifiedAcesSpecific(sid, AceQualifier.SystemAudit, accessMask, (AceFlags) ((byte) (GenericAce.AceFlagsFromAuditFlags(auditFlags) | GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags))), objectFlags, objectType, inheritedObjectType);
        }

        public void SetAudit(AuditFlags auditFlags, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags)
        {
            base.CheckFlags(inheritanceFlags, propagationFlags);
            base.SetQualifiedAce(sid, AceQualifier.SystemAudit, accessMask, (AceFlags) ((byte) (GenericAce.AceFlagsFromAuditFlags(auditFlags) | GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags))), ObjectAceFlags.None, Guid.Empty, Guid.Empty);
        }

        public void SetAudit(AuditFlags auditFlags, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
        {
            if (!base.IsDS)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_OnlyValidForDS"));
            }
            base.CheckFlags(inheritanceFlags, propagationFlags);
            base.SetQualifiedAce(sid, AceQualifier.SystemAudit, accessMask, (AceFlags) ((byte) (GenericAce.AceFlagsFromAuditFlags(auditFlags) | GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags))), objectFlags, objectType, inheritedObjectType);
        }
    }
}

