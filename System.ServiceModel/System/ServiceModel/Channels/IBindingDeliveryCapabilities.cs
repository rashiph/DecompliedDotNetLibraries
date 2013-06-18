namespace System.ServiceModel.Channels
{
    using System;

    public interface IBindingDeliveryCapabilities
    {
        bool AssuresOrderedDelivery { get; }

        bool QueuedDelivery { get; }
    }
}

