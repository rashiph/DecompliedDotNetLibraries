namespace System.Xml
{
    using System;
    using System.Collections.Generic;

    internal class QueryOutputWriter : XmlRawWriter
    {
        private BitStack bitsCData;
        private bool checkWellFormedDoc;
        private int depth;
        private bool hasDocElem;
        private bool inAttr;
        private bool inCDataSection;
        private Dictionary<XmlQualifiedName, int> lookupCDataElems;
        private bool outputDocType;
        private string publicId;
        private XmlQualifiedName qnameCData;
        private string systemId;
        private XmlRawWriter wrapped;

        public QueryOutputWriter(XmlRawWriter writer, XmlWriterSettings settings)
        {
            this.wrapped = writer;
            this.systemId = settings.DocTypeSystem;
            this.publicId = settings.DocTypePublic;
            if (settings.OutputMethod == XmlOutputMethod.Xml)
            {
                if (this.systemId != null)
                {
                    this.outputDocType = true;
                    this.checkWellFormedDoc = true;
                }
                if (settings.AutoXmlDeclaration && (settings.Standalone == XmlStandalone.Yes))
                {
                    this.checkWellFormedDoc = true;
                }
                if (settings.CDataSectionElements.Count > 0)
                {
                    this.bitsCData = new BitStack();
                    this.lookupCDataElems = new Dictionary<XmlQualifiedName, int>();
                    this.qnameCData = new XmlQualifiedName();
                    foreach (XmlQualifiedName name in settings.CDataSectionElements)
                    {
                        this.lookupCDataElems[name] = 0;
                    }
                    this.bitsCData.PushBit(false);
                }
            }
            else if ((settings.OutputMethod == XmlOutputMethod.Html) && ((this.systemId != null) || (this.publicId != null)))
            {
                this.outputDocType = true;
            }
        }

        public override void Close()
        {
            this.wrapped.Close();
            if (this.checkWellFormedDoc && !this.hasDocElem)
            {
                throw new XmlException("Xml_NoRoot", string.Empty);
            }
        }

        private void EndCDataSection()
        {
            this.inCDataSection = false;
        }

        public override void Flush()
        {
            this.wrapped.Flush();
        }

        private bool StartCDataSection()
        {
            if ((this.lookupCDataElems != null) && this.bitsCData.PeekBit())
            {
                this.inCDataSection = true;
                return true;
            }
            return false;
        }

        internal override void StartElementContent()
        {
            this.wrapped.StartElementContent();
        }

        public override void WriteCData(string text)
        {
            this.wrapped.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            this.EndCDataSection();
            this.wrapped.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            if (!this.inAttr && (this.inCDataSection || this.StartCDataSection()))
            {
                this.wrapped.WriteCData(new string(buffer, index, count));
            }
            else
            {
                this.wrapped.WriteChars(buffer, index, count);
            }
        }

        public override void WriteComment(string text)
        {
            this.EndCDataSection();
            this.wrapped.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            if ((this.publicId == null) && (this.systemId == null))
            {
                this.wrapped.WriteDocType(name, pubid, sysid, subset);
            }
        }

        public override void WriteEndAttribute()
        {
            this.inAttr = false;
            this.wrapped.WriteEndAttribute();
        }

        internal override void WriteEndElement(string prefix, string localName, string ns)
        {
            this.EndCDataSection();
            this.wrapped.WriteEndElement(prefix, localName, ns);
            if (this.checkWellFormedDoc)
            {
                this.depth--;
            }
            if (this.lookupCDataElems != null)
            {
                this.bitsCData.PopBit();
            }
        }

        internal override void WriteEndNamespaceDeclaration()
        {
            this.wrapped.WriteEndNamespaceDeclaration();
        }

        public override void WriteEntityRef(string name)
        {
            this.EndCDataSection();
            this.wrapped.WriteEntityRef(name);
        }

        internal override void WriteFullEndElement(string prefix, string localName, string ns)
        {
            this.EndCDataSection();
            this.wrapped.WriteFullEndElement(prefix, localName, ns);
            if (this.checkWellFormedDoc)
            {
                this.depth--;
            }
            if (this.lookupCDataElems != null)
            {
                this.bitsCData.PopBit();
            }
        }

        internal override void WriteNamespaceDeclaration(string prefix, string ns)
        {
            this.wrapped.WriteNamespaceDeclaration(prefix, ns);
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this.EndCDataSection();
            this.wrapped.WriteProcessingInstruction(name, text);
        }

        public override void WriteRaw(string data)
        {
            if (!this.inAttr && (this.inCDataSection || this.StartCDataSection()))
            {
                this.wrapped.WriteCData(data);
            }
            else
            {
                this.wrapped.WriteRaw(data);
            }
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            if (!this.inAttr && (this.inCDataSection || this.StartCDataSection()))
            {
                this.wrapped.WriteCData(new string(buffer, index, count));
            }
            else
            {
                this.wrapped.WriteRaw(buffer, index, count);
            }
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            this.inAttr = true;
            this.wrapped.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.EndCDataSection();
            if (this.checkWellFormedDoc)
            {
                if ((this.depth == 0) && this.hasDocElem)
                {
                    throw new XmlException("Xml_NoMultipleRoots", string.Empty);
                }
                this.depth++;
                this.hasDocElem = true;
            }
            if (this.outputDocType)
            {
                this.wrapped.WriteDocType((prefix.Length != 0) ? (prefix + ":" + localName) : localName, this.publicId, this.systemId, null);
                this.outputDocType = false;
            }
            this.wrapped.WriteStartElement(prefix, localName, ns);
            if (this.lookupCDataElems != null)
            {
                this.qnameCData.Init(localName, ns);
                this.bitsCData.PushBit(this.lookupCDataElems.ContainsKey(this.qnameCData));
            }
        }

        internal override void WriteStartNamespaceDeclaration(string prefix)
        {
            this.wrapped.WriteStartNamespaceDeclaration(prefix);
        }

        public override void WriteString(string text)
        {
            if (!this.inAttr && (this.inCDataSection || this.StartCDataSection()))
            {
                this.wrapped.WriteCData(text);
            }
            else
            {
                this.wrapped.WriteString(text);
            }
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            this.EndCDataSection();
            this.wrapped.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            if (!this.inAttr && (this.inCDataSection || this.StartCDataSection()))
            {
                this.wrapped.WriteCData(ws);
            }
            else
            {
                this.wrapped.WriteWhitespace(ws);
            }
        }

        internal override void WriteXmlDeclaration(string xmldecl)
        {
            this.wrapped.WriteXmlDeclaration(xmldecl);
        }

        internal override void WriteXmlDeclaration(XmlStandalone standalone)
        {
            this.wrapped.WriteXmlDeclaration(standalone);
        }

        internal override IXmlNamespaceResolver NamespaceResolver
        {
            get
            {
                return base.resolver;
            }
            set
            {
                base.resolver = value;
                this.wrapped.NamespaceResolver = value;
            }
        }

        public override XmlWriterSettings Settings
        {
            get
            {
                XmlWriterSettings settings = this.wrapped.Settings;
                settings.ReadOnly = false;
                settings.DocTypeSystem = this.systemId;
                settings.DocTypePublic = this.publicId;
                settings.ReadOnly = true;
                return settings;
            }
        }

        internal override bool SupportsNamespaceDeclarationInChunks
        {
            get
            {
                return this.wrapped.SupportsNamespaceDeclarationInChunks;
            }
        }
    }
}

