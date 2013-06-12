namespace MS.Internal.Xml.Cache
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Xml.XPath;

    [StructLayout(LayoutKind.Sequential)]
    internal struct XPathNode
    {
        private const uint NodeTypeMask = 15;
        private const uint HasAttributeBit = 0x10;
        private const uint HasContentChildBit = 0x20;
        private const uint HasElementChildBit = 0x40;
        private const uint HasCollapsedTextBit = 0x80;
        private const uint AllowShortcutTagBit = 0x100;
        private const uint HasNmspDeclsBit = 0x200;
        private const uint LineNumberMask = 0xfffc00;
        private const int LineNumberShift = 10;
        private const int CollapsedPositionShift = 0x18;
        public const int MaxLineNumberOffset = 0x3fff;
        public const int MaxLinePositionOffset = 0xffff;
        public const int MaxCollapsedPositionOffset = 0xff;
        private XPathNodeInfoAtom info;
        private ushort idxSibling;
        private ushort idxParent;
        private ushort idxSimilar;
        private ushort posOffset;
        private uint props;
        private string value;
        public XPathNodeType NodeType
        {
            get
            {
                return (((XPathNodeType) this.props) & (XPathNodeType.All | XPathNodeType.Whitespace));
            }
        }
        public string Prefix
        {
            get
            {
                return this.info.Prefix;
            }
        }
        public string LocalName
        {
            get
            {
                return this.info.LocalName;
            }
        }
        public string Name
        {
            get
            {
                if (this.Prefix.Length == 0)
                {
                    return this.LocalName;
                }
                return (this.Prefix + ":" + this.LocalName);
            }
        }
        public string NamespaceUri
        {
            get
            {
                return this.info.NamespaceUri;
            }
        }
        public XPathDocument Document
        {
            get
            {
                return this.info.Document;
            }
        }
        public string BaseUri
        {
            get
            {
                return this.info.BaseUri;
            }
        }
        public int LineNumber
        {
            get
            {
                return (this.info.LineNumberBase + ((int) ((this.props & 0xfffc00) >> 10)));
            }
        }
        public int LinePosition
        {
            get
            {
                return (this.info.LinePositionBase + this.posOffset);
            }
        }
        public int CollapsedLinePosition
        {
            get
            {
                return (this.LinePosition + ((int) (this.props >> 0x18)));
            }
        }
        public XPathNodePageInfo PageInfo
        {
            get
            {
                return this.info.PageInfo;
            }
        }
        public int GetRoot(out XPathNode[] pageNode)
        {
            return this.info.Document.GetRootNode(out pageNode);
        }

        public int GetParent(out XPathNode[] pageNode)
        {
            pageNode = this.info.ParentPage;
            return this.idxParent;
        }

        public int GetSibling(out XPathNode[] pageNode)
        {
            pageNode = this.info.SiblingPage;
            return this.idxSibling;
        }

        public int GetSimilarElement(out XPathNode[] pageNode)
        {
            pageNode = this.info.SimilarElementPage;
            return this.idxSimilar;
        }

        public bool NameMatch(string localName, string namespaceName)
        {
            return ((this.info.LocalName == localName) && (this.info.NamespaceUri == namespaceName));
        }

        public bool ElementMatch(string localName, string namespaceName)
        {
            return (((this.NodeType == XPathNodeType.Element) && (this.info.LocalName == localName)) && (this.info.NamespaceUri == namespaceName));
        }

        public bool IsXmlNamespaceNode
        {
            get
            {
                string localName = this.info.LocalName;
                return (((this.NodeType == XPathNodeType.Namespace) && (localName.Length == 3)) && (localName == "xml"));
            }
        }
        public bool HasSibling
        {
            get
            {
                return (this.idxSibling != 0);
            }
        }
        public bool HasCollapsedText
        {
            get
            {
                return ((this.props & 0x80) != 0);
            }
        }
        public bool HasAttribute
        {
            get
            {
                return ((this.props & 0x10) != 0);
            }
        }
        public bool HasContentChild
        {
            get
            {
                return ((this.props & 0x20) != 0);
            }
        }
        public bool HasElementChild
        {
            get
            {
                return ((this.props & 0x40) != 0);
            }
        }
        public bool IsAttrNmsp
        {
            get
            {
                XPathNodeType nodeType = this.NodeType;
                if (nodeType != XPathNodeType.Attribute)
                {
                    return (nodeType == XPathNodeType.Namespace);
                }
                return true;
            }
        }
        public bool IsText
        {
            get
            {
                return XPathNavigator.IsText(this.NodeType);
            }
        }
        public bool HasNamespaceDecls
        {
            get
            {
                return ((this.props & 0x200) != 0);
            }
            set
            {
                if (value)
                {
                    this.props |= 0x200;
                }
                else
                {
                    this.props &= 0xff;
                }
            }
        }
        public bool AllowShortcutTag
        {
            get
            {
                return ((this.props & 0x100) != 0);
            }
        }
        public int LocalNameHashCode
        {
            get
            {
                return this.info.LocalNameHashCode;
            }
        }
        public string Value
        {
            get
            {
                return this.value;
            }
        }
        public void Create(XPathNodePageInfo pageInfo)
        {
            this.info = new XPathNodeInfoAtom(pageInfo);
        }

        public void Create(XPathNodeInfoAtom info, XPathNodeType xptyp, int idxParent)
        {
            this.info = info;
            this.props = (uint) xptyp;
            this.idxParent = (ushort) idxParent;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public void SetLineInfoOffsets(int lineNumOffset, int linePosOffset)
        {
            this.props |= (uint) (lineNumOffset << 10);
            this.posOffset = (ushort) linePosOffset;
        }

        public void SetCollapsedLineInfoOffset(int posOffset)
        {
            this.props |= (uint) (posOffset << 0x18);
        }

        public void SetValue(string value)
        {
            this.value = value;
        }

        public void SetEmptyValue(bool allowShortcutTag)
        {
            this.value = string.Empty;
            if (allowShortcutTag)
            {
                this.props |= 0x100;
            }
        }

        public void SetCollapsedValue(string value)
        {
            this.value = value;
            this.props |= 160;
        }

        public void SetParentProperties(XPathNodeType xptyp)
        {
            if (xptyp == XPathNodeType.Attribute)
            {
                this.props |= 0x10;
            }
            else
            {
                this.props |= 0x20;
                if (xptyp == XPathNodeType.Element)
                {
                    this.props |= 0x40;
                }
            }
        }

        public void SetSibling(XPathNodeInfoTable infoTable, XPathNode[] pageSibling, int idxSibling)
        {
            this.idxSibling = (ushort) idxSibling;
            if (pageSibling != this.info.SiblingPage)
            {
                this.info = infoTable.Create(this.info.LocalName, this.info.NamespaceUri, this.info.Prefix, this.info.BaseUri, this.info.ParentPage, pageSibling, this.info.SimilarElementPage, this.info.Document, this.info.LineNumberBase, this.info.LinePositionBase);
            }
        }

        public void SetSimilarElement(XPathNodeInfoTable infoTable, XPathNode[] pageSimilar, int idxSimilar)
        {
            this.idxSimilar = (ushort) idxSimilar;
            if (pageSimilar != this.info.SimilarElementPage)
            {
                this.info = infoTable.Create(this.info.LocalName, this.info.NamespaceUri, this.info.Prefix, this.info.BaseUri, this.info.ParentPage, this.info.SiblingPage, pageSimilar, this.info.Document, this.info.LineNumberBase, this.info.LinePositionBase);
            }
        }
    }
}

