namespace System.ServiceModel
{
    using System;

    internal static class ServiceDefaults
    {
        internal const string CloseTimeoutString = "00:01:00";
        internal const string OpenTimeoutString = "00:01:00";
        internal const string ReceiveTimeoutString = "00:10:00";
        internal const string SendTimeoutString = "00:01:00";
        internal const string ServiceHostCloseTimeoutString = "00:00:10";
        internal const string TransactionTimeoutString = "00:00:00";

        internal static TimeSpan CloseTimeout
        {
            get
            {
                return TimeSpanHelper.FromMinutes(1, "00:01:00");
            }
        }

        internal static TimeSpan OpenTimeout
        {
            get
            {
                return TimeSpanHelper.FromMinutes(1, "00:01:00");
            }
        }

        internal static TimeSpan ReceiveTimeout
        {
            get
            {
                return TimeSpanHelper.FromMinutes(10, "00:10:00");
            }
        }

        internal static TimeSpan SendTimeout
        {
            get
            {
                return TimeSpanHelper.FromMinutes(1, "00:01:00");
            }
        }

        internal static TimeSpan ServiceHostCloseTimeout
        {
            get
            {
                return TimeSpanHelper.FromSeconds(10, "00:00:10");
            }
        }

        internal static TimeSpan TransactionTimeout
        {
            get
            {
                return TimeSpanHelper.FromMinutes(1, "00:00:00");
            }
        }
    }
}

