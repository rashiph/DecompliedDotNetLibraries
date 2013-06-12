namespace System.Security.AccessControl
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    public sealed class FileSecurity : FileSystemSecurity
    {
        [SecuritySafeCritical]
        public FileSecurity() : base(false)
        {
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        public FileSecurity(string fileName, AccessControlSections includeSections) : base(false, fileName, includeSections, false)
        {
            string fullPathInternal = Path.GetFullPathInternal(fileName);
            new FileIOPermission(FileIOPermissionAccess.NoAccess, AccessControlActions.View, fullPathInternal).Demand();
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        internal FileSecurity(SafeFileHandle handle, string fullPath, AccessControlSections includeSections) : base(false, handle, includeSections, false)
        {
            if (fullPath != null)
            {
                new FileIOPermission(FileIOPermissionAccess.NoAccess, AccessControlActions.View, fullPath).Demand();
            }
            else
            {
                new FileIOPermission(PermissionState.Unrestricted).Demand();
            }
        }
    }
}

