namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal static class ReliableSessionDefaults
    {
        internal const string AcknowledgementIntervalString = "00:00:00.2";
        internal const bool Enabled = false;
        internal const bool FlowControlEnabled = true;
        internal const string InactivityTimeoutString = "00:10:00";
        internal const int MaxPendingChannels = 4;
        internal const int MaxRetryCount = 8;
        internal const int MaxTransferWindowSize = 8;
        internal const bool Ordered = true;
        internal const string ReliableMessagingVersionString = "WSReliableMessagingFebruary2005";

        internal static TimeSpan AcknowledgementInterval
        {
            get
            {
                return TimeSpanHelper.FromMilliseconds(200, "00:00:00.2");
            }
        }

        internal static TimeSpan InactivityTimeout
        {
            get
            {
                return TimeSpanHelper.FromMinutes(10, "00:10:00");
            }
        }

        internal static System.ServiceModel.ReliableMessagingVersion ReliableMessagingVersion
        {
            get
            {
                return System.ServiceModel.ReliableMessagingVersion.WSReliableMessagingFebruary2005;
            }
        }
    }
}

