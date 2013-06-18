namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [KnownType(typeof(List<ServiceInfo>))]
    internal sealed class ServiceInfoCollection : Collection<ServiceInfo>
    {
        internal ServiceInfoCollection(IEnumerable<ServiceHostBase> services)
        {
            foreach (ServiceHostBase base2 in services)
            {
                base.Add(new ServiceInfo(base2));
            }
        }
    }
}

