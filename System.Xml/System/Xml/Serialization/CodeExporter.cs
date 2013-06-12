namespace System.Xml.Serialization
{
    using Microsoft.CSharp;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Xml;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class CodeExporter
    {
        private System.CodeDom.CodeCompileUnit codeCompileUnit;
        private System.CodeDom.CodeNamespace codeNamespace;
        private CodeDomProvider codeProvider;
        private Hashtable exportedClasses;
        private Hashtable exportedMappings;
        private CodeAttributeDeclaration generatedCodeAttribute;
        private CodeAttributeDeclarationCollection includeMetadata = new CodeAttributeDeclarationCollection();
        private CodeGenerationOptions options;
        private bool rootExported;
        private TypeScope scope;

        internal CodeExporter(System.CodeDom.CodeNamespace codeNamespace, System.CodeDom.CodeCompileUnit codeCompileUnit, CodeDomProvider codeProvider, CodeGenerationOptions options, Hashtable exportedMappings)
        {
            if (codeNamespace != null)
            {
                CodeGenerator.ValidateIdentifiers(codeNamespace);
            }
            this.codeNamespace = codeNamespace;
            if (codeCompileUnit != null)
            {
                if (!codeCompileUnit.ReferencedAssemblies.Contains("System.dll"))
                {
                    codeCompileUnit.ReferencedAssemblies.Add("System.dll");
                }
                if (!codeCompileUnit.ReferencedAssemblies.Contains("System.Xml.dll"))
                {
                    codeCompileUnit.ReferencedAssemblies.Add("System.Xml.dll");
                }
            }
            this.codeCompileUnit = codeCompileUnit;
            this.options = options;
            this.exportedMappings = exportedMappings;
            this.codeProvider = codeProvider;
        }

        internal static void AddIncludeMetadata(CodeAttributeDeclarationCollection metadata, StructMapping mapping, Type type)
        {
            if (!mapping.IsAnonymousType)
            {
                for (StructMapping mapping2 = mapping.DerivedMappings; mapping2 != null; mapping2 = mapping2.NextDerivedMapping)
                {
                    CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(type.FullName);
                    declaration.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(mapping2.TypeDesc.FullName)));
                    metadata.Add(declaration);
                    AddIncludeMetadata(metadata, mapping2, type);
                }
            }
        }

        internal void AddPropertyChangedNotifier(CodeTypeDeclaration codeClass)
        {
            if (this.EnableDataBinding && (codeClass != null))
            {
                if (codeClass.BaseTypes.Count == 0)
                {
                    codeClass.BaseTypes.Add(typeof(object));
                }
                codeClass.BaseTypes.Add(new CodeTypeReference(typeof(INotifyPropertyChanged)));
                codeClass.Members.Add(PropertyChangedEvent);
                codeClass.Members.Add(RaisePropertyChangedEventMethod);
            }
        }

        internal void AddTypeMetadata(CodeAttributeDeclarationCollection metadata, Type type, string defaultName, string name, string ns, bool includeInSchema)
        {
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(type.FullName);
            if ((name == null) || (name.Length == 0))
            {
                declaration.Arguments.Add(new CodeAttributeArgument("AnonymousType", new CodePrimitiveExpression(true)));
            }
            else if (defaultName != name)
            {
                declaration.Arguments.Add(new CodeAttributeArgument("TypeName", new CodePrimitiveExpression(name)));
            }
            if ((ns != null) && (ns.Length != 0))
            {
                declaration.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(ns)));
            }
            if (!includeInSchema)
            {
                declaration.Arguments.Add(new CodeAttributeArgument("IncludeInSchema", new CodePrimitiveExpression(false)));
            }
            if (declaration.Arguments.Count > 0)
            {
                metadata.Add(declaration);
            }
        }

        internal static void AddWarningComment(CodeCommentStatementCollection comments, string text)
        {
            comments.Add(new CodeCommentStatement(Res.GetString("XmlCodegenWarningDetails", new object[] { text }), false));
        }

        internal void CheckScope(TypeScope scope)
        {
            if (this.scope == null)
            {
                this.scope = scope;
            }
            else if (this.scope != scope)
            {
                throw new InvalidOperationException(Res.GetString("XmlMappingsScopeMismatch"));
            }
        }

        internal CodeMemberProperty CreatePropertyDeclaration(CodeMemberField field, string name, string typeName)
        {
            CodeMemberProperty property;
            property = new CodeMemberProperty {
                Type = new CodeTypeReference(typeName),
                Name = name,
                Attributes = (property.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public
            };
            CodeMethodReturnStatement statement = new CodeMethodReturnStatement {
                Expression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name)
            };
            property.GetStatements.Add(statement);
            CodeAssignStatement statement2 = new CodeAssignStatement();
            CodeExpression expression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name);
            CodeExpression expression2 = new CodePropertySetValueReferenceExpression();
            statement2.Left = expression;
            statement2.Right = expression2;
            if (this.EnableDataBinding)
            {
                property.SetStatements.Add(statement2);
                property.SetStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), RaisePropertyChangedEventMethod.Name, new CodeExpression[] { new CodePrimitiveExpression(name) }));
                return property;
            }
            property.SetStatements.Add(statement2);
            return property;
        }

        internal abstract void EnsureTypesExported(Accessor[] accessors, string ns);
        internal static void ExportConstant(CodeTypeDeclaration codeClass, ConstantMapping constant, Type type, bool init, long enumValue)
        {
            CodeMemberField field = new CodeMemberField(typeof(int).FullName, constant.Name);
            field.Comments.Add(new CodeCommentStatement(Res.GetString("XmlRemarks"), true));
            if (init)
            {
                field.InitExpression = new CodePrimitiveExpression(enumValue);
            }
            codeClass.Members.Add(field);
            if (constant.XmlName != constant.Name)
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(type.FullName);
                declaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(constant.XmlName)));
                field.CustomAttributes.Add(declaration);
            }
        }

        internal abstract void ExportDerivedStructs(StructMapping mapping);
        internal CodeTypeDeclaration ExportEnum(EnumMapping mapping, Type type)
        {
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(mapping.TypeDesc.Name);
            declaration.Comments.Add(new CodeCommentStatement(Res.GetString("XmlRemarks"), true));
            declaration.IsEnum = true;
            if (mapping.IsFlags && (mapping.Constants.Length > 0x1f))
            {
                declaration.BaseTypes.Add(new CodeTypeReference(typeof(long)));
            }
            declaration.TypeAttributes |= TypeAttributes.Public;
            this.CodeNamespace.Types.Add(declaration);
            for (int i = 0; i < mapping.Constants.Length; i++)
            {
                ExportConstant(declaration, mapping.Constants[i], type, mapping.IsFlags, ((long) 1L) << i);
            }
            if (mapping.IsFlags)
            {
                CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration(typeof(FlagsAttribute).FullName);
                declaration.CustomAttributes.Add(declaration2);
            }
            CodeGenerator.ValidateIdentifiers(declaration);
            return declaration;
        }

        internal void ExportRoot(StructMapping mapping, Type includeType)
        {
            if (!this.rootExported)
            {
                this.rootExported = true;
                this.ExportDerivedStructs(mapping);
                for (StructMapping mapping2 = mapping.DerivedMappings; mapping2 != null; mapping2 = mapping2.NextDerivedMapping)
                {
                    if ((!mapping2.ReferencedByElement && mapping2.IncludeInSchema) && !mapping2.IsAnonymousType)
                    {
                        CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(includeType.FullName);
                        declaration.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(mapping2.TypeDesc.FullName)));
                        this.includeMetadata.Add(declaration);
                    }
                }
                Hashtable hashtable = new Hashtable();
                foreach (TypeMapping mapping3 in this.Scope.TypeMappings)
                {
                    if (mapping3 is ArrayMapping)
                    {
                        ArrayMapping arrayMapping = (ArrayMapping) mapping3;
                        if (ShouldInclude(arrayMapping) && !hashtable.Contains(arrayMapping.TypeDesc.FullName))
                        {
                            CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration(includeType.FullName);
                            declaration2.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(arrayMapping.TypeDesc.FullName)));
                            this.includeMetadata.Add(declaration2);
                            hashtable.Add(arrayMapping.TypeDesc.FullName, string.Empty);
                            this.EnsureTypesExported(arrayMapping.Elements, arrayMapping.Namespace);
                        }
                    }
                }
            }
        }

        internal static CodeAttributeDeclaration FindAttributeDeclaration(Type type, CodeAttributeDeclarationCollection metadata)
        {
            foreach (CodeAttributeDeclaration declaration in metadata)
            {
                if ((declaration.Name == type.FullName) || (declaration.Name == type.Name))
                {
                    return declaration;
                }
            }
            return null;
        }

        private static string GetProductVersion(Assembly assembly)
        {
            object[] customAttributes = assembly.GetCustomAttributes(true);
            for (int i = 0; i < customAttributes.Length; i++)
            {
                if (customAttributes[i] is AssemblyInformationalVersionAttribute)
                {
                    AssemblyInformationalVersionAttribute attribute = (AssemblyInformationalVersionAttribute) customAttributes[i];
                    return attribute.InformationalVersion;
                }
            }
            return null;
        }

        internal static string MakeFieldName(string name)
        {
            return (CodeIdentifier.MakeCamel(name) + "Field");
        }

        internal static object PromoteType(Type type, object value)
        {
            if (type == typeof(sbyte))
            {
                return ((IConvertible) value).ToInt16(null);
            }
            if (type == typeof(ushort))
            {
                return ((IConvertible) value).ToInt32(null);
            }
            if (type == typeof(uint))
            {
                return ((IConvertible) value).ToInt64(null);
            }
            if (type == typeof(ulong))
            {
                return ((IConvertible) value).ToDecimal(null);
            }
            return value;
        }

        private static bool ShouldInclude(ArrayMapping arrayMapping)
        {
            if (arrayMapping.ReferencedByElement)
            {
                return false;
            }
            if (arrayMapping.Next != null)
            {
                return false;
            }
            if ((arrayMapping.Elements.Length == 1) && (arrayMapping.Elements[0].Mapping.TypeDesc.Kind == TypeKind.Node))
            {
                return false;
            }
            for (int i = 0; i < arrayMapping.Elements.Length; i++)
            {
                if (arrayMapping.Elements[i].Name != arrayMapping.Elements[i].Mapping.DefaultElementName)
                {
                    return false;
                }
            }
            return true;
        }

        internal System.CodeDom.CodeCompileUnit CodeCompileUnit
        {
            get
            {
                return this.codeCompileUnit;
            }
        }

        internal System.CodeDom.CodeNamespace CodeNamespace
        {
            get
            {
                if (this.codeNamespace == null)
                {
                    this.codeNamespace = new System.CodeDom.CodeNamespace();
                }
                return this.codeNamespace;
            }
        }

        internal CodeDomProvider CodeProvider
        {
            get
            {
                if (this.codeProvider == null)
                {
                    this.codeProvider = new CSharpCodeProvider();
                }
                return this.codeProvider;
            }
        }

        private bool EnableDataBinding
        {
            get
            {
                return ((this.options & CodeGenerationOptions.EnableDataBinding) != CodeGenerationOptions.None);
            }
        }

        internal Hashtable ExportedClasses
        {
            get
            {
                if (this.exportedClasses == null)
                {
                    this.exportedClasses = new Hashtable();
                }
                return this.exportedClasses;
            }
        }

        internal Hashtable ExportedMappings
        {
            get
            {
                if (this.exportedMappings == null)
                {
                    this.exportedMappings = new Hashtable();
                }
                return this.exportedMappings;
            }
        }

        internal CodeAttributeDeclaration GeneratedCodeAttribute
        {
            get
            {
                if (this.generatedCodeAttribute == null)
                {
                    CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute).FullName);
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly == null)
                    {
                        entryAssembly = Assembly.GetExecutingAssembly();
                        if (entryAssembly == null)
                        {
                            entryAssembly = typeof(CodeExporter).Assembly;
                        }
                    }
                    AssemblyName name = entryAssembly.GetName();
                    declaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(name.Name)));
                    string productVersion = GetProductVersion(entryAssembly);
                    declaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression((productVersion == null) ? name.Version.ToString() : productVersion)));
                    this.generatedCodeAttribute = declaration;
                }
                return this.generatedCodeAttribute;
            }
        }

        internal bool GenerateProperties
        {
            get
            {
                return ((this.options & CodeGenerationOptions.GenerateProperties) != CodeGenerationOptions.None);
            }
        }

        public CodeAttributeDeclarationCollection IncludeMetadata
        {
            get
            {
                return this.includeMetadata;
            }
        }

        internal static CodeMemberEvent PropertyChangedEvent
        {
            get
            {
                CodeMemberEvent event2 = new CodeMemberEvent {
                    Attributes = MemberAttributes.Public,
                    Name = "PropertyChanged",
                    Type = new CodeTypeReference(typeof(PropertyChangedEventHandler))
                };
                event2.ImplementationTypes.Add(typeof(INotifyPropertyChanged));
                return event2;
            }
        }

        internal static CodeMemberMethod RaisePropertyChangedEventMethod
        {
            get
            {
                CodeMemberMethod method = new CodeMemberMethod {
                    Name = "RaisePropertyChanged",
                    Attributes = MemberAttributes.Family | MemberAttributes.Final
                };
                CodeArgumentReferenceExpression expression = new CodeArgumentReferenceExpression("propertyName");
                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), expression.ParameterName));
                CodeVariableReferenceExpression left = new CodeVariableReferenceExpression("propertyChanged");
                method.Statements.Add(new CodeVariableDeclarationStatement(typeof(PropertyChangedEventHandler), left.VariableName, new CodeEventReferenceExpression(new CodeThisReferenceExpression(), PropertyChangedEvent.Name)));
                CodeConditionStatement statement = new CodeConditionStatement(new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)), new CodeStatement[0]);
                method.Statements.Add(statement);
                statement.TrueStatements.Add(new CodeDelegateInvokeExpression(left, new CodeExpression[] { new CodeThisReferenceExpression(), new CodeObjectCreateExpression(typeof(PropertyChangedEventArgs), new CodeExpression[] { expression }) }));
                return method;
            }
        }

        internal TypeScope Scope
        {
            get
            {
                return this.scope;
            }
        }
    }
}

