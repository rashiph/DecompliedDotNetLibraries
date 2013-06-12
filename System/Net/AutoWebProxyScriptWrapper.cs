namespace System.Net
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Remoting;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Threading;

    internal class AutoWebProxyScriptWrapper
    {
        private int appDomainIndex;
        private const string c_appDomainName = "WebProxyScript";
        private DateTime lastModified;
        private static AppDomainSetup s_AppDomainInfo;
        private static Hashtable s_AppDomains = new Hashtable();
        private static bool s_CleanedUp;
        private static AppDomain s_ExcessAppDomain;
        private static int s_NextAppDomainIndex;
        private static Exception s_ProxyScriptHelperLoadError;
        private static object s_ProxyScriptHelperLock = new object();
        private static Type s_ProxyScriptHelperType;
        private byte[] scriptBytes;
        private AppDomain scriptDomain;
        private string scriptText;
        private IWebProxyScript site;

        static AutoWebProxyScriptWrapper()
        {
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(AutoWebProxyScriptWrapper.OnDomainUnload);
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.TypeInformation), ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        internal AutoWebProxyScriptWrapper()
        {
            Exception exception = null;
            if ((s_ProxyScriptHelperLoadError == null) && (s_ProxyScriptHelperType == null))
            {
                lock (s_ProxyScriptHelperLock)
                {
                    if ((s_ProxyScriptHelperLoadError == null) && (s_ProxyScriptHelperType == null))
                    {
                        try
                        {
                            s_ProxyScriptHelperType = Type.GetType("System.Net.VsaWebProxyScript, Microsoft.JScript, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true);
                        }
                        catch (Exception exception2)
                        {
                            exception = exception2;
                        }
                        if (s_ProxyScriptHelperType == null)
                        {
                            s_ProxyScriptHelperLoadError = (exception == null) ? new InternalException() : exception;
                        }
                    }
                }
            }
            if (s_ProxyScriptHelperLoadError != null)
            {
                throw new TypeLoadException(SR.GetString("net_cannot_load_proxy_helper"), (s_ProxyScriptHelperLoadError is InternalException) ? null : s_ProxyScriptHelperLoadError);
            }
            this.CreateAppDomain();
            exception = null;
            try
            {
                ObjectHandle handle = Activator.CreateInstance(this.scriptDomain, s_ProxyScriptHelperType.Assembly.FullName, s_ProxyScriptHelperType.FullName, false, BindingFlags.CreateInstance | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, null, null, null);
                if (handle != null)
                {
                    this.site = (IWebProxyScript) handle.Unwrap();
                }
            }
            catch (Exception exception3)
            {
                exception = exception3;
            }
            if (this.site == null)
            {
                lock (s_ProxyScriptHelperLock)
                {
                    if (s_ProxyScriptHelperLoadError == null)
                    {
                        s_ProxyScriptHelperLoadError = (exception == null) ? new InternalException() : exception;
                    }
                }
                throw new TypeLoadException(SR.GetString("net_cannot_load_proxy_helper"), (s_ProxyScriptHelperLoadError is InternalException) ? null : s_ProxyScriptHelperLoadError);
            }
        }

        internal void Close()
        {
            this.site.Close();
            TimerThread.GetOrCreateQueue(0).CreateTimer(new TimerThread.Callback(AutoWebProxyScriptWrapper.CloseAppDomainCallback), this.appDomainIndex);
            GC.SuppressFinalize(this);
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlAppDomain)]
        private static void CloseAppDomain(int index)
        {
            AppDomain domain;
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(s_AppDomains.SyncRoot, ref lockTaken);
                if (s_CleanedUp)
                {
                    return;
                }
                domain = (AppDomain) s_AppDomains[index];
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(s_AppDomains.SyncRoot);
                    lockTaken = false;
                }
            }
            try
            {
                AppDomain.Unload(domain);
            }
            catch (AppDomainUnloadedException)
            {
            }
            finally
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    Monitor.Enter(s_AppDomains.SyncRoot, ref lockTaken);
                    s_AppDomains.Remove(index);
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(s_AppDomains.SyncRoot);
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlAppDomain)]
        private static void CloseAppDomainCallback(TimerThread.Timer timer, int timeNoticed, object context)
        {
            try
            {
                AppDomain objA = context as AppDomain;
                if (objA == null)
                {
                    CloseAppDomain((int) context);
                }
                else if (object.ReferenceEquals(objA, s_ExcessAppDomain))
                {
                    try
                    {
                        AppDomain.Unload(objA);
                    }
                    catch (AppDomainUnloadedException)
                    {
                    }
                    s_ExcessAppDomain = null;
                }
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception))
                {
                    throw;
                }
            }
        }

        internal bool Compile(Uri engineScriptLocation, string scriptBody, byte[] buffer)
        {
            if (this.site.Load(engineScriptLocation, scriptBody, typeof(WebProxyScriptHelper)))
            {
                this.scriptText = scriptBody;
                this.scriptBytes = buffer;
                return true;
            }
            return false;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlAppDomain)]
        private void CreateAppDomain()
        {
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(s_AppDomains.SyncRoot, ref lockTaken);
                if (s_CleanedUp)
                {
                    throw new InvalidOperationException(SR.GetString("net_cant_perform_during_shutdown"));
                }
                if (s_AppDomainInfo == null)
                {
                    s_AppDomainInfo = new AppDomainSetup();
                    s_AppDomainInfo.DisallowBindingRedirects = true;
                    s_AppDomainInfo.DisallowCodeDownload = true;
                    NamedPermissionSet permSet = new NamedPermissionSet("__WebProxySandbox", PermissionState.None);
                    permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                    ApplicationTrust trust = new ApplicationTrust {
                        DefaultGrantSet = new PolicyStatement(permSet)
                    };
                    s_AppDomainInfo.ApplicationTrust = trust;
                    s_AppDomainInfo.ApplicationBase = Environment.SystemDirectory;
                }
                AppDomain context = s_ExcessAppDomain;
                if (context != null)
                {
                    TimerThread.GetOrCreateQueue(0).CreateTimer(new TimerThread.Callback(AutoWebProxyScriptWrapper.CloseAppDomainCallback), context);
                    throw new InvalidOperationException(SR.GetString("net_cant_create_environment"));
                }
                this.appDomainIndex = s_NextAppDomainIndex++;
                try
                {
                }
                finally
                {
                    PermissionSet grantSet = new PermissionSet(PermissionState.None);
                    grantSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                    s_ExcessAppDomain = AppDomain.CreateDomain("WebProxyScript", null, s_AppDomainInfo, grantSet, null);
                    try
                    {
                        s_AppDomains.Add(this.appDomainIndex, s_ExcessAppDomain);
                        this.scriptDomain = s_ExcessAppDomain;
                    }
                    finally
                    {
                        if (object.ReferenceEquals(this.scriptDomain, s_ExcessAppDomain))
                        {
                            s_ExcessAppDomain = null;
                        }
                        else
                        {
                            try
                            {
                                s_AppDomains.Remove(this.appDomainIndex);
                            }
                            finally
                            {
                                TimerThread.GetOrCreateQueue(0).CreateTimer(new TimerThread.Callback(AutoWebProxyScriptWrapper.CloseAppDomainCallback), s_ExcessAppDomain);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(s_AppDomains.SyncRoot);
                }
            }
        }

        ~AutoWebProxyScriptWrapper()
        {
            if (!NclUtilities.HasShutdownStarted && (this.scriptDomain != null))
            {
                TimerThread.GetOrCreateQueue(0).CreateTimer(new TimerThread.Callback(AutoWebProxyScriptWrapper.CloseAppDomainCallback), this.appDomainIndex);
            }
        }

        internal string FindProxyForURL(string url, string host)
        {
            return this.site.Run(url, host);
        }

        [ReliabilityContract(Consistency.MayCorruptProcess, Cer.MayFail), SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlAppDomain)]
        private static void OnDomainUnload(object sender, EventArgs e)
        {
            lock (s_AppDomains.SyncRoot)
            {
                if (!s_CleanedUp)
                {
                    s_CleanedUp = true;
                    foreach (AppDomain domain in s_AppDomains.Values)
                    {
                        try
                        {
                            AppDomain.Unload(domain);
                        }
                        catch
                        {
                        }
                    }
                    s_AppDomains.Clear();
                    AppDomain domain2 = s_ExcessAppDomain;
                    if (domain2 != null)
                    {
                        try
                        {
                            AppDomain.Unload(domain2);
                        }
                        catch
                        {
                        }
                        s_ExcessAppDomain = null;
                    }
                }
            }
        }

        internal byte[] Buffer
        {
            get
            {
                return this.scriptBytes;
            }
            set
            {
                this.scriptBytes = value;
            }
        }

        internal DateTime LastModified
        {
            get
            {
                return this.lastModified;
            }
            set
            {
                this.lastModified = value;
            }
        }

        internal string ScriptBody
        {
            get
            {
                return this.scriptText;
            }
        }
    }
}

