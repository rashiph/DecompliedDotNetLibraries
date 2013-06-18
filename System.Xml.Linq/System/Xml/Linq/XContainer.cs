namespace System.Xml.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Xml;

    public abstract class XContainer : XNode
    {
        internal object content;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal XContainer()
        {
        }

        internal XContainer(XContainer other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            if (other.content is string)
            {
                this.content = other.content;
            }
            else
            {
                XNode content = (XNode) other.content;
                if (content != null)
                {
                    do
                    {
                        this.AppendNodeSkipNotify(content.next.CloneNode());
                    }
                    while (content != other.content);
                }
            }
        }

        public void Add(object content)
        {
            if (base.SkipNotify())
            {
                this.AddContentSkipNotify(content);
            }
            else if (content != null)
            {
                XNode n = content as XNode;
                if (n != null)
                {
                    this.AddNode(n);
                }
                else
                {
                    string s = content as string;
                    if (s != null)
                    {
                        this.AddString(s);
                    }
                    else
                    {
                        XAttribute a = content as XAttribute;
                        if (a != null)
                        {
                            this.AddAttribute(a);
                        }
                        else
                        {
                            XStreamingElement other = content as XStreamingElement;
                            if (other != null)
                            {
                                this.AddNode(new XElement(other));
                            }
                            else
                            {
                                object[] objArray = content as object[];
                                if (objArray != null)
                                {
                                    foreach (object obj2 in objArray)
                                    {
                                        this.Add(obj2);
                                    }
                                }
                                else
                                {
                                    IEnumerable enumerable = content as IEnumerable;
                                    if (enumerable != null)
                                    {
                                        foreach (object obj3 in enumerable)
                                        {
                                            this.Add(obj3);
                                        }
                                    }
                                    else
                                    {
                                        this.AddString(GetStringValue(content));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Add(params object[] content)
        {
            this.Add(content);
        }

        internal virtual void AddAttribute(XAttribute a)
        {
        }

        internal virtual void AddAttributeSkipNotify(XAttribute a)
        {
        }

        internal void AddContentSkipNotify(object content)
        {
            if (content != null)
            {
                XNode n = content as XNode;
                if (n != null)
                {
                    this.AddNodeSkipNotify(n);
                }
                else
                {
                    string s = content as string;
                    if (s != null)
                    {
                        this.AddStringSkipNotify(s);
                    }
                    else
                    {
                        XAttribute a = content as XAttribute;
                        if (a != null)
                        {
                            this.AddAttributeSkipNotify(a);
                        }
                        else
                        {
                            XStreamingElement other = content as XStreamingElement;
                            if (other != null)
                            {
                                this.AddNodeSkipNotify(new XElement(other));
                            }
                            else
                            {
                                object[] objArray = content as object[];
                                if (objArray != null)
                                {
                                    foreach (object obj2 in objArray)
                                    {
                                        this.AddContentSkipNotify(obj2);
                                    }
                                }
                                else
                                {
                                    IEnumerable enumerable = content as IEnumerable;
                                    if (enumerable != null)
                                    {
                                        foreach (object obj3 in enumerable)
                                        {
                                            this.AddContentSkipNotify(obj3);
                                        }
                                    }
                                    else
                                    {
                                        this.AddStringSkipNotify(GetStringValue(content));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void AddContentToList(List<object> list, object content)
        {
            IEnumerable enumerable = (content is string) ? null : (content as IEnumerable);
            if (enumerable == null)
            {
                list.Add(content);
            }
            else
            {
                foreach (object obj2 in enumerable)
                {
                    if (obj2 != null)
                    {
                        AddContentToList(list, obj2);
                    }
                }
            }
        }

        public void AddFirst(object content)
        {
            new Inserter(this, null).Add(content);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void AddFirst(params object[] content)
        {
            this.AddFirst(content);
        }

        internal void AddNode(XNode n)
        {
            this.ValidateNode(n, this);
            if (n.parent != null)
            {
                n = n.CloneNode();
            }
            else
            {
                XNode parent = this;
                while (parent.parent != null)
                {
                    parent = parent.parent;
                }
                if (n == parent)
                {
                    n = n.CloneNode();
                }
            }
            this.ConvertTextToNode();
            this.AppendNode(n);
        }

        internal void AddNodeSkipNotify(XNode n)
        {
            this.ValidateNode(n, this);
            if (n.parent != null)
            {
                n = n.CloneNode();
            }
            else
            {
                XNode parent = this;
                while (parent.parent != null)
                {
                    parent = parent.parent;
                }
                if (n == parent)
                {
                    n = n.CloneNode();
                }
            }
            this.ConvertTextToNode();
            this.AppendNodeSkipNotify(n);
        }

        internal void AddString(string s)
        {
            this.ValidateString(s);
            if (this.content == null)
            {
                if (s.Length > 0)
                {
                    this.AppendNode(new XText(s));
                }
                else if (this is XElement)
                {
                    base.NotifyChanging(this, XObjectChangeEventArgs.Value);
                    if (this.content != null)
                    {
                        throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExternalCode"));
                    }
                    this.content = s;
                    base.NotifyChanged(this, XObjectChangeEventArgs.Value);
                }
                else
                {
                    this.content = s;
                }
            }
            else if (s.Length > 0)
            {
                this.ConvertTextToNode();
                XText content = this.content as XText;
                if ((content != null) && !(content is XCData))
                {
                    content.Value = content.Value + s;
                }
                else
                {
                    this.AppendNode(new XText(s));
                }
            }
        }

        internal void AddStringSkipNotify(string s)
        {
            this.ValidateString(s);
            if (this.content == null)
            {
                this.content = s;
            }
            else if (s.Length > 0)
            {
                if (this.content is string)
                {
                    this.content = ((string) this.content) + s;
                }
                else
                {
                    XText content = this.content as XText;
                    if ((content != null) && !(content is XCData))
                    {
                        content.text = content.text + s;
                    }
                    else
                    {
                        this.AppendNodeSkipNotify(new XText(s));
                    }
                }
            }
        }

        internal void AppendNode(XNode n)
        {
            bool flag = base.NotifyChanging(n, XObjectChangeEventArgs.Add);
            if (n.parent != null)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExternalCode"));
            }
            this.AppendNodeSkipNotify(n);
            if (flag)
            {
                base.NotifyChanged(n, XObjectChangeEventArgs.Add);
            }
        }

        internal void AppendNodeSkipNotify(XNode n)
        {
            n.parent = this;
            if ((this.content == null) || (this.content is string))
            {
                n.next = n;
            }
            else
            {
                XNode content = (XNode) this.content;
                n.next = content.next;
                content.next = n;
            }
            this.content = n;
        }

        internal override void AppendText(StringBuilder sb)
        {
            string content = this.content as string;
            if (content != null)
            {
                sb.Append(content);
            }
            else
            {
                XNode node = (XNode) this.content;
                if (node != null)
                {
                    do
                    {
                        node.next.AppendText(sb);
                    }
                    while (node != this.content);
                }
            }
        }

        private string CollectText(ref XNode n)
        {
            string str = "";
            while ((n != null) && (n.NodeType == XmlNodeType.Text))
            {
                str = str + ((XText) n).Value;
                n = (n != this.content) ? n.next : null;
            }
            return str;
        }

        internal bool ContentsEqual(XContainer e)
        {
            if (this.content == e.content)
            {
                return true;
            }
            string textOnly = this.GetTextOnly();
            if (textOnly != null)
            {
                return (textOnly == e.GetTextOnly());
            }
            XNode content = this.content as XNode;
            XNode n = e.content as XNode;
            if ((content == null) || (n == null))
            {
                goto Label_00A9;
            }
            content = content.next;
            n = n.next;
        Label_0053:
            if (this.CollectText(ref content) == e.CollectText(ref n))
            {
                if ((content == null) && (n == null))
                {
                    return true;
                }
                if (((content != null) && (n != null)) && content.DeepEquals(n))
                {
                    content = (content != this.content) ? content.next : null;
                    n = (n != e.content) ? n.next : null;
                    goto Label_0053;
                }
            }
        Label_00A9:
            return false;
        }

        internal int ContentsHashCode()
        {
            string textOnly = this.GetTextOnly();
            if (textOnly != null)
            {
                return textOnly.GetHashCode();
            }
            int num = 0;
            XNode content = this.content as XNode;
            if (content == null)
            {
                return num;
            }
        Label_0022:
            content = content.next;
            string str2 = this.CollectText(ref content);
            if (str2.Length > 0)
            {
                num ^= str2.GetHashCode();
            }
            if (content != null)
            {
                num ^= content.GetDeepHashCode();
                if (content != this.content)
                {
                    goto Label_0022;
                }
            }
            return num;
        }

        internal void ConvertTextToNode()
        {
            string content = this.content as string;
            if ((content != null) && (content.Length > 0))
            {
                XText text;
                text = new XText(content) {
                    parent = this,
                    next = text
                };
                this.content = text;
            }
        }

        public XmlWriter CreateWriter()
        {
            XmlWriterSettings settings = new XmlWriterSettings {
                ConformanceLevel = (this is XDocument) ? ConformanceLevel.Document : ConformanceLevel.Fragment
            };
            return XmlWriter.Create(new XNodeBuilder(this), settings);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IEnumerable<XNode> DescendantNodes()
        {
            return this.GetDescendantNodes(false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IEnumerable<XElement> Descendants()
        {
            return this.GetDescendants(null, false);
        }

        public IEnumerable<XElement> Descendants(XName name)
        {
            if (name == null)
            {
                return XElement.EmptySequence;
            }
            return this.GetDescendants(name, false);
        }

        public XElement Element(XName name)
        {
            XNode content = this.content as XNode;
            if (content != null)
            {
                do
                {
                    content = content.next;
                    XElement element = content as XElement;
                    if ((element != null) && (element.name == name))
                    {
                        return element;
                    }
                }
                while (content != this.content);
            }
            return null;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IEnumerable<XElement> Elements()
        {
            return this.GetElements(null);
        }

        public IEnumerable<XElement> Elements(XName name)
        {
            if (name == null)
            {
                return XElement.EmptySequence;
            }
            return this.GetElements(name);
        }

        internal static object GetContentSnapshot(object content)
        {
            if ((content is string) || !(content is IEnumerable))
            {
                return content;
            }
            List<object> list = new List<object>();
            AddContentToList(list, content);
            return list;
        }

        internal static string GetDateTimeString(DateTime value)
        {
            return XmlConvert.ToString(value, XmlDateTimeSerializationMode.RoundtripKind);
        }

        internal IEnumerable<XNode> GetDescendantNodes(bool self)
        {
            if (self)
            {
                yield return this;
            }
            XNode next = this;
            while (true)
            {
                XNode iteratorVariable2;
                XContainer iteratorVariable1 = next as XContainer;
                if ((iteratorVariable1 == null) || ((iteratorVariable2 = iteratorVariable1.FirstNode) == null))
                {
                    while (((next != null) && (next != this)) && (next == next.parent.content))
                    {
                        next = next.parent;
                    }
                    if ((next == null) || (next == this))
                    {
                        break;
                    }
                    next = next.next;
                }
                else
                {
                    next = iteratorVariable2;
                }
                yield return next;
            }
        }

        internal IEnumerable<XElement> GetDescendants(XName name, bool self)
        {
            if (self)
            {
                XElement iteratorVariable0 = (XElement) this;
                if ((name == null) || (iteratorVariable0.name == name))
                {
                    yield return iteratorVariable0;
                }
            }
            XNode next = this;
            XContainer iteratorVariable2 = this;
            while (true)
            {
                if ((iteratorVariable2 == null) || !(iteratorVariable2.content is XNode))
                {
                    while ((next != this) && (next == next.parent.content))
                    {
                        next = next.parent;
                    }
                    if (next == this)
                    {
                        break;
                    }
                    next = next.next;
                }
                else
                {
                    next = ((XNode) iteratorVariable2.content).next;
                }
                XElement iteratorVariable3 = next as XElement;
                if ((iteratorVariable3 != null) && ((name == null) || (iteratorVariable3.name == name)))
                {
                    yield return iteratorVariable3;
                }
                iteratorVariable2 = iteratorVariable3;
            }
        }

        private IEnumerable<XElement> GetElements(XName name)
        {
            XNode content = this.content as XNode;
            if (content != null)
            {
                do
                {
                    content = content.next;
                    XElement iteratorVariable1 = content as XElement;
                    if ((iteratorVariable1 != null) && ((name == null) || (iteratorVariable1.name == name)))
                    {
                        yield return iteratorVariable1;
                    }
                }
                while ((content.parent == this) && (content != this.content));
            }
        }

        internal static string GetStringValue(object value)
        {
            string dateTimeString;
            if (value is string)
            {
                dateTimeString = (string) value;
            }
            else if (value is double)
            {
                dateTimeString = XmlConvert.ToString((double) value);
            }
            else if (value is float)
            {
                dateTimeString = XmlConvert.ToString((float) value);
            }
            else if (value is decimal)
            {
                dateTimeString = XmlConvert.ToString((decimal) value);
            }
            else if (value is bool)
            {
                dateTimeString = XmlConvert.ToString((bool) value);
            }
            else if (value is DateTime)
            {
                dateTimeString = GetDateTimeString((DateTime) value);
            }
            else if (value is DateTimeOffset)
            {
                dateTimeString = XmlConvert.ToString((DateTimeOffset) value);
            }
            else if (value is TimeSpan)
            {
                dateTimeString = XmlConvert.ToString((TimeSpan) value);
            }
            else
            {
                if (value is XObject)
                {
                    throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_XObjectValue"));
                }
                dateTimeString = value.ToString();
            }
            if (dateTimeString == null)
            {
                throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_ConvertToString"));
            }
            return dateTimeString;
        }

        private string GetTextOnly()
        {
            if (this.content == null)
            {
                return null;
            }
            string content = this.content as string;
            if (content == null)
            {
                XNode next = (XNode) this.content;
                do
                {
                    next = next.next;
                    if (next.NodeType != XmlNodeType.Text)
                    {
                        return null;
                    }
                    content = content + ((XText) next).Value;
                }
                while (next != this.content);
            }
            return content;
        }

        public IEnumerable<XNode> Nodes()
        {
            XNode lastNode = this.LastNode;
            if (lastNode != null)
            {
                do
                {
                    lastNode = lastNode.next;
                    yield return lastNode;
                }
                while ((lastNode.parent == this) && (lastNode != this.content));
            }
        }

        internal void ReadContentFrom(XmlReader r)
        {
            if (r.ReadState != System.Xml.ReadState.Interactive)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExpectedInteractive"));
            }
            XContainer parent = this;
            NamespaceCache cache = new NamespaceCache();
            NamespaceCache cache2 = new NamespaceCache();
            do
            {
                switch (r.NodeType)
                {
                    case XmlNodeType.Element:
                    {
                        XElement n = new XElement(cache.Get(r.NamespaceURI).GetName(r.LocalName));
                        if (r.MoveToFirstAttribute())
                        {
                            do
                            {
                                n.AppendAttributeSkipNotify(new XAttribute(cache2.Get((r.Prefix.Length == 0) ? string.Empty : r.NamespaceURI).GetName(r.LocalName), r.Value));
                            }
                            while (r.MoveToNextAttribute());
                            r.MoveToElement();
                        }
                        parent.AddNodeSkipNotify(n);
                        if (!r.IsEmptyElement)
                        {
                            parent = n;
                        }
                        break;
                    }
                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        parent.AddStringSkipNotify(r.Value);
                        break;

                    case XmlNodeType.CDATA:
                        parent.AddNodeSkipNotify(new XCData(r.Value));
                        break;

                    case XmlNodeType.EntityReference:
                        if (!r.CanResolveEntity)
                        {
                            throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_UnresolvedEntityReference"));
                        }
                        r.ResolveEntity();
                        break;

                    case XmlNodeType.ProcessingInstruction:
                        parent.AddNodeSkipNotify(new XProcessingInstruction(r.Name, r.Value));
                        break;

                    case XmlNodeType.Comment:
                        parent.AddNodeSkipNotify(new XComment(r.Value));
                        break;

                    case XmlNodeType.DocumentType:
                        parent.AddNodeSkipNotify(new XDocumentType(r.LocalName, r.GetAttribute("PUBLIC"), r.GetAttribute("SYSTEM"), r.Value, r.DtdInfo));
                        break;

                    case XmlNodeType.EndElement:
                        if (parent.content == null)
                        {
                            parent.content = string.Empty;
                        }
                        if (parent == this)
                        {
                            return;
                        }
                        parent = parent.parent;
                        break;

                    case XmlNodeType.EndEntity:
                        break;

                    default:
                        throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_UnexpectedNodeType", new object[] { r.NodeType }));
                }
            }
            while (r.Read());
        }

        internal void ReadContentFrom(XmlReader r, LoadOptions o)
        {
            if ((o & (LoadOptions.SetLineInfo | LoadOptions.SetBaseUri)) == LoadOptions.None)
            {
                this.ReadContentFrom(r);
            }
            else
            {
                if (r.ReadState != System.Xml.ReadState.Interactive)
                {
                    throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExpectedInteractive"));
                }
                XContainer parent = this;
                XNode n = null;
                NamespaceCache cache = new NamespaceCache();
                NamespaceCache cache2 = new NamespaceCache();
                string baseUri = ((o & LoadOptions.SetBaseUri) != LoadOptions.None) ? r.BaseURI : null;
                IXmlLineInfo info = ((o & LoadOptions.SetLineInfo) != LoadOptions.None) ? (r as IXmlLineInfo) : null;
                do
                {
                    string baseURI = r.BaseURI;
                    switch (r.NodeType)
                    {
                        case XmlNodeType.Element:
                        {
                            XElement element = new XElement(cache.Get(r.NamespaceURI).GetName(r.LocalName));
                            if ((baseUri != null) && (baseUri != baseURI))
                            {
                                element.SetBaseUri(baseURI);
                            }
                            if ((info != null) && info.HasLineInfo())
                            {
                                element.SetLineInfo(info.LineNumber, info.LinePosition);
                            }
                            if (r.MoveToFirstAttribute())
                            {
                                do
                                {
                                    XAttribute a = new XAttribute(cache2.Get((r.Prefix.Length == 0) ? string.Empty : r.NamespaceURI).GetName(r.LocalName), r.Value);
                                    if ((info != null) && info.HasLineInfo())
                                    {
                                        a.SetLineInfo(info.LineNumber, info.LinePosition);
                                    }
                                    element.AppendAttributeSkipNotify(a);
                                }
                                while (r.MoveToNextAttribute());
                                r.MoveToElement();
                            }
                            parent.AddNodeSkipNotify(element);
                            if (!r.IsEmptyElement)
                            {
                                parent = element;
                                if (baseUri != null)
                                {
                                    baseUri = baseURI;
                                }
                            }
                            break;
                        }
                        case XmlNodeType.Text:
                        case XmlNodeType.Whitespace:
                        case XmlNodeType.SignificantWhitespace:
                            if (((baseUri == null) || (baseUri == baseURI)) && ((info == null) || !info.HasLineInfo()))
                            {
                                parent.AddStringSkipNotify(r.Value);
                            }
                            else
                            {
                                n = new XText(r.Value);
                            }
                            break;

                        case XmlNodeType.CDATA:
                            n = new XCData(r.Value);
                            break;

                        case XmlNodeType.EntityReference:
                            if (!r.CanResolveEntity)
                            {
                                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_UnresolvedEntityReference"));
                            }
                            r.ResolveEntity();
                            break;

                        case XmlNodeType.ProcessingInstruction:
                            n = new XProcessingInstruction(r.Name, r.Value);
                            break;

                        case XmlNodeType.Comment:
                            n = new XComment(r.Value);
                            break;

                        case XmlNodeType.DocumentType:
                            n = new XDocumentType(r.LocalName, r.GetAttribute("PUBLIC"), r.GetAttribute("SYSTEM"), r.Value, r.DtdInfo);
                            break;

                        case XmlNodeType.EndElement:
                        {
                            if (parent.content == null)
                            {
                                parent.content = string.Empty;
                            }
                            XElement element2 = parent as XElement;
                            if (((element2 != null) && (info != null)) && info.HasLineInfo())
                            {
                                element2.SetEndElementLineInfo(info.LineNumber, info.LinePosition);
                            }
                            if (parent == this)
                            {
                                return;
                            }
                            if ((baseUri != null) && parent.HasBaseUri)
                            {
                                baseUri = parent.parent.BaseUri;
                            }
                            parent = parent.parent;
                            break;
                        }
                        case XmlNodeType.EndEntity:
                            break;

                        default:
                            throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_UnexpectedNodeType", new object[] { r.NodeType }));
                    }
                    if (n != null)
                    {
                        if ((baseUri != null) && (baseUri != baseURI))
                        {
                            n.SetBaseUri(baseURI);
                        }
                        if ((info != null) && info.HasLineInfo())
                        {
                            n.SetLineInfo(info.LineNumber, info.LinePosition);
                        }
                        parent.AddNodeSkipNotify(n);
                        n = null;
                    }
                }
                while (r.Read());
            }
        }

        internal void RemoveNode(XNode n)
        {
            bool flag = base.NotifyChanging(n, XObjectChangeEventArgs.Remove);
            if (n.parent != this)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExternalCode"));
            }
            XNode content = (XNode) this.content;
            while (content.next != n)
            {
                content = content.next;
            }
            if (content == n)
            {
                this.content = null;
            }
            else
            {
                if (this.content == n)
                {
                    this.content = content;
                }
                content.next = n.next;
            }
            n.parent = null;
            n.next = null;
            if (flag)
            {
                base.NotifyChanged(n, XObjectChangeEventArgs.Remove);
            }
        }

        public void RemoveNodes()
        {
            if (base.SkipNotify())
            {
                this.RemoveNodesSkipNotify();
            }
            else
            {
                while (this.content != null)
                {
                    string content = this.content as string;
                    if (content != null)
                    {
                        if (content.Length > 0)
                        {
                            this.ConvertTextToNode();
                        }
                        else if (this is XElement)
                        {
                            base.NotifyChanging(this, XObjectChangeEventArgs.Value);
                            if (content != this.content)
                            {
                                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExternalCode"));
                            }
                            this.content = null;
                            base.NotifyChanged(this, XObjectChangeEventArgs.Value);
                        }
                        else
                        {
                            this.content = null;
                        }
                    }
                    XNode node = this.content as XNode;
                    if (node != null)
                    {
                        XNode next = node.next;
                        base.NotifyChanging(next, XObjectChangeEventArgs.Remove);
                        if ((node != this.content) || (next != node.next))
                        {
                            throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExternalCode"));
                        }
                        if (next != node)
                        {
                            node.next = next.next;
                        }
                        else
                        {
                            this.content = null;
                        }
                        next.parent = null;
                        next.next = null;
                        base.NotifyChanged(next, XObjectChangeEventArgs.Remove);
                    }
                }
            }
        }

        private void RemoveNodesSkipNotify()
        {
            XNode content = this.content as XNode;
            if (content != null)
            {
                do
                {
                    XNode next = content.next;
                    content.parent = null;
                    content.next = null;
                    content = next;
                }
                while (content != this.content);
            }
            this.content = null;
        }

        public void ReplaceNodes(object content)
        {
            content = GetContentSnapshot(content);
            this.RemoveNodes();
            this.Add(content);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void ReplaceNodes(params object[] content)
        {
            this.ReplaceNodes(content);
        }

        internal virtual void ValidateNode(XNode node, XNode previous)
        {
        }

        internal virtual void ValidateString(string s)
        {
        }

        internal void WriteContentTo(XmlWriter writer)
        {
            if (this.content != null)
            {
                if (this.content is string)
                {
                    if (this is XDocument)
                    {
                        writer.WriteWhitespace((string) this.content);
                    }
                    else
                    {
                        writer.WriteString((string) this.content);
                    }
                }
                else
                {
                    XNode content = (XNode) this.content;
                    do
                    {
                        content.next.WriteTo(writer);
                    }
                    while (content != this.content);
                }
            }
        }

        public XNode FirstNode
        {
            get
            {
                XNode lastNode = this.LastNode;
                if (lastNode == null)
                {
                    return null;
                }
                return lastNode.next;
            }
        }

        public XNode LastNode
        {
            get
            {
                if (this.content == null)
                {
                    return null;
                }
                XNode content = this.content as XNode;
                if (content != null)
                {
                    return content;
                }
                string str = this.content as string;
                if (str != null)
                {
                    XText text;
                    if (str.Length == 0)
                    {
                        return null;
                    }
                    text = new XText(str) {
                        parent = this,
                        next = text
                    };
                    Interlocked.CompareExchange<object>(ref this.content, text, str);
                }
                return (XNode) this.content;
            }
        }




    }
}

