namespace System.Web.UI.WebControls
{
    using System;

    public sealed class DayRenderEventArgs
    {
        private TableCell cell;
        private CalendarDay day;
        private string selectUrl;

        public DayRenderEventArgs(TableCell cell, CalendarDay day)
        {
            this.day = day;
            this.cell = cell;
        }

        public DayRenderEventArgs(TableCell cell, CalendarDay day, string selectUrl)
        {
            this.day = day;
            this.cell = cell;
            this.selectUrl = selectUrl;
        }

        public TableCell Cell
        {
            get
            {
                return this.cell;
            }
        }

        public CalendarDay Day
        {
            get
            {
                return this.day;
            }
        }

        public string SelectUrl
        {
            get
            {
                return this.selectUrl;
            }
        }
    }
}

