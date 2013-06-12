namespace System.Security.AccessControl
{
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    public sealed class DirectorySecurity : FileSystemSecurity
    {
        [SecuritySafeCritical]
        public DirectorySecurity() : base(true)
        {
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        public DirectorySecurity(string name, AccessControlSections includeSections) : base(true, name, includeSections, true)
        {
            string fullPathInternal = Path.GetFullPathInternal(name);
            new FileIOPermission(FileIOPermissionAccess.NoAccess, AccessControlActions.View, fullPathInternal).Demand();
        }
    }
}

