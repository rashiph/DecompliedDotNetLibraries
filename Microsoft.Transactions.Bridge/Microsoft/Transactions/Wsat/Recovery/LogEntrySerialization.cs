namespace Microsoft.Transactions.Wsat.Recovery
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;

    internal class LogEntrySerialization
    {
        private int maxLogEntrySize;
        private ProtocolVersion protocolVersion;
        private ProtocolState state;

        public LogEntrySerialization(ProtocolState state)
        {
            if (state == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("state"));
            }
            this.state = state;
            this.protocolVersion = state.ProtocolVersion;
            this.maxLogEntrySize = this.state.TransactionManager.MaxLogEntrySize;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public LogEntrySerialization(int maxLogEntrySize, ProtocolVersion protocolVersion)
        {
            this.protocolVersion = protocolVersion;
            this.maxLogEntrySize = maxLogEntrySize;
        }

        private LogEntryDeserializer CreateDeserializer(MemoryStream mem, LogEntry logEntry)
        {
            return new WsATv1LogEntryDeserializer(mem, logEntry, this.protocolVersion);
        }

        public CoordinatorEnlistment DeserializeCoordinator(Enlistment enlistment)
        {
            byte[] recoveryData = enlistment.GetRecoveryData();
            LogEntry entry = this.DeserializeLogEntry(recoveryData, enlistment.LocalTransactionId);
            enlistment.RemoteTransactionId = entry.RemoteTransactionId;
            return new CoordinatorEnlistment(this.state, enlistment, entry.LocalEnlistmentId, entry.Endpoint);
        }

        private LogEntry DeserializeHeader(MemoryStream mem, Guid localTransactionId)
        {
            LogEntryHeaderDeserializer deserializer;
            if (SerializationUtils.ReadByte(mem) == 1)
            {
                deserializer = new LogEntryHeaderv1Deserializer(mem, localTransactionId);
            }
            else
            {
                DiagnosticUtility.FailFast("Log entry with unsupported major version");
                deserializer = null;
            }
            return deserializer.DeserializeHeader();
        }

        private LogEntry DeserializeLogEntry(byte[] buffer, Guid localTransactionId)
        {
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Deserializing {0} byte buffer", buffer.Length);
            }
            MemoryStream mem = new MemoryStream(buffer, 0, buffer.Length, false, true);
            LogEntry logEntry = this.DeserializeHeader(mem, localTransactionId);
            return this.CreateDeserializer(mem, logEntry).Deserialize();
        }

        public ParticipantEnlistment DeserializeParticipant(Enlistment enlistment)
        {
            byte[] recoveryData = enlistment.GetRecoveryData();
            LogEntry entry = this.DeserializeLogEntry(recoveryData, enlistment.LocalTransactionId);
            enlistment.RemoteTransactionId = entry.RemoteTransactionId;
            return new ParticipantEnlistment(this.state, enlistment, entry.LocalEnlistmentId, entry.Endpoint);
        }

        public byte[] Serialize(CoordinatorEnlistment coordinator)
        {
            Enlistment enlistment = coordinator.Enlistment;
            LogEntry logEntry = new LogEntry(enlistment.RemoteTransactionId, enlistment.LocalTransactionId, coordinator.EnlistmentId, coordinator.CoordinatorProxy.To);
            return this.SerializeLogEntry(logEntry);
        }

        public byte[] Serialize(ParticipantEnlistment participant)
        {
            Enlistment enlistment = participant.Enlistment;
            LogEntry logEntry = new LogEntry(enlistment.RemoteTransactionId, enlistment.LocalTransactionId, participant.EnlistmentId, participant.ParticipantProxy.To);
            return this.SerializeLogEntry(logEntry);
        }

        private byte[] SerializeLogEntry(LogEntry logEntry)
        {
            byte[] buffer = new WsATv1LogEntrySerializer(logEntry, this.protocolVersion).Serialize();
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Serialized {0} byte buffer", buffer.Length);
            }
            if (buffer.Length > this.maxLogEntrySize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(Microsoft.Transactions.SR.GetString("SerializationLogEntryTooBig", new object[] { buffer.Length })));
            }
            return buffer;
        }
    }
}

