namespace System.Security.AccessControl
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Threading;

    [ComVisible(false)]
    public sealed class SemaphoreSecurity : NativeObjectSecurity
    {
        public SemaphoreSecurity() : base(true, ResourceType.KernelObject)
        {
        }

        internal SemaphoreSecurity(SafeWaitHandle handle, AccessControlSections includeSections) : base(true, ResourceType.KernelObject, handle, includeSections, new NativeObjectSecurity.ExceptionFromErrorCode(SemaphoreSecurity._HandleErrorCode), null)
        {
        }

        public SemaphoreSecurity(string name, AccessControlSections includeSections) : base(true, ResourceType.KernelObject, name, includeSections, new NativeObjectSecurity.ExceptionFromErrorCode(SemaphoreSecurity._HandleErrorCode), null)
        {
        }

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
                return new WaitHandleCannotBeOpenedException(SR.GetString("WaitHandleCannotBeOpenedException_InvalidHandle", new object[] { name }));
            }
            return new WaitHandleCannotBeOpenedException();
        }

        public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new SemaphoreAccessRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, type);
        }

        public void AddAccessRule(SemaphoreAccessRule rule)
        {
            base.AddAccessRule(rule);
        }

        public void AddAuditRule(SemaphoreAuditRule rule)
        {
            base.AddAuditRule(rule);
        }

        public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new SemaphoreAuditRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, flags);
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

        public bool RemoveAccessRule(SemaphoreAccessRule rule)
        {
            return base.RemoveAccessRule(rule);
        }

        public void RemoveAccessRuleAll(SemaphoreAccessRule rule)
        {
            base.RemoveAccessRuleAll(rule);
        }

        public void RemoveAccessRuleSpecific(SemaphoreAccessRule rule)
        {
            base.RemoveAccessRuleSpecific(rule);
        }

        public bool RemoveAuditRule(SemaphoreAuditRule rule)
        {
            return base.RemoveAuditRule(rule);
        }

        public void RemoveAuditRuleAll(SemaphoreAuditRule rule)
        {
            base.RemoveAuditRuleAll(rule);
        }

        public void RemoveAuditRuleSpecific(SemaphoreAuditRule rule)
        {
            base.RemoveAuditRuleSpecific(rule);
        }

        public void ResetAccessRule(SemaphoreAccessRule rule)
        {
            base.ResetAccessRule(rule);
        }

        public void SetAccessRule(SemaphoreAccessRule rule)
        {
            base.SetAccessRule(rule);
        }

        public void SetAuditRule(SemaphoreAuditRule rule)
        {
            base.SetAuditRule(rule);
        }

        public override Type AccessRightType
        {
            get
            {
                return typeof(SemaphoreRights);
            }
        }

        public override Type AccessRuleType
        {
            get
            {
                return typeof(SemaphoreAccessRule);
            }
        }

        public override Type AuditRuleType
        {
            get
            {
                return typeof(SemaphoreAuditRule);
            }
        }
    }
}

