namespace System.ServiceModel.ComIntegration
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;

    internal interface IProvideChannelBuilderSettings
    {
        KeyedByTypeCollection<IEndpointBehavior> Behaviors { get; }

        System.ServiceModel.Channels.ServiceChannel ServiceChannel { get; }

        ServiceChannelFactory ServiceChannelFactoryReadOnly { get; }

        ServiceChannelFactory ServiceChannelFactoryReadWrite { get; }
    }
}

