namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Schema;

    public class XmlSchemaExporter
    {
        internal const XmlSchemaForm attributeFormDefault = XmlSchemaForm.Unqualified;
        private Hashtable attributes = new Hashtable();
        internal const XmlSchemaForm elementFormDefault = XmlSchemaForm.Qualified;
        private Hashtable elements = new Hashtable();
        private bool needToExportRoot;
        private Hashtable references = new Hashtable();
        private XmlSchemas schemas;
        private TypeScope scope;
        private Hashtable types = new Hashtable();

        public XmlSchemaExporter(XmlSchemas schemas)
        {
            this.schemas = schemas;
        }

        private XmlSchema AddSchema(string targetNamespace)
        {
            XmlSchema schema = new XmlSchema {
                TargetNamespace = string.IsNullOrEmpty(targetNamespace) ? null : targetNamespace,
                ElementFormDefault = XmlSchemaForm.Qualified,
                AttributeFormDefault = XmlSchemaForm.None
            };
            this.schemas.Add(schema);
            return schema;
        }

        private void AddSchemaImport(string ns, string referencingNs)
        {
            if ((referencingNs != null) && !NamespacesEqual(ns, referencingNs))
            {
                XmlSchema schema = this.schemas[referencingNs];
                if (schema == null)
                {
                    schema = this.AddSchema(referencingNs);
                }
                if (this.FindImport(schema, ns) == null)
                {
                    XmlSchemaImport item = new XmlSchemaImport();
                    if ((ns != null) && (ns.Length > 0))
                    {
                        item.Namespace = ns;
                    }
                    schema.Includes.Add(item);
                }
            }
        }

        private void AddSchemaItem(XmlSchemaObject item, string ns, string referencingNs)
        {
            XmlSchema schema = this.schemas[ns];
            if (schema == null)
            {
                schema = this.AddSchema(ns);
            }
            if (item is XmlSchemaElement)
            {
                XmlSchemaElement element = (XmlSchemaElement) item;
                if (element.Form == XmlSchemaForm.Unqualified)
                {
                    throw new InvalidOperationException(Res.GetString("XmlIllegalForm", new object[] { element.Name }));
                }
                element.Form = XmlSchemaForm.None;
            }
            else if (item is XmlSchemaAttribute)
            {
                XmlSchemaAttribute attribute = (XmlSchemaAttribute) item;
                if (attribute.Form == XmlSchemaForm.Unqualified)
                {
                    throw new InvalidOperationException(Res.GetString("XmlIllegalForm", new object[] { attribute.Name }));
                }
                attribute.Form = XmlSchemaForm.None;
            }
            schema.Items.Add(item);
            this.AddSchemaImport(ns, referencingNs);
        }

        private void AddXmlnsAnnotation(XmlSchemaComplexType type, string xmlnsMemberName)
        {
            XmlSchemaAnnotation annotation = new XmlSchemaAnnotation();
            XmlSchemaAppInfo item = new XmlSchemaAppInfo();
            XmlDocument document = new XmlDocument();
            XmlElement element = document.CreateElement("keepNamespaceDeclarations");
            if (xmlnsMemberName != null)
            {
                element.InsertBefore(document.CreateTextNode(xmlnsMemberName), null);
            }
            item.Markup = new XmlNode[] { element };
            annotation.Items.Add(item);
            type.Annotation = annotation;
        }

        private void CheckForDuplicateType(TypeMapping mapping, string newNamespace)
        {
            if (!mapping.IsAnonymousType)
            {
                string typeName = mapping.TypeName;
                XmlSchema schema = this.schemas[newNamespace];
                if (schema != null)
                {
                    foreach (XmlSchemaType type in schema.Items)
                    {
                        if ((type != null) && (type.Name == typeName))
                        {
                            throw new InvalidOperationException(Res.GetString("XmlDuplicateTypeName", new object[] { typeName, newNamespace }));
                        }
                    }
                }
            }
        }

        private void CheckScope(TypeScope scope)
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

        private XmlSchemaType ExportAnonymousPrimitiveMapping(PrimitiveMapping mapping)
        {
            if (!(mapping is EnumMapping))
            {
                throw new InvalidOperationException(Res.GetString("XmlInternalErrorDetails", new object[] { "Unsuported anonymous mapping type: " + mapping.ToString() }));
            }
            return this.ExportEnumMapping((EnumMapping) mapping, null);
        }

        public string ExportAnyType(string ns)
        {
            string name = "any";
            int num = 0;
            XmlSchema schema = this.schemas[ns];
            if (schema != null)
            {
                while (true)
                {
                    XmlSchemaType schemaType = FindSchemaType(name, schema.Items);
                    if (schemaType == null)
                    {
                        break;
                    }
                    if (IsAnyType(schemaType, true, true))
                    {
                        return name;
                    }
                    num++;
                    name = "any" + num.ToString(CultureInfo.InvariantCulture);
                }
            }
            XmlSchemaComplexType item = new XmlSchemaComplexType {
                Name = name,
                IsMixed = true
            };
            XmlSchemaSequence sequence = new XmlSchemaSequence();
            XmlSchemaAny any = new XmlSchemaAny {
                MinOccurs = 0M,
                MaxOccurs = 79228162514264337593543950335M
            };
            sequence.Items.Add(any);
            item.Particle = sequence;
            this.AddSchemaItem(item, ns, null);
            return name;
        }

        public string ExportAnyType(XmlMembersMapping members)
        {
            if (((members.Count != 1) || !members[0].Any) || (members[0].ElementName.Length != 0))
            {
                return null;
            }
            XmlMemberMapping mapping = members[0];
            string ns = mapping.Namespace;
            bool isArrayLike = mapping.Mapping.TypeDesc.IsArrayLike;
            bool mixed = (isArrayLike && (mapping.Mapping.TypeDesc.ArrayElementTypeDesc != null)) ? mapping.Mapping.TypeDesc.ArrayElementTypeDesc.IsMixed : mapping.Mapping.TypeDesc.IsMixed;
            if (mixed && mapping.Mapping.TypeDesc.IsMixed)
            {
                isArrayLike = true;
            }
            string str2 = mixed ? "any" : (isArrayLike ? "anyElements" : "anyElement");
            string name = str2;
            int num = 0;
            XmlSchema schema = this.schemas[ns];
            if (schema != null)
            {
                while (true)
                {
                    XmlSchemaType schemaType = FindSchemaType(name, schema.Items);
                    if (schemaType == null)
                    {
                        break;
                    }
                    if (IsAnyType(schemaType, mixed, isArrayLike))
                    {
                        return name;
                    }
                    num++;
                    name = str2 + num.ToString(CultureInfo.InvariantCulture);
                }
            }
            XmlSchemaComplexType item = new XmlSchemaComplexType {
                Name = name,
                IsMixed = mixed
            };
            XmlSchemaSequence sequence = new XmlSchemaSequence();
            XmlSchemaAny any = new XmlSchemaAny {
                MinOccurs = 0M
            };
            if (isArrayLike)
            {
                any.MaxOccurs = 79228162514264337593543950335M;
            }
            sequence.Items.Add(any);
            item.Particle = sequence;
            this.AddSchemaItem(item, ns, null);
            return name;
        }

        private void ExportArrayMapping(ArrayMapping mapping, string ns, XmlSchemaElement element)
        {
            ArrayMapping next = mapping;
            while (next.Next != null)
            {
                next = next.Next;
            }
            XmlSchemaComplexType item = (XmlSchemaComplexType) this.types[next];
            if (item == null)
            {
                this.CheckForDuplicateType(next, next.Namespace);
                item = new XmlSchemaComplexType();
                if (!mapping.IsAnonymousType)
                {
                    item.Name = mapping.TypeName;
                    this.AddSchemaItem(item, mapping.Namespace, ns);
                }
                if (!next.IsAnonymousType)
                {
                    this.types.Add(next, item);
                }
                XmlSchemaSequence group = new XmlSchemaSequence();
                this.ExportElementAccessors(group, mapping.Elements, true, false, mapping.Namespace);
                if (group.Items.Count > 0)
                {
                    if (group.Items[0] is XmlSchemaChoice)
                    {
                        item.Particle = (XmlSchemaChoice) group.Items[0];
                    }
                    else
                    {
                        item.Particle = group;
                    }
                }
            }
            else
            {
                this.AddSchemaImport(mapping.Namespace, ns);
            }
            if (element != null)
            {
                if (mapping.IsAnonymousType)
                {
                    element.SchemaType = item;
                }
                else
                {
                    element.SchemaTypeName = new XmlQualifiedName(item.Name, mapping.Namespace);
                }
            }
        }

        private void ExportAttributeAccessor(XmlSchemaComplexType type, AttributeAccessor accessor, bool valueTypeOptional, string ns)
        {
            if (accessor != null)
            {
                XmlSchemaObjectCollection attributes;
                if (type.ContentModel != null)
                {
                    if (type.ContentModel.Content is XmlSchemaComplexContentRestriction)
                    {
                        attributes = ((XmlSchemaComplexContentRestriction) type.ContentModel.Content).Attributes;
                    }
                    else if (!(type.ContentModel.Content is XmlSchemaComplexContentExtension))
                    {
                        if (!(type.ContentModel.Content is XmlSchemaSimpleContentExtension))
                        {
                            throw new InvalidOperationException(Res.GetString("XmlInvalidContent", new object[] { type.ContentModel.Content.GetType().Name }));
                        }
                        attributes = ((XmlSchemaSimpleContentExtension) type.ContentModel.Content).Attributes;
                    }
                    else
                    {
                        attributes = ((XmlSchemaComplexContentExtension) type.ContentModel.Content).Attributes;
                    }
                }
                else
                {
                    attributes = type.Attributes;
                }
                if (accessor.IsSpecialXmlNamespace)
                {
                    this.AddSchemaImport("http://www.w3.org/XML/1998/namespace", ns);
                    XmlSchemaAttribute item = new XmlSchemaAttribute {
                        Use = XmlSchemaUse.Optional,
                        RefName = new XmlQualifiedName(accessor.Name, "http://www.w3.org/XML/1998/namespace")
                    };
                    attributes.Add(item);
                }
                else if (accessor.Any)
                {
                    if (type.ContentModel == null)
                    {
                        type.AnyAttribute = new XmlSchemaAnyAttribute();
                    }
                    else
                    {
                        XmlSchemaContent content = type.ContentModel.Content;
                        if (content is XmlSchemaComplexContentExtension)
                        {
                            XmlSchemaComplexContentExtension extension = (XmlSchemaComplexContentExtension) content;
                            extension.AnyAttribute = new XmlSchemaAnyAttribute();
                        }
                        else if (content is XmlSchemaComplexContentRestriction)
                        {
                            XmlSchemaComplexContentRestriction restriction = (XmlSchemaComplexContentRestriction) content;
                            restriction.AnyAttribute = new XmlSchemaAnyAttribute();
                        }
                        else if (type.ContentModel.Content is XmlSchemaSimpleContentExtension)
                        {
                            XmlSchemaSimpleContentExtension extension2 = (XmlSchemaSimpleContentExtension) content;
                            extension2.AnyAttribute = new XmlSchemaAnyAttribute();
                        }
                    }
                }
                else
                {
                    XmlSchemaAttribute attribute2 = new XmlSchemaAttribute {
                        Use = XmlSchemaUse.None
                    };
                    if ((!accessor.HasDefault && !valueTypeOptional) && accessor.Mapping.TypeDesc.IsValueType)
                    {
                        attribute2.Use = XmlSchemaUse.Required;
                    }
                    attribute2.Name = accessor.Name;
                    if ((accessor.Namespace == null) || (accessor.Namespace == ns))
                    {
                        XmlSchema schema = this.schemas[ns];
                        if (schema == null)
                        {
                            attribute2.Form = (accessor.Form == XmlSchemaForm.Unqualified) ? XmlSchemaForm.None : accessor.Form;
                        }
                        else
                        {
                            attribute2.Form = (accessor.Form == schema.AttributeFormDefault) ? XmlSchemaForm.None : accessor.Form;
                        }
                        attributes.Add(attribute2);
                    }
                    else
                    {
                        if (this.attributes[accessor] == null)
                        {
                            attribute2.Use = XmlSchemaUse.None;
                            attribute2.Form = accessor.Form;
                            this.AddSchemaItem(attribute2, accessor.Namespace, ns);
                            this.attributes.Add(accessor, accessor);
                        }
                        XmlSchemaAttribute attribute3 = new XmlSchemaAttribute {
                            Use = XmlSchemaUse.None,
                            RefName = new XmlQualifiedName(accessor.Name, accessor.Namespace)
                        };
                        attributes.Add(attribute3);
                        this.AddSchemaImport(accessor.Namespace, ns);
                    }
                    if (accessor.Mapping is PrimitiveMapping)
                    {
                        PrimitiveMapping mapping = (PrimitiveMapping) accessor.Mapping;
                        if (mapping.IsList)
                        {
                            XmlSchemaSimpleType type2 = new XmlSchemaSimpleType();
                            XmlSchemaSimpleTypeList list = new XmlSchemaSimpleTypeList();
                            if (mapping.IsAnonymousType)
                            {
                                list.ItemType = (XmlSchemaSimpleType) this.ExportAnonymousPrimitiveMapping(mapping);
                            }
                            else
                            {
                                list.ItemTypeName = this.ExportPrimitiveMapping(mapping, (accessor.Namespace == null) ? ns : accessor.Namespace);
                            }
                            type2.Content = list;
                            attribute2.SchemaType = type2;
                        }
                        else if (mapping.IsAnonymousType)
                        {
                            attribute2.SchemaType = (XmlSchemaSimpleType) this.ExportAnonymousPrimitiveMapping(mapping);
                        }
                        else
                        {
                            attribute2.SchemaTypeName = this.ExportPrimitiveMapping(mapping, (accessor.Namespace == null) ? ns : accessor.Namespace);
                        }
                    }
                    else if (!(accessor.Mapping is SpecialMapping))
                    {
                        throw new InvalidOperationException(Res.GetString("XmlInternalError"));
                    }
                    if (accessor.HasDefault)
                    {
                        attribute2.DefaultValue = ExportDefaultValue(accessor.Mapping, accessor.Default);
                    }
                }
            }
        }

        internal static string ExportDefaultValue(TypeMapping mapping, object value)
        {
            if (!(mapping is PrimitiveMapping))
            {
                return null;
            }
            if ((value == null) || (value == DBNull.Value))
            {
                return null;
            }
            if (mapping is EnumMapping)
            {
                EnumMapping mapping2 = (EnumMapping) mapping;
                ConstantMapping[] constants = mapping2.Constants;
                if (mapping2.IsFlags)
                {
                    string[] vals = new string[constants.Length];
                    long[] ids = new long[constants.Length];
                    Hashtable hashtable = new Hashtable();
                    for (int j = 0; j < constants.Length; j++)
                    {
                        vals[j] = constants[j].XmlName;
                        ids[j] = ((int) 1) << j;
                        hashtable.Add(constants[j].Name, ids[j]);
                    }
                    long val = XmlCustomFormatter.ToEnum((string) value, hashtable, mapping2.TypeName, false);
                    if (val == 0L)
                    {
                        return null;
                    }
                    return XmlCustomFormatter.FromEnum(val, vals, ids, mapping.TypeDesc.FullName);
                }
                for (int i = 0; i < constants.Length; i++)
                {
                    if (constants[i].Name == ((string) value))
                    {
                        return constants[i].XmlName;
                    }
                }
                return null;
            }
            PrimitiveMapping mapping3 = (PrimitiveMapping) mapping;
            if (!mapping3.TypeDesc.HasCustomFormatter)
            {
                if (mapping3.TypeDesc.FormatterName == "String")
                {
                    return (string) value;
                }
                Type type = typeof(XmlConvert);
                MethodInfo method = type.GetMethod("ToString", new Type[] { mapping3.TypeDesc.Type });
                if (method != null)
                {
                    return (string) method.Invoke(type, new object[] { value });
                }
            }
            else
            {
                string str = XmlCustomFormatter.FromDefaultValue(value, mapping3.TypeDesc.FormatterName);
                if (str == null)
                {
                    throw new InvalidOperationException(Res.GetString("XmlInvalidDefaultValue", new object[] { value.ToString(), mapping3.TypeDesc.Name }));
                }
                return str;
            }
            throw new InvalidOperationException(Res.GetString("XmlInvalidDefaultValue", new object[] { value.ToString(), mapping3.TypeDesc.Name }));
        }

        private void ExportDerivedMappings(StructMapping mapping)
        {
            if (!mapping.IsAnonymousType)
            {
                for (StructMapping mapping2 = mapping.DerivedMappings; mapping2 != null; mapping2 = mapping2.NextDerivedMapping)
                {
                    if (mapping2.IncludeInSchema)
                    {
                        this.ExportStructMapping(mapping2, mapping2.Namespace, null);
                    }
                }
            }
        }

        private XmlSchemaElement ExportElement(ElementAccessor accessor)
        {
            if (!accessor.Mapping.IncludeInSchema && !accessor.Mapping.TypeDesc.IsRoot)
            {
                return null;
            }
            if (accessor.Any && (accessor.Name.Length == 0))
            {
                throw new InvalidOperationException(Res.GetString("XmlIllegalWildcard"));
            }
            XmlSchemaElement element = (XmlSchemaElement) this.elements[accessor];
            if (element == null)
            {
                element = new XmlSchemaElement {
                    Name = accessor.Name,
                    IsNillable = accessor.IsNullable
                };
                this.elements.Add(accessor, element);
                element.Form = accessor.Form;
                this.AddSchemaItem(element, accessor.Namespace, null);
                this.ExportElementMapping(element, accessor.Mapping, accessor.Namespace, accessor.Any);
            }
            return element;
        }

        private void ExportElementAccessor(XmlSchemaGroupBase group, ElementAccessor accessor, bool repeats, bool valueTypeOptional, string ns)
        {
            if (accessor.Any && (accessor.Name.Length == 0))
            {
                XmlSchemaAny item = new XmlSchemaAny {
                    MinOccurs = 0M,
                    MaxOccurs = repeats ? 79228162514264337593543950335M : 1M
                };
                if (((accessor.Namespace != null) && (accessor.Namespace.Length > 0)) && (accessor.Namespace != ns))
                {
                    item.Namespace = accessor.Namespace;
                }
                group.Items.Add(item);
            }
            else
            {
                XmlSchemaElement element = (XmlSchemaElement) this.elements[accessor];
                int num = (((repeats || accessor.HasDefault) || (!accessor.IsNullable && !accessor.Mapping.TypeDesc.IsValueType)) || valueTypeOptional) ? 0 : 1;
                decimal num2 = (repeats || accessor.IsUnbounded) ? 79228162514264337593543950335M : 1M;
                if (element == null)
                {
                    element = new XmlSchemaElement {
                        IsNillable = accessor.IsNullable,
                        Name = accessor.Name
                    };
                    if (accessor.HasDefault)
                    {
                        element.DefaultValue = ExportDefaultValue(accessor.Mapping, accessor.Default);
                    }
                    if (accessor.IsTopLevelInSchema)
                    {
                        this.elements.Add(accessor, element);
                        element.Form = accessor.Form;
                        this.AddSchemaItem(element, accessor.Namespace, ns);
                    }
                    else
                    {
                        element.MinOccurs = num;
                        element.MaxOccurs = num2;
                        XmlSchema schema = this.schemas[ns];
                        if (schema == null)
                        {
                            element.Form = (accessor.Form == XmlSchemaForm.Qualified) ? XmlSchemaForm.None : accessor.Form;
                        }
                        else
                        {
                            element.Form = (accessor.Form == schema.ElementFormDefault) ? XmlSchemaForm.None : accessor.Form;
                        }
                    }
                    this.ExportElementMapping(element, accessor.Mapping, accessor.Namespace, accessor.Any);
                }
                if (accessor.IsTopLevelInSchema)
                {
                    XmlSchemaElement element2 = new XmlSchemaElement {
                        RefName = new XmlQualifiedName(accessor.Name, accessor.Namespace),
                        MinOccurs = num,
                        MaxOccurs = num2
                    };
                    group.Items.Add(element2);
                    this.AddSchemaImport(accessor.Namespace, ns);
                }
                else
                {
                    group.Items.Add(element);
                }
            }
        }

        private void ExportElementAccessors(XmlSchemaGroupBase group, ElementAccessor[] accessors, bool repeats, bool valueTypeOptional, string ns)
        {
            if (accessors.Length != 0)
            {
                if (accessors.Length == 1)
                {
                    this.ExportElementAccessor(group, accessors[0], repeats, valueTypeOptional, ns);
                }
                else
                {
                    XmlSchemaChoice choice = new XmlSchemaChoice {
                        MaxOccurs = repeats ? 79228162514264337593543950335M : 1M,
                        MinOccurs = repeats ? 0 : 1
                    };
                    for (int i = 0; i < accessors.Length; i++)
                    {
                        this.ExportElementAccessor(choice, accessors[i], false, valueTypeOptional, ns);
                    }
                    if (choice.Items.Count > 0)
                    {
                        group.Items.Add(choice);
                    }
                }
            }
        }

        private void ExportElementMapping(XmlSchemaElement element, Mapping mapping, string ns, bool isAny)
        {
            if (mapping is ArrayMapping)
            {
                this.ExportArrayMapping((ArrayMapping) mapping, ns, element);
            }
            else if (mapping is PrimitiveMapping)
            {
                PrimitiveMapping mapping2 = (PrimitiveMapping) mapping;
                if (mapping2.IsAnonymousType)
                {
                    element.SchemaType = this.ExportAnonymousPrimitiveMapping(mapping2);
                }
                else
                {
                    element.SchemaTypeName = this.ExportPrimitiveMapping(mapping2, ns);
                }
            }
            else if (mapping is StructMapping)
            {
                this.ExportStructMapping((StructMapping) mapping, ns, element);
            }
            else if (mapping is MembersMapping)
            {
                element.SchemaType = this.ExportMembersMapping((MembersMapping) mapping, ns);
            }
            else if (mapping is SpecialMapping)
            {
                this.ExportSpecialMapping((SpecialMapping) mapping, ns, isAny, element);
            }
            else
            {
                if (!(mapping is NullableMapping))
                {
                    throw new ArgumentException(Res.GetString("XmlInternalError"), "mapping");
                }
                this.ExportElementMapping(element, ((NullableMapping) mapping).BaseMapping, ns, isAny);
            }
        }

        private XmlSchemaType ExportEnumMapping(EnumMapping mapping, string ns)
        {
            if (!mapping.IncludeInSchema)
            {
                throw new InvalidOperationException(Res.GetString("XmlCannotIncludeInSchema", new object[] { mapping.TypeDesc.Name }));
            }
            XmlSchemaSimpleType type = (XmlSchemaSimpleType) this.types[mapping];
            if (type == null)
            {
                this.CheckForDuplicateType(mapping, mapping.Namespace);
                type = new XmlSchemaSimpleType {
                    Name = mapping.TypeName
                };
                if (!mapping.IsAnonymousType)
                {
                    this.types.Add(mapping, type);
                    this.AddSchemaItem(type, mapping.Namespace, ns);
                }
                XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction {
                    BaseTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema")
                };
                for (int i = 0; i < mapping.Constants.Length; i++)
                {
                    ConstantMapping mapping2 = mapping.Constants[i];
                    XmlSchemaEnumerationFacet item = new XmlSchemaEnumerationFacet {
                        Value = mapping2.XmlName
                    };
                    restriction.Facets.Add(item);
                }
                if (!mapping.IsFlags)
                {
                    type.Content = restriction;
                }
                else
                {
                    XmlSchemaSimpleTypeList list = new XmlSchemaSimpleTypeList();
                    XmlSchemaSimpleType type2 = new XmlSchemaSimpleType {
                        Content = restriction
                    };
                    list.ItemType = type2;
                    type.Content = list;
                }
            }
            if (!mapping.IsAnonymousType)
            {
                this.AddSchemaImport(mapping.Namespace, ns);
            }
            return type;
        }

        private void ExportMapping(Mapping mapping, string ns, bool isAny)
        {
            if (mapping is ArrayMapping)
            {
                this.ExportArrayMapping((ArrayMapping) mapping, ns, null);
            }
            else if (mapping is PrimitiveMapping)
            {
                this.ExportPrimitiveMapping((PrimitiveMapping) mapping, ns);
            }
            else if (mapping is StructMapping)
            {
                this.ExportStructMapping((StructMapping) mapping, ns, null);
            }
            else if (mapping is MembersMapping)
            {
                this.ExportMembersMapping((MembersMapping) mapping, ns);
            }
            else if (mapping is SpecialMapping)
            {
                this.ExportSpecialMapping((SpecialMapping) mapping, ns, isAny, null);
            }
            else
            {
                if (!(mapping is NullableMapping))
                {
                    throw new ArgumentException(Res.GetString("XmlInternalError"), "mapping");
                }
                this.ExportMapping(((NullableMapping) mapping).BaseMapping, ns, isAny);
            }
        }

        public void ExportMembersMapping(XmlMembersMapping xmlMembersMapping)
        {
            this.ExportMembersMapping(xmlMembersMapping, true);
        }

        private XmlSchemaType ExportMembersMapping(MembersMapping mapping, string ns)
        {
            XmlSchemaComplexType type = new XmlSchemaComplexType();
            this.ExportTypeMembers(type, mapping.Members, mapping.TypeName, ns, false, false);
            if (mapping.XmlnsMember != null)
            {
                this.AddXmlnsAnnotation(type, mapping.XmlnsMember.Name);
            }
            return type;
        }

        public void ExportMembersMapping(XmlMembersMapping xmlMembersMapping, bool exportEnclosingType)
        {
            xmlMembersMapping.CheckShallow();
            MembersMapping mapping = (MembersMapping) xmlMembersMapping.Accessor.Mapping;
            this.CheckScope(xmlMembersMapping.Scope);
            if (mapping.HasWrapperElement && exportEnclosingType)
            {
                this.ExportElement(xmlMembersMapping.Accessor);
            }
            else
            {
                foreach (MemberMapping mapping2 in mapping.Members)
                {
                    if (mapping2.Attribute != null)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlBareAttributeMember", new object[] { mapping2.Attribute.Name }));
                    }
                    if (mapping2.Text != null)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlBareTextMember", new object[] { mapping2.Text.Name }));
                    }
                    if ((mapping2.Elements != null) && (mapping2.Elements.Length != 0))
                    {
                        if (mapping2.TypeDesc.IsArrayLike && !(mapping2.Elements[0].Mapping is ArrayMapping))
                        {
                            throw new InvalidOperationException(Res.GetString("XmlIllegalArrayElement", new object[] { mapping2.Elements[0].Name }));
                        }
                        if (exportEnclosingType)
                        {
                            this.ExportElement(mapping2.Elements[0]);
                        }
                        else
                        {
                            this.ExportMapping(mapping2.Elements[0].Mapping, mapping2.Elements[0].Namespace, mapping2.Elements[0].Any);
                        }
                    }
                }
            }
            this.ExportRootIfNecessary(xmlMembersMapping.Scope);
        }

        private XmlQualifiedName ExportNonXsdPrimitiveMapping(PrimitiveMapping mapping, string ns)
        {
            XmlSchemaSimpleType dataType = (XmlSchemaSimpleType) mapping.TypeDesc.DataType;
            if (!this.SchemaContainsItem(dataType, "http://microsoft.com/wsdl/types/"))
            {
                this.AddSchemaItem(dataType, "http://microsoft.com/wsdl/types/", ns);
            }
            else
            {
                this.AddSchemaImport(mapping.Namespace, ns);
            }
            return new XmlQualifiedName(mapping.TypeDesc.DataType.Name, "http://microsoft.com/wsdl/types/");
        }

        private XmlQualifiedName ExportPrimitiveMapping(PrimitiveMapping mapping, string ns)
        {
            if (mapping is EnumMapping)
            {
                return new XmlQualifiedName(this.ExportEnumMapping((EnumMapping) mapping, ns).Name, mapping.Namespace);
            }
            if (mapping.TypeDesc.IsXsdType)
            {
                return new XmlQualifiedName(mapping.TypeDesc.DataType.Name, "http://www.w3.org/2001/XMLSchema");
            }
            return this.ExportNonXsdPrimitiveMapping(mapping, ns);
        }

        private void ExportRootIfNecessary(TypeScope typeScope)
        {
            if (this.needToExportRoot)
            {
                foreach (TypeMapping mapping in typeScope.TypeMappings)
                {
                    if ((mapping is StructMapping) && mapping.TypeDesc.IsRoot)
                    {
                        this.ExportDerivedMappings((StructMapping) mapping);
                    }
                    else if (mapping is ArrayMapping)
                    {
                        this.ExportArrayMapping((ArrayMapping) mapping, mapping.Namespace, null);
                    }
                    else if (mapping is SerializableMapping)
                    {
                        this.ExportSpecialMapping((SerializableMapping) mapping, mapping.Namespace, false, null);
                    }
                }
            }
        }

        private XmlSchemaType ExportSpecialMapping(SpecialMapping mapping, string ns, bool isAny, XmlSchemaElement element)
        {
            switch (mapping.TypeDesc.Kind)
            {
                case TypeKind.Node:
                {
                    XmlSchemaComplexType type = new XmlSchemaComplexType {
                        IsMixed = mapping.TypeDesc.IsMixed
                    };
                    XmlSchemaSequence sequence = new XmlSchemaSequence();
                    XmlSchemaAny item = new XmlSchemaAny();
                    if (isAny)
                    {
                        type.AnyAttribute = new XmlSchemaAnyAttribute();
                        type.IsMixed = true;
                        item.MaxOccurs = 79228162514264337593543950335M;
                    }
                    sequence.Items.Add(item);
                    type.Particle = sequence;
                    if (element != null)
                    {
                        element.SchemaType = type;
                    }
                    return type;
                }
                case TypeKind.Serializable:
                {
                    SerializableMapping mapping2 = (SerializableMapping) mapping;
                    if (!mapping2.IsAny)
                    {
                        if ((mapping2.XsiType != null) || (mapping2.XsdType != null))
                        {
                            XmlSchemaType xsdType = mapping2.XsdType;
                            foreach (XmlSchema schema2 in mapping2.Schemas.Schemas())
                            {
                                if (schema2.TargetNamespace != "http://www.w3.org/2001/XMLSchema")
                                {
                                    this.schemas.Add(schema2, true);
                                    this.AddSchemaImport(schema2.TargetNamespace, ns);
                                    if (!mapping2.XsiType.IsEmpty && (mapping2.XsiType.Namespace == schema2.TargetNamespace))
                                    {
                                        xsdType = (XmlSchemaType) schema2.SchemaTypes[mapping2.XsiType];
                                    }
                                }
                            }
                            if (element != null)
                            {
                                element.SchemaTypeName = mapping2.XsiType;
                                if (element.SchemaTypeName.IsEmpty)
                                {
                                    element.SchemaType = xsdType;
                                }
                            }
                            mapping2.CheckDuplicateElement(element, ns);
                            return xsdType;
                        }
                        if (mapping2.Schema != null)
                        {
                            XmlSchemaComplexType type4 = new XmlSchemaComplexType();
                            XmlSchemaAny any3 = new XmlSchemaAny();
                            XmlSchemaSequence sequence3 = new XmlSchemaSequence();
                            sequence3.Items.Add(any3);
                            type4.Particle = sequence3;
                            string targetNamespace = mapping2.Schema.TargetNamespace;
                            any3.Namespace = (targetNamespace == null) ? "" : targetNamespace;
                            XmlSchema schema3 = this.schemas[targetNamespace];
                            if (schema3 == null)
                            {
                                this.schemas.Add(mapping2.Schema);
                            }
                            else if (schema3 != mapping2.Schema)
                            {
                                throw new InvalidOperationException(Res.GetString("XmlDuplicateNamespace", new object[] { targetNamespace }));
                            }
                            if (element != null)
                            {
                                element.SchemaType = type4;
                            }
                            mapping2.CheckDuplicateElement(element, ns);
                            return type4;
                        }
                        XmlSchemaComplexType type5 = new XmlSchemaComplexType();
                        XmlSchemaElement element2 = new XmlSchemaElement {
                            RefName = new XmlQualifiedName("schema", "http://www.w3.org/2001/XMLSchema")
                        };
                        XmlSchemaSequence sequence4 = new XmlSchemaSequence();
                        sequence4.Items.Add(element2);
                        sequence4.Items.Add(new XmlSchemaAny());
                        type5.Particle = sequence4;
                        this.AddSchemaImport("http://www.w3.org/2001/XMLSchema", ns);
                        if (element != null)
                        {
                            element.SchemaType = type5;
                        }
                        return type5;
                    }
                    XmlSchemaComplexType type2 = new XmlSchemaComplexType {
                        IsMixed = mapping.TypeDesc.IsMixed
                    };
                    XmlSchemaSequence sequence2 = new XmlSchemaSequence();
                    XmlSchemaAny any2 = new XmlSchemaAny();
                    if (isAny)
                    {
                        type2.AnyAttribute = new XmlSchemaAnyAttribute();
                        type2.IsMixed = true;
                        any2.MaxOccurs = 79228162514264337593543950335M;
                    }
                    if (mapping2.NamespaceList.Length > 0)
                    {
                        any2.Namespace = mapping2.NamespaceList;
                    }
                    any2.ProcessContents = XmlSchemaContentProcessing.Lax;
                    if (mapping2.Schemas != null)
                    {
                        foreach (XmlSchema schema in mapping2.Schemas.Schemas())
                        {
                            if (schema.TargetNamespace != "http://www.w3.org/2001/XMLSchema")
                            {
                                this.schemas.Add(schema, true);
                                this.AddSchemaImport(schema.TargetNamespace, ns);
                            }
                        }
                    }
                    sequence2.Items.Add(any2);
                    type2.Particle = sequence2;
                    if (element != null)
                    {
                        element.SchemaType = type2;
                    }
                    return type2;
                }
            }
            throw new ArgumentException(Res.GetString("XmlInternalError"), "mapping");
        }

        private XmlQualifiedName ExportStructMapping(StructMapping mapping, string ns, XmlSchemaElement element)
        {
            if (mapping.TypeDesc.IsRoot)
            {
                this.needToExportRoot = true;
                return XmlQualifiedName.Empty;
            }
            if (mapping.IsAnonymousType)
            {
                if (this.references[mapping] != null)
                {
                    throw new InvalidOperationException(Res.GetString("XmlCircularReference2", new object[] { mapping.TypeDesc.Name, "AnonymousType", "false" }));
                }
                this.references[mapping] = mapping;
            }
            XmlSchemaComplexType item = (XmlSchemaComplexType) this.types[mapping];
            if (item == null)
            {
                if (!mapping.IncludeInSchema)
                {
                    throw new InvalidOperationException(Res.GetString("XmlCannotIncludeInSchema", new object[] { mapping.TypeDesc.Name }));
                }
                this.CheckForDuplicateType(mapping, mapping.Namespace);
                item = new XmlSchemaComplexType();
                if (!mapping.IsAnonymousType)
                {
                    item.Name = mapping.TypeName;
                    this.AddSchemaItem(item, mapping.Namespace, ns);
                    this.types.Add(mapping, item);
                }
                item.IsAbstract = mapping.TypeDesc.IsAbstract;
                bool isOpenModel = mapping.IsOpenModel;
                if ((mapping.BaseMapping != null) && mapping.BaseMapping.IncludeInSchema)
                {
                    if (mapping.BaseMapping.IsAnonymousType)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlAnonymousBaseType", new object[] { mapping.TypeDesc.Name, mapping.BaseMapping.TypeDesc.Name, "AnonymousType", "false" }));
                    }
                    if (mapping.HasSimpleContent)
                    {
                        XmlSchemaSimpleContent content = new XmlSchemaSimpleContent();
                        XmlSchemaSimpleContentExtension extension = new XmlSchemaSimpleContentExtension {
                            BaseTypeName = this.ExportStructMapping(mapping.BaseMapping, mapping.Namespace, null)
                        };
                        content.Content = extension;
                        item.ContentModel = content;
                    }
                    else
                    {
                        XmlSchemaComplexContentExtension extension2 = new XmlSchemaComplexContentExtension {
                            BaseTypeName = this.ExportStructMapping(mapping.BaseMapping, mapping.Namespace, null)
                        };
                        XmlSchemaComplexContent content2 = new XmlSchemaComplexContent {
                            Content = extension2,
                            IsMixed = XmlSchemaImporter.IsMixed((XmlSchemaComplexType) this.types[mapping.BaseMapping])
                        };
                        item.ContentModel = content2;
                    }
                    isOpenModel = false;
                }
                this.ExportTypeMembers(item, mapping.Members, mapping.TypeName, mapping.Namespace, mapping.HasSimpleContent, isOpenModel);
                this.ExportDerivedMappings(mapping);
                if (mapping.XmlnsMember != null)
                {
                    this.AddXmlnsAnnotation(item, mapping.XmlnsMember.Name);
                }
            }
            else
            {
                this.AddSchemaImport(mapping.Namespace, ns);
            }
            if (mapping.IsAnonymousType)
            {
                this.references[mapping] = null;
                if (element != null)
                {
                    element.SchemaType = item;
                }
                return XmlQualifiedName.Empty;
            }
            XmlQualifiedName name = new XmlQualifiedName(item.Name, mapping.Namespace);
            if (element != null)
            {
                element.SchemaTypeName = name;
            }
            return name;
        }

        public XmlQualifiedName ExportTypeMapping(XmlMembersMapping xmlMembersMapping)
        {
            xmlMembersMapping.CheckShallow();
            this.CheckScope(xmlMembersMapping.Scope);
            MembersMapping mapping = (MembersMapping) xmlMembersMapping.Accessor.Mapping;
            if ((mapping.Members.Length != 1) || !(mapping.Members[0].Elements[0].Mapping is SpecialMapping))
            {
                return null;
            }
            SpecialMapping mapping2 = (SpecialMapping) mapping.Members[0].Elements[0].Mapping;
            XmlSchemaType item = this.ExportSpecialMapping(mapping2, xmlMembersMapping.Accessor.Namespace, false, null);
            if (((item != null) && (item.Name != null)) && (item.Name.Length > 0))
            {
                item.Name = xmlMembersMapping.Accessor.Name;
                this.AddSchemaItem(item, xmlMembersMapping.Accessor.Namespace, null);
            }
            this.ExportRootIfNecessary(xmlMembersMapping.Scope);
            return new XmlQualifiedName(xmlMembersMapping.Accessor.Name, xmlMembersMapping.Accessor.Namespace);
        }

        public void ExportTypeMapping(XmlTypeMapping xmlTypeMapping)
        {
            xmlTypeMapping.CheckShallow();
            this.CheckScope(xmlTypeMapping.Scope);
            this.ExportElement(xmlTypeMapping.Accessor);
            this.ExportRootIfNecessary(xmlTypeMapping.Scope);
        }

        private void ExportTypeMembers(XmlSchemaComplexType type, MemberMapping[] members, string name, string ns, bool hasSimpleContent, bool openModel)
        {
            XmlSchemaGroupBase group = new XmlSchemaSequence();
            TypeMapping mapping = null;
            for (int i = 0; i < members.Length; i++)
            {
                MemberMapping mapping2 = members[i];
                if (!mapping2.Ignore)
                {
                    if (mapping2.Text != null)
                    {
                        if (mapping != null)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlIllegalMultipleText", new object[] { name }));
                        }
                        mapping = mapping2.Text.Mapping;
                    }
                    if (mapping2.Elements.Length > 0)
                    {
                        bool repeats = mapping2.TypeDesc.IsArrayLike && ((mapping2.Elements.Length != 1) || !(mapping2.Elements[0].Mapping is ArrayMapping));
                        bool valueTypeOptional = (mapping2.CheckSpecified != SpecifiedAccessor.None) || mapping2.CheckShouldPersist;
                        this.ExportElementAccessors(group, mapping2.Elements, repeats, valueTypeOptional, ns);
                    }
                }
            }
            if (group.Items.Count > 0)
            {
                if (type.ContentModel != null)
                {
                    if (type.ContentModel.Content is XmlSchemaComplexContentRestriction)
                    {
                        ((XmlSchemaComplexContentRestriction) type.ContentModel.Content).Particle = group;
                    }
                    else
                    {
                        if (!(type.ContentModel.Content is XmlSchemaComplexContentExtension))
                        {
                            throw new InvalidOperationException(Res.GetString("XmlInvalidContent", new object[] { type.ContentModel.Content.GetType().Name }));
                        }
                        ((XmlSchemaComplexContentExtension) type.ContentModel.Content).Particle = group;
                    }
                }
                else
                {
                    type.Particle = group;
                }
            }
            if (mapping != null)
            {
                if (hasSimpleContent)
                {
                    if ((mapping is PrimitiveMapping) && (group.Items.Count == 0))
                    {
                        PrimitiveMapping mapping3 = (PrimitiveMapping) mapping;
                        if (mapping3.IsList)
                        {
                            type.IsMixed = true;
                        }
                        else
                        {
                            if (mapping3.IsAnonymousType)
                            {
                                throw new InvalidOperationException(Res.GetString("XmlAnonymousBaseType", new object[] { mapping.TypeDesc.Name, mapping3.TypeDesc.Name, "AnonymousType", "false" }));
                            }
                            XmlSchemaSimpleContent content = new XmlSchemaSimpleContent();
                            XmlSchemaSimpleContentExtension extension = new XmlSchemaSimpleContentExtension();
                            content.Content = extension;
                            type.ContentModel = content;
                            extension.BaseTypeName = this.ExportPrimitiveMapping(mapping3, ns);
                        }
                    }
                }
                else
                {
                    type.IsMixed = true;
                }
            }
            bool flag3 = false;
            for (int j = 0; j < members.Length; j++)
            {
                if (members[j].Attribute != null)
                {
                    this.ExportAttributeAccessor(type, members[j].Attribute, (members[j].CheckSpecified != SpecifiedAccessor.None) || members[j].CheckShouldPersist, ns);
                    if (members[j].Attribute.Any)
                    {
                        flag3 = true;
                    }
                }
            }
            if (openModel && !flag3)
            {
                AttributeAccessor accessor = new AttributeAccessor {
                    Any = true
                };
                this.ExportAttributeAccessor(type, accessor, false, ns);
            }
        }

        private XmlSchemaImport FindImport(XmlSchema schema, string ns)
        {
            foreach (object obj2 in schema.Includes)
            {
                if (obj2 is XmlSchemaImport)
                {
                    XmlSchemaImport import = (XmlSchemaImport) obj2;
                    if (NamespacesEqual(import.Namespace, ns))
                    {
                        return import;
                    }
                }
            }
            return null;
        }

        private static XmlSchemaType FindSchemaType(string name, XmlSchemaObjectCollection items)
        {
            foreach (XmlSchemaType type in items)
            {
                if ((type != null) && (type.Name == name))
                {
                    return type;
                }
            }
            return null;
        }

        private static bool IsAnyType(XmlSchemaType schemaType, bool mixed, bool unbounded)
        {
            XmlSchemaComplexType type = schemaType as XmlSchemaComplexType;
            if (type != null)
            {
                if (type.IsMixed != mixed)
                {
                    return false;
                }
                if (type.Particle is XmlSchemaSequence)
                {
                    XmlSchemaSequence particle = (XmlSchemaSequence) type.Particle;
                    if ((particle.Items.Count == 1) && (particle.Items[0] is XmlSchemaAny))
                    {
                        XmlSchemaAny any = (XmlSchemaAny) particle.Items[0];
                        return (unbounded == any.IsMultipleOccurrence);
                    }
                }
            }
            return false;
        }

        private static bool NamespacesEqual(string ns1, string ns2)
        {
            if ((ns1 != null) && (ns1.Length != 0))
            {
                return (ns1 == ns2);
            }
            if (ns2 != null)
            {
                return (ns2.Length == 0);
            }
            return true;
        }

        private bool SchemaContainsItem(XmlSchemaObject item, string ns)
        {
            XmlSchema schema = this.schemas[ns];
            return ((schema != null) && schema.Items.Contains(item));
        }
    }
}

