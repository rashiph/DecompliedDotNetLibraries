namespace System.Xml
{
    using System;
    using System.IO;

    internal class HtmlUtf8RawTextWriter : XmlUtf8RawTextWriter
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

        public HtmlUtf8RawTextWriter(Stream stream, XmlWriterSettings settings) : base(stream, settings)
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
            base.bufBytes[base.bufPos++] = 0x61;
            base.bufBytes[base.bufPos++] = 0x6d;
            base.bufBytes[base.bufPos++] = 0x70;
            base.bufBytes[base.bufPos++] = 0x3b;
        }

        internal override void StartElementContent()
        {
            base.bufBytes[base.bufPos++] = 0x3e;
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
                base.bufBytes[base.bufPos++] = 0x22;
            }
            else if (sysid != null)
            {
                base.RawText(" SYSTEM \"");
                base.RawText(sysid);
                base.bufBytes[base.bufPos++] = 0x22;
            }
            else
            {
                base.bufBytes[base.bufPos++] = 0x20;
            }
            if (subset != null)
            {
                base.bufBytes[base.bufPos++] = 0x5b;
                base.RawText(subset);
                base.bufBytes[base.bufPos++] = 0x5d;
            }
            base.bufBytes[base.bufPos++] = 0x3e;
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
                base.bufBytes[base.bufPos++] = 0x22;
            }
            base.inAttributeValue = false;
            base.attrEndPos = base.bufPos;
        }

        internal override void WriteEndElement(string prefix, string localName, string ns)
        {
            if (ns.Length == 0)
            {
                if ((this.currentElementProperties & ElementProperties.EMPTY) == ElementProperties.DEFAULT)
                {
                    base.bufBytes[base.bufPos++] = 60;
                    base.bufBytes[base.bufPos++] = 0x2f;
                    base.RawText(localName);
                    base.bufBytes[base.bufPos++] = 0x3e;
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
                if ((this.currentElementProperties & ElementProperties.EMPTY) == ElementProperties.DEFAULT)
                {
                    base.bufBytes[base.bufPos++] = 60;
                    base.bufBytes[base.bufPos++] = 0x2f;
                    base.RawText(localName);
                    base.bufBytes[base.bufPos++] = 0x3e;
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
            fixed (byte* numRef = base.bufBytes)
            {
                byte* numPtr2;
                byte* pDst = numRef + base.bufPos;
                char ch = '\0';
            Label_0050:
                numPtr2 = pDst + ((byte*) ((long) ((pSrcEnd - pSrc) / 2)));
                if (numPtr2 > (numRef + base.bufLen))
                {
                    numPtr2 = numRef + base.bufLen;
                }
                while (((pDst < numPtr2) && ((this.xmlCharType.charProperties[ch = pSrc[0]] & 0x80) != 0)) && (ch <= '\x007f'))
                {
                    pDst++;
                    pDst[0] = (byte) ch;
                    pSrc++;
                }
                if (pSrc >= pSrcEnd)
                {
                    goto Label_0191;
                }
                if (pDst >= numPtr2)
                {
                    base.bufPos = (int) ((long) ((pDst - numRef) / 1));
                    this.FlushBuffer();
                    pDst = numRef + 1;
                    goto Label_0050;
                }
                switch (ch)
                {
                    case '\t':
                    case '\'':
                    case '<':
                    case '>':
                        pDst++;
                        pDst[0] = (byte) ch;
                        goto Label_0186;

                    case '\n':
                        pDst = XmlUtf8RawTextWriter.LineFeedEntity(pDst);
                        goto Label_0186;

                    case '\r':
                        pDst = XmlUtf8RawTextWriter.CarriageReturnEntity(pDst);
                        goto Label_0186;

                    case '"':
                        pDst = XmlUtf8RawTextWriter.QuoteEntity(pDst);
                        goto Label_0186;

                    case '&':
                        if ((pSrc + 1) != pSrcEnd)
                        {
                            break;
                        }
                        this.endsWithAmpersand = true;
                        goto Label_0145;

                    default:
                        base.EncodeChar(ref pSrc, pSrcEnd, ref pDst);
                        goto Label_0050;
                }
                if (pSrc[1] != '{')
                {
                    pDst = XmlUtf8RawTextWriter.AmpEntity(pDst);
                    goto Label_0186;
                }
            Label_0145:
                pDst++;
                pDst[0] = (byte) ch;
            Label_0186:
                pSrc++;
                goto Label_0050;
            Label_0191:
                base.bufPos = (int) ((long) ((pDst - numRef) / 1));
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
            base.bufBytes[base.bufPos++] = 60;
            base.bufBytes[base.bufPos++] = 0x3f;
            base.RawText(target);
            base.bufBytes[base.bufPos++] = 0x20;
            base.WriteCommentOrPi(text, 0x3f);
            base.bufBytes[base.bufPos++] = 0x3e;
            if (base.bufPos > base.bufLen)
            {
                this.FlushBuffer();
            }
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            if (ns.Length == 0)
            {
                if (base.attrEndPos == base.bufPos)
                {
                    base.bufBytes[base.bufPos++] = 0x20;
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
                base.bufBytes[base.bufPos++] = 0x3d;
                base.bufBytes[base.bufPos++] = 0x22;
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
                this.currentElementProperties = (ElementProperties) elementPropertySearch.FindCaseInsensitiveString(localName);
                base.bufBytes[base.bufPos++] = 60;
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
            fixed (byte* numRef = base.bufBytes)
            {
                byte* numPtr2;
                byte* pDst = numRef + base.bufPos;
                char ch = '\0';
            Label_0050:
                numPtr2 = pDst + ((byte*) ((long) ((pSrcEnd - pSrc) / 2)));
                if (numPtr2 > (numRef + base.bufLen))
                {
                    numPtr2 = numRef + base.bufLen;
                }
                while (((pDst < numPtr2) && ((this.xmlCharType.charProperties[ch = pSrc[0]] & 0x80) != 0)) && (ch < '\x0080'))
                {
                    pDst++;
                    pDst[0] = (byte) ch;
                    pSrc++;
                }
                if (pSrc >= pSrcEnd)
                {
                    goto Label_021D;
                }
                if (pDst >= numPtr2)
                {
                    base.bufPos = (int) ((long) ((pDst - numRef) / 1));
                    this.FlushBuffer();
                    pDst = numRef + 1;
                    goto Label_0050;
                }
                switch (ch)
                {
                    case '\t':
                    case '\'':
                    case '<':
                    case '>':
                        pDst++;
                        pDst[0] = (byte) ch;
                        goto Label_0212;

                    case '\n':
                        pDst = XmlUtf8RawTextWriter.LineFeedEntity(pDst);
                        goto Label_0212;

                    case '\r':
                        pDst = XmlUtf8RawTextWriter.CarriageReturnEntity(pDst);
                        goto Label_0212;

                    case '"':
                        pDst = XmlUtf8RawTextWriter.QuoteEntity(pDst);
                        goto Label_0212;

                    case '&':
                        if ((pSrc + 1) != pSrcEnd)
                        {
                            break;
                        }
                        this.endsWithAmpersand = true;
                        goto Label_014E;

                    default:
                        fixed (byte* numRef2 = this.uriEscapingBuffer)
                        {
                            byte* numPtr3 = numRef2;
                            byte* numPtr4 = numPtr3;
                            XmlUtf8RawTextWriter.CharToUTF8(ref pSrc, pSrcEnd, ref numPtr4);
                            while (numPtr3 < numPtr4)
                            {
                                pDst++;
                                pDst[0] = 0x25;
                                pDst++;
                                pDst[0] = (byte) "0123456789ABCDEF"[numPtr3[0] >> 4];
                                pDst++;
                                pDst[0] = (byte) "0123456789ABCDEF"[numPtr3[0] & 15];
                                numPtr3++;
                            }
                        }
                        goto Label_0050;
                }
                if (pSrc[1] != '{')
                {
                    pDst = XmlUtf8RawTextWriter.AmpEntity(pDst);
                    goto Label_0212;
                }
            Label_014E:
                pDst++;
                pDst[0] = (byte) ch;
            Label_0212:
                pSrc++;
                goto Label_0050;
            Label_021D:
                base.bufPos = (int) ((long) ((pDst - numRef) / 1));
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

