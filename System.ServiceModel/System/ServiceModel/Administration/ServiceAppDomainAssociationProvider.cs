namespace System.ServiceModel.Administration
{
    using System;

    internal class ServiceAppDomainAssociationProvider : ProviderBase, IWmiProvider
    {
        void IWmiProvider.EnumInstances(IWmiInstances instances)
        {
            foreach (ServiceInfo info in new ServiceInfoCollection(ManagementExtension.Services))
            {
                IWmiInstance inst = instances.NewInstance(null);
                inst.SetProperty("AppDomainInfo", AppDomainInstanceProvider.GetReference());
                inst.SetProperty("Service", ServiceInstanceProvider.GetReference(info));
                instances.AddInstance(inst);
            }
        }

        bool IWmiProvider.GetInstance(IWmiInstance instance)
        {
            string property = instance.GetProperty("Service") as string;
            string str2 = instance.GetProperty("AppDomainInfo") as string;
            return (!string.IsNullOrEmpty(property) && !string.IsNullOrEmpty(str2));
        }
    }
}

