namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading;
    using System.Xml.XPath;

    internal class XmlMtomWriter : XmlDictionaryWriter, IXmlMtomWriterInitializer
    {
        private IList<MtomBinaryData> binaryDataChunks;
        private byte[] bytes;
        private char[] chars;
        private string contentID;
        private string contentType;
        private MemoryStream contentTypeStream;
        private int depth;
        private Encoding encoding;
        private XmlDictionaryWriter infosetWriter;
        private string initialContentTypeForMimeMessage;
        private string initialContentTypeForRootPart;
        private bool isClosed;
        private bool isUTF8;
        private const int MaxInlinedBytes = 0x2ff;
        private int maxSizeInBytes;
        private List<MimePart> mimeParts;
        private MimeWriter mimeWriter;
        private bool ownsStream;
        private int sizeOfBufferedBinaryData;
        private int totalSizeOfMimeParts;
        private XmlDictionaryWriter writer;

        private static string CharSet(Encoding enc)
        {
            string webName = enc.WebName;
            if (string.Compare(webName, Encoding.UTF8.WebName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                if (string.Compare(webName, Encoding.Unicode.WebName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return "utf-16LE";
                }
                if (string.Compare(webName, Encoding.BigEndianUnicode.WebName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return "utf-16BE";
                }
            }
            return webName;
        }

        private void CheckIfEndContentTypeAttribute()
        {
            if (this.contentTypeStream != null)
            {
                this.Writer.WriteEndAttribute();
                this.Writer.WriteEndElement();
                this.Writer.Flush();
                this.contentTypeStream.Position = 0L;
                XmlReader reader = XmlDictionaryReader.CreateBinaryReader(this.contentTypeStream, null, XmlDictionaryReaderQuotas.Max, null, null);
                while (reader.Read())
                {
                    if (reader.IsStartElement("Wrapper"))
                    {
                        this.contentType = reader.GetAttribute(MtomGlobals.MimeContentTypeLocalName, MtomGlobals.MimeContentTypeNamespace200406);
                        if (this.contentType == null)
                        {
                            this.contentType = reader.GetAttribute(MtomGlobals.MimeContentTypeLocalName, MtomGlobals.MimeContentTypeNamespace200505);
                        }
                        break;
                    }
                }
                this.writer = this.infosetWriter;
                this.infosetWriter = null;
                this.contentTypeStream = null;
                if (this.contentType != null)
                {
                    this.Writer.WriteString(this.contentType);
                }
            }
        }

        private void CheckIfStartContentTypeAttribute(string localName, string ns)
        {
            if ((((localName != null) && (localName == MtomGlobals.MimeContentTypeLocalName)) && (ns != null)) && ((ns == MtomGlobals.MimeContentTypeNamespace200406) || (ns == MtomGlobals.MimeContentTypeNamespace200505)))
            {
                this.contentTypeStream = new MemoryStream();
                this.infosetWriter = this.Writer;
                this.writer = XmlDictionaryWriter.CreateBinaryWriter(this.contentTypeStream);
                this.Writer.WriteStartElement("Wrapper");
                this.Writer.WriteStartAttribute(localName, ns);
            }
        }

        public override void Close()
        {
            if (!this.isClosed)
            {
                this.isClosed = true;
                if (this.IsInitialized)
                {
                    this.WriteXOPInclude();
                    if (((this.Writer.WriteState == System.Xml.WriteState.Element) || (this.Writer.WriteState == System.Xml.WriteState.Attribute)) || (this.Writer.WriteState == System.Xml.WriteState.Content))
                    {
                        this.Writer.WriteEndDocument();
                    }
                    this.Writer.Flush();
                    this.depth = 0;
                    this.WriteXOPBinaryParts();
                    this.Writer.Close();
                }
            }
        }

        public override void Flush()
        {
            if (this.IsInitialized)
            {
                this.Writer.Flush();
            }
        }

        public static string GenerateUriForMimePart(int index)
        {
            return string.Format(CultureInfo.InvariantCulture, "http://tempuri.org/{0}/{1}", new object[] { index, DateTime.Now.Ticks });
        }

        private static string GetBoundaryString()
        {
            return MimeBoundaryGenerator.Next();
        }

        private static string GetContentTypeForMimeMessage(string boundary, string startUri, string startInfo)
        {
            StringBuilder builder = new StringBuilder(string.Format(CultureInfo.InvariantCulture, "{0}/{1};{2}=\"{3}\";{4}=\"{5}\"", new object[] { MtomGlobals.MediaType, MtomGlobals.MediaSubtype, MtomGlobals.TypeParam, MtomGlobals.XopType, MtomGlobals.BoundaryParam, boundary }));
            if ((startUri != null) && (startUri.Length > 0))
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, ";{0}=\"<{1}>\"", new object[] { MtomGlobals.StartParam, startUri });
            }
            if ((startInfo != null) && (startInfo.Length > 0))
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, ";{0}=\"{1}\"", new object[] { MtomGlobals.StartInfoParam, startInfo });
            }
            return builder.ToString();
        }

        private static string GetContentTypeForRootMimePart(Encoding encoding, string startInfo)
        {
            string str = string.Format(CultureInfo.InvariantCulture, "{0};{1}={2}", new object[] { MtomGlobals.XopType, MtomGlobals.CharsetParam, CharSet(encoding) });
            if (startInfo != null)
            {
                str = string.Format(CultureInfo.InvariantCulture, "{0};{1}=\"{2}\"", new object[] { str, MtomGlobals.TypeParam, startInfo });
            }
            return str;
        }

        private void Initialize()
        {
            if (this.isClosed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlWriterClosed")));
            }
            if (this.initialContentTypeForRootPart != null)
            {
                if (this.initialContentTypeForMimeMessage != null)
                {
                    this.mimeWriter.StartPreface();
                    this.mimeWriter.WriteHeader(MimeGlobals.MimeVersionHeader, MimeGlobals.DefaultVersion);
                    this.mimeWriter.WriteHeader(MimeGlobals.ContentTypeHeader, this.initialContentTypeForMimeMessage);
                    this.initialContentTypeForMimeMessage = null;
                }
                this.WriteMimeHeaders(this.contentID, this.initialContentTypeForRootPart, this.isUTF8 ? MimeGlobals.Encoding8bit : MimeGlobals.EncodingBinary);
                Stream contentStream = this.mimeWriter.GetContentStream();
                IXmlTextWriterInitializer writer = this.writer as IXmlTextWriterInitializer;
                if (writer == null)
                {
                    this.writer = XmlDictionaryWriter.CreateTextWriter(contentStream, this.encoding, this.ownsStream);
                }
                else
                {
                    writer.SetOutput(contentStream, this.encoding, this.ownsStream);
                }
                this.contentID = null;
                this.initialContentTypeForRootPart = null;
            }
        }

        private void Initialize(Stream stream, string startInfo, string boundary, string startUri, bool writeMessageHeaders, bool ownsStream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            if (startInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("startInfo");
            }
            if (boundary == null)
            {
                boundary = GetBoundaryString();
            }
            if (startUri == null)
            {
                startUri = GenerateUriForMimePart(0);
            }
            if (!MailBnfHelper.IsValidMimeBoundary(boundary))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("MtomBoundaryInvalid", new object[] { boundary }), "boundary"));
            }
            this.ownsStream = ownsStream;
            this.isClosed = false;
            this.depth = 0;
            this.totalSizeOfMimeParts = 0;
            this.sizeOfBufferedBinaryData = 0;
            this.binaryDataChunks = null;
            this.contentType = null;
            this.contentTypeStream = null;
            this.contentID = startUri;
            if (this.mimeParts != null)
            {
                this.mimeParts.Clear();
            }
            this.mimeWriter = new MimeWriter(stream, boundary);
            this.initialContentTypeForRootPart = GetContentTypeForRootMimePart(this.encoding, startInfo);
            if (writeMessageHeaders)
            {
                this.initialContentTypeForMimeMessage = GetContentTypeForMimeMessage(boundary, startUri, startInfo);
            }
        }

        internal static bool IsUTF8Encoding(Encoding encoding)
        {
            return (encoding.WebName == "utf-8");
        }

        public override string LookupPrefix(string ns)
        {
            return this.Writer.LookupPrefix(ns);
        }

        public void SetOutput(Stream stream, Encoding encoding, int maxSizeInBytes, string startInfo, string boundary, string startUri, bool writeMessageHeaders, bool ownsStream)
        {
            if (encoding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoding");
            }
            if (maxSizeInBytes < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxSizeInBytes", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            this.maxSizeInBytes = maxSizeInBytes;
            this.encoding = encoding;
            this.isUTF8 = IsUTF8Encoding(encoding);
            this.Initialize(stream, startInfo, boundary, startUri, writeMessageHeaders, ownsStream);
        }

        private void ThrowIfElementIsXOPInclude(string prefix, string localName, string ns)
        {
            if (ns == null)
            {
                XmlBaseWriter writer = this.Writer as XmlBaseWriter;
                if (writer != null)
                {
                    ns = writer.LookupNamespace(prefix);
                }
            }
            if ((localName == MtomGlobals.XopIncludeLocalName) && (ns == MtomGlobals.XopIncludeNamespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomDataMustNotContainXopInclude", new object[] { MtomGlobals.XopIncludeLocalName, MtomGlobals.XopIncludeNamespace })));
            }
        }

        internal static int ValidateSizeOfMessage(int maxSize, int offset, int size)
        {
            if (size > (maxSize - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("MtomExceededMaxSizeInBytes", new object[] { maxSize })));
            }
            return size;
        }

        public override void WriteAttributes(XmlReader reader, bool defattr)
        {
            this.Writer.WriteAttributes(reader, defattr);
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            if (this.Writer.WriteState == System.Xml.WriteState.Element)
            {
                if (buffer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
                }
                if (index < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
                }
                if (count < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
                }
                if (count > (buffer.Length - index))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { buffer.Length - index })));
                }
                if (this.binaryDataChunks == null)
                {
                    this.binaryDataChunks = new List<MtomBinaryData>();
                    this.contentID = GenerateUriForMimePart((this.mimeParts == null) ? 1 : (this.mimeParts.Count + 1));
                }
                int offset = ValidateSizeOfMessage(this.maxSizeInBytes, 0, this.totalSizeOfMimeParts);
                offset += ValidateSizeOfMessage(this.maxSizeInBytes, offset, this.sizeOfBufferedBinaryData);
                offset += ValidateSizeOfMessage(this.maxSizeInBytes, offset, count);
                this.sizeOfBufferedBinaryData += count;
                this.binaryDataChunks.Add(new MtomBinaryData(buffer, index, count));
            }
            else
            {
                this.Writer.WriteBase64(buffer, index, count);
            }
        }

        private void WriteBase64Inline()
        {
            foreach (MtomBinaryData data in this.binaryDataChunks)
            {
                if (data.type == MtomBinaryDataType.Provider)
                {
                    this.Writer.WriteValue(data.provider);
                }
                else
                {
                    this.Writer.WriteBase64(data.chunk, 0, data.chunk.Length);
                }
            }
            this.sizeOfBufferedBinaryData = 0;
            this.binaryDataChunks = null;
            this.contentType = null;
            this.contentID = null;
        }

        private void WriteBase64InlineIfPresent()
        {
            if (this.binaryDataChunks != null)
            {
                this.WriteBase64Inline();
            }
        }

        public override void WriteBinHex(byte[] buffer, int index, int count)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteBinHex(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            if ((this.depth != 0) || (this.mimeWriter.WriteState != MimeWriterState.Closed))
            {
                this.WriteBase64InlineIfPresent();
                this.Writer.WriteComment(text);
            }
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteEndAttribute()
        {
            this.CheckIfEndContentTypeAttribute();
            this.Writer.WriteEndAttribute();
        }

        public override void WriteEndDocument()
        {
            this.WriteXOPInclude();
            this.Writer.WriteEndDocument();
            this.depth = 0;
            this.WriteXOPBinaryParts();
        }

        public override void WriteEndElement()
        {
            this.WriteXOPInclude();
            this.Writer.WriteEndElement();
            this.depth--;
            this.WriteXOPBinaryParts();
        }

        public override void WriteEntityRef(string name)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteEntityRef(name);
        }

        public override void WriteFullEndElement()
        {
            this.WriteXOPInclude();
            this.Writer.WriteFullEndElement();
            this.depth--;
            this.WriteXOPBinaryParts();
        }

        private void WriteMimeHeaders(string contentID, string contentType, string contentTransferEncoding)
        {
            this.mimeWriter.StartPart();
            if (contentID != null)
            {
                this.mimeWriter.WriteHeader(MimeGlobals.ContentIDHeader, string.Format(CultureInfo.InvariantCulture, "<{0}>", new object[] { contentID }));
            }
            if (contentTransferEncoding != null)
            {
                this.mimeWriter.WriteHeader(MimeGlobals.ContentTransferEncodingHeader, contentTransferEncoding);
            }
            if (contentType != null)
            {
                this.mimeWriter.WriteHeader(MimeGlobals.ContentTypeHeader, contentType);
            }
        }

        public override void WriteName(string name)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteName(name);
        }

        public override void WriteNmToken(string name)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteNmToken(name);
        }

        public override void WriteNode(XPathNavigator navigator, bool defattr)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteNode(navigator, defattr);
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteProcessingInstruction(name, text);
        }

        public override void WriteQualifiedName(string localName, string namespaceUri)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteQualifiedName(localName, namespaceUri);
        }

        public override void WriteRaw(string data)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteRaw(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteRaw(buffer, index, count);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            this.Writer.WriteStartAttribute(prefix, localName, ns);
            this.CheckIfStartContentTypeAttribute(localName, ns);
        }

        public override void WriteStartAttribute(string prefix, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            this.Writer.WriteStartAttribute(prefix, localName, ns);
            if ((localName != null) && (ns != null))
            {
                this.CheckIfStartContentTypeAttribute(localName.Value, ns.Value);
            }
        }

        public override void WriteStartDocument()
        {
            this.Writer.WriteStartDocument();
        }

        public override void WriteStartDocument(bool standalone)
        {
            this.Writer.WriteStartDocument(standalone);
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.WriteBase64InlineIfPresent();
            this.ThrowIfElementIsXOPInclude(prefix, localName, ns);
            this.Writer.WriteStartElement(prefix, localName, ns);
            this.depth++;
        }

        public override void WriteStartElement(string prefix, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            this.WriteBase64InlineIfPresent();
            this.ThrowIfElementIsXOPInclude(prefix, localName.Value, (ns == null) ? null : ns.Value);
            this.Writer.WriteStartElement(prefix, localName, ns);
            this.depth++;
        }

        public override void WriteString(string text)
        {
            if (((this.depth != 0) || (this.mimeWriter.WriteState != MimeWriterState.Closed)) || !XmlConverter.IsWhitespace(text))
            {
                this.WriteBase64InlineIfPresent();
                this.Writer.WriteString(text);
            }
        }

        public override void WriteString(XmlDictionaryString value)
        {
            if (((this.depth != 0) || (this.mimeWriter.WriteState != MimeWriterState.Closed)) || !XmlConverter.IsWhitespace(value.Value))
            {
                this.WriteBase64InlineIfPresent();
                this.Writer.WriteString(value);
            }
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteSurrogateCharEntity(lowChar, highChar);
        }

        protected override void WriteTextNode(XmlDictionaryReader reader, bool attribute)
        {
            Type valueType = reader.ValueType;
            if (valueType == typeof(string))
            {
                if (reader.CanReadValueChunk)
                {
                    int num;
                    if (this.chars == null)
                    {
                        this.chars = new char[0x100];
                    }
                    while ((num = reader.ReadValueChunk(this.chars, 0, this.chars.Length)) > 0)
                    {
                        this.WriteChars(this.chars, 0, num);
                    }
                }
                else
                {
                    this.WriteString(reader.Value);
                }
                if (!attribute)
                {
                    reader.Read();
                }
            }
            else if (valueType == typeof(byte[]))
            {
                if (reader.CanReadBinaryContent)
                {
                    int num2;
                    if (this.bytes == null)
                    {
                        this.bytes = new byte[0x180];
                    }
                    while ((num2 = reader.ReadValueAsBase64(this.bytes, 0, this.bytes.Length)) > 0)
                    {
                        this.WriteBase64(this.bytes, 0, num2);
                    }
                }
                else
                {
                    this.WriteString(reader.Value);
                }
                if (!attribute)
                {
                    reader.Read();
                }
            }
            else
            {
                base.WriteTextNode(reader, attribute);
            }
        }

        public override void WriteValue(bool value)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteValue(value);
        }

        public override void WriteValue(DateTime value)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteValue(value);
        }

        public override void WriteValue(double value)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteValue(value);
        }

        public override void WriteValue(int value)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteValue(value);
        }

        public override void WriteValue(long value)
        {
            this.WriteBase64InlineIfPresent();
            this.Writer.WriteValue(value);
        }

        public override void WriteValue(object value)
        {
            IStreamProvider provider = value as IStreamProvider;
            if (provider != null)
            {
                this.WriteValue(provider);
            }
            else
            {
                this.WriteBase64InlineIfPresent();
                this.Writer.WriteValue(value);
            }
        }

        public override void WriteValue(string value)
        {
            if (((this.depth != 0) || (this.mimeWriter.WriteState != MimeWriterState.Closed)) || !XmlConverter.IsWhitespace(value))
            {
                this.WriteBase64InlineIfPresent();
                this.Writer.WriteValue(value);
            }
        }

        public override void WriteValue(IStreamProvider value)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
            }
            if (this.Writer.WriteState == System.Xml.WriteState.Element)
            {
                if (this.binaryDataChunks == null)
                {
                    this.binaryDataChunks = new List<MtomBinaryData>();
                    this.contentID = GenerateUriForMimePart((this.mimeParts == null) ? 1 : (this.mimeParts.Count + 1));
                }
                this.binaryDataChunks.Add(new MtomBinaryData(value));
            }
            else
            {
                this.Writer.WriteValue(value);
            }
        }

        public override void WriteValue(XmlDictionaryString value)
        {
            if (((this.depth != 0) || (this.mimeWriter.WriteState != MimeWriterState.Closed)) || !XmlConverter.IsWhitespace(value.Value))
            {
                this.WriteBase64InlineIfPresent();
                this.Writer.WriteValue(value);
            }
        }

        public override void WriteWhitespace(string whitespace)
        {
            if ((this.depth != 0) || (this.mimeWriter.WriteState != MimeWriterState.Closed))
            {
                this.WriteBase64InlineIfPresent();
                this.Writer.WriteWhitespace(whitespace);
            }
        }

        public override void WriteXmlnsAttribute(string prefix, string ns)
        {
            this.Writer.WriteXmlnsAttribute(prefix, ns);
        }

        public override void WriteXmlnsAttribute(string prefix, XmlDictionaryString ns)
        {
            this.Writer.WriteXmlnsAttribute(prefix, ns);
        }

        private void WriteXOPBinaryParts()
        {
            if ((this.depth <= 0) && (this.mimeWriter.WriteState != MimeWriterState.Closed))
            {
                if (this.Writer.WriteState != System.Xml.WriteState.Closed)
                {
                    this.Writer.Flush();
                }
                if (this.mimeParts != null)
                {
                    foreach (MimePart part in this.mimeParts)
                    {
                        this.WriteMimeHeaders(part.contentID, part.contentType, part.contentTransferEncoding);
                        Stream contentStream = this.mimeWriter.GetContentStream();
                        int count = 0x100;
                        int num2 = 0;
                        byte[] buffer = new byte[count];
                        Stream stream = null;
                        foreach (MtomBinaryData data in part.binaryData)
                        {
                            if (data.type != MtomBinaryDataType.Provider)
                            {
                                goto Label_0122;
                            }
                            stream = data.provider.GetStream();
                            if (stream == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlInvalidStream")));
                            }
                        Label_00DD:
                            num2 = stream.Read(buffer, 0, count);
                            if (num2 > 0)
                            {
                                contentStream.Write(buffer, 0, num2);
                                if ((count < 0x10000) && (num2 == count))
                                {
                                    count *= 0x10;
                                    buffer = new byte[count];
                                }
                                goto Label_00DD;
                            }
                            data.provider.ReleaseStream(stream);
                            continue;
                        Label_0122:
                            contentStream.Write(data.chunk, 0, data.chunk.Length);
                        }
                    }
                    this.mimeParts.Clear();
                }
                this.mimeWriter.Close();
            }
        }

        private void WriteXOPInclude()
        {
            if (this.binaryDataChunks != null)
            {
                bool flag = true;
                long num = 0L;
                foreach (MtomBinaryData data in this.binaryDataChunks)
                {
                    long length = data.Length;
                    if ((length < 0L) || (length > (0x2ffL - num)))
                    {
                        flag = false;
                        break;
                    }
                    num += length;
                }
                if (flag)
                {
                    this.WriteBase64Inline();
                }
                else
                {
                    if (this.mimeParts == null)
                    {
                        this.mimeParts = new List<MimePart>();
                    }
                    MimePart item = new MimePart(this.binaryDataChunks, this.contentID, this.contentType, MimeGlobals.EncodingBinary, this.sizeOfBufferedBinaryData, this.maxSizeInBytes);
                    this.mimeParts.Add(item);
                    this.totalSizeOfMimeParts += ValidateSizeOfMessage(this.maxSizeInBytes, this.totalSizeOfMimeParts, item.sizeInBytes);
                    this.totalSizeOfMimeParts += ValidateSizeOfMessage(this.maxSizeInBytes, this.totalSizeOfMimeParts, this.mimeWriter.GetBoundarySize());
                    this.Writer.WriteStartElement(MtomGlobals.XopIncludePrefix, MtomGlobals.XopIncludeLocalName, MtomGlobals.XopIncludeNamespace);
                    this.Writer.WriteStartAttribute(MtomGlobals.XopIncludeHrefLocalName, MtomGlobals.XopIncludeHrefNamespace);
                    this.Writer.WriteValue(string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { MimeGlobals.ContentIDScheme, Uri.EscapeDataString(this.contentID) }));
                    this.Writer.WriteEndAttribute();
                    this.Writer.WriteEndElement();
                    this.binaryDataChunks = null;
                    this.sizeOfBufferedBinaryData = 0;
                    this.contentType = null;
                    this.contentID = null;
                }
            }
        }

        private bool IsInitialized
        {
            get
            {
                return (this.initialContentTypeForRootPart == null);
            }
        }

        public override XmlWriterSettings Settings
        {
            get
            {
                return this.Writer.Settings;
            }
        }

        private XmlDictionaryWriter Writer
        {
            get
            {
                if (!this.IsInitialized)
                {
                    this.Initialize();
                }
                return this.writer;
            }
        }

        public override System.Xml.WriteState WriteState
        {
            get
            {
                return this.Writer.WriteState;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.Writer.XmlLang;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this.Writer.XmlSpace;
            }
        }

        private static class MimeBoundaryGenerator
        {
            private static long id;
            private static string prefix = (Guid.NewGuid().ToString() + "+id=");

            internal static string Next()
            {
                long num = Interlocked.Increment(ref id);
                return string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { prefix, num });
            }
        }

        private class MimePart
        {
            internal IList<MtomBinaryData> binaryData;
            internal string contentID;
            internal string contentTransferEncoding;
            internal string contentType;
            internal int sizeInBytes;

            internal MimePart(IList<MtomBinaryData> binaryData, string contentID, string contentType, string contentTransferEncoding, int sizeOfBufferedBinaryData, int maxSizeInBytes)
            {
                this.binaryData = binaryData;
                this.contentID = contentID;
                this.contentType = contentType ?? MtomGlobals.DefaultContentTypeForBinary;
                this.contentTransferEncoding = contentTransferEncoding;
                this.sizeInBytes = GetSize(contentID, contentType, contentTransferEncoding, sizeOfBufferedBinaryData, maxSizeInBytes);
            }

            private static int GetSize(string contentID, string contentType, string contentTransferEncoding, int sizeOfBufferedBinaryData, int maxSizeInBytes)
            {
                int offset = XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, 0, MimeGlobals.CRLF.Length * 3);
                if (contentTransferEncoding != null)
                {
                    offset += XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, offset, MimeWriter.GetHeaderSize(MimeGlobals.ContentTransferEncodingHeader, contentTransferEncoding, maxSizeInBytes));
                }
                if (contentType != null)
                {
                    offset += XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, offset, MimeWriter.GetHeaderSize(MimeGlobals.ContentTypeHeader, contentType, maxSizeInBytes));
                }
                if (contentID != null)
                {
                    offset += XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, offset, MimeWriter.GetHeaderSize(MimeGlobals.ContentIDHeader, contentID, maxSizeInBytes));
                    offset += XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, offset, 2);
                }
                return (offset + XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, offset, sizeOfBufferedBinaryData));
            }
        }
    }
}

