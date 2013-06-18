namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    public class SystemInformation
    {
        private static bool checkMultiMonitorSupport = false;
        private static bool checkNativeMouseWheelSupport = false;
        private const int DefaultMouseWheelScrollLines = 3;
        private static bool highContrast = false;
        private static bool isUserInteractive = false;
        private static bool multiMonitorSupport = false;
        private static bool nativeMouseWheelSupport = true;
        private static System.Windows.Forms.PowerStatus powerStatus = null;
        private static IntPtr processWinStation = IntPtr.Zero;
        private static bool systemEventsAttached = false;
        private static bool systemEventsDirty = true;

        private SystemInformation()
        {
        }

        private static void EnsureSystemEvents()
        {
            if (!systemEventsAttached)
            {
                SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(SystemInformation.OnUserPreferenceChanged);
                systemEventsAttached = true;
            }
        }

        internal static bool InLockedTerminalSession()
        {
            bool flag = false;
            if (TerminalServerSession && (System.Windows.Forms.SafeNativeMethods.OpenInputDesktop(0, false, 0x100) == IntPtr.Zero))
            {
                flag = Marshal.GetLastWin32Error() == 5;
            }
            return flag;
        }

        private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs pref)
        {
            systemEventsDirty = true;
        }

        public static int ActiveWindowTrackingDelay
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x2002, 0, ref num, 0);
                return num;
            }
        }

        public static System.Windows.Forms.ArrangeDirection ArrangeDirection
        {
            get
            {
                System.Windows.Forms.ArrangeDirection down = System.Windows.Forms.ArrangeDirection.Down;
                int systemMetrics = System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x38);
                return (down & systemMetrics);
            }
        }

        public static System.Windows.Forms.ArrangeStartingPosition ArrangeStartingPosition
        {
            get
            {
                System.Windows.Forms.ArrangeStartingPosition position = System.Windows.Forms.ArrangeStartingPosition.Hide | System.Windows.Forms.ArrangeStartingPosition.TopRight;
                int systemMetrics = System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x38);
                return (position & systemMetrics);
            }
        }

        public static System.Windows.Forms.BootMode BootMode
        {
            get
            {
                System.Windows.Forms.IntSecurity.SensitiveSystemInformation.Demand();
                return (System.Windows.Forms.BootMode) System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x43);
            }
        }

        public static Size Border3DSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x2d), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x2e));
            }
        }

        public static int BorderMultiplierFactor
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(5, 0, ref num, 0);
                return num;
            }
        }

        public static Size BorderSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(5), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(6));
            }
        }

        public static Size CaptionButtonSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(30), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x1f));
            }
        }

        public static int CaptionHeight
        {
            get
            {
                return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(4);
            }
        }

        public static int CaretBlinkTime
        {
            get
            {
                return (int) System.Windows.Forms.SafeNativeMethods.GetCaretBlinkTime();
            }
        }

        public static int CaretWidth
        {
            get
            {
                if (!OSFeature.Feature.OnXp && !OSFeature.Feature.OnWin2k)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("SystemInformationFeatureNotSupported"));
                }
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x2006, 0, ref num, 0);
                return num;
            }
        }

        public static string ComputerName
        {
            get
            {
                System.Windows.Forms.IntSecurity.SensitiveSystemInformation.Demand();
                StringBuilder lpBuffer = new StringBuilder(0x100);
                System.Windows.Forms.UnsafeNativeMethods.GetComputerName(lpBuffer, new int[] { lpBuffer.Capacity });
                return lpBuffer.ToString();
            }
        }

        public static Size CursorSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(13), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(14));
            }
        }

        public static bool DbcsEnabled
        {
            get
            {
                return (System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x2a) != 0);
            }
        }

        public static bool DebugOS
        {
            get
            {
                System.Windows.Forms.IntSecurity.SensitiveSystemInformation.Demand();
                return (System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x16) != 0);
            }
        }

        public static Size DoubleClickSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x24), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x25));
            }
        }

        public static int DoubleClickTime
        {
            get
            {
                return System.Windows.Forms.SafeNativeMethods.GetDoubleClickTime();
            }
        }

        public static bool DragFullWindows
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x26, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static Size DragSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x44), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x45));
            }
        }

        public static Size FixedFrameBorderSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(7), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(8));
            }
        }

        public static int FontSmoothingContrast
        {
            get
            {
                if (!OSFeature.Feature.OnXp)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("SystemInformationFeatureNotSupported"));
                }
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x200c, 0, ref num, 0);
                return num;
            }
        }

        public static int FontSmoothingType
        {
            get
            {
                if (!OSFeature.Feature.OnXp)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("SystemInformationFeatureNotSupported"));
                }
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x200a, 0, ref num, 0);
                return num;
            }
        }

        public static Size FrameBorderSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x20), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x21));
            }
        }

        public static bool HighContrast
        {
            get
            {
                EnsureSystemEvents();
                if (systemEventsDirty)
                {
                    System.Windows.Forms.NativeMethods.HIGHCONTRAST_I highcontrast_i;
                    highcontrast_i = new System.Windows.Forms.NativeMethods.HIGHCONTRAST_I {
                        cbSize = Marshal.SizeOf(highcontrast_i),
                        dwFlags = 0,
                        lpszDefaultScheme = IntPtr.Zero
                    };
                    if (System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x42, highcontrast_i.cbSize, ref highcontrast_i, 0))
                    {
                        highContrast = (highcontrast_i.dwFlags & 1) != 0;
                    }
                    else
                    {
                        highContrast = false;
                    }
                    systemEventsDirty = false;
                }
                return highContrast;
            }
        }

        public static int HorizontalFocusThickness
        {
            get
            {
                if (!OSFeature.Feature.OnXp)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("SystemInformationFeatureNotSupported"));
                }
                return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x53);
            }
        }

        public static int HorizontalResizeBorderThickness
        {
            get
            {
                return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x20);
            }
        }

        public static int HorizontalScrollBarArrowWidth
        {
            get
            {
                return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x15);
            }
        }

        public static int HorizontalScrollBarHeight
        {
            get
            {
                return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(3);
            }
        }

        public static int HorizontalScrollBarThumbWidth
        {
            get
            {
                return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(10);
            }
        }

        public static int IconHorizontalSpacing
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(13, 0, ref num, 0);
                return num;
            }
        }

        public static Size IconSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(11), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(12));
            }
        }

        public static Size IconSpacingSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x26), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x27));
            }
        }

        public static int IconVerticalSpacing
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x18, 0, ref num, 0);
                return num;
            }
        }

        public static bool IsActiveWindowTrackingEnabled
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x1000, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static bool IsComboBoxAnimationEnabled
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x1004, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static bool IsDropShadowEnabled
        {
            get
            {
                if (OSFeature.Feature.OnXp)
                {
                    int num = 0;
                    System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x1024, 0, ref num, 0);
                    return (num != 0);
                }
                return false;
            }
        }

        public static bool IsFlatMenuEnabled
        {
            get
            {
                if (OSFeature.Feature.OnXp)
                {
                    int num = 0;
                    System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x1022, 0, ref num, 0);
                    return (num != 0);
                }
                return false;
            }
        }

        public static bool IsFontSmoothingEnabled
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x4a, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static bool IsHotTrackingEnabled
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x100e, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static bool IsIconTitleWrappingEnabled
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x19, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static bool IsKeyboardPreferred
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x44, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static bool IsListBoxSmoothScrollingEnabled
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x1006, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static bool IsMenuAnimationEnabled
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x1002, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static bool IsMenuFadeEnabled
        {
            get
            {
                if (!OSFeature.Feature.OnXp && !OSFeature.Feature.OnWin2k)
                {
                    return false;
                }
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x1012, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static bool IsMinimizeRestoreAnimationEnabled
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x48, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static bool IsSelectionFadeEnabled
        {
            get
            {
                if (!OSFeature.Feature.OnXp && !OSFeature.Feature.OnWin2k)
                {
                    return false;
                }
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x1014, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static bool IsSnapToDefaultEnabled
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x5f, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static bool IsTitleBarGradientEnabled
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x1008, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static bool IsToolTipAnimationEnabled
        {
            get
            {
                if (!OSFeature.Feature.OnXp && !OSFeature.Feature.OnWin2k)
                {
                    return false;
                }
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x1016, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static int KanjiWindowHeight
        {
            get
            {
                return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x12);
            }
        }

        public static int KeyboardDelay
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x16, 0, ref num, 0);
                return num;
            }
        }

        public static int KeyboardSpeed
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(10, 0, ref num, 0);
                return num;
            }
        }

        public static Size MaxWindowTrackSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x3b), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(60));
            }
        }

        public static bool MenuAccessKeysUnderlined
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x100a, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static Size MenuBarButtonSize
        {
            get
            {
                System.Windows.Forms.NativeMethods.NONCLIENTMETRICS metrics = new System.Windows.Forms.NativeMethods.NONCLIENTMETRICS();
                if ((System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x29, metrics.cbSize, metrics, 0) && (metrics.iMenuHeight > 0)) && (metrics.iMenuWidth > 0))
                {
                    return new Size(metrics.iMenuWidth, metrics.iMenuHeight);
                }
                return Size.Empty;
            }
        }

        public static Size MenuButtonSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x36), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x37));
            }
        }

        public static Size MenuCheckSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x47), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x48));
            }
        }

        public static Font MenuFont
        {
            get
            {
                Font defaultFont = null;
                System.Windows.Forms.NativeMethods.NONCLIENTMETRICS metrics = new System.Windows.Forms.NativeMethods.NONCLIENTMETRICS();
                if (System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x29, metrics.cbSize, metrics, 0) && (metrics.lfMenuFont != null))
                {
                    System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        defaultFont = Font.FromLogFont(metrics.lfMenuFont);
                    }
                    catch
                    {
                        defaultFont = Control.DefaultFont;
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
                return defaultFont;
            }
        }

        public static int MenuHeight
        {
            get
            {
                return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(15);
            }
        }

        public static int MenuShowDelay
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x6a, 0, ref num, 0);
                return num;
            }
        }

        public static bool MidEastEnabled
        {
            get
            {
                return (System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x4a) != 0);
            }
        }

        public static Size MinimizedWindowSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x39), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x3a));
            }
        }

        public static Size MinimizedWindowSpacingSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x2f), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x30));
            }
        }

        public static Size MinimumWindowSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x1c), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x1d));
            }
        }

        public static Size MinWindowTrackSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x22), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x23));
            }
        }

        public static int MonitorCount
        {
            get
            {
                if (MultiMonitorSupport)
                {
                    return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(80);
                }
                return 1;
            }
        }

        public static bool MonitorsSameDisplayFormat
        {
            get
            {
                if (MultiMonitorSupport)
                {
                    return (System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x51) != 0);
                }
                return true;
            }
        }

        public static int MouseButtons
        {
            get
            {
                return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x2b);
            }
        }

        public static bool MouseButtonsSwapped
        {
            get
            {
                return (System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x17) != 0);
            }
        }

        public static Size MouseHoverSize
        {
            get
            {
                int num = 0;
                int num2 = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(100, 0, ref num, 0);
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x62, 0, ref num2, 0);
                return new Size(num2, num);
            }
        }

        public static int MouseHoverTime
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x66, 0, ref num, 0);
                return num;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool MousePresent
        {
            get
            {
                return (System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x13) != 0);
            }
        }

        public static int MouseSpeed
        {
            get
            {
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x70, 0, ref num, 0);
                return num;
            }
        }

        public static bool MouseWheelPresent
        {
            get
            {
                bool flag = false;
                if (!NativeMouseWheelSupport)
                {
                    if (System.Windows.Forms.UnsafeNativeMethods.FindWindow("MouseZ", "Magellan MSWHEEL") != IntPtr.Zero)
                    {
                        flag = true;
                    }
                    return flag;
                }
                return (System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x4b) != 0);
            }
        }

        public static int MouseWheelScrollDelta
        {
            get
            {
                return 120;
            }
        }

        public static int MouseWheelScrollLines
        {
            get
            {
                if (NativeMouseWheelSupport)
                {
                    int num = 0;
                    System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x68, 0, ref num, 0);
                    return num;
                }
                IntPtr zero = IntPtr.Zero;
                zero = System.Windows.Forms.UnsafeNativeMethods.FindWindow("MouseZ", "Magellan MSWHEEL");
                if (zero != IntPtr.Zero)
                {
                    int msg = System.Windows.Forms.SafeNativeMethods.RegisterWindowMessage("MSH_SCROLL_LINES_MSG");
                    int num3 = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(null, zero), msg, 0, 0);
                    if (num3 != 0)
                    {
                        return num3;
                    }
                }
                return 3;
            }
        }

        private static bool MultiMonitorSupport
        {
            get
            {
                if (!checkMultiMonitorSupport)
                {
                    multiMonitorSupport = System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(80) != 0;
                    checkMultiMonitorSupport = true;
                }
                return multiMonitorSupport;
            }
        }

        public static bool NativeMouseWheelSupport
        {
            get
            {
                if (!checkNativeMouseWheelSupport)
                {
                    nativeMouseWheelSupport = System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x4b) != 0;
                    checkNativeMouseWheelSupport = true;
                }
                return nativeMouseWheelSupport;
            }
        }

        public static bool Network
        {
            get
            {
                return ((System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x3f) & 1) != 0);
            }
        }

        public static bool PenWindows
        {
            get
            {
                return (System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x29) != 0);
            }
        }

        public static LeftRightAlignment PopupMenuAlignment
        {
            get
            {
                bool flag = false;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x1b, 0, ref flag, 0);
                if (flag)
                {
                    return LeftRightAlignment.Left;
                }
                return LeftRightAlignment.Right;
            }
        }

        public static System.Windows.Forms.PowerStatus PowerStatus
        {
            get
            {
                if (powerStatus == null)
                {
                    powerStatus = new System.Windows.Forms.PowerStatus();
                }
                return powerStatus;
            }
        }

        public static Size PrimaryMonitorMaximizedWindowSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x3d), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x3e));
            }
        }

        public static Size PrimaryMonitorSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(1));
            }
        }

        public static bool RightAlignedMenus
        {
            get
            {
                return (System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(40) != 0);
            }
        }

        public static System.Windows.Forms.ScreenOrientation ScreenOrientation
        {
            get
            {
                System.Windows.Forms.ScreenOrientation dmDisplayOrientation = System.Windows.Forms.ScreenOrientation.Angle0;
                System.Windows.Forms.NativeMethods.DEVMODE lpDevMode = new System.Windows.Forms.NativeMethods.DEVMODE {
                    dmSize = (short) Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.DEVMODE)),
                    dmDriverExtra = 0
                };
                try
                {
                    System.Windows.Forms.SafeNativeMethods.EnumDisplaySettings(null, -1, ref lpDevMode);
                    if ((lpDevMode.dmFields & 0x80) > 0)
                    {
                        dmDisplayOrientation = lpDevMode.dmDisplayOrientation;
                    }
                }
                catch
                {
                }
                return dmDisplayOrientation;
            }
        }

        public static bool Secure
        {
            get
            {
                System.Windows.Forms.IntSecurity.SensitiveSystemInformation.Demand();
                return (System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x2c) != 0);
            }
        }

        public static bool ShowSounds
        {
            get
            {
                return (System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(70) != 0);
            }
        }

        public static int SizingBorderWidth
        {
            get
            {
                System.Windows.Forms.NativeMethods.NONCLIENTMETRICS metrics = new System.Windows.Forms.NativeMethods.NONCLIENTMETRICS();
                if (System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x29, metrics.cbSize, metrics, 0) && (metrics.iBorderWidth > 0))
                {
                    return metrics.iBorderWidth;
                }
                return 0;
            }
        }

        public static Size SmallCaptionButtonSize
        {
            get
            {
                System.Windows.Forms.NativeMethods.NONCLIENTMETRICS metrics = new System.Windows.Forms.NativeMethods.NONCLIENTMETRICS();
                if ((System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x29, metrics.cbSize, metrics, 0) && (metrics.iSmCaptionHeight > 0)) && (metrics.iSmCaptionWidth > 0))
                {
                    return new Size(metrics.iSmCaptionWidth, metrics.iSmCaptionHeight);
                }
                return Size.Empty;
            }
        }

        public static Size SmallIconSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x31), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(50));
            }
        }

        public static bool TerminalServerSession
        {
            get
            {
                return ((System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x1000) & 1) != 0);
            }
        }

        public static Size ToolWindowCaptionButtonSize
        {
            get
            {
                return new Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x34), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x35));
            }
        }

        public static int ToolWindowCaptionHeight
        {
            get
            {
                return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x33);
            }
        }

        public static bool UIEffectsEnabled
        {
            get
            {
                if (!OSFeature.Feature.OnXp && !OSFeature.Feature.OnWin2k)
                {
                    return false;
                }
                int num = 0;
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x103e, 0, ref num, 0);
                return (num != 0);
            }
        }

        public static string UserDomainName
        {
            get
            {
                return Environment.UserDomainName;
            }
        }

        public static bool UserInteractive
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    IntPtr zero = IntPtr.Zero;
                    zero = System.Windows.Forms.UnsafeNativeMethods.GetProcessWindowStation();
                    if ((zero != IntPtr.Zero) && (processWinStation != zero))
                    {
                        isUserInteractive = true;
                        int lpnLengthNeeded = 0;
                        System.Windows.Forms.NativeMethods.USEROBJECTFLAGS pvBuffer = new System.Windows.Forms.NativeMethods.USEROBJECTFLAGS();
                        if (System.Windows.Forms.UnsafeNativeMethods.GetUserObjectInformation(new HandleRef(null, zero), 1, pvBuffer, Marshal.SizeOf(pvBuffer), ref lpnLengthNeeded) && ((pvBuffer.dwFlags & 1) == 0))
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

        public static string UserName
        {
            get
            {
                System.Windows.Forms.IntSecurity.SensitiveSystemInformation.Demand();
                StringBuilder lpBuffer = new StringBuilder(0x100);
                System.Windows.Forms.UnsafeNativeMethods.GetUserName(lpBuffer, new int[] { lpBuffer.Capacity });
                return lpBuffer.ToString();
            }
        }

        public static int VerticalFocusThickness
        {
            get
            {
                if (!OSFeature.Feature.OnXp)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("SystemInformationFeatureNotSupported"));
                }
                return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x54);
            }
        }

        public static int VerticalResizeBorderThickness
        {
            get
            {
                return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x21);
            }
        }

        public static int VerticalScrollBarArrowHeight
        {
            get
            {
                return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(20);
            }
        }

        public static int VerticalScrollBarThumbHeight
        {
            get
            {
                return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(9);
            }
        }

        public static int VerticalScrollBarWidth
        {
            get
            {
                return System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(2);
            }
        }

        public static Rectangle VirtualScreen
        {
            get
            {
                if (MultiMonitorSupport)
                {
                    return new Rectangle(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x4c), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x4d), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x4e), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(0x4f));
                }
                Size primaryMonitorSize = PrimaryMonitorSize;
                return new Rectangle(0, 0, primaryMonitorSize.Width, primaryMonitorSize.Height);
            }
        }

        public static Rectangle WorkingArea
        {
            get
            {
                System.Windows.Forms.NativeMethods.RECT rc = new System.Windows.Forms.NativeMethods.RECT();
                System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x30, 0, ref rc, 0);
                return Rectangle.FromLTRB(rc.left, rc.top, rc.right, rc.bottom);
            }
        }
    }
}

