namespace System.Web.UI.WebControls
{
    using System;

    public class MonthChangedEventArgs
    {
        private DateTime newDate;
        private DateTime previousDate;

        public MonthChangedEventArgs(DateTime newDate, DateTime previousDate)
        {
            this.newDate = newDate;
            this.previousDate = previousDate;
        }

        public DateTime NewDate
        {
            get
            {
                return this.newDate;
            }
        }

        public DateTime PreviousDate
        {
            get
            {
                return this.previousDate;
            }
        }
    }
}

