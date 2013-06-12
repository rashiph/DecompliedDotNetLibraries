namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public sealed class FileSystemAccessRule : AccessRule
    {
        public FileSystemAccessRule(IdentityReference identity, System.Security.AccessControl.FileSystemRights fileSystemRights, AccessControlType type) : this(identity, AccessMaskFromRights(fileSystemRights, type), false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public FileSystemAccessRule(string identity, System.Security.AccessControl.FileSystemRights fileSystemRights, AccessControlType type) : this(new NTAccount(identity), AccessMaskFromRights(fileSystemRights, type), false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public FileSystemAccessRule(IdentityReference identity, System.Security.AccessControl.FileSystemRights fileSystemRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : this(identity, AccessMaskFromRights(fileSystemRights, type), false, inheritanceFlags, propagationFlags, type)
        {
        }

        public FileSystemAccessRule(string identity, System.Security.AccessControl.FileSystemRights fileSystemRights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : this(new NTAccount(identity), AccessMaskFromRights(fileSystemRights, type), false, inheritanceFlags, propagationFlags, type)
        {
        }

        internal FileSystemAccessRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, type)
        {
        }

        internal static int AccessMaskFromRights(System.Security.AccessControl.FileSystemRights fileSystemRights, AccessControlType controlType)
        {
            if ((fileSystemRights < 0) || (fileSystemRights > System.Security.AccessControl.FileSystemRights.FullControl))
            {
                throw new ArgumentOutOfRangeException("fileSystemRights", Environment.GetResourceString("Argument_InvalidEnumValue", new object[] { fileSystemRights, "FileSystemRights" }));
            }
            if (controlType == AccessControlType.Allow)
            {
                fileSystemRights |= System.Security.AccessControl.FileSystemRights.Synchronize;
            }
            else if (((controlType == AccessControlType.Deny) && (fileSystemRights != System.Security.AccessControl.FileSystemRights.FullControl)) && (fileSystemRights != (System.Security.AccessControl.FileSystemRights.Synchronize | System.Security.AccessControl.FileSystemRights.TakeOwnership | System.Security.AccessControl.FileSystemRights.ChangePermissions | System.Security.AccessControl.FileSystemRights.Modify)))
            {
                fileSystemRights &= ~System.Security.AccessControl.FileSystemRights.Synchronize;
            }
            return (int) fileSystemRights;
        }

        internal static System.Security.AccessControl.FileSystemRights RightsFromAccessMask(int accessMask)
        {
            return (System.Security.AccessControl.FileSystemRights) accessMask;
        }

        public System.Security.AccessControl.FileSystemRights FileSystemRights
        {
            get
            {
                return RightsFromAccessMask(base.AccessMask);
            }
        }
    }
}

