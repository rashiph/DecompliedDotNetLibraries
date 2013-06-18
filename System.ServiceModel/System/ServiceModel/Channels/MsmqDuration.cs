namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal static class MsmqDuration
    {
        public static int FromTimeSpan(TimeSpan timeSpan)
        {
            long totalSeconds = (long) timeSpan.TotalSeconds;
            if (totalSeconds > 0x7fffffffL)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqTimeSpanTooLarge")));
            }
            return (int) totalSeconds;
        }

        public static TimeSpan ToTimeSpan(int seconds)
        {
            return TimeSpan.FromSeconds((double) seconds);
        }
    }
}

