namespace System.Runtime.Serialization
{
    using System;
    using System.Xml;

    internal class XmlSerializableWriter : XmlWriter
    {
        private int depth;
        private object obj;
        private XmlWriter xmlWriter;

        internal void BeginWrite(XmlWriter xmlWriter, object obj)
        {
            this.depth = 0;
            this.xmlWriter = xmlWriter;
            this.obj = obj;
        }

        public override void Close()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("IXmlSerializableIllegalOperation")));
        }

        internal void EndWrite()
        {
            if (this.depth != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("IXmlSerializableMissingEndElements", new object[] { (this.obj == null) ? string.Empty : DataContract.GetClrTypeFullName(this.obj.GetType()) })));
            }
            this.obj = null;
        }

        public override void Flush()
        {
            this.xmlWriter.Flush();
        }

        public override string LookupPrefix(string ns)
        {
            return this.xmlWriter.LookupPrefix(ns);
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            this.xmlWriter.WriteBase64(buffer, index, count);
        }

        public override void WriteBinHex(byte[] buffer, int index, int count)
        {
            this.xmlWriter.WriteBinHex(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            this.xmlWriter.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            this.xmlWriter.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.xmlWriter.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            this.xmlWriter.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
        }

        public override void WriteEndAttribute()
        {
            this.xmlWriter.WriteEndAttribute();
        }

        public override void WriteEndDocument()
        {
            this.xmlWriter.WriteEndDocument();
        }

        public override void WriteEndElement()
        {
            if (this.depth == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("IXmlSerializableWritePastSubTree", new object[] { (this.obj == null) ? string.Empty : DataContract.GetClrTypeFullName(this.obj.GetType()) })));
            }
            this.xmlWriter.WriteEndElement();
            this.depth--;
        }

        public override void WriteEntityRef(string name)
        {
            this.xmlWriter.WriteEntityRef(name);
        }

        public override void WriteFullEndElement()
        {
            if (this.depth == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("IXmlSerializableWritePastSubTree", new object[] { (this.obj == null) ? string.Empty : DataContract.GetClrTypeFullName(this.obj.GetType()) })));
            }
            this.xmlWriter.WriteFullEndElement();
            this.depth--;
        }

        public override void WriteName(string name)
        {
            this.xmlWriter.WriteName(name);
        }

        public override void WriteNmToken(string name)
        {
            this.xmlWriter.WriteNmToken(name);
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this.xmlWriter.WriteProcessingInstruction(name, text);
        }

        public override void WriteQualifiedName(string localName, string ns)
        {
            this.xmlWriter.WriteQualifiedName(localName, ns);
        }

        public override void WriteRaw(string data)
        {
            this.xmlWriter.WriteRaw(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this.xmlWriter.WriteRaw(buffer, index, count);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            this.xmlWriter.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartDocument()
        {
            if (this.WriteState == System.Xml.WriteState.Start)
            {
                this.xmlWriter.WriteStartDocument();
            }
        }

        public override void WriteStartDocument(bool standalone)
        {
            if (this.WriteState == System.Xml.WriteState.Start)
            {
                this.xmlWriter.WriteStartDocument(standalone);
            }
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.xmlWriter.WriteStartElement(prefix, localName, ns);
            this.depth++;
        }

        public override void WriteString(string text)
        {
            this.xmlWriter.WriteString(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            this.xmlWriter.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            this.xmlWriter.WriteWhitespace(ws);
        }

        public override System.Xml.WriteState WriteState
        {
            get
            {
                return this.xmlWriter.WriteState;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.xmlWriter.XmlLang;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this.xmlWriter.XmlSpace;
            }
        }
    }
}

