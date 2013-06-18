namespace Microsoft.Build.Tasks
{
    using Microsoft.Win32;
    using System;
    using System.Collections;

    internal static class RegistryHelper
    {
        internal static string GetDefaultValue(RegistryKey baseKey, string subkey)
        {
            RegistryKey key = baseKey.OpenSubKey(subkey);
            if ((key != null) && (key.ValueCount != 0))
            {
                return (string) key.GetValue("");
            }
            return null;
        }

        internal static IEnumerable GetSubKeyNames(RegistryKey baseKey, string subkey)
        {
            RegistryKey key = baseKey.OpenSubKey(subkey);
            if (key == null)
            {
                return new string[0];
            }
            IEnumerable subKeyNames = key.GetSubKeyNames();
            if (subKeyNames == null)
            {
                return new string[0];
            }
            return subKeyNames;
        }

        internal static RegistryKey OpenBaseKey(RegistryHive hive, RegistryView view)
        {
            return RegistryKey.OpenBaseKey(hive, view);
        }
    }
}

