namespace System.Drawing.Internal
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [UIPermission(SecurityAction.Assert, Unrestricted=true), SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), FileIOPermission(SecurityAction.Assert, Unrestricted=true), EnvironmentPermission(SecurityAction.Assert, Unrestricted=true), ReflectionPermission(SecurityAction.Assert, MemberAccess=true)]
    internal sealed class DbgUtil
    {
        public static int finalizeMaxFrameCount = 5;
        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        public const int FORMAT_MESSAGE_DEFAULT = 0x1200;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        public static int gdipInitMaxFrameCount = 8;
        public static int gdiUseMaxFrameCount = 8;

        [Conditional("DEBUG")]
        public static void AssertFinalization(object obj, bool disposing)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertWin32(bool expression, string message)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertWin32(bool expression, string format, object arg1)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertWin32(bool expression, string format, object arg1, object arg2)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertWin32(bool expression, string format, object arg1, object arg2, object arg3)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertWin32(bool expression, string format, object arg1, object arg2, object arg3, object arg4)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertWin32(bool expression, string format, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
        }

        [Conditional("DEBUG")]
        private static void AssertWin32Impl(bool expression, string format, object[] args)
        {
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int FormatMessage(int dwFlags, HandleRef lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, HandleRef arguments);
        public static string GetLastErrorStr()
        {
            int capacity = 0xff;
            StringBuilder lpBuffer = new StringBuilder(capacity);
            string str = string.Empty;
            int dwMessageId = 0;
            try
            {
                dwMessageId = Marshal.GetLastWin32Error();
                str = (FormatMessage(0x1200, new HandleRef(null, IntPtr.Zero), dwMessageId, GetUserDefaultLCID(), lpBuffer, capacity, new HandleRef(null, IntPtr.Zero)) != 0) ? lpBuffer.ToString() : "<error returned>";
            }
            catch (Exception exception)
            {
                if (IsCriticalException(exception))
                {
                    throw;
                }
                str = exception.ToString();
            }
            return string.Format(CultureInfo.CurrentCulture, "0x{0:x8} - {1}", new object[] { dwMessageId, str });
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetUserDefaultLCID();
        private static bool IsCriticalException(Exception ex)
        {
            return (((ex is StackOverflowException) || (ex is OutOfMemoryException)) || (ex is ThreadAbortException));
        }

        public static string StackFramesToStr()
        {
            return StackFramesToStr(gdipInitMaxFrameCount);
        }

        public static string StackFramesToStr(int maxFrameCount)
        {
            string str = string.Empty;
            try
            {
                System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(true);
                int index = 0;
                while (index < trace.FrameCount)
                {
                    StackFrame frame = trace.GetFrame(index);
                    if ((frame == null) || (frame.GetMethod().DeclaringType != typeof(DbgUtil)))
                    {
                        break;
                    }
                    index++;
                }
                maxFrameCount += index;
                if (maxFrameCount > trace.FrameCount)
                {
                    maxFrameCount = trace.FrameCount;
                }
                for (int i = index; i < maxFrameCount; i++)
                {
                    StackFrame frame2 = trace.GetFrame(i);
                    if (frame2 != null)
                    {
                        MethodBase method = frame2.GetMethod();
                        if (method != null)
                        {
                            string str2 = string.Empty;
                            string fileName = frame2.GetFileName();
                            int num3 = (fileName == null) ? -1 : fileName.LastIndexOf('\\');
                            if (num3 != -1)
                            {
                                fileName = fileName.Substring(num3 + 1, (fileName.Length - num3) - 1);
                            }
                            foreach (ParameterInfo info in method.GetParameters())
                            {
                                str2 = str2 + info.ParameterType.Name + ", ";
                            }
                            if (str2.Length > 0)
                            {
                                str2 = str2.Substring(0, str2.Length - 2);
                            }
                            str = str + string.Format(CultureInfo.CurrentCulture, "at {0} {1}.{2}({3})\r\n", new object[] { fileName, method.DeclaringType, method.Name, str2 });
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (IsCriticalException(exception))
                {
                    throw;
                }
                str = str + exception.ToString();
            }
            return str.ToString();
        }

        public static string StackTraceToStr(string message)
        {
            return StackTraceToStr(message, gdipInitMaxFrameCount);
        }

        public static string StackTraceToStr(string message, int frameCount)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}\r\nTop Stack Trace:\r\n{1}", new object[] { message, StackFramesToStr(frameCount) });
        }

        public static string StackTrace
        {
            get
            {
                return Environment.StackTrace;
            }
        }
    }
}

