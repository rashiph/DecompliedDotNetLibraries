namespace System.Windows.Forms
{
    using System;

    public class DateRangeEventArgs : EventArgs
    {
        private readonly DateTime end;
        private readonly DateTime start;

        public DateRangeEventArgs(DateTime start, DateTime end)
        {
            this.start = start;
            this.end = end;
        }

        public DateTime End
        {
            get
            {
                return this.end;
            }
        }

        public DateTime Start
        {
            get
            {
                return this.start;
            }
        }
    }
}

