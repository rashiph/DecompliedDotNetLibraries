namespace System.Management.Instrumentation
{
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security.Principal;

    internal sealed class WMICapabilities
    {
        private const string AutoRecoverMofsTimestampVal = "Autorecover MOFs timestamp";
        private const string AutoRecoverMofsVal = "Autorecover MOFs";
        private const string FrameworkSubDirectory = "Framework";
        private static string installationDirectory = null;
        private const string InstallationDirectoryVal = "Installation Directory";
        private static int multiIndicateSupported = -1;
        private const string MultiIndicateSupportedValueNameVal = "MultiIndicateSupported";
        private const string WMICIMOMKeyPath = @"Software\Microsoft\WBEM\CIMOM";
        private static RegistryKey wmiKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\WBEM", false);
        private const string WMIKeyPath = @"Software\Microsoft\WBEM";
        private static RegistryKey wmiNetKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\WBEM\.NET", false);
        private const string WMINetKeyPath = @"Software\Microsoft\WBEM\.NET";

        public static void AddAutorecoverMof(string path)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\WBEM\CIMOM", true);
            if (key != null)
            {
                object obj2 = key.GetValue("Autorecover MOFs");
                string[] strArray = obj2 as string[];
                if (strArray == null)
                {
                    if (obj2 != null)
                    {
                        return;
                    }
                    strArray = new string[0];
                }
                key.SetValue("Autorecover MOFs timestamp", DateTime.Now.ToFileTime().ToString((IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(long))));
                foreach (string str in strArray)
                {
                    if (string.Compare(str, path, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return;
                    }
                }
                string[] array = new string[strArray.Length + 1];
                strArray.CopyTo(array, 0);
                array[array.Length - 1] = path;
                key.SetValue("Autorecover MOFs", array);
                key.SetValue("Autorecover MOFs timestamp", DateTime.Now.ToFileTime().ToString((IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(long))));
            }
        }

        private static bool IsNovaFile(FileVersionInfo info)
        {
            return (((info.FileMajorPart == 1) && (info.FileMinorPart == 50)) && (info.FileBuildPart == 0x43d));
        }

        public static bool IsUserAdmin()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
            {
                return true;
            }
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return (principal.Identity.IsAuthenticated && principal.IsInRole(WindowsBuiltInRole.Administrator));
        }

        public static bool IsWindowsXPOrHigher()
        {
            OperatingSystem oSVersion = Environment.OSVersion;
            return ((oSVersion.Platform == PlatformID.Win32NT) && (oSVersion.Version >= new Version(5, 1)));
        }

        private static bool MultiIndicatePossible()
        {
            OperatingSystem oSVersion = Environment.OSVersion;
            if ((oSVersion.Platform == PlatformID.Win32NT) && (oSVersion.Version >= new Version(5, 1)))
            {
                return true;
            }
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.SystemDirectory, @"wbem\fastprox.dll"));
            return (IsNovaFile(versionInfo) && (versionInfo.FilePrivatePart >= 0x38));
        }

        public static string FrameworkDirectory
        {
            get
            {
                return Path.Combine(InstallationDirectory, "Framework");
            }
        }

        public static string InstallationDirectory
        {
            get
            {
                if ((installationDirectory == null) && (wmiKey != null))
                {
                    installationDirectory = wmiKey.GetValue("Installation Directory").ToString();
                }
                return installationDirectory;
            }
        }

        public static bool MultiIndicateSupported
        {
            get
            {
                if (-1 == multiIndicateSupported)
                {
                    multiIndicateSupported = MultiIndicatePossible() ? 1 : 0;
                    if (wmiNetKey != null)
                    {
                        object obj2 = wmiNetKey.GetValue("MultiIndicateSupported", multiIndicateSupported);
                        if ((obj2.GetType() == typeof(int)) && (((int) obj2) == 1))
                        {
                            multiIndicateSupported = 1;
                        }
                    }
                }
                return (multiIndicateSupported == 1);
            }
        }
    }
}

