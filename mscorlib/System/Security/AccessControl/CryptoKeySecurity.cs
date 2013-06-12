namespace System.Security.AccessControl
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Principal;

    public sealed class CryptoKeySecurity : NativeObjectSecurity
    {
        private const ResourceType s_ResourceType = ResourceType.FileObject;

        public CryptoKeySecurity() : base(false, ResourceType.FileObject)
        {
        }

        [SecuritySafeCritical]
        public CryptoKeySecurity(CommonSecurityDescriptor securityDescriptor) : base(ResourceType.FileObject, securityDescriptor)
        {
        }

        public sealed override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new CryptoKeyAccessRule(identityReference, CryptoKeyAccessRule.RightsFromAccessMask(accessMask), type);
        }

        public void AddAccessRule(CryptoKeyAccessRule rule)
        {
            base.AddAccessRule(rule);
        }

        public void AddAuditRule(CryptoKeyAuditRule rule)
        {
            base.AddAuditRule(rule);
        }

        public sealed override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new CryptoKeyAuditRule(identityReference, CryptoKeyAuditRule.RightsFromAccessMask(accessMask), flags);
        }

        public bool RemoveAccessRule(CryptoKeyAccessRule rule)
        {
            return base.RemoveAccessRule(rule);
        }

        public void RemoveAccessRuleAll(CryptoKeyAccessRule rule)
        {
            base.RemoveAccessRuleAll(rule);
        }

        public void RemoveAccessRuleSpecific(CryptoKeyAccessRule rule)
        {
            base.RemoveAccessRuleSpecific(rule);
        }

        public bool RemoveAuditRule(CryptoKeyAuditRule rule)
        {
            return base.RemoveAuditRule(rule);
        }

        public void RemoveAuditRuleAll(CryptoKeyAuditRule rule)
        {
            base.RemoveAuditRuleAll(rule);
        }

        public void RemoveAuditRuleSpecific(CryptoKeyAuditRule rule)
        {
            base.RemoveAuditRuleSpecific(rule);
        }

        public void ResetAccessRule(CryptoKeyAccessRule rule)
        {
            base.ResetAccessRule(rule);
        }

        public void SetAccessRule(CryptoKeyAccessRule rule)
        {
            base.SetAccessRule(rule);
        }

        public void SetAuditRule(CryptoKeyAuditRule rule)
        {
            base.SetAuditRule(rule);
        }

        public override Type AccessRightType
        {
            get
            {
                return typeof(CryptoKeyRights);
            }
        }

        public override Type AccessRuleType
        {
            get
            {
                return typeof(CryptoKeyAccessRule);
            }
        }

        public override Type AuditRuleType
        {
            get
            {
                return typeof(CryptoKeyAuditRule);
            }
        }

        internal AccessControlSections ChangedAccessControlSections
        {
            [SecurityCritical]
            get
            {
                AccessControlSections none = AccessControlSections.None;
                bool flag = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                    }
                    finally
                    {
                        base.ReadLock();
                        flag = true;
                    }
                    if (base.AccessRulesModified)
                    {
                        none |= AccessControlSections.Access;
                    }
                    if (base.AuditRulesModified)
                    {
                        none |= AccessControlSections.Audit;
                    }
                    if (base.GroupModified)
                    {
                        none |= AccessControlSections.Group;
                    }
                    if (base.OwnerModified)
                    {
                        none |= AccessControlSections.Owner;
                    }
                }
                finally
                {
                    if (flag)
                    {
                        base.ReadUnlock();
                    }
                }
                return none;
            }
        }
    }
}

