namespace System
{
    using System.Globalization;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ParsingInfo
    {
        internal Calendar calendar;
        internal int dayOfWeek;
        internal DateTimeParse.TM timeMark;
        internal bool fUseHour12;
        internal bool fUseTwoDigitYear;
        internal bool fAllowInnerWhite;
        internal bool fAllowTrailingWhite;
        internal bool fCustomNumberParser;
        internal DateTimeParse.MatchNumberDelegate parseNumberDelegate;
        internal void Init()
        {
            this.dayOfWeek = -1;
            this.timeMark = DateTimeParse.TM.NotSet;
        }
    }
}

