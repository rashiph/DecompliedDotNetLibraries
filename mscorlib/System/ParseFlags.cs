namespace System
{
    [Flags]
    internal enum ParseFlags
    {
        CaptureOffset = 0x800,
        HaveDate = 0x80,
        HaveDay = 4,
        HaveHour = 8,
        HaveMinute = 0x10,
        HaveMonth = 2,
        HaveSecond = 0x20,
        HaveTime = 0x40,
        HaveYear = 1,
        ParsedMonthName = 0x400,
        Rfc1123Pattern = 0x2000,
        TimeZoneUsed = 0x100,
        TimeZoneUtc = 0x200,
        UtcSortPattern = 0x4000,
        YearDefault = 0x1000
    }
}

