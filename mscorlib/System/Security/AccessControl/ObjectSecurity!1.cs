namespace System.Security.AccessControl
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;

    public abstract class ObjectSecurity<T> : NativeObjectSecurity where T: struct
    {
        [SecuritySafeCritical]
        protected ObjectSecurity(bool isContainer, ResourceType resourceType) : base(isContainer, resourceType, (NativeObjectSecurity.ExceptionFromErrorCode) null, null)
        {
        }

        [SecuritySafeCritical]
        protected ObjectSecurity(bool isContainer, ResourceType resourceType, SafeHandle safeHandle, AccessControlSections includeSections) : base(isContainer, resourceType, safeHandle, includeSections, null, null)
        {
        }

        [SecuritySafeCritical]
        protected ObjectSecurity(bool isContainer, ResourceType resourceType, string name, AccessControlSections includeSections) : base(isContainer, resourceType, name, includeSections, null, null)
        {
        }

        [SecuritySafeCritical]
        protected ObjectSecurity(bool isContainer, ResourceType resourceType, SafeHandle safeHandle, AccessControlSections includeSections, NativeObjectSecurity.ExceptionFromErrorCode exceptionFromErrorCode, object exceptionContext) : base(isContainer, resourceType, safeHandle, includeSections, exceptionFromErrorCode, exceptionContext)
        {
        }

        [SecuritySafeCritical]
        protected ObjectSecurity(bool isContainer, ResourceType resourceType, string name, AccessControlSections includeSections, NativeObjectSecurity.ExceptionFromErrorCode exceptionFromErrorCode, object exceptionContext) : base(isContainer, resourceType, name, includeSections, exceptionFromErrorCode, exceptionContext)
        {
        }

        public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new AccessRule<T>(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, type);
        }

        public virtual void AddAccessRule(AccessRule<T> rule)
        {
            base.AddAccessRule(rule);
        }

        public virtual void AddAuditRule(AuditRule<T> rule)
        {
            base.AddAuditRule(rule);
        }

        public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new AuditRule<T>(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, flags);
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

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
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

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
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

        public virtual bool RemoveAccessRule(AccessRule<T> rule)
        {
            return base.RemoveAccessRule(rule);
        }

        public virtual void RemoveAccessRuleAll(AccessRule<T> rule)
        {
            base.RemoveAccessRuleAll(rule);
        }

        public virtual void RemoveAccessRuleSpecific(AccessRule<T> rule)
        {
            base.RemoveAccessRuleSpecific(rule);
        }

        public virtual bool RemoveAuditRule(AuditRule<T> rule)
        {
            return base.RemoveAuditRule(rule);
        }

        public virtual void RemoveAuditRuleAll(AuditRule<T> rule)
        {
            base.RemoveAuditRuleAll(rule);
        }

        public virtual void RemoveAuditRuleSpecific(AuditRule<T> rule)
        {
            base.RemoveAuditRuleSpecific(rule);
        }

        public virtual void ResetAccessRule(AccessRule<T> rule)
        {
            base.ResetAccessRule(rule);
        }

        public virtual void SetAccessRule(AccessRule<T> rule)
        {
            base.SetAccessRule(rule);
        }

        public virtual void SetAuditRule(AuditRule<T> rule)
        {
            base.SetAuditRule(rule);
        }

        public override Type AccessRightType
        {
            get
            {
                return typeof(T);
            }
        }

        public override Type AccessRuleType
        {
            get
            {
                return typeof(AccessRule<T>);
            }
        }

        public override Type AuditRuleType
        {
            get
            {
                return typeof(AuditRule<T>);
            }
        }
    }
}

