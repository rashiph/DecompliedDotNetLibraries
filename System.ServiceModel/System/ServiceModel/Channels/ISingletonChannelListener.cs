namespace System.ServiceModel.Channels
{
    using System;

    internal interface ISingletonChannelListener
    {
        void ReceiveRequest(RequestContext requestContext, Action callback, bool canDispatchOnThisThread);

        TimeSpan ReceiveTimeout { get; }
    }
}

