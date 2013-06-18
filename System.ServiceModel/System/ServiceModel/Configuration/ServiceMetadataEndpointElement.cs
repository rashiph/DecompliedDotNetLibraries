namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Description;

    public class ServiceMetadataEndpointElement : StandardEndpointElement
    {
        protected internal override ServiceEndpoint CreateServiceEndpoint(ContractDescription contractDescription)
        {
            return new ServiceMetadataEndpoint();
        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ChannelEndpointElement serviceEndpointElement)
        {
        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement)
        {
        }

        protected override void OnInitializeAndValidate(ChannelEndpointElement channelEndpointElement)
        {
            if (string.IsNullOrEmpty(channelEndpointElement.Binding))
            {
                channelEndpointElement.Binding = "mexHttpBinding";
            }
            channelEndpointElement.Contract = "IMetadataExchange";
        }

        protected override void OnInitializeAndValidate(ServiceEndpointElement serviceEndpointElement)
        {
            if (string.IsNullOrEmpty(serviceEndpointElement.Binding))
            {
                serviceEndpointElement.Binding = "mexHttpBinding";
            }
            serviceEndpointElement.Contract = "IMetadataExchange";
            serviceEndpointElement.IsSystemEndpoint = true;
        }

        protected internal override Type EndpointType
        {
            get
            {
                return typeof(ServiceMetadataEndpoint);
            }
        }
    }
}

