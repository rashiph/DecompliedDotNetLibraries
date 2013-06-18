namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SystemTime
    {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;
        public SystemTime(DateTime date)
        {
            this.wYear = (short) date.Year;
            this.wMonth = (short) date.Month;
            this.wDayOfWeek = (short) date.DayOfWeek;
            this.wDay = (short) date.Day;
            this.wHour = (short) date.Hour;
            this.wMinute = (short) date.Minute;
            this.wSecond = (short) date.Second;
            this.wMilliseconds = (short) date.Millisecond;
        }
    }
}

