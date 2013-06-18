namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    internal static class ComPlusTypedChannelBuilderTrace
    {
        public static void Trace(TraceEventType type, int v, string description, System.Type contractType, Binding binding)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusTypedChannelBuilderSchema schema = new ComPlusTypedChannelBuilderSchema(contractType.ToString(), (binding != null) ? binding.GetType().ToString() : null);
                TraceUtility.TraceEvent(type, v, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
            }
        }
    }
}

