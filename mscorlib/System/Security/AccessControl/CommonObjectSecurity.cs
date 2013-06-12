namespace System.Security.AccessControl
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class CommonObjectSecurity : ObjectSecurity
    {
        protected CommonObjectSecurity(bool isContainer) : base(isContainer, false)
        {
        }

        internal CommonObjectSecurity(CommonSecurityDescriptor securityDescriptor) : base(securityDescriptor)
        {
        }

        private bool AceNeedsTranslation(CommonAce ace, bool isAccessAce, bool includeExplicit, bool includeInherited)
        {
            if (ace == null)
            {
                return false;
            }
            if (isAccessAce)
            {
                if ((ace.AceQualifier != AceQualifier.AccessAllowed) && (ace.AceQualifier != AceQualifier.AccessDenied))
                {
                    return false;
                }
            }
            else if (ace.AceQualifier != AceQualifier.SystemAudit)
            {
                return false;
            }
            if ((!includeExplicit || (((byte) (ace.AceFlags & AceFlags.Inherited)) != 0)) && (!includeInherited || (((byte) (ace.AceFlags & AceFlags.Inherited)) == 0)))
            {
                return false;
            }
            return true;
        }

        protected void AddAccessRule(AccessRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.WriteLock();
            try
            {
                bool flag;
                this.ModifyAccess(AccessControlModification.Add, rule, out flag);
            }
            finally
            {
                base.WriteUnlock();
            }
        }

        protected void AddAuditRule(AuditRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.WriteLock();
            try
            {
                bool flag;
                this.ModifyAudit(AccessControlModification.Add, rule, out flag);
            }
            finally
            {
                base.WriteUnlock();
            }
        }

        [SecuritySafeCritical]
        public AuthorizationRuleCollection GetAccessRules(bool includeExplicit, bool includeInherited, Type targetType)
        {
            return this.GetRules(true, includeExplicit, includeInherited, targetType);
        }

        [SecuritySafeCritical]
        public AuthorizationRuleCollection GetAuditRules(bool includeExplicit, bool includeInherited, Type targetType)
        {
            return this.GetRules(false, includeExplicit, includeInherited, targetType);
        }

        private AuthorizationRuleCollection GetRules(bool access, bool includeExplicit, bool includeInherited, Type targetType)
        {
            AuthorizationRuleCollection rules2;
            base.ReadLock();
            try
            {
                AuthorizationRuleCollection rules = new AuthorizationRuleCollection();
                if (!SecurityIdentifier.IsValidTargetTypeStatic(targetType))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_MustBeIdentityReferenceType"), "targetType");
                }
                CommonAcl discretionaryAcl = null;
                if (access)
                {
                    if ((base._securityDescriptor.ControlFlags & ControlFlags.DiscretionaryAclPresent) != ControlFlags.None)
                    {
                        discretionaryAcl = base._securityDescriptor.DiscretionaryAcl;
                    }
                }
                else if ((base._securityDescriptor.ControlFlags & ControlFlags.SystemAclPresent) != ControlFlags.None)
                {
                    discretionaryAcl = base._securityDescriptor.SystemAcl;
                }
                if (discretionaryAcl == null)
                {
                    return rules;
                }
                IdentityReferenceCollection references = null;
                if (targetType != typeof(SecurityIdentifier))
                {
                    IdentityReferenceCollection references2 = new IdentityReferenceCollection(discretionaryAcl.Count);
                    for (int j = 0; j < discretionaryAcl.Count; j++)
                    {
                        CommonAce ace = discretionaryAcl[j] as CommonAce;
                        if (this.AceNeedsTranslation(ace, access, includeExplicit, includeInherited))
                        {
                            references2.Add(ace.SecurityIdentifier);
                        }
                    }
                    references = references2.Translate(targetType);
                }
                int num2 = 0;
                for (int i = 0; i < discretionaryAcl.Count; i++)
                {
                    CommonAce ace2 = discretionaryAcl[i] as CommonAce;
                    if (this.AceNeedsTranslation(ace2, access, includeExplicit, includeInherited))
                    {
                        IdentityReference identityReference = (targetType == typeof(SecurityIdentifier)) ? ace2.SecurityIdentifier : references[num2++];
                        if (access)
                        {
                            AccessControlType allow;
                            if (ace2.AceQualifier == AceQualifier.AccessAllowed)
                            {
                                allow = AccessControlType.Allow;
                            }
                            else
                            {
                                allow = AccessControlType.Deny;
                            }
                            rules.AddRule(this.AccessRuleFactory(identityReference, ace2.AccessMask, ace2.IsInherited, ace2.InheritanceFlags, ace2.PropagationFlags, allow));
                        }
                        else
                        {
                            rules.AddRule(this.AuditRuleFactory(identityReference, ace2.AccessMask, ace2.IsInherited, ace2.InheritanceFlags, ace2.PropagationFlags, ace2.AuditFlags));
                        }
                    }
                }
                rules2 = rules;
            }
            finally
            {
                base.ReadUnlock();
            }
            return rules2;
        }

        protected override bool ModifyAccess(AccessControlModification modification, AccessRule rule, out bool modified)
        {
            bool flag2;
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.WriteLock();
            try
            {
                bool flag = true;
                if (base._securityDescriptor.DiscretionaryAcl == null)
                {
                    if (((modification == AccessControlModification.Remove) || (modification == AccessControlModification.RemoveAll)) || (modification == AccessControlModification.RemoveSpecific))
                    {
                        modified = false;
                        return flag;
                    }
                    base._securityDescriptor.DiscretionaryAcl = new DiscretionaryAcl(base.IsContainer, base.IsDS, GenericAcl.AclRevision, 1);
                    base._securityDescriptor.AddControlFlags(ControlFlags.DiscretionaryAclPresent);
                }
                SecurityIdentifier sid = rule.IdentityReference.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
                if (rule.AccessControlType == AccessControlType.Allow)
                {
                    switch (modification)
                    {
                        case AccessControlModification.Add:
                            base._securityDescriptor.DiscretionaryAcl.AddAccess(AccessControlType.Allow, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
                            goto Label_0343;

                        case AccessControlModification.Set:
                            base._securityDescriptor.DiscretionaryAcl.SetAccess(AccessControlType.Allow, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
                            goto Label_0343;

                        case AccessControlModification.Reset:
                            base._securityDescriptor.DiscretionaryAcl.RemoveAccess(AccessControlType.Deny, sid, -1, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None);
                            base._securityDescriptor.DiscretionaryAcl.SetAccess(AccessControlType.Allow, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
                            goto Label_0343;

                        case AccessControlModification.Remove:
                            flag = base._securityDescriptor.DiscretionaryAcl.RemoveAccess(AccessControlType.Allow, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
                            goto Label_0343;

                        case AccessControlModification.RemoveAll:
                            flag = base._securityDescriptor.DiscretionaryAcl.RemoveAccess(AccessControlType.Allow, sid, -1, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None);
                            if (!flag)
                            {
                                throw new SystemException();
                            }
                            goto Label_0343;

                        case AccessControlModification.RemoveSpecific:
                            base._securityDescriptor.DiscretionaryAcl.RemoveAccessSpecific(AccessControlType.Allow, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
                            goto Label_0343;
                    }
                    throw new ArgumentOutOfRangeException("modification", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
                }
                if (rule.AccessControlType == AccessControlType.Deny)
                {
                    switch (modification)
                    {
                        case AccessControlModification.Add:
                            base._securityDescriptor.DiscretionaryAcl.AddAccess(AccessControlType.Deny, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
                            goto Label_0343;

                        case AccessControlModification.Set:
                            base._securityDescriptor.DiscretionaryAcl.SetAccess(AccessControlType.Deny, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
                            goto Label_0343;

                        case AccessControlModification.Reset:
                            base._securityDescriptor.DiscretionaryAcl.RemoveAccess(AccessControlType.Allow, sid, -1, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None);
                            base._securityDescriptor.DiscretionaryAcl.SetAccess(AccessControlType.Deny, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
                            goto Label_0343;

                        case AccessControlModification.Remove:
                            flag = base._securityDescriptor.DiscretionaryAcl.RemoveAccess(AccessControlType.Deny, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
                            goto Label_0343;

                        case AccessControlModification.RemoveAll:
                            flag = base._securityDescriptor.DiscretionaryAcl.RemoveAccess(AccessControlType.Deny, sid, -1, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None);
                            if (!flag)
                            {
                                throw new SystemException();
                            }
                            goto Label_0343;

                        case AccessControlModification.RemoveSpecific:
                            base._securityDescriptor.DiscretionaryAcl.RemoveAccessSpecific(AccessControlType.Deny, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
                            goto Label_0343;
                    }
                    throw new ArgumentOutOfRangeException("modification", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
                }
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) rule.AccessControlType }), "rule.AccessControlType");
            Label_0343:
                modified = flag;
                base.AccessRulesModified |= modified;
                flag2 = flag;
            }
            finally
            {
                base.WriteUnlock();
            }
            return flag2;
        }

        protected override bool ModifyAudit(AccessControlModification modification, AuditRule rule, out bool modified)
        {
            bool flag2;
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.WriteLock();
            try
            {
                bool flag = true;
                if (base._securityDescriptor.SystemAcl == null)
                {
                    if (((modification == AccessControlModification.Remove) || (modification == AccessControlModification.RemoveAll)) || (modification == AccessControlModification.RemoveSpecific))
                    {
                        modified = false;
                        return flag;
                    }
                    base._securityDescriptor.SystemAcl = new SystemAcl(base.IsContainer, base.IsDS, GenericAcl.AclRevision, 1);
                    base._securityDescriptor.AddControlFlags(ControlFlags.SystemAclPresent);
                }
                SecurityIdentifier sid = rule.IdentityReference.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
                switch (modification)
                {
                    case AccessControlModification.Add:
                        base._securityDescriptor.SystemAcl.AddAudit(rule.AuditFlags, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
                        break;

                    case AccessControlModification.Set:
                        base._securityDescriptor.SystemAcl.SetAudit(rule.AuditFlags, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
                        break;

                    case AccessControlModification.Reset:
                        base._securityDescriptor.SystemAcl.SetAudit(rule.AuditFlags, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
                        break;

                    case AccessControlModification.Remove:
                        flag = base._securityDescriptor.SystemAcl.RemoveAudit(rule.AuditFlags, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
                        break;

                    case AccessControlModification.RemoveAll:
                        flag = base._securityDescriptor.SystemAcl.RemoveAudit(AuditFlags.Failure | AuditFlags.Success, sid, -1, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None);
                        if (!flag)
                        {
                            throw new InvalidProgramException();
                        }
                        break;

                    case AccessControlModification.RemoveSpecific:
                        base._securityDescriptor.SystemAcl.RemoveAuditSpecific(rule.AuditFlags, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("modification", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
                }
                modified = flag;
                base.AuditRulesModified |= modified;
                flag2 = flag;
            }
            finally
            {
                base.WriteUnlock();
            }
            return flag2;
        }

        protected bool RemoveAccessRule(AccessRule rule)
        {
            bool flag2;
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.WriteLock();
            try
            {
                bool flag;
                if (base._securityDescriptor == null)
                {
                    return true;
                }
                flag2 = this.ModifyAccess(AccessControlModification.Remove, rule, out flag);
            }
            finally
            {
                base.WriteUnlock();
            }
            return flag2;
        }

        protected void RemoveAccessRuleAll(AccessRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.WriteLock();
            try
            {
                if (base._securityDescriptor != null)
                {
                    bool flag;
                    this.ModifyAccess(AccessControlModification.RemoveAll, rule, out flag);
                }
            }
            finally
            {
                base.WriteUnlock();
            }
        }

        protected void RemoveAccessRuleSpecific(AccessRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.WriteLock();
            try
            {
                if (base._securityDescriptor != null)
                {
                    bool flag;
                    this.ModifyAccess(AccessControlModification.RemoveSpecific, rule, out flag);
                }
            }
            finally
            {
                base.WriteUnlock();
            }
        }

        protected bool RemoveAuditRule(AuditRule rule)
        {
            bool flag2;
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.WriteLock();
            try
            {
                bool flag;
                flag2 = this.ModifyAudit(AccessControlModification.Remove, rule, out flag);
            }
            finally
            {
                base.WriteUnlock();
            }
            return flag2;
        }

        protected void RemoveAuditRuleAll(AuditRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.WriteLock();
            try
            {
                bool flag;
                this.ModifyAudit(AccessControlModification.RemoveAll, rule, out flag);
            }
            finally
            {
                base.WriteUnlock();
            }
        }

        protected void RemoveAuditRuleSpecific(AuditRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.WriteLock();
            try
            {
                bool flag;
                this.ModifyAudit(AccessControlModification.RemoveSpecific, rule, out flag);
            }
            finally
            {
                base.WriteUnlock();
            }
        }

        protected void ResetAccessRule(AccessRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.WriteLock();
            try
            {
                bool flag;
                this.ModifyAccess(AccessControlModification.Reset, rule, out flag);
            }
            finally
            {
                base.WriteUnlock();
            }
        }

        protected void SetAccessRule(AccessRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.WriteLock();
            try
            {
                bool flag;
                this.ModifyAccess(AccessControlModification.Set, rule, out flag);
            }
            finally
            {
                base.WriteUnlock();
            }
        }

        protected void SetAuditRule(AuditRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            base.WriteLock();
            try
            {
                bool flag;
                this.ModifyAudit(AccessControlModification.Set, rule, out flag);
            }
            finally
            {
                base.WriteUnlock();
            }
        }
    }
}

