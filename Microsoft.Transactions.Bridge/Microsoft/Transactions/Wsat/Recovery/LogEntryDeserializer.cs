namespace Microsoft.Transactions.Wsat.Recovery
{
    using System;
    using System.IO;
    using System.Runtime;

    internal abstract class LogEntryDeserializer
    {
        protected LogEntry entry;
        protected MemoryStream mem;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected LogEntryDeserializer(MemoryStream mem, LogEntry entry)
        {
            this.entry = entry;
            this.mem = mem;
        }

        public LogEntry Deserialize()
        {
            this.DeserializeExtended();
            return this.entry;
        }

        protected abstract void DeserializeExtended();
    }
}

