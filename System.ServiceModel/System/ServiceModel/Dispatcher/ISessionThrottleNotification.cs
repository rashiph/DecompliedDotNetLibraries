namespace System.ServiceModel.Dispatcher
{
    using System;

    internal interface ISessionThrottleNotification
    {
        void ThrottleAcquired();
    }
}

