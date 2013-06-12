namespace System.Net
{
    using System;

    internal class RequestLifetimeSetter
    {
        private long m_RequestStartTimestamp;

        internal RequestLifetimeSetter(long requestStartTimestamp)
        {
            this.m_RequestStartTimestamp = requestStartTimestamp;
        }

        internal static void Report(RequestLifetimeSetter tracker)
        {
            if (tracker != null)
            {
                NetworkingPerfCounters.Instance.IncrementAverage(NetworkingPerfCounterName.HttpWebRequestAvgLifeTime, tracker.m_RequestStartTimestamp);
            }
        }
    }
}

