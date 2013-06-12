namespace System.Diagnostics
{
    using System;
    using System.Security.Permissions;

    public sealed class Trace
    {
        private static System.Diagnostics.CorrelationManager correlationManager;

        private Trace()
        {
        }

        [Conditional("TRACE")]
        public static void Assert(bool condition)
        {
            TraceInternal.Assert(condition);
        }

        [Conditional("TRACE")]
        public static void Assert(bool condition, string message)
        {
            TraceInternal.Assert(condition, message);
        }

        [Conditional("TRACE")]
        public static void Assert(bool condition, string message, string detailMessage)
        {
            TraceInternal.Assert(condition, message, detailMessage);
        }

        [Conditional("TRACE")]
        public static void Close()
        {
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            TraceInternal.Close();
        }

        [Conditional("TRACE")]
        public static void Fail(string message)
        {
            TraceInternal.Fail(message);
        }

        [Conditional("TRACE")]
        public static void Fail(string message, string detailMessage)
        {
            TraceInternal.Fail(message, detailMessage);
        }

        [Conditional("TRACE")]
        public static void Flush()
        {
            TraceInternal.Flush();
        }

        [Conditional("TRACE")]
        public static void Indent()
        {
            TraceInternal.Indent();
        }

        public static void Refresh()
        {
            DiagnosticsConfiguration.Refresh();
            Switch.RefreshAll();
            TraceSource.RefreshAll();
            TraceInternal.Refresh();
        }

        [Conditional("TRACE")]
        public static void TraceError(string message)
        {
            TraceInternal.TraceEvent(TraceEventType.Error, 0, message, null);
        }

        [Conditional("TRACE")]
        public static void TraceError(string format, params object[] args)
        {
            TraceInternal.TraceEvent(TraceEventType.Error, 0, format, args);
        }

        [Conditional("TRACE")]
        public static void TraceInformation(string message)
        {
            TraceInternal.TraceEvent(TraceEventType.Information, 0, message, null);
        }

        [Conditional("TRACE")]
        public static void TraceInformation(string format, params object[] args)
        {
            TraceInternal.TraceEvent(TraceEventType.Information, 0, format, args);
        }

        [Conditional("TRACE")]
        public static void TraceWarning(string message)
        {
            TraceInternal.TraceEvent(TraceEventType.Warning, 0, message, null);
        }

        [Conditional("TRACE")]
        public static void TraceWarning(string format, params object[] args)
        {
            TraceInternal.TraceEvent(TraceEventType.Warning, 0, format, args);
        }

        [Conditional("TRACE")]
        public static void Unindent()
        {
            TraceInternal.Unindent();
        }

        [Conditional("TRACE")]
        public static void Write(object value)
        {
            TraceInternal.Write(value);
        }

        [Conditional("TRACE")]
        public static void Write(string message)
        {
            TraceInternal.Write(message);
        }

        [Conditional("TRACE")]
        public static void Write(object value, string category)
        {
            TraceInternal.Write(value, category);
        }

        [Conditional("TRACE")]
        public static void Write(string message, string category)
        {
            TraceInternal.Write(message, category);
        }

        [Conditional("TRACE")]
        public static void WriteIf(bool condition, object value)
        {
            TraceInternal.WriteIf(condition, value);
        }

        [Conditional("TRACE")]
        public static void WriteIf(bool condition, string message)
        {
            TraceInternal.WriteIf(condition, message);
        }

        [Conditional("TRACE")]
        public static void WriteIf(bool condition, object value, string category)
        {
            TraceInternal.WriteIf(condition, value, category);
        }

        [Conditional("TRACE")]
        public static void WriteIf(bool condition, string message, string category)
        {
            TraceInternal.WriteIf(condition, message, category);
        }

        [Conditional("TRACE")]
        public static void WriteLine(object value)
        {
            TraceInternal.WriteLine(value);
        }

        [Conditional("TRACE")]
        public static void WriteLine(string message)
        {
            TraceInternal.WriteLine(message);
        }

        [Conditional("TRACE")]
        public static void WriteLine(object value, string category)
        {
            TraceInternal.WriteLine(value, category);
        }

        [Conditional("TRACE")]
        public static void WriteLine(string message, string category)
        {
            TraceInternal.WriteLine(message, category);
        }

        [Conditional("TRACE")]
        public static void WriteLineIf(bool condition, object value)
        {
            TraceInternal.WriteLineIf(condition, value);
        }

        [Conditional("TRACE")]
        public static void WriteLineIf(bool condition, string message)
        {
            TraceInternal.WriteLineIf(condition, message);
        }

        [Conditional("TRACE")]
        public static void WriteLineIf(bool condition, object value, string category)
        {
            TraceInternal.WriteLineIf(condition, value, category);
        }

        [Conditional("TRACE")]
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

        public static System.Diagnostics.CorrelationManager CorrelationManager
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                if (correlationManager == null)
                {
                    correlationManager = new System.Diagnostics.CorrelationManager();
                }
                return correlationManager;
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
            [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
            get
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                return TraceInternal.Listeners;
            }
        }

        public static bool UseGlobalLock
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                return TraceInternal.UseGlobalLock;
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            set
            {
                TraceInternal.UseGlobalLock = value;
            }
        }
    }
}

