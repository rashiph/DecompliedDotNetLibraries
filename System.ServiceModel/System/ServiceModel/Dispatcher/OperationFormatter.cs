namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal abstract class OperationFormatter : IClientMessageFormatter, IDispatchMessageFormatter
    {
        private XmlDictionaryString action;
        private XmlDictionary dictionary;
        private static object[] emptyObjectArray = new object[0];
        private string operationName;
        private XmlDictionaryString replyAction;
        private MessageDescription replyDescription;
        protected StreamFormatter replyStreamFormatter;
        private MessageDescription requestDescription;
        protected StreamFormatter requestStreamFormatter;

        public OperationFormatter(OperationDescription description, bool isRpc, bool isEncoded)
        {
            Validate(description, isRpc, isEncoded);
            this.requestDescription = description.Messages[0];
            if (description.Messages.Count == 2)
            {
                this.replyDescription = description.Messages[1];
            }
            int num = 3 + this.requestDescription.Body.Parts.Count;
            if (this.replyDescription != null)
            {
                num += 2 + this.replyDescription.Body.Parts.Count;
            }
            this.dictionary = new XmlDictionary(num * 2);
            GetActions(description, this.dictionary, out this.action, out this.replyAction);
            this.operationName = description.Name;
            this.requestStreamFormatter = StreamFormatter.Create(this.requestDescription, this.operationName, true);
            if (this.replyDescription != null)
            {
                this.replyStreamFormatter = StreamFormatter.Create(this.replyDescription, this.operationName, false);
            }
        }

        protected abstract void AddHeadersToMessage(Message message, MessageDescription messageDescription, object[] parameters, bool isRequest);
        private void AddPropertiesToMessage(Message message, MessageDescription messageDescription, object[] parameters)
        {
            if (messageDescription.Properties.Count > 0)
            {
                this.AddPropertiesToMessageCore(message, messageDescription, parameters);
            }
        }

        private void AddPropertiesToMessageCore(Message message, MessageDescription messageDescription, object[] parameters)
        {
            MessageProperties properties = message.Properties;
            MessagePropertyDescriptionCollection descriptions = messageDescription.Properties;
            for (int i = 0; i < descriptions.Count; i++)
            {
                MessagePropertyDescription description = descriptions[i];
                object property = parameters[description.Index];
                if (property != null)
                {
                    properties.Add(description.Name, property);
                }
            }
        }

        protected XmlDictionaryString AddToDictionary(string s)
        {
            return AddToDictionary(this.dictionary, s);
        }

        internal static XmlDictionaryString AddToDictionary(XmlDictionary dictionary, string s)
        {
            XmlDictionaryString str;
            if (!dictionary.TryLookup(s, out str))
            {
                str = dictionary.Add(s);
            }
            return str;
        }

        internal static NetDispatcherFaultException CreateDeserializationFailedFault(string reason, Exception innerException)
        {
            reason = System.ServiceModel.SR.GetString("SFxDeserializationFailed1", new object[] { reason });
            FaultCode subCode = new FaultCode("DeserializationFailed", "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher");
            return new NetDispatcherFaultException(reason, FaultCode.CreateSenderFaultCode(subCode), innerException);
        }

        private static object CreateTypedMessageInstance(System.Type messageContractType)
        {
            object obj3;
            BindingFlags bindingAttr = BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            try
            {
                obj3 = Activator.CreateInstance(messageContractType, bindingAttr, null, emptyObjectArray, null);
            }
            catch (MissingMethodException exception)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxMessageContractRequiresDefaultConstructor", new object[] { messageContractType.FullName }), exception));
            }
            return obj3;
        }

        protected abstract object DeserializeBody(XmlDictionaryReader reader, MessageVersion version, string action, MessageDescription messageDescription, object[] parameters, bool isRequest);
        private object DeserializeBodyContents(Message message, object[] parameters, bool isRequest)
        {
            MessageDescription requestDescription;
            StreamFormatter requestStreamFormatter;
            if (isRequest)
            {
                requestStreamFormatter = this.requestStreamFormatter;
                requestDescription = this.requestDescription;
            }
            else
            {
                requestStreamFormatter = this.replyStreamFormatter;
                requestDescription = this.replyDescription;
            }
            if (requestStreamFormatter != null)
            {
                object retVal = null;
                requestStreamFormatter.Deserialize(parameters, ref retVal, message);
                return retVal;
            }
            if (message.IsEmpty)
            {
                return null;
            }
            XmlDictionaryReader readerAtBodyContents = message.GetReaderAtBodyContents();
            using (readerAtBodyContents)
            {
                object obj3 = this.DeserializeBody(readerAtBodyContents, message.Version, this.RequestAction, requestDescription, parameters, isRequest);
                message.ReadFromBodyContentsToEnd(readerAtBodyContents);
                return obj3;
            }
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            object obj4;
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (parameters == null)
            {
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("parameters"), message);
            }
            try
            {
                object obj2 = null;
                if (this.replyDescription.IsTypedMessage)
                {
                    object instance = CreateTypedMessageInstance(this.replyDescription.MessageType);
                    TypedMessageParts parts = new TypedMessageParts(instance, this.replyDescription);
                    object[] objArray = new object[parts.Count];
                    this.GetPropertiesFromMessage(message, this.replyDescription, objArray);
                    this.GetHeadersFromMessage(message, this.replyDescription, objArray, false);
                    this.DeserializeBodyContents(message, objArray, false);
                    parts.SetTypedMessageParts(objArray);
                    obj2 = instance;
                }
                else
                {
                    this.GetPropertiesFromMessage(message, this.replyDescription, parameters);
                    this.GetHeadersFromMessage(message, this.replyDescription, parameters, false);
                    obj2 = this.DeserializeBodyContents(message, parameters, false);
                }
                obj4 = obj2;
            }
            catch (XmlException exception)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorDeserializingReplyBodyMore", new object[] { this.operationName, exception.Message }), exception));
            }
            catch (FormatException exception2)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorDeserializingReplyBodyMore", new object[] { this.operationName, exception2.Message }), exception2));
            }
            catch (SerializationException exception3)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorDeserializingReplyBodyMore", new object[] { this.operationName, exception3.Message }), exception3));
            }
            return obj4;
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (parameters == null)
            {
                throw TraceUtility.ThrowHelperError(new ArgumentNullException("parameters"), message);
            }
            try
            {
                if (this.requestDescription.IsTypedMessage)
                {
                    object instance = CreateTypedMessageInstance(this.requestDescription.MessageType);
                    TypedMessageParts parts = new TypedMessageParts(instance, this.requestDescription);
                    object[] objArray = new object[parts.Count];
                    this.GetPropertiesFromMessage(message, this.requestDescription, objArray);
                    this.GetHeadersFromMessage(message, this.requestDescription, objArray, true);
                    this.DeserializeBodyContents(message, objArray, true);
                    parts.SetTypedMessageParts(objArray);
                    parameters[0] = instance;
                }
                else
                {
                    this.GetPropertiesFromMessage(message, this.requestDescription, parameters);
                    this.GetHeadersFromMessage(message, this.requestDescription, parameters, true);
                    this.DeserializeBodyContents(message, parameters, true);
                }
            }
            catch (XmlException exception)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateDeserializationFailedFault(System.ServiceModel.SR.GetString("SFxErrorDeserializingRequestBodyMore", new object[] { this.operationName, exception.Message }), exception));
            }
            catch (FormatException exception2)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateDeserializationFailedFault(System.ServiceModel.SR.GetString("SFxErrorDeserializingRequestBodyMore", new object[] { this.operationName, exception2.Message }), exception2));
            }
            catch (SerializationException exception3)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorDeserializingRequestBodyMore", new object[] { this.operationName, exception3.Message }), exception3));
            }
        }

        internal static void GetActions(OperationDescription description, XmlDictionary dictionary, out XmlDictionaryString action, out XmlDictionaryString replyAction)
        {
            string str2;
            XmlDictionaryString str3;
            string s = description.Messages[0].Action;
            if (s == "*")
            {
                s = null;
            }
            if (!description.IsOneWay)
            {
                str2 = description.Messages[1].Action;
            }
            else
            {
                str2 = null;
            }
            if (str2 == "*")
            {
                str2 = null;
            }
            replyAction = (XmlDictionaryString) (str3 = null);
            action = str3;
            if (s != null)
            {
                action = AddToDictionary(dictionary, s);
            }
            if (str2 != null)
            {
                replyAction = AddToDictionary(dictionary, str2);
            }
        }

        internal static object GetContentOfMessageHeaderOfT(MessageHeaderDescription headerDescription, object parameterValue, out bool mustUnderstand, out bool relay, out string actor)
        {
            actor = headerDescription.Actor;
            mustUnderstand = headerDescription.MustUnderstand;
            relay = headerDescription.Relay;
            if (headerDescription.TypedHeader && (parameterValue != null))
            {
                parameterValue = TypedHeaderManager.GetContent(headerDescription.Type, parameterValue, out mustUnderstand, out relay, out actor);
            }
            return parameterValue;
        }

        protected abstract void GetHeadersFromMessage(Message message, MessageDescription messageDescription, object[] parameters, bool isRequest);
        private void GetPropertiesFromMessage(Message message, MessageDescription messageDescription, object[] parameters)
        {
            if (messageDescription.Properties.Count > 0)
            {
                this.GetPropertiesFromMessageCore(message, messageDescription, parameters);
            }
        }

        private void GetPropertiesFromMessageCore(Message message, MessageDescription messageDescription, object[] parameters)
        {
            MessageProperties properties = message.Properties;
            MessagePropertyDescriptionCollection descriptions = messageDescription.Properties;
            for (int i = 0; i < descriptions.Count; i++)
            {
                MessagePropertyDescription description = descriptions[i];
                if (properties.ContainsKey(description.Name))
                {
                    parameters[description.Index] = properties[description.Name];
                }
            }
        }

        internal static bool IsValidReturnValue(MessagePartDescription returnValue)
        {
            return ((returnValue != null) && (returnValue.Type != typeof(void)));
        }

        protected abstract void SerializeBody(XmlDictionaryWriter writer, MessageVersion version, string action, MessageDescription messageDescription, object returnValue, object[] parameters, bool isRequest);
        private void SerializeBodyContents(XmlDictionaryWriter writer, MessageVersion version, object[] parameters, object returnValue, bool isRequest)
        {
            MessageDescription requestDescription;
            StreamFormatter requestStreamFormatter;
            if (isRequest)
            {
                requestStreamFormatter = this.requestStreamFormatter;
                string requestAction = this.RequestAction;
                requestDescription = this.requestDescription;
            }
            else
            {
                requestStreamFormatter = this.replyStreamFormatter;
                string replyAction = this.ReplyAction;
                requestDescription = this.replyDescription;
            }
            if (requestStreamFormatter != null)
            {
                requestStreamFormatter.Serialize(writer, parameters, returnValue);
            }
            else
            {
                this.SerializeBody(writer, version, this.RequestAction, requestDescription, returnValue, parameters, isRequest);
            }
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            object[] values = null;
            object returnValue = null;
            if (messageVersion == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageVersion");
            }
            if (parameters == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            if (this.replyDescription.IsTypedMessage)
            {
                TypedMessageParts parts = new TypedMessageParts(result, this.replyDescription);
                values = new object[parts.Count];
                parts.GetTypedMessageParts(values);
                returnValue = null;
            }
            else
            {
                values = parameters;
                returnValue = result;
            }
            Message message = new OperationFormatterMessage(this, messageVersion, (this.replyAction == null) ? null : ActionHeader.Create(this.replyAction, messageVersion.Addressing), values, returnValue, false);
            this.AddPropertiesToMessage(message, this.replyDescription, values);
            this.AddHeadersToMessage(message, this.replyDescription, values, false);
            return message;
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            object[] values = null;
            if (messageVersion == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageVersion");
            }
            if (parameters == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            if (this.requestDescription.IsTypedMessage)
            {
                TypedMessageParts parts = new TypedMessageParts(parameters[0], this.requestDescription);
                values = new object[parts.Count];
                parts.GetTypedMessageParts(values);
            }
            else
            {
                values = parameters;
            }
            Message message = new OperationFormatterMessage(this, messageVersion, (this.action == null) ? null : ActionHeader.Create(this.action, messageVersion.Addressing), values, null, true);
            this.AddPropertiesToMessage(message, this.requestDescription, values);
            this.AddHeadersToMessage(message, this.requestDescription, values, true);
            return message;
        }

        internal static void TraceAndSkipElement(XmlReader xmlReader)
        {
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x30007, System.ServiceModel.SR.GetString("SFxTraceCodeElementIgnored"), (TraceRecord) new StringTraceRecord("Element", xmlReader.NamespaceURI + ":" + xmlReader.LocalName));
            }
            xmlReader.Skip();
        }

        internal static void Validate(OperationDescription operation, bool isRpc, bool isEncoded)
        {
            if (isEncoded && !isRpc)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxDocEncodedNotSupported", new object[] { operation.Name })));
            }
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            for (int i = 0; i < operation.Messages.Count; i++)
            {
                MessageDescription description = operation.Messages[i];
                if (description.IsTypedMessage || description.IsUntypedMessage)
                {
                    if ((isRpc && operation.IsValidateRpcWrapperName) && !isEncoded)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTypedMessageCannotBeRpcLiteral", new object[] { operation.Name })));
                    }
                    flag2 = true;
                }
                else if (description.IsVoid)
                {
                    flag = true;
                }
                else
                {
                    flag3 = true;
                }
            }
            if (flag3 && flag2)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTypedOrUntypedMessageCannotBeMixedWithParameters", new object[] { operation.Name })));
            }
            if ((isRpc && flag2) && flag)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTypedOrUntypedMessageCannotBeMixedWithVoidInRpc", new object[] { operation.Name })));
            }
        }

        protected virtual void WriteBodyAttributes(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
        }

        protected XmlDictionary Dictionary
        {
            get
            {
                return this.dictionary;
            }
        }

        protected string OperationName
        {
            get
            {
                return this.operationName;
            }
        }

        internal string ReplyAction
        {
            get
            {
                if (this.replyAction != null)
                {
                    return this.replyAction.Value;
                }
                return null;
            }
        }

        protected MessageDescription ReplyDescription
        {
            get
            {
                return this.replyDescription;
            }
        }

        internal string RequestAction
        {
            get
            {
                if (this.action != null)
                {
                    return this.action.Value;
                }
                return null;
            }
        }

        protected MessageDescription RequestDescription
        {
            get
            {
                return this.requestDescription;
            }
        }

        internal class MessageHeaderDescriptionTable : Dictionary<OperationFormatter.QName, MessageHeaderDescription>
        {
            internal MessageHeaderDescriptionTable() : base(OperationFormatter.QNameComparer.Singleton)
            {
            }

            internal void Add(string name, string ns, MessageHeaderDescription message)
            {
                base.Add(new OperationFormatter.QName(name, ns), message);
            }

            internal MessageHeaderDescription Get(string name, string ns)
            {
                MessageHeaderDescription description;
                if (base.TryGetValue(new OperationFormatter.QName(name, ns), out description))
                {
                    return description;
                }
                return null;
            }
        }

        internal abstract class OperationFormatterHeader : MessageHeader
        {
            protected MessageHeader innerHeader;
            protected OperationFormatter operationFormatter;
            protected MessageVersion version;

            public OperationFormatterHeader(OperationFormatter operationFormatter, MessageVersion version, string name, string ns, bool mustUnderstand, string actor, bool relay)
            {
                this.operationFormatter = operationFormatter;
                this.version = version;
                if (actor != null)
                {
                    this.innerHeader = MessageHeader.CreateHeader(name, ns, null, mustUnderstand, actor, relay);
                }
                else
                {
                    this.innerHeader = MessageHeader.CreateHeader(name, ns, null, mustUnderstand, "", relay);
                }
            }

            public override bool IsMessageVersionSupported(MessageVersion messageVersion)
            {
                return this.innerHeader.IsMessageVersionSupported(messageVersion);
            }

            protected virtual void OnWriteHeaderAttributes(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                base.WriteHeaderAttributes(writer, messageVersion);
            }

            protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteStartElement(((this.Namespace == null) || (this.Namespace.Length == 0)) ? string.Empty : "h", this.Name, this.Namespace);
                this.OnWriteHeaderAttributes(writer, messageVersion);
            }

            public override string Actor
            {
                get
                {
                    return this.innerHeader.Actor;
                }
            }

            public override bool MustUnderstand
            {
                get
                {
                    return this.innerHeader.MustUnderstand;
                }
            }

            public override string Name
            {
                get
                {
                    return this.innerHeader.Name;
                }
            }

            public override string Namespace
            {
                get
                {
                    return this.innerHeader.Namespace;
                }
            }

            public override bool Relay
            {
                get
                {
                    return this.innerHeader.Relay;
                }
            }
        }

        internal class OperationFormatterMessage : BodyWriterMessage
        {
            private OperationFormatter operationFormatter;

            private OperationFormatterMessage(MessageHeaders headers, KeyValuePair<string, object>[] properties, OperationFormatterBodyWriter bodyWriter) : base(headers, properties, bodyWriter)
            {
                this.operationFormatter = bodyWriter.OperationFormatter;
            }

            public OperationFormatterMessage(MessageVersion version, string action, BodyWriter bodyWriter) : base(version, action, bodyWriter)
            {
            }

            public OperationFormatterMessage(OperationFormatter operationFormatter, MessageVersion version, ActionHeader action, object[] parameters, object returnValue, bool isRequest) : base(version, action, new OperationFormatterBodyWriter(operationFormatter, version, parameters, returnValue, isRequest))
            {
                this.operationFormatter = operationFormatter;
            }

            protected override MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
            {
                BodyWriter bodyWriter;
                if (base.BodyWriter.IsBuffered)
                {
                    bodyWriter = base.BodyWriter;
                }
                else
                {
                    bodyWriter = base.BodyWriter.CreateBufferedCopy(maxBufferSize);
                }
                KeyValuePair<string, object>[] array = new KeyValuePair<string, object>[base.Properties.Count];
                ((ICollection<KeyValuePair<string, object>>) base.Properties).CopyTo(array, 0);
                return new OperationFormatterMessageBuffer(base.Headers, array, bodyWriter);
            }

            protected override void OnWriteStartBody(XmlDictionaryWriter writer)
            {
                base.OnWriteStartBody(writer);
                this.operationFormatter.WriteBodyAttributes(writer, this.Version);
            }

            private class OperationFormatterBodyWriter : BodyWriter
            {
                private bool isRequest;
                private System.ServiceModel.Dispatcher.OperationFormatter operationFormatter;
                private object[] parameters;
                private object returnValue;
                private MessageVersion version;

                public OperationFormatterBodyWriter(System.ServiceModel.Dispatcher.OperationFormatter operationFormatter, MessageVersion version, object[] parameters, object returnValue, bool isRequest) : base(AreParametersBuffered(isRequest, operationFormatter))
                {
                    this.parameters = parameters;
                    this.returnValue = returnValue;
                    this.isRequest = isRequest;
                    this.operationFormatter = operationFormatter;
                    this.version = version;
                }

                private static bool AreParametersBuffered(bool isRequest, System.ServiceModel.Dispatcher.OperationFormatter operationFormatter)
                {
                    StreamFormatter formatter = isRequest ? operationFormatter.requestStreamFormatter : operationFormatter.replyStreamFormatter;
                    return (formatter == null);
                }

                protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
                {
                    lock (this.ThisLock)
                    {
                        this.operationFormatter.SerializeBodyContents(writer, this.version, this.parameters, this.returnValue, this.isRequest);
                    }
                }

                internal System.ServiceModel.Dispatcher.OperationFormatter OperationFormatter
                {
                    get
                    {
                        return this.operationFormatter;
                    }
                }

                private object ThisLock
                {
                    get
                    {
                        return this;
                    }
                }
            }

            private class OperationFormatterMessageBuffer : BodyWriterMessageBuffer
            {
                public OperationFormatterMessageBuffer(MessageHeaders headers, KeyValuePair<string, object>[] properties, BodyWriter bodyWriter) : base(headers, properties, bodyWriter)
                {
                }

                public override Message CreateMessage()
                {
                    OperationFormatter.OperationFormatterMessage.OperationFormatterBodyWriter bodyWriter = base.BodyWriter as OperationFormatter.OperationFormatterMessage.OperationFormatterBodyWriter;
                    if (bodyWriter == null)
                    {
                        return base.CreateMessage();
                    }
                    lock (base.ThisLock)
                    {
                        if (base.Closed)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateBufferDisposedException());
                        }
                        return new OperationFormatter.OperationFormatterMessage(base.Headers, base.Properties, bodyWriter);
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct QName
        {
            internal string Name;
            internal string Namespace;
            internal QName(string name, string ns)
            {
                this.Name = name;
                this.Namespace = ns;
            }
        }

        internal class QNameComparer : IEqualityComparer<OperationFormatter.QName>
        {
            internal static OperationFormatter.QNameComparer Singleton = new OperationFormatter.QNameComparer();

            private QNameComparer()
            {
            }

            public bool Equals(OperationFormatter.QName x, OperationFormatter.QName y)
            {
                return ((x.Name == y.Name) && (x.Namespace == y.Namespace));
            }

            public int GetHashCode(OperationFormatter.QName obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        private class TypedMessageParts
        {
            private object instance;
            private MemberInfo[] members;

            public TypedMessageParts(object instance, MessageDescription description)
            {
                if (description == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("description"));
                }
                if (instance == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException(System.ServiceModel.SR.GetString("SFxTypedMessageCannotBeNull", new object[] { description.Action })));
                }
                this.members = new MemberInfo[(description.Body.Parts.Count + description.Properties.Count) + description.Headers.Count];
                foreach (MessagePartDescription description2 in description.Headers)
                {
                    this.members[description2.Index] = description2.MemberInfo;
                }
                foreach (MessagePartDescription description3 in description.Properties)
                {
                    this.members[description3.Index] = description3.MemberInfo;
                }
                foreach (MessagePartDescription description4 in description.Body.Parts)
                {
                    this.members[description4.Index] = description4.MemberInfo;
                }
                this.instance = instance;
            }

            internal void GetTypedMessageParts(object[] values)
            {
                for (int i = 0; i < this.members.Length; i++)
                {
                    values[i] = this.GetValue(i);
                }
            }

            private object GetValue(int index)
            {
                MemberInfo info = this.members[index];
                if (info.MemberType == MemberTypes.Property)
                {
                    return ((PropertyInfo) this.members[index]).GetValue(this.instance, null);
                }
                return ((FieldInfo) this.members[index]).GetValue(this.instance);
            }

            internal void SetTypedMessageParts(object[] values)
            {
                for (int i = 0; i < this.members.Length; i++)
                {
                    this.SetValue(values[i], i);
                }
            }

            private void SetValue(object value, int index)
            {
                MemberInfo info = this.members[index];
                if (info.MemberType == MemberTypes.Property)
                {
                    ((PropertyInfo) this.members[index]).SetValue(this.instance, value, null);
                }
                else
                {
                    ((FieldInfo) this.members[index]).SetValue(this.instance, value);
                }
            }

            internal int Count
            {
                get
                {
                    return this.members.Length;
                }
            }
        }

        internal class XmlElementMessageHeader : OperationFormatter.OperationFormatterHeader
        {
            protected XmlElement headerValue;

            public XmlElementMessageHeader(OperationFormatter operationFormatter, MessageVersion version, string name, string ns, bool mustUnderstand, string actor, bool relay, XmlElement headerValue) : base(operationFormatter, version, name, ns, mustUnderstand, actor, relay)
            {
                this.headerValue = headerValue;
            }

            protected override void OnWriteHeaderAttributes(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                base.WriteHeaderAttributes(writer, messageVersion);
                XmlDictionaryReader reader = XmlDictionaryReader.CreateDictionaryReader(new XmlNodeReader(this.headerValue));
                reader.MoveToContent();
                writer.WriteAttributes(reader, false);
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                this.headerValue.WriteContentTo(writer);
            }
        }
    }
}

