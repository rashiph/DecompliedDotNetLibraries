namespace System.Data
{
    using System;
    using System.Collections;
    using System.Data.Common;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class XmlDataLoader
    {
        private Stack childRowsStack;
        private XmlReader dataReader;
        private DataSet dataSet;
        private DataTable dataTable;
        private object DFFNS;
        private object DIFFID;
        private bool fIsXdr;
        private bool fromInference;
        private object HASCHANGES;
        private Hashtable htableExcludedNS;
        private bool ignoreSchema;
        internal bool isDiffgram;
        private bool isTableLevel;
        private object MSDNS;
        private Hashtable nodeToRowMap;
        private XmlToDatasetMap nodeToSchemaMap;
        private object ROWORDER;
        private object SQL_SYNC;
        private XmlElement topMostNode;
        private DataRow topMostRow;
        private object UPDGNS;
        private object XDR_SCHEMA;
        private object XDRNS;
        private object XSD_SCHEMA;
        private object XSD_XMLNS_NS;
        private object XSDNS;

        internal XmlDataLoader(DataSet dataset, bool IsXdr, bool ignoreSchema)
        {
            this.dataSet = dataset;
            this.nodeToRowMap = new Hashtable();
            this.fIsXdr = IsXdr;
            this.ignoreSchema = ignoreSchema;
        }

        internal XmlDataLoader(DataTable datatable, bool IsXdr, bool ignoreSchema)
        {
            this.dataSet = null;
            this.dataTable = datatable;
            this.isTableLevel = true;
            this.nodeToRowMap = new Hashtable();
            this.fIsXdr = IsXdr;
            this.ignoreSchema = ignoreSchema;
        }

        internal XmlDataLoader(DataSet dataset, bool IsXdr, XmlElement topNode, bool ignoreSchema)
        {
            this.dataSet = dataset;
            this.nodeToRowMap = new Hashtable();
            this.fIsXdr = IsXdr;
            this.childRowsStack = new Stack(50);
            this.topMostNode = topNode;
            this.ignoreSchema = ignoreSchema;
        }

        internal XmlDataLoader(DataTable datatable, bool IsXdr, XmlElement topNode, bool ignoreSchema)
        {
            this.dataSet = null;
            this.dataTable = datatable;
            this.isTableLevel = true;
            this.nodeToRowMap = new Hashtable();
            this.fIsXdr = IsXdr;
            this.childRowsStack = new Stack(50);
            this.topMostNode = topNode;
            this.ignoreSchema = ignoreSchema;
        }

        private void AttachRows(DataRow parentRow, XmlNode parentElement)
        {
            if (parentElement != null)
            {
                for (XmlNode node = parentElement.FirstChild; node != null; node = node.NextSibling)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        XmlElement e = (XmlElement) node;
                        DataRow rowFromElement = this.GetRowFromElement(e);
                        if ((rowFromElement != null) && (rowFromElement.RowState == DataRowState.Detached))
                        {
                            if (parentRow != null)
                            {
                                rowFromElement.SetNestedParentRow(parentRow, false);
                            }
                            rowFromElement.Table.Rows.Add(rowFromElement);
                        }
                        else if (rowFromElement == null)
                        {
                            this.AttachRows(parentRow, node);
                        }
                        this.AttachRows(rowFromElement, node);
                    }
                }
            }
        }

        private int CountNonNSAttributes(XmlNode node)
        {
            int num2 = 0;
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                XmlAttribute attribute1 = node.Attributes[i];
                if (!this.FExcludedNamespace(node.Attributes[i].NamespaceURI))
                {
                    num2++;
                }
            }
            return num2;
        }

        internal bool FColumnElement(XmlElement e)
        {
            if (this.nodeToSchemaMap.GetColumnSchema(e, this.FIgnoreNamespace(e)) == null)
            {
                return false;
            }
            if (this.CountNonNSAttributes(e) > 0)
            {
                return false;
            }
            for (XmlNode node = e.FirstChild; node != null; node = node.NextSibling)
            {
                if (node is XmlElement)
                {
                    return false;
                }
            }
            return true;
        }

        private bool FExcludedNamespace(string ns)
        {
            if (ns.Equals("http://www.w3.org/2000/xmlns/"))
            {
                return true;
            }
            if (this.htableExcludedNS == null)
            {
                return false;
            }
            return this.htableExcludedNS.Contains(ns);
        }

        private bool FIgnoreNamespace(XmlNode node)
        {
            XmlNode ownerElement;
            if (!this.fIsXdr)
            {
                return false;
            }
            if (node is XmlAttribute)
            {
                ownerElement = ((XmlAttribute) node).OwnerElement;
            }
            else
            {
                ownerElement = node;
            }
            return ownerElement.NamespaceURI.StartsWith("x-schema:#", StringComparison.Ordinal);
        }

        private bool FIgnoreNamespace(XmlReader node)
        {
            return (this.fIsXdr && node.NamespaceURI.StartsWith("x-schema:#", StringComparison.Ordinal));
        }

        private string GetInitialTextFromNodes(ref XmlNode n)
        {
            string str = null;
            if (n != null)
            {
                while (n.NodeType == XmlNodeType.Whitespace)
                {
                    n = n.NextSibling;
                }
                if (this.IsTextLikeNode(n.NodeType) && ((n.NextSibling == null) || !this.IsTextLikeNode(n.NodeType)))
                {
                    str = n.Value;
                    n = n.NextSibling;
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    while ((n != null) && this.IsTextLikeNode(n.NodeType))
                    {
                        builder.Append(n.Value);
                        n = n.NextSibling;
                    }
                    str = builder.ToString();
                }
            }
            if (str == null)
            {
                str = string.Empty;
            }
            return str;
        }

        internal DataRow GetRowFromElement(XmlElement e)
        {
            return (DataRow) this.nodeToRowMap[e];
        }

        private DataColumn GetTextOnlyColumn(DataRow row)
        {
            DataColumnCollection columns = row.Table.Columns;
            int count = columns.Count;
            for (int i = 0; i < count; i++)
            {
                DataColumn c = columns[i];
                if (this.IsTextOnly(c))
                {
                    return c;
                }
            }
            return null;
        }

        private string GetValueForTextOnlyColums(XmlNode n)
        {
            string str = null;
            while ((n != null) && ((n.NodeType == XmlNodeType.Whitespace) || !this.IsTextLikeNode(n.NodeType)))
            {
                n = n.NextSibling;
            }
            if (n != null)
            {
                if (this.IsTextLikeNode(n.NodeType) && ((n.NextSibling == null) || !this.IsTextLikeNode(n.NodeType)))
                {
                    str = n.Value;
                    n = n.NextSibling;
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    while ((n != null) && this.IsTextLikeNode(n.NodeType))
                    {
                        builder.Append(n.Value);
                        n = n.NextSibling;
                    }
                    str = builder.ToString();
                }
            }
            if (str == null)
            {
                str = string.Empty;
            }
            return str;
        }

        private void InitNameTable()
        {
            XmlNameTable nameTable = this.dataReader.NameTable;
            this.XSD_XMLNS_NS = nameTable.Add("http://www.w3.org/2000/xmlns/");
            this.XDR_SCHEMA = nameTable.Add("Schema");
            this.XDRNS = nameTable.Add("urn:schemas-microsoft-com:xml-data");
            this.SQL_SYNC = nameTable.Add("sync");
            this.UPDGNS = nameTable.Add("urn:schemas-microsoft-com:xml-updategram");
            this.XSD_SCHEMA = nameTable.Add("schema");
            this.XSDNS = nameTable.Add("http://www.w3.org/2001/XMLSchema");
            this.DFFNS = nameTable.Add("urn:schemas-microsoft-com:xml-diffgram-v1");
            this.MSDNS = nameTable.Add("urn:schemas-microsoft-com:xml-msdata");
            this.DIFFID = nameTable.Add("id");
            this.HASCHANGES = nameTable.Add("hasChanges");
            this.ROWORDER = nameTable.Add("rowOrder");
        }

        internal bool IsTextLikeNode(XmlNodeType n)
        {
            switch (n)
            {
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return true;

                case XmlNodeType.EntityReference:
                    throw ExceptionBuilder.FoundEntity();
            }
            return false;
        }

        internal bool IsTextOnly(DataColumn c)
        {
            if (c.ColumnMapping != MappingType.SimpleContent)
            {
                return false;
            }
            return true;
        }

        private void LoadColumn(DataColumn column, object[] foundColumns)
        {
            string s = string.Empty;
            string str3 = null;
            int depth = this.dataReader.Depth;
            if (this.dataReader.AttributeCount > 0)
            {
                str3 = this.dataReader.GetAttribute("nil", "http://www.w3.org/2001/XMLSchema-instance");
            }
            if (column.IsCustomType)
            {
                object staticNullForUdtType = null;
                string attribute = null;
                string str2 = null;
                XmlRootAttribute xmlAttrib = null;
                if (this.dataReader.AttributeCount > 0)
                {
                    attribute = this.dataReader.GetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance");
                    str2 = this.dataReader.GetAttribute("InstanceType", "urn:schemas-microsoft-com:xml-msdata");
                }
                bool flag2 = !column.ImplementsIXMLSerializable && (((column.DataType != typeof(object)) && (str2 == null)) && (attribute == null));
                if ((str3 != null) && XmlConvert.ToBoolean(str3))
                {
                    if ((!flag2 && (str2 != null)) && (str2.Length > 0))
                    {
                        staticNullForUdtType = SqlUdtStorage.GetStaticNullForUdtType(DataStorage.GetType(str2));
                    }
                    if (staticNullForUdtType == null)
                    {
                        staticNullForUdtType = DBNull.Value;
                    }
                    if (!this.dataReader.IsEmptyElement)
                    {
                        while (this.dataReader.Read() && (depth < this.dataReader.Depth))
                        {
                        }
                    }
                    this.dataReader.Read();
                }
                else
                {
                    bool flag = false;
                    if ((column.Table.DataSet != null) && column.Table.DataSet.UdtIsWrapped)
                    {
                        this.dataReader.Read();
                        flag = true;
                    }
                    if (flag2)
                    {
                        if (flag)
                        {
                            xmlAttrib = new XmlRootAttribute(this.dataReader.LocalName) {
                                Namespace = this.dataReader.NamespaceURI
                            };
                        }
                        else
                        {
                            xmlAttrib = new XmlRootAttribute(column.EncodedColumnName) {
                                Namespace = column.Namespace
                            };
                        }
                    }
                    staticNullForUdtType = column.ConvertXmlToObject(this.dataReader, xmlAttrib);
                    if (flag)
                    {
                        this.dataReader.Read();
                    }
                }
                foundColumns[column.Ordinal] = staticNullForUdtType;
            }
            else
            {
                if (this.dataReader.Read() && (depth < this.dataReader.Depth))
                {
                    while (depth < this.dataReader.Depth)
                    {
                        DataTable table2;
                        object obj3;
                        switch (this.dataReader.NodeType)
                        {
                            case XmlNodeType.Element:
                            {
                                if (!this.ProcessXsdSchema())
                                {
                                    obj3 = this.nodeToSchemaMap.GetColumnSchema(column.Table, this.dataReader, this.FIgnoreNamespace(this.dataReader));
                                    DataColumn column2 = obj3 as DataColumn;
                                    if (column2 == null)
                                    {
                                        goto Label_0301;
                                    }
                                    if (foundColumns[column2.Ordinal] != null)
                                    {
                                        goto Label_02F3;
                                    }
                                    this.LoadColumn(column2, foundColumns);
                                }
                                continue;
                            }
                            case XmlNodeType.Text:
                            case XmlNodeType.CDATA:
                            case XmlNodeType.Whitespace:
                            case XmlNodeType.SignificantWhitespace:
                                if (s.Length != 0)
                                {
                                    goto Label_028C;
                                }
                                s = this.dataReader.Value;
                                goto Label_0252;

                            case XmlNodeType.EntityReference:
                                throw ExceptionBuilder.FoundEntity();

                            default:
                                goto Label_035B;
                        }
                    Label_0240:
                        s = s + this.dataReader.Value;
                    Label_0252:
                        if ((this.dataReader.Read() && (depth < this.dataReader.Depth)) && this.IsTextLikeNode(this.dataReader.NodeType))
                        {
                            goto Label_0240;
                        }
                        continue;
                    Label_028C:
                        this.dataReader.ReadString();
                        continue;
                    Label_02F3:
                        this.dataReader.Read();
                        continue;
                    Label_0301:
                        table2 = obj3 as DataTable;
                        if (table2 != null)
                        {
                            this.LoadTable(table2, true);
                        }
                        else
                        {
                            DataTable tableForNode = this.nodeToSchemaMap.GetTableForNode(this.dataReader, this.FIgnoreNamespace(this.dataReader));
                            if (tableForNode != null)
                            {
                                this.LoadTable(tableForNode, false);
                            }
                            else
                            {
                                this.dataReader.Read();
                            }
                        }
                        continue;
                    Label_035B:
                        this.dataReader.Read();
                    }
                    this.dataReader.Read();
                }
                if (((s.Length == 0) && (str3 != null)) && XmlConvert.ToBoolean(str3))
                {
                    foundColumns[column.Ordinal] = DBNull.Value;
                }
                else
                {
                    foundColumns[column.Ordinal] = column.ConvertXmlToObject(s);
                }
            }
        }

        internal void LoadData(XmlDocument xdoc)
        {
            if (xdoc.DocumentElement != null)
            {
                bool enforceConstraints;
                if (this.isTableLevel)
                {
                    enforceConstraints = this.dataTable.EnforceConstraints;
                    this.dataTable.EnforceConstraints = false;
                }
                else
                {
                    enforceConstraints = this.dataSet.EnforceConstraints;
                    this.dataSet.EnforceConstraints = false;
                    this.dataSet.fInReadXml = true;
                }
                if (this.isTableLevel)
                {
                    this.nodeToSchemaMap = new XmlToDatasetMap(this.dataTable, xdoc.NameTable);
                }
                else
                {
                    this.nodeToSchemaMap = new XmlToDatasetMap(this.dataSet, xdoc.NameTable);
                }
                DataRow row = null;
                if (this.isTableLevel || ((this.dataSet != null) && this.dataSet.fTopLevelTable))
                {
                    XmlElement documentElement = xdoc.DocumentElement;
                    DataTable schemaForNode = (DataTable) this.nodeToSchemaMap.GetSchemaForNode(documentElement, this.FIgnoreNamespace(documentElement));
                    if (schemaForNode != null)
                    {
                        row = schemaForNode.CreateEmptyRow();
                        this.nodeToRowMap[documentElement] = row;
                        this.LoadRowData(row, documentElement);
                        schemaForNode.Rows.Add(row);
                    }
                }
                this.LoadRows(row, xdoc.DocumentElement);
                this.AttachRows(row, xdoc.DocumentElement);
                if (this.isTableLevel)
                {
                    this.dataTable.EnforceConstraints = enforceConstraints;
                }
                else
                {
                    this.dataSet.fInReadXml = false;
                    this.dataSet.EnforceConstraints = enforceConstraints;
                }
            }
        }

        internal void LoadData(XmlReader reader)
        {
            this.dataReader = DataTextReader.CreateReader(reader);
            int depth = this.dataReader.Depth;
            bool flag = this.isTableLevel ? this.dataTable.EnforceConstraints : this.dataSet.EnforceConstraints;
            this.InitNameTable();
            if (this.nodeToSchemaMap == null)
            {
                this.nodeToSchemaMap = this.isTableLevel ? new XmlToDatasetMap(this.dataReader.NameTable, this.dataTable) : new XmlToDatasetMap(this.dataReader.NameTable, this.dataSet);
            }
            if (this.isTableLevel)
            {
                this.dataTable.EnforceConstraints = false;
            }
            else
            {
                this.dataSet.EnforceConstraints = false;
                this.dataSet.fInReadXml = true;
            }
            if (this.topMostNode != null)
            {
                if (!this.isDiffgram && !this.isTableLevel)
                {
                    DataTable schemaForNode = this.nodeToSchemaMap.GetSchemaForNode(this.topMostNode, this.FIgnoreNamespace(this.topMostNode)) as DataTable;
                    if (schemaForNode != null)
                    {
                        this.LoadTopMostTable(schemaForNode);
                    }
                }
                this.topMostNode = null;
            }
            while (!this.dataReader.EOF)
            {
                if (this.dataReader.Depth < depth)
                {
                    break;
                }
                if (reader.NodeType != XmlNodeType.Element)
                {
                    this.dataReader.Read();
                }
                else
                {
                    DataTable tableForNode = this.nodeToSchemaMap.GetTableForNode(this.dataReader, this.FIgnoreNamespace(this.dataReader));
                    if (tableForNode == null)
                    {
                        if (!this.ProcessXsdSchema())
                        {
                            this.dataReader.Read();
                        }
                        continue;
                    }
                    this.LoadTable(tableForNode, false);
                }
            }
            if (this.isTableLevel)
            {
                this.dataTable.EnforceConstraints = flag;
            }
            else
            {
                this.dataSet.fInReadXml = false;
                this.dataSet.EnforceConstraints = flag;
            }
        }

        private void LoadRowData(DataRow row, XmlElement rowElement)
        {
            DataTable table = row.Table;
            if (this.FromInference)
            {
                table.Prefix = rowElement.Prefix;
            }
            Hashtable hashtable = new Hashtable();
            row.BeginEdit();
            XmlNode firstChild = rowElement.FirstChild;
            DataColumn textOnlyColumn = this.GetTextOnlyColumn(row);
            if (textOnlyColumn != null)
            {
                hashtable[textOnlyColumn] = textOnlyColumn;
                string valueForTextOnlyColums = this.GetValueForTextOnlyColums(firstChild);
                if (XMLSchema.GetBooleanAttribute(rowElement, "nil", "http://www.w3.org/2001/XMLSchema-instance", false) && ADP.IsEmpty(valueForTextOnlyColums))
                {
                    row[textOnlyColumn] = DBNull.Value;
                }
                else
                {
                    this.SetRowValueFromXmlText(row, textOnlyColumn, valueForTextOnlyColums);
                }
            }
            while ((firstChild != null) && (firstChild != rowElement))
            {
                if (firstChild.NodeType == XmlNodeType.Element)
                {
                    XmlElement node = (XmlElement) firstChild;
                    object schemaForNode = this.nodeToSchemaMap.GetSchemaForNode(node, this.FIgnoreNamespace(node));
                    if ((schemaForNode is DataTable) && this.FColumnElement(node))
                    {
                        schemaForNode = this.nodeToSchemaMap.GetColumnSchema(node, this.FIgnoreNamespace(node));
                    }
                    if ((schemaForNode == null) || (schemaForNode is DataColumn))
                    {
                        firstChild = node.FirstChild;
                        if ((schemaForNode != null) && (schemaForNode is DataColumn))
                        {
                            DataColumn col = (DataColumn) schemaForNode;
                            if (((col.Table == row.Table) && (col.ColumnMapping != MappingType.Attribute)) && (hashtable[col] == null))
                            {
                                hashtable[col] = col;
                                string str = this.GetValueForTextOnlyColums(firstChild);
                                if (XMLSchema.GetBooleanAttribute(node, "nil", "http://www.w3.org/2001/XMLSchema-instance", false) && ADP.IsEmpty(str))
                                {
                                    row[col] = DBNull.Value;
                                }
                                else
                                {
                                    this.SetRowValueFromXmlText(row, col, str);
                                }
                            }
                        }
                        else if ((schemaForNode == null) && (firstChild != null))
                        {
                            continue;
                        }
                        if (firstChild == null)
                        {
                            firstChild = node;
                        }
                    }
                }
                while ((firstChild != rowElement) && (firstChild.NextSibling == null))
                {
                    firstChild = firstChild.ParentNode;
                }
                if (firstChild != rowElement)
                {
                    firstChild = firstChild.NextSibling;
                }
            }
            foreach (XmlAttribute attribute in rowElement.Attributes)
            {
                object columnSchema = this.nodeToSchemaMap.GetColumnSchema(attribute, this.FIgnoreNamespace(attribute));
                if ((columnSchema != null) && (columnSchema is DataColumn))
                {
                    DataColumn column3 = (DataColumn) columnSchema;
                    if ((column3.ColumnMapping == MappingType.Attribute) && (hashtable[column3] == null))
                    {
                        hashtable[column3] = column3;
                        firstChild = attribute.FirstChild;
                        this.SetRowValueFromXmlText(row, column3, this.GetInitialTextFromNodes(ref firstChild));
                    }
                }
            }
            foreach (DataColumn column in row.Table.Columns)
            {
                if ((hashtable[column] == null) && XmlToDatasetMap.IsMappedColumn(column))
                {
                    if (!column.AutoIncrement)
                    {
                        if (column.AllowDBNull)
                        {
                            row[column] = DBNull.Value;
                        }
                        else
                        {
                            row[column] = column.DefaultValue;
                        }
                    }
                    else
                    {
                        column.Init(row.tempRecord);
                    }
                }
            }
            row.EndEdit();
        }

        private void LoadRows(DataRow parentRow, XmlNode parentElement)
        {
            if ((parentElement != null) && ((((parentElement.LocalName != "schema") || (parentElement.NamespaceURI != "http://www.w3.org/2001/XMLSchema")) && ((parentElement.LocalName != "sync") || (parentElement.NamespaceURI != "urn:schemas-microsoft-com:xml-updategram"))) && ((parentElement.LocalName != "Schema") || (parentElement.NamespaceURI != "urn:schemas-microsoft-com:xml-data"))))
            {
                for (XmlNode node = parentElement.FirstChild; node != null; node = node.NextSibling)
                {
                    if (node is XmlElement)
                    {
                        XmlElement element = (XmlElement) node;
                        object schemaForNode = this.nodeToSchemaMap.GetSchemaForNode(element, this.FIgnoreNamespace(element));
                        if ((schemaForNode != null) && (schemaForNode is DataTable))
                        {
                            DataRow rowFromElement = this.GetRowFromElement(element);
                            if (rowFromElement == null)
                            {
                                if ((parentRow != null) && this.FColumnElement(element))
                                {
                                    continue;
                                }
                                rowFromElement = ((DataTable) schemaForNode).CreateEmptyRow();
                                this.nodeToRowMap[element] = rowFromElement;
                                this.LoadRowData(rowFromElement, element);
                            }
                            this.LoadRows(rowFromElement, node);
                        }
                        else
                        {
                            this.LoadRows(null, node);
                        }
                    }
                }
            }
        }

        private void LoadTable(DataTable table, bool isNested)
        {
            DataColumn xmlText;
            DataRow row = null;
            int depth = this.dataReader.Depth;
            int count = this.childRowsStack.Count;
            DataColumnCollection columns = table.Columns;
            object[] foundColumns = new object[columns.Count];
            int pos = -1;
            string str3 = string.Empty;
            string str2 = null;
            bool flag = false;
            for (int i = this.dataReader.AttributeCount - 1; i >= 0; i--)
            {
                this.dataReader.MoveToAttribute(i);
                xmlText = this.nodeToSchemaMap.GetColumnSchema(table, this.dataReader, this.FIgnoreNamespace(this.dataReader)) as DataColumn;
                if ((xmlText != null) && (xmlText.ColumnMapping == MappingType.Attribute))
                {
                    foundColumns[xmlText.Ordinal] = xmlText.ConvertXmlToObject(this.dataReader.Value);
                }
                if (this.isDiffgram)
                {
                    if (!(this.dataReader.NamespaceURI == "urn:schemas-microsoft-com:xml-diffgram-v1"))
                    {
                        goto Label_0161;
                    }
                    string localName = this.dataReader.LocalName;
                    if (localName != null)
                    {
                        if (!(localName == "id"))
                        {
                            if (localName == "hasChanges")
                            {
                                goto Label_0124;
                            }
                            if (localName == "hasErrors")
                            {
                                goto Label_0136;
                            }
                        }
                        else
                        {
                            str3 = this.dataReader.Value;
                        }
                    }
                }
                continue;
            Label_0124:
                str2 = this.dataReader.Value;
                continue;
            Label_0136:
                flag = (bool) Convert.ChangeType(this.dataReader.Value, typeof(bool), CultureInfo.InvariantCulture);
                continue;
            Label_0161:
                if (this.dataReader.NamespaceURI == "urn:schemas-microsoft-com:xml-msdata")
                {
                    if (this.dataReader.LocalName == "rowOrder")
                    {
                        pos = (int) Convert.ChangeType(this.dataReader.Value, typeof(int), CultureInfo.InvariantCulture);
                    }
                    else if (this.dataReader.LocalName.StartsWith("hidden", StringComparison.Ordinal))
                    {
                        xmlText = columns[XmlConvert.DecodeName(this.dataReader.LocalName.Substring(6))];
                        if ((xmlText != null) && (xmlText.ColumnMapping == MappingType.Hidden))
                        {
                            foundColumns[xmlText.Ordinal] = xmlText.ConvertXmlToObject(this.dataReader.Value);
                        }
                    }
                }
            }
            if (this.dataReader.Read() && (depth < this.dataReader.Depth))
            {
                while (depth < this.dataReader.Depth)
                {
                    DataTable table3;
                    object obj2;
                    switch (this.dataReader.NodeType)
                    {
                        case XmlNodeType.Element:
                        {
                            obj2 = this.nodeToSchemaMap.GetColumnSchema(table, this.dataReader, this.FIgnoreNamespace(this.dataReader));
                            xmlText = obj2 as DataColumn;
                            if (xmlText == null)
                            {
                                goto Label_02DE;
                            }
                            if (foundColumns[xmlText.Ordinal] != null)
                            {
                                break;
                            }
                            this.LoadColumn(xmlText, foundColumns);
                            continue;
                        }
                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Whitespace:
                        case XmlNodeType.SignificantWhitespace:
                        {
                            string s = this.dataReader.ReadString();
                            xmlText = table.xmlText;
                            if ((xmlText != null) && (foundColumns[xmlText.Ordinal] == null))
                            {
                                foundColumns[xmlText.Ordinal] = xmlText.ConvertXmlToObject(s);
                            }
                            continue;
                        }
                        case XmlNodeType.EntityReference:
                            throw ExceptionBuilder.FoundEntity();

                        default:
                            goto Label_0379;
                    }
                    this.dataReader.Read();
                    continue;
                Label_02DE:
                    table3 = obj2 as DataTable;
                    if (table3 != null)
                    {
                        this.LoadTable(table3, true);
                    }
                    else if (!this.ProcessXsdSchema())
                    {
                        DataTable tableForNode = this.nodeToSchemaMap.GetTableForNode(this.dataReader, this.FIgnoreNamespace(this.dataReader));
                        if (tableForNode != null)
                        {
                            this.LoadTable(tableForNode, false);
                        }
                        else
                        {
                            this.dataReader.Read();
                        }
                    }
                    continue;
                Label_0379:
                    this.dataReader.Read();
                }
                this.dataReader.Read();
            }
            if (this.isDiffgram)
            {
                row = table.NewRow(table.NewUninitializedRecord());
                row.BeginEdit();
                for (int j = foundColumns.Length - 1; j >= 0; j--)
                {
                    xmlText = columns[j];
                    xmlText[row.tempRecord] = (foundColumns[j] != null) ? foundColumns[j] : DBNull.Value;
                }
                row.EndEdit();
                table.Rows.DiffInsertAt(row, pos);
                if (str2 == null)
                {
                    row.oldRecord = row.newRecord;
                }
                if ((str2 == "modified") || flag)
                {
                    table.RowDiffId[str3] = row;
                }
            }
            else
            {
                for (int k = foundColumns.Length - 1; k >= 0; k--)
                {
                    if (foundColumns[k] == null)
                    {
                        xmlText = columns[k];
                        if ((xmlText.AllowDBNull && (xmlText.ColumnMapping != MappingType.Hidden)) && !xmlText.AutoIncrement)
                        {
                            foundColumns[k] = DBNull.Value;
                        }
                    }
                }
                row = table.Rows.AddWithColumnEvents(foundColumns);
            }
            while (count < this.childRowsStack.Count)
            {
                DataRow row2 = (DataRow) this.childRowsStack.Pop();
                bool flag2 = row2.RowState == DataRowState.Unchanged;
                row2.SetNestedParentRow(row, false);
                if (flag2)
                {
                    row2.oldRecord = row2.newRecord;
                }
            }
            if (isNested)
            {
                this.childRowsStack.Push(row);
            }
        }

        internal void LoadTopMostRow(ref bool[] foundColumns)
        {
            object schemaForNode = this.nodeToSchemaMap.GetSchemaForNode(this.topMostNode, this.FIgnoreNamespace(this.topMostNode));
            if (schemaForNode is DataTable)
            {
                this.topMostRow = ((DataTable) schemaForNode).CreateEmptyRow();
                foundColumns = new bool[this.topMostRow.Table.Columns.Count];
                foreach (XmlAttribute attribute in this.topMostNode.Attributes)
                {
                    object columnSchema = this.nodeToSchemaMap.GetColumnSchema(attribute, this.FIgnoreNamespace(attribute));
                    if ((columnSchema != null) && (columnSchema is DataColumn))
                    {
                        DataColumn col = (DataColumn) columnSchema;
                        if (col.ColumnMapping == MappingType.Attribute)
                        {
                            XmlNode firstChild = attribute.FirstChild;
                            this.SetRowValueFromXmlText(this.topMostRow, col, this.GetInitialTextFromNodes(ref firstChild));
                            foundColumns[col.Ordinal] = true;
                        }
                    }
                }
            }
            this.topMostNode = null;
        }

        private void LoadTopMostTable(DataTable table)
        {
            DataColumn columnSchema;
            bool flag3 = this.isTableLevel || (this.dataSet.DataSetName != table.TableName);
            DataRow parentRow = null;
            bool flag = false;
            int num3 = this.dataReader.Depth - 1;
            int count = this.childRowsStack.Count;
            DataColumnCollection columns = table.Columns;
            object[] foundColumns = new object[columns.Count];
            foreach (XmlAttribute attribute in this.topMostNode.Attributes)
            {
                columnSchema = this.nodeToSchemaMap.GetColumnSchema(attribute, this.FIgnoreNamespace(attribute)) as DataColumn;
                if ((columnSchema != null) && (columnSchema.ColumnMapping == MappingType.Attribute))
                {
                    XmlNode firstChild = attribute.FirstChild;
                    foundColumns[columnSchema.Ordinal] = columnSchema.ConvertXmlToObject(this.GetInitialTextFromNodes(ref firstChild));
                    flag = true;
                }
            }
            while (num3 < this.dataReader.Depth)
            {
                DataTable table2;
                object obj2;
                switch (this.dataReader.NodeType)
                {
                    case XmlNodeType.Element:
                    {
                        obj2 = this.nodeToSchemaMap.GetColumnSchema(table, this.dataReader, this.FIgnoreNamespace(this.dataReader));
                        columnSchema = obj2 as DataColumn;
                        if (columnSchema == null)
                        {
                            goto Label_017F;
                        }
                        if (foundColumns[columnSchema.Ordinal] != null)
                        {
                            break;
                        }
                        this.LoadColumn(columnSchema, foundColumns);
                        flag = true;
                        continue;
                    }
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                    {
                        string s = this.dataReader.ReadString();
                        columnSchema = table.xmlText;
                        if ((columnSchema != null) && (foundColumns[columnSchema.Ordinal] == null))
                        {
                            foundColumns[columnSchema.Ordinal] = columnSchema.ConvertXmlToObject(s);
                        }
                        continue;
                    }
                    case XmlNodeType.EntityReference:
                        throw ExceptionBuilder.FoundEntity();

                    default:
                        goto Label_01F2;
                }
                this.dataReader.Read();
                continue;
            Label_017F:
                table2 = obj2 as DataTable;
                if (table2 != null)
                {
                    this.LoadTable(table2, true);
                    flag = true;
                }
                else if (!this.ProcessXsdSchema())
                {
                    if (!flag && !flag3)
                    {
                        return;
                    }
                    this.dataReader.Read();
                }
                continue;
            Label_01F2:
                this.dataReader.Read();
            }
            this.dataReader.Read();
            for (int i = foundColumns.Length - 1; i >= 0; i--)
            {
                if (foundColumns[i] == null)
                {
                    columnSchema = columns[i];
                    if ((columnSchema.AllowDBNull && (columnSchema.ColumnMapping != MappingType.Hidden)) && !columnSchema.AutoIncrement)
                    {
                        foundColumns[i] = DBNull.Value;
                    }
                }
            }
            parentRow = table.Rows.AddWithColumnEvents(foundColumns);
            while (count < this.childRowsStack.Count)
            {
                DataRow row = (DataRow) this.childRowsStack.Pop();
                bool flag2 = row.RowState == DataRowState.Unchanged;
                row.SetNestedParentRow(parentRow, false);
                if (flag2)
                {
                    row.oldRecord = row.newRecord;
                }
            }
        }

        private bool ProcessXsdSchema()
        {
            if ((this.dataReader.LocalName == this.XSD_SCHEMA) && (this.dataReader.NamespaceURI == this.XSDNS))
            {
                if (this.ignoreSchema)
                {
                    this.dataReader.Skip();
                }
                else if (this.isTableLevel)
                {
                    this.dataTable.ReadXSDSchema(this.dataReader, false);
                    this.nodeToSchemaMap = new XmlToDatasetMap(this.dataReader.NameTable, this.dataTable);
                }
                else
                {
                    this.dataSet.ReadXSDSchema(this.dataReader, false);
                    this.nodeToSchemaMap = new XmlToDatasetMap(this.dataReader.NameTable, this.dataSet);
                }
            }
            else
            {
                if (((this.dataReader.LocalName != this.XDR_SCHEMA) || (this.dataReader.NamespaceURI != this.XDRNS)) && ((this.dataReader.LocalName != this.SQL_SYNC) || (this.dataReader.NamespaceURI != this.UPDGNS)))
                {
                    return false;
                }
                this.dataReader.Skip();
            }
            return true;
        }

        private void SetRowValueFromXmlText(DataRow row, DataColumn col, string xmlText)
        {
            row[col] = col.ConvertXmlToObject(xmlText);
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
    }
}

