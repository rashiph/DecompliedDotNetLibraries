namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public abstract class TraceListener : MarshalByRefObject, IDisposable
    {
        private StringDictionary attributes;
        private TraceFilter filter;
        private int indentLevel;
        private int indentSize;
        internal string initializeData;
        private string listenerName;
        private bool needIndent;
        private TraceOptions traceOptions;

        protected TraceListener()
        {
            this.indentSize = 4;
            this.needIndent = true;
        }

        protected TraceListener(string name)
        {
            this.indentSize = 4;
            this.needIndent = true;
            this.listenerName = name;
        }

        public virtual void Close()
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public virtual void Fail(string message)
        {
            this.Fail(message, null);
        }

        public virtual void Fail(string message, string detailMessage)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(SR.GetString("TraceListenerFail"));
            builder.Append(" ");
            builder.Append(message);
            if (detailMessage != null)
            {
                builder.Append(" ");
                builder.Append(detailMessage);
            }
            this.WriteLine(builder.ToString());
        }

        public virtual void Flush()
        {
        }

        protected internal virtual string[] GetSupportedAttributes()
        {
            return null;
        }

        internal bool IsEnabled(TraceOptions opts)
        {
            return ((opts & this.TraceOutputOptions) != TraceOptions.None);
        }

        internal void SetAttributes(Hashtable attribs)
        {
            TraceUtils.VerifyAttributes(attribs, this.GetSupportedAttributes(), this);
            this.attributes = new StringDictionary();
            this.attributes.ReplaceHashtable(attribs);
        }

        [ComVisible(false)]
        public virtual void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if ((this.Filter == null) || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data))
            {
                this.WriteHeader(source, eventType, id);
                string message = string.Empty;
                if (data != null)
                {
                    message = data.ToString();
                }
                this.WriteLine(message);
                this.WriteFooter(eventCache);
            }
        }

        [ComVisible(false)]
        public virtual void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            if ((this.Filter == null) || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
            {
                this.WriteHeader(source, eventType, id);
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
                this.WriteLine(builder.ToString());
                this.WriteFooter(eventCache);
            }
        }

        [ComVisible(false)]
        public virtual void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            this.TraceEvent(eventCache, source, eventType, id, string.Empty);
        }

        [ComVisible(false)]
        public virtual void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if ((this.Filter == null) || this.Filter.ShouldTrace(eventCache, source, eventType, id, message))
            {
                this.WriteHeader(source, eventType, id);
                this.WriteLine(message);
                this.WriteFooter(eventCache);
            }
        }

        [ComVisible(false)]
        public virtual void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if ((this.Filter == null) || this.Filter.ShouldTrace(eventCache, source, eventType, id, format, args))
            {
                this.WriteHeader(source, eventType, id);
                if (args != null)
                {
                    this.WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
                }
                else
                {
                    this.WriteLine(format);
                }
                this.WriteFooter(eventCache);
            }
        }

        [ComVisible(false)]
        public virtual void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            this.TraceEvent(eventCache, source, TraceEventType.Transfer, id, message + ", relatedActivityId=" + relatedActivityId.ToString());
        }

        public virtual void Write(object o)
        {
            if (((this.Filter == null) || this.Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, null, null, o)) && (o != null))
            {
                this.Write(o.ToString());
            }
        }

        public abstract void Write(string message);
        public virtual void Write(object o, string category)
        {
            if ((this.Filter == null) || this.Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, category, null, o))
            {
                if (category == null)
                {
                    this.Write(o);
                }
                else
                {
                    this.Write((o == null) ? "" : o.ToString(), category);
                }
            }
        }

        public virtual void Write(string message, string category)
        {
            if ((this.Filter == null) || this.Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, message))
            {
                if (category == null)
                {
                    this.Write(message);
                }
                else
                {
                    this.Write(category + ": " + ((message == null) ? string.Empty : message));
                }
            }
        }

        private void WriteFooter(TraceEventCache eventCache)
        {
            if (eventCache != null)
            {
                this.indentLevel++;
                if (this.IsEnabled(TraceOptions.ProcessId))
                {
                    this.WriteLine("ProcessId=" + eventCache.ProcessId);
                }
                if (this.IsEnabled(TraceOptions.LogicalOperationStack))
                {
                    this.Write("LogicalOperationStack=");
                    Stack logicalOperationStack = eventCache.LogicalOperationStack;
                    bool flag = true;
                    foreach (object obj2 in logicalOperationStack)
                    {
                        if (!flag)
                        {
                            this.Write(", ");
                        }
                        else
                        {
                            flag = false;
                        }
                        this.Write(obj2.ToString());
                    }
                    this.WriteLine(string.Empty);
                }
                if (this.IsEnabled(TraceOptions.ThreadId))
                {
                    this.WriteLine("ThreadId=" + eventCache.ThreadId);
                }
                if (this.IsEnabled(TraceOptions.DateTime))
                {
                    this.WriteLine("DateTime=" + eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
                }
                if (this.IsEnabled(TraceOptions.Timestamp))
                {
                    this.WriteLine("Timestamp=" + eventCache.Timestamp);
                }
                if (this.IsEnabled(TraceOptions.Callstack))
                {
                    this.WriteLine("Callstack=" + eventCache.Callstack);
                }
                this.indentLevel--;
            }
        }

        private void WriteHeader(string source, TraceEventType eventType, int id)
        {
            this.Write(string.Format(CultureInfo.InvariantCulture, "{0} {1}: {2} : ", new object[] { source, eventType.ToString(), id.ToString(CultureInfo.InvariantCulture) }));
        }

        protected virtual void WriteIndent()
        {
            this.NeedIndent = false;
            for (int i = 0; i < this.indentLevel; i++)
            {
                if (this.indentSize == 4)
                {
                    this.Write("    ");
                }
                else
                {
                    for (int j = 0; j < this.indentSize; j++)
                    {
                        this.Write(" ");
                    }
                }
            }
        }

        public virtual void WriteLine(object o)
        {
            if ((this.Filter == null) || this.Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, null, null, o))
            {
                this.WriteLine((o == null) ? "" : o.ToString());
            }
        }

        public abstract void WriteLine(string message);
        public virtual void WriteLine(object o, string category)
        {
            if ((this.Filter == null) || this.Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, category, null, o))
            {
                this.WriteLine((o == null) ? "" : o.ToString(), category);
            }
        }

        public virtual void WriteLine(string message, string category)
        {
            if ((this.Filter == null) || this.Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, message))
            {
                if (category == null)
                {
                    this.WriteLine(message);
                }
                else
                {
                    this.WriteLine(category + ": " + ((message == null) ? string.Empty : message));
                }
            }
        }

        public StringDictionary Attributes
        {
            get
            {
                if (this.attributes == null)
                {
                    this.attributes = new StringDictionary();
                }
                return this.attributes;
            }
        }

        [ComVisible(false)]
        public TraceFilter Filter
        {
            get
            {
                return this.filter;
            }
            set
            {
                this.filter = value;
            }
        }

        public int IndentLevel
        {
            get
            {
                return this.indentLevel;
            }
            set
            {
                this.indentLevel = (value < 0) ? 0 : value;
            }
        }

        public int IndentSize
        {
            get
            {
                return this.indentSize;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("IndentSize", value, SR.GetString("TraceListenerIndentSize"));
                }
                this.indentSize = value;
            }
        }

        public virtual bool IsThreadSafe
        {
            get
            {
                return false;
            }
        }

        public virtual string Name
        {
            get
            {
                if (this.listenerName != null)
                {
                    return this.listenerName;
                }
                return "";
            }
            set
            {
                this.listenerName = value;
            }
        }

        protected bool NeedIndent
        {
            get
            {
                return this.needIndent;
            }
            set
            {
                this.needIndent = value;
            }
        }

        [ComVisible(false)]
        public TraceOptions TraceOutputOptions
        {
            get
            {
                return this.traceOptions;
            }
            set
            {
                if ((((int) value) >> 6) != 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.traceOptions = value;
            }
        }
    }
}

