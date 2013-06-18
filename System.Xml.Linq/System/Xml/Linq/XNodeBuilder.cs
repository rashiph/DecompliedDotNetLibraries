namespace System.Xml.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    internal class XNodeBuilder : XmlWriter
    {
        private XName attrName;
        private string attrValue;
        private List<object> content;
        private XContainer parent;
        private XContainer root;

        public XNodeBuilder(XContainer container)
        {
            this.root = container;
        }

        private void Add(object o)
        {
            if (this.content == null)
            {
                this.content = new List<object>();
            }
            this.content.Add(o);
        }

        private void AddNode(XNode n)
        {
            if (this.parent != null)
            {
                this.parent.Add(n);
            }
            else
            {
                this.Add(n);
            }
            XContainer container = n as XContainer;
            if (container != null)
            {
                this.parent = container;
            }
        }

        private void AddString(string s)
        {
            if (s != null)
            {
                if (this.attrValue != null)
                {
                    this.attrValue = this.attrValue + s;
                }
                else if (this.parent != null)
                {
                    this.parent.Add(s);
                }
                else
                {
                    this.Add(s);
                }
            }
        }

        public override void Close()
        {
            this.root.Add(this.content);
        }

        public override void Flush()
        {
        }

        public override string LookupPrefix(string namespaceName)
        {
            throw new NotSupportedException();
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            throw new NotSupportedException(System.Xml.Linq.Res.GetString("NotSupported_WriteBase64"));
        }

        public override void WriteCData(string text)
        {
            this.AddNode(new XCData(text));
        }

        public override void WriteCharEntity(char ch)
        {
            this.AddString(new string(ch, 1));
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.AddString(new string(buffer, index, count));
        }

        public override void WriteComment(string text)
        {
            this.AddNode(new XComment(text));
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            this.AddNode(new XDocumentType(name, pubid, sysid, subset));
        }

        public override void WriteEndAttribute()
        {
            XAttribute content = new XAttribute(this.attrName, this.attrValue);
            this.attrName = null;
            this.attrValue = null;
            if (this.parent != null)
            {
                this.parent.Add(content);
            }
            else
            {
                this.Add(content);
            }
        }

        public override void WriteEndDocument()
        {
        }

        public override void WriteEndElement()
        {
            this.parent = ((XElement) this.parent).parent;
        }

        public override void WriteEntityRef(string name)
        {
            switch (name)
            {
                case "amp":
                    this.AddString("&");
                    return;

                case "apos":
                    this.AddString("'");
                    return;

                case "gt":
                    this.AddString(">");
                    return;

                case "lt":
                    this.AddString("<");
                    return;

                case "quot":
                    this.AddString("\"");
                    return;
            }
            throw new NotSupportedException(System.Xml.Linq.Res.GetString("NotSupported_WriteEntityRef"));
        }

        public override void WriteFullEndElement()
        {
            XElement parent = (XElement) this.parent;
            if (parent.IsEmpty)
            {
                parent.Add(string.Empty);
            }
            this.parent = parent.parent;
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            if (name != "xml")
            {
                this.AddNode(new XProcessingInstruction(name, text));
            }
        }

        public override void WriteRaw(string data)
        {
            this.AddString(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this.AddString(new string(buffer, index, count));
        }

        public override void WriteStartAttribute(string prefix, string localName, string namespaceName)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }
            this.attrName = XNamespace.Get((prefix.Length == 0) ? string.Empty : namespaceName).GetName(localName);
            this.attrValue = string.Empty;
        }

        public override void WriteStartDocument()
        {
        }

        public override void WriteStartDocument(bool standalone)
        {
        }

        public override void WriteStartElement(string prefix, string localName, string namespaceName)
        {
            this.AddNode(new XElement(XNamespace.Get(namespaceName).GetName(localName)));
        }

        public override void WriteString(string text)
        {
            this.AddString(text);
        }

        public override void WriteSurrogateCharEntity(char lowCh, char highCh)
        {
            this.AddString(new string(new char[] { highCh, lowCh }));
        }

        public override void WriteWhitespace(string ws)
        {
            this.AddString(ws);
        }

        public override XmlWriterSettings Settings
        {
            get
            {
                return new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Auto };
            }
        }

        public override System.Xml.WriteState WriteState
        {
            get
            {
                throw new NotSupportedException();
            }
        }
    }
}

