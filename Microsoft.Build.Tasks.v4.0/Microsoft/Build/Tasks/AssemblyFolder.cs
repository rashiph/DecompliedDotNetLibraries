namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.IO;

    internal static class AssemblyFolder
    {
        private static Hashtable assemblyFolders;
        private static object syncLock = new object();

        private static void AddFoldersFromRegistryKey(string key, Hashtable directories)
        {
            AddFoldersFromRegistryKey(Registry.CurrentUser, key, directories);
            AddFoldersFromRegistryKey(Registry.LocalMachine, key, directories);
        }

        private static void AddFoldersFromRegistryKey(RegistryKey hive, string key, Hashtable directories)
        {
            RegistryKey key2 = hive.OpenSubKey(key);
            string str = string.Empty;
            if (hive == Registry.CurrentUser)
            {
                str = "hkcu";
            }
            else if (hive == Registry.LocalMachine)
            {
                str = "hklm";
            }
            else
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(false, "AssemblyFolder.AddFoldersFromRegistryKey expected a known hive.");
            }
            if (key2 != null)
            {
                foreach (string str2 in key2.GetSubKeyNames())
                {
                    RegistryKey key3 = key2.OpenSubKey(str2);
                    if (key3.ValueCount > 0)
                    {
                        string path = (string) key3.GetValue("");
                        if (Directory.Exists(path))
                        {
                            string str4 = str + @"\" + str2;
                            directories[str4] = path;
                        }
                    }
                }
            }
        }

        private static void CreateAssemblyFolders()
        {
            assemblyFolders = new Hashtable(StringComparer.OrdinalIgnoreCase);
            AddFoldersFromRegistryKey(@"SOFTWARE\Microsoft\.NETFramework\AssemblyFolders", assemblyFolders);
            AddFoldersFromRegistryKey(@"SOFTWARE\Microsoft\VisualStudio\8.0\AssemblyFolders", assemblyFolders);
        }

        internal static ICollection GetAssemblyFolders(string regKeyAlias)
        {
            lock (syncLock)
            {
                if (assemblyFolders == null)
                {
                    CreateAssemblyFolders();
                }
            }
            if ((regKeyAlias == null) || (regKeyAlias.Length == 0))
            {
                return assemblyFolders.Values;
            }
            ArrayList list = new ArrayList();
            string str = (string) assemblyFolders[regKeyAlias];
            if ((str != null) && (str.Length > 0))
            {
                list.Add(str);
            }
            return list;
        }
    }
}

