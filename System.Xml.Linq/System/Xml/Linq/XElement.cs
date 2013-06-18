namespace System.Xml.Linq
{
    using MS.Internal.Xml.Linq.ComponentModel;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [TypeDescriptionProvider(typeof(XTypeDescriptionProvider<XElement>)), XmlSchemaProvider(null, IsAny=true)]
    public class XElement : XContainer, IXmlSerializable
    {
        private static IEnumerable<XElement> emptySequence;
        internal XAttribute lastAttr;
        internal XName name;

        internal XElement() : this("default")
        {
        }

        public XElement(XElement other) : base(other)
        {
            this.name = other.name;
            XAttribute lastAttr = other.lastAttr;
            if (lastAttr != null)
            {
                do
                {
                    lastAttr = lastAttr.next;
                    this.AppendAttributeSkipNotify(new XAttribute(lastAttr));
                }
                while (lastAttr != other.lastAttr);
            }
        }

        public XElement(XName name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.name = name;
        }

        public XElement(XStreamingElement other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            this.name = other.name;
            base.AddContentSkipNotify(other.content);
        }

        internal XElement(XmlReader r) : this(r, LoadOptions.None)
        {
        }

        public XElement(XName name, object content) : this(name)
        {
            base.AddContentSkipNotify(content);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XElement(XName name, params object[] content) : this(name, content)
        {
        }

        internal XElement(XmlReader r, LoadOptions o)
        {
            this.ReadElementFrom(r, o);
        }

        internal override void AddAttribute(XAttribute a)
        {
            if (this.Attribute(a.Name) != null)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_DuplicateAttribute"));
            }
            if (a.parent != null)
            {
                a = new XAttribute(a);
            }
            this.AppendAttribute(a);
        }

        internal override void AddAttributeSkipNotify(XAttribute a)
        {
            if (this.Attribute(a.Name) != null)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_DuplicateAttribute"));
            }
            if (a.parent != null)
            {
                a = new XAttribute(a);
            }
            this.AppendAttributeSkipNotify(a);
        }

        public IEnumerable<XElement> AncestorsAndSelf()
        {
            return base.GetAncestors(null, true);
        }

        public IEnumerable<XElement> AncestorsAndSelf(XName name)
        {
            if (name == null)
            {
                return EmptySequence;
            }
            return base.GetAncestors(name, true);
        }

        internal void AppendAttribute(XAttribute a)
        {
            bool flag = base.NotifyChanging(a, XObjectChangeEventArgs.Add);
            if (a.parent != null)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExternalCode"));
            }
            this.AppendAttributeSkipNotify(a);
            if (flag)
            {
                base.NotifyChanged(a, XObjectChangeEventArgs.Add);
            }
        }

        internal void AppendAttributeSkipNotify(XAttribute a)
        {
            a.parent = this;
            if (this.lastAttr == null)
            {
                a.next = a;
            }
            else
            {
                a.next = this.lastAttr.next;
                this.lastAttr.next = a;
            }
            this.lastAttr = a;
        }

        public XAttribute Attribute(XName name)
        {
            XAttribute lastAttr = this.lastAttr;
            if (lastAttr != null)
            {
                do
                {
                    lastAttr = lastAttr.next;
                    if (lastAttr.name == name)
                    {
                        return lastAttr;
                    }
                }
                while (lastAttr != this.lastAttr);
            }
            return null;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IEnumerable<XAttribute> Attributes()
        {
            return this.GetAttributes(null);
        }

        public IEnumerable<XAttribute> Attributes(XName name)
        {
            if (name == null)
            {
                return XAttribute.EmptySequence;
            }
            return this.GetAttributes(name);
        }

        private bool AttributesEqual(XElement e)
        {
            XAttribute lastAttr = this.lastAttr;
            XAttribute next = e.lastAttr;
            if ((lastAttr != null) && (next != null))
            {
                do
                {
                    lastAttr = lastAttr.next;
                    next = next.next;
                    if ((lastAttr.name != next.name) || (lastAttr.value != next.value))
                    {
                        return false;
                    }
                }
                while (lastAttr != this.lastAttr);
                return (next == e.lastAttr);
            }
            return ((lastAttr == null) && (next == null));
        }

        internal override XNode CloneNode()
        {
            return new XElement(this);
        }

        internal override bool DeepEquals(XNode node)
        {
            XElement e = node as XElement;
            return ((((e != null) && (this.name == e.name)) && base.ContentsEqual(e)) && this.AttributesEqual(e));
        }

        public IEnumerable<XNode> DescendantNodesAndSelf()
        {
            return base.GetDescendantNodes(true);
        }

        public IEnumerable<XElement> DescendantsAndSelf()
        {
            return base.GetDescendants(null, true);
        }

        public IEnumerable<XElement> DescendantsAndSelf(XName name)
        {
            if (name == null)
            {
                return EmptySequence;
            }
            return base.GetDescendants(name, true);
        }

        private IEnumerable<XAttribute> GetAttributes(XName name)
        {
            XAttribute lastAttr = this.lastAttr;
            if (lastAttr != null)
            {
                do
                {
                    lastAttr = lastAttr.next;
                    if ((name == null) || (lastAttr.name == name))
                    {
                        yield return lastAttr;
                    }
                }
                while ((lastAttr.parent == this) && (lastAttr != this.lastAttr));
            }
        }

        internal override int GetDeepHashCode()
        {
            int num = this.name.GetHashCode() ^ base.ContentsHashCode();
            XAttribute lastAttr = this.lastAttr;
            if (lastAttr != null)
            {
                do
                {
                    lastAttr = lastAttr.next;
                    num ^= lastAttr.GetDeepHashCode();
                }
                while (lastAttr != this.lastAttr);
            }
            return num;
        }

        public XNamespace GetDefaultNamespace()
        {
            string namespaceOfPrefixInScope = this.GetNamespaceOfPrefixInScope("xmlns", null);
            if (namespaceOfPrefixInScope == null)
            {
                return XNamespace.None;
            }
            return XNamespace.Get(namespaceOfPrefixInScope);
        }

        public XNamespace GetNamespaceOfPrefix(string prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }
            if (prefix.Length == 0)
            {
                throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_InvalidPrefix", new object[] { prefix }));
            }
            if (prefix == "xmlns")
            {
                return XNamespace.Xmlns;
            }
            string namespaceOfPrefixInScope = this.GetNamespaceOfPrefixInScope(prefix, null);
            if (namespaceOfPrefixInScope != null)
            {
                return XNamespace.Get(namespaceOfPrefixInScope);
            }
            if (prefix == "xml")
            {
                return XNamespace.Xml;
            }
            return null;
        }

        private string GetNamespaceOfPrefixInScope(string prefix, XElement outOfScope)
        {
            for (XElement element = this; element != outOfScope; element = element.parent as XElement)
            {
                XAttribute lastAttr = element.lastAttr;
                if (lastAttr != null)
                {
                    do
                    {
                        lastAttr = lastAttr.next;
                        if (lastAttr.IsNamespaceDeclaration && (lastAttr.Name.LocalName == prefix))
                        {
                            return lastAttr.Value;
                        }
                    }
                    while (lastAttr != element.lastAttr);
                }
            }
            return null;
        }

        public string GetPrefixOfNamespace(XNamespace ns)
        {
            if (ns == null)
            {
                throw new ArgumentNullException("ns");
            }
            string namespaceName = ns.NamespaceName;
            bool flag = false;
            XElement outOfScope = this;
            do
            {
                XAttribute lastAttr = outOfScope.lastAttr;
                if (lastAttr != null)
                {
                    bool flag2 = false;
                    do
                    {
                        lastAttr = lastAttr.next;
                        if (lastAttr.IsNamespaceDeclaration)
                        {
                            if (((lastAttr.Value == namespaceName) && (lastAttr.Name.NamespaceName.Length != 0)) && (!flag || (this.GetNamespaceOfPrefixInScope(lastAttr.Name.LocalName, outOfScope) == null)))
                            {
                                return lastAttr.Name.LocalName;
                            }
                            flag2 = true;
                        }
                    }
                    while (lastAttr != outOfScope.lastAttr);
                    flag |= flag2;
                }
                outOfScope = outOfScope.parent as XElement;
            }
            while (outOfScope != null);
            switch (namespaceName)
            {
                case "http://www.w3.org/XML/1998/namespace":
                    if (!flag || (this.GetNamespaceOfPrefixInScope("xml", null) == null))
                    {
                        return "xml";
                    }
                    break;

                case "http://www.w3.org/2000/xmlns/":
                    return "xmlns";
            }
            return null;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XElement Load(Stream stream)
        {
            return Load(stream, LoadOptions.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XElement Load(TextReader textReader)
        {
            return Load(textReader, LoadOptions.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XElement Load(string uri)
        {
            return Load(uri, LoadOptions.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XElement Load(XmlReader reader)
        {
            return Load(reader, LoadOptions.None);
        }

        public static XElement Load(Stream stream, LoadOptions options)
        {
            XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
            using (XmlReader reader = XmlReader.Create(stream, xmlReaderSettings))
            {
                return Load(reader, options);
            }
        }

        public static XElement Load(TextReader textReader, LoadOptions options)
        {
            XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
            using (XmlReader reader = XmlReader.Create(textReader, xmlReaderSettings))
            {
                return Load(reader, options);
            }
        }

        public static XElement Load(string uri, LoadOptions options)
        {
            XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
            using (XmlReader reader = XmlReader.Create(uri, xmlReaderSettings))
            {
                return Load(reader, options);
            }
        }

        public static XElement Load(XmlReader reader, LoadOptions options)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (reader.MoveToContent() != XmlNodeType.Element)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExpectedNodeType", new object[] { XmlNodeType.Element, reader.NodeType }));
            }
            XElement element = new XElement(reader, options);
            reader.MoveToContent();
            if (!reader.EOF)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExpectedEndOfFile"));
            }
            return element;
        }

        [CLSCompliant(false)]
        public static explicit operator bool(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return XmlConvert.ToBoolean(element.Value.ToLower(CultureInfo.InvariantCulture));
        }

        [CLSCompliant(false)]
        public static explicit operator DateTime(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return DateTime.Parse(element.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        [CLSCompliant(false)]
        public static explicit operator DateTimeOffset(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return XmlConvert.ToDateTimeOffset(element.Value);
        }

        [CLSCompliant(false)]
        public static explicit operator decimal(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return XmlConvert.ToDecimal(element.Value);
        }

        [CLSCompliant(false)]
        public static explicit operator double(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return XmlConvert.ToDouble(element.Value);
        }

        [CLSCompliant(false)]
        public static explicit operator Guid(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return XmlConvert.ToGuid(element.Value);
        }

        [CLSCompliant(false)]
        public static explicit operator int(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return XmlConvert.ToInt32(element.Value);
        }

        [CLSCompliant(false)]
        public static explicit operator long(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return XmlConvert.ToInt64(element.Value);
        }

        [CLSCompliant(false)]
        public static explicit operator bool?(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return new bool?(XmlConvert.ToBoolean(element.Value.ToLower(CultureInfo.InvariantCulture)));
        }

        [CLSCompliant(false)]
        public static explicit operator float(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return XmlConvert.ToSingle(element.Value);
        }

        [CLSCompliant(false)]
        public static explicit operator ulong(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return XmlConvert.ToUInt64(element.Value);
        }

        [CLSCompliant(false)]
        public static explicit operator DateTime?(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return new DateTime?(DateTime.Parse(element.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
        }

        [CLSCompliant(false)]
        public static explicit operator TimeSpan(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return XmlConvert.ToTimeSpan(element.Value);
        }

        [CLSCompliant(false)]
        public static explicit operator DateTimeOffset?(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return new DateTimeOffset?(XmlConvert.ToDateTimeOffset(element.Value));
        }

        [CLSCompliant(false)]
        public static explicit operator decimal?(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return new decimal?(XmlConvert.ToDecimal(element.Value));
        }

        [CLSCompliant(false)]
        public static explicit operator double?(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return new double?(XmlConvert.ToDouble(element.Value));
        }

        [CLSCompliant(false)]
        public static explicit operator long?(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return new long?(XmlConvert.ToInt64(element.Value));
        }

        [CLSCompliant(false)]
        public static explicit operator string(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return element.Value;
        }

        [CLSCompliant(false)]
        public static explicit operator Guid?(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return new Guid?(XmlConvert.ToGuid(element.Value));
        }

        [CLSCompliant(false)]
        public static explicit operator int?(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return new int?(XmlConvert.ToInt32(element.Value));
        }

        [CLSCompliant(false)]
        public static explicit operator float?(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return new float?(XmlConvert.ToSingle(element.Value));
        }

        [CLSCompliant(false)]
        public static explicit operator TimeSpan?(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return new TimeSpan?(XmlConvert.ToTimeSpan(element.Value));
        }

        [CLSCompliant(false)]
        public static explicit operator uint(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return XmlConvert.ToUInt32(element.Value);
        }

        [CLSCompliant(false)]
        public static explicit operator uint?(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return new uint?(XmlConvert.ToUInt32(element.Value));
        }

        [CLSCompliant(false)]
        public static explicit operator ulong?(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return new ulong?(XmlConvert.ToUInt64(element.Value));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XElement Parse(string text)
        {
            return Parse(text, LoadOptions.None);
        }

        public static XElement Parse(string text, LoadOptions options)
        {
            XElement element;
            using (StringReader reader = new StringReader(text))
            {
                XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
                using (XmlReader reader2 = XmlReader.Create(reader, xmlReaderSettings))
                {
                    element = Load(reader2, options);
                }
            }
            return element;
        }

        private void ReadElementFrom(XmlReader r, LoadOptions o)
        {
            if (r.ReadState != System.Xml.ReadState.Interactive)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExpectedInteractive"));
            }
            this.name = XNamespace.Get(r.NamespaceURI).GetName(r.LocalName);
            if ((o & LoadOptions.SetBaseUri) != LoadOptions.None)
            {
                string baseURI = r.BaseURI;
                if ((baseURI != null) && (baseURI.Length != 0))
                {
                    base.SetBaseUri(baseURI);
                }
            }
            IXmlLineInfo info = null;
            if ((o & LoadOptions.SetLineInfo) != LoadOptions.None)
            {
                info = r as IXmlLineInfo;
                if ((info != null) && info.HasLineInfo())
                {
                    base.SetLineInfo(info.LineNumber, info.LinePosition);
                }
            }
            if (r.MoveToFirstAttribute())
            {
                do
                {
                    XAttribute a = new XAttribute(XNamespace.Get((r.Prefix.Length == 0) ? string.Empty : r.NamespaceURI).GetName(r.LocalName), r.Value);
                    if ((info != null) && info.HasLineInfo())
                    {
                        a.SetLineInfo(info.LineNumber, info.LinePosition);
                    }
                    this.AppendAttributeSkipNotify(a);
                }
                while (r.MoveToNextAttribute());
                r.MoveToElement();
            }
            if (!r.IsEmptyElement)
            {
                r.Read();
                base.ReadContentFrom(r, o);
            }
            r.Read();
        }

        public void RemoveAll()
        {
            this.RemoveAttributes();
            base.RemoveNodes();
        }

        internal void RemoveAttribute(XAttribute a)
        {
            XAttribute attribute2;
            bool flag = base.NotifyChanging(a, XObjectChangeEventArgs.Remove);
            if (a.parent != this)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExternalCode"));
            }
            XAttribute lastAttr = this.lastAttr;
            while ((attribute2 = lastAttr.next) != a)
            {
                lastAttr = attribute2;
            }
            if (lastAttr == a)
            {
                this.lastAttr = null;
            }
            else
            {
                if (this.lastAttr == a)
                {
                    this.lastAttr = lastAttr;
                }
                lastAttr.next = a.next;
            }
            a.parent = null;
            a.next = null;
            if (flag)
            {
                base.NotifyChanged(a, XObjectChangeEventArgs.Remove);
            }
        }

        public void RemoveAttributes()
        {
            if (base.SkipNotify())
            {
                this.RemoveAttributesSkipNotify();
            }
            else
            {
                while (this.lastAttr != null)
                {
                    XAttribute next = this.lastAttr.next;
                    base.NotifyChanging(next, XObjectChangeEventArgs.Remove);
                    if ((this.lastAttr == null) || (next != this.lastAttr.next))
                    {
                        throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExternalCode"));
                    }
                    if (next != this.lastAttr)
                    {
                        this.lastAttr.next = next.next;
                    }
                    else
                    {
                        this.lastAttr = null;
                    }
                    next.parent = null;
                    next.next = null;
                    base.NotifyChanged(next, XObjectChangeEventArgs.Remove);
                }
            }
        }

        private void RemoveAttributesSkipNotify()
        {
            if (this.lastAttr != null)
            {
                XAttribute lastAttr = this.lastAttr;
                do
                {
                    XAttribute next = lastAttr.next;
                    lastAttr.parent = null;
                    lastAttr.next = null;
                    lastAttr = next;
                }
                while (lastAttr != this.lastAttr);
                this.lastAttr = null;
            }
        }

        public void ReplaceAll(object content)
        {
            content = XContainer.GetContentSnapshot(content);
            this.RemoveAll();
            base.Add(content);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void ReplaceAll(params object[] content)
        {
            this.ReplaceAll(content);
        }

        public void ReplaceAttributes(object content)
        {
            content = XContainer.GetContentSnapshot(content);
            this.RemoveAttributes();
            base.Add(content);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void ReplaceAttributes(params object[] content)
        {
            this.ReplaceAttributes(content);
        }

        public void Save(Stream stream)
        {
            this.Save(stream, base.GetSaveOptionsFromAnnotations());
        }

        public void Save(TextWriter textWriter)
        {
            this.Save(textWriter, base.GetSaveOptionsFromAnnotations());
        }

        public void Save(string fileName)
        {
            this.Save(fileName, base.GetSaveOptionsFromAnnotations());
        }

        public void Save(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            writer.WriteStartDocument();
            this.WriteTo(writer);
            writer.WriteEndDocument();
        }

        public void Save(Stream stream, SaveOptions options)
        {
            XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
            using (XmlWriter writer = XmlWriter.Create(stream, xmlWriterSettings))
            {
                this.Save(writer);
            }
        }

        public void Save(TextWriter textWriter, SaveOptions options)
        {
            XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
            using (XmlWriter writer = XmlWriter.Create(textWriter, xmlWriterSettings))
            {
                this.Save(writer);
            }
        }

        public void Save(string fileName, SaveOptions options)
        {
            XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
            using (XmlWriter writer = XmlWriter.Create(fileName, xmlWriterSettings))
            {
                this.Save(writer);
            }
        }

        public void SetAttributeValue(XName name, object value)
        {
            XAttribute a = this.Attribute(name);
            if (value == null)
            {
                if (a != null)
                {
                    this.RemoveAttribute(a);
                }
            }
            else if (a != null)
            {
                a.Value = XContainer.GetStringValue(value);
            }
            else
            {
                this.AppendAttribute(new XAttribute(name, value));
            }
        }

        public void SetElementValue(XName name, object value)
        {
            XElement n = base.Element(name);
            if (value == null)
            {
                if (n != null)
                {
                    base.RemoveNode(n);
                }
            }
            else if (n != null)
            {
                n.Value = XContainer.GetStringValue(value);
            }
            else
            {
                base.AddNode(new XElement(name, XContainer.GetStringValue(value)));
            }
        }

        internal void SetEndElementLineInfo(int lineNumber, int linePosition)
        {
            base.AddAnnotation(new LineInfoEndElementAnnotation(lineNumber, linePosition));
        }

        public void SetValue(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.Value = XContainer.GetStringValue(value);
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (((base.parent != null) || (base.annotations != null)) || ((base.content != null) || (this.lastAttr != null)))
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_DeserializeInstance"));
            }
            if (reader.MoveToContent() != XmlNodeType.Element)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExpectedNodeType", new object[] { XmlNodeType.Element, reader.NodeType }));
            }
            this.ReadElementFrom(reader, LoadOptions.None);
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            this.WriteTo(writer);
        }

        internal override void ValidateNode(XNode node, XNode previous)
        {
            if (node is XDocument)
            {
                throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_AddNode", new object[] { XmlNodeType.Document }));
            }
            if (node is XDocumentType)
            {
                throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_AddNode", new object[] { XmlNodeType.DocumentType }));
            }
        }

        public override void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            new ElementWriter(writer).WriteElement(this);
        }

        public static IEnumerable<XElement> EmptySequence
        {
            get
            {
                if (emptySequence == null)
                {
                    emptySequence = new XElement[0];
                }
                return emptySequence;
            }
        }

        public XAttribute FirstAttribute
        {
            get
            {
                if (this.lastAttr == null)
                {
                    return null;
                }
                return this.lastAttr.next;
            }
        }

        public bool HasAttributes
        {
            get
            {
                return (this.lastAttr != null);
            }
        }

        public bool HasElements
        {
            get
            {
                XNode content = base.content as XNode;
                if (content != null)
                {
                    do
                    {
                        if (content is XElement)
                        {
                            return true;
                        }
                        content = content.next;
                    }
                    while (content != base.content);
                }
                return false;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (base.content == null);
            }
        }

        public XAttribute LastAttribute
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.lastAttr;
            }
        }

        public XName Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                bool flag = base.NotifyChanging(this, XObjectChangeEventArgs.Name);
                this.name = value;
                if (flag)
                {
                    base.NotifyChanged(this, XObjectChangeEventArgs.Name);
                }
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return XmlNodeType.Element;
            }
        }

        public string Value
        {
            get
            {
                if (base.content == null)
                {
                    return string.Empty;
                }
                string content = base.content as string;
                if (content != null)
                {
                    return content;
                }
                StringBuilder sb = new StringBuilder();
                this.AppendText(sb);
                return sb.ToString();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                base.RemoveNodes();
                base.Add(value);
            }
        }

    }
}

