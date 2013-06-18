namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;

    internal abstract class MessageContractExporter
    {
        protected readonly WsdlContractConversionContext contractContext;
        private static readonly XmlSchemaSequence emptySequence = new XmlSchemaSequence();
        protected readonly WsdlExporter exporter;
        protected readonly IOperationBehavior extension;
        protected readonly OperationDescription operation;

        protected MessageContractExporter(WsdlExporter exporter, WsdlContractConversionContext context, OperationDescription operation, IOperationBehavior extension)
        {
            this.exporter = exporter;
            this.contractContext = context;
            this.operation = operation;
            this.extension = extension;
        }

        private void AddElementToSchema(XmlSchemaElement element, string elementNs, XmlSchemaSet schemaSet)
        {
            OperationDescription operation = this.operation;
            if (operation.OperationMethod != null)
            {
                OperationElement element2;
                XmlQualifiedName key = new XmlQualifiedName(element.Name, elementNs);
                if (this.ExportedMessages.ElementTypes.TryGetValue(key, out element2))
                {
                    if ((element2.Operation.OperationMethod != operation.OperationMethod) && !System.ServiceModel.Description.SchemaHelper.IsMatch(element, element2.Element))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotHaveTwoOperationsWithTheSameElement5", new object[] { operation.OperationMethod.DeclaringType, operation.OperationMethod.Name, key, element2.Operation.OperationMethod.DeclaringType, element2.Operation.Name })));
                    }
                    return;
                }
                this.ExportedMessages.ElementTypes.Add(key, new OperationElement(element, operation));
            }
            System.ServiceModel.Description.SchemaHelper.AddElementToSchema(element, System.ServiceModel.Description.SchemaHelper.GetSchema(elementNs, schemaSet), schemaSet);
        }

        protected static MessagePart AddMessagePart(Message message, string partName, XmlQualifiedName elementName, XmlQualifiedName typeName)
        {
            if (message.Parts[partName] != null)
            {
                if (IsNullOrEmpty(elementName))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SFxPartNameMustBeUniqueInRpc", new object[] { partName })));
                }
                int num = 1;
                while (message.Parts[partName + num] != null)
                {
                    if (num == 0x7fffffff)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SFxTooManyPartsWithSameName", new object[] { partName })));
                    }
                    num++;
                }
                partName = partName + num.ToString(CultureInfo.InvariantCulture);
            }
            MessagePart messagePart = new MessagePart {
                Name = partName,
                Element = elementName,
                Type = typeName
            };
            message.Parts.Add(messagePart);
            EnsureXsdImport(IsNullOrEmpty(elementName) ? typeName.Namespace : elementName.Namespace, message.ServiceDescription);
            return messagePart;
        }

        protected void AddParameterOrder(MessageDescription message)
        {
            if (this.operation != null)
            {
                Operation operation = this.contractContext.GetOperation(this.operation);
                if (operation != null)
                {
                    if (operation.ParameterOrder == null)
                    {
                        operation.ParameterOrder = new string[this.GetParameterCount()];
                    }
                    if (operation.ParameterOrder.Length != 0)
                    {
                        foreach (MessagePartDescription description in message.Body.Parts)
                        {
                            ParameterInfo additionalAttributesProvider = description.AdditionalAttributesProvider as ParameterInfo;
                            if ((additionalAttributesProvider != null) && (additionalAttributesProvider.Position >= 0))
                            {
                                operation.ParameterOrder[additionalAttributesProvider.Position] = description.Name;
                            }
                        }
                    }
                }
            }
        }

        protected virtual void Compile()
        {
            foreach (System.Xml.Schema.XmlSchema schema in this.SchemaSet.Schemas())
            {
                this.SchemaSet.Reprocess(schema);
            }
            System.ServiceModel.Description.SchemaHelper.Compile(this.SchemaSet, this.exporter.Errors);
        }

        protected bool CreateHeaderMessage(MessageDescription message, out Message wsdlMessage)
        {
            wsdlMessage = null;
            if (this.ExportedMessages.WsdlHeaderMessages.ContainsKey(new MessageDescriptionDictionaryKey(this.contractContext.Contract, message)))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MultipleCallsToExportContractWithSameContract")));
            }
            TypedMessageKey key = null;
            if (message.IsTypedMessage)
            {
                key = new TypedMessageKey(message.MessageType, this.operation.DeclaringContract.Namespace, this.GetExtensionData());
                if (this.ExportedMessages.TypedHeaderMessages.TryGetValue(key, out wsdlMessage))
                {
                    this.ExportedMessages.WsdlHeaderMessages.Add(new MessageDescriptionDictionaryKey(this.contractContext.Contract, message), wsdlMessage);
                    return false;
                }
            }
            string headerMessageName = this.GetHeaderMessageName(message);
            wsdlMessage = new Message();
            wsdlMessage.Name = headerMessageName;
            this.contractContext.WsdlPortType.ServiceDescription.Messages.Add(wsdlMessage);
            if (message.IsTypedMessage)
            {
                this.ExportedMessages.TypedHeaderMessages.Add(key, wsdlMessage);
            }
            this.ExportedMessages.WsdlHeaderMessages.Add(new MessageDescriptionDictionaryKey(this.contractContext.Contract, message), wsdlMessage);
            return true;
        }

        protected bool CreateMessage(MessageDescription message, int messageIndex, out Message wsdlMessage)
        {
            wsdlMessage = null;
            bool flag = true;
            if (this.ExportedMessages.WsdlMessages.ContainsKey(new MessageDescriptionDictionaryKey(this.contractContext.Contract, message)))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MultipleCallsToExportContractWithSameContract")));
            }
            TypedMessageKey key = null;
            OperationMessageKey key2 = null;
            if (message.IsTypedMessage)
            {
                key = new TypedMessageKey(message.MessageType, this.operation.DeclaringContract.Namespace, this.GetExtensionData());
                if (this.ExportedMessages.TypedMessages.TryGetValue(key, out wsdlMessage))
                {
                    flag = false;
                }
            }
            else if (this.operation.OperationMethod != null)
            {
                key2 = new OperationMessageKey(this.operation, messageIndex);
                if (this.ExportedMessages.ParameterMessages.TryGetValue(key2, out wsdlMessage))
                {
                    flag = false;
                }
            }
            System.Web.Services.Description.ServiceDescription serviceDescription = this.contractContext.WsdlPortType.ServiceDescription;
            if (flag)
            {
                wsdlMessage = new Message();
                wsdlMessage.Name = this.GetMessageName(message);
                serviceDescription.Messages.Add(wsdlMessage);
                if (message.IsTypedMessage)
                {
                    this.ExportedMessages.TypedMessages.Add(key, wsdlMessage);
                }
                else if (key2 != null)
                {
                    this.ExportedMessages.ParameterMessages.Add(key2, wsdlMessage);
                }
            }
            this.contractContext.GetOperationMessage(message).Message = new XmlQualifiedName(wsdlMessage.Name, wsdlMessage.ServiceDescription.TargetNamespace);
            this.ExportedMessages.WsdlMessages.Add(new MessageDescriptionDictionaryKey(this.contractContext.Contract, message), wsdlMessage);
            return flag;
        }

        private static bool DoesMessageNameExist(string messageName, object wsdlObject)
        {
            return (((System.Web.Services.Description.ServiceDescription) wsdlObject).Messages[messageName] != null);
        }

        private static void EnsureXsdImport(string ns, System.Web.Services.Description.ServiceDescription wsdl)
        {
            string targetNamespace = wsdl.TargetNamespace;
            if (!targetNamespace.EndsWith("/", StringComparison.Ordinal))
            {
                targetNamespace = targetNamespace + "/Imports";
            }
            else
            {
                targetNamespace = targetNamespace + "Imports";
            }
            if (targetNamespace == ns)
            {
                targetNamespace = wsdl.TargetNamespace;
            }
            System.Xml.Schema.XmlSchema containedSchema = GetContainedSchema(wsdl, targetNamespace);
            if (containedSchema != null)
            {
                foreach (XmlSchemaImport import in containedSchema.Includes)
                {
                    if ((import != null) && System.ServiceModel.Description.SchemaHelper.NamespacesEqual(import.Namespace, ns))
                    {
                        return;
                    }
                }
            }
            else
            {
                containedSchema = new System.Xml.Schema.XmlSchema {
                    TargetNamespace = targetNamespace
                };
                wsdl.Types.Schemas.Add(containedSchema);
            }
            XmlSchemaImport item = new XmlSchemaImport();
            if ((ns != null) && (ns.Length > 0))
            {
                item.Namespace = ns;
            }
            containedSchema.Includes.Add(item);
        }

        private void ExportAnyMessage(Message message, MessagePartDescription part)
        {
            XmlSchemaSet generatedXmlSchemas = this.exporter.GeneratedXmlSchemas;
            System.Xml.Schema.XmlSchema schema = System.ServiceModel.Description.SchemaHelper.GetSchema(DataContractSerializerMessageContractImporter.GenericMessageTypeName.Namespace, generatedXmlSchemas);
            if (!schema.SchemaTypes.Contains(DataContractSerializerMessageContractImporter.GenericMessageTypeName))
            {
                XmlSchemaComplexType type = new XmlSchemaComplexType {
                    Name = DataContractSerializerMessageContractImporter.GenericMessageTypeName.Name
                };
                XmlSchemaSequence sequence = new XmlSchemaSequence();
                type.Particle = sequence;
                XmlSchemaAny item = new XmlSchemaAny {
                    MinOccurs = 0M,
                    MaxOccurs = 79228162514264337593543950335M,
                    Namespace = "##any"
                };
                sequence.Items.Add(item);
                System.ServiceModel.Description.SchemaHelper.AddTypeToSchema(type, schema, generatedXmlSchemas);
            }
            string partName = string.IsNullOrEmpty(part.UniquePartName) ? part.Name : part.UniquePartName;
            MessagePart part2 = AddMessagePart(message, partName, XmlQualifiedName.Empty, DataContractSerializerMessageContractImporter.GenericMessageTypeName);
            part.UniquePartName = part2.Name;
        }

        protected abstract void ExportBody(int messageIndex, object state);
        private void ExportFault(FaultDescription fault)
        {
            Message message = new Message {
                Name = this.GetFaultMessageName(fault.Name)
            };
            XmlQualifiedName elementName = this.ExportFaultElement(fault);
            this.contractContext.WsdlPortType.ServiceDescription.Messages.Add(message);
            AddMessagePart(message, "detail", elementName, null);
            OperationFault operationFault = this.contractContext.GetOperationFault(fault);
            WsdlExporter.WSAddressingHelper.AddActionAttribute(fault.Action, operationFault, this.exporter.PolicyVersion);
            operationFault.Message = new XmlQualifiedName(message.Name, message.ServiceDescription.TargetNamespace);
        }

        private XmlQualifiedName ExportFaultElement(FaultDescription fault)
        {
            XmlSchemaType type;
            XmlQualifiedName rootElementName;
            XmlQualifiedName typeName = this.ExportType(fault.DetailType, fault.Name, this.operation.Name, out type);
            if (System.ServiceModel.Description.XmlName.IsNullOrEmpty(fault.ElementName))
            {
                rootElementName = this.DataContractExporter.GetRootElementName(fault.DetailType);
                if (rootElementName == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxFaultTypeAnonymous", new object[] { this.operation.Name, fault.DetailType.FullName })));
                }
            }
            else
            {
                rootElementName = new XmlQualifiedName(fault.ElementName.EncodedName, fault.Namespace);
            }
            this.ExportGlobalElement(rootElementName.Name, rootElementName.Namespace, true, typeName, type, this.exporter.GeneratedXmlSchemas);
            return rootElementName;
        }

        protected virtual void ExportFaults(object state)
        {
            foreach (FaultDescription description in this.operation.Faults)
            {
                this.ExportFault(description);
            }
        }

        protected void ExportGlobalElement(string elementName, string elementNs, bool isNillable, XmlQualifiedName typeName, XmlSchemaType xsdType, XmlSchemaSet schemaSet)
        {
            XmlSchemaElement element = new XmlSchemaElement {
                Name = elementName
            };
            if (xsdType != null)
            {
                element.SchemaType = xsdType;
            }
            else
            {
                element.SchemaTypeName = typeName;
            }
            element.IsNillable = isNillable;
            this.AddElementToSchema(element, elementNs, schemaSet);
        }

        protected abstract void ExportHeaders(int messageIndex, object state);
        protected abstract void ExportKnownTypes();
        private void ExportLocalElement(string wrapperNs, string elementName, string elementNs, XmlQualifiedName typeName, XmlSchemaType xsdType, bool multiple, bool isOptional, bool isNillable, XmlSchemaSequence sequence, XmlSchemaSet schemaSet)
        {
            System.Xml.Schema.XmlSchema schema = System.ServiceModel.Description.SchemaHelper.GetSchema(wrapperNs, schemaSet);
            XmlSchemaElement element = new XmlSchemaElement();
            if (elementNs == wrapperNs)
            {
                element.Name = elementName;
                if (xsdType != null)
                {
                    element.SchemaType = xsdType;
                }
                else
                {
                    element.SchemaTypeName = typeName;
                    System.ServiceModel.Description.SchemaHelper.AddImportToSchema(element.SchemaTypeName.Namespace, schema);
                }
                System.ServiceModel.Description.SchemaHelper.AddElementForm(element, schema);
                element.IsNillable = isNillable;
            }
            else
            {
                element.RefName = new XmlQualifiedName(elementName, elementNs);
                System.ServiceModel.Description.SchemaHelper.AddImportToSchema(elementNs, schema);
                this.ExportGlobalElement(elementName, elementNs, isNillable, typeName, xsdType, schemaSet);
            }
            if (multiple)
            {
                element.MaxOccurs = 79228162514264337593543950335M;
            }
            if (isOptional)
            {
                element.MinOccurs = 0M;
            }
            sequence.Items.Add(element);
        }

        private void ExportMessage(int messageIndex, object state)
        {
            try
            {
                Message message;
                MessageDescription description = this.operation.Messages[messageIndex];
                if (this.CreateMessage(description, messageIndex, out message))
                {
                    if (description.IsUntypedMessage)
                    {
                        this.ExportAnyMessage(message, description.Body.ReturnValue ?? description.Body.Parts[0]);
                        return;
                    }
                    bool isRequest = messageIndex == 0;
                    StreamFormatter formatter = StreamFormatter.Create(description, this.operation.Name, isRequest);
                    if (formatter != null)
                    {
                        this.ExportStreamBody(message, formatter.WrapperName, formatter.WrapperNamespace, formatter.PartName, formatter.PartNamespace, this.IsRpcStyle(), false);
                    }
                    else
                    {
                        this.ExportBody(messageIndex, state);
                    }
                }
                if (!description.IsUntypedMessage)
                {
                    this.ExportHeaders(messageIndex, state);
                }
            }
            finally
            {
                this.Compile();
            }
        }

        internal static void ExportMessageBinding(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext, Type messageContractExporterType, OperationDescription operation)
        {
            new MessageBindingExporter(exporter, endpointContext).ExportMessageBinding(operation, messageContractExporterType);
        }

        internal void ExportMessageContract()
        {
            if (this.extension != null)
            {
                object state = this.OnExportMessageContract();
                OperationFormatter.Validate(this.operation, this.IsRpcStyle(), this.IsEncoded());
                this.ExportKnownTypes();
                for (int i = 0; i < this.operation.Messages.Count; i++)
                {
                    this.ExportMessage(i, state);
                }
                if (!this.operation.IsOneWay)
                {
                    this.ExportFaults(state);
                }
                foreach (System.Xml.Schema.XmlSchema schema in this.exporter.GeneratedXmlSchemas.Schemas())
                {
                    EnsureXsdImport(schema.TargetNamespace, this.contractContext.WsdlPortType.ServiceDescription);
                }
            }
        }

        protected void ExportMessagePart(Message message, MessagePartDescription part, XmlQualifiedName typeName, XmlSchemaType xsdType, bool isOptional, bool isNillable, bool skipSchemaExport, bool generateElement, string wrapperNs, XmlSchemaSequence wrapperSequence, XmlSchemaSet schemaSet)
        {
            if (!IsNullOrEmpty(typeName) || (xsdType != null))
            {
                string name = part.Name;
                string elementName = string.IsNullOrEmpty(part.UniquePartName) ? name : part.UniquePartName;
                MessagePart part2 = null;
                if (generateElement)
                {
                    if (wrapperSequence != null)
                    {
                        if (!skipSchemaExport)
                        {
                            this.ExportLocalElement(wrapperNs, elementName, part.Namespace, typeName, xsdType, part.Multiple, isOptional, isNillable, wrapperSequence, schemaSet);
                        }
                    }
                    else
                    {
                        if (!skipSchemaExport)
                        {
                            this.ExportGlobalElement(name, part.Namespace, isNillable, typeName, xsdType, schemaSet);
                        }
                        part2 = AddMessagePart(message, elementName, new XmlQualifiedName(name, part.Namespace), XmlQualifiedName.Empty);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(typeName.Name))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxAnonymousTypeNotSupported", new object[] { message.Name, elementName })));
                    }
                    part2 = AddMessagePart(message, elementName, XmlQualifiedName.Empty, typeName);
                }
                if (part2 != null)
                {
                    part.UniquePartName = part2.Name;
                }
            }
        }

        protected void ExportStreamBody(Message message, string wrapperName, string wrapperNs, string partName, string partNs, bool isRpc, bool skipSchemaExport)
        {
            XmlSchemaSet generatedXmlSchemas = this.exporter.GeneratedXmlSchemas;
            System.Xml.Schema.XmlSchema schema = System.ServiceModel.Description.SchemaHelper.GetSchema(DataContractSerializerMessageContractImporter.StreamBodyTypeName.Namespace, generatedXmlSchemas);
            if (!schema.SchemaTypes.Contains(DataContractSerializerMessageContractImporter.StreamBodyTypeName))
            {
                XmlSchemaSimpleType type = new XmlSchemaSimpleType {
                    Name = DataContractSerializerMessageContractImporter.StreamBodyTypeName.Name
                };
                XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction {
                    BaseTypeName = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Base64Binary).QualifiedName
                };
                type.Content = restriction;
                System.ServiceModel.Description.SchemaHelper.AddTypeToSchema(type, schema, generatedXmlSchemas);
            }
            XmlSchemaSequence wrapperSequence = null;
            if (!isRpc && (wrapperName != null))
            {
                wrapperSequence = this.ExportWrappedPart(message, wrapperName, wrapperNs, generatedXmlSchemas, skipSchemaExport);
            }
            MessagePartDescription part = new MessagePartDescription(partName, partNs);
            this.ExportMessagePart(message, part, DataContractSerializerMessageContractImporter.StreamBodyTypeName, null, false, false, skipSchemaExport, !isRpc, wrapperNs, wrapperSequence, generatedXmlSchemas);
        }

        protected XmlQualifiedName ExportType(Type type, string partName, string operationName, out XmlSchemaType xsdType)
        {
            xsdType = null;
            if (type == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxExportMustHaveType", new object[] { operationName, partName })));
            }
            if (type == typeof(void))
            {
                return null;
            }
            this.DataContractExporter.Export(type);
            XmlQualifiedName schemaTypeName = this.DataContractExporter.GetSchemaTypeName(type);
            if (IsNullOrEmpty(schemaTypeName))
            {
                xsdType = this.DataContractExporter.GetSchemaType(type);
            }
            return schemaTypeName;
        }

        protected XmlSchemaSequence ExportWrappedPart(Message message, string elementName, string elementNs, XmlSchemaSet schemaSet, bool skipSchemaExport)
        {
            AddMessagePart(message, "parameters", new XmlQualifiedName(elementName, elementNs), XmlQualifiedName.Empty);
            if (skipSchemaExport)
            {
                return emptySequence;
            }
            XmlSchemaElement element = new XmlSchemaElement {
                Name = elementName
            };
            XmlSchemaComplexType type = new XmlSchemaComplexType();
            element.SchemaType = type;
            XmlSchemaSequence sequence = new XmlSchemaSequence();
            type.Particle = sequence;
            this.AddElementToSchema(element, elementNs, schemaSet);
            return sequence;
        }

        private static System.Xml.Schema.XmlSchema GetContainedSchema(System.Web.Services.Description.ServiceDescription wsdl, string ns)
        {
            foreach (System.Xml.Schema.XmlSchema schema in wsdl.Types.Schemas)
            {
                if (System.ServiceModel.Description.SchemaHelper.NamespacesEqual(schema.TargetNamespace, ns))
                {
                    return schema;
                }
            }
            return null;
        }

        protected abstract object GetExtensionData();
        protected string GetFaultMessageName(string faultName)
        {
            string name = this.contractContext.WsdlPortType.Name;
            string str2 = this.contractContext.GetOperation(this.operation).Name;
            string messageNameBase = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_FaultMessage", new object[] { name, str2, faultName });
            System.Web.Services.Description.ServiceDescription serviceDescription = this.contractContext.WsdlPortType.ServiceDescription;
            return this.GetUniqueMessageName(serviceDescription, messageNameBase);
        }

        private string GetHeaderMessageName(MessageDescription messageDescription)
        {
            Message message = this.ExportedMessages.WsdlMessages[new MessageDescriptionDictionaryKey(this.contractContext.Contract, messageDescription)];
            string messageNameBase = string.Format(CultureInfo.InvariantCulture, "{0}_Headers", new object[] { message.Name });
            System.Web.Services.Description.ServiceDescription serviceDescription = this.contractContext.WsdlPortType.ServiceDescription;
            return this.GetUniqueMessageName(serviceDescription, messageNameBase);
        }

        private static MessageExportContext GetMessageExportContext(WsdlExporter exporter)
        {
            object obj2;
            if (!exporter.State.TryGetValue(typeof(MessageExportContext), out obj2))
            {
                obj2 = new MessageExportContext();
                exporter.State[typeof(MessageExportContext)] = obj2;
            }
            return (MessageExportContext) obj2;
        }

        private string GetMessageName(MessageDescription messageDescription)
        {
            string str = System.ServiceModel.Description.XmlName.IsNullOrEmpty(messageDescription.MessageName) ? null : messageDescription.MessageName.EncodedName;
            if (string.IsNullOrEmpty(str))
            {
                string name = this.contractContext.WsdlPortType.Name;
                string str3 = this.contractContext.GetOperation(this.operation).Name;
                string str4 = this.operation.IsServerInitiated() ? "Callback" : string.Empty;
                if (messageDescription.Direction == MessageDirection.Input)
                {
                    str = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_Input{2}Message", new object[] { name, str3, str4 });
                }
                else
                {
                    str = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_Output{2}Message", new object[] { name, str3, str4 });
                }
            }
            System.Web.Services.Description.ServiceDescription serviceDescription = this.contractContext.WsdlPortType.ServiceDescription;
            return this.GetUniqueMessageName(serviceDescription, str);
        }

        private int GetParameterCount()
        {
            int position = -1;
            foreach (MessageDescription description in this.operation.Messages)
            {
                foreach (MessagePartDescription description2 in description.Body.Parts)
                {
                    ParameterInfo additionalAttributesProvider = description2.AdditionalAttributesProvider as ParameterInfo;
                    if (additionalAttributesProvider == null)
                    {
                        return 0;
                    }
                    if (position < additionalAttributesProvider.Position)
                    {
                        position = additionalAttributesProvider.Position;
                    }
                }
            }
            return (position + 1);
        }

        private string GetUniqueMessageName(System.Web.Services.Description.ServiceDescription wsdl, string messageNameBase)
        {
            return NamingHelper.GetUniqueName(messageNameBase, new NamingHelper.DoesNameExist(MessageContractExporter.DoesMessageNameExist), wsdl);
        }

        protected abstract bool IsEncoded();
        protected static bool IsNullOrEmpty(XmlQualifiedName qname)
        {
            if (qname != null)
            {
                return qname.IsEmpty;
            }
            return true;
        }

        protected bool IsOperationInherited()
        {
            return (this.operation.DeclaringContract != this.contractContext.Contract);
        }

        protected abstract bool IsRpcStyle();
        protected abstract object OnExportMessageContract();

        protected XsdDataContractExporter DataContractExporter
        {
            get
            {
                object obj2;
                if (!this.exporter.State.TryGetValue(typeof(XsdDataContractExporter), out obj2))
                {
                    obj2 = new XsdDataContractExporter(this.exporter.GeneratedXmlSchemas);
                    this.exporter.State.Add(typeof(XsdDataContractExporter), obj2);
                }
                return (XsdDataContractExporter) obj2;
            }
        }

        protected MessageExportContext ExportedMessages
        {
            get
            {
                return GetMessageExportContext(this.exporter);
            }
        }

        protected XmlSchemaSet SchemaSet
        {
            get
            {
                return this.exporter.GeneratedXmlSchemas;
            }
        }

        private class MessageBindingExporter
        {
            private WsdlEndpointConversionContext endpointContext;
            private MessageContractExporter.MessageExportContext exportedMessages;
            private WsdlExporter exporter;
            private EnvelopeVersion soapVersion;

            internal MessageBindingExporter(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext)
            {
                this.endpointContext = endpointContext;
                this.exportedMessages = (MessageContractExporter.MessageExportContext) exporter.State[typeof(MessageContractExporter.MessageExportContext)];
                this.soapVersion = SoapHelper.GetSoapVersion(endpointContext.WsdlBinding);
                this.exporter = exporter;
            }

            private void ExportFaultBinding(FaultDescription fault, bool isEncoded, OperationBinding operationBinding)
            {
                SoapHelper.CreateSoapFaultBinding(fault.Name, this.endpointContext, this.endpointContext.GetFaultBinding(fault), isEncoded);
            }

            internal void ExportMessageBinding(OperationDescription operation, Type messageContractExporterType)
            {
                bool flag;
                bool flag2;
                OperationBinding operationBinding = this.endpointContext.GetOperationBinding(operation);
                if (GetStyleAndUse(operation, messageContractExporterType, out flag, out flag2))
                {
                    SoapOperationBinding binding2 = SoapHelper.GetOrCreateSoapOperationBinding(this.endpointContext, operation, this.exporter);
                    if (binding2 != null)
                    {
                        binding2.Style = flag ? SoapBindingStyle.Rpc : SoapBindingStyle.Document;
                        if (flag)
                        {
                            SoapBinding binding3 = (SoapBinding) this.endpointContext.WsdlBinding.Extensions.Find(typeof(SoapBinding));
                            binding3.Style = binding2.Style;
                        }
                        binding2.SoapAction = operation.Messages[0].Action;
                        foreach (MessageDescription description in operation.Messages)
                        {
                            Message message;
                            MessageBinding messageBinding = this.endpointContext.GetMessageBinding(description);
                            if (this.exportedMessages.WsdlHeaderMessages.TryGetValue(new MessageContractExporter.MessageDescriptionDictionaryKey(this.endpointContext.Endpoint.Contract, description), out message))
                            {
                                XmlQualifiedName messageName = new XmlQualifiedName(message.Name, message.ServiceDescription.TargetNamespace);
                                foreach (MessageHeaderDescription description2 in description.Headers)
                                {
                                    if (!description2.IsUnknownHeaderCollection)
                                    {
                                        this.ExportMessageHeaderBinding(description2, messageName, flag2, messageBinding);
                                    }
                                }
                            }
                            this.ExportMessageBodyBinding(description, flag, flag2, messageBinding);
                        }
                        foreach (FaultDescription description3 in operation.Faults)
                        {
                            this.ExportFaultBinding(description3, flag2, operationBinding);
                        }
                    }
                }
            }

            private void ExportMessageBodyBinding(MessageDescription messageDescription, bool isRpc, bool isEncoded, MessageBinding messageBinding)
            {
                SoapBodyBinding binding = SoapHelper.GetOrCreateSoapBodyBinding(this.endpointContext, messageBinding, this.exporter);
                if (binding != null)
                {
                    binding.Use = isEncoded ? SoapBindingUse.Encoded : SoapBindingUse.Literal;
                    if (isRpc)
                    {
                        string wrapperNamespace;
                        if (!this.ExportedMessages.WrapperNamespaces.TryGetValue(new MessageContractExporter.MessageDescriptionDictionaryKey(this.endpointContext.ContractConversionContext.Contract, messageDescription), out wrapperNamespace))
                        {
                            wrapperNamespace = messageDescription.Body.WrapperNamespace;
                        }
                        binding.Namespace = wrapperNamespace;
                    }
                    if (isEncoded)
                    {
                        binding.Encoding = XmlSerializerOperationFormatter.GetEncoding(this.soapVersion);
                    }
                }
            }

            private void ExportMessageHeaderBinding(MessageHeaderDescription header, XmlQualifiedName messageName, bool isEncoded, MessageBinding messageBinding)
            {
                SoapHeaderBinding binding = SoapHelper.CreateSoapHeaderBinding(this.endpointContext, messageBinding);
                binding.Part = string.IsNullOrEmpty(header.UniquePartName) ? header.Name : header.UniquePartName;
                binding.Message = messageName;
                binding.Use = isEncoded ? SoapBindingUse.Encoded : SoapBindingUse.Literal;
                if (isEncoded)
                {
                    binding.Encoding = XmlSerializerOperationFormatter.GetEncoding(this.soapVersion);
                }
            }

            private static bool GetStyleAndUse(OperationDescription operation, Type messageContractExporterType, out bool isRpc, out bool isEncoded)
            {
                isRpc = isEncoded = false;
                if ((messageContractExporterType == typeof(DataContractSerializerMessageContractExporter)) || (messageContractExporterType == null))
                {
                    DataContractSerializerOperationBehavior behavior = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                    if (behavior != null)
                    {
                        isRpc = behavior.DataContractFormatAttribute.Style == OperationFormatStyle.Rpc;
                        isEncoded = false;
                        return true;
                    }
                    if (messageContractExporterType == typeof(DataContractSerializerMessageContractExporter))
                    {
                        return false;
                    }
                }
                if ((messageContractExporterType == typeof(XmlSerializerMessageContractExporter)) || (messageContractExporterType == null))
                {
                    XmlSerializerOperationBehavior behavior2 = operation.Behaviors.Find<XmlSerializerOperationBehavior>();
                    if (behavior2 != null)
                    {
                        isRpc = behavior2.XmlSerializerFormatAttribute.Style == OperationFormatStyle.Rpc;
                        isEncoded = behavior2.XmlSerializerFormatAttribute.IsEncoded;
                        return true;
                    }
                }
                return false;
            }

            private MessageContractExporter.MessageExportContext ExportedMessages
            {
                get
                {
                    return MessageContractExporter.GetMessageExportContext(this.exporter);
                }
            }
        }

        protected sealed class MessageDescriptionDictionaryKey
        {
            public readonly ContractDescription Contract;
            public readonly System.ServiceModel.Description.MessageDescription MessageDescription;

            public MessageDescriptionDictionaryKey(ContractDescription contract, System.ServiceModel.Description.MessageDescription MessageDescription)
            {
                this.Contract = contract;
                this.MessageDescription = MessageDescription;
            }

            public override bool Equals(object obj)
            {
                MessageContractExporter.MessageDescriptionDictionaryKey key = obj as MessageContractExporter.MessageDescriptionDictionaryKey;
                return (((key != null) && (key.MessageDescription == this.MessageDescription)) && (key.Contract == this.Contract));
            }

            public override int GetHashCode()
            {
                return (this.Contract.GetHashCode() ^ this.MessageDescription.GetHashCode());
            }
        }

        protected class MessageExportContext
        {
            internal readonly Dictionary<XmlQualifiedName, MessageContractExporter.OperationElement> ElementTypes = new Dictionary<XmlQualifiedName, MessageContractExporter.OperationElement>();
            internal readonly Dictionary<MessageContractExporter.OperationMessageKey, Message> ParameterMessages = new Dictionary<MessageContractExporter.OperationMessageKey, Message>();
            internal readonly Dictionary<MessageContractExporter.TypedMessageKey, Message> TypedHeaderMessages = new Dictionary<MessageContractExporter.TypedMessageKey, Message>();
            internal readonly Dictionary<MessageContractExporter.TypedMessageKey, Message> TypedMessages = new Dictionary<MessageContractExporter.TypedMessageKey, Message>();
            internal readonly Dictionary<MessageContractExporter.MessageDescriptionDictionaryKey, string> WrapperNamespaces = new Dictionary<MessageContractExporter.MessageDescriptionDictionaryKey, string>();
            internal readonly Dictionary<MessageContractExporter.MessageDescriptionDictionaryKey, Message> WsdlHeaderMessages = new Dictionary<MessageContractExporter.MessageDescriptionDictionaryKey, Message>();
            internal readonly Dictionary<MessageContractExporter.MessageDescriptionDictionaryKey, Message> WsdlMessages = new Dictionary<MessageContractExporter.MessageDescriptionDictionaryKey, Message>();
        }

        internal sealed class OperationElement
        {
            private XmlSchemaElement element;
            private OperationDescription operation;

            internal OperationElement(XmlSchemaElement element, OperationDescription operation)
            {
                this.element = element;
                this.operation = operation;
            }

            internal XmlSchemaElement Element
            {
                get
                {
                    return this.element;
                }
            }

            internal OperationDescription Operation
            {
                get
                {
                    return this.operation;
                }
            }
        }

        internal sealed class OperationMessageKey
        {
            private ContractDescription declaringContract;
            private int messageIndex;
            private MethodInfo methodInfo;

            public OperationMessageKey(OperationDescription operation, int messageIndex)
            {
                this.methodInfo = operation.OperationMethod;
                this.messageIndex = messageIndex;
                this.declaringContract = operation.DeclaringContract;
            }

            public override bool Equals(object obj)
            {
                MessageContractExporter.OperationMessageKey key = obj as MessageContractExporter.OperationMessageKey;
                return ((((key != null) && (key.methodInfo == this.methodInfo)) && ((key.messageIndex == this.messageIndex) && (key.declaringContract.Name == this.declaringContract.Name))) && (key.declaringContract.Namespace == this.declaringContract.Namespace));
            }

            public override int GetHashCode()
            {
                return (this.methodInfo.GetHashCode() ^ this.messageIndex);
            }
        }

        internal sealed class TypedMessageKey
        {
            private string contractNS;
            private object extensionData;
            private Type type;

            public TypedMessageKey(Type type, string contractNS, object extensionData)
            {
                this.type = type;
                this.contractNS = contractNS;
                this.extensionData = extensionData;
            }

            public override bool Equals(object obj)
            {
                MessageContractExporter.TypedMessageKey key = obj as MessageContractExporter.TypedMessageKey;
                return (((key != null) && (key.type == this.type)) && ((key.contractNS == this.contractNS) && key.extensionData.Equals(this.extensionData)));
            }

            public override int GetHashCode()
            {
                return this.type.GetHashCode();
            }
        }
    }
}

