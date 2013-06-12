namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class Screen
    {
        private readonly int bitDepth;
        private readonly Rectangle bounds;
        private int currentDesktopChangedCount;
        private static int desktopChangedCount = -1;
        private readonly string deviceName;
        private readonly IntPtr hmonitor;
        private const int MONITOR_DEFAULTTONEAREST = 2;
        private const int MONITOR_DEFAULTTONULL = 0;
        private const int MONITOR_DEFAULTTOPRIMARY = 1;
        private const int MONITORINFOF_PRIMARY = 1;
        private static bool multiMonitorSupport = (System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(80) != 0);
        private readonly bool primary;
        private const int PRIMARY_MONITOR = -1163005939;
        private static Screen[] screens;
        private static object syncLock = new object();
        private Rectangle workingArea;

        internal Screen(IntPtr monitor) : this(monitor, IntPtr.Zero)
        {
        }

        internal Screen(IntPtr monitor, IntPtr hdc)
        {
            this.workingArea = Rectangle.Empty;
            this.currentDesktopChangedCount = -1;
            IntPtr handle = hdc;
            if (!multiMonitorSupport || (monitor == ((IntPtr) (-1163005939))))
            {
                this.bounds = SystemInformation.VirtualScreen;
                this.primary = true;
                this.deviceName = "DISPLAY";
            }
            else
            {
                System.Windows.Forms.NativeMethods.MONITORINFOEX info = new System.Windows.Forms.NativeMethods.MONITORINFOEX();
                System.Windows.Forms.SafeNativeMethods.GetMonitorInfo(new HandleRef(null, monitor), info);
                this.bounds = Rectangle.FromLTRB(info.rcMonitor.left, info.rcMonitor.top, info.rcMonitor.right, info.rcMonitor.bottom);
                this.primary = (info.dwFlags & 1) != 0;
                for (int i = info.szDevice.Length; (i > 0) && (info.szDevice[i - 1] == '\0'); i--)
                {
                }
                this.deviceName = new string(info.szDevice);
                this.deviceName = this.deviceName.TrimEnd(new char[1]);
                if (hdc == IntPtr.Zero)
                {
                    handle = System.Windows.Forms.UnsafeNativeMethods.CreateDC(this.deviceName);
                }
            }
            this.hmonitor = monitor;
            this.bitDepth = System.Windows.Forms.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, handle), 12);
            this.bitDepth *= System.Windows.Forms.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, handle), 14);
            if (hdc != handle)
            {
                System.Windows.Forms.UnsafeNativeMethods.DeleteDC(new HandleRef(null, handle));
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Screen)
            {
                Screen screen = (Screen) obj;
                if (this.hmonitor == screen.hmonitor)
                {
                    return true;
                }
            }
            return false;
        }

        public static Screen FromControl(Control control)
        {
            return FromHandleInternal(control.Handle);
        }

        public static Screen FromHandle(IntPtr hwnd)
        {
            System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Demand();
            return FromHandleInternal(hwnd);
        }

        internal static Screen FromHandleInternal(IntPtr hwnd)
        {
            if (multiMonitorSupport)
            {
                return new Screen(System.Windows.Forms.SafeNativeMethods.MonitorFromWindow(new HandleRef(null, hwnd), 2));
            }
            return new Screen((IntPtr) (-1163005939), IntPtr.Zero);
        }

        public static Screen FromPoint(Point point)
        {
            if (multiMonitorSupport)
            {
                System.Windows.Forms.NativeMethods.POINTSTRUCT pt = new System.Windows.Forms.NativeMethods.POINTSTRUCT(point.X, point.Y);
                return new Screen(System.Windows.Forms.SafeNativeMethods.MonitorFromPoint(pt, 2));
            }
            return new Screen((IntPtr) (-1163005939));
        }

        public static Screen FromRectangle(Rectangle rect)
        {
            if (multiMonitorSupport)
            {
                return new Screen(System.Windows.Forms.SafeNativeMethods.MonitorFromRect(ref System.Windows.Forms.NativeMethods.RECT.FromXYWH(rect.X, rect.Y, rect.Width, rect.Height), 2));
            }
            return new Screen((IntPtr) (-1163005939), IntPtr.Zero);
        }

        public static Rectangle GetBounds(Point pt)
        {
            return FromPoint(pt).Bounds;
        }

        public static Rectangle GetBounds(Rectangle rect)
        {
            return FromRectangle(rect).Bounds;
        }

        public static Rectangle GetBounds(Control ctl)
        {
            return FromControl(ctl).Bounds;
        }

        public override int GetHashCode()
        {
            return (int) this.hmonitor;
        }

        public static Rectangle GetWorkingArea(Point pt)
        {
            return FromPoint(pt).WorkingArea;
        }

        public static Rectangle GetWorkingArea(Rectangle rect)
        {
            return FromRectangle(rect).WorkingArea;
        }

        public static Rectangle GetWorkingArea(Control ctl)
        {
            return FromControl(ctl).WorkingArea;
        }

        private static void OnDisplaySettingsChanging(object sender, EventArgs e)
        {
            SystemEvents.DisplaySettingsChanging -= new EventHandler(Screen.OnDisplaySettingsChanging);
            screens = null;
        }

        private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Desktop)
            {
                Interlocked.Increment(ref desktopChangedCount);
            }
        }

        public override string ToString()
        {
            return (base.GetType().Name + "[Bounds=" + this.bounds.ToString() + " WorkingArea=" + this.WorkingArea.ToString() + " Primary=" + this.primary.ToString() + " DeviceName=" + this.deviceName);
        }

        public static Screen[] AllScreens
        {
            get
            {
                if (screens == null)
                {
                    if (multiMonitorSupport)
                    {
                        MonitorEnumCallback callback = new MonitorEnumCallback();
                        System.Windows.Forms.NativeMethods.MonitorEnumProc lpfnEnum = new System.Windows.Forms.NativeMethods.MonitorEnumProc(callback.Callback);
                        System.Windows.Forms.SafeNativeMethods.EnumDisplayMonitors(System.Windows.Forms.NativeMethods.NullHandleRef, null, lpfnEnum, IntPtr.Zero);
                        if (callback.screens.Count > 0)
                        {
                            Screen[] array = new Screen[callback.screens.Count];
                            callback.screens.CopyTo(array, 0);
                            screens = array;
                        }
                        else
                        {
                            screens = new Screen[] { new Screen((IntPtr) (-1163005939)) };
                        }
                    }
                    else
                    {
                        screens = new Screen[] { PrimaryScreen };
                    }
                    SystemEvents.DisplaySettingsChanging += new EventHandler(Screen.OnDisplaySettingsChanging);
                }
                return screens;
            }
        }

        public int BitsPerPixel
        {
            get
            {
                return this.bitDepth;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        private static int DesktopChangedCount
        {
            get
            {
                if (desktopChangedCount == -1)
                {
                    lock (syncLock)
                    {
                        if (desktopChangedCount == -1)
                        {
                            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(Screen.OnUserPreferenceChanged);
                            desktopChangedCount = 0;
                        }
                    }
                }
                return desktopChangedCount;
            }
        }

        public string DeviceName
        {
            get
            {
                return this.deviceName;
            }
        }

        public bool Primary
        {
            get
            {
                return this.primary;
            }
        }

        public static Screen PrimaryScreen
        {
            get
            {
                if (!multiMonitorSupport)
                {
                    return new Screen((IntPtr) (-1163005939), IntPtr.Zero);
                }
                Screen[] allScreens = AllScreens;
                for (int i = 0; i < allScreens.Length; i++)
                {
                    if (allScreens[i].primary)
                    {
                        return allScreens[i];
                    }
                }
                return null;
            }
        }

        public Rectangle WorkingArea
        {
            get
            {
                if (this.currentDesktopChangedCount != DesktopChangedCount)
                {
                    Interlocked.Exchange(ref this.currentDesktopChangedCount, DesktopChangedCount);
                    if (!multiMonitorSupport || (this.hmonitor == ((IntPtr) (-1163005939))))
                    {
                        this.workingArea = SystemInformation.WorkingArea;
                    }
                    else
                    {
                        System.Windows.Forms.NativeMethods.MONITORINFOEX info = new System.Windows.Forms.NativeMethods.MONITORINFOEX();
                        System.Windows.Forms.SafeNativeMethods.GetMonitorInfo(new HandleRef(null, this.hmonitor), info);
                        this.workingArea = Rectangle.FromLTRB(info.rcWork.left, info.rcWork.top, info.rcWork.right, info.rcWork.bottom);
                    }
                }
                return this.workingArea;
            }
        }

        private class MonitorEnumCallback
        {
            public ArrayList screens = new ArrayList();

            public virtual bool Callback(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lparam)
            {
                this.screens.Add(new Screen(monitor, hdc));
                return true;
            }
        }
    }
}

