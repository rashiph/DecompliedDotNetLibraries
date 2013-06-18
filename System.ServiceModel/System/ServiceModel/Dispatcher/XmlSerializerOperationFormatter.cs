namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Xml;
    using System.Xml.Serialization;

    internal class XmlSerializerOperationFormatter : OperationFormatter
    {
        private bool isEncoded;
        private MessageInfo replyMessageInfo;
        private MessageInfo requestMessageInfo;
        private const string soap11Encoding = "http://schemas.xmlsoap.org/soap/encoding/";
        private const string soap12Encoding = "http://www.w3.org/2003/05/soap-encoding";

        public XmlSerializerOperationFormatter(OperationDescription description, XmlSerializerFormatAttribute xmlSerializerFormatAttribute, MessageInfo requestMessageInfo, MessageInfo replyMessageInfo) : base(description, xmlSerializerFormatAttribute.Style == OperationFormatStyle.Rpc, xmlSerializerFormatAttribute.IsEncoded)
        {
            if (xmlSerializerFormatAttribute.IsEncoded && (xmlSerializerFormatAttribute.Style != OperationFormatStyle.Rpc))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxDocEncodedNotSupported", new object[] { description.Name })));
            }
            this.isEncoded = xmlSerializerFormatAttribute.IsEncoded;
            this.requestMessageInfo = requestMessageInfo;
            this.replyMessageInfo = replyMessageInfo;
        }

        protected override void AddHeadersToMessage(Message message, MessageDescription messageDescription, object[] parameters, bool isRequest)
        {
            try
            {
                XmlSerializer headerSerializer;
                OperationFormatter.MessageHeaderDescriptionTable headerDescriptionTable;
                MessageHeaderDescription unknownHeaderDescription;
                bool mustUnderstand;
                bool relay;
                string actor;
                if (isRequest)
                {
                    headerSerializer = this.requestMessageInfo.HeaderSerializer;
                    headerDescriptionTable = this.requestMessageInfo.HeaderDescriptionTable;
                    unknownHeaderDescription = this.requestMessageInfo.UnknownHeaderDescription;
                }
                else
                {
                    headerSerializer = this.replyMessageInfo.HeaderSerializer;
                    headerDescriptionTable = this.replyMessageInfo.HeaderDescriptionTable;
                    unknownHeaderDescription = this.replyMessageInfo.UnknownHeaderDescription;
                }
                if (headerSerializer != null)
                {
                    object[] o = new object[headerDescriptionTable.Count];
                    MessageHeaderOfTHelper helper = null;
                    int num = 0;
                    foreach (MessageHeaderDescription description2 in messageDescription.Headers)
                    {
                        object obj2 = parameters[description2.Index];
                        if (!description2.IsUnknownHeaderCollection)
                        {
                            if (description2.TypedHeader)
                            {
                                if (helper == null)
                                {
                                    helper = new MessageHeaderOfTHelper(parameters.Length);
                                }
                                o[num++] = helper.GetContentAndSaveHeaderAttributes(parameters[description2.Index], description2);
                            }
                            else
                            {
                                o[num++] = obj2;
                            }
                        }
                    }
                    MemoryStream stream = new MemoryStream();
                    XmlDictionaryWriter xmlWriter = XmlDictionaryWriter.CreateTextWriter(stream);
                    xmlWriter.WriteStartElement("root");
                    headerSerializer.Serialize(xmlWriter, o, null, this.isEncoded ? GetEncoding(message.Version.Envelope) : null);
                    xmlWriter.WriteEndElement();
                    xmlWriter.Flush();
                    XmlDocument document = new XmlDocument();
                    stream.Position = 0L;
                    document.Load(stream);
                    foreach (XmlElement element in document.DocumentElement.ChildNodes)
                    {
                        MessageHeaderDescription headerDescription = headerDescriptionTable.Get(element.LocalName, element.NamespaceURI);
                        if (headerDescription == null)
                        {
                            message.Headers.Add(new OperationFormatter.XmlElementMessageHeader(this, message.Version, element.LocalName, element.NamespaceURI, false, null, false, element));
                        }
                        else
                        {
                            if (headerDescription.TypedHeader)
                            {
                                helper.GetHeaderAttributes(headerDescription, out mustUnderstand, out relay, out actor);
                            }
                            else
                            {
                                mustUnderstand = headerDescription.MustUnderstand;
                                relay = headerDescription.Relay;
                                actor = headerDescription.Actor;
                            }
                            message.Headers.Add(new OperationFormatter.XmlElementMessageHeader(this, message.Version, element.LocalName, element.NamespaceURI, mustUnderstand, actor, relay, element));
                        }
                    }
                }
                if ((unknownHeaderDescription != null) && (parameters[unknownHeaderDescription.Index] != null))
                {
                    foreach (object obj3 in (IEnumerable) parameters[unknownHeaderDescription.Index])
                    {
                        XmlElement headerValue = (XmlElement) OperationFormatter.GetContentOfMessageHeaderOfT(unknownHeaderDescription, obj3, out mustUnderstand, out relay, out actor);
                        if (headerValue != null)
                        {
                            message.Headers.Add(new OperationFormatter.XmlElementMessageHeader(this, message.Version, headerValue.LocalName, headerValue.NamespaceURI, mustUnderstand, actor, relay, headerValue));
                        }
                    }
                }
            }
            catch (InvalidOperationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorSerializingHeader", new object[] { messageDescription.MessageName, exception.Message }), exception));
            }
        }

        private static void AddUnknownHeader(MessageHeaderDescription unknownHeaderDescription, ArrayList unknownHeaders, XmlDocument xmlDoc, XmlDictionaryWriter bufferWriter, MessageHeaderInfo header, XmlDictionaryReader headerReader)
        {
            object content = xmlDoc.ReadNode(headerReader);
            if (bufferWriter != null)
            {
                ((XmlElement) content).WriteTo(bufferWriter);
            }
            if ((content != null) && unknownHeaderDescription.TypedHeader)
            {
                content = TypedHeaderManager.Create(unknownHeaderDescription.Type, content, header.MustUnderstand, header.Relay, header.Actor);
            }
            unknownHeaders.Add(content);
        }

        protected override object DeserializeBody(XmlDictionaryReader reader, MessageVersion version, string action, MessageDescription messageDescription, object[] parameters, bool isRequest)
        {
            MessageInfo requestMessageInfo;
            if (isRequest)
            {
                requestMessageInfo = this.requestMessageInfo;
            }
            else
            {
                requestMessageInfo = this.replyMessageInfo;
            }
            if (requestMessageInfo.RpcEncodedTypedMessageBodyParts == null)
            {
                return this.DeserializeBody(reader, version, requestMessageInfo.BodySerializer, messageDescription.Body.ReturnValue, messageDescription.Body.Parts, parameters, isRequest);
            }
            object[] objArray = new object[requestMessageInfo.RpcEncodedTypedMessageBodyParts.Count];
            this.DeserializeBody(reader, version, requestMessageInfo.BodySerializer, null, requestMessageInfo.RpcEncodedTypedMessageBodyParts, objArray, isRequest);
            object obj2 = Activator.CreateInstance(messageDescription.Body.Parts[0].Type);
            int num = 0;
            foreach (MessagePartDescription description in requestMessageInfo.RpcEncodedTypedMessageBodyParts)
            {
                MemberInfo memberInfo = description.MemberInfo;
                FieldInfo info3 = memberInfo as FieldInfo;
                if (info3 != null)
                {
                    info3.SetValue(obj2, objArray[num++]);
                }
                else
                {
                    PropertyInfo info4 = memberInfo as PropertyInfo;
                    if (info4 != null)
                    {
                        info4.SetValue(obj2, objArray[num++], null);
                    }
                }
            }
            parameters[messageDescription.Body.Parts[0].Index] = obj2;
            return null;
        }

        private object DeserializeBody(XmlDictionaryReader reader, MessageVersion version, XmlSerializer serializer, MessagePartDescription returnPart, MessagePartDescriptionCollection bodyParts, object[] parameters, bool isRequest)
        {
            object obj3;
            try
            {
                if (reader == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
                }
                if (parameters == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parameters"));
                }
                object obj2 = null;
                if (serializer == null)
                {
                    return null;
                }
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    return null;
                }
                object[] objArray = (object[]) serializer.Deserialize(reader, this.isEncoded ? GetEncoding(version.Envelope) : null);
                int num = 0;
                if (OperationFormatter.IsValidReturnValue(returnPart))
                {
                    obj2 = objArray[num++];
                }
                for (int i = 0; i < bodyParts.Count; i++)
                {
                    parameters[bodyParts[i].Index] = objArray[num++];
                }
                obj3 = obj2;
            }
            catch (InvalidOperationException exception)
            {
                string name = isRequest ? "SFxErrorDeserializingRequestBody" : "SFxErrorDeserializingReplyBody";
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString(name, new object[] { base.OperationName }), exception));
            }
            return obj3;
        }

        internal static string GetEncoding(EnvelopeVersion version)
        {
            if (version == EnvelopeVersion.Soap11)
            {
                return "http://schemas.xmlsoap.org/soap/encoding/";
            }
            if (version != EnvelopeVersion.Soap12)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("version", System.ServiceModel.SR.GetString("EnvelopeVersionNotSupported", new object[] { version }));
            }
            return "http://www.w3.org/2003/05/soap-encoding";
        }

        protected override void GetHeadersFromMessage(Message message, MessageDescription messageDescription, object[] parameters, bool isRequest)
        {
            try
            {
                XmlSerializer headerSerializer;
                OperationFormatter.MessageHeaderDescriptionTable headerDescriptionTable;
                MessageHeaderDescription unknownHeaderDescription;
                if (isRequest)
                {
                    headerSerializer = this.requestMessageInfo.HeaderSerializer;
                    headerDescriptionTable = this.requestMessageInfo.HeaderDescriptionTable;
                    unknownHeaderDescription = this.requestMessageInfo.UnknownHeaderDescription;
                }
                else
                {
                    headerSerializer = this.replyMessageInfo.HeaderSerializer;
                    headerDescriptionTable = this.replyMessageInfo.HeaderDescriptionTable;
                    unknownHeaderDescription = this.replyMessageInfo.UnknownHeaderDescription;
                }
                MessageHeaders headers = message.Headers;
                ArrayList unknownHeaders = null;
                XmlDocument xmlDoc = null;
                if (unknownHeaderDescription != null)
                {
                    unknownHeaders = new ArrayList();
                    xmlDoc = new XmlDocument();
                }
                if (headerSerializer == null)
                {
                    if (unknownHeaderDescription != null)
                    {
                        for (int i = 0; i < headers.Count; i++)
                        {
                            AddUnknownHeader(unknownHeaderDescription, unknownHeaders, xmlDoc, null, headers[i], headers.GetReaderAtHeader(i));
                        }
                        parameters[unknownHeaderDescription.Index] = unknownHeaders.ToArray(unknownHeaderDescription.TypedHeader ? typeof(MessageHeader<XmlElement>) : typeof(XmlElement));
                    }
                }
                else
                {
                    MemoryStream stream = new MemoryStream();
                    XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);
                    message.WriteStartEnvelope(writer);
                    message.WriteStartHeaders(writer);
                    MessageHeaderOfTHelper helper = null;
                    for (int j = 0; j < headers.Count; j++)
                    {
                        MessageHeaderInfo headerInfo = headers[j];
                        XmlDictionaryReader readerAtHeader = headers.GetReaderAtHeader(j);
                        MessageHeaderDescription headerDescription = headerDescriptionTable.Get(headerInfo.Name, headerInfo.Namespace);
                        if (headerDescription != null)
                        {
                            if (headerInfo.MustUnderstand)
                            {
                                headers.UnderstoodHeaders.Add(headerInfo);
                            }
                            if (headerDescription.TypedHeader)
                            {
                                if (helper == null)
                                {
                                    helper = new MessageHeaderOfTHelper(parameters.Length);
                                }
                                helper.SetHeaderAttributes(headerDescription, headerInfo.MustUnderstand, headerInfo.Relay, headerInfo.Actor);
                            }
                        }
                        if ((headerDescription == null) && (unknownHeaderDescription != null))
                        {
                            AddUnknownHeader(unknownHeaderDescription, unknownHeaders, xmlDoc, writer, headerInfo, readerAtHeader);
                        }
                        else
                        {
                            writer.WriteNode(readerAtHeader, false);
                        }
                        readerAtHeader.Close();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.Flush();
                    stream.Position = 0L;
                    XmlDictionaryReader xmlReader = XmlDictionaryReader.CreateTextReader(stream.GetBuffer(), 0, (int) stream.Length, XmlDictionaryReaderQuotas.Max);
                    xmlReader.ReadStartElement();
                    xmlReader.MoveToContent();
                    if (!xmlReader.IsEmptyElement)
                    {
                        xmlReader.ReadStartElement();
                        object[] objArray = (object[]) headerSerializer.Deserialize(xmlReader, this.isEncoded ? GetEncoding(message.Version.Envelope) : null);
                        int num3 = 0;
                        foreach (MessageHeaderDescription description3 in messageDescription.Headers)
                        {
                            if (!description3.IsUnknownHeaderCollection)
                            {
                                object headerValue = objArray[num3++];
                                if (description3.TypedHeader && (headerValue != null))
                                {
                                    headerValue = helper.CreateMessageHeader(description3, headerValue);
                                }
                                parameters[description3.Index] = headerValue;
                            }
                        }
                        xmlReader.Close();
                    }
                    if (unknownHeaderDescription != null)
                    {
                        parameters[unknownHeaderDescription.Index] = unknownHeaders.ToArray(unknownHeaderDescription.TypedHeader ? typeof(MessageHeader<XmlElement>) : typeof(XmlElement));
                    }
                }
            }
            catch (InvalidOperationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorDeserializingHeader", new object[] { messageDescription.MessageName }), exception));
            }
        }

        protected override void SerializeBody(XmlDictionaryWriter writer, MessageVersion version, string action, MessageDescription messageDescription, object returnValue, object[] parameters, bool isRequest)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parameters"));
            }
            try
            {
                MessageInfo requestMessageInfo;
                if (isRequest)
                {
                    requestMessageInfo = this.requestMessageInfo;
                }
                else
                {
                    requestMessageInfo = this.replyMessageInfo;
                }
                if (requestMessageInfo.RpcEncodedTypedMessageBodyParts == null)
                {
                    this.SerializeBody(writer, version, requestMessageInfo.BodySerializer, messageDescription.Body.ReturnValue, messageDescription.Body.Parts, returnValue, parameters);
                }
                else
                {
                    object[] objArray = new object[requestMessageInfo.RpcEncodedTypedMessageBodyParts.Count];
                    object obj2 = parameters[messageDescription.Body.Parts[0].Index];
                    if (obj2 == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxBodyCannotBeNull", new object[] { messageDescription.MessageName })));
                    }
                    int num = 0;
                    foreach (MessagePartDescription description in requestMessageInfo.RpcEncodedTypedMessageBodyParts)
                    {
                        MemberInfo memberInfo = description.MemberInfo;
                        FieldInfo info3 = memberInfo as FieldInfo;
                        if (info3 != null)
                        {
                            objArray[num++] = info3.GetValue(obj2);
                        }
                        else
                        {
                            PropertyInfo info4 = memberInfo as PropertyInfo;
                            if (info4 != null)
                            {
                                objArray[num++] = info4.GetValue(obj2, null);
                            }
                        }
                    }
                    this.SerializeBody(writer, version, requestMessageInfo.BodySerializer, null, requestMessageInfo.RpcEncodedTypedMessageBodyParts, null, objArray);
                }
            }
            catch (InvalidOperationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorSerializingBody", new object[] { messageDescription.MessageName, exception.Message }), exception));
            }
        }

        private void SerializeBody(XmlDictionaryWriter writer, MessageVersion version, XmlSerializer serializer, MessagePartDescription returnPart, MessagePartDescriptionCollection bodyParts, object returnValue, object[] parameters)
        {
            if (serializer != null)
            {
                bool flag = OperationFormatter.IsValidReturnValue(returnPart);
                object[] o = new object[bodyParts.Count + (flag ? 1 : 0)];
                int num = 0;
                if (flag)
                {
                    o[num++] = returnValue;
                }
                for (int i = 0; i < bodyParts.Count; i++)
                {
                    o[num++] = parameters[bodyParts[i].Index];
                }
                string encodingStyle = this.isEncoded ? GetEncoding(version.Envelope) : null;
                serializer.Serialize(writer, o, null, encodingStyle);
            }
        }

        protected override void WriteBodyAttributes(XmlDictionaryWriter writer, MessageVersion version)
        {
            if (this.isEncoded && (version.Envelope == EnvelopeVersion.Soap11))
            {
                string encoding = GetEncoding(version.Envelope);
                writer.WriteAttributeString("encodingStyle", version.Envelope.Namespace, encoding);
            }
            writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
        }

        private class MessageHeaderOfTHelper
        {
            private object[] attributes;

            internal MessageHeaderOfTHelper(int parameterCount)
            {
                this.attributes = new object[parameterCount];
            }

            internal object CreateMessageHeader(MessageHeaderDescription headerDescription, object headerValue)
            {
                if (headerDescription.Multiple)
                {
                    IList<MessageHeader<object>> list = (IList<MessageHeader<object>>) this.attributes[headerDescription.Index];
                    object[] objArray = (object[]) Array.CreateInstance(TypedHeaderManager.GetMessageHeaderType(headerDescription.Type), list.Count);
                    Array array = (Array) headerValue;
                    for (int i = 0; i < objArray.Length; i++)
                    {
                        MessageHeader<object> header = list[i];
                        objArray[i] = TypedHeaderManager.Create(headerDescription.Type, array.GetValue(i), header.MustUnderstand, header.Relay, header.Actor);
                    }
                    return objArray;
                }
                MessageHeader<object> header2 = (MessageHeader<object>) this.attributes[headerDescription.Index];
                return TypedHeaderManager.Create(headerDescription.Type, headerValue, header2.MustUnderstand, header2.Relay, header2.Actor);
            }

            internal object GetContentAndSaveHeaderAttributes(object parameterValue, MessageHeaderDescription headerDescription)
            {
                bool flag;
                bool flag2;
                string str;
                if (parameterValue == null)
                {
                    return null;
                }
                if (headerDescription.Multiple)
                {
                    object[] objArray = (object[]) parameterValue;
                    MessageHeader<object>[] headerArray = new MessageHeader<object>[objArray.Length];
                    Array array = Array.CreateInstance(headerDescription.Type, objArray.Length);
                    for (int i = 0; i < array.Length; i++)
                    {
                        array.SetValue(OperationFormatter.GetContentOfMessageHeaderOfT(headerDescription, objArray[i], out flag, out flag2, out str), i);
                        headerArray[i] = new MessageHeader<object>(null, flag, str, flag2);
                    }
                    this.attributes[headerDescription.Index] = headerArray;
                    return array;
                }
                object obj2 = OperationFormatter.GetContentOfMessageHeaderOfT(headerDescription, parameterValue, out flag, out flag2, out str);
                this.attributes[headerDescription.Index] = new MessageHeader<object>(null, flag, str, flag2);
                return obj2;
            }

            internal void GetHeaderAttributes(MessageHeaderDescription headerDescription, out bool mustUnderstand, out bool relay, out string actor)
            {
                MessageHeader<object> header = null;
                if (headerDescription.Multiple)
                {
                    MessageHeader<object>[] headerArray = (MessageHeader<object>[]) this.attributes[headerDescription.Index];
                    for (int i = 0; i < headerArray.Length; i++)
                    {
                        if (headerArray[i] != null)
                        {
                            header = headerArray[i];
                            headerArray[i] = null;
                            break;
                        }
                    }
                }
                else
                {
                    header = (MessageHeader<object>) this.attributes[headerDescription.Index];
                }
                mustUnderstand = header.MustUnderstand;
                relay = header.Relay;
                actor = header.Actor;
            }

            internal void SetHeaderAttributes(MessageHeaderDescription headerDescription, bool mustUnderstand, bool relay, string actor)
            {
                if (headerDescription.Multiple)
                {
                    if (this.attributes[headerDescription.Index] == null)
                    {
                        this.attributes[headerDescription.Index] = new List<MessageHeader<object>>();
                    }
                    ((List<MessageHeader<object>>) this.attributes[headerDescription.Index]).Add(new MessageHeader<object>(null, mustUnderstand, actor, relay));
                }
                else
                {
                    this.attributes[headerDescription.Index] = new MessageHeader<object>(null, mustUnderstand, actor, relay);
                }
            }
        }

        internal abstract class MessageInfo
        {
            protected MessageInfo()
            {
            }

            internal abstract XmlSerializer BodySerializer { get; }

            internal abstract OperationFormatter.MessageHeaderDescriptionTable HeaderDescriptionTable { get; }

            internal abstract XmlSerializer HeaderSerializer { get; }

            internal abstract MessagePartDescriptionCollection RpcEncodedTypedMessageBodyParts { get; }

            internal abstract MessageHeaderDescription UnknownHeaderDescription { get; }
        }
    }
}

