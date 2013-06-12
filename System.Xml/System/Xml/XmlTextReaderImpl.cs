namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Xml.Schema;

    internal class XmlTextReaderImpl : XmlReader, IXmlLineInfo, IXmlNamespaceResolver
    {
        private bool addDefaultAttributesAndNormalize;
        private bool afterResetState;
        private const int ApproxXmlDeclLength = 80;
        private int attrCount;
        private NodeData[] attrDuplSortingArray;
        private int attrDuplWalkCount;
        private int attrHashtable;
        private int attributeValueBaseEntityId;
        private bool attrNeedNamespaceLookup;
        private Base64Decoder base64Decoder;
        private BinHexDecoder binHexDecoder;
        private long charactersFromEntities;
        private long charactersInDocument;
        private bool checkCharacters;
        private bool closeInput;
        private CompressedStack compressedStack;
        private int curAttrIndex;
        private NodeData curNode;
        private Dictionary<IDtdEntityInfo, IDtdEntityInfo> currentEntities;
        private bool disableUndeclaredEntityCheck;
        private int documentStartBytePos;
        private const int DtdChidrenInitialSize = 2;
        private IDtdInfo dtdInfo;
        private System.Xml.DtdProcessing dtdProcessing;
        private bool emptyEntityInAttributeResolved;
        private System.Xml.EntityHandling entityHandling;
        private bool fragment;
        private XmlParserContext fragmentParserContext;
        private XmlNodeType fragmentType;
        private bool fullAttrCleanup;
        private bool ignoreComments;
        private bool ignorePIs;
        private IncrementalReadDecoder incReadDecoder;
        private int incReadDepth;
        private int incReadLeftEndPos;
        private int incReadLeftStartPos;
        private LineInfo incReadLineInfo;
        private IncrementalReadState incReadState;
        private int index;
        private const int InitialAttributesCount = 4;
        private const int InitialParsingStatesDepth = 2;
        private const int InitialParsingStateStackSize = 2;
        private IDtdEntityInfo lastEntity;
        private string lastPrefix;
        private int lineNumberOffset;
        private int linePositionOffset;
        private const int MaxAttrDuplWalkCount = 250;
        private const int MaxByteSequenceLen = 6;
        private const int MaxBytesToMove = 0x80;
        private long maxCharactersFromEntities;
        private long maxCharactersInDocument;
        private const int MinWhitespaceLookahedCount = 0x1000;
        private XmlNamespaceManager namespaceManager;
        private XmlNameTable nameTable;
        private bool nameTableFromSettings;
        private int nextEntityId;
        private ParsingFunction nextNextParsingFunction;
        private ParsingFunction nextParsingFunction;
        private NodeData[] nodes;
        private const int NodesInitialSize = 8;
        private bool normalize;
        private OnDefaultAttributeUseDelegate onDefaultAttributeUse;
        private XmlReader outerReader;
        private ParsingFunction parsingFunction;
        private ParsingMode parsingMode;
        private ParsingState[] parsingStatesStack;
        private int parsingStatesStackTop;
        private ParsingState ps;
        private IncrementalReadCharsDecoder readCharsDecoder;
        private System.Xml.ReadState readState;
        private int readValueOffset;
        private string reportedBaseUri;
        private System.Text.Encoding reportedEncoding;
        private bool rootElementParsed;
        private bool standalone;
        private StringBuilder stringBuilder;
        private bool supportNamespaces;
        private string url;
        private bool v1Compat;
        private bool validatingReaderCompatFlag;
        private IValidationEventHandling validationEventHandling;
        private System.Xml.WhitespaceHandling whitespaceHandling;
        private string Xml;
        private XmlCharType xmlCharType;
        private XmlContext xmlContext;
        private const string XmlDeclarationBegining = "<?xml";
        private string XmlNs;
        private System.Xml.XmlResolver xmlResolver;

        internal XmlTextReaderImpl()
        {
            this.xmlCharType = XmlCharType.Instance;
            this.curAttrIndex = -1;
            this.url = string.Empty;
            this.supportNamespaces = true;
            this.dtdProcessing = System.Xml.DtdProcessing.Parse;
            this.lastPrefix = string.Empty;
            this.parsingStatesStackTop = -1;
            this.fragmentType = XmlNodeType.Document;
            this.nextEntityId = 1;
            this.curNode = new NodeData();
            this.parsingFunction = ParsingFunction.NoData;
        }

        internal XmlTextReaderImpl(Stream input) : this(string.Empty, input, new System.Xml.NameTable())
        {
        }

        internal XmlTextReaderImpl(TextReader input) : this(string.Empty, input, new System.Xml.NameTable())
        {
        }

        public XmlTextReaderImpl(string url) : this(url, new System.Xml.NameTable())
        {
        }

        internal XmlTextReaderImpl(XmlNameTable nt)
        {
            this.xmlCharType = XmlCharType.Instance;
            this.curAttrIndex = -1;
            this.url = string.Empty;
            this.supportNamespaces = true;
            this.dtdProcessing = System.Xml.DtdProcessing.Parse;
            this.lastPrefix = string.Empty;
            this.parsingStatesStackTop = -1;
            this.fragmentType = XmlNodeType.Document;
            this.nextEntityId = 1;
            this.v1Compat = true;
            this.outerReader = this;
            this.nameTable = nt;
            nt.Add(string.Empty);
            this.xmlResolver = new XmlUrlResolver();
            this.Xml = nt.Add("xml");
            this.XmlNs = nt.Add("xmlns");
            this.nodes = new NodeData[8];
            this.nodes[0] = new NodeData();
            this.curNode = this.nodes[0];
            this.stringBuilder = new StringBuilder();
            this.xmlContext = new XmlContext();
            this.parsingFunction = ParsingFunction.SwitchToInteractiveXmlDecl;
            this.nextParsingFunction = ParsingFunction.DocumentContent;
            this.entityHandling = System.Xml.EntityHandling.ExpandCharEntities;
            this.whitespaceHandling = System.Xml.WhitespaceHandling.All;
            this.closeInput = true;
            this.maxCharactersInDocument = 0L;
            this.maxCharactersFromEntities = 0L;
            this.charactersInDocument = 0L;
            this.charactersFromEntities = 0L;
            this.ps.lineNo = 1;
            this.ps.lineStartPos = -1;
        }

        internal XmlTextReaderImpl(Stream input, XmlNameTable nt) : this(string.Empty, input, nt)
        {
        }

        internal XmlTextReaderImpl(TextReader input, XmlNameTable nt) : this(string.Empty, input, nt)
        {
        }

        internal XmlTextReaderImpl(string url, Stream input) : this(url, input, new System.Xml.NameTable())
        {
        }

        internal XmlTextReaderImpl(string url, TextReader input) : this(url, input, new System.Xml.NameTable())
        {
        }

        public XmlTextReaderImpl(string url, XmlNameTable nt) : this(nt)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            if (url.Length == 0)
            {
                throw new ArgumentException(Res.GetString("Xml_EmptyUrl"), "url");
            }
            this.namespaceManager = new XmlNamespaceManager(nt);
            this.compressedStack = CompressedStack.Capture();
            this.url = url;
            this.ps.baseUri = this.xmlResolver.ResolveUri(null, url);
            this.ps.baseUriStr = this.ps.baseUri.ToString();
            this.reportedBaseUri = this.ps.baseUriStr;
            this.parsingFunction = ParsingFunction.OpenUrl;
        }

        internal XmlTextReaderImpl(string xmlFragment, XmlParserContext context) : this(((context == null) || (context.NameTable == null)) ? new System.Xml.NameTable() : context.NameTable)
        {
            this.InitStringInput((context == null) ? string.Empty : context.BaseURI, System.Text.Encoding.Unicode, "<?xml " + xmlFragment + "?>");
            this.InitFragmentReader(XmlNodeType.XmlDeclaration, context, true);
        }

        internal XmlTextReaderImpl(Stream xmlFragment, XmlNodeType fragType, XmlParserContext context) : this(((context != null) && (context.NameTable != null)) ? context.NameTable : new System.Xml.NameTable())
        {
            System.Text.Encoding encoding = (context != null) ? context.Encoding : null;
            if (((context == null) || (context.BaseURI == null)) || (context.BaseURI.Length == 0))
            {
                this.InitStreamInput(xmlFragment, encoding);
            }
            else
            {
                this.InitStreamInput(this.xmlResolver.ResolveUri(null, context.BaseURI), xmlFragment, encoding);
            }
            this.InitFragmentReader(fragType, context, false);
            this.reportedBaseUri = this.ps.baseUriStr;
            this.reportedEncoding = this.ps.encoding;
        }

        internal XmlTextReaderImpl(string url, Stream input, XmlNameTable nt) : this(nt)
        {
            this.namespaceManager = new XmlNamespaceManager(nt);
            if ((url == null) || (url.Length == 0))
            {
                this.InitStreamInput(input, null);
            }
            else
            {
                this.InitStreamInput(url, input, null);
            }
            this.reportedBaseUri = this.ps.baseUriStr;
            this.reportedEncoding = this.ps.encoding;
        }

        internal XmlTextReaderImpl(string url, TextReader input, XmlNameTable nt) : this(nt)
        {
            this.namespaceManager = new XmlNamespaceManager(nt);
            this.reportedBaseUri = (url != null) ? url : string.Empty;
            this.InitTextReaderInput(this.reportedBaseUri, input);
            this.reportedEncoding = this.ps.encoding;
        }

        internal XmlTextReaderImpl(string xmlFragment, XmlNodeType fragType, XmlParserContext context) : this(((context == null) || (context.NameTable == null)) ? new System.Xml.NameTable() : context.NameTable)
        {
            if (context == null)
            {
                this.InitStringInput(string.Empty, System.Text.Encoding.Unicode, xmlFragment);
            }
            else
            {
                this.reportedBaseUri = context.BaseURI;
                this.InitStringInput(context.BaseURI, System.Text.Encoding.Unicode, xmlFragment);
            }
            this.InitFragmentReader(fragType, context, false);
            this.reportedEncoding = this.ps.encoding;
        }

        internal XmlTextReaderImpl(string xmlFragment, XmlParserContext context, XmlReaderSettings settings) : this(null, settings, context)
        {
            this.InitStringInput(string.Empty, System.Text.Encoding.Unicode, xmlFragment);
            this.reportedBaseUri = this.ps.baseUriStr;
            this.reportedEncoding = this.ps.encoding;
        }

        private XmlTextReaderImpl(System.Xml.XmlResolver resolver, XmlReaderSettings settings, XmlParserContext context)
        {
            this.xmlCharType = XmlCharType.Instance;
            this.curAttrIndex = -1;
            this.url = string.Empty;
            this.supportNamespaces = true;
            this.dtdProcessing = System.Xml.DtdProcessing.Parse;
            this.lastPrefix = string.Empty;
            this.parsingStatesStackTop = -1;
            this.fragmentType = XmlNodeType.Document;
            this.nextEntityId = 1;
            this.v1Compat = false;
            this.outerReader = this;
            this.xmlContext = new XmlContext();
            XmlNameTable nameTable = settings.NameTable;
            if (context == null)
            {
                if (nameTable == null)
                {
                    nameTable = new System.Xml.NameTable();
                }
                else
                {
                    this.nameTableFromSettings = true;
                }
                this.nameTable = nameTable;
                this.namespaceManager = new XmlNamespaceManager(nameTable);
            }
            else
            {
                this.SetupFromParserContext(context, settings);
                nameTable = this.nameTable;
            }
            nameTable.Add(string.Empty);
            this.Xml = nameTable.Add("xml");
            this.XmlNs = nameTable.Add("xmlns");
            this.xmlResolver = resolver;
            this.nodes = new NodeData[8];
            this.nodes[0] = new NodeData();
            this.curNode = this.nodes[0];
            this.stringBuilder = new StringBuilder();
            this.entityHandling = System.Xml.EntityHandling.ExpandEntities;
            this.whitespaceHandling = settings.IgnoreWhitespace ? System.Xml.WhitespaceHandling.Significant : System.Xml.WhitespaceHandling.All;
            this.normalize = true;
            this.ignorePIs = settings.IgnoreProcessingInstructions;
            this.ignoreComments = settings.IgnoreComments;
            this.checkCharacters = settings.CheckCharacters;
            this.lineNumberOffset = settings.LineNumberOffset;
            this.linePositionOffset = settings.LinePositionOffset;
            this.ps.lineNo = this.lineNumberOffset + 1;
            this.ps.lineStartPos = -this.linePositionOffset - 1;
            this.curNode.SetLineInfo(this.ps.LineNo - 1, this.ps.LinePos - 1);
            this.dtdProcessing = settings.DtdProcessing;
            this.maxCharactersInDocument = settings.MaxCharactersInDocument;
            this.maxCharactersFromEntities = settings.MaxCharactersFromEntities;
            this.charactersInDocument = 0L;
            this.charactersFromEntities = 0L;
            this.fragmentParserContext = context;
            this.parsingFunction = ParsingFunction.SwitchToInteractiveXmlDecl;
            this.nextParsingFunction = ParsingFunction.DocumentContent;
            switch (settings.ConformanceLevel)
            {
                case ConformanceLevel.Auto:
                    this.fragmentType = XmlNodeType.None;
                    this.fragment = true;
                    return;

                case ConformanceLevel.Fragment:
                    this.fragmentType = XmlNodeType.Element;
                    this.fragment = true;
                    return;
            }
            this.fragmentType = XmlNodeType.Document;
        }

        internal XmlTextReaderImpl(TextReader input, XmlReaderSettings settings, string baseUriStr, XmlParserContext context) : this(settings.GetXmlResolver(), settings, context)
        {
            if ((context != null) && (context.BaseURI != null))
            {
                baseUriStr = context.BaseURI;
            }
            this.InitTextReaderInput(baseUriStr, input);
            this.closeInput = settings.CloseInput;
            this.reportedBaseUri = this.ps.baseUriStr;
            this.reportedEncoding = this.ps.encoding;
            if ((context != null) && context.HasDtdInfo)
            {
                this.ProcessDtdFromParserContext(context);
            }
        }

        internal XmlTextReaderImpl(Stream stream, byte[] bytes, int byteCount, XmlReaderSettings settings, Uri baseUri, string baseUriStr, XmlParserContext context, bool closeInput) : this(settings.GetXmlResolver(), settings, context)
        {
            System.Text.Encoding encoding = null;
            if (context != null)
            {
                if (((context.BaseURI != null) && (context.BaseURI.Length > 0)) && !this.UriEqual(baseUri, baseUriStr, context.BaseURI, settings.GetXmlResolver()))
                {
                    if (baseUriStr.Length > 0)
                    {
                        this.Throw("Xml_DoubleBaseUri");
                    }
                    baseUriStr = context.BaseURI;
                }
                encoding = context.Encoding;
            }
            this.InitStreamInput(baseUri, baseUriStr, stream, bytes, byteCount, encoding);
            this.closeInput = closeInput;
            this.reportedBaseUri = this.ps.baseUriStr;
            this.reportedEncoding = this.ps.encoding;
            if ((context != null) && context.HasDtdInfo)
            {
                this.ProcessDtdFromParserContext(context);
            }
        }

        private NodeData AddAttribute(int endNamePos, int colonPos)
        {
            if ((colonPos == -1) || !this.supportNamespaces)
            {
                string localName = this.nameTable.Add(this.ps.chars, this.ps.charPos, endNamePos - this.ps.charPos);
                return this.AddAttribute(localName, string.Empty, localName);
            }
            this.attrNeedNamespaceLookup = true;
            int charPos = this.ps.charPos;
            int num2 = colonPos - charPos;
            if ((num2 == this.lastPrefix.Length) && XmlConvert.StrEqual(this.ps.chars, charPos, num2, this.lastPrefix))
            {
                return this.AddAttribute(this.nameTable.Add(this.ps.chars, colonPos + 1, (endNamePos - colonPos) - 1), this.lastPrefix, null);
            }
            string prefix = this.nameTable.Add(this.ps.chars, charPos, num2);
            this.lastPrefix = prefix;
            return this.AddAttribute(this.nameTable.Add(this.ps.chars, colonPos + 1, (endNamePos - colonPos) - 1), prefix, null);
        }

        private NodeData AddAttribute(string localName, string prefix, string nameWPrefix)
        {
            NodeData data = this.AddNode((this.index + this.attrCount) + 1, this.index + 1);
            data.SetNamedNode(XmlNodeType.Attribute, localName, prefix, nameWPrefix);
            int num = ((int) 1) << localName[0];
            if ((this.attrHashtable & num) == 0)
            {
                this.attrHashtable |= num;
            }
            else if (this.attrDuplWalkCount < 250)
            {
                this.attrDuplWalkCount++;
                for (int i = this.index + 1; i < ((this.index + this.attrCount) + 1); i++)
                {
                    NodeData data2 = this.nodes[i];
                    if (Ref.Equal(data2.localName, data.localName))
                    {
                        this.attrDuplWalkCount = 250;
                        break;
                    }
                }
            }
            this.attrCount++;
            return data;
        }

        private void AddAttributeChunkToList(NodeData attr, NodeData chunk, ref NodeData lastChunk)
        {
            if (lastChunk == null)
            {
                lastChunk = chunk;
                attr.nextAttrValueChunk = chunk;
            }
            else
            {
                lastChunk.nextAttrValueChunk = chunk;
                lastChunk = chunk;
            }
        }

        private NodeData AddAttributeNoChecks(string name, int attrDepth)
        {
            NodeData data = this.AddNode((this.index + this.attrCount) + 1, attrDepth);
            data.SetNamedNode(XmlNodeType.Attribute, this.nameTable.Add(name));
            this.attrCount++;
            return data;
        }

        private bool AddDefaultAttributeDtd(IDtdDefaultAttributeInfo defAttrInfo, bool definedInDtd, NodeData[] nameSortedNodeData)
        {
            if (defAttrInfo.Prefix.Length > 0)
            {
                this.attrNeedNamespaceLookup = true;
            }
            string localName = defAttrInfo.LocalName;
            string prefix = defAttrInfo.Prefix;
            if (nameSortedNodeData != null)
            {
                if (Array.BinarySearch<object>(nameSortedNodeData, defAttrInfo, DtdDefaultAttributeInfoToNodeDataComparer.Instance) >= 0)
                {
                    return false;
                }
            }
            else
            {
                for (int i = this.index + 1; i < ((this.index + 1) + this.attrCount); i++)
                {
                    if ((this.nodes[i].localName == localName) && (this.nodes[i].prefix == prefix))
                    {
                        return false;
                    }
                }
            }
            NodeData data = this.AddDefaultAttributeInternal(defAttrInfo.LocalName, null, defAttrInfo.Prefix, defAttrInfo.DefaultValueExpanded, defAttrInfo.LineNumber, defAttrInfo.LinePosition, defAttrInfo.ValueLineNumber, defAttrInfo.ValueLinePosition, defAttrInfo.IsXmlAttribute);
            if (this.DtdValidation)
            {
                if (this.onDefaultAttributeUse != null)
                {
                    this.onDefaultAttributeUse(defAttrInfo, this);
                }
                data.typedValue = defAttrInfo.DefaultValueTyped;
            }
            return (data != null);
        }

        private NodeData AddDefaultAttributeInternal(string localName, string ns, string prefix, string value, int lineNo, int linePos, int valueLineNo, int valueLinePos, bool isXmlAttribute)
        {
            NodeData attr = this.AddAttribute(localName, prefix, (prefix.Length > 0) ? null : localName);
            if (ns != null)
            {
                attr.ns = ns;
            }
            attr.SetValue(value);
            attr.IsDefaultAttribute = true;
            attr.lineInfo.Set(lineNo, linePos);
            attr.lineInfo2.Set(valueLineNo, valueLinePos);
            if (attr.prefix.Length == 0)
            {
                if (Ref.Equal(attr.localName, this.XmlNs))
                {
                    this.OnDefaultNamespaceDecl(attr);
                    if (!this.attrNeedNamespaceLookup && (this.nodes[this.index].prefix.Length == 0))
                    {
                        this.nodes[this.index].ns = this.xmlContext.defaultNamespace;
                    }
                }
            }
            else if (Ref.Equal(attr.prefix, this.XmlNs))
            {
                this.OnNamespaceDecl(attr);
                if (!this.attrNeedNamespaceLookup)
                {
                    string str = attr.localName;
                    for (int i = this.index; i < ((this.index + this.attrCount) + 1); i++)
                    {
                        if (this.nodes[i].prefix.Equals(str))
                        {
                            this.nodes[i].ns = this.namespaceManager.LookupNamespace(str);
                        }
                    }
                }
            }
            else if (isXmlAttribute)
            {
                this.OnXmlReservedAttribute(attr);
            }
            this.fullAttrCleanup = true;
            return attr;
        }

        internal bool AddDefaultAttributeNonDtd(SchemaAttDef attrDef)
        {
            string localName = this.nameTable.Add(attrDef.Name.Name);
            string prefix = this.nameTable.Add(attrDef.Prefix);
            string uri = this.nameTable.Add(attrDef.Name.Namespace);
            if ((prefix.Length == 0) && (uri.Length > 0))
            {
                prefix = this.namespaceManager.LookupPrefix(uri);
                if (prefix == null)
                {
                    prefix = string.Empty;
                }
            }
            for (int i = this.index + 1; i < ((this.index + 1) + this.attrCount); i++)
            {
                if ((this.nodes[i].localName == localName) && ((this.nodes[i].prefix == prefix) || ((this.nodes[i].ns == uri) && (uri != null))))
                {
                    return false;
                }
            }
            NodeData data = this.AddDefaultAttributeInternal(localName, uri, prefix, attrDef.DefaultValueExpanded, attrDef.LineNumber, attrDef.LinePosition, attrDef.ValueLineNumber, attrDef.ValueLinePosition, attrDef.Reserved != SchemaAttDef.Reserve.None);
            data.schemaType = (attrDef.SchemaType == null) ? ((object) attrDef.Datatype) : ((object) attrDef.SchemaType);
            data.typedValue = attrDef.DefaultValueTyped;
            return true;
        }

        private void AddDefaultAttributesAndNormalize()
        {
            IDtdAttributeListInfo attributeList = this.dtdInfo.LookupAttributeList(this.curNode.localName, this.curNode.prefix);
            if (attributeList != null)
            {
                if (this.normalize && attributeList.HasNonCDataAttributes)
                {
                    for (int i = this.index + 1; i < ((this.index + 1) + this.attrCount); i++)
                    {
                        NodeData data = this.nodes[i];
                        IDtdAttributeInfo attribute = attributeList.LookupAttribute(data.prefix, data.localName);
                        if ((attribute != null) && attribute.IsNonCDataType)
                        {
                            if ((this.DtdValidation && this.standalone) && attribute.IsDeclaredInExternal)
                            {
                                string stringValue = data.StringValue;
                                data.TrimSpacesInValue();
                                if (stringValue != data.StringValue)
                                {
                                    this.SendValidationEvent(XmlSeverityType.Error, "Sch_StandAloneNormalization", data.GetNameWPrefix(this.nameTable), data.LineNo, data.LinePos);
                                }
                            }
                            else
                            {
                                data.TrimSpacesInValue();
                            }
                        }
                    }
                }
                IEnumerable<IDtdDefaultAttributeInfo> defaultAttributes = attributeList.LookupDefaultAttributes();
                if (defaultAttributes != null)
                {
                    int attrCount = this.attrCount;
                    NodeData[] destinationArray = null;
                    if (this.attrCount >= 250)
                    {
                        destinationArray = new NodeData[this.attrCount];
                        Array.Copy(this.nodes, this.index + 1, destinationArray, 0, this.attrCount);
                        Array.Sort<object>(destinationArray, DtdDefaultAttributeInfoToNodeDataComparer.Instance);
                    }
                    foreach (IDtdDefaultAttributeInfo info3 in defaultAttributes)
                    {
                        if ((this.AddDefaultAttributeDtd(info3, true, destinationArray) && this.DtdValidation) && (this.standalone && info3.IsDeclaredInExternal))
                        {
                            string prefix = info3.Prefix;
                            string arg = (prefix.Length == 0) ? info3.LocalName : (prefix + ':' + info3.LocalName);
                            this.SendValidationEvent(XmlSeverityType.Error, "Sch_UnSpecifiedDefaultAttributeInExternalStandalone", arg, this.curNode.LineNo, this.curNode.LinePos);
                        }
                    }
                    if ((attrCount == 0) && this.attrNeedNamespaceLookup)
                    {
                        this.AttributeNamespaceLookup();
                        this.attrNeedNamespaceLookup = false;
                    }
                }
            }
        }

        private void AddNamespace(string prefix, string uri, NodeData attr)
        {
            if (uri == "http://www.w3.org/2000/xmlns/")
            {
                if (Ref.Equal(prefix, this.XmlNs))
                {
                    this.Throw("Xml_XmlnsPrefix", attr.lineInfo2.lineNo, attr.lineInfo2.linePos);
                }
                else
                {
                    this.Throw("Xml_NamespaceDeclXmlXmlns", prefix, attr.lineInfo2.lineNo, attr.lineInfo2.linePos);
                }
            }
            else if (((uri == "http://www.w3.org/XML/1998/namespace") && !Ref.Equal(prefix, this.Xml)) && !this.v1Compat)
            {
                this.Throw("Xml_NamespaceDeclXmlXmlns", prefix, attr.lineInfo2.lineNo, attr.lineInfo2.linePos);
            }
            if ((uri.Length == 0) && (prefix.Length > 0))
            {
                this.Throw("Xml_BadNamespaceDecl", attr.lineInfo.lineNo, attr.lineInfo.linePos);
            }
            try
            {
                this.namespaceManager.AddNamespace(prefix, uri);
            }
            catch (ArgumentException exception)
            {
                this.ReThrow(exception, attr.lineInfo.lineNo, attr.lineInfo.linePos);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private NodeData AddNode(int nodeIndex, int nodeDepth)
        {
            NodeData data = this.nodes[nodeIndex];
            if (data != null)
            {
                data.depth = nodeDepth;
                return data;
            }
            return this.AllocNode(nodeIndex, nodeDepth);
        }

        internal static unsafe void AdjustLineInfo(char* pChars, int length, bool isNormalized, ref LineInfo lineInfo)
        {
            int num = -1;
            for (int i = 0; i < length; i++)
            {
                switch (pChars[i])
                {
                    case '\n':
                        lineInfo.lineNo++;
                        num = i;
                        break;

                    case '\r':
                        if (!isNormalized)
                        {
                            lineInfo.lineNo++;
                            num = i;
                            if (((i + 1) < length) && (pChars[i + 1] == '\n'))
                            {
                                i++;
                                num++;
                            }
                        }
                        break;
                }
            }
            if (num >= 0)
            {
                lineInfo.linePos = length - num;
            }
        }

        internal static unsafe void AdjustLineInfo(char[] chars, int startPos, int endPos, bool isNormalized, ref LineInfo lineInfo)
        {
            fixed (char* chRef = &(chars[startPos]))
            {
                AdjustLineInfo(chRef, endPos - startPos, isNormalized, ref lineInfo);
            }
        }

        internal static unsafe void AdjustLineInfo(string str, int startPos, int endPos, bool isNormalized, ref LineInfo lineInfo)
        {
            fixed (char* str2 = ((char*) str))
            {
                char* chPtr = str2;
                AdjustLineInfo(chPtr + startPos, endPos - startPos, isNormalized, ref lineInfo);
            }
        }

        private NodeData AllocNode(int nodeIndex, int nodeDepth)
        {
            if (nodeIndex >= (this.nodes.Length - 1))
            {
                NodeData[] destinationArray = new NodeData[this.nodes.Length * 2];
                Array.Copy(this.nodes, 0, destinationArray, 0, this.nodes.Length);
                this.nodes = destinationArray;
            }
            NodeData data = this.nodes[nodeIndex];
            if (data == null)
            {
                data = new NodeData();
                this.nodes[nodeIndex] = data;
            }
            data.depth = nodeDepth;
            return data;
        }

        private void AttributeDuplCheck()
        {
            if (this.attrCount < 250)
            {
                for (int i = this.index + 1; i < ((this.index + 1) + this.attrCount); i++)
                {
                    NodeData data = this.nodes[i];
                    for (int j = i + 1; j < ((this.index + 1) + this.attrCount); j++)
                    {
                        if (Ref.Equal(data.localName, this.nodes[j].localName) && Ref.Equal(data.ns, this.nodes[j].ns))
                        {
                            this.Throw("Xml_DupAttributeName", this.nodes[j].GetNameWPrefix(this.nameTable), this.nodes[j].LineNo, this.nodes[j].LinePos);
                        }
                    }
                }
            }
            else
            {
                if ((this.attrDuplSortingArray == null) || (this.attrDuplSortingArray.Length < this.attrCount))
                {
                    this.attrDuplSortingArray = new NodeData[this.attrCount];
                }
                Array.Copy(this.nodes, this.index + 1, this.attrDuplSortingArray, 0, this.attrCount);
                Array.Sort<NodeData>(this.attrDuplSortingArray, 0, this.attrCount);
                NodeData data2 = this.attrDuplSortingArray[0];
                for (int k = 1; k < this.attrCount; k++)
                {
                    NodeData data3 = this.attrDuplSortingArray[k];
                    if (Ref.Equal(data2.localName, data3.localName) && Ref.Equal(data2.ns, data3.ns))
                    {
                        this.Throw("Xml_DupAttributeName", data3.GetNameWPrefix(this.nameTable), data3.LineNo, data3.LinePos);
                    }
                    data2 = data3;
                }
            }
        }

        private void AttributeNamespaceLookup()
        {
            for (int i = this.index + 1; i < ((this.index + this.attrCount) + 1); i++)
            {
                NodeData node = this.nodes[i];
                if ((node.type == XmlNodeType.Attribute) && (node.prefix.Length > 0))
                {
                    node.ns = this.LookupNamespace(node);
                }
            }
        }

        internal static void BlockCopy(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            Buffer.BlockCopy(src, srcOffset, dst, dstOffset, count);
        }

        internal static void BlockCopyChars(char[] src, int srcOffset, char[] dst, int dstOffset, int count)
        {
            Buffer.BlockCopy(src, srcOffset * 2, dst, dstOffset * 2, count * 2);
        }

        internal void ChangeCurrentNodeType(XmlNodeType newNodeType)
        {
            this.curNode.type = newNodeType;
        }

        private System.Text.Encoding CheckEncoding(string newEncodingName)
        {
            if (this.ps.stream == null)
            {
                return this.ps.encoding;
            }
            if (((string.Compare(newEncodingName, "ucs-2", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(newEncodingName, "utf-16", StringComparison.OrdinalIgnoreCase) == 0)) || ((string.Compare(newEncodingName, "iso-10646-ucs-2", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(newEncodingName, "ucs-4", StringComparison.OrdinalIgnoreCase) == 0)))
            {
                if (((this.ps.encoding.WebName != "utf-16BE") && (this.ps.encoding.WebName != "utf-16")) && (string.Compare(newEncodingName, "ucs-4", StringComparison.OrdinalIgnoreCase) != 0))
                {
                    if (this.afterResetState)
                    {
                        this.Throw("Xml_EncodingSwitchAfterResetState", newEncodingName);
                    }
                    else
                    {
                        this.ThrowWithoutLineInfo("Xml_MissingByteOrderMark");
                    }
                }
                return this.ps.encoding;
            }
            System.Text.Encoding encoding = null;
            if (string.Compare(newEncodingName, "utf-8", StringComparison.OrdinalIgnoreCase) == 0)
            {
                encoding = new UTF8Encoding(true, true);
            }
            else
            {
                try
                {
                    encoding = System.Text.Encoding.GetEncoding(newEncodingName);
                }
                catch (NotSupportedException exception)
                {
                    this.Throw("Xml_UnknownEncoding", newEncodingName, exception);
                }
                catch (ArgumentException exception2)
                {
                    this.Throw("Xml_UnknownEncoding", newEncodingName, exception2);
                }
            }
            if (this.afterResetState && (this.ps.encoding.WebName != encoding.WebName))
            {
                this.Throw("Xml_EncodingSwitchAfterResetState", newEncodingName);
            }
            return encoding;
        }

        public override void Close()
        {
            this.Close(this.closeInput);
        }

        internal void Close(bool closeInput)
        {
            if (this.parsingFunction != ParsingFunction.ReaderClosed)
            {
                while (this.InEntity)
                {
                    this.PopParsingState();
                }
                this.ps.Close(closeInput);
                this.curNode = NodeData.None;
                this.parsingFunction = ParsingFunction.ReaderClosed;
                this.reportedEncoding = null;
                this.reportedBaseUri = string.Empty;
                this.readState = System.Xml.ReadState.Closed;
                this.fullAttrCleanup = false;
                this.ResetAttributes();
            }
        }

        private System.Text.Encoding DetectEncoding()
        {
            if (this.ps.bytesUsed >= 2)
            {
                int num = (this.ps.bytes[0] << 8) | this.ps.bytes[1];
                int num2 = (this.ps.bytesUsed >= 4) ? ((this.ps.bytes[2] << 8) | this.ps.bytes[3]) : 0;
                switch (num)
                {
                    case 0:
                        switch (num2)
                        {
                            case 0xfeff:
                                return Ucs4Encoding.UCS4_Bigendian;

                            case 0xfffe:
                                return Ucs4Encoding.UCS4_2143;

                            case 60:
                                return Ucs4Encoding.UCS4_Bigendian;

                            case 0x3c00:
                                return Ucs4Encoding.UCS4_2143;
                        }
                        break;

                    case 60:
                        if (num2 == 0)
                        {
                            return Ucs4Encoding.UCS4_3412;
                        }
                        return System.Text.Encoding.BigEndianUnicode;

                    case 0x3c00:
                        if (num2 == 0)
                        {
                            return Ucs4Encoding.UCS4_Littleendian;
                        }
                        return System.Text.Encoding.Unicode;

                    case 0xfeff:
                        if (num2 == 0)
                        {
                            return Ucs4Encoding.UCS4_3412;
                        }
                        return System.Text.Encoding.BigEndianUnicode;

                    case 0xfffe:
                        if (num2 == 0)
                        {
                            return Ucs4Encoding.UCS4_Littleendian;
                        }
                        return System.Text.Encoding.Unicode;

                    case 0x4c6f:
                        if (num2 == 0xa794)
                        {
                            this.Throw("Xml_UnknownEncoding", "ebcdic");
                        }
                        break;

                    case 0xefbb:
                        if ((num2 & 0xff00) == 0xbf00)
                        {
                            return new UTF8Encoding(true, true);
                        }
                        break;
                }
            }
            return null;
        }

        internal void DtdParserProxy_OnNewLine(int pos)
        {
            this.OnNewLine(pos);
        }

        internal void DtdParserProxy_OnPublicId(string publicId, LineInfo keywordLineInfo, LineInfo publicLiteralLineInfo)
        {
            NodeData data = this.AddAttributeNoChecks("PUBLIC", this.index + 1);
            data.SetValue(publicId);
            data.lineInfo = keywordLineInfo;
            data.lineInfo2 = publicLiteralLineInfo;
        }

        internal void DtdParserProxy_OnSystemId(string systemId, LineInfo keywordLineInfo, LineInfo systemLiteralLineInfo)
        {
            NodeData data = this.AddAttributeNoChecks("SYSTEM", this.index + 1);
            data.SetValue(systemId);
            data.lineInfo = keywordLineInfo;
            data.lineInfo2 = systemLiteralLineInfo;
        }

        internal void DtdParserProxy_ParseComment(StringBuilder sb)
        {
            try
            {
                if (sb == null)
                {
                    ParsingMode parsingMode = this.parsingMode;
                    this.parsingMode = ParsingMode.SkipNode;
                    this.ParseCDataOrComment(XmlNodeType.Comment);
                    this.parsingMode = parsingMode;
                }
                else
                {
                    NodeData curNode = this.curNode;
                    this.curNode = this.AddNode((this.index + this.attrCount) + 1, this.index);
                    this.ParseCDataOrComment(XmlNodeType.Comment);
                    this.curNode.CopyTo(0, sb);
                    this.curNode = curNode;
                }
            }
            catch (XmlException exception)
            {
                if ((exception.ResString != "Xml_UnexpectedEOF") || (this.ps.entity == null))
                {
                    throw;
                }
                this.SendValidationEvent(XmlSeverityType.Error, "Sch_ParEntityRefNesting", null, this.ps.LineNo, this.ps.LinePos);
            }
        }

        internal int DtdParserProxy_ParseNamedCharRef(bool expand, StringBuilder internalSubsetBuilder)
        {
            return this.ParseNamedCharRef(expand, internalSubsetBuilder);
        }

        internal int DtdParserProxy_ParseNumericCharRef(StringBuilder internalSubsetBuilder)
        {
            EntityType type;
            return this.ParseNumericCharRef(true, internalSubsetBuilder, out type);
        }

        internal void DtdParserProxy_ParsePI(StringBuilder sb)
        {
            if (sb == null)
            {
                ParsingMode parsingMode = this.parsingMode;
                this.parsingMode = ParsingMode.SkipNode;
                this.ParsePI(null);
                this.parsingMode = parsingMode;
            }
            else
            {
                this.ParsePI(sb);
            }
        }

        internal bool DtdParserProxy_PopEntity(out IDtdEntityInfo oldEntity, out int newEntityId)
        {
            if (this.parsingStatesStackTop == -1)
            {
                oldEntity = null;
                newEntityId = -1;
                return false;
            }
            oldEntity = this.ps.entity;
            this.PopEntity();
            newEntityId = this.ps.entityId;
            return true;
        }

        internal bool DtdParserProxy_PushEntity(IDtdEntityInfo entity, out int entityId)
        {
            bool flag;
            if (entity.IsExternal)
            {
                if (this.xmlResolver == null)
                {
                    entityId = -1;
                    return false;
                }
                flag = this.PushExternalEntity(entity);
            }
            else
            {
                this.PushInternalEntity(entity);
                flag = true;
            }
            entityId = this.ps.entityId;
            return flag;
        }

        internal bool DtdParserProxy_PushExternalSubset(string systemId, string publicId)
        {
            if (this.xmlResolver == null)
            {
                return false;
            }
            this.PushExternalEntityOrSubset(publicId, systemId, this.ps.baseUriStr, ref this.ps.baseUri, null);
            this.ps.entity = null;
            this.ps.entityId = 0;
            int charPos = this.ps.charPos;
            if (this.v1Compat)
            {
                this.EatWhitespaces(null);
            }
            if (!this.ParseXmlDeclaration(true))
            {
                this.ps.charPos = charPos;
            }
            return true;
        }

        internal void DtdParserProxy_PushInternalDtd(string baseUri, string internalDtd)
        {
            this.PushParsingState();
            this.RegisterConsumedCharacters((long) internalDtd.Length, false);
            this.InitStringInput(baseUri, System.Text.Encoding.Unicode, internalDtd);
            this.ps.entity = null;
            this.ps.entityId = 0;
            this.ps.eolNormalized = false;
        }

        internal int DtdParserProxy_ReadData()
        {
            return this.ReadData();
        }

        internal void DtdParserProxy_Throw(Exception e)
        {
            this.Throw(e);
        }

        private int EatWhitespaces(StringBuilder sb)
        {
            int num5;
            int charPos = this.ps.charPos;
            int num2 = 0;
            char[] chars = this.ps.chars;
        Label_001A:
            switch (chars[charPos])
            {
                case '\t':
                case ' ':
                    charPos++;
                    goto Label_001A;

                case '\n':
                    charPos++;
                    this.OnNewLine(charPos);
                    goto Label_001A;

                case '\r':
                {
                    if (chars[charPos + 1] != '\n')
                    {
                        if (((charPos + 1) >= this.ps.charsUsed) && !this.ps.isEof)
                        {
                            goto Label_0151;
                        }
                        if (!this.ps.eolNormalized)
                        {
                            chars[charPos] = '\n';
                        }
                        charPos++;
                        break;
                    }
                    int charCount = charPos - this.ps.charPos;
                    if ((sb != null) && !this.ps.eolNormalized)
                    {
                        if (charCount > 0)
                        {
                            sb.Append(chars, this.ps.charPos, charCount);
                            num2 += charCount;
                        }
                        this.ps.charPos = charPos + 1;
                    }
                    charPos += 2;
                    break;
                }
                default:
                    if (charPos != this.ps.charsUsed)
                    {
                        int num4 = charPos - this.ps.charPos;
                        if (num4 <= 0)
                        {
                            return num2;
                        }
                        if (sb != null)
                        {
                            sb.Append(this.ps.chars, this.ps.charPos, num4);
                        }
                        this.ps.charPos = charPos;
                        return (num2 + num4);
                    }
                    goto Label_0151;
            }
            this.OnNewLine(charPos);
            goto Label_001A;
        Label_0151:
            num5 = charPos - this.ps.charPos;
            if (num5 > 0)
            {
                if (sb != null)
                {
                    sb.Append(this.ps.chars, this.ps.charPos, num5);
                }
                this.ps.charPos = charPos;
                num2 += num5;
            }
            if (this.ReadData() == 0)
            {
                if ((this.ps.charsUsed - this.ps.charPos) == 0)
                {
                    return num2;
                }
                if (this.ps.chars[this.ps.charPos] != '\r')
                {
                    this.Throw("Xml_UnexpectedEOF1");
                }
            }
            charPos = this.ps.charPos;
            chars = this.ps.chars;
            goto Label_001A;
        }

        private void ElementNamespaceLookup()
        {
            if (this.curNode.prefix.Length == 0)
            {
                this.curNode.ns = this.xmlContext.defaultNamespace;
            }
            else
            {
                this.curNode.ns = this.LookupNamespace(this.curNode);
            }
        }

        private void FinishAttributeValueIterator()
        {
            if (this.parsingFunction == ParsingFunction.InReadValueChunk)
            {
                this.FinishReadValueChunk();
            }
            else if (this.parsingFunction == ParsingFunction.InReadContentAsBinary)
            {
                this.FinishReadContentAsBinary();
            }
            if (this.parsingFunction == ParsingFunction.InReadAttributeValue)
            {
                while (this.ps.entityId != this.attributeValueBaseEntityId)
                {
                    this.HandleEntityEnd(false);
                }
                this.emptyEntityInAttributeResolved = false;
                this.parsingFunction = this.nextParsingFunction;
                this.nextParsingFunction = (this.index > 0) ? ParsingFunction.ElementContent : ParsingFunction.DocumentContent;
            }
        }

        private void FinishIncrementalRead()
        {
            this.incReadDecoder = new IncrementalReadDummyDecoder();
            this.IncrementalRead();
            this.incReadDecoder = null;
        }

        private void FinishOtherValueIterator()
        {
            switch (this.parsingFunction)
            {
                case ParsingFunction.InReadAttributeValue:
                    break;

                case ParsingFunction.InReadValueChunk:
                    if (this.incReadState != IncrementalReadState.ReadValueChunk_OnPartialValue)
                    {
                        if (this.readValueOffset <= 0)
                        {
                            break;
                        }
                        this.curNode.SetValue(this.curNode.StringValue.Substring(this.readValueOffset));
                        this.readValueOffset = 0;
                        return;
                    }
                    this.FinishPartialValue();
                    this.incReadState = IncrementalReadState.ReadValueChunk_OnCachedValue;
                    return;

                case ParsingFunction.InReadContentAsBinary:
                case ParsingFunction.InReadElementContentAsBinary:
                    switch (this.incReadState)
                    {
                        case IncrementalReadState.ReadContentAsBinary_OnCachedValue:
                            if (this.readValueOffset <= 0)
                            {
                                break;
                            }
                            this.curNode.SetValue(this.curNode.StringValue.Substring(this.readValueOffset));
                            this.readValueOffset = 0;
                            return;

                        case IncrementalReadState.ReadContentAsBinary_OnPartialValue:
                            this.FinishPartialValue();
                            this.incReadState = IncrementalReadState.ReadContentAsBinary_OnCachedValue;
                            return;

                        case IncrementalReadState.ReadContentAsBinary_End:
                            this.curNode.SetValue(string.Empty);
                            return;
                    }
                    return;

                default:
                    return;
            }
        }

        private void FinishPartialValue()
        {
            int num;
            int num2;
            this.curNode.CopyTo(this.readValueOffset, this.stringBuilder);
            int outOrChars = 0;
            while (!this.ParseText(out num, out num2, ref outOrChars))
            {
                this.stringBuilder.Append(this.ps.chars, num, num2 - num);
            }
            this.stringBuilder.Append(this.ps.chars, num, num2 - num);
            this.curNode.SetValue(this.stringBuilder.ToString());
            this.stringBuilder.Length = 0;
        }

        private void FinishReadContentAsBinary()
        {
            this.readValueOffset = 0;
            if (this.incReadState == IncrementalReadState.ReadContentAsBinary_OnPartialValue)
            {
                this.SkipPartialTextValue();
            }
            else
            {
                this.parsingFunction = this.nextParsingFunction;
                this.nextParsingFunction = this.nextNextParsingFunction;
            }
            if (this.incReadState != IncrementalReadState.ReadContentAsBinary_End)
            {
                while (this.MoveToNextContentNode(true))
                {
                }
            }
        }

        private void FinishReadElementContentAsBinary()
        {
            this.FinishReadContentAsBinary();
            if (this.curNode.type != XmlNodeType.EndElement)
            {
                this.Throw("Xml_InvalidNodeType", this.curNode.type.ToString());
            }
            this.outerReader.Read();
        }

        private void FinishReadValueChunk()
        {
            this.readValueOffset = 0;
            if (this.incReadState == IncrementalReadState.ReadValueChunk_OnPartialValue)
            {
                this.SkipPartialTextValue();
            }
            else
            {
                this.parsingFunction = this.nextParsingFunction;
                this.nextParsingFunction = this.nextNextParsingFunction;
            }
        }

        private void FullAttributeCleanup()
        {
            for (int i = this.index + 1; i < ((this.index + this.attrCount) + 1); i++)
            {
                NodeData data = this.nodes[i];
                data.nextAttrValueChunk = null;
                data.IsDefaultAttribute = false;
            }
            this.fullAttrCleanup = false;
        }

        public override string GetAttribute(int i)
        {
            if ((i < 0) || (i >= this.attrCount))
            {
                throw new ArgumentOutOfRangeException("i");
            }
            return this.nodes[(this.index + i) + 1].StringValue;
        }

        public override string GetAttribute(string name)
        {
            int indexOfAttributeWithoutPrefix;
            if (name.IndexOf(':') == -1)
            {
                indexOfAttributeWithoutPrefix = this.GetIndexOfAttributeWithoutPrefix(name);
            }
            else
            {
                indexOfAttributeWithoutPrefix = this.GetIndexOfAttributeWithPrefix(name);
            }
            if (indexOfAttributeWithoutPrefix < 0)
            {
                return null;
            }
            return this.nodes[indexOfAttributeWithoutPrefix].StringValue;
        }

        public override string GetAttribute(string localName, string namespaceURI)
        {
            namespaceURI = (namespaceURI == null) ? string.Empty : this.nameTable.Get(namespaceURI);
            localName = this.nameTable.Get(localName);
            for (int i = this.index + 1; i < ((this.index + this.attrCount) + 1); i++)
            {
                if (Ref.Equal(this.nodes[i].localName, localName) && Ref.Equal(this.nodes[i].ns, namespaceURI))
                {
                    return this.nodes[i].StringValue;
                }
            }
            return null;
        }

        private int GetChars(int maxCharsCount)
        {
            int num2;
            int byteCount = this.ps.bytesUsed - this.ps.bytePos;
            if (byteCount == 0)
            {
                return 0;
            }
            try
            {
                bool flag;
                this.ps.decoder.Convert(this.ps.bytes, this.ps.bytePos, byteCount, this.ps.chars, this.ps.charsUsed, maxCharsCount, false, out byteCount, out num2, out flag);
            }
            catch (ArgumentException)
            {
                this.InvalidCharRecovery(ref byteCount, out num2);
            }
            this.ps.bytePos += byteCount;
            this.ps.charsUsed += num2;
            return num2;
        }

        private int GetIndexOfAttributeWithoutPrefix(string name)
        {
            name = this.nameTable.Get(name);
            if (name != null)
            {
                for (int i = this.index + 1; i < ((this.index + this.attrCount) + 1); i++)
                {
                    if (Ref.Equal(this.nodes[i].localName, name) && (this.nodes[i].prefix.Length == 0))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private int GetIndexOfAttributeWithPrefix(string name)
        {
            name = this.nameTable.Add(name);
            if (name != null)
            {
                for (int i = this.index + 1; i < ((this.index + this.attrCount) + 1); i++)
                {
                    if (Ref.Equal(this.nodes[i].GetNameWPrefix(this.nameTable), name))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        internal IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
        {
            return this.namespaceManager.GetNamespacesInScope(scope);
        }

        internal TextReader GetRemainder()
        {
            switch (this.parsingFunction)
            {
                case ParsingFunction.Eof:
                case ParsingFunction.ReaderClosed:
                    return new StringReader(string.Empty);

                case ParsingFunction.InIncrementalRead:
                    if (!this.InEntity)
                    {
                        this.stringBuilder.Append(this.ps.chars, this.incReadLeftStartPos, this.incReadLeftEndPos - this.incReadLeftStartPos);
                    }
                    break;

                case ParsingFunction.OpenUrl:
                    this.OpenUrl();
                    break;
            }
            while (this.InEntity)
            {
                this.HandleEntityEnd(true);
            }
            this.ps.appendMode = false;
            do
            {
                this.stringBuilder.Append(this.ps.chars, this.ps.charPos, this.ps.charsUsed - this.ps.charPos);
                this.ps.charPos = this.ps.charsUsed;
            }
            while (this.ReadData() != 0);
            this.OnEof();
            string s = this.stringBuilder.ToString();
            this.stringBuilder.Length = 0;
            return new StringReader(s);
        }

        internal System.Xml.XmlResolver GetResolver()
        {
            return this.xmlResolver;
        }

        private XmlNodeType GetTextNodeType(int orChars)
        {
            if (orChars > 0x20)
            {
                return XmlNodeType.Text;
            }
            return this.GetWhitespaceType();
        }

        private XmlNodeType GetWhitespaceType()
        {
            if (this.whitespaceHandling != System.Xml.WhitespaceHandling.None)
            {
                if (this.xmlContext.xmlSpace == System.Xml.XmlSpace.Preserve)
                {
                    return XmlNodeType.SignificantWhitespace;
                }
                if (this.whitespaceHandling == System.Xml.WhitespaceHandling.All)
                {
                    return XmlNodeType.Whitespace;
                }
            }
            return XmlNodeType.None;
        }

        private bool HandleEntityEnd(bool checkEntityNesting)
        {
            if (this.parsingStatesStackTop == -1)
            {
                this.Throw("Xml_InternalError");
            }
            if (this.ps.entityResolvedManually)
            {
                this.index--;
                if (checkEntityNesting && (this.ps.entityId != this.nodes[this.index].entityId))
                {
                    this.Throw("Xml_IncompleteEntity");
                }
                this.lastEntity = this.ps.entity;
                this.PopEntity();
                return true;
            }
            if (checkEntityNesting && (this.ps.entityId != this.nodes[this.index].entityId))
            {
                this.Throw("Xml_IncompleteEntity");
            }
            this.PopEntity();
            this.reportedEncoding = this.ps.encoding;
            this.reportedBaseUri = this.ps.baseUriStr;
            return false;
        }

        private EntityType HandleEntityReference(bool isInAttributeValue, EntityExpandType expandType, out int charRefEndPos)
        {
            int num;
            if (((this.ps.charPos + 1) == this.ps.charsUsed) && (this.ReadData() == 0))
            {
                this.Throw("Xml_UnexpectedEOF1");
            }
            if (this.ps.chars[this.ps.charPos + 1] == '#')
            {
                EntityType type;
                charRefEndPos = this.ParseNumericCharRef(expandType != EntityExpandType.OnlyGeneral, null, out type);
                return type;
            }
            charRefEndPos = this.ParseNamedCharRef(expandType != EntityExpandType.OnlyGeneral, null);
            if (charRefEndPos >= 0)
            {
                return EntityType.CharacterNamed;
            }
            if ((expandType == EntityExpandType.OnlyCharacter) || ((this.entityHandling != System.Xml.EntityHandling.ExpandEntities) && (!isInAttributeValue || !this.validatingReaderCompatFlag)))
            {
                return EntityType.Unexpanded;
            }
            this.ps.charPos++;
            int linePos = this.ps.LinePos;
            try
            {
                num = this.ParseName();
            }
            catch (XmlException)
            {
                this.Throw("Xml_ErrorParsingEntityName", this.ps.LineNo, linePos);
                return EntityType.Skipped;
            }
            if (this.ps.chars[num] != ';')
            {
                this.ThrowUnexpectedToken(num, ";");
            }
            int entityStartLinePos = this.ps.LinePos;
            string name = this.nameTable.Add(this.ps.chars, this.ps.charPos, num - this.ps.charPos);
            this.ps.charPos = num + 1;
            charRefEndPos = -1;
            EntityType type2 = this.HandleGeneralEntityReference(name, isInAttributeValue, false, entityStartLinePos);
            this.reportedBaseUri = this.ps.baseUriStr;
            this.reportedEncoding = this.ps.encoding;
            return type2;
        }

        private EntityType HandleGeneralEntityReference(string name, bool isInAttributeValue, bool pushFakeEntityIfNullResolver, int entityStartLinePos)
        {
            IDtdEntityInfo info = null;
            if (((this.dtdInfo == null) && (this.fragmentParserContext != null)) && (this.fragmentParserContext.HasDtdInfo && (this.dtdProcessing == System.Xml.DtdProcessing.Parse)))
            {
                this.ParseDtdFromParserContext();
            }
            if ((this.dtdInfo == null) || ((info = this.dtdInfo.LookupEntity(name)) == null))
            {
                if (this.disableUndeclaredEntityCheck)
                {
                    SchemaEntity entity = new SchemaEntity(new XmlQualifiedName(name), false) {
                        Text = string.Empty
                    };
                    info = entity;
                }
                else
                {
                    this.Throw("Xml_UndeclaredEntity", name, this.ps.LineNo, entityStartLinePos);
                }
            }
            if (info.IsUnparsedEntity)
            {
                if (this.disableUndeclaredEntityCheck)
                {
                    SchemaEntity entity2 = new SchemaEntity(new XmlQualifiedName(name), false) {
                        Text = string.Empty
                    };
                    info = entity2;
                }
                else
                {
                    this.Throw("Xml_UnparsedEntityRef", name, this.ps.LineNo, entityStartLinePos);
                }
            }
            if (this.standalone && info.IsDeclaredInExternal)
            {
                this.Throw("Xml_ExternalEntityInStandAloneDocument", info.Name, this.ps.LineNo, entityStartLinePos);
            }
            if (info.IsExternal)
            {
                if (isInAttributeValue)
                {
                    this.Throw("Xml_ExternalEntityInAttValue", name, this.ps.LineNo, entityStartLinePos);
                    return EntityType.Skipped;
                }
                if (this.parsingMode == ParsingMode.SkipContent)
                {
                    return EntityType.Skipped;
                }
                if (this.xmlResolver == null)
                {
                    if (pushFakeEntityIfNullResolver)
                    {
                        this.PushExternalEntity(info);
                        this.curNode.entityId = this.ps.entityId;
                        return EntityType.FakeExpanded;
                    }
                    return EntityType.Skipped;
                }
                this.PushExternalEntity(info);
                this.curNode.entityId = this.ps.entityId;
                if (isInAttributeValue && this.validatingReaderCompatFlag)
                {
                    return EntityType.ExpandedInAttribute;
                }
                return EntityType.Expanded;
            }
            if (this.parsingMode == ParsingMode.SkipContent)
            {
                return EntityType.Skipped;
            }
            this.PushInternalEntity(info);
            this.curNode.entityId = this.ps.entityId;
            if (isInAttributeValue && this.validatingReaderCompatFlag)
            {
                return EntityType.ExpandedInAttribute;
            }
            return EntityType.Expanded;
        }

        public bool HasLineInfo()
        {
            return true;
        }

        private unsafe int IncrementalRead()
        {
            int num2;
            char ch;
            int num10;
            int num11;
            int num = 0;
        Label_0002:
            num2 = this.incReadLeftEndPos - this.incReadLeftStartPos;
            if (num2 > 0)
            {
                int num3;
                try
                {
                    num3 = this.incReadDecoder.Decode(this.ps.chars, this.incReadLeftStartPos, num2);
                }
                catch (XmlException exception)
                {
                    this.ReThrow(exception, this.incReadLineInfo.lineNo, this.incReadLineInfo.linePos);
                    return 0;
                }
                if (num3 < num2)
                {
                    this.incReadLeftStartPos += num3;
                    this.incReadLineInfo.linePos += num3;
                    return num3;
                }
                this.incReadLeftStartPos = 0;
                this.incReadLeftEndPos = 0;
                this.incReadLineInfo.linePos += num3;
                if (this.incReadDecoder.IsFull)
                {
                    return num3;
                }
            }
            int outStartPos = 0;
            int outEndPos = 0;
        Label_00BA:
            switch (this.incReadState)
            {
                case IncrementalReadState.PI:
                    if (this.ParsePIValue(out outStartPos, out outEndPos))
                    {
                        this.ps.charPos -= 2;
                        this.incReadState = IncrementalReadState.Text;
                    }
                    goto Label_06F5;

                case IncrementalReadState.CDATA:
                    if (this.ParseCDataOrComment(XmlNodeType.CDATA, out outStartPos, out outEndPos))
                    {
                        this.ps.charPos -= 3;
                        this.incReadState = IncrementalReadState.Text;
                    }
                    goto Label_06F5;

                case IncrementalReadState.Comment:
                    if (this.ParseCDataOrComment(XmlNodeType.Comment, out outStartPos, out outEndPos))
                    {
                        this.ps.charPos -= 3;
                        this.incReadState = IncrementalReadState.Text;
                    }
                    goto Label_06F5;

                case IncrementalReadState.ReadData:
                    if (this.ReadData() == 0)
                    {
                        this.ThrowUnclosedElements();
                    }
                    this.incReadState = IncrementalReadState.Text;
                    outEndPos = this.ps.charPos;
                    break;

                case IncrementalReadState.EndElement:
                    this.parsingFunction = ParsingFunction.PopElementContext;
                    this.nextParsingFunction = ((this.index > 0) || (this.fragmentType != XmlNodeType.Document)) ? ParsingFunction.ElementContent : ParsingFunction.DocumentContent;
                    this.outerReader.Read();
                    this.incReadState = IncrementalReadState.End;
                    return num;

                case IncrementalReadState.End:
                    return num;
            }
            char[] chars = this.ps.chars;
            outStartPos = this.ps.charPos;
            outEndPos = outStartPos;
        Label_0200:
            this.incReadLineInfo.Set(this.ps.LineNo, this.ps.LinePos);
            if (this.incReadState != IncrementalReadState.Attributes)
            {
                while ((this.xmlCharType.charProperties[ch = chars[outEndPos]] & 0x80) != 0)
                {
                    outEndPos++;
                }
            }
            else
            {
                while (((this.xmlCharType.charProperties[ch = chars[outEndPos]] & 0x80) != 0) && (ch != '/'))
                {
                    outEndPos++;
                }
            }
            if ((chars[outEndPos] == '&') || (chars[outEndPos] == '\t'))
            {
                outEndPos++;
                goto Label_0200;
            }
            if ((outEndPos - outStartPos) > 0)
            {
                goto Label_06E8;
            }
            switch (chars[outEndPos])
            {
                case '<':
                    if (this.incReadState == IncrementalReadState.Text)
                    {
                        if ((this.ps.charsUsed - outEndPos) < 2)
                        {
                            goto Label_06E1;
                        }
                        char ch3 = chars[outEndPos + 1];
                        if (ch3 != '!')
                        {
                            if (ch3 == '/')
                            {
                                int num6;
                                int index = this.ParseQName(true, 2, out num6);
                                if (XmlConvert.StrEqual(chars, this.ps.charPos + 2, (index - this.ps.charPos) - 2, this.curNode.GetNameWPrefix(this.nameTable)) && ((this.ps.chars[index] == '>') || this.xmlCharType.IsWhiteSpace(this.ps.chars[index])))
                                {
                                    if (--this.incReadDepth > 0)
                                    {
                                        outEndPos = index + 1;
                                        goto Label_0200;
                                    }
                                    this.ps.charPos = index;
                                    if (this.xmlCharType.IsWhiteSpace(this.ps.chars[index]))
                                    {
                                        this.EatWhitespaces(null);
                                    }
                                    if (this.ps.chars[this.ps.charPos] != '>')
                                    {
                                        this.ThrowUnexpectedToken(">");
                                    }
                                    this.ps.charPos++;
                                    this.incReadState = IncrementalReadState.EndElement;
                                    goto Label_0002;
                                }
                                outEndPos = index;
                                outStartPos = this.ps.charPos;
                                chars = this.ps.chars;
                                goto Label_0200;
                            }
                            if (ch3 != '?')
                            {
                                int num8;
                                int num9 = this.ParseQName(true, 1, out num8);
                                if (XmlConvert.StrEqual(this.ps.chars, this.ps.charPos + 1, (num9 - this.ps.charPos) - 1, this.curNode.localName) && (((this.ps.chars[num9] == '>') || (this.ps.chars[num9] == '/')) || this.xmlCharType.IsWhiteSpace(this.ps.chars[num9])))
                                {
                                    this.incReadDepth++;
                                    this.incReadState = IncrementalReadState.Attributes;
                                    outEndPos = num9;
                                    goto Label_06E8;
                                }
                                outEndPos = num9;
                                outStartPos = this.ps.charPos;
                                chars = this.ps.chars;
                                goto Label_0200;
                            }
                            outEndPos += 2;
                            this.incReadState = IncrementalReadState.PI;
                        }
                        else
                        {
                            if ((this.ps.charsUsed - outEndPos) < 4)
                            {
                                goto Label_06E1;
                            }
                            if ((chars[outEndPos + 2] == '-') && (chars[outEndPos + 3] == '-'))
                            {
                                outEndPos += 4;
                                this.incReadState = IncrementalReadState.Comment;
                            }
                            else
                            {
                                if ((this.ps.charsUsed - outEndPos) < 9)
                                {
                                    goto Label_06E1;
                                }
                                if (!XmlConvert.StrEqual(chars, outEndPos + 2, 7, "[CDATA["))
                                {
                                    goto Label_0200;
                                }
                                outEndPos += 9;
                                this.incReadState = IncrementalReadState.CDATA;
                            }
                        }
                        goto Label_06E8;
                    }
                    outEndPos++;
                    goto Label_0200;

                case '>':
                    if (this.incReadState == IncrementalReadState.Attributes)
                    {
                        this.incReadState = IncrementalReadState.Text;
                    }
                    outEndPos++;
                    goto Label_0200;

                case '/':
                    if (this.incReadState == IncrementalReadState.Attributes)
                    {
                        if ((this.ps.charsUsed - outEndPos) < 2)
                        {
                            goto Label_06E1;
                        }
                        if (chars[outEndPos + 1] == '>')
                        {
                            this.incReadState = IncrementalReadState.Text;
                            this.incReadDepth--;
                        }
                    }
                    outEndPos++;
                    goto Label_0200;

                case '\'':
                case '"':
                    switch (this.incReadState)
                    {
                        case IncrementalReadState.Attributes:
                            this.curNode.quoteChar = chars[outEndPos];
                            this.incReadState = IncrementalReadState.AttributeValue;
                            goto Label_06BC;

                        case IncrementalReadState.AttributeValue:
                            if (chars[outEndPos] == this.curNode.quoteChar)
                            {
                                this.incReadState = IncrementalReadState.Attributes;
                            }
                            goto Label_06BC;
                    }
                    break;

                case '\n':
                    outEndPos++;
                    this.OnNewLine(outEndPos);
                    goto Label_0200;

                case '\r':
                    if (chars[outEndPos + 1] == '\n')
                    {
                        outEndPos += 2;
                    }
                    else
                    {
                        if ((outEndPos + 1) >= this.ps.charsUsed)
                        {
                            goto Label_06E1;
                        }
                        outEndPos++;
                    }
                    this.OnNewLine(outEndPos);
                    goto Label_0200;

                default:
                    if (outEndPos != this.ps.charsUsed)
                    {
                        outEndPos++;
                        goto Label_0200;
                    }
                    goto Label_06E1;
            }
        Label_06BC:
            outEndPos++;
            goto Label_0200;
        Label_06E1:
            this.incReadState = IncrementalReadState.ReadData;
        Label_06E8:
            this.ps.charPos = outEndPos;
        Label_06F5:
            num10 = outEndPos - outStartPos;
            if (num10 <= 0)
            {
                goto Label_00BA;
            }
            try
            {
                num11 = this.incReadDecoder.Decode(this.ps.chars, outStartPos, num10);
            }
            catch (XmlException exception2)
            {
                this.ReThrow(exception2, this.incReadLineInfo.lineNo, this.incReadLineInfo.linePos);
                return 0;
            }
            num += num11;
            if (!this.incReadDecoder.IsFull)
            {
                goto Label_00BA;
            }
            this.incReadLeftStartPos = outStartPos + num11;
            this.incReadLeftEndPos = outEndPos;
            this.incReadLineInfo.linePos += num11;
            return num;
        }

        private int IncrementalRead(Array array, int index, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException((this.incReadDecoder is IncrementalReadCharsDecoder) ? "buffer" : "array");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException((this.incReadDecoder is IncrementalReadCharsDecoder) ? "count" : "len");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException((this.incReadDecoder is IncrementalReadCharsDecoder) ? "index" : "offset");
            }
            if ((array.Length - index) < count)
            {
                throw new ArgumentException((this.incReadDecoder is IncrementalReadCharsDecoder) ? "count" : "len");
            }
            if (count == 0)
            {
                return 0;
            }
            this.curNode.lineInfo = this.incReadLineInfo;
            this.incReadDecoder.SetNextOutputBuffer(array, index, count);
            this.IncrementalRead();
            return this.incReadDecoder.DecodedCount;
        }

        private void InitBase64Decoder()
        {
            if (this.base64Decoder == null)
            {
                this.base64Decoder = new Base64Decoder();
            }
            else
            {
                this.base64Decoder.Reset();
            }
            this.incReadDecoder = this.base64Decoder;
        }

        private void InitBinHexDecoder()
        {
            if (this.binHexDecoder == null)
            {
                this.binHexDecoder = new BinHexDecoder();
            }
            else
            {
                this.binHexDecoder.Reset();
            }
            this.incReadDecoder = this.binHexDecoder;
        }

        private void InitFragmentReader(XmlNodeType fragmentType, XmlParserContext parserContext, bool allowXmlDeclFragment)
        {
            this.fragmentParserContext = parserContext;
            if (parserContext != null)
            {
                if (parserContext.NamespaceManager != null)
                {
                    this.namespaceManager = parserContext.NamespaceManager;
                    this.xmlContext.defaultNamespace = this.namespaceManager.LookupNamespace(string.Empty);
                }
                else
                {
                    this.namespaceManager = new XmlNamespaceManager(this.nameTable);
                }
                this.ps.baseUriStr = parserContext.BaseURI;
                this.ps.baseUri = null;
                this.xmlContext.xmlLang = parserContext.XmlLang;
                this.xmlContext.xmlSpace = parserContext.XmlSpace;
            }
            else
            {
                this.namespaceManager = new XmlNamespaceManager(this.nameTable);
                this.ps.baseUriStr = string.Empty;
                this.ps.baseUri = null;
            }
            this.reportedBaseUri = this.ps.baseUriStr;
            switch (fragmentType)
            {
                case XmlNodeType.Element:
                    this.nextParsingFunction = ParsingFunction.DocumentContent;
                    goto Label_0149;

                case XmlNodeType.Attribute:
                    this.ps.appendMode = false;
                    this.parsingFunction = ParsingFunction.SwitchToInteractive;
                    this.nextParsingFunction = ParsingFunction.FragmentAttribute;
                    goto Label_0149;

                case XmlNodeType.Document:
                    goto Label_0149;

                case XmlNodeType.XmlDeclaration:
                    if (allowXmlDeclFragment)
                    {
                        this.ps.appendMode = false;
                        this.parsingFunction = ParsingFunction.SwitchToInteractive;
                        this.nextParsingFunction = ParsingFunction.XmlDeclarationFragment;
                        goto Label_0149;
                    }
                    break;
            }
            this.Throw("Xml_PartialContentNodeTypeNotSupportedEx", fragmentType.ToString());
            return;
        Label_0149:
            this.fragmentType = fragmentType;
            this.fragment = true;
        }

        private void InitIncrementalRead(IncrementalReadDecoder decoder)
        {
            this.ResetAttributes();
            decoder.Reset();
            this.incReadDecoder = decoder;
            this.incReadState = IncrementalReadState.Text;
            this.incReadDepth = 1;
            this.incReadLeftStartPos = this.ps.charPos;
            this.incReadLeftEndPos = this.ps.charPos;
            this.incReadLineInfo.Set(this.ps.LineNo, this.ps.LinePos);
            this.parsingFunction = ParsingFunction.InIncrementalRead;
        }

        private bool InitReadContentAsBinary()
        {
            if (this.parsingFunction == ParsingFunction.InReadValueChunk)
            {
                throw new InvalidOperationException(Res.GetString("Xml_MixingReadValueChunkWithBinary"));
            }
            if (this.parsingFunction == ParsingFunction.InIncrementalRead)
            {
                throw new InvalidOperationException(Res.GetString("Xml_MixingV1StreamingWithV2Binary"));
            }
            if (!XmlReader.IsTextualNode(this.curNode.type) && !this.MoveToNextContentNode(false))
            {
                return false;
            }
            this.SetupReadContentAsBinaryState(ParsingFunction.InReadContentAsBinary);
            this.incReadLineInfo.Set(this.curNode.LineNo, this.curNode.LinePos);
            return true;
        }

        private bool InitReadElementContentAsBinary()
        {
            bool isEmptyElement = this.curNode.IsEmptyElement;
            this.outerReader.Read();
            if (isEmptyElement)
            {
                return false;
            }
            if (!this.MoveToNextContentNode(false))
            {
                if (this.curNode.type != XmlNodeType.EndElement)
                {
                    this.Throw("Xml_InvalidNodeType", this.curNode.type.ToString());
                }
                this.outerReader.Read();
                return false;
            }
            this.SetupReadContentAsBinaryState(ParsingFunction.InReadElementContentAsBinary);
            this.incReadLineInfo.Set(this.curNode.LineNo, this.curNode.LinePos);
            return true;
        }

        private void InitStreamInput(Stream stream, System.Text.Encoding encoding)
        {
            this.InitStreamInput(null, string.Empty, stream, null, 0, encoding);
        }

        private void InitStreamInput(string baseUriStr, Stream stream, System.Text.Encoding encoding)
        {
            this.InitStreamInput(null, baseUriStr, stream, null, 0, encoding);
        }

        private void InitStreamInput(Uri baseUri, Stream stream, System.Text.Encoding encoding)
        {
            this.InitStreamInput(baseUri, baseUri.ToString(), stream, null, 0, encoding);
        }

        private void InitStreamInput(Uri baseUri, string baseUriStr, Stream stream, System.Text.Encoding encoding)
        {
            this.InitStreamInput(baseUri, baseUriStr, stream, null, 0, encoding);
        }

        private void InitStreamInput(Uri baseUri, string baseUriStr, Stream stream, byte[] bytes, int byteCount, System.Text.Encoding encoding)
        {
            int num;
            this.ps.stream = stream;
            this.ps.baseUri = baseUri;
            this.ps.baseUriStr = baseUriStr;
            if (bytes != null)
            {
                this.ps.bytes = bytes;
                this.ps.bytesUsed = byteCount;
                num = this.ps.bytes.Length;
            }
            else
            {
                num = XmlReader.CalcBufferSize(stream);
                if ((this.ps.bytes == null) || (this.ps.bytes.Length < num))
                {
                    this.ps.bytes = new byte[num];
                }
            }
            if ((this.ps.chars == null) || (this.ps.chars.Length < (num + 1)))
            {
                this.ps.chars = new char[num + 1];
            }
            this.ps.bytePos = 0;
            while ((this.ps.bytesUsed < 4) && ((this.ps.bytes.Length - this.ps.bytesUsed) > 0))
            {
                int num2 = stream.Read(this.ps.bytes, this.ps.bytesUsed, this.ps.bytes.Length - this.ps.bytesUsed);
                if (num2 == 0)
                {
                    this.ps.isStreamEof = true;
                    break;
                }
                this.ps.bytesUsed += num2;
            }
            if (encoding == null)
            {
                encoding = this.DetectEncoding();
            }
            this.SetupEncoding(encoding);
            byte[] preamble = this.ps.encoding.GetPreamble();
            int length = preamble.Length;
            int index = 0;
            while ((index < length) && (index < this.ps.bytesUsed))
            {
                if (this.ps.bytes[index] != preamble[index])
                {
                    break;
                }
                index++;
            }
            if (index == length)
            {
                this.ps.bytePos = length;
            }
            this.documentStartBytePos = this.ps.bytePos;
            this.ps.eolNormalized = !this.normalize;
            this.ps.appendMode = true;
            this.ReadData();
        }

        private void InitStringInput(string baseUriStr, System.Text.Encoding originalEncoding, string str)
        {
            this.ps.baseUriStr = baseUriStr;
            this.ps.baseUri = null;
            int length = str.Length;
            this.ps.chars = new char[length + 1];
            str.CopyTo(0, this.ps.chars, 0, str.Length);
            this.ps.charsUsed = length;
            this.ps.chars[length] = '\0';
            this.ps.encoding = originalEncoding;
            this.ps.eolNormalized = !this.normalize;
            this.ps.isEof = true;
        }

        private void InitTextReaderInput(string baseUriStr, TextReader input)
        {
            this.InitTextReaderInput(baseUriStr, null, input);
        }

        private void InitTextReaderInput(string baseUriStr, Uri baseUri, TextReader input)
        {
            this.ps.textReader = input;
            this.ps.baseUriStr = baseUriStr;
            this.ps.baseUri = baseUri;
            if (this.ps.chars == null)
            {
                this.ps.chars = new char[0x1001];
            }
            this.ps.encoding = System.Text.Encoding.Unicode;
            this.ps.eolNormalized = !this.normalize;
            this.ps.appendMode = true;
            this.ReadData();
        }

        private void InvalidCharRecovery(ref int bytesCount, out int charsCount)
        {
            int num = 0;
            int num2 = 0;
            try
            {
                while (num2 < bytesCount)
                {
                    int num3;
                    int num4;
                    bool flag;
                    this.ps.decoder.Convert(this.ps.bytes, this.ps.bytePos + num2, 1, this.ps.chars, this.ps.charsUsed + num, 1, false, out num4, out num3, out flag);
                    num += num3;
                    num2 += num4;
                }
            }
            catch (ArgumentException)
            {
            }
            if (num == 0)
            {
                this.Throw(this.ps.charsUsed, "Xml_InvalidCharInThisEncoding");
            }
            charsCount = num;
            bytesCount = num2;
        }

        public override string LookupNamespace(string prefix)
        {
            if (!this.supportNamespaces)
            {
                return null;
            }
            return this.namespaceManager.LookupNamespace(prefix);
        }

        private string LookupNamespace(NodeData node)
        {
            string str = this.namespaceManager.LookupNamespace(node.prefix);
            if (str != null)
            {
                return str;
            }
            this.Throw("Xml_UnknownNs", node.prefix, node.LineNo, node.LinePos);
            return null;
        }

        internal string LookupPrefix(string namespaceName)
        {
            return this.namespaceManager.LookupPrefix(namespaceName);
        }

        internal void MoveOffEntityReference()
        {
            if (((this.outerReader.NodeType == XmlNodeType.EntityReference) && (this.parsingFunction == ParsingFunction.AfterResolveEntityInContent)) && !this.outerReader.Read())
            {
                throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
            }
        }

        public override void MoveToAttribute(int i)
        {
            if ((i < 0) || (i >= this.attrCount))
            {
                throw new ArgumentOutOfRangeException("i");
            }
            if (this.InAttributeValueIterator)
            {
                this.FinishAttributeValueIterator();
            }
            this.curAttrIndex = i;
            this.curNode = this.nodes[(this.index + 1) + this.curAttrIndex];
        }

        public override bool MoveToAttribute(string name)
        {
            int indexOfAttributeWithoutPrefix;
            if (name.IndexOf(':') == -1)
            {
                indexOfAttributeWithoutPrefix = this.GetIndexOfAttributeWithoutPrefix(name);
            }
            else
            {
                indexOfAttributeWithoutPrefix = this.GetIndexOfAttributeWithPrefix(name);
            }
            if (indexOfAttributeWithoutPrefix < 0)
            {
                return false;
            }
            if (this.InAttributeValueIterator)
            {
                this.FinishAttributeValueIterator();
            }
            this.curAttrIndex = (indexOfAttributeWithoutPrefix - this.index) - 1;
            this.curNode = this.nodes[indexOfAttributeWithoutPrefix];
            return true;
        }

        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            namespaceURI = (namespaceURI == null) ? string.Empty : this.nameTable.Get(namespaceURI);
            localName = this.nameTable.Get(localName);
            for (int i = this.index + 1; i < ((this.index + this.attrCount) + 1); i++)
            {
                if (Ref.Equal(this.nodes[i].localName, localName) && Ref.Equal(this.nodes[i].ns, namespaceURI))
                {
                    this.curAttrIndex = (i - this.index) - 1;
                    this.curNode = this.nodes[i];
                    if (this.InAttributeValueIterator)
                    {
                        this.FinishAttributeValueIterator();
                    }
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToElement()
        {
            if (this.InAttributeValueIterator)
            {
                this.FinishAttributeValueIterator();
            }
            else if (this.curNode.type != XmlNodeType.Attribute)
            {
                return false;
            }
            this.curAttrIndex = -1;
            this.curNode = this.nodes[this.index];
            return true;
        }

        public override bool MoveToFirstAttribute()
        {
            if (this.attrCount == 0)
            {
                return false;
            }
            if (this.InAttributeValueIterator)
            {
                this.FinishAttributeValueIterator();
            }
            this.curAttrIndex = 0;
            this.curNode = this.nodes[this.index + 1];
            return true;
        }

        public override bool MoveToNextAttribute()
        {
            if ((this.curAttrIndex + 1) >= this.attrCount)
            {
                return false;
            }
            if (this.InAttributeValueIterator)
            {
                this.FinishAttributeValueIterator();
            }
            this.curNode = this.nodes[(this.index + 1) + ++this.curAttrIndex];
            return true;
        }

        private bool MoveToNextContentNode(bool moveIfOnContentNode)
        {
            do
            {
                switch (this.curNode.type)
                {
                    case XmlNodeType.Attribute:
                        return !moveIfOnContentNode;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        if (moveIfOnContentNode)
                        {
                            break;
                        }
                        return true;

                    case XmlNodeType.EntityReference:
                        this.outerReader.ResolveEntity();
                        break;

                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.EndEntity:
                        break;

                    default:
                        return false;
                }
                moveIfOnContentNode = false;
            }
            while (this.outerReader.Read());
            return false;
        }

        private void OnDefaultNamespaceDecl(NodeData attr)
        {
            if (this.supportNamespaces)
            {
                string uri = this.nameTable.Add(attr.StringValue);
                attr.ns = this.nameTable.Add("http://www.w3.org/2000/xmlns/");
                if (!this.curNode.xmlContextPushed)
                {
                    this.PushXmlContext();
                }
                this.xmlContext.defaultNamespace = uri;
                this.AddNamespace(string.Empty, uri, attr);
            }
        }

        private void OnEof()
        {
            this.curNode = this.nodes[0];
            this.curNode.Clear(XmlNodeType.None);
            this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
            this.parsingFunction = ParsingFunction.Eof;
            this.readState = System.Xml.ReadState.EndOfFile;
            this.reportedEncoding = null;
        }

        private void OnNamespaceDecl(NodeData attr)
        {
            if (this.supportNamespaces)
            {
                string uri = this.nameTable.Add(attr.StringValue);
                if (uri.Length == 0)
                {
                    this.Throw("Xml_BadNamespaceDecl", attr.lineInfo2.lineNo, attr.lineInfo2.linePos - 1);
                }
                this.AddNamespace(attr.localName, uri, attr);
            }
        }

        private void OnNewLine(int pos)
        {
            this.ps.lineNo++;
            this.ps.lineStartPos = pos - 1;
        }

        private void OnXmlReservedAttribute(NodeData attr)
        {
            string localName = attr.localName;
            if (localName != null)
            {
                if (!(localName == "space"))
                {
                    if (localName == "lang")
                    {
                        if (!this.curNode.xmlContextPushed)
                        {
                            this.PushXmlContext();
                        }
                        this.xmlContext.xmlLang = attr.StringValue;
                    }
                }
                else
                {
                    if (!this.curNode.xmlContextPushed)
                    {
                        this.PushXmlContext();
                    }
                    switch (XmlConvert.TrimString(attr.StringValue))
                    {
                        case "preserve":
                            this.xmlContext.xmlSpace = System.Xml.XmlSpace.Preserve;
                            return;

                        case "default":
                            this.xmlContext.xmlSpace = System.Xml.XmlSpace.Default;
                            return;
                    }
                    this.Throw("Xml_InvalidXmlSpace", attr.StringValue, attr.lineInfo.lineNo, attr.lineInfo.linePos);
                }
            }
        }

        private bool OpenAndPush(Uri uri)
        {
            if (this.xmlResolver.SupportsType(uri, typeof(TextReader)))
            {
                TextReader input = (TextReader) this.xmlResolver.GetEntity(uri, null, typeof(TextReader));
                if (input == null)
                {
                    return false;
                }
                this.PushParsingState();
                this.InitTextReaderInput(uri.ToString(), uri, input);
            }
            else
            {
                Stream stream = (Stream) this.xmlResolver.GetEntity(uri, null, typeof(Stream));
                if (stream == null)
                {
                    return false;
                }
                this.PushParsingState();
                this.InitStreamInput(uri, stream, null);
            }
            return true;
        }

        private void OpenUrl()
        {
            System.Xml.XmlResolver xmlResolver;
            if (this.ps.baseUri != null)
            {
                xmlResolver = this.xmlResolver;
            }
            else
            {
                xmlResolver = (this.xmlResolver == null) ? new XmlUrlResolver() : this.xmlResolver;
                this.ps.baseUri = xmlResolver.ResolveUri(null, this.url);
                this.ps.baseUriStr = this.ps.baseUri.ToString();
            }
            try
            {
                CompressedStack.Run(this.compressedStack, new ContextCallback(this.OpenUrlDelegate), xmlResolver);
            }
            catch
            {
                this.SetErrorState();
                throw;
            }
            if (this.ps.stream == null)
            {
                this.ThrowWithoutLineInfo("Xml_CannotResolveUrl", this.ps.baseUriStr);
            }
            this.InitStreamInput(this.ps.baseUri, this.ps.baseUriStr, this.ps.stream, null);
            this.reportedEncoding = this.ps.encoding;
        }

        private void OpenUrlDelegate(object xmlResolver)
        {
            this.ps.stream = (Stream) ((System.Xml.XmlResolver) xmlResolver).GetEntity(this.ps.baseUri, null, typeof(Stream));
        }

        private unsafe void ParseAttributes()
        {
            int num2;
            char ch;
            char ch2;
            char ch3;
            char ch5;
            int charPos = this.ps.charPos;
            char[] chars = this.ps.chars;
            NodeData attr = null;
        Label_001A:
            num2 = 0;
            while ((this.xmlCharType.charProperties[ch = chars[charPos]] & 1) != 0)
            {
                switch (ch)
                {
                    case '\n':
                        this.OnNewLine(charPos + 1);
                        num2++;
                        break;

                    case '\r':
                        if (chars[charPos + 1] == '\n')
                        {
                            this.OnNewLine(charPos + 2);
                            num2++;
                            charPos++;
                            break;
                        }
                        if ((charPos + 1) != this.ps.charsUsed)
                        {
                            this.OnNewLine(charPos + 1);
                            num2++;
                        }
                        else
                        {
                            this.ps.charPos = charPos;
                            goto Label_0431;
                        }
                        break;
                }
                charPos++;
            }
            int num3 = 0;
            if ((this.xmlCharType.charProperties[ch2 = chars[charPos]] & 4) != 0)
            {
                num3 = 1;
            }
            if (num3 == 0)
            {
                switch (ch2)
                {
                    case '>':
                        this.ps.charPos = charPos + 1;
                        this.parsingFunction = ParsingFunction.MoveToElementContent;
                        goto Label_0474;

                    case '/':
                        if ((charPos + 1) == this.ps.charsUsed)
                        {
                            goto Label_0431;
                        }
                        if (chars[charPos + 1] == '>')
                        {
                            this.ps.charPos = charPos + 2;
                            this.curNode.IsEmptyElement = true;
                            this.nextParsingFunction = this.parsingFunction;
                            this.parsingFunction = ParsingFunction.PopEmptyElementContext;
                            goto Label_0474;
                        }
                        this.ThrowUnexpectedToken((int) (charPos + 1), ">");
                        goto Label_0188;
                }
                if (charPos == this.ps.charsUsed)
                {
                    goto Label_0431;
                }
                if ((ch2 != ':') || this.supportNamespaces)
                {
                    this.Throw(charPos, "Xml_BadStartNameChar", XmlException.BuildCharExceptionArgs(chars, this.ps.charsUsed, charPos));
                }
            }
        Label_0188:
            if (charPos == this.ps.charPos)
            {
                this.ThrowExpectingWhitespace(charPos);
            }
            this.ps.charPos = charPos;
            int linePos = this.ps.LinePos;
            int colonPos = -1;
            charPos += num3;
        Label_01BE:
            while ((this.xmlCharType.charProperties[ch3 = chars[charPos]] & 8) != 0)
            {
                charPos++;
            }
            if (ch3 == ':')
            {
                if (colonPos != -1)
                {
                    if (this.supportNamespaces)
                    {
                        this.Throw(charPos, "Xml_BadNameChar", XmlException.BuildCharExceptionArgs(':', '\0'));
                        goto Label_0267;
                    }
                    charPos++;
                    goto Label_01BE;
                }
                colonPos = charPos;
                charPos++;
                if ((this.xmlCharType.charProperties[chars[charPos]] & 4) != 0)
                {
                    charPos++;
                    goto Label_01BE;
                }
                charPos = this.ParseQName(out colonPos);
                chars = this.ps.chars;
            }
            else if ((charPos + 1) >= this.ps.charsUsed)
            {
                charPos = this.ParseQName(out colonPos);
                chars = this.ps.chars;
            }
        Label_0267:
            attr = this.AddAttribute(charPos, colonPos);
            attr.SetLineInfo(this.ps.LineNo, linePos);
            if (chars[charPos] != '=')
            {
                this.ps.charPos = charPos;
                this.EatWhitespaces(null);
                charPos = this.ps.charPos;
                if (chars[charPos] != '=')
                {
                    this.ThrowUnexpectedToken("=");
                }
            }
            charPos++;
            char quoteChar = chars[charPos];
            if ((quoteChar != '"') && (quoteChar != '\''))
            {
                this.ps.charPos = charPos;
                this.EatWhitespaces(null);
                charPos = this.ps.charPos;
                quoteChar = chars[charPos];
                if ((quoteChar != '"') && (quoteChar != '\''))
                {
                    this.ThrowUnexpectedToken("\"", "'");
                }
            }
            charPos++;
            this.ps.charPos = charPos;
            attr.quoteChar = quoteChar;
            attr.SetLineInfo2(this.ps.LineNo, this.ps.LinePos);
            while ((this.xmlCharType.charProperties[ch5 = chars[charPos]] & 0x80) != 0)
            {
                charPos++;
            }
            if (ch5 == quoteChar)
            {
                attr.SetValue(chars, this.ps.charPos, charPos - this.ps.charPos);
                charPos++;
                this.ps.charPos = charPos;
            }
            else
            {
                this.ParseAttributeValueSlow(charPos, quoteChar, attr);
                charPos = this.ps.charPos;
                chars = this.ps.chars;
            }
            if (attr.prefix.Length == 0)
            {
                if (Ref.Equal(attr.localName, this.XmlNs))
                {
                    this.OnDefaultNamespaceDecl(attr);
                }
            }
            else if (Ref.Equal(attr.prefix, this.XmlNs))
            {
                this.OnNamespaceDecl(attr);
            }
            else if (Ref.Equal(attr.prefix, this.Xml))
            {
                this.OnXmlReservedAttribute(attr);
            }
            goto Label_001A;
        Label_0431:
            this.ps.lineNo -= num2;
            if (this.ReadData() != 0)
            {
                charPos = this.ps.charPos;
                chars = this.ps.chars;
            }
            else
            {
                this.ThrowUnclosedElements();
            }
            goto Label_001A;
        Label_0474:
            if (this.addDefaultAttributesAndNormalize)
            {
                this.AddDefaultAttributesAndNormalize();
            }
            this.ElementNamespaceLookup();
            if (this.attrNeedNamespaceLookup)
            {
                this.AttributeNamespaceLookup();
                this.attrNeedNamespaceLookup = false;
            }
            if (this.attrDuplWalkCount >= 250)
            {
                this.AttributeDuplCheck();
            }
        }

        private unsafe bool ParseAttributeValueChunk()
        {
            char[] chars = this.ps.chars;
            int charPos = this.ps.charPos;
            this.curNode = this.AddNode((this.index + this.attrCount) + 1, this.index + 2);
            this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
            if (this.emptyEntityInAttributeResolved)
            {
                this.curNode.SetValueNode(XmlNodeType.Text, string.Empty);
                this.emptyEntityInAttributeResolved = false;
                return true;
            }
        Label_0082:
            while ((this.xmlCharType.charProperties[chars[charPos]] & 0x80) != 0)
            {
                charPos++;
            }
            switch (chars[charPos])
            {
                case '\t':
                case '\n':
                    if (this.normalize)
                    {
                        chars[charPos] = ' ';
                    }
                    charPos++;
                    goto Label_0082;

                case '\r':
                    charPos++;
                    goto Label_0082;

                case '"':
                case '\'':
                case '>':
                    charPos++;
                    goto Label_0082;

                case '&':
                    if ((charPos - this.ps.charPos) > 0)
                    {
                        this.stringBuilder.Append(chars, this.ps.charPos, charPos - this.ps.charPos);
                    }
                    this.ps.charPos = charPos;
                    switch (this.HandleEntityReference(true, EntityExpandType.OnlyCharacter, out charPos))
                    {
                        case EntityType.CharacterDec:
                        case EntityType.CharacterHex:
                        case EntityType.CharacterNamed:
                            chars = this.ps.chars;
                            if ((this.normalize && this.xmlCharType.IsWhiteSpace(chars[this.ps.charPos])) && ((charPos - this.ps.charPos) == 1))
                            {
                                chars[this.ps.charPos] = ' ';
                            }
                            goto Label_0249;

                        case EntityType.Unexpanded:
                            if (this.stringBuilder.Length != 0)
                            {
                                goto Label_0333;
                            }
                            this.curNode.lineInfo.linePos++;
                            this.ps.charPos++;
                            this.curNode.SetNamedNode(XmlNodeType.EntityReference, this.ParseEntityName());
                            return true;
                    }
                    break;

                case '<':
                    this.Throw(charPos, "Xml_BadAttributeChar", XmlException.BuildCharExceptionArgs('<', '\0'));
                    goto Label_02AE;

                default:
                    if (charPos != this.ps.charsUsed)
                    {
                        char ch = chars[charPos];
                        if (XmlCharType.IsHighSurrogate(ch))
                        {
                            if ((charPos + 1) == this.ps.charsUsed)
                            {
                                goto Label_02AE;
                            }
                            charPos++;
                            if (XmlCharType.IsLowSurrogate(chars[charPos]))
                            {
                                charPos++;
                                goto Label_0082;
                            }
                        }
                        this.ThrowInvalidChar(chars, this.ps.charsUsed, charPos);
                    }
                    goto Label_02AE;
            }
        Label_0249:
            chars = this.ps.chars;
            goto Label_0082;
        Label_02AE:
            if ((charPos - this.ps.charPos) > 0)
            {
                this.stringBuilder.Append(chars, this.ps.charPos, charPos - this.ps.charPos);
                this.ps.charPos = charPos;
            }
            if (this.ReadData() == 0)
            {
                if (this.stringBuilder.Length > 0)
                {
                    goto Label_0333;
                }
                if (this.HandleEntityEnd(false))
                {
                    this.SetupEndEntityNodeInAttribute();
                    return true;
                }
            }
            charPos = this.ps.charPos;
            chars = this.ps.chars;
            goto Label_0082;
        Label_0333:
            if ((charPos - this.ps.charPos) > 0)
            {
                this.stringBuilder.Append(chars, this.ps.charPos, charPos - this.ps.charPos);
                this.ps.charPos = charPos;
            }
            this.curNode.SetValueNode(XmlNodeType.Text, this.stringBuilder.ToString());
            this.stringBuilder.Length = 0;
            return true;
        }

        private unsafe void ParseAttributeValueSlow(int curPos, char quoteChar, NodeData attr)
        {
            int index = curPos;
            char[] chars = this.ps.chars;
            int entityId = this.ps.entityId;
            int startIndex = 0;
            LineInfo info = new LineInfo(this.ps.lineNo, this.ps.LinePos);
            NodeData lastChunk = null;
        Label_0042:
            while ((this.xmlCharType.charProperties[chars[index]] & 0x80) != 0)
            {
                index++;
            }
            if ((index - this.ps.charPos) > 0)
            {
                this.stringBuilder.Append(chars, this.ps.charPos, index - this.ps.charPos);
                this.ps.charPos = index;
            }
            if ((chars[index] == quoteChar) && (entityId == this.ps.entityId))
            {
                goto Label_0654;
            }
            switch (chars[index])
            {
                case '\t':
                    index++;
                    if (this.normalize)
                    {
                        this.stringBuilder.Append(' ');
                        this.ps.charPos++;
                    }
                    goto Label_0042;

                case '\n':
                    index++;
                    this.OnNewLine(index);
                    if (this.normalize)
                    {
                        this.stringBuilder.Append(' ');
                        this.ps.charPos++;
                    }
                    goto Label_0042;

                case '\r':
                    if (chars[index + 1] != '\n')
                    {
                        if (((index + 1) >= this.ps.charsUsed) && !this.ps.isEof)
                        {
                            goto Label_055F;
                        }
                        index++;
                        if (this.normalize)
                        {
                            this.stringBuilder.Append(' ');
                            this.ps.charPos = index;
                        }
                        break;
                    }
                    index += 2;
                    if (this.normalize)
                    {
                        this.stringBuilder.Append(this.ps.eolNormalized ? "  " : " ");
                        this.ps.charPos = index;
                    }
                    break;

                case '"':
                case '\'':
                case '>':
                    index++;
                    goto Label_0042;

                case '&':
                {
                    if ((index - this.ps.charPos) > 0)
                    {
                        this.stringBuilder.Append(chars, this.ps.charPos, index - this.ps.charPos);
                    }
                    this.ps.charPos = index;
                    int num4 = this.ps.entityId;
                    LineInfo info2 = new LineInfo(this.ps.lineNo, this.ps.LinePos + 1);
                    switch (this.HandleEntityReference(true, EntityExpandType.All, out index))
                    {
                        case EntityType.CharacterDec:
                        case EntityType.CharacterHex:
                        case EntityType.CharacterNamed:
                            goto Label_04F8;

                        case EntityType.Unexpanded:
                            if ((this.parsingMode == ParsingMode.Full) && (this.ps.entityId == entityId))
                            {
                                int length = this.stringBuilder.Length - startIndex;
                                if (length > 0)
                                {
                                    NodeData data2 = new NodeData {
                                        lineInfo = info,
                                        depth = attr.depth + 1
                                    };
                                    data2.SetValueNode(XmlNodeType.Text, this.stringBuilder.ToString(startIndex, length));
                                    this.AddAttributeChunkToList(attr, data2, ref lastChunk);
                                }
                                this.ps.charPos++;
                                string localName = this.ParseEntityName();
                                NodeData chunk = new NodeData {
                                    lineInfo = info2,
                                    depth = attr.depth + 1
                                };
                                chunk.SetNamedNode(XmlNodeType.EntityReference, localName);
                                this.AddAttributeChunkToList(attr, chunk, ref lastChunk);
                                this.stringBuilder.Append('&');
                                this.stringBuilder.Append(localName);
                                this.stringBuilder.Append(';');
                                startIndex = this.stringBuilder.Length;
                                info.Set(this.ps.LineNo, this.ps.LinePos);
                                this.fullAttrCleanup = true;
                            }
                            else
                            {
                                this.ps.charPos++;
                                this.ParseEntityName();
                            }
                            index = this.ps.charPos;
                            goto Label_04F8;

                        case EntityType.ExpandedInAttribute:
                            if ((this.parsingMode == ParsingMode.Full) && (num4 == entityId))
                            {
                                int num6 = this.stringBuilder.Length - startIndex;
                                if (num6 > 0)
                                {
                                    NodeData data4 = new NodeData {
                                        lineInfo = info,
                                        depth = attr.depth + 1
                                    };
                                    data4.SetValueNode(XmlNodeType.Text, this.stringBuilder.ToString(startIndex, num6));
                                    this.AddAttributeChunkToList(attr, data4, ref lastChunk);
                                }
                                NodeData data5 = new NodeData {
                                    lineInfo = info2,
                                    depth = attr.depth + 1
                                };
                                data5.SetNamedNode(XmlNodeType.EntityReference, this.ps.entity.Name);
                                this.AddAttributeChunkToList(attr, data5, ref lastChunk);
                                this.fullAttrCleanup = true;
                            }
                            index = this.ps.charPos;
                            goto Label_04F8;
                    }
                    index = this.ps.charPos;
                    goto Label_04F8;
                }
                case '<':
                    this.Throw(index, "Xml_BadAttributeChar", XmlException.BuildCharExceptionArgs('<', '\0'));
                    goto Label_055F;

                default:
                    if (index != this.ps.charsUsed)
                    {
                        char ch = chars[index];
                        if (XmlCharType.IsHighSurrogate(ch))
                        {
                            if ((index + 1) == this.ps.charsUsed)
                            {
                                goto Label_055F;
                            }
                            index++;
                            if (XmlCharType.IsLowSurrogate(chars[index]))
                            {
                                index++;
                                goto Label_0042;
                            }
                        }
                        this.ThrowInvalidChar(chars, this.ps.charsUsed, index);
                    }
                    goto Label_055F;
            }
            this.OnNewLine(index);
            goto Label_0042;
        Label_04F8:
            chars = this.ps.chars;
            goto Label_0042;
        Label_055F:
            if (this.ReadData() == 0)
            {
                if ((this.ps.charsUsed - this.ps.charPos) > 0)
                {
                    if (this.ps.chars[this.ps.charPos] != '\r')
                    {
                        this.Throw("Xml_UnexpectedEOF1");
                    }
                }
                else
                {
                    if (!this.InEntity)
                    {
                        if (this.fragmentType == XmlNodeType.Attribute)
                        {
                            if (entityId != this.ps.entityId)
                            {
                                this.Throw("Xml_EntityRefNesting");
                            }
                            goto Label_0654;
                        }
                        this.Throw("Xml_UnclosedQuote");
                    }
                    if (this.HandleEntityEnd(true))
                    {
                        this.Throw("Xml_InternalError");
                    }
                    if (entityId == this.ps.entityId)
                    {
                        startIndex = this.stringBuilder.Length;
                        info.Set(this.ps.LineNo, this.ps.LinePos);
                    }
                }
            }
            index = this.ps.charPos;
            chars = this.ps.chars;
            goto Label_0042;
        Label_0654:
            if (attr.nextAttrValueChunk != null)
            {
                int num7 = this.stringBuilder.Length - startIndex;
                if (num7 > 0)
                {
                    NodeData data6 = new NodeData {
                        lineInfo = info,
                        depth = attr.depth + 1
                    };
                    data6.SetValueNode(XmlNodeType.Text, this.stringBuilder.ToString(startIndex, num7));
                    this.AddAttributeChunkToList(attr, data6, ref lastChunk);
                }
            }
            this.ps.charPos = index + 1;
            attr.SetValue(this.stringBuilder.ToString());
            this.stringBuilder.Length = 0;
        }

        private void ParseCData()
        {
            this.ParseCDataOrComment(XmlNodeType.CDATA);
        }

        private void ParseCDataOrComment(XmlNodeType type)
        {
            int num;
            int num2;
            if (this.parsingMode == ParsingMode.Full)
            {
                this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
                if (this.ParseCDataOrComment(type, out num, out num2))
                {
                    this.curNode.SetValueNode(type, this.ps.chars, num, num2 - num);
                }
                else
                {
                    do
                    {
                        this.stringBuilder.Append(this.ps.chars, num, num2 - num);
                    }
                    while (!this.ParseCDataOrComment(type, out num, out num2));
                    this.stringBuilder.Append(this.ps.chars, num, num2 - num);
                    this.curNode.SetValueNode(type, this.stringBuilder.ToString());
                    this.stringBuilder.Length = 0;
                }
            }
            else
            {
                while (!this.ParseCDataOrComment(type, out num, out num2))
                {
                }
            }
        }

        private unsafe bool ParseCDataOrComment(XmlNodeType type, out int outStartPos, out int outEndPos)
        {
            char ch2;
            if (((this.ps.charsUsed - this.ps.charPos) < 3) && (this.ReadData() == 0))
            {
                this.Throw("Xml_UnexpectedEOF", (type == XmlNodeType.Comment) ? "Comment" : "CDATA");
            }
            int charPos = this.ps.charPos;
            char[] chars = this.ps.chars;
            int num2 = 0;
            int destPos = -1;
            char ch = (type == XmlNodeType.Comment) ? '-' : ']';
        Label_006B:
            while (((this.xmlCharType.charProperties[ch2 = chars[charPos]] & 0x40) != 0) && (ch2 != ch))
            {
                charPos++;
            }
            if (chars[charPos] == ch)
            {
                if (chars[charPos + 1] == ch)
                {
                    if (chars[charPos + 2] == '>')
                    {
                        if (num2 > 0)
                        {
                            this.ShiftBuffer(destPos + num2, destPos, (charPos - destPos) - num2);
                            outEndPos = charPos - num2;
                        }
                        else
                        {
                            outEndPos = charPos;
                        }
                        outStartPos = this.ps.charPos;
                        this.ps.charPos = charPos + 3;
                        return true;
                    }
                    if ((charPos + 2) == this.ps.charsUsed)
                    {
                        goto Label_0288;
                    }
                    if (type == XmlNodeType.Comment)
                    {
                        this.Throw(charPos, "Xml_InvalidCommentChars");
                    }
                }
                else if ((charPos + 1) == this.ps.charsUsed)
                {
                    goto Label_0288;
                }
                charPos++;
                goto Label_006B;
            }
            switch (chars[charPos])
            {
                case '<':
                case ']':
                case '\t':
                case '&':
                    charPos++;
                    goto Label_006B;

                case '\n':
                    charPos++;
                    this.OnNewLine(charPos);
                    goto Label_006B;

                case '\r':
                    if (chars[charPos + 1] != '\n')
                    {
                        if (((charPos + 1) >= this.ps.charsUsed) && !this.ps.isEof)
                        {
                            goto Label_0288;
                        }
                        if (!this.ps.eolNormalized)
                        {
                            chars[charPos] = '\n';
                        }
                        charPos++;
                        goto Label_021D;
                    }
                    if (!this.ps.eolNormalized && (this.parsingMode == ParsingMode.Full))
                    {
                        if ((charPos - this.ps.charPos) <= 0)
                        {
                            this.ps.charPos++;
                            break;
                        }
                        if (num2 != 0)
                        {
                            this.ShiftBuffer(destPos + num2, destPos, (charPos - destPos) - num2);
                            destPos = charPos - num2;
                            num2++;
                            break;
                        }
                        num2 = 1;
                        destPos = charPos;
                    }
                    break;

                default:
                    if (charPos != this.ps.charsUsed)
                    {
                        char ch3 = chars[charPos];
                        if (XmlCharType.IsHighSurrogate(ch3))
                        {
                            if ((charPos + 1) == this.ps.charsUsed)
                            {
                                goto Label_0288;
                            }
                            charPos++;
                            if (XmlCharType.IsLowSurrogate(chars[charPos]))
                            {
                                charPos++;
                                goto Label_006B;
                            }
                        }
                        this.ThrowInvalidChar(chars, this.ps.charsUsed, charPos);
                    }
                    goto Label_0288;
            }
            charPos += 2;
        Label_021D:
            this.OnNewLine(charPos);
            goto Label_006B;
        Label_0288:
            if (num2 > 0)
            {
                this.ShiftBuffer(destPos + num2, destPos, (charPos - destPos) - num2);
                outEndPos = charPos - num2;
            }
            else
            {
                outEndPos = charPos;
            }
            outStartPos = this.ps.charPos;
            this.ps.charPos = charPos;
            return false;
        }

        private int ParseCharRefInline(int startPos, out int charCount, out EntityType entityType)
        {
            if (this.ps.chars[startPos + 1] == '#')
            {
                return this.ParseNumericCharRefInline(startPos, true, null, out charCount, out entityType);
            }
            charCount = 1;
            entityType = EntityType.CharacterNamed;
            return this.ParseNamedCharRefInline(startPos, true, null);
        }

        private bool ParseComment()
        {
            if (this.ignoreComments)
            {
                ParsingMode parsingMode = this.parsingMode;
                this.parsingMode = ParsingMode.SkipNode;
                this.ParseCDataOrComment(XmlNodeType.Comment);
                this.parsingMode = parsingMode;
                return false;
            }
            this.ParseCDataOrComment(XmlNodeType.Comment);
            return true;
        }

        private bool ParseDoctypeDecl()
        {
            if (this.dtdProcessing == System.Xml.DtdProcessing.Prohibit)
            {
                this.ThrowWithoutLineInfo(this.v1Compat ? "Xml_DtdIsProhibited" : "Xml_DtdIsProhibitedEx");
            }
            while ((this.ps.charsUsed - this.ps.charPos) < 8)
            {
                if (this.ReadData() == 0)
                {
                    this.Throw("Xml_UnexpectedEOF", "DOCTYPE");
                }
            }
            if (!XmlConvert.StrEqual(this.ps.chars, this.ps.charPos, 7, "DOCTYPE"))
            {
                this.ThrowUnexpectedToken((!this.rootElementParsed && (this.dtdInfo == null)) ? "DOCTYPE" : "<!--");
            }
            if (!this.xmlCharType.IsWhiteSpace(this.ps.chars[this.ps.charPos + 7]))
            {
                this.ThrowExpectingWhitespace(this.ps.charPos + 7);
            }
            if (this.dtdInfo != null)
            {
                this.Throw((int) (this.ps.charPos - 2), "Xml_MultipleDTDsProvided");
            }
            if (this.rootElementParsed)
            {
                this.Throw((int) (this.ps.charPos - 2), "Xml_DtdAfterRootElement");
            }
            this.ps.charPos += 8;
            this.EatWhitespaces(null);
            if (this.dtdProcessing == System.Xml.DtdProcessing.Parse)
            {
                this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
                this.ParseDtd();
                this.nextParsingFunction = this.parsingFunction;
                this.parsingFunction = ParsingFunction.ResetAttributesRootLevel;
                return true;
            }
            this.SkipDtd();
            return false;
        }

        private bool ParseDocumentContent()
        {
            bool flag;
        Label_0000:
            flag = false;
            int charPos = this.ps.charPos;
            char[] chars = this.ps.chars;
            if (chars[charPos] != '<')
            {
                int num2;
                if (chars[charPos] != '&')
                {
                    if ((charPos != this.ps.charsUsed) && (!this.v1Compat || (chars[charPos] != '\0')))
                    {
                        if (this.fragmentType != XmlNodeType.Document)
                        {
                            if (this.ParseText())
                            {
                                if ((this.fragmentType == XmlNodeType.None) && (this.curNode.type == XmlNodeType.Text))
                                {
                                    this.fragmentType = XmlNodeType.Element;
                                }
                                return true;
                            }
                        }
                        else if (this.ParseRootLevelWhitespace())
                        {
                            return true;
                        }
                        goto Label_0000;
                    }
                    goto Label_0322;
                }
                if (this.fragmentType == XmlNodeType.Document)
                {
                    this.Throw(charPos, "Xml_InvalidRootData");
                    goto Label_0322;
                }
                if (this.fragmentType == XmlNodeType.None)
                {
                    this.fragmentType = XmlNodeType.Element;
                }
                switch (this.HandleEntityReference(false, EntityExpandType.OnlyGeneral, out num2))
                {
                    case EntityType.CharacterDec:
                    case EntityType.CharacterHex:
                    case EntityType.CharacterNamed:
                        if (!this.ParseText())
                        {
                            goto Label_0000;
                        }
                        return true;

                    case EntityType.Unexpanded:
                        if (this.parsingFunction == ParsingFunction.EntityReference)
                        {
                            this.parsingFunction = this.nextParsingFunction;
                        }
                        this.ParseEntityReference();
                        return true;
                }
            }
            else
            {
                flag = true;
                if ((this.ps.charsUsed - charPos) < 4)
                {
                    goto Label_0322;
                }
                charPos++;
                switch (chars[charPos])
                {
                    case '!':
                        charPos++;
                        if ((this.ps.charsUsed - charPos) >= 2)
                        {
                            if (chars[charPos] != '-')
                            {
                                if (chars[charPos] == '[')
                                {
                                    if (this.fragmentType != XmlNodeType.Document)
                                    {
                                        charPos++;
                                        if ((this.ps.charsUsed - charPos) >= 6)
                                        {
                                            if (XmlConvert.StrEqual(chars, charPos, 6, "CDATA["))
                                            {
                                                this.ps.charPos = charPos + 6;
                                                this.ParseCData();
                                                if (this.fragmentType == XmlNodeType.None)
                                                {
                                                    this.fragmentType = XmlNodeType.Element;
                                                }
                                                return true;
                                            }
                                            this.ThrowUnexpectedToken(charPos, "CDATA[");
                                        }
                                    }
                                    else
                                    {
                                        this.Throw(this.ps.charPos, "Xml_InvalidRootData");
                                    }
                                }
                                else
                                {
                                    if ((this.fragmentType == XmlNodeType.Document) || (this.fragmentType == XmlNodeType.None))
                                    {
                                        this.fragmentType = XmlNodeType.Document;
                                        this.ps.charPos = charPos;
                                        if (this.ParseDoctypeDecl())
                                        {
                                            return true;
                                        }
                                        goto Label_0000;
                                    }
                                    if (this.ParseUnexpectedToken(charPos) == "DOCTYPE")
                                    {
                                        this.Throw("Xml_BadDTDLocation");
                                    }
                                    else
                                    {
                                        this.ThrowUnexpectedToken(charPos, "<!--", "<[CDATA[");
                                    }
                                }
                            }
                            else
                            {
                                if (chars[charPos + 1] == '-')
                                {
                                    this.ps.charPos = charPos + 2;
                                    if (this.ParseComment())
                                    {
                                        return true;
                                    }
                                    goto Label_0000;
                                }
                                this.ThrowUnexpectedToken((int) (charPos + 1), "-");
                            }
                        }
                        goto Label_0322;

                    case '/':
                        this.Throw((int) (charPos + 1), "Xml_UnexpectedEndTag");
                        goto Label_0322;

                    case '?':
                        this.ps.charPos = charPos + 1;
                        if (this.ParsePI())
                        {
                            return true;
                        }
                        goto Label_0000;

                    default:
                        if (this.rootElementParsed)
                        {
                            if (this.fragmentType == XmlNodeType.Document)
                            {
                                this.Throw(charPos, "Xml_MultipleRoots");
                            }
                            if (this.fragmentType == XmlNodeType.None)
                            {
                                this.fragmentType = XmlNodeType.Element;
                            }
                        }
                        this.ps.charPos = charPos;
                        this.rootElementParsed = true;
                        this.ParseElement();
                        return true;
                }
            }
            chars = this.ps.chars;
            charPos = this.ps.charPos;
            goto Label_0000;
        Label_0322:
            if (this.ReadData() != 0)
            {
                charPos = this.ps.charPos;
            }
            else
            {
                if (flag)
                {
                    this.Throw("Xml_InvalidRootData");
                }
                if (this.InEntity)
                {
                    if (this.HandleEntityEnd(true))
                    {
                        this.SetupEndEntityNodeInContent();
                        return true;
                    }
                    goto Label_0000;
                }
                if (!this.rootElementParsed && (this.fragmentType == XmlNodeType.Document))
                {
                    this.ThrowWithoutLineInfo("Xml_MissingRoot");
                }
                if (this.fragmentType == XmlNodeType.None)
                {
                    this.fragmentType = this.rootElementParsed ? XmlNodeType.Document : XmlNodeType.Element;
                }
                this.OnEof();
                return false;
            }
            charPos = this.ps.charPos;
            chars = this.ps.chars;
            goto Label_0000;
        }

        private void ParseDtd()
        {
            this.dtdInfo = DtdParser.Create().ParseInternalDtd(new DtdParserProxy(this), true);
            if ((this.validatingReaderCompatFlag || !this.v1Compat) && (this.dtdInfo.HasDefaultAttributes || this.dtdInfo.HasNonCDataAttributes))
            {
                this.addDefaultAttributesAndNormalize = true;
            }
            this.curNode.SetNamedNode(XmlNodeType.DocumentType, this.dtdInfo.Name.ToString(), string.Empty, null);
            this.curNode.SetValue(this.dtdInfo.InternalDtdSubset);
        }

        private void ParseDtdFromParserContext()
        {
            this.dtdInfo = DtdParser.Create().ParseFreeFloatingDtd(this.fragmentParserContext.BaseURI, this.fragmentParserContext.DocTypeName, this.fragmentParserContext.PublicId, this.fragmentParserContext.SystemId, this.fragmentParserContext.InternalSubset, new DtdParserProxy(this));
            if ((this.validatingReaderCompatFlag || !this.v1Compat) && (this.dtdInfo.HasDefaultAttributes || this.dtdInfo.HasNonCDataAttributes))
            {
                this.addDefaultAttributesAndNormalize = true;
            }
        }

        private unsafe void ParseElement()
        {
            int charPos = this.ps.charPos;
            char[] chars = this.ps.chars;
            int colonPos = -1;
            this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
        Label_003B:
            if ((this.xmlCharType.charProperties[chars[charPos]] & 4) == 0)
            {
                goto Label_00B4;
            }
            charPos++;
        Label_0053:
            while ((this.xmlCharType.charProperties[chars[charPos]] & 8) != 0)
            {
                charPos++;
            }
            if (chars[charPos] == ':')
            {
                if (colonPos != -1)
                {
                    if (this.supportNamespaces)
                    {
                        this.Throw(charPos, "Xml_BadNameChar", XmlException.BuildCharExceptionArgs(':', '\0'));
                        goto Label_00B4;
                    }
                    charPos++;
                    goto Label_0053;
                }
                colonPos = charPos;
                charPos++;
                goto Label_003B;
            }
            if ((charPos + 1) < this.ps.charsUsed)
            {
                goto Label_00C9;
            }
        Label_00B4:
            charPos = this.ParseQName(out colonPos);
            chars = this.ps.chars;
        Label_00C9:
            this.namespaceManager.PushScope();
            if ((colonPos == -1) || !this.supportNamespaces)
            {
                this.curNode.SetNamedNode(XmlNodeType.Element, this.nameTable.Add(chars, this.ps.charPos, charPos - this.ps.charPos));
            }
            else
            {
                int num3 = this.ps.charPos;
                int num4 = colonPos - num3;
                if ((num4 == this.lastPrefix.Length) && XmlConvert.StrEqual(chars, num3, num4, this.lastPrefix))
                {
                    this.curNode.SetNamedNode(XmlNodeType.Element, this.nameTable.Add(chars, colonPos + 1, (charPos - colonPos) - 1), this.lastPrefix, null);
                }
                else
                {
                    this.curNode.SetNamedNode(XmlNodeType.Element, this.nameTable.Add(chars, colonPos + 1, (charPos - colonPos) - 1), this.nameTable.Add(chars, this.ps.charPos, num4), null);
                    this.lastPrefix = this.curNode.prefix;
                }
            }
            char index = chars[charPos];
            if ((this.xmlCharType.charProperties[index] & 1) != 0)
            {
                this.ps.charPos = charPos;
                this.ParseAttributes();
            }
            else
            {
                if (index == '>')
                {
                    this.ps.charPos = charPos + 1;
                    this.parsingFunction = ParsingFunction.MoveToElementContent;
                }
                else if (index == '/')
                {
                    if ((charPos + 1) == this.ps.charsUsed)
                    {
                        this.ps.charPos = charPos;
                        if (this.ReadData() == 0)
                        {
                            this.Throw(charPos, "Xml_UnexpectedEOF", ">");
                        }
                        charPos = this.ps.charPos;
                        chars = this.ps.chars;
                    }
                    if (chars[charPos + 1] == '>')
                    {
                        this.curNode.IsEmptyElement = true;
                        this.nextParsingFunction = this.parsingFunction;
                        this.parsingFunction = ParsingFunction.PopEmptyElementContext;
                        this.ps.charPos = charPos + 2;
                    }
                    else
                    {
                        this.ThrowUnexpectedToken(charPos, ">");
                    }
                }
                else
                {
                    this.Throw(charPos, "Xml_BadNameChar", XmlException.BuildCharExceptionArgs(chars, this.ps.charsUsed, charPos));
                }
                if (this.addDefaultAttributesAndNormalize)
                {
                    this.AddDefaultAttributesAndNormalize();
                }
                this.ElementNamespaceLookup();
            }
        }

        private bool ParseElementContent()
        {
            int num;
        Label_0000:
            num = this.ps.charPos;
            char[] chars = this.ps.chars;
            char ch = chars[num];
            if (ch == '&')
            {
                if (this.ParseText())
                {
                    return true;
                }
                goto Label_0000;
            }
            if (ch != '<')
            {
                if (num != this.ps.charsUsed)
                {
                    if (this.ParseText())
                    {
                        return true;
                    }
                    goto Label_0000;
                }
            }
            else
            {
                switch (chars[num + 1])
                {
                    case '!':
                        num += 2;
                        if ((this.ps.charsUsed - num) >= 2)
                        {
                            if (chars[num] != '-')
                            {
                                if (chars[num] == '[')
                                {
                                    num++;
                                    if ((this.ps.charsUsed - num) >= 6)
                                    {
                                        if (XmlConvert.StrEqual(chars, num, 6, "CDATA["))
                                        {
                                            this.ps.charPos = num + 6;
                                            this.ParseCData();
                                            return true;
                                        }
                                        this.ThrowUnexpectedToken(num, "CDATA[");
                                    }
                                }
                                else if (this.ParseUnexpectedToken(num) == "DOCTYPE")
                                {
                                    this.Throw("Xml_BadDTDLocation");
                                }
                                else
                                {
                                    this.ThrowUnexpectedToken(num, "<!--", "<[CDATA[");
                                }
                                break;
                            }
                            if (chars[num + 1] == '-')
                            {
                                this.ps.charPos = num + 2;
                                if (this.ParseComment())
                                {
                                    return true;
                                }
                                goto Label_0000;
                            }
                            this.ThrowUnexpectedToken((int) (num + 1), "-");
                        }
                        break;

                    case '/':
                        this.ps.charPos = num + 2;
                        this.ParseEndElement();
                        return true;

                    case '?':
                        this.ps.charPos = num + 2;
                        if (this.ParsePI())
                        {
                            return true;
                        }
                        goto Label_0000;

                    default:
                        if ((num + 1) == this.ps.charsUsed)
                        {
                            break;
                        }
                        this.ps.charPos = num + 1;
                        this.ParseElement();
                        return true;
                }
            }
            if (this.ReadData() != 0)
            {
                goto Label_0000;
            }
            if ((this.ps.charsUsed - this.ps.charPos) != 0)
            {
                this.ThrowUnclosedElements();
            }
            if (!this.InEntity)
            {
                if ((this.index == 0) && (this.fragmentType != XmlNodeType.Document))
                {
                    this.OnEof();
                    return false;
                }
                this.ThrowUnclosedElements();
            }
            if (!this.HandleEntityEnd(true))
            {
                goto Label_0000;
            }
            this.SetupEndEntityNodeInContent();
            return true;
        }

        private unsafe void ParseEndElement()
        {
            int num3;
            NodeData startTag = this.nodes[this.index - 1];
            int length = startTag.prefix.Length;
            int num2 = startTag.localName.Length;
            while ((this.ps.charsUsed - this.ps.charPos) < ((length + num2) + 1))
            {
                if (this.ReadData() == 0)
                {
                    break;
                }
            }
            char[] chars = this.ps.chars;
            if (startTag.prefix.Length == 0)
            {
                if (!XmlConvert.StrEqual(chars, this.ps.charPos, num2, startTag.localName))
                {
                    this.ThrowTagMismatch(startTag);
                }
                num3 = num2;
            }
            else
            {
                int index = this.ps.charPos + length;
                if ((!XmlConvert.StrEqual(chars, this.ps.charPos, length, startTag.prefix) || (chars[index] != ':')) || !XmlConvert.StrEqual(chars, index + 1, num2, startTag.localName))
                {
                    this.ThrowTagMismatch(startTag);
                }
                num3 = (num2 + length) + 1;
            }
            LineInfo info = new LineInfo(this.ps.lineNo, this.ps.LinePos);
            while (true)
            {
                int num5 = this.ps.charPos + num3;
                chars = this.ps.chars;
                if (num5 != this.ps.charsUsed)
                {
                    if (((this.xmlCharType.charProperties[chars[num5]] & 8) != 0) || (chars[num5] == ':'))
                    {
                        this.ThrowTagMismatch(startTag);
                    }
                    if (chars[num5] != '>')
                    {
                        char ch;
                        while (this.xmlCharType.IsWhiteSpace(ch = chars[num5]))
                        {
                            num5++;
                            switch (ch)
                            {
                                case '\n':
                                    this.OnNewLine(num5);
                                    break;

                                case '\r':
                                    if (chars[num5] == '\n')
                                    {
                                        num5++;
                                    }
                                    else if ((num5 == this.ps.charsUsed) && !this.ps.isEof)
                                    {
                                        break;
                                    }
                                    this.OnNewLine(num5);
                                    break;
                            }
                        }
                    }
                    if (chars[num5] == '>')
                    {
                        this.index--;
                        this.curNode = this.nodes[this.index];
                        startTag.lineInfo = info;
                        startTag.type = XmlNodeType.EndElement;
                        this.ps.charPos = num5 + 1;
                        this.nextParsingFunction = (this.index > 0) ? this.parsingFunction : ParsingFunction.DocumentContent;
                        this.parsingFunction = ParsingFunction.PopElementContext;
                        return;
                    }
                    if (num5 != this.ps.charsUsed)
                    {
                        this.ThrowUnexpectedToken(num5, ">");
                    }
                }
                if (this.ReadData() == 0)
                {
                    this.ThrowUnclosedElements();
                }
            }
        }

        private string ParseEntityName()
        {
            int num;
            try
            {
                num = this.ParseName();
            }
            catch (XmlException)
            {
                this.Throw("Xml_ErrorParsingEntityName");
                return null;
            }
            if (this.ps.chars[num] != ';')
            {
                this.Throw("Xml_ErrorParsingEntityName");
            }
            string str = this.nameTable.Add(this.ps.chars, this.ps.charPos, num - this.ps.charPos);
            this.ps.charPos = num + 1;
            return str;
        }

        private void ParseEntityReference()
        {
            this.ps.charPos++;
            this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
            this.curNode.SetNamedNode(XmlNodeType.EntityReference, this.ParseEntityName());
        }

        private bool ParseFragmentAttribute()
        {
            if (this.curNode.type == XmlNodeType.None)
            {
                this.curNode.type = XmlNodeType.Attribute;
                this.curAttrIndex = 0;
                this.ParseAttributeValueSlow(this.ps.charPos, ' ', this.curNode);
            }
            else
            {
                this.parsingFunction = ParsingFunction.InReadAttributeValue;
            }
            if (this.ReadAttributeValue())
            {
                this.parsingFunction = ParsingFunction.FragmentAttribute;
                return true;
            }
            this.OnEof();
            return false;
        }

        private int ParseName()
        {
            int num;
            return this.ParseQName(false, 0, out num);
        }

        private int ParseNamedCharRef(bool expand, StringBuilder internalSubsetBuilder)
        {
            int num;
        Label_0000:
            switch ((num = this.ParseNamedCharRefInline(this.ps.charPos, expand, internalSubsetBuilder)))
            {
                case -2:
                    if (this.ReadData() != 0)
                    {
                        goto Label_0000;
                    }
                    return -1;

                case -1:
                    return -1;
            }
            if (expand)
            {
                this.ps.charPos = num - 1;
            }
            return num;
        }

        private int ParseNamedCharRefInline(int startPos, bool expand, StringBuilder internalSubsetBuilder)
        {
            char ch;
            int index = startPos + 1;
            char[] chars = this.ps.chars;
            switch (chars[index])
            {
                case 'l':
                    if ((this.ps.charsUsed - index) < 3)
                    {
                        break;
                    }
                    if ((chars[index + 1] == 't') && (chars[index + 2] == ';'))
                    {
                        index += 3;
                        ch = '<';
                        goto Label_0175;
                    }
                    return -1;

                case 'q':
                    if ((this.ps.charsUsed - index) < 5)
                    {
                        break;
                    }
                    if (((chars[index + 1] == 'u') && (chars[index + 2] == 'o')) && ((chars[index + 3] == 't') && (chars[index + 4] == ';')))
                    {
                        index += 5;
                        ch = '"';
                        goto Label_0175;
                    }
                    return -1;

                case 'a':
                    index++;
                    if (chars[index] == 'm')
                    {
                        if ((this.ps.charsUsed - index) >= 3)
                        {
                            if ((chars[index + 1] != 'p') || (chars[index + 2] != ';'))
                            {
                                return -1;
                            }
                            index += 3;
                            ch = '&';
                            goto Label_0175;
                        }
                        break;
                    }
                    if (chars[index] == 'p')
                    {
                        if ((this.ps.charsUsed - index) >= 4)
                        {
                            if (((chars[index + 1] != 'o') || (chars[index + 2] != 's')) || (chars[index + 3] != ';'))
                            {
                                return -1;
                            }
                            index += 4;
                            ch = '\'';
                            goto Label_0175;
                        }
                        break;
                    }
                    if (index >= this.ps.charsUsed)
                    {
                        break;
                    }
                    return -1;

                case 'g':
                    if ((this.ps.charsUsed - index) < 3)
                    {
                        break;
                    }
                    if ((chars[index + 1] == 't') && (chars[index + 2] == ';'))
                    {
                        index += 3;
                        ch = '>';
                        goto Label_0175;
                    }
                    return -1;

                default:
                    return -1;
            }
            return -2;
        Label_0175:
            if (expand)
            {
                if (internalSubsetBuilder != null)
                {
                    internalSubsetBuilder.Append(this.ps.chars, this.ps.charPos, index - this.ps.charPos);
                }
                this.ps.chars[index - 1] = ch;
            }
            return index;
        }

        private int ParseNumericCharRef(bool expand, StringBuilder internalSubsetBuilder, out EntityType entityType)
        {
            int num;
            int num2;
            int num3;
        Label_0000:
            num3 = num = this.ParseNumericCharRefInline(this.ps.charPos, expand, internalSubsetBuilder, out num2, out entityType);
            if (num3 == -2)
            {
                if (this.ReadData() == 0)
                {
                    this.Throw("Xml_UnexpectedEOF");
                }
                goto Label_0000;
            }
            if (expand)
            {
                this.ps.charPos = num - num2;
            }
            return num;
        }

        private int ParseNumericCharRefInline(int startPos, bool expand, StringBuilder internalSubsetBuilder, out int charCount, out EntityType entityType)
        {
            char ch3;
            char ch4;
            int combinedChar = 0;
            string res = null;
            char[] chars = this.ps.chars;
            int index = startPos + 2;
            charCount = 0;
            int num3 = 0;
            try
            {
                if (chars[index] == 'x')
                {
                    index++;
                    num3 = index;
                    res = "Xml_BadHexEntity";
                    while (true)
                    {
                        char ch = chars[index];
                        if ((ch >= '0') && (ch <= '9'))
                        {
                            combinedChar = ((combinedChar * 0x10) + ch) - 0x30;
                        }
                        else if ((ch >= 'a') && (ch <= 'f'))
                        {
                            combinedChar = (((combinedChar * 0x10) + 10) + ch) - 0x61;
                        }
                        else
                        {
                            if ((ch < 'A') || (ch > 'F'))
                            {
                                break;
                            }
                            combinedChar = (((combinedChar * 0x10) + 10) + ch) - 0x41;
                        }
                        index++;
                    }
                    entityType = EntityType.CharacterHex;
                }
                else if (index < this.ps.charsUsed)
                {
                    num3 = index;
                    res = "Xml_BadDecimalEntity";
                    while ((chars[index] >= '0') && (chars[index] <= '9'))
                    {
                        combinedChar = ((combinedChar * 10) + chars[index]) - 0x30;
                        index++;
                    }
                    entityType = EntityType.CharacterDec;
                }
                else
                {
                    entityType = EntityType.Skipped;
                    return -2;
                }
            }
            catch (OverflowException exception)
            {
                this.ps.charPos = index;
                entityType = EntityType.Skipped;
                this.Throw("Xml_CharEntityOverflow", (string) null, (Exception) exception);
            }
            if ((chars[index] != ';') || (num3 == index))
            {
                if (index == this.ps.charsUsed)
                {
                    return -2;
                }
                this.Throw(index, res);
            }
            if (combinedChar <= 0xffff)
            {
                char ch2 = (char) combinedChar;
                if (!this.xmlCharType.IsCharData(ch2) && ((this.v1Compat && this.normalize) || (!this.v1Compat && this.checkCharacters)))
                {
                    this.Throw((this.ps.chars[this.ps.charPos + 2] == 'x') ? (this.ps.charPos + 3) : (this.ps.charPos + 2), "Xml_InvalidCharacter", XmlException.BuildCharExceptionArgs(ch2, '\0'));
                }
                if (expand)
                {
                    if (internalSubsetBuilder != null)
                    {
                        internalSubsetBuilder.Append(this.ps.chars, this.ps.charPos, (index - this.ps.charPos) + 1);
                    }
                    chars[index] = ch2;
                }
                charCount = 1;
                return (index + 1);
            }
            XmlCharType.SplitSurrogateChar(combinedChar, out ch3, out ch4);
            if (this.normalize && (!XmlCharType.IsHighSurrogate(ch4) || !XmlCharType.IsLowSurrogate(ch3)))
            {
                this.Throw((this.ps.chars[this.ps.charPos + 2] == 'x') ? (this.ps.charPos + 3) : (this.ps.charPos + 2), "Xml_InvalidCharacter", XmlException.BuildCharExceptionArgs(ch4, ch3));
            }
            if (expand)
            {
                if (internalSubsetBuilder != null)
                {
                    internalSubsetBuilder.Append(this.ps.chars, this.ps.charPos, (index - this.ps.charPos) + 1);
                }
                chars[index - 1] = ch4;
                chars[index] = ch3;
            }
            charCount = 2;
            return (index + 1);
        }

        private bool ParsePI()
        {
            return this.ParsePI(null);
        }

        private bool ParsePI(StringBuilder piInDtdStringBuilder)
        {
            int num2;
            int num3;
            if (this.parsingMode == ParsingMode.Full)
            {
                this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
            }
            int num = this.ParseName();
            string strA = this.nameTable.Add(this.ps.chars, this.ps.charPos, num - this.ps.charPos);
            if (string.Compare(strA, "xml", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.Throw(strA.Equals("xml") ? "Xml_XmlDeclNotFirst" : "Xml_InvalidPIName", strA);
            }
            this.ps.charPos = num;
            if (piInDtdStringBuilder == null)
            {
                if (!this.ignorePIs && (this.parsingMode == ParsingMode.Full))
                {
                    this.curNode.SetNamedNode(XmlNodeType.ProcessingInstruction, strA);
                }
            }
            else
            {
                piInDtdStringBuilder.Append(strA);
            }
            char ch = this.ps.chars[this.ps.charPos];
            if (this.EatWhitespaces(piInDtdStringBuilder) == 0)
            {
                if ((this.ps.charsUsed - this.ps.charPos) < 2)
                {
                    this.ReadData();
                }
                if ((ch != '?') || (this.ps.chars[this.ps.charPos + 1] != '>'))
                {
                    this.Throw("Xml_BadNameChar", XmlException.BuildCharExceptionArgs(this.ps.chars, this.ps.charsUsed, this.ps.charPos));
                }
            }
            if (this.ParsePIValue(out num2, out num3))
            {
                if (piInDtdStringBuilder == null)
                {
                    if (this.ignorePIs)
                    {
                        return false;
                    }
                    if (this.parsingMode == ParsingMode.Full)
                    {
                        this.curNode.SetValue(this.ps.chars, num2, num3 - num2);
                    }
                }
                else
                {
                    piInDtdStringBuilder.Append(this.ps.chars, num2, num3 - num2);
                }
            }
            else
            {
                StringBuilder stringBuilder;
                if (piInDtdStringBuilder == null)
                {
                    if (this.ignorePIs || (this.parsingMode != ParsingMode.Full))
                    {
                        while (!this.ParsePIValue(out num2, out num3))
                        {
                        }
                        return false;
                    }
                    stringBuilder = this.stringBuilder;
                }
                else
                {
                    stringBuilder = piInDtdStringBuilder;
                }
                do
                {
                    stringBuilder.Append(this.ps.chars, num2, num3 - num2);
                }
                while (!this.ParsePIValue(out num2, out num3));
                stringBuilder.Append(this.ps.chars, num2, num3 - num2);
                if (piInDtdStringBuilder == null)
                {
                    this.curNode.SetValue(this.stringBuilder.ToString());
                    this.stringBuilder.Length = 0;
                }
            }
            return true;
        }

        private unsafe bool ParsePIValue(out int outStartPos, out int outEndPos)
        {
            char ch;
            if (((this.ps.charsUsed - this.ps.charPos) < 2) && (this.ReadData() == 0))
            {
                this.Throw(this.ps.charsUsed, "Xml_UnexpectedEOF", "PI");
            }
            int charPos = this.ps.charPos;
            char[] chars = this.ps.chars;
            int num2 = 0;
            int destPos = -1;
        Label_005F:
            while (((this.xmlCharType.charProperties[ch = chars[charPos]] & 0x40) != 0) && (ch != '?'))
            {
                charPos++;
            }
            switch (chars[charPos])
            {
                case '<':
                case ']':
                case '\t':
                case '&':
                    charPos++;
                    goto Label_005F;

                case '?':
                    if (chars[charPos + 1] != '>')
                    {
                        if ((charPos + 1) == this.ps.charsUsed)
                        {
                            goto Label_024F;
                        }
                        charPos++;
                        goto Label_005F;
                    }
                    if (num2 > 0)
                    {
                        this.ShiftBuffer(destPos + num2, destPos, (charPos - destPos) - num2);
                        outEndPos = charPos - num2;
                    }
                    else
                    {
                        outEndPos = charPos;
                    }
                    outStartPos = this.ps.charPos;
                    this.ps.charPos = charPos + 2;
                    return true;

                case '\n':
                    charPos++;
                    this.OnNewLine(charPos);
                    goto Label_005F;

                case '\r':
                    if (chars[charPos + 1] != '\n')
                    {
                        if (((charPos + 1) >= this.ps.charsUsed) && !this.ps.isEof)
                        {
                            goto Label_024F;
                        }
                        if (!this.ps.eolNormalized)
                        {
                            chars[charPos] = '\n';
                        }
                        charPos++;
                        goto Label_01DF;
                    }
                    if (!this.ps.eolNormalized && (this.parsingMode == ParsingMode.Full))
                    {
                        if ((charPos - this.ps.charPos) <= 0)
                        {
                            this.ps.charPos++;
                            break;
                        }
                        if (num2 != 0)
                        {
                            this.ShiftBuffer(destPos + num2, destPos, (charPos - destPos) - num2);
                            destPos = charPos - num2;
                            num2++;
                            break;
                        }
                        num2 = 1;
                        destPos = charPos;
                    }
                    break;

                default:
                    if (charPos != this.ps.charsUsed)
                    {
                        char ch2 = chars[charPos];
                        if (XmlCharType.IsHighSurrogate(ch2))
                        {
                            if ((charPos + 1) == this.ps.charsUsed)
                            {
                                goto Label_024F;
                            }
                            charPos++;
                            if (XmlCharType.IsLowSurrogate(chars[charPos]))
                            {
                                charPos++;
                                goto Label_005F;
                            }
                        }
                        this.ThrowInvalidChar(chars, this.ps.charsUsed, charPos);
                        goto Label_005F;
                    }
                    goto Label_024F;
            }
            charPos += 2;
        Label_01DF:
            this.OnNewLine(charPos);
            goto Label_005F;
        Label_024F:
            if (num2 > 0)
            {
                this.ShiftBuffer(destPos + num2, destPos, (charPos - destPos) - num2);
                outEndPos = charPos - num2;
            }
            else
            {
                outEndPos = charPos;
            }
            outStartPos = this.ps.charPos;
            this.ps.charPos = charPos;
            return false;
        }

        private int ParseQName(out int colonPos)
        {
            return this.ParseQName(true, 0, out colonPos);
        }

        private unsafe int ParseQName(bool isQName, int startOffset, out int colonPos)
        {
            char[] chars;
            int num = -1;
            int index = this.ps.charPos + startOffset;
        Label_0010:
            chars = this.ps.chars;
            if ((this.xmlCharType.charProperties[chars[index]] & 4) != 0)
            {
                index++;
            }
            else
            {
                if ((index + 1) >= this.ps.charsUsed)
                {
                    if (this.ReadDataInName(ref index))
                    {
                        goto Label_0010;
                    }
                    this.Throw(index, "Xml_UnexpectedEOF", "Name");
                }
                if ((chars[index] != ':') || this.supportNamespaces)
                {
                    this.Throw(index, "Xml_BadStartNameChar", XmlException.BuildCharExceptionArgs(chars, this.ps.charsUsed, index));
                }
            }
        Label_008E:
            while ((this.xmlCharType.charProperties[chars[index]] & 8) != 0)
            {
                index++;
            }
            if (chars[index] == ':')
            {
                if (this.supportNamespaces)
                {
                    if ((num != -1) || !isQName)
                    {
                        this.Throw(index, "Xml_BadNameChar", XmlException.BuildCharExceptionArgs(':', '\0'));
                    }
                    num = index - this.ps.charPos;
                    index++;
                    goto Label_0010;
                }
                num = index - this.ps.charPos;
                index++;
                goto Label_008E;
            }
            if (index == this.ps.charsUsed)
            {
                if (this.ReadDataInName(ref index))
                {
                    chars = this.ps.chars;
                    goto Label_008E;
                }
                this.Throw(index, "Xml_UnexpectedEOF", "Name");
            }
            colonPos = (num == -1) ? -1 : (this.ps.charPos + num);
            return index;
        }

        private bool ParseRootLevelWhitespace()
        {
            XmlNodeType whitespaceType = this.GetWhitespaceType();
            if (whitespaceType == XmlNodeType.None)
            {
                this.EatWhitespaces(null);
                if (((this.ps.chars[this.ps.charPos] == '<') || ((this.ps.charsUsed - this.ps.charPos) == 0)) || this.ZeroEndingStream(this.ps.charPos))
                {
                    return false;
                }
            }
            else
            {
                this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
                this.EatWhitespaces(this.stringBuilder);
                if (((this.ps.chars[this.ps.charPos] == '<') || ((this.ps.charsUsed - this.ps.charPos) == 0)) || this.ZeroEndingStream(this.ps.charPos))
                {
                    if (this.stringBuilder.Length > 0)
                    {
                        this.curNode.SetValueNode(whitespaceType, this.stringBuilder.ToString());
                        this.stringBuilder.Length = 0;
                        return true;
                    }
                    return false;
                }
            }
            if (this.xmlCharType.IsCharData(this.ps.chars[this.ps.charPos]))
            {
                this.Throw("Xml_InvalidRootData");
            }
            else
            {
                this.ThrowInvalidChar(this.ps.chars, this.ps.charsUsed, this.ps.charPos);
            }
            return false;
        }

        private bool ParseText()
        {
            int num;
            int num2;
            int outOrChars = 0;
            if (this.parsingMode != ParsingMode.Full)
            {
                while (!this.ParseText(out num, out num2, ref outOrChars))
                {
                }
            }
            else
            {
                this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
                if (this.ParseText(out num, out num2, ref outOrChars))
                {
                    if ((num2 - num) != 0)
                    {
                        XmlNodeType textNodeType = this.GetTextNodeType(outOrChars);
                        if (textNodeType != XmlNodeType.None)
                        {
                            this.curNode.SetValueNode(textNodeType, this.ps.chars, num, num2 - num);
                            return true;
                        }
                    }
                }
                else if (this.v1Compat)
                {
                    do
                    {
                        if ((num2 - num) > 0)
                        {
                            this.stringBuilder.Append(this.ps.chars, num, num2 - num);
                        }
                    }
                    while (!this.ParseText(out num, out num2, ref outOrChars));
                    if ((num2 - num) > 0)
                    {
                        this.stringBuilder.Append(this.ps.chars, num, num2 - num);
                    }
                    XmlNodeType type = this.GetTextNodeType(outOrChars);
                    if (type != XmlNodeType.None)
                    {
                        this.curNode.SetValueNode(type, this.stringBuilder.ToString());
                        this.stringBuilder.Length = 0;
                        return true;
                    }
                    this.stringBuilder.Length = 0;
                }
                else
                {
                    bool flag = false;
                    if (outOrChars > 0x20)
                    {
                        this.curNode.SetValueNode(XmlNodeType.Text, this.ps.chars, num, num2 - num);
                        this.nextParsingFunction = this.parsingFunction;
                        this.parsingFunction = ParsingFunction.PartialTextValue;
                        return true;
                    }
                    if ((num2 - num) > 0)
                    {
                        this.stringBuilder.Append(this.ps.chars, num, num2 - num);
                    }
                    do
                    {
                        flag = this.ParseText(out num, out num2, ref outOrChars);
                        if ((num2 - num) > 0)
                        {
                            this.stringBuilder.Append(this.ps.chars, num, num2 - num);
                        }
                    }
                    while ((!flag && (outOrChars <= 0x20)) && (this.stringBuilder.Length < 0x1000));
                    XmlNodeType type3 = (this.stringBuilder.Length < 0x1000) ? this.GetTextNodeType(outOrChars) : XmlNodeType.Text;
                    if (type3 == XmlNodeType.None)
                    {
                        this.stringBuilder.Length = 0;
                        if (!flag)
                        {
                            while (!this.ParseText(out num, out num2, ref outOrChars))
                            {
                            }
                        }
                    }
                    else
                    {
                        this.curNode.SetValueNode(type3, this.stringBuilder.ToString());
                        this.stringBuilder.Length = 0;
                        if (!flag)
                        {
                            this.nextParsingFunction = this.parsingFunction;
                            this.parsingFunction = ParsingFunction.PartialTextValue;
                        }
                        return true;
                    }
                }
            }
            if (this.parsingFunction == ParsingFunction.ReportEndEntity)
            {
                this.SetupEndEntityNodeInContent();
                this.parsingFunction = this.nextParsingFunction;
                return true;
            }
            if (this.parsingFunction == ParsingFunction.EntityReference)
            {
                this.parsingFunction = this.nextNextParsingFunction;
                this.ParseEntityReference();
                return true;
            }
            return false;
        }

        private unsafe bool ParseText(out int startPos, out int endPos, ref int outOrChars)
        {
            char ch;
            int num8;
            char[] chars = this.ps.chars;
            int charPos = this.ps.charPos;
            int num2 = 0;
            int destPos = -1;
            int num4 = outOrChars;
        Label_002D:
            while ((this.xmlCharType.charProperties[ch = chars[charPos]] & 0x40) != 0)
            {
                num4 |= ch;
                charPos++;
            }
            switch (ch)
            {
                case '<':
                    goto Label_0437;

                case ']':
                    if (((this.ps.charsUsed - charPos) < 3) && !this.ps.isEof)
                    {
                        goto Label_036E;
                    }
                    if ((chars[charPos + 1] == ']') && (chars[charPos + 2] == '>'))
                    {
                        this.Throw(charPos, "Xml_CDATAEndInText");
                    }
                    num4 |= 0x5d;
                    charPos++;
                    goto Label_002D;

                case '\t':
                    charPos++;
                    goto Label_002D;

                case '\n':
                    charPos++;
                    this.OnNewLine(charPos);
                    goto Label_002D;

                case '\r':
                    if (chars[charPos + 1] != '\n')
                    {
                        if (((charPos + 1) >= this.ps.charsUsed) && !this.ps.isEof)
                        {
                            goto Label_036E;
                        }
                        if (!this.ps.eolNormalized)
                        {
                            chars[charPos] = '\n';
                        }
                        charPos++;
                        goto Label_0144;
                    }
                    if (!this.ps.eolNormalized && (this.parsingMode == ParsingMode.Full))
                    {
                        if ((charPos - this.ps.charPos) <= 0)
                        {
                            this.ps.charPos++;
                            break;
                        }
                        if (num2 != 0)
                        {
                            this.ShiftBuffer(destPos + num2, destPos, (charPos - destPos) - num2);
                            destPos = charPos - num2;
                            num2++;
                            break;
                        }
                        num2 = 1;
                        destPos = charPos;
                    }
                    break;

                case '&':
                {
                    int num6;
                    EntityType type;
                    int num5 = this.ParseCharRefInline(charPos, out num6, out type);
                    if (num5 > 0)
                    {
                        if (num2 > 0)
                        {
                            this.ShiftBuffer(destPos + num2, destPos, (charPos - destPos) - num2);
                        }
                        destPos = charPos - num2;
                        num2 += (num5 - charPos) - num6;
                        charPos = num5;
                        if (!this.xmlCharType.IsWhiteSpace(chars[num5 - num6]) || (this.v1Compat && (type == EntityType.CharacterDec)))
                        {
                            num4 |= 0xff;
                        }
                        goto Label_002D;
                    }
                    if (charPos > this.ps.charPos)
                    {
                        goto Label_0437;
                    }
                    switch (this.HandleEntityReference(false, EntityExpandType.All, out charPos))
                    {
                        case EntityType.CharacterDec:
                            if (!this.v1Compat)
                            {
                                goto Label_0229;
                            }
                            num4 |= 0xff;
                            goto Label_025D;

                        case EntityType.CharacterHex:
                        case EntityType.CharacterNamed:
                            goto Label_0229;

                        case EntityType.Unexpanded:
                            this.nextParsingFunction = this.parsingFunction;
                            this.parsingFunction = ParsingFunction.EntityReference;
                            goto Label_042B;
                    }
                    charPos = this.ps.charPos;
                    goto Label_025D;
                }
                default:
                    if (charPos != this.ps.charsUsed)
                    {
                        char ch2 = chars[charPos];
                        if (XmlCharType.IsHighSurrogate(ch2))
                        {
                            if ((charPos + 1) == this.ps.charsUsed)
                            {
                                goto Label_036E;
                            }
                            charPos++;
                            if (XmlCharType.IsLowSurrogate(chars[charPos]))
                            {
                                charPos++;
                                num4 |= ch2;
                                goto Label_002D;
                            }
                        }
                        int num7 = charPos - this.ps.charPos;
                        if (this.ZeroEndingStream(charPos))
                        {
                            chars = this.ps.chars;
                            charPos = this.ps.charPos + num7;
                            goto Label_0437;
                        }
                        this.ThrowInvalidChar(this.ps.chars, this.ps.charsUsed, this.ps.charPos + num7);
                    }
                    goto Label_036E;
            }
            charPos += 2;
        Label_0144:
            this.OnNewLine(charPos);
            goto Label_002D;
        Label_0229:
            if (!this.xmlCharType.IsWhiteSpace(this.ps.chars[charPos - 1]))
            {
                num4 |= 0xff;
            }
        Label_025D:
            chars = this.ps.chars;
            goto Label_002D;
        Label_036E:
            if (charPos > this.ps.charPos)
            {
                goto Label_0437;
            }
            if (this.ReadData() == 0)
            {
                if ((this.ps.charsUsed - this.ps.charPos) > 0)
                {
                    if ((this.ps.chars[this.ps.charPos] != '\r') && (this.ps.chars[this.ps.charPos] != ']'))
                    {
                        this.Throw("Xml_UnexpectedEOF1");
                    }
                }
                else
                {
                    if (!this.InEntity)
                    {
                        goto Label_042B;
                    }
                    if (this.HandleEntityEnd(true))
                    {
                        this.nextParsingFunction = this.parsingFunction;
                        this.parsingFunction = ParsingFunction.ReportEndEntity;
                        goto Label_042B;
                    }
                }
            }
            charPos = this.ps.charPos;
            chars = this.ps.chars;
            goto Label_002D;
        Label_042B:
            endPos = num8 = charPos;
            startPos = num8;
            return true;
        Label_0437:
            if ((this.parsingMode == ParsingMode.Full) && (num2 > 0))
            {
                this.ShiftBuffer(destPos + num2, destPos, (charPos - destPos) - num2);
            }
            startPos = this.ps.charPos;
            endPos = charPos - num2;
            this.ps.charPos = charPos;
            outOrChars = num4;
            return (ch == '<');
        }

        private string ParseUnexpectedToken()
        {
            if (this.ps.charPos == this.ps.charsUsed)
            {
                return null;
            }
            if (!this.xmlCharType.IsNCNameSingleChar(this.ps.chars[this.ps.charPos]))
            {
                return new string(this.ps.chars, this.ps.charPos, 1);
            }
            int index = this.ps.charPos + 1;
            while (this.xmlCharType.IsNCNameSingleChar(this.ps.chars[index]))
            {
                index++;
            }
            return new string(this.ps.chars, this.ps.charPos, index - this.ps.charPos);
        }

        private string ParseUnexpectedToken(int pos)
        {
            this.ps.charPos = pos;
            return this.ParseUnexpectedToken();
        }

        private unsafe bool ParseXmlDeclaration(bool isTextDecl)
        {
            int num2;
            char[] chArray;
            while ((this.ps.charsUsed - this.ps.charPos) < 6)
            {
                if (this.ReadData() == 0)
                {
                    goto Label_07F1;
                }
            }
            if (!XmlConvert.StrEqual(this.ps.chars, this.ps.charPos, 5, "<?xml") || this.xmlCharType.IsNameSingleChar(this.ps.chars[this.ps.charPos + 5]))
            {
                goto Label_07F1;
            }
            if (!isTextDecl)
            {
                this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos + 2);
                this.curNode.SetNamedNode(XmlNodeType.XmlDeclaration, this.Xml);
            }
            this.ps.charPos += 5;
            StringBuilder sb = isTextDecl ? new StringBuilder() : this.stringBuilder;
            int num = 0;
            System.Text.Encoding newEncoding = null;
        Label_00D7:
            num2 = sb.Length;
            int num3 = this.EatWhitespaces((num == 0) ? null : sb);
            if (this.ps.chars[this.ps.charPos] == '?')
            {
                sb.Length = num2;
                if (this.ps.chars[this.ps.charPos + 1] == '>')
                {
                    if (num == 0)
                    {
                        this.Throw(isTextDecl ? "Xml_InvalidTextDecl" : "Xml_InvalidXmlDecl");
                    }
                    this.ps.charPos += 2;
                    if (!isTextDecl)
                    {
                        this.curNode.SetValue(sb.ToString());
                        sb.Length = 0;
                        this.nextParsingFunction = this.parsingFunction;
                        this.parsingFunction = ParsingFunction.ResetAttributesRootLevel;
                    }
                    if (newEncoding == null)
                    {
                        if (isTextDecl)
                        {
                            this.Throw("Xml_InvalidTextDecl");
                        }
                        if (this.afterResetState)
                        {
                            string webName = this.ps.encoding.WebName;
                            if (((webName != "utf-8") && (webName != "utf-16")) && ((webName != "utf-16BE") && !(this.ps.encoding is Ucs4Encoding)))
                            {
                                this.Throw("Xml_EncodingSwitchAfterResetState", (this.ps.encoding.GetByteCount("A") == 1) ? "UTF-8" : "UTF-16");
                            }
                        }
                        if (this.ps.decoder is SafeAsciiDecoder)
                        {
                            this.SwitchEncodingToUTF8();
                        }
                    }
                    else
                    {
                        this.SwitchEncoding(newEncoding);
                    }
                    this.ps.appendMode = false;
                    return true;
                }
                if ((this.ps.charPos + 1) == this.ps.charsUsed)
                {
                    goto Label_07C9;
                }
                this.ThrowUnexpectedToken("'>'");
            }
            if ((num3 == 0) && (num != 0))
            {
                this.ThrowUnexpectedToken("?>");
            }
            int num4 = this.ParseName();
            NodeData data = null;
            char ch2 = this.ps.chars[this.ps.charPos];
            switch (ch2)
            {
                case 'e':
                    if (!XmlConvert.StrEqual(this.ps.chars, this.ps.charPos, num4 - this.ps.charPos, "encoding") || ((num != 1) && (!isTextDecl || (num != 0))))
                    {
                        break;
                    }
                    if (!isTextDecl)
                    {
                        data = this.AddAttributeNoChecks("encoding", 1);
                    }
                    num = 1;
                    goto Label_03D0;

                case 's':
                    if ((!XmlConvert.StrEqual(this.ps.chars, this.ps.charPos, num4 - this.ps.charPos, "standalone") || ((num != 1) && (num != 2))) || isTextDecl)
                    {
                        break;
                    }
                    if (!isTextDecl)
                    {
                        data = this.AddAttributeNoChecks("standalone", 1);
                    }
                    num = 2;
                    goto Label_03D0;

                default:
                    if (((ch2 == 'v') && XmlConvert.StrEqual(this.ps.chars, this.ps.charPos, num4 - this.ps.charPos, "version")) && (num == 0))
                    {
                        if (!isTextDecl)
                        {
                            data = this.AddAttributeNoChecks("version", 1);
                        }
                        goto Label_03D0;
                    }
                    break;
            }
            this.Throw(isTextDecl ? "Xml_InvalidTextDecl" : "Xml_InvalidXmlDecl");
        Label_03D0:
            if (!isTextDecl)
            {
                data.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
            }
            sb.Append(this.ps.chars, this.ps.charPos, num4 - this.ps.charPos);
            this.ps.charPos = num4;
            if (this.ps.chars[this.ps.charPos] != '=')
            {
                this.EatWhitespaces(sb);
                if (this.ps.chars[this.ps.charPos] != '=')
                {
                    this.ThrowUnexpectedToken("=");
                }
            }
            sb.Append('=');
            this.ps.charPos++;
            char ch = this.ps.chars[this.ps.charPos];
            if ((ch != '"') && (ch != '\''))
            {
                this.EatWhitespaces(sb);
                ch = this.ps.chars[this.ps.charPos];
                if ((ch != '"') && (ch != '\''))
                {
                    this.ThrowUnexpectedToken("\"", "'");
                }
            }
            sb.Append(ch);
            this.ps.charPos++;
            if (!isTextDecl)
            {
                data.quoteChar = ch;
                data.SetLineInfo2(this.ps.LineNo, this.ps.LinePos);
            }
            int charPos = this.ps.charPos;
        Label_0541:
            chArray = this.ps.chars;
            while ((this.xmlCharType.charProperties[chArray[charPos]] & 0x80) != 0)
            {
                charPos++;
            }
            if (this.ps.chars[charPos] != ch)
            {
                if (charPos == this.ps.charsUsed)
                {
                    if (this.ReadData() != 0)
                    {
                        goto Label_0541;
                    }
                    this.Throw("Xml_UnclosedQuote");
                }
                else
                {
                    this.Throw(isTextDecl ? "Xml_InvalidTextDecl" : "Xml_InvalidXmlDecl");
                }
                goto Label_07C9;
            }
            switch (num)
            {
                case 0:
                    if (!XmlConvert.StrEqual(this.ps.chars, this.ps.charPos, charPos - this.ps.charPos, "1.0"))
                    {
                        string arg = new string(this.ps.chars, this.ps.charPos, charPos - this.ps.charPos);
                        this.Throw("Xml_InvalidVersionNumber", arg);
                    }
                    else
                    {
                        if (!isTextDecl)
                        {
                            data.SetValue(this.ps.chars, this.ps.charPos, charPos - this.ps.charPos);
                        }
                        num = 1;
                    }
                    goto Label_074E;

                case 1:
                {
                    string newEncodingName = new string(this.ps.chars, this.ps.charPos, charPos - this.ps.charPos);
                    newEncoding = this.CheckEncoding(newEncodingName);
                    if (!isTextDecl)
                    {
                        data.SetValue(newEncodingName);
                    }
                    num = 2;
                    goto Label_074E;
                }
                case 2:
                    if (!XmlConvert.StrEqual(this.ps.chars, this.ps.charPos, charPos - this.ps.charPos, "yes"))
                    {
                        if (XmlConvert.StrEqual(this.ps.chars, this.ps.charPos, charPos - this.ps.charPos, "no"))
                        {
                            this.standalone = false;
                        }
                        else
                        {
                            this.Throw("Xml_InvalidXmlDecl", this.ps.LineNo, this.ps.LinePos - 1);
                        }
                        break;
                    }
                    this.standalone = true;
                    break;

                default:
                    goto Label_074E;
            }
            if (!isTextDecl)
            {
                data.SetValue(this.ps.chars, this.ps.charPos, charPos - this.ps.charPos);
            }
            num = 3;
        Label_074E:
            sb.Append(chArray, this.ps.charPos, charPos - this.ps.charPos);
            sb.Append(ch);
            this.ps.charPos = charPos + 1;
            goto Label_00D7;
        Label_07C9:
            if (this.ps.isEof || (this.ReadData() == 0))
            {
                this.Throw("Xml_UnexpectedEOF1");
            }
            goto Label_00D7;
        Label_07F1:
            if (!isTextDecl)
            {
                this.parsingFunction = this.nextParsingFunction;
            }
            if (this.afterResetState)
            {
                string str4 = this.ps.encoding.WebName;
                if (((str4 != "utf-8") && (str4 != "utf-16")) && ((str4 != "utf-16BE") && !(this.ps.encoding is Ucs4Encoding)))
                {
                    this.Throw("Xml_EncodingSwitchAfterResetState", (this.ps.encoding.GetByteCount("A") == 1) ? "UTF-8" : "UTF-16");
                }
            }
            if (this.ps.decoder is SafeAsciiDecoder)
            {
                this.SwitchEncodingToUTF8();
            }
            this.ps.appendMode = false;
            return false;
        }

        private void ParseXmlDeclarationFragment()
        {
            try
            {
                this.ParseXmlDeclaration(false);
            }
            catch (XmlException exception)
            {
                this.ReThrow(exception, exception.LineNumber, exception.LinePosition - 6);
            }
        }

        private void PopElementContext()
        {
            this.namespaceManager.PopScope();
            if (this.curNode.xmlContextPushed)
            {
                this.PopXmlContext();
            }
        }

        private void PopEntity()
        {
            if (this.ps.stream != null)
            {
                this.ps.stream.Close();
            }
            this.UnregisterEntity();
            this.PopParsingState();
            this.curNode.entityId = this.ps.entityId;
        }

        private void PopParsingState()
        {
            this.ps.Close(true);
            this.ps = this.parsingStatesStack[this.parsingStatesStackTop--];
        }

        private void PopXmlContext()
        {
            this.xmlContext = this.xmlContext.previousContext;
            this.curNode.xmlContextPushed = false;
        }

        private void ProcessDtdFromParserContext(XmlParserContext context)
        {
            switch (this.dtdProcessing)
            {
                case System.Xml.DtdProcessing.Prohibit:
                    this.ThrowWithoutLineInfo("Xml_DtdIsProhibitedEx");
                    return;

                case System.Xml.DtdProcessing.Ignore:
                    break;

                case System.Xml.DtdProcessing.Parse:
                    this.ParseDtdFromParserContext();
                    break;

                default:
                    return;
            }
        }

        private bool PushExternalEntity(IDtdEntityInfo entity)
        {
            if (this.xmlResolver != null)
            {
                Uri baseUri = null;
                this.PushExternalEntityOrSubset(entity.PublicId, entity.SystemId, entity.BaseUriString, ref baseUri, entity.Name);
                this.RegisterEntity(entity);
                int charPos = this.ps.charPos;
                if (this.v1Compat)
                {
                    this.EatWhitespaces(null);
                }
                if (!this.ParseXmlDeclaration(true))
                {
                    this.ps.charPos = charPos;
                }
                return true;
            }
            System.Text.Encoding originalEncoding = this.ps.encoding;
            this.PushParsingState();
            this.InitStringInput(entity.SystemId, originalEncoding, string.Empty);
            this.RegisterEntity(entity);
            this.RegisterConsumedCharacters(0L, true);
            return false;
        }

        private void PushExternalEntityOrSubset(string publicId, string systemId, string baseUriStr, ref Uri baseUri, string entityName)
        {
            Uri uri;
            if ((baseUri == null) && !string.IsNullOrEmpty(baseUriStr))
            {
                baseUri = this.xmlResolver.ResolveUri(null, baseUriStr);
            }
            if (!string.IsNullOrEmpty(publicId))
            {
                try
                {
                    uri = this.xmlResolver.ResolveUri(baseUri, publicId);
                    if (this.OpenAndPush(uri))
                    {
                        return;
                    }
                }
                catch (Exception)
                {
                }
            }
            uri = this.xmlResolver.ResolveUri(baseUri, systemId);
            try
            {
                if (this.OpenAndPush(uri))
                {
                    return;
                }
            }
            catch (Exception exception)
            {
                if (this.v1Compat)
                {
                    throw;
                }
                string message = exception.Message;
                this.Throw(new XmlException((entityName == null) ? "Xml_ErrorOpeningExternalDtd" : "Xml_ErrorOpeningExternalEntity", new string[] { uri.ToString(), message }, exception, 0, 0));
            }
            if (entityName == null)
            {
                this.ThrowWithoutLineInfo("Xml_CannotResolveExternalSubset", new string[] { (publicId != null) ? publicId : string.Empty, systemId }, null);
            }
            else
            {
                this.Throw((this.dtdProcessing == System.Xml.DtdProcessing.Ignore) ? "Xml_CannotResolveEntityDtdIgnored" : "Xml_CannotResolveEntity", entityName);
            }
        }

        private void PushInternalEntity(IDtdEntityInfo entity)
        {
            System.Text.Encoding originalEncoding = this.ps.encoding;
            this.PushParsingState();
            this.InitStringInput((entity.DeclaredUriString != null) ? entity.DeclaredUriString : string.Empty, originalEncoding, entity.Text);
            this.RegisterEntity(entity);
            this.ps.lineNo = entity.LineNumber;
            this.ps.lineStartPos = -entity.LinePosition - 1;
            this.ps.eolNormalized = true;
            this.RegisterConsumedCharacters((long) entity.Text.Length, true);
        }

        private void PushParsingState()
        {
            if (this.parsingStatesStack == null)
            {
                this.parsingStatesStack = new ParsingState[2];
            }
            else if ((this.parsingStatesStackTop + 1) == this.parsingStatesStack.Length)
            {
                ParsingState[] destinationArray = new ParsingState[this.parsingStatesStack.Length * 2];
                Array.Copy(this.parsingStatesStack, 0, destinationArray, 0, this.parsingStatesStack.Length);
                this.parsingStatesStack = destinationArray;
            }
            this.parsingStatesStackTop++;
            this.parsingStatesStack[this.parsingStatesStackTop] = this.ps;
            this.ps.Clear();
        }

        private void PushXmlContext()
        {
            this.xmlContext = new XmlContext(this.xmlContext);
            this.curNode.xmlContextPushed = true;
        }

        public override bool Read()
        {
        Label_0000:
            switch (this.parsingFunction)
            {
                case ParsingFunction.ElementContent:
                    return this.ParseElementContent();

                case ParsingFunction.NoData:
                    this.ThrowWithoutLineInfo("Xml_MissingRoot");
                    return false;

                case ParsingFunction.OpenUrl:
                    this.OpenUrl();
                    break;

                case ParsingFunction.SwitchToInteractive:
                    this.readState = System.Xml.ReadState.Interactive;
                    this.parsingFunction = this.nextParsingFunction;
                    goto Label_0000;

                case ParsingFunction.SwitchToInteractiveXmlDecl:
                    break;

                case ParsingFunction.DocumentContent:
                    return this.ParseDocumentContent();

                case ParsingFunction.MoveToElementContent:
                    this.ResetAttributes();
                    this.index++;
                    this.curNode = this.AddNode(this.index, this.index);
                    this.parsingFunction = ParsingFunction.ElementContent;
                    goto Label_0000;

                case ParsingFunction.PopElementContext:
                    this.PopElementContext();
                    this.parsingFunction = this.nextParsingFunction;
                    goto Label_0000;

                case ParsingFunction.PopEmptyElementContext:
                    this.curNode = this.nodes[this.index];
                    this.curNode.IsEmptyElement = false;
                    this.ResetAttributes();
                    this.PopElementContext();
                    this.parsingFunction = this.nextParsingFunction;
                    goto Label_0000;

                case ParsingFunction.ResetAttributesRootLevel:
                    this.ResetAttributes();
                    this.curNode = this.nodes[this.index];
                    this.parsingFunction = (this.index == 0) ? ParsingFunction.DocumentContent : ParsingFunction.ElementContent;
                    goto Label_0000;

                case ParsingFunction.Error:
                case ParsingFunction.Eof:
                case ParsingFunction.ReaderClosed:
                    return false;

                case ParsingFunction.EntityReference:
                    this.parsingFunction = this.nextParsingFunction;
                    this.ParseEntityReference();
                    return true;

                case ParsingFunction.InIncrementalRead:
                    this.FinishIncrementalRead();
                    return true;

                case ParsingFunction.FragmentAttribute:
                    return this.ParseFragmentAttribute();

                case ParsingFunction.ReportEndEntity:
                    this.SetupEndEntityNodeInContent();
                    this.parsingFunction = this.nextParsingFunction;
                    return true;

                case ParsingFunction.AfterResolveEntityInContent:
                    this.curNode = this.AddNode(this.index, this.index);
                    this.reportedEncoding = this.ps.encoding;
                    this.reportedBaseUri = this.ps.baseUriStr;
                    this.parsingFunction = this.nextParsingFunction;
                    goto Label_0000;

                case ParsingFunction.AfterResolveEmptyEntityInContent:
                    this.curNode = this.AddNode(this.index, this.index);
                    this.curNode.SetValueNode(XmlNodeType.Text, string.Empty);
                    this.curNode.SetLineInfo(this.ps.lineNo, this.ps.LinePos);
                    this.reportedEncoding = this.ps.encoding;
                    this.reportedBaseUri = this.ps.baseUriStr;
                    this.parsingFunction = this.nextParsingFunction;
                    return true;

                case ParsingFunction.XmlDeclarationFragment:
                    this.ParseXmlDeclarationFragment();
                    this.parsingFunction = ParsingFunction.GoToEof;
                    return true;

                case ParsingFunction.GoToEof:
                    this.OnEof();
                    return false;

                case ParsingFunction.PartialTextValue:
                    this.SkipPartialTextValue();
                    goto Label_0000;

                case ParsingFunction.InReadAttributeValue:
                    this.FinishAttributeValueIterator();
                    this.curNode = this.nodes[this.index];
                    goto Label_0000;

                case ParsingFunction.InReadValueChunk:
                    this.FinishReadValueChunk();
                    goto Label_0000;

                case ParsingFunction.InReadContentAsBinary:
                    this.FinishReadContentAsBinary();
                    goto Label_0000;

                case ParsingFunction.InReadElementContentAsBinary:
                    this.FinishReadElementContentAsBinary();
                    goto Label_0000;

                default:
                    goto Label_0000;
            }
            this.readState = System.Xml.ReadState.Interactive;
            this.parsingFunction = this.nextParsingFunction;
            if (this.ParseXmlDeclaration(false))
            {
                this.reportedEncoding = this.ps.encoding;
                return true;
            }
            this.reportedEncoding = this.ps.encoding;
            goto Label_0000;
        }

        public override bool ReadAttributeValue()
        {
            if (this.parsingFunction != ParsingFunction.InReadAttributeValue)
            {
                if (this.curNode.type != XmlNodeType.Attribute)
                {
                    return false;
                }
                if ((this.readState != System.Xml.ReadState.Interactive) || (this.curAttrIndex < 0))
                {
                    return false;
                }
                if (this.parsingFunction == ParsingFunction.InReadValueChunk)
                {
                    this.FinishReadValueChunk();
                }
                if (this.parsingFunction == ParsingFunction.InReadContentAsBinary)
                {
                    this.FinishReadContentAsBinary();
                }
                if ((this.curNode.nextAttrValueChunk == null) || (this.entityHandling == System.Xml.EntityHandling.ExpandEntities))
                {
                    NodeData data = this.AddNode((this.index + this.attrCount) + 1, this.curNode.depth + 1);
                    data.SetValueNode(XmlNodeType.Text, this.curNode.StringValue);
                    data.lineInfo = this.curNode.lineInfo2;
                    data.depth = this.curNode.depth + 1;
                    this.curNode = data;
                    data.nextAttrValueChunk = null;
                }
                else
                {
                    this.curNode = this.curNode.nextAttrValueChunk;
                    this.AddNode((this.index + this.attrCount) + 1, this.index + 2);
                    this.nodes[(this.index + this.attrCount) + 1] = this.curNode;
                    this.fullAttrCleanup = true;
                }
                this.nextParsingFunction = this.parsingFunction;
                this.parsingFunction = ParsingFunction.InReadAttributeValue;
                this.attributeValueBaseEntityId = this.ps.entityId;
                return true;
            }
            if (this.ps.entityId != this.attributeValueBaseEntityId)
            {
                return this.ParseAttributeValueChunk();
            }
            if (this.curNode.nextAttrValueChunk != null)
            {
                this.curNode = this.curNode.nextAttrValueChunk;
                this.nodes[(this.index + this.attrCount) + 1] = this.curNode;
                return true;
            }
            return false;
        }

        internal int ReadBase64(byte[] array, int offset, int len)
        {
            if (this.parsingFunction == ParsingFunction.InIncrementalRead)
            {
                if (this.incReadDecoder != this.base64Decoder)
                {
                    this.InitBase64Decoder();
                }
                return this.IncrementalRead(array, offset, len);
            }
            if (this.curNode.type != XmlNodeType.Element)
            {
                return 0;
            }
            if (this.curNode.IsEmptyElement)
            {
                this.outerReader.Read();
                return 0;
            }
            if (this.base64Decoder == null)
            {
                this.base64Decoder = new Base64Decoder();
            }
            this.InitIncrementalRead(this.base64Decoder);
            return this.IncrementalRead(array, offset, len);
        }

        internal int ReadBinHex(byte[] array, int offset, int len)
        {
            if (this.parsingFunction == ParsingFunction.InIncrementalRead)
            {
                if (this.incReadDecoder != this.binHexDecoder)
                {
                    this.InitBinHexDecoder();
                }
                return this.IncrementalRead(array, offset, len);
            }
            if (this.curNode.type != XmlNodeType.Element)
            {
                return 0;
            }
            if (this.curNode.IsEmptyElement)
            {
                this.outerReader.Read();
                return 0;
            }
            if (this.binHexDecoder == null)
            {
                this.binHexDecoder = new BinHexDecoder();
            }
            this.InitIncrementalRead(this.binHexDecoder);
            return this.IncrementalRead(array, offset, len);
        }

        internal int ReadChars(char[] buffer, int index, int count)
        {
            if (this.parsingFunction == ParsingFunction.InIncrementalRead)
            {
                if (this.incReadDecoder != this.readCharsDecoder)
                {
                    if (this.readCharsDecoder == null)
                    {
                        this.readCharsDecoder = new IncrementalReadCharsDecoder();
                    }
                    this.readCharsDecoder.Reset();
                    this.incReadDecoder = this.readCharsDecoder;
                }
                return this.IncrementalRead(buffer, index, count);
            }
            if (this.curNode.type != XmlNodeType.Element)
            {
                return 0;
            }
            if (this.curNode.IsEmptyElement)
            {
                this.outerReader.Read();
                return 0;
            }
            if (this.readCharsDecoder == null)
            {
                this.readCharsDecoder = new IncrementalReadCharsDecoder();
            }
            this.InitIncrementalRead(this.readCharsDecoder);
            return this.IncrementalRead(buffer, index, count);
        }

        public override int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (this.parsingFunction == ParsingFunction.InReadContentAsBinary)
            {
                if (this.incReadDecoder == this.base64Decoder)
                {
                    return this.ReadContentAsBinary(buffer, index, count);
                }
            }
            else
            {
                if (this.readState != System.Xml.ReadState.Interactive)
                {
                    return 0;
                }
                if (this.parsingFunction == ParsingFunction.InReadElementContentAsBinary)
                {
                    throw new InvalidOperationException(Res.GetString("Xml_MixingBinaryContentMethods"));
                }
                if (!XmlReader.CanReadContentAs(this.curNode.type))
                {
                    throw base.CreateReadContentAsException("ReadContentAsBase64");
                }
                if (!this.InitReadContentAsBinary())
                {
                    return 0;
                }
            }
            this.InitBase64Decoder();
            return this.ReadContentAsBinary(buffer, index, count);
        }

        private int ReadContentAsBinary(byte[] buffer, int index, int count)
        {
            if (this.incReadState == IncrementalReadState.ReadContentAsBinary_End)
            {
                return 0;
            }
            this.incReadDecoder.SetNextOutputBuffer(buffer, index, count);
            while (true)
            {
                int num = 0;
                try
                {
                    num = this.curNode.CopyToBinary(this.incReadDecoder, this.readValueOffset);
                }
                catch (XmlException exception)
                {
                    this.curNode.AdjustLineInfo(this.readValueOffset, this.ps.eolNormalized, ref this.incReadLineInfo);
                    this.ReThrow(exception, this.incReadLineInfo.lineNo, this.incReadLineInfo.linePos);
                }
                this.readValueOffset += num;
                if (this.incReadDecoder.IsFull)
                {
                    return this.incReadDecoder.DecodedCount;
                }
                if (this.incReadState == IncrementalReadState.ReadContentAsBinary_OnPartialValue)
                {
                    this.curNode.SetValue(string.Empty);
                    bool flag = false;
                    int startPos = 0;
                    int endPos = 0;
                    while (!this.incReadDecoder.IsFull && !flag)
                    {
                        int outOrChars = 0;
                        this.incReadLineInfo.Set(this.ps.LineNo, this.ps.LinePos);
                        flag = this.ParseText(out startPos, out endPos, ref outOrChars);
                        try
                        {
                            num = this.incReadDecoder.Decode(this.ps.chars, startPos, endPos - startPos);
                        }
                        catch (XmlException exception2)
                        {
                            this.ReThrow(exception2, this.incReadLineInfo.lineNo, this.incReadLineInfo.linePos);
                        }
                        startPos += num;
                    }
                    this.incReadState = flag ? IncrementalReadState.ReadContentAsBinary_OnCachedValue : IncrementalReadState.ReadContentAsBinary_OnPartialValue;
                    this.readValueOffset = 0;
                    if (this.incReadDecoder.IsFull)
                    {
                        this.curNode.SetValue(this.ps.chars, startPos, endPos - startPos);
                        AdjustLineInfo(this.ps.chars, startPos - num, startPos, this.ps.eolNormalized, ref this.incReadLineInfo);
                        this.curNode.SetLineInfo(this.incReadLineInfo.lineNo, this.incReadLineInfo.linePos);
                        return this.incReadDecoder.DecodedCount;
                    }
                }
                ParsingFunction parsingFunction = this.parsingFunction;
                this.parsingFunction = this.nextParsingFunction;
                this.nextParsingFunction = this.nextNextParsingFunction;
                if (!this.MoveToNextContentNode(true))
                {
                    this.SetupReadContentAsBinaryState(parsingFunction);
                    this.incReadState = IncrementalReadState.ReadContentAsBinary_End;
                    return this.incReadDecoder.DecodedCount;
                }
                this.SetupReadContentAsBinaryState(parsingFunction);
                this.incReadLineInfo.Set(this.curNode.LineNo, this.curNode.LinePos);
            }
        }

        public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (this.parsingFunction == ParsingFunction.InReadContentAsBinary)
            {
                if (this.incReadDecoder == this.binHexDecoder)
                {
                    return this.ReadContentAsBinary(buffer, index, count);
                }
            }
            else
            {
                if (this.readState != System.Xml.ReadState.Interactive)
                {
                    return 0;
                }
                if (this.parsingFunction == ParsingFunction.InReadElementContentAsBinary)
                {
                    throw new InvalidOperationException(Res.GetString("Xml_MixingBinaryContentMethods"));
                }
                if (!XmlReader.CanReadContentAs(this.curNode.type))
                {
                    throw base.CreateReadContentAsException("ReadContentAsBinHex");
                }
                if (!this.InitReadContentAsBinary())
                {
                    return 0;
                }
            }
            this.InitBinHexDecoder();
            return this.ReadContentAsBinary(buffer, index, count);
        }

        private int ReadData()
        {
            int chars;
            if (this.ps.isEof)
            {
                return 0;
            }
            if (this.ps.appendMode)
            {
                if (this.ps.charsUsed == (this.ps.chars.Length - 1))
                {
                    for (int i = 0; i < this.attrCount; i++)
                    {
                        this.nodes[(this.index + i) + 1].OnBufferInvalidated();
                    }
                    char[] dst = new char[this.ps.chars.Length * 2];
                    BlockCopyChars(this.ps.chars, 0, dst, 0, this.ps.chars.Length);
                    this.ps.chars = dst;
                }
                if (((this.ps.stream != null) && ((this.ps.bytesUsed - this.ps.bytePos) < 6)) && ((this.ps.bytes.Length - this.ps.bytesUsed) < 6))
                {
                    byte[] buffer = new byte[this.ps.bytes.Length * 2];
                    BlockCopy(this.ps.bytes, 0, buffer, 0, this.ps.bytesUsed);
                    this.ps.bytes = buffer;
                }
                chars = (this.ps.chars.Length - this.ps.charsUsed) - 1;
                if (chars > 80)
                {
                    chars = 80;
                }
            }
            else
            {
                int length = this.ps.chars.Length;
                if ((length - this.ps.charsUsed) <= (length / 2))
                {
                    for (int j = 0; j < this.attrCount; j++)
                    {
                        this.nodes[(this.index + j) + 1].OnBufferInvalidated();
                    }
                    int count = this.ps.charsUsed - this.ps.charPos;
                    if (count < (length - 1))
                    {
                        this.ps.lineStartPos -= this.ps.charPos;
                        if (count > 0)
                        {
                            BlockCopyChars(this.ps.chars, this.ps.charPos, this.ps.chars, 0, count);
                        }
                        this.ps.charPos = 0;
                        this.ps.charsUsed = count;
                    }
                    else
                    {
                        char[] chArray2 = new char[this.ps.chars.Length * 2];
                        BlockCopyChars(this.ps.chars, 0, chArray2, 0, this.ps.chars.Length);
                        this.ps.chars = chArray2;
                    }
                }
                if (this.ps.stream != null)
                {
                    int num6 = this.ps.bytesUsed - this.ps.bytePos;
                    if (num6 <= 0x80)
                    {
                        if (num6 == 0)
                        {
                            this.ps.bytesUsed = 0;
                        }
                        else
                        {
                            BlockCopy(this.ps.bytes, this.ps.bytePos, this.ps.bytes, 0, num6);
                            this.ps.bytesUsed = num6;
                        }
                        this.ps.bytePos = 0;
                    }
                }
                chars = (this.ps.chars.Length - this.ps.charsUsed) - 1;
            }
            if (this.ps.stream != null)
            {
                if ((!this.ps.isStreamEof && (this.ps.bytePos == this.ps.bytesUsed)) && ((this.ps.bytes.Length - this.ps.bytesUsed) > 0))
                {
                    int num7 = this.ps.stream.Read(this.ps.bytes, this.ps.bytesUsed, this.ps.bytes.Length - this.ps.bytesUsed);
                    if (num7 == 0)
                    {
                        this.ps.isStreamEof = true;
                    }
                    this.ps.bytesUsed += num7;
                }
                int bytePos = this.ps.bytePos;
                chars = this.GetChars(chars);
                if ((chars == 0) && (this.ps.bytePos != bytePos))
                {
                    return this.ReadData();
                }
            }
            else if (this.ps.textReader != null)
            {
                chars = this.ps.textReader.Read(this.ps.chars, this.ps.charsUsed, (this.ps.chars.Length - this.ps.charsUsed) - 1);
                this.ps.charsUsed += chars;
            }
            else
            {
                chars = 0;
            }
            this.RegisterConsumedCharacters((long) chars, this.InEntity);
            if (chars == 0)
            {
                this.ps.isEof = true;
            }
            this.ps.chars[this.ps.charsUsed] = '\0';
            return chars;
        }

        private bool ReadDataInName(ref int pos)
        {
            int num = pos - this.ps.charPos;
            bool flag = this.ReadData() != 0;
            pos = this.ps.charPos + num;
            return flag;
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (this.parsingFunction == ParsingFunction.InReadElementContentAsBinary)
            {
                if (this.incReadDecoder == this.base64Decoder)
                {
                    return this.ReadElementContentAsBinary(buffer, index, count);
                }
            }
            else
            {
                if (this.readState != System.Xml.ReadState.Interactive)
                {
                    return 0;
                }
                if (this.parsingFunction == ParsingFunction.InReadContentAsBinary)
                {
                    throw new InvalidOperationException(Res.GetString("Xml_MixingBinaryContentMethods"));
                }
                if (this.curNode.type != XmlNodeType.Element)
                {
                    throw base.CreateReadElementContentAsException("ReadElementContentAsBinHex");
                }
                if (!this.InitReadElementContentAsBinary())
                {
                    return 0;
                }
            }
            this.InitBase64Decoder();
            return this.ReadElementContentAsBinary(buffer, index, count);
        }

        private int ReadElementContentAsBinary(byte[] buffer, int index, int count)
        {
            if (count != 0)
            {
                int num = this.ReadContentAsBinary(buffer, index, count);
                if (num > 0)
                {
                    return num;
                }
                if (this.curNode.type != XmlNodeType.EndElement)
                {
                    throw new XmlException("Xml_InvalidNodeType", this.curNode.type.ToString(), this);
                }
                this.parsingFunction = this.nextParsingFunction;
                this.nextParsingFunction = this.nextNextParsingFunction;
                this.outerReader.Read();
            }
            return 0;
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (this.parsingFunction == ParsingFunction.InReadElementContentAsBinary)
            {
                if (this.incReadDecoder == this.binHexDecoder)
                {
                    return this.ReadElementContentAsBinary(buffer, index, count);
                }
            }
            else
            {
                if (this.readState != System.Xml.ReadState.Interactive)
                {
                    return 0;
                }
                if (this.parsingFunction == ParsingFunction.InReadContentAsBinary)
                {
                    throw new InvalidOperationException(Res.GetString("Xml_MixingBinaryContentMethods"));
                }
                if (this.curNode.type != XmlNodeType.Element)
                {
                    throw base.CreateReadElementContentAsException("ReadElementContentAsBinHex");
                }
                if (!this.InitReadElementContentAsBinary())
                {
                    return 0;
                }
            }
            this.InitBinHexDecoder();
            return this.ReadElementContentAsBinary(buffer, index, count);
        }

        public override string ReadString()
        {
            this.MoveOffEntityReference();
            return base.ReadString();
        }

        public override int ReadValueChunk(char[] buffer, int index, int count)
        {
            if (!XmlReader.HasValueInternal(this.curNode.type))
            {
                throw new InvalidOperationException(Res.GetString("Xml_InvalidReadValueChunk", new object[] { this.curNode.type }));
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (this.parsingFunction != ParsingFunction.InReadValueChunk)
            {
                if (this.readState != System.Xml.ReadState.Interactive)
                {
                    return 0;
                }
                if (this.parsingFunction == ParsingFunction.PartialTextValue)
                {
                    this.incReadState = IncrementalReadState.ReadValueChunk_OnPartialValue;
                }
                else
                {
                    this.incReadState = IncrementalReadState.ReadValueChunk_OnCachedValue;
                    this.nextNextParsingFunction = this.nextParsingFunction;
                    this.nextParsingFunction = this.parsingFunction;
                }
                this.parsingFunction = ParsingFunction.InReadValueChunk;
                this.readValueOffset = 0;
            }
            if (count == 0)
            {
                return 0;
            }
            int num = 0;
            int num2 = this.curNode.CopyTo(this.readValueOffset, buffer, index + num, count - num);
            num += num2;
            this.readValueOffset += num2;
            if (num == count)
            {
                char ch = buffer[(index + count) - 1];
                if (XmlCharType.IsHighSurrogate(ch))
                {
                    num--;
                    this.readValueOffset--;
                    if (num == 0)
                    {
                        this.Throw("Xml_NotEnoughSpaceForSurrogatePair");
                    }
                }
                return num;
            }
            if (this.incReadState == IncrementalReadState.ReadValueChunk_OnPartialValue)
            {
                this.curNode.SetValue(string.Empty);
                bool flag = false;
                int startPos = 0;
                int endPos = 0;
                while ((num < count) && !flag)
                {
                    int outOrChars = 0;
                    flag = this.ParseText(out startPos, out endPos, ref outOrChars);
                    int num6 = count - num;
                    if (num6 > (endPos - startPos))
                    {
                        num6 = endPos - startPos;
                    }
                    BlockCopyChars(this.ps.chars, startPos, buffer, index + num, num6);
                    num += num6;
                    startPos += num6;
                }
                this.incReadState = flag ? IncrementalReadState.ReadValueChunk_OnCachedValue : IncrementalReadState.ReadValueChunk_OnPartialValue;
                if (num == count)
                {
                    char ch2 = buffer[(index + count) - 1];
                    if (XmlCharType.IsHighSurrogate(ch2))
                    {
                        num--;
                        startPos--;
                        if (num == 0)
                        {
                            this.Throw("Xml_NotEnoughSpaceForSurrogatePair");
                        }
                    }
                }
                this.readValueOffset = 0;
                this.curNode.SetValue(this.ps.chars, startPos, endPos - startPos);
            }
            return num;
        }

        private void RegisterConsumedCharacters(long characters, bool inEntityReference)
        {
            if (this.maxCharactersInDocument > 0L)
            {
                long num = this.charactersInDocument + characters;
                if (num < this.charactersInDocument)
                {
                    this.ThrowWithoutLineInfo("Xml_LimitExceeded", "MaxCharactersInDocument");
                }
                else
                {
                    this.charactersInDocument = num;
                }
                if (this.charactersInDocument > this.maxCharactersInDocument)
                {
                    this.ThrowWithoutLineInfo("Xml_LimitExceeded", "MaxCharactersInDocument");
                }
            }
            if ((this.maxCharactersFromEntities > 0L) && inEntityReference)
            {
                long num2 = this.charactersFromEntities + characters;
                if (num2 < this.charactersFromEntities)
                {
                    this.ThrowWithoutLineInfo("Xml_LimitExceeded", "MaxCharactersFromEntities");
                }
                else
                {
                    this.charactersFromEntities = num2;
                }
                if (this.charactersFromEntities > this.maxCharactersFromEntities)
                {
                    this.ThrowWithoutLineInfo("Xml_LimitExceeded", "MaxCharactersFromEntities");
                }
            }
        }

        private void RegisterEntity(IDtdEntityInfo entity)
        {
            if ((this.currentEntities != null) && this.currentEntities.ContainsKey(entity))
            {
                this.Throw(entity.IsParameterEntity ? "Xml_RecursiveParEntity" : "Xml_RecursiveGenEntity", entity.Name, this.parsingStatesStack[this.parsingStatesStackTop].LineNo, this.parsingStatesStack[this.parsingStatesStackTop].LinePos);
            }
            this.ps.entity = entity;
            this.ps.entityId = this.nextEntityId++;
            if (entity != null)
            {
                if (this.currentEntities == null)
                {
                    this.currentEntities = new Dictionary<IDtdEntityInfo, IDtdEntityInfo>();
                }
                this.currentEntities.Add(entity, entity);
            }
        }

        private void ResetAttributes()
        {
            if (this.fullAttrCleanup)
            {
                this.FullAttributeCleanup();
            }
            this.curAttrIndex = -1;
            this.attrCount = 0;
            this.attrHashtable = 0;
            this.attrDuplWalkCount = 0;
        }

        internal void ResetState()
        {
            if (this.fragment)
            {
                this.Throw(new InvalidOperationException(Res.GetString("Xml_InvalidResetStateCall")));
            }
            if (this.readState != System.Xml.ReadState.Initial)
            {
                this.ResetAttributes();
                while (this.namespaceManager.PopScope())
                {
                }
                while (this.InEntity)
                {
                    this.HandleEntityEnd(true);
                }
                this.readState = System.Xml.ReadState.Initial;
                this.parsingFunction = ParsingFunction.SwitchToInteractiveXmlDecl;
                this.nextParsingFunction = ParsingFunction.DocumentContent;
                this.curNode = this.nodes[0];
                this.curNode.Clear(XmlNodeType.None);
                this.curNode.SetLineInfo(0, 0);
                this.index = 0;
                this.rootElementParsed = false;
                this.charactersInDocument = 0L;
                this.charactersFromEntities = 0L;
                this.afterResetState = true;
            }
        }

        public override void ResolveEntity()
        {
            if (this.curNode.type != XmlNodeType.EntityReference)
            {
                throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
            }
            if ((this.parsingFunction != ParsingFunction.InReadAttributeValue) && (this.parsingFunction != ParsingFunction.FragmentAttribute))
            {
                switch (this.HandleGeneralEntityReference(this.curNode.localName, false, true, this.curNode.LinePos))
                {
                    case EntityType.Expanded:
                    case EntityType.ExpandedInAttribute:
                        this.nextParsingFunction = this.parsingFunction;
                        if (((this.ps.charsUsed - this.ps.charPos) != 0) || this.ps.entity.IsExternal)
                        {
                            this.parsingFunction = ParsingFunction.AfterResolveEntityInContent;
                        }
                        else
                        {
                            this.parsingFunction = ParsingFunction.AfterResolveEmptyEntityInContent;
                        }
                        goto Label_0164;

                    case EntityType.FakeExpanded:
                        this.nextParsingFunction = this.parsingFunction;
                        this.parsingFunction = ParsingFunction.AfterResolveEmptyEntityInContent;
                        goto Label_0164;
                }
                throw new XmlException("Xml_InternalError", string.Empty);
            }
            switch (this.HandleGeneralEntityReference(this.curNode.localName, true, true, this.curNode.LinePos))
            {
                case EntityType.Expanded:
                case EntityType.ExpandedInAttribute:
                    if ((this.ps.charsUsed - this.ps.charPos) == 0)
                    {
                        this.emptyEntityInAttributeResolved = true;
                    }
                    break;

                case EntityType.FakeExpanded:
                    this.emptyEntityInAttributeResolved = true;
                    break;

                default:
                    throw new XmlException("Xml_InternalError", string.Empty);
            }
        Label_0164:
            this.ps.entityResolvedManually = true;
            this.index++;
        }

        private void ReThrow(Exception e, int lineNo, int linePos)
        {
            this.Throw(new XmlException(e.Message, null, lineNo, linePos, this.ps.baseUriStr));
        }

        private void SendValidationEvent(XmlSeverityType severity, XmlSchemaException exception)
        {
            if (this.validationEventHandling != null)
            {
                this.validationEventHandling.SendEvent(exception, severity);
            }
        }

        private void SendValidationEvent(XmlSeverityType severity, string code, string arg, int lineNo, int linePos)
        {
            this.SendValidationEvent(severity, new XmlSchemaException(code, arg, this.ps.baseUriStr, lineNo, linePos));
        }

        internal void SetDtdInfo(IDtdInfo newDtdInfo)
        {
            this.dtdInfo = newDtdInfo;
            if (((this.dtdInfo != null) && (this.validatingReaderCompatFlag || !this.v1Compat)) && (this.dtdInfo.HasDefaultAttributes || this.dtdInfo.HasNonCDataAttributes))
            {
                this.addDefaultAttributesAndNormalize = true;
            }
        }

        private void SetErrorState()
        {
            this.parsingFunction = ParsingFunction.Error;
            this.readState = System.Xml.ReadState.Error;
        }

        private void SetupEncoding(System.Text.Encoding encoding)
        {
            if (encoding == null)
            {
                this.ps.encoding = System.Text.Encoding.UTF8;
                this.ps.decoder = new SafeAsciiDecoder();
            }
            else
            {
                this.ps.encoding = encoding;
                switch (this.ps.encoding.WebName)
                {
                    case "utf-16":
                        this.ps.decoder = new UTF16Decoder(false);
                        return;

                    case "utf-16BE":
                        this.ps.decoder = new UTF16Decoder(true);
                        return;
                }
                this.ps.decoder = encoding.GetDecoder();
            }
        }

        private void SetupEndEntityNodeInAttribute()
        {
            this.curNode = this.nodes[(this.index + this.attrCount) + 1];
            this.curNode.lineInfo.linePos += this.curNode.localName.Length;
            this.curNode.type = XmlNodeType.EndEntity;
        }

        private void SetupEndEntityNodeInContent()
        {
            this.reportedEncoding = this.ps.encoding;
            this.reportedBaseUri = this.ps.baseUriStr;
            this.curNode = this.nodes[this.index];
            this.curNode.SetNamedNode(XmlNodeType.EndEntity, this.lastEntity.Name);
            this.curNode.lineInfo.Set(this.ps.lineNo, this.ps.LinePos - 1);
            if ((this.index == 0) && (this.parsingFunction == ParsingFunction.ElementContent))
            {
                this.parsingFunction = ParsingFunction.DocumentContent;
            }
        }

        private void SetupFromParserContext(XmlParserContext context, XmlReaderSettings settings)
        {
            XmlNameTable nameTable = settings.NameTable;
            this.nameTableFromSettings = nameTable != null;
            if (context.NamespaceManager != null)
            {
                if ((nameTable != null) && (nameTable != context.NamespaceManager.NameTable))
                {
                    throw new XmlException("Xml_NametableMismatch");
                }
                this.namespaceManager = context.NamespaceManager;
                this.xmlContext.defaultNamespace = this.namespaceManager.LookupNamespace(string.Empty);
                nameTable = this.namespaceManager.NameTable;
            }
            else if (context.NameTable != null)
            {
                if ((nameTable != null) && (nameTable != context.NameTable))
                {
                    throw new XmlException("Xml_NametableMismatch", string.Empty);
                }
                nameTable = context.NameTable;
            }
            else if (nameTable == null)
            {
                nameTable = new System.Xml.NameTable();
            }
            this.nameTable = nameTable;
            if (this.namespaceManager == null)
            {
                this.namespaceManager = new XmlNamespaceManager(nameTable);
            }
            this.xmlContext.xmlSpace = context.XmlSpace;
            this.xmlContext.xmlLang = context.XmlLang;
        }

        private void SetupReadContentAsBinaryState(ParsingFunction inReadBinaryFunction)
        {
            if (this.parsingFunction == ParsingFunction.PartialTextValue)
            {
                this.incReadState = IncrementalReadState.ReadContentAsBinary_OnPartialValue;
            }
            else
            {
                this.incReadState = IncrementalReadState.ReadContentAsBinary_OnCachedValue;
                this.nextNextParsingFunction = this.nextParsingFunction;
                this.nextParsingFunction = this.parsingFunction;
            }
            this.readValueOffset = 0;
            this.parsingFunction = inReadBinaryFunction;
        }

        private void ShiftBuffer(int sourcePos, int destPos, int count)
        {
            BlockCopyChars(this.ps.chars, sourcePos, this.ps.chars, destPos, count);
        }

        public override void Skip()
        {
            if (this.readState != System.Xml.ReadState.Interactive)
            {
                return;
            }
            if (this.InAttributeValueIterator)
            {
                this.FinishAttributeValueIterator();
                this.curNode = this.nodes[this.index];
            }
            else
            {
                switch (this.parsingFunction)
                {
                    case ParsingFunction.PartialTextValue:
                        this.SkipPartialTextValue();
                        break;

                    case ParsingFunction.InReadValueChunk:
                        this.FinishReadValueChunk();
                        break;

                    case ParsingFunction.InReadContentAsBinary:
                        this.FinishReadContentAsBinary();
                        break;

                    case ParsingFunction.InReadElementContentAsBinary:
                        this.FinishReadElementContentAsBinary();
                        break;

                    case ParsingFunction.InIncrementalRead:
                        this.FinishIncrementalRead();
                        break;
                }
            }
            switch (this.curNode.type)
            {
                case XmlNodeType.Element:
                    break;

                case XmlNodeType.Attribute:
                    this.outerReader.MoveToElement();
                    break;

                default:
                    goto Label_00E4;
            }
            if (!this.curNode.IsEmptyElement)
            {
                int index = this.index;
                this.parsingMode = ParsingMode.SkipContent;
                while (this.outerReader.Read() && (this.index > index))
                {
                }
                this.parsingMode = ParsingMode.Full;
            }
        Label_00E4:
            this.outerReader.Read();
        }

        private void SkipDtd()
        {
            int num;
            int num2 = this.ParseQName(out num);
            this.ps.charPos = num2;
            this.EatWhitespaces(null);
            if (this.ps.chars[this.ps.charPos] == 'P')
            {
                while ((this.ps.charsUsed - this.ps.charPos) < 6)
                {
                    if (this.ReadData() == 0)
                    {
                        this.Throw("Xml_UnexpectedEOF1");
                    }
                }
                if (!XmlConvert.StrEqual(this.ps.chars, this.ps.charPos, 6, "PUBLIC"))
                {
                    this.ThrowUnexpectedToken("PUBLIC");
                }
                this.ps.charPos += 6;
                if (this.EatWhitespaces(null) == 0)
                {
                    this.ThrowExpectingWhitespace(this.ps.charPos);
                }
                this.SkipPublicOrSystemIdLiteral();
                if (this.EatWhitespaces(null) == 0)
                {
                    this.ThrowExpectingWhitespace(this.ps.charPos);
                }
                this.SkipPublicOrSystemIdLiteral();
                this.EatWhitespaces(null);
            }
            else if (this.ps.chars[this.ps.charPos] == 'S')
            {
                while ((this.ps.charsUsed - this.ps.charPos) < 6)
                {
                    if (this.ReadData() == 0)
                    {
                        this.Throw("Xml_UnexpectedEOF1");
                    }
                }
                if (!XmlConvert.StrEqual(this.ps.chars, this.ps.charPos, 6, "SYSTEM"))
                {
                    this.ThrowUnexpectedToken("SYSTEM");
                }
                this.ps.charPos += 6;
                if (this.EatWhitespaces(null) == 0)
                {
                    this.ThrowExpectingWhitespace(this.ps.charPos);
                }
                this.SkipPublicOrSystemIdLiteral();
                this.EatWhitespaces(null);
            }
            else if ((this.ps.chars[this.ps.charPos] != '[') && (this.ps.chars[this.ps.charPos] != '>'))
            {
                this.Throw("Xml_ExpectExternalOrClose");
            }
            if (this.ps.chars[this.ps.charPos] == '[')
            {
                this.ps.charPos++;
                this.SkipUntil(']', true);
                this.EatWhitespaces(null);
                if (this.ps.chars[this.ps.charPos] != '>')
                {
                    this.ThrowUnexpectedToken(">");
                }
            }
            else if (this.ps.chars[this.ps.charPos] == '>')
            {
                this.curNode.SetValue(string.Empty);
            }
            else
            {
                this.Throw("Xml_ExpectSubOrClose");
            }
            this.ps.charPos++;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SkipPartialTextValue()
        {
            int num;
            int num2;
            int outOrChars = 0;
            this.parsingFunction = this.nextParsingFunction;
            while (!this.ParseText(out num, out num2, ref outOrChars))
            {
            }
        }

        private void SkipPublicOrSystemIdLiteral()
        {
            char stopChar = this.ps.chars[this.ps.charPos];
            if ((stopChar != '"') && (stopChar != '\''))
            {
                this.ThrowUnexpectedToken("\"", "'");
            }
            this.ps.charPos++;
            this.SkipUntil(stopChar, false);
        }

        private unsafe void SkipUntil(char stopChar, bool recognizeLiterals)
        {
            char ch2;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            char ch = '"';
            char[] chars = this.ps.chars;
            int charPos = this.ps.charPos;
        Label_002B:
            while ((((this.xmlCharType.charProperties[ch2 = chars[charPos]] & 0x80) != 0) && (chars[charPos] != stopChar)) && ((ch2 != '-') && (ch2 != '?')))
            {
                charPos++;
            }
            if ((ch2 == stopChar) && !flag)
            {
                this.ps.charPos = charPos + 1;
                return;
            }
            this.ps.charPos = charPos;
            switch (ch2)
            {
                case '<':
                    if (chars[charPos + 1] != '?')
                    {
                        if (chars[charPos + 1] == '!')
                        {
                            if (((charPos + 3) >= this.ps.charsUsed) && !this.ps.isEof)
                            {
                                goto Label_032D;
                            }
                            if ((((chars[charPos + 2] != '-') || (chars[charPos + 3] != '-')) || (!recognizeLiterals || flag)) || flag3)
                            {
                                goto Label_01F7;
                            }
                            flag2 = true;
                            charPos += 4;
                            goto Label_002B;
                        }
                        if (((charPos + 1) >= this.ps.charsUsed) && !this.ps.isEof)
                        {
                            goto Label_032D;
                        }
                        goto Label_01F7;
                    }
                    if ((!recognizeLiterals || flag) || flag2)
                    {
                        goto Label_01F7;
                    }
                    flag3 = true;
                    charPos += 2;
                    goto Label_002B;

                case '>':
                case ']':
                case '\t':
                case '&':
                    charPos++;
                    goto Label_002B;

                case '?':
                    if (flag3)
                    {
                        if (((charPos + 1) >= this.ps.charsUsed) && !this.ps.isEof)
                        {
                            goto Label_032D;
                        }
                        if (chars[charPos + 1] == '>')
                        {
                            flag3 = false;
                            charPos++;
                            goto Label_002B;
                        }
                    }
                    charPos++;
                    goto Label_002B;

                case '-':
                    if (flag2)
                    {
                        if (((charPos + 2) >= this.ps.charsUsed) && !this.ps.isEof)
                        {
                            goto Label_032D;
                        }
                        if ((chars[charPos + 1] == '-') && (chars[charPos + 2] == '>'))
                        {
                            flag2 = false;
                            charPos += 2;
                            goto Label_002B;
                        }
                    }
                    charPos++;
                    goto Label_002B;

                case '\n':
                    charPos++;
                    this.OnNewLine(charPos);
                    goto Label_002B;

                case '\r':
                    if (chars[charPos + 1] != '\n')
                    {
                        if (((charPos + 1) >= this.ps.charsUsed) && !this.ps.isEof)
                        {
                            goto Label_032D;
                        }
                        charPos++;
                        break;
                    }
                    charPos += 2;
                    break;

                case '"':
                case '\'':
                    if (flag)
                    {
                        if (ch == ch2)
                        {
                            flag = false;
                        }
                    }
                    else if ((recognizeLiterals && !flag2) && !flag3)
                    {
                        flag = true;
                        ch = ch2;
                    }
                    charPos++;
                    goto Label_002B;

                default:
                    if (charPos != this.ps.charsUsed)
                    {
                        char ch3 = chars[charPos];
                        if (XmlCharType.IsHighSurrogate(ch3))
                        {
                            if ((charPos + 1) == this.ps.charsUsed)
                            {
                                goto Label_032D;
                            }
                            charPos++;
                            if (XmlCharType.IsLowSurrogate(chars[charPos]))
                            {
                                charPos++;
                                goto Label_002B;
                            }
                        }
                        this.ThrowInvalidChar(chars, this.ps.charsUsed, charPos);
                    }
                    goto Label_032D;
            }
            this.OnNewLine(charPos);
            goto Label_002B;
        Label_01F7:
            charPos++;
            goto Label_002B;
        Label_032D:
            if (this.ReadData() == 0)
            {
                if ((this.ps.charsUsed - this.ps.charPos) > 0)
                {
                    if (this.ps.chars[this.ps.charPos] != '\r')
                    {
                        this.Throw("Xml_UnexpectedEOF1");
                    }
                }
                else
                {
                    this.Throw("Xml_UnexpectedEOF1");
                }
            }
            chars = this.ps.chars;
            charPos = this.ps.charPos;
            goto Label_002B;
        }

        internal static string StripSpaces(string value)
        {
            int length = value.Length;
            if (length <= 0)
            {
                return string.Empty;
            }
            int startIndex = 0;
            StringBuilder builder = null;
            while (value[startIndex] == ' ')
            {
                startIndex++;
                if (startIndex == length)
                {
                    return " ";
                }
            }
            int num3 = startIndex;
            while (num3 < length)
            {
                if (value[num3] == ' ')
                {
                    int num4 = num3 + 1;
                    while ((num4 < length) && (value[num4] == ' '))
                    {
                        num4++;
                    }
                    if (num4 == length)
                    {
                        if (builder == null)
                        {
                            return value.Substring(startIndex, num3 - startIndex);
                        }
                        builder.Append(value, startIndex, num3 - startIndex);
                        return builder.ToString();
                    }
                    if (num4 > (num3 + 1))
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder(length);
                        }
                        builder.Append(value, startIndex, (num3 - startIndex) + 1);
                        startIndex = num4;
                        num3 = num4 - 1;
                    }
                }
                num3++;
            }
            if (builder == null)
            {
                if (startIndex != 0)
                {
                    return value.Substring(startIndex, length - startIndex);
                }
                return value;
            }
            if (num3 > startIndex)
            {
                builder.Append(value, startIndex, num3 - startIndex);
            }
            return builder.ToString();
        }

        internal static void StripSpaces(char[] value, int index, ref int len)
        {
            if (len > 0)
            {
                int num = index;
                int num2 = index + len;
                while (value[num] == ' ')
                {
                    num++;
                    if (num == num2)
                    {
                        len = 1;
                        return;
                    }
                }
                int num3 = num - index;
                for (int i = num; i < num2; i++)
                {
                    char ch = value[i];
                    if (ch == ' ')
                    {
                        int num5 = i + 1;
                        while ((num5 < num2) && (value[num5] == ' '))
                        {
                            num5++;
                        }
                        if (num5 == num2)
                        {
                            num3 += num5 - i;
                            break;
                        }
                        if (num5 > (i + 1))
                        {
                            num3 += (num5 - i) - 1;
                            i = num5 - 1;
                        }
                    }
                    value[i - num3] = ch;
                }
                len -= num3;
            }
        }

        private void SwitchEncoding(System.Text.Encoding newEncoding)
        {
            if (((newEncoding.WebName != this.ps.encoding.WebName) || (this.ps.decoder is SafeAsciiDecoder)) && !this.afterResetState)
            {
                this.UnDecodeChars();
                this.ps.appendMode = false;
                this.SetupEncoding(newEncoding);
                this.ReadData();
            }
        }

        private void SwitchEncodingToUTF8()
        {
            this.SwitchEncoding(new UTF8Encoding(true, true));
        }

        IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
        {
            return this.GetNamespacesInScope(scope);
        }

        string IXmlNamespaceResolver.LookupNamespace(string prefix)
        {
            return this.LookupNamespace(prefix);
        }

        string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
        {
            return this.LookupPrefix(namespaceName);
        }

        private void Throw(Exception e)
        {
            this.SetErrorState();
            XmlException exception = e as XmlException;
            if (exception != null)
            {
                this.curNode.SetLineInfo(exception.LineNumber, exception.LinePosition);
            }
            throw e;
        }

        private void Throw(string res)
        {
            this.Throw(res, string.Empty);
        }

        private void Throw(int pos, string res)
        {
            this.ps.charPos = pos;
            this.Throw(res, string.Empty);
        }

        private void Throw(string res, string arg)
        {
            this.Throw(new XmlException(res, arg, this.ps.LineNo, this.ps.LinePos, this.ps.baseUriStr));
        }

        private void Throw(string res, string[] args)
        {
            this.Throw(new XmlException(res, args, this.ps.LineNo, this.ps.LinePos, this.ps.baseUriStr));
        }

        private void Throw(int pos, string res, string arg)
        {
            this.ps.charPos = pos;
            this.Throw(res, arg);
        }

        private void Throw(int pos, string res, string[] args)
        {
            this.ps.charPos = pos;
            this.Throw(res, args);
        }

        private void Throw(string res, int lineNo, int linePos)
        {
            this.Throw(new XmlException(res, string.Empty, lineNo, linePos, this.ps.baseUriStr));
        }

        private void Throw(string res, string arg, Exception innerException)
        {
            this.Throw(res, new string[] { arg }, innerException);
        }

        private void Throw(string res, string[] args, Exception innerException)
        {
            this.Throw(new XmlException(res, args, innerException, this.ps.LineNo, this.ps.LinePos, this.ps.baseUriStr));
        }

        private void Throw(string res, string arg, int lineNo, int linePos)
        {
            this.Throw(new XmlException(res, arg, lineNo, linePos, this.ps.baseUriStr));
        }

        private void ThrowExpectingWhitespace(int pos)
        {
            string arg = this.ParseUnexpectedToken(pos);
            if (arg == null)
            {
                this.Throw(pos, "Xml_UnexpectedEOF1");
            }
            else
            {
                this.Throw(pos, "Xml_ExpectingWhiteSpace", arg);
            }
        }

        private void ThrowInvalidChar(char[] data, int length, int invCharPos)
        {
            this.Throw(invCharPos, "Xml_InvalidCharacter", XmlException.BuildCharExceptionArgs(data, length, invCharPos));
        }

        private void ThrowTagMismatch(NodeData startTag)
        {
            if (startTag.type == XmlNodeType.Element)
            {
                int num;
                int num2 = this.ParseQName(out num);
                string[] args = new string[] { startTag.GetNameWPrefix(this.nameTable), startTag.lineInfo.lineNo.ToString(CultureInfo.InvariantCulture), startTag.lineInfo.linePos.ToString(CultureInfo.InvariantCulture), new string(this.ps.chars, this.ps.charPos, num2 - this.ps.charPos) };
                this.Throw("Xml_TagMismatchEx", args);
            }
            else
            {
                this.Throw("Xml_UnexpectedEndTag");
            }
        }

        private void ThrowUnclosedElements()
        {
            if ((this.index == 0) && (this.curNode.type != XmlNodeType.Element))
            {
                this.Throw(this.ps.charsUsed, "Xml_UnexpectedEOF1");
            }
            else
            {
                int index = (this.parsingFunction == ParsingFunction.InIncrementalRead) ? this.index : (this.index - 1);
                this.stringBuilder.Length = 0;
                while (index >= 0)
                {
                    NodeData data = this.nodes[index];
                    if (data.type == XmlNodeType.Element)
                    {
                        this.stringBuilder.Append(data.GetNameWPrefix(this.nameTable));
                        if (index > 0)
                        {
                            this.stringBuilder.Append(", ");
                        }
                        else
                        {
                            this.stringBuilder.Append(".");
                        }
                    }
                    index--;
                }
                this.Throw(this.ps.charsUsed, "Xml_UnexpectedEOFInElementContent", this.stringBuilder.ToString());
            }
        }

        private void ThrowUnexpectedToken(string expectedToken1)
        {
            this.ThrowUnexpectedToken(expectedToken1, null);
        }

        private void ThrowUnexpectedToken(int pos, string expectedToken)
        {
            this.ThrowUnexpectedToken(pos, expectedToken, null);
        }

        private void ThrowUnexpectedToken(string expectedToken1, string expectedToken2)
        {
            string str = this.ParseUnexpectedToken();
            if (str == null)
            {
                this.Throw("Xml_UnexpectedEOF1");
            }
            if (expectedToken2 != null)
            {
                this.Throw("Xml_UnexpectedTokens2", new string[] { str, expectedToken1, expectedToken2 });
            }
            else
            {
                this.Throw("Xml_UnexpectedTokenEx", new string[] { str, expectedToken1 });
            }
        }

        private void ThrowUnexpectedToken(int pos, string expectedToken1, string expectedToken2)
        {
            this.ps.charPos = pos;
            this.ThrowUnexpectedToken(expectedToken1, expectedToken2);
        }

        private void ThrowWithoutLineInfo(string res)
        {
            this.Throw(new XmlException(res, string.Empty, this.ps.baseUriStr));
        }

        private void ThrowWithoutLineInfo(string res, string arg)
        {
            this.Throw(new XmlException(res, arg, this.ps.baseUriStr));
        }

        private void ThrowWithoutLineInfo(string res, string[] args, Exception innerException)
        {
            this.Throw(new XmlException(res, args, innerException, 0, 0, this.ps.baseUriStr));
        }

        private void UnDecodeChars()
        {
            if (this.maxCharactersInDocument > 0L)
            {
                this.charactersInDocument -= this.ps.charsUsed - this.ps.charPos;
            }
            if ((this.maxCharactersFromEntities > 0L) && this.InEntity)
            {
                this.charactersFromEntities -= this.ps.charsUsed - this.ps.charPos;
            }
            this.ps.bytePos = this.documentStartBytePos;
            if (this.ps.charPos > 0)
            {
                this.ps.bytePos += this.ps.encoding.GetByteCount(this.ps.chars, 0, this.ps.charPos);
            }
            this.ps.charsUsed = this.ps.charPos;
            this.ps.isEof = false;
        }

        private void UnregisterEntity()
        {
            if (this.ps.entity != null)
            {
                this.currentEntities.Remove(this.ps.entity);
            }
        }

        private bool UriEqual(Uri uri1, string uri1Str, string uri2Str, System.Xml.XmlResolver resolver)
        {
            if (resolver == null)
            {
                return (uri1Str == uri2Str);
            }
            if (uri1 == null)
            {
                uri1 = resolver.ResolveUri(null, uri1Str);
            }
            Uri uri = resolver.ResolveUri(null, uri2Str);
            return uri1.Equals(uri);
        }

        private bool ZeroEndingStream(int pos)
        {
            if (((this.v1Compat && (pos == (this.ps.charsUsed - 1))) && ((this.ps.chars[pos] == '\0') && (this.ReadData() == 0))) && this.ps.isStreamEof)
            {
                this.ps.charsUsed--;
                return true;
            }
            return false;
        }

        public override int AttributeCount
        {
            get
            {
                return this.attrCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                return this.reportedBaseUri;
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
                return true;
            }
        }

        public override int Depth
        {
            get
            {
                return this.curNode.depth;
            }
        }

        internal bool DisableUndeclaredEntityCheck
        {
            set
            {
                this.disableUndeclaredEntityCheck = value;
            }
        }

        internal override IDtdInfo DtdInfo
        {
            get
            {
                return this.dtdInfo;
            }
        }

        internal Uri DtdParserProxy_BaseUri
        {
            get
            {
                if (((this.ps.baseUriStr.Length > 0) && (this.ps.baseUri == null)) && (this.xmlResolver != null))
                {
                    this.ps.baseUri = this.xmlResolver.ResolveUri(null, this.ps.baseUriStr);
                }
                return this.ps.baseUri;
            }
        }

        internal int DtdParserProxy_CurrentPosition
        {
            get
            {
                return this.ps.charPos;
            }
            set
            {
                this.ps.charPos = value;
            }
        }

        internal bool DtdParserProxy_DtdValidation
        {
            get
            {
                return this.DtdValidation;
            }
        }

        internal int DtdParserProxy_EntityStackLength
        {
            get
            {
                return (this.parsingStatesStackTop + 1);
            }
        }

        internal bool DtdParserProxy_IsEntityEolNormalized
        {
            get
            {
                return this.ps.eolNormalized;
            }
        }

        internal bool DtdParserProxy_IsEof
        {
            get
            {
                return this.ps.isEof;
            }
        }

        internal int DtdParserProxy_LineNo
        {
            get
            {
                return this.ps.LineNo;
            }
        }

        internal int DtdParserProxy_LineStartPosition
        {
            get
            {
                return this.ps.lineStartPos;
            }
        }

        internal IXmlNamespaceResolver DtdParserProxy_NamespaceResolver
        {
            get
            {
                return this.namespaceManager;
            }
        }

        internal bool DtdParserProxy_Namespaces
        {
            get
            {
                return this.supportNamespaces;
            }
        }

        internal XmlNameTable DtdParserProxy_NameTable
        {
            get
            {
                return this.nameTable;
            }
        }

        internal bool DtdParserProxy_Normalization
        {
            get
            {
                return this.normalize;
            }
        }

        internal char[] DtdParserProxy_ParsingBuffer
        {
            get
            {
                return this.ps.chars;
            }
        }

        internal int DtdParserProxy_ParsingBufferLength
        {
            get
            {
                return this.ps.charsUsed;
            }
        }

        internal bool DtdParserProxy_V1CompatibilityMode
        {
            get
            {
                return this.v1Compat;
            }
        }

        internal IValidationEventHandling DtdParserProxy_ValidationEventHandling
        {
            get
            {
                return this.validationEventHandling;
            }
            set
            {
                this.validationEventHandling = value;
            }
        }

        internal System.Xml.DtdProcessing DtdProcessing
        {
            get
            {
                return this.dtdProcessing;
            }
            set
            {
                if (value > System.Xml.DtdProcessing.Parse)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.dtdProcessing = value;
            }
        }

        private bool DtdValidation
        {
            get
            {
                return (this.validationEventHandling != null);
            }
        }

        internal System.Text.Encoding Encoding
        {
            get
            {
                if (this.readState != System.Xml.ReadState.Interactive)
                {
                    return null;
                }
                return this.reportedEncoding;
            }
        }

        internal System.Xml.EntityHandling EntityHandling
        {
            get
            {
                return this.entityHandling;
            }
            set
            {
                if ((value != System.Xml.EntityHandling.ExpandEntities) && (value != System.Xml.EntityHandling.ExpandCharEntities))
                {
                    throw new XmlException("Xml_EntityHandling", string.Empty);
                }
                this.entityHandling = value;
            }
        }

        public override bool EOF
        {
            get
            {
                return (this.parsingFunction == ParsingFunction.Eof);
            }
        }

        internal XmlNodeType FragmentType
        {
            get
            {
                return this.fragmentType;
            }
        }

        private bool InAttributeValueIterator
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return ((this.attrCount > 0) && (this.parsingFunction >= ParsingFunction.InReadAttributeValue));
            }
        }

        private bool InEntity
        {
            get
            {
                return (this.parsingStatesStackTop >= 0);
            }
        }

        internal object InternalSchemaType
        {
            get
            {
                return this.curNode.schemaType;
            }
            set
            {
                this.curNode.schemaType = value;
            }
        }

        internal object InternalTypedValue
        {
            get
            {
                return this.curNode.typedValue;
            }
            set
            {
                this.curNode.typedValue = value;
            }
        }

        public override bool IsDefault
        {
            get
            {
                return this.curNode.IsDefaultAttribute;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this.curNode.IsEmptyElement;
            }
        }

        public int LineNumber
        {
            get
            {
                return this.curNode.LineNo;
            }
        }

        public int LinePosition
        {
            get
            {
                return this.curNode.LinePos;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.curNode.localName;
            }
        }

        public override string Name
        {
            get
            {
                return this.curNode.GetNameWPrefix(this.nameTable);
            }
        }

        internal override XmlNamespaceManager NamespaceManager
        {
            get
            {
                return this.namespaceManager;
            }
        }

        internal bool Namespaces
        {
            get
            {
                return this.supportNamespaces;
            }
            set
            {
                if (this.readState != System.Xml.ReadState.Initial)
                {
                    throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
                }
                this.supportNamespaces = value;
                if (value)
                {
                    if (this.namespaceManager is NoNamespaceManager)
                    {
                        if ((this.fragment && (this.fragmentParserContext != null)) && (this.fragmentParserContext.NamespaceManager != null))
                        {
                            this.namespaceManager = this.fragmentParserContext.NamespaceManager;
                        }
                        else
                        {
                            this.namespaceManager = new XmlNamespaceManager(this.nameTable);
                        }
                    }
                    this.xmlContext.defaultNamespace = this.namespaceManager.LookupNamespace(string.Empty);
                }
                else
                {
                    if (!(this.namespaceManager is NoNamespaceManager))
                    {
                        this.namespaceManager = new NoNamespaceManager();
                    }
                    this.xmlContext.defaultNamespace = string.Empty;
                }
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.curNode.ns;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.nameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this.curNode.type;
            }
        }

        internal bool Normalization
        {
            get
            {
                return this.normalize;
            }
            set
            {
                if (this.readState == System.Xml.ReadState.Closed)
                {
                    throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
                }
                this.normalize = value;
                if ((this.ps.entity == null) || this.ps.entity.IsExternal)
                {
                    this.ps.eolNormalized = !value;
                }
            }
        }

        internal OnDefaultAttributeUseDelegate OnDefaultAttributeUse
        {
            set
            {
                this.onDefaultAttributeUse = value;
            }
        }

        internal XmlReader OuterReader
        {
            get
            {
                return this.outerReader;
            }
            set
            {
                this.outerReader = value;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.curNode.prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                if (this.curNode.type != XmlNodeType.Attribute)
                {
                    return '"';
                }
                return this.curNode.quoteChar;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                return this.readState;
            }
        }

        public override XmlReaderSettings Settings
        {
            get
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                if (this.nameTableFromSettings)
                {
                    settings.NameTable = this.nameTable;
                }
                switch (this.fragmentType)
                {
                    case XmlNodeType.Element:
                        settings.ConformanceLevel = ConformanceLevel.Fragment;
                        break;

                    case XmlNodeType.Document:
                        settings.ConformanceLevel = ConformanceLevel.Document;
                        break;

                    default:
                        settings.ConformanceLevel = ConformanceLevel.Auto;
                        break;
                }
                settings.CheckCharacters = this.checkCharacters;
                settings.LineNumberOffset = this.lineNumberOffset;
                settings.LinePositionOffset = this.linePositionOffset;
                settings.IgnoreWhitespace = this.whitespaceHandling == System.Xml.WhitespaceHandling.Significant;
                settings.IgnoreProcessingInstructions = this.ignorePIs;
                settings.IgnoreComments = this.ignoreComments;
                settings.DtdProcessing = this.dtdProcessing;
                settings.MaxCharactersInDocument = this.maxCharactersInDocument;
                settings.MaxCharactersFromEntities = this.maxCharactersFromEntities;
                settings.ReadOnly = true;
                return settings;
            }
        }

        internal bool StandAlone
        {
            get
            {
                return this.standalone;
            }
        }

        internal ConformanceLevel V1ComformanceLevel
        {
            get
            {
                if (this.fragmentType != XmlNodeType.Element)
                {
                    return ConformanceLevel.Document;
                }
                return ConformanceLevel.Fragment;
            }
        }

        internal bool V1Compat
        {
            get
            {
                return this.v1Compat;
            }
        }

        internal IValidationEventHandling ValidationEventHandling
        {
            set
            {
                this.validationEventHandling = value;
            }
        }

        public override string Value
        {
            get
            {
                if (this.parsingFunction >= ParsingFunction.PartialTextValue)
                {
                    if (this.parsingFunction == ParsingFunction.PartialTextValue)
                    {
                        this.FinishPartialValue();
                        this.parsingFunction = this.nextParsingFunction;
                    }
                    else
                    {
                        this.FinishOtherValueIterator();
                    }
                }
                return this.curNode.StringValue;
            }
        }

        internal System.Xml.WhitespaceHandling WhitespaceHandling
        {
            get
            {
                return this.whitespaceHandling;
            }
            set
            {
                if (this.readState == System.Xml.ReadState.Closed)
                {
                    throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
                }
                if (value > System.Xml.WhitespaceHandling.None)
                {
                    throw new XmlException("Xml_WhitespaceHandling", string.Empty);
                }
                this.whitespaceHandling = value;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.xmlContext.xmlLang;
            }
        }

        internal System.Xml.XmlResolver XmlResolver
        {
            set
            {
                this.xmlResolver = value;
                this.ps.baseUri = null;
                for (int i = 0; i <= this.parsingStatesStackTop; i++)
                {
                    this.parsingStatesStack[i].baseUri = null;
                }
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this.xmlContext.xmlSpace;
            }
        }

        internal bool XmlValidatingReaderCompatibilityMode
        {
            set
            {
                this.validatingReaderCompatFlag = value;
                if (value)
                {
                    this.nameTable.Add("http://www.w3.org/2001/XMLSchema");
                    this.nameTable.Add("http://www.w3.org/2001/XMLSchema-instance");
                    this.nameTable.Add("urn:schemas-microsoft-com:datatypes");
                }
            }
        }

        private class DtdDefaultAttributeInfoToNodeDataComparer : IComparer<object>
        {
            private static IComparer<object> s_instance = new XmlTextReaderImpl.DtdDefaultAttributeInfoToNodeDataComparer();

            public int Compare(object x, object y)
            {
                string localName;
                string str2;
                string prefix;
                string str4;
                if (x == null)
                {
                    if (y != null)
                    {
                        return -1;
                    }
                    return 0;
                }
                if (y == null)
                {
                    return 1;
                }
                XmlTextReaderImpl.NodeData data = x as XmlTextReaderImpl.NodeData;
                if (data != null)
                {
                    localName = data.localName;
                    prefix = data.prefix;
                }
                else
                {
                    IDtdDefaultAttributeInfo info = x as IDtdDefaultAttributeInfo;
                    if (info == null)
                    {
                        throw new XmlException("Xml_DefaultException", string.Empty);
                    }
                    localName = info.LocalName;
                    prefix = info.Prefix;
                }
                data = y as XmlTextReaderImpl.NodeData;
                if (data != null)
                {
                    str2 = data.localName;
                    str4 = data.prefix;
                }
                else
                {
                    IDtdDefaultAttributeInfo info2 = y as IDtdDefaultAttributeInfo;
                    if (info2 == null)
                    {
                        throw new XmlException("Xml_DefaultException", string.Empty);
                    }
                    str2 = info2.LocalName;
                    str4 = info2.Prefix;
                }
                int num = string.Compare(localName, str2, StringComparison.Ordinal);
                if (num != 0)
                {
                    return num;
                }
                return string.Compare(prefix, str4, StringComparison.Ordinal);
            }

            internal static IComparer<object> Instance
            {
                get
                {
                    return s_instance;
                }
            }
        }

        internal class DtdParserProxy : IDtdParserAdapterV1, IDtdParserAdapterWithValidation, IDtdParserAdapter
        {
            private XmlTextReaderImpl reader;

            internal DtdParserProxy(XmlTextReaderImpl reader)
            {
                this.reader = reader;
            }

            void IDtdParserAdapter.OnNewLine(int pos)
            {
                this.reader.DtdParserProxy_OnNewLine(pos);
            }

            void IDtdParserAdapter.OnPublicId(string publicId, LineInfo keywordLineInfo, LineInfo publicLiteralLineInfo)
            {
                this.reader.DtdParserProxy_OnPublicId(publicId, keywordLineInfo, publicLiteralLineInfo);
            }

            void IDtdParserAdapter.OnSystemId(string systemId, LineInfo keywordLineInfo, LineInfo systemLiteralLineInfo)
            {
                this.reader.DtdParserProxy_OnSystemId(systemId, keywordLineInfo, systemLiteralLineInfo);
            }

            void IDtdParserAdapter.ParseComment(StringBuilder sb)
            {
                this.reader.DtdParserProxy_ParseComment(sb);
            }

            int IDtdParserAdapter.ParseNamedCharRef(bool expand, StringBuilder internalSubsetBuilder)
            {
                return this.reader.DtdParserProxy_ParseNamedCharRef(expand, internalSubsetBuilder);
            }

            int IDtdParserAdapter.ParseNumericCharRef(StringBuilder internalSubsetBuilder)
            {
                return this.reader.DtdParserProxy_ParseNumericCharRef(internalSubsetBuilder);
            }

            void IDtdParserAdapter.ParsePI(StringBuilder sb)
            {
                this.reader.DtdParserProxy_ParsePI(sb);
            }

            bool IDtdParserAdapter.PopEntity(out IDtdEntityInfo oldEntity, out int newEntityId)
            {
                return this.reader.DtdParserProxy_PopEntity(out oldEntity, out newEntityId);
            }

            bool IDtdParserAdapter.PushEntity(IDtdEntityInfo entity, out int entityId)
            {
                return this.reader.DtdParserProxy_PushEntity(entity, out entityId);
            }

            bool IDtdParserAdapter.PushExternalSubset(string systemId, string publicId)
            {
                return this.reader.DtdParserProxy_PushExternalSubset(systemId, publicId);
            }

            void IDtdParserAdapter.PushInternalDtd(string baseUri, string internalDtd)
            {
                this.reader.DtdParserProxy_PushInternalDtd(baseUri, internalDtd);
            }

            int IDtdParserAdapter.ReadData()
            {
                return this.reader.DtdParserProxy_ReadData();
            }

            void IDtdParserAdapter.Throw(Exception e)
            {
                this.reader.DtdParserProxy_Throw(e);
            }

            Uri IDtdParserAdapter.BaseUri
            {
                get
                {
                    return this.reader.DtdParserProxy_BaseUri;
                }
            }

            int IDtdParserAdapter.CurrentPosition
            {
                get
                {
                    return this.reader.DtdParserProxy_CurrentPosition;
                }
                set
                {
                    this.reader.DtdParserProxy_CurrentPosition = value;
                }
            }

            int IDtdParserAdapter.EntityStackLength
            {
                get
                {
                    return this.reader.DtdParserProxy_EntityStackLength;
                }
            }

            bool IDtdParserAdapter.IsEntityEolNormalized
            {
                get
                {
                    return this.reader.DtdParserProxy_IsEntityEolNormalized;
                }
            }

            bool IDtdParserAdapter.IsEof
            {
                get
                {
                    return this.reader.DtdParserProxy_IsEof;
                }
            }

            int IDtdParserAdapter.LineNo
            {
                get
                {
                    return this.reader.DtdParserProxy_LineNo;
                }
            }

            int IDtdParserAdapter.LineStartPosition
            {
                get
                {
                    return this.reader.DtdParserProxy_LineStartPosition;
                }
            }

            IXmlNamespaceResolver IDtdParserAdapter.NamespaceResolver
            {
                get
                {
                    return this.reader.DtdParserProxy_NamespaceResolver;
                }
            }

            XmlNameTable IDtdParserAdapter.NameTable
            {
                get
                {
                    return this.reader.DtdParserProxy_NameTable;
                }
            }

            char[] IDtdParserAdapter.ParsingBuffer
            {
                get
                {
                    return this.reader.DtdParserProxy_ParsingBuffer;
                }
            }

            int IDtdParserAdapter.ParsingBufferLength
            {
                get
                {
                    return this.reader.DtdParserProxy_ParsingBufferLength;
                }
            }

            bool IDtdParserAdapterV1.Namespaces
            {
                get
                {
                    return this.reader.DtdParserProxy_Namespaces;
                }
            }

            bool IDtdParserAdapterV1.Normalization
            {
                get
                {
                    return this.reader.DtdParserProxy_Normalization;
                }
            }

            bool IDtdParserAdapterV1.V1CompatibilityMode
            {
                get
                {
                    return this.reader.DtdParserProxy_V1CompatibilityMode;
                }
            }

            bool IDtdParserAdapterWithValidation.DtdValidation
            {
                get
                {
                    return this.reader.DtdParserProxy_DtdValidation;
                }
            }

            IValidationEventHandling IDtdParserAdapterWithValidation.ValidationEventHandling
            {
                get
                {
                    return this.reader.DtdParserProxy_ValidationEventHandling;
                }
            }
        }

        private enum EntityExpandType
        {
            All,
            OnlyGeneral,
            OnlyCharacter
        }

        private enum EntityType
        {
            CharacterDec,
            CharacterHex,
            CharacterNamed,
            Expanded,
            Skipped,
            FakeExpanded,
            Unexpanded,
            ExpandedInAttribute
        }

        private enum IncrementalReadState
        {
            Text,
            StartTag,
            PI,
            CDATA,
            Comment,
            Attributes,
            AttributeValue,
            ReadData,
            EndElement,
            End,
            ReadValueChunk_OnCachedValue,
            ReadValueChunk_OnPartialValue,
            ReadContentAsBinary_OnCachedValue,
            ReadContentAsBinary_OnPartialValue,
            ReadContentAsBinary_End
        }

        private class NodeData : IComparable
        {
            private char[] chars;
            internal int depth;
            internal int entityId;
            private bool isEmptyOrDefault;
            internal LineInfo lineInfo;
            internal LineInfo lineInfo2;
            internal string localName;
            internal string nameWPrefix;
            internal XmlTextReaderImpl.NodeData nextAttrValueChunk;
            internal string ns;
            internal string prefix;
            internal char quoteChar;
            private static XmlTextReaderImpl.NodeData s_None;
            internal object schemaType;
            internal XmlNodeType type;
            internal object typedValue;
            private string value;
            private int valueLength;
            private int valueStartPos;
            internal bool xmlContextPushed;

            internal NodeData()
            {
                this.Clear(XmlNodeType.None);
                this.xmlContextPushed = false;
            }

            internal void AdjustLineInfo(int valueOffset, bool isNormalized, ref LineInfo lineInfo)
            {
                if (valueOffset != 0)
                {
                    if (this.valueStartPos != -1)
                    {
                        XmlTextReaderImpl.AdjustLineInfo(this.chars, this.valueStartPos, this.valueStartPos + valueOffset, isNormalized, ref lineInfo);
                    }
                    else
                    {
                        XmlTextReaderImpl.AdjustLineInfo(this.value, 0, valueOffset, isNormalized, ref lineInfo);
                    }
                }
            }

            internal void Clear(XmlNodeType type)
            {
                this.type = type;
                this.ClearName();
                this.value = string.Empty;
                this.valueStartPos = -1;
                this.nameWPrefix = string.Empty;
                this.schemaType = null;
                this.typedValue = null;
            }

            internal void ClearName()
            {
                this.localName = string.Empty;
                this.prefix = string.Empty;
                this.ns = string.Empty;
                this.nameWPrefix = string.Empty;
            }

            internal void CopyTo(int valueOffset, StringBuilder sb)
            {
                if (this.value == null)
                {
                    sb.Append(this.chars, this.valueStartPos + valueOffset, this.valueLength - valueOffset);
                }
                else if (valueOffset <= 0)
                {
                    sb.Append(this.value);
                }
                else
                {
                    sb.Append(this.value, valueOffset, this.value.Length - valueOffset);
                }
            }

            internal int CopyTo(int valueOffset, char[] buffer, int offset, int length)
            {
                if (this.value == null)
                {
                    int num = this.valueLength - valueOffset;
                    if (num > length)
                    {
                        num = length;
                    }
                    XmlTextReaderImpl.BlockCopyChars(this.chars, this.valueStartPos + valueOffset, buffer, offset, num);
                    return num;
                }
                int count = this.value.Length - valueOffset;
                if (count > length)
                {
                    count = length;
                }
                this.value.CopyTo(valueOffset, buffer, offset, count);
                return count;
            }

            internal int CopyToBinary(IncrementalReadDecoder decoder, int valueOffset)
            {
                if (this.value == null)
                {
                    return decoder.Decode(this.chars, this.valueStartPos + valueOffset, this.valueLength - valueOffset);
                }
                return decoder.Decode(this.value, valueOffset, this.value.Length - valueOffset);
            }

            internal string CreateNameWPrefix(XmlNameTable nt)
            {
                if (this.prefix.Length == 0)
                {
                    this.nameWPrefix = this.localName;
                }
                else
                {
                    this.nameWPrefix = nt.Add(this.prefix + ":" + this.localName);
                }
                return this.nameWPrefix;
            }

            internal string GetNameWPrefix(XmlNameTable nt)
            {
                if (this.nameWPrefix != null)
                {
                    return this.nameWPrefix;
                }
                return this.CreateNameWPrefix(nt);
            }

            internal void OnBufferInvalidated()
            {
                if (this.value == null)
                {
                    this.value = new string(this.chars, this.valueStartPos, this.valueLength);
                }
                this.valueStartPos = -1;
            }

            internal void SetLineInfo(int lineNo, int linePos)
            {
                this.lineInfo.Set(lineNo, linePos);
            }

            internal void SetLineInfo2(int lineNo, int linePos)
            {
                this.lineInfo2.Set(lineNo, linePos);
            }

            internal void SetNamedNode(XmlNodeType type, string localName)
            {
                this.SetNamedNode(type, localName, string.Empty, localName);
            }

            internal void SetNamedNode(XmlNodeType type, string localName, string prefix, string nameWPrefix)
            {
                this.type = type;
                this.localName = localName;
                this.prefix = prefix;
                this.nameWPrefix = nameWPrefix;
                this.ns = string.Empty;
                this.value = string.Empty;
                this.valueStartPos = -1;
            }

            internal void SetValue(string value)
            {
                this.valueStartPos = -1;
                this.value = value;
            }

            internal void SetValue(char[] chars, int startPos, int len)
            {
                this.value = null;
                this.chars = chars;
                this.valueStartPos = startPos;
                this.valueLength = len;
            }

            internal void SetValueNode(XmlNodeType type, string value)
            {
                this.type = type;
                this.ClearName();
                this.value = value;
                this.valueStartPos = -1;
            }

            internal void SetValueNode(XmlNodeType type, char[] chars, int startPos, int len)
            {
                this.type = type;
                this.ClearName();
                this.value = null;
                this.chars = chars;
                this.valueStartPos = startPos;
                this.valueLength = len;
            }

            int IComparable.CompareTo(object obj)
            {
                XmlTextReaderImpl.NodeData data = obj as XmlTextReaderImpl.NodeData;
                if (data == null)
                {
                    return this.GetHashCode().CompareTo(data.GetHashCode());
                }
                if (!Ref.Equal(this.localName, data.localName))
                {
                    return string.CompareOrdinal(this.localName, data.localName);
                }
                if (Ref.Equal(this.ns, data.ns))
                {
                    return 0;
                }
                return string.CompareOrdinal(this.ns, data.ns);
            }

            internal void TrimSpacesInValue()
            {
                if (this.ValueBuffered)
                {
                    XmlTextReaderImpl.StripSpaces(this.chars, this.valueStartPos, ref this.valueLength);
                }
                else
                {
                    this.value = XmlTextReaderImpl.StripSpaces(this.value);
                }
            }

            internal bool IsDefaultAttribute
            {
                get
                {
                    return ((this.type == XmlNodeType.Attribute) && this.isEmptyOrDefault);
                }
                set
                {
                    this.isEmptyOrDefault = value;
                }
            }

            internal bool IsEmptyElement
            {
                get
                {
                    return ((this.type == XmlNodeType.Element) && this.isEmptyOrDefault);
                }
                set
                {
                    this.isEmptyOrDefault = value;
                }
            }

            internal int LineNo
            {
                get
                {
                    return this.lineInfo.lineNo;
                }
            }

            internal int LinePos
            {
                get
                {
                    return this.lineInfo.linePos;
                }
            }

            internal static XmlTextReaderImpl.NodeData None
            {
                get
                {
                    if (s_None == null)
                    {
                        s_None = new XmlTextReaderImpl.NodeData();
                    }
                    return s_None;
                }
            }

            internal string StringValue
            {
                get
                {
                    if (this.value == null)
                    {
                        this.value = new string(this.chars, this.valueStartPos, this.valueLength);
                    }
                    return this.value;
                }
            }

            internal bool ValueBuffered
            {
                get
                {
                    return (this.value == null);
                }
            }
        }

        private class NoNamespaceManager : XmlNamespaceManager
        {
            public override void AddNamespace(string prefix, string uri)
            {
            }

            public override IEnumerator GetEnumerator()
            {
                return null;
            }

            public override IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
            {
                return null;
            }

            public override bool HasNamespace(string prefix)
            {
                return false;
            }

            public override string LookupNamespace(string prefix)
            {
                return string.Empty;
            }

            public override string LookupPrefix(string uri)
            {
                return null;
            }

            public override bool PopScope()
            {
                return false;
            }

            public override void PushScope()
            {
            }

            public override void RemoveNamespace(string prefix, string uri)
            {
            }

            public override string DefaultNamespace
            {
                get
                {
                    return string.Empty;
                }
            }
        }

        internal delegate void OnDefaultAttributeUseDelegate(IDtdDefaultAttributeInfo defaultAttribute, XmlTextReaderImpl coreReader);

        private enum ParsingFunction
        {
            ElementContent,
            NoData,
            OpenUrl,
            SwitchToInteractive,
            SwitchToInteractiveXmlDecl,
            DocumentContent,
            MoveToElementContent,
            PopElementContext,
            PopEmptyElementContext,
            ResetAttributesRootLevel,
            Error,
            Eof,
            ReaderClosed,
            EntityReference,
            InIncrementalRead,
            FragmentAttribute,
            ReportEndEntity,
            AfterResolveEntityInContent,
            AfterResolveEmptyEntityInContent,
            XmlDeclarationFragment,
            GoToEof,
            PartialTextValue,
            InReadAttributeValue,
            InReadValueChunk,
            InReadContentAsBinary,
            InReadElementContentAsBinary
        }

        private enum ParsingMode
        {
            Full,
            SkipNode,
            SkipContent
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ParsingState
        {
            internal char[] chars;
            internal int charPos;
            internal int charsUsed;
            internal Encoding encoding;
            internal bool appendMode;
            internal Stream stream;
            internal System.Text.Decoder decoder;
            internal byte[] bytes;
            internal int bytePos;
            internal int bytesUsed;
            internal TextReader textReader;
            internal int lineNo;
            internal int lineStartPos;
            internal string baseUriStr;
            internal Uri baseUri;
            internal bool isEof;
            internal bool isStreamEof;
            internal IDtdEntityInfo entity;
            internal int entityId;
            internal bool eolNormalized;
            internal bool entityResolvedManually;
            internal void Clear()
            {
                this.chars = null;
                this.charPos = 0;
                this.charsUsed = 0;
                this.encoding = null;
                this.stream = null;
                this.decoder = null;
                this.bytes = null;
                this.bytePos = 0;
                this.bytesUsed = 0;
                this.textReader = null;
                this.lineNo = 1;
                this.lineStartPos = -1;
                this.baseUriStr = string.Empty;
                this.baseUri = null;
                this.isEof = false;
                this.isStreamEof = false;
                this.eolNormalized = true;
                this.entityResolvedManually = false;
            }

            internal void Close(bool closeInput)
            {
                if (closeInput)
                {
                    if (this.stream != null)
                    {
                        this.stream.Close();
                    }
                    else if (this.textReader != null)
                    {
                        this.textReader.Close();
                    }
                }
            }

            internal int LineNo
            {
                get
                {
                    return this.lineNo;
                }
            }
            internal int LinePos
            {
                get
                {
                    return (this.charPos - this.lineStartPos);
                }
            }
        }

        private class XmlContext
        {
            internal string defaultNamespace;
            internal XmlTextReaderImpl.XmlContext previousContext;
            internal string xmlLang;
            internal XmlSpace xmlSpace;

            internal XmlContext()
            {
                this.xmlSpace = XmlSpace.None;
                this.xmlLang = string.Empty;
                this.defaultNamespace = string.Empty;
                this.previousContext = null;
            }

            internal XmlContext(XmlTextReaderImpl.XmlContext previousContext)
            {
                this.xmlSpace = previousContext.xmlSpace;
                this.xmlLang = previousContext.xmlLang;
                this.defaultNamespace = previousContext.defaultNamespace;
                this.previousContext = previousContext;
            }
        }
    }
}

