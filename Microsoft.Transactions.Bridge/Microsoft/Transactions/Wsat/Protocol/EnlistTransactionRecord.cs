namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Diagnostics;

    internal static class EnlistTransactionRecord
    {
        public static void Trace(Guid enlistmentId, CoordinationContext context)
        {
            CoordinationContextRecordSchema extendedData = new CoordinationContextRecordSchema(context);
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TxTraceUtility.Trace(TraceEventType.Information, 0xb000b, Microsoft.Transactions.SR.GetString("EnlistTransaction"), extendedData, null, enlistmentId, null);
            }
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

