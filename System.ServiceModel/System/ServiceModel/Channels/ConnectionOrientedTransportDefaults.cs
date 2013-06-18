namespace System.ServiceModel.Channels
{
    using System;
    using System.Net.Security;
    using System.ServiceModel;

    internal static class ConnectionOrientedTransportDefaults
    {
        internal const bool AllowNtlm = true;
        internal const string ChannelInitializationTimeoutString = "00:00:05";
        internal const int ConnectionBufferSize = 0x2000;
        internal const string ConnectionPoolGroupName = "default";
        internal const System.ServiceModel.HostNameComparisonMode HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
        internal const string IdleTimeoutString = "00:02:00";
        internal const int MaxContentTypeSize = 0x100;
        internal const int MaxOutboundConnectionsPerEndpoint = 10;
        internal const string MaxOutputDelayString = "00:00:00.2";
        internal const int MaxPendingAccepts = 1;
        internal const int MaxPendingConnections = 10;
        internal const int MaxViaSize = 0x800;
        internal const System.Net.Security.ProtectionLevel ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
        internal const System.ServiceModel.TransferMode TransferMode = System.ServiceModel.TransferMode.Buffered;

        internal static TimeSpan ChannelInitializationTimeout
        {
            get
            {
                return TimeSpanHelper.FromSeconds(5, "00:00:05");
            }
        }

        internal static TimeSpan IdleTimeout
        {
            get
            {
                return TimeSpanHelper.FromMinutes(2, "00:02:00");
            }
        }

        internal static TimeSpan MaxOutputDelay
        {
            get
            {
                return TimeSpanHelper.FromMilliseconds(200, "00:00:00.2");
            }
        }
    }
}

