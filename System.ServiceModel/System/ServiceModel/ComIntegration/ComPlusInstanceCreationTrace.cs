namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    internal static class ComPlusInstanceCreationTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info, Message message, Guid incomingTransactionID)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                WindowsIdentity messageIdentity = MessageUtil.GetMessageIdentity(message);
                Uri from = null;
                if (message.Headers.From != null)
                {
                    from = message.Headers.From.Uri;
                }
                ComPlusInstanceCreationRequestSchema extendedData = new ComPlusInstanceCreationRequestSchema(info.AppID, info.Clsid, from, incomingTransactionID, messageIdentity.Name);
                TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), extendedData, null, null, message);
            }
        }

        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info, InstanceContext instanceContext, int instanceID)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusInstanceReleasedSchema schema = new ComPlusInstanceReleasedSchema(info.AppID, info.Clsid, instanceID);
                TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
            }
        }

        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info, Message message, int instanceID, Guid incomingTransactionID)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                WindowsIdentity messageIdentity = MessageUtil.GetMessageIdentity(message);
                Uri from = null;
                if (message.Headers.From != null)
                {
                    from = message.Headers.From.Uri;
                }
                ComPlusInstanceCreationSuccessSchema extendedData = new ComPlusInstanceCreationSuccessSchema(info.AppID, info.Clsid, from, incomingTransactionID, messageIdentity.Name, instanceID);
                TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), extendedData, null, null, message);
            }
        }
    }
}

