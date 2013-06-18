namespace System.DirectoryServices
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Security.Principal;

    public class ActiveDirectorySecurity : DirectoryObjectSecurity
    {
        private SecurityMasks securityMaskUsedInRetrieval;

        public ActiveDirectorySecurity()
        {
            this.securityMaskUsedInRetrieval = SecurityMasks.Sacl | SecurityMasks.Dacl | SecurityMasks.Group | SecurityMasks.Owner;
        }

        internal ActiveDirectorySecurity(byte[] sdBinaryForm, SecurityMasks securityMask) : base(new CommonSecurityDescriptor(true, true, sdBinaryForm, 0))
        {
            this.securityMaskUsedInRetrieval = SecurityMasks.Sacl | SecurityMasks.Dacl | SecurityMasks.Group | SecurityMasks.Owner;
            this.securityMaskUsedInRetrieval = securityMask;
        }

        public sealed override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new ActiveDirectoryAccessRule(identityReference, accessMask, type, Guid.Empty, isInherited, inheritanceFlags, propagationFlags, Guid.Empty);
        }

        public sealed override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type, Guid objectGuid, Guid inheritedObjectGuid)
        {
            return new ActiveDirectoryAccessRule(identityReference, accessMask, type, objectGuid, isInherited, inheritanceFlags, propagationFlags, inheritedObjectGuid);
        }

        public void AddAccessRule(ActiveDirectoryAccessRule rule)
        {
            if (!this.DaclRetrieved())
            {
                throw new InvalidOperationException(Res.GetString("CannotModifyDacl"));
            }
            base.AddAccessRule(rule);
        }

        public void AddAuditRule(ActiveDirectoryAuditRule rule)
        {
            if (!this.SaclRetrieved())
            {
                throw new InvalidOperationException(Res.GetString("CannotModifySacl"));
            }
            base.AddAuditRule(rule);
        }

        public sealed override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new ActiveDirectoryAuditRule(identityReference, accessMask, flags, Guid.Empty, isInherited, inheritanceFlags, propagationFlags, Guid.Empty);
        }

        public sealed override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags, Guid objectGuid, Guid inheritedObjectGuid)
        {
            return new ActiveDirectoryAuditRule(identityReference, accessMask, flags, objectGuid, isInherited, inheritanceFlags, propagationFlags, inheritedObjectGuid);
        }

        private bool DaclRetrieved()
        {
            return ((this.securityMaskUsedInRetrieval & SecurityMasks.Dacl) != SecurityMasks.None);
        }

        internal bool IsModified()
        {
            bool flag;
            base.ReadLock();
            try
            {
                flag = ((base.OwnerModified || base.GroupModified) || base.AccessRulesModified) || base.AuditRulesModified;
            }
            finally
            {
                base.ReadUnlock();
            }
            return flag;
        }

        public override bool ModifyAccessRule(AccessControlModification modification, AccessRule rule, out bool modified)
        {
            if (!this.DaclRetrieved())
            {
                throw new InvalidOperationException(Res.GetString("CannotModifyDacl"));
            }
            return base.ModifyAccessRule(modification, rule, out modified);
        }

        public override bool ModifyAuditRule(AccessControlModification modification, AuditRule rule, out bool modified)
        {
            if (!this.SaclRetrieved())
            {
                throw new InvalidOperationException(Res.GetString("CannotModifySacl"));
            }
            return base.ModifyAuditRule(modification, rule, out modified);
        }

        public override void PurgeAccessRules(IdentityReference identity)
        {
            if (!this.DaclRetrieved())
            {
                throw new InvalidOperationException(Res.GetString("CannotModifyDacl"));
            }
            base.PurgeAccessRules(identity);
        }

        public override void PurgeAuditRules(IdentityReference identity)
        {
            if (!this.SaclRetrieved())
            {
                throw new InvalidOperationException(Res.GetString("CannotModifySacl"));
            }
            base.PurgeAuditRules(identity);
        }

        public void RemoveAccess(IdentityReference identity, AccessControlType type)
        {
            if (!this.DaclRetrieved())
            {
                throw new InvalidOperationException(Res.GetString("CannotModifyDacl"));
            }
            ActiveDirectoryAccessRule rule = new ActiveDirectoryAccessRule(identity, ActiveDirectoryRights.GenericRead, type, ActiveDirectorySecurityInheritance.None);
            base.RemoveAccessRuleAll(rule);
        }

        public bool RemoveAccessRule(ActiveDirectoryAccessRule rule)
        {
            if (!this.DaclRetrieved())
            {
                throw new InvalidOperationException(Res.GetString("CannotModifyDacl"));
            }
            return base.RemoveAccessRule(rule);
        }

        public void RemoveAccessRuleSpecific(ActiveDirectoryAccessRule rule)
        {
            if (!this.DaclRetrieved())
            {
                throw new InvalidOperationException(Res.GetString("CannotModifyDacl"));
            }
            base.RemoveAccessRuleSpecific(rule);
        }

        public void RemoveAudit(IdentityReference identity)
        {
            if (!this.SaclRetrieved())
            {
                throw new InvalidOperationException(Res.GetString("CannotModifySacl"));
            }
            ActiveDirectoryAuditRule rule = new ActiveDirectoryAuditRule(identity, ActiveDirectoryRights.GenericRead, AuditFlags.Failure | AuditFlags.Success, ActiveDirectorySecurityInheritance.None);
            base.RemoveAuditRuleAll(rule);
        }

        public bool RemoveAuditRule(ActiveDirectoryAuditRule rule)
        {
            if (!this.SaclRetrieved())
            {
                throw new InvalidOperationException(Res.GetString("CannotModifySacl"));
            }
            return base.RemoveAuditRule(rule);
        }

        public void RemoveAuditRuleSpecific(ActiveDirectoryAuditRule rule)
        {
            if (!this.SaclRetrieved())
            {
                throw new InvalidOperationException(Res.GetString("CannotModifySacl"));
            }
            base.RemoveAuditRuleSpecific(rule);
        }

        public void ResetAccessRule(ActiveDirectoryAccessRule rule)
        {
            if (!this.DaclRetrieved())
            {
                throw new InvalidOperationException(Res.GetString("CannotModifyDacl"));
            }
            base.ResetAccessRule(rule);
        }

        private bool SaclRetrieved()
        {
            return ((this.securityMaskUsedInRetrieval & SecurityMasks.Sacl) != SecurityMasks.None);
        }

        public void SetAccessRule(ActiveDirectoryAccessRule rule)
        {
            if (!this.DaclRetrieved())
            {
                throw new InvalidOperationException(Res.GetString("CannotModifyDacl"));
            }
            base.SetAccessRule(rule);
        }

        public void SetAuditRule(ActiveDirectoryAuditRule rule)
        {
            if (!this.SaclRetrieved())
            {
                throw new InvalidOperationException(Res.GetString("CannotModifySacl"));
            }
            base.SetAuditRule(rule);
        }

        public override Type AccessRightType
        {
            get
            {
                return typeof(ActiveDirectoryRights);
            }
        }

        public override Type AccessRuleType
        {
            get
            {
                return typeof(ActiveDirectoryAccessRule);
            }
        }

        public override Type AuditRuleType
        {
            get
            {
                return typeof(ActiveDirectoryAuditRule);
            }
        }
    }
}

