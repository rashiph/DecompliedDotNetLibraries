namespace Microsoft.Transactions.Wsat.Recovery
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class WsATv1LogEntrySerializer : LogEntrySerializer
    {
        private ProtocolVersion protocolVersion;
        private static readonly string standardCoordinatorAddressPath10 = ("/WsatService/" + BindingStrings.TwoPhaseCommitCoordinatorSuffix(ProtocolVersion.Version10));
        private static readonly string standardCoordinatorAddressPath11 = ("/WsatService/" + BindingStrings.TwoPhaseCommitCoordinatorSuffix(ProtocolVersion.Version11));
        private static readonly string standardParticipantAddressPath10 = ("/WsatService/" + BindingStrings.TwoPhaseCommitParticipantSuffix(ProtocolVersion.Version10));
        private static readonly string standardParticipantAddressPath11 = ("/WsatService/" + BindingStrings.TwoPhaseCommitParticipantSuffix(ProtocolVersion.Version11));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WsATv1LogEntrySerializer(LogEntry logEntry, ProtocolVersion protocolVersion) : base(logEntry)
        {
            this.protocolVersion = protocolVersion;
        }

        private static WsATv1LogEntryFlags GetPathFlags(EndpointAddress address, ProtocolVersion protocolVersion)
        {
            if (address.Uri.AbsolutePath == StandardCoordinatorAddressPath(protocolVersion))
            {
                return WsATv1LogEntryFlags.UsesStandardCoordinatorAddressPath;
            }
            if (address.Uri.AbsolutePath == StandardParticipantAddressPath(protocolVersion))
            {
                return WsATv1LogEntryFlags.UsesStandardParticipantAddressPath;
            }
            return 0;
        }

        private static bool GetRemoteEnlistmentId(EndpointAddress address, out Guid remoteEnlistmentId)
        {
            AddressHeaderCollection headers = address.Headers;
            if (headers.Count == 1)
            {
                AddressHeader header = headers.FindHeader("Enlistment", "http://schemas.microsoft.com/ws/2006/02/transactions");
                if (header != null)
                {
                    XmlDictionaryReader addressHeaderReader = header.GetAddressHeaderReader();
                    XmlDictionaryReader reader2 = addressHeaderReader;
                    try
                    {
                        ControlProtocol protocol;
                        EnlistmentHeader.ReadFrom(addressHeaderReader, out remoteEnlistmentId, out protocol);
                        return (protocol == ControlProtocol.Durable2PC);
                    }
                    catch (InvalidEnlistmentHeaderException exception)
                    {
                        Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                    finally
                    {
                        if (reader2 != null)
                        {
                            reader2.Dispose();
                        }
                    }
                }
            }
            remoteEnlistmentId = Guid.Empty;
            return false;
        }

        protected override void SerializeExtended()
        {
            Guid guid;
            DebugTrace.TraceEnter(this, "SerializeExtended");
            WsATv1LogEntryFlags flags = 0;
            WsATv1LogEntryFlags pathFlags = 0;
            EndpointAddress endpoint = base.logEntry.Endpoint;
            Uri uri = endpoint.Uri;
            if (GetRemoteEnlistmentId(endpoint, out guid))
            {
                flags = (WsATv1LogEntryFlags) ((byte) (flags | WsATv1LogEntryFlags.OptimizedEndpointRepresentation));
                if (string.Compare(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("Endpoints must use the HTTPS scheme");
                }
                if (0x944 == uri.Port)
                {
                    flags = (WsATv1LogEntryFlags) ((byte) (flags | WsATv1LogEntryFlags.UsesDefaultPort));
                }
                pathFlags = GetPathFlags(endpoint, this.protocolVersion);
                flags = (WsATv1LogEntryFlags) ((byte) (flags | pathFlags));
            }
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "SerializeExtended flags: {0}", flags);
            }
            byte num = 0;
            ProtocolVersionHelper.AssertProtocolVersion(this.protocolVersion, base.GetType(), "SerializeExtended");
            switch (this.protocolVersion)
            {
                case ProtocolVersion.Version10:
                    num = 1;
                    break;

                case ProtocolVersion.Version11:
                    num = 2;
                    break;
            }
            base.mem.WriteByte(num);
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Wrote version: {0} bytes", base.mem.Length);
            }
            base.mem.WriteByte((byte) flags);
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Wrote flags: {0} bytes", base.mem.Length);
            }
            if (((byte) (flags & WsATv1LogEntryFlags.OptimizedEndpointRepresentation)) == 0)
            {
                SerializationUtils.WriteEndpointAddress(base.mem, endpoint, this.protocolVersion);
            }
            else
            {
                SerializationUtils.WriteGuid(base.mem, ref guid);
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Wrote enlistmentId: {0} bytes", base.mem.Length);
                }
                SerializationUtils.WriteString(base.mem, uri.Host);
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Wrote hostName: {0} bytes", base.mem.Length);
                }
                if (((byte) (flags & WsATv1LogEntryFlags.UsesDefaultPort)) == 0)
                {
                    if ((uri.Port < 0) || (uri.Port > 0xffff))
                    {
                        Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("TCP port must be valid");
                    }
                    SerializationUtils.WriteInt(base.mem, uri.Port);
                    if (DebugTrace.Verbose)
                    {
                        DebugTrace.Trace(TraceLevel.Verbose, "Wrote port: {0} bytes", base.mem.Length);
                    }
                }
                if (((int) pathFlags) == 0)
                {
                    SerializationUtils.WriteString(base.mem, uri.AbsolutePath);
                    if (DebugTrace.Verbose)
                    {
                        DebugTrace.Trace(TraceLevel.Verbose, "Wrote address path: {0} bytes", base.mem.Length);
                    }
                }
            }
            DebugTrace.TraceLeave(this, "DeserializeExtended");
        }

        public static string StandardCoordinatorAddressPath(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(WsATv1LogEntrySerializer), "StandardCoordinatorAddressPath");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return standardCoordinatorAddressPath10;

                case ProtocolVersion.Version11:
                    return standardCoordinatorAddressPath11;
            }
            return null;
        }

        public static string StandardParticipantAddressPath(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(WsATv1LogEntrySerializer), "StandardParticipantAddressPath");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return standardParticipantAddressPath10;

                case ProtocolVersion.Version11:
                    return standardParticipantAddressPath11;
            }
            return null;
        }
    }
}

