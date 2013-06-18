namespace System.DirectoryServices
{
    using System;
    using System.Security.AccessControl;
    using System.Security.Principal;

    public sealed class DeleteTreeAccessRule : ActiveDirectoryAccessRule
    {
        public DeleteTreeAccessRule(IdentityReference identity, AccessControlType type) : base(identity, 0x40, type, Guid.Empty, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
        {
        }

        public DeleteTreeAccessRule(IdentityReference identity, AccessControlType type, ActiveDirectorySecurityInheritance inheritanceType) : base(identity, 0x40, type, Guid.Empty, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), Guid.Empty)
        {
        }

        public DeleteTreeAccessRule(IdentityReference identity, AccessControlType type, ActiveDirectorySecurityInheritance inheritanceType, Guid inheritedObjectType) : base(identity, 0x40, type, Guid.Empty, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), inheritedObjectType)
        {
        }
    }
}

