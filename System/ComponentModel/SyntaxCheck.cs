namespace System.ComponentModel
{
    using System;
    using System.IO;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public static class SyntaxCheck
    {
        public static bool CheckMachineName(string value)
        {
            if (value == null)
            {
                return false;
            }
            value = value.Trim();
            if (value.Equals(string.Empty))
            {
                return false;
            }
            return (value.IndexOf('\\') == -1);
        }

        public static bool CheckPath(string value)
        {
            if (value == null)
            {
                return false;
            }
            value = value.Trim();
            if (value.Equals(string.Empty))
            {
                return false;
            }
            return value.StartsWith(@"\\");
        }

        public static bool CheckRootedPath(string value)
        {
            if (value == null)
            {
                return false;
            }
            value = value.Trim();
            if (value.Equals(string.Empty))
            {
                return false;
            }
            return Path.IsPathRooted(value);
        }
    }
}

