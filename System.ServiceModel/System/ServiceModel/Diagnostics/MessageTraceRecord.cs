namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class MessageTraceRecord : TraceRecord
    {
        private System.ServiceModel.Channels.Message message;

        internal MessageTraceRecord(System.ServiceModel.Channels.Message message)
        {
            this.message = message;
        }

        internal override void WriteTo(XmlWriter xml)
        {
            if (((this.message != null) && (this.message.State != MessageState.Closed)) && (this.message.Headers != null))
            {
                try
                {
                    xml.WriteStartElement("MessageProperties");
                    if (this.message.Properties.Encoder != null)
                    {
                        xml.WriteElementString("Encoder", this.message.Properties.Encoder.ToString());
                    }
                    xml.WriteElementString("AllowOutputBatching", this.message.Properties.AllowOutputBatching.ToString());
                    if ((this.message.Properties.Security != null) && (this.message.Properties.Security.ServiceSecurityContext != null))
                    {
                        xml.WriteStartElement("Security");
                        xml.WriteElementString("IsAnonymous", this.message.Properties.Security.ServiceSecurityContext.IsAnonymous.ToString());
                        xml.WriteElementString("WindowsIdentityUsed", ((this.message.Properties.Security.ServiceSecurityContext.WindowsIdentity != null) && !string.IsNullOrEmpty(this.message.Properties.Security.ServiceSecurityContext.WindowsIdentity.Name)).ToString());
                        if (DiagnosticUtility.ShouldTraceVerbose)
                        {
                            xml.WriteStartElement("Claims");
                            AuthorizationContext authorizationContext = this.message.Properties.Security.ServiceSecurityContext.AuthorizationContext;
                            for (int j = 0; j < authorizationContext.ClaimSets.Count; j++)
                            {
                                ClaimSet set = authorizationContext.ClaimSets[j];
                                xml.WriteStartElement("ClaimSet");
                                xml.WriteAttributeString("ClrType", base.XmlEncode(set.GetType().AssemblyQualifiedName));
                                for (int k = 0; k < set.Count; k++)
                                {
                                    SecurityTraceRecordHelper.WriteClaim(xml, set[k]);
                                }
                                xml.WriteEndElement();
                            }
                            xml.WriteEndElement();
                        }
                        xml.WriteEndElement();
                    }
                    if (this.message.Properties.Via != null)
                    {
                        xml.WriteElementString("Via", this.message.Properties.Via.ToString());
                    }
                    xml.WriteEndElement();
                    xml.WriteStartElement("MessageHeaders");
                    for (int i = 0; i < this.message.Headers.Count; i++)
                    {
                        this.message.Headers.WriteHeader(i, xml);
                    }
                    xml.WriteEndElement();
                }
                catch (CommunicationException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, 0x2000a, System.ServiceModel.SR.GetString("TraceCodeDiagnosticsFailedMessageTrace"), (Exception) exception, this.message);
                    }
                }
            }
        }

        internal override string EventId
        {
            get
            {
                return base.BuildEventId("Message");
            }
        }

        protected System.ServiceModel.Channels.Message Message
        {
            get
            {
                return this.message;
            }
        }
    }
}

