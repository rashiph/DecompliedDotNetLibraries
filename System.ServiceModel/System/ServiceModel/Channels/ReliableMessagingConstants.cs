namespace System.ServiceModel.Channels
{
    using System;

    internal static class ReliableMessagingConstants
    {
        public static int MaxSequenceRanges = 0x80;
        public static TimeSpan RequestorIterationTime = TimeSpan.FromSeconds(10.0);
        public static TimeSpan RequestorReceiveTime = TimeSpan.FromSeconds(10.0);
        public static TimeSpan UnknownInitiationTime = TimeSpan.FromSeconds(2.0);
    }
}

