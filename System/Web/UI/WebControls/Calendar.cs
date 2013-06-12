namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [DataBindingHandler("System.Web.UI.Design.WebControls.CalendarDataBindingHandler, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("SelectedDate"), Designer("System.Web.UI.Design.WebControls.CalendarDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), SupportsEventValidation, ControlValueProperty("SelectedDate", typeof(DateTime), "1/1/0001"), DefaultEvent("SelectionChanged")]
    public class Calendar : WebControl, IPostBackEventHandler
    {
        private static DateTime baseDate = new DateTime(0x7d0, 1, 1);
        private const int cachedNumberMax = 0x1f;
        private static readonly string[] cachedNumbers = new string[] { 
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", 
            "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31"
         };
        private ArrayList dateList;
        private TableItemStyle dayHeaderStyle;
        private TableItemStyle dayStyle;
        private string defaultButtonColorText;
        private Color defaultForeColor;
        private static readonly Color DefaultForeColor = Color.Black;
        private static readonly object EventDayRender = new object();
        private static readonly object EventSelectionChanged = new object();
        private static readonly object EventVisibleMonthChanged = new object();
        private DateTime maxSupportedDate;
        private DateTime minSupportedDate;
        private const string NAVIGATE_MONTH_COMMAND = "V";
        private TableItemStyle nextPrevStyle;
        private TableItemStyle otherMonthDayStyle;
        private const string ROWBEGINTAG = "<tr>";
        private const string ROWENDTAG = "</tr>";
        private const string SELECT_RANGE_COMMAND = "R";
        private SelectedDatesCollection selectedDates;
        private TableItemStyle selectedDayStyle;
        private TableItemStyle selectorStyle;
        private const int STYLEMASK_DAY = 0x10;
        private const int STYLEMASK_OTHERMONTH = 2;
        private const int STYLEMASK_SELECTED = 8;
        private const int STYLEMASK_TODAY = 4;
        private const int STYLEMASK_UNIQUE = 15;
        private const int STYLEMASK_WEEKEND = 1;
        private System.Globalization.Calendar threadCalendar;
        private TableItemStyle titleStyle;
        private TableItemStyle todayDayStyle;
        private TableItemStyle weekendDayStyle;

        [WebCategory("Action"), WebSysDescription("Calendar_OnDayRender")]
        public event DayRenderEventHandler DayRender
        {
            add
            {
                base.Events.AddHandler(EventDayRender, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDayRender, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("Calendar_OnSelectionChanged")]
        public event EventHandler SelectionChanged
        {
            add
            {
                base.Events.AddHandler(EventSelectionChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSelectionChanged, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("Calendar_OnVisibleMonthChanged")]
        public event MonthChangedEventHandler VisibleMonthChanged
        {
            add
            {
                base.Events.AddHandler(EventVisibleMonthChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventVisibleMonthChanged, value);
            }
        }

        public Calendar() : base(HtmlTextWriterTag.Table)
        {
        }

        private void ApplyTitleStyle(TableCell titleCell, Table titleTable, TableItemStyle titleStyle)
        {
            if (titleStyle.BackColor != Color.Empty)
            {
                titleCell.BackColor = titleStyle.BackColor;
            }
            if (titleStyle.BorderColor != Color.Empty)
            {
                titleCell.BorderColor = titleStyle.BorderColor;
            }
            if (titleStyle.BorderWidth != Unit.Empty)
            {
                titleCell.BorderWidth = titleStyle.BorderWidth;
            }
            if (titleStyle.BorderStyle != BorderStyle.NotSet)
            {
                titleCell.BorderStyle = titleStyle.BorderStyle;
            }
            if (titleStyle.Height != Unit.Empty)
            {
                titleCell.Height = titleStyle.Height;
            }
            if (titleStyle.VerticalAlign != VerticalAlign.NotSet)
            {
                titleCell.VerticalAlign = titleStyle.VerticalAlign;
            }
            if (titleStyle.CssClass.Length > 0)
            {
                titleTable.CssClass = titleStyle.CssClass;
            }
            else if (this.CssClass.Length > 0)
            {
                titleTable.CssClass = this.CssClass;
            }
            if (titleStyle.ForeColor != Color.Empty)
            {
                titleTable.ForeColor = titleStyle.ForeColor;
            }
            else if (this.ForeColor != Color.Empty)
            {
                titleTable.ForeColor = this.ForeColor;
            }
            titleTable.Font.CopyFrom(titleStyle.Font);
            titleTable.Font.MergeWith(this.Font);
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new InternalControlCollection(this);
        }

        private DateTime EffectiveVisibleDate()
        {
            DateTime visibleDate = this.VisibleDate;
            if (visibleDate.Equals(DateTime.MinValue))
            {
                visibleDate = this.TodaysDate;
            }
            if (this.IsMinSupportedYearMonth(visibleDate))
            {
                return this.minSupportedDate;
            }
            return this.threadCalendar.AddDays(visibleDate, -(this.threadCalendar.GetDayOfMonth(visibleDate) - 1));
        }

        private DateTime FirstCalendarDay(DateTime visibleDate)
        {
            DateTime date = visibleDate;
            if (this.IsMinSupportedYearMonth(date))
            {
                return date;
            }
            int num = ((int) this.threadCalendar.GetDayOfWeek(date)) - this.NumericFirstDayOfWeek();
            if (num <= 0)
            {
                num += 7;
            }
            return this.threadCalendar.AddDays(date, -num);
        }

        private string GetCalendarButtonText(string eventArgument, string buttonText, string title, bool showLink, Color foreColor)
        {
            if (!showLink)
            {
                return buttonText;
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("<a href=\"");
            builder.Append(this.Page.ClientScript.GetPostBackClientHyperlink(this, eventArgument, true));
            builder.Append("\" style=\"color:");
            builder.Append(foreColor.IsEmpty ? this.defaultButtonColorText : ColorTranslator.ToHtml(foreColor));
            if (!string.IsNullOrEmpty(title))
            {
                builder.Append("\" title=\"");
                builder.Append(title);
            }
            builder.Append("\">");
            builder.Append(buttonText);
            builder.Append("</a>");
            return builder.ToString();
        }

        private int GetDefinedStyleMask()
        {
            int num = 8;
            if ((this.dayStyle != null) && !this.dayStyle.IsEmpty)
            {
                num |= 0x10;
            }
            if ((this.todayDayStyle != null) && !this.todayDayStyle.IsEmpty)
            {
                num |= 4;
            }
            if ((this.otherMonthDayStyle != null) && !this.otherMonthDayStyle.IsEmpty)
            {
                num |= 2;
            }
            if ((this.weekendDayStyle != null) && !this.weekendDayStyle.IsEmpty)
            {
                num |= 1;
            }
            return num;
        }

        private string GetMonthName(int m, bool bFull)
        {
            if (bFull)
            {
                return DateTimeFormatInfo.CurrentInfo.GetMonthName(m);
            }
            return DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(m);
        }

        protected bool HasWeekSelectors(CalendarSelectionMode selectionMode)
        {
            if (selectionMode != CalendarSelectionMode.DayWeek)
            {
                return (selectionMode == CalendarSelectionMode.DayWeekMonth);
            }
            return true;
        }

        private bool IsMaxSupportedYearMonth(DateTime date)
        {
            return this.IsTheSameYearMonth(this.maxSupportedDate, date);
        }

        private bool IsMinSupportedYearMonth(DateTime date)
        {
            return this.IsTheSameYearMonth(this.minSupportedDate, date);
        }

        private bool IsTheSameYearMonth(DateTime date1, DateTime date2)
        {
            return (((this.threadCalendar.GetEra(date1) == this.threadCalendar.GetEra(date2)) && (this.threadCalendar.GetYear(date1) == this.threadCalendar.GetYear(date2))) && (this.threadCalendar.GetMonth(date1) == this.threadCalendar.GetMonth(date2)));
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                object[] objArray = (object[]) savedState;
                if (objArray[0] != null)
                {
                    base.LoadViewState(objArray[0]);
                }
                if (objArray[1] != null)
                {
                    ((IStateManager) this.TitleStyle).LoadViewState(objArray[1]);
                }
                if (objArray[2] != null)
                {
                    ((IStateManager) this.NextPrevStyle).LoadViewState(objArray[2]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.DayStyle).LoadViewState(objArray[3]);
                }
                if (objArray[4] != null)
                {
                    ((IStateManager) this.DayHeaderStyle).LoadViewState(objArray[4]);
                }
                if (objArray[5] != null)
                {
                    ((IStateManager) this.TodayDayStyle).LoadViewState(objArray[5]);
                }
                if (objArray[6] != null)
                {
                    ((IStateManager) this.WeekendDayStyle).LoadViewState(objArray[6]);
                }
                if (objArray[7] != null)
                {
                    ((IStateManager) this.OtherMonthDayStyle).LoadViewState(objArray[7]);
                }
                if (objArray[8] != null)
                {
                    ((IStateManager) this.SelectedDayStyle).LoadViewState(objArray[8]);
                }
                if (objArray[9] != null)
                {
                    ((IStateManager) this.SelectorStyle).LoadViewState(objArray[9]);
                }
                ArrayList list = (ArrayList) this.ViewState["SD"];
                if (list != null)
                {
                    this.dateList = list;
                    this.selectedDates = null;
                }
            }
        }

        private int NumericFirstDayOfWeek()
        {
            if (this.FirstDayOfWeek != System.Web.UI.WebControls.FirstDayOfWeek.Default)
            {
                return (int) this.FirstDayOfWeek;
            }
            return (int) DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek;
        }

        protected virtual void OnDayRender(TableCell cell, CalendarDay day)
        {
            DayRenderEventHandler handler = (DayRenderEventHandler) base.Events[EventDayRender];
            if (handler != null)
            {
                int days = day.Date.Subtract(baseDate).Days;
                string selectUrl = null;
                if (this.Page != null)
                {
                    string argument = days.ToString(CultureInfo.InvariantCulture);
                    selectUrl = this.Page.ClientScript.GetPostBackClientHyperlink(this, argument, true);
                }
                handler(this, new DayRenderEventArgs(cell, day, selectUrl));
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (this.Page != null)
            {
                this.Page.RegisterPostBackScript();
            }
        }

        protected virtual void OnSelectionChanged()
        {
            EventHandler handler = (EventHandler) base.Events[EventSelectionChanged];
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected virtual void OnVisibleMonthChanged(DateTime newDate, DateTime previousDate)
        {
            MonthChangedEventHandler handler = (MonthChangedEventHandler) base.Events[EventVisibleMonthChanged];
            if (handler != null)
            {
                handler(this, new MonthChangedEventArgs(newDate, previousDate));
            }
        }

        protected virtual void RaisePostBackEvent(string eventArgument)
        {
            base.ValidateEvent(this.UniqueID, eventArgument);
            if (base.AdapterInternal != null)
            {
                IPostBackEventHandler adapterInternal = base.AdapterInternal as IPostBackEventHandler;
                if (adapterInternal != null)
                {
                    adapterInternal.RaisePostBackEvent(eventArgument);
                }
            }
            else if (string.Compare(eventArgument, 0, "V", 0, "V".Length, StringComparison.Ordinal) == 0)
            {
                DateTime visibleDate = this.VisibleDate;
                if (visibleDate.Equals(DateTime.MinValue))
                {
                    visibleDate = this.TodaysDate;
                }
                int num = int.Parse(eventArgument.Substring("V".Length), CultureInfo.InvariantCulture);
                this.VisibleDate = baseDate.AddDays((double) num);
                if (this.VisibleDate == DateTime.MinValue)
                {
                    this.VisibleDate = DateTimeFormatInfo.CurrentInfo.Calendar.AddDays(this.VisibleDate, 1);
                }
                this.OnVisibleMonthChanged(this.VisibleDate, visibleDate);
            }
            else if (string.Compare(eventArgument, 0, "R", 0, "R".Length, StringComparison.Ordinal) == 0)
            {
                int num2 = int.Parse(eventArgument.Substring("R".Length), CultureInfo.InvariantCulture);
                int num3 = num2 / 100;
                int num4 = num2 % 100;
                if (num4 < 1)
                {
                    num4 = 100 + num4;
                    num3--;
                }
                DateTime dateFrom = baseDate.AddDays((double) num3);
                this.SelectRange(dateFrom, dateFrom.AddDays((double) (num4 - 1)));
            }
            else
            {
                int num5 = int.Parse(eventArgument, CultureInfo.InvariantCulture);
                DateTime time3 = baseDate.AddDays((double) num5);
                this.SelectRange(time3, time3);
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            bool isEnabled;
            this.threadCalendar = DateTimeFormatInfo.CurrentInfo.Calendar;
            this.minSupportedDate = this.threadCalendar.MinSupportedDateTime;
            this.maxSupportedDate = this.threadCalendar.MaxSupportedDateTime;
            DateTime visibleDate = this.EffectiveVisibleDate();
            DateTime firstDay = this.FirstCalendarDay(visibleDate);
            CalendarSelectionMode selectionMode = this.SelectionMode;
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            if ((this.Page == null) || base.DesignMode)
            {
                isEnabled = false;
            }
            else
            {
                isEnabled = base.IsEnabled;
            }
            this.defaultForeColor = this.ForeColor;
            if (this.defaultForeColor == Color.Empty)
            {
                this.defaultForeColor = DefaultForeColor;
            }
            this.defaultButtonColorText = ColorTranslator.ToHtml(this.defaultForeColor);
            Table table = new Table();
            if (this.ID != null)
            {
                table.ID = this.ClientID;
            }
            table.CopyBaseAttributes(this);
            if (base.ControlStyleCreated)
            {
                table.ApplyStyle(base.ControlStyle);
            }
            table.Width = this.Width;
            table.Height = this.Height;
            table.CellPadding = this.CellPadding;
            table.CellSpacing = this.CellSpacing;
            if ((!base.ControlStyleCreated || !base.ControlStyle.IsSet(0x20)) || this.BorderWidth.Equals(Unit.Empty))
            {
                table.BorderWidth = Unit.Pixel(1);
            }
            if (this.ShowGridLines)
            {
                table.GridLines = GridLines.Both;
            }
            else
            {
                table.GridLines = GridLines.None;
            }
            bool useAccessibleHeader = this.UseAccessibleHeader;
            if (useAccessibleHeader && (table.Attributes["title"] == null))
            {
                table.Attributes["title"] = System.Web.SR.GetString("Calendar_TitleText");
            }
            string caption = this.Caption;
            if (caption.Length > 0)
            {
                table.Caption = caption;
                table.CaptionAlign = this.CaptionAlign;
            }
            table.RenderBeginTag(writer);
            if (this.ShowTitle)
            {
                this.RenderTitle(writer, visibleDate, selectionMode, isEnabled, useAccessibleHeader);
            }
            if (this.ShowDayHeader)
            {
                this.RenderDayHeader(writer, visibleDate, selectionMode, isEnabled, useAccessibleHeader);
            }
            this.RenderDays(writer, firstDay, visibleDate, selectionMode, isEnabled, useAccessibleHeader);
            table.RenderEndTag(writer);
        }

        private void RenderCalendarCell(HtmlTextWriter writer, TableItemStyle style, string text, string title, bool hasButton, string eventArgument)
        {
            style.AddAttributesToRender(writer, this);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            if (hasButton)
            {
                Color foreColor = style.ForeColor;
                writer.Write("<a href=\"");
                writer.Write(this.Page.ClientScript.GetPostBackClientHyperlink(this, eventArgument, true));
                writer.Write("\" style=\"color:");
                writer.Write(foreColor.IsEmpty ? this.defaultButtonColorText : ColorTranslator.ToHtml(foreColor));
                if (!string.IsNullOrEmpty(title))
                {
                    writer.Write("\" title=\"");
                    writer.Write(title);
                }
                writer.Write("\">");
                writer.Write(text);
                writer.Write("</a>");
            }
            else
            {
                writer.Write(text);
            }
            writer.RenderEndTag();
        }

        private void RenderCalendarHeaderCell(HtmlTextWriter writer, TableItemStyle style, string text, string abbrText)
        {
            style.AddAttributesToRender(writer, this);
            writer.AddAttribute("abbr", abbrText);
            writer.AddAttribute("scope", "col");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);
            writer.Write(text);
            writer.RenderEndTag();
        }

        private void RenderDayHeader(HtmlTextWriter writer, DateTime visibleDate, CalendarSelectionMode selectionMode, bool buttonsActive, bool useAccessibleHeader)
        {
            writer.Write("<tr>");
            DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
            if (this.HasWeekSelectors(selectionMode))
            {
                TableItemStyle style = new TableItemStyle {
                    HorizontalAlign = HorizontalAlign.Center
                };
                if (selectionMode == CalendarSelectionMode.DayWeekMonth)
                {
                    int days = visibleDate.Subtract(baseDate).Days;
                    int dayOfMonth = this.threadCalendar.GetDaysInMonth(this.threadCalendar.GetYear(visibleDate), this.threadCalendar.GetMonth(visibleDate), this.threadCalendar.GetEra(visibleDate));
                    if (this.IsMinSupportedYearMonth(visibleDate))
                    {
                        dayOfMonth = (dayOfMonth - this.threadCalendar.GetDayOfMonth(visibleDate)) + 1;
                    }
                    else if (this.IsMaxSupportedYearMonth(visibleDate))
                    {
                        dayOfMonth = this.threadCalendar.GetDayOfMonth(this.maxSupportedDate);
                    }
                    string eventArgument = "R" + (((days * 100) + dayOfMonth)).ToString(CultureInfo.InvariantCulture);
                    style.CopyFrom(this.SelectorStyle);
                    string title = null;
                    if (useAccessibleHeader)
                    {
                        title = System.Web.SR.GetString("Calendar_SelectMonthTitle");
                    }
                    this.RenderCalendarCell(writer, style, this.SelectMonthText, title, buttonsActive, eventArgument);
                }
                else
                {
                    style.CopyFrom(this.DayHeaderStyle);
                    this.RenderCalendarCell(writer, style, string.Empty, null, false, null);
                }
            }
            TableItemStyle style2 = new TableItemStyle {
                HorizontalAlign = HorizontalAlign.Center
            };
            style2.CopyFrom(this.DayHeaderStyle);
            System.Web.UI.WebControls.DayNameFormat dayNameFormat = this.DayNameFormat;
            int num3 = this.NumericFirstDayOfWeek();
            for (int i = num3; i < (num3 + 7); i++)
            {
                string dayName;
                int num5 = i % 7;
                switch (dayNameFormat)
                {
                    case System.Web.UI.WebControls.DayNameFormat.Full:
                        dayName = currentInfo.GetDayName((DayOfWeek) num5);
                        break;

                    case System.Web.UI.WebControls.DayNameFormat.FirstLetter:
                        dayName = currentInfo.GetDayName((DayOfWeek) num5).Substring(0, 1);
                        break;

                    case System.Web.UI.WebControls.DayNameFormat.FirstTwoLetters:
                        dayName = currentInfo.GetDayName((DayOfWeek) num5).Substring(0, 2);
                        break;

                    case System.Web.UI.WebControls.DayNameFormat.Shortest:
                        dayName = currentInfo.GetShortestDayName((DayOfWeek) num5);
                        break;

                    default:
                        dayName = currentInfo.GetAbbreviatedDayName((DayOfWeek) num5);
                        break;
                }
                if (useAccessibleHeader)
                {
                    string abbrText = currentInfo.GetDayName((DayOfWeek) num5);
                    this.RenderCalendarHeaderCell(writer, style2, dayName, abbrText);
                }
                else
                {
                    this.RenderCalendarCell(writer, style2, dayName, null, false, null);
                }
            }
            writer.Write("</tr>");
        }

        private void RenderDays(HtmlTextWriter writer, DateTime firstDay, DateTime visibleDate, CalendarSelectionMode selectionMode, bool buttonsActive, bool useAccessibleHeader)
        {
            Unit unit;
            DateTime time = firstDay;
            TableItemStyle style = null;
            bool flag = this.HasWeekSelectors(selectionMode);
            if (flag)
            {
                style = new TableItemStyle {
                    Width = Unit.Percentage(12.0),
                    HorizontalAlign = HorizontalAlign.Center
                };
                style.CopyFrom(this.SelectorStyle);
                unit = Unit.Percentage(12.0);
            }
            else
            {
                unit = Unit.Percentage(14.0);
            }
            bool flag2 = !(this.threadCalendar is HebrewCalendar);
            bool flag3 = (base.GetType() != typeof(System.Web.UI.WebControls.Calendar)) || (base.Events[EventDayRender] != null);
            TableItemStyle[] styleArray = new TableItemStyle[0x10];
            int definedStyleMask = this.GetDefinedStyleMask();
            DateTime todaysDate = this.TodaysDate;
            string selectWeekText = this.SelectWeekText;
            bool hasButton = buttonsActive && (selectionMode != CalendarSelectionMode.None);
            int month = this.threadCalendar.GetMonth(visibleDate);
            int days = firstDay.Subtract(baseDate).Days;
            bool flag5 = base.DesignMode && (this.SelectionMode != CalendarSelectionMode.None);
            int num4 = 0;
            if (this.IsMinSupportedYearMonth(visibleDate))
            {
                num4 = ((int) this.threadCalendar.GetDayOfWeek(firstDay)) - this.NumericFirstDayOfWeek();
                if (num4 < 0)
                {
                    num4 += 7;
                }
            }
            bool flag6 = false;
            DateTime time3 = this.threadCalendar.AddMonths(this.maxSupportedDate, -1);
            bool flag7 = this.IsMaxSupportedYearMonth(visibleDate) || this.IsTheSameYearMonth(time3, visibleDate);
            for (int i = 0; i < 6; i++)
            {
                if (flag6)
                {
                    return;
                }
                writer.Write("<tr>");
                if (flag)
                {
                    int num6 = (days * 100) + 7;
                    if (num4 > 0)
                    {
                        num6 -= num4;
                    }
                    else if (flag7)
                    {
                        int num7 = this.maxSupportedDate.Subtract(time).Days;
                        if (num7 < 6)
                        {
                            num6 -= 6 - num7;
                        }
                    }
                    string eventArgument = "R" + num6.ToString(CultureInfo.InvariantCulture);
                    string title = null;
                    if (useAccessibleHeader)
                    {
                        int num8 = i + 1;
                        title = System.Web.SR.GetString("Calendar_SelectWeekTitle", new object[] { num8.ToString(CultureInfo.InvariantCulture) });
                    }
                    this.RenderCalendarCell(writer, style, selectWeekText, title, buttonsActive, eventArgument);
                }
                for (int j = 0; j < 7; j++)
                {
                    string str4;
                    if (num4 > 0)
                    {
                        j += num4;
                        while (num4 > 0)
                        {
                            writer.RenderBeginTag(HtmlTextWriterTag.Td);
                            writer.RenderEndTag();
                            num4--;
                        }
                    }
                    else if (flag6)
                    {
                        while (j < 7)
                        {
                            writer.RenderBeginTag(HtmlTextWriterTag.Td);
                            writer.RenderEndTag();
                            j++;
                        }
                        break;
                    }
                    int dayOfWeek = (int) this.threadCalendar.GetDayOfWeek(time);
                    int dayOfMonth = this.threadCalendar.GetDayOfMonth(time);
                    if ((dayOfMonth <= 0x1f) && flag2)
                    {
                        str4 = cachedNumbers[dayOfMonth];
                    }
                    else
                    {
                        str4 = time.ToString("dd", CultureInfo.CurrentCulture);
                    }
                    CalendarDay day = new CalendarDay(time, (dayOfWeek == 0) || (dayOfWeek == 6), time.Equals(todaysDate), (this.selectedDates != null) && this.selectedDates.Contains(time), this.threadCalendar.GetMonth(time) != month, str4);
                    int num12 = 0x10;
                    if (day.IsSelected)
                    {
                        num12 |= 8;
                    }
                    if (day.IsOtherMonth)
                    {
                        num12 |= 2;
                    }
                    if (day.IsToday)
                    {
                        num12 |= 4;
                    }
                    if (day.IsWeekend)
                    {
                        num12 |= 1;
                    }
                    int styleMask = definedStyleMask & num12;
                    int index = styleMask & 15;
                    TableItemStyle style2 = styleArray[index];
                    if (style2 == null)
                    {
                        style2 = new TableItemStyle();
                        this.SetDayStyles(style2, styleMask, unit);
                        styleArray[index] = style2;
                    }
                    string str5 = null;
                    if (useAccessibleHeader)
                    {
                        str5 = time.ToString("m", CultureInfo.CurrentCulture);
                    }
                    if (flag3)
                    {
                        TableCell cell = new TableCell();
                        cell.ApplyStyle(style2);
                        LiteralControl child = new LiteralControl(str4);
                        cell.Controls.Add(child);
                        day.IsSelectable = hasButton;
                        this.OnDayRender(cell, day);
                        child.Text = this.GetCalendarButtonText(days.ToString(CultureInfo.InvariantCulture), str4, str5, buttonsActive && day.IsSelectable, cell.ForeColor);
                        cell.RenderControl(writer);
                    }
                    else
                    {
                        if (flag5 && style2.ForeColor.IsEmpty)
                        {
                            style2.ForeColor = this.defaultForeColor;
                        }
                        this.RenderCalendarCell(writer, style2, str4, str5, hasButton, days.ToString(CultureInfo.InvariantCulture));
                    }
                    if ((flag7 && (time.Month == this.maxSupportedDate.Month)) && (time.Day == this.maxSupportedDate.Day))
                    {
                        flag6 = true;
                    }
                    else
                    {
                        time = this.threadCalendar.AddDays(time, 1);
                        days++;
                    }
                }
                writer.Write("</tr>");
            }
        }

        private void RenderTitle(HtmlTextWriter writer, DateTime visibleDate, CalendarSelectionMode selectionMode, bool buttonsActive, bool useAccessibleHeader)
        {
            string str4;
            writer.Write("<tr>");
            TableCell titleCell = new TableCell();
            Table titleTable = new Table();
            titleCell.ColumnSpan = this.HasWeekSelectors(selectionMode) ? 8 : 7;
            titleCell.BackColor = Color.Silver;
            titleTable.GridLines = GridLines.None;
            titleTable.Width = Unit.Percentage(100.0);
            titleTable.CellSpacing = 0;
            TableItemStyle titleStyle = this.TitleStyle;
            this.ApplyTitleStyle(titleCell, titleTable, titleStyle);
            titleCell.RenderBeginTag(writer);
            titleTable.RenderBeginTag(writer);
            writer.Write("<tr>");
            System.Web.UI.WebControls.NextPrevFormat nextPrevFormat = this.NextPrevFormat;
            TableItemStyle style = new TableItemStyle {
                Width = Unit.Percentage(15.0)
            };
            style.CopyFrom(this.NextPrevStyle);
            if (this.ShowNextPrevMonth)
            {
                if (this.IsMinSupportedYearMonth(visibleDate))
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.RenderEndTag();
                }
                else
                {
                    string monthName;
                    DateTime minSupportedDate;
                    switch (nextPrevFormat)
                    {
                        case System.Web.UI.WebControls.NextPrevFormat.ShortMonth:
                        case System.Web.UI.WebControls.NextPrevFormat.FullMonth:
                        {
                            int month = this.threadCalendar.GetMonth(this.threadCalendar.AddMonths(visibleDate, -1));
                            monthName = this.GetMonthName(month, nextPrevFormat == System.Web.UI.WebControls.NextPrevFormat.FullMonth);
                            break;
                        }
                        default:
                            monthName = this.PrevMonthText;
                            break;
                    }
                    DateTime time2 = this.threadCalendar.AddMonths(this.minSupportedDate, 1);
                    if (this.IsTheSameYearMonth(time2, visibleDate))
                    {
                        minSupportedDate = this.minSupportedDate;
                    }
                    else
                    {
                        minSupportedDate = this.threadCalendar.AddMonths(visibleDate, -1);
                    }
                    string eventArgument = "V" + minSupportedDate.Subtract(baseDate).Days.ToString(CultureInfo.InvariantCulture);
                    string title = null;
                    if (useAccessibleHeader)
                    {
                        title = System.Web.SR.GetString("Calendar_PreviousMonthTitle");
                    }
                    this.RenderCalendarCell(writer, style, monthName, title, buttonsActive, eventArgument);
                }
            }
            TableItemStyle style3 = new TableItemStyle();
            if (titleStyle.HorizontalAlign != HorizontalAlign.NotSet)
            {
                style3.HorizontalAlign = titleStyle.HorizontalAlign;
            }
            else
            {
                style3.HorizontalAlign = HorizontalAlign.Center;
            }
            style3.Wrap = titleStyle.Wrap;
            style3.Width = Unit.Percentage(70.0);
            switch (this.TitleFormat)
            {
                case System.Web.UI.WebControls.TitleFormat.Month:
                    str4 = visibleDate.ToString("MMMM", CultureInfo.CurrentCulture);
                    break;

                default:
                {
                    string yearMonthPattern = DateTimeFormatInfo.CurrentInfo.YearMonthPattern;
                    if (yearMonthPattern.IndexOf(',') >= 0)
                    {
                        yearMonthPattern = "MMMM yyyy";
                    }
                    str4 = visibleDate.ToString(yearMonthPattern, CultureInfo.CurrentCulture);
                    break;
                }
            }
            this.RenderCalendarCell(writer, style3, str4, null, false, null);
            if (this.ShowNextPrevMonth)
            {
                if (this.IsMaxSupportedYearMonth(visibleDate))
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.RenderEndTag();
                }
                else
                {
                    string nextMonthText;
                    style.HorizontalAlign = HorizontalAlign.Right;
                    switch (nextPrevFormat)
                    {
                        case System.Web.UI.WebControls.NextPrevFormat.ShortMonth:
                        case System.Web.UI.WebControls.NextPrevFormat.FullMonth:
                        {
                            int m = this.threadCalendar.GetMonth(this.threadCalendar.AddMonths(visibleDate, 1));
                            nextMonthText = this.GetMonthName(m, nextPrevFormat == System.Web.UI.WebControls.NextPrevFormat.FullMonth);
                            break;
                        }
                        default:
                            nextMonthText = this.NextMonthText;
                            break;
                    }
                    DateTime time3 = this.threadCalendar.AddMonths(visibleDate, 1);
                    string str7 = "V" + time3.Subtract(baseDate).Days.ToString(CultureInfo.InvariantCulture);
                    string str8 = null;
                    if (useAccessibleHeader)
                    {
                        str8 = System.Web.SR.GetString("Calendar_NextMonthTitle");
                    }
                    this.RenderCalendarCell(writer, style, nextMonthText, str8, buttonsActive, str7);
                }
            }
            writer.Write("</tr>");
            titleTable.RenderEndTag(writer);
            titleCell.RenderEndTag(writer);
            writer.Write("</tr>");
        }

        protected override object SaveViewState()
        {
            if (this.SelectedDates.Count > 0)
            {
                this.ViewState["SD"] = this.dateList;
            }
            object[] objArray = new object[] { base.SaveViewState(), (this.titleStyle != null) ? ((IStateManager) this.titleStyle).SaveViewState() : null, (this.nextPrevStyle != null) ? ((IStateManager) this.nextPrevStyle).SaveViewState() : null, (this.dayStyle != null) ? ((IStateManager) this.dayStyle).SaveViewState() : null, (this.dayHeaderStyle != null) ? ((IStateManager) this.dayHeaderStyle).SaveViewState() : null, (this.todayDayStyle != null) ? ((IStateManager) this.todayDayStyle).SaveViewState() : null, (this.weekendDayStyle != null) ? ((IStateManager) this.weekendDayStyle).SaveViewState() : null, (this.otherMonthDayStyle != null) ? ((IStateManager) this.otherMonthDayStyle).SaveViewState() : null, (this.selectedDayStyle != null) ? ((IStateManager) this.selectedDayStyle).SaveViewState() : null, (this.selectorStyle != null) ? ((IStateManager) this.selectorStyle).SaveViewState() : null };
            for (int i = 0; i < objArray.Length; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
        }

        private void SelectRange(DateTime dateFrom, DateTime dateTo)
        {
            TimeSpan span = (TimeSpan) (dateTo - dateFrom);
            if (((this.SelectedDates.Count != (span.Days + 1)) || (this.SelectedDates[0] != dateFrom)) || (this.SelectedDates[this.SelectedDates.Count - 1] != dateTo))
            {
                this.SelectedDates.SelectRange(dateFrom, dateTo);
                this.OnSelectionChanged();
            }
        }

        private void SetDayStyles(TableItemStyle style, int styleMask, Unit defaultWidth)
        {
            style.Width = defaultWidth;
            style.HorizontalAlign = HorizontalAlign.Center;
            if ((styleMask & 0x10) != 0)
            {
                style.CopyFrom(this.DayStyle);
            }
            if ((styleMask & 1) != 0)
            {
                style.CopyFrom(this.WeekendDayStyle);
            }
            if ((styleMask & 2) != 0)
            {
                style.CopyFrom(this.OtherMonthDayStyle);
            }
            if ((styleMask & 4) != 0)
            {
                style.CopyFrom(this.TodayDayStyle);
            }
            if ((styleMask & 8) != 0)
            {
                style.ForeColor = Color.White;
                style.BackColor = Color.Silver;
                style.CopyFrom(this.SelectedDayStyle);
            }
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this.titleStyle != null)
            {
                ((IStateManager) this.titleStyle).TrackViewState();
            }
            if (this.nextPrevStyle != null)
            {
                ((IStateManager) this.nextPrevStyle).TrackViewState();
            }
            if (this.dayStyle != null)
            {
                ((IStateManager) this.dayStyle).TrackViewState();
            }
            if (this.dayHeaderStyle != null)
            {
                ((IStateManager) this.dayHeaderStyle).TrackViewState();
            }
            if (this.todayDayStyle != null)
            {
                ((IStateManager) this.todayDayStyle).TrackViewState();
            }
            if (this.weekendDayStyle != null)
            {
                ((IStateManager) this.weekendDayStyle).TrackViewState();
            }
            if (this.otherMonthDayStyle != null)
            {
                ((IStateManager) this.otherMonthDayStyle).TrackViewState();
            }
            if (this.selectedDayStyle != null)
            {
                ((IStateManager) this.selectedDayStyle).TrackViewState();
            }
            if (this.selectorStyle != null)
            {
                ((IStateManager) this.selectorStyle).TrackViewState();
            }
        }

        [WebCategory("Accessibility"), WebSysDescription("Calendar_Caption"), DefaultValue(""), Localizable(true)]
        public virtual string Caption
        {
            get
            {
                string str = (string) this.ViewState["Caption"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["Caption"] = value;
            }
        }

        [WebCategory("Accessibility"), WebSysDescription("WebControl_CaptionAlign"), DefaultValue(0)]
        public virtual TableCaptionAlign CaptionAlign
        {
            get
            {
                object obj2 = this.ViewState["CaptionAlign"];
                if (obj2 == null)
                {
                    return TableCaptionAlign.NotSet;
                }
                return (TableCaptionAlign) obj2;
            }
            set
            {
                if ((value < TableCaptionAlign.NotSet) || (value > TableCaptionAlign.Right))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["CaptionAlign"] = value;
            }
        }

        [WebCategory("Layout"), DefaultValue(2), WebSysDescription("Calendar_CellPadding")]
        public int CellPadding
        {
            get
            {
                object obj2 = this.ViewState["CellPadding"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 2;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["CellPadding"] = value;
            }
        }

        [DefaultValue(0), WebCategory("Layout"), WebSysDescription("Calendar_CellSpacing")]
        public int CellSpacing
        {
            get
            {
                object obj2 = this.ViewState["CellSpacing"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 0;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["CellSpacing"] = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebSysDescription("Calendar_DayHeaderStyle"), WebCategory("Styles"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle DayHeaderStyle
        {
            get
            {
                if (this.dayHeaderStyle == null)
                {
                    this.dayHeaderStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.dayHeaderStyle).TrackViewState();
                    }
                }
                return this.dayHeaderStyle;
            }
        }

        [WebCategory("Appearance"), DefaultValue(1), WebSysDescription("Calendar_DayNameFormat")]
        public System.Web.UI.WebControls.DayNameFormat DayNameFormat
        {
            get
            {
                object obj2 = this.ViewState["DayNameFormat"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.DayNameFormat) obj2;
                }
                return System.Web.UI.WebControls.DayNameFormat.Short;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.DayNameFormat.Full) || (value > System.Web.UI.WebControls.DayNameFormat.Shortest))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["DayNameFormat"] = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), WebSysDescription("Calendar_DayStyle"), WebCategory("Styles"), NotifyParentProperty(true)]
        public TableItemStyle DayStyle
        {
            get
            {
                if (this.dayStyle == null)
                {
                    this.dayStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.dayStyle).TrackViewState();
                    }
                }
                return this.dayStyle;
            }
        }

        [WebCategory("Appearance"), DefaultValue(7), WebSysDescription("Calendar_FirstDayOfWeek")]
        public System.Web.UI.WebControls.FirstDayOfWeek FirstDayOfWeek
        {
            get
            {
                object obj2 = this.ViewState["FirstDayOfWeek"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.FirstDayOfWeek) obj2;
                }
                return System.Web.UI.WebControls.FirstDayOfWeek.Default;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.FirstDayOfWeek.Sunday) || (value > System.Web.UI.WebControls.FirstDayOfWeek.Default))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["FirstDayOfWeek"] = value;
            }
        }

        [WebCategory("Appearance"), DefaultValue("&gt;"), WebSysDescription("Calendar_NextMonthText"), Localizable(true)]
        public string NextMonthText
        {
            get
            {
                object obj2 = this.ViewState["NextMonthText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return "&gt;";
            }
            set
            {
                this.ViewState["NextMonthText"] = value;
            }
        }

        [WebSysDescription("Calendar_NextPrevFormat"), WebCategory("Appearance"), DefaultValue(0)]
        public System.Web.UI.WebControls.NextPrevFormat NextPrevFormat
        {
            get
            {
                object obj2 = this.ViewState["NextPrevFormat"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.NextPrevFormat) obj2;
                }
                return System.Web.UI.WebControls.NextPrevFormat.CustomText;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.NextPrevFormat.CustomText) || (value > System.Web.UI.WebControls.NextPrevFormat.FullMonth))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["NextPrevFormat"] = value;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebCategory("Styles"), WebSysDescription("Calendar_NextPrevStyle")]
        public TableItemStyle NextPrevStyle
        {
            get
            {
                if (this.nextPrevStyle == null)
                {
                    this.nextPrevStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.nextPrevStyle).TrackViewState();
                    }
                }
                return this.nextPrevStyle;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Styles"), DefaultValue((string) null), WebSysDescription("Calendar_OtherMonthDayStyle")]
        public TableItemStyle OtherMonthDayStyle
        {
            get
            {
                if (this.otherMonthDayStyle == null)
                {
                    this.otherMonthDayStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.otherMonthDayStyle).TrackViewState();
                    }
                }
                return this.otherMonthDayStyle;
            }
        }

        [WebCategory("Appearance"), DefaultValue("&lt;"), WebSysDescription("Calendar_PrevMonthText"), Localizable(true)]
        public string PrevMonthText
        {
            get
            {
                object obj2 = this.ViewState["PrevMonthText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return "&lt;";
            }
            set
            {
                this.ViewState["PrevMonthText"] = value;
            }
        }

        [WebSysDescription("Calendar_SelectedDate"), Bindable(true, BindingDirection.TwoWay), DefaultValue(typeof(DateTime), "1/1/0001")]
        public DateTime SelectedDate
        {
            get
            {
                if (this.SelectedDates.Count == 0)
                {
                    return DateTime.MinValue;
                }
                return this.SelectedDates[0];
            }
            set
            {
                if (value == DateTime.MinValue)
                {
                    this.SelectedDates.Clear();
                }
                else
                {
                    this.SelectedDates.SelectRange(value, value);
                }
            }
        }

        [WebSysDescription("Calendar_SelectedDates"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SelectedDatesCollection SelectedDates
        {
            get
            {
                if (this.selectedDates == null)
                {
                    if (this.dateList == null)
                    {
                        this.dateList = new ArrayList();
                    }
                    this.selectedDates = new SelectedDatesCollection(this.dateList);
                }
                return this.selectedDates;
            }
        }

        [WebCategory("Styles"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), WebSysDescription("Calendar_SelectedDayStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
        public TableItemStyle SelectedDayStyle
        {
            get
            {
                if (this.selectedDayStyle == null)
                {
                    this.selectedDayStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.selectedDayStyle).TrackViewState();
                    }
                }
                return this.selectedDayStyle;
            }
        }

        [WebSysDescription("Calendar_SelectionMode"), WebCategory("Behavior"), DefaultValue(1)]
        public CalendarSelectionMode SelectionMode
        {
            get
            {
                object obj2 = this.ViewState["SelectionMode"];
                if (obj2 != null)
                {
                    return (CalendarSelectionMode) obj2;
                }
                return CalendarSelectionMode.Day;
            }
            set
            {
                if ((value < CalendarSelectionMode.None) || (value > CalendarSelectionMode.DayWeekMonth))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["SelectionMode"] = value;
            }
        }

        [Localizable(true), WebCategory("Appearance"), DefaultValue("&gt;&gt;"), WebSysDescription("Calendar_SelectMonthText")]
        public string SelectMonthText
        {
            get
            {
                object obj2 = this.ViewState["SelectMonthText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return "&gt;&gt;";
            }
            set
            {
                this.ViewState["SelectMonthText"] = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebCategory("Styles"), WebSysDescription("Calendar_SelectorStyle"), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle SelectorStyle
        {
            get
            {
                if (this.selectorStyle == null)
                {
                    this.selectorStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.selectorStyle).TrackViewState();
                    }
                }
                return this.selectorStyle;
            }
        }

        [WebCategory("Appearance"), Localizable(true), DefaultValue("&gt;"), WebSysDescription("Calendar_SelectWeekText")]
        public string SelectWeekText
        {
            get
            {
                object obj2 = this.ViewState["SelectWeekText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return "&gt;";
            }
            set
            {
                this.ViewState["SelectWeekText"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("Calendar_ShowDayHeader"), DefaultValue(true)]
        public bool ShowDayHeader
        {
            get
            {
                object obj2 = this.ViewState["ShowDayHeader"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["ShowDayHeader"] = value;
            }
        }

        [DefaultValue(false), WebSysDescription("Calendar_ShowGridLines"), WebCategory("Appearance")]
        public bool ShowGridLines
        {
            get
            {
                object obj2 = this.ViewState["ShowGridLines"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["ShowGridLines"] = value;
            }
        }

        [WebSysDescription("Calendar_ShowNextPrevMonth"), DefaultValue(true), WebCategory("Appearance")]
        public bool ShowNextPrevMonth
        {
            get
            {
                object obj2 = this.ViewState["ShowNextPrevMonth"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["ShowNextPrevMonth"] = value;
            }
        }

        [WebSysDescription("Calendar_ShowTitle"), WebCategory("Appearance"), DefaultValue(true)]
        public bool ShowTitle
        {
            get
            {
                object obj2 = this.ViewState["ShowTitle"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["ShowTitle"] = value;
            }
        }

        public override bool SupportsDisabledAttribute
        {
            get
            {
                return (this.RenderingCompatibility < VersionUtil.Framework40);
            }
        }

        [DefaultValue(1), WebCategory("Appearance"), WebSysDescription("Calendar_TitleFormat")]
        public System.Web.UI.WebControls.TitleFormat TitleFormat
        {
            get
            {
                object obj2 = this.ViewState["TitleFormat"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.TitleFormat) obj2;
                }
                return System.Web.UI.WebControls.TitleFormat.MonthYear;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.TitleFormat.Month) || (value > System.Web.UI.WebControls.TitleFormat.MonthYear))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["TitleFormat"] = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Styles"), NotifyParentProperty(true), WebSysDescription("Calendar_TitleStyle"), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle TitleStyle
        {
            get
            {
                if (this.titleStyle == null)
                {
                    this.titleStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.titleStyle).TrackViewState();
                    }
                }
                return this.titleStyle;
            }
        }

        [WebSysDescription("Calendar_TodayDayStyle"), PersistenceMode(PersistenceMode.InnerProperty), NotifyParentProperty(true), DefaultValue((string) null), WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TableItemStyle TodayDayStyle
        {
            get
            {
                if (this.todayDayStyle == null)
                {
                    this.todayDayStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.todayDayStyle).TrackViewState();
                    }
                }
                return this.todayDayStyle;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebSysDescription("Calendar_TodaysDate")]
        public DateTime TodaysDate
        {
            get
            {
                object obj2 = this.ViewState["TodaysDate"];
                if (obj2 != null)
                {
                    return (DateTime) obj2;
                }
                return DateTime.Today;
            }
            set
            {
                this.ViewState["TodaysDate"] = value.Date;
            }
        }

        [DefaultValue(true), WebCategory("Accessibility"), WebSysDescription("Table_UseAccessibleHeader")]
        public virtual bool UseAccessibleHeader
        {
            get
            {
                object obj2 = this.ViewState["UseAccessibleHeader"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                this.ViewState["UseAccessibleHeader"] = value;
            }
        }

        [WebSysDescription("Calendar_VisibleDate"), DefaultValue(typeof(DateTime), "1/1/0001"), Bindable(true)]
        public DateTime VisibleDate
        {
            get
            {
                object obj2 = this.ViewState["VisibleDate"];
                if (obj2 != null)
                {
                    return (DateTime) obj2;
                }
                return DateTime.MinValue;
            }
            set
            {
                this.ViewState["VisibleDate"] = value.Date;
            }
        }

        [WebCategory("Styles"), NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("Calendar_WeekendDayStyle")]
        public TableItemStyle WeekendDayStyle
        {
            get
            {
                if (this.weekendDayStyle == null)
                {
                    this.weekendDayStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.weekendDayStyle).TrackViewState();
                    }
                }
                return this.weekendDayStyle;
            }
        }
    }
}

