namespace System.Diagnostics
{
    using System;

    public abstract class TraceFilter
    {
        internal string initializeData;

        protected TraceFilter()
        {
        }

        internal bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage)
        {
            return this.ShouldTrace(cache, source, eventType, id, formatOrMessage, null, null, null);
        }

        internal bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args)
        {
            return this.ShouldTrace(cache, source, eventType, id, formatOrMessage, args, null, null);
        }

        internal bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1)
        {
            return this.ShouldTrace(cache, source, eventType, id, formatOrMessage, args, data1, null);
        }

        public abstract bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data);
    }
}

