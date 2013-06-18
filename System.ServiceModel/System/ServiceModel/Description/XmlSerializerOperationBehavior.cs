namespace System.ServiceModel.Description
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSerializerOperationBehavior : IOperationBehavior, IWsdlExportExtension
    {
        private readonly bool builtInOperationBehavior;
        private readonly System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector.OperationReflector reflector;

        public XmlSerializerOperationBehavior(OperationDescription operation) : this(operation, null)
        {
        }

        public XmlSerializerOperationBehavior(OperationDescription operation, System.ServiceModel.XmlSerializerFormatAttribute attribute)
        {
            if (operation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operation");
            }
            this.reflector = new Reflector(operation.DeclaringContract.Namespace, operation.DeclaringContract.ContractType).ReflectOperation(operation, attribute ?? new System.ServiceModel.XmlSerializerFormatAttribute());
        }

        private XmlSerializerOperationBehavior(System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector.OperationReflector reflector, bool builtInOperationBehavior)
        {
            this.reflector = reflector;
            this.builtInOperationBehavior = builtInOperationBehavior;
        }

        internal XmlSerializerOperationBehavior(OperationDescription operation, System.ServiceModel.XmlSerializerFormatAttribute attribute, Reflector parentReflector) : this(operation, attribute)
        {
            this.reflector = parentReflector.ReflectOperation(operation, attribute ?? new System.ServiceModel.XmlSerializerFormatAttribute());
        }

        internal static void AddBehaviors(ContractDescription contract)
        {
            AddBehaviors(contract, false);
        }

        private static void AddBehaviors(ContractDescription contract, bool builtInOperationBehavior)
        {
            Reflector reflector = new Reflector(contract.Namespace, contract.ContractType);
            foreach (OperationDescription description in contract.Operations)
            {
                System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector.OperationReflector reflector2 = reflector.ReflectOperation(description);
                if ((reflector2 != null) && (description.DeclaringContract == contract))
                {
                    description.Behaviors.Add(new XmlSerializerOperationBehavior(reflector2, builtInOperationBehavior));
                    description.Behaviors.Add(new XmlSerializerOperationGenerator(new XmlSerializerImportOptions()));
                }
            }
        }

        internal static void AddBuiltInBehaviors(ContractDescription contract)
        {
            AddBehaviors(contract, true);
        }

        private XmlSerializerFaultFormatter CreateFaultFormatter(SynchronizedCollection<FaultContractInfo> faultContractInfos)
        {
            return new XmlSerializerFaultFormatter(faultContractInfos, this.reflector.XmlSerializerFaultContractInfos);
        }

        internal XmlSerializerOperationFormatter CreateFormatter()
        {
            return new XmlSerializerOperationFormatter(this.reflector.Operation, this.reflector.Attribute, this.reflector.Request, this.reflector.Reply);
        }

        internal static XmlSerializerOperationFormatter CreateOperationFormatter(OperationDescription operation)
        {
            return new XmlSerializerOperationBehavior(operation).CreateFormatter();
        }

        internal static XmlSerializerOperationFormatter CreateOperationFormatter(OperationDescription operation, System.ServiceModel.XmlSerializerFormatAttribute attr)
        {
            return new XmlSerializerOperationBehavior(operation, attr).CreateFormatter();
        }

        public Collection<XmlMapping> GetXmlMappings()
        {
            Collection<XmlMapping> collection = new Collection<XmlMapping>();
            if ((this.OperationReflector.Request != null) && (this.OperationReflector.Request.HeadersMapping != null))
            {
                collection.Add(this.OperationReflector.Request.HeadersMapping);
            }
            if ((this.OperationReflector.Request != null) && (this.OperationReflector.Request.BodyMapping != null))
            {
                collection.Add(this.OperationReflector.Request.BodyMapping);
            }
            if ((this.OperationReflector.Reply != null) && (this.OperationReflector.Reply.HeadersMapping != null))
            {
                collection.Add(this.OperationReflector.Reply.HeadersMapping);
            }
            if ((this.OperationReflector.Reply != null) && (this.OperationReflector.Reply.BodyMapping != null))
            {
                collection.Add(this.OperationReflector.Reply.BodyMapping);
            }
            return collection;
        }

        void IOperationBehavior.AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
        {
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription description, ClientOperation proxy)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            if (proxy == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("proxy");
            }
            if (proxy.Formatter == null)
            {
                proxy.Formatter = this.CreateFormatter();
                proxy.SerializeRequest = this.reflector.RequestRequiresSerialization;
                proxy.DeserializeReply = this.reflector.ReplyRequiresSerialization;
            }
            if (this.reflector.Attribute.SupportFaults && !proxy.IsFaultFormatterSetExplicit)
            {
                proxy.FaultFormatter = this.CreateFaultFormatter(proxy.FaultContractInfos);
            }
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            if (dispatch == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatch");
            }
            if (dispatch.Formatter == null)
            {
                dispatch.Formatter = this.CreateFormatter();
                dispatch.DeserializeRequest = this.reflector.RequestRequiresSerialization;
                dispatch.SerializeReply = this.reflector.ReplyRequiresSerialization;
            }
            if (this.reflector.Attribute.SupportFaults && !dispatch.IsFaultFormatterSetExplicit)
            {
                dispatch.FaultFormatter = this.CreateFaultFormatter(dispatch.FaultContractInfos);
            }
        }

        void IOperationBehavior.Validate(OperationDescription description)
        {
        }

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext contractContext)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }
            if (contractContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractContext");
            }
            new XmlSerializerMessageContractExporter(exporter, contractContext, this.reflector.Operation, this).ExportMessageContract();
        }

        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }
            if (endpointContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointContext");
            }
            MessageContractExporter.ExportMessageBinding(exporter, endpointContext, typeof(XmlSerializerMessageContractExporter), this.reflector.Operation);
        }

        internal bool IsBuiltInOperationBehavior
        {
            get
            {
                return this.builtInOperationBehavior;
            }
        }

        internal System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector.OperationReflector OperationReflector
        {
            get
            {
                return this.reflector;
            }
        }

        public System.ServiceModel.XmlSerializerFormatAttribute XmlSerializerFormatAttribute
        {
            get
            {
                return this.reflector.Attribute;
            }
        }

        internal class Reflector
        {
            private readonly SerializerGenerationContext generation;
            private readonly XmlSerializerImporter importer;
            private Collection<OperationReflector> operationReflectors = new Collection<OperationReflector>();
            private object thisLock = new object();

            internal Reflector(string defaultNs, System.Type type)
            {
                this.importer = new XmlSerializerImporter(defaultNs);
                this.generation = new SerializerGenerationContext(type);
            }

            internal void EnsureMessageInfos()
            {
                lock (this.thisLock)
                {
                    foreach (OperationReflector reflector in this.operationReflectors)
                    {
                        reflector.EnsureMessageInfos();
                    }
                }
            }

            private static XmlSerializerFormatAttribute FindAttribute(OperationDescription operation)
            {
                System.Type attrProvider = (operation.DeclaringContract != null) ? operation.DeclaringContract.ContractType : null;
                XmlSerializerFormatAttribute defaultFormatAttribute = (attrProvider != null) ? (TypeLoader.GetFormattingAttribute(attrProvider, null) as XmlSerializerFormatAttribute) : null;
                return (TypeLoader.GetFormattingAttribute(operation.OperationMethod, defaultFormatAttribute) as XmlSerializerFormatAttribute);
            }

            internal OperationReflector ReflectOperation(OperationDescription operation)
            {
                XmlSerializerFormatAttribute attrOverride = FindAttribute(operation);
                if (attrOverride == null)
                {
                    return null;
                }
                return this.ReflectOperation(operation, attrOverride);
            }

            internal OperationReflector ReflectOperation(OperationDescription operation, XmlSerializerFormatAttribute attrOverride)
            {
                OperationReflector item = new OperationReflector(this, operation, attrOverride, true);
                this.operationReflectors.Add(item);
                return item;
            }

            internal class MessageInfo : XmlSerializerOperationFormatter.MessageInfo
            {
                private XmlSerializerOperationBehavior.Reflector.SerializerStub body;
                private OperationFormatter.MessageHeaderDescriptionTable headerDescriptionTable;
                private XmlSerializerOperationBehavior.Reflector.SerializerStub headers;
                private MessagePartDescriptionCollection rpcEncodedTypedMessageBodyParts;
                private MessageHeaderDescription unknownHeaderDescription;

                internal void SetBody(XmlSerializerOperationBehavior.Reflector.SerializerStub body, MessagePartDescriptionCollection rpcEncodedTypedMessageBodyParts)
                {
                    this.body = body;
                    this.rpcEncodedTypedMessageBodyParts = rpcEncodedTypedMessageBodyParts;
                }

                internal void SetHeaderDescriptionTable(OperationFormatter.MessageHeaderDescriptionTable headerDescriptionTable)
                {
                    this.headerDescriptionTable = headerDescriptionTable;
                }

                internal void SetHeaders(XmlSerializerOperationBehavior.Reflector.SerializerStub headers)
                {
                    this.headers = headers;
                }

                internal void SetUnknownHeaderDescription(MessageHeaderDescription unknownHeaderDescription)
                {
                    this.unknownHeaderDescription = unknownHeaderDescription;
                }

                internal XmlMembersMapping BodyMapping
                {
                    get
                    {
                        return this.body.Mapping;
                    }
                }

                internal override XmlSerializer BodySerializer
                {
                    get
                    {
                        return this.body.GetSerializer();
                    }
                }

                internal override OperationFormatter.MessageHeaderDescriptionTable HeaderDescriptionTable
                {
                    get
                    {
                        return this.headerDescriptionTable;
                    }
                }

                internal override XmlSerializer HeaderSerializer
                {
                    get
                    {
                        return this.headers.GetSerializer();
                    }
                }

                internal XmlMembersMapping HeadersMapping
                {
                    get
                    {
                        return this.headers.Mapping;
                    }
                }

                internal override MessagePartDescriptionCollection RpcEncodedTypedMessageBodyParts
                {
                    get
                    {
                        return this.rpcEncodedTypedMessageBodyParts;
                    }
                }

                internal override MessageHeaderDescription UnknownHeaderDescription
                {
                    get
                    {
                        return this.unknownHeaderDescription;
                    }
                }
            }

            internal class OperationReflector
            {
                internal readonly XmlSerializerFormatAttribute Attribute;
                internal readonly bool IsEncoded;
                internal readonly bool IsOneWay;
                internal readonly bool IsRpc;
                private readonly string keyBase;
                internal readonly OperationDescription Operation;
                private readonly XmlSerializerOperationBehavior.Reflector parent;
                private XmlSerializerOperationBehavior.Reflector.MessageInfo reply;
                internal readonly bool ReplyRequiresSerialization;
                private XmlSerializerOperationBehavior.Reflector.MessageInfo request;
                internal readonly bool RequestRequiresSerialization;
                private SynchronizedCollection<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo> xmlSerializerFaultContractInfos;

                internal OperationReflector(XmlSerializerOperationBehavior.Reflector parent, OperationDescription operation, XmlSerializerFormatAttribute attr, bool reflectOnDemand)
                {
                    OperationFormatter.Validate(operation, attr.Style == OperationFormatStyle.Rpc, attr.IsEncoded);
                    this.parent = parent;
                    this.Operation = operation;
                    this.Attribute = attr;
                    this.IsEncoded = attr.IsEncoded;
                    this.IsRpc = attr.Style == OperationFormatStyle.Rpc;
                    this.IsOneWay = operation.Messages.Count == 1;
                    this.RequestRequiresSerialization = !operation.Messages[0].IsUntypedMessage;
                    this.ReplyRequiresSerialization = !this.IsOneWay && !operation.Messages[1].IsUntypedMessage;
                    MethodInfo operationMethod = operation.OperationMethod;
                    if (operationMethod == null)
                    {
                        this.keyBase = string.Empty;
                        if (operation.DeclaringContract != null)
                        {
                            this.keyBase = operation.DeclaringContract.Name + "," + operation.DeclaringContract.Namespace + ":";
                        }
                        this.keyBase = this.keyBase + operation.Name;
                    }
                    else
                    {
                        this.keyBase = operationMethod.DeclaringType.FullName + ":" + operationMethod.ToString();
                    }
                    foreach (MessageDescription description in operation.Messages)
                    {
                        foreach (MessageHeaderDescription description2 in description.Headers)
                        {
                            this.SetUnknownHeaderInDescription(description2);
                        }
                    }
                    if (!reflectOnDemand)
                    {
                        this.EnsureMessageInfos();
                    }
                }

                private void CreateHeaderDescriptionTable(MessageDescription message, XmlSerializerOperationBehavior.Reflector.MessageInfo info, XmlMembersMapping headersMapping)
                {
                    int num = 0;
                    OperationFormatter.MessageHeaderDescriptionTable headerDescriptionTable = new OperationFormatter.MessageHeaderDescriptionTable();
                    info.SetHeaderDescriptionTable(headerDescriptionTable);
                    foreach (MessageHeaderDescription description in message.Headers)
                    {
                        if (description.IsUnknownHeaderCollection)
                        {
                            info.SetUnknownHeaderDescription(description);
                        }
                        else if (headersMapping != null)
                        {
                            string typeName;
                            string typeNamespace;
                            XmlMemberMapping mapping = headersMapping[num++];
                            if (this.IsEncoded)
                            {
                                typeName = mapping.TypeName;
                                typeNamespace = mapping.TypeNamespace;
                            }
                            else
                            {
                                typeName = mapping.XsdElementName;
                                typeNamespace = mapping.Namespace;
                            }
                            if (typeName != description.Name)
                            {
                                if (message.MessageType != null)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxHeaderNameMismatchInMessageContract", new object[] { message.MessageType, description.MemberInfo.Name, description.Name, typeName })));
                                }
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxHeaderNameMismatchInOperation", new object[] { this.Operation.Name, this.Operation.DeclaringContract.Name, this.Operation.DeclaringContract.Namespace, description.Name, typeName })));
                            }
                            if (typeNamespace != description.Namespace)
                            {
                                if (message.MessageType != null)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxHeaderNamespaceMismatchInMessageContract", new object[] { message.MessageType, description.MemberInfo.Name, description.Namespace, typeNamespace })));
                                }
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxHeaderNamespaceMismatchInOperation", new object[] { this.Operation.Name, this.Operation.DeclaringContract.Name, this.Operation.DeclaringContract.Namespace, description.Namespace, typeNamespace })));
                            }
                            headerDescriptionTable.Add(typeName, typeNamespace, description);
                        }
                    }
                }

                private XmlSerializerOperationBehavior.Reflector.MessageInfo CreateMessageInfo(MessageDescription message, string key)
                {
                    MessagePartDescriptionCollection descriptions;
                    if (message.IsUntypedMessage)
                    {
                        return null;
                    }
                    XmlSerializerOperationBehavior.Reflector.MessageInfo info = new XmlSerializerOperationBehavior.Reflector.MessageInfo();
                    if (message.IsTypedMessage)
                    {
                        key = string.Concat(new object[] { message.MessageType.FullName, ":", this.IsEncoded, ":", this.IsRpc });
                    }
                    XmlMembersMapping mapping = this.LoadHeadersMapping(message, key + ":Headers");
                    info.SetHeaders(this.parent.generation.AddSerializer(mapping));
                    info.SetBody(this.parent.generation.AddSerializer(this.LoadBodyMapping(message, key, out descriptions)), descriptions);
                    this.CreateHeaderDescriptionTable(message, info, mapping);
                    return info;
                }

                internal void EnsureMessageInfos()
                {
                    if (this.request == null)
                    {
                        foreach (System.Type type in this.Operation.KnownTypes)
                        {
                            if (type == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxKnownTypeNull", new object[] { this.Operation.Name })));
                            }
                            this.parent.importer.IncludeType(type, this.IsEncoded);
                        }
                        this.request = this.CreateMessageInfo(this.Operation.Messages[0], ":Request");
                        if (((this.request != null) && this.IsRpc) && (this.Operation.IsValidateRpcWrapperName && (this.request.BodyMapping.XsdElementName != this.Operation.Name)))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxRpcMessageBodyPartNameInvalid", new object[] { this.Operation.Name, this.Operation.Messages[0].MessageName, this.request.BodyMapping.XsdElementName, this.Operation.Name })));
                        }
                        if (!this.IsOneWay)
                        {
                            this.reply = this.CreateMessageInfo(this.Operation.Messages[1], ":Response");
                            System.ServiceModel.Description.XmlName bodyWrapperResponseName = TypeLoader.GetBodyWrapperResponseName(this.Operation.Name);
                            if (((this.reply != null) && this.IsRpc) && (this.Operation.IsValidateRpcWrapperName && (this.reply.BodyMapping.XsdElementName != bodyWrapperResponseName.EncodedName)))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxRpcMessageBodyPartNameInvalid", new object[] { this.Operation.Name, this.Operation.Messages[1].MessageName, this.reply.BodyMapping.XsdElementName, bodyWrapperResponseName.EncodedName })));
                            }
                        }
                        if (this.Attribute.SupportFaults)
                        {
                            this.GenerateXmlSerializerFaultContractInfos();
                        }
                    }
                }

                private void GenerateXmlSerializerFaultContractInfos()
                {
                    SynchronizedCollection<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo> synchronizeds = new SynchronizedCollection<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo>();
                    for (int i = 0; i < this.Operation.Faults.Count; i++)
                    {
                        XmlQualifiedName name;
                        FaultDescription fault = this.Operation.Faults[i];
                        FaultContractInfo faultContractInfo = new FaultContractInfo(fault.Action, fault.DetailType, fault.ElementName, fault.Namespace, this.Operation.KnownTypes);
                        XmlMembersMapping mapping = this.ImportFaultElement(fault, out name);
                        XmlSerializerOperationBehavior.Reflector.SerializerStub serializerStub = this.parent.generation.AddSerializer(mapping);
                        synchronizeds.Add(new XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo(faultContractInfo, serializerStub, name));
                    }
                    this.xmlSerializerFaultContractInfos = synchronizeds;
                }

                private MessagePartDescriptionCollection GetWrappedParts(MessagePartDescription bodyPart)
                {
                    System.Type type = bodyPart.Type;
                    MessagePartDescriptionCollection descriptions = new MessagePartDescriptionCollection();
                    foreach (MemberInfo info in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (((info.MemberType & (MemberTypes.Property | MemberTypes.Field)) != 0) && !info.IsDefined(typeof(SoapIgnoreAttribute), false))
                        {
                            MessagePartDescription description;
                            System.ServiceModel.Description.XmlName name = new System.ServiceModel.Description.XmlName(info.Name);
                            description = new MessagePartDescription(name.EncodedName, string.Empty) {
                                AdditionalAttributesProvider = description.MemberInfo = info,
                                Index = description.SerializationPosition = descriptions.Count,
                                Type = (info.MemberType == MemberTypes.Property) ? ((PropertyInfo) info).PropertyType : ((FieldInfo) info).FieldType
                            };
                            if (bodyPart.HasProtectionLevel)
                            {
                                description.ProtectionLevel = bodyPart.ProtectionLevel;
                            }
                            descriptions.Add(description);
                        }
                    }
                    return descriptions;
                }

                private MessagePartDescription GetWrapperPart(MessageDescription message)
                {
                    if (message.Body.Parts.Count != 1)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxRpcMessageMustHaveASingleBody", new object[] { this.Operation.Name, message.MessageName })));
                    }
                    MessagePartDescription description = message.Body.Parts[0];
                    System.Type c = description.Type;
                    if ((c.BaseType != null) && (c.BaseType != typeof(object)))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxBodyObjectTypeCannotBeInherited", new object[] { c.FullName })));
                    }
                    if (typeof(IEnumerable).IsAssignableFrom(c))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxBodyObjectTypeCannotBeInterface", new object[] { c.FullName, typeof(IEnumerable).FullName })));
                    }
                    if (typeof(IXmlSerializable).IsAssignableFrom(c))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxBodyObjectTypeCannotBeInterface", new object[] { c.FullName, typeof(IXmlSerializable).FullName })));
                    }
                    return description;
                }

                internal XmlMembersMapping ImportFaultElement(FaultDescription fault, out XmlQualifiedName elementName)
                {
                    XmlReflectionMember[] members = new XmlReflectionMember[1];
                    System.ServiceModel.Description.XmlName name = fault.ElementName;
                    string ns = fault.Namespace;
                    if (name == null)
                    {
                        XmlTypeMapping mapping = this.parent.importer.ImportTypeMapping(fault.DetailType, this.IsEncoded);
                        name = new System.ServiceModel.Description.XmlName(mapping.ElementName, this.IsEncoded);
                        ns = mapping.Namespace;
                        if (name == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxFaultTypeAnonymous", new object[] { this.Operation.Name, fault.DetailType.FullName })));
                        }
                    }
                    elementName = new XmlQualifiedName(name.DecodedName, ns);
                    members[0] = XmlSerializerHelper.GetXmlReflectionMember(null, name, ns, fault.DetailType, null, false, this.IsEncoded, false);
                    string mappingKey = "fault:" + name.DecodedName + ":" + ns;
                    return this.ImportMembersMapping(name.EncodedName, ns, members, false, this.IsRpc, mappingKey);
                }

                internal XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool rpc, string mappingKey)
                {
                    string str = mappingKey.StartsWith(":", StringComparison.Ordinal) ? (this.keyBase + mappingKey) : mappingKey;
                    return this.parent.importer.ImportMembersMapping(new System.ServiceModel.Description.XmlName(elementName, true), ns, members, hasWrapperElement, rpc, this.IsEncoded, str);
                }

                private XmlMembersMapping LoadBodyMapping(MessageDescription message, string mappingKey, out MessagePartDescriptionCollection rpcEncodedTypedMessageBodyParts)
                {
                    MessagePartDescription description;
                    string name;
                    string wrapperNamespace;
                    MessagePartDescriptionCollection parts;
                    if ((this.IsEncoded && message.IsTypedMessage) && (message.Body.WrapperName == null))
                    {
                        MessagePartDescription wrapperPart = this.GetWrapperPart(message);
                        description = null;
                        rpcEncodedTypedMessageBodyParts = parts = this.GetWrappedParts(wrapperPart);
                        name = wrapperPart.Name;
                        wrapperNamespace = wrapperPart.Namespace;
                    }
                    else
                    {
                        rpcEncodedTypedMessageBodyParts = null;
                        description = OperationFormatter.IsValidReturnValue(message.Body.ReturnValue) ? message.Body.ReturnValue : null;
                        parts = message.Body.Parts;
                        name = message.Body.WrapperName;
                        wrapperNamespace = message.Body.WrapperNamespace;
                    }
                    bool isWrapped = name != null;
                    bool flag2 = description != null;
                    int num = parts.Count + (flag2 ? 1 : 0);
                    if ((num == 0) && !isWrapped)
                    {
                        return null;
                    }
                    XmlReflectionMember[] members = new XmlReflectionMember[num];
                    int num2 = 0;
                    if (flag2)
                    {
                        members[num2++] = XmlSerializerHelper.GetXmlReflectionMember(description, this.IsRpc, this.IsEncoded, isWrapped);
                    }
                    for (int i = 0; i < parts.Count; i++)
                    {
                        members[num2++] = XmlSerializerHelper.GetXmlReflectionMember(parts[i], this.IsRpc, this.IsEncoded, isWrapped);
                    }
                    if (!isWrapped)
                    {
                        wrapperNamespace = this.ContractNamespace;
                    }
                    return this.ImportMembersMapping(name, wrapperNamespace, members, isWrapped, this.IsRpc, mappingKey);
                }

                private XmlMembersMapping LoadHeadersMapping(MessageDescription message, string mappingKey)
                {
                    int count = message.Headers.Count;
                    if (count == 0)
                    {
                        return null;
                    }
                    if (this.IsEncoded)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxHeadersAreNotSupportedInEncoded", new object[] { message.MessageName })));
                    }
                    int num2 = 0;
                    int num3 = 0;
                    XmlReflectionMember[] sourceArray = new XmlReflectionMember[count];
                    for (int i = 0; i < count; i++)
                    {
                        MessageHeaderDescription part = message.Headers[i];
                        if (!part.IsUnknownHeaderCollection)
                        {
                            sourceArray[num3++] = XmlSerializerHelper.GetXmlReflectionMember(part, false, this.IsEncoded, false);
                        }
                        else
                        {
                            num2++;
                        }
                    }
                    if (num2 == count)
                    {
                        return null;
                    }
                    if (num2 > 0)
                    {
                        XmlReflectionMember[] destinationArray = new XmlReflectionMember[count - num2];
                        Array.Copy(sourceArray, destinationArray, destinationArray.Length);
                        sourceArray = destinationArray;
                    }
                    return this.ImportMembersMapping(this.ContractName, this.ContractNamespace, sourceArray, false, false, mappingKey);
                }

                private void SetUnknownHeaderInDescription(MessageHeaderDescription header)
                {
                    if (!this.IsEncoded && (header.AdditionalAttributesProvider != null))
                    {
                        XmlAttributes attributes = new XmlAttributes(header.AdditionalAttributesProvider);
                        foreach (XmlAnyElementAttribute attribute in attributes.XmlAnyElements)
                        {
                            if (string.IsNullOrEmpty(attribute.Name))
                            {
                                header.IsUnknownHeaderCollection = true;
                            }
                        }
                    }
                }

                private string ContractName
                {
                    get
                    {
                        return this.Operation.DeclaringContract.Name;
                    }
                }

                private string ContractNamespace
                {
                    get
                    {
                        return this.Operation.DeclaringContract.Namespace;
                    }
                }

                internal XmlSerializerOperationBehavior.Reflector.MessageInfo Reply
                {
                    get
                    {
                        this.parent.EnsureMessageInfos();
                        return this.reply;
                    }
                }

                internal XmlSerializerOperationBehavior.Reflector.MessageInfo Request
                {
                    get
                    {
                        this.parent.EnsureMessageInfos();
                        return this.request;
                    }
                }

                internal SynchronizedCollection<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo> XmlSerializerFaultContractInfos
                {
                    get
                    {
                        this.parent.EnsureMessageInfos();
                        return this.xmlSerializerFaultContractInfos;
                    }
                }
            }

            internal class SerializerGenerationContext
            {
                private List<XmlMembersMapping> Mappings = new List<XmlMembersMapping>();
                private XmlSerializer[] serializers;
                private object thisLock = new object();
                private System.Type type;

                internal SerializerGenerationContext(System.Type type)
                {
                    this.type = type;
                }

                internal XmlSerializerOperationBehavior.Reflector.SerializerStub AddSerializer(XmlMembersMapping mapping)
                {
                    int handle = -1;
                    if (mapping != null)
                    {
                        handle = this.Mappings.Add(mapping);
                    }
                    return new XmlSerializerOperationBehavior.Reflector.SerializerStub(this, mapping, handle);
                }

                [SecuritySafeCritical]
                private XmlSerializer[] CreateSerializersFromMappings(XmlMapping[] mappings, System.Type type)
                {
                    return XmlSerializer.FromMappings(mappings, type);
                }

                private XmlSerializer[] GenerateSerializers()
                {
                    List<XmlMembersMapping> list = new List<XmlMembersMapping>();
                    int[] numArray = new int[this.Mappings.Count];
                    for (int i = 0; i < this.Mappings.Count; i++)
                    {
                        XmlMembersMapping item = this.Mappings[i];
                        int index = list.IndexOf(item);
                        if (index < 0)
                        {
                            list.Add(item);
                            index = list.Count - 1;
                        }
                        numArray[i] = index;
                    }
                    XmlSerializer[] serializerArray = this.CreateSerializersFromMappings(list.ToArray(), this.type);
                    if (list.Count == this.Mappings.Count)
                    {
                        return serializerArray;
                    }
                    XmlSerializer[] serializerArray2 = new XmlSerializer[this.Mappings.Count];
                    for (int j = 0; j < this.Mappings.Count; j++)
                    {
                        serializerArray2[j] = serializerArray[numArray[j]];
                    }
                    return serializerArray2;
                }

                internal XmlSerializer GetSerializer(int handle)
                {
                    if (handle < 0)
                    {
                        return null;
                    }
                    if (this.serializers == null)
                    {
                        lock (this.thisLock)
                        {
                            if (this.serializers == null)
                            {
                                this.serializers = this.GenerateSerializers();
                            }
                        }
                    }
                    return this.serializers[handle];
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct SerializerStub
            {
                private readonly XmlSerializerOperationBehavior.Reflector.SerializerGenerationContext context;
                internal readonly XmlMembersMapping Mapping;
                internal readonly int Handle;
                internal SerializerStub(XmlSerializerOperationBehavior.Reflector.SerializerGenerationContext context, XmlMembersMapping mapping, int handle)
                {
                    this.context = context;
                    this.Mapping = mapping;
                    this.Handle = handle;
                }

                internal XmlSerializer GetSerializer()
                {
                    return this.context.GetSerializer(this.Handle);
                }
            }

            internal class XmlSerializerFaultContractInfo
            {
                private XmlQualifiedName faultContractElementName;
                private System.ServiceModel.Dispatcher.FaultContractInfo faultContractInfo;
                private XmlSerializerObjectSerializer serializer;
                private XmlSerializerOperationBehavior.Reflector.SerializerStub serializerStub;

                internal XmlSerializerFaultContractInfo(System.ServiceModel.Dispatcher.FaultContractInfo faultContractInfo, XmlSerializerOperationBehavior.Reflector.SerializerStub serializerStub, XmlQualifiedName faultContractElementName)
                {
                    if (faultContractInfo == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("faultContractInfo");
                    }
                    if (faultContractElementName == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("faultContractElementName");
                    }
                    this.faultContractInfo = faultContractInfo;
                    this.serializerStub = serializerStub;
                    this.faultContractElementName = faultContractElementName;
                }

                internal XmlQualifiedName FaultContractElementName
                {
                    get
                    {
                        return this.faultContractElementName;
                    }
                }

                internal System.ServiceModel.Dispatcher.FaultContractInfo FaultContractInfo
                {
                    get
                    {
                        return this.faultContractInfo;
                    }
                }

                internal XmlSerializerObjectSerializer Serializer
                {
                    get
                    {
                        if (this.serializer == null)
                        {
                            this.serializer = new XmlSerializerObjectSerializer(this.faultContractInfo.Detail, this.faultContractElementName, this.serializerStub.GetSerializer());
                        }
                        return this.serializer;
                    }
                }
            }

            private class XmlSerializerImporter
            {
                private readonly string defaultNs;
                private SoapReflectionImporter soapImporter;
                private XmlReflectionImporter xmlImporter;
                private Dictionary<string, XmlMembersMapping> xmlMappings;

                internal XmlSerializerImporter(string defaultNs)
                {
                    this.defaultNs = defaultNs;
                    this.xmlImporter = null;
                    this.soapImporter = null;
                }

                internal XmlMembersMapping ImportMembersMapping(System.ServiceModel.Description.XmlName elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool rpc, bool isEncoded, string mappingKey)
                {
                    XmlMembersMapping mapping;
                    string decodedName = elementName.DecodedName;
                    if (!this.XmlMappings.TryGetValue(mappingKey, out mapping))
                    {
                        if (isEncoded)
                        {
                            mapping = this.SoapImporter.ImportMembersMapping(decodedName, ns, members, hasWrapperElement, rpc);
                        }
                        else
                        {
                            mapping = this.XmlImporter.ImportMembersMapping(decodedName, ns, members, hasWrapperElement, rpc);
                        }
                        mapping.SetKey(mappingKey);
                        this.XmlMappings.Add(mappingKey, mapping);
                    }
                    return mapping;
                }

                internal XmlTypeMapping ImportTypeMapping(System.Type type, bool isEncoded)
                {
                    if (isEncoded)
                    {
                        return this.SoapImporter.ImportTypeMapping(type);
                    }
                    return this.XmlImporter.ImportTypeMapping(type);
                }

                internal void IncludeType(System.Type knownType, bool isEncoded)
                {
                    if (isEncoded)
                    {
                        this.SoapImporter.IncludeType(knownType);
                    }
                    else
                    {
                        this.XmlImporter.IncludeType(knownType);
                    }
                }

                private SoapReflectionImporter SoapImporter
                {
                    get
                    {
                        if (this.soapImporter == null)
                        {
                            this.soapImporter = new SoapReflectionImporter(NamingHelper.CombineUriStrings(this.defaultNs, "encoded"));
                        }
                        return this.soapImporter;
                    }
                }

                private XmlReflectionImporter XmlImporter
                {
                    get
                    {
                        if (this.xmlImporter == null)
                        {
                            this.xmlImporter = new XmlReflectionImporter(this.defaultNs);
                        }
                        return this.xmlImporter;
                    }
                }

                private Dictionary<string, XmlMembersMapping> XmlMappings
                {
                    get
                    {
                        if (this.xmlMappings == null)
                        {
                            this.xmlMappings = new Dictionary<string, XmlMembersMapping>();
                        }
                        return this.xmlMappings;
                    }
                }
            }
        }
    }
}

