namespace System.Xml.Linq
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class XNodeReader : XmlReader, IXmlLineInfo
    {
        private IDtdInfo dtdInfo;
        private bool dtdInfoInitialized;
        private XmlNameTable nameTable;
        private bool omitDuplicateNamespaces;
        private object parent;
        private XNode root;
        private object source;
        private System.Xml.ReadState state;

        internal XNodeReader(XNode node, XmlNameTable nameTable) : this(node, nameTable, ((node.GetSaveOptionsFromAnnotations() & SaveOptions.OmitDuplicateNamespaces) != SaveOptions.None) ? ReaderOptions.OmitDuplicateNamespaces : ReaderOptions.None)
        {
        }

        internal XNodeReader(XNode node, XmlNameTable nameTable, ReaderOptions options)
        {
            this.source = node;
            this.root = node;
            this.nameTable = (nameTable != null) ? nameTable : CreateNameTable();
            this.omitDuplicateNamespaces = (options & ReaderOptions.OmitDuplicateNamespaces) != ReaderOptions.None;
        }

        public override void Close()
        {
            this.source = null;
            this.parent = null;
            this.root = null;
            this.state = System.Xml.ReadState.Closed;
        }

        private static XmlNameTable CreateNameTable()
        {
            XmlNameTable table = new System.Xml.NameTable();
            table.Add(string.Empty);
            table.Add("http://www.w3.org/2000/xmlns/");
            table.Add("http://www.w3.org/XML/1998/namespace");
            return table;
        }

        public override string GetAttribute(int index)
        {
            if (this.IsInteractive)
            {
                if (index < 0)
                {
                    return null;
                }
                XElement elementInAttributeScope = this.GetElementInAttributeScope();
                if (elementInAttributeScope != null)
                {
                    XAttribute lastAttr = elementInAttributeScope.lastAttr;
                    if (lastAttr != null)
                    {
                        do
                        {
                            lastAttr = lastAttr.next;
                            if ((!this.omitDuplicateNamespaces || !this.IsDuplicateNamespaceAttribute(lastAttr)) && (index-- == 0))
                            {
                                return lastAttr.Value;
                            }
                        }
                        while (lastAttr != elementInAttributeScope.lastAttr);
                    }
                }
            }
            return null;
        }

        public override string GetAttribute(string name)
        {
            if (this.IsInteractive)
            {
                string str3;
                XElement elementInAttributeScope = this.GetElementInAttributeScope();
                if (elementInAttributeScope != null)
                {
                    string str;
                    string str2;
                    GetNameInAttributeScope(name, elementInAttributeScope, out str, out str2);
                    XAttribute lastAttr = elementInAttributeScope.lastAttr;
                    if (lastAttr != null)
                    {
                        do
                        {
                            lastAttr = lastAttr.next;
                            if ((lastAttr.Name.LocalName == str) && (lastAttr.Name.NamespaceName == str2))
                            {
                                if (this.omitDuplicateNamespaces && this.IsDuplicateNamespaceAttribute(lastAttr))
                                {
                                    return null;
                                }
                                return lastAttr.Value;
                            }
                        }
                        while (lastAttr != elementInAttributeScope.lastAttr);
                    }
                    return null;
                }
                XDocumentType source = this.source as XDocumentType;
                if ((source != null) && ((str3 = name) != null))
                {
                    if (str3 == "PUBLIC")
                    {
                        return source.PublicId;
                    }
                    if (str3 == "SYSTEM")
                    {
                        return source.SystemId;
                    }
                }
            }
            return null;
        }

        public override string GetAttribute(string localName, string namespaceName)
        {
            if (this.IsInteractive)
            {
                XElement elementInAttributeScope = this.GetElementInAttributeScope();
                if (elementInAttributeScope != null)
                {
                    if (localName == "xmlns")
                    {
                        if ((namespaceName != null) && (namespaceName.Length == 0))
                        {
                            return null;
                        }
                        if (namespaceName == "http://www.w3.org/2000/xmlns/")
                        {
                            namespaceName = string.Empty;
                        }
                    }
                    XAttribute lastAttr = elementInAttributeScope.lastAttr;
                    if (lastAttr != null)
                    {
                        do
                        {
                            lastAttr = lastAttr.next;
                            if ((lastAttr.Name.LocalName == localName) && (lastAttr.Name.NamespaceName == namespaceName))
                            {
                                if (this.omitDuplicateNamespaces && this.IsDuplicateNamespaceAttribute(lastAttr))
                                {
                                    return null;
                                }
                                return lastAttr.Value;
                            }
                        }
                        while (lastAttr != elementInAttributeScope.lastAttr);
                    }
                }
            }
            return null;
        }

        private static int GetDepth(XObject o)
        {
            int num = 0;
            while (o.parent != null)
            {
                num++;
                o = o.parent;
            }
            if (o is XDocument)
            {
                num--;
            }
            return num;
        }

        private XElement GetElementInAttributeScope()
        {
            XElement source = this.source as XElement;
            if (source != null)
            {
                if (this.IsEndElement)
                {
                    return null;
                }
                return source;
            }
            XAttribute parent = this.source as XAttribute;
            if (parent != null)
            {
                return (XElement) parent.parent;
            }
            parent = this.parent as XAttribute;
            if (parent != null)
            {
                return (XElement) parent.parent;
            }
            return null;
        }

        private XElement GetElementInScope()
        {
            XElement source = this.source as XElement;
            if (source != null)
            {
                return source;
            }
            XNode node = this.source as XNode;
            if (node != null)
            {
                return (node.parent as XElement);
            }
            XAttribute parent = this.source as XAttribute;
            if (parent != null)
            {
                return (XElement) parent.parent;
            }
            source = this.parent as XElement;
            if (source != null)
            {
                return source;
            }
            parent = this.parent as XAttribute;
            if (parent != null)
            {
                return (XElement) parent.parent;
            }
            return null;
        }

        private XAttribute GetFirstNonDuplicateNamespaceAttribute(XAttribute candidate)
        {
            if (!this.IsDuplicateNamespaceAttribute(candidate))
            {
                return candidate;
            }
            XElement parent = candidate.parent as XElement;
            if ((parent != null) && (candidate != parent.lastAttr))
            {
                do
                {
                    candidate = candidate.next;
                    if (!this.IsDuplicateNamespaceAttribute(candidate))
                    {
                        return candidate;
                    }
                }
                while (candidate != parent.lastAttr);
            }
            return null;
        }

        private string GetLocalName()
        {
            if (this.IsInteractive)
            {
                XElement source = this.source as XElement;
                if (source != null)
                {
                    return source.Name.LocalName;
                }
                XAttribute attribute = this.source as XAttribute;
                if (attribute != null)
                {
                    return attribute.Name.LocalName;
                }
                XProcessingInstruction instruction = this.source as XProcessingInstruction;
                if (instruction != null)
                {
                    return instruction.Target;
                }
                XDocumentType type = this.source as XDocumentType;
                if (type != null)
                {
                    return type.Name;
                }
            }
            return string.Empty;
        }

        private static void GetNameInAttributeScope(string qualifiedName, XElement e, out string localName, out string namespaceName)
        {
            if ((qualifiedName != null) && (qualifiedName.Length != 0))
            {
                int index = qualifiedName.IndexOf(':');
                if ((index != 0) && (index != (qualifiedName.Length - 1)))
                {
                    if (index == -1)
                    {
                        localName = qualifiedName;
                        namespaceName = string.Empty;
                        return;
                    }
                    XNamespace namespaceOfPrefix = e.GetNamespaceOfPrefix(qualifiedName.Substring(0, index));
                    if (namespaceOfPrefix != null)
                    {
                        localName = qualifiedName.Substring(index + 1, (qualifiedName.Length - index) - 1);
                        namespaceName = namespaceOfPrefix.NamespaceName;
                        return;
                    }
                }
            }
            localName = null;
            namespaceName = null;
        }

        private string GetNamespaceURI()
        {
            if (!this.IsInteractive)
            {
                return string.Empty;
            }
            XElement source = this.source as XElement;
            if (source != null)
            {
                return source.Name.NamespaceName;
            }
            XAttribute attribute = this.source as XAttribute;
            if (attribute == null)
            {
                return string.Empty;
            }
            string namespaceName = attribute.Name.NamespaceName;
            if ((namespaceName.Length == 0) && (attribute.Name.LocalName == "xmlns"))
            {
                return "http://www.w3.org/2000/xmlns/";
            }
            return namespaceName;
        }

        private string GetPrefix()
        {
            if (this.IsInteractive)
            {
                XElement source = this.source as XElement;
                if (source != null)
                {
                    string prefixOfNamespace = source.GetPrefixOfNamespace(source.Name.Namespace);
                    if (prefixOfNamespace != null)
                    {
                        return prefixOfNamespace;
                    }
                    return string.Empty;
                }
                XAttribute attribute = this.source as XAttribute;
                if (attribute != null)
                {
                    string str2 = attribute.GetPrefixOfNamespace(attribute.Name.Namespace);
                    if (str2 != null)
                    {
                        return str2;
                    }
                }
            }
            return string.Empty;
        }

        private bool IsDuplicateNamespaceAttribute(XAttribute candidateAttribute)
        {
            if (!candidateAttribute.IsNamespaceDeclaration)
            {
                return false;
            }
            return this.IsDuplicateNamespaceAttributeInner(candidateAttribute);
        }

        private bool IsDuplicateNamespaceAttributeInner(XAttribute candidateAttribute)
        {
            if (candidateAttribute.Name.LocalName == "xml")
            {
                return true;
            }
            XElement parent = candidateAttribute.parent as XElement;
            if ((parent != this.root) && (parent != null))
            {
                for (parent = parent.parent as XElement; parent != null; parent = parent.parent as XElement)
                {
                    XAttribute lastAttr = parent.lastAttr;
                    if (lastAttr != null)
                    {
                        do
                        {
                            if (lastAttr.name == candidateAttribute.name)
                            {
                                return (lastAttr.Value == candidateAttribute.Value);
                            }
                            lastAttr = lastAttr.next;
                        }
                        while (lastAttr != parent.lastAttr);
                    }
                    if (parent == this.root)
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public override string LookupNamespace(string prefix)
        {
            if (this.IsInteractive)
            {
                if (prefix == null)
                {
                    return null;
                }
                XElement elementInScope = this.GetElementInScope();
                if (elementInScope != null)
                {
                    XNamespace namespace2 = (prefix.Length == 0) ? elementInScope.GetDefaultNamespace() : elementInScope.GetNamespaceOfPrefix(prefix);
                    if (namespace2 != null)
                    {
                        return this.nameTable.Add(namespace2.NamespaceName);
                    }
                }
            }
            return null;
        }

        public override void MoveToAttribute(int index)
        {
            if (this.IsInteractive)
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                XElement elementInAttributeScope = this.GetElementInAttributeScope();
                if (elementInAttributeScope != null)
                {
                    XAttribute lastAttr = elementInAttributeScope.lastAttr;
                    if (lastAttr != null)
                    {
                        do
                        {
                            lastAttr = lastAttr.next;
                            if ((!this.omitDuplicateNamespaces || !this.IsDuplicateNamespaceAttribute(lastAttr)) && (index-- == 0))
                            {
                                this.source = lastAttr;
                                this.parent = null;
                                return;
                            }
                        }
                        while (lastAttr != elementInAttributeScope.lastAttr);
                    }
                }
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public override bool MoveToAttribute(string name)
        {
            if (this.IsInteractive)
            {
                XElement elementInAttributeScope = this.GetElementInAttributeScope();
                if (elementInAttributeScope != null)
                {
                    string str;
                    string str2;
                    GetNameInAttributeScope(name, elementInAttributeScope, out str, out str2);
                    XAttribute lastAttr = elementInAttributeScope.lastAttr;
                    if (lastAttr != null)
                    {
                        do
                        {
                            lastAttr = lastAttr.next;
                            if ((lastAttr.Name.LocalName == str) && (lastAttr.Name.NamespaceName == str2))
                            {
                                if (this.omitDuplicateNamespaces && this.IsDuplicateNamespaceAttribute(lastAttr))
                                {
                                    return false;
                                }
                                this.source = lastAttr;
                                this.parent = null;
                                return true;
                            }
                        }
                        while (lastAttr != elementInAttributeScope.lastAttr);
                    }
                }
            }
            return false;
        }

        public override bool MoveToAttribute(string localName, string namespaceName)
        {
            if (this.IsInteractive)
            {
                XElement elementInAttributeScope = this.GetElementInAttributeScope();
                if (elementInAttributeScope != null)
                {
                    if (localName == "xmlns")
                    {
                        if ((namespaceName != null) && (namespaceName.Length == 0))
                        {
                            return false;
                        }
                        if (namespaceName == "http://www.w3.org/2000/xmlns/")
                        {
                            namespaceName = string.Empty;
                        }
                    }
                    XAttribute lastAttr = elementInAttributeScope.lastAttr;
                    if (lastAttr != null)
                    {
                        do
                        {
                            lastAttr = lastAttr.next;
                            if ((lastAttr.Name.LocalName == localName) && (lastAttr.Name.NamespaceName == namespaceName))
                            {
                                if (this.omitDuplicateNamespaces && this.IsDuplicateNamespaceAttribute(lastAttr))
                                {
                                    return false;
                                }
                                this.source = lastAttr;
                                this.parent = null;
                                return true;
                            }
                        }
                        while (lastAttr != elementInAttributeScope.lastAttr);
                    }
                }
            }
            return false;
        }

        public override bool MoveToElement()
        {
            if (this.IsInteractive)
            {
                XAttribute source = this.source as XAttribute;
                if (source == null)
                {
                    source = this.parent as XAttribute;
                }
                if ((source != null) && (source.parent != null))
                {
                    this.source = source.parent;
                    this.parent = null;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToFirstAttribute()
        {
            if (!this.IsInteractive)
            {
                return false;
            }
            XElement elementInAttributeScope = this.GetElementInAttributeScope();
            if ((elementInAttributeScope == null) || (elementInAttributeScope.lastAttr == null))
            {
                return false;
            }
            if (this.omitDuplicateNamespaces)
            {
                object firstNonDuplicateNamespaceAttribute = this.GetFirstNonDuplicateNamespaceAttribute(elementInAttributeScope.lastAttr.next);
                if (firstNonDuplicateNamespaceAttribute == null)
                {
                    return false;
                }
                this.source = firstNonDuplicateNamespaceAttribute;
            }
            else
            {
                this.source = elementInAttributeScope.lastAttr.next;
            }
            return true;
        }

        public override bool MoveToNextAttribute()
        {
            if (!this.IsInteractive)
            {
                return false;
            }
            XElement source = this.source as XElement;
            if (source != null)
            {
                if (this.IsEndElement)
                {
                    return false;
                }
                if (source.lastAttr == null)
                {
                    return false;
                }
                if (this.omitDuplicateNamespaces)
                {
                    object firstNonDuplicateNamespaceAttribute = this.GetFirstNonDuplicateNamespaceAttribute(source.lastAttr.next);
                    if (firstNonDuplicateNamespaceAttribute == null)
                    {
                        return false;
                    }
                    this.source = firstNonDuplicateNamespaceAttribute;
                }
                else
                {
                    this.source = source.lastAttr.next;
                }
                return true;
            }
            XAttribute parent = this.source as XAttribute;
            if (parent == null)
            {
                parent = this.parent as XAttribute;
            }
            if (((parent == null) || (parent.parent == null)) || (((XElement) parent.parent).lastAttr == parent))
            {
                return false;
            }
            if (this.omitDuplicateNamespaces)
            {
                object obj3 = this.GetFirstNonDuplicateNamespaceAttribute(parent.next);
                if (obj3 == null)
                {
                    return false;
                }
                this.source = obj3;
            }
            else
            {
                this.source = parent.next;
            }
            this.parent = null;
            return true;
        }

        public override bool Read()
        {
            switch (this.state)
            {
                case System.Xml.ReadState.Initial:
                {
                    this.state = System.Xml.ReadState.Interactive;
                    XDocument source = this.source as XDocument;
                    return ((source == null) || this.ReadIntoDocument(source));
                }
                case System.Xml.ReadState.Interactive:
                    return this.Read(false);
            }
            return false;
        }

        private bool Read(bool skipContent)
        {
            XElement source = this.source as XElement;
            if (source != null)
            {
                if ((!source.IsEmpty && !this.IsEndElement) && !skipContent)
                {
                    return this.ReadIntoElement(source);
                }
                return this.ReadOverNode(source);
            }
            XNode n = this.source as XNode;
            if (n != null)
            {
                return this.ReadOverNode(n);
            }
            XAttribute a = this.source as XAttribute;
            if (a != null)
            {
                return this.ReadOverAttribute(a, skipContent);
            }
            return this.ReadOverText(skipContent);
        }

        public override bool ReadAttributeValue()
        {
            if (!this.IsInteractive)
            {
                return false;
            }
            XAttribute source = this.source as XAttribute;
            return ((source != null) && this.ReadIntoAttribute(source));
        }

        private bool ReadIntoAttribute(XAttribute a)
        {
            this.source = a.value;
            this.parent = a;
            return true;
        }

        private bool ReadIntoDocument(XDocument d)
        {
            XNode content = d.content as XNode;
            if (content != null)
            {
                this.source = content.next;
                return true;
            }
            string str = d.content as string;
            if ((str != null) && (str.Length > 0))
            {
                this.source = str;
                this.parent = d;
                return true;
            }
            return this.ReadToEnd();
        }

        private bool ReadIntoElement(XElement e)
        {
            XNode content = e.content as XNode;
            if (content != null)
            {
                this.source = content.next;
                return true;
            }
            string str = e.content as string;
            if (str == null)
            {
                return this.ReadToEnd();
            }
            if (str.Length > 0)
            {
                this.source = str;
                this.parent = e;
            }
            else
            {
                this.source = e;
                this.IsEndElement = true;
            }
            return true;
        }

        private bool ReadOverAttribute(XAttribute a, bool skipContent)
        {
            XElement parent = (XElement) a.parent;
            if (parent == null)
            {
                return this.ReadToEnd();
            }
            if (!parent.IsEmpty && !skipContent)
            {
                return this.ReadIntoElement(parent);
            }
            return this.ReadOverNode(parent);
        }

        private bool ReadOverNode(XNode n)
        {
            if (n == this.root)
            {
                return this.ReadToEnd();
            }
            XNode next = n.next;
            if (((next == null) || (next == n)) || (n == n.parent.content))
            {
                if ((n.parent == null) || ((n.parent.parent == null) && (n.parent is XDocument)))
                {
                    return this.ReadToEnd();
                }
                this.source = n.parent;
                this.IsEndElement = true;
            }
            else
            {
                this.source = next;
                this.IsEndElement = false;
            }
            return true;
        }

        private bool ReadOverText(bool skipContent)
        {
            if (this.parent is XElement)
            {
                this.source = this.parent;
                this.parent = null;
                this.IsEndElement = true;
                return true;
            }
            if (this.parent is XAttribute)
            {
                XAttribute parent = (XAttribute) this.parent;
                this.parent = null;
                return this.ReadOverAttribute(parent, skipContent);
            }
            return this.ReadToEnd();
        }

        public override bool ReadToDescendant(string localName, string namespaceName)
        {
            if (this.IsInteractive)
            {
                this.MoveToElement();
                XElement source = this.source as XElement;
                if ((source != null) && !source.IsEmpty)
                {
                    if (this.IsEndElement)
                    {
                        return false;
                    }
                    foreach (XElement element2 in source.Descendants())
                    {
                        if ((element2.Name.LocalName == localName) && (element2.Name.NamespaceName == namespaceName))
                        {
                            this.source = element2;
                            return true;
                        }
                    }
                    this.IsEndElement = true;
                }
            }
            return false;
        }

        private bool ReadToEnd()
        {
            this.state = System.Xml.ReadState.EndOfFile;
            return false;
        }

        public override bool ReadToFollowing(string localName, string namespaceName)
        {
            while (this.Read())
            {
                XElement source = this.source as XElement;
                if (((source != null) && !this.IsEndElement) && ((source.Name.LocalName == localName) && (source.Name.NamespaceName == namespaceName)))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool ReadToNextSibling(string localName, string namespaceName)
        {
            if (!this.IsInteractive)
            {
                return false;
            }
            this.MoveToElement();
            if (this.source != this.root)
            {
                XNode source = this.source as XNode;
                if (source != null)
                {
                    foreach (XElement element in source.ElementsAfterSelf())
                    {
                        if ((element.Name.LocalName == localName) && (element.Name.NamespaceName == namespaceName))
                        {
                            this.source = element;
                            this.IsEndElement = false;
                            return true;
                        }
                    }
                    if (source.parent is XElement)
                    {
                        this.source = source.parent;
                        this.IsEndElement = true;
                        return false;
                    }
                }
                else if (this.parent is XElement)
                {
                    this.source = this.parent;
                    this.parent = null;
                    this.IsEndElement = true;
                    return false;
                }
            }
            return this.ReadToEnd();
        }

        public override void ResolveEntity()
        {
        }

        public override void Skip()
        {
            if (this.IsInteractive)
            {
                this.Read(true);
            }
        }

        bool IXmlLineInfo.HasLineInfo()
        {
            if (this.IsEndElement)
            {
                XElement source = this.source as XElement;
                if (source != null)
                {
                    return (source.Annotation<LineInfoEndElementAnnotation>() != null);
                }
            }
            else
            {
                IXmlLineInfo info = this.source as IXmlLineInfo;
                if (info != null)
                {
                    return info.HasLineInfo();
                }
            }
            return false;
        }

        public override int AttributeCount
        {
            get
            {
                if (!this.IsInteractive)
                {
                    return 0;
                }
                int num = 0;
                XElement elementInAttributeScope = this.GetElementInAttributeScope();
                if (elementInAttributeScope != null)
                {
                    XAttribute lastAttr = elementInAttributeScope.lastAttr;
                    if (lastAttr == null)
                    {
                        return num;
                    }
                    do
                    {
                        lastAttr = lastAttr.next;
                        if (!this.omitDuplicateNamespaces || !this.IsDuplicateNamespaceAttribute(lastAttr))
                        {
                            num++;
                        }
                    }
                    while (lastAttr != elementInAttributeScope.lastAttr);
                }
                return num;
            }
        }

        public override string BaseURI
        {
            get
            {
                XObject source = this.source as XObject;
                if (source != null)
                {
                    return source.BaseUri;
                }
                source = this.parent as XObject;
                if (source != null)
                {
                    return source.BaseUri;
                }
                return string.Empty;
            }
        }

        public override int Depth
        {
            get
            {
                if (this.IsInteractive)
                {
                    XObject source = this.source as XObject;
                    if (source != null)
                    {
                        return GetDepth(source);
                    }
                    source = this.parent as XObject;
                    if (source != null)
                    {
                        return (GetDepth(source) + 1);
                    }
                }
                return 0;
            }
        }

        internal override IDtdInfo DtdInfo
        {
            get
            {
                if (!this.dtdInfoInitialized)
                {
                    this.dtdInfoInitialized = true;
                    XDocumentType source = this.source as XDocumentType;
                    if (source == null)
                    {
                        for (XNode node = this.root; node != null; node = node.parent)
                        {
                            XDocument document = node as XDocument;
                            if (document != null)
                            {
                                source = document.DocumentType;
                                break;
                            }
                        }
                    }
                    if (source != null)
                    {
                        this.dtdInfo = source.DtdInfo;
                    }
                }
                return this.dtdInfo;
            }
        }

        public override bool EOF
        {
            get
            {
                return (this.state == System.Xml.ReadState.EndOfFile);
            }
        }

        public override bool HasAttributes
        {
            get
            {
                if (!this.IsInteractive)
                {
                    return false;
                }
                XElement elementInAttributeScope = this.GetElementInAttributeScope();
                if ((elementInAttributeScope == null) || (elementInAttributeScope.lastAttr == null))
                {
                    return false;
                }
                if (this.omitDuplicateNamespaces)
                {
                    return (this.GetFirstNonDuplicateNamespaceAttribute(elementInAttributeScope.lastAttr.next) != null);
                }
                return true;
            }
        }

        public override bool HasValue
        {
            get
            {
                if (this.IsInteractive)
                {
                    XObject source = this.source as XObject;
                    if (source == null)
                    {
                        return true;
                    }
                    switch (source.NodeType)
                    {
                        case XmlNodeType.Attribute:
                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                        case XmlNodeType.ProcessingInstruction:
                        case XmlNodeType.Comment:
                        case XmlNodeType.DocumentType:
                            return true;
                    }
                }
                return false;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                if (!this.IsInteractive)
                {
                    return false;
                }
                XElement source = this.source as XElement;
                return ((source != null) && source.IsEmpty);
            }
        }

        private bool IsEndElement
        {
            get
            {
                return (this.parent == this.source);
            }
            set
            {
                this.parent = value ? this.source : null;
            }
        }

        private bool IsInteractive
        {
            get
            {
                return (this.state == System.Xml.ReadState.Interactive);
            }
        }

        public override string LocalName
        {
            get
            {
                return this.nameTable.Add(this.GetLocalName());
            }
        }

        public override string Name
        {
            get
            {
                string prefix = this.GetPrefix();
                if (prefix.Length == 0)
                {
                    return this.nameTable.Add(this.GetLocalName());
                }
                return this.nameTable.Add(prefix + ":" + this.GetLocalName());
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.nameTable.Add(this.GetNamespaceURI());
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
                if (!this.IsInteractive)
                {
                    return XmlNodeType.None;
                }
                XObject source = this.source as XObject;
                if (source != null)
                {
                    if (this.IsEndElement)
                    {
                        return XmlNodeType.EndElement;
                    }
                    XmlNodeType nodeType = source.NodeType;
                    if (nodeType != XmlNodeType.Text)
                    {
                        return nodeType;
                    }
                    if (((source.parent != null) && (source.parent.parent == null)) && (source.parent is XDocument))
                    {
                        return XmlNodeType.Whitespace;
                    }
                    return XmlNodeType.Text;
                }
                if (this.parent is XDocument)
                {
                    return XmlNodeType.Whitespace;
                }
                return XmlNodeType.Text;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.nameTable.Add(this.GetPrefix());
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                return this.state;
            }
        }

        public override XmlReaderSettings Settings
        {
            get
            {
                return new XmlReaderSettings { CheckCharacters = false };
            }
        }

        int IXmlLineInfo.LineNumber
        {
            get
            {
                if (this.IsEndElement)
                {
                    XElement source = this.source as XElement;
                    if (source != null)
                    {
                        LineInfoEndElementAnnotation annotation = source.Annotation<LineInfoEndElementAnnotation>();
                        if (annotation != null)
                        {
                            return annotation.lineNumber;
                        }
                    }
                }
                else
                {
                    IXmlLineInfo info = this.source as IXmlLineInfo;
                    if (info != null)
                    {
                        return info.LineNumber;
                    }
                }
                return 0;
            }
        }

        int IXmlLineInfo.LinePosition
        {
            get
            {
                if (this.IsEndElement)
                {
                    XElement source = this.source as XElement;
                    if (source != null)
                    {
                        LineInfoEndElementAnnotation annotation = source.Annotation<LineInfoEndElementAnnotation>();
                        if (annotation != null)
                        {
                            return annotation.linePosition;
                        }
                    }
                }
                else
                {
                    IXmlLineInfo info = this.source as IXmlLineInfo;
                    if (info != null)
                    {
                        return info.LinePosition;
                    }
                }
                return 0;
            }
        }

        public override string Value
        {
            get
            {
                if (this.IsInteractive)
                {
                    XObject source = this.source as XObject;
                    if (source == null)
                    {
                        return (string) this.source;
                    }
                    switch (source.NodeType)
                    {
                        case XmlNodeType.Attribute:
                            return ((XAttribute) source).Value;

                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                            return ((XText) source).Value;

                        case XmlNodeType.ProcessingInstruction:
                            return ((XProcessingInstruction) source).Data;

                        case XmlNodeType.Comment:
                            return ((XComment) source).Value;

                        case XmlNodeType.DocumentType:
                            return ((XDocumentType) source).InternalSubset;
                    }
                }
                return string.Empty;
            }
        }

        public override string XmlLang
        {
            get
            {
                if (this.IsInteractive)
                {
                    XElement elementInScope = this.GetElementInScope();
                    if (elementInScope != null)
                    {
                        XName name = XNamespace.Xml.GetName("lang");
                        do
                        {
                            XAttribute attribute = elementInScope.Attribute(name);
                            if (attribute != null)
                            {
                                return attribute.Value;
                            }
                            elementInScope = elementInScope.parent as XElement;
                        }
                        while (elementInScope != null);
                    }
                }
                return string.Empty;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                if (this.IsInteractive)
                {
                    XElement elementInScope = this.GetElementInScope();
                    if (elementInScope != null)
                    {
                        XName name = XNamespace.Xml.GetName("space");
                        do
                        {
                            string str;
                            XAttribute attribute = elementInScope.Attribute(name);
                            if ((attribute != null) && ((str = attribute.Value.Trim(new char[] { ' ', '\t', '\n', '\r' })) != null))
                            {
                                if (str == "preserve")
                                {
                                    return System.Xml.XmlSpace.Preserve;
                                }
                                if (str == "default")
                                {
                                    return System.Xml.XmlSpace.Default;
                                }
                            }
                            elementInScope = elementInScope.parent as XElement;
                        }
                        while (elementInScope != null);
                    }
                }
                return System.Xml.XmlSpace.None;
            }
        }
    }
}

