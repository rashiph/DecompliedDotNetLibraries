namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public class DelimitedListTraceListener : TextWriterTraceListener
    {
        private string delimiter;
        private bool initializedDelim;
        private string secondaryDelim;

        public DelimitedListTraceListener(Stream stream) : base(stream)
        {
            this.delimiter = ";";
            this.secondaryDelim = ",";
        }

        public DelimitedListTraceListener(TextWriter writer) : base(writer)
        {
            this.delimiter = ";";
            this.secondaryDelim = ",";
        }

        public DelimitedListTraceListener(string fileName) : base(fileName)
        {
            this.delimiter = ";";
            this.secondaryDelim = ",";
        }

        public DelimitedListTraceListener(Stream stream, string name) : base(stream, name)
        {
            this.delimiter = ";";
            this.secondaryDelim = ",";
        }

        public DelimitedListTraceListener(TextWriter writer, string name) : base(writer, name)
        {
            this.delimiter = ";";
            this.secondaryDelim = ",";
        }

        public DelimitedListTraceListener(string fileName, string name) : base(fileName, name)
        {
            this.delimiter = ";";
            this.secondaryDelim = ",";
        }

        protected internal override string[] GetSupportedAttributes()
        {
            return new string[] { "delimiter" };
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data))
            {
                this.WriteHeader(source, eventType, id);
                this.Write(this.Delimiter);
                this.WriteEscaped(data.ToString());
                this.Write(this.Delimiter);
                this.WriteFooter(eventCache);
            }
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
            {
                this.WriteHeader(source, eventType, id);
                this.Write(this.Delimiter);
                if (data != null)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (i != 0)
                        {
                            this.Write(this.secondaryDelim);
                        }
                        this.WriteEscaped(data[i].ToString());
                    }
                }
                this.Write(this.Delimiter);
                this.WriteFooter(eventCache);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, message))
            {
                this.WriteHeader(source, eventType, id);
                this.WriteEscaped(message);
                this.Write(this.Delimiter);
                this.Write(this.Delimiter);
                this.WriteFooter(eventCache);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, format, args))
            {
                this.WriteHeader(source, eventType, id);
                if (args != null)
                {
                    this.WriteEscaped(string.Format(CultureInfo.InvariantCulture, format, args));
                }
                else
                {
                    this.WriteEscaped(format);
                }
                this.Write(this.Delimiter);
                this.Write(this.Delimiter);
                this.WriteFooter(eventCache);
            }
        }

        private void WriteEscaped(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                int num;
                StringBuilder builder = new StringBuilder("\"");
                int startIndex = 0;
                while ((num = message.IndexOf('"', startIndex)) != -1)
                {
                    builder.Append(message, startIndex, num - startIndex);
                    builder.Append("\"\"");
                    startIndex = num + 1;
                }
                builder.Append(message, startIndex, message.Length - startIndex);
                builder.Append("\"");
                this.Write(builder.ToString());
            }
        }

        private void WriteFooter(TraceEventCache eventCache)
        {
            if (eventCache != null)
            {
                if (base.IsEnabled(TraceOptions.ProcessId))
                {
                    this.Write(eventCache.ProcessId.ToString(CultureInfo.InvariantCulture));
                }
                this.Write(this.Delimiter);
                if (base.IsEnabled(TraceOptions.LogicalOperationStack))
                {
                    this.WriteStackEscaped(eventCache.LogicalOperationStack);
                }
                this.Write(this.Delimiter);
                if (base.IsEnabled(TraceOptions.ThreadId))
                {
                    this.WriteEscaped(eventCache.ThreadId.ToString(CultureInfo.InvariantCulture));
                }
                this.Write(this.Delimiter);
                if (base.IsEnabled(TraceOptions.DateTime))
                {
                    this.WriteEscaped(eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
                }
                this.Write(this.Delimiter);
                if (base.IsEnabled(TraceOptions.Timestamp))
                {
                    this.Write(eventCache.Timestamp.ToString(CultureInfo.InvariantCulture));
                }
                this.Write(this.Delimiter);
                if (base.IsEnabled(TraceOptions.Callstack))
                {
                    this.WriteEscaped(eventCache.Callstack);
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    this.Write(this.Delimiter);
                }
            }
            this.WriteLine("");
        }

        private void WriteHeader(string source, TraceEventType eventType, int id)
        {
            this.WriteEscaped(source);
            this.Write(this.Delimiter);
            this.Write(eventType.ToString());
            this.Write(this.Delimiter);
            this.Write(id.ToString(CultureInfo.InvariantCulture));
            this.Write(this.Delimiter);
        }

        private void WriteStackEscaped(Stack stack)
        {
            StringBuilder builder = new StringBuilder("\"");
            bool flag = true;
            foreach (object obj2 in stack)
            {
                int num;
                if (!flag)
                {
                    builder.Append(", ");
                }
                else
                {
                    flag = false;
                }
                string str = obj2.ToString();
                int startIndex = 0;
                while ((num = str.IndexOf('"', startIndex)) != -1)
                {
                    builder.Append(str, startIndex, num - startIndex);
                    builder.Append("\"\"");
                    startIndex = num + 1;
                }
                builder.Append(str, startIndex, str.Length - startIndex);
            }
            builder.Append("\"");
            this.Write(builder.ToString());
        }

        public string Delimiter
        {
            get
            {
                lock (this)
                {
                    if (!this.initializedDelim)
                    {
                        if (base.Attributes.ContainsKey("delimiter"))
                        {
                            this.delimiter = base.Attributes["delimiter"];
                        }
                        this.initializedDelim = true;
                    }
                }
                return this.delimiter;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Delimiter");
                }
                if (value.Length == 0)
                {
                    throw new ArgumentException(SR.GetString("Generic_ArgCantBeEmptyString", new object[] { "Delimiter" }));
                }
                lock (this)
                {
                    this.delimiter = value;
                    this.initializedDelim = true;
                }
                if (this.delimiter == ",")
                {
                    this.secondaryDelim = ";";
                }
                else
                {
                    this.secondaryDelim = ",";
                }
            }
        }
    }
}

