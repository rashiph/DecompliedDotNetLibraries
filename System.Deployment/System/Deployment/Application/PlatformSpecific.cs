namespace System.Deployment.Application
{
    using System;

    internal static class PlatformSpecific
    {
        public static bool OnVistaOrAbove
        {
            get
            {
                OperatingSystem oSVersion = Environment.OSVersion;
                return ((oSVersion.Platform == PlatformID.Win32NT) && (oSVersion.Version.Major >= 6));
            }
        }

        public static bool OnWin9x
        {
            get
            {
                return (Environment.OSVersion.Platform == PlatformID.Win32Windows);
            }
        }

        public static bool OnWindows2003
        {
            get
            {
                OperatingSystem oSVersion = Environment.OSVersion;
                if (oSVersion.Platform != PlatformID.Win32NT)
                {
                    return false;
                }
                return ((oSVersion.Version.Major == 5) && (oSVersion.Version.Minor == 2));
            }
        }

        public static bool OnWinMe
        {
            get
            {
                OperatingSystem oSVersion = Environment.OSVersion;
                return (((oSVersion.Platform == PlatformID.Win32Windows) && (oSVersion.Version.Major == 4)) && (oSVersion.Version.Minor == 90));
            }
        }

        public static bool OnXPOrAbove
        {
            get
            {
                OperatingSystem oSVersion = Environment.OSVersion;
                if (oSVersion.Platform != PlatformID.Win32NT)
                {
                    return false;
                }
                return (((oSVersion.Version.Major == 5) && (oSVersion.Version.Minor >= 1)) || (oSVersion.Version.Major >= 6));
            }
        }
    }
}

