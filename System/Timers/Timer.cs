namespace System.Timers
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [DefaultEvent("Elapsed"), DefaultProperty("Interval"), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class Timer : Component, ISupportInitialize
    {
        private bool autoReset;
        private TimerCallback callback;
        private object cookie;
        private bool delayedEnable;
        private bool disposed;
        private bool enabled;
        private bool initializing;
        private double interval;
        private ISynchronizeInvoke synchronizingObject;
        private System.Threading.Timer timer;

        [Category("Behavior"), TimersDescription("TimerIntervalElapsed")]
        public event ElapsedEventHandler Elapsed;

        public Timer()
        {
            this.interval = 100.0;
            this.enabled = false;
            this.autoReset = true;
            this.initializing = false;
            this.delayedEnable = false;
            this.callback = new TimerCallback(this.MyTimerCallback);
        }

        public Timer(double interval) : this()
        {
            if (interval <= 0.0)
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "interval", interval }));
            }
            int num = (int) Math.Ceiling(interval);
            if (num < 0)
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "interval", interval }));
            }
            this.interval = interval;
        }

        public void BeginInit()
        {
            this.Close();
            this.initializing = true;
        }

        public void Close()
        {
            this.initializing = false;
            this.delayedEnable = false;
            this.enabled = false;
            if (this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            this.Close();
            this.disposed = true;
            base.Dispose(disposing);
        }

        public void EndInit()
        {
            this.initializing = false;
            this.Enabled = this.delayedEnable;
        }

        [SuppressUnmanagedCodeSecurity, DllImport("kernel32.dll")]
        internal static extern void GetSystemTimeAsFileTime(ref FILE_TIME lpSystemTimeAsFileTime);
        private void MyTimerCallback(object state)
        {
            if (state == this.cookie)
            {
                if (!this.autoReset)
                {
                    this.enabled = false;
                }
                FILE_TIME lpSystemTimeAsFileTime = new FILE_TIME();
                GetSystemTimeAsFileTime(ref lpSystemTimeAsFileTime);
                ElapsedEventArgs e = new ElapsedEventArgs(lpSystemTimeAsFileTime.ftTimeLow, lpSystemTimeAsFileTime.ftTimeHigh);
                try
                {
                    ElapsedEventHandler onIntervalElapsed = this.onIntervalElapsed;
                    if (onIntervalElapsed != null)
                    {
                        if ((this.SynchronizingObject != null) && this.SynchronizingObject.InvokeRequired)
                        {
                            this.SynchronizingObject.BeginInvoke(onIntervalElapsed, new object[] { this, e });
                        }
                        else
                        {
                            onIntervalElapsed(this, e);
                        }
                    }
                }
                catch
                {
                }
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

        private void UpdateTimer()
        {
            int dueTime = (int) Math.Ceiling(this.interval);
            this.timer.Change(dueTime, this.autoReset ? dueTime : -1);
        }

        [Category("Behavior"), TimersDescription("TimerAutoReset"), DefaultValue(true)]
        public bool AutoReset
        {
            get
            {
                return this.autoReset;
            }
            set
            {
                if (base.DesignMode)
                {
                    this.autoReset = value;
                }
                else if (this.autoReset != value)
                {
                    this.autoReset = value;
                    if (this.timer != null)
                    {
                        this.UpdateTimer();
                    }
                }
            }
        }

        [DefaultValue(false), Category("Behavior"), TimersDescription("TimerEnabled")]
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                if (base.DesignMode)
                {
                    this.delayedEnable = value;
                    this.enabled = value;
                }
                else if (this.initializing)
                {
                    this.delayedEnable = value;
                }
                else if (this.enabled != value)
                {
                    if (!value)
                    {
                        if (this.timer != null)
                        {
                            this.cookie = null;
                            this.timer.Dispose();
                            this.timer = null;
                        }
                        this.enabled = value;
                    }
                    else
                    {
                        this.enabled = value;
                        if (this.timer == null)
                        {
                            if (this.disposed)
                            {
                                throw new ObjectDisposedException(base.GetType().Name);
                            }
                            int dueTime = (int) Math.Ceiling(this.interval);
                            this.cookie = new object();
                            this.timer = new System.Threading.Timer(this.callback, this.cookie, dueTime, this.autoReset ? dueTime : -1);
                        }
                        else
                        {
                            this.UpdateTimer();
                        }
                    }
                }
            }
        }

        [DefaultValue((double) 100.0), Category("Behavior"), TimersDescription("TimerInterval"), SettingsBindable(true)]
        public double Interval
        {
            get
            {
                return this.interval;
            }
            set
            {
                if (value <= 0.0)
                {
                    throw new ArgumentException(SR.GetString("TimerInvalidInterval", new object[] { value, 0 }));
                }
                this.interval = value;
                if (this.timer != null)
                {
                    this.UpdateTimer();
                }
            }
        }

        public override ISite Site
        {
            get
            {
                return base.Site;
            }
            set
            {
                base.Site = value;
                if (base.DesignMode)
                {
                    this.enabled = true;
                }
            }
        }

        [Browsable(false), DefaultValue((string) null), TimersDescription("TimerSynchronizingObject")]
        public ISynchronizeInvoke SynchronizingObject
        {
            get
            {
                if ((this.synchronizingObject == null) && base.DesignMode)
                {
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        object rootComponent = service.RootComponent;
                        if ((rootComponent != null) && (rootComponent is ISynchronizeInvoke))
                        {
                            this.synchronizingObject = (ISynchronizeInvoke) rootComponent;
                        }
                    }
                }
                return this.synchronizingObject;
            }
            set
            {
                this.synchronizingObject = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FILE_TIME
        {
            internal int ftTimeLow;
            internal int ftTimeHigh;
        }
    }
}

