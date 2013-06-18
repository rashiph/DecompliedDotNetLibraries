namespace Microsoft.Transactions.Wsat.Recovery
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;

    internal class LogEntryHeaderv1Deserializer : LogEntryHeaderDeserializer
    {
        private Guid localTransactionId;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public LogEntryHeaderv1Deserializer(MemoryStream mem, Guid localTransactionId) : base(mem)
        {
            this.localTransactionId = localTransactionId;
        }

        private void CheckFlags(LogEntryHeaderv1Flags flags)
        {
            if (((byte) (flags | LogEntryHeaderv1Flags.StandardRemoteTransactionId)) != 1)
            {
                DiagnosticUtility.FailFast("Invalid LogEntryHeaderv1Flags");
            }
        }

        public override LogEntry DeserializeHeader()
        {
            string str;
            LogEntryHeaderv1Flags flags = (LogEntryHeaderv1Flags) SerializationUtils.ReadByte(base.mem);
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "DeserializeHeader flags: {0}", flags);
            }
            this.CheckFlags(flags);
            if (((byte) (flags & LogEntryHeaderv1Flags.StandardRemoteTransactionId)) == 0)
            {
                str = SerializationUtils.ReadString(base.mem);
            }
            else
            {
                str = CoordinationContext.CreateNativeIdentifier(this.localTransactionId);
            }
            return new LogEntry(str, this.localTransactionId, SerializationUtils.ReadGuid(base.mem));
        }
    }
}

