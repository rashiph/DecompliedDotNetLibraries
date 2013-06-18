namespace MS.Internal.Xaml.Parser
{
    using MS.Internal.Xaml.Context;
    using System;
    using System.Collections.Generic;
    using System.Xaml;
    using System.Xaml.MS.Impl;
    using System.Xaml.Schema;
    using System.Xml;

    internal class XamlScanner
    {
        private XamlText _accumulatedText;
        private List<XamlAttribute> _attributes;
        private XamlScannerNode _currentNode;
        private bool _hasKeyAttribute;
        private int _nextAttribute;
        private XamlParserContext _parserContext;
        private Queue<XamlScannerNode> _readNodesQueue;
        private XamlScannerStack _scannerStack;
        private XamlXmlReaderSettings _settings;
        private XamlAttribute _typeArgumentAttribute;
        private IXmlLineInfo _xmlLineInfo;
        private XmlReader _xmlReader;

        internal XamlScanner(XamlParserContext context, XmlReader xmlReader, XamlXmlReaderSettings settings)
        {
            this._xmlReader = xmlReader;
            this._xmlLineInfo = settings.ProvideLineInfo ? (xmlReader as IXmlLineInfo) : null;
            this._parserContext = context;
            this._scannerStack = new XamlScannerStack();
            this._readNodesQueue = new Queue<XamlScannerNode>();
            this._settings = settings;
            if (settings.XmlSpacePreserve)
            {
                this._scannerStack.CurrentXmlSpacePreserve = true;
            }
        }

        private void ClearAccumulatedText()
        {
            this._accumulatedText = null;
        }

        private XamlType CreateErrorXamlType(XamlName name, string xmlns)
        {
            return new XamlType(xmlns, name.Name, null, this._parserContext.SchemaContext);
        }

        private void DoXmlRead()
        {
            while (this._readNodesQueue.Count == 0)
            {
                if (this._xmlReader.Read())
                {
                    this.ProcessCurrentXmlNode();
                }
                else
                {
                    this.ReadNone();
                }
            }
        }

        private void EnqueueAnotherAttribute(bool isEmptyTag)
        {
            XamlAttribute attr = this._attributes[this._nextAttribute++];
            XamlScannerNode item = new XamlScannerNode(attr);
            switch (attr.Kind)
            {
                case ScannerAttributeKind.CtorDirective:
                case ScannerAttributeKind.Name:
                case ScannerAttributeKind.Directive:
                    item.NodeType = ScannerNodeType.DIRECTIVE;
                    goto Label_00F3;

                case ScannerAttributeKind.XmlSpace:
                    if (!isEmptyTag)
                    {
                        if (!KS.Eq(attr.Value, "preserve"))
                        {
                            this._scannerStack.CurrentXmlSpacePreserve = false;
                            break;
                        }
                        this._scannerStack.CurrentXmlSpacePreserve = true;
                    }
                    break;

                case ScannerAttributeKind.Event:
                case ScannerAttributeKind.Property:
                    item.IsCtorForcingMember = true;
                    item.NodeType = ScannerNodeType.ATTRIBUTE;
                    goto Label_00F3;

                case ScannerAttributeKind.AttachableProperty:
                    item.NodeType = ScannerNodeType.ATTRIBUTE;
                    goto Label_00F3;

                case ScannerAttributeKind.Unknown:
                {
                    XamlMember property = attr.Property;
                    item.IsCtorForcingMember = !property.IsAttachable && !property.IsDirective;
                    item.NodeType = ScannerNodeType.ATTRIBUTE;
                    goto Label_00F3;
                }
                default:
                    throw new XamlInternalException(System.Xaml.SR.Get("AttributeUnhandledKind"));
            }
            item.NodeType = ScannerNodeType.DIRECTIVE;
        Label_00F3:
            item.PropertyAttribute = attr.Property;
            XamlText text = new XamlText(true);
            text.Paste(attr.Value, false);
            item.PropertyAttributeText = text;
            item.Prefix = attr.Name.Prefix;
            this._readNodesQueue.Enqueue(item);
            if (this._nextAttribute >= this._attributes.Count)
            {
                this._attributes = null;
                this._nextAttribute = -1;
            }
        }

        private void EnqueueAnyText()
        {
            if (this.HaveAccumulatedText)
            {
                this.EnqueueTextNode();
            }
            this.ClearAccumulatedText();
        }

        private void EnqueuePrefixDefinition(XamlAttribute attr)
        {
            string xmlNsPrefixDefined = attr.XmlNsPrefixDefined;
            string xmlNsUriDefined = attr.XmlNsUriDefined;
            this._parserContext.AddNamespacePrefix(xmlNsPrefixDefined, xmlNsUriDefined);
            XamlScannerNode item = new XamlScannerNode(attr) {
                NodeType = ScannerNodeType.PREFIXDEFINITION,
                Prefix = xmlNsPrefixDefined,
                TypeNamespace = xmlNsUriDefined
            };
            this._readNodesQueue.Enqueue(item);
        }

        private void EnqueueTextNode()
        {
            if ((this._scannerStack.Depth != 0) || !this.AccumulatedText.IsWhiteSpaceOnly)
            {
                XamlScannerNode item = new XamlScannerNode(this._xmlLineInfo) {
                    NodeType = ScannerNodeType.TEXT,
                    TextContent = this.AccumulatedText
                };
                this._readNodesQueue.Enqueue(item);
            }
        }

        private bool IsXDataElement(string xmlns, string name)
        {
            return (XamlLanguage.XamlNamespaces.Contains(xmlns) && KS.Eq(XamlLanguage.XData.Name, name));
        }

        private XamlException LineInfo(XamlException e)
        {
            if (this._xmlLineInfo != null)
            {
                e.SetLineInfo(this._xmlLineInfo.LineNumber, this._xmlLineInfo.LinePosition);
            }
            return e;
        }

        private void LoadQueue()
        {
            if (this._readNodesQueue.Count == 0)
            {
                this.DoXmlRead();
            }
        }

        private void PostprocessAttributes(XamlScannerNode node)
        {
            if (this._attributes != null)
            {
                this._nextAttribute = 0;
                if (node.Type == null)
                {
                    if (this._settings.IgnoreUidsOnPropertyElements)
                    {
                        this.StripUidProperty();
                    }
                }
                else
                {
                    bool tagIsRoot = this._scannerStack.Depth == 0;
                    foreach (XamlAttribute attribute in this._attributes)
                    {
                        attribute.Initialize(this._parserContext, node.Type, node.TypeNamespace, tagIsRoot);
                    }
                    List<XamlAttribute> collection = null;
                    List<XamlAttribute> list2 = null;
                    List<XamlAttribute> list3 = null;
                    XamlAttribute item = null;
                    foreach (XamlAttribute attribute3 in this._attributes)
                    {
                        switch (attribute3.Kind)
                        {
                            case ScannerAttributeKind.CtorDirective:
                                if (collection == null)
                                {
                                    collection = new List<XamlAttribute>();
                                }
                                collection.Add(attribute3);
                                break;

                            case ScannerAttributeKind.Name:
                                item = attribute3;
                                break;

                            case ScannerAttributeKind.Directive:
                            case ScannerAttributeKind.XmlSpace:
                                if (attribute3.Property == XamlLanguage.Key)
                                {
                                    this._hasKeyAttribute = true;
                                }
                                if (list2 == null)
                                {
                                    list2 = new List<XamlAttribute>();
                                }
                                list2.Add(attribute3);
                                break;

                            default:
                                if (list3 == null)
                                {
                                    list3 = new List<XamlAttribute>();
                                }
                                list3.Add(attribute3);
                                break;
                        }
                    }
                    this._attributes = new List<XamlAttribute>();
                    if (collection != null)
                    {
                        this._attributes.AddRange(collection);
                    }
                    if (list2 != null)
                    {
                        this._attributes.AddRange(list2);
                    }
                    if (item != null)
                    {
                        this._attributes.Add(item);
                    }
                    if (list3 != null)
                    {
                        this._attributes.AddRange(list3);
                    }
                }
            }
        }

        private void PreprocessAttributes()
        {
            if (this._xmlReader.MoveToFirstAttribute())
            {
                List<XamlAttribute> attrList = new List<XamlAttribute>();
                do
                {
                    string longName = this._xmlReader.Name;
                    string val = this._xmlReader.Value;
                    XamlPropertyName propName = XamlPropertyName.Parse(longName);
                    if (propName == null)
                    {
                        throw new XamlParseException(System.Xaml.SR.Get("InvalidXamlMemberName", new object[] { longName }));
                    }
                    XamlAttribute attr = new XamlAttribute(propName, val, this._xmlLineInfo);
                    if (attr.Kind == ScannerAttributeKind.Namespace)
                    {
                        this.EnqueuePrefixDefinition(attr);
                    }
                    else
                    {
                        attrList.Add(attr);
                    }
                }
                while (this._xmlReader.MoveToNextAttribute());
                this.PreprocessForTypeArguments(attrList);
                if (attrList.Count > 0)
                {
                    this._attributes = attrList;
                }
                this._xmlReader.MoveToElement();
            }
        }

        private void PreprocessForTypeArguments(List<XamlAttribute> attrList)
        {
            int index = -1;
            for (int i = 0; i < attrList.Count; i++)
            {
                XamlAttribute attribute = attrList[i];
                if (KS.Eq(attribute.Name.Name, XamlLanguage.TypeArguments.Name))
                {
                    string xamlNS = this._parserContext.FindNamespaceByPrefix(attribute.Name.Prefix);
                    if (this._parserContext.ResolveDirectiveProperty(xamlNS, attribute.Name.Name) != null)
                    {
                        index = i;
                        this._typeArgumentAttribute = attribute;
                        break;
                    }
                }
            }
            if (index >= 0)
            {
                attrList.RemoveAt(index);
            }
        }

        private void ProcessCurrentXmlNode()
        {
            switch (this._xmlReader.NodeType)
            {
                case XmlNodeType.None:
                    this.ReadNone();
                    break;

                case XmlNodeType.Element:
                    this.ReadElement();
                    return;

                case XmlNodeType.Attribute:
                    break;

                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    this.ReadText();
                    return;

                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    this.ReadWhitespace();
                    return;

                case XmlNodeType.EndElement:
                    this.ReadEndElement();
                    return;

                default:
                    return;
            }
        }

        public void Read()
        {
            this.LoadQueue();
            this._currentNode = this._readNodesQueue.Dequeue();
        }

        private void ReadElement()
        {
            this.EnqueueAnyText();
            this._hasKeyAttribute = false;
            bool isEmptyElement = this._xmlReader.IsEmptyElement;
            string prefix = this._xmlReader.Prefix;
            string localName = this._xmlReader.LocalName;
            if (XamlName.ContainsDot(localName))
            {
                XamlPropertyName name = XamlPropertyName.Parse(this._xmlReader.Name, this._xmlReader.NamespaceURI);
                if (this._scannerStack.CurrentType == null)
                {
                    throw this.LineInfo(new XamlParseException(System.Xaml.SR.Get("ParentlessPropertyElement", new object[] { this._xmlReader.Name })));
                }
                this.ReadPropertyElement(name, this._scannerStack.CurrentType, this._scannerStack.CurrentTypeNamespace, isEmptyElement);
            }
            else
            {
                XamlName name2 = new XamlQualifiedName(prefix, localName);
                this.ReadObjectElement(name2, isEmptyElement);
            }
        }

        private void ReadEndElement()
        {
            this.EnqueueAnyText();
            if (this._scannerStack.CurrentProperty != null)
            {
                this._scannerStack.CurrentProperty = null;
                this._scannerStack.CurrentlyInContent = false;
            }
            else
            {
                this._scannerStack.Pop();
            }
            XamlScannerNode item = new XamlScannerNode(this._xmlLineInfo) {
                NodeType = ScannerNodeType.ENDTAG
            };
            this._readNodesQueue.Enqueue(item);
        }

        private void ReadInnerXDataSection()
        {
            XamlScannerNode item = new XamlScannerNode(this._xmlLineInfo);
            this._xmlReader.MoveToContent();
            string str = this._xmlReader.ReadInnerXml().Trim();
            item.NodeType = ScannerNodeType.TEXT;
            item.IsTextXML = true;
            XamlText text = new XamlText(true);
            text.Paste(str, false);
            item.TextContent = text;
            this._readNodesQueue.Enqueue(item);
            this.ProcessCurrentXmlNode();
        }

        private void ReadNone()
        {
            XamlScannerNode item = new XamlScannerNode(this._xmlLineInfo) {
                NodeType = ScannerNodeType.NONE
            };
            this._readNodesQueue.Enqueue(item);
        }

        private void ReadObjectElement(XamlName name, bool isEmptyTag)
        {
            this._typeArgumentAttribute = null;
            XamlScannerNode node = new XamlScannerNode(this._xmlLineInfo);
            this.PreprocessAttributes();
            node.Prefix = name.Prefix;
            node.IsEmptyTag = isEmptyTag;
            string namespaceURI = this._xmlReader.NamespaceURI;
            if (namespaceURI == null)
            {
                this.ReadObjectElement_NoNamespace(name, node);
            }
            else
            {
                node.TypeNamespace = namespaceURI;
                XamlMember xamlDirective = this._parserContext.SchemaContext.GetXamlDirective(namespaceURI, name.Name);
                if (xamlDirective != null)
                {
                    this.ReadObjectElement_DirectiveProperty(xamlDirective, node);
                }
                else if (this.ReadObjectElement_Object(namespaceURI, name.Name, node))
                {
                    return;
                }
            }
            this._readNodesQueue.Enqueue(node);
            while (this.HaveUnprocessedAttributes)
            {
                this.EnqueueAnotherAttribute(isEmptyTag);
            }
        }

        private void ReadObjectElement_DirectiveProperty(XamlMember dirProperty, XamlScannerNode node)
        {
            node.PropertyElement = dirProperty;
            this.PostprocessAttributes(node);
            if (this._scannerStack.Depth > 0)
            {
                this._scannerStack.CurrentlyInContent = false;
            }
            if (!node.IsEmptyTag)
            {
                this._scannerStack.CurrentProperty = node.PropertyElement;
            }
            node.NodeType = ScannerNodeType.PROPERTYELEMENT;
            node.IsCtorForcingMember = false;
        }

        private void ReadObjectElement_NoNamespace(XamlName name, XamlScannerNode node)
        {
            XamlType type = this.CreateErrorXamlType(name, string.Empty);
            node.Type = type;
            this.PostprocessAttributes(node);
            if (!node.IsEmptyTag)
            {
                node.NodeType = ScannerNodeType.ELEMENT;
                this._scannerStack.Push(node.Type, node.TypeNamespace);
            }
            else
            {
                node.NodeType = ScannerNodeType.EMPTYELEMENT;
            }
        }

        private bool ReadObjectElement_Object(string xmlns, string name, XamlScannerNode node)
        {
            if (this.IsXDataElement(xmlns, name))
            {
                this.ReadInnerXDataSection();
                return true;
            }
            IList<XamlTypeName> typeArguments = null;
            if (this._typeArgumentAttribute != null)
            {
                string str;
                typeArguments = XamlTypeName.ParseListInternal(this._typeArgumentAttribute.Value, new Func<string, string>(this._parserContext.FindNamespaceByPrefix), out str);
                if (typeArguments == null)
                {
                    throw new XamlParseException(this._typeArgumentAttribute.LineNumber, this._typeArgumentAttribute.LinePosition, str);
                }
            }
            XamlTypeName typeName = new XamlTypeName(xmlns, name, typeArguments);
            node.Type = this._parserContext.GetXamlType(typeName, true);
            this.PostprocessAttributes(node);
            if (this._scannerStack.Depth > 0)
            {
                this._scannerStack.CurrentlyInContent = true;
            }
            if (!node.IsEmptyTag)
            {
                node.NodeType = ScannerNodeType.ELEMENT;
                this._scannerStack.Push(node.Type, node.TypeNamespace);
            }
            else
            {
                node.NodeType = ScannerNodeType.EMPTYELEMENT;
            }
            return false;
        }

        private void ReadPropertyElement(XamlPropertyName name, XamlType tagType, string tagNamespace, bool isEmptyTag)
        {
            XamlScannerNode node = new XamlScannerNode(this._xmlLineInfo);
            this.PreprocessAttributes();
            string namespaceURI = this._xmlReader.NamespaceURI;
            XamlMember member = null;
            bool tagIsRoot = this._scannerStack.Depth == 1;
            member = this._parserContext.GetDottedProperty(tagType, tagNamespace, name, tagIsRoot);
            node.Prefix = name.Prefix;
            node.TypeNamespace = namespaceURI;
            node.IsEmptyTag = isEmptyTag;
            this.PostprocessAttributes(node);
            if (this._scannerStack.Depth > 0)
            {
                this._scannerStack.CurrentlyInContent = false;
            }
            node.PropertyElement = member;
            node.IsCtorForcingMember = !member.IsAttachable;
            if (!node.IsEmptyTag)
            {
                this._scannerStack.CurrentProperty = node.PropertyElement;
                node.NodeType = ScannerNodeType.PROPERTYELEMENT;
            }
            else
            {
                node.NodeType = ScannerNodeType.EMPTYPROPERTYELEMENT;
            }
            this._readNodesQueue.Enqueue(node);
            while (this.HaveUnprocessedAttributes)
            {
                this.EnqueueAnotherAttribute(isEmptyTag);
            }
        }

        private void ReadText()
        {
            bool trimLeadingWhitespace = !this._scannerStack.CurrentlyInContent;
            this.AccumulatedText.Paste(this._xmlReader.Value, trimLeadingWhitespace);
            this._scannerStack.CurrentlyInContent = true;
        }

        private void ReadWhitespace()
        {
            bool trimLeadingWhitespace = !this._scannerStack.CurrentlyInContent;
            this.AccumulatedText.Paste(this._xmlReader.Value, trimLeadingWhitespace);
        }

        private void StripUidProperty()
        {
            for (int i = this._attributes.Count - 1; i >= 0; i--)
            {
                if (KS.Eq(this._attributes[i].Name.ScopedName, XamlLanguage.Uid.Name))
                {
                    this._attributes.RemoveAt(i);
                }
            }
            if (this._attributes.Count == 0)
            {
                this._attributes = null;
            }
        }

        private XamlText AccumulatedText
        {
            get
            {
                if (this._accumulatedText == null)
                {
                    this._accumulatedText = new XamlText(this._scannerStack.CurrentXmlSpacePreserve);
                }
                return this._accumulatedText;
            }
        }

        public bool HasKeyAttribute
        {
            get
            {
                return this._hasKeyAttribute;
            }
        }

        private bool HaveAccumulatedText
        {
            get
            {
                return ((this._accumulatedText != null) && !this._accumulatedText.IsEmpty);
            }
        }

        private bool HaveUnprocessedAttributes
        {
            get
            {
                return (this._attributes != null);
            }
        }

        public bool IsCtorForcingMember
        {
            get
            {
                return this._currentNode.IsCtorForcingMember;
            }
        }

        public bool IsTextXML
        {
            get
            {
                return this._currentNode.IsTextXML;
            }
        }

        public int LineNumber
        {
            get
            {
                return this._currentNode.LineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                return this._currentNode.LinePosition;
            }
        }

        public string Namespace
        {
            get
            {
                return this._currentNode.TypeNamespace;
            }
        }

        public ScannerNodeType NodeType
        {
            get
            {
                return this._currentNode.NodeType;
            }
        }

        public ScannerNodeType PeekNodeType
        {
            get
            {
                this.LoadQueue();
                return this._readNodesQueue.Peek().NodeType;
            }
        }

        public XamlType PeekType
        {
            get
            {
                this.LoadQueue();
                return this._readNodesQueue.Peek().Type;
            }
        }

        public string Prefix
        {
            get
            {
                return this._currentNode.Prefix;
            }
        }

        public XamlMember PropertyAttribute
        {
            get
            {
                return this._currentNode.PropertyAttribute;
            }
        }

        public XamlText PropertyAttributeText
        {
            get
            {
                return this._currentNode.PropertyAttributeText;
            }
        }

        public XamlMember PropertyElement
        {
            get
            {
                return this._currentNode.PropertyElement;
            }
        }

        public XamlText TextContent
        {
            get
            {
                return this._currentNode.TextContent;
            }
        }

        public XamlType Type
        {
            get
            {
                return this._currentNode.Type;
            }
        }
    }
}

