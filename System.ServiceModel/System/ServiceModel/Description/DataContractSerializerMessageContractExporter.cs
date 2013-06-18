namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;

    internal class DataContractSerializerMessageContractExporter : MessageContractExporter
    {
        internal DataContractSerializerMessageContractExporter(WsdlExporter exporter, WsdlContractConversionContext context, OperationDescription operation, IOperationBehavior extension) : base(exporter, context, operation, extension)
        {
        }

        protected override void Compile()
        {
            System.Xml.Schema.XmlSchema schema = StockSchemas.CreateWsdl();
            System.Xml.Schema.XmlSchema schema2 = StockSchemas.CreateSoap();
            System.Xml.Schema.XmlSchema schema3 = StockSchemas.CreateSoapEncoding();
            System.Xml.Schema.XmlSchema schema4 = StockSchemas.CreateFakeXsdSchema();
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
            XmlSchemaType type;
            MessageDescription messageDescription = base.operation.Messages[messageIndex];
            Message message = base.ExportedMessages.WsdlMessages[new MessageContractExporter.MessageDescriptionDictionaryKey(base.contractContext.Contract, messageDescription)];
            DataContractFormatAttribute dataContractFormatAttribute = ((DataContractSerializerOperationBehavior) base.extension).DataContractFormatAttribute;
            XmlSchemaSequence wrapperSequence = null;
            bool flag = messageDescription.Body.WrapperName != null;
            if ((dataContractFormatAttribute.Style == OperationFormatStyle.Document) && flag)
            {
                wrapperSequence = base.ExportWrappedPart(message, messageDescription.Body.WrapperName, messageDescription.Body.WrapperNamespace, base.SchemaSet, false);
            }
            if (OperationFormatter.IsValidReturnValue(messageDescription.Body.ReturnValue))
            {
                XmlQualifiedName typeName = base.ExportType(messageDescription.Body.ReturnValue.Type, messageDescription.Body.ReturnValue.Name, base.operation.Name, out type);
                base.ExportMessagePart(message, messageDescription.Body.ReturnValue, typeName, type, true, IsTypeNullable(messageDescription.Body.ReturnValue.Type), false, dataContractFormatAttribute.Style != OperationFormatStyle.Rpc, messageDescription.Body.WrapperNamespace, wrapperSequence, base.SchemaSet);
            }
            foreach (MessagePartDescription description2 in messageDescription.Body.Parts)
            {
                XmlQualifiedName name2 = base.ExportType(description2.Type, description2.Name, base.operation.Name, out type);
                base.ExportMessagePart(message, description2, name2, type, true, IsTypeNullable(description2.Type), false, dataContractFormatAttribute.Style != OperationFormatStyle.Rpc, messageDescription.Body.WrapperNamespace, wrapperSequence, base.SchemaSet);
            }
            if (dataContractFormatAttribute.Style == OperationFormatStyle.Rpc)
            {
                base.AddParameterOrder(messageDescription);
            }
        }

        protected override void ExportHeaders(int messageIndex, object state)
        {
            Message message;
            MessageDescription description = base.operation.Messages[messageIndex];
            if ((description.Headers.Count > 0) && base.CreateHeaderMessage(description, out message))
            {
                foreach (MessageHeaderDescription description2 in description.Headers)
                {
                    if (!description2.IsUnknownHeaderCollection)
                    {
                        XmlSchemaType type;
                        XmlQualifiedName typeName = base.ExportType(description2.Type, description2.Name, base.operation.Name, out type);
                        base.ExportMessagePart(message, description2, typeName, type, true, IsTypeNullable(description2.Type), false, true, null, null, base.SchemaSet);
                    }
                }
            }
        }

        protected override void ExportKnownTypes()
        {
            foreach (Type type in base.operation.KnownTypes)
            {
                base.DataContractExporter.Export(type);
            }
        }

        protected override object GetExtensionData()
        {
            return new ExtensionData(((DataContractSerializerOperationBehavior) base.extension).DataContractFormatAttribute);
        }

        protected override bool IsEncoded()
        {
            return false;
        }

        protected override bool IsRpcStyle()
        {
            return (((DataContractSerializerOperationBehavior) base.extension).DataContractFormatAttribute.Style == OperationFormatStyle.Rpc);
        }

        internal static bool IsTypeNullable(Type type)
        {
            return (!type.IsValueType || (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>))));
        }

        protected override object OnExportMessageContract()
        {
            return null;
        }

        private class ExtensionData
        {
            private DataContractFormatAttribute dcFormatAttr;

            internal ExtensionData(DataContractFormatAttribute dcFormatAttr)
            {
                this.dcFormatAttr = dcFormatAttr;
            }

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(this.dcFormatAttr, obj))
                {
                    return true;
                }
                DataContractSerializerMessageContractExporter.ExtensionData data = obj as DataContractSerializerMessageContractExporter.ExtensionData;
                if (data == null)
                {
                    return false;
                }
                return (this.dcFormatAttr.Style == data.dcFormatAttr.Style);
            }

            public override int GetHashCode()
            {
                return 1;
            }
        }
    }
}

