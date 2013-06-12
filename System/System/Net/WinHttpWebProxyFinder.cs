namespace System.Net
{
    using System;
    using System.Collections.Generic;
    using System.Net.Configuration;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal sealed class WinHttpWebProxyFinder : BaseWebProxyFinder
    {
        private bool autoDetectFailed;
        private SafeInternetHandle session;

        public WinHttpWebProxyFinder(AutoWebProxyScriptEngine engine) : base(engine)
        {
            this.session = UnsafeNclNativeMethods.WinHttp.WinHttpOpen(null, UnsafeNclNativeMethods.WinHttp.AccessType.NoProxy, null, null, 0);
            if ((this.session == null) || this.session.IsInvalid)
            {
                int num = GetLastWin32Error();
                if (Logging.On)
                {
                    Logging.PrintError(Logging.Web, SR.GetString("net_log_proxy_winhttp_cant_open_session", new object[] { num }));
                }
            }
            else
            {
                int downloadTimeout = SettingsSectionInternal.Section.DownloadTimeout;
                if (!UnsafeNclNativeMethods.WinHttp.WinHttpSetTimeouts(this.session, downloadTimeout, downloadTimeout, downloadTimeout, downloadTimeout))
                {
                    int num3 = GetLastWin32Error();
                    if (Logging.On)
                    {
                        Logging.PrintError(Logging.Web, SR.GetString("net_log_proxy_winhttp_timeout_error", new object[] { num3 }));
                    }
                }
            }
        }

        public override void Abort()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if ((disposing && (this.session != null)) && !this.session.IsInvalid)
            {
                this.session.Close();
            }
        }

        private static int GetLastWin32Error()
        {
            int num = Marshal.GetLastWin32Error();
            if (num == 8)
            {
                throw new OutOfMemoryException();
            }
            return num;
        }

        public override bool GetProxies(Uri destination, out IList<string> proxyList)
        {
            proxyList = null;
            if ((this.session == null) || this.session.IsInvalid)
            {
                return false;
            }
            if (base.State == BaseWebProxyFinder.AutoWebProxyState.UnrecognizedScheme)
            {
                return false;
            }
            string proxyListString = null;
            int errorCode = 0x2f94;
            if (base.Engine.AutomaticallyDetectSettings && !this.autoDetectFailed)
            {
                errorCode = this.GetProxies(destination, null, out proxyListString);
                this.autoDetectFailed = IsErrorFatalForAutoDetect(errorCode);
                if (errorCode == 0x2ee6)
                {
                    base.State = BaseWebProxyFinder.AutoWebProxyState.UnrecognizedScheme;
                    return false;
                }
            }
            if ((base.Engine.AutomaticConfigurationScript != null) && IsRecoverableAutoProxyError(errorCode))
            {
                errorCode = this.GetProxies(destination, base.Engine.AutomaticConfigurationScript, out proxyListString);
            }
            base.State = GetStateFromErrorCode(errorCode);
            if (base.State != BaseWebProxyFinder.AutoWebProxyState.Completed)
            {
                return false;
            }
            if (string.IsNullOrEmpty(proxyListString))
            {
                string[] strArray = new string[1];
                proxyList = strArray;
            }
            else
            {
                proxyList = RemoveWhitespaces(proxyListString).Split(new char[] { ';' });
            }
            return true;
        }

        private int GetProxies(Uri destination, Uri scriptLocation, out string proxyListString)
        {
            int num = 0;
            proxyListString = null;
            UnsafeNclNativeMethods.WinHttp.WINHTTP_AUTOPROXY_OPTIONS autoProxyOptions = new UnsafeNclNativeMethods.WinHttp.WINHTTP_AUTOPROXY_OPTIONS {
                AutoLogonIfChallenged = false
            };
            if (scriptLocation == null)
            {
                autoProxyOptions.Flags = UnsafeNclNativeMethods.WinHttp.AutoProxyFlags.AutoDetect;
                autoProxyOptions.AutoConfigUrl = null;
                autoProxyOptions.AutoDetectFlags = UnsafeNclNativeMethods.WinHttp.AutoDetectType.DnsA | UnsafeNclNativeMethods.WinHttp.AutoDetectType.Dhcp;
            }
            else
            {
                autoProxyOptions.Flags = UnsafeNclNativeMethods.WinHttp.AutoProxyFlags.AutoProxyConfigUrl;
                autoProxyOptions.AutoConfigUrl = scriptLocation.ToString();
                autoProxyOptions.AutoDetectFlags = UnsafeNclNativeMethods.WinHttp.AutoDetectType.None;
            }
            if (!this.WinHttpGetProxyForUrl(destination.ToString(), ref autoProxyOptions, out proxyListString))
            {
                num = GetLastWin32Error();
                if ((num == 0x2eef) && (base.Engine.Credentials != null))
                {
                    autoProxyOptions.AutoLogonIfChallenged = true;
                    if (!this.WinHttpGetProxyForUrl(destination.ToString(), ref autoProxyOptions, out proxyListString))
                    {
                        num = GetLastWin32Error();
                    }
                }
                if (Logging.On)
                {
                    Logging.PrintError(Logging.Web, SR.GetString("net_log_proxy_winhttp_getproxy_failed", new object[] { destination, num }));
                }
            }
            return num;
        }

        private static BaseWebProxyFinder.AutoWebProxyState GetStateFromErrorCode(int errorCode)
        {
            if (errorCode == 0L)
            {
                return BaseWebProxyFinder.AutoWebProxyState.Completed;
            }
            switch (((UnsafeNclNativeMethods.WinHttp.ErrorCodes) errorCode))
            {
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.InvalidUrl:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.BadAutoProxyScript:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.AutoProxyServiceError:
                    return BaseWebProxyFinder.AutoWebProxyState.Completed;

                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.UnrecognizedScheme:
                    return BaseWebProxyFinder.AutoWebProxyState.UnrecognizedScheme;

                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.UnableToDownloadScript:
                    return BaseWebProxyFinder.AutoWebProxyState.DownloadFailure;

                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.AudodetectionFailed:
                    return BaseWebProxyFinder.AutoWebProxyState.DiscoveryFailure;
            }
            return BaseWebProxyFinder.AutoWebProxyState.CompilationFailure;
        }

        private static bool IsErrorFatalForAutoDetect(int errorCode)
        {
            UnsafeNclNativeMethods.WinHttp.ErrorCodes codes = (UnsafeNclNativeMethods.WinHttp.ErrorCodes) errorCode;
            if (codes <= UnsafeNclNativeMethods.WinHttp.ErrorCodes.InvalidUrl)
            {
                switch (codes)
                {
                    case UnsafeNclNativeMethods.WinHttp.ErrorCodes.Success:
                    case UnsafeNclNativeMethods.WinHttp.ErrorCodes.InvalidUrl:
                        goto Label_0028;
                }
                goto Label_002A;
            }
            if ((codes != UnsafeNclNativeMethods.WinHttp.ErrorCodes.BadAutoProxyScript) && (codes != UnsafeNclNativeMethods.WinHttp.ErrorCodes.AutoProxyServiceError))
            {
                goto Label_002A;
            }
        Label_0028:
            return false;
        Label_002A:
            return true;
        }

        private static bool IsRecoverableAutoProxyError(int errorCode)
        {
            switch (((UnsafeNclNativeMethods.WinHttp.ErrorCodes) errorCode))
            {
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.Timeout:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.UnrecognizedScheme:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.LoginFailure:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.OperationCancelled:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.BadAutoProxyScript:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.UnableToDownloadScript:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.AutoProxyServiceError:
                case UnsafeNclNativeMethods.WinHttp.ErrorCodes.AudodetectionFailed:
                    return true;
            }
            return false;
        }

        private static string RemoveWhitespaces(string value)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char ch in value)
            {
                if (!char.IsWhiteSpace(ch))
                {
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }

        public override void Reset()
        {
            base.Reset();
            this.autoDetectFailed = false;
        }

        private bool WinHttpGetProxyForUrl(string destination, ref UnsafeNclNativeMethods.WinHttp.WINHTTP_AUTOPROXY_OPTIONS autoProxyOptions, out string proxyListString)
        {
            proxyListString = null;
            bool flag = false;
            UnsafeNclNativeMethods.WinHttp.WINHTTP_PROXY_INFO proxyInfo = new UnsafeNclNativeMethods.WinHttp.WINHTTP_PROXY_INFO();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                flag = UnsafeNclNativeMethods.WinHttp.WinHttpGetProxyForUrl(this.session, destination, ref autoProxyOptions, out proxyInfo);
                if (flag)
                {
                    proxyListString = Marshal.PtrToStringUni(proxyInfo.Proxy);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(proxyInfo.Proxy);
                Marshal.FreeHGlobal(proxyInfo.ProxyBypass);
            }
            return flag;
        }
    }
}

