namespace System.Xml.Serialization
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization.Advanced;

    public class XmlSchemaImporter : SchemaImporter
    {
        public XmlSchemaImporter(XmlSchemas schemas) : base(schemas, CodeGenerationOptions.GenerateProperties, null, new System.Xml.Serialization.ImportContext())
        {
        }

        public XmlSchemaImporter(XmlSchemas schemas, CodeIdentifiers typeIdentifiers) : base(schemas, CodeGenerationOptions.GenerateProperties, null, new System.Xml.Serialization.ImportContext(typeIdentifiers, false))
        {
        }

        public XmlSchemaImporter(XmlSchemas schemas, CodeGenerationOptions options, System.Xml.Serialization.ImportContext context) : base(schemas, options, null, context)
        {
        }

        public XmlSchemaImporter(XmlSchemas schemas, CodeIdentifiers typeIdentifiers, CodeGenerationOptions options) : base(schemas, options, null, new System.Xml.Serialization.ImportContext(typeIdentifiers, false))
        {
        }

        public XmlSchemaImporter(XmlSchemas schemas, CodeGenerationOptions options, CodeDomProvider codeProvider, System.Xml.Serialization.ImportContext context) : base(schemas, options, codeProvider, context)
        {
        }

        private void AddScopeElement(INameScope scope, ElementAccessor element, ref bool duplicateElements, bool allowDuplicates)
        {
            if (scope != null)
            {
                ElementAccessor accessor = (ElementAccessor) scope[element.Name, element.Namespace];
                if (accessor != null)
                {
                    if (!allowDuplicates)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlDuplicateElementInScope", new object[] { element.Name, element.Namespace }));
                    }
                    if (accessor.Mapping.TypeDesc != element.Mapping.TypeDesc)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlDuplicateElementInScope1", new object[] { element.Name, element.Namespace }));
                    }
                    duplicateElements = true;
                }
                else
                {
                    scope[element.Name, element.Namespace] = element;
                }
            }
        }

        private void AddScopeElements(INameScope scope, ElementAccessor[] elements, ref bool duplicateElements, bool allowDuplicates)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                this.AddScopeElement(scope, elements[i], ref duplicateElements, allowDuplicates);
            }
        }

        private XmlSchemaForm AttributeForm(string ns, XmlSchemaAttribute attribute)
        {
            if (attribute.Form != XmlSchemaForm.None)
            {
                return attribute.Form;
            }
            XmlSchemaObject parent = attribute;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }
            XmlSchema schema = parent as XmlSchema;
            if (schema != null)
            {
                if ((ns == null) || (ns.Length == 0))
                {
                    if (schema.AttributeFormDefault != XmlSchemaForm.None)
                    {
                        return schema.AttributeFormDefault;
                    }
                    return XmlSchemaForm.Unqualified;
                }
                XmlSchemas.Preprocess(schema);
                if ((attribute.QualifiedName.Namespace != null) && (attribute.QualifiedName.Namespace.Length != 0))
                {
                    return XmlSchemaForm.Qualified;
                }
            }
            return XmlSchemaForm.Unqualified;
        }

        internal static XmlQualifiedName BaseTypeName(XmlSchemaSimpleType dataType)
        {
            XmlSchemaSimpleTypeContent content = dataType.Content;
            if (content is XmlSchemaSimpleTypeRestriction)
            {
                return ((XmlSchemaSimpleTypeRestriction) content).BaseTypeName;
            }
            if (content is XmlSchemaSimpleTypeList)
            {
                XmlSchemaSimpleTypeList list = (XmlSchemaSimpleTypeList) content;
                if ((list.ItemTypeName != null) && !list.ItemTypeName.IsEmpty)
                {
                    return list.ItemTypeName;
                }
                if (list.ItemType != null)
                {
                    return BaseTypeName(list.ItemType);
                }
            }
            return new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
        }

        private XmlSchemaForm ElementForm(string ns, XmlSchemaElement element)
        {
            if (element.Form != XmlSchemaForm.None)
            {
                return element.Form;
            }
            XmlSchemaObject parent = element;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }
            XmlSchema schema = parent as XmlSchema;
            if (schema == null)
            {
                return XmlSchemaForm.Qualified;
            }
            if ((ns == null) || (ns.Length == 0))
            {
                if (schema.ElementFormDefault != XmlSchemaForm.None)
                {
                    return schema.ElementFormDefault;
                }
                return XmlSchemaForm.Unqualified;
            }
            XmlSchemas.Preprocess(schema);
            if ((element.QualifiedName.Namespace != null) && (element.QualifiedName.Namespace.Length != 0))
            {
                return XmlSchemaForm.Qualified;
            }
            return XmlSchemaForm.Unqualified;
        }

        private XmlSchemaAttribute FindAttribute(XmlQualifiedName name)
        {
            XmlSchemaAttribute attribute = (XmlSchemaAttribute) base.Schemas.Find(name, typeof(XmlSchemaAttribute));
            if (attribute == null)
            {
                throw new InvalidOperationException(Res.GetString("XmlMissingAttribute", new object[] { name.Name }));
            }
            return attribute;
        }

        private XmlSchemaAttributeGroup FindAttributeGroup(XmlQualifiedName name)
        {
            XmlSchemaAttributeGroup group = (XmlSchemaAttributeGroup) base.Schemas.Find(name, typeof(XmlSchemaAttributeGroup));
            if (group == null)
            {
                throw new InvalidOperationException(Res.GetString("XmlMissingAttributeGroup", new object[] { name.Name }));
            }
            return group;
        }

        private XmlSchemaSimpleType FindDataType(XmlQualifiedName name, TypeFlags flags)
        {
            if ((name == null) || name.IsEmpty)
            {
                return (XmlSchemaSimpleType) base.Scope.GetTypeDesc(typeof(string)).DataType;
            }
            TypeDesc desc = base.Scope.GetTypeDesc(name.Name, name.Namespace, flags);
            if ((desc != null) && (desc.DataType is XmlSchemaSimpleType))
            {
                return (XmlSchemaSimpleType) desc.DataType;
            }
            XmlSchemaSimpleType type = (XmlSchemaSimpleType) base.Schemas.Find(name, typeof(XmlSchemaSimpleType));
            if (type != null)
            {
                return type;
            }
            if (name.Namespace == "http://www.w3.org/2001/XMLSchema")
            {
                return (XmlSchemaSimpleType) base.Scope.GetTypeDesc("string", "http://www.w3.org/2001/XMLSchema", flags).DataType;
            }
            if ((name.Name == "Array") && (name.Namespace == "http://schemas.xmlsoap.org/soap/encoding/"))
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidEncoding", new object[] { name.ToString() }));
            }
            throw new InvalidOperationException(Res.GetString("XmlMissingDataType", new object[] { name.ToString() }));
        }

        private XmlSchemaElement FindElement(XmlQualifiedName name)
        {
            XmlSchemaElement element = (XmlSchemaElement) base.Schemas.Find(name, typeof(XmlSchemaElement));
            if (element == null)
            {
                throw new InvalidOperationException(Res.GetString("XmlMissingElement", new object[] { name.ToString() }));
            }
            return element;
        }

        internal string FindExtendedAnyElement(XmlSchemaAny any, bool mixed, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, out SchemaImporterExtension extension)
        {
            extension = null;
            foreach (SchemaImporterExtension extension2 in base.Extensions)
            {
                string str = extension2.ImportAnyElement(any, mixed, base.Schemas, this, compileUnit, mainNamespace, base.Options, base.CodeProvider);
                if ((str != null) && (str.Length > 0))
                {
                    extension = extension2;
                    return str;
                }
            }
            return null;
        }

        internal string FindExtendedType(XmlSchemaType type, XmlSchemaObject context, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, out SchemaImporterExtension extension)
        {
            extension = null;
            foreach (SchemaImporterExtension extension2 in base.Extensions)
            {
                string str = extension2.ImportSchemaType(type, context, base.Schemas, this, compileUnit, mainNamespace, base.Options, base.CodeProvider);
                if ((str != null) && (str.Length > 0))
                {
                    extension = extension2;
                    return str;
                }
            }
            return null;
        }

        internal string FindExtendedType(string name, string ns, XmlSchemaObject context, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, out SchemaImporterExtension extension)
        {
            extension = null;
            foreach (SchemaImporterExtension extension2 in base.Extensions)
            {
                string str = extension2.ImportSchemaType(name, ns, context, base.Schemas, this, compileUnit, mainNamespace, base.Options, base.CodeProvider);
                if ((str != null) && (str.Length > 0))
                {
                    extension = extension2;
                    return str;
                }
            }
            return null;
        }

        private XmlSchemaGroup FindGroup(XmlQualifiedName name)
        {
            XmlSchemaGroup group = (XmlSchemaGroup) base.Schemas.Find(name, typeof(XmlSchemaGroup));
            if (group == null)
            {
                throw new InvalidOperationException(Res.GetString("XmlMissingGroup", new object[] { name.Name }));
            }
            return group;
        }

        private XmlSchemaType FindType(XmlQualifiedName name, TypeFlags flags)
        {
            if ((name == null) || name.IsEmpty)
            {
                return base.Scope.GetTypeDesc(typeof(string)).DataType;
            }
            object obj2 = base.Schemas.Find(name, typeof(XmlSchemaComplexType));
            if (obj2 != null)
            {
                return (XmlSchemaComplexType) obj2;
            }
            return this.FindDataType(name, flags);
        }

        private bool GatherGroupChoices(XmlSchemaGroup group, System.Xml.Serialization.NameTable choiceElements, string identifier, string ns, ref bool needExplicitOrder, bool allowDuplicates)
        {
            return this.GatherGroupChoices(group.Particle, choiceElements, identifier, ns, ref needExplicitOrder, allowDuplicates);
        }

        private bool GatherGroupChoices(XmlSchemaParticle particle, System.Xml.Serialization.NameTable choiceElements, string identifier, string ns, ref bool needExplicitOrder, bool allowDuplicates)
        {
            if (particle is XmlSchemaGroupRef)
            {
                XmlSchemaGroupRef ref2 = (XmlSchemaGroupRef) particle;
                if (!ref2.RefName.IsEmpty)
                {
                    base.AddReference(ref2.RefName, base.GroupsInUse, "XmlCircularGroupReference");
                    if (this.GatherGroupChoices(this.FindGroup(ref2.RefName), choiceElements, identifier, ref2.RefName.Namespace, ref needExplicitOrder, allowDuplicates))
                    {
                        base.RemoveReference(ref2.RefName, base.GroupsInUse);
                        return true;
                    }
                    base.RemoveReference(ref2.RefName, base.GroupsInUse);
                }
            }
            else if (particle is XmlSchemaGroupBase)
            {
                XmlSchemaGroupBase base2 = (XmlSchemaGroupBase) particle;
                bool isMultipleOccurrence = base2.IsMultipleOccurrence;
                XmlSchemaAny any = null;
                bool duplicateElements = false;
                for (int i = 0; i < base2.Items.Count; i++)
                {
                    object obj2 = base2.Items[i];
                    if ((obj2 is XmlSchemaGroupBase) || (obj2 is XmlSchemaGroupRef))
                    {
                        if (this.GatherGroupChoices((XmlSchemaParticle) obj2, choiceElements, identifier, ns, ref needExplicitOrder, allowDuplicates))
                        {
                            isMultipleOccurrence = true;
                        }
                    }
                    else if (obj2 is XmlSchemaAny)
                    {
                        if (this.GenerateOrder)
                        {
                            this.AddScopeElements(choiceElements, this.ImportAny((XmlSchemaAny) obj2, true, ns), ref duplicateElements, allowDuplicates);
                        }
                        else
                        {
                            any = (XmlSchemaAny) obj2;
                        }
                    }
                    else if (obj2 is XmlSchemaElement)
                    {
                        XmlSchemaElement element = (XmlSchemaElement) obj2;
                        XmlSchemaElement topLevelElement = this.GetTopLevelElement(element);
                        if (topLevelElement != null)
                        {
                            XmlSchemaElement[] equivalentElements = this.GetEquivalentElements(topLevelElement);
                            for (int j = 0; j < equivalentElements.Length; j++)
                            {
                                if (equivalentElements[j].IsMultipleOccurrence)
                                {
                                    isMultipleOccurrence = true;
                                }
                                this.AddScopeElement(choiceElements, this.ImportElement(equivalentElements[j], identifier, typeof(TypeMapping), null, equivalentElements[j].QualifiedName.Namespace, true), ref duplicateElements, allowDuplicates);
                            }
                        }
                        if (element.IsMultipleOccurrence)
                        {
                            isMultipleOccurrence = true;
                        }
                        this.AddScopeElement(choiceElements, this.ImportElement(element, identifier, typeof(TypeMapping), null, element.QualifiedName.Namespace, false), ref duplicateElements, allowDuplicates);
                    }
                }
                if (any != null)
                {
                    this.AddScopeElements(choiceElements, this.ImportAny(any, true, ns), ref duplicateElements, allowDuplicates);
                }
                if ((!isMultipleOccurrence && !(base2 is XmlSchemaChoice)) && (base2.Items.Count > 1))
                {
                    isMultipleOccurrence = true;
                }
                return isMultipleOccurrence;
            }
            return false;
        }

        private string GenerateUniqueTypeName(string desiredName, string ns)
        {
            int num = 1;
            string str = desiredName;
            while (true)
            {
                XmlQualifiedName name = new XmlQualifiedName(str, ns);
                if (base.Schemas.Find(name, typeof(XmlSchemaType)) == null)
                {
                    break;
                }
                str = desiredName + num.ToString(CultureInfo.InvariantCulture);
                num++;
            }
            str = CodeIdentifier.MakeValid(str);
            return base.TypeIdentifiers.AddUnique(str, str);
        }

        private TypeDesc GetDataTypeSource(XmlSchemaSimpleType dataType, TypeFlags flags)
        {
            TypeDesc dataTypeSource = null;
            if ((dataType.Name != null) && (dataType.Name.Length != 0))
            {
                dataTypeSource = base.Scope.GetTypeDesc(dataType);
                if (dataTypeSource != null)
                {
                    return dataTypeSource;
                }
            }
            XmlQualifiedName name = BaseTypeName(dataType);
            base.AddReference(name, base.TypesInUse, "XmlCircularTypeReference");
            dataTypeSource = this.GetDataTypeSource(this.FindDataType(name, flags), flags);
            if (name.Namespace != "http://www.w3.org/2001/XMLSchema")
            {
                base.RemoveReference(name, base.TypesInUse);
            }
            return dataTypeSource;
        }

        internal TypeMapping GetDefaultMapping(TypeFlags flags)
        {
            PrimitiveMapping mapping;
            return new PrimitiveMapping { TypeDesc = base.Scope.GetTypeDesc("string", "http://www.w3.org/2001/XMLSchema", flags), TypeName = mapping.TypeDesc.DataType.Name, Namespace = "http://www.w3.org/2001/XMLSchema" };
        }

        private XmlSchemaElement[] GetEquivalentElements(XmlSchemaElement element)
        {
            ArrayList list = new ArrayList();
            foreach (XmlSchema schema in base.Schemas.SchemaSet.Schemas())
            {
                for (int i = 0; i < schema.Items.Count; i++)
                {
                    object obj2 = schema.Items[i];
                    if (obj2 is XmlSchemaElement)
                    {
                        XmlSchemaElement element2 = (XmlSchemaElement) obj2;
                        if ((!element2.IsAbstract && (element2.SubstitutionGroup.Namespace == schema.TargetNamespace)) && (element2.SubstitutionGroup.Name == element.Name))
                        {
                            list.Add(element2);
                        }
                    }
                }
            }
            return (XmlSchemaElement[]) list.ToArray(typeof(XmlSchemaElement));
        }

        private XmlSchemaElement GetTopLevelElement(XmlSchemaElement element)
        {
            if (!element.RefName.IsEmpty)
            {
                return this.FindElement(element.RefName);
            }
            return null;
        }

        private TypeItems GetTypeItems(XmlSchemaType type)
        {
            TypeItems items = new TypeItems();
            if (type is XmlSchemaComplexType)
            {
                XmlSchemaParticle particle = null;
                XmlSchemaComplexType type2 = (XmlSchemaComplexType) type;
                if (type2.ContentModel != null)
                {
                    XmlSchemaContent content = type2.ContentModel.Content;
                    if (content is XmlSchemaComplexContentExtension)
                    {
                        XmlSchemaComplexContentExtension extension = (XmlSchemaComplexContentExtension) content;
                        items.Attributes = extension.Attributes;
                        items.AnyAttribute = extension.AnyAttribute;
                        particle = extension.Particle;
                    }
                    else if (content is XmlSchemaSimpleContentExtension)
                    {
                        XmlSchemaSimpleContentExtension extension2 = (XmlSchemaSimpleContentExtension) content;
                        items.Attributes = extension2.Attributes;
                        items.AnyAttribute = extension2.AnyAttribute;
                        items.baseSimpleType = extension2.BaseTypeName;
                    }
                }
                else
                {
                    items.Attributes = type2.Attributes;
                    items.AnyAttribute = type2.AnyAttribute;
                    particle = type2.Particle;
                }
                if (particle is XmlSchemaGroupRef)
                {
                    XmlSchemaGroupRef ref2 = (XmlSchemaGroupRef) particle;
                    items.Particle = this.FindGroup(ref2.RefName).Particle;
                    items.IsUnbounded = particle.IsMultipleOccurrence;
                    return items;
                }
                if (particle is XmlSchemaGroupBase)
                {
                    items.Particle = (XmlSchemaGroupBase) particle;
                    items.IsUnbounded = particle.IsMultipleOccurrence;
                }
            }
            return items;
        }

        private ElementAccessor[] ImportAny(XmlSchemaAny any, bool makeElement, string targetNamespace)
        {
            SpecialMapping mapping;
            mapping = new SpecialMapping {
                TypeDesc = base.Scope.GetTypeDesc(makeElement ? typeof(XmlElement) : typeof(XmlNode)),
                TypeName = mapping.TypeDesc.Name
            };
            TypeFlags canBeElementValue = TypeFlags.CanBeElementValue;
            if (makeElement)
            {
                canBeElementValue |= TypeFlags.CanBeTextValue;
            }
            this.RunSchemaExtensions(mapping, XmlQualifiedName.Empty, null, any, canBeElementValue);
            if (this.GenerateOrder && (any.Namespace != null))
            {
                NamespaceList list = new NamespaceList(any.Namespace, targetNamespace);
                if (list.Type == NamespaceList.ListType.Set)
                {
                    ICollection enumerate = list.Enumerate;
                    ElementAccessor[] accessorArray = new ElementAccessor[(enumerate.Count == 0) ? 1 : enumerate.Count];
                    int num = 0;
                    foreach (string str in list.Enumerate)
                    {
                        ElementAccessor accessor = new ElementAccessor {
                            Mapping = mapping,
                            Any = true,
                            Namespace = str
                        };
                        accessorArray[num++] = accessor;
                    }
                    if (num > 0)
                    {
                        return accessorArray;
                    }
                }
            }
            ElementAccessor accessor2 = new ElementAccessor {
                Mapping = mapping,
                Any = true
            };
            return new ElementAccessor[] { accessor2 };
        }

        private void ImportAnyAttributeMember(XmlSchemaAnyAttribute any, CodeIdentifiers members, CodeIdentifiers membersScope)
        {
            SpecialMapping mapping;
            MemberMapping mapping2;
            mapping = new SpecialMapping {
                TypeDesc = base.Scope.GetTypeDesc(typeof(XmlAttribute)),
                TypeName = mapping.TypeDesc.Name
            };
            AttributeAccessor accessor = new AttributeAccessor {
                Any = true,
                Mapping = mapping
            };
            mapping2 = new MemberMapping {
                Elements = new ElementAccessor[0],
                Attribute = accessor,
                Name = membersScope.MakeRightCase("AnyAttr"),
                Name = membersScope.AddUnique(mapping2.Name, mapping2)
            };
            members.Add(mapping2.Name, mapping2);
            mapping2.TypeDesc = accessor.Mapping.TypeDesc;
            mapping2.TypeDesc = mapping2.TypeDesc.CreateArrayTypeDesc();
        }

        private SpecialMapping ImportAnyMapping(XmlSchemaType type, string identifier, string ns, bool repeats)
        {
            if (type == null)
            {
                return null;
            }
            if (!type.DerivedFrom.IsEmpty)
            {
                return null;
            }
            bool flag = IsMixed(type);
            TypeItems typeItems = this.GetTypeItems(type);
            if (typeItems.Particle == null)
            {
                return null;
            }
            if (!(typeItems.Particle is XmlSchemaAll) && !(typeItems.Particle is XmlSchemaSequence))
            {
                return null;
            }
            if ((typeItems.Attributes != null) && (typeItems.Attributes.Count > 0))
            {
                return null;
            }
            XmlSchemaGroupBase particle = typeItems.Particle;
            if ((particle.Items.Count != 1) || !(particle.Items[0] is XmlSchemaAny))
            {
                return null;
            }
            XmlSchemaAny context = (XmlSchemaAny) particle.Items[0];
            SpecialMapping mapping = new SpecialMapping();
            if (((typeItems.AnyAttribute != null) && context.IsMultipleOccurrence) && flag)
            {
                mapping.NamedAny = true;
                mapping.TypeDesc = base.Scope.GetTypeDesc(typeof(XmlElement));
            }
            else
            {
                if ((typeItems.AnyAttribute != null) || context.IsMultipleOccurrence)
                {
                    return null;
                }
                mapping.TypeDesc = base.Scope.GetTypeDesc(flag ? typeof(XmlNode) : typeof(XmlElement));
            }
            TypeFlags canBeElementValue = TypeFlags.CanBeElementValue;
            if ((typeItems.AnyAttribute != null) || flag)
            {
                canBeElementValue |= TypeFlags.CanBeTextValue;
            }
            this.RunSchemaExtensions(mapping, XmlQualifiedName.Empty, null, context, canBeElementValue);
            mapping.TypeName = mapping.TypeDesc.Name;
            if (repeats)
            {
                mapping.TypeDesc = mapping.TypeDesc.CreateArrayTypeDesc();
            }
            return mapping;
        }

        private MemberMapping ImportAnyMember(XmlSchemaAny any, string identifier, CodeIdentifiers members, CodeIdentifiers membersScope, INameScope elementsScope, string ns, ref bool mixed, ref bool needExplicitOrder, bool allowDuplicates)
        {
            MemberMapping mapping;
            ElementAccessor[] elements = this.ImportAny(any, !mixed, ns);
            this.AddScopeElements(elementsScope, elements, ref needExplicitOrder, allowDuplicates);
            mapping = new MemberMapping {
                Elements = elements,
                Name = membersScope.MakeRightCase("Any"),
                Name = membersScope.AddUnique(mapping.Name, mapping)
            };
            members.Add(mapping.Name, mapping);
            mapping.TypeDesc = elements[0].Mapping.TypeDesc;
            bool isMultipleOccurrence = any.IsMultipleOccurrence;
            if (mixed)
            {
                SpecialMapping mapping2;
                mapping2 = new SpecialMapping {
                    TypeDesc = base.Scope.GetTypeDesc(typeof(XmlNode)),
                    TypeName = mapping2.TypeDesc.Name
                };
                mapping.TypeDesc = mapping2.TypeDesc;
                TextAccessor accessor = new TextAccessor {
                    Mapping = mapping2
                };
                mapping.Text = accessor;
                isMultipleOccurrence = true;
                mixed = false;
            }
            if (isMultipleOccurrence)
            {
                mapping.TypeDesc = mapping.TypeDesc.CreateArrayTypeDesc();
            }
            return mapping;
        }

        public XmlMembersMapping ImportAnyType(XmlQualifiedName typeName, string elementName)
        {
            MembersMapping mapping2 = this.ImportType(typeName, typeof(MembersMapping), null, TypeFlags.CanBeElementValue, true) as MembersMapping;
            if (mapping2 == null)
            {
                XmlSchemaComplexType type = new XmlSchemaComplexType();
                XmlSchemaSequence sequence = new XmlSchemaSequence();
                type.Particle = sequence;
                XmlSchemaElement item = new XmlSchemaElement {
                    Name = elementName,
                    SchemaTypeName = typeName
                };
                sequence.Items.Add(item);
                mapping2 = this.ImportMembersType(type, typeName.Namespace, elementName);
            }
            if ((mapping2.Members.Length != 1) || !mapping2.Members[0].Accessor.Any)
            {
                return null;
            }
            mapping2.Members[0].Name = elementName;
            ElementAccessor accessor = new ElementAccessor {
                Name = elementName,
                Namespace = typeName.Namespace,
                Mapping = mapping2,
                Any = true
            };
            XmlSchemaObject obj2 = base.Schemas.SchemaSet.GlobalTypes[typeName];
            if (obj2 != null)
            {
                XmlSchema parent = obj2.Parent as XmlSchema;
                if (parent != null)
                {
                    accessor.Form = (parent.ElementFormDefault == XmlSchemaForm.None) ? XmlSchemaForm.Unqualified : parent.ElementFormDefault;
                }
            }
            return new XmlMembersMapping(base.Scope, accessor, XmlMappingAccess.Write | XmlMappingAccess.Read);
        }

        private ElementAccessor ImportArray(XmlSchemaElement element, string identifier, string ns, bool repeats)
        {
            if (repeats)
            {
                return null;
            }
            if (element.SchemaType == null)
            {
                return null;
            }
            if (element.IsMultipleOccurrence)
            {
                return null;
            }
            XmlSchemaType schemaType = element.SchemaType;
            ArrayMapping mapping = this.ImportArrayMapping(schemaType, identifier, ns, repeats);
            if (mapping == null)
            {
                return null;
            }
            ElementAccessor accessor = new ElementAccessor {
                Name = element.Name,
                Namespace = ns,
                Mapping = mapping
            };
            if (mapping.TypeDesc.IsNullable)
            {
                accessor.IsNullable = element.IsNillable;
            }
            accessor.Form = this.ElementForm(ns, element);
            return accessor;
        }

        private ArrayMapping ImportArrayMapping(XmlSchemaType type, string identifier, string ns, bool repeats)
        {
            if (!(type is XmlSchemaComplexType))
            {
                return null;
            }
            if (!type.DerivedFrom.IsEmpty)
            {
                return null;
            }
            if (IsMixed(type))
            {
                return null;
            }
            Mapping mapping = (Mapping) base.ImportedMappings[type];
            if (mapping != null)
            {
                if (mapping is ArrayMapping)
                {
                    return (ArrayMapping) mapping;
                }
                return null;
            }
            TypeItems typeItems = this.GetTypeItems(type);
            if ((typeItems.Attributes != null) && (typeItems.Attributes.Count > 0))
            {
                return null;
            }
            if (typeItems.AnyAttribute != null)
            {
                return null;
            }
            if (typeItems.Particle == null)
            {
                return null;
            }
            XmlSchemaGroupBase particle = typeItems.Particle;
            ArrayMapping typeMapping = new ArrayMapping {
                TypeName = identifier,
                Namespace = ns
            };
            if (particle is XmlSchemaChoice)
            {
                XmlSchemaChoice group = (XmlSchemaChoice) particle;
                if (!group.IsMultipleOccurrence)
                {
                    return null;
                }
                bool needExplicitOrder = false;
                MemberMapping mapping3 = this.ImportChoiceGroup(group, identifier, null, null, null, ns, true, ref needExplicitOrder, false);
                if (mapping3.ChoiceIdentifier != null)
                {
                    return null;
                }
                typeMapping.TypeDesc = mapping3.TypeDesc;
                typeMapping.Elements = mapping3.Elements;
                typeMapping.TypeName = ((type.Name == null) || (type.Name.Length == 0)) ? ("ArrayOf" + CodeIdentifier.MakePascal(typeMapping.TypeDesc.Name)) : type.Name;
            }
            else
            {
                if (!(particle is XmlSchemaAll) && !(particle is XmlSchemaSequence))
                {
                    return null;
                }
                if ((particle.Items.Count != 1) || !(particle.Items[0] is XmlSchemaElement))
                {
                    return null;
                }
                XmlSchemaElement element = (XmlSchemaElement) particle.Items[0];
                if (!element.IsMultipleOccurrence)
                {
                    return null;
                }
                List<string> identifiers = new List<string>(1) {
                    identifier
                };
                if (this.IsCyclicReferencedType(element, identifiers))
                {
                    return null;
                }
                ElementAccessor accessor = this.ImportElement(element, identifier, typeof(TypeMapping), null, ns, false);
                if (accessor.Any)
                {
                    return null;
                }
                typeMapping.Elements = new ElementAccessor[] { accessor };
                typeMapping.TypeDesc = accessor.Mapping.TypeDesc.CreateArrayTypeDesc();
                typeMapping.TypeName = ((type.Name == null) || (type.Name.Length == 0)) ? ("ArrayOf" + CodeIdentifier.MakePascal(accessor.Mapping.TypeDesc.Name)) : type.Name;
            }
            base.ImportedMappings[type] = typeMapping;
            base.Scope.AddTypeMapping(typeMapping);
            typeMapping.TopLevelMapping = this.ImportStructType(type, ns, identifier, null, true);
            typeMapping.TopLevelMapping.ReferencedByTopLevelElement = true;
            if ((type.Name != null) && (type.Name.Length != 0))
            {
                this.ImportDerivedTypes(new XmlQualifiedName(identifier, ns));
            }
            return typeMapping;
        }

        private AttributeAccessor ImportAttribute(XmlSchemaAttribute attribute, string identifier, string ns, XmlSchemaAttribute defaultValueProvider)
        {
            TypeMapping defaultMapping;
            if (attribute.Use == XmlSchemaUse.Prohibited)
            {
                return null;
            }
            if (!attribute.RefName.IsEmpty)
            {
                if (attribute.RefName.Namespace == "http://www.w3.org/XML/1998/namespace")
                {
                    return this.ImportSpecialAttribute(attribute.RefName, identifier);
                }
                return this.ImportAttribute(this.FindAttribute(attribute.RefName), identifier, attribute.RefName.Namespace, defaultValueProvider);
            }
            if (attribute.Name.Length == 0)
            {
                throw new InvalidOperationException(Res.GetString("XmlAttributeHasNoName"));
            }
            if (identifier.Length == 0)
            {
                identifier = CodeIdentifier.MakeValid(attribute.Name);
            }
            else
            {
                identifier = identifier + CodeIdentifier.MakePascal(attribute.Name);
            }
            if (!attribute.SchemaTypeName.IsEmpty)
            {
                defaultMapping = this.ImportType(attribute.SchemaTypeName, typeof(TypeMapping), null, TypeFlags.CanBeAttributeValue, false);
            }
            else if (attribute.SchemaType != null)
            {
                defaultMapping = this.ImportDataType(attribute.SchemaType, ns, identifier, null, TypeFlags.CanBeAttributeValue, false);
            }
            else
            {
                defaultMapping = this.GetDefaultMapping(TypeFlags.CanBeAttributeValue);
            }
            if ((defaultMapping != null) && !defaultMapping.TypeDesc.IsMappedType)
            {
                this.RunSchemaExtensions(defaultMapping, attribute.SchemaTypeName, attribute.SchemaType, attribute, TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue | TypeFlags.CanBeAttributeValue);
            }
            AttributeAccessor accessor = new AttributeAccessor {
                Name = attribute.Name,
                Namespace = ns,
                Form = this.AttributeForm(ns, attribute)
            };
            accessor.CheckSpecial();
            accessor.Mapping = defaultMapping;
            accessor.IsList = defaultMapping.IsList;
            accessor.IsOptional = attribute.Use != XmlSchemaUse.Required;
            if (defaultValueProvider.DefaultValue != null)
            {
                accessor.Default = defaultValueProvider.DefaultValue;
                return accessor;
            }
            if (defaultValueProvider.FixedValue != null)
            {
                accessor.Default = defaultValueProvider.FixedValue;
                accessor.IsFixed = true;
                return accessor;
            }
            if (attribute != defaultValueProvider)
            {
                if (attribute.DefaultValue != null)
                {
                    accessor.Default = attribute.DefaultValue;
                    return accessor;
                }
                if (attribute.FixedValue != null)
                {
                    accessor.Default = attribute.FixedValue;
                    accessor.IsFixed = true;
                }
            }
            return accessor;
        }

        private void ImportAttributeGroupMembers(XmlSchemaAttributeGroup group, string identifier, CodeIdentifiers members, CodeIdentifiers membersScope, string ns)
        {
            for (int i = 0; i < group.Attributes.Count; i++)
            {
                object obj2 = group.Attributes[i];
                if (obj2 is XmlSchemaAttributeGroup)
                {
                    this.ImportAttributeGroupMembers((XmlSchemaAttributeGroup) obj2, identifier, members, membersScope, ns);
                }
                else if (obj2 is XmlSchemaAttribute)
                {
                    this.ImportAttributeMember((XmlSchemaAttribute) obj2, identifier, members, membersScope, ns);
                }
            }
            if (group.AnyAttribute != null)
            {
                this.ImportAnyAttributeMember(group.AnyAttribute, members, membersScope);
            }
        }

        private void ImportAttributeMember(XmlSchemaAttribute attribute, string identifier, CodeIdentifiers members, CodeIdentifiers membersScope, string ns)
        {
            AttributeAccessor accessor = this.ImportAttribute(attribute, identifier, ns, attribute);
            if (accessor != null)
            {
                MemberMapping mapping;
                mapping = new MemberMapping {
                    Elements = new ElementAccessor[0],
                    Attribute = accessor,
                    Name = CodeIdentifier.MakeValid(Accessor.UnescapeName(accessor.Name)),
                    Name = membersScope.AddUnique(mapping.Name, mapping)
                };
                if (mapping.Name.EndsWith("Specified", StringComparison.Ordinal))
                {
                    string name = mapping.Name;
                    mapping.Name = membersScope.AddUnique(mapping.Name, mapping);
                    membersScope.Remove(name);
                }
                members.Add(mapping.Name, mapping);
                mapping.TypeDesc = accessor.IsList ? accessor.Mapping.TypeDesc.CreateArrayTypeDesc() : accessor.Mapping.TypeDesc;
                if (((attribute.Use == XmlSchemaUse.Optional) || (attribute.Use == XmlSchemaUse.None)) && ((mapping.TypeDesc.IsValueType && !attribute.HasDefault) && !mapping.TypeDesc.HasIsEmpty))
                {
                    mapping.CheckSpecified = SpecifiedAccessor.ReadWrite;
                }
            }
        }

        private MemberMapping ImportChoiceGroup(XmlSchemaGroupBase group, string identifier, CodeIdentifiers members, CodeIdentifiers membersScope, INameScope elementsScope, string ns, bool groupRepeats, ref bool needExplicitOrder, bool allowDuplicates)
        {
            System.Xml.Serialization.NameTable choiceElements = new System.Xml.Serialization.NameTable();
            if (this.GatherGroupChoices(group, choiceElements, identifier, ns, ref needExplicitOrder, allowDuplicates))
            {
                groupRepeats = true;
            }
            MemberMapping mapping = new MemberMapping {
                Elements = (ElementAccessor[]) choiceElements.ToArray(typeof(ElementAccessor))
            };
            Array.Sort(mapping.Elements, new ElementComparer());
            this.AddScopeElements(elementsScope, mapping.Elements, ref needExplicitOrder, allowDuplicates);
            bool flag = false;
            bool flag2 = false;
            Hashtable hashtable = new Hashtable(mapping.Elements.Length);
            for (int i = 0; i < mapping.Elements.Length; i++)
            {
                ElementAccessor accessor = mapping.Elements[i];
                string fullName = accessor.Mapping.TypeDesc.FullName;
                object obj2 = hashtable[fullName];
                if (obj2 != null)
                {
                    flag = true;
                    ElementAccessor accessor2 = (ElementAccessor) obj2;
                    if (!flag2 && (accessor2.IsNullable != accessor.IsNullable))
                    {
                        flag2 = true;
                    }
                }
                else
                {
                    hashtable.Add(fullName, accessor);
                }
                ArrayMapping arrayMapping = accessor.Mapping as ArrayMapping;
                if ((arrayMapping != null) && this.IsNeedXmlSerializationAttributes(arrayMapping))
                {
                    accessor.Mapping = arrayMapping.TopLevelMapping;
                    accessor.Mapping.ReferencedByTopLevelElement = false;
                    accessor.Mapping.ReferencedByElement = true;
                }
            }
            if (flag2)
            {
                mapping.TypeDesc = base.Scope.GetTypeDesc(typeof(object));
            }
            else
            {
                TypeDesc[] typeDescs = new TypeDesc[hashtable.Count];
                IEnumerator enumerator = hashtable.Values.GetEnumerator();
                for (int j = 0; j < typeDescs.Length; j++)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    typeDescs[j] = ((ElementAccessor) enumerator.Current).Mapping.TypeDesc;
                }
                mapping.TypeDesc = TypeDesc.FindCommonBaseTypeDesc(typeDescs);
                if (mapping.TypeDesc == null)
                {
                    mapping.TypeDesc = base.Scope.GetTypeDesc(typeof(object));
                }
            }
            if (groupRepeats)
            {
                mapping.TypeDesc = mapping.TypeDesc.CreateArrayTypeDesc();
            }
            if (membersScope != null)
            {
                mapping.Name = membersScope.AddUnique(groupRepeats ? "Items" : "Item", mapping);
                if (members != null)
                {
                    members.Add(mapping.Name, mapping);
                }
            }
            if (flag)
            {
                mapping.ChoiceIdentifier = new ChoiceIdentifierAccessor();
                mapping.ChoiceIdentifier.MemberName = mapping.Name + "ElementName";
                mapping.ChoiceIdentifier.Mapping = this.ImportEnumeratedChoice(mapping.Elements, ns, mapping.Name + "ChoiceType");
                mapping.ChoiceIdentifier.MemberIds = new string[mapping.Elements.Length];
                ConstantMapping[] constants = ((EnumMapping) mapping.ChoiceIdentifier.Mapping).Constants;
                for (int k = 0; k < mapping.Elements.Length; k++)
                {
                    mapping.ChoiceIdentifier.MemberIds[k] = constants[k].Name;
                }
                MemberMapping mapping3 = new MemberMapping {
                    Ignore = true,
                    Name = mapping.ChoiceIdentifier.MemberName
                };
                if (groupRepeats)
                {
                    mapping3.TypeDesc = mapping.ChoiceIdentifier.Mapping.TypeDesc.CreateArrayTypeDesc();
                }
                else
                {
                    mapping3.TypeDesc = mapping.ChoiceIdentifier.Mapping.TypeDesc;
                }
                ElementAccessor accessor3 = new ElementAccessor {
                    Name = mapping3.Name,
                    Namespace = ns,
                    Mapping = mapping.ChoiceIdentifier.Mapping
                };
                mapping3.Elements = new ElementAccessor[] { accessor3 };
                if (membersScope != null)
                {
                    accessor3.Name = mapping3.Name = mapping.ChoiceIdentifier.MemberName = membersScope.AddUnique(mapping.ChoiceIdentifier.MemberName, mapping3);
                    if (members != null)
                    {
                        members.Add(accessor3.Name, mapping3);
                    }
                }
            }
            return mapping;
        }

        private TypeMapping ImportDataType(XmlSchemaSimpleType dataType, string typeNs, string identifier, Type baseType, TypeFlags flags, bool isList)
        {
            if (baseType != null)
            {
                return this.ImportStructDataType(dataType, typeNs, identifier, baseType);
            }
            TypeMapping mapping = this.ImportNonXsdPrimitiveDataType(dataType, typeNs, flags);
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
                            return this.ImportEnumeratedDataType(dataType, typeNs, identifier, flags, isList);
                        }
                    }
                }
                if (content.BaseType != null)
                {
                    return this.ImportDataType(content.BaseType, typeNs, identifier, null, flags, false);
                }
                base.AddReference(content.BaseTypeName, base.TypesInUse, "XmlCircularTypeReference");
                mapping = this.ImportDataType(this.FindDataType(content.BaseTypeName, flags), content.BaseTypeName.Namespace, identifier, null, flags, false);
                if (content.BaseTypeName.Namespace != "http://www.w3.org/2001/XMLSchema")
                {
                    base.RemoveReference(content.BaseTypeName, base.TypesInUse);
                }
                return mapping;
            }
            if (!(dataType.Content is XmlSchemaSimpleTypeList) && !(dataType.Content is XmlSchemaSimpleTypeUnion))
            {
                return this.ImportPrimitiveDataType(dataType, flags);
            }
            if (dataType.Content is XmlSchemaSimpleTypeList)
            {
                XmlSchemaSimpleTypeList list = (XmlSchemaSimpleTypeList) dataType.Content;
                if (list.ItemType != null)
                {
                    mapping = this.ImportDataType(list.ItemType, typeNs, identifier, null, flags, true);
                    if (mapping != null)
                    {
                        mapping.TypeName = dataType.Name;
                        return mapping;
                    }
                }
                else if ((list.ItemTypeName != null) && !list.ItemTypeName.IsEmpty)
                {
                    mapping = this.ImportType(list.ItemTypeName, typeof(TypeMapping), null, TypeFlags.CanBeAttributeValue, true);
                    if ((mapping != null) && (mapping is PrimitiveMapping))
                    {
                        ((PrimitiveMapping) mapping).IsList = true;
                        return mapping;
                    }
                }
            }
            return this.GetDefaultMapping(flags);
        }

        public XmlTypeMapping ImportDerivedTypeMapping(XmlQualifiedName name, Type baseType)
        {
            return this.ImportDerivedTypeMapping(name, baseType, false);
        }

        public XmlTypeMapping ImportDerivedTypeMapping(XmlQualifiedName name, Type baseType, bool baseTypeCanBeIndirect)
        {
            ElementAccessor accessor = this.ImportElement(name, typeof(TypeMapping), baseType);
            if (accessor.Mapping is StructMapping)
            {
                base.MakeDerived((StructMapping) accessor.Mapping, baseType, baseTypeCanBeIndirect);
            }
            else if (baseType != null)
            {
                if (!(accessor.Mapping is ArrayMapping))
                {
                    throw new InvalidOperationException(Res.GetString("XmlBadBaseElement", new object[] { name.Name, name.Namespace, baseType.FullName }));
                }
                accessor.Mapping = ((ArrayMapping) accessor.Mapping).TopLevelMapping;
                base.MakeDerived((StructMapping) accessor.Mapping, baseType, baseTypeCanBeIndirect);
            }
            return new XmlTypeMapping(base.Scope, accessor);
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
                            if ((type.DerivedFrom == baseName) && (base.TypesInUse[type.Name, schema.TargetNamespace] == null))
                            {
                                this.ImportType(type.QualifiedName, typeof(TypeMapping), null, TypeFlags.CanBeElementValue, false);
                            }
                        }
                    }
                }
            }
        }

        private ElementAccessor ImportElement(XmlQualifiedName name, Type desiredMappingType, Type baseType)
        {
            XmlSchemaElement element = this.FindElement(name);
            ElementAccessor accessor = (ElementAccessor) base.ImportedElements[element];
            if (accessor == null)
            {
                accessor = this.ImportElement(element, string.Empty, desiredMappingType, baseType, name.Namespace, true);
                ElementAccessor accessor2 = (ElementAccessor) base.ImportedElements[element];
                if (accessor2 != null)
                {
                    return accessor2;
                }
                base.ImportedElements.Add(element, accessor);
            }
            return accessor;
        }

        private ElementAccessor ImportElement(XmlSchemaElement element, string identifier, Type desiredMappingType, Type baseType, string ns, bool topLevelElement)
        {
            if (!element.RefName.IsEmpty)
            {
                ElementAccessor accessor = this.ImportElement(element.RefName, desiredMappingType, baseType);
                if (element.IsMultipleOccurrence && (accessor.Mapping is ArrayMapping))
                {
                    ElementAccessor accessor2 = accessor.Clone();
                    accessor2.IsTopLevelInSchema = false;
                    accessor2.Mapping.ReferencedByElement = true;
                    return accessor2;
                }
                return accessor;
            }
            if (element.Name.Length == 0)
            {
                XmlQualifiedName parentName = XmlSchemas.GetParentName(element);
                throw new InvalidOperationException(Res.GetString("XmlElementHasNoName", new object[] { parentName.Name, parentName.Namespace }));
            }
            string str = Accessor.UnescapeName(element.Name);
            if (identifier.Length == 0)
            {
                identifier = CodeIdentifier.MakeValid(str);
            }
            else
            {
                identifier = identifier + CodeIdentifier.MakePascal(str);
            }
            TypeMapping mapping = this.ImportElementType(element, identifier, desiredMappingType, baseType, ns);
            ElementAccessor accessor3 = new ElementAccessor {
                IsTopLevelInSchema = element.Parent is XmlSchema,
                Name = element.Name,
                Namespace = ns,
                Mapping = mapping,
                IsOptional = element.MinOccurs == 0M
            };
            if (element.DefaultValue != null)
            {
                accessor3.Default = element.DefaultValue;
            }
            else if (element.FixedValue != null)
            {
                accessor3.Default = element.FixedValue;
                accessor3.IsFixed = true;
            }
            if ((mapping is SpecialMapping) && ((SpecialMapping) mapping).NamedAny)
            {
                accessor3.Any = true;
            }
            accessor3.IsNullable = element.IsNillable;
            if (topLevelElement)
            {
                accessor3.Form = XmlSchemaForm.Qualified;
                return accessor3;
            }
            accessor3.Form = this.ElementForm(ns, element);
            return accessor3;
        }

        private void ImportElementMember(XmlSchemaElement element, string identifier, CodeIdentifiers members, CodeIdentifiers membersScope, INameScope elementsScope, string ns, bool repeats, ref bool needExplicitOrder, bool allowDuplicates, bool allowUnboundedElements)
        {
            repeats |= element.IsMultipleOccurrence;
            XmlSchemaElement topLevelElement = this.GetTopLevelElement(element);
            if ((topLevelElement == null) || !this.ImportSubstitutionGroupMember(topLevelElement, identifier, members, membersScope, ns, repeats, ref needExplicitOrder, allowDuplicates))
            {
                ElementAccessor accessor = this.ImportArray(element, identifier, ns, repeats);
                if (accessor == null)
                {
                    accessor = this.ImportElement(element, identifier, typeof(TypeMapping), null, ns, false);
                }
                MemberMapping mapping = new MemberMapping();
                string name = CodeIdentifier.MakeValid(Accessor.UnescapeName(accessor.Name));
                mapping.Name = membersScope.AddUnique(name, mapping);
                if (mapping.Name.EndsWith("Specified", StringComparison.Ordinal))
                {
                    name = mapping.Name;
                    mapping.Name = membersScope.AddUnique(mapping.Name, mapping);
                    membersScope.Remove(name);
                }
                members.Add(mapping.Name, mapping);
                if (accessor.Mapping.IsList)
                {
                    accessor.Mapping = this.GetDefaultMapping(TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue);
                    mapping.TypeDesc = accessor.Mapping.TypeDesc;
                }
                else
                {
                    mapping.TypeDesc = accessor.Mapping.TypeDesc;
                }
                this.AddScopeElement(elementsScope, accessor, ref needExplicitOrder, allowDuplicates);
                mapping.Elements = new ElementAccessor[] { accessor };
                if (element.IsMultipleOccurrence || repeats)
                {
                    if (!allowUnboundedElements && (accessor.Mapping is ArrayMapping))
                    {
                        accessor.Mapping = ((ArrayMapping) accessor.Mapping).TopLevelMapping;
                        accessor.Mapping.ReferencedByTopLevelElement = false;
                        accessor.Mapping.ReferencedByElement = true;
                    }
                    mapping.TypeDesc = accessor.Mapping.TypeDesc.CreateArrayTypeDesc();
                }
                if (((element.MinOccurs == 0M) && mapping.TypeDesc.IsValueType) && (!element.HasDefault && !mapping.TypeDesc.HasIsEmpty))
                {
                    mapping.CheckSpecified = SpecifiedAccessor.ReadWrite;
                }
            }
        }

        private TypeMapping ImportElementType(XmlSchemaElement element, string identifier, Type desiredMappingType, Type baseType, string ns)
        {
            TypeMapping mapping;
            if (!element.SchemaTypeName.IsEmpty)
            {
                mapping = this.ImportType(element.SchemaTypeName, desiredMappingType, baseType, TypeFlags.CanBeElementValue, false);
                if (!mapping.ReferencedByElement)
                {
                    object obj2 = this.FindType(element.SchemaTypeName, TypeFlags.CanBeElementValue);
                    XmlSchemaObject parent = element;
                    while ((parent.Parent != null) && (obj2 != parent))
                    {
                        parent = parent.Parent;
                    }
                    mapping.ReferencedByElement = obj2 != parent;
                }
            }
            else if (element.SchemaType != null)
            {
                if (element.SchemaType is XmlSchemaComplexType)
                {
                    mapping = this.ImportType((XmlSchemaComplexType) element.SchemaType, ns, identifier, desiredMappingType, baseType, TypeFlags.CanBeElementValue);
                }
                else
                {
                    mapping = this.ImportDataType((XmlSchemaSimpleType) element.SchemaType, ns, identifier, baseType, TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue | TypeFlags.CanBeAttributeValue, false);
                }
                mapping.ReferencedByElement = true;
            }
            else if (!element.SubstitutionGroup.IsEmpty)
            {
                mapping = this.ImportElementType(this.FindElement(element.SubstitutionGroup), identifier, desiredMappingType, baseType, ns);
            }
            else if (desiredMappingType == typeof(MembersMapping))
            {
                mapping = this.ImportMembersType(new XmlSchemaType(), ns, identifier);
            }
            else
            {
                mapping = base.ImportRootMapping();
            }
            if (!desiredMappingType.IsAssignableFrom(mapping.GetType()))
            {
                throw new InvalidOperationException(Res.GetString("XmlElementImportedTwice", new object[] { element.Name, ns, mapping.GetType().Name, desiredMappingType.Name }));
            }
            if (!mapping.TypeDesc.IsMappedType)
            {
                this.RunSchemaExtensions(mapping, element.SchemaTypeName, element.SchemaType, element, TypeFlags.CanBeElementValue);
            }
            return mapping;
        }

        private EnumMapping ImportEnumeratedChoice(ElementAccessor[] choice, string typeNs, string typeName)
        {
            typeName = this.GenerateUniqueTypeName(Accessor.UnescapeName(typeName), typeNs);
            EnumMapping typeMapping = new EnumMapping {
                TypeDesc = new TypeDesc(typeName, typeName, TypeKind.Enum, null, TypeFlags.None),
                TypeName = typeName,
                Namespace = typeNs,
                IsFlags = false,
                IncludeInSchema = false
            };
            if (this.GenerateOrder)
            {
                Array.Sort(choice, new ElementComparer());
            }
            CodeIdentifiers identifiers = new CodeIdentifiers();
            for (int i = 0; i < choice.Length; i++)
            {
                ElementAccessor accessor = choice[i];
                ConstantMapping mapping2 = new ConstantMapping();
                string identifier = CodeIdentifier.MakeValid(accessor.Name);
                mapping2.Name = identifiers.AddUnique(identifier, mapping2);
                mapping2.XmlName = accessor.ToString(typeNs);
                mapping2.Value = i;
            }
            typeMapping.Constants = (ConstantMapping[]) identifiers.ToArray(typeof(ConstantMapping));
            base.Scope.AddTypeMapping(typeMapping);
            return typeMapping;
        }

        private TypeMapping ImportEnumeratedDataType(XmlSchemaSimpleType dataType, string typeNs, string identifier, TypeFlags flags, bool isList)
        {
            TypeMapping defaultMapping = (TypeMapping) base.ImportedMappings[dataType];
            if (defaultMapping != null)
            {
                return defaultMapping;
            }
            XmlSchemaType type = dataType;
            while (!type.DerivedFrom.IsEmpty)
            {
                type = this.FindType(type.DerivedFrom, TypeFlags.CanBeElementValue | TypeFlags.CanBeAttributeValue);
            }
            if (type is XmlSchemaComplexType)
            {
                return null;
            }
            TypeDesc typeDesc = base.Scope.GetTypeDesc((XmlSchemaSimpleType) type);
            if ((typeDesc != null) && (typeDesc.FullName != typeof(string).FullName))
            {
                return this.ImportPrimitiveDataType(dataType, flags);
            }
            identifier = Accessor.UnescapeName(identifier);
            string name = base.GenerateUniqueTypeName(identifier);
            EnumMapping mapping2 = new EnumMapping {
                IsReference = base.Schemas.IsReference(dataType),
                TypeDesc = new TypeDesc(name, name, TypeKind.Enum, null, TypeFlags.None)
            };
            if ((dataType.Name != null) && (dataType.Name.Length > 0))
            {
                mapping2.TypeName = identifier;
            }
            mapping2.Namespace = typeNs;
            mapping2.IsFlags = isList;
            CodeIdentifiers identifiers = new CodeIdentifiers();
            XmlSchemaSimpleTypeContent content = dataType.Content;
            if (content is XmlSchemaSimpleTypeRestriction)
            {
                XmlSchemaSimpleTypeRestriction restriction = (XmlSchemaSimpleTypeRestriction) content;
                for (int i = 0; i < restriction.Facets.Count; i++)
                {
                    object obj2 = restriction.Facets[i];
                    if (obj2 is XmlSchemaEnumerationFacet)
                    {
                        XmlSchemaEnumerationFacet facet = (XmlSchemaEnumerationFacet) obj2;
                        if ((typeDesc != null) && typeDesc.HasCustomFormatter)
                        {
                            XmlCustomFormatter.ToDefaultValue(facet.Value, typeDesc.FormatterName);
                        }
                        ConstantMapping mapping3 = new ConstantMapping();
                        string str2 = CodeIdentifier.MakeValid(facet.Value);
                        mapping3.Name = identifiers.AddUnique(str2, mapping3);
                        mapping3.XmlName = facet.Value;
                        mapping3.Value = i;
                    }
                }
            }
            mapping2.Constants = (ConstantMapping[]) identifiers.ToArray(typeof(ConstantMapping));
            if (isList && (mapping2.Constants.Length > 0x3f))
            {
                defaultMapping = this.GetDefaultMapping(TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue | TypeFlags.CanBeAttributeValue);
                base.ImportedMappings.Add(dataType, defaultMapping);
                return defaultMapping;
            }
            base.ImportedMappings.Add(dataType, mapping2);
            base.Scope.AddTypeMapping(mapping2);
            return mapping2;
        }

        private void ImportGroup(XmlSchemaGroupBase group, string identifier, CodeIdentifiers members, CodeIdentifiers membersScope, INameScope elementsScope, string ns, bool mixed, ref bool needExplicitOrder, bool allowDuplicates, bool groupRepeats, bool allowUnboundedElements)
        {
            if (group is XmlSchemaChoice)
            {
                this.ImportChoiceGroup((XmlSchemaChoice) group, identifier, members, membersScope, elementsScope, ns, groupRepeats, ref needExplicitOrder, allowDuplicates);
            }
            else
            {
                this.ImportGroupMembers(group, identifier, members, membersScope, elementsScope, ns, groupRepeats, ref mixed, ref needExplicitOrder, allowDuplicates, allowUnboundedElements);
            }
            if (mixed)
            {
                this.ImportTextMember(members, membersScope, null);
            }
        }

        private void ImportGroupMembers(XmlSchemaParticle particle, string identifier, CodeIdentifiers members, CodeIdentifiers membersScope, INameScope elementsScope, string ns, bool groupRepeats, ref bool mixed, ref bool needExplicitOrder, bool allowDuplicates, bool allowUnboundedElements)
        {
            if (particle is XmlSchemaGroupRef)
            {
                XmlSchemaGroupRef ref2 = (XmlSchemaGroupRef) particle;
                if (!ref2.RefName.IsEmpty)
                {
                    base.AddReference(ref2.RefName, base.GroupsInUse, "XmlCircularGroupReference");
                    this.ImportGroupMembers(this.FindGroup(ref2.RefName).Particle, identifier, members, membersScope, elementsScope, ref2.RefName.Namespace, groupRepeats | ref2.IsMultipleOccurrence, ref mixed, ref needExplicitOrder, allowDuplicates, allowUnboundedElements);
                    base.RemoveReference(ref2.RefName, base.GroupsInUse);
                }
            }
            else if (particle is XmlSchemaGroupBase)
            {
                XmlSchemaGroupBase group = (XmlSchemaGroupBase) particle;
                if (group.IsMultipleOccurrence)
                {
                    groupRepeats = true;
                }
                if ((this.GenerateOrder && groupRepeats) && (group.Items.Count > 1))
                {
                    this.ImportChoiceGroup(group, identifier, members, membersScope, elementsScope, ns, groupRepeats, ref needExplicitOrder, allowDuplicates);
                }
                else
                {
                    for (int i = 0; i < group.Items.Count; i++)
                    {
                        object obj2 = group.Items[i];
                        if (obj2 is XmlSchemaChoice)
                        {
                            this.ImportChoiceGroup((XmlSchemaGroupBase) obj2, identifier, members, membersScope, elementsScope, ns, groupRepeats, ref needExplicitOrder, allowDuplicates);
                        }
                        else if (obj2 is XmlSchemaElement)
                        {
                            this.ImportElementMember((XmlSchemaElement) obj2, identifier, members, membersScope, elementsScope, ns, groupRepeats, ref needExplicitOrder, allowDuplicates, allowUnboundedElements);
                        }
                        else if (obj2 is XmlSchemaAny)
                        {
                            this.ImportAnyMember((XmlSchemaAny) obj2, identifier, members, membersScope, elementsScope, ns, ref mixed, ref needExplicitOrder, allowDuplicates);
                        }
                        else if (obj2 is XmlSchemaParticle)
                        {
                            this.ImportGroupMembers((XmlSchemaParticle) obj2, identifier, members, membersScope, elementsScope, ns, groupRepeats, ref mixed, ref needExplicitOrder, allowDuplicates, true);
                        }
                    }
                }
            }
        }

        public XmlMembersMapping ImportMembersMapping(XmlQualifiedName name)
        {
            return new XmlMembersMapping(base.Scope, this.ImportElement(name, typeof(MembersMapping), null), XmlMappingAccess.Write | XmlMappingAccess.Read);
        }

        public XmlMembersMapping ImportMembersMapping(XmlQualifiedName[] names)
        {
            return this.ImportMembersMapping(names, null, false);
        }

        public XmlMembersMapping ImportMembersMapping(string name, string ns, SoapSchemaMember[] members)
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
            MembersMapping mapping = this.ImportMembersType(type, null, name);
            return new XmlMembersMapping(base.Scope, new ElementAccessor { Name = Accessor.EscapeName(name), Namespace = ns, Mapping = mapping, IsNullable = false, Form = XmlSchemaForm.Qualified }, XmlMappingAccess.Write | XmlMappingAccess.Read);
        }

        public XmlMembersMapping ImportMembersMapping(XmlQualifiedName[] names, Type baseType, bool baseTypeCanBeIndirect)
        {
            CodeIdentifiers identifiers = new CodeIdentifiers {
                UseCamelCasing = true
            };
            MemberMapping[] mappingArray = new MemberMapping[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                MemberMapping mapping;
                XmlQualifiedName name = names[i];
                ElementAccessor accessor = this.ImportElement(name, typeof(TypeMapping), baseType);
                if ((baseType != null) && (accessor.Mapping is StructMapping))
                {
                    base.MakeDerived((StructMapping) accessor.Mapping, baseType, baseTypeCanBeIndirect);
                }
                mappingArray[i] = new MemberMapping { Name = CodeIdentifier.MakeValid(Accessor.UnescapeName(accessor.Name)), Name = identifiers.AddUnique(mapping.Name, mapping), TypeDesc = accessor.Mapping.TypeDesc, Elements = new ElementAccessor[] { accessor } };
            }
            MembersMapping mapping2 = new MembersMapping {
                HasWrapperElement = false,
                TypeDesc = base.Scope.GetTypeDesc(typeof(object[])),
                Members = mappingArray
            };
            return new XmlMembersMapping(base.Scope, new ElementAccessor { Mapping = mapping2 }, XmlMappingAccess.Write | XmlMappingAccess.Read);
        }

        private MembersMapping ImportMembersType(XmlSchemaType type, string typeNs, string identifier)
        {
            if (!type.DerivedFrom.IsEmpty)
            {
                throw new InvalidOperationException(Res.GetString("XmlMembersDeriveError"));
            }
            CodeIdentifiers members = new CodeIdentifiers {
                UseCamelCasing = true
            };
            bool needExplicitOrder = false;
            MemberMapping[] mappingArray = this.ImportTypeMembers(type, typeNs, identifier, members, new CodeIdentifiers(), new System.Xml.Serialization.NameTable(), ref needExplicitOrder, false, false);
            return new MembersMapping { HasWrapperElement = true, TypeDesc = base.Scope.GetTypeDesc(typeof(object[])), Members = mappingArray };
        }

        private PrimitiveMapping ImportNonXsdPrimitiveDataType(XmlSchemaSimpleType dataType, string ns, TypeFlags flags)
        {
            PrimitiveMapping mapping = null;
            TypeDesc desc = null;
            if ((dataType.Name != null) && (dataType.Name.Length != 0))
            {
                desc = base.Scope.GetTypeDesc(dataType.Name, ns, flags);
                if (desc != null)
                {
                    mapping = new PrimitiveMapping {
                        TypeDesc = desc,
                        TypeName = desc.DataType.Name,
                        Namespace = mapping.TypeDesc.IsXsdType ? "http://www.w3.org/2001/XMLSchema" : ns
                    };
                }
            }
            return mapping;
        }

        private PrimitiveMapping ImportPrimitiveDataType(XmlSchemaSimpleType dataType, TypeFlags flags)
        {
            PrimitiveMapping mapping;
            TypeDesc dataTypeSource = this.GetDataTypeSource(dataType, flags);
            return new PrimitiveMapping { TypeDesc = dataTypeSource, TypeName = dataTypeSource.DataType.Name, Namespace = mapping.TypeDesc.IsXsdType ? "http://www.w3.org/2001/XMLSchema" : "http://microsoft.com/wsdl/types/" };
        }

        public XmlTypeMapping ImportSchemaType(XmlQualifiedName typeName)
        {
            return this.ImportSchemaType(typeName, null, false);
        }

        public XmlTypeMapping ImportSchemaType(XmlQualifiedName typeName, Type baseType)
        {
            return this.ImportSchemaType(typeName, baseType, false);
        }

        public XmlTypeMapping ImportSchemaType(XmlQualifiedName typeName, Type baseType, bool baseTypeCanBeIndirect)
        {
            TypeMapping mapping = this.ImportType(typeName, typeof(TypeMapping), baseType, TypeFlags.CanBeElementValue, true);
            mapping.ReferencedByElement = false;
            ElementAccessor accessor = new ElementAccessor {
                IsTopLevelInSchema = true,
                Name = typeName.Name,
                Namespace = typeName.Namespace,
                Mapping = mapping
            };
            if ((mapping is SpecialMapping) && ((SpecialMapping) mapping).NamedAny)
            {
                accessor.Any = true;
            }
            accessor.IsNullable = mapping.TypeDesc.IsNullable;
            accessor.Form = XmlSchemaForm.Qualified;
            if (accessor.Mapping is StructMapping)
            {
                base.MakeDerived((StructMapping) accessor.Mapping, baseType, baseTypeCanBeIndirect);
            }
            else if (baseType != null)
            {
                if (!(accessor.Mapping is ArrayMapping))
                {
                    throw new InvalidOperationException(Res.GetString("XmlBadBaseType", new object[] { typeName.Name, typeName.Namespace, baseType.FullName }));
                }
                accessor.Mapping = ((ArrayMapping) accessor.Mapping).TopLevelMapping;
                base.MakeDerived((StructMapping) accessor.Mapping, baseType, baseTypeCanBeIndirect);
            }
            return new XmlTypeMapping(base.Scope, accessor);
        }

        private AttributeAccessor ImportSpecialAttribute(XmlQualifiedName name, string identifier)
        {
            PrimitiveMapping mapping;
            mapping = new PrimitiveMapping {
                TypeDesc = base.Scope.GetTypeDesc(typeof(string)),
                TypeName = mapping.TypeDesc.DataType.Name
            };
            AttributeAccessor accessor = new AttributeAccessor {
                Name = name.Name,
                Namespace = "http://www.w3.org/XML/1998/namespace"
            };
            accessor.CheckSpecial();
            accessor.Mapping = mapping;
            return accessor;
        }

        private StructMapping ImportStructDataType(XmlSchemaSimpleType dataType, string typeNs, string identifier, Type baseType)
        {
            identifier = Accessor.UnescapeName(identifier);
            string name = base.GenerateUniqueTypeName(identifier);
            StructMapping typeMapping = new StructMapping {
                IsReference = base.Schemas.IsReference(dataType)
            };
            TypeFlags reference = TypeFlags.Reference;
            TypeDesc typeDesc = base.Scope.GetTypeDesc(baseType);
            typeMapping.TypeDesc = new TypeDesc(name, name, TypeKind.Struct, typeDesc, reference);
            typeMapping.Namespace = typeNs;
            typeMapping.TypeName = identifier;
            CodeIdentifiers scope = new CodeIdentifiers();
            scope.AddReserved(name);
            base.AddReservedIdentifiersForDataBinding(scope);
            this.ImportTextMember(scope, new CodeIdentifiers(), null);
            typeMapping.Members = (MemberMapping[]) scope.ToArray(typeof(MemberMapping));
            typeMapping.Scope = scope;
            base.Scope.AddTypeMapping(typeMapping);
            return typeMapping;
        }

        private StructMapping ImportStructType(XmlSchemaType type, string typeNs, string identifier, Type baseType, bool arrayLike)
        {
            TypeDesc baseTypeDesc = null;
            TypeMapping topLevelMapping = null;
            bool flag = false;
            if (!type.DerivedFrom.IsEmpty)
            {
                topLevelMapping = this.ImportType(type.DerivedFrom, typeof(TypeMapping), null, TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue, false);
                if (topLevelMapping is StructMapping)
                {
                    baseTypeDesc = ((StructMapping) topLevelMapping).TypeDesc;
                }
                else if (topLevelMapping is ArrayMapping)
                {
                    topLevelMapping = ((ArrayMapping) topLevelMapping).TopLevelMapping;
                    if (topLevelMapping != null)
                    {
                        topLevelMapping.ReferencedByTopLevelElement = false;
                        topLevelMapping.ReferencedByElement = true;
                        baseTypeDesc = topLevelMapping.TypeDesc;
                    }
                }
                else
                {
                    topLevelMapping = null;
                }
            }
            if ((baseTypeDesc == null) && (baseType != null))
            {
                baseTypeDesc = base.Scope.GetTypeDesc(baseType);
            }
            if (topLevelMapping == null)
            {
                topLevelMapping = base.GetRootMapping();
                flag = true;
            }
            Mapping mapping2 = (Mapping) base.ImportedMappings[type];
            if (mapping2 != null)
            {
                if (mapping2 is StructMapping)
                {
                    return (StructMapping) mapping2;
                }
                if (!arrayLike || !(mapping2 is ArrayMapping))
                {
                    throw new InvalidOperationException(Res.GetString("XmlTypeUsedTwice", new object[] { type.QualifiedName.Name, type.QualifiedName.Namespace }));
                }
                ArrayMapping mapping3 = (ArrayMapping) mapping2;
                if (mapping3.TopLevelMapping != null)
                {
                    return mapping3.TopLevelMapping;
                }
            }
            StructMapping mapping4 = new StructMapping {
                IsReference = base.Schemas.IsReference(type)
            };
            TypeFlags reference = TypeFlags.Reference;
            if ((type is XmlSchemaComplexType) && ((XmlSchemaComplexType) type).IsAbstract)
            {
                reference |= TypeFlags.Abstract;
            }
            identifier = Accessor.UnescapeName(identifier);
            string name = ((type.Name == null) || (type.Name.Length == 0)) ? this.GenerateUniqueTypeName(identifier, typeNs) : base.GenerateUniqueTypeName(identifier);
            mapping4.TypeDesc = new TypeDesc(name, name, TypeKind.Struct, baseTypeDesc, reference);
            mapping4.Namespace = typeNs;
            mapping4.TypeName = ((type.Name == null) || (type.Name.Length == 0)) ? null : identifier;
            mapping4.BaseMapping = (StructMapping) topLevelMapping;
            if (!arrayLike)
            {
                base.ImportedMappings.Add(type, mapping4);
            }
            CodeIdentifiers scope = new CodeIdentifiers();
            CodeIdentifiers identifiers2 = mapping4.BaseMapping.Scope.Clone();
            scope.AddReserved(name);
            identifiers2.AddReserved(name);
            base.AddReservedIdentifiersForDataBinding(scope);
            if (flag)
            {
                base.AddReservedIdentifiersForDataBinding(identifiers2);
            }
            bool needExplicitOrder = false;
            mapping4.Members = this.ImportTypeMembers(type, typeNs, identifier, scope, identifiers2, mapping4, ref needExplicitOrder, true, true);
            if (!this.IsAllGroup(type))
            {
                if (needExplicitOrder && !this.GenerateOrder)
                {
                    mapping4.SetSequence();
                }
                else if (this.GenerateOrder)
                {
                    mapping4.IsSequence = true;
                }
            }
            for (int i = 0; i < mapping4.Members.Length; i++)
            {
                StructMapping mapping5;
                MemberMapping mapping6 = ((StructMapping) topLevelMapping).FindDeclaringMapping(mapping4.Members[i], out mapping5, mapping4.TypeName);
                if ((mapping6 != null) && (mapping6.TypeDesc != mapping4.Members[i].TypeDesc))
                {
                    throw new InvalidOperationException(Res.GetString("XmlIllegalOverride", new object[] { type.Name, mapping6.Name, mapping6.TypeDesc.FullName, mapping4.Members[i].TypeDesc.FullName, mapping5.TypeDesc.FullName }));
                }
            }
            mapping4.Scope = identifiers2;
            base.Scope.AddTypeMapping(mapping4);
            return mapping4;
        }

        private bool ImportSubstitutionGroupMember(XmlSchemaElement element, string identifier, CodeIdentifiers members, CodeIdentifiers membersScope, string ns, bool repeats, ref bool needExplicitOrder, bool allowDuplicates)
        {
            XmlSchemaElement[] equivalentElements = this.GetEquivalentElements(element);
            if (equivalentElements.Length == 0)
            {
                return false;
            }
            XmlSchemaChoice group = new XmlSchemaChoice();
            for (int i = 0; i < equivalentElements.Length; i++)
            {
                group.Items.Add(equivalentElements[i]);
            }
            if (!element.IsAbstract)
            {
                group.Items.Add(element);
            }
            if (identifier.Length == 0)
            {
                identifier = CodeIdentifier.MakeValid(Accessor.UnescapeName(element.Name));
            }
            else
            {
                identifier = identifier + CodeIdentifier.MakePascal(Accessor.UnescapeName(element.Name));
            }
            this.ImportChoiceGroup(group, identifier, members, membersScope, null, ns, repeats, ref needExplicitOrder, allowDuplicates);
            return true;
        }

        private void ImportTextMember(CodeIdentifiers members, CodeIdentifiers membersScope, XmlQualifiedName simpleContentType)
        {
            TypeMapping defaultMapping;
            bool flag = false;
            if (simpleContentType != null)
            {
                defaultMapping = this.ImportType(simpleContentType, typeof(TypeMapping), null, TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue, false);
                if (!(defaultMapping is PrimitiveMapping) && !defaultMapping.TypeDesc.CanBeTextValue)
                {
                    return;
                }
            }
            else
            {
                flag = true;
                defaultMapping = this.GetDefaultMapping(TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue);
            }
            TextAccessor accessor = new TextAccessor {
                Mapping = defaultMapping
            };
            MemberMapping mapping2 = new MemberMapping {
                Elements = new ElementAccessor[0],
                Text = accessor
            };
            if (flag)
            {
                mapping2.TypeDesc = accessor.Mapping.TypeDesc.CreateArrayTypeDesc();
                mapping2.Name = members.MakeRightCase("Text");
            }
            else
            {
                PrimitiveMapping mapping = (PrimitiveMapping) accessor.Mapping;
                if (mapping.IsList)
                {
                    mapping2.TypeDesc = accessor.Mapping.TypeDesc.CreateArrayTypeDesc();
                    mapping2.Name = members.MakeRightCase("Text");
                }
                else
                {
                    mapping2.TypeDesc = accessor.Mapping.TypeDesc;
                    mapping2.Name = members.MakeRightCase("Value");
                }
            }
            mapping2.Name = membersScope.AddUnique(mapping2.Name, mapping2);
            members.Add(mapping2.Name, mapping2);
        }

        private TypeMapping ImportType(XmlQualifiedName name, Type desiredMappingType, Type baseType, TypeFlags flags, bool addref)
        {
            if ((name.Name == "anyType") && (name.Namespace == "http://www.w3.org/2001/XMLSchema"))
            {
                return base.ImportRootMapping();
            }
            object obj2 = this.FindType(name, flags);
            TypeMapping mapping = (TypeMapping) base.ImportedMappings[obj2];
            if ((mapping == null) || !desiredMappingType.IsAssignableFrom(mapping.GetType()))
            {
                if (addref)
                {
                    base.AddReference(name, base.TypesInUse, "XmlCircularTypeReference");
                }
                if (obj2 is XmlSchemaComplexType)
                {
                    mapping = this.ImportType((XmlSchemaComplexType) obj2, name.Namespace, name.Name, desiredMappingType, baseType, flags);
                }
                else
                {
                    if (!(obj2 is XmlSchemaSimpleType))
                    {
                        throw new InvalidOperationException(Res.GetString("XmlInternalError"));
                    }
                    mapping = this.ImportDataType((XmlSchemaSimpleType) obj2, name.Namespace, name.Name, baseType, flags, false);
                }
                if (addref && (name.Namespace != "http://www.w3.org/2001/XMLSchema"))
                {
                    base.RemoveReference(name, base.TypesInUse);
                }
            }
            return mapping;
        }

        private TypeMapping ImportType(XmlSchemaComplexType type, string typeNs, string identifier, Type desiredMappingType, Type baseType, TypeFlags flags)
        {
            if (type.Redefined != null)
            {
                throw new NotSupportedException(Res.GetString("XmlUnsupportedRedefine", new object[] { type.Name, typeNs }));
            }
            if (desiredMappingType == typeof(TypeMapping))
            {
                TypeMapping mapping = null;
                if ((baseType == null) && ((mapping = this.ImportArrayMapping(type, identifier, typeNs, false)) == null))
                {
                    mapping = this.ImportAnyMapping(type, identifier, typeNs, false);
                }
                if (mapping == null)
                {
                    mapping = this.ImportStructType(type, typeNs, identifier, baseType, false);
                    if (((mapping != null) && (type.Name != null)) && (type.Name.Length != 0))
                    {
                        this.ImportDerivedTypes(new XmlQualifiedName(identifier, typeNs));
                    }
                }
                return mapping;
            }
            if (desiredMappingType != typeof(MembersMapping))
            {
                throw new ArgumentException(Res.GetString("XmlInternalError"), "desiredMappingType");
            }
            return this.ImportMembersType(type, typeNs, identifier);
        }

        public XmlTypeMapping ImportTypeMapping(XmlQualifiedName name)
        {
            return this.ImportDerivedTypeMapping(name, null);
        }

        private MemberMapping[] ImportTypeMembers(XmlSchemaType type, string typeNs, string identifier, CodeIdentifiers members, CodeIdentifiers membersScope, INameScope elementsScope, ref bool needExplicitOrder, bool order, bool allowUnboundedElements)
        {
            TypeItems typeItems = this.GetTypeItems(type);
            bool mixed = IsMixed(type);
            if (mixed)
            {
                XmlSchemaType type2 = type;
                while (!type2.DerivedFrom.IsEmpty)
                {
                    type2 = this.FindType(type2.DerivedFrom, TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue);
                    if (IsMixed(type2))
                    {
                        mixed = false;
                        break;
                    }
                }
            }
            if (typeItems.Particle != null)
            {
                this.ImportGroup(typeItems.Particle, identifier, members, membersScope, elementsScope, typeNs, mixed, ref needExplicitOrder, order, typeItems.IsUnbounded, allowUnboundedElements);
            }
            for (int i = 0; i < typeItems.Attributes.Count; i++)
            {
                object obj2 = typeItems.Attributes[i];
                if (obj2 is XmlSchemaAttribute)
                {
                    this.ImportAttributeMember((XmlSchemaAttribute) obj2, identifier, members, membersScope, typeNs);
                }
                else if (obj2 is XmlSchemaAttributeGroupRef)
                {
                    XmlQualifiedName refName = ((XmlSchemaAttributeGroupRef) obj2).RefName;
                    this.ImportAttributeGroupMembers(this.FindAttributeGroup(refName), identifier, members, membersScope, refName.Namespace);
                }
            }
            if (typeItems.AnyAttribute != null)
            {
                this.ImportAnyAttributeMember(typeItems.AnyAttribute, members, membersScope);
            }
            if ((typeItems.baseSimpleType != null) || ((typeItems.Particle == null) && mixed))
            {
                this.ImportTextMember(members, membersScope, mixed ? null : typeItems.baseSimpleType);
            }
            this.ImportXmlnsDeclarationsMember(type, members, membersScope);
            return (MemberMapping[]) members.ToArray(typeof(MemberMapping));
        }

        private void ImportXmlnsDeclarationsMember(XmlSchemaType type, CodeIdentifiers members, CodeIdentifiers membersScope)
        {
            string str;
            if (this.KeepXmlnsDeclarations(type, out str))
            {
                StructMapping mapping;
                MemberMapping mapping2;
                TypeDesc typeDesc = base.Scope.GetTypeDesc(typeof(XmlSerializerNamespaces));
                mapping = new StructMapping {
                    TypeDesc = typeDesc,
                    TypeName = mapping.TypeDesc.Name,
                    Members = new MemberMapping[0],
                    IncludeInSchema = false,
                    ReferencedByTopLevelElement = true
                };
                ElementAccessor accessor = new ElementAccessor {
                    Mapping = mapping
                };
                mapping2 = new MemberMapping {
                    Elements = new ElementAccessor[] { accessor },
                    Name = CodeIdentifier.MakeValid((str == null) ? "Namespaces" : str),
                    Name = membersScope.AddUnique(mapping2.Name, mapping2)
                };
                members.Add(mapping2.Name, mapping2);
                mapping2.TypeDesc = typeDesc;
                mapping2.Xmlns = new XmlnsAccessor();
                mapping2.Ignore = true;
            }
        }

        private bool IsAllGroup(XmlSchemaType type)
        {
            TypeItems typeItems = this.GetTypeItems(type);
            return ((typeItems.Particle != null) && (typeItems.Particle is XmlSchemaAll));
        }

        private bool IsCyclicReferencedType(XmlSchemaElement element, List<string> identifiers)
        {
            if (!element.RefName.IsEmpty)
            {
                XmlSchemaElement element2 = this.FindElement(element.RefName);
                string item = CodeIdentifier.MakeValid(Accessor.UnescapeName(element2.Name));
                foreach (string str2 in identifiers)
                {
                    if (item == str2)
                    {
                        return true;
                    }
                }
                identifiers.Add(item);
                XmlSchemaType schemaType = element2.SchemaType;
                if (schemaType is XmlSchemaComplexType)
                {
                    TypeItems typeItems = this.GetTypeItems(schemaType);
                    if (((typeItems.Particle is XmlSchemaSequence) || (typeItems.Particle is XmlSchemaAll)) && ((typeItems.Particle.Items.Count == 1) && (typeItems.Particle.Items[0] is XmlSchemaElement)))
                    {
                        XmlSchemaElement element3 = (XmlSchemaElement) typeItems.Particle.Items[0];
                        if (element3.IsMultipleOccurrence)
                        {
                            return this.IsCyclicReferencedType(element3, identifiers);
                        }
                    }
                }
            }
            return false;
        }

        internal static bool IsMixed(XmlSchemaType type)
        {
            if (!(type is XmlSchemaComplexType))
            {
                return false;
            }
            XmlSchemaComplexType type2 = (XmlSchemaComplexType) type;
            bool isMixed = type2.IsMixed;
            if ((!isMixed && (type2.ContentModel != null)) && (type2.ContentModel is XmlSchemaComplexContent))
            {
                isMixed = ((XmlSchemaComplexContent) type2.ContentModel).IsMixed;
            }
            return isMixed;
        }

        private bool IsNeedXmlSerializationAttributes(ArrayMapping arrayMapping)
        {
            if (arrayMapping.Elements.Length != 1)
            {
                return true;
            }
            ElementAccessor accessor = arrayMapping.Elements[0];
            TypeMapping mapping = accessor.Mapping;
            if (accessor.Name != mapping.DefaultElementName)
            {
                return true;
            }
            if ((accessor.Form != XmlSchemaForm.None) && (accessor.Form != XmlSchemaForm.Qualified))
            {
                return true;
            }
            if (accessor.Mapping.TypeDesc != null)
            {
                if (accessor.IsNullable != accessor.Mapping.TypeDesc.IsNullable)
                {
                    return true;
                }
                if (accessor.Mapping.TypeDesc.IsAmbiguousDataType)
                {
                    return true;
                }
            }
            return false;
        }

        private bool KeepXmlnsDeclarations(XmlSchemaType type, out string xmlnsMemberName)
        {
            xmlnsMemberName = null;
            if (type.Annotation != null)
            {
                if ((type.Annotation.Items == null) || (type.Annotation.Items.Count == 0))
                {
                    return false;
                }
                foreach (XmlSchemaObject obj2 in type.Annotation.Items)
                {
                    if (obj2 is XmlSchemaAppInfo)
                    {
                        XmlNode[] markup = ((XmlSchemaAppInfo) obj2).Markup;
                        if ((markup != null) && (markup.Length > 0))
                        {
                            foreach (XmlNode node in markup)
                            {
                                if (node is XmlElement)
                                {
                                    XmlElement element = (XmlElement) node;
                                    if (element.Name == "keepNamespaceDeclarations")
                                    {
                                        if (element.LastNode is XmlText)
                                        {
                                            xmlnsMemberName = ((XmlText) element.LastNode).Value.Trim(null);
                                        }
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void RunSchemaExtensions(TypeMapping mapping, XmlQualifiedName qname, XmlSchemaType type, XmlSchemaObject context, TypeFlags flags)
        {
            string typeName = null;
            SchemaImporterExtension extension = null;
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            CodeNamespace namespace2 = new CodeNamespace();
            compileUnit.Namespaces.Add(namespace2);
            if (!qname.IsEmpty)
            {
                typeName = this.FindExtendedType(qname.Name, qname.Namespace, context, compileUnit, namespace2, out extension);
            }
            else if (type != null)
            {
                typeName = this.FindExtendedType(type, context, compileUnit, namespace2, out extension);
            }
            else if (context is XmlSchemaAny)
            {
                typeName = this.FindExtendedAnyElement((XmlSchemaAny) context, (flags & TypeFlags.CanBeTextValue) != TypeFlags.None, compileUnit, namespace2, out extension);
            }
            if ((typeName != null) && (typeName.Length > 0))
            {
                typeName = typeName.Replace('+', '.');
                try
                {
                    CodeGenerator.ValidateIdentifiers(new CodeTypeReference(typeName));
                }
                catch (ArgumentException)
                {
                    if (qname.IsEmpty)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlImporterExtensionBadLocalTypeName", new object[] { extension.GetType().FullName, typeName }));
                    }
                    throw new InvalidOperationException(Res.GetString("XmlImporterExtensionBadTypeName", new object[] { extension.GetType().FullName, qname.Name, qname.Namespace, typeName }));
                }
                foreach (CodeNamespace namespace3 in compileUnit.Namespaces)
                {
                    CodeGenerator.ValidateIdentifiers(namespace3);
                }
                mapping.TypeDesc = mapping.TypeDesc.CreateMappedTypeDesc(new MappedTypeDesc(typeName, qname.Name, qname.Namespace, type, context, extension, namespace2, compileUnit.ReferencedAssemblies));
                if (mapping is ArrayMapping)
                {
                    TypeMapping topLevelMapping = ((ArrayMapping) mapping).TopLevelMapping;
                    topLevelMapping.TypeName = mapping.TypeName;
                    topLevelMapping.TypeDesc = mapping.TypeDesc;
                }
                else
                {
                    mapping.TypeName = qname.IsEmpty ? null : typeName;
                }
            }
        }

        internal bool GenerateOrder
        {
            get
            {
                return ((base.Options & CodeGenerationOptions.GenerateOrder) != CodeGenerationOptions.None);
            }
        }

        internal class ElementComparer : IComparer
        {
            public int Compare(object o1, object o2)
            {
                ElementAccessor accessor = (ElementAccessor) o1;
                ElementAccessor accessor2 = (ElementAccessor) o2;
                return string.Compare(accessor.ToString(string.Empty), accessor2.ToString(string.Empty), StringComparison.Ordinal);
            }
        }

        private class TypeItems
        {
            internal XmlSchemaAnyAttribute AnyAttribute;
            internal XmlSchemaObjectCollection Attributes = new XmlSchemaObjectCollection();
            internal XmlQualifiedName baseSimpleType;
            internal bool IsUnbounded;
            internal XmlSchemaGroupBase Particle;
        }
    }
}

