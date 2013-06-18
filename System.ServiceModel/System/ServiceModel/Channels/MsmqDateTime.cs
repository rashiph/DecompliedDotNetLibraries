namespace System.ServiceModel.Channels
{
    using System;

    internal static class MsmqDateTime
    {
        public static DateTime ToDateTime(int seconds)
        {
            DateTime time = new DateTime(0x7b2, 1, 1);
            return time.AddSeconds((double) seconds);
        }
    }
}

