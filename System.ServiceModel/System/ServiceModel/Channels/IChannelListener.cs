namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    public interface IChannelListener : ICommunicationObject
    {
        IAsyncResult BeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state);
        bool EndWaitForChannel(IAsyncResult result);
        T GetProperty<T>() where T: class;
        bool WaitForChannel(TimeSpan timeout);

        System.Uri Uri { get; }
    }
}

