namespace System.Data
{
    using System;
    using System.Collections;
    using System.Data.Common;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class XMLDiffLoader
    {
        private DataSet dataSet;
        private DataTable dataTable;
        private ArrayList tables;

        private void CreateTablesHierarchy(DataTable dt)
        {
            foreach (DataRelation relation in dt.ChildRelations)
            {
                if (!this.tables.Contains(relation.ChildTable))
                {
                    this.tables.Add(relation.ChildTable);
                    this.CreateTablesHierarchy(relation.ChildTable);
                }
            }
        }

        private DataTable GetTable(string tableName, string ns)
        {
            if (this.tables == null)
            {
                return this.dataSet.Tables.GetTable(tableName, ns);
            }
            if (this.tables.Count == 0)
            {
                return (DataTable) this.tables[0];
            }
            for (int i = 0; i < this.tables.Count; i++)
            {
                DataTable table = (DataTable) this.tables[i];
                if ((string.Compare(table.TableName, tableName, StringComparison.Ordinal) == 0) && (string.Compare(table.Namespace, ns, StringComparison.Ordinal) == 0))
                {
                    return table;
                }
            }
            return null;
        }

        internal void LoadDiffGram(DataSet ds, XmlReader dataTextReader)
        {
            XmlReader ssync = DataTextReader.CreateReader(dataTextReader);
            this.dataSet = ds;
            while ((ssync.LocalName == "before") && (ssync.NamespaceURI == "urn:schemas-microsoft-com:xml-diffgram-v1"))
            {
                this.ProcessDiffs(ds, ssync);
                ssync.Read();
            }
            while ((ssync.LocalName == "errors") && (ssync.NamespaceURI == "urn:schemas-microsoft-com:xml-diffgram-v1"))
            {
                this.ProcessErrors(ds, ssync);
                ssync.Read();
            }
        }

        internal void LoadDiffGram(DataTable dt, XmlReader dataTextReader)
        {
            XmlReader ssync = DataTextReader.CreateReader(dataTextReader);
            this.dataTable = dt;
            this.tables = new ArrayList();
            this.tables.Add(dt);
            this.CreateTablesHierarchy(dt);
            while ((ssync.LocalName == "before") && (ssync.NamespaceURI == "urn:schemas-microsoft-com:xml-diffgram-v1"))
            {
                this.ProcessDiffs(this.tables, ssync);
                ssync.Read();
            }
            while ((ssync.LocalName == "errors") && (ssync.NamespaceURI == "urn:schemas-microsoft-com:xml-diffgram-v1"))
            {
                this.ProcessErrors(this.tables, ssync);
                ssync.Read();
            }
        }

        internal void ProcessDiffs(ArrayList tableList, XmlReader ssync)
        {
            int pos = -1;
            int depth = ssync.Depth;
            ssync.Read();
            while (depth < ssync.Depth)
            {
                DataTable table = null;
                string attribute = null;
                int num = -1;
                int num1 = ssync.Depth;
                attribute = ssync.GetAttribute("id", "urn:schemas-microsoft-com:xml-diffgram-v1");
                bool flag = ssync.GetAttribute("hasErrors", "urn:schemas-microsoft-com:xml-diffgram-v1") == "true";
                num = this.ReadOldRowData(this.dataSet, ref table, ref pos, ssync);
                if (num != -1)
                {
                    if (table == null)
                    {
                        throw ExceptionBuilder.DiffgramMissingSQL();
                    }
                    DataRow row = (DataRow) table.RowDiffId[attribute];
                    if (row != null)
                    {
                        row.oldRecord = num;
                        table.recordManager[num] = row;
                    }
                    else
                    {
                        row = table.NewEmptyRow();
                        table.recordManager[num] = row;
                        row.oldRecord = num;
                        row.newRecord = num;
                        table.Rows.DiffInsertAt(row, pos);
                        row.Delete();
                        if (flag)
                        {
                            table.RowDiffId[attribute] = row;
                        }
                    }
                }
            }
        }

        internal void ProcessDiffs(DataSet ds, XmlReader ssync)
        {
            int pos = -1;
            int depth = ssync.Depth;
            ssync.Read();
            this.SkipWhitespaces(ssync);
            while (depth < ssync.Depth)
            {
                DataTable table = null;
                string attribute = null;
                int num = -1;
                int num1 = ssync.Depth;
                attribute = ssync.GetAttribute("id", "urn:schemas-microsoft-com:xml-diffgram-v1");
                bool flag = ssync.GetAttribute("hasErrors", "urn:schemas-microsoft-com:xml-diffgram-v1") == "true";
                num = this.ReadOldRowData(ds, ref table, ref pos, ssync);
                if (num != -1)
                {
                    if (table == null)
                    {
                        throw ExceptionBuilder.DiffgramMissingSQL();
                    }
                    DataRow row = (DataRow) table.RowDiffId[attribute];
                    if (row != null)
                    {
                        row.oldRecord = num;
                        table.recordManager[num] = row;
                    }
                    else
                    {
                        row = table.NewEmptyRow();
                        table.recordManager[num] = row;
                        row.oldRecord = num;
                        row.newRecord = num;
                        table.Rows.DiffInsertAt(row, pos);
                        row.Delete();
                        if (flag)
                        {
                            table.RowDiffId[attribute] = row;
                        }
                    }
                }
            }
        }

        internal void ProcessErrors(ArrayList dt, XmlReader ssync)
        {
            int depth = ssync.Depth;
            ssync.Read();
            while (depth < ssync.Depth)
            {
                DataTable table = this.GetTable(XmlConvert.DecodeName(ssync.LocalName), ssync.NamespaceURI);
                if (table == null)
                {
                    throw ExceptionBuilder.DiffgramMissingSQL();
                }
                string attribute = ssync.GetAttribute("id", "urn:schemas-microsoft-com:xml-diffgram-v1");
                DataRow row = (DataRow) table.RowDiffId[attribute];
                if (row == null)
                {
                    for (int i = 0; i < dt.Count; i++)
                    {
                        row = (DataRow) ((DataTable) dt[i]).RowDiffId[attribute];
                        if (row != null)
                        {
                            table = row.Table;
                            break;
                        }
                    }
                }
                string str = ssync.GetAttribute("Error", "urn:schemas-microsoft-com:xml-diffgram-v1");
                if (str != null)
                {
                    row.RowError = str;
                }
                int num3 = ssync.Depth;
                ssync.Read();
                while (num3 < ssync.Depth)
                {
                    if (XmlNodeType.Element == ssync.NodeType)
                    {
                        DataColumn column = table.Columns[XmlConvert.DecodeName(ssync.LocalName), ssync.NamespaceURI];
                        string error = ssync.GetAttribute("Error", "urn:schemas-microsoft-com:xml-diffgram-v1");
                        row.SetColumnError(column, error);
                    }
                    ssync.Read();
                }
                while ((ssync.NodeType == XmlNodeType.EndElement) && (depth < ssync.Depth))
                {
                    ssync.Read();
                }
            }
        }

        internal void ProcessErrors(DataSet ds, XmlReader ssync)
        {
            int depth = ssync.Depth;
            ssync.Read();
            while (depth < ssync.Depth)
            {
                DataTable table = ds.Tables.GetTable(XmlConvert.DecodeName(ssync.LocalName), ssync.NamespaceURI);
                if (table == null)
                {
                    throw ExceptionBuilder.DiffgramMissingSQL();
                }
                string attribute = ssync.GetAttribute("id", "urn:schemas-microsoft-com:xml-diffgram-v1");
                DataRow row = (DataRow) table.RowDiffId[attribute];
                string str = ssync.GetAttribute("Error", "urn:schemas-microsoft-com:xml-diffgram-v1");
                if (str != null)
                {
                    row.RowError = str;
                }
                int num2 = ssync.Depth;
                ssync.Read();
                while (num2 < ssync.Depth)
                {
                    if (XmlNodeType.Element == ssync.NodeType)
                    {
                        DataColumn column = table.Columns[XmlConvert.DecodeName(ssync.LocalName), ssync.NamespaceURI];
                        string error = ssync.GetAttribute("Error", "urn:schemas-microsoft-com:xml-diffgram-v1");
                        row.SetColumnError(column, error);
                    }
                    ssync.Read();
                }
                while ((ssync.NodeType == XmlNodeType.EndElement) && (depth < ssync.Depth))
                {
                    ssync.Read();
                }
            }
        }

        private int ReadOldRowData(DataSet ds, ref DataTable table, ref int pos, XmlReader row)
        {
            if (ds != null)
            {
                table = ds.Tables.GetTable(XmlConvert.DecodeName(row.LocalName), row.NamespaceURI);
            }
            else
            {
                table = this.GetTable(XmlConvert.DecodeName(row.LocalName), row.NamespaceURI);
            }
            if (table == null)
            {
                row.Skip();
                return -1;
            }
            int depth = row.Depth;
            string str = null;
            if (table == null)
            {
                throw ExceptionBuilder.DiffgramMissingTable(XmlConvert.DecodeName(row.LocalName));
            }
            str = row.GetAttribute("rowOrder", "urn:schemas-microsoft-com:xml-msdata");
            if (!ADP.IsEmpty(str))
            {
                pos = (int) Convert.ChangeType(str, typeof(int), null);
            }
            int num = table.NewRecord();
            foreach (DataColumn column4 in table.Columns)
            {
                column4[num] = DBNull.Value;
            }
            foreach (DataColumn column2 in table.Columns)
            {
                if ((column2.ColumnMapping != MappingType.Element) && (column2.ColumnMapping != MappingType.SimpleContent))
                {
                    if (column2.ColumnMapping == MappingType.Hidden)
                    {
                        str = row.GetAttribute("hidden" + column2.EncodedColumnName, "urn:schemas-microsoft-com:xml-msdata");
                    }
                    else
                    {
                        str = row.GetAttribute(column2.EncodedColumnName, column2.Namespace);
                    }
                    if (str != null)
                    {
                        column2[num] = column2.ConvertXmlToObject(str);
                    }
                }
            }
            row.Read();
            this.SkipWhitespaces(row);
            if (row.Depth > depth)
            {
                if (table.XmlText == null)
                {
                    while (row.Depth > depth)
                    {
                        string str3 = XmlConvert.DecodeName(row.LocalName);
                        string namespaceURI = row.NamespaceURI;
                        DataColumn column = table.Columns[str3, namespaceURI];
                        if (column == null)
                        {
                            while (((row.NodeType != XmlNodeType.EndElement) && (row.LocalName != str3)) && (row.NamespaceURI != namespaceURI))
                            {
                                row.Read();
                            }
                            row.Read();
                        }
                        else if (column.IsCustomType)
                        {
                            bool flag2 = ((column.DataType == typeof(object)) || (row.GetAttribute("InstanceType", "urn:schemas-microsoft-com:xml-msdata") != null)) || (row.GetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance") != null);
                            bool flag = false;
                            if ((column.Table.DataSet != null) && column.Table.DataSet.UdtIsWrapped)
                            {
                                row.Read();
                                flag = true;
                            }
                            XmlRootAttribute xmlAttrib = null;
                            if (!flag2 && !column.ImplementsIXMLSerializable)
                            {
                                if (flag)
                                {
                                    xmlAttrib = new XmlRootAttribute(row.LocalName) {
                                        Namespace = row.NamespaceURI
                                    };
                                }
                                else
                                {
                                    xmlAttrib = new XmlRootAttribute(column.EncodedColumnName) {
                                        Namespace = column.Namespace
                                    };
                                }
                            }
                            column[num] = column.ConvertXmlToObject(row, xmlAttrib);
                            if (flag)
                            {
                                row.Read();
                            }
                        }
                        else
                        {
                            int num3 = row.Depth;
                            row.Read();
                            if (row.Depth > num3)
                            {
                                if (((row.NodeType == XmlNodeType.Text) || (row.NodeType == XmlNodeType.Whitespace)) || (row.NodeType == XmlNodeType.SignificantWhitespace))
                                {
                                    string s = row.ReadString();
                                    column[num] = column.ConvertXmlToObject(s);
                                    row.Read();
                                }
                            }
                            else if (column.DataType == typeof(string))
                            {
                                column[num] = string.Empty;
                            }
                        }
                    }
                }
                else
                {
                    DataColumn xmlText = table.XmlText;
                    xmlText[num] = xmlText.ConvertXmlToObject(row.ReadString());
                }
                row.Read();
                this.SkipWhitespaces(row);
            }
            return num;
        }

        internal void SkipWhitespaces(XmlReader reader)
        {
            while ((reader.NodeType == XmlNodeType.Whitespace) || (reader.NodeType == XmlNodeType.SignificantWhitespace))
            {
                reader.Read();
            }
        }
    }
}

