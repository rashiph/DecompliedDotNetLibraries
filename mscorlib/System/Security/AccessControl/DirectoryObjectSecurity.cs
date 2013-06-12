namespace System.Security.AccessControl
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;

    public abstract class DirectoryObjectSecurity : ObjectSecurity
    {
        protected DirectoryObjectSecurity() : base(true, true)
        {
        }

        protected DirectoryObjectSecurity(CommonSecurityDescriptor securityDescriptor) : base(securityDescriptor)
        {
        }

        public virtual AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type, Guid objectType, Guid inheritedObjectType)
        {
            throw new NotImplementedException();
        }

        protected void AddAccessRule(ObjectAccessRule rule)
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

        protected void AddAuditRule(ObjectAuditRule rule)
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

        public virtual AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags, Guid objectType, Guid inheritedObjectType)
        {
            throw new NotImplementedException();
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
                        QualifiedAce ace = discretionaryAcl[j] as QualifiedAce;
                        if ((ace == null) || ace.IsCallback)
                        {
                            continue;
                        }
                        if (access)
                        {
                            if ((ace.AceQualifier == AceQualifier.AccessAllowed) || (ace.AceQualifier == AceQualifier.AccessDenied))
                            {
                                goto Label_00DD;
                            }
                            continue;
                        }
                        if (ace.AceQualifier != AceQualifier.SystemAudit)
                        {
                            continue;
                        }
                    Label_00DD:
                        references2.Add(ace.SecurityIdentifier);
                    }
                    references = references2.Translate(targetType);
                }
                for (int i = 0; i < discretionaryAcl.Count; i++)
                {
                    QualifiedAce ace2 = discretionaryAcl[i] as CommonAce;
                    if (ace2 == null)
                    {
                        ace2 = discretionaryAcl[i] as ObjectAce;
                        if (ace2 == null)
                        {
                            continue;
                        }
                    }
                    if (ace2.IsCallback)
                    {
                        continue;
                    }
                    if (access)
                    {
                        if ((ace2.AceQualifier == AceQualifier.AccessAllowed) || (ace2.AceQualifier == AceQualifier.AccessDenied))
                        {
                            goto Label_0174;
                        }
                        continue;
                    }
                    if (ace2.AceQualifier != AceQualifier.SystemAudit)
                    {
                        continue;
                    }
                Label_0174:
                    if ((includeExplicit && (((byte) (ace2.AceFlags & AceFlags.Inherited)) == 0)) || (includeInherited && (((byte) (ace2.AceFlags & AceFlags.Inherited)) != 0)))
                    {
                        IdentityReference identityReference = (targetType == typeof(SecurityIdentifier)) ? ace2.SecurityIdentifier : references[i];
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
                            if (ace2 is ObjectAce)
                            {
                                ObjectAce ace3 = ace2 as ObjectAce;
                                rules.AddRule(this.AccessRuleFactory(identityReference, ace3.AccessMask, ace3.IsInherited, ace3.InheritanceFlags, ace3.PropagationFlags, allow, ace3.ObjectAceType, ace3.InheritedObjectAceType));
                            }
                            else
                            {
                                CommonAce ace4 = ace2 as CommonAce;
                                if (ace4 != null)
                                {
                                    rules.AddRule(this.AccessRuleFactory(identityReference, ace4.AccessMask, ace4.IsInherited, ace4.InheritanceFlags, ace4.PropagationFlags, allow));
                                }
                            }
                        }
                        else if (ace2 is ObjectAce)
                        {
                            ObjectAce ace5 = ace2 as ObjectAce;
                            rules.AddRule(this.AuditRuleFactory(identityReference, ace5.AccessMask, ace5.IsInherited, ace5.InheritanceFlags, ace5.PropagationFlags, ace5.AuditFlags, ace5.ObjectAceType, ace5.InheritedObjectAceType));
                        }
                        else
                        {
                            CommonAce ace6 = ace2 as CommonAce;
                            if (ace6 != null)
                            {
                                rules.AddRule(this.AuditRuleFactory(identityReference, ace6.AccessMask, ace6.IsInherited, ace6.InheritanceFlags, ace6.PropagationFlags, ace6.AuditFlags));
                            }
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
            if (!this.AccessRuleType.IsAssignableFrom(rule.GetType()))
            {
                throw new ArgumentException(Environment.GetResourceString("AccessControl_InvalidAccessRuleType"), "rule");
            }
            return this.ModifyAccess(modification, rule as ObjectAccessRule, out modified);
        }

        private bool ModifyAccess(AccessControlModification modification, ObjectAccessRule rule, out bool modified)
        {
            bool flag = true;
            if (base._securityDescriptor.DiscretionaryAcl == null)
            {
                if (((modification == AccessControlModification.Remove) || (modification == AccessControlModification.RemoveAll)) || (modification == AccessControlModification.RemoveSpecific))
                {
                    modified = false;
                    return flag;
                }
                base._securityDescriptor.DiscretionaryAcl = new DiscretionaryAcl(base.IsContainer, base.IsDS, GenericAcl.AclRevisionDS, 1);
                base._securityDescriptor.AddControlFlags(ControlFlags.DiscretionaryAclPresent);
            }
            else if ((((modification == AccessControlModification.Add) || (modification == AccessControlModification.Set)) || (modification == AccessControlModification.Reset)) && ((rule.ObjectFlags != ObjectAceFlags.None) && (base._securityDescriptor.DiscretionaryAcl.Revision < GenericAcl.AclRevisionDS)))
            {
                byte[] binaryForm = new byte[base._securityDescriptor.DiscretionaryAcl.BinaryLength];
                base._securityDescriptor.DiscretionaryAcl.GetBinaryForm(binaryForm, 0);
                binaryForm[0] = GenericAcl.AclRevisionDS;
                base._securityDescriptor.DiscretionaryAcl = new DiscretionaryAcl(base.IsContainer, base.IsDS, new RawAcl(binaryForm, 0));
            }
            SecurityIdentifier sid = rule.IdentityReference.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
            if (rule.AccessControlType == AccessControlType.Allow)
            {
                switch (modification)
                {
                    case AccessControlModification.Add:
                        base._securityDescriptor.DiscretionaryAcl.AddAccess(AccessControlType.Allow, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
                        goto Label_045E;

                    case AccessControlModification.Set:
                        base._securityDescriptor.DiscretionaryAcl.SetAccess(AccessControlType.Allow, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
                        goto Label_045E;

                    case AccessControlModification.Reset:
                        base._securityDescriptor.DiscretionaryAcl.RemoveAccess(AccessControlType.Deny, sid, -1, InheritanceFlags.ContainerInherit, PropagationFlags.None, ObjectAceFlags.None, Guid.Empty, Guid.Empty);
                        base._securityDescriptor.DiscretionaryAcl.SetAccess(AccessControlType.Allow, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
                        goto Label_045E;

                    case AccessControlModification.Remove:
                        flag = base._securityDescriptor.DiscretionaryAcl.RemoveAccess(AccessControlType.Allow, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
                        goto Label_045E;

                    case AccessControlModification.RemoveAll:
                        flag = base._securityDescriptor.DiscretionaryAcl.RemoveAccess(AccessControlType.Allow, sid, -1, InheritanceFlags.ContainerInherit, PropagationFlags.None, ObjectAceFlags.None, Guid.Empty, Guid.Empty);
                        if (!flag)
                        {
                            throw new SystemException();
                        }
                        goto Label_045E;

                    case AccessControlModification.RemoveSpecific:
                        base._securityDescriptor.DiscretionaryAcl.RemoveAccessSpecific(AccessControlType.Allow, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
                        goto Label_045E;
                }
                throw new ArgumentOutOfRangeException("modification", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            if (rule.AccessControlType != AccessControlType.Deny)
            {
                throw new SystemException();
            }
            switch (modification)
            {
                case AccessControlModification.Add:
                    base._securityDescriptor.DiscretionaryAcl.AddAccess(AccessControlType.Deny, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
                    break;

                case AccessControlModification.Set:
                    base._securityDescriptor.DiscretionaryAcl.SetAccess(AccessControlType.Deny, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
                    break;

                case AccessControlModification.Reset:
                    base._securityDescriptor.DiscretionaryAcl.RemoveAccess(AccessControlType.Allow, sid, -1, InheritanceFlags.ContainerInherit, PropagationFlags.None, ObjectAceFlags.None, Guid.Empty, Guid.Empty);
                    base._securityDescriptor.DiscretionaryAcl.SetAccess(AccessControlType.Deny, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
                    break;

                case AccessControlModification.Remove:
                    flag = base._securityDescriptor.DiscretionaryAcl.RemoveAccess(AccessControlType.Deny, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
                    break;

                case AccessControlModification.RemoveAll:
                    flag = base._securityDescriptor.DiscretionaryAcl.RemoveAccess(AccessControlType.Deny, sid, -1, InheritanceFlags.ContainerInherit, PropagationFlags.None, ObjectAceFlags.None, Guid.Empty, Guid.Empty);
                    if (!flag)
                    {
                        throw new SystemException();
                    }
                    break;

                case AccessControlModification.RemoveSpecific:
                    base._securityDescriptor.DiscretionaryAcl.RemoveAccessSpecific(AccessControlType.Deny, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("modification", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
        Label_045E:
            modified = flag;
            base.AccessRulesModified |= modified;
            return flag;
        }

        protected override bool ModifyAudit(AccessControlModification modification, AuditRule rule, out bool modified)
        {
            if (!this.AuditRuleType.IsAssignableFrom(rule.GetType()))
            {
                throw new ArgumentException(Environment.GetResourceString("AccessControl_InvalidAuditRuleType"), "rule");
            }
            return this.ModifyAudit(modification, rule as ObjectAuditRule, out modified);
        }

        private bool ModifyAudit(AccessControlModification modification, ObjectAuditRule rule, out bool modified)
        {
            bool flag = true;
            if (base._securityDescriptor.SystemAcl == null)
            {
                if (((modification == AccessControlModification.Remove) || (modification == AccessControlModification.RemoveAll)) || (modification == AccessControlModification.RemoveSpecific))
                {
                    modified = false;
                    return flag;
                }
                base._securityDescriptor.SystemAcl = new SystemAcl(base.IsContainer, base.IsDS, GenericAcl.AclRevisionDS, 1);
                base._securityDescriptor.AddControlFlags(ControlFlags.SystemAclPresent);
            }
            else if ((((modification == AccessControlModification.Add) || (modification == AccessControlModification.Set)) || (modification == AccessControlModification.Reset)) && ((rule.ObjectFlags != ObjectAceFlags.None) && (base._securityDescriptor.SystemAcl.Revision < GenericAcl.AclRevisionDS)))
            {
                byte[] binaryForm = new byte[base._securityDescriptor.SystemAcl.BinaryLength];
                base._securityDescriptor.SystemAcl.GetBinaryForm(binaryForm, 0);
                binaryForm[0] = GenericAcl.AclRevisionDS;
                base._securityDescriptor.SystemAcl = new SystemAcl(base.IsContainer, base.IsDS, new RawAcl(binaryForm, 0));
            }
            SecurityIdentifier sid = rule.IdentityReference.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
            switch (modification)
            {
                case AccessControlModification.Add:
                    base._securityDescriptor.SystemAcl.AddAudit(rule.AuditFlags, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
                    break;

                case AccessControlModification.Set:
                    base._securityDescriptor.SystemAcl.SetAudit(rule.AuditFlags, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
                    break;

                case AccessControlModification.Reset:
                    base._securityDescriptor.SystemAcl.RemoveAudit(AuditFlags.Failure | AuditFlags.Success, sid, -1, InheritanceFlags.ContainerInherit, PropagationFlags.None, ObjectAceFlags.None, Guid.Empty, Guid.Empty);
                    base._securityDescriptor.SystemAcl.SetAudit(rule.AuditFlags, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
                    break;

                case AccessControlModification.Remove:
                    flag = base._securityDescriptor.SystemAcl.RemoveAudit(rule.AuditFlags, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
                    break;

                case AccessControlModification.RemoveAll:
                    flag = base._securityDescriptor.SystemAcl.RemoveAudit(AuditFlags.Failure | AuditFlags.Success, sid, -1, InheritanceFlags.ContainerInherit, PropagationFlags.None, ObjectAceFlags.None, Guid.Empty, Guid.Empty);
                    if (!flag)
                    {
                        throw new SystemException();
                    }
                    break;

                case AccessControlModification.RemoveSpecific:
                    base._securityDescriptor.SystemAcl.RemoveAuditSpecific(rule.AuditFlags, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("modification", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            modified = flag;
            base.AuditRulesModified |= modified;
            return flag;
        }

        protected bool RemoveAccessRule(ObjectAccessRule rule)
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

        protected void RemoveAccessRuleAll(ObjectAccessRule rule)
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

        protected void RemoveAccessRuleSpecific(ObjectAccessRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            if (base._securityDescriptor != null)
            {
                base.WriteLock();
                try
                {
                    bool flag;
                    this.ModifyAccess(AccessControlModification.RemoveSpecific, rule, out flag);
                }
                finally
                {
                    base.WriteUnlock();
                }
            }
        }

        protected bool RemoveAuditRule(ObjectAuditRule rule)
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

        protected void RemoveAuditRuleAll(ObjectAuditRule rule)
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

        protected void RemoveAuditRuleSpecific(ObjectAuditRule rule)
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

        protected void ResetAccessRule(ObjectAccessRule rule)
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

        protected void SetAccessRule(ObjectAccessRule rule)
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

        protected void SetAuditRule(ObjectAuditRule rule)
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

