namespace System.Security.AccessControl
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;

    public abstract class FileSystemSecurity : NativeObjectSecurity
    {
        private const ResourceType s_ResourceType = ResourceType.FileObject;

        [SecurityCritical]
        internal FileSystemSecurity(bool isContainer) : base(isContainer, ResourceType.FileObject, new NativeObjectSecurity.ExceptionFromErrorCode(FileSystemSecurity._HandleErrorCode), isContainer)
        {
        }

        [SecurityCritical]
        internal FileSystemSecurity(bool isContainer, SafeFileHandle handle, AccessControlSections includeSections, bool isDirectory) : base(isContainer, ResourceType.FileObject, handle, includeSections, new NativeObjectSecurity.ExceptionFromErrorCode(FileSystemSecurity._HandleErrorCode), isDirectory)
        {
        }

        [SecurityCritical]
        internal FileSystemSecurity(bool isContainer, string name, AccessControlSections includeSections, bool isDirectory) : base(isContainer, ResourceType.FileObject, name, includeSections, new NativeObjectSecurity.ExceptionFromErrorCode(FileSystemSecurity._HandleErrorCode), isDirectory)
        {
        }

        [SecurityCritical]
        private static Exception _HandleErrorCode(int errorCode, string name, SafeHandle handle, object context)
        {
            Exception exception = null;
            switch (errorCode)
            {
                case 2:
                    if (((context == null) || !(context is bool)) || !((bool) context))
                    {
                        if ((name != null) && (name.Length != 0))
                        {
                            return new FileNotFoundException(name);
                        }
                        return new FileNotFoundException();
                    }
                    if ((name != null) && (name.Length != 0))
                    {
                        return new DirectoryNotFoundException(name);
                    }
                    return new DirectoryNotFoundException();

                case 6:
                    return new ArgumentException(Environment.GetResourceString("AccessControl_InvalidHandle"));

                case 0x7b:
                    exception = new ArgumentException(Environment.GetResourceString("Argument_InvalidName"), "name");
                    break;
            }
            return exception;
        }

        public sealed override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new FileSystemAccessRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, type);
        }

        public void AddAccessRule(FileSystemAccessRule rule)
        {
            base.AddAccessRule(rule);
        }

        public void AddAuditRule(FileSystemAuditRule rule)
        {
            base.AddAuditRule(rule);
        }

        public sealed override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new FileSystemAuditRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, flags);
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

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        internal void Persist(string fullPath)
        {
            new FileIOPermission(FileIOPermissionAccess.NoAccess, AccessControlActions.Change, fullPath).Demand();
            base.WriteLock();
            try
            {
                bool flag;
                bool flag2;
                AccessControlSections accessControlSectionsFromChanges = this.GetAccessControlSectionsFromChanges();
                base.Persist(fullPath, accessControlSectionsFromChanges);
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
        internal void Persist(SafeFileHandle handle, string fullPath)
        {
            if (fullPath != null)
            {
                new FileIOPermission(FileIOPermissionAccess.NoAccess, AccessControlActions.Change, fullPath).Demand();
            }
            else
            {
                new FileIOPermission(PermissionState.Unrestricted).Demand();
            }
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

        [SecuritySafeCritical]
        public bool RemoveAccessRule(FileSystemAccessRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            AuthorizationRuleCollection rules = base.GetAccessRules(true, true, rule.IdentityReference.GetType());
            for (int i = 0; i < rules.Count; i++)
            {
                FileSystemAccessRule rule2 = rules[i] as FileSystemAccessRule;
                if (((rule2 != null) && (rule2.FileSystemRights == rule.FileSystemRights)) && ((rule2.IdentityReference == rule.IdentityReference) && (rule2.AccessControlType == rule.AccessControlType)))
                {
                    return base.RemoveAccessRule(rule);
                }
            }
            FileSystemAccessRule rule3 = new FileSystemAccessRule(rule.IdentityReference, FileSystemAccessRule.AccessMaskFromRights(rule.FileSystemRights, AccessControlType.Deny), rule.IsInherited, rule.InheritanceFlags, rule.PropagationFlags, rule.AccessControlType);
            return base.RemoveAccessRule(rule3);
        }

        public void RemoveAccessRuleAll(FileSystemAccessRule rule)
        {
            base.RemoveAccessRuleAll(rule);
        }

        [SecuritySafeCritical]
        public void RemoveAccessRuleSpecific(FileSystemAccessRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            AuthorizationRuleCollection rules = base.GetAccessRules(true, true, rule.IdentityReference.GetType());
            for (int i = 0; i < rules.Count; i++)
            {
                FileSystemAccessRule rule2 = rules[i] as FileSystemAccessRule;
                if (((rule2 != null) && (rule2.FileSystemRights == rule.FileSystemRights)) && ((rule2.IdentityReference == rule.IdentityReference) && (rule2.AccessControlType == rule.AccessControlType)))
                {
                    base.RemoveAccessRuleSpecific(rule);
                    return;
                }
            }
            FileSystemAccessRule rule3 = new FileSystemAccessRule(rule.IdentityReference, FileSystemAccessRule.AccessMaskFromRights(rule.FileSystemRights, AccessControlType.Deny), rule.IsInherited, rule.InheritanceFlags, rule.PropagationFlags, rule.AccessControlType);
            base.RemoveAccessRuleSpecific(rule3);
        }

        public bool RemoveAuditRule(FileSystemAuditRule rule)
        {
            return base.RemoveAuditRule(rule);
        }

        public void RemoveAuditRuleAll(FileSystemAuditRule rule)
        {
            base.RemoveAuditRuleAll(rule);
        }

        public void RemoveAuditRuleSpecific(FileSystemAuditRule rule)
        {
            base.RemoveAuditRuleSpecific(rule);
        }

        public void ResetAccessRule(FileSystemAccessRule rule)
        {
            base.ResetAccessRule(rule);
        }

        public void SetAccessRule(FileSystemAccessRule rule)
        {
            base.SetAccessRule(rule);
        }

        public void SetAuditRule(FileSystemAuditRule rule)
        {
            base.SetAuditRule(rule);
        }

        public override Type AccessRightType
        {
            get
            {
                return typeof(FileSystemRights);
            }
        }

        public override Type AccessRuleType
        {
            get
            {
                return typeof(FileSystemAccessRule);
            }
        }

        public override Type AuditRuleType
        {
            get
            {
                return typeof(FileSystemAuditRule);
            }
        }
    }
}

