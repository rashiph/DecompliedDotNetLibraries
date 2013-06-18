namespace System.Web.Util
{
    using System;
    using System.Reflection;

    internal static class AssemblyUtil
    {
        private const string _emptyFileVersion = "0.0.0.0";

        public static string GetAssemblyFileVersion(Assembly assembly)
        {
            AssemblyFileVersionAttribute[] customAttributes = (AssemblyFileVersionAttribute[]) assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
            if (customAttributes.Length > 0)
            {
                string version = customAttributes[0].Version;
                if (string.IsNullOrEmpty(version))
                {
                    version = "0.0.0.0";
                }
                return version;
            }
            return "0.0.0.0";
        }
    }
}

