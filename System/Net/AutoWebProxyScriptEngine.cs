namespace System.Net
{
    using System;
    using System.Collections.Generic;
    using System.Net.NetworkInformation;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;

    internal class AutoWebProxyScriptEngine
    {
        private bool automaticallyDetectSettings;
        private Uri automaticConfigurationScript;
        private SafeRegistryHandle hkcu;
        private AutoDetector m_AutoDetector;
        private WindowsIdentity m_Identity;
        private bool m_LockHeld;
        private int m_NetworkChangeStatus;
        private bool m_UseRegistry;
        private bool needConnectoidUpdate;
        private bool needRegistryUpdate;
        private bool registryChangeDeferred;
        private AutoResetEvent registryChangeEvent;
        private AutoResetEvent registryChangeEventLM;
        private AutoResetEvent registryChangeEventPolicy;
        private bool registryChangeLMDeferred;
        private bool registryChangePolicyDeferred;
        private bool registrySuppress;
        private SafeRegistryHandle regKey;
        private SafeRegistryHandle regKeyLM;
        private SafeRegistryHandle regKeyPolicy;
        private WebProxy webProxy;
        private IWebProxyFinder webProxyFinder;

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlPrincipal)]
        internal AutoWebProxyScriptEngine(WebProxy proxy, bool useRegistry)
        {
            this.webProxy = proxy;
            this.m_UseRegistry = useRegistry;
            this.m_AutoDetector = AutoDetector.CurrentAutoDetector;
            this.m_NetworkChangeStatus = this.m_AutoDetector.NetworkChangeStatus;
            SafeRegistryHandle.RegOpenCurrentUser(0x20019, out this.hkcu);
            if (this.m_UseRegistry)
            {
                this.ListenForRegistry();
                this.m_Identity = WindowsIdentity.GetCurrent();
            }
            this.webProxyFinder = new HybridWebProxyFinder(this);
        }

        internal void Abort(ref int syncStatus)
        {
            lock (this)
            {
                switch (syncStatus)
                {
                    case 0:
                        syncStatus = 4;
                        break;

                    case 1:
                        syncStatus = 4;
                        Monitor.PulseAll(this);
                        break;

                    case 2:
                        syncStatus = 3;
                        this.webProxyFinder.Abort();
                        break;
                }
            }
        }

        internal void CheckForChanges()
        {
            int syncStatus = 0;
            this.CheckForChanges(ref syncStatus);
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlPrincipal)]
        private void CheckForChanges(ref int syncStatus)
        {
            try
            {
                bool flag = AutoDetector.CheckForNetworkChanges(ref this.m_NetworkChangeStatus);
                bool flag2 = false;
                if (flag || this.needConnectoidUpdate)
                {
                    try
                    {
                        this.EnterLock(ref syncStatus);
                        if (flag || this.needConnectoidUpdate)
                        {
                            this.needConnectoidUpdate = syncStatus != 2;
                            if (!this.needConnectoidUpdate)
                            {
                                this.ConnectoidChanged();
                                flag2 = true;
                            }
                        }
                    }
                    finally
                    {
                        this.ExitLock(ref syncStatus);
                    }
                }
                if (this.m_UseRegistry)
                {
                    bool flag3 = false;
                    AutoResetEvent registryChangeEvent = this.registryChangeEvent;
                    if (this.registryChangeDeferred || (flag3 = (registryChangeEvent != null) && registryChangeEvent.WaitOne(0, false)))
                    {
                        try
                        {
                            this.EnterLock(ref syncStatus);
                            if (flag3 || this.registryChangeDeferred)
                            {
                                this.registryChangeDeferred = syncStatus != 2;
                                if (!this.registryChangeDeferred && (this.registryChangeEvent != null))
                                {
                                    try
                                    {
                                        using (this.m_Identity.Impersonate())
                                        {
                                            this.ListenForRegistryHelper(ref this.regKey, ref this.registryChangeEvent, IntPtr.Zero, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Connections");
                                        }
                                    }
                                    catch
                                    {
                                        throw;
                                    }
                                    this.needRegistryUpdate = true;
                                }
                            }
                        }
                        finally
                        {
                            this.ExitLock(ref syncStatus);
                        }
                    }
                    flag3 = false;
                    registryChangeEvent = this.registryChangeEventLM;
                    if (this.registryChangeLMDeferred || (flag3 = (registryChangeEvent != null) && registryChangeEvent.WaitOne(0, false)))
                    {
                        try
                        {
                            this.EnterLock(ref syncStatus);
                            if (flag3 || this.registryChangeLMDeferred)
                            {
                                this.registryChangeLMDeferred = syncStatus != 2;
                                if (!this.registryChangeLMDeferred && (this.registryChangeEventLM != null))
                                {
                                    try
                                    {
                                        using (this.m_Identity.Impersonate())
                                        {
                                            this.ListenForRegistryHelper(ref this.regKeyLM, ref this.registryChangeEventLM, UnsafeNclNativeMethods.RegistryHelper.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Connections");
                                        }
                                    }
                                    catch
                                    {
                                        throw;
                                    }
                                    this.needRegistryUpdate = true;
                                }
                            }
                        }
                        finally
                        {
                            this.ExitLock(ref syncStatus);
                        }
                    }
                    flag3 = false;
                    registryChangeEvent = this.registryChangeEventPolicy;
                    if (this.registryChangePolicyDeferred || (flag3 = (registryChangeEvent != null) && registryChangeEvent.WaitOne(0, false)))
                    {
                        try
                        {
                            this.EnterLock(ref syncStatus);
                            if (flag3 || this.registryChangePolicyDeferred)
                            {
                                this.registryChangePolicyDeferred = syncStatus != 2;
                                if (!this.registryChangePolicyDeferred && (this.registryChangeEventPolicy != null))
                                {
                                    try
                                    {
                                        using (this.m_Identity.Impersonate())
                                        {
                                            this.ListenForRegistryHelper(ref this.regKeyPolicy, ref this.registryChangeEventPolicy, UnsafeNclNativeMethods.RegistryHelper.HKEY_LOCAL_MACHINE, @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\Internet Settings");
                                        }
                                    }
                                    catch
                                    {
                                        throw;
                                    }
                                    this.needRegistryUpdate = true;
                                }
                            }
                        }
                        finally
                        {
                            this.ExitLock(ref syncStatus);
                        }
                    }
                    if (this.needRegistryUpdate)
                    {
                        try
                        {
                            this.EnterLock(ref syncStatus);
                            if (this.needRegistryUpdate && (syncStatus == 2))
                            {
                                this.needRegistryUpdate = false;
                                if (!flag2)
                                {
                                    this.RegistryChanged();
                                }
                            }
                        }
                        finally
                        {
                            this.ExitLock(ref syncStatus);
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        internal void Close()
        {
            if (this.m_AutoDetector != null)
            {
                int syncStatus = 0;
                try
                {
                    this.EnterLock(ref syncStatus);
                    if (this.m_AutoDetector != null)
                    {
                        this.registrySuppress = true;
                        if (this.registryChangeEventPolicy != null)
                        {
                            this.registryChangeEventPolicy.Close();
                            this.registryChangeEventPolicy = null;
                        }
                        if (this.registryChangeEventLM != null)
                        {
                            this.registryChangeEventLM.Close();
                            this.registryChangeEventLM = null;
                        }
                        if (this.registryChangeEvent != null)
                        {
                            this.registryChangeEvent.Close();
                            this.registryChangeEvent = null;
                        }
                        if ((this.regKeyPolicy != null) && !this.regKeyPolicy.IsInvalid)
                        {
                            this.regKeyPolicy.Close();
                        }
                        if ((this.regKeyLM != null) && !this.regKeyLM.IsInvalid)
                        {
                            this.regKeyLM.Close();
                        }
                        if ((this.regKey != null) && !this.regKey.IsInvalid)
                        {
                            this.regKey.Close();
                        }
                        if (this.hkcu != null)
                        {
                            this.hkcu.RegCloseKey();
                            this.hkcu = null;
                        }
                        if (this.m_Identity != null)
                        {
                            this.m_Identity.Dispose();
                            this.m_Identity = null;
                        }
                        this.webProxyFinder.Dispose();
                        this.m_AutoDetector = null;
                    }
                }
                finally
                {
                    this.ExitLock(ref syncStatus);
                }
            }
        }

        private void ConnectoidChanged()
        {
            if (Logging.On)
            {
                Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_update_due_to_ip_config_change"));
            }
            this.m_AutoDetector = AutoDetector.CurrentAutoDetector;
            if (this.m_UseRegistry)
            {
                WebProxyData webProxyData;
                using (this.m_Identity.Impersonate())
                {
                    webProxyData = this.GetWebProxyData();
                }
                this.webProxy.Update(webProxyData);
            }
            if (this.automaticallyDetectSettings)
            {
                this.webProxyFinder.Reset();
            }
        }

        private void EnterLock(ref int syncStatus)
        {
            if (syncStatus == 0)
            {
                lock (this)
                {
                    if (syncStatus != 4)
                    {
                        syncStatus = 1;
                        do
                        {
                            if (!this.m_LockHeld)
                            {
                                syncStatus = 2;
                                this.m_LockHeld = true;
                                return;
                            }
                            Monitor.Wait(this);
                        }
                        while (syncStatus != 4);
                        Monitor.Pulse(this);
                    }
                }
            }
        }

        private void ExitLock(ref int syncStatus)
        {
            if ((syncStatus != 0) && (syncStatus != 4))
            {
                lock (this)
                {
                    this.m_LockHeld = false;
                    if (syncStatus == 3)
                    {
                        this.webProxyFinder.Reset();
                        syncStatus = 4;
                    }
                    else
                    {
                        syncStatus = 0;
                    }
                    Monitor.Pulse(this);
                }
            }
        }

        internal bool GetProxies(Uri destination, out IList<string> proxyList)
        {
            int syncStatus = 0;
            return this.GetProxies(destination, out proxyList, ref syncStatus);
        }

        internal bool GetProxies(Uri destination, out IList<string> proxyList, ref int syncStatus)
        {
            bool proxies;
            proxyList = null;
            this.CheckForChanges(ref syncStatus);
            if (!this.webProxyFinder.IsValid)
            {
                return false;
            }
            try
            {
                this.EnterLock(ref syncStatus);
                if (syncStatus != 2)
                {
                    return false;
                }
                proxies = this.webProxyFinder.GetProxies(destination, out proxyList);
            }
            finally
            {
                this.ExitLock(ref syncStatus);
            }
            return proxies;
        }

        internal WebProxyData GetWebProxyData()
        {
            WebProxyDataBuilder builder = null;
            if (ComNetOS.IsWin7)
            {
                builder = new WinHttpWebProxyBuilder();
            }
            else
            {
                builder = new RegBlobWebProxyDataBuilder(this.m_AutoDetector.Connectoid, this.hkcu);
            }
            return builder.Build();
        }

        internal void ListenForRegistry()
        {
            if (!this.registrySuppress)
            {
                if (this.registryChangeEvent == null)
                {
                    this.ListenForRegistryHelper(ref this.regKey, ref this.registryChangeEvent, IntPtr.Zero, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Connections");
                }
                if (this.registryChangeEventLM == null)
                {
                    this.ListenForRegistryHelper(ref this.regKeyLM, ref this.registryChangeEventLM, UnsafeNclNativeMethods.RegistryHelper.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Connections");
                }
                if (this.registryChangeEventPolicy == null)
                {
                    this.ListenForRegistryHelper(ref this.regKeyPolicy, ref this.registryChangeEventPolicy, UnsafeNclNativeMethods.RegistryHelper.HKEY_LOCAL_MACHINE, @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\Internet Settings");
                }
                if (((this.registryChangeEvent == null) && (this.registryChangeEventLM == null)) && (this.registryChangeEventPolicy == null))
                {
                    this.registrySuppress = true;
                }
            }
        }

        private void ListenForRegistryHelper(ref SafeRegistryHandle key, ref AutoResetEvent changeEvent, IntPtr baseKey, string subKey)
        {
            uint num = 0;
            if ((key == null) || key.IsInvalid)
            {
                if (baseKey == IntPtr.Zero)
                {
                    if (this.hkcu != null)
                    {
                        num = this.hkcu.RegOpenKeyEx(subKey, 0, 0x20019, out key);
                    }
                    else
                    {
                        num = 0x490;
                    }
                }
                else
                {
                    num = SafeRegistryHandle.RegOpenKeyEx(baseKey, subKey, 0, 0x20019, out key);
                }
                if (num == 0)
                {
                    changeEvent = new AutoResetEvent(false);
                }
            }
            if (num == 0)
            {
                num = key.RegNotifyChangeKeyValue(true, 4, changeEvent.SafeWaitHandle, true);
            }
            if (num != 0)
            {
                if ((key != null) && !key.IsInvalid)
                {
                    try
                    {
                        num = key.RegCloseKey();
                    }
                    catch (Exception exception)
                    {
                        if (NclUtilities.IsFatal(exception))
                        {
                            throw;
                        }
                    }
                }
                key = null;
                if (changeEvent != null)
                {
                    changeEvent.Close();
                    changeEvent = null;
                }
            }
        }

        private void RegistryChanged()
        {
            WebProxyData webProxyData;
            if (Logging.On)
            {
                Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_system_setting_update"));
            }
            using (this.m_Identity.Impersonate())
            {
                webProxyData = this.GetWebProxyData();
            }
            this.webProxy.Update(webProxyData);
        }

        internal bool AutomaticallyDetectSettings
        {
            get
            {
                return this.automaticallyDetectSettings;
            }
            set
            {
                if (this.automaticallyDetectSettings != value)
                {
                    this.automaticallyDetectSettings = value;
                    this.webProxyFinder.Reset();
                }
            }
        }

        internal Uri AutomaticConfigurationScript
        {
            get
            {
                return this.automaticConfigurationScript;
            }
            set
            {
                if (!object.Equals(this.automaticConfigurationScript, value))
                {
                    this.automaticConfigurationScript = value;
                    this.webProxyFinder.Reset();
                }
            }
        }

        internal ICredentials Credentials
        {
            get
            {
                return this.webProxy.Credentials;
            }
        }

        private class AutoDetector
        {
            private readonly string m_Connectoid;
            private readonly int m_CurrentVersion;
            private static NetworkAddressChangePolled s_AddressChange;
            private static volatile AutoWebProxyScriptEngine.AutoDetector s_CurrentAutoDetector;
            private static int s_CurrentVersion = 0;
            private static volatile bool s_Initialized;
            private static object s_LockObject = new object();
            private static UnsafeNclNativeMethods.RasHelper s_RasHelper;

            private AutoDetector(string connectoid, int currentVersion)
            {
                this.m_Connectoid = connectoid;
                this.m_CurrentVersion = currentVersion;
            }

            private static void CheckForChanges()
            {
                bool flag = false;
                if ((s_RasHelper != null) && s_RasHelper.HasChanged)
                {
                    s_RasHelper.Reset();
                    flag = true;
                }
                if ((s_AddressChange != null) && s_AddressChange.CheckAndReset())
                {
                    flag = true;
                }
                if (flag)
                {
                    Interlocked.Increment(ref s_CurrentVersion);
                    s_CurrentAutoDetector = new AutoWebProxyScriptEngine.AutoDetector(UnsafeNclNativeMethods.RasHelper.GetCurrentConnectoid(), s_CurrentVersion);
                }
            }

            internal static bool CheckForNetworkChanges(ref int changeStatus)
            {
                Initialize();
                CheckForChanges();
                int num = changeStatus;
                changeStatus = s_CurrentVersion;
                return (num != changeStatus);
            }

            private static void Initialize()
            {
                if (!s_Initialized)
                {
                    lock (s_LockObject)
                    {
                        if (!s_Initialized)
                        {
                            s_CurrentAutoDetector = new AutoWebProxyScriptEngine.AutoDetector(UnsafeNclNativeMethods.RasHelper.GetCurrentConnectoid(), 1);
                            if (NetworkChange.CanListenForNetworkChanges)
                            {
                                s_AddressChange = new NetworkAddressChangePolled();
                            }
                            if (UnsafeNclNativeMethods.RasHelper.RasSupported)
                            {
                                s_RasHelper = new UnsafeNclNativeMethods.RasHelper();
                            }
                            s_CurrentVersion = 1;
                            s_Initialized = true;
                        }
                    }
                }
            }

            internal string Connectoid
            {
                get
                {
                    return this.m_Connectoid;
                }
            }

            internal static AutoWebProxyScriptEngine.AutoDetector CurrentAutoDetector
            {
                get
                {
                    Initialize();
                    return s_CurrentAutoDetector;
                }
            }

            internal int NetworkChangeStatus
            {
                get
                {
                    return this.m_CurrentVersion;
                }
            }
        }

        private static class SyncStatus
        {
            internal const int Aborted = 4;
            internal const int AbortedLocked = 3;
            internal const int Locking = 1;
            internal const int LockOwner = 2;
            internal const int Unlocked = 0;
        }
    }
}

