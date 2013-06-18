namespace System.Data
{
    using System;
    using System.Collections;
    using System.Data.Common;
    using System.Globalization;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class NewDiffgramGen
    {
        internal XmlDocument _doc;
        internal DataSet _ds;
        internal DataTable _dt;
        private ArrayList _tables;
        private bool _writeHierarchy;
        internal XmlWriter _xmlw;
        private bool fBefore;
        private bool fErrors;
        internal Hashtable rowsOrder;

        internal NewDiffgramGen(DataSet ds)
        {
            this._tables = new ArrayList();
            this._ds = ds;
            this._dt = null;
            this._doc = new XmlDocument();
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                this._tables.Add(ds.Tables[i]);
            }
            this.DoAssignments(this._tables);
        }

        internal NewDiffgramGen(DataTable dt, bool writeHierarchy)
        {
            this._tables = new ArrayList();
            this._ds = null;
            this._dt = dt;
            this._doc = new XmlDocument();
            this._tables.Add(dt);
            if (writeHierarchy)
            {
                this._writeHierarchy = true;
                this.CreateTableHierarchy(dt);
            }
            this.DoAssignments(this._tables);
        }

        private void CreateTableHierarchy(DataTable dt)
        {
            foreach (DataRelation relation in dt.ChildRelations)
            {
                if (!this._tables.Contains(relation.ChildTable))
                {
                    this._tables.Add(relation.ChildTable);
                    this.CreateTableHierarchy(relation.ChildTable);
                }
            }
        }

        private void DoAssignments(ArrayList tables)
        {
            int capacity = 0;
            for (int i = 0; i < tables.Count; i++)
            {
                capacity += ((DataTable) tables[i]).Rows.Count;
            }
            this.rowsOrder = new Hashtable(capacity);
            for (int j = 0; j < tables.Count; j++)
            {
                DataTable table = (DataTable) tables[j];
                DataRowCollection rows = table.Rows;
                capacity = rows.Count;
                for (int k = 0; k < capacity; k++)
                {
                    this.rowsOrder[rows[k]] = k;
                }
            }
        }

        private bool EmptyData()
        {
            for (int i = 0; i < this._tables.Count; i++)
            {
                if (((DataTable) this._tables[i]).Rows.Count > 0)
                {
                    return false;
                }
            }
            return true;
        }

        private void GenerateColumn(DataRow row, DataColumn col, DataRowVersion version)
        {
            string columnValueAsString = null;
            columnValueAsString = col.GetColumnValueAsString(row, version);
            if (columnValueAsString == null)
            {
                if (col.ColumnMapping == MappingType.SimpleContent)
                {
                    this._xmlw.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
                }
            }
            else
            {
                bool flag;
                string prefix = (col.Namespace.Length != 0) ? col.Prefix : string.Empty;
                switch (col.ColumnMapping)
                {
                    case MappingType.Element:
                    {
                        flag = true;
                        object obj2 = row[col, version];
                        if ((!col.IsCustomType || !col.IsValueCustomTypeInstance(obj2)) || typeof(IXmlSerializable).IsAssignableFrom(obj2.GetType()))
                        {
                            this._xmlw.WriteStartElement(prefix, col.EncodedColumnName, col.Namespace);
                            flag = false;
                        }
                        Type type = obj2.GetType();
                        if (col.IsCustomType)
                        {
                            if ((obj2 != DBNull.Value) && (!col.ImplementsINullable || !DataStorage.IsObjectSqlNull(obj2)))
                            {
                                if (col.IsValueCustomTypeInstance(obj2))
                                {
                                    if (!flag && (obj2.GetType() != col.DataType))
                                    {
                                        this._xmlw.WriteAttributeString("msdata", "InstanceType", "urn:schemas-microsoft-com:xml-msdata", DataStorage.GetQualifiedName(type));
                                    }
                                    if (!flag)
                                    {
                                        col.ConvertObjectToXml(obj2, this._xmlw, null);
                                    }
                                    else
                                    {
                                        if (obj2.GetType() != col.DataType)
                                        {
                                            throw ExceptionBuilder.PolymorphismNotSupported(type.AssemblyQualifiedName);
                                        }
                                        XmlRootAttribute xmlAttrib = new XmlRootAttribute(col.EncodedColumnName) {
                                            Namespace = col.Namespace
                                        };
                                        col.ConvertObjectToXml(obj2, this._xmlw, xmlAttrib);
                                    }
                                }
                                else
                                {
                                    if (((type == typeof(Type)) || (type == typeof(Guid))) || ((type == typeof(char)) || DataStorage.IsSqlType(type)))
                                    {
                                        this._xmlw.WriteAttributeString("msdata", "InstanceType", "urn:schemas-microsoft-com:xml-msdata", type.FullName);
                                    }
                                    else if (obj2 is Type)
                                    {
                                        this._xmlw.WriteAttributeString("msdata", "InstanceType", "urn:schemas-microsoft-com:xml-msdata", "Type");
                                    }
                                    else
                                    {
                                        string str3 = "xs:" + XmlTreeGen.XmlDataTypeName(type);
                                        this._xmlw.WriteAttributeString("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance", str3);
                                        this._xmlw.WriteAttributeString("xmlns:xs", "http://www.w3.org/2001/XMLSchema");
                                    }
                                    if (!DataStorage.IsSqlType(type))
                                    {
                                        this._xmlw.WriteString(col.ConvertObjectToXml(obj2));
                                    }
                                    else
                                    {
                                        col.ConvertObjectToXml(obj2, this._xmlw, null);
                                    }
                                }
                            }
                            break;
                        }
                        if (((type == typeof(char)) || (type == typeof(string))) && XmlDataTreeWriter.PreserveSpace(columnValueAsString))
                        {
                            this._xmlw.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");
                        }
                        this._xmlw.WriteString(columnValueAsString);
                        break;
                    }
                    case MappingType.Attribute:
                        this._xmlw.WriteAttributeString(prefix, col.EncodedColumnName, col.Namespace, columnValueAsString);
                        return;

                    case MappingType.SimpleContent:
                        this._xmlw.WriteString(columnValueAsString);
                        return;

                    case MappingType.Hidden:
                        this._xmlw.WriteAttributeString("msdata", "hidden" + col.EncodedColumnName, "urn:schemas-microsoft-com:xml-msdata", columnValueAsString);
                        return;

                    default:
                        return;
                }
                if (!flag)
                {
                    this._xmlw.WriteEndElement();
                }
            }
        }

        private void GenerateRow(DataRow row)
        {
            DataRowState rowState = row.RowState;
            switch (rowState)
            {
                case DataRowState.Unchanged:
                case DataRowState.Added:
                    return;
            }
            if (!this.fBefore)
            {
                this._xmlw.WriteStartElement("diffgr", "before", "urn:schemas-microsoft-com:xml-diffgram-v1");
                this.fBefore = true;
            }
            DataTable table = row.Table;
            int count = table.Columns.Count;
            string str3 = table.TableName + row.rowID.ToString(CultureInfo.InvariantCulture);
            string str = null;
            if ((rowState == DataRowState.Deleted) && (row.Table.NestedParentRelations.Length != 0))
            {
                DataRow nestedParentRow = row.GetNestedParentRow(DataRowVersion.Original);
                if (nestedParentRow != null)
                {
                    str = nestedParentRow.Table.TableName + nestedParentRow.rowID.ToString(CultureInfo.InvariantCulture);
                }
            }
            string prefix = (table.Namespace.Length != 0) ? table.Prefix : string.Empty;
            if (table.XmlText != null)
            {
                object obj1 = row[table.XmlText, DataRowVersion.Original];
            }
            this._xmlw.WriteStartElement(prefix, row.Table.EncodedTableName, row.Table.Namespace);
            this._xmlw.WriteAttributeString("diffgr", "id", "urn:schemas-microsoft-com:xml-diffgram-v1", str3);
            if ((rowState == DataRowState.Deleted) && XmlDataTreeWriter.RowHasErrors(row))
            {
                this._xmlw.WriteAttributeString("diffgr", "hasErrors", "urn:schemas-microsoft-com:xml-diffgram-v1", "true");
            }
            if (str != null)
            {
                this._xmlw.WriteAttributeString("diffgr", "parentId", "urn:schemas-microsoft-com:xml-diffgram-v1", str);
            }
            this._xmlw.WriteAttributeString("msdata", "rowOrder", "urn:schemas-microsoft-com:xml-msdata", this.rowsOrder[row].ToString());
            for (int i = 0; i < count; i++)
            {
                if ((row.Table.Columns[i].ColumnMapping == MappingType.Attribute) || (row.Table.Columns[i].ColumnMapping == MappingType.Hidden))
                {
                    this.GenerateColumn(row, row.Table.Columns[i], DataRowVersion.Original);
                }
            }
            for (int j = 0; j < count; j++)
            {
                if ((row.Table.Columns[j].ColumnMapping == MappingType.Element) || (row.Table.Columns[j].ColumnMapping == MappingType.SimpleContent))
                {
                    this.GenerateColumn(row, row.Table.Columns[j], DataRowVersion.Original);
                }
            }
            this._xmlw.WriteEndElement();
        }

        private void GenerateTable(DataTable table)
        {
            int count = table.Rows.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    this.GenerateRow(table.Rows[i]);
                }
            }
        }

        private void GenerateTableErrors(DataTable table)
        {
            int count = table.Rows.Count;
            int num3 = table.Columns.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    bool flag = false;
                    DataRow row = table.Rows[i];
                    string prefix = (table.Namespace.Length != 0) ? table.Prefix : string.Empty;
                    if (row.HasErrors && (row.RowError.Length > 0))
                    {
                        if (!this.fErrors)
                        {
                            this._xmlw.WriteStartElement("diffgr", "errors", "urn:schemas-microsoft-com:xml-diffgram-v1");
                            this.fErrors = true;
                        }
                        this._xmlw.WriteStartElement(prefix, row.Table.EncodedTableName, row.Table.Namespace);
                        this._xmlw.WriteAttributeString("diffgr", "id", "urn:schemas-microsoft-com:xml-diffgram-v1", row.Table.TableName + row.rowID.ToString(CultureInfo.InvariantCulture));
                        this._xmlw.WriteAttributeString("diffgr", "Error", "urn:schemas-microsoft-com:xml-diffgram-v1", row.RowError);
                        flag = true;
                    }
                    if (num3 > 0)
                    {
                        for (int j = 0; j < num3; j++)
                        {
                            DataColumn column = table.Columns[j];
                            string columnError = row.GetColumnError(column);
                            string str3 = (column.Namespace.Length != 0) ? column.Prefix : string.Empty;
                            if ((columnError != null) && (columnError.Length != 0))
                            {
                                if (!flag)
                                {
                                    if (!this.fErrors)
                                    {
                                        this._xmlw.WriteStartElement("diffgr", "errors", "urn:schemas-microsoft-com:xml-diffgram-v1");
                                        this.fErrors = true;
                                    }
                                    this._xmlw.WriteStartElement(prefix, row.Table.EncodedTableName, row.Table.Namespace);
                                    this._xmlw.WriteAttributeString("diffgr", "id", "urn:schemas-microsoft-com:xml-diffgram-v1", row.Table.TableName + row.rowID.ToString(CultureInfo.InvariantCulture));
                                    flag = true;
                                }
                                this._xmlw.WriteStartElement(str3, column.EncodedColumnName, column.Namespace);
                                this._xmlw.WriteAttributeString("diffgr", "Error", "urn:schemas-microsoft-com:xml-diffgram-v1", columnError);
                                this._xmlw.WriteEndElement();
                            }
                        }
                        if (flag)
                        {
                            this._xmlw.WriteEndElement();
                        }
                    }
                }
            }
        }

        internal static string QualifiedName(string prefix, string name)
        {
            if (prefix != null)
            {
                return (prefix + ":" + name);
            }
            return name;
        }

        internal void Save(XmlWriter xmlw)
        {
            this.Save(xmlw, null);
        }

        internal void Save(XmlWriter xmlw, DataTable table)
        {
            this._xmlw = DataTextWriter.CreateWriter(xmlw);
            this._xmlw.WriteStartElement("diffgr", "diffgram", "urn:schemas-microsoft-com:xml-diffgram-v1");
            this._xmlw.WriteAttributeString("xmlns", "msdata", null, "urn:schemas-microsoft-com:xml-msdata");
            if (!this.EmptyData())
            {
                if (table != null)
                {
                    new XmlDataTreeWriter(table, this._writeHierarchy).SaveDiffgramData(this._xmlw, this.rowsOrder);
                }
                else
                {
                    new XmlDataTreeWriter(this._ds).SaveDiffgramData(this._xmlw, this.rowsOrder);
                }
                if (table == null)
                {
                    for (int i = 0; i < this._ds.Tables.Count; i++)
                    {
                        this.GenerateTable(this._ds.Tables[i]);
                    }
                }
                else
                {
                    for (int j = 0; j < this._tables.Count; j++)
                    {
                        this.GenerateTable((DataTable) this._tables[j]);
                    }
                }
                if (this.fBefore)
                {
                    this._xmlw.WriteEndElement();
                }
                if (table == null)
                {
                    for (int k = 0; k < this._ds.Tables.Count; k++)
                    {
                        this.GenerateTableErrors(this._ds.Tables[k]);
                    }
                }
                else
                {
                    for (int m = 0; m < this._tables.Count; m++)
                    {
                        this.GenerateTableErrors((DataTable) this._tables[m]);
                    }
                }
                if (this.fErrors)
                {
                    this._xmlw.WriteEndElement();
                }
            }
            this._xmlw.WriteEndElement();
            this._xmlw.Flush();
        }
    }
}

