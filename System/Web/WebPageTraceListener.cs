namespace System.Web
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public class WebPageTraceListener : TraceListener
    {
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType severity, int id, string message)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, severity, id, message, null, null, null))
            {
                HttpContext current = HttpContext.Current;
                if (current != null)
                {
                    string str = string.Concat(new object[] { System.Web.SR.GetString("WebPageTraceListener_Event"), " ", id, ": ", message });
                    if (severity <= TraceEventType.Warning)
                    {
                        current.Trace.WarnInternal(source, str, false);
                    }
                    else
                    {
                        current.Trace.WriteInternal(source, str, false);
                    }
                }
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType severity, int id, string format, params object[] args)
        {
            this.TraceEvent(eventCache, source, severity, id, string.Format(CultureInfo.InvariantCulture, format, args));
        }

        public override void Write(string message)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(null, string.Empty, TraceEventType.Verbose, 0, message, null, null, null))
            {
                HttpContext current = HttpContext.Current;
                if (current != null)
                {
                    current.Trace.WriteInternal(message, false);
                }
            }
        }

        public override void Write(string message, string category)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(null, string.Empty, TraceEventType.Verbose, 0, message, null, null, null))
            {
                HttpContext current = HttpContext.Current;
                if (current != null)
                {
                    current.Trace.WriteInternal(category, message, false);
                }
            }
        }

        public override void WriteLine(string message)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(null, string.Empty, TraceEventType.Verbose, 0, message, null, null, null))
            {
                HttpContext current = HttpContext.Current;
                if (current != null)
                {
                    current.Trace.WriteInternal(message, false);
                }
            }
        }

        public override void WriteLine(string message, string category)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(null, string.Empty, TraceEventType.Verbose, 0, message, null, null, null))
            {
                HttpContext current = HttpContext.Current;
                if (current != null)
                {
                    current.Trace.WriteInternal(category, message, false);
                }
            }
        }
    }
}

