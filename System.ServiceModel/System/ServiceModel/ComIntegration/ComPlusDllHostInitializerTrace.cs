namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Diagnostics;

    internal static class ComPlusDllHostInitializerTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, Guid appid)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusDllHostInitializerSchema schema = new ComPlusDllHostInitializerSchema(appid);
                TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
            }
        }

        public static void Trace(TraceEventType type, int traceCode, string description, Guid appid, Guid clsid, ServiceElement service)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                foreach (ServiceEndpointElement element in service.Endpoints)
                {
                    ComPlusDllHostInitializerAddingHostSchema schema = new ComPlusDllHostInitializerAddingHostSchema(appid, clsid, service.BehaviorConfiguration, service.Name, element.Address.ToString(), element.BindingConfiguration, element.BindingName, element.BindingNamespace, element.Binding, element.Contract);
                    TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
                }
            }
        }
    }
}

