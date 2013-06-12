namespace System.Web.Management
{
    using System;
    using System.Collections;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Configuration;

    public static class WebEventManager
    {
        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public static void Flush()
        {
            HealthMonitoringSectionHelper.ProviderInstances providerInstances = HealthMonitoringManager.ProviderInstances;
            if (providerInstances != null)
            {
                using (new ApplicationImpersonationContext())
                {
                    foreach (DictionaryEntry entry in providerInstances)
                    {
                        ((WebEventProvider) entry.Value).Flush();
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public static void Flush(string providerName)
        {
            HealthMonitoringSectionHelper.ProviderInstances providerInstances = HealthMonitoringManager.ProviderInstances;
            if (providerInstances != null)
            {
                if (!providerInstances.ContainsKey(providerName))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Health_mon_provider_not_found", new object[] { providerName }));
                }
                using (new ApplicationImpersonationContext())
                {
                    providerInstances[providerName].Flush();
                }
            }
        }

        internal static void Shutdown()
        {
            HealthMonitoringSectionHelper.ProviderInstances providerInstances = HealthMonitoringManager.ProviderInstances;
            if (providerInstances != null)
            {
                foreach (DictionaryEntry entry in providerInstances)
                {
                    ((WebEventProvider) entry.Value).Shutdown();
                }
            }
        }
    }
}

