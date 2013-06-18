namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Threading;

    internal sealed class DebugTraceHelper
    {
        private string name;
        private TraceSwitch traceSwitch;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DebugTraceHelper(string name, TraceSwitch traceSwitch)
        {
            this.name = name;
            this.traceSwitch = traceSwitch;
        }

        private string FormatMessage(string message)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} [{1,4:x8}] [{2}] : {3}", new object[] { DateTime.Now.ToString("u", DateTimeFormatInfo.InvariantInfo), Thread.CurrentThread.ManagedThreadId.ToString("x", CultureInfo.InvariantCulture), this.name, message });
        }

        public void Trace(TraceLevel level, string message)
        {
            if (this.TraceEnabled(level))
            {
                message = this.FormatMessage(message);
                System.Diagnostics.Trace.WriteLine(message);
            }
        }

        public void Trace(TraceLevel level, string message, object param0)
        {
            if (this.TraceEnabled(level))
            {
                message = string.Format(CultureInfo.InvariantCulture, message, new object[] { param0 });
                message = this.FormatMessage(message);
                System.Diagnostics.Trace.WriteLine(message);
            }
        }

        public void Trace(TraceLevel level, string message, object param0, object param1)
        {
            if (this.TraceEnabled(level))
            {
                message = string.Format(CultureInfo.InvariantCulture, message, new object[] { param0, param1 });
                message = this.FormatMessage(message);
                System.Diagnostics.Trace.WriteLine(message);
            }
        }

        public void Trace(TraceLevel level, string message, object param0, object param1, object param2)
        {
            if (this.TraceEnabled(level))
            {
                message = string.Format(CultureInfo.InvariantCulture, message, new object[] { param0, param1, param2 });
                message = this.FormatMessage(message);
                System.Diagnostics.Trace.WriteLine(message);
            }
        }

        public void Trace(TraceLevel level, string message, object param0, object param1, object param2, object param3)
        {
            if (this.TraceEnabled(level))
            {
                message = string.Format(CultureInfo.InvariantCulture, message, new object[] { param0, param1, param2, param3 });
                message = this.FormatMessage(message);
                System.Diagnostics.Trace.WriteLine(message);
            }
        }

        public bool TraceEnabled(TraceLevel level)
        {
            return (this.traceSwitch.Level >= level);
        }
    }
}

