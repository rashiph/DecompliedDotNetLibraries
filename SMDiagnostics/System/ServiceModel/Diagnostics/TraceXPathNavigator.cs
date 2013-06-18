namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.XPath;

    [DebuggerDisplay("")]
    internal class TraceXPathNavigator : XPathNavigator
    {
        private bool closed;
        private TraceNode current;
        private long currentSize;
        private int maxSize;
        private ElementNode root;
        private XPathNodeType state = XPathNodeType.Element;
        private const int UnlimitedSize = -1;

        public TraceXPathNavigator(int maxSize)
        {
            this.maxSize = maxSize;
            this.currentSize = 0L;
        }

        internal void AddAttribute(string name, string value, string xmlns, string prefix)
        {
            if (this.closed)
            {
                throw new InvalidOperationException();
            }
            if (this.current == null)
            {
                throw new InvalidOperationException();
            }
            AttributeNode node = new AttributeNode(name, prefix, value, xmlns);
            this.VerifySize(node);
            this.CurrentElement.attributes.Add(node);
        }

        internal void AddComment(string text)
        {
            if (this.closed)
            {
                throw new InvalidOperationException();
            }
            if (this.current == null)
            {
                throw new InvalidOperationException();
            }
            CommentNode node = new CommentNode(text, this.CurrentElement);
            this.VerifySize(node);
            this.CurrentElement.Add(node);
        }

        internal void AddElement(string prefix, string name, string xmlns)
        {
            if (this.closed)
            {
                throw new InvalidOperationException();
            }
            ElementNode node = new ElementNode(name, prefix, this.CurrentElement, xmlns);
            if (this.current == null)
            {
                this.VerifySize(node);
                this.root = node;
                this.current = this.root;
            }
            else if (!this.closed)
            {
                this.VerifySize(node);
                this.CurrentElement.Add(node);
                this.current = node;
            }
        }

        internal void AddProcessingInstruction(string name, string text)
        {
            if (this.current != null)
            {
                ProcessingInstructionNode node = new ProcessingInstructionNode(name, text, this.CurrentElement);
                this.VerifySize(node);
                this.CurrentElement.Add(node);
            }
        }

        internal void AddText(string value)
        {
            if (this.closed)
            {
                throw new InvalidOperationException();
            }
            if (this.current != null)
            {
                if (this.CurrentElement.text == null)
                {
                    TextNode node = new TextNode(value);
                    this.VerifySize(node);
                    this.CurrentElement.text = node;
                }
                else if (!string.IsNullOrEmpty(value))
                {
                    this.VerifySize(value);
                    this.CurrentElement.text.nodeValue = this.CurrentElement.text.nodeValue + value;
                }
            }
        }

        public override XPathNavigator Clone()
        {
            return this;
        }

        internal void CloseElement()
        {
            if (this.closed)
            {
                throw new InvalidOperationException();
            }
            this.current = this.CurrentElement.parent;
            if (this.current == null)
            {
                this.closed = true;
            }
        }

        public override bool IsSamePosition(XPathNavigator other)
        {
            return false;
        }

        public override string LookupPrefix(string ns)
        {
            return this.LookupPrefix(ns, this.CurrentElement);
        }

        private string LookupPrefix(string ns, ElementNode node)
        {
            string prefix = null;
            if (string.Compare(ns, node.xmlns, StringComparison.Ordinal) == 0)
            {
                prefix = node.prefix;
            }
            else
            {
                foreach (AttributeNode node2 in node.attributes)
                {
                    if ((string.Compare("xmlns", node2.prefix, StringComparison.Ordinal) == 0) && (string.Compare(ns, node2.nodeValue, StringComparison.Ordinal) == 0))
                    {
                        prefix = node2.name;
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(prefix) && (node.parent != null))
            {
                prefix = this.LookupPrefix(ns, node.parent);
            }
            return prefix;
        }

        private static void MaskElement(ElementNode element)
        {
            if (element != null)
            {
                element.childNodes.Clear();
                element.Add(new CommentNode("Removed", element));
                element.text = null;
                element.attributes = null;
            }
        }

        private static void MaskSubnodes(ElementNode element, string[] elementNames)
        {
            MaskSubnodes(element, elementNames, false);
        }

        private static void MaskSubnodes(ElementNode element, string[] elementNames, bool processNodeItself)
        {
            if (elementNames == null)
            {
                throw new ArgumentNullException("elementNames");
            }
            if (element != null)
            {
                bool flag = true;
                if (processNodeItself)
                {
                    foreach (string str in elementNames)
                    {
                        if (string.CompareOrdinal(str, element.name) == 0)
                        {
                            MaskElement(element);
                            flag = false;
                            break;
                        }
                    }
                }
                if (flag && (element.childNodes != null))
                {
                    foreach (ElementNode node in element.childNodes)
                    {
                        MaskSubnodes(node, elementNames, true);
                    }
                }
            }
        }

        public override bool MoveTo(XPathNavigator other)
        {
            return false;
        }

        public override bool MoveToFirstAttribute()
        {
            if (this.current == null)
            {
                throw new InvalidOperationException();
            }
            bool flag = this.CurrentElement.MoveToFirstAttribute();
            if (flag)
            {
                this.state = XPathNodeType.Attribute;
            }
            return flag;
        }

        public override bool MoveToFirstChild()
        {
            if (this.current == null)
            {
                throw new InvalidOperationException();
            }
            bool flag = false;
            if ((this.CurrentElement.childNodes != null) && (this.CurrentElement.childNodes.Count > 0))
            {
                this.current = this.CurrentElement.childNodes[0];
                this.state = this.current.NodeType;
                return true;
            }
            if (((this.CurrentElement.childNodes == null) || (this.CurrentElement.childNodes.Count == 0)) && (this.CurrentElement.text != null))
            {
                this.state = XPathNodeType.Text;
                this.CurrentElement.movedToText = true;
                flag = true;
            }
            return flag;
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            return false;
        }

        public override bool MoveToId(string id)
        {
            return false;
        }

        public override bool MoveToNext()
        {
            if (this.current == null)
            {
                throw new InvalidOperationException();
            }
            bool flag = false;
            if (this.state != XPathNodeType.Text)
            {
                ElementNode parent = this.current.parent;
                if (parent == null)
                {
                    return flag;
                }
                TraceNode node2 = parent.MoveToNext();
                if (((node2 == null) && (parent.text != null)) && !parent.movedToText)
                {
                    this.state = XPathNodeType.Text;
                    parent.movedToText = true;
                    this.current = parent;
                    return true;
                }
                if (node2 != null)
                {
                    this.state = node2.NodeType;
                    flag = true;
                    this.current = node2;
                }
            }
            return flag;
        }

        public override bool MoveToNextAttribute()
        {
            if (this.current == null)
            {
                throw new InvalidOperationException();
            }
            bool flag = this.CurrentElement.MoveToNextAttribute();
            if (flag)
            {
                this.state = XPathNodeType.Attribute;
            }
            return flag;
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            return false;
        }

        public override bool MoveToParent()
        {
            if (this.current == null)
            {
                throw new InvalidOperationException();
            }
            bool flag = false;
            switch (this.state)
            {
                case XPathNodeType.Element:
                case XPathNodeType.ProcessingInstruction:
                case XPathNodeType.Comment:
                    if (this.current.parent != null)
                    {
                        this.current = this.current.parent;
                        this.state = this.current.NodeType;
                        flag = true;
                    }
                    return flag;

                case XPathNodeType.Attribute:
                    this.state = XPathNodeType.Element;
                    return true;

                case XPathNodeType.Namespace:
                    this.state = XPathNodeType.Element;
                    return true;

                case XPathNodeType.Text:
                    this.state = XPathNodeType.Element;
                    return true;

                case XPathNodeType.SignificantWhitespace:
                case XPathNodeType.Whitespace:
                    return flag;
            }
            return flag;
        }

        public override bool MoveToPrevious()
        {
            return false;
        }

        public override void MoveToRoot()
        {
            this.current = this.root;
            this.state = XPathNodeType.Element;
            this.root.Reset();
        }

        public void RemovePii(string[][] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }
            foreach (string[] strArray in paths)
            {
                this.RemovePii(strArray);
            }
        }

        public void RemovePii(string[] path)
        {
            this.RemovePii(path, System.ServiceModel.Diagnostics.DiagnosticStrings.PiiList);
        }

        public void RemovePii(string[] headersPath, string[] piiList)
        {
            if (this.root == null)
            {
                throw new InvalidOperationException();
            }
            foreach (ElementNode node in this.root.FindSubnodes(headersPath))
            {
                MaskSubnodes(node, piiList);
            }
        }

        public override string ToString()
        {
            this.MoveToRoot();
            StringBuilder sb = new StringBuilder();
            new EncodingFallbackAwareXmlTextWriter(new StringWriter(sb, CultureInfo.CurrentCulture)).WriteNode(this, false);
            return sb.ToString();
        }

        private void VerifySize(int nodeSize)
        {
            if ((this.maxSize != -1) && ((this.currentSize + nodeSize) > this.maxSize))
            {
                throw new PlainXmlWriter.MaxSizeExceededException();
            }
            this.currentSize += nodeSize;
        }

        private void VerifySize(IMeasurable node)
        {
            this.VerifySize(node.Size);
        }

        private void VerifySize(string node)
        {
            this.VerifySize(node.Length);
        }

        public override string BaseURI
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return string.Empty;
            }
        }

        private CommentNode CurrentComment
        {
            get
            {
                return (this.current as CommentNode);
            }
        }

        private ElementNode CurrentElement
        {
            get
            {
                return (this.current as ElementNode);
            }
        }

        private ProcessingInstructionNode CurrentProcessingInstruction
        {
            get
            {
                return (this.current as ProcessingInstructionNode);
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                bool flag = true;
                if (this.current != null)
                {
                    flag = (this.CurrentElement.text != null) || (this.CurrentElement.childNodes.Count > 0);
                }
                return flag;
            }
        }

        [DebuggerDisplay("")]
        public override string LocalName
        {
            get
            {
                return this.Name;
            }
        }

        [DebuggerDisplay("")]
        public override string Name
        {
            get
            {
                string str = string.Empty;
                if (this.current != null)
                {
                    switch (this.state)
                    {
                        case XPathNodeType.Element:
                            return this.CurrentElement.name;

                        case XPathNodeType.Attribute:
                            return this.CurrentElement.CurrentAttribute.name;

                        case XPathNodeType.ProcessingInstruction:
                            return this.CurrentProcessingInstruction.name;
                    }
                }
                return str;
            }
        }

        [DebuggerDisplay("")]
        public override string NamespaceURI
        {
            get
            {
                string str = string.Empty;
                if (this.current != null)
                {
                    switch (this.state)
                    {
                        case XPathNodeType.Element:
                            return this.CurrentElement.xmlns;

                        case XPathNodeType.Attribute:
                            return this.CurrentElement.CurrentAttribute.xmlns;

                        case XPathNodeType.Namespace:
                            return null;
                    }
                }
                return str;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return null;
            }
        }

        [DebuggerDisplay("")]
        public override XPathNodeType NodeType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.state;
            }
        }

        [DebuggerDisplay("")]
        public override string Prefix
        {
            get
            {
                string str = string.Empty;
                if (this.current != null)
                {
                    switch (this.state)
                    {
                        case XPathNodeType.Element:
                            return this.CurrentElement.prefix;

                        case XPathNodeType.Attribute:
                            return this.CurrentElement.CurrentAttribute.prefix;

                        case XPathNodeType.Namespace:
                            return null;
                    }
                }
                return str;
            }
        }

        [DebuggerDisplay("")]
        public override string Value
        {
            get
            {
                string str = string.Empty;
                if (this.current != null)
                {
                    switch (this.state)
                    {
                        case XPathNodeType.Attribute:
                            return this.CurrentElement.CurrentAttribute.nodeValue;

                        case XPathNodeType.Namespace:
                        case XPathNodeType.SignificantWhitespace:
                        case XPathNodeType.Whitespace:
                            return str;

                        case XPathNodeType.Text:
                            return this.CurrentElement.text.nodeValue;

                        case XPathNodeType.ProcessingInstruction:
                            return this.CurrentProcessingInstruction.text;

                        case XPathNodeType.Comment:
                            return this.CurrentComment.nodeValue;
                    }
                }
                return str;
            }
        }

        internal System.Xml.WriteState WriteState
        {
            get
            {
                System.Xml.WriteState error = System.Xml.WriteState.Error;
                if (this.current == null)
                {
                    return System.Xml.WriteState.Start;
                }
                if (this.closed)
                {
                    return System.Xml.WriteState.Closed;
                }
                switch (this.state)
                {
                    case XPathNodeType.Element:
                        return System.Xml.WriteState.Element;

                    case XPathNodeType.Attribute:
                        return System.Xml.WriteState.Attribute;

                    case XPathNodeType.Namespace:
                        return error;

                    case XPathNodeType.Text:
                        return System.Xml.WriteState.Content;

                    case XPathNodeType.Comment:
                        return System.Xml.WriteState.Content;
                }
                return error;
            }
        }

        private class AttributeNode : TraceXPathNavigator.IMeasurable
        {
            internal string name;
            internal string nodeValue;
            internal string prefix;
            internal string xmlns;

            internal AttributeNode(string name, string prefix, string value, string xmlns)
            {
                this.name = name;
                this.prefix = prefix;
                this.nodeValue = value;
                this.xmlns = xmlns;
            }

            public int Size
            {
                get
                {
                    int num = (this.name.Length + this.nodeValue.Length) + 5;
                    if (!string.IsNullOrEmpty(this.prefix))
                    {
                        num += this.prefix.Length + 1;
                    }
                    if (!string.IsNullOrEmpty(this.xmlns))
                    {
                        num += this.xmlns.Length + 9;
                    }
                    return num;
                }
            }
        }

        private class CommentNode : TraceXPathNavigator.TraceNode, TraceXPathNavigator.IMeasurable
        {
            internal string nodeValue;

            internal CommentNode(string text, TraceXPathNavigator.ElementNode parent) : base(XPathNodeType.Comment, parent)
            {
                this.nodeValue = text;
            }

            public int Size
            {
                get
                {
                    return (this.nodeValue.Length + 8);
                }
            }
        }

        private class ElementNode : TraceXPathNavigator.TraceNode, TraceXPathNavigator.IMeasurable
        {
            private int attributeIndex;
            internal List<TraceXPathNavigator.AttributeNode> attributes;
            internal List<TraceXPathNavigator.TraceNode> childNodes;
            private int elementIndex;
            internal bool movedToText;
            internal string name;
            internal string prefix;
            internal TraceXPathNavigator.TextNode text;
            internal string xmlns;

            internal ElementNode(string name, string prefix, TraceXPathNavigator.ElementNode parent, string xmlns) : base(XPathNodeType.Element, parent)
            {
                this.childNodes = new List<TraceXPathNavigator.TraceNode>();
                this.attributes = new List<TraceXPathNavigator.AttributeNode>();
                this.name = name;
                this.prefix = prefix;
                this.xmlns = xmlns;
            }

            internal void Add(TraceXPathNavigator.TraceNode node)
            {
                this.childNodes.Add(node);
            }

            internal IEnumerable<TraceXPathNavigator.ElementNode> FindSubnodes(string[] headersPath)
            {
                if (headersPath == null)
                {
                    throw new ArgumentNullException("headersPath");
                }
                TraceXPathNavigator.ElementNode iteratorVariable0 = this;
                if (string.CompareOrdinal(iteratorVariable0.name, headersPath[0]) != 0)
                {
                    iteratorVariable0 = null;
                }
                int index = 0;
                while ((iteratorVariable0 != null) && (++index < headersPath.Length))
                {
                    TraceXPathNavigator.ElementNode iteratorVariable2 = null;
                    if (iteratorVariable0.childNodes != null)
                    {
                        foreach (TraceXPathNavigator.TraceNode iteratorVariable3 in iteratorVariable0.childNodes)
                        {
                            if (iteratorVariable3.NodeType == XPathNodeType.Element)
                            {
                                TraceXPathNavigator.ElementNode iteratorVariable4 = iteratorVariable3 as TraceXPathNavigator.ElementNode;
                                if ((iteratorVariable4 != null) && (string.CompareOrdinal(iteratorVariable4.name, headersPath[index]) == 0))
                                {
                                    if (headersPath.Length == (index + 1))
                                    {
                                        yield return iteratorVariable4;
                                    }
                                    else
                                    {
                                        iteratorVariable2 = iteratorVariable4;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    iteratorVariable0 = iteratorVariable2;
                }
            }

            internal bool MoveToFirstAttribute()
            {
                this.attributeIndex = 0;
                return ((this.attributes != null) && (this.attributes.Count > 0));
            }

            internal TraceXPathNavigator.TraceNode MoveToNext()
            {
                TraceXPathNavigator.TraceNode node = null;
                if ((this.elementIndex + 1) < this.childNodes.Count)
                {
                    this.elementIndex++;
                    node = this.childNodes[this.elementIndex];
                }
                return node;
            }

            internal bool MoveToNextAttribute()
            {
                bool flag = false;
                if ((this.attributeIndex + 1) < this.attributes.Count)
                {
                    this.attributeIndex++;
                    flag = true;
                }
                return flag;
            }

            internal void Reset()
            {
                this.attributeIndex = 0;
                this.elementIndex = 0;
                this.movedToText = false;
                if (this.childNodes != null)
                {
                    foreach (TraceXPathNavigator.TraceNode node in this.childNodes)
                    {
                        if (node.NodeType == XPathNodeType.Element)
                        {
                            TraceXPathNavigator.ElementNode node2 = node as TraceXPathNavigator.ElementNode;
                            if (node2 != null)
                            {
                                node2.Reset();
                            }
                        }
                    }
                }
            }

            internal TraceXPathNavigator.AttributeNode CurrentAttribute
            {
                get
                {
                    return this.attributes[this.attributeIndex];
                }
            }

            public int Size
            {
                get
                {
                    int num = (2 * this.name.Length) + 6;
                    if (!string.IsNullOrEmpty(this.prefix))
                    {
                        num += this.prefix.Length + 1;
                    }
                    if (!string.IsNullOrEmpty(this.xmlns))
                    {
                        num += this.xmlns.Length + 9;
                    }
                    return num;
                }
            }

        }

        private interface IMeasurable
        {
            int Size { get; }
        }

        private class ProcessingInstructionNode : TraceXPathNavigator.TraceNode, TraceXPathNavigator.IMeasurable
        {
            internal string name;
            internal string text;

            internal ProcessingInstructionNode(string name, string text, TraceXPathNavigator.ElementNode parent) : base(XPathNodeType.ProcessingInstruction, parent)
            {
                this.name = name;
                this.text = text;
            }

            public int Size
            {
                get
                {
                    return ((this.name.Length + this.text.Length) + 12);
                }
            }
        }

        private class TextNode : TraceXPathNavigator.IMeasurable
        {
            internal string nodeValue;

            internal TextNode(string value)
            {
                this.nodeValue = value;
            }

            public int Size
            {
                get
                {
                    return this.nodeValue.Length;
                }
            }
        }

        private class TraceNode
        {
            private XPathNodeType nodeType;
            internal TraceXPathNavigator.ElementNode parent;

            protected TraceNode(XPathNodeType nodeType, TraceXPathNavigator.ElementNode parent)
            {
                this.nodeType = nodeType;
                this.parent = parent;
            }

            internal XPathNodeType NodeType
            {
                get
                {
                    return this.nodeType;
                }
            }
        }
    }
}

