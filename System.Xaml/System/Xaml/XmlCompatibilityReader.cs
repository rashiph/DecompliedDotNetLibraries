namespace System.Xaml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Xml;

    internal sealed class XmlCompatibilityReader : System.Xaml.XmlWrappingReader
    {
        private string _alternateContent;
        private Dictionary<string, HandleAttributeCallback> _attributeHandler;
        private int _attributePosition;
        private string _choice;
        private CompatibilityScope _compatibilityScope;
        private string _compatibilityUri;
        private string _currentName;
        private int _depthOffset;
        private Dictionary<string, HandleElementCallback> _elementHandler;
        private string _fallback;
        private string _ignorable;
        private int _ignoredAttributeCount;
        private bool _inAttribute;
        private Dictionary<string, object> _knownNamespaces;
        private string _mustUnderstand;
        private IsXmlNamespaceSupportedCallback _namespaceCallback;
        private Dictionary<string, string> _namespaceMap;
        private static string[] _predefinedNamespaces = new string[] { "http://www.w3.org/2000/xmlns/", "http://www.w3.org/XML/1998/namespace", "http://www.w3.org/2001/XMLSchema-instance", "http://schemas.openxmlformats.org/markup-compatibility/2006" };
        private string _preserveAttributes;
        private string _preserveElements;
        private string _processContent;
        private string _requires;
        private Dictionary<string, object> _subsumingNamespaces;
        private bool isPreviousElementEmpty;
        private const string MarkupCompatibilityURI = "http://schemas.openxmlformats.org/markup-compatibility/2006";
        private int previousElementDepth;
        private const string XmlnsDeclaration = "xmlns";

        public XmlCompatibilityReader(XmlReader baseReader) : base(baseReader)
        {
            this._namespaceMap = new Dictionary<string, string>();
            this._elementHandler = new Dictionary<string, HandleElementCallback>();
            this._attributeHandler = new Dictionary<string, HandleAttributeCallback>();
            this._compatibilityScope = new CompatibilityScope(null, -1, this);
            foreach (string str in _predefinedNamespaces)
            {
                this.AddKnownNamespace(str);
                this._namespaceMap[str] = str;
                base.Reader.NameTable.Add(str);
            }
            this._elementHandler.Add(this.AlternateContent, new HandleElementCallback(this.HandleAlternateContent));
            this._elementHandler.Add(this.Choice, new HandleElementCallback(this.HandleChoice));
            this._elementHandler.Add(this.Fallback, new HandleElementCallback(this.HandleFallback));
            this._attributeHandler.Add(this.Ignorable, new HandleAttributeCallback(this.HandleIgnorable));
            this._attributeHandler.Add(this.MustUnderstand, new HandleAttributeCallback(this.HandleMustUnderstand));
            this._attributeHandler.Add(this.ProcessContent, new HandleAttributeCallback(this.HandleProcessContent));
            this._attributeHandler.Add(this.PreserveElements, new HandleAttributeCallback(this.HandlePreserveElements));
            this._attributeHandler.Add(this.PreserveAttributes, new HandleAttributeCallback(this.HandlePreserveAttributes));
        }

        public XmlCompatibilityReader(XmlReader baseReader, IEnumerable<string> supportedNamespaces) : this(baseReader, null, supportedNamespaces)
        {
        }

        public XmlCompatibilityReader(XmlReader baseReader, IsXmlNamespaceSupportedCallback isXmlNamespaceSupported) : this(baseReader)
        {
            this._namespaceCallback = isXmlNamespaceSupported;
        }

        public XmlCompatibilityReader(XmlReader baseReader, IsXmlNamespaceSupportedCallback isXmlNamespaceSupported, IEnumerable<string> supportedNamespaces) : this(baseReader, isXmlNamespaceSupported)
        {
            foreach (string str in supportedNamespaces)
            {
                this.AddKnownNamespace(str);
                this._namespaceMap[str] = str;
            }
        }

        private void AddKnownNamespace(string namespaceName)
        {
            if (this._knownNamespaces == null)
            {
                this._knownNamespaces = new Dictionary<string, object>();
            }
            this._knownNamespaces[namespaceName] = null;
        }

        private void AddSubsumingNamespace(string namespaceName)
        {
            if (this._subsumingNamespaces == null)
            {
                this._subsumingNamespaces = new Dictionary<string, object>();
            }
            this._subsumingNamespaces[namespaceName] = null;
        }

        public void DeclareNamespaceCompatibility(string newNamespace, string oldNamespace)
        {
            if (newNamespace != oldNamespace)
            {
                string str;
                this.AddSubsumingNamespace(newNamespace);
                if (this._namespaceMap.TryGetValue(newNamespace, out str))
                {
                    newNamespace = str;
                }
                if (this.IsSubsumingNamespace(oldNamespace))
                {
                    List<string> list = new List<string>();
                    foreach (KeyValuePair<string, string> pair in this._namespaceMap)
                    {
                        if (pair.Value == oldNamespace)
                        {
                            list.Add(pair.Key);
                        }
                    }
                    foreach (string str2 in list)
                    {
                        this._namespaceMap[str2] = newNamespace;
                    }
                }
            }
            this._namespaceMap[oldNamespace] = newNamespace;
        }

        private void Error(string message, params object[] args)
        {
            IXmlLineInfo reader = base.Reader as IXmlLineInfo;
            throw new XmlException(string.Format(CultureInfo.InvariantCulture, message, args), null, (reader == null) ? 1 : reader.LineNumber, (reader == null) ? 1 : reader.LinePosition);
        }

        public override string GetAttribute(int i)
        {
            string str = null;
            if (this._ignoredAttributeCount == 0)
            {
                return base.Reader.GetAttribute(i);
            }
            this.SaveReaderPosition();
            this.MoveToAttribute(i);
            str = base.Reader.Value;
            this.RestoreReaderPosition();
            return str;
        }

        public override string GetAttribute(string name)
        {
            string str = null;
            if (this._ignoredAttributeCount == 0)
            {
                return base.Reader.GetAttribute(name);
            }
            this.SaveReaderPosition();
            if (this.MoveToAttribute(name))
            {
                str = base.Reader.Value;
                this.RestoreReaderPosition();
            }
            return str;
        }

        public override string GetAttribute(string localName, string namespaceURI)
        {
            string str = null;
            if ((this._ignoredAttributeCount != 0) && this.ShouldIgnoreNamespace(namespaceURI))
            {
                return str;
            }
            return base.Reader.GetAttribute(localName, namespaceURI);
        }

        private string GetMappedNamespace(string namespaceName)
        {
            string str;
            if (!this._namespaceMap.TryGetValue(namespaceName, out str))
            {
                return this.MapNewNamespace(namespaceName);
            }
            if (str == null)
            {
                str = namespaceName;
            }
            return str;
        }

        private void HandleAlternateContent(int elementDepth, ref bool more)
        {
            if (this.Scope.InAlternateContent)
            {
                this.Error(System.Xaml.SR.Get("XCRInvalidACChild", new object[] { base.Reader.Name }), new object[0]);
            }
            if (base.Reader.IsEmptyElement)
            {
                this.Error(System.Xaml.SR.Get("XCRChoiceNotFound"), new object[0]);
            }
            this.ScanForCompatibility(elementDepth);
            this.PushScope(elementDepth);
            this.Scope.InAlternateContent = true;
            this._depthOffset++;
            more = base.Reader.Read();
        }

        private void HandleChoice(int elementDepth, ref bool more)
        {
            if (!this.Scope.InAlternateContent)
            {
                this.Error(System.Xaml.SR.Get("XCRChoiceOnlyInAC"), new object[0]);
            }
            if (this.Scope.FallbackSeen)
            {
                this.Error(System.Xaml.SR.Get("XCRChoiceAfterFallback"), new object[0]);
            }
            string attribute = base.Reader.GetAttribute(this.Requires);
            if (attribute == null)
            {
                this.Error(System.Xaml.SR.Get("XCRRequiresAttribNotFound"), new object[0]);
            }
            if (string.IsNullOrEmpty(attribute))
            {
                this.Error(System.Xaml.SR.Get("XCRInvalidRequiresAttribute"), new object[0]);
            }
            CompatibilityScope scope = this.Scope;
            this.ScanForCompatibility(elementDepth);
            if (this.AttributeCount != 1)
            {
                this.MoveToFirstAttribute();
                if (base.Reader.LocalName == this.Requires)
                {
                    this.MoveToNextAttribute();
                }
                string localName = base.Reader.LocalName;
                this.MoveToElement();
                this.Error(System.Xaml.SR.Get("XCRInvalidAttribInElement"), new object[] { localName, this.Choice });
            }
            if (scope.ChoiceTaken)
            {
                this.ScanForEndCompatibility(elementDepth);
                base.Reader.Skip();
            }
            else
            {
                scope.ChoiceSeen = true;
                bool flag = true;
                bool flag2 = false;
                foreach (string str3 in this.PrefixesToNamespaces(attribute))
                {
                    flag2 = true;
                    if (!this.IsNamespaceKnown(str3))
                    {
                        flag = false;
                        break;
                    }
                }
                if (!flag2)
                {
                    this.Error(System.Xaml.SR.Get("XCRInvalidRequiresAttribute"), new object[0]);
                }
                if (flag)
                {
                    scope.ChoiceTaken = true;
                    this.PushScope(elementDepth);
                    this._depthOffset++;
                    more = base.Reader.Read();
                }
                else
                {
                    this.ScanForEndCompatibility(elementDepth);
                    base.Reader.Skip();
                }
            }
        }

        private void HandleFallback(int elementDepth, ref bool more)
        {
            if (!this.Scope.InAlternateContent)
            {
                this.Error(System.Xaml.SR.Get("XCRFallbackOnlyInAC"), new object[0]);
            }
            if (!this.Scope.ChoiceSeen)
            {
                this.Error(System.Xaml.SR.Get("XCRChoiceNotFound"), new object[0]);
            }
            if (this.Scope.FallbackSeen)
            {
                this.Error(System.Xaml.SR.Get("XCRMultipleFallbackFound"), new object[0]);
            }
            this.Scope.FallbackSeen = true;
            bool choiceTaken = this.Scope.ChoiceTaken;
            this.ScanForCompatibility(elementDepth);
            if (this.AttributeCount != 0)
            {
                this.MoveToFirstAttribute();
                string localName = base.Reader.LocalName;
                this.MoveToElement();
                this.Error(System.Xaml.SR.Get("XCRInvalidAttribInElement"), new object[] { localName, this.Fallback });
            }
            if (choiceTaken)
            {
                this.ScanForEndCompatibility(elementDepth);
                base.Reader.Skip();
            }
            else
            {
                if (!base.Reader.IsEmptyElement)
                {
                    this.PushScope(elementDepth);
                    this._depthOffset++;
                }
                more = base.Reader.Read();
            }
        }

        private void HandleIgnorable(int elementDepth)
        {
            this.PushScope(elementDepth);
            foreach (string str in this.PrefixesToNamespaces(base.Reader.Value))
            {
                this.Scope.Ignorable(str);
            }
            if (this._ignoredAttributeCount < this._attributePosition)
            {
                this._ignoredAttributeCount = 0;
                base.Reader.MoveToFirstAttribute();
                for (int i = 0; i < this._attributePosition; i++)
                {
                    if (this.ShouldIgnoreNamespace(base.Reader.NamespaceURI))
                    {
                        this._ignoredAttributeCount++;
                    }
                    base.Reader.MoveToNextAttribute();
                }
            }
        }

        private void HandleMustUnderstand(int elementDepth)
        {
            foreach (string str in this.PrefixesToNamespaces(base.Reader.Value))
            {
                if (!this.IsNamespaceKnown(str))
                {
                    this.Error(System.Xaml.SR.Get("XCRMustUnderstandFailed"), new object[] { str });
                }
            }
        }

        private void HandlePreserveAttributes(int elementDepth)
        {
            this.PushScope(elementDepth);
            foreach (NamespaceElementPair pair in this.ParseContentToNamespaceElementPair(base.Reader.Value, this._preserveAttributes))
            {
                this.Scope.PreserveAttribute(pair.namespaceName, pair.itemName);
            }
        }

        private void HandlePreserveElements(int elementDepth)
        {
            this.PushScope(elementDepth);
            foreach (NamespaceElementPair pair in this.ParseContentToNamespaceElementPair(base.Reader.Value, this._preserveElements))
            {
                this.Scope.PreserveElement(pair.namespaceName, pair.itemName);
            }
        }

        private void HandleProcessContent(int elementDepth)
        {
            this.PushScope(elementDepth);
            foreach (NamespaceElementPair pair in this.ParseContentToNamespaceElementPair(base.Reader.Value, this._processContent))
            {
                this.Scope.ProcessContent(pair.namespaceName, pair.itemName);
            }
        }

        private bool IsNamespaceKnown(string namespaceName)
        {
            return ((this._knownNamespaces != null) && this._knownNamespaces.ContainsKey(namespaceName));
        }

        private bool IsSubsumingNamespace(string namespaceName)
        {
            return ((this._subsumingNamespaces != null) && this._subsumingNamespaces.ContainsKey(namespaceName));
        }

        public override string LookupNamespace(string prefix)
        {
            string namespaceName = base.Reader.LookupNamespace(prefix);
            if (namespaceName != null)
            {
                namespaceName = this.GetMappedNamespace(namespaceName);
            }
            return namespaceName;
        }

        private string MapNewNamespace(string namespaceName)
        {
            if (this._namespaceCallback != null)
            {
                string str;
                if (this._namespaceCallback(namespaceName, out str))
                {
                    string str2;
                    this.AddKnownNamespace(namespaceName);
                    if (string.IsNullOrEmpty(str) || (namespaceName == str))
                    {
                        this._namespaceMap[namespaceName] = namespaceName;
                        return namespaceName;
                    }
                    if (!this._namespaceMap.TryGetValue(str, out str2))
                    {
                        if (this.IsNamespaceKnown(str))
                        {
                            this.Error(System.Xaml.SR.Get("XCRCompatCycle"), new object[] { str });
                        }
                        str2 = this.MapNewNamespace(str);
                    }
                    this.DeclareNamespaceCompatibility(str2, namespaceName);
                    namespaceName = str2;
                    return namespaceName;
                }
                this._namespaceMap[namespaceName] = null;
            }
            return namespaceName;
        }

        public override void MoveToAttribute(int i)
        {
            if (this._ignoredAttributeCount == 0)
            {
                base.Reader.MoveToAttribute(i);
            }
            else
            {
                if ((i < 0) || (i >= this.AttributeCount))
                {
                    throw new ArgumentOutOfRangeException("i");
                }
                base.Reader.MoveToFirstAttribute();
                while (this.ShouldIgnoreNamespace(this.NamespaceURI) || (i-- != 0))
                {
                    base.Reader.MoveToNextAttribute();
                }
            }
        }

        public override bool MoveToAttribute(string name)
        {
            if (this._ignoredAttributeCount == 0)
            {
                return base.Reader.MoveToAttribute(name);
            }
            this.SaveReaderPosition();
            bool flag = base.Reader.MoveToAttribute(name);
            if (flag && this.ShouldIgnoreNamespace(this.NamespaceURI))
            {
                flag = false;
                this.RestoreReaderPosition();
            }
            return flag;
        }

        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            if (this._ignoredAttributeCount == 0)
            {
                return base.Reader.MoveToAttribute(localName, namespaceURI);
            }
            this.SaveReaderPosition();
            bool flag = base.Reader.MoveToAttribute(localName, namespaceURI);
            if (flag && this.ShouldIgnoreNamespace(namespaceURI))
            {
                flag = false;
                this.RestoreReaderPosition();
            }
            return flag;
        }

        public override bool MoveToFirstAttribute()
        {
            bool hasAttributes = this.HasAttributes;
            if (hasAttributes)
            {
                this.MoveToAttribute(0);
            }
            return hasAttributes;
        }

        public override bool MoveToNextAttribute()
        {
            if (this._ignoredAttributeCount == 0)
            {
                return base.Reader.MoveToNextAttribute();
            }
            this.SaveReaderPosition();
            bool flag = base.Reader.MoveToNextAttribute();
            if (flag)
            {
                flag = this.SkipToKnownAttribute();
                if (!flag)
                {
                    this.RestoreReaderPosition();
                }
            }
            return flag;
        }

        private IEnumerable<NamespaceElementPair> ParseContentToNamespaceElementPair(string content, string callerContext)
        {
            foreach (string iteratorVariable0 in content.Trim().Split(new char[] { ' ' }))
            {
                if (!string.IsNullOrEmpty(iteratorVariable0))
                {
                    int index = iteratorVariable0.IndexOf(':');
                    int length = iteratorVariable0.Length;
                    if (((index <= 0) || (index >= (length - 1))) || (index != iteratorVariable0.LastIndexOf(':')))
                    {
                        this.Error(System.Xaml.SR.Get("XCRInvalidFormat"), new object[] { callerContext });
                    }
                    string prefix = iteratorVariable0.Substring(0, index);
                    string str = iteratorVariable0.Substring(index + 1, (length - 1) - index);
                    string namespaceName = this.LookupNamespace(prefix);
                    if (namespaceName == null)
                    {
                        this.Error(System.Xaml.SR.Get("XCRUndefinedPrefix"), new object[] { prefix });
                    }
                    else if ((str != "*") && !XmlReader.IsName(str))
                    {
                        this.Error(System.Xaml.SR.Get("XCRInvalidXMLName"), new object[] { iteratorVariable0 });
                    }
                    else
                    {
                        yield return new NamespaceElementPair(namespaceName, str);
                    }
                }
            }
        }

        private void PopScope()
        {
            this._compatibilityScope = this._compatibilityScope.Previous;
        }

        private IEnumerable<string> PrefixesToNamespaces(string prefixes)
        {
            foreach (string iteratorVariable0 in prefixes.Trim().Split(new char[] { ' ' }))
            {
                if (!string.IsNullOrEmpty(iteratorVariable0))
                {
                    string iteratorVariable1 = this.LookupNamespace(iteratorVariable0);
                    if (iteratorVariable1 == null)
                    {
                        this.Error(System.Xaml.SR.Get("XCRUndefinedPrefix"), new object[] { iteratorVariable0 });
                    }
                    else
                    {
                        yield return iteratorVariable1;
                    }
                }
            }
        }

        private void PushScope(int elementDepth)
        {
            if (this._compatibilityScope.Depth < elementDepth)
            {
                this._compatibilityScope = new CompatibilityScope(this._compatibilityScope, elementDepth, this);
            }
        }

        public override bool Read()
        {
            if (this.isPreviousElementEmpty)
            {
                this.isPreviousElementEmpty = false;
                this.ScanForEndCompatibility(this.previousElementDepth);
            }
            bool more = base.Reader.Read();
            while (more)
            {
                XmlNodeType nodeType = base.Reader.NodeType;
                if (nodeType != XmlNodeType.Element)
                {
                    if (nodeType == XmlNodeType.EndElement)
                    {
                        goto Label_004E;
                    }
                    goto Label_0058;
                }
                if (this.ReadStartElement(ref more))
                {
                    goto Label_0058;
                }
                continue;
            Label_004E:
                if (!this.ReadEndElement(ref more))
                {
                    continue;
                }
            Label_0058:
                return true;
            }
            return false;
        }

        private bool ReadEndElement(ref bool more)
        {
            int depth = base.Reader.Depth;
            string namespaceURI = this.NamespaceURI;
            bool flag = false;
            if (object.ReferenceEquals(namespaceURI, this.CompatibilityUri))
            {
                if (object.ReferenceEquals(base.Reader.LocalName, this.AlternateContent) && !this.Scope.ChoiceSeen)
                {
                    this.Error(System.Xaml.SR.Get("XCRChoiceNotFound"), new object[0]);
                }
                this._depthOffset--;
                this.PopScope();
                more = base.Reader.Read();
                return flag;
            }
            if (this.ShouldIgnoreNamespace(namespaceURI))
            {
                this.ScanForEndCompatibility(depth);
                this._depthOffset--;
                more = base.Reader.Read();
                return flag;
            }
            this.ScanForEndCompatibility(depth);
            return true;
        }

        private bool ReadStartElement(ref bool more)
        {
            int depth = base.Reader.Depth;
            int num2 = this._depthOffset;
            bool isEmptyElement = base.Reader.IsEmptyElement;
            string namespaceURI = this.NamespaceURI;
            bool flag2 = false;
            if (object.ReferenceEquals(namespaceURI, this.CompatibilityUri))
            {
                HandleElementCallback callback;
                string localName = base.Reader.LocalName;
                if (!this._elementHandler.TryGetValue(localName, out callback))
                {
                    this.Error(System.Xaml.SR.Get("XCRUnknownCompatElement"), new object[] { localName });
                }
                callback(depth, ref more);
            }
            else
            {
                this.ScanForCompatibility(depth);
                if (this.ShouldIgnoreNamespace(namespaceURI))
                {
                    if (this.Scope.ShouldProcessContent(namespaceURI, base.Reader.LocalName))
                    {
                        if (this.Scope.Depth == depth)
                        {
                            this.Scope.InProcessContent = true;
                        }
                        this._depthOffset++;
                        more = base.Reader.Read();
                    }
                    else
                    {
                        this.ScanForEndCompatibility(depth);
                        base.Reader.Skip();
                    }
                }
                else
                {
                    if (this.Scope.InAlternateContent)
                    {
                        this.Error(System.Xaml.SR.Get("XCRInvalidACChild"), new object[] { base.Reader.Name });
                    }
                    flag2 = true;
                }
            }
            if (isEmptyElement)
            {
                this.isPreviousElementEmpty = true;
                this.previousElementDepth = depth;
                this._depthOffset = num2;
            }
            return flag2;
        }

        private void RestoreReaderPosition()
        {
            if (this._inAttribute)
            {
                base.Reader.MoveToAttribute(this._currentName);
            }
            else
            {
                base.Reader.MoveToElement();
            }
        }

        private void SaveReaderPosition()
        {
            this._inAttribute = base.Reader.NodeType == XmlNodeType.Attribute;
            this._currentName = base.Reader.Name;
        }

        private void ScanForCompatibility(int elementDepth)
        {
            bool flag = base.Reader.MoveToFirstAttribute();
            this._ignoredAttributeCount = 0;
            if (flag)
            {
                this._attributePosition = 0;
                do
                {
                    string namespaceURI = this.NamespaceURI;
                    if (this.ShouldIgnoreNamespace(namespaceURI))
                    {
                        if (object.ReferenceEquals(namespaceURI, this.CompatibilityUri))
                        {
                            HandleAttributeCallback callback;
                            string localName = base.Reader.LocalName;
                            if (!this._attributeHandler.TryGetValue(localName, out callback))
                            {
                                this.Error(System.Xaml.SR.Get("XCRUnknownCompatAttrib"), new object[] { localName });
                            }
                            callback(elementDepth);
                        }
                        this._ignoredAttributeCount++;
                    }
                    flag = base.Reader.MoveToNextAttribute();
                    this._attributePosition++;
                }
                while (flag);
                if (this.Scope.Depth == elementDepth)
                {
                    this.Scope.Verify();
                }
                base.Reader.MoveToElement();
            }
        }

        private void ScanForEndCompatibility(int elementDepth)
        {
            if (elementDepth == this.Scope.Depth)
            {
                this.PopScope();
            }
        }

        private bool ShouldIgnoreNamespace(string namespaceName)
        {
            if (this.IsNamespaceKnown(namespaceName))
            {
                return object.ReferenceEquals(namespaceName, this.CompatibilityUri);
            }
            return this.Scope.CanIgnore(namespaceName);
        }

        private bool SkipToKnownAttribute()
        {
            bool flag = true;
            while (flag && this.ShouldIgnoreNamespace(this.NamespaceURI))
            {
                flag = base.Reader.MoveToNextAttribute();
            }
            return flag;
        }

        private string AlternateContent
        {
            get
            {
                if (this._alternateContent == null)
                {
                    this._alternateContent = base.Reader.NameTable.Add("AlternateContent");
                }
                return this._alternateContent;
            }
        }

        public override int AttributeCount
        {
            get
            {
                return (base.Reader.AttributeCount - this._ignoredAttributeCount);
            }
        }

        private string Choice
        {
            get
            {
                if (this._choice == null)
                {
                    this._choice = base.Reader.NameTable.Add("Choice");
                }
                return this._choice;
            }
        }

        private string CompatibilityUri
        {
            get
            {
                if (this._compatibilityUri == null)
                {
                    this._compatibilityUri = base.Reader.NameTable.Add("http://schemas.openxmlformats.org/markup-compatibility/2006");
                }
                return this._compatibilityUri;
            }
        }

        public override int Depth
        {
            get
            {
                return (base.Reader.Depth - this._depthOffset);
            }
        }

        internal System.Text.Encoding Encoding
        {
            get
            {
                XmlTextReader reader = base.Reader as XmlTextReader;
                if (reader == null)
                {
                    return new UTF8Encoding(true, true);
                }
                return reader.Encoding;
            }
        }

        private string Fallback
        {
            get
            {
                if (this._fallback == null)
                {
                    this._fallback = base.Reader.NameTable.Add("Fallback");
                }
                return this._fallback;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                return (this.AttributeCount != 0);
            }
        }

        private string Ignorable
        {
            get
            {
                if (this._ignorable == null)
                {
                    this._ignorable = base.Reader.NameTable.Add("Ignorable");
                }
                return this._ignorable;
            }
        }

        private string MustUnderstand
        {
            get
            {
                if (this._mustUnderstand == null)
                {
                    this._mustUnderstand = base.Reader.NameTable.Add("MustUnderstand");
                }
                return this._mustUnderstand;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.GetMappedNamespace(base.Reader.NamespaceURI);
            }
        }

        public bool Normalization
        {
            set
            {
                XmlTextReader reader = base.Reader as XmlTextReader;
                if (reader != null)
                {
                    reader.Normalization = value;
                }
            }
        }

        private string PreserveAttributes
        {
            get
            {
                if (this._preserveAttributes == null)
                {
                    this._preserveAttributes = base.Reader.NameTable.Add("PreserveAttributes");
                }
                return this._preserveAttributes;
            }
        }

        private string PreserveElements
        {
            get
            {
                if (this._preserveElements == null)
                {
                    this._preserveElements = base.Reader.NameTable.Add("PreserveElements");
                }
                return this._preserveElements;
            }
        }

        private string ProcessContent
        {
            get
            {
                if (this._processContent == null)
                {
                    this._processContent = base.Reader.NameTable.Add("ProcessContent");
                }
                return this._processContent;
            }
        }

        private string Requires
        {
            get
            {
                if (this._requires == null)
                {
                    this._requires = base.Reader.NameTable.Add("Requires");
                }
                return this._requires;
            }
        }

        private CompatibilityScope Scope
        {
            get
            {
                return this._compatibilityScope;
            }
        }

        public override string Value
        {
            get
            {
                if (string.Equals("xmlns", base.Reader.LocalName, StringComparison.Ordinal))
                {
                    return this.LookupNamespace(string.Empty);
                }
                if (string.Equals("xmlns", base.Reader.Prefix, StringComparison.Ordinal))
                {
                    return this.LookupNamespace(base.Reader.LocalName);
                }
                return base.Reader.Value;
            }
        }



        private class CompatibilityScope
        {
            private bool _choiceSeen;
            private bool _choiceTaken;
            private int _depth;
            private bool _fallbackSeen;
            private Dictionary<string, object> _ignorables;
            private bool _inAlternateContent;
            private bool _inProcessContent;
            private Dictionary<string, XmlCompatibilityReader.PreserveItemSet> _preserveAttributes;
            private Dictionary<string, XmlCompatibilityReader.PreserveItemSet> _preserveElements;
            private XmlCompatibilityReader.CompatibilityScope _previous;
            private Dictionary<string, XmlCompatibilityReader.ProcessContentSet> _processContents;
            private XmlCompatibilityReader _reader;

            public CompatibilityScope(XmlCompatibilityReader.CompatibilityScope previous, int depth, XmlCompatibilityReader reader)
            {
                this._previous = previous;
                this._depth = depth;
                this._reader = reader;
            }

            public bool CanIgnore(string namespaceName)
            {
                bool flag = this.IsIgnorableAtCurrentScope(namespaceName);
                if (!flag && (this._previous != null))
                {
                    flag = this._previous.CanIgnore(namespaceName);
                }
                return flag;
            }

            public void Ignorable(string namespaceName)
            {
                if (this._ignorables == null)
                {
                    this._ignorables = new Dictionary<string, object>();
                }
                this._ignorables[namespaceName] = null;
            }

            public bool IsIgnorableAtCurrentScope(string namespaceName)
            {
                return ((this._ignorables != null) && this._ignorables.ContainsKey(namespaceName));
            }

            public void PreserveAttribute(string namespaceName, string attributeName)
            {
                XmlCompatibilityReader.PreserveItemSet set;
                if (this._preserveAttributes == null)
                {
                    this._preserveAttributes = new Dictionary<string, XmlCompatibilityReader.PreserveItemSet>();
                }
                if (!this._preserveAttributes.TryGetValue(namespaceName, out set))
                {
                    set = new XmlCompatibilityReader.PreserveItemSet(namespaceName, this._reader);
                    this._preserveAttributes.Add(namespaceName, set);
                }
                set.Add(attributeName);
            }

            public void PreserveElement(string namespaceName, string elementName)
            {
                XmlCompatibilityReader.PreserveItemSet set;
                if (this._preserveElements == null)
                {
                    this._preserveElements = new Dictionary<string, XmlCompatibilityReader.PreserveItemSet>();
                }
                if (!this._preserveElements.TryGetValue(namespaceName, out set))
                {
                    set = new XmlCompatibilityReader.PreserveItemSet(namespaceName, this._reader);
                    this._preserveElements.Add(namespaceName, set);
                }
                set.Add(elementName);
            }

            public void ProcessContent(string namespaceName, string elementName)
            {
                XmlCompatibilityReader.ProcessContentSet set;
                if (this._processContents == null)
                {
                    this._processContents = new Dictionary<string, XmlCompatibilityReader.ProcessContentSet>();
                }
                if (!this._processContents.TryGetValue(namespaceName, out set))
                {
                    set = new XmlCompatibilityReader.ProcessContentSet(namespaceName, this._reader);
                    this._processContents.Add(namespaceName, set);
                }
                set.Add(elementName);
            }

            public bool ShouldProcessContent(string namespaceName, string elementName)
            {
                XmlCompatibilityReader.ProcessContentSet set;
                bool flag = false;
                if ((this._processContents != null) && this._processContents.TryGetValue(namespaceName, out set))
                {
                    return set.ShouldProcessContent(elementName);
                }
                if (this._previous != null)
                {
                    flag = this._previous.ShouldProcessContent(namespaceName, elementName);
                }
                return flag;
            }

            public void Verify()
            {
                if (this._processContents != null)
                {
                    foreach (string str in this._processContents.Keys)
                    {
                        if (!this.IsIgnorableAtCurrentScope(str))
                        {
                            this._reader.Error(System.Xaml.SR.Get("XCRNSProcessContentNotIgnorable"), new object[] { str });
                        }
                    }
                }
                if (this._preserveElements != null)
                {
                    foreach (string str2 in this._preserveElements.Keys)
                    {
                        if (!this.IsIgnorableAtCurrentScope(str2))
                        {
                            this._reader.Error(System.Xaml.SR.Get("XCRNSPreserveNotIgnorable"), new object[] { str2 });
                        }
                    }
                }
                if (this._preserveAttributes != null)
                {
                    foreach (string str3 in this._preserveAttributes.Keys)
                    {
                        if (!this.IsIgnorableAtCurrentScope(str3))
                        {
                            this._reader.Error(System.Xaml.SR.Get("XCRNSPreserveNotIgnorable"), new object[] { str3 });
                        }
                    }
                }
            }

            public bool ChoiceSeen
            {
                get
                {
                    if (this._inProcessContent && (this._previous != null))
                    {
                        return this._previous.ChoiceSeen;
                    }
                    return this._choiceSeen;
                }
                set
                {
                    if (this._inProcessContent && (this._previous != null))
                    {
                        this._previous.ChoiceSeen = value;
                    }
                    else
                    {
                        this._choiceSeen = value;
                    }
                }
            }

            public bool ChoiceTaken
            {
                get
                {
                    if (this._inProcessContent && (this._previous != null))
                    {
                        return this._previous.ChoiceTaken;
                    }
                    return this._choiceTaken;
                }
                set
                {
                    if (this._inProcessContent && (this._previous != null))
                    {
                        this._previous.ChoiceTaken = value;
                    }
                    else
                    {
                        this._choiceTaken = value;
                    }
                }
            }

            public int Depth
            {
                get
                {
                    return this._depth;
                }
            }

            public bool FallbackSeen
            {
                get
                {
                    if (this._inProcessContent && (this._previous != null))
                    {
                        return this._previous.FallbackSeen;
                    }
                    return this._fallbackSeen;
                }
                set
                {
                    if (this._inProcessContent && (this._previous != null))
                    {
                        this._previous.FallbackSeen = value;
                    }
                    else
                    {
                        this._fallbackSeen = value;
                    }
                }
            }

            public bool InAlternateContent
            {
                get
                {
                    if (this._inProcessContent && (this._previous != null))
                    {
                        return this._previous.InAlternateContent;
                    }
                    return this._inAlternateContent;
                }
                set
                {
                    this._inAlternateContent = value;
                }
            }

            public bool InProcessContent
            {
                set
                {
                    this._inProcessContent = value;
                }
            }

            public XmlCompatibilityReader.CompatibilityScope Previous
            {
                get
                {
                    return this._previous;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NamespaceElementPair
        {
            public string namespaceName;
            public string itemName;
            public NamespaceElementPair(string namespaceName, string itemName)
            {
                this.namespaceName = namespaceName;
                this.itemName = itemName;
            }
        }

        private class PreserveItemSet
        {
            private bool _all;
            private Dictionary<string, string> _names;
            private string _namespaceName;
            private XmlCompatibilityReader _reader;

            public PreserveItemSet(string namespaceName, XmlCompatibilityReader reader)
            {
                this._namespaceName = namespaceName;
                this._reader = reader;
            }

            public void Add(string itemName)
            {
                if (this.ShouldPreserveItem(itemName))
                {
                    if (itemName == "*")
                    {
                        this._reader.Error(System.Xaml.SR.Get("XCRDuplicateWildcardPreserve"), new object[] { this._namespaceName });
                    }
                    else
                    {
                        this._reader.Error(System.Xaml.SR.Get("XCRDuplicatePreserve"), new object[] { itemName, this._namespaceName });
                    }
                }
                if (itemName == "*")
                {
                    if (this._names != null)
                    {
                        this._reader.Error(System.Xaml.SR.Get("XCRInvalidPreserve"), new object[] { this._namespaceName });
                    }
                    else
                    {
                        this._all = true;
                    }
                }
                else
                {
                    if (this._names == null)
                    {
                        this._names = new Dictionary<string, string>();
                    }
                    this._names.Add(itemName, itemName);
                }
            }

            public bool ShouldPreserveItem(string itemName)
            {
                return (this._all || ((this._names != null) && this._names.ContainsKey(itemName)));
            }
        }

        private class ProcessContentSet
        {
            private bool _all;
            private Dictionary<string, object> _names;
            private string _namespaceName;
            private XmlCompatibilityReader _reader;

            public ProcessContentSet(string namespaceName, XmlCompatibilityReader reader)
            {
                this._namespaceName = namespaceName;
                this._reader = reader;
            }

            public void Add(string elementName)
            {
                if (this.ShouldProcessContent(elementName))
                {
                    if (elementName == "*")
                    {
                        this._reader.Error(System.Xaml.SR.Get("XCRDuplicateWildcardProcessContent"), new object[] { this._namespaceName });
                    }
                    else
                    {
                        this._reader.Error(System.Xaml.SR.Get("XCRDuplicateProcessContent"), new object[] { this._namespaceName, elementName });
                    }
                }
                if (elementName == "*")
                {
                    if (this._names != null)
                    {
                        this._reader.Error(System.Xaml.SR.Get("XCRInvalidProcessContent"), new object[] { this._namespaceName });
                    }
                    else
                    {
                        this._all = true;
                    }
                }
                else
                {
                    if (this._names == null)
                    {
                        this._names = new Dictionary<string, object>();
                    }
                    this._names[elementName] = null;
                }
            }

            public bool ShouldProcessContent(string elementName)
            {
                return (this._all || ((this._names != null) && this._names.ContainsKey(elementName)));
            }
        }
    }
}

