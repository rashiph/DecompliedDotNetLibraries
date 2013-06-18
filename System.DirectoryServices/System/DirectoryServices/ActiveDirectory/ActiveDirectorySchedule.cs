namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.ComponentModel;
    using System.DirectoryServices;

    public class ActiveDirectorySchedule
    {
        private bool[] scheduleArray;
        private long utcOffSet;

        public ActiveDirectorySchedule()
        {
            this.scheduleArray = new bool[0x2a0];
            this.utcOffSet = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Ticks / 0x861c46800L;
        }

        public ActiveDirectorySchedule(ActiveDirectorySchedule schedule) : this()
        {
            if (schedule == null)
            {
                throw new ArgumentNullException();
            }
            bool[] scheduleArray = schedule.scheduleArray;
            for (int i = 0; i < 0x2a0; i++)
            {
                this.scheduleArray[i] = scheduleArray[i];
            }
        }

        internal ActiveDirectorySchedule(bool[] schedule) : this()
        {
            for (int i = 0; i < 0x2a0; i++)
            {
                this.scheduleArray[i] = schedule[i];
            }
        }

        internal byte[] GetUnmanagedSchedule()
        {
            byte num = 0;
            int index = 0;
            byte[] buffer = new byte[0xbc];
            int num3 = 0;
            buffer[0] = 0xbc;
            buffer[8] = 1;
            buffer[0x10] = 20;
            for (int i = 20; i < 0xbc; i++)
            {
                num = 0;
                index = (i - 20) * 4;
                if (this.scheduleArray[index])
                {
                    num = (byte) (num | 1);
                }
                if (this.scheduleArray[index + 1])
                {
                    num = (byte) (num | 2);
                }
                if (this.scheduleArray[index + 2])
                {
                    num = (byte) (num | 4);
                }
                if (this.scheduleArray[index + 3])
                {
                    num = (byte) (num | 8);
                }
                num3 = i - ((int) this.utcOffSet);
                if (num3 >= 0xbc)
                {
                    num3 = (num3 - 0xbc) + 20;
                }
                else if (num3 < 20)
                {
                    num3 = 0xbc - (20 - num3);
                }
                buffer[num3] = num;
            }
            return buffer;
        }

        public void ResetSchedule()
        {
            for (int i = 0; i < 0x2a0; i++)
            {
                this.scheduleArray[i] = false;
            }
        }

        public void SetDailySchedule(HourOfDay fromHour, MinuteOfHour fromMinute, HourOfDay toHour, MinuteOfHour toMinute)
        {
            for (int i = 0; i < 7; i++)
            {
                this.SetSchedule((DayOfWeek) i, fromHour, fromMinute, toHour, toMinute);
            }
        }

        public void SetSchedule(DayOfWeek day, HourOfDay fromHour, MinuteOfHour fromMinute, HourOfDay toHour, MinuteOfHour toMinute)
        {
            if ((day < DayOfWeek.Sunday) || (day > DayOfWeek.Saturday))
            {
                throw new InvalidEnumArgumentException("day", (int) day, typeof(DayOfWeek));
            }
            if ((fromHour < HourOfDay.Zero) || (fromHour > HourOfDay.TwentyThree))
            {
                throw new InvalidEnumArgumentException("fromHour", (int) fromHour, typeof(HourOfDay));
            }
            if (((fromMinute != MinuteOfHour.Zero) && (fromMinute != MinuteOfHour.Fifteen)) && ((fromMinute != MinuteOfHour.Thirty) && (fromMinute != MinuteOfHour.FortyFive)))
            {
                throw new InvalidEnumArgumentException("fromMinute", (int) fromMinute, typeof(MinuteOfHour));
            }
            if ((toHour < HourOfDay.Zero) || (toHour > HourOfDay.TwentyThree))
            {
                throw new InvalidEnumArgumentException("toHour", (int) toHour, typeof(HourOfDay));
            }
            if (((toMinute != MinuteOfHour.Zero) && (toMinute != MinuteOfHour.Fifteen)) && ((toMinute != MinuteOfHour.Thirty) && (toMinute != MinuteOfHour.FortyFive)))
            {
                throw new InvalidEnumArgumentException("toMinute", (int) toMinute, typeof(MinuteOfHour));
            }
            if (((fromHour * ((HourOfDay) 60)) + ((HourOfDay) ((int) fromMinute))) > ((toHour * ((HourOfDay) 60)) + ((HourOfDay) ((int) toMinute))))
            {
                throw new ArgumentException(Res.GetString("InvalidTime"));
            }
            int num = (int) ((((day * ((DayOfWeek) 0x18)) * DayOfWeek.Thursday) + ((DayOfWeek) ((int) (fromHour * HourOfDay.Four)))) + ((DayOfWeek) ((int) (fromMinute / MinuteOfHour.Fifteen))));
            int num2 = (int) ((((day * ((DayOfWeek) 0x18)) * DayOfWeek.Thursday) + ((DayOfWeek) ((int) (toHour * HourOfDay.Four)))) + ((DayOfWeek) ((int) (toMinute / MinuteOfHour.Fifteen))));
            for (int i = num; i <= num2; i++)
            {
                this.scheduleArray[i] = true;
            }
        }

        public void SetSchedule(DayOfWeek[] days, HourOfDay fromHour, MinuteOfHour fromMinute, HourOfDay toHour, MinuteOfHour toMinute)
        {
            if (days == null)
            {
                throw new ArgumentNullException("days");
            }
            for (int i = 0; i < days.Length; i++)
            {
                if ((days[i] < DayOfWeek.Sunday) || (days[i] > DayOfWeek.Saturday))
                {
                    throw new InvalidEnumArgumentException("days", (int) days[i], typeof(DayOfWeek));
                }
            }
            for (int j = 0; j < days.Length; j++)
            {
                this.SetSchedule(days[j], fromHour, fromMinute, toHour, toMinute);
            }
        }

        internal void SetUnmanagedSchedule(byte[] unmanagedSchedule)
        {
            int num = 0;
            int index = 0;
            int num3 = 0;
            for (int i = 20; i < 0xbc; i++)
            {
                num = 0;
                index = (i - 20) * 4;
                num3 = i - ((int) this.utcOffSet);
                if (num3 >= 0xbc)
                {
                    num3 = (num3 - 0xbc) + 20;
                }
                else if (num3 < 20)
                {
                    num3 = 0xbc - (20 - num3);
                }
                num = unmanagedSchedule[num3];
                if ((num & 1) != 0)
                {
                    this.scheduleArray[index] = true;
                }
                if ((num & 2) != 0)
                {
                    this.scheduleArray[index + 1] = true;
                }
                if ((num & 4) != 0)
                {
                    this.scheduleArray[index + 2] = true;
                }
                if ((num & 8) != 0)
                {
                    this.scheduleArray[index + 3] = true;
                }
            }
        }

        private void ValidateRawArray(bool[,,] array)
        {
            if (array.Length != 0x2a0)
            {
                throw new ArgumentException("value");
            }
            int length = array.GetLength(0);
            int num2 = array.GetLength(1);
            int num3 = array.GetLength(2);
            if (((length != 7) || (num2 != 0x18)) || (num3 != 4))
            {
                throw new ArgumentException("value");
            }
        }

        public bool[,,] RawSchedule
        {
            get
            {
                bool[,,] flagArray = new bool[7, 0x18, 4];
                for (int i = 0; i < 7; i++)
                {
                    for (int j = 0; j < 0x18; j++)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            flagArray[i, j, k] = this.scheduleArray[(((i * 0x18) * 4) + (j * 4)) + k];
                        }
                    }
                }
                return flagArray;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.ValidateRawArray(value);
                for (int i = 0; i < 7; i++)
                {
                    for (int j = 0; j < 0x18; j++)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            this.scheduleArray[(((i * 0x18) * 4) + (j * 4)) + k] = value[i, j, k];
                        }
                    }
                }
            }
        }
    }
}

