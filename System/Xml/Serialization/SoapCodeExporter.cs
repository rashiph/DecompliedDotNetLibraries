namespace System.Xml.Serialization
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Xml;

    public class SoapCodeExporter : CodeExporter
    {
        public SoapCodeExporter(CodeNamespace codeNamespace) : base(codeNamespace, null, null, CodeGenerationOptions.GenerateProperties, null)
        {
        }

        public SoapCodeExporter(CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit) : base(codeNamespace, codeCompileUnit, null, CodeGenerationOptions.GenerateProperties, null)
        {
        }

        public SoapCodeExporter(CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit, CodeGenerationOptions options) : base(codeNamespace, codeCompileUnit, null, CodeGenerationOptions.GenerateProperties, null)
        {
        }

        public SoapCodeExporter(CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit, CodeGenerationOptions options, Hashtable mappings) : base(codeNamespace, codeCompileUnit, null, options, mappings)
        {
        }

        public SoapCodeExporter(CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit, CodeDomProvider codeProvider, CodeGenerationOptions options, Hashtable mappings) : base(codeNamespace, codeCompileUnit, codeProvider, options, mappings)
        {
        }

        private void AddElementMetadata(CodeAttributeDeclarationCollection metadata, string elementName, TypeDesc typeDesc, bool isNullable)
        {
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(typeof(SoapElementAttribute).FullName);
            if (elementName != null)
            {
                declaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(elementName)));
            }
            if ((typeDesc != null) && typeDesc.IsAmbiguousDataType)
            {
                declaration.Arguments.Add(new CodeAttributeArgument("DataType", new CodePrimitiveExpression(typeDesc.DataType.Name)));
            }
            if (isNullable)
            {
                declaration.Arguments.Add(new CodeAttributeArgument("IsNullable", new CodePrimitiveExpression(true)));
            }
            metadata.Add(declaration);
        }

        public void AddMappingMetadata(CodeAttributeDeclarationCollection metadata, XmlMemberMapping member)
        {
            this.AddMemberMetadata(metadata, member.Mapping, false);
        }

        public void AddMappingMetadata(CodeAttributeDeclarationCollection metadata, XmlMemberMapping member, bool forceUseMemberName)
        {
            this.AddMemberMetadata(metadata, member.Mapping, forceUseMemberName);
        }

        private void AddMemberMetadata(CodeAttributeDeclarationCollection metadata, MemberMapping member, bool forceUseMemberName)
        {
            if (member.Elements.Length != 0)
            {
                ElementAccessor accessor = member.Elements[0];
                TypeMapping mapping = accessor.Mapping;
                string str = Accessor.UnescapeName(accessor.Name);
                bool flag = (str == member.Name) && !forceUseMemberName;
                if ((!flag || mapping.TypeDesc.IsAmbiguousDataType) || accessor.IsNullable)
                {
                    this.AddElementMetadata(metadata, flag ? null : str, mapping.TypeDesc.IsAmbiguousDataType ? mapping.TypeDesc : null, accessor.IsNullable);
                }
            }
        }

        internal override void EnsureTypesExported(Accessor[] accessors, string ns)
        {
            if (accessors != null)
            {
                for (int i = 0; i < accessors.Length; i++)
                {
                    this.ExportType(accessors[i].Mapping);
                }
            }
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        internal override void ExportDerivedStructs(StructMapping mapping)
        {
            for (StructMapping mapping2 = mapping.DerivedMappings; mapping2 != null; mapping2 = mapping2.NextDerivedMapping)
            {
                this.ExportType(mapping2);
            }
        }

        private void ExportElement(ElementAccessor element)
        {
            this.ExportType(element.Mapping);
        }

        private void ExportMember(CodeTypeDeclaration codeClass, MemberMapping member)
        {
            CodeMemberField field;
            field = new CodeMemberField(member.GetTypeName(base.CodeProvider), member.Name) {
                Attributes = (field.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public
            };
            field.Comments.Add(new CodeCommentStatement(Res.GetString("XmlRemarks"), true));
            codeClass.Members.Add(field);
            this.AddMemberMetadata(field.CustomAttributes, member, false);
            if (member.CheckSpecified != SpecifiedAccessor.None)
            {
                field = new CodeMemberField(typeof(bool).FullName, member.Name + "Specified") {
                    Attributes = (field.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public
                };
                field.Comments.Add(new CodeCommentStatement(Res.GetString("XmlRemarks"), true));
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(typeof(SoapIgnoreAttribute).FullName);
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
                this.ExportElement((ElementAccessor) xmlMembersMapping[i].Accessor);
            }
        }

        private void ExportProperty(CodeTypeDeclaration codeClass, MemberMapping member, CodeIdentifiers memberScope)
        {
            string name = memberScope.AddUnique(CodeExporter.MakeFieldName(member.Name), member);
            string typeName = member.GetTypeName(base.CodeProvider);
            CodeMemberField field = new CodeMemberField(typeName, name) {
                Attributes = MemberAttributes.Private
            };
            codeClass.Members.Add(field);
            CodeMemberProperty property = base.CreatePropertyDeclaration(field, member.Name, typeName);
            property.Comments.Add(new CodeCommentStatement(Res.GetString("XmlRemarks"), true));
            this.AddMemberMetadata(property.CustomAttributes, member, false);
            codeClass.Members.Add(property);
            if (member.CheckSpecified != SpecifiedAccessor.None)
            {
                field = new CodeMemberField(typeof(bool).FullName, name + "Specified") {
                    Attributes = MemberAttributes.Private
                };
                codeClass.Members.Add(field);
                property = base.CreatePropertyDeclaration(field, member.Name + "Specified", typeof(bool).FullName);
                property.Comments.Add(new CodeCommentStatement(Res.GetString("XmlRemarks"), true));
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(typeof(SoapIgnoreAttribute).FullName);
                property.CustomAttributes.Add(declaration);
                codeClass.Members.Add(property);
            }
        }

        private CodeTypeDeclaration ExportStruct(StructMapping mapping)
        {
            if (mapping.TypeDesc.IsRoot)
            {
                base.ExportRoot(mapping, typeof(SoapIncludeAttribute));
                return null;
            }
            if (!mapping.IncludeInSchema)
            {
                return null;
            }
            string name = mapping.TypeDesc.Name;
            string str2 = (mapping.TypeDesc.BaseTypeDesc == null) ? string.Empty : mapping.TypeDesc.BaseTypeDesc.Name;
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(name) {
                IsPartial = base.CodeProvider.Supports(GeneratorSupport.PartialTypes)
            };
            declaration.Comments.Add(new CodeCommentStatement(Res.GetString("XmlRemarks"), true));
            base.CodeNamespace.Types.Add(declaration);
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
            CodeExporter.AddIncludeMetadata(declaration.CustomAttributes, mapping, typeof(SoapIncludeAttribute));
            if (base.GenerateProperties)
            {
                for (int j = 0; j < mapping.Members.Length; j++)
                {
                    this.ExportProperty(declaration, mapping.Members[j], mapping.Scope);
                }
            }
            else
            {
                for (int k = 0; k < mapping.Members.Length; k++)
                {
                    this.ExportMember(declaration, mapping.Members[k]);
                }
            }
            for (int i = 0; i < mapping.Members.Length; i++)
            {
                this.EnsureTypesExported(mapping.Members[i].Elements, null);
            }
            if (mapping.BaseMapping != null)
            {
                this.ExportType(mapping.BaseMapping);
            }
            this.ExportDerivedStructs(mapping);
            CodeGenerator.ValidateIdentifiers(declaration);
            return declaration;
        }

        private void ExportType(TypeMapping mapping)
        {
            if (!mapping.IsReference && (base.ExportedMappings[mapping] == null))
            {
                CodeTypeDeclaration declaration = null;
                base.ExportedMappings.Add(mapping, mapping);
                if (mapping is EnumMapping)
                {
                    declaration = base.ExportEnum((EnumMapping) mapping, typeof(SoapEnumAttribute));
                }
                else if (mapping is StructMapping)
                {
                    declaration = this.ExportStruct((StructMapping) mapping);
                }
                else if (mapping is ArrayMapping)
                {
                    this.EnsureTypesExported(((ArrayMapping) mapping).Elements, null);
                }
                if (declaration != null)
                {
                    declaration.CustomAttributes.Add(base.GeneratedCodeAttribute);
                    declaration.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(SerializableAttribute).FullName));
                    if (!declaration.IsEnum)
                    {
                        declaration.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(DebuggerStepThroughAttribute).FullName));
                        declaration.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(DesignerCategoryAttribute).FullName, new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression("code")) }));
                    }
                    base.AddTypeMetadata(declaration.CustomAttributes, typeof(SoapTypeAttribute), mapping.TypeDesc.Name, Accessor.UnescapeName(mapping.TypeName), mapping.Namespace, mapping.IncludeInSchema);
                    base.ExportedClasses.Add(mapping, declaration);
                }
            }
        }

        public void ExportTypeMapping(XmlTypeMapping xmlTypeMapping)
        {
            xmlTypeMapping.CheckShallow();
            base.CheckScope(xmlTypeMapping.Scope);
            this.ExportElement(xmlTypeMapping.Accessor);
        }
    }
}

