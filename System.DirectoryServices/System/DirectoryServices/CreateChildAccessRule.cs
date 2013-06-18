namespace System.DirectoryServices
{
    using System;
    using System.Security.AccessControl;
    using System.Security.Principal;

    public sealed class CreateChildAccessRule : ActiveDirectoryAccessRule
    {
        public CreateChildAccessRule(IdentityReference identity, AccessControlType type) : base(identity, 1, type, Guid.Empty, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
        {
        }

        public CreateChildAccessRule(IdentityReference identity, AccessControlType type, ActiveDirectorySecurityInheritance inheritanceType) : base(identity, 1, type, Guid.Empty, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), Guid.Empty)
        {
        }

        public CreateChildAccessRule(IdentityReference identity, AccessControlType type, Guid childType) : base(identity, 1, type, childType, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
        {
        }

        public CreateChildAccessRule(IdentityReference identity, AccessControlType type, ActiveDirectorySecurityInheritance inheritanceType, Guid inheritedObjectType) : base(identity, 1, type, Guid.Empty, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), inheritedObjectType)
        {
        }

        public CreateChildAccessRule(IdentityReference identity, AccessControlType type, Guid childType, ActiveDirectorySecurityInheritance inheritanceType) : base(identity, 1, type, childType, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), Guid.Empty)
        {
        }

        public CreateChildAccessRule(IdentityReference identity, AccessControlType type, Guid childType, ActiveDirectorySecurityInheritance inheritanceType, Guid inheritedObjectType) : base(identity, 1, type, childType, false, ActiveDirectoryInheritanceTranslator.GetInheritanceFlags(inheritanceType), ActiveDirectoryInheritanceTranslator.GetPropagationFlags(inheritanceType), inheritedObjectType)
        {
        }
    }
}

