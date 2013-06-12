namespace System.Net
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    internal sealed class HybridWebProxyFinder : IWebProxyFinder, IDisposable
    {
        private static bool allowFallback;
        private const string allowFallbackKey = @"SOFTWARE\Microsoft\.NETFramework";
        private const string allowFallbackKeyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework";
        private const string allowFallbackValueName = "LegacyWPADSupport";
        private BaseWebProxyFinder currentFinder;
        private AutoWebProxyScriptEngine engine;
        private NetWebProxyFinder netFinder;
        private WinHttpWebProxyFinder winHttpFinder;

        static HybridWebProxyFinder()
        {
            InitializeFallbackSettings();
        }

        public HybridWebProxyFinder(AutoWebProxyScriptEngine engine)
        {
            this.engine = engine;
            this.winHttpFinder = new WinHttpWebProxyFinder(engine);
            this.currentFinder = this.winHttpFinder;
        }

        public void Abort()
        {
            this.currentFinder.Abort();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.winHttpFinder.Dispose();
                if (this.netFinder != null)
                {
                    this.netFinder.Dispose();
                }
            }
        }

        public bool GetProxies(Uri destination, out IList<string> proxyList)
        {
            if (this.currentFinder.GetProxies(destination, out proxyList))
            {
                return true;
            }
            if ((!allowFallback || !this.currentFinder.IsUnrecognizedScheme) || (this.currentFinder != this.winHttpFinder))
            {
                return false;
            }
            if (this.netFinder == null)
            {
                this.netFinder = new NetWebProxyFinder(this.engine);
            }
            this.currentFinder = this.netFinder;
            return this.currentFinder.GetProxies(destination, out proxyList);
        }

        [RegistryPermission(SecurityAction.Assert, Read=@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework")]
        private static void InitializeFallbackSettings()
        {
            allowFallback = false;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\.NETFramework"))
                {
                    try
                    {
                        if (key.GetValueKind("LegacyWPADSupport") == RegistryValueKind.DWord)
                        {
                            allowFallback = ((int) key.GetValue("LegacyWPADSupport")) == 1;
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                }
            }
            catch (SecurityException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public void Reset()
        {
            this.winHttpFinder.Reset();
            if (this.netFinder != null)
            {
                this.netFinder.Reset();
            }
            this.currentFinder = this.winHttpFinder;
        }

        public bool IsValid
        {
            get
            {
                return this.currentFinder.IsValid;
            }
        }
    }
}

