namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;

    internal abstract class XmlBaseReader : XmlDictionaryReader
    {
        private XmlAtomicTextNode atomicTextNode;
        private int attributeCount;
        private int attributeIndex;
        private XmlAttributeNode[] attributeNodes;
        private AttributeSorter attributeSorter;
        private int attributeStart;
        private static System.Text.Base64Encoding base64Encoding;
        private static System.Text.BinHexEncoding binhexEncoding;
        private XmlBufferReader bufferReader;
        private XmlCDataNode cdataNode;
        private char[] chars;
        private static XmlClosedNode closedNode = new XmlClosedNode(XmlBufferReader.Empty);
        private XmlCommentNode commentNode;
        private XmlComplexTextNode complexTextNode;
        private XmlDeclarationNode declarationNode;
        private int depth;
        private XmlElementNode[] elementNodes;
        private static XmlEndOfFileNode endOfFileNode = new XmlEndOfFileNode(XmlBufferReader.Empty);
        private static XmlInitialNode initialNode = new XmlInitialNode(XmlBufferReader.Empty);
        private string localName;
        private XmlNameTable nameTable;
        private XmlNode node;
        private string ns;
        private NamespaceManager nsMgr;
        private string prefix;
        private XmlDictionaryReaderQuotas quotas;
        private bool readingElement;
        private bool rootElement;
        private XmlElementNode rootElementNode;
        private bool signing;
        private XmlSigningNodeWriter signingWriter;
        private int trailByteCount;
        private byte[] trailBytes;
        private int trailCharCount;
        private char[] trailChars;
        private string value;
        private XmlWhitespaceTextNode whitespaceTextNode;
        private const string xml = "xml";
        private const string xmlNamespace = "http://www.w3.org/XML/1998/namespace";
        private const string xmlns = "xmlns";
        private const string xmlnsNamespace = "http://www.w3.org/2000/xmlns/";

        protected XmlBaseReader()
        {
            this.bufferReader = new XmlBufferReader(this);
            this.nsMgr = new NamespaceManager(this.bufferReader);
            this.quotas = new XmlDictionaryReaderQuotas();
            this.rootElementNode = new XmlElementNode(this.bufferReader);
            this.atomicTextNode = new XmlAtomicTextNode(this.bufferReader);
            this.node = closedNode;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected XmlAttributeNode AddAttribute()
        {
            return this.AddAttribute(QNameType.Normal, true);
        }

        private XmlAttributeNode AddAttribute(QNameType qnameType, bool isAtomicValue)
        {
            int attributeCount = this.attributeCount;
            if (this.attributeNodes == null)
            {
                this.attributeNodes = new XmlAttributeNode[4];
            }
            else if (this.attributeNodes.Length == attributeCount)
            {
                XmlAttributeNode[] destinationArray = new XmlAttributeNode[attributeCount * 2];
                Array.Copy(this.attributeNodes, destinationArray, attributeCount);
                this.attributeNodes = destinationArray;
            }
            XmlAttributeNode node = this.attributeNodes[attributeCount];
            if (node == null)
            {
                node = new XmlAttributeNode(this.bufferReader);
                this.attributeNodes[attributeCount] = node;
            }
            node.QNameType = qnameType;
            node.IsAtomicValue = isAtomicValue;
            node.AttributeText.QNameType = qnameType;
            node.AttributeText.IsAtomicValue = isAtomicValue;
            this.attributeCount++;
            return node;
        }

        protected Namespace AddNamespace()
        {
            return this.nsMgr.AddNamespace();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected XmlAttributeNode AddXmlAttribute()
        {
            return this.AddAttribute(QNameType.Normal, true);
        }

        protected XmlAttributeNode AddXmlnsAttribute(Namespace ns)
        {
            if (!ns.Prefix.IsEmpty && ns.Uri.IsEmpty)
            {
                XmlExceptionHelper.ThrowEmptyNamespace(this);
            }
            if (ns.Prefix.IsXml && (ns.Uri != "http://www.w3.org/XML/1998/namespace"))
            {
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlSpecificBindingPrefix", new object[] { "xml", "http://www.w3.org/XML/1998/namespace" })));
            }
            else if (ns.Prefix.IsXmlns && (ns.Uri != "http://www.w3.org/2000/xmlns/"))
            {
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlSpecificBindingPrefix", new object[] { "xmlns", "http://www.w3.org/2000/xmlns/" })));
            }
            this.nsMgr.Register(ns);
            XmlAttributeNode node = this.AddAttribute(QNameType.Xmlns, false);
            node.Namespace = ns;
            node.AttributeText.Namespace = ns;
            return node;
        }

        private void CheckAttributes(XmlAttributeNode[] attributeNodes, int attributeCount)
        {
            if (this.attributeSorter == null)
            {
                this.attributeSorter = new AttributeSorter();
            }
            if (!this.attributeSorter.Sort(attributeNodes, attributeCount))
            {
                int num;
                int num2;
                this.attributeSorter.GetIndeces(out num, out num2);
                if (attributeNodes[num].QNameType == QNameType.Xmlns)
                {
                    XmlExceptionHelper.ThrowDuplicateXmlnsAttribute(this, attributeNodes[num].Namespace.Prefix.GetString(), "http://www.w3.org/2000/xmlns/");
                }
                else
                {
                    XmlExceptionHelper.ThrowDuplicateAttribute(this, attributeNodes[num].Prefix.GetString(), attributeNodes[num2].Prefix.GetString(), attributeNodes[num].LocalName.GetString(), attributeNodes[num].Namespace.Uri.GetString());
                }
            }
        }

        private bool CheckDeclAttribute(int index, string localName, string value, bool checkLower, string valueSR)
        {
            XmlAttributeNode node = this.attributeNodes[index];
            if (!node.Prefix.IsEmpty)
            {
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlMalformedDecl")));
            }
            if (node.LocalName != localName)
            {
                return false;
            }
            if ((value != null) && !node.Value.Equals2(value, checkLower))
            {
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString(valueSR)));
            }
            return true;
        }

        private bool CheckStandalone(int attr)
        {
            XmlAttributeNode node = this.attributeNodes[attr];
            if (!node.Prefix.IsEmpty)
            {
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlMalformedDecl")));
            }
            if (node.LocalName != "standalone")
            {
                return false;
            }
            if (!node.Value.Equals2("yes", false) && !node.Value.Equals2("no", false))
            {
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlInvalidStandalone")));
            }
            return true;
        }

        public override void Close()
        {
            this.MoveToNode(closedNode);
            this.nameTable = null;
            if ((this.attributeNodes != null) && (this.attributeNodes.Length > 0x10))
            {
                this.attributeNodes = null;
            }
            if ((this.elementNodes != null) && (this.elementNodes.Length > 0x10))
            {
                this.elementNodes = null;
            }
            this.nsMgr.Close();
            this.bufferReader.Close();
            if (this.signingWriter != null)
            {
                this.signingWriter.Close();
            }
            if (this.attributeSorter != null)
            {
                this.attributeSorter.Close();
            }
        }

        protected abstract XmlSigningNodeWriter CreateSigningNodeWriter();
        public override void EndCanonicalization()
        {
            if (!this.signing)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlCanonicalizationNotStarted")));
            }
            this.signingWriter.Flush();
            this.signingWriter.Close();
            this.signing = false;
        }

        protected XmlElementNode EnterScope()
        {
            if (this.depth == 0)
            {
                if (this.rootElement)
                {
                    XmlExceptionHelper.ThrowMultipleRootElements(this);
                }
                this.rootElement = true;
            }
            this.nsMgr.EnterScope();
            this.depth++;
            if (this.depth > this.quotas.MaxDepth)
            {
                XmlExceptionHelper.ThrowMaxDepthExceeded(this, this.quotas.MaxDepth);
            }
            if (this.elementNodes == null)
            {
                this.elementNodes = new XmlElementNode[4];
            }
            else if (this.elementNodes.Length == this.depth)
            {
                XmlElementNode[] destinationArray = new XmlElementNode[this.depth * 2];
                Array.Copy(this.elementNodes, destinationArray, this.depth);
                this.elementNodes = destinationArray;
            }
            XmlElementNode node = this.elementNodes[this.depth];
            if (node == null)
            {
                node = new XmlElementNode(this.bufferReader);
                this.elementNodes[this.depth] = node;
            }
            this.attributeCount = 0;
            this.attributeStart = -1;
            this.attributeIndex = -1;
            this.MoveToNode(node);
            return node;
        }

        protected void ExitScope()
        {
            if (this.depth == 0)
            {
                XmlExceptionHelper.ThrowUnexpectedEndElement(this);
            }
            this.depth--;
            this.nsMgr.ExitScope();
        }

        protected void FixXmlAttribute(XmlAttributeNode attributeNode)
        {
            if (attributeNode.Prefix == "xml")
            {
                if (attributeNode.LocalName == "lang")
                {
                    this.nsMgr.AddLangAttribute(attributeNode.Value.GetString());
                }
                else if (attributeNode.LocalName == "space")
                {
                    switch (attributeNode.Value.GetString())
                    {
                        case "preserve":
                            this.nsMgr.AddSpaceAttribute(System.Xml.XmlSpace.Preserve);
                            return;

                        case "default":
                            this.nsMgr.AddSpaceAttribute(System.Xml.XmlSpace.Default);
                            break;
                    }
                }
            }
        }

        public override string GetAttribute(int index)
        {
            return this.GetAttributeNode(index).ValueAsString;
        }

        public override string GetAttribute(string name)
        {
            XmlAttributeNode attributeNode = this.GetAttributeNode(name);
            if (attributeNode == null)
            {
                return null;
            }
            return attributeNode.ValueAsString;
        }

        public override string GetAttribute(string localName, string namespaceUri)
        {
            XmlAttributeNode attributeNode = this.GetAttributeNode(localName, namespaceUri);
            if (attributeNode == null)
            {
                return null;
            }
            return attributeNode.ValueAsString;
        }

        public override string GetAttribute(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            XmlAttributeNode attributeNode = this.GetAttributeNode(localName, namespaceUri);
            if (attributeNode == null)
            {
                return null;
            }
            return attributeNode.ValueAsString;
        }

        private XmlAttributeNode GetAttributeNode(int index)
        {
            if (!this.node.CanGetAttribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", System.Runtime.Serialization.SR.GetString("XmlElementAttributes")));
            }
            if (index < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (index >= this.attributeCount)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { this.attributeCount })));
            }
            return this.attributeNodes[index];
        }

        private XmlAttributeNode GetAttributeNode(string name)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            }
            if (this.node.CanGetAttribute)
            {
                string str;
                string str2;
                int index = name.IndexOf(':');
                if (index == -1)
                {
                    if (name == "xmlns")
                    {
                        str = "xmlns";
                        str2 = string.Empty;
                    }
                    else
                    {
                        str = string.Empty;
                        str2 = name;
                    }
                }
                else
                {
                    str = name.Substring(0, index);
                    str2 = name.Substring(index + 1);
                }
                XmlAttributeNode[] attributeNodes = this.attributeNodes;
                int attributeCount = this.attributeCount;
                int attributeStart = this.attributeStart;
                for (int i = 0; i < attributeCount; i++)
                {
                    if (++attributeStart >= attributeCount)
                    {
                        attributeStart = 0;
                    }
                    XmlAttributeNode node = attributeNodes[attributeStart];
                    if (node.IsPrefixAndLocalName(str, str2))
                    {
                        this.attributeStart = attributeStart;
                        return node;
                    }
                }
            }
            return null;
        }

        private XmlAttributeNode GetAttributeNode(string localName, string namespaceUri)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            }
            if (namespaceUri == null)
            {
                namespaceUri = string.Empty;
            }
            if (this.node.CanGetAttribute)
            {
                XmlAttributeNode[] attributeNodes = this.attributeNodes;
                int attributeCount = this.attributeCount;
                int attributeStart = this.attributeStart;
                for (int i = 0; i < attributeCount; i++)
                {
                    if (++attributeStart >= attributeCount)
                    {
                        attributeStart = 0;
                    }
                    XmlAttributeNode node = attributeNodes[attributeStart];
                    if (node.IsLocalNameAndNamespaceUri(localName, namespaceUri))
                    {
                        this.attributeStart = attributeStart;
                        return node;
                    }
                }
            }
            return null;
        }

        private XmlAttributeNode GetAttributeNode(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            }
            if (namespaceUri == null)
            {
                namespaceUri = XmlDictionaryString.Empty;
            }
            if (this.node.CanGetAttribute)
            {
                XmlAttributeNode[] attributeNodes = this.attributeNodes;
                int attributeCount = this.attributeCount;
                int attributeStart = this.attributeStart;
                for (int i = 0; i < attributeCount; i++)
                {
                    if (++attributeStart >= attributeCount)
                    {
                        attributeStart = 0;
                    }
                    XmlAttributeNode node = attributeNodes[attributeStart];
                    if (node.IsLocalNameAndNamespaceUri(localName, namespaceUri))
                    {
                        this.attributeStart = attributeStart;
                        return node;
                    }
                }
            }
            return null;
        }

        private char[] GetCharBuffer(int count)
        {
            if (count > 0x400)
            {
                return new char[count];
            }
            if ((this.chars == null) || (this.chars.Length < count))
            {
                this.chars = new char[count];
            }
            return this.chars;
        }

        private string GetLocalName(bool enforceAtomization)
        {
            if (this.localName != null)
            {
                return this.localName;
            }
            if (this.node.QNameType == QNameType.Normal)
            {
                if (!enforceAtomization && (this.nameTable == null))
                {
                    return this.node.LocalName.GetString();
                }
                return this.node.LocalName.GetString(this.NameTable);
            }
            if (this.node.Namespace.Prefix.IsEmpty)
            {
                return "xmlns";
            }
            if (!enforceAtomization && (this.nameTable == null))
            {
                return this.node.Namespace.Prefix.GetString();
            }
            return this.node.Namespace.Prefix.GetString(this.NameTable);
        }

        private string GetNamespaceUri(bool enforceAtomization)
        {
            if (this.ns != null)
            {
                return this.ns;
            }
            if (this.node.QNameType != QNameType.Normal)
            {
                return "http://www.w3.org/2000/xmlns/";
            }
            if (!enforceAtomization && (this.nameTable == null))
            {
                return this.node.Namespace.Uri.GetString();
            }
            return this.node.Namespace.Uri.GetString(this.NameTable);
        }

        public override void GetNonAtomizedNames(out string localName, out string namespaceUri)
        {
            localName = this.GetLocalName(false);
            namespaceUri = this.GetNamespaceUri(false);
        }

        public string GetOpenElements()
        {
            string str = string.Empty;
            for (int i = this.depth; i > 0; i--)
            {
                string str2 = this.elementNodes[i].LocalName.GetString();
                if (i != this.depth)
                {
                    str = str + ", ";
                }
                str = str + str2;
            }
            return str;
        }

        public override int IndexOfLocalName(string[] localNames, string namespaceUri)
        {
            if (localNames == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localNames");
            }
            if (namespaceUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            }
            QNameType qNameType = this.node.QNameType;
            if (this.node.IsNamespaceUri(namespaceUri))
            {
                if (qNameType == QNameType.Normal)
                {
                    StringHandle localName = this.node.LocalName;
                    for (int i = 0; i < localNames.Length; i++)
                    {
                        string str = localNames[i];
                        if (str == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "localNames[{0}]", new object[] { i }));
                        }
                        if (localName == str)
                        {
                            return i;
                        }
                    }
                }
                else
                {
                    PrefixHandle prefix = this.node.Namespace.Prefix;
                    for (int j = 0; j < localNames.Length; j++)
                    {
                        string str2 = localNames[j];
                        if (str2 == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "localNames[{0}]", new object[] { j }));
                        }
                        if (prefix == str2)
                        {
                            return j;
                        }
                    }
                }
            }
            return -1;
        }

        public override int IndexOfLocalName(XmlDictionaryString[] localNames, XmlDictionaryString namespaceUri)
        {
            if (localNames == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localNames");
            }
            if (namespaceUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            }
            QNameType qNameType = this.node.QNameType;
            if (this.node.IsNamespaceUri(namespaceUri))
            {
                if (qNameType == QNameType.Normal)
                {
                    StringHandle localName = this.node.LocalName;
                    for (int i = 0; i < localNames.Length; i++)
                    {
                        XmlDictionaryString str = localNames[i];
                        if (str == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "localNames[{0}]", new object[] { i }));
                        }
                        if (localName == str)
                        {
                            return i;
                        }
                    }
                }
                else
                {
                    PrefixHandle prefix = this.node.Namespace.Prefix;
                    for (int j = 0; j < localNames.Length; j++)
                    {
                        XmlDictionaryString str2 = localNames[j];
                        if (str2 == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "localNames[{0}]", new object[] { j }));
                        }
                        if (prefix == str2)
                        {
                            return j;
                        }
                    }
                }
            }
            return -1;
        }

        public override bool IsLocalName(string localName)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            }
            return this.node.IsLocalName(localName);
        }

        public override bool IsLocalName(XmlDictionaryString localName)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localName"));
            }
            return this.node.IsLocalName(localName);
        }

        public override bool IsNamespaceUri(string namespaceUri)
        {
            if (namespaceUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            }
            return this.node.IsNamespaceUri(namespaceUri);
        }

        public override bool IsNamespaceUri(XmlDictionaryString namespaceUri)
        {
            if (namespaceUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            }
            return this.node.IsNamespaceUri(namespaceUri);
        }

        public sealed override bool IsStartElement()
        {
            switch (this.node.NodeType)
            {
                case XmlNodeType.Element:
                    return true;

                case XmlNodeType.EndElement:
                    return false;

                case XmlNodeType.None:
                    this.Read();
                    if (this.node.NodeType == XmlNodeType.Element)
                    {
                        return true;
                    }
                    break;
            }
            return (this.MoveToContent() == XmlNodeType.Element);
        }

        public override bool IsStartElement(string name)
        {
            string str;
            string str2;
            if (name == null)
            {
                return false;
            }
            int index = name.IndexOf(':');
            if (index == -1)
            {
                str = string.Empty;
                str2 = name;
            }
            else
            {
                str = name.Substring(0, index);
                str2 = name.Substring(index + 1);
            }
            return ((((this.node.NodeType == XmlNodeType.Element) || this.IsStartElement()) && (this.node.Prefix == str)) && (this.node.LocalName == str2));
        }

        public override bool IsStartElement(string localName, string namespaceUri)
        {
            if (localName == null)
            {
                return false;
            }
            if (namespaceUri == null)
            {
                return false;
            }
            return ((((this.node.NodeType == XmlNodeType.Element) || this.IsStartElement()) && (this.node.LocalName == localName)) && this.node.IsNamespaceUri(namespaceUri));
        }

        public override bool IsStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            if (namespaceUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            }
            return ((((this.node.NodeType == XmlNodeType.Element) || this.IsStartElement()) && (this.node.LocalName == localName)) && this.node.IsNamespaceUri(namespaceUri));
        }

        public override string LookupNamespace(string prefix)
        {
            Namespace namespace2 = this.nsMgr.LookupNamespace(prefix);
            if (namespace2 != null)
            {
                return namespace2.Uri.GetString(this.NameTable);
            }
            if (prefix == "xmlns")
            {
                return "http://www.w3.org/2000/xmlns/";
            }
            return null;
        }

        protected Namespace LookupNamespace(PrefixHandle prefix)
        {
            Namespace namespace2 = this.nsMgr.LookupNamespace(prefix);
            if (namespace2 == null)
            {
                XmlExceptionHelper.ThrowUndefinedPrefix(this, prefix.GetString());
            }
            return namespace2;
        }

        protected Namespace LookupNamespace(PrefixHandleType prefix)
        {
            Namespace namespace2 = this.nsMgr.LookupNamespace(prefix);
            if (namespace2 == null)
            {
                XmlExceptionHelper.ThrowUndefinedPrefix(this, PrefixHandle.GetString(prefix));
            }
            return namespace2;
        }

        protected XmlAtomicTextNode MoveToAtomicText()
        {
            XmlAtomicTextNode atomicTextNode = this.atomicTextNode;
            this.MoveToNode(atomicTextNode);
            return atomicTextNode;
        }

        public override void MoveToAttribute(int index)
        {
            this.MoveToNode(this.GetAttributeNode(index));
            this.attributeIndex = index;
        }

        public override bool MoveToAttribute(string name)
        {
            XmlNode attributeNode = this.GetAttributeNode(name);
            if (attributeNode == null)
            {
                return false;
            }
            this.MoveToNode(attributeNode);
            this.attributeIndex = this.attributeStart;
            return true;
        }

        public override bool MoveToAttribute(string localName, string namespaceUri)
        {
            XmlNode attributeNode = this.GetAttributeNode(localName, namespaceUri);
            if (attributeNode == null)
            {
                return false;
            }
            this.MoveToNode(attributeNode);
            this.attributeIndex = this.attributeStart;
            return true;
        }

        protected XmlCDataNode MoveToCData()
        {
            if (this.cdataNode == null)
            {
                this.cdataNode = new XmlCDataNode(this.bufferReader);
            }
            this.MoveToNode(this.cdataNode);
            return this.cdataNode;
        }

        protected XmlCommentNode MoveToComment()
        {
            if (this.commentNode == null)
            {
                this.commentNode = new XmlCommentNode(this.bufferReader);
            }
            this.MoveToNode(this.commentNode);
            return this.commentNode;
        }

        protected XmlComplexTextNode MoveToComplexText()
        {
            if (this.complexTextNode == null)
            {
                this.complexTextNode = new XmlComplexTextNode(this.bufferReader);
            }
            this.MoveToNode(this.complexTextNode);
            return this.complexTextNode;
        }

        public override XmlNodeType MoveToContent()
        {
        Label_0000:
            if (this.node.HasContent)
            {
                if (((this.node.NodeType == XmlNodeType.Text) || (this.node.NodeType == XmlNodeType.CDATA)) && (this.trailByteCount <= 0))
                {
                    if (this.value == null)
                    {
                        if (this.node.Value.IsWhitespace())
                        {
                            goto Label_0074;
                        }
                    }
                    else if (XmlConverter.IsWhitespace(this.value))
                    {
                        goto Label_0074;
                    }
                }
                goto Label_007C;
            }
            if (this.node.NodeType == XmlNodeType.Attribute)
            {
                this.MoveToElement();
                goto Label_007C;
            }
        Label_0074:
            if (this.Read())
            {
                goto Label_0000;
            }
        Label_007C:
            return this.node.NodeType;
        }

        protected XmlDeclarationNode MoveToDeclaration()
        {
            if (this.attributeCount < 1)
            {
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlDeclMissingVersion")));
            }
            if (this.attributeCount > 3)
            {
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlMalformedDecl")));
            }
            if (!this.CheckDeclAttribute(0, "version", "1.0", false, "XmlInvalidVersion"))
            {
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlDeclMissingVersion")));
            }
            if (this.attributeCount > 1)
            {
                if (this.CheckDeclAttribute(1, "encoding", null, true, "XmlInvalidEncoding"))
                {
                    if ((this.attributeCount == 3) && !this.CheckStandalone(2))
                    {
                        XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlMalformedDecl")));
                    }
                }
                else if (!this.CheckStandalone(1) || (this.attributeCount > 2))
                {
                    XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlMalformedDecl")));
                }
            }
            if (this.declarationNode == null)
            {
                this.declarationNode = new XmlDeclarationNode(this.bufferReader);
            }
            this.MoveToNode(this.declarationNode);
            return this.declarationNode;
        }

        public override bool MoveToElement()
        {
            if (!this.node.CanMoveToElement)
            {
                return false;
            }
            if (this.depth == 0)
            {
                this.MoveToDeclaration();
            }
            else
            {
                this.MoveToNode(this.elementNodes[this.depth]);
            }
            this.attributeIndex = -1;
            return true;
        }

        protected void MoveToEndElement()
        {
            if (this.depth == 0)
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this);
            }
            XmlElementNode node = this.elementNodes[this.depth];
            XmlEndElementNode endElement = node.EndElement;
            endElement.Namespace = node.Namespace;
            this.MoveToNode(endElement);
        }

        protected void MoveToEndOfFile()
        {
            if (this.depth != 0)
            {
                XmlExceptionHelper.ThrowUnexpectedEndOfFile(this);
            }
            this.MoveToNode(endOfFileNode);
        }

        public override bool MoveToFirstAttribute()
        {
            if (!this.node.CanGetAttribute || (this.attributeCount == 0))
            {
                return false;
            }
            this.MoveToNode(this.GetAttributeNode(0));
            this.attributeIndex = 0;
            return true;
        }

        protected void MoveToInitial(XmlDictionaryReaderQuotas quotas)
        {
            if (quotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("quotas");
            }
            quotas.InternalCopyTo(this.quotas);
            this.quotas.MakeReadOnly();
            this.nsMgr.Clear();
            this.depth = 0;
            this.attributeCount = 0;
            this.attributeStart = -1;
            this.attributeIndex = -1;
            this.rootElement = false;
            this.readingElement = false;
            this.signing = false;
            this.MoveToNode(initialNode);
        }

        public override bool MoveToNextAttribute()
        {
            if (!this.node.CanGetAttribute)
            {
                return false;
            }
            int index = this.attributeIndex + 1;
            if (index >= this.attributeCount)
            {
                return false;
            }
            this.MoveToNode(this.GetAttributeNode(index));
            this.attributeIndex = index;
            return true;
        }

        protected void MoveToNode(XmlNode node)
        {
            this.node = node;
            this.ns = null;
            this.localName = null;
            this.prefix = null;
            this.value = null;
        }

        protected XmlTextNode MoveToWhitespaceText()
        {
            if (this.whitespaceTextNode == null)
            {
                this.whitespaceTextNode = new XmlWhitespaceTextNode(this.bufferReader);
            }
            if (this.nsMgr.XmlSpace == System.Xml.XmlSpace.Preserve)
            {
                this.whitespaceTextNode.NodeType = XmlNodeType.SignificantWhitespace;
            }
            else
            {
                this.whitespaceTextNode.NodeType = XmlNodeType.Whitespace;
            }
            this.MoveToNode(this.whitespaceTextNode);
            return this.whitespaceTextNode;
        }

        protected void ProcessAttributes()
        {
            if (this.attributeCount > 0)
            {
                this.ProcessAttributes(this.attributeNodes, this.attributeCount);
            }
        }

        private void ProcessAttributes(XmlAttributeNode[] attributeNodes, int attributeCount)
        {
            for (int i = 0; i < attributeCount; i++)
            {
                XmlAttributeNode node = attributeNodes[i];
                if (node.QNameType == QNameType.Normal)
                {
                    PrefixHandle prefix = node.Prefix;
                    if (!prefix.IsEmpty)
                    {
                        node.Namespace = this.LookupNamespace(prefix);
                    }
                    else
                    {
                        node.Namespace = NamespaceManager.EmptyNamespace;
                    }
                    node.AttributeText.Namespace = node.Namespace;
                }
            }
            if (attributeCount > 1)
            {
                if (attributeCount < 12)
                {
                    for (int j = 0; j < (attributeCount - 1); j++)
                    {
                        XmlAttributeNode node2 = attributeNodes[j];
                        if (node2.QNameType == QNameType.Normal)
                        {
                            for (int k = j + 1; k < attributeCount; k++)
                            {
                                XmlAttributeNode node3 = attributeNodes[k];
                                if (((node3.QNameType == QNameType.Normal) && (node2.LocalName == node3.LocalName)) && (node2.Namespace.Uri == node3.Namespace.Uri))
                                {
                                    XmlExceptionHelper.ThrowDuplicateAttribute(this, node2.Prefix.GetString(), node3.Prefix.GetString(), node2.LocalName.GetString(), node2.Namespace.Uri.GetString());
                                }
                            }
                        }
                        else
                        {
                            for (int m = j + 1; m < attributeCount; m++)
                            {
                                XmlAttributeNode node4 = attributeNodes[m];
                                if ((node4.QNameType == QNameType.Xmlns) && (node2.Namespace.Prefix == node4.Namespace.Prefix))
                                {
                                    XmlExceptionHelper.ThrowDuplicateAttribute(this, "xmlns", "xmlns", node2.Namespace.Prefix.GetString(), "http://www.w3.org/2000/xmlns/");
                                }
                            }
                        }
                    }
                }
                else
                {
                    this.CheckAttributes(attributeNodes, attributeCount);
                }
            }
        }

        public override bool ReadAttributeValue()
        {
            XmlAttributeTextNode attributeText = this.node.AttributeText;
            if (attributeText == null)
            {
                return false;
            }
            this.MoveToNode(attributeText);
            return true;
        }

        private int ReadBytes(Encoding encoding, int byteBlock, int charBlock, byte[] buffer, int offset, int byteCount, bool readContent)
        {
            int num2;
            char[] charBuffer;
            int num3;
            int num8;
            if (this.trailByteCount > 0)
            {
                int length = Math.Min(this.trailByteCount, byteCount);
                Array.Copy(this.trailBytes, 0, buffer, offset, length);
                this.trailByteCount -= length;
                Array.Copy(this.trailBytes, length, this.trailBytes, 0, this.trailByteCount);
                return length;
            }
            switch (this.node.NodeType)
            {
                case XmlNodeType.Element:
                case XmlNodeType.EndElement:
                    return 0;

                default:
                    if (byteCount < byteBlock)
                    {
                        num2 = charBlock;
                    }
                    else
                    {
                        num2 = (byteCount / byteBlock) * charBlock;
                    }
                    charBuffer = this.GetCharBuffer(num2);
                    num3 = 0;
                    break;
            }
        Label_0083:
            if (this.trailCharCount > 0)
            {
                Array.Copy(this.trailChars, 0, charBuffer, num3, this.trailCharCount);
                num3 += this.trailCharCount;
                this.trailCharCount = 0;
            }
            while (num3 < charBlock)
            {
                int num4;
                if (readContent)
                {
                    num4 = this.ReadContentAsChars(charBuffer, num3, num2 - num3);
                }
                else
                {
                    num4 = this.ReadValueChunk(charBuffer, num3, num2 - num3);
                }
                if (num4 == 0)
                {
                    break;
                }
                num3 += num4;
            }
            if (num3 >= charBlock)
            {
                this.trailCharCount = num3 % charBlock;
                if (this.trailCharCount > 0)
                {
                    if (this.trailChars == null)
                    {
                        this.trailChars = new char[4];
                    }
                    num3 -= this.trailCharCount;
                    Array.Copy(charBuffer, num3, this.trailChars, 0, this.trailCharCount);
                }
            }
            try
            {
                if (byteCount < byteBlock)
                {
                    if (this.trailBytes == null)
                    {
                        this.trailBytes = new byte[3];
                    }
                    this.trailByteCount = encoding.GetBytes(charBuffer, 0, num3, this.trailBytes, 0);
                    int num5 = Math.Min(this.trailByteCount, byteCount);
                    Array.Copy(this.trailBytes, 0, buffer, offset, num5);
                    this.trailByteCount -= num5;
                    Array.Copy(this.trailBytes, num5, this.trailBytes, 0, this.trailByteCount);
                    return num5;
                }
                num8 = encoding.GetBytes(charBuffer, 0, num3, buffer, offset);
            }
            catch (FormatException exception)
            {
                int num6 = 0;
                int index = 0;
            Label_01D7:
                while ((index < num3) && XmlConverter.IsWhitespace(charBuffer[index]))
                {
                    index++;
                }
                if (index != num3)
                {
                    charBuffer[num6++] = charBuffer[index++];
                    goto Label_01D7;
                }
                if (num6 == num3)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(exception.Message, exception.InnerException));
                }
                num3 = num6;
                goto Label_0083;
            }
            return num8;
        }

        public override object ReadContentAs(Type type, IXmlNamespaceResolver namespaceResolver)
        {
            if (type == typeof(ulong))
            {
                if ((this.value == null) && this.node.IsAtomicValue)
                {
                    ulong num = this.node.Value.ToULong();
                    this.SkipValue(this.node);
                    return num;
                }
                return XmlConverter.ToUInt64(this.ReadContentAsString());
            }
            if (type == typeof(bool))
            {
                return this.ReadContentAsBoolean();
            }
            if (type == typeof(int))
            {
                return this.ReadContentAsInt();
            }
            if (type == typeof(long))
            {
                return this.ReadContentAsLong();
            }
            if (type == typeof(float))
            {
                return this.ReadContentAsFloat();
            }
            if (type == typeof(double))
            {
                return this.ReadContentAsDouble();
            }
            if (type == typeof(decimal))
            {
                return this.ReadContentAsDecimal();
            }
            if (type == typeof(DateTime))
            {
                return this.ReadContentAsDateTime();
            }
            if (type == typeof(UniqueId))
            {
                return this.ReadContentAsUniqueId();
            }
            if (type == typeof(Guid))
            {
                return this.ReadContentAsGuid();
            }
            if (type == typeof(TimeSpan))
            {
                return this.ReadContentAsTimeSpan();
            }
            if (type == typeof(object))
            {
                return this.ReadContentAsObject();
            }
            return base.ReadContentAs(type, namespaceResolver);
        }

        public override byte[] ReadContentAsBase64()
        {
            if (((this.trailByteCount == 0) && (this.trailCharCount == 0)) && (this.value == null))
            {
                XmlNode node = this.Node;
                if (node.IsAtomicValue)
                {
                    byte[] buffer = node.Value.ToByteArray();
                    if (buffer.Length > this.quotas.MaxArrayLength)
                    {
                        XmlExceptionHelper.ThrowMaxArrayLengthExceeded(this, this.quotas.MaxArrayLength);
                    }
                    this.SkipValue(node);
                    return buffer;
                }
            }
            if (!this.bufferReader.IsStreamed)
            {
                return base.ReadContentAsBase64(this.quotas.MaxArrayLength, this.bufferReader.Buffer.Length);
            }
            return base.ReadContentAsBase64(this.quotas.MaxArrayLength, 0xffff);
        }

        public override int ReadContentAsBase64(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
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
            if (count != 0)
            {
                if (((this.trailByteCount == 0) && (this.trailCharCount == 0)) && ((this.value == null) && (this.node.QNameType == QNameType.Normal)))
                {
                    int num;
                    while ((this.node.NodeType != XmlNodeType.Comment) && this.node.Value.TryReadBase64(buffer, offset, count, out num))
                    {
                        if (num != 0)
                        {
                            return num;
                        }
                        this.Read();
                    }
                }
                XmlNodeType nodeType = this.node.NodeType;
                if ((nodeType != XmlNodeType.Element) && (nodeType != XmlNodeType.EndElement))
                {
                    return this.ReadBytes(Base64Encoding, 3, 4, buffer, offset, Math.Min(count, 0x200), true);
                }
            }
            return 0;
        }

        public override byte[] ReadContentAsBinHex()
        {
            return base.ReadContentAsBinHex(this.quotas.MaxArrayLength);
        }

        public override int ReadContentAsBinHex(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
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
            if (count == 0)
            {
                return 0;
            }
            return this.ReadBytes(BinHexEncoding, 1, 2, buffer, offset, Math.Min(count, 0x200), true);
        }

        public override bool ReadContentAsBoolean()
        {
            XmlNode node = this.Node;
            if ((this.value == null) && node.IsAtomicValue)
            {
                bool flag = node.Value.ToBoolean();
                this.SkipValue(node);
                return flag;
            }
            return XmlConverter.ToBoolean(this.ReadContentAsString());
        }

        public override DateTime ReadContentAsDateTime()
        {
            XmlNode node = this.Node;
            if ((this.value == null) && node.IsAtomicValue)
            {
                DateTime time = node.Value.ToDateTime();
                this.SkipValue(node);
                return time;
            }
            return XmlConverter.ToDateTime(this.ReadContentAsString());
        }

        public override decimal ReadContentAsDecimal()
        {
            XmlNode node = this.Node;
            if ((this.value == null) && node.IsAtomicValue)
            {
                decimal num = node.Value.ToDecimal();
                this.SkipValue(node);
                return num;
            }
            return XmlConverter.ToDecimal(this.ReadContentAsString());
        }

        public override double ReadContentAsDouble()
        {
            XmlNode node = this.Node;
            if ((this.value == null) && node.IsAtomicValue)
            {
                double num = node.Value.ToDouble();
                this.SkipValue(node);
                return num;
            }
            return XmlConverter.ToDouble(this.ReadContentAsString());
        }

        public override float ReadContentAsFloat()
        {
            XmlNode node = this.Node;
            if ((this.value == null) && node.IsAtomicValue)
            {
                float num = node.Value.ToSingle();
                this.SkipValue(node);
                return num;
            }
            return XmlConverter.ToSingle(this.ReadContentAsString());
        }

        public override Guid ReadContentAsGuid()
        {
            XmlNode node = this.Node;
            if ((this.value == null) && node.IsAtomicValue)
            {
                Guid guid = node.Value.ToGuid();
                this.SkipValue(node);
                return guid;
            }
            return XmlConverter.ToGuid(this.ReadContentAsString());
        }

        public override int ReadContentAsInt()
        {
            XmlNode node = this.Node;
            if ((this.value == null) && node.IsAtomicValue)
            {
                int num = node.Value.ToInt();
                this.SkipValue(node);
                return num;
            }
            return XmlConverter.ToInt32(this.ReadContentAsString());
        }

        public override long ReadContentAsLong()
        {
            XmlNode node = this.Node;
            if ((this.value == null) && node.IsAtomicValue)
            {
                long num = node.Value.ToLong();
                this.SkipValue(node);
                return num;
            }
            return XmlConverter.ToInt64(this.ReadContentAsString());
        }

        public override object ReadContentAsObject()
        {
            XmlNode node = this.Node;
            if ((this.value == null) && node.IsAtomicValue)
            {
                object obj2 = node.Value.ToObject();
                this.SkipValue(node);
                return obj2;
            }
            return this.ReadContentAsString();
        }

        public override string ReadContentAsString()
        {
            string str;
            XmlNode node = this.Node;
            if (!node.IsAtomicValue)
            {
                return base.ReadContentAsString(this.quotas.MaxStringContentLength);
            }
            if (this.value != null)
            {
                str = this.value;
                if (node.AttributeText == null)
                {
                    this.value = string.Empty;
                }
                return str;
            }
            str = node.Value.GetString();
            this.SkipValue(node);
            if (str.Length > this.quotas.MaxStringContentLength)
            {
                XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, this.quotas.MaxStringContentLength);
            }
            return str;
        }

        public override TimeSpan ReadContentAsTimeSpan()
        {
            XmlNode node = this.Node;
            if ((this.value == null) && node.IsAtomicValue)
            {
                TimeSpan span = node.Value.ToTimeSpan();
                this.SkipValue(node);
                return span;
            }
            return XmlConverter.ToTimeSpan(this.ReadContentAsString());
        }

        public override UniqueId ReadContentAsUniqueId()
        {
            XmlNode node = this.Node;
            if ((this.value == null) && node.IsAtomicValue)
            {
                UniqueId id = node.Value.ToUniqueId();
                this.SkipValue(node);
                return id;
            }
            return XmlConverter.ToUniqueId(this.ReadContentAsString());
        }

        public override DateTime[] ReadDateTimeArray(string localName, string namespaceUri)
        {
            return DateTimeArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override DateTime[] ReadDateTimeArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return DateTimeArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override decimal[] ReadDecimalArray(string localName, string namespaceUri)
        {
            return DecimalArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override decimal[] ReadDecimalArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return DecimalArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override double[] ReadDoubleArray(string localName, string namespaceUri)
        {
            return DoubleArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override double[] ReadDoubleArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return DoubleArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int offset, int count)
        {
            if (!this.readingElement)
            {
                if (this.IsEmptyElement)
                {
                    this.Read();
                    return 0;
                }
                this.ReadStartElement();
                this.readingElement = true;
            }
            int num = this.ReadContentAsBase64(buffer, offset, count);
            if (num == 0)
            {
                this.ReadEndElement();
                this.readingElement = false;
            }
            return num;
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int offset, int count)
        {
            if (!this.readingElement)
            {
                if (this.IsEmptyElement)
                {
                    this.Read();
                    return 0;
                }
                this.ReadStartElement();
                this.readingElement = true;
            }
            int num = this.ReadContentAsBinHex(buffer, offset, count);
            if (num == 0)
            {
                this.ReadEndElement();
                this.readingElement = false;
            }
            return num;
        }

        public override string ReadElementContentAsString()
        {
            if (this.node.NodeType != XmlNodeType.Element)
            {
                this.MoveToStartElement();
            }
            if (this.node.IsEmptyElement)
            {
                this.Read();
                return string.Empty;
            }
            this.Read();
            string str = this.ReadContentAsString();
            this.ReadEndElement();
            return str;
        }

        public override string ReadElementString()
        {
            this.MoveToStartElement();
            if (this.IsEmptyElement)
            {
                this.Read();
                return string.Empty;
            }
            this.Read();
            string str = this.ReadString();
            this.ReadEndElement();
            return str;
        }

        public override string ReadElementString(string name)
        {
            this.MoveToStartElement(name);
            return this.ReadElementString();
        }

        public override string ReadElementString(string localName, string namespaceUri)
        {
            this.MoveToStartElement(localName, namespaceUri);
            return this.ReadElementString();
        }

        public override void ReadEndElement()
        {
            if ((this.node.NodeType != XmlNodeType.EndElement) && (this.MoveToContent() != XmlNodeType.EndElement))
            {
                int index = (this.node.NodeType == XmlNodeType.Element) ? (this.depth - 1) : this.depth;
                if (index == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlEndElementNoOpenNodes")));
                }
                XmlElementNode node = this.elementNodes[index];
                XmlExceptionHelper.ThrowEndElementExpected(this, node.LocalName.GetString(), node.Namespace.Uri.GetString());
            }
            this.Read();
        }

        public override Guid[] ReadGuidArray(string localName, string namespaceUri)
        {
            return GuidArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override Guid[] ReadGuidArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return GuidArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override short[] ReadInt16Array(string localName, string namespaceUri)
        {
            return Int16ArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override short[] ReadInt16Array(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return Int16ArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override int[] ReadInt32Array(string localName, string namespaceUri)
        {
            return Int32ArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override int[] ReadInt32Array(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return Int32ArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override long[] ReadInt64Array(string localName, string namespaceUri)
        {
            return Int64ArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override long[] ReadInt64Array(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return Int64ArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override float[] ReadSingleArray(string localName, string namespaceUri)
        {
            return SingleArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override float[] ReadSingleArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return SingleArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override void ReadStartElement()
        {
            if (this.node.NodeType != XmlNodeType.Element)
            {
                this.MoveToStartElement();
            }
            this.Read();
        }

        public override void ReadStartElement(string name)
        {
            this.MoveToStartElement(name);
            this.Read();
        }

        public override void ReadStartElement(string localName, string namespaceUri)
        {
            this.MoveToStartElement(localName, namespaceUri);
            this.Read();
        }

        public override TimeSpan[] ReadTimeSpanArray(string localName, string namespaceUri)
        {
            return TimeSpanArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override TimeSpan[] ReadTimeSpanArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return TimeSpanArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, this.quotas.MaxArrayLength);
        }

        public override int ReadValueAsBase64(byte[] buffer, int offset, int count)
        {
            int num;
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
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
            if (count == 0)
            {
                return 0;
            }
            if ((((this.value == null) && (this.trailByteCount == 0)) && ((this.trailCharCount == 0) && (this.node.QNameType == QNameType.Normal))) && this.node.Value.TryReadBase64(buffer, offset, count, out num))
            {
                return num;
            }
            return this.ReadBytes(Base64Encoding, 3, 4, buffer, offset, Math.Min(count, 0x200), false);
        }

        public override int ReadValueChunk(char[] chars, int offset, int count)
        {
            int num;
            if (chars == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
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
            if (((this.value != null) || (this.node.QNameType != QNameType.Normal)) || !this.node.Value.TryReadChars(chars, offset, count, out num))
            {
                string str = this.Value;
                num = Math.Min(count, str.Length);
                str.CopyTo(0, chars, offset, num);
                this.value = str.Substring(num);
            }
            return num;
        }

        public override void ResolveEntity()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlInvalidOperation")));
        }

        private void SignAttribute(XmlSigningNodeWriter writer, XmlAttributeNode attributeNode)
        {
            if (attributeNode.QNameType == QNameType.Normal)
            {
                int num;
                int num2;
                int num3;
                int num4;
                byte[] prefixBuffer = attributeNode.Prefix.GetString(out num, out num2);
                byte[] localNameBuffer = attributeNode.LocalName.GetString(out num3, out num4);
                writer.WriteStartAttribute(prefixBuffer, num, num2, localNameBuffer, num3, num4);
                attributeNode.Value.Sign(writer);
                writer.WriteEndAttribute();
            }
            else
            {
                int num5;
                int num6;
                int num7;
                int num8;
                byte[] buffer3 = attributeNode.Namespace.Prefix.GetString(out num5, out num6);
                byte[] nsBuffer = attributeNode.Namespace.Uri.GetString(out num7, out num8);
                writer.WriteXmlnsAttribute(buffer3, num5, num6, nsBuffer, num7, num8);
            }
        }

        private void SignEndElement(XmlSigningNodeWriter writer)
        {
            int num;
            int num2;
            int num3;
            int num4;
            byte[] prefixBuffer = this.node.Prefix.GetString(out num, out num2);
            byte[] localNameBuffer = this.node.LocalName.GetString(out num3, out num4);
            writer.WriteEndElement(prefixBuffer, num, num2, localNameBuffer, num3, num4);
        }

        protected void SignNode()
        {
            if (this.signing)
            {
                this.SignNode(this.signingWriter);
            }
        }

        private void SignNode(XmlSigningNodeWriter writer)
        {
            switch (this.node.NodeType)
            {
                case XmlNodeType.None:
                    return;

                case XmlNodeType.Element:
                    this.SignStartElement(writer);
                    for (int i = 0; i < this.attributeCount; i++)
                    {
                        this.SignAttribute(writer, this.attributeNodes[i]);
                    }
                    writer.WriteEndStartElement(this.node.IsEmptyElement);
                    return;

                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    this.node.Value.Sign(writer);
                    return;

                case XmlNodeType.Comment:
                    writer.WriteComment(this.node.Value.GetString());
                    return;

                case XmlNodeType.EndElement:
                    this.SignEndElement(writer);
                    return;

                case XmlNodeType.XmlDeclaration:
                    writer.WriteDeclaration();
                    return;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
        }

        private void SignStartElement(XmlSigningNodeWriter writer)
        {
            int num;
            int num2;
            int num3;
            int num4;
            byte[] prefixBuffer = this.node.Prefix.GetString(out num, out num2);
            byte[] localNameBuffer = this.node.LocalName.GetString(out num3, out num4);
            writer.WriteStartElement(prefixBuffer, num, num2, localNameBuffer, num3, num4);
        }

        public override void Skip()
        {
            if (this.node.ReadState == System.Xml.ReadState.Interactive)
            {
                if (((this.node.NodeType == XmlNodeType.Element) || this.MoveToElement()) && !this.IsEmptyElement)
                {
                    int depth = this.Depth;
                    while (this.Read() && (depth < this.Depth))
                    {
                    }
                    if (this.node.NodeType == XmlNodeType.EndElement)
                    {
                        this.Read();
                    }
                }
                else
                {
                    this.Read();
                }
            }
        }

        private void SkipValue(XmlNode node)
        {
            if (node.SkipValue)
            {
                this.Read();
            }
        }

        public override void StartCanonicalization(Stream stream, bool includeComments, string[] inclusivePrefixes)
        {
            if (this.signing)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlCanonicalizationStarted")));
            }
            if (this.signingWriter == null)
            {
                this.signingWriter = this.CreateSigningNodeWriter();
            }
            this.signingWriter.SetOutput(XmlNodeWriter.Null, stream, includeComments, inclusivePrefixes);
            this.nsMgr.Sign(this.signingWriter);
            this.signing = true;
        }

        public override bool TryGetBase64ContentLength(out int length)
        {
            if (((this.trailByteCount == 0) && (this.trailCharCount == 0)) && (this.value == null))
            {
                XmlNode node = this.Node;
                if (node.IsAtomicValue)
                {
                    return node.Value.TryGetByteArrayLength(out length);
                }
            }
            return base.TryGetBase64ContentLength(out length);
        }

        public override bool TryGetLocalNameAsDictionaryString(out XmlDictionaryString localName)
        {
            return this.node.TryGetLocalNameAsDictionaryString(out localName);
        }

        public override bool TryGetNamespaceUriAsDictionaryString(out XmlDictionaryString localName)
        {
            return this.node.TryGetNamespaceUriAsDictionaryString(out localName);
        }

        public override bool TryGetValueAsDictionaryString(out XmlDictionaryString value)
        {
            return this.node.TryGetValueAsDictionaryString(out value);
        }

        public override int AttributeCount
        {
            get
            {
                if (this.node.CanGetAttribute)
                {
                    return this.attributeCount;
                }
                return 0;
            }
        }

        private static System.Text.Base64Encoding Base64Encoding
        {
            get
            {
                if (base64Encoding == null)
                {
                    base64Encoding = new System.Text.Base64Encoding();
                }
                return base64Encoding;
            }
        }

        public override string BaseURI
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return string.Empty;
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

        protected XmlBufferReader BufferReader
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.bufferReader;
            }
        }

        public override bool CanCanonicalize
        {
            get
            {
                return true;
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

        public sealed override int Depth
        {
            get
            {
                return (this.depth + this.node.DepthDelta);
            }
        }

        protected XmlElementNode ElementNode
        {
            get
            {
                if (this.depth == 0)
                {
                    return this.rootElementNode;
                }
                return this.elementNodes[this.depth];
            }
        }

        public override bool EOF
        {
            get
            {
                return (this.node.ReadState == System.Xml.ReadState.EndOfFile);
            }
        }

        public override bool HasValue
        {
            get
            {
                return this.node.HasValue;
            }
        }

        public override bool IsDefault
        {
            get
            {
                return false;
            }
        }

        public sealed override bool IsEmptyElement
        {
            get
            {
                return this.node.IsEmptyElement;
            }
        }

        public override string this[int index]
        {
            get
            {
                return this.GetAttribute(index);
            }
        }

        public override string this[string name]
        {
            get
            {
                return this.GetAttribute(name);
            }
        }

        public override string this[string localName, string namespaceUri]
        {
            get
            {
                return this.GetAttribute(localName, namespaceUri);
            }
        }

        public override string LocalName
        {
            get
            {
                if (this.localName == null)
                {
                    this.localName = this.GetLocalName(true);
                }
                return this.localName;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                if (this.ns == null)
                {
                    this.ns = this.GetNamespaceUri(true);
                }
                return this.ns;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                if (this.nameTable == null)
                {
                    this.nameTable = new QuotaNameTable(this, this.quotas.MaxNameTableCharCount);
                    this.nameTable.Add("xml");
                    this.nameTable.Add("xmlns");
                    this.nameTable.Add("http://www.w3.org/2000/xmlns/");
                    this.nameTable.Add("http://www.w3.org/XML/1998/namespace");
                    for (PrefixHandleType type = PrefixHandleType.A; type <= PrefixHandleType.Z; type += 1)
                    {
                        this.nameTable.Add(PrefixHandle.GetString(type));
                    }
                }
                return this.nameTable;
            }
        }

        protected XmlNode Node
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.node;
            }
        }

        public sealed override XmlNodeType NodeType
        {
            get
            {
                return this.node.NodeType;
            }
        }

        protected bool OutsideRootElement
        {
            get
            {
                return (this.depth == 0);
            }
        }

        public override string Prefix
        {
            get
            {
                if (this.prefix == null)
                {
                    switch (this.node.QNameType)
                    {
                        case QNameType.Normal:
                            this.prefix = this.node.Prefix.GetString(this.NameTable);
                            goto Label_0075;

                        case QNameType.Xmlns:
                            if (this.node.Namespace.Prefix.IsEmpty)
                            {
                                this.prefix = string.Empty;
                            }
                            else
                            {
                                this.prefix = "xmlns";
                            }
                            goto Label_0075;
                    }
                    this.prefix = "xml";
                }
            Label_0075:
                return this.prefix;
            }
        }

        public override XmlDictionaryReaderQuotas Quotas
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.quotas;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return this.node.QuoteChar;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                return this.node.ReadState;
            }
        }

        protected bool Signing
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.signing;
            }
        }

        public override string Value
        {
            get
            {
                if (this.value == null)
                {
                    this.value = this.node.ValueAsString;
                }
                return this.value;
            }
        }

        public override Type ValueType
        {
            get
            {
                if ((this.value == null) && (this.node.QNameType == QNameType.Normal))
                {
                    Type type = this.node.Value.ToType();
                    if (this.node.IsAtomicValue)
                    {
                        return type;
                    }
                    if (type == typeof(byte[]))
                    {
                        return type;
                    }
                }
                return typeof(string);
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

        private class AttributeSorter : IComparer
        {
            private int attributeCount;
            private int attributeIndex1;
            private int attributeIndex2;
            private XmlBaseReader.XmlAttributeNode[] attributeNodes;
            private object[] indeces;

            public void Close()
            {
                if ((this.indeces != null) && (this.indeces.Length > 0x20))
                {
                    this.indeces = null;
                }
            }

            public int Compare(object obj1, object obj2)
            {
                int index = (int) obj1;
                int num2 = (int) obj2;
                XmlBaseReader.XmlAttributeNode node = this.attributeNodes[index];
                XmlBaseReader.XmlAttributeNode node2 = this.attributeNodes[num2];
                int num3 = this.CompareQNameType(node.QNameType, node2.QNameType);
                if (num3 != 0)
                {
                    return num3;
                }
                if (node.QNameType == XmlBaseReader.QNameType.Normal)
                {
                    num3 = node.LocalName.CompareTo(node2.LocalName);
                    if (num3 == 0)
                    {
                        num3 = node.Namespace.Uri.CompareTo(node2.Namespace.Uri);
                    }
                    return num3;
                }
                return node.Namespace.Prefix.CompareTo(node2.Namespace.Prefix);
            }

            public int CompareQNameType(XmlBaseReader.QNameType type1, XmlBaseReader.QNameType type2)
            {
                return (int) (type1 - type2);
            }

            public void GetIndeces(out int attributeIndex1, out int attributeIndex2)
            {
                attributeIndex1 = this.attributeIndex1;
                attributeIndex2 = this.attributeIndex2;
            }

            private bool IsSorted()
            {
                for (int i = 0; i < (this.indeces.Length - 1); i++)
                {
                    if (this.Compare(this.indeces[i], this.indeces[i + 1]) >= 0)
                    {
                        this.attributeIndex1 = (int) this.indeces[i];
                        this.attributeIndex2 = (int) this.indeces[i + 1];
                        return false;
                    }
                }
                return true;
            }

            private bool Sort()
            {
                if (((this.indeces != null) && (this.indeces.Length == this.attributeCount)) && this.IsSorted())
                {
                    return true;
                }
                object[] objArray = new object[this.attributeCount];
                for (int i = 0; i < objArray.Length; i++)
                {
                    objArray[i] = i;
                }
                this.indeces = objArray;
                Array.Sort(this.indeces, 0, this.attributeCount, this);
                return this.IsSorted();
            }

            public bool Sort(XmlBaseReader.XmlAttributeNode[] attributeNodes, int attributeCount)
            {
                this.attributeIndex1 = -1;
                this.attributeIndex2 = -1;
                this.attributeNodes = attributeNodes;
                this.attributeCount = attributeCount;
                bool flag = this.Sort();
                this.attributeNodes = null;
                this.attributeCount = 0;
                return flag;
            }
        }

        protected class Namespace
        {
            private int depth;
            private XmlBaseReader.Namespace outerUri;
            private PrefixHandle prefix;
            private StringHandle uri;
            private string uriString;

            public Namespace(XmlBufferReader bufferReader)
            {
                this.prefix = new PrefixHandle(bufferReader);
                this.uri = new StringHandle(bufferReader);
                this.outerUri = null;
                this.uriString = null;
            }

            public void Clear()
            {
                this.uriString = null;
            }

            public bool IsUri(string s)
            {
                if (object.ReferenceEquals(s, this.uriString))
                {
                    return true;
                }
                if (this.uri == s)
                {
                    this.uriString = s;
                    return true;
                }
                return false;
            }

            public bool IsUri(XmlDictionaryString s)
            {
                if (object.ReferenceEquals(s.Value, this.uriString))
                {
                    return true;
                }
                if (this.uri == s)
                {
                    this.uriString = s.Value;
                    return true;
                }
                return false;
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

            public XmlBaseReader.Namespace OuterUri
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.outerUri;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.outerUri = value;
                }
            }

            public PrefixHandle Prefix
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.prefix;
                }
            }

            public StringHandle Uri
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.uri;
                }
            }
        }

        private class NamespaceManager
        {
            private int attributeCount;
            private XmlAttribute[] attributes;
            private XmlBufferReader bufferReader;
            private int depth;
            private static XmlBaseReader.Namespace emptyNamespace = new XmlBaseReader.Namespace(XmlBufferReader.Empty);
            private string lang;
            private XmlBaseReader.Namespace[] namespaces;
            private int nsCount;
            private XmlBaseReader.Namespace[] shortPrefixUri;
            private System.Xml.XmlSpace space;
            private static XmlBaseReader.Namespace xmlNamespace;

            public NamespaceManager(XmlBufferReader bufferReader)
            {
                this.bufferReader = bufferReader;
                this.shortPrefixUri = new XmlBaseReader.Namespace[0x1c];
                this.shortPrefixUri[0] = emptyNamespace;
                this.namespaces = null;
                this.nsCount = 0;
                this.attributes = null;
                this.attributeCount = 0;
                this.space = System.Xml.XmlSpace.None;
                this.lang = string.Empty;
                this.depth = 0;
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

            public XmlBaseReader.Namespace AddNamespace()
            {
                if (this.namespaces == null)
                {
                    this.namespaces = new XmlBaseReader.Namespace[4];
                }
                else if (this.namespaces.Length == this.nsCount)
                {
                    XmlBaseReader.Namespace[] destinationArray = new XmlBaseReader.Namespace[this.nsCount * 2];
                    Array.Copy(this.namespaces, destinationArray, this.nsCount);
                    this.namespaces = destinationArray;
                }
                XmlBaseReader.Namespace namespace2 = this.namespaces[this.nsCount];
                if (namespace2 == null)
                {
                    namespace2 = new XmlBaseReader.Namespace(this.bufferReader);
                    this.namespaces[this.nsCount] = namespace2;
                }
                namespace2.Clear();
                namespace2.Depth = this.depth;
                this.nsCount++;
                return namespace2;
            }

            public void AddSpaceAttribute(System.Xml.XmlSpace space)
            {
                this.AddAttribute();
                this.space = space;
            }

            public void Clear()
            {
                if (this.nsCount != 0)
                {
                    if (this.shortPrefixUri != null)
                    {
                        for (int i = 0; i < this.shortPrefixUri.Length; i++)
                        {
                            this.shortPrefixUri[i] = null;
                        }
                    }
                    this.shortPrefixUri[0] = emptyNamespace;
                    this.nsCount = 0;
                }
                this.attributeCount = 0;
                this.space = System.Xml.XmlSpace.None;
                this.lang = string.Empty;
                this.depth = 0;
            }

            public void Close()
            {
                if ((this.namespaces != null) && (this.namespaces.Length > 0x20))
                {
                    this.namespaces = null;
                }
                if ((this.attributes != null) && (this.attributes.Length > 4))
                {
                    this.attributes = null;
                }
                this.lang = string.Empty;
            }

            public void EnterScope()
            {
                this.depth++;
            }

            public void ExitScope()
            {
                while (this.nsCount > 0)
                {
                    PrefixHandleType type;
                    XmlBaseReader.Namespace namespace2 = this.namespaces[this.nsCount - 1];
                    if (namespace2.Depth != this.depth)
                    {
                        break;
                    }
                    if (namespace2.Prefix.TryGetShortPrefix(out type))
                    {
                        this.shortPrefixUri[(int) type] = namespace2.OuterUri;
                    }
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
                    this.attributeCount--;
                }
                this.depth--;
            }

            public XmlBaseReader.Namespace LookupNamespace(string prefix)
            {
                PrefixHandleType type;
                if (this.TryGetShortPrefix(prefix, out type))
                {
                    return this.LookupNamespace(type);
                }
                for (int i = this.nsCount - 1; i >= 0; i--)
                {
                    XmlBaseReader.Namespace namespace2 = this.namespaces[i];
                    if (namespace2.Prefix == prefix)
                    {
                        return namespace2;
                    }
                }
                if (prefix == "xml")
                {
                    return XmlNamespace;
                }
                return null;
            }

            public XmlBaseReader.Namespace LookupNamespace(PrefixHandle prefix)
            {
                PrefixHandleType type;
                if (prefix.TryGetShortPrefix(out type))
                {
                    return this.LookupNamespace(type);
                }
                for (int i = this.nsCount - 1; i >= 0; i--)
                {
                    XmlBaseReader.Namespace namespace2 = this.namespaces[i];
                    if (namespace2.Prefix == prefix)
                    {
                        return namespace2;
                    }
                }
                if (prefix.IsXml)
                {
                    return XmlNamespace;
                }
                return null;
            }

            public XmlBaseReader.Namespace LookupNamespace(PrefixHandleType prefix)
            {
                return this.shortPrefixUri[(int) prefix];
            }

            public void Register(XmlBaseReader.Namespace nameSpace)
            {
                PrefixHandleType type;
                if (nameSpace.Prefix.TryGetShortPrefix(out type))
                {
                    nameSpace.OuterUri = this.shortPrefixUri[(int) type];
                    this.shortPrefixUri[(int) type] = nameSpace;
                }
                else
                {
                    nameSpace.OuterUri = null;
                }
            }

            public void Sign(XmlSigningNodeWriter writer)
            {
                for (int i = 0; i < this.nsCount; i++)
                {
                    PrefixHandle prefix = this.namespaces[i].Prefix;
                    bool flag = false;
                    for (int j = i + 1; j < this.nsCount; j++)
                    {
                        if (object.Equals(prefix, this.namespaces[j].Prefix))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        int num3;
                        int num4;
                        int num5;
                        int num6;
                        byte[] prefixBuffer = prefix.GetString(out num3, out num4);
                        byte[] nsBuffer = this.namespaces[i].Uri.GetString(out num5, out num6);
                        writer.WriteXmlnsAttribute(prefixBuffer, num3, num4, nsBuffer, num5, num6);
                    }
                }
            }

            private bool TryGetShortPrefix(string s, out PrefixHandleType shortPrefix)
            {
                switch (s.Length)
                {
                    case 0:
                        shortPrefix = PrefixHandleType.Empty;
                        return true;

                    case 1:
                    {
                        char ch = s[0];
                        if ((ch >= 'a') && (ch <= 'z'))
                        {
                            shortPrefix = PrefixHandle.GetAlphaPrefix(ch - 'a');
                            return true;
                        }
                        break;
                    }
                }
                shortPrefix = PrefixHandleType.Empty;
                return false;
            }

            public static XmlBaseReader.Namespace EmptyNamespace
            {
                get
                {
                    return emptyNamespace;
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

            public static XmlBaseReader.Namespace XmlNamespace
            {
                get
                {
                    if (xmlNamespace == null)
                    {
                        byte[] buffer = new byte[] { 
                            120, 0x6d, 0x6c, 0x68, 0x74, 0x74, 0x70, 0x3a, 0x2f, 0x2f, 0x77, 0x77, 0x77, 0x2e, 0x77, 0x33, 
                            0x2e, 0x6f, 0x72, 0x67, 0x2f, 0x58, 0x4d, 0x4c, 0x2f, 0x31, 0x39, 0x39, 0x38, 0x2f, 110, 0x61, 
                            0x6d, 0x65, 0x73, 0x70, 0x61, 0x63, 0x65
                         };
                        XmlBaseReader.Namespace namespace2 = new XmlBaseReader.Namespace(new XmlBufferReader(buffer));
                        namespace2.Prefix.SetValue(0, 3);
                        namespace2.Uri.SetValue(3, buffer.Length - 3);
                        xmlNamespace = namespace2;
                    }
                    return xmlNamespace;
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

            private class XmlAttribute
            {
                private int depth;
                private string lang;
                private System.Xml.XmlSpace space;

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

        protected enum QNameType
        {
            Normal,
            Xmlns
        }

        private class QuotaNameTable : XmlNameTable
        {
            private int charCount;
            private int maxCharCount;
            private XmlNameTable nameTable;
            private XmlDictionaryReader reader;

            public QuotaNameTable(XmlDictionaryReader reader, int maxCharCount)
            {
                this.reader = reader;
                this.nameTable = new NameTable();
                this.maxCharCount = maxCharCount;
                this.charCount = 0;
            }

            private void Add(int charCount)
            {
                if (charCount > (this.maxCharCount - this.charCount))
                {
                    XmlExceptionHelper.ThrowMaxNameTableCharCountExceeded(this.reader, this.maxCharCount);
                }
                this.charCount += charCount;
            }

            public override string Add(string value)
            {
                string str = this.nameTable.Get(value);
                if (str != null)
                {
                    return str;
                }
                this.Add(value.Length);
                return this.nameTable.Add(value);
            }

            public override string Add(char[] chars, int offset, int count)
            {
                string str = this.nameTable.Get(chars, offset, count);
                if (str != null)
                {
                    return str;
                }
                this.Add(count);
                return this.nameTable.Add(chars, offset, count);
            }

            public override string Get(string value)
            {
                return this.nameTable.Get(value);
            }

            public override string Get(char[] chars, int offset, int count)
            {
                return this.nameTable.Get(chars, offset, count);
            }
        }

        protected class XmlAtomicTextNode : XmlBaseReader.XmlTextNode
        {
            public XmlAtomicTextNode(XmlBufferReader bufferReader) : base(XmlNodeType.Text, new PrefixHandle(bufferReader), new StringHandle(bufferReader), new ValueHandle(bufferReader), XmlBaseReader.XmlNode.XmlNodeFlags.HasContent | XmlBaseReader.XmlNode.XmlNodeFlags.SkipValue | XmlBaseReader.XmlNode.XmlNodeFlags.AtomicValue | XmlBaseReader.XmlNode.XmlNodeFlags.HasValue, System.Xml.ReadState.Interactive, null, 0)
            {
            }
        }

        protected class XmlAttributeNode : XmlBaseReader.XmlNode
        {
            public XmlAttributeNode(XmlBufferReader bufferReader) : this(new PrefixHandle(bufferReader), new StringHandle(bufferReader), new ValueHandle(bufferReader))
            {
            }

            private XmlAttributeNode(PrefixHandle prefix, StringHandle localName, ValueHandle value) : base(XmlNodeType.Attribute, prefix, localName, value, XmlBaseReader.XmlNode.XmlNodeFlags.AtomicValue | XmlBaseReader.XmlNode.XmlNodeFlags.HasValue | XmlBaseReader.XmlNode.XmlNodeFlags.CanMoveToElement | XmlBaseReader.XmlNode.XmlNodeFlags.CanGetAttribute, System.Xml.ReadState.Interactive, new XmlBaseReader.XmlAttributeTextNode(prefix, localName, value), 0)
            {
            }
        }

        protected class XmlAttributeTextNode : XmlBaseReader.XmlTextNode
        {
            public XmlAttributeTextNode(PrefixHandle prefix, StringHandle localName, ValueHandle value) : base(XmlNodeType.Text, prefix, localName, value, XmlBaseReader.XmlNode.XmlNodeFlags.HasContent | XmlBaseReader.XmlNode.XmlNodeFlags.AtomicValue | XmlBaseReader.XmlNode.XmlNodeFlags.HasValue | XmlBaseReader.XmlNode.XmlNodeFlags.CanMoveToElement | XmlBaseReader.XmlNode.XmlNodeFlags.CanGetAttribute, System.Xml.ReadState.Interactive, null, 1)
            {
            }
        }

        protected class XmlCDataNode : XmlBaseReader.XmlTextNode
        {
            public XmlCDataNode(XmlBufferReader bufferReader) : base(XmlNodeType.CDATA, new PrefixHandle(bufferReader), new StringHandle(bufferReader), new ValueHandle(bufferReader), XmlBaseReader.XmlNode.XmlNodeFlags.HasContent | XmlBaseReader.XmlNode.XmlNodeFlags.HasValue, System.Xml.ReadState.Interactive, null, 0)
            {
            }
        }

        protected class XmlClosedNode : XmlBaseReader.XmlNode
        {
            public XmlClosedNode(XmlBufferReader bufferReader) : base(XmlNodeType.None, new PrefixHandle(bufferReader), new StringHandle(bufferReader), new ValueHandle(bufferReader), XmlBaseReader.XmlNode.XmlNodeFlags.None, System.Xml.ReadState.Closed, null, 0)
            {
            }
        }

        protected class XmlCommentNode : XmlBaseReader.XmlNode
        {
            public XmlCommentNode(XmlBufferReader bufferReader) : base(XmlNodeType.Comment, new PrefixHandle(bufferReader), new StringHandle(bufferReader), new ValueHandle(bufferReader), XmlBaseReader.XmlNode.XmlNodeFlags.HasValue, System.Xml.ReadState.Interactive, null, 0)
            {
            }
        }

        protected class XmlComplexTextNode : XmlBaseReader.XmlTextNode
        {
            public XmlComplexTextNode(XmlBufferReader bufferReader) : base(XmlNodeType.Text, new PrefixHandle(bufferReader), new StringHandle(bufferReader), new ValueHandle(bufferReader), XmlBaseReader.XmlNode.XmlNodeFlags.HasContent | XmlBaseReader.XmlNode.XmlNodeFlags.HasValue, System.Xml.ReadState.Interactive, null, 0)
            {
            }
        }

        protected class XmlDeclarationNode : XmlBaseReader.XmlNode
        {
            public XmlDeclarationNode(XmlBufferReader bufferReader) : base(XmlNodeType.XmlDeclaration, new PrefixHandle(bufferReader), new StringHandle(bufferReader), new ValueHandle(bufferReader), XmlBaseReader.XmlNode.XmlNodeFlags.CanGetAttribute, System.Xml.ReadState.Interactive, null, 0)
            {
            }
        }

        protected class XmlElementNode : XmlBaseReader.XmlNode
        {
            private int bufferOffset;
            private XmlBaseReader.XmlEndElementNode endElementNode;
            public int NameLength;
            public int NameOffset;

            public XmlElementNode(XmlBufferReader bufferReader) : this(new PrefixHandle(bufferReader), new StringHandle(bufferReader), new ValueHandle(bufferReader))
            {
            }

            private XmlElementNode(PrefixHandle prefix, StringHandle localName, ValueHandle value) : base(XmlNodeType.Element, prefix, localName, value, XmlBaseReader.XmlNode.XmlNodeFlags.HasContent | XmlBaseReader.XmlNode.XmlNodeFlags.CanGetAttribute, System.Xml.ReadState.Interactive, null, -1)
            {
                this.endElementNode = new XmlBaseReader.XmlEndElementNode(prefix, localName, value);
            }

            public int BufferOffset
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.bufferOffset;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.bufferOffset = value;
                }
            }

            public XmlBaseReader.XmlEndElementNode EndElement
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.endElementNode;
                }
            }
        }

        protected class XmlEndElementNode : XmlBaseReader.XmlNode
        {
            public XmlEndElementNode(PrefixHandle prefix, StringHandle localName, ValueHandle value) : base(XmlNodeType.EndElement, prefix, localName, value, XmlBaseReader.XmlNode.XmlNodeFlags.HasContent, System.Xml.ReadState.Interactive, null, -1)
            {
            }
        }

        protected class XmlEndOfFileNode : XmlBaseReader.XmlNode
        {
            public XmlEndOfFileNode(XmlBufferReader bufferReader) : base(XmlNodeType.None, new PrefixHandle(bufferReader), new StringHandle(bufferReader), new ValueHandle(bufferReader), XmlBaseReader.XmlNode.XmlNodeFlags.None, System.Xml.ReadState.EndOfFile, null, 0)
            {
            }
        }

        protected class XmlInitialNode : XmlBaseReader.XmlNode
        {
            public XmlInitialNode(XmlBufferReader bufferReader) : base(XmlNodeType.None, new PrefixHandle(bufferReader), new StringHandle(bufferReader), new ValueHandle(bufferReader), XmlBaseReader.XmlNode.XmlNodeFlags.None, System.Xml.ReadState.Initial, null, 0)
            {
            }
        }

        protected class XmlNode
        {
            private XmlBaseReader.XmlAttributeTextNode attributeTextNode;
            private bool canGetAttribute;
            private bool canMoveToElement;
            private int depthDelta;
            private bool exitScope;
            private bool hasContent;
            private bool hasValue;
            private bool isAtomicValue;
            private bool isEmptyElement;
            private StringHandle localName;
            private XmlNodeType nodeType;
            private System.Xml.XmlBaseReader.Namespace ns;
            private PrefixHandle prefix;
            private System.Xml.XmlBaseReader.QNameType qnameType;
            private char quoteChar;
            private System.Xml.ReadState readState;
            private bool skipValue;
            private ValueHandle value;

            protected XmlNode(XmlNodeType nodeType, PrefixHandle prefix, StringHandle localName, ValueHandle value, XmlNodeFlags nodeFlags, System.Xml.ReadState readState, XmlBaseReader.XmlAttributeTextNode attributeTextNode, int depthDelta)
            {
                this.nodeType = nodeType;
                this.prefix = prefix;
                this.localName = localName;
                this.value = value;
                this.ns = XmlBaseReader.NamespaceManager.EmptyNamespace;
                this.hasValue = (nodeFlags & XmlNodeFlags.HasValue) != XmlNodeFlags.None;
                this.canGetAttribute = (nodeFlags & XmlNodeFlags.CanGetAttribute) != XmlNodeFlags.None;
                this.canMoveToElement = (nodeFlags & XmlNodeFlags.CanMoveToElement) != XmlNodeFlags.None;
                this.isAtomicValue = (nodeFlags & XmlNodeFlags.AtomicValue) != XmlNodeFlags.None;
                this.skipValue = (nodeFlags & XmlNodeFlags.SkipValue) != XmlNodeFlags.None;
                this.hasContent = (nodeFlags & XmlNodeFlags.HasContent) != XmlNodeFlags.None;
                this.readState = readState;
                this.attributeTextNode = attributeTextNode;
                this.exitScope = nodeType == XmlNodeType.EndElement;
                this.depthDelta = depthDelta;
                this.isEmptyElement = false;
                this.quoteChar = '"';
                this.qnameType = System.Xml.XmlBaseReader.QNameType.Normal;
            }

            public bool IsLocalName(string localName)
            {
                if (this.qnameType == System.Xml.XmlBaseReader.QNameType.Normal)
                {
                    return (this.LocalName == localName);
                }
                return (this.Namespace.Prefix == localName);
            }

            public bool IsLocalName(XmlDictionaryString localName)
            {
                if (this.qnameType == System.Xml.XmlBaseReader.QNameType.Normal)
                {
                    return (this.LocalName == localName);
                }
                return (this.Namespace.Prefix == localName);
            }

            public bool IsLocalNameAndNamespaceUri(string localName, string ns)
            {
                if (this.qnameType == System.Xml.XmlBaseReader.QNameType.Normal)
                {
                    return ((this.LocalName == localName) && this.Namespace.IsUri(ns));
                }
                return ((this.Namespace.Prefix == localName) && (ns == "http://www.w3.org/2000/xmlns/"));
            }

            public bool IsLocalNameAndNamespaceUri(XmlDictionaryString localName, XmlDictionaryString ns)
            {
                if (this.qnameType == System.Xml.XmlBaseReader.QNameType.Normal)
                {
                    return ((this.LocalName == localName) && this.Namespace.IsUri(ns));
                }
                return ((this.Namespace.Prefix == localName) && (ns.Value == "http://www.w3.org/2000/xmlns/"));
            }

            public bool IsNamespaceUri(string ns)
            {
                if (this.qnameType == System.Xml.XmlBaseReader.QNameType.Normal)
                {
                    return this.Namespace.IsUri(ns);
                }
                return (ns == "http://www.w3.org/2000/xmlns/");
            }

            public bool IsNamespaceUri(XmlDictionaryString ns)
            {
                if (this.qnameType == System.Xml.XmlBaseReader.QNameType.Normal)
                {
                    return this.Namespace.IsUri(ns);
                }
                return (ns.Value == "http://www.w3.org/2000/xmlns/");
            }

            public bool IsPrefixAndLocalName(string prefix, string localName)
            {
                if (this.qnameType == System.Xml.XmlBaseReader.QNameType.Normal)
                {
                    return ((this.Prefix == prefix) && (this.LocalName == localName));
                }
                return ((prefix == "xmlns") && (this.Namespace.Prefix == localName));
            }

            public bool TryGetLocalNameAsDictionaryString(out XmlDictionaryString localName)
            {
                if (this.qnameType == System.Xml.XmlBaseReader.QNameType.Normal)
                {
                    return this.LocalName.TryGetDictionaryString(out localName);
                }
                localName = null;
                return false;
            }

            public bool TryGetNamespaceUriAsDictionaryString(out XmlDictionaryString ns)
            {
                if (this.qnameType == System.Xml.XmlBaseReader.QNameType.Normal)
                {
                    return this.Namespace.Uri.TryGetDictionaryString(out ns);
                }
                ns = null;
                return false;
            }

            public bool TryGetValueAsDictionaryString(out XmlDictionaryString value)
            {
                if (this.qnameType == System.Xml.XmlBaseReader.QNameType.Normal)
                {
                    return this.Value.TryGetDictionaryString(out value);
                }
                value = null;
                return false;
            }

            public XmlBaseReader.XmlAttributeTextNode AttributeText
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.attributeTextNode;
                }
            }

            public bool CanGetAttribute
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.canGetAttribute;
                }
            }

            public bool CanMoveToElement
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.canMoveToElement;
                }
            }

            public int DepthDelta
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.depthDelta;
                }
            }

            public bool ExitScope
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.exitScope;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.exitScope = value;
                }
            }

            public bool HasContent
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.hasContent;
                }
            }

            public bool HasValue
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.hasValue;
                }
            }

            public bool IsAtomicValue
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.isAtomicValue;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.isAtomicValue = value;
                }
            }

            public bool IsEmptyElement
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.isEmptyElement;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.isEmptyElement = value;
                }
            }

            public StringHandle LocalName
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.localName;
                }
            }

            public System.Xml.XmlBaseReader.Namespace Namespace
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

            public XmlNodeType NodeType
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.nodeType;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.nodeType = value;
                }
            }

            public PrefixHandle Prefix
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.prefix;
                }
            }

            public System.Xml.XmlBaseReader.QNameType QNameType
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.qnameType;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.qnameType = value;
                }
            }

            public char QuoteChar
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.quoteChar;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.quoteChar = value;
                }
            }

            public System.Xml.ReadState ReadState
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.readState;
                }
            }

            public bool SkipValue
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.skipValue;
                }
            }

            public ValueHandle Value
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.value;
                }
            }

            public string ValueAsString
            {
                get
                {
                    if (this.qnameType == System.Xml.XmlBaseReader.QNameType.Normal)
                    {
                        return this.Value.GetString();
                    }
                    return this.Namespace.Uri.GetString();
                }
            }

            protected enum XmlNodeFlags
            {
                AtomicValue = 8,
                CanGetAttribute = 1,
                CanMoveToElement = 2,
                HasContent = 0x20,
                HasValue = 4,
                None = 0,
                SkipValue = 0x10
            }
        }

        protected class XmlTextNode : XmlBaseReader.XmlNode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            protected XmlTextNode(XmlNodeType nodeType, PrefixHandle prefix, StringHandle localName, ValueHandle value, XmlBaseReader.XmlNode.XmlNodeFlags nodeFlags, System.Xml.ReadState readState, XmlBaseReader.XmlAttributeTextNode attributeTextNode, int depthDelta) : base(nodeType, prefix, localName, value, nodeFlags, readState, attributeTextNode, depthDelta)
            {
            }
        }

        protected class XmlWhitespaceTextNode : XmlBaseReader.XmlTextNode
        {
            public XmlWhitespaceTextNode(XmlBufferReader bufferReader) : base(XmlNodeType.Whitespace, new PrefixHandle(bufferReader), new StringHandle(bufferReader), new ValueHandle(bufferReader), XmlBaseReader.XmlNode.XmlNodeFlags.HasValue, System.Xml.ReadState.Interactive, null, 0)
            {
            }
        }
    }
}

