namespace Microsoft.Transactions.Wsat.Recovery
{
    using System;
    using System.IO;
    using System.Runtime;

    internal abstract class LogEntryHeaderDeserializer
    {
        protected MemoryStream mem;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected LogEntryHeaderDeserializer(MemoryStream mem)
        {
            this.mem = mem;
        }

        public abstract LogEntry DeserializeHeader();
    }
}

