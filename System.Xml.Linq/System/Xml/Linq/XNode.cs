namespace System.Xml.Linq
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

    public abstract class XNode : XObject
    {
        private static XNodeDocumentOrderComparer documentOrderComparer;
        private static XNodeEqualityComparer equalityComparer;
        internal XNode next;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal XNode()
        {
        }

        public void AddAfterSelf(object content)
        {
            if (base.parent == null)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_MissingParent"));
            }
            new Inserter(base.parent, this).Add(content);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void AddAfterSelf(params object[] content)
        {
            this.AddAfterSelf(content);
        }

        public void AddBeforeSelf(object content)
        {
            if (base.parent == null)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_MissingParent"));
            }
            XNode anchor = (XNode) base.parent.content;
            while (anchor.next != this)
            {
                anchor = anchor.next;
            }
            if (anchor == base.parent.content)
            {
                anchor = null;
            }
            new Inserter(base.parent, anchor).Add(content);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void AddBeforeSelf(params object[] content)
        {
            this.AddBeforeSelf(content);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IEnumerable<XElement> Ancestors()
        {
            return this.GetAncestors(null, false);
        }

        public IEnumerable<XElement> Ancestors(XName name)
        {
            if (name == null)
            {
                return XElement.EmptySequence;
            }
            return this.GetAncestors(name, false);
        }

        internal virtual void AppendText(StringBuilder sb)
        {
        }

        internal abstract XNode CloneNode();
        public static int CompareDocumentOrder(XNode n1, XNode n2)
        {
            XNode next;
            if (n1 == n2)
            {
                return 0;
            }
            if (n1 == null)
            {
                return -1;
            }
            if (n2 != null)
            {
                if (n1.parent == n2.parent)
                {
                    if (n1.parent == null)
                    {
                        throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_MissingAncestor"));
                    }
                    goto Label_00CF;
                }
                int num = 0;
                XNode parent = n1;
                while (parent.parent != null)
                {
                    parent = parent.parent;
                    num++;
                }
                XNode node2 = n2;
                while (node2.parent != null)
                {
                    node2 = node2.parent;
                    num--;
                }
                if (parent != node2)
                {
                    throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_MissingAncestor"));
                }
                if (num < 0)
                {
                    do
                    {
                        n2 = n2.parent;
                        num++;
                    }
                    while (num != 0);
                    if (n1 == n2)
                    {
                        return -1;
                    }
                    goto Label_00A7;
                }
                if (num <= 0)
                {
                    goto Label_00A7;
                }
                do
                {
                    n1 = n1.parent;
                    num--;
                }
                while (num != 0);
                if (n1 != n2)
                {
                    goto Label_00A7;
                }
            }
            return 1;
        Label_00A7:
            while (n1.parent != n2.parent)
            {
                n1 = n1.parent;
                n2 = n2.parent;
            }
        Label_00CF:
            next = (XNode) n1.parent.content;
            do
            {
                next = next.next;
                if (next == n1)
                {
                    return -1;
                }
            }
            while (next != n2);
            return 1;
        }

        public XmlReader CreateReader()
        {
            return new XNodeReader(this, null);
        }

        public XmlReader CreateReader(ReaderOptions readerOptions)
        {
            return new XNodeReader(this, null, readerOptions);
        }

        internal abstract bool DeepEquals(XNode node);
        public static bool DeepEquals(XNode n1, XNode n2)
        {
            return ((n1 == n2) || (((n1 != null) && (n2 != null)) && n1.DeepEquals(n2)));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IEnumerable<XElement> ElementsAfterSelf()
        {
            return this.GetElementsAfterSelf(null);
        }

        public IEnumerable<XElement> ElementsAfterSelf(XName name)
        {
            if (name == null)
            {
                return XElement.EmptySequence;
            }
            return this.GetElementsAfterSelf(name);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IEnumerable<XElement> ElementsBeforeSelf()
        {
            return this.GetElementsBeforeSelf(null);
        }

        public IEnumerable<XElement> ElementsBeforeSelf(XName name)
        {
            if (name == null)
            {
                return XElement.EmptySequence;
            }
            return this.GetElementsBeforeSelf(name);
        }

        internal IEnumerable<XElement> GetAncestors(XName name, bool self)
        {
            for (XElement iteratorVariable0 = (self ? ((XElement) this) : ((XElement) this.parent)) as XElement; iteratorVariable0 != null; iteratorVariable0 = iteratorVariable0.parent as XElement)
            {
                if ((name == null) || (iteratorVariable0.name == name))
                {
                    yield return iteratorVariable0;
                }
            }
        }

        internal abstract int GetDeepHashCode();
        private IEnumerable<XElement> GetElementsAfterSelf(XName name)
        {
            XNode next = this;
        Label_PostSwitchInIterator:;
            while ((next.parent != null) && (next != next.parent.content))
            {
                next = next.next;
                XElement iteratorVariable1 = next as XElement;
                if ((iteratorVariable1 != null) && ((name == null) || (iteratorVariable1.name == name)))
                {
                    yield return iteratorVariable1;
                    goto Label_PostSwitchInIterator;
                }
            }
        }

        private IEnumerable<XElement> GetElementsBeforeSelf(XName name)
        {
            if (this.parent != null)
            {
                XNode content = (XNode) this.parent.content;
                do
                {
                    content = content.next;
                    if (content == this)
                    {
                        break;
                    }
                    XElement iteratorVariable1 = content as XElement;
                    if ((iteratorVariable1 != null) && ((name == null) || (iteratorVariable1.name == name)))
                    {
                        yield return iteratorVariable1;
                    }
                }
                while ((this.parent != null) && (this.parent == content.parent));
            }
        }

        internal static XmlReaderSettings GetXmlReaderSettings(LoadOptions o)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            if ((o & LoadOptions.PreserveWhitespace) == LoadOptions.None)
            {
                settings.IgnoreWhitespace = true;
            }
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.MaxCharactersFromEntities = 0x989680L;
            settings.XmlResolver = null;
            return settings;
        }

        private string GetXmlString(SaveOptions o)
        {
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                XmlWriterSettings settings = new XmlWriterSettings {
                    OmitXmlDeclaration = true
                };
                if ((o & SaveOptions.DisableFormatting) == SaveOptions.None)
                {
                    settings.Indent = true;
                }
                if ((o & SaveOptions.OmitDuplicateNamespaces) != SaveOptions.None)
                {
                    settings.NamespaceHandling |= NamespaceHandling.OmitDuplicates;
                }
                if (this is XText)
                {
                    settings.ConformanceLevel = ConformanceLevel.Fragment;
                }
                using (XmlWriter writer2 = XmlWriter.Create(writer, settings))
                {
                    XDocument document = this as XDocument;
                    if (document != null)
                    {
                        document.WriteContentTo(writer2);
                    }
                    else
                    {
                        this.WriteTo(writer2);
                    }
                }
                return writer.ToString();
            }
        }

        internal static XmlWriterSettings GetXmlWriterSettings(SaveOptions o)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            if ((o & SaveOptions.DisableFormatting) == SaveOptions.None)
            {
                settings.Indent = true;
            }
            if ((o & SaveOptions.OmitDuplicateNamespaces) != SaveOptions.None)
            {
                settings.NamespaceHandling |= NamespaceHandling.OmitDuplicates;
            }
            return settings;
        }

        public bool IsAfter(XNode node)
        {
            return (CompareDocumentOrder(this, node) > 0);
        }

        public bool IsBefore(XNode node)
        {
            return (CompareDocumentOrder(this, node) < 0);
        }

        public IEnumerable<XNode> NodesAfterSelf()
        {
            XNode next = this;
            while (true)
            {
                if ((next.parent == null) || (next == next.parent.content))
                {
                    yield break;
                }
                next = next.next;
                yield return next;
            }
        }

        public IEnumerable<XNode> NodesBeforeSelf()
        {
            if (this.parent == null)
            {
                goto Label_00A9;
            }
            XNode content = (XNode) this.parent.content;
        Label_PostSwitchInIterator:;
            content = content.next;
            if (content != this)
            {
                yield return content;
                if ((this.parent != null) && (this.parent == content.parent))
                {
                    goto Label_PostSwitchInIterator;
                }
            }
        Label_00A9:;
        }

        public static XNode ReadFrom(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (reader.ReadState != System.Xml.ReadState.Interactive)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExpectedInteractive"));
            }
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    return new XElement(reader);

                case XmlNodeType.Text:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return new XText(reader);

                case XmlNodeType.CDATA:
                    return new XCData(reader);

                case XmlNodeType.ProcessingInstruction:
                    return new XProcessingInstruction(reader);

                case XmlNodeType.Comment:
                    return new XComment(reader);

                case XmlNodeType.DocumentType:
                    return new XDocumentType(reader);
            }
            throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_UnexpectedNodeType", new object[] { reader.NodeType }));
        }

        public void Remove()
        {
            if (base.parent == null)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_MissingParent"));
            }
            base.parent.RemoveNode(this);
        }

        public void ReplaceWith(object content)
        {
            if (base.parent == null)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_MissingParent"));
            }
            XContainer parent = base.parent;
            XNode anchor = (XNode) base.parent.content;
            while (anchor.next != this)
            {
                anchor = anchor.next;
            }
            if (anchor == base.parent.content)
            {
                anchor = null;
            }
            base.parent.RemoveNode(this);
            if ((anchor != null) && (anchor.parent != parent))
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_ExternalCode"));
            }
            new Inserter(parent, anchor).Add(content);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void ReplaceWith(params object[] content)
        {
            this.ReplaceWith(content);
        }

        public override string ToString()
        {
            return this.GetXmlString(base.GetSaveOptionsFromAnnotations());
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public string ToString(SaveOptions options)
        {
            return this.GetXmlString(options);
        }

        public abstract void WriteTo(XmlWriter writer);

        public static XNodeDocumentOrderComparer DocumentOrderComparer
        {
            get
            {
                if (documentOrderComparer == null)
                {
                    documentOrderComparer = new XNodeDocumentOrderComparer();
                }
                return documentOrderComparer;
            }
        }

        public static XNodeEqualityComparer EqualityComparer
        {
            get
            {
                if (equalityComparer == null)
                {
                    equalityComparer = new XNodeEqualityComparer();
                }
                return equalityComparer;
            }
        }

        public XNode NextNode
        {
            get
            {
                if ((base.parent != null) && (this != base.parent.content))
                {
                    return this.next;
                }
                return null;
            }
        }

        public XNode PreviousNode
        {
            get
            {
                if (base.parent == null)
                {
                    return null;
                }
                XNode next = ((XNode) base.parent.content).next;
                XNode node2 = null;
                while (next != this)
                {
                    node2 = next;
                    next = next.next;
                }
                return node2;
            }
        }





    }
}

