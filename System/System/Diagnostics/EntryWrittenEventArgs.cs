namespace System.Diagnostics
{
    using System;

    public class EntryWrittenEventArgs : EventArgs
    {
        private EventLogEntry entry;

        public EntryWrittenEventArgs()
        {
        }

        public EntryWrittenEventArgs(EventLogEntry entry)
        {
            this.entry = entry;
        }

        public EventLogEntry Entry
        {
            get
            {
                return this.entry;
            }
        }
    }
}

