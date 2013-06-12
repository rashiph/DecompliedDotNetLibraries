namespace System.Data.SqlClient
{
    using System;
    using System.Data.Common;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal sealed class SqlStreamingXml
    {
        private long _charsRemoved;
        private int _columnOrdinal;
        private SqlDataReader _reader;
        private StringWriter _strWriter;
        private XmlReader _xmlReader;
        private XmlWriter _xmlWriter;

        public SqlStreamingXml(int i, SqlDataReader reader)
        {
            this._columnOrdinal = i;
            this._reader = reader;
        }

        public void Close()
        {
            this._xmlWriter.Dispose();
            this._xmlReader.Dispose();
            this._reader = null;
            this._xmlReader = null;
            this._xmlWriter = null;
            this._strWriter = null;
        }

        public long GetChars(long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            if (this._xmlReader == null)
            {
                this._xmlReader = new SqlStream(this._columnOrdinal, this._reader, true, false, false).ToXmlReader();
                this._strWriter = new StringWriter(null);
                XmlWriterSettings settings = new XmlWriterSettings {
                    CloseOutput = true,
                    ConformanceLevel = ConformanceLevel.Fragment
                };
                this._xmlWriter = XmlWriter.Create(this._strWriter, settings);
            }
            int num2 = 0;
            int num = 0;
            if (dataIndex < this._charsRemoved)
            {
                throw ADP.NonSeqByteAccess(dataIndex, this._charsRemoved, "GetChars");
            }
            if (dataIndex > this._charsRemoved)
            {
                num2 = (int) (dataIndex - this._charsRemoved);
            }
            if (buffer == null)
            {
                return -1L;
            }
            StringBuilder stringBuilder = this._strWriter.GetStringBuilder();
            while (!this._xmlReader.EOF)
            {
                if (stringBuilder.Length >= (length + num2))
                {
                    break;
                }
                this.WriteXmlElement();
                if (num2 > 0)
                {
                    num = (stringBuilder.Length < num2) ? stringBuilder.Length : num2;
                    stringBuilder.Remove(0, num);
                    num2 -= num;
                    this._charsRemoved += num;
                }
            }
            if (num2 > 0)
            {
                num = (stringBuilder.Length < num2) ? stringBuilder.Length : num2;
                stringBuilder.Remove(0, num);
                num2 -= num;
                this._charsRemoved += num;
            }
            if (stringBuilder.Length == 0)
            {
                return 0L;
            }
            num = (stringBuilder.Length < length) ? stringBuilder.Length : length;
            for (int i = 0; i < num; i++)
            {
                buffer[bufferIndex + i] = stringBuilder[i];
            }
            stringBuilder.Remove(0, num);
            this._charsRemoved += num;
            return (long) num;
        }

        private void WriteXmlElement()
        {
            if (!this._xmlReader.EOF)
            {
                bool canReadValueChunk = this._xmlReader.CanReadValueChunk;
                char[] buffer = null;
                this._xmlReader.Read();
                switch (this._xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        this._xmlWriter.WriteStartElement(this._xmlReader.Prefix, this._xmlReader.LocalName, this._xmlReader.NamespaceURI);
                        this._xmlWriter.WriteAttributes(this._xmlReader, true);
                        if (this._xmlReader.IsEmptyElement)
                        {
                            this._xmlWriter.WriteEndElement();
                        }
                        break;

                    case XmlNodeType.Text:
                        int num;
                        if (!canReadValueChunk)
                        {
                            this._xmlWriter.WriteString(this._xmlReader.Value);
                            break;
                        }
                        if (buffer == null)
                        {
                            buffer = new char[0x400];
                        }
                        while ((num = this._xmlReader.ReadValueChunk(buffer, 0, 0x400)) > 0)
                        {
                            this._xmlWriter.WriteChars(buffer, 0, num);
                        }
                        break;

                    case XmlNodeType.CDATA:
                        this._xmlWriter.WriteCData(this._xmlReader.Value);
                        break;

                    case XmlNodeType.EntityReference:
                        this._xmlWriter.WriteEntityRef(this._xmlReader.Name);
                        break;

                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.XmlDeclaration:
                        this._xmlWriter.WriteProcessingInstruction(this._xmlReader.Name, this._xmlReader.Value);
                        break;

                    case XmlNodeType.Comment:
                        this._xmlWriter.WriteComment(this._xmlReader.Value);
                        break;

                    case XmlNodeType.DocumentType:
                        this._xmlWriter.WriteDocType(this._xmlReader.Name, this._xmlReader.GetAttribute("PUBLIC"), this._xmlReader.GetAttribute("SYSTEM"), this._xmlReader.Value);
                        break;

                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        this._xmlWriter.WriteWhitespace(this._xmlReader.Value);
                        break;

                    case XmlNodeType.EndElement:
                        this._xmlWriter.WriteFullEndElement();
                        break;
                }
                this._xmlWriter.Flush();
            }
        }

        public int ColumnOrdinal
        {
            get
            {
                return this._columnOrdinal;
            }
        }
    }
}

