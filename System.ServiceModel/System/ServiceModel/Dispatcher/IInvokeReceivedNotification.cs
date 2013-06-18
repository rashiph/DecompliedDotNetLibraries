namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;

    internal interface IInvokeReceivedNotification
    {
        void NotifyInvokeReceived();
        void NotifyInvokeReceived(RequestContext request);
    }
}

