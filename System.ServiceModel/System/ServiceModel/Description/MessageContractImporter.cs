namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal class MessageContractImporter
    {
        private readonly XmlSchemaSet allSchemas;
        private static readonly XmlQualifiedName AnyType = new XmlQualifiedName("anyType", "http://www.w3.org/2001/XMLSchema");
        private Dictionary<System.Web.Services.Description.Message, IList<string>> bodyPartsTable;
        private readonly WsdlContractConversionContext contractContext;
        private readonly FaultImportOptions faultImportOptions;
        private readonly WsdlImporter importer;
        private SchemaImporter schemaImporter;
        private static object schemaImporterLock = new object();

        private MessageContractImporter(WsdlImporter importer, WsdlContractConversionContext contractContext, SchemaImporter schemaImporter)
        {
            object obj2;
            this.contractContext = contractContext;
            this.importer = importer;
            this.allSchemas = GatherSchemas(importer);
            this.schemaImporter = schemaImporter;
            if (this.importer.State.TryGetValue(typeof(FaultImportOptions), out obj2))
            {
                this.faultImportOptions = (FaultImportOptions) obj2;
            }
            else
            {
                this.faultImportOptions = new FaultImportOptions();
            }
        }

        private void AddError(string message)
        {
            this.AddError(message, false);
        }

        private void AddError(string message, bool isWarning)
        {
            MetadataConversionError item = new MetadataConversionError(message, isWarning);
            if (!this.importer.Errors.Contains(item))
            {
                this.importer.Errors.Add(item);
            }
        }

        private static void AddImport(System.Xml.Schema.XmlSchema schema, Hashtable imports, XmlSchemaSet allSchemas)
        {
            if ((schema != null) && (imports[schema] == null))
            {
                imports.Add(schema, schema);
                foreach (XmlSchemaExternal external in schema.Includes)
                {
                    if (external is XmlSchemaImport)
                    {
                        XmlSchemaImport import = (XmlSchemaImport) external;
                        foreach (System.Xml.Schema.XmlSchema schema2 in allSchemas.Schemas(import.Namespace))
                        {
                            AddImport(schema2, imports, allSchemas);
                        }
                    }
                }
            }
        }

        private static void AddSchema(System.Xml.Schema.XmlSchema schema, bool isEncoded, bool isLiteral, XmlSchemas encodedSchemas, XmlSchemas literalSchemas, Hashtable references)
        {
            if (schema != null)
            {
                if (isEncoded && !encodedSchemas.Contains(schema))
                {
                    if (references.Contains(schema))
                    {
                        encodedSchemas.AddReference(schema);
                    }
                    else
                    {
                        encodedSchemas.Add(schema);
                    }
                }
                if (isLiteral && !literalSchemas.Contains(schema))
                {
                    if (references.Contains(schema))
                    {
                        literalSchemas.AddReference(schema);
                    }
                    else
                    {
                        literalSchemas.Add(schema);
                    }
                }
            }
        }

        private static void AddSoapEncodingSchemaIfNeeded(XmlSchemas schemas)
        {
            System.Xml.Schema.XmlSchema schema = StockSchemas.CreateFakeXsdSchema();
            foreach (System.Xml.Schema.XmlSchema schema2 in schemas)
            {
                foreach (XmlSchemaImport import in schema2.Includes)
                {
                    if ((import != null) && (import.Namespace == schema.TargetNamespace))
                    {
                        schemas.Add(schema);
                        break;
                    }
                }
            }
        }

        internal void AddWarning(string message)
        {
            this.AddError(message, true);
        }

        private bool CanImportAnyMessage(MessagePart part)
        {
            return CheckPart(part.Type, DataContractSerializerMessageContractImporter.GenericMessageTypeName);
        }

        private bool CanImportFault(OperationFault fault, OperationDescription description)
        {
            XmlSchemaElement element;
            XmlQualifiedName name;
            XmlQualifiedName name2;
            if (!this.ValidateFault(fault, description, out element, out name, out name2))
            {
                return false;
            }
            return this.CurrentSchemaImporter.CanImportFault(element, name);
        }

        private bool CanImportFaultBinding(FaultBinding faultBinding, OperationFormatStyle style, out bool isFaultEncoded)
        {
            bool? nullable = null;
            foreach (object obj2 in faultBinding.Extensions)
            {
                bool flag;
                XmlElement element = obj2 as XmlElement;
                if (SoapHelper.IsSoapFaultBinding(element))
                {
                    flag = SoapHelper.IsEncoded(element);
                }
                else
                {
                    SoapFaultBinding soapFaultBinding = obj2 as SoapFaultBinding;
                    if ((soapFaultBinding == null) || !ValidWsdl.Check(soapFaultBinding, faultBinding, new WsdlWarningHandler(this.AddWarning)))
                    {
                        continue;
                    }
                    flag = soapFaultBinding.Use == SoapBindingUse.Encoded;
                }
                if (!nullable.HasValue)
                {
                    nullable = new bool?(flag);
                }
                else if (nullable.Value != flag)
                {
                    this.AddError(System.ServiceModel.SR.GetString("SFxInconsistentWsdlOperationUseInBindingExtensions", new object[] { faultBinding.OperationBinding.Name, faultBinding.OperationBinding.Binding.Name }));
                }
            }
            bool? nullable2 = nullable;
            isFaultEncoded = nullable2.HasValue ? nullable2.GetValueOrDefault() : false;
            return this.CurrentSchemaImporter.CanImportStyleAndUse(style, isFaultEncoded);
        }

        private bool CanImportFaults(Operation operation, OperationDescription description)
        {
            if (this.faultImportOptions.UseMessageFormat)
            {
                foreach (OperationFault fault in operation.Faults)
                {
                    if (!this.CanImportFault(fault, description))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool CanImportMessage(System.Web.Services.Description.Message wsdlMessage, string operationName, out OperationFormatStyle? inferredStyle, ref bool areAllMessagesWrapped)
        {
            IList<string> list;
            MessagePartCollection parts = wsdlMessage.Parts;
            if (parts.Count == 1)
            {
                if (this.CanImportAnyMessage(parts[0]))
                {
                    areAllMessagesWrapped = false;
                    inferredStyle = 0;
                    return true;
                }
                if (this.CanImportStream(parts[0], out inferredStyle, ref areAllMessagesWrapped))
                {
                    return true;
                }
                if (areAllMessagesWrapped && this.CanImportWrappedMessage(parts[0]))
                {
                    inferredStyle = 0;
                    return true;
                }
                areAllMessagesWrapped = false;
            }
            inferredStyle = 0;
            this.BodyPartsTable.TryGetValue(wsdlMessage, out list);
            foreach (MessagePart part in parts)
            {
                if ((list == null) || list.Contains(part.Name))
                {
                    OperationFormatStyle style;
                    if (!this.CurrentSchemaImporter.CanImportMessagePart(part, out style))
                    {
                        return false;
                    }
                    if (!inferredStyle.HasValue)
                    {
                        inferredStyle = new OperationFormatStyle?(style);
                    }
                    else if (style != ((OperationFormatStyle) inferredStyle.Value))
                    {
                        this.AddError(System.ServiceModel.SR.GetString("SFxInconsistentWsdlOperationStyleInMessageParts", new object[] { operationName }));
                    }
                }
            }
            return true;
        }

        private bool CanImportMessageBinding(MessageBinding messageBinding, System.Web.Services.Description.Message wsdlMessage, OperationFormatStyle style, out bool isEncoded)
        {
            isEncoded = false;
            bool? nullable = null;
            foreach (object obj2 in messageBinding.Extensions)
            {
                bool flag;
                SoapHeaderBinding soapHeaderBinding = obj2 as SoapHeaderBinding;
                if (soapHeaderBinding != null)
                {
                    if (!ValidWsdl.Check(soapHeaderBinding, messageBinding, new WsdlWarningHandler(this.AddWarning)))
                    {
                        return false;
                    }
                    if (!this.CanImportMessageHeaderBinding(soapHeaderBinding, wsdlMessage, style, out flag))
                    {
                        return false;
                    }
                    if (!nullable.HasValue)
                    {
                        nullable = new bool?(flag);
                    }
                    else if (nullable.Value != flag)
                    {
                        this.AddError(System.ServiceModel.SR.GetString("SFxInconsistentWsdlOperationUseInBindingExtensions", new object[] { messageBinding.OperationBinding.Name, messageBinding.OperationBinding.Binding.Name }));
                    }
                }
                else
                {
                    SoapBodyBinding bodyBinding = obj2 as SoapBodyBinding;
                    if (bodyBinding != null)
                    {
                        IList<string> list;
                        if (!this.CanImportMessageBodyBinding(bodyBinding, style, out flag))
                        {
                            return false;
                        }
                        if (!nullable.HasValue)
                        {
                            nullable = new bool?(flag);
                        }
                        else if (nullable.Value != flag)
                        {
                            this.AddError(System.ServiceModel.SR.GetString("SFxInconsistentWsdlOperationUseInBindingExtensions", new object[] { messageBinding.OperationBinding.Name, messageBinding.OperationBinding.Binding.Name }));
                        }
                        string[] parts = bodyBinding.Parts;
                        if (parts == null)
                        {
                            parts = new string[wsdlMessage.Parts.Count];
                            for (int i = 0; i < parts.Length; i++)
                            {
                                parts[i] = wsdlMessage.Parts[i].Name;
                            }
                        }
                        bool flag2 = false;
                        if (!this.BodyPartsTable.TryGetValue(wsdlMessage, out list))
                        {
                            list = new List<string>();
                            this.BodyPartsTable.Add(wsdlMessage, list);
                            flag2 = true;
                        }
                        foreach (string str in parts)
                        {
                            if (!string.IsNullOrEmpty(str))
                            {
                                if (flag2)
                                {
                                    list.Add(str);
                                }
                                else if (!list.Contains(str))
                                {
                                    this.AddError(System.ServiceModel.SR.GetString("SFxInconsistentBindingBodyParts", new object[] { messageBinding.OperationBinding.Name, messageBinding.OperationBinding.Binding.Name, str }));
                                    list.Add(str);
                                }
                            }
                        }
                    }
                }
            }
            if (nullable.HasValue)
            {
                isEncoded = nullable.Value;
            }
            return true;
        }

        private bool CanImportMessageBodyBinding(SoapBodyBinding bodyBinding, OperationFormatStyle style, out bool isEncoded)
        {
            isEncoded = bodyBinding.Use == SoapBindingUse.Encoded;
            return this.CurrentSchemaImporter.CanImportStyleAndUse(style, isEncoded);
        }

        private bool CanImportMessageHeaderBinding(SoapHeaderBinding headerBinding, System.Web.Services.Description.Message wsdlMessage, OperationFormatStyle style, out bool isEncoded)
        {
            OperationFormatStyle style2;
            isEncoded = headerBinding.Use == SoapBindingUse.Encoded;
            MessagePart part = FindPartByName(wsdlMessage.ServiceDescription.ServiceDescriptions.GetMessage(headerBinding.Message), headerBinding.Part);
            if (!this.CurrentSchemaImporter.CanImportMessagePart(part, out style2))
            {
                return false;
            }
            if (style2 != style)
            {
                this.AddError(System.ServiceModel.SR.GetString("SFxInconsistentWsdlOperationStyleInHeader", new object[] { part.Name, style2, style }));
            }
            return this.CurrentSchemaImporter.CanImportStyleAndUse(style, isEncoded);
        }

        private bool CanImportOperation(OperationDescription operation, out OperationInfo operationInfo)
        {
            operationInfo = null;
            if (OperationHasBeenHandled(operation))
            {
                return false;
            }
            Operation operation2 = this.contractContext.GetOperation(operation);
            Collection<OperationBinding> operationBindings = this.contractContext.GetOperationBindings(operation2);
            return (this.CanImportOperation(operation, operation2, operationBindings, out operationInfo) && this.CanImportFaults(operation2, operation));
        }

        private bool CanImportOperation(OperationDescription operation, Operation wsdlOperation, Collection<OperationBinding> operationBindings, out OperationInfo operationInfo)
        {
            operationInfo = null;
            OperationFormatStyle document = OperationFormatStyle.Document;
            bool isEncoded = false;
            bool areAllMessagesWrapped = true;
            StyleAndUse? nullable = null;
            ServiceDescriptionCollection serviceDescriptions = wsdlOperation.PortType.ServiceDescription.ServiceDescriptions;
            OperationBinding binding = null;
            foreach (OperationBinding binding2 in operationBindings)
            {
                OperationFormatStyle style = GetStyle(binding2);
                bool? nullable2 = null;
                foreach (MessageDescription description in operation.Messages)
                {
                    OperationMessage operationMessage = this.contractContext.GetOperationMessage(description);
                    if (operationMessage.Message.IsEmpty)
                    {
                        if (operationMessage is OperationInput)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxWsdlOperationInputNeedsMessageAttribute2", new object[] { wsdlOperation.Name, wsdlOperation.PortType.Name })));
                        }
                        if (operationMessage is OperationOutput)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxWsdlOperationOutputNeedsMessageAttribute2", new object[] { wsdlOperation.Name, wsdlOperation.PortType.Name })));
                        }
                    }
                    System.Web.Services.Description.Message message = serviceDescriptions.GetMessage(operationMessage.Message);
                    if (message != null)
                    {
                        MessageBinding messageBinding = (description.Direction == MessageDirection.Input) ? ((MessageBinding) binding2.Input) : ((MessageBinding) binding2.Output);
                        if (messageBinding != null)
                        {
                            bool flag3;
                            if (!this.CanImportMessageBinding(messageBinding, message, style, out flag3))
                            {
                                return false;
                            }
                            if (!nullable2.HasValue)
                            {
                                nullable2 = new bool?(flag3);
                            }
                            else
                            {
                                bool? nullable5 = nullable2;
                                bool flag6 = flag3;
                                if ((nullable5.GetValueOrDefault() != flag6) || !nullable5.HasValue)
                                {
                                    this.AddError(System.ServiceModel.SR.GetString("SFxInconsistentWsdlOperationUseInBindingMessages", new object[] { messageBinding.OperationBinding.Name, messageBinding.OperationBinding.Binding.Name }));
                                }
                            }
                        }
                    }
                }
                foreach (FaultBinding binding4 in binding2.Faults)
                {
                    bool flag4;
                    if (!this.CanImportFaultBinding(binding4, style, out flag4))
                    {
                        return false;
                    }
                    if (!nullable2.HasValue)
                    {
                        nullable2 = new bool?(flag4);
                    }
                    else
                    {
                        bool? nullable6 = nullable2;
                        bool flag7 = flag4;
                        if ((nullable6.GetValueOrDefault() != flag7) || !nullable6.HasValue)
                        {
                            this.AddError(System.ServiceModel.SR.GetString("SFxInconsistentWsdlOperationUseInBindingFaults", new object[] { binding4.OperationBinding.Name, binding4.OperationBinding.Binding.Name }));
                        }
                    }
                }
                bool? nullable7 = nullable2;
                nullable2 = new bool?(nullable7.HasValue ? nullable7.GetValueOrDefault() : false);
                if (!nullable.HasValue)
                {
                    nullable = new StyleAndUse?(GetStyleAndUse(style, nullable2.Value));
                    document = style;
                    isEncoded = nullable2.Value;
                    binding = binding2;
                }
                else
                {
                    StyleAndUse styleAndUse = GetStyleAndUse(style, nullable2.Value);
                    if (styleAndUse != ((StyleAndUse) nullable))
                    {
                        this.AddError(System.ServiceModel.SR.GetString("SFxInconsistentWsdlOperationUseAndStyleInBinding", new object[] { operation.Name, binding2.Binding.Name, GetUse(styleAndUse), GetStyle(styleAndUse), binding.Binding.Name, GetUse(nullable.Value), GetStyle(nullable.Value) }));
                    }
                    if (styleAndUse < ((StyleAndUse) nullable))
                    {
                        nullable = new StyleAndUse?(styleAndUse);
                        document = style;
                        isEncoded = nullable2.Value;
                        binding = binding2;
                    }
                }
            }
            OperationFormatStyle? nullable3 = null;
            foreach (OperationMessage message3 in wsdlOperation.Messages)
            {
                OperationFormatStyle? nullable4;
                if (message3.Message.IsEmpty)
                {
                    if (message3 is OperationInput)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxWsdlOperationInputNeedsMessageAttribute2", new object[] { wsdlOperation.Name, wsdlOperation.PortType.Name })));
                    }
                    if (message3 is OperationOutput)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxWsdlOperationOutputNeedsMessageAttribute2", new object[] { wsdlOperation.Name, wsdlOperation.PortType.Name })));
                    }
                }
                System.Web.Services.Description.Message wsdlMessage = serviceDescriptions.GetMessage(message3.Message);
                if (!this.CanImportMessage(wsdlMessage, message3.Name, out nullable4, ref areAllMessagesWrapped))
                {
                    return false;
                }
                if ((wsdlMessage.Parts.Count > 0) && (!nullable3.HasValue || (nullable4.HasValue && ((nullable4 != nullable3) && (((OperationFormatStyle) nullable4.Value) == OperationFormatStyle.Document)))))
                {
                    nullable3 = nullable4;
                }
            }
            if (!nullable.HasValue)
            {
                OperationFormatStyle? nullable12 = nullable3;
                document = nullable12.HasValue ? nullable12.GetValueOrDefault() : OperationFormatStyle.Document;
            }
            else if ((nullable3.HasValue && (((OperationFormatStyle) nullable3.Value) != document)) && (((OperationFormatStyle) nullable3.Value) == OperationFormatStyle.Document))
            {
                this.AddError(System.ServiceModel.SR.GetString("SFxInconsistentWsdlOperationStyleInOperationMessages", new object[] { operation.Name, nullable3, document }));
            }
            operationInfo = new OperationInfo(document, isEncoded, areAllMessagesWrapped);
            return true;
        }

        private bool CanImportStream(MessagePart part, out OperationFormatStyle? style, ref bool areAllMessagesWrapped)
        {
            style = 0;
            if (areAllMessagesWrapped && this.IsWrapperPart(part))
            {
                string str;
                XmlSchemaForm form;
                XmlSchemaComplexType complexType = GetElementComplexType(part.Element, this.allSchemas, out str, out form);
                if (complexType != null)
                {
                    XmlSchemaSequence rootSequence = GetRootSequence(complexType);
                    if (((rootSequence != null) && (rootSequence.Items.Count == 1)) && (rootSequence.Items[0] is XmlSchemaElement))
                    {
                        return CheckPart(((XmlSchemaElement) rootSequence.Items[0]).SchemaTypeName, DataContractSerializerMessageContractImporter.StreamBodyTypeName);
                    }
                }
                return false;
            }
            areAllMessagesWrapped = false;
            XmlQualifiedName qname = part.Type;
            style = 1;
            if (IsNullOrEmpty(qname))
            {
                if (IsNullOrEmpty(part.Element))
                {
                    return false;
                }
                style = 0;
                qname = GetTypeName(FindSchemaElement(this.allSchemas, part.Element));
            }
            return CheckPart(qname, DataContractSerializerMessageContractImporter.StreamBodyTypeName);
        }

        private bool CanImportWrappedMessage(MessagePart wsdlPart)
        {
            if (!this.IsWrapperPart(wsdlPart))
            {
                return false;
            }
            return this.CurrentSchemaImporter.CanImportWrapperElement(wsdlPart.Element);
        }

        private static bool CheckAndAddPart(XmlQualifiedName typeNameFound, XmlQualifiedName typeNameRequired, string name, string ns, System.Type type, MessageDescription description, bool isReply)
        {
            if (IsNullOrEmpty(typeNameFound) || (typeNameFound != typeNameRequired))
            {
                return false;
            }
            MessagePartDescription item = new MessagePartDescription(name, ns) {
                Type = type
            };
            if (isReply && (description.Body.ReturnValue == null))
            {
                description.Body.ReturnValue = item;
            }
            else
            {
                description.Body.Parts.Add(item);
            }
            return true;
        }

        private bool CheckIsRef(MessageDescription requestMessage, MessagePartDescription part)
        {
            foreach (MessagePartDescription description in requestMessage.Body.Parts)
            {
                if (this.CompareMessageParts(description, part))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CheckPart(XmlQualifiedName typeNameFound, XmlQualifiedName typeNameRequired)
        {
            return (!IsNullOrEmpty(typeNameFound) && (typeNameFound == typeNameRequired));
        }

        private static void CollectEncodedAndLiteralSchemas(ServiceDescriptionCollection serviceDescriptions, XmlSchemas encodedSchemas, XmlSchemas literalSchemas, XmlSchemaSet allSchemas)
        {
            Hashtable hashtable2;
            System.Xml.Schema.XmlSchema schema = StockSchemas.CreateWsdl();
            System.Xml.Schema.XmlSchema schema2 = StockSchemas.CreateSoap();
            System.Xml.Schema.XmlSchema schema3 = StockSchemas.CreateSoapEncoding();
            Hashtable references = new Hashtable();
            if (!allSchemas.Contains(schema.TargetNamespace))
            {
                references[schema2] = schema;
            }
            if (!allSchemas.Contains(schema2.TargetNamespace))
            {
                references[schema2] = schema2;
            }
            if (!allSchemas.Contains(schema3.TargetNamespace))
            {
                references[schema3] = schema3;
            }
            foreach (System.Web.Services.Description.ServiceDescription description in serviceDescriptions)
            {
                foreach (System.Web.Services.Description.Message message in description.Messages)
                {
                    foreach (MessagePart part in message.Parts)
                    {
                        bool flag;
                        bool flag2;
                        FindUse(part, out flag, out flag2);
                        if ((part.Element != null) && !part.Element.IsEmpty)
                        {
                            XmlSchemaElement element = FindSchemaElement(allSchemas, part.Element);
                            if (element != null)
                            {
                                AddSchema(element.Parent as System.Xml.Schema.XmlSchema, flag, flag2, encodedSchemas, literalSchemas, references);
                                if ((element.SchemaTypeName != null) && !element.SchemaTypeName.IsEmpty)
                                {
                                    XmlSchemaType type = FindSchemaType(allSchemas, element.SchemaTypeName);
                                    if (type != null)
                                    {
                                        AddSchema(type.Parent as System.Xml.Schema.XmlSchema, flag, flag2, encodedSchemas, literalSchemas, references);
                                    }
                                }
                            }
                        }
                        if ((part.Type != null) && !part.Type.IsEmpty)
                        {
                            XmlSchemaType type2 = FindSchemaType(allSchemas, part.Type);
                            if (type2 != null)
                            {
                                AddSchema(type2.Parent as System.Xml.Schema.XmlSchema, flag, flag2, encodedSchemas, literalSchemas, references);
                            }
                        }
                    }
                }
            }
            foreach (XmlSchemas schemas in new XmlSchemas[] { encodedSchemas, literalSchemas })
            {
                hashtable2 = new Hashtable();
                foreach (System.Xml.Schema.XmlSchema schema4 in schemas)
                {
                    AddImport(schema4, hashtable2, allSchemas);
                }
                foreach (System.Xml.Schema.XmlSchema schema5 in hashtable2.Keys)
                {
                    if ((references[schema5] == null) && !schemas.Contains(schema5))
                    {
                        schemas.Add(schema5);
                    }
                }
            }
            hashtable2 = new Hashtable();
            foreach (System.Xml.Schema.XmlSchema schema6 in allSchemas.Schemas())
            {
                if (!encodedSchemas.Contains(schema6) && !literalSchemas.Contains(schema6))
                {
                    AddImport(schema6, hashtable2, allSchemas);
                }
            }
            foreach (System.Xml.Schema.XmlSchema schema7 in hashtable2.Keys)
            {
                if (references[schema7] == null)
                {
                    if (!encodedSchemas.Contains(schema7))
                    {
                        encodedSchemas.Add(schema7);
                    }
                    if (!literalSchemas.Contains(schema7))
                    {
                        literalSchemas.Add(schema7);
                    }
                }
            }
            if (encodedSchemas.Count > 0)
            {
                foreach (System.Xml.Schema.XmlSchema schema8 in references.Values)
                {
                    encodedSchemas.AddReference(schema8);
                }
            }
            if (literalSchemas.Count > 0)
            {
                foreach (System.Xml.Schema.XmlSchema schema9 in references.Values)
                {
                    literalSchemas.AddReference(schema9);
                }
            }
            AddSoapEncodingSchemaIfNeeded(literalSchemas);
        }

        private bool CompareMessageParts(MessagePartDescription x, MessagePartDescription y)
        {
            return ((x.Name == y.Name) && (x.Namespace == y.Namespace));
        }

        private static MessagePart FindPartByName(System.Web.Services.Description.Message message, string name)
        {
            foreach (MessagePart part in message.Parts)
            {
                if (part.Name == name)
                {
                    return part;
                }
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxWsdlMessageDoesNotContainPart3", new object[] { name, message.Name, message.ServiceDescription.TargetNamespace })));
        }

        private static XmlSchemaElement FindSchemaElement(XmlSchemaSet schemaSet, XmlQualifiedName elementName)
        {
            System.Xml.Schema.XmlSchema schema;
            return FindSchemaElement(schemaSet, elementName, out schema);
        }

        private static XmlSchemaElement FindSchemaElement(XmlSchemaSet schemaSet, XmlQualifiedName elementName, out System.Xml.Schema.XmlSchema containingSchema)
        {
            XmlSchemaElement element = null;
            containingSchema = null;
            foreach (System.Xml.Schema.XmlSchema schema in GetSchema(schemaSet, elementName.Namespace))
            {
                element = (XmlSchemaElement) schema.Elements[elementName];
                if (element != null)
                {
                    containingSchema = schema;
                    break;
                }
            }
            if (element == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxSchemaDoesNotContainElement", new object[] { elementName.Name, elementName.Namespace })));
            }
            return element;
        }

        private static XmlSchemaType FindSchemaType(XmlSchemaSet schemaSet, XmlQualifiedName typeName)
        {
            System.Xml.Schema.XmlSchema schema;
            if (typeName.Namespace == "http://www.w3.org/2001/XMLSchema")
            {
                return null;
            }
            return FindSchemaType(schemaSet, typeName, out schema);
        }

        private static XmlSchemaType FindSchemaType(XmlSchemaSet schemaSet, XmlQualifiedName typeName, out System.Xml.Schema.XmlSchema containingSchema)
        {
            containingSchema = null;
            if (StockSchemas.IsKnownSchema(typeName.Namespace))
            {
                return null;
            }
            XmlSchemaType type = null;
            foreach (System.Xml.Schema.XmlSchema schema in GetSchema(schemaSet, typeName.Namespace))
            {
                type = (XmlSchemaType) schema.SchemaTypes[typeName];
                if (type != null)
                {
                    containingSchema = schema;
                    break;
                }
            }
            if (type == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxSchemaDoesNotContainType", new object[] { typeName.Name, typeName.Namespace })));
            }
            return type;
        }

        private static void FindUse(MessagePart part, out bool isEncoded, out bool isLiteral)
        {
            isEncoded = false;
            isLiteral = false;
            string name = part.Message.Name;
            Operation operation = null;
            System.Web.Services.Description.ServiceDescription serviceDescription = part.Message.ServiceDescription;
            foreach (PortType type in serviceDescription.PortTypes)
            {
                foreach (Operation operation2 in type.Operations)
                {
                    foreach (OperationMessage message in operation2.Messages)
                    {
                        if (message.Message.Equals(new XmlQualifiedName(part.Message.Name, serviceDescription.TargetNamespace)))
                        {
                            operation = operation2;
                            FindUse(operation, serviceDescription, name, ref isEncoded, ref isLiteral);
                        }
                    }
                }
            }
            if (operation == null)
            {
                FindUse(null, serviceDescription, name, ref isEncoded, ref isLiteral);
            }
        }

        private static void FindUse(Operation operation, System.Web.Services.Description.ServiceDescription description, string messageName, ref bool isEncoded, ref bool isLiteral)
        {
            string targetNamespace = description.TargetNamespace;
            foreach (System.Web.Services.Description.Binding binding in description.Bindings)
            {
                if ((operation == null) || new XmlQualifiedName(operation.PortType.Name, targetNamespace).Equals(binding.Type))
                {
                    foreach (OperationBinding binding2 in binding.Operations)
                    {
                        if (binding2.Input != null)
                        {
                            foreach (object obj2 in binding2.Input.Extensions)
                            {
                                if (operation != null)
                                {
                                    SoapBodyBinding binding3 = obj2 as SoapBodyBinding;
                                    if ((binding3 != null) && operation.IsBoundBy(binding2))
                                    {
                                        if (binding3.Use == SoapBindingUse.Encoded)
                                        {
                                            isEncoded = true;
                                        }
                                        else if (binding3.Use == SoapBindingUse.Literal)
                                        {
                                            isLiteral = true;
                                        }
                                    }
                                }
                                else
                                {
                                    SoapHeaderBinding binding4 = obj2 as SoapHeaderBinding;
                                    if ((binding4 != null) && (binding4.Message.Name == messageName))
                                    {
                                        if (binding4.Use == SoapBindingUse.Encoded)
                                        {
                                            isEncoded = true;
                                        }
                                        else if (binding4.Use == SoapBindingUse.Literal)
                                        {
                                            isLiteral = true;
                                        }
                                    }
                                }
                            }
                        }
                        if (binding2.Output != null)
                        {
                            foreach (object obj3 in binding2.Output.Extensions)
                            {
                                if (operation != null)
                                {
                                    if (operation.IsBoundBy(binding2))
                                    {
                                        SoapBodyBinding binding5 = obj3 as SoapBodyBinding;
                                        if (binding5 != null)
                                        {
                                            if (binding5.Use == SoapBindingUse.Encoded)
                                            {
                                                isEncoded = true;
                                            }
                                            else if (binding5.Use == SoapBindingUse.Literal)
                                            {
                                                isLiteral = true;
                                            }
                                        }
                                        else if (obj3 is MimeXmlBinding)
                                        {
                                            isLiteral = true;
                                        }
                                    }
                                }
                                else
                                {
                                    SoapHeaderBinding binding6 = obj3 as SoapHeaderBinding;
                                    if ((binding6 != null) && (binding6.Message.Name == messageName))
                                    {
                                        if (binding6.Use == SoapBindingUse.Encoded)
                                        {
                                            isEncoded = true;
                                        }
                                        else if (binding6.Use == SoapBindingUse.Literal)
                                        {
                                            isLiteral = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static XmlSchemaSet GatherSchemas(WsdlImporter importer)
        {
            XmlSchemaSet set = new XmlSchemaSet {
                XmlResolver = null
            };
            foreach (System.Web.Services.Description.ServiceDescription description in importer.WsdlDocuments)
            {
                XmlQualifiedName[] nameArray = description.Namespaces.ToArray();
                if ((description.Types != null) && (description.Types.Schemas != null))
                {
                    foreach (System.Xml.Schema.XmlSchema schema in description.Types.Schemas)
                    {
                        XmlSerializerNamespaces namespaces = schema.Namespaces;
                        XmlQualifiedName[] nameArray2 = namespaces.ToArray();
                        Dictionary<string, object> dictionary = new Dictionary<string, object>();
                        foreach (XmlQualifiedName name in nameArray2)
                        {
                            dictionary.Add(name.Name, null);
                        }
                        foreach (XmlQualifiedName name2 in nameArray)
                        {
                            if (!dictionary.ContainsKey(name2.Name))
                            {
                                namespaces.Add(name2.Name, name2.Namespace);
                            }
                        }
                        if (schema.Items.Count > 0)
                        {
                            set.Add(schema);
                        }
                        else
                        {
                            foreach (XmlSchemaExternal external in schema.Includes)
                            {
                                if (external.Schema != null)
                                {
                                    set.Add(external.Schema);
                                }
                            }
                        }
                    }
                }
            }
            set.Add(importer.XmlSchemas);
            return set;
        }

        private static XmlSchemaComplexType GetElementComplexType(XmlQualifiedName elementName, XmlSchemaSet schemaSet, out string ns, out XmlSchemaForm elementFormDefault)
        {
            System.Xml.Schema.XmlSchema schema;
            XmlSchemaElement element = FindSchemaElement(schemaSet, elementName, out schema);
            ns = elementName.Namespace;
            elementFormDefault = schema.ElementFormDefault;
            XmlSchemaType schemaType = null;
            if (element.SchemaType != null)
            {
                schemaType = element.SchemaType;
            }
            else
            {
                XmlQualifiedName typeName = GetTypeName(element);
                if (typeName.Namespace == "http://www.w3.org/2001/XMLSchema")
                {
                    return null;
                }
                schemaType = FindSchemaType(schemaSet, typeName, out schema);
                ns = typeName.Namespace;
                elementFormDefault = schema.ElementFormDefault;
            }
            if (schemaType == null)
            {
                return null;
            }
            return (schemaType as XmlSchemaComplexType);
        }

        private static string GetLocalElementNamespace(string ns, XmlSchemaElement element, XmlSchemaForm elementFormDefault)
        {
            XmlSchemaForm form = (element.Form != XmlSchemaForm.None) ? element.Form : elementFormDefault;
            if (form != XmlSchemaForm.Qualified)
            {
                return string.Empty;
            }
            return ns;
        }

        private static XmlSchemaSequence GetRootSequence(XmlSchemaComplexType complexType)
        {
            if (complexType == null)
            {
                return null;
            }
            if (complexType.Particle == null)
            {
                return null;
            }
            return (complexType.Particle as XmlSchemaSequence);
        }

        private static IEnumerable GetSchema(XmlSchemaSet schemaSet, string ns)
        {
            ICollection is2 = schemaSet.Schemas(ns);
            if ((is2 == null) || (is2.Count == 0))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxSchemaNotFound", new object[] { ns })));
            }
            return is2;
        }

        private static string GetStyle(StyleAndUse styleAndUse)
        {
            if ((styleAndUse != StyleAndUse.RpcLiteral) && (styleAndUse != StyleAndUse.RpcEncoded))
            {
                return "document";
            }
            return "rpc";
        }

        private static SoapBindingStyle GetStyle(System.Web.Services.Description.Binding binding)
        {
            SoapBindingStyle style = SoapBindingStyle.Default;
            if (binding != null)
            {
                SoapBinding binding2 = binding.Extensions.Find(typeof(SoapBinding)) as SoapBinding;
                if (binding2 != null)
                {
                    style = binding2.Style;
                }
            }
            return style;
        }

        private static OperationFormatStyle GetStyle(OperationBinding operationBinding)
        {
            SoapBindingStyle style = GetStyle(operationBinding.Binding);
            if (operationBinding != null)
            {
                SoapOperationBinding binding = operationBinding.Extensions.Find(typeof(SoapOperationBinding)) as SoapOperationBinding;
                if ((binding != null) && (binding.Style != SoapBindingStyle.Default))
                {
                    style = binding.Style;
                }
            }
            if (style != SoapBindingStyle.Rpc)
            {
                return OperationFormatStyle.Document;
            }
            return OperationFormatStyle.Rpc;
        }

        private static StyleAndUse GetStyleAndUse(OperationFormatStyle style, bool isEncoded)
        {
            if (style == OperationFormatStyle.Document)
            {
                if (!isEncoded)
                {
                    return StyleAndUse.DocumentLiteral;
                }
                return StyleAndUse.DocumentEncoded;
            }
            if (!isEncoded)
            {
                return StyleAndUse.RpcLiteral;
            }
            return StyleAndUse.RpcEncoded;
        }

        private static XmlQualifiedName GetTypeName(XmlSchemaElement element)
        {
            if (element.SchemaType != null)
            {
                return XmlQualifiedName.Empty;
            }
            if (IsNullOrEmpty(element.SchemaTypeName))
            {
                return AnyType;
            }
            return element.SchemaTypeName;
        }

        private static string GetUse(StyleAndUse styleAndUse)
        {
            if ((styleAndUse != StyleAndUse.RpcEncoded) && (styleAndUse != StyleAndUse.DocumentEncoded))
            {
                return "literal";
            }
            return "encoded";
        }

        private void ImportFault(OperationFault fault, OperationDescription description, bool isEncoded)
        {
            XmlSchemaElement element;
            XmlQualifiedName name;
            XmlQualifiedName name2;
            if (this.ValidateFault(fault, description, out element, out name, out name2))
            {
                SchemaImporter currentSchemaImporter;
                CodeTypeReference reference;
                if (this.faultImportOptions.UseMessageFormat)
                {
                    currentSchemaImporter = this.CurrentSchemaImporter;
                }
                else
                {
                    currentSchemaImporter = DataContractSerializerSchemaImporter.Get(this.importer);
                }
                if (IsNullOrEmpty(name))
                {
                    reference = currentSchemaImporter.ImportFaultElement(name2, element, isEncoded);
                }
                else
                {
                    reference = currentSchemaImporter.ImportFaultType(name2, name, isEncoded);
                }
                FaultDescription faultDescription = this.contractContext.GetFaultDescription(fault);
                faultDescription.DetailTypeReference = reference;
                faultDescription.ElementName = new System.ServiceModel.Description.XmlName(name2.Name, true);
                faultDescription.Namespace = name2.Namespace;
            }
        }

        private void ImportFaults(Operation operation, OperationDescription description, bool isEncoded)
        {
            foreach (OperationFault fault in operation.Faults)
            {
                this.ImportFault(fault, description, isEncoded);
            }
        }

        private void ImportMessage(OperationMessage wsdlOperationMessage, bool isReply, bool isEncoded, bool areAllMessagesWrapped)
        {
            IList<string> list;
            MessageDescription messageDescription = this.contractContext.GetMessageDescription(wsdlOperationMessage);
            OperationDescription operationDescription = this.contractContext.GetOperationDescription(wsdlOperationMessage.Operation);
            System.Web.Services.Description.Message wsdlMessage = wsdlOperationMessage.Operation.PortType.ServiceDescription.ServiceDescriptions.GetMessage(wsdlOperationMessage.Message);
            if (wsdlMessage.Parts.Count == 1)
            {
                if (this.TryImportAnyMessage(wsdlMessage.Parts[0], messageDescription, isReply))
                {
                    return;
                }
                if (this.TryImportStream(wsdlMessage.Parts[0], messageDescription, isReply, areAllMessagesWrapped))
                {
                    return;
                }
                if (areAllMessagesWrapped && this.TryImportWrappedMessage(messageDescription, operationDescription.Messages[0], wsdlMessage, isReply))
                {
                    return;
                }
            }
            MessagePartCollection parts = wsdlMessage.Parts;
            this.BodyPartsTable.TryGetValue(wsdlMessage, out list);
            string[] parameterOrder = wsdlOperationMessage.Operation.ParameterOrder;
            foreach (MessagePart part in parts)
            {
                if (ValidWsdl.Check(part, wsdlMessage, new WsdlWarningHandler(this.AddWarning)) && ((list == null) || list.Contains(part.Name)))
                {
                    bool flag = false;
                    if ((parameterOrder != null) && isReply)
                    {
                        flag = Array.IndexOf<string>(parameterOrder, part.Name) == -1;
                    }
                    MessagePartDescription item = this.CurrentSchemaImporter.ImportMessagePart(part, false, isEncoded);
                    if (flag && (messageDescription.Body.ReturnValue == null))
                    {
                        messageDescription.Body.ReturnValue = item;
                    }
                    else
                    {
                        messageDescription.Body.Parts.Add(item);
                    }
                }
            }
            if ((isReply && (messageDescription.Body.ReturnValue == null)) && ((messageDescription.Body.Parts.Count > 0) && !this.CheckIsRef(operationDescription.Messages[0], messageDescription.Body.Parts[0])))
            {
                messageDescription.Body.ReturnValue = messageDescription.Body.Parts[0];
                messageDescription.Body.Parts.RemoveAt(0);
            }
        }

        internal static void ImportMessageBinding(WsdlImporter importer, WsdlEndpointConversionContext endpointContext, System.Type schemaImporterType)
        {
            bool flag = IsReferencedContract(importer, endpointContext);
            MarkSoapExtensionsAsHandled(endpointContext.WsdlBinding);
            foreach (OperationBinding binding in endpointContext.WsdlBinding.Operations)
            {
                OperationDescription operationDescription = endpointContext.GetOperationDescription(binding);
                if (flag || OperationHasBeenHandled(operationDescription))
                {
                    MarkSoapExtensionsAsHandled(binding);
                    if (binding.Input != null)
                    {
                        MarkSoapExtensionsAsHandled(binding.Input);
                    }
                    if (binding.Output != null)
                    {
                        MarkSoapExtensionsAsHandled(binding.Output);
                    }
                    foreach (MessageBinding binding2 in binding.Faults)
                    {
                        MarkSoapExtensionsAsHandled(binding2);
                    }
                }
            }
        }

        private void ImportMessageBinding(MessageBinding messageBinding, System.Web.Services.Description.Message wsdlMessage, MessageDescription description, OperationFormatStyle style, bool isEncoded)
        {
            this.contractContext.GetOperationMessage(description);
            foreach (object obj2 in messageBinding.Extensions)
            {
                SoapHeaderBinding headerBinding = obj2 as SoapHeaderBinding;
                if (headerBinding != null)
                {
                    this.ImportMessageHeaderBinding(headerBinding, wsdlMessage, description, style, isEncoded, messageBinding.OperationBinding.Name);
                }
                else
                {
                    SoapBodyBinding bodyBinding = obj2 as SoapBodyBinding;
                    if (bodyBinding != null)
                    {
                        this.ImportMessageBodyBinding(bodyBinding, wsdlMessage, description, style, isEncoded, messageBinding.OperationBinding.Name);
                    }
                }
            }
        }

        private void ImportMessageBodyBinding(SoapBodyBinding bodyBinding, System.Web.Services.Description.Message wsdlMessage, MessageDescription description, OperationFormatStyle style, bool isEncoded, string operationName)
        {
            if ((style == OperationFormatStyle.Rpc) && (bodyBinding.Namespace != null))
            {
                description.Body.WrapperNamespace = bodyBinding.Namespace;
            }
            this.CurrentSchemaImporter.ValidateStyleAndUse(style, isEncoded, operationName);
        }

        private void ImportMessageContract()
        {
            if (this.contractContext.Contract.Operations.Count > 0)
            {
                this.CurrentSchemaImporter.PreprocessSchema();
                bool used = true;
                OperationInfo[] infoArray = new OperationInfo[this.contractContext.Contract.Operations.Count];
                int num = 0;
                foreach (OperationDescription description in this.contractContext.Contract.Operations)
                {
                    OperationInfo info;
                    if (!this.CanImportOperation(description, out info))
                    {
                        this.TraceImportInformation(description);
                        used = false;
                        break;
                    }
                    infoArray[num++] = info;
                }
                if (used)
                {
                    num = 0;
                    foreach (OperationDescription description2 in this.contractContext.Contract.Operations)
                    {
                        this.ImportOperationContract(description2, infoArray[num++]);
                    }
                }
                this.CurrentSchemaImporter.PostprocessSchema(used);
            }
        }

        internal static void ImportMessageContract(WsdlImporter importer, WsdlContractConversionContext contractContext, SchemaImporter schemaImporter)
        {
            new MessageContractImporter(importer, contractContext, schemaImporter).ImportMessageContract();
        }

        private void ImportMessageHeaderBinding(SoapHeaderBinding headerBinding, System.Web.Services.Description.Message wsdlMessage, MessageDescription description, OperationFormatStyle style, bool isEncoded, string operationName)
        {
            MessagePart part = FindPartByName(wsdlMessage.ServiceDescription.ServiceDescriptions.GetMessage(headerBinding.Message), headerBinding.Part);
            if (!description.Headers.Contains(this.CurrentSchemaImporter.GetPartName(part)))
            {
                description.Headers.Add((MessageHeaderDescription) this.schemaImporter.ImportMessagePart(part, true, isEncoded));
                this.CurrentSchemaImporter.ValidateStyleAndUse(style, isEncoded, operationName);
            }
        }

        private void ImportOperationContract(OperationDescription operation, OperationInfo operationInfo)
        {
            Operation operation2 = this.contractContext.GetOperation(operation);
            Collection<OperationBinding> operationBindings = this.contractContext.GetOperationBindings(operation2);
            bool isReply = false;
            foreach (OperationMessage message in operation2.Messages)
            {
                this.ImportMessage(message, isReply, operationInfo.IsEncoded, operationInfo.AreAllMessagesWrapped);
                isReply = true;
            }
            if (operationInfo.Style == OperationFormatStyle.Rpc)
            {
                SetWrapperName(operation);
            }
            this.CurrentSchemaImporter.SetOperationStyle(operation, operationInfo.Style);
            this.CurrentSchemaImporter.SetOperationIsEncoded(operation, operationInfo.IsEncoded);
            this.CurrentSchemaImporter.SetOperationSupportFaults(operation, this.faultImportOptions.UseMessageFormat);
            this.ImportFaults(operation2, operation, operationInfo.IsEncoded);
            foreach (OperationBinding binding in operationBindings)
            {
                foreach (MessageDescription description in operation.Messages)
                {
                    OperationMessage operationMessage = this.contractContext.GetOperationMessage(description);
                    System.Web.Services.Description.Message wsdlMessage = operationMessage.Operation.PortType.ServiceDescription.ServiceDescriptions.GetMessage(operationMessage.Message);
                    MessageBinding messageBinding = (description.Direction == MessageDirection.Input) ? ((MessageBinding) binding.Input) : ((MessageBinding) binding.Output);
                    if (messageBinding != null)
                    {
                        this.ImportMessageBinding(messageBinding, wsdlMessage, description, operationInfo.Style, operationInfo.IsEncoded);
                    }
                }
            }
            operation.Behaviors.Add(this.CurrentSchemaImporter.GetOperationGenerator());
        }

        private static bool IsNullOrEmpty(XmlQualifiedName qname)
        {
            if (qname != null)
            {
                return qname.IsEmpty;
            }
            return true;
        }

        private static bool IsReferencedContract(WsdlImporter importer, WsdlEndpointConversionContext endpointContext)
        {
            return importer.KnownContracts.ContainsValue(endpointContext.Endpoint.Contract);
        }

        private static bool IsSoapBindingExtension(ServiceDescriptionFormatExtension ext)
        {
            return ((((ext is SoapBinding) || (ext is SoapBodyBinding)) || ((ext is SoapHeaderBinding) || (ext is SoapOperationBinding))) || ((ext is SoapFaultBinding) || (ext is SoapHeaderFaultBinding)));
        }

        private bool IsWrapperPart(MessagePart wsdlPart)
        {
            bool wrappedFlag = false;
            object obj2 = null;
            if (this.importer.State.TryGetValue(typeof(WrappedOptions), out obj2))
            {
                wrappedFlag = ((WrappedOptions) obj2).WrappedFlag;
            }
            return (((wsdlPart.Name == "parameters") && !IsNullOrEmpty(wsdlPart.Element)) && !wrappedFlag);
        }

        private static void MarkSoapExtensionsAsHandled(NamedItem item)
        {
            foreach (object obj2 in item.Extensions)
            {
                ServiceDescriptionFormatExtension ext = obj2 as ServiceDescriptionFormatExtension;
                if ((ext != null) && IsSoapBindingExtension(ext))
                {
                    ext.Handled = true;
                }
                else if (SoapHelper.IsSoapFaultBinding(obj2 as XmlElement))
                {
                    ext.Handled = true;
                }
            }
        }

        private static bool OperationHasBeenHandled(OperationDescription operation)
        {
            return (operation.Behaviors.Find<IOperationContractGenerationExtension>() != null);
        }

        private static void SetWrapperName(OperationDescription operation)
        {
            MessageDescriptionCollection messages = operation.Messages;
            if ((messages != null) && (messages.Count > 0))
            {
                MessageDescription description = messages[0];
                if (description != null)
                {
                    description.Body.WrapperName = operation.Name;
                    description.Body.WrapperNamespace = operation.DeclaringContract.Namespace;
                }
                if (messages.Count > 1)
                {
                    MessageDescription description2 = messages[1];
                    if (description2 != null)
                    {
                        description2.Body.WrapperName = TypeLoader.GetBodyWrapperResponseName(operation.Name).EncodedName;
                        description2.Body.WrapperNamespace = operation.DeclaringContract.Namespace;
                    }
                }
            }
        }

        private void TraceFaultCannotBeImported(string faultName, string operationName, string message)
        {
            this.AddWarning(System.ServiceModel.SR.GetString("SFxFaultCannotBeImported", new object[] { faultName, operationName, message }));
        }

        private void TraceImportInformation(OperationDescription operation)
        {
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, string> dictionary2 = new Dictionary<string, string>(2);
                dictionary2.Add("Operation", operation.Name);
                dictionary2.Add("Format", this.CurrentSchemaImporter.GetFormatName());
                Dictionary<string, string> dictionary = dictionary2;
                TraceUtility.TraceEvent(TraceEventType.Information, 0x80042, System.ServiceModel.SR.GetString("TraceCodeCannotBeImportedInCurrentFormat"), new DictionaryTraceRecord(dictionary), null, null);
            }
        }

        private bool TryImportAnyMessage(MessagePart part, MessageDescription description, bool isReply)
        {
            return CheckAndAddPart(part.Type, DataContractSerializerMessageContractImporter.GenericMessageTypeName, part.Name, string.Empty, typeof(System.ServiceModel.Channels.Message), description, isReply);
        }

        private bool TryImportStream(MessagePart part, MessageDescription description, bool isReply, bool areAllMessagesWrapped)
        {
            string ns = string.Empty;
            if (areAllMessagesWrapped && this.IsWrapperPart(part))
            {
                XmlSchemaForm form;
                XmlSchemaSequence rootSequence = GetRootSequence(GetElementComplexType(part.Element, this.allSchemas, out ns, out form));
                if (((rootSequence != null) && (rootSequence.Items.Count == 1)) && (rootSequence.Items[0] is XmlSchemaElement))
                {
                    XmlSchemaElement element = (XmlSchemaElement) rootSequence.Items[0];
                    description.Body.WrapperName = new System.ServiceModel.Description.XmlName(part.Element.Name, true).EncodedName;
                    description.Body.WrapperNamespace = part.Element.Namespace;
                    return CheckAndAddPart(element.SchemaTypeName, DataContractSerializerMessageContractImporter.StreamBodyTypeName, element.Name, GetLocalElementNamespace(ns, element, form), typeof(Stream), description, isReply);
                }
                return false;
            }
            XmlQualifiedName type = part.Type;
            if (IsNullOrEmpty(type))
            {
                if (IsNullOrEmpty(part.Element))
                {
                    return false;
                }
                ns = part.Element.Namespace;
                type = GetTypeName(FindSchemaElement(this.allSchemas, part.Element));
            }
            return CheckAndAddPart(type, DataContractSerializerMessageContractImporter.StreamBodyTypeName, part.Name, ns, typeof(Stream), description, isReply);
        }

        private bool TryImportWrappedMessage(MessageDescription messageDescription, MessageDescription requestMessage, System.Web.Services.Description.Message wsdlMessage, bool isReply)
        {
            MessagePart wsdlPart = wsdlMessage.Parts[0];
            if (!this.CanImportWrappedMessage(wsdlPart))
            {
                return false;
            }
            XmlQualifiedName element = wsdlPart.Element;
            MessagePartDescription[] descriptionArray = this.CurrentSchemaImporter.ImportWrapperElement(element);
            if (descriptionArray == null)
            {
                return false;
            }
            messageDescription.Body.WrapperName = new System.ServiceModel.Description.XmlName(element.Name, true).EncodedName;
            messageDescription.Body.WrapperNamespace = element.Namespace;
            if (descriptionArray.Length > 0)
            {
                int index = 0;
                if ((isReply && (messageDescription.Body.ReturnValue == null)) && !this.CheckIsRef(requestMessage, descriptionArray[0]))
                {
                    messageDescription.Body.ReturnValue = descriptionArray[0];
                    index = 1;
                }
                while (index < descriptionArray.Length)
                {
                    MessagePartDescription item = descriptionArray[index];
                    messageDescription.Body.Parts.Add(item);
                    index++;
                }
            }
            return true;
        }

        private bool ValidateFault(OperationFault fault, OperationDescription description, out XmlSchemaElement detailElement, out XmlQualifiedName detailElementTypeName, out XmlQualifiedName detailElementQname)
        {
            detailElement = null;
            detailElementTypeName = null;
            detailElementQname = null;
            ServiceDescriptionCollection serviceDescriptions = fault.Operation.PortType.ServiceDescription.ServiceDescriptions;
            if (fault.Message.IsEmpty)
            {
                this.TraceFaultCannotBeImported(fault.Name, description.Name, System.ServiceModel.SR.GetString("SFxWsdlOperationFaultNeedsMessageAttribute2", new object[] { fault.Name, fault.Operation.PortType.Name }));
                description.Faults.Remove(this.contractContext.GetFaultDescription(fault));
                return false;
            }
            System.Web.Services.Description.Message message = serviceDescriptions.GetMessage(fault.Message);
            if (message.Parts.Count != 1)
            {
                this.TraceFaultCannotBeImported(fault.Name, description.Name, System.ServiceModel.SR.GetString("UnsupportedWSDLOnlyOneMessage"));
                description.Faults.Remove(this.contractContext.GetFaultDescription(fault));
                return false;
            }
            MessagePart part = message.Parts[0];
            detailElementQname = part.Element;
            if (IsNullOrEmpty(detailElementQname) || !IsNullOrEmpty(part.Type))
            {
                this.TraceFaultCannotBeImported(fault.Name, description.Name, System.ServiceModel.SR.GetString("UnsupportedWSDLTheFault"));
                description.Faults.Remove(this.contractContext.GetFaultDescription(fault));
                return false;
            }
            detailElement = FindSchemaElement(this.AllSchemas, detailElementQname);
            detailElementTypeName = GetTypeName(detailElement);
            return true;
        }

        private XmlSchemaSet AllSchemas
        {
            get
            {
                return this.allSchemas;
            }
        }

        private Dictionary<System.Web.Services.Description.Message, IList<string>> BodyPartsTable
        {
            get
            {
                if (this.bodyPartsTable == null)
                {
                    this.bodyPartsTable = new Dictionary<System.Web.Services.Description.Message, IList<string>>();
                }
                return this.bodyPartsTable;
            }
        }

        private SchemaImporter CurrentSchemaImporter
        {
            get
            {
                return this.schemaImporter;
            }
        }

        internal class DataContractSerializerSchemaImporter : MessageContractImporter.SchemaImporter
        {
            private ValidationEventHandler compileValidationEventHandler;
            private System.ServiceModel.Description.DataContractSerializerOperationGenerator DataContractSerializerOperationGenerator;
            private Collection<MetadataConversionError> errors;

            public DataContractSerializerSchemaImporter(WsdlImporter importer) : base(importer)
            {
                this.DataContractSerializerOperationGenerator = new System.ServiceModel.Description.DataContractSerializerOperationGenerator(this.DataContractImporter.CodeCompileUnit);
            }

            internal override bool CanImportElement(XmlSchemaElement element)
            {
                if (!element.IsNillable && !System.ServiceModel.Description.SchemaHelper.IsElementValueType(element))
                {
                    return false;
                }
                return this.DataContractImporter.CanImport(base.schemaSet, element);
            }

            internal override bool CanImportFault(XmlSchemaElement detailElement, XmlQualifiedName detailElementTypeName)
            {
                MessageContractImporter.DataContractSerializerSchemaImporter importer = Get(base.importer);
                if (MessageContractImporter.IsNullOrEmpty(detailElementTypeName))
                {
                    return importer.CanImportFaultElement(detailElement);
                }
                return importer.CanImportFaultType(detailElementTypeName);
            }

            internal bool CanImportFaultElement(XmlSchemaElement element)
            {
                bool flag;
                int oldValue = this.SetImportXmlType(false);
                try
                {
                    flag = this.DataContractImporter.CanImport(base.schemaSet, element);
                }
                finally
                {
                    this.RestoreImportXmlType(oldValue);
                }
                return flag;
            }

            internal bool CanImportFaultType(XmlQualifiedName typeName)
            {
                bool flag;
                int oldValue = this.SetImportXmlType(false);
                try
                {
                    flag = this.DataContractImporter.CanImport(base.schemaSet, typeName);
                }
                finally
                {
                    this.RestoreImportXmlType(oldValue);
                }
                return flag;
            }

            internal override bool CanImportStyleAndUse(OperationFormatStyle style, bool isEncoded)
            {
                return !isEncoded;
            }

            internal override bool CanImportType(XmlQualifiedName typeName)
            {
                return this.DataContractImporter.CanImport(base.schemaSet, typeName);
            }

            internal override bool CanImportWrapperElement(XmlQualifiedName elementName)
            {
                string str;
                XmlSchemaForm form;
                XmlSchemaComplexType type = MessageContractImporter.GetElementComplexType(elementName, base.schemaSet, out str, out form);
                if (type == null)
                {
                    return false;
                }
                if (type.Particle != null)
                {
                    XmlSchemaSequence particle = type.Particle as XmlSchemaSequence;
                    if (particle == null)
                    {
                        return false;
                    }
                    for (int i = 0; i < particle.Items.Count; i++)
                    {
                        XmlSchemaElement element = particle.Items[i] as XmlSchemaElement;
                        if (element == null)
                        {
                            return false;
                        }
                        if (!MessageContractImporter.IsNullOrEmpty(element.RefName))
                        {
                            element = MessageContractImporter.FindSchemaElement(base.schemaSet, element.RefName);
                        }
                        if (element.MaxOccurs > 1M)
                        {
                            return false;
                        }
                        if (!this.DataContractImporter.CanImport(base.schemaSet, element))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            internal static MessageContractImporter.DataContractSerializerSchemaImporter Get(WsdlImporter importer)
            {
                object obj2;
                System.Type key = typeof(MessageContractImporter.DataContractSerializerSchemaImporter);
                if (importer.State.ContainsKey(key))
                {
                    obj2 = importer.State[key];
                }
                else
                {
                    obj2 = new MessageContractImporter.DataContractSerializerSchemaImporter(importer);
                    importer.State.Add(key, obj2);
                }
                return (MessageContractImporter.DataContractSerializerSchemaImporter) obj2;
            }

            internal override string GetFormatName()
            {
                return "DataContract";
            }

            internal override IOperationBehavior GetOperationGenerator()
            {
                return this.DataContractSerializerOperationGenerator;
            }

            internal override bool GetOperationIsEncoded(OperationDescription operation)
            {
                return false;
            }

            internal override string ImportElement(MessagePartDescription part, XmlSchemaElement element, bool isEncoded)
            {
                if (part.Multiple)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxDataContractSerializerDoesNotSupportBareArray", new object[] { part.Name })));
                }
                XmlQualifiedName typeName = this.DataContractImporter.Import(base.schemaSet, element);
                CodeTypeReference codeTypeReference = this.DataContractImporter.GetCodeTypeReference(typeName, element);
                ICollection<CodeTypeReference> knownTypeReferences = this.DataContractImporter.GetKnownTypeReferences(typeName);
                this.DataContractSerializerOperationGenerator.Add(part, codeTypeReference, knownTypeReferences, !element.IsNillable && !this.IsValueType(typeName));
                if (codeTypeReference.ArrayRank == 0)
                {
                    return codeTypeReference.BaseType;
                }
                return (codeTypeReference.BaseType + "[]");
            }

            internal override CodeTypeReference ImportFaultElement(XmlQualifiedName elementName, XmlSchemaElement element, bool isEncoded)
            {
                CodeTypeReference codeTypeReference;
                int oldValue = this.SetImportXmlType(true);
                try
                {
                    XmlQualifiedName typeName = this.DataContractImporter.Import(base.schemaSet, element);
                    codeTypeReference = this.DataContractImporter.GetCodeTypeReference(typeName, element);
                }
                finally
                {
                    this.RestoreImportXmlType(oldValue);
                }
                return codeTypeReference;
            }

            internal override CodeTypeReference ImportFaultType(XmlQualifiedName elementName, XmlQualifiedName typeName, bool isEncoded)
            {
                CodeTypeReference codeTypeReference;
                int oldValue = this.SetImportXmlType(true);
                try
                {
                    this.DataContractImporter.Import(base.schemaSet, typeName);
                    codeTypeReference = this.DataContractImporter.GetCodeTypeReference(typeName);
                }
                finally
                {
                    this.RestoreImportXmlType(oldValue);
                }
                return codeTypeReference;
            }

            internal override string ImportType(MessagePartDescription part, XmlQualifiedName typeName, bool isEncoded)
            {
                if (isEncoded)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxDataContractSerializerDoesNotSupportEncoded", new object[] { part.Name })));
                }
                this.DataContractImporter.Import(base.schemaSet, typeName);
                CodeTypeReference codeTypeReference = this.DataContractImporter.GetCodeTypeReference(typeName);
                ICollection<CodeTypeReference> knownTypeReferences = this.DataContractImporter.GetKnownTypeReferences(typeName);
                this.DataContractSerializerOperationGenerator.Add(part, codeTypeReference, knownTypeReferences, false);
                if (codeTypeReference.ArrayRank == 0)
                {
                    return codeTypeReference.BaseType;
                }
                return (codeTypeReference.BaseType + "[]");
            }

            internal override MessagePartDescription[] ImportWrapperElement(XmlQualifiedName elementName)
            {
                string str;
                XmlSchemaForm form;
                XmlSchemaComplexType type = MessageContractImporter.GetElementComplexType(elementName, base.schemaSet, out str, out form);
                if (type == null)
                {
                    return null;
                }
                if (type.Particle == null)
                {
                    return new MessagePartDescription[0];
                }
                XmlSchemaSequence particle = type.Particle as XmlSchemaSequence;
                if (particle == null)
                {
                    return null;
                }
                MessagePartDescription[] descriptionArray = new MessagePartDescription[particle.Items.Count];
                for (int i = 0; i < particle.Items.Count; i++)
                {
                    XmlSchemaElement element = particle.Items[i] as XmlSchemaElement;
                    if (element == null)
                    {
                        return null;
                    }
                    descriptionArray[i] = base.ImportParameterElement(element, MessageContractImporter.GetLocalElementNamespace(str, element, form), false, false);
                    if (descriptionArray[i] == null)
                    {
                        return null;
                    }
                }
                return descriptionArray;
            }

            private bool IsValueType(XmlQualifiedName typeName)
            {
                XmlSchemaElement element = new XmlSchemaElement {
                    IsNillable = true
                };
                return (this.DataContractImporter.GetCodeTypeReference(typeName, element).BaseType == typeof(Nullable<>).FullName);
            }

            internal override void PostprocessSchema(bool used)
            {
                if (used && (this.errors != null))
                {
                    foreach (MetadataConversionError error in this.errors)
                    {
                        base.importer.Errors.Add(error);
                    }
                    this.errors.Clear();
                }
                base.schemaSet.ValidationEventHandler -= this.compileValidationEventHandler;
            }

            internal override void PreprocessSchema()
            {
                this.errors = new Collection<MetadataConversionError>();
                this.compileValidationEventHandler = (sender, args) => System.ServiceModel.Description.SchemaHelper.HandleSchemaValidationError(sender, args, this.errors);
                base.schemaSet.ValidationEventHandler += this.compileValidationEventHandler;
            }

            private void RestoreImportXmlType(int oldValue)
            {
                if (oldValue != 1)
                {
                    if (oldValue == 0)
                    {
                        this.DataContractImporter.Options.ImportXmlType = !this.DataContractImporter.Options.ImportXmlType;
                    }
                    else
                    {
                        this.DataContractImporter.Options = null;
                    }
                }
            }

            private int SetImportXmlType(bool value)
            {
                if (this.DataContractImporter.Options == null)
                {
                    this.DataContractImporter.Options = new ImportOptions();
                    this.DataContractImporter.Options.ImportXmlType = value;
                    return -1;
                }
                if (this.DataContractImporter.Options.ImportXmlType != value)
                {
                    this.DataContractImporter.Options.ImportXmlType = value;
                    return 0;
                }
                return 1;
            }

            internal override void SetOperationIsEncoded(OperationDescription operation, bool isEncoded)
            {
                if (isEncoded)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxDataContractSerializerDoesNotSupportEncoded", new object[] { operation.Name })));
                }
            }

            internal override void SetOperationStyle(OperationDescription operation, OperationFormatStyle style)
            {
                DataContractSerializerOperationBehavior item = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (item == null)
                {
                    item = new DataContractSerializerOperationBehavior(operation, new DataContractFormatAttribute());
                    operation.Behaviors.Add(item);
                }
                item.DataContractFormatAttribute.Style = style;
            }

            internal override void SetOperationSupportFaults(OperationDescription operation, bool supportFaults)
            {
            }

            internal override void ValidateStyleAndUse(OperationFormatStyle style, bool isEncoded, string operationName)
            {
                if (isEncoded)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxDataContractSerializerDoesNotSupportEncoded", new object[] { operationName })));
                }
            }

            private XsdDataContractImporter DataContractImporter
            {
                get
                {
                    object obj2;
                    if (!base.importer.State.TryGetValue(typeof(XsdDataContractImporter), out obj2))
                    {
                        object obj3;
                        if (!base.importer.State.TryGetValue(typeof(CodeCompileUnit), out obj3))
                        {
                            obj3 = new CodeCompileUnit();
                            base.importer.State.Add(typeof(CodeCompileUnit), obj3);
                        }
                        obj2 = new XsdDataContractImporter((CodeCompileUnit) obj3);
                        base.importer.State.Add(typeof(XsdDataContractImporter), obj2);
                    }
                    return (XsdDataContractImporter) obj2;
                }
            }
        }

        private class OperationInfo
        {
            private bool areAllMessagesWrapped;
            private bool isEncoded;
            private OperationFormatStyle style;

            internal OperationInfo(OperationFormatStyle style, bool isEncoded, bool areAllMessagesWrapped)
            {
                this.style = style;
                this.isEncoded = isEncoded;
                this.areAllMessagesWrapped = areAllMessagesWrapped;
            }

            internal bool AreAllMessagesWrapped
            {
                get
                {
                    return this.areAllMessagesWrapped;
                }
            }

            internal bool IsEncoded
            {
                get
                {
                    return this.isEncoded;
                }
            }

            internal OperationFormatStyle Style
            {
                get
                {
                    return this.style;
                }
            }
        }

        internal abstract class SchemaImporter
        {
            protected readonly WsdlImporter importer;
            protected readonly XmlSchemaSet schemaSet;

            internal SchemaImporter(WsdlImporter importer)
            {
                this.importer = importer;
                this.schemaSet = MessageContractImporter.GatherSchemas(importer);
            }

            internal abstract bool CanImportElement(XmlSchemaElement element);
            internal virtual bool CanImportFault(XmlSchemaElement detailElement, XmlQualifiedName detailElementTypeName)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            internal bool CanImportMessagePart(MessagePart part, out OperationFormatStyle style)
            {
                style = OperationFormatStyle.Document;
                if (!MessageContractImporter.IsNullOrEmpty(part.Element))
                {
                    return this.CanImportElement(MessageContractImporter.FindSchemaElement(this.schemaSet, part.Element));
                }
                if (!MessageContractImporter.IsNullOrEmpty(part.Type))
                {
                    style = OperationFormatStyle.Rpc;
                    return this.CanImportType(part.Type);
                }
                return false;
            }

            internal abstract bool CanImportStyleAndUse(OperationFormatStyle style, bool isEncoded);
            internal abstract bool CanImportType(XmlQualifiedName typeName);
            internal abstract bool CanImportWrapperElement(XmlQualifiedName elementName);
            internal abstract string GetFormatName();
            internal abstract IOperationBehavior GetOperationGenerator();
            internal abstract bool GetOperationIsEncoded(OperationDescription operation);
            internal XmlQualifiedName GetPartName(MessagePart part)
            {
                if (!MessageContractImporter.IsNullOrEmpty(part.Element))
                {
                    return part.Element;
                }
                if (MessageContractImporter.IsNullOrEmpty(part.Type))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxWsdlPartMustHaveElementOrType", new object[] { part.Name, part.Message.Name, part.Message.Namespaces })));
                }
                return new XmlQualifiedName(part.Name, string.Empty);
            }

            internal abstract string ImportElement(MessagePartDescription part, XmlSchemaElement element, bool isEncoded);
            internal virtual CodeTypeReference ImportFaultElement(XmlQualifiedName elementName, XmlSchemaElement element, bool isEncoded)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            internal virtual CodeTypeReference ImportFaultType(XmlQualifiedName elementName, XmlQualifiedName typeName, bool isEncoded)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            internal MessagePartDescription ImportMessagePart(MessagePart part, bool isHeader, bool isEncoded)
            {
                MessagePartDescription description = null;
                if (!MessageContractImporter.IsNullOrEmpty(part.Element))
                {
                    return this.ImportParameterElement(part.Element, isHeader, false);
                }
                if (MessageContractImporter.IsNullOrEmpty(part.Type))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxWsdlPartMustHaveElementOrType", new object[] { part.Name, part.Message.Name, part.Message.Namespaces })));
                }
                description = isHeader ? new MessageHeaderDescription(part.Name, string.Empty) : new MessagePartDescription(part.Name, string.Empty);
                description.BaseType = this.ImportType(description, part.Type, isEncoded);
                return description;
            }

            internal MessagePartDescription ImportParameterElement(XmlQualifiedName elementName, bool isHeader, bool isMultiple)
            {
                return this.ImportParameterElement(MessageContractImporter.FindSchemaElement(this.schemaSet, elementName), elementName.Namespace, isHeader, isMultiple);
            }

            internal MessagePartDescription ImportParameterElement(XmlSchemaElement element, string ns, bool isHeader, bool isMultiple)
            {
                if (element.MaxOccurs > 1M)
                {
                    isMultiple = true;
                }
                if (!MessageContractImporter.IsNullOrEmpty(element.RefName))
                {
                    return this.ImportParameterElement(element.RefName, isHeader, isMultiple);
                }
                MessagePartDescription part = isHeader ? new MessageHeaderDescription(element.Name, ns) : new MessagePartDescription(element.Name, ns);
                part.Multiple = isMultiple;
                part.BaseType = this.ImportElement(part, element, false);
                return part;
            }

            internal abstract string ImportType(MessagePartDescription part, XmlQualifiedName typeName, bool isEncoded);
            internal abstract MessagePartDescription[] ImportWrapperElement(XmlQualifiedName elementName);
            internal abstract void PostprocessSchema(bool used);
            internal abstract void PreprocessSchema();
            internal abstract void SetOperationIsEncoded(OperationDescription operation, bool isEncoded);
            internal abstract void SetOperationStyle(OperationDescription operation, OperationFormatStyle style);
            internal virtual void SetOperationSupportFaults(OperationDescription operation, bool supportFaults)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            internal abstract void ValidateStyleAndUse(OperationFormatStyle style, bool isEncoded, string operationName);
        }

        private enum StyleAndUse
        {
            DocumentLiteral,
            RpcLiteral,
            RpcEncoded,
            DocumentEncoded
        }

        internal class XmlSerializerSchemaImporter : MessageContractImporter.SchemaImporter
        {
            private CodeDomProvider codeProvider;
            private XmlSchemas encodedSchemas;
            private XmlSchemas literalSchemas;
            private SoapSchemaImporter soapImporter;
            private XmlSchemaImporter xmlImporter;
            private XmlSerializerOperationGenerator xmlSerializerOperationGenerator;

            public XmlSerializerSchemaImporter(WsdlImporter importer) : base(importer)
            {
                XmlSerializerImportOptions options;
                if (importer.State.ContainsKey(typeof(XmlSerializerImportOptions)))
                {
                    options = (XmlSerializerImportOptions) importer.State[typeof(XmlSerializerImportOptions)];
                }
                else
                {
                    object obj2;
                    if (!importer.State.TryGetValue(typeof(CodeCompileUnit), out obj2))
                    {
                        obj2 = new CodeCompileUnit();
                        importer.State.Add(typeof(CodeCompileUnit), obj2);
                    }
                    options = new XmlSerializerImportOptions((CodeCompileUnit) obj2);
                    importer.State.Add(typeof(XmlSerializerImportOptions), options);
                }
                WebReferenceOptions webReferenceOptions = options.WebReferenceOptions;
                this.codeProvider = options.CodeProvider;
                this.encodedSchemas = new XmlSchemas();
                this.literalSchemas = new XmlSchemas();
                MessageContractImporter.CollectEncodedAndLiteralSchemas(importer.WsdlDocuments, this.encodedSchemas, this.literalSchemas, base.schemaSet);
                CodeIdentifiers identifiers = new CodeIdentifiers();
                lock (MessageContractImporter.schemaImporterLock)
                {
                    this.xmlImporter = new XmlSchemaImporter(this.literalSchemas, webReferenceOptions.CodeGenerationOptions, options.CodeProvider, new System.Xml.Serialization.ImportContext(identifiers, false));
                }
                if (webReferenceOptions != null)
                {
                    foreach (string str in webReferenceOptions.SchemaImporterExtensions)
                    {
                        this.xmlImporter.Extensions.Add(str, System.Type.GetType(str, true));
                    }
                }
                lock (MessageContractImporter.schemaImporterLock)
                {
                    this.soapImporter = new SoapSchemaImporter(this.encodedSchemas, webReferenceOptions.CodeGenerationOptions, options.CodeProvider, new System.Xml.Serialization.ImportContext(identifiers, false));
                }
                this.xmlSerializerOperationGenerator = new XmlSerializerOperationGenerator(options);
            }

            private string AddPartType(MessagePartDescription part, XmlMembersMapping membersMapping, bool isEncoded)
            {
                this.xmlSerializerOperationGenerator.Add(part, membersMapping[0], membersMapping, isEncoded);
                return membersMapping[0].GenerateTypeName(this.codeProvider);
            }

            internal override bool CanImportElement(XmlSchemaElement element)
            {
                return true;
            }

            internal override bool CanImportFault(XmlSchemaElement detailElement, XmlQualifiedName detailElementTypeName)
            {
                return true;
            }

            internal override bool CanImportStyleAndUse(OperationFormatStyle style, bool isEncoded)
            {
                return true;
            }

            internal override bool CanImportType(XmlQualifiedName typeName)
            {
                return true;
            }

            internal override bool CanImportWrapperElement(XmlQualifiedName elementName)
            {
                string str;
                XmlSchemaForm form;
                if (MessageContractImporter.GetElementComplexType(elementName, base.schemaSet, out str, out form) == null)
                {
                    return false;
                }
                return true;
            }

            internal static MessageContractImporter.XmlSerializerSchemaImporter Get(WsdlImporter importer)
            {
                object obj2;
                System.Type key = typeof(MessageContractImporter.XmlSerializerSchemaImporter);
                if (importer.State.ContainsKey(key))
                {
                    obj2 = importer.State[key];
                }
                else
                {
                    obj2 = new MessageContractImporter.XmlSerializerSchemaImporter(importer);
                    importer.State.Add(key, obj2);
                }
                return (MessageContractImporter.XmlSerializerSchemaImporter) obj2;
            }

            internal static XmlSerializerFormatAttribute GetFormatAttribute(OperationDescription operation, bool createNew)
            {
                XmlSerializerOperationBehavior item = operation.Behaviors.Find<XmlSerializerOperationBehavior>();
                if (item == null)
                {
                    if (!createNew)
                    {
                        return null;
                    }
                    item = new XmlSerializerOperationBehavior(operation);
                    operation.Behaviors.Add(item);
                }
                return item.XmlSerializerFormatAttribute;
            }

            internal override string GetFormatName()
            {
                return "XmlSerializer";
            }

            internal override IOperationBehavior GetOperationGenerator()
            {
                return this.xmlSerializerOperationGenerator;
            }

            internal override bool GetOperationIsEncoded(OperationDescription operation)
            {
                XmlSerializerFormatAttribute formatAttribute = GetFormatAttribute(operation, false);
                if (formatAttribute == null)
                {
                    return TypeLoader.DefaultXmlSerializerFormatAttribute.IsEncoded;
                }
                return formatAttribute.IsEncoded;
            }

            internal override string ImportElement(MessagePartDescription part, XmlSchemaElement element, bool isEncoded)
            {
                if (isEncoded)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxDocEncodedNotSupported", new object[] { part.Name })));
                }
                XmlMembersMapping membersMapping = this.xmlImporter.ImportMembersMapping(new XmlQualifiedName[] { element.QualifiedName });
                return this.AddPartType(part, membersMapping, isEncoded);
            }

            internal override CodeTypeReference ImportFaultElement(XmlQualifiedName elementName, XmlSchemaElement element, bool isEncoded)
            {
                if (isEncoded)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxDocEncodedFaultNotSupported")));
                }
                XmlMembersMapping xmlMembersMapping = this.xmlImporter.ImportMembersMapping(new XmlQualifiedName[] { elementName });
                this.xmlSerializerOperationGenerator.XmlExporter.ExportMembersMapping(xmlMembersMapping);
                return new CodeTypeReference(this.xmlSerializerOperationGenerator.GetTypeName(xmlMembersMapping[0]));
            }

            internal override CodeTypeReference ImportFaultType(XmlQualifiedName elementName, XmlQualifiedName typeName, bool isEncoded)
            {
                XmlMembersMapping mapping;
                System.ServiceModel.Description.XmlName name = new System.ServiceModel.Description.XmlName(elementName.Name, true);
                string ns = elementName.Namespace;
                SoapSchemaMember member = new SoapSchemaMember {
                    MemberName = name.EncodedName,
                    MemberType = typeName
                };
                if (isEncoded)
                {
                    mapping = this.soapImporter.ImportMembersMapping(name.DecodedName, ns, new SoapSchemaMember[] { member });
                    this.xmlSerializerOperationGenerator.SoapExporter.ExportMembersMapping(mapping);
                }
                else
                {
                    mapping = this.xmlImporter.ImportMembersMapping(name.DecodedName, ns, new SoapSchemaMember[] { member });
                    this.xmlSerializerOperationGenerator.XmlExporter.ExportMembersMapping(mapping);
                }
                return new CodeTypeReference(this.xmlSerializerOperationGenerator.GetTypeName(mapping[0]));
            }

            internal override string ImportType(MessagePartDescription part, XmlQualifiedName typeName, bool isEncoded)
            {
                XmlMembersMapping mapping;
                System.ServiceModel.Description.XmlName name = new System.ServiceModel.Description.XmlName(part.Name, true);
                string ns = part.Namespace;
                SoapSchemaMember member = new SoapSchemaMember {
                    MemberName = name.EncodedName,
                    MemberType = typeName
                };
                if (isEncoded)
                {
                    mapping = this.soapImporter.ImportMembersMapping(name.DecodedName, ns, new SoapSchemaMember[] { member });
                }
                else
                {
                    mapping = this.xmlImporter.ImportMembersMapping(name.DecodedName, ns, new SoapSchemaMember[] { member });
                }
                return this.AddPartType(part, mapping, isEncoded);
            }

            internal override MessagePartDescription[] ImportWrapperElement(XmlQualifiedName elementName)
            {
                XmlMembersMapping membersMapping = this.xmlImporter.ImportMembersMapping(elementName);
                ArrayList list = new ArrayList();
                for (int i = 0; i < membersMapping.Count; i++)
                {
                    XmlMemberMapping memberMapping = membersMapping[i];
                    MessagePartDescription part = new MessagePartDescription(NamingHelper.XmlName(memberMapping.MemberName), (memberMapping.Namespace == null) ? string.Empty : memberMapping.Namespace);
                    this.xmlSerializerOperationGenerator.Add(part, memberMapping, membersMapping, false);
                    part.BaseType = memberMapping.GenerateTypeName(this.codeProvider);
                    list.Add(part);
                }
                return (MessagePartDescription[]) list.ToArray(typeof(MessagePartDescription));
            }

            internal override void PostprocessSchema(bool used)
            {
            }

            internal override void PreprocessSchema()
            {
                System.Xml.Schema.XmlSchema schema = StockSchemas.CreateWsdl();
                System.Xml.Schema.XmlSchema schema2 = StockSchemas.CreateSoap();
                System.Xml.Schema.XmlSchema schema3 = StockSchemas.CreateSoapEncoding();
                System.Xml.Schema.XmlSchema schema4 = StockSchemas.CreateFakeXsdSchema();
                System.Xml.Schema.XmlSchema schema5 = StockSchemas.CreateFakeXmlSchema();
                base.schemaSet.Add(schema);
                base.schemaSet.Add(schema2);
                base.schemaSet.Add(schema3);
                base.schemaSet.Add(schema4);
                base.schemaSet.Add(schema5);
                System.ServiceModel.Description.SchemaHelper.Compile(base.schemaSet, base.importer.Errors);
                base.schemaSet.Remove(schema);
                base.schemaSet.Remove(schema2);
                base.schemaSet.Remove(schema3);
                base.schemaSet.Remove(schema4);
                base.schemaSet.Remove(schema5);
            }

            internal override void SetOperationIsEncoded(OperationDescription operation, bool isEncoded)
            {
                GetFormatAttribute(operation, true).IsEncoded = isEncoded;
            }

            internal override void SetOperationStyle(OperationDescription operation, OperationFormatStyle style)
            {
                GetFormatAttribute(operation, true).Style = style;
            }

            internal override void SetOperationSupportFaults(OperationDescription operation, bool supportFaults)
            {
                GetFormatAttribute(operation, true).SupportFaults = supportFaults;
            }

            internal override void ValidateStyleAndUse(OperationFormatStyle style, bool isEncoded, string operationName)
            {
                if (isEncoded && (style != OperationFormatStyle.Rpc))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxDocEncodedNotSupported", new object[] { operationName })));
                }
            }
        }
    }
}

