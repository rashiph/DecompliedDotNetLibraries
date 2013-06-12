namespace System.Xml
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;

    internal class XmlEncodedRawTextWriter : XmlRawWriter
    {
        protected int attrEndPos;
        protected bool autoXmlDeclaration;
        protected byte[] bufBytes;
        protected int bufBytesUsed;
        protected char[] bufChars;
        protected int bufLen;
        protected int bufPos;
        private const int BUFSIZE = 0x1800;
        protected int cdataPos;
        private CharEntityEncoderFallback charEntityFallback;
        protected bool checkCharacters;
        protected bool closeOutput;
        protected int contentPos;
        protected System.Text.Encoder encoder;
        protected Encoding encoding;
        protected bool hadDoubleBracket;
        protected bool inAttributeValue;
        private const int INIT_MARKS_COUNT = 0x40;
        protected bool inTextContent;
        private int lastMarkPos;
        protected bool mergeCDataSections;
        protected string newLineChars;
        protected NewLineHandling newLineHandling;
        protected bool omitXmlDeclaration;
        protected XmlOutputMethod outputMethod;
        private const int OVERFLOW = 0x20;
        protected XmlStandalone standalone;
        protected Stream stream;
        private int[] textContentMarks;
        protected int textPos;
        protected bool trackTextContent;
        protected TextWriter writer;
        protected bool writeToNull;
        protected XmlCharType xmlCharType;

        protected XmlEncodedRawTextWriter(XmlWriterSettings settings)
        {
            this.xmlCharType = XmlCharType.Instance;
            this.bufPos = 1;
            this.textPos = 1;
            this.bufLen = 0x1800;
            this.newLineHandling = settings.NewLineHandling;
            this.omitXmlDeclaration = settings.OmitXmlDeclaration;
            this.newLineChars = settings.NewLineChars;
            this.checkCharacters = settings.CheckCharacters;
            this.closeOutput = settings.CloseOutput;
            this.standalone = settings.Standalone;
            this.outputMethod = settings.OutputMethod;
            this.mergeCDataSections = settings.MergeCDataSections;
            if (this.checkCharacters && (this.newLineHandling == NewLineHandling.Replace))
            {
                this.ValidateContentChars(this.newLineChars, "NewLineChars", false);
            }
        }

        public XmlEncodedRawTextWriter(Stream stream, XmlWriterSettings settings) : this(settings)
        {
            this.stream = stream;
            this.encoding = settings.Encoding;
            this.bufChars = new char[0x1820];
            this.bufBytes = new byte[this.bufChars.Length];
            this.bufBytesUsed = 0;
            this.trackTextContent = true;
            this.inTextContent = false;
            this.lastMarkPos = 0;
            this.textContentMarks = new int[0x40];
            this.textContentMarks[0] = 1;
            this.charEntityFallback = new CharEntityEncoderFallback();
            this.encoding = (Encoding) settings.Encoding.Clone();
            this.encoding.EncoderFallback = this.charEntityFallback;
            this.encoder = this.encoding.GetEncoder();
            if (!stream.CanSeek || (stream.Position == 0L))
            {
                byte[] preamble = this.encoding.GetPreamble();
                if (preamble.Length != 0)
                {
                    this.stream.Write(preamble, 0, preamble.Length);
                }
            }
            if (settings.AutoXmlDeclaration)
            {
                this.WriteXmlDeclaration(this.standalone);
                this.autoXmlDeclaration = true;
            }
        }

        public XmlEncodedRawTextWriter(TextWriter writer, XmlWriterSettings settings) : this(settings)
        {
            this.writer = writer;
            this.encoding = writer.Encoding;
            this.bufChars = new char[0x1820];
            if (settings.AutoXmlDeclaration)
            {
                this.WriteXmlDeclaration(this.standalone);
                this.autoXmlDeclaration = true;
            }
        }

        protected static unsafe char* AmpEntity(char* pDst)
        {
            pDst[0] = '&';
            pDst[1] = 'a';
            pDst[2] = 'm';
            pDst[3] = 'p';
            pDst[4] = ';';
            return (pDst + 5);
        }

        protected static unsafe char* CarriageReturnEntity(char* pDst)
        {
            pDst[0] = '&';
            pDst[1] = '#';
            pDst[2] = 'x';
            pDst[3] = 'D';
            pDst[4] = ';';
            return (pDst + 5);
        }

        protected void ChangeTextContentMark(bool value)
        {
            this.inTextContent = value;
            if ((this.lastMarkPos + 1) == this.textContentMarks.Length)
            {
                this.GrowTextContentMarks();
            }
            this.textContentMarks[++this.lastMarkPos] = this.bufPos;
        }

        private static unsafe char* CharEntity(char* pDst, char ch)
        {
            string str = ((int) ch).ToString("X", NumberFormatInfo.InvariantInfo);
            pDst[0] = '&';
            pDst[1] = '#';
            pDst[2] = 'x';
            pDst += 3;
            fixed (char* str2 = ((char*) str))
            {
                char* chPtr2 = str2;
                do
                {
                    pDst++;
                    chPtr2++;
                }
                while ((pDst[0] = chPtr2[0]) != '\0');
            }
            pDst[-1] = ';';
            return pDst;
        }

        public override void Close()
        {
            try
            {
                this.FlushBuffer();
                this.FlushEncoder();
            }
            finally
            {
                this.writeToNull = true;
                if (this.stream != null)
                {
                    try
                    {
                        this.stream.Flush();
                        goto Label_007B;
                    }
                    finally
                    {
                        try
                        {
                            if (this.closeOutput)
                            {
                                this.stream.Close();
                            }
                        }
                        finally
                        {
                            this.stream = null;
                        }
                    }
                }
                if (this.writer != null)
                {
                    try
                    {
                        this.writer.Flush();
                    }
                    finally
                    {
                        try
                        {
                            if (this.closeOutput)
                            {
                                this.writer.Close();
                            }
                        }
                        finally
                        {
                            this.writer = null;
                        }
                    }
                }
            Label_007B:;
            }
        }

        internal unsafe void EncodeChar(ref char* pSrc, char* pSrcEnd, ref char* pDst)
        {
            int ch = (int) pSrc;
            if (XmlCharType.IsSurrogate(ch))
            {
                pDst = EncodeSurrogate(pSrc, pSrcEnd, pDst);
                pSrc += (IntPtr) 4;
            }
            else if ((ch <= 0x7f) || (ch >= 0xfffe))
            {
                pDst = this.InvalidXmlChar(ch, pDst, false);
                pSrc += (IntPtr) 2;
            }
            else
            {
                pDst = (char*) ((ushort) ch);
                pDst += (IntPtr) 2;
                pSrc += (IntPtr) 2;
            }
        }

        private void EncodeChars(int startOffset, int endOffset, bool writeAllToStream)
        {
            while (startOffset < endOffset)
            {
                int num;
                int num2;
                bool flag;
                if (this.charEntityFallback != null)
                {
                    this.charEntityFallback.StartOffset = startOffset;
                }
                this.encoder.Convert(this.bufChars, startOffset, endOffset - startOffset, this.bufBytes, this.bufBytesUsed, this.bufBytes.Length - this.bufBytesUsed, false, out num, out num2, out flag);
                startOffset += num;
                this.bufBytesUsed += num2;
                if (this.bufBytesUsed >= (this.bufBytes.Length - 0x10))
                {
                    this.stream.Write(this.bufBytes, 0, this.bufBytesUsed);
                    this.bufBytesUsed = 0;
                }
            }
            if (writeAllToStream && (this.bufBytesUsed > 0))
            {
                this.stream.Write(this.bufBytes, 0, this.bufBytesUsed);
                this.bufBytesUsed = 0;
            }
        }

        private static unsafe char* EncodeSurrogate(char* pSrc, char* pSrcEnd, char* pDst)
        {
            int num = pSrc[0];
            if (num > 0xdbff)
            {
                throw XmlConvert.CreateInvalidHighSurrogateCharException((char) num);
            }
            if ((pSrc + 1) >= pSrcEnd)
            {
                throw new ArgumentException(Res.GetString("Xml_InvalidSurrogateMissingLowChar"));
            }
            int num2 = pSrc[1];
            if (num2 < 0xdc00)
            {
                throw XmlConvert.CreateInvalidSurrogatePairException((char) num2, (char) num);
            }
            pDst[0] = (char) num;
            pDst[1] = (char) num2;
            pDst += 2;
            return pDst;
        }

        public override void Flush()
        {
            this.FlushBuffer();
            this.FlushEncoder();
            if (this.stream != null)
            {
                this.stream.Flush();
            }
            else if (this.writer != null)
            {
                this.writer.Flush();
            }
        }

        protected virtual void FlushBuffer()
        {
            try
            {
                if (!this.writeToNull)
                {
                    if (this.stream != null)
                    {
                        if (this.trackTextContent)
                        {
                            this.charEntityFallback.Reset(this.textContentMarks, this.lastMarkPos);
                            if ((this.lastMarkPos & 1) != 0)
                            {
                                this.textContentMarks[1] = 1;
                                this.lastMarkPos = 1;
                            }
                            else
                            {
                                this.lastMarkPos = 0;
                            }
                        }
                        this.EncodeChars(1, this.bufPos, true);
                    }
                    else
                    {
                        this.writer.Write(this.bufChars, 1, this.bufPos - 1);
                    }
                }
            }
            catch
            {
                this.writeToNull = true;
                throw;
            }
            finally
            {
                this.bufChars[0] = this.bufChars[this.bufPos - 1];
                this.textPos = (this.textPos == this.bufPos) ? 1 : 0;
                this.attrEndPos = (this.attrEndPos == this.bufPos) ? 1 : 0;
                this.contentPos = 0;
                this.cdataPos = 0;
                this.bufPos = 1;
            }
        }

        private void FlushEncoder()
        {
            if (this.stream != null)
            {
                int num;
                int num2;
                bool flag;
                this.encoder.Convert(this.bufChars, 1, 0, this.bufBytes, 0, this.bufBytes.Length, true, out num, out num2, out flag);
                if (num2 != 0)
                {
                    this.stream.Write(this.bufBytes, 0, num2);
                }
            }
        }

        private void GrowTextContentMarks()
        {
            int[] destinationArray = new int[this.textContentMarks.Length * 2];
            Array.Copy(this.textContentMarks, destinationArray, this.textContentMarks.Length);
            this.textContentMarks = destinationArray;
        }

        protected static unsafe char* GtEntity(char* pDst)
        {
            pDst[0] = '&';
            pDst[1] = 'g';
            pDst[2] = 't';
            pDst[3] = ';';
            return (pDst + 4);
        }

        private unsafe char* InvalidXmlChar(int ch, char* pDst, bool entitize)
        {
            if (this.checkCharacters)
            {
                throw XmlConvert.CreateInvalidCharException((char) ch, '\0');
            }
            if (entitize)
            {
                return CharEntity(pDst, (char) ch);
            }
            pDst[0] = (char) ch;
            pDst++;
            return pDst;
        }

        protected static unsafe char* LineFeedEntity(char* pDst)
        {
            pDst[0] = '&';
            pDst[1] = '#';
            pDst[2] = 'x';
            pDst[3] = 'A';
            pDst[4] = ';';
            return (pDst + 5);
        }

        protected static unsafe char* LtEntity(char* pDst)
        {
            pDst[0] = '&';
            pDst[1] = 'l';
            pDst[2] = 't';
            pDst[3] = ';';
            return (pDst + 4);
        }

        protected static unsafe char* QuoteEntity(char* pDst)
        {
            pDst[0] = '&';
            pDst[1] = 'q';
            pDst[2] = 'u';
            pDst[3] = 'o';
            pDst[4] = 't';
            pDst[5] = ';';
            return (pDst + 6);
        }

        protected static unsafe char* RawEndCData(char* pDst)
        {
            pDst[0] = ']';
            pDst[1] = ']';
            pDst[2] = '>';
            return (pDst + 3);
        }

        protected static unsafe char* RawStartCData(char* pDst)
        {
            pDst[0] = '<';
            pDst[1] = '!';
            pDst[2] = '[';
            pDst[3] = 'C';
            pDst[4] = 'D';
            pDst[5] = 'A';
            pDst[6] = 'T';
            pDst[7] = 'A';
            pDst[8] = '[';
            return (pDst + 9);
        }

        protected unsafe void RawText(string s)
        {
            fixed (char* str = ((char*) s))
            {
                char* pSrcBegin = str;
                this.RawText(pSrcBegin, pSrcBegin + s.Length);
            }
        }

        protected unsafe void RawText(char* pSrcBegin, char* pSrcEnd)
        {
            fixed (char* chRef = this.bufChars)
            {
                char* chPtr3;
                char* pDst = chRef + this.bufPos;
                char* pSrc = pSrcBegin;
                int ch = 0;
            Label_0030:
                chPtr3 = pDst + ((char*) (((long) ((pSrcEnd - pSrc) / 2)) * 2L));
                if (chPtr3 > (chRef + this.bufLen))
                {
                    chPtr3 = chRef + this.bufLen;
                }
                while ((pDst < chPtr3) && ((ch = pSrc[0]) < 0xd800))
                {
                    pSrc++;
                    pDst[0] = (char) ch;
                    pDst++;
                }
                if (pSrc < pSrcEnd)
                {
                    if (pDst >= chPtr3)
                    {
                        this.bufPos = (int) ((long) ((pDst - chRef) / 2));
                        this.FlushBuffer();
                        pDst = chRef + 1;
                    }
                    else if (XmlCharType.IsSurrogate(ch))
                    {
                        pDst = EncodeSurrogate(pSrc, pSrcEnd, pDst);
                        pSrc += 2;
                    }
                    else if ((ch <= 0x7f) || (ch >= 0xfffe))
                    {
                        pDst = this.InvalidXmlChar(ch, pDst, false);
                        pSrc++;
                    }
                    else
                    {
                        pDst[0] = (char) ch;
                        pDst++;
                        pSrc++;
                    }
                    goto Label_0030;
                }
                this.bufPos = (int) ((long) ((pDst - chRef) / 2));
            }
        }

        internal override void StartElementContent()
        {
            this.bufChars[this.bufPos++] = '>';
            this.contentPos = this.bufPos;
        }

        protected static unsafe char* TabEntity(char* pDst)
        {
            pDst[0] = '&';
            pDst[1] = '#';
            pDst[2] = 'x';
            pDst[3] = '9';
            pDst[4] = ';';
            return (pDst + 5);
        }

        protected void ValidateContentChars(string chars, string propertyName, bool allowOnlyWhitespace)
        {
            if (allowOnlyWhitespace)
            {
                if (!this.xmlCharType.IsOnlyWhitespace(chars))
                {
                    throw new ArgumentException(Res.GetString("Xml_IndentCharsNotWhitespace", new object[] { propertyName }));
                }
                return;
            }
            string str = null;
            for (int i = 0; i < chars.Length; i++)
            {
                if (this.xmlCharType.IsTextChar(chars[i]))
                {
                    continue;
                }
                switch (chars[i])
                {
                    case '<':
                    case ']':
                    case '&':
                        str = Res.GetString("Xml_InvalidCharacter", XmlException.BuildCharExceptionArgs(chars, i));
                        goto Label_0132;

                    case '\t':
                    case '\n':
                    case '\r':
                    {
                        continue;
                    }
                }
                if (XmlCharType.IsHighSurrogate(chars[i]))
                {
                    if (((i + 1) < chars.Length) && XmlCharType.IsLowSurrogate(chars[i + 1]))
                    {
                        i++;
                        continue;
                    }
                    str = Res.GetString("Xml_InvalidSurrogateMissingLowChar");
                    goto Label_0132;
                }
                if (XmlCharType.IsLowSurrogate(chars[i]))
                {
                    object[] args = new object[] { ((uint) chars[i]).ToString("X", CultureInfo.InvariantCulture) };
                    str = Res.GetString("Xml_InvalidSurrogateHighChar", args);
                    goto Label_0132;
                }
            }
            return;
        Label_0132:;
            throw new ArgumentException(Res.GetString("Xml_InvalidCharsInIndent", new string[] { propertyName, str }));
        }

        protected unsafe void WriteAttributeTextBlock(char* pSrc, char* pSrcEnd)
        {
            fixed (char* chRef = this.bufChars)
            {
                char* chPtr2;
                char* pDst = chRef + this.bufPos;
                int ch = 0;
            Label_002E:
                chPtr2 = pDst + ((char*) (((long) ((pSrcEnd - pSrc) / 2)) * 2L));
                if (chPtr2 > (chRef + this.bufLen))
                {
                    chPtr2 = chRef + this.bufLen;
                }
                while ((pDst < chPtr2) && ((this.xmlCharType.charProperties[ch = pSrc[0]] & 0x80) != 0))
                {
                    pDst[0] = (char) ch;
                    pDst++;
                    pSrc++;
                }
                if (pSrc >= pSrcEnd)
                {
                    goto Label_0203;
                }
                if (pDst >= chPtr2)
                {
                    this.bufPos = (int) ((long) ((pDst - chRef) / 2));
                    this.FlushBuffer();
                    pDst = chRef + 1;
                    goto Label_002E;
                }
                switch (ch)
                {
                    case 9:
                        if (this.newLineHandling != NewLineHandling.None)
                        {
                            break;
                        }
                        pDst[0] = (char) ch;
                        pDst++;
                        goto Label_01F8;

                    case 10:
                        if (this.newLineHandling != NewLineHandling.None)
                        {
                            goto Label_019D;
                        }
                        pDst[0] = (char) ch;
                        pDst++;
                        goto Label_01F8;

                    case 13:
                        if (this.newLineHandling != NewLineHandling.None)
                        {
                            goto Label_0180;
                        }
                        pDst[0] = (char) ch;
                        pDst++;
                        goto Label_01F8;

                    case 0x22:
                        pDst = QuoteEntity(pDst);
                        goto Label_01F8;

                    case 0x26:
                        pDst = AmpEntity(pDst);
                        goto Label_01F8;

                    case 0x27:
                        pDst[0] = (char) ch;
                        pDst++;
                        goto Label_01F8;

                    case 60:
                        pDst = LtEntity(pDst);
                        goto Label_01F8;

                    case 0x3e:
                        pDst = GtEntity(pDst);
                        goto Label_01F8;

                    default:
                        if (XmlCharType.IsSurrogate(ch))
                        {
                            pDst = EncodeSurrogate(pSrc, pSrcEnd, pDst);
                            pSrc += 2;
                        }
                        else if ((ch <= 0x7f) || (ch >= 0xfffe))
                        {
                            pDst = this.InvalidXmlChar(ch, pDst, true);
                            pSrc++;
                        }
                        else
                        {
                            pDst[0] = (char) ch;
                            pDst++;
                            pSrc++;
                        }
                        goto Label_002E;
                }
                pDst = TabEntity(pDst);
                goto Label_01F8;
            Label_0180:
                pDst = CarriageReturnEntity(pDst);
                goto Label_01F8;
            Label_019D:
                pDst = LineFeedEntity(pDst);
            Label_01F8:
                pSrc++;
                goto Label_002E;
            Label_0203:
                this.bufPos = (int) ((long) ((pDst - chRef) / 2));
            }
        }

        public override void WriteCData(string text)
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            if (this.mergeCDataSections && (this.bufPos == this.cdataPos))
            {
                this.bufPos -= 3;
            }
            else
            {
                this.bufChars[this.bufPos++] = '<';
                this.bufChars[this.bufPos++] = '!';
                this.bufChars[this.bufPos++] = '[';
                this.bufChars[this.bufPos++] = 'C';
                this.bufChars[this.bufPos++] = 'D';
                this.bufChars[this.bufPos++] = 'A';
                this.bufChars[this.bufPos++] = 'T';
                this.bufChars[this.bufPos++] = 'A';
                this.bufChars[this.bufPos++] = '[';
            }
            this.WriteCDataSection(text);
            this.bufChars[this.bufPos++] = ']';
            this.bufChars[this.bufPos++] = ']';
            this.bufChars[this.bufPos++] = '>';
            this.textPos = this.bufPos;
            this.cdataPos = this.bufPos;
        }

        protected unsafe void WriteCDataSection(string text)
        {
            if (text.Length == 0)
            {
                if (this.bufPos >= this.bufLen)
                {
                    this.FlushBuffer();
                }
            }
            else
            {
                fixed (char* str = ((char*) text))
                {
                    char* chPtr = str;
                    fixed (char* chRef = this.bufChars)
                    {
                        char* chPtr5;
                        char* pSrc = chPtr;
                        char* pSrcEnd = chPtr + text.Length;
                        char* pDst = chRef + this.bufPos;
                        int ch = 0;
                    Label_006B:
                        chPtr5 = pDst + ((char*) (((long) ((pSrcEnd - pSrc) / 2)) * 2L));
                        if (chPtr5 > (chRef + this.bufLen))
                        {
                            chPtr5 = chRef + this.bufLen;
                        }
                        while (((pDst < chPtr5) && ((this.xmlCharType.charProperties[ch = pSrc[0]] & 0x80) != 0)) && (ch != 0x5d))
                        {
                            pDst[0] = (char) ch;
                            pDst++;
                            pSrc++;
                        }
                        if (pSrc >= pSrcEnd)
                        {
                            goto Label_0299;
                        }
                        if (pDst >= chPtr5)
                        {
                            this.bufPos = (int) ((long) ((pDst - chRef) / 2));
                            this.FlushBuffer();
                            pDst = chRef + 1;
                            goto Label_006B;
                        }
                        switch (ch)
                        {
                            case 9:
                            case 0x22:
                            case 0x26:
                            case 0x27:
                            case 60:
                                pDst[0] = (char) ch;
                                pDst++;
                                goto Label_028F;

                            case 10:
                                if (this.newLineHandling != NewLineHandling.Replace)
                                {
                                    break;
                                }
                                pDst = this.WriteNewLine(pDst);
                                goto Label_028F;

                            case 13:
                                if (this.newLineHandling == NewLineHandling.Replace)
                                {
                                    if (pSrc[1] == '\n')
                                    {
                                        pSrc++;
                                    }
                                    pDst = this.WriteNewLine(pDst);
                                }
                                else
                                {
                                    pDst[0] = (char) ch;
                                    pDst++;
                                }
                                goto Label_028F;

                            case 0x3e:
                                if (this.hadDoubleBracket && (pDst[-1] == ']'))
                                {
                                    pDst = RawStartCData(RawEndCData(pDst));
                                }
                                pDst[0] = '>';
                                pDst++;
                                goto Label_028F;

                            case 0x5d:
                                if (pDst[-1] == ']')
                                {
                                    this.hadDoubleBracket = true;
                                }
                                else
                                {
                                    this.hadDoubleBracket = false;
                                }
                                pDst[0] = ']';
                                pDst++;
                                goto Label_028F;

                            default:
                                if (XmlCharType.IsSurrogate(ch))
                                {
                                    pDst = EncodeSurrogate(pSrc, pSrcEnd, pDst);
                                    pSrc += 2;
                                }
                                else if ((ch <= 0x7f) || (ch >= 0xfffe))
                                {
                                    pDst = this.InvalidXmlChar(ch, pDst, false);
                                    pSrc++;
                                }
                                else
                                {
                                    pDst[0] = (char) ch;
                                    pDst++;
                                    pSrc++;
                                }
                                goto Label_006B;
                        }
                        pDst[0] = (char) ch;
                        pDst++;
                    Label_028F:
                        pSrc++;
                        goto Label_006B;
                    Label_0299:
                        this.bufPos = (int) ((long) ((pDst - chRef) / 2));
                        str = null;
                    }
                }
            }
        }

        public override void WriteCharEntity(char ch)
        {
            string s = ((int) ch).ToString("X", NumberFormatInfo.InvariantInfo);
            if (this.checkCharacters && !this.xmlCharType.IsCharData(ch))
            {
                throw XmlConvert.CreateInvalidCharException(ch, '\0');
            }
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            this.bufChars[this.bufPos++] = '&';
            this.bufChars[this.bufPos++] = '#';
            this.bufChars[this.bufPos++] = 'x';
            this.RawText(s);
            this.bufChars[this.bufPos++] = ';';
            if (this.bufPos > this.bufLen)
            {
                this.FlushBuffer();
            }
            this.textPos = this.bufPos;
        }

        public override unsafe void WriteChars(char[] buffer, int index, int count)
        {
            if (this.trackTextContent && !this.inTextContent)
            {
                this.ChangeTextContentMark(true);
            }
            fixed (char* chRef = &(buffer[index]))
            {
                if (this.inAttributeValue)
                {
                    this.WriteAttributeTextBlock(chRef, chRef + count);
                }
                else
                {
                    this.WriteElementTextBlock(chRef, chRef + count);
                }
            }
        }

        public override void WriteComment(string text)
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            this.bufChars[this.bufPos++] = '<';
            this.bufChars[this.bufPos++] = '!';
            this.bufChars[this.bufPos++] = '-';
            this.bufChars[this.bufPos++] = '-';
            this.WriteCommentOrPi(text, 0x2d);
            this.bufChars[this.bufPos++] = '-';
            this.bufChars[this.bufPos++] = '-';
            this.bufChars[this.bufPos++] = '>';
        }

        protected unsafe void WriteCommentOrPi(string text, int stopChar)
        {
            if (text.Length == 0)
            {
                if (this.bufPos >= this.bufLen)
                {
                    this.FlushBuffer();
                }
            }
            else
            {
                fixed (char* str = ((char*) text))
                {
                    char* chPtr = str;
                    fixed (char* chRef = this.bufChars)
                    {
                        char* chPtr5;
                        char* pSrc = chPtr;
                        char* pSrcEnd = chPtr + text.Length;
                        char* pDst = chRef + this.bufPos;
                        int ch = 0;
                    Label_006B:
                        chPtr5 = pDst + ((char*) (((long) ((pSrcEnd - pSrc) / 2)) * 2L));
                        if (chPtr5 > (chRef + this.bufLen))
                        {
                            chPtr5 = chRef + this.bufLen;
                        }
                        while (((pDst < chPtr5) && ((this.xmlCharType.charProperties[ch = pSrc[0]] & 0x40) != 0)) && (ch != stopChar))
                        {
                            pDst[0] = (char) ch;
                            pDst++;
                            pSrc++;
                        }
                        if (pSrc >= pSrcEnd)
                        {
                            goto Label_02A4;
                        }
                        if (pDst >= chPtr5)
                        {
                            this.bufPos = (int) ((long) ((pDst - chRef) / 2));
                            this.FlushBuffer();
                            pDst = chRef + 1;
                            goto Label_006B;
                        }
                        switch (ch)
                        {
                            case 60:
                            case 9:
                            case 0x26:
                                pDst[0] = (char) ch;
                                pDst++;
                                goto Label_029A;

                            case 0x3f:
                                pDst[0] = '?';
                                pDst++;
                                if (((ch == stopChar) && ((pSrc + 1) < pSrcEnd)) && (pSrc[1] == '>'))
                                {
                                    pDst[0] = ' ';
                                    pDst++;
                                }
                                goto Label_029A;

                            case 0x5d:
                                pDst[0] = ']';
                                pDst++;
                                goto Label_029A;

                            case 10:
                                if (this.newLineHandling != NewLineHandling.Replace)
                                {
                                    break;
                                }
                                pDst = this.WriteNewLine(pDst);
                                goto Label_029A;

                            case 13:
                                if (this.newLineHandling == NewLineHandling.Replace)
                                {
                                    if (pSrc[1] == '\n')
                                    {
                                        pSrc++;
                                    }
                                    pDst = this.WriteNewLine(pDst);
                                }
                                else
                                {
                                    pDst[0] = (char) ch;
                                    pDst++;
                                }
                                goto Label_029A;

                            case 0x2d:
                                pDst[0] = '-';
                                pDst++;
                                if ((ch == stopChar) && (((pSrc + 1) == pSrcEnd) || (pSrc[1] == '-')))
                                {
                                    pDst[0] = ' ';
                                    pDst++;
                                }
                                goto Label_029A;

                            default:
                                if (XmlCharType.IsSurrogate(ch))
                                {
                                    pDst = EncodeSurrogate(pSrc, pSrcEnd, pDst);
                                    pSrc += 2;
                                }
                                else if ((ch <= 0x7f) || (ch >= 0xfffe))
                                {
                                    pDst = this.InvalidXmlChar(ch, pDst, false);
                                    pSrc++;
                                }
                                else
                                {
                                    pDst[0] = (char) ch;
                                    pDst++;
                                    pSrc++;
                                }
                                goto Label_006B;
                        }
                        pDst[0] = (char) ch;
                        pDst++;
                    Label_029A:
                        pSrc++;
                        goto Label_006B;
                    Label_02A4:
                        this.bufPos = (int) ((long) ((pDst - chRef) / 2));
                        str = null;
                    }
                }
            }
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            this.RawText("<!DOCTYPE ");
            this.RawText(name);
            if (pubid != null)
            {
                this.RawText(" PUBLIC \"");
                this.RawText(pubid);
                this.RawText("\" \"");
                if (sysid != null)
                {
                    this.RawText(sysid);
                }
                this.bufChars[this.bufPos++] = '"';
            }
            else if (sysid != null)
            {
                this.RawText(" SYSTEM \"");
                this.RawText(sysid);
                this.bufChars[this.bufPos++] = '"';
            }
            else
            {
                this.bufChars[this.bufPos++] = ' ';
            }
            if (subset != null)
            {
                this.bufChars[this.bufPos++] = '[';
                this.RawText(subset);
                this.bufChars[this.bufPos++] = ']';
            }
            this.bufChars[this.bufPos++] = '>';
        }

        protected unsafe void WriteElementTextBlock(char* pSrc, char* pSrcEnd)
        {
            fixed (char* chRef = this.bufChars)
            {
                char* chPtr2;
                char* pDst = chRef + this.bufPos;
                int ch = 0;
            Label_002E:
                chPtr2 = pDst + ((char*) (((long) ((pSrcEnd - pSrc) / 2)) * 2L));
                if (chPtr2 > (chRef + this.bufLen))
                {
                    chPtr2 = chRef + this.bufLen;
                }
                while ((pDst < chPtr2) && ((this.xmlCharType.charProperties[ch = pSrc[0]] & 0x80) != 0))
                {
                    pDst[0] = (char) ch;
                    pDst++;
                    pSrc++;
                }
                if (pSrc >= pSrcEnd)
                {
                    goto Label_0207;
                }
                if (pDst >= chPtr2)
                {
                    this.bufPos = (int) ((long) ((pDst - chRef) / 2));
                    this.FlushBuffer();
                    pDst = chRef + 1;
                    goto Label_002E;
                }
                switch (ch)
                {
                    case 9:
                    case 0x22:
                    case 0x27:
                        pDst[0] = (char) ch;
                        pDst++;
                        goto Label_01FC;

                    case 10:
                        if (this.newLineHandling == NewLineHandling.Replace)
                        {
                            pDst = this.WriteNewLine(pDst);
                        }
                        else
                        {
                            pDst[0] = (char) ch;
                            pDst++;
                        }
                        goto Label_01FC;

                    case 13:
                        switch (this.newLineHandling)
                        {
                            case NewLineHandling.Entitize:
                                goto Label_0196;

                            case NewLineHandling.None:
                                goto Label_019F;
                        }
                        goto Label_01FC;

                    case 0x26:
                        pDst = AmpEntity(pDst);
                        goto Label_01FC;

                    case 60:
                        pDst = LtEntity(pDst);
                        goto Label_01FC;

                    case 0x3e:
                        pDst = GtEntity(pDst);
                        goto Label_01FC;

                    default:
                        if (XmlCharType.IsSurrogate(ch))
                        {
                            pDst = EncodeSurrogate(pSrc, pSrcEnd, pDst);
                            pSrc += 2;
                        }
                        else if ((ch <= 0x7f) || (ch >= 0xfffe))
                        {
                            pDst = this.InvalidXmlChar(ch, pDst, true);
                            pSrc++;
                        }
                        else
                        {
                            pDst[0] = (char) ch;
                            pDst++;
                            pSrc++;
                        }
                        goto Label_002E;
                }
                if (pSrc[1] == '\n')
                {
                    pSrc++;
                }
                pDst = this.WriteNewLine(pDst);
                goto Label_01FC;
            Label_0196:
                pDst = CarriageReturnEntity(pDst);
                goto Label_01FC;
            Label_019F:
                pDst[0] = (char) ch;
                pDst++;
            Label_01FC:
                pSrc++;
                goto Label_002E;
            Label_0207:
                this.bufPos = (int) ((long) ((pDst - chRef) / 2));
                this.textPos = this.bufPos;
                this.contentPos = 0;
            }
        }

        public override void WriteEndAttribute()
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            this.bufChars[this.bufPos++] = '"';
            this.inAttributeValue = false;
            this.attrEndPos = this.bufPos;
        }

        internal override void WriteEndElement(string prefix, string localName, string ns)
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            if (this.contentPos != this.bufPos)
            {
                this.bufChars[this.bufPos++] = '<';
                this.bufChars[this.bufPos++] = '/';
                if ((prefix != null) && (prefix.Length != 0))
                {
                    this.RawText(prefix);
                    this.bufChars[this.bufPos++] = ':';
                }
                this.RawText(localName);
                this.bufChars[this.bufPos++] = '>';
            }
            else
            {
                this.bufPos--;
                this.bufChars[this.bufPos++] = ' ';
                this.bufChars[this.bufPos++] = '/';
                this.bufChars[this.bufPos++] = '>';
            }
        }

        internal override void WriteEndNamespaceDeclaration()
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            this.inAttributeValue = false;
            this.bufChars[this.bufPos++] = '"';
            this.attrEndPos = this.bufPos;
        }

        public override void WriteEntityRef(string name)
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            this.bufChars[this.bufPos++] = '&';
            this.RawText(name);
            this.bufChars[this.bufPos++] = ';';
            if (this.bufPos > this.bufLen)
            {
                this.FlushBuffer();
            }
            this.textPos = this.bufPos;
        }

        internal override void WriteFullEndElement(string prefix, string localName, string ns)
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            this.bufChars[this.bufPos++] = '<';
            this.bufChars[this.bufPos++] = '/';
            if ((prefix != null) && (prefix.Length != 0))
            {
                this.RawText(prefix);
                this.bufChars[this.bufPos++] = ':';
            }
            this.RawText(localName);
            this.bufChars[this.bufPos++] = '>';
        }

        internal override void WriteNamespaceDeclaration(string prefix, string namespaceName)
        {
            this.WriteStartNamespaceDeclaration(prefix);
            this.WriteString(namespaceName);
            this.WriteEndNamespaceDeclaration();
        }

        protected unsafe char* WriteNewLine(char* pDst)
        {
            fixed (char* chRef = this.bufChars)
            {
                this.bufPos = (int) ((long) ((pDst - chRef) / 2));
                this.RawText(this.newLineChars);
                return (chRef + this.bufPos);
            }
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            this.bufChars[this.bufPos++] = '<';
            this.bufChars[this.bufPos++] = '?';
            this.RawText(name);
            if (text.Length > 0)
            {
                this.bufChars[this.bufPos++] = ' ';
                this.WriteCommentOrPi(text, 0x3f);
            }
            this.bufChars[this.bufPos++] = '?';
            this.bufChars[this.bufPos++] = '>';
        }

        public override unsafe void WriteRaw(string data)
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            fixed (char* str = ((char*) data))
            {
                char* pSrcBegin = str;
                this.WriteRawWithCharChecking(pSrcBegin, pSrcBegin + data.Length);
            }
            this.textPos = this.bufPos;
        }

        public override unsafe void WriteRaw(char[] buffer, int index, int count)
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            fixed (char* chRef = &(buffer[index]))
            {
                this.WriteRawWithCharChecking(chRef, chRef + count);
            }
            this.textPos = this.bufPos;
        }

        protected unsafe void WriteRawWithCharChecking(char* pSrcBegin, char* pSrcEnd)
        {
            fixed (char* chRef = this.bufChars)
            {
                char* chPtr3;
                char* pSrc = pSrcBegin;
                char* pDst = chRef + this.bufPos;
                int ch = 0;
            Label_0030:
                chPtr3 = pDst + ((char*) (((long) ((pSrcEnd - pSrc) / 2)) * 2L));
                if (chPtr3 > (chRef + this.bufLen))
                {
                    chPtr3 = chRef + this.bufLen;
                }
                while ((pDst < chPtr3) && ((this.xmlCharType.charProperties[ch = pSrc[0]] & 0x40) != 0))
                {
                    pDst[0] = (char) ch;
                    pDst++;
                    pSrc++;
                }
                if (pSrc >= pSrcEnd)
                {
                    goto Label_0199;
                }
                if (pDst >= chPtr3)
                {
                    this.bufPos = (int) ((long) ((pDst - chRef) / 2));
                    this.FlushBuffer();
                    pDst = chRef + 1;
                    goto Label_0030;
                }
                switch (ch)
                {
                    case 60:
                    case 0x5d:
                    case 9:
                    case 0x26:
                        pDst[0] = (char) ch;
                        pDst++;
                        goto Label_018F;

                    case 10:
                        if (this.newLineHandling != NewLineHandling.Replace)
                        {
                            break;
                        }
                        pDst = this.WriteNewLine(pDst);
                        goto Label_018F;

                    case 13:
                        if (this.newLineHandling == NewLineHandling.Replace)
                        {
                            if (pSrc[1] == '\n')
                            {
                                pSrc++;
                            }
                            pDst = this.WriteNewLine(pDst);
                        }
                        else
                        {
                            pDst[0] = (char) ch;
                            pDst++;
                        }
                        goto Label_018F;

                    default:
                        if (XmlCharType.IsSurrogate(ch))
                        {
                            pDst = EncodeSurrogate(pSrc, pSrcEnd, pDst);
                            pSrc += 2;
                        }
                        else if ((ch <= 0x7f) || (ch >= 0xfffe))
                        {
                            pDst = this.InvalidXmlChar(ch, pDst, false);
                            pSrc++;
                        }
                        else
                        {
                            pDst[0] = (char) ch;
                            pDst++;
                            pSrc++;
                        }
                        goto Label_0030;
                }
                pDst[0] = (char) ch;
                pDst++;
            Label_018F:
                pSrc++;
                goto Label_0030;
            Label_0199:
                this.bufPos = (int) ((long) ((pDst - chRef) / 2));
            }
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            if (this.attrEndPos == this.bufPos)
            {
                this.bufChars[this.bufPos++] = ' ';
            }
            if ((prefix != null) && (prefix.Length > 0))
            {
                this.RawText(prefix);
                this.bufChars[this.bufPos++] = ':';
            }
            this.RawText(localName);
            this.bufChars[this.bufPos++] = '=';
            this.bufChars[this.bufPos++] = '"';
            this.inAttributeValue = true;
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            this.bufChars[this.bufPos++] = '<';
            if ((prefix != null) && (prefix.Length != 0))
            {
                this.RawText(prefix);
                this.bufChars[this.bufPos++] = ':';
            }
            this.RawText(localName);
            this.attrEndPos = this.bufPos;
        }

        internal override void WriteStartNamespaceDeclaration(string prefix)
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            if (prefix.Length == 0)
            {
                this.RawText(" xmlns=\"");
            }
            else
            {
                this.RawText(" xmlns:");
                this.RawText(prefix);
                this.bufChars[this.bufPos++] = '=';
                this.bufChars[this.bufPos++] = '"';
            }
            this.inAttributeValue = true;
            if (this.trackTextContent && !this.inTextContent)
            {
                this.ChangeTextContentMark(true);
            }
        }

        public override unsafe void WriteString(string text)
        {
            if (this.trackTextContent && !this.inTextContent)
            {
                this.ChangeTextContentMark(true);
            }
            fixed (char* str = ((char*) text))
            {
                char* pSrc = str;
                char* pSrcEnd = pSrc + text.Length;
                if (this.inAttributeValue)
                {
                    this.WriteAttributeTextBlock(pSrc, pSrcEnd);
                }
                else
                {
                    this.WriteElementTextBlock(pSrc, pSrcEnd);
                }
            }
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            int num = XmlCharType.CombineSurrogateChar(lowChar, highChar);
            this.bufChars[this.bufPos++] = '&';
            this.bufChars[this.bufPos++] = '#';
            this.bufChars[this.bufPos++] = 'x';
            this.RawText(num.ToString("X", NumberFormatInfo.InvariantInfo));
            this.bufChars[this.bufPos++] = ';';
            this.textPos = this.bufPos;
        }

        public override unsafe void WriteWhitespace(string ws)
        {
            if (this.trackTextContent && this.inTextContent)
            {
                this.ChangeTextContentMark(false);
            }
            fixed (char* str = ((char*) ws))
            {
                char* pSrc = str;
                char* pSrcEnd = pSrc + ws.Length;
                if (this.inAttributeValue)
                {
                    this.WriteAttributeTextBlock(pSrc, pSrcEnd);
                }
                else
                {
                    this.WriteElementTextBlock(pSrc, pSrcEnd);
                }
            }
        }

        internal override void WriteXmlDeclaration(string xmldecl)
        {
            if (!this.omitXmlDeclaration && !this.autoXmlDeclaration)
            {
                this.WriteProcessingInstruction("xml", xmldecl);
            }
        }

        internal override void WriteXmlDeclaration(XmlStandalone standalone)
        {
            if (!this.omitXmlDeclaration && !this.autoXmlDeclaration)
            {
                if (this.trackTextContent && this.inTextContent)
                {
                    this.ChangeTextContentMark(false);
                }
                this.RawText("<?xml version=\"");
                this.RawText("1.0");
                if (this.encoding != null)
                {
                    this.RawText("\" encoding=\"");
                    this.RawText(this.encoding.WebName);
                }
                if (standalone != XmlStandalone.Omit)
                {
                    this.RawText("\" standalone=\"");
                    this.RawText((standalone == XmlStandalone.Yes) ? "yes" : "no");
                }
                this.RawText("\"?>");
            }
        }

        public override XmlWriterSettings Settings
        {
            get
            {
                return new XmlWriterSettings { Encoding = this.encoding, OmitXmlDeclaration = this.omitXmlDeclaration, NewLineHandling = this.newLineHandling, NewLineChars = this.newLineChars, CloseOutput = this.closeOutput, ConformanceLevel = ConformanceLevel.Auto, CheckCharacters = this.checkCharacters, AutoXmlDeclaration = this.autoXmlDeclaration, Standalone = this.standalone, OutputMethod = this.outputMethod, ReadOnly = true };
            }
        }

        internal override bool SupportsNamespaceDeclarationInChunks
        {
            get
            {
                return true;
            }
        }
    }
}

