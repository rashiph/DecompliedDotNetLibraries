namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Xml;

    internal sealed class MessageLogTraceRecord : TraceRecord
    {
        internal const string AddressingElementName = "Addressing";
        internal const string BodyElementName = "Body";
        internal const string HttpRequestMessagePropertyElementName = "HttpRequest";
        internal const string HttpResponseMessagePropertyElementName = "HttpResponse";
        private bool logMessageBody;
        private System.ServiceModel.Channels.Message message;
        internal const string MessageHeaderElementName = "Header";
        internal const string MessageHeadersElementName = "MessageHeaders";
        internal const string MessageLogTraceRecordElementName = "MessageLogTraceRecord";
        private string messageString;
        internal const string MethodElementName = "Method";
        internal const string NamespacePrefix = "";
        internal const string NamespaceUri = "http://schemas.microsoft.com/2004/06/ServiceModel/Management/MessageTrace";
        internal const string QueryStringElementName = "QueryString";
        private XmlReader reader;
        private System.ServiceModel.Diagnostics.MessageLoggingSource source;
        internal const string StatusCodeElementName = "StatusCode";
        internal const string StatusDescriptionElementName = "StatusDescription";
        private DateTime timestamp;
        internal const string TraceTimeAttributeName = "Time";
        private System.Type type;
        internal const string TypeElementName = "Type";
        internal const string WebHeadersElementName = "WebHeaders";

        private MessageLogTraceRecord(System.ServiceModel.Diagnostics.MessageLoggingSource source)
        {
            this.logMessageBody = true;
            this.source = source;
            this.timestamp = DateTime.Now;
        }

        internal MessageLogTraceRecord(ArraySegment<byte> buffer, System.ServiceModel.Diagnostics.MessageLoggingSource source) : this(source)
        {
            this.type = null;
            this.messageString = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        }

        internal MessageLogTraceRecord(Stream stream, System.ServiceModel.Diagnostics.MessageLoggingSource source) : this(source)
        {
            this.type = null;
            StringBuilder builder = new StringBuilder();
            StreamReader reader = new StreamReader(stream);
            int size = 0x1000;
            char[] buffer = DiagnosticUtility.Utility.AllocateCharArray(size);
            int maxMessageSize = MessageLogger.MaxMessageSize;
            if (-1 == maxMessageSize)
            {
                maxMessageSize = 0x1000;
            }
            while (maxMessageSize > 0)
            {
                int num3 = reader.Read(buffer, 0, size);
                if (num3 == 0)
                {
                    break;
                }
                int charCount = (maxMessageSize < num3) ? maxMessageSize : num3;
                builder.Append(buffer, 0, charCount);
                maxMessageSize -= num3;
            }
            reader.Close();
            this.messageString = builder.ToString();
        }

        internal MessageLogTraceRecord(string message, System.ServiceModel.Diagnostics.MessageLoggingSource source) : this(source)
        {
            this.type = null;
            this.messageString = message;
        }

        internal MessageLogTraceRecord(ref System.ServiceModel.Channels.Message message, XmlReader reader, System.ServiceModel.Diagnostics.MessageLoggingSource source, bool logMessageBody) : this(source)
        {
            MessageBuffer buffer = null;
            try
            {
                this.logMessageBody = logMessageBody;
                this.message = message;
                this.reader = reader;
                this.type = message.GetType();
            }
            finally
            {
                if (buffer != null)
                {
                    buffer.Close();
                }
            }
        }

        private void WriteAddressingProperties(XmlWriter dictionaryWriter)
        {
            object obj2;
            if (this.message.Properties.TryGetValue(AddressingProperty.Name, out obj2))
            {
                AddressingProperty property = (AddressingProperty) obj2;
                dictionaryWriter.WriteStartElement("Addressing");
                dictionaryWriter.WriteElementString("Action", property.Action);
                if (null != property.ReplyTo)
                {
                    dictionaryWriter.WriteElementString("ReplyTo", property.ReplyTo.ToString());
                }
                if (null != property.To)
                {
                    dictionaryWriter.WriteElementString("To", property.To.AbsoluteUri);
                }
                if (null != property.MessageId)
                {
                    dictionaryWriter.WriteElementString("MessageID", property.MessageId.ToString());
                }
                dictionaryWriter.WriteEndElement();
                this.message.Properties.Remove(AddressingProperty.Name);
            }
        }

        private void WriteHeader(XmlDictionaryWriter dictionaryWriter)
        {
            dictionaryWriter.WriteStartElement(XD.MessageDictionary.Prefix.Value, XD.MessageDictionary.Header, this.message.Version.Envelope.DictionaryNamespace);
            MessageHeaders headers = this.message.Headers;
            ReceiveSecurityHeader receivedSecurityHeader = null;
            if (this.message is SecurityVerifiedMessage)
            {
                SecurityVerifiedMessage message = this.message as SecurityVerifiedMessage;
                receivedSecurityHeader = message.ReceivedSecurityHeader;
            }
            for (int i = 0; i < headers.Count; i++)
            {
                if (((receivedSecurityHeader != null) && receivedSecurityHeader.HasAtLeastOneItemInsideSecurityHeaderEncrypted) && (receivedSecurityHeader.HeaderIndex == i))
                {
                    receivedSecurityHeader.WriteStartHeader(dictionaryWriter, headers.MessageVersion);
                    receivedSecurityHeader.WriteHeaderContents(dictionaryWriter, headers.MessageVersion);
                    dictionaryWriter.WriteEndElement();
                }
                else
                {
                    headers.WriteHeader(i, dictionaryWriter);
                }
            }
            dictionaryWriter.WriteEndElement();
        }

        private void WriteHttpProperties(XmlWriter dictionaryWriter)
        {
            object obj2;
            if (this.message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out obj2))
            {
                HttpResponseMessageProperty property = (HttpResponseMessageProperty) obj2;
                dictionaryWriter.WriteStartElement("HttpResponse");
                dictionaryWriter.WriteElementString("StatusCode", property.StatusCode.ToString());
                if (property.StatusDescription != null)
                {
                    dictionaryWriter.WriteElementString("StatusDescription", property.StatusDescription);
                }
                dictionaryWriter.WriteStartElement("WebHeaders");
                WebHeaderCollection headers = property.Headers;
                for (int i = 0; i < headers.Count; i++)
                {
                    string localName = headers.Keys[i];
                    string str2 = headers[i];
                    dictionaryWriter.WriteElementString(localName, str2);
                }
                dictionaryWriter.WriteEndElement();
                dictionaryWriter.WriteEndElement();
            }
            if (this.message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out obj2))
            {
                HttpRequestMessageProperty property2 = (HttpRequestMessageProperty) obj2;
                dictionaryWriter.WriteStartElement("HttpRequest");
                dictionaryWriter.WriteElementString("Method", property2.Method);
                dictionaryWriter.WriteElementString("QueryString", property2.QueryString);
                dictionaryWriter.WriteStartElement("WebHeaders");
                WebHeaderCollection headers2 = property2.Headers;
                for (int j = 0; j < headers2.Count; j++)
                {
                    string str3 = headers2.Keys[j];
                    string str4 = headers2[j];
                    dictionaryWriter.WriteElementString(str3, str4);
                }
                dictionaryWriter.WriteEndElement();
                dictionaryWriter.WriteEndElement();
            }
        }

        internal override void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement("", "MessageLogTraceRecord", "http://schemas.microsoft.com/2004/06/ServiceModel/Management/MessageTrace");
            writer.WriteAttributeString("Time", this.timestamp.ToString("o", CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Source", this.source.ToString());
            if (null != this.type)
            {
                XmlDictionaryWriter dictionaryWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer);
                dictionaryWriter.WriteAttributeString("Type", this.type.ToString());
                this.WriteAddressingProperties(dictionaryWriter);
                this.WriteHttpProperties(dictionaryWriter);
                if (this.reader != null)
                {
                    this.reader.MoveToContent();
                }
                if (this.logMessageBody)
                {
                    if (this.reader != null)
                    {
                        dictionaryWriter.WriteNode(this.reader, true);
                    }
                    else
                    {
                        bool hasAtLeastOneItemInsideSecurityHeaderEncrypted = false;
                        if (this.message is SecurityVerifiedMessage)
                        {
                            SecurityVerifiedMessage message = this.message as SecurityVerifiedMessage;
                            hasAtLeastOneItemInsideSecurityHeaderEncrypted = message.ReceivedSecurityHeader.HasAtLeastOneItemInsideSecurityHeaderEncrypted;
                        }
                        if (!hasAtLeastOneItemInsideSecurityHeaderEncrypted)
                        {
                            this.message.ToString(dictionaryWriter);
                        }
                        else
                        {
                            if (this.message.Version.Envelope != EnvelopeVersion.None)
                            {
                                dictionaryWriter.WriteStartElement(XD.MessageDictionary.Prefix.Value, XD.MessageDictionary.Envelope, this.message.Version.Envelope.DictionaryNamespace);
                                this.WriteHeader(dictionaryWriter);
                                this.message.WriteStartBody(writer);
                            }
                            this.message.BodyToString(dictionaryWriter);
                            if (this.message.Version.Envelope != EnvelopeVersion.None)
                            {
                                writer.WriteEndElement();
                                dictionaryWriter.WriteEndElement();
                            }
                        }
                    }
                }
                else if (this.message.Version.Envelope != EnvelopeVersion.None)
                {
                    if (this.reader != null)
                    {
                        dictionaryWriter.WriteStartElement(this.reader.Prefix, this.reader.LocalName, this.reader.NamespaceURI);
                        this.reader.Read();
                        if (string.CompareOrdinal(this.reader.LocalName, "Header") == 0)
                        {
                            dictionaryWriter.WriteNode(this.reader, true);
                        }
                        dictionaryWriter.WriteEndElement();
                    }
                    else
                    {
                        dictionaryWriter.WriteStartElement(XD.MessageDictionary.Prefix.Value, XD.MessageDictionary.Envelope, this.message.Version.Envelope.DictionaryNamespace);
                        this.WriteHeader(dictionaryWriter);
                        dictionaryWriter.WriteEndElement();
                    }
                }
                if (this.reader != null)
                {
                    this.reader.Close();
                    this.reader = null;
                }
            }
            else
            {
                writer.WriteCData(this.messageString);
            }
            writer.WriteEndElement();
        }

        public System.ServiceModel.Channels.Message Message
        {
            get
            {
                return this.message;
            }
        }

        public System.ServiceModel.Diagnostics.MessageLoggingSource MessageLoggingSource
        {
            get
            {
                return this.source;
            }
        }
    }
}

