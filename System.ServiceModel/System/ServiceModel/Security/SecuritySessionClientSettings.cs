namespace System.ServiceModel.Security
{
    using System;

    internal static class SecuritySessionClientSettings
    {
        internal static readonly TimeSpan defaultKeyRenewalInterval = TimeSpan.Parse("10:00:00", CultureInfo.InvariantCulture);
        internal const string defaultKeyRenewalIntervalString = "10:00:00";
        internal static readonly TimeSpan defaultKeyRolloverInterval = TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture);
        internal const string defaultKeyRolloverIntervalString = "00:05:00";
        internal const bool defaultTolerateTransportFailures = true;
    }
}

