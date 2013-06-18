namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    internal class DataContractSerializerOperationGenerator : IOperationBehavior, IOperationContractGenerationExtension
    {
        private CodeCompileUnit codeCompileUnit;
        private Dictionary<MessagePartDescription, bool> isNonNillableReferenceTypes;
        private Dictionary<MessagePartDescription, ICollection<CodeTypeReference>> knownTypes;
        private Dictionary<OperationDescription, DataContractFormatAttribute> operationAttributes;
        private System.ServiceModel.Description.OperationGenerator operationGenerator;

        public DataContractSerializerOperationGenerator() : this(new CodeCompileUnit())
        {
        }

        public DataContractSerializerOperationGenerator(CodeCompileUnit codeCompileUnit)
        {
            this.operationAttributes = new Dictionary<OperationDescription, DataContractFormatAttribute>();
            this.codeCompileUnit = codeCompileUnit;
            this.operationGenerator = new System.ServiceModel.Description.OperationGenerator();
        }

        internal void Add(MessagePartDescription part, CodeTypeReference typeReference, ICollection<CodeTypeReference> knownTypeReferences, bool isNonNillableReferenceType)
        {
            this.OperationGenerator.ParameterTypes.Add(part, typeReference);
            if (knownTypeReferences != null)
            {
                this.KnownTypes.Add(part, knownTypeReferences);
            }
            if (isNonNillableReferenceType)
            {
                if (this.isNonNillableReferenceTypes == null)
                {
                    this.isNonNillableReferenceTypes = new Dictionary<MessagePartDescription, bool>();
                }
                this.isNonNillableReferenceTypes.Add(part, isNonNillableReferenceType);
            }
        }

        private void AddKnownTypesForPart(OperationContractGenerationContext context, MessagePartDescription part, Dictionary<CodeTypeReference, object> operationKnownTypes)
        {
            ICollection<CodeTypeReference> is2;
            if (this.knownTypes.TryGetValue(part, out is2))
            {
                foreach (CodeTypeReference reference in is2)
                {
                    object obj2;
                    if (!operationKnownTypes.TryGetValue(reference, out obj2))
                    {
                        operationKnownTypes.Add(reference, null);
                        CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(typeof(ServiceKnownTypeAttribute).FullName);
                        declaration.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(reference)));
                        context.SyncMethod.CustomAttributes.Add(declaration);
                    }
                }
            }
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
            DataContractSerializerOperationBehavior behavior = context.Operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
            DataContractFormatAttribute attribute = (behavior == null) ? new DataContractFormatAttribute() : behavior.DataContractFormatAttribute;
            OperationFormatStyle style = attribute.Style;
            this.operationGenerator.GenerateOperation(context, ref style, false, new WrappedBodyTypeGenerator(this, context), this.knownTypes);
            attribute.Style = style;
            if (attribute.Style != TypeLoader.DefaultDataContractFormatAttribute.Style)
            {
                context.SyncMethod.CustomAttributes.Add(System.ServiceModel.Description.OperationGenerator.GenerateAttributeDeclaration(context.Contract.ServiceContractGenerator, attribute));
            }
            if (this.knownTypes != null)
            {
                Dictionary<CodeTypeReference, object> operationKnownTypes = new Dictionary<CodeTypeReference, object>(new CodeTypeReferenceComparer());
                foreach (MessageDescription description in context.Operation.Messages)
                {
                    foreach (MessagePartDescription description2 in description.Body.Parts)
                    {
                        this.AddKnownTypesForPart(context, description2, operationKnownTypes);
                    }
                    foreach (MessageHeaderDescription description3 in description.Headers)
                    {
                        this.AddKnownTypesForPart(context, description3, operationKnownTypes);
                    }
                    if (OperationFormatter.IsValidReturnValue(description.Body.ReturnValue))
                    {
                        this.AddKnownTypesForPart(context, description.Body.ReturnValue, operationKnownTypes);
                    }
                }
            }
            UpdateTargetCompileUnit(context, this.codeCompileUnit);
        }

        internal static void UpdateTargetCompileUnit(OperationContractGenerationContext context, CodeCompileUnit codeCompileUnit)
        {
            CodeCompileUnit targetCompileUnit = context.ServiceContractGenerator.TargetCompileUnit;
            if (!object.ReferenceEquals(targetCompileUnit, codeCompileUnit))
            {
                foreach (CodeNamespace namespace2 in codeCompileUnit.Namespaces)
                {
                    if (!targetCompileUnit.Namespaces.Contains(namespace2))
                    {
                        targetCompileUnit.Namespaces.Add(namespace2);
                    }
                }
                foreach (string str in codeCompileUnit.ReferencedAssemblies)
                {
                    if (!targetCompileUnit.ReferencedAssemblies.Contains(str))
                    {
                        targetCompileUnit.ReferencedAssemblies.Add(str);
                    }
                }
                foreach (CodeAttributeDeclaration declaration in codeCompileUnit.AssemblyCustomAttributes)
                {
                    if (!targetCompileUnit.AssemblyCustomAttributes.Contains(declaration))
                    {
                        targetCompileUnit.AssemblyCustomAttributes.Add(declaration);
                    }
                }
                foreach (CodeDirective directive in codeCompileUnit.StartDirectives)
                {
                    if (!targetCompileUnit.StartDirectives.Contains(directive))
                    {
                        targetCompileUnit.StartDirectives.Add(directive);
                    }
                }
                foreach (CodeDirective directive2 in codeCompileUnit.EndDirectives)
                {
                    if (!targetCompileUnit.EndDirectives.Contains(directive2))
                    {
                        targetCompileUnit.EndDirectives.Add(directive2);
                    }
                }
                foreach (DictionaryEntry entry in codeCompileUnit.UserData)
                {
                    targetCompileUnit.UserData[entry.Key] = entry.Value;
                }
            }
        }

        internal Dictionary<MessagePartDescription, ICollection<CodeTypeReference>> KnownTypes
        {
            get
            {
                if (this.knownTypes == null)
                {
                    this.knownTypes = new Dictionary<MessagePartDescription, ICollection<CodeTypeReference>>();
                }
                return this.knownTypes;
            }
        }

        internal Dictionary<OperationDescription, DataContractFormatAttribute> OperationAttributes
        {
            get
            {
                return this.operationAttributes;
            }
        }

        internal System.ServiceModel.Description.OperationGenerator OperationGenerator
        {
            get
            {
                return this.operationGenerator;
            }
        }

        private class CodeTypeReferenceComparer : IEqualityComparer<CodeTypeReference>
        {
            public bool Equals(CodeTypeReference x, CodeTypeReference y)
            {
                if (!object.ReferenceEquals(x, y))
                {
                    if (((x == null) || (y == null)) || ((x.ArrayRank != y.ArrayRank) || (x.BaseType != y.BaseType)))
                    {
                        return false;
                    }
                    CodeTypeReferenceCollection typeArguments = x.TypeArguments;
                    CodeTypeReferenceCollection references2 = y.TypeArguments;
                    if (references2.Count == typeArguments.Count)
                    {
                        foreach (CodeTypeReference reference in typeArguments)
                        {
                            using (IEnumerator enumerator2 = references2.GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    CodeTypeReference current = (CodeTypeReference) enumerator2.Current;
                                    if (!this.Equals(reference, reference))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }

            public int GetHashCode(CodeTypeReference obj)
            {
                return obj.GetHashCode();
            }
        }

        internal class WrappedBodyTypeGenerator : IWrappedBodyTypeGenerator
        {
            private OperationContractGenerationContext context;
            private static CodeTypeReference dataContractAttributeTypeRef = new CodeTypeReference(typeof(DataContractAttribute));
            private DataContractSerializerOperationGenerator dataContractSerializerOperationGenerator;
            private int memberCount;

            public WrappedBodyTypeGenerator(DataContractSerializerOperationGenerator dataContractSerializerOperationGenerator, OperationContractGenerationContext context)
            {
                this.context = context;
                this.dataContractSerializerOperationGenerator = dataContractSerializerOperationGenerator;
            }

            public void AddMemberAttributes(XmlName messageName, MessagePartDescription part, CodeAttributeDeclarationCollection attributesImported, CodeAttributeDeclarationCollection typeAttributes, CodeAttributeDeclarationCollection fieldAttributes)
            {
                CodeAttributeDeclaration declaration = null;
                foreach (CodeAttributeDeclaration declaration2 in typeAttributes)
                {
                    if (declaration2.AttributeType.BaseType == dataContractAttributeTypeRef.BaseType)
                    {
                        declaration = declaration2;
                        break;
                    }
                }
                if (declaration == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(string.Format(CultureInfo.InvariantCulture, "Cannot find DataContract attribute for  {0}", new object[] { messageName })));
                }
                bool flag = false;
                foreach (CodeAttributeArgument argument in declaration.Arguments)
                {
                    if (argument.Name == "Namespace")
                    {
                        flag = true;
                        if (((CodePrimitiveExpression) argument.Value).Value.ToString() != part.Namespace)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxWrapperTypeHasMultipleNamespaces", new object[] { messageName })));
                        }
                    }
                }
                if (!flag)
                {
                    declaration.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(part.Namespace)));
                }
                DataMemberAttribute attribute = new DataMemberAttribute {
                    Order = this.memberCount++,
                    EmitDefaultValue = !this.IsNonNillableReferenceType(part)
                };
                fieldAttributes.Add(OperationGenerator.GenerateAttributeDeclaration(this.context.Contract.ServiceContractGenerator, attribute));
            }

            public void AddTypeAttributes(string messageName, string typeNS, CodeAttributeDeclarationCollection typeAttributes, bool isEncoded)
            {
                typeAttributes.Add(OperationGenerator.GenerateAttributeDeclaration(this.context.Contract.ServiceContractGenerator, new DataContractAttribute()));
                this.memberCount = 0;
            }

            private bool IsNonNillableReferenceType(MessagePartDescription part)
            {
                if (this.dataContractSerializerOperationGenerator.isNonNillableReferenceTypes == null)
                {
                    return false;
                }
                return this.dataContractSerializerOperationGenerator.isNonNillableReferenceTypes.ContainsKey(part);
            }

            private void ValidateForParameterMode(MessagePartDescription part)
            {
                if (this.dataContractSerializerOperationGenerator.isNonNillableReferenceTypes.ContainsKey(part))
                {
                    ParameterModeException exception = new ParameterModeException(System.ServiceModel.SR.GetString("SFxCannotImportAsParameters_ElementIsNotNillable", new object[] { part.Name, part.Namespace })) {
                        MessageContractType = MessageContractType.BareMessageContract
                    };
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                }
            }

            public void ValidateForParameterMode(OperationDescription operation)
            {
                if (this.dataContractSerializerOperationGenerator.isNonNillableReferenceTypes != null)
                {
                    foreach (MessageDescription description in operation.Messages)
                    {
                        if (description.Body != null)
                        {
                            if (description.Body.ReturnValue != null)
                            {
                                this.ValidateForParameterMode(description.Body.ReturnValue);
                            }
                            foreach (MessagePartDescription description2 in description.Body.Parts)
                            {
                                this.ValidateForParameterMode(description2);
                            }
                        }
                    }
                }
            }
        }
    }
}

