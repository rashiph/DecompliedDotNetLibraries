namespace System.ServiceModel.Description
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal class XmlSerializerMessageContractExporter : MessageContractExporter
    {
        internal XmlSerializerMessageContractExporter(WsdlExporter exporter, WsdlContractConversionContext context, OperationDescription operation, IOperationBehavior extension) : base(exporter, context, operation, extension)
        {
        }

        protected override void Compile()
        {
            System.Xml.Schema.XmlSchema schema = StockSchemas.CreateWsdl();
            System.Xml.Schema.XmlSchema schema2 = StockSchemas.CreateSoap();
            System.Xml.Schema.XmlSchema schema3 = StockSchemas.CreateSoapEncoding();
            System.Xml.Schema.XmlSchema schema4 = StockSchemas.CreateFakeXsdSchema();
            this.MoveSchemas();
            base.SchemaSet.Add(schema);
            base.SchemaSet.Add(schema2);
            base.SchemaSet.Add(schema3);
            base.SchemaSet.Add(schema4);
            base.Compile();
            base.SchemaSet.Remove(schema);
            base.SchemaSet.Remove(schema2);
            base.SchemaSet.Remove(schema3);
            base.SchemaSet.Remove(schema4);
        }

        protected override void ExportBody(int messageIndex, object state)
        {
            MessageDescription messageDescription = base.operation.Messages[messageIndex];
            string name = base.contractContext.WsdlPortType.Name;
            string targetNamespace = base.contractContext.WsdlPortType.ServiceDescription.TargetNamespace;
            MessageContractExporter.MessageDescriptionDictionaryKey key = new MessageContractExporter.MessageDescriptionDictionaryKey(base.contractContext.Contract, messageDescription);
            Message message = base.ExportedMessages.WsdlMessages[key];
            System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector.OperationReflector reflector = (System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector.OperationReflector) state;
            XmlMembersMapping membersMapping = null;
            if (messageIndex == 0)
            {
                membersMapping = reflector.Request.BodyMapping;
            }
            else
            {
                membersMapping = reflector.Reply.BodyMapping;
            }
            if (membersMapping != null)
            {
                bool isDocWrapped = !reflector.IsRpc && (messageDescription.Body.WrapperName != null);
                this.ExportMembersMapping(membersMapping, message, false, reflector.IsEncoded, reflector.IsRpc, isDocWrapped, false);
                if (reflector.IsRpc)
                {
                    base.AddParameterOrder(base.operation.Messages[messageIndex]);
                    base.ExportedMessages.WrapperNamespaces.Add(key, membersMapping.Namespace);
                }
            }
        }

        private void ExportFault(FaultDescription fault, System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector.OperationReflector operationReflector)
        {
            Message message = new Message {
                Name = base.GetFaultMessageName(fault.Name)
            };
            XmlQualifiedName elementName = this.ExportFaultElement(fault, operationReflector);
            base.contractContext.WsdlPortType.ServiceDescription.Messages.Add(message);
            MessageContractExporter.AddMessagePart(message, "detail", elementName, null);
            OperationFault operationFault = base.contractContext.GetOperationFault(fault);
            WsdlExporter.WSAddressingHelper.AddActionAttribute(fault.Action, operationFault, base.exporter.PolicyVersion);
            operationFault.Message = new XmlQualifiedName(message.Name, message.ServiceDescription.TargetNamespace);
        }

        private XmlQualifiedName ExportFaultElement(FaultDescription fault, System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector.OperationReflector operationReflector)
        {
            XmlQualifiedName name;
            XmlMembersMapping xmlMembersMapping = operationReflector.ImportFaultElement(fault, out name);
            if (operationReflector.IsEncoded)
            {
                this.SoapExporter.ExportMembersMapping(xmlMembersMapping);
                return name;
            }
            this.XmlExporter.ExportMembersMapping(xmlMembersMapping);
            return name;
        }

        protected override void ExportFaults(object state)
        {
            System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector.OperationReflector operationReflector = (System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector.OperationReflector) state;
            if (operationReflector.Attribute.SupportFaults)
            {
                foreach (FaultDescription description in base.operation.Faults)
                {
                    this.ExportFault(description, operationReflector);
                }
                this.Compile();
            }
            else
            {
                base.ExportFaults(state);
            }
        }

        protected override void ExportHeaders(int messageIndex, object state)
        {
            string name = base.contractContext.WsdlPortType.Name;
            string targetNamespace = base.contractContext.WsdlPortType.ServiceDescription.TargetNamespace;
            MessageDescription description = base.operation.Messages[messageIndex];
            if (description.Headers.Count > 0)
            {
                Message message;
                System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector.OperationReflector reflector = (System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector.OperationReflector) state;
                XmlMembersMapping membersMapping = null;
                if (messageIndex == 0)
                {
                    membersMapping = reflector.Request.HeadersMapping;
                }
                else
                {
                    membersMapping = reflector.Reply.HeadersMapping;
                }
                if ((membersMapping != null) && base.CreateHeaderMessage(description, out message))
                {
                    this.ExportMembersMapping(membersMapping, message, false, reflector.IsEncoded, false, false, true);
                }
            }
        }

        protected override void ExportKnownTypes()
        {
        }

        private void ExportMembersMapping(XmlMembersMapping membersMapping, Message message, bool skipSchemaExport, bool isEncoded, bool isRpc, bool isDocWrapped, bool isHeader)
        {
            if (!skipSchemaExport)
            {
                if (isEncoded)
                {
                    this.SoapExporter.ExportMembersMapping(membersMapping);
                }
                else
                {
                    this.XmlExporter.ExportMembersMapping(membersMapping, !isRpc);
                }
            }
            if (isDocWrapped)
            {
                if (isHeader)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Header cannot be Document Wrapped", new object[0])));
                }
                MessageContractExporter.AddMessagePart(message, "parameters", new XmlQualifiedName(membersMapping.XsdElementName, membersMapping.Namespace), XmlQualifiedName.Empty);
            }
            else
            {
                bool flag = !isRpc && !isEncoded;
                for (int i = 0; i < membersMapping.Count; i++)
                {
                    XmlMemberMapping mapping = membersMapping[i];
                    string partName = (isHeader || flag) ? NamingHelper.XmlName(mapping.MemberName) : mapping.XsdElementName;
                    if (flag)
                    {
                        MessageContractExporter.AddMessagePart(message, partName, new XmlQualifiedName(mapping.XsdElementName, mapping.Namespace), XmlQualifiedName.Empty);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(mapping.TypeName))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxAnonymousTypeNotSupported", new object[] { message.Name, partName })));
                        }
                        MessageContractExporter.AddMessagePart(message, partName, XmlQualifiedName.Empty, new XmlQualifiedName(mapping.TypeName, mapping.TypeNamespace));
                    }
                }
            }
        }

        protected override object GetExtensionData()
        {
            return new ExtensionData(((XmlSerializerOperationBehavior) base.extension).XmlSerializerFormatAttribute);
        }

        protected override bool IsEncoded()
        {
            return ((XmlSerializerOperationBehavior) base.extension).XmlSerializerFormatAttribute.IsEncoded;
        }

        protected override bool IsRpcStyle()
        {
            return (((XmlSerializerOperationBehavior) base.extension).XmlSerializerFormatAttribute.Style == OperationFormatStyle.Rpc);
        }

        private void MoveSchemas()
        {
            ValidationEventHandler handler = null;
            XmlSchemas schemas = this.Schemas;
            XmlSchemaSet schemaSet = base.SchemaSet;
            if (schemas != null)
            {
                if (handler == null)
                {
                    handler = (sender, args) => SchemaHelper.HandleSchemaValidationError(sender, args, base.exporter.Errors);
                }
                schemas.Compile(handler, false);
                foreach (System.Xml.Schema.XmlSchema schema in schemas)
                {
                    if (!schemaSet.Contains(schema))
                    {
                        schemaSet.Add(schema);
                        schemaSet.Reprocess(schema);
                    }
                }
            }
        }

        protected override object OnExportMessageContract()
        {
            object obj2 = this.Reflector.ReflectOperation(base.operation);
            if (obj2 == null)
            {
                XmlSerializerOperationBehavior extension = base.extension as XmlSerializerOperationBehavior;
                if (extension != null)
                {
                    obj2 = this.Reflector.ReflectOperation(base.operation, extension.XmlSerializerFormatAttribute);
                }
            }
            return obj2;
        }

        private System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector Reflector
        {
            get
            {
                object obj2;
                if (!base.exporter.State.TryGetValue(typeof(System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector), out obj2))
                {
                    obj2 = new System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector(base.contractContext.Contract.Namespace, base.contractContext.Contract.ContractType);
                    base.exporter.State.Add(typeof(System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector), obj2);
                }
                return (System.ServiceModel.Description.XmlSerializerOperationBehavior.Reflector) obj2;
            }
        }

        private XmlSchemas Schemas
        {
            get
            {
                object obj2;
                if (!base.exporter.State.TryGetValue(typeof(XmlSchemas), out obj2))
                {
                    obj2 = new XmlSchemas();
                    foreach (System.Xml.Schema.XmlSchema schema in base.SchemaSet.Schemas())
                    {
                        if (!((XmlSchemas) obj2).Contains(schema.TargetNamespace))
                        {
                            ((XmlSchemas) obj2).Add(schema);
                        }
                    }
                    base.exporter.State.Add(typeof(XmlSchemas), obj2);
                }
                return (XmlSchemas) obj2;
            }
        }

        private SoapSchemaExporter SoapExporter
        {
            get
            {
                object obj2;
                if (!base.exporter.State.TryGetValue(typeof(SoapSchemaExporter), out obj2))
                {
                    obj2 = new SoapSchemaExporter(this.Schemas);
                    base.exporter.State.Add(typeof(SoapSchemaExporter), obj2);
                }
                return (SoapSchemaExporter) obj2;
            }
        }

        private XmlSchemaExporter XmlExporter
        {
            get
            {
                object obj2;
                if (!base.exporter.State.TryGetValue(typeof(XmlSchemaExporter), out obj2))
                {
                    obj2 = new XmlSchemaExporter(this.Schemas);
                    base.exporter.State.Add(typeof(XmlSchemaExporter), obj2);
                }
                return (XmlSchemaExporter) obj2;
            }
        }

        private class ExtensionData
        {
            private XmlSerializerFormatAttribute xsFormatAttr;

            internal ExtensionData(XmlSerializerFormatAttribute xsFormatAttr)
            {
                this.xsFormatAttr = xsFormatAttr;
            }

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(this.xsFormatAttr, obj))
                {
                    return true;
                }
                XmlSerializerMessageContractExporter.ExtensionData data = obj as XmlSerializerMessageContractExporter.ExtensionData;
                if (data == null)
                {
                    return false;
                }
                return ((this.xsFormatAttr.Style == data.xsFormatAttr.Style) && (this.xsFormatAttr.Use == data.xsFormatAttr.Use));
            }

            public override int GetHashCode()
            {
                return 1;
            }
        }
    }
}

