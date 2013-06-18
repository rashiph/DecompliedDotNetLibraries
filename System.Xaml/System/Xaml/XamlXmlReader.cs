namespace System.Xaml
{
    using MS.Internal.Xaml;
    using MS.Internal.Xaml.Context;
    using MS.Internal.Xaml.Parser;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xaml.Schema;
    using System.Xml;

    public class XamlXmlReader : XamlReader, IXamlLineInfo
    {
        private XamlParserContext _context;
        private XamlNode _current;
        private System.Xaml.LineInfo _currentLineInfo;
        private XamlNode _endOfStreamNode;
        private XamlXmlReaderSettings _mergedSettings;
        private IEnumerator<XamlNode> _nodeStream;

        public XamlXmlReader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this.Initialize(this.CreateXmlReader(stream, null), null, null);
        }

        public XamlXmlReader(TextReader textReader)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }
            this.Initialize(this.CreateXmlReader(textReader, null), null, null);
        }

        public XamlXmlReader(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            this.Initialize(this.CreateXmlReader(fileName, null), null, null);
        }

        public XamlXmlReader(XmlReader xmlReader)
        {
            if (xmlReader == null)
            {
                throw new ArgumentNullException("xmlReader");
            }
            this.Initialize(xmlReader, null, null);
        }

        public XamlXmlReader(Stream stream, XamlSchemaContext schemaContext)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this.Initialize(this.CreateXmlReader(stream, null), schemaContext, null);
        }

        public XamlXmlReader(Stream stream, XamlXmlReaderSettings settings)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this.Initialize(this.CreateXmlReader(stream, settings), null, settings);
        }

        public XamlXmlReader(TextReader textReader, XamlSchemaContext schemaContext)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this.Initialize(this.CreateXmlReader(textReader, null), schemaContext, null);
        }

        public XamlXmlReader(TextReader textReader, XamlXmlReaderSettings settings)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }
            this.Initialize(this.CreateXmlReader(textReader, settings), null, settings);
        }

        public XamlXmlReader(string fileName, XamlSchemaContext schemaContext)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this.Initialize(this.CreateXmlReader(fileName, null), schemaContext, null);
        }

        public XamlXmlReader(string fileName, XamlXmlReaderSettings settings)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            this.Initialize(this.CreateXmlReader(fileName, settings), null, settings);
        }

        public XamlXmlReader(XmlReader xmlReader, XamlSchemaContext schemaContext)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            if (xmlReader == null)
            {
                throw new ArgumentNullException("xmlReader");
            }
            this.Initialize(xmlReader, schemaContext, null);
        }

        public XamlXmlReader(XmlReader xmlReader, XamlXmlReaderSettings settings)
        {
            if (xmlReader == null)
            {
                throw new ArgumentNullException("xmlReader");
            }
            this.Initialize(xmlReader, null, settings);
        }

        public XamlXmlReader(Stream stream, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this.Initialize(this.CreateXmlReader(stream, settings), schemaContext, settings);
        }

        public XamlXmlReader(TextReader textReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this.Initialize(this.CreateXmlReader(textReader, settings), schemaContext, settings);
        }

        public XamlXmlReader(string fileName, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this.Initialize(this.CreateXmlReader(fileName, settings), schemaContext, settings);
        }

        public XamlXmlReader(XmlReader xmlReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            if (xmlReader == null)
            {
                throw new ArgumentNullException("xmlReader");
            }
            this.Initialize(xmlReader, schemaContext, settings);
        }

        private XmlReader CreateXmlReader(Stream stream, XamlXmlReaderSettings settings)
        {
            bool flag = (settings != null) && settings.CloseInput;
            XmlReaderSettings settings2 = new XmlReaderSettings {
                CloseInput = flag,
                DtdProcessing = DtdProcessing.Prohibit
            };
            return XmlReader.Create(stream, settings2);
        }

        private XmlReader CreateXmlReader(TextReader textReader, XamlXmlReaderSettings settings)
        {
            bool flag = (settings != null) && settings.CloseInput;
            XmlReaderSettings settings2 = new XmlReaderSettings {
                CloseInput = flag,
                DtdProcessing = DtdProcessing.Prohibit
            };
            return XmlReader.Create(textReader, settings2);
        }

        private XmlReader CreateXmlReader(string fileName, XamlXmlReaderSettings settings)
        {
            bool flag = (settings == null) || settings.CloseInput;
            XmlReaderSettings settings2 = new XmlReaderSettings {
                CloseInput = flag,
                DtdProcessing = DtdProcessing.Prohibit
            };
            return XmlReader.Create(fileName, settings2);
        }

        private void Initialize(XmlReader givenXmlReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
        {
            XmlReader reader;
            this._mergedSettings = (settings == null) ? new XamlXmlReaderSettings() : new XamlXmlReaderSettings(settings);
            if (!this._mergedSettings.SkipXmlCompatibilityProcessing)
            {
                XmlCompatibilityReader reader2 = new XmlCompatibilityReader(givenXmlReader, new IsXmlNamespaceSupportedCallback(this.IsXmlNamespaceSupported)) {
                    Normalization = true
                };
                reader = reader2;
            }
            else
            {
                reader = givenXmlReader;
            }
            if (!string.IsNullOrEmpty(reader.BaseURI))
            {
                this._mergedSettings.BaseUri = new Uri(reader.BaseURI);
            }
            if (reader.XmlSpace == XmlSpace.Preserve)
            {
                this._mergedSettings.XmlSpacePreserve = true;
            }
            if (!string.IsNullOrEmpty(reader.XmlLang))
            {
                this._mergedSettings.XmlLang = reader.XmlLang;
            }
            IXmlNamespaceResolver resolver = reader as IXmlNamespaceResolver;
            Dictionary<string, string> xmlnsDictionary = null;
            if (resolver != null)
            {
                IDictionary<string, string> namespacesInScope = resolver.GetNamespacesInScope(XmlNamespaceScope.Local);
                if (namespacesInScope != null)
                {
                    foreach (KeyValuePair<string, string> pair in namespacesInScope)
                    {
                        if (xmlnsDictionary == null)
                        {
                            xmlnsDictionary = new Dictionary<string, string>();
                        }
                        xmlnsDictionary[pair.Key] = pair.Value;
                    }
                }
            }
            if (schemaContext == null)
            {
                schemaContext = new XamlSchemaContext();
            }
            this._endOfStreamNode = new XamlNode(XamlNode.InternalNodeType.EndOfStream);
            this._context = new XamlParserContext(schemaContext, this._mergedSettings.LocalAssembly);
            this._context.AllowProtectedMembersOnRoot = this._mergedSettings.AllowProtectedMembersOnRoot;
            this._context.AddNamespacePrefix("xml", "http://www.w3.org/XML/1998/namespace");
            Func<string, string> func = new Func<string, string>(reader.LookupNamespace);
            this._context.XmlNamespaceResolver = func;
            XamlScanner scanner = new XamlScanner(this._context, reader, this._mergedSettings);
            XamlPullParser parser = new XamlPullParser(this._context, scanner, this._mergedSettings);
            this._nodeStream = new NodeStreamSorter(this._context, parser, this._mergedSettings, xmlnsDictionary);
            this._current = new XamlNode(XamlNode.InternalNodeType.StartOfStream);
            this._currentLineInfo = new System.Xaml.LineInfo(0, 0);
        }

        internal bool IsXmlNamespaceSupported(string xmlNamespace, out string newXmlNamespace)
        {
            string str;
            string fullName;
            if (((this._mergedSettings.LocalAssembly != null) && ClrNamespaceUriParser.TryParseUri(xmlNamespace, out str, out fullName)) && string.IsNullOrEmpty(fullName))
            {
                fullName = this._mergedSettings.LocalAssembly.FullName;
                newXmlNamespace = ClrNamespaceUriParser.GetUri(str, fullName);
                return true;
            }
            bool flag = this._context.SchemaContext.TryGetCompatibleXamlNamespace(xmlNamespace, out newXmlNamespace);
            if (newXmlNamespace == null)
            {
                newXmlNamespace = string.Empty;
            }
            return flag;
        }

        public override bool Read()
        {
            this.ThrowIfDisposed();
        Label_0006:
            if (this._nodeStream.MoveNext())
            {
                this._current = this._nodeStream.Current;
                if (this._current.NodeType != XamlNodeType.None)
                {
                    goto Label_006E;
                }
                if (this._current.LineInfo != null)
                {
                    this._currentLineInfo = this._current.LineInfo;
                    goto Label_006E;
                }
                if (!this._current.IsEof)
                {
                    goto Label_006E;
                }
            }
            else
            {
                this._current = this._endOfStreamNode;
            }
            goto Label_007B;
        Label_006E:
            if (this._current.NodeType == XamlNodeType.None)
            {
                goto Label_0006;
            }
        Label_007B:
            return !this.IsEof;
        }

        private void ThrowIfDisposed()
        {
            if (base.IsDisposed)
            {
                throw new ObjectDisposedException("XamlXmlReader");
            }
        }

        public bool HasLineInfo
        {
            get
            {
                return this._mergedSettings.ProvideLineInfo;
            }
        }

        public override bool IsEof
        {
            get
            {
                return this._current.IsEof;
            }
        }

        public int LineNumber
        {
            get
            {
                return this._currentLineInfo.LineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                return this._currentLineInfo.LinePosition;
            }
        }

        public override XamlMember Member
        {
            get
            {
                return this._current.Member;
            }
        }

        public override System.Xaml.NamespaceDeclaration Namespace
        {
            get
            {
                return this._current.NamespaceDeclaration;
            }
        }

        public override XamlNodeType NodeType
        {
            get
            {
                return this._current.NodeType;
            }
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return this._context.SchemaContext;
            }
        }

        public override XamlType Type
        {
            get
            {
                return this._current.XamlType;
            }
        }

        public override object Value
        {
            get
            {
                return this._current.Value;
            }
        }
    }
}

