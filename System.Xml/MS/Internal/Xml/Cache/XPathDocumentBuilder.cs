namespace MS.Internal.Xml.Cache
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.XPath;

    internal sealed class XPathDocumentBuilder : XmlRawWriter
    {
        private bool atomizeNames;
        private XPathDocument doc;
        private const int ElementIndexSize = 0x40;
        private Hashtable elemIdMap;
        private XPathNodeRef[] elemNameIndex;
        private XmlQualifiedName idAttrName;
        private int idxNmsp;
        private int idxParent;
        private int idxSibling;
        private XPathNodeInfoTable infoTable;
        private IXmlLineInfo lineInfo;
        private int lineNumBase;
        private int linePosBase;
        private XmlNameTable nameTable;
        private NodePageFactory nmspPageFact;
        private NodePageFactory nodePageFact;
        private XPathNode[] pageNmsp;
        private XPathNode[] pageParent;
        private XPathNode[] pageSibling;
        private Stack<XPathNodeRef> stkNmsp;
        private TextBlockBuilder textBldr;

        public XPathDocumentBuilder(XPathDocument doc, IXmlLineInfo lineInfo, string baseUri, XPathDocument.LoadFlags flags)
        {
            this.nodePageFact.Init(0x100);
            this.nmspPageFact.Init(0x10);
            this.stkNmsp = new Stack<XPathNodeRef>();
            this.Initialize(doc, lineInfo, baseUri, flags);
        }

        private void AddSibling(XPathNodeType xptyp, string localName, string namespaceUri, string prefix, string baseUri)
        {
            XPathNode[] nodeArray;
            if (this.textBldr.HasText)
            {
                this.CachedTextNode();
            }
            int idxSibling = this.NewNode(out nodeArray, xptyp, localName, namespaceUri, prefix, baseUri);
            if (this.idxParent != 0)
            {
                this.pageParent[this.idxParent].SetParentProperties(xptyp);
                if (this.idxSibling != 0)
                {
                    this.pageSibling[this.idxSibling].SetSibling(this.infoTable, nodeArray, idxSibling);
                }
            }
            this.pageSibling = nodeArray;
            this.idxSibling = idxSibling;
        }

        private void CachedTextNode()
        {
            TextBlockType textType = this.textBldr.TextType;
            string str = this.textBldr.ReadText();
            this.AddSibling((XPathNodeType) textType, string.Empty, string.Empty, string.Empty, string.Empty);
            this.pageSibling[this.idxSibling].SetValue(str);
        }

        public override void Close()
        {
            XPathNode[] nodeArray;
            if (this.textBldr.HasText)
            {
                this.CachedTextNode();
            }
            if ((this.doc.GetRootNode(out nodeArray) == this.nodePageFact.NextNodeIndex) && (nodeArray == this.nodePageFact.NextNodePage))
            {
                this.AddSibling(XPathNodeType.Text, string.Empty, string.Empty, string.Empty, string.Empty);
                this.pageSibling[this.idxSibling].SetValue(string.Empty);
            }
        }

        private void ComputeLineInfo(bool isTextNode, out int lineNumOffset, out int linePosOffset)
        {
            if (this.lineInfo == null)
            {
                lineNumOffset = 0;
                linePosOffset = 0;
            }
            else
            {
                int lineNumber;
                int linePosition;
                if (isTextNode)
                {
                    lineNumber = this.textBldr.LineNumber;
                    linePosition = this.textBldr.LinePosition;
                }
                else
                {
                    lineNumber = this.lineInfo.LineNumber;
                    linePosition = this.lineInfo.LinePosition;
                }
                lineNumOffset = lineNumber - this.lineNumBase;
                if ((lineNumOffset < 0) || (lineNumOffset > 0x3fff))
                {
                    this.lineNumBase = lineNumber;
                    lineNumOffset = 0;
                }
                linePosOffset = linePosition - this.linePosBase;
                if ((linePosOffset < 0) || (linePosOffset > 0xffff))
                {
                    this.linePosBase = linePosition;
                    linePosOffset = 0;
                }
            }
        }

        public void CreateIdTables(IDtdInfo dtdInfo)
        {
            foreach (IDtdAttributeListInfo info in dtdInfo.GetAttributeLists())
            {
                IDtdAttributeInfo idAttribute = info.LookupIdAttribute();
                if (idAttribute != null)
                {
                    if (this.elemIdMap == null)
                    {
                        this.elemIdMap = new Hashtable();
                    }
                    this.elemIdMap.Add(new XmlQualifiedName(info.LocalName, info.Prefix), new XmlQualifiedName(idAttribute.LocalName, idAttribute.Prefix));
                }
            }
        }

        public override void Flush()
        {
        }

        public void Initialize(XPathDocument doc, IXmlLineInfo lineInfo, string baseUri, XPathDocument.LoadFlags flags)
        {
            XPathNode[] nodeArray;
            this.doc = doc;
            this.nameTable = doc.NameTable;
            this.atomizeNames = (flags & XPathDocument.LoadFlags.AtomizeNames) != XPathDocument.LoadFlags.None;
            this.idxParent = this.idxSibling = 0;
            this.elemNameIndex = new XPathNodeRef[0x40];
            this.textBldr.Initialize(lineInfo);
            this.lineInfo = lineInfo;
            this.lineNumBase = 0;
            this.linePosBase = 0;
            this.infoTable = new XPathNodeInfoTable();
            int idxText = this.NewNode(out nodeArray, XPathNodeType.Text, string.Empty, string.Empty, string.Empty, string.Empty);
            this.doc.SetCollapsedTextNode(nodeArray, idxText);
            this.idxNmsp = this.NewNamespaceNode(out this.pageNmsp, this.nameTable.Add("xml"), this.nameTable.Add("http://www.w3.org/XML/1998/namespace"), null, 0);
            this.doc.SetXmlNamespaceNode(this.pageNmsp, this.idxNmsp);
            if ((flags & XPathDocument.LoadFlags.Fragment) == XPathDocument.LoadFlags.None)
            {
                this.idxParent = this.NewNode(out this.pageParent, XPathNodeType.Root, string.Empty, string.Empty, string.Empty, baseUri);
                this.doc.SetRootNode(this.pageParent, this.idxParent);
            }
            else
            {
                this.doc.SetRootNode(this.nodePageFact.NextNodePage, this.nodePageFact.NextNodeIndex);
            }
        }

        private XPathNodeRef LinkSimilarElements(XPathNode[] pagePrev, int idxPrev, XPathNode[] pageNext, int idxNext)
        {
            if (pagePrev != null)
            {
                pagePrev[idxPrev].SetSimilarElement(this.infoTable, pageNext, idxNext);
            }
            return new XPathNodeRef(pageNext, idxNext);
        }

        private int NewNamespaceNode(out XPathNode[] page, string prefix, string namespaceUri, XPathNode[] pageElem, int idxElem)
        {
            XPathNode[] nodeArray;
            int num;
            int num2;
            int num3;
            this.nmspPageFact.AllocateSlot(out nodeArray, out num);
            this.ComputeLineInfo(false, out num2, out num3);
            XPathNodeInfoAtom info = this.infoTable.Create(prefix, string.Empty, string.Empty, string.Empty, pageElem, nodeArray, null, this.doc, this.lineNumBase, this.linePosBase);
            nodeArray[num].Create(info, XPathNodeType.Namespace, idxElem);
            nodeArray[num].SetValue(namespaceUri);
            nodeArray[num].SetLineInfoOffsets(num2, num3);
            page = nodeArray;
            return num;
        }

        private int NewNode(out XPathNode[] page, XPathNodeType xptyp, string localName, string namespaceUri, string prefix, string baseUri)
        {
            XPathNode[] nodeArray;
            int num;
            int num2;
            int num3;
            this.nodePageFact.AllocateSlot(out nodeArray, out num);
            this.ComputeLineInfo(XPathNavigator.IsText(xptyp), out num2, out num3);
            XPathNodeInfoAtom info = this.infoTable.Create(localName, namespaceUri, prefix, baseUri, this.pageParent, nodeArray, nodeArray, this.doc, this.lineNumBase, this.linePosBase);
            nodeArray[num].Create(info, xptyp, this.idxParent);
            nodeArray[num].SetLineInfoOffsets(num2, num3);
            page = nodeArray;
            return num;
        }

        internal override void StartElementContent()
        {
        }

        public override void WriteCData(string text)
        {
            this.WriteString(text, TextBlockType.Text);
        }

        public override void WriteCharEntity(char ch)
        {
            char[] chArray = new char[] { ch };
            this.WriteString(new string(chArray), TextBlockType.Text);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.WriteString(new string(buffer, index, count), TextBlockType.Text);
        }

        public override void WriteComment(string text)
        {
            this.AddSibling(XPathNodeType.Comment, string.Empty, string.Empty, string.Empty, string.Empty);
            this.pageSibling[this.idxSibling].SetValue(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
        }

        public override void WriteEndAttribute()
        {
            this.pageSibling[this.idxSibling].SetValue(this.textBldr.ReadText());
            if (((this.idAttrName != null) && (this.pageSibling[this.idxSibling].LocalName == this.idAttrName.Name)) && (this.pageSibling[this.idxSibling].Prefix == this.idAttrName.Namespace))
            {
                this.doc.AddIdElement(this.pageSibling[this.idxSibling].Value, this.pageParent, this.idxParent);
            }
        }

        public override void WriteEndElement()
        {
            this.WriteEndElement(true);
        }

        public void WriteEndElement(bool allowShortcutTag)
        {
            if (this.pageParent[this.idxParent].HasContentChild)
            {
                if (this.textBldr.HasText)
                {
                    this.CachedTextNode();
                }
            }
            else
            {
                switch (this.textBldr.TextType)
                {
                    case TextBlockType.Text:
                        if (this.lineInfo != null)
                        {
                            if (this.textBldr.LineNumber != this.pageParent[this.idxParent].LineNumber)
                            {
                                break;
                            }
                            int posOffset = this.textBldr.LinePosition - this.pageParent[this.idxParent].LinePosition;
                            if ((posOffset < 0) || (posOffset > 0xff))
                            {
                                break;
                            }
                            this.pageParent[this.idxParent].SetCollapsedLineInfoOffset(posOffset);
                        }
                        this.pageParent[this.idxParent].SetCollapsedValue(this.textBldr.ReadText());
                        goto Label_0134;

                    case TextBlockType.SignificantWhitespace:
                    case TextBlockType.Whitespace:
                        break;

                    default:
                        this.pageParent[this.idxParent].SetEmptyValue(allowShortcutTag);
                        goto Label_0134;
                }
                this.CachedTextNode();
                this.pageParent[this.idxParent].SetValue(this.pageSibling[this.idxSibling].Value);
            }
        Label_0134:
            if (this.pageParent[this.idxParent].HasNamespaceDecls)
            {
                this.doc.AddNamespace(this.pageParent, this.idxParent, this.pageNmsp, this.idxNmsp);
                XPathNodeRef ref2 = this.stkNmsp.Pop();
                this.pageNmsp = ref2.Page;
                this.idxNmsp = ref2.Index;
            }
            this.pageSibling = this.pageParent;
            this.idxSibling = this.idxParent;
            this.idxParent = this.pageParent[this.idxParent].GetParent(out this.pageParent);
        }

        internal override void WriteEndElement(string prefix, string localName, string namespaceName)
        {
            this.WriteEndElement(true);
        }

        public override void WriteEntityRef(string name)
        {
            throw new NotImplementedException();
        }

        public override void WriteFullEndElement()
        {
            this.WriteEndElement(false);
        }

        internal override void WriteFullEndElement(string prefix, string localName, string namespaceName)
        {
            this.WriteEndElement(false);
        }

        internal override void WriteNamespaceDeclaration(string prefix, string namespaceName)
        {
            XPathNode[] nodeArray3;
            if (this.atomizeNames)
            {
                prefix = this.nameTable.Add(prefix);
            }
            namespaceName = this.nameTable.Add(namespaceName);
            XPathNode[] pageNmsp = this.pageNmsp;
            int idxNmsp = this.idxNmsp;
            while (idxNmsp != 0)
            {
                if (pageNmsp[idxNmsp].LocalName == prefix)
                {
                    break;
                }
                idxNmsp = pageNmsp[idxNmsp].GetSibling(out pageNmsp);
            }
            int index = this.NewNamespaceNode(out nodeArray3, prefix, namespaceName, this.pageParent, this.idxParent);
            if (idxNmsp != 0)
            {
                XPathNode[] pageNode = this.pageNmsp;
                int sibling = this.idxNmsp;
                XPathNode[] nodeArray5 = nodeArray3;
                int num5 = index;
                while ((sibling != idxNmsp) || (pageNode != pageNmsp))
                {
                    XPathNode[] nodeArray;
                    int parent = pageNode[sibling].GetParent(out nodeArray);
                    parent = this.NewNamespaceNode(out nodeArray, pageNode[sibling].LocalName, pageNode[sibling].Value, nodeArray, parent);
                    nodeArray5[num5].SetSibling(this.infoTable, nodeArray, parent);
                    nodeArray5 = nodeArray;
                    num5 = parent;
                    sibling = pageNode[sibling].GetSibling(out pageNode);
                }
                idxNmsp = pageNmsp[idxNmsp].GetSibling(out pageNmsp);
                if (idxNmsp != 0)
                {
                    nodeArray5[num5].SetSibling(this.infoTable, pageNmsp, idxNmsp);
                }
            }
            else if (this.idxParent != 0)
            {
                nodeArray3[index].SetSibling(this.infoTable, this.pageNmsp, this.idxNmsp);
            }
            else
            {
                this.doc.SetRootNode(nodeArray3, index);
            }
            if (this.idxParent != 0)
            {
                if (!this.pageParent[this.idxParent].HasNamespaceDecls)
                {
                    this.stkNmsp.Push(new XPathNodeRef(this.pageNmsp, this.idxNmsp));
                    this.pageParent[this.idxParent].HasNamespaceDecls = true;
                }
                this.pageNmsp = nodeArray3;
                this.idxNmsp = index;
            }
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this.WriteProcessingInstruction(name, text, string.Empty);
        }

        public void WriteProcessingInstruction(string name, string text, string baseUri)
        {
            if (this.atomizeNames)
            {
                name = this.nameTable.Add(name);
            }
            this.AddSibling(XPathNodeType.ProcessingInstruction, name, string.Empty, string.Empty, baseUri);
            this.pageSibling[this.idxSibling].SetValue(text);
        }

        public override void WriteRaw(string data)
        {
            this.WriteString(data, TextBlockType.Text);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this.WriteString(new string(buffer, index, count), TextBlockType.Text);
        }

        public override void WriteStartAttribute(string prefix, string localName, string namespaceName)
        {
            if (this.atomizeNames)
            {
                prefix = this.nameTable.Add(prefix);
                localName = this.nameTable.Add(localName);
                namespaceName = this.nameTable.Add(namespaceName);
            }
            this.AddSibling(XPathNodeType.Attribute, localName, namespaceName, prefix, string.Empty);
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.WriteStartElement(prefix, localName, ns, string.Empty);
        }

        public void WriteStartElement(string prefix, string localName, string ns, string baseUri)
        {
            if (this.atomizeNames)
            {
                prefix = this.nameTable.Add(prefix);
                localName = this.nameTable.Add(localName);
                ns = this.nameTable.Add(ns);
            }
            this.AddSibling(XPathNodeType.Element, localName, ns, prefix, baseUri);
            this.pageParent = this.pageSibling;
            this.idxParent = this.idxSibling;
            this.idxSibling = 0;
            int index = this.pageParent[this.idxParent].LocalNameHashCode & 0x3f;
            this.elemNameIndex[index] = this.LinkSimilarElements(this.elemNameIndex[index].Page, this.elemNameIndex[index].Index, this.pageParent, this.idxParent);
            if (this.elemIdMap != null)
            {
                this.idAttrName = (XmlQualifiedName) this.elemIdMap[new XmlQualifiedName(localName, prefix)];
            }
        }

        public override void WriteString(string text)
        {
            this.WriteString(text, TextBlockType.Text);
        }

        public void WriteString(string text, TextBlockType textType)
        {
            this.textBldr.WriteTextBlock(text, textType);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            char[] chArray = new char[] { highChar, lowChar };
            this.WriteString(new string(chArray), TextBlockType.Text);
        }

        public override void WriteWhitespace(string ws)
        {
            this.WriteString(ws, TextBlockType.Whitespace);
        }

        internal override void WriteXmlDeclaration(string xmldecl)
        {
        }

        internal override void WriteXmlDeclaration(XmlStandalone standalone)
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NodePageFactory
        {
            private XPathNode[] page;
            private XPathNodePageInfo pageInfo;
            private int pageSize;
            public void Init(int initialPageSize)
            {
                this.pageSize = initialPageSize;
                this.page = new XPathNode[this.pageSize];
                this.pageInfo = new XPathNodePageInfo(null, 1);
                this.page[0].Create(this.pageInfo);
            }

            public XPathNode[] NextNodePage
            {
                get
                {
                    return this.page;
                }
            }
            public int NextNodeIndex
            {
                get
                {
                    return this.pageInfo.NodeCount;
                }
            }
            public void AllocateSlot(out XPathNode[] page, out int idx)
            {
                page = this.page;
                idx = this.pageInfo.NodeCount;
                if (++this.pageInfo.NodeCount >= this.page.Length)
                {
                    if (this.pageSize < 0x10000)
                    {
                        this.pageSize *= 2;
                    }
                    this.page = new XPathNode[this.pageSize];
                    this.pageInfo.NextPage = this.page;
                    this.pageInfo = new XPathNodePageInfo(page, this.pageInfo.PageNumber + 1);
                    this.page[0].Create(this.pageInfo);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TextBlockBuilder
        {
            private IXmlLineInfo lineInfo;
            private TextBlockType textType;
            private string text;
            private int lineNum;
            private int linePos;
            public void Initialize(IXmlLineInfo lineInfo)
            {
                this.lineInfo = lineInfo;
                this.textType = TextBlockType.None;
            }

            public TextBlockType TextType
            {
                get
                {
                    return this.textType;
                }
            }
            public bool HasText
            {
                get
                {
                    return (this.textType != TextBlockType.None);
                }
            }
            public int LineNumber
            {
                get
                {
                    return this.lineNum;
                }
            }
            public int LinePosition
            {
                get
                {
                    return this.linePos;
                }
            }
            public void WriteTextBlock(string text, TextBlockType textType)
            {
                if (text.Length != 0)
                {
                    if (this.textType == TextBlockType.None)
                    {
                        this.text = text;
                        this.textType = textType;
                        if (this.lineInfo != null)
                        {
                            this.lineNum = this.lineInfo.LineNumber;
                            this.linePos = this.lineInfo.LinePosition;
                        }
                    }
                    else
                    {
                        this.text = this.text + text;
                        if (textType < this.textType)
                        {
                            this.textType = textType;
                        }
                    }
                }
            }

            public string ReadText()
            {
                if (this.textType == TextBlockType.None)
                {
                    return string.Empty;
                }
                this.textType = TextBlockType.None;
                return this.text;
            }
        }
    }
}

