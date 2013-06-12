namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    [TypeConverter(typeof(SelectionRangeConverter))]
    public sealed class SelectionRange
    {
        private DateTime end;
        private DateTime start;

        public SelectionRange()
        {
            this.start = DateTime.MinValue.Date;
            this.end = DateTime.MaxValue.Date;
        }

        public SelectionRange(SelectionRange range)
        {
            this.start = DateTime.MinValue.Date;
            this.end = DateTime.MaxValue.Date;
            this.start = range.start;
            this.end = range.end;
        }

        public SelectionRange(DateTime lower, DateTime upper)
        {
            this.start = DateTime.MinValue.Date;
            this.end = DateTime.MaxValue.Date;
            if (lower < upper)
            {
                this.start = lower.Date;
                this.end = upper.Date;
            }
            else
            {
                this.start = upper.Date;
                this.end = lower.Date;
            }
        }

        public override string ToString()
        {
            return ("SelectionRange: Start: " + this.start.ToString() + ", End: " + this.end.ToString());
        }

        public DateTime End
        {
            get
            {
                return this.end;
            }
            set
            {
                this.end = value.Date;
            }
        }

        public DateTime Start
        {
            get
            {
                return this.start;
            }
            set
            {
                this.start = value.Date;
            }
        }
    }
}

