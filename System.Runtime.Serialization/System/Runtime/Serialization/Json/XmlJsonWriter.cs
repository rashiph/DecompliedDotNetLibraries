namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;
    using System.Xml;

    internal class XmlJsonWriter : XmlDictionaryWriter, IXmlJsonWriterInitializer
    {
        private string attributeText;
        private const char BACK_SLASH = '\\';
        [SecurityCritical]
        private static System.Text.BinHexEncoding binHexEncoding;
        private JsonDataType dataType;
        private int depth;
        private bool endElementBuffer;
        private const char FORWARD_SLASH = '/';
        private const char HIGH_SURROGATE_START = '\ud800';
        private bool isWritingDataTypeAttribute;
        private bool isWritingServerTypeAttribute;
        private bool isWritingXmlnsAttribute;
        private bool isWritingXmlnsAttributeDefaultNs;
        private const char LOW_SURROGATE_END = '\udfff';
        private const char MAX_CHAR = '￾';
        private NameState nameState;
        private JsonNodeType nodeType;
        private JsonNodeWriter nodeWriter;
        private JsonNodeType[] scopes;
        private string serverTypeValue;
        private const char WHITESPACE = ' ';
        private System.Xml.WriteState writeState;
        private bool wroteServerTypeAttribute;
        private const string xmlNamespace = "http://www.w3.org/XML/1998/namespace";
        private const string xmlnsNamespace = "http://www.w3.org/2000/xmlns/";

        public XmlJsonWriter()
        {
            this.InitializeWriter();
        }

        internal static bool CharacterNeedsEscaping(char ch)
        {
            if (((ch != '/') && (ch != '"')) && ((ch >= ' ') && (ch != '\\')))
            {
                if (ch < 0xd800)
                {
                    return false;
                }
                if (ch > 0xdfff)
                {
                    return (ch >= 0xfffe);
                }
            }
            return true;
        }

        private void CheckText(JsonNodeType nextNodeType)
        {
            if (this.IsClosed)
            {
                ThrowClosed();
            }
            if (this.depth == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlIllegalOutsideRoot")));
            }
            if ((nextNodeType == JsonNodeType.StandaloneText) && (this.nodeType == JsonNodeType.QuotedText))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonCannotWriteStandaloneTextAfterQuotedText")));
            }
        }

        public override void Close()
        {
            if (!this.IsClosed)
            {
                try
                {
                    this.WriteEndDocument();
                }
                finally
                {
                    try
                    {
                        this.nodeWriter.Flush();
                        this.nodeWriter.Close();
                    }
                    finally
                    {
                        this.writeState = System.Xml.WriteState.Closed;
                        if (this.depth != 0)
                        {
                            this.depth = 0;
                        }
                    }
                }
            }
        }

        private void EnterScope(JsonNodeType currentNodeType)
        {
            this.depth++;
            if (this.scopes == null)
            {
                this.scopes = new JsonNodeType[4];
            }
            else if (this.scopes.Length == this.depth)
            {
                JsonNodeType[] destinationArray = new JsonNodeType[this.depth * 2];
                Array.Copy(this.scopes, destinationArray, this.depth);
                this.scopes = destinationArray;
            }
            this.scopes[this.depth] = currentNodeType;
        }

        private JsonNodeType ExitScope()
        {
            JsonNodeType type = this.scopes[this.depth];
            this.scopes[this.depth] = JsonNodeType.None;
            this.depth--;
            return type;
        }

        public override void Flush()
        {
            if (this.IsClosed)
            {
                ThrowClosed();
            }
            this.nodeWriter.Flush();
        }

        private void InitializeWriter()
        {
            this.nodeType = JsonNodeType.None;
            this.dataType = JsonDataType.None;
            this.isWritingDataTypeAttribute = false;
            this.wroteServerTypeAttribute = false;
            this.isWritingServerTypeAttribute = false;
            this.serverTypeValue = null;
            this.attributeText = null;
            if (this.depth != 0)
            {
                this.depth = 0;
            }
            if ((this.scopes != null) && (this.scopes.Length > 0x19))
            {
                this.scopes = null;
            }
            this.writeState = System.Xml.WriteState.Start;
            this.endElementBuffer = false;
        }

        public override string LookupPrefix(string ns)
        {
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");
            }
            if (ns == "http://www.w3.org/2000/xmlns/")
            {
                return "xmlns";
            }
            if (ns == "http://www.w3.org/XML/1998/namespace")
            {
                return "xml";
            }
            if (ns == string.Empty)
            {
                return string.Empty;
            }
            return null;
        }

        public void SetOutput(Stream stream, Encoding encoding, bool ownsStream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            if (encoding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoding");
            }
            if (encoding.WebName != Encoding.UTF8.WebName)
            {
                stream = new JsonEncodingStreamWrapper(stream, encoding, false);
            }
            else
            {
                encoding = null;
            }
            if (this.nodeWriter == null)
            {
                this.nodeWriter = new JsonNodeWriter();
            }
            this.nodeWriter.SetOutput(stream, ownsStream, encoding);
            this.InitializeWriter();
        }

        private void StartText()
        {
            if (this.HasOpenAttribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("JsonMustUseWriteStringForWritingAttributeValues")));
            }
            if ((this.dataType == JsonDataType.None) && (this.serverTypeValue != null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonMustSpecifyDataType", new object[] { "type", "object", "__type" })));
            }
            if (this.IsWritingNameWithMapping && !this.WrittenNameWithMapping)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonMustSpecifyDataType", new object[] { "item", string.Empty, "item" })));
            }
            if ((this.dataType == JsonDataType.String) || (this.dataType == JsonDataType.None))
            {
                this.CheckText(JsonNodeType.QuotedText);
                if (this.nodeType != JsonNodeType.QuotedText)
                {
                    this.WriteJsonQuote();
                }
                this.nodeType = JsonNodeType.QuotedText;
            }
            else if ((this.dataType == JsonDataType.Number) || (this.dataType == JsonDataType.Boolean))
            {
                this.CheckText(JsonNodeType.StandaloneText);
                this.nodeType = JsonNodeType.StandaloneText;
            }
            else
            {
                this.ThrowInvalidAttributeContent();
            }
        }

        private static void ThrowClosed()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("JsonWriterClosed")));
        }

        private void ThrowIfServerTypeWritten(string dataTypeSpecified)
        {
            if (this.serverTypeValue != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonInvalidDataTypeSpecifiedForServerType", new object[] { "type", dataTypeSpecified, "__type", "object" })));
            }
        }

        private void ThrowInvalidAttributeContent()
        {
            if (this.HasOpenAttribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonInvalidMethodBetweenStartEndAttribute")));
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonCannotWriteTextAfterNonTextAttribute", new object[] { this.dataType.ToString().ToLowerInvariant() })));
        }

        private bool TrySetWritingNameWithMapping(string localName, string ns)
        {
            if (localName.Equals("item") && ns.Equals("item"))
            {
                this.nameState = NameState.IsWritingNameWithMapping;
                return true;
            }
            return false;
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, bool[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, DateTime[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, decimal[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, double[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, Guid[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, short[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, int[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, long[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, float[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, TimeSpan[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, bool[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, DateTime[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, decimal[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, double[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, Guid[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, short[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, int[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, long[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, float[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, TimeSpan[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonWriteArrayNotSupported")));
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
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
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("JsonSizeExceedsRemainingBufferSpace", new object[] { buffer.Length - index })));
            }
            this.StartText();
            this.nodeWriter.WriteBase64Text(buffer, 0, buffer, index, count);
        }

        public override void WriteBinHex(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
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
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("JsonSizeExceedsRemainingBufferSpace", new object[] { buffer.Length - index })));
            }
            this.StartText();
            this.WriteEscapedJsonString(BinHexEncoding.GetString(buffer, index, count));
        }

        public override void WriteCData(string text)
        {
            this.WriteString(text);
        }

        public override void WriteCharEntity(char ch)
        {
            this.WriteString(ch.ToString());
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
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
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("JsonSizeExceedsRemainingBufferSpace", new object[] { buffer.Length - index })));
            }
            this.WriteString(new string(buffer, index, count));
        }

        public override void WriteComment(string text)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonMethodNotSupported", new object[] { "WriteComment" })));
        }

        private void WriteDataTypeServerType()
        {
            switch (this.dataType)
            {
                case JsonDataType.Object:
                    this.EnterScope(JsonNodeType.Object);
                    this.nodeWriter.WriteText(0x7b);
                    break;

                case JsonDataType.Array:
                    this.EnterScope(JsonNodeType.Collection);
                    this.nodeWriter.WriteText(0x5b);
                    break;

                case JsonDataType.Null:
                    this.nodeWriter.WriteText("null");
                    break;

                case JsonDataType.None:
                    return;
            }
            if (this.serverTypeValue != null)
            {
                this.WriteServerTypeAttribute();
            }
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonMethodNotSupported", new object[] { "WriteDocType" })));
        }

        public override void WriteEndAttribute()
        {
            if (this.IsClosed)
            {
                ThrowClosed();
            }
            if (!this.HasOpenAttribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonNoMatchingStartAttribute")));
            }
            if (!this.isWritingDataTypeAttribute)
            {
                if (this.isWritingServerTypeAttribute)
                {
                    this.serverTypeValue = this.attributeText;
                    this.attributeText = null;
                    this.isWritingServerTypeAttribute = false;
                    if ((!this.IsWritingNameWithMapping || this.WrittenNameWithMapping) && (this.dataType == JsonDataType.Object))
                    {
                        this.WriteServerTypeAttribute();
                    }
                }
                else if (this.IsWritingNameAttribute)
                {
                    this.WriteJsonElementName(this.attributeText);
                    this.attributeText = null;
                    this.nameState = NameState.WrittenNameWithMapping | NameState.IsWritingNameWithMapping;
                    this.WriteDataTypeServerType();
                }
                else if (this.isWritingXmlnsAttribute)
                {
                    if (!string.IsNullOrEmpty(this.attributeText) && this.isWritingXmlnsAttributeDefaultNs)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("ns", System.Runtime.Serialization.SR.GetString("JsonNamespaceMustBeEmpty", new object[] { this.attributeText }));
                    }
                    this.attributeText = null;
                    this.isWritingXmlnsAttribute = false;
                    this.isWritingXmlnsAttributeDefaultNs = false;
                }
            }
            else
            {
                switch (this.attributeText)
                {
                    case "number":
                        this.ThrowIfServerTypeWritten("number");
                        this.dataType = JsonDataType.Number;
                        break;

                    case "string":
                        this.ThrowIfServerTypeWritten("string");
                        this.dataType = JsonDataType.String;
                        break;

                    case "array":
                        this.ThrowIfServerTypeWritten("array");
                        this.dataType = JsonDataType.Array;
                        break;

                    case "object":
                        this.dataType = JsonDataType.Object;
                        break;

                    case "null":
                        this.ThrowIfServerTypeWritten("null");
                        this.dataType = JsonDataType.Null;
                        break;

                    case "boolean":
                        this.ThrowIfServerTypeWritten("boolean");
                        this.dataType = JsonDataType.Boolean;
                        break;

                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonUnexpectedAttributeValue", new object[] { this.attributeText })));
                }
                this.attributeText = null;
                this.isWritingDataTypeAttribute = false;
                if (!this.IsWritingNameWithMapping || this.WrittenNameWithMapping)
                {
                    this.WriteDataTypeServerType();
                }
            }
        }

        public override void WriteEndDocument()
        {
            if (this.IsClosed)
            {
                ThrowClosed();
            }
            if (this.nodeType != JsonNodeType.None)
            {
                while (this.depth > 0)
                {
                    this.WriteEndElement();
                }
            }
        }

        public override void WriteEndElement()
        {
            if (this.IsClosed)
            {
                ThrowClosed();
            }
            if (this.depth == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonEndElementNoOpenNodes")));
            }
            if (this.HasOpenAttribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonOpenAttributeMustBeClosedFirst", new object[] { "WriteEndElement" })));
            }
            this.endElementBuffer = false;
            JsonNodeType type = this.ExitScope();
            if (type == JsonNodeType.Collection)
            {
                this.nodeWriter.WriteText(0x5d);
                type = this.ExitScope();
            }
            else if (this.nodeType == JsonNodeType.QuotedText)
            {
                this.WriteJsonQuote();
            }
            else if (this.nodeType == JsonNodeType.Element)
            {
                if ((this.dataType == JsonDataType.None) && (this.serverTypeValue != null))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonMustSpecifyDataType", new object[] { "type", "object", "__type" })));
                }
                if (this.IsWritingNameWithMapping && !this.WrittenNameWithMapping)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonMustSpecifyDataType", new object[] { "item", string.Empty, "item" })));
                }
                if ((this.dataType == JsonDataType.None) || (this.dataType == JsonDataType.String))
                {
                    this.nodeWriter.WriteText(0x22);
                    this.nodeWriter.WriteText(0x22);
                }
            }
            if (this.depth != 0)
            {
                switch (type)
                {
                    case JsonNodeType.Element:
                        this.endElementBuffer = true;
                        break;

                    case JsonNodeType.Object:
                        this.nodeWriter.WriteText(0x7d);
                        if ((this.depth > 0) && (this.scopes[this.depth] == JsonNodeType.Element))
                        {
                            this.ExitScope();
                            this.endElementBuffer = true;
                        }
                        break;
                }
            }
            this.dataType = JsonDataType.None;
            this.nodeType = JsonNodeType.EndElement;
            this.nameState = NameState.None;
            this.wroteServerTypeAttribute = false;
        }

        public override void WriteEntityRef(string name)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonMethodNotSupported", new object[] { "WriteEntityRef" })));
        }

        [SecuritySafeCritical]
        private unsafe void WriteEscapedJsonString(string str)
        {
            fixed (char* str2 = ((char*) str))
            {
                char* chPtr = str2;
                int num = 0;
                int index = 0;
                while (index < str.Length)
                {
                    char ch = chPtr[index];
                    if (ch <= '/')
                    {
                        switch (ch)
                        {
                            case '/':
                            case '"':
                                this.nodeWriter.WriteChars(chPtr + num, index - num);
                                this.nodeWriter.WriteText(0x5c);
                                this.nodeWriter.WriteText(ch);
                                num = index + 1;
                                goto Label_0186;
                        }
                        if (ch < ' ')
                        {
                            this.nodeWriter.WriteChars(chPtr + num, index - num);
                            this.nodeWriter.WriteText(0x5c);
                            this.nodeWriter.WriteText(0x75);
                            this.nodeWriter.WriteText(string.Format(CultureInfo.InvariantCulture, "{0:x4}", new object[] { (int) ch }));
                            num = index + 1;
                        }
                    }
                    else if (ch == '\\')
                    {
                        this.nodeWriter.WriteChars(chPtr + num, index - num);
                        this.nodeWriter.WriteText(0x5c);
                        this.nodeWriter.WriteText(ch);
                        num = index + 1;
                    }
                    else if ((ch >= 0xd800) && ((ch <= 0xdfff) || (ch >= 0xfffe)))
                    {
                        this.nodeWriter.WriteChars(chPtr + num, index - num);
                        this.nodeWriter.WriteText(0x5c);
                        this.nodeWriter.WriteText(0x75);
                        this.nodeWriter.WriteText(string.Format(CultureInfo.InvariantCulture, "{0:x4}", new object[] { (int) ch }));
                        num = index + 1;
                    }
                Label_0186:
                    index++;
                }
                if (num < index)
                {
                    this.nodeWriter.WriteChars(chPtr + num, index - num);
                }
            }
        }

        public override void WriteFullEndElement()
        {
            this.WriteEndElement();
        }

        private void WriteJsonElementName(string localName)
        {
            this.WriteJsonQuote();
            this.WriteEscapedJsonString(localName);
            this.WriteJsonQuote();
            this.nodeWriter.WriteText(0x3a);
        }

        private void WriteJsonQuote()
        {
            this.nodeWriter.WriteText(0x22);
        }

        private void WritePrimitiveValue(object value)
        {
            if (this.IsClosed)
            {
                ThrowClosed();
            }
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
            }
            if (value is ulong)
            {
                this.WriteValue((ulong) value);
            }
            else if (value is string)
            {
                this.WriteValue((string) value);
            }
            else if (value is int)
            {
                this.WriteValue((int) value);
            }
            else if (value is long)
            {
                this.WriteValue((long) value);
            }
            else if (value is bool)
            {
                this.WriteValue((bool) value);
            }
            else if (value is double)
            {
                this.WriteValue((double) value);
            }
            else if (value is DateTime)
            {
                this.WriteValue((DateTime) value);
            }
            else if (value is float)
            {
                this.WriteValue((float) value);
            }
            else if (value is decimal)
            {
                this.WriteValue((decimal) value);
            }
            else if (value is XmlDictionaryString)
            {
                this.WriteValue((XmlDictionaryString) value);
            }
            else if (value is UniqueId)
            {
                this.WriteValue((UniqueId) value);
            }
            else if (value is Guid)
            {
                this.WriteValue((Guid) value);
            }
            else if (value is TimeSpan)
            {
                this.WriteValue((TimeSpan) value);
            }
            else
            {
                if (value.GetType().IsArray)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("JsonNestedArraysNotSupported"), "value"));
                }
                base.WriteValue(value);
            }
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            if (this.IsClosed)
            {
                ThrowClosed();
            }
            if (!name.Equals("xml", StringComparison.OrdinalIgnoreCase))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("JsonXmlProcessingInstructionNotSupported"), "name"));
            }
            if (this.WriteState != System.Xml.WriteState.Start)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonXmlInvalidDeclaration")));
            }
        }

        public override void WriteQualifiedName(string localName, string ns)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            if (localName.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("localName", System.Runtime.Serialization.SR.GetString("JsonInvalidLocalNameEmpty"));
            }
            if (ns == null)
            {
                ns = string.Empty;
            }
            base.WriteQualifiedName(localName, ns);
        }

        public override void WriteRaw(string data)
        {
            this.WriteString(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
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
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("JsonSizeExceedsRemainingBufferSpace", new object[] { buffer.Length - index })));
            }
            this.WriteString(new string(buffer, index, count));
        }

        private void WriteServerTypeAttribute()
        {
            string serverTypeValue = this.serverTypeValue;
            JsonDataType dataType = this.dataType;
            NameState nameState = this.nameState;
            base.WriteStartElement("__type");
            this.WriteValue(serverTypeValue);
            this.WriteEndElement();
            this.dataType = dataType;
            this.nameState = nameState;
            this.wroteServerTypeAttribute = true;
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            if (this.IsClosed)
            {
                ThrowClosed();
            }
            if (!string.IsNullOrEmpty(prefix))
            {
                if (!this.IsWritingNameWithMapping || (prefix != "xmlns"))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("prefix", System.Runtime.Serialization.SR.GetString("JsonPrefixMustBeNullOrEmpty", new object[] { prefix }));
                }
                if ((ns != null) && (ns != "http://www.w3.org/2000/xmlns/"))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlPrefixBoundToNamespace", new object[] { "xmlns", "http://www.w3.org/2000/xmlns/", ns }), "ns"));
                }
            }
            else if ((this.IsWritingNameWithMapping && (ns == "http://www.w3.org/2000/xmlns/")) && (localName != "xmlns"))
            {
                prefix = "xmlns";
            }
            if (!string.IsNullOrEmpty(ns))
            {
                if (!this.IsWritingNameWithMapping || (ns != "http://www.w3.org/2000/xmlns/"))
                {
                    if ((!string.IsNullOrEmpty(prefix) || (localName != "xmlns")) || (ns != "http://www.w3.org/2000/xmlns/"))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("ns", System.Runtime.Serialization.SR.GetString("JsonNamespaceMustBeEmpty", new object[] { ns }));
                    }
                    prefix = "xmlns";
                    this.isWritingXmlnsAttributeDefaultNs = true;
                }
                else
                {
                    prefix = "xmlns";
                }
            }
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            if (localName.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("localName", System.Runtime.Serialization.SR.GetString("JsonInvalidLocalNameEmpty"));
            }
            if ((this.nodeType != JsonNodeType.Element) && !this.wroteServerTypeAttribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonAttributeMustHaveElement")));
            }
            if (this.HasOpenAttribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonOpenAttributeMustBeClosedFirst", new object[] { "WriteStartAttribute" })));
            }
            if (prefix == "xmlns")
            {
                this.isWritingXmlnsAttribute = true;
            }
            else if (localName == "type")
            {
                if (this.dataType != JsonDataType.None)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonAttributeAlreadyWritten", new object[] { "type" })));
                }
                this.isWritingDataTypeAttribute = true;
            }
            else if (localName == "__type")
            {
                if (this.serverTypeValue != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonAttributeAlreadyWritten", new object[] { "__type" })));
                }
                if ((this.dataType != JsonDataType.None) && (this.dataType != JsonDataType.Object))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonServerTypeSpecifiedForInvalidDataType", new object[] { "__type", "type", this.dataType.ToString().ToLowerInvariant(), "object" })));
                }
                this.isWritingServerTypeAttribute = true;
            }
            else
            {
                if (localName != "item")
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("localName", System.Runtime.Serialization.SR.GetString("JsonUnexpectedAttributeLocalName", new object[] { localName }));
                }
                if (this.WrittenNameWithMapping)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonAttributeAlreadyWritten", new object[] { "item" })));
                }
                if (!this.IsWritingNameWithMapping)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonEndElementNoOpenNodes")));
                }
                this.nameState |= NameState.IsWritingNameAttribute;
            }
        }

        public override void WriteStartDocument()
        {
            if (this.IsClosed)
            {
                ThrowClosed();
            }
            if (this.WriteState != System.Xml.WriteState.Start)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonInvalidWriteState", new object[] { "WriteStartDocument", this.WriteState.ToString() })));
            }
        }

        public override void WriteStartDocument(bool standalone)
        {
            this.WriteStartDocument();
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            if (localName.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("localName", System.Runtime.Serialization.SR.GetString("JsonInvalidLocalNameEmpty"));
            }
            if (!string.IsNullOrEmpty(prefix) && (string.IsNullOrEmpty(ns) || !this.TrySetWritingNameWithMapping(localName, ns)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("prefix", System.Runtime.Serialization.SR.GetString("JsonPrefixMustBeNullOrEmpty", new object[] { prefix }));
            }
            if (!string.IsNullOrEmpty(ns) && !this.TrySetWritingNameWithMapping(localName, ns))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("ns", System.Runtime.Serialization.SR.GetString("JsonNamespaceMustBeEmpty", new object[] { ns }));
            }
            if (this.IsClosed)
            {
                ThrowClosed();
            }
            if (this.HasOpenAttribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonOpenAttributeMustBeClosedFirst", new object[] { "WriteStartElement" })));
            }
            if ((this.nodeType != JsonNodeType.None) && (this.depth == 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonMultipleRootElementsNotAllowedOnWriter")));
            }
            switch (this.nodeType)
            {
                case JsonNodeType.None:
                    if (!localName.Equals("root"))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonInvalidRootElementName", new object[] { localName, "root" })));
                    }
                    this.EnterScope(JsonNodeType.Element);
                    break;

                case JsonNodeType.Element:
                    if ((this.dataType != JsonDataType.Array) && (this.dataType != JsonDataType.Object))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonNodeTypeArrayOrObjectNotSpecified")));
                    }
                    if (!this.IsWritingCollection)
                    {
                        if (this.nameState != NameState.IsWritingNameWithMapping)
                        {
                            this.WriteJsonElementName(localName);
                        }
                    }
                    else if (!localName.Equals("item"))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonInvalidItemNameForArrayElement", new object[] { localName, "item" })));
                    }
                    this.EnterScope(JsonNodeType.Element);
                    break;

                case JsonNodeType.EndElement:
                    if (this.endElementBuffer)
                    {
                        this.nodeWriter.WriteText(0x2c);
                    }
                    if (!this.IsWritingCollection)
                    {
                        if (this.nameState != NameState.IsWritingNameWithMapping)
                        {
                            this.WriteJsonElementName(localName);
                        }
                    }
                    else if (!localName.Equals("item"))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonInvalidItemNameForArrayElement", new object[] { localName, "item" })));
                    }
                    this.EnterScope(JsonNodeType.Element);
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonInvalidStartElementCall")));
            }
            this.isWritingDataTypeAttribute = false;
            this.isWritingServerTypeAttribute = false;
            this.isWritingXmlnsAttribute = false;
            this.wroteServerTypeAttribute = false;
            this.serverTypeValue = null;
            this.dataType = JsonDataType.None;
            this.nodeType = JsonNodeType.Element;
        }

        public override void WriteString(string text)
        {
            if (this.HasOpenAttribute && (text != null))
            {
                this.attributeText = this.attributeText + text;
            }
            else
            {
                if (text == null)
                {
                    text = string.Empty;
                }
                if ((((this.dataType != JsonDataType.Array) && (this.dataType != JsonDataType.Object)) && (this.nodeType != JsonNodeType.EndElement)) || !XmlConverter.IsWhitespace(text))
                {
                    this.StartText();
                    this.WriteEscapedJsonString(text);
                }
            }
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            this.WriteString(highChar + lowChar);
        }

        private void WriteValue(Array array)
        {
            JsonDataType dataType = this.dataType;
            this.dataType = JsonDataType.String;
            this.StartText();
            for (int i = 0; i < array.Length; i++)
            {
                if (i != 0)
                {
                    this.nodeWriter.WriteText(0x20);
                }
                this.WritePrimitiveValue(array.GetValue(i));
            }
            this.dataType = dataType;
        }

        public override void WriteValue(bool value)
        {
            this.StartText();
            this.nodeWriter.WriteBoolText(value);
        }

        public override void WriteValue(DateTime value)
        {
            this.StartText();
            this.nodeWriter.WriteDateTimeText(value);
        }

        public override void WriteValue(decimal value)
        {
            this.StartText();
            this.nodeWriter.WriteDecimalText(value);
        }

        public override void WriteValue(double value)
        {
            this.StartText();
            this.nodeWriter.WriteDoubleText(value);
        }

        public override void WriteValue(Guid value)
        {
            this.StartText();
            this.nodeWriter.WriteGuidText(value);
        }

        public override void WriteValue(int value)
        {
            this.StartText();
            this.nodeWriter.WriteInt32Text(value);
        }

        public override void WriteValue(long value)
        {
            this.StartText();
            this.nodeWriter.WriteInt64Text(value);
        }

        public override void WriteValue(object value)
        {
            if (this.IsClosed)
            {
                ThrowClosed();
            }
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            if (value is Array)
            {
                this.WriteValue((Array) value);
            }
            else if (value is IStreamProvider)
            {
                this.WriteValue((IStreamProvider) value);
            }
            else
            {
                this.WritePrimitiveValue(value);
            }
        }

        public override void WriteValue(float value)
        {
            this.StartText();
            this.nodeWriter.WriteFloatText(value);
        }

        public override void WriteValue(string value)
        {
            this.WriteString(value);
        }

        public override void WriteValue(TimeSpan value)
        {
            this.StartText();
            this.nodeWriter.WriteTimeSpanText(value);
        }

        private void WriteValue(ulong value)
        {
            this.StartText();
            this.nodeWriter.WriteUInt64Text(value);
        }

        public override void WriteValue(UniqueId value)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            this.StartText();
            this.nodeWriter.WriteUniqueIdText(value);
        }

        public override void WriteWhitespace(string ws)
        {
            if (this.IsClosed)
            {
                ThrowClosed();
            }
            if (ws == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ws");
            }
            for (int i = 0; i < ws.Length; i++)
            {
                char ch = ws[i];
                if (((ch != ' ') && (ch != '\t')) && ((ch != '\n') && (ch != '\r')))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("ws", System.Runtime.Serialization.SR.GetString("JsonOnlyWhitespace", new object[] { ch.ToString(), "WriteWhitespace" }));
                }
            }
            this.WriteString(ws);
        }

        public override void WriteXmlAttribute(string localName, string value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonMethodNotSupported", new object[] { "WriteXmlAttribute" })));
        }

        public override void WriteXmlAttribute(XmlDictionaryString localName, XmlDictionaryString value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonMethodNotSupported", new object[] { "WriteXmlAttribute" })));
        }

        public override void WriteXmlnsAttribute(string prefix, string namespaceUri)
        {
            if (!this.IsWritingNameWithMapping)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonMethodNotSupported", new object[] { "WriteXmlnsAttribute" })));
            }
        }

        public override void WriteXmlnsAttribute(string prefix, XmlDictionaryString namespaceUri)
        {
            if (!this.IsWritingNameWithMapping)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonMethodNotSupported", new object[] { "WriteXmlnsAttribute" })));
            }
        }

        private static System.Text.BinHexEncoding BinHexEncoding
        {
            [SecuritySafeCritical]
            get
            {
                if (binHexEncoding == null)
                {
                    binHexEncoding = new System.Text.BinHexEncoding();
                }
                return binHexEncoding;
            }
        }

        private bool HasOpenAttribute
        {
            get
            {
                if ((!this.isWritingDataTypeAttribute && !this.isWritingServerTypeAttribute) && !this.IsWritingNameAttribute)
                {
                    return this.isWritingXmlnsAttribute;
                }
                return true;
            }
        }

        private bool IsClosed
        {
            get
            {
                return (this.WriteState == System.Xml.WriteState.Closed);
            }
        }

        private bool IsWritingCollection
        {
            get
            {
                return ((this.depth > 0) && (this.scopes[this.depth] == JsonNodeType.Collection));
            }
        }

        private bool IsWritingNameAttribute
        {
            get
            {
                return ((this.nameState & NameState.IsWritingNameAttribute) == NameState.IsWritingNameAttribute);
            }
        }

        private bool IsWritingNameWithMapping
        {
            get
            {
                return ((this.nameState & NameState.IsWritingNameWithMapping) == NameState.IsWritingNameWithMapping);
            }
        }

        public override XmlWriterSettings Settings
        {
            get
            {
                return null;
            }
        }

        public override System.Xml.WriteState WriteState
        {
            get
            {
                if (this.writeState == System.Xml.WriteState.Closed)
                {
                    return System.Xml.WriteState.Closed;
                }
                if (this.HasOpenAttribute)
                {
                    return System.Xml.WriteState.Attribute;
                }
                switch (this.nodeType)
                {
                    case JsonNodeType.None:
                        return System.Xml.WriteState.Start;

                    case JsonNodeType.Element:
                        return System.Xml.WriteState.Element;

                    case JsonNodeType.EndElement:
                    case JsonNodeType.QuotedText:
                    case JsonNodeType.StandaloneText:
                        return System.Xml.WriteState.Content;
                }
                return System.Xml.WriteState.Error;
            }
        }

        private bool WrittenNameWithMapping
        {
            get
            {
                return ((this.nameState & NameState.WrittenNameWithMapping) == NameState.WrittenNameWithMapping);
            }
        }

        public override string XmlLang
        {
            get
            {
                return null;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return System.Xml.XmlSpace.None;
            }
        }

        private enum JsonDataType
        {
            None,
            Null,
            Boolean,
            Number,
            String,
            Object,
            Array
        }

        private class JsonNodeWriter : XmlUTF8NodeWriter
        {
            [SecurityCritical]
            internal unsafe void WriteChars(char* chars, int charCount)
            {
                base.UnsafeWriteUTF8Chars(chars, charCount);
            }
        }

        [Flags]
        private enum NameState
        {
            IsWritingNameAttribute = 2,
            IsWritingNameWithMapping = 1,
            None = 0,
            WrittenNameWithMapping = 4
        }
    }
}

