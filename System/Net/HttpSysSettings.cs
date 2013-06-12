namespace System.Net
{
    using Microsoft.Win32;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    internal static class HttpSysSettings
    {
        private static bool enableNonUtf8 = true;
        private const bool enableNonUtf8Default = true;
        private const string enableNonUtf8Name = "EnableNonUtf8";
        private static bool favorUtf8 = true;
        private const bool favorUtf8Default = true;
        private const string favorUtf8Name = "FavorUtf8";
        private const string httpSysParametersKey = @"System\CurrentControlSet\Services\HTTP\Parameters";

        static HttpSysSettings()
        {
            ReadHttpSysRegistrySettings();
        }

        private static void LogRegistryException(string methodName, Exception e)
        {
            LogWarning(methodName, "net_log_listener_httpsys_registry_error", new object[] { @"System\CurrentControlSet\Services\HTTP\Parameters", e });
        }

        private static void LogWarning(string methodName, string message, params object[] args)
        {
            if (Logging.On)
            {
                Logging.PrintWarning(Logging.HttpListener, typeof(HttpSysSettings), methodName, SR.GetString(message, args));
            }
        }

        [RegistryPermission(SecurityAction.Assert, Read=@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\HTTP\Parameters")]
        private static void ReadHttpSysRegistrySettings()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services\HTTP\Parameters");
                if (key == null)
                {
                    LogWarning("ReadHttpSysRegistrySettings", "net_log_listener_httpsys_registry_null", new object[] { @"System\CurrentControlSet\Services\HTTP\Parameters" });
                }
                else
                {
                    using (key)
                    {
                        enableNonUtf8 = ReadRegistryValue(key, "EnableNonUtf8", true);
                        favorUtf8 = ReadRegistryValue(key, "FavorUtf8", true);
                    }
                }
            }
            catch (SecurityException exception)
            {
                LogRegistryException("ReadHttpSysRegistrySettings", exception);
            }
            catch (ObjectDisposedException exception2)
            {
                LogRegistryException("ReadHttpSysRegistrySettings", exception2);
            }
        }

        private static bool ReadRegistryValue(RegistryKey key, string valueName, bool defaultValue)
        {
            try
            {
                if (key.GetValueKind(valueName) == RegistryValueKind.DWord)
                {
                    return Convert.ToBoolean(key.GetValue(valueName), CultureInfo.InvariantCulture);
                }
            }
            catch (UnauthorizedAccessException exception)
            {
                LogRegistryException("ReadRegistryValue", exception);
            }
            catch (IOException exception2)
            {
                LogRegistryException("ReadRegistryValue", exception2);
            }
            catch (SecurityException exception3)
            {
                LogRegistryException("ReadRegistryValue", exception3);
            }
            catch (ObjectDisposedException exception4)
            {
                LogRegistryException("ReadRegistryValue", exception4);
            }
            return defaultValue;
        }

        public static bool EnableNonUtf8
        {
            get
            {
                return enableNonUtf8;
            }
        }

        public static bool FavorUtf8
        {
            get
            {
                return favorUtf8;
            }
        }
    }
}

