namespace System.Xml
{
    using System;
    using System.Text;

    internal class XmlCharCheckingWriter : XmlWrappingWriter
    {
        private bool checkNames;
        private bool checkValues;
        private string newLineChars;
        private bool replaceNewLines;
        private XmlCharType xmlCharType;

        internal XmlCharCheckingWriter(XmlWriter baseWriter, bool checkValues, bool checkNames, bool replaceNewLines, string newLineChars) : base(baseWriter)
        {
            this.checkValues = checkValues;
            this.checkNames = checkNames;
            this.replaceNewLines = replaceNewLines;
            this.newLineChars = newLineChars;
            if (checkValues)
            {
                this.xmlCharType = XmlCharType.Instance;
            }
        }

        private void CheckCharacters(string str)
        {
            XmlConvert.VerifyCharData(str, ExceptionType.ArgumentException);
        }

        private void CheckCharacters(char[] data, int offset, int len)
        {
            XmlConvert.VerifyCharData(data, offset, len, ExceptionType.ArgumentException);
        }

        private string InterleaveInvalidChars(string text, char invChar1, char invChar2)
        {
            StringBuilder builder = null;
            int startIndex = 0;
            int num2 = 0;
            while (num2 < text.Length)
            {
                if (((text[num2] == invChar2) && (num2 > 0)) && (text[num2 - 1] == invChar1))
                {
                    if (builder == null)
                    {
                        builder = new StringBuilder(text.Length + 5);
                    }
                    builder.Append(text, startIndex, num2 - startIndex);
                    builder.Append(' ');
                    startIndex = num2;
                }
                num2++;
            }
            if (builder == null)
            {
                if ((num2 != 0) && (text[num2 - 1] == invChar1))
                {
                    return (text + ' ');
                }
                return text;
            }
            builder.Append(text, startIndex, num2 - startIndex);
            if ((num2 > 0) && (text[num2 - 1] == invChar1))
            {
                builder.Append(' ');
            }
            return builder.ToString();
        }

        private string ReplaceNewLines(string str)
        {
            if (str == null)
            {
                return null;
            }
            StringBuilder builder = null;
            int startIndex = 0;
            int num2 = 0;
            while (num2 < str.Length)
            {
                char ch = str[num2];
                if (ch < ' ')
                {
                    if (ch == '\n')
                    {
                        if (this.newLineChars == "\n")
                        {
                            goto Label_00F7;
                        }
                        if (builder == null)
                        {
                            builder = new StringBuilder(str.Length + 5);
                        }
                        builder.Append(str, startIndex, num2 - startIndex);
                    }
                    else
                    {
                        if (ch != '\r')
                        {
                            goto Label_00F7;
                        }
                        if (((num2 + 1) < str.Length) && (str[num2 + 1] == '\n'))
                        {
                            if (this.newLineChars == "\r\n")
                            {
                                num2++;
                                goto Label_00F7;
                            }
                            if (builder == null)
                            {
                                builder = new StringBuilder(str.Length + 5);
                            }
                            builder.Append(str, startIndex, num2 - startIndex);
                            num2++;
                        }
                        else
                        {
                            if (this.newLineChars == "\r")
                            {
                                goto Label_00F7;
                            }
                            if (builder == null)
                            {
                                builder = new StringBuilder(str.Length + 5);
                            }
                            builder.Append(str, startIndex, num2 - startIndex);
                        }
                    }
                    builder.Append(this.newLineChars);
                    startIndex = num2 + 1;
                }
            Label_00F7:
                num2++;
            }
            if (builder == null)
            {
                return str;
            }
            builder.Append(str, startIndex, num2 - startIndex);
            return builder.ToString();
        }

