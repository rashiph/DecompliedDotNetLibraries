namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using Microsoft.Win32;
    using System;
    using System.Globalization;
    using System.Threading;

    internal static class Util
    {
        private const string BOOTSTRAPPER_PACKAGE_PATH = "Bootstrapper";
        private const string BOOTSTRAPPER_REGISTRY_PATH = @"Software\Microsoft\GenericBootstrapper\4.0";
        private const string BOOTSTRAPPER_WOW64_REGISTRY_PATH = @"Software\Wow6432Node\Microsoft\GenericBootstrapper\4.0";
        private static string defaultPath;
        private const string DOTNET_FRAMEWORK_REGISTRY_PATH = @"Software\Microsoft\.NetFramework";
        private const string REGISTRY_DEFAULTPATH = "Path";

        public static string AddTrailingChar(string str, char ch)
        {
            if (str.LastIndexOf(ch) == (str.Length - 1))
            {
                return str;
            }
            return (str + ch);
        }

        public static CultureInfo GetCultureInfoFromString(string cultureName)
        {
            try
            {
                return new CultureInfo(cultureName);
            }
            catch (ArgumentException)
            {
            }
            return null;
        }

        public static bool IsUncPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            try
            {
                Uri uri = new Uri(path);
                return uri.IsUnc;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }

        public static bool IsWebUrl(string path)
        {
            if (!path.StartsWith("http://", StringComparison.Ordinal))
            {
                return path.StartsWith("https://", StringComparison.Ordinal);
            }
            return true;
        }

        private static string ReadRegistryString(RegistryKey key, string path, string registryValue)
        {
            RegistryKey key2 = key.OpenSubKey(path, false);
            if (key2 != null)
            {
                object obj2 = key2.GetValue(registryValue);
                if ((obj2 != null) && (key2.GetValueKind(registryValue) == RegistryValueKind.String))
                {
                    return (string) obj2;
                }
            }
            return null;
        }

        public static CultureInfo DefaultCultureInfo
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture;
            }
        }

        public static string DefaultPath
        {
            get
            {
                if (string.IsNullOrEmpty(defaultPath))
                {
                    defaultPath = ReadRegistryString(Registry.LocalMachine, @"Software\Microsoft\GenericBootstrapper\4.0", "Path");
                    if (!string.IsNullOrEmpty(defaultPath))
                    {
                        return defaultPath;
                    }
                    defaultPath = ReadRegistryString(Registry.LocalMachine, @"Software\Wow6432Node\Microsoft\GenericBootstrapper\4.0", "Path");
                    if (!string.IsNullOrEmpty(defaultPath))
                    {
                        return defaultPath;
                    }
                    defaultPath = Environment.CurrentDirectory;
                }
                return defaultPath;
            }
        }
    }
}

