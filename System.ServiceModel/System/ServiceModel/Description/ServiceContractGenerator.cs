namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Security;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Threading;

    public class ServiceContractGenerator
    {
        private CodeCompileUnit compileUnit;
        private System.Configuration.Configuration configuration;
        private ConfigWriter configWriter;
        private Collection<MetadataConversionError> errors;
        private Dictionary<OperationDescription, OperationContractGenerationContext> generatedOperations;
        private Dictionary<MessageDescription, CodeTypeReference> generatedTypedMessages;
        private Dictionary<ContractDescription, ServiceContractGenerationContext> generatedTypes;
        private NamespaceHelper namespaceManager;
        private OptionsHelper options;
        private Dictionary<ContractDescription, System.Type> referencedTypes;

        public ServiceContractGenerator() : this(null, null)
        {
        }

        public ServiceContractGenerator(CodeCompileUnit targetCompileUnit) : this(targetCompileUnit, null)
        {
        }

        public ServiceContractGenerator(System.Configuration.Configuration targetConfig) : this(null, targetConfig)
        {
        }

        public ServiceContractGenerator(CodeCompileUnit targetCompileUnit, System.Configuration.Configuration targetConfig)
        {
            this.options = new OptionsHelper(ServiceContractGenerationOptions.ClientClass | ServiceContractGenerationOptions.ChannelInterface);
            this.errors = new Collection<MetadataConversionError>();
            this.compileUnit = targetCompileUnit ?? new CodeCompileUnit();
            this.namespaceManager = new NamespaceHelper(this.compileUnit.Namespaces);
            this.AddReferencedAssembly(typeof(ServiceContractGenerator).Assembly);
            this.configuration = targetConfig;
            if (targetConfig != null)
            {
                this.configWriter = new ConfigWriter(targetConfig);
            }
            this.generatedTypes = new Dictionary<ContractDescription, ServiceContractGenerationContext>();
            this.generatedOperations = new Dictionary<OperationDescription, OperationContractGenerationContext>();
            this.referencedTypes = new Dictionary<ContractDescription, System.Type>();
        }

        internal void AddReferencedAssembly(Assembly assembly)
        {
            string fileName = Path.GetFileName(assembly.Location);
            bool flag = false;
            using (StringEnumerator enumerator = this.compileUnit.ReferencedAssemblies.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (string.Compare(enumerator.Current, fileName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        flag = true;
                        goto Label_0054;
                    }
                }
            }
        Label_0054:
            if (!flag)
            {
                this.compileUnit.ReferencedAssemblies.Add(fileName);
            }
        }

        public void GenerateBinding(Binding binding, out string bindingSectionName, out string configurationName)
        {
            this.configWriter.WriteBinding(binding, out bindingSectionName, out configurationName);
        }

        public CodeTypeReference GenerateServiceContractType(ContractDescription contractDescription)
        {
            CodeTypeReference reference = this.GenerateServiceContractTypeInternal(contractDescription);
            CodeGenerator.ValidateIdentifiers(this.TargetCompileUnit);
            return reference;
        }

        private CodeTypeReference GenerateServiceContractTypeInternal(ContractDescription contractDescription)
        {
            System.Type type;
            ServiceContractGenerationContext context;
            if (contractDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractDescription");
            }
            if (this.referencedTypes.TryGetValue(contractDescription, out type))
            {
                return this.GetCodeTypeReference(type);
            }
            this.NamespaceManager.EnsureNamespace(contractDescription.Namespace);
            if (!this.generatedTypes.TryGetValue(contractDescription, out context))
            {
                context = new ContextInitializer(this, new CodeTypeFactory(this, this.options.IsSet(ServiceContractGenerationOptions.InternalTypes))).CreateContext(contractDescription);
                ExtensionsHelper.CallContractExtensions(this.GetBeforeExtensionsBuiltInContractGenerators(), context);
                ExtensionsHelper.CallOperationExtensions(this.GetBeforeExtensionsBuiltInOperationGenerators(), context);
                ExtensionsHelper.CallBehaviorExtensions(context);
                ExtensionsHelper.CallContractExtensions(this.GetAfterExtensionsBuiltInContractGenerators(), context);
                ExtensionsHelper.CallOperationExtensions(this.GetAfterExtensionsBuiltInOperationGenerators(), context);
                this.generatedTypes.Add(contractDescription, context);
            }
            return context.ContractTypeReference;
        }

        public CodeTypeReference GenerateServiceEndpoint(ServiceEndpoint endpoint, out ChannelEndpointElement channelElement)
        {
            CodeTypeReference codeTypeReference;
            string fullName;
            System.Type type;
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if (this.configuration == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxServiceContractGeneratorConfigRequired")));
            }
            if (this.referencedTypes.TryGetValue(endpoint.Contract, out type))
            {
                codeTypeReference = this.GetCodeTypeReference(type);
                fullName = type.FullName;
            }
            else
            {
                codeTypeReference = this.GenerateServiceContractType(endpoint.Contract);
                fullName = codeTypeReference.BaseType;
            }
            channelElement = this.configWriter.WriteChannelDescription(endpoint, fullName);
            return codeTypeReference;
        }

        private IEnumerable<IServiceContractGenerationExtension> GetAfterExtensionsBuiltInContractGenerators()
        {
            if (this.options.IsSet(ServiceContractGenerationOptions.ChannelInterface))
            {
                yield return new ChannelInterfaceGenerator();
            }
            if (this.options.IsSet(ServiceContractGenerationOptions.ClientClass))
            {
                bool tryAddHelperMethod = !this.options.IsSet(ServiceContractGenerationOptions.TypedMessages);
                bool generateEventAsyncMethods = this.options.IsSet(ServiceContractGenerationOptions.EventBasedAsynchronousMethods);
                yield return new ClientClassGenerator(tryAddHelperMethod, generateEventAsyncMethods);
            }
        }

        private IEnumerable<IOperationContractGenerationExtension> GetAfterExtensionsBuiltInOperationGenerators()
        {
            return EmptyArray<IOperationContractGenerationExtension>.Instance;
        }

        private IEnumerable<IServiceContractGenerationExtension> GetBeforeExtensionsBuiltInContractGenerators()
        {
            return EmptyArray<IServiceContractGenerationExtension>.Instance;
        }

        private IEnumerable<IOperationContractGenerationExtension> GetBeforeExtensionsBuiltInOperationGenerators()
        {
            yield return new FaultContractAttributeGenerator();
            yield return new TransactionFlowAttributeGenerator();
        }

        internal CodeTypeReference GetCodeTypeReference(System.Type type)
        {
            this.AddReferencedAssembly(type.Assembly);
            return new CodeTypeReference(type);
        }

        internal static CodeExpression GetEnumReference<EnumType>(EnumType value)
        {
            return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(EnumType)), Enum.Format(typeof(EnumType), value, "G"));
        }

        public System.Configuration.Configuration Configuration
        {
            get
            {
                return this.configuration;
            }
        }

        public Collection<MetadataConversionError> Errors
        {
            get
            {
                return this.errors;
            }
        }

        internal Dictionary<MessageDescription, CodeTypeReference> GeneratedTypedMessages
        {
            get
            {
                if (this.generatedTypedMessages == null)
                {
                    this.generatedTypedMessages = new Dictionary<MessageDescription, CodeTypeReference>(MessageDescriptionComparer.Singleton);
                }
                return this.generatedTypedMessages;
            }
        }

        internal NamespaceHelper NamespaceManager
        {
            get
            {
                return this.namespaceManager;
            }
        }

        public Dictionary<string, string> NamespaceMappings
        {
            get
            {
                return this.NamespaceManager.NamespaceMappings;
            }
        }

        public ServiceContractGenerationOptions Options
        {
            get
            {
                return this.options.Options;
            }
            set
            {
                this.options = new OptionsHelper(value);
            }
        }

        internal OptionsHelper OptionsInternal
        {
            get
            {
                return this.options;
            }
        }

        public Dictionary<ContractDescription, System.Type> ReferencedTypes
        {
            get
            {
                return this.referencedTypes;
            }
        }

        public CodeCompileUnit TargetCompileUnit
        {
            get
            {
                return this.compileUnit;
            }
        }



        private class ChannelInterfaceGenerator : IServiceContractGenerationExtension
        {
            void IServiceContractGenerationExtension.GenerateContract(ServiceContractGenerationContext context)
            {
                CodeTypeDeclaration codeType = context.TypeFactory.CreateInterfaceType();
                codeType.BaseTypes.Add(context.ContractTypeReference);
                codeType.BaseTypes.Add(context.ServiceContractGenerator.GetCodeTypeReference(typeof(IClientChannel)));
                new UniqueCodeNamespaceScope(context.Namespace).AddUnique(codeType, context.ContractType.Name + "Channel", "Channel");
            }
        }

        internal class CodeTypeFactory
        {
            private bool internalTypes;
            private ServiceContractGenerator parent;

            public CodeTypeFactory(ServiceContractGenerator parent, bool internalTypes)
            {
                this.parent = parent;
                this.internalTypes = internalTypes;
            }

            private void AddDebuggerStepThroughAttribute(CodeTypeDeclaration codeType)
            {
                if (codeType.IsClass)
                {
                    codeType.CustomAttributes.Add(new CodeAttributeDeclaration(this.parent.GetCodeTypeReference(typeof(DebuggerStepThroughAttribute))));
                }
            }

            private void AddGeneratedCodeAttribute(CodeTypeDeclaration codeType)
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(this.parent.GetCodeTypeReference(typeof(GeneratedCodeAttribute)));
                AssemblyName name = Assembly.GetExecutingAssembly().GetName();
                declaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(name.Name)));
                declaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(name.Version.ToString())));
                codeType.CustomAttributes.Add(declaration);
            }

            private void AddInternal(CodeTypeDeclaration codeType)
            {
                if (this.internalTypes)
                {
                    codeType.TypeAttributes &= ~TypeAttributes.Public;
                }
            }

            private void AddPartial(CodeTypeDeclaration codeType)
            {
                if (codeType.IsClass)
                {
                    codeType.IsPartial = true;
                }
            }

            public CodeTypeDeclaration CreateClassType()
            {
                return this.CreateCodeType(false);
            }

            private CodeTypeDeclaration CreateCodeType(bool isInterface)
            {
                CodeTypeDeclaration codeType = new CodeTypeDeclaration {
                    IsClass = !isInterface,
                    IsInterface = isInterface
                };
                this.RunDecorators(codeType);
                return codeType;
            }

            public CodeTypeDeclaration CreateInterfaceType()
            {
                return this.CreateCodeType(true);
            }

            private void RunDecorators(CodeTypeDeclaration codeType)
            {
                this.AddPartial(codeType);
                this.AddInternal(codeType);
                this.AddDebuggerStepThroughAttribute(codeType);
                this.AddGeneratedCodeAttribute(codeType);
            }
        }

        internal class ContextInitializer
        {
            private readonly bool asyncMethods;
            private UniqueCodeIdentifierScope callbackMemberScope;
            private ServiceContractGenerationContext context;
            private UniqueCodeIdentifierScope contractMemberScope;
            private readonly ServiceContractGenerator parent;
            private readonly ServiceContractGenerator.CodeTypeFactory typeFactory;

            internal ContextInitializer(ServiceContractGenerator parent, ServiceContractGenerator.CodeTypeFactory typeFactory)
            {
                this.parent = parent;
                this.typeFactory = typeFactory;
                this.asyncMethods = parent.OptionsInternal.IsSet(ServiceContractGenerationOptions.AsynchronousMethods);
            }

            private void AddOperationContractAttributes(OperationContractGenerationContext context)
            {
                if (context.SyncMethod != null)
                {
                    context.SyncMethod.CustomAttributes.Add(this.CreateOperationContractAttributeDeclaration(context.Operation, false));
                }
                if (context.BeginMethod != null)
                {
                    context.BeginMethod.CustomAttributes.Add(this.CreateOperationContractAttributeDeclaration(context.Operation, true));
                }
            }

            private void AddServiceContractAttribute(ServiceContractGenerationContext context)
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(context.ServiceContractGenerator.GetCodeTypeReference(typeof(ServiceContractAttribute)));
                if (context.ContractType.Name != context.Contract.CodeName)
                {
                    string str = (NamingHelper.XmlName(context.Contract.CodeName) == context.Contract.Name) ? context.Contract.CodeName : context.Contract.Name;
                    declaration.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(str)));
                }
                if ("http://tempuri.org/" != context.Contract.Namespace)
                {
                    declaration.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(context.Contract.Namespace)));
                }
                declaration.Arguments.Add(new CodeAttributeArgument("ConfigurationName", new CodePrimitiveExpression(ServiceContractGenerator.NamespaceHelper.GetCodeTypeReference(context.Namespace, context.ContractType).BaseType)));
                if (context.Contract.HasProtectionLevel)
                {
                    declaration.Arguments.Add(new CodeAttributeArgument("ProtectionLevel", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(ProtectionLevel)), context.Contract.ProtectionLevel.ToString())));
                }
                if (context.DuplexCallbackType != null)
                {
                    declaration.Arguments.Add(new CodeAttributeArgument("CallbackContract", new CodeTypeOfExpression(context.DuplexCallbackTypeReference)));
                }
                if (context.Contract.SessionMode != SessionMode.Allowed)
                {
                    declaration.Arguments.Add(new CodeAttributeArgument("SessionMode", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(SessionMode)), context.Contract.SessionMode.ToString())));
                }
                context.ContractType.CustomAttributes.Add(declaration);
            }

            public ServiceContractGenerationContext CreateContext(ContractDescription contractDescription)
            {
                this.VisitContract(contractDescription);
                return this.context;
            }

            private CodeAttributeDeclaration CreateOperationContractAttributeDeclaration(OperationDescription operationDescription, bool asyncPattern)
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(this.context.ServiceContractGenerator.GetCodeTypeReference(typeof(OperationContractAttribute)));
                if (operationDescription.IsOneWay)
                {
                    declaration.Arguments.Add(new CodeAttributeArgument("IsOneWay", new CodePrimitiveExpression(true)));
                }
                if ((operationDescription.DeclaringContract.SessionMode == SessionMode.Required) && operationDescription.IsTerminating)
                {
                    declaration.Arguments.Add(new CodeAttributeArgument("IsTerminating", new CodePrimitiveExpression(true)));
                }
                if ((operationDescription.DeclaringContract.SessionMode == SessionMode.Required) && !operationDescription.IsInitiating)
                {
                    declaration.Arguments.Add(new CodeAttributeArgument("IsInitiating", new CodePrimitiveExpression(false)));
                }
                if (asyncPattern)
                {
                    declaration.Arguments.Add(new CodeAttributeArgument("AsyncPattern", new CodePrimitiveExpression(true)));
                }
                if (operationDescription.HasProtectionLevel)
                {
                    declaration.Arguments.Add(new CodeAttributeArgument("ProtectionLevel", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(ProtectionLevel)), operationDescription.ProtectionLevel.ToString())));
                }
                return declaration;
            }

            private static bool IsDuplex(ContractDescription contract)
            {
                foreach (OperationDescription description in contract.Operations)
                {
                    if (description.IsServerInitiated())
                    {
                        return true;
                    }
                }
                return false;
            }

            private void Visit(ContractDescription contractDescription)
            {
                bool flag = IsDuplex(contractDescription);
                this.contractMemberScope = new UniqueCodeIdentifierScope();
                this.callbackMemberScope = flag ? new UniqueCodeIdentifierScope() : null;
                UniqueCodeNamespaceScope scope = new UniqueCodeNamespaceScope(this.parent.NamespaceManager.EnsureNamespace(contractDescription.Namespace));
                CodeTypeDeclaration codeType = this.typeFactory.CreateInterfaceType();
                CodeTypeReference reference = scope.AddUnique(codeType, contractDescription.CodeName, "IContract");
                CodeTypeDeclaration declaration2 = null;
                CodeTypeReference reference2 = null;
                if (flag)
                {
                    declaration2 = this.typeFactory.CreateInterfaceType();
                    reference2 = scope.AddUnique(declaration2, contractDescription.CodeName + "Callback", "IContract");
                }
                this.context = new ServiceContractGenerationContext(this.parent, contractDescription, codeType, declaration2);
                this.context.Namespace = scope.CodeNamespace;
                this.context.TypeFactory = this.typeFactory;
                this.context.ContractTypeReference = reference;
                this.context.DuplexCallbackTypeReference = reference2;
                this.AddServiceContractAttribute(this.context);
            }

            private void Visit(OperationDescription operationDescription)
            {
                OperationContractGenerationContext context;
                bool flag = operationDescription.IsServerInitiated();
                CodeTypeDeclaration declaringType = flag ? this.context.DuplexCallbackType : this.context.ContractType;
                string str = (flag ? this.callbackMemberScope : this.contractMemberScope).AddUnique(operationDescription.CodeName, "Method");
                CodeMemberMethod method = new CodeMemberMethod {
                    Name = str
                };
                declaringType.Members.Add(method);
                if (this.asyncMethods)
                {
                    CodeMemberMethod method2 = new CodeMemberMethod {
                        Name = "Begin" + str
                    };
                    method2.Parameters.Add(new CodeParameterDeclarationExpression(this.context.ServiceContractGenerator.GetCodeTypeReference(typeof(AsyncCallback)), "callback"));
                    method2.Parameters.Add(new CodeParameterDeclarationExpression(this.context.ServiceContractGenerator.GetCodeTypeReference(typeof(object)), "asyncState"));
                    method2.ReturnType = this.context.ServiceContractGenerator.GetCodeTypeReference(typeof(IAsyncResult));
                    declaringType.Members.Add(method2);
                    CodeMemberMethod method3 = new CodeMemberMethod {
                        Name = "End" + str
                    };
                    method3.Parameters.Add(new CodeParameterDeclarationExpression(this.context.ServiceContractGenerator.GetCodeTypeReference(typeof(IAsyncResult)), "result"));
                    declaringType.Members.Add(method3);
                    context = new OperationContractGenerationContext(this.parent, this.context, operationDescription, declaringType, method, method2, method3);
                }
                else
                {
                    context = new OperationContractGenerationContext(this.parent, this.context, operationDescription, declaringType, method);
                }
                context.DeclaringTypeReference = operationDescription.IsServerInitiated() ? this.context.DuplexCallbackTypeReference : this.context.ContractTypeReference;
                this.context.Operations.Add(context);
                this.AddOperationContractAttributes(context);
            }

            private void VisitContract(ContractDescription contract)
            {
                this.Visit(contract);
                foreach (OperationDescription description in contract.Operations)
                {
                    this.Visit(description);
                }
            }
        }

        internal static class ExtensionsHelper
        {
            internal static void CallBehaviorExtensions(ServiceContractGenerationContext context)
            {
                CallContractExtensions(EnumerateBehaviorExtensions(context.Contract), context);
                foreach (OperationContractGenerationContext context2 in context.Operations)
                {
                    CallOperationExtensions(EnumerateBehaviorExtensions(context2.Operation), context2);
                }
            }

            internal static void CallContractExtensions(IEnumerable<IServiceContractGenerationExtension> extensions, ServiceContractGenerationContext context)
            {
                foreach (IServiceContractGenerationExtension extension in extensions)
                {
                    extension.GenerateContract(context);
                }
            }

            private static void CallOperationExtensions(IEnumerable<IOperationContractGenerationExtension> extensions, OperationContractGenerationContext context)
            {
                foreach (IOperationContractGenerationExtension extension in extensions)
                {
                    extension.GenerateOperation(context);
                }
            }

            internal static void CallOperationExtensions(IEnumerable<IOperationContractGenerationExtension> extensions, ServiceContractGenerationContext context)
            {
                foreach (OperationContractGenerationContext context2 in context.Operations)
                {
                    CallOperationExtensions(extensions, context2);
                }
            }

            private static IEnumerable<IServiceContractGenerationExtension> EnumerateBehaviorExtensions(ContractDescription contract)
            {
                foreach (IContractBehavior iteratorVariable0 in contract.Behaviors)
                {
                    if (iteratorVariable0 is IServiceContractGenerationExtension)
                    {
                        yield return (IServiceContractGenerationExtension) iteratorVariable0;
                    }
                }
            }

            private static IEnumerable<IOperationContractGenerationExtension> EnumerateBehaviorExtensions(OperationDescription operation)
            {
                foreach (IOperationBehavior iteratorVariable0 in operation.Behaviors)
                {
                    if (iteratorVariable0 is IOperationContractGenerationExtension)
                    {
                        yield return (IOperationContractGenerationExtension) iteratorVariable0;
                    }
                }
            }


        }

        private class FaultContractAttributeGenerator : IOperationContractGenerationExtension
        {
            private static CodeTypeReference voidTypeReference = new CodeTypeReference(typeof(void));

            private static CodeAttributeDeclaration CreateAttrDecl(OperationContractGenerationContext context, FaultDescription fault)
            {
                CodeTypeReference type = (fault.DetailType != null) ? context.Contract.ServiceContractGenerator.GetCodeTypeReference(fault.DetailType) : fault.DetailTypeReference;
                if ((type == null) || (type == voidTypeReference))
                {
                    return null;
                }
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(context.ServiceContractGenerator.GetCodeTypeReference(typeof(FaultContractAttribute)));
                declaration.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(type)));
                if (fault.Action != null)
                {
                    declaration.Arguments.Add(new CodeAttributeArgument("Action", new CodePrimitiveExpression(fault.Action)));
                }
                if (fault.HasProtectionLevel)
                {
                    declaration.Arguments.Add(new CodeAttributeArgument("ProtectionLevel", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(ProtectionLevel)), fault.ProtectionLevel.ToString())));
                }
                if (!XmlName.IsNullOrEmpty(fault.ElementName))
                {
                    declaration.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(fault.ElementName.EncodedName)));
                }
                if (fault.Namespace != context.Contract.Contract.Namespace)
                {
                    declaration.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(fault.Namespace)));
                }
                return declaration;
            }

            void IOperationContractGenerationExtension.GenerateOperation(OperationContractGenerationContext context)
            {
                CodeMemberMethod method = context.SyncMethod ?? context.BeginMethod;
                foreach (FaultDescription description in context.Operation.Faults)
                {
                    CodeAttributeDeclaration declaration = CreateAttrDecl(context, description);
                    if (declaration != null)
                    {
                        method.CustomAttributes.Add(declaration);
                    }
                }
            }
        }

        private class MessageDescriptionComparer : IEqualityComparer<MessageDescription>
        {
            internal static ServiceContractGenerator.MessageDescriptionComparer Singleton = new ServiceContractGenerator.MessageDescriptionComparer();

            private MessageDescriptionComparer()
            {
            }

            bool IEqualityComparer<MessageDescription>.Equals(MessageDescription x, MessageDescription y)
            {
                if (x.XsdTypeName != y.XsdTypeName)
                {
                    return false;
                }
                if (x.Headers.Count != y.Headers.Count)
                {
                    return false;
                }
                MessageHeaderDescription[] array = new MessageHeaderDescription[x.Headers.Count];
                x.Headers.CopyTo(array, 0);
                MessageHeaderDescription[] descriptionArray2 = new MessageHeaderDescription[y.Headers.Count];
                y.Headers.CopyTo(descriptionArray2, 0);
                if (x.Headers.Count > 1)
                {
                    Array.Sort<MessagePartDescription>((MessagePartDescription[]) array, MessagePartDescriptionComparer.Singleton);
                    Array.Sort<MessagePartDescription>((MessagePartDescription[]) descriptionArray2, MessagePartDescriptionComparer.Singleton);
                }
                for (int i = 0; i < array.Length; i++)
                {
                    if (MessagePartDescriptionComparer.Singleton.Compare(array[i], descriptionArray2[i]) != 0)
                    {
                        return false;
                    }
                }
                return true;
            }

            int IEqualityComparer<MessageDescription>.GetHashCode(MessageDescription obj)
            {
                return obj.XsdTypeName.GetHashCode();
            }

            private class MessagePartDescriptionComparer : IComparer<MessagePartDescription>
            {
                internal static ServiceContractGenerator.MessageDescriptionComparer.MessagePartDescriptionComparer Singleton = new ServiceContractGenerator.MessageDescriptionComparer.MessagePartDescriptionComparer();

                private MessagePartDescriptionComparer()
                {
                }

                public int Compare(MessagePartDescription p1, MessagePartDescription p2)
                {
                    if (p1 == null)
                    {
                        if (p2 != null)
                        {
                            return -1;
                        }
                        return 0;
                    }
                    if (p2 == null)
                    {
                        return 1;
                    }
                    int num = string.CompareOrdinal(p1.Namespace, p2.Namespace);
                    if (num == 0)
                    {
                        num = string.CompareOrdinal(p1.Name, p2.Name);
                    }
                    return num;
                }
            }
        }

        internal class NamespaceHelper
        {
            private readonly CodeNamespaceCollection codeNamespaces;
            private Dictionary<string, string> namespaceMappings;
            private static readonly object referenceKey = new object();
            private const string WildcardNamespaceMapping = "*";

            public NamespaceHelper(CodeNamespaceCollection namespaces)
            {
                this.codeNamespaces = namespaces;
            }

            private string DescriptionToCode(string descriptionNamespace)
            {
                string str = string.Empty;
                if (((this.namespaceMappings != null) && !this.namespaceMappings.TryGetValue(descriptionNamespace, out str)) && !this.namespaceMappings.TryGetValue("*", out str))
                {
                    return string.Empty;
                }
                return str;
            }

            public CodeNamespace EnsureNamespace(string descriptionNamespace)
            {
                string ns = this.DescriptionToCode(descriptionNamespace);
                CodeNamespace namespace2 = this.FindNamespace(ns);
                if (namespace2 == null)
                {
                    namespace2 = new CodeNamespace(ns);
                    this.codeNamespaces.Add(namespace2);
                }
                return namespace2;
            }

            private CodeNamespace FindNamespace(string ns)
            {
                foreach (CodeNamespace namespace2 in this.codeNamespaces)
                {
                    if (namespace2.Name == ns)
                    {
                        return namespace2;
                    }
                }
                return null;
            }

            public static CodeTypeDeclaration GetCodeType(CodeTypeReference codeTypeReference)
            {
                return (codeTypeReference.UserData[referenceKey] as CodeTypeDeclaration);
            }

            internal static CodeTypeReference GetCodeTypeReference(CodeNamespace codeNamespace, CodeTypeDeclaration codeType)
            {
                CodeTypeReference reference = new CodeTypeReference(string.IsNullOrEmpty(codeNamespace.Name) ? codeType.Name : (codeNamespace.Name + '.' + codeType.Name));
                reference.UserData[referenceKey] = codeType;
                return reference;
            }

            public Dictionary<string, string> NamespaceMappings
            {
                get
                {
                    if (this.namespaceMappings == null)
                    {
                        this.namespaceMappings = new Dictionary<string, string>();
                    }
                    return this.namespaceMappings;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct OptionsHelper
        {
            public readonly ServiceContractGenerationOptions Options;
            public OptionsHelper(ServiceContractGenerationOptions options)
            {
                this.Options = options;
            }

            public bool IsSet(ServiceContractGenerationOptions option)
            {
                return ((this.Options & option) != ServiceContractGenerationOptions.None);
            }

            private static bool IsSingleBit(int x)
            {
                return ((x != 0) && ((x & (x + -1)) == 0));
            }
        }

        private static class Strings
        {
            public const string AsyncCallbackArgName = "callback";
            public const string AsyncResultArgName = "result";
            public const string AsyncStateArgName = "asyncState";
            public const string CallbackTypeSuffix = "Callback";
            public const string ChannelTypeSuffix = "Channel";
            public const string DefaultContractName = "IContract";
            public const string DefaultOperationName = "Method";
            public const string InterfaceTypePrefix = "I";
        }

        private class TransactionFlowAttributeGenerator : IOperationContractGenerationExtension
        {
            private static CodeAttributeDeclaration CreateAttrDecl(OperationContractGenerationContext context, TransactionFlowAttribute attr)
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(context.Contract.ServiceContractGenerator.GetCodeTypeReference(typeof(TransactionFlowAttribute)));
                declaration.Arguments.Add(new CodeAttributeArgument(ServiceContractGenerator.GetEnumReference<TransactionFlowOption>(attr.Transactions)));
                return declaration;
            }

            void IOperationContractGenerationExtension.GenerateOperation(OperationContractGenerationContext context)
            {
                TransactionFlowAttribute attr = context.Operation.Behaviors.Find<TransactionFlowAttribute>();
                if ((attr != null) && (attr.Transactions != TransactionFlowOption.NotAllowed))
                {
                    CodeMemberMethod method = context.SyncMethod ?? context.BeginMethod;
                    method.CustomAttributes.Add(CreateAttrDecl(context, attr));
                }
            }
        }
    }
}

