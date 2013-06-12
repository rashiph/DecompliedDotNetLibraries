namespace System.Xml.Serialization
{
    using System;
    using System.CodeDom.Compiler;
    using System.Security.Permissions;
    using System.Xml;
    using System.Xml.Schema;

    public class SoapSchemaImporter : SchemaImporter
    {
        public SoapSchemaImporter(XmlSchemas schemas) : base(schemas, CodeGenerationOptions.GenerateProperties, null, new System.Xml.Serialization.ImportContext())
        {
        }

        public SoapSchemaImporter(XmlSchemas schemas, CodeIdentifiers typeIdentifiers) : base(schemas, CodeGenerationOptions.GenerateProperties, null, new System.Xml.Serialization.ImportContext(typeIdentifiers, false))
        {
        }

        public SoapSchemaImporter(XmlSchemas schemas, CodeGenerationOptions options, System.Xml.Serialization.ImportContext context) : base(schemas, options, null, context)
        {
        }

        public SoapSchemaImporter(XmlSchemas schemas, CodeIdentifiers typeIdentifiers, CodeGenerationOptions options) : base(schemas, options, null, new System.Xml.Serialization.ImportContext(typeIdentifiers, false))
        {
        }

        public SoapSchemaImporter(XmlSchemas schemas, CodeGenerationOptions options, CodeDomProvider codeProvider, System.Xml.Serialization.ImportContext context) : base(schemas, options, codeProvider, context)
        {
        }

        private XmlSchemaSimpleType FindDataType(XmlQualifiedName name)
        {
            TypeDesc typeDesc = base.Scope.GetTypeDesc(name.Name, name.Namespace);
            if ((typeDesc != null) && (typeDesc.DataType is XmlSchemaSimpleType))
            {
                return (XmlSchemaSimpleType) typeDesc.DataType;
            }
            XmlSchemaSimpleType type = (XmlSchemaSimpleType) base.Schemas.Find(name, typeof(XmlSchemaSimpleType));
            if (type != null)
            {
                return type;
            }
            if (name.Namespace != "http://www.w3.org/2001/XMLSchema")
            {
                throw new InvalidOperationException(Res.GetString("XmlMissingDataType", new object[] { name.ToString() }));
            }
            return (XmlSchemaSimpleType) base.Scope.GetTypeDesc(typeof(string)).DataType;
        }

        private object FindType(XmlQualifiedName name)
        {
            if ((name != null) && (name.Namespace == "http://schemas.xmlsoap.org/soap/encoding/"))
            {
                object obj2 = base.Schemas.Find(name, typeof(XmlSchemaComplexType));
                if (obj2 == null)
                {
                    return this.FindDataType(name);
                }
                XmlSchemaType type = (XmlSchemaType) obj2;
                XmlQualifiedName derivedFrom = type.DerivedFrom;
                if (!derivedFrom.IsEmpty)
                {
                    return this.FindType(derivedFrom);
                }
                return type;
            }
            object obj3 = base.Schemas.Find(name, typeof(XmlSchemaComplexType));
            if (obj3 != null)
            {
                return obj3;
            }
            return this.FindDataType(name);
        }

        private TypeDesc GetDataTypeSource(XmlSchemaSimpleType dataType)
        {
            if ((dataType.Name != null) && (dataType.Name.Length != 0))
            {
                TypeDesc typeDesc = base.Scope.GetTypeDesc(dataType);
                if (typeDesc != null)
                {
                    return typeDesc;
                }
            }
            if (!dataType.DerivedFrom.IsEmpty)
            {
                return this.GetDataTypeSource(this.FindDataType(dataType.DerivedFrom));
            }
            return base.Scope.GetTypeDesc(typeof(string));
        }

        private TypeMapping ImportAnyType(XmlSchemaComplexType type, string typeNs)
        {
            if (type.Particle != null)
            {
                if (!(type.Particle is XmlSchemaAll) && !(type.Particle is XmlSchemaSequence))
                {
                    return null;
                }
                XmlSchemaGroupBase particle = (XmlSchemaGroupBase) type.Particle;
                if ((particle.Items.Count == 1) && (particle.Items[0] is XmlSchemaAny))
                {
                    return base.ImportRootMapping();
                }
            }
            return null;
        }

        private ElementAccessor ImportArray(XmlSchemaElement element, string ns)
        {
            if (element.SchemaType == null)
            {
                return null;
            }
            if (!element.IsMultipleOccurrence)
            {
                return null;
            }
            XmlSchemaType schemaType = element.SchemaType;
            ArrayMapping mapping = this.ImportArrayMapping(schemaType, ns);
            if (mapping == null)
            {
                return null;
            }
            return new ElementAccessor { IsSoap = true, Name = element.Name, Namespace = ns, Mapping = mapping, IsNullable = false, Form = XmlSchemaForm.None };
        }

        private ArrayMapping ImportArrayMapping(XmlSchemaType type, string ns)
        {
            ArrayMapping mapping;
            if ((type.Name == "Array") && (ns == "http://schemas.xmlsoap.org/soap/encoding/"))
            {
                mapping = new ArrayMapping();
                TypeMapping rootMapping = base.GetRootMapping();
                ElementAccessor accessor = new ElementAccessor {
                    IsSoap = true,
                    Name = "anyType",
                    Namespace = ns,
                    Mapping = rootMapping,
                    IsNullable = true,
                    Form = XmlSchemaForm.None
                };
                mapping.Elements = new ElementAccessor[] { accessor };
                mapping.TypeDesc = accessor.Mapping.TypeDesc.CreateArrayTypeDesc();
                mapping.TypeName = "ArrayOf" + CodeIdentifier.MakePascal(accessor.Mapping.TypeName);
                return mapping;
            }
            if ((type.DerivedFrom.Name != "Array") || (type.DerivedFrom.Namespace != "http://schemas.xmlsoap.org/soap/encoding/"))
            {
                return null;
            }
            XmlSchemaContentModel contentModel = ((XmlSchemaComplexType) type).ContentModel;
            if (!(contentModel.Content is XmlSchemaComplexContentRestriction))
            {
                return null;
            }
            mapping = new ArrayMapping();
            XmlSchemaComplexContentRestriction content = (XmlSchemaComplexContentRestriction) contentModel.Content;
            for (int i = 0; i < content.Attributes.Count; i++)
            {
                XmlSchemaAttribute parent = content.Attributes[i] as XmlSchemaAttribute;
                if (((parent != null) && (parent.RefName.Name == "arrayType")) && (parent.RefName.Namespace == "http://schemas.xmlsoap.org/soap/encoding/"))
                {
                    string str = null;
                    if (parent.UnhandledAttributes != null)
                    {
                        foreach (XmlAttribute attribute2 in parent.UnhandledAttributes)
                        {
                            if ((attribute2.LocalName == "arrayType") && (attribute2.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/"))
                            {
                                str = attribute2.Value;
                                break;
                            }
                        }
                    }
                    if (str != null)
                    {
                        string str2;
                        TypeMapping mapping3;
                        XmlQualifiedName name = TypeScope.ParseWsdlArrayType(str, out str2, parent);
                        TypeDesc typeDesc = base.Scope.GetTypeDesc(name.Name, name.Namespace);
                        if ((typeDesc != null) && typeDesc.IsPrimitive)
                        {
                            mapping3 = new PrimitiveMapping {
                                TypeDesc = typeDesc,
                                TypeName = typeDesc.DataType.Name
                            };
                        }
                        else
                        {
                            mapping3 = this.ImportType(name, false);
                        }
                        ElementAccessor accessor2 = new ElementAccessor {
                            IsSoap = true,
                            Name = name.Name,
                            Namespace = ns,
                            Mapping = mapping3,
                            IsNullable = true,
                            Form = XmlSchemaForm.None
                        };
                        mapping.Elements = new ElementAccessor[] { accessor2 };
                        mapping.TypeDesc = accessor2.Mapping.TypeDesc.CreateArrayTypeDesc();
                        mapping.TypeName = "ArrayOf" + CodeIdentifier.MakePascal(accessor2.Mapping.TypeName);
                        return mapping;
                    }
                }
            }
            XmlSchemaParticle particle = content.Particle;
            if (!(particle is XmlSchemaAll) && !(particle is XmlSchemaSequence))
            {
                return null;
            }
            XmlSchemaGroupBase base2 = (XmlSchemaGroupBase) particle;
            if ((base2.Items.Count != 1) || !(base2.Items[0] is XmlSchemaElement))
            {
                return null;
            }
            XmlSchemaElement element = (XmlSchemaElement) base2.Items[0];
            if (!element.IsMultipleOccurrence)
            {
                return null;
            }
            ElementAccessor accessor3 = this.ImportElement(element, ns);
            mapping.Elements = new ElementAccessor[] { accessor3 };
            mapping.TypeDesc = accessor3.Mapping.TypeDesc.CreateArrayTypeDesc();
            return mapping;
        }

        private TypeMapping ImportDataType(XmlSchemaSimpleType dataType, string typeNs, string identifier, bool isList)
        {
            TypeMapping mapping = this.ImportNonXsdPrimitiveDataType(dataType, typeNs);
            if (mapping != null)
            {
                return mapping;
            }
            if (dataType.Content is XmlSchemaSimpleTypeRestriction)
            {
                XmlSchemaSimpleTypeRestriction content = (XmlSchemaSimpleTypeRestriction) dataType.Content;
                using (XmlSchemaObjectEnumerator enumerator = content.Facets.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current is XmlSchemaEnumerationFacet)
                        {
                            return this.ImportEnumeratedDataType(dataType, typeNs, identifier, isList);
                        }
                    }
                    goto Label_0104;
                }
            }
            if ((dataType.Content is XmlSchemaSimpleTypeList) || (dataType.Content is XmlSchemaSimpleTypeUnion))
            {
                if (dataType.Content is XmlSchemaSimpleTypeList)
                {
                    XmlSchemaSimpleTypeList list = (XmlSchemaSimpleTypeList) dataType.Content;
                    if (list.ItemType != null)
                    {
                        mapping = this.ImportDataType(list.ItemType, typeNs, identifier, true);
                        if (mapping != null)
                        {
                            return mapping;
                        }
                    }
                }
                return new PrimitiveMapping { TypeDesc = base.Scope.GetTypeDesc(typeof(string)), TypeName = mapping.TypeDesc.DataType.Name };
            }
        Label_0104:
            return this.ImportPrimitiveDataType(dataType);
        }

        public XmlTypeMapping ImportDerivedTypeMapping(XmlQualifiedName name, Type baseType, bool baseTypeCanBeIndirect)
        {
            TypeMapping mapping = this.ImportType(name, false);
            if (mapping is StructMapping)
            {
                base.MakeDerived((StructMapping) mapping, baseType, baseTypeCanBeIndirect);
            }
            else if (baseType != null)
            {
                throw new InvalidOperationException(Res.GetString("XmlPrimitiveBaseType", new object[] { name.Name, name.Namespace, baseType.FullName }));
            }
            return new XmlTypeMapping(base.Scope, new ElementAccessor { IsSoap = true, Name = name.Name, Namespace = name.Namespace, Mapping = mapping, IsNullable = true, Form = XmlSchemaForm.Qualified });
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        internal override void ImportDerivedTypes(XmlQualifiedName baseName)
        {
            foreach (XmlSchema schema in base.Schemas)
            {
                if (!base.Schemas.IsReference(schema) && !XmlSchemas.IsDataSet(schema))
                {
                    XmlSchemas.Preprocess(schema);
                    foreach (object obj2 in schema.SchemaTypes.Values)
                    {
                        if (obj2 is XmlSchemaType)
                        {
                            XmlSchemaType type = (XmlSchemaType) obj2;
                            if (type.DerivedFrom == baseName)
                            {
                                this.ImportType(type.QualifiedName, false);
                            }
                        }
                    }
                }
            }
        }

        private ElementAccessor ImportElement(XmlSchemaElement element, string ns)
        {
            if (!element.RefName.IsEmpty)
            {
                throw new InvalidOperationException(Res.GetString("RefSyntaxNotSupportedForElements0", new object[] { element.RefName.Name, element.RefName.Namespace }));
            }
            if (element.Name.Length == 0)
            {
                XmlQualifiedName parentName = XmlSchemas.GetParentName(element);
                throw new InvalidOperationException(Res.GetString("XmlElementHasNoName", new object[] { parentName.Name, parentName.Namespace }));
            }
            TypeMapping mapping = this.ImportElementType(element, ns);
            return new ElementAccessor { IsSoap = true, Name = element.Name, Namespace = ns, Mapping = mapping, IsNullable = element.IsNillable, Form = XmlSchemaForm.None };
        }

        private void ImportElementMember(XmlSchemaElement element, CodeIdentifiers members, string ns)
        {
            MemberMapping mapping;
            ElementAccessor accessor = this.ImportArray(element, ns);
            if (accessor == null)
            {
                accessor = this.ImportElement(element, ns);
            }
            mapping = new MemberMapping {
                Name = CodeIdentifier.MakeValid(Accessor.UnescapeName(accessor.Name)),
                Name = members.AddUnique(mapping.Name, mapping)
            };
            if (mapping.Name.EndsWith("Specified", StringComparison.Ordinal))
            {
                string name = mapping.Name;
                mapping.Name = members.AddUnique(mapping.Name, mapping);
                members.Remove(name);
            }
            mapping.TypeDesc = accessor.Mapping.TypeDesc;
            mapping.Elements = new ElementAccessor[] { accessor };
            if (element.IsMultipleOccurrence)
            {
                mapping.TypeDesc = mapping.TypeDesc.CreateArrayTypeDesc();
            }
            if (((element.MinOccurs == 0M) && mapping.TypeDesc.IsValueType) && !mapping.TypeDesc.HasIsEmpty)
            {
                mapping.CheckSpecified = SpecifiedAccessor.ReadWrite;
            }
        }

        private TypeMapping ImportElementType(XmlSchemaElement element, string ns)
        {
            TypeMapping mapping;
            if (!element.SchemaTypeName.IsEmpty)
            {
                mapping = this.ImportType(element.SchemaTypeName, false);
            }
            else
            {
                if (element.SchemaType != null)
                {
                    XmlQualifiedName name = XmlSchemas.GetParentName(element);
                    if (element.SchemaType is XmlSchemaComplexType)
                    {
                        mapping = this.ImportType((XmlSchemaComplexType) element.SchemaType, ns, false);
                        if (!(mapping is ArrayMapping))
                        {
                            throw new InvalidOperationException(Res.GetString("XmlInvalidSchemaElementType", new object[] { name.Name, name.Namespace, element.Name }));
                        }
                        goto Label_014F;
                    }
                    throw new InvalidOperationException(Res.GetString("XmlInvalidSchemaElementType", new object[] { name.Name, name.Namespace, element.Name }));
                }
                if (!element.SubstitutionGroup.IsEmpty)
                {
                    XmlQualifiedName name2 = XmlSchemas.GetParentName(element);
                    throw new InvalidOperationException(Res.GetString("XmlInvalidSubstitutionGroupUse", new object[] { name2.Name, name2.Namespace }));
                }
                XmlQualifiedName parentName = XmlSchemas.GetParentName(element);
                throw new InvalidOperationException(Res.GetString("XmlElementMissingType", new object[] { parentName.Name, parentName.Namespace, element.Name }));
            }
        Label_014F:
            mapping.ReferencedByElement = true;
            return mapping;
        }

        private TypeMapping ImportEnumeratedDataType(XmlSchemaSimpleType dataType, string typeNs, string identifier, bool isList)
        {
            TypeMapping mapping = (TypeMapping) base.ImportedMappings[dataType];
            if (mapping != null)
            {
                return mapping;
            }
            XmlSchemaSimpleType type = this.FindDataType(dataType.DerivedFrom);
            TypeDesc typeDesc = base.Scope.GetTypeDesc(type);
            if ((typeDesc != null) && (typeDesc != base.Scope.GetTypeDesc(typeof(string))))
            {
                return this.ImportPrimitiveDataType(dataType);
            }
            identifier = Accessor.UnescapeName(identifier);
            string name = base.GenerateUniqueTypeName(identifier);
            EnumMapping mapping2 = new EnumMapping {
                IsReference = base.Schemas.IsReference(dataType),
                TypeDesc = new TypeDesc(name, name, TypeKind.Enum, null, TypeFlags.None),
                TypeName = identifier,
                Namespace = typeNs,
                IsFlags = isList
            };
            CodeIdentifiers identifiers = new CodeIdentifiers();
            if (!(dataType.Content is XmlSchemaSimpleTypeRestriction))
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidEnumContent", new object[] { dataType.Content.GetType().Name, identifier }));
            }
            XmlSchemaSimpleTypeRestriction content = (XmlSchemaSimpleTypeRestriction) dataType.Content;
            for (int i = 0; i < content.Facets.Count; i++)
            {
                object obj2 = content.Facets[i];
                if (obj2 is XmlSchemaEnumerationFacet)
                {
                    XmlSchemaEnumerationFacet facet = (XmlSchemaEnumerationFacet) obj2;
                    ConstantMapping mapping3 = new ConstantMapping();
                    string str2 = CodeIdentifier.MakeValid(facet.Value);
                    mapping3.Name = identifiers.AddUnique(str2, mapping3);
                    mapping3.XmlName = facet.Value;
                    mapping3.Value = i;
                }
            }
            mapping2.Constants = (ConstantMapping[]) identifiers.ToArray(typeof(ConstantMapping));
            if (isList && (mapping2.Constants.Length > 0x3f))
            {
                mapping = new PrimitiveMapping {
                    TypeDesc = base.Scope.GetTypeDesc(typeof(string)),
                    TypeName = mapping.TypeDesc.DataType.Name
                };
                base.ImportedMappings.Add(dataType, mapping);
                return mapping;
            }
            base.ImportedMappings.Add(dataType, mapping2);
            base.Scope.AddTypeMapping(mapping2);
            return mapping2;
        }

        private void ImportGroup(XmlSchemaParticle group, CodeIdentifiers members, string ns)
        {
            if (group is XmlSchemaChoice)
            {
                XmlQualifiedName parentName = XmlSchemas.GetParentName(group);
                throw new InvalidOperationException(Res.GetString("XmlSoapInvalidChoice", new object[] { parentName.Name, parentName.Namespace }));
            }
            this.ImportGroupMembers(group, members, ns);
        }

        private void ImportGroupMembers(XmlSchemaParticle particle, CodeIdentifiers members, string ns)
        {
            XmlQualifiedName parentName = XmlSchemas.GetParentName(particle);
            if (particle is XmlSchemaGroupRef)
            {
                throw new InvalidOperationException(Res.GetString("XmlSoapUnsupportedGroupRef", new object[] { parentName.Name, parentName.Namespace }));
            }
            if (particle is XmlSchemaGroupBase)
            {
                XmlSchemaGroupBase base2 = (XmlSchemaGroupBase) particle;
                if (base2.IsMultipleOccurrence)
                {
                    throw new InvalidOperationException(Res.GetString("XmlSoapUnsupportedGroupRepeat", new object[] { parentName.Name, parentName.Namespace }));
                }
                for (int i = 0; i < base2.Items.Count; i++)
                {
                    object obj2 = base2.Items[i];
                    if ((obj2 is XmlSchemaGroupBase) || (obj2 is XmlSchemaGroupRef))
                    {
                        throw new InvalidOperationException(Res.GetString("XmlSoapUnsupportedGroupNested", new object[] { parentName.Name, parentName.Namespace }));
                    }
                    if (obj2 is XmlSchemaElement)
                    {
                        this.ImportElementMember((XmlSchemaElement) obj2, members, ns);
                    }
                    else if (obj2 is XmlSchemaAny)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlSoapUnsupportedGroupAny", new object[] { parentName.Name, parentName.Namespace }));
                    }
                }
            }
        }

        public XmlMembersMapping ImportMembersMapping(string name, string ns, SoapSchemaMember member)
        {
            TypeMapping mapping = this.ImportType(member.MemberType, true);
            if (!(mapping is StructMapping))
            {
                return this.ImportMembersMapping(name, ns, new SoapSchemaMember[] { member });
            }
            MembersMapping mapping2 = new MembersMapping {
                TypeDesc = base.Scope.GetTypeDesc(typeof(object[])),
                Members = ((StructMapping) mapping).Members,
                HasWrapperElement = true
            };
            return new XmlMembersMapping(base.Scope, new ElementAccessor { IsSoap = true, Name = name, Namespace = (mapping.Namespace != null) ? mapping.Namespace : ns, Mapping = mapping2, IsNullable = false, Form = XmlSchemaForm.Qualified }, XmlMappingAccess.Write | XmlMappingAccess.Read);
        }

        public XmlMembersMapping ImportMembersMapping(string name, string ns, SoapSchemaMember[] members)
        {
            return this.ImportMembersMapping(name, ns, members, true);
        }

        public XmlMembersMapping ImportMembersMapping(string name, string ns, SoapSchemaMember[] members, bool hasWrapperElement)
        {
            return this.ImportMembersMapping(name, ns, members, hasWrapperElement, null, false);
        }

        public XmlMembersMapping ImportMembersMapping(string name, string ns, SoapSchemaMember[] members, bool hasWrapperElement, Type baseType, bool baseTypeCanBeIndirect)
        {
            XmlSchemaComplexType type = new XmlSchemaComplexType();
            XmlSchemaSequence sequence = new XmlSchemaSequence();
            type.Particle = sequence;
            foreach (SoapSchemaMember member in members)
            {
                XmlSchemaElement item = new XmlSchemaElement {
                    Name = member.MemberName,
                    SchemaTypeName = member.MemberType
                };
                sequence.Items.Add(item);
            }
            CodeIdentifiers identifiers = new CodeIdentifiers {
                UseCamelCasing = true
            };
            MembersMapping mapping = new MembersMapping {
                TypeDesc = base.Scope.GetTypeDesc(typeof(object[])),
                Members = this.ImportTypeMembers(type, ns, identifiers),
                HasWrapperElement = hasWrapperElement
            };
            if (baseType != null)
            {
                for (int i = 0; i < mapping.Members.Length; i++)
                {
                    MemberMapping mapping2 = mapping.Members[i];
                    if (mapping2.Accessor.Mapping is StructMapping)
                    {
                        base.MakeDerived((StructMapping) mapping2.Accessor.Mapping, baseType, baseTypeCanBeIndirect);
                    }
                }
            }
            return new XmlMembersMapping(base.Scope, new ElementAccessor { IsSoap = true, Name = name, Namespace = ns, Mapping = mapping, IsNullable = false, Form = XmlSchemaForm.Qualified }, XmlMappingAccess.Write | XmlMappingAccess.Read);
        }

        private PrimitiveMapping ImportNonXsdPrimitiveDataType(XmlSchemaSimpleType dataType, string ns)
        {
            PrimitiveMapping mapping = null;
            TypeDesc typeDesc = null;
            if ((dataType.Name != null) && (dataType.Name.Length != 0))
            {
                typeDesc = base.Scope.GetTypeDesc(dataType.Name, ns);
                if (typeDesc != null)
                {
                    mapping = new PrimitiveMapping {
                        TypeDesc = typeDesc,
                        TypeName = typeDesc.DataType.Name
                    };
                }
            }
            return mapping;
        }

        private PrimitiveMapping ImportPrimitiveDataType(XmlSchemaSimpleType dataType)
        {
            TypeDesc dataTypeSource = this.GetDataTypeSource(dataType);
            return new PrimitiveMapping { TypeDesc = dataTypeSource, TypeName = dataTypeSource.DataType.Name };
        }

        private StructMapping ImportStructType(XmlSchemaComplexType type, string typeNs, bool excludeFromImport)
        {
            if (type.Name == null)
            {
                XmlSchemaElement parent = (XmlSchemaElement) type.Parent;
                XmlQualifiedName parentName = XmlSchemas.GetParentName(parent);
                throw new InvalidOperationException(Res.GetString("XmlInvalidSchemaElementType", new object[] { parentName.Name, parentName.Namespace, parent.Name }));
            }
            TypeDesc baseTypeDesc = null;
            Mapping rootMapping = null;
            if (!type.DerivedFrom.IsEmpty)
            {
                rootMapping = this.ImportType(type.DerivedFrom, excludeFromImport);
                if (rootMapping is StructMapping)
                {
                    baseTypeDesc = ((StructMapping) rootMapping).TypeDesc;
                }
                else
                {
                    rootMapping = null;
                }
            }
            if (rootMapping == null)
            {
                rootMapping = base.GetRootMapping();
            }
            Mapping mapping2 = (Mapping) base.ImportedMappings[type];
            if (mapping2 != null)
            {
                return (StructMapping) mapping2;
            }
            string str = base.GenerateUniqueTypeName(Accessor.UnescapeName(type.Name));
            StructMapping mapping3 = new StructMapping {
                IsReference = base.Schemas.IsReference(type)
            };
            TypeFlags reference = TypeFlags.Reference;
            if (type.IsAbstract)
            {
                reference |= TypeFlags.Abstract;
            }
            mapping3.TypeDesc = new TypeDesc(str, str, TypeKind.Struct, baseTypeDesc, reference);
            mapping3.Namespace = typeNs;
            mapping3.TypeName = type.Name;
            mapping3.BaseMapping = (StructMapping) rootMapping;
            base.ImportedMappings.Add(type, mapping3);
            if (excludeFromImport)
            {
                mapping3.IncludeInSchema = false;
            }
            CodeIdentifiers scope = new CodeIdentifiers();
            scope.AddReserved(str);
            base.AddReservedIdentifiersForDataBinding(scope);
            mapping3.Members = this.ImportTypeMembers(type, typeNs, scope);
            base.Scope.AddTypeMapping(mapping3);
            this.ImportDerivedTypes(new XmlQualifiedName(type.Name, typeNs));
            return mapping3;
        }

        private TypeMapping ImportType(XmlQualifiedName name, bool excludeFromImport)
        {
            if ((name.Name == "anyType") && (name.Namespace == "http://www.w3.org/2001/XMLSchema"))
            {
                return base.ImportRootMapping();
            }
            object obj2 = this.FindType(name);
            TypeMapping mapping = (TypeMapping) base.ImportedMappings[obj2];
            if (mapping == null)
            {
                if (!(obj2 is XmlSchemaComplexType))
                {
                    if (!(obj2 is XmlSchemaSimpleType))
                    {
                        throw new InvalidOperationException(Res.GetString("XmlInternalError"));
                    }
                    mapping = this.ImportDataType((XmlSchemaSimpleType) obj2, name.Namespace, name.Name, false);
                }
                else
                {
                    mapping = this.ImportType((XmlSchemaComplexType) obj2, name.Namespace, excludeFromImport);
                }
            }
            if (excludeFromImport)
            {
                mapping.IncludeInSchema = false;
            }
            return mapping;
        }

        private TypeMapping ImportType(XmlSchemaComplexType type, string typeNs, bool excludeFromImport)
        {
            if (type.Redefined != null)
            {
                throw new NotSupportedException(Res.GetString("XmlUnsupportedRedefine", new object[] { type.Name, typeNs }));
            }
            TypeMapping mapping = this.ImportAnyType(type, typeNs);
            if (mapping == null)
            {
                mapping = this.ImportArrayMapping(type, typeNs);
            }
            if (mapping == null)
            {
                mapping = this.ImportStructType(type, typeNs, excludeFromImport);
            }
            return mapping;
        }

        private MemberMapping[] ImportTypeMembers(XmlSchemaComplexType type, string typeNs, CodeIdentifiers members)
        {
            if (type.AnyAttribute != null)
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidAnyAttributeUse", new object[] { type.Name, type.QualifiedName.Namespace }));
            }
            XmlSchemaObjectCollection attributes = type.Attributes;
            for (int i = 0; i < attributes.Count; i++)
            {
                object obj2 = attributes[i];
                if (obj2 is XmlSchemaAttributeGroup)
                {
                    throw new InvalidOperationException(Res.GetString("XmlSoapInvalidAttributeUse", new object[] { type.Name, type.QualifiedName.Namespace }));
                }
                if (obj2 is XmlSchemaAttribute)
                {
                    XmlSchemaAttribute attribute = (XmlSchemaAttribute) obj2;
                    if (attribute.Use != XmlSchemaUse.Prohibited)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlSoapInvalidAttributeUse", new object[] { type.Name, type.QualifiedName.Namespace }));
                    }
                }
            }
            if (type.Particle != null)
            {
                this.ImportGroup(type.Particle, members, typeNs);
            }
            else if ((type.ContentModel != null) && (type.ContentModel is XmlSchemaComplexContent))
            {
                XmlSchemaComplexContent contentModel = (XmlSchemaComplexContent) type.ContentModel;
                if (contentModel.Content is XmlSchemaComplexContentExtension)
                {
                    if (((XmlSchemaComplexContentExtension) contentModel.Content).Particle != null)
                    {
                        this.ImportGroup(((XmlSchemaComplexContentExtension) contentModel.Content).Particle, members, typeNs);
                    }
                }
                else if ((contentModel.Content is XmlSchemaComplexContentRestriction) && (((XmlSchemaComplexContentRestriction) contentModel.Content).Particle != null))
                {
                    this.ImportGroup(((XmlSchemaComplexContentRestriction) contentModel.Content).Particle, members, typeNs);
                }
            }
            return (MemberMapping[]) members.ToArray(typeof(MemberMapping));
        }
    }
}

