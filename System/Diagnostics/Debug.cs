namespace System.Diagnostics
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;

    public static class Debug
    {
        [Conditional("DEBUG")]
        public static void Assert(bool condition)
        {
            TraceInternal.Assert(condition);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message)
        {
            TraceInternal.Assert(condition, message);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message, string detailMessage)
        {
            TraceInternal.Assert(condition, message, detailMessage);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message, string detailMessageFormat, params object[] args)
        {
            TraceInternal.Assert(condition, message, string.Format(CultureInfo.InvariantCulture, detailMessageFormat, args));
        }

        [Conditional("DEBUG"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static void Close()
        {
            TraceInternal.Close();
        }

        [Conditional("DEBUG")]
        public static void Fail(string message)
        {
            TraceInternal.Fail(message);
        }

        [Conditional("DEBUG")]
        public static void Fail(string message, string detailMessage)
        {
            TraceInternal.Fail(message, detailMessage);
        }

        [Conditional("DEBUG")]
        public static void Flush()
        {
            TraceInternal.Flush();
        }

        [Conditional("DEBUG")]
        public static void Indent()
        {
            TraceInternal.Indent();
        }

        [Conditional("DEBUG")]
        public static void Print(string message)
        {
            TraceInternal.WriteLine(message);
        }

        [Conditional("DEBUG")]
        public static void Print(string format, params object[] args)
        {
            TraceInternal.WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
        }

        [Conditional("DEBUG")]
        public static void Unindent()
        {
            TraceInternal.Unindent();
        }

        [Conditional("DEBUG")]
        public static void Write(object value)
        {
            TraceInternal.Write(value);
        }

        [Conditional("DEBUG")]
        public static void Write(string message)
        {
            TraceInternal.Write(message);
        }

        [Conditional("DEBUG")]
        public static void Write(object value, string category)
        {
            TraceInternal.Write(value, category);
        }

        [Conditional("DEBUG")]
        public static void Write(string message, string category)
        {
            TraceInternal.Write(message, category);
        }

        [Conditional("DEBUG")]
        public static void WriteIf(bool condition, object value)
        {
            TraceInternal.WriteIf(condition, value);
        }

        [Conditional("DEBUG")]
        public static void WriteIf(bool condition, string message)
        {
            TraceInternal.WriteIf(condition, message);
        }

        [Conditional("DEBUG")]
        public static void WriteIf(bool condition, object value, string category)
        {
            TraceInternal.WriteIf(condition, value, category);
        }

        [Conditional("DEBUG")]
        public static void WriteIf(bool condition, string message, string category)
        {
            TraceInternal.WriteIf(condition, message, category);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(object value)
        {
            TraceInternal.WriteLine(value);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string message)
        {
            TraceInternal.WriteLine(message);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(object value, string category)
        {
            TraceInternal.WriteLine(value, category);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string message, string category)
        {
            TraceInternal.WriteLine(message, category);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args)
        {
            TraceInternal.WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
        }

        [Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, object value)
        {
            TraceInternal.WriteLineIf(condition, value);
        }

        [Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, string message)
        {
            TraceInternal.WriteLineIf(condition, message);
        }

        [Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, object value, string category)
        {
            TraceInternal.WriteLineIf(condition, value, category);
        }

        [Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, string message, string category)
        {
            TraceInternal.WriteLineIf(condition, message, category);
        }

        public static bool AutoFlush
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                return TraceInternal.AutoFlush;
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            set
            {
                TraceInternal.AutoFlush = value;
            }
        }

        public static int IndentLevel
        {
            get
            {
                return TraceInternal.IndentLevel;
            }
            set
            {
                TraceInternal.IndentLevel = value;
            }
        }

        public static int IndentSize
        {
            get
            {
                return TraceInternal.IndentSize;
            }
            set
            {
                TraceInternal.IndentSize = value;
            }
        }

        public static TraceListenerCollection Listeners
        {
            [HostProtection(SecurityAction.LinkDemand, SharedState=true), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                return TraceInternal.Listeners;
            }
        }
    }
}

