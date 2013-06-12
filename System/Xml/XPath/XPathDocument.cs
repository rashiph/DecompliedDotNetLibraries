namespace System.Xml.XPath
{
    using MS.Internal.Xml.Cache;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml;

    public class XPathDocument : IXPathNavigable
    {
        private bool hasLineInfo;
        private Dictionary<string, XPathNodeRef> idValueMap;
        private int idxRoot;
        private int idxText;
        private int idxXmlNmsp;
        private Dictionary<XPathNodeRef, XPathNodeRef> mapNmsp;
        private XmlNameTable nameTable;
        private XPathNode[] pageRoot;
        private XPathNode[] pageText;
        private XPathNode[] pageXmlNmsp;

        internal XPathDocument()
        {
            this.nameTable = new System.Xml.NameTable();
        }

        public XPathDocument(Stream stream)
        {
            XmlTextReaderImpl reader = this.SetupReader(new XmlTextReaderImpl(string.Empty, stream));
            try
            {
                this.LoadFromReader(reader, XmlSpace.Default);
            }
            finally
            {
                reader.Close();
            }
        }

        public XPathDocument(TextReader textReader)
        {
            XmlTextReaderImpl reader = this.SetupReader(new XmlTextReaderImpl(string.Empty, textReader));
            try
            {
                this.LoadFromReader(reader, XmlSpace.Default);
            }
            finally
            {
                reader.Close();
            }
        }

        public XPathDocument(string uri) : this(uri, XmlSpace.Default)
        {
        }

        internal XPathDocument(XmlNameTable nameTable)
        {
            if (nameTable == null)
            {
                throw new ArgumentNullException("nameTable");
            }
            this.nameTable = nameTable;
        }

        public XPathDocument(XmlReader reader) : this(reader, XmlSpace.Default)
        {
        }

        public XPathDocument(string uri, XmlSpace space)
        {
            XmlTextReaderImpl reader = this.SetupReader(new XmlTextReaderImpl(uri));
            try
            {
                this.LoadFromReader(reader, space);
            }
            finally
            {
                reader.Close();
            }
        }

        public XPathDocument(XmlReader reader, XmlSpace space)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            this.LoadFromReader(reader, space);
        }

        internal void AddIdElement(string id, XPathNode[] pageElem, int idxElem)
        {
            if (this.idValueMap == null)
            {
                this.idValueMap = new Dictionary<string, XPathNodeRef>();
            }
            if (!this.idValueMap.ContainsKey(id))
            {
                this.idValueMap.Add(id, new XPathNodeRef(pageElem, idxElem));
            }
        }

        internal void AddNamespace(XPathNode[] pageElem, int idxElem, XPathNode[] pageNmsp, int idxNmsp)
        {
            if (this.mapNmsp == null)
            {
                this.mapNmsp = new Dictionary<XPathNodeRef, XPathNodeRef>();
            }
            this.mapNmsp.Add(new XPathNodeRef(pageElem, idxElem), new XPathNodeRef(pageNmsp, idxNmsp));
        }

        public XPathNavigator CreateNavigator()
        {
            return new XPathDocumentNavigator(this.pageRoot, this.idxRoot, null, 0);
        }

        internal int GetCollapsedTextNode(out XPathNode[] pageText)
        {
            pageText = this.pageText;
            return this.idxText;
        }

        internal int GetRootNode(out XPathNode[] pageRoot)
        {
            pageRoot = this.pageRoot;
            return this.idxRoot;
        }

        internal int GetXmlNamespaceNode(out XPathNode[] pageXmlNmsp)
        {
            pageXmlNmsp = this.pageXmlNmsp;
            return this.idxXmlNmsp;
        }

        internal void LoadFromReader(XmlReader reader, XmlSpace space)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            IXmlLineInfo lineInfo = reader as IXmlLineInfo;
            if ((lineInfo == null) || !lineInfo.HasLineInfo())
            {
                lineInfo = null;
            }
            this.hasLineInfo = lineInfo != null;
            this.nameTable = reader.NameTable;
            XPathDocumentBuilder builder = new XPathDocumentBuilder(this, lineInfo, reader.BaseURI, LoadFlags.None);
            try
            {
                bool isEmptyElement;
                string str2;
                bool flag = reader.ReadState == ReadState.Initial;
                int depth = reader.Depth;
                string str = this.nameTable.Get("http://www.w3.org/2000/xmlns/");
                if (flag && !reader.Read())
                {
                    return;
                }
            Label_007D:
                if (!flag && (reader.Depth < depth))
                {
                    return;
                }
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        isEmptyElement = reader.IsEmptyElement;
                        builder.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI, reader.BaseURI);
                        goto Label_017B;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        builder.WriteString(reader.Value, TextBlockType.Text);
                        goto Label_022B;

                    case XmlNodeType.EntityReference:
                        reader.ResolveEntity();
                        goto Label_022B;

                    case XmlNodeType.ProcessingInstruction:
                        builder.WriteProcessingInstruction(reader.LocalName, reader.Value, reader.BaseURI);
                        goto Label_022B;

                    case XmlNodeType.Comment:
                        builder.WriteComment(reader.Value);
                        goto Label_022B;

                    case XmlNodeType.DocumentType:
                    {
                        IDtdInfo dtdInfo = reader.DtdInfo;
                        if (dtdInfo != null)
                        {
                            builder.CreateIdTables(dtdInfo);
                        }
                        goto Label_022B;
                    }
                    case XmlNodeType.Whitespace:
                        goto Label_01C9;

                    case XmlNodeType.SignificantWhitespace:
                        if (reader.XmlSpace != XmlSpace.Preserve)
                        {
                            goto Label_01C9;
                        }
                        builder.WriteString(reader.Value, TextBlockType.SignificantWhitespace);
                        goto Label_022B;

                    case XmlNodeType.EndElement:
                        builder.WriteEndElement(false);
                        goto Label_022B;

                    default:
                        goto Label_022B;
                }
            Label_0113:
                str2 = reader.NamespaceURI;
                if (str2 == str)
                {
                    if (reader.Prefix.Length == 0)
                    {
                        builder.WriteNamespaceDeclaration(string.Empty, reader.Value);
                    }
                    else
                    {
                        builder.WriteNamespaceDeclaration(reader.LocalName, reader.Value);
                    }
                }
                else
                {
                    builder.WriteStartAttribute(reader.Prefix, reader.LocalName, str2);
                    builder.WriteString(reader.Value, TextBlockType.Text);
                    builder.WriteEndAttribute();
                }
            Label_017B:
                if (reader.MoveToNextAttribute())
                {
                    goto Label_0113;
                }
                if (isEmptyElement)
                {
                    builder.WriteEndElement(true);
                }
                goto Label_022B;
            Label_01C9:
                if ((space == XmlSpace.Preserve) && (!flag || (reader.Depth != 0)))
                {
                    builder.WriteString(reader.Value, TextBlockType.Whitespace);
                }
            Label_022B:
                if (reader.Read())
                {
                    goto Label_007D;
                }
            }
            finally
            {
                builder.Close();
            }
        }

        internal XmlRawWriter LoadFromWriter(LoadFlags flags, string baseUri)
        {
            return new XPathDocumentBuilder(this, null, baseUri, flags);
        }

        internal int LookupIdElement(string id, out XPathNode[] pageElem)
        {
            if ((this.idValueMap == null) || !this.idValueMap.ContainsKey(id))
            {
                pageElem = null;
                return 0;
            }
            XPathNodeRef ref2 = this.idValueMap[id];
            pageElem = ref2.Page;
            return ref2.Index;
        }

        internal int LookupNamespaces(XPathNode[] pageElem, int idxElem, out XPathNode[] pageNmsp)
        {
            XPathNodeRef key = new XPathNodeRef(pageElem, idxElem);
            if ((this.mapNmsp == null) || !this.mapNmsp.ContainsKey(key))
            {
                pageNmsp = null;
                return 0;
            }
            key = this.mapNmsp[key];
            pageNmsp = key.Page;
            return key.Index;
        }

        internal void SetCollapsedTextNode(XPathNode[] pageText, int idxText)
        {
            this.pageText = pageText;
            this.idxText = idxText;
        }

        internal void SetRootNode(XPathNode[] pageRoot, int idxRoot)
        {
            this.pageRoot = pageRoot;
            this.idxRoot = idxRoot;
        }

        private XmlTextReaderImpl SetupReader(XmlTextReaderImpl reader)
        {
            reader.EntityHandling = EntityHandling.ExpandEntities;
            reader.XmlValidatingReaderCompatibilityMode = true;
            return reader;
        }

        internal void SetXmlNamespaceNode(XPathNode[] pageXmlNmsp, int idxXmlNmsp)
        {
            this.pageXmlNmsp = pageXmlNmsp;
            this.idxXmlNmsp = idxXmlNmsp;
        }

        internal bool HasLineInfo
        {
            get
            {
                return this.hasLineInfo;
            }
        }

        internal XmlNameTable NameTable
        {
            get
            {
                return this.nameTable;
            }
        }

        internal enum LoadFlags
        {
            None,
            AtomizeNames,
            Fragment
        }
    }
}

