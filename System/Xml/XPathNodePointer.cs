namespace System.Xml
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.Xml.XPath;

    internal sealed class XPathNodePointer : IXmlDataVirtualNode
    {
        private bool _bNeedFoliate;
        private DataColumn _column;
        private XmlDataDocument _doc;
        private bool _fOnValue;
        private XmlNode _node;
        private WeakReference _owner;
        internal XmlBoundElement _parentOfNS;
        internal static string s_strReservedXml = "http://www.w3.org/XML/1998/namespace";
        internal static string s_strReservedXmlns = "http://www.w3.org/2000/xmlns/";
        internal static string s_strXmlNS = "xmlns";
        internal static int[] xmlNodeType_To_XpathNodeType_Map = new int[20];

        static XPathNodePointer()
        {
            xmlNodeType_To_XpathNodeType_Map[0] = -1;
            xmlNodeType_To_XpathNodeType_Map[1] = 1;
            xmlNodeType_To_XpathNodeType_Map[2] = 2;
            xmlNodeType_To_XpathNodeType_Map[3] = 4;
            xmlNodeType_To_XpathNodeType_Map[4] = 4;
            xmlNodeType_To_XpathNodeType_Map[5] = -1;
            xmlNodeType_To_XpathNodeType_Map[6] = -1;
            xmlNodeType_To_XpathNodeType_Map[7] = 7;
            xmlNodeType_To_XpathNodeType_Map[8] = 8;
            xmlNodeType_To_XpathNodeType_Map[9] = 0;
            xmlNodeType_To_XpathNodeType_Map[10] = -1;
            xmlNodeType_To_XpathNodeType_Map[11] = 0;
            xmlNodeType_To_XpathNodeType_Map[12] = -1;
            xmlNodeType_To_XpathNodeType_Map[13] = 6;
            xmlNodeType_To_XpathNodeType_Map[14] = 5;
            xmlNodeType_To_XpathNodeType_Map[15] = -1;
            xmlNodeType_To_XpathNodeType_Map[0x10] = -1;
            xmlNodeType_To_XpathNodeType_Map[0x11] = -1;
        }

        internal XPathNodePointer(DataDocumentXPathNavigator owner, XPathNodePointer pointer) : this(owner, pointer._doc, pointer._node, pointer._column, pointer._fOnValue, pointer._parentOfNS)
        {
        }

        internal XPathNodePointer(DataDocumentXPathNavigator owner, XmlDataDocument doc, XmlNode node) : this(owner, doc, node, null, false, null)
        {
        }

        private XPathNodePointer(DataDocumentXPathNavigator owner, XmlDataDocument doc, XmlNode node, DataColumn c, bool bOnValue, XmlBoundElement parentOfNS)
        {
            this._owner = new WeakReference(owner);
            this._doc = doc;
            this._node = node;
            this._column = c;
            this._fOnValue = bOnValue;
            this._parentOfNS = parentOfNS;
            this._doc.AddPointer(this);
            this._bNeedFoliate = false;
        }

        [Conditional("DEBUG")]
        private void AssertValid()
        {
            if (this._column != null)
            {
                XmlBoundElement element = this._node as XmlBoundElement;
                DataRowState rowState = element.Row.RowState;
            }
            DataColumn column1 = this._column;
        }

        internal XPathNodePointer Clone(DataDocumentXPathNavigator owner)
        {
            this.RealFoliate();
            return new XPathNodePointer(owner, this);
        }

        private int ColumnCount(DataRow row, bool fAttribute)
        {
            DataColumn col = null;
            int num = 0;
            while ((col = this.NextColumn(row, col, fAttribute)) != null)
            {
                if (col.Namespace != s_strReservedXmlns)
                {
                    num++;
                }
            }
            return num;
        }

        private XmlNodeOrder CompareNamespacePosition(XPathNodePointer other)
        {
            XPathNodePointer pointer = this.Clone((DataDocumentXPathNavigator) this._owner.Target);
            other.Clone((DataDocumentXPathNavigator) other._owner.Target);
            while (pointer.MoveToNextNamespace(XPathNamespaceScope.All))
            {
                if (pointer.IsSamePosition(other))
                {
                    return XmlNodeOrder.Before;
                }
            }
            return XmlNodeOrder.After;
        }

        internal XmlNodeOrder ComparePosition(XPathNodePointer other)
        {
            this.RealFoliate();
            other.RealFoliate();
            if (this.IsSamePosition(other))
            {
                return XmlNodeOrder.Same;
            }
            XmlNode node = null;
            XmlNode node2 = null;
            if ((this.NodeType == XPathNodeType.Namespace) && (other.NodeType == XPathNodeType.Namespace))
            {
                if (this._parentOfNS == other._parentOfNS)
                {
                    return this.CompareNamespacePosition(other);
                }
                node = this._parentOfNS;
                node2 = other._parentOfNS;
            }
            else if (this.NodeType == XPathNodeType.Namespace)
            {
                if (this._parentOfNS == other._node)
                {
                    if (other._column == null)
                    {
                        return XmlNodeOrder.After;
                    }
                    return XmlNodeOrder.Before;
                }
                node = this._parentOfNS;
                node2 = other._node;
            }
            else if (other.NodeType == XPathNodeType.Namespace)
            {
                if (this._node == other._parentOfNS)
                {
                    if (this._column == null)
                    {
                        return XmlNodeOrder.Before;
                    }
                    return XmlNodeOrder.After;
                }
                node = this._node;
                node2 = other._parentOfNS;
            }
            else
            {
                if (this._node == other._node)
                {
                    if (this._column == other._column)
                    {
                        if (this._fOnValue)
                        {
                            return XmlNodeOrder.After;
                        }
                        return XmlNodeOrder.Before;
                    }
                    if (this._column == null)
                    {
                        return XmlNodeOrder.Before;
                    }
                    if ((other._column != null) && (this._column.Ordinal < other._column.Ordinal))
                    {
                        return XmlNodeOrder.Before;
                    }
                    return XmlNodeOrder.After;
                }
                node = this._node;
                node2 = other._node;
            }
            if ((node != null) && (node2 != null))
            {
                int depth = -1;
                int num = -1;
                XmlNode root = GetRoot(node, ref depth);
                XmlNode node6 = GetRoot(node2, ref num);
                if (root != node6)
                {
                    return XmlNodeOrder.Unknown;
                }
                if (depth > num)
                {
                    while ((node != null) && (depth > num))
                    {
                        node = (node.NodeType == XmlNodeType.Attribute) ? ((XmlAttribute) node).OwnerElement : node.ParentNode;
                        depth--;
                    }
                    if (node == node2)
                    {
                        return XmlNodeOrder.After;
                    }
                }
                else if (num > depth)
                {
                    while ((node2 != null) && (num > depth))
                    {
                        node2 = (node2.NodeType == XmlNodeType.Attribute) ? ((XmlAttribute) node2).OwnerElement : node2.ParentNode;
                        num--;
                    }
                    if (node == node2)
                    {
                        return XmlNodeOrder.Before;
                    }
                }
                XmlNode parent = this.GetParent(node);
                XmlNode parentNode = this.GetParent(node2);
                XmlNode nextSibling = null;
                while ((parent != null) && (parentNode != null))
                {
                    if (parent == parentNode)
                    {
                        while (node != null)
                        {
                            nextSibling = node.NextSibling;
                            if (nextSibling == node2)
                            {
                                return XmlNodeOrder.Before;
                            }
                            node = nextSibling;
                        }
                        return XmlNodeOrder.After;
                    }
                    node = parent;
                    node2 = parentNode;
                    parent = node.ParentNode;
                    parentNode = node2.ParentNode;
                }
            }
            return XmlNodeOrder.Unknown;
        }

        private XPathNodeType ConvertNodeType(XmlNode node)
        {
            int num = -1;
            if (XmlDataDocument.IsTextNode(node.NodeType))
            {
                return this.DecideXPNodeTypeForTextNodes(node);
            }
            num = xmlNodeType_To_XpathNodeType_Map[(int) node.NodeType];
            if (num != 2)
            {
                return (XPathNodeType) num;
            }
            if (node.NamespaceURI == s_strReservedXmlns)
            {
                return XPathNodeType.Namespace;
            }
            return XPathNodeType.Attribute;
        }

        private XPathNodeType DecideXPNodeTypeForTextNodes(XmlNode node)
        {
            XPathNodeType whitespace = XPathNodeType.Whitespace;
            while (node != null)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        return XPathNodeType.Text;

                    case XmlNodeType.Whitespace:
                        break;

                    case XmlNodeType.SignificantWhitespace:
                        whitespace = XPathNodeType.SignificantWhitespace;
                        break;

                    default:
                        return whitespace;
                }
                node = this._doc.SafeNextSibling(node);
            }
            return whitespace;
        }

        private bool DuplicateNS(XmlBoundElement endElem, string lname)
        {
            if ((this._parentOfNS != null) && (endElem != null))
            {
                XmlBoundElement be = this._parentOfNS;
                XmlNode parentNode = null;
                while ((be != null) && (be != endElem))
                {
                    if (this.GetNamespace(be, lname) != null)
                    {
                        return true;
                    }
                    parentNode = be;
                    do
                    {
                        parentNode = parentNode.ParentNode;
                    }
                    while ((parentNode != null) && (parentNode.NodeType != XmlNodeType.Element));
                    be = parentNode as XmlBoundElement;
                }
            }
            return false;
        }

        internal string GetNamespace(string name)
        {
            if (name == "xml")
            {
                return s_strReservedXml;
            }
            if (name == "xmlns")
            {
                return s_strReservedXmlns;
            }
            if ((name != null) && (name.Length == 0))
            {
                name = "xmlns";
            }
            this.RealFoliate();
            XmlNode ownerElement = this._node;
            XmlNodeType nodeType = ownerElement.NodeType;
            string str = null;
            while (ownerElement != null)
            {
                while ((ownerElement != null) && ((nodeType = ownerElement.NodeType) != XmlNodeType.Element))
                {
                    if (nodeType == XmlNodeType.Attribute)
                    {
                        ownerElement = ((XmlAttribute) ownerElement).OwnerElement;
                    }
                    else
                    {
                        ownerElement = ownerElement.ParentNode;
                    }
                }
                if (ownerElement != null)
                {
                    str = this.GetNamespace((XmlBoundElement) ownerElement, name);
                    if (str != null)
                    {
                        return str;
                    }
                    ownerElement = ownerElement.ParentNode;
                }
            }
            return string.Empty;
        }

        private string GetNamespace(XmlBoundElement be, string name)
        {
            if (be != null)
            {
                XmlAttribute attributeNode = null;
                if (be.IsFoliated)
                {
                    attributeNode = be.GetAttributeNode(name, s_strReservedXmlns);
                    if (attributeNode != null)
                    {
                        return attributeNode.Value;
                    }
                    return null;
                }
                DataRow row = be.Row;
                if (row != null)
                {
                    for (DataColumn column = this.PreviousColumn(row, null, true); column != null; column = this.PreviousColumn(row, column, true))
                    {
                        if (column.Namespace == s_strReservedXmlns)
                        {
                            DataRowVersion version = (row.RowState == DataRowState.Detached) ? DataRowVersion.Proposed : DataRowVersion.Current;
                            return column.ConvertObjectToXml(row[column, version]);
                        }
                    }
                }
            }
            return null;
        }

        private XmlNode GetParent(XmlNode node)
        {
            switch (this.ConvertNodeType(node))
            {
                case XPathNodeType.Namespace:
                    return this._parentOfNS;

                case XPathNodeType.Attribute:
                    return ((XmlAttribute) node).OwnerElement;
            }
            return node.ParentNode;
        }

        private static XmlNode GetRoot(XmlNode node, ref int depth)
        {
            depth = 0;
            XmlNode node2 = node;
            XmlNode parentNode = (node2.NodeType == XmlNodeType.Attribute) ? ((XmlAttribute) node2).OwnerElement : node2.ParentNode;
            while (parentNode != null)
            {
                node2 = parentNode;
                parentNode = node2.ParentNode;
                depth++;
            }
            return node2;
        }

        private XmlBoundElement GetRowElement()
        {
            XmlBoundElement element;
            if (this._column != null)
            {
                return (this._node as XmlBoundElement);
            }
            this._doc.Mapper.GetRegion(this._node, out element);
            return element;
        }

        private bool IsFoliated(XmlNode node)
        {
            if ((node != null) && (node is XmlBoundElement))
            {
                return ((XmlBoundElement) node).IsFoliated;
            }
            return true;
        }

        private bool IsNamespaceNode(XmlNodeType nt, string ns)
        {
            return ((nt == XmlNodeType.Attribute) && (ns == s_strReservedXmlns));
        }

        internal bool IsSamePosition(XPathNodePointer pointer)
        {
            this.RealFoliate();
            pointer.RealFoliate();
            if ((this._column == null) && (pointer._column == null))
            {
                return ((pointer._node == this._node) && (pointer._parentOfNS == this._parentOfNS));
            }
            return ((((pointer._doc == this._doc) && (pointer._node == this._node)) && ((pointer._column == this._column) && (pointer._fOnValue == this._fOnValue))) && (pointer._parentOfNS == this._parentOfNS));
        }

        private bool IsValidChild(XmlNode parent, DataColumn c)
        {
            switch (xmlNodeType_To_XpathNodeType_Map[(int) parent.NodeType])
            {
                case 0:
                    return (c.ColumnMapping == MappingType.Element);

                case 1:
                    return ((c.ColumnMapping == MappingType.Element) || (c.ColumnMapping == MappingType.SimpleContent));
            }
            return false;
        }

        private bool IsValidChild(XmlNode parent, XmlNode child)
        {
            int num = xmlNodeType_To_XpathNodeType_Map[(int) child.NodeType];
            if (num != -1)
            {
                switch (xmlNodeType_To_XpathNodeType_Map[(int) parent.NodeType])
                {
                    case 0:
                        switch (num)
                        {
                            case 1:
                            case 8:
                                return true;
                        }
                        return (num == 7);

                    case 1:
                        switch (num)
                        {
                            case 1:
                            case 4:
                            case 8:
                            case 6:
                            case 5:
                                return true;
                        }
                        return (num == 7);
                }
            }
            return false;
        }

        private void MoveTo(XmlNode node)
        {
            this._node = node;
            this._column = null;
            this._fOnValue = false;
        }

        internal bool MoveTo(XPathNodePointer pointer)
        {
            if (this._doc != pointer._doc)
            {
                return false;
            }
            this._node = pointer._node;
            this._column = pointer._column;
            this._fOnValue = pointer._fOnValue;
            this._bNeedFoliate = pointer._bNeedFoliate;
            return true;
        }

        private void MoveTo(XmlNode node, DataColumn column, bool _fOnValue)
        {
            this._node = node;
            this._column = column;
            this._fOnValue = _fOnValue;
        }

        internal bool MoveToAttribute(string localName, string namespaceURI)
        {
            this.RealFoliate();
            if ((namespaceURI != s_strReservedXmlns) && (((this._node != null) && ((this._column == null) || (this._column.ColumnMapping == MappingType.Attribute))) && (this._node.NodeType == XmlNodeType.Element)))
            {
                if (!this.IsFoliated(this._node))
                {
                    DataColumn col = null;
                    while ((col = this.NextColumn(this.Row, col, true)) != null)
                    {
                        if ((col.EncodedColumnName == localName) && (col.Namespace == namespaceURI))
                        {
                            this.MoveTo(this._node, col, false);
                            return true;
                        }
                    }
                }
                else
                {
                    XmlNode namedItem = this._node.Attributes.GetNamedItem(localName, namespaceURI);
                    if (namedItem != null)
                    {
                        this.MoveTo(namedItem, null, false);
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool MoveToFirst()
        {
            this.RealFoliate();
            if (this._node != null)
            {
                DataRow row = null;
                XmlNode parentNode = null;
                if (this._column != null)
                {
                    row = this.Row;
                    parentNode = this._node;
                }
                else
                {
                    parentNode = this._node.ParentNode;
                    if (parentNode == null)
                    {
                        return false;
                    }
                    if (!this.IsFoliated(parentNode) && (parentNode is XmlBoundElement))
                    {
                        row = ((XmlBoundElement) parentNode).Row;
                    }
                }
                if (row != null)
                {
                    for (DataColumn column = this.NextColumn(row, null, false); column != null; column = this.NextColumn(row, column, false))
                    {
                        if (this.IsValidChild(this._node, column))
                        {
                            this.MoveTo(this._node, column, this._doc.IsTextOnly(column));
                            return true;
                        }
                    }
                }
                for (XmlNode node2 = this._doc.SafeFirstChild(parentNode); node2 != null; node2 = this._doc.SafeNextSibling(node2))
                {
                    if (this.IsValidChild(parentNode, node2))
                    {
                        this.MoveTo(node2);
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool MoveToFirstChild()
        {
            this.RealFoliate();
            if (this._node != null)
            {
                if (this._column != null)
                {
                    if ((this._column.ColumnMapping == MappingType.Attribute) || (this._column.ColumnMapping == MappingType.Hidden))
                    {
                        return false;
                    }
                    if (this._fOnValue)
                    {
                        return false;
                    }
                    this._fOnValue = true;
                    return true;
                }
                if (!this.IsFoliated(this._node))
                {
                    DataRow row = this.Row;
                    for (DataColumn column = this.NextColumn(row, null, false); column != null; column = this.NextColumn(row, column, false))
                    {
                        if (this.IsValidChild(this._node, column))
                        {
                            this.MoveTo(this._node, column, this._doc.IsTextOnly(column));
                            return true;
                        }
                    }
                }
                for (XmlNode node = this._doc.SafeFirstChild(this._node); node != null; node = this._doc.SafeNextSibling(node))
                {
                    if (this.IsValidChild(this._node, node))
                    {
                        this.MoveTo(node);
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            this.RealFoliate();
            this._parentOfNS = this._node as XmlBoundElement;
            if (this._parentOfNS == null)
            {
                return false;
            }
            XmlNode parentNode = this._node;
            XmlBoundElement be = null;
            while (parentNode != null)
            {
                be = parentNode as XmlBoundElement;
                if (this.MoveToNextNamespace(be, null, null))
                {
                    return true;
                }
                if (namespaceScope == XPathNamespaceScope.Local)
                {
                    goto Label_0072;
                }
                do
                {
                    parentNode = parentNode.ParentNode;
                }
                while ((parentNode != null) && (parentNode.NodeType != XmlNodeType.Element));
            }
            if (namespaceScope == XPathNamespaceScope.All)
            {
                this.MoveTo(this._doc.attrXml, null, false);
                return true;
            }
        Label_0072:
            this._parentOfNS = null;
            return false;
        }

        internal bool MoveToNamespace(string name)
        {
            this._parentOfNS = this._node as XmlBoundElement;
            if (this._parentOfNS != null)
            {
                string str = name;
                if (str == "xmlns")
                {
                    str = "xmlns:xmlns";
                }
                if ((str != null) && (str.Length == 0))
                {
                    str = "xmlns";
                }
                this.RealFoliate();
                XmlNode parentNode = this._node;
                XmlNodeType nodeType = parentNode.NodeType;
                XmlAttribute attributeNode = null;
                XmlBoundElement element = null;
                while (parentNode != null)
                {
                    element = parentNode as XmlBoundElement;
                    if (element != null)
                    {
                        if (element.IsFoliated)
                        {
                            attributeNode = element.GetAttributeNode(name, s_strReservedXmlns);
                            if (attributeNode != null)
                            {
                                this.MoveTo(attributeNode);
                                return true;
                            }
                        }
                        else
                        {
                            DataRow row = element.Row;
                            if (row == null)
                            {
                                return false;
                            }
                            for (DataColumn column = this.PreviousColumn(row, null, true); column != null; column = this.PreviousColumn(row, column, true))
                            {
                                if ((column.Namespace == s_strReservedXmlns) && (column.ColumnName == name))
                                {
                                    this.MoveTo(element, column, false);
                                    return true;
                                }
                            }
                        }
                    }
                    do
                    {
                        parentNode = parentNode.ParentNode;
                    }
                    while ((parentNode != null) && (parentNode.NodeType != XmlNodeType.Element));
                }
                this._parentOfNS = null;
            }
            return false;
        }

        internal bool MoveToNextAttribute(bool bFirst)
        {
            this.RealFoliate();
            if (this._node != null)
            {
                if (bFirst && ((this._column != null) || (this._node.NodeType != XmlNodeType.Element)))
                {
                    return false;
                }
                if (!bFirst)
                {
                    if ((this._column != null) && (this._column.ColumnMapping != MappingType.Attribute))
                    {
                        return false;
                    }
                    if ((this._column == null) && (this._node.NodeType != XmlNodeType.Attribute))
                    {
                        return false;
                    }
                }
                if (!this.IsFoliated(this._node))
                {
                    DataColumn col = this._column;
                    while ((col = this.NextColumn(this.Row, col, true)) != null)
                    {
                        if (col.Namespace != s_strReservedXmlns)
                        {
                            this.MoveTo(this._node, col, false);
                            return true;
                        }
                    }
                    return false;
                }
                if (bFirst)
                {
                    foreach (XmlAttribute attribute2 in this._node.Attributes)
                    {
                        if (attribute2.NamespaceURI != s_strReservedXmlns)
                        {
                            this.MoveTo(attribute2, null, false);
                            return true;
                        }
                    }
                }
                else
                {
                    XmlAttributeCollection attributes = ((XmlAttribute) this._node).OwnerElement.Attributes;
                    bool flag = false;
                    foreach (XmlAttribute attribute in attributes)
                    {
                        if (flag && (attribute.NamespaceURI != s_strReservedXmlns))
                        {
                            this.MoveTo(attribute, null, false);
                            return true;
                        }
                        if (attribute == this._node)
                        {
                            flag = true;
                        }
                    }
                }
            }
            return false;
        }

        internal bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            this.RealFoliate();
            XmlNode parentNode = this._node;
            if (this._column != null)
            {
                if ((namespaceScope == XPathNamespaceScope.Local) && (this._parentOfNS != this._node))
                {
                    return false;
                }
                XmlBoundElement element = this._node as XmlBoundElement;
                DataRow row = element.Row;
                for (DataColumn column = this.PreviousColumn(row, this._column, true); column != null; column = this.PreviousColumn(row, column, true))
                {
                    if (column.Namespace == s_strReservedXmlns)
                    {
                        this.MoveTo(element, column, false);
                        return true;
                    }
                }
                if (namespaceScope == XPathNamespaceScope.Local)
                {
                    return false;
                }
                do
                {
                    parentNode = parentNode.ParentNode;
                }
                while ((parentNode != null) && (parentNode.NodeType != XmlNodeType.Element));
            }
            else if (this._node.NodeType == XmlNodeType.Attribute)
            {
                XmlAttribute curAttr = (XmlAttribute) this._node;
                parentNode = curAttr.OwnerElement;
                if (parentNode == null)
                {
                    return false;
                }
                if ((namespaceScope == XPathNamespaceScope.Local) && (this._parentOfNS != parentNode))
                {
                    return false;
                }
                if (this.MoveToNextNamespace((XmlBoundElement) parentNode, null, curAttr))
                {
                    return true;
                }
                if (namespaceScope == XPathNamespaceScope.Local)
                {
                    return false;
                }
                do
                {
                    parentNode = parentNode.ParentNode;
                }
                while ((parentNode != null) && (parentNode.NodeType != XmlNodeType.Element));
            }
            while (parentNode != null)
            {
                XmlBoundElement be = parentNode as XmlBoundElement;
                if (this.MoveToNextNamespace(be, null, null))
                {
                    return true;
                }
                do
                {
                    parentNode = parentNode.ParentNode;
                }
                while ((parentNode != null) && (parentNode.NodeType == XmlNodeType.Element));
            }
            if (namespaceScope == XPathNamespaceScope.All)
            {
                this.MoveTo(this._doc.attrXml, null, false);
                return true;
            }
            return false;
        }

        private bool MoveToNextNamespace(XmlBoundElement be, DataColumn col, XmlAttribute curAttr)
        {
            if (be != null)
            {
                if (be.IsFoliated)
                {
                    XmlAttributeCollection attributes = be.Attributes;
                    XmlAttribute node = null;
                    bool flag = false;
                    if (curAttr == null)
                    {
                        flag = true;
                    }
                    int count = attributes.Count;
                    while (count > 0)
                    {
                        count--;
                        node = attributes[count];
                        if ((flag && (node.NamespaceURI == s_strReservedXmlns)) && !this.DuplicateNS(be, node.LocalName))
                        {
                            this.MoveTo(node);
                            return true;
                        }
                        if (node == curAttr)
                        {
                            flag = true;
                        }
                    }
                }
                else
                {
                    DataRow row = be.Row;
                    if (row == null)
                    {
                        return false;
                    }
                    for (DataColumn column = this.PreviousColumn(row, col, true); column != null; column = this.PreviousColumn(row, column, true))
                    {
                        if ((column.Namespace == s_strReservedXmlns) && !this.DuplicateNS(be, column.ColumnName))
                        {
                            this.MoveTo(be, column, false);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal bool MoveToNextSibling()
        {
            this.RealFoliate();
            if (this._node != null)
            {
                if (this._column != null)
                {
                    if (this._fOnValue)
                    {
                        return false;
                    }
                    DataRow row = this.Row;
                    for (DataColumn column = this.NextColumn(row, this._column, false); column != null; column = this.NextColumn(row, column, false))
                    {
                        if (this.IsValidChild(this._node, column))
                        {
                            this.MoveTo(this._node, column, this._doc.IsTextOnly(column));
                            return true;
                        }
                    }
                    XmlNode node3 = this._doc.SafeFirstChild(this._node);
                    if (node3 != null)
                    {
                        this.MoveTo(node3);
                        return true;
                    }
                }
                else
                {
                    XmlNode child = this._node;
                    XmlNode parentNode = this._node.ParentNode;
                    if (parentNode == null)
                    {
                        return false;
                    }
                    bool flag = XmlDataDocument.IsTextNode(this._node.NodeType);
                    do
                    {
                        child = this._doc.SafeNextSibling(child);
                    }
                    while ((((child != null) && flag) && XmlDataDocument.IsTextNode(child.NodeType)) || ((child != null) && !this.IsValidChild(parentNode, child)));
                    if (child != null)
                    {
                        this.MoveTo(child);
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool MoveToParent()
        {
            this.RealFoliate();
            if (this.NodeType == XPathNodeType.Namespace)
            {
                this.MoveTo(this._parentOfNS);
                return true;
            }
            if (this._node != null)
            {
                if (this._column != null)
                {
                    if (this._fOnValue && !this._doc.IsTextOnly(this._column))
                    {
                        this.MoveTo(this._node, this._column, false);
                        return true;
                    }
                    this.MoveTo(this._node, null, false);
                    return true;
                }
                XmlNode ownerElement = null;
                if (this._node.NodeType == XmlNodeType.Attribute)
                {
                    ownerElement = ((XmlAttribute) this._node).OwnerElement;
                }
                else
                {
                    ownerElement = this._node.ParentNode;
                }
                if (ownerElement != null)
                {
                    this.MoveTo(ownerElement);
                    return true;
                }
            }
            return false;
        }

        internal bool MoveToPreviousSibling()
        {
            this.RealFoliate();
            if (this._node != null)
            {
                if (this._column != null)
                {
                    if (this._fOnValue)
                    {
                        return false;
                    }
                    DataRow row2 = this.Row;
                    for (DataColumn column = this.PreviousColumn(row2, this._column, false); column != null; column = this.PreviousColumn(row2, column, false))
                    {
                        if (this.IsValidChild(this._node, column))
                        {
                            this.MoveTo(this._node, column, this._doc.IsTextOnly(column));
                            return true;
                        }
                    }
                }
                else
                {
                    XmlNode child = this._node;
                    XmlNode parentNode = this._node.ParentNode;
                    if (parentNode == null)
                    {
                        return false;
                    }
                    bool flag = XmlDataDocument.IsTextNode(this._node.NodeType);
                    do
                    {
                        child = this._doc.SafePreviousSibling(child);
                    }
                    while ((((child != null) && flag) && XmlDataDocument.IsTextNode(child.NodeType)) || ((child != null) && !this.IsValidChild(parentNode, child)));
                    if (child != null)
                    {
                        this.MoveTo(child);
                        return true;
                    }
                    if (!this.IsFoliated(parentNode) && (parentNode is XmlBoundElement))
                    {
                        DataRow row = ((XmlBoundElement) parentNode).Row;
                        if (row != null)
                        {
                            DataColumn column2 = this.PreviousColumn(row, null, false);
                            if (column2 != null)
                            {
                                this.MoveTo(parentNode, column2, this._doc.IsTextOnly(column2));
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        internal void MoveToRoot()
        {
            XmlNode node2 = this._node;
            for (XmlNode node = this._node; node != null; node = this.GetParent(node))
            {
                node2 = node;
            }
            this._node = node2;
            this._column = null;
            this._fOnValue = false;
        }

        internal DataColumn NextColumn(DataRow row, DataColumn col, bool fAttribute)
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
                    if ((!this._doc.IsNotMapped(c) && ((c.ColumnMapping == MappingType.Attribute) == fAttribute)) && !Convert.IsDBNull(row[c, version]))
                    {
                        return c;
                    }
                    num++;
                }
            }
            return null;
        }

        internal DataColumn PreviousColumn(DataRow row, DataColumn col, bool fAttribute)
        {
            if (row.RowState != DataRowState.Deleted)
            {
                DataColumnCollection columns = row.Table.Columns;
                int num = (col != null) ? (col.Ordinal - 1) : (columns.Count - 1);
                int count = columns.Count;
                DataRowVersion version = (row.RowState == DataRowState.Detached) ? DataRowVersion.Proposed : DataRowVersion.Current;
                while (num >= 0)
                {
                    DataColumn c = columns[num];
                    if ((!this._doc.IsNotMapped(c) && ((c.ColumnMapping == MappingType.Attribute) == fAttribute)) && !Convert.IsDBNull(row[c, version]))
                    {
                        return c;
                    }
                    num--;
                }
            }
            return null;
        }

        private void RealFoliate()
        {
            if (this._bNeedFoliate)
            {
                this._bNeedFoliate = false;
                XmlNode firstChild = null;
                if (this._doc.IsTextOnly(this._column))
                {
                    firstChild = this._node.FirstChild;
                }
                else
                {
                    if (this._column.ColumnMapping == MappingType.Attribute)
                    {
                        firstChild = this._node.Attributes.GetNamedItem(this._column.EncodedColumnName, this._column.Namespace);
                    }
                    else
                    {
                        firstChild = this._node.FirstChild;
                        while (firstChild != null)
                        {
                            if ((firstChild.LocalName == this._column.EncodedColumnName) && (firstChild.NamespaceURI == this._column.Namespace))
                            {
                                break;
                            }
                            firstChild = firstChild.NextSibling;
                        }
                    }
                    if ((firstChild != null) && this._fOnValue)
                    {
                        firstChild = firstChild.FirstChild;
                    }
                }
                if (firstChild == null)
                {
                    throw new InvalidOperationException(System.Data.Res.GetString("DataDom_Foliation"));
                }
                this._node = firstChild;
                this._column = null;
                this._fOnValue = false;
                this._bNeedFoliate = false;
            }
        }

        bool IXmlDataVirtualNode.IsInUse()
        {
            return this._owner.IsAlive;
        }

        bool IXmlDataVirtualNode.IsOnColumn(DataColumn col)
        {
            this.RealFoliate();
            return (col == this._column);
        }

        bool IXmlDataVirtualNode.IsOnNode(XmlNode nodeToCheck)
        {
            this.RealFoliate();
            return (nodeToCheck == this._node);
        }

        void IXmlDataVirtualNode.OnFoliated(XmlNode foliatedNode)
        {
            if ((this._node == foliatedNode) && (this._column != null))
            {
                this._bNeedFoliate = true;
            }
        }

        internal int AttributeCount
        {
            get
            {
                this.RealFoliate();
                if (((this._node == null) || (this._column != null)) || (this._node.NodeType != XmlNodeType.Element))
                {
                    return 0;
                }
                if (!this.IsFoliated(this._node))
                {
                    return this.ColumnCount(this.Row, true);
                }
                int num = 0;
                foreach (XmlAttribute attribute in this._node.Attributes)
                {
                    if (attribute.NamespaceURI != s_strReservedXmlns)
                    {
                        num++;
                    }
                }
                return num;
            }
        }

        internal string BaseURI
        {
            get
            {
                this.RealFoliate();
                if (this._node != null)
                {
                    return this._node.BaseURI;
                }
                return string.Empty;
            }
        }

        internal XmlDataDocument Document
        {
            get
            {
                return this._doc;
            }
        }

        internal bool HasChildren
        {
            get
            {
                this.RealFoliate();
                if (this._node != null)
                {
                    if (this._column != null)
                    {
                        return (((this._column.ColumnMapping != MappingType.Attribute) && (this._column.ColumnMapping != MappingType.Hidden)) && !this._fOnValue);
                    }
                    if (!this.IsFoliated(this._node))
                    {
                        DataRow row = this.Row;
                        for (DataColumn column = this.NextColumn(row, null, false); column != null; column = this.NextColumn(row, column, false))
                        {
                            if (this.IsValidChild(this._node, column))
                            {
                                return true;
                            }
                        }
                    }
                    for (XmlNode node = this._doc.SafeFirstChild(this._node); node != null; node = this._doc.SafeNextSibling(node))
                    {
                        if (this.IsValidChild(this._node, node))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        internal string InnerText
        {
            get
            {
                this.RealFoliate();
                if (this._node != null)
                {
                    if (this._column == null)
                    {
                        if (this._node.NodeType != XmlNodeType.Document)
                        {
                            return this._node.InnerText;
                        }
                        XmlElement documentElement = ((XmlDocument) this._node).DocumentElement;
                        if (documentElement != null)
                        {
                            return documentElement.InnerText;
                        }
                        return string.Empty;
                    }
                    DataRow row = this.Row;
                    DataRowVersion version = (row.RowState == DataRowState.Detached) ? DataRowVersion.Proposed : DataRowVersion.Current;
                    object obj2 = row[this._column, version];
                    if (!Convert.IsDBNull(obj2))
                    {
                        return this._column.ConvertObjectToXml(obj2);
                    }
                }
                return string.Empty;
            }
        }

        internal bool IsEmptyElement
        {
            get
            {
                return ((((this._node != null) && (this._column == null)) && (this._node.NodeType == XmlNodeType.Element)) && ((XmlElement) this._node).IsEmpty);
            }
        }

        internal string LocalName
        {
            get
            {
                this.RealFoliate();
                if (this._node == null)
                {
                    return string.Empty;
                }
                if (this._column == null)
                {
                    XmlNodeType nodeType = this._node.NodeType;
                    if (this.IsNamespaceNode(nodeType, this._node.NamespaceURI) && (this._node.LocalName == s_strXmlNS))
                    {
                        return string.Empty;
                    }
                    if (((nodeType != XmlNodeType.Element) && (nodeType != XmlNodeType.Attribute)) && (nodeType != XmlNodeType.ProcessingInstruction))
                    {
                        return string.Empty;
                    }
                    return this._node.LocalName;
                }
                if (this._fOnValue)
                {
                    return string.Empty;
                }
                return this._doc.NameTable.Add(this._column.EncodedColumnName);
            }
        }

        internal string Name
        {
            get
            {
                this.RealFoliate();
                if (this._node == null)
                {
                    return string.Empty;
                }
                if (this._column == null)
                {
                    XmlNodeType nodeType = this._node.NodeType;
                    if (this.IsNamespaceNode(nodeType, this._node.NamespaceURI))
                    {
                        if (this._node.LocalName == s_strXmlNS)
                        {
                            return string.Empty;
                        }
                        return this._node.LocalName;
                    }
                    if (((nodeType != XmlNodeType.Element) && (nodeType != XmlNodeType.Attribute)) && (nodeType != XmlNodeType.ProcessingInstruction))
                    {
                        return string.Empty;
                    }
                    return this._node.Name;
                }
                if (this._fOnValue)
                {
                    return string.Empty;
                }
                return this._doc.NameTable.Add(this._column.EncodedColumnName);
            }
        }

        internal string NamespaceURI
        {
            get
            {
                this.RealFoliate();
                if (this._node == null)
                {
                    return string.Empty;
                }
                if (this._column == null)
                {
                    XPathNodeType type = this.ConvertNodeType(this._node);
                    if (((type != XPathNodeType.Element) && (type != XPathNodeType.Root)) && (type != XPathNodeType.Attribute))
                    {
                        return string.Empty;
                    }
                    return this._node.NamespaceURI;
                }
                if (this._fOnValue)
                {
                    return string.Empty;
                }
                if (this._column.Namespace == s_strReservedXmlns)
                {
                    return string.Empty;
                }
                return this._doc.NameTable.Add(this._column.Namespace);
            }
        }

        internal XmlNode Node
        {
            get
            {
                this.RealFoliate();
                if (this._node == null)
                {
                    return null;
                }
                XmlBoundElement rowElement = this.GetRowElement();
                if (rowElement != null)
                {
                    bool isFoliationEnabled = this._doc.IsFoliationEnabled;
                    this._doc.IsFoliationEnabled = true;
                    this._doc.Foliate(rowElement, ElementState.StrongFoliation);
                    this._doc.IsFoliationEnabled = isFoliationEnabled;
                }
                this.RealFoliate();
                return this._node;
            }
        }

        internal XPathNodeType NodeType
        {
            get
            {
                this.RealFoliate();
                if (this._node == null)
                {
                    return XPathNodeType.All;
                }
                if (this._column == null)
                {
                    return this.ConvertNodeType(this._node);
                }
                if (this._fOnValue)
                {
                    return XPathNodeType.Text;
                }
                if (this._column.ColumnMapping != MappingType.Attribute)
                {
                    return XPathNodeType.Element;
                }
                if (this._column.Namespace == s_strReservedXmlns)
                {
                    return XPathNodeType.Namespace;
                }
                return XPathNodeType.Attribute;
            }
        }

        internal string Prefix
        {
            get
            {
                this.RealFoliate();
                if (this._node == null)
                {
                    return string.Empty;
                }
                if (this._column != null)
                {
                    return string.Empty;
                }
                if (this.IsNamespaceNode(this._node.NodeType, this._node.NamespaceURI))
                {
                    return string.Empty;
                }
                return this._node.Prefix;
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

        internal string Value
        {
            get
            {
                this.RealFoliate();
                if (this._node != null)
                {
                    if (this._column == null)
                    {
                        string str = this._node.Value;
                        if (XmlDataDocument.IsTextNode(this._node.NodeType))
                        {
                            if (this._node.ParentNode == null)
                            {
                                return str;
                            }
                            for (XmlNode node = this._doc.SafeNextSibling(this._node); (node != null) && XmlDataDocument.IsTextNode(node.NodeType); node = this._doc.SafeNextSibling(node))
                            {
                                str = str + node.Value;
                            }
                        }
                        return str;
                    }
                    if ((this._column.ColumnMapping == MappingType.Attribute) || this._fOnValue)
                    {
                        DataRow row = this.Row;
                        DataRowVersion version = (row.RowState == DataRowState.Detached) ? DataRowVersion.Proposed : DataRowVersion.Current;
                        object obj2 = row[this._column, version];
                        if (!Convert.IsDBNull(obj2))
                        {
                            return this._column.ConvertObjectToXml(obj2);
                        }
                    }
                }
                return null;
            }
        }

        internal string XmlLang
        {
            get
            {
                this.RealFoliate();
                XmlNode ownerElement = this._node;
                XmlBoundElement element = null;
                object obj2 = null;
                while (ownerElement != null)
                {
                    element = ownerElement as XmlBoundElement;
                    if (element != null)
                    {
                        if (element.ElementState == ElementState.Defoliated)
                        {
                            DataRow row = element.Row;
                            foreach (DataColumn column in row.Table.Columns)
                            {
                                if ((column.Prefix == "xml") && (column.EncodedColumnName == "lang"))
                                {
                                    obj2 = row[column];
                                    if (obj2 == DBNull.Value)
                                    {
                                        break;
                                    }
                                    return (string) obj2;
                                }
                            }
                        }
                        else if (element.HasAttribute("xml:lang"))
                        {
                            return element.GetAttribute("xml:lang");
                        }
                    }
                    if (ownerElement.NodeType == XmlNodeType.Attribute)
                    {
                        ownerElement = ((XmlAttribute) ownerElement).OwnerElement;
                    }
                    else
                    {
                        ownerElement = ownerElement.ParentNode;
                    }
                }
                return string.Empty;
            }
        }
    }
}

