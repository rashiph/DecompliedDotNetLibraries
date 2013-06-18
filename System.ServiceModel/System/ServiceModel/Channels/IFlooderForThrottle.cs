namespace System.ServiceModel.Channels
{
    using System;

    internal interface IFlooderForThrottle
    {
        void OnThrottleReached();
        void OnThrottleReleased();
    }
}

