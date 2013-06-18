namespace System.ServiceModel.Description
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [DebuggerDisplay("Address={address}"), DebuggerDisplay("Name={name}")]
    public class ServiceMetadataEndpoint : ServiceEndpoint
    {
        public ServiceMetadataEndpoint() : this(MetadataExchangeBindings.CreateMexHttpBinding(), null)
        {
        }

        public ServiceMetadataEndpoint(EndpointAddress address) : this(MetadataExchangeBindings.CreateMexHttpBinding(), address)
        {
        }

        public ServiceMetadataEndpoint(Binding binding, EndpointAddress address) : base(ServiceMetadataBehavior.MexContract, binding, address)
        {
            base.IsSystemEndpoint = true;
        }
    }
}

