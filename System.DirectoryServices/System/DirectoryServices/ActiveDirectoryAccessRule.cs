namespace System.DirectoryServices
{
    using System;
    using System.Security.AccessControl;
    using System.Security.Principal;

    public class ActiveDirectoryAccessRule : ObjectAccessRule
    {
        public ActiveDirectoryAccessRule(IdentityReference identity, System.DirectoryServices.ActiveDirectoryRights adRights, AccessControlType type) : this(identity, ActiveDirectoryRightsTranslator.AccessMaskFromRights(adRights), type, Guid.Empty, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
        {
        }

        public ActiveDirectoryAccessRule(IdentityReference identity, System.DirectoryServices.ActiveDirectoryRights adRights, AccessControlType type, ActiveDirectorySecurityInheritance inheritanceType) : this(identity, ActiveDirectoryRightsTranslator.AccessMaskFromRights(adRights), type, Guid.Empty, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), Guid.Empty)
        {
        }

        public ActiveDirectoryAccessRule(IdentityReference identity, System.DirectoryServices.ActiveDirectoryRights adRights, AccessControlType type, Guid objectType) : this(identity, ActiveDirectoryRightsTranslator.AccessMaskFromRights(adRights), type, objectType, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
        {
        }

        public ActiveDirectoryAccessRule(IdentityReference identity, System.DirectoryServices.ActiveDirectoryRights adRights, AccessControlType type, ActiveDirectorySecurityInheritance inheritanceType, Guid inheritedObjectType) : this(identity, ActiveDirectoryRightsTranslator.AccessMaskFromRights(adRights), type, Guid.Empty, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), inheritedObjectType)
        {
        }

        public ActiveDirectoryAccessRule(IdentityReference identity, System.DirectoryServices.ActiveDirectoryRights adRights, AccessControlType type, Guid objectType, ActiveDirectorySecurityInheritance inheritanceType) : this(identity, ActiveDirectoryRightsTranslator.AccessMaskFromRights(adRights), type, objectType, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), Guid.Empty)
        {
        }

        public ActiveDirectoryAccessRule(IdentityReference identity, System.DirectoryServices.ActiveDirectoryRights adRights, AccessControlType type, Guid objectType, ActiveDirectorySecurityInheritance inheritanceType, Guid inheritedObjectType) : this(identity, ActiveDirectoryRightsTranslator.AccessMaskFromRights(adRights), type, objectType, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), inheritedObjectType)
        {
        }

        internal ActiveDirectoryAccessRule(IdentityReference identity, int accessMask, AccessControlType type, Guid objectType, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, Guid inheritedObjectType) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, objectType, inheritedObjectType, type)
        {
        }

        public System.DirectoryServices.ActiveDirectoryRights ActiveDirectoryRights
        {
            get
            {
                return ActiveDirectoryRightsTranslator.RightsFromAccessMask(base.AccessMask);
            }
        }

        public ActiveDirectorySecurityInheritance InheritanceType
        {
            get
            {
                return ActiveDirectoryInheritanceTranslator.GetEffectiveInheritanceFlags(base.InheritanceFlags, base.PropagationFlags);
            }
        }
    }
}

