namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class ActivityIdHeader : DictionaryHeader
    {
        private Guid guid;
        private Guid headerId;

        internal ActivityIdHeader(Guid activityId)
        {
            this.guid = activityId;
            this.headerId = Guid.NewGuid();
        }

        internal void AddTo(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (((message.State != MessageState.Closed) && (message.Headers.MessageVersion.Envelope != EnvelopeVersion.None)) && (message.Headers.FindHeader("ActivityId", "http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics") < 0))
            {
                message.Headers.Add(this);
            }
        }

        internal static bool ExtractActivityAndCorrelationId(Message message, out Guid activityId, out Guid correlationId)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            activityId = Guid.Empty;
            correlationId = Guid.Empty;
            try
            {
                if ((message.State != MessageState.Closed) && (message.Headers != null))
                {
                    int headerIndex = message.Headers.FindHeader("ActivityId", "http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics");
                    if (headerIndex >= 0)
                    {
                        using (XmlDictionaryReader reader = message.Headers.GetReaderAtHeader(headerIndex))
                        {
                            correlationId = Fx.CreateGuid(reader.GetAttribute("CorrelationId", null));
                            activityId = reader.ReadElementContentAsGuid();
                            return (activityId != Guid.Empty);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (DiagnosticUtility.ShouldTraceError)
                {
                    TraceUtility.TraceEvent(TraceEventType.Error, 0x20007, System.ServiceModel.SR.GetString("TraceCodeFailedToReadAnActivityIdHeader"), null, exception);
                }
            }
            return false;
        }

        internal static Guid ExtractActivityId(Message message)
        {
            Guid empty = Guid.Empty;
            try
            {
                if (((message == null) || (message.State == MessageState.Closed)) || (message.Headers == null))
                {
                    return empty;
                }
                int headerIndex = message.Headers.FindHeader("ActivityId", "http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics");
                if (headerIndex < 0)
                {
                    return empty;
                }
                using (XmlDictionaryReader reader = message.Headers.GetReaderAtHeader(headerIndex))
                {
                    empty = reader.ReadElementContentAsGuid();
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (DiagnosticUtility.ShouldTraceError)
                {
                    TraceUtility.TraceEvent(TraceEventType.Error, 0x20007, System.ServiceModel.SR.GetString("TraceCodeFailedToReadAnActivityIdHeader"), null, exception);
                }
            }
            return empty;
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            writer.WriteAttributeString("CorrelationId", this.headerId.ToString());
            writer.WriteValue(this.guid);
        }

        public override XmlDictionaryString DictionaryName
        {
            get
            {
                return XD.ActivityIdFlowDictionary.ActivityId;
            }
        }

        public override XmlDictionaryString DictionaryNamespace
        {
            get
            {
                return XD.ActivityIdFlowDictionary.ActivityIdNamespace;
            }
        }
    }
}

