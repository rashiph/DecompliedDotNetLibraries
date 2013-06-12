namespace System.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Globalization;
    using System.Xml;
    using System.Xml.Schema;

    internal sealed class XSDSchema : XMLSchema
    {
        private DataSet _ds;
        private string _schemaName;
        private XmlSchemaSet _schemaSet;
        private XmlSchemaObjectCollection annotations;
        private Hashtable attributeGroups;
        private Hashtable attributes;
        private ArrayList ColumnExpressions;
        private ArrayList complexTypes;
        private Hashtable ConstraintNodes;
        private XmlSchemaElement dsElement;
        private XmlSchemaObjectCollection elements;
        private Hashtable elementsTable;
        private Hashtable existingSimpleTypeMap;
        private Hashtable expressions;
        private bool fromInference;
        private static readonly NameType[] mapNameTypeXsd = new NameType[] { 
            new NameType("ENTITIES", typeof(string)), new NameType("ENTITY", typeof(string)), new NameType("ID", typeof(string)), new NameType("IDREF", typeof(string)), new NameType("IDREFS", typeof(string)), new NameType("NCName", typeof(string)), new NameType("NMTOKEN", typeof(string)), new NameType("NMTOKENS", typeof(string)), new NameType("NOTATION", typeof(string)), new NameType("Name", typeof(string)), new NameType("QName", typeof(string)), new NameType("anyType", typeof(object)), new NameType("anyURI", typeof(Uri)), new NameType("base64Binary", typeof(byte[])), new NameType("boolean", typeof(bool)), new NameType("byte", typeof(sbyte)), 
            new NameType("date", typeof(DateTime)), new NameType("dateTime", typeof(DateTime)), new NameType("decimal", typeof(decimal)), new NameType("double", typeof(double)), new NameType("duration", typeof(TimeSpan)), new NameType("float", typeof(float)), new NameType("gDay", typeof(DateTime)), new NameType("gMonth", typeof(DateTime)), new NameType("gMonthDay", typeof(DateTime)), new NameType("gYear", typeof(DateTime)), new NameType("gYearMonth", typeof(DateTime)), new NameType("hexBinary", typeof(byte[])), new NameType("int", typeof(int)), new NameType("integer", typeof(long)), new NameType("language", typeof(string)), new NameType("long", typeof(long)), 
            new NameType("negativeInteger", typeof(long)), new NameType("nonNegativeInteger", typeof(ulong)), new NameType("nonPositiveInteger", typeof(long)), new NameType("normalizedString", typeof(string)), new NameType("positiveInteger", typeof(ulong)), new NameType("short", typeof(short)), new NameType("string", typeof(string)), new NameType("time", typeof(DateTime)), new NameType("unsignedByte", typeof(byte)), new NameType("unsignedInt", typeof(uint)), new NameType("unsignedLong", typeof(ulong)), new NameType("unsignedShort", typeof(ushort))
         };
        private ArrayList RefTables;
        private Hashtable schemaTypes;
        private Dictionary<DataTable, List<DataTable>> tableDictionary;
        private Hashtable udSimpleTypes;

        private void AddTablesToList(List<DataTable> tableList, DataTable dt)
        {
            if (!tableList.Contains(dt))
            {
                tableList.Add(dt);
                foreach (DataTable table in this.tableDictionary[dt])
                {
                    this.AddTablesToList(tableList, table);
                }
            }
        }

        internal DataColumn[] BuildKey(XmlSchemaIdentityConstraint keyNode, DataTable table)
        {
            ArrayList list = new ArrayList();
            foreach (XmlSchemaXPath path in keyNode.Fields)
            {
                list.Add(this.FindField(table, path.XPath));
            }
            DataColumn[] array = new DataColumn[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        private void CollectElementsAnnotations(XmlSchema schema)
        {
            ArrayList schemaList = new ArrayList();
            this.CollectElementsAnnotations(schema, schemaList);
            schemaList.Clear();
        }

        private void CollectElementsAnnotations(XmlSchema schema, ArrayList schemaList)
        {
            if (!schemaList.Contains(schema))
            {
                schemaList.Add(schema);
                foreach (object obj2 in schema.Items)
                {
                    if (obj2 is XmlSchemaAnnotation)
                    {
                        this.annotations.Add((XmlSchemaAnnotation) obj2);
                    }
                    if (obj2 is XmlSchemaElement)
                    {
                        XmlSchemaElement item = (XmlSchemaElement) obj2;
                        this.elements.Add(item);
                        this.elementsTable[item.QualifiedName] = item;
                    }
                    if (obj2 is XmlSchemaAttribute)
                    {
                        XmlSchemaAttribute attribute = (XmlSchemaAttribute) obj2;
                        this.attributes[attribute.QualifiedName] = attribute;
                    }
                    if (obj2 is XmlSchemaAttributeGroup)
                    {
                        XmlSchemaAttributeGroup group = (XmlSchemaAttributeGroup) obj2;
                        this.attributeGroups[group.QualifiedName] = group;
                    }
                    if (obj2 is XmlSchemaType)
                    {
                        if (obj2 is XmlSchemaSimpleType)
                        {
                            GetMsdataAttribute((XmlSchemaType) obj2, "targetNamespace");
                        }
                        XmlSchemaType type = (XmlSchemaType) obj2;
                        this.schemaTypes[type.QualifiedName] = type;
                        XmlSchemaSimpleType node = obj2 as XmlSchemaSimpleType;
                        if (node != null)
                        {
                            if (this.udSimpleTypes == null)
                            {
                                this.udSimpleTypes = new Hashtable();
                            }
                            this.udSimpleTypes[type.QualifiedName.ToString()] = node;
                            DataColumn column = (DataColumn) this.existingSimpleTypeMap[type.QualifiedName.ToString()];
                            SimpleType type4 = (column != null) ? column.SimpleType : null;
                            if (type4 != null)
                            {
                                SimpleType otherSimpleType = new SimpleType(node);
                                string errorStr = type4.HasConflictingDefinition(otherSimpleType);
                                if (errorStr.Length != 0)
                                {
                                    throw ExceptionBuilder.InvalidDuplicateNamedSimpleTypeDelaration(otherSimpleType.SimpleTypeQualifiedName, errorStr);
                                }
                            }
                        }
                    }
                }
                foreach (XmlSchemaExternal external in schema.Includes)
                {
                    if (!(external is XmlSchemaImport) && (external.Schema != null))
                    {
                        this.CollectElementsAnnotations(external.Schema, schemaList);
                    }
                }
            }
        }

        private int DatasetElementCount(XmlSchemaObjectCollection elements)
        {
            int num = 0;
            foreach (XmlSchemaElement element in elements)
            {
                if (this.GetBooleanAttribute(element, "IsDataSet", false))
                {
                    num++;
                }
            }
            return num;
        }

        private XmlSchemaElement FindDatasetElement(XmlSchemaObjectCollection elements)
        {
            foreach (XmlSchemaElement element2 in elements)
            {
                if (this.GetBooleanAttribute(element2, "IsDataSet", false))
                {
                    return element2;
                }
            }
            if ((elements.Count == 1) || (this.FromInference && (elements.Count > 0)))
            {
                XmlSchemaElement element = (XmlSchemaElement) elements[0];
                if (!this.GetBooleanAttribute(element, "IsDataSet", true))
                {
                    return null;
                }
                XmlSchemaComplexType schemaType = element.SchemaType as XmlSchemaComplexType;
                if (schemaType != null)
                {
                    while (schemaType != null)
                    {
                        if (this.HasAttributes(schemaType.Attributes))
                        {
                            return null;
                        }
                        if (schemaType.ContentModel is XmlSchemaSimpleContent)
                        {
                            XmlSchemaAnnotated content = ((XmlSchemaSimpleContent) schemaType.ContentModel).Content;
                            if (content is XmlSchemaSimpleContentExtension)
                            {
                                XmlSchemaSimpleContentExtension extension = (XmlSchemaSimpleContentExtension) content;
                                if (this.HasAttributes(extension.Attributes))
                                {
                                    return null;
                                }
                            }
                            else
                            {
                                XmlSchemaSimpleContentRestriction restriction = (XmlSchemaSimpleContentRestriction) content;
                                if (this.HasAttributes(restriction.Attributes))
                                {
                                    return null;
                                }
                            }
                        }
                        XmlSchemaParticle pt = this.GetParticle(schemaType);
                        if ((pt != null) && !this.IsDatasetParticle(pt))
                        {
                            return null;
                        }
                        if (!(schemaType.BaseXmlSchemaType is XmlSchemaComplexType))
                        {
                            return element;
                        }
                        schemaType = (XmlSchemaComplexType) schemaType.BaseXmlSchemaType;
                    }
                    return element;
                }
            }
            return null;
        }

        internal DataColumn FindField(DataTable table, string field)
        {
            bool flag = false;
            string name = field;
            if (field.StartsWith("@", StringComparison.Ordinal))
            {
                flag = true;
                name = field.Substring(1);
            }
            string[] strArray = name.Split(new char[] { ':' });
            name = strArray[strArray.Length - 1];
            name = XmlConvert.DecodeName(name);
            DataColumn column = table.Columns[name];
            if (column == null)
            {
                throw ExceptionBuilder.InvalidField(field);
            }
            bool flag2 = (column.ColumnMapping == MappingType.Attribute) || (column.ColumnMapping == MappingType.Hidden);
            if (flag2 != flag)
            {
                throw ExceptionBuilder.InvalidField(field);
            }
            return column;
        }

        private static NameType FindNameType(string name)
        {
            int index = Array.BinarySearch(mapNameTypeXsd, name);
            if (index < 0)
            {
                throw ExceptionBuilder.UndefinedDatatype(name);
            }
            return mapNameTypeXsd[index];
        }

        internal XmlSchemaAnnotated FindTypeNode(XmlSchemaAnnotated node)
        {
            XmlSchemaAttribute attribute = node as XmlSchemaAttribute;
            XmlSchemaElement element = node as XmlSchemaElement;
            bool flag = false;
            if (attribute != null)
            {
                flag = true;
            }
            string str = flag ? attribute.SchemaTypeName.Name : element.SchemaTypeName.Name;
            string str2 = flag ? attribute.SchemaTypeName.Namespace : element.SchemaTypeName.Namespace;
            if (str2 == "http://www.w3.org/2001/XMLSchema")
            {
                return null;
            }
            if ((str == null) || (str.Length == 0))
            {
                str = flag ? attribute.RefName.Name : element.RefName.Name;
                if ((str == null) || (str.Length == 0))
                {
                    return (flag ? attribute.SchemaType : element.SchemaType);
                }
                return (flag ? this.FindTypeNode((XmlSchemaAnnotated) this.attributes[attribute.RefName]) : this.FindTypeNode((XmlSchemaAnnotated) this.elementsTable[element.RefName]));
            }
            return (XmlSchemaAnnotated) this.schemaTypes[flag ? ((XmlSchemaAttribute) node).SchemaTypeName : ((XmlSchemaElement) node).SchemaTypeName];
        }

        internal bool GetBooleanAttribute(XmlSchemaAnnotated element, string attrName, bool defVal)
        {
            string msdataAttribute = GetMsdataAttribute(element, attrName);
            if ((msdataAttribute == null) || (msdataAttribute.Length == 0))
            {
                return defVal;
            }
            if ((msdataAttribute == "true") || (msdataAttribute == "1"))
            {
                return true;
            }
            if (!(msdataAttribute == "false") && !(msdataAttribute == "0"))
            {
                throw ExceptionBuilder.InvalidAttributeValue(attrName, msdataAttribute);
            }
            return false;
        }

        internal string GetInstanceName(XmlSchemaAnnotated node)
        {
            string str = null;
            if (node is XmlSchemaElement)
            {
                XmlSchemaElement element = (XmlSchemaElement) node;
                return ((element.Name != null) ? element.Name : element.RefName.Name);
            }
            if (node is XmlSchemaAttribute)
            {
                XmlSchemaAttribute attribute = (XmlSchemaAttribute) node;
                str = (attribute.Name != null) ? attribute.Name : attribute.RefName.Name;
            }
            return str;
        }

        internal static string GetMsdataAttribute(XmlSchemaAnnotated node, string ln)
        {
            XmlAttribute[] unhandledAttributes = node.UnhandledAttributes;
            if (unhandledAttributes != null)
            {
                for (int i = 0; i < unhandledAttributes.Length; i++)
                {
                    if ((unhandledAttributes[i].LocalName == ln) && (unhandledAttributes[i].NamespaceURI == "urn:schemas-microsoft-com:xml-msdata"))
                    {
                        return unhandledAttributes[i].Value;
                    }
                }
            }
            return null;
        }

        private string GetNamespaceFromPrefix(string prefix)
        {
            if ((prefix != null) && (prefix.Length != 0))
            {
                foreach (XmlSchema schema in this._schemaSet.Schemas())
                {
                    XmlQualifiedName[] nameArray = schema.Namespaces.ToArray();
                    for (int i = 0; i < nameArray.Length; i++)
                    {
                        if (nameArray[i].Name == prefix)
                        {
                            return nameArray[i].Namespace;
                        }
                    }
                }
            }
            return null;
        }

        internal XmlSchemaParticle GetParticle(XmlSchemaComplexType ct)
        {
            if (ct.ContentModel == null)
            {
                return ct.Particle;
            }
            if (!(ct.ContentModel is XmlSchemaComplexContent))
            {
                return null;
            }
            XmlSchemaAnnotated content = ((XmlSchemaComplexContent) ct.ContentModel).Content;
            if (content is XmlSchemaComplexContentExtension)
            {
                return ((XmlSchemaComplexContentExtension) content).Particle;
            }
            return ((XmlSchemaComplexContentRestriction) content).Particle;
        }

        internal XmlSchemaObjectCollection GetParticleItems(XmlSchemaParticle pt)
        {
            if (pt is XmlSchemaSequence)
            {
                return ((XmlSchemaSequence) pt).Items;
            }
            if (pt is XmlSchemaAll)
            {
                return ((XmlSchemaAll) pt).Items;
            }
            if (pt is XmlSchemaChoice)
            {
                return ((XmlSchemaChoice) pt).Items;
            }
            if (!(pt is XmlSchemaAny))
            {
                if (pt is XmlSchemaElement)
                {
                    XmlSchemaObjectCollection objects = new XmlSchemaObjectCollection();
                    objects.Add(pt);
                    return objects;
                }
                if (pt is XmlSchemaGroupRef)
                {
                    return this.GetParticleItems(((XmlSchemaGroupRef) pt).Particle);
                }
            }
            return null;
        }

        private string GetPrefix(string ns)
        {
            if (ns != null)
            {
                foreach (XmlSchema schema in this._schemaSet.Schemas())
                {
                    XmlQualifiedName[] nameArray = schema.Namespaces.ToArray();
                    for (int i = 0; i < nameArray.Length; i++)
                    {
                        if (nameArray[i].Namespace == ns)
                        {
                            return nameArray[i].Name;
                        }
                    }
                }
            }
            return null;
        }

        internal string GetStringAttribute(XmlSchemaAnnotated element, string attrName, string defVal)
        {
            string msdataAttribute = GetMsdataAttribute(element, attrName);
            if ((msdataAttribute != null) && (msdataAttribute.Length != 0))
            {
                return msdataAttribute;
            }
            return defVal;
        }

        private string GetTableName(XmlSchemaIdentityConstraint key)
        {
            string xPath = key.Selector.XPath;
            string[] strArray = xPath.Split(new char[] { '/', ':' });
            string name = strArray[strArray.Length - 1];
            if ((name == null) || (name.Length == 0))
            {
                throw ExceptionBuilder.InvalidSelector(xPath);
            }
            return XmlConvert.DecodeName(name);
        }

        private string GetTableNamespace(XmlSchemaIdentityConstraint key)
        {
            string xPath = key.Selector.XPath;
            string[] strArray = xPath.Split(new char[] { '/' });
            string name = string.Empty;
            string str = strArray[strArray.Length - 1];
            if ((str == null) || (str.Length == 0))
            {
                throw ExceptionBuilder.InvalidSelector(xPath);
            }
            if (str.IndexOf(':') != -1)
            {
                name = str.Substring(0, str.IndexOf(':'));
            }
            else
            {
                return GetMsdataAttribute(key, "TableNamespace");
            }
            name = XmlConvert.DecodeName(name);
            return this.GetNamespaceFromPrefix(name);
        }

        internal void HandleAttributeColumn(XmlSchemaAttribute attrib, DataTable table, bool isBase)
        {
            DataColumn column;
            Type dataType = null;
            XmlSchemaAttribute node = (attrib.Name != null) ? attrib : ((XmlSchemaAttribute) this.attributes[attrib.RefName]);
            XmlSchemaAnnotated annotated = this.FindTypeNode(node);
            string str = null;
            SimpleType type2 = null;
            if (annotated == null)
            {
                str = node.SchemaTypeName.Name;
                if (ADP.IsEmpty(str))
                {
                    str = "";
                    dataType = typeof(string);
                }
                else if (node.SchemaTypeName.Namespace != "http://www.w3.org/2001/XMLSchema")
                {
                    dataType = this.ParseDataType(node.SchemaTypeName.ToString());
                }
                else
                {
                    dataType = this.ParseDataType(node.SchemaTypeName.Name);
                }
            }
            else if (annotated is XmlSchemaSimpleType)
            {
                XmlSchemaSimpleType type3 = annotated as XmlSchemaSimpleType;
                type2 = new SimpleType(type3);
                if (((type3.QualifiedName.Name != null) && (type3.QualifiedName.Name.Length != 0)) && (type3.QualifiedName.Namespace != "http://www.w3.org/2001/XMLSchema"))
                {
                    str = type3.QualifiedName.ToString();
                    dataType = this.ParseDataType(type3.QualifiedName.ToString());
                }
                else
                {
                    dataType = this.ParseDataType(type2.BaseType);
                    str = type2.Name;
                    if ((type2.Length == 1) && (dataType == typeof(string)))
                    {
                        dataType = typeof(char);
                    }
                }
            }
            else if (annotated is XmlSchemaElement)
            {
                str = ((XmlSchemaElement) annotated).SchemaTypeName.Name;
                dataType = this.ParseDataType(str);
            }
            else
            {
                if (annotated.Id == null)
                {
                    throw ExceptionBuilder.DatatypeNotDefined();
                }
                throw ExceptionBuilder.UndefinedDatatype(annotated.Id);
            }
            string name = XmlConvert.DecodeName(this.GetInstanceName(node));
            bool flag = true;
            if ((!isBase || this.FromInference) && table.Columns.Contains(name, true))
            {
                column = table.Columns[name];
                flag = false;
                if (this.FromInference)
                {
                    if (column.ColumnMapping != MappingType.Attribute)
                    {
                        throw ExceptionBuilder.ColumnTypeConflict(column.ColumnName);
                    }
                    if ((ADP.IsEmpty(attrib.QualifiedName.Namespace) && ADP.IsEmpty(column._columnUri)) || (string.Compare(attrib.QualifiedName.Namespace, column.Namespace, StringComparison.Ordinal) == 0))
                    {
                        return;
                    }
                    column = new DataColumn(name, dataType, null, MappingType.Attribute);
                    flag = true;
                }
            }
            else
            {
                column = new DataColumn(name, dataType, null, MappingType.Attribute);
            }
            SetProperties(column, node.UnhandledAttributes);
            this.HandleColumnExpression(column, node.UnhandledAttributes);
            SetExtProperties(column, node.UnhandledAttributes);
            if ((column.Expression != null) && (column.Expression.Length != 0))
            {
                this.ColumnExpressions.Add(column);
            }
            if (((type2 != null) && (type2.Name != null)) && (type2.Name.Length > 0))
            {
                if (GetMsdataAttribute(annotated, "targetNamespace") != null)
                {
                    column.XmlDataType = type2.SimpleTypeQualifiedName;
                }
            }
            else
            {
                column.XmlDataType = str;
            }
            column.SimpleType = type2;
            column.AllowDBNull = attrib.Use != XmlSchemaUse.Required;
            column.Namespace = attrib.QualifiedName.Namespace;
            column.Namespace = this.GetStringAttribute(attrib, "targetNamespace", column.Namespace);
            if (flag)
            {
                if (this.FromInference)
                {
                    column.AllowDBNull = true;
                    column.Prefix = this.GetPrefix(column.Namespace);
                }
                table.Columns.Add(column);
            }
            if (attrib.Use == XmlSchemaUse.Prohibited)
            {
                column.ColumnMapping = MappingType.Hidden;
                column.AllowDBNull = this.GetBooleanAttribute(node, "AllowDBNull", true);
                string msdataAttribute = GetMsdataAttribute(node, "DefaultValue");
                if (msdataAttribute != null)
                {
                    try
                    {
                        column.DefaultValue = column.ConvertXmlToObject(msdataAttribute);
                    }
                    catch (FormatException)
                    {
                        throw ExceptionBuilder.CannotConvert(msdataAttribute, dataType.FullName);
                    }
                }
            }
            string s = (attrib.Use == XmlSchemaUse.Required) ? GetMsdataAttribute(node, "DefaultValue") : node.DefaultValue;
            if ((node.Use == XmlSchemaUse.Optional) && (s == null))
            {
                s = node.FixedValue;
            }
            if (s != null)
            {
                try
                {
                    column.DefaultValue = column.ConvertXmlToObject(s);
                }
                catch (FormatException)
                {
                    throw ExceptionBuilder.CannotConvert(s, dataType.FullName);
                }
            }
        }

        private void HandleAttributeGroup(XmlSchemaAttributeGroup attributeGroup, DataTable table, bool isBase)
        {
            foreach (XmlSchemaObject obj2 in attributeGroup.Attributes)
            {
                if (obj2 is XmlSchemaAttribute)
                {
                    this.HandleAttributeColumn((XmlSchemaAttribute) obj2, table, isBase);
                }
                else
                {
                    XmlSchemaAttributeGroup redefinedAttributeGroup;
                    XmlSchemaAttributeGroupRef ref2 = (XmlSchemaAttributeGroupRef) obj2;
                    if ((attributeGroup.RedefinedAttributeGroup != null) && (ref2.RefName == new XmlQualifiedName(attributeGroup.Name, ref2.RefName.Namespace)))
                    {
                        redefinedAttributeGroup = attributeGroup.RedefinedAttributeGroup;
                    }
                    else
                    {
                        redefinedAttributeGroup = (XmlSchemaAttributeGroup) this.attributeGroups[ref2.RefName];
                    }
                    if (redefinedAttributeGroup != null)
                    {
                        this.HandleAttributeGroup(redefinedAttributeGroup, table, isBase);
                    }
                }
            }
        }

        internal void HandleAttributes(XmlSchemaObjectCollection attributes, DataTable table, bool isBase)
        {
            foreach (XmlSchemaObject obj2 in attributes)
            {
                if (obj2 is XmlSchemaAttribute)
                {
                    this.HandleAttributeColumn((XmlSchemaAttribute) obj2, table, isBase);
                }
                else
                {
                    XmlSchemaAttributeGroupRef ref2 = obj2 as XmlSchemaAttributeGroupRef;
                    XmlSchemaAttributeGroup attributeGroup = this.attributeGroups[ref2.RefName] as XmlSchemaAttributeGroup;
                    if (attributeGroup != null)
                    {
                        this.HandleAttributeGroup(attributeGroup, table, isBase);
                    }
                }
            }
        }

        private void HandleColumnExpression(object instance, XmlAttribute[] attrs)
        {
            if (attrs != null)
            {
                DataColumn column = instance as DataColumn;
                if (column != null)
                {
                    for (int i = 0; i < attrs.Length; i++)
                    {
                        if ((attrs[i].NamespaceURI == "urn:schemas-microsoft-com:xml-msdata") && (attrs[i].LocalName == "Expression"))
                        {
                            if (this.expressions == null)
                            {
                                this.expressions = new Hashtable();
                            }
                            this.expressions[column] = attrs[i].Value;
                            this.ColumnExpressions.Add(column);
                            return;
                        }
                    }
                }
            }
        }

        internal void HandleComplexType(XmlSchemaComplexType ct, DataTable table, ArrayList tableChildren, bool isNillable)
        {
            if (this.complexTypes.Contains(ct))
            {
                throw ExceptionBuilder.CircularComplexType(ct.Name);
            }
            bool isBase = false;
            this.complexTypes.Add(ct);
            if (ct.ContentModel != null)
            {
                if (ct.ContentModel is XmlSchemaComplexContent)
                {
                    XmlSchemaAnnotated content = ((XmlSchemaComplexContent) ct.ContentModel).Content;
                    if (content is XmlSchemaComplexContentExtension)
                    {
                        XmlSchemaComplexContentExtension extension = (XmlSchemaComplexContentExtension) content;
                        if (!(ct.BaseXmlSchemaType is XmlSchemaComplexType) || !this.FromInference)
                        {
                            this.HandleAttributes(extension.Attributes, table, isBase);
                        }
                        if (ct.BaseXmlSchemaType is XmlSchemaComplexType)
                        {
                            this.HandleComplexType((XmlSchemaComplexType) ct.BaseXmlSchemaType, table, tableChildren, isNillable);
                        }
                        else if (extension.BaseTypeName.Namespace != "http://www.w3.org/2001/XMLSchema")
                        {
                            this.HandleSimpleContentColumn(extension.BaseTypeName.ToString(), table, isBase, ct.ContentModel.UnhandledAttributes, isNillable);
                        }
                        else
                        {
                            this.HandleSimpleContentColumn(extension.BaseTypeName.Name, table, isBase, ct.ContentModel.UnhandledAttributes, isNillable);
                        }
                        if (extension.Particle != null)
                        {
                            this.HandleParticle(extension.Particle, table, tableChildren, isBase);
                        }
                        if ((ct.BaseXmlSchemaType is XmlSchemaComplexType) && this.FromInference)
                        {
                            this.HandleAttributes(extension.Attributes, table, isBase);
                        }
                    }
                    else
                    {
                        XmlSchemaComplexContentRestriction restriction = (XmlSchemaComplexContentRestriction) content;
                        if (!this.FromInference)
                        {
                            this.HandleAttributes(restriction.Attributes, table, isBase);
                        }
                        if (restriction.Particle != null)
                        {
                            this.HandleParticle(restriction.Particle, table, tableChildren, isBase);
                        }
                        if (this.FromInference)
                        {
                            this.HandleAttributes(restriction.Attributes, table, isBase);
                        }
                    }
                }
                else
                {
                    XmlSchemaAnnotated annotated = ((XmlSchemaSimpleContent) ct.ContentModel).Content;
                    if (annotated is XmlSchemaSimpleContentExtension)
                    {
                        XmlSchemaSimpleContentExtension extension2 = (XmlSchemaSimpleContentExtension) annotated;
                        this.HandleAttributes(extension2.Attributes, table, isBase);
                        if (ct.BaseXmlSchemaType is XmlSchemaComplexType)
                        {
                            this.HandleComplexType((XmlSchemaComplexType) ct.BaseXmlSchemaType, table, tableChildren, isNillable);
                        }
                        else
                        {
                            this.HandleSimpleTypeSimpleContentColumn((XmlSchemaSimpleType) ct.BaseXmlSchemaType, extension2.BaseTypeName.Name, table, isBase, ct.ContentModel.UnhandledAttributes, isNillable);
                        }
                    }
                    else
                    {
                        XmlSchemaSimpleContentRestriction restriction2 = (XmlSchemaSimpleContentRestriction) annotated;
                        this.HandleAttributes(restriction2.Attributes, table, isBase);
                    }
                }
            }
            else
            {
                isBase = true;
                if (!this.FromInference)
                {
                    this.HandleAttributes(ct.Attributes, table, isBase);
                }
                if (ct.Particle != null)
                {
                    this.HandleParticle(ct.Particle, table, tableChildren, isBase);
                }
                if (this.FromInference)
                {
                    this.HandleAttributes(ct.Attributes, table, isBase);
                    if (isNillable)
                    {
                        this.HandleSimpleContentColumn("string", table, isBase, null, isNillable);
                    }
                }
            }
            this.complexTypes.Remove(ct);
        }

        internal void HandleConstraint(XmlSchemaIdentityConstraint keyNode)
        {
            string key = null;
            key = XmlConvert.DecodeName(keyNode.Name);
            if ((key == null) || (key.Length == 0))
            {
                throw ExceptionBuilder.MissingAttribute("name");
            }
            if (this.ConstraintNodes.ContainsKey(key))
            {
                throw ExceptionBuilder.DuplicateConstraintRead(key);
            }
            string tableName = this.GetTableName(keyNode);
            string msdataAttribute = GetMsdataAttribute(keyNode, "TableNamespace");
            DataTable tableSmart = this._ds.Tables.GetTableSmart(tableName, msdataAttribute);
            if (tableSmart != null)
            {
                this.ConstraintNodes.Add(key, new ConstraintTable(tableSmart, keyNode));
                bool primaryKey = this.GetBooleanAttribute(keyNode, "PrimaryKey", false);
                key = this.GetStringAttribute(keyNode, "ConstraintName", key);
                DataColumn[] columns = this.BuildKey(keyNode, tableSmart);
                if (0 < columns.Length)
                {
                    UniqueConstraint instance = (UniqueConstraint) columns[0].Table.Constraints.FindConstraint(new UniqueConstraint(key, columns));
                    if (instance == null)
                    {
                        columns[0].Table.Constraints.Add(key, columns, primaryKey);
                        SetExtProperties(columns[0].Table.Constraints[key], keyNode.UnhandledAttributes);
                    }
                    else
                    {
                        columns = instance.ColumnsReference;
                        SetExtProperties(instance, keyNode.UnhandledAttributes);
                        if (primaryKey)
                        {
                            columns[0].Table.PrimaryKey = columns;
                        }
                    }
                    if (keyNode is XmlSchemaKey)
                    {
                        for (int i = 0; i < columns.Length; i++)
                        {
                            columns[i].AllowDBNull = false;
                        }
                    }
                }
            }
        }

        internal void HandleDataSet(XmlSchemaElement node, bool isNewDataSet)
        {
            string name = node.Name;
            string str3 = node.QualifiedName.Namespace;
            int count = this._ds.Tables.Count;
            List<DataTable> list = new List<DataTable>();
            string msdataAttribute = GetMsdataAttribute(node, "Locale");
            if (msdataAttribute != null)
            {
                if (msdataAttribute.Length != 0)
                {
                    this._ds.Locale = new CultureInfo(msdataAttribute);
                }
                else
                {
                    this._ds.Locale = CultureInfo.InvariantCulture;
                }
            }
            else if (this.GetBooleanAttribute(node, "UseCurrentLocale", false))
            {
                this._ds.SetLocaleValue(CultureInfo.CurrentCulture, false);
            }
            else
            {
                this._ds.SetLocaleValue(new CultureInfo(0x409), false);
            }
            msdataAttribute = GetMsdataAttribute(node, "DataSetName");
            if ((msdataAttribute != null) && (msdataAttribute.Length != 0))
            {
                name = msdataAttribute;
            }
            msdataAttribute = GetMsdataAttribute(node, "DataSetNamespace");
            if ((msdataAttribute != null) && (msdataAttribute.Length != 0))
            {
                str3 = msdataAttribute;
            }
            SetProperties(this._ds, node.UnhandledAttributes);
            SetExtProperties(this._ds, node.UnhandledAttributes);
            if ((name != null) && (name.Length != 0))
            {
                this._ds.DataSetName = XmlConvert.DecodeName(name);
            }
            this._ds.Namespace = str3;
            if (this.FromInference)
            {
                this._ds.Prefix = this.GetPrefix(this._ds.Namespace);
            }
            XmlSchemaComplexType type = (XmlSchemaComplexType) this.FindTypeNode(node);
            if (type.Particle != null)
            {
                XmlSchemaObjectCollection particleItems = this.GetParticleItems(type.Particle);
                if (particleItems == null)
                {
                    return;
                }
                foreach (XmlSchemaAnnotated annotated in particleItems)
                {
                    if (annotated is XmlSchemaElement)
                    {
                        if (((XmlSchemaElement) annotated).RefName.Name.Length != 0)
                        {
                            if (!this.FromInference)
                            {
                                continue;
                            }
                            DataTable table3 = this._ds.Tables.GetTable(XmlConvert.DecodeName(this.GetInstanceName((XmlSchemaElement) annotated)), node.QualifiedName.Namespace);
                            if (table3 != null)
                            {
                                list.Add(table3);
                            }
                            bool flag = false;
                            if ((node.ElementSchemaType != null) || !(((XmlSchemaElement) annotated).SchemaType is XmlSchemaComplexType))
                            {
                                flag = true;
                            }
                            if ((((XmlSchemaElement) annotated).MaxOccurs != 1M) && !flag)
                            {
                                continue;
                            }
                        }
                        DataTable item = this.HandleTable((XmlSchemaElement) annotated);
                        if (item != null)
                        {
                            item.fNestedInDataset = true;
                        }
                        if (this.FromInference)
                        {
                            list.Add(item);
                        }
                    }
                    else if (annotated is XmlSchemaChoice)
                    {
                        XmlSchemaObjectCollection items = ((XmlSchemaChoice) annotated).Items;
                        if (items != null)
                        {
                            foreach (XmlSchemaAnnotated annotated2 in items)
                            {
                                if (annotated2 is XmlSchemaElement)
                                {
                                    if ((((XmlSchemaParticle) annotated).MaxOccurs > 1M) && (((XmlSchemaElement) annotated2).SchemaType is XmlSchemaComplexType))
                                    {
                                        ((XmlSchemaElement) annotated2).MaxOccurs = ((XmlSchemaParticle) annotated).MaxOccurs;
                                    }
                                    if (((((XmlSchemaElement) annotated2).RefName.Name.Length == 0) || this.FromInference) || ((((XmlSchemaElement) annotated2).MaxOccurs == 1M) || (((XmlSchemaElement) annotated2).SchemaType is XmlSchemaComplexType)))
                                    {
                                        DataTable table = this.HandleTable((XmlSchemaElement) annotated2);
                                        if (this.FromInference)
                                        {
                                            list.Add(table);
                                        }
                                        if (table != null)
                                        {
                                            table.fNestedInDataset = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (node.Constraints != null)
            {
                foreach (XmlSchemaIdentityConstraint constraint in node.Constraints)
                {
                    XmlSchemaKeyref element = constraint as XmlSchemaKeyref;
                    if ((element != null) && !this.GetBooleanAttribute(element, "IsNested", false))
                    {
                        this.HandleKeyref(element);
                    }
                }
            }
            if (this.FromInference && isNewDataSet)
            {
                List<DataTable> tableList = new List<DataTable>(this._ds.Tables.Count);
                foreach (DataTable table4 in list)
                {
                    this.AddTablesToList(tableList, table4);
                }
                this._ds.Tables.ReplaceFromInference(tableList);
            }
        }

        internal void HandleElementColumn(XmlSchemaElement elem, DataTable table, bool isBase)
        {
            Type dataType = null;
            XmlSchemaElement node = (elem.Name != null) ? elem : ((XmlSchemaElement) this.elementsTable[elem.RefName]);
            if (node != null)
            {
                DataColumn column;
                XmlSchemaAnnotated annotated = this.FindTypeNode(node);
                string str = null;
                SimpleType type = null;
                if (annotated == null)
                {
                    str = node.SchemaTypeName.Name;
                    if (ADP.IsEmpty(str))
                    {
                        str = "";
                        dataType = typeof(string);
                    }
                    else
                    {
                        dataType = this.ParseDataType(node.SchemaTypeName.Name);
                    }
                }
                else if (annotated is XmlSchemaSimpleType)
                {
                    XmlSchemaSimpleType type3 = annotated as XmlSchemaSimpleType;
                    type = new SimpleType(type3);
                    if (((((XmlSchemaSimpleType) annotated).Name != null) && (((XmlSchemaSimpleType) annotated).Name.Length != 0)) && (((XmlSchemaSimpleType) annotated).QualifiedName.Namespace != "http://www.w3.org/2001/XMLSchema"))
                    {
                        GetMsdataAttribute(annotated, "targetNamespace");
                        str = ((XmlSchemaSimpleType) annotated).QualifiedName.ToString();
                        dataType = this.ParseDataType(str);
                    }
                    else
                    {
                        for (type3 = ((type.XmlBaseType != null) && (type.XmlBaseType.Namespace != "http://www.w3.org/2001/XMLSchema")) ? (this.schemaTypes[type.XmlBaseType] as XmlSchemaSimpleType) : null; type3 != null; type3 = ((type.XmlBaseType != null) && (type.XmlBaseType.Namespace != "http://www.w3.org/2001/XMLSchema")) ? (this.schemaTypes[type.XmlBaseType] as XmlSchemaSimpleType) : null)
                        {
                            type.LoadTypeValues(type3);
                        }
                        dataType = this.ParseDataType(type.BaseType);
                        str = type.Name;
                        if ((type.Length == 1) && (dataType == typeof(string)))
                        {
                            dataType = typeof(char);
                        }
                    }
                }
                else if (annotated is XmlSchemaElement)
                {
                    str = ((XmlSchemaElement) annotated).SchemaTypeName.Name;
                    dataType = this.ParseDataType(str);
                }
                else if (annotated is XmlSchemaComplexType)
                {
                    if (ADP.IsEmpty(GetMsdataAttribute(elem, "DataType")))
                    {
                        throw ExceptionBuilder.DatatypeNotDefined();
                    }
                    dataType = typeof(object);
                }
                else
                {
                    if (annotated.Id == null)
                    {
                        throw ExceptionBuilder.DatatypeNotDefined();
                    }
                    throw ExceptionBuilder.UndefinedDatatype(annotated.Id);
                }
                string name = XmlConvert.DecodeName(this.GetInstanceName(node));
                bool flag = true;
                if ((!isBase || this.FromInference) && table.Columns.Contains(name, true))
                {
                    column = table.Columns[name];
                    flag = false;
                    if (this.FromInference)
                    {
                        if (column.ColumnMapping != MappingType.Element)
                        {
                            throw ExceptionBuilder.ColumnTypeConflict(column.ColumnName);
                        }
                        if ((ADP.IsEmpty(elem.QualifiedName.Namespace) && ADP.IsEmpty(column._columnUri)) || (string.Compare(elem.QualifiedName.Namespace, column.Namespace, StringComparison.Ordinal) == 0))
                        {
                            return;
                        }
                        column = new DataColumn(name, dataType, null, MappingType.Element);
                        flag = true;
                    }
                }
                else
                {
                    column = new DataColumn(name, dataType, null, MappingType.Element);
                }
                SetProperties(column, node.UnhandledAttributes);
                this.HandleColumnExpression(column, node.UnhandledAttributes);
                SetExtProperties(column, node.UnhandledAttributes);
                if (!ADP.IsEmpty(column.Expression))
                {
                    this.ColumnExpressions.Add(column);
                }
                if (((type != null) && (type.Name != null)) && (type.Name.Length > 0))
                {
                    if (GetMsdataAttribute(annotated, "targetNamespace") != null)
                    {
                        column.XmlDataType = type.SimpleTypeQualifiedName;
                    }
                }
                else
                {
                    column.XmlDataType = str;
                }
                column.SimpleType = type;
                column.AllowDBNull = (this.FromInference || (elem.MinOccurs == 0M)) || elem.IsNillable;
                if (!elem.RefName.IsEmpty || (elem.QualifiedName.Namespace != table.Namespace))
                {
                    column.Namespace = elem.QualifiedName.Namespace;
                    column.Namespace = this.GetStringAttribute(node, "targetNamespace", column.Namespace);
                }
                else if (elem.Form == XmlSchemaForm.Unqualified)
                {
                    column.Namespace = string.Empty;
                }
                else if (elem.Form == XmlSchemaForm.None)
                {
                    XmlSchemaObject parent = elem.Parent;
                    while (parent.Parent != null)
                    {
                        parent = parent.Parent;
                    }
                    if (((XmlSchema) parent).ElementFormDefault == XmlSchemaForm.Unqualified)
                    {
                        column.Namespace = string.Empty;
                    }
                }
                else
                {
                    column.Namespace = elem.QualifiedName.Namespace;
                    column.Namespace = this.GetStringAttribute(node, "targetNamespace", column.Namespace);
                }
                int index = (int) Convert.ChangeType(this.GetStringAttribute(elem, "Ordinal", "-1"), typeof(int), null);
                if (flag)
                {
                    if ((index > -1) && (index < table.Columns.Count))
                    {
                        table.Columns.AddAt(index, column);
                    }
                    else
                    {
                        table.Columns.Add(column);
                    }
                }
                if (column.Namespace == table.Namespace)
                {
                    column._columnUri = null;
                }
                if (this.FromInference)
                {
                    column.Prefix = this.GetPrefix(column.Namespace);
                }
                string defaultValue = node.DefaultValue;
                if (defaultValue != null)
                {
                    try
                    {
                        column.DefaultValue = column.ConvertXmlToObject(defaultValue);
                    }
                    catch (FormatException)
                    {
                        throw ExceptionBuilder.CannotConvert(defaultValue, dataType.FullName);
                    }
                }
            }
        }

        internal void HandleKeyref(XmlSchemaKeyref keyref)
        {
            string str3 = XmlConvert.DecodeName(keyref.Refer.Name);
            string defVal = XmlConvert.DecodeName(keyref.Name);
            defVal = this.GetStringAttribute(keyref, "ConstraintName", defVal);
            string tableName = this.GetTableName(keyref);
            string msdataAttribute = GetMsdataAttribute(keyref, "TableNamespace");
            DataTable tableSmart = this._ds.Tables.GetTableSmart(tableName, msdataAttribute);
            if (tableSmart != null)
            {
                if ((str3 == null) || (str3.Length == 0))
                {
                    throw ExceptionBuilder.MissingRefer(defVal);
                }
                ConstraintTable table = (ConstraintTable) this.ConstraintNodes[str3];
                if (table == null)
                {
                    throw ExceptionBuilder.InvalidKey(defVal);
                }
                DataColumn[] parentColumns = this.BuildKey(table.constraint, table.table);
                DataColumn[] childColumns = this.BuildKey(keyref, tableSmart);
                ForeignKeyConstraint childKeyConstraint = null;
                if (this.GetBooleanAttribute(keyref, "ConstraintOnly", false))
                {
                    int num2 = childColumns[0].Table.Constraints.InternalIndexOf(defVal);
                    if ((num2 > -1) && (childColumns[0].Table.Constraints[num2].ConstraintName != defVal))
                    {
                        num2 = -1;
                    }
                    if (num2 < 0)
                    {
                        childKeyConstraint = new ForeignKeyConstraint(defVal, parentColumns, childColumns);
                        childColumns[0].Table.Constraints.Add(childKeyConstraint);
                    }
                }
                else
                {
                    string name = XmlConvert.DecodeName(this.GetStringAttribute(keyref, "RelationName", keyref.Name));
                    if ((name == null) || (name.Length == 0))
                    {
                        name = defVal;
                    }
                    int num = childColumns[0].Table.DataSet.Relations.InternalIndexOf(name);
                    if ((num > -1) && (childColumns[0].Table.DataSet.Relations[num].RelationName != name))
                    {
                        num = -1;
                    }
                    DataRelation instance = null;
                    if (num < 0)
                    {
                        instance = new DataRelation(name, parentColumns, childColumns);
                        SetExtProperties(instance, keyref.UnhandledAttributes);
                        parentColumns[0].Table.DataSet.Relations.Add(instance);
                        if ((this.FromInference && instance.Nested) && this.tableDictionary.ContainsKey(instance.ParentTable))
                        {
                            this.tableDictionary[instance.ParentTable].Add(instance.ChildTable);
                        }
                        childKeyConstraint = instance.ChildKeyConstraint;
                        childKeyConstraint.ConstraintName = defVal;
                    }
                    else
                    {
                        instance = childColumns[0].Table.DataSet.Relations[num];
                    }
                    if (this.GetBooleanAttribute(keyref, "IsNested", false))
                    {
                        instance.Nested = true;
                    }
                }
                string strRule = GetMsdataAttribute(keyref, "AcceptRejectRule");
                string str5 = GetMsdataAttribute(keyref, "UpdateRule");
                string str4 = GetMsdataAttribute(keyref, "DeleteRule");
                if (childKeyConstraint != null)
                {
                    if (strRule != null)
                    {
                        childKeyConstraint.AcceptRejectRule = TranslateAcceptRejectRule(strRule);
                    }
                    if (str5 != null)
                    {
                        childKeyConstraint.UpdateRule = TranslateRule(str5);
                    }
                    if (str4 != null)
                    {
                        childKeyConstraint.DeleteRule = TranslateRule(str4);
                    }
                    SetExtProperties(childKeyConstraint, keyref.UnhandledAttributes);
                }
            }
        }

        internal void HandleParticle(XmlSchemaParticle pt, DataTable table, ArrayList tableChildren, bool isBase)
        {
            XmlSchemaObjectCollection particleItems = this.GetParticleItems(pt);
            if (particleItems != null)
            {
                foreach (XmlSchemaAnnotated annotated in particleItems)
                {
                    XmlSchemaElement node = annotated as XmlSchemaElement;
                    if (node != null)
                    {
                        if ((this.FromInference && (pt is XmlSchemaChoice)) && ((pt.MaxOccurs > 1M) && (node.SchemaType is XmlSchemaComplexType)))
                        {
                            node.MaxOccurs = pt.MaxOccurs;
                        }
                        DataTable table2 = null;
                        if ((((node.Name == null) && (node.RefName.Name == table.EncodedTableName)) && (node.RefName.Namespace == table.Namespace)) || (this.IsTable(node) && (node.Name == table.TableName)))
                        {
                            if (this.FromInference)
                            {
                                table2 = this.HandleTable(node);
                            }
                            else
                            {
                                table2 = table;
                            }
                        }
                        else
                        {
                            table2 = this.HandleTable(node);
                            if (((table2 == null) && this.FromInference) && (node.Name == table.TableName))
                            {
                                table2 = table;
                            }
                        }
                        if (table2 == null)
                        {
                            if (!this.FromInference || (node.Name != table.TableName))
                            {
                                this.HandleElementColumn(node, table, isBase);
                            }
                        }
                        else
                        {
                            DataRelation relation = null;
                            if (node.Annotation != null)
                            {
                                this.HandleRelations(node.Annotation, true);
                            }
                            DataRelationCollection childRelations = table.ChildRelations;
                            for (int i = 0; i < childRelations.Count; i++)
                            {
                                if (childRelations[i].Nested && (table2 == childRelations[i].ChildTable))
                                {
                                    relation = childRelations[i];
                                }
                            }
                            if (relation == null)
                            {
                                tableChildren.Add(table2);
                                if (this.FromInference && (table.UKColumnPositionForInference == -1))
                                {
                                    int num2 = -1;
                                    foreach (DataColumn column in table.Columns)
                                    {
                                        if (column.ColumnMapping == MappingType.Element)
                                        {
                                            num2++;
                                        }
                                    }
                                    table.UKColumnPositionForInference = num2 + 1;
                                }
                            }
                        }
                    }
                    else
                    {
                        this.HandleParticle((XmlSchemaParticle) annotated, table, tableChildren, isBase);
                    }
                }
            }
        }

        internal void HandleRefTableProperties(ArrayList RefTables, XmlSchemaElement element)
        {
            string instanceName = this.GetInstanceName(element);
            DataTable instance = this._ds.Tables.GetTable(XmlConvert.DecodeName(instanceName), element.QualifiedName.Namespace);
            SetProperties(instance, element.UnhandledAttributes);
            SetExtProperties(instance, element.UnhandledAttributes);
        }

        internal void HandleRelation(XmlElement node, bool fNested)
        {
            bool createConstraints = false;
            DataRelationCollection relations = this._ds.Relations;
            string strB = XmlConvert.DecodeName(node.GetAttribute("name"));
            for (int i = 0; i < relations.Count; i++)
            {
                if (string.Compare(relations[i].RelationName, strB, StringComparison.Ordinal) == 0)
                {
                    return;
                }
            }
            string attribute = node.GetAttribute("parent", "urn:schemas-microsoft-com:xml-msdata");
            if ((attribute == null) || (attribute.Length == 0))
            {
                throw ExceptionBuilder.RelationParentNameMissing(strB);
            }
            attribute = XmlConvert.DecodeName(attribute);
            string name = node.GetAttribute("child", "urn:schemas-microsoft-com:xml-msdata");
            if ((name == null) || (name.Length == 0))
            {
                throw ExceptionBuilder.RelationChildNameMissing(strB);
            }
            name = XmlConvert.DecodeName(name);
            string str = node.GetAttribute("parentkey", "urn:schemas-microsoft-com:xml-msdata");
            if ((str == null) || (str.Length == 0))
            {
                throw ExceptionBuilder.RelationTableKeyMissing(strB);
            }
            string[] strArray2 = str.TrimEnd(null).Split(new char[] { ' ', '+' });
            str = node.GetAttribute("childkey", "urn:schemas-microsoft-com:xml-msdata");
            if ((str == null) || (str.Length == 0))
            {
                throw ExceptionBuilder.RelationChildKeyMissing(strB);
            }
            string[] strArray = str.TrimEnd(null).Split(new char[] { ' ', '+' });
            int length = strArray2.Length;
            if (length != strArray.Length)
            {
                throw ExceptionBuilder.MismatchKeyLength();
            }
            DataColumn[] parentColumns = new DataColumn[length];
            DataColumn[] childColumns = new DataColumn[length];
            string ns = node.GetAttribute("ParentTableNamespace", "urn:schemas-microsoft-com:xml-msdata");
            string str5 = node.GetAttribute("ChildTableNamespace", "urn:schemas-microsoft-com:xml-msdata");
            DataTable tableSmart = this._ds.Tables.GetTableSmart(attribute, ns);
            if (tableSmart == null)
            {
                throw ExceptionBuilder.ElementTypeNotFound(attribute);
            }
            DataTable table = this._ds.Tables.GetTableSmart(name, str5);
            if (table == null)
            {
                throw ExceptionBuilder.ElementTypeNotFound(name);
            }
            for (int j = 0; j < length; j++)
            {
                parentColumns[j] = tableSmart.Columns[XmlConvert.DecodeName(strArray2[j])];
                if (parentColumns[j] == null)
                {
                    throw ExceptionBuilder.ElementTypeNotFound(strArray2[j]);
                }
                childColumns[j] = table.Columns[XmlConvert.DecodeName(strArray[j])];
                if (childColumns[j] == null)
                {
                    throw ExceptionBuilder.ElementTypeNotFound(strArray[j]);
                }
            }
            DataRelation instance = new DataRelation(strB, parentColumns, childColumns, createConstraints) {
                Nested = fNested
            };
            SetExtProperties(instance, node.Attributes);
            this._ds.Relations.Add(instance);
            if (this.FromInference && instance.Nested)
            {
                this.tableDictionary[instance.ParentTable].Add(instance.ChildTable);
            }
        }

        private void HandleRelations(XmlSchemaAnnotation ann, bool fNested)
        {
            foreach (object obj2 in ann.Items)
            {
                if (obj2 is XmlSchemaAppInfo)
                {
                    XmlNode[] markup = ((XmlSchemaAppInfo) obj2).Markup;
                    for (int i = 0; i < markup.Length; i++)
                    {
                        if (XMLSchema.FEqualIdentity(markup[i], "Relationship", "urn:schemas-microsoft-com:xml-msdata"))
                        {
                            this.HandleRelation((XmlElement) markup[i], fNested);
                        }
                    }
                }
            }
        }

        internal void HandleSimpleContentColumn(string strType, DataTable table, bool isBase, XmlAttribute[] attrs, bool isNillable)
        {
            if (!this.FromInference || (table.XmlText == null))
            {
                Type dataType = null;
                if (strType != null)
                {
                    DataColumn column;
                    string str;
                    dataType = this.ParseDataType(strType);
                    if (this.FromInference)
                    {
                        int num3 = 0;
                        for (str = table.TableName + "_Text"; table.Columns[str] != null; str = str + num3++)
                        {
                        }
                    }
                    else
                    {
                        str = table.TableName + "_text";
                    }
                    string name = str;
                    bool flag = true;
                    if (!isBase && table.Columns.Contains(name, true))
                    {
                        column = table.Columns[name];
                        flag = false;
                    }
                    else
                    {
                        column = new DataColumn(name, dataType, null, MappingType.SimpleContent);
                    }
                    SetProperties(column, attrs);
                    this.HandleColumnExpression(column, attrs);
                    SetExtProperties(column, attrs);
                    string str4 = "-1";
                    string s = null;
                    column.AllowDBNull = isNillable;
                    if (attrs != null)
                    {
                        for (int i = 0; i < attrs.Length; i++)
                        {
                            if (((attrs[i].LocalName == "AllowDBNull") && (attrs[i].NamespaceURI == "urn:schemas-microsoft-com:xml-msdata")) && (attrs[i].Value == "false"))
                            {
                                column.AllowDBNull = false;
                            }
                            if ((attrs[i].LocalName == "Ordinal") && (attrs[i].NamespaceURI == "urn:schemas-microsoft-com:xml-msdata"))
                            {
                                str4 = attrs[i].Value;
                            }
                            if ((attrs[i].LocalName == "DefaultValue") && (attrs[i].NamespaceURI == "urn:schemas-microsoft-com:xml-msdata"))
                            {
                                s = attrs[i].Value;
                            }
                        }
                    }
                    int index = (int) Convert.ChangeType(str4, typeof(int), null);
                    if ((column.Expression != null) && (column.Expression.Length != 0))
                    {
                        this.ColumnExpressions.Add(column);
                    }
                    column.XmlDataType = strType;
                    column.SimpleType = null;
                    if (this.FromInference)
                    {
                        column.Prefix = this.GetPrefix(column.Namespace);
                    }
                    if (flag)
                    {
                        if (this.FromInference)
                        {
                            column.AllowDBNull = true;
                        }
                        if ((index > -1) && (index < table.Columns.Count))
                        {
                            table.Columns.AddAt(index, column);
                        }
                        else
                        {
                            table.Columns.Add(column);
                        }
                    }
                    if (s != null)
                    {
                        try
                        {
                            column.DefaultValue = column.ConvertXmlToObject(s);
                        }
                        catch (FormatException)
                        {
                            throw ExceptionBuilder.CannotConvert(s, dataType.FullName);
                        }
                    }
                }
            }
        }

        internal void HandleSimpleTypeSimpleContentColumn(XmlSchemaSimpleType typeNode, string strType, DataTable table, bool isBase, XmlAttribute[] attrs, bool isNillable)
        {
            if (!this.FromInference || (table.XmlText == null))
            {
                DataColumn column;
                string str;
                Type dataType = null;
                SimpleType type = null;
                if (((typeNode.QualifiedName.Name != null) && (typeNode.QualifiedName.Name.Length != 0)) && (typeNode.QualifiedName.Namespace != "http://www.w3.org/2001/XMLSchema"))
                {
                    type = new SimpleType(typeNode);
                    strType = typeNode.QualifiedName.ToString();
                    dataType = this.ParseDataType(typeNode.QualifiedName.ToString());
                }
                else
                {
                    XmlSchemaSimpleType baseXmlSchemaType = typeNode.BaseXmlSchemaType as XmlSchemaSimpleType;
                    if ((baseXmlSchemaType != null) && (baseXmlSchemaType.QualifiedName.Namespace != "http://www.w3.org/2001/XMLSchema"))
                    {
                        type = new SimpleType(typeNode);
                        SimpleType baseSimpleType = type;
                        while (baseSimpleType.BaseSimpleType != null)
                        {
                            baseSimpleType = baseSimpleType.BaseSimpleType;
                        }
                        dataType = this.ParseDataType(baseSimpleType.BaseType);
                        strType = type.Name;
                    }
                    else
                    {
                        dataType = this.ParseDataType(strType);
                    }
                }
                if (this.FromInference)
                {
                    int num3 = 0;
                    for (str = table.TableName + "_Text"; table.Columns[str] != null; str = str + num3++)
                    {
                    }
                }
                else
                {
                    str = table.TableName + "_text";
                }
                string name = str;
                bool flag = true;
                if (!isBase && table.Columns.Contains(name, true))
                {
                    column = table.Columns[name];
                    flag = false;
                }
                else
                {
                    column = new DataColumn(name, dataType, null, MappingType.SimpleContent);
                }
                SetProperties(column, attrs);
                this.HandleColumnExpression(column, attrs);
                SetExtProperties(column, attrs);
                string str4 = "-1";
                string s = null;
                column.AllowDBNull = isNillable;
                if (attrs != null)
                {
                    for (int i = 0; i < attrs.Length; i++)
                    {
                        if (((attrs[i].LocalName == "AllowDBNull") && (attrs[i].NamespaceURI == "urn:schemas-microsoft-com:xml-msdata")) && (attrs[i].Value == "false"))
                        {
                            column.AllowDBNull = false;
                        }
                        if ((attrs[i].LocalName == "Ordinal") && (attrs[i].NamespaceURI == "urn:schemas-microsoft-com:xml-msdata"))
                        {
                            str4 = attrs[i].Value;
                        }
                        if ((attrs[i].LocalName == "DefaultValue") && (attrs[i].NamespaceURI == "urn:schemas-microsoft-com:xml-msdata"))
                        {
                            s = attrs[i].Value;
                        }
                    }
                }
                int index = (int) Convert.ChangeType(str4, typeof(int), null);
                if ((column.Expression != null) && (column.Expression.Length != 0))
                {
                    this.ColumnExpressions.Add(column);
                }
                if (((type != null) && (type.Name != null)) && (type.Name.Length > 0))
                {
                    if (GetMsdataAttribute(typeNode, "targetNamespace") != null)
                    {
                        column.XmlDataType = type.SimpleTypeQualifiedName;
                    }
                }
                else
                {
                    column.XmlDataType = strType;
                }
                column.SimpleType = type;
                if (flag)
                {
                    if (this.FromInference)
                    {
                        column.Prefix = this.GetPrefix(table.Namespace);
                        column.AllowDBNull = true;
                    }
                    if ((index > -1) && (index < table.Columns.Count))
                    {
                        table.Columns.AddAt(index, column);
                    }
                    else
                    {
                        table.Columns.Add(column);
                    }
                }
                if (s != null)
                {
                    try
                    {
                        column.DefaultValue = column.ConvertXmlToObject(s);
                    }
                    catch (FormatException)
                    {
                        throw ExceptionBuilder.CannotConvert(s, dataType.FullName);
                    }
                }
            }
        }

        internal DataTable HandleTable(XmlSchemaElement node)
        {
            if (!this.IsTable(node))
            {
                return null;
            }
            object obj2 = this.FindTypeNode(node);
            if ((node.MaxOccurs > 1M) && (obj2 == null))
            {
                return this.InstantiateSimpleTable(node);
            }
            DataTable table = this.InstantiateTable(node, (XmlSchemaComplexType) obj2, node.RefName != null);
            table.fNestedInDataset = false;
            return table;
        }

        private bool HasAttributes(XmlSchemaObjectCollection attributes)
        {
            foreach (XmlSchemaObject obj2 in attributes)
            {
                if (obj2 is XmlSchemaAttribute)
                {
                    return true;
                }
                if (obj2 is XmlSchemaAttributeGroup)
                {
                    return true;
                }
                if (obj2 is XmlSchemaAttributeGroupRef)
                {
                    return true;
                }
            }
            return false;
        }

        internal DataTable InstantiateSimpleTable(XmlSchemaElement node)
        {
            string name = XmlConvert.DecodeName(this.GetInstanceName(node));
            string ns = node.QualifiedName.Namespace;
            DataTable instance = this._ds.Tables.GetTable(name, ns);
            if (!this.FromInference && (instance != null))
            {
                throw ExceptionBuilder.DuplicateDeclaration(name);
            }
            if (instance == null)
            {
                instance = new DataTable(name) {
                    Namespace = ns,
                    Namespace = this.GetStringAttribute(node, "targetNamespace", ns)
                };
                if (!this.FromInference)
                {
                    instance.MinOccurs = node.MinOccurs;
                    instance.MaxOccurs = node.MaxOccurs;
                }
                else
                {
                    string prefix = this.GetPrefix(ns);
                    if (prefix != null)
                    {
                        instance.Prefix = prefix;
                    }
                }
                SetProperties(instance, node.UnhandledAttributes);
                SetExtProperties(instance, node.UnhandledAttributes);
            }
            XmlSchemaComplexType schemaType = node.SchemaType as XmlSchemaComplexType;
            bool flag = (node.ElementSchemaType.BaseXmlSchemaType != null) || ((schemaType != null) && (schemaType.ContentModel is XmlSchemaSimpleContent));
            if (!this.FromInference || (flag && (instance.Columns.Count == 0)))
            {
                string str2;
                this.HandleElementColumn(node, instance, false);
                if (this.FromInference)
                {
                    int num = 0;
                    for (str2 = name + "_Text"; instance.Columns[str2] != null; str2 = str2 + num++)
                    {
                    }
                }
                else
                {
                    str2 = name + "_Column";
                }
                instance.Columns[0].ColumnName = str2;
                instance.Columns[0].ColumnMapping = MappingType.SimpleContent;
            }
            if (!this.FromInference || (this._ds.Tables.GetTable(name, ns) == null))
            {
                this._ds.Tables.Add(instance);
                if (this.FromInference)
                {
                    this.tableDictionary.Add(instance, new List<DataTable>());
                }
            }
            if ((this.dsElement != null) && (this.dsElement.Constraints != null))
            {
                foreach (XmlSchemaIdentityConstraint constraint in this.dsElement.Constraints)
                {
                    if (!(constraint is XmlSchemaKeyref) && (this.GetTableName(constraint) == instance.TableName))
                    {
                        this.HandleConstraint(constraint);
                    }
                }
            }
            instance.fNestedInDataset = false;
            return instance;
        }

        internal DataTable InstantiateTable(XmlSchemaElement node, XmlSchemaComplexType typeNode, bool isRef)
        {
            string instanceName = this.GetInstanceName(node);
            ArrayList tableChildren = new ArrayList();
            string ns = node.QualifiedName.Namespace;
            DataTable table = this._ds.Tables.GetTable(XmlConvert.DecodeName(instanceName), ns);
            if (!this.FromInference || (this.FromInference && (table == null)))
            {
                if (table != null)
                {
                    if (!isRef)
                    {
                        throw ExceptionBuilder.DuplicateDeclaration(instanceName);
                    }
                    return table;
                }
                if (isRef)
                {
                    this.RefTables.Add(ns + ":" + instanceName);
                }
                table = new DataTable(XmlConvert.DecodeName(instanceName)) {
                    TypeName = node.SchemaTypeName,
                    Namespace = ns,
                    Namespace = this.GetStringAttribute(node, "targetNamespace", ns)
                };
                string name = this.GetStringAttribute(typeNode, "CaseSensitive", "");
                if (name.Length == 0)
                {
                    name = this.GetStringAttribute(node, "CaseSensitive", "");
                }
                if (0 < name.Length)
                {
                    if ((name == "true") || (name == "True"))
                    {
                        table.CaseSensitive = true;
                    }
                    if ((name == "false") || (name == "False"))
                    {
                        table.CaseSensitive = false;
                    }
                }
                name = GetMsdataAttribute(node, "Locale");
                if (name != null)
                {
                    if (0 < name.Length)
                    {
                        table.Locale = new CultureInfo(name);
                    }
                    else
                    {
                        table.Locale = CultureInfo.InvariantCulture;
                    }
                }
                if (!this.FromInference)
                {
                    table.MinOccurs = node.MinOccurs;
                    table.MaxOccurs = node.MaxOccurs;
                }
                else
                {
                    string prefix = this.GetPrefix(ns);
                    if (prefix != null)
                    {
                        table.Prefix = prefix;
                    }
                }
                this._ds.Tables.Add(table);
                if (this.FromInference)
                {
                    this.tableDictionary.Add(table, new List<DataTable>());
                }
            }
            this.HandleComplexType(typeNode, table, tableChildren, node.IsNillable);
            for (int i = 0; i < table.Columns.Count; i++)
            {
                table.Columns[i].SetOrdinalInternal(i);
            }
            SetProperties(table, node.UnhandledAttributes);
            SetExtProperties(table, node.UnhandledAttributes);
            if ((this.dsElement != null) && (this.dsElement.Constraints != null))
            {
                foreach (XmlSchemaIdentityConstraint constraint in this.dsElement.Constraints)
                {
                    if ((!(constraint is XmlSchemaKeyref) && (this.GetTableName(constraint) == table.TableName)) && ((this.GetTableNamespace(constraint) == table.Namespace) || (this.GetTableNamespace(constraint) == null)))
                    {
                        this.HandleConstraint(constraint);
                    }
                }
            }
            foreach (DataTable table2 in tableChildren)
            {
                if ((table2 != table) && (table.Namespace == table2.Namespace))
                {
                    table2.tableNamespace = null;
                }
                if ((this.dsElement != null) && (this.dsElement.Constraints != null))
                {
                    foreach (XmlSchemaIdentityConstraint constraint2 in this.dsElement.Constraints)
                    {
                        XmlSchemaKeyref element = constraint2 as XmlSchemaKeyref;
                        if (((element != null) && this.GetBooleanAttribute(element, "IsNested", false)) && (this.GetTableName(element) == table2.TableName))
                        {
                            if (table2.DataSet.Tables.InternalIndexOf(table2.TableName) < -1)
                            {
                                if (this.GetTableNamespace(element) == table2.Namespace)
                                {
                                    this.HandleKeyref(element);
                                }
                            }
                            else
                            {
                                this.HandleKeyref(element);
                            }
                        }
                    }
                }
                DataRelation relation = null;
                DataRelationCollection childRelations = table.ChildRelations;
                for (int j = 0; j < childRelations.Count; j++)
                {
                    if (childRelations[j].Nested && (table2 == childRelations[j].ChildTable))
                    {
                        relation = childRelations[j];
                    }
                }
                if (relation == null)
                {
                    DataColumn column;
                    if (this.FromInference)
                    {
                        int uKColumnPositionForInference = table.UKColumnPositionForInference;
                        if (uKColumnPositionForInference == -1)
                        {
                            foreach (DataColumn column3 in table.Columns)
                            {
                                if (column3.ColumnMapping == MappingType.Attribute)
                                {
                                    uKColumnPositionForInference = column3.Ordinal;
                                    break;
                                }
                            }
                        }
                        column = table.AddUniqueKey(uKColumnPositionForInference);
                    }
                    else
                    {
                        column = table.AddUniqueKey();
                    }
                    DataColumn childColumn = table2.AddForeignKey(column);
                    if (this.FromInference)
                    {
                        childColumn.Prefix = table2.Prefix;
                    }
                    relation = new DataRelation(table.TableName + "_" + table2.TableName, column, childColumn, true) {
                        Nested = true
                    };
                    table2.DataSet.Relations.Add(relation);
                    if ((this.FromInference && relation.Nested) && this.tableDictionary.ContainsKey(relation.ParentTable))
                    {
                        this.tableDictionary[relation.ParentTable].Add(relation.ChildTable);
                    }
                }
            }
            return table;
        }

        private bool IsDatasetParticle(XmlSchemaParticle pt)
        {
            XmlSchemaObjectCollection particleItems = this.GetParticleItems(pt);
            if (particleItems == null)
            {
                return false;
            }
            bool flag2 = this.FromInference && (pt is XmlSchemaChoice);
            foreach (XmlSchemaAnnotated annotated in particleItems)
            {
                if (annotated is XmlSchemaElement)
                {
                    if ((flag2 && (pt.MaxOccurs > 1M)) && (((XmlSchemaElement) annotated).SchemaType is XmlSchemaComplexType))
                    {
                        ((XmlSchemaElement) annotated).MaxOccurs = pt.MaxOccurs;
                    }
                    if (((((XmlSchemaElement) annotated).RefName.Name.Length == 0) || (this.FromInference && ((((XmlSchemaElement) annotated).MaxOccurs == 1M) || (((XmlSchemaElement) annotated).SchemaType is XmlSchemaComplexType)))) && !this.IsTable((XmlSchemaElement) annotated))
                    {
                        return false;
                    }
                }
                else if ((annotated is XmlSchemaParticle) && !this.IsDatasetParticle((XmlSchemaParticle) annotated))
                {
                    return false;
                }
            }
            return true;
        }

        internal bool IsTable(XmlSchemaElement node)
        {
            if (node.MaxOccurs == 0M)
            {
                return false;
            }
            XmlAttribute[] unhandledAttributes = node.UnhandledAttributes;
            if (unhandledAttributes != null)
            {
                for (int i = 0; i < unhandledAttributes.Length; i++)
                {
                    XmlAttribute attribute = unhandledAttributes[i];
                    if (((attribute.LocalName == "DataType") && (attribute.Prefix == "msdata")) && (attribute.NamespaceURI == "urn:schemas-microsoft-com:xml-msdata"))
                    {
                        return false;
                    }
                }
            }
            object obj2 = this.FindTypeNode(node);
            if ((node.MaxOccurs <= 1M) || (obj2 != null))
            {
                if ((obj2 == null) || !(obj2 is XmlSchemaComplexType))
                {
                    return false;
                }
                XmlSchemaComplexType type = (XmlSchemaComplexType) obj2;
                if (type.IsAbstract)
                {
                    throw ExceptionBuilder.CannotInstantiateAbstract(node.Name);
                }
            }
            return true;
        }

        internal static bool IsXsdType(string name)
        {
            if (Array.BinarySearch(mapNameTypeXsd, name) < 0)
            {
                return false;
            }
            return true;
        }

        public void LoadSchema(XmlSchemaSet schemaSet, DataSet ds)
        {
            this.ConstraintNodes = new Hashtable();
            this.RefTables = new ArrayList();
            this.ColumnExpressions = new ArrayList();
            this.complexTypes = new ArrayList();
            bool flag = false;
            bool isNewDataSet = ds.Tables.Count == 0;
            if (schemaSet != null)
            {
                this._schemaSet = schemaSet;
                this._ds = ds;
                ds.fIsSchemaLoading = true;
                foreach (XmlSchema schema in schemaSet.Schemas())
                {
                    this._schemaName = schema.Id;
                    if ((this._schemaName == null) || (this._schemaName.Length == 0))
                    {
                        this._schemaName = "NewDataSet";
                    }
                    ds.DataSetName = XmlConvert.DecodeName(this._schemaName);
                    string targetNamespace = schema.TargetNamespace;
                    if ((ds.namespaceURI == null) || (ds.namespaceURI.Length == 0))
                    {
                        ds.namespaceURI = (targetNamespace == null) ? string.Empty : targetNamespace;
                    }
                    break;
                }
                this.annotations = new XmlSchemaObjectCollection();
                this.elements = new XmlSchemaObjectCollection();
                this.elementsTable = new Hashtable();
                this.attributes = new Hashtable();
                this.attributeGroups = new Hashtable();
                this.schemaTypes = new Hashtable();
                this.tableDictionary = new Dictionary<DataTable, List<DataTable>>();
                this.existingSimpleTypeMap = new Hashtable();
                foreach (DataTable table3 in ds.Tables)
                {
                    foreach (DataColumn column in table3.Columns)
                    {
                        if (((column.SimpleType != null) && (column.SimpleType.Name != null)) && (column.SimpleType.Name.Length != 0))
                        {
                            this.existingSimpleTypeMap[column.SimpleType.SimpleTypeQualifiedName] = column;
                        }
                    }
                }
                foreach (XmlSchema schema3 in schemaSet.Schemas())
                {
                    this.CollectElementsAnnotations(schema3);
                }
                this.dsElement = this.FindDatasetElement(this.elements);
                if (this.dsElement != null)
                {
                    string name = this.GetStringAttribute(this.dsElement, "MainDataTable", "");
                    if (name != null)
                    {
                        ds.MainTableName = XmlConvert.DecodeName(name);
                    }
                }
                else
                {
                    if (this.FromInference)
                    {
                        ds.fTopLevelTable = true;
                    }
                    flag = true;
                }
                List<XmlQualifiedName> list = new List<XmlQualifiedName>();
                if ((ds != null) && ds.UseDataSetSchemaOnly)
                {
                    int num3 = this.DatasetElementCount(this.elements);
                    if (num3 == 0)
                    {
                        throw ExceptionBuilder.IsDataSetAttributeMissingInSchema();
                    }
                    if (num3 > 1)
                    {
                        throw ExceptionBuilder.TooManyIsDataSetAtributeInSchema();
                    }
                    XmlSchemaComplexType type = (XmlSchemaComplexType) this.FindTypeNode(this.dsElement);
                    if (type.Particle != null)
                    {
                        XmlSchemaObjectCollection particleItems = this.GetParticleItems(type.Particle);
                        if (particleItems != null)
                        {
                            foreach (XmlSchemaAnnotated annotated in particleItems)
                            {
                                XmlSchemaElement element2 = annotated as XmlSchemaElement;
                                if ((element2 != null) && (element2.RefName.Name.Length != 0))
                                {
                                    list.Add(element2.QualifiedName);
                                }
                            }
                        }
                    }
                }
                foreach (XmlSchemaElement element in this.elements)
                {
                    if ((element != this.dsElement) && (((ds == null) || !ds.UseDataSetSchemaOnly) || (((this.dsElement == null) || (this.dsElement.Parent == element.Parent)) || list.Contains(element.QualifiedName))))
                    {
                        string instanceName = this.GetInstanceName(element);
                        if (this.RefTables.Contains(element.QualifiedName.Namespace + ":" + instanceName))
                        {
                            this.HandleRefTableProperties(this.RefTables, element);
                        }
                        else
                        {
                            this.HandleTable(element);
                        }
                    }
                }
                if (this.dsElement != null)
                {
                    this.HandleDataSet(this.dsElement, isNewDataSet);
                }
                foreach (XmlSchemaAnnotation annotation in this.annotations)
                {
                    this.HandleRelations(annotation, false);
                }
                for (int i = 0; i < this.ColumnExpressions.Count; i++)
                {
                    DataColumn column2 = (DataColumn) this.ColumnExpressions[i];
                    column2.Expression = (string) this.expressions[column2];
                }
                foreach (DataTable table in ds.Tables)
                {
                    if ((table.NestedParentRelations.Length == 0) && (table.Namespace == ds.Namespace))
                    {
                        DataRelationCollection childRelations = table.ChildRelations;
                        for (int j = 0; j < childRelations.Count; j++)
                        {
                            if (childRelations[j].Nested && (table.Namespace == childRelations[j].ChildTable.Namespace))
                            {
                                childRelations[j].ChildTable.tableNamespace = null;
                            }
                        }
                        table.tableNamespace = null;
                    }
                }
                DataTable table2 = ds.Tables[ds.DataSetName, ds.Namespace];
                if (table2 != null)
                {
                    table2.fNestedInDataset = true;
                }
                if ((this.FromInference && (ds.Tables.Count == 0)) && (string.Compare(ds.DataSetName, "NewDataSet", StringComparison.Ordinal) == 0))
                {
                    ds.DataSetName = XmlConvert.DecodeName(((XmlSchemaElement) this.elements[0]).Name);
                }
                ds.fIsSchemaLoading = false;
                if (flag)
                {
                    if (ds.Tables.Count > 0)
                    {
                        ds.Namespace = ds.Tables[0].Namespace;
                        ds.Prefix = ds.Tables[0].Prefix;
                    }
                    else
                    {
                        foreach (XmlSchema schema2 in schemaSet.Schemas())
                        {
                            ds.Namespace = schema2.TargetNamespace;
                        }
                    }
                }
            }
        }

        public void LoadSchema(XmlSchemaSet schemaSet, DataTable dt)
        {
            if (dt.DataSet != null)
            {
                this.LoadSchema(schemaSet, dt.DataSet);
            }
        }

        private Type ParseDataType(string dt)
        {
            if (IsXsdType(dt) || (this.udSimpleTypes == null))
            {
                return FindNameType(dt).type;
            }
            XmlSchemaSimpleType node = (XmlSchemaSimpleType) this.udSimpleTypes[dt];
            if (node == null)
            {
                throw ExceptionBuilder.UndefinedDatatype(dt);
            }
            SimpleType baseSimpleType = new SimpleType(node);
            while (baseSimpleType.BaseSimpleType != null)
            {
                baseSimpleType = baseSimpleType.BaseSimpleType;
            }
            return this.ParseDataType(baseSimpleType.BaseType);
        }

        internal static string QualifiedName(string name)
        {
            if (name.IndexOf(':') == -1)
            {
                return ("xs:" + name);
            }
            return name;
        }

        private static void SetExtProperties(object instance, XmlAttribute[] attrs)
        {
            PropertyCollection propertys = null;
            if (attrs != null)
            {
                for (int i = 0; i < attrs.Length; i++)
                {
                    if (attrs[i].NamespaceURI == "urn:schemas-microsoft-com:xml-msprop")
                    {
                        if (propertys == null)
                        {
                            propertys = (PropertyCollection) TypeDescriptor.GetProperties(instance)["ExtendedProperties"].GetValue(instance);
                        }
                        string key = XmlConvert.DecodeName(attrs[i].LocalName);
                        if (instance is ForeignKeyConstraint)
                        {
                            if (!key.StartsWith("fk_", StringComparison.Ordinal))
                            {
                                continue;
                            }
                            key = key.Substring(3);
                        }
                        if ((instance is DataRelation) && key.StartsWith("rel_", StringComparison.Ordinal))
                        {
                            key = key.Substring(4);
                        }
                        else if ((instance is DataRelation) && key.StartsWith("fk_", StringComparison.Ordinal))
                        {
                            continue;
                        }
                        propertys.Add(key, attrs[i].Value);
                    }
                }
            }
        }

        private static void SetExtProperties(object instance, XmlAttributeCollection attrs)
        {
            PropertyCollection propertys = null;
            for (int i = 0; i < attrs.Count; i++)
            {
                if (attrs[i].NamespaceURI == "urn:schemas-microsoft-com:xml-msprop")
                {
                    if (propertys == null)
                    {
                        propertys = (PropertyCollection) TypeDescriptor.GetProperties(instance)["ExtendedProperties"].GetValue(instance);
                    }
                    string key = XmlConvert.DecodeName(attrs[i].LocalName);
                    propertys.Add(key, attrs[i].Value);
                }
            }
        }

        internal static void SetProperties(object instance, XmlAttribute[] attrs)
        {
            if (attrs != null)
            {
                for (int i = 0; i < attrs.Length; i++)
                {
                    if (attrs[i].NamespaceURI == "urn:schemas-microsoft-com:xml-msdata")
                    {
                        string localName = attrs[i].LocalName;
                        string str2 = attrs[i].Value;
                        if ((((localName != "DefaultValue") && (localName != "Ordinal")) && ((localName != "Locale") && (localName != "RemotingFormat"))) && ((localName != "Expression") || !(instance is DataColumn)))
                        {
                            if (localName == "DataType")
                            {
                                DataColumn column = instance as DataColumn;
                                if (column != null)
                                {
                                    column.DataType = DataStorage.GetType(str2);
                                }
                            }
                            else
                            {
                                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(instance)[localName];
                                if (descriptor != null)
                                {
                                    object type;
                                    Type propertyType = descriptor.PropertyType;
                                    TypeConverter converter = XMLSchema.GetConverter(propertyType);
                                    if (converter.CanConvertFrom(typeof(string)))
                                    {
                                        type = converter.ConvertFromString(str2);
                                    }
                                    else if (propertyType == typeof(Type))
                                    {
                                        type = Type.GetType(str2);
                                    }
                                    else
                                    {
                                        if (propertyType != typeof(CultureInfo))
                                        {
                                            throw ExceptionBuilder.CannotConvert(str2, propertyType.FullName);
                                        }
                                        type = new CultureInfo(str2);
                                    }
                                    descriptor.SetValue(instance, type);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static AcceptRejectRule TranslateAcceptRejectRule(string strRule)
        {
            if (strRule == "Cascade")
            {
                return AcceptRejectRule.Cascade;
            }
            if (strRule == "None")
            {
                return AcceptRejectRule.None;
            }
            return AcceptRejectRule.None;
        }

        internal static Rule TranslateRule(string strRule)
        {
            if (strRule != "Cascade")
            {
                if (strRule == "None")
                {
                    return Rule.None;
                }
                if (strRule == "SetDefault")
                {
                    return Rule.SetDefault;
                }
                if (strRule == "SetNull")
                {
                    return Rule.SetNull;
                }
            }
            return Rule.Cascade;
        }

        public static Type XsdtoClr(string xsdTypeName)
        {
            int index = Array.BinarySearch(mapNameTypeXsd, xsdTypeName);
            if (index < 0)
            {
                throw ExceptionBuilder.UndefinedDatatype(xsdTypeName);
            }
            return mapNameTypeXsd[index].type;
        }

        internal bool FromInference
        {
            get
            {
                return this.fromInference;
            }
            set
            {
                this.fromInference = value;
            }
        }

        private sealed class NameType : IComparable
        {
            public readonly string name;
            public readonly Type type;

            public NameType(string n, Type t)
            {
                this.name = n;
                this.type = t;
            }

            public int CompareTo(object obj)
            {
                return string.Compare(this.name, (string) obj, StringComparison.Ordinal);
            }
        }
    }
}

