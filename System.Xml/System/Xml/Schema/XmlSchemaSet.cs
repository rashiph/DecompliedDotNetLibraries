namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Xml;

    public class XmlSchemaSet
    {
        internal XmlSchemaObjectTable attributes;
        private SchemaInfo cachedCompiledInfo;
        private Hashtable chameleonSchemas;
        private XmlSchemaCompilationSettings compilationSettings;
        private bool compileAll;
        internal XmlSchemaObjectTable elements;
        private System.Xml.Schema.ValidationEventHandler eventHandler;
        private System.Xml.Schema.ValidationEventHandler internalEventHandler;
        private object internalSyncObject;
        private bool isCompiled;
        private XmlNameTable nameTable;
        private XmlReaderSettings readerSettings;
        private XmlSchema schemaForSchema;
        private Hashtable schemaLocations;
        private SchemaNames schemaNames;
        private SortedList schemas;
        internal XmlSchemaObjectTable schemaTypes;
        internal XmlSchemaObjectTable substitutionGroups;
        private Hashtable targetNamespaces;
        private XmlSchemaObjectTable typeExtensions;

        public event System.Xml.Schema.ValidationEventHandler ValidationEventHandler
        {
            add
            {
                this.eventHandler = (System.Xml.Schema.ValidationEventHandler) Delegate.Remove(this.eventHandler, this.internalEventHandler);
                this.eventHandler = (System.Xml.Schema.ValidationEventHandler) Delegate.Combine(this.eventHandler, value);
                if (this.eventHandler == null)
                {
                    this.eventHandler = this.internalEventHandler;
                }
            }
            remove
            {
                this.eventHandler = (System.Xml.Schema.ValidationEventHandler) Delegate.Remove(this.eventHandler, value);
                if (this.eventHandler == null)
                {
                    this.eventHandler = this.internalEventHandler;
                }
            }
        }

        public XmlSchemaSet() : this(new System.Xml.NameTable())
        {
        }

        public XmlSchemaSet(XmlNameTable nameTable)
        {
            if (nameTable == null)
            {
                throw new ArgumentNullException("nameTable");
            }
            this.nameTable = nameTable;
            this.schemas = new SortedList();
            this.schemaLocations = new Hashtable();
            this.chameleonSchemas = new Hashtable();
            this.targetNamespaces = new Hashtable();
            this.internalEventHandler = new System.Xml.Schema.ValidationEventHandler(this.InternalValidationCallback);
            this.eventHandler = this.internalEventHandler;
            this.readerSettings = new XmlReaderSettings();
            this.readerSettings.NameTable = nameTable;
            this.readerSettings.DtdProcessing = DtdProcessing.Prohibit;
            this.compilationSettings = new XmlSchemaCompilationSettings();
            this.cachedCompiledInfo = new SchemaInfo();
            this.compileAll = true;
        }

        public XmlSchema Add(XmlSchema schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }
            lock (this.InternalSyncObject)
            {
                if (this.schemas.ContainsKey(schema.SchemaId))
                {
                    return schema;
                }
                return this.Add(schema.TargetNamespace, schema);
            }
        }

        public void Add(XmlSchemaSet schemas)
        {
            if (schemas == null)
            {
                throw new ArgumentNullException("schemas");
            }
            if (this != schemas)
            {
                bool lockTaken = false;
                bool flag2 = false;
                try
                {
                Label_0017:
                    Monitor.TryEnter(this.InternalSyncObject, ref lockTaken);
                    if (!lockTaken)
                    {
                        goto Label_0017;
                    }
                    Monitor.TryEnter(schemas.InternalSyncObject, ref flag2);
                    if (!flag2)
                    {
                        Monitor.Exit(this.InternalSyncObject);
                        lockTaken = false;
                        Thread.Yield();
                        goto Label_0017;
                    }
                    if (schemas.IsCompiled)
                    {
                        this.CopyFromCompiledSet(schemas);
                    }
                    else
                    {
                        bool flag3 = false;
                        string ns = null;
                        foreach (XmlSchema schema2 in schemas.SortedSchemas.Values)
                        {
                            ns = schema2.TargetNamespace;
                            if (ns == null)
                            {
                                ns = string.Empty;
                            }
                            if ((!this.schemas.ContainsKey(schema2.SchemaId) && (this.FindSchemaByNSAndUrl(schema2.BaseUri, ns, null) == null)) && (this.Add(schema2.TargetNamespace, schema2) == null))
                            {
                                flag3 = true;
                                break;
                            }
                        }
                        if (flag3)
                        {
                            foreach (XmlSchema schema3 in schemas.SortedSchemas.Values)
                            {
                                this.schemas.Remove(schema3.SchemaId);
                                this.schemaLocations.Remove(schema3.BaseUri);
                            }
                        }
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(this.InternalSyncObject);
                    }
                    if (flag2)
                    {
                        Monitor.Exit(schemas.InternalSyncObject);
                    }
                }
            }
        }

        public XmlSchema Add(string targetNamespace, string schemaUri)
        {
            if ((schemaUri == null) || (schemaUri.Length == 0))
            {
                throw new ArgumentNullException("schemaUri");
            }
            if (targetNamespace != null)
            {
                targetNamespace = XmlComplianceUtil.CDataNormalize(targetNamespace);
            }
            XmlSchema schema = null;
            lock (this.InternalSyncObject)
            {
                System.Xml.XmlResolver xmlResolver = this.readerSettings.GetXmlResolver();
                if (xmlResolver == null)
                {
                    xmlResolver = new XmlUrlResolver();
                }
                Uri uri = xmlResolver.ResolveUri(null, schemaUri);
                if (this.IsSchemaLoaded(uri, targetNamespace, out schema))
                {
                    return schema;
                }
                XmlReader reader = XmlReader.Create(schemaUri, this.readerSettings);
                try
                {
                    schema = this.Add(targetNamespace, this.ParseSchema(targetNamespace, reader));
                    while (reader.Read())
                    {
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return schema;
        }

        private XmlSchema Add(string targetNamespace, XmlSchema schema)
        {
            if (((schema != null) && (schema.ErrorCount == 0)) && this.PreprocessSchema(ref schema, targetNamespace))
            {
                this.AddSchemaToSet(schema);
                this.isCompiled = false;
                return schema;
            }
            return null;
        }

        public XmlSchema Add(string targetNamespace, XmlReader schemaDocument)
        {
            if (schemaDocument == null)
            {
                throw new ArgumentNullException("schemaDocument");
            }
            if (targetNamespace != null)
            {
                targetNamespace = XmlComplianceUtil.CDataNormalize(targetNamespace);
            }
            lock (this.InternalSyncObject)
            {
                XmlSchema schema = null;
                Uri schemaUri = new Uri(schemaDocument.BaseURI, UriKind.RelativeOrAbsolute);
                if (!this.IsSchemaLoaded(schemaUri, targetNamespace, out schema))
                {
                    DtdProcessing dtdProcessing = this.readerSettings.DtdProcessing;
                    this.SetDtdProcessing(schemaDocument);
                    schema = this.Add(targetNamespace, this.ParseSchema(targetNamespace, schemaDocument));
                    this.readerSettings.DtdProcessing = dtdProcessing;
                }
                return schema;
            }
        }

        internal void Add(string targetNamespace, XmlReader reader, Hashtable validatedNamespaces)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (targetNamespace == null)
            {
                targetNamespace = string.Empty;
            }
            if (validatedNamespaces[targetNamespace] != null)
            {
                if (this.FindSchemaByNSAndUrl(new Uri(reader.BaseURI, UriKind.RelativeOrAbsolute), targetNamespace, null) == null)
                {
                    throw new XmlSchemaException("Sch_ComponentAlreadySeenForNS", targetNamespace);
                }
            }
            else
            {
                XmlSchema schema;
                if (!this.IsSchemaLoaded(new Uri(reader.BaseURI, UriKind.RelativeOrAbsolute), targetNamespace, out schema))
                {
                    schema = this.ParseSchema(targetNamespace, reader);
                    DictionaryEntry[] array = new DictionaryEntry[this.schemaLocations.Count];
                    this.schemaLocations.CopyTo(array, 0);
                    this.Add(targetNamespace, schema);
                    if (schema.ImportedSchemas.Count > 0)
                    {
                        for (int i = 0; i < schema.ImportedSchemas.Count; i++)
                        {
                            XmlSchema schema2 = (XmlSchema) schema.ImportedSchemas[i];
                            string ns = schema2.TargetNamespace;
                            if (ns == null)
                            {
                                ns = string.Empty;
                            }
                            if ((validatedNamespaces[ns] != null) && (this.FindSchemaByNSAndUrl(schema2.BaseUri, ns, array) == null))
                            {
                                this.RemoveRecursive(schema);
                                throw new XmlSchemaException("Sch_ComponentAlreadySeenForNS", ns);
                            }
                        }
                    }
                }
            }
        }

        private void AddSchemaToSet(XmlSchema schema)
        {
            this.schemas.Add(schema.SchemaId, schema);
            string targetNamespace = this.GetTargetNamespace(schema);
            if (this.targetNamespaces[targetNamespace] == null)
            {
                this.targetNamespaces.Add(targetNamespace, targetNamespace);
            }
            if (((this.schemaForSchema == null) && (targetNamespace == "http://www.w3.org/2001/XMLSchema")) && (schema.SchemaTypes[DatatypeImplementation.QnAnyType] != null))
            {
                this.schemaForSchema = schema;
            }
            for (int i = 0; i < schema.ImportedSchemas.Count; i++)
            {
                XmlSchema schema2 = (XmlSchema) schema.ImportedSchemas[i];
                if (!this.schemas.ContainsKey(schema2.SchemaId))
                {
                    this.schemas.Add(schema2.SchemaId, schema2);
                }
                targetNamespace = this.GetTargetNamespace(schema2);
                if (this.targetNamespaces[targetNamespace] == null)
                {
                    this.targetNamespaces.Add(targetNamespace, targetNamespace);
                }
                if (((this.schemaForSchema == null) && (targetNamespace == "http://www.w3.org/2001/XMLSchema")) && (schema.SchemaTypes[DatatypeImplementation.QnAnyType] != null))
                {
                    this.schemaForSchema = schema;
                }
            }
        }

        private bool AddToTable(XmlSchemaObjectTable table, XmlQualifiedName qname, XmlSchemaObject item)
        {
            if (qname.Name.Length != 0)
            {
                XmlSchemaObject obj2 = table[qname];
                if (obj2 != null)
                {
                    if ((obj2 == item) || (obj2.SourceUri == item.SourceUri))
                    {
                        return true;
                    }
                    string res = string.Empty;
                    if (item is XmlSchemaComplexType)
                    {
                        res = "Sch_DupComplexType";
                    }
                    else if (item is XmlSchemaSimpleType)
                    {
                        res = "Sch_DupSimpleType";
                    }
                    else if (item is XmlSchemaElement)
                    {
                        res = "Sch_DupGlobalElement";
                    }
                    else if (item is XmlSchemaAttribute)
                    {
                        if (qname.Namespace == "http://www.w3.org/XML/1998/namespace")
                        {
                            XmlSchemaObject obj3 = Preprocessor.GetBuildInSchema().Attributes[qname];
                            if (obj2 == obj3)
                            {
                                table.Insert(qname, item);
                                return true;
                            }
                            if (item == obj3)
                            {
                                return true;
                            }
                        }
                        res = "Sch_DupGlobalAttribute";
                    }
                    this.SendValidationEvent(new XmlSchemaException(res, qname.ToString()), XmlSeverityType.Error);
                    return false;
                }
                table.Add(qname, item);
            }
            return true;
        }

        private void ClearTables()
        {
            this.GlobalElements.Clear();
            this.GlobalAttributes.Clear();
            this.GlobalTypes.Clear();
            this.SubstitutionGroups.Clear();
            this.TypeExtensions.Clear();
        }

        public void Compile()
        {
            if (!this.isCompiled)
            {
                if (this.schemas.Count == 0)
                {
                    this.ClearTables();
                    this.cachedCompiledInfo = new SchemaInfo();
                    this.isCompiled = true;
                    this.compileAll = false;
                }
                else
                {
                    lock (this.InternalSyncObject)
                    {
                        if (!this.isCompiled)
                        {
                            Compiler compiler = new Compiler(this.nameTable, this.eventHandler, this.schemaForSchema, this.compilationSettings);
                            SchemaInfo schemaCompiledInfo = new SchemaInfo();
                            int index = 0;
                            if (!this.compileAll)
                            {
                                compiler.ImportAllCompiledSchemas(this);
                            }
                            try
                            {
                                XmlSchema buildInSchema = Preprocessor.GetBuildInSchema();
                                index = 0;
                                while (index < this.schemas.Count)
                                {
                                    XmlSchema byIndex = (XmlSchema) this.schemas.GetByIndex(index);
                                    Monitor.Enter(byIndex);
                                    if (!byIndex.IsPreprocessed)
                                    {
                                        this.SendValidationEvent(new XmlSchemaException("Sch_SchemaNotPreprocessed", string.Empty), XmlSeverityType.Error);
                                        this.isCompiled = false;
                                        goto Label_01BA;
                                    }
                                    if (byIndex.IsCompiledBySet)
                                    {
                                        if (!this.compileAll)
                                        {
                                            goto Label_00FD;
                                        }
                                        if (byIndex == buildInSchema)
                                        {
                                            compiler.Prepare(byIndex, false);
                                            goto Label_00FD;
                                        }
                                    }
                                    compiler.Prepare(byIndex, true);
                                Label_00FD:
                                    index++;
                                }
                                this.isCompiled = compiler.Execute(this, schemaCompiledInfo);
                                if (this.isCompiled)
                                {
                                    if (!this.compileAll)
                                    {
                                        schemaCompiledInfo.Add(this.cachedCompiledInfo, this.eventHandler);
                                    }
                                    this.compileAll = false;
                                    this.cachedCompiledInfo = schemaCompiledInfo;
                                }
                            }
                            finally
                            {
                                if (index == this.schemas.Count)
                                {
                                    index--;
                                }
                                for (int i = index; i >= 0; i--)
                                {
                                    XmlSchema schema3 = (XmlSchema) this.schemas.GetByIndex(i);
                                    if (schema3 == Preprocessor.GetBuildInSchema())
                                    {
                                        Monitor.Exit(schema3);
                                    }
                                    else
                                    {
                                        schema3.IsCompiledBySet = this.isCompiled;
                                        Monitor.Exit(schema3);
                                    }
                                }
                            }
                        }
                    Label_01BA:;
                    }
                }
            }
        }

        public bool Contains(string targetNamespace)
        {
            if (targetNamespace == null)
            {
                targetNamespace = string.Empty;
            }
            return (this.targetNamespaces[targetNamespace] != null);
        }

        public bool Contains(XmlSchema schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }
            return this.schemas.ContainsValue(schema);
        }

        internal void CopyFromCompiledSet(XmlSchemaSet otherSet)
        {
            SortedList sortedSchemas = otherSet.SortedSchemas;
            bool flag = this.schemas.Count == 0;
            ArrayList list2 = new ArrayList();
            SchemaInfo info = new SchemaInfo();
            for (int i = 0; i < sortedSchemas.Count; i++)
            {
                XmlSchema byIndex = (XmlSchema) sortedSchemas.GetByIndex(i);
                Uri baseUri = byIndex.BaseUri;
                if (this.schemas.ContainsKey(byIndex.SchemaId) || (((baseUri != null) && (baseUri.OriginalString.Length != 0)) && (this.schemaLocations[baseUri] != null)))
                {
                    list2.Add(byIndex);
                }
                else
                {
                    this.schemas.Add(byIndex.SchemaId, byIndex);
                    if ((baseUri != null) && (baseUri.OriginalString.Length != 0))
                    {
                        this.schemaLocations.Add(baseUri, byIndex);
                    }
                    string targetNamespace = this.GetTargetNamespace(byIndex);
                    if (this.targetNamespaces[targetNamespace] == null)
                    {
                        this.targetNamespaces.Add(targetNamespace, targetNamespace);
                    }
                }
            }
            this.VerifyTables();
            foreach (XmlSchemaElement element in otherSet.GlobalElements.Values)
            {
                if (!this.AddToTable(this.elements, element.QualifiedName, element))
                {
                    goto Label_026E;
                }
            }
            foreach (XmlSchemaAttribute attribute in otherSet.GlobalAttributes.Values)
            {
                if (!this.AddToTable(this.attributes, attribute.QualifiedName, attribute))
                {
                    goto Label_026E;
                }
            }
            foreach (XmlSchemaType type in otherSet.GlobalTypes.Values)
            {
                if (!this.AddToTable(this.schemaTypes, type.QualifiedName, type))
                {
                    goto Label_026E;
                }
            }
            this.ProcessNewSubstitutionGroups(otherSet.SubstitutionGroups, false);
            info.Add(this.cachedCompiledInfo, this.eventHandler);
            info.Add(otherSet.CompiledInfo, this.eventHandler);
            this.cachedCompiledInfo = info;
            if (flag)
            {
                this.isCompiled = true;
                this.compileAll = false;
            }
            return;
        Label_026E:
            foreach (XmlSchema schema2 in sortedSchemas.Values)
            {
                if (!list2.Contains(schema2))
                {
                    this.Remove(schema2, false);
                }
            }
            foreach (XmlSchemaElement element2 in otherSet.GlobalElements.Values)
            {
                if (!list2.Contains((XmlSchema) element2.Parent))
                {
                    this.elements.Remove(element2.QualifiedName);
                }
            }
            foreach (XmlSchemaAttribute attribute2 in otherSet.GlobalAttributes.Values)
            {
                if (!list2.Contains((XmlSchema) attribute2.Parent))
                {
                    this.attributes.Remove(attribute2.QualifiedName);
                }
            }
            foreach (XmlSchemaType type2 in otherSet.GlobalTypes.Values)
            {
                if (!list2.Contains((XmlSchema) type2.Parent))
                {
                    this.schemaTypes.Remove(type2.QualifiedName);
                }
            }
        }

        public void CopyTo(XmlSchema[] schemas, int index)
        {
            if (schemas == null)
            {
                throw new ArgumentNullException("schemas");
            }
            if ((index < 0) || (index > (schemas.Length - 1)))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            this.schemas.Values.CopyTo(schemas, index);
        }

        internal XmlSchema FindSchemaByNSAndUrl(Uri schemaUri, string ns, DictionaryEntry[] locationsTable)
        {
            if ((schemaUri != null) && (schemaUri.OriginalString.Length != 0))
            {
                XmlSchema originalSchema = null;
                if (locationsTable == null)
                {
                    originalSchema = (XmlSchema) this.schemaLocations[schemaUri];
                }
                else
                {
                    for (int i = 0; i < locationsTable.Length; i++)
                    {
                        if (schemaUri.Equals(locationsTable[i].Key))
                        {
                            originalSchema = (XmlSchema) locationsTable[i].Value;
                            break;
                        }
                    }
                }
                if (originalSchema == null)
                {
                    return originalSchema;
                }
                string str = (originalSchema.TargetNamespace == null) ? string.Empty : originalSchema.TargetNamespace;
                if (str == ns)
                {
                    return originalSchema;
                }
                if (str == string.Empty)
                {
                    ChameleonKey key = new ChameleonKey(ns, originalSchema);
                    return (XmlSchema) this.chameleonSchemas[key];
                }
            }
            return null;
        }

        internal System.Xml.Schema.ValidationEventHandler GetEventHandler()
        {
            return this.eventHandler;
        }

        internal System.Xml.XmlResolver GetResolver()
        {
            return this.readerSettings.GetXmlResolver();
        }

        internal bool GetSchemaByUri(Uri schemaUri, out XmlSchema schema)
        {
            schema = null;
            if ((schemaUri == null) || (schemaUri.OriginalString.Length == 0))
            {
                return false;
            }
            schema = (XmlSchema) this.schemaLocations[schemaUri];
            return (schema != null);
        }

        internal SchemaNames GetSchemaNames(XmlNameTable nt)
        {
            if (this.nameTable != nt)
            {
                return new SchemaNames(nt);
            }
            if (this.schemaNames == null)
            {
                this.schemaNames = new SchemaNames(this.nameTable);
            }
            return this.schemaNames;
        }

        internal string GetTargetNamespace(XmlSchema schema)
        {
            if (schema.TargetNamespace != null)
            {
                return schema.TargetNamespace;
            }
            return string.Empty;
        }

        private void InternalValidationCallback(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error)
            {
                throw e.Exception;
            }
        }

        internal bool IsSchemaLoaded(Uri schemaUri, string targetNamespace, out XmlSchema schema)
        {
            schema = null;
            if (targetNamespace == null)
            {
                targetNamespace = string.Empty;
            }
            if (!this.GetSchemaByUri(schemaUri, out schema))
            {
                return false;
            }
            if (!this.schemas.ContainsKey(schema.SchemaId) || ((targetNamespace.Length != 0) && (targetNamespace != schema.TargetNamespace)))
            {
                if (schema.TargetNamespace == null)
                {
                    XmlSchema schema2 = this.FindSchemaByNSAndUrl(schemaUri, targetNamespace, null);
                    if ((schema2 != null) && this.schemas.ContainsKey(schema2.SchemaId))
                    {
                        schema = schema2;
                    }
                    else
                    {
                        schema = this.Add(targetNamespace, schema);
                    }
                }
                else if ((targetNamespace.Length != 0) && (targetNamespace != schema.TargetNamespace))
                {
                    this.SendValidationEvent(new XmlSchemaException("Sch_MismatchTargetNamespaceEx", new string[] { targetNamespace, schema.TargetNamespace }), XmlSeverityType.Error);
                    schema = null;
                }
                else
                {
                    this.AddSchemaToSet(schema);
                }
            }
            return true;
        }

        internal XmlSchema ParseSchema(string targetNamespace, XmlReader reader)
        {
            XmlNameTable nameTable = reader.NameTable;
            SchemaNames schemaNames = this.GetSchemaNames(nameTable);
            System.Xml.Schema.Parser parser = new System.Xml.Schema.Parser(SchemaType.XSD, nameTable, schemaNames, this.eventHandler) {
                XmlResolver = this.readerSettings.GetXmlResolver()
            };
            try
            {
                parser.Parse(reader, targetNamespace);
            }
            catch (XmlSchemaException exception)
            {
                this.SendValidationEvent(exception, XmlSeverityType.Error);
                return null;
            }
            return parser.XmlSchema;
        }

        internal bool PreprocessSchema(ref XmlSchema schema, string targetNamespace)
        {
            Preprocessor preprocessor = new Preprocessor(this.nameTable, this.GetSchemaNames(this.nameTable), this.eventHandler, this.compilationSettings) {
                XmlResolver = this.readerSettings.GetXmlResolver(),
                ReaderSettings = this.readerSettings,
                SchemaLocations = this.schemaLocations,
                ChameleonSchemas = this.chameleonSchemas
            };
            bool flag = preprocessor.Execute(schema, targetNamespace, true);
            schema = preprocessor.RootSchema;
            return flag;
        }

        private void ProcessNewSubstitutionGroups(XmlSchemaObjectTable substitutionGroupsTable, bool resolve)
        {
            foreach (XmlSchemaSubstitutionGroup group in substitutionGroupsTable.Values)
            {
                if (resolve)
                {
                    this.ResolveSubstitutionGroup(group, substitutionGroupsTable);
                }
                XmlQualifiedName examplar = group.Examplar;
                XmlSchemaSubstitutionGroup group2 = (XmlSchemaSubstitutionGroup) this.substitutionGroups[examplar];
                if (group2 != null)
                {
                    for (int i = 0; i < group.Members.Count; i++)
                    {
                        if (!group2.Members.Contains(group.Members[i]))
                        {
                            group2.Members.Add(group.Members[i]);
                        }
                    }
                }
                else
                {
                    this.AddToTable(this.substitutionGroups, examplar, group);
                }
            }
        }

        public XmlSchema Remove(XmlSchema schema)
        {
            return this.Remove(schema, true);
        }

        internal XmlSchema Remove(XmlSchema schema, bool forceCompile)
        {
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }
            lock (this.InternalSyncObject)
            {
                if (this.schemas.ContainsKey(schema.SchemaId))
                {
                    if (forceCompile)
                    {
                        this.RemoveSchemaFromGlobalTables(schema);
                        this.RemoveSchemaFromCaches(schema);
                    }
                    this.schemas.Remove(schema.SchemaId);
                    if (schema.BaseUri != null)
                    {
                        this.schemaLocations.Remove(schema.BaseUri);
                    }
                    string targetNamespace = this.GetTargetNamespace(schema);
                    if (this.Schemas(targetNamespace).Count == 0)
                    {
                        this.targetNamespaces.Remove(targetNamespace);
                    }
                    if (forceCompile)
                    {
                        this.isCompiled = false;
                        this.compileAll = true;
                    }
                    return schema;
                }
            }
            return null;
        }

        public bool RemoveRecursive(XmlSchema schemaToRemove)
        {
            if (schemaToRemove == null)
            {
                throw new ArgumentNullException("schemaToRemove");
            }
            if (this.schemas.ContainsKey(schemaToRemove.SchemaId))
            {
                lock (this.InternalSyncObject)
                {
                    if (this.schemas.ContainsKey(schemaToRemove.SchemaId))
                    {
                        XmlSchema byIndex;
                        Hashtable hashtable = new Hashtable();
                        hashtable.Add(this.GetTargetNamespace(schemaToRemove), schemaToRemove);
                        for (int i = 0; i < schemaToRemove.ImportedNamespaces.Count; i++)
                        {
                            string key = (string) schemaToRemove.ImportedNamespaces[i];
                            if (hashtable[key] == null)
                            {
                                hashtable.Add(key, key);
                            }
                        }
                        ArrayList list = new ArrayList();
                        for (int j = 0; j < this.schemas.Count; j++)
                        {
                            byIndex = (XmlSchema) this.schemas.GetByIndex(j);
                            if ((byIndex != schemaToRemove) && !schemaToRemove.ImportedSchemas.Contains(byIndex))
                            {
                                list.Add(byIndex);
                            }
                        }
                        byIndex = null;
                        for (int k = 0; k < list.Count; k++)
                        {
                            byIndex = (XmlSchema) list[k];
                            if (byIndex.ImportedNamespaces.Count > 0)
                            {
                                foreach (string str2 in hashtable.Keys)
                                {
                                    if (byIndex.ImportedNamespaces.Contains(str2))
                                    {
                                        this.SendValidationEvent(new XmlSchemaException("Sch_SchemaNotRemoved", string.Empty), XmlSeverityType.Warning);
                                        return false;
                                    }
                                }
                            }
                        }
                        this.Remove(schemaToRemove, true);
                        for (int m = 0; m < schemaToRemove.ImportedSchemas.Count; m++)
                        {
                            XmlSchema schema = (XmlSchema) schemaToRemove.ImportedSchemas[m];
                            this.Remove(schema, true);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private void RemoveSchemaFromCaches(XmlSchema schema)
        {
            List<XmlSchema> extList = new List<XmlSchema>();
            schema.GetExternalSchemasList(extList, schema);
            for (int i = 0; i < extList.Count; i++)
            {
                if ((extList[i].BaseUri != null) && (extList[i].BaseUri.OriginalString.Length != 0))
                {
                    this.schemaLocations.Remove(extList[i].BaseUri);
                }
                ICollection keys = this.chameleonSchemas.Keys;
                ArrayList list2 = new ArrayList();
                foreach (ChameleonKey key in keys)
                {
                    if (key.chameleonLocation.Equals(extList[i].BaseUri) && ((key.originalSchema == null) || object.ReferenceEquals(key.originalSchema, extList[i])))
                    {
                        list2.Add(key);
                    }
                }
                for (int j = 0; j < list2.Count; j++)
                {
                    this.chameleonSchemas.Remove(list2[j]);
                }
            }
        }

        private void RemoveSchemaFromGlobalTables(XmlSchema schema)
        {
            if (this.schemas.Count != 0)
            {
                this.VerifyTables();
                foreach (XmlSchemaElement element in schema.Elements.Values)
                {
                    XmlSchemaElement element2 = (XmlSchemaElement) this.elements[element.QualifiedName];
                    if (element2 == element)
                    {
                        this.elements.Remove(element.QualifiedName);
                    }
                }
                foreach (XmlSchemaAttribute attribute in schema.Attributes.Values)
                {
                    XmlSchemaAttribute attribute2 = (XmlSchemaAttribute) this.attributes[attribute.QualifiedName];
                    if (attribute2 == attribute)
                    {
                        this.attributes.Remove(attribute.QualifiedName);
                    }
                }
                foreach (XmlSchemaType type in schema.SchemaTypes.Values)
                {
                    XmlSchemaType type2 = (XmlSchemaType) this.schemaTypes[type.QualifiedName];
                    if (type2 == type)
                    {
                        this.schemaTypes.Remove(type.QualifiedName);
                    }
                }
            }
        }

        public XmlSchema Reprocess(XmlSchema schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }
            if (!this.schemas.ContainsKey(schema.SchemaId))
            {
                throw new ArgumentException(Res.GetString("Sch_SchemaDoesNotExist"), "schema");
            }
            XmlSchema schema2 = schema;
            lock (this.InternalSyncObject)
            {
                this.RemoveSchemaFromGlobalTables(schema);
                this.RemoveSchemaFromCaches(schema);
                if (schema.BaseUri != null)
                {
                    this.schemaLocations.Remove(schema.BaseUri);
                }
                string targetNamespace = this.GetTargetNamespace(schema);
                if (this.Schemas(targetNamespace).Count == 0)
                {
                    this.targetNamespaces.Remove(targetNamespace);
                }
                this.isCompiled = false;
                this.compileAll = true;
                if ((schema.ErrorCount == 0) && this.PreprocessSchema(ref schema, schema.TargetNamespace))
                {
                    if (this.targetNamespaces[targetNamespace] == null)
                    {
                        this.targetNamespaces.Add(targetNamespace, targetNamespace);
                    }
                    if (((this.schemaForSchema == null) && (targetNamespace == "http://www.w3.org/2001/XMLSchema")) && (schema.SchemaTypes[DatatypeImplementation.QnAnyType] != null))
                    {
                        this.schemaForSchema = schema;
                    }
                    for (int i = 0; i < schema.ImportedSchemas.Count; i++)
                    {
                        XmlSchema schema3 = (XmlSchema) schema.ImportedSchemas[i];
                        if (!this.schemas.ContainsKey(schema3.SchemaId))
                        {
                            this.schemas.Add(schema3.SchemaId, schema3);
                        }
                        targetNamespace = this.GetTargetNamespace(schema3);
                        if (this.targetNamespaces[targetNamespace] == null)
                        {
                            this.targetNamespaces.Add(targetNamespace, targetNamespace);
                        }
                        if (((this.schemaForSchema == null) && (targetNamespace == "http://www.w3.org/2001/XMLSchema")) && (schema.SchemaTypes[DatatypeImplementation.QnAnyType] != null))
                        {
                            this.schemaForSchema = schema;
                        }
                    }
                    return schema;
                }
                return schema2;
            }
        }

        private void ResolveSubstitutionGroup(XmlSchemaSubstitutionGroup substitutionGroup, XmlSchemaObjectTable substTable)
        {
            List<XmlSchemaElement> list = null;
            XmlSchemaElement item = (XmlSchemaElement) this.elements[substitutionGroup.Examplar];
            if (!substitutionGroup.Members.Contains(item))
            {
                for (int i = 0; i < substitutionGroup.Members.Count; i++)
                {
                    XmlSchemaElement element2 = (XmlSchemaElement) substitutionGroup.Members[i];
                    XmlSchemaSubstitutionGroup group = (XmlSchemaSubstitutionGroup) substTable[element2.QualifiedName];
                    if (group != null)
                    {
                        this.ResolveSubstitutionGroup(group, substTable);
                        for (int j = 0; j < group.Members.Count; j++)
                        {
                            XmlSchemaElement element3 = (XmlSchemaElement) group.Members[j];
                            if (element3 != element2)
                            {
                                if (list == null)
                                {
                                    list = new List<XmlSchemaElement>();
                                }
                                list.Add(element3);
                            }
                        }
                    }
                }
                if (list != null)
                {
                    for (int k = 0; k < list.Count; k++)
                    {
                        substitutionGroup.Members.Add(list[k]);
                    }
                }
                substitutionGroup.Members.Add(item);
            }
        }

        public ICollection Schemas()
        {
            return this.schemas.Values;
        }

        public ICollection Schemas(string targetNamespace)
        {
            ArrayList list = new ArrayList();
            if (targetNamespace == null)
            {
                targetNamespace = string.Empty;
            }
            for (int i = 0; i < this.schemas.Count; i++)
            {
                XmlSchema byIndex = (XmlSchema) this.schemas.GetByIndex(i);
                if (this.GetTargetNamespace(byIndex) == targetNamespace)
                {
                    list.Add(byIndex);
                }
            }
            return list;
        }

        private void SendValidationEvent(XmlSchemaException e, XmlSeverityType severity)
        {
            if (this.eventHandler == null)
            {
                throw e;
            }
            this.eventHandler(this, new ValidationEventArgs(e, severity));
        }

        private void SetDtdProcessing(XmlReader reader)
        {
            if (reader.Settings != null)
            {
                this.readerSettings.DtdProcessing = reader.Settings.DtdProcessing;
            }
            else
            {
                XmlTextReader reader2 = reader as XmlTextReader;
                if (reader2 != null)
                {
                    this.readerSettings.DtdProcessing = reader2.DtdProcessing;
                }
            }
        }

        private void VerifyTables()
        {
            if (this.elements == null)
            {
                this.elements = new XmlSchemaObjectTable();
            }
            if (this.attributes == null)
            {
                this.attributes = new XmlSchemaObjectTable();
            }
            if (this.schemaTypes == null)
            {
                this.schemaTypes = new XmlSchemaObjectTable();
            }
            if (this.substitutionGroups == null)
            {
                this.substitutionGroups = new XmlSchemaObjectTable();
            }
        }

        public XmlSchemaCompilationSettings CompilationSettings
        {
            get
            {
                return this.compilationSettings;
            }
            set
            {
                this.compilationSettings = value;
            }
        }

        internal bool CompileAll
        {
            get
            {
                return this.compileAll;
            }
        }

        internal SchemaInfo CompiledInfo
        {
            get
            {
                return this.cachedCompiledInfo;
            }
        }

        public int Count
        {
            get
            {
                return this.schemas.Count;
            }
        }

        public XmlSchemaObjectTable GlobalAttributes
        {
            get
            {
                if (this.attributes == null)
                {
                    this.attributes = new XmlSchemaObjectTable();
                }
                return this.attributes;
            }
        }

        public XmlSchemaObjectTable GlobalElements
        {
            get
            {
                if (this.elements == null)
                {
                    this.elements = new XmlSchemaObjectTable();
                }
                return this.elements;
            }
        }

        public XmlSchemaObjectTable GlobalTypes
        {
            get
            {
                if (this.schemaTypes == null)
                {
                    this.schemaTypes = new XmlSchemaObjectTable();
                }
                return this.schemaTypes;
            }
        }

        internal object InternalSyncObject
        {
            get
            {
                if (this.internalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange<object>(ref this.internalSyncObject, obj2, null);
                }
                return this.internalSyncObject;
            }
        }

        public bool IsCompiled
        {
            get
            {
                return this.isCompiled;
            }
        }

        public XmlNameTable NameTable
        {
            get
            {
                return this.nameTable;
            }
        }

        internal XmlReaderSettings ReaderSettings
        {
            get
            {
                return this.readerSettings;
            }
        }

        internal Hashtable SchemaLocations
        {
            get
            {
                return this.schemaLocations;
            }
        }

        internal SortedList SortedSchemas
        {
            get
            {
                return this.schemas;
            }
        }

        internal XmlSchemaObjectTable SubstitutionGroups
        {
            get
            {
                if (this.substitutionGroups == null)
                {
                    this.substitutionGroups = new XmlSchemaObjectTable();
                }
                return this.substitutionGroups;
            }
        }

        internal XmlSchemaObjectTable TypeExtensions
        {
            get
            {
                if (this.typeExtensions == null)
                {
                    this.typeExtensions = new XmlSchemaObjectTable();
                }
                return this.typeExtensions;
            }
        }

        public System.Xml.XmlResolver XmlResolver
        {
            set
            {
                this.readerSettings.XmlResolver = value;
            }
        }
    }
}

