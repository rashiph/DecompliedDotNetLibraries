namespace System.Security.AccessControl
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;

    public sealed class RegistrySecurity : NativeObjectSecurity
    {
        public RegistrySecurity() : base(true, ResourceType.RegistryKey)
        {
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        internal RegistrySecurity(SafeRegistryHandle hKey, string name, AccessControlSections includeSections) : base(true, ResourceType.RegistryKey, hKey, includeSections, new NativeObjectSecurity.ExceptionFromErrorCode(RegistrySecurity._HandleErrorCode), null)
        {
            new RegistryPermission(RegistryPermissionAccess.NoAccess, AccessControlActions.View, name).Demand();
        }

        [SecurityCritical]
        private static Exception _HandleErrorCode(int errorCode, string name, SafeHandle handle, object context)
        {
            switch (errorCode)
            {
                case 2:
                    return new IOException(Environment.GetResourceString("Arg_RegKeyNotFound", new object[] { errorCode }));

                case 6:
                    return new ArgumentException(Environment.GetResourceString("AccessControl_InvalidHandle"));

                case 0x7b:
                    return new ArgumentException(Environment.GetResourceString("Arg_RegInvalidKeyName", new object[] { "name" }));
            }
            return null;
        }

        public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new RegistryAccessRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, type);
        }

        public void AddAccessRule(RegistryAccessRule rule)
        {
            base.AddAccessRule(rule);
        }

        public void AddAuditRule(RegistryAuditRule rule)
        {
            base.AddAuditRule(rule);
        }

        public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new RegistryAuditRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, flags);
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
        internal void Persist(SafeRegistryHandle hKey, string keyName)
        {
            new RegistryPermission(RegistryPermissionAccess.NoAccess, AccessControlActions.Change, keyName).Demand();
            base.WriteLock();
            try
            {
                AccessControlSections accessControlSectionsFromChanges = this.GetAccessControlSectionsFromChanges();
                if (accessControlSectionsFromChanges != AccessControlSections.None)
                {
                    bool flag;
                    bool flag2;
                    base.Persist(hKey, accessControlSectionsFromChanges);
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

        public bool RemoveAccessRule(RegistryAccessRule rule)
        {
            return base.RemoveAccessRule(rule);
        }

        public void RemoveAccessRuleAll(RegistryAccessRule rule)
        {
            base.RemoveAccessRuleAll(rule);
        }

        public void RemoveAccessRuleSpecific(RegistryAccessRule rule)
        {
            base.RemoveAccessRuleSpecific(rule);
        }

        public bool RemoveAuditRule(RegistryAuditRule rule)
        {
            return base.RemoveAuditRule(rule);
        }

        public void RemoveAuditRuleAll(RegistryAuditRule rule)
        {
            base.RemoveAuditRuleAll(rule);
        }

        public void RemoveAuditRuleSpecific(RegistryAuditRule rule)
        {
            base.RemoveAuditRuleSpecific(rule);
        }

        public void ResetAccessRule(RegistryAccessRule rule)
        {
            base.ResetAccessRule(rule);
        }

        public void SetAccessRule(RegistryAccessRule rule)
        {
            base.SetAccessRule(rule);
        }

        public void SetAuditRule(RegistryAuditRule rule)
        {
            base.SetAuditRule(rule);
        }

        public override Type AccessRightType
        {
            get
            {
                return typeof(RegistryRights);
            }
        }

        public override Type AccessRuleType
        {
            get
            {
                return typeof(RegistryAccessRule);
            }
        }

        public override Type AuditRuleType
        {
            get
            {
                return typeof(RegistryAuditRule);
            }
        }
    }
}

