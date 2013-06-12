namespace System.Data
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Xml;

    internal sealed class XmlToDatasetMap
    {
        private TableSchemaInfo lastTableSchemaInfo;
        private XmlNodeIdHashtable tableSchemaMap;

        public XmlToDatasetMap(DataSet dataSet, XmlNameTable nameTable)
        {
            this.BuildIdentityMap(dataSet, nameTable);
        }

        public XmlToDatasetMap(DataTable dataTable, XmlNameTable nameTable)
        {
            this.BuildIdentityMap(dataTable, nameTable);
        }

        public XmlToDatasetMap(XmlNameTable nameTable, DataSet dataSet)
        {
            this.BuildIdentityMap(nameTable, dataSet);
        }

        public XmlToDatasetMap(XmlNameTable nameTable, DataTable dataTable)
        {
            this.BuildIdentityMap(nameTable, dataTable);
        }

        private bool AddColumnSchema(DataColumn col, XmlNameTable nameTable, XmlNodeIdHashtable columns)
        {
            string localName = nameTable.Get(col.EncodedColumnName);
            string namespaceURI = nameTable.Get(col.Namespace);
            if (localName == null)
            {
                return false;
            }
            XmlNodeIdentety identety = new XmlNodeIdentety(localName, namespaceURI);
            columns[identety] = col;
            if (col.ColumnName.StartsWith("xml", StringComparison.OrdinalIgnoreCase))
            {
                this.HandleSpecialColumn(col, nameTable, columns);
            }
            return true;
        }

        private bool AddColumnSchema(XmlNameTable nameTable, DataColumn col, XmlNodeIdHashtable columns)
        {
            string array = XmlConvert.EncodeLocalName(col.ColumnName);
            string localName = nameTable.Get(array);
            if (localName == null)
            {
                localName = nameTable.Add(array);
            }
            col.encodedColumnName = localName;
            string namespaceURI = nameTable.Get(col.Namespace);
            if (namespaceURI == null)
            {
                namespaceURI = nameTable.Add(col.Namespace);
            }
            else if (col._columnUri != null)
            {
                col._columnUri = namespaceURI;
            }
            XmlNodeIdentety identety = new XmlNodeIdentety(localName, namespaceURI);
            columns[identety] = col;
            if (col.ColumnName.StartsWith("xml", StringComparison.OrdinalIgnoreCase))
            {
                this.HandleSpecialColumn(col, nameTable, columns);
            }
            return true;
        }

        private TableSchemaInfo AddTableSchema(DataTable table, XmlNameTable nameTable)
        {
            string localName = nameTable.Get(table.EncodedTableName);
            string namespaceURI = nameTable.Get(table.Namespace);
            if (localName == null)
            {
                return null;
            }
            TableSchemaInfo info = new TableSchemaInfo(table);
            this.tableSchemaMap[new XmlNodeIdentety(localName, namespaceURI)] = info;
            return info;
        }

        private TableSchemaInfo AddTableSchema(XmlNameTable nameTable, DataTable table)
        {
            string encodedTableName = table.EncodedTableName;
            string localName = nameTable.Get(encodedTableName);
            if (localName == null)
            {
                localName = nameTable.Add(encodedTableName);
            }
            table.encodedTableName = localName;
            string namespaceURI = nameTable.Get(table.Namespace);
            if (namespaceURI == null)
            {
                namespaceURI = nameTable.Add(table.Namespace);
            }
            else if (table.tableNamespace != null)
            {
                table.tableNamespace = namespaceURI;
            }
            TableSchemaInfo info = new TableSchemaInfo(table);
            this.tableSchemaMap[new XmlNodeIdentety(localName, namespaceURI)] = info;
            return info;
        }

        private void BuildIdentityMap(DataSet dataSet, XmlNameTable nameTable)
        {
            this.tableSchemaMap = new XmlNodeIdHashtable(dataSet.Tables.Count);
            foreach (DataTable table in dataSet.Tables)
            {
                TableSchemaInfo info = this.AddTableSchema(table, nameTable);
                if (info != null)
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        if (IsMappedColumn(column))
                        {
                            this.AddColumnSchema(column, nameTable, info.ColumnsSchemaMap);
                        }
                    }
                }
            }
        }

        private void BuildIdentityMap(DataTable dataTable, XmlNameTable nameTable)
        {
            this.tableSchemaMap = new XmlNodeIdHashtable(1);
            TableSchemaInfo info = this.AddTableSchema(dataTable, nameTable);
            if (info != null)
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (IsMappedColumn(column))
                    {
                        this.AddColumnSchema(column, nameTable, info.ColumnsSchemaMap);
                    }
                }
            }
        }

        private void BuildIdentityMap(XmlNameTable nameTable, DataSet dataSet)
        {
            this.tableSchemaMap = new XmlNodeIdHashtable(dataSet.Tables.Count);
            string str3 = nameTable.Get(dataSet.Namespace);
            if (str3 == null)
            {
                str3 = nameTable.Add(dataSet.Namespace);
            }
            dataSet.namespaceURI = str3;
            foreach (DataTable table in dataSet.Tables)
            {
                TableSchemaInfo info = this.AddTableSchema(nameTable, table);
                if (info != null)
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        if (IsMappedColumn(column))
                        {
                            this.AddColumnSchema(nameTable, column, info.ColumnsSchemaMap);
                        }
                    }
                    foreach (DataRelation relation in table.ChildRelations)
                    {
                        if (relation.Nested)
                        {
                            string array = XmlConvert.EncodeLocalName(relation.ChildTable.TableName);
                            string localName = nameTable.Get(array);
                            if (localName == null)
                            {
                                localName = nameTable.Add(array);
                            }
                            string namespaceURI = nameTable.Get(relation.ChildTable.Namespace);
                            if (namespaceURI == null)
                            {
                                namespaceURI = nameTable.Add(relation.ChildTable.Namespace);
                            }
                            XmlNodeIdentety identety = new XmlNodeIdentety(localName, namespaceURI);
                            info.ColumnsSchemaMap[identety] = relation.ChildTable;
                        }
                    }
                }
            }
        }

        private void BuildIdentityMap(XmlNameTable nameTable, DataTable dataTable)
        {
            ArrayList selfAndDescendants = this.GetSelfAndDescendants(dataTable);
            this.tableSchemaMap = new XmlNodeIdHashtable(selfAndDescendants.Count);
            foreach (DataTable table in selfAndDescendants)
            {
                TableSchemaInfo info = this.AddTableSchema(nameTable, table);
                if (info != null)
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        if (IsMappedColumn(column))
                        {
                            this.AddColumnSchema(nameTable, column, info.ColumnsSchemaMap);
                        }
                    }
                    foreach (DataRelation relation in table.ChildRelations)
                    {
                        if (relation.Nested)
                        {
                            string array = XmlConvert.EncodeLocalName(relation.ChildTable.TableName);
                            string localName = nameTable.Get(array);
                            if (localName == null)
                            {
                                localName = nameTable.Add(array);
                            }
                            string namespaceURI = nameTable.Get(relation.ChildTable.Namespace);
                            if (namespaceURI == null)
                            {
                                namespaceURI = nameTable.Add(relation.ChildTable.Namespace);
                            }
                            XmlNodeIdentety identety = new XmlNodeIdentety(localName, namespaceURI);
                            info.ColumnsSchemaMap[identety] = relation.ChildTable;
                        }
                    }
                }
            }
        }

        public object GetColumnSchema(XmlNode node, bool fIgnoreNamespace)
        {
            TableSchemaInfo info = null;
            XmlNode parentNode = (node.NodeType == XmlNodeType.Attribute) ? ((XmlAttribute) node).OwnerElement : node.ParentNode;
            do
            {
                if ((parentNode == null) || (parentNode.NodeType != XmlNodeType.Element))
                {
                    return null;
                }
                info = fIgnoreNamespace ? ((TableSchemaInfo) this.tableSchemaMap[parentNode.LocalName]) : ((TableSchemaInfo) this.tableSchemaMap[parentNode]);
                parentNode = parentNode.ParentNode;
            }
            while (info == null);
            if (fIgnoreNamespace)
            {
                return info.ColumnsSchemaMap[node.LocalName];
            }
            return info.ColumnsSchemaMap[node];
        }

        public object GetColumnSchema(DataTable table, XmlReader dataReader, bool fIgnoreNamespace)
        {
            if ((this.lastTableSchemaInfo == null) || (this.lastTableSchemaInfo.TableSchema != table))
            {
                this.lastTableSchemaInfo = fIgnoreNamespace ? ((TableSchemaInfo) this.tableSchemaMap[table.EncodedTableName]) : ((TableSchemaInfo) this.tableSchemaMap[table]);
            }
            if (fIgnoreNamespace)
            {
                return this.lastTableSchemaInfo.ColumnsSchemaMap[dataReader.LocalName];
            }
            return this.lastTableSchemaInfo.ColumnsSchemaMap[dataReader];
        }

        public object GetSchemaForNode(XmlNode node, bool fIgnoreNamespace)
        {
            TableSchemaInfo info = null;
            if (node.NodeType == XmlNodeType.Element)
            {
                info = fIgnoreNamespace ? ((TableSchemaInfo) this.tableSchemaMap[node.LocalName]) : ((TableSchemaInfo) this.tableSchemaMap[node]);
            }
            if (info != null)
            {
                return info.TableSchema;
            }
            return this.GetColumnSchema(node, fIgnoreNamespace);
        }

        private ArrayList GetSelfAndDescendants(DataTable dt)
        {
            ArrayList list = new ArrayList();
            list.Add(dt);
            for (int i = 0; i < list.Count; i++)
            {
                foreach (DataRelation relation in ((DataTable) list[i]).ChildRelations)
                {
                    if (!list.Contains(relation.ChildTable))
                    {
                        list.Add(relation.ChildTable);
                    }
                }
            }
            return list;
        }

        public DataTable GetTableForNode(XmlReader node, bool fIgnoreNamespace)
        {
            TableSchemaInfo info = fIgnoreNamespace ? ((TableSchemaInfo) this.tableSchemaMap[node.LocalName]) : ((TableSchemaInfo) this.tableSchemaMap[node]);
            if (info != null)
            {
                this.lastTableSchemaInfo = info;
                return this.lastTableSchemaInfo.TableSchema;
            }
            return null;
        }

        private void HandleSpecialColumn(DataColumn col, XmlNameTable nameTable, XmlNodeIdHashtable columns)
        {
            string str;
            if ('x' == col.ColumnName[0])
            {
                str = "_x0078_";
            }
            else
            {
                str = "_x0058_";
            }
            str = str + col.ColumnName.Substring(1);
            if (nameTable.Get(str) == null)
            {
                nameTable.Add(str);
            }
            string namespaceURI = nameTable.Get(col.Namespace);
            XmlNodeIdentety identety = new XmlNodeIdentety(str, namespaceURI);
            columns[identety] = col;
        }

        internal static bool IsMappedColumn(DataColumn c)
        {
            return (c.ColumnMapping != MappingType.Hidden);
        }

        private sealed class TableSchemaInfo
        {
            public XmlToDatasetMap.XmlNodeIdHashtable ColumnsSchemaMap;
            public DataTable TableSchema;

            public TableSchemaInfo(DataTable tableSchema)
            {
                this.TableSchema = tableSchema;
                this.ColumnsSchemaMap = new XmlToDatasetMap.XmlNodeIdHashtable(tableSchema.Columns.Count);
            }
        }

        private sealed class XmlNodeIdentety
        {
            public string LocalName;
            public string NamespaceURI;

            public XmlNodeIdentety(string localName, string namespaceURI)
            {
                this.LocalName = localName;
                this.NamespaceURI = namespaceURI;
            }

            public override bool Equals(object obj)
            {
                XmlToDatasetMap.XmlNodeIdentety identety = (XmlToDatasetMap.XmlNodeIdentety) obj;
                return ((string.Compare(this.LocalName, identety.LocalName, StringComparison.OrdinalIgnoreCase) == 0) && (string.Compare(this.NamespaceURI, identety.NamespaceURI, StringComparison.OrdinalIgnoreCase) == 0));
            }

            public override int GetHashCode()
            {
                return this.LocalName.GetHashCode();
            }
        }

        internal sealed class XmlNodeIdHashtable : Hashtable
        {
            private XmlToDatasetMap.XmlNodeIdentety id;

            public XmlNodeIdHashtable(int capacity) : base(capacity)
            {
                this.id = new XmlToDatasetMap.XmlNodeIdentety(string.Empty, string.Empty);
            }

            public object this[XmlNode node]
            {
                get
                {
                    this.id.LocalName = node.LocalName;
                    this.id.NamespaceURI = node.NamespaceURI;
                    return this[this.id];
                }
            }

            public object this[XmlReader dataReader]
            {
                get
                {
                    this.id.LocalName = dataReader.LocalName;
                    this.id.NamespaceURI = dataReader.NamespaceURI;
                    return this[this.id];
                }
            }

            public object this[DataTable table]
            {
                get
                {
                    this.id.LocalName = table.EncodedTableName;
                    this.id.NamespaceURI = table.Namespace;
                    return this[this.id];
                }
            }

            public object this[string name]
            {
                get
                {
                    this.id.LocalName = name;
                    this.id.NamespaceURI = string.Empty;
                    return this[this.id];
                }
            }
        }
    }
}

