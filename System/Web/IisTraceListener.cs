namespace System.Web
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Hosting;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public sealed class IisTraceListener : TraceListener
    {
        public IisTraceListener()
        {
            HttpContext current = HttpContext.Current;
            if (((current != null) && !HttpRuntime.UseIntegratedPipeline) && !(current.WorkerRequest is ISAPIWorkerRequestInProcForIIS7))
            {
                throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_7"));
            }
        }

        private string AppendTraceOptions(TraceEventCache eventCache, string message)
        {
            if ((eventCache == null) || (base.TraceOutputOptions == TraceOptions.None))
            {
                return message;
            }
            StringBuilder builder = new StringBuilder(message, 0x400);
            if (this.IsEnabled(TraceOptions.ProcessId))
            {
                builder.Append("\r\nProcessId=");
                builder.Append(eventCache.ProcessId);
            }
            if (this.IsEnabled(TraceOptions.LogicalOperationStack))
            {
                builder.Append("\r\nLogicalOperationStack=");
                bool flag = true;
                foreach (object obj2 in eventCache.LogicalOperationStack)
                {
                    if (!flag)
                    {
                        builder.Append(", ");
                    }
                    else
                    {
                        flag = false;
                    }
                    builder.Append(obj2);
                }
            }
            if (this.IsEnabled(TraceOptions.ThreadId))
            {
                builder.Append("\r\nThreadId=");
                builder.Append(eventCache.ThreadId);
            }
            if (this.IsEnabled(TraceOptions.DateTime))
            {
                builder.Append("\r\nDateTime=");
                builder.Append(eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
            }
            if (this.IsEnabled(TraceOptions.Timestamp))
            {
                builder.Append("\r\nTimestamp=");
                builder.Append(eventCache.Timestamp);
            }
            if (this.IsEnabled(TraceOptions.Callstack))
            {
                builder.Append("\r\nCallstack=");
                builder.Append(eventCache.Callstack);
            }
            return builder.ToString();
        }

        private IntegratedTraceType Convert(TraceEventType tet)
        {
            switch (tet)
            {
                case TraceEventType.Verbose:
                    return IntegratedTraceType.DiagVerbose;

                case TraceEventType.Start:
                    return IntegratedTraceType.DiagStart;

                case TraceEventType.Critical:
                    return IntegratedTraceType.DiagCritical;

                case TraceEventType.Error:
                    return IntegratedTraceType.DiagError;

                case TraceEventType.Warning:
                    return IntegratedTraceType.DiagWarning;

                case TraceEventType.Information:
                    return IntegratedTraceType.DiagInfo;

                case TraceEventType.Stop:
                    return IntegratedTraceType.DiagStop;

                case TraceEventType.Suspend:
                    return IntegratedTraceType.DiagSuspend;

                case TraceEventType.Resume:
                    return IntegratedTraceType.DiagResume;

                case TraceEventType.Transfer:
                    return IntegratedTraceType.DiagTransfer;
            }
            return IntegratedTraceType.DiagVerbose;
        }

        private bool IsEnabled(TraceOptions opts)
        {
            return ((opts & base.TraceOutputOptions) != TraceOptions.None);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
            {
                HttpContext current = HttpContext.Current;
                if (current != null)
                {
                    string message = string.Empty;
                    if (data != null)
                    {
                        message = data.ToString();
                    }
                    current.WorkerRequest.RaiseTraceEvent(this.Convert(eventType), this.AppendTraceOptions(eventCache, message));
                }
            }
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            HttpContext current = HttpContext.Current;
            if ((current != null) && ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data)))
            {
                StringBuilder builder = new StringBuilder();
                if (data != null)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (i != 0)
                        {
                            builder.Append(", ");
                        }
                        if (data[i] != null)
                        {
                            builder.Append(data[i].ToString());
                        }
                    }
                }
                if (current != null)
                {
                    current.WorkerRequest.RaiseTraceEvent(this.Convert(eventType), this.AppendTraceOptions(eventCache, builder.ToString()));
                }
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType severity, int id, string message)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, severity, id, message, null, null, null))
            {
                HttpContext current = HttpContext.Current;
                if (current != null)
                {
                    current.WorkerRequest.RaiseTraceEvent(this.Convert(severity), this.AppendTraceOptions(eventCache, message));
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
                    current.WorkerRequest.RaiseTraceEvent(IntegratedTraceType.TraceWrite, message);
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
                    current.WorkerRequest.RaiseTraceEvent(IntegratedTraceType.TraceWrite, message);
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
                    current.WorkerRequest.RaiseTraceEvent(IntegratedTraceType.TraceWrite, message);
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
                    current.WorkerRequest.RaiseTraceEvent(IntegratedTraceType.TraceWrite, message);
                }
            }
        }
    }
}

