namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    [XmlRoot(ElementName="XPathMessageFilter", Namespace="http://schemas.microsoft.com/serviceModel/2004/05/xpathfilter"), XmlSchemaProvider("StaticGetSchema")]
    public class XPathMessageFilter : MessageFilter, IXmlSerializable
    {
        private const string DialectAttr = "Dialect";
        private static XPathQueryMatcher dummyMatcher = new XPathQueryMatcher(true);
        private const string InnerElem = "XPath";
        private XPathQueryMatcher matcher;
        private const string Namespace = "http://schemas.microsoft.com/serviceModel/2004/05/xpathfilter/";
        internal XmlNamespaceManager namespaces;
        internal const string NodeQuotaAttr = "NodeQuota";
        private const string OuterTypeName = "XPathMessageFilter";
        private const string RootNamespace = "http://schemas.microsoft.com/serviceModel/2004/05/xpathfilter";
        private const string WSEventingNamespace = "http://schemas.xmlsoap.org/ws/2004/06/eventing";
        private const string XmlnsP = "xmlns";
        private const string XmlP = "xml";
        private string xpath;
        internal const string XPathDialect = "http://www.w3.org/TR/1999/REC-xpath-19991116";

        public XPathMessageFilter() : this(string.Empty)
        {
        }

        public XPathMessageFilter(string xpath) : this(xpath, (XsltContext) new XPathMessageContext())
        {
        }

        public XPathMessageFilter(XmlReader reader) : this(reader, (XsltContext) new XPathMessageContext())
        {
        }

        public XPathMessageFilter(string xpath, XmlNamespaceManager namespaces)
        {
            if (xpath == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xpath");
            }
            this.Init(xpath, namespaces);
        }

        public XPathMessageFilter(string xpath, XsltContext context)
        {
            if (xpath == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xpath");
            }
            this.Init(xpath, context);
        }

        public XPathMessageFilter(XmlReader reader, XmlNamespaceManager namespaces)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            this.ReadFrom(reader, namespaces);
        }

        public XPathMessageFilter(XmlReader reader, XsltContext context)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            this.ReadFrom(reader, context);
        }

        private void Compile()
        {
            if (!this.matcher.IsCompiled)
            {
                this.EnsureMatcher();
                this.matcher.Compile(this.xpath, this.namespaces);
            }
        }

        internal void Compile(bool internalEngine)
        {
            this.EnsureMatcher();
            if (internalEngine)
            {
                this.matcher.CompileForInternal(this.xpath, this.namespaces);
            }
            else
            {
                this.matcher.CompileForExternal(this.xpath, this.namespaces);
            }
        }

        protected internal override IMessageFilterTable<FilterData> CreateFilterTable<FilterData>()
        {
            return new XPathMessageFilterTable<FilterData> { NodeQuota = this.NodeQuota };
        }

        private static XmlSchemaComplexType CreateOuterType()
        {
            XmlSchemaAttribute item = new XmlSchemaAttribute {
                Name = "Dialect",
                SchemaTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"),
                Use = XmlSchemaUse.Optional
            };
            XmlSchemaSimpleContentExtension extension = new XmlSchemaSimpleContentExtension {
                BaseTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema")
            };
            extension.Attributes.Add(item);
            XmlSchemaSimpleContent content = new XmlSchemaSimpleContent {
                Content = extension
            };
            XmlSchemaComplexType type = new XmlSchemaComplexType {
                ContentModel = content
            };
            XmlSchemaElement element = new XmlSchemaElement {
                Name = "XPath",
                SchemaType = type
            };
            XmlSchemaSequence sequence = new XmlSchemaSequence();
            sequence.Items.Add(element);
            XmlSchemaAttribute attribute2 = new XmlSchemaAttribute {
                Name = "NodeQuota",
                SchemaTypeName = new XmlQualifiedName("int", "http://www.w3.org/2001/XMLSchema"),
                Use = XmlSchemaUse.Optional
            };
            XmlSchemaAnyAttribute attribute3 = new XmlSchemaAnyAttribute();
            XmlSchemaComplexType type2 = new XmlSchemaComplexType {
                Name = "XPathMessageFilter",
                Particle = sequence
            };
            type2.Attributes.Add(attribute2);
            type2.AnyAttribute = attribute3;
            return type2;
        }

        private void EnsureMatcher()
        {
            if (this.matcher == dummyMatcher)
            {
                this.matcher = new XPathQueryMatcher(true);
            }
        }

        private void Init(string xpath, XmlNamespaceManager namespaces)
        {
            this.xpath = xpath;
            this.namespaces = namespaces;
            this.matcher = dummyMatcher;
            this.Compile();
        }

        public override bool Match(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            return this.ProcessResult(this.matcher.Match(message, false));
        }

        public override bool Match(MessageBuffer messageBuffer)
        {
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            return this.ProcessResult(this.matcher.Match(messageBuffer));
        }

        public bool Match(SeekableXPathNavigator navigator)
        {
            if (navigator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }
            return this.ProcessResult(this.matcher.Match(navigator));
        }

        public bool Match(XPathNavigator navigator)
        {
            if (navigator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }
            return this.ProcessResult(this.matcher.Match(navigator));
        }

        protected virtual XmlSchema OnGetSchema()
        {
            XmlSchemaComplexType item = CreateOuterType();
            XmlSchema schema = new XmlSchema();
            schema.Items.Add(item);
            schema.TargetNamespace = "http://schemas.microsoft.com/serviceModel/2004/05/xpathfilter/";
            return schema;
        }

        protected virtual void OnReadXml(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (!reader.IsStartElement())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("reader", System.ServiceModel.SR.GetString("FilterReaderNotStartElem"));
            }
            if (reader.IsEmptyElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("reader", System.ServiceModel.SR.GetString("FilterInvalidInner", new object[] { "XPath" }));
            }
            string s = null;
            while (reader.MoveToNextAttribute())
            {
                if ((QueryDataModel.IsAttribute(reader.NamespaceURI) && (reader.LocalName == "NodeQuota")) && (reader.NamespaceURI.Length == 0))
                {
                    s = reader.Value;
                    break;
                }
            }
            if (reader.NodeType == XmlNodeType.Attribute)
            {
                reader.MoveToElement();
            }
            int num = (s == null) ? 0x7fffffff : int.Parse(s, NumberFormatInfo.InvariantInfo);
            reader.ReadStartElement();
            reader.MoveToContent();
            if (reader.LocalName != "XPath")
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("reader", System.ServiceModel.SR.GetString("FilterInvalidInner", new object[] { "XPath" }));
            }
            this.ReadFrom(reader, new XPathMessageContext());
            reader.MoveToContent();
            reader.ReadEndElement();
            this.NodeQuota = num;
        }

        protected virtual void OnWriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            writer.WriteAttributeString("NodeQuota", this.NodeQuota.ToString(NumberFormatInfo.InvariantInfo));
            this.WriteXPathTo(writer, null, "XPath", null, true);
        }

        private bool ProcessResult(FilterResult result)
        {
            bool flag = result.Result;
            this.matcher.ReleaseResult(result);
            return flag;
        }

        private void ReadFrom(XmlReader reader, XmlNamespaceManager namespaces)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (!reader.IsStartElement())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("reader", System.ServiceModel.SR.GetString("FilterReaderNotStartElem"));
            }
            bool flag = false;
            string str = null;
            while (reader.MoveToNextAttribute())
            {
                if (QueryDataModel.IsAttribute(reader.NamespaceURI))
                {
                    if ((flag || (reader.LocalName != "Dialect")) || (reader.NamespaceURI != "http://schemas.xmlsoap.org/ws/2004/06/eventing"))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FilterInvalidAttribute")));
                    }
                    str = reader.Value;
                    flag = true;
                }
            }
            if (reader.NodeType == XmlNodeType.Attribute)
            {
                reader.MoveToElement();
            }
            if ((str != null) && (str != "http://www.w3.org/TR/1999/REC-xpath-19991116"))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FilterInvalidDialect", new object[] { "http://www.w3.org/TR/1999/REC-xpath-19991116" })));
            }
            bool isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (isEmptyElement)
            {
                this.Init(string.Empty, namespaces);
            }
            else
            {
                this.ReadXPath(reader, namespaces);
                reader.ReadEndElement();
            }
        }

        protected void ReadXPath(XmlReader reader, XmlNamespaceManager namespaces)
        {
            string xpath = reader.ReadString().Trim();
            if (xpath.Length != 0)
            {
                XPathLexer lexer = new XPathLexer(xpath, false);
                while (lexer.MoveNext())
                {
                    string prefix = lexer.Token.Prefix;
                    if (prefix.Length > 0)
                    {
                        string uri = null;
                        if (namespaces != null)
                        {
                            uri = namespaces.LookupNamespace(prefix);
                        }
                        if ((uri == null) || (uri.Length <= 0))
                        {
                            uri = reader.LookupNamespace(prefix);
                            if ((uri != null) && (uri.Length > 0))
                            {
                                if (namespaces == null)
                                {
                                    namespaces = new XPathMessageContext();
                                }
                                namespaces.AddNamespace(prefix, uri);
                            }
                        }
                    }
                }
            }
            this.Init(xpath, namespaces);
        }

        public static XmlSchemaType StaticGetSchema(XmlSchemaSet schemas)
        {
            if (schemas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("schemas");
            }
            XmlSchemaComplexType item = CreateOuterType();
            if (schemas.Contains("http://schemas.microsoft.com/serviceModel/2004/05/xpathfilter/"))
            {
                IEnumerator enumerator = schemas.Schemas("http://schemas.microsoft.com/serviceModel/2004/05/xpathfilter/").GetEnumerator();
                enumerator.MoveNext();
                ((XmlSchema) enumerator.Current).Items.Add(item);
                return item;
            }
            XmlSchema schema = new XmlSchema();
            schema.Items.Add(item);
            schema.TargetNamespace = "http://schemas.microsoft.com/serviceModel/2004/05/xpathfilter/";
            schemas.Add(schema);
            return item;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return this.OnGetSchema();
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            this.OnReadXml(reader);
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            this.OnWriteXml(writer);
        }

        public void TrimToSize()
        {
            this.matcher.Trim();
        }

        protected void WriteXPath(XmlWriter writer, IXmlNamespaceResolver resolver)
        {
            int startIndex = 0;
            int firstTokenChar = 0;
            string text = "";
            XPathLexer lexer = new XPathLexer(this.xpath, false);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            List<string> list = new List<string>();
            while (lexer.MoveNext())
            {
                string prefix = lexer.Token.Prefix;
                string str3 = resolver.LookupNamespace(prefix);
                if ((prefix.Length > 0) && ((str3 == null) || ((str3 != null) && (str3 != this.namespaces.LookupNamespace(prefix)))))
                {
                    if (this.xpath[firstTokenChar] == '$')
                    {
                        text = text + this.xpath.Substring(startIndex, (firstTokenChar - startIndex) + 1);
                        startIndex = firstTokenChar + 1;
                    }
                    else
                    {
                        text = text + this.xpath.Substring(startIndex, firstTokenChar - startIndex);
                        startIndex = firstTokenChar;
                    }
                    if (!dictionary.ContainsKey(prefix))
                    {
                        list.Add(prefix);
                        if (str3 != null)
                        {
                            string str4 = prefix;
                            for (int j = 0; (resolver.LookupNamespace(str4) != null) || (this.namespaces.LookupNamespace(str4) != null); j++)
                            {
                                str4 = str4 + j.ToString(NumberFormatInfo.InvariantInfo);
                            }
                            dictionary.Add(prefix, str4);
                        }
                        else
                        {
                            dictionary.Add(prefix, prefix);
                        }
                    }
                    text = text + dictionary[prefix];
                    startIndex += prefix.Length;
                }
                firstTokenChar = lexer.FirstTokenChar;
            }
            text = text + this.xpath.Substring(startIndex);
            for (int i = 0; i < list.Count; i++)
            {
                string str5 = list[i];
                writer.WriteAttributeString("xmlns", dictionary[str5], null, this.namespaces.LookupNamespace(str5));
            }
            writer.WriteString(text);
        }

        public void WriteXPathTo(XmlWriter writer, string prefix, string localName, string ns, bool writeNamespaces)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            if (localName.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("localName", System.ServiceModel.SR.GetString("FilterEmptyString"));
            }
            if (prefix == null)
            {
                prefix = string.Empty;
            }
            if (ns == null)
            {
                ns = string.Empty;
            }
            writer.WriteStartElement(prefix, localName, ns);
            XmlNamespaceManager resolver = new XmlNamespaceManager(new System.Xml.NameTable());
            if (!writeNamespaces)
            {
                foreach (string str in this.namespaces)
                {
                    if ((str != "xml") && (str != "xmlns"))
                    {
                        resolver.AddNamespace(str, this.namespaces.LookupNamespace(str));
                    }
                }
            }
            resolver.AddNamespace(prefix, ns);
            this.WriteXPath(writer, resolver);
            writer.WriteEndElement();
        }

        public XmlNamespaceManager Namespaces
        {
            get
            {
                return this.namespaces;
            }
        }

        public int NodeQuota
        {
            get
            {
                return this.matcher.NodeQuota;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("NodeQuota", value, System.ServiceModel.SR.GetString("FilterQuotaRange")));
                }
                this.EnsureMatcher();
                this.matcher.NodeQuota = value;
            }
        }

        public string XPath
        {
            get
            {
                return this.xpath;
            }
        }
    }
}

