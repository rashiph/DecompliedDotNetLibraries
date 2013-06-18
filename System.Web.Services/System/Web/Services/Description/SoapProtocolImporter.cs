namespace System.Web.Services.Description
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Design;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.Serialization;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class SoapProtocolImporter : ProtocolImporter
    {
        private Hashtable classHeaders = new Hashtable();
        private ArrayList codeClasses = new ArrayList();
        private SoapExtensionImporter[] extensions;
        private Hashtable headers = new Hashtable();
        private ArrayList propertyNames = new ArrayList();
        private ArrayList propertyValues = new ArrayList();
        private System.Web.Services.Description.SoapBinding soapBinding;
        private SoapCodeExporter soapExporter;
        private SoapSchemaImporter soapImporter;
        private ArrayList soapMembers = new ArrayList();
        private SoapTransportImporter transport;
        private static System.Data.Design.TypedDataSetSchemaImporterExtension typedDataSetSchemaImporterExtension;
        private XmlCodeExporter xmlExporter;
        private XmlSchemaImporter xmlImporter;
        private ArrayList xmlMembers = new ArrayList();

        private void AddMetadataProperty(string name, CodeExpression expr)
        {
            this.propertyNames.Add(name);
            this.propertyValues.Add(expr);
        }

        private void AddMetadataProperty(string name, object value)
        {
            this.AddMetadataProperty(name, (CodeExpression) new CodePrimitiveExpression(value));
        }

        protected override CodeTypeDeclaration BeginClass()
        {
            base.MethodNames.Clear();
            this.soapBinding = (System.Web.Services.Description.SoapBinding) base.Binding.Extensions.Find(typeof(System.Web.Services.Description.SoapBinding));
            this.transport = this.GetTransport(this.soapBinding.Transport);
            Type[] types = new Type[] { typeof(SoapDocumentMethodAttribute), typeof(XmlAttributeAttribute), typeof(WebService), typeof(object), typeof(DebuggerStepThroughAttribute), typeof(DesignerCategoryAttribute) };
            WebCodeGenerator.AddImports(base.CodeNamespace, WebCodeGenerator.GetNamespacesForTypes(types));
            CodeFlags isAbstract = (CodeFlags) 0;
            if (base.Style == ServiceDescriptionImportStyle.Server)
            {
                isAbstract = CodeFlags.IsAbstract;
            }
            else if (base.Style == ServiceDescriptionImportStyle.ServerInterface)
            {
                isAbstract = CodeFlags.IsInterface;
            }
            CodeTypeDeclaration declaration = WebCodeGenerator.CreateClass(base.ClassName, null, new string[0], null, CodeFlags.IsPublic | isAbstract, base.ServiceImporter.CodeGenerator.Supports(GeneratorSupport.PartialTypes));
            declaration.Comments.Add(new CodeCommentStatement(System.Web.Services.Res.GetString("CodeRemarks"), true));
            if (base.Style == ServiceDescriptionImportStyle.Client)
            {
                declaration.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(DebuggerStepThroughAttribute).FullName));
                declaration.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(DesignerCategoryAttribute).FullName, new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression("code")) }));
            }
            else if (base.Style == ServiceDescriptionImportStyle.Server)
            {
                CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration(typeof(WebServiceAttribute).FullName);
                string str = (base.Service != null) ? base.Service.ServiceDescription.TargetNamespace : base.Binding.ServiceDescription.TargetNamespace;
                declaration2.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(str)));
                declaration.CustomAttributes.Add(declaration2);
            }
            CodeAttributeDeclaration declaration3 = new CodeAttributeDeclaration(typeof(WebServiceBindingAttribute).FullName);
            declaration3.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(XmlConvert.DecodeName(base.Binding.Name))));
            declaration3.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(base.Binding.ServiceDescription.TargetNamespace)));
            declaration.CustomAttributes.Add(declaration3);
            this.codeClasses.Add(declaration);
            this.classHeaders.Clear();
            return declaration;
        }

        private void BeginMetadata()
        {
            this.propertyNames.Clear();
            this.propertyValues.Clear();
        }

        protected override void BeginNamespace()
        {
            try
            {
                base.MethodNames.Clear();
                base.ExtraCodeClasses.Clear();
                this.soapImporter = new SoapSchemaImporter(base.AbstractSchemas, base.ServiceImporter.CodeGenerationOptions, base.ImportContext);
                this.xmlImporter = new XmlSchemaImporter(base.ConcreteSchemas, base.ServiceImporter.CodeGenerationOptions, base.ServiceImporter.CodeGenerator, base.ImportContext);
                foreach (Type type in base.ServiceImporter.Extensions)
                {
                    this.xmlImporter.Extensions.Add(type.FullName, type);
                }
                this.xmlImporter.Extensions.Add(TypedDataSetSchemaImporterExtension);
                this.xmlImporter.Extensions.Add(new DataSetSchemaImporterExtension());
                this.xmlExporter = new XmlCodeExporter(base.CodeNamespace, base.ServiceImporter.CodeCompileUnit, base.ServiceImporter.CodeGenerator, base.ServiceImporter.CodeGenerationOptions, base.ExportContext);
                this.soapExporter = new SoapCodeExporter(base.CodeNamespace, null, base.ServiceImporter.CodeGenerator, base.ServiceImporter.CodeGenerationOptions, base.ExportContext);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new InvalidOperationException(System.Web.Services.Res.GetString("InitFailed"), exception);
            }
        }

        private bool CheckMessageStyles(string messageName, MessagePart[] parts, SoapBodyBinding soapBodyBinding, SoapBindingStyle soapBindingStyle, out bool hasWrapper)
        {
            hasWrapper = false;
            if (soapBodyBinding.Use == SoapBindingUse.Default)
            {
                soapBodyBinding.Use = SoapBindingUse.Literal;
            }
            if (soapBodyBinding.Use == SoapBindingUse.Literal)
            {
                if (soapBindingStyle == SoapBindingStyle.Rpc)
                {
                    foreach (MessagePart part in parts)
                    {
                        if (!part.Element.IsEmpty)
                        {
                            base.UnsupportedOperationBindingWarning(System.Web.Services.Res.GetString("EachMessagePartInRpcUseLiteralMessageMustSpecify0"));
                            return false;
                        }
                    }
                    return true;
                }
                if ((parts.Length == 1) && !parts[0].Type.IsEmpty)
                {
                    if (!parts[0].Element.IsEmpty)
                    {
                        base.UnsupportedOperationBindingWarning(System.Web.Services.Res.GetString("SpecifyingATypeForUseLiteralMessagesIs0"));
                        return false;
                    }
                    if (this.xmlImporter.ImportAnyType(parts[0].Type, parts[0].Name) == null)
                    {
                        base.UnsupportedOperationBindingWarning(System.Web.Services.Res.GetString("SpecifyingATypeForUseLiteralMessagesIsAny", new object[] { parts[0].Type.Name, parts[0].Type.Namespace }));
                        return false;
                    }
                    return true;
                }
                foreach (MessagePart part2 in parts)
                {
                    if (!part2.Type.IsEmpty)
                    {
                        base.UnsupportedOperationBindingWarning(System.Web.Services.Res.GetString("SpecifyingATypeForUseLiteralMessagesIs0"));
                        return false;
                    }
                    if (part2.Element.IsEmpty)
                    {
                        base.UnsupportedOperationBindingWarning(System.Web.Services.Res.GetString("EachMessagePartInAUseLiteralMessageMustSpecify0"));
                        return false;
                    }
                }
            }
            else if (soapBodyBinding.Use == SoapBindingUse.Encoded)
            {
                if (!this.IsSoapEncodingPresent(soapBodyBinding.Encoding))
                {
                    base.UnsupportedOperationBindingWarning(System.Web.Services.Res.GetString("TheEncodingIsNotSupported1", new object[] { soapBodyBinding.Encoding }));
                    return false;
                }
                foreach (MessagePart part3 in parts)
                {
                    if (!part3.Element.IsEmpty)
                    {
                        base.UnsupportedOperationBindingWarning(System.Web.Services.Res.GetString("SpecifyingAnElementForUseEncodedMessageParts0"));
                        return false;
                    }
                    if (part3.Type.IsEmpty)
                    {
                        base.UnsupportedOperationBindingWarning(System.Web.Services.Res.GetString("EachMessagePartInAnUseEncodedMessageMustSpecify0"));
                        return false;
                    }
                }
            }
            if (soapBindingStyle == SoapBindingStyle.Rpc)
            {
                return true;
            }
            if (soapBindingStyle == SoapBindingStyle.Document)
            {
                hasWrapper = (parts.Length == 1) && (string.Compare(parts[0].Name, "parameters", StringComparison.Ordinal) == 0);
                return true;
            }
            return false;
        }

        private void CreateInvokeParams(CodeExpression[] invokeParams, string methodName, IList parameters, int checkSpecifiedCount)
        {
            invokeParams[0] = new CodePrimitiveExpression(methodName);
            CodeExpression[] initializers = new CodeExpression[parameters.Count + checkSpecifiedCount];
            int num = 0;
            for (int i = 0; i < parameters.Count; i++)
            {
                SoapParameter parameter = (SoapParameter) parameters[i];
                initializers[num++] = new CodeArgumentReferenceExpression(parameter.name);
                if (parameter.mapping.CheckSpecified)
                {
                    initializers[num++] = new CodeArgumentReferenceExpression(parameter.specifiedName);
                }
            }
            invokeParams[1] = new CodeArrayCreateExpression(typeof(object).FullName, initializers);
        }

        protected override void EndClass()
        {
            if (this.transport != null)
            {
                this.transport.ImportClass();
            }
            this.soapBinding = null;
        }

        private void EndMetadata(CodeAttributeDeclarationCollection metadata, Type attributeType, string parameter)
        {
            CodeExpression[] expressionArray;
            if (parameter == null)
            {
                expressionArray = new CodeExpression[0];
            }
            else
            {
                expressionArray = new CodeExpression[] { new CodePrimitiveExpression(parameter) };
            }
            WebCodeGenerator.AddCustomAttribute(metadata, attributeType, expressionArray, (string[]) this.propertyNames.ToArray(typeof(string)), (CodeExpression[]) this.propertyValues.ToArray(typeof(CodeExpression)));
        }

        protected override void EndNamespace()
        {
            base.ConcreteSchemas.Compile(null, false);
            foreach (GlobalSoapHeader header in this.headers.Values)
            {
                if (header.isEncoded)
                {
                    this.soapExporter.ExportTypeMapping(header.mapping);
                }
                else
                {
                    this.xmlExporter.ExportTypeMapping(header.mapping);
                }
            }
            foreach (XmlMembersMapping mapping in this.xmlMembers)
            {
                this.xmlExporter.ExportMembersMapping(mapping);
            }
            foreach (XmlMembersMapping mapping2 in this.soapMembers)
            {
                this.soapExporter.ExportMembersMapping(mapping2);
            }
            foreach (CodeTypeDeclaration declaration in this.codeClasses)
            {
                foreach (CodeAttributeDeclaration declaration2 in this.xmlExporter.IncludeMetadata)
                {
                    declaration.CustomAttributes.Add(declaration2);
                }
                foreach (CodeAttributeDeclaration declaration3 in this.soapExporter.IncludeMetadata)
                {
                    declaration.CustomAttributes.Add(declaration3);
                }
            }
            foreach (CodeTypeDeclaration declaration4 in base.ExtraCodeClasses)
            {
                base.CodeNamespace.Types.Add(declaration4);
            }
            CodeGenerator.ValidateIdentifiers(base.CodeNamespace);
        }

        private void GenerateExtensionMetadata(CodeAttributeDeclarationCollection metadata)
        {
            if (this.extensions == null)
            {
                TypeElementCollection soapExtensionImporterTypes = WebServicesSection.Current.SoapExtensionImporterTypes;
                this.extensions = new SoapExtensionImporter[soapExtensionImporterTypes.Count];
                for (int i = 0; i < this.extensions.Length; i++)
                {
                    SoapExtensionImporter importer = (SoapExtensionImporter) Activator.CreateInstance(soapExtensionImporterTypes[i].Type);
                    importer.ImportContext = this;
                    this.extensions[i] = importer;
                }
            }
            foreach (SoapExtensionImporter importer2 in this.extensions)
            {
                importer2.ImportMethod(metadata);
            }
        }

        private void GenerateHeaders(CodeAttributeDeclarationCollection metadata, SoapBindingUse use, bool rpc, MessageBinding requestMessage, MessageBinding responseMessage)
        {
            Hashtable hashtable = new Hashtable();
            for (int i = 0; i < 2; i++)
            {
                MessageBinding binding;
                SoapHeaderDirection @in;
                if (i == 0)
                {
                    binding = requestMessage;
                    @in = SoapHeaderDirection.In;
                }
                else
                {
                    if (responseMessage == null)
                    {
                        continue;
                    }
                    binding = responseMessage;
                    @in = SoapHeaderDirection.Out;
                }
                SoapHeaderBinding[] bindingArray = (SoapHeaderBinding[]) binding.Extensions.FindAll(typeof(SoapHeaderBinding));
                foreach (SoapHeaderBinding binding2 in bindingArray)
                {
                    if (binding2.MapToProperty)
                    {
                        XmlTypeMapping mapping;
                        string str;
                        if (use != binding2.Use)
                        {
                            throw new InvalidOperationException(System.Web.Services.Res.GetString("WebDescriptionHeaderAndBodyUseMismatch"));
                        }
                        if ((use == SoapBindingUse.Encoded) && !this.IsSoapEncodingPresent(binding2.Encoding))
                        {
                            throw new InvalidOperationException(System.Web.Services.Res.GetString("WebUnknownEncodingStyle", new object[] { binding2.Encoding }));
                        }
                        Message message = base.ServiceDescriptions.GetMessage(binding2.Message);
                        if (message == null)
                        {
                            throw new InvalidOperationException(System.Web.Services.Res.GetString("MissingMessage2", new object[] { binding2.Message.Name, binding2.Message.Namespace }));
                        }
                        MessagePart part = message.FindPartByName(binding2.Part);
                        if (part == null)
                        {
                            throw new InvalidOperationException(System.Web.Services.Res.GetString("MissingMessagePartForMessageFromNamespace3", new object[] { part.Name, binding2.Message.Name, binding2.Message.Namespace }));
                        }
                        if (use == SoapBindingUse.Encoded)
                        {
                            if (part.Type.IsEmpty)
                            {
                                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebDescriptionPartTypeRequired", new object[] { part.Name, binding2.Message.Name, binding2.Message.Namespace }));
                            }
                            if (!part.Element.IsEmpty)
                            {
                                base.UnsupportedOperationBindingWarning(System.Web.Services.Res.GetString("WebDescriptionPartElementWarning", new object[] { part.Name, binding2.Message.Name, binding2.Message.Namespace }));
                            }
                            mapping = this.soapImporter.ImportDerivedTypeMapping(part.Type, typeof(SoapHeader), true);
                            str = "type=" + part.Type.ToString();
                        }
                        else
                        {
                            if (part.Element.IsEmpty)
                            {
                                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebDescriptionPartElementRequired", new object[] { part.Name, binding2.Message.Name, binding2.Message.Namespace }));
                            }
                            if (!part.Type.IsEmpty)
                            {
                                base.UnsupportedOperationBindingWarning(System.Web.Services.Res.GetString("WebDescriptionPartTypeWarning", new object[] { part.Name, binding2.Message.Name, binding2.Message.Namespace }));
                            }
                            mapping = this.xmlImporter.ImportDerivedTypeMapping(part.Element, typeof(SoapHeader), true);
                            str = "element=" + part.Element.ToString();
                        }
                        LocalSoapHeader header = (LocalSoapHeader) hashtable[str];
                        if (header == null)
                        {
                            GlobalSoapHeader header2 = (GlobalSoapHeader) this.classHeaders[str];
                            if (header2 == null)
                            {
                                header2 = new GlobalSoapHeader {
                                    isEncoded = use == SoapBindingUse.Encoded
                                };
                                string identifier = CodeIdentifier.MakeValid(mapping.ElementName);
                                if (identifier == mapping.TypeName)
                                {
                                    identifier = identifier + "Value";
                                }
                                identifier = base.MethodNames.AddUnique(identifier, mapping);
                                header2.fieldName = identifier;
                                WebCodeGenerator.AddMember(base.CodeTypeDeclaration, mapping.TypeFullName, header2.fieldName, null, null, CodeFlags.IsPublic, base.ServiceImporter.CodeGenerationOptions);
                                header2.mapping = mapping;
                                this.classHeaders.Add(str, header2);
                                if (this.headers[str] == null)
                                {
                                    this.headers.Add(str, header2);
                                }
                            }
                            header = new LocalSoapHeader {
                                fieldName = header2.fieldName,
                                direction = @in
                            };
                            hashtable.Add(str, header);
                        }
                        else if (header.direction != @in)
                        {
                            header.direction = SoapHeaderDirection.InOut;
                        }
                    }
                }
            }
            foreach (LocalSoapHeader header3 in hashtable.Values)
            {
                this.BeginMetadata();
                if (header3.direction == SoapHeaderDirection.Out)
                {
                    this.AddMetadataProperty("Direction", (CodeExpression) new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(SoapHeaderDirection).FullName), SoapHeaderDirection.Out.ToString()));
                }
                else if (header3.direction == SoapHeaderDirection.InOut)
                {
                    this.AddMetadataProperty("Direction", (CodeExpression) new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(SoapHeaderDirection).FullName), SoapHeaderDirection.InOut.ToString()));
                }
                this.EndMetadata(metadata, typeof(SoapHeaderAttribute), header3.fieldName);
            }
        }

        protected override CodeMemberMethod GenerateMethod()
        {
            Message outputMessage;
            SoapBodyBinding binding2;
            MessageBinding output;
            bool flag;
            SoapOperationBinding binding5 = (SoapOperationBinding) base.OperationBinding.Extensions.Find(typeof(SoapOperationBinding));
            if (binding5 == null)
            {
                throw base.OperationBindingSyntaxException(System.Web.Services.Res.GetString("MissingSoapOperationBinding0"));
            }
            SoapBindingStyle soapBindingStyle = binding5.Style;
            if (soapBindingStyle == SoapBindingStyle.Default)
            {
                soapBindingStyle = this.SoapBinding.Style;
            }
            if (soapBindingStyle == SoapBindingStyle.Default)
            {
                soapBindingStyle = SoapBindingStyle.Document;
            }
            string[] parameterOrder = base.Operation.ParameterOrder;
            Message inputMessage = base.InputMessage;
            MessageBinding input = base.OperationBinding.Input;
            SoapBodyBinding soapBodyBinding = (SoapBodyBinding) base.OperationBinding.Input.Extensions.Find(typeof(SoapBodyBinding));
            if (soapBodyBinding == null)
            {
                base.UnsupportedOperationBindingWarning(System.Web.Services.Res.GetString("MissingSoapBodyInputBinding0"));
                return null;
            }
            if (base.Operation.Messages.Output != null)
            {
                outputMessage = base.OutputMessage;
                output = base.OperationBinding.Output;
                binding2 = (SoapBodyBinding) base.OperationBinding.Output.Extensions.Find(typeof(SoapBodyBinding));
                if (binding2 == null)
                {
                    base.UnsupportedOperationBindingWarning(System.Web.Services.Res.GetString("MissingSoapBodyOutputBinding0"));
                    return null;
                }
            }
            else
            {
                outputMessage = null;
                output = null;
                binding2 = null;
            }
            CodeAttributeDeclarationCollection metadata = new CodeAttributeDeclarationCollection();
            this.PrepareHeaders(input);
            if (output != null)
            {
                this.PrepareHeaders(output);
            }
            string name = null;
            string str = (!string.IsNullOrEmpty(input.Name) && (soapBindingStyle != SoapBindingStyle.Rpc)) ? input.Name : base.Operation.Name;
            str = XmlConvert.DecodeName(str);
            if (output != null)
            {
                name = (!string.IsNullOrEmpty(output.Name) && (soapBindingStyle != SoapBindingStyle.Rpc)) ? output.Name : (base.Operation.Name + "Response");
                name = XmlConvert.DecodeName(name);
            }
            this.GenerateExtensionMetadata(metadata);
            this.GenerateHeaders(metadata, soapBodyBinding.Use, soapBindingStyle == SoapBindingStyle.Rpc, input, output);
            MessagePart[] messageParts = this.GetMessageParts(inputMessage, soapBodyBinding);
            if (!this.CheckMessageStyles(base.MethodName, messageParts, soapBodyBinding, soapBindingStyle, out flag))
            {
                return null;
            }
            MessagePart[] parts = null;
            if (outputMessage != null)
            {
                bool flag2;
                parts = this.GetMessageParts(outputMessage, binding2);
                if (!this.CheckMessageStyles(base.MethodName, parts, binding2, soapBindingStyle, out flag2))
                {
                    return null;
                }
                if (flag != flag2)
                {
                    flag = false;
                }
            }
            bool flag3 = ((soapBindingStyle != SoapBindingStyle.Rpc) && flag) || ((soapBodyBinding.Use == SoapBindingUse.Literal) && (soapBindingStyle == SoapBindingStyle.Rpc));
            XmlMembersMapping request = this.ImportMessage(str, messageParts, soapBodyBinding, soapBindingStyle, flag);
            if (request == null)
            {
                return null;
            }
            XmlMembersMapping response = null;
            if (outputMessage != null)
            {
                response = this.ImportMessage(name, parts, binding2, soapBindingStyle, flag);
                if (response == null)
                {
                    return null;
                }
            }
            string str3 = CodeIdentifier.MakeValid(XmlConvert.DecodeName(base.Operation.Name));
            if (base.ClassName == str3)
            {
                str3 = "Call" + str3;
            }
            string identifier = base.MethodNames.AddUnique(CodeIdentifier.MakeValid(XmlConvert.DecodeName(str3)), base.Operation);
            bool flag4 = str3 != identifier;
            CodeIdentifiers identifiers = new CodeIdentifiers(false);
            identifiers.AddReserved(identifier);
            SoapParameters parameters = new SoapParameters(request, response, parameterOrder, base.MethodNames);
            foreach (SoapParameter parameter in parameters.Parameters)
            {
                if ((parameter.IsOut || parameter.IsByRef) && !base.ServiceImporter.CodeGenerator.Supports(GeneratorSupport.ReferenceParameters))
                {
                    base.UnsupportedOperationWarning(System.Web.Services.Res.GetString("CodeGenSupportReferenceParameters", new object[] { base.ServiceImporter.CodeGenerator.GetType().Name }));
                    return null;
                }
                parameter.name = identifiers.AddUnique(parameter.name, null);
                if (parameter.mapping.CheckSpecified)
                {
                    parameter.specifiedName = identifiers.AddUnique(parameter.name + "Specified", null);
                }
            }
            if ((base.Style != ServiceDescriptionImportStyle.Client) || flag4)
            {
                this.BeginMetadata();
                if (flag4)
                {
                    this.AddMetadataProperty("MessageName", identifier);
                }
                this.EndMetadata(metadata, typeof(WebMethodAttribute), null);
            }
            this.BeginMetadata();
            if ((flag3 && (request.ElementName.Length > 0)) && (request.ElementName != identifier))
            {
                this.AddMetadataProperty("RequestElementName", request.ElementName);
            }
            if (request.Namespace != null)
            {
                this.AddMetadataProperty("RequestNamespace", request.Namespace);
            }
            if (response == null)
            {
                this.AddMetadataProperty("OneWay", true);
            }
            else
            {
                if ((flag3 && (response.ElementName.Length > 0)) && (response.ElementName != (identifier + "Response")))
                {
                    this.AddMetadataProperty("ResponseElementName", response.ElementName);
                }
                if (response.Namespace != null)
                {
                    this.AddMetadataProperty("ResponseNamespace", response.Namespace);
                }
            }
            if (soapBindingStyle == SoapBindingStyle.Rpc)
            {
                if (soapBodyBinding.Use != SoapBindingUse.Encoded)
                {
                    this.AddMetadataProperty("Use", (CodeExpression) new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(SoapBindingUse).FullName), Enum.Format(typeof(SoapBindingUse), soapBodyBinding.Use, "G")));
                }
                this.EndMetadata(metadata, typeof(SoapRpcMethodAttribute), binding5.SoapAction);
            }
            else
            {
                this.AddMetadataProperty("Use", (CodeExpression) new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(SoapBindingUse).FullName), Enum.Format(typeof(SoapBindingUse), soapBodyBinding.Use, "G")));
                this.AddMetadataProperty("ParameterStyle", (CodeExpression) new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(SoapParameterStyle).FullName), Enum.Format(typeof(SoapParameterStyle), flag ? SoapParameterStyle.Wrapped : SoapParameterStyle.Bare, "G")));
                this.EndMetadata(metadata, typeof(SoapDocumentMethodAttribute), binding5.SoapAction);
            }
            base.IsEncodedBinding = base.IsEncodedBinding || (soapBodyBinding.Use == SoapBindingUse.Encoded);
            CodeAttributeDeclarationCollection[] parameterAttributes = new CodeAttributeDeclarationCollection[parameters.Parameters.Count + parameters.CheckSpecifiedCount];
            int index = 0;
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(typeof(XmlIgnoreAttribute).FullName);
            foreach (SoapParameter parameter2 in parameters.Parameters)
            {
                parameterAttributes[index] = new CodeAttributeDeclarationCollection();
                if (soapBodyBinding.Use == SoapBindingUse.Encoded)
                {
                    this.soapExporter.AddMappingMetadata(parameterAttributes[index], parameter2.mapping, parameter2.name != parameter2.mapping.MemberName);
                }
                else
                {
                    string ns = (soapBindingStyle == SoapBindingStyle.Rpc) ? parameter2.mapping.Namespace : (parameter2.IsOut ? response.Namespace : request.Namespace);
                    bool forceUseMemberName = parameter2.name != parameter2.mapping.MemberName;
                    this.xmlExporter.AddMappingMetadata(parameterAttributes[index], parameter2.mapping, ns, forceUseMemberName);
                    if (parameter2.mapping.CheckSpecified)
                    {
                        index++;
                        parameterAttributes[index] = new CodeAttributeDeclarationCollection();
                        this.xmlExporter.AddMappingMetadata(parameterAttributes[index], parameter2.mapping, ns, parameter2.specifiedName != (parameter2.mapping.MemberName + "Specified"));
                        parameterAttributes[index].Add(declaration);
                    }
                }
                if ((parameterAttributes[index].Count > 0) && !base.ServiceImporter.CodeGenerator.Supports(GeneratorSupport.ParameterAttributes))
                {
                    base.UnsupportedOperationWarning(System.Web.Services.Res.GetString("CodeGenSupportParameterAttributes", new object[] { base.ServiceImporter.CodeGenerator.GetType().Name }));
                    return null;
                }
                index++;
            }
            CodeFlags[] codeFlags = SoapParameter.GetCodeFlags(parameters.Parameters, parameters.CheckSpecifiedCount);
            string[] parameterTypeNames = SoapParameter.GetTypeFullNames(parameters.Parameters, parameters.CheckSpecifiedCount, base.ServiceImporter.CodeGenerator);
            string returnTypeName = (parameters.Return == null) ? typeof(void).FullName : WebCodeGenerator.FullTypeName(parameters.Return, base.ServiceImporter.CodeGenerator);
            CodeMemberMethod codeMethod = WebCodeGenerator.AddMethod(base.CodeTypeDeclaration, str3, codeFlags, parameterTypeNames, SoapParameter.GetNames(parameters.Parameters, parameters.CheckSpecifiedCount), parameterAttributes, returnTypeName, metadata, CodeFlags.IsPublic | ((base.Style == ServiceDescriptionImportStyle.Client) ? ((CodeFlags) 0) : CodeFlags.IsAbstract));
            codeMethod.Comments.Add(new CodeCommentStatement(System.Web.Services.Res.GetString("CodeRemarks"), true));
            if (parameters.Return != null)
            {
                if (soapBodyBinding.Use == SoapBindingUse.Encoded)
                {
                    this.soapExporter.AddMappingMetadata(codeMethod.ReturnTypeCustomAttributes, parameters.Return, parameters.Return.ElementName != (identifier + "Result"));
                }
                else
                {
                    this.xmlExporter.AddMappingMetadata(codeMethod.ReturnTypeCustomAttributes, parameters.Return, response.Namespace, parameters.Return.ElementName != (identifier + "Result"));
                }
                if ((codeMethod.ReturnTypeCustomAttributes.Count != 0) && !base.ServiceImporter.CodeGenerator.Supports(GeneratorSupport.ReturnTypeAttributes))
                {
                    base.UnsupportedOperationWarning(System.Web.Services.Res.GetString("CodeGenSupportReturnTypeAttributes", new object[] { base.ServiceImporter.CodeGenerator.GetType().Name }));
                    return null;
                }
            }
            string resultsName = identifiers.MakeUnique("results");
            if (base.Style == ServiceDescriptionImportStyle.Client)
            {
                bool flag6 = (base.ServiceImporter.CodeGenerationOptions & CodeGenerationOptions.GenerateOldAsync) != CodeGenerationOptions.None;
                bool flag7 = (((base.ServiceImporter.CodeGenerationOptions & CodeGenerationOptions.GenerateNewAsync) != CodeGenerationOptions.None) && base.ServiceImporter.CodeGenerator.Supports(GeneratorSupport.DeclareEvents)) && base.ServiceImporter.CodeGenerator.Supports(GeneratorSupport.DeclareDelegates);
                CodeExpression[] invokeParams = new CodeExpression[2];
                this.CreateInvokeParams(invokeParams, identifier, parameters.InParameters, parameters.InCheckSpecifiedCount);
                CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Invoke", invokeParams);
                this.WriteReturnMappings(codeMethod, invoke, parameters, resultsName);
                if (flag6)
                {
                    int num2 = parameters.InParameters.Count + parameters.InCheckSpecifiedCount;
                    string[] typeFullNames = new string[num2 + 2];
                    SoapParameter.GetTypeFullNames(parameters.InParameters, typeFullNames, 0, parameters.InCheckSpecifiedCount, base.ServiceImporter.CodeGenerator);
                    typeFullNames[num2] = typeof(AsyncCallback).FullName;
                    typeFullNames[num2 + 1] = typeof(object).FullName;
                    string[] strArray4 = new string[num2 + 2];
                    SoapParameter.GetNames(parameters.InParameters, strArray4, 0, parameters.InCheckSpecifiedCount);
                    strArray4[num2] = "callback";
                    strArray4[num2 + 1] = "asyncState";
                    CodeFlags[] parameterFlags = new CodeFlags[num2 + 2];
                    CodeMemberMethod method2 = WebCodeGenerator.AddMethod(base.CodeTypeDeclaration, "Begin" + identifier, parameterFlags, typeFullNames, strArray4, typeof(IAsyncResult).FullName, null, CodeFlags.IsPublic);
                    method2.Comments.Add(new CodeCommentStatement(System.Web.Services.Res.GetString("CodeRemarks"), true));
                    invokeParams = new CodeExpression[4];
                    this.CreateInvokeParams(invokeParams, identifier, parameters.InParameters, parameters.InCheckSpecifiedCount);
                    invokeParams[2] = new CodeArgumentReferenceExpression("callback");
                    invokeParams[3] = new CodeArgumentReferenceExpression("asyncState");
                    invoke = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "BeginInvoke", invokeParams);
                    method2.Statements.Add(new CodeMethodReturnStatement(invoke));
                    int num3 = parameters.OutParameters.Count + parameters.OutCheckSpecifiedCount;
                    string[] strArray5 = new string[num3 + 1];
                    SoapParameter.GetTypeFullNames(parameters.OutParameters, strArray5, 1, parameters.OutCheckSpecifiedCount, base.ServiceImporter.CodeGenerator);
                    strArray5[0] = typeof(IAsyncResult).FullName;
                    string[] strArray6 = new string[num3 + 1];
                    SoapParameter.GetNames(parameters.OutParameters, strArray6, 1, parameters.OutCheckSpecifiedCount);
                    strArray6[0] = "asyncResult";
                    CodeFlags[] flagsArray3 = new CodeFlags[num3 + 1];
                    for (int i = 0; i < num3; i++)
                    {
                        flagsArray3[i + 1] = CodeFlags.IsOut;
                    }
                    CodeMemberMethod method3 = WebCodeGenerator.AddMethod(base.CodeTypeDeclaration, "End" + identifier, flagsArray3, strArray5, strArray6, (parameters.Return == null) ? typeof(void).FullName : WebCodeGenerator.FullTypeName(parameters.Return, base.ServiceImporter.CodeGenerator), null, CodeFlags.IsPublic);
                    method3.Comments.Add(new CodeCommentStatement(System.Web.Services.Res.GetString("CodeRemarks"), true));
                    CodeExpression expression2 = new CodeArgumentReferenceExpression("asyncResult");
                    invoke = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "EndInvoke", new CodeExpression[] { expression2 });
                    this.WriteReturnMappings(method3, invoke, parameters, resultsName);
                }
                if (!flag7)
                {
                    return codeMethod;
                }
                string str8 = ProtocolImporter.MethodSignature(identifier, returnTypeName, codeFlags, parameterTypeNames);
                DelegateInfo info = (DelegateInfo) base.ExportContext[str8];
                if (info == null)
                {
                    string handlerType = base.ClassNames.AddUnique(identifier + "CompletedEventHandler", identifier);
                    string handlerArgs = base.ClassNames.AddUnique(identifier + "CompletedEventArgs", identifier);
                    info = new DelegateInfo(handlerType, handlerArgs);
                }
                string handlerName = base.MethodNames.AddUnique(identifier + "Completed", identifier);
                string methodName = base.MethodNames.AddUnique(identifier + "Async", identifier);
                string callbackMember = base.MethodNames.AddUnique(identifier + "OperationCompleted", identifier);
                string callbackName = base.MethodNames.AddUnique("On" + identifier + "OperationCompleted", identifier);
                WebCodeGenerator.AddEvent(base.CodeTypeDeclaration.Members, info.handlerType, handlerName);
                WebCodeGenerator.AddCallbackDeclaration(base.CodeTypeDeclaration.Members, callbackMember);
                string[] names = SoapParameter.GetNames(parameters.InParameters, parameters.InCheckSpecifiedCount);
                string userState = ProtocolImporter.UniqueName("userState", names);
                CodeMemberMethod method4 = WebCodeGenerator.AddAsyncMethod(base.CodeTypeDeclaration, methodName, SoapParameter.GetTypeFullNames(parameters.InParameters, parameters.InCheckSpecifiedCount, base.ServiceImporter.CodeGenerator), names, callbackMember, callbackName, userState);
                invokeParams = new CodeExpression[4];
                this.CreateInvokeParams(invokeParams, identifier, parameters.InParameters, parameters.InCheckSpecifiedCount);
                invokeParams[2] = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), callbackMember);
                invokeParams[3] = new CodeArgumentReferenceExpression(userState);
                invoke = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InvokeAsync", invokeParams);
                method4.Statements.Add(invoke);
                bool methodHasOutParameters = (parameters.Return != null) || (parameters.OutParameters.Count > 0);
                WebCodeGenerator.AddCallbackImplementation(base.CodeTypeDeclaration, callbackName, handlerName, info.handlerArgs, methodHasOutParameters);
                if (base.ExportContext[str8] != null)
                {
                    return codeMethod;
                }
                WebCodeGenerator.AddDelegate(base.ExtraCodeClasses, info.handlerType, methodHasOutParameters ? info.handlerArgs : typeof(AsyncCompletedEventArgs).FullName);
                if (methodHasOutParameters)
                {
                    int num5 = parameters.OutParameters.Count + parameters.OutCheckSpecifiedCount;
                    string[] strArray8 = new string[num5 + 1];
                    SoapParameter.GetTypeFullNames(parameters.OutParameters, strArray8, 1, parameters.OutCheckSpecifiedCount, base.ServiceImporter.CodeGenerator);
                    strArray8[0] = (parameters.Return == null) ? null : WebCodeGenerator.FullTypeName(parameters.Return, base.ServiceImporter.CodeGenerator);
                    string[] strArray9 = new string[num5 + 1];
                    SoapParameter.GetNames(parameters.OutParameters, strArray9, 1, parameters.OutCheckSpecifiedCount);
                    strArray9[0] = (parameters.Return == null) ? null : "Result";
                    base.ExtraCodeClasses.Add(WebCodeGenerator.CreateArgsClass(info.handlerArgs, strArray8, strArray9, base.ServiceImporter.CodeGenerator.Supports(GeneratorSupport.PartialTypes)));
                }
                base.ExportContext[str8] = info;
            }
            return codeMethod;
        }

        private MessagePart[] GetMessageParts(Message message, SoapBodyBinding soapBodyBinding)
        {
            if (soapBodyBinding.Parts == null)
            {
                MessagePart[] array = new MessagePart[message.Parts.Count];
                message.Parts.CopyTo(array, 0);
                return array;
            }
            return message.FindPartsByName(soapBodyBinding.Parts);
        }

        internal SoapTransportImporter GetTransport(string transport)
        {
            foreach (Type type in WebServicesSection.Current.SoapTransportImporters)
            {
                SoapTransportImporter importer = (SoapTransportImporter) Activator.CreateInstance(type);
                importer.ImportContext = this;
                if (importer.IsSupportedTransport(transport))
                {
                    return importer;
                }
            }
            return null;
        }

        private XmlMembersMapping ImportEncodedMessage(string messageName, MessagePart[] parts, SoapBodyBinding soapBodyBinding, bool wrapped)
        {
            XmlMembersMapping mapping;
            if (wrapped)
            {
                SoapSchemaMember member = new SoapSchemaMember {
                    MemberName = parts[0].Name,
                    MemberType = parts[0].Type
                };
                mapping = this.soapImporter.ImportMembersMapping(messageName, soapBodyBinding.Namespace, member);
            }
            else
            {
                SoapSchemaMember[] members = new SoapSchemaMember[parts.Length];
                for (int i = 0; i < members.Length; i++)
                {
                    MessagePart part = parts[i];
                    members[i] = new SoapSchemaMember { MemberName = part.Name, MemberType = part.Type };
                }
                mapping = this.soapImporter.ImportMembersMapping(messageName, soapBodyBinding.Namespace, members);
            }
            this.soapMembers.Add(mapping);
            return mapping;
        }

        private XmlMembersMapping ImportLiteralMessage(string messageName, MessagePart[] parts, SoapBodyBinding soapBodyBinding, SoapBindingStyle soapBindingStyle, bool wrapped)
        {
            XmlMembersMapping mapping;
            if (soapBindingStyle == SoapBindingStyle.Rpc)
            {
                SoapSchemaMember[] members = new SoapSchemaMember[parts.Length];
                for (int i = 0; i < members.Length; i++)
                {
                    MessagePart part = parts[i];
                    members[i] = new SoapSchemaMember { MemberName = part.Name, MemberType = part.Type };
                }
                mapping = this.xmlImporter.ImportMembersMapping(messageName, soapBodyBinding.Namespace, members);
            }
            else if (wrapped)
            {
                mapping = this.xmlImporter.ImportMembersMapping(parts[0].Element);
            }
            else
            {
                if ((parts.Length == 1) && !parts[0].Type.IsEmpty)
                {
                    mapping = this.xmlImporter.ImportAnyType(parts[0].Type, parts[0].Name);
                    this.xmlMembers.Add(mapping);
                    return mapping;
                }
                XmlQualifiedName[] names = new XmlQualifiedName[parts.Length];
                for (int j = 0; j < parts.Length; j++)
                {
                    names[j] = parts[j].Element;
                }
                mapping = this.xmlImporter.ImportMembersMapping(names);
            }
            this.xmlMembers.Add(mapping);
            return mapping;
        }

        private XmlMembersMapping ImportMessage(string messageName, MessagePart[] parts, SoapBodyBinding soapBodyBinding, SoapBindingStyle soapBindingStyle, bool wrapped)
        {
            if (soapBodyBinding.Use == SoapBindingUse.Encoded)
            {
                return this.ImportEncodedMessage(messageName, parts, soapBodyBinding, wrapped);
            }
            return this.ImportLiteralMessage(messageName, parts, soapBodyBinding, soapBindingStyle, wrapped);
        }

        protected override bool IsBindingSupported()
        {
            System.Web.Services.Description.SoapBinding binding = (System.Web.Services.Description.SoapBinding) base.Binding.Extensions.Find(typeof(System.Web.Services.Description.SoapBinding));
            if ((binding == null) || (binding.GetType() != typeof(System.Web.Services.Description.SoapBinding)))
            {
                return false;
            }
            if (this.GetTransport(binding.Transport) == null)
            {
                base.UnsupportedBindingWarning(System.Web.Services.Res.GetString("ThereIsNoSoapTransportImporterThatUnderstands1", new object[] { binding.Transport }));
                return false;
            }
            return true;
        }

        protected override bool IsOperationFlowSupported(OperationFlow flow)
        {
            if (flow != OperationFlow.OneWay)
            {
                return (flow == OperationFlow.RequestResponse);
            }
            return true;
        }

        protected virtual bool IsSoapEncodingPresent(string uriList)
        {
            int startIndex = 0;
            do
            {
                startIndex = uriList.IndexOf("http://schemas.xmlsoap.org/soap/encoding/", startIndex, StringComparison.Ordinal);
                if (startIndex < 0)
                {
                    return false;
                }
                int num2 = startIndex + "http://schemas.xmlsoap.org/soap/encoding/".Length;
                if (((startIndex == 0) || (uriList[startIndex - 1] == ' ')) && ((num2 == uriList.Length) || (uriList[num2] == ' ')))
                {
                    return true;
                }
                startIndex = num2;
            }
            while (startIndex < uriList.Length);
            return false;
        }

        private void PrepareHeaders(MessageBinding messageBinding)
        {
            SoapHeaderBinding[] bindingArray = (SoapHeaderBinding[]) messageBinding.Extensions.FindAll(typeof(SoapHeaderBinding));
            foreach (SoapHeaderBinding binding in bindingArray)
            {
                binding.MapToProperty = true;
            }
        }

        private void WriteReturnMappings(CodeMemberMethod codeMethod, CodeExpression invoke, SoapParameters parameters, string resultsName)
        {
            if ((parameters.Return == null) && (parameters.OutParameters.Count == 0))
            {
                codeMethod.Statements.Add(new CodeExpressionStatement(invoke));
            }
            else
            {
                codeMethod.Statements.Add(new CodeVariableDeclarationStatement(typeof(object[]), resultsName, invoke));
                int num = (parameters.Return == null) ? 0 : 1;
                for (int i = 0; i < parameters.OutParameters.Count; i++)
                {
                    SoapParameter parameter = (SoapParameter) parameters.OutParameters[i];
                    CodeExpression left = new CodeArgumentReferenceExpression(parameter.name);
                    CodeExpression expression = new CodeArrayIndexerExpression();
                    ((CodeArrayIndexerExpression) expression).TargetObject = new CodeVariableReferenceExpression(resultsName);
                    ((CodeArrayIndexerExpression) expression).Indices.Add(new CodePrimitiveExpression(num++));
                    expression = new CodeCastExpression(WebCodeGenerator.FullTypeName(parameter.mapping, base.ServiceImporter.CodeGenerator), expression);
                    codeMethod.Statements.Add(new CodeAssignStatement(left, expression));
                    if (parameter.mapping.CheckSpecified)
                    {
                        left = new CodeArgumentReferenceExpression(parameter.name + "Specified");
                        expression = new CodeArrayIndexerExpression();
                        ((CodeArrayIndexerExpression) expression).TargetObject = new CodeVariableReferenceExpression(resultsName);
                        ((CodeArrayIndexerExpression) expression).Indices.Add(new CodePrimitiveExpression(num++));
                        expression = new CodeCastExpression(typeof(bool).FullName, expression);
                        codeMethod.Statements.Add(new CodeAssignStatement(left, expression));
                    }
                }
                if (parameters.Return != null)
                {
                    CodeExpression expression3 = new CodeArrayIndexerExpression();
                    ((CodeArrayIndexerExpression) expression3).TargetObject = new CodeVariableReferenceExpression(resultsName);
                    ((CodeArrayIndexerExpression) expression3).Indices.Add(new CodePrimitiveExpression(0));
                    expression3 = new CodeCastExpression(WebCodeGenerator.FullTypeName(parameters.Return, base.ServiceImporter.CodeGenerator), expression3);
                    codeMethod.Statements.Add(new CodeMethodReturnStatement(expression3));
                }
            }
        }

        private bool MetadataPropertiesAdded
        {
            get
            {
                return (this.propertyNames.Count > 0);
            }
        }

        public override string ProtocolName
        {
            get
            {
                return "Soap";
            }
        }

        public System.Web.Services.Description.SoapBinding SoapBinding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.soapBinding;
            }
        }

        public SoapCodeExporter SoapExporter
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.soapExporter;
            }
        }

        public SoapSchemaImporter SoapImporter
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.soapImporter;
            }
        }

        private static System.Data.Design.TypedDataSetSchemaImporterExtension TypedDataSetSchemaImporterExtension
        {
            get
            {
                if (typedDataSetSchemaImporterExtension == null)
                {
                    typedDataSetSchemaImporterExtension = new System.Data.Design.TypedDataSetSchemaImporterExtension();
                }
                return typedDataSetSchemaImporterExtension;
            }
        }

        public XmlCodeExporter XmlExporter
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.xmlExporter;
            }
        }

        public XmlSchemaImporter XmlImporter
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.xmlImporter;
            }
        }
    }
}

