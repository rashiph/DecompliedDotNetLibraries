namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Diagnostics;
    using System.Runtime;

    internal class PluggableProtocol10 : PluggableProtocol
    {
        internal static readonly Guid ProtocolGuid = new Guid("cc228cf4-a9c8-43fc-8281-8565eb5889f2");
        internal const string ProtocolName = "WS-AtomicTransaction 1.0";

        public PluggableProtocol10() : base(ProtocolGuid, "WS-AtomicTransaction 1.0", ProtocolVersion.Version10)
        {
        }

        public override byte[] GetProtocolInformation()
        {
            DebugTrace.TraceEnter(this, "GetProtocolInformation");
            byte[] protocolInformation = null;
            if (base.state.Config.NetworkEndpointsEnabled)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Generating protocol information");
                protocolInformation = new ProtocolInformationWriter(base.state).GetProtocolInformation();
            }
            else
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Generating null protocol information");
            }
            DebugTrace.TraceLeave(this, "GetProtocolInformation");
            return protocolInformation;
        }

        public override Guid ProtocolId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return ProtocolGuid;
            }
        }
    }
}

