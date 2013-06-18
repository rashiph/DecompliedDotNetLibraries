namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;

    internal class SeekableMessageNavigator : SeekableXPathNavigator, INodeCounter
    {
        private bool atomize;
        private static Node[] BlankDom = new Node[6];
        private int bodyIndex;
        private const string BodyTag = "Body";
        private SeekableMessageNavigator counter;
        private SeekableMessageNavigator dom;
        private const int EnvelopeIndex = 2;
        private const string EnvelopeTag = "Envelope";
        private const int FirstHeaderIndex = 6;
        private const int GrowFactor = 2;
        private const int GrowInc = 0x3e8;
        private const int HeaderIndex = 5;
        private MessageHeaders headers;
        private const string HeaderTag = "Header";
        private bool includeBody;
        private int location;
        private System.ServiceModel.Channels.Message message;
        private System.Xml.NameTable nameTable;
        private int nextFreeIndex;
        private int nodeCount;
        private int nodeCountMax;
        private Node[] nodes;
        private Stack<string> nsStack;
        private const int NullIndex = 0;
        private const int RootIndex = 1;
        private const int SoapNSIndex = 3;
        private const string SoapP = "s";
        private XmlSpace space;
        private int specialParent;
        private const int StartSize = 50;
        private const int StretchMax = 0x3e8;
        private StringBuilder stringBuilder;
        private const int XmlNSIndex = 4;
        private const string XmlnsP = "xmlns";
        private const string XmlP = "xml";

        static SeekableMessageNavigator()
        {
            BlankDom[1].type = XPathNodeType.Root;
            BlankDom[1].firstChild = 2;
            BlankDom[1].prefix = string.Empty;
            BlankDom[1].name = string.Empty;
            BlankDom[1].val = string.Empty;
            BlankDom[2].type = XPathNodeType.Element;
            BlankDom[2].prefix = "s";
            BlankDom[2].name = "Envelope";
            BlankDom[2].parent = 1;
            BlankDom[2].firstChild = 5;
            BlankDom[2].firstNamespace = 3;
            BlankDom[3].type = XPathNodeType.Namespace;
            BlankDom[3].name = "s";
            BlankDom[3].nextSibling = 4;
            BlankDom[3].parent = 2;
            BlankDom[4].type = XPathNodeType.Namespace;
            BlankDom[4].name = "xml";
            BlankDom[4].val = "http://www.w3.org/XML/1998/namespace";
            BlankDom[4].prevSibling = 3;
            BlankDom[4].parent = 1;
            BlankDom[5].type = XPathNodeType.Element;
            BlankDom[5].prefix = "s";
            BlankDom[5].name = "Header";
            BlankDom[5].parent = 2;
            BlankDom[5].firstNamespace = 3;
        }

        internal SeekableMessageNavigator(SeekableMessageNavigator nav)
        {
            this.counter = nav.counter;
            this.dom = nav.dom;
            this.location = nav.location;
            this.specialParent = nav.specialParent;
            if (this.specialParent != 0)
            {
                this.nsStack = nav.CloneNSStack();
            }
        }

        internal SeekableMessageNavigator(System.ServiceModel.Channels.Message msg, int countMax, XmlSpace space, bool includeBody, bool atomize)
        {
            this.Init(msg, countMax, space, includeBody, atomize);
        }

        private void AddAttribute(int node, int attr)
        {
            this.nodes[attr].parent = node;
            this.nodes[attr].nextSibling = this.nodes[node].firstAttribute;
            this.nodes[node].firstAttribute = attr;
        }

        private void AddChild(int parent, int child)
        {
            if (this.nodes[parent].firstChild == 0)
            {
                this.nodes[parent].firstChild = child;
                this.nodes[child].parent = parent;
            }
            else
            {
                this.AddSibling(this.nodes[parent].firstChild, child);
            }
        }

        private void AddNamespace(int node, int ns)
        {
            this.nodes[ns].parent = node;
            this.nodes[ns].nextSibling = this.nodes[node].firstNamespace;
            this.nodes[node].firstNamespace = ns;
        }

        private void AddSibling(int node1, int node2)
        {
            int index = this.LastSibling(node1);
            this.nodes[index].nextSibling = node2;
            this.nodes[node2].prevSibling = index;
            this.nodes[node2].parent = this.nodes[index].parent;
        }

        internal void Atomize()
        {
            if (!this.dom.atomize)
            {
                this.dom.atomize = true;
                this.dom.nameTable = new System.Xml.NameTable();
                this.dom.nameTable.Add(string.Empty);
                this.dom.Atomize(1, this.nextFreeIndex);
            }
        }

        private void Atomize(int first, int bound)
        {
            while (first < bound)
            {
                string prefix = this.nodes[first].prefix;
                if (prefix != null)
                {
                    this.nodes[first].prefix = this.nameTable.Add(prefix);
                }
                prefix = this.nodes[first].name;
                if (prefix != null)
                {
                    this.nodes[first].name = this.nameTable.Add(prefix);
                }
                prefix = this.nodes[first].ns;
                if (prefix != null)
                {
                    this.nodes[first].ns = this.nameTable.Add(prefix);
                }
                first++;
            }
        }

        public override XPathNavigator Clone()
        {
            return new SeekableMessageNavigator(this);
        }

        private Stack<string> CloneNSStack()
        {
            Stack<string> stack = new Stack<string>();
            foreach (string str in this.nsStack)
            {
                stack.Push(str);
            }
            return stack;
        }

        private XmlNodeOrder CompareLocation(int loc1, int loc2)
        {
            if (loc1 == loc2)
            {
                return XmlNodeOrder.Same;
            }
            if (loc1 < loc2)
            {
                return XmlNodeOrder.Before;
            }
            return XmlNodeOrder.After;
        }

        internal XmlNodeOrder ComparePosition(SeekableMessageNavigator nav)
        {
            if (nav == null)
            {
                return XmlNodeOrder.Unknown;
            }
            if (this.dom != nav.dom)
            {
                return XmlNodeOrder.Unknown;
            }
            return this.dom.ComparePosition(this.specialParent, this.location, nav.specialParent, nav.location);
        }

        public override XmlNodeOrder ComparePosition(XPathNavigator nav)
        {
            if (nav != null)
            {
                SeekableMessageNavigator navigator = nav as SeekableMessageNavigator;
                if (navigator != null)
                {
                    return this.ComparePosition(navigator);
                }
            }
            return XmlNodeOrder.Unknown;
        }

        public override XmlNodeOrder ComparePosition(long pos1, long pos2)
        {
            Position position = this.dom.DecodePosition(pos1);
            Position position2 = this.dom.DecodePosition(pos2);
            return this.dom.ComparePosition(position.parent, position.elem, position2.parent, position2.elem);
        }

        private XmlNodeOrder ComparePosition(int p1, int loc1, int p2, int loc2)
        {
            int parent;
            int num2;
            if ((p1 == p2) && (p1 != 0))
            {
                return this.CompareLocation(loc1, loc2);
            }
            if (p1 == 0)
            {
                if (this.nodes[loc1].type == XPathNodeType.Attribute)
                {
                    parent = this.nodes[loc1].parent;
                }
                else
                {
                    parent = loc1;
                }
            }
            else
            {
                parent = p1;
            }
            if (p2 == 0)
            {
                if (this.nodes[loc2].type == XPathNodeType.Attribute)
                {
                    num2 = this.nodes[loc2].parent;
                }
                else
                {
                    num2 = loc2;
                }
            }
            else
            {
                num2 = p2;
            }
            if (parent == num2)
            {
                XPathNodeType type = this.nodes[loc1].type;
                XPathNodeType type2 = this.nodes[loc2].type;
                if (type == XPathNodeType.Namespace)
                {
                    if (type2 == XPathNodeType.Attribute)
                    {
                        return XmlNodeOrder.Before;
                    }
                    return XmlNodeOrder.After;
                }
                if (type2 == XPathNodeType.Namespace)
                {
                    if (type == XPathNodeType.Attribute)
                    {
                        return XmlNodeOrder.After;
                    }
                    return XmlNodeOrder.Before;
                }
            }
            int index = parent;
            while (index > this.bodyIndex)
            {
                index = this.nodes[index].parent;
            }
            int num4 = num2;
            while (num4 > this.bodyIndex)
            {
                num4 = this.nodes[num4].parent;
            }
            if (index == num4)
            {
                return this.CompareLocation(loc1, loc2);
            }
            return this.CompareLocation(index, num4);
        }

        private Position DecodePosition(long pos)
        {
            Position position = new Position((int) pos, (int) (pos >> 0x20));
            if ((position.elem > 0) && (position.elem < this.nextFreeIndex))
            {
                if (position.parent == 0)
                {
                    return position;
                }
                if (((position.parent > 0) && (position.parent < this.nextFreeIndex)) && ((this.nodes[position.parent].type == XPathNodeType.Element) && (this.nodes[position.elem].type == XPathNodeType.Namespace)))
                {
                    int parent = this.nodes[position.elem].parent;
                    int index = position.parent;
                    do
                    {
                        if (index == parent)
                        {
                            return position;
                        }
                        index = this.nodes[index].parent;
                    }
                    while (index != 0);
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.InvalidNavigatorPosition, System.ServiceModel.SR.GetString("SeekableMessageNavInvalidPosition")));
        }

        public override object Evaluate(string xpath)
        {
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, System.ServiceModel.SR.GetString("SeekableMessageNavNonAtomized", new object[] { "Evaluate" })));
            }
            return base.Evaluate(xpath);
        }

        public override object Evaluate(XPathExpression expr)
        {
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, System.ServiceModel.SR.GetString("SeekableMessageNavNonAtomized", new object[] { "Evaluate" })));
            }
            return base.Evaluate(expr);
        }

        public override object Evaluate(XPathExpression expr, XPathNodeIterator context)
        {
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, System.ServiceModel.SR.GetString("SeekableMessageNavNonAtomized", new object[] { "Evaluate" })));
            }
            return base.Evaluate(expr, context);
        }

        private int FindNamespace(int parent, int ns, XPathNamespaceScope scope)
        {
            bool flag = false;
            int num = 0;
            while ((ns != 0) && !flag)
            {
                this.Increase();
                string name = this.dom.nodes[ns].name;
                if (this.nsStack.Contains(name))
                {
                    ns = this.dom.nodes[ns].nextSibling;
                }
                else
                {
                    this.nsStack.Push(name);
                    num++;
                    string val = this.dom.nodes[ns].val;
                    if ((name.Length != 0) || (val.Length != 0))
                    {
                        switch (scope)
                        {
                            case XPathNamespaceScope.All:
                                flag = true;
                                break;

                            case XPathNamespaceScope.ExcludeXml:
                                if (string.CompareOrdinal(name, "xml") != 0)
                                {
                                    goto Label_00D1;
                                }
                                this.Increase();
                                ns = this.dom.nodes[ns].nextSibling;
                                break;

                            case XPathNamespaceScope.Local:
                                if (this.dom.nodes[ns].parent == parent)
                                {
                                    goto Label_00F3;
                                }
                                ns = 0;
                                break;
                        }
                    }
                }
                continue;
            Label_00D1:
                flag = true;
                continue;
            Label_00F3:
                flag = true;
            }
            if (ns == 0)
            {
                for (int i = 0; i < num; i++)
                {
                    this.nsStack.Pop();
                }
            }
            return ns;
        }

        internal void ForkNodeCount(int count)
        {
            this.nodeCount = count;
            this.nodeCountMax = count;
            this.counter = this;
        }

        public override string GetAttribute(string name, string ns)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");
            }
            if (this.NodeType != XPathNodeType.Element)
            {
                return string.Empty;
            }
            string str = string.Empty;
            this.Increase();
            this.LoadOnDemand();
            for (int i = this.dom.nodes[this.location].firstAttribute; i != 0; i = this.dom.nodes[i].nextSibling)
            {
                if ((string.CompareOrdinal(this.dom.nodes[i].name, name) == 0) && (string.CompareOrdinal(this.dom.nodes[i].ns, ns) == 0))
                {
                    return this.dom.nodes[i].val;
                }
                this.Increase();
            }
            return str;
        }

        public override string GetLocalName(long pos)
        {
            string name = this.dom.nodes[this.dom.DecodePosition(pos).elem].name;
            if (name != null)
            {
                return name;
            }
            return string.Empty;
        }

        private string GetName(int elem)
        {
            this.LoadOnDemand(elem);
            string prefix = this.dom.nodes[elem].prefix;
            string name = this.dom.nodes[elem].name;
            if ((prefix != null) && (prefix.Length > 0))
            {
                return (prefix + ":" + name);
            }
            return name;
        }

        public override string GetName(long pos)
        {
            return this.GetName(this.dom.DecodePosition(pos).elem);
        }

        public override string GetNamespace(long pos)
        {
            string ns = this.dom.nodes[this.dom.DecodePosition(pos).elem].ns;
            if (ns != null)
            {
                return ns;
            }
            return string.Empty;
        }

        public override string GetNamespace(string name)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            if (this.NodeType != XPathNodeType.Element)
            {
                return string.Empty;
            }
            this.Increase();
            this.LoadOnDemand();
            int firstNamespace = this.dom.nodes[this.location].firstNamespace;
            string str = string.Empty;
            while (firstNamespace != 0)
            {
                this.Increase();
                if (string.CompareOrdinal(this.dom.nodes[firstNamespace].name, name) == 0)
                {
                    return this.dom.nodes[firstNamespace].val;
                }
                firstNamespace = this.dom.nodes[firstNamespace].nextSibling;
            }
            return str;
        }

        public override XPathNodeType GetNodeType(long pos)
        {
            return this.dom.nodes[this.dom.DecodePosition(pos).elem].type;
        }

        private string GetValue(int elem)
        {
            string val = this.nodes[elem].val;
            if (val != null)
            {
                return val;
            }
            if (this.stringBuilder == null)
            {
                this.stringBuilder = new StringBuilder();
            }
            else
            {
                this.stringBuilder.Length = 0;
            }
            this.GetValueDriver(elem);
            string str2 = this.stringBuilder.ToString();
            this.nodes[elem].val = str2;
            return str2;
        }

        public override string GetValue(long pos)
        {
            string str = this.dom.GetValue(this.dom.DecodePosition(pos).elem);
            if (str != null)
            {
                return str;
            }
            return string.Empty;
        }

        private void GetValueDriver(int elem)
        {
            this.dom.LoadOnDemand(elem);
            switch (this.nodes[elem].type)
            {
                case XPathNodeType.Root:
                case XPathNodeType.Element:
                {
                    string val = this.nodes[elem].val;
                    if (val != null)
                    {
                        this.stringBuilder.Append(val);
                        return;
                    }
                    for (int i = this.nodes[elem].firstChild; i != 0; i = this.nodes[i].nextSibling)
                    {
                        this.Increase();
                        this.GetValueDriver(i);
                    }
                    return;
                }
            }
            this.stringBuilder.Append(this.nodes[elem].val);
        }

        private void Increase()
        {
            if (this.counter.nodeCount <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XPathNavigatorException(System.ServiceModel.SR.GetString("FilterNodeQuotaExceeded", new object[] { this.counter.nodeCountMax })));
            }
            this.counter.nodeCount--;
        }

        internal void Init(System.ServiceModel.Channels.Message msg, int countMax, XmlSpace space, bool includeBody, bool atomize)
        {
            this.counter = this;
            this.nodeCount = countMax;
            this.nodeCountMax = countMax;
            this.dom = this;
            this.location = 1;
            this.specialParent = 0;
            this.includeBody = includeBody;
            this.message = msg;
            this.headers = msg.Headers;
            this.space = space;
            this.atomize = false;
            int num = (msg.Headers.Count + 6) + 1;
            if ((this.nodes == null) || (this.nodes.Length < num))
            {
                this.nodes = new Node[num + 50];
            }
            else
            {
                Array.Clear(this.nodes, 1, this.nextFreeIndex - 1);
            }
            this.bodyIndex = num - 1;
            this.nextFreeIndex = num;
            Array.Copy(BlankDom, this.nodes, 6);
            string str = msg.Version.Envelope.Namespace;
            this.nodes[2].ns = str;
            this.nodes[3].val = str;
            this.nodes[5].ns = str;
            this.nodes[5].nextSibling = this.bodyIndex;
            this.nodes[5].firstChild = (this.bodyIndex != 6) ? 6 : 0;
            if (msg.Headers.Count > 0)
            {
                int index = 6;
                for (int i = 0; i < msg.Headers.Count; i++)
                {
                    this.nodes[index].type = XPathNodeType.Element;
                    this.nodes[index].parent = 5;
                    this.nodes[index].nextSibling = index + 1;
                    this.nodes[index].prevSibling = index - 1;
                    MessageHeaderInfo info = msg.Headers[i];
                    this.nodes[index].ns = info.Namespace;
                    this.nodes[index].name = info.Name;
                    this.nodes[index].firstChild = -1;
                    index++;
                }
                this.nodes[6].prevSibling = 0;
                this.nodes[this.bodyIndex - 1].nextSibling = 0;
            }
            this.nodes[this.bodyIndex].type = XPathNodeType.Element;
            this.nodes[this.bodyIndex].prefix = "s";
            this.nodes[this.bodyIndex].ns = str;
            this.nodes[this.bodyIndex].name = "Body";
            this.nodes[this.bodyIndex].parent = 2;
            this.nodes[this.bodyIndex].prevSibling = 5;
            this.nodes[this.bodyIndex].firstNamespace = 3;
            this.nodes[this.bodyIndex].firstChild = -1;
            if (atomize)
            {
                this.Atomize();
            }
        }

        internal bool IsDescendant(SeekableMessageNavigator nav)
        {
            if (nav != null)
            {
                if (this.dom != nav.dom)
                {
                    return false;
                }
                switch (this.dom.nodes[nav.location].type)
                {
                    case XPathNodeType.Namespace:
                    case XPathNodeType.Attribute:
                        return false;
                }
                switch (this.dom.nodes[this.location].type)
                {
                    case XPathNodeType.Namespace:
                    case XPathNodeType.Attribute:
                        return false;
                }
                int location = nav.location;
                while (location != 0)
                {
                    this.Increase();
                    location = this.dom.nodes[location].parent;
                    if (location == this.location)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool IsDescendant(XPathNavigator nav)
        {
            if (nav == null)
            {
                return false;
            }
            SeekableMessageNavigator navigator = nav as SeekableMessageNavigator;
            return ((navigator != null) && this.IsDescendant(navigator));
        }

        internal bool IsSamePosition(SeekableMessageNavigator nav)
        {
            if (nav == null)
            {
                return false;
            }
            return (((this.dom == nav.dom) && (this.location == nav.location)) && (this.specialParent == nav.specialParent));
        }

        public override bool IsSamePosition(XPathNavigator nav)
        {
            if (nav == null)
            {
                return false;
            }
            SeekableMessageNavigator navigator = nav as SeekableMessageNavigator;
            return ((navigator != null) && this.IsSamePosition(navigator));
        }

        private int LastChild(int n)
        {
            n = this.nodes[n].firstChild;
            if (n == 0)
            {
                return 0;
            }
            return this.LastSibling(n);
        }

        private int LastSibling(int n)
        {
            while (this.nodes[n].nextSibling != 0)
            {
                n = this.nodes[n].nextSibling;
            }
            return n;
        }

        private void LoadBody()
        {
            if (!this.message.IsEmpty)
            {
                XmlReader readerAtBodyContents = this.message.GetReaderAtBodyContents();
                if (readerAtBodyContents.ReadState == System.Xml.ReadState.Initial)
                {
                    readerAtBodyContents.Read();
                }
                int nextFreeIndex = this.nextFreeIndex;
                this.ReadChildNodes(readerAtBodyContents, this.bodyIndex, 3);
                int bound = this.nextFreeIndex;
                if (this.atomize)
                {
                    this.Atomize(nextFreeIndex, bound);
                }
            }
        }

        private void LoadHeader(int self)
        {
            XmlReader readerAtHeader = this.headers.GetReaderAtHeader(self - 6);
            if (readerAtHeader.ReadState == System.Xml.ReadState.Initial)
            {
                readerAtHeader.Read();
            }
            int nextFreeIndex = this.nextFreeIndex;
            this.nodes[self].firstNamespace = 3;
            this.nodes[self].prefix = this.atomize ? this.nameTable.Add(readerAtHeader.Prefix) : readerAtHeader.Prefix;
            this.nodes[self].baseUri = readerAtHeader.BaseURI;
            this.nodes[self].xmlLang = readerAtHeader.XmlLang;
            if (!readerAtHeader.IsEmptyElement)
            {
                this.ReadAttributes(self, readerAtHeader);
                readerAtHeader.Read();
                this.ReadChildNodes(readerAtHeader, self, this.nodes[self].firstNamespace);
            }
            else
            {
                this.ReadAttributes(self, readerAtHeader);
            }
            int bound = this.nextFreeIndex;
            if (this.atomize)
            {
                this.Atomize(nextFreeIndex, bound);
            }
        }

        private void LoadOnDemand()
        {
            this.dom.LoadOnDemand(this.location);
        }

        private void LoadOnDemand(int elem)
        {
            if ((elem <= this.bodyIndex) && (elem >= 6))
            {
                if (this.nodes[elem].firstChild == -1)
                {
                    this.nodes[elem].firstChild = 0;
                    if (elem == this.bodyIndex)
                    {
                        if (!this.includeBody)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NavigatorInvalidBodyAccessException(System.ServiceModel.SR.GetString("SeekableMessageNavBodyForbidden")));
                        }
                        this.LoadBody();
                    }
                    else
                    {
                        this.LoadHeader(elem);
                    }
                }
                else if ((elem == this.bodyIndex) && !this.includeBody)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NavigatorInvalidBodyAccessException(System.ServiceModel.SR.GetString("SeekableMessageNavBodyForbidden")));
                }
            }
        }

        public override bool Matches(string xpath)
        {
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, System.ServiceModel.SR.GetString("SeekableMessageNavNonAtomized", new object[] { "Matches" })));
            }
            return base.Matches(xpath);
        }

        public override bool Matches(XPathExpression expr)
        {
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, System.ServiceModel.SR.GetString("SeekableMessageNavNonAtomized", new object[] { "Matches" })));
            }
            return base.Matches(expr);
        }

        internal bool MoveTo(SeekableMessageNavigator nav)
        {
            if (nav == null)
            {
                return false;
            }
            this.dom = nav.dom;
            this.counter = nav.counter;
            this.location = nav.location;
            this.specialParent = nav.specialParent;
            if (this.specialParent != 0)
            {
                this.nsStack = nav.CloneNSStack();
            }
            return true;
        }

        public override bool MoveTo(XPathNavigator nav)
        {
            if (nav == null)
            {
                return false;
            }
            SeekableMessageNavigator navigator = nav as SeekableMessageNavigator;
            return ((navigator != null) && this.MoveTo(navigator));
        }

        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            if (namespaceURI == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceURI");
            }
            this.LoadOnDemand();
            if (this.dom.nodes[this.location].type == XPathNodeType.Element)
            {
                this.Increase();
                for (int i = this.dom.nodes[this.location].firstAttribute; i != 0; i = this.dom.nodes[i].nextSibling)
                {
                    if ((string.CompareOrdinal(this.dom.nodes[i].name, localName) == 0) && (string.CompareOrdinal(this.dom.nodes[i].ns, namespaceURI) == 0))
                    {
                        this.location = i;
                        return true;
                    }
                    this.Increase();
                }
            }
            return false;
        }

        public override bool MoveToFirst()
        {
            switch (this.dom.nodes[this.location].type)
            {
                case XPathNodeType.Attribute:
                case XPathNodeType.Namespace:
                    return false;
            }
            this.Increase();
            int parent = this.dom.nodes[this.location].parent;
            if (parent != 0)
            {
                this.Increase();
                this.location = this.dom.nodes[parent].firstChild;
            }
            return true;
        }

        public override bool MoveToFirstAttribute()
        {
            if (this.dom.nodes[this.location].type == XPathNodeType.Element)
            {
                this.LoadOnDemand();
                int firstAttribute = this.dom.nodes[this.location].firstAttribute;
                if (firstAttribute != 0)
                {
                    this.Increase();
                    this.location = firstAttribute;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToFirstChild()
        {
            if ((this.location == 1) || (this.dom.nodes[this.location].type == XPathNodeType.Element))
            {
                this.LoadOnDemand();
                int firstChild = this.dom.nodes[this.location].firstChild;
                if (firstChild != 0)
                {
                    this.Increase();
                    this.location = firstChild;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope scope)
        {
            if (this.dom.nodes[this.location].type == XPathNodeType.Element)
            {
                if (this.nsStack == null)
                {
                    this.nsStack = new Stack<string>();
                }
                else
                {
                    this.nsStack.Clear();
                }
                this.LoadOnDemand();
                int num = this.FindNamespace(this.location, this.dom.nodes[this.location].firstNamespace, scope);
                if (num != 0)
                {
                    this.specialParent = this.location;
                    this.Increase();
                    this.location = num;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToId(string id)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.NotSupported, System.ServiceModel.SR.GetString("SeekableMessageNavIDNotSupported")));
        }

        public override bool MoveToNamespace(string name)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            if (this.dom.nodes[this.location].type == XPathNodeType.Element)
            {
                if (this.nsStack == null)
                {
                    this.nsStack = new Stack<string>();
                }
                else
                {
                    this.nsStack.Clear();
                }
                this.Increase();
                this.LoadOnDemand();
                int firstNamespace = this.dom.nodes[this.location].firstNamespace;
                int num2 = 0;
                while (firstNamespace != 0)
                {
                    string item = this.dom.nodes[firstNamespace].name;
                    if (!this.nsStack.Contains(item))
                    {
                        this.nsStack.Push(item);
                        num2++;
                        string val = this.dom.nodes[firstNamespace].val;
                        if (((item.Length > 0) || (val.Length > 0)) && (string.CompareOrdinal(item, name) == 0))
                        {
                            this.specialParent = this.location;
                            this.location = firstNamespace;
                            return true;
                        }
                    }
                    this.Increase();
                    firstNamespace = this.dom.nodes[firstNamespace].nextSibling;
                }
                for (int i = 0; i < num2; i++)
                {
                    this.nsStack.Pop();
                }
            }
            return false;
        }

        public override bool MoveToNext()
        {
            switch (this.dom.nodes[this.location].type)
            {
                case XPathNodeType.Attribute:
                case XPathNodeType.Namespace:
                    return false;
            }
            int nextSibling = this.dom.nodes[this.location].nextSibling;
            if (nextSibling != 0)
            {
                this.Increase();
                this.location = nextSibling;
                return true;
            }
            return false;
        }

        public override bool MoveToNextAttribute()
        {
            if (this.dom.nodes[this.location].type == XPathNodeType.Attribute)
            {
                int nextSibling = this.dom.nodes[this.location].nextSibling;
                if (nextSibling != 0)
                {
                    this.Increase();
                    this.location = nextSibling;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope scope)
        {
            if (this.dom.nodes[this.location].type == XPathNodeType.Namespace)
            {
                int num = this.FindNamespace(this.specialParent, this.dom.nodes[this.location].nextSibling, scope);
                if (num != 0)
                {
                    this.Increase();
                    this.location = num;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToParent()
        {
            if (this.location == 1)
            {
                return false;
            }
            this.Increase();
            if (this.specialParent != 0)
            {
                this.Increase();
                this.location = this.specialParent;
                this.specialParent = 0;
            }
            else
            {
                this.location = this.dom.nodes[this.location].parent;
            }
            return true;
        }

        public override bool MoveToPrevious()
        {
            int prevSibling = 0;
            XPathNodeType type = this.dom.nodes[this.location].type;
            if ((type != XPathNodeType.Attribute) && (type != XPathNodeType.Namespace))
            {
                prevSibling = this.dom.nodes[this.location].prevSibling;
            }
            if (prevSibling != 0)
            {
                this.Increase();
                this.location = prevSibling;
                return true;
            }
            return false;
        }

        public override void MoveToRoot()
        {
            this.Increase();
            this.location = 1;
            this.specialParent = 0;
        }

        private int NewNode()
        {
            if (this.nextFreeIndex == this.nodes.Length)
            {
                int num;
                if (this.nodes.Length <= 0x3e8)
                {
                    num = this.nodes.Length * 2;
                }
                else
                {
                    num = this.nodes.Length + 0x3e8;
                }
                Node[] array = new Node[num];
                this.nodes.CopyTo(array, 0);
                this.nodes = array;
            }
            return this.nextFreeIndex++;
        }

        private void ReadAttributes(int elem, XmlReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                if (QueryDataModel.IsAttribute(reader.NamespaceURI))
                {
                    int index = this.NewNode();
                    this.nodes[index].type = XPathNodeType.Attribute;
                    this.nodes[index].prefix = reader.Prefix;
                    this.nodes[index].name = reader.LocalName;
                    this.nodes[index].ns = reader.NamespaceURI;
                    this.nodes[index].val = reader.Value;
                    this.nodes[index].baseUri = reader.BaseURI;
                    this.nodes[index].xmlLang = reader.XmlLang;
                    this.AddAttribute(elem, index);
                }
                else
                {
                    string strA = (reader.Prefix.Length == 0) ? string.Empty : reader.LocalName;
                    if ((string.CompareOrdinal(strA, "xml") == 0) || (string.CompareOrdinal(strA, "xmlns") == 0))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.InvalidNamespacePrefix, System.ServiceModel.SR.GetString("SeekableMessageNavOverrideForbidden", new object[] { reader.Name })));
                    }
                    int num2 = this.NewNode();
                    this.nodes[num2].type = XPathNodeType.Namespace;
                    this.nodes[num2].name = strA;
                    this.nodes[num2].val = reader.Value;
                    this.nodes[num2].baseUri = reader.BaseURI;
                    this.nodes[num2].xmlLang = reader.XmlLang;
                    this.AddNamespace(elem, num2);
                }
            }
        }

        private int ReadChildNodes(XmlReader reader, int parent, int parentNS)
        {
            int index = 0;
        Label_0002:
            switch (reader.NodeType)
            {
                case XmlNodeType.None:
                case XmlNodeType.EndElement:
                case XmlNodeType.EndEntity:
                    return index;

                case XmlNodeType.Element:
                    index = this.NewNode();
                    this.nodes[index].type = XPathNodeType.Element;
                    this.nodes[index].prefix = reader.Prefix;
                    this.nodes[index].name = reader.LocalName;
                    this.nodes[index].ns = reader.NamespaceURI;
                    this.nodes[index].firstNamespace = parentNS;
                    this.nodes[index].baseUri = reader.BaseURI;
                    this.nodes[index].xmlLang = reader.XmlLang;
                    if (reader.IsEmptyElement)
                    {
                        this.ReadAttributes(index, reader);
                        this.nodes[index].empty = true;
                        break;
                    }
                    this.ReadAttributes(index, reader);
                    reader.Read();
                    this.ReadChildNodes(reader, index, this.nodes[index].firstNamespace);
                    break;

                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    index = this.LastChild(parent);
                    if ((index == 0) || (((this.nodes[index].type != XPathNodeType.Text) && (this.nodes[index].type != XPathNodeType.Whitespace)) && (this.nodes[index].type != XPathNodeType.SignificantWhitespace)))
                    {
                        index = this.NewNode();
                        this.nodes[index].baseUri = reader.BaseURI;
                        this.nodes[index].xmlLang = reader.XmlLang;
                        this.AddChild(parent, index);
                    }
                    this.nodes[index].type = XPathNodeType.Text;
                    this.nodes[index].val = reader.Value;
                    goto Label_04EB;

                case XmlNodeType.EntityReference:
                    reader.ResolveEntity();
                    reader.Read();
                    this.ReadChildNodes(reader, parent, parentNS);
                    goto Label_04EB;

                case XmlNodeType.ProcessingInstruction:
                    index = this.NewNode();
                    this.nodes[index].type = XPathNodeType.ProcessingInstruction;
                    this.nodes[index].name = reader.LocalName;
                    this.nodes[index].val = reader.Value;
                    this.nodes[index].baseUri = reader.BaseURI;
                    this.nodes[index].xmlLang = reader.XmlLang;
                    this.AddChild(parent, index);
                    goto Label_04EB;

                case XmlNodeType.Comment:
                    index = this.NewNode();
                    this.nodes[index].type = XPathNodeType.Comment;
                    this.nodes[index].val = reader.Value;
                    this.nodes[index].baseUri = reader.BaseURI;
                    this.nodes[index].xmlLang = reader.XmlLang;
                    this.AddChild(parent, index);
                    goto Label_04EB;

                case XmlNodeType.Whitespace:
                    goto Label_0331;

                case XmlNodeType.SignificantWhitespace:
                    if (reader.XmlSpace != XmlSpace.Preserve)
                    {
                        goto Label_0331;
                    }
                    index = this.LastChild(parent);
                    if ((index == 0) || (((this.nodes[index].type != XPathNodeType.Text) && (this.nodes[index].type != XPathNodeType.Whitespace)) && (this.nodes[index].type != XPathNodeType.SignificantWhitespace)))
                    {
                        index = this.NewNode();
                        this.nodes[index].type = XPathNodeType.SignificantWhitespace;
                        this.nodes[index].val = reader.Value;
                        this.nodes[index].baseUri = reader.BaseURI;
                        this.nodes[index].xmlLang = reader.XmlLang;
                        this.AddChild(parent, index);
                    }
                    else
                    {
                        this.nodes[index].val = this.nodes[index].val + reader.Value;
                    }
                    goto Label_04EB;

                default:
                    goto Label_04EB;
            }
            this.AddChild(parent, index);
            goto Label_04EB;
        Label_0331:
            if (this.space == XmlSpace.Preserve)
            {
                index = this.LastChild(parent);
                if ((index != 0) && (((this.nodes[index].type == XPathNodeType.Text) || (this.nodes[index].type == XPathNodeType.Whitespace)) || (this.nodes[index].type == XPathNodeType.SignificantWhitespace)))
                {
                    this.nodes[index].val = this.nodes[index].val + reader.Value;
                }
                else
                {
                    index = this.NewNode();
                    this.nodes[index].type = XPathNodeType.Whitespace;
                    this.nodes[index].val = reader.Value;
                    this.nodes[index].baseUri = reader.BaseURI;
                    this.nodes[index].xmlLang = reader.XmlLang;
                    this.AddChild(parent, index);
                }
            }
        Label_04EB:
            if (reader.Read())
            {
                goto Label_0002;
            }
            return index;
        }

        public override XPathNodeIterator Select(string xpath)
        {
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, System.ServiceModel.SR.GetString("SeekableMessageNavNonAtomized", new object[] { "Select" })));
            }
            return base.Select(xpath);
        }

        public override XPathNodeIterator Select(XPathExpression xpath)
        {
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, System.ServiceModel.SR.GetString("SeekableMessageNavNonAtomized", new object[] { "Select" })));
            }
            return base.Select(xpath);
        }

        public override XPathNodeIterator SelectAncestors(XPathNodeType type, bool matchSelf)
        {
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, System.ServiceModel.SR.GetString("SeekableMessageNavNonAtomized", new object[] { "SelectAncestors" })));
            }
            return base.SelectAncestors(type, matchSelf);
        }

        public override XPathNodeIterator SelectAncestors(string name, string namespaceURI, bool matchSelf)
        {
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, System.ServiceModel.SR.GetString("SeekableMessageNavNonAtomized", new object[] { "SelectAncestors" })));
            }
            return base.SelectAncestors(name, namespaceURI, matchSelf);
        }

        public override XPathNodeIterator SelectChildren(XPathNodeType type)
        {
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, System.ServiceModel.SR.GetString("SeekableMessageNavNonAtomized", new object[] { "SelectChildren" })));
            }
            return base.SelectChildren(type);
        }

        public override XPathNodeIterator SelectChildren(string name, string namespaceURI)
        {
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, System.ServiceModel.SR.GetString("SeekableMessageNavNonAtomized", new object[] { "SelectChildren" })));
            }
            return base.SelectChildren(name, namespaceURI);
        }

        public override XPathNodeIterator SelectDescendants(XPathNodeType type, bool matchSelf)
        {
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, System.ServiceModel.SR.GetString("SeekableMessageNavNonAtomized", new object[] { "SelectDescendants" })));
            }
            return base.SelectDescendants(type, matchSelf);
        }

        public override XPathNodeIterator SelectDescendants(string name, string namespaceURI, bool matchSelf)
        {
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, System.ServiceModel.SR.GetString("SeekableMessageNavNonAtomized", new object[] { "SelectDescendants" })));
            }
            return base.SelectDescendants(name, namespaceURI, matchSelf);
        }

        int INodeCounter.ElapsedCount(int marker)
        {
            return (marker - this.counter.nodeCount);
        }

        void INodeCounter.Increase()
        {
            this.Increase();
        }

        void INodeCounter.IncreaseBy(int count)
        {
            this.counter.nodeCount -= count - 1;
            this.Increase();
        }

        public override string BaseURI
        {
            get
            {
                this.LoadOnDemand();
                string baseUri = this.dom.nodes[this.location].baseUri;
                if (baseUri != null)
                {
                    return baseUri;
                }
                return string.Empty;
            }
        }

        public override long CurrentPosition
        {
            get
            {
                long specialParent = this.specialParent;
                specialParent = specialParent << 0x20;
                return (specialParent + this.location);
            }
            set
            {
                Position position = this.dom.DecodePosition(value);
                if (position.parent != 0)
                {
                    if (this.nsStack == null)
                    {
                        this.nsStack = new Stack<string>();
                    }
                    else
                    {
                        this.nsStack.Clear();
                    }
                    for (int i = this.dom.nodes[position.parent].firstNamespace; i != position.elem; i = this.dom.nodes[i].nextSibling)
                    {
                        if (i == 0)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.InvalidNavigatorPosition, System.ServiceModel.SR.GetString("SeekableMessageNavInvalidPosition")));
                        }
                        this.nsStack.Push(this.dom.nodes[i].name);
                    }
                }
                this.location = position.elem;
                this.specialParent = position.parent;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                this.LoadOnDemand();
                return (this.dom.nodes[this.location].firstAttribute != 0);
            }
        }

        public override bool HasChildren
        {
            get
            {
                this.LoadOnDemand();
                return (this.dom.nodes[this.location].firstChild != 0);
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this.dom.nodes[this.location].empty;
            }
        }

        public override string LocalName
        {
            get
            {
                string name = this.dom.nodes[this.location].name;
                if (name != null)
                {
                    return name;
                }
                return string.Empty;
            }
        }

        internal System.ServiceModel.Channels.Message Message
        {
            get
            {
                return this.dom.message;
            }
        }

        public override string Name
        {
            get
            {
                return this.GetName(this.location);
            }
        }

        public override string NamespaceURI
        {
            get
            {
                string ns = this.dom.nodes[this.location].ns;
                if (ns != null)
                {
                    return ns;
                }
                return string.Empty;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                if (!this.dom.atomize)
                {
                    this.dom.Atomize();
                }
                return this.dom.nameTable;
            }
        }

        public override XPathNodeType NodeType
        {
            get
            {
                return this.dom.nodes[this.location].type;
            }
        }

        public override string Prefix
        {
            get
            {
                this.LoadOnDemand();
                string prefix = this.dom.nodes[this.location].prefix;
                if (prefix != null)
                {
                    return prefix;
                }
                return string.Empty;
            }
        }

        int INodeCounter.CounterMarker
        {
            get
            {
                return this.counter.nodeCount;
            }
            set
            {
                this.counter.nodeCount = value;
            }
        }

        int INodeCounter.MaxCounter
        {
            set
            {
                this.counter.nodeCountMax = value;
            }
        }

        public override string Value
        {
            get
            {
                return this.dom.GetValue(this.location);
            }
        }

        public override string XmlLang
        {
            get
            {
                this.LoadOnDemand();
                string xmlLang = this.dom.nodes[this.location].xmlLang;
                if (xmlLang != null)
                {
                    return xmlLang;
                }
                return string.Empty;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Node
        {
            internal int parent;
            internal int firstAttribute;
            internal int firstChild;
            internal int firstNamespace;
            internal int nextSibling;
            internal int prevSibling;
            internal string baseUri;
            internal bool empty;
            internal string name;
            internal string ns;
            internal string prefix;
            internal string val;
            internal string xmlLang;
            internal XPathNodeType type;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Position
        {
            internal int elem;
            internal int parent;
            internal Position(int e, int p)
            {
                this.elem = e;
                this.parent = p;
            }
        }
    }
}

