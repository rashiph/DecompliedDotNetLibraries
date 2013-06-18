namespace System.Xml.Linq
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ElementWriter
    {
        private XmlWriter writer;
        private NamespaceResolver resolver;
        public ElementWriter(XmlWriter writer)
        {
            this.writer = writer;
            this.resolver = new NamespaceResolver();
        }

        public void WriteElement(XElement e)
        {
            this.PushAncestors(e);
            XElement element = e;
            XNode next = e;
        Label_000B:
            e = next as XElement;
            if (e != null)
            {
                this.WriteStartElement(e);
                if (e.content == null)
                {
                    this.WriteEndElement();
                    goto Label_007E;
                }
                string content = e.content as string;
                if (content != null)
                {
                    this.writer.WriteString(content);
                    this.WriteFullEndElement();
                    goto Label_007E;
                }
                next = ((XNode) e.content).next;
                goto Label_000B;
            }
            next.WriteTo(this.writer);
        Label_007E:
            while ((next != element) && (next == next.parent.content))
            {
                next = next.parent;
                this.WriteFullEndElement();
            }
            if (next == element)
            {
                return;
            }
            next = next.next;
            goto Label_000B;
        }

        private string GetPrefixOfNamespace(XNamespace ns, bool allowDefaultNamespace)
        {
            string namespaceName = ns.NamespaceName;
            if (namespaceName.Length == 0)
            {
                return string.Empty;
            }
            string prefixOfNamespace = this.resolver.GetPrefixOfNamespace(ns, allowDefaultNamespace);
            if (prefixOfNamespace != null)
            {
                return prefixOfNamespace;
            }
            switch (namespaceName)
            {
                case "http://www.w3.org/XML/1998/namespace":
                    return "xml";

                case "http://www.w3.org/2000/xmlns/":
                    return "xmlns";
            }
            return null;
        }

        private void PushAncestors(XElement e)
        {
            while (true)
            {
                e = e.parent as XElement;
                if (e == null)
                {
                    return;
                }
                XAttribute lastAttr = e.lastAttr;
                if (lastAttr != null)
                {
                    do
                    {
                        lastAttr = lastAttr.next;
                        if (lastAttr.IsNamespaceDeclaration)
                        {
                            this.resolver.AddFirst((lastAttr.Name.NamespaceName.Length == 0) ? string.Empty : lastAttr.Name.LocalName, XNamespace.Get(lastAttr.Value));
                        }
                    }
                    while (lastAttr != e.lastAttr);
                }
            }
        }

        private void PushElement(XElement e)
        {
            this.resolver.PushScope();
            XAttribute lastAttr = e.lastAttr;
            if (lastAttr != null)
            {
                do
                {
                    lastAttr = lastAttr.next;
                    if (lastAttr.IsNamespaceDeclaration)
                    {
                        this.resolver.Add((lastAttr.Name.NamespaceName.Length == 0) ? string.Empty : lastAttr.Name.LocalName, XNamespace.Get(lastAttr.Value));
                    }
                }
                while (lastAttr != e.lastAttr);
            }
        }

        private void WriteEndElement()
        {
            this.writer.WriteEndElement();
            this.resolver.PopScope();
        }

        private void WriteFullEndElement()
        {
            this.writer.WriteFullEndElement();
            this.resolver.PopScope();
        }

        private void WriteStartElement(XElement e)
        {
            this.PushElement(e);
            XNamespace ns = e.Name.Namespace;
            this.writer.WriteStartElement(this.GetPrefixOfNamespace(ns, true), e.Name.LocalName, ns.NamespaceName);
            XAttribute lastAttr = e.lastAttr;
            if (lastAttr != null)
            {
                do
                {
                    lastAttr = lastAttr.next;
                    ns = lastAttr.Name.Namespace;
                    string localName = lastAttr.Name.LocalName;
                    string namespaceName = ns.NamespaceName;
                    this.writer.WriteAttributeString(this.GetPrefixOfNamespace(ns, false), localName, ((namespaceName.Length == 0) && (localName == "xmlns")) ? "http://www.w3.org/2000/xmlns/" : namespaceName, lastAttr.Value);
                }
                while (lastAttr != e.lastAttr);
            }
        }
    }
}

