namespace System.Xml.Schema
{
    using System;

    [Flags]
    internal enum XsdDateTimeFlags
    {
        AllXsd = 0xff,
        Date = 4,
        DateTime = 1,
        GDay = 0x40,
        GMonth = 0x80,
        GMonthDay = 0x20,
        GYear = 0x10,
        GYearMonth = 8,
        Time = 2,
        XdrDateTime = 0x200,
        XdrDateTimeNoTz = 0x100,
        XdrTimeNoTz = 0x400
    }
}

