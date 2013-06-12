namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [ToolboxItemFilter("System.Windows.Forms"), DefaultProperty("Interval"), DefaultEvent("Tick"), System.Windows.Forms.SRDescription("DescriptionTimer")]
    public class Timer : Component
    {
        private bool enabled;
        private int interval;
        private object syncObj;
        private GCHandle timerRoot;
        private TimerNativeWindow timerWindow;
        private object userData;

        [System.Windows.Forms.SRDescription("TimerTimerDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler Tick;

        public Timer()
        {
            this.syncObj = new object();
            this.interval = 100;
        }

        public Timer(IContainer container) : this()
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            container.Add(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.timerWindow != null)
                {
                    this.timerWindow.StopTimer();
                }
                this.Enabled = false;
            }
            this.timerWindow = null;
            base.Dispose(disposing);
        }

        protected virtual void OnTick(EventArgs e)
        {
            if (this.onTimer != null)
            {
                this.onTimer(this, e);
            }
        }

        public void Start()
        {
            this.Enabled = true;
        }

        public void Stop()
        {
            this.Enabled = false;
        }

        public override string ToString()
        {
            return (base.ToString() + ", Interval: " + this.Interval.ToString(CultureInfo.CurrentCulture));
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TimerEnabledDescr"), DefaultValue(false)]
        public virtual bool Enabled
        {
            get
            {
                if (this.timerWindow == null)
                {
                    return this.enabled;
                }
                return this.timerWindow.IsTimerRunning;
            }
            set
            {
                lock (this.syncObj)
                {
                    if (this.enabled != value)
                    {
                        this.enabled = value;
                        if (!base.DesignMode)
                        {
                            if (value)
                            {
                                if (this.timerWindow == null)
                                {
                                    this.timerWindow = new TimerNativeWindow(this);
                                }
                                this.timerRoot = GCHandle.Alloc(this);
                                this.timerWindow.StartTimer(this.interval);
                            }
                            else
                            {
                                if (this.timerWindow != null)
                                {
                                    this.timerWindow.StopTimer();
                                }
                                if (this.timerRoot.IsAllocated)
                                {
                                    this.timerRoot.Free();
                                }
                            }
                        }
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("TimerIntervalDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(100)]
        public int Interval
        {
            get
            {
                return this.interval;
            }
            set
            {
                lock (this.syncObj)
                {
                    if (value < 1)
                    {
                        object[] args = new object[] { value, 0.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("Interval", System.Windows.Forms.SR.GetString("TimerInvalidInterval", args));
                    }
                    if (this.interval != value)
                    {
                        this.interval = value;
                        if ((this.Enabled && !base.DesignMode) && (this.timerWindow != null))
                        {
                            this.timerWindow.RestartTimer(value);
                        }
                    }
                }
            }
        }

        [Localizable(false), System.Windows.Forms.SRCategory("CatData"), TypeConverter(typeof(StringConverter)), Bindable(true), System.Windows.Forms.SRDescription("ControlTagDescr"), DefaultValue((string) null)]
        public object Tag
        {
            get
            {
                return this.userData;
            }
            set
            {
                this.userData = value;
            }
        }

        private class TimerNativeWindow : NativeWindow
        {
            private Timer _owner;
            private bool _stoppingTimer;
            private int _timerID;
            private static int TimerID = 1;

            internal TimerNativeWindow(Timer owner)
            {
                this._owner = owner;
            }

            public override void DestroyHandle()
            {
                this.StopTimer(false, IntPtr.Zero);
                base.DestroyHandle();
            }

            private bool EnsureHandle()
            {
                if (base.Handle == IntPtr.Zero)
                {
                    CreateParams cp = new CreateParams {
                        Style = 0,
                        ExStyle = 0,
                        ClassStyle = 0,
                        Caption = base.GetType().Name
                    };
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        cp.Parent = (IntPtr) System.Windows.Forms.NativeMethods.HWND_MESSAGE;
                    }
                    this.CreateHandle(cp);
                }
                return (base.Handle != IntPtr.Zero);
            }

            ~TimerNativeWindow()
            {
                this.StopTimer();
            }

            private bool GetInvokeRequired(IntPtr hWnd)
            {
                if (hWnd != IntPtr.Zero)
                {
                    int num;
                    int windowThreadProcessId = SafeNativeMethods.GetWindowThreadProcessId(new HandleRef(this, hWnd), out num);
                    int currentThreadId = SafeNativeMethods.GetCurrentThreadId();
                    return (windowThreadProcessId != currentThreadId);
                }
                return false;
            }

            protected override void OnThreadException(Exception e)
            {
                Application.OnThreadException(e);
            }

            public override void ReleaseHandle()
            {
                this.StopTimer(false, IntPtr.Zero);
                base.ReleaseHandle();
            }

            public void RestartTimer(int newInterval)
            {
                this.StopTimer(false, IntPtr.Zero);
                this.StartTimer(newInterval);
            }

            public void StartTimer(int interval)
            {
                if (((this._timerID == 0) && !this._stoppingTimer) && this.EnsureHandle())
                {
                    this._timerID = (int) SafeNativeMethods.SetTimer(new HandleRef(this, base.Handle), TimerID++, interval, IntPtr.Zero);
                }
            }

            public void StopTimer()
            {
                this.StopTimer(true, IntPtr.Zero);
            }

            public void StopTimer(bool destroyHwnd, IntPtr hWnd)
            {
                if (hWnd == IntPtr.Zero)
                {
                    hWnd = base.Handle;
                }
                if (this.GetInvokeRequired(hWnd))
                {
                    UnsafeNativeMethods.PostMessage(new HandleRef(this, hWnd), 0x10, 0, 0);
                }
                else
                {
                    lock (this)
                    {
                        if ((!this._stoppingTimer && (hWnd != IntPtr.Zero)) && UnsafeNativeMethods.IsWindow(new HandleRef(this, hWnd)))
                        {
                            if (this._timerID != 0)
                            {
                                try
                                {
                                    this._stoppingTimer = true;
                                    SafeNativeMethods.KillTimer(new HandleRef(this, hWnd), this._timerID);
                                }
                                finally
                                {
                                    this._timerID = 0;
                                    this._stoppingTimer = false;
                                }
                            }
                            if (destroyHwnd)
                            {
                                base.DestroyHandle();
                            }
                        }
                    }
                }
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x113)
                {
                    if (((int) ((long) m.WParam)) == this._timerID)
                    {
                        this._owner.OnTick(EventArgs.Empty);
                        return;
                    }
                }
                else if (m.Msg == 0x10)
                {
                    this.StopTimer(true, m.HWnd);
                    return;
                }
                base.WndProc(ref m);
            }

            public bool IsTimerRunning
            {
                get
                {
                    return ((this._timerID != 0) && (base.Handle != IntPtr.Zero));
                }
            }
        }
    }
}

