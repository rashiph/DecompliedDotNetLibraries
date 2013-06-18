namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Diagnostics;
    using System.Runtime;

    internal static class DebugTrace
    {
        private static DebugTraceHelper instance = new DebugTraceHelper("WSAT", new TraceSwitch("Microsoft.Transactions.Wsat", "Tracing for WS-AT protocol"));
        private static bool tracePiiEnabled = false;

        private static string FormatEnlistmentTrace(Guid enlistmentId, string text)
        {
            return (enlistmentId.ToString() + " " + text);
        }

        public static void Trace(TraceLevel level, string text)
        {
            instance.Trace(level, text);
        }

        public static void Trace(TraceLevel level, string text, object arg0)
        {
            instance.Trace(level, text, arg0);
        }

        public static void Trace(TraceLevel level, string text, object arg0, object arg1)
        {
            instance.Trace(level, text, arg0, arg1);
        }

        public static void Trace(TraceLevel level, string text, object arg0, object arg1, object arg2)
        {
            instance.Trace(level, text, arg0, arg1, arg2);
        }

        public static void Trace(TraceLevel level, string text, object arg0, object arg1, object arg2, object arg3)
        {
            instance.Trace(level, text, arg0, arg1, arg2, arg3);
        }

        public static void TraceEnter(string function)
        {
            if (instance.TraceEnabled(TraceLevel.Verbose))
            {
                instance.Trace(TraceLevel.Verbose, "Enter {0}", function);
            }
        }

        public static void TraceEnter(object obj, string function)
        {
            if (instance.TraceEnabled(TraceLevel.Verbose))
            {
                instance.Trace(TraceLevel.Verbose, "Enter {0}.{1}", obj.GetType().Name, function);
            }
        }

        public static void TraceLeave(string function)
        {
            if (instance.TraceEnabled(TraceLevel.Verbose))
            {
                instance.Trace(TraceLevel.Verbose, "Leave {0}", function);
            }
        }

        public static void TraceLeave(object obj, string function)
        {
            if (instance.TraceEnabled(TraceLevel.Verbose))
            {
                instance.Trace(TraceLevel.Verbose, "Leave {0}.{1}", obj.GetType().Name, function);
            }
        }

        public static void TracePii(TraceLevel level, string text)
        {
            if (tracePiiEnabled)
            {
                instance.Trace(level, text);
            }
        }

        public static void TracePii(TraceLevel level, string text, object arg0)
        {
            if (tracePiiEnabled)
            {
                instance.Trace(level, text, arg0);
            }
        }

        public static void TraceSendFailure(Exception e)
        {
            if (instance.TraceEnabled(TraceLevel.Warning))
            {
                Trace(TraceLevel.Warning, "Failure sending message: {0}", e.Message);
            }
        }

        public static void TraceSendFailure(Guid enlistmentId, Exception e)
        {
            if (instance.TraceEnabled(TraceLevel.Warning))
            {
                TxTrace(TraceLevel.Warning, enlistmentId, "Failure sending message: {0}", e.Message);
            }
        }

        public static void TxTrace(TraceLevel level, Guid enlistmentId, string text)
        {
            instance.Trace(level, FormatEnlistmentTrace(enlistmentId, text));
        }

        public static void TxTrace(TraceLevel level, Guid enlistmentId, string text, object arg0)
        {
            instance.Trace(level, FormatEnlistmentTrace(enlistmentId, text), arg0);
        }

        public static void TxTrace(TraceLevel level, Guid enlistmentId, string text, object arg0, object arg1)
        {
            instance.Trace(level, FormatEnlistmentTrace(enlistmentId, text), arg0, arg1);
        }

        public static void TxTrace(TraceLevel level, Guid enlistmentId, string text, object arg0, object arg1, object arg2)
        {
            instance.Trace(level, FormatEnlistmentTrace(enlistmentId, text), arg0, arg1, arg2);
        }

        public static bool Error
        {
            get
            {
                return instance.TraceEnabled(TraceLevel.Error);
            }
        }

        public static bool Info
        {
            get
            {
                return instance.TraceEnabled(TraceLevel.Info);
            }
        }

        public static bool Pii
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return tracePiiEnabled;
            }
            set
            {
                if (value)
                {
                    Trace(TraceLevel.Warning, "PII tracing is enabled");
                }
                tracePiiEnabled = value;
            }
        }

        public static bool Verbose
        {
            get
            {
                return instance.TraceEnabled(TraceLevel.Verbose);
            }
        }

        public static bool Warning
        {
            get
            {
                return instance.TraceEnabled(TraceLevel.Warning);
            }
        }
    }
}

