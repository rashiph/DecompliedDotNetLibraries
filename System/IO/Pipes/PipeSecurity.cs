namespace System.IO.Pipes
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Security.Principal;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class PipeSecurity : NativeObjectSecurity
    {
        public PipeSecurity() : base(false, ResourceType.KernelObject)
        {
        }

        [SecuritySafeCritical]
        internal PipeSecurity(SafePipeHandle safeHandle, AccessControlSections includeSections) : base(false, ResourceType.KernelObject, safeHandle, includeSections)
        {
        }

        public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            if (inheritanceFlags != InheritanceFlags.None)
            {
                throw new ArgumentException(System.SR.GetString("Argument_NonContainerInvalidAnyFlag"), "inheritanceFlags");
            }
            if (propagationFlags != PropagationFlags.None)
            {
                throw new ArgumentException(System.SR.GetString("Argument_NonContainerInvalidAnyFlag"), "propagationFlags");
            }
            return new PipeAccessRule(identityReference, accessMask, isInherited, type);
        }

        public void AddAccessRule(PipeAccessRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.AddAccessRule(rule);
        }

        public void AddAuditRule(PipeAuditRule rule)
        {
            base.AddAuditRule(rule);
        }

        public sealed override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            if (inheritanceFlags != InheritanceFlags.None)
            {
                throw new ArgumentException(System.SR.GetString("Argument_NonContainerInvalidAnyFlag"), "inheritanceFlags");
            }
            if (propagationFlags != PropagationFlags.None)
            {
                throw new ArgumentException(System.SR.GetString("Argument_NonContainerInvalidAnyFlag"), "propagationFlags");
            }
            return new PipeAuditRule(identityReference, accessMask, isInherited, flags);
        }

        private AccessControlSections GetAccessControlSectionsFromChanges()
        {
            AccessControlSections none = AccessControlSections.None;
            if (base.AccessRulesModified)
            {
                none = AccessControlSections.Access;
            }
            if (base.AuditRulesModified)
            {
                none |= AccessControlSections.Audit;
            }
            if (base.OwnerModified)
            {
                none |= AccessControlSections.Owner;
            }
            if (base.GroupModified)
            {
                none |= AccessControlSections.Group;
            }
            return none;
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        protected internal void Persist(SafeHandle handle)
        {
            base.WriteLock();
            try
            {
                bool flag;
                bool flag2;
                AccessControlSections accessControlSectionsFromChanges = this.GetAccessControlSectionsFromChanges();
                base.Persist(handle, accessControlSectionsFromChanges);
                base.AccessRulesModified = flag = false;
                base.AuditRulesModified = flag2 = flag;
                base.OwnerModified = base.GroupModified = flag2;
            }
            finally
            {
                base.WriteUnlock();
            }
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        protected internal void Persist(string name)
        {
            base.WriteLock();
            try
            {
                bool flag;
                bool flag2;
                AccessControlSections accessControlSectionsFromChanges = this.GetAccessControlSectionsFromChanges();
                base.Persist(name, accessControlSectionsFromChanges);
                base.AccessRulesModified = flag = false;
                base.AuditRulesModified = flag2 = flag;
                base.OwnerModified = base.GroupModified = flag2;
            }
            finally
            {
                base.WriteUnlock();
            }
        }

        public bool RemoveAccessRule(PipeAccessRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            AuthorizationRuleCollection rules = base.GetAccessRules(true, true, rule.IdentityReference.GetType());
            for (int i = 0; i < rules.Count; i++)
            {
                PipeAccessRule rule2 = rules[i] as PipeAccessRule;
                if (((rule2 != null) && (rule2.PipeAccessRights == rule.PipeAccessRights)) && ((rule2.IdentityReference == rule.IdentityReference) && (rule2.AccessControlType == rule.AccessControlType)))
                {
                    return base.RemoveAccessRule(rule);
                }
            }
            if (rule.PipeAccessRights != PipeAccessRights.FullControl)
            {
                return base.RemoveAccessRule(new PipeAccessRule(rule.IdentityReference, PipeAccessRule.AccessMaskFromRights(rule.PipeAccessRights, AccessControlType.Deny), false, rule.AccessControlType));
            }
            return base.RemoveAccessRule(rule);
        }

        public void RemoveAccessRuleSpecific(PipeAccessRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            AuthorizationRuleCollection rules = base.GetAccessRules(true, true, rule.IdentityReference.GetType());
            for (int i = 0; i < rules.Count; i++)
            {
                PipeAccessRule rule2 = rules[i] as PipeAccessRule;
                if (((rule2 != null) && (rule2.PipeAccessRights == rule.PipeAccessRights)) && ((rule2.IdentityReference == rule.IdentityReference) && (rule2.AccessControlType == rule.AccessControlType)))
                {
                    base.RemoveAccessRuleSpecific(rule);
                    return;
                }
            }
            if (rule.PipeAccessRights != PipeAccessRights.FullControl)
            {
                base.RemoveAccessRuleSpecific(new PipeAccessRule(rule.IdentityReference, PipeAccessRule.AccessMaskFromRights(rule.PipeAccessRights, AccessControlType.Deny), false, rule.AccessControlType));
            }
            else
            {
                base.RemoveAccessRuleSpecific(rule);
            }
        }

        public bool RemoveAuditRule(PipeAuditRule rule)
        {
            return base.RemoveAuditRule(rule);
        }

        public void RemoveAuditRuleAll(PipeAuditRule rule)
        {
            base.RemoveAuditRuleAll(rule);
        }

        public void RemoveAuditRuleSpecific(PipeAuditRule rule)
        {
            base.RemoveAuditRuleSpecific(rule);
        }

        public void ResetAccessRule(PipeAccessRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.ResetAccessRule(rule);
        }

        public void SetAccessRule(PipeAccessRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.SetAccessRule(rule);
        }

        public void SetAuditRule(PipeAuditRule rule)
        {
            base.SetAuditRule(rule);
        }

        public override Type AccessRightType
        {
            get
            {
                return typeof(PipeAccessRights);
            }
        }

        public override Type AccessRuleType
        {
            get
            {
                return typeof(PipeAccessRule);
            }
        }

        public override Type AuditRuleType
        {
            get
            {
                return typeof(PipeAuditRule);
            }
        }
    }
}

