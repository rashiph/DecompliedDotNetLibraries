namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Schema;

    public class XmlSchemas : CollectionBase, IEnumerable<System.Xml.Schema.XmlSchema>, IEnumerable
    {
        private SchemaObjectCache cache;
        internal Hashtable delayedSchemas = new Hashtable();
        private bool isCompiled;
        private Hashtable mergedSchemas;
        private Hashtable references;
        private XmlSchemaSet schemaSet;
        private bool shareTypes;
        private static System.Xml.Schema.XmlSchema xml;
        internal const string xmlSchema = "<?xml version='1.0' encoding='UTF-8' ?> \r\n<xs:schema targetNamespace='http://www.w3.org/XML/1998/namespace' xmlns:xs='http://www.w3.org/2001/XMLSchema' xml:lang='en'>\r\n <xs:attribute name='lang' type='xs:language'/>\r\n <xs:attribute name='space'>\r\n  <xs:simpleType>\r\n   <xs:restriction base='xs:NCName'>\r\n    <xs:enumeration value='default'/>\r\n    <xs:enumeration value='preserve'/>\r\n   </xs:restriction>\r\n  </xs:simpleType>\r\n </xs:attribute>\r\n <xs:attribute name='base' type='xs:anyURI'/>\r\n <xs:attribute name='id' type='xs:ID' />\r\n <xs:attributeGroup name='specialAttrs'>\r\n  <xs:attribute ref='xml:base'/>\r\n  <xs:attribute ref='xml:lang'/>\r\n  <xs:attribute ref='xml:space'/>\r\n </xs:attributeGroup>\r\n</xs:schema>";
        private static System.Xml.Schema.XmlSchema xsd;

        public int Add(System.Xml.Schema.XmlSchema schema)
        {
            if (base.List.Contains(schema))
            {
                return base.List.IndexOf(schema);
            }
            return base.List.Add(schema);
        }

        public void Add(XmlSchemas schemas)
        {
            foreach (System.Xml.Schema.XmlSchema schema in schemas)
            {
                this.Add(schema);
            }
        }

        internal int Add(System.Xml.Schema.XmlSchema schema, bool delay)
        {
            if (!delay)
            {
                return this.Add(schema);
            }
            if (this.delayedSchemas[schema] == null)
            {
                this.delayedSchemas.Add(schema, schema);
            }
            return -1;
        }

        public int Add(System.Xml.Schema.XmlSchema schema, Uri baseUri)
        {
            if (base.List.Contains(schema))
            {
                return base.List.IndexOf(schema);
            }
            if (baseUri != null)
            {
                schema.BaseUri = baseUri;
            }
            return base.List.Add(schema);
        }

        private void AddImport(IList schemas, string ns)
        {
            foreach (System.Xml.Schema.XmlSchema schema in schemas)
            {
                bool flag = true;
                foreach (XmlSchemaExternal external in schema.Includes)
                {
                    if ((external is XmlSchemaImport) && (((XmlSchemaImport) external).Namespace == ns))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    XmlSchemaImport item = new XmlSchemaImport {
                        Namespace = ns
                    };
                    schema.Includes.Add(item);
                }
            }
        }

        private void AddName(System.Xml.Schema.XmlSchema schema)
        {
            if (this.isCompiled)
            {
                throw new InvalidOperationException(Res.GetString("XmlSchemaCompiled"));
            }
            if (this.SchemaSet.Contains(schema))
            {
                this.SchemaSet.Reprocess(schema);
            }
            else
            {
                this.Prepare(schema);
                this.SchemaSet.Add(schema);
            }
        }

        public void AddReference(System.Xml.Schema.XmlSchema schema)
        {
            this.References[schema] = schema;
        }

        public void Compile(ValidationEventHandler handler, bool fullCompile)
        {
            if (!this.isCompiled)
            {
                foreach (System.Xml.Schema.XmlSchema schema in this.delayedSchemas.Values)
                {
                    this.Merge(schema);
                }
                this.delayedSchemas.Clear();
                if (fullCompile)
                {
                    this.schemaSet = new XmlSchemaSet();
                    this.schemaSet.XmlResolver = null;
                    this.schemaSet.ValidationEventHandler += handler;
                    foreach (System.Xml.Schema.XmlSchema schema2 in this.References.Values)
                    {
                        this.schemaSet.Add(schema2);
                    }
                    int count = this.schemaSet.Count;
                    foreach (System.Xml.Schema.XmlSchema schema3 in base.List)
                    {
                        if (!this.SchemaSet.Contains(schema3))
                        {
                            this.schemaSet.Add(schema3);
                            count++;
                        }
                    }
                    if (!this.SchemaSet.Contains("http://www.w3.org/2001/XMLSchema"))
                    {
                        this.AddReference(XsdSchema);
                        this.schemaSet.Add(XsdSchema);
                        count++;
                    }
                    if (!this.SchemaSet.Contains("http://www.w3.org/XML/1998/namespace"))
                    {
                        this.AddReference(XmlSchema);
                        this.schemaSet.Add(XmlSchema);
                        count++;
                    }
                    this.schemaSet.Compile();
                    this.schemaSet.ValidationEventHandler -= handler;
                    this.isCompiled = this.schemaSet.IsCompiled && (count == this.schemaSet.Count);
                }
                else
                {
                    try
                    {
                        XmlNameTable nameTable = new System.Xml.NameTable();
                        Preprocessor preprocessor = new Preprocessor(nameTable, new SchemaNames(nameTable), null) {
                            XmlResolver = null,
                            SchemaLocations = new Hashtable(),
                            ChameleonSchemas = new Hashtable()
                        };
                        foreach (System.Xml.Schema.XmlSchema schema4 in this.SchemaSet.Schemas())
                        {
                            preprocessor.Execute(schema4, schema4.TargetNamespace, true);
                        }
                    }
                    catch (XmlSchemaException exception)
                    {
                        throw CreateValidationException(exception, exception.Message);
                    }
                }
            }
        }

        public bool Contains(string targetNamespace)
        {
            return this.SchemaSet.Contains(targetNamespace);
        }

        public bool Contains(System.Xml.Schema.XmlSchema schema)
        {
            return base.List.Contains(schema);
        }

        public void CopyTo(System.Xml.Schema.XmlSchema[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        private static System.Xml.Schema.XmlSchema CreateFakeXsdSchema(string ns, string name)
        {
            System.Xml.Schema.XmlSchema schema = new System.Xml.Schema.XmlSchema {
                TargetNamespace = ns
            };
            XmlSchemaElement item = new XmlSchemaElement {
                Name = name
            };
            XmlSchemaComplexType type = new XmlSchemaComplexType();
            item.SchemaType = type;
            schema.Items.Add(item);
            return schema;
        }

        internal static Exception CreateValidationException(XmlSchemaException exception, string message)
        {
            XmlSchemaObject sourceSchemaObject = exception.SourceSchemaObject;
            if ((exception.LineNumber == 0) && (exception.LinePosition == 0))
            {
                throw new InvalidOperationException(GetSchemaItem(sourceSchemaObject, null, message), exception);
            }
            string targetNamespace = null;
            if (sourceSchemaObject != null)
            {
                while (sourceSchemaObject.Parent != null)
                {
                    sourceSchemaObject = sourceSchemaObject.Parent;
                }
                if (sourceSchemaObject is System.Xml.Schema.XmlSchema)
                {
                    targetNamespace = ((System.Xml.Schema.XmlSchema) sourceSchemaObject).TargetNamespace;
                }
            }
            throw new InvalidOperationException(Res.GetString("XmlSchemaSyntaxErrorDetails", new object[] { targetNamespace, message, exception.LineNumber, exception.LinePosition }), exception);
        }

        private static string Dump(XmlSchemaObject o)
        {
            XmlWriterSettings settings = new XmlWriterSettings {
                OmitXmlDeclaration = true,
                Indent = true
            };
            XmlSerializer serializer = new XmlSerializer(o.GetType());
            StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
            XmlWriter xmlWriter = XmlWriter.Create(output, settings);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("xs", "http://www.w3.org/2001/XMLSchema");
            serializer.Serialize(xmlWriter, o, namespaces);
            return output.ToString();
        }

        internal XmlSchemaObject Find(XmlSchemaObject o, IList originals)
        {
            string str = ItemName(o);
            if (str != null)
            {
                Type type = o.GetType();
                foreach (System.Xml.Schema.XmlSchema schema in originals)
                {
                    foreach (XmlSchemaObject obj2 in schema.Items)
                    {
                        if ((obj2.GetType() == type) && (str == ItemName(obj2)))
                        {
                            return obj2;
                        }
                    }
                }
            }
            return null;
        }

        public object Find(XmlQualifiedName name, Type type)
        {
            return this.Find(name, type, true);
        }

        internal object Find(XmlQualifiedName name, Type type, bool checkCache)
        {
            if (!this.IsCompiled)
            {
                foreach (System.Xml.Schema.XmlSchema schema in base.List)
                {
                    Preprocess(schema);
                }
            }
            IList list = (IList) this.SchemaSet.Schemas(name.Namespace);
            if (list != null)
            {
                foreach (System.Xml.Schema.XmlSchema schema2 in list)
                {
                    Preprocess(schema2);
                    XmlSchemaObject obj2 = null;
                    if (typeof(XmlSchemaType).IsAssignableFrom(type))
                    {
                        obj2 = schema2.SchemaTypes[name];
                        if ((obj2 != null) && type.IsAssignableFrom(obj2.GetType()))
                        {
                            goto Label_0169;
                        }
                        continue;
                    }
                    if (type == typeof(XmlSchemaGroup))
                    {
                        obj2 = schema2.Groups[name];
                    }
                    else if (type == typeof(XmlSchemaAttributeGroup))
                    {
                        obj2 = schema2.AttributeGroups[name];
                    }
                    else if (type == typeof(XmlSchemaElement))
                    {
                        obj2 = schema2.Elements[name];
                    }
                    else if (type == typeof(XmlSchemaAttribute))
                    {
                        obj2 = schema2.Attributes[name];
                    }
                    else if (type == typeof(XmlSchemaNotation))
                    {
                        obj2 = schema2.Notations[name];
                    }
                Label_0169:
                    if (((obj2 != null) && this.shareTypes) && (checkCache && !this.IsReference(obj2)))
                    {
                        obj2 = this.Cache.AddItem(obj2, name, this);
                    }
                    if (obj2 != null)
                    {
                        return obj2;
                    }
                }
            }
            return null;
        }

        internal static XmlQualifiedName GetParentName(XmlSchemaObject item)
        {
            while (item.Parent != null)
            {
                if (item.Parent is XmlSchemaType)
                {
                    XmlSchemaType parent = (XmlSchemaType) item.Parent;
                    if ((parent.Name != null) && (parent.Name.Length != 0))
                    {
                        return parent.QualifiedName;
                    }
                }
                item = item.Parent;
            }
            return XmlQualifiedName.Empty;
        }

        private static string GetSchemaItem(XmlSchemaObject o, string ns, string details)
        {
            if (o != null)
            {
                while ((o.Parent != null) && !(o.Parent is System.Xml.Schema.XmlSchema))
                {
                    o = o.Parent;
                }
                if ((ns == null) || (ns.Length == 0))
                {
                    XmlSchemaObject parent = o;
                    while (parent.Parent != null)
                    {
                        parent = parent.Parent;
                    }
                    if (parent is System.Xml.Schema.XmlSchema)
                    {
                        ns = ((System.Xml.Schema.XmlSchema) parent).TargetNamespace;
                    }
                }
                if (o is XmlSchemaNotation)
                {
                    return Res.GetString("XmlSchemaNamedItem", new object[] { ns, "notation", ((XmlSchemaNotation) o).Name, details });
                }
                if (o is XmlSchemaGroup)
                {
                    return Res.GetString("XmlSchemaNamedItem", new object[] { ns, "group", ((XmlSchemaGroup) o).Name, details });
                }
                if (o is XmlSchemaElement)
                {
                    XmlSchemaElement element = (XmlSchemaElement) o;
                    if ((element.Name == null) || (element.Name.Length == 0))
                    {
                        XmlQualifiedName parentName = GetParentName(o);
                        return Res.GetString("XmlSchemaElementReference", new object[] { element.RefName.ToString(), parentName.Name, parentName.Namespace });
                    }
                    return Res.GetString("XmlSchemaNamedItem", new object[] { ns, "element", element.Name, details });
                }
                if (o is XmlSchemaType)
                {
                    object[] objArray5 = new object[4];
                    objArray5[0] = ns;
                    objArray5[1] = (o.GetType() == typeof(XmlSchemaSimpleType)) ? "simpleType" : "complexType";
                    objArray5[2] = ((XmlSchemaType) o).Name;
                    return Res.GetString("XmlSchemaNamedItem", objArray5);
                }
                if (o is XmlSchemaAttributeGroup)
                {
                    return Res.GetString("XmlSchemaNamedItem", new object[] { ns, "attributeGroup", ((XmlSchemaAttributeGroup) o).Name, details });
                }
                if (o is XmlSchemaAttribute)
                {
                    XmlSchemaAttribute attribute = (XmlSchemaAttribute) o;
                    if ((attribute.Name == null) || (attribute.Name.Length == 0))
                    {
                        XmlQualifiedName name2 = GetParentName(o);
                        return Res.GetString("XmlSchemaAttributeReference", new object[] { attribute.RefName.ToString(), name2.Name, name2.Namespace });
                    }
                    return Res.GetString("XmlSchemaNamedItem", new object[] { ns, "attribute", attribute.Name, details });
                }
                if (o is XmlSchemaContent)
                {
                    XmlQualifiedName name3 = GetParentName(o);
                    object[] objArray9 = new object[3];
                    objArray9[0] = name3.Name;
                    objArray9[1] = name3.Namespace;
                    return Res.GetString("XmlSchemaContentDef", objArray9);
                }
                if (o is XmlSchemaExternal)
                {
                    string str2 = (o is XmlSchemaImport) ? "import" : ((o is XmlSchemaInclude) ? "include" : ((o is XmlSchemaRedefine) ? "redefine" : o.GetType().Name));
                    return Res.GetString("XmlSchemaItem", new object[] { ns, str2, details });
                }
                if (o is System.Xml.Schema.XmlSchema)
                {
                    return Res.GetString("XmlSchema", new object[] { ns, details });
                }
                object[] args = new object[4];
                args[0] = ns;
                args[1] = o.GetType().Name;
                args[3] = details;
                return Res.GetString("XmlSchemaNamedItem", args);
            }
            return null;
        }

        public IList GetSchemas(string ns)
        {
            return (IList) this.SchemaSet.Schemas(ns);
        }

        internal static void IgnoreCompileErrors(object sender, ValidationEventArgs args)
        {
        }

        public int IndexOf(System.Xml.Schema.XmlSchema schema)
        {
            return base.List.IndexOf(schema);
        }

        public void Insert(int index, System.Xml.Schema.XmlSchema schema)
        {
            base.List.Insert(index, schema);
        }

        public static bool IsDataSet(System.Xml.Schema.XmlSchema schema)
        {
            foreach (XmlSchemaObject obj2 in schema.Items)
            {
                if (obj2 is XmlSchemaElement)
                {
                    XmlSchemaElement element = (XmlSchemaElement) obj2;
                    if (element.UnhandledAttributes != null)
                    {
                        foreach (XmlAttribute attribute in element.UnhandledAttributes)
                        {
                            if (((attribute.LocalName == "IsDataSet") && (attribute.NamespaceURI == "urn:schemas-microsoft-com:xml-msdata")) && (((attribute.Value == "True") || (attribute.Value == "true")) || (attribute.Value == "1")))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        internal bool IsReference(XmlSchemaObject type)
        {
            XmlSchemaObject key = type;
            while (key.Parent != null)
            {
                key = key.Parent;
            }
            return this.References.Contains(key);
        }

        private static string ItemName(XmlSchemaObject o)
        {
            if (o is XmlSchemaNotation)
            {
                return ((XmlSchemaNotation) o).Name;
            }
            if (o is XmlSchemaGroup)
            {
                return ((XmlSchemaGroup) o).Name;
            }
            if (o is XmlSchemaElement)
            {
                return ((XmlSchemaElement) o).Name;
            }
            if (o is XmlSchemaType)
            {
                return ((XmlSchemaType) o).Name;
            }
            if (o is XmlSchemaAttributeGroup)
            {
                return ((XmlSchemaAttributeGroup) o).Name;
            }
            if (o is XmlSchemaAttribute)
            {
                return ((XmlSchemaAttribute) o).Name;
            }
            return null;
        }

        private void Merge(System.Xml.Schema.XmlSchema schema)
        {
            if (this.MergedSchemas[schema] == null)
            {
                IList originals = (IList) this.SchemaSet.Schemas(schema.TargetNamespace);
                if ((originals != null) && (originals.Count > 0))
                {
                    this.MergedSchemas.Add(schema, schema);
                    this.Merge(originals, schema);
                }
                else
                {
                    this.Add(schema);
                    this.MergedSchemas.Add(schema, schema);
                }
            }
        }

        private void Merge(IList originals, System.Xml.Schema.XmlSchema schema)
        {
            foreach (System.Xml.Schema.XmlSchema schema2 in originals)
            {
                if (schema == schema2)
                {
                    return;
                }
            }
            foreach (XmlSchemaExternal external in schema.Includes)
            {
                if (external is XmlSchemaImport)
                {
                    external.SchemaLocation = null;
                    if (external.Schema != null)
                    {
                        this.Merge(external.Schema);
                    }
                    else
                    {
                        this.AddImport(originals, ((XmlSchemaImport) external).Namespace);
                    }
                }
                else if (external.Schema == null)
                {
                    if (external.SchemaLocation != null)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlSchemaIncludeLocation", new object[] { base.GetType().Name, external.SchemaLocation }));
                    }
                }
                else
                {
                    external.SchemaLocation = null;
                    this.Merge(originals, external.Schema);
                }
            }
            bool[] flagArray = new bool[schema.Items.Count];
            int num = 0;
            for (int i = 0; i < schema.Items.Count; i++)
            {
                XmlSchemaObject o = schema.Items[i];
                XmlSchemaObject obj3 = this.Find(o, originals);
                if (obj3 != null)
                {
                    if (!this.Cache.Match(obj3, o, this.shareTypes))
                    {
                        throw new InvalidOperationException(MergeFailedMessage(o, obj3, schema.TargetNamespace));
                    }
                    flagArray[i] = true;
                    num++;
                }
            }
            if (num != schema.Items.Count)
            {
                System.Xml.Schema.XmlSchema schema3 = (System.Xml.Schema.XmlSchema) originals[0];
                for (int j = 0; j < schema.Items.Count; j++)
                {
                    if (!flagArray[j])
                    {
                        schema3.Items.Add(schema.Items[j]);
                    }
                }
                schema3.IsPreprocessed = false;
                Preprocess(schema3);
            }
        }

        private static string MergeFailedMessage(XmlSchemaObject src, XmlSchemaObject dest, string ns)
        {
            return ((Res.GetString("XmlSerializableMergeItem", new object[] { ns, GetSchemaItem(src, ns, null) }) + "\r\n" + Dump(src)) + "\r\n" + Dump(dest));
        }

        protected override void OnClear()
        {
            this.schemaSet = null;
        }

        protected override void OnInsert(int index, object value)
        {
            this.AddName((System.Xml.Schema.XmlSchema) value);
        }

        protected override void OnRemove(int index, object value)
        {
            this.RemoveName((System.Xml.Schema.XmlSchema) value);
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            this.RemoveName((System.Xml.Schema.XmlSchema) oldValue);
            this.AddName((System.Xml.Schema.XmlSchema) newValue);
        }

        private void Prepare(System.Xml.Schema.XmlSchema schema)
        {
            ArrayList list = new ArrayList();
            string targetNamespace = schema.TargetNamespace;
            foreach (XmlSchemaExternal external in schema.Includes)
            {
                if ((external is XmlSchemaImport) && (targetNamespace == ((XmlSchemaImport) external).Namespace))
                {
                    list.Add(external);
                }
            }
            foreach (XmlSchemaObject obj2 in list)
            {
                schema.Includes.Remove(obj2);
            }
        }

        internal static void Preprocess(System.Xml.Schema.XmlSchema schema)
        {
            if (!schema.IsPreprocessed)
            {
                try
                {
                    XmlNameTable nameTable = new System.Xml.NameTable();
                    new Preprocessor(nameTable, new SchemaNames(nameTable), null) { SchemaLocations = new Hashtable() }.Execute(schema, schema.TargetNamespace, false);
                }
                catch (XmlSchemaException exception)
                {
                    throw CreateValidationException(exception, exception.Message);
                }
            }
        }

        public void Remove(System.Xml.Schema.XmlSchema schema)
        {
            base.List.Remove(schema);
        }

        private void RemoveName(System.Xml.Schema.XmlSchema schema)
        {
            this.SchemaSet.Remove(schema);
        }

        internal void SetCache(SchemaObjectCache cache, bool shareTypes)
        {
            this.shareTypes = shareTypes;
            this.cache = cache;
            if (shareTypes)
            {
                cache.GenerateSchemaGraph(this);
            }
        }

        IEnumerator<System.Xml.Schema.XmlSchema> IEnumerable<System.Xml.Schema.XmlSchema>.GetEnumerator()
        {
            return new XmlSchemaEnumerator(this);
        }

        internal SchemaObjectCache Cache
        {
            get
            {
                if (this.cache == null)
                {
                    this.cache = new SchemaObjectCache();
                }
                return this.cache;
            }
        }

        public bool IsCompiled
        {
            get
            {
                return this.isCompiled;
            }
        }

        public System.Xml.Schema.XmlSchema this[int index]
        {
            get
            {
                return (System.Xml.Schema.XmlSchema) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        public System.Xml.Schema.XmlSchema this[string ns]
        {
            get
            {
                IList list = (IList) this.SchemaSet.Schemas(ns);
                if (list.Count == 0)
                {
                    return null;
                }
                if (list.Count != 1)
                {
                    throw new InvalidOperationException(Res.GetString("XmlSchemaDuplicateNamespace", new object[] { ns }));
                }
                return (System.Xml.Schema.XmlSchema) list[0];
            }
        }

        internal Hashtable MergedSchemas
        {
            get
            {
                if (this.mergedSchemas == null)
                {
                    this.mergedSchemas = new Hashtable();
                }
                return this.mergedSchemas;
            }
        }

        internal Hashtable References
        {
            get
            {
                if (this.references == null)
                {
                    this.references = new Hashtable();
                }
                return this.references;
            }
        }

        internal XmlSchemaSet SchemaSet
        {
            get
            {
                if (this.schemaSet == null)
                {
                    this.schemaSet = new XmlSchemaSet();
                    this.schemaSet.XmlResolver = null;
                    this.schemaSet.ValidationEventHandler += new ValidationEventHandler(XmlSchemas.IgnoreCompileErrors);
                }
                return this.schemaSet;
            }
        }

        internal static System.Xml.Schema.XmlSchema XmlSchema
        {
            get
            {
                if (xml == null)
                {
                    xml = System.Xml.Schema.XmlSchema.Read(new StringReader("<?xml version='1.0' encoding='UTF-8' ?> \r\n<xs:schema targetNamespace='http://www.w3.org/XML/1998/namespace' xmlns:xs='http://www.w3.org/2001/XMLSchema' xml:lang='en'>\r\n <xs:attribute name='lang' type='xs:language'/>\r\n <xs:attribute name='space'>\r\n  <xs:simpleType>\r\n   <xs:restriction base='xs:NCName'>\r\n    <xs:enumeration value='default'/>\r\n    <xs:enumeration value='preserve'/>\r\n   </xs:restriction>\r\n  </xs:simpleType>\r\n </xs:attribute>\r\n <xs:attribute name='base' type='xs:anyURI'/>\r\n <xs:attribute name='id' type='xs:ID' />\r\n <xs:attributeGroup name='specialAttrs'>\r\n  <xs:attribute ref='xml:base'/>\r\n  <xs:attribute ref='xml:lang'/>\r\n  <xs:attribute ref='xml:space'/>\r\n </xs:attributeGroup>\r\n</xs:schema>"), null);
                }
                return xml;
            }
        }

        internal static System.Xml.Schema.XmlSchema XsdSchema
        {
            get
            {
                if (xsd == null)
                {
                    xsd = CreateFakeXsdSchema("http://www.w3.org/2001/XMLSchema", "schema");
                }
                return xsd;
            }
        }
    }
}

