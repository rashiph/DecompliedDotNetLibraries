namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal static class TcpTransportDefaults
    {
        internal const string ConnectionLeaseTimeoutString = "00:05:00";
        internal const int ListenBacklog = 10;
        internal const bool PortSharingEnabled = false;
        internal const bool TeredoEnabled = false;

        internal static TimeSpan ConnectionLeaseTimeout
        {
            get
            {
                return TimeSpanHelper.FromMinutes(5, "00:05:00");
            }
        }
    }
}

