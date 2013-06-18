namespace Microsoft.Transactions.Wsat.Recovery
{
    using System;
    using System.IO;

    internal abstract class LogEntrySerializer
    {
        private LogEntryHeaderSerializer headerSerializer;
        protected LogEntry logEntry;
        protected MemoryStream mem;

        protected LogEntrySerializer(LogEntry logEntry)
        {
            this.logEntry = logEntry;
            this.headerSerializer = new LogEntryHeaderSerializer(logEntry);
        }

        public byte[] Serialize()
        {
            this.mem = this.headerSerializer.WriteHeader();
            this.SerializeExtended();
            return this.mem.ToArray();
        }

        protected abstract void SerializeExtended();
    }
}

