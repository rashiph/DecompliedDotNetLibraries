namespace System.Transactions.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.XPath;

    internal class PlainXmlWriter : XmlWriter
    {
        private string currentAttributeName;
        private string currentAttributeNs;
        private string currentAttributePrefix;
        private bool format;
        private TraceXPathNavigator navigator;
        private Stack<string> stack;
        private bool writingAttribute;

        public PlainXmlWriter() : this(false)
        {
        }

        public PlainXmlWriter(bool format)
        {
            this.navigator = new TraceXPathNavigator();
            this.stack = new Stack<string>();
            this.format = format;
        }

        public override void Close()
        {
        }

        public override void Flush()
        {
        }

        public override string LookupPrefix(string ns)
        {
            throw new NotSupportedException();
        }

        public XPathNavigator ToNavigator()
        {
            return this.navigator;
        }

        public override void WriteBase64(byte[] buffer, int offset, int count)
        {
        }

        public override void WriteBinHex(byte[] buffer, int index, int count)
        {
            throw new NotSupportedException();
        }

        public override void WriteCData(string text)
        {
            throw new NotSupportedException();
        }

        public override void WriteCharEntity(char ch)
        {
            throw new NotSupportedException();
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            throw new NotSupportedException();
        }

        public override void WriteComment(string text)
        {
            throw new NotSupportedException();
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
        }

        public override void WriteEndAttribute()
        {
            this.writingAttribute = false;
        }

        public override void WriteEndDocument()
        {
            throw new NotSupportedException();
        }

        public override void WriteEndElement()
        {
            this.navigator.CloseElement();
        }

        public override void WriteEntityRef(string name)
        {
            throw new NotSupportedException();
        }

        public override void WriteFullEndElement()
        {
            this.WriteEndElement();
        }

        public override void WriteName(string name)
        {
            throw new NotSupportedException();
        }

        public override void WriteNmToken(string name)
        {
            throw new NotSupportedException();
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            throw new NotSupportedException();
        }

        public override void WriteQualifiedName(string localName, string ns)
        {
            throw new NotSupportedException();
        }

        public override void WriteRaw(string data)
        {
            throw new NotSupportedException();
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            throw new NotSupportedException();
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            this.currentAttributeName = localName;
            this.currentAttributePrefix = prefix;
            this.currentAttributeNs = ns;
            this.writingAttribute = true;
        }

        public override void WriteStartDocument()
        {
        }

        public override void WriteStartDocument(bool standalone)
        {
            throw new NotSupportedException();
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.navigator.AddElement(prefix, localName, ns);
        }

        public override void WriteString(string text)
        {
            if (this.writingAttribute)
            {
                this.navigator.AddAttribute(this.currentAttributeName, text, this.currentAttributeNs, this.currentAttributePrefix);
            }
            else
            {
                this.WriteValue(text);
            }
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            throw new NotSupportedException();
        }

        public override void WriteValue(object value)
        {
            this.navigator.AddText(value.ToString());
        }

        public override void WriteValue(string value)
        {
            this.navigator.AddText(value);
        }

        public override void WriteWhitespace(string ws)
        {
            throw new NotSupportedException();
        }

        public override System.Xml.WriteState WriteState
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override string XmlLang
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                throw new NotSupportedException();
            }
        }
    }
}

