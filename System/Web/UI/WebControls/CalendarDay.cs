namespace System.Web.UI.WebControls
{
    using System;

    public class CalendarDay
    {
        private DateTime date;
        private string dayNumberText;
        private bool isOtherMonth;
        private bool isSelectable;
        private bool isSelected;
        private bool isToday;
        private bool isWeekend;

        public CalendarDay(DateTime date, bool isWeekend, bool isToday, bool isSelected, bool isOtherMonth, string dayNumberText)
        {
            this.date = date;
            this.isWeekend = isWeekend;
            this.isToday = isToday;
            this.isOtherMonth = isOtherMonth;
            this.isSelected = isSelected;
            this.dayNumberText = dayNumberText;
        }

        public DateTime Date
        {
            get
            {
                return this.date;
            }
        }

        public string DayNumberText
        {
            get
            {
                return this.dayNumberText;
            }
        }

        public bool IsOtherMonth
        {
            get
            {
                return this.isOtherMonth;
            }
        }

        public bool IsSelectable
        {
            get
            {
                return this.isSelectable;
            }
            set
            {
                this.isSelectable = value;
            }
        }

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
        }

        public bool IsToday
        {
            get
            {
                return this.isToday;
            }
        }

        public bool IsWeekend
        {
            get
            {
                return this.isWeekend;
            }
        }
    }
}

