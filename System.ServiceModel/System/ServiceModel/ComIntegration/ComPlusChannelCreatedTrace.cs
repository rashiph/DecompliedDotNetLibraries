namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal static class ComPlusChannelCreatedTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, Uri address, Type contractType)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusChannelCreatedSchema schema = new ComPlusChannelCreatedSchema(address, (contractType != null) ? contractType.ToString() : null);
                TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
            }
        }
    }
}

