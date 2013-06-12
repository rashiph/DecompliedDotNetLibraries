namespace System.Xml
{
    using System;
    using System.Reflection;

    internal class XsdCachingReader : XmlReader, IXmlLineInfo
    {
        private int attributeCount;
        private ValidatingReaderNodeData[] attributeEvents;
        private ValidatingReaderNodeData cachedNode;
        private CachingEventHandler cacheHandler;
        private CachingReaderState cacheState;
        private ValidatingReaderNodeData[] contentEvents;
        private int contentIndex;
        private XmlReader coreReader;
        private XmlNameTable coreReaderNameTable;
        private int currentAttrIndex;
        private int currentContentIndex;
        private const int InitialAttributeCount = 8;
        private const int InitialContentCount = 4;
        private IXmlLineInfo lineInfo;
        private bool readAhead;
        private bool returnOriginalStringValues;
        private ValidatingReaderNodeData textNode;

        internal XsdCachingReader(XmlReader reader, IXmlLineInfo lineInfo, CachingEventHandler handlerMethod)
        {
            this.coreReader = reader;
            this.lineInfo = lineInfo;
            this.cacheHandler = handlerMethod;
            this.attributeEvents = new ValidatingReaderNodeData[8];
            this.contentEvents = new ValidatingReaderNodeData[4];
            this.Init();
        }

        private ValidatingReaderNodeData AddAttribute(int attIndex)
        {
            ValidatingReaderNodeData data = this.attributeEvents[attIndex];
            if (data != null)
            {
                data.Clear(XmlNodeType.Attribute);
                return data;
            }
            if (attIndex >= (this.attributeEvents.Length - 1))
            {
                ValidatingReaderNodeData[] destinationArray = new ValidatingReaderNodeData[this.attributeEvents.Length * 2];
                Array.Copy(this.attributeEvents, 0, destinationArray, 0, this.attributeEvents.Length);
                this.attributeEvents = destinationArray;
            }
            data = this.attributeEvents[attIndex];
            if (data == null)
            {
                data = new ValidatingReaderNodeData(XmlNodeType.Attribute);
                this.attributeEvents[attIndex] = data;
            }
            return data;
        }

        private ValidatingReaderNodeData AddContent(XmlNodeType nodeType)
        {
            ValidatingReaderNodeData data = this.contentEvents[this.contentIndex];
            if (data != null)
            {
                data.Clear(nodeType);
                this.contentIndex++;
                return data;
            }
            if (this.contentIndex >= (this.contentEvents.Length - 1))
            {
                ValidatingReaderNodeData[] destinationArray = new ValidatingReaderNodeData[this.contentEvents.Length * 2];
                Array.Copy(this.contentEvents, 0, destinationArray, 0, this.contentEvents.Length);
                this.contentEvents = destinationArray;
            }
            data = this.contentEvents[this.contentIndex];
            if (data == null)
            {
                data = new ValidatingReaderNodeData(nodeType);
                this.contentEvents[this.contentIndex] = data;
            }
            this.contentIndex++;
            return data;
        }

        private void ClearAttributesInfo()
        {
            this.attributeCount = 0;
            this.currentAttrIndex = -1;
        }

        public override void Close()
        {
            this.coreReader.Close();
            this.cacheState = CachingReaderState.ReaderClosed;
        }

        private ValidatingReaderNodeData CreateDummyTextNode(string attributeValue, int depth)
        {
            if (this.textNode == null)
            {
                this.textNode = new ValidatingReaderNodeData(XmlNodeType.Text);
            }
            this.textNode.Depth = depth;
            this.textNode.RawValue = attributeValue;
            return this.textNode;
        }

        public override string GetAttribute(int i)
        {
            if ((i < 0) || (i >= this.attributeCount))
            {
                throw new ArgumentOutOfRangeException("i");
            }
            return this.attributeEvents[i].RawValue;
        }

        public override string GetAttribute(string name)
        {
            int attributeIndexWithoutPrefix;
            if (name.IndexOf(':') == -1)
            {
                attributeIndexWithoutPrefix = this.GetAttributeIndexWithoutPrefix(name);
            }
            else
            {
                attributeIndexWithoutPrefix = this.GetAttributeIndexWithPrefix(name);
            }
            if (attributeIndexWithoutPrefix < 0)
            {
                return null;
            }
            return this.attributeEvents[attributeIndexWithoutPrefix].RawValue;
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            namespaceURI = (namespaceURI == null) ? string.Empty : this.coreReaderNameTable.Get(namespaceURI);
            name = this.coreReaderNameTable.Get(name);
            for (int i = 0; i < this.attributeCount; i++)
            {
                ValidatingReaderNodeData data = this.attributeEvents[i];
                if (Ref.Equal(data.LocalName, name) && Ref.Equal(data.Namespace, namespaceURI))
                {
                    return data.RawValue;
                }
            }
            return null;
        }

        private int GetAttributeIndexWithoutPrefix(string name)
        {
            name = this.coreReaderNameTable.Get(name);
            if (name != null)
            {
                for (int i = 0; i < this.attributeCount; i++)
                {
                    ValidatingReaderNodeData data = this.attributeEvents[i];
                    if (Ref.Equal(data.LocalName, name) && (data.Prefix.Length == 0))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private int GetAttributeIndexWithPrefix(string name)
        {
            name = this.coreReaderNameTable.Get(name);
            if (name != null)
            {
                for (int i = 0; i < this.attributeCount; i++)
                {
                    ValidatingReaderNodeData data = this.attributeEvents[i];
                    if (Ref.Equal(data.GetAtomizedNameWPrefix(this.coreReaderNameTable), name))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        internal XmlReader GetCoreReader()
        {
            return this.coreReader;
        }

        internal IXmlLineInfo GetLineInfo()
        {
            return this.lineInfo;
        }

        private void Init()
        {
            this.coreReaderNameTable = this.coreReader.NameTable;
            this.cacheState = CachingReaderState.Init;
            this.contentIndex = 0;
            this.currentAttrIndex = -1;
            this.currentContentIndex = -1;
            this.attributeCount = 0;
            this.cachedNode = null;
            this.readAhead = false;
            if (this.coreReader.NodeType == XmlNodeType.Element)
            {
                ValidatingReaderNodeData data = this.AddContent(this.coreReader.NodeType);
                data.SetItemData(this.coreReader.LocalName, this.coreReader.Prefix, this.coreReader.NamespaceURI, this.coreReader.Depth);
                data.SetLineInfo(this.lineInfo);
                this.RecordAttributes();
            }
        }

        public override string LookupNamespace(string prefix)
        {
            return this.coreReader.LookupNamespace(prefix);
        }

        public override void MoveToAttribute(int i)
        {
            if ((i < 0) || (i >= this.attributeCount))
            {
                throw new ArgumentOutOfRangeException("i");
            }
            this.currentAttrIndex = i;
            this.cachedNode = this.attributeEvents[i];
        }

        public override bool MoveToAttribute(string name)
        {
            int attributeIndexWithoutPrefix;
            if (name.IndexOf(':') == -1)
            {
                attributeIndexWithoutPrefix = this.GetAttributeIndexWithoutPrefix(name);
            }
            else
            {
                attributeIndexWithoutPrefix = this.GetAttributeIndexWithPrefix(name);
            }
            if (attributeIndexWithoutPrefix >= 0)
            {
                this.currentAttrIndex = attributeIndexWithoutPrefix;
                this.cachedNode = this.attributeEvents[attributeIndexWithoutPrefix];
                return true;
            }
            return false;
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            ns = (ns == null) ? string.Empty : this.coreReaderNameTable.Get(ns);
            name = this.coreReaderNameTable.Get(name);
            for (int i = 0; i < this.attributeCount; i++)
            {
                ValidatingReaderNodeData data = this.attributeEvents[i];
                if (Ref.Equal(data.LocalName, name) && Ref.Equal(data.Namespace, ns))
                {
                    this.currentAttrIndex = i;
                    this.cachedNode = this.attributeEvents[i];
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToElement()
        {
            if ((this.cacheState != CachingReaderState.Replay) || (this.cachedNode.NodeType != XmlNodeType.Attribute))
            {
                return false;
            }
            this.currentContentIndex = 0;
            this.currentAttrIndex = -1;
            this.Read();
            return true;
        }

        public override bool MoveToFirstAttribute()
        {
            if (this.attributeCount == 0)
            {
                return false;
            }
            this.currentAttrIndex = 0;
            this.cachedNode = this.attributeEvents[0];
            return true;
        }

        public override bool MoveToNextAttribute()
        {
            if ((this.currentAttrIndex + 1) < this.attributeCount)
            {
                this.cachedNode = this.attributeEvents[++this.currentAttrIndex];
                return true;
            }
            return false;
        }

        public override bool Read()
        {
            switch (this.cacheState)
            {
                case CachingReaderState.Init:
                    this.cacheState = CachingReaderState.Record;
                    break;

                case CachingReaderState.Record:
                    break;

                case CachingReaderState.Replay:
                    if (this.currentContentIndex < this.contentIndex)
                    {
                        this.cachedNode = this.contentEvents[this.currentContentIndex];
                        if (this.currentContentIndex > 0)
                        {
                            this.ClearAttributesInfo();
                        }
                        this.currentContentIndex++;
                        return true;
                    }
                    this.cacheState = CachingReaderState.ReaderClosed;
                    this.cacheHandler(this);
                    return (((this.coreReader.NodeType == XmlNodeType.Element) && !this.readAhead) || this.coreReader.Read());

                default:
                    return false;
            }
            ValidatingReaderNodeData data = null;
            if (!this.coreReader.Read())
            {
                this.cacheState = CachingReaderState.ReaderClosed;
                return false;
            }
            switch (this.coreReader.NodeType)
            {
                case XmlNodeType.Element:
                    this.cacheState = CachingReaderState.ReaderClosed;
                    return false;

                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    data = this.AddContent(this.coreReader.NodeType);
                    data.SetItemData(this.coreReader.Value);
                    data.SetLineInfo(this.lineInfo);
                    data.Depth = this.coreReader.Depth;
                    break;

                case XmlNodeType.EndElement:
                    data = this.AddContent(this.coreReader.NodeType);
                    data.SetItemData(this.coreReader.LocalName, this.coreReader.Prefix, this.coreReader.NamespaceURI, this.coreReader.Depth);
                    data.SetLineInfo(this.lineInfo);
                    break;
            }
            this.cachedNode = data;
            return true;
        }

        public override bool ReadAttributeValue()
        {
            if (this.cachedNode.NodeType != XmlNodeType.Attribute)
            {
                return false;
            }
            this.cachedNode = this.CreateDummyTextNode(this.cachedNode.RawValue, this.cachedNode.Depth + 1);
            return true;
        }

        internal string ReadOriginalContentAsString()
        {
            this.returnOriginalStringValues = true;
            string str = base.InternalReadContentAsString();
            this.returnOriginalStringValues = false;
            return str;
        }

        private void RecordAttributes()
        {
            this.attributeCount = this.coreReader.AttributeCount;
            if (this.coreReader.MoveToFirstAttribute())
            {
                int attIndex = 0;
                do
                {
                    ValidatingReaderNodeData data = this.AddAttribute(attIndex);
                    data.SetItemData(this.coreReader.LocalName, this.coreReader.Prefix, this.coreReader.NamespaceURI, this.coreReader.Depth);
                    data.SetLineInfo(this.lineInfo);
                    data.RawValue = this.coreReader.Value;
                    attIndex++;
                }
                while (this.coreReader.MoveToNextAttribute());
                this.coreReader.MoveToElement();
            }
        }

        internal void RecordEndElementNode()
        {
            ValidatingReaderNodeData data = this.AddContent(XmlNodeType.EndElement);
            data.SetItemData(this.coreReader.LocalName, this.coreReader.Prefix, this.coreReader.NamespaceURI, this.coreReader.Depth);
            data.SetLineInfo(this.coreReader as IXmlLineInfo);
            if (this.coreReader.IsEmptyElement)
            {
                this.readAhead = true;
            }
        }

        internal ValidatingReaderNodeData RecordTextNode(string textValue, string originalStringValue, int depth, int lineNo, int linePos)
        {
            ValidatingReaderNodeData data = this.AddContent(XmlNodeType.Text);
            data.SetItemData(textValue, originalStringValue);
            data.SetLineInfo(lineNo, linePos);
            data.Depth = depth;
            return data;
        }

        internal void Reset(XmlReader reader)
        {
            this.coreReader = reader;
            this.Init();
        }

        public override void ResolveEntity()
        {
            throw new InvalidOperationException();
        }

        internal void SetToReplayMode()
        {
            this.cacheState = CachingReaderState.Replay;
            this.currentContentIndex = 0;
            this.currentAttrIndex = -1;
            this.Read();
        }

        public override void Skip()
        {
            switch (this.cachedNode.NodeType)
            {
                case XmlNodeType.Element:
                    break;

                case XmlNodeType.Attribute:
                    this.MoveToElement();
                    break;

                default:
                    this.Read();
                    return;
            }
            if ((this.coreReader.NodeType != XmlNodeType.EndElement) && !this.readAhead)
            {
                int num = this.coreReader.Depth - 1;
                while (this.coreReader.Read() && (this.coreReader.Depth > num))
                {
                }
            }
            this.coreReader.Read();
            this.cacheState = CachingReaderState.ReaderClosed;
            this.cacheHandler(this);
        }

        internal void SwitchTextNodeAndEndElement(string textValue, string originalStringValue)
        {
            ValidatingReaderNodeData data = this.RecordTextNode(textValue, originalStringValue, this.coreReader.Depth + 1, 0, 0);
            int index = this.contentIndex - 2;
            ValidatingReaderNodeData data2 = this.contentEvents[index];
            this.contentEvents[index] = data;
            this.contentEvents[this.contentIndex - 1] = data2;
        }

        bool IXmlLineInfo.HasLineInfo()
        {
            return true;
        }

        public override int AttributeCount
        {
            get
            {
                return this.attributeCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                return this.coreReader.BaseURI;
            }
        }

        public override int Depth
        {
            get
            {
                return this.cachedNode.Depth;
            }
        }

        public override bool EOF
        {
            get
            {
                return ((this.cacheState == CachingReaderState.ReaderClosed) && this.coreReader.EOF);
            }
        }

        public override bool HasValue
        {
            get
            {
                return XmlReader.HasValueInternal(this.cachedNode.NodeType);
            }
        }

        public override bool IsDefault
        {
            get
            {
                return false;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return false;
            }
        }

        public override string this[int i]
        {
            get
            {
                return this.GetAttribute(i);
            }
        }

        public override string this[string name]
        {
            get
            {
                return this.GetAttribute(name);
            }
        }

        public override string this[string name, string namespaceURI]
        {
            get
            {
                return this.GetAttribute(name, namespaceURI);
            }
        }

        public override string LocalName
        {
            get
            {
                return this.cachedNode.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this.cachedNode.GetAtomizedNameWPrefix(this.coreReaderNameTable);
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.cachedNode.Namespace;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.coreReaderNameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this.cachedNode.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.cachedNode.Prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return this.coreReader.QuoteChar;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                return this.coreReader.ReadState;
            }
        }

        public override XmlReaderSettings Settings
        {
            get
            {
                return this.coreReader.Settings;
            }
        }

        int IXmlLineInfo.LineNumber
        {
            get
            {
                return this.cachedNode.LineNumber;
            }
        }

        int IXmlLineInfo.LinePosition
        {
            get
            {
                return this.cachedNode.LinePosition;
            }
        }

        public override string Value
        {
            get
            {
                if (!this.returnOriginalStringValues)
                {
                    return this.cachedNode.RawValue;
                }
                return this.cachedNode.OriginalStringValue;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.coreReader.XmlLang;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this.coreReader.XmlSpace;
            }
        }

        private enum CachingReaderState
        {
            None,
            Init,
            Record,
            Replay,
            ReaderClosed,
            Error
        }
    }
}

