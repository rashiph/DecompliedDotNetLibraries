namespace System.Runtime.Caching.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Runtime.Caching.Resources;

    internal static class ConfigUtil
    {
        internal const string CacheMemoryLimitMegabytes = "cacheMemoryLimitMegabytes";
        internal const int DefaultPollingTimeMilliseconds = 0x1d4c0;
        internal const string PhysicalMemoryLimitPercentage = "physicalMemoryLimitPercentage";
        internal const string PollingInterval = "pollingInterval";

        internal static int GetIntValue(NameValueCollection config, string valueName, int defaultValue, bool zeroAllowed, int maxValueAllowed)
        {
            int num;
            string s = config[valueName];
            if (s == null)
            {
                return defaultValue;
            }
            if ((!int.TryParse(s, out num) || (num < 0)) || (!zeroAllowed && (num == 0)))
            {
                if (zeroAllowed)
                {
                    throw new ArgumentException(RH.Format(R.Value_must_be_non_negative_integer, new object[] { valueName, s }), "config");
                }
                throw new ArgumentException(RH.Format(R.Value_must_be_positive_integer, new object[] { valueName, s }), "config");
            }
            if ((maxValueAllowed > 0) && (num > maxValueAllowed))
            {
                throw new ArgumentException(RH.Format(R.Value_too_big, new object[] { valueName, s, maxValueAllowed.ToString(CultureInfo.InvariantCulture) }), "config");
            }
            return num;
        }

        internal static int GetIntValueFromTimeSpan(NameValueCollection config, string valueName, int defaultValue)
        {
            TimeSpan span;
            string s = config[valueName];
            switch (s)
            {
                case null:
                    return defaultValue;

                case "Infinite":
                    return 0x7fffffff;
            }
            if (!TimeSpan.TryParse(s, out span) || (span <= TimeSpan.Zero))
            {
                throw new ArgumentException(RH.Format(R.TimeSpan_invalid_format, new object[] { valueName, s }), "config");
            }
            double totalMilliseconds = span.TotalMilliseconds;
            return ((totalMilliseconds < 2147483647.0) ? ((int) totalMilliseconds) : 0x7fffffff);
        }
    }
}

