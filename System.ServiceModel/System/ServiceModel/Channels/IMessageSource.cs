namespace System.ServiceModel.Channels
{
    using System;
    using System.Threading;

    internal interface IMessageSource
    {
        AsyncReceiveResult BeginReceive(TimeSpan timeout, WaitCallback callback, object state);
        AsyncReceiveResult BeginWaitForMessage(TimeSpan timeout, WaitCallback callback, object state);
        Message EndReceive();
        bool EndWaitForMessage();
        Message Receive(TimeSpan timeout);
        bool WaitForMessage(TimeSpan timeout);
    }
}

