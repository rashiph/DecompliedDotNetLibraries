namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum BatteryChargeStatus
    {
        Charging = 8,
        Critical = 4,
        High = 1,
        Low = 2,
        NoSystemBattery = 0x80,
        Unknown = 0xff
    }
}

