namespace Microsoft.Win32
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class SystemEvents
    {
        private static Dictionary<object, List<SystemEventInvokeInfo>> _handlers;
        private static object appFileVersion;
        private static bool checkedThreadAffinity = false;
        private static string className = null;
        private Microsoft.Win32.NativeMethods.ConHndlr consoleHandler;
        private static IntPtr defWindowProc;
        private static int domainQualifier;
        private static readonly object eventLockObject = new object();
        private static ManualResetEvent eventThreadTerminated;
        private static ManualResetEvent eventWindowReady;
        private const string everettThreadAffinityValue = "EnableSystemEventsThreadAffinityCompatibility";
        private static string executablePath = null;
        private static bool isUserInteractive = false;
        private static Type mainType;
        private static readonly object OnDisplaySettingsChangedEvent = new object();
        private static readonly object OnDisplaySettingsChangingEvent = new object();
        private static readonly object OnEventsThreadShutdownEvent = new object();
        private static readonly object OnInstalledFontsChangedEvent = new object();
        private static readonly object OnLowMemoryEvent = new object();
        private static readonly object OnPaletteChangedEvent = new object();
        private static readonly object OnPowerModeChangedEvent = new object();
        private static readonly object OnSessionEndedEvent = new object();
        private static readonly object OnSessionEndingEvent = new object();
        private static readonly object OnSessionSwitchEvent = new object();
        private static readonly object OnTimeChangedEvent = new object();
        private static readonly object OnTimerElapsedEvent = new object();
        private static readonly object OnUserPreferenceChangedEvent = new object();
        private static readonly object OnUserPreferenceChangingEvent = new object();
        private static IntPtr processWinStation = IntPtr.Zero;
        private static readonly object procLockObject = new object();
        private static Random randomTimerId = new Random();
        private static bool registeredSessionNotification = false;
        private static bool startupRecreates;
        private static Microsoft.Win32.NativeMethods.WNDCLASS staticwndclass;
        private static SystemEvents systemEvents;
        private static Queue threadCallbackList;
        private static int threadCallbackMessage = 0;
        private static bool useEverettThreadAffinity = false;
        private IntPtr windowHandle;
        private Microsoft.Win32.NativeMethods.WndProc windowProc;
        private static Thread windowThread;

        public static  event EventHandler DisplaySettingsChanged
        {
            add
            {
                AddEventHandler(OnDisplaySettingsChangedEvent, value);
            }
            remove
            {
                RemoveEventHandler(OnDisplaySettingsChangedEvent, value);
            }
        }

        public static  event EventHandler DisplaySettingsChanging
        {
            add
            {
                AddEventHandler(OnDisplaySettingsChangingEvent, value);
            }
            remove
            {
                RemoveEventHandler(OnDisplaySettingsChangingEvent, value);
            }
        }

        public static  event EventHandler EventsThreadShutdown
        {
            add
            {
                AddEventHandler(OnEventsThreadShutdownEvent, value);
            }
            remove
            {
                RemoveEventHandler(OnEventsThreadShutdownEvent, value);
            }
        }

        public static  event EventHandler InstalledFontsChanged
        {
            add
            {
                AddEventHandler(OnInstalledFontsChangedEvent, value);
            }
            remove
            {
                RemoveEventHandler(OnInstalledFontsChangedEvent, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("This event has been deprecated. http://go.microsoft.com/fwlink/?linkid=14202"), Browsable(false)]
        public static  event EventHandler LowMemory
        {
            add
            {
                EnsureSystemEvents(true, true);
                AddEventHandler(OnLowMemoryEvent, value);
            }
            remove
            {
                RemoveEventHandler(OnLowMemoryEvent, value);
            }
        }

        public static  event EventHandler PaletteChanged
        {
            add
            {
                AddEventHandler(OnPaletteChangedEvent, value);
            }
            remove
            {
                RemoveEventHandler(OnPaletteChangedEvent, value);
            }
        }

        public static  event PowerModeChangedEventHandler PowerModeChanged
        {
            add
            {
                EnsureSystemEvents(true, true);
                AddEventHandler(OnPowerModeChangedEvent, value);
            }
            remove
            {
                RemoveEventHandler(OnPowerModeChangedEvent, value);
            }
        }

        public static  event SessionEndedEventHandler SessionEnded
        {
            add
            {
                EnsureSystemEvents(true, false);
                AddEventHandler(OnSessionEndedEvent, value);
            }
            remove
            {
                RemoveEventHandler(OnSessionEndedEvent, value);
            }
        }

        public static  event SessionEndingEventHandler SessionEnding
        {
            add
            {
                EnsureSystemEvents(true, false);
                AddEventHandler(OnSessionEndingEvent, value);
            }
            remove
            {
                RemoveEventHandler(OnSessionEndingEvent, value);
            }
        }

        public static  event SessionSwitchEventHandler SessionSwitch
        {
            add
            {
                EnsureSystemEvents(true, true);
                EnsureRegisteredSessionNotification();
                AddEventHandler(OnSessionSwitchEvent, value);
            }
            remove
            {
                RemoveEventHandler(OnSessionSwitchEvent, value);
            }
        }

        public static  event EventHandler TimeChanged
        {
            add
            {
                EnsureSystemEvents(true, false);
                AddEventHandler(OnTimeChangedEvent, value);
            }
            remove
            {
                RemoveEventHandler(OnTimeChangedEvent, value);
            }
        }

        public static  event TimerElapsedEventHandler TimerElapsed
        {
            add
            {
                EnsureSystemEvents(true, false);
                AddEventHandler(OnTimerElapsedEvent, value);
            }
            remove
            {
                RemoveEventHandler(OnTimerElapsedEvent, value);
            }
        }

        public static  event UserPreferenceChangedEventHandler UserPreferenceChanged
        {
            add
            {
                AddEventHandler(OnUserPreferenceChangedEvent, value);
            }
            remove
            {
                RemoveEventHandler(OnUserPreferenceChangedEvent, value);
            }
        }

        public static  event UserPreferenceChangingEventHandler UserPreferenceChanging
        {
            add
            {
                AddEventHandler(OnUserPreferenceChangingEvent, value);
            }
            remove
            {
                RemoveEventHandler(OnUserPreferenceChangingEvent, value);
            }
        }

        private SystemEvents()
        {
        }

        private static void AddEventHandler(object key, Delegate value)
        {
            lock (eventLockObject)
            {
                List<SystemEventInvokeInfo> list;
                if (_handlers == null)
                {
                    _handlers = new Dictionary<object, List<SystemEventInvokeInfo>>();
                    EnsureSystemEvents(false, false);
                }
                if (!_handlers.TryGetValue(key, out list))
                {
                    list = new List<SystemEventInvokeInfo>();
                    _handlers[key] = list;
                }
                else
                {
                    list = _handlers[key];
                }
                list.Add(new SystemEventInvokeInfo(value));
            }
        }

        private void BumpQualifier()
        {
            staticwndclass = null;
            domainQualifier++;
        }

        private int ConsoleHandlerProc(int signalType)
        {
            switch (signalType)
            {
                case 5:
                    this.OnSessionEnded((IntPtr) 1, (IntPtr) (-2147483648));
                    break;

                case 6:
                    this.OnSessionEnded((IntPtr) 1, IntPtr.Zero);
                    break;
            }
            return 0;
        }

        private IntPtr CreateBroadcastWindow()
        {
            Microsoft.Win32.NativeMethods.WNDCLASS_I wc = new Microsoft.Win32.NativeMethods.WNDCLASS_I();
            IntPtr moduleHandle = Microsoft.Win32.UnsafeNativeMethods.GetModuleHandle(null);
            if (!Microsoft.Win32.UnsafeNativeMethods.GetClassInfo(new HandleRef(this, moduleHandle), this.WndClass.lpszClassName, wc))
            {
                if (Microsoft.Win32.UnsafeNativeMethods.RegisterClass(this.WndClass) == 0)
                {
                    this.windowProc = null;
                    return IntPtr.Zero;
                }
            }
            else if (wc.lpfnWndProc == this.DefWndProc)
            {
                short num = 0;
                if (Microsoft.Win32.UnsafeNativeMethods.UnregisterClass(this.WndClass.lpszClassName, new HandleRef(null, Microsoft.Win32.UnsafeNativeMethods.GetModuleHandle(null))) != 0)
                {
                    num = Microsoft.Win32.UnsafeNativeMethods.RegisterClass(this.WndClass);
                }
                if (num == 0)
                {
                    do
                    {
                        this.BumpQualifier();
                    }
                    while ((Microsoft.Win32.UnsafeNativeMethods.RegisterClass(this.WndClass) == 0) && (Marshal.GetLastWin32Error() == 0x582));
                }
            }
            return Microsoft.Win32.UnsafeNativeMethods.CreateWindowEx(0, this.WndClass.lpszClassName, this.WndClass.lpszClassName, -2147483648, 0, 0, 0, 0, Microsoft.Win32.NativeMethods.NullHandleRef, Microsoft.Win32.NativeMethods.NullHandleRef, new HandleRef(this, moduleHandle), null);
        }

        public static IntPtr CreateTimer(int interval)
        {
            if (interval <= 0)
            {
                throw new ArgumentException(SR.GetString("InvalidLowBoundArgument", new object[] { "interval", interval.ToString(Thread.CurrentThread.CurrentCulture), "0" }));
            }
            EnsureSystemEvents(true, true);
            IntPtr ptr = Microsoft.Win32.UnsafeNativeMethods.SendMessage(new HandleRef(systemEvents, systemEvents.windowHandle), 0x401, (IntPtr) interval, IntPtr.Zero);
            if (ptr == IntPtr.Zero)
            {
                throw new ExternalException(SR.GetString("ErrorCreateTimer"));
            }
            return ptr;
        }

        private void Dispose()
        {
            if (this.windowHandle != IntPtr.Zero)
            {
                if (registeredSessionNotification)
                {
                    Microsoft.Win32.UnsafeNativeMethods.WTSUnRegisterSessionNotification(new HandleRef(systemEvents, systemEvents.windowHandle));
                }
                IntPtr windowHandle = this.windowHandle;
                this.windowHandle = IntPtr.Zero;
                HandleRef hWnd = new HandleRef(this, windowHandle);
                if (Microsoft.Win32.UnsafeNativeMethods.IsWindow(hWnd) && (this.DefWndProc != IntPtr.Zero))
                {
                    Microsoft.Win32.UnsafeNativeMethods.SetWindowLong(hWnd, -4, new HandleRef(this, this.DefWndProc));
                    Microsoft.Win32.UnsafeNativeMethods.SetClassLong(hWnd, -24, this.DefWndProc);
                }
                if (Microsoft.Win32.UnsafeNativeMethods.IsWindow(hWnd) && !Microsoft.Win32.UnsafeNativeMethods.DestroyWindow(hWnd))
                {
                    Microsoft.Win32.UnsafeNativeMethods.PostMessage(hWnd, 0x10, IntPtr.Zero, IntPtr.Zero);
                }
                else
                {
                    IntPtr moduleHandle = Microsoft.Win32.UnsafeNativeMethods.GetModuleHandle(null);
                    Microsoft.Win32.UnsafeNativeMethods.UnregisterClass(className, new HandleRef(this, moduleHandle));
                }
            }
            if (this.consoleHandler != null)
            {
                Microsoft.Win32.UnsafeNativeMethods.SetConsoleCtrlHandler(this.consoleHandler, 0);
                this.consoleHandler = null;
            }
        }

        private static void EnsureRegisteredSessionNotification()
        {
            if (!registeredSessionNotification)
            {
                IntPtr handle = Microsoft.Win32.SafeNativeMethods.LoadLibrary("wtsapi32.dll");
                if (handle != IntPtr.Zero)
                {
                    Microsoft.Win32.UnsafeNativeMethods.WTSRegisterSessionNotification(new HandleRef(systemEvents, systemEvents.windowHandle), 0);
                    registeredSessionNotification = true;
                    Microsoft.Win32.SafeNativeMethods.FreeLibrary(new HandleRef(null, handle));
                }
            }
        }

        private static void EnsureSystemEvents(bool requireHandle, bool throwOnRefusal)
        {
            if (systemEvents == null)
            {
                lock (procLockObject)
                {
                    if (systemEvents == null)
                    {
                        if (Thread.GetDomain().GetData(".appDomain") != null)
                        {
                            if (throwOnRefusal)
                            {
                                throw new InvalidOperationException(SR.GetString("ErrorSystemEventsNotSupported"));
                            }
                        }
                        else
                        {
                            if (!UserInteractive || (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA))
                            {
                                systemEvents = new SystemEvents();
                                systemEvents.Initialize();
                            }
                            else
                            {
                                eventWindowReady = new ManualResetEvent(false);
                                systemEvents = new SystemEvents();
                                windowThread = new Thread(new ThreadStart(systemEvents.WindowThreadProc));
                                windowThread.IsBackground = true;
                                windowThread.Name = ".NET SystemEvents";
                                windowThread.Start();
                                eventWindowReady.WaitOne();
                            }
                            if (requireHandle && (systemEvents.windowHandle == IntPtr.Zero))
                            {
                                throw new ExternalException(SR.GetString("ErrorCreateSystemEvents"));
                            }
                            startupRecreates = false;
                        }
                    }
                }
            }
        }

        private static FileVersionInfo GetAppFileVersionInfo()
        {
            if (appFileVersion == null)
            {
                Type appMainType = GetAppMainType();
                if (appMainType != null)
                {
                    new FileIOPermission(PermissionState.None) { AllFiles = FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read }.Assert();
                    try
                    {
                        appFileVersion = FileVersionInfo.GetVersionInfo(appMainType.Module.FullyQualifiedName);
                        goto Label_0057;
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
                appFileVersion = FileVersionInfo.GetVersionInfo(ExecutablePath);
            }
        Label_0057:
            return (FileVersionInfo) appFileVersion;
        }

        private static Type GetAppMainType()
        {
            if (mainType == null)
            {
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    mainType = entryAssembly.EntryPoint.ReflectedType;
                }
            }
            return mainType;
        }

        private UserPreferenceCategory GetUserPreferenceCategory(int msg, IntPtr wParam, IntPtr lParam)
        {
            UserPreferenceCategory general = UserPreferenceCategory.General;
            if (msg != 0x1a)
            {
                if (msg == 0x15)
                {
                    general = UserPreferenceCategory.Color;
                }
                return general;
            }
            if ((lParam != IntPtr.Zero) && Marshal.PtrToStringAuto(lParam).Equals("Policy"))
            {
                return UserPreferenceCategory.Policy;
            }
            if ((lParam != IntPtr.Zero) && Marshal.PtrToStringAuto(lParam).Equals("intl"))
            {
                return UserPreferenceCategory.Locale;
            }
            int num = (int) wParam;
            if (num <= 0x71)
            {
                switch (num)
                {
                    case 4:
                    case 0x1d:
                    case 30:
                    case 0x20:
                    case 0x21:
                    case 0x5d:
                    case 0x60:
                    case 0x65:
                    case 0x67:
                    case 0x69:
                    case 0x71:
                        goto Label_02E6;

                    case 5:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 12:
                    case 14:
                    case 0x10:
                    case 0x12:
                    case 0x16:
                    case 0x19:
                    case 0x1b:
                    case 0x1f:
                    case 0x23:
                    case 0x24:
                    case 0x26:
                    case 0x27:
                    case 40:
                    case 0x29:
                    case 0x2b:
                    case 0x2d:
                    case 0x30:
                    case 0x31:
                    case 50:
                    case 0x34:
                    case 0x36:
                    case 0x38:
                    case 0x3a:
                    case 60:
                    case 0x3e:
                    case 0x40:
                    case 0x42:
                    case 0x44:
                    case 70:
                    case 0x48:
                    case 0x4a:
                    case 0x4e:
                    case 0x4f:
                    case 80:
                    case 0x53:
                    case 0x54:
                    case 0x59:
                    case 90:
                    case 0x5c:
                    case 0x5e:
                    case 0x5f:
                        return general;

                    case 6:
                    case 0x25:
                    case 0x2a:
                    case 0x2c:
                    case 0x49:
                    case 0x4c:
                    case 0x4d:
                    case 0x6f:
                        goto Label_02FC;

                    case 11:
                    case 0x17:
                    case 0x45:
                    case 0x5b:
                        return UserPreferenceCategory.Keyboard;

                    case 13:
                    case 0x18:
                    case 0x1a:
                    case 0x22:
                    case 0x2e:
                    case 0x58:
                        return UserPreferenceCategory.Icon;

                    case 15:
                    case 0x11:
                    case 0x61:
                        return UserPreferenceCategory.Screensaver;

                    case 0x13:
                    case 20:
                    case 0x15:
                    case 0x2f:
                    case 0x4b:
                    case 0x57:
                        return UserPreferenceCategory.Desktop;

                    case 0x1c:
                    case 0x6b:
                        goto Label_02EE;

                    case 0x33:
                    case 0x35:
                    case 0x37:
                    case 0x39:
                    case 0x3b:
                    case 0x3d:
                    case 0x3f:
                    case 0x41:
                    case 0x43:
                    case 0x47:
                        return UserPreferenceCategory.Accessibility;

                    case 0x51:
                    case 0x52:
                    case 0x55:
                    case 0x56:
                        return UserPreferenceCategory.Power;

                    case 0x66:
                    case 0x68:
                    case 0x6a:
                        return general;

                    case 0x70:
                        return general;
                }
                return general;
            }
            if (num <= 0x101b)
            {
                switch (num)
                {
                    case 0x1001:
                    case 0x1005:
                    case 0x1007:
                    case 0x1009:
                    case 0x100b:
                    case 0x100d:
                        goto Label_02FC;

                    case 0x1002:
                    case 0x1004:
                    case 0x1006:
                    case 0x1008:
                    case 0x100a:
                    case 0x100c:
                    case 0x100e:
                        return general;

                    case 0x1003:
                    case 0x1013:
                    case 0x1015:
                        goto Label_02EE;

                    case 0x100f:
                    case 0x1017:
                    case 0x1019:
                    case 0x101b:
                        goto Label_02E6;

                    case 0x1014:
                    case 0x1016:
                    case 0x1018:
                    case 0x101a:
                        return general;
                }
                return general;
            }
            switch (num)
            {
                case 0x2001:
                case 0x2003:
                case 0x2005:
                case 0x2007:
                case 0x103f:
                    goto Label_02FC;

                case 0x2002:
                case 0x2004:
                case 0x2006:
                    return general;

                default:
                    return general;
            }
        Label_02E6:
            return UserPreferenceCategory.Mouse;
        Label_02EE:
            return UserPreferenceCategory.Menu;
        Label_02FC:
            return UserPreferenceCategory.Window;
        }

        private void Initialize()
        {
            this.consoleHandler = new Microsoft.Win32.NativeMethods.ConHndlr(this.ConsoleHandlerProc);
            if (!Microsoft.Win32.UnsafeNativeMethods.SetConsoleCtrlHandler(this.consoleHandler, 1))
            {
                this.consoleHandler = null;
            }
            this.windowHandle = this.CreateBroadcastWindow();
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(SystemEvents.Shutdown);
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(SystemEvents.Shutdown);
        }

        private void InvokeMarshaledCallbacks()
        {
            Delegate delegate2 = null;
            lock (threadCallbackList)
            {
                if (threadCallbackList.Count > 0)
                {
                    delegate2 = (Delegate) threadCallbackList.Dequeue();
                }
                goto Label_00A6;
            }
        Label_003D:
            try
            {
                EventHandler handler = delegate2 as EventHandler;
                if (handler != null)
                {
                    handler(null, EventArgs.Empty);
                }
                else
                {
                    delegate2.DynamicInvoke(new object[0]);
                }
            }
            catch (Exception)
            {
            }
            lock (threadCallbackList)
            {
                if (threadCallbackList.Count > 0)
                {
                    delegate2 = (Delegate) threadCallbackList.Dequeue();
                }
                else
                {
                    delegate2 = null;
                }
            }
        Label_00A6:
            if (delegate2 != null)
            {
                goto Label_003D;
            }
        }

        public static void InvokeOnEventsThread(Delegate method)
        {
            EnsureSystemEvents(true, true);
            if (threadCallbackList == null)
            {
                lock (eventLockObject)
                {
                    if (threadCallbackList == null)
                    {
                        threadCallbackList = new Queue();
                        threadCallbackMessage = Microsoft.Win32.SafeNativeMethods.RegisterWindowMessage("SystemEventsThreadCallbackMessage");
                    }
                }
            }
            lock (threadCallbackList)
            {
                threadCallbackList.Enqueue(method);
            }
            Microsoft.Win32.UnsafeNativeMethods.PostMessage(new HandleRef(systemEvents, systemEvents.windowHandle), threadCallbackMessage, IntPtr.Zero, IntPtr.Zero);
        }

        public static void KillTimer(IntPtr timerId)
        {
            EnsureSystemEvents(true, true);
            if ((systemEvents.windowHandle != IntPtr.Zero) && (((int) Microsoft.Win32.UnsafeNativeMethods.SendMessage(new HandleRef(systemEvents, systemEvents.windowHandle), 0x402, timerId, IntPtr.Zero)) == 0))
            {
                throw new ExternalException(SR.GetString("ErrorKillTimer"));
            }
        }

        private IntPtr OnCreateTimer(IntPtr wParam)
        {
            IntPtr handle = (IntPtr) randomTimerId.Next();
            if (!(Microsoft.Win32.UnsafeNativeMethods.SetTimer(new HandleRef(this, this.windowHandle), new HandleRef(this, handle), (int) wParam, Microsoft.Win32.NativeMethods.NullHandleRef) == IntPtr.Zero))
            {
                return handle;
            }
            return IntPtr.Zero;
        }

        private void OnDisplaySettingsChanged()
        {
            RaiseEvent(OnDisplaySettingsChangedEvent, new object[] { this, EventArgs.Empty });
        }

        private void OnDisplaySettingsChanging()
        {
            RaiseEvent(OnDisplaySettingsChangingEvent, new object[] { this, EventArgs.Empty });
        }

        private void OnGenericEvent(object eventKey)
        {
            RaiseEvent(eventKey, new object[] { this, EventArgs.Empty });
        }

        private bool OnKillTimer(IntPtr wParam)
        {
            return Microsoft.Win32.UnsafeNativeMethods.KillTimer(new HandleRef(this, this.windowHandle), new HandleRef(this, wParam));
        }

        private void OnPowerModeChanged(IntPtr wParam)
        {
            PowerModes suspend;
            switch (((int) wParam))
            {
                case 4:
                case 5:
                    suspend = PowerModes.Suspend;
                    break;

                case 6:
                case 7:
                case 8:
                    suspend = PowerModes.Resume;
                    break;

                case 9:
                case 10:
                case 11:
                    suspend = PowerModes.StatusChange;
                    break;

                default:
                    return;
            }
            RaiseEvent(OnPowerModeChangedEvent, new object[] { this, new PowerModeChangedEventArgs(suspend) });
        }

        private void OnSessionEnded(IntPtr wParam, IntPtr lParam)
        {
            if (wParam != IntPtr.Zero)
            {
                SessionEndReasons systemShutdown = SessionEndReasons.SystemShutdown;
                if ((((int) ((long) lParam)) & -2147483648) != 0)
                {
                    systemShutdown = SessionEndReasons.Logoff;
                }
                SessionEndedEventArgs args = new SessionEndedEventArgs(systemShutdown);
                RaiseEvent(OnSessionEndedEvent, new object[] { this, args });
            }
        }

        private int OnSessionEnding(IntPtr lParam)
        {
            SessionEndReasons systemShutdown = SessionEndReasons.SystemShutdown;
            if ((((long) lParam) & -2147483648L) != 0L)
            {
                systemShutdown = SessionEndReasons.Logoff;
            }
            SessionEndingEventArgs args = new SessionEndingEventArgs(systemShutdown);
            RaiseEvent(OnSessionEndingEvent, new object[] { this, args });
            return (args.Cancel ? 0 : 1);
        }

        private void OnSessionSwitch(int wParam)
        {
            SessionSwitchEventArgs args = new SessionSwitchEventArgs((SessionSwitchReason) wParam);
            RaiseEvent(OnSessionSwitchEvent, new object[] { this, args });
        }

        private void OnShutdown(object eventKey)
        {
            RaiseEvent(false, eventKey, new object[] { this, EventArgs.Empty });
        }

        private void OnThemeChanged()
        {
            RaiseEvent(OnUserPreferenceChangingEvent, new object[] { this, new UserPreferenceChangingEventArgs(UserPreferenceCategory.VisualStyle) });
            UserPreferenceCategory window = UserPreferenceCategory.Window;
            RaiseEvent(OnUserPreferenceChangedEvent, new object[] { this, new UserPreferenceChangedEventArgs(window) });
            window = UserPreferenceCategory.VisualStyle;
            RaiseEvent(OnUserPreferenceChangedEvent, new object[] { this, new UserPreferenceChangedEventArgs(window) });
        }

        private void OnTimerElapsed(IntPtr wParam)
        {
            RaiseEvent(OnTimerElapsedEvent, new object[] { this, new TimerElapsedEventArgs(wParam) });
        }

        private void OnUserPreferenceChanged(int msg, IntPtr wParam, IntPtr lParam)
        {
            UserPreferenceCategory category = this.GetUserPreferenceCategory(msg, wParam, lParam);
            RaiseEvent(OnUserPreferenceChangedEvent, new object[] { this, new UserPreferenceChangedEventArgs(category) });
        }

        private void OnUserPreferenceChanging(int msg, IntPtr wParam, IntPtr lParam)
        {
            UserPreferenceCategory category = this.GetUserPreferenceCategory(msg, wParam, lParam);
            RaiseEvent(OnUserPreferenceChangingEvent, new object[] { this, new UserPreferenceChangingEventArgs(category) });
        }

        private static void RaiseEvent(object key, params object[] args)
        {
            RaiseEvent(true, key, args);
        }

        private static void RaiseEvent(bool checkFinalization, object key, params object[] args)
        {
            if (!checkFinalization || !AppDomain.CurrentDomain.IsFinalizingForUnload())
            {
                SystemEventInvokeInfo[] infoArray = null;
                lock (eventLockObject)
                {
                    if ((_handlers != null) && _handlers.ContainsKey(key))
                    {
                        List<SystemEventInvokeInfo> list = _handlers[key];
                        if (list != null)
                        {
                            infoArray = list.ToArray();
                        }
                    }
                }
                if (infoArray != null)
                {
                    for (int i = 0; i < infoArray.Length; i++)
                    {
                        try
                        {
                            infoArray[i].Invoke(checkFinalization, args);
                            infoArray[i] = null;
                        }
                        catch (Exception)
                        {
                        }
                    }
                    lock (eventLockObject)
                    {
                        List<SystemEventInvokeInfo> list2 = null;
                        for (int j = 0; j < infoArray.Length; j++)
                        {
                            SystemEventInvokeInfo item = infoArray[j];
                            if (item != null)
                            {
                                if ((list2 == null) && !_handlers.TryGetValue(key, out list2))
                                {
                                    return;
                                }
                                list2.Remove(item);
                            }
                        }
                    }
                }
            }
        }

        private static void RemoveEventHandler(object key, Delegate value)
        {
            lock (eventLockObject)
            {
                if ((_handlers != null) && _handlers.ContainsKey(key))
                {
                    _handlers[key].Remove(new SystemEventInvokeInfo(value));
                }
            }
        }

        private static void Shutdown()
        {
            if ((systemEvents != null) && (systemEvents.windowHandle != IntPtr.Zero))
            {
                lock (procLockObject)
                {
                    if (systemEvents != null)
                    {
                        startupRecreates = true;
                        if (windowThread != null)
                        {
                            eventThreadTerminated = new ManualResetEvent(false);
                            Microsoft.Win32.UnsafeNativeMethods.PostMessage(new HandleRef(systemEvents, systemEvents.windowHandle), 0x12, IntPtr.Zero, IntPtr.Zero);
                            eventThreadTerminated.WaitOne();
                            windowThread.Join();
                        }
                        else
                        {
                            systemEvents.Dispose();
                            systemEvents = null;
                        }
                    }
                }
            }
        }

        [PrePrepareMethod]
        private static void Shutdown(object sender, EventArgs e)
        {
            Shutdown();
        }

        private static void Startup()
        {
            if (startupRecreates)
            {
                EnsureSystemEvents(false, false);
            }
        }

        private IntPtr WindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case 0x1a:
                {
                    IntPtr lparam = lParam;
                    if (lParam != IntPtr.Zero)
                    {
                        string s = Marshal.PtrToStringAuto(lParam);
                        if (s != null)
                        {
                            lparam = Marshal.StringToHGlobalAuto(s);
                        }
                    }
                    Microsoft.Win32.UnsafeNativeMethods.PostMessage(new HandleRef(this, this.windowHandle), 0x2000 + msg, wParam, lparam);
                    goto Label_02D6;
                }
                case 0x1d:
                case 30:
                case 0x41:
                case 0x15:
                case 0x7e:
                case 0x113:
                case 0x311:
                case 0x31a:
                    Microsoft.Win32.UnsafeNativeMethods.PostMessage(new HandleRef(this, this.windowHandle), 0x2000 + msg, wParam, lParam);
                    goto Label_02D6;

                case 0x16:
                    this.OnSessionEnded(wParam, lParam);
                    goto Label_02D6;

                case 0x11:
                    return (IntPtr) this.OnSessionEnding(lParam);

                case 0x218:
                    this.OnPowerModeChanged(wParam);
                    goto Label_02D6;

                case 0x2b1:
                    this.OnSessionSwitch((int) wParam);
                    goto Label_02D6;

                case 0x201a:
                    try
                    {
                        this.OnUserPreferenceChanging(msg - 0x2000, wParam, lParam);
                        this.OnUserPreferenceChanged(msg - 0x2000, wParam, lParam);
                        goto Label_02D6;
                    }
                    finally
                    {
                        try
                        {
                            if (lParam != IntPtr.Zero)
                            {
                                Marshal.FreeHGlobal(lParam);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    break;

                case 0x201d:
                    this.OnGenericEvent(OnInstalledFontsChangedEvent);
                    goto Label_02D6;

                case 0x201e:
                    this.OnGenericEvent(OnTimeChangedEvent);
                    goto Label_02D6;

                case 0x2015:
                    break;

                case 0x401:
                    return this.OnCreateTimer(wParam);

                case 0x402:
                    return (this.OnKillTimer(wParam) ? ((IntPtr) 1) : IntPtr.Zero);

                case 0x2041:
                    this.OnGenericEvent(OnLowMemoryEvent);
                    goto Label_02D6;

                case 0x207e:
                    this.OnDisplaySettingsChanging();
                    this.OnDisplaySettingsChanged();
                    goto Label_02D6;

                case 0x2113:
                    this.OnTimerElapsed(wParam);
                    goto Label_02D6;

                case 0x2311:
                    this.OnGenericEvent(OnPaletteChangedEvent);
                    goto Label_02D6;

                case 0x231a:
                    this.OnThemeChanged();
                    goto Label_02D6;

                default:
                    if ((msg == threadCallbackMessage) && (msg != 0))
                    {
                        this.InvokeMarshaledCallbacks();
                        return IntPtr.Zero;
                    }
                    goto Label_02D6;
            }
            this.OnUserPreferenceChanging(msg - 0x2000, wParam, lParam);
            this.OnUserPreferenceChanged(msg - 0x2000, wParam, lParam);
        Label_02D6:
            return Microsoft.Win32.UnsafeNativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private void WindowThreadProc()
        {
            try
            {
                this.Initialize();
                eventWindowReady.Set();
                if (this.windowHandle != IntPtr.Zero)
                {
                    Microsoft.Win32.NativeMethods.MSG msg = new Microsoft.Win32.NativeMethods.MSG();
                    bool flag = true;
                    while (flag)
                    {
                        if (Microsoft.Win32.UnsafeNativeMethods.MsgWaitForMultipleObjectsEx(0, IntPtr.Zero, 100, 0xff, 4) != 0x102)
                        {
                            goto Label_0072;
                        }
                        Thread.Sleep(1);
                        continue;
                    Label_0053:
                        if (msg.message == 0x12)
                        {
                            flag = false;
                            continue;
                        }
                        Microsoft.Win32.UnsafeNativeMethods.TranslateMessage(ref msg);
                        Microsoft.Win32.UnsafeNativeMethods.DispatchMessage(ref msg);
                    Label_0072:
                        if (Microsoft.Win32.UnsafeNativeMethods.PeekMessage(ref msg, Microsoft.Win32.NativeMethods.NullHandleRef, 0, 0, 1))
                        {
                            goto Label_0053;
                        }
                    }
                }
                this.OnShutdown(OnEventsThreadShutdownEvent);
            }
            catch (Exception exception)
            {
                eventWindowReady.Set();
                if (!(exception is ThreadInterruptedException))
                {
                    ThreadAbortException exception2 = exception as ThreadAbortException;
                }
            }
            this.Dispose();
            if (eventThreadTerminated != null)
            {
                eventThreadTerminated.Set();
            }
        }

        private static string CompanyNameInternal
        {
            get
            {
                string company = null;
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    object[] customAttributes = entryAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                    {
                        company = ((AssemblyCompanyAttribute) customAttributes[0]).Company;
                    }
                }
                if ((company == null) || (company.Length == 0))
                {
                    company = GetAppFileVersionInfo().CompanyName;
                    if (company != null)
                    {
                        company = company.Trim();
                    }
                }
                if ((company != null) && (company.Length != 0))
                {
                    return company;
                }
                Type appMainType = GetAppMainType();
                if (appMainType == null)
                {
                    return company;
                }
                string str2 = appMainType.Namespace;
                if (!string.IsNullOrEmpty(str2))
                {
                    int index = str2.IndexOf(".", StringComparison.Ordinal);
                    if (index != -1)
                    {
                        return str2.Substring(0, index);
                    }
                    return str2;
                }
                return ProductNameInternal;
            }
        }

        private IntPtr DefWndProc
        {
            get
            {
                if (defWindowProc == IntPtr.Zero)
                {
                    string lpProcName = (Marshal.SystemDefaultCharSize == 1) ? "DefWindowProcA" : "DefWindowProcW";
                    defWindowProc = Microsoft.Win32.UnsafeNativeMethods.GetProcAddress(new HandleRef(this, Microsoft.Win32.UnsafeNativeMethods.GetModuleHandle("user32.dll")), lpProcName);
                }
                return defWindowProc;
            }
        }

        private static string ExecutablePath
        {
            get
            {
                if (executablePath == null)
                {
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly == null)
                    {
                        StringBuilder buffer = new StringBuilder(260);
                        Microsoft.Win32.UnsafeNativeMethods.GetModuleFileName(Microsoft.Win32.NativeMethods.NullHandleRef, buffer, buffer.Capacity);
                        executablePath = IntSecurity.UnsafeGetFullPath(buffer.ToString());
                    }
                    else
                    {
                        string escapedCodeBase = entryAssembly.EscapedCodeBase;
                        Uri uri = new Uri(escapedCodeBase);
                        if (uri.Scheme == "file")
                        {
                            executablePath = Microsoft.Win32.NativeMethods.GetLocalPath(escapedCodeBase);
                        }
                        else
                        {
                            executablePath = uri.ToString();
                        }
                    }
                }
                Uri uri2 = new Uri(executablePath);
                if (uri2.Scheme == "file")
                {
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, executablePath).Demand();
                }
                return executablePath;
            }
        }

        private static string ProductNameInternal
        {
            get
            {
                string product = null;
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    object[] customAttributes = entryAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                    {
                        product = ((AssemblyProductAttribute) customAttributes[0]).Product;
                    }
                }
                if ((product == null) || (product.Length == 0))
                {
                    product = GetAppFileVersionInfo().ProductName;
                    if (product != null)
                    {
                        product = product.Trim();
                    }
                }
                if ((product != null) && (product.Length != 0))
                {
                    return product;
                }
                Type appMainType = GetAppMainType();
                if (appMainType == null)
                {
                    return product;
                }
                string str2 = appMainType.Namespace;
                if (!string.IsNullOrEmpty(str2))
                {
                    int num = str2.LastIndexOf(".", StringComparison.Ordinal);
                    if ((num != -1) && (num < (str2.Length - 1)))
                    {
                        return str2.Substring(num + 1);
                    }
                    return str2;
                }
                return appMainType.Name;
            }
        }

        private static string ProductVersionInternal
        {
            get
            {
                string informationalVersion = null;
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    object[] customAttributes = entryAssembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                    {
                        informationalVersion = ((AssemblyInformationalVersionAttribute) customAttributes[0]).InformationalVersion;
                    }
                }
                if ((informationalVersion == null) || (informationalVersion.Length == 0))
                {
                    informationalVersion = GetAppFileVersionInfo().ProductVersion;
                    if (informationalVersion != null)
                    {
                        informationalVersion = informationalVersion.Trim();
                    }
                }
                if ((informationalVersion != null) && (informationalVersion.Length != 0))
                {
                    return informationalVersion;
                }
                return "1.0.0.0";
            }
        }

        internal static bool UseEverettThreadAffinity
        {
            get
            {
                if (!checkedThreadAffinity)
                {
                    lock (eventLockObject)
                    {
                        if (!checkedThreadAffinity)
                        {
                            checkedThreadAffinity = true;
                            string format = @"Software\{0}\{1}\{2}";
                            try
                            {
                                new RegistryPermission(PermissionState.Unrestricted).Assert();
                                RegistryKey key = Registry.LocalMachine.OpenSubKey(string.Format(CultureInfo.CurrentCulture, format, new object[] { CompanyNameInternal, ProductNameInternal, ProductVersionInternal }));
                                if (key != null)
                                {
                                    object obj2 = key.GetValue("EnableSystemEventsThreadAffinityCompatibility");
                                    if ((obj2 != null) && (((int) obj2) != 0))
                                    {
                                        useEverettThreadAffinity = true;
                                    }
                                }
                            }
                            catch (SecurityException)
                            {
                            }
                            catch (InvalidCastException)
                            {
                            }
                        }
                    }
                }
                return useEverettThreadAffinity;
            }
        }

        private static bool UserInteractive
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    IntPtr zero = IntPtr.Zero;
                    zero = Microsoft.Win32.UnsafeNativeMethods.GetProcessWindowStation();
                    if ((zero != IntPtr.Zero) && (processWinStation != zero))
                    {
                        isUserInteractive = true;
                        int lpnLengthNeeded = 0;
                        Microsoft.Win32.NativeMethods.USEROBJECTFLAGS pvBuffer = new Microsoft.Win32.NativeMethods.USEROBJECTFLAGS();
                        if (Microsoft.Win32.UnsafeNativeMethods.GetUserObjectInformation(new HandleRef(null, zero), 1, pvBuffer, Marshal.SizeOf(pvBuffer), ref lpnLengthNeeded) && ((pvBuffer.dwFlags & 1) == 0))
                        {
                            isUserInteractive = false;
                        }
                        processWinStation = zero;
                    }
                }
                else
                {
                    isUserInteractive = true;
                }
                return isUserInteractive;
            }
        }

        private Microsoft.Win32.NativeMethods.WNDCLASS WndClass
        {
            get
            {
                if (staticwndclass == null)
                {
                    IntPtr moduleHandle = Microsoft.Win32.UnsafeNativeMethods.GetModuleHandle(null);
                    className = string.Format(CultureInfo.InvariantCulture, ".NET-BroadcastEventWindow.{0}.{1}.{2}", new object[] { "4.0.0.0", Convert.ToString(AppDomain.CurrentDomain.GetHashCode(), 0x10), domainQualifier });
                    staticwndclass = new Microsoft.Win32.NativeMethods.WNDCLASS();
                    staticwndclass.hbrBackground = (IntPtr) 6;
                    staticwndclass.style = 0;
                    this.windowProc = new Microsoft.Win32.NativeMethods.WndProc(this.WindowProc);
                    staticwndclass.lpszClassName = className;
                    staticwndclass.lpfnWndProc = this.windowProc;
                    staticwndclass.hInstance = moduleHandle;
                }
                return staticwndclass;
            }
        }

        private class SystemEventInvokeInfo
        {
            private Delegate _delegate;
            private SynchronizationContext _syncContext;

            public SystemEventInvokeInfo(Delegate d)
            {
                this._delegate = d;
                this._syncContext = AsyncOperationManager.SynchronizationContext;
            }

            public override bool Equals(object other)
            {
                SystemEvents.SystemEventInvokeInfo info = other as SystemEvents.SystemEventInvokeInfo;
                if (info == null)
                {
                    return false;
                }
                return info._delegate.Equals(this._delegate);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public void Invoke(bool checkFinalization, params object[] args)
            {
                try
                {
                    if ((this._syncContext == null) || SystemEvents.UseEverettThreadAffinity)
                    {
                        this.InvokeCallback(args);
                    }
                    else
                    {
                        this._syncContext.Send(new SendOrPostCallback(this.InvokeCallback), args);
                    }
                }
                catch (InvalidAsynchronousStateException)
                {
                    if (!checkFinalization || !AppDomain.CurrentDomain.IsFinalizingForUnload())
                    {
                        this.InvokeCallback(args);
                    }
                }
            }

            private void InvokeCallback(object arg)
            {
                this._delegate.DynamicInvoke((object[]) arg);
            }
        }
    }
}

