namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Runtime;

    internal class PluggableProtocol11 : PluggableProtocol
    {
        internal static readonly Guid ProtocolGuid = new Guid("c05b9cad-ab24-4bb3-9440-3548fa7b4b1b");
        internal const string ProtocolName = "WS-AtomicTransaction 1.1";

        public PluggableProtocol11() : base(ProtocolGuid, "WS-AtomicTransaction 1.1", ProtocolVersion.Version11)
        {
        }

        public override byte[] GetProtocolInformation()
        {
            DebugTrace.TraceEnter(this, "GetProtocolInformation");
            DebugTrace.TraceLeave(this, "GetProtocolInformation");
            return null;
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

