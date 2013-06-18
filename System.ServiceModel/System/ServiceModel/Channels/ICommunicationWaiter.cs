namespace System.ServiceModel.Channels
{
    using System;

    internal interface ICommunicationWaiter : IDisposable
    {
        void Signal();
        CommunicationWaitResult Wait(TimeSpan timeout, bool aborting);
    }
}

