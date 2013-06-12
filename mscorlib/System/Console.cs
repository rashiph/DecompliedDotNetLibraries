namespace System
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System.IO;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    public static class Console
    {
        [SecurityCritical]
        private static Win32Native.InputRecord _cachedInputRecord;
        private static ConsoleCancelEventHandler _cancelCallbacks;
        private static IntPtr _consoleInputHandle;
        private static IntPtr _consoleOutputHandle;
        private static byte _defaultColors;
        private const int _DefaultConsoleBufferSize = 0x100;
        private static TextWriter _error;
        private static bool _haveReadDefaultColors;
        private static ControlCHooker _hooker;
        private static TextReader _in;
        private static Encoding _inputEncoding;
        private static TextWriter _out;
        private static Encoding _outputEncoding;
        private static bool _wasErrorRedirected;
        private static bool _wasOutRedirected;
        private const short AltVKCode = 0x12;
        private const int CapsLockVKCode = 20;
        private const int MaxBeepFrequency = 0x7fff;
        private const int MaxConsoleTitleLength = 0x5fb4;
        private const int MinBeepFrequency = 0x25;
        private const int NumberLockVKCode = 0x90;
        private static object s_InternalSyncObject;

        public static  event ConsoleCancelEventHandler CancelKeyPress
        {
            [SecuritySafeCritical] add
            {
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
                lock (InternalSyncObject)
                {
                    _cancelCallbacks = (ConsoleCancelEventHandler) Delegate.Combine(_cancelCallbacks, value);
                    if (_hooker == null)
                    {
                        _hooker = new ControlCHooker();
                        _hooker.Hook();
                    }
                }
            }
            [SecuritySafeCritical] remove
            {
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
                lock (InternalSyncObject)
                {
                    _cancelCallbacks = (ConsoleCancelEventHandler) Delegate.Remove(_cancelCallbacks, value);
                    if ((_hooker != null) && (_cancelCallbacks == null))
                    {
                        _hooker.Unhook();
                    }
                }
            }
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Beep()
        {
            Beep(800, 200);
        }

        [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Beep(int frequency, int duration)
        {
            if ((frequency < 0x25) || (frequency > 0x7fff))
            {
                throw new ArgumentOutOfRangeException("frequency", frequency, Environment.GetResourceString("ArgumentOutOfRange_BeepFrequency", new object[] { 0x25, 0x7fff }));
            }
            if (duration <= 0)
            {
                throw new ArgumentOutOfRangeException("duration", duration, Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            Win32Native.Beep(frequency, duration);
        }

        private static bool BreakEvent(int controlType)
        {
            if ((controlType != 0) && (controlType != 1))
            {
                return false;
            }
            ConsoleCancelEventHandler cancelCallbacks = _cancelCallbacks;
            if (cancelCallbacks == null)
            {
                return false;
            }
            ConsoleSpecialKey controlKey = (controlType == 0) ? ConsoleSpecialKey.ControlC : ConsoleSpecialKey.ControlBreak;
            ControlCDelegateData state = new ControlCDelegateData(controlKey, cancelCallbacks);
            WaitCallback callBack = new WaitCallback(Console.ControlCDelegate);
            if (!ThreadPool.QueueUserWorkItem(callBack, state))
            {
                return false;
            }
            TimeSpan timeout = new TimeSpan(0, 0, 30);
            state.CompletionEvent.WaitOne(timeout, false);
            if (!state.DelegateStarted)
            {
                return false;
            }
            state.CompletionEvent.WaitOne();
            state.CompletionEvent.Close();
            return state.Cancel;
        }

        [SecuritySafeCritical]
        public static void Clear()
        {
            Win32Native.COORD dwWriteCoord = new Win32Native.COORD();
            IntPtr consoleOutputHandle = ConsoleOutputHandle;
            if (consoleOutputHandle == Win32Native.INVALID_HANDLE_VALUE)
            {
                throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
            }
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
            int nLength = bufferInfo.dwSize.X * bufferInfo.dwSize.Y;
            int pNumCharsWritten = 0;
            if (!Win32Native.FillConsoleOutputCharacter(consoleOutputHandle, ' ', nLength, dwWriteCoord, out pNumCharsWritten))
            {
                __Error.WinIOError();
            }
            pNumCharsWritten = 0;
            if (!Win32Native.FillConsoleOutputAttribute(consoleOutputHandle, bufferInfo.wAttributes, nLength, dwWriteCoord, out pNumCharsWritten))
            {
                __Error.WinIOError();
            }
            if (!Win32Native.SetConsoleCursorPosition(consoleOutputHandle, dwWriteCoord))
            {
                __Error.WinIOError();
            }
        }

        [SecurityCritical]
        private static ConsoleColor ColorAttributeToConsoleColor(Win32Native.Color c)
        {
            if (((short) (c & (Win32Native.Color.BackgroundBlue | Win32Native.Color.BackgroundGreen | Win32Native.Color.BackgroundIntensity | Win32Native.Color.BackgroundRed))) != 0)
            {
                c = (Win32Native.Color) ((short) (((short) c) >> 4));
            }
            return (ConsoleColor) c;
        }

        [SecurityCritical]
        private static Win32Native.Color ConsoleColorToColorAttribute(ConsoleColor color, bool isBackground)
        {
            if ((color & ~ConsoleColor.White) != ConsoleColor.Black)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidConsoleColor"));
            }
            Win32Native.Color color2 = (Win32Native.Color) ((short) color);
            if (isBackground)
            {
                color2 = (Win32Native.Color) ((short) (((short) color2) << 4));
            }
            return color2;
        }

        [SecuritySafeCritical]
        private static unsafe bool ConsoleHandleIsValid(SafeFileHandle handle)
        {
            int num;
            if (handle.IsInvalid)
            {
                return false;
            }
            byte bytes = 0x41;
            return (__ConsoleStream.WriteFile(handle, &bytes, 0, out num, IntPtr.Zero) != 0);
        }

        private static void ControlCDelegate(object data)
        {
            ControlCDelegateData data2 = (ControlCDelegateData) data;
            try
            {
                data2.DelegateStarted = true;
                ConsoleCancelEventArgs e = new ConsoleCancelEventArgs(data2.ControlKey);
                data2.CancelCallbacks(null, e);
                data2.Cancel = e.Cancel;
            }
            finally
            {
                data2.CompletionEvent.Set();
            }
        }

        [SecurityCritical]
        private static Win32Native.CONSOLE_SCREEN_BUFFER_INFO GetBufferInfo()
        {
            bool flag;
            return GetBufferInfo(true, out flag);
        }

        [SecuritySafeCritical]
        private static Win32Native.CONSOLE_SCREEN_BUFFER_INFO GetBufferInfo(bool throwOnNoConsole, out bool succeeded)
        {
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO console_screen_buffer_info;
            succeeded = false;
            IntPtr consoleOutputHandle = ConsoleOutputHandle;
            if (consoleOutputHandle == Win32Native.INVALID_HANDLE_VALUE)
            {
                if (throwOnNoConsole)
                {
                    throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
                }
                return new Win32Native.CONSOLE_SCREEN_BUFFER_INFO();
            }
            if (!Win32Native.GetConsoleScreenBufferInfo(consoleOutputHandle, out console_screen_buffer_info))
            {
                bool consoleScreenBufferInfo = Win32Native.GetConsoleScreenBufferInfo(Win32Native.GetStdHandle(-12), out console_screen_buffer_info);
                if (!consoleScreenBufferInfo)
                {
                    consoleScreenBufferInfo = Win32Native.GetConsoleScreenBufferInfo(Win32Native.GetStdHandle(-10), out console_screen_buffer_info);
                }
                if (!consoleScreenBufferInfo)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    if ((errorCode == 6) && !throwOnNoConsole)
                    {
                        return new Win32Native.CONSOLE_SCREEN_BUFFER_INFO();
                    }
                    __Error.WinIOError(errorCode, null);
                }
            }
            if (!_haveReadDefaultColors)
            {
                _defaultColors = (byte) (console_screen_buffer_info.wAttributes & 0xff);
                _haveReadDefaultColors = true;
            }
            succeeded = true;
            return console_screen_buffer_info;
        }

        [SecuritySafeCritical]
        private static Stream GetStandardFile(int stdHandleName, FileAccess access, int bufferSize)
        {
            SafeFileHandle handle = new SafeFileHandle(Win32Native.GetStdHandle(stdHandleName), false);
            if (handle.IsInvalid)
            {
                handle.SetHandleAsInvalid();
                return Stream.Null;
            }
            if ((stdHandleName != -10) && !ConsoleHandleIsValid(handle))
            {
                return Stream.Null;
            }
            return new __ConsoleStream(handle, access);
        }

        [SecuritySafeCritical]
        private static void InitializeStdOutError(bool stdout)
        {
            lock (InternalSyncObject)
            {
                if ((!stdout || (_out == null)) && (stdout || (_error == null)))
                {
                    Stream stream;
                    TextWriter writer = null;
                    if (stdout)
                    {
                        stream = OpenStandardOutput(0x100);
                    }
                    else
                    {
                        stream = OpenStandardError(0x100);
                    }
                    if (stream == Stream.Null)
                    {
                        writer = TextWriter.Synchronized(StreamWriter.Null);
                    }
                    else
                    {
                        Encoding encoding = Encoding.GetEncoding((int) Win32Native.GetConsoleOutputCP());
                        StreamWriter writer2 = new StreamWriter(stream, encoding, 0x100, false) {
                            HaveWrittenPreamble = true,
                            AutoFlush = true
                        };
                        writer = TextWriter.Synchronized(writer2);
                    }
                    if (stdout)
                    {
                        _out = writer;
                    }
                    else
                    {
                        _error = writer;
                    }
                }
            }
        }

        [SecurityCritical]
        private static bool IsAltKeyDown(Win32Native.InputRecord ir)
        {
            return ((ir.keyEvent.controlKeyState & 3) != 0);
        }

        [SecurityCritical]
        private static bool IsKeyDownEvent(Win32Native.InputRecord ir)
        {
            return ((ir.eventType == 1) && ir.keyEvent.keyDown);
        }

        [SecurityCritical]
        private static bool IsModKey(Win32Native.InputRecord ir)
        {
            short virtualKeyCode = ir.keyEvent.virtualKeyCode;
            if (((virtualKeyCode < 0x10) || (virtualKeyCode > 0x12)) && ((virtualKeyCode != 20) && (virtualKeyCode != 0x90)))
            {
                return (virtualKeyCode == 0x91);
            }
            return true;
        }

        [SecuritySafeCritical]
        public static void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop)
        {
            MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop, ' ', ConsoleColor.Black, BackgroundColor);
        }

        [SecuritySafeCritical]
        public static unsafe void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
        {
            if ((sourceForeColor < ConsoleColor.Black) || (sourceForeColor > ConsoleColor.White))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidConsoleColor"), "sourceForeColor");
            }
            if ((sourceBackColor < ConsoleColor.Black) || (sourceBackColor > ConsoleColor.White))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidConsoleColor"), "sourceBackColor");
            }
            Win32Native.COORD dwSize = GetBufferInfo().dwSize;
            if ((sourceLeft < 0) || (sourceLeft > dwSize.X))
            {
                throw new ArgumentOutOfRangeException("sourceLeft", sourceLeft, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            }
            if ((sourceTop < 0) || (sourceTop > dwSize.Y))
            {
                throw new ArgumentOutOfRangeException("sourceTop", sourceTop, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            }
            if ((sourceWidth < 0) || (sourceWidth > (dwSize.X - sourceLeft)))
            {
                throw new ArgumentOutOfRangeException("sourceWidth", sourceWidth, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            }
            if ((sourceHeight < 0) || (sourceTop > (dwSize.Y - sourceHeight)))
            {
                throw new ArgumentOutOfRangeException("sourceHeight", sourceHeight, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            }
            if ((targetLeft < 0) || (targetLeft > dwSize.X))
            {
                throw new ArgumentOutOfRangeException("targetLeft", targetLeft, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            }
            if ((targetTop < 0) || (targetTop > dwSize.Y))
            {
                throw new ArgumentOutOfRangeException("targetTop", targetTop, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            }
            if ((sourceWidth != 0) && (sourceHeight != 0))
            {
                bool flag;
                Win32Native.CHAR_INFO[] char_infoArray3;
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
                Win32Native.CHAR_INFO[] char_infoArray = new Win32Native.CHAR_INFO[sourceWidth * sourceHeight];
                dwSize.X = (short) sourceWidth;
                dwSize.Y = (short) sourceHeight;
                Win32Native.COORD bufferCoord = new Win32Native.COORD();
                Win32Native.SMALL_RECT readRegion = new Win32Native.SMALL_RECT {
                    Left = (short) sourceLeft,
                    Right = (short) ((sourceLeft + sourceWidth) - 1),
                    Top = (short) sourceTop,
                    Bottom = (short) ((sourceTop + sourceHeight) - 1)
                };
                fixed (Win32Native.CHAR_INFO* char_infoRef = char_infoArray)
                {
                    flag = Win32Native.ReadConsoleOutput(ConsoleOutputHandle, char_infoRef, dwSize, bufferCoord, ref readRegion);
                }
                if (!flag)
                {
                    __Error.WinIOError();
                }
                Win32Native.COORD dwWriteCoord = new Win32Native.COORD {
                    X = (short) sourceLeft
                };
                Win32Native.Color color = (Win32Native.Color) ((short) (ConsoleColorToColorAttribute(sourceBackColor, true) | ConsoleColorToColorAttribute(sourceForeColor, false)));
                short wColorAttribute = (short) color;
                for (int i = sourceTop; i < (sourceTop + sourceHeight); i++)
                {
                    int num2;
                    dwWriteCoord.Y = (short) i;
                    if (!Win32Native.FillConsoleOutputCharacter(ConsoleOutputHandle, sourceChar, sourceWidth, dwWriteCoord, out num2))
                    {
                        __Error.WinIOError();
                    }
                    if (!Win32Native.FillConsoleOutputAttribute(ConsoleOutputHandle, wColorAttribute, sourceWidth, dwWriteCoord, out num2))
                    {
                        __Error.WinIOError();
                    }
                }
                Win32Native.SMALL_RECT writeRegion = new Win32Native.SMALL_RECT {
                    Left = (short) targetLeft,
                    Right = (short) (targetLeft + sourceWidth),
                    Top = (short) targetTop,
                    Bottom = (short) (targetTop + sourceHeight)
                };
                if (((char_infoArray3 = char_infoArray) == null) || (char_infoArray3.Length == 0))
                {
                    char_infoRef2 = null;
                    goto Label_02C4;
                }
                fixed (Win32Native.CHAR_INFO* char_infoRef2 = char_infoArray3)
                {
                Label_02C4:
                    flag = Win32Native.WriteConsoleOutput(ConsoleOutputHandle, char_infoRef2, dwSize, bufferCoord, ref writeRegion);
                }
            }
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static Stream OpenStandardError()
        {
            return OpenStandardError(0x100);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static Stream OpenStandardError(int bufferSize)
        {
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            return GetStandardFile(-12, FileAccess.Write, bufferSize);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static Stream OpenStandardInput()
        {
            return OpenStandardInput(0x100);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static Stream OpenStandardInput(int bufferSize)
        {
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            return GetStandardFile(-10, FileAccess.Read, bufferSize);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static Stream OpenStandardOutput()
        {
            return OpenStandardOutput(0x100);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static Stream OpenStandardOutput(int bufferSize)
        {
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            return GetStandardFile(-11, FileAccess.Write, bufferSize);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static int Read()
        {
            return In.Read();
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static ConsoleKeyInfo ReadKey()
        {
            return ReadKey(false);
        }

        [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static ConsoleKeyInfo ReadKey(bool intercept)
        {
            Win32Native.InputRecord record;
            ControlKeyState state;
            int numEventsRead = -1;
            if (_cachedInputRecord.eventType == 1)
            {
                record = _cachedInputRecord;
                if (_cachedInputRecord.keyEvent.repeatCount == 0)
                {
                    _cachedInputRecord.eventType = -1;
                }
                else
                {
                    _cachedInputRecord.keyEvent.repeatCount = (short) (_cachedInputRecord.keyEvent.repeatCount - 1);
                }
                goto Label_0109;
            }
        Label_0053:
            if (!Win32Native.ReadConsoleInput(ConsoleInputHandle, out record, 1, out numEventsRead) || (numEventsRead == 0))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ConsoleReadKeyOnFile"));
            }
            short virtualKeyCode = record.keyEvent.virtualKeyCode;
            if ((!IsKeyDownEvent(record) && (virtualKeyCode != 0x12)) || ((record.keyEvent.uChar == '\0') && IsModKey(record)))
            {
                goto Label_0053;
            }
            ConsoleKey key = (ConsoleKey) virtualKeyCode;
            if (IsAltKeyDown(record) && (((key >= ConsoleKey.NumPad0) && (key <= ConsoleKey.NumPad9)) || (((key == ConsoleKey.Clear) || (key == ConsoleKey.Insert)) || ((key >= ConsoleKey.PageUp) && (key <= ConsoleKey.DownArrow)))))
            {
                goto Label_0053;
            }
            if (record.keyEvent.repeatCount > 1)
            {
                record.keyEvent.repeatCount = (short) (record.keyEvent.repeatCount - 1);
                _cachedInputRecord = record;
            }
        Label_0109:
            state = (ControlKeyState) record.keyEvent.controlKeyState;
            bool shift = (state & ControlKeyState.ShiftPressed) != 0;
            bool alt = (state & (ControlKeyState.LeftAltPressed | ControlKeyState.RightAltPressed)) != 0;
            bool control = (state & (ControlKeyState.LeftCtrlPressed | ControlKeyState.RightCtrlPressed)) != 0;
            ConsoleKeyInfo info = new ConsoleKeyInfo(record.keyEvent.uChar, (ConsoleKey) record.keyEvent.virtualKeyCode, shift, alt, control);
            if (!intercept)
            {
                Write(record.keyEvent.uChar);
            }
            return info;
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static string ReadLine()
        {
            return In.ReadLine();
        }

        [SecuritySafeCritical]
        public static void ResetColor()
        {
            bool flag;
            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
            GetBufferInfo(false, out flag);
            if (flag)
            {
                short attributes = _defaultColors;
                Win32Native.SetConsoleTextAttribute(ConsoleOutputHandle, attributes);
            }
        }

        [SecuritySafeCritical]
        public static void SetBufferSize(int width, int height)
        {
            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
            Win32Native.SMALL_RECT srWindow = GetBufferInfo().srWindow;
            if ((width < (srWindow.Right + 1)) || (width >= 0x7fff))
            {
                throw new ArgumentOutOfRangeException("width", width, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferLessThanWindowSize"));
            }
            if ((height < (srWindow.Bottom + 1)) || (height >= 0x7fff))
            {
                throw new ArgumentOutOfRangeException("height", height, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferLessThanWindowSize"));
            }
            Win32Native.COORD size = new Win32Native.COORD {
                X = (short) width,
                Y = (short) height
            };
            if (!Win32Native.SetConsoleScreenBufferSize(ConsoleOutputHandle, size))
            {
                __Error.WinIOError();
            }
        }

        [SecuritySafeCritical]
        public static void SetCursorPosition(int left, int top)
        {
            if ((left < 0) || (left >= 0x7fff))
            {
                throw new ArgumentOutOfRangeException("left", left, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            }
            if ((top < 0) || (top >= 0x7fff))
            {
                throw new ArgumentOutOfRangeException("top", top, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            }
            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
            IntPtr consoleOutputHandle = ConsoleOutputHandle;
            Win32Native.COORD cursorPosition = new Win32Native.COORD {
                X = (short) left,
                Y = (short) top
            };
            if (!Win32Native.SetConsoleCursorPosition(consoleOutputHandle, cursorPosition))
            {
                int errorCode = Marshal.GetLastWin32Error();
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
                if ((left < 0) || (left >= bufferInfo.dwSize.X))
                {
                    throw new ArgumentOutOfRangeException("left", left, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
                }
                if ((top < 0) || (top >= bufferInfo.dwSize.Y))
                {
                    throw new ArgumentOutOfRangeException("top", top, Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
                }
                __Error.WinIOError(errorCode, string.Empty);
            }
        }

        [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void SetError(TextWriter newError)
        {
            if (newError == null)
            {
                throw new ArgumentNullException("newError");
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            _wasErrorRedirected = true;
            newError = TextWriter.Synchronized(newError);
            lock (InternalSyncObject)
            {
                _error = newError;
            }
        }

        [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void SetIn(TextReader newIn)
        {
            if (newIn == null)
            {
                throw new ArgumentNullException("newIn");
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            newIn = TextReader.Synchronized(newIn);
            lock (InternalSyncObject)
            {
                _in = newIn;
            }
        }

        [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void SetOut(TextWriter newOut)
        {
            if (newOut == null)
            {
                throw new ArgumentNullException("newOut");
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            _wasOutRedirected = true;
            newOut = TextWriter.Synchronized(newOut);
            lock (InternalSyncObject)
            {
                _out = newOut;
            }
        }

        [SecuritySafeCritical]
        public static unsafe void SetWindowPosition(int left, int top)
        {
            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
            Win32Native.SMALL_RECT srWindow = bufferInfo.srWindow;
            int num = ((left + srWindow.Right) - srWindow.Left) + 1;
            if (((left < 0) || (num > bufferInfo.dwSize.X)) || (num < 0))
            {
                throw new ArgumentOutOfRangeException("left", left, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowPos"));
            }
            int num2 = ((top + srWindow.Bottom) - srWindow.Top) + 1;
            if (((top < 0) || (num2 > bufferInfo.dwSize.Y)) || (num2 < 0))
            {
                throw new ArgumentOutOfRangeException("top", top, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowPos"));
            }
            srWindow.Bottom = (short) (srWindow.Bottom - ((short) (srWindow.Top - top)));
            srWindow.Right = (short) (srWindow.Right - ((short) (srWindow.Left - left)));
            srWindow.Left = (short) left;
            srWindow.Top = (short) top;
            if (!Win32Native.SetConsoleWindowInfo(ConsoleOutputHandle, true, &srWindow))
            {
                __Error.WinIOError();
            }
        }

        [SecuritySafeCritical]
        public static unsafe void SetWindowSize(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException("width", width, Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("height", height, Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
            bool flag2 = false;
            Win32Native.COORD size = new Win32Native.COORD {
                X = bufferInfo.dwSize.X,
                Y = bufferInfo.dwSize.Y
            };
            if (bufferInfo.dwSize.X < (bufferInfo.srWindow.Left + width))
            {
                if (bufferInfo.srWindow.Left >= (0x7fff - width))
                {
                    throw new ArgumentOutOfRangeException("width", Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowBufferSize"));
                }
                size.X = (short) (bufferInfo.srWindow.Left + width);
                flag2 = true;
            }
            if (bufferInfo.dwSize.Y < (bufferInfo.srWindow.Top + height))
            {
                if (bufferInfo.srWindow.Top >= (0x7fff - height))
                {
                    throw new ArgumentOutOfRangeException("height", Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowBufferSize"));
                }
                size.Y = (short) (bufferInfo.srWindow.Top + height);
                flag2 = true;
            }
            if (flag2 && !Win32Native.SetConsoleScreenBufferSize(ConsoleOutputHandle, size))
            {
                __Error.WinIOError();
            }
            Win32Native.SMALL_RECT srWindow = bufferInfo.srWindow;
            srWindow.Bottom = (short) ((srWindow.Top + height) - 1);
            srWindow.Right = (short) ((srWindow.Left + width) - 1);
            if (!Win32Native.SetConsoleWindowInfo(ConsoleOutputHandle, true, &srWindow))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (flag2)
                {
                    Win32Native.SetConsoleScreenBufferSize(ConsoleOutputHandle, bufferInfo.dwSize);
                }
                Win32Native.COORD largestConsoleWindowSize = Win32Native.GetLargestConsoleWindowSize(ConsoleOutputHandle);
                if (width > largestConsoleWindowSize.X)
                {
                    throw new ArgumentOutOfRangeException("width", width, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowSize_Size", new object[] { largestConsoleWindowSize.X }));
                }
                if (height > largestConsoleWindowSize.Y)
                {
                    throw new ArgumentOutOfRangeException("height", height, Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowSize_Size", new object[] { largestConsoleWindowSize.Y }));
                }
                __Error.WinIOError(errorCode, string.Empty);
            }
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(bool value)
        {
            Out.Write(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(char value)
        {
            Out.Write(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(char[] buffer)
        {
            Out.Write(buffer);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(decimal value)
        {
            Out.Write(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(double value)
        {
            Out.Write(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(int value)
        {
            Out.Write(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(long value)
        {
            Out.Write(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(object value)
        {
            Out.Write(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(float value)
        {
            Out.Write(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(string value)
        {
            Out.Write(value);
        }

        [CLSCompliant(false), HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(uint value)
        {
            Out.Write(value);
        }

        [CLSCompliant(false), HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(ulong value)
        {
            Out.Write(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(string format, object arg0)
        {
            Out.Write(format, arg0);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(string format, params object[] arg)
        {
            if (arg == null)
            {
                Out.Write(format, null, null);
            }
            else
            {
                Out.Write(format, arg);
            }
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(char[] buffer, int index, int count)
        {
            Out.Write(buffer, index, count);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(string format, object arg0, object arg1)
        {
            Out.Write(format, arg0, arg1);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(string format, object arg0, object arg1, object arg2)
        {
            Out.Write(format, arg0, arg1, arg2);
        }

        [SecuritySafeCritical, CLSCompliant(false), HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void Write(string format, object arg0, object arg1, object arg2, object arg3, __arglist)
        {
            ArgIterator iterator = new ArgIterator(__arglist);
            int num = iterator.GetRemainingCount() + 4;
            object[] arg = new object[num];
            arg[0] = arg0;
            arg[1] = arg1;
            arg[2] = arg2;
            arg[3] = arg3;
            for (int i = 4; i < num; i++)
            {
                arg[i] = TypedReference.ToObject(iterator.GetNextArg());
            }
            Out.Write(format, arg);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine()
        {
            Out.WriteLine();
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(bool value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(char value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(char[] buffer)
        {
            Out.WriteLine(buffer);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(decimal value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(double value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(int value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(long value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(object value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(float value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(string value)
        {
            Out.WriteLine(value);
        }

        [CLSCompliant(false), HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(uint value)
        {
            Out.WriteLine(value);
        }

        [CLSCompliant(false), HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(ulong value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(string format, object arg0)
        {
            Out.WriteLine(format, arg0);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(string format, params object[] arg)
        {
            if (arg == null)
            {
                Out.WriteLine(format, null, null);
            }
            else
            {
                Out.WriteLine(format, arg);
            }
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(char[] buffer, int index, int count)
        {
            Out.WriteLine(buffer, index, count);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(string format, object arg0, object arg1)
        {
            Out.WriteLine(format, arg0, arg1);
        }

        [HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            Out.WriteLine(format, arg0, arg1, arg2);
        }

        [CLSCompliant(false), SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, UI=true)]
        public static void WriteLine(string format, object arg0, object arg1, object arg2, object arg3, __arglist)
        {
            ArgIterator iterator = new ArgIterator(__arglist);
            int num = iterator.GetRemainingCount() + 4;
            object[] arg = new object[num];
            arg[0] = arg0;
            arg[1] = arg1;
            arg[2] = arg2;
            arg[3] = arg3;
            for (int i = 4; i < num; i++)
            {
                arg[i] = TypedReference.ToObject(iterator.GetNextArg());
            }
            Out.WriteLine(format, arg);
        }

        public static ConsoleColor BackgroundColor
        {
            [SecuritySafeCritical]
            get
            {
                bool flag;
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo(false, out flag);
                if (!flag)
                {
                    return ConsoleColor.Black;
                }
                Win32Native.Color c = (Win32Native.Color) ((short) (bufferInfo.wAttributes & 240));
                return ColorAttributeToConsoleColor(c);
            }
            [SecuritySafeCritical]
            set
            {
                bool flag;
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
                Win32Native.Color color = ConsoleColorToColorAttribute(value, true);
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo(false, out flag);
                if (flag)
                {
                    short attributes = (short) (bufferInfo.wAttributes & -241);
                    attributes = (short) (((ushort) attributes) | ((ushort) color));
                    Win32Native.SetConsoleTextAttribute(ConsoleOutputHandle, attributes);
                }
            }
        }

        public static int BufferHeight
        {
            [SecuritySafeCritical]
            get
            {
                return GetBufferInfo().dwSize.Y;
            }
            [SecuritySafeCritical]
            set
            {
                SetBufferSize(BufferWidth, value);
            }
        }

        public static int BufferWidth
        {
            [SecuritySafeCritical]
            get
            {
                return GetBufferInfo().dwSize.X;
            }
            [SecuritySafeCritical]
            set
            {
                SetBufferSize(value, BufferHeight);
            }
        }

        public static bool CapsLock
        {
            [SecuritySafeCritical]
            get
            {
                return ((Win32Native.GetKeyState(20) & 1) == 1);
            }
        }

        private static IntPtr ConsoleInputHandle
        {
            [SecurityCritical]
            get
            {
                if (_consoleInputHandle == IntPtr.Zero)
                {
                    _consoleInputHandle = Win32Native.GetStdHandle(-10);
                }
                return _consoleInputHandle;
            }
        }

        private static IntPtr ConsoleOutputHandle
        {
            [SecurityCritical]
            get
            {
                if (_consoleOutputHandle == IntPtr.Zero)
                {
                    _consoleOutputHandle = Win32Native.GetStdHandle(-11);
                }
                return _consoleOutputHandle;
            }
        }

        public static int CursorLeft
        {
            [SecuritySafeCritical]
            get
            {
                return GetBufferInfo().dwCursorPosition.X;
            }
            [SecuritySafeCritical]
            set
            {
                SetCursorPosition(value, CursorTop);
            }
        }

        public static int CursorSize
        {
            [SecuritySafeCritical]
            get
            {
                Win32Native.CONSOLE_CURSOR_INFO console_cursor_info;
                if (!Win32Native.GetConsoleCursorInfo(ConsoleOutputHandle, out console_cursor_info))
                {
                    __Error.WinIOError();
                }
                return console_cursor_info.dwSize;
            }
            [SecuritySafeCritical]
            set
            {
                Win32Native.CONSOLE_CURSOR_INFO console_cursor_info;
                if ((value < 1) || (value > 100))
                {
                    throw new ArgumentOutOfRangeException("value", value, Environment.GetResourceString("ArgumentOutOfRange_CursorSize"));
                }
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
                IntPtr consoleOutputHandle = ConsoleOutputHandle;
                if (!Win32Native.GetConsoleCursorInfo(consoleOutputHandle, out console_cursor_info))
                {
                    __Error.WinIOError();
                }
                console_cursor_info.dwSize = value;
                if (!Win32Native.SetConsoleCursorInfo(consoleOutputHandle, ref console_cursor_info))
                {
                    __Error.WinIOError();
                }
            }
        }

        public static int CursorTop
        {
            [SecuritySafeCritical]
            get
            {
                return GetBufferInfo().dwCursorPosition.Y;
            }
            [SecuritySafeCritical]
            set
            {
                SetCursorPosition(CursorLeft, value);
            }
        }

        public static bool CursorVisible
        {
            [SecuritySafeCritical]
            get
            {
                Win32Native.CONSOLE_CURSOR_INFO console_cursor_info;
                if (!Win32Native.GetConsoleCursorInfo(ConsoleOutputHandle, out console_cursor_info))
                {
                    __Error.WinIOError();
                }
                return console_cursor_info.bVisible;
            }
            [SecuritySafeCritical]
            set
            {
                Win32Native.CONSOLE_CURSOR_INFO console_cursor_info;
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
                IntPtr consoleOutputHandle = ConsoleOutputHandle;
                if (!Win32Native.GetConsoleCursorInfo(consoleOutputHandle, out console_cursor_info))
                {
                    __Error.WinIOError();
                }
                console_cursor_info.bVisible = value;
                if (!Win32Native.SetConsoleCursorInfo(consoleOutputHandle, ref console_cursor_info))
                {
                    __Error.WinIOError();
                }
            }
        }

        public static TextWriter Error
        {
            [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, UI=true)]
            get
            {
                if (_error == null)
                {
                    InitializeStdOutError(false);
                }
                return _error;
            }
        }

        public static ConsoleColor ForegroundColor
        {
            [SecuritySafeCritical]
            get
            {
                bool flag;
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo(false, out flag);
                if (!flag)
                {
                    return ConsoleColor.Gray;
                }
                Win32Native.Color c = (Win32Native.Color) ((short) (bufferInfo.wAttributes & 15));
                return ColorAttributeToConsoleColor(c);
            }
            [SecuritySafeCritical]
            set
            {
                bool flag;
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
                Win32Native.Color color = ConsoleColorToColorAttribute(value, false);
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo(false, out flag);
                if (flag)
                {
                    short attributes = (short) (bufferInfo.wAttributes & -16);
                    attributes = (short) (((ushort) attributes) | ((ushort) color));
                    Win32Native.SetConsoleTextAttribute(ConsoleOutputHandle, attributes);
                }
            }
        }

        public static TextReader In
        {
            [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, UI=true)]
            get
            {
                if (_in == null)
                {
                    lock (InternalSyncObject)
                    {
                        if (_in == null)
                        {
                            TextReader @null;
                            Stream stream = OpenStandardInput(0x100);
                            if (stream == Stream.Null)
                            {
                                @null = StreamReader.Null;
                            }
                            else
                            {
                                Encoding encoding = Encoding.GetEncoding((int) Win32Native.GetConsoleCP());
                                @null = TextReader.Synchronized(new StreamReader(stream, encoding, false, 0x100, false));
                            }
                            Thread.MemoryBarrier();
                            _in = @null;
                        }
                    }
                }
                return _in;
            }
        }

        public static Encoding InputEncoding
        {
            [SecuritySafeCritical]
            get
            {
                lock (InternalSyncObject)
                {
                    if (_inputEncoding == null)
                    {
                        _inputEncoding = Encoding.GetEncoding((int) Win32Native.GetConsoleCP());
                    }
                    return _inputEncoding;
                }
            }
            [SecuritySafeCritical]
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
                uint codePage = (uint) value.CodePage;
                lock (InternalSyncObject)
                {
                    if (!Win32Native.SetConsoleCP(codePage))
                    {
                        __Error.WinIOError();
                    }
                    _inputEncoding = (Encoding) value.Clone();
                    _in = null;
                }
            }
        }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange<object>(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        public static bool KeyAvailable
        {
            [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, UI=true)]
            get
            {
                if (_cachedInputRecord.eventType == 1)
                {
                    return true;
                }
                Win32Native.InputRecord buffer = new Win32Native.InputRecord();
                int numEventsRead = 0;
                while (true)
                {
                    if (!Win32Native.PeekConsoleInput(ConsoleInputHandle, out buffer, 1, out numEventsRead))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        if (errorCode == 6)
                        {
                            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ConsoleKeyAvailableOnFile"));
                        }
                        __Error.WinIOError(errorCode, "stdin");
                    }
                    if (numEventsRead == 0)
                    {
                        return false;
                    }
                    if (IsKeyDownEvent(buffer) && !IsModKey(buffer))
                    {
                        return true;
                    }
                    if (!Win32Native.ReadConsoleInput(ConsoleInputHandle, out buffer, 1, out numEventsRead))
                    {
                        __Error.WinIOError();
                    }
                }
            }
        }

        public static int LargestWindowHeight
        {
            [SecuritySafeCritical]
            get
            {
                return Win32Native.GetLargestConsoleWindowSize(ConsoleOutputHandle).Y;
            }
        }

        public static int LargestWindowWidth
        {
            [SecuritySafeCritical]
            get
            {
                return Win32Native.GetLargestConsoleWindowSize(ConsoleOutputHandle).X;
            }
        }

        public static bool NumberLock
        {
            [SecuritySafeCritical]
            get
            {
                return ((Win32Native.GetKeyState(0x90) & 1) == 1);
            }
        }

        public static TextWriter Out
        {
            [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, UI=true)]
            get
            {
                if (_out == null)
                {
                    InitializeStdOutError(true);
                }
                return _out;
            }
        }

        public static Encoding OutputEncoding
        {
            [SecuritySafeCritical]
            get
            {
                lock (InternalSyncObject)
                {
                    if (_outputEncoding == null)
                    {
                        _outputEncoding = Encoding.GetEncoding((int) Win32Native.GetConsoleOutputCP());
                    }
                    return _outputEncoding;
                }
            }
            [SecuritySafeCritical]
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
                lock (InternalSyncObject)
                {
                    if ((_out != null) && !_wasOutRedirected)
                    {
                        _out.Flush();
                        _out = null;
                    }
                    if ((_error != null) && !_wasErrorRedirected)
                    {
                        _error.Flush();
                        _error = null;
                    }
                    if (!Win32Native.SetConsoleOutputCP((uint) value.CodePage))
                    {
                        __Error.WinIOError();
                    }
                    _outputEncoding = (Encoding) value.Clone();
                }
            }
        }

        public static string Title
        {
            [SecuritySafeCritical]
            get
            {
                StringBuilder sb = new StringBuilder(0x5fb5);
                Win32Native.SetLastError(0);
                int consoleTitle = Win32Native.GetConsoleTitle(sb, sb.Capacity);
                if (consoleTitle == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    if (errorCode == 0)
                    {
                        sb.Length = 0;
                    }
                    else
                    {
                        __Error.WinIOError(errorCode, string.Empty);
                    }
                }
                else if (consoleTitle > 0x5fb4)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("ArgumentOutOfRange_ConsoleTitleTooLong"));
                }
                return sb.ToString();
            }
            [SecuritySafeCritical]
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Length > 0x5fb4)
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_ConsoleTitleTooLong"));
                }
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
                if (!Win32Native.SetConsoleTitle(value))
                {
                    __Error.WinIOError();
                }
            }
        }

        public static bool TreatControlCAsInput
        {
            [SecuritySafeCritical]
            get
            {
                IntPtr consoleInputHandle = ConsoleInputHandle;
                if (consoleInputHandle == Win32Native.INVALID_HANDLE_VALUE)
                {
                    throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
                }
                int mode = 0;
                if (!Win32Native.GetConsoleMode(consoleInputHandle, out mode))
                {
                    __Error.WinIOError();
                }
                return ((mode & 1) == 0);
            }
            [SecuritySafeCritical]
            set
            {
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
                IntPtr consoleInputHandle = ConsoleInputHandle;
                if (consoleInputHandle == Win32Native.INVALID_HANDLE_VALUE)
                {
                    throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
                }
                int mode = 0;
                bool consoleMode = Win32Native.GetConsoleMode(consoleInputHandle, out mode);
                if (value)
                {
                    mode &= -2;
                }
                else
                {
                    mode |= 1;
                }
                if (!Win32Native.SetConsoleMode(consoleInputHandle, mode))
                {
                    __Error.WinIOError();
                }
            }
        }

        public static int WindowHeight
        {
            [SecuritySafeCritical]
            get
            {
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
                return ((bufferInfo.srWindow.Bottom - bufferInfo.srWindow.Top) + 1);
            }
            [SecuritySafeCritical]
            set
            {
                SetWindowSize(WindowWidth, value);
            }
        }

        public static int WindowLeft
        {
            [SecuritySafeCritical]
            get
            {
                return GetBufferInfo().srWindow.Left;
            }
            [SecuritySafeCritical]
            set
            {
                SetWindowPosition(value, WindowTop);
            }
        }

        public static int WindowTop
        {
            [SecuritySafeCritical]
            get
            {
                return GetBufferInfo().srWindow.Top;
            }
            [SecuritySafeCritical]
            set
            {
                SetWindowPosition(WindowLeft, value);
            }
        }

        public static int WindowWidth
        {
            [SecuritySafeCritical]
            get
            {
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO bufferInfo = GetBufferInfo();
                return ((bufferInfo.srWindow.Right - bufferInfo.srWindow.Left) + 1);
            }
            [SecuritySafeCritical]
            set
            {
                SetWindowSize(value, WindowHeight);
            }
        }

        private sealed class ControlCDelegateData
        {
            internal bool Cancel;
            internal ConsoleCancelEventHandler CancelCallbacks;
            internal ManualResetEvent CompletionEvent;
            internal ConsoleSpecialKey ControlKey;
            internal bool DelegateStarted;

            internal ControlCDelegateData(ConsoleSpecialKey controlKey, ConsoleCancelEventHandler cancelCallbacks)
            {
                this.ControlKey = controlKey;
                this.CancelCallbacks = cancelCallbacks;
                this.CompletionEvent = new ManualResetEvent(false);
            }
        }

        internal sealed class ControlCHooker : CriticalFinalizerObject
        {
            [SecurityCritical]
            private Win32Native.ConsoleCtrlHandlerRoutine _handler = new Win32Native.ConsoleCtrlHandlerRoutine(Console.BreakEvent);
            private bool _hooked;

            [SecurityCritical]
            internal ControlCHooker()
            {
            }

            ~ControlCHooker()
            {
                this.Unhook();
            }

            [SecuritySafeCritical]
            internal void Hook()
            {
                if (!this._hooked)
                {
                    if (!Win32Native.SetConsoleCtrlHandler(this._handler, true))
                    {
                        __Error.WinIOError();
                    }
                    this._hooked = true;
                }
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
            internal void Unhook()
            {
                if (this._hooked)
                {
                    if (!Win32Native.SetConsoleCtrlHandler(this._handler, false))
                    {
                        __Error.WinIOError();
                    }
                    this._hooked = false;
                }
            }
        }

        [Flags]
        internal enum ControlKeyState
        {
            CapsLockOn = 0x80,
            EnhancedKey = 0x100,
            LeftAltPressed = 2,
            LeftCtrlPressed = 8,
            NumLockOn = 0x20,
            RightAltPressed = 1,
            RightCtrlPressed = 4,
            ScrollLockOn = 0x40,
            ShiftPressed = 0x10
        }
    }
}

