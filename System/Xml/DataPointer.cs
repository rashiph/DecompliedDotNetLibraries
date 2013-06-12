namespace System.Xml
{
    using System;
    using System.Data;
    using System.Diagnostics;

    internal sealed class DataPointer : IXmlDataVirtualNode
    {
        private bool _isInUse;
        private bool bNeedFoliate;
        private DataColumn column;
        private XmlDataDocument doc;
        private bool fOnValue;
        private XmlNode node;

        internal DataPointer(DataPointer pointer)
        {
            this.doc = pointer.doc;
            this.node = pointer.node;
            this.column = pointer.column;
            this.fOnValue = pointer.fOnValue;
            this.bNeedFoliate = false;
            this._isInUse = true;
        }

        internal DataPointer(XmlDataDocument doc, XmlNode node)
        {
            this.doc = doc;
            this.node = node;
            this.column = null;
            this.fOnValue = false;
            this.bNeedFoliate = false;
            this._isInUse = true;
        }

        internal void AddPointer()
        {
            this.doc.AddPointer(this);
        }

        [Conditional("DEBUG")]
        private void AssertValid()
        {
            if (this.column != null)
            {
                XmlBoundElement node = this.node as XmlBoundElement;
                DataRow row = node.Row;
                ElementState elementState = node.ElementState;
                DataRowState rowState = row.RowState;
            }
        }

        private int ColumnCount(DataRow row, bool fAttribute, bool fNulls)
        {
            DataColumn col = null;
            int num = 0;
            while ((col = this.NextColumn(row, col, fAttribute, fNulls)) != null)
            {
                num++;
            }
            return num;
        }

        internal XmlNode GetNode()
        {
            return this.node;
        }

        private XmlBoundElement GetRowElement()
        {
            XmlBoundElement element;
            if (this.column != null)
            {
                return (this.node as XmlBoundElement);
            }
            this.doc.Mapper.GetRegion(this.node, out element);
            return element;
        }

        private static bool IsFoliated(XmlNode node)
        {
            if ((node != null) && (node is XmlBoundElement))
            {
                return ((XmlBoundElement) node).IsFoliated;
            }
            return true;
        }

        private bool IsLocalNameEmpty(XmlNodeType nt)
        {
            switch (nt)
            {
                case XmlNodeType.None:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Comment:
                case XmlNodeType.Document:
                case XmlNodeType.DocumentFragment:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.EndElement:
                case XmlNodeType.EndEntity:
                    return true;

                case XmlNodeType.Element:
                case XmlNodeType.Attribute:
                case XmlNodeType.EntityReference:
                case XmlNodeType.Entity:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.DocumentType:
                case XmlNodeType.Notation:
                case XmlNodeType.XmlDeclaration:
                    return false;
            }
            return true;
        }

        internal void MoveTo(DataPointer pointer)
        {
            this.doc = pointer.doc;
            this.node = pointer.node;
            this.column = pointer.column;
            this.fOnValue = pointer.fOnValue;
        }

        private void MoveTo(XmlNode node)
        {
            this.node = node;
            this.column = null;
            this.fOnValue = false;
        }

        private void MoveTo(XmlNode node, DataColumn column, bool fOnValue)
        {
            this.node = node;
            this.column = column;
            this.fOnValue = fOnValue;
        }

        internal bool MoveToAttribute(int i)
        {
            this.RealFoliate();
            if ((i >= 0) && (((this.node != null) && ((this.column == null) || (this.column.ColumnMapping == MappingType.Attribute))) && (this.node.NodeType == XmlNodeType.Element)))
            {
                if (!IsFoliated(this.node))
                {
                    DataColumn column = this.NthColumn(this.Row, true, i, false);
                    if (column != null)
                    {
                        this.MoveTo(this.node, column, false);
                        return true;
                    }
                }
                else
                {
                    XmlNode node = this.node.Attributes.Item(i);
                    if (node != null)
                    {
                        this.MoveTo(node, null, false);
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool MoveToFirstChild()
        {
            this.RealFoliate();
            if (this.node != null)
            {
                if (this.column != null)
                {
                    if (this.fOnValue)
                    {
                        return false;
                    }
                    this.fOnValue = true;
                    return true;
                }
                if (!IsFoliated(this.node))
                {
                    DataColumn column = this.NextColumn(this.Row, null, false, false);
                    if (column != null)
                    {
                        this.MoveTo(this.node, column, this.doc.IsTextOnly(column));
                        return true;
                    }
                }
                XmlNode node = this.doc.SafeFirstChild(this.node);
                if (node != null)
                {
                    this.MoveTo(node);
                    return true;
                }
            }
            return false;
        }

        internal bool MoveToNextSibling()
        {
            this.RealFoliate();
            if (this.node != null)
            {
                if (this.column != null)
                {
                    if (this.fOnValue && !this.doc.IsTextOnly(this.column))
                    {
                        return false;
                    }
                    DataColumn column = this.NextColumn(this.Row, this.column, false, false);
                    if (column != null)
                    {
                        this.MoveTo(this.node, column, false);
                        return true;
                    }
                    XmlNode node2 = this.doc.SafeFirstChild(this.node);
                    if (node2 != null)
                    {
                        this.MoveTo(node2);
                        return true;
                    }
                }
                else
                {
                    XmlNode node = this.doc.SafeNextSibling(this.node);
                    if (node != null)
                    {
                        this.MoveTo(node);
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool MoveToOwnerElement()
        {
            this.RealFoliate();
            if (this.node != null)
            {
                if (this.column != null)
                {
                    if ((this.fOnValue || this.doc.IsTextOnly(this.column)) || (this.column.ColumnMapping != MappingType.Attribute))
                    {
                        return false;
                    }
                    this.MoveTo(this.node, null, false);
                    return true;
                }
                if (this.node.NodeType == XmlNodeType.Attribute)
                {
                    XmlNode ownerElement = ((XmlAttribute) this.node).OwnerElement;
                    if (ownerElement != null)
                    {
                        this.MoveTo(ownerElement, null, false);
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool MoveToParent()
        {
            this.RealFoliate();
            if (this.node != null)
            {
                if (this.column != null)
                {
                    if (this.fOnValue && !this.doc.IsTextOnly(this.column))
                    {
                        this.MoveTo(this.node, this.column, false);
                        return true;
                    }
                    if (this.column.ColumnMapping != MappingType.Attribute)
                    {
                        this.MoveTo(this.node, null, false);
                        return true;
                    }
                }
                else
                {
                    XmlNode parentNode = this.node.ParentNode;
                    if (parentNode != null)
                    {
                        this.MoveTo(parentNode);
                        return true;
                    }
                }
            }
            return false;
        }

        private DataColumn NextColumn(DataRow row, DataColumn col, bool fAttribute, bool fNulls)
        {
            if (row.RowState != DataRowState.Deleted)
            {
                DataColumnCollection columns = row.Table.Columns;
                int num = (col != null) ? (col.Ordinal + 1) : 0;
                int count = columns.Count;
                DataRowVersion version = (row.RowState == DataRowState.Detached) ? DataRowVersion.Proposed : DataRowVersion.Current;
                while (num < count)
                {
                    DataColumn c = columns[num];
                    if ((!this.doc.IsNotMapped(c) && ((c.ColumnMapping == MappingType.Attribute) == fAttribute)) && (fNulls || !Convert.IsDBNull(row[c, version])))
                    {
                        return c;
                    }
                    num++;
                }
            }
            return null;
        }

        private DataColumn NthColumn(DataRow row, bool fAttribute, int iColumn, bool fNulls)
        {
            DataColumn col = null;
            while ((col = this.NextColumn(row, col, fAttribute, fNulls)) != null)
            {
                if (iColumn == 0)
                {
                    return col;
                }
                iColumn--;
            }
            return null;
        }

        internal void RealFoliate()
        {
            if (this.bNeedFoliate)
            {
                XmlNode firstChild = null;
                if (this.doc.IsTextOnly(this.column))
                {
                    firstChild = this.node.FirstChild;
                }
                else
                {
                    if (this.column.ColumnMapping == MappingType.Attribute)
                    {
                        firstChild = this.node.Attributes.GetNamedItem(this.column.EncodedColumnName, this.column.Namespace);
                    }
                    else
                    {
                        firstChild = this.node.FirstChild;
                        while (firstChild != null)
                        {
                            if ((firstChild.LocalName == this.column.EncodedColumnName) && (firstChild.NamespaceURI == this.column.Namespace))
                            {
                                break;
                            }
                            firstChild = firstChild.NextSibling;
                        }
                    }
                    if ((firstChild != null) && this.fOnValue)
                    {
                        firstChild = firstChild.FirstChild;
                    }
                }
                if (firstChild == null)
                {
                    throw new InvalidOperationException(System.Data.Res.GetString("DataDom_Foliation"));
                }
                this.node = firstChild;
                this.column = null;
                this.fOnValue = false;
                this.bNeedFoliate = false;
            }
        }

        internal void SetNoLongerUse()
        {
            this.node = null;
            this.column = null;
            this.fOnValue = false;
            this.bNeedFoliate = false;
            this._isInUse = false;
        }

        bool IXmlDataVirtualNode.IsInUse()
        {
            return this._isInUse;
        }

        bool IXmlDataVirtualNode.IsOnColumn(DataColumn col)
        {
            this.RealFoliate();
            return (col == this.column);
        }

        bool IXmlDataVirtualNode.IsOnNode(XmlNode nodeToCheck)
        {
            this.RealFoliate();
            return (nodeToCheck == this.node);
        }

        void IXmlDataVirtualNode.OnFoliated(XmlNode foliatedNode)
        {
            if ((this.node == foliatedNode) && (this.column != null))
            {
                this.bNeedFoliate = true;
            }
        }

        internal int AttributeCount
        {
            get
            {
                this.RealFoliate();
                if (((this.node == null) || (this.column != null)) || (this.node.NodeType != XmlNodeType.Element))
                {
                    return 0;
                }
                if (!IsFoliated(this.node))
                {
                    return this.ColumnCount(this.Row, true, false);
                }
                return this.node.Attributes.Count;
            }
        }

        internal XmlDeclaration Declaration
        {
            get
            {
                XmlNode node = this.doc.SafeFirstChild(this.doc);
                if ((node != null) && (node.NodeType == XmlNodeType.XmlDeclaration))
                {
                    return (XmlDeclaration) node;
                }
                return null;
            }
        }

        internal string Encoding
        {
            get
            {
                if (this.NodeType == XmlNodeType.XmlDeclaration)
                {
                    return ((XmlDeclaration) this.node).Encoding;
                }
                if (this.NodeType == XmlNodeType.Document)
                {
                    XmlDeclaration declaration = this.Declaration;
                    if (declaration != null)
                    {
                        return declaration.Encoding;
                    }
                }
                return null;
            }
        }

        internal string InternalSubset
        {
            get
            {
                if (this.NodeType == XmlNodeType.DocumentType)
                {
                    return ((XmlDocumentType) this.node).InternalSubset;
                }
                return null;
            }
        }

        internal bool IsDefault
        {
            get
            {
                this.RealFoliate();
                return ((((this.node != null) && (this.column == null)) && (this.node.NodeType == XmlNodeType.Attribute)) && !((XmlAttribute) this.node).Specified);
            }
        }

        internal bool IsEmptyElement
        {
            get
            {
                this.RealFoliate();
                return ((((this.node != null) && (this.column == null)) && (this.node.NodeType == XmlNodeType.Element)) && ((XmlElement) this.node).IsEmpty);
            }
        }

        internal string LocalName
        {
            get
            {
                this.RealFoliate();
                if (this.node == null)
                {
                    return string.Empty;
                }
                if (this.column == null)
                {
                    string localName = this.node.LocalName;
                    if (this.IsLocalNameEmpty(this.node.NodeType))
                    {
                        return string.Empty;
                    }
                    return localName;
                }
                if (this.fOnValue)
                {
                    return string.Empty;
                }
                return this.doc.NameTable.Add(this.column.EncodedColumnName);
            }
        }

        internal string Name
        {
            get
            {
                this.RealFoliate();
                if (this.node == null)
                {
                    return string.Empty;
                }
                if (this.column == null)
                {
                    string name = this.node.Name;
                    if (this.IsLocalNameEmpty(this.node.NodeType))
                    {
                        return string.Empty;
                    }
                    return name;
                }
                string prefix = this.Prefix;
                string localName = this.LocalName;
                if ((prefix == null) || (prefix.Length <= 0))
                {
                    return localName;
                }
                if ((localName != null) && (localName.Length > 0))
                {
                    return this.doc.NameTable.Add(prefix + ":" + localName);
                }
                return prefix;
            }
        }

        internal string NamespaceURI
        {
            get
            {
                this.RealFoliate();
                if (this.node == null)
                {
                    return string.Empty;
                }
                if (this.column == null)
                {
                    return this.node.NamespaceURI;
                }
                if (this.fOnValue)
                {
                    return string.Empty;
                }
                return this.doc.NameTable.Add(this.column.Namespace);
            }
        }

        internal XmlNodeType NodeType
        {
            get
            {
                this.RealFoliate();
                if (this.node == null)
                {
                    return XmlNodeType.None;
                }
                if (this.column == null)
                {
                    return this.node.NodeType;
                }
                if (this.fOnValue)
                {
                    return XmlNodeType.Text;
                }
                if (this.column.ColumnMapping == MappingType.Attribute)
                {
                    return XmlNodeType.Attribute;
                }
                return XmlNodeType.Element;
            }
        }

        internal string Prefix
        {
            get
            {
                this.RealFoliate();
                if ((this.node != null) && (this.column == null))
                {
                    return this.node.Prefix;
                }
                return string.Empty;
            }
        }

        internal string PublicId
        {
            get
            {
                switch (this.NodeType)
                {
                    case XmlNodeType.DocumentType:
                        return ((XmlDocumentType) this.node).PublicId;

                    case XmlNodeType.Notation:
                        return ((XmlNotation) this.node).PublicId;

                    case XmlNodeType.Entity:
                        return ((XmlEntity) this.node).PublicId;
                }
                return null;
            }
        }

        private DataRow Row
        {
            get
            {
                XmlBoundElement rowElement = this.GetRowElement();
                if (rowElement == null)
                {
                    return null;
                }
                return rowElement.Row;
            }
        }

        internal string Standalone
        {
            get
            {
                if (this.NodeType == XmlNodeType.XmlDeclaration)
                {
                    return ((XmlDeclaration) this.node).Standalone;
                }
                if (this.NodeType == XmlNodeType.Document)
                {
                    XmlDeclaration declaration = this.Declaration;
                    if (declaration != null)
                    {
                        return declaration.Standalone;
                    }
                }
                return null;
            }
        }

        internal string SystemId
        {
            get
            {
                switch (this.NodeType)
                {
                    case XmlNodeType.DocumentType:
                        return ((XmlDocumentType) this.node).SystemId;

                    case XmlNodeType.Notation:
                        return ((XmlNotation) this.node).SystemId;

                    case XmlNodeType.Entity:
                        return ((XmlEntity) this.node).SystemId;
                }
                return null;
            }
        }

        internal string Value
        {
            get
            {
                this.RealFoliate();
                if (this.node != null)
                {
                    if (this.column == null)
                    {
                        return this.node.Value;
                    }
                    if ((this.column.ColumnMapping != MappingType.Attribute) && !this.fOnValue)
                    {
                        return null;
                    }
                    DataRow row = this.Row;
                    DataRowVersion version = (row.RowState == DataRowState.Detached) ? DataRowVersion.Proposed : DataRowVersion.Current;
                    object obj2 = row[this.column, version];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return this.column.ConvertObjectToXml(obj2);
                    }
                }
                return null;
            }
        }

        internal string Version
        {
            get
            {
                if (this.NodeType == XmlNodeType.XmlDeclaration)
                {
                    return ((XmlDeclaration) this.node).Version;
                }
                if (this.NodeType == XmlNodeType.Document)
                {
                    XmlDeclaration declaration = this.Declaration;
                    if (declaration != null)
                    {
                        return declaration.Version;
                    }
                }
                return null;
            }
        }
    }
}

