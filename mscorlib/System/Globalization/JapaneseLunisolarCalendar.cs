namespace System.Globalization
{
    using System;

    [Serializable]
    public class JapaneseLunisolarCalendar : EastAsianLunisolarCalendar
    {
        internal GregorianCalendarHelper helper;
        public const int JapaneseEra = 1;
        internal const int MAX_GREGORIAN_DAY = 0x16;
        internal const int MAX_GREGORIAN_MONTH = 1;
        internal const int MAX_GREGORIAN_YEAR = 0x802;
        internal const int MAX_LUNISOLAR_YEAR = 0x801;
        internal static DateTime maxDate;
        internal const int MIN_GREGORIAN_DAY = 0x1c;
        internal const int MIN_GREGORIAN_MONTH = 1;
        internal const int MIN_GREGORIAN_YEAR = 0x7a8;
        internal const int MIN_LUNISOLAR_YEAR = 0x7a8;
        internal static DateTime minDate = new DateTime(0x7a8, 1, 0x1c);
        private static readonly int[,] yinfo;

        static JapaneseLunisolarCalendar()
        {
            DateTime time = new DateTime(0x802, 1, 0x16, 0x17, 0x3b, 0x3b, 0x3e7);
            maxDate = new DateTime(time.Ticks + 0x270fL);
            yinfo = new int[,] { 
                { 6, 1, 0x1c, 0xad50 }, { 0, 2, 15, 0xab50 }, { 0, 2, 5, 0x4d60 }, { 4, 1, 0x19, 0xa570 }, { 0, 2, 13, 0xa570 }, { 0, 2, 2, 0x5270 }, { 3, 1, 0x16, 0x6930 }, { 0, 2, 9, 0xd950 }, { 7, 1, 30, 0x6aa8 }, { 0, 2, 0x11, 0x56a0 }, { 0, 2, 6, 0x9ad0 }, { 5, 1, 0x1b, 0x4ae8 }, { 0, 2, 15, 0x4ae0 }, { 0, 2, 3, 0xa4e0 }, { 4, 1, 0x17, 0xd268 }, { 0, 2, 11, 0xd250 }, 
                { 8, 1, 0x1f, 0xd548 }, { 0, 2, 0x12, 0xb540 }, { 0, 2, 7, 0xd6a0 }, { 6, 1, 0x1c, 0x96d0 }, { 0, 2, 0x10, 0x95b0 }, { 0, 2, 5, 0x49b0 }, { 4, 1, 0x19, 0xa4d8 }, { 0, 2, 13, 0xa4b0 }, { 10, 2, 2, 0xb258 }, { 0, 2, 20, 0x6a50 }, { 0, 2, 9, 0x6d40 }, { 6, 1, 0x1d, 0xb5a8 }, { 0, 2, 0x12, 0x2b60 }, { 0, 2, 6, 0x95b0 }, { 5, 1, 0x1b, 0x49b8 }, { 0, 2, 15, 0x4970 }, 
                { 0, 2, 4, 0x64b0 }, { 3, 1, 0x17, 0x6a50 }, { 0, 2, 10, 0xea50 }, { 8, 1, 0x1f, 0x6d48 }, { 0, 2, 0x13, 0x5ad0 }, { 0, 2, 8, 0x2b60 }, { 5, 1, 0x1c, 0x9370 }, { 0, 2, 0x10, 0x92e0 }, { 0, 2, 5, 0xc960 }, { 4, 1, 0x18, 0xe4a8 }, { 0, 2, 12, 0xd4a0 }, { 0, 2, 1, 0xda50 }, { 2, 1, 0x16, 0x5aa8 }, { 0, 2, 9, 0x56c0 }, { 7, 1, 0x1d, 0xaad8 }, { 0, 2, 0x12, 0x25d0 }, 
                { 0, 2, 7, 0x92d0 }, { 5, 1, 0x1a, 0xc958 }, { 0, 2, 14, 0xa950 }, { 0, 2, 3, 0xb4a0 }, { 3, 1, 0x17, 0xba50 }, { 0, 2, 10, 0xb550 }, { 9, 1, 0x1f, 0x55a8 }, { 0, 2, 0x13, 0x4ba0 }, { 0, 2, 8, 0xa5b0 }, { 5, 1, 0x1c, 0x52b8 }, { 0, 2, 0x10, 0x52b0 }, { 0, 2, 5, 0xa950 }, { 4, 1, 0x19, 0xb4a8 }, { 0, 2, 12, 0x6aa0 }, { 0, 2, 1, 0xad50 }, { 2, 1, 0x16, 0x55a8 }, 
                { 0, 2, 10, 0x4b60 }, { 6, 1, 0x1d, 0xa570 }, { 0, 2, 0x11, 0xa570 }, { 0, 2, 7, 0x5270 }, { 5, 1, 0x1b, 0x6930 }, { 0, 2, 13, 0xd930 }, { 0, 2, 3, 0x5aa0 }, { 3, 1, 0x17, 0xab50 }, { 0, 2, 11, 0x96d0 }, { 11, 1, 0x1f, 0x4ae8 }, { 0, 2, 0x13, 0x4ae0 }, { 0, 2, 8, 0xa4d0 }, { 6, 1, 0x1c, 0xd268 }, { 0, 2, 15, 0xd250 }, { 0, 2, 4, 0xd520 }, { 5, 1, 0x18, 0xdaa0 }, 
                { 0, 2, 12, 0xb6a0 }, { 0, 2, 1, 0x96d0 }, { 2, 1, 0x16, 0x4ad8 }, { 0, 2, 10, 0x49b0 }, { 7, 1, 30, 0xa4b8 }, { 0, 2, 0x11, 0xa4b0 }, { 0, 2, 6, 0xb250 }, { 5, 1, 0x1a, 0xb528 }, { 0, 2, 14, 0x6d40 }, { 0, 2, 2, 0xada0 }
             };
        }

        public JapaneseLunisolarCalendar()
        {
            this.helper = new GregorianCalendarHelper(this, TrimEras(JapaneseCalendar.GetEraInfo()));
        }

        public override int GetEra(DateTime time)
        {
            return this.helper.GetEra(time);
        }

        internal override int GetGregorianYear(int year, int era)
        {
            return this.helper.GetGregorianYear(year, era);
        }

        internal override int GetYear(int year, DateTime time)
        {
            return this.helper.GetYear(year, time);
        }

        internal override int GetYearInfo(int LunarYear, int Index)
        {
            if ((LunarYear < 0x7a8) || (LunarYear > 0x801))
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0x7a8, 0x801 }));
            }
            return yinfo[LunarYear - 0x7a8, Index];
        }

        private static EraInfo[] TrimEras(EraInfo[] baseEras)
        {
            EraInfo[] array = new EraInfo[baseEras.Length];
            int index = 0;
            for (int i = 0; i < baseEras.Length; i++)
            {
                if ((baseEras[i].yearOffset + baseEras[i].minEraYear) < 0x801)
                {
                    if ((baseEras[i].yearOffset + baseEras[i].maxEraYear) < 0x7a8)
                    {
                        break;
                    }
                    array[index] = baseEras[i];
                    index++;
                }
            }
            if (index == 0)
            {
                return baseEras;
            }
            Array.Resize<EraInfo>(ref array, index);
            return array;
        }

        internal override int BaseCalendarID
        {
            get
            {
                return 3;
            }
        }

        internal override EraInfo[] CalEraInfo
        {
            get
            {
                return JapaneseCalendar.GetEraInfo();
            }
        }

        public override int[] Eras
        {
            get
            {
                return this.helper.Eras;
            }
        }

        internal override int ID
        {
            get
            {
                return 14;
            }
        }

        internal override int MaxCalendarYear
        {
            get
            {
                return 0x801;
            }
        }

        internal override DateTime MaxDate
        {
            get
            {
                return maxDate;
            }
        }

        public override DateTime MaxSupportedDateTime
        {
            get
            {
                return maxDate;
            }
        }

        internal override int MinCalendarYear
        {
            get
            {
                return 0x7a8;
            }
        }

        internal override DateTime MinDate
        {
            get
            {
                return minDate;
            }
        }

        public override DateTime MinSupportedDateTime
        {
            get
            {
                return minDate;
            }
        }
    }
}

