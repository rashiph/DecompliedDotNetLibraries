namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public sealed class RegistryAccessRule : AccessRule
    {
        public RegistryAccessRule(IdentityReference identity, System.Security.AccessControl.RegistryRights registryRights, AccessControlType type) : this(identity, (int) registryRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public RegistryAccessRule(string identity, System.Security.AccessControl.RegistryRights registryRights, AccessControlType type) : this(new NTAccount(identity), (int) registryRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public RegistryAccessRule(IdentityReference identity, System.Security.AccessControl.RegistryRights registryRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : this(identity, (int) registryRights, false, inheritanceFlags, propagationFlags, type)
        {
        }

        public RegistryAccessRule(string identity, System.Security.AccessControl.RegistryRights registryRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : this(new NTAccount(identity), (int) registryRights, false, inheritanceFlags, propagationFlags, type)
        {
        }

        internal RegistryAccessRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, type)
        {
        }

        public System.Security.AccessControl.RegistryRights RegistryRights
        {
            get
            {
                return (System.Security.AccessControl.RegistryRights) base.AccessMask;
            }
        }
    }
}

