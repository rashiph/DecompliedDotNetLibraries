namespace Microsoft.Transactions.Wsat.Recovery
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;

    internal class WsATv1LogEntryDeserializer : LogEntryDeserializer
    {
        private ProtocolVersion protocolVersion;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WsATv1LogEntryDeserializer(MemoryStream mem, LogEntry entry, ProtocolVersion protocolVersion) : base(mem, entry)
        {
            this.protocolVersion = protocolVersion;
        }

        private void CheckFlags(WsATv1LogEntryFlags flags)
        {
            if (((byte) (flags | (WsATv1LogEntryFlags.OptimizedEndpointRepresentation | WsATv1LogEntryFlags.UsesDefaultPort | WsATv1LogEntryFlags.UsesStandardCoordinatorAddressPath | WsATv1LogEntryFlags.UsesStandardParticipantAddressPath))) != 15)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("Invalid WsATv1LogEntryFlags");
            }
            if ((((byte) (flags & WsATv1LogEntryFlags.OptimizedEndpointRepresentation)) == 0) && (((int) flags) != 0))
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("If OptimizedEndpointRepresentation is not set, no other flag should be set");
            }
            if (((byte) (flags & (WsATv1LogEntryFlags.UsesStandardCoordinatorAddressPath | WsATv1LogEntryFlags.UsesStandardParticipantAddressPath))) == 12)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("Both address flags cannot be set at once");
            }
        }

        protected override void DeserializeExtended()
        {
            DebugTrace.TraceEnter(this, "DeserializeExtended");
            WsATv1LogEntryVersion version = (WsATv1LogEntryVersion) SerializationUtils.ReadByte(base.mem);
            if (!Enum.IsDefined(typeof(WsATv1LogEntryVersion), version))
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("Unsupported WsATv1LogEntryVersion");
            }
            WsATv1LogEntryFlags flags = (WsATv1LogEntryFlags) SerializationUtils.ReadByte(base.mem);
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "DeserializeExtended flags: {0}", flags);
            }
            this.CheckFlags(flags);
            if (((byte) (flags & WsATv1LogEntryFlags.OptimizedEndpointRepresentation)) == 0)
            {
                base.entry.Endpoint = SerializationUtils.ReadEndpointAddress(base.mem, this.protocolVersion);
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Read endpoint address: {0}", base.entry.Endpoint.Uri);
                }
            }
            else
            {
                int num;
                string str2;
                Guid guid = SerializationUtils.ReadGuid(base.mem);
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Read remote EnlistmentId: {0}", guid);
                }
                string str = SerializationUtils.ReadString(base.mem);
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Read hostName: {0}", str);
                }
                if (((byte) (flags & WsATv1LogEntryFlags.UsesDefaultPort)) != 0)
                {
                    num = 0x944;
                }
                else
                {
                    num = SerializationUtils.ReadInt(base.mem);
                    if (DebugTrace.Verbose)
                    {
                        DebugTrace.Trace(TraceLevel.Verbose, "Read port: {0}", num);
                    }
                }
                if (((byte) (flags & WsATv1LogEntryFlags.UsesStandardCoordinatorAddressPath)) != 0)
                {
                    str2 = WsATv1LogEntrySerializer.StandardCoordinatorAddressPath(this.protocolVersion);
                }
                else if (((byte) (flags & WsATv1LogEntryFlags.UsesStandardParticipantAddressPath)) != 0)
                {
                    str2 = WsATv1LogEntrySerializer.StandardParticipantAddressPath(this.protocolVersion);
                }
                else
                {
                    str2 = SerializationUtils.ReadString(base.mem);
                    if (DebugTrace.Verbose)
                    {
                        DebugTrace.Trace(TraceLevel.Verbose, "Read address path: {0}", str2);
                    }
                }
                UriBuilder builder = new UriBuilder(Uri.UriSchemeHttps, str, num, str2);
                EnlistmentHeader header = new EnlistmentHeader(guid, ControlProtocol.Durable2PC);
                base.entry.Endpoint = new EndpointAddress(builder.Uri, new AddressHeader[] { header });
            }
            DebugTrace.TraceLeave(this, "DeserializeExtended");
        }
    }
}

