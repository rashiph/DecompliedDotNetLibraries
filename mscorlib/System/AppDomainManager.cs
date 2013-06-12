namespace System
{
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Hosting;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Threading;

    [ComVisible(true), SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    public class AppDomainManager : MarshalByRefObject
    {
        private System.Runtime.Hosting.ApplicationActivator m_appActivator;
        private Assembly m_entryAssembly;
        private AppDomainManagerInitializationOptions m_flags;

        public virtual bool CheckSecuritySettings(SecurityState state)
        {
            return false;
        }

        [SecurityCritical]
        public virtual AppDomain CreateDomain(string friendlyName, Evidence securityInfo, AppDomainSetup appDomainInfo)
        {
            return CreateDomainHelper(friendlyName, securityInfo, appDomainInfo);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, ControlAppDomain=true)]
        protected static AppDomain CreateDomainHelper(string friendlyName, Evidence securityInfo, AppDomainSetup appDomainInfo)
        {
            if (friendlyName == null)
            {
                throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_String"));
            }
            if (securityInfo != null)
            {
                new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
                AppDomain.CheckDomainCreationEvidence(appDomainInfo, securityInfo);
            }
            if (appDomainInfo == null)
            {
                appDomainInfo = new AppDomainSetup();
            }
            if ((appDomainInfo.AppDomainManagerAssembly == null) || (appDomainInfo.AppDomainManagerType == null))
            {
                string str;
                string str2;
                AppDomain.CurrentDomain.GetAppDomainManagerType(out str, out str2);
                if (appDomainInfo.AppDomainManagerAssembly == null)
                {
                    appDomainInfo.AppDomainManagerAssembly = str;
                }
                if (appDomainInfo.AppDomainManagerType == null)
                {
                    appDomainInfo.AppDomainManagerType = str2;
                }
            }
            return AppDomain.nCreateDomain(friendlyName, appDomainInfo, securityInfo, (securityInfo == null) ? AppDomain.CurrentDomain.InternalEvidence : null, AppDomain.CurrentDomain.GetSecurityDescriptor());
        }

        [SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetEntryAssembly(ObjectHandleOnStack retAssembly);
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool HasHost();
        [SecurityCritical]
        public virtual void InitializeNewDomain(AppDomainSetup appDomainInfo)
        {
        }

        internal void RegisterWithHost()
        {
            if (HasHost())
            {
                IntPtr zero = IntPtr.Zero;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    zero = Marshal.GetIUnknownForObject(this);
                    RegisterWithHost(zero);
                }
                finally
                {
                    if (!zero.IsNull())
                    {
                        Marshal.Release(zero);
                    }
                }
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void RegisterWithHost(IntPtr appDomainManager);

        public virtual System.Runtime.Hosting.ApplicationActivator ApplicationActivator
        {
            get
            {
                if (this.m_appActivator == null)
                {
                    this.m_appActivator = new System.Runtime.Hosting.ApplicationActivator();
                }
                return this.m_appActivator;
            }
        }

        internal static AppDomainManager CurrentAppDomainManager
        {
            [SecurityCritical]
            get
            {
                return AppDomain.CurrentDomain.DomainManager;
            }
        }

        public virtual Assembly EntryAssembly
        {
            [SecurityCritical]
            get
            {
                if (this.m_entryAssembly == null)
                {
                    AppDomain currentDomain = AppDomain.CurrentDomain;
                    if (currentDomain.IsDefaultAppDomain() && (currentDomain.ActivationContext != null))
                    {
                        ManifestRunner runner = new ManifestRunner(currentDomain, currentDomain.ActivationContext);
                        this.m_entryAssembly = runner.EntryAssembly;
                    }
                    else
                    {
                        RuntimeAssembly o = null;
                        GetEntryAssembly(JitHelpers.GetObjectHandleOnStack<RuntimeAssembly>(ref o));
                        this.m_entryAssembly = o;
                    }
                }
                return this.m_entryAssembly;
            }
        }

        public virtual System.Threading.HostExecutionContextManager HostExecutionContextManager
        {
            get
            {
                return System.Threading.HostExecutionContextManager.GetInternalHostExecutionContextManager();
            }
        }

        public virtual System.Security.HostSecurityManager HostSecurityManager
        {
            get
            {
                return null;
            }
        }

        public AppDomainManagerInitializationOptions InitializationFlags
        {
            get
            {
                return this.m_flags;
            }
            set
            {
                this.m_flags = value;
            }
        }
    }
}

