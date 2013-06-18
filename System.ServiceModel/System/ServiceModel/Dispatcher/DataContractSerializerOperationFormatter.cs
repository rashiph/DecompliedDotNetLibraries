namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Xml;

    internal class DataContractSerializerOperationFormatter : OperationFormatter
    {
        private XsdDataContractExporter dataContractExporter;
        private IList<System.Type> knownTypes;
        protected MessageInfo replyMessageInfo;
        protected MessageInfo requestMessageInfo;
        private DataContractSerializerOperationBehavior serializerFactory;

        public DataContractSerializerOperationFormatter(OperationDescription description, DataContractFormatAttribute dataContractFormatAttribute, DataContractSerializerOperationBehavior serializerFactory) : base(description, dataContractFormatAttribute.Style == OperationFormatStyle.Rpc, false)
        {
            if (description == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            this.serializerFactory = serializerFactory ?? new DataContractSerializerOperationBehavior(description);
            foreach (System.Type type in description.KnownTypes)
            {
                if (this.knownTypes == null)
                {
                    this.knownTypes = new List<System.Type>();
                }
                if (type == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxKnownTypeNull", new object[] { description.Name })));
                }
                this.ValidateDataContractType(type);
                this.knownTypes.Add(type);
            }
            this.requestMessageInfo = this.CreateMessageInfo(dataContractFormatAttribute, base.RequestDescription, this.serializerFactory);
            if (base.ReplyDescription != null)
            {
                this.replyMessageInfo = this.CreateMessageInfo(dataContractFormatAttribute, base.ReplyDescription, this.serializerFactory);
            }
        }

        protected override void AddHeadersToMessage(Message message, MessageDescription messageDescription, object[] parameters, bool isRequest)
        {
            MessageInfo info = isRequest ? this.requestMessageInfo : this.replyMessageInfo;
            PartInfo[] headerParts = info.HeaderParts;
            if ((headerParts != null) && (headerParts.Length != 0))
            {
                MessageHeaders headers = message.Headers;
                for (int i = 0; i < headerParts.Length; i++)
                {
                    PartInfo headerPart = headerParts[i];
                    MessageHeaderDescription description = (MessageHeaderDescription) headerPart.Description;
                    object parameterValue = parameters[description.Index];
                    if (description.Multiple)
                    {
                        if (parameterValue != null)
                        {
                            bool isXmlElement = description.Type == typeof(XmlElement);
                            foreach (object obj3 in (IEnumerable) parameterValue)
                            {
                                this.AddMessageHeaderForParameter(headers, headerPart, message.Version, obj3, isXmlElement);
                            }
                        }
                    }
                    else
                    {
                        this.AddMessageHeaderForParameter(headers, headerPart, message.Version, parameterValue, false);
                    }
                }
            }
        }

        private void AddMessageHeaderForParameter(MessageHeaders headers, PartInfo headerPart, MessageVersion messageVersion, object parameterValue, bool isXmlElement)
        {
            string str;
            bool flag;
            bool flag2;
            MessageHeaderDescription headerDescription = (MessageHeaderDescription) headerPart.Description;
            object headerValue = OperationFormatter.GetContentOfMessageHeaderOfT(headerDescription, parameterValue, out flag, out flag2, out str);
            if (isXmlElement)
            {
                if (headerValue != null)
                {
                    XmlElement element = (XmlElement) headerValue;
                    headers.Add(new OperationFormatter.XmlElementMessageHeader(this, messageVersion, element.LocalName, element.NamespaceURI, flag, str, flag2, element));
                }
            }
            else
            {
                headers.Add(new DataContractSerializerMessageHeader(headerPart, headerValue, flag, str, flag2));
            }
        }

        private MessageInfo CreateMessageInfo(DataContractFormatAttribute dataContractFormatAttribute, MessageDescription messageDescription, DataContractSerializerOperationBehavior serializerFactory)
        {
            if (messageDescription.IsUntypedMessage)
            {
                return null;
            }
            MessageInfo info = new MessageInfo();
            MessageBodyDescription body = messageDescription.Body;
            if (body.WrapperName != null)
            {
                info.WrapperName = base.AddToDictionary(body.WrapperName);
                info.WrapperNamespace = base.AddToDictionary(body.WrapperNamespace);
            }
            MessagePartDescriptionCollection parts = body.Parts;
            info.BodyParts = new PartInfo[parts.Count];
            for (int i = 0; i < parts.Count; i++)
            {
                info.BodyParts[i] = this.CreatePartInfo(parts[i], dataContractFormatAttribute.Style, serializerFactory);
            }
            if (OperationFormatter.IsValidReturnValue(messageDescription.Body.ReturnValue))
            {
                info.ReturnPart = this.CreatePartInfo(messageDescription.Body.ReturnValue, dataContractFormatAttribute.Style, serializerFactory);
            }
            info.HeaderDescriptionTable = new OperationFormatter.MessageHeaderDescriptionTable();
            info.HeaderParts = new PartInfo[messageDescription.Headers.Count];
            for (int j = 0; j < messageDescription.Headers.Count; j++)
            {
                MessageHeaderDescription message = messageDescription.Headers[j];
                if (message.IsUnknownHeaderCollection)
                {
                    info.UnknownHeaderDescription = message;
                }
                else
                {
                    this.ValidateDataContractType(message.Type);
                    info.HeaderDescriptionTable.Add(message.Name, message.Namespace, message);
                }
                info.HeaderParts[j] = this.CreatePartInfo(message, OperationFormatStyle.Document, serializerFactory);
            }
            info.AnyHeaders = (info.UnknownHeaderDescription != null) || (info.HeaderDescriptionTable.Count > 0);
            return info;
        }

        private PartInfo CreatePartInfo(MessagePartDescription part, OperationFormatStyle style, DataContractSerializerOperationBehavior serializerFactory)
        {
            this.ValidateDataContractType(part.Type);
            string s = ((style == OperationFormatStyle.Rpc) || (part.Namespace == null)) ? string.Empty : part.Namespace;
            return new PartInfo(part, base.AddToDictionary(part.Name), base.AddToDictionary(s), this.knownTypes, serializerFactory);
        }

        protected override object DeserializeBody(XmlDictionaryReader reader, MessageVersion version, string action, MessageDescription messageDescription, object[] parameters, bool isRequest)
        {
            MessageInfo requestMessageInfo;
            PartInfo info2;
            if (reader == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
            }
            if (parameters == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parameters"));
            }
            if (isRequest)
            {
                requestMessageInfo = this.requestMessageInfo;
            }
            else
            {
                requestMessageInfo = this.replyMessageInfo;
            }
            if (requestMessageInfo.WrapperName != null)
            {
                if (!reader.IsStartElement(requestMessageInfo.WrapperName, requestMessageInfo.WrapperNamespace))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("SFxInvalidMessageBody", new object[] { requestMessageInfo.WrapperName, requestMessageInfo.WrapperNamespace, reader.NodeType, reader.Name, reader.NamespaceURI })));
                }
                bool isEmptyElement = reader.IsEmptyElement;
                reader.Read();
                if (isEmptyElement)
                {
                    return null;
                }
            }
            object obj2 = null;
            if (requestMessageInfo.ReturnPart == null)
            {
                goto Label_010E;
            }
        Label_00DC:
            info2 = requestMessageInfo.ReturnPart;
            if (info2.Serializer.IsStartObject(reader))
            {
                obj2 = this.DeserializeParameter(reader, info2, isRequest);
            }
            else if (reader.IsStartElement())
            {
                OperationFormatter.TraceAndSkipElement(reader);
                goto Label_00DC;
            }
        Label_010E:
            this.DeserializeParameters(reader, requestMessageInfo.BodyParts, parameters, isRequest);
            if (requestMessageInfo.WrapperName != null)
            {
                reader.ReadEndElement();
            }
            return obj2;
        }

        private object DeserializeHeaderContents(XmlDictionaryReader reader, MessageVersion version, MessageHeaderDescription headerDescription)
        {
            return this.serializerFactory.CreateSerializer(headerDescription.Type, headerDescription.Name, headerDescription.Namespace, this.knownTypes).ReadObject(reader);
        }

        private object DeserializeParameter(XmlDictionaryReader reader, PartInfo part, bool isRequest)
        {
            if (!part.Description.Multiple)
            {
                return this.DeserializeParameterPart(reader, part, isRequest);
            }
            ArrayList list = new ArrayList();
            while (part.Serializer.IsStartObject(reader))
            {
                list.Add(this.DeserializeParameterPart(reader, part, isRequest));
            }
            return list.ToArray(part.Description.Type);
        }

        private object DeserializeParameterPart(XmlDictionaryReader reader, PartInfo part, bool isRequest)
        {
            object obj2;
            XmlObjectSerializer serializer = part.Serializer;
            try
            {
                obj2 = serializer.ReadObject(reader, false);
            }
            catch (InvalidOperationException exception)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidMessageBodyErrorDeserializingParameter", new object[] { part.Description.Namespace, part.Description.Name }), exception));
            }
            catch (InvalidDataContractException exception2)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.ServiceModel.SR.GetString("SFxInvalidMessageBodyErrorDeserializingParameter", new object[] { part.Description.Namespace, part.Description.Name }), exception2));
            }
            catch (FormatException exception3)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(OperationFormatter.CreateDeserializationFailedFault(System.ServiceModel.SR.GetString("SFxInvalidMessageBodyErrorDeserializingParameterMore", new object[] { part.Description.Namespace, part.Description.Name, exception3.Message }), exception3));
            }
            catch (SerializationException exception4)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(OperationFormatter.CreateDeserializationFailedFault(System.ServiceModel.SR.GetString("SFxInvalidMessageBodyErrorDeserializingParameterMore", new object[] { part.Description.Namespace, part.Description.Name, exception4.Message }), exception4));
            }
            return obj2;
        }

        private void DeserializeParameters(XmlDictionaryReader reader, PartInfo[] parts, object[] parameters, bool isRequest)
        {
            int num = 0;
            while (reader.IsStartElement())
            {
                for (int i = num; i < parts.Length; i++)
                {
                    PartInfo part = parts[i];
                    if (part.Serializer.IsStartObject(reader))
                    {
                        object obj2 = this.DeserializeParameter(reader, part, isRequest);
                        parameters[part.Description.Index] = obj2;
                        num = i + 1;
                    }
                    else
                    {
                        parameters[part.Description.Index] = null;
                    }
                }
                if (reader.IsStartElement())
                {
                    OperationFormatter.TraceAndSkipElement(reader);
                }
            }
        }

        protected override void GetHeadersFromMessage(Message message, MessageDescription messageDescription, object[] parameters, bool isRequest)
        {
            MessageInfo info = isRequest ? this.requestMessageInfo : this.replyMessageInfo;
            if (info.AnyHeaders)
            {
                MessageHeaders headers = message.Headers;
                KeyValuePair<System.Type, ArrayList>[] pairArray = null;
                ArrayList list = null;
                if (info.UnknownHeaderDescription != null)
                {
                    list = new ArrayList();
                }
                for (int i = 0; i < headers.Count; i++)
                {
                    MessageHeaderInfo headerInfo = headers[i];
                    MessageHeaderDescription headerDescription = info.HeaderDescriptionTable.Get(headerInfo.Name, headerInfo.Namespace);
                    if (headerDescription != null)
                    {
                        if (headerInfo.MustUnderstand)
                        {
                            headers.UnderstoodHeaders.Add(headerInfo);
                        }
                        object obj2 = null;
                        XmlDictionaryReader readerAtHeader = headers.GetReaderAtHeader(i);
                        try
                        {
                            object content = this.DeserializeHeaderContents(readerAtHeader, message.Version, headerDescription);
                            if (headerDescription.TypedHeader)
                            {
                                obj2 = TypedHeaderManager.Create(headerDescription.Type, content, headers[i].MustUnderstand, headers[i].Relay, headers[i].Actor);
                            }
                            else
                            {
                                obj2 = content;
                            }
                        }
                        finally
                        {
                            readerAtHeader.Close();
                        }
                        if (headerDescription.Multiple)
                        {
                            if (pairArray == null)
                            {
                                pairArray = new KeyValuePair<System.Type, ArrayList>[parameters.Length];
                            }
                            if (pairArray[headerDescription.Index].Key == null)
                            {
                                pairArray[headerDescription.Index] = new KeyValuePair<System.Type, ArrayList>(headerDescription.TypedHeader ? TypedHeaderManager.GetMessageHeaderType(headerDescription.Type) : headerDescription.Type, new ArrayList());
                            }
                            pairArray[headerDescription.Index].Value.Add(obj2);
                        }
                        else
                        {
                            parameters[headerDescription.Index] = obj2;
                        }
                    }
                    else if (info.UnknownHeaderDescription != null)
                    {
                        MessageHeaderDescription unknownHeaderDescription = info.UnknownHeaderDescription;
                        XmlDictionaryReader reader = headers.GetReaderAtHeader(i);
                        try
                        {
                            object obj4 = new XmlDocument().ReadNode(reader);
                            if ((obj4 != null) && unknownHeaderDescription.TypedHeader)
                            {
                                obj4 = TypedHeaderManager.Create(unknownHeaderDescription.Type, obj4, headers[i].MustUnderstand, headers[i].Relay, headers[i].Actor);
                            }
                            list.Add(obj4);
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                }
                if (pairArray != null)
                {
                    for (int j = 0; j < parameters.Length; j++)
                    {
                        if (pairArray[j].Key != null)
                        {
                            parameters[j] = pairArray[j].Value.ToArray(pairArray[j].Key);
                        }
                    }
                }
                if (info.UnknownHeaderDescription != null)
                {
                    parameters[info.UnknownHeaderDescription.Index] = list.ToArray(info.UnknownHeaderDescription.TypedHeader ? typeof(MessageHeader<XmlElement>) : typeof(XmlElement));
                }
            }
        }

        protected override void SerializeBody(XmlDictionaryWriter writer, MessageVersion version, string action, MessageDescription messageDescription, object returnValue, object[] parameters, bool isRequest)
        {
            MessageInfo requestMessageInfo;
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            if (parameters == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parameters"));
            }
            if (isRequest)
            {
                requestMessageInfo = this.requestMessageInfo;
            }
            else
            {
                requestMessageInfo = this.replyMessageInfo;
            }
            if (requestMessageInfo.WrapperName != null)
            {
                writer.WriteStartElement(requestMessageInfo.WrapperName, requestMessageInfo.WrapperNamespace);
            }
            if (requestMessageInfo.ReturnPart != null)
            {
                this.SerializeParameter(writer, requestMessageInfo.ReturnPart, returnValue);
            }
            this.SerializeParameters(writer, requestMessageInfo.BodyParts, parameters);
            if (requestMessageInfo.WrapperName != null)
            {
                writer.WriteEndElement();
            }
        }

        private void SerializeParameter(XmlDictionaryWriter writer, PartInfo part, object graph)
        {
            if (part.Description.Multiple)
            {
                if (graph != null)
                {
                    foreach (object obj2 in (IEnumerable) graph)
                    {
                        this.SerializeParameterPart(writer, part, obj2);
                    }
                }
            }
            else
            {
                this.SerializeParameterPart(writer, part, graph);
            }
        }

        private void SerializeParameterPart(XmlDictionaryWriter writer, PartInfo part, object graph)
        {
            try
            {
                part.Serializer.WriteObject(writer, graph);
            }
            catch (SerializationException exception)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxInvalidMessageBodyErrorSerializingParameter", new object[] { part.Description.Namespace, part.Description.Name, exception.Message }), exception));
            }
        }

        private void SerializeParameters(XmlDictionaryWriter writer, PartInfo[] parts, object[] parameters)
        {
            for (int i = 0; i < parts.Length; i++)
            {
                PartInfo part = parts[i];
                object graph = parameters[part.Description.Index];
                this.SerializeParameter(writer, part, graph);
            }
        }

        private void ValidateDataContractType(System.Type type)
        {
            if (this.dataContractExporter == null)
            {
                this.dataContractExporter = new XsdDataContractExporter();
                if ((this.serializerFactory != null) && (this.serializerFactory.DataContractSurrogate != null))
                {
                    ExportOptions options = new ExportOptions {
                        DataContractSurrogate = this.serializerFactory.DataContractSurrogate
                    };
                    this.dataContractExporter.Options = options;
                }
            }
            this.dataContractExporter.GetSchemaTypeName(type);
        }

        private class DataContractSerializerMessageHeader : XmlObjectSerializerHeader
        {
            private DataContractSerializerOperationFormatter.PartInfo headerPart;

            public DataContractSerializerMessageHeader(DataContractSerializerOperationFormatter.PartInfo headerPart, object headerValue, bool mustUnderstand, string actor, bool relay) : base(headerPart.DictionaryName.Value, headerPart.DictionaryNamespace.Value, headerValue, headerPart.Serializer, mustUnderstand, actor ?? string.Empty, relay)
            {
                this.headerPart = headerPart;
            }

            protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                string prefix = ((this.Namespace == null) || (this.Namespace.Length == 0)) ? string.Empty : "h";
                writer.WriteStartElement(prefix, this.headerPart.DictionaryName, this.headerPart.DictionaryNamespace);
                base.WriteHeaderAttributes(writer, messageVersion);
            }
        }

        protected class MessageInfo
        {
            internal bool AnyHeaders;
            internal DataContractSerializerOperationFormatter.PartInfo[] BodyParts;
            internal OperationFormatter.MessageHeaderDescriptionTable HeaderDescriptionTable;
            internal DataContractSerializerOperationFormatter.PartInfo[] HeaderParts;
            internal DataContractSerializerOperationFormatter.PartInfo ReturnPart;
            internal MessageHeaderDescription UnknownHeaderDescription;
            internal XmlDictionaryString WrapperName;
            internal XmlDictionaryString WrapperNamespace;
        }

        protected class PartInfo
        {
            private MessagePartDescription description;
            private XmlDictionaryString dictionaryName;
            private XmlDictionaryString dictionaryNamespace;
            private IList<System.Type> knownTypes;
            private XmlObjectSerializer serializer;
            private DataContractSerializerOperationBehavior serializerFactory;

            public PartInfo(MessagePartDescription description, XmlDictionaryString dictionaryName, XmlDictionaryString dictionaryNamespace, IList<System.Type> knownTypes, DataContractSerializerOperationBehavior behavior)
            {
                this.dictionaryName = dictionaryName;
                this.dictionaryNamespace = dictionaryNamespace;
                this.description = description;
                this.knownTypes = knownTypes;
                this.serializerFactory = behavior;
            }

            public MessagePartDescription Description
            {
                get
                {
                    return this.description;
                }
            }

            public XmlDictionaryString DictionaryName
            {
                get
                {
                    return this.dictionaryName;
                }
            }

            public XmlDictionaryString DictionaryNamespace
            {
                get
                {
                    return this.dictionaryNamespace;
                }
            }

            public XmlObjectSerializer Serializer
            {
                get
                {
                    if (this.serializer == null)
                    {
                        this.serializer = this.serializerFactory.CreateSerializer(this.description.Type, this.DictionaryName, this.DictionaryNamespace, this.knownTypes);
                    }
                    return this.serializer;
                }
            }
        }
    }
}

