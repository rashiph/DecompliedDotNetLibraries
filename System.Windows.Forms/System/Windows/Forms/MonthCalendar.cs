namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.Internal;
    using System.Windows.Forms.Layout;

    [DefaultProperty("SelectionRange"), System.Windows.Forms.SRDescription("DescriptionMonthCalendar"), Designer("System.Windows.Forms.Design.MonthCalendarDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultBindingProperty("SelectionRange"), DefaultEvent("DateChanged"), ComVisible(true)]
    public class MonthCalendar : Control
    {
        private const int ANNUAL_DATE = 1;
        private ArrayList annualArrayOfDates = new ArrayList();
        private ArrayList arrayOfDates = new ArrayList();
        private int datesToBoldMonthly;
        private const long DAYS_TO_10000 = 0x372c9cL;
        private const long DAYS_TO_1601 = 0x85d85L;
        private const Day DEFAULT_FIRST_DAY_OF_WEEK = Day.Default;
        private const int DEFAULT_MAX_SELECTION_COUNT = 7;
        private const int DEFAULT_SCROLL_CHANGE = 0;
        private static readonly System.Drawing.Color DEFAULT_TITLE_BACK_COLOR = SystemColors.ActiveCaption;
        private static readonly System.Drawing.Color DEFAULT_TITLE_FORE_COLOR = SystemColors.ActiveCaptionText;
        private static readonly System.Drawing.Color DEFAULT_TRAILING_FORE_COLOR = SystemColors.GrayText;
        private static readonly System.Drawing.Size DefaultSingleMonthSize = new System.Drawing.Size(0xb0, 0x99);
        private System.Drawing.Size dimensions = new System.Drawing.Size(1, 1);
        private const int ExtraPadding = 2;
        private Day firstDayOfWeek = Day.Default;
        private const int INSERT_HEIGHT_SIZE = 6;
        private const int INSERT_WIDTH_SIZE = 6;
        private DateTime maxDate = DateTime.MaxValue;
        private const int MaxScrollChange = 0x4e20;
        private int maxSelectionCount = 7;
        private IntPtr mdsBuffer = IntPtr.Zero;
        private int mdsBufferSize;
        private DateTime minDate = DateTime.MinValue;
        private const int MINIMUM_ALLOC_SIZE = 12;
        private const int MONTHLY_DATE = 2;
        private ArrayList monthlyArrayOfDates = new ArrayList();
        private const int MONTHS_IN_YEAR = 12;
        private int[] monthsOfYear = new int[12];
        private bool rightToLeftLayout;
        private int scrollChange;
        private DateTime selectionEnd;
        private DateTime selectionStart;
        private bool showToday = true;
        private bool showTodayCircle = true;
        private bool showWeekNumbers;
        private System.Drawing.Color titleBackColor = DEFAULT_TITLE_BACK_COLOR;
        private System.Drawing.Color titleForeColor = DEFAULT_TITLE_FORE_COLOR;
        private DateTime todayDate = DateTime.Now.Date;
        private bool todayDateSet;
        private System.Drawing.Color trailingForeColor = DEFAULT_TRAILING_FORE_COLOR;
        private const int UNIQUE_DATE = 0;

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler BackgroundImageChanged
        {
            add
            {
                base.BackgroundImageChanged += value;
            }
            remove
            {
                base.BackgroundImageChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler BackgroundImageLayoutChanged
        {
            add
            {
                base.BackgroundImageLayoutChanged += value;
            }
            remove
            {
                base.BackgroundImageLayoutChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler Click
        {
            add
            {
                base.Click += value;
            }
            remove
            {
                base.Click -= value;
            }
        }

        [System.Windows.Forms.SRDescription("MonthCalendarOnDateChangedDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event DateRangeEventHandler DateChanged;

        [System.Windows.Forms.SRDescription("MonthCalendarOnDateSelectedDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event DateRangeEventHandler DateSelected;

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler DoubleClick
        {
            add
            {
                base.DoubleClick += value;
            }
            remove
            {
                base.DoubleClick -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler ImeModeChanged
        {
            add
            {
                base.ImeModeChanged += value;
            }
            remove
            {
                base.ImeModeChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event MouseEventHandler MouseClick
        {
            add
            {
                base.MouseClick += value;
            }
            remove
            {
                base.MouseClick -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event MouseEventHandler MouseDoubleClick
        {
            add
            {
                base.MouseDoubleClick += value;
            }
            remove
            {
                base.MouseDoubleClick -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler PaddingChanged
        {
            add
            {
                base.PaddingChanged += value;
            }
            remove
            {
                base.PaddingChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event PaintEventHandler Paint
        {
            add
            {
                base.Paint += value;
            }
            remove
            {
                base.Paint -= value;
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnRightToLeftLayoutChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler RightToLeftLayoutChanged;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler TextChanged
        {
            add
            {
                base.TextChanged += value;
            }
            remove
            {
                base.TextChanged -= value;
            }
        }

        public MonthCalendar()
        {
            this.selectionStart = this.todayDate;
            this.selectionEnd = this.todayDate;
            base.SetStyle(ControlStyles.UserPaint, false);
            base.SetStyle(ControlStyles.StandardClick, false);
            base.TabStop = true;
        }

        public void AddAnnuallyBoldedDate(DateTime date)
        {
            this.annualArrayOfDates.Add(date);
            this.monthsOfYear[date.Month - 1] |= ((int) 1) << (date.Day - 1);
        }

        public void AddBoldedDate(DateTime date)
        {
            if (!this.arrayOfDates.Contains(date))
            {
                this.arrayOfDates.Add(date);
            }
        }

        public void AddMonthlyBoldedDate(DateTime date)
        {
            this.monthlyArrayOfDates.Add(date);
            this.datesToBoldMonthly |= ((int) 1) << (date.Day - 1);
        }

        private void AdjustSize()
        {
            System.Drawing.Size minReqRect = this.GetMinReqRect();
            this.Size = minReqRect;
        }

        private void BoldDates(DateBoldEventArgs e)
        {
            int size = e.Size;
            e.DaysToBold = new int[size];
            System.Windows.Forms.SelectionRange displayRange = this.GetDisplayRange(false);
            int month = displayRange.Start.Month;
            int year = displayRange.Start.Year;
            int count = this.arrayOfDates.Count;
            for (int i = 0; i < count; i++)
            {
                DateTime time = (DateTime) this.arrayOfDates[i];
                if ((DateTime.Compare(time, displayRange.Start) >= 0) && (DateTime.Compare(time, displayRange.End) <= 0))
                {
                    int num6 = time.Month;
                    int num7 = time.Year;
                    int num8 = (num7 == year) ? (num6 - month) : (((num6 + (num7 * 12)) - (year * 12)) - month);
                    e.DaysToBold[num8] |= ((int) 1) << (time.Day - 1);
                }
            }
            month--;
            int index = 0;
            while (index < size)
            {
                e.DaysToBold[index] |= this.monthsOfYear[month % 12] | this.datesToBoldMonthly;
                index++;
                month++;
            }
        }

        private bool CompareDayAndMonth(DateTime t1, DateTime t2)
        {
            return ((t1.Day == t2.Day) && (t1.Month == t2.Month));
        }

        protected override void CreateHandle()
        {
            if (!base.RecreatingHandle)
            {
                IntPtr userCookie = System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Activate();
                try
                {
                    System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX icc = new System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX {
                        dwICC = 0x100
                    };
                    System.Windows.Forms.SafeNativeMethods.InitCommonControlsEx(icc);
                }
                finally
                {
                    System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Deactivate(userCookie);
                }
            }
            base.CreateHandle();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.mdsBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.mdsBuffer);
                this.mdsBuffer = IntPtr.Zero;
            }
            base.Dispose(disposing);
        }

        private static string FormatDate(DateTime value)
        {
            return value.ToString("d", CultureInfo.CurrentCulture);
        }

        public System.Windows.Forms.SelectionRange GetDisplayRange(bool visible)
        {
            if (visible)
            {
                return this.GetMonthRange(0);
            }
            return this.GetMonthRange(1);
        }

        private HitArea GetHitArea(int hit)
        {
            switch (hit)
            {
                case 0x10000:
                    return HitArea.TitleBackground;

                case 0x10001:
                    return HitArea.TitleMonth;

                case 0x10002:
                    return HitArea.TitleYear;

                case 0x20000:
                    return HitArea.CalendarBackground;

                case 0x20001:
                    return HitArea.Date;

                case 0x20002:
                    return HitArea.DayOfWeek;

                case 0x20003:
                    return HitArea.WeekNumbers;

                case 0x30000:
                    return HitArea.TodayLink;

                case 0x1010003:
                    return HitArea.NextMonthButton;

                case 0x1020001:
                    return HitArea.NextMonthDate;

                case 0x2010003:
                    return HitArea.PrevMonthButton;

                case 0x2020001:
                    return HitArea.PrevMonthDate;
            }
            return HitArea.Nowhere;
        }

        private System.Drawing.Size GetMinReqRect()
        {
            return this.GetMinReqRect(0, false, false);
        }

        private System.Drawing.Size GetMinReqRect(int newDimensionLength, bool updateRows, bool updateCols)
        {
            System.Drawing.Size textExtent;
            System.Drawing.Size singleMonthSize = this.SingleMonthSize;
            using (WindowsFont font = WindowsFont.FromFont(this.Font))
            {
                textExtent = WindowsGraphicsCacheManager.MeasurementGraphics.GetTextExtent(DateTime.Now.ToShortDateString(), font);
            }
            int num = textExtent.Height + 4;
            int height = singleMonthSize.Height;
            if (this.ShowToday)
            {
                height -= num;
            }
            if (updateRows)
            {
                int num3 = ((newDimensionLength - num) + 6) / (height + 6);
                this.dimensions.Height = (num3 < 1) ? 1 : num3;
            }
            if (updateCols)
            {
                int num4 = (newDimensionLength - 2) / singleMonthSize.Width;
                this.dimensions.Width = (num4 < 1) ? 1 : num4;
            }
            singleMonthSize.Width = ((singleMonthSize.Width + 6) * this.dimensions.Width) - 6;
            singleMonthSize.Height = (((height + 6) * this.dimensions.Height) - 6) + num;
            if (base.IsHandleCreated)
            {
                int num5 = (int) ((long) base.SendMessage(0x1015, 0, 0));
                if (num5 > singleMonthSize.Width)
                {
                    singleMonthSize.Width = num5;
                }
            }
            singleMonthSize.Width += 2;
            singleMonthSize.Height += 2;
            return singleMonthSize;
        }

        private System.Windows.Forms.SelectionRange GetMonthRange(int flag)
        {
            System.Windows.Forms.NativeMethods.SYSTEMTIMEARRAY lParam = new System.Windows.Forms.NativeMethods.SYSTEMTIMEARRAY();
            System.Windows.Forms.SelectionRange range = new System.Windows.Forms.SelectionRange();
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1007, flag, lParam);
            System.Windows.Forms.NativeMethods.SYSTEMTIME s = new System.Windows.Forms.NativeMethods.SYSTEMTIME {
                wYear = lParam.wYear1,
                wMonth = lParam.wMonth1,
                wDayOfWeek = lParam.wDayOfWeek1,
                wDay = lParam.wDay1
            };
            range.Start = DateTimePicker.SysTimeToDateTime(s);
            s.wYear = lParam.wYear2;
            s.wMonth = lParam.wMonth2;
            s.wDayOfWeek = lParam.wDayOfWeek2;
            s.wDay = lParam.wDay2;
            range.End = DateTimePicker.SysTimeToDateTime(s);
            return range;
        }

        private int GetPreferredHeight(int height, bool updateRows)
        {
            return this.GetMinReqRect(height, updateRows, false).Height;
        }

        private int GetPreferredWidth(int width, bool updateCols)
        {
            return this.GetMinReqRect(width, false, updateCols).Width;
        }

        public HitTestInfo HitTest(Point point)
        {
            return this.HitTest(point.X, point.Y);
        }

        public HitTestInfo HitTest(int x, int y)
        {
            System.Windows.Forms.NativeMethods.MCHITTESTINFO lParam = new System.Windows.Forms.NativeMethods.MCHITTESTINFO {
                pt_x = x,
                pt_y = y,
                cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.MCHITTESTINFO))
            };
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x100e, 0, lParam);
            HitArea hitArea = this.GetHitArea(lParam.uHit);
            if (HitTestInfo.HitAreaHasValidDateTime(hitArea))
            {
                System.Windows.Forms.NativeMethods.SYSTEMTIME s = new System.Windows.Forms.NativeMethods.SYSTEMTIME {
                    wYear = lParam.st_wYear,
                    wMonth = lParam.st_wMonth,
                    wDayOfWeek = lParam.st_wDayOfWeek,
                    wDay = lParam.st_wDay,
                    wHour = lParam.st_wHour,
                    wMinute = lParam.st_wMinute,
                    wSecond = lParam.st_wSecond,
                    wMilliseconds = lParam.st_wMilliseconds
                };
                return new HitTestInfo(new Point(lParam.pt_x, lParam.pt_y), hitArea, DateTimePicker.SysTimeToDateTime(s));
            }
            return new HitTestInfo(new Point(lParam.pt_x, lParam.pt_y), hitArea);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if ((keyData & Keys.Alt) == Keys.Alt)
            {
                return false;
            }
            switch ((keyData & Keys.KeyCode))
            {
                case Keys.PageUp:
                case Keys.Next:
                case Keys.End:
                case Keys.Home:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        private void MarshaledUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs pref)
        {
            try
            {
                base.BeginInvoke(new UserPreferenceChangedEventHandler(this.UserPreferenceChanged), new object[] { sender, pref });
            }
            catch (InvalidOperationException)
            {
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            this.SetControlColor(4, this.BackColor);
        }

        protected virtual void OnDateChanged(DateRangeEventArgs drevent)
        {
            if (this.onDateChanged != null)
            {
                this.onDateChanged(this, drevent);
            }
        }

        protected virtual void OnDateSelected(DateRangeEventArgs drevent)
        {
            if (this.onDateSelected != null)
            {
                this.onDateSelected(this, drevent);
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.AdjustSize();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            this.SetControlColor(1, this.ForeColor);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            int firstDayOfWeek;
            base.OnHandleCreated(e);
            this.SetSelRange(this.selectionStart, this.selectionEnd);
            if (this.maxSelectionCount != 7)
            {
                base.SendMessage(0x1004, this.maxSelectionCount, 0);
            }
            this.AdjustSize();
            if (this.todayDateSet)
            {
                System.Windows.Forms.NativeMethods.SYSTEMTIME lParam = DateTimePicker.DateTimeToSysTime(this.todayDate);
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x100c, 0, lParam);
            }
            this.SetControlColor(1, this.ForeColor);
            this.SetControlColor(4, this.BackColor);
            this.SetControlColor(2, this.titleBackColor);
            this.SetControlColor(3, this.titleForeColor);
            this.SetControlColor(5, this.trailingForeColor);
            if (this.firstDayOfWeek == Day.Default)
            {
                firstDayOfWeek = 0x100c;
            }
            else
            {
                firstDayOfWeek = (int) this.firstDayOfWeek;
            }
            base.SendMessage(0x100f, 0, firstDayOfWeek);
            this.SetRange();
            if (this.scrollChange != 0)
            {
                base.SendMessage(0x1014, this.scrollChange, 0);
            }
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.MarshaledUserPreferenceChanged);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.MarshaledUserPreferenceChanged);
            base.OnHandleDestroyed(e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnRightToLeftLayoutChanged(EventArgs e)
        {
            if (!base.GetAnyDisposingInHierarchy())
            {
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    base.RecreateHandle();
                }
                if (this.onRightToLeftLayoutChanged != null)
                {
                    this.onRightToLeftLayoutChanged(this, e);
                }
            }
        }

        public void RemoveAllAnnuallyBoldedDates()
        {
            this.annualArrayOfDates.Clear();
            for (int i = 0; i < 12; i++)
            {
                this.monthsOfYear[i] = 0;
            }
        }

        public void RemoveAllBoldedDates()
        {
            this.arrayOfDates.Clear();
        }

        public void RemoveAllMonthlyBoldedDates()
        {
            this.monthlyArrayOfDates.Clear();
            this.datesToBoldMonthly = 0;
        }

        public void RemoveAnnuallyBoldedDate(DateTime date)
        {
            int count = this.annualArrayOfDates.Count;
            int index = 0;
            while (index < count)
            {
                if (this.CompareDayAndMonth((DateTime) this.annualArrayOfDates[index], date))
                {
                    this.annualArrayOfDates.RemoveAt(index);
                    break;
                }
                index++;
            }
            count--;
            for (int i = index; i < count; i++)
            {
                if (this.CompareDayAndMonth((DateTime) this.annualArrayOfDates[i], date))
                {
                    return;
                }
            }
            this.monthsOfYear[date.Month - 1] &= ~(((int) 1) << (date.Day - 1));
        }

        public void RemoveBoldedDate(DateTime date)
        {
            int count = this.arrayOfDates.Count;
            for (int i = 0; i < count; i++)
            {
                DateTime time = (DateTime) this.arrayOfDates[i];
                if (DateTime.Compare(time.Date, date.Date) == 0)
                {
                    this.arrayOfDates.RemoveAt(i);
                    base.Invalidate();
                    return;
                }
            }
        }

        public void RemoveMonthlyBoldedDate(DateTime date)
        {
            int count = this.monthlyArrayOfDates.Count;
            int index = 0;
            while (index < count)
            {
                if (this.CompareDayAndMonth((DateTime) this.monthlyArrayOfDates[index], date))
                {
                    this.monthlyArrayOfDates.RemoveAt(index);
                    break;
                }
                index++;
            }
            count--;
            for (int i = index; i < count; i++)
            {
                if (this.CompareDayAndMonth((DateTime) this.monthlyArrayOfDates[i], date))
                {
                    return;
                }
            }
            this.datesToBoldMonthly &= ~(((int) 1) << (date.Day - 1));
        }

        private IntPtr RequestBuffer(int reqSize)
        {
            int num = 4;
            if ((reqSize * num) > this.mdsBufferSize)
            {
                if (this.mdsBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(this.mdsBuffer);
                    this.mdsBuffer = IntPtr.Zero;
                }
                float num2 = ((float) (reqSize - 1)) / 12f;
                int num3 = ((int) (num2 + 1f)) * 12;
                this.mdsBufferSize = num3 * num;
                this.mdsBuffer = Marshal.AllocHGlobal(this.mdsBufferSize);
            }
            return this.mdsBuffer;
        }

        private void ResetAnnuallyBoldedDates()
        {
            this.annualArrayOfDates.Clear();
        }

        private void ResetBoldedDates()
        {
            this.arrayOfDates.Clear();
        }

        private void ResetCalendarDimensions()
        {
            this.CalendarDimensions = new System.Drawing.Size(1, 1);
        }

        private void ResetMaxDate()
        {
            this.MaxDate = DateTime.MaxValue;
        }

        private void ResetMinDate()
        {
            this.MinDate = DateTime.MinValue;
        }

        private void ResetMonthlyBoldedDates()
        {
            this.monthlyArrayOfDates.Clear();
        }

        private void ResetSelectionRange()
        {
            this.SetSelectionRange(this.Now, this.Now);
        }

        private void ResetTitleBackColor()
        {
            this.TitleBackColor = DEFAULT_TITLE_BACK_COLOR;
        }

        private void ResetTitleForeColor()
        {
            this.TitleForeColor = DEFAULT_TITLE_FORE_COLOR;
        }

        private void ResetTodayDate()
        {
            this.todayDateSet = false;
            this.UpdateTodayDate();
        }

        private void ResetTrailingForeColor()
        {
            this.TrailingForeColor = DEFAULT_TRAILING_FORE_COLOR;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            Rectangle bounds = base.Bounds;
            System.Drawing.Size maxWindowTrackSize = SystemInformation.MaxWindowTrackSize;
            if (width != bounds.Width)
            {
                if (width > maxWindowTrackSize.Width)
                {
                    width = maxWindowTrackSize.Width;
                }
                width = this.GetPreferredWidth(width, true);
            }
            if (height != bounds.Height)
            {
                if (height > maxWindowTrackSize.Height)
                {
                    height = maxWindowTrackSize.Height;
                }
                height = this.GetPreferredHeight(height, true);
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        public void SetCalendarDimensions(int x, int y)
        {
            if (x < 1)
            {
                throw new ArgumentOutOfRangeException("x", System.Windows.Forms.SR.GetString("MonthCalendarInvalidDimensions", new object[] { x.ToString("D", CultureInfo.CurrentCulture), y.ToString("D", CultureInfo.CurrentCulture) }));
            }
            if (y < 1)
            {
                throw new ArgumentOutOfRangeException("y", System.Windows.Forms.SR.GetString("MonthCalendarInvalidDimensions", new object[] { x.ToString("D", CultureInfo.CurrentCulture), y.ToString("D", CultureInfo.CurrentCulture) }));
            }
            while ((x * y) > 12)
            {
                if (x > y)
                {
                    x--;
                }
                else
                {
                    y--;
                }
            }
            if ((this.dimensions.Width != x) || (this.dimensions.Height != y))
            {
                this.dimensions.Width = x;
                this.dimensions.Height = y;
                this.AdjustSize();
            }
        }

        private void SetControlColor(int colorIndex, System.Drawing.Color value)
        {
            if (base.IsHandleCreated)
            {
                base.SendMessage(0x100a, colorIndex, ColorTranslator.ToWin32(value));
            }
        }

        public void SetDate(DateTime date)
        {
            if (date.Ticks < this.minDate.Ticks)
            {
                throw new ArgumentOutOfRangeException("date", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", new object[] { "date", FormatDate(date), "MinDate" }));
            }
            if (date.Ticks > this.maxDate.Ticks)
            {
                throw new ArgumentOutOfRangeException("date", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", new object[] { "date", FormatDate(date), "MaxDate" }));
            }
            this.SetSelectionRange(date, date);
        }

        private void SetRange()
        {
            this.SetRange(DateTimePicker.EffectiveMinDate(this.minDate), DateTimePicker.EffectiveMaxDate(this.maxDate));
        }

        private void SetRange(DateTime minDate, DateTime maxDate)
        {
            if (this.selectionStart < minDate)
            {
                this.selectionStart = minDate;
            }
            if (this.selectionStart > maxDate)
            {
                this.selectionStart = maxDate;
            }
            if (this.selectionEnd < minDate)
            {
                this.selectionEnd = minDate;
            }
            if (this.selectionEnd > maxDate)
            {
                this.selectionEnd = maxDate;
            }
            this.SetSelRange(this.selectionStart, this.selectionEnd);
            if (base.IsHandleCreated)
            {
                int wParam = 0;
                System.Windows.Forms.NativeMethods.SYSTEMTIMEARRAY lParam = new System.Windows.Forms.NativeMethods.SYSTEMTIMEARRAY();
                wParam |= 3;
                System.Windows.Forms.NativeMethods.SYSTEMTIME systemtime = DateTimePicker.DateTimeToSysTime(minDate);
                lParam.wYear1 = systemtime.wYear;
                lParam.wMonth1 = systemtime.wMonth;
                lParam.wDayOfWeek1 = systemtime.wDayOfWeek;
                lParam.wDay1 = systemtime.wDay;
                systemtime = DateTimePicker.DateTimeToSysTime(maxDate);
                lParam.wYear2 = systemtime.wYear;
                lParam.wMonth2 = systemtime.wMonth;
                lParam.wDayOfWeek2 = systemtime.wDayOfWeek;
                lParam.wDay2 = systemtime.wDay;
                if (((int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1012, wParam, lParam)) == 0)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("MonthCalendarRange", new object[] { minDate.ToShortDateString(), maxDate.ToShortDateString() }));
                }
            }
        }

        public void SetSelectionRange(DateTime date1, DateTime date2)
        {
            if (date1.Ticks < this.minDate.Ticks)
            {
                throw new ArgumentOutOfRangeException("date1", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", new object[] { "SelectionStart", FormatDate(date1), "MinDate" }));
            }
            if (date1.Ticks > this.maxDate.Ticks)
            {
                throw new ArgumentOutOfRangeException("date1", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", new object[] { "SelectionEnd", FormatDate(date1), "MaxDate" }));
            }
            if (date2.Ticks < this.minDate.Ticks)
            {
                throw new ArgumentOutOfRangeException("date2", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", new object[] { "SelectionStart", FormatDate(date2), "MinDate" }));
            }
            if (date2.Ticks > this.maxDate.Ticks)
            {
                throw new ArgumentOutOfRangeException("date2", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", new object[] { "SelectionEnd", FormatDate(date2), "MaxDate" }));
            }
            if (date1 > date2)
            {
                date2 = date1;
            }
            TimeSpan span = (TimeSpan) (date2 - date1);
            if (span.Days >= this.maxSelectionCount)
            {
                if (date1.Ticks == this.selectionStart.Ticks)
                {
                    date1 = date2.AddDays((double) (1 - this.maxSelectionCount));
                }
                else
                {
                    date2 = date1.AddDays((double) (this.maxSelectionCount - 1));
                }
            }
            this.SetSelRange(date1, date2);
        }

        private void SetSelRange(DateTime lower, DateTime upper)
        {
            bool flag = false;
            if ((this.selectionStart != lower) || (this.selectionEnd != upper))
            {
                flag = true;
                this.selectionStart = lower;
                this.selectionEnd = upper;
            }
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.NativeMethods.SYSTEMTIMEARRAY lParam = new System.Windows.Forms.NativeMethods.SYSTEMTIMEARRAY();
                System.Windows.Forms.NativeMethods.SYSTEMTIME systemtime = DateTimePicker.DateTimeToSysTime(lower);
                lParam.wYear1 = systemtime.wYear;
                lParam.wMonth1 = systemtime.wMonth;
                lParam.wDayOfWeek1 = systemtime.wDayOfWeek;
                lParam.wDay1 = systemtime.wDay;
                systemtime = DateTimePicker.DateTimeToSysTime(upper);
                lParam.wYear2 = systemtime.wYear;
                lParam.wMonth2 = systemtime.wMonth;
                lParam.wDayOfWeek2 = systemtime.wDayOfWeek;
                lParam.wDay2 = systemtime.wDay;
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1006, 0, lParam);
            }
            if (flag)
            {
                this.OnDateChanged(new DateRangeEventArgs(lower, upper));
            }
        }

        private bool ShouldSerializeAnnuallyBoldedDates()
        {
            return (this.annualArrayOfDates.Count > 0);
        }

        private bool ShouldSerializeBoldedDates()
        {
            return (this.arrayOfDates.Count > 0);
        }

        private bool ShouldSerializeCalendarDimensions()
        {
            return !this.dimensions.Equals(new System.Drawing.Size(1, 1));
        }

        private bool ShouldSerializeMaxDate()
        {
            return ((this.maxDate != DateTimePicker.MaximumDateTime) && (this.maxDate != DateTime.MaxValue));
        }

        private bool ShouldSerializeMinDate()
        {
            return ((this.minDate != DateTimePicker.MinimumDateTime) && (this.minDate != DateTime.MinValue));
        }

        private bool ShouldSerializeMonthlyBoldedDates()
        {
            return (this.monthlyArrayOfDates.Count > 0);
        }

        private bool ShouldSerializeSelectionRange()
        {
            return !DateTime.Equals(this.selectionEnd, this.selectionStart);
        }

        private bool ShouldSerializeTitleBackColor()
        {
            return !this.TitleBackColor.Equals(DEFAULT_TITLE_BACK_COLOR);
        }

        private bool ShouldSerializeTitleForeColor()
        {
            return !this.TitleForeColor.Equals(DEFAULT_TITLE_FORE_COLOR);
        }

        private bool ShouldSerializeTodayDate()
        {
            return this.todayDateSet;
        }

        private bool ShouldSerializeTrailingForeColor()
        {
            return !this.TrailingForeColor.Equals(DEFAULT_TRAILING_FORE_COLOR);
        }

        public override string ToString()
        {
            return (base.ToString() + ", " + this.SelectionRange.ToString());
        }

        public void UpdateBoldedDates()
        {
            base.RecreateHandle();
        }

        private void UpdateTodayDate()
        {
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.NativeMethods.SYSTEMTIME lParam = null;
                if (this.todayDateSet)
                {
                    lParam = DateTimePicker.DateTimeToSysTime(this.todayDate);
                }
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x100c, 0, lParam);
            }
        }

        private void UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs pref)
        {
            if (pref.Category == UserPreferenceCategory.Locale)
            {
                base.RecreateHandle();
            }
        }

        private void WmDateBold(ref Message m)
        {
            System.Windows.Forms.NativeMethods.NMDAYSTATE lParam = (System.Windows.Forms.NativeMethods.NMDAYSTATE) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMDAYSTATE));
            DateBoldEventArgs e = new DateBoldEventArgs(DateTimePicker.SysTimeToDateTime(lParam.stStart), lParam.cDayState);
            this.BoldDates(e);
            this.mdsBuffer = this.RequestBuffer(e.Size);
            Marshal.Copy(e.DaysToBold, 0, this.mdsBuffer, e.Size);
            lParam.prgDayState = this.mdsBuffer;
            Marshal.StructureToPtr(lParam, m.LParam, false);
        }

        private void WmDateChanged(ref Message m)
        {
            System.Windows.Forms.NativeMethods.NMSELCHANGE lParam = (System.Windows.Forms.NativeMethods.NMSELCHANGE) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMSELCHANGE));
            DateTime start = this.selectionStart = DateTimePicker.SysTimeToDateTime(lParam.stSelStart);
            DateTime end = this.selectionEnd = DateTimePicker.SysTimeToDateTime(lParam.stSelEnd);
            if ((start.Ticks < this.minDate.Ticks) || (end.Ticks < this.minDate.Ticks))
            {
                this.SetSelRange(this.minDate, this.minDate);
            }
            else if ((start.Ticks > this.maxDate.Ticks) || (end.Ticks > this.maxDate.Ticks))
            {
                this.SetSelRange(this.maxDate, this.maxDate);
            }
            this.OnDateChanged(new DateRangeEventArgs(start, end));
        }

        private void WmDateSelected(ref Message m)
        {
            System.Windows.Forms.NativeMethods.NMSELCHANGE lParam = (System.Windows.Forms.NativeMethods.NMSELCHANGE) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMSELCHANGE));
            DateTime start = this.selectionStart = DateTimePicker.SysTimeToDateTime(lParam.stSelStart);
            DateTime end = this.selectionEnd = DateTimePicker.SysTimeToDateTime(lParam.stSelEnd);
            if ((start.Ticks < this.minDate.Ticks) || (end.Ticks < this.minDate.Ticks))
            {
                this.SetSelRange(this.minDate, this.minDate);
            }
            else if ((start.Ticks > this.maxDate.Ticks) || (end.Ticks > this.maxDate.Ticks))
            {
                this.SetSelRange(this.maxDate, this.maxDate);
            }
            this.OnDateSelected(new DateRangeEventArgs(start, end));
        }

        private void WmGetDlgCode(ref Message m)
        {
            m.Result = (IntPtr) 1;
        }

        private void WmReflectCommand(ref Message m)
        {
            if (m.HWnd == base.Handle)
            {
                System.Windows.Forms.NativeMethods.NMHDR lParam = (System.Windows.Forms.NativeMethods.NMHDR) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHDR));
                switch (lParam.code)
                {
                    case -749:
                        this.WmDateChanged(ref m);
                        return;

                    case -748:
                        return;

                    case -747:
                        this.WmDateBold(ref m);
                        return;

                    case -746:
                        this.WmDateSelected(ref m);
                        return;
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x87:
                    this.WmGetDlgCode(ref m);
                    return;

                case 0x201:
                    this.FocusInternal();
                    if (base.ValidationCancelled)
                    {
                        break;
                    }
                    base.WndProc(ref m);
                    return;

                case 0x204e:
                    this.WmReflectCommand(ref m);
                    base.WndProc(ref m);
                    return;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        [System.Windows.Forms.SRDescription("MonthCalendarAnnuallyBoldedDatesDescr"), Localizable(true)]
        public DateTime[] AnnuallyBoldedDates
        {
            get
            {
                DateTime[] timeArray = new DateTime[this.annualArrayOfDates.Count];
                for (int i = 0; i < this.annualArrayOfDates.Count; i++)
                {
                    timeArray[i] = (DateTime) this.annualArrayOfDates[i];
                }
                return timeArray;
            }
            set
            {
                this.annualArrayOfDates.Clear();
                for (int i = 0; i < 12; i++)
                {
                    this.monthsOfYear[i] = 0;
                }
                if ((value != null) && (value.Length > 0))
                {
                    for (int j = 0; j < value.Length; j++)
                    {
                        this.annualArrayOfDates.Add(value[j]);
                    }
                    for (int k = 0; k < value.Length; k++)
                    {
                        this.monthsOfYear[value[k].Month - 1] |= ((int) 1) << (value[k].Day - 1);
                    }
                }
                base.RecreateHandle();
            }
        }

        [System.Windows.Forms.SRDescription("MonthCalendarMonthBackColorDescr")]
        public override System.Drawing.Color BackColor
        {
            get
            {
                if (this.ShouldSerializeBackColor())
                {
                    return base.BackColor;
                }
                return SystemColors.Window;
            }
            set
            {
                base.BackColor = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                base.BackgroundImage = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override ImageLayout BackgroundImageLayout
        {
            get
            {
                return base.BackgroundImageLayout;
            }
            set
            {
                base.BackgroundImageLayout = value;
            }
        }

        [Localizable(true)]
        public DateTime[] BoldedDates
        {
            get
            {
                DateTime[] timeArray = new DateTime[this.arrayOfDates.Count];
                for (int i = 0; i < this.arrayOfDates.Count; i++)
                {
                    timeArray[i] = (DateTime) this.arrayOfDates[i];
                }
                return timeArray;
            }
            set
            {
                this.arrayOfDates.Clear();
                if ((value != null) && (value.Length > 0))
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        this.arrayOfDates.Add(value[i]);
                    }
                }
                base.RecreateHandle();
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("MonthCalendarDimensionsDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public System.Drawing.Size CalendarDimensions
        {
            get
            {
                return this.dimensions;
            }
            set
            {
                if (!this.dimensions.Equals(value))
                {
                    this.SetCalendarDimensions(value.Width, value.Height);
                }
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ClassName = "SysMonthCal32";
                createParams.Style |= 3;
                if (!this.showToday)
                {
                    createParams.Style |= 0x10;
                }
                if (!this.showTodayCircle)
                {
                    createParams.Style |= 8;
                }
                if (this.showWeekNumbers)
                {
                    createParams.Style |= 4;
                }
                if ((this.RightToLeft == RightToLeft.Yes) && this.RightToLeftLayout)
                {
                    createParams.ExStyle |= 0x400000;
                    createParams.ExStyle &= -28673;
                }
                return createParams;
            }
        }

        protected override System.Windows.Forms.ImeMode DefaultImeMode
        {
            get
            {
                return System.Windows.Forms.ImeMode.Disable;
            }
        }

        protected override System.Windows.Forms.Padding DefaultMargin
        {
            get
            {
                return new System.Windows.Forms.Padding(9);
            }
        }

        protected override System.Drawing.Size DefaultSize
        {
            get
            {
                return this.GetMinReqRect();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override bool DoubleBuffered
        {
            get
            {
                return base.DoubleBuffered;
            }
            set
            {
                base.DoubleBuffered = value;
            }
        }

        [DefaultValue(7), System.Windows.Forms.SRDescription("MonthCalendarFirstDayOfWeekDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatBehavior")]
        public Day FirstDayOfWeek
        {
            get
            {
                return this.firstDayOfWeek;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 7))
                {
                    throw new InvalidEnumArgumentException("FirstDayOfWeek", (int) value, typeof(Day));
                }
                if (value != this.firstDayOfWeek)
                {
                    this.firstDayOfWeek = value;
                    if (base.IsHandleCreated)
                    {
                        if (value == Day.Default)
                        {
                            base.RecreateHandle();
                        }
                        else
                        {
                            base.SendMessage(0x100f, 0, (int) value);
                        }
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("MonthCalendarForeColorDescr")]
        public override System.Drawing.Color ForeColor
        {
            get
            {
                if (this.ShouldSerializeForeColor())
                {
                    return base.ForeColor;
                }
                return SystemColors.WindowText;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public System.Windows.Forms.ImeMode ImeMode
        {
            get
            {
                return base.ImeMode;
            }
            set
            {
                base.ImeMode = value;
            }
        }

        [System.Windows.Forms.SRDescription("MonthCalendarMaxDateDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public DateTime MaxDate
        {
            get
            {
                return DateTimePicker.EffectiveMaxDate(this.maxDate);
            }
            set
            {
                if (value != this.maxDate)
                {
                    if (value < DateTimePicker.EffectiveMinDate(this.minDate))
                    {
                        throw new ArgumentOutOfRangeException("MaxDate", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", new object[] { "MaxDate", FormatDate(value), "MinDate" }));
                    }
                    this.maxDate = value;
                    this.SetRange();
                }
            }
        }

        [System.Windows.Forms.SRDescription("MonthCalendarMaxSelectionCountDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(7)]
        public int MaxSelectionCount
        {
            get
            {
                return this.maxSelectionCount;
            }
            set
            {
                if (value < 1)
                {
                    object[] args = new object[] { "MaxSelectionCount", value.ToString("D", CultureInfo.CurrentCulture), 1.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("MaxSelectionCount", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                if (value != this.maxSelectionCount)
                {
                    if (base.IsHandleCreated && (((int) ((long) base.SendMessage(0x1004, value, 0))) == 0))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("MonthCalendarMaxSelCount", new object[] { value.ToString("D", CultureInfo.CurrentCulture) }), "MaxSelectionCount");
                    }
                    this.maxSelectionCount = value;
                }
            }
        }

        [System.Windows.Forms.SRDescription("MonthCalendarMinDateDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public DateTime MinDate
        {
            get
            {
                return DateTimePicker.EffectiveMinDate(this.minDate);
            }
            set
            {
                if (value != this.minDate)
                {
                    if (value > DateTimePicker.EffectiveMaxDate(this.maxDate))
                    {
                        throw new ArgumentOutOfRangeException("MinDate", System.Windows.Forms.SR.GetString("InvalidHighBoundArgument", new object[] { "MinDate", FormatDate(value), "MaxDate" }));
                    }
                    if (value < DateTimePicker.MinimumDateTime)
                    {
                        throw new ArgumentOutOfRangeException("MinDate", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", new object[] { "MinDate", FormatDate(value), FormatDate(DateTimePicker.MinimumDateTime) }));
                    }
                    this.minDate = value;
                    this.SetRange();
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("MonthCalendarMonthlyBoldedDatesDescr")]
        public DateTime[] MonthlyBoldedDates
        {
            get
            {
                DateTime[] timeArray = new DateTime[this.monthlyArrayOfDates.Count];
                for (int i = 0; i < this.monthlyArrayOfDates.Count; i++)
                {
                    timeArray[i] = (DateTime) this.monthlyArrayOfDates[i];
                }
                return timeArray;
            }
            set
            {
                this.monthlyArrayOfDates.Clear();
                this.datesToBoldMonthly = 0;
                if ((value != null) && (value.Length > 0))
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        this.monthlyArrayOfDates.Add(value[i]);
                    }
                    for (int j = 0; j < value.Length; j++)
                    {
                        this.datesToBoldMonthly |= ((int) 1) << (value[j].Day - 1);
                    }
                }
                base.RecreateHandle();
            }
        }

        private DateTime Now
        {
            get
            {
                return DateTime.Now.Date;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public System.Windows.Forms.Padding Padding
        {
            get
            {
                return base.Padding;
            }
            set
            {
                base.Padding = value;
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("ControlRightToLeftLayoutDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatAppearance")]
        public virtual bool RightToLeftLayout
        {
            get
            {
                return this.rightToLeftLayout;
            }
            set
            {
                if (value != this.rightToLeftLayout)
                {
                    this.rightToLeftLayout = value;
                    using (new LayoutTransaction(this, this, PropertyNames.RightToLeftLayout))
                    {
                        this.OnRightToLeftLayoutChanged(EventArgs.Empty);
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("MonthCalendarScrollChangeDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(0)]
        public int ScrollChange
        {
            get
            {
                return this.scrollChange;
            }
            set
            {
                if (this.scrollChange != value)
                {
                    if (value < 0)
                    {
                        object[] args = new object[] { "ScrollChange", value.ToString("D", CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("ScrollChange", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                    }
                    if (value > 0x4e20)
                    {
                        object[] objArray2 = new object[] { "ScrollChange", value.ToString("D", CultureInfo.CurrentCulture), 0x4e20.ToString("D", CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("ScrollChange", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", objArray2));
                    }
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x1014, value, 0);
                    }
                    this.scrollChange = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("MonthCalendarSelectionEndDescr"), System.Windows.Forms.SRCategory("CatBehavior"), Browsable(false)]
        public DateTime SelectionEnd
        {
            get
            {
                return this.selectionEnd;
            }
            set
            {
                if (this.selectionEnd != value)
                {
                    if (value < this.MinDate)
                    {
                        throw new ArgumentOutOfRangeException("SelectionEnd", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", new object[] { "SelectionEnd", FormatDate(value), "MinDate" }));
                    }
                    if (value > this.MaxDate)
                    {
                        throw new ArgumentOutOfRangeException("SelectionEnd", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", new object[] { "SelectionEnd", FormatDate(value), "MaxDate" }));
                    }
                    if (this.selectionStart > value)
                    {
                        this.selectionStart = value;
                    }
                    TimeSpan span = (TimeSpan) (value - this.selectionStart);
                    if (span.Days >= this.maxSelectionCount)
                    {
                        this.selectionStart = value.AddDays((double) (1 - this.maxSelectionCount));
                    }
                    this.SetSelRange(this.selectionStart, value);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("MonthCalendarSelectionRangeDescr"), Bindable(true)]
        public System.Windows.Forms.SelectionRange SelectionRange
        {
            get
            {
                return new System.Windows.Forms.SelectionRange(this.SelectionStart, this.SelectionEnd);
            }
            set
            {
                this.SetSelectionRange(value.Start, value.End);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("MonthCalendarSelectionStartDescr"), Browsable(false)]
        public DateTime SelectionStart
        {
            get
            {
                return this.selectionStart;
            }
            set
            {
                if (this.selectionStart != value)
                {
                    if (value < this.minDate)
                    {
                        throw new ArgumentOutOfRangeException("SelectionStart", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", new object[] { "SelectionStart", FormatDate(value), "MinDate" }));
                    }
                    if (value > this.maxDate)
                    {
                        throw new ArgumentOutOfRangeException("SelectionStart", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", new object[] { "SelectionStart", FormatDate(value), "MaxDate" }));
                    }
                    if (this.selectionEnd < value)
                    {
                        this.selectionEnd = value;
                    }
                    TimeSpan span = (TimeSpan) (this.selectionEnd - value);
                    if (span.Days >= this.maxSelectionCount)
                    {
                        this.selectionEnd = value.AddDays((double) (this.maxSelectionCount - 1));
                    }
                    this.SetSelRange(value, this.selectionEnd);
                }
            }
        }

        [System.Windows.Forms.SRDescription("MonthCalendarShowTodayDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true)]
        public bool ShowToday
        {
            get
            {
                return this.showToday;
            }
            set
            {
                if (this.showToday != value)
                {
                    this.showToday = value;
                    base.UpdateStyles();
                    this.AdjustSize();
                }
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("MonthCalendarShowTodayCircleDescr")]
        public bool ShowTodayCircle
        {
            get
            {
                return this.showTodayCircle;
            }
            set
            {
                if (this.showTodayCircle != value)
                {
                    this.showTodayCircle = value;
                    base.UpdateStyles();
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("MonthCalendarShowWeekNumbersDescr"), System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true)]
        public bool ShowWeekNumbers
        {
            get
            {
                return this.showWeekNumbers;
            }
            set
            {
                if (this.showWeekNumbers != value)
                {
                    this.showWeekNumbers = value;
                    base.UpdateStyles();
                    this.AdjustSize();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("MonthCalendarSingleMonthSizeDescr"), Browsable(false)]
        public System.Drawing.Size SingleMonthSize
        {
            get
            {
                System.Windows.Forms.NativeMethods.RECT lparam = new System.Windows.Forms.NativeMethods.RECT();
                if (!base.IsHandleCreated)
                {
                    return DefaultSingleMonthSize;
                }
                if (((int) ((long) base.SendMessage(0x1009, 0, ref lparam))) == 0)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("InvalidSingleMonthSize"));
                }
                return new System.Drawing.Size(lparam.right, lparam.bottom);
            }
        }

        [Localizable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Drawing.Size Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                base.Size = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Bindable(false), Browsable(false)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("MonthCalendarTitleBackColorDescr")]
        public System.Drawing.Color TitleBackColor
        {
            get
            {
                return this.titleBackColor;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidNullArgument", new object[] { "value" }));
                }
                this.titleBackColor = value;
                this.SetControlColor(2, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("MonthCalendarTitleForeColorDescr")]
        public System.Drawing.Color TitleForeColor
        {
            get
            {
                return this.titleForeColor;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidNullArgument", new object[] { "value" }));
                }
                this.titleForeColor = value;
                this.SetControlColor(3, value);
            }
        }

        [System.Windows.Forms.SRDescription("MonthCalendarTodayDateDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public DateTime TodayDate
        {
            get
            {
                if (this.todayDateSet)
                {
                    return this.todayDate;
                }
                if (base.IsHandleCreated)
                {
                    System.Windows.Forms.NativeMethods.SYSTEMTIME lParam = new System.Windows.Forms.NativeMethods.SYSTEMTIME();
                    int num1 = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x100d, 0, lParam);
                    return DateTimePicker.SysTimeToDateTime(lParam).Date;
                }
                return this.Now.Date;
            }
            set
            {
                if (!this.todayDateSet || (DateTime.Compare(value, this.todayDate) != 0))
                {
                    if (DateTime.Compare(value, this.maxDate) > 0)
                    {
                        throw new ArgumentOutOfRangeException("TodayDate", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", new object[] { "TodayDate", FormatDate(value), FormatDate(this.maxDate) }));
                    }
                    if (DateTime.Compare(value, this.minDate) < 0)
                    {
                        throw new ArgumentOutOfRangeException("TodayDate", System.Windows.Forms.SR.GetString("InvalidLowBoundArgument", new object[] { "TodayDate", FormatDate(value), FormatDate(this.minDate) }));
                    }
                    this.todayDate = value.Date;
                    this.todayDateSet = true;
                    this.UpdateTodayDate();
                }
            }
        }

        [System.Windows.Forms.SRDescription("MonthCalendarTodayDateSetDescr"), System.Windows.Forms.SRCategory("CatBehavior"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool TodayDateSet
        {
            get
            {
                return this.todayDateSet;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("MonthCalendarTrailingForeColorDescr")]
        public System.Drawing.Color TrailingForeColor
        {
            get
            {
                return this.trailingForeColor;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidNullArgument", new object[] { "value" }));
                }
                this.trailingForeColor = value;
                this.SetControlColor(5, value);
            }
        }

        public enum HitArea
        {
            Nowhere,
            TitleBackground,
            TitleMonth,
            TitleYear,
            NextMonthButton,
            PrevMonthButton,
            CalendarBackground,
            Date,
            NextMonthDate,
            PrevMonthDate,
            DayOfWeek,
            WeekNumbers,
            TodayLink
        }

        public sealed class HitTestInfo
        {
            private readonly System.Windows.Forms.MonthCalendar.HitArea hitArea;
            private readonly System.Drawing.Point point;
            private readonly DateTime time;

            internal HitTestInfo(System.Drawing.Point pt, System.Windows.Forms.MonthCalendar.HitArea area)
            {
                this.point = pt;
                this.hitArea = area;
            }

            internal HitTestInfo(System.Drawing.Point pt, System.Windows.Forms.MonthCalendar.HitArea area, DateTime time)
            {
                this.point = pt;
                this.hitArea = area;
                this.time = time;
            }

            internal static bool HitAreaHasValidDateTime(System.Windows.Forms.MonthCalendar.HitArea hitArea)
            {
                System.Windows.Forms.MonthCalendar.HitArea area = hitArea;
                if ((area != System.Windows.Forms.MonthCalendar.HitArea.Date) && (area != System.Windows.Forms.MonthCalendar.HitArea.WeekNumbers))
                {
                    return false;
                }
                return true;
            }

            public System.Windows.Forms.MonthCalendar.HitArea HitArea
            {
                get
                {
                    return this.hitArea;
                }
            }

            public System.Drawing.Point Point
            {
                get
                {
                    return this.point;
                }
            }

            public DateTime Time
            {
                get
                {
                    return this.time;
                }
            }
        }
    }
}

