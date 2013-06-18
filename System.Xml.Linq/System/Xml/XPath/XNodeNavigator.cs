namespace System.Xml.XPath
{
    using System;
    using System.Threading;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;

    internal class XNodeNavigator : XPathNavigator, IXmlLineInfo
    {
        private const int DocumentContentMask = 0x182;
        private static readonly int[] ElementContentMasks = new int[] { 0, 2, 0, 0, 0x18, 0, 0, 0x80, 0x100, 410 };
        private XmlNameTable nameTable;
        private XElement parent;
        private object source;
        private const int TextMask = 0x18;
        private static XAttribute XmlNamespaceDeclaration;

        public XNodeNavigator(XNodeNavigator other)
        {
            this.source = other.source;
            this.parent = other.parent;
            this.nameTable = other.nameTable;
        }

        public XNodeNavigator(XNode node, XmlNameTable nameTable)
        {
            this.source = node;
            this.nameTable = (nameTable != null) ? nameTable : CreateNameTable();
        }

        public override bool CheckValidity(XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
        {
            throw new NotSupportedException(System.Xml.Linq.Res.GetString("NotSupported_CheckValidity"));
        }

        public override XPathNavigator Clone()
        {
            return new XNodeNavigator(this);
        }

        private static string CollectText(XText n)
        {
            string str = n.Value;
            if (n.parent != null)
            {
                while (n != n.parent.content)
                {
                    n = n.next as XText;
                    if (n == null)
                    {
                        return str;
                    }
                    str = str + n.Value;
                }
            }
            return str;
        }

        private static XmlNameTable CreateNameTable()
        {
            XmlNameTable table = new System.Xml.NameTable();
            table.Add(string.Empty);
            table.Add("http://www.w3.org/2000/xmlns/");
            table.Add("http://www.w3.org/XML/1998/namespace");
            return table;
        }

        private static int GetElementContentMask(XPathNodeType type)
        {
            return ElementContentMasks[(int) type];
        }

        private static XAttribute GetFirstNamespaceDeclarationGlobal(XElement e)
        {
            do
            {
                XAttribute firstNamespaceDeclarationLocal = GetFirstNamespaceDeclarationLocal(e);
                if (firstNamespaceDeclarationLocal != null)
                {
                    return firstNamespaceDeclarationLocal;
                }
                e = e.parent as XElement;
            }
            while (e != null);
            return null;
        }

        private static XAttribute GetFirstNamespaceDeclarationLocal(XElement e)
        {
            XAttribute lastAttr = e.lastAttr;
            if (lastAttr != null)
            {
                do
                {
                    lastAttr = lastAttr.next;
                    if (lastAttr.IsNamespaceDeclaration)
                    {
                        return lastAttr;
                    }
                }
                while (lastAttr != e.lastAttr);
            }
            return null;
        }

        private string GetLocalName()
        {
            XElement source = this.source as XElement;
            if (source != null)
            {
                return source.Name.LocalName;
            }
            XAttribute attribute = this.source as XAttribute;
            if (attribute != null)
            {
                if ((this.parent != null) && (attribute.Name.NamespaceName.Length == 0))
                {
                    return string.Empty;
                }
                return attribute.Name.LocalName;
            }
            XProcessingInstruction instruction = this.source as XProcessingInstruction;
            if (instruction != null)
            {
                return instruction.Target;
            }
            return string.Empty;
        }

        private string GetNamespaceURI()
        {
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
            if (this.parent != null)
            {
                return string.Empty;
            }
            return attribute.Name.NamespaceName;
        }

        private static XAttribute GetNextNamespaceDeclarationGlobal(XAttribute a)
        {
            XElement parent = (XElement) a.parent;
            if (parent == null)
            {
                return null;
            }
            XAttribute nextNamespaceDeclarationLocal = GetNextNamespaceDeclarationLocal(a);
            if (nextNamespaceDeclarationLocal != null)
            {
                return nextNamespaceDeclarationLocal;
            }
            parent = parent.parent as XElement;
            if (parent == null)
            {
                return null;
            }
            return GetFirstNamespaceDeclarationGlobal(parent);
        }

        private static XAttribute GetNextNamespaceDeclarationLocal(XAttribute a)
        {
            XElement parent = (XElement) a.parent;
            if (parent != null)
            {
                while (a != parent.lastAttr)
                {
                    a = a.next;
                    if (a.IsNamespaceDeclaration)
                    {
                        return a;
                    }
                }
                return null;
            }
            return null;
        }

        private string GetPrefix()
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
                if (this.parent != null)
                {
                    return string.Empty;
                }
                string str2 = attribute.GetPrefixOfNamespace(attribute.Name.Namespace);
                if (str2 != null)
                {
                    return str2;
                }
            }
            return string.Empty;
        }

        private static XAttribute GetXmlNamespaceDeclaration()
        {
            if (XmlNamespaceDeclaration == null)
            {
                Interlocked.CompareExchange<XAttribute>(ref XmlNamespaceDeclaration, new XAttribute(XNamespace.Xmlns.GetName("xml"), "http://www.w3.org/XML/1998/namespace"), null);
            }
            return XmlNamespaceDeclaration;
        }

        private static bool HasNamespaceDeclarationInScope(XAttribute a, XElement e)
        {
            XName name = a.Name;
            while ((e != null) && (e != a.parent))
            {
                if (e.Attribute(name) != null)
                {
                    return true;
                }
                e = e.parent as XElement;
            }
            return false;
        }

        private static bool IsContent(XContainer c, XNode n)
        {
            if ((c.parent == null) && !(c is XElement))
            {
                return (((((int) 1) << n.NodeType) & 0x182) != 0);
            }
            return true;
        }

        public override bool IsSamePosition(XPathNavigator navigator)
        {
            XNodeNavigator navigator2 = navigator as XNodeNavigator;
            if (navigator2 == null)
            {
                return false;
            }
            return IsSamePosition(this, navigator2);
        }

        private static bool IsSamePosition(XNodeNavigator n1, XNodeNavigator n2)
        {
            if ((n1.source == n2.source) && (n1.parent == n2.parent))
            {
                return true;
            }
            if ((n1.parent != null) ^ (n2.parent != null))
            {
                XText source = n1.source as XText;
                if (source != null)
                {
                    return ((source.Value == n2.source) && (source.parent == n2.parent));
                }
                XText text2 = n2.source as XText;
                if (text2 != null)
                {
                    return ((text2.Value == n1.source) && (text2.parent == n1.parent));
                }
            }
            return false;
        }

        private static bool IsXmlNamespaceDeclaration(XAttribute a)
        {
            return (a == GetXmlNamespaceDeclaration());
        }

        public override bool MoveTo(XPathNavigator navigator)
        {
            XNodeNavigator navigator2 = navigator as XNodeNavigator;
            if (navigator2 != null)
            {
                this.source = navigator2.source;
                this.parent = navigator2.parent;
                return true;
            }
            return false;
        }

        public override bool MoveToAttribute(string localName, string namespaceName)
        {
            XElement source = this.source as XElement;
            if (source != null)
            {
                XAttribute lastAttr = source.lastAttr;
                if (lastAttr != null)
                {
                    do
                    {
                        lastAttr = lastAttr.next;
                        if (((lastAttr.Name.LocalName == localName) && (lastAttr.Name.NamespaceName == namespaceName)) && !lastAttr.IsNamespaceDeclaration)
                        {
                            this.source = lastAttr;
                            return true;
                        }
                    }
                    while (lastAttr != source.lastAttr);
                }
            }
            return false;
        }

        public override bool MoveToChild(XPathNodeType type)
        {
            XContainer source = this.source as XContainer;
            if ((source != null) && (source.content != null))
            {
                XNode content = source.content as XNode;
                if (content != null)
                {
                    int elementContentMask = GetElementContentMask(type);
                    if ((((0x18 & elementContentMask) != 0) && (source.parent == null)) && (source is XDocument))
                    {
                        elementContentMask &= -25;
                    }
                    do
                    {
                        content = content.next;
                        if (((((int) 1) << content.NodeType) & elementContentMask) != 0)
                        {
                            this.source = content;
                            return true;
                        }
                    }
                    while (content != source.content);
                    return false;
                }
                string str = (string) source.content;
                if (str.Length != 0)
                {
                    int num2 = GetElementContentMask(type);
                    if ((((0x18 & num2) != 0) && (source.parent == null)) && (source is XDocument))
                    {
                        return false;
                    }
                    if ((8 & num2) != 0)
                    {
                        this.source = str;
                        this.parent = (XElement) source;
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool MoveToChild(string localName, string namespaceName)
        {
            XContainer source = this.source as XContainer;
            if ((source != null) && (source.content != null))
            {
                XNode content = source.content as XNode;
                if (content != null)
                {
                    do
                    {
                        content = content.next;
                        XElement element = content as XElement;
                        if (((element != null) && (element.Name.LocalName == localName)) && (element.Name.NamespaceName == namespaceName))
                        {
                            this.source = element;
                            return true;
                        }
                    }
                    while (content != source.content);
                }
            }
            return false;
        }

        public override bool MoveToFirstAttribute()
        {
            XElement source = this.source as XElement;
            if (source != null)
            {
                XAttribute lastAttr = source.lastAttr;
                if (lastAttr != null)
                {
                    do
                    {
                        lastAttr = lastAttr.next;
                        if (!lastAttr.IsNamespaceDeclaration)
                        {
                            this.source = lastAttr;
                            return true;
                        }
                    }
                    while (lastAttr != source.lastAttr);
                }
            }
            return false;
        }

        public override bool MoveToFirstChild()
        {
            XContainer source = this.source as XContainer;
            if ((source != null) && (source.content != null))
            {
                XNode content = source.content as XNode;
                if (content != null)
                {
                    do
                    {
                        content = content.next;
                        if (IsContent(source, content))
                        {
                            this.source = content;
                            return true;
                        }
                    }
                    while (content != source.content);
                    return false;
                }
                string str = (string) source.content;
                if ((str.Length != 0) && ((source.parent != null) || (source is XElement)))
                {
                    this.source = str;
                    this.parent = (XElement) source;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope scope)
        {
            XElement source = this.source as XElement;
            if (source != null)
            {
                XAttribute a = null;
                switch (scope)
                {
                    case XPathNamespaceScope.All:
                        a = GetFirstNamespaceDeclarationGlobal(source);
                        if (a == null)
                        {
                            a = GetXmlNamespaceDeclaration();
                        }
                        break;

                    case XPathNamespaceScope.ExcludeXml:
                        for (a = GetFirstNamespaceDeclarationGlobal(source); (a != null) && (a.Name.LocalName == "xml"); a = GetNextNamespaceDeclarationGlobal(a))
                        {
                        }
                        break;

                    case XPathNamespaceScope.Local:
                        a = GetFirstNamespaceDeclarationLocal(source);
                        break;
                }
                if (a != null)
                {
                    this.source = a;
                    this.parent = source;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToId(string id)
        {
            throw new NotSupportedException(System.Xml.Linq.Res.GetString("NotSupported_MoveToId"));
        }

        public override bool MoveToNamespace(string localName)
        {
            XElement source = this.source as XElement;
            if (source != null)
            {
                if (localName == "xmlns")
                {
                    return false;
                }
                if ((localName != null) && (localName.Length == 0))
                {
                    localName = "xmlns";
                }
                for (XAttribute attribute = GetFirstNamespaceDeclarationGlobal(source); attribute != null; attribute = GetNextNamespaceDeclarationGlobal(attribute))
                {
                    if (attribute.Name.LocalName == localName)
                    {
                        this.source = attribute;
                        this.parent = source;
                        return true;
                    }
                }
                if (localName == "xml")
                {
                    this.source = GetXmlNamespaceDeclaration();
                    this.parent = source;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToNext()
        {
            XNode source = this.source as XNode;
            if (source != null)
            {
                XContainer parent = source.parent;
                if ((parent != null) && (source != parent.content))
                {
                    do
                    {
                        XNode next = source.next;
                        if (IsContent(parent, next) && (!(source is XText) || !(next is XText)))
                        {
                            this.source = next;
                            return true;
                        }
                        source = next;
                    }
                    while (source != parent.content);
                }
            }
            return false;
        }

        public override bool MoveToNext(XPathNodeType type)
        {
            XNode source = this.source as XNode;
            if (source != null)
            {
                XContainer parent = source.parent;
                if ((parent != null) && (source != parent.content))
                {
                    int elementContentMask = GetElementContentMask(type);
                    if ((((0x18 & elementContentMask) != 0) && (parent.parent == null)) && (parent is XDocument))
                    {
                        elementContentMask &= -25;
                    }
                    do
                    {
                        XNode next = source.next;
                        if ((((((int) 1) << next.NodeType) & elementContentMask) != 0) && (!(source is XText) || !(next is XText)))
                        {
                            this.source = next;
                            return true;
                        }
                        source = next;
                    }
                    while (source != parent.content);
                }
            }
            return false;
        }

        public override bool MoveToNext(string localName, string namespaceName)
        {
            XNode source = this.source as XNode;
            if (source != null)
            {
                XContainer parent = source.parent;
                if ((parent != null) && (source != parent.content))
                {
                    do
                    {
                        source = source.next;
                        XElement element = source as XElement;
                        if (((element != null) && (element.Name.LocalName == localName)) && (element.Name.NamespaceName == namespaceName))
                        {
                            this.source = element;
                            return true;
                        }
                    }
                    while (source != parent.content);
                }
            }
            return false;
        }

        public override bool MoveToNextAttribute()
        {
            XAttribute source = this.source as XAttribute;
            if ((source != null) && (this.parent == null))
            {
                XElement parent = (XElement) source.parent;
                if (parent != null)
                {
                    while (source != parent.lastAttr)
                    {
                        source = source.next;
                        if (!source.IsNamespaceDeclaration)
                        {
                            this.source = source;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope scope)
        {
            XAttribute source = this.source as XAttribute;
            if (((source != null) && (this.parent != null)) && !IsXmlNamespaceDeclaration(source))
            {
                switch (scope)
                {
                    case XPathNamespaceScope.All:
                        do
                        {
                            source = GetNextNamespaceDeclarationGlobal(source);
                        }
                        while ((source != null) && HasNamespaceDeclarationInScope(source, this.parent));
                        if ((source == null) && !HasNamespaceDeclarationInScope(GetXmlNamespaceDeclaration(), this.parent))
                        {
                            source = GetXmlNamespaceDeclaration();
                        }
                        break;

                    case XPathNamespaceScope.ExcludeXml:
                        do
                        {
                            source = GetNextNamespaceDeclarationGlobal(source);
                        }
                        while ((source != null) && ((source.Name.LocalName == "xml") || HasNamespaceDeclarationInScope(source, this.parent)));
                        break;

                    case XPathNamespaceScope.Local:
                        if (source.parent == this.parent)
                        {
                            source = GetNextNamespaceDeclarationLocal(source);
                            break;
                        }
                        return false;
                }
                if (source != null)
                {
                    this.source = source;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToParent()
        {
            if (this.parent != null)
            {
                this.source = this.parent;
                this.parent = null;
                return true;
            }
            XObject source = (XObject) this.source;
            if (source.parent != null)
            {
                this.source = source.parent;
                return true;
            }
            return false;
        }

        public override bool MoveToPrevious()
        {
            XNode source = this.source as XNode;
            if (source != null)
            {
                XContainer parent = source.parent;
                if (parent != null)
                {
                    XNode content = (XNode) parent.content;
                    if (content.next != source)
                    {
                        XNode node3 = null;
                        do
                        {
                            content = content.next;
                            if (IsContent(parent, content))
                            {
                                node3 = ((node3 is XText) && (content is XText)) ? node3 : content;
                            }
                        }
                        while (content.next != source);
                        if (node3 != null)
                        {
                            this.source = node3;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override XmlReader ReadSubtree()
        {
            XContainer source = this.source as XContainer;
            if (source == null)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_BadNodeType", new object[] { this.NodeType }));
            }
            return new XNodeReader(source, this.nameTable);
        }

        bool IXmlLineInfo.HasLineInfo()
        {
            IXmlLineInfo source = this.source as IXmlLineInfo;
            return ((source != null) && source.HasLineInfo());
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
                if (this.parent != null)
                {
                    return this.parent.BaseUri;
                }
                return string.Empty;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                XElement source = this.source as XElement;
                if (source != null)
                {
                    XAttribute lastAttr = source.lastAttr;
                    if (lastAttr != null)
                    {
                        do
                        {
                            lastAttr = lastAttr.next;
                            if (!lastAttr.IsNamespaceDeclaration)
                            {
                                return true;
                            }
                        }
                        while (lastAttr != source.lastAttr);
                    }
                }
                return false;
            }
        }

        public override bool HasChildren
        {
            get
            {
                XContainer source = this.source as XContainer;
                if ((source != null) && (source.content != null))
                {
                    XNode content = source.content as XNode;
                    if (content != null)
                    {
                        do
                        {
                            content = content.next;
                            if (IsContent(source, content))
                            {
                                return true;
                            }
                        }
                        while (content != source.content);
                        return false;
                    }
                    string str = (string) source.content;
                    if ((str.Length != 0) && ((source.parent != null) || (source is XElement)))
                    {
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
                XElement source = this.source as XElement;
                return ((source != null) && source.IsEmpty);
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

        public override XPathNodeType NodeType
        {
            get
            {
                XObject source = this.source as XObject;
                if (source != null)
                {
                    switch (source.NodeType)
                    {
                        case XmlNodeType.Element:
                            return XPathNodeType.Element;

                        case XmlNodeType.Attribute:
                            if (this.parent == null)
                            {
                                return XPathNodeType.Attribute;
                            }
                            return XPathNodeType.Namespace;

                        case XmlNodeType.ProcessingInstruction:
                            return XPathNodeType.ProcessingInstruction;

                        case XmlNodeType.Comment:
                            return XPathNodeType.Comment;

                        case XmlNodeType.Document:
                            return XPathNodeType.Root;
                    }
                }
                return XPathNodeType.Text;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.nameTable.Add(this.GetPrefix());
            }
        }

        int IXmlLineInfo.LineNumber
        {
            get
            {
                IXmlLineInfo source = this.source as IXmlLineInfo;
                if (source != null)
                {
                    return source.LineNumber;
                }
                return 0;
            }
        }

        int IXmlLineInfo.LinePosition
        {
            get
            {
                IXmlLineInfo source = this.source as IXmlLineInfo;
                if (source != null)
                {
                    return source.LinePosition;
                }
                return 0;
            }
        }

        public override object UnderlyingObject
        {
            get
            {
                if (this.source is string)
                {
                    this.source = this.parent.LastNode;
                    this.parent = null;
                }
                return this.source;
            }
        }

        public override string Value
        {
            get
            {
                XObject source = this.source as XObject;
                if (source == null)
                {
                    return (string) this.source;
                }
                switch (source.NodeType)
                {
                    case XmlNodeType.Element:
                        return ((XElement) source).Value;

                    case XmlNodeType.Attribute:
                        return ((XAttribute) source).Value;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        return CollectText((XText) source);

                    case XmlNodeType.ProcessingInstruction:
                        return ((XProcessingInstruction) source).Data;

                    case XmlNodeType.Comment:
                        return ((XComment) source).Value;

                    case XmlNodeType.Document:
                    {
                        XElement root = ((XDocument) source).Root;
                        if (root != null)
                        {
                            return root.Value;
                        }
                        return string.Empty;
                    }
                }
                return string.Empty;
            }
        }
    }
}

