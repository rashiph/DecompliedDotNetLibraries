namespace System.Xml
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;

    internal class XmlUtf8RawTextWriter : XmlRawWriter
    {
        protected int attrEndPos;
        protected bool autoXmlDeclaration;
        protected byte[] bufBytes;
        protected int bufLen;
        protected int bufPos;
        private const int BUFSIZE = 0x1800;
        protected int cdataPos;
        protected bool checkCharacters;
        protected bool closeOutput;
        protected int contentPos;
        protected Encoding encoding;
        protected bool hadDoubleBracket;
        protected bool inAttributeValue;
        private const int INIT_MARKS_COUNT = 0x40;
        protected bool mergeCDataSections;
        protected string newLineChars;
        protected NewLineHandling newLineHandling;
        protected bool omitXmlDeclaration;
        protected XmlOutputMethod outputMethod;
        private const int OVERFLOW = 0x20;
        protected XmlStandalone standalone;
        protected Stream stream;
        protected int textPos;
        protected bool writeToNull;
        protected XmlCharType xmlCharType;

        protected XmlUtf8RawTextWriter(XmlWriterSettings settings)
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

        public XmlUtf8RawTextWriter(Stream stream, XmlWriterSettings settings) : this(settings)
        {
            this.stream = stream;
            this.encoding = settings.Encoding;
            this.bufBytes = new byte[0x1820];
            if (!stream.CanSeek || (stream.Position == 0L))
            {
                byte[] preamble = this.encoding.GetPreamble();
                if (preamble.Length != 0)
                {
                    Buffer.BlockCopy(preamble, 0, this.bufBytes, 1, preamble.Length);
                    this.bufPos += preamble.Length;
                    this.textPos += preamble.Length;
                }
            }
            if (settings.AutoXmlDeclaration)
            {
                this.WriteXmlDeclaration(this.standalone);
                this.autoXmlDeclaration = true;
            }
        }

        protected static unsafe byte* AmpEntity(byte* pDst)
        {
            pDst[0] = 0x26;
            pDst[1] = 0x61;
            pDst[2] = 0x6d;
            pDst[3] = 0x70;
            pDst[4] = 0x3b;
            return (pDst + 5);
        }

        protected static unsafe byte* CarriageReturnEntity(byte* pDst)
        {
            pDst[0] = 0x26;
            pDst[1] = 0x23;
            pDst[2] = 120;
            pDst[3] = 0x44;
            pDst[4] = 0x3b;
            return (pDst + 5);
        }

        private static unsafe byte* CharEntity(byte* pDst, char ch)
        {
            string str = ((int) ch).ToString("X", NumberFormatInfo.InvariantInfo);
            pDst[0] = 0x26;
            pDst[1] = 0x23;
            pDst[2] = 120;
            pDst += 3;
            fixed (char* str2 = ((char*) str))
            {
                char* chPtr2 = str2;
                do
                {
                    pDst++;
                    chPtr2++;
                }
                while ((pDst[0] = (byte) chPtr2[0]) != 0);
            }
            pDst[-1] = 0x3b;
            return pDst;
        }

        internal static unsafe void CharToUTF8(ref char* pSrc, char* pSrcEnd, ref byte* pDst)
        {
            int ch = (int) pSrc;
            if (ch <= 0x7f)
            {
                pDst = (byte*) ((byte) ch);
                pDst += (IntPtr) 1;
                pSrc += (IntPtr) 2;
            }
            else if (XmlCharType.IsSurrogate(ch))
            {
                pDst = EncodeSurrogate(pSrc, pSrcEnd, pDst);
                pSrc += (IntPtr) 4;
            }
            else
            {
                pDst = EncodeMultibyteUTF8(ch, pDst);
                pSrc += (IntPtr) 2;
            }
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
            }
        }

        internal unsafe void EncodeChar(ref char* pSrc, char* pSrcEnd, ref byte* pDst)
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
                pDst = EncodeMultibyteUTF8(ch, pDst);
                pSrc += (IntPtr) 2;
            }
        }

        internal static unsafe byte* EncodeMultibyteUTF8(int ch, byte* pDst)
        {
            if (ch < 0x800)
            {
                pDst[0] = (byte) (-64 | (ch >> 6));
            }
            else
            {
                pDst[0] = (byte) (-32 | (ch >> 12));
                pDst++;
                pDst[0] = (byte) (-128 | ((ch >> 6) & 0x3f));
            }
            pDst++;
            pDst[0] = (byte) (0x80 | (ch & 0x3f));
            return (pDst + 1);
        }

        private static unsafe byte* EncodeSurrogate(char* pSrc, char* pSrcEnd, byte* pDst)
        {
            int highChar = pSrc[0];
            if (highChar > 0xdbff)
            {
                throw XmlConvert.CreateInvalidHighSurrogateCharException((char) highChar);
            }
            if ((pSrc + 1) >= pSrcEnd)
            {
                throw new ArgumentException(Res.GetString("Xml_InvalidSurrogateMissingLowChar"));
            }
            int lowChar = pSrc[1];
            if (lowChar < 0xdc00)
            {
                throw XmlConvert.CreateInvalidSurrogatePairException((char) lowChar, (char) highChar);
            }
            highChar = XmlCharType.CombineSurrogateChar(lowChar, highChar);
            pDst[0] = (byte) (240 | (highChar >> 0x12));
            pDst[1] = (byte) (0x80 | ((highChar >> 12) & 0x3f));
            pDst[2] = (byte) (0x80 | ((highChar >> 6) & 0x3f));
            pDst[3] = (byte) (0x80 | (highChar & 0x3f));
            pDst += 4;
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
        }

        protected virtual void FlushBuffer()
        {
            try
            {
                if (!this.writeToNull)
                {
                    this.stream.Write(this.bufBytes, 1, this.bufPos - 1);
                }
            }
            catch
            {
                this.writeToNull = true;
                throw;
            }
            finally
            {
                this.bufBytes[0] = this.bufBytes[this.bufPos - 1];
                if (IsSurrogateByte(this.bufBytes[0]))
                {
                    this.bufBytes[1] = this.bufBytes[this.bufPos];
                    this.bufBytes[2] = this.bufBytes[this.bufPos + 1];
                    this.bufBytes[3] = this.bufBytes[this.bufPos + 2];
                }
                this.textPos = (this.textPos == this.bufPos) ? 1 : 0;
                this.attrEndPos = (this.attrEndPos == this.bufPos) ? 1 : 0;
                this.contentPos = 0;
                this.cdataPos = 0;
                this.bufPos = 1;
            }
        }

        private void FlushEncoder()
        {
        }

        protected static unsafe byte* GtEntity(byte* pDst)
        {
            pDst[0] = 0x26;
            pDst[1] = 0x67;
            pDst[2] = 0x74;
            pDst[3] = 0x3b;
            return (pDst + 4);
        }

        private unsafe byte* InvalidXmlChar(int ch, byte* pDst, bool entitize)
        {
            if (this.checkCharacters)
            {
                throw XmlConvert.CreateInvalidCharException((char) ch, '\0');
            }
            if (entitize)
            {
                return CharEntity(pDst, (char) ch);
            }
            if (ch < 0x80)
            {
                pDst[0] = (byte) ch;
                pDst++;
                return pDst;
            }
            pDst = EncodeMultibyteUTF8(ch, pDst);
            return pDst;
        }

        private static bool IsSurrogateByte(byte b)
        {
            return ((b & 0xf8) == 240);
        }

        protected static unsafe byte* LineFeedEntity(byte* pDst)
        {
            pDst[0] = 0x26;
            pDst[1] = 0x23;
            pDst[2] = 120;
            pDst[3] = 0x41;
            pDst[4] = 0x3b;
            return (pDst + 5);
        }

        protected static unsafe byte* LtEntity(byte* pDst)
        {
            pDst[0] = 0x26;
            pDst[1] = 0x6c;
            pDst[2] = 0x74;
            pDst[3] = 0x3b;
            return (pDst + 4);
        }

        protected static unsafe byte* QuoteEntity(byte* pDst)
        {
            pDst[0] = 0x26;
            pDst[1] = 0x71;
            pDst[2] = 0x75;
            pDst[3] = 0x6f;
            pDst[4] = 0x74;
            pDst[5] = 0x3b;
            return (pDst + 6);
        }

        protected static unsafe byte* RawEndCData(byte* pDst)
        {
            pDst[0] = 0x5d;
            pDst[1] = 0x5d;
            pDst[2] = 0x3e;
            return (pDst + 3);
        }

        protected static unsafe byte* RawStartCData(byte* pDst)
        {
            pDst[0] = 60;
            pDst[1] = 0x21;
            pDst[2] = 0x5b;
            pDst[3] = 0x43;
            pDst[4] = 0x44;
            pDst[5] = 0x41;
            pDst[6] = 0x54;
            pDst[7] = 0x41;
            pDst[8] = 0x5b;
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
            fixed (byte* numRef = this.bufBytes)
            {
                byte* numPtr2;
                byte* pDst = numRef + this.bufPos;
                char* pSrc = pSrcBegin;
                int ch = 0;
            Label_002D:
                numPtr2 = pDst + ((byte*) ((long) ((pSrcEnd - pSrc) / 2)));
                if (numPtr2 > (numRef + this.bufLen))
                {
                    numPtr2 = numRef + this.bufLen;
                }
                while ((pDst < numPtr2) && ((ch = pSrc[0]) <= '\x007f'))
                {
                    pSrc++;
                    pDst[0] = (byte) ch;
                    pDst++;
                }
                if (pSrc < pSrcEnd)
                {
                    if (pDst >= numPtr2)
                    {
                        this.bufPos = (int) ((long) ((pDst - numRef) / 1));
                        this.FlushBuffer();
                        pDst = numRef + 1;
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
                        pDst = EncodeMultibyteUTF8(ch, pDst);
                        pSrc++;
                    }
                    goto Label_002D;
                }
                this.bufPos = (int) ((long) ((pDst - numRef) / 1));
            }
        }

        internal override void StartElementContent()
        {
            this.bufBytes[this.bufPos++] = 0x3e;
            this.contentPos = this.bufPos;
        }

        protected static unsafe byte* TabEntity(byte* pDst)
        {
            pDst[0] = 0x26;
            pDst[1] = 0x23;
            pDst[2] = 120;
            pDst[3] = 0x39;
            pDst[4] = 0x3b;
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
            fixed (byte* numRef = this.bufBytes)
            {
                byte* numPtr2;
                byte* pDst = numRef + this.bufPos;
                int ch = 0;
            Label_002B:
                numPtr2 = pDst + ((byte*) ((long) ((pSrcEnd - pSrc) / 2)));
                if (numPtr2 > (numRef + this.bufLen))
                {
                    numPtr2 = numRef + this.bufLen;
                }
                while (((pDst < numPtr2) && ((this.xmlCharType.charProperties[ch = pSrc[0]] & 0x80) != 0)) && (ch <= 0x7f))
                {
                    pDst[0] = (byte) ch;
                    pDst++;
                    pSrc++;
                }
                if (pSrc >= pSrcEnd)
                {
                    goto Label_01FB;
                }
                if (pDst >= numPtr2)
                {
                    this.bufPos = (int) ((long) ((pDst - numRef) / 1));
                    this.FlushBuffer();
                    pDst = numRef + 1;
                    goto Label_002B;
                }
                switch (ch)
                {
                    case 9:
                        if (this.newLineHandling != NewLineHandling.None)
                        {
                            break;
                        }
                        pDst[0] = (byte) ch;
                        pDst++;
                        goto Label_01F0;

                    case 10:
                        if (this.newLineHandling != NewLineHandling.None)
                        {
                            goto Label_0196;
                        }
                        pDst[0] = (byte) ch;
                        pDst++;
                        goto Label_01F0;

                    case 13:
                        if (this.newLineHandling != NewLineHandling.None)
                        {
                            goto Label_0179;
                        }
                        pDst[0] = (byte) ch;
                        pDst++;
                        goto Label_01F0;

                    case 0x22:
                        pDst = QuoteEntity(pDst);
                        goto Label_01F0;

                    case 0x26:
                        pDst = AmpEntity(pDst);
                        goto Label_01F0;

                    case 0x27:
                        pDst[0] = (byte) ch;
                        pDst++;
                        goto Label_01F0;

                    case 60:
                        pDst = LtEntity(pDst);
                        goto Label_01F0;

                    case 0x3e:
                        pDst = GtEntity(pDst);
                        goto Label_01F0;

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
                            pDst = EncodeMultibyteUTF8(ch, pDst);
                            pSrc++;
                        }
                        goto Label_002B;
                }
                pDst = TabEntity(pDst);
                goto Label_01F0;
            Label_0179:
                pDst = CarriageReturnEntity(pDst);
                goto Label_01F0;
            Label_0196:
                pDst = LineFeedEntity(pDst);
            Label_01F0:
                pSrc++;
                goto Label_002B;
            Label_01FB:
                this.bufPos = (int) ((long) ((pDst - numRef) / 1));
            }
        }

        public override void WriteCData(string text)
        {
            if (this.mergeCDataSections && (this.bufPos == this.cdataPos))
            {
                this.bufPos -= 3;
            }
            else
            {
                this.bufBytes[this.bufPos++] = 60;
                this.bufBytes[this.bufPos++] = 0x21;
                this.bufBytes[this.bufPos++] = 0x5b;
                this.bufBytes[this.bufPos++] = 0x43;
                this.bufBytes[this.bufPos++] = 0x44;
                this.bufBytes[this.bufPos++] = 0x41;
                this.bufBytes[this.bufPos++] = 0x54;
                this.bufBytes[this.bufPos++] = 0x41;
                this.bufBytes[this.bufPos++] = 0x5b;
            }
            this.WriteCDataSection(text);
            this.bufBytes[this.bufPos++] = 0x5d;
            this.bufBytes[this.bufPos++] = 0x5d;
            this.bufBytes[this.bufPos++] = 0x3e;
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
                    fixed (byte* numRef = this.bufBytes)
                    {
                        byte* numPtr2;
                        char* pSrc = chPtr;
                        char* pSrcEnd = chPtr + text.Length;
                        byte* pDst = numRef + this.bufPos;
                        int ch = 0;
                    Label_0068:
                        numPtr2 = pDst + ((byte*) ((long) ((pSrcEnd - pSrc) / 2)));
                        if (numPtr2 > (numRef + this.bufLen))
                        {
                            numPtr2 = numRef + this.bufLen;
                        }
                        while (((pDst < numPtr2) && ((this.xmlCharType.charProperties[ch = pSrc[0]] & 0x80) != 0)) && ((ch != 0x5d) && (ch <= 0x7f)))
                        {
                            pDst[0] = (byte) ch;
                            pDst++;
                            pSrc++;
                        }
                        if (pSrc >= pSrcEnd)
                        {
                            goto Label_028F;
                        }
                        if (pDst >= numPtr2)
                        {
                            this.bufPos = (int) ((long) ((pDst - numRef) / 1));
                            this.FlushBuffer();
                            pDst = numRef + 1;
                            goto Label_0068;
                        }
                        switch (ch)
                        {
                            case 9:
                            case 0x22:
                            case 0x26:
                            case 0x27:
                            case 60:
                                pDst[0] = (byte) ch;
                                pDst++;
                                goto Label_0285;

                            case 10:
                                if (this.newLineHandling != NewLineHandling.Replace)
                                {
                                    break;
                                }
                                pDst = this.WriteNewLine(pDst);
                                goto Label_0285;

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
                                    pDst[0] = (byte) ch;
                                    pDst++;
                                }
                                goto Label_0285;

                            case 0x3e:
                                if (this.hadDoubleBracket && (pDst[-1] == 0x5d))
                                {
                                    pDst = RawStartCData(RawEndCData(pDst));
                                }
                                pDst[0] = 0x3e;
                                pDst++;
                                goto Label_0285;

                            case 0x5d:
                                if (pDst[-1] == 0x5d)
                                {
                                    this.hadDoubleBracket = true;
                                }
                                else
                                {
                                    this.hadDoubleBracket = false;
                                }
                                pDst[0] = 0x5d;
                                pDst++;
                                goto Label_0285;

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
                                    pDst = EncodeMultibyteUTF8(ch, pDst);
                                    pSrc++;
                                }
                                goto Label_0068;
                        }
                        pDst[0] = (byte) ch;
                        pDst++;
                    Label_0285:
                        pSrc++;
                        goto Label_0068;
                    Label_028F:
                        this.bufPos = (int) ((long) ((pDst - numRef) / 1));
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
            this.bufBytes[this.bufPos++] = 0x26;
            this.bufBytes[this.bufPos++] = 0x23;
            this.bufBytes[this.bufPos++] = 120;
            this.RawText(s);
            this.bufBytes[this.bufPos++] = 0x3b;
            if (this.bufPos > this.bufLen)
            {
                this.FlushBuffer();
            }
            this.textPos = this.bufPos;
        }

        public override unsafe void WriteChars(char[] buffer, int index, int count)
        {
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
            this.bufBytes[this.bufPos++] = 60;
            this.bufBytes[this.bufPos++] = 0x21;
            this.bufBytes[this.bufPos++] = 0x2d;
            this.bufBytes[this.bufPos++] = 0x2d;
            this.WriteCommentOrPi(text, 0x2d);
            this.bufBytes[this.bufPos++] = 0x2d;
            this.bufBytes[this.bufPos++] = 0x2d;
            this.bufBytes[this.bufPos++] = 0x3e;
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
                    fixed (byte* numRef = this.bufBytes)
                    {
                        byte* numPtr2;
                        char* pSrc = chPtr;
                        char* pSrcEnd = chPtr + text.Length;
                        byte* pDst = numRef + this.bufPos;
                        int ch = 0;
                    Label_0068:
                        numPtr2 = pDst + ((byte*) ((long) ((pSrcEnd - pSrc) / 2)));
                        if (numPtr2 > (numRef + this.bufLen))
                        {
                            numPtr2 = numRef + this.bufLen;
                        }
                        while (((pDst < numPtr2) && ((this.xmlCharType.charProperties[ch = pSrc[0]] & 0x40) != 0)) && ((ch != stopChar) && (ch <= 0x7f)))
                        {
                            pDst[0] = (byte) ch;
                            pDst++;
                            pSrc++;
                        }
                        if (pSrc >= pSrcEnd)
                        {
                            goto Label_029C;
                        }
                        if (pDst >= numPtr2)
                        {
                            this.bufPos = (int) ((long) ((pDst - numRef) / 1));
                            this.FlushBuffer();
                            pDst = numRef + 1;
                            goto Label_0068;
                        }
                        switch (ch)
                        {
                            case 60:
                            case 9:
                            case 0x26:
                                pDst[0] = (byte) ch;
                                pDst++;
                                goto Label_0292;

                            case 0x3f:
                                pDst[0] = 0x3f;
                                pDst++;
                                if (((ch == stopChar) && ((pSrc + 1) < pSrcEnd)) && (pSrc[1] == '>'))
                                {
                                    pDst[0] = 0x20;
                                    pDst++;
                                }
                                goto Label_0292;

                            case 0x5d:
                                pDst[0] = 0x5d;
                                pDst++;
                                goto Label_0292;

                            case 10:
                                if (this.newLineHandling != NewLineHandling.Replace)
                                {
                                    break;
                                }
                                pDst = this.WriteNewLine(pDst);
                                goto Label_0292;

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
                                    pDst[0] = (byte) ch;
                                    pDst++;
                                }
                                goto Label_0292;

                            case 0x2d:
                                pDst[0] = 0x2d;
                                pDst++;
                                if ((ch == stopChar) && (((pSrc + 1) == pSrcEnd) || (pSrc[1] == '-')))
                                {
                                    pDst[0] = 0x20;
                                    pDst++;
                                }
                                goto Label_0292;

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
                                    pDst = EncodeMultibyteUTF8(ch, pDst);
                                    pSrc++;
                                }
                                goto Label_0068;
                        }
                        pDst[0] = (byte) ch;
                        pDst++;
                    Label_0292:
                        pSrc++;
                        goto Label_0068;
                    Label_029C:
                        this.bufPos = (int) ((long) ((pDst - numRef) / 1));
                        str = null;
                    }
                }
            }
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
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
                this.bufBytes[this.bufPos++] = 0x22;
            }
            else if (sysid != null)
            {
                this.RawText(" SYSTEM \"");
                this.RawText(sysid);
                this.bufBytes[this.bufPos++] = 0x22;
            }
            else
            {
                this.bufBytes[this.bufPos++] = 0x20;
            }
            if (subset != null)
            {
                this.bufBytes[this.bufPos++] = 0x5b;
                this.RawText(subset);
                this.bufBytes[this.bufPos++] = 0x5d;
            }
            this.bufBytes[this.bufPos++] = 0x3e;
        }

        protected unsafe void WriteElementTextBlock(char* pSrc, char* pSrcEnd)
        {
            fixed (byte* numRef = this.bufBytes)
            {
                byte* numPtr2;
                byte* pDst = numRef + this.bufPos;
                int ch = 0;
            Label_002B:
                numPtr2 = pDst + ((byte*) ((long) ((pSrcEnd - pSrc) / 2)));
                if (numPtr2 > (numRef + this.bufLen))
                {
                    numPtr2 = numRef + this.bufLen;
                }
                while (((pDst < numPtr2) && ((this.xmlCharType.charProperties[ch = pSrc[0]] & 0x80) != 0)) && (ch <= 0x7f))
                {
                    pDst[0] = (byte) ch;
                    pDst++;
                    pSrc++;
                }
                if (pSrc >= pSrcEnd)
                {
                    goto Label_01FF;
                }
                if (pDst >= numPtr2)
                {
                    this.bufPos = (int) ((long) ((pDst - numRef) / 1));
                    this.FlushBuffer();
                    pDst = numRef + 1;
                    goto Label_002B;
                }
                switch (ch)
                {
                    case 9:
                    case 0x22:
                    case 0x27:
                        pDst[0] = (byte) ch;
                        pDst++;
                        goto Label_01F4;

                    case 10:
                        if (this.newLineHandling == NewLineHandling.Replace)
                        {
                            pDst = this.WriteNewLine(pDst);
                        }
                        else
                        {
                            pDst[0] = (byte) ch;
                            pDst++;
                        }
                        goto Label_01F4;

                    case 13:
                        switch (this.newLineHandling)
                        {
                            case NewLineHandling.Entitize:
                                goto Label_018F;

                            case NewLineHandling.None:
                                goto Label_0198;
                        }
                        goto Label_01F4;

                    case 0x26:
                        pDst = AmpEntity(pDst);
                        goto Label_01F4;

                    case 60:
                        pDst = LtEntity(pDst);
                        goto Label_01F4;

                    case 0x3e:
                        pDst = GtEntity(pDst);
                        goto Label_01F4;

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
                            pDst = EncodeMultibyteUTF8(ch, pDst);
                            pSrc++;
                        }
                        goto Label_002B;
                }
                if (pSrc[1] == '\n')
                {
                    pSrc++;
                }
                pDst = this.WriteNewLine(pDst);
                goto Label_01F4;
            Label_018F:
                pDst = CarriageReturnEntity(pDst);
                goto Label_01F4;
            Label_0198:
                pDst[0] = (byte) ch;
                pDst++;
            Label_01F4:
                pSrc++;
                goto Label_002B;
            Label_01FF:
                this.bufPos = (int) ((long) ((pDst - numRef) / 1));
                this.textPos = this.bufPos;
                this.contentPos = 0;
            }
        }

        public override void WriteEndAttribute()
        {
            this.bufBytes[this.bufPos++] = 0x22;
            this.inAttributeValue = false;
            this.attrEndPos = this.bufPos;
        }

        internal override void WriteEndElement(string prefix, string localName, string ns)
        {
            if (this.contentPos != this.bufPos)
            {
                this.bufBytes[this.bufPos++] = 60;
                this.bufBytes[this.bufPos++] = 0x2f;
                if ((prefix != null) && (prefix.Length != 0))
                {
                    this.RawText(prefix);
                    this.bufBytes[this.bufPos++] = 0x3a;
                }
                this.RawText(localName);
                this.bufBytes[this.bufPos++] = 0x3e;
            }
            else
            {
                this.bufPos--;
                this.bufBytes[this.bufPos++] = 0x20;
                this.bufBytes[this.bufPos++] = 0x2f;
                this.bufBytes[this.bufPos++] = 0x3e;
            }
        }

        internal override void WriteEndNamespaceDeclaration()
        {
            this.inAttributeValue = false;
            this.bufBytes[this.bufPos++] = 0x22;
            this.attrEndPos = this.bufPos;
        }

        public override void WriteEntityRef(string name)
        {
            this.bufBytes[this.bufPos++] = 0x26;
            this.RawText(name);
            this.bufBytes[this.bufPos++] = 0x3b;
            if (this.bufPos > this.bufLen)
            {
                this.FlushBuffer();
            }
            this.textPos = this.bufPos;
        }

        internal override void WriteFullEndElement(string prefix, string localName, string ns)
        {
            this.bufBytes[this.bufPos++] = 60;
            this.bufBytes[this.bufPos++] = 0x2f;
            if ((prefix != null) && (prefix.Length != 0))
            {
                this.RawText(prefix);
                this.bufBytes[this.bufPos++] = 0x3a;
            }
            this.RawText(localName);
            this.bufBytes[this.bufPos++] = 0x3e;
        }

        internal override void WriteNamespaceDeclaration(string prefix, string namespaceName)
        {
            this.WriteStartNamespaceDeclaration(prefix);
            this.WriteString(namespaceName);
            this.WriteEndNamespaceDeclaration();
        }

        protected unsafe byte* WriteNewLine(byte* pDst)
        {
            fixed (byte* numRef = this.bufBytes)
            {
                this.bufPos = (int) ((long) ((pDst - numRef) / 1));
                this.RawText(this.newLineChars);
                return (numRef + this.bufPos);
            }
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this.bufBytes[this.bufPos++] = 60;
            this.bufBytes[this.bufPos++] = 0x3f;
            this.RawText(name);
            if (text.Length > 0)
            {
                this.bufBytes[this.bufPos++] = 0x20;
                this.WriteCommentOrPi(text, 0x3f);
            }
            this.bufBytes[this.bufPos++] = 0x3f;
            this.bufBytes[this.bufPos++] = 0x3e;
        }

        public override unsafe void WriteRaw(string data)
        {
            fixed (char* str = ((char*) data))
            {
                char* pSrcBegin = str;
                this.WriteRawWithCharChecking(pSrcBegin, pSrcBegin + data.Length);
            }
            this.textPos = this.bufPos;
        }

        public override unsafe void WriteRaw(char[] buffer, int index, int count)
        {
            fixed (char* chRef = &(buffer[index]))
            {
                this.WriteRawWithCharChecking(chRef, chRef + count);
            }
            this.textPos = this.bufPos;
        }

        protected unsafe void WriteRawWithCharChecking(char* pSrcBegin, char* pSrcEnd)
        {
            fixed (byte* numRef = this.bufBytes)
            {
                byte* numPtr2;
                char* pSrc = pSrcBegin;
                byte* pDst = numRef + this.bufPos;
                int ch = 0;
            Label_002D:
                numPtr2 = pDst + ((byte*) ((long) ((pSrcEnd - pSrc) / 2)));
                if (numPtr2 > (numRef + this.bufLen))
                {
                    numPtr2 = numRef + this.bufLen;
                }
                while (((pDst < numPtr2) && ((this.xmlCharType.charProperties[ch = pSrc[0]] & 0x40) != 0)) && (ch <= 0x7f))
                {
                    pDst[0] = (byte) ch;
                    pDst++;
                    pSrc++;
                }
                if (pSrc >= pSrcEnd)
                {
                    goto Label_0191;
                }
                if (pDst >= numPtr2)
                {
                    this.bufPos = (int) ((long) ((pDst - numRef) / 1));
                    this.FlushBuffer();
                    pDst = numRef + 1;
                    goto Label_002D;
                }
                switch (ch)
                {
                    case 60:
                    case 0x5d:
                    case 9:
                    case 0x26:
                        pDst[0] = (byte) ch;
                        pDst++;
                        goto Label_0187;

                    case 10:
                        if (this.newLineHandling != NewLineHandling.Replace)
                        {
                            break;
                        }
                        pDst = this.WriteNewLine(pDst);
                        goto Label_0187;

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
                            pDst[0] = (byte) ch;
                            pDst++;
                        }
                        goto Label_0187;

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
                            pDst = EncodeMultibyteUTF8(ch, pDst);
                            pSrc++;
                        }
                        goto Label_002D;
                }
                pDst[0] = (byte) ch;
                pDst++;
            Label_0187:
                pSrc++;
                goto Label_002D;
            Label_0191:
                this.bufPos = (int) ((long) ((pDst - numRef) / 1));
            }
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            if (this.attrEndPos == this.bufPos)
            {
                this.bufBytes[this.bufPos++] = 0x20;
            }
            if ((prefix != null) && (prefix.Length > 0))
            {
                this.RawText(prefix);
                this.bufBytes[this.bufPos++] = 0x3a;
            }
            this.RawText(localName);
            this.bufBytes[this.bufPos++] = 0x3d;
            this.bufBytes[this.bufPos++] = 0x22;
            this.inAttributeValue = true;
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.bufBytes[this.bufPos++] = 60;
            if ((prefix != null) && (prefix.Length != 0))
            {
                this.RawText(prefix);
                this.bufBytes[this.bufPos++] = 0x3a;
            }
            this.RawText(localName);
            this.attrEndPos = this.bufPos;
        }

        internal override void WriteStartNamespaceDeclaration(string prefix)
        {
            if (prefix.Length == 0)
            {
                this.RawText(" xmlns=\"");
            }
            else
            {
                this.RawText(" xmlns:");
                this.RawText(prefix);
                this.bufBytes[this.bufPos++] = 0x3d;
                this.bufBytes[this.bufPos++] = 0x22;
            }
            this.inAttributeValue = true;
        }

        public override unsafe void WriteString(string text)
        {
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
            int num = XmlCharType.CombineSurrogateChar(lowChar, highChar);
            this.bufBytes[this.bufPos++] = 0x26;
            this.bufBytes[this.bufPos++] = 0x23;
            this.bufBytes[this.bufPos++] = 120;
            this.RawText(num.ToString("X", NumberFormatInfo.InvariantInfo));
            this.bufBytes[this.bufPos++] = 0x3b;
            this.textPos = this.bufPos;
        }

        public override unsafe void WriteWhitespace(string ws)
        {
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

