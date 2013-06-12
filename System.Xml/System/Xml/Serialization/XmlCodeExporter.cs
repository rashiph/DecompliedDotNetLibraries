namespace System.Xml.Serialization
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization.Advanced;

    public class XmlCodeExporter : CodeExporter
    {
        public XmlCodeExporter(CodeNamespace codeNamespace) : base(codeNamespace, null, null, CodeGenerationOptions.GenerateProperties, null)
        {
        }

        public XmlCodeExporter(CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit) : base(codeNamespace, codeCompileUnit, null, CodeGenerationOptions.GenerateProperties, null)
        {
        }

        public XmlCodeExporter(CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit, CodeGenerationOptions options) : base(codeNamespace, codeCompileUnit, null, options, null)
        {
        }

        public XmlCodeExporter(CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit, CodeGenerationOptions options, Hashtable mappings) : base(codeNamespace, codeCompileUnit, null, options, mappings)
        {
        }

        public XmlCodeExporter(CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit, CodeDomProvider codeProvider, CodeGenerationOptions options, Hashtable mappings) : base(codeNamespace, codeCompileUnit, codeProvider, options, mappings)
        {
        }

        private void AddDefaultValueAttribute(CodeMemberField field, CodeAttributeDeclarationCollection metadata, object defaultValue, TypeMapping mapping, CodeCommentStatementCollection comments, TypeDesc memberTypeDesc, Accessor accessor, CodeConstructor ctor)
        {
            string str = accessor.IsFixed ? "fixed" : "default";
            if (!memberTypeDesc.HasDefaultSupport)
            {
                if ((comments != null) && (defaultValue is string))
                {
                    DropDefaultAttribute(accessor, comments, memberTypeDesc.FullName);
                    CodeExporter.AddWarningComment(comments, Res.GetString("XmlDropAttributeValue", new object[] { str, mapping.TypeName, defaultValue.ToString() }));
                }
            }
            else if (memberTypeDesc.IsArrayLike && (accessor is ElementAccessor))
            {
                if ((comments != null) && (defaultValue is string))
                {
                    DropDefaultAttribute(accessor, comments, memberTypeDesc.FullName);
                    CodeExporter.AddWarningComment(comments, Res.GetString("XmlDropArrayAttributeValue", new object[] { str, defaultValue.ToString(), ((ElementAccessor) accessor).Name }));
                }
            }
            else if ((mapping.TypeDesc.IsMappedType && (field != null)) && (defaultValue is string))
            {
                SchemaImporterExtension extension = mapping.TypeDesc.ExtendedType.Extension;
                CodeExpression init = extension.ImportDefaultValue((string) defaultValue, mapping.TypeDesc.FullName);
                if (init != null)
                {
                    if (ctor != null)
                    {
                        AddInitializationStatement(ctor, field, init);
                    }
                    else
                    {
                        field.InitExpression = extension.ImportDefaultValue((string) defaultValue, mapping.TypeDesc.FullName);
                    }
                }
                if (comments != null)
                {
                    DropDefaultAttribute(accessor, comments, mapping.TypeDesc.FullName);
                    if (init == null)
                    {
                        CodeExporter.AddWarningComment(comments, Res.GetString("XmlNotKnownDefaultValue", new object[] { extension.GetType().FullName, str, (string) defaultValue, mapping.TypeName, mapping.Namespace }));
                    }
                }
            }
            else
            {
                object obj2 = null;
                if ((defaultValue is string) || (defaultValue == null))
                {
                    obj2 = this.ImportDefault(mapping, (string) defaultValue);
                }
                if (obj2 != null)
                {
                    if (!(mapping is PrimitiveMapping))
                    {
                        DropDefaultAttribute(accessor, comments, memberTypeDesc.FullName);
                        CodeExporter.AddWarningComment(comments, Res.GetString("XmlDropNonPrimitiveAttributeValue", new object[] { str, defaultValue.ToString() }));
                    }
                    else
                    {
                        PrimitiveMapping mapping2 = (PrimitiveMapping) mapping;
                        if (((comments != null) && !mapping2.TypeDesc.HasDefaultSupport) && mapping2.TypeDesc.IsMappedType)
                        {
                            DropDefaultAttribute(accessor, comments, mapping2.TypeDesc.FullName);
                        }
                        else if (obj2 == DBNull.Value)
                        {
                            if (comments != null)
                            {
                                CodeExporter.AddWarningComment(comments, Res.GetString("XmlDropAttributeValue", new object[] { str, mapping2.TypeName, defaultValue.ToString() }));
                            }
                        }
                        else
                        {
                            CodeAttributeArgument[] arguments = null;
                            CodeExpression initExpression = null;
                            if (mapping2.IsList)
                            {
                                object[] objArray = (object[]) obj2;
                                CodeExpression[] initializers = new CodeExpression[objArray.Length];
                                for (int i = 0; i < objArray.Length; i++)
                                {
                                    this.GetDefaultValueArguments(mapping2, objArray[i], out initializers[i]);
                                }
                                initExpression = new CodeArrayCreateExpression(field.Type, initializers);
                            }
                            else
                            {
                                arguments = this.GetDefaultValueArguments(mapping2, obj2, out initExpression);
                            }
                            if (field != null)
                            {
                                if (ctor != null)
                                {
                                    AddInitializationStatement(ctor, field, initExpression);
                                }
                                else
                                {
                                    field.InitExpression = initExpression;
                                }
                            }
                            if (((arguments != null) && mapping2.TypeDesc.HasDefaultSupport) && (accessor.IsOptional && !accessor.IsFixed))
                            {
                                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(typeof(DefaultValueAttribute).FullName, arguments);
                                metadata.Add(declaration);
                            }
                            else if (comments != null)
                            {
                                DropDefaultAttribute(accessor, comments, memberTypeDesc.FullName);
                            }
                        }
                    }
                }
            }
        }

        private static void AddInitializationStatement(CodeConstructor ctor, CodeMemberField field, CodeExpression init)
        {
            CodeAssignStatement statement = new CodeAssignStatement {
                Left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name),
                Right = init
            };
            ctor.Statements.Add(statement);
        }

        public void AddMappingMetadata(CodeAttributeDeclarationCollection metadata, XmlMemberMapping member, string ns)
        {
            this.AddMemberMetadata(null, metadata, member.Mapping, ns, false, null, null);
        }

        public void AddMappingMetadata(CodeAttributeDeclarationCollection metadata, XmlTypeMapping mapping, string ns)
        {
            mapping.CheckShallow();
            base.CheckScope(mapping.Scope);
            if (!(mapping.Mapping is StructMapping) && !(mapping.Mapping is EnumMapping))
            {
                this.AddRootMetadata(metadata, mapping.Mapping, Accessor.UnescapeName(mapping.Accessor.Name), mapping.Accessor.Namespace, mapping.Accessor);
            }
        }

        public void AddMappingMetadata(CodeAttributeDeclarationCollection metadata, XmlMemberMapping member, string ns, bool forceUseMemberName)
        {
            this.AddMemberMetadata(null, metadata, member.Mapping, ns, forceUseMemberName, null, null);
        }

        private void AddMemberMetadata(CodeMemberField field, CodeAttributeDeclarationCollection metadata, MemberMapping member, string ns, bool forceUseMemberName, CodeCommentStatementCollection comments, CodeConstructor ctor)
        {
            if (member.Xmlns != null)
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(typeof(XmlNamespaceDeclarationsAttribute).FullName);
                metadata.Add(declaration);
            }
            else if (member.Attribute != null)
            {
                AttributeAccessor attribute = member.Attribute;
                if (attribute.Any)
                {
                    this.ExportAnyAttribute(metadata);
                }
                else
                {
                    TypeMapping mapping = attribute.Mapping;
                    string str = Accessor.UnescapeName(attribute.Name);
                    bool flag = (mapping.TypeDesc == member.TypeDesc) || (member.TypeDesc.IsArrayLike && (mapping.TypeDesc == member.TypeDesc.ArrayElementTypeDesc));
                    bool flag2 = (str == member.Name) && !forceUseMemberName;
                    bool flag3 = attribute.Namespace == ns;
                    bool flag4 = attribute.Form != XmlSchemaForm.Qualified;
                    this.ExportAttribute(metadata, flag2 ? null : str, (flag3 || flag4) ? null : attribute.Namespace, flag ? null : mapping.TypeDesc, mapping.TypeDesc, flag4 ? XmlSchemaForm.None : attribute.Form);
                    this.AddDefaultValueAttribute(field, metadata, attribute.Default, mapping, comments, member.TypeDesc, attribute, ctor);
                }
            }
            else
            {
                if (member.Text != null)
                {
                    TypeMapping mapping2 = member.Text.Mapping;
                    bool flag5 = (mapping2.TypeDesc == member.TypeDesc) || (member.TypeDesc.IsArrayLike && (mapping2.TypeDesc == member.TypeDesc.ArrayElementTypeDesc));
                    this.ExportText(metadata, flag5 ? null : mapping2.TypeDesc, mapping2.TypeDesc.IsAmbiguousDataType ? mapping2.TypeDesc.DataType.Name : null);
                }
                if (member.Elements.Length == 1)
                {
                    ElementAccessor accessor = member.Elements[0];
                    TypeMapping mapping3 = accessor.Mapping;
                    string name = Accessor.UnescapeName(accessor.Name);
                    bool flag6 = (name == member.Name) && !forceUseMemberName;
                    bool flag7 = mapping3 is ArrayMapping;
                    bool flag8 = accessor.Namespace == ns;
                    bool flag9 = accessor.Form != XmlSchemaForm.Unqualified;
                    if (accessor.Any)
                    {
                        this.ExportAnyElement(metadata, name, accessor.Namespace, member.SequenceId);
                    }
                    else if (flag7)
                    {
                        TypeDesc typeDesc = mapping3.TypeDesc;
                        TypeDesc desc2 = member.TypeDesc;
                        ArrayMapping array = (ArrayMapping) mapping3;
                        if ((!flag6 || !flag8) || ((accessor.IsNullable || !flag9) || (member.SequenceId != -1)))
                        {
                            this.ExportArray(metadata, flag6 ? null : name, flag8 ? null : accessor.Namespace, accessor.IsNullable, flag9 ? XmlSchemaForm.None : accessor.Form, member.SequenceId);
                        }
                        else if (mapping3.TypeDesc.ArrayElementTypeDesc == new TypeScope().GetTypeDesc(typeof(byte)))
                        {
                            this.ExportArray(metadata, null, null, false, XmlSchemaForm.None, member.SequenceId);
                        }
                        this.ExportArrayElements(metadata, array, accessor.Namespace, member.TypeDesc.ArrayElementTypeDesc, 0);
                    }
                    else
                    {
                        bool flag10 = (mapping3.TypeDesc == member.TypeDesc) || (member.TypeDesc.IsArrayLike && (mapping3.TypeDesc == member.TypeDesc.ArrayElementTypeDesc));
                        if (member.TypeDesc.IsArrayLike)
                        {
                            flag6 = false;
                        }
                        this.ExportElement(metadata, flag6 ? null : name, flag8 ? null : accessor.Namespace, flag10 ? null : mapping3.TypeDesc, mapping3.TypeDesc, accessor.IsNullable, flag9 ? XmlSchemaForm.None : accessor.Form, member.SequenceId);
                    }
                    this.AddDefaultValueAttribute(field, metadata, accessor.Default, mapping3, comments, member.TypeDesc, accessor, ctor);
                }
                else
                {
                    for (int i = 0; i < member.Elements.Length; i++)
                    {
                        ElementAccessor accessor3 = member.Elements[i];
                        string str3 = Accessor.UnescapeName(accessor3.Name);
                        bool flag11 = accessor3.Namespace == ns;
                        if (accessor3.Any)
                        {
                            this.ExportAnyElement(metadata, str3, accessor3.Namespace, member.SequenceId);
                        }
                        else
                        {
                            bool flag12 = accessor3.Form != XmlSchemaForm.Unqualified;
                            this.ExportElement(metadata, str3, flag11 ? null : accessor3.Namespace, accessor3.Mapping.TypeDesc, accessor3.Mapping.TypeDesc, accessor3.IsNullable, flag12 ? XmlSchemaForm.None : accessor3.Form, member.SequenceId);
                        }
                    }
                }
                if (member.ChoiceIdentifier != null)
                {
                    CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration(typeof(XmlChoiceIdentifierAttribute).FullName);
                    declaration2.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(member.ChoiceIdentifier.MemberName)));
                    metadata.Add(declaration2);
                }
                if (member.Ignore)
                {
                    CodeAttributeDeclaration declaration3 = new CodeAttributeDeclaration(typeof(XmlIgnoreAttribute).FullName);
                    metadata.Add(declaration3);
                }
            }
        }

        private void AddRootMetadata(CodeAttributeDeclarationCollection metadata, TypeMapping typeMapping, string name, string ns, ElementAccessor rootElement)
        {
            string fullName = typeof(XmlRootAttribute).FullName;
            foreach (CodeAttributeDeclaration declaration in metadata)
            {
                if (declaration.Name == fullName)
                {
                    return;
                }
            }
            CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration(fullName);
            if (typeMapping.TypeDesc.Name != name)
            {
                declaration2.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(name)));
            }
            if (ns != null)
            {
                declaration2.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(ns)));
            }
            if ((typeMapping.TypeDesc != null) && typeMapping.TypeDesc.IsAmbiguousDataType)
            {
                declaration2.Arguments.Add(new CodeAttributeArgument("DataType", new CodePrimitiveExpression(typeMapping.TypeDesc.DataType.Name)));
            }
            if (rootElement.IsNullable)
            {
                declaration2.Arguments.Add(new CodeAttributeArgument("IsNullable", new CodePrimitiveExpression(rootElement.IsNullable)));
            }
            metadata.Add(declaration2);
        }

        private static void DropDefaultAttribute(Accessor accessor, CodeCommentStatementCollection comments, string type)
        {
            if (!accessor.IsFixed && accessor.IsOptional)
            {
                CodeExporter.AddWarningComment(comments, Res.GetString("XmlDropDefaultAttribute", new object[] { type }));
            }
        }

        internal override void EnsureTypesExported(Accessor[] accessors, string ns)
        {
            if (accessors != null)
            {
                for (int i = 0; i < accessors.Length; i++)
                {
                    this.EnsureTypesExported(accessors[i], ns);
                }
            }
        }

        private void EnsureTypesExported(Accessor accessor, string ns)
        {
            if (accessor != null)
            {
                this.ExportType(accessor.Mapping, null, ns, null, false);
            }
        }

        private void ExportAnyAttribute(CodeAttributeDeclarationCollection metadata)
        {
            metadata.Add(new CodeAttributeDeclaration(typeof(XmlAnyAttributeAttribute).FullName));
        }

        private void ExportAnyElement(CodeAttributeDeclarationCollection metadata, string name, string ns, int sequenceId)
        {
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(typeof(XmlAnyElementAttribute).FullName);
            if ((name != null) && (name.Length > 0))
            {
                declaration.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(name)));
            }
            if (ns != null)
            {
                declaration.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(ns)));
            }
            if (sequenceId >= 0)
            {
                declaration.Arguments.Add(new CodeAttributeArgument("Order", new CodePrimitiveExpression(sequenceId)));
            }
            metadata.Add(declaration);
        }

        private void ExportArray(CodeAttributeDeclarationCollection metadata, string name, string ns, bool isNullable, XmlSchemaForm form, int sequenceId)
        {
            this.ExportMetadata(metadata, typeof(XmlArrayAttribute), name, ns, null, null, isNullable ? ((object) true) : null, form, 0, sequenceId);
        }

        private void ExportArrayElements(CodeAttributeDeclarationCollection metadata, ArrayMapping array, string ns, TypeDesc elementTypeDesc, int nestingLevel)
        {
            for (int i = 0; i < array.Elements.Length; i++)
            {
                ElementAccessor accessor = array.Elements[i];
                TypeMapping mapping = accessor.Mapping;
                string str = Accessor.UnescapeName(accessor.Name);
                bool flag = !accessor.Mapping.TypeDesc.IsArray && (str == accessor.Mapping.TypeName);
                bool flag2 = mapping.TypeDesc == elementTypeDesc;
                bool flag3 = (accessor.Form == XmlSchemaForm.Unqualified) || (accessor.Namespace == ns);
                bool flag4 = accessor.IsNullable == mapping.TypeDesc.IsNullable;
                bool flag5 = accessor.Form != XmlSchemaForm.Unqualified;
                if (((!flag || !flag2) || (!flag3 || !flag4)) || (!flag5 || (nestingLevel > 0)))
                {
                    this.ExportArrayItem(metadata, flag ? null : str, flag3 ? null : accessor.Namespace, flag2 ? null : mapping.TypeDesc, mapping.TypeDesc, accessor.IsNullable, flag5 ? XmlSchemaForm.None : accessor.Form, nestingLevel);
                }
                if (mapping is ArrayMapping)
                {
                    this.ExportArrayElements(metadata, (ArrayMapping) mapping, ns, elementTypeDesc.ArrayElementTypeDesc, nestingLevel + 1);
                }
            }
        }

        private void ExportArrayItem(CodeAttributeDeclarationCollection metadata, string name, string ns, TypeDesc typeDesc, TypeDesc dataTypeDesc, bool isNullable, XmlSchemaForm form, int nestingLevel)
        {
            this.ExportMetadata(metadata, typeof(XmlArrayItemAttribute), name, ns, typeDesc, dataTypeDesc, isNullable ? null : ((object) false), form, nestingLevel, -1);
        }

        private void ExportAttribute(CodeAttributeDeclarationCollection metadata, string name, string ns, TypeDesc typeDesc, TypeDesc dataTypeDesc, XmlSchemaForm form)
        {
            this.ExportMetadata(metadata, typeof(XmlAttributeAttribute), name, ns, typeDesc, dataTypeDesc, null, form, 0, -1);
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        internal override void ExportDerivedStructs(StructMapping mapping)
        {
            for (StructMapping mapping2 = mapping.DerivedMappings; mapping2 != null; mapping2 = mapping2.NextDerivedMapping)
            {
                this.ExportType(mapping2, mapping.Namespace);
            }
        }

        private void ExportElement(ElementAccessor element)
        {
            this.ExportType(element.Mapping, Accessor.UnescapeName(element.Name), element.Namespace, element, true);
        }

        private void ExportElement(CodeAttributeDeclarationCollection metadata, string name, string ns, TypeDesc typeDesc, TypeDesc dataTypeDesc, bool isNullable, XmlSchemaForm form, int sequenceId)
        {
            this.ExportMetadata(metadata, typeof(XmlElementAttribute), name, ns, typeDesc, dataTypeDesc, isNullable ? ((object) true) : null, form, 0, sequenceId);
        }

        private void ExportMember(CodeTypeDeclaration codeClass, MemberMapping member, string ns, CodeConstructor ctor)
        {
            CodeMemberField field;
            field = new CodeMemberField(member.GetTypeName(base.CodeProvider), member.Name) {
                Attributes = (field.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public
            };
            field.Comments.Add(new CodeCommentStatement(Res.GetString("XmlRemarks"), true));
            codeClass.Members.Add(field);
            this.AddMemberMetadata(field, field.CustomAttributes, member, ns, false, field.Comments, ctor);
            if (member.CheckSpecified != SpecifiedAccessor.None)
            {
                field = new CodeMemberField(typeof(bool).FullName, member.Name + "Specified") {
                    Attributes = (field.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public
                };
                field.Comments.Add(new CodeCommentStatement(Res.GetString("XmlRemarks"), true));
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(typeof(XmlIgnoreAttribute).FullName);
                field.CustomAttributes.Add(declaration);
                codeClass.Members.Add(field);
            }
        }

        public void ExportMembersMapping(XmlMembersMapping xmlMembersMapping)
        {
            xmlMembersMapping.CheckShallow();
            base.CheckScope(xmlMembersMapping.Scope);
            for (int i = 0; i < xmlMembersMapping.Count; i++)
            {
                AccessorMapping mapping = xmlMembersMapping[i].Mapping;
                if (mapping.Xmlns == null)
                {
                    if (mapping.Attribute != null)
                    {
                        this.ExportType(mapping.Attribute.Mapping, Accessor.UnescapeName(mapping.Attribute.Name), mapping.Attribute.Namespace, null, false);
                    }
                    if (mapping.Elements != null)
                    {
                        for (int j = 0; j < mapping.Elements.Length; j++)
                        {
                            ElementAccessor accessor = mapping.Elements[j];
                            this.ExportType(accessor.Mapping, Accessor.UnescapeName(accessor.Name), accessor.Namespace, null, false);
                        }
                    }
                    if (mapping.Text != null)
                    {
                        this.ExportType(mapping.Text.Mapping, Accessor.UnescapeName(mapping.Text.Name), mapping.Text.Namespace, null, false);
                    }
                }
            }
        }

        private void ExportMetadata(CodeAttributeDeclarationCollection metadata, Type attributeType, string name, string ns, TypeDesc typeDesc, TypeDesc dataTypeDesc, object isNullable, XmlSchemaForm form, int nestingLevel, int sequenceId)
        {
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(attributeType.FullName);
            if (name != null)
            {
                declaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(name)));
            }
            if (typeDesc != null)
            {
                if ((((isNullable != null) && ((bool) isNullable)) && (typeDesc.IsValueType && !typeDesc.IsMappedType)) && base.CodeProvider.Supports(GeneratorSupport.GenericTypeReference))
                {
                    declaration.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression("System.Nullable`1[" + typeDesc.FullName + "]")));
                    isNullable = null;
                }
                else
                {
                    declaration.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(typeDesc.FullName)));
                }
            }
            if (form != XmlSchemaForm.None)
            {
                declaration.Arguments.Add(new CodeAttributeArgument("Form", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(XmlSchemaForm).FullName), Enum.Format(typeof(XmlSchemaForm), form, "G"))));
                if (((form == XmlSchemaForm.Unqualified) && (ns != null)) && (ns.Length == 0))
                {
                    ns = null;
                }
            }
            if (ns != null)
            {
                declaration.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(ns)));
            }
            if (((dataTypeDesc != null) && dataTypeDesc.IsAmbiguousDataType) && !dataTypeDesc.IsMappedType)
            {
                declaration.Arguments.Add(new CodeAttributeArgument("DataType", new CodePrimitiveExpression(dataTypeDesc.DataType.Name)));
            }
            if (isNullable != null)
            {
                declaration.Arguments.Add(new CodeAttributeArgument("IsNullable", new CodePrimitiveExpression((bool) isNullable)));
            }
            if (nestingLevel > 0)
            {
                declaration.Arguments.Add(new CodeAttributeArgument("NestingLevel", new CodePrimitiveExpression(nestingLevel)));
            }
            if (sequenceId >= 0)
            {
                declaration.Arguments.Add(new CodeAttributeArgument("Order", new CodePrimitiveExpression(sequenceId)));
            }
            if ((declaration.Arguments.Count != 0) || (attributeType != typeof(XmlElementAttribute)))
            {
                metadata.Add(declaration);
            }
        }

        private void ExportProperty(CodeTypeDeclaration codeClass, MemberMapping member, string ns, CodeIdentifiers memberScope, CodeConstructor ctor)
        {
            string name = memberScope.AddUnique(CodeExporter.MakeFieldName(member.Name), member);
            string typeName = member.GetTypeName(base.CodeProvider);
            CodeMemberField field = new CodeMemberField(typeName, name) {
                Attributes = MemberAttributes.Private
            };
            codeClass.Members.Add(field);
            CodeMemberProperty property = base.CreatePropertyDeclaration(field, member.Name, typeName);
            property.Comments.Add(new CodeCommentStatement(Res.GetString("XmlRemarks"), true));
            this.AddMemberMetadata(field, property.CustomAttributes, member, ns, false, property.Comments, ctor);
            codeClass.Members.Add(property);
            if (member.CheckSpecified != SpecifiedAccessor.None)
            {
                field = new CodeMemberField(typeof(bool).FullName, name + "Specified") {
                    Attributes = MemberAttributes.Private
                };
                codeClass.Members.Add(field);
                property = base.CreatePropertyDeclaration(field, member.Name + "Specified", typeof(bool).FullName);
                property.Comments.Add(new CodeCommentStatement(Res.GetString("XmlRemarks"), true));
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(typeof(XmlIgnoreAttribute).FullName);
                property.CustomAttributes.Add(declaration);
                codeClass.Members.Add(property);
            }
        }

        private CodeTypeDeclaration ExportStruct(StructMapping mapping)
        {
            CodeConstructor constructor;
            if (mapping.TypeDesc.IsRoot)
            {
                base.ExportRoot(mapping, typeof(XmlIncludeAttribute));
                return null;
            }
            string name = mapping.TypeDesc.Name;
            string str2 = ((mapping.TypeDesc.BaseTypeDesc == null) || mapping.TypeDesc.BaseTypeDesc.IsRoot) ? string.Empty : mapping.TypeDesc.BaseTypeDesc.FullName;
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(name) {
                IsPartial = base.CodeProvider.Supports(GeneratorSupport.PartialTypes)
            };
            declaration.Comments.Add(new CodeCommentStatement(Res.GetString("XmlRemarks"), true));
            base.CodeNamespace.Types.Add(declaration);
            constructor = new CodeConstructor {
                Attributes = (constructor.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public
            };
            declaration.Members.Add(constructor);
            if (mapping.TypeDesc.IsAbstract)
            {
                constructor.Attributes |= MemberAttributes.Abstract;
            }
            if ((str2 != null) && (str2.Length > 0))
            {
                declaration.BaseTypes.Add(str2);
            }
            else
            {
                base.AddPropertyChangedNotifier(declaration);
            }
            declaration.TypeAttributes |= TypeAttributes.Public;
            if (mapping.TypeDesc.IsAbstract)
            {
                declaration.TypeAttributes |= TypeAttributes.Abstract;
            }
            CodeExporter.AddIncludeMetadata(declaration.CustomAttributes, mapping, typeof(XmlIncludeAttribute));
            if (mapping.IsSequence)
            {
                int num = 0;
                for (int j = 0; j < mapping.Members.Length; j++)
                {
                    MemberMapping mapping2 = mapping.Members[j];
                    if (mapping2.IsParticle && (mapping2.SequenceId < 0))
                    {
                        mapping2.SequenceId = num++;
                    }
                }
            }
            if (base.GenerateProperties)
            {
                for (int k = 0; k < mapping.Members.Length; k++)
                {
                    this.ExportProperty(declaration, mapping.Members[k], mapping.Namespace, mapping.Scope, constructor);
                }
            }
            else
            {
                for (int m = 0; m < mapping.Members.Length; m++)
                {
                    this.ExportMember(declaration, mapping.Members[m], mapping.Namespace, constructor);
                }
            }
            for (int i = 0; i < mapping.Members.Length; i++)
            {
                if (mapping.Members[i].Xmlns == null)
                {
                    this.EnsureTypesExported(mapping.Members[i].Elements, mapping.Namespace);
                    this.EnsureTypesExported(mapping.Members[i].Attribute, mapping.Namespace);
                    this.EnsureTypesExported(mapping.Members[i].Text, mapping.Namespace);
                }
            }
            if (mapping.BaseMapping != null)
            {
                this.ExportType(mapping.BaseMapping, null, mapping.Namespace, null, false);
            }
            this.ExportDerivedStructs(mapping);
            CodeGenerator.ValidateIdentifiers(declaration);
            if (constructor.Statements.Count == 0)
            {
                declaration.Members.Remove(constructor);
            }
            return declaration;
        }

        private void ExportText(CodeAttributeDeclarationCollection metadata, TypeDesc typeDesc, string dataType)
        {
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(typeof(XmlTextAttribute).FullName);
            if (typeDesc != null)
            {
                declaration.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(typeDesc.FullName)));
            }
            if (dataType != null)
            {
                declaration.Arguments.Add(new CodeAttributeArgument("DataType", new CodePrimitiveExpression(dataType)));
            }
            metadata.Add(declaration);
        }

        private void ExportType(TypeMapping mapping, string ns)
        {
            this.ExportType(mapping, null, ns, null, true);
        }

        private void ExportType(TypeMapping mapping, string name, string ns, ElementAccessor rootElement, bool checkReference)
        {
            if ((!mapping.IsReference || (mapping.Namespace == "http://schemas.xmlsoap.org/soap/encoding/")) && ((!(mapping is StructMapping) || !checkReference) || (!((StructMapping) mapping).ReferencedByTopLevelElement || (rootElement != null))))
            {
                if (((mapping is ArrayMapping) && (rootElement != null)) && (rootElement.IsTopLevelInSchema && (((ArrayMapping) mapping).TopLevelMapping != null)))
                {
                    mapping = ((ArrayMapping) mapping).TopLevelMapping;
                }
                CodeTypeDeclaration declaration = null;
                if (base.ExportedMappings[mapping] == null)
                {
                    base.ExportedMappings.Add(mapping, mapping);
                    if (mapping.TypeDesc.IsMappedType)
                    {
                        declaration = mapping.TypeDesc.ExtendedType.ExportTypeDefinition(base.CodeNamespace, base.CodeCompileUnit);
                    }
                    else if (mapping is EnumMapping)
                    {
                        declaration = base.ExportEnum((EnumMapping) mapping, typeof(XmlEnumAttribute));
                    }
                    else if (mapping is StructMapping)
                    {
                        declaration = this.ExportStruct((StructMapping) mapping);
                    }
                    else if (mapping is ArrayMapping)
                    {
                        this.EnsureTypesExported(((ArrayMapping) mapping).Elements, ns);
                    }
                    if (declaration != null)
                    {
                        if (!mapping.TypeDesc.IsMappedType)
                        {
                            declaration.CustomAttributes.Add(base.GeneratedCodeAttribute);
                            declaration.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(SerializableAttribute).FullName));
                            if (!declaration.IsEnum)
                            {
                                declaration.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(DebuggerStepThroughAttribute).FullName));
                                declaration.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(DesignerCategoryAttribute).FullName, new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression("code")) }));
                            }
                            base.AddTypeMetadata(declaration.CustomAttributes, typeof(XmlTypeAttribute), mapping.TypeDesc.Name, Accessor.UnescapeName(mapping.TypeName), mapping.Namespace, mapping.IncludeInSchema);
                        }
                        else if (CodeExporter.FindAttributeDeclaration(typeof(GeneratedCodeAttribute), declaration.CustomAttributes) == null)
                        {
                            declaration.CustomAttributes.Add(base.GeneratedCodeAttribute);
                        }
                        base.ExportedClasses.Add(mapping, declaration);
                    }
                }
                else
                {
                    declaration = (CodeTypeDeclaration) base.ExportedClasses[mapping];
                }
                if ((declaration != null) && (rootElement != null))
                {
                    this.AddRootMetadata(declaration.CustomAttributes, mapping, name, ns, rootElement);
                }
            }
        }

        public void ExportTypeMapping(XmlTypeMapping xmlTypeMapping)
        {
            xmlTypeMapping.CheckShallow();
            base.CheckScope(xmlTypeMapping.Scope);
            if (xmlTypeMapping.Accessor.Any)
            {
                throw new InvalidOperationException(Res.GetString("XmlIllegalWildcard"));
            }
            this.ExportElement(xmlTypeMapping.Accessor);
        }

        private CodeAttributeArgument[] GetDefaultValueArguments(PrimitiveMapping mapping, object value, out CodeExpression initExpression)
        {
            initExpression = null;
            if (value == null)
            {
                return null;
            }
            CodeExpression left = null;
            CodeExpression expression2 = null;
            Type type = value.GetType();
            CodeAttributeArgument[] argumentArray = null;
            if (mapping is EnumMapping)
            {
                if (((EnumMapping) mapping).IsFlags)
                {
                    string[] strArray = ((string) value).Split(null);
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        if (strArray[i].Length != 0)
                        {
                            CodeExpression right = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(mapping.TypeDesc.FullName), strArray[i]);
                            if (left != null)
                            {
                                left = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.BitwiseOr, right);
                            }
                            else
                            {
                                left = right;
                            }
                        }
                    }
                }
                else
                {
                    left = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(mapping.TypeDesc.FullName), (string) value);
                }
                initExpression = left;
                argumentArray = new CodeAttributeArgument[] { new CodeAttributeArgument(left) };
            }
            else if (((type == typeof(bool)) || (type == typeof(int))) || ((type == typeof(string)) || (type == typeof(double))))
            {
                initExpression = left = new CodePrimitiveExpression(value);
                argumentArray = new CodeAttributeArgument[] { new CodeAttributeArgument(left) };
            }
            else if (((type == typeof(short)) || (type == typeof(long))) || (((type == typeof(float)) || (type == typeof(byte))) || (type == typeof(decimal))))
            {
                left = new CodePrimitiveExpression(Convert.ToString(value, NumberFormatInfo.InvariantInfo));
                expression2 = new CodeTypeOfExpression(type.FullName);
                argumentArray = new CodeAttributeArgument[] { new CodeAttributeArgument(expression2), new CodeAttributeArgument(left) };
                initExpression = new CodeCastExpression(type.FullName, new CodePrimitiveExpression(value));
            }
            else if (((type == typeof(sbyte)) || (type == typeof(ushort))) || ((type == typeof(uint)) || (type == typeof(ulong))))
            {
                value = CodeExporter.PromoteType(type, value);
                left = new CodePrimitiveExpression(Convert.ToString(value, NumberFormatInfo.InvariantInfo));
                expression2 = new CodeTypeOfExpression(type.FullName);
                argumentArray = new CodeAttributeArgument[] { new CodeAttributeArgument(expression2), new CodeAttributeArgument(left) };
                initExpression = new CodeCastExpression(type.FullName, new CodePrimitiveExpression(value));
            }
            else if (type == typeof(DateTime))
            {
                string str;
                long ticks;
                DateTime time = (DateTime) value;
                if (mapping.TypeDesc.FormatterName == "Date")
                {
                    str = XmlCustomFormatter.FromDate(time);
                    DateTime time2 = new DateTime(time.Year, time.Month, time.Day);
                    ticks = time2.Ticks;
                }
                else if (mapping.TypeDesc.FormatterName == "Time")
                {
                    str = XmlCustomFormatter.FromDateTime(time);
                    ticks = time.Ticks;
                }
                else
                {
                    str = XmlCustomFormatter.FromDateTime(time);
                    ticks = time.Ticks;
                }
                left = new CodePrimitiveExpression(str);
                expression2 = new CodeTypeOfExpression(type.FullName);
                argumentArray = new CodeAttributeArgument[] { new CodeAttributeArgument(expression2), new CodeAttributeArgument(left) };
                initExpression = new CodeObjectCreateExpression(new CodeTypeReference(typeof(DateTime)), new CodeExpression[] { new CodePrimitiveExpression(ticks) });
            }
            else if (type == typeof(Guid))
            {
                left = new CodePrimitiveExpression(Convert.ToString(value, NumberFormatInfo.InvariantInfo));
                expression2 = new CodeTypeOfExpression(type.FullName);
                argumentArray = new CodeAttributeArgument[] { new CodeAttributeArgument(expression2), new CodeAttributeArgument(left) };
                initExpression = new CodeObjectCreateExpression(new CodeTypeReference(typeof(Guid)), new CodeExpression[] { left });
            }
            if ((mapping.TypeDesc.FullName != type.ToString()) && !(mapping is EnumMapping))
            {
                initExpression = new CodeCastExpression(mapping.TypeDesc.FullName, initExpression);
            }
            return argumentArray;
        }

        private object ImportDefault(TypeMapping mapping, string defaultValue)
        {
            if (defaultValue == null)
            {
                return null;
            }
            if (!mapping.IsList)
            {
                return this.ImportDefaultValue(mapping, defaultValue);
            }
            string[] strArray = defaultValue.Trim().Split(null);
            int num = 0;
            for (int i = 0; i < strArray.Length; i++)
            {
                if ((strArray[i] != null) && (strArray[i].Length > 0))
                {
                    num++;
                }
            }
            object[] objArray = new object[num];
            num = 0;
            for (int j = 0; j < strArray.Length; j++)
            {
                if ((strArray[j] != null) && (strArray[j].Length > 0))
                {
                    objArray[num++] = this.ImportDefaultValue(mapping, strArray[j]);
                }
            }
            return objArray;
        }

        private object ImportDefaultValue(TypeMapping mapping, string defaultValue)
        {
            if (defaultValue == null)
            {
                return null;
            }
            if (mapping is PrimitiveMapping)
            {
                if (mapping is EnumMapping)
                {
                    EnumMapping mapping2 = (EnumMapping) mapping;
                    ConstantMapping[] constants = mapping2.Constants;
                    if (mapping2.IsFlags)
                    {
                        Hashtable vals = new Hashtable();
                        string[] strArray = new string[constants.Length];
                        long[] ids = new long[constants.Length];
                        for (int j = 0; j < constants.Length; j++)
                        {
                            ids[j] = mapping2.IsFlags ? (((long) 1L) << j) : ((long) j);
                            strArray[j] = constants[j].Name;
                            vals.Add(constants[j].Name, ids[j]);
                        }
                        return XmlCustomFormatter.FromEnum(XmlCustomFormatter.ToEnum(defaultValue, vals, mapping2.TypeName, true), strArray, ids, mapping2.TypeDesc.FullName);
                    }
                    for (int i = 0; i < constants.Length; i++)
                    {
                        if (constants[i].XmlName == defaultValue)
                        {
                            return constants[i].Name;
                        }
                    }
                    throw new InvalidOperationException(Res.GetString("XmlInvalidDefaultValue", new object[] { defaultValue, mapping2.TypeDesc.FullName }));
                }
                PrimitiveMapping mapping3 = (PrimitiveMapping) mapping;
                if (!mapping3.TypeDesc.HasCustomFormatter)
                {
                    if (mapping3.TypeDesc.FormatterName == "String")
                    {
                        return defaultValue;
                    }
                    if (mapping3.TypeDesc.FormatterName == "DateTime")
                    {
                        return XmlCustomFormatter.ToDateTime(defaultValue);
                    }
                    Type type = typeof(XmlConvert);
                    MethodInfo method = type.GetMethod("To" + mapping3.TypeDesc.FormatterName, new Type[] { typeof(string) });
                    if (method != null)
                    {
                        return method.Invoke(type, new object[] { defaultValue });
                    }
                }
                else if (mapping3.TypeDesc.HasDefaultSupport)
                {
                    return XmlCustomFormatter.ToDefaultValue(defaultValue, mapping3.TypeDesc.FormatterName);
                }
            }
            return DBNull.Value;
        }
    }
}

