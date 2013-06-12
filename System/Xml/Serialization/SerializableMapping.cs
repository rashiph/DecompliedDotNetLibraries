namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;

    internal class SerializableMapping : SpecialMapping
    {
        private bool any;
        private SerializableMapping baseMapping;
        private SerializableMapping derivedMappings;
        private MethodInfo getSchemaMethod;
        private string namespaces;
        private bool needSchema;
        private SerializableMapping next;
        private SerializableMapping nextDerivedMapping;
        private XmlSchema schema;
        private XmlSchemaSet schemas;
        private System.Type type;
        private XmlSchemaType xsdType;
        private XmlQualifiedName xsiType;

        internal SerializableMapping()
        {
            this.needSchema = true;
        }

        internal SerializableMapping(XmlQualifiedName xsiType, XmlSchemaSet schemas)
        {
            this.needSchema = true;
            this.xsiType = xsiType;
            this.schemas = schemas;
            base.TypeName = xsiType.Name;
            base.Namespace = xsiType.Namespace;
            this.needSchema = false;
        }

        internal SerializableMapping(MethodInfo getSchemaMethod, bool any, string ns)
        {
            this.needSchema = true;
            this.getSchemaMethod = getSchemaMethod;
            this.any = any;
            base.Namespace = ns;
            this.needSchema = getSchemaMethod != null;
        }

        internal void CheckDuplicateElement(XmlSchemaElement element, string elementNs)
        {
            if ((element != null) && ((element.Parent != null) && (element.Parent is XmlSchema)))
            {
                XmlSchemaObjectTable elements = null;
                if ((this.Schema != null) && (this.Schema.TargetNamespace == elementNs))
                {
                    XmlSchemas.Preprocess(this.Schema);
                    elements = this.Schema.Elements;
                }
                else if (this.Schemas != null)
                {
                    elements = this.Schemas.GlobalElements;
                }
                else
                {
                    return;
                }
                foreach (XmlSchemaElement element2 in elements.Values)
                {
                    if ((element2.Name == element.Name) && (element2.QualifiedName.Namespace == elementNs))
                    {
                        if (!this.Match(element2, element))
                        {
                            throw new InvalidOperationException(Res.GetString("XmlSerializableRootDupName", new object[] { this.getSchemaMethod.DeclaringType.FullName, element2.Name, elementNs }));
                        }
                        break;
                    }
                }
            }
        }

        private bool Match(XmlSchemaElement e1, XmlSchemaElement e2)
        {
            if (e1.IsNillable != e2.IsNillable)
            {
                return false;
            }
            if (e1.RefName != e2.RefName)
            {
                return false;
            }
            if (e1.SchemaType != e2.SchemaType)
            {
                return false;
            }
            if (e1.SchemaTypeName != e2.SchemaTypeName)
            {
                return false;
            }
            if (e1.MinOccurs != e2.MinOccurs)
            {
                return false;
            }
            if (e1.MaxOccurs != e2.MaxOccurs)
            {
                return false;
            }
            if (e1.IsAbstract != e2.IsAbstract)
            {
                return false;
            }
            if (e1.DefaultValue != e2.DefaultValue)
            {
                return false;
            }
            if (e1.SubstitutionGroup != e2.SubstitutionGroup)
            {
                return false;
            }
            return true;
        }

        private void RetrieveSerializableSchema()
        {
            if (this.needSchema)
            {
                this.needSchema = false;
                if (this.getSchemaMethod != null)
                {
                    if (this.schemas == null)
                    {
                        this.schemas = new XmlSchemaSet();
                    }
                    object obj2 = this.getSchemaMethod.Invoke(null, new object[] { this.schemas });
                    this.xsiType = XmlQualifiedName.Empty;
                    if (obj2 != null)
                    {
                        if (!typeof(XmlSchemaType).IsAssignableFrom(this.getSchemaMethod.ReturnType))
                        {
                            if (!typeof(XmlQualifiedName).IsAssignableFrom(this.getSchemaMethod.ReturnType))
                            {
                                throw new InvalidOperationException(Res.GetString("XmlGetSchemaMethodReturnType", new object[] { this.type.Name, this.getSchemaMethod.Name, typeof(XmlSchemaProviderAttribute).Name, typeof(XmlQualifiedName).FullName }));
                            }
                            this.xsiType = (XmlQualifiedName) obj2;
                            if (this.xsiType.IsEmpty)
                            {
                                throw new InvalidOperationException(Res.GetString("XmlGetSchemaEmptyTypeName", new object[] { this.type.FullName, this.getSchemaMethod.Name }));
                            }
                        }
                        else
                        {
                            this.xsdType = (XmlSchemaType) obj2;
                            this.xsiType = this.xsdType.QualifiedName;
                        }
                    }
                    else
                    {
                        this.any = true;
                    }
                    this.schemas.ValidationEventHandler += new ValidationEventHandler(SerializableMapping.ValidationCallbackWithErrorCode);
                    this.schemas.Compile();
                    if (!this.xsiType.IsEmpty && (this.xsiType.Namespace != "http://www.w3.org/2001/XMLSchema"))
                    {
                        ArrayList list = (ArrayList) this.schemas.Schemas(this.xsiType.Namespace);
                        if (list.Count == 0)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlMissingSchema", new object[] { this.xsiType.Namespace }));
                        }
                        if (list.Count > 1)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlGetSchemaInclude", new object[] { this.xsiType.Namespace, this.getSchemaMethod.DeclaringType.FullName, this.getSchemaMethod.Name }));
                        }
                        XmlSchema schema = (XmlSchema) list[0];
                        if (schema == null)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlMissingSchema", new object[] { this.xsiType.Namespace }));
                        }
                        this.xsdType = (XmlSchemaType) schema.SchemaTypes[this.xsiType];
                        if (this.xsdType == null)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlGetSchemaTypeMissing", new object[] { this.getSchemaMethod.DeclaringType.FullName, this.getSchemaMethod.Name, this.xsiType.Name, this.xsiType.Namespace }));
                        }
                        this.xsdType = (this.xsdType.Redefined != null) ? this.xsdType.Redefined : this.xsdType;
                    }
                }
                else
                {
                    this.schema = ((IXmlSerializable) Activator.CreateInstance(this.type)).GetSchema();
                    if ((this.schema != null) && ((this.schema.Id == null) || (this.schema.Id.Length == 0)))
                    {
                        throw new InvalidOperationException(Res.GetString("XmlSerializableNameMissing1", new object[] { this.type.FullName }));
                    }
                }
            }
        }

        internal void SetBaseMapping(SerializableMapping mapping)
        {
            this.baseMapping = mapping;
            if (this.baseMapping != null)
            {
                this.nextDerivedMapping = this.baseMapping.derivedMappings;
                this.baseMapping.derivedMappings = this;
                if (this == this.nextDerivedMapping)
                {
                    throw new InvalidOperationException(Res.GetString("XmlCircularDerivation", new object[] { base.TypeDesc.FullName }));
                }
            }
        }

        internal static void ValidationCallbackWithErrorCode(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Error)
            {
                throw new InvalidOperationException(Res.GetString("XmlSerializableSchemaError", new object[] { typeof(IXmlSerializable).Name, args.Message }));
            }
        }

        internal SerializableMapping DerivedMappings
        {
            get
            {
                return this.derivedMappings;
            }
        }

        internal bool IsAny
        {
            get
            {
                if (this.any)
                {
                    return true;
                }
                if (this.getSchemaMethod == null)
                {
                    return false;
                }
                if (this.needSchema && typeof(XmlSchemaType).IsAssignableFrom(this.getSchemaMethod.ReturnType))
                {
                    return false;
                }
                this.RetrieveSerializableSchema();
                return this.any;
            }
        }

        internal string NamespaceList
        {
            get
            {
                this.RetrieveSerializableSchema();
                if (this.namespaces == null)
                {
                    if (this.schemas != null)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (XmlSchema schema in this.schemas.Schemas())
                        {
                            if ((schema.TargetNamespace != null) && (schema.TargetNamespace.Length > 0))
                            {
                                if (builder.Length > 0)
                                {
                                    builder.Append(" ");
                                }
                                builder.Append(schema.TargetNamespace);
                            }
                        }
                        this.namespaces = builder.ToString();
                    }
                    else
                    {
                        this.namespaces = string.Empty;
                    }
                }
                return this.namespaces;
            }
        }

        internal SerializableMapping Next
        {
            get
            {
                return this.next;
            }
            set
            {
                this.next = value;
            }
        }

        internal SerializableMapping NextDerivedMapping
        {
            get
            {
                return this.nextDerivedMapping;
            }
        }

        internal XmlSchema Schema
        {
            get
            {
                this.RetrieveSerializableSchema();
                return this.schema;
            }
        }

        internal XmlSchemaSet Schemas
        {
            get
            {
                this.RetrieveSerializableSchema();
                return this.schemas;
            }
        }

        internal System.Type Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        internal XmlSchemaType XsdType
        {
            get
            {
                this.RetrieveSerializableSchema();
                return this.xsdType;
            }
        }

        internal XmlQualifiedName XsiType
        {
            get
            {
                if (this.needSchema)
                {
                    if (this.getSchemaMethod == null)
                    {
                        return null;
                    }
                    if (typeof(XmlSchemaType).IsAssignableFrom(this.getSchemaMethod.ReturnType))
                    {
                        return null;
                    }
                    this.RetrieveSerializableSchema();
                }
                return this.xsiType;
            }
        }
    }
}

