namespace System.ServiceModel.Administration
{
    using System;

    internal class ServiceEndpointAssociationProvider : ProviderBase, IWmiProvider
    {
        void IWmiProvider.EnumInstances(IWmiInstances instances)
        {
            foreach (ServiceInfo info in new ServiceInfoCollection(ManagementExtension.Services))
            {
                string reference = ServiceInstanceProvider.GetReference(info);
                foreach (EndpointInfo info2 in info.Endpoints)
                {
                    IWmiInstance inst = instances.NewInstance(null);
                    string str2 = EndpointInstanceProvider.EndpointReference(info2.ListenUri, info2.Contract.Name);
                    inst.SetProperty("Endpoint", str2);
                    inst.SetProperty("Service", reference);
                    instances.AddInstance(inst);
                }
            }
        }

        bool IWmiProvider.GetInstance(IWmiInstance instance)
        {
            string property = instance.GetProperty("Service") as string;
            string str2 = instance.GetProperty("Endpoint") as string;
            return (!string.IsNullOrEmpty(property) && !string.IsNullOrEmpty(str2));
        }
    }
}

