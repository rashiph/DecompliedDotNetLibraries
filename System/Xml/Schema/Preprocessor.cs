namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Xml;

    internal sealed class Preprocessor : BaseProcessor
    {
        private XmlSchemaForm attributeFormDefault;
        private XmlSchemaDerivationMethod blockDefault;
        private static XmlSchema builtInSchemaForXmlNS;
        private Hashtable chameleonSchemas;
        private const XmlSchemaDerivationMethod complexTypeBlockAllowed = (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension);
        private const XmlSchemaDerivationMethod complexTypeFinalAllowed = (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension);
        private XmlSchema currentSchema;
        private const XmlSchemaDerivationMethod elementBlockAllowed = (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Substitution);
        private const XmlSchemaDerivationMethod elementFinalAllowed = (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension);
        private XmlSchemaForm elementFormDefault;
        private XmlSchemaDerivationMethod finalDefault;
        private SortedList lockList;
        private string NsXsi;
        private Hashtable processedExternals;
        private XmlReaderSettings readerSettings;
        private ArrayList redefinedList;
        private Hashtable referenceNamespaces;
        private XmlSchema rootSchema;
        private XmlSchema rootSchemaForRedefine;
        private const XmlSchemaDerivationMethod schemaBlockDefaultAllowed = (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Substitution);
        private const XmlSchemaDerivationMethod schemaFinalDefaultAllowed = (XmlSchemaDerivationMethod.Union | XmlSchemaDerivationMethod.List | XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension);
        private Hashtable schemaLocations;
        private const XmlSchemaDerivationMethod simpleTypeFinalAllowed = (XmlSchemaDerivationMethod.Union | XmlSchemaDerivationMethod.List | XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension);
        private string targetNamespace;
        private string Xmlns;
        private System.Xml.XmlResolver xmlResolver;

        public Preprocessor(XmlNameTable nameTable, SchemaNames schemaNames, ValidationEventHandler eventHandler) : this(nameTable, schemaNames, eventHandler, new XmlSchemaCompilationSettings())
        {
        }

        public Preprocessor(XmlNameTable nameTable, SchemaNames schemaNames, ValidationEventHandler eventHandler, XmlSchemaCompilationSettings compilationSettings) : base(nameTable, schemaNames, eventHandler, compilationSettings)
        {
            this.referenceNamespaces = new Hashtable();
            this.processedExternals = new Hashtable();
            this.lockList = new SortedList();
        }

        private void BuildRefNamespaces(XmlSchema schema)
        {
            this.referenceNamespaces.Clear();
            this.referenceNamespaces.Add("http://www.w3.org/2001/XMLSchema", "http://www.w3.org/2001/XMLSchema");
            for (int i = 0; i < schema.Includes.Count; i++)
            {
                XmlSchemaExternal external = (XmlSchemaExternal) schema.Includes[i];
                if (external is XmlSchemaImport)
                {
                    XmlSchemaImport import = external as XmlSchemaImport;
                    string key = import.Namespace;
                    if (key == null)
                    {
                        key = string.Empty;
                    }
                    if (this.referenceNamespaces[key] == null)
                    {
                        this.referenceNamespaces.Add(key, key);
                    }
                }
            }
            string targetNamespace = schema.TargetNamespace;
            if (targetNamespace == null)
            {
                targetNamespace = string.Empty;
            }
            if (this.referenceNamespaces[targetNamespace] == null)
            {
                this.referenceNamespaces.Add(targetNamespace, targetNamespace);
            }
        }

        private void BuildSchemaList(XmlSchema schema)
        {
            if (!this.lockList.Contains(schema.SchemaId))
            {
                this.lockList.Add(schema.SchemaId, schema);
                for (int i = 0; i < schema.Includes.Count; i++)
                {
                    XmlSchemaExternal external = (XmlSchemaExternal) schema.Includes[i];
                    if (external.Schema != null)
                    {
                        this.BuildSchemaList(external.Schema);
                    }
                }
            }
        }

        private void CheckRefinedAttributeGroup(XmlSchemaAttributeGroup attributeGroup)
        {
            int num = 0;
            for (int i = 0; i < attributeGroup.Attributes.Count; i++)
            {
                XmlSchemaAttributeGroupRef ref2 = attributeGroup.Attributes[i] as XmlSchemaAttributeGroupRef;
                if ((ref2 != null) && (ref2.RefName == attributeGroup.QualifiedName))
                {
                    num++;
                }
            }
            if (num > 1)
            {
                base.SendValidationEvent("Sch_MultipleAttrGroupSelfRef", attributeGroup);
            }
            attributeGroup.SelfReferenceCount = num;
        }

        private void CheckRefinedComplexType(XmlSchemaComplexType ctype)
        {
            if (ctype.ContentModel != null)
            {
                XmlQualifiedName baseTypeName;
                if (ctype.ContentModel is XmlSchemaComplexContent)
                {
                    XmlSchemaComplexContent contentModel = (XmlSchemaComplexContent) ctype.ContentModel;
                    if (contentModel.Content is XmlSchemaComplexContentRestriction)
                    {
                        baseTypeName = ((XmlSchemaComplexContentRestriction) contentModel.Content).BaseTypeName;
                    }
                    else
                    {
                        baseTypeName = ((XmlSchemaComplexContentExtension) contentModel.Content).BaseTypeName;
                    }
                }
                else
                {
                    XmlSchemaSimpleContent content2 = (XmlSchemaSimpleContent) ctype.ContentModel;
                    if (content2.Content is XmlSchemaSimpleContentRestriction)
                    {
                        baseTypeName = ((XmlSchemaSimpleContentRestriction) content2.Content).BaseTypeName;
                    }
                    else
                    {
                        baseTypeName = ((XmlSchemaSimpleContentExtension) content2.Content).BaseTypeName;
                    }
                }
                if (baseTypeName == ctype.QualifiedName)
                {
                    return;
                }
            }
            base.SendValidationEvent("Sch_InvalidTypeRedefine", ctype);
        }

        private void CheckRefinedGroup(XmlSchemaGroup group)
        {
            int num = 0;
            if (group.Particle != null)
            {
                num = this.CountGroupSelfReference(group.Particle.Items, group.QualifiedName, group.Redefined);
            }
            if (num > 1)
            {
                base.SendValidationEvent("Sch_MultipleGroupSelfRef", group);
            }
            group.SelfReferenceCount = num;
        }

        private void CheckRefinedSimpleType(XmlSchemaSimpleType stype)
        {
            if ((stype.Content != null) && (stype.Content is XmlSchemaSimpleTypeRestriction))
            {
                XmlSchemaSimpleTypeRestriction content = (XmlSchemaSimpleTypeRestriction) stype.Content;
                if (content.BaseTypeName == stype.QualifiedName)
                {
                    return;
                }
            }
            base.SendValidationEvent("Sch_InvalidTypeRedefine", stype);
        }

        private void Cleanup(XmlSchema schema)
        {
            if (schema != GetBuildInSchema())
            {
                schema.Attributes.Clear();
                schema.AttributeGroups.Clear();
                schema.SchemaTypes.Clear();
                schema.Elements.Clear();
                schema.Groups.Clear();
                schema.Notations.Clear();
                schema.Ids.Clear();
                schema.IdentityConstraints.Clear();
                schema.IsRedefined = false;
                schema.IsCompiledBySet = false;
            }
        }

        private void CleanupRedefine(XmlSchemaExternal include)
        {
            XmlSchemaRedefine redefine = include as XmlSchemaRedefine;
            redefine.AttributeGroups.Clear();
            redefine.Groups.Clear();
            redefine.SchemaTypes.Clear();
        }

        private void CopyIncludedComponents(XmlSchema includedSchema, XmlSchema schema)
        {
            foreach (XmlSchemaElement element in includedSchema.Elements.Values)
            {
                base.AddToTable(schema.Elements, element.QualifiedName, element);
            }
            foreach (XmlSchemaAttribute attribute in includedSchema.Attributes.Values)
            {
                base.AddToTable(schema.Attributes, attribute.QualifiedName, attribute);
            }
            foreach (XmlSchemaGroup group in includedSchema.Groups.Values)
            {
                base.AddToTable(schema.Groups, group.QualifiedName, group);
            }
            foreach (XmlSchemaAttributeGroup group2 in includedSchema.AttributeGroups.Values)
            {
                base.AddToTable(schema.AttributeGroups, group2.QualifiedName, group2);
            }
            foreach (XmlSchemaType type in includedSchema.SchemaTypes.Values)
            {
                base.AddToTable(schema.SchemaTypes, type.QualifiedName, type);
            }
            foreach (XmlSchemaNotation notation in includedSchema.Notations.Values)
            {
                base.AddToTable(schema.Notations, notation.QualifiedName, notation);
            }
        }

        private int CountGroupSelfReference(XmlSchemaObjectCollection items, XmlQualifiedName name, XmlSchemaGroup redefined)
        {
            int num = 0;
            for (int i = 0; i < items.Count; i++)
            {
                XmlSchemaGroupRef source = items[i] as XmlSchemaGroupRef;
                if (source != null)
                {
                    if (source.RefName == name)
                    {
                        source.Redefined = redefined;
                        if ((source.MinOccurs != 1M) || (source.MaxOccurs != 1M))
                        {
                            base.SendValidationEvent("Sch_MinMaxGroupRedefine", source);
                        }
                        num++;
                    }
                }
                else if (items[i] is XmlSchemaGroupBase)
                {
                    num += this.CountGroupSelfReference(((XmlSchemaGroupBase) items[i]).Items, name, redefined);
                }
                if (num > 1)
                {
                    return num;
                }
            }
            return num;
        }

        public bool Execute(XmlSchema schema, string targetNamespace, bool loadExternals)
        {
            XmlSchema byIndex;
            this.rootSchema = schema;
            this.Xmlns = base.NameTable.Add("xmlns");
            this.NsXsi = base.NameTable.Add("http://www.w3.org/2001/XMLSchema-instance");
            this.rootSchema.ImportedSchemas.Clear();
            this.rootSchema.ImportedNamespaces.Clear();
            if ((this.rootSchema.BaseUri != null) && (this.schemaLocations[this.rootSchema.BaseUri] == null))
            {
                this.schemaLocations.Add(this.rootSchema.BaseUri, this.rootSchema);
            }
            if (this.rootSchema.TargetNamespace != null)
            {
                if (targetNamespace == null)
                {
                    targetNamespace = this.rootSchema.TargetNamespace;
                }
                else if (targetNamespace != this.rootSchema.TargetNamespace)
                {
                    base.SendValidationEvent("Sch_MismatchTargetNamespaceEx", targetNamespace, this.rootSchema.TargetNamespace, this.rootSchema);
                }
            }
            else if ((targetNamespace != null) && (targetNamespace.Length != 0))
            {
                this.rootSchema = this.GetChameleonSchema(targetNamespace, this.rootSchema);
            }
            if (loadExternals && (this.xmlResolver != null))
            {
                this.LoadExternals(this.rootSchema);
            }
            this.BuildSchemaList(this.rootSchema);
            int index = 0;
            try
            {
                index = 0;
                while (index < this.lockList.Count)
                {
                    byIndex = (XmlSchema) this.lockList.GetByIndex(index);
                    Monitor.Enter(byIndex);
                    byIndex.IsProcessing = false;
                    index++;
                }
                this.rootSchemaForRedefine = this.rootSchema;
                this.Preprocess(this.rootSchema, targetNamespace, this.rootSchema.ImportedSchemas);
                if (this.redefinedList != null)
                {
                    for (int i = 0; i < this.redefinedList.Count; i++)
                    {
                        this.PreprocessRedefine((RedefineEntry) this.redefinedList[i]);
                    }
                }
            }
            finally
            {
                if (index == this.lockList.Count)
                {
                    index--;
                }
                while (index >= 0)
                {
                    byIndex = (XmlSchema) this.lockList.GetByIndex(index);
                    byIndex.IsProcessing = false;
                    if (byIndex == GetBuildInSchema())
                    {
                        Monitor.Exit(byIndex);
                    }
                    else
                    {
                        byIndex.IsCompiledBySet = false;
                        byIndex.IsPreprocessed = !base.HasErrors;
                        Monitor.Exit(byIndex);
                    }
                    index--;
                }
            }
            this.rootSchema.IsPreprocessed = !base.HasErrors;
            return !base.HasErrors;
        }

        internal static XmlSchema GetBuildInSchema()
        {
            if (builtInSchemaForXmlNS == null)
            {
                XmlSchema schema = new XmlSchema {
                    TargetNamespace = "http://www.w3.org/XML/1998/namespace"
                };
                schema.Namespaces.Add("xml", "http://www.w3.org/XML/1998/namespace");
                XmlSchemaAttribute item = new XmlSchemaAttribute {
                    Name = "lang",
                    SchemaTypeName = new XmlQualifiedName("language", "http://www.w3.org/2001/XMLSchema")
                };
                schema.Items.Add(item);
                XmlSchemaAttribute attribute2 = new XmlSchemaAttribute {
                    Name = "base",
                    SchemaTypeName = new XmlQualifiedName("anyURI", "http://www.w3.org/2001/XMLSchema")
                };
                schema.Items.Add(attribute2);
                XmlSchemaAttribute attribute3 = new XmlSchemaAttribute {
                    Name = "space"
                };
                XmlSchemaSimpleType type = new XmlSchemaSimpleType();
                XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction {
                    BaseTypeName = new XmlQualifiedName("NCName", "http://www.w3.org/2001/XMLSchema")
                };
                XmlSchemaEnumerationFacet facet = new XmlSchemaEnumerationFacet {
                    Value = "default"
                };
                restriction.Facets.Add(facet);
                XmlSchemaEnumerationFacet facet2 = new XmlSchemaEnumerationFacet {
                    Value = "preserve"
                };
                restriction.Facets.Add(facet2);
                type.Content = restriction;
                attribute3.SchemaType = type;
                attribute3.DefaultValue = "preserve";
                schema.Items.Add(attribute3);
                XmlSchemaAttributeGroup group = new XmlSchemaAttributeGroup {
                    Name = "specialAttrs"
                };
                XmlSchemaAttribute attribute4 = new XmlSchemaAttribute {
                    RefName = new XmlQualifiedName("lang", "http://www.w3.org/XML/1998/namespace")
                };
                group.Attributes.Add(attribute4);
                XmlSchemaAttribute attribute5 = new XmlSchemaAttribute {
                    RefName = new XmlQualifiedName("space", "http://www.w3.org/XML/1998/namespace")
                };
                group.Attributes.Add(attribute5);
                XmlSchemaAttribute attribute6 = new XmlSchemaAttribute {
                    RefName = new XmlQualifiedName("base", "http://www.w3.org/XML/1998/namespace")
                };
                group.Attributes.Add(attribute6);
                schema.Items.Add(group);
                schema.IsPreprocessed = true;
                schema.CompileSchemaInSet(new NameTable(), null, null);
                Interlocked.CompareExchange<XmlSchema>(ref builtInSchemaForXmlNS, schema, null);
            }
            return builtInSchemaForXmlNS;
        }

        private XmlSchema GetChameleonSchema(string targetNamespace, XmlSchema schema)
        {
            ChameleonKey key = new ChameleonKey(targetNamespace, schema);
            XmlSchema schema2 = (XmlSchema) this.chameleonSchemas[key];
            if (schema2 == null)
            {
                schema2 = schema.DeepClone();
                schema2.IsChameleon = true;
                schema2.TargetNamespace = targetNamespace;
                this.chameleonSchemas.Add(key, schema2);
                schema2.SourceUri = schema.SourceUri;
                schema.IsProcessing = false;
            }
            return schema2;
        }

        private void GetIncludedSet(XmlSchema schema, ArrayList includesList)
        {
            if (!includesList.Contains(schema))
            {
                includesList.Add(schema);
                for (int i = 0; i < schema.Includes.Count; i++)
                {
                    XmlSchemaExternal external = (XmlSchemaExternal) schema.Includes[i];
                    if (((external.Compositor == System.Xml.Schema.Compositor.Include) || (external.Compositor == System.Xml.Schema.Compositor.Redefine)) && (external.Schema != null))
                    {
                        this.GetIncludedSet(external.Schema, includesList);
                    }
                }
            }
        }

        internal static XmlSchema GetParentSchema(XmlSchemaObject currentSchemaObject)
        {
            XmlSchema schema = null;
            while ((schema == null) && (currentSchemaObject != null))
            {
                currentSchemaObject = currentSchemaObject.Parent;
                schema = currentSchemaObject as XmlSchema;
            }
            return schema;
        }

        private object GetSchemaEntity(Uri ruri)
        {
            return this.xmlResolver.GetEntity(ruri, null, null);
        }

        private void LoadExternals(XmlSchema schema)
        {
            if (!schema.IsProcessing)
            {
                schema.IsProcessing = true;
                for (int i = 0; i < schema.Includes.Count; i++)
                {
                    Uri key = null;
                    XmlSchemaExternal source = (XmlSchemaExternal) schema.Includes[i];
                    XmlSchema xmlSchema = source.Schema;
                    if (xmlSchema != null)
                    {
                        key = xmlSchema.BaseUri;
                        if ((key != null) && (this.schemaLocations[key] == null))
                        {
                            this.schemaLocations.Add(key, xmlSchema);
                        }
                        this.LoadExternals(xmlSchema);
                    }
                    else
                    {
                        string schemaLocation = source.SchemaLocation;
                        Uri ruri = null;
                        Exception innerException = null;
                        if (schemaLocation != null)
                        {
                            try
                            {
                                ruri = this.ResolveSchemaLocationUri(schema, schemaLocation);
                            }
                            catch (Exception exception2)
                            {
                                ruri = null;
                                innerException = exception2;
                            }
                        }
                        if (source.Compositor == System.Xml.Schema.Compositor.Import)
                        {
                            XmlSchemaImport import = source as XmlSchemaImport;
                            string item = (import.Namespace != null) ? import.Namespace : string.Empty;
                            if (!schema.ImportedNamespaces.Contains(item))
                            {
                                schema.ImportedNamespaces.Add(item);
                            }
                            if ((item == "http://www.w3.org/XML/1998/namespace") && (ruri == null))
                            {
                                source.Schema = GetBuildInSchema();
                                goto Label_0390;
                            }
                        }
                        if (ruri == null)
                        {
                            if (schemaLocation != null)
                            {
                                base.SendValidationEvent(new XmlSchemaException("Sch_InvalidIncludeLocation", null, innerException, source.SourceUri, source.LineNumber, source.LinePosition, source), XmlSeverityType.Warning);
                            }
                        }
                        else if (this.schemaLocations[ruri] == null)
                        {
                            object schemaEntity = null;
                            try
                            {
                                schemaEntity = this.GetSchemaEntity(ruri);
                            }
                            catch (Exception exception3)
                            {
                                innerException = exception3;
                                schemaEntity = null;
                            }
                            if (schemaEntity != null)
                            {
                                source.BaseUri = ruri;
                                Type c = schemaEntity.GetType();
                                if (typeof(XmlSchema).IsAssignableFrom(c))
                                {
                                    source.Schema = (XmlSchema) schemaEntity;
                                    this.schemaLocations.Add(ruri, source.Schema);
                                    this.LoadExternals(source.Schema);
                                    goto Label_0390;
                                }
                                XmlReader reader = null;
                                if (c.IsSubclassOf(typeof(Stream)))
                                {
                                    this.readerSettings.CloseInput = true;
                                    this.readerSettings.XmlResolver = this.xmlResolver;
                                    reader = XmlReader.Create((Stream) schemaEntity, this.readerSettings, ruri.ToString());
                                }
                                else if (c.IsSubclassOf(typeof(XmlReader)))
                                {
                                    reader = (XmlReader) schemaEntity;
                                }
                                else if (c.IsSubclassOf(typeof(TextReader)))
                                {
                                    this.readerSettings.CloseInput = true;
                                    this.readerSettings.XmlResolver = this.xmlResolver;
                                    reader = XmlReader.Create((TextReader) schemaEntity, this.readerSettings, ruri.ToString());
                                }
                                if (reader == null)
                                {
                                    base.SendValidationEvent("Sch_InvalidIncludeLocation", source, XmlSeverityType.Warning);
                                    goto Label_0390;
                                }
                                try
                                {
                                    try
                                    {
                                        System.Xml.Schema.Parser parser = new System.Xml.Schema.Parser(SchemaType.XSD, base.NameTable, base.SchemaNames, base.EventHandler);
                                        parser.Parse(reader, null);
                                        while (reader.Read())
                                        {
                                        }
                                        xmlSchema = parser.XmlSchema;
                                        source.Schema = xmlSchema;
                                        this.schemaLocations.Add(ruri, xmlSchema);
                                        this.LoadExternals(xmlSchema);
                                    }
                                    catch (XmlSchemaException exception4)
                                    {
                                        base.SendValidationEvent("Sch_CannotLoadSchemaLocation", schemaLocation, exception4.Message, exception4.SourceUri, exception4.LineNumber, exception4.LinePosition);
                                    }
                                    catch (Exception exception5)
                                    {
                                        base.SendValidationEvent(new XmlSchemaException("Sch_InvalidIncludeLocation", null, exception5, source.SourceUri, source.LineNumber, source.LinePosition, source), XmlSeverityType.Warning);
                                    }
                                    goto Label_0390;
                                }
                                finally
                                {
                                    reader.Close();
                                }
                            }
                            base.SendValidationEvent(new XmlSchemaException("Sch_InvalidIncludeLocation", null, innerException, source.SourceUri, source.LineNumber, source.LinePosition, source), XmlSeverityType.Warning);
                        }
                        else
                        {
                            source.Schema = (XmlSchema) this.schemaLocations[ruri];
                        }
                    Label_0390:;
                    }
                }
            }
        }

        private void ParseUri(string uri, string code, XmlSchemaObject sourceSchemaObject)
        {
            try
            {
                XmlConvert.ToUri(uri);
            }
            catch (FormatException exception)
            {
                base.SendValidationEvent(code, new string[] { uri }, exception, sourceSchemaObject);
            }
        }

        private void Preprocess(XmlSchema schema, string targetNamespace, ArrayList imports)
        {
            XmlSchema rootSchemaForRedefine = null;
            if (!schema.IsProcessing)
            {
                schema.IsProcessing = true;
                string array = schema.TargetNamespace;
                if (array != null)
                {
                    schema.TargetNamespace = array = base.NameTable.Add(array);
                    if (array.Length == 0)
                    {
                        base.SendValidationEvent("Sch_InvalidTargetNamespaceAttribute", schema);
                    }
                    else
                    {
                        this.ParseUri(array, "Sch_InvalidNamespace", schema);
                    }
                }
                if (schema.Version != null)
                {
                    object obj2;
                    XmlSchemaDatatype datatype = DatatypeImplementation.GetSimpleTypeFromTypeCode(XmlTypeCode.Token).Datatype;
                    Exception innerException = datatype.TryParseValue(schema.Version, null, null, out obj2);
                    if (innerException != null)
                    {
                        base.SendValidationEvent("Sch_AttributeValueDataTypeDetailed", new string[] { "version", schema.Version, datatype.TypeCodeString, innerException.Message }, innerException, schema);
                    }
                    else
                    {
                        schema.Version = (string) obj2;
                    }
                }
                this.Cleanup(schema);
                for (int i = 0; i < schema.Includes.Count; i++)
                {
                    string str3;
                    XmlSchemaExternal child = (XmlSchemaExternal) schema.Includes[i];
                    XmlSchema chameleonSchema = child.Schema;
                    this.SetParent(child, schema);
                    this.PreprocessAnnotation(child);
                    string schemaLocation = child.SchemaLocation;
                    if (schemaLocation != null)
                    {
                        this.ParseUri(schemaLocation, "Sch_InvalidSchemaLocation", child);
                    }
                    else if (((child.Compositor == System.Xml.Schema.Compositor.Include) || (child.Compositor == System.Xml.Schema.Compositor.Redefine)) && (chameleonSchema == null))
                    {
                        base.SendValidationEvent("Sch_MissRequiredAttribute", "schemaLocation", child);
                    }
                    switch (child.Compositor)
                    {
                        case System.Xml.Schema.Compositor.Include:
                            if (child.Schema == null)
                            {
                                continue;
                            }
                            goto Label_0245;

                        case System.Xml.Schema.Compositor.Import:
                        {
                            XmlSchemaImport source = child as XmlSchemaImport;
                            str3 = source.Namespace;
                            if (str3 == schema.TargetNamespace)
                            {
                                base.SendValidationEvent("Sch_ImportTargetNamespace", child);
                            }
                            if (chameleonSchema == null)
                            {
                                break;
                            }
                            if (str3 != chameleonSchema.TargetNamespace)
                            {
                                base.SendValidationEvent("Sch_MismatchTargetNamespaceImport", str3, chameleonSchema.TargetNamespace, source);
                            }
                            rootSchemaForRedefine = this.rootSchemaForRedefine;
                            this.rootSchemaForRedefine = chameleonSchema;
                            this.Preprocess(chameleonSchema, str3, imports);
                            this.rootSchemaForRedefine = rootSchemaForRedefine;
                            continue;
                        }
                        case System.Xml.Schema.Compositor.Redefine:
                            if (chameleonSchema == null)
                            {
                                continue;
                            }
                            this.CleanupRedefine(child);
                            goto Label_0245;

                        default:
                            goto Label_0245;
                    }
                    if (str3 != null)
                    {
                        if (str3.Length == 0)
                        {
                            base.SendValidationEvent("Sch_InvalidNamespaceAttribute", str3, child);
                        }
                        else
                        {
                            this.ParseUri(str3, "Sch_InvalidNamespace", child);
                        }
                    }
                    continue;
                Label_0245:
                    if (chameleonSchema.TargetNamespace != null)
                    {
                        if (schema.TargetNamespace != chameleonSchema.TargetNamespace)
                        {
                            base.SendValidationEvent("Sch_MismatchTargetNamespaceInclude", chameleonSchema.TargetNamespace, schema.TargetNamespace, child);
                        }
                    }
                    else if ((targetNamespace != null) && (targetNamespace.Length != 0))
                    {
                        chameleonSchema = this.GetChameleonSchema(targetNamespace, chameleonSchema);
                        child.Schema = chameleonSchema;
                    }
                    this.Preprocess(chameleonSchema, schema.TargetNamespace, imports);
                }
                this.currentSchema = schema;
                this.BuildRefNamespaces(schema);
                this.ValidateIdAttribute(schema);
                this.targetNamespace = (targetNamespace == null) ? string.Empty : targetNamespace;
                this.SetSchemaDefaults(schema);
                this.processedExternals.Clear();
                for (int j = 0; j < schema.Includes.Count; j++)
                {
                    XmlSchemaExternal external2 = (XmlSchemaExternal) schema.Includes[j];
                    XmlSchema key = external2.Schema;
                    if (key != null)
                    {
                        switch (external2.Compositor)
                        {
                            case System.Xml.Schema.Compositor.Include:
                                if (this.processedExternals[key] != null)
                                {
                                    continue;
                                }
                                this.processedExternals.Add(key, external2);
                                this.CopyIncludedComponents(key, schema);
                                break;

                            case System.Xml.Schema.Compositor.Import:
                                if (key != this.rootSchema)
                                {
                                    XmlSchemaImport import2 = external2 as XmlSchemaImport;
                                    string item = (import2.Namespace != null) ? import2.Namespace : string.Empty;
                                    if (!imports.Contains(key))
                                    {
                                        imports.Add(key);
                                    }
                                    if (!this.rootSchema.ImportedNamespaces.Contains(item))
                                    {
                                        this.rootSchema.ImportedNamespaces.Add(item);
                                    }
                                }
                                break;

                            case System.Xml.Schema.Compositor.Redefine:
                                if (this.redefinedList == null)
                                {
                                    this.redefinedList = new ArrayList();
                                }
                                this.redefinedList.Add(new RedefineEntry(external2 as XmlSchemaRedefine, this.rootSchemaForRedefine));
                                if (this.processedExternals[key] != null)
                                {
                                    continue;
                                }
                                this.processedExternals.Add(key, external2);
                                this.CopyIncludedComponents(key, schema);
                                break;
                        }
                    }
                    else if (external2.Compositor == System.Xml.Schema.Compositor.Redefine)
                    {
                        XmlSchemaRedefine redefine = external2 as XmlSchemaRedefine;
                        if (redefine.BaseUri == null)
                        {
                            for (int n = 0; n < redefine.Items.Count; n++)
                            {
                                if (!(redefine.Items[n] is XmlSchemaAnnotation))
                                {
                                    base.SendValidationEvent("Sch_RedefineNoSchema", redefine);
                                    break;
                                }
                            }
                        }
                    }
                    this.ValidateIdAttribute(external2);
                }
                List<XmlSchemaObject> list = new List<XmlSchemaObject>();
                XmlSchemaObjectCollection items = schema.Items;
                for (int k = 0; k < items.Count; k++)
                {
                    this.SetParent(items[k], schema);
                    XmlSchemaAttribute attribute = items[k] as XmlSchemaAttribute;
                    if (attribute != null)
                    {
                        this.PreprocessAttribute(attribute);
                        base.AddToTable(schema.Attributes, attribute.QualifiedName, attribute);
                    }
                    else if (items[k] is XmlSchemaAttributeGroup)
                    {
                        XmlSchemaAttributeGroup attributeGroup = (XmlSchemaAttributeGroup) items[k];
                        this.PreprocessAttributeGroup(attributeGroup);
                        base.AddToTable(schema.AttributeGroups, attributeGroup.QualifiedName, attributeGroup);
                    }
                    else if (items[k] is XmlSchemaComplexType)
                    {
                        XmlSchemaComplexType complexType = (XmlSchemaComplexType) items[k];
                        this.PreprocessComplexType(complexType, false);
                        base.AddToTable(schema.SchemaTypes, complexType.QualifiedName, complexType);
                    }
                    else if (items[k] is XmlSchemaSimpleType)
                    {
                        XmlSchemaSimpleType simpleType = (XmlSchemaSimpleType) items[k];
                        this.PreprocessSimpleType(simpleType, false);
                        base.AddToTable(schema.SchemaTypes, simpleType.QualifiedName, simpleType);
                    }
                    else if (items[k] is XmlSchemaElement)
                    {
                        XmlSchemaElement element = (XmlSchemaElement) items[k];
                        this.PreprocessElement(element);
                        base.AddToTable(schema.Elements, element.QualifiedName, element);
                    }
                    else if (items[k] is XmlSchemaGroup)
                    {
                        XmlSchemaGroup group = (XmlSchemaGroup) items[k];
                        this.PreprocessGroup(group);
                        base.AddToTable(schema.Groups, group.QualifiedName, group);
                    }
                    else if (items[k] is XmlSchemaNotation)
                    {
                        XmlSchemaNotation notation = (XmlSchemaNotation) items[k];
                        this.PreprocessNotation(notation);
                        base.AddToTable(schema.Notations, notation.QualifiedName, notation);
                    }
                    else if (items[k] is XmlSchemaAnnotation)
                    {
                        this.PreprocessAnnotation(items[k] as XmlSchemaAnnotation);
                    }
                    else
                    {
                        base.SendValidationEvent("Sch_InvalidCollection", items[k]);
                        list.Add(items[k]);
                    }
                }
                for (int m = 0; m < list.Count; m++)
                {
                    schema.Items.Remove(list[m]);
                }
            }
        }

        private void PreprocessAnnotation(XmlSchemaAnnotation annotation)
        {
            this.ValidateIdAttribute(annotation);
            for (int i = 0; i < annotation.Items.Count; i++)
            {
                annotation.Items[i].Parent = annotation;
            }
        }

        private void PreprocessAnnotation(XmlSchemaObject schemaObject)
        {
            if (schemaObject is XmlSchemaAnnotated)
            {
                XmlSchemaAnnotated annotated = schemaObject as XmlSchemaAnnotated;
                XmlSchemaAnnotation annotation = annotated.Annotation;
                if (annotation != null)
                {
                    this.PreprocessAnnotation(annotation);
                    annotation.Parent = schemaObject;
                }
            }
        }

        private void PreprocessAttribute(XmlSchemaAttribute attribute)
        {
            if (attribute.Name != null)
            {
                this.ValidateNameAttribute(attribute);
                attribute.SetQualifiedName(new XmlQualifiedName(attribute.Name, this.targetNamespace));
            }
            else
            {
                base.SendValidationEvent("Sch_MissRequiredAttribute", "name", attribute);
            }
            if (attribute.Use != XmlSchemaUse.None)
            {
                base.SendValidationEvent("Sch_ForbiddenAttribute", "use", attribute);
            }
            if (attribute.Form != XmlSchemaForm.None)
            {
                base.SendValidationEvent("Sch_ForbiddenAttribute", "form", attribute);
            }
            this.PreprocessAttributeContent(attribute);
            this.ValidateIdAttribute(attribute);
        }

        private void PreprocessAttributeContent(XmlSchemaAttribute attribute)
        {
            this.PreprocessAnnotation(attribute);
            if (Ref.Equal(this.currentSchema.TargetNamespace, this.NsXsi))
            {
                base.SendValidationEvent("Sch_TargetNamespaceXsi", attribute);
            }
            if (!attribute.RefName.IsEmpty)
            {
                base.SendValidationEvent("Sch_ForbiddenAttribute", "ref", attribute);
            }
            if ((attribute.DefaultValue != null) && (attribute.FixedValue != null))
            {
                base.SendValidationEvent("Sch_DefaultFixedAttributes", attribute);
            }
            if (((attribute.DefaultValue != null) && (attribute.Use != XmlSchemaUse.Optional)) && (attribute.Use != XmlSchemaUse.None))
            {
                base.SendValidationEvent("Sch_OptionalDefaultAttribute", attribute);
            }
            if (attribute.Name == this.Xmlns)
            {
                base.SendValidationEvent("Sch_XmlNsAttribute", attribute);
            }
            if (attribute.SchemaType != null)
            {
                this.SetParent(attribute.SchemaType, attribute);
                if (!attribute.SchemaTypeName.IsEmpty)
                {
                    base.SendValidationEvent("Sch_TypeMutualExclusive", attribute);
                }
                this.PreprocessSimpleType(attribute.SchemaType, true);
            }
            if (!attribute.SchemaTypeName.IsEmpty)
            {
                this.ValidateQNameAttribute(attribute, "type", attribute.SchemaTypeName);
            }
        }

        private void PreprocessAttributeGroup(XmlSchemaAttributeGroup attributeGroup)
        {
            if (attributeGroup.Name != null)
            {
                this.ValidateNameAttribute(attributeGroup);
                attributeGroup.SetQualifiedName(new XmlQualifiedName(attributeGroup.Name, this.targetNamespace));
            }
            else
            {
                base.SendValidationEvent("Sch_MissRequiredAttribute", "name", attributeGroup);
            }
            this.PreprocessAttributes(attributeGroup.Attributes, attributeGroup.AnyAttribute, attributeGroup);
            this.PreprocessAnnotation(attributeGroup);
            this.ValidateIdAttribute(attributeGroup);
        }

        private void PreprocessAttributes(XmlSchemaObjectCollection attributes, XmlSchemaAnyAttribute anyAttribute, XmlSchemaObject parent)
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                this.SetParent(attributes[i], parent);
                XmlSchemaAttribute attribute = attributes[i] as XmlSchemaAttribute;
                if (attribute != null)
                {
                    this.PreprocessLocalAttribute(attribute);
                }
                else
                {
                    XmlSchemaAttributeGroupRef source = (XmlSchemaAttributeGroupRef) attributes[i];
                    if (source.RefName.IsEmpty)
                    {
                        base.SendValidationEvent("Sch_MissAttribute", "ref", source);
                    }
                    else
                    {
                        this.ValidateQNameAttribute(source, "ref", source.RefName);
                    }
                    this.PreprocessAnnotation(attributes[i]);
                    this.ValidateIdAttribute(attributes[i]);
                }
            }
            if (anyAttribute != null)
            {
                try
                {
                    this.SetParent(anyAttribute, parent);
                    this.PreprocessAnnotation(anyAttribute);
                    anyAttribute.BuildNamespaceList(this.targetNamespace);
                }
                catch (FormatException exception)
                {
                    base.SendValidationEvent("Sch_InvalidAnyDetailed", new string[] { exception.Message }, exception, anyAttribute);
                }
                this.ValidateIdAttribute(anyAttribute);
            }
        }

        private void PreprocessComplexType(XmlSchemaComplexType complexType, bool local)
        {
            if (local)
            {
                if (complexType.Name != null)
                {
                    base.SendValidationEvent("Sch_ForbiddenAttribute", "name", complexType);
                }
            }
            else
            {
                if (complexType.Name != null)
                {
                    this.ValidateNameAttribute(complexType);
                    complexType.SetQualifiedName(new XmlQualifiedName(complexType.Name, this.targetNamespace));
                }
                else
                {
                    base.SendValidationEvent("Sch_MissRequiredAttribute", "name", complexType);
                }
                if (complexType.Block == XmlSchemaDerivationMethod.All)
                {
                    complexType.SetBlockResolved(XmlSchemaDerivationMethod.All);
                }
                else if (complexType.Block == XmlSchemaDerivationMethod.None)
                {
                    complexType.SetBlockResolved(this.blockDefault & (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension));
                }
                else
                {
                    if ((complexType.Block & ~(XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension)) != XmlSchemaDerivationMethod.Empty)
                    {
                        base.SendValidationEvent("Sch_InvalidComplexTypeBlockValue", complexType);
                    }
                    complexType.SetBlockResolved(complexType.Block & (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension));
                }
                if (complexType.Final == XmlSchemaDerivationMethod.All)
                {
                    complexType.SetFinalResolved(XmlSchemaDerivationMethod.All);
                }
                else if (complexType.Final == XmlSchemaDerivationMethod.None)
                {
                    if (this.finalDefault == XmlSchemaDerivationMethod.All)
                    {
                        complexType.SetFinalResolved(XmlSchemaDerivationMethod.All);
                    }
                    else
                    {
                        complexType.SetFinalResolved(this.finalDefault & (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension));
                    }
                }
                else
                {
                    if ((complexType.Final & ~(XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension)) != XmlSchemaDerivationMethod.Empty)
                    {
                        base.SendValidationEvent("Sch_InvalidComplexTypeFinalValue", complexType);
                    }
                    complexType.SetFinalResolved(complexType.Final & (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension));
                }
            }
            if (complexType.ContentModel != null)
            {
                this.SetParent(complexType.ContentModel, complexType);
                this.PreprocessAnnotation(complexType.ContentModel);
                if (complexType.Particle == null)
                {
                    XmlSchemaObjectCollection attributes = complexType.Attributes;
                }
                if (complexType.ContentModel is XmlSchemaSimpleContent)
                {
                    XmlSchemaSimpleContent contentModel = (XmlSchemaSimpleContent) complexType.ContentModel;
                    if (contentModel.Content == null)
                    {
                        if (complexType.QualifiedName == XmlQualifiedName.Empty)
                        {
                            base.SendValidationEvent("Sch_NoRestOrExt", complexType);
                        }
                        else
                        {
                            base.SendValidationEvent("Sch_NoRestOrExtQName", complexType.QualifiedName.Name, complexType.QualifiedName.Namespace, complexType);
                        }
                    }
                    else
                    {
                        this.SetParent(contentModel.Content, contentModel);
                        this.PreprocessAnnotation(contentModel.Content);
                        if (contentModel.Content is XmlSchemaSimpleContentExtension)
                        {
                            XmlSchemaSimpleContentExtension content = (XmlSchemaSimpleContentExtension) contentModel.Content;
                            if (content.BaseTypeName.IsEmpty)
                            {
                                base.SendValidationEvent("Sch_MissAttribute", "base", content);
                            }
                            else
                            {
                                this.ValidateQNameAttribute(content, "base", content.BaseTypeName);
                            }
                            this.PreprocessAttributes(content.Attributes, content.AnyAttribute, content);
                            this.ValidateIdAttribute(content);
                        }
                        else
                        {
                            XmlSchemaSimpleContentRestriction source = (XmlSchemaSimpleContentRestriction) contentModel.Content;
                            if (source.BaseTypeName.IsEmpty)
                            {
                                base.SendValidationEvent("Sch_MissAttribute", "base", source);
                            }
                            else
                            {
                                this.ValidateQNameAttribute(source, "base", source.BaseTypeName);
                            }
                            if (source.BaseType != null)
                            {
                                this.SetParent(source.BaseType, source);
                                this.PreprocessSimpleType(source.BaseType, true);
                            }
                            this.PreprocessAttributes(source.Attributes, source.AnyAttribute, source);
                            this.ValidateIdAttribute(source);
                        }
                    }
                    this.ValidateIdAttribute(contentModel);
                }
                else
                {
                    XmlSchemaComplexContent parent = (XmlSchemaComplexContent) complexType.ContentModel;
                    if (parent.Content == null)
                    {
                        if (complexType.QualifiedName == XmlQualifiedName.Empty)
                        {
                            base.SendValidationEvent("Sch_NoRestOrExt", complexType);
                        }
                        else
                        {
                            base.SendValidationEvent("Sch_NoRestOrExtQName", complexType.QualifiedName.Name, complexType.QualifiedName.Namespace, complexType);
                        }
                    }
                    else
                    {
                        if (!parent.HasMixedAttribute && complexType.IsMixed)
                        {
                            parent.IsMixed = true;
                        }
                        this.SetParent(parent.Content, parent);
                        this.PreprocessAnnotation(parent.Content);
                        if (parent.Content is XmlSchemaComplexContentExtension)
                        {
                            XmlSchemaComplexContentExtension extension2 = (XmlSchemaComplexContentExtension) parent.Content;
                            if (extension2.BaseTypeName.IsEmpty)
                            {
                                base.SendValidationEvent("Sch_MissAttribute", "base", extension2);
                            }
                            else
                            {
                                this.ValidateQNameAttribute(extension2, "base", extension2.BaseTypeName);
                            }
                            if (extension2.Particle != null)
                            {
                                this.SetParent(extension2.Particle, extension2);
                                this.PreprocessParticle(extension2.Particle);
                            }
                            this.PreprocessAttributes(extension2.Attributes, extension2.AnyAttribute, extension2);
                            this.ValidateIdAttribute(extension2);
                        }
                        else
                        {
                            XmlSchemaComplexContentRestriction restriction2 = (XmlSchemaComplexContentRestriction) parent.Content;
                            if (restriction2.BaseTypeName.IsEmpty)
                            {
                                base.SendValidationEvent("Sch_MissAttribute", "base", restriction2);
                            }
                            else
                            {
                                this.ValidateQNameAttribute(restriction2, "base", restriction2.BaseTypeName);
                            }
                            if (restriction2.Particle != null)
                            {
                                this.SetParent(restriction2.Particle, restriction2);
                                this.PreprocessParticle(restriction2.Particle);
                            }
                            this.PreprocessAttributes(restriction2.Attributes, restriction2.AnyAttribute, restriction2);
                            this.ValidateIdAttribute(restriction2);
                        }
                        this.ValidateIdAttribute(parent);
                    }
                }
            }
            else
            {
                if (complexType.Particle != null)
                {
                    this.SetParent(complexType.Particle, complexType);
                    this.PreprocessParticle(complexType.Particle);
                }
                this.PreprocessAttributes(complexType.Attributes, complexType.AnyAttribute, complexType);
            }
            this.ValidateIdAttribute(complexType);
        }

        private void PreprocessElement(XmlSchemaElement element)
        {
            if (element.Name != null)
            {
                this.ValidateNameAttribute(element);
                element.SetQualifiedName(new XmlQualifiedName(element.Name, this.targetNamespace));
            }
            else
            {
                base.SendValidationEvent("Sch_MissRequiredAttribute", "name", element);
            }
            this.PreprocessElementContent(element);
            if (element.Final == XmlSchemaDerivationMethod.All)
            {
                element.SetFinalResolved(XmlSchemaDerivationMethod.All);
            }
            else if (element.Final == XmlSchemaDerivationMethod.None)
            {
                if (this.finalDefault == XmlSchemaDerivationMethod.All)
                {
                    element.SetFinalResolved(XmlSchemaDerivationMethod.All);
                }
                else
                {
                    element.SetFinalResolved(this.finalDefault & (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension));
                }
            }
            else
            {
                if ((element.Final & ~(XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension)) != XmlSchemaDerivationMethod.Empty)
                {
                    base.SendValidationEvent("Sch_InvalidElementFinalValue", element);
                }
                element.SetFinalResolved(element.Final & (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension));
            }
            if (element.Form != XmlSchemaForm.None)
            {
                base.SendValidationEvent("Sch_ForbiddenAttribute", "form", element);
            }
            if (element.MinOccursString != null)
            {
                base.SendValidationEvent("Sch_ForbiddenAttribute", "minOccurs", element);
            }
            if (element.MaxOccursString != null)
            {
                base.SendValidationEvent("Sch_ForbiddenAttribute", "maxOccurs", element);
            }
            if (!element.SubstitutionGroup.IsEmpty)
            {
                this.ValidateQNameAttribute(element, "type", element.SubstitutionGroup);
            }
            this.ValidateIdAttribute(element);
        }

        private void PreprocessElementContent(XmlSchemaElement element)
        {
            this.PreprocessAnnotation(element);
            if (!element.RefName.IsEmpty)
            {
                base.SendValidationEvent("Sch_ForbiddenAttribute", "ref", element);
            }
            if (element.Block == XmlSchemaDerivationMethod.All)
            {
                element.SetBlockResolved(XmlSchemaDerivationMethod.All);
            }
            else if (element.Block == XmlSchemaDerivationMethod.None)
            {
                if (this.blockDefault == XmlSchemaDerivationMethod.All)
                {
                    element.SetBlockResolved(XmlSchemaDerivationMethod.All);
                }
                else
                {
                    element.SetBlockResolved(this.blockDefault & (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Substitution));
                }
            }
            else
            {
                if ((element.Block & ~(XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Substitution)) != XmlSchemaDerivationMethod.Empty)
                {
                    base.SendValidationEvent("Sch_InvalidElementBlockValue", element);
                }
                element.SetBlockResolved(element.Block & (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Substitution));
            }
            if (element.SchemaType != null)
            {
                this.SetParent(element.SchemaType, element);
                if (!element.SchemaTypeName.IsEmpty)
                {
                    base.SendValidationEvent("Sch_TypeMutualExclusive", element);
                }
                if (element.SchemaType is XmlSchemaComplexType)
                {
                    this.PreprocessComplexType((XmlSchemaComplexType) element.SchemaType, true);
                }
                else
                {
                    this.PreprocessSimpleType((XmlSchemaSimpleType) element.SchemaType, true);
                }
            }
            if (!element.SchemaTypeName.IsEmpty)
            {
                this.ValidateQNameAttribute(element, "type", element.SchemaTypeName);
            }
            if ((element.DefaultValue != null) && (element.FixedValue != null))
            {
                base.SendValidationEvent("Sch_DefaultFixedAttributes", element);
            }
            for (int i = 0; i < element.Constraints.Count; i++)
            {
                XmlSchemaIdentityConstraint child = (XmlSchemaIdentityConstraint) element.Constraints[i];
                this.SetParent(child, element);
                this.PreprocessIdentityConstraint(child);
            }
        }

        private void PreprocessGroup(XmlSchemaGroup group)
        {
            if (group.Name != null)
            {
                this.ValidateNameAttribute(group);
                group.SetQualifiedName(new XmlQualifiedName(group.Name, this.targetNamespace));
            }
            else
            {
                base.SendValidationEvent("Sch_MissRequiredAttribute", "name", group);
            }
            if (group.Particle == null)
            {
                base.SendValidationEvent("Sch_NoGroupParticle", group);
            }
            else
            {
                if (group.Particle.MinOccursString != null)
                {
                    base.SendValidationEvent("Sch_ForbiddenAttribute", "minOccurs", group.Particle);
                }
                if (group.Particle.MaxOccursString != null)
                {
                    base.SendValidationEvent("Sch_ForbiddenAttribute", "maxOccurs", group.Particle);
                }
                this.PreprocessParticle(group.Particle);
                this.PreprocessAnnotation(group);
                this.ValidateIdAttribute(group);
            }
        }

        private void PreprocessIdentityConstraint(XmlSchemaIdentityConstraint constraint)
        {
            bool flag = true;
            this.PreprocessAnnotation(constraint);
            if (constraint.Name != null)
            {
                this.ValidateNameAttribute(constraint);
                constraint.SetQualifiedName(new XmlQualifiedName(constraint.Name, this.targetNamespace));
            }
            else
            {
                base.SendValidationEvent("Sch_MissRequiredAttribute", "name", constraint);
                flag = false;
            }
            if (this.rootSchema.IdentityConstraints[constraint.QualifiedName] != null)
            {
                base.SendValidationEvent("Sch_DupIdentityConstraint", constraint.QualifiedName.ToString(), constraint);
                flag = false;
            }
            else
            {
                this.rootSchema.IdentityConstraints.Add(constraint.QualifiedName, constraint);
            }
            if (constraint.Selector == null)
            {
                base.SendValidationEvent("Sch_IdConstraintNoSelector", constraint);
                flag = false;
            }
            if (constraint.Fields.Count == 0)
            {
                base.SendValidationEvent("Sch_IdConstraintNoFields", constraint);
                flag = false;
            }
            if (constraint is XmlSchemaKeyref)
            {
                XmlSchemaKeyref xso = (XmlSchemaKeyref) constraint;
                if (xso.Refer.IsEmpty)
                {
                    base.SendValidationEvent("Sch_IdConstraintNoRefer", constraint);
                    flag = false;
                }
                else
                {
                    this.ValidateQNameAttribute(xso, "refer", xso.Refer);
                }
            }
            if (flag)
            {
                this.ValidateIdAttribute(constraint);
                this.ValidateIdAttribute(constraint.Selector);
                this.SetParent(constraint.Selector, constraint);
                for (int i = 0; i < constraint.Fields.Count; i++)
                {
                    this.SetParent(constraint.Fields[i], constraint);
                    this.ValidateIdAttribute(constraint.Fields[i]);
                }
            }
        }

        private void PreprocessLocalAttribute(XmlSchemaAttribute attribute)
        {
            if (attribute.Name != null)
            {
                this.ValidateNameAttribute(attribute);
                this.PreprocessAttributeContent(attribute);
                attribute.SetQualifiedName(new XmlQualifiedName(attribute.Name, ((attribute.Form == XmlSchemaForm.Qualified) || ((attribute.Form == XmlSchemaForm.None) && (this.attributeFormDefault == XmlSchemaForm.Qualified))) ? this.targetNamespace : null));
            }
            else
            {
                this.PreprocessAnnotation(attribute);
                if (attribute.RefName.IsEmpty)
                {
                    base.SendValidationEvent("Sch_AttributeNameRef", "???", attribute);
                }
                else
                {
                    this.ValidateQNameAttribute(attribute, "ref", attribute.RefName);
                }
                if ((!attribute.SchemaTypeName.IsEmpty || (attribute.SchemaType != null)) || (attribute.Form != XmlSchemaForm.None))
                {
                    base.SendValidationEvent("Sch_InvalidAttributeRef", attribute);
                }
                attribute.SetQualifiedName(attribute.RefName);
            }
            this.ValidateIdAttribute(attribute);
        }

        private void PreprocessLocalElement(XmlSchemaElement element)
        {
            if (element.Name != null)
            {
                this.ValidateNameAttribute(element);
                this.PreprocessElementContent(element);
                element.SetQualifiedName(new XmlQualifiedName(element.Name, ((element.Form == XmlSchemaForm.Qualified) || ((element.Form == XmlSchemaForm.None) && (this.elementFormDefault == XmlSchemaForm.Qualified))) ? this.targetNamespace : null));
            }
            else
            {
                this.PreprocessAnnotation(element);
                if (element.RefName.IsEmpty)
                {
                    base.SendValidationEvent("Sch_ElementNameRef", element);
                }
                else
                {
                    this.ValidateQNameAttribute(element, "ref", element.RefName);
                }
                if (((!element.SchemaTypeName.IsEmpty || element.HasAbstractAttribute) || ((element.Block != XmlSchemaDerivationMethod.None) || (element.SchemaType != null))) || ((element.HasConstraints || (element.DefaultValue != null)) || (((element.Form != XmlSchemaForm.None) || (element.FixedValue != null)) || element.HasNillableAttribute)))
                {
                    base.SendValidationEvent("Sch_InvalidElementRef", element);
                }
                if ((element.DefaultValue != null) && (element.FixedValue != null))
                {
                    base.SendValidationEvent("Sch_DefaultFixedAttributes", element);
                }
                element.SetQualifiedName(element.RefName);
            }
            if (element.MinOccurs > element.MaxOccurs)
            {
                element.MinOccurs = 0M;
                base.SendValidationEvent("Sch_MinGtMax", element);
            }
            if (element.HasAbstractAttribute)
            {
                base.SendValidationEvent("Sch_ForbiddenAttribute", "abstract", element);
            }
            if (element.Final != XmlSchemaDerivationMethod.None)
            {
                base.SendValidationEvent("Sch_ForbiddenAttribute", "final", element);
            }
            if (!element.SubstitutionGroup.IsEmpty)
            {
                base.SendValidationEvent("Sch_ForbiddenAttribute", "substitutionGroup", element);
            }
            this.ValidateIdAttribute(element);
        }

        private void PreprocessNotation(XmlSchemaNotation notation)
        {
            if (notation.Name != null)
            {
                this.ValidateNameAttribute(notation);
                notation.QualifiedName = new XmlQualifiedName(notation.Name, this.targetNamespace);
            }
            else
            {
                base.SendValidationEvent("Sch_MissRequiredAttribute", "name", notation);
            }
            if ((notation.Public == null) && (notation.System == null))
            {
                base.SendValidationEvent("Sch_MissingPublicSystemAttribute", notation);
            }
            else
            {
                if (notation.Public != null)
                {
                    try
                    {
                        XmlConvert.VerifyTOKEN(notation.Public);
                    }
                    catch (XmlException exception)
                    {
                        base.SendValidationEvent("Sch_InvalidPublicAttribute", new string[] { notation.Public }, exception, notation);
                    }
                }
                if (notation.System != null)
                {
                    this.ParseUri(notation.System, "Sch_InvalidSystemAttribute", notation);
                }
            }
            this.PreprocessAnnotation(notation);
            this.ValidateIdAttribute(notation);
        }

        private void PreprocessParticle(XmlSchemaParticle particle)
        {
            XmlSchemaObjectCollection items;
            if (particle is XmlSchemaAll)
            {
                if ((particle.MinOccurs != 0M) && (particle.MinOccurs != 1M))
                {
                    particle.MinOccurs = 1M;
                    base.SendValidationEvent("Sch_InvalidAllMin", particle);
                }
                if (particle.MaxOccurs != 1M)
                {
                    particle.MaxOccurs = 1M;
                    base.SendValidationEvent("Sch_InvalidAllMax", particle);
                }
                items = ((XmlSchemaAll) particle).Items;
                for (int i = 0; i < items.Count; i++)
                {
                    XmlSchemaElement source = (XmlSchemaElement) items[i];
                    if ((source.MaxOccurs != 0M) && (source.MaxOccurs != 1M))
                    {
                        source.MaxOccurs = 1M;
                        base.SendValidationEvent("Sch_InvalidAllElementMax", source);
                    }
                    this.SetParent(source, particle);
                    this.PreprocessLocalElement(source);
                }
            }
            else
            {
                if (particle.MinOccurs > particle.MaxOccurs)
                {
                    particle.MinOccurs = particle.MaxOccurs;
                    base.SendValidationEvent("Sch_MinGtMax", particle);
                }
                if (particle is XmlSchemaChoice)
                {
                    items = ((XmlSchemaChoice) particle).Items;
                    for (int j = 0; j < items.Count; j++)
                    {
                        this.SetParent(items[j], particle);
                        XmlSchemaElement element = items[j] as XmlSchemaElement;
                        if (element != null)
                        {
                            this.PreprocessLocalElement(element);
                        }
                        else
                        {
                            this.PreprocessParticle((XmlSchemaParticle) items[j]);
                        }
                    }
                }
                else if (particle is XmlSchemaSequence)
                {
                    items = ((XmlSchemaSequence) particle).Items;
                    for (int k = 0; k < items.Count; k++)
                    {
                        this.SetParent(items[k], particle);
                        XmlSchemaElement element3 = items[k] as XmlSchemaElement;
                        if (element3 != null)
                        {
                            this.PreprocessLocalElement(element3);
                        }
                        else
                        {
                            this.PreprocessParticle((XmlSchemaParticle) items[k]);
                        }
                    }
                }
                else if (particle is XmlSchemaGroupRef)
                {
                    XmlSchemaGroupRef ref2 = (XmlSchemaGroupRef) particle;
                    if (ref2.RefName.IsEmpty)
                    {
                        base.SendValidationEvent("Sch_MissAttribute", "ref", ref2);
                    }
                    else
                    {
                        this.ValidateQNameAttribute(ref2, "ref", ref2.RefName);
                    }
                }
                else if (particle is XmlSchemaAny)
                {
                    try
                    {
                        ((XmlSchemaAny) particle).BuildNamespaceList(this.targetNamespace);
                    }
                    catch (FormatException exception)
                    {
                        base.SendValidationEvent("Sch_InvalidAnyDetailed", new string[] { exception.Message }, exception, particle);
                    }
                }
            }
            this.PreprocessAnnotation(particle);
            this.ValidateIdAttribute(particle);
        }

        private void PreprocessRedefine(RedefineEntry redefineEntry)
        {
            XmlSchemaRedefine currentSchemaObject = redefineEntry.redefine;
            XmlSchema schema = currentSchemaObject.Schema;
            this.currentSchema = GetParentSchema(currentSchemaObject);
            this.SetSchemaDefaults(this.currentSchema);
            if (schema.IsRedefined)
            {
                base.SendValidationEvent("Sch_MultipleRedefine", currentSchemaObject, XmlSeverityType.Warning);
            }
            else
            {
                schema.IsRedefined = true;
                XmlSchema schemaToUpdate = redefineEntry.schemaToUpdate;
                ArrayList includesList = new ArrayList();
                this.GetIncludedSet(schema, includesList);
                string ns = (schemaToUpdate.TargetNamespace == null) ? string.Empty : schemaToUpdate.TargetNamespace;
                XmlSchemaObjectCollection items = currentSchemaObject.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    this.SetParent(items[i], currentSchemaObject);
                    XmlSchemaGroup group = items[i] as XmlSchemaGroup;
                    if (group != null)
                    {
                        this.PreprocessGroup(group);
                        group.QualifiedName.SetNamespace(ns);
                        if (currentSchemaObject.Groups[group.QualifiedName] != null)
                        {
                            base.SendValidationEvent("Sch_GroupDoubleRedefine", group);
                        }
                        else
                        {
                            base.AddToTable(currentSchemaObject.Groups, group.QualifiedName, group);
                            XmlSchemaGroup group2 = (XmlSchemaGroup) schemaToUpdate.Groups[group.QualifiedName];
                            XmlSchema parentSchema = GetParentSchema(group2);
                            if ((group2 == null) || ((parentSchema != schema) && !includesList.Contains(parentSchema)))
                            {
                                base.SendValidationEvent("Sch_ComponentRedefineNotFound", "<group>", group.QualifiedName.ToString(), group);
                            }
                            else
                            {
                                group.Redefined = group2;
                                schemaToUpdate.Groups.Insert(group.QualifiedName, group);
                                this.CheckRefinedGroup(group);
                            }
                        }
                    }
                    else if (items[i] is XmlSchemaAttributeGroup)
                    {
                        XmlSchemaAttributeGroup attributeGroup = (XmlSchemaAttributeGroup) items[i];
                        this.PreprocessAttributeGroup(attributeGroup);
                        attributeGroup.QualifiedName.SetNamespace(ns);
                        if (currentSchemaObject.AttributeGroups[attributeGroup.QualifiedName] != null)
                        {
                            base.SendValidationEvent("Sch_AttrGroupDoubleRedefine", attributeGroup);
                        }
                        else
                        {
                            base.AddToTable(currentSchemaObject.AttributeGroups, attributeGroup.QualifiedName, attributeGroup);
                            XmlSchemaAttributeGroup group4 = (XmlSchemaAttributeGroup) schemaToUpdate.AttributeGroups[attributeGroup.QualifiedName];
                            XmlSchema item = GetParentSchema(group4);
                            if ((group4 == null) || ((item != schema) && !includesList.Contains(item)))
                            {
                                base.SendValidationEvent("Sch_ComponentRedefineNotFound", "<attributeGroup>", attributeGroup.QualifiedName.ToString(), attributeGroup);
                            }
                            else
                            {
                                attributeGroup.Redefined = group4;
                                schemaToUpdate.AttributeGroups.Insert(attributeGroup.QualifiedName, attributeGroup);
                                this.CheckRefinedAttributeGroup(attributeGroup);
                            }
                        }
                    }
                    else if (items[i] is XmlSchemaComplexType)
                    {
                        XmlSchemaComplexType complexType = (XmlSchemaComplexType) items[i];
                        this.PreprocessComplexType(complexType, false);
                        complexType.QualifiedName.SetNamespace(ns);
                        if (currentSchemaObject.SchemaTypes[complexType.QualifiedName] != null)
                        {
                            base.SendValidationEvent("Sch_ComplexTypeDoubleRedefine", complexType);
                        }
                        else
                        {
                            base.AddToTable(currentSchemaObject.SchemaTypes, complexType.QualifiedName, complexType);
                            XmlSchemaType type2 = (XmlSchemaType) schemaToUpdate.SchemaTypes[complexType.QualifiedName];
                            XmlSchema schema5 = GetParentSchema(type2);
                            if ((type2 == null) || ((schema5 != schema) && !includesList.Contains(schema5)))
                            {
                                base.SendValidationEvent("Sch_ComponentRedefineNotFound", "<complexType>", complexType.QualifiedName.ToString(), complexType);
                            }
                            else if (type2 is XmlSchemaComplexType)
                            {
                                complexType.Redefined = type2;
                                schemaToUpdate.SchemaTypes.Insert(complexType.QualifiedName, complexType);
                                this.CheckRefinedComplexType(complexType);
                            }
                            else
                            {
                                base.SendValidationEvent("Sch_SimpleToComplexTypeRedefine", complexType);
                            }
                        }
                    }
                    else if (items[i] is XmlSchemaSimpleType)
                    {
                        XmlSchemaSimpleType simpleType = (XmlSchemaSimpleType) items[i];
                        this.PreprocessSimpleType(simpleType, false);
                        simpleType.QualifiedName.SetNamespace(ns);
                        if (currentSchemaObject.SchemaTypes[simpleType.QualifiedName] != null)
                        {
                            base.SendValidationEvent("Sch_SimpleTypeDoubleRedefine", simpleType);
                        }
                        else
                        {
                            base.AddToTable(currentSchemaObject.SchemaTypes, simpleType.QualifiedName, simpleType);
                            XmlSchemaType type4 = (XmlSchemaType) schemaToUpdate.SchemaTypes[simpleType.QualifiedName];
                            XmlSchema schema6 = GetParentSchema(type4);
                            if ((type4 == null) || ((schema6 != schema) && !includesList.Contains(schema6)))
                            {
                                base.SendValidationEvent("Sch_ComponentRedefineNotFound", "<simpleType>", simpleType.QualifiedName.ToString(), simpleType);
                            }
                            else if (type4 is XmlSchemaSimpleType)
                            {
                                simpleType.Redefined = type4;
                                schemaToUpdate.SchemaTypes.Insert(simpleType.QualifiedName, simpleType);
                                this.CheckRefinedSimpleType(simpleType);
                            }
                            else
                            {
                                base.SendValidationEvent("Sch_ComplexToSimpleTypeRedefine", simpleType);
                            }
                        }
                    }
                }
            }
        }

        private void PreprocessSimpleType(XmlSchemaSimpleType simpleType, bool local)
        {
            if (local)
            {
                if (simpleType.Name != null)
                {
                    base.SendValidationEvent("Sch_ForbiddenAttribute", "name", simpleType);
                }
            }
            else
            {
                if (simpleType.Name != null)
                {
                    this.ValidateNameAttribute(simpleType);
                    simpleType.SetQualifiedName(new XmlQualifiedName(simpleType.Name, this.targetNamespace));
                }
                else
                {
                    base.SendValidationEvent("Sch_MissRequiredAttribute", "name", simpleType);
                }
                if (simpleType.Final == XmlSchemaDerivationMethod.All)
                {
                    simpleType.SetFinalResolved(XmlSchemaDerivationMethod.All);
                }
                else if (simpleType.Final == XmlSchemaDerivationMethod.None)
                {
                    if (this.finalDefault == XmlSchemaDerivationMethod.All)
                    {
                        simpleType.SetFinalResolved(XmlSchemaDerivationMethod.All);
                    }
                    else
                    {
                        simpleType.SetFinalResolved(this.finalDefault & (XmlSchemaDerivationMethod.Union | XmlSchemaDerivationMethod.List | XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension));
                    }
                }
                else
                {
                    if ((simpleType.Final & ~(XmlSchemaDerivationMethod.Union | XmlSchemaDerivationMethod.List | XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension)) != XmlSchemaDerivationMethod.Empty)
                    {
                        base.SendValidationEvent("Sch_InvalidSimpleTypeFinalValue", simpleType);
                    }
                    simpleType.SetFinalResolved(simpleType.Final & (XmlSchemaDerivationMethod.Union | XmlSchemaDerivationMethod.List | XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension));
                }
            }
            if (simpleType.Content == null)
            {
                base.SendValidationEvent("Sch_NoSimpleTypeContent", simpleType);
            }
            else if (simpleType.Content is XmlSchemaSimpleTypeRestriction)
            {
                XmlSchemaSimpleTypeRestriction content = (XmlSchemaSimpleTypeRestriction) simpleType.Content;
                this.SetParent(content, simpleType);
                for (int i = 0; i < content.Facets.Count; i++)
                {
                    this.SetParent(content.Facets[i], content);
                }
                if (content.BaseType != null)
                {
                    if (!content.BaseTypeName.IsEmpty)
                    {
                        base.SendValidationEvent("Sch_SimpleTypeRestRefBase", content);
                    }
                    this.PreprocessSimpleType(content.BaseType, true);
                }
                else if (content.BaseTypeName.IsEmpty)
                {
                    base.SendValidationEvent("Sch_SimpleTypeRestRefBaseNone", content);
                }
                else
                {
                    this.ValidateQNameAttribute(content, "base", content.BaseTypeName);
                }
                this.PreprocessAnnotation(content);
                this.ValidateIdAttribute(content);
            }
            else if (simpleType.Content is XmlSchemaSimpleTypeList)
            {
                XmlSchemaSimpleTypeList child = (XmlSchemaSimpleTypeList) simpleType.Content;
                this.SetParent(child, simpleType);
                if (child.ItemType != null)
                {
                    if (!child.ItemTypeName.IsEmpty)
                    {
                        base.SendValidationEvent("Sch_SimpleTypeListRefBase", child);
                    }
                    this.SetParent(child.ItemType, child);
                    this.PreprocessSimpleType(child.ItemType, true);
                }
                else if (child.ItemTypeName.IsEmpty)
                {
                    base.SendValidationEvent("Sch_SimpleTypeListRefBaseNone", child);
                }
                else
                {
                    this.ValidateQNameAttribute(child, "itemType", child.ItemTypeName);
                }
                this.PreprocessAnnotation(child);
                this.ValidateIdAttribute(child);
            }
            else
            {
                XmlSchemaSimpleTypeUnion union = (XmlSchemaSimpleTypeUnion) simpleType.Content;
                this.SetParent(union, simpleType);
                int count = union.BaseTypes.Count;
                if (union.MemberTypes != null)
                {
                    count += union.MemberTypes.Length;
                    XmlQualifiedName[] memberTypes = union.MemberTypes;
                    for (int k = 0; k < memberTypes.Length; k++)
                    {
                        this.ValidateQNameAttribute(union, "memberTypes", memberTypes[k]);
                    }
                }
                if (count == 0)
                {
                    base.SendValidationEvent("Sch_SimpleTypeUnionNoBase", union);
                }
                for (int j = 0; j < union.BaseTypes.Count; j++)
                {
                    XmlSchemaSimpleType type = (XmlSchemaSimpleType) union.BaseTypes[j];
                    this.SetParent(type, union);
                    this.PreprocessSimpleType(type, true);
                }
                this.PreprocessAnnotation(union);
                this.ValidateIdAttribute(union);
            }
            this.ValidateIdAttribute(simpleType);
        }

        private Uri ResolveSchemaLocationUri(XmlSchema enclosingSchema, string location)
        {
            if (location.Length == 0)
            {
                return null;
            }
            return this.xmlResolver.ResolveUri(enclosingSchema.BaseUri, location);
        }

        private void SetParent(XmlSchemaObject child, XmlSchemaObject parent)
        {
            child.Parent = parent;
        }

        private void SetSchemaDefaults(XmlSchema schema)
        {
            if (schema.BlockDefault == XmlSchemaDerivationMethod.All)
            {
                this.blockDefault = XmlSchemaDerivationMethod.All;
            }
            else if (schema.BlockDefault == XmlSchemaDerivationMethod.None)
            {
                this.blockDefault = XmlSchemaDerivationMethod.Empty;
            }
            else
            {
                if ((schema.BlockDefault & ~(XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Substitution)) != XmlSchemaDerivationMethod.Empty)
                {
                    base.SendValidationEvent("Sch_InvalidBlockDefaultValue", schema);
                }
                this.blockDefault = schema.BlockDefault & (XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Substitution);
            }
            if (schema.FinalDefault == XmlSchemaDerivationMethod.All)
            {
                this.finalDefault = XmlSchemaDerivationMethod.All;
            }
            else if (schema.FinalDefault == XmlSchemaDerivationMethod.None)
            {
                this.finalDefault = XmlSchemaDerivationMethod.Empty;
            }
            else
            {
                if ((schema.FinalDefault & ~(XmlSchemaDerivationMethod.Union | XmlSchemaDerivationMethod.List | XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension)) != XmlSchemaDerivationMethod.Empty)
                {
                    base.SendValidationEvent("Sch_InvalidFinalDefaultValue", schema);
                }
                this.finalDefault = schema.FinalDefault & (XmlSchemaDerivationMethod.Union | XmlSchemaDerivationMethod.List | XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.Extension);
            }
            this.elementFormDefault = schema.ElementFormDefault;
            if (this.elementFormDefault == XmlSchemaForm.None)
            {
                this.elementFormDefault = XmlSchemaForm.Unqualified;
            }
            this.attributeFormDefault = schema.AttributeFormDefault;
            if (this.attributeFormDefault == XmlSchemaForm.None)
            {
                this.attributeFormDefault = XmlSchemaForm.Unqualified;
            }
        }

        private void ValidateIdAttribute(XmlSchemaObject xso)
        {
            if (xso.IdAttribute != null)
            {
                try
                {
                    xso.IdAttribute = base.NameTable.Add(XmlConvert.VerifyNCName(xso.IdAttribute));
                }
                catch (XmlException exception)
                {
                    base.SendValidationEvent("Sch_InvalidIdAttribute", new string[] { exception.Message }, exception, xso);
                    return;
                }
                catch (ArgumentNullException)
                {
                    base.SendValidationEvent("Sch_InvalidIdAttribute", Res.GetString("Sch_NullValue"), xso);
                    return;
                }
                try
                {
                    this.currentSchema.Ids.Add(xso.IdAttribute, xso);
                }
                catch (ArgumentException)
                {
                    base.SendValidationEvent("Sch_DupIdAttribute", xso);
                }
            }
        }

        private void ValidateNameAttribute(XmlSchemaObject xso)
        {
            string nameAttribute = xso.NameAttribute;
            if ((nameAttribute == null) || (nameAttribute.Length == 0))
            {
                base.SendValidationEvent("Sch_InvalidNameAttributeEx", null, Res.GetString("Sch_NullValue"), xso);
            }
            nameAttribute = XmlComplianceUtil.NonCDataNormalize(nameAttribute);
            int invCharIndex = ValidateNames.ParseNCName(nameAttribute, 0);
            if (invCharIndex != nameAttribute.Length)
            {
                string[] strArray = XmlException.BuildCharExceptionArgs(nameAttribute, invCharIndex);
                string str2 = Res.GetString("Xml_BadNameCharWithPos", new object[] { strArray[0], strArray[1], invCharIndex });
                base.SendValidationEvent("Sch_InvalidNameAttributeEx", nameAttribute, str2, xso);
            }
            else
            {
                xso.NameAttribute = base.NameTable.Add(nameAttribute);
            }
        }

        private void ValidateQNameAttribute(XmlSchemaObject xso, string attributeName, XmlQualifiedName value)
        {
            try
            {
                value.Verify();
                value.Atomize(base.NameTable);
                if (this.currentSchema.IsChameleon && (value.Namespace.Length == 0))
                {
                    value.SetNamespace(this.currentSchema.TargetNamespace);
                }
                if (this.referenceNamespaces[value.Namespace] == null)
                {
                    base.SendValidationEvent("Sch_UnrefNS", value.Namespace, xso, XmlSeverityType.Warning);
                }
            }
            catch (FormatException exception)
            {
                base.SendValidationEvent("Sch_InvalidAttribute", new string[] { attributeName, exception.Message }, exception, xso);
            }
            catch (XmlException exception2)
            {
                base.SendValidationEvent("Sch_InvalidAttribute", new string[] { attributeName, exception2.Message }, exception2, xso);
            }
        }

        internal Hashtable ChameleonSchemas
        {
            set
            {
                this.chameleonSchemas = value;
            }
        }

        internal XmlReaderSettings ReaderSettings
        {
            get
            {
                if (this.readerSettings == null)
                {
                    this.readerSettings = new XmlReaderSettings();
                    this.readerSettings.DtdProcessing = DtdProcessing.Prohibit;
                }
                return this.readerSettings;
            }
            set
            {
                this.readerSettings = value;
            }
        }

        internal XmlSchema RootSchema
        {
            get
            {
                return this.rootSchema;
            }
        }

        internal Hashtable SchemaLocations
        {
            set
            {
                this.schemaLocations = value;
            }
        }

        internal System.Xml.XmlResolver XmlResolver
        {
            set
            {
                this.xmlResolver = value;
            }
        }
    }
}

