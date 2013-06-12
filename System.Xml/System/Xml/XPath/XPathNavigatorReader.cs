namespace System.Xml.XPath
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Schema;

    internal class XPathNavigatorReader : XmlReader, IXmlNamespaceResolver
    {
        private int attrCount;
        internal static XmlNodeType[] convertFromXPathNodeType;
        private int depth;
        protected IXmlLineInfo lineInfo;
        private XPathNavigator nav;
        private XPathNavigator navToRead;
        private XmlNodeType nodeType;
        private ReadContentAsBinaryHelper readBinaryHelper;
        private bool readEntireDocument;
        private State savedState;
        protected IXmlSchemaInfo schemaInfo;
        internal const string space = "space";
        private State state;

        static XPathNavigatorReader()
        {
            XmlNodeType[] typeArray = new XmlNodeType[10];
            typeArray[0] = XmlNodeType.Document;
            typeArray[1] = XmlNodeType.Element;
            typeArray[2] = XmlNodeType.Attribute;
            typeArray[3] = XmlNodeType.Attribute;
            typeArray[4] = XmlNodeType.Text;
            typeArray[5] = XmlNodeType.SignificantWhitespace;
            typeArray[6] = XmlNodeType.Whitespace;
            typeArray[7] = XmlNodeType.ProcessingInstruction;
            typeArray[8] = XmlNodeType.Comment;
            convertFromXPathNodeType = typeArray;
        }

        protected XPathNavigatorReader(XPathNavigator navToRead, IXmlLineInfo xli, IXmlSchemaInfo xsi)
        {
            this.navToRead = navToRead;
            this.lineInfo = xli;
            this.schemaInfo = xsi;
            this.nav = XmlEmptyNavigator.Singleton;
            this.state = State.Initial;
            this.depth = 0;
            this.nodeType = ToXmlNodeType(this.nav.NodeType);
        }

        public override void Close()
        {
            this.nav = XmlEmptyNavigator.Singleton;
            this.nodeType = XmlNodeType.None;
            this.state = State.Closed;
            this.depth = 0;
        }

        public static XPathNavigatorReader Create(XPathNavigator navToRead)
        {
            XPathNavigator navigator = navToRead.Clone();
            IXmlLineInfo xli = navigator as IXmlLineInfo;
            IXmlSchemaInfo xsi = navigator as IXmlSchemaInfo;
            if (xsi == null)
            {
                return new XPathNavigatorReader(navigator, xli, xsi);
            }
            return new XPathNavigatorReaderWithSI(navigator, xli, xsi);
        }

        public override string GetAttribute(int index)
        {
            if (index >= 0)
            {
                XPathNavigator elemNav = this.GetElemNav();
                if (elemNav != null)
                {
                    if (elemNav.MoveToFirstNamespace(XPathNamespaceScope.Local))
                    {
                        int num;
                        string str = GetNamespaceByIndex(elemNav, index, out num);
                        if (str != null)
                        {
                            return str;
                        }
                        index -= num;
                        elemNav.MoveToParent();
                    }
                    if (elemNav.MoveToFirstAttribute())
                    {
                        do
                        {
                            if (index == 0)
                            {
                                return elemNav.Value;
                            }
                            index--;
                        }
                        while (elemNav.MoveToNextAttribute());
                    }
                }
            }
            throw new ArgumentOutOfRangeException("index");
        }

        public override string GetAttribute(string name)
        {
            string str;
            string str2;
            XPathNavigator nav = this.nav;
            switch (nav.NodeType)
            {
                case XPathNodeType.Element:
                    break;

                case XPathNodeType.Attribute:
                    nav = nav.Clone();
                    if (nav.MoveToParent())
                    {
                        break;
                    }
                    return null;

                default:
                    return null;
            }
            ValidateNames.SplitQName(name, out str, out str2);
            if (str.Length == 0)
            {
                if (str2 == "xmlns")
                {
                    return nav.GetNamespace(string.Empty);
                }
                if (nav == this.nav)
                {
                    nav = nav.Clone();
                }
                if (nav.MoveToAttribute(str2, string.Empty))
                {
                    return nav.Value;
                }
            }
            else
            {
                if (str == "xmlns")
                {
                    return nav.GetNamespace(str2);
                }
                if (nav == this.nav)
                {
                    nav = nav.Clone();
                }
                if (nav.MoveToFirstAttribute())
                {
                    do
                    {
                        if ((nav.LocalName == str2) && (nav.Prefix == str))
                        {
                            return nav.Value;
                        }
                    }
                    while (nav.MoveToNextAttribute());
                }
            }
            return null;
        }

        public override string GetAttribute(string localName, string namespaceURI)
        {
            if (localName == null)
            {
                throw new ArgumentNullException("localName");
            }
            XPathNavigator nav = this.nav;
            switch (nav.NodeType)
            {
                case XPathNodeType.Element:
                    break;

                case XPathNodeType.Attribute:
                    nav = nav.Clone();
                    if (nav.MoveToParent())
                    {
                        break;
                    }
                    return null;

                default:
                    return null;
            }
            if (namespaceURI == "http://www.w3.org/2000/xmlns/")
            {
                if (localName == "xmlns")
                {
                    localName = string.Empty;
                }
                return nav.GetNamespace(localName);
            }
            if (namespaceURI == null)
            {
                namespaceURI = string.Empty;
            }
            if (nav == this.nav)
            {
                nav = nav.Clone();
            }
            if (nav.MoveToAttribute(localName, namespaceURI))
            {
                return nav.Value;
            }
            return null;
        }

        private XPathNavigator GetElemNav()
        {
            switch (this.state)
            {
                case State.Content:
                    return this.nav.Clone();

                case State.Attribute:
                case State.AttrVal:
                {
                    XPathNavigator navigator = this.nav.Clone();
                    if (!navigator.MoveToParent())
                    {
                        break;
                    }
                    return navigator;
                }
                case State.InReadBinary:
                {
                    this.state = this.savedState;
                    XPathNavigator elemNav = this.GetElemNav();
                    this.state = State.InReadBinary;
                    return elemNav;
                }
            }
            return null;
        }

        private XPathNavigator GetElemNav(out int depth)
        {
            XPathNavigator elemNav = null;
            switch (this.state)
            {
                case State.Content:
                    if (this.nodeType == XmlNodeType.Element)
                    {
                        elemNav = this.nav.Clone();
                    }
                    depth = this.depth;
                    return elemNav;

                case State.Attribute:
                    elemNav = this.nav.Clone();
                    elemNav.MoveToParent();
                    depth = this.depth - 1;
                    return elemNav;

                case State.AttrVal:
                    elemNav = this.nav.Clone();
                    elemNav.MoveToParent();
                    depth = this.depth - 2;
                    return elemNav;

                case State.InReadBinary:
                    this.state = this.savedState;
                    elemNav = this.GetElemNav(out depth);
                    this.state = State.InReadBinary;
                    return elemNav;
            }
            depth = this.depth;
            return elemNav;
        }

        private static string GetNamespaceByIndex(XPathNavigator nav, int index, out int count)
        {
            string str = nav.Value;
            string str2 = null;
            if (nav.MoveToNextNamespace(XPathNamespaceScope.Local))
            {
                str2 = GetNamespaceByIndex(nav, index, out count);
            }
            else
            {
                count = 0;
            }
            if (count == index)
            {
                str2 = str;
            }
            count++;
            return str2;
        }

        public override string LookupNamespace(string prefix)
        {
            return this.nav.LookupNamespace(prefix);
        }

        private void MoveToAttr(XPathNavigator nav, int depth)
        {
            this.nav.MoveTo(nav);
            this.depth = depth;
            this.nodeType = XmlNodeType.Attribute;
            this.state = State.Attribute;
        }

        public override bool MoveToAttribute(string name)
        {
            int num;
            string str;
            string str2;
            XPathNavigator elemNav = this.GetElemNav(out num);
            if (elemNav == null)
            {
                return false;
            }
            ValidateNames.SplitQName(name, out str, out str2);
            bool flag = false;
            if ((flag = (str.Length == 0) && (str2 == "xmlns")) || (str == "xmlns"))
            {
                if (flag)
                {
                    str2 = string.Empty;
                }
                if (elemNav.MoveToFirstNamespace(XPathNamespaceScope.Local))
                {
                    do
                    {
                        if (elemNav.LocalName == str2)
                        {
                            goto Label_00B5;
                        }
                    }
                    while (elemNav.MoveToNextNamespace(XPathNamespaceScope.Local));
                }
            }
            else
            {
                if (str.Length == 0)
                {
                    if (!elemNav.MoveToAttribute(str2, string.Empty))
                    {
                        goto Label_00B3;
                    }
                    goto Label_00B5;
                }
                if (elemNav.MoveToFirstAttribute())
                {
                    do
                    {
                        if ((elemNav.LocalName == str2) && (elemNav.Prefix == str))
                        {
                            goto Label_00B5;
                        }
                    }
                    while (elemNav.MoveToNextAttribute());
                }
            }
        Label_00B3:
            return false;
        Label_00B5:
            if (this.state == State.InReadBinary)
            {
                this.readBinaryHelper.Finish();
                this.state = this.savedState;
            }
            this.MoveToAttr(elemNav, num + 1);
            return true;
        }

        public override bool MoveToAttribute(string localName, string namespaceName)
        {
            if (localName == null)
            {
                throw new ArgumentNullException("localName");
            }
            int depth = this.depth;
            XPathNavigator elemNav = this.GetElemNav(out depth);
            if (elemNav != null)
            {
                if (namespaceName == "http://www.w3.org/2000/xmlns/")
                {
                    if (localName == "xmlns")
                    {
                        localName = string.Empty;
                    }
                    if (elemNav.MoveToFirstNamespace(XPathNamespaceScope.Local))
                    {
                        do
                        {
                            if (elemNav.LocalName == localName)
                            {
                                goto Label_007A;
                            }
                        }
                        while (elemNav.MoveToNextNamespace(XPathNamespaceScope.Local));
                    }
                }
                else
                {
                    if (namespaceName == null)
                    {
                        namespaceName = string.Empty;
                    }
                    if (elemNav.MoveToAttribute(localName, namespaceName))
                    {
                        goto Label_007A;
                    }
                }
            }
            return false;
        Label_007A:
            if (this.state == State.InReadBinary)
            {
                this.readBinaryHelper.Finish();
                this.state = this.savedState;
            }
            this.MoveToAttr(elemNav, depth + 1);
            return true;
        }

        public override bool MoveToElement()
        {
            switch (this.state)
            {
                case State.Attribute:
                case State.AttrVal:
                    if (this.nav.MoveToParent())
                    {
                        this.depth--;
                        if (this.state == State.AttrVal)
                        {
                            this.depth--;
                        }
                        this.state = State.Content;
                        this.nodeType = XmlNodeType.Element;
                        return true;
                    }
                    return false;

                case State.InReadBinary:
                    this.state = this.savedState;
                    if (this.MoveToElement())
                    {
                        this.readBinaryHelper.Finish();
                        break;
                    }
                    this.state = State.InReadBinary;
                    return false;
            }
            return false;
        }

        public override bool MoveToFirstAttribute()
        {
            int num;
            XPathNavigator elemNav = this.GetElemNav(out num);
            if (elemNav != null)
            {
                if (elemNav.MoveToFirstNamespace(XPathNamespaceScope.Local))
                {
                    while (elemNav.MoveToNextNamespace(XPathNamespaceScope.Local))
                    {
                    }
                    goto Label_002A;
                }
                if (elemNav.MoveToFirstAttribute())
                {
                    goto Label_002A;
                }
            }
            return false;
        Label_002A:
            if (this.state == State.InReadBinary)
            {
                this.readBinaryHelper.Finish();
                this.state = this.savedState;
            }
            this.MoveToAttr(elemNav, num + 1);
            return true;
        }

        public override bool MoveToNextAttribute()
        {
            switch (this.state)
            {
                case State.Content:
                    return this.MoveToFirstAttribute();

                case State.Attribute:
                    if (XPathNodeType.Attribute != this.nav.NodeType)
                    {
                        XPathNavigator other = this.nav.Clone();
                        if (other.MoveToParent())
                        {
                            if (!other.MoveToFirstNamespace(XPathNamespaceScope.Local))
                            {
                                return false;
                            }
                            if (other.IsSamePosition(this.nav))
                            {
                                other.MoveToParent();
                                if (!other.MoveToFirstAttribute())
                                {
                                    return false;
                                }
                                this.nav.MoveTo(other);
                                return true;
                            }
                            XPathNavigator navigator2 = other.Clone();
                            while (other.MoveToNextNamespace(XPathNamespaceScope.Local))
                            {
                                if (other.IsSamePosition(this.nav))
                                {
                                    this.nav.MoveTo(navigator2);
                                    return true;
                                }
                                navigator2.MoveTo(other);
                            }
                        }
                        return false;
                    }
                    return this.nav.MoveToNextAttribute();

                case State.AttrVal:
                    this.depth--;
                    this.state = State.Attribute;
                    if (this.MoveToNextAttribute())
                    {
                        break;
                    }
                    this.depth++;
                    this.state = State.AttrVal;
                    return false;

                case State.InReadBinary:
                    this.state = this.savedState;
                    if (this.MoveToNextAttribute())
                    {
                        this.readBinaryHelper.Finish();
                        return true;
                    }
                    this.state = State.InReadBinary;
                    return false;

                default:
                    return false;
            }
            this.nodeType = XmlNodeType.Attribute;
            return true;
        }

        public override bool Read()
        {
            this.attrCount = -1;
            switch (this.state)
            {
                case State.Initial:
                    this.nav = this.navToRead;
                    this.state = State.Content;
                    if (this.nav.NodeType != XPathNodeType.Root)
                    {
                        if (XPathNodeType.Attribute == this.nav.NodeType)
                        {
                            this.state = State.Attribute;
                        }
                        break;
                    }
                    if (this.nav.MoveToFirstChild())
                    {
                        this.readEntireDocument = true;
                        break;
                    }
                    this.SetEOF();
                    return false;

                case State.Content:
                    goto Label_00AD;

                case State.EndElement:
                    goto Label_0114;

                case State.Attribute:
                case State.AttrVal:
                    if (this.nav.MoveToParent())
                    {
                        this.nodeType = ToXmlNodeType(this.nav.NodeType);
                        this.depth--;
                        if (this.state == State.AttrVal)
                        {
                            this.depth--;
                        }
                        goto Label_00AD;
                    }
                    this.SetEOF();
                    return false;

                case State.InReadBinary:
                    this.state = this.savedState;
                    this.readBinaryHelper.Finish();
                    return this.Read();

                case State.EOF:
                case State.Closed:
                case State.Error:
                    return false;

                default:
                    goto Label_020E;
            }
            this.nodeType = ToXmlNodeType(this.nav.NodeType);
            goto Label_020E;
        Label_00AD:
            if (this.nav.MoveToFirstChild())
            {
                this.nodeType = ToXmlNodeType(this.nav.NodeType);
                this.depth++;
                this.state = State.Content;
                goto Label_020E;
            }
            if ((this.nodeType == XmlNodeType.Element) && !this.nav.IsEmptyElement)
            {
                this.nodeType = XmlNodeType.EndElement;
                this.state = State.EndElement;
                goto Label_020E;
            }
        Label_0114:
            if ((this.depth == 0) && !this.readEntireDocument)
            {
                this.SetEOF();
                return false;
            }
            if (this.nav.MoveToNext())
            {
                this.nodeType = ToXmlNodeType(this.nav.NodeType);
                this.state = State.Content;
            }
            else if ((this.depth > 0) && this.nav.MoveToParent())
            {
                this.nodeType = XmlNodeType.EndElement;
                this.state = State.EndElement;
                this.depth--;
            }
            else
            {
                this.SetEOF();
                return false;
            }
        Label_020E:
            return true;
        }

        public override bool ReadAttributeValue()
        {
            if (this.state == State.InReadBinary)
            {
                this.readBinaryHelper.Finish();
                this.state = this.savedState;
            }
            if (this.state == State.Attribute)
            {
                this.state = State.AttrVal;
                this.nodeType = XmlNodeType.Text;
                this.depth++;
                return true;
            }
            return false;
        }

        public override int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.state != State.InReadBinary)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
                this.savedState = this.state;
            }
            this.state = this.savedState;
            int num = this.readBinaryHelper.ReadContentAsBase64(buffer, index, count);
            this.savedState = this.state;
            this.state = State.InReadBinary;
            return num;
        }

        public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.state != State.InReadBinary)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
                this.savedState = this.state;
            }
            this.state = this.savedState;
            int num = this.readBinaryHelper.ReadContentAsBinHex(buffer, index, count);
            this.savedState = this.state;
            this.state = State.InReadBinary;
            return num;
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.state != State.InReadBinary)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
                this.savedState = this.state;
            }
            this.state = this.savedState;
            int num = this.readBinaryHelper.ReadElementContentAsBase64(buffer, index, count);
            this.savedState = this.state;
            this.state = State.InReadBinary;
            return num;
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.state != State.InReadBinary)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
                this.savedState = this.state;
            }
            this.state = this.savedState;
            int num = this.readBinaryHelper.ReadElementContentAsBinHex(buffer, index, count);
            this.savedState = this.state;
            this.state = State.InReadBinary;
            return num;
        }

        public override void ResolveEntity()
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        private void SetEOF()
        {
            this.nav = XmlEmptyNavigator.Singleton;
            this.nodeType = XmlNodeType.None;
            this.state = State.EOF;
            this.depth = 0;
        }

        IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
        {
            return this.nav.GetNamespacesInScope(scope);
        }

        string IXmlNamespaceResolver.LookupNamespace(string prefix)
        {
            return this.nav.LookupNamespace(prefix);
        }

        string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
        {
            return this.nav.LookupPrefix(namespaceName);
        }

        internal static XmlNodeType ToXmlNodeType(XPathNodeType typ)
        {
            return convertFromXPathNodeType[(int) typ];
        }

        public override int AttributeCount
        {
            get
            {
                if (this.attrCount < 0)
                {
                    XPathNavigator elemNav = this.GetElemNav();
                    int num = 0;
                    if (elemNav != null)
                    {
                        if (elemNav.MoveToFirstNamespace(XPathNamespaceScope.Local))
                        {
                            do
                            {
                                num++;
                            }
                            while (elemNav.MoveToNextNamespace(XPathNamespaceScope.Local));
                            elemNav.MoveToParent();
                        }
                        if (elemNav.MoveToFirstAttribute())
                        {
                            do
                            {
                                num++;
                            }
                            while (elemNav.MoveToNextAttribute());
                        }
                    }
                    this.attrCount = num;
                }
                return this.attrCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                if (this.state == State.Initial)
                {
                    return this.navToRead.BaseURI;
                }
                return this.nav.BaseURI;
            }
        }

        public override bool CanReadBinaryContent
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
                return this.depth;
            }
        }

        public override bool EOF
        {
            get
            {
                return (this.state == State.EOF);
            }
        }

        public override bool HasValue
        {
            get
            {
                return (((this.nodeType != XmlNodeType.Element) && (this.nodeType != XmlNodeType.Document)) && ((this.nodeType != XmlNodeType.EndElement) && (this.nodeType != XmlNodeType.None)));
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this.nav.IsEmptyElement;
            }
        }

        protected bool IsReading
        {
            get
            {
                return ((this.state > State.Initial) && (this.state < State.EOF));
            }
        }

        public override string LocalName
        {
            get
            {
                if ((this.nav.NodeType == XPathNodeType.Namespace) && (this.nav.LocalName.Length == 0))
                {
                    return this.NameTable.Add("xmlns");
                }
                if (this.NodeType == XmlNodeType.Text)
                {
                    return string.Empty;
                }
                return this.nav.LocalName;
            }
        }

        internal override XmlNamespaceManager NamespaceManager
        {
            get
            {
                return XPathNavigator.GetNamespaces(this);
            }
        }

        public override string NamespaceURI
        {
            get
            {
                if (this.nav.NodeType == XPathNodeType.Namespace)
                {
                    return this.NameTable.Add("http://www.w3.org/2000/xmlns/");
                }
                if (this.NodeType == XmlNodeType.Text)
                {
                    return string.Empty;
                }
                return this.nav.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.navToRead.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this.nodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                if ((this.nav.NodeType == XPathNodeType.Namespace) && (this.nav.LocalName.Length != 0))
                {
                    return this.NameTable.Add("xmlns");
                }
                if (this.NodeType == XmlNodeType.Text)
                {
                    return string.Empty;
                }
                return this.nav.Prefix;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                switch (this.state)
                {
                    case State.Initial:
                        return System.Xml.ReadState.Initial;

                    case State.Content:
                    case State.EndElement:
                    case State.Attribute:
                    case State.AttrVal:
                    case State.InReadBinary:
                        return System.Xml.ReadState.Interactive;

                    case State.EOF:
                        return System.Xml.ReadState.EndOfFile;

                    case State.Closed:
                        return System.Xml.ReadState.Closed;
                }
                return System.Xml.ReadState.Error;
            }
        }

        public override IXmlSchemaInfo SchemaInfo
        {
            get
            {
                if (this.nodeType == XmlNodeType.Text)
                {
                    return null;
                }
                return this.nav.SchemaInfo;
            }
        }

        public override XmlReaderSettings Settings
        {
            get
            {
                return new XmlReaderSettings { NameTable = this.NameTable, ConformanceLevel = ConformanceLevel.Fragment, CheckCharacters = false, ReadOnly = true };
            }
        }

        internal object UnderlyingObject
        {
            get
            {
                return this.nav.UnderlyingObject;
            }
        }

        public override string Value
        {
            get
            {
                if (((this.nodeType != XmlNodeType.Element) && (this.nodeType != XmlNodeType.Document)) && ((this.nodeType != XmlNodeType.EndElement) && (this.nodeType != XmlNodeType.None)))
                {
                    return this.nav.Value;
                }
                return string.Empty;
            }
        }

        public override Type ValueType
        {
            get
            {
                return this.nav.ValueType;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.nav.XmlLang;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                XPathNavigator navigator = this.nav.Clone();
                do
                {
                    if (navigator.MoveToAttribute("space", "http://www.w3.org/XML/1998/namespace"))
                    {
                        switch (XmlConvert.TrimString(navigator.Value))
                        {
                            case "default":
                                return System.Xml.XmlSpace.Default;

                            case "preserve":
                                return System.Xml.XmlSpace.Preserve;
                        }
                        navigator.MoveToParent();
                    }
                }
                while (navigator.MoveToParent());
                return System.Xml.XmlSpace.None;
            }
        }

        private enum State
        {
            Initial,
            Content,
            EndElement,
            Attribute,
            AttrVal,
            InReadBinary,
            EOF,
            Closed,
            Error
        }
    }
}

