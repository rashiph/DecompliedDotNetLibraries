namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;

    [System.Windows.Forms.SRDescription("DescriptionDateTimePicker"), DefaultEvent("ValueChanged"), ComVisible(true), Designer("System.Windows.Forms.Design.DateTimePickerDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultBindingProperty("Value"), DefaultProperty("Value"), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class DateTimePicker : Control
    {
        private Font calendarFont;
        private Control.FontHandleWrapper calendarFontHandleWrapper;
        private System.Drawing.Color calendarForeColor = Control.DefaultForeColor;
        private System.Drawing.Color calendarMonthBackground = DefaultMonthBackColor;
        private System.Drawing.Color calendarTitleBackColor = DefaultTitleBackColor;
        private System.Drawing.Color calendarTitleForeColor = DefaultTitleForeColor;
        private System.Drawing.Color calendarTrailingText = DefaultTrailingForeColor;
        private DateTime creationTime = DateTime.Now;
        private string customFormat;
        protected static readonly System.Drawing.Color DefaultMonthBackColor = SystemColors.Window;
        protected static readonly System.Drawing.Color DefaultTitleBackColor = SystemColors.ActiveCaption;
        protected static readonly System.Drawing.Color DefaultTitleForeColor = SystemColors.ActiveCaptionText;
        protected static readonly System.Drawing.Color DefaultTrailingForeColor = SystemColors.GrayText;
        private static readonly object EVENT_FORMATCHANGED = new object();
        private DateTimePickerFormat format;
        private DateTime max = DateTime.MaxValue;
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DateTime MaxDateTime = new DateTime(0x270e, 12, 0x1f);
        private DateTime min = DateTime.MinValue;
        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public static readonly DateTime MinDateTime = new DateTime(0x6d9, 1, 1);
        private short prefHeightCache = -1;
        private bool rightToLeftLayout;
        private int style;
        private const int TIMEFORMAT_NOUPDOWN = 8;
        private bool userHasSetValue;
        private bool validTime = true;
        private DateTime value = DateTime.Now;

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler BackColorChanged
        {
            add
            {
                base.BackColorChanged += value;
            }
            remove
            {
                base.BackColorChanged -= value;
            }
        }

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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("DateTimePickerOnCloseUpDescr")]
        public event EventHandler CloseUp;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("DateTimePickerOnDropDownDescr")]
        public event EventHandler DropDown;

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler ForeColorChanged
        {
            add
            {
                base.ForeColorChanged += value;
            }
            remove
            {
                base.ForeColorChanged -= value;
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("DateTimePickerOnFormatChangedDescr")]
        public event EventHandler FormatChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_FORMATCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_FORMATCHANGED, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnRightToLeftLayoutChangedDescr")]
        public event EventHandler RightToLeftLayoutChanged;

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
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

        [System.Windows.Forms.SRDescription("valueChangedEventDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event EventHandler ValueChanged;

        public DateTimePicker()
        {
            base.SetState2(0x800, true);
            base.SetStyle(ControlStyles.FixedHeight, true);
            base.SetStyle(ControlStyles.StandardClick | ControlStyles.UserPaint, false);
            this.format = DateTimePickerFormat.Long;
        }

        internal override Rectangle ApplyBoundsConstraints(int suggestedX, int suggestedY, int proposedWidth, int proposedHeight)
        {
            return base.ApplyBoundsConstraints(suggestedX, suggestedY, proposedWidth, this.PreferredHeight);
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new DateTimePickerAccessibleObject(this);
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
            this.creationTime = DateTime.Now;
            base.CreateHandle();
            if (this.userHasSetValue && this.validTime)
            {
                int wParam = 0;
                System.Windows.Forms.NativeMethods.SYSTEMTIME lParam = DateTimeToSysTime(this.Value);
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1002, wParam, lParam);
            }
            else if (!this.validTime)
            {
                int num2 = 1;
                System.Windows.Forms.NativeMethods.SYSTEMTIME systemtime2 = null;
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1002, num2, systemtime2);
            }
            if (this.format == DateTimePickerFormat.Custom)
            {
                base.SendMessage(System.Windows.Forms.NativeMethods.DTM_SETFORMAT, 0, this.customFormat);
            }
            this.UpdateUpDown();
            this.SetAllControlColors();
            this.SetControlCalendarFont();
            this.SetRange();
        }

        internal static System.Windows.Forms.NativeMethods.SYSTEMTIME DateTimeToSysTime(DateTime time)
        {
            return new System.Windows.Forms.NativeMethods.SYSTEMTIME { wYear = (short) time.Year, wMonth = (short) time.Month, wDayOfWeek = (short) time.DayOfWeek, wDay = (short) time.Day, wHour = (short) time.Hour, wMinute = (short) time.Minute, wSecond = (short) time.Second, wMilliseconds = 0 };
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected override void DestroyHandle()
        {
            this.value = this.Value;
            base.DestroyHandle();
        }

        internal static DateTime EffectiveMaxDate(DateTime maxDate)
        {
            DateTime maximumDateTime = MaximumDateTime;
            if (maxDate > maximumDateTime)
            {
                return maximumDateTime;
            }
            return maxDate;
        }

        internal static DateTime EffectiveMinDate(DateTime minDate)
        {
            DateTime minimumDateTime = MinimumDateTime;
            if (minDate < minimumDateTime)
            {
                return minimumDateTime;
            }
            return minDate;
        }

        private static string FormatDateTime(DateTime value)
        {
            return value.ToString("G", CultureInfo.CurrentCulture);
        }

        internal override Size GetPreferredSizeCore(Size proposedConstraints)
        {
            return new Size(CommonProperties.GetSpecifiedBounds(this).Width, this.PreferredHeight);
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

        protected virtual void OnCloseUp(EventArgs eventargs)
        {
            if (this.onCloseUp != null)
            {
                this.onCloseUp(this, eventargs);
            }
        }

        protected virtual void OnDropDown(EventArgs eventargs)
        {
            if (this.onDropDown != null)
            {
                this.onDropDown(this, eventargs);
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.prefHeightCache = -1;
            base.Height = this.PreferredHeight;
            if (this.calendarFont == null)
            {
                this.calendarFontHandleWrapper = null;
                this.SetControlCalendarFont();
            }
        }

        protected virtual void OnFormatChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_FORMATCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
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

        protected override void OnSystemColorsChanged(EventArgs e)
        {
            this.SetAllControlColors();
            base.OnSystemColorsChanged(e);
        }

        protected virtual void OnValueChanged(EventArgs eventargs)
        {
            if (this.onValueChanged != null)
            {
                this.onValueChanged(this, eventargs);
            }
        }

        private void ResetCalendarFont()
        {
            this.CalendarFont = null;
        }

        private void ResetCalendarForeColor()
        {
            this.CalendarForeColor = Control.DefaultForeColor;
        }

        private void ResetCalendarMonthBackground()
        {
            this.CalendarMonthBackground = DefaultMonthBackColor;
        }

        private void ResetCalendarTitleBackColor()
        {
            this.CalendarTitleBackColor = DefaultTitleBackColor;
        }

        private void ResetCalendarTitleForeColor()
        {
            this.CalendarTitleBackColor = Control.DefaultForeColor;
        }

        private void ResetCalendarTrailingForeColor()
        {
            this.CalendarTrailingForeColor = DefaultTrailingForeColor;
        }

        private void ResetFormat()
        {
            this.Format = DateTimePickerFormat.Long;
        }

        private void ResetMaxDate()
        {
            this.MaxDate = MaximumDateTime;
        }

        private void ResetMinDate()
        {
            this.MinDate = MinimumDateTime;
        }

        private void ResetValue()
        {
            this.value = DateTime.Now;
            this.userHasSetValue = false;
            if (base.IsHandleCreated)
            {
                int wParam = 0;
                System.Windows.Forms.NativeMethods.SYSTEMTIME lParam = DateTimeToSysTime(this.value);
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1002, wParam, lParam);
            }
            this.Checked = false;
            this.OnValueChanged(EventArgs.Empty);
            this.OnTextChanged(EventArgs.Empty);
        }

        private void SetAllControlColors()
        {
            this.SetControlColor(4, this.calendarMonthBackground);
            this.SetControlColor(1, this.calendarForeColor);
            this.SetControlColor(2, this.calendarTitleBackColor);
            this.SetControlColor(3, this.calendarTitleForeColor);
            this.SetControlColor(5, this.calendarTrailingText);
        }

        private void SetControlCalendarFont()
        {
            if (base.IsHandleCreated)
            {
                base.SendMessage(0x1009, this.CalendarFontHandle, System.Windows.Forms.NativeMethods.InvalidIntPtr);
            }
        }

        private void SetControlColor(int colorIndex, System.Drawing.Color value)
        {
            if (base.IsHandleCreated)
            {
                base.SendMessage(0x1006, colorIndex, ColorTranslator.ToWin32(value));
            }
        }

        private void SetRange()
        {
            this.SetRange(EffectiveMinDate(this.min), EffectiveMaxDate(this.max));
        }

        private void SetRange(DateTime min, DateTime max)
        {
            if (base.IsHandleCreated)
            {
                int wParam = 0;
                System.Windows.Forms.NativeMethods.SYSTEMTIMEARRAY lParam = new System.Windows.Forms.NativeMethods.SYSTEMTIMEARRAY();
                wParam |= 3;
                System.Windows.Forms.NativeMethods.SYSTEMTIME systemtime = DateTimeToSysTime(min);
                lParam.wYear1 = systemtime.wYear;
                lParam.wMonth1 = systemtime.wMonth;
                lParam.wDayOfWeek1 = systemtime.wDayOfWeek;
                lParam.wDay1 = systemtime.wDay;
                lParam.wHour1 = systemtime.wHour;
                lParam.wMinute1 = systemtime.wMinute;
                lParam.wSecond1 = systemtime.wSecond;
                lParam.wMilliseconds1 = systemtime.wMilliseconds;
                systemtime = DateTimeToSysTime(max);
                lParam.wYear2 = systemtime.wYear;
                lParam.wMonth2 = systemtime.wMonth;
                lParam.wDayOfWeek2 = systemtime.wDayOfWeek;
                lParam.wDay2 = systemtime.wDay;
                lParam.wHour2 = systemtime.wHour;
                lParam.wMinute2 = systemtime.wMinute;
                lParam.wSecond2 = systemtime.wSecond;
                lParam.wMilliseconds2 = systemtime.wMilliseconds;
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1004, wParam, lParam);
            }
        }

        private void SetStyleBit(bool flag, int bit)
        {
            if (((this.style & bit) != 0) != flag)
            {
                if (flag)
                {
                    this.style |= bit;
                }
                else
                {
                    this.style &= ~bit;
                }
                if (base.IsHandleCreated)
                {
                    base.RecreateHandle();
                    base.Invalidate();
                    base.Update();
                }
            }
        }

        private bool ShouldSerializeCalendarFont()
        {
            return (this.calendarFont != null);
        }

        private bool ShouldSerializeCalendarForeColor()
        {
            return !this.CalendarForeColor.Equals(Control.DefaultForeColor);
        }

        private bool ShouldSerializeCalendarMonthBackground()
        {
            return !this.calendarMonthBackground.Equals(DefaultMonthBackColor);
        }

        private bool ShouldSerializeCalendarTitleBackColor()
        {
            return !this.calendarTitleBackColor.Equals(DefaultTitleBackColor);
        }

        private bool ShouldSerializeCalendarTitleForeColor()
        {
            return !this.calendarTitleForeColor.Equals(DefaultTitleForeColor);
        }

        private bool ShouldSerializeCalendarTrailingForeColor()
        {
            return !this.calendarTrailingText.Equals(DefaultTrailingForeColor);
        }

        private bool ShouldSerializeFormat()
        {
            return (this.Format != DateTimePickerFormat.Long);
        }

        private bool ShouldSerializeMaxDate()
        {
            return ((this.max != MaximumDateTime) && (this.max != DateTime.MaxValue));
        }

        private bool ShouldSerializeMinDate()
        {
            return ((this.min != MinimumDateTime) && (this.min != DateTime.MinValue));
        }

        private bool ShouldSerializeValue()
        {
            return this.userHasSetValue;
        }

        internal static DateTime SysTimeToDateTime(System.Windows.Forms.NativeMethods.SYSTEMTIME s)
        {
            return new DateTime(s.wYear, s.wMonth, s.wDay, s.wHour, s.wMinute, s.wSecond);
        }

        public override string ToString()
        {
            return (base.ToString() + ", Value: " + FormatDateTime(this.Value));
        }

        private void UpdateUpDown()
        {
            if (this.ShowUpDown)
            {
                EnumChildren wrapper = new EnumChildren();
                System.Windows.Forms.NativeMethods.EnumChildrenCallback lpEnumFunc = new System.Windows.Forms.NativeMethods.EnumChildrenCallback(wrapper.enumChildren);
                System.Windows.Forms.UnsafeNativeMethods.EnumChildWindows(new HandleRef(this, base.Handle), lpEnumFunc, System.Windows.Forms.NativeMethods.NullHandleRef);
                if (wrapper.hwndFound != IntPtr.Zero)
                {
                    System.Windows.Forms.SafeNativeMethods.InvalidateRect(new HandleRef(wrapper, wrapper.hwndFound), (System.Windows.Forms.NativeMethods.COMRECT) null, true);
                    System.Windows.Forms.SafeNativeMethods.UpdateWindow(new HandleRef(wrapper, wrapper.hwndFound));
                }
            }
        }

        private void UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs pref)
        {
            if (pref.Category == UserPreferenceCategory.Locale)
            {
                base.RecreateHandle();
            }
        }

        private void WmCloseUp(ref Message m)
        {
            this.OnCloseUp(EventArgs.Empty);
        }

        private void WmDateTimeChange(ref Message m)
        {
            System.Windows.Forms.NativeMethods.NMDATETIMECHANGE lParam = (System.Windows.Forms.NativeMethods.NMDATETIMECHANGE) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMDATETIMECHANGE));
            DateTime time = this.value;
            bool validTime = this.validTime;
            if (lParam.dwFlags != 1)
            {
                this.validTime = true;
                this.value = SysTimeToDateTime(lParam.st);
                this.userHasSetValue = true;
            }
            else
            {
                this.validTime = false;
            }
            if ((this.value != time) || (validTime != this.validTime))
            {
                this.OnValueChanged(EventArgs.Empty);
                this.OnTextChanged(EventArgs.Empty);
            }
        }

        private void WmDropDown(ref Message m)
        {
            if (this.RightToLeftLayout && (this.RightToLeft == RightToLeft.Yes))
            {
                IntPtr handle = base.SendMessage(0x1008, 0, 0);
                if (handle != IntPtr.Zero)
                {
                    int windowLong = (int) ((long) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, handle), -20));
                    windowLong |= 0x500000;
                    windowLong &= -12289;
                    System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, handle), -20, new HandleRef(this, (IntPtr) windowLong));
                }
            }
            this.OnDropDown(EventArgs.Empty);
        }

        private void WmReflectCommand(ref Message m)
        {
            if (m.HWnd == base.Handle)
            {
                System.Windows.Forms.NativeMethods.NMHDR lParam = (System.Windows.Forms.NativeMethods.NMHDR) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHDR));
                switch (lParam.code)
                {
                    case -754:
                        this.WmDropDown(ref m);
                        break;

                    case -753:
                        this.WmCloseUp(ref m);
                        return;

                    case -759:
                        this.WmDateTimeChange(ref m);
                        return;

                    default:
                        return;
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x47:
                    base.WndProc(ref m);
                    this.UpdateUpDown();
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [Localizable(true), AmbientValue((string) null), System.Windows.Forms.SRDescription("DateTimePickerCalendarFontDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public Font CalendarFont
        {
            get
            {
                if (this.calendarFont == null)
                {
                    return this.Font;
                }
                return this.calendarFont;
            }
            set
            {
                if (((value == null) && (this.calendarFont != null)) || ((value != null) && !value.Equals(this.calendarFont)))
                {
                    this.calendarFont = value;
                    this.calendarFontHandleWrapper = null;
                    this.SetControlCalendarFont();
                }
            }
        }

        private IntPtr CalendarFontHandle
        {
            get
            {
                if (this.calendarFont == null)
                {
                    return base.FontHandle;
                }
                if (this.calendarFontHandleWrapper == null)
                {
                    this.calendarFontHandleWrapper = new Control.FontHandleWrapper(this.CalendarFont);
                }
                return this.calendarFontHandleWrapper.Handle;
            }
        }

        [System.Windows.Forms.SRDescription("DateTimePickerCalendarForeColorDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public System.Drawing.Color CalendarForeColor
        {
            get
            {
                return this.calendarForeColor;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidNullArgument", new object[] { "value" }));
                }
                if (!value.Equals(this.calendarForeColor))
                {
                    this.calendarForeColor = value;
                    this.SetControlColor(1, value);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("DateTimePickerCalendarMonthBackgroundDescr")]
        public System.Drawing.Color CalendarMonthBackground
        {
            get
            {
                return this.calendarMonthBackground;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidNullArgument", new object[] { "value" }));
                }
                if (!value.Equals(this.calendarMonthBackground))
                {
                    this.calendarMonthBackground = value;
                    this.SetControlColor(4, value);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("DateTimePickerCalendarTitleBackColorDescr")]
        public System.Drawing.Color CalendarTitleBackColor
        {
            get
            {
                return this.calendarTitleBackColor;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidNullArgument", new object[] { "value" }));
                }
                if (!value.Equals(this.calendarTitleBackColor))
                {
                    this.calendarTitleBackColor = value;
                    this.SetControlColor(2, value);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("DateTimePickerCalendarTitleForeColorDescr")]
        public System.Drawing.Color CalendarTitleForeColor
        {
            get
            {
                return this.calendarTitleForeColor;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidNullArgument", new object[] { "value" }));
                }
                if (!value.Equals(this.calendarTitleForeColor))
                {
                    this.calendarTitleForeColor = value;
                    this.SetControlColor(3, value);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("DateTimePickerCalendarTrailingForeColorDescr")]
        public System.Drawing.Color CalendarTrailingForeColor
        {
            get
            {
                return this.calendarTrailingText;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidNullArgument", new object[] { "value" }));
                }
                if (!value.Equals(this.calendarTrailingText))
                {
                    this.calendarTrailingText = value;
                    this.SetControlColor(5, value);
                }
            }
        }

        [Bindable(true), System.Windows.Forms.SRDescription("DateTimePickerCheckedDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true)]
        public bool Checked
        {
            get
            {
                if (this.ShowCheckBox && base.IsHandleCreated)
                {
                    System.Windows.Forms.NativeMethods.SYSTEMTIME lParam = new System.Windows.Forms.NativeMethods.SYSTEMTIME();
                    int num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1001, 0, lParam);
                    return (num == 0);
                }
                return this.validTime;
            }
            set
            {
                if (this.Checked != value)
                {
                    if (this.ShowCheckBox && base.IsHandleCreated)
                    {
                        if (value)
                        {
                            int wParam = 0;
                            System.Windows.Forms.NativeMethods.SYSTEMTIME lParam = DateTimeToSysTime(this.Value);
                            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1002, wParam, lParam);
                        }
                        else
                        {
                            int num2 = 1;
                            System.Windows.Forms.NativeMethods.SYSTEMTIME systemtime2 = null;
                            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1002, num2, systemtime2);
                        }
                    }
                    this.validTime = value;
                }
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ClassName = "SysDateTimePick32";
                createParams.Style |= this.style;
                switch (this.format)
                {
                    case DateTimePickerFormat.Long:
                        createParams.Style |= 4;
                        break;

                    case DateTimePickerFormat.Time:
                        createParams.Style |= 8;
                        break;
                }
                createParams.ExStyle |= 0x200;
                if ((this.RightToLeft == RightToLeft.Yes) && this.RightToLeftLayout)
                {
                    createParams.ExStyle |= 0x400000;
                    createParams.ExStyle &= -28673;
                }
                return createParams;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), RefreshProperties(RefreshProperties.Repaint), DefaultValue((string) null), Localizable(true), System.Windows.Forms.SRDescription("DateTimePickerCustomFormatDescr")]
        public string CustomFormat
        {
            get
            {
                return this.customFormat;
            }
            set
            {
                if (((value != null) && !value.Equals(this.customFormat)) || ((value == null) && (this.customFormat != null)))
                {
                    this.customFormat = value;
                    if (base.IsHandleCreated && (this.format == DateTimePickerFormat.Custom))
                    {
                        base.SendMessage(System.Windows.Forms.NativeMethods.DTM_SETFORMAT, 0, this.customFormat);
                    }
                }
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(200, this.PreferredHeight);
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

        [System.Windows.Forms.SRDescription("DateTimePickerDropDownAlignDescr"), DefaultValue(0), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance")]
        public LeftRightAlignment DropDownAlign
        {
            get
            {
                if ((this.style & 0x20) == 0)
                {
                    return LeftRightAlignment.Left;
                }
                return LeftRightAlignment.Right;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(LeftRightAlignment));
                }
                this.SetStyleBit(value == LeftRightAlignment.Right, 0x20);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRDescription("DateTimePickerFormatDescr"), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRCategory("CatAppearance")]
        public DateTimePickerFormat Format
        {
            get
            {
                return this.format;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 1, 8, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DateTimePickerFormat));
                }
                if (this.format != value)
                {
                    this.format = value;
                    base.RecreateHandle();
                    this.OnFormatChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("DateTimePickerMaxDateDescr")]
        public DateTime MaxDate
        {
            get
            {
                return EffectiveMaxDate(this.max);
            }
            set
            {
                if (value != this.max)
                {
                    if (value < EffectiveMinDate(this.min))
                    {
                        throw new ArgumentOutOfRangeException("MaxDate", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", new object[] { "MaxDate", FormatDateTime(value), "MinDate" }));
                    }
                    if (value > MaximumDateTime)
                    {
                        throw new ArgumentOutOfRangeException("MaxDate", System.Windows.Forms.SR.GetString("DateTimePickerMaxDate", new object[] { FormatDateTime(MaxDateTime) }));
                    }
                    this.max = value;
                    this.SetRange();
                    if (this.Value > this.max)
                    {
                        this.Value = this.max;
                    }
                }
            }
        }

        public static DateTime MaximumDateTime
        {
            get
            {
                DateTime maxSupportedDateTime = CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime;
                if (maxSupportedDateTime.Year > MaxDateTime.Year)
                {
                    return MaxDateTime;
                }
                return maxSupportedDateTime;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("DateTimePickerMinDateDescr")]
        public DateTime MinDate
        {
            get
            {
                return EffectiveMinDate(this.min);
            }
            set
            {
                if (value != this.min)
                {
                    if (value > EffectiveMaxDate(this.max))
                    {
                        throw new ArgumentOutOfRangeException("MinDate", System.Windows.Forms.SR.GetString("InvalidHighBoundArgument", new object[] { "MinDate", FormatDateTime(value), "MaxDate" }));
                    }
                    if (value < MinimumDateTime)
                    {
                        throw new ArgumentOutOfRangeException("MinDate", System.Windows.Forms.SR.GetString("DateTimePickerMinDate", new object[] { FormatDateTime(MinimumDateTime) }));
                    }
                    this.min = value;
                    this.SetRange();
                    if (this.Value < this.min)
                    {
                        this.Value = this.min;
                    }
                }
            }
        }

        public static DateTime MinimumDateTime
        {
            get
            {
                DateTime minSupportedDateTime = CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime;
                if (minSupportedDateTime.Year < 0x6d9)
                {
                    return new DateTime(0x6d9, 1, 1);
                }
                return minSupportedDateTime;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int PreferredHeight
        {
            get
            {
                if (this.prefHeightCache > -1)
                {
                    return this.prefHeightCache;
                }
                int num = base.FontHeight + ((SystemInformation.BorderSize.Height * 4) + 3);
                this.prefHeightCache = (short) num;
                return num;
            }
        }

        [System.Windows.Forms.SRDescription("ControlRightToLeftLayoutDescr"), Localizable(true), DefaultValue(false), System.Windows.Forms.SRCategory("CatAppearance")]
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

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(false), System.Windows.Forms.SRDescription("DateTimePickerShowNoneDescr")]
        public bool ShowCheckBox
        {
            get
            {
                return ((this.style & 2) != 0);
            }
            set
            {
                this.SetStyleBit(value, 2);
            }
        }

        [System.Windows.Forms.SRDescription("DateTimePickerShowUpDownDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatAppearance")]
        public bool ShowUpDown
        {
            get
            {
                return ((this.style & 1) != 0);
            }
            set
            {
                if (this.ShowUpDown != value)
                {
                    this.SetStyleBit(value, 1);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if ((value == null) || (value.Length == 0))
                {
                    this.ResetValue();
                }
                else
                {
                    this.Value = DateTime.Parse(value, CultureInfo.CurrentCulture);
                }
            }
        }

        [Bindable(true), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("DateTimePickerValueDescr"), RefreshProperties(RefreshProperties.All)]
        public DateTime Value
        {
            get
            {
                if (!this.userHasSetValue && this.validTime)
                {
                    return this.creationTime;
                }
                return this.value;
            }
            set
            {
                bool flag = !DateTime.Equals(this.Value, value);
                if (!this.userHasSetValue || flag)
                {
                    if ((value < this.MinDate) || (value > this.MaxDate))
                    {
                        throw new ArgumentOutOfRangeException("Value", System.Windows.Forms.SR.GetString("InvalidBoundArgument", new object[] { "Value", FormatDateTime(value), "'MinDate'", "'MaxDate'" }));
                    }
                    string text = this.Text;
                    this.value = value;
                    this.userHasSetValue = true;
                    if (base.IsHandleCreated)
                    {
                        int wParam = 0;
                        System.Windows.Forms.NativeMethods.SYSTEMTIME lParam = DateTimeToSysTime(value);
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1002, wParam, lParam);
                    }
                    if (flag)
                    {
                        this.OnValueChanged(EventArgs.Empty);
                    }
                    if (!text.Equals(this.Text))
                    {
                        this.OnTextChanged(EventArgs.Empty);
                    }
                }
            }
        }

        [ComVisible(true)]
        public class DateTimePickerAccessibleObject : Control.ControlAccessibleObject
        {
            public DateTimePickerAccessibleObject(DateTimePicker owner) : base(owner)
            {
            }

            public override string KeyboardShortcut
            {
                get
                {
                    Label previousLabel = base.PreviousLabel;
                    if (previousLabel != null)
                    {
                        char mnemonic = WindowsFormsUtils.GetMnemonic(previousLabel.Text, false);
                        if (mnemonic != '\0')
                        {
                            return ("Alt+" + mnemonic);
                        }
                    }
                    string keyboardShortcut = base.KeyboardShortcut;
                    if ((keyboardShortcut == null) || (keyboardShortcut.Length == 0))
                    {
                        char ch2 = WindowsFormsUtils.GetMnemonic(base.Owner.Text, false);
                        if (ch2 != '\0')
                        {
                            return ("Alt+" + ch2);
                        }
                    }
                    return keyboardShortcut;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    AccessibleRole accessibleRole = base.Owner.AccessibleRole;
                    if (accessibleRole != AccessibleRole.Default)
                    {
                        return accessibleRole;
                    }
                    return AccessibleRole.DropList;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates state = base.State;
                    if (((DateTimePicker) base.Owner).ShowCheckBox && ((DateTimePicker) base.Owner).Checked)
                    {
                        state |= AccessibleStates.Checked;
                    }
                    return state;
                }
            }

            public override string Value
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    string str = base.Value;
                    if ((str != null) && (str.Length != 0))
                    {
                        return str;
                    }
                    return base.Owner.Text;
                }
            }
        }

        private sealed class EnumChildren
        {
            public IntPtr hwndFound = IntPtr.Zero;

            public bool enumChildren(IntPtr hwnd, IntPtr lparam)
            {
                this.hwndFound = hwnd;
                return true;
            }
        }
    }
}

