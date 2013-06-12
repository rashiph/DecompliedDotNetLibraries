namespace System.ComponentModel
{
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    internal static class IntSecurity
    {
        public static readonly CodeAccessPermission FullReflection = new ReflectionPermission(PermissionState.Unrestricted);
        public static readonly CodeAccessPermission UnmanagedCode = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);

        public static string UnsafeGetFullPath(string fileName)
        {
            string fullPath = fileName;
            new FileIOPermission(PermissionState.None) { AllFiles = FileIOPermissionAccess.PathDiscovery }.Assert();
            try
            {
                fullPath = Path.GetFullPath(fileName);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return fullPath;
        }
    }
}

