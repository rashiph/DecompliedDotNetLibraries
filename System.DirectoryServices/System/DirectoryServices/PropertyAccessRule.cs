namespace System.DirectoryServices
{
    using System;
    using System.Security.AccessControl;
    using System.Security.Principal;

    public sealed class PropertyAccessRule : ActiveDirectoryAccessRule
    {
        public PropertyAccessRule(IdentityReference identity, AccessControlType type, PropertyAccess access) : base(identity, PropertyAccessTranslator.AccessMaskFromPropertyAccess(access), type, Guid.Empty, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
        {
        }

        public PropertyAccessRule(IdentityReference identity, AccessControlType type, PropertyAccess access, ActiveDirectorySecurityInheritance inheritanceType) : base(identity, PropertyAccessTranslator.AccessMaskFromPropertyAccess(access), type, Guid.Empty, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), Guid.Empty)
        {
        }

        public PropertyAccessRule(IdentityReference identity, AccessControlType type, PropertyAccess access, Guid propertyType) : base(identity, PropertyAccessTranslator.AccessMaskFromPropertyAccess(access), type, propertyType, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
        {
        }

        public PropertyAccessRule(IdentityReference identity, AccessControlType type, PropertyAccess access, ActiveDirectorySecurityInheritance inheritanceType, Guid inheritedObjectType) : base(identity, PropertyAccessTranslator.AccessMaskFromPropertyAccess(access), type, Guid.Empty, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), inheritedObjectType)
        {
        }

        public PropertyAccessRule(IdentityReference identity, AccessControlType type, PropertyAccess access, Guid propertyType, ActiveDirectorySecurityInheritance inheritanceType) : base(identity, PropertyAccessTranslator.AccessMaskFromPropertyAccess(access), type, propertyType, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), Guid.Empty)
        {
        }

        public PropertyAccessRule(IdentityReference identity, AccessControlType type, PropertyAccess access, Guid propertyType, ActiveDirectorySecurityInheritance inheritanceType, Guid inheritedObjectType) : base(identity, PropertyAccessTranslator.AccessMaskFromPropertyAccess(access), type, propertyType, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), inheritedObjectType)
        {
        }
    }
}

