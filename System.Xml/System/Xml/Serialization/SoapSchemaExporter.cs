namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Xml;
    using System.Xml.Schema;

    public class SoapSchemaExporter
    {
        private static XmlQualifiedName ArrayQName = new XmlQualifiedName("Array", "http://schemas.xmlsoap.org/soap/encoding/");
        private static XmlQualifiedName ArrayTypeQName = new XmlQualifiedName("arrayType", "http://schemas.xmlsoap.org/soap/encoding/");
        private XmlDocument document;
        internal const XmlSchemaForm elementFormDefault = XmlSchemaForm.Qualified;
        private bool exportedRoot;
        private XmlSchemas schemas;
        private TypeScope scope;
        private Hashtable types = new Hashtable();

        public SoapSchemaExporter(XmlSchemas schemas)
        {
            this.schemas = schemas;
        }

        private void AddSchemaImport(string ns, string referencingNs)
        {
            if (((referencingNs != null) && (ns != null)) && (ns != referencingNs))
            {
                XmlSchema schema = this.schemas[referencingNs];
                if (schema == null)
                {
                    throw new InvalidOperationException(Res.GetString("XmlMissingSchema", new object[] { referencingNs }));
                }
                if (((ns != null) && (ns.Length > 0)) && (this.FindImport(schema, ns) == null))
                {
                    XmlSchemaImport item = new XmlSchemaImport {
                        Namespace = ns
                    };
                    schema.Includes.Add(item);
                }
            }
        }

        private void AddSchemaItem(XmlSchemaObject item, string ns, string referencingNs)
        {
            if (!this.SchemaContainsItem(item, ns))
            {
                XmlSchema schema = this.schemas[ns];
                if (schema == null)
                {
                    schema = new XmlSchema {
                        TargetNamespace = ((ns == null) || (ns.Length == 0)) ? null : ns,
                        ElementFormDefault = XmlSchemaForm.Qualified
                    };
                    this.schemas.Add(schema);
                }
                schema.Items.Add(item);
            }
            if (referencingNs != null)
            {
                this.AddSchemaImport(ns, referencingNs);
            }
        }

        private void CheckForDuplicateType(string newTypeName, string newNamespace)
        {
            XmlSchema schema = this.schemas[newNamespace];
            if (schema != null)
            {
                foreach (XmlSchemaType type in schema.Items)
                {
                    if ((type != null) && (type.Name == newTypeName))
                    {
                        throw new InvalidOperationException(Res.GetString("XmlDuplicateTypeName", new object[] { newTypeName, newNamespace }));
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

        private XmlQualifiedName ExportArrayMapping(ArrayMapping mapping, string ns)
        {
            while (mapping.Next != null)
            {
                mapping = mapping.Next;
            }
            if (((XmlSchemaComplexType) this.types[mapping]) == null)
            {
                this.CheckForDuplicateType(mapping.TypeName, mapping.Namespace);
                XmlSchemaComplexType type = new XmlSchemaComplexType {
                    Name = mapping.TypeName
                };
                this.types.Add(mapping, type);
                this.AddSchemaItem(type, mapping.Namespace, ns);
                this.AddSchemaImport("http://schemas.xmlsoap.org/soap/encoding/", mapping.Namespace);
                this.AddSchemaImport("http://schemas.xmlsoap.org/wsdl/", mapping.Namespace);
                XmlSchemaComplexContentRestriction restriction = new XmlSchemaComplexContentRestriction();
                XmlQualifiedName name = this.ExportTypeMapping(mapping.Elements[0].Mapping, mapping.Namespace);
                if (name.IsEmpty)
                {
                    name = new XmlQualifiedName("anyType", "http://www.w3.org/2001/XMLSchema");
                }
                XmlSchemaAttribute item = new XmlSchemaAttribute {
                    RefName = ArrayTypeQName
                };
                XmlAttribute attribute2 = new XmlAttribute("wsdl", "arrayType", "http://schemas.xmlsoap.org/wsdl/", this.Document) {
                    Value = name.Namespace + ":" + name.Name + "[]"
                };
                item.UnhandledAttributes = new XmlAttribute[] { attribute2 };
                restriction.Attributes.Add(item);
                restriction.BaseTypeName = ArrayQName;
                XmlSchemaComplexContent content = new XmlSchemaComplexContent {
                    Content = restriction
                };
                type.ContentModel = content;
                if (name.Namespace != "http://www.w3.org/2001/XMLSchema")
                {
                    this.AddSchemaImport(name.Namespace, mapping.Namespace);
                }
            }
            else
            {
                this.AddSchemaImport(mapping.Namespace, ns);
            }
            return new XmlQualifiedName(mapping.TypeName, mapping.Namespace);
        }

        private void ExportDerivedMappings(StructMapping mapping)
        {
            for (StructMapping mapping2 = mapping.DerivedMappings; mapping2 != null; mapping2 = mapping2.NextDerivedMapping)
            {
                if (mapping2.IncludeInSchema)
                {
                    this.ExportStructMapping(mapping2, mapping.TypeDesc.IsRoot ? null : mapping.Namespace);
                }
            }
        }

        private void ExportElementAccessor(XmlSchemaGroupBase group, ElementAccessor accessor, bool repeats, bool valueTypeOptional, string ns)
        {
            XmlSchemaElement item = new XmlSchemaElement {
                MinOccurs = (repeats || valueTypeOptional) ? 0 : 1,
                MaxOccurs = repeats ? 79228162514264337593543950335M : 1M,
                Name = accessor.Name,
                IsNillable = accessor.IsNullable || (accessor.Mapping is NullableMapping),
                Form = XmlSchemaForm.Unqualified,
                SchemaTypeName = this.ExportTypeMapping(accessor.Mapping, accessor.Namespace)
            };
            group.Items.Add(item);
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

        private XmlQualifiedName ExportEnumMapping(EnumMapping mapping, string ns)
        {
            if (((XmlSchemaSimpleType) this.types[mapping]) == null)
            {
                this.CheckForDuplicateType(mapping.TypeName, mapping.Namespace);
                XmlSchemaSimpleType type = new XmlSchemaSimpleType {
                    Name = mapping.TypeName
                };
                this.types.Add(mapping, type);
                this.AddSchemaItem(type, mapping.Namespace, ns);
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
            else
            {
                this.AddSchemaImport(mapping.Namespace, ns);
            }
            return new XmlQualifiedName(mapping.TypeName, mapping.Namespace);
        }

        public void ExportMembersMapping(XmlMembersMapping xmlMembersMapping)
        {
            this.ExportMembersMapping(xmlMembersMapping, false);
        }

        private XmlQualifiedName ExportMembersMapping(MembersMapping mapping, string ns)
        {
            XmlSchemaComplexType type = (XmlSchemaComplexType) this.types[mapping];
            if (type == null)
            {
                this.CheckForDuplicateType(mapping.TypeName, mapping.Namespace);
                type = new XmlSchemaComplexType {
                    Name = mapping.TypeName
                };
                this.types.Add(mapping, type);
                this.AddSchemaItem(type, mapping.Namespace, ns);
                this.ExportTypeMembers(type, mapping.Members, mapping.Namespace);
            }
            else
            {
                this.AddSchemaImport(mapping.Namespace, ns);
            }
            return new XmlQualifiedName(type.Name, mapping.Namespace);
        }

        public void ExportMembersMapping(XmlMembersMapping xmlMembersMapping, bool exportEnclosingType)
        {
            this.CheckScope(xmlMembersMapping.Scope);
            MembersMapping mapping = (MembersMapping) xmlMembersMapping.Accessor.Mapping;
            if (exportEnclosingType)
            {
                this.ExportTypeMapping(mapping, null);
            }
            else
            {
                foreach (MemberMapping mapping2 in mapping.Members)
                {
                    if (mapping2.Elements.Length > 0)
                    {
                        this.ExportTypeMapping(mapping2.Elements[0].Mapping, null);
                    }
                }
            }
        }

        private XmlQualifiedName ExportNonXsdPrimitiveMapping(PrimitiveMapping mapping, string ns)
        {
            XmlSchemaType dataType = mapping.TypeDesc.DataType;
            if (!this.SchemaContainsItem(dataType, "http://microsoft.com/wsdl/types/"))
            {
                this.AddSchemaItem(dataType, "http://microsoft.com/wsdl/types/", ns);
            }
            else
            {
                this.AddSchemaImport("http://microsoft.com/wsdl/types/", ns);
            }
            return new XmlQualifiedName(mapping.TypeDesc.DataType.Name, "http://microsoft.com/wsdl/types/");
        }

        private XmlQualifiedName ExportPrimitiveMapping(PrimitiveMapping mapping)
        {
            return new XmlQualifiedName(mapping.TypeDesc.DataType.Name, "http://www.w3.org/2001/XMLSchema");
        }

        private XmlQualifiedName ExportRootMapping(StructMapping mapping)
        {
            if (!this.exportedRoot)
            {
                this.exportedRoot = true;
                this.ExportDerivedMappings(mapping);
            }
            return new XmlQualifiedName("anyType", "http://www.w3.org/2001/XMLSchema");
        }

        private XmlQualifiedName ExportStructMapping(StructMapping mapping, string ns)
        {
            if (mapping.TypeDesc.IsRoot)
            {
                return this.ExportRootMapping(mapping);
            }
            XmlSchemaComplexType type = (XmlSchemaComplexType) this.types[mapping];
            if (type == null)
            {
                if (!mapping.IncludeInSchema)
                {
                    throw new InvalidOperationException(Res.GetString("XmlSoapCannotIncludeInSchema", new object[] { mapping.TypeDesc.Name }));
                }
                this.CheckForDuplicateType(mapping.TypeName, mapping.Namespace);
                type = new XmlSchemaComplexType {
                    Name = mapping.TypeName
                };
                this.types.Add(mapping, type);
                this.AddSchemaItem(type, mapping.Namespace, ns);
                type.IsAbstract = mapping.TypeDesc.IsAbstract;
                if ((mapping.BaseMapping != null) && mapping.BaseMapping.IncludeInSchema)
                {
                    XmlSchemaComplexContentExtension extension = new XmlSchemaComplexContentExtension {
                        BaseTypeName = this.ExportStructMapping(mapping.BaseMapping, mapping.Namespace)
                    };
                    XmlSchemaComplexContent content = new XmlSchemaComplexContent {
                        Content = extension
                    };
                    type.ContentModel = content;
                }
                this.ExportTypeMembers(type, mapping.Members, mapping.Namespace);
                this.ExportDerivedMappings(mapping);
            }
            else
            {
                this.AddSchemaImport(mapping.Namespace, ns);
            }
            return new XmlQualifiedName(type.Name, mapping.Namespace);
        }

        public void ExportTypeMapping(XmlTypeMapping xmlTypeMapping)
        {
            this.CheckScope(xmlTypeMapping.Scope);
            this.ExportTypeMapping(xmlTypeMapping.Mapping, null);
        }

        private XmlQualifiedName ExportTypeMapping(TypeMapping mapping, string ns)
        {
            if (mapping is ArrayMapping)
            {
                return this.ExportArrayMapping((ArrayMapping) mapping, ns);
            }
            if (mapping is EnumMapping)
            {
                return this.ExportEnumMapping((EnumMapping) mapping, ns);
            }
            if (mapping is PrimitiveMapping)
            {
                PrimitiveMapping mapping2 = (PrimitiveMapping) mapping;
                if (mapping2.TypeDesc.IsXsdType)
                {
                    return this.ExportPrimitiveMapping(mapping2);
                }
                return this.ExportNonXsdPrimitiveMapping(mapping2, ns);
            }
            if (mapping is StructMapping)
            {
                return this.ExportStructMapping((StructMapping) mapping, ns);
            }
            if (mapping is NullableMapping)
            {
                return this.ExportTypeMapping(((NullableMapping) mapping).BaseMapping, ns);
            }
            if (!(mapping is MembersMapping))
            {
                throw new ArgumentException(Res.GetString("XmlInternalError"), "mapping");
            }
            return this.ExportMembersMapping((MembersMapping) mapping, ns);
        }

        private void ExportTypeMembers(XmlSchemaComplexType type, MemberMapping[] members, string ns)
        {
            XmlSchemaGroupBase group = new XmlSchemaSequence();
            for (int i = 0; i < members.Length; i++)
            {
                MemberMapping mapping = members[i];
                if (mapping.Elements.Length > 0)
                {
                    bool valueTypeOptional = ((mapping.CheckSpecified != SpecifiedAccessor.None) || mapping.CheckShouldPersist) || !mapping.TypeDesc.IsValueType;
                    this.ExportElementAccessors(group, mapping.Elements, false, valueTypeOptional, ns);
                }
            }
            if (group.Items.Count > 0)
            {
                if (type.ContentModel != null)
                {
                    if (type.ContentModel.Content is XmlSchemaComplexContentExtension)
                    {
                        ((XmlSchemaComplexContentExtension) type.ContentModel.Content).Particle = group;
                    }
                    else
                    {
                        if (!(type.ContentModel.Content is XmlSchemaComplexContentRestriction))
                        {
                            throw new InvalidOperationException(Res.GetString("XmlInvalidContent", new object[] { type.ContentModel.Content.GetType().Name }));
                        }
                        ((XmlSchemaComplexContentRestriction) type.ContentModel.Content).Particle = group;
                    }
                }
                else
                {
                    type.Particle = group;
                }
            }
        }

        private XmlSchemaImport FindImport(XmlSchema schema, string ns)
        {
            foreach (object obj2 in schema.Includes)
            {
                if (obj2 is XmlSchemaImport)
                {
                    XmlSchemaImport import = (XmlSchemaImport) obj2;
                    if (import.Namespace == ns)
                    {
                        return import;
                    }
                }
            }
            return null;
        }

        private bool SchemaContainsItem(XmlSchemaObject item, string ns)
        {
            XmlSchema schema = this.schemas[ns];
            return ((schema != null) && schema.Items.Contains(item));
        }

        internal XmlDocument Document
        {
            get
            {
                if (this.document == null)
                {
                    this.document = new XmlDocument();
                }
                return this.document;
            }
        }
    }
}

