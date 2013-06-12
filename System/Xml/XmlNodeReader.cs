namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Schema;

    public class XmlNodeReader : XmlReader, IXmlNamespaceResolver
    {
        private bool bInReadBinary;
        private bool bResolveEntity;
        private bool bStartFromDocument;
        private int curDepth;
        private bool fEOF;
        private XmlNodeType nodeType;
        private ReadContentAsBinaryHelper readBinaryHelper;
        private XmlNodeReaderNavigator readerNav;
        private System.Xml.ReadState readState;

        public XmlNodeReader(XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            this.readerNav = new XmlNodeReaderNavigator(node);
            this.curDepth = 0;
            this.readState = System.Xml.ReadState.Initial;
            this.fEOF = false;
            this.nodeType = XmlNodeType.None;
            this.bResolveEntity = false;
            this.bStartFromDocument = false;
        }

        public override void Close()
        {
            this.readState = System.Xml.ReadState.Closed;
        }

        private void FinishReadBinary()
        {
            this.bInReadBinary = false;
            this.readBinaryHelper.Finish();
        }

        public override string GetAttribute(int attributeIndex)
        {
            if (!this.IsInReadingStates())
            {
                throw new ArgumentOutOfRangeException("attributeIndex");
            }
            return this.readerNav.GetAttribute(attributeIndex);
        }

        public override string GetAttribute(string name)
        {
            if (!this.IsInReadingStates())
            {
                return null;
            }
            return this.readerNav.GetAttribute(name);
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            if (!this.IsInReadingStates())
            {
                return null;
            }
            string ns = (namespaceURI == null) ? string.Empty : namespaceURI;
            return this.readerNav.GetAttribute(name, ns);
        }

        internal bool IsInReadingStates()
        {
            return (this.readState == System.Xml.ReadState.Interactive);
        }

        public override string LookupNamespace(string prefix)
        {
            if (!this.IsInReadingStates())
            {
                return null;
            }
            string str = this.readerNav.LookupNamespace(prefix);
            if ((str != null) && (str.Length == 0))
            {
                return null;
            }
            return str;
        }

        public override void MoveToAttribute(int attributeIndex)
        {
            if (!this.IsInReadingStates())
            {
                throw new ArgumentOutOfRangeException("attributeIndex");
            }
            this.readerNav.ResetMove(ref this.curDepth, ref this.nodeType);
            try
            {
                if (this.AttributeCount <= 0)
                {
                    throw new ArgumentOutOfRangeException("attributeIndex");
                }
                this.readerNav.MoveToAttribute(attributeIndex);
                if (this.bInReadBinary)
                {
                    this.FinishReadBinary();
                }
            }
            catch
            {
                this.readerNav.RollBackMove(ref this.curDepth);
                throw;
            }
            this.curDepth++;
            this.nodeType = this.readerNav.NodeType;
        }

        public override bool MoveToAttribute(string name)
        {
            if (this.IsInReadingStates())
            {
                this.readerNav.ResetMove(ref this.curDepth, ref this.nodeType);
                if (this.readerNav.MoveToAttribute(name))
                {
                    this.curDepth++;
                    this.nodeType = this.readerNav.NodeType;
                    if (this.bInReadBinary)
                    {
                        this.FinishReadBinary();
                    }
                    return true;
                }
                this.readerNav.RollBackMove(ref this.curDepth);
            }
            return false;
        }

        public override bool MoveToAttribute(string name, string namespaceURI)
        {
            if (this.IsInReadingStates())
            {
                this.readerNav.ResetMove(ref this.curDepth, ref this.nodeType);
                string str = (namespaceURI == null) ? string.Empty : namespaceURI;
                if (this.readerNav.MoveToAttribute(name, str))
                {
                    this.curDepth++;
                    this.nodeType = this.readerNav.NodeType;
                    if (this.bInReadBinary)
                    {
                        this.FinishReadBinary();
                    }
                    return true;
                }
                this.readerNav.RollBackMove(ref this.curDepth);
            }
            return false;
        }

        public override bool MoveToElement()
        {
            if (this.IsInReadingStates())
            {
                this.readerNav.LogMove(this.curDepth);
                this.readerNav.ResetToAttribute(ref this.curDepth);
                if (this.readerNav.MoveToElement())
                {
                    this.curDepth--;
                    this.nodeType = this.readerNav.NodeType;
                    if (this.bInReadBinary)
                    {
                        this.FinishReadBinary();
                    }
                    return true;
                }
                this.readerNav.RollBackMove(ref this.curDepth);
            }
            return false;
        }

        public override bool MoveToFirstAttribute()
        {
            if (this.IsInReadingStates())
            {
                this.readerNav.ResetMove(ref this.curDepth, ref this.nodeType);
                if (this.AttributeCount > 0)
                {
                    this.readerNav.MoveToAttribute(0);
                    this.curDepth++;
                    this.nodeType = this.readerNav.NodeType;
                    if (this.bInReadBinary)
                    {
                        this.FinishReadBinary();
                    }
                    return true;
                }
                this.readerNav.RollBackMove(ref this.curDepth);
            }
            return false;
        }

        public override bool MoveToNextAttribute()
        {
            if (this.IsInReadingStates() && (this.nodeType != XmlNodeType.EndElement))
            {
                this.readerNav.LogMove(this.curDepth);
                this.readerNav.ResetToAttribute(ref this.curDepth);
                if (this.readerNav.MoveToNextAttribute(ref this.curDepth))
                {
                    this.nodeType = this.readerNav.NodeType;
                    if (this.bInReadBinary)
                    {
                        this.FinishReadBinary();
                    }
                    return true;
                }
                this.readerNav.RollBackMove(ref this.curDepth);
            }
            return false;
        }

        public override bool Read()
        {
            return this.Read(false);
        }

        private bool Read(bool fSkipChildren)
        {
            if (!this.fEOF)
            {
                if (this.readState == System.Xml.ReadState.Initial)
                {
                    if ((this.readerNav.NodeType == XmlNodeType.Document) || (this.readerNav.NodeType == XmlNodeType.DocumentFragment))
                    {
                        this.bStartFromDocument = true;
                        if (!this.ReadNextNode(fSkipChildren))
                        {
                            this.readState = System.Xml.ReadState.Error;
                            return false;
                        }
                    }
                    this.ReSetReadingMarks();
                    this.readState = System.Xml.ReadState.Interactive;
                    this.nodeType = this.readerNav.NodeType;
                    this.curDepth = 0;
                    return true;
                }
                if (this.bInReadBinary)
                {
                    this.FinishReadBinary();
                }
                if (!this.readerNav.CreatedOnAttribute)
                {
                    this.ReSetReadingMarks();
                    if (this.ReadNextNode(fSkipChildren))
                    {
                        return true;
                    }
                    if ((this.readState == System.Xml.ReadState.Initial) || (this.readState == System.Xml.ReadState.Interactive))
                    {
                        this.readState = System.Xml.ReadState.Error;
                    }
                    if (this.readState == System.Xml.ReadState.EndOfFile)
                    {
                        this.nodeType = XmlNodeType.None;
                    }
                }
            }
            return false;
        }

        public override bool ReadAttributeValue()
        {
            if (this.IsInReadingStates() && this.readerNav.ReadAttributeValue(ref this.curDepth, ref this.bResolveEntity, ref this.nodeType))
            {
                this.bInReadBinary = false;
                return true;
            }
            return false;
        }

        private bool ReadAtZeroLevel(bool fSkipChildren)
        {
            if ((!fSkipChildren && (this.nodeType != XmlNodeType.EndElement)) && ((this.readerNav.NodeType == XmlNodeType.Element) && !this.readerNav.IsEmptyElement))
            {
                this.nodeType = XmlNodeType.EndElement;
                return true;
            }
            this.SetEndOfFile();
            return false;
        }

        public override int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            if (this.readState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (!this.bInReadBinary)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
            }
            this.bInReadBinary = false;
            int num = this.readBinaryHelper.ReadContentAsBase64(buffer, index, count);
            this.bInReadBinary = true;
            return num;
        }

        public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            if (this.readState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (!this.bInReadBinary)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
            }
            this.bInReadBinary = false;
            int num = this.readBinaryHelper.ReadContentAsBinHex(buffer, index, count);
            this.bInReadBinary = true;
            return num;
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
        {
            if (this.readState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (!this.bInReadBinary)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
            }
            this.bInReadBinary = false;
            int num = this.readBinaryHelper.ReadElementContentAsBase64(buffer, index, count);
            this.bInReadBinary = true;
            return num;
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
        {
            if (this.readState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (!this.bInReadBinary)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
            }
            this.bInReadBinary = false;
            int num = this.readBinaryHelper.ReadElementContentAsBinHex(buffer, index, count);
            this.bInReadBinary = true;
            return num;
        }

        private bool ReadForward(bool fSkipChildren)
        {
            if (this.readState == System.Xml.ReadState.Error)
            {
                return false;
            }
            if (!this.bStartFromDocument && (this.curDepth == 0))
            {
                return this.ReadAtZeroLevel(fSkipChildren);
            }
            if (this.readerNav.MoveToNext())
            {
                this.nodeType = this.readerNav.NodeType;
                return true;
            }
            if (this.curDepth == 0)
            {
                return this.ReadAtZeroLevel(fSkipChildren);
            }
            if (!this.readerNav.MoveToParent())
            {
                return false;
            }
            if (this.readerNav.NodeType == XmlNodeType.Element)
            {
                this.curDepth--;
                this.nodeType = XmlNodeType.EndElement;
                return true;
            }
            if (this.readerNav.NodeType == XmlNodeType.EntityReference)
            {
                this.curDepth--;
                this.nodeType = XmlNodeType.EndEntity;
                return true;
            }
            return true;
        }

        private bool ReadNextNode(bool fSkipChildren)
        {
            if ((this.readState != System.Xml.ReadState.Interactive) && (this.readState != System.Xml.ReadState.Initial))
            {
                this.nodeType = XmlNodeType.None;
                return false;
            }
            bool flag = !fSkipChildren;
            XmlNodeType nodeType = this.readerNav.NodeType;
            if (((flag && (this.nodeType != XmlNodeType.EndElement)) && (this.nodeType != XmlNodeType.EndEntity)) && (((nodeType == XmlNodeType.Element) || ((nodeType == XmlNodeType.EntityReference) && this.bResolveEntity)) || (((this.readerNav.NodeType == XmlNodeType.Document) || (this.readerNav.NodeType == XmlNodeType.DocumentFragment)) && (this.readState == System.Xml.ReadState.Initial))))
            {
                if (this.readerNav.MoveToFirstChild())
                {
                    this.nodeType = this.readerNav.NodeType;
                    this.curDepth++;
                    if (this.bResolveEntity)
                    {
                        this.bResolveEntity = false;
                    }
                    return true;
                }
                if ((this.readerNav.NodeType == XmlNodeType.Element) && !this.readerNav.IsEmptyElement)
                {
                    this.nodeType = XmlNodeType.EndElement;
                    return true;
                }
                if ((this.readerNav.NodeType == XmlNodeType.EntityReference) && this.bResolveEntity)
                {
                    this.bResolveEntity = false;
                    this.nodeType = XmlNodeType.EndEntity;
                    return true;
                }
                return this.ReadForward(fSkipChildren);
            }
            if ((this.readerNav.NodeType != XmlNodeType.EntityReference) || !this.bResolveEntity)
            {
                return this.ReadForward(fSkipChildren);
            }
            if (this.readerNav.MoveToFirstChild())
            {
                this.nodeType = this.readerNav.NodeType;
                this.curDepth++;
            }
            else
            {
                this.nodeType = XmlNodeType.EndEntity;
            }
            this.bResolveEntity = false;
            return true;
        }

        public override string ReadString()
        {
            if (((this.NodeType == XmlNodeType.EntityReference) && this.bResolveEntity) && !this.Read())
            {
                throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
            }
            return base.ReadString();
        }

        private void ReSetReadingMarks()
        {
            this.readerNav.ResetMove(ref this.curDepth, ref this.nodeType);
        }

        public override void ResolveEntity()
        {
            if (!this.IsInReadingStates() || (this.nodeType != XmlNodeType.EntityReference))
            {
                throw new InvalidOperationException(Res.GetString("Xnr_ResolveEntity"));
            }
            this.bResolveEntity = true;
        }

        private void SetEndOfFile()
        {
            this.fEOF = true;
            this.readState = System.Xml.ReadState.EndOfFile;
            this.nodeType = XmlNodeType.None;
        }

        public override void Skip()
        {
            this.Read(true);
        }

        IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
        {
            return this.readerNav.GetNamespacesInScope(scope);
        }

        string IXmlNamespaceResolver.LookupNamespace(string prefix)
        {
            if (!this.IsInReadingStates())
            {
                return this.readerNav.DefaultLookupNamespace(prefix);
            }
            string array = this.readerNav.LookupNamespace(prefix);
            if (array != null)
            {
                array = this.readerNav.NameTable.Add(array);
            }
            return array;
        }

        string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
        {
            return this.readerNav.LookupPrefix(namespaceName);
        }

        public override int AttributeCount
        {
            get
            {
                if (this.IsInReadingStates() && (this.nodeType != XmlNodeType.EndElement))
                {
                    return this.readerNav.AttributeCount;
                }
                return 0;
            }
        }

        public override string BaseURI
        {
            get
            {
                return this.readerNav.BaseURI;
            }
        }

        public override bool CanReadBinaryContent
        {
            get
            {
                return true;
            }
        }

        public override bool CanResolveEntity
        {
            get
            {
                return true;
            }
        }

        public override int Depth
        {
            get
            {
                return this.curDepth;
            }
        }

        internal override IDtdInfo DtdInfo
        {
            get
            {
                return this.readerNav.Document.DtdSchemaInfo;
            }
        }

        public override bool EOF
        {
            get
            {
                return ((this.readState != System.Xml.ReadState.Closed) && this.fEOF);
            }
        }

        public override bool HasAttributes
        {
            get
            {
                return (this.AttributeCount > 0);
            }
        }

        public override bool HasValue
        {
            get
            {
                if (!this.IsInReadingStates())
                {
                    return false;
                }
                return this.readerNav.HasValue;
            }
        }

        public override bool IsDefault
        {
            get
            {
                if (!this.IsInReadingStates())
                {
                    return false;
                }
                return this.readerNav.IsDefault;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                if (!this.IsInReadingStates())
                {
                    return false;
                }
                return this.readerNav.IsEmptyElement;
            }
        }

        public override string LocalName
        {
            get
            {
                if (!this.IsInReadingStates())
                {
                    return string.Empty;
                }
                return this.readerNav.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                if (!this.IsInReadingStates())
                {
                    return string.Empty;
                }
                return this.readerNav.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                if (!this.IsInReadingStates())
                {
                    return string.Empty;
                }
                return this.readerNav.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.readerNav.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                if (!this.IsInReadingStates())
                {
                    return XmlNodeType.None;
                }
                return this.nodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                if (!this.IsInReadingStates())
                {
                    return string.Empty;
                }
                return this.readerNav.Prefix;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                return this.readState;
            }
        }

        public override IXmlSchemaInfo SchemaInfo
        {
            get
            {
                if (!this.IsInReadingStates())
                {
                    return null;
                }
                return this.readerNav.SchemaInfo;
            }
        }

        public override string Value
        {
            get
            {
                if (!this.IsInReadingStates())
                {
                    return string.Empty;
                }
                return this.readerNav.Value;
            }
        }

        public override string XmlLang
        {
            get
            {
                if (!this.IsInReadingStates())
                {
                    return string.Empty;
                }
                return this.readerNav.XmlLang;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                if (!this.IsInReadingStates())
                {
                    return System.Xml.XmlSpace.None;
                }
                return this.readerNav.XmlSpace;
            }
        }
    }
}

