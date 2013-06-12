namespace System.Xml
{
    using System;
    using System.Collections.Generic;

    internal class QueryOutputWriterV1 : XmlWriter
    {
        private BitStack bitsCData;
        private bool inAttr;
        private bool inCDataSection;
        private Dictionary<XmlQualifiedName, XmlQualifiedName> lookupCDataElems;
        private bool outputDocType;
        private string publicId;
        private XmlQualifiedName qnameCData;
        private XmlStandalone standalone;
        private string systemId;
        private XmlWriter wrapped;

        public QueryOutputWriterV1(XmlWriter writer, XmlWriterSettings settings)
        {
            this.wrapped = writer;
            this.systemId = settings.DocTypeSystem;
            this.publicId = settings.DocTypePublic;
            if (settings.OutputMethod == XmlOutputMethod.Xml)
            {
                bool flag = false;
                if (this.systemId != null)
                {
                    flag = true;
                    this.outputDocType = true;
                }
                if (settings.Standalone == XmlStandalone.Yes)
                {
                    flag = true;
                    this.standalone = settings.Standalone;
                }
                if (flag)
                {
                    if (settings.Standalone == XmlStandalone.Yes)
                    {
                        this.wrapped.WriteStartDocument(true);
                    }
                    else
                    {
                        this.wrapped.WriteStartDocument();
                    }
                }
                if ((settings.CDataSectionElements != null) && (settings.CDataSectionElements.Count > 0))
                {
                    this.bitsCData = new BitStack();
                    this.lookupCDataElems = new Dictionary<XmlQualifiedName, XmlQualifiedName>();
                    this.qnameCData = new XmlQualifiedName();
                    foreach (XmlQualifiedName name in settings.CDataSectionElements)
                    {
                        this.lookupCDataElems[name] = null;
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
        }

        private void EndCDataSection()
        {
            this.inCDataSection = false;
        }

        public override void Flush()
        {
            this.wrapped.Flush();
        }

        public override string LookupPrefix(string ns)
        {
            return this.wrapped.LookupPrefix(ns);
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

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            if (!this.inAttr && (this.inCDataSection || this.StartCDataSection()))
            {
                this.wrapped.WriteBase64(buffer, index, count);
            }
            else
            {
                this.wrapped.WriteBase64(buffer, index, count);
            }
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

        public override void WriteEndDocument()
        {
            this.wrapped.WriteEndDocument();
        }

        public override void WriteEndElement()
        {
            this.EndCDataSection();
            this.wrapped.WriteEndElement();
            if (this.lookupCDataElems != null)
            {
                this.bitsCData.PopBit();
            }
        }

        public override void WriteEntityRef(string name)
        {
            this.EndCDataSection();
            this.wrapped.WriteEntityRef(name);
        }

        public override void WriteFullEndElement()
        {
            this.EndCDataSection();
            this.wrapped.WriteFullEndElement();
            if (this.lookupCDataElems != null)
            {
                this.bitsCData.PopBit();
            }
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

        public override void WriteStartDocument()
        {
            this.wrapped.WriteStartDocument();
        }

        public override void WriteStartDocument(bool standalone)
        {
            this.wrapped.WriteStartDocument(standalone);
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.EndCDataSection();
            if (this.outputDocType)
            {
                switch (this.wrapped.WriteState)
                {
                    case System.Xml.WriteState.Start:
                    case System.Xml.WriteState.Prolog:
                        this.wrapped.WriteDocType((prefix.Length != 0) ? (prefix + ":" + localName) : localName, this.publicId, this.systemId, null);
                        break;
                }
                this.outputDocType = false;
            }
            this.wrapped.WriteStartElement(prefix, localName, ns);
            if (this.lookupCDataElems != null)
            {
                this.qnameCData.Init(localName, ns);
                this.bitsCData.PushBit(this.lookupCDataElems.ContainsKey(this.qnameCData));
            }
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

        public override System.Xml.WriteState WriteState
        {
            get
            {
                return this.wrapped.WriteState;
            }
        }
    }
}

