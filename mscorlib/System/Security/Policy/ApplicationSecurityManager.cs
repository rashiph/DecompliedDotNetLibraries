namespace System.Security.Policy
{
    using System;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true)]
    public static class ApplicationSecurityManager
    {
        private static IApplicationTrustManager m_appTrustManager = null;
        private static string s_machineConfigFile = (Config.MachineDirectory + "applicationtrust.config");

        [SecurityCritical]
        private static SecurityElement CreateDefaultApplicationTrustManagerElement()
        {
            SecurityElement element = new SecurityElement("IApplicationTrustManager");
            element.AddAttribute("class", "System.Security.Policy.TrustManager, System.Windows.Forms, Version=" + ((RuntimeAssembly) Assembly.GetExecutingAssembly()).GetVersion() + ", Culture=neutral, PublicKeyToken=b77a5c561934e089");
            element.AddAttribute("version", "1");
            return element;
        }

        [SecurityCritical]
        private static IApplicationTrustManager DecodeAppTrustManager()
        {
            if (File.InternalExists(s_machineConfigFile))
            {
                string str;
                using (FileStream stream = new FileStream(s_machineConfigFile, FileMode.Open, FileAccess.Read))
                {
                    str = new StreamReader(stream).ReadToEnd();
                }
                SecurityElement element2 = SecurityElement.FromString(str).SearchForChildByTag("mscorlib");
                if (element2 != null)
                {
                    SecurityElement element3 = element2.SearchForChildByTag("security");
                    if (element3 != null)
                    {
                        SecurityElement element4 = element3.SearchForChildByTag("policy");
                        if (element4 != null)
                        {
                            SecurityElement element5 = element4.SearchForChildByTag("ApplicationSecurityManager");
                            if (element5 != null)
                            {
                                SecurityElement elTrustManager = element5.SearchForChildByTag("IApplicationTrustManager");
                                if (elTrustManager != null)
                                {
                                    IApplicationTrustManager manager = DecodeAppTrustManagerFromElement(elTrustManager);
                                    if (manager != null)
                                    {
                                        return manager;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return DecodeAppTrustManagerFromElement(CreateDefaultApplicationTrustManagerElement());
        }

        [SecurityCritical]
        private static IApplicationTrustManager DecodeAppTrustManagerFromElement(SecurityElement elTrustManager)
        {
            new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Assert();
            Type type = Type.GetType(elTrustManager.Attribute("class"), false, false);
            if (type == null)
            {
                return null;
            }
            IApplicationTrustManager manager = Activator.CreateInstance(type) as IApplicationTrustManager;
            if (manager != null)
            {
                manager.FromXml(elTrustManager);
            }
            return manager;
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, Unrestricted=true)]
        public static bool DetermineApplicationTrust(ActivationContext activationContext, TrustManagerContext context)
        {
            if (activationContext == null)
            {
                throw new ArgumentNullException("activationContext");
            }
            ApplicationTrust trust = null;
            AppDomainManager domainManager = AppDomain.CurrentDomain.DomainManager;
            if (domainManager != null)
            {
                HostSecurityManager hostSecurityManager = domainManager.HostSecurityManager;
                if ((hostSecurityManager != null) && ((hostSecurityManager.Flags & HostSecurityManagerOptions.HostDetermineApplicationTrust) == HostSecurityManagerOptions.HostDetermineApplicationTrust))
                {
                    trust = hostSecurityManager.DetermineApplicationTrust(CmsUtils.MergeApplicationEvidence(null, activationContext.Identity, activationContext, null), null, context);
                    if (trust == null)
                    {
                        return false;
                    }
                    return trust.IsApplicationTrustedToRun;
                }
            }
            trust = DetermineApplicationTrustInternal(activationContext, context);
            if (trust == null)
            {
                return false;
            }
            return trust.IsApplicationTrustedToRun;
        }

        [SecurityCritical]
        internal static ApplicationTrust DetermineApplicationTrustInternal(ActivationContext activationContext, TrustManagerContext context)
        {
            ApplicationTrust trust = null;
            ApplicationTrustCollection trusts = new ApplicationTrustCollection(true);
            if ((context == null) || !context.IgnorePersistedDecision)
            {
                trust = trusts[activationContext.Identity.FullName];
                if (trust != null)
                {
                    return trust;
                }
            }
            trust = ApplicationTrustManager.DetermineApplicationTrust(activationContext, context);
            if (trust == null)
            {
                trust = new ApplicationTrust(activationContext.Identity);
            }
            trust.ApplicationIdentity = activationContext.Identity;
            if (trust.Persist)
            {
                trusts.Add(trust);
            }
            return trust;
        }

        public static IApplicationTrustManager ApplicationTrustManager
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
            get
            {
                if (m_appTrustManager == null)
                {
                    m_appTrustManager = DecodeAppTrustManager();
                    if (m_appTrustManager == null)
                    {
                        throw new PolicyException(Environment.GetResourceString("Policy_NoTrustManager"));
                    }
                }
                return m_appTrustManager;
            }
        }

        public static ApplicationTrustCollection UserApplicationTrusts
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
            get
            {
                return new ApplicationTrustCollection(true);
            }
        }
    }
}

