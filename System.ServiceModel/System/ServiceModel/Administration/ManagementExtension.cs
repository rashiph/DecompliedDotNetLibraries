namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;

    internal sealed class ManagementExtension
    {
        private static bool activated = false;
        private static bool isEnabled = GetIsWmiProviderEnabled();
        private static Dictionary<ServiceHostBase, DateTime> services;
        private static object syncRoot = new object();

        private static void Activate()
        {
            WbemProvider provider = new WbemProvider(@"root\ServiceModel", "ServiceModel");
            provider.Register("AppDomainInfo", new AppDomainInstanceProvider());
            provider.Register("Service", new ServiceInstanceProvider());
            provider.Register("Contract", new ContractInstanceProvider());
            provider.Register("Endpoint", new EndpointInstanceProvider());
            provider.Register("ServiceAppDomain", new ServiceAppDomainAssociationProvider());
            provider.Register("ServiceToEndpointAssociation", new ServiceEndpointAssociationProvider());
        }

        private static void Add(ServiceHostBase service)
        {
            Dictionary<ServiceHostBase, DateTime> services = GetServices();
            lock (services)
            {
                if (!services.ContainsKey(service))
                {
                    services.Add(service, DateTime.Now);
                }
            }
        }

        private static void EnsureManagementProvider()
        {
            if (!activated)
            {
                lock (syncRoot)
                {
                    if (!activated)
                    {
                        Activate();
                        activated = true;
                    }
                }
            }
        }

        [SecuritySafeCritical]
        private static bool GetIsWmiProviderEnabled()
        {
            return DiagnosticSection.UnsafeGetSection().WmiProviderEnabled;
        }

        private static Dictionary<ServiceHostBase, DateTime> GetServices()
        {
            if (services == null)
            {
                lock (syncRoot)
                {
                    if (services == null)
                    {
                        services = new Dictionary<ServiceHostBase, DateTime>();
                    }
                }
            }
            return services;
        }

        internal static DateTime GetTimeOpened(ServiceHostBase service)
        {
            return GetServices()[service];
        }

        public static void OnServiceClosing(ServiceHostBase serviceHostBase)
        {
            Remove(serviceHostBase);
        }

        public static void OnServiceOpened(ServiceHostBase serviceHostBase)
        {
            EnsureManagementProvider();
            Add(serviceHostBase);
        }

        private static void Remove(ServiceHostBase service)
        {
            Dictionary<ServiceHostBase, DateTime> services = GetServices();
            lock (services)
            {
                if (services.ContainsKey(service))
                {
                    services.Remove(service);
                }
            }
        }

        internal static bool IsActivated
        {
            get
            {
                return activated;
            }
        }

        internal static bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
        }

        internal static ICollection<ServiceHostBase> Services
        {
            get
            {
                return GetServices().Keys;
            }
        }
    }
}

