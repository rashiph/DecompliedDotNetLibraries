namespace System.ServiceModel.Channels
{
    using System;

    internal interface IRequest : IRequestBase
    {
        void SendRequest(Message message, TimeSpan timeout);
        Message WaitForReply(TimeSpan timeout);
    }
}

