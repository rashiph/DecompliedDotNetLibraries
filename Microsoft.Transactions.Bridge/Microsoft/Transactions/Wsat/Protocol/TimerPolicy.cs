namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TimerPolicy
    {
        public TimeSpan InitialDelay;
        public TimeSpan NotificationInterval;
        public TimeSpan MaxNotificationInterval;
        public uint MaxNotifications;
        public ushort IntervalIncreasePercentage;
    }
}

