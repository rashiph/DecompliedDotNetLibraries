namespace System.Diagnostics
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public sealed class EventLogTraceListener : TraceListener
    {
        private System.Diagnostics.EventLog eventLog;
        private bool nameSet;

        public EventLogTraceListener()
        {
        }

        public EventLogTraceListener(System.Diagnostics.EventLog eventLog) : base((eventLog != null) ? eventLog.Source : string.Empty)
        {
            this.eventLog = eventLog;
        }

        public EventLogTraceListener(string source)
        {
            this.eventLog = new System.Diagnostics.EventLog();
            this.eventLog.Source = source;
        }

        public override void Close()
        {
            if (this.eventLog != null)
            {
                this.eventLog.Close();
            }
        }

        private EventInstance CreateEventInstance(TraceEventType severity, int id)
        {
            if (id > 0xffff)
            {
                id = 0xffff;
            }
            if (id < 0)
            {
                id = 0;
            }
            EventInstance instance = new EventInstance((long) id, 0);
            if ((severity == TraceEventType.Error) || (severity == TraceEventType.Critical))
            {
                instance.EntryType = EventLogEntryType.Error;
                return instance;
            }
            if (severity == TraceEventType.Warning)
            {
                instance.EntryType = EventLogEntryType.Warning;
                return instance;
            }
            instance.EntryType = EventLogEntryType.Information;
            return instance;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.Close();
                }
                else
                {
                    if (this.eventLog != null)
                    {
                        this.eventLog.Close();
                    }
                    this.eventLog = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        [ComVisible(false)]
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType severity, int id, object data)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, severity, id, null, null, data))
            {
                EventInstance instance = this.CreateEventInstance(severity, id);
                this.eventLog.WriteEvent(instance, new object[] { data });
            }
        }

        [ComVisible(false)]
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType severity, int id, params object[] data)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, severity, id, null, null, null, data))
            {
                EventInstance instance = this.CreateEventInstance(severity, id);
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
                this.eventLog.WriteEvent(instance, new object[] { builder.ToString() });
            }
        }

        [ComVisible(false)]
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType severity, int id, string message)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, severity, id, message))
            {
                EventInstance instance = this.CreateEventInstance(severity, id);
                this.eventLog.WriteEvent(instance, new object[] { message });
            }
        }

        [ComVisible(false)]
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType severity, int id, string format, params object[] args)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, severity, id, format, args))
            {
                EventInstance instance = this.CreateEventInstance(severity, id);
                if (args == null)
                {
                    this.eventLog.WriteEvent(instance, new object[] { format });
                }
                else if (string.IsNullOrEmpty(format))
                {
                    string[] values = new string[args.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        values[i] = args[i].ToString();
                    }
                    this.eventLog.WriteEvent(instance, values);
                }
                else
                {
                    this.eventLog.WriteEvent(instance, new object[] { string.Format(CultureInfo.InvariantCulture, format, args) });
                }
            }
        }

        public override void Write(string message)
        {
            if (this.eventLog != null)
            {
                this.eventLog.WriteEntry(message);
            }
        }

        public override void WriteLine(string message)
        {
            this.Write(message);
        }

        public System.Diagnostics.EventLog EventLog
        {
            get
            {
                return this.eventLog;
            }
            set
            {
                this.eventLog = value;
            }
        }

        public override string Name
        {
            get
            {
                if (!this.nameSet && (this.eventLog != null))
                {
                    this.nameSet = true;
                    base.Name = this.eventLog.Source;
                }
                return base.Name;
            }
            set
            {
                this.nameSet = true;
                base.Name = value;
            }
        }
    }
}

