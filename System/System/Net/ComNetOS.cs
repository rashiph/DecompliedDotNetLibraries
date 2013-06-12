namespace System.Net
{
    using Microsoft.Win32;
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    internal static class ComNetOS
    {
        internal static readonly WindowsInstallationType InstallationType;
        private const string InstallTypeStringClient = "Client";
        private const string InstallTypeStringEmbedded = "Embedded";
        private const string InstallTypeStringServer = "Server";
        private const string InstallTypeStringServerCore = "Server Core";
        internal static readonly bool IsAspNetServer;
        internal static readonly bool IsPostWin2K;
        internal static readonly bool IsVista;
        internal static readonly bool IsWin2K;
        internal static readonly bool IsWin2k3;
        internal static readonly bool IsWin2k3Sp1;
        internal static readonly bool IsWin7;
        internal static readonly bool IsWin9x;
        internal static readonly bool IsWinHttp51;
        internal static readonly bool IsWinNt;
        internal static readonly bool IsXpSp2;
        private const string OSInstallTypeRegKey = @"Software\Microsoft\Windows NT\CurrentVersion";
        private const string OSInstallTypeRegKeyPath = @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion";
        private const string OSInstallTypeRegName = "InstallationType";

        [EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
        static ComNetOS()
        {
            OperatingSystem oSVersion = Environment.OSVersion;
            if (oSVersion.Platform == PlatformID.Win32Windows)
            {
                IsWin9x = true;
            }
            else
            {
                try
                {
                    IsAspNetServer = Thread.GetDomain().GetData(".appDomain") != null;
                }
                catch
                {
                }
                IsWinNt = true;
                IsWin2K = true;
                if ((oSVersion.Version.Major == 5) && (oSVersion.Version.Minor == 0))
                {
                    IsWinHttp51 = oSVersion.Version.MajorRevision >= 3;
                }
                else
                {
                    IsPostWin2K = true;
                    if ((((oSVersion.Version.Major == 5) && (oSVersion.Version.Minor == 1)) && (oSVersion.Version.MajorRevision >= 2)) || (oSVersion.Version.Major >= 6))
                    {
                        IsXpSp2 = true;
                    }
                    if ((oSVersion.Version.Major == 5) && (oSVersion.Version.Minor == 1))
                    {
                        IsWinHttp51 = oSVersion.Version.MajorRevision >= 1;
                    }
                    else
                    {
                        IsWinHttp51 = true;
                        IsWin2k3 = true;
                        if ((((oSVersion.Version.Major == 5) && (oSVersion.Version.Minor == 2)) && (oSVersion.Version.MajorRevision >= 1)) || (oSVersion.Version.Major >= 6))
                        {
                            IsWin2k3Sp1 = true;
                        }
                        if (oSVersion.Version.Major >= 6)
                        {
                            IsVista = true;
                        }
                        if ((oSVersion.Version.Major >= 7) || ((oSVersion.Version.Major == 6) && (oSVersion.Version.Minor >= 1)))
                        {
                            IsWin7 = true;
                        }
                        InstallationType = GetWindowsInstallType();
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.Web, SR.GetString("net_osinstalltype", new object[] { InstallationType }));
                        }
                    }
                }
            }
        }

        [RegistryPermission(SecurityAction.Assert, Read=@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion")]
        private static WindowsInstallationType GetWindowsInstallType()
        {
            WindowsInstallationType unknown;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion"))
                {
                    string str = key.GetValue("InstallationType") as string;
                    if (string.IsNullOrEmpty(str))
                    {
                        if (Logging.On)
                        {
                            Logging.PrintWarning(Logging.Web, SR.GetString("net_empty_osinstalltype", new object[] { @"Software\Microsoft\Windows NT\CurrentVersion\InstallationType" }));
                        }
                        return WindowsInstallationType.Unknown;
                    }
                    if (string.Compare(str, "Client", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return WindowsInstallationType.Client;
                    }
                    if (string.Compare(str, "Server", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return WindowsInstallationType.Server;
                    }
                    if (string.Compare(str, "Server Core", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return WindowsInstallationType.ServerCore;
                    }
                    if (string.Compare(str, "Embedded", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return WindowsInstallationType.Embedded;
                    }
                    if (Logging.On)
                    {
                        Logging.PrintError(Logging.Web, SR.GetString("net_unknown_osinstalltype", new object[] { str }));
                    }
                    unknown = WindowsInstallationType.Unknown;
                }
            }
            catch (UnauthorizedAccessException exception)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_cant_determine_osinstalltype", new object[] { @"Software\Microsoft\Windows NT\CurrentVersion", exception.Message }));
                }
                unknown = WindowsInstallationType.Unknown;
            }
            catch (SecurityException exception2)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_cant_determine_osinstalltype", new object[] { @"Software\Microsoft\Windows NT\CurrentVersion", exception2.Message }));
                }
                unknown = WindowsInstallationType.Unknown;
            }
            return unknown;
        }
    }
}

