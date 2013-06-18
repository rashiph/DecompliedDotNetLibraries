namespace System.Xml
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Text;

    internal abstract class XmlBaseWriter : XmlDictionaryWriter, IFragmentCapableXmlDictionaryWriter
    {
        private string attributeLocalName;
        private string attributeValue;
        private static System.Text.BinHexEncoding binhexEncoding;
        private int depth;
        private DocumentState documentState = DocumentState.None;
        private Element[] elements;
        private bool inList;
        private bool isXmlAttribute;
        private bool isXmlnsAttribute;
        private XmlStreamNodeWriter nodeWriter;
        private NamespaceManager nsMgr = new NamespaceManager();
        private int oldNamespaceBoundary;
        private Stream oldStream;
        private XmlNodeWriter oldWriter;
        private static string[] prefixes = new string[] { 
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", 
            "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
         };
        private XmlSigningNodeWriter signingWriter;
        private XmlUTF8NodeWriter textFragmentWriter;
        private int trailByteCount;
        private byte[] trailBytes;
        private XmlNodeWriter writer;
        private System.Xml.WriteState writeState = System.Xml.WriteState.Start;
        private const string xmlNamespace = "http://www.w3.org/XML/1998/namespace";
        private const string xmlnsNamespace = "http://www.w3.org/2000/xmlns/";

        protected XmlBaseWriter()
        {
        }

        private void AutoComplete(System.Xml.WriteState writeState)
        {
            if (this.writeState == System.Xml.WriteState.Element)
            {
                this.EndStartElement();
            }
            this.writeState = writeState;
        }

        public override void Close()
        {
            if (!this.IsClosed)
            {
                try
                {
                    this.FinishDocument();
                    this.AutoComplete(System.Xml.WriteState.Closed);
                    this.writer.Flush();
                }
                finally
                {
                    this.nsMgr.Close();
                    if (this.depth != 0)
                    {
                        this.elements = null;
                        this.depth = 0;
                    }
                    this.attributeValue = null;
                    this.attributeLocalName = null;
                    this.nodeWriter.Close();
                    if (this.signingWriter != null)
                    {
                        this.signingWriter.Close();
                    }
                    if (this.textFragmentWriter != null)
                    {
                        this.textFragmentWriter.Close();
                    }
                    this.oldWriter = null;
                    this.oldStream = null;
                }
            }
        }

        protected abstract XmlSigningNodeWriter CreateSigningNodeWriter();
        protected void EndArray()
        {
        }

        public override void EndCanonicalization()
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (!this.Signing)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlCanonicalizationNotStarted")));
            }
            this.signingWriter.Flush();
            this.writer = this.signingWriter.NodeWriter;
        }

        protected void EndComment()
        {
        }

        protected void EndContent()
        {
        }

        public void EndFragment()
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if ((this.oldStream == null) && (this.oldWriter == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
            }
            if (this.WriteState == System.Xml.WriteState.Attribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidWriteState", new object[] { "EndFragment", this.WriteState.ToString() })));
            }
            this.FlushElement();
            this.writer.Flush();
            if (this.Signing)
            {
                if (this.oldWriter != null)
                {
                    this.signingWriter.NodeWriter = this.oldWriter;
                }
                else
                {
                    ((XmlStreamNodeWriter) this.signingWriter.NodeWriter).Stream = this.oldStream;
                }
            }
            else if (this.oldWriter != null)
            {
                this.writer = this.oldWriter;
            }
            else
            {
                this.nodeWriter.Stream = this.oldStream;
            }
            this.NamespaceBoundary = this.oldNamespaceBoundary;
            this.oldWriter = null;
            this.oldStream = null;
        }

        private void EndStartElement()
        {
            this.nsMgr.DeclareNamespaces(this.writer);
            this.writer.WriteEndStartElement(false);
        }

        private Element EnterScope()
        {
            this.nsMgr.EnterScope();
            this.depth++;
            if (this.elements == null)
            {
                this.elements = new Element[4];
            }
            else if (this.elements.Length == this.depth)
            {
                Element[] destinationArray = new Element[this.depth * 2];
                Array.Copy(this.elements, destinationArray, this.depth);
                this.elements = destinationArray;
            }
            Element element = this.elements[this.depth];
            if (element == null)
            {
                element = new Element();
                this.elements[this.depth] = element;
            }
            return element;
        }

        private void ExitScope()
        {
            this.elements[this.depth].Clear();
            this.depth--;
            if ((this.depth == 0) && (this.documentState == DocumentState.Document))
            {
                this.documentState = DocumentState.Epilog;
            }
            this.nsMgr.ExitScope();
        }

        private void FinishDocument()
        {
            if (this.writeState == System.Xml.WriteState.Attribute)
            {
                this.WriteEndAttribute();
            }
            while (this.depth > 0)
            {
                this.WriteEndElement();
            }
        }

        public override void Flush()
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            this.writer.Flush();
        }

        private void FlushBase64()
        {
            if (this.trailByteCount > 0)
            {
                this.FlushTrailBytes();
            }
        }

        protected void FlushElement()
        {
            if (this.writeState == System.Xml.WriteState.Element)
            {
                this.AutoComplete(System.Xml.WriteState.Content);
            }
        }

        private void FlushTrailBytes()
        {
            if (this.attributeValue != null)
            {
                this.WriteAttributeText(XmlConverter.Base64Encoding.GetString(this.trailBytes, 0, this.trailByteCount));
            }
            if (!this.isXmlnsAttribute)
            {
                this.StartContent();
                this.writer.WriteBase64Text(this.trailBytes, this.trailByteCount, this.trailBytes, 0, 0);
                this.EndContent();
            }
            this.trailByteCount = 0;
        }

        private string GeneratePrefix(string ns, XmlDictionaryString xNs)
        {
            if ((this.writeState != System.Xml.WriteState.Element) && (this.writeState != System.Xml.WriteState.Attribute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidPrefixState", new object[] { this.WriteState.ToString() })));
            }
            string prefix = this.nsMgr.AddNamespace(ns, xNs);
            if (prefix == null)
            {
                do
                {
                    int num2;
                    Element element1 = this.elements[this.depth];
                    element1.PrefixId = (num2 = element1.PrefixId) + 1;
                    prefix = "d" + this.depth.ToString(CultureInfo.InvariantCulture) + "p" + num2.ToString(CultureInfo.InvariantCulture);
                }
                while (this.nsMgr.LookupNamespace(prefix) != null);
                this.nsMgr.AddNamespace(prefix, ns, xNs);
            }
            return prefix;
        }

        private string GetQualifiedNamePrefix(string namespaceUri, XmlDictionaryString xNs)
        {
            string prefix = this.nsMgr.LookupPrefix(namespaceUri);
            if (prefix != null)
            {
                return prefix;
            }
            if (this.writeState != System.Xml.WriteState.Attribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlNamespaceNotFound", new object[] { namespaceUri }), "namespaceUri"));
            }
            return this.GeneratePrefix(namespaceUri, xNs);
        }

        private bool IsWhitespace(char ch)
        {
            if (((ch != ' ') && (ch != '\n')) && (ch != '\r'))
            {
                return (ch == 't');
            }
            return true;
        }

        internal string LookupNamespace(string prefix)
        {
            if (prefix == null)
            {
                return null;
            }
            return this.nsMgr.LookupNamespace(prefix);
        }

        public override string LookupPrefix(string ns)
        {
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("ns"));
            }
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            return this.nsMgr.LookupPrefix(ns);
        }

        protected void SetOutput(XmlStreamNodeWriter writer)
        {
            this.inList = false;
            this.writer = writer;
            this.nodeWriter = writer;
            this.writeState = System.Xml.WriteState.Start;
            this.documentState = DocumentState.None;
            this.nsMgr.Clear();
            if (this.depth != 0)
            {
                this.elements = null;
                this.depth = 0;
            }
            this.attributeLocalName = null;
            this.attributeValue = null;
            this.oldWriter = null;
            this.oldStream = null;
        }

        protected void SignScope(XmlCanonicalWriter signingWriter)
        {
            this.nsMgr.Sign(signingWriter);
        }

        protected void StartArray(int count)
        {
            this.FlushBase64();
            if (this.documentState == DocumentState.Epilog)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlOnlyOneRoot")));
            }
            if (((this.documentState == DocumentState.Document) && (count > 1)) && (this.depth == 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlOnlyOneRoot")));
            }
            if (this.writeState == System.Xml.WriteState.Attribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidWriteState", new object[] { "WriteStartElement", this.WriteState.ToString() })));
            }
            this.AutoComplete(System.Xml.WriteState.Content);
        }

        private void StartAttribute(ref string prefix, string localName, string ns, XmlDictionaryString xNs)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (this.writeState == System.Xml.WriteState.Attribute)
            {
                this.WriteEndAttribute();
            }
            if ((localName == null) || ((localName.Length == 0) && (prefix != "xmlns")))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            }
            if (this.writeState != System.Xml.WriteState.Element)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidWriteState", new object[] { "WriteStartAttribute", this.WriteState.ToString() })));
            }
            if (prefix == null)
            {
                if ((ns == "http://www.w3.org/2000/xmlns/") && (localName != "xmlns"))
                {
                    prefix = "xmlns";
                }
                else if (ns == "http://www.w3.org/XML/1998/namespace")
                {
                    prefix = "xml";
                }
                else
                {
                    prefix = string.Empty;
                }
            }
            if ((prefix.Length == 0) && (localName == "xmlns"))
            {
                prefix = "xmlns";
                localName = string.Empty;
            }
            this.isXmlnsAttribute = false;
            this.isXmlAttribute = false;
            if (prefix == "xml")
            {
                if ((ns != null) && (ns != "http://www.w3.org/XML/1998/namespace"))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlPrefixBoundToNamespace", new object[] { "xml", "http://www.w3.org/XML/1998/namespace", ns }), "ns"));
                }
                this.isXmlAttribute = true;
                this.attributeValue = string.Empty;
                this.attributeLocalName = localName;
            }
            else if (prefix == "xmlns")
            {
                if ((ns != null) && (ns != "http://www.w3.org/2000/xmlns/"))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlPrefixBoundToNamespace", new object[] { "xmlns", "http://www.w3.org/2000/xmlns/", ns }), "ns"));
                }
                this.isXmlnsAttribute = true;
                this.attributeValue = string.Empty;
                this.attributeLocalName = localName;
            }
            else if (ns == null)
            {
                if (prefix.Length != 0)
                {
                    ns = this.nsMgr.LookupNamespace(prefix);
                    if (ns == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlUndefinedPrefix", new object[] { prefix }), "prefix"));
                    }
                }
                else
                {
                    ns = string.Empty;
                }
            }
            else if (ns.Length == 0)
            {
                if (prefix.Length != 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlEmptyNamespaceRequiresNullPrefix"), "prefix"));
                }
            }
            else if (prefix.Length == 0)
            {
                prefix = this.nsMgr.LookupAttributePrefix(ns);
                if (prefix == null)
                {
                    if ((ns.Length == "http://www.w3.org/2000/xmlns/".Length) && (ns == "http://www.w3.org/2000/xmlns/"))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlSpecificBindingNamespace", new object[] { "xmlns", ns })));
                    }
                    if ((ns.Length == "http://www.w3.org/XML/1998/namespace".Length) && (ns == "http://www.w3.org/XML/1998/namespace"))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlSpecificBindingNamespace", new object[] { "xml", ns })));
                    }
                    prefix = this.GeneratePrefix(ns, xNs);
                }
            }
            else
            {
                this.nsMgr.AddNamespaceIfNotDeclared(prefix, ns, xNs);
            }
            this.writeState = System.Xml.WriteState.Attribute;
        }

        public override void StartCanonicalization(Stream stream, bool includeComments, string[] inclusivePrefixes)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (this.Signing)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlCanonicalizationStarted")));
            }
            this.FlushElement();
            if (this.signingWriter == null)
            {
                this.signingWriter = this.CreateSigningNodeWriter();
            }
            this.signingWriter.SetOutput(this.writer, stream, includeComments, inclusivePrefixes);
            this.writer = this.signingWriter;
            this.SignScope(this.signingWriter.CanonicalWriter);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected void StartComment()
        {
            this.FlushElement();
        }

        protected void StartContent()
        {
            this.FlushElement();
            if (this.depth == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlIllegalOutsideRoot")));
            }
        }

        protected void StartContent(char ch)
        {
            this.FlushElement();
            if (this.depth == 0)
            {
                this.VerifyWhitespace(ch);
            }
        }

        protected void StartContent(string s)
        {
            this.FlushElement();
            if (this.depth == 0)
            {
                this.VerifyWhitespace(s);
            }
        }

        protected void StartContent(char[] chars, int offset, int count)
        {
            this.FlushElement();
            if (this.depth == 0)
            {
                this.VerifyWhitespace(chars, offset, count);
            }
        }

        private void StartElement(ref string prefix, string localName, string ns, XmlDictionaryString xNs)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (this.documentState == DocumentState.Epilog)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlOnlyOneRoot")));
            }
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            }
            if (localName.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("InvalidLocalNameEmpty"), "localName"));
            }
            if (this.writeState == System.Xml.WriteState.Attribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidWriteState", new object[] { "WriteStartElement", this.WriteState.ToString() })));
            }
            this.FlushBase64();
            this.AutoComplete(System.Xml.WriteState.Element);
            Element element = this.EnterScope();
            if (ns == null)
            {
                if (prefix == null)
                {
                    prefix = string.Empty;
                }
                ns = this.nsMgr.LookupNamespace(prefix);
                if (ns == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlUndefinedPrefix", new object[] { prefix }), "prefix"));
                }
            }
            else if (prefix == null)
            {
                prefix = this.nsMgr.LookupPrefix(ns);
                if (prefix == null)
                {
                    prefix = string.Empty;
                    this.nsMgr.AddNamespace(string.Empty, ns, xNs);
                }
            }
            else
            {
                this.nsMgr.AddNamespaceIfNotDeclared(prefix, ns, xNs);
            }
            element.Prefix = prefix;
            element.LocalName = localName;
        }

        public void StartFragment(Stream stream, bool generateSelfContainedTextFragment)
        {
            if (!this.CanFragment)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("stream"));
            }
            if ((this.oldStream != null) || (this.oldWriter != null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
            }
            if (this.WriteState == System.Xml.WriteState.Attribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidWriteState", new object[] { "StartFragment", this.WriteState.ToString() })));
            }
            this.FlushElement();
            this.writer.Flush();
            this.oldNamespaceBoundary = this.NamespaceBoundary;
            XmlStreamNodeWriter textFragmentWriter = null;
            if (generateSelfContainedTextFragment)
            {
                this.NamespaceBoundary = this.depth + 1;
                if (this.textFragmentWriter == null)
                {
                    this.textFragmentWriter = new XmlUTF8NodeWriter();
                }
                this.textFragmentWriter.SetOutput(stream, false, Encoding.UTF8);
                textFragmentWriter = this.textFragmentWriter;
            }
            if (this.Signing)
            {
                if (textFragmentWriter != null)
                {
                    this.oldWriter = this.signingWriter.NodeWriter;
                    this.signingWriter.NodeWriter = textFragmentWriter;
                }
                else
                {
                    this.oldStream = ((XmlStreamNodeWriter) this.signingWriter.NodeWriter).Stream;
                    ((XmlStreamNodeWriter) this.signingWriter.NodeWriter).Stream = stream;
                }
            }
            else if (textFragmentWriter != null)
            {
                this.oldWriter = this.writer;
                this.writer = textFragmentWriter;
            }
            else
            {
                this.oldStream = this.nodeWriter.Stream;
                this.nodeWriter.Stream = stream;
            }
        }

        protected void ThrowClosed()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlWriterClosed")));
        }

        private void VerifyWhitespace(char ch)
        {
            if (!this.IsWhitespace(ch))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlIllegalOutsideRoot")));
            }
        }

        private void VerifyWhitespace(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (!this.IsWhitespace(s[i]))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlIllegalOutsideRoot")));
                }
            }
        }

        private void VerifyWhitespace(char[] chars, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!this.IsWhitespace(chars[offset + i]))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlIllegalOutsideRoot")));
                }
            }
        }

        private void WriteAttributeText(string value)
        {
            if (this.attributeValue.Length == 0)
            {
                this.attributeValue = value;
            }
            else
            {
                this.attributeValue = this.attributeValue + value;
            }
        }

        public override void WriteBase64(byte[] buffer, int offset, int count)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count > (buffer.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { buffer.Length - offset })));
            }
            if (count > 0)
            {
                if (this.trailByteCount > 0)
                {
                    while ((this.trailByteCount < 3) && (count > 0))
                    {
                        this.trailBytes[this.trailByteCount++] = buffer[offset++];
                        count--;
                    }
                }
                int num = this.trailByteCount + count;
                int num2 = num - (num % 3);
                if (this.trailBytes == null)
                {
                    this.trailBytes = new byte[3];
                }
                if (num2 >= 3)
                {
                    if (this.attributeValue != null)
                    {
                        this.WriteAttributeText(XmlConverter.Base64Encoding.GetString(this.trailBytes, 0, this.trailByteCount));
                        this.WriteAttributeText(XmlConverter.Base64Encoding.GetString(buffer, offset, num2 - this.trailByteCount));
                    }
                    if (!this.isXmlnsAttribute)
                    {
                        this.StartContent();
                        this.writer.WriteBase64Text(this.trailBytes, this.trailByteCount, buffer, offset, num2 - this.trailByteCount);
                        this.EndContent();
                    }
                    this.trailByteCount = num - num2;
                    if (this.trailByteCount > 0)
                    {
                        int num3 = (offset + count) - this.trailByteCount;
                        for (int i = 0; i < this.trailByteCount; i++)
                        {
                            this.trailBytes[i] = buffer[num3++];
                        }
                    }
                }
                else
                {
                    Buffer.BlockCopy(buffer, offset, this.trailBytes, this.trailByteCount, count);
                    this.trailByteCount += count;
                }
            }
        }

        public override void WriteBinHex(byte[] buffer, int offset, int count)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count > (buffer.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { buffer.Length - offset })));
            }
            this.WriteRaw(BinHexEncoding.GetString(buffer, offset, count));
        }

        public override void WriteCData(string text)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (this.writeState == System.Xml.WriteState.Attribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidWriteState", new object[] { "WriteCData", this.WriteState.ToString() })));
            }
            if (text == null)
            {
                text = string.Empty;
            }
            if (text.Length > 0)
            {
                this.StartContent();
                this.FlushBase64();
                this.writer.WriteCData(text);
                this.EndContent();
            }
        }

        public override void WriteCharEntity(char ch)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if ((ch >= 0xd800) && (ch <= 0xdfff))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlMissingLowSurrogate"), "ch"));
            }
            if (this.attributeValue != null)
            {
                this.WriteAttributeText(ch.ToString());
            }
            if (!this.isXmlnsAttribute)
            {
                this.StartContent(ch);
                this.FlushBase64();
                this.writer.WriteCharEntity(ch);
                this.EndContent();
            }
        }

        public override void WriteChars(char[] chars, int offset, int count)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (chars == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count > (chars.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { chars.Length - offset })));
            }
            if (count > 0)
            {
                this.FlushBase64();
                if (this.attributeValue != null)
                {
                    this.WriteAttributeText(new string(chars, offset, count));
                }
                if (!this.isXmlnsAttribute)
                {
                    this.StartContent(chars, offset, count);
                    this.writer.WriteEscapedText(chars, offset, count);
                    this.EndContent();
                }
            }
        }

        public override void WriteComment(string text)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (this.writeState == System.Xml.WriteState.Attribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidWriteState", new object[] { "WriteComment", this.WriteState.ToString() })));
            }
            if (text == null)
            {
                text = string.Empty;
            }
            else if ((text.IndexOf("--", StringComparison.Ordinal) != -1) || ((text.Length > 0) && (text[text.Length - 1] == '-')))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlInvalidCommentChars"), "text"));
            }
            this.StartComment();
            this.FlushBase64();
            this.writer.WriteComment(text);
            this.EndComment();
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("XmlMethodNotSupported", new object[] { "WriteDocType" })));
        }

        public override void WriteEndAttribute()
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (this.writeState != System.Xml.WriteState.Attribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidWriteState", new object[] { "WriteEndAttribute", this.WriteState.ToString() })));
            }
            this.FlushBase64();
            try
            {
                if (this.isXmlAttribute)
                {
                    if (this.attributeLocalName == "lang")
                    {
                        this.nsMgr.AddLangAttribute(this.attributeValue);
                    }
                    else if (this.attributeLocalName == "space")
                    {
                        if (this.attributeValue != "preserve")
                        {
                            if (this.attributeValue != "default")
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlInvalidXmlSpace", new object[] { this.attributeValue })));
                            }
                            this.nsMgr.AddSpaceAttribute(System.Xml.XmlSpace.Default);
                        }
                        else
                        {
                            this.nsMgr.AddSpaceAttribute(System.Xml.XmlSpace.Preserve);
                        }
                    }
                    this.isXmlAttribute = false;
                    this.attributeLocalName = null;
                    this.attributeValue = null;
                }
                if (this.isXmlnsAttribute)
                {
                    this.nsMgr.AddNamespaceIfNotDeclared(this.attributeLocalName, this.attributeValue, null);
                    this.isXmlnsAttribute = false;
                    this.attributeLocalName = null;
                    this.attributeValue = null;
                }
                else
                {
                    this.writer.WriteEndAttribute();
                }
            }
            finally
            {
                this.writeState = System.Xml.WriteState.Element;
            }
        }

        public override void WriteEndDocument()
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if ((this.writeState == System.Xml.WriteState.Start) || (this.writeState == System.Xml.WriteState.Prolog))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlNoRootElement")));
            }
            this.FinishDocument();
            this.writeState = System.Xml.WriteState.Start;
            this.documentState = DocumentState.End;
        }

        public override void WriteEndElement()
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (this.depth == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidDepth", new object[] { "WriteEndElement", this.depth.ToString(CultureInfo.InvariantCulture) })));
            }
            if (this.writeState == System.Xml.WriteState.Attribute)
            {
                this.WriteEndAttribute();
            }
            this.FlushBase64();
            if (this.writeState == System.Xml.WriteState.Element)
            {
                this.nsMgr.DeclareNamespaces(this.writer);
                this.writer.WriteEndStartElement(true);
            }
            else
            {
                Element element = this.elements[this.depth];
                this.writer.WriteEndElement(element.Prefix, element.LocalName);
            }
            this.ExitScope();
            this.writeState = System.Xml.WriteState.Content;
        }

        public override void WriteEntityRef(string name)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("XmlMethodNotSupported", new object[] { "WriteEntityRef" })));
        }

        public void WriteFragment(byte[] buffer, int offset, int count)
        {
            if (!this.CanFragment)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count > (buffer.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { buffer.Length - offset })));
            }
            if (this.WriteState == System.Xml.WriteState.Attribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidWriteState", new object[] { "WriteFragment", this.WriteState.ToString() })));
            }
            if (this.writer != this.nodeWriter)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
            }
            this.FlushElement();
            this.FlushBase64();
            this.nodeWriter.Flush();
            this.nodeWriter.Stream.Write(buffer, offset, count);
        }

        public override void WriteFullEndElement()
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (this.writeState == System.Xml.WriteState.Attribute)
            {
                this.WriteEndAttribute();
            }
            if ((this.writeState != System.Xml.WriteState.Element) && (this.writeState != System.Xml.WriteState.Content))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidWriteState", new object[] { "WriteFullEndElement", this.WriteState.ToString() })));
            }
            this.AutoComplete(System.Xml.WriteState.Content);
            this.WriteEndElement();
        }

        public override void WriteName(string name)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            this.WriteString(name);
        }

        public override void WriteNmToken(string name)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("XmlMethodNotSupported", new object[] { "WriteNmToken" })));
        }

        protected void WritePrimitiveValue(object value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlNestedArraysNotSupported"), "value"));
                }
                base.WriteValue(value);
            }
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (name != "xml")
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlProcessingInstructionNotSupported"), "name"));
            }
            if (this.writeState != System.Xml.WriteState.Start)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidDeclaration")));
            }
            this.writer.WriteDeclaration();
        }

        public override void WriteQualifiedName(string localName, string namespaceUri)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            }
            if (localName.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("InvalidLocalNameEmpty"), "localName"));
            }
            if (namespaceUri == null)
            {
                namespaceUri = string.Empty;
            }
            string qualifiedNamePrefix = this.GetQualifiedNamePrefix(namespaceUri, null);
            if (qualifiedNamePrefix.Length != 0)
            {
                this.WriteString(qualifiedNamePrefix);
                this.WriteString(":");
            }
            this.WriteString(localName);
        }

        public override void WriteQualifiedName(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            }
            if (localName.Value.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("InvalidLocalNameEmpty"), "localName"));
            }
            if (namespaceUri == null)
            {
                namespaceUri = XmlDictionaryString.Empty;
            }
            string qualifiedNamePrefix = this.GetQualifiedNamePrefix(namespaceUri.Value, namespaceUri);
            this.FlushBase64();
            if (this.attributeValue != null)
            {
                this.WriteAttributeText(qualifiedNamePrefix + ":" + namespaceUri.Value);
            }
            if (!this.isXmlnsAttribute)
            {
                this.StartContent();
                this.writer.WriteQualifiedName(qualifiedNamePrefix, localName);
                this.EndContent();
            }
        }

        public override void WriteRaw(string value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (value == null)
            {
                value = string.Empty;
            }
            if (value.Length > 0)
            {
                this.FlushBase64();
                if (this.attributeValue != null)
                {
                    this.WriteAttributeText(value);
                }
                if (!this.isXmlnsAttribute)
                {
                    this.StartContent(value);
                    this.writer.WriteText(value);
                    this.EndContent();
                }
            }
        }

        public override void WriteRaw(char[] chars, int offset, int count)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (chars == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count > (chars.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { chars.Length - offset })));
            }
            if (count > 0)
            {
                this.FlushBase64();
                if (this.attributeValue != null)
                {
                    this.WriteAttributeText(new string(chars, offset, count));
                }
                if (!this.isXmlnsAttribute)
                {
                    this.StartContent(chars, offset, count);
                    this.writer.WriteText(chars, offset, count);
                    this.EndContent();
                }
            }
        }

        public override void WriteStartAttribute(string prefix, string localName, string namespaceUri)
        {
            this.StartAttribute(ref prefix, localName, namespaceUri, null);
            if (!this.isXmlnsAttribute)
            {
                this.writer.WriteStartAttribute(prefix, localName);
            }
        }

        public override void WriteStartAttribute(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            this.StartAttribute(ref prefix, (localName != null) ? localName.Value : null, (namespaceUri != null) ? namespaceUri.Value : null, namespaceUri);
            if (!this.isXmlnsAttribute)
            {
                this.writer.WriteStartAttribute(prefix, localName);
            }
        }

        public override void WriteStartDocument()
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (this.writeState != System.Xml.WriteState.Start)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidWriteState", new object[] { "WriteStartDocument", this.WriteState.ToString() })));
            }
            this.writeState = System.Xml.WriteState.Prolog;
            this.documentState = DocumentState.Document;
            this.writer.WriteDeclaration();
        }

        public override void WriteStartDocument(bool standalone)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            this.WriteStartDocument();
        }

        public override void WriteStartElement(string prefix, string localName, string namespaceUri)
        {
            this.StartElement(ref prefix, localName, namespaceUri, null);
            this.writer.WriteStartElement(prefix, localName);
        }

        public override void WriteStartElement(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            this.StartElement(ref prefix, (localName != null) ? localName.Value : null, (namespaceUri != null) ? namespaceUri.Value : null, namespaceUri);
            this.writer.WriteStartElement(prefix, localName);
        }

        public override void WriteString(string value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (value == null)
            {
                value = string.Empty;
            }
            if ((value.Length > 0) || this.inList)
            {
                this.FlushBase64();
                if (this.attributeValue != null)
                {
                    this.WriteAttributeText(value);
                }
                if (!this.isXmlnsAttribute)
                {
                    this.StartContent(value);
                    this.writer.WriteEscapedText(value);
                    this.EndContent();
                }
            }
        }

        public override void WriteString(XmlDictionaryString value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            if (value.Value.Length > 0)
            {
                this.FlushBase64();
                if (this.attributeValue != null)
                {
                    this.WriteAttributeText(value.Value);
                }
                if (!this.isXmlnsAttribute)
                {
                    this.StartContent(value.Value);
                    this.writer.WriteEscapedText(value);
                    this.EndContent();
                }
            }
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            SurrogateChar ch = new SurrogateChar(lowChar, highChar);
            if (this.attributeValue != null)
            {
                char[] chArray = new char[] { highChar, lowChar };
                this.WriteAttributeText(new string(chArray));
            }
            if (!this.isXmlnsAttribute)
            {
                this.StartContent();
                this.FlushBase64();
                this.writer.WriteCharEntity(ch.Char);
                this.EndContent();
            }
        }

        private void WriteValue(Array array)
        {
            this.FlushBase64();
            this.StartContent();
            this.writer.WriteStartListText();
            this.inList = true;
            for (int i = 0; i < array.Length; i++)
            {
                if (i != 0)
                {
                    this.writer.WriteListSeparator();
                }
                this.WritePrimitiveValue(array.GetValue(i));
            }
            this.inList = false;
            this.writer.WriteEndListText();
            this.EndContent();
        }

        public override void WriteValue(bool value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            this.FlushBase64();
            if (this.attributeValue != null)
            {
                this.WriteAttributeText(XmlConverter.ToString(value));
            }
            if (!this.isXmlnsAttribute)
            {
                this.StartContent();
                this.writer.WriteBoolText(value);
                this.EndContent();
            }
        }

        public override void WriteValue(DateTime value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            this.FlushBase64();
            if (this.attributeValue != null)
            {
                this.WriteAttributeText(XmlConverter.ToString(value));
            }
            if (!this.isXmlnsAttribute)
            {
                this.StartContent();
                this.writer.WriteDateTimeText(value);
                this.EndContent();
            }
        }

        public override void WriteValue(decimal value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            this.FlushBase64();
            if (this.attributeValue != null)
            {
                this.WriteAttributeText(XmlConverter.ToString(value));
            }
            if (!this.isXmlnsAttribute)
            {
                this.StartContent();
                this.writer.WriteDecimalText(value);
                this.EndContent();
            }
        }

        public override void WriteValue(double value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            this.FlushBase64();
            if (this.attributeValue != null)
            {
                this.WriteAttributeText(XmlConverter.ToString(value));
            }
            if (!this.isXmlnsAttribute)
            {
                this.StartContent();
                this.writer.WriteDoubleText(value);
                this.EndContent();
            }
        }

        public override void WriteValue(Guid value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            this.FlushBase64();
            if (this.attributeValue != null)
            {
                this.WriteAttributeText(XmlConverter.ToString(value));
            }
            if (!this.isXmlnsAttribute)
            {
                this.StartContent();
                this.writer.WriteGuidText(value);
                this.EndContent();
            }
        }

        public override void WriteValue(int value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            this.FlushBase64();
            if (this.attributeValue != null)
            {
                this.WriteAttributeText(XmlConverter.ToString(value));
            }
            if (!this.isXmlnsAttribute)
            {
                this.StartContent();
                this.writer.WriteInt32Text(value);
                this.EndContent();
            }
        }

        public override void WriteValue(long value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            this.FlushBase64();
            if (this.attributeValue != null)
            {
                this.WriteAttributeText(XmlConverter.ToString(value));
            }
            if (!this.isXmlnsAttribute)
            {
                this.StartContent();
                this.writer.WriteInt64Text(value);
                this.EndContent();
            }
        }

        public override void WriteValue(object value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
            }
            if (value is object[])
            {
                this.WriteValue((object[]) value);
            }
            else if (value is Array)
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
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            this.FlushBase64();
            if (this.attributeValue != null)
            {
                this.WriteAttributeText(XmlConverter.ToString(value));
            }
            if (!this.isXmlnsAttribute)
            {
                this.StartContent();
                this.writer.WriteFloatText(value);
                this.EndContent();
            }
        }

        public override void WriteValue(string value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            this.WriteString(value);
        }

        public override void WriteValue(TimeSpan value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            this.FlushBase64();
            if (this.attributeValue != null)
            {
                this.WriteAttributeText(XmlConverter.ToString(value));
            }
            if (!this.isXmlnsAttribute)
            {
                this.StartContent();
                this.writer.WriteTimeSpanText(value);
                this.EndContent();
            }
        }

        private void WriteValue(ulong value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            this.FlushBase64();
            if (this.attributeValue != null)
            {
                this.WriteAttributeText(XmlConverter.ToString(value));
            }
            if (!this.isXmlnsAttribute)
            {
                this.StartContent();
                this.writer.WriteUInt64Text(value);
                this.EndContent();
            }
        }

        public override void WriteValue(UniqueId value)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            this.FlushBase64();
            if (this.attributeValue != null)
            {
                this.WriteAttributeText(XmlConverter.ToString(value));
            }
            if (!this.isXmlnsAttribute)
            {
                this.StartContent();
                this.writer.WriteUniqueIdText(value);
                this.EndContent();
            }
        }

        public override void WriteValue(XmlDictionaryString value)
        {
            this.WriteString(value);
        }

        private void WriteValue(object[] array)
        {
            this.FlushBase64();
            this.StartContent();
            this.writer.WriteStartListText();
            this.inList = true;
            for (int i = 0; i < array.Length; i++)
            {
                if (i != 0)
                {
                    this.writer.WriteListSeparator();
                }
                this.WritePrimitiveValue(array[i]);
            }
            this.inList = false;
            this.writer.WriteEndListText();
            this.EndContent();
        }

        public override void WriteWhitespace(string whitespace)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (whitespace == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("whitespace");
            }
            for (int i = 0; i < whitespace.Length; i++)
            {
                char ch = whitespace[i];
                if (((ch != ' ') && (ch != '\t')) && ((ch != '\n') && (ch != '\r')))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlOnlyWhitespace"), "whitespace"));
                }
            }
            this.WriteString(whitespace);
        }

        public override void WriteXmlnsAttribute(string prefix, string ns)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");
            }
            if (this.writeState != System.Xml.WriteState.Element)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidWriteState", new object[] { "WriteXmlnsAttribute", this.WriteState.ToString() })));
            }
            if (prefix == null)
            {
                prefix = this.nsMgr.LookupPrefix(ns);
                if (prefix == null)
                {
                    this.GeneratePrefix(ns, null);
                }
            }
            else
            {
                this.nsMgr.AddNamespaceIfNotDeclared(prefix, ns, null);
            }
        }

        public override void WriteXmlnsAttribute(string prefix, XmlDictionaryString ns)
        {
            if (this.IsClosed)
            {
                this.ThrowClosed();
            }
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");
            }
            if (this.writeState != System.Xml.WriteState.Element)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidWriteState", new object[] { "WriteXmlnsAttribute", this.WriteState.ToString() })));
            }
            if (prefix == null)
            {
                prefix = this.nsMgr.LookupPrefix(ns.Value);
                if (prefix == null)
                {
                    this.GeneratePrefix(ns.Value, ns);
                }
            }
            else
            {
                this.nsMgr.AddNamespaceIfNotDeclared(prefix, ns.Value, ns);
            }
        }

        private static System.Text.BinHexEncoding BinHexEncoding
        {
            get
            {
                if (binhexEncoding == null)
                {
                    binhexEncoding = new System.Text.BinHexEncoding();
                }
                return binhexEncoding;
            }
        }

        public override bool CanCanonicalize
        {
            get
            {
                return true;
            }
        }

        public virtual bool CanFragment
        {
            get
            {
                return true;
            }
        }

        protected bool IsClosed
        {
            get
            {
                return (this.writeState == System.Xml.WriteState.Closed);
            }
        }

        protected int NamespaceBoundary
        {
            get
            {
                return this.nsMgr.NamespaceBoundary;
            }
            set
            {
                this.nsMgr.NamespaceBoundary = value;
            }
        }

        protected bool Signing
        {
            get
            {
                return (this.writer == this.signingWriter);
            }
        }

        public override System.Xml.WriteState WriteState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.writeState;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.nsMgr.XmlLang;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this.nsMgr.XmlSpace;
            }
        }

        private enum DocumentState : byte
        {
            Document = 1,
            End = 3,
            Epilog = 2,
            None = 0
        }

        private class Element
        {
            private string localName;
            private string prefix;
            private int prefixId;

            public void Clear()
            {
                this.prefix = null;
                this.localName = null;
                this.prefixId = 0;
            }

            public string LocalName
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.localName;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.localName = value;
                }
            }

            public string Prefix
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.prefix;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.prefix = value;
                }
            }

            public int PrefixId
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.prefixId;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.prefixId = value;
                }
            }
        }

        private class NamespaceManager
        {
            private int attributeCount;
            private XmlAttribute[] attributes;
            private Namespace defaultNamespace = new Namespace();
            private int depth;
            private string lang;
            private Namespace lastNameSpace;
            private int namespaceBoundary;
            private Namespace[] namespaces;
            private int nsCount;
            private int nsTop;
            private System.Xml.XmlSpace space;

            public NamespaceManager()
            {
                this.defaultNamespace.Depth = 0;
                this.defaultNamespace.Prefix = string.Empty;
                this.defaultNamespace.Uri = string.Empty;
                this.defaultNamespace.UriDictionaryString = null;
            }

            private void AddAttribute()
            {
                if (this.attributes == null)
                {
                    this.attributes = new XmlAttribute[1];
                }
                else if (this.attributes.Length == this.attributeCount)
                {
                    XmlAttribute[] destinationArray = new XmlAttribute[this.attributeCount * 2];
                    Array.Copy(this.attributes, destinationArray, this.attributeCount);
                    this.attributes = destinationArray;
                }
                XmlAttribute attribute = this.attributes[this.attributeCount];
                if (attribute == null)
                {
                    attribute = new XmlAttribute();
                    this.attributes[this.attributeCount] = attribute;
                }
                attribute.XmlLang = this.lang;
                attribute.XmlSpace = this.space;
                attribute.Depth = this.depth;
                this.attributeCount++;
            }

            public void AddLangAttribute(string lang)
            {
                this.AddAttribute();
                this.lang = lang;
            }

            public string AddNamespace(string uri, XmlDictionaryString uriDictionaryString)
            {
                if (uri.Length == 0)
                {
                    this.AddNamespaceIfNotDeclared(string.Empty, uri, uriDictionaryString);
                    return string.Empty;
                }
                for (int i = 0; i < XmlBaseWriter.prefixes.Length; i++)
                {
                    string prefix = XmlBaseWriter.prefixes[i];
                    bool flag = false;
                    for (int j = this.nsCount - 1; j >= this.nsTop; j--)
                    {
                        Namespace namespace2 = this.namespaces[j];
                        if (namespace2.Prefix == prefix)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        this.AddNamespace(prefix, uri, uriDictionaryString);
                        return prefix;
                    }
                }
                return null;
            }

            public void AddNamespace(string prefix, string uri, XmlDictionaryString uriDictionaryString)
            {
                if (((prefix.Length >= 3) && ((prefix[0] & '￟') == 0x58)) && (((prefix[1] & '￟') == 0x4d) && ((prefix[2] & '￟') == 0x4c)))
                {
                    if (((prefix != "xml") || (uri != "http://www.w3.org/XML/1998/namespace")) && ((prefix != "xmlns") || (uri != "http://www.w3.org/2000/xmlns/")))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlReservedPrefix"), "prefix"));
                    }
                }
                else
                {
                    Namespace namespace2;
                    for (int i = this.nsCount - 1; i >= 0; i--)
                    {
                        namespace2 = this.namespaces[i];
                        if (namespace2.Depth != this.depth)
                        {
                            break;
                        }
                        if (namespace2.Prefix == prefix)
                        {
                            if (namespace2.Uri != uri)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlPrefixBoundToNamespace", new object[] { prefix, namespace2.Uri, uri }), "prefix"));
                            }
                            return;
                        }
                    }
                    if ((prefix.Length != 0) && (uri.Length == 0))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlEmptyNamespaceRequiresNullPrefix"), "prefix"));
                    }
                    if ((uri.Length == "http://www.w3.org/2000/xmlns/".Length) && (uri == "http://www.w3.org/2000/xmlns/"))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlSpecificBindingNamespace", new object[] { "xmlns", uri })));
                    }
                    if (((uri.Length == "http://www.w3.org/XML/1998/namespace".Length) && (uri[0x12] == 'X')) && (uri == "http://www.w3.org/XML/1998/namespace"))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlSpecificBindingNamespace", new object[] { "xml", uri })));
                    }
                    if (this.namespaces.Length == this.nsCount)
                    {
                        Namespace[] destinationArray = new Namespace[this.nsCount * 2];
                        Array.Copy(this.namespaces, destinationArray, this.nsCount);
                        this.namespaces = destinationArray;
                    }
                    namespace2 = this.namespaces[this.nsCount];
                    if (namespace2 == null)
                    {
                        namespace2 = new Namespace();
                        this.namespaces[this.nsCount] = namespace2;
                    }
                    namespace2.Depth = this.depth;
                    namespace2.Prefix = prefix;
                    namespace2.Uri = uri;
                    namespace2.UriDictionaryString = uriDictionaryString;
                    this.nsCount++;
                    this.lastNameSpace = null;
                }
            }

            public void AddNamespaceIfNotDeclared(string prefix, string uri, XmlDictionaryString uriDictionaryString)
            {
                if (this.LookupNamespace(prefix) != uri)
                {
                    this.AddNamespace(prefix, uri, uriDictionaryString);
                }
            }

            public void AddSpaceAttribute(System.Xml.XmlSpace space)
            {
                this.AddAttribute();
                this.space = space;
            }

            public void Clear()
            {
                if (this.namespaces == null)
                {
                    this.namespaces = new Namespace[4];
                    this.namespaces[0] = this.defaultNamespace;
                }
                this.nsCount = 1;
                this.nsTop = 0;
                this.depth = 0;
                this.attributeCount = 0;
                this.space = System.Xml.XmlSpace.None;
                this.lang = null;
                this.lastNameSpace = null;
                this.namespaceBoundary = 0;
            }

            public void Close()
            {
                if (this.depth == 0)
                {
                    if ((this.namespaces != null) && (this.namespaces.Length > 0x20))
                    {
                        this.namespaces = null;
                    }
                    if ((this.attributes != null) && (this.attributes.Length > 4))
                    {
                        this.attributes = null;
                    }
                }
                else
                {
                    this.namespaces = null;
                    this.attributes = null;
                }
                this.lang = null;
            }

            public void DeclareNamespaces(XmlNodeWriter writer)
            {
                int nsCount = this.nsCount;
                while (nsCount > 0)
                {
                    Namespace namespace2 = this.namespaces[nsCount - 1];
                    if (namespace2.Depth != this.depth)
                    {
                        break;
                    }
                    nsCount--;
                }
                while (nsCount < this.nsCount)
                {
                    Namespace namespace3 = this.namespaces[nsCount];
                    if (namespace3.UriDictionaryString != null)
                    {
                        writer.WriteXmlnsAttribute(namespace3.Prefix, namespace3.UriDictionaryString);
                    }
                    else
                    {
                        writer.WriteXmlnsAttribute(namespace3.Prefix, namespace3.Uri);
                    }
                    nsCount++;
                }
            }

            public void EnterScope()
            {
                this.depth++;
            }

            public void ExitScope()
            {
                while (this.nsCount > 0)
                {
                    Namespace namespace2 = this.namespaces[this.nsCount - 1];
                    if (namespace2.Depth != this.depth)
                    {
                        break;
                    }
                    if (this.lastNameSpace == namespace2)
                    {
                        this.lastNameSpace = null;
                    }
                    namespace2.Clear();
                    this.nsCount--;
                }
                while (this.attributeCount > 0)
                {
                    XmlAttribute attribute = this.attributes[this.attributeCount - 1];
                    if (attribute.Depth != this.depth)
                    {
                        break;
                    }
                    this.space = attribute.XmlSpace;
                    this.lang = attribute.XmlLang;
                    attribute.Clear();
                    this.attributeCount--;
                }
                this.depth--;
            }

            public string LookupAttributePrefix(string ns)
            {
                if (((this.lastNameSpace != null) && (this.lastNameSpace.Uri == ns)) && (this.lastNameSpace.Prefix.Length != 0))
                {
                    return this.lastNameSpace.Prefix;
                }
                int nsCount = this.nsCount;
                for (int i = nsCount - 1; i >= this.nsTop; i--)
                {
                    Namespace namespace2 = this.namespaces[i];
                    if (object.ReferenceEquals(namespace2.Uri, ns))
                    {
                        string prefix = namespace2.Prefix;
                        if (prefix.Length != 0)
                        {
                            bool flag = false;
                            for (int k = i + 1; k < nsCount; k++)
                            {
                                if (this.namespaces[k].Prefix == prefix)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                this.lastNameSpace = namespace2;
                                return prefix;
                            }
                        }
                    }
                }
                for (int j = nsCount - 1; j >= this.nsTop; j--)
                {
                    Namespace namespace3 = this.namespaces[j];
                    if (namespace3.Uri == ns)
                    {
                        string str2 = namespace3.Prefix;
                        if (str2.Length != 0)
                        {
                            bool flag2 = false;
                            for (int m = j + 1; m < nsCount; m++)
                            {
                                if (this.namespaces[m].Prefix == str2)
                                {
                                    flag2 = true;
                                    break;
                                }
                            }
                            if (!flag2)
                            {
                                this.lastNameSpace = namespace3;
                                return str2;
                            }
                        }
                    }
                }
                if (ns.Length == 0)
                {
                    return string.Empty;
                }
                return null;
            }

            public string LookupNamespace(string prefix)
            {
                int nsCount = this.nsCount;
                if (prefix.Length == 0)
                {
                    for (int j = nsCount - 1; j >= this.nsTop; j--)
                    {
                        Namespace namespace2 = this.namespaces[j];
                        if (namespace2.Prefix.Length == 0)
                        {
                            return namespace2.Uri;
                        }
                    }
                    return string.Empty;
                }
                if (prefix.Length == 1)
                {
                    char ch = prefix[0];
                    for (int k = nsCount - 1; k >= this.nsTop; k--)
                    {
                        Namespace namespace3 = this.namespaces[k];
                        if (namespace3.PrefixChar == ch)
                        {
                            return namespace3.Uri;
                        }
                    }
                    return null;
                }
                for (int i = nsCount - 1; i >= this.nsTop; i--)
                {
                    Namespace namespace4 = this.namespaces[i];
                    if (namespace4.Prefix == prefix)
                    {
                        return namespace4.Uri;
                    }
                }
                if (prefix == "xmlns")
                {
                    return "http://www.w3.org/2000/xmlns/";
                }
                if (prefix == "xml")
                {
                    return "http://www.w3.org/XML/1998/namespace";
                }
                return null;
            }

            public string LookupPrefix(string ns)
            {
                if ((this.lastNameSpace != null) && (this.lastNameSpace.Uri == ns))
                {
                    return this.lastNameSpace.Prefix;
                }
                int nsCount = this.nsCount;
                for (int i = nsCount - 1; i >= this.nsTop; i--)
                {
                    Namespace namespace2 = this.namespaces[i];
                    if (object.ReferenceEquals(namespace2.Uri, ns))
                    {
                        string prefix = namespace2.Prefix;
                        bool flag = false;
                        for (int k = i + 1; k < nsCount; k++)
                        {
                            if (this.namespaces[k].Prefix == prefix)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            this.lastNameSpace = namespace2;
                            return prefix;
                        }
                    }
                }
                for (int j = nsCount - 1; j >= this.nsTop; j--)
                {
                    Namespace namespace3 = this.namespaces[j];
                    if (namespace3.Uri == ns)
                    {
                        string str2 = namespace3.Prefix;
                        bool flag2 = false;
                        for (int m = j + 1; m < nsCount; m++)
                        {
                            if (this.namespaces[m].Prefix == str2)
                            {
                                flag2 = true;
                                break;
                            }
                        }
                        if (!flag2)
                        {
                            this.lastNameSpace = namespace3;
                            return str2;
                        }
                    }
                }
                if (ns.Length == 0)
                {
                    bool flag3 = true;
                    for (int n = nsCount - 1; n >= this.nsTop; n--)
                    {
                        if (this.namespaces[n].Prefix.Length == 0)
                        {
                            flag3 = false;
                            break;
                        }
                    }
                    if (flag3)
                    {
                        return string.Empty;
                    }
                }
                if (ns == "http://www.w3.org/2000/xmlns/")
                {
                    return "xmlns";
                }
                if (ns == "http://www.w3.org/XML/1998/namespace")
                {
                    return "xml";
                }
                return null;
            }

            public void Sign(XmlCanonicalWriter signingWriter)
            {
                int nsCount = this.nsCount;
                for (int i = 1; i < nsCount; i++)
                {
                    Namespace namespace2 = this.namespaces[i];
                    bool flag = false;
                    for (int j = i + 1; (j < nsCount) && !flag; j++)
                    {
                        flag = namespace2.Prefix == this.namespaces[j].Prefix;
                    }
                    if (!flag)
                    {
                        signingWriter.WriteXmlnsAttribute(namespace2.Prefix, namespace2.Uri);
                    }
                }
            }

            public int NamespaceBoundary
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.namespaceBoundary;
                }
                set
                {
                    int index = 0;
                    while (index < this.nsCount)
                    {
                        if (this.namespaces[index].Depth >= value)
                        {
                            break;
                        }
                        index++;
                    }
                    this.nsTop = index;
                    this.namespaceBoundary = value;
                    this.lastNameSpace = null;
                }
            }

            public string XmlLang
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.lang;
                }
            }

            public System.Xml.XmlSpace XmlSpace
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.space;
                }
            }

            private class Namespace
            {
                private int depth;
                private string ns;
                private string prefix;
                private char prefixChar;
                private XmlDictionaryString xNs;

                public void Clear()
                {
                    this.prefix = null;
                    this.prefixChar = '\0';
                    this.ns = null;
                    this.xNs = null;
                    this.depth = 0;
                }

                public int Depth
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.depth;
                    }
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.depth = value;
                    }
                }

                public string Prefix
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.prefix;
                    }
                    set
                    {
                        if (value.Length == 1)
                        {
                            this.prefixChar = value[0];
                        }
                        else
                        {
                            this.prefixChar = '\0';
                        }
                        this.prefix = value;
                    }
                }

                public char PrefixChar
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.prefixChar;
                    }
                }

                public string Uri
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.ns;
                    }
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.ns = value;
                    }
                }

                public XmlDictionaryString UriDictionaryString
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.xNs;
                    }
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.xNs = value;
                    }
                }
            }

            private class XmlAttribute
            {
                private int depth;
                private string lang;
                private System.Xml.XmlSpace space;

                public void Clear()
                {
                    this.lang = null;
                }

                public int Depth
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.depth;
                    }
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.depth = value;
                    }
                }

                public string XmlLang
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.lang;
                    }
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.lang = value;
                    }
                }

                public System.Xml.XmlSpace XmlSpace
                {
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    get
                    {
                        return this.space;
                    }
                    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                    set
                    {
                        this.space = value;
                    }
                }
            }
        }
    }
}

