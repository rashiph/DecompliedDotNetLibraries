namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Runtime.InteropServices;

    internal sealed class DataSetMapper
    {
        private Hashtable columnSchemaMap = new Hashtable();
        private DataSet dataSet;
        private XmlDataDocument doc;
        internal const string strReservedXmlns = "http://www.w3.org/2000/xmlns/";
        private Hashtable tableSchemaMap = new Hashtable();

        internal DataSetMapper()
        {
        }

        private void AddColumnSchema(DataColumn col)
        {
            DataTable table = col.Table;
            object identity = GetIdentity(table.EncodedTableName, table.Namespace);
            object obj3 = GetIdentity(col.EncodedColumnName, col.Namespace);
            Hashtable hashtable = (Hashtable) this.columnSchemaMap[identity];
            if (hashtable == null)
            {
                hashtable = new Hashtable();
                this.columnSchemaMap[identity] = hashtable;
            }
            hashtable[obj3] = col;
        }

        private void AddTableSchema(DataTable table)
        {
            object identity = GetIdentity(table.EncodedTableName, table.Namespace);
            this.tableSchemaMap[identity] = table;
        }

        internal DataColumn GetColumnSchemaForNode(XmlBoundElement rowElem, XmlNode node)
        {
            object identity = GetIdentity(rowElem.LocalName, rowElem.NamespaceURI);
            object obj2 = GetIdentity(node.LocalName, node.NamespaceURI);
            Hashtable hashtable = (Hashtable) this.columnSchemaMap[identity];
            if (hashtable != null)
            {
                DataColumn column = (DataColumn) hashtable[obj2];
                if (column == null)
                {
                    return null;
                }
                MappingType columnMapping = column.ColumnMapping;
                if ((node.NodeType == XmlNodeType.Attribute) && (columnMapping == MappingType.Attribute))
                {
                    return column;
                }
                if ((node.NodeType == XmlNodeType.Element) && (columnMapping == MappingType.Element))
                {
                    return column;
                }
            }
            return null;
        }

        private static object GetIdentity(string localName, string namespaceURI)
        {
            return (localName + ":" + namespaceURI);
        }

        internal bool GetRegion(XmlNode node, out XmlBoundElement rowElem)
        {
            while (node != null)
            {
                XmlBoundElement be = node as XmlBoundElement;
                if ((be != null) && (this.GetRowFromElement(be) != null))
                {
                    rowElem = be;
                    return true;
                }
                if (node.NodeType == XmlNodeType.Attribute)
                {
                    node = ((XmlAttribute) node).OwnerElement;
                }
                else
                {
                    node = node.ParentNode;
                }
            }
            rowElem = null;
            return false;
        }

        internal DataRow GetRowFromElement(XmlBoundElement be)
        {
            return be.Row;
        }

        internal DataRow GetRowFromElement(XmlElement e)
        {
            XmlBoundElement element = e as XmlBoundElement;
            if (element != null)
            {
                return element.Row;
            }
            return null;
        }

        internal DataTable GetTableSchemaForElement(XmlBoundElement be)
        {
            DataRow row = be.Row;
            if (row != null)
            {
                return row.Table;
            }
            return null;
        }

        internal DataTable GetTableSchemaForElement(XmlElement elem)
        {
            XmlBoundElement be = elem as XmlBoundElement;
            if (be == null)
            {
                return null;
            }
            return this.GetTableSchemaForElement(be);
        }

        internal bool IsMapped()
        {
            return (this.dataSet != null);
        }

        private bool IsNextColumn(DataColumnCollection columns, ref int iColumn, DataColumn col)
        {
            while (iColumn < columns.Count)
            {
                if (columns[iColumn] == col)
                {
                    iColumn++;
                    return true;
                }
                iColumn++;
            }
            return false;
        }

        internal static bool IsNotMapped(DataColumn c)
        {
            return (c.ColumnMapping == MappingType.Hidden);
        }

        internal bool IsRegionRadical(XmlBoundElement rowElem)
        {
            if (rowElem.ElementState != ElementState.Defoliated)
            {
                DataColumnCollection columns = this.GetTableSchemaForElement(rowElem).Columns;
                int iColumn = 0;
                int count = rowElem.Attributes.Count;
                for (int i = 0; i < count; i++)
                {
                    XmlAttribute node = rowElem.Attributes[i];
                    if (!node.Specified)
                    {
                        return false;
                    }
                    DataColumn columnSchemaForNode = this.GetColumnSchemaForNode(rowElem, node);
                    if (columnSchemaForNode == null)
                    {
                        return false;
                    }
                    if (!this.IsNextColumn(columns, ref iColumn, columnSchemaForNode))
                    {
                        return false;
                    }
                    XmlNode node3 = node.FirstChild;
                    if (((node3 == null) || (node3.NodeType != XmlNodeType.Text)) || (node3.NextSibling != null))
                    {
                        return false;
                    }
                }
                iColumn = 0;
                XmlNode firstChild = rowElem.FirstChild;
                while (firstChild != null)
                {
                    if (firstChild.NodeType != XmlNodeType.Element)
                    {
                        return false;
                    }
                    XmlElement e = firstChild as XmlElement;
                    if (this.GetRowFromElement(e) != null)
                    {
                        break;
                    }
                    DataColumn col = this.GetColumnSchemaForNode(rowElem, e);
                    if (col == null)
                    {
                        return false;
                    }
                    if (!this.IsNextColumn(columns, ref iColumn, col))
                    {
                        return false;
                    }
                    if (e.HasAttributes)
                    {
                        return false;
                    }
                    XmlNode node2 = e.FirstChild;
                    if (((node2 == null) || (node2.NodeType != XmlNodeType.Text)) || (node2.NextSibling != null))
                    {
                        return false;
                    }
                    firstChild = firstChild.NextSibling;
                }
                while (firstChild != null)
                {
                    if (firstChild.NodeType != XmlNodeType.Element)
                    {
                        return false;
                    }
                    if (this.GetRowFromElement((XmlElement) firstChild) == null)
                    {
                        return false;
                    }
                    firstChild = firstChild.NextSibling;
                }
            }
            return true;
        }

        internal DataTable SearchMatchingTableSchema(string localName, string namespaceURI)
        {
            object identity = GetIdentity(localName, namespaceURI);
            return (DataTable) this.tableSchemaMap[identity];
        }

        internal DataTable SearchMatchingTableSchema(XmlBoundElement rowElem, XmlBoundElement elem)
        {
            DataTable table = this.SearchMatchingTableSchema(elem.LocalName, elem.NamespaceURI);
            if (table != null)
            {
                if (rowElem == null)
                {
                    return table;
                }
                if (this.GetColumnSchemaForNode(rowElem, elem) == null)
                {
                    return table;
                }
                foreach (XmlAttribute attribute in elem.Attributes)
                {
                    if (attribute.NamespaceURI != "http://www.w3.org/2000/xmlns/")
                    {
                        return table;
                    }
                }
                for (XmlNode node = elem.FirstChild; node != null; node = node.NextSibling)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        return table;
                    }
                }
            }
            return null;
        }

        internal void SetupMapping(XmlDataDocument xd, DataSet ds)
        {
            if (this.IsMapped())
            {
                this.tableSchemaMap = new Hashtable();
                this.columnSchemaMap = new Hashtable();
            }
            this.doc = xd;
            this.dataSet = ds;
            foreach (DataTable table in this.dataSet.Tables)
            {
                this.AddTableSchema(table);
                foreach (DataColumn column in table.Columns)
                {
                    if (!IsNotMapped(column))
                    {
                        this.AddColumnSchema(column);
                    }
                }
            }
        }
    }
}

