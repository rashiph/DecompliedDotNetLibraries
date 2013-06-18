namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Xml.Serialization;

    internal class XmlSerializerOperationGenerator : IOperationBehavior, IOperationContractGenerationExtension
    {
        private CodeNamespace codeNamespace;
        private static object contractMarker = new object();
        private Dictionary<OperationDescription, XmlSerializerFormatAttribute> operationAttributes = new Dictionary<OperationDescription, XmlSerializerFormatAttribute>();
        private System.ServiceModel.Description.OperationGenerator operationGenerator = new System.ServiceModel.Description.OperationGenerator();
        private XmlSerializerImportOptions options;
        private Dictionary<MessagePartDescription, PartInfo> partInfoTable;
        private SoapCodeExporter soapExporter;
        private XmlCodeExporter xmlExporter;

        internal XmlSerializerOperationGenerator(XmlSerializerImportOptions options)
        {
            this.options = options;
            this.codeNamespace = GetTargetCodeNamespace(options);
            this.partInfoTable = new Dictionary<MessagePartDescription, PartInfo>();
        }

        internal void Add(MessagePartDescription part, XmlMemberMapping memberMapping, XmlMembersMapping membersMapping, bool isEncoded)
        {
            PartInfo info = new PartInfo {
                MemberMapping = memberMapping,
                MembersMapping = membersMapping,
                IsEncoded = isEncoded
            };
            this.partInfoTable[part] = info;
        }

        private void AddKnownTypes(CodeAttributeDeclarationCollection destination, CodeAttributeDeclarationCollection source)
        {
            foreach (CodeAttributeDeclaration declaration in source)
            {
                CodeAttributeDeclaration declaration2 = this.ToKnownType(declaration);
                if (declaration2 != null)
                {
                    destination.Add(declaration2);
                }
            }
        }

        private void GeneratePartType(Dictionary<XmlMembersMapping, XmlMembersMapping> alreadyExported, MessagePartDescription part)
        {
            if (this.partInfoTable.ContainsKey(part))
            {
                PartInfo info = this.partInfoTable[part];
                XmlMembersMapping membersMapping = info.MembersMapping;
                XmlMemberMapping memberMapping = info.MemberMapping;
                if (!alreadyExported.ContainsKey(membersMapping))
                {
                    if (info.IsEncoded)
                    {
                        this.SoapExporter.ExportMembersMapping(membersMapping);
                    }
                    else
                    {
                        this.XmlExporter.ExportMembersMapping(membersMapping);
                    }
                    alreadyExported.Add(membersMapping, membersMapping);
                }
                CodeAttributeDeclarationCollection metadata = new CodeAttributeDeclarationCollection();
                if (info.IsEncoded)
                {
                    this.SoapExporter.AddMappingMetadata(metadata, memberMapping, false);
                }
                else
                {
                    this.XmlExporter.AddMappingMetadata(metadata, memberMapping, part.Namespace, false);
                }
                part.BaseType = this.GetTypeName(memberMapping);
                this.operationGenerator.ParameterTypes.Add(part, new CodeTypeReference(part.BaseType));
                this.operationGenerator.ParameterAttributes.Add(part, metadata);
            }
        }

        private static CodeNamespace GetTargetCodeNamespace(XmlSerializerImportOptions options)
        {
            CodeNamespace namespace2 = null;
            string name = options.ClrNamespace ?? string.Empty;
            foreach (CodeNamespace namespace3 in options.CodeCompileUnit.Namespaces)
            {
                if (namespace3.Name == name)
                {
                    namespace2 = namespace3;
                }
            }
            if (namespace2 == null)
            {
                namespace2 = new CodeNamespace(name);
                options.CodeCompileUnit.Namespaces.Add(namespace2);
            }
            return namespace2;
        }

        internal string GetTypeName(XmlMemberMapping member)
        {
            string str = member.GenerateTypeName(this.options.CodeProvider);
            if ((this.codeNamespace != null) && !string.IsNullOrEmpty(this.codeNamespace.Name))
            {
                foreach (CodeTypeDeclaration declaration in this.codeNamespace.Types)
                {
                    if (declaration.Name == str)
                    {
                        str = this.codeNamespace.Name + "." + str;
                    }
                }
            }
            return str;
        }

        void IOperationBehavior.AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
        {
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription description, ClientOperation proxy)
        {
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch)
        {
        }

        void IOperationBehavior.Validate(OperationDescription description)
        {
        }

        void IOperationContractGenerationExtension.GenerateOperation(OperationContractGenerationContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if ((this.partInfoTable != null) && (this.partInfoTable.Count > 0))
            {
                Dictionary<XmlMembersMapping, XmlMembersMapping> alreadyExported = new Dictionary<XmlMembersMapping, XmlMembersMapping>();
                foreach (MessageDescription description in context.Operation.Messages)
                {
                    foreach (MessageHeaderDescription description2 in description.Headers)
                    {
                        this.GeneratePartType(alreadyExported, description2);
                    }
                    if (OperationFormatter.IsValidReturnValue(description.Body.ReturnValue))
                    {
                        this.GeneratePartType(alreadyExported, description.Body.ReturnValue);
                    }
                    foreach (MessagePartDescription description3 in description.Body.Parts)
                    {
                        this.GeneratePartType(alreadyExported, description3);
                    }
                }
            }
            XmlSerializerOperationBehavior behavior = context.Operation.Behaviors.Find<XmlSerializerOperationBehavior>();
            if (behavior != null)
            {
                XmlSerializerFormatAttribute attribute = (behavior == null) ? new XmlSerializerFormatAttribute() : behavior.XmlSerializerFormatAttribute;
                OperationFormatStyle style = attribute.Style;
                this.operationGenerator.GenerateOperation(context, ref style, attribute.IsEncoded, new WrappedBodyTypeGenerator(context), new Dictionary<MessagePartDescription, ICollection<CodeTypeReference>>());
                context.ServiceContractGenerator.AddReferencedAssembly(typeof(XmlTypeAttribute).Assembly);
                attribute.Style = style;
                context.SyncMethod.CustomAttributes.Add(System.ServiceModel.Description.OperationGenerator.GenerateAttributeDeclaration(context.Contract.ServiceContractGenerator, attribute));
                this.AddKnownTypes(context.SyncMethod.CustomAttributes, attribute.IsEncoded ? this.SoapExporter.IncludeMetadata : this.XmlExporter.IncludeMetadata);
                DataContractSerializerOperationGenerator.UpdateTargetCompileUnit(context, this.options.CodeCompileUnit);
            }
        }

        private CodeAttributeDeclaration ToKnownType(CodeAttributeDeclaration include)
        {
            if (!(include.Name == typeof(SoapIncludeAttribute).FullName) && !(include.Name == typeof(XmlIncludeAttribute).FullName))
            {
                return null;
            }
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(new CodeTypeReference(typeof(ServiceKnownTypeAttribute)));
            foreach (CodeAttributeArgument argument in include.Arguments)
            {
                declaration.Arguments.Add(argument);
            }
            return declaration;
        }

        internal Dictionary<OperationDescription, XmlSerializerFormatAttribute> OperationAttributes
        {
            get
            {
                return this.operationAttributes;
            }
        }

        private System.ServiceModel.Description.OperationGenerator OperationGenerator
        {
            get
            {
                return this.operationGenerator;
            }
        }

        public SoapCodeExporter SoapExporter
        {
            get
            {
                if (this.soapExporter == null)
                {
                    this.soapExporter = new SoapCodeExporter(this.codeNamespace, this.options.CodeCompileUnit, this.options.CodeProvider, this.options.WebReferenceOptions.CodeGenerationOptions, null);
                }
                return this.soapExporter;
            }
        }

        public XmlCodeExporter XmlExporter
        {
            get
            {
                if (this.xmlExporter == null)
                {
                    this.xmlExporter = new XmlCodeExporter(this.codeNamespace, this.options.CodeCompileUnit, this.options.CodeProvider, this.options.WebReferenceOptions.CodeGenerationOptions, null);
                }
                return this.xmlExporter;
            }
        }

        private class PartInfo
        {
            internal bool IsEncoded;
            internal XmlMemberMapping MemberMapping;
            internal XmlMembersMapping MembersMapping;
        }

        internal class WrappedBodyTypeGenerator : IWrappedBodyTypeGenerator
        {
            private OperationContractGenerationContext context;

            public WrappedBodyTypeGenerator(OperationContractGenerationContext context)
            {
                this.context = context;
            }

            public void AddMemberAttributes(XmlName messageName, MessagePartDescription part, CodeAttributeDeclarationCollection importedAttributes, CodeAttributeDeclarationCollection typeAttributes, CodeAttributeDeclarationCollection fieldAttributes)
            {
                if (importedAttributes != null)
                {
                    fieldAttributes.AddRange(importedAttributes);
                }
            }

            public void AddTypeAttributes(string messageName, string typeNS, CodeAttributeDeclarationCollection typeAttributes, bool isEncoded)
            {
                if (!isEncoded)
                {
                    XmlTypeAttribute attribute = new XmlTypeAttribute {
                        Namespace = typeNS
                    };
                    typeAttributes.Add(OperationGenerator.GenerateAttributeDeclaration(this.context.Contract.ServiceContractGenerator, attribute));
                }
            }

            public void ValidateForParameterMode(OperationDescription operation)
            {
            }
        }
    }
}

