namespace System.Management.Instrumentation
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal sealed class AssemblyNameUtility
    {
        private static string BinToString(byte[] rg)
        {
            if (rg == null)
            {
                return "";
            }
            string str = "";
            for (int i = 0; i < rg.GetLength(0); i++)
            {
                str = str + string.Format("{0:x2}", rg[i]);
            }
            return str;
        }

        public static string UniqueToAssemblyBuild(Assembly assembly)
        {
            return (UniqueToAssemblyVersion(assembly) + "_Mvid_" + MetaDataInfo.GetMvid(assembly).ToString().ToLower(CultureInfo.InvariantCulture));
        }

        public static string UniqueToAssemblyFullVersion(Assembly assembly)
        {
            AssemblyName name = assembly.GetName(true);
            return string.Concat(new object[] { name.Name, "_SN_", BinToString(name.GetPublicKeyToken()), "_Version_", name.Version.Major, ".", name.Version.Minor, ".", name.Version.Build, ".", name.Version.Revision });
        }

        public static string UniqueToAssemblyMinorVersion(Assembly assembly)
        {
            AssemblyName name = assembly.GetName(true);
            return string.Concat(new object[] { name.Name, "_SN_", BinToString(name.GetPublicKeyToken()), "_Version_", name.Version.Major, ".", name.Version.Minor });
        }

        private static string UniqueToAssemblyVersion(Assembly assembly)
        {
            AssemblyName name = assembly.GetName(true);
            return string.Concat(new object[] { name.Name, "_SN_", BinToString(name.GetPublicKeyToken()), "_Version_", name.Version.Major, ".", name.Version.Minor, ".", name.Version.Build, ".", name.Version.Revision });
        }
    }
}

