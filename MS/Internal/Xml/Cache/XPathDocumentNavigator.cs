namespace MS.Internal.Xml.Cache
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;

    internal sealed class XPathDocumentNavigator : XPathNavigator, IXmlLineInfo
    {
        private string atomizedLocalName;
        private int idxCurrent;
        private int idxParent;
        private XPathNode[] pageCurrent;
        private XPathNode[] pageParent;

        public XPathDocumentNavigator(XPathDocumentNavigator nav) : this(nav.pageCurrent, nav.idxCurrent, nav.pageParent, nav.idxParent)
        {
            this.atomizedLocalName = nav.atomizedLocalName;
        }

        public XPathDocumentNavigator(XPathNode[] pageCurrent, int idxCurrent, XPathNode[] pageParent, int idxParent)
        {
            this.pageCurrent = pageCurrent;
            this.pageParent = pageParent;
            this.idxCurrent = idxCurrent;
            this.idxParent = idxParent;
        }

        public override XPathNavigator Clone()
        {
            return new XPathDocumentNavigator(this.pageCurrent, this.idxCurrent, this.pageParent, this.idxParent);
        }

        public override XmlNodeOrder ComparePosition(XPathNavigator other)
        {
            XPathDocumentNavigator navigator = other as XPathDocumentNavigator;
            if (navigator != null)
            {
                XPathDocument document = this.pageCurrent[this.idxCurrent].Document;
                XPathDocument document2 = navigator.pageCurrent[navigator.idxCurrent].Document;
                if (document == document2)
                {
                    int primaryLocation = this.GetPrimaryLocation();
                    int secondaryLocation = navigator.GetPrimaryLocation();
                    if (primaryLocation == secondaryLocation)
                    {
                        primaryLocation = this.GetSecondaryLocation();
                        secondaryLocation = navigator.GetSecondaryLocation();
                        if (primaryLocation == secondaryLocation)
                        {
                            return XmlNodeOrder.Same;
                        }
                    }
                    if (primaryLocation >= secondaryLocation)
                    {
                        return XmlNodeOrder.After;
                    }
                    return XmlNodeOrder.Before;
                }
            }
            return XmlNodeOrder.Unknown;
        }

        private int GetFollowingEnd(XPathDocumentNavigator end, bool useParentOfVirtual, out XPathNode[] pageEnd)
        {
            if ((end != null) && (this.pageCurrent[this.idxCurrent].Document == end.pageCurrent[end.idxCurrent].Document))
            {
                if (end.idxParent == 0)
                {
                    pageEnd = end.pageCurrent;
                    return end.idxCurrent;
                }
                pageEnd = end.pageParent;
                if (!useParentOfVirtual)
                {
                    return (end.idxParent + 1);
                }
                return end.idxParent;
            }
            pageEnd = null;
            return 0;
        }

        public int GetPositionHashCode()
        {
            return (this.idxCurrent ^ this.idxParent);
        }

        private int GetPrimaryLocation()
        {
            if (this.idxParent == 0)
            {
                return XPathNodeHelper.GetLocation(this.pageCurrent, this.idxCurrent);
            }
            return XPathNodeHelper.GetLocation(this.pageParent, this.idxParent);
        }

        private int GetSecondaryLocation()
        {
            if (this.idxParent == 0)
            {
                return -2147483648;
            }
            switch (this.pageCurrent[this.idxCurrent].NodeType)
            {
                case XPathNodeType.Attribute:
                    return XPathNodeHelper.GetLocation(this.pageCurrent, this.idxCurrent);

                case XPathNodeType.Namespace:
                    return (-2147483647 + XPathNodeHelper.GetLocation(this.pageCurrent, this.idxCurrent));
            }
            return 0x7fffffff;
        }

        public bool HasLineInfo()
        {
            return this.pageCurrent[this.idxCurrent].Document.HasLineInfo;
        }

        public bool IsContentKindMatch(XPathNodeType typ)
        {
            return (((((int) 1) << this.pageCurrent[this.idxCurrent].NodeType) & XPathNavigator.GetContentKindMask(typ)) != 0);
        }

        public override bool IsDescendant(XPathNavigator other)
        {
            XPathDocumentNavigator navigator = other as XPathDocumentNavigator;
            if (navigator != null)
            {
                XPathNode[] pageParent;
                int idxParent;
                if (navigator.idxParent != 0)
                {
                    pageParent = navigator.pageParent;
                    idxParent = navigator.idxParent;
                }
                else
                {
                    idxParent = navigator.pageCurrent[navigator.idxCurrent].GetParent(out pageParent);
                }
                while (idxParent != 0)
                {
                    if ((idxParent == this.idxCurrent) && (pageParent == this.pageCurrent))
                    {
                        return true;
                    }
                    idxParent = pageParent[idxParent].GetParent(out pageParent);
                }
            }
            return false;
        }

        public bool IsElementMatch(string localName, string namespaceURI)
        {
            if (localName != this.atomizedLocalName)
            {
                this.atomizedLocalName = (localName != null) ? this.NameTable.Get(localName) : null;
            }
            if (this.idxParent != 0)
            {
                return false;
            }
            return this.pageCurrent[this.idxCurrent].ElementMatch(this.atomizedLocalName, namespaceURI);
        }

        public bool IsKindMatch(XPathNodeType typ)
        {
            return (((((int) 1) << this.pageCurrent[this.idxCurrent].NodeType) & XPathNavigator.GetKindMask(typ)) != 0);
        }

        public override bool IsSamePosition(XPathNavigator other)
        {
            XPathDocumentNavigator navigator = other as XPathDocumentNavigator;
            if (navigator == null)
            {
                return false;
            }
            return ((((this.idxCurrent == navigator.idxCurrent) && (this.pageCurrent == navigator.pageCurrent)) && (this.idxParent == navigator.idxParent)) && (this.pageParent == navigator.pageParent));
        }

        public override bool MoveTo(XPathNavigator other)
        {
            XPathDocumentNavigator navigator = other as XPathDocumentNavigator;
            if (navigator != null)
            {
                this.pageCurrent = navigator.pageCurrent;
                this.idxCurrent = navigator.idxCurrent;
                this.pageParent = navigator.pageParent;
                this.idxParent = navigator.idxParent;
                return true;
            }
            return false;
        }

        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            XPathNode[] pageCurrent = this.pageCurrent;
            int idxCurrent = this.idxCurrent;
            if (localName != this.atomizedLocalName)
            {
                this.atomizedLocalName = (localName != null) ? this.NameTable.Get(localName) : null;
            }
            if (XPathNodeHelper.GetAttribute(ref this.pageCurrent, ref this.idxCurrent, this.atomizedLocalName, namespaceURI))
            {
                this.pageParent = pageCurrent;
                this.idxParent = idxCurrent;
                return true;
            }
            return false;
        }

        public override bool MoveToChild(XPathNodeType type)
        {
            if (!this.pageCurrent[this.idxCurrent].HasCollapsedText)
            {
                return XPathNodeHelper.GetContentChild(ref this.pageCurrent, ref this.idxCurrent, type);
            }
            if ((type != XPathNodeType.Text) && (type != XPathNodeType.All))
            {
                return false;
            }
            this.pageParent = this.pageCurrent;
            this.idxParent = this.idxCurrent;
            this.idxCurrent = this.pageCurrent[this.idxCurrent].Document.GetCollapsedTextNode(out this.pageCurrent);
            return true;
        }

        public override bool MoveToChild(string localName, string namespaceURI)
        {
            if (localName != this.atomizedLocalName)
            {
                this.atomizedLocalName = (localName != null) ? this.NameTable.Get(localName) : null;
            }
            return XPathNodeHelper.GetElementChild(ref this.pageCurrent, ref this.idxCurrent, this.atomizedLocalName, namespaceURI);
        }

        public override bool MoveToFirstAttribute()
        {
            XPathNode[] pageCurrent = this.pageCurrent;
            int idxCurrent = this.idxCurrent;
            if (XPathNodeHelper.GetFirstAttribute(ref this.pageCurrent, ref this.idxCurrent))
            {
                this.pageParent = pageCurrent;
                this.idxParent = idxCurrent;
                return true;
            }
            return false;
        }

        public override bool MoveToFirstChild()
        {
            if (this.pageCurrent[this.idxCurrent].HasCollapsedText)
            {
                this.pageParent = this.pageCurrent;
                this.idxParent = this.idxCurrent;
                this.idxCurrent = this.pageCurrent[this.idxCurrent].Document.GetCollapsedTextNode(out this.pageCurrent);
                return true;
            }
            return XPathNodeHelper.GetContentChild(ref this.pageCurrent, ref this.idxCurrent);
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            XPathNode[] nodeArray;
            int sibling;
            if (namespaceScope == XPathNamespaceScope.Local)
            {
                sibling = XPathNodeHelper.GetLocalNamespaces(this.pageCurrent, this.idxCurrent, out nodeArray);
            }
            else
            {
                sibling = XPathNodeHelper.GetInScopeNamespaces(this.pageCurrent, this.idxCurrent, out nodeArray);
            }
            while (sibling != 0)
            {
                if ((namespaceScope != XPathNamespaceScope.ExcludeXml) || !nodeArray[sibling].IsXmlNamespaceNode)
                {
                    this.pageParent = this.pageCurrent;
                    this.idxParent = this.idxCurrent;
                    this.pageCurrent = nodeArray;
                    this.idxCurrent = sibling;
                    return true;
                }
                sibling = nodeArray[sibling].GetSibling(out nodeArray);
            }
            return false;
        }

        public override bool MoveToFollowing(XPathNodeType type, XPathNavigator end)
        {
            XPathNode[] nodeArray2;
            int num2;
            XPathDocumentNavigator navigator = end as XPathDocumentNavigator;
            if ((type == XPathNodeType.Text) || (type == XPathNodeType.All))
            {
                if (this.pageCurrent[this.idxCurrent].HasCollapsedText)
                {
                    if (((navigator != null) && (this.idxCurrent == navigator.idxParent)) && (this.pageCurrent == navigator.pageParent))
                    {
                        return false;
                    }
                    this.pageParent = this.pageCurrent;
                    this.idxParent = this.idxCurrent;
                    this.idxCurrent = this.pageCurrent[this.idxCurrent].Document.GetCollapsedTextNode(out this.pageCurrent);
                    return true;
                }
                if (type == XPathNodeType.Text)
                {
                    XPathNode[] pageParent;
                    int idxParent;
                    num2 = this.GetFollowingEnd(navigator, true, out nodeArray2);
                    if (this.idxParent != 0)
                    {
                        pageParent = this.pageParent;
                        idxParent = this.idxParent;
                    }
                    else
                    {
                        pageParent = this.pageCurrent;
                        idxParent = this.idxCurrent;
                    }
                    if (((navigator != null) && (navigator.idxParent != 0)) && ((idxParent == num2) && (pageParent == nodeArray2)))
                    {
                        return false;
                    }
                    if (!XPathNodeHelper.GetTextFollowing(ref pageParent, ref idxParent, nodeArray2, num2))
                    {
                        return false;
                    }
                    if (pageParent[idxParent].NodeType == XPathNodeType.Element)
                    {
                        this.idxCurrent = pageParent[idxParent].Document.GetCollapsedTextNode(out this.pageCurrent);
                        this.pageParent = pageParent;
                        this.idxParent = idxParent;
                    }
                    else
                    {
                        this.pageCurrent = pageParent;
                        this.idxCurrent = idxParent;
                        this.pageParent = null;
                        this.idxParent = 0;
                    }
                    return true;
                }
            }
            num2 = this.GetFollowingEnd(navigator, false, out nodeArray2);
            if (this.idxParent == 0)
            {
                return XPathNodeHelper.GetContentFollowing(ref this.pageCurrent, ref this.idxCurrent, nodeArray2, num2, type);
            }
            if (!XPathNodeHelper.GetContentFollowing(ref this.pageParent, ref this.idxParent, nodeArray2, num2, type))
            {
                return false;
            }
            this.pageCurrent = this.pageParent;
            this.idxCurrent = this.idxParent;
            this.pageParent = null;
            this.idxParent = 0;
            return true;
        }

        public override bool MoveToFollowing(string localName, string namespaceURI, XPathNavigator end)
        {
            XPathNode[] nodeArray;
            if (localName != this.atomizedLocalName)
            {
                this.atomizedLocalName = (localName != null) ? this.NameTable.Get(localName) : null;
            }
            int idxEnd = this.GetFollowingEnd(end as XPathDocumentNavigator, false, out nodeArray);
            if (this.idxParent == 0)
            {
                return XPathNodeHelper.GetElementFollowing(ref this.pageCurrent, ref this.idxCurrent, nodeArray, idxEnd, this.atomizedLocalName, namespaceURI);
            }
            if (!XPathNodeHelper.GetElementFollowing(ref this.pageParent, ref this.idxParent, nodeArray, idxEnd, this.atomizedLocalName, namespaceURI))
            {
                return false;
            }
            this.pageCurrent = this.pageParent;
            this.idxCurrent = this.idxParent;
            this.pageParent = null;
            this.idxParent = 0;
            return true;
        }

        public override bool MoveToId(string id)
        {
            XPathNode[] nodeArray;
            int idElement = this.pageCurrent[this.idxCurrent].Document.LookupIdElement(id, out nodeArray);
            if (idElement != 0)
            {
                this.pageCurrent = nodeArray;
                this.idxCurrent = idElement;
                this.pageParent = null;
                this.idxParent = 0;
                return true;
            }
            return false;
        }

        public override bool MoveToNext()
        {
            return XPathNodeHelper.GetContentSibling(ref this.pageCurrent, ref this.idxCurrent);
        }

        public override bool MoveToNext(XPathNodeType type)
        {
            return XPathNodeHelper.GetContentSibling(ref this.pageCurrent, ref this.idxCurrent, type);
        }

        public override bool MoveToNext(string localName, string namespaceURI)
        {
            if (localName != this.atomizedLocalName)
            {
                this.atomizedLocalName = (localName != null) ? this.NameTable.Get(localName) : null;
            }
            return XPathNodeHelper.GetElementSibling(ref this.pageCurrent, ref this.idxCurrent, this.atomizedLocalName, namespaceURI);
        }

        public override bool MoveToNextAttribute()
        {
            return XPathNodeHelper.GetNextAttribute(ref this.pageCurrent, ref this.idxCurrent);
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope scope)
        {
            XPathNode[] pageCurrent = this.pageCurrent;
            int idxCurrent = this.idxCurrent;
            if (pageCurrent[idxCurrent].NodeType != XPathNodeType.Namespace)
            {
                return false;
            }
        Label_001F:
            idxCurrent = pageCurrent[idxCurrent].GetSibling(out pageCurrent);
            if (idxCurrent == 0)
            {
                return false;
            }
            switch (scope)
            {
                case XPathNamespaceScope.ExcludeXml:
                    if (pageCurrent[idxCurrent].IsXmlNamespaceNode)
                    {
                        goto Label_001F;
                    }
                    break;

                case XPathNamespaceScope.Local:
                    XPathNode[] nodeArray2;
                    if ((pageCurrent[idxCurrent].GetParent(out nodeArray2) == this.idxParent) && (nodeArray2 == this.pageParent))
                    {
                        break;
                    }
                    return false;
            }
            this.pageCurrent = pageCurrent;
            this.idxCurrent = idxCurrent;
            return true;
        }

        public override bool MoveToParent()
        {
            if (this.idxParent != 0)
            {
                this.pageCurrent = this.pageParent;
                this.idxCurrent = this.idxParent;
                this.pageParent = null;
                this.idxParent = 0;
                return true;
            }
            return XPathNodeHelper.GetParent(ref this.pageCurrent, ref this.idxCurrent);
        }

        public override bool MoveToPrevious()
        {
            if (this.idxParent != 0)
            {
                return false;
            }
            return XPathNodeHelper.GetPreviousContentSibling(ref this.pageCurrent, ref this.idxCurrent);
        }

        public override void MoveToRoot()
        {
            if (this.idxParent != 0)
            {
                this.pageParent = null;
                this.idxParent = 0;
            }
            this.idxCurrent = this.pageCurrent[this.idxCurrent].GetRoot(out this.pageCurrent);
        }

        public override XPathNodeIterator SelectChildren(XPathNodeType type)
        {
            return new XPathDocumentKindChildIterator(this, type);
        }

        public override XPathNodeIterator SelectChildren(string name, string namespaceURI)
        {
            if ((name != null) && (name.Length != 0))
            {
                return new XPathDocumentElementChildIterator(this, name, namespaceURI);
            }
            return base.SelectChildren(name, namespaceURI);
        }

        public override XPathNodeIterator SelectDescendants(XPathNodeType type, bool matchSelf)
        {
            return new XPathDocumentKindDescendantIterator(this, type, matchSelf);
        }

        public override XPathNodeIterator SelectDescendants(string name, string namespaceURI, bool matchSelf)
        {
            if ((name != null) && (name.Length != 0))
            {
                return new XPathDocumentElementDescendantIterator(this, name, namespaceURI, matchSelf);
            }
            return base.SelectDescendants(name, namespaceURI, matchSelf);
        }

        public override string BaseURI
        {
            get
            {
                XPathNode[] pageParent;
                int idxParent;
                if (this.idxParent != 0)
                {
                    pageParent = this.pageParent;
                    idxParent = this.idxParent;
                }
                else
                {
                    pageParent = this.pageCurrent;
                    idxParent = this.idxCurrent;
                }
                do
                {
                    switch (pageParent[idxParent].NodeType)
                    {
                        case XPathNodeType.Root:
                        case XPathNodeType.Element:
                        case XPathNodeType.ProcessingInstruction:
                            return pageParent[idxParent].BaseUri;
                    }
                    idxParent = pageParent[idxParent].GetParent(out pageParent);
                }
                while (idxParent != 0);
                return string.Empty;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                return this.pageCurrent[this.idxCurrent].HasAttribute;
            }
        }

        public override bool HasChildren
        {
            get
            {
                return this.pageCurrent[this.idxCurrent].HasContentChild;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this.pageCurrent[this.idxCurrent].AllowShortcutTag;
            }
        }

        public int LineNumber
        {
            get
            {
                if ((this.idxParent != 0) && (this.NodeType == XPathNodeType.Text))
                {
                    return this.pageParent[this.idxParent].LineNumber;
                }
                return this.pageCurrent[this.idxCurrent].LineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                if ((this.idxParent != 0) && (this.NodeType == XPathNodeType.Text))
                {
                    return this.pageParent[this.idxParent].CollapsedLinePosition;
                }
                return this.pageCurrent[this.idxCurrent].LinePosition;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.pageCurrent[this.idxCurrent].LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this.pageCurrent[this.idxCurrent].Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.pageCurrent[this.idxCurrent].NamespaceUri;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.pageCurrent[this.idxCurrent].Document.NameTable;
            }
        }

        public override XPathNodeType NodeType
        {
            get
            {
                return this.pageCurrent[this.idxCurrent].NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.pageCurrent[this.idxCurrent].Prefix;
            }
        }

        public override object UnderlyingObject
        {
            get
            {
                return this.Clone();
            }
        }

        internal override string UniqueId
        {
            get
            {
                int num2;
                char[] chArray = new char[0x10];
                int length = 0;
                chArray[length++] = XPathNavigator.NodeTypeLetter[(int) this.pageCurrent[this.idxCurrent].NodeType];
                if (this.idxParent != 0)
                {
                    num2 = ((this.pageParent[0].PageInfo.PageNumber - 1) << 0x10) | (this.idxParent - 1);
                    do
                    {
                        chArray[length++] = XPathNavigator.UniqueIdTbl[num2 & 0x1f];
                        num2 = num2 >> 5;
                    }
                    while (num2 != 0);
                    chArray[length++] = '0';
                }
                num2 = ((this.pageCurrent[0].PageInfo.PageNumber - 1) << 0x10) | (this.idxCurrent - 1);
                do
                {
                    chArray[length++] = XPathNavigator.UniqueIdTbl[num2 & 0x1f];
                    num2 = num2 >> 5;
                }
                while (num2 != 0);
                return new string(chArray, 0, length);
            }
        }

        public override string Value
        {
            get
            {
                XPathNode[] nodeArray2;
                int num2;
                string str = this.pageCurrent[this.idxCurrent].Value;
                if (str != null)
                {
                    return str;
                }
                if (this.idxParent != 0)
                {
                    return this.pageParent[this.idxParent].Value;
                }
                string str2 = string.Empty;
                StringBuilder builder = null;
                XPathNode[] pageCurrent = nodeArray2 = this.pageCurrent;
                int idxCurrent = num2 = this.idxCurrent;
                if (!XPathNodeHelper.GetNonDescendant(ref nodeArray2, ref num2))
                {
                    nodeArray2 = null;
                    num2 = 0;
                }
                while (XPathNodeHelper.GetTextFollowing(ref pageCurrent, ref idxCurrent, nodeArray2, num2))
                {
                    if (str2.Length == 0)
                    {
                        str2 = pageCurrent[idxCurrent].Value;
                    }
                    else
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder();
                            builder.Append(str2);
                        }
                        builder.Append(pageCurrent[idxCurrent].Value);
                    }
                }
                if (builder == null)
                {
                    return str2;
                }
                return builder.ToString();
            }
        }
    }
}

