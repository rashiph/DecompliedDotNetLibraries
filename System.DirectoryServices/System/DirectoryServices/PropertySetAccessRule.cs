namespace System.DirectoryServices
{
    using System;
    using System.Security.AccessControl;
    using System.Security.Principal;

    public sealed class PropertySetAccessRule : ActiveDirectoryAccessRule
    {
        public PropertySetAccessRule(IdentityReference identity, AccessControlType type, PropertyAccess access, Guid propertySetType) : base(identity, PropertyAccessTranslator.AccessMaskFromPropertyAccess(access), type, propertySetType, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
        {
        }

        public PropertySetAccessRule(IdentityReference identity, AccessControlType type, PropertyAccess access, Guid propertySetType, ActiveDirectorySecurityInheritance inheritanceType) : base(identity, PropertyAccessTranslator.AccessMaskFromPropertyAccess(access), type, propertySetType, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), Guid.Empty)
        {
        }

        public PropertySetAccessRule(IdentityReference identity, AccessControlType type, PropertyAccess access, Guid propertySetType, ActiveDirectorySecurityInheritance inheritanceType, Guid inheritedObjectType) : base(identity, PropertyAccessTranslator.AccessMaskFromPropertyAccess(access), type, propertySetType, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), inheritedObjectType)
        {
        }
    }
}

