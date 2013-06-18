namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;

    internal class XmlMtomReader : XmlDictionaryReader, IXmlLineInfo, IXmlMtomReaderInitializer
    {
        private int bufferRemaining;
        private Encoding[] encodings;
        private XmlDictionaryReader infosetReader;
        private int maxBufferSize;
        private Dictionary<string, MimePart> mimeParts;
        private MimeReader mimeReader;
        private OnXmlDictionaryReaderClose onClose;
        private MimePart part;
        private bool readingBinaryElement;
        private XmlDictionaryReader xmlReader;

        private void AdvanceToContentOnElement()
        {
            if (this.NodeType != XmlNodeType.Attribute)
            {
                this.MoveToContent();
            }
        }

        private void CheckContentTransferEncodingOnBinaryPart(ContentTransferEncodingHeader header)
        {
            if (header == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomContentTransferEncodingNotPresent", new object[] { ContentTransferEncodingHeader.Binary.ContentTransferEncodingValue })));
            }
            if (header.ContentTransferEncoding != ContentTransferEncoding.Binary)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomInvalidTransferEncodingForMimePart", new object[] { header.Value, ContentTransferEncodingHeader.Binary.ContentTransferEncodingValue })));
            }
        }

        private void CheckContentTransferEncodingOnRoot(ContentTransferEncodingHeader header)
        {
            if ((header != null) && (header.ContentTransferEncoding == ContentTransferEncoding.Other))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomContentTransferEncodingNotSupported", new object[] { header.Value, ContentTransferEncodingHeader.SevenBit.ContentTransferEncodingValue, ContentTransferEncodingHeader.EightBit.ContentTransferEncodingValue, ContentTransferEncodingHeader.Binary.ContentTransferEncodingValue })));
            }
        }

        private void CheckContentType(string contentType)
        {
            if ((contentType != null) && (contentType.Length == 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("MtomContentTypeInvalid"), "contentType"));
            }
        }

        public override void Close()
        {
            this.xmlReader.Close();
            this.mimeReader.Close();
            OnXmlDictionaryReaderClose onClose = this.onClose;
            this.onClose = null;
            if (onClose != null)
            {
                try
                {
                    onClose(this);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        internal static void DecrementBufferQuota(int maxBuffer, ref int remaining, int size)
        {
            if ((remaining - size) <= 0)
            {
                remaining = 0;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomBufferQuotaExceeded", new object[] { maxBuffer })));
            }
            remaining -= size;
        }

        public override string GetAttribute(int index)
        {
            return this.xmlReader.GetAttribute(index);
        }

        public override string GetAttribute(string name)
        {
            return this.xmlReader.GetAttribute(name);
        }

        public override string GetAttribute(string name, string ns)
        {
            return this.xmlReader.GetAttribute(name, ns);
        }

        public override string GetAttribute(XmlDictionaryString localName, XmlDictionaryString ns)
        {
            return this.xmlReader.GetAttribute(localName, ns);
        }

        private string GetStartUri(string startUri)
        {
            if (startUri.StartsWith("<", StringComparison.Ordinal))
            {
                if (!startUri.EndsWith(">", StringComparison.Ordinal))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomInvalidStartUri", new object[] { startUri })));
                }
                return startUri;
            }
            return string.Format(CultureInfo.InvariantCulture, "<{0}>", new object[] { startUri });
        }

        public bool HasLineInfo()
        {
            if (this.xmlReader.ReadState == System.Xml.ReadState.Closed)
            {
                return false;
            }
            IXmlLineInfo xmlReader = this.xmlReader as IXmlLineInfo;
            if (xmlReader == null)
            {
                return false;
            }
            return xmlReader.HasLineInfo();
        }

        private void Initialize(Stream stream, string contentType, XmlDictionaryReaderQuotas quotas, int maxBufferSize)
        {
            string str;
            string str2;
            string str3;
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            this.maxBufferSize = maxBufferSize;
            this.bufferRemaining = maxBufferSize;
            if (contentType == null)
            {
                MimeMessageReader reader = new MimeMessageReader(stream);
                MimeHeaders headers = reader.ReadHeaders(this.maxBufferSize, ref this.bufferRemaining);
                this.ReadMessageMimeVersionHeader(headers.MimeVersion);
                this.ReadMessageContentTypeHeader(headers.ContentType, out str, out str2, out str3);
                stream = reader.GetContentStream();
                if (stream == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomMessageInvalidContent")));
                }
            }
            else
            {
                this.ReadMessageContentTypeHeader(new ContentTypeHeader(contentType), out str, out str2, out str3);
            }
            this.mimeReader = new MimeReader(stream, str);
            this.mimeParts = null;
            this.readingBinaryElement = false;
            MimePart part = (str2 == null) ? this.ReadRootMimePart() : this.ReadMimePart(this.GetStartUri(str2));
            byte[] buffer = part.GetBuffer(this.maxBufferSize, ref this.bufferRemaining);
            int length = (int) part.Length;
            Encoding encoding = this.ReadRootContentTypeHeader(part.Headers.ContentType, this.encodings, str3);
            this.CheckContentTransferEncodingOnRoot(part.Headers.ContentTransferEncoding);
            IXmlTextReaderInitializer xmlReader = this.xmlReader as IXmlTextReaderInitializer;
            if (xmlReader != null)
            {
                xmlReader.SetInput(buffer, 0, length, encoding, quotas, null);
            }
            else
            {
                this.xmlReader = XmlDictionaryReader.CreateTextReader(buffer, 0, length, encoding, quotas, null);
            }
        }

        public override bool IsLocalName(string localName)
        {
            return this.xmlReader.IsLocalName(localName);
        }

        public override bool IsLocalName(XmlDictionaryString localName)
        {
            return this.xmlReader.IsLocalName(localName);
        }

        public override bool IsNamespaceUri(string ns)
        {
            return this.xmlReader.IsNamespaceUri(ns);
        }

        public override bool IsNamespaceUri(XmlDictionaryString ns)
        {
            return this.xmlReader.IsNamespaceUri(ns);
        }

        public override bool IsStartElement()
        {
            return this.xmlReader.IsStartElement();
        }

        public override bool IsStartElement(string localName)
        {
            return this.xmlReader.IsStartElement(localName);
        }

        public override bool IsStartElement(string localName, string ns)
        {
            return this.xmlReader.IsStartElement(localName, ns);
        }

        public override bool IsStartElement(XmlDictionaryString localName, XmlDictionaryString ns)
        {
            return this.xmlReader.IsStartElement(localName, ns);
        }

        public override string LookupNamespace(string ns)
        {
            return this.xmlReader.LookupNamespace(ns);
        }

        public override void MoveToAttribute(int index)
        {
            this.xmlReader.MoveToAttribute(index);
        }

        public override bool MoveToAttribute(string name)
        {
            return this.xmlReader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return this.xmlReader.MoveToAttribute(name, ns);
        }

        public override bool MoveToElement()
        {
            return this.xmlReader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return this.xmlReader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return this.xmlReader.MoveToNextAttribute();
        }

        public override bool Read()
        {
            bool flag = this.xmlReader.Read();
            if (this.xmlReader.NodeType == XmlNodeType.Element)
            {
                XopIncludeReader reader = null;
                if (this.xmlReader.IsStartElement(MtomGlobals.XopIncludeLocalName, MtomGlobals.XopIncludeNamespace))
                {
                    string uri = null;
                    while (this.xmlReader.MoveToNextAttribute())
                    {
                        if ((this.xmlReader.LocalName == MtomGlobals.XopIncludeHrefLocalName) && (this.xmlReader.NamespaceURI == MtomGlobals.XopIncludeHrefNamespace))
                        {
                            uri = this.xmlReader.Value;
                        }
                        else if (this.xmlReader.NamespaceURI == MtomGlobals.XopIncludeNamespace)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomXopIncludeInvalidXopAttributes", new object[] { this.xmlReader.LocalName, MtomGlobals.XopIncludeNamespace })));
                        }
                    }
                    if (uri == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomXopIncludeHrefNotSpecified", new object[] { MtomGlobals.XopIncludeHrefLocalName })));
                    }
                    MimePart part = this.ReadMimePart(uri);
                    this.CheckContentTransferEncodingOnBinaryPart(part.Headers.ContentTransferEncoding);
                    this.part = part;
                    reader = new XopIncludeReader(part, this.xmlReader);
                    reader.Read();
                    this.xmlReader.MoveToElement();
                    if (this.xmlReader.IsEmptyElement)
                    {
                        this.xmlReader.Read();
                    }
                    else
                    {
                        int depth = this.xmlReader.Depth;
                        this.xmlReader.ReadStartElement();
                        while (this.xmlReader.Depth > depth)
                        {
                            if (this.xmlReader.IsStartElement() && (this.xmlReader.NamespaceURI == MtomGlobals.XopIncludeNamespace))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomXopIncludeInvalidXopElement", new object[] { this.xmlReader.LocalName, MtomGlobals.XopIncludeNamespace })));
                            }
                            this.xmlReader.Skip();
                        }
                        this.xmlReader.ReadEndElement();
                    }
                }
                if (reader != null)
                {
                    this.xmlReader.MoveToContent();
                    this.infosetReader = this.xmlReader;
                    this.xmlReader = reader;
                    reader = null;
                }
            }
            if ((this.xmlReader.ReadState == System.Xml.ReadState.EndOfFile) && (this.infosetReader != null))
            {
                if (!flag)
                {
                    flag = this.infosetReader.Read();
                }
                this.part.Release(this.maxBufferSize, ref this.bufferRemaining);
                this.xmlReader = this.infosetReader;
                this.infosetReader = null;
            }
            return flag;
        }

        public override bool ReadAttributeValue()
        {
            return this.xmlReader.ReadAttributeValue();
        }

        public override object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            this.AdvanceToContentOnElement();
            return this.xmlReader.ReadContentAs(returnType, namespaceResolver);
        }

        public override byte[] ReadContentAsBase64()
        {
            this.AdvanceToContentOnElement();
            return this.xmlReader.ReadContentAsBase64();
        }

        public override int ReadContentAsBase64(byte[] buffer, int offset, int count)
        {
            this.AdvanceToContentOnElement();
            return this.xmlReader.ReadContentAsBase64(buffer, offset, count);
        }

        public override int ReadContentAsBinHex(byte[] buffer, int offset, int count)
        {
            this.AdvanceToContentOnElement();
            return this.xmlReader.ReadContentAsBinHex(buffer, offset, count);
        }

        public override bool ReadContentAsBoolean()
        {
            this.AdvanceToContentOnElement();
            return this.xmlReader.ReadContentAsBoolean();
        }

        public override int ReadContentAsChars(char[] chars, int index, int count)
        {
            this.AdvanceToContentOnElement();
            return this.xmlReader.ReadContentAsChars(chars, index, count);
        }

        public override DateTime ReadContentAsDateTime()
        {
            this.AdvanceToContentOnElement();
            return this.xmlReader.ReadContentAsDateTime();
        }

        public override decimal ReadContentAsDecimal()
        {
            this.AdvanceToContentOnElement();
            return this.xmlReader.ReadContentAsDecimal();
        }

        public override double ReadContentAsDouble()
        {
            this.AdvanceToContentOnElement();
            return this.xmlReader.ReadContentAsDouble();
        }

        public override float ReadContentAsFloat()
        {
            this.AdvanceToContentOnElement();
            return this.xmlReader.ReadContentAsFloat();
        }

        public override int ReadContentAsInt()
        {
            this.AdvanceToContentOnElement();
            return this.xmlReader.ReadContentAsInt();
        }

        public override long ReadContentAsLong()
        {
            this.AdvanceToContentOnElement();
            return this.xmlReader.ReadContentAsLong();
        }

        public override object ReadContentAsObject()
        {
            this.AdvanceToContentOnElement();
            return this.xmlReader.ReadContentAsObject();
        }

        public override string ReadContentAsString()
        {
            this.AdvanceToContentOnElement();
            return this.xmlReader.ReadContentAsString();
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int offset, int count)
        {
            if (!this.readingBinaryElement)
            {
                if (this.IsEmptyElement)
                {
                    this.Read();
                    return 0;
                }
                this.ReadStartElement();
                this.readingBinaryElement = true;
            }
            int num = this.ReadContentAsBase64(buffer, offset, count);
            if (num == 0)
            {
                this.ReadEndElement();
                this.readingBinaryElement = false;
            }
            return num;
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int offset, int count)
        {
            if (!this.readingBinaryElement)
            {
                if (this.IsEmptyElement)
                {
                    this.Read();
                    return 0;
                }
                this.ReadStartElement();
                this.readingBinaryElement = true;
            }
            int num = this.ReadContentAsBinHex(buffer, offset, count);
            if (num == 0)
            {
                this.ReadEndElement();
                this.readingBinaryElement = false;
            }
            return num;
        }

        public override string ReadInnerXml()
        {
            return this.xmlReader.ReadInnerXml();
        }

        private void ReadMessageContentTypeHeader(ContentTypeHeader header, out string boundary, out string start, out string startInfo)
        {
            string str;
            if (header == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomMessageContentTypeNotFound")));
            }
            if ((string.Compare(MtomGlobals.MediaType, header.MediaType, StringComparison.OrdinalIgnoreCase) != 0) || (string.Compare(MtomGlobals.MediaSubtype, header.MediaSubtype, StringComparison.OrdinalIgnoreCase) != 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomMessageNotMultipart", new object[] { MtomGlobals.MediaType, MtomGlobals.MediaSubtype })));
            }
            if (!header.Parameters.TryGetValue(MtomGlobals.TypeParam, out str) || (MtomGlobals.XopType != str))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomMessageNotApplicationXopXml", new object[] { MtomGlobals.XopType })));
            }
            if (!header.Parameters.TryGetValue(MtomGlobals.BoundaryParam, out boundary))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomMessageRequiredParamNotSpecified", new object[] { MtomGlobals.BoundaryParam })));
            }
            if (!MailBnfHelper.IsValidMimeBoundary(boundary))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomBoundaryInvalid", new object[] { boundary })));
            }
            if (!header.Parameters.TryGetValue(MtomGlobals.StartParam, out start))
            {
                start = null;
            }
            if (!header.Parameters.TryGetValue(MtomGlobals.StartInfoParam, out startInfo))
            {
                startInfo = null;
            }
        }

        private void ReadMessageMimeVersionHeader(MimeVersionHeader header)
        {
            if ((header != null) && (header.Version != MimeVersionHeader.Default.Version))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomMessageInvalidMimeVersion", new object[] { header.Version, MimeVersionHeader.Default.Version })));
            }
        }

        private MimePart ReadMimePart(string uri)
        {
            MimePart part = null;
            if ((uri == null) || (uri.Length == 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomInvalidEmptyURI")));
            }
            string key = null;
            if (uri.StartsWith(MimeGlobals.ContentIDScheme, StringComparison.Ordinal))
            {
                key = string.Format(CultureInfo.InvariantCulture, "<{0}>", new object[] { Uri.UnescapeDataString(uri.Substring(MimeGlobals.ContentIDScheme.Length)) });
            }
            else if (uri.StartsWith("<", StringComparison.Ordinal))
            {
                key = uri;
            }
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomInvalidCIDUri", new object[] { uri })));
            }
            if ((this.mimeParts == null) || !this.mimeParts.TryGetValue(key, out part))
            {
                while ((part == null) && this.mimeReader.ReadNextPart())
                {
                    MimeHeaders headers = this.mimeReader.ReadHeaders(this.maxBufferSize, ref this.bufferRemaining);
                    Stream contentStream = this.mimeReader.GetContentStream();
                    if (contentStream == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomMessageInvalidContentInMimePart")));
                    }
                    ContentIDHeader header = (headers == null) ? null : headers.ContentID;
                    if ((header == null) || (header.Value == null))
                    {
                        int count = 0x100;
                        byte[] buffer = new byte[count];
                        while (contentStream.Read(buffer, 0, count) > 0)
                        {
                        }
                    }
                    else
                    {
                        string str2 = headers.ContentID.Value;
                        MimePart part2 = new MimePart(contentStream, headers);
                        if (this.mimeParts == null)
                        {
                            this.mimeParts = new Dictionary<string, MimePart>();
                        }
                        this.mimeParts.Add(str2, part2);
                        if (str2.Equals(key))
                        {
                            part = part2;
                        }
                        else
                        {
                            part2.GetBuffer(this.maxBufferSize, ref this.bufferRemaining);
                        }
                    }
                }
                if (part == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomPartNotFound", new object[] { uri })));
                }
            }
            else if (part.ReferencedFromInfoset)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomMimePartReferencedMoreThanOnce", new object[] { key })));
            }
            part.ReferencedFromInfoset = true;
            return part;
        }

        public override string ReadOuterXml()
        {
            return this.xmlReader.ReadOuterXml();
        }

        private Encoding ReadRootContentTypeHeader(ContentTypeHeader header, Encoding[] expectedEncodings, string expectedType)
        {
            string str;
            if (header == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomRootContentTypeNotFound")));
            }
            if ((string.Compare(MtomGlobals.XopMediaType, header.MediaType, StringComparison.OrdinalIgnoreCase) != 0) || (string.Compare(MtomGlobals.XopMediaSubtype, header.MediaSubtype, StringComparison.OrdinalIgnoreCase) != 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomRootNotApplicationXopXml", new object[] { MtomGlobals.XopMediaType, MtomGlobals.XopMediaSubtype })));
            }
            if ((!header.Parameters.TryGetValue(MtomGlobals.CharsetParam, out str) || (str == null)) || (str.Length == 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomRootRequiredParamNotSpecified", new object[] { MtomGlobals.CharsetParam })));
            }
            Encoding encoding = null;
            for (int i = 0; i < this.encodings.Length; i++)
            {
                if (string.Compare(str, expectedEncodings[i].WebName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    encoding = expectedEncodings[i];
                    break;
                }
            }
            if (encoding == null)
            {
                if (string.Compare(str, "utf-16LE", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    for (int j = 0; j < this.encodings.Length; j++)
                    {
                        if (string.Compare(expectedEncodings[j].WebName, Encoding.Unicode.WebName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            encoding = expectedEncodings[j];
                            break;
                        }
                    }
                }
                else if (string.Compare(str, "utf-16BE", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    for (int k = 0; k < this.encodings.Length; k++)
                    {
                        if (string.Compare(expectedEncodings[k].WebName, Encoding.BigEndianUnicode.WebName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            encoding = expectedEncodings[k];
                            break;
                        }
                    }
                }
                if (encoding == null)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int m = 0; m < this.encodings.Length; m++)
                    {
                        if (builder.Length != 0)
                        {
                            builder.Append(" | ");
                        }
                        builder.Append(this.encodings[m].WebName);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomRootUnexpectedCharset", new object[] { str, builder.ToString() })));
                }
            }
            if (expectedType != null)
            {
                string str2;
                if ((!header.Parameters.TryGetValue(MtomGlobals.TypeParam, out str2) || (str2 == null)) || (str2.Length == 0))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomRootRequiredParamNotSpecified", new object[] { MtomGlobals.TypeParam })));
                }
                if (str2 != expectedType)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomRootUnexpectedType", new object[] { str2, expectedType })));
                }
            }
            return encoding;
        }

        private MimePart ReadRootMimePart()
        {
            if (!this.mimeReader.ReadNextPart())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomRootPartNotFound")));
            }
            MimeHeaders headers = this.mimeReader.ReadHeaders(this.maxBufferSize, ref this.bufferRemaining);
            Stream contentStream = this.mimeReader.GetContentStream();
            if (contentStream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomMessageInvalidContentInMimePart")));
            }
            return new MimePart(contentStream, headers);
        }

        public override int ReadValueAsBase64(byte[] buffer, int offset, int count)
        {
            this.AdvanceToContentOnElement();
            return this.xmlReader.ReadValueAsBase64(buffer, offset, count);
        }

        public override int ReadValueChunk(char[] buffer, int index, int count)
        {
            return this.xmlReader.ReadValueChunk(buffer, index, count);
        }

        public override void ResolveEntity()
        {
            this.xmlReader.ResolveEntity();
        }

        public void SetInput(Stream stream, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas, int maxBufferSize, OnXmlDictionaryReaderClose onClose)
        {
            this.SetReadEncodings(encodings);
            this.CheckContentType(contentType);
            this.Initialize(stream, contentType, quotas, maxBufferSize);
            this.onClose = onClose;
        }

        public void SetInput(byte[] buffer, int offset, int count, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas, int maxBufferSize, OnXmlDictionaryReaderClose onClose)
        {
            this.SetInput(new MemoryStream(buffer, offset, count), encodings, contentType, quotas, maxBufferSize, onClose);
        }

        private void SetReadEncodings(Encoding[] encodings)
        {
            if (encodings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encodings");
            }
            for (int i = 0; i < encodings.Length; i++)
            {
                if (encodings[i] == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "encodings[{0}]", new object[] { i }));
                }
            }
            this.encodings = new Encoding[encodings.Length];
            encodings.CopyTo(this.encodings, 0);
        }

        public override void Skip()
        {
            this.xmlReader.Skip();
        }

        public override int AttributeCount
        {
            get
            {
                return this.xmlReader.AttributeCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                return this.xmlReader.BaseURI;
            }
        }

        public override bool CanReadBinaryContent
        {
            get
            {
                return this.xmlReader.CanReadBinaryContent;
            }
        }

        public override bool CanReadValueChunk
        {
            get
            {
                return this.xmlReader.CanReadValueChunk;
            }
        }

        public override bool CanResolveEntity
        {
            get
            {
                return this.xmlReader.CanResolveEntity;
            }
        }

        public override int Depth
        {
            get
            {
                return this.xmlReader.Depth;
            }
        }

        public override bool EOF
        {
            get
            {
                return this.xmlReader.EOF;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                return this.xmlReader.HasAttributes;
            }
        }

        public override bool HasValue
        {
            get
            {
                return this.xmlReader.HasValue;
            }
        }

        public override bool IsDefault
        {
            get
            {
                return this.xmlReader.IsDefault;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this.xmlReader.IsEmptyElement;
            }
        }

        public override string this[int index]
        {
            get
            {
                return this.xmlReader[index];
            }
        }

        public override string this[string name]
        {
            get
            {
                return this.xmlReader[name];
            }
        }

        public override string this[string name, string ns]
        {
            get
            {
                return this.xmlReader[name, ns];
            }
        }

        public int LineNumber
        {
            get
            {
                if (this.xmlReader.ReadState == System.Xml.ReadState.Closed)
                {
                    return 0;
                }
                IXmlLineInfo xmlReader = this.xmlReader as IXmlLineInfo;
                if (xmlReader == null)
                {
                    return 0;
                }
                return xmlReader.LineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                if (this.xmlReader.ReadState == System.Xml.ReadState.Closed)
                {
                    return 0;
                }
                IXmlLineInfo xmlReader = this.xmlReader as IXmlLineInfo;
                if (xmlReader == null)
                {
                    return 0;
                }
                return xmlReader.LinePosition;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.xmlReader.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this.xmlReader.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.xmlReader.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.xmlReader.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this.xmlReader.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.xmlReader.Prefix;
            }
        }

        public override XmlDictionaryReaderQuotas Quotas
        {
            get
            {
                return this.xmlReader.Quotas;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return this.xmlReader.QuoteChar;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                if ((this.xmlReader.ReadState != System.Xml.ReadState.Interactive) && (this.infosetReader != null))
                {
                    return this.infosetReader.ReadState;
                }
                return this.xmlReader.ReadState;
            }
        }

        public override XmlReaderSettings Settings
        {
            get
            {
                return this.xmlReader.Settings;
            }
        }

        public override string Value
        {
            get
            {
                return this.xmlReader.Value;
            }
        }

        public override Type ValueType
        {
            get
            {
                return this.xmlReader.ValueType;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.xmlReader.XmlLang;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this.xmlReader.XmlSpace;
            }
        }

        internal class MimePart
        {
            private byte[] buffer;
            private MimeHeaders headers;
            private bool isReferencedFromInfoset;
            private System.IO.Stream stream;

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal MimePart(System.IO.Stream stream, MimeHeaders headers)
            {
                this.stream = stream;
                this.headers = headers;
            }

            internal byte[] GetBuffer(int maxBuffer, ref int remaining)
            {
                if (this.buffer == null)
                {
                    MemoryStream stream = this.stream.CanSeek ? new MemoryStream((int) this.stream.Length) : new MemoryStream();
                    int count = 0x100;
                    byte[] buffer = new byte[count];
                    int size = 0;
                    do
                    {
                        size = this.stream.Read(buffer, 0, count);
                        XmlMtomReader.DecrementBufferQuota(maxBuffer, ref remaining, size);
                        if (size > 0)
                        {
                            stream.Write(buffer, 0, size);
                        }
                    }
                    while (size > 0);
                    stream.Seek(0L, SeekOrigin.Begin);
                    this.buffer = stream.GetBuffer();
                    this.stream = stream;
                }
                return this.buffer;
            }

            internal void Release(int maxBuffer, ref int remaining)
            {
                remaining += (int) this.Length;
                this.headers.Release(ref remaining);
            }

            internal MimeHeaders Headers
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.headers;
                }
            }

            internal long Length
            {
                get
                {
                    if (!this.stream.CanSeek)
                    {
                        return 0L;
                    }
                    return this.stream.Length;
                }
            }

            internal bool ReferencedFromInfoset
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.isReferencedFromInfoset;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.isReferencedFromInfoset = value;
                }
            }

            internal System.IO.Stream Stream
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.stream;
                }
            }
        }

        internal class XopIncludeReader : XmlDictionaryReader, IXmlLineInfo
        {
            private MemoryStream binHexStream;
            private int bytesRemaining;
            private int chunkSize = 0x1000;
            private bool finishedStream;
            private XmlNodeType nodeType;
            private XmlDictionaryReader parentReader;
            private XmlMtomReader.MimePart part;
            private System.Xml.ReadState readState;
            private int stringOffset;
            private string stringValue;
            private byte[] valueBuffer;
            private int valueCount;
            private int valueOffset;

            public XopIncludeReader(XmlMtomReader.MimePart part, XmlDictionaryReader reader)
            {
                if (part == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("part");
                }
                if (reader == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
                }
                this.part = part;
                this.parentReader = reader;
                this.readState = System.Xml.ReadState.Initial;
                this.nodeType = XmlNodeType.None;
                this.chunkSize = Math.Min(reader.Quotas.MaxBytesPerRead, this.chunkSize);
                this.bytesRemaining = this.chunkSize;
                this.finishedStream = false;
            }

            public override void Close()
            {
                this.CloseStreams();
                this.readState = System.Xml.ReadState.Closed;
            }

            private void CloseStreams()
            {
                if (this.binHexStream != null)
                {
                    this.binHexStream.Close();
                    this.binHexStream = null;
                }
            }

            public override string GetAttribute(int index)
            {
                return null;
            }

            public override string GetAttribute(string name)
            {
                return null;
            }

            public override string GetAttribute(string name, string ns)
            {
                return null;
            }

            public override string GetAttribute(XmlDictionaryString localName, XmlDictionaryString ns)
            {
                return null;
            }

            public override bool IsLocalName(string localName)
            {
                return false;
            }

            public override bool IsLocalName(XmlDictionaryString localName)
            {
                return false;
            }

            public override bool IsNamespaceUri(string ns)
            {
                return false;
            }

            public override bool IsNamespaceUri(XmlDictionaryString ns)
            {
                return false;
            }

            public override bool IsStartElement()
            {
                return false;
            }

            public override bool IsStartElement(string localName)
            {
                return false;
            }

            public override bool IsStartElement(string localName, string ns)
            {
                return false;
            }

            public override bool IsStartElement(XmlDictionaryString localName, XmlDictionaryString ns)
            {
                return false;
            }

            public override string LookupNamespace(string ns)
            {
                return this.parentReader.LookupNamespace(ns);
            }

            public override void MoveToAttribute(int index)
            {
            }

            public override bool MoveToAttribute(string name)
            {
                return false;
            }

            public override bool MoveToAttribute(string name, string ns)
            {
                return false;
            }

            public override bool MoveToElement()
            {
                return false;
            }

            public override bool MoveToFirstAttribute()
            {
                return false;
            }

            public override bool MoveToNextAttribute()
            {
                return false;
            }

            public override bool Read()
            {
                bool flag = true;
                switch (this.readState)
                {
                    case System.Xml.ReadState.Initial:
                        this.readState = System.Xml.ReadState.Interactive;
                        this.nodeType = XmlNodeType.Text;
                        break;

                    case System.Xml.ReadState.Interactive:
                        if (!this.finishedStream && ((this.bytesRemaining != this.chunkSize) || (this.stringValue != null)))
                        {
                            this.bytesRemaining = this.chunkSize;
                            break;
                        }
                        this.readState = System.Xml.ReadState.EndOfFile;
                        this.nodeType = XmlNodeType.EndElement;
                        break;

                    case System.Xml.ReadState.EndOfFile:
                        this.nodeType = XmlNodeType.None;
                        flag = false;
                        break;
                }
                this.stringValue = null;
                this.binHexStream = null;
                this.valueOffset = 0;
                this.valueCount = 0;
                this.stringOffset = 0;
                this.CloseStreams();
                return flag;
            }

            public override bool ReadAttributeValue()
            {
                return false;
            }

            public override int ReadContentAsBase64(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
                }
                if (offset < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
                }
                if (offset > buffer.Length)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { buffer.Length })));
                }
                if (count < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
                }
                if (count > (buffer.Length - offset))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { buffer.Length - offset })));
                }
                if (this.valueCount > 0)
                {
                    count = Math.Min(count, this.valueCount);
                    Buffer.BlockCopy(this.valueBuffer, this.valueOffset, buffer, offset, count);
                    this.valueOffset += count;
                    this.valueCount -= count;
                    return count;
                }
                if (this.chunkSize < count)
                {
                    count = this.chunkSize;
                }
                int num = 0;
                if (this.readState == System.Xml.ReadState.Interactive)
                {
                    while (num < count)
                    {
                        int num2 = this.part.Stream.Read(buffer, offset + num, count - num);
                        if (num2 == 0)
                        {
                            this.finishedStream = true;
                            if (!this.Read())
                            {
                                break;
                            }
                        }
                        num += num2;
                    }
                }
                this.bytesRemaining = this.chunkSize;
                return num;
            }

            public override int ReadContentAsBinHex(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
                }
                if (offset < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
                }
                if (offset > buffer.Length)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { buffer.Length })));
                }
                if (count < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
                }
                if (count > (buffer.Length - offset))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { buffer.Length - offset })));
                }
                if (this.chunkSize < count)
                {
                    count = this.chunkSize;
                }
                int num = 0;
                int num2 = 0;
                while (num < count)
                {
                    if (this.binHexStream == null)
                    {
                        try
                        {
                            this.binHexStream = new MemoryStream(new BinHexEncoding().GetBytes(this.Value));
                        }
                        catch (FormatException exception)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(exception.Message, exception));
                        }
                    }
                    int num3 = this.binHexStream.Read(buffer, offset + num, count - num);
                    if (num3 == 0)
                    {
                        this.finishedStream = true;
                        if (!this.Read())
                        {
                            break;
                        }
                        num2 = 0;
                    }
                    num += num3;
                    num2 += num3;
                }
                if ((this.stringValue != null) && (num2 > 0))
                {
                    this.stringValue = this.stringValue.Substring(num2 * 2);
                    this.stringOffset = Math.Max(0, this.stringOffset - (num2 * 2));
                    this.bytesRemaining = this.chunkSize;
                }
                return num;
            }

            public override string ReadContentAsString()
            {
                int maxStringContentLength = this.Quotas.MaxStringContentLength;
                StringBuilder builder = new StringBuilder();
                do
                {
                    string str = this.Value;
                    if (str.Length > maxStringContentLength)
                    {
                        XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, this.Quotas.MaxStringContentLength);
                    }
                    maxStringContentLength -= str.Length;
                    builder.Append(str);
                }
                while (this.Read());
                return builder.ToString();
            }

            public override string ReadInnerXml()
            {
                return this.ReadContentAsString();
            }

            public override string ReadOuterXml()
            {
                return this.ReadContentAsString();
            }

            public override int ReadValueAsBase64(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
                }
                if (offset < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
                }
                if (offset > buffer.Length)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { buffer.Length })));
                }
                if (count < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
                }
                if (count > (buffer.Length - offset))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { buffer.Length - offset })));
                }
                if (this.stringValue != null)
                {
                    count = Math.Min(count, this.valueCount);
                    if (count > 0)
                    {
                        Buffer.BlockCopy(this.valueBuffer, this.valueOffset, buffer, offset, count);
                        this.valueOffset += count;
                        this.valueCount -= count;
                    }
                    return count;
                }
                if (this.bytesRemaining < count)
                {
                    count = this.bytesRemaining;
                }
                int num = 0;
                if (this.readState == System.Xml.ReadState.Interactive)
                {
                    while (num < count)
                    {
                        int num2 = this.part.Stream.Read(buffer, offset + num, count - num);
                        if (num2 == 0)
                        {
                            this.finishedStream = true;
                            break;
                        }
                        num += num2;
                    }
                }
                this.bytesRemaining -= num;
                return num;
            }

            public override int ReadValueChunk(char[] chars, int offset, int count)
            {
                if (chars == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("chars");
                }
                if (offset < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
                }
                if (offset > chars.Length)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { chars.Length })));
                }
                if (count < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
                }
                if (count > (chars.Length - offset))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { chars.Length - offset })));
                }
                if (this.readState != System.Xml.ReadState.Interactive)
                {
                    return 0;
                }
                string text1 = this.Value;
                count = Math.Min(this.stringValue.Length - this.stringOffset, count);
                if (count > 0)
                {
                    this.stringValue.CopyTo(this.stringOffset, chars, offset, count);
                    this.stringOffset += count;
                }
                return count;
            }

            public override void ResolveEntity()
            {
            }

            public override void Skip()
            {
                this.Read();
            }

            bool IXmlLineInfo.HasLineInfo()
            {
                return ((IXmlLineInfo) this.parentReader).HasLineInfo();
            }

            public override int AttributeCount
            {
                get
                {
                    return 0;
                }
            }

            public override string BaseURI
            {
                get
                {
                    return this.parentReader.BaseURI;
                }
            }

            public override bool CanReadBinaryContent
            {
                get
                {
                    return true;
                }
            }

            public override bool CanReadValueChunk
            {
                get
                {
                    return true;
                }
            }

            public override bool CanResolveEntity
            {
                get
                {
                    return this.parentReader.CanResolveEntity;
                }
            }

            public override int Depth
            {
                get
                {
                    if (this.readState != System.Xml.ReadState.Interactive)
                    {
                        return this.parentReader.Depth;
                    }
                    return (this.parentReader.Depth + 1);
                }
            }

            public override bool EOF
            {
                get
                {
                    return (this.readState == System.Xml.ReadState.EndOfFile);
                }
            }

            public override bool HasAttributes
            {
                get
                {
                    return false;
                }
            }

            public override bool HasValue
            {
                get
                {
                    return (this.readState == System.Xml.ReadState.Interactive);
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return false;
                }
            }

            public override bool IsEmptyElement
            {
                get
                {
                    return false;
                }
            }

            public override string this[int index]
            {
                get
                {
                    return null;
                }
            }

            public override string this[string name]
            {
                get
                {
                    return null;
                }
            }

            public override string this[string name, string ns]
            {
                get
                {
                    return null;
                }
            }

            public override string LocalName
            {
                get
                {
                    if (this.readState != System.Xml.ReadState.Interactive)
                    {
                        return this.parentReader.LocalName;
                    }
                    return string.Empty;
                }
            }

            public override string Name
            {
                get
                {
                    if (this.readState != System.Xml.ReadState.Interactive)
                    {
                        return this.parentReader.Name;
                    }
                    return string.Empty;
                }
            }

            public override string NamespaceURI
            {
                get
                {
                    if (this.readState != System.Xml.ReadState.Interactive)
                    {
                        return this.parentReader.NamespaceURI;
                    }
                    return string.Empty;
                }
            }

            public override XmlNameTable NameTable
            {
                get
                {
                    return this.parentReader.NameTable;
                }
            }

            public override XmlNodeType NodeType
            {
                get
                {
                    if (this.readState != System.Xml.ReadState.Interactive)
                    {
                        return this.parentReader.NodeType;
                    }
                    return this.nodeType;
                }
            }

            public override string Prefix
            {
                get
                {
                    if (this.readState != System.Xml.ReadState.Interactive)
                    {
                        return this.parentReader.Prefix;
                    }
                    return string.Empty;
                }
            }

            public override XmlDictionaryReaderQuotas Quotas
            {
                get
                {
                    return this.parentReader.Quotas;
                }
            }

            public override char QuoteChar
            {
                get
                {
                    return this.parentReader.QuoteChar;
                }
            }

            public override System.Xml.ReadState ReadState
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.readState;
                }
            }

            public override XmlReaderSettings Settings
            {
                get
                {
                    return this.parentReader.Settings;
                }
            }

            int IXmlLineInfo.LineNumber
            {
                get
                {
                    return ((IXmlLineInfo) this.parentReader).LineNumber;
                }
            }

            int IXmlLineInfo.LinePosition
            {
                get
                {
                    return ((IXmlLineInfo) this.parentReader).LinePosition;
                }
            }

            public override string Value
            {
                get
                {
                    if (this.readState != System.Xml.ReadState.Interactive)
                    {
                        return string.Empty;
                    }
                    if (this.stringValue == null)
                    {
                        int bytesRemaining = this.bytesRemaining;
                        bytesRemaining -= bytesRemaining % 3;
                        if ((this.valueCount > 0) && (this.valueOffset > 0))
                        {
                            Buffer.BlockCopy(this.valueBuffer, this.valueOffset, this.valueBuffer, 0, this.valueCount);
                            this.valueOffset = 0;
                        }
                        bytesRemaining -= this.valueCount;
                        if (this.valueBuffer == null)
                        {
                            this.valueBuffer = new byte[bytesRemaining];
                        }
                        else if (this.valueBuffer.Length < bytesRemaining)
                        {
                            Array.Resize<byte>(ref this.valueBuffer, bytesRemaining);
                        }
                        byte[] valueBuffer = this.valueBuffer;
                        int offset = 0;
                        int num3 = 0;
                        while (bytesRemaining > 0)
                        {
                            num3 = this.part.Stream.Read(valueBuffer, offset, bytesRemaining);
                            if (num3 == 0)
                            {
                                this.finishedStream = true;
                                break;
                            }
                            this.bytesRemaining -= num3;
                            this.valueCount += num3;
                            bytesRemaining -= num3;
                            offset += num3;
                        }
                        this.stringValue = Convert.ToBase64String(valueBuffer, 0, this.valueCount);
                    }
                    return this.stringValue;
                }
            }

            public override Type ValueType
            {
                get
                {
                    if (this.readState != System.Xml.ReadState.Interactive)
                    {
                        return this.parentReader.ValueType;
                    }
                    return typeof(byte[]);
                }
            }

            public override string XmlLang
            {
                get
                {
                    return this.parentReader.XmlLang;
                }
            }

            public override System.Xml.XmlSpace XmlSpace
            {
                get
                {
                    return this.parentReader.XmlSpace;
                }
            }
        }
    }
}

