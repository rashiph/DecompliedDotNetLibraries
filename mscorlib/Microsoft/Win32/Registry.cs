namespace Microsoft.Win32
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public static class Registry
    {
        public static readonly RegistryKey ClassesRoot = RegistryKey.GetBaseKey(RegistryKey.HKEY_CLASSES_ROOT);
        public static readonly RegistryKey CurrentConfig = RegistryKey.GetBaseKey(RegistryKey.HKEY_CURRENT_CONFIG);
        public static readonly RegistryKey CurrentUser = RegistryKey.GetBaseKey(RegistryKey.HKEY_CURRENT_USER);
        [Obsolete("The DynData registry key only works on Win9x, which is no longer supported by the CLR.  On NT-based operating systems, use the PerformanceData registry key instead.")]
        public static readonly RegistryKey DynData = RegistryKey.GetBaseKey(RegistryKey.HKEY_DYN_DATA);
        public static readonly RegistryKey LocalMachine = RegistryKey.GetBaseKey(RegistryKey.HKEY_LOCAL_MACHINE);
        public static readonly RegistryKey PerformanceData = RegistryKey.GetBaseKey(RegistryKey.HKEY_PERFORMANCE_DATA);
        public static readonly RegistryKey Users = RegistryKey.GetBaseKey(RegistryKey.HKEY_USERS);

        [SecurityCritical]
        private static RegistryKey GetBaseKeyFromKeyName(string keyName, out string subKeyName)
        {
            string str;
            if (keyName == null)
            {
                throw new ArgumentNullException("keyName");
            }
            int index = keyName.IndexOf('\\');
            if (index != -1)
            {
                str = keyName.Substring(0, index).ToUpper(CultureInfo.InvariantCulture);
            }
            else
            {
                str = keyName.ToUpper(CultureInfo.InvariantCulture);
            }
            RegistryKey currentUser = null;
            switch (str)
            {
                case "HKEY_CURRENT_USER":
                    currentUser = CurrentUser;
                    break;

                case "HKEY_LOCAL_MACHINE":
                    currentUser = LocalMachine;
                    break;

                case "HKEY_CLASSES_ROOT":
                    currentUser = ClassesRoot;
                    break;

                case "HKEY_USERS":
                    currentUser = Users;
                    break;

                case "HKEY_PERFORMANCE_DATA":
                    currentUser = PerformanceData;
                    break;

                case "HKEY_CURRENT_CONFIG":
                    currentUser = CurrentConfig;
                    break;

                case "HKEY_DYN_DATA":
                    currentUser = RegistryKey.GetBaseKey(RegistryKey.HKEY_DYN_DATA);
                    break;

                default:
                    throw new ArgumentException(Environment.GetResourceString("Arg_RegInvalidKeyName", new object[] { "keyName" }));
            }
            if ((index == -1) || (index == keyName.Length))
            {
                subKeyName = string.Empty;
                return currentUser;
            }
            subKeyName = keyName.Substring(index + 1, (keyName.Length - index) - 1);
            return currentUser;
        }

        [SecuritySafeCritical]
        public static object GetValue(string keyName, string valueName, object defaultValue)
        {
            string str;
            object obj2;
            RegistryKey key2 = GetBaseKeyFromKeyName(keyName, out str).OpenSubKey(str);
            if (key2 == null)
            {
                return null;
            }
            try
            {
                obj2 = key2.GetValue(valueName, defaultValue);
            }
            finally
            {
                key2.Close();
            }
            return obj2;
        }

        [SecuritySafeCritical]
        public static void SetValue(string keyName, string valueName, object value)
        {
            SetValue(keyName, valueName, value, RegistryValueKind.Unknown);
        }

        [SecuritySafeCritical]
        public static void SetValue(string keyName, string valueName, object value, RegistryValueKind valueKind)
        {
            string str;
            RegistryKey key2 = GetBaseKeyFromKeyName(keyName, out str).CreateSubKey(str);
            try
            {
                key2.SetValue(valueName, value, valueKind);
            }
            finally
            {
                key2.Close();
            }
        }
    }
}

