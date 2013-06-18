namespace System.Deployment.Application
{
    using Microsoft.Win32;
    using System;
    using System.Globalization;
    using System.IO;

    internal static class PolicyKeys
    {
        private static bool CheckDeploymentBoolString(string keyName, bool compare, bool defaultIfNotSet)
        {
            bool flag = false;
            bool flag2 = false;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\.NETFramework\DeploymentFramework", false))
            {
                if (key != null)
                {
                    string str = key.GetValue(keyName) as string;
                    if (str != null)
                    {
                        flag2 = true;
                        CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
                        if (compareInfo.Compare(str, "true", CompareOptions.IgnoreCase) == 0)
                        {
                            flag = true;
                        }
                        else if (compareInfo.Compare(str, "false", CompareOptions.IgnoreCase) == 0)
                        {
                            flag = false;
                        }
                    }
                }
            }
            if (!flag2)
            {
                return defaultIfNotSet;
            }
            return (flag == compare);
        }

        private static bool CheckRegistryBoolString(RegistryKey registryKey, string valueName, bool compare, bool defaultIfNotSet)
        {
            bool flag = false;
            bool flag2 = false;
            if (registryKey != null)
            {
                string str = registryKey.GetValue(valueName) as string;
                if (str != null)
                {
                    flag2 = true;
                    CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
                    if (compareInfo.Compare(str, "true", CompareOptions.IgnoreCase) == 0)
                    {
                        flag = true;
                    }
                    else if (compareInfo.Compare(str, "false", CompareOptions.IgnoreCase) == 0)
                    {
                        flag = false;
                    }
                }
            }
            if (!flag2)
            {
                return defaultIfNotSet;
            }
            return (flag == compare);
        }

        public static HostType ClrHostType()
        {
            int num = 0;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\.NETFramework\DeploymentFramework", false))
            {
                if (key != null)
                {
                    object obj2 = key.GetValue("ClickOnceHost");
                    num = (obj2 != null) ? ((int) obj2) : 0;
                }
            }
            switch (((HostType) num))
            {
                case HostType.AppLaunch:
                    Logger.AddWarningInformation(Resources.GetString("ForceAppLaunch"));
                    break;

                case HostType.Cor:
                    Logger.AddWarningInformation(Resources.GetString("ForceCor"));
                    break;
            }
            return (HostType) num;
        }

        public static bool DisableGenericExceptionHandler()
        {
            if (CheckDeploymentBoolString("DisableGenericExceptionHandler", true, false))
            {
                Logger.AddWarningInformation(Resources.GetString("DisableGenericExceptionHandler"));
                return true;
            }
            return false;
        }

        public static ushort GetLogVerbosityLevel()
        {
            ushort num = 0;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\Software\Microsoft\Windows\CurrentVersion\Deployment", false))
                {
                    if (key != null)
                    {
                        object obj2 = key.GetValue("LogVerbosityLevel");
                        if (obj2 is string)
                        {
                            Logger.AddWarningInformation(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("LogVerbosityLevelSet"), new object[] { obj2 }));
                            num = Convert.ToUInt16(obj2, CultureInfo.InvariantCulture);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (ExceptionUtility.IsHardException(exception))
                {
                    throw;
                }
                num = 0;
            }
            return num;
        }

        public static bool ProduceDetailedExecutionSectionInLog()
        {
            return (GetLogVerbosityLevel() > 0);
        }

        public static bool RequireHashInManifests()
        {
            return CheckDeploymentBoolString("RequireHashInManifests", true, false);
        }

        public static bool RequireSignedManifests()
        {
            return CheckDeploymentBoolString("RequireSignedManifests", true, false);
        }

        public static bool SkipApplicationDependencyHashCheck()
        {
            if (CheckDeploymentBoolString("SkipApplicationDependencyHashCheck", true, false))
            {
                Logger.AddWarningInformation(Resources.GetString("SkipApplicationDependencyHashCheck"));
                return true;
            }
            return false;
        }

        public static bool SkipDeploymentProvider()
        {
            if (CheckDeploymentBoolString("SkipDeploymentProvider", true, false))
            {
                Logger.AddWarningInformation(Resources.GetString("SkipDeploymentProvider"));
                return true;
            }
            return false;
        }

        public static bool SkipSchemaValidation()
        {
            if (CheckDeploymentBoolString("SkipSchemaValidation", true, false))
            {
                Logger.AddWarningInformation(Resources.GetString("SkipSchemaValidation"));
                return true;
            }
            return false;
        }

        public static bool SkipSemanticValidation()
        {
            if (CheckDeploymentBoolString("SkipSemanticValidation", true, false))
            {
                Logger.AddWarningInformation(Resources.GetString("SkipAllSemanticValidation"));
                return true;
            }
            return false;
        }

        public static bool SkipSignatureValidation()
        {
            if (CheckDeploymentBoolString("SkipSignatureValidation", true, false))
            {
                Logger.AddWarningInformation(Resources.GetString("SkipAllSigValidation"));
                return true;
            }
            return false;
        }

        public static bool SkipSKUDetection()
        {
            bool flag = false;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Fusion", false))
                {
                    if (key != null)
                    {
                        object obj2 = key.GetValue("NoClientChecks");
                        if ((obj2 != null) && (Convert.ToUInt32(obj2) > 0))
                        {
                            Logger.AddWarningInformation(Resources.GetString("SkippedSKUDetection"));
                            flag = true;
                        }
                    }
                }
            }
            catch (OverflowException)
            {
                flag = false;
            }
            catch (InvalidCastException)
            {
                flag = false;
            }
            catch (IOException)
            {
                flag = false;
            }
            return flag;
        }

        public static bool SuppressLimitOnNumberOfActivations()
        {
            if (CheckDeploymentBoolString("SuppressLimitOnNumberOfActivations", true, false))
            {
                Logger.AddWarningInformation(Resources.GetString("SuppressLimitOnNumberOfActivations"));
                return true;
            }
            return false;
        }

        public enum HostType
        {
            Default,
            AppLaunch,
            Cor
        }
    }
}

