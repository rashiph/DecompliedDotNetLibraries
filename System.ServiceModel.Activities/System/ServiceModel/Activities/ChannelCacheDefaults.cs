namespace System.ServiceModel.Activities
{
    using System;

    internal static class ChannelCacheDefaults
    {
        internal const bool DefaultAllowUnsafeSharing = false;
        internal static TimeSpan DefaultChannelLeaseTimeout = TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture);
        internal const string DefaultChannelLeaseTimeoutString = "00:05:00";
        internal static TimeSpan DefaultFactoryLeaseTimeout = TimeSpan.MaxValue;
        internal const string DefaultFactoryLeaseTimeoutString = "Infinite";
        internal static TimeSpan DefaultIdleTimeout = TimeSpan.Parse("00:02:00", CultureInfo.InvariantCulture);
        internal const string DefaultIdleTimeoutString = "00:02:00";
        internal static TimeSpan DefaultLeaseTimeout = TimeSpan.Parse("00:10:00", CultureInfo.InvariantCulture);
        internal const string DefaultLeaseTimeoutString = "00:10:00";
        internal static int DefaultMaxItemsPerCache = int.Parse("16", CultureInfo.CurrentCulture);
        internal const string DefaultMaxItemsPerCacheString = "16";
    }
}

