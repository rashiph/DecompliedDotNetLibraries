namespace System.ServiceModel.Activation
{
    using Microsoft.Win32;
    using System;
    using System.Security;
    using System.Security.Permissions;

    internal static class Iis7Helper
    {
        private static int iisVersion;
        private static bool isIis7 = GetIsIis7();
        private const string subKey = @"Software\Microsoft\InetSTP";

        [SecuritySafeCritical]
        private static bool GetIsIis7()
        {
            iisVersion = -1;
            object obj2 = UnsafeGetMajorVersionFromRegistry();
            if ((obj2 != null) && obj2.GetType().Equals(typeof(int)))
            {
                iisVersion = (int) obj2;
            }
            return (iisVersion >= 7);
        }

        [SecurityCritical, RegistryPermission(SecurityAction.Assert, Read=@"HKEY_LOCAL_MACHINE\Software\Microsoft\InetSTP")]
        private static object UnsafeGetMajorVersionFromRegistry()
        {
            object obj2;
            using (RegistryKey key = Registry.LocalMachine)
            {
                using (RegistryKey key2 = key.OpenSubKey(@"Software\Microsoft\InetSTP"))
                {
                    obj2 = (key2 != null) ? key2.GetValue("MajorVersion") : null;
                }
            }
            return obj2;
        }

        internal static int IisVersion
        {
            get
            {
                return iisVersion;
            }
        }

        internal static bool IsIis7
        {
            get
            {
                return isIis7;
            }
        }
    }
}

