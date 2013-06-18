namespace System.Xml.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StreamingElementWriter
    {
        private XmlWriter writer;
        private XStreamingElement element;
        private List<XAttribute> attributes;
        private NamespaceResolver resolver;
        public StreamingElementWriter(XmlWriter w)
        {
            this.writer = w;
            this.element = null;
            this.attributes = new List<XAttribute>();
            this.resolver = new NamespaceResolver();
        }

        private void FlushElement()
        {
            if (this.element != null)
            {
                this.PushElement();
                XNamespace ns = this.element.Name.Namespace;
                this.writer.WriteStartElement(this.GetPrefixOfNamespace(ns, true), this.element.Name.LocalName, ns.NamespaceName);
                foreach (XAttribute attribute in this.attributes)
                {
                    ns = attribute.Name.Namespace;
                    string localName = attribute.Name.LocalName;
                    string namespaceName = ns.NamespaceName;
                    this.writer.WriteAttributeString(this.GetPrefixOfNamespace(ns, false), localName, ((namespaceName.Length == 0) && (localName == "xmlns")) ? "http://www.w3.org/2000/xmlns/" : namespaceName, attribute.Value);
                }
                this.element = null;
                this.attributes.Clear();
            }
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

        private void PushElement()
        {
            this.resolver.PushScope();
            foreach (XAttribute attribute in this.attributes)
            {
                if (attribute.IsNamespaceDeclaration)
                {
                    this.resolver.Add((attribute.Name.NamespaceName.Length == 0) ? string.Empty : attribute.Name.LocalName, XNamespace.Get(attribute.Value));
                }
            }
        }

        private void Write(object content)
        {
            if (content != null)
            {
                XNode n = content as XNode;
                if (n != null)
                {
                    this.WriteNode(n);
                }
                else
                {
                    string s = content as string;
                    if (s != null)
                    {
                        this.WriteString(s);
                    }
                    else
                    {
                        XAttribute a = content as XAttribute;
                        if (a != null)
                        {
                            this.WriteAttribute(a);
                        }
                        else
                        {
                            XStreamingElement e = content as XStreamingElement;
                            if (e != null)
                            {
                                this.WriteStreamingElement(e);
                            }
                            else
                            {
                                object[] objArray = content as object[];
                                if (objArray != null)
                                {
                                    foreach (object obj2 in objArray)
                                    {
                                        this.Write(obj2);
                                    }
                                }
                                else
                                {
                                    IEnumerable enumerable = content as IEnumerable;
                                    if (enumerable != null)
                                    {
                                        foreach (object obj3 in enumerable)
                                        {
                                            this.Write(obj3);
                                        }
                                    }
                                    else
                                    {
                                        this.WriteString(XContainer.GetStringValue(content));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WriteAttribute(XAttribute a)
        {
            if (this.element == null)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_WriteAttribute"));
            }
            this.attributes.Add(a);
        }

        private void WriteNode(XNode n)
        {
            this.FlushElement();
            n.WriteTo(this.writer);
        }

        internal void WriteStreamingElement(XStreamingElement e)
        {
            this.FlushElement();
            this.element = e;
            this.Write(e.content);
            bool flag = this.element == null;
            this.FlushElement();
            if (flag)
            {
                this.writer.WriteFullEndElement();
            }
            else
            {
                this.writer.WriteEndElement();
            }
            this.resolver.PopScope();
        }

        private void WriteString(string s)
        {
            this.FlushElement();
            this.writer.WriteString(s);
        }
    }
}

