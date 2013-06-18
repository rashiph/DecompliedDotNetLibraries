namespace System.ServiceModel.Channels
{
    using System;

    internal interface IAsyncRequest : IAsyncResult, IRequestBase
    {
        void BeginSendRequest(Message message, TimeSpan timeout);
        Message End();
    }
}

