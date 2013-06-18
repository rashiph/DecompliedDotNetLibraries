namespace System.Web.Util
{
    using System;

    internal sealed class DateTimeUtil
    {
        private const long FileTimeOffset = 0x701ce1722770000L;
        private static readonly DateTime MaxValueMinusOneDay = DateTime.MaxValue.AddDays(-1.0);
        private static readonly DateTime MinValuePlusOneDay = DateTime.MinValue.AddDays(1.0);

        private DateTimeUtil()
        {
        }

        internal static DateTime ConvertToLocalTime(DateTime utcTime)
        {
            if (utcTime < MinValuePlusOneDay)
            {
                return DateTime.MinValue;
            }
            if (utcTime > MaxValueMinusOneDay)
            {
                return DateTime.MaxValue;
            }
            return utcTime.ToLocalTime();
        }

        internal static DateTime ConvertToUniversalTime(DateTime localTime)
        {
            if (localTime < MinValuePlusOneDay)
            {
                return DateTime.MinValue;
            }
            if (localTime > MaxValueMinusOneDay)
            {
                return DateTime.MaxValue;
            }
            return localTime.ToUniversalTime();
        }

        internal static DateTime FromFileTimeToUtc(long filetime)
        {
            return new DateTime(filetime + 0x701ce1722770000L, DateTimeKind.Utc);
        }
    }
}

