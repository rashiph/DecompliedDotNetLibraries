namespace System.ServiceModel.Channels
{
    using System;

    internal static class PeerTransportConstants
    {
        public const int AckTimeout = 0x7530;
        public const uint AckWindow = 0x20;
        public const int ConnectTimeout = 0xea60;
        public static TimeSpan ForwardInterval = TimeSpan.FromSeconds(10.0);
        public static TimeSpan ForwardTimeout = TimeSpan.FromSeconds(60.0);
        public const int IdealNeighbors = 3;
        public const ulong InvalidNodeId = 0L;
        public const int MaintainerInterval = 0x493e0;
        public const int MaintainerRetryInterval = 0x2710;
        public const int MaintainerTimeout = 0x1d4c0;
        public const ulong MaxHopCount = ulong.MaxValue;
        public const int MaxNeighbors = 7;
        public static int MaxOutgoingMessages = 0x80;
        public const int MaxPort = 0xffff;
        public const int MaxReferralCacheSize = 50;
        public const int MaxReferrals = 10;
        public const int MaxResolveAddresses = 3;
        public const int MessageThreshold = 0x20;
        public const long MinMessageSize = 0x4000L;
        public const int MinNeighbors = 2;
        public const int MinPort = 0;
        public const int UnregisterTimeout = 0x1d4c0;
    }
}

