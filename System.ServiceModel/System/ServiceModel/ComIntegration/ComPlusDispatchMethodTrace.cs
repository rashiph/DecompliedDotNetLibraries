namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal static class ComPlusDispatchMethodTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, Dictionary<uint, DispatchProxy.MethodInfo> dispToOperationDescription)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                uint key = 10;
                DispatchProxy.MethodInfo info = null;
                while (dispToOperationDescription.TryGetValue(key, out info))
                {
                    ComPlusDispatchMethodSchema schema = new ComPlusDispatchMethodSchema(info.opDesc.Name, info.paramList, info.ReturnVal);
                    TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
                    key++;
                }
            }
        }
    }
}

