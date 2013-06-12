namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public sealed class FileSystemAuditRule : AuditRule
    {
        public FileSystemAuditRule(IdentityReference identity, System.Security.AccessControl.FileSystemRights fileSystemRights, AuditFlags flags) : this(identity, fileSystemRights, InheritanceFlags.None, PropagationFlags.None, flags)
        {
        }

        public FileSystemAuditRule(string identity, System.Security.AccessControl.FileSystemRights fileSystemRights, AuditFlags flags) : this(new NTAccount(identity), fileSystemRights, InheritanceFlags.None, PropagationFlags.None, flags)
        {
        }

        public FileSystemAuditRule(IdentityReference identity, System.Security.AccessControl.FileSystemRights fileSystemRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags) : this(identity, AccessMaskFromRights(fileSystemRights), false, inheritanceFlags, propagationFlags, flags)
        {
        }

        public FileSystemAuditRule(string identity, System.Security.AccessControl.FileSystemRights fileSystemRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags) : this(new NTAccount(identity), AccessMaskFromRights(fileSystemRights), false, inheritanceFlags, propagationFlags, flags)
        {
        }

        internal FileSystemAuditRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, flags)
        {
        }

        private static int AccessMaskFromRights(System.Security.AccessControl.FileSystemRights fileSystemRights)
        {
            if ((fileSystemRights < 0) || (fileSystemRights > System.Security.AccessControl.FileSystemRights.FullControl))
            {
                throw new ArgumentOutOfRangeException("fileSystemRights", Environment.GetResourceString("Argument_InvalidEnumValue", new object[] { fileSystemRights, "FileSystemRights" }));
            }
            return (int) fileSystemRights;
        }

        public System.Security.AccessControl.FileSystemRights FileSystemRights
        {
            get
            {
                return FileSystemAccessRule.RightsFromAccessMask(base.AccessMask);
            }
        }
    }
}

