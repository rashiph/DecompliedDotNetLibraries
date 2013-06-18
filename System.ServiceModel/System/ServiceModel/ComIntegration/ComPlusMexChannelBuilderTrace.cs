namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;

    internal static class ComPlusMexChannelBuilderTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, ContractDescription contract, Binding binding, string address)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusMexChannelBuilderSchema schema = new ComPlusMexChannelBuilderSchema(contract.Name, contract.Namespace, binding.Name, binding.Namespace, address);
                TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
            }
        }
    }
}