        private string ReplaceNewLines(char[] data, int offset, int len)
        {
            if (data == null)
            {
                return null;
            }
            StringBuilder builder = null;
            int startIndex = offset;
            int num2 = offset + len;
            int index = offset;
            while (index < num2)
            {
                char ch = data[index];
                if (ch < ' ')
                {
                    if (ch == '\n')
                    {
                        if (this.newLineChars == "\n")
                        {
                            goto Label_00DF;
                        }
                        if (builder == null)
                        {
                            builder = new StringBuilder(len + 5);
                        }
                        builder.Append(data, startIndex, index - startIndex);
                    }
                    else
                    {
                        if (ch != '\r')
                        {
                            goto Label_00DF;
                        }
                        if (((index + 1) < num2) && (data[index + 1] == '\n'))
                        {
                            if (this.newLineChars == "\r\n")
                            {
                                index++;
                                goto Label_00DF;
                            }
                            if (builder == null)
                            {
                                builder = new StringBuilder(len + 5);
                            }
                            builder.Append(data, startIndex, index - startIndex);
                            index++;
                        }
                        else
                        {
                            if (this.newLineChars == "\r")
                            {
                                goto Label_00DF;
                            }
                            if (builder == null)
                            {
                                builder = new StringBuilder(len + 5);
                            }
                            builder.Append(data, startIndex, index - startIndex);
                        }
                    }
                    builder.Append(this.newLineChars);
                    startIndex = index + 1;
                }
            Label_00DF:
                index++;
            }
            if (builder == null)
            {
                return null;
            }
            builder.Append(data, startIndex, index - startIndex);
            return builder.ToString();
        }

        private void ValidateNCName(string ncname)
        {
            if (ncname.Length == 0)
            {
                throw new ArgumentException(Res.GetString("Xml_EmptyName"));
            }
            int invCharIndex = ValidateNames.ParseNCName(ncname, 0);
            if (invCharIndex != ncname.Length)
            {
                throw new ArgumentException(Res.GetString((invCharIndex == 0) ? "Xml_BadStartNameChar" : "Xml_BadNameChar", XmlException.BuildCharExceptionArgs(ncname, invCharIndex)));
            }
        }

        private void ValidateQName(string name)
        {
            int num;
            if (name.Length == 0)
            {
                throw new ArgumentException(Res.GetString("Xml_EmptyName"));
            }
            int invCharIndex = ValidateNames.ParseQName(name, 0, out num);
            if (invCharIndex != name.Length)
            {
                string str = ((invCharIndex == 0) || ((num > -1) && (invCharIndex == (num + 1)))) ? "Xml_BadStartNameChar" : "Xml_BadNameChar";
                throw new ArgumentException(Res.GetString(str, XmlException.BuildCharExceptionArgs(name, invCharIndex)));
            }
        }

