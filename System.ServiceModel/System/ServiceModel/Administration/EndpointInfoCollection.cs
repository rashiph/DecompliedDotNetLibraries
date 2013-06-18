namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Description;

    internal sealed class EndpointInfoCollection : Collection<System.ServiceModel.Administration.EndpointInfo>
    {
        internal EndpointInfoCollection(ServiceEndpointCollection endpoints, string serviceName)
        {
            for (int i = 0; i < endpoints.Count; i++)
            {
                base.Add(new System.ServiceModel.Administration.EndpointInfo(endpoints[i], serviceName));
            }
        }
    }
}

