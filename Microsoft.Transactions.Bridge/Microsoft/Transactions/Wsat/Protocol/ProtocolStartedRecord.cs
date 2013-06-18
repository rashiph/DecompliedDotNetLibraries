namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;

    internal static class ProtocolStartedRecord
    {
        public static void Trace(Guid protocolId, string protocolName)
        {
            ProtocolServiceRecordSchema extendedData = new ProtocolServiceRecordSchema(protocolName, protocolId);
            TxTraceUtility.Trace(TraceEventType.Information, 0xb000f, Microsoft.Transactions.SR.GetString("ProtocolStarted"), extendedData, null, protocolId, null);
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

