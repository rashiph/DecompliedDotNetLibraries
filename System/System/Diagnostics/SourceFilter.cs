namespace System.Diagnostics
{
    using System;

    public class SourceFilter : TraceFilter
    {
        private string src;

        public SourceFilter(string source)
        {
            this.Source = source;
        }

        public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return string.Equals(this.src, source);
        }

        public string Source
        {
            get
            {
                return this.src;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("source");
                }
                this.src = value;
            }
        }
    }
}

