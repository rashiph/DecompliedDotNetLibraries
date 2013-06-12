namespace System.Security.AccessControl
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;
    using System.Threading;

    public sealed class MutexSecurity : NativeObjectSecurity
    {
        public MutexSecurity() : base(true, ResourceType.KernelObject)
        {
        }

        [SecurityCritical]
        internal MutexSecurity(SafeWaitHandle handle, AccessControlSections includeSections) : base(true, ResourceType.KernelObject, handle, includeSections, new NativeObjectSecurity.ExceptionFromErrorCode(MutexSecurity._HandleErrorCode), null)
        {
        }

        [SecuritySafeCritical]
        public MutexSecurity(string name, AccessControlSections includeSections) : base(true, ResourceType.KernelObject, name, includeSections, new NativeObjectSecurity.ExceptionFromErrorCode(MutexSecurity._HandleErrorCode), null)
        {
        }

        [SecurityCritical]
        private static Exception _HandleErrorCode(int errorCode, string name, SafeHandle handle, object context)
        {
            Exception exception = null;
            int num = errorCode;
            if (((num != 2) && (num != 6)) && (num != 0x7b))
            {
                return exception;
            }
            if ((name != null) && (name.Length != 0))
            {
                return new WaitHandleCannotBeOpenedException(Environment.GetResourceString("Threading.WaitHandleCannotBeOpenedException_InvalidHandle", new object[] { name }));
            }
            return new WaitHandleCannotBeOpenedException();
        }

        public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new MutexAccessRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, type);
        }

        public void AddAccessRule(MutexAccessRule rule)
        {
            base.AddAccessRule(rule);
        }

        public void AddAuditRule(MutexAuditRule rule)
        {
            base.AddAuditRule(rule);
        }

        public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new MutexAuditRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, flags);
        }

        internal AccessControlSections GetAccessControlSectionsFromChanges()
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

        [SecurityCritical]
        internal void Persist(SafeWaitHandle handle)
        {
            base.WriteLock();
            try
            {
                AccessControlSections accessControlSectionsFromChanges = this.GetAccessControlSectionsFromChanges();
                if (accessControlSectionsFromChanges != AccessControlSections.None)
                {
                    bool flag;
                    bool flag2;
                    base.Persist(handle, accessControlSectionsFromChanges);
                    base.AccessRulesModified = flag = false;
                    base.AuditRulesModified = flag2 = flag;
                    base.OwnerModified = base.GroupModified = flag2;
                }
            }
            finally
            {
                base.WriteUnlock();
            }
        }

        public bool RemoveAccessRule(MutexAccessRule rule)
        {
            return base.RemoveAccessRule(rule);
        }

        public void RemoveAccessRuleAll(MutexAccessRule rule)
        {
            base.RemoveAccessRuleAll(rule);
        }

        public void RemoveAccessRuleSpecific(MutexAccessRule rule)
        {
            base.RemoveAccessRuleSpecific(rule);
        }

        public bool RemoveAuditRule(MutexAuditRule rule)
        {
            return base.RemoveAuditRule(rule);
        }

        public void RemoveAuditRuleAll(MutexAuditRule rule)
        {
            base.RemoveAuditRuleAll(rule);
        }

        public void RemoveAuditRuleSpecific(MutexAuditRule rule)
        {
            base.RemoveAuditRuleSpecific(rule);
        }

        public void ResetAccessRule(MutexAccessRule rule)
        {
            base.ResetAccessRule(rule);
        }

        public void SetAccessRule(MutexAccessRule rule)
        {
            base.SetAccessRule(rule);
        }

        public void SetAuditRule(MutexAuditRule rule)
        {
            base.SetAuditRule(rule);
        }

        public override Type AccessRightType
        {
            get
            {
                return typeof(MutexRights);
            }
        }

        public override Type AccessRuleType
        {
            get
            {
                return typeof(MutexAccessRule);
            }
        }

        public override Type AuditRuleType
        {
            get
            {
                return typeof(MutexAuditRule);
            }
        }
    }
}

