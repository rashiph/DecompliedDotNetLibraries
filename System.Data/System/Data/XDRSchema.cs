namespace System.Data
{
    using System;
    using System.Collections;
    using System.Data.Common;
    using System.Globalization;
    using System.Xml;

    internal sealed class XDRSchema : XMLSchema
    {
        internal DataSet _ds;
        internal string _schemaName = string.Empty;
        internal XmlElement _schemaRoot = null;
        internal string _schemaUri = string.Empty;
        private static char[] colonArray = new char[] { ':' };
        private static NameType enumerationNameType = FindNameType("enumeration");
        private static NameType[] mapNameTypeXdr = new NameType[] { 
            new NameType("bin.base64", typeof(byte[])), new NameType("bin.hex", typeof(byte[])), new NameType("boolean", typeof(bool)), new NameType("byte", typeof(sbyte)), new NameType("char", typeof(char)), new NameType("date", typeof(DateTime)), new NameType("dateTime", typeof(DateTime)), new NameType("dateTime.tz", typeof(DateTime)), new NameType("entities", typeof(string)), new NameType("entity", typeof(string)), new NameType("enumeration", typeof(string)), new NameType("fixed.14.4", typeof(decimal)), new NameType("float", typeof(double)), new NameType("i1", typeof(sbyte)), new NameType("i2", typeof(short)), new NameType("i4", typeof(int)), 
            new NameType("i8", typeof(long)), new NameType("id", typeof(string)), new NameType("idref", typeof(string)), new NameType("idrefs", typeof(string)), new NameType("int", typeof(int)), new NameType("nmtoken", typeof(string)), new NameType("nmtokens", typeof(string)), new NameType("notation", typeof(string)), new NameType("number", typeof(decimal)), new NameType("r4", typeof(float)), new NameType("r8", typeof(double)), new NameType("string", typeof(string)), new NameType("time", typeof(DateTime)), new NameType("time.tz", typeof(DateTime)), new NameType("ui1", typeof(byte)), new NameType("ui2", typeof(ushort)), 
            new NameType("ui4", typeof(uint)), new NameType("ui8", typeof(ulong)), new NameType("uri", typeof(string)), new NameType("uuid", typeof(Guid))
         };

        internal XDRSchema(DataSet ds, bool fInline)
        {
            this._ds = ds;
        }

        private static NameType FindNameType(string name)
        {
            int index = Array.BinarySearch(mapNameTypeXdr, name);
            if (index < 0)
            {
                throw ExceptionBuilder.UndefinedDatatype(name);
            }
            return mapNameTypeXdr[index];
        }

        internal XmlElement FindTypeNode(XmlElement node)
        {
            if (XMLSchema.FEqualIdentity(node, "ElementType", "urn:schemas-microsoft-com:xml-data"))
            {
                return node;
            }
            string attribute = node.GetAttribute("type");
            if (XMLSchema.FEqualIdentity(node, "element", "urn:schemas-microsoft-com:xml-data") || XMLSchema.FEqualIdentity(node, "attribute", "urn:schemas-microsoft-com:xml-data"))
            {
                if ((attribute == null) || (attribute.Length == 0))
                {
                    return null;
                }
                XmlNode firstChild = node.OwnerDocument.FirstChild;
                XmlNode ownerDocument = node.OwnerDocument;
                while (firstChild != ownerDocument)
                {
                    if (((XMLSchema.FEqualIdentity(firstChild, "ElementType", "urn:schemas-microsoft-com:xml-data") && XMLSchema.FEqualIdentity(node, "element", "urn:schemas-microsoft-com:xml-data")) || (XMLSchema.FEqualIdentity(firstChild, "AttributeType", "urn:schemas-microsoft-com:xml-data") && XMLSchema.FEqualIdentity(node, "attribute", "urn:schemas-microsoft-com:xml-data"))) && ((firstChild is XmlElement) && (((XmlElement) firstChild).GetAttribute("name") == attribute)))
                    {
                        return (XmlElement) firstChild;
                    }
                    if (firstChild.FirstChild != null)
                    {
                        firstChild = firstChild.FirstChild;
                    }
                    else
                    {
                        if (firstChild.NextSibling == null)
                        {
                            goto Label_0115;
                        }
                        firstChild = firstChild.NextSibling;
                    }
                    continue;
                Label_00FD:
                    firstChild = firstChild.ParentNode;
                    if (firstChild.NextSibling != null)
                    {
                        firstChild = firstChild.NextSibling;
                        continue;
                    }
                Label_0115:
                    if (firstChild != ownerDocument)
                    {
                        goto Label_00FD;
                    }
                }
            }
            return null;
        }

        internal string GetInstanceName(XmlElement node)
        {
            string attribute;
            if (XMLSchema.FEqualIdentity(node, "ElementType", "urn:schemas-microsoft-com:xml-data") || XMLSchema.FEqualIdentity(node, "AttributeType", "urn:schemas-microsoft-com:xml-data"))
            {
                attribute = node.GetAttribute("name");
                if ((attribute == null) || (attribute.Length == 0))
                {
                    throw ExceptionBuilder.MissingAttribute("Element", "name");
                }
                return attribute;
            }
            attribute = node.GetAttribute("type");
            if ((attribute == null) || (attribute.Length == 0))
            {
                throw ExceptionBuilder.MissingAttribute("Element", "type");
            }
            return attribute;
        }

        internal void GetMinMax(XmlElement elNode, ref int minOccurs, ref int maxOccurs)
        {
            this.GetMinMax(elNode, false, ref minOccurs, ref maxOccurs);
        }

        internal void GetMinMax(XmlElement elNode, bool isAttribute, ref int minOccurs, ref int maxOccurs)
        {
            string attribute = elNode.GetAttribute("minOccurs");
            if ((attribute != null) && (attribute.Length > 0))
            {
                try
                {
                    minOccurs = int.Parse(attribute, CultureInfo.InvariantCulture);
                }
                catch (Exception exception2)
                {
                    if (!ADP.IsCatchableExceptionType(exception2))
                    {
                        throw;
                    }
                    throw ExceptionBuilder.AttributeValues("minOccurs", "0", "1");
                }
            }
            attribute = elNode.GetAttribute("maxOccurs");
            if ((attribute != null) && (attribute.Length > 0))
            {
                if (string.Compare(attribute, "*", StringComparison.Ordinal) == 0)
                {
                    maxOccurs = -1;
                }
                else
                {
                    try
                    {
                        maxOccurs = int.Parse(attribute, CultureInfo.InvariantCulture);
                    }
                    catch (Exception exception)
                    {
                        if (!ADP.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                        throw ExceptionBuilder.AttributeValues("maxOccurs", "1", "*");
                    }
                    if (maxOccurs != 1)
                    {
                        throw ExceptionBuilder.AttributeValues("maxOccurs", "1", "*");
                    }
                }
            }
        }

        internal void HandleColumn(XmlElement node, DataTable table)
        {
            DataColumn column;
            Type type;
            string str3;
            string str4;
            int minOccurs = 0;
            int maxOccurs = 1;
            node.GetAttribute("use");
            if (node.Attributes.Count > 0)
            {
                string str7 = node.GetAttribute("ref");
                if ((str7 != null) && (str7.Length > 0))
                {
                    return;
                }
                str3 = str4 = this.GetInstanceName(node);
                column = table.Columns[str4, this._schemaUri];
                if (column != null)
                {
                    if (column.ColumnMapping == MappingType.Attribute)
                    {
                        if (XMLSchema.FEqualIdentity(node, "attribute", "urn:schemas-microsoft-com:xml-data"))
                        {
                            throw ExceptionBuilder.DuplicateDeclaration(str3);
                        }
                    }
                    else if (XMLSchema.FEqualIdentity(node, "element", "urn:schemas-microsoft-com:xml-data"))
                    {
                        throw ExceptionBuilder.DuplicateDeclaration(str3);
                    }
                    str4 = XMLSchema.GenUniqueColumnName(str3, table);
                }
            }
            else
            {
                str3 = str4 = "";
            }
            XmlElement element = this.FindTypeNode(node);
            SimpleType type2 = null;
            if (element == null)
            {
                throw ExceptionBuilder.UndefinedDatatype(node.GetAttribute("type"));
            }
            string attribute = element.GetAttribute("type", "urn:schemas-microsoft-com:datatypes");
            string dtValues = element.GetAttribute("values", "urn:schemas-microsoft-com:datatypes");
            if ((attribute == null) || (attribute.Length == 0))
            {
                attribute = "";
                type = typeof(string);
            }
            else
            {
                type = this.ParseDataType(attribute, dtValues);
                if (attribute == "float")
                {
                    attribute = "";
                }
                if (attribute == "char")
                {
                    attribute = "";
                    type2 = SimpleType.CreateSimpleType(type);
                }
                if (attribute == "enumeration")
                {
                    attribute = "";
                    type2 = SimpleType.CreateEnumeratedType(dtValues);
                }
                if (attribute == "bin.base64")
                {
                    attribute = "";
                    type2 = SimpleType.CreateByteArrayType("base64");
                }
                if (attribute == "bin.hex")
                {
                    attribute = "";
                    type2 = SimpleType.CreateByteArrayType("hex");
                }
            }
            bool isAttribute = XMLSchema.FEqualIdentity(node, "attribute", "urn:schemas-microsoft-com:xml-data");
            this.GetMinMax(node, isAttribute, ref minOccurs, ref maxOccurs);
            string str2 = null;
            str2 = node.GetAttribute("default");
            bool flag2 = false;
            column = new DataColumn(XmlConvert.DecodeName(str4), type, null, isAttribute ? MappingType.Attribute : MappingType.Element);
            XMLSchema.SetProperties(column, node.Attributes);
            column.XmlDataType = attribute;
            column.SimpleType = type2;
            column.AllowDBNull = (minOccurs == 0) || flag2;
            column.Namespace = isAttribute ? string.Empty : this._schemaUri;
            if (node.Attributes != null)
            {
                for (int i = 0; i < node.Attributes.Count; i++)
                {
                    if ((node.Attributes[i].NamespaceURI == "urn:schemas-microsoft-com:xml-msdata") && (node.Attributes[i].LocalName == "Expression"))
                    {
                        column.Expression = node.Attributes[i].Value;
                        break;
                    }
                }
            }
            string str5 = node.GetAttribute("targetNamespace");
            if ((str5 != null) && (str5.Length > 0))
            {
                column.Namespace = str5;
            }
            table.Columns.Add(column);
            if ((str2 != null) && (str2.Length != 0))
            {
                try
                {
                    column.DefaultValue = SqlConvert.ChangeTypeForXML(str2, type);
                }
                catch (FormatException)
                {
                    throw ExceptionBuilder.CannotConvert(str2, type.FullName);
                }
            }
            for (XmlNode node2 = node.FirstChild; node2 != null; node2 = node2.NextSibling)
            {
                if (XMLSchema.FEqualIdentity(node2, "description", "urn:schemas-microsoft-com:xml-data"))
                {
                    column.Description(((XmlElement) node2).InnerText);
                }
            }
        }

        internal DataTable HandleTable(XmlElement node)
        {
            XmlElement typeNode = this.FindTypeNode(node);
            string attribute = node.GetAttribute("minOccurs");
            if (((attribute != null) && (attribute.Length > 0)) && ((Convert.ToInt32(attribute, CultureInfo.InvariantCulture) > 1) && (typeNode == null)))
            {
                return this.InstantiateSimpleTable(this._ds, node);
            }
            attribute = node.GetAttribute("maxOccurs");
            if (((attribute != null) && (attribute.Length > 0)) && ((string.Compare(attribute, "1", StringComparison.Ordinal) != 0) && (typeNode == null)))
            {
                return this.InstantiateSimpleTable(this._ds, node);
            }
            if (typeNode == null)
            {
                return null;
            }
            if (this.IsXDRField(node, typeNode))
            {
                return null;
            }
            return this.InstantiateTable(this._ds, node, typeNode);
        }

        internal void HandleTypeNode(XmlElement typeNode, DataTable table, ArrayList tableChildren)
        {
            for (XmlNode node = typeNode.FirstChild; node != null; node = node.NextSibling)
            {
                if (node is XmlElement)
                {
                    if (XMLSchema.FEqualIdentity(node, "element", "urn:schemas-microsoft-com:xml-data"))
                    {
                        DataTable table2 = this.HandleTable((XmlElement) node);
                        if (table2 != null)
                        {
                            tableChildren.Add(table2);
                            continue;
                        }
                    }
                    if (XMLSchema.FEqualIdentity(node, "attribute", "urn:schemas-microsoft-com:xml-data") || XMLSchema.FEqualIdentity(node, "element", "urn:schemas-microsoft-com:xml-data"))
                    {
                        this.HandleColumn((XmlElement) node, table);
                    }
                }
            }
        }

        internal DataTable InstantiateSimpleTable(DataSet dataSet, XmlElement node)
        {
            XmlAttributeCollection attrs = node.Attributes;
            int minOccurs = 1;
            int maxOccurs = 1;
            string instanceName = this.GetInstanceName(node);
            if (dataSet.Tables.GetTable(instanceName, this._schemaUri) != null)
            {
                throw ExceptionBuilder.DuplicateDeclaration(instanceName);
            }
            string tableName = XmlConvert.DecodeName(instanceName);
            DataTable instance = new DataTable(tableName) {
                Namespace = this._schemaUri
            };
            this.GetMinMax(node, ref minOccurs, ref maxOccurs);
            instance.MinOccurs = minOccurs;
            instance.MaxOccurs = maxOccurs;
            XMLSchema.SetProperties(instance, attrs);
            instance.repeatableElement = true;
            this.HandleColumn(node, instance);
            instance.Columns[0].ColumnName = tableName + "_Column";
            this._ds.Tables.Add(instance);
            return instance;
        }

        internal DataTable InstantiateTable(DataSet dataSet, XmlElement node, XmlElement typeNode)
        {
            DataTable table;
            string name = "";
            XmlAttributeCollection attrs = node.Attributes;
            int minOccurs = 1;
            int maxOccurs = 1;
            string str2 = null;
            ArrayList tableChildren = new ArrayList();
            if (attrs.Count > 0)
            {
                name = this.GetInstanceName(node);
                table = dataSet.Tables.GetTable(name, this._schemaUri);
                if (table != null)
                {
                    return table;
                }
            }
            table = new DataTable(XmlConvert.DecodeName(name)) {
                Namespace = this._schemaUri
            };
            this.GetMinMax(node, ref minOccurs, ref maxOccurs);
            table.MinOccurs = minOccurs;
            table.MaxOccurs = maxOccurs;
            this._ds.Tables.Add(table);
            this.HandleTypeNode(typeNode, table, tableChildren);
            XMLSchema.SetProperties(table, attrs);
            if (str2 != null)
            {
                string[] strArray = str2.TrimEnd(null).Split(null);
                int length = strArray.Length;
                DataColumn[] columnArray = new DataColumn[length];
                for (int i = 0; i < length; i++)
                {
                    DataColumn column2 = table.Columns[strArray[i], this._schemaUri];
                    if (column2 == null)
                    {
                        throw ExceptionBuilder.ElementTypeNotFound(strArray[i]);
                    }
                    columnArray[i] = column2;
                }
                table.PrimaryKey = columnArray;
            }
            foreach (DataTable table2 in tableChildren)
            {
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
                    DataColumn parentKey = table.AddUniqueKey();
                    DataColumn childColumn = table2.AddForeignKey(parentKey);
                    relation = new DataRelation(table.TableName + "_" + table2.TableName, parentKey, childColumn, true) {
                        CheckMultipleNested = false,
                        Nested = true
                    };
                    table2.DataSet.Relations.Add(relation);
                    relation.CheckMultipleNested = true;
                }
            }
            return table;
        }

        internal bool IsTextOnlyContent(XmlElement node)
        {
            string attribute = node.GetAttribute("content");
            if ((attribute == null) || (attribute.Length == 0))
            {
                string str2 = node.GetAttribute("type", "urn:schemas-microsoft-com:datatypes");
                return ((str2 != null) && (str2.Length > 0));
            }
            if (((attribute == "empty") || (attribute == "eltOnly")) || ((attribute == "elementOnly") || (attribute == "mixed")))
            {
                return false;
            }
            if (attribute != "textOnly")
            {
                throw ExceptionBuilder.InvalidAttributeValue("content", attribute);
            }
            return true;
        }

        internal bool IsXDRField(XmlElement node, XmlElement typeNode)
        {
            int minOccurs = 1;
            int maxOccurs = 1;
            if (!this.IsTextOnlyContent(typeNode))
            {
                return false;
            }
            for (XmlNode node2 = typeNode.FirstChild; node2 != null; node2 = node2.NextSibling)
            {
                if (XMLSchema.FEqualIdentity(node2, "element", "urn:schemas-microsoft-com:xml-data") || XMLSchema.FEqualIdentity(node2, "attribute", "urn:schemas-microsoft-com:xml-data"))
                {
                    return false;
                }
            }
            if (XMLSchema.FEqualIdentity(node, "element", "urn:schemas-microsoft-com:xml-data"))
            {
                this.GetMinMax(node, ref minOccurs, ref maxOccurs);
                if ((maxOccurs == -1) || (maxOccurs > 1))
                {
                    return false;
                }
            }
            return true;
        }

        internal void LoadSchema(XmlElement schemaRoot, DataSet ds)
        {
            if (schemaRoot != null)
            {
                this._schemaRoot = schemaRoot;
                this._ds = ds;
                this._schemaName = schemaRoot.GetAttribute("name");
                this._schemaUri = "";
                if ((this._schemaName == null) || (this._schemaName.Length == 0))
                {
                    this._schemaName = "NewDataSet";
                }
                ds.Namespace = this._schemaUri;
                for (XmlNode node = schemaRoot.FirstChild; node != null; node = node.NextSibling)
                {
                    if (node is XmlElement)
                    {
                        XmlElement element = (XmlElement) node;
                        if (XMLSchema.FEqualIdentity(element, "ElementType", "urn:schemas-microsoft-com:xml-data"))
                        {
                            this.HandleTable(element);
                        }
                    }
                }
                this._schemaName = XmlConvert.DecodeName(this._schemaName);
                if (ds.Tables[this._schemaName] == null)
                {
                    ds.DataSetName = this._schemaName;
                }
            }
        }

        private Type ParseDataType(string dt, string dtValues)
        {
            string name = dt;
            string[] strArray = dt.Split(colonArray);
            if (strArray.Length > 2)
            {
                throw ExceptionBuilder.InvalidAttributeValue("type", dt);
            }
            if (strArray.Length == 2)
            {
                name = strArray[1];
            }
            NameType type = FindNameType(name);
            if ((type == enumerationNameType) && ((dtValues == null) || (dtValues.Length == 0)))
            {
                throw ExceptionBuilder.MissingAttribute("type", "values");
            }
            return type.type;
        }

        private sealed class NameType : IComparable
        {
            public string name;
            public Type type;

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

