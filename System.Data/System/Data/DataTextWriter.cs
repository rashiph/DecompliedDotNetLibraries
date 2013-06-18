namespace System.Data
{
    using System;
    using System.IO;
    using System.Xml;

    internal sealed class DataTextWriter : XmlWriter
    {
        private XmlWriter _xmltextWriter;

        private DataTextWriter(XmlWriter w)
        {
            this._xmltextWriter = w;
        }

        public override void Close()
        {
            this._xmltextWriter.Close();
        }

        internal static XmlWriter CreateWriter(XmlWriter xw)
        {
            return new DataTextWriter(xw);
        }

        public override void Flush()
        {
            this._xmltextWriter.Flush();
        }

        public override string LookupPrefix(string ns)
        {
            return this._xmltextWriter.LookupPrefix(ns);
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            this._xmltextWriter.WriteBase64(buffer, index, count);
        }

        public override void WriteBinHex(byte[] buffer, int index, int count)
        {
            this._xmltextWriter.WriteBinHex(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            this._xmltextWriter.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            this._xmltextWriter.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this._xmltextWriter.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            this._xmltextWriter.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            this._xmltextWriter.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteEndAttribute()
        {
            this._xmltextWriter.WriteEndAttribute();
        }

        public override void WriteEndDocument()
        {
            this._xmltextWriter.WriteEndDocument();
        }

        public override void WriteEndElement()
        {
            this._xmltextWriter.WriteEndElement();
        }

        public override void WriteEntityRef(string name)
        {
            this._xmltextWriter.WriteEntityRef(name);
        }

        public override void WriteFullEndElement()
        {
            this._xmltextWriter.WriteFullEndElement();
        }

        public override void WriteName(string name)
        {
            this._xmltextWriter.WriteName(name);
        }

        public override void WriteNmToken(string name)
        {
            this._xmltextWriter.WriteNmToken(name);
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this._xmltextWriter.WriteProcessingInstruction(name, text);
        }

        public override void WriteQualifiedName(string localName, string ns)
        {
            this._xmltextWriter.WriteQualifiedName(localName, ns);
        }

        public override void WriteRaw(string data)
        {
            this._xmltextWriter.WriteRaw(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this._xmltextWriter.WriteRaw(buffer, index, count);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            this._xmltextWriter.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartDocument()
        {
            this._xmltextWriter.WriteStartDocument();
        }

        public override void WriteStartDocument(bool standalone)
        {
            this._xmltextWriter.WriteStartDocument(standalone);
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this._xmltextWriter.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteString(string text)
        {
            this._xmltextWriter.WriteString(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            this._xmltextWriter.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            this._xmltextWriter.WriteWhitespace(ws);
        }

        internal Stream BaseStream
        {
            get
            {
                XmlTextWriter writer = this._xmltextWriter as XmlTextWriter;
                if (writer != null)
                {
                    return writer.BaseStream;
                }
                return null;
            }
        }

        public override System.Xml.WriteState WriteState
        {
            get
            {
                return this._xmltextWriter.WriteState;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this._xmltextWriter.XmlLang;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this._xmltextWriter.XmlSpace;
            }
        }
    }
}

