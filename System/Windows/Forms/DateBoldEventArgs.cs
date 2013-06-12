namespace System.Windows.Forms
{
    using System;

    public class DateBoldEventArgs : EventArgs
    {
        private int[] daysToBold;
        private readonly int size;
        private readonly DateTime startDate;

        internal DateBoldEventArgs(DateTime start, int size)
        {
            this.startDate = start;
            this.size = size;
        }

        public int[] DaysToBold
        {
            get
            {
                return this.daysToBold;
            }
            set
            {
                this.daysToBold = value;
            }
        }

        public int Size
        {
            get
            {
                return this.size;
            }
        }

        public DateTime StartDate
        {
            get
            {
                return this.startDate;
            }
        }
    }
}

