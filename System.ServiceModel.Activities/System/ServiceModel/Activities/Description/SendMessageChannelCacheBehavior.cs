namespace System.ServiceModel.Activities.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    internal class SendMessageChannelCacheBehavior : IServiceBehavior
    {
        public void AddBindingParameters(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            WorkflowServiceHost host = serviceHostBase as WorkflowServiceHost;
            if (host != null)
            {
                host.WorkflowExtensions.Add(new SendMessageChannelCache(this.FactorySettings, this.ChannelSettings, this.AllowUnsafeCaching));
            }
        }

        public void Validate(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        public bool AllowUnsafeCaching { get; set; }

        public ChannelCacheSettings ChannelSettings { get; set; }

        public ChannelCacheSettings FactorySettings { get; set; }
    }
}

