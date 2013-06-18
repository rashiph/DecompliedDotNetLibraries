namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;

    internal static class ProtocolInitializedRecord
    {
        public static void Trace(Guid protocolId, string protocolName)
        {
            ProtocolServiceRecordSchema extendedData = new ProtocolServiceRecordSchema(protocolName, protocolId);
            TxTraceUtility.Trace(TraceEventType.Information, 0xb000e, Microsoft.Transactions.SR.GetString("ProtocolInitialized"), extendedData, null, protocolId, null);
        }

        public static bool ShouldTrace
        {
            get
            {
                return DiagnosticUtility.ShouldTraceInformation;
            }
        }
    }
}

