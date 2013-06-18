namespace System.DirectoryServices
{
    using System;
    using System.Security.AccessControl;
    using System.Security.Principal;

    public class ActiveDirectoryAuditRule : ObjectAuditRule
    {
        public ActiveDirectoryAuditRule(IdentityReference identity, System.DirectoryServices.ActiveDirectoryRights adRights, AuditFlags auditFlags) : this(identity, ActiveDirectoryRightsTranslator.AccessMaskFromRights(adRights), auditFlags, Guid.Empty, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
        {
        }

        public ActiveDirectoryAuditRule(IdentityReference identity, System.DirectoryServices.ActiveDirectoryRights adRights, AuditFlags auditFlags, ActiveDirectorySecurityInheritance inheritanceType) : this(identity, ActiveDirectoryRightsTranslator.AccessMaskFromRights(adRights), auditFlags, Guid.Empty, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), Guid.Empty)
        {
        }

        public ActiveDirectoryAuditRule(IdentityReference identity, System.DirectoryServices.ActiveDirectoryRights adRights, AuditFlags auditFlags, Guid objectType) : this(identity, ActiveDirectoryRightsTranslator.AccessMaskFromRights(adRights), auditFlags, objectType, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
        {
        }

        public ActiveDirectoryAuditRule(IdentityReference identity, System.DirectoryServices.ActiveDirectoryRights adRights, AuditFlags auditFlags, ActiveDirectorySecurityInheritance inheritanceType, Guid inheritedObjectType) : this(identity, ActiveDirectoryRightsTranslator.AccessMaskFromRights(adRights), auditFlags, Guid.Empty, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), inheritedObjectType)
        {
        }

        public ActiveDirectoryAuditRule(IdentityReference identity, System.DirectoryServices.ActiveDirectoryRights adRights, AuditFlags auditFlags, Guid objectType, ActiveDirectorySecurityInheritance inheritanceType) : this(identity, ActiveDirectoryRightsTranslator.AccessMaskFromRights(adRights), auditFlags, objectType, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), Guid.Empty)
        {
        }

        public ActiveDirectoryAuditRule(IdentityReference identity, System.DirectoryServices.ActiveDirectoryRights adRights, AuditFlags auditFlags, Guid objectType, ActiveDirectorySecurityInheritance inheritanceType, Guid inheritedObjectType) : this(identity, ActiveDirectoryRightsTranslator.AccessMaskFromRights(adRights), auditFlags, objectType, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), inheritedObjectType)
        {
        }

        internal ActiveDirectoryAuditRule(IdentityReference identity, int accessMask, AuditFlags auditFlags, Guid objectGuid, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, Guid inheritedObjectType) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, objectGuid, inheritedObjectType, auditFlags)
        {
        }

        public System.DirectoryServices.ActiveDirectoryRights ActiveDirectoryRights
        {
            get
            {
                return ActiveDirectoryRightsTranslator.RightsFromAccessMask(base.AccessMask);
            }
        }

        public ActiveDirectorySecurityInheritance InheritanceType
        {
            get
            {
                return ActiveDirectoryInheritanceTranslator.GetEffectiveInheritanceFlags(base.InheritanceFlags, base.PropagationFlags);
            }
        }
    }
}

