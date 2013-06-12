namespace System.Diagnostics
{
    using System;

    public class EventTypeFilter : TraceFilter
    {
        private SourceLevels level;

        public EventTypeFilter(SourceLevels level)
        {
            this.level = level;
        }

        public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data)
        {
            return ((eventType & ((TraceEventType) ((int) this.level))) != ((TraceEventType) 0));
        }

        public SourceLevels EventType
        {
            get
            {
                return this.level;
            }
            set
            {
                this.level = value;
            }
        }
    }
}

