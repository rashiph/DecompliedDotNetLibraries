namespace System.DirectoryServices.Protocols
{
    using System;

    internal class Utility
    {
        private static bool isWin2k3Above;
        private static bool isWin2kOS;
        private static bool platformSupported;

        static Utility()
        {
            OperatingSystem oSVersion = Environment.OSVersion;
            if ((oSVersion.Platform == PlatformID.Win32NT) && (oSVersion.Version.Major >= 5))
            {
                platformSupported = true;
                if ((oSVersion.Version.Major == 5) && (oSVersion.Version.Minor == 0))
                {
                    isWin2kOS = true;
                }
                if ((oSVersion.Version.Major > 5) || (oSVersion.Version.Minor >= 2))
                {
                    isWin2k3Above = true;
                }
            }
        }

        internal static void CheckOSVersion()
        {
            if (!platformSupported)
            {
                throw new PlatformNotSupportedException(Res.GetString("SupportedPlatforms"));
            }
        }

        internal static bool IsLdapError(LdapError error)
        {
            return ((((error == LdapError.IsLeaf) || (error == LdapError.InvalidCredentials)) || (error == LdapError.SendTimeOut)) || ((error >= LdapError.ServerDown) && (error <= LdapError.ReferralLimitExceeded)));
        }

        internal static bool IsResultCode(ResultCode code)
        {
            if ((code < ResultCode.Success) || (code > ResultCode.SaslBindInProgress))
            {
                if ((code >= ResultCode.NoSuchAttribute) && (code <= ResultCode.InvalidAttributeSyntax))
                {
                    return true;
                }
                if ((code >= ResultCode.NoSuchObject) && (code <= ResultCode.InvalidDNSyntax))
                {
                    return true;
                }
                if ((code >= ResultCode.InsufficientAccessRights) && (code <= ResultCode.LoopDetect))
                {
                    return true;
                }
                if ((code >= ResultCode.NamingViolation) && (code <= ResultCode.AffectsMultipleDsas))
                {
                    return true;
                }
                if ((((code != ResultCode.AliasDereferencingProblem) && (code != ResultCode.InappropriateAuthentication)) && ((code != ResultCode.SortControlMissing) && (code != ResultCode.OffsetRangeError))) && ((code != ResultCode.VirtualListViewError) && (code != ResultCode.Other)))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsWin2k3AboveOS
        {
            get
            {
                return isWin2k3Above;
            }
        }

        internal static bool IsWin2kOS
        {
            get
            {
                return isWin2kOS;
            }
        }
    }
}

