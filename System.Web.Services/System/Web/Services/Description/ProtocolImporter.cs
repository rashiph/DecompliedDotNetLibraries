namespace System.Web.Services.Description
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Xml;
    using System.Xml.Serialization;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public abstract class ProtocolImporter
    {
        private bool anyPorts;
        private System.Web.Services.Description.Binding binding;
        private int bindingCount;
        private CodeTypeDeclarationCollection classes;
        private string className;
        private System.CodeDom.CodeTypeDeclaration codeClass;
        private System.CodeDom.CodeNamespace codeNamespace;
        private bool encodedBinding;
        private Hashtable exportContext;
        private System.Xml.Serialization.ImportContext importContext;
        private ServiceDescriptionImporter importer;
        private Message inputMessage;
        private CodeIdentifiers methodNames;
        private System.Web.Services.Description.Operation operation;
        private System.Web.Services.Description.OperationBinding operationBinding;
        private Message outputMessage;
        private System.Web.Services.Description.Port port;
        private System.Web.Services.Description.PortType portType;
        private System.Web.Services.Description.Service service;
        private ServiceDescriptionImportWarnings warnings;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ProtocolImporter()
        {
        }

        public void AddExtensionWarningComments(CodeCommentStatementCollection comments, ServiceDescriptionFormatExtensionCollection extensions)
        {
            foreach (object obj2 in extensions)
            {
                if (!extensions.IsHandled(obj2))
                {
                    string localName = null;
                    string namespaceURI = null;
                    if (obj2 is XmlElement)
                    {
                        XmlElement element = (XmlElement) obj2;
                        localName = element.LocalName;
                        namespaceURI = element.NamespaceURI;
                    }
                    else if (obj2 is ServiceDescriptionFormatExtension)
                    {
                        XmlFormatExtensionAttribute[] customAttributes = (XmlFormatExtensionAttribute[]) obj2.GetType().GetCustomAttributes(typeof(XmlFormatExtensionAttribute), false);
                        if (customAttributes.Length > 0)
                        {
                            localName = customAttributes[0].ElementName;
                            namespaceURI = customAttributes[0].Namespace;
                        }
                    }
                    if (localName != null)
                    {
                        if (extensions.IsRequired(obj2))
                        {
                            this.warnings |= ServiceDescriptionImportWarnings.RequiredExtensionsIgnored;
                            AddWarningComment(comments, System.Web.Services.Res.GetString("WebServiceDescriptionIgnoredRequired", new object[] { localName, namespaceURI }));
                        }
                        else
                        {
                            this.warnings |= ServiceDescriptionImportWarnings.OptionalExtensionsIgnored;
                            AddWarningComment(comments, System.Web.Services.Res.GetString("WebServiceDescriptionIgnoredOptional", new object[] { localName, namespaceURI }));
                        }
                    }
                }
            }
        }

        internal static void AddWarningComment(CodeCommentStatementCollection comments, string text)
        {
            comments.Add(new CodeCommentStatement(System.Web.Services.Res.GetString("CodegenWarningDetails", new object[] { text })));
        }

        protected abstract System.CodeDom.CodeTypeDeclaration BeginClass();
        protected virtual void BeginNamespace()
        {
            this.MethodNames.Clear();
        }

        protected virtual void EndClass()
        {
        }

        protected virtual void EndNamespace()
        {
            if (this.classes != null)
            {
                foreach (System.CodeDom.CodeTypeDeclaration declaration in this.classes)
                {
                    this.codeNamespace.Types.Add(declaration);
                }
            }
            CodeGenerator.ValidateIdentifiers(this.codeNamespace);
        }

        private void GenerateClassForBinding()
        {
            try
            {
                if (((this.bindingCount == 1) && (this.service != null)) && (this.Style != ServiceDescriptionImportStyle.ServerInterface))
                {
                    this.className = XmlConvert.DecodeName(this.service.Name);
                }
                else
                {
                    this.className = this.binding.Name;
                    if (this.Style == ServiceDescriptionImportStyle.ServerInterface)
                    {
                        this.className = "I" + CodeIdentifier.MakePascal(this.className);
                    }
                }
                this.className = XmlConvert.DecodeName(this.className);
                this.className = this.ClassNames.AddUnique(CodeIdentifier.MakeValid(this.className), null);
                this.codeClass = this.BeginClass();
                int num = 0;
                for (int i = 0; i < this.portType.Operations.Count; i++)
                {
                    CodeMemberMethod method;
                    this.MoveToOperation(this.portType.Operations[i]);
                    if (!this.IsOperationFlowSupported(this.operation.Messages.Flow))
                    {
                        switch (this.operation.Messages.Flow)
                        {
                            case OperationFlow.OneWay:
                            {
                                this.UnsupportedOperationWarning(System.Web.Services.Res.GetString("OneWayIsNotSupported0"));
                                continue;
                            }
                            case OperationFlow.Notification:
                            {
                                this.UnsupportedOperationWarning(System.Web.Services.Res.GetString("NotificationIsNotSupported0"));
                                continue;
                            }
                            case OperationFlow.RequestResponse:
                            {
                                this.UnsupportedOperationWarning(System.Web.Services.Res.GetString("RequestResponseIsNotSupported0"));
                                continue;
                            }
                            case OperationFlow.SolicitResponse:
                            {
                                this.UnsupportedOperationWarning(System.Web.Services.Res.GetString("SolicitResponseIsNotSupported0"));
                                continue;
                            }
                        }
                    }
                    try
                    {
                        method = this.GenerateMethod();
                    }
                    catch (Exception exception)
                    {
                        if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                        {
                            throw;
                        }
                        throw new InvalidOperationException(System.Web.Services.Res.GetString("UnableToImportOperation1", new object[] { this.operation.Name }), exception);
                    }
                    if (method != null)
                    {
                        this.AddExtensionWarningComments(this.codeClass.Comments, this.operationBinding.Extensions);
                        if (this.operationBinding.Input != null)
                        {
                            this.AddExtensionWarningComments(this.codeClass.Comments, this.operationBinding.Input.Extensions);
                        }
                        if (this.operationBinding.Output != null)
                        {
                            this.AddExtensionWarningComments(this.codeClass.Comments, this.operationBinding.Output.Extensions);
                        }
                        num++;
                    }
                }
                if ((((((this.ServiceImporter.CodeGenerationOptions & CodeGenerationOptions.GenerateNewAsync) != CodeGenerationOptions.None) && this.ServiceImporter.CodeGenerator.Supports(GeneratorSupport.DeclareEvents)) && this.ServiceImporter.CodeGenerator.Supports(GeneratorSupport.DeclareDelegates)) && (num > 0)) && (this.Style == ServiceDescriptionImportStyle.Client))
                {
                    CodeAttributeDeclarationCollection metadata = new CodeAttributeDeclarationCollection();
                    string identifier = "CancelAsync";
                    string methodName = this.MethodNames.AddUnique(identifier, identifier);
                    CodeMemberMethod method2 = WebCodeGenerator.AddMethod(this.CodeTypeDeclaration, methodName, new CodeFlags[1], new string[] { typeof(object).FullName }, new string[] { "userState" }, typeof(void).FullName, metadata, CodeFlags.IsPublic | ((identifier != methodName) ? ((CodeFlags) 0) : CodeFlags.IsNew));
                    method2.Comments.Add(new CodeCommentStatement(System.Web.Services.Res.GetString("CodeRemarks"), true));
                    CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), identifier, new CodeExpression[0]);
                    expression.Parameters.Add(new CodeArgumentReferenceExpression("userState"));
                    method2.Statements.Add(expression);
                }
                this.EndClass();
                if (this.portType.Operations.Count == 0)
                {
                    this.NoMethodsGeneratedWarning();
                }
                this.AddExtensionWarningComments(this.codeClass.Comments, this.binding.Extensions);
                if (this.port != null)
                {
                    this.AddExtensionWarningComments(this.codeClass.Comments, this.port.Extensions);
                }
                this.codeNamespace.Types.Add(this.codeClass);
            }
            catch (Exception exception2)
            {
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                throw new InvalidOperationException(System.Web.Services.Res.GetString("UnableToImportBindingFromNamespace2", new object[] { this.binding.Name, this.binding.ServiceDescription.TargetNamespace }), exception2);
            }
        }

        internal bool GenerateCode(System.CodeDom.CodeNamespace codeNamespace, System.Xml.Serialization.ImportContext importContext, Hashtable exportContext)
        {
            this.bindingCount = 0;
            this.anyPorts = false;
            this.codeNamespace = codeNamespace;
            Hashtable hashtable = new Hashtable();
            Hashtable hashtable2 = new Hashtable();
            foreach (ServiceDescription description in this.ServiceDescriptions)
            {
                foreach (System.Web.Services.Description.Service service in description.Services)
                {
                    foreach (System.Web.Services.Description.Port port in service.Ports)
                    {
                        System.Web.Services.Description.Binding key = this.ServiceDescriptions.GetBinding(port.Binding);
                        if (!hashtable.Contains(key))
                        {
                            System.Web.Services.Description.PortType portType = this.ServiceDescriptions.GetPortType(key.Type);
                            this.MoveToBinding(service, port, key, portType);
                            if (this.IsBindingSupported())
                            {
                                this.bindingCount++;
                                this.anyPorts = true;
                                hashtable.Add(key, key);
                            }
                            else if (key != null)
                            {
                                hashtable2[key] = key;
                            }
                        }
                    }
                }
            }
            if (this.bindingCount == 0)
            {
                foreach (ServiceDescription description2 in this.ServiceDescriptions)
                {
                    foreach (System.Web.Services.Description.Binding binding2 in description2.Bindings)
                    {
                        if (!hashtable2.Contains(binding2))
                        {
                            System.Web.Services.Description.PortType type2 = this.ServiceDescriptions.GetPortType(binding2.Type);
                            this.MoveToBinding(binding2, type2);
                            if (this.IsBindingSupported())
                            {
                                this.bindingCount++;
                            }
                        }
                    }
                }
            }
            if (this.bindingCount == 0)
            {
                return (codeNamespace.Comments.Count > 0);
            }
            this.importContext = importContext;
            this.exportContext = exportContext;
            this.BeginNamespace();
            hashtable.Clear();
            foreach (ServiceDescription description3 in this.ServiceDescriptions)
            {
                if (this.anyPorts)
                {
                    foreach (System.Web.Services.Description.Service service2 in description3.Services)
                    {
                        foreach (System.Web.Services.Description.Port port2 in service2.Ports)
                        {
                            System.Web.Services.Description.Binding binding = this.ServiceDescriptions.GetBinding(port2.Binding);
                            System.Web.Services.Description.PortType type3 = this.ServiceDescriptions.GetPortType(binding.Type);
                            this.MoveToBinding(service2, port2, binding, type3);
                            if (this.IsBindingSupported() && !hashtable.Contains(binding))
                            {
                                this.GenerateClassForBinding();
                                hashtable.Add(binding, binding);
                            }
                        }
                    }
                }
                else
                {
                    foreach (System.Web.Services.Description.Binding binding4 in description3.Bindings)
                    {
                        System.Web.Services.Description.PortType type4 = this.ServiceDescriptions.GetPortType(binding4.Type);
                        this.MoveToBinding(binding4, type4);
                        if (this.IsBindingSupported())
                        {
                            this.GenerateClassForBinding();
                        }
                    }
                }
            }
            this.EndNamespace();
            return true;
        }

        protected abstract CodeMemberMethod GenerateMethod();
        internal void Initialize(ServiceDescriptionImporter importer)
        {
            this.importer = importer;
        }

        protected abstract bool IsBindingSupported();
        protected abstract bool IsOperationFlowSupported(OperationFlow flow);
        internal static string MethodSignature(string methodName, string returnType, CodeFlags[] parameterFlags, string[] parameterTypes)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(returnType);
            builder.Append(" ");
            builder.Append(methodName);
            builder.Append(" (");
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                if ((parameterFlags[i] & CodeFlags.IsByRef) != ((CodeFlags) 0))
                {
                    builder.Append("ref ");
                }
                else if ((parameterFlags[i] & CodeFlags.IsOut) != ((CodeFlags) 0))
                {
                    builder.Append("out ");
                }
                builder.Append(parameterTypes[i]);
                if (i > 0)
                {
                    builder.Append(",");
                }
            }
            builder.Append(")");
            return builder.ToString();
        }

        private void MoveToBinding(System.Web.Services.Description.Binding binding, System.Web.Services.Description.PortType portType)
        {
            this.MoveToBinding(null, null, binding, portType);
        }

        private void MoveToBinding(System.Web.Services.Description.Service service, System.Web.Services.Description.Port port, System.Web.Services.Description.Binding binding, System.Web.Services.Description.PortType portType)
        {
            this.service = service;
            this.port = port;
            this.portType = portType;
            this.binding = binding;
            this.encodedBinding = false;
        }

        private void MoveToOperation(System.Web.Services.Description.Operation operation)
        {
            this.operation = operation;
            this.operationBinding = null;
            foreach (System.Web.Services.Description.OperationBinding binding in this.binding.Operations)
            {
                if (operation.IsBoundBy(binding))
                {
                    if (this.operationBinding != null)
                    {
                        throw this.OperationSyntaxException(System.Web.Services.Res.GetString("DuplicateInputOutputNames0"));
                    }
                    this.operationBinding = binding;
                }
            }
            if (this.operationBinding == null)
            {
                throw this.OperationSyntaxException(System.Web.Services.Res.GetString("MissingBinding0"));
            }
            if ((operation.Messages.Input != null) && (this.operationBinding.Input == null))
            {
                throw this.OperationSyntaxException(System.Web.Services.Res.GetString("MissingInputBinding0"));
            }
            if ((operation.Messages.Output != null) && (this.operationBinding.Output == null))
            {
                throw this.OperationSyntaxException(System.Web.Services.Res.GetString("MissingOutputBinding0"));
            }
            this.inputMessage = (operation.Messages.Input == null) ? null : this.ServiceDescriptions.GetMessage(operation.Messages.Input.Message);
            this.outputMessage = (operation.Messages.Output == null) ? null : this.ServiceDescriptions.GetMessage(operation.Messages.Output.Message);
        }

        private void NoMethodsGeneratedWarning()
        {
            AddWarningComment(this.codeClass.Comments, System.Web.Services.Res.GetString("NoMethodsWereFoundInTheWSDLForThisProtocol"));
            this.warnings |= ServiceDescriptionImportWarnings.NoMethodsGenerated;
        }

        public Exception OperationBindingSyntaxException(string text)
        {
            return new Exception(System.Web.Services.Res.GetString("TheOperationBindingFromNamespaceHadInvalid3", new object[] { this.operationBinding.Name, this.operationBinding.Binding.ServiceDescription.TargetNamespace, text }));
        }

        public Exception OperationSyntaxException(string text)
        {
            return new Exception(System.Web.Services.Res.GetString("TheOperationFromNamespaceHadInvalidSyntax3", new object[] { this.operation.Name, this.operation.PortType.Name, this.operation.PortType.ServiceDescription.TargetNamespace, text }));
        }

        internal static string UniqueName(string baseName, string[] scope)
        {
            CodeIdentifiers identifiers = new CodeIdentifiers();
            for (int i = 0; i < scope.Length; i++)
            {
                identifiers.AddUnique(scope[i], scope[i]);
            }
            return identifiers.AddUnique(baseName, baseName);
        }

        public void UnsupportedBindingWarning(string text)
        {
            AddWarningComment((this.codeClass == null) ? this.codeNamespace.Comments : this.codeClass.Comments, System.Web.Services.Res.GetString("TheBinding0FromNamespace1WasIgnored2", new object[] { this.Binding.Name, this.Binding.ServiceDescription.TargetNamespace, text }));
            this.warnings |= ServiceDescriptionImportWarnings.UnsupportedBindingsIgnored;
        }

        public void UnsupportedOperationBindingWarning(string text)
        {
            AddWarningComment((this.codeClass == null) ? this.codeNamespace.Comments : this.codeClass.Comments, System.Web.Services.Res.GetString("TheOperationBinding0FromNamespace1WasIgnored", new object[] { this.operationBinding.Name, this.operationBinding.Binding.ServiceDescription.TargetNamespace, text }));
            this.warnings |= ServiceDescriptionImportWarnings.UnsupportedOperationsIgnored;
        }

        public void UnsupportedOperationWarning(string text)
        {
            AddWarningComment((this.codeClass == null) ? this.codeNamespace.Comments : this.codeClass.Comments, System.Web.Services.Res.GetString("TheOperation0FromNamespace1WasIgnored2", new object[] { this.operation.Name, this.operation.PortType.ServiceDescription.TargetNamespace, text }));
            this.warnings |= ServiceDescriptionImportWarnings.UnsupportedOperationsIgnored;
        }

        public XmlSchemas AbstractSchemas
        {
            get
            {
                return this.importer.AbstractSchemas;
            }
        }

        public System.Web.Services.Description.Binding Binding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.binding;
            }
        }

        public string ClassName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.className;
            }
        }

        public CodeIdentifiers ClassNames
        {
            get
            {
                return this.importContext.TypeIdentifiers;
            }
        }

        public System.CodeDom.CodeNamespace CodeNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.codeNamespace;
            }
        }

        public System.CodeDom.CodeTypeDeclaration CodeTypeDeclaration
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.codeClass;
            }
        }

        public XmlSchemas ConcreteSchemas
        {
            get
            {
                return this.importer.ConcreteSchemas;
            }
        }

        internal Hashtable ExportContext
        {
            get
            {
                if (this.exportContext == null)
                {
                    this.exportContext = new Hashtable();
                }
                return this.exportContext;
            }
        }

        internal CodeTypeDeclarationCollection ExtraCodeClasses
        {
            get
            {
                if (this.classes == null)
                {
                    this.classes = new CodeTypeDeclarationCollection();
                }
                return this.classes;
            }
        }

        internal System.Xml.Serialization.ImportContext ImportContext
        {
            get
            {
                return this.importContext;
            }
        }

        public Message InputMessage
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.inputMessage;
            }
        }

        internal bool IsEncodedBinding
        {
            get
            {
                return this.encodedBinding;
            }
            set
            {
                this.encodedBinding = value;
            }
        }

        public string MethodName
        {
            get
            {
                return CodeIdentifier.MakeValid(XmlConvert.DecodeName(this.Operation.Name));
            }
        }

        internal CodeIdentifiers MethodNames
        {
            get
            {
                if (this.methodNames == null)
                {
                    this.methodNames = new CodeIdentifiers();
                }
                return this.methodNames;
            }
        }

        public System.Web.Services.Description.Operation Operation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.operation;
            }
        }

        public System.Web.Services.Description.OperationBinding OperationBinding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.operationBinding;
            }
        }

        public Message OutputMessage
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.outputMessage;
            }
        }

        public System.Web.Services.Description.Port Port
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.port;
            }
        }

        public System.Web.Services.Description.PortType PortType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.portType;
            }
        }

        public abstract string ProtocolName { get; }

        public XmlSchemas Schemas
        {
            get
            {
                return this.importer.AllSchemas;
            }
        }

        public System.Web.Services.Description.Service Service
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.service;
            }
        }

        public ServiceDescriptionCollection ServiceDescriptions
        {
            get
            {
                return this.importer.ServiceDescriptions;
            }
        }

        internal ServiceDescriptionImporter ServiceImporter
        {
            get
            {
                return this.importer;
            }
        }

        public ServiceDescriptionImportStyle Style
        {
            get
            {
                return this.importer.Style;
            }
        }

        public ServiceDescriptionImportWarnings Warnings
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.warnings;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.warnings = value;
            }
        }
    }
}

