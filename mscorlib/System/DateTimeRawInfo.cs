namespace System
{
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct DateTimeRawInfo
    {
        private unsafe int* num;
        internal int numCount;
        internal int month;
        internal int year;
        internal int dayOfWeek;
        internal int era;
        internal DateTimeParse.TM timeMark;
        internal double fraction;
        internal bool timeZone;
        [SecurityCritical]
        internal unsafe void Init(int* numberBuffer)
        {
            this.month = -1;
            this.year = -1;
            this.dayOfWeek = -1;
            this.era = -1;
            this.timeMark = DateTimeParse.TM.NotSet;
            this.fraction = -1.0;
            this.num = numberBuffer;
        }

        [SecuritySafeCritical]
        internal unsafe void AddNumber(int value)
        {
            this.num[this.numCount++] = value;
        }

        [SecuritySafeCritical]
        internal unsafe int GetNumber(int index)
        {
            return this.num[index];
        }
    }
}

