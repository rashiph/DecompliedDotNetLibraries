namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal static class ComPlusTxProxyTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, Guid appid, Guid clsid, Guid transactionID, int instanceID)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusTxProxySchema schema = new ComPlusTxProxySchema(appid, clsid, transactionID, instanceID);
                TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
            }
        }
    }
}

