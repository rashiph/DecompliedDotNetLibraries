namespace System
{
    internal enum TokenType
    {
        Am = 3,
        DateWordToken = 10,
        DayOfWeekToken = 7,
        EndOfString = 6,
        EraToken = 9,
        HebrewNumber = 12,
        IgnorableSymbol = 15,
        JapaneseEraToken = 13,
        MonthToken = 5,
        NumberToken = 1,
        Pm = 4,
        RegularTokenMask = 0xff,
        SEP_Am = 0x400,
        SEP_Date = 0x600,
        SEP_DateOrOffset = 0xf00,
        SEP_DaySuff = 0xa00,
        SEP_End = 0x200,
        SEP_HourSuff = 0xb00,
        SEP_LocalTimeMark = 0xe00,
        SEP_MinuteSuff = 0xc00,
        SEP_MonthSuff = 0x900,
        SEP_Pm = 0x500,
        SEP_SecondSuff = 0xd00,
        SEP_Space = 0x300,
        SEP_Time = 0x700,
        SEP_Unk = 0x100,
        SEP_YearSuff = 0x800,
        SeparatorTokenMask = 0xff00,
        TEraToken = 14,
        TimeZoneToken = 8,
        UnknownToken = 11,
        YearNumberToken = 2
    }
}

