namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal class PrimitiveOperationFormatter : IClientMessageFormatter, IDispatchMessageFormatter
    {
        private XmlDictionaryString action;
        private ActionHeader actionHeader10;
        private ActionHeader actionHeaderAugust2004;
        private ActionHeader actionHeaderNone;
        private OperationDescription operation;
        private XmlDictionaryString replyAction;
        private ActionHeader replyActionHeader10;
        private ActionHeader replyActionHeaderAugust2004;
        private ActionHeader replyActionHeaderNone;
        private MessageDescription requestMessage;
        private PartInfo[] requestParts;
        private XmlDictionaryString requestWrapperName;
        private XmlDictionaryString requestWrapperNamespace;
        private MessageDescription responseMessage;
        private PartInfo[] responseParts;
        private XmlDictionaryString responseWrapperName;
        private XmlDictionaryString responseWrapperNamespace;
        private PartInfo returnPart;
        private XmlDictionaryString xsiNilLocalName;
        private XmlDictionaryString xsiNilNamespace;

        public PrimitiveOperationFormatter(OperationDescription description, bool isRpc)
        {
            if (description == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            OperationFormatter.Validate(description, isRpc, false);
            this.operation = description;
            this.requestMessage = description.Messages[0];
            if (description.Messages.Count == 2)
            {
                this.responseMessage = description.Messages[1];
            }
            int num = 3 + this.requestMessage.Body.Parts.Count;
            if (this.responseMessage != null)
            {
                num += 2 + this.responseMessage.Body.Parts.Count;
            }
            XmlDictionary dictionary = new XmlDictionary(num * 2);
            this.xsiNilLocalName = dictionary.Add("nil");
            this.xsiNilNamespace = dictionary.Add("http://www.w3.org/2001/XMLSchema-instance");
            OperationFormatter.GetActions(description, dictionary, out this.action, out this.replyAction);
            if (this.requestMessage.Body.WrapperName != null)
            {
                this.requestWrapperName = AddToDictionary(dictionary, this.requestMessage.Body.WrapperName);
                this.requestWrapperNamespace = AddToDictionary(dictionary, this.requestMessage.Body.WrapperNamespace);
            }
            this.requestParts = AddToDictionary(dictionary, this.requestMessage.Body.Parts, isRpc);
            if (this.responseMessage != null)
            {
                if (this.responseMessage.Body.WrapperName != null)
                {
                    this.responseWrapperName = AddToDictionary(dictionary, this.responseMessage.Body.WrapperName);
                    this.responseWrapperNamespace = AddToDictionary(dictionary, this.responseMessage.Body.WrapperNamespace);
                }
                this.responseParts = AddToDictionary(dictionary, this.responseMessage.Body.Parts, isRpc);
                if ((this.responseMessage.Body.ReturnValue != null) && (this.responseMessage.Body.ReturnValue.Type != typeof(void)))
                {
                    this.returnPart = AddToDictionary(dictionary, this.responseMessage.Body.ReturnValue, isRpc);
                }
            }
        }

        private static XmlDictionaryString AddToDictionary(XmlDictionary dictionary, string s)
        {
            XmlDictionaryString str;
            if (!dictionary.TryLookup(s, out str))
            {
                str = dictionary.Add(s);
            }
            return str;
        }

        private static PartInfo AddToDictionary(XmlDictionary dictionary, MessagePartDescription part, bool isRpc)
        {
            System.Type type = part.Type;
            XmlDictionaryString itemName = null;
            XmlDictionaryString itemNamespace = null;
            if (type.IsArray && (type != typeof(byte[])))
            {
                string arrayItemName = GetArrayItemName(type.GetElementType());
                itemName = AddToDictionary(dictionary, arrayItemName);
                itemNamespace = AddToDictionary(dictionary, "http://schemas.microsoft.com/2003/10/Serialization/Arrays");
            }
            return new PartInfo(part, AddToDictionary(dictionary, part.Name), AddToDictionary(dictionary, isRpc ? string.Empty : part.Namespace), itemName, itemNamespace);
        }

        private static PartInfo[] AddToDictionary(XmlDictionary dictionary, MessagePartDescriptionCollection parts, bool isRpc)
        {
            PartInfo[] infoArray = new PartInfo[parts.Count];
            for (int i = 0; i < parts.Count; i++)
            {
                infoArray[i] = AddToDictionary(dictionary, parts[i], isRpc);
            }
            return infoArray;
        }

        private static bool AreTypesSupported(MessagePartDescriptionCollection bodyDescriptions)
        {
            for (int i = 0; i < bodyDescriptions.Count; i++)
            {
                if (!IsTypeSupported(bodyDescriptions[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private object DeserializeParameter(XmlDictionaryReader reader, PartInfo part)
        {
            if (((reader.AttributeCount > 0) && reader.MoveToAttribute(this.xsiNilLocalName.Value, this.xsiNilNamespace.Value)) && reader.ReadContentAsBoolean())
            {
                reader.Skip();
                return null;
            }
            return part.ReadValue(reader);
        }

        private void DeserializeParameters(XmlDictionaryReader reader, PartInfo[] parts, object[] parameters)
        {
            if (parts.Length != parameters.Length)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SFxParameterCountMismatch", new object[] { "parts", parts.Length, "parameters", parameters.Length }), "parameters"));
            }
            int num = 0;
            while (reader.IsStartElement())
            {
                for (int i = num; i < parts.Length; i++)
                {
                    PartInfo part = parts[i];
                    if (this.IsPartElement(reader, part))
                    {
                        parameters[part.Description.Index] = this.DeserializeParameter(reader, parts[i]);
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

        public object DeserializeReply(Message message, object[] parameters)
        {
            object obj3;
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
            }
            if (parameters == null)
            {
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("parameters"), message);
            }
            try
            {
                if (message.IsEmpty)
                {
                    if (this.responseWrapperName != null)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("SFxInvalidMessageBodyEmptyMessage")));
                    }
                    return null;
                }
                XmlDictionaryReader readerAtBodyContents = message.GetReaderAtBodyContents();
                using (readerAtBodyContents)
                {
                    object obj2 = this.DeserializeResponse(readerAtBodyContents, parameters);
                    message.ReadFromBodyContentsToEnd(readerAtBodyContents);
                    obj3 = obj2;
                }
            }
            catch (XmlException exception)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorDeserializingReplyBodyMore", new object[] { this.operation.Name, exception.Message }), exception));
            }
            catch (FormatException exception2)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorDeserializingReplyBodyMore", new object[] { this.operation.Name, exception2.Message }), exception2));
            }
            catch (SerializationException exception3)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorDeserializingReplyBodyMore", new object[] { this.operation.Name, exception3.Message }), exception3));
            }
            return obj3;
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
            }
            if (parameters == null)
            {
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("parameters"), message);
            }
            try
            {
                if (message.IsEmpty)
                {
                    if (this.requestWrapperName != null)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("SFxInvalidMessageBodyEmptyMessage")));
                    }
                }
                else
                {
                    XmlDictionaryReader readerAtBodyContents = message.GetReaderAtBodyContents();
                    using (readerAtBodyContents)
                    {
                        this.DeserializeRequest(readerAtBodyContents, parameters);
                        message.ReadFromBodyContentsToEnd(readerAtBodyContents);
                    }
                }
            }
            catch (XmlException exception)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(OperationFormatter.CreateDeserializationFailedFault(System.ServiceModel.SR.GetString("SFxErrorDeserializingRequestBodyMore", new object[] { this.operation.Name, exception.Message }), exception));
            }
            catch (FormatException exception2)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(OperationFormatter.CreateDeserializationFailedFault(System.ServiceModel.SR.GetString("SFxErrorDeserializingRequestBodyMore", new object[] { this.operation.Name, exception2.Message }), exception2));
            }
            catch (SerializationException exception3)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorDeserializingRequestBodyMore", new object[] { this.operation.Name, exception3.Message }), exception3));
            }
        }

        private void DeserializeRequest(XmlDictionaryReader reader, object[] parameters)
        {
            if (this.requestWrapperName != null)
            {
                if (!reader.IsStartElement(this.requestWrapperName, this.requestWrapperNamespace))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("SFxInvalidMessageBody", new object[] { this.requestWrapperName, this.requestWrapperNamespace, reader.NodeType, reader.Name, reader.NamespaceURI })));
                }
                bool isEmptyElement = reader.IsEmptyElement;
                reader.Read();
                if (isEmptyElement)
                {
                    return;
                }
            }
            this.DeserializeParameters(reader, this.requestParts, parameters);
            if (this.requestWrapperName != null)
            {
                reader.ReadEndElement();
            }
        }

        private object DeserializeResponse(XmlDictionaryReader reader, object[] parameters)
        {
            if (this.responseWrapperName != null)
            {
                if (!reader.IsStartElement(this.responseWrapperName, this.responseWrapperNamespace))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.ServiceModel.SR.GetString("SFxInvalidMessageBody", new object[] { this.responseWrapperName, this.responseWrapperNamespace, reader.NodeType, reader.Name, reader.NamespaceURI })));
                }
                bool isEmptyElement = reader.IsEmptyElement;
                reader.Read();
                if (isEmptyElement)
                {
                    return null;
                }
            }
            object obj2 = null;
            if (this.returnPart == null)
            {
                goto Label_00CB;
            }
        Label_008D:
            if (this.IsPartElement(reader, this.returnPart))
            {
                obj2 = this.DeserializeParameter(reader, this.returnPart);
            }
            else if (reader.IsStartElement() && !this.IsPartElements(reader, this.responseParts))
            {
                OperationFormatter.TraceAndSkipElement(reader);
                goto Label_008D;
            }
        Label_00CB:
            this.DeserializeParameters(reader, this.responseParts, parameters);
            if (this.responseWrapperName != null)
            {
                reader.ReadEndElement();
            }
            return obj2;
        }

        private ActionHeader GetActionHeader(AddressingVersion addressing)
        {
            if (this.action == null)
            {
                return null;
            }
            if (addressing == AddressingVersion.WSAddressingAugust2004)
            {
                return this.ActionHeaderAugust2004;
            }
            if (addressing == AddressingVersion.WSAddressing10)
            {
                return this.ActionHeader10;
            }
            if (addressing != AddressingVersion.None)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { addressing })));
            }
            return this.ActionHeaderNone;
        }

        private static string GetArrayItemName(System.Type type)
        {
            switch (System.Type.GetTypeCode(type))
            {
                case TypeCode.Int32:
                    return "int";

                case TypeCode.Int64:
                    return "long";

                case TypeCode.Single:
                    return "float";

                case TypeCode.Double:
                    return "double";

                case TypeCode.Decimal:
                    return "decimal";

                case TypeCode.DateTime:
                    return "dateTime";

                case TypeCode.Boolean:
                    return "boolean";
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidUseOfPrimitiveOperationFormatter")));
        }

        private ActionHeader GetReplyActionHeader(AddressingVersion addressing)
        {
            if (this.replyAction == null)
            {
                return null;
            }
            if (addressing == AddressingVersion.WSAddressingAugust2004)
            {
                return this.ReplyActionHeaderAugust2004;
            }
            if (addressing == AddressingVersion.WSAddressing10)
            {
                return this.ReplyActionHeader10;
            }
            if (addressing != AddressingVersion.None)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { addressing })));
            }
            return this.ReplyActionHeaderNone;
        }

        private static bool IsArrayTypeSupported(System.Type type)
        {
            if (!type.IsEnum)
            {
                switch (System.Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                    case TypeCode.DateTime:
                        return true;
                }
            }
            return false;
        }

        public static bool IsContractSupported(OperationDescription description)
        {
            if (description == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            MessageDescription description2 = description.Messages[0];
            MessageDescription description3 = null;
            if (description.Messages.Count == 2)
            {
                description3 = description.Messages[1];
            }
            if (description2.Headers.Count > 0)
            {
                return false;
            }
            if (description2.Properties.Count > 0)
            {
                return false;
            }
            if (description2.IsTypedMessage)
            {
                return false;
            }
            if (description3 != null)
            {
                if (description3.Headers.Count > 0)
                {
                    return false;
                }
                if (description3.Properties.Count > 0)
                {
                    return false;
                }
                if (description3.IsTypedMessage)
                {
                    return false;
                }
            }
            if (!AreTypesSupported(description2.Body.Parts))
            {
                return false;
            }
            if (description3 != null)
            {
                if (!AreTypesSupported(description3.Body.Parts))
                {
                    return false;
                }
                if ((description3.Body.ReturnValue != null) && !IsTypeSupported(description3.Body.ReturnValue))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsPartElement(XmlDictionaryReader reader, PartInfo part)
        {
            return reader.IsStartElement(part.DictionaryName, part.DictionaryNamespace);
        }

        private bool IsPartElements(XmlDictionaryReader reader, PartInfo[] parts)
        {
            foreach (PartInfo info in parts)
            {
                if (this.IsPartElement(reader, info))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsTypeSupported(MessagePartDescription bodyDescription)
        {
            System.Type type = bodyDescription.Type;
            if (type == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxMessagePartDescriptionMissingType", new object[] { bodyDescription.Name, bodyDescription.Namespace })));
            }
            if (!bodyDescription.Multiple)
            {
                if (type == typeof(void))
                {
                    return true;
                }
                if (type.IsEnum)
                {
                    return false;
                }
                switch (System.Type.GetTypeCode(type))
                {
                    case TypeCode.Object:
                        if ((!type.IsArray || (type.GetArrayRank() != 1)) || !IsArrayTypeSupported(type.GetElementType()))
                        {
                            break;
                        }
                        return true;

                    case TypeCode.Boolean:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                    case TypeCode.DateTime:
                    case TypeCode.String:
                        return true;
                }
            }
            return false;
        }

        private void SerializeParameter(XmlDictionaryWriter writer, PartInfo part, object graph)
        {
            writer.WriteStartElement(part.DictionaryName, part.DictionaryNamespace);
            if (graph == null)
            {
                writer.WriteStartAttribute(this.xsiNilLocalName, this.xsiNilNamespace);
                writer.WriteValue(true);
                writer.WriteEndAttribute();
            }
            else
            {
                part.WriteValue(writer, graph);
            }
            writer.WriteEndElement();
        }

        private void SerializeParameters(XmlDictionaryWriter writer, PartInfo[] parts, object[] parameters)
        {
            if (parts.Length != parameters.Length)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SFxParameterCountMismatch", new object[] { "parts", parts.Length, "parameters", parameters.Length }), "parameters"));
            }
            for (int i = 0; i < parts.Length; i++)
            {
                PartInfo part = parts[i];
                this.SerializeParameter(writer, part, parameters[part.Description.Index]);
            }
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            if (messageVersion == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageVersion");
            }
            if (parameters == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            return Message.CreateMessage(messageVersion, this.GetReplyActionHeader(messageVersion.Addressing), new PrimitiveResponseBodyWriter(parameters, result, this));
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            if (messageVersion == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageVersion");
            }
            if (parameters == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            return Message.CreateMessage(messageVersion, this.GetActionHeader(messageVersion.Addressing), new PrimitiveRequestBodyWriter(parameters, this));
        }

        private void SerializeRequest(XmlDictionaryWriter writer, object[] parameters)
        {
            if (this.requestWrapperName != null)
            {
                writer.WriteStartElement(this.requestWrapperName, this.requestWrapperNamespace);
            }
            this.SerializeParameters(writer, this.requestParts, parameters);
            if (this.requestWrapperName != null)
            {
                writer.WriteEndElement();
            }
        }

        private void SerializeResponse(XmlDictionaryWriter writer, object returnValue, object[] parameters)
        {
            if (this.responseWrapperName != null)
            {
                writer.WriteStartElement(this.responseWrapperName, this.responseWrapperNamespace);
            }
            if (this.returnPart != null)
            {
                this.SerializeParameter(writer, this.returnPart, returnValue);
            }
            this.SerializeParameters(writer, this.responseParts, parameters);
            if (this.responseWrapperName != null)
            {
                writer.WriteEndElement();
            }
        }

        private ActionHeader ActionHeader10
        {
            get
            {
                if (this.actionHeader10 == null)
                {
                    this.actionHeader10 = ActionHeader.Create(this.action, AddressingVersion.WSAddressing10);
                }
                return this.actionHeader10;
            }
        }

        private ActionHeader ActionHeaderAugust2004
        {
            get
            {
                if (this.actionHeaderAugust2004 == null)
                {
                    this.actionHeaderAugust2004 = ActionHeader.Create(this.action, AddressingVersion.WSAddressingAugust2004);
                }
                return this.actionHeaderAugust2004;
            }
        }

        private ActionHeader ActionHeaderNone
        {
            get
            {
                if (this.actionHeaderNone == null)
                {
                    this.actionHeaderNone = ActionHeader.Create(this.action, AddressingVersion.None);
                }
                return this.actionHeaderNone;
            }
        }

        private ActionHeader ReplyActionHeader10
        {
            get
            {
                if (this.replyActionHeader10 == null)
                {
                    this.replyActionHeader10 = ActionHeader.Create(this.replyAction, AddressingVersion.WSAddressing10);
                }
                return this.replyActionHeader10;
            }
        }

        private ActionHeader ReplyActionHeaderAugust2004
        {
            get
            {
                if (this.replyActionHeaderAugust2004 == null)
                {
                    this.replyActionHeaderAugust2004 = ActionHeader.Create(this.replyAction, AddressingVersion.WSAddressingAugust2004);
                }
                return this.replyActionHeaderAugust2004;
            }
        }

        private ActionHeader ReplyActionHeaderNone
        {
            get
            {
                if (this.replyActionHeaderNone == null)
                {
                    this.replyActionHeaderNone = ActionHeader.Create(this.replyAction, AddressingVersion.None);
                }
                return this.replyActionHeaderNone;
            }
        }

        private class PartInfo
        {
            private MessagePartDescription description;
            private XmlDictionaryString dictionaryName;
            private XmlDictionaryString dictionaryNamespace;
            private bool isArray;
            private XmlDictionaryString itemName;
            private XmlDictionaryString itemNamespace;
            private TypeCode typeCode;

            public PartInfo(MessagePartDescription description, XmlDictionaryString dictionaryName, XmlDictionaryString dictionaryNamespace, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
            {
                this.dictionaryName = dictionaryName;
                this.dictionaryNamespace = dictionaryNamespace;
                this.itemName = itemName;
                this.itemNamespace = itemNamespace;
                this.description = description;
                if (description.Type.IsArray)
                {
                    this.isArray = true;
                    this.typeCode = System.Type.GetTypeCode(description.Type.GetElementType());
                }
                else
                {
                    this.isArray = false;
                    this.typeCode = System.Type.GetTypeCode(description.Type);
                }
            }

            public object ReadValue(XmlDictionaryReader reader)
            {
                object obj2;
                if (!this.isArray)
                {
                    switch (this.typeCode)
                    {
                        case TypeCode.Int32:
                            return reader.ReadElementContentAsInt();

                        case TypeCode.Int64:
                            return reader.ReadElementContentAsLong();

                        case TypeCode.Single:
                            return reader.ReadElementContentAsFloat();

                        case TypeCode.Double:
                            return reader.ReadElementContentAsDouble();

                        case TypeCode.Decimal:
                            return reader.ReadElementContentAsDecimal();

                        case TypeCode.DateTime:
                            return reader.ReadElementContentAsDateTime();

                        case TypeCode.String:
                            return reader.ReadElementContentAsString();

                        case TypeCode.Boolean:
                            return reader.ReadElementContentAsBoolean();
                    }
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidUseOfPrimitiveOperationFormatter")));
                }
                switch (this.typeCode)
                {
                    case TypeCode.Boolean:
                        if (reader.IsEmptyElement)
                        {
                            reader.Read();
                            return new bool[0];
                        }
                        reader.ReadStartElement();
                        obj2 = reader.ReadBooleanArray(this.itemName, this.itemNamespace);
                        reader.ReadEndElement();
                        return obj2;

                    case TypeCode.Byte:
                        return reader.ReadElementContentAsBase64();

                    case TypeCode.Int32:
                        if (reader.IsEmptyElement)
                        {
                            reader.Read();
                            return new int[0];
                        }
                        reader.ReadStartElement();
                        obj2 = reader.ReadInt32Array(this.itemName, this.itemNamespace);
                        reader.ReadEndElement();
                        return obj2;

                    case TypeCode.Int64:
                        if (reader.IsEmptyElement)
                        {
                            reader.Read();
                            return new long[0];
                        }
                        reader.ReadStartElement();
                        obj2 = reader.ReadInt64Array(this.itemName, this.itemNamespace);
                        reader.ReadEndElement();
                        return obj2;

                    case TypeCode.Single:
                        if (reader.IsEmptyElement)
                        {
                            reader.Read();
                            return new float[0];
                        }
                        reader.ReadStartElement();
                        obj2 = reader.ReadSingleArray(this.itemName, this.itemNamespace);
                        reader.ReadEndElement();
                        return obj2;

                    case TypeCode.Double:
                        if (reader.IsEmptyElement)
                        {
                            reader.Read();
                            return new double[0];
                        }
                        reader.ReadStartElement();
                        obj2 = reader.ReadDoubleArray(this.itemName, this.itemNamespace);
                        reader.ReadEndElement();
                        return obj2;

                    case TypeCode.Decimal:
                        if (reader.IsEmptyElement)
                        {
                            reader.Read();
                            return new decimal[0];
                        }
                        reader.ReadStartElement();
                        obj2 = reader.ReadDecimalArray(this.itemName, this.itemNamespace);
                        reader.ReadEndElement();
                        return obj2;

                    case TypeCode.DateTime:
                        if (reader.IsEmptyElement)
                        {
                            reader.Read();
                            return new DateTime[0];
                        }
                        reader.ReadStartElement();
                        obj2 = reader.ReadDateTimeArray(this.itemName, this.itemNamespace);
                        reader.ReadEndElement();
                        return obj2;
                }
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidUseOfPrimitiveOperationFormatter")));
            }

            public void WriteValue(XmlDictionaryWriter writer, object value)
            {
                if (!this.isArray)
                {
                    switch (this.typeCode)
                    {
                        case TypeCode.Int32:
                            writer.WriteValue((int) value);
                            return;

                        case TypeCode.Int64:
                            writer.WriteValue((long) value);
                            return;

                        case TypeCode.Single:
                            writer.WriteValue((float) value);
                            return;

                        case TypeCode.Double:
                            writer.WriteValue((double) value);
                            return;

                        case TypeCode.Decimal:
                            writer.WriteValue((decimal) value);
                            return;

                        case TypeCode.DateTime:
                            writer.WriteValue((DateTime) value);
                            return;

                        case TypeCode.String:
                            writer.WriteString((string) value);
                            return;

                        case TypeCode.Boolean:
                            writer.WriteValue((bool) value);
                            return;
                    }
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidUseOfPrimitiveOperationFormatter")));
                }
                switch (this.typeCode)
                {
                    case TypeCode.Boolean:
                    {
                        bool[] array = (bool[]) value;
                        writer.WriteArray(null, this.itemName, this.itemNamespace, array, 0, array.Length);
                        return;
                    }
                    case TypeCode.Byte:
                    {
                        byte[] buffer = (byte[]) value;
                        writer.WriteBase64(buffer, 0, buffer.Length);
                        return;
                    }
                    case TypeCode.Int32:
                    {
                        int[] numArray2 = (int[]) value;
                        writer.WriteArray(null, this.itemName, this.itemNamespace, numArray2, 0, numArray2.Length);
                        return;
                    }
                    case TypeCode.Int64:
                    {
                        long[] numArray3 = (long[]) value;
                        writer.WriteArray(null, this.itemName, this.itemNamespace, numArray3, 0, numArray3.Length);
                        return;
                    }
                    case TypeCode.Single:
                    {
                        float[] numArray4 = (float[]) value;
                        writer.WriteArray(null, this.itemName, this.itemNamespace, numArray4, 0, numArray4.Length);
                        return;
                    }
                    case TypeCode.Double:
                    {
                        double[] numArray5 = (double[]) value;
                        writer.WriteArray(null, this.itemName, this.itemNamespace, numArray5, 0, numArray5.Length);
                        return;
                    }
                    case TypeCode.Decimal:
                    {
                        decimal[] numArray = (decimal[]) value;
                        writer.WriteArray(null, this.itemName, this.itemNamespace, numArray, 0, numArray.Length);
                        return;
                    }
                    case TypeCode.DateTime:
                    {
                        DateTime[] timeArray = (DateTime[]) value;
                        writer.WriteArray(null, this.itemName, this.itemNamespace, timeArray, 0, timeArray.Length);
                        return;
                    }
                }
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidUseOfPrimitiveOperationFormatter")));
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
        }

        private class PrimitiveRequestBodyWriter : BodyWriter
        {
            private object[] parameters;
            private PrimitiveOperationFormatter primitiveOperationFormatter;

            public PrimitiveRequestBodyWriter(object[] parameters, PrimitiveOperationFormatter primitiveOperationFormatter) : base(true)
            {
                this.parameters = parameters;
                this.primitiveOperationFormatter = primitiveOperationFormatter;
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                this.primitiveOperationFormatter.SerializeRequest(writer, this.parameters);
            }
        }

        private class PrimitiveResponseBodyWriter : BodyWriter
        {
            private object[] parameters;
            private PrimitiveOperationFormatter primitiveOperationFormatter;
            private object returnValue;

            public PrimitiveResponseBodyWriter(object[] parameters, object returnValue, PrimitiveOperationFormatter primitiveOperationFormatter) : base(true)
            {
                this.parameters = parameters;
                this.returnValue = returnValue;
                this.primitiveOperationFormatter = primitiveOperationFormatter;
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                this.primitiveOperationFormatter.SerializeResponse(writer, this.returnValue, this.parameters);
            }
        }
    }
}

