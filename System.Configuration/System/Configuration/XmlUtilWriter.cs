namespace System.Configuration
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml;

    internal class XmlUtilWriter
    {
        private Stream _baseStream;
        private bool _isLastLineBlank;
        private int _lineNumber;
        private int _linePosition;
        private object _lineStartCheckpoint;
        private bool _trackPosition;
        private TextWriter _writer;
        private const string NL = "\r\n";
        private const char SPACE = ' ';
        private static string SPACES_2 = new string(' ', 2);
        private static string SPACES_4 = new string(' ', 4);
        private static string SPACES_8 = new string(' ', 8);

        internal XmlUtilWriter(TextWriter writer, bool trackPosition)
        {
            this._writer = writer;
            this._trackPosition = trackPosition;
            this._lineNumber = 1;
            this._linePosition = 1;
            this._isLastLineBlank = true;
            if (this._trackPosition)
            {
                this._baseStream = ((StreamWriter) this._writer).BaseStream;
                this._lineStartCheckpoint = this.CreateStreamCheckpoint();
            }
        }

        internal int AppendAttributeValue(XmlTextReader reader)
        {
            int num = 0;
            char quoteChar = reader.QuoteChar;
            if ((quoteChar != '"') && (quoteChar != '\''))
            {
                quoteChar = '"';
            }
            num += this.Write(quoteChar);
            while (reader.ReadAttributeValue())
            {
                if (reader.NodeType == XmlNodeType.Text)
                {
                    num += this.AppendEscapeXmlString(reader.Value, true, quoteChar);
                }
                else
                {
                    num += this.AppendEntityRef(reader.Name);
                }
            }
            return (num + this.Write(quoteChar));
        }

        internal int AppendCData(string cdata)
        {
            this.Write("<![CDATA[");
            this.Write(cdata);
            this.Write("]]>");
            return (cdata.Length + 12);
        }

        internal int AppendCharEntity(char ch)
        {
            string s = ((int) ch).ToString("X", CultureInfo.InvariantCulture);
            this.Write('&');
            this.Write('#');
            this.Write('x');
            this.Write(s);
            this.Write(';');
            return (s.Length + 4);
        }

        internal int AppendComment(string comment)
        {
            this.Write("<!--");
            this.Write(comment);
            this.Write("-->");
            return (comment.Length + 7);
        }

        internal int AppendEntityRef(string entityRef)
        {
            this.Write('&');
            this.Write(entityRef);
            this.Write(';');
            return (entityRef.Length + 2);
        }

        internal int AppendEscapeTextString(string s)
        {
            return this.AppendEscapeXmlString(s, false, 'A');
        }

        internal int AppendEscapeXmlString(string s, bool inAttribute, char quoteChar)
        {
            int num = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                bool flag = false;
                string entityRef = null;
                if ((((ch < ' ') && (ch != '\t')) && ((ch != '\r') && (ch != '\n'))) || (ch > 0xfffd))
                {
                    flag = true;
                }
                else
                {
                    switch (ch)
                    {
                        case '\n':
                        case '\r':
                            flag = inAttribute;
                            break;

                        case '&':
                            entityRef = "amp";
                            break;

                        case '\'':
                            if (inAttribute && (quoteChar == ch))
                            {
                                entityRef = "apos";
                            }
                            break;

                        case '"':
                            if (inAttribute && (quoteChar == ch))
                            {
                                entityRef = "quot";
                            }
                            break;

                        case '<':
                            entityRef = "lt";
                            break;

                        case '>':
                            entityRef = "gt";
                            break;
                    }
                }
                if (flag)
                {
                    num += this.AppendCharEntity(ch);
                }
                else if (entityRef != null)
                {
                    num += this.AppendEntityRef(entityRef);
                }
                else
                {
                    num += this.Write(ch);
                }
            }
            return num;
        }

        internal int AppendIndent(int linePosition, int indent, int depth, bool newLine)
        {
            int num = 0;
            if (newLine)
            {
                num += this.AppendNewLine();
            }
            int count = (linePosition - 1) + (indent * depth);
            return (num + this.AppendSpaces(count));
        }

        internal int AppendNewLine()
        {
            return this.Write("\r\n");
        }

        internal int AppendProcessingInstruction(string name, string value)
        {
            this.Write("<?");
            this.Write(name);
            this.AppendSpace();
            this.Write(value);
            this.Write("?>");
            return ((name.Length + value.Length) + 5);
        }

        internal int AppendRequiredWhiteSpace(int fromLineNumber, int fromLinePosition, int toLineNumber, int toLinePosition)
        {
            int num = this.AppendWhiteSpace(fromLineNumber, fromLinePosition, toLineNumber, toLinePosition);
            if (num == 0)
            {
                num += this.AppendSpace();
            }
            return num;
        }

        internal int AppendSpace()
        {
            return this.Write(' ');
        }

        internal int AppendSpaces(int count)
        {
            int num = count;
            while (num > 0)
            {
                if (num >= 8)
                {
                    this.Write(SPACES_8);
                    num -= 8;
                }
                else
                {
                    if (num >= 4)
                    {
                        this.Write(SPACES_4);
                        num -= 4;
                        continue;
                    }
                    if (num >= 2)
                    {
                        this.Write(SPACES_2);
                        num -= 2;
                        continue;
                    }
                    this.Write(' ');
                    break;
                }
            }
            if (count <= 0)
            {
                return 0;
            }
            return count;
        }

        internal int AppendSpacesToLinePosition(int linePosition)
        {
            if (linePosition <= 0)
            {
                return 0;
            }
            int num = linePosition - this._linePosition;
            if ((num < 0) && this.IsLastLineBlank)
            {
                this.SeekToLineStart();
            }
            return this.AppendSpaces(linePosition - this._linePosition);
        }

        internal int AppendWhiteSpace(int fromLineNumber, int fromLinePosition, int toLineNumber, int toLinePosition)
        {
            int num = 0;
            while (fromLineNumber++ < toLineNumber)
            {
                num += this.AppendNewLine();
                fromLinePosition = 1;
            }
            return (num + this.AppendSpaces(toLinePosition - fromLinePosition));
        }

        internal object CreateStreamCheckpoint()
        {
            return new StreamWriterCheckpoint(this);
        }

        internal void Flush()
        {
            this._writer.Flush();
        }

        internal void RestoreStreamCheckpoint(object o)
        {
            StreamWriterCheckpoint checkpoint = (StreamWriterCheckpoint) o;
            this.Flush();
            this._lineNumber = checkpoint._lineNumber;
            this._linePosition = checkpoint._linePosition;
            this._isLastLineBlank = checkpoint._isLastLineBlank;
            this._baseStream.Seek(checkpoint._streamPosition, SeekOrigin.Begin);
            this._baseStream.SetLength(checkpoint._streamLength);
            this._baseStream.Flush();
        }

        internal void SeekToLineStart()
        {
            this.RestoreStreamCheckpoint(this._lineStartCheckpoint);
        }

        private void UpdatePosition(char ch)
        {
            switch (ch)
            {
                case '\t':
                case ' ':
                    this._linePosition++;
                    return;

                case '\n':
                    this._lineStartCheckpoint = this.CreateStreamCheckpoint();
                    return;

                case '\r':
                    this._lineNumber++;
                    this._linePosition = 1;
                    this._isLastLineBlank = true;
                    return;
            }
            this._linePosition++;
            this._isLastLineBlank = false;
        }

        internal int Write(char ch)
        {
            this._writer.Write(ch);
            if (this._trackPosition)
            {
                this.UpdatePosition(ch);
            }
            return 1;
        }

        internal int Write(string s)
        {
            if (this._trackPosition)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    char ch = s[i];
                    this._writer.Write(ch);
                    this.UpdatePosition(ch);
                }
            }
            else
            {
                this._writer.Write(s);
            }
            return s.Length;
        }

        internal bool IsLastLineBlank
        {
            get
            {
                return this._isLastLineBlank;
            }
        }

        internal int LineNumber
        {
            get
            {
                return this._lineNumber;
            }
        }

        internal int LinePosition
        {
            get
            {
                return this._linePosition;
            }
        }

        internal bool TrackPosition
        {
            get
            {
                return this._trackPosition;
            }
        }

        internal TextWriter Writer
        {
            get
            {
                return this._writer;
            }
        }

        private class StreamWriterCheckpoint
        {
            internal bool _isLastLineBlank;
            internal int _lineNumber;
            internal int _linePosition;
            internal long _streamLength;
            internal long _streamPosition;

            internal StreamWriterCheckpoint(XmlUtilWriter writer)
            {
                writer.Flush();
                this._lineNumber = writer._lineNumber;
                this._linePosition = writer._linePosition;
                this._isLastLineBlank = writer._isLastLineBlank;
                writer._baseStream.Flush();
                this._streamPosition = writer._baseStream.Position;
                this._streamLength = writer._baseStream.Length;
            }
        }
    }
}

