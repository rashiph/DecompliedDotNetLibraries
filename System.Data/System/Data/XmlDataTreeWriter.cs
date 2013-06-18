namespace System.Data
{
    using System;
    using System.Collections;
    using System.Data.Common;
    using System.Globalization;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class XmlDataTreeWriter
    {
        private DataSet _ds;
        private DataTable _dt;
        private ArrayList _dTables;
        private bool _writeHierarchy;
        private XmlWriter _xmlw;
        private bool fFromTable;
        private bool isDiffgram;
        private Hashtable rowsOrder;
        private DataTable[] topLevelTables;

        internal XmlDataTreeWriter(DataSet ds)
        {
            this._dTables = new ArrayList();
            this._ds = ds;
            this.topLevelTables = ds.TopLevelTables();
            foreach (DataTable table in ds.Tables)
            {
                this._dTables.Add(table);
            }
        }

        internal XmlDataTreeWriter(DataSet ds, DataTable dt)
        {
            this._dTables = new ArrayList();
            this._ds = ds;
            this._dt = dt;
            this._dTables.Add(dt);
            this.topLevelTables = ds.TopLevelTables();
        }

        internal XmlDataTreeWriter(DataTable dt, bool writeHierarchy)
        {
            this._dTables = new ArrayList();
            this._dt = dt;
            this.fFromTable = true;
            if (dt.DataSet == null)
            {
                this._dTables.Add(dt);
                this.topLevelTables = new DataTable[] { dt };
            }
            else
            {
                this._ds = dt.DataSet;
                this._dTables.Add(dt);
                if (writeHierarchy)
                {
                    this._writeHierarchy = true;
                    this.CreateTablesHierarchy(dt);
                    this.topLevelTables = this.CreateToplevelTables();
                }
                else
                {
                    this.topLevelTables = new DataTable[] { dt };
                }
            }
        }

        private void CreateTablesHierarchy(DataTable dt)
        {
            foreach (DataRelation relation in dt.ChildRelations)
            {
                if (!this._dTables.Contains(relation.ChildTable))
                {
                    this._dTables.Add(relation.ChildTable);
                    this.CreateTablesHierarchy(relation.ChildTable);
                }
            }
        }

        private DataTable[] CreateToplevelTables()
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < this._dTables.Count; i++)
            {
                DataTable table = (DataTable) this._dTables[i];
                if (table.ParentRelations.Count == 0)
                {
                    list.Add(table);
                    continue;
                }
                bool flag = false;
                for (int j = 0; j < table.ParentRelations.Count; j++)
                {
                    if (table.ParentRelations[j].Nested)
                    {
                        if (table.ParentRelations[j].ParentTable == table)
                        {
                            flag = false;
                            break;
                        }
                        flag = true;
                    }
                }
                if (!flag)
                {
                    list.Add(table);
                }
            }
            if (list.Count == 0)
            {
                return new DataTable[0];
            }
            DataTable[] array = new DataTable[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        private ArrayList GetNestedChildRelations(DataRow row)
        {
            ArrayList list = new ArrayList();
            foreach (DataRelation relation in row.Table.ChildRelations)
            {
                if (relation.Nested)
                {
                    list.Add(relation);
                }
            }
            return list;
        }

        internal static bool PreserveSpace(object value)
        {
            string s = value.ToString();
            if (s.Length == 0)
            {
                return false;
            }
            for (int i = 0; i < s.Length; i++)
            {
                if (!char.IsWhiteSpace(s, i))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool RowHasErrors(DataRow row)
        {
            int count = row.Table.Columns.Count;
            if (row.HasErrors && (row.RowError.Length > 0))
            {
                return true;
            }
            for (int i = 0; i < count; i++)
            {
                DataColumn column = row.Table.Columns[i];
                string columnError = row.GetColumnError(column);
                if ((columnError != null) && (columnError.Length != 0))
                {
                    return true;
                }
            }
            return false;
        }

        internal void Save(XmlWriter xw, bool writeSchema)
        {
            this._xmlw = DataTextWriter.CreateWriter(xw);
            int length = this.topLevelTables.Length;
            bool flag = true;
            string prefix = (this._ds != null) ? ((this._ds.Namespace.Length == 0) ? "" : this._ds.Prefix) : ((this._dt.Namespace.Length == 0) ? "" : this._dt.Prefix);
            if (((!writeSchema && (this._ds != null)) && (this._ds.fTopLevelTable && (length == 1))) && (this._ds.TopLevelTables()[0].Rows.Count == 1))
            {
                flag = false;
            }
            if (flag)
            {
                if (this._ds == null)
                {
                    this._xmlw.WriteStartElement(prefix, "DocumentElement", this._dt.Namespace);
                }
                else if ((this._ds.DataSetName == null) || (this._ds.DataSetName.Length == 0))
                {
                    this._xmlw.WriteStartElement(prefix, "DocumentElement", this._ds.Namespace);
                }
                else
                {
                    this._xmlw.WriteStartElement(prefix, XmlConvert.EncodeLocalName(this._ds.DataSetName), this._ds.Namespace);
                }
                for (int j = 0; j < this._dTables.Count; j++)
                {
                    if (((DataTable) this._dTables[j]).xmlText != null)
                    {
                        this._xmlw.WriteAttributeString("xmlns", "xsi", "http://www.w3.org/2000/xmlns/", "http://www.w3.org/2001/XMLSchema-instance");
                        break;
                    }
                }
                if (writeSchema)
                {
                    if (!this.fFromTable)
                    {
                        new XmlTreeGen(SchemaFormat.Public).Save(this._ds, this._xmlw);
                    }
                    else
                    {
                        new XmlTreeGen(SchemaFormat.Public).Save(null, this._dt, this._xmlw, this._writeHierarchy);
                    }
                }
            }
            for (int i = 0; i < this._dTables.Count; i++)
            {
                foreach (DataRow row in ((DataTable) this._dTables[i]).Rows)
                {
                    if (row.RowState != DataRowState.Deleted)
                    {
                        int nestedParentCount = row.GetNestedParentCount();
                        if (nestedParentCount == 0)
                        {
                            this.XmlDataRowWriter(row, ((DataTable) this._dTables[i]).EncodedTableName);
                        }
                        else if (nestedParentCount > 1)
                        {
                            DataTable table = (DataTable) this._dTables[i];
                            throw ExceptionBuilder.MultipleParentRows((table.Namespace.Length == 0) ? table.TableName : (table.Namespace + table.TableName));
                        }
                    }
                }
            }
            if (flag)
            {
                this._xmlw.WriteEndElement();
            }
            this._xmlw.Flush();
        }

        internal void SaveDiffgramData(XmlWriter xw, Hashtable rowsOrder)
        {
            this._xmlw = DataTextWriter.CreateWriter(xw);
            this.isDiffgram = true;
            this.rowsOrder = rowsOrder;
            string prefix = (this._ds != null) ? ((this._ds.Namespace.Length == 0) ? "" : this._ds.Prefix) : ((this._dt.Namespace.Length == 0) ? "" : this._dt.Prefix);
            if (((this._ds == null) || (this._ds.DataSetName == null)) || (this._ds.DataSetName.Length == 0))
            {
                this._xmlw.WriteStartElement(prefix, "DocumentElement", (this._dt.Namespace == null) ? "" : this._dt.Namespace);
            }
            else
            {
                this._xmlw.WriteStartElement(prefix, XmlConvert.EncodeLocalName(this._ds.DataSetName), this._ds.Namespace);
            }
            for (int i = 0; i < this._dTables.Count; i++)
            {
                DataTable table = (DataTable) this._dTables[i];
                foreach (DataRow row in table.Rows)
                {
                    if (row.RowState != DataRowState.Deleted)
                    {
                        int nestedParentCount = row.GetNestedParentCount();
                        if (nestedParentCount == 0)
                        {
                            DataTable table2 = (DataTable) this._dTables[i];
                            this.XmlDataRowWriter(row, table2.EncodedTableName);
                        }
                        else if (nestedParentCount > 1)
                        {
                            throw ExceptionBuilder.MultipleParentRows((table.Namespace.Length == 0) ? table.TableName : (table.Namespace + table.TableName));
                        }
                    }
                }
            }
            this._xmlw.WriteEndElement();
            this._xmlw.Flush();
        }

        internal void XmlDataRowWriter(DataRow row, string encodedTableName)
        {
            object obj2;
            string prefix = (row.Table.Namespace.Length == 0) ? "" : row.Table.Prefix;
            this._xmlw.WriteStartElement(prefix, encodedTableName, row.Table.Namespace);
            if (this.isDiffgram)
            {
                this._xmlw.WriteAttributeString("diffgr", "id", "urn:schemas-microsoft-com:xml-diffgram-v1", row.Table.TableName + row.rowID.ToString(CultureInfo.InvariantCulture));
                this._xmlw.WriteAttributeString("msdata", "rowOrder", "urn:schemas-microsoft-com:xml-msdata", this.rowsOrder[row].ToString());
                if (row.RowState == DataRowState.Added)
                {
                    this._xmlw.WriteAttributeString("diffgr", "hasChanges", "urn:schemas-microsoft-com:xml-diffgram-v1", "inserted");
                }
                if (row.RowState == DataRowState.Modified)
                {
                    this._xmlw.WriteAttributeString("diffgr", "hasChanges", "urn:schemas-microsoft-com:xml-diffgram-v1", "modified");
                }
                if (RowHasErrors(row))
                {
                    this._xmlw.WriteAttributeString("diffgr", "hasErrors", "urn:schemas-microsoft-com:xml-diffgram-v1", "true");
                }
            }
            foreach (DataColumn column2 in row.Table.Columns)
            {
                if (column2.columnMapping == MappingType.Attribute)
                {
                    obj2 = row[column2];
                    string str3 = (column2.Namespace.Length == 0) ? "" : column2.Prefix;
                    if ((obj2 != DBNull.Value) && (!column2.ImplementsINullable || !DataStorage.IsObjectSqlNull(obj2)))
                    {
                        XmlTreeGen.ValidateColumnMapping(column2.DataType);
                        this._xmlw.WriteAttributeString(str3, column2.EncodedColumnName, column2.Namespace, column2.ConvertObjectToXml(obj2));
                    }
                }
                if (this.isDiffgram && (column2.columnMapping == MappingType.Hidden))
                {
                    obj2 = row[column2];
                    if ((obj2 != DBNull.Value) && (!column2.ImplementsINullable || !DataStorage.IsObjectSqlNull(obj2)))
                    {
                        XmlTreeGen.ValidateColumnMapping(column2.DataType);
                        this._xmlw.WriteAttributeString("msdata", "hidden" + column2.EncodedColumnName, "urn:schemas-microsoft-com:xml-msdata", column2.ConvertObjectToXml(obj2));
                    }
                }
            }
            foreach (DataColumn column in row.Table.Columns)
            {
                if (column.columnMapping != MappingType.Hidden)
                {
                    obj2 = row[column];
                    string str2 = (column.Namespace.Length == 0) ? "" : column.Prefix;
                    bool flag = true;
                    if (((obj2 == DBNull.Value) || (column.ImplementsINullable && DataStorage.IsObjectSqlNull(obj2))) && (column.ColumnMapping == MappingType.SimpleContent))
                    {
                        this._xmlw.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
                    }
                    if (((obj2 != DBNull.Value) && (!column.ImplementsINullable || !DataStorage.IsObjectSqlNull(obj2))) && (column.columnMapping != MappingType.Attribute))
                    {
                        if ((column.columnMapping != MappingType.SimpleContent) && ((!column.IsCustomType || !column.IsValueCustomTypeInstance(obj2)) || typeof(IXmlSerializable).IsAssignableFrom(obj2.GetType())))
                        {
                            this._xmlw.WriteStartElement(str2, column.EncodedColumnName, column.Namespace);
                            flag = false;
                        }
                        Type type = obj2.GetType();
                        if (!column.IsCustomType)
                        {
                            if (((type == typeof(char)) || (type == typeof(string))) && PreserveSpace(obj2))
                            {
                                this._xmlw.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");
                            }
                            this._xmlw.WriteString(column.ConvertObjectToXml(obj2));
                        }
                        else if (column.IsValueCustomTypeInstance(obj2))
                        {
                            if (!flag && (type != column.DataType))
                            {
                                this._xmlw.WriteAttributeString("msdata", "InstanceType", "urn:schemas-microsoft-com:xml-msdata", DataStorage.GetQualifiedName(type));
                            }
                            if (!flag)
                            {
                                column.ConvertObjectToXml(obj2, this._xmlw, null);
                            }
                            else
                            {
                                if (obj2.GetType() != column.DataType)
                                {
                                    throw ExceptionBuilder.PolymorphismNotSupported(type.AssemblyQualifiedName);
                                }
                                XmlRootAttribute xmlAttrib = new XmlRootAttribute(column.EncodedColumnName) {
                                    Namespace = column.Namespace
                                };
                                column.ConvertObjectToXml(obj2, this._xmlw, xmlAttrib);
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
                                string str = "xs:" + XmlTreeGen.XmlDataTypeName(type);
                                this._xmlw.WriteAttributeString("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance", str);
                                this._xmlw.WriteAttributeString("xmlns:xs", "http://www.w3.org/2001/XMLSchema");
                            }
                            if (!DataStorage.IsSqlType(type))
                            {
                                this._xmlw.WriteString(column.ConvertObjectToXml(obj2));
                            }
                            else
                            {
                                column.ConvertObjectToXml(obj2, this._xmlw, null);
                            }
                        }
                        if ((column.columnMapping != MappingType.SimpleContent) && !flag)
                        {
                            this._xmlw.WriteEndElement();
                        }
                    }
                }
            }
            if (this._ds != null)
            {
                foreach (DataRelation relation in this.GetNestedChildRelations(row))
                {
                    foreach (DataRow row2 in row.GetChildRows(relation))
                    {
                        this.XmlDataRowWriter(row2, relation.ChildTable.EncodedTableName);
                    }
                }
            }
            this._xmlw.WriteEndElement();
        }
    }
}

