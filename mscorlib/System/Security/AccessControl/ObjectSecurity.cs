namespace System.Security.AccessControl
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;
    using System.Threading;

    public abstract class ObjectSecurity
    {
        private bool _daclModified;
        private bool _groupModified;
        private readonly ReaderWriterLock _lock;
        private bool _ownerModified;
        private bool _saclModified;
        internal CommonSecurityDescriptor _securityDescriptor;
        private static readonly ControlFlags DACL_CONTROL_FLAGS = (ControlFlags.DiscretionaryAclProtected | ControlFlags.DiscretionaryAclAutoInherited | ControlFlags.DiscretionaryAclPresent);
        private static readonly ControlFlags SACL_CONTROL_FLAGS = (ControlFlags.SystemAclProtected | ControlFlags.SystemAclAutoInherited | ControlFlags.SystemAclPresent);

        private ObjectSecurity()
        {
            this._lock = new ReaderWriterLock();
        }

        internal ObjectSecurity(CommonSecurityDescriptor securityDescriptor) : this()
        {
            if (securityDescriptor == null)
            {
                throw new ArgumentNullException("securityDescriptor");
            }
            this._securityDescriptor = securityDescriptor;
        }

        protected ObjectSecurity(bool isContainer, bool isDS) : this()
        {
            DiscretionaryAcl discretionaryAcl = new DiscretionaryAcl(isContainer, isDS, 5);
            this._securityDescriptor = new CommonSecurityDescriptor(isContainer, isDS, ControlFlags.None, null, null, null, discretionaryAcl);
        }

        public abstract AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type);
        public abstract AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags);
        public IdentityReference GetGroup(Type targetType)
        {
            IdentityReference reference;
            this.ReadLock();
            try
            {
                if (this._securityDescriptor.Group == null)
                {
                    return null;
                }
                reference = this._securityDescriptor.Group.Translate(targetType);
            }
            finally
            {
                this.ReadUnlock();
            }
            return reference;
        }

        public IdentityReference GetOwner(Type targetType)
        {
            IdentityReference reference;
            this.ReadLock();
            try
            {
                if (this._securityDescriptor.Owner == null)
                {
                    return null;
                }
                reference = this._securityDescriptor.Owner.Translate(targetType);
            }
            finally
            {
                this.ReadUnlock();
            }
            return reference;
        }

        public byte[] GetSecurityDescriptorBinaryForm()
        {
            byte[] buffer2;
            this.ReadLock();
            try
            {
                byte[] binaryForm = new byte[this._securityDescriptor.BinaryLength];
                this._securityDescriptor.GetBinaryForm(binaryForm, 0);
                buffer2 = binaryForm;
            }
            finally
            {
                this.ReadUnlock();
            }
            return buffer2;
        }

        [SecuritySafeCritical]
        public string GetSecurityDescriptorSddlForm(AccessControlSections includeSections)
        {
            string sddlForm;
            this.ReadLock();
            try
            {
                sddlForm = this._securityDescriptor.GetSddlForm(includeSections);
            }
            finally
            {
                this.ReadUnlock();
            }
            return sddlForm;
        }

        public static bool IsSddlConversionSupported()
        {
            return true;
        }

        protected abstract bool ModifyAccess(AccessControlModification modification, AccessRule rule, out bool modified);
        public virtual bool ModifyAccessRule(AccessControlModification modification, AccessRule rule, out bool modified)
        {
            bool flag;
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            if (!this.AccessRuleType.IsAssignableFrom(rule.GetType()))
            {
                throw new ArgumentException(Environment.GetResourceString("AccessControl_InvalidAccessRuleType"), "rule");
            }
            this.WriteLock();
            try
            {
                flag = this.ModifyAccess(modification, rule, out modified);
            }
            finally
            {
                this.WriteUnlock();
            }
            return flag;
        }

        protected abstract bool ModifyAudit(AccessControlModification modification, AuditRule rule, out bool modified);
        public virtual bool ModifyAuditRule(AccessControlModification modification, AuditRule rule, out bool modified)
        {
            bool flag;
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            if (!this.AuditRuleType.IsAssignableFrom(rule.GetType()))
            {
                throw new ArgumentException(Environment.GetResourceString("AccessControl_InvalidAuditRuleType"), "rule");
            }
            this.WriteLock();
            try
            {
                flag = this.ModifyAudit(modification, rule, out modified);
            }
            finally
            {
                this.WriteUnlock();
            }
            return flag;
        }

        [SecuritySafeCritical]
        protected virtual void Persist(SafeHandle handle, AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        protected virtual void Persist(string name, AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        [HandleProcessCorruptedStateExceptions, SecuritySafeCritical]
        protected virtual void Persist(bool enableOwnershipPrivilege, string name, AccessControlSections includeSections)
        {
            Privilege privilege = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (enableOwnershipPrivilege)
                {
                    privilege = new Privilege("SeTakeOwnershipPrivilege");
                    try
                    {
                        privilege.Enable();
                    }
                    catch (PrivilegeNotHeldException)
                    {
                    }
                }
                this.Persist(name, includeSections);
            }
            catch
            {
                if (privilege != null)
                {
                    privilege.Revert();
                }
                throw;
            }
            finally
            {
                if (privilege != null)
                {
                    privilege.Revert();
                }
            }
        }

        public virtual void PurgeAccessRules(IdentityReference identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            this.WriteLock();
            try
            {
                this._securityDescriptor.PurgeAccessControl(identity.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier);
                this._daclModified = true;
            }
            finally
            {
                this.WriteUnlock();
            }
        }

        public virtual void PurgeAuditRules(IdentityReference identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            this.WriteLock();
            try
            {
                this._securityDescriptor.PurgeAudit(identity.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier);
                this._saclModified = true;
            }
            finally
            {
                this.WriteUnlock();
            }
        }

        protected void ReadLock()
        {
            this._lock.AcquireReaderLock(-1);
        }

        protected void ReadUnlock()
        {
            this._lock.ReleaseReaderLock();
        }

        public void SetAccessRuleProtection(bool isProtected, bool preserveInheritance)
        {
            this.WriteLock();
            try
            {
                this._securityDescriptor.SetDiscretionaryAclProtection(isProtected, preserveInheritance);
                this._daclModified = true;
            }
            finally
            {
                this.WriteUnlock();
            }
        }

        public void SetAuditRuleProtection(bool isProtected, bool preserveInheritance)
        {
            this.WriteLock();
            try
            {
                this._securityDescriptor.SetSystemAclProtection(isProtected, preserveInheritance);
                this._saclModified = true;
            }
            finally
            {
                this.WriteUnlock();
            }
        }

        public void SetGroup(IdentityReference identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            this.WriteLock();
            try
            {
                this._securityDescriptor.Group = identity.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
                this._groupModified = true;
            }
            finally
            {
                this.WriteUnlock();
            }
        }

        public void SetOwner(IdentityReference identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            this.WriteLock();
            try
            {
                this._securityDescriptor.Owner = identity.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
                this._ownerModified = true;
            }
            finally
            {
                this.WriteUnlock();
            }
        }

        public void SetSecurityDescriptorBinaryForm(byte[] binaryForm)
        {
            this.SetSecurityDescriptorBinaryForm(binaryForm, AccessControlSections.All);
        }

        public void SetSecurityDescriptorBinaryForm(byte[] binaryForm, AccessControlSections includeSections)
        {
            if (binaryForm == null)
            {
                throw new ArgumentNullException("binaryForm");
            }
            if ((includeSections & AccessControlSections.All) == AccessControlSections.None)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumAtLeastOneFlag"), "includeSections");
            }
            this.WriteLock();
            try
            {
                this.UpdateWithNewSecurityDescriptor(new RawSecurityDescriptor(binaryForm, 0), includeSections);
            }
            finally
            {
                this.WriteUnlock();
            }
        }

        [SecuritySafeCritical]
        public void SetSecurityDescriptorSddlForm(string sddlForm)
        {
            this.SetSecurityDescriptorSddlForm(sddlForm, AccessControlSections.All);
        }

        [SecuritySafeCritical]
        public void SetSecurityDescriptorSddlForm(string sddlForm, AccessControlSections includeSections)
        {
            if (sddlForm == null)
            {
                throw new ArgumentNullException("sddlForm");
            }
            if ((includeSections & AccessControlSections.All) == AccessControlSections.None)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumAtLeastOneFlag"), "includeSections");
            }
            this.WriteLock();
            try
            {
                this.UpdateWithNewSecurityDescriptor(new RawSecurityDescriptor(sddlForm), includeSections);
            }
            finally
            {
                this.WriteUnlock();
            }
        }

        private void UpdateWithNewSecurityDescriptor(RawSecurityDescriptor newOne, AccessControlSections includeSections)
        {
            if ((includeSections & AccessControlSections.Owner) != AccessControlSections.None)
            {
                this._ownerModified = true;
                this._securityDescriptor.Owner = newOne.Owner;
            }
            if ((includeSections & AccessControlSections.Group) != AccessControlSections.None)
            {
                this._groupModified = true;
                this._securityDescriptor.Group = newOne.Group;
            }
            if ((includeSections & AccessControlSections.Audit) != AccessControlSections.None)
            {
                this._saclModified = true;
                if (newOne.SystemAcl != null)
                {
                    this._securityDescriptor.SystemAcl = new SystemAcl(this.IsContainer, this.IsDS, newOne.SystemAcl, true);
                }
                else
                {
                    this._securityDescriptor.SystemAcl = null;
                }
                this._securityDescriptor.UpdateControlFlags(SACL_CONTROL_FLAGS, newOne.ControlFlags & SACL_CONTROL_FLAGS);
            }
            if ((includeSections & AccessControlSections.Access) != AccessControlSections.None)
            {
                this._daclModified = true;
                if (newOne.DiscretionaryAcl != null)
                {
                    this._securityDescriptor.DiscretionaryAcl = new DiscretionaryAcl(this.IsContainer, this.IsDS, newOne.DiscretionaryAcl, true);
                }
                else
                {
                    this._securityDescriptor.DiscretionaryAcl = null;
                }
                ControlFlags flags = this._securityDescriptor.ControlFlags & ControlFlags.DiscretionaryAclPresent;
                this._securityDescriptor.UpdateControlFlags(DACL_CONTROL_FLAGS, (newOne.ControlFlags | flags) & DACL_CONTROL_FLAGS);
            }
        }

        protected void WriteLock()
        {
            this._lock.AcquireWriterLock(-1);
        }

        protected void WriteUnlock()
        {
            this._lock.ReleaseWriterLock();
        }

        public abstract Type AccessRightType { get; }

        protected bool AccessRulesModified
        {
            get
            {
                if (!this._lock.IsReaderLockHeld && !this._lock.IsWriterLockHeld)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustLockForReadOrWrite"));
                }
                return this._daclModified;
            }
            set
            {
                if (!this._lock.IsWriterLockHeld)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustLockForWrite"));
                }
                this._daclModified = value;
            }
        }

        public abstract Type AccessRuleType { get; }

        public bool AreAccessRulesCanonical
        {
            get
            {
                bool isDiscretionaryAclCanonical;
                this.ReadLock();
                try
                {
                    isDiscretionaryAclCanonical = this._securityDescriptor.IsDiscretionaryAclCanonical;
                }
                finally
                {
                    this.ReadUnlock();
                }
                return isDiscretionaryAclCanonical;
            }
        }

        public bool AreAccessRulesProtected
        {
            get
            {
                bool flag;
                this.ReadLock();
                try
                {
                    flag = (this._securityDescriptor.ControlFlags & ControlFlags.DiscretionaryAclProtected) != ControlFlags.None;
                }
                finally
                {
                    this.ReadUnlock();
                }
                return flag;
            }
        }

        public bool AreAuditRulesCanonical
        {
            get
            {
                bool isSystemAclCanonical;
                this.ReadLock();
                try
                {
                    isSystemAclCanonical = this._securityDescriptor.IsSystemAclCanonical;
                }
                finally
                {
                    this.ReadUnlock();
                }
                return isSystemAclCanonical;
            }
        }

        public bool AreAuditRulesProtected
        {
            get
            {
                bool flag;
                this.ReadLock();
                try
                {
                    flag = (this._securityDescriptor.ControlFlags & ControlFlags.SystemAclProtected) != ControlFlags.None;
                }
                finally
                {
                    this.ReadUnlock();
                }
                return flag;
            }
        }

        protected bool AuditRulesModified
        {
            get
            {
                if (!this._lock.IsReaderLockHeld && !this._lock.IsWriterLockHeld)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustLockForReadOrWrite"));
                }
                return this._saclModified;
            }
            set
            {
                if (!this._lock.IsWriterLockHeld)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustLockForWrite"));
                }
                this._saclModified = value;
            }
        }

        public abstract Type AuditRuleType { get; }

        protected bool GroupModified
        {
            get
            {
                if (!this._lock.IsReaderLockHeld && !this._lock.IsWriterLockHeld)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustLockForReadOrWrite"));
                }
                return this._groupModified;
            }
            set
            {
                if (!this._lock.IsWriterLockHeld)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustLockForWrite"));
                }
                this._groupModified = value;
            }
        }

        protected bool IsContainer
        {
            get
            {
                return this._securityDescriptor.IsContainer;
            }
        }

        protected bool IsDS
        {
            get
            {
                return this._securityDescriptor.IsDS;
            }
        }

        protected bool OwnerModified
        {
            get
            {
                if (!this._lock.IsReaderLockHeld && !this._lock.IsWriterLockHeld)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustLockForReadOrWrite"));
                }
                return this._ownerModified;
            }
            set
            {
                if (!this._lock.IsWriterLockHeld)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustLockForWrite"));
                }
                this._ownerModified = value;
            }
        }
    }
}