        public override void WriteCData(string text)
        {
            if (text != null)
            {
                int num;
                if (this.checkValues)
                {
                    this.CheckCharacters(text);
                }
                if (this.replaceNewLines)
                {
                    text = this.ReplaceNewLines(text);
                }
                while ((num = text.IndexOf("]]>", StringComparison.Ordinal)) >= 0)
                {
                    base.writer.WriteCData(text.Substring(0, num + 2));
                    text = text.Substring(num + 2);
                }
            }
            base.writer.WriteCData(text);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (count > (buffer.Length - index))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (this.checkValues)
            {
                this.CheckCharacters(buffer, index, count);
            }
            if (this.replaceNewLines && (this.WriteState != WriteState.Attribute))
            {
                string text = this.ReplaceNewLines(buffer, index, count);
                if (text != null)
                {
                    this.WriteString(text);
                    return;
                }
            }
            base.writer.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            if (text != null)
            {
                if (this.checkValues)
                {
                    this.CheckCharacters(text);
                    text = this.InterleaveInvalidChars(text, '-', '-');
                }
                if (this.replaceNewLines)
                {
                    text = this.ReplaceNewLines(text);
                }
            }
            base.writer.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            if (this.checkNames)
            {
                this.ValidateQName(name);
            }
            if (this.checkValues)
            {
                int num;
                if ((pubid != null) && ((num = this.xmlCharType.IsPublicId(pubid)) >= 0))
                {
                    throw XmlConvert.CreateInvalidCharException(pubid, num);
                }
                if (sysid != null)
                {
                    this.CheckCharacters(sysid);
                }
                if (subset != null)
                {
                    this.CheckCharacters(subset);
                }
            }
            if (this.replaceNewLines)
            {
                sysid = this.ReplaceNewLines(sysid);
                pubid = this.ReplaceNewLines(pubid);
                subset = this.ReplaceNewLines(subset);
            }
            base.writer.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteEntityRef(string name)
        {
            if (this.checkNames)
            {
                this.ValidateQName(name);
            }
            base.writer.WriteEntityRef(name);
        }

        public override void WriteName(string name)
        {
            if (this.checkNames)
            {
                XmlConvert.VerifyQName(name, ExceptionType.XmlException);
            }
            base.writer.WriteName(name);
        }

        public override void WriteNmToken(string name)
        {
            if (this.checkNames)
            {
                if ((name == null) || (name.Length == 0))
                {
                    throw new ArgumentException(Res.GetString("Xml_EmptyName"));
                }
                XmlConvert.VerifyNMTOKEN(name);
            }
            base.writer.WriteNmToken(name);
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            if (this.checkNames)
            {
                this.ValidateNCName(name);
            }
            if (text != null)
            {
                if (this.checkValues)
                {
                    this.CheckCharacters(text);
                    text = this.InterleaveInvalidChars(text, '?', '>');
                }
                if (this.replaceNewLines)
                {
                    text = this.ReplaceNewLines(text);
                }
            }
            base.writer.WriteProcessingInstruction(name, text);
        }

        public override void WriteQualifiedName(string localName, string ns)
        {
            if (this.checkNames)
            {
                this.ValidateNCName(localName);
            }
            base.writer.WriteQualifiedName(localName, ns);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            if (this.checkNames)
            {
                if ((localName == null) || (localName.Length == 0))
                {
                    throw new ArgumentException(Res.GetString("Xml_EmptyLocalName"));
                }
                this.ValidateNCName(localName);
                if ((prefix != null) && (prefix.Length > 0))
                {
                    this.ValidateNCName(prefix);
                }
            }
            base.writer.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            if (this.checkNames)
            {
                if ((localName == null) || (localName.Length == 0))
                {
                    throw new ArgumentException(Res.GetString("Xml_EmptyLocalName"));
                }
                this.ValidateNCName(localName);
                if ((prefix != null) && (prefix.Length > 0))
                {
                    this.ValidateNCName(prefix);
                }
            }
            base.writer.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteString(string text)
        {
            if (text != null)
            {
                if (this.checkValues)
                {
                    this.CheckCharacters(text);
                }
                if (this.replaceNewLines && (this.WriteState != WriteState.Attribute))
                {
                    text = this.ReplaceNewLines(text);
                }
            }
            base.writer.WriteString(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            base.writer.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            int num;
            if (ws == null)
            {
                ws = string.Empty;
            }
            if (this.checkNames && ((num = this.xmlCharType.IsOnlyWhitespaceWithPos(ws)) != -1))
            {
                throw new ArgumentException(Res.GetString("Xml_InvalidWhitespaceCharacter", XmlException.BuildCharExceptionArgs(ws, num)));
            }
            if (this.replaceNewLines)
            {
                ws = this.ReplaceNewLines(ws);
            }
            base.writer.WriteWhitespace(ws);
        }

        public override XmlWriterSettings Settings
        {
            get
            {
                XmlWriterSettings settings = base.writer.Settings;
                settings = (settings != null) ? settings.Clone() : new XmlWriterSettings();
                if (this.checkValues)
                {
                    settings.CheckCharacters = true;
                }
                if (this.replaceNewLines)
                {
                    settings.NewLineHandling = NewLineHandling.Replace;
                    settings.NewLineChars = this.newLineChars;
                }
                settings.ReadOnly = true;
                return settings;
            }
        }
    }
}

