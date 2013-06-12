namespace System.Xml
{
    using System;
    using System.IO;

    internal class HtmlEncodedRawTextWriter : XmlEncodedRawTextWriter
    {
        protected static TernaryTreeReadOnly attributePropertySearch;
        private AttributeProperties currentAttributeProperties;
        protected ElementProperties currentElementProperties;
        protected static TernaryTreeReadOnly elementPropertySearch;
        protected ByteStack elementScope;
        private bool endsWithAmpersand;
        private string mediaType;
        private const int StackIncrement = 10;
        private byte[] uriEscapingBuffer;

        public HtmlEncodedRawTextWriter(Stream stream, XmlWriterSettings settings) : base(stream, settings)
        {
            this.Init(settings);
        }

        public HtmlEncodedRawTextWriter(TextWriter writer, XmlWriterSettings settings) : base(writer, settings)
        {
            this.Init(settings);
        }

        private void Init(XmlWriterSettings settings)
        {
            if (elementPropertySearch == null)
            {
                attributePropertySearch = new TernaryTreeReadOnly(HtmlTernaryTree.htmlAttributes);
                elementPropertySearch = new TernaryTreeReadOnly(HtmlTernaryTree.htmlElements);
            }
            this.elementScope = new ByteStack(10);
            this.uriEscapingBuffer = new byte[5];
            this.currentElementProperties = ElementProperties.DEFAULT;
            this.mediaType = settings.MediaType;
        }

        private void OutputRestAmps()
        {
            base.bufChars[base.bufPos++] = 'a';
            base.bufChars[base.bufPos++] = 'm';
            base.bufChars[base.bufPos++] = 'p';
            base.bufChars[base.bufPos++] = ';';
        }

        internal override void StartElementContent()
        {
            base.bufChars[base.bufPos++] = '>';
            base.contentPos = base.bufPos;
            if ((this.currentElementProperties & ElementProperties.HEAD) != ElementProperties.DEFAULT)
            {
                this.WriteMetaElement();
            }
        }

        public override void WriteCharEntity(char ch)
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        public override unsafe void WriteChars(char[] buffer, int index, int count)
        {
            if (base.trackTextContent && !base.inTextContent)
            {
                base.ChangeTextContentMark(true);
            }
            fixed (char* chRef = &(buffer[index]))
            {
                if (base.inAttributeValue)
                {
                    base.WriteAttributeTextBlock(chRef, chRef + count);
                }
                else
                {
                    base.WriteElementTextBlock(chRef, chRef + count);
                }
            }
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            if (base.trackTextContent && base.inTextContent)
            {
                base.ChangeTextContentMark(false);
            }
            base.RawText("<!DOCTYPE ");
            if (name == "HTML")
            {
                base.RawText("HTML");
            }
            else
            {
                base.RawText("html");
            }
            if (pubid != null)
            {
                base.RawText(" PUBLIC \"");
                base.RawText(pubid);
                if (sysid != null)
                {
                    base.RawText("\" \"");
                    base.RawText(sysid);
                }
                base.bufChars[base.bufPos++] = '"';
            }
            else if (sysid != null)
            {
                base.RawText(" SYSTEM \"");
                base.RawText(sysid);
                base.bufChars[base.bufPos++] = '"';
            }
            else
            {
                base.bufChars[base.bufPos++] = ' ';
            }
            if (subset != null)
            {
                base.bufChars[base.bufPos++] = '[';
                base.RawText(subset);
                base.bufChars[base.bufPos++] = ']';
            }
            base.bufChars[base.bufPos++] = '>';
        }

        public override void WriteEndAttribute()
        {
            if ((this.currentAttributeProperties & AttributeProperties.BOOLEAN) != AttributeProperties.DEFAULT)
            {
                base.attrEndPos = base.bufPos;
            }
            else
            {
                if (this.endsWithAmpersand)
                {
                    this.OutputRestAmps();
                    this.endsWithAmpersand = false;
                }
                if (base.trackTextContent && base.inTextContent)
                {
                    base.ChangeTextContentMark(false);
                }
                base.bufChars[base.bufPos++] = '"';
            }
            base.inAttributeValue = false;
            base.attrEndPos = base.bufPos;
        }

        internal override void WriteEndElement(string prefix, string localName, string ns)
        {
            if (ns.Length == 0)
            {
                if (base.trackTextContent && base.inTextContent)
                {
                    base.ChangeTextContentMark(false);
                }
                if ((this.currentElementProperties & ElementProperties.EMPTY) == ElementProperties.DEFAULT)
                {
                    base.bufChars[base.bufPos++] = '<';
                    base.bufChars[base.bufPos++] = '/';
                    base.RawText(localName);
                    base.bufChars[base.bufPos++] = '>';
                }
            }
            else
            {
                base.WriteEndElement(prefix, localName, ns);
            }
            this.currentElementProperties = (ElementProperties) this.elementScope.Pop();
        }

        public override void WriteEntityRef(string name)
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        internal override void WriteFullEndElement(string prefix, string localName, string ns)
        {
            if (ns.Length == 0)
            {
                if (base.trackTextContent && base.inTextContent)
                {
                    base.ChangeTextContentMark(false);
                }
                if ((this.currentElementProperties & ElementProperties.EMPTY) == ElementProperties.DEFAULT)
                {
                    base.bufChars[base.bufPos++] = '<';
                    base.bufChars[base.bufPos++] = '/';
                    base.RawText(localName);
                    base.bufChars[base.bufPos++] = '>';
                }
            }
            else
            {
                base.WriteFullEndElement(prefix, localName, ns);
            }
            this.currentElementProperties = (ElementProperties) this.elementScope.Pop();
        }

        private unsafe void WriteHtmlAttributeText(char* pSrc, char* pSrcEnd)
        {
            if (this.endsWithAmpersand)
            {
                if ((((long) ((pSrcEnd - pSrc) / 2)) > 0L) && (pSrc[0] != '{'))
                {
                    this.OutputRestAmps();
                }
                this.endsWithAmpersand = false;
            }
            fixed (char* chRef = base.bufChars)
            {
                char* chPtr2;
                char* pDst = chRef + base.bufPos;
                char ch = '\0';
            Label_0053:
                chPtr2 = pDst + ((char*) (((long) ((pSrcEnd - pSrc) / 2)) * 2L));
                if (chPtr2 > (chRef + base.bufLen))
                {
                    chPtr2 = chRef + base.bufLen;
                }
                while ((pDst < chPtr2) && ((this.xmlCharType.charProperties[ch = pSrc[0]] & 0x80) != 0))
                {
                    pDst++;
                    pDst[0] = ch;
                    pSrc++;
                }
                if (pSrc >= pSrcEnd)
                {
                    goto Label_0195;
                }
                if (pDst >= chPtr2)
                {
                    base.bufPos = (int) ((long) ((pDst - chRef) / 2));
                    this.FlushBuffer();
                    pDst = chRef + 1;
                    goto Label_0053;
                }
                switch (ch)
                {
                    case '\t':
                    case '\'':
                    case '<':
                    case '>':
                        pDst++;
                        pDst[0] = ch;
                        goto Label_018A;

                    case '\n':
                        pDst = XmlEncodedRawTextWriter.LineFeedEntity(pDst);
                        goto Label_018A;

                    case '\r':
                        pDst = XmlEncodedRawTextWriter.CarriageReturnEntity(pDst);
                        goto Label_018A;

                    case '"':
                        pDst = XmlEncodedRawTextWriter.QuoteEntity(pDst);
                        goto Label_018A;

                    case '&':
                        if ((pSrc + 1) != pSrcEnd)
                        {
                            break;
                        }
                        this.endsWithAmpersand = true;
                        goto Label_014B;

                    default:
                        base.EncodeChar(ref pSrc, pSrcEnd, ref pDst);
                        goto Label_0053;
                }
                if (pSrc[1] != '{')
                {
                    pDst = XmlEncodedRawTextWriter.AmpEntity(pDst);
                    goto Label_018A;
                }
            Label_014B:
                pDst++;
                pDst[0] = ch;
            Label_018A:
                pSrc++;
                goto Label_0053;
            Label_0195:
                base.bufPos = (int) ((long) ((pDst - chRef) / 2));
            }
        }

        protected unsafe void WriteHtmlAttributeTextBlock(char* pSrc, char* pSrcEnd)
        {
            if ((this.currentAttributeProperties & (AttributeProperties.BOOLEAN | AttributeProperties.NAME | AttributeProperties.URI)) != AttributeProperties.DEFAULT)
            {
                if ((this.currentAttributeProperties & AttributeProperties.BOOLEAN) == AttributeProperties.DEFAULT)
                {
                    if ((this.currentAttributeProperties & (AttributeProperties.DEFAULT | AttributeProperties.NAME | AttributeProperties.URI)) != AttributeProperties.DEFAULT)
                    {
                        this.WriteUriAttributeText(pSrc, pSrcEnd);
                    }
                    else
                    {
                        this.WriteHtmlAttributeText(pSrc, pSrcEnd);
                    }
                }
            }
            else if ((this.currentElementProperties & ElementProperties.HAS_NS) != ElementProperties.DEFAULT)
            {
                base.WriteAttributeTextBlock(pSrc, pSrcEnd);
            }
            else
            {
                this.WriteHtmlAttributeText(pSrc, pSrcEnd);
            }
        }

        protected unsafe void WriteHtmlElementTextBlock(char* pSrc, char* pSrcEnd)
        {
            if ((this.currentElementProperties & ElementProperties.NO_ENTITIES) != ElementProperties.DEFAULT)
            {
                base.RawText(pSrc, pSrcEnd);
            }
            else
            {
                base.WriteElementTextBlock(pSrc, pSrcEnd);
            }
        }

        protected void WriteMetaElement()
        {
            base.RawText("<META http-equiv=\"Content-Type\"");
            if (this.mediaType == null)
            {
                this.mediaType = "text/html";
            }
            base.RawText(" content=\"");
            base.RawText(this.mediaType);
            base.RawText("; charset=");
            base.RawText(base.encoding.WebName);
            base.RawText("\">");
        }

        public override void WriteProcessingInstruction(string target, string text)
        {
            if (base.trackTextContent && base.inTextContent)
            {
                base.ChangeTextContentMark(false);
            }
            base.bufChars[base.bufPos++] = '<';
            base.bufChars[base.bufPos++] = '?';
            base.RawText(target);
            base.bufChars[base.bufPos++] = ' ';
            base.WriteCommentOrPi(text, 0x3f);
            base.bufChars[base.bufPos++] = '>';
            if (base.bufPos > base.bufLen)
            {
                this.FlushBuffer();
            }
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            if (ns.Length == 0)
            {
                if (base.trackTextContent && base.inTextContent)
                {
                    base.ChangeTextContentMark(false);
                }
                if (base.attrEndPos == base.bufPos)
                {
                    base.bufChars[base.bufPos++] = ' ';
                }
                base.RawText(localName);
                if ((this.currentElementProperties & (ElementProperties.BOOL_PARENT | ElementProperties.NAME_PARENT | ElementProperties.URI_PARENT)) != ElementProperties.DEFAULT)
                {
                    this.currentAttributeProperties = ((AttributeProperties) attributePropertySearch.FindCaseInsensitiveString(localName)) & ((AttributeProperties) this.currentElementProperties);
                    if ((this.currentAttributeProperties & AttributeProperties.BOOLEAN) != AttributeProperties.DEFAULT)
                    {
                        base.inAttributeValue = true;
                        return;
                    }
                }
                else
                {
                    this.currentAttributeProperties = AttributeProperties.DEFAULT;
                }
                base.bufChars[base.bufPos++] = '=';
                base.bufChars[base.bufPos++] = '"';
            }
            else
            {
                base.WriteStartAttribute(prefix, localName, ns);
                this.currentAttributeProperties = AttributeProperties.DEFAULT;
            }
            base.inAttributeValue = true;
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.elementScope.Push((byte) this.currentElementProperties);
            if (ns.Length == 0)
            {
                if (base.trackTextContent && base.inTextContent)
                {
                    base.ChangeTextContentMark(false);
                }
                this.currentElementProperties = (ElementProperties) elementPropertySearch.FindCaseInsensitiveString(localName);
                base.bufChars[base.bufPos++] = '<';
                base.RawText(localName);
                base.attrEndPos = base.bufPos;
            }
            else
            {
                this.currentElementProperties = ElementProperties.HAS_NS;
                base.WriteStartElement(prefix, localName, ns);
            }
        }

        public override unsafe void WriteString(string text)
        {
            if (base.trackTextContent && !base.inTextContent)
            {
                base.ChangeTextContentMark(true);
            }
            fixed (char* str = ((char*) text))
            {
                char* pSrc = str;
                char* pSrcEnd = pSrc + text.Length;
                if (base.inAttributeValue)
                {
                    this.WriteHtmlAttributeTextBlock(pSrc, pSrcEnd);
                }
                else
                {
                    this.WriteHtmlElementTextBlock(pSrc, pSrcEnd);
                }
            }
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        private unsafe void WriteUriAttributeText(char* pSrc, char* pSrcEnd)
        {
            if (this.endsWithAmpersand)
            {
                if ((((long) ((pSrcEnd - pSrc) / 2)) > 0L) && (pSrc[0] != '{'))
                {
                    this.OutputRestAmps();
                }
                this.endsWithAmpersand = false;
            }
            fixed (char* chRef = base.bufChars)
            {
                char* chPtr2;
                char* pDst = chRef + base.bufPos;
                char ch = '\0';
            Label_0053:
                chPtr2 = pDst + ((char*) (((long) ((pSrcEnd - pSrc) / 2)) * 2L));
                if (chPtr2 > (chRef + base.bufLen))
                {
                    chPtr2 = chRef + base.bufLen;
                }
                while (((pDst < chPtr2) && ((this.xmlCharType.charProperties[ch = pSrc[0]] & 0x80) != 0)) && (ch < '\x0080'))
                {
                    pDst++;
                    pDst[0] = ch;
                    pSrc++;
                }
                if (pSrc >= pSrcEnd)
                {
                    goto Label_0227;
                }
                if (pDst >= chPtr2)
                {
                    base.bufPos = (int) ((long) ((pDst - chRef) / 2));
                    this.FlushBuffer();
                    pDst = chRef + 1;
                    goto Label_0053;
                }
                switch (ch)
                {
                    case '\t':
                    case '\'':
                    case '<':
                    case '>':
                        pDst++;
                        pDst[0] = ch;
                        goto Label_021C;

                    case '\n':
                        pDst = XmlEncodedRawTextWriter.LineFeedEntity(pDst);
                        goto Label_021C;

                    case '\r':
                        pDst = XmlEncodedRawTextWriter.CarriageReturnEntity(pDst);
                        goto Label_021C;

                    case '"':
                        pDst = XmlEncodedRawTextWriter.QuoteEntity(pDst);
                        goto Label_021C;

                    case '&':
                        if ((pSrc + 1) != pSrcEnd)
                        {
                            break;
                        }
                        this.endsWithAmpersand = true;
                        goto Label_015C;

                    default:
                        fixed (byte* numRef = this.uriEscapingBuffer)
                        {
                            byte* numPtr = numRef;
                            byte* numPtr2 = numPtr;
                            XmlUtf8RawTextWriter.CharToUTF8(ref pSrc, pSrcEnd, ref numPtr2);
                            while (numPtr < numPtr2)
                            {
                                pDst++;
                                pDst[0] = '%';
                                pDst++;
                                pDst[0] = "0123456789ABCDEF"[numPtr[0] >> 4];
                                pDst++;
                                pDst[0] = "0123456789ABCDEF"[numPtr[0] & 15];
                                numPtr++;
                            }
                        }
                        goto Label_0053;
                }
                if (pSrc[1] != '{')
                {
                    pDst = XmlEncodedRawTextWriter.AmpEntity(pDst);
                    goto Label_021C;
                }
            Label_015C:
                pDst++;
                pDst[0] = ch;
            Label_021C:
                pSrc++;
                goto Label_0053;
            Label_0227:
                base.bufPos = (int) ((long) ((pDst - chRef) / 2));
            }
        }

        internal override void WriteXmlDeclaration(string xmldecl)
        {
        }

        internal override void WriteXmlDeclaration(XmlStandalone standalone)
        {
        }
    }
}

