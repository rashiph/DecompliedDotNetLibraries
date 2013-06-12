namespace System.Xml
{
    using System;
    using System.Xml.Schema;
    using System.Xml.XPath;

    public class XmlElement : XmlLinkedNode
    {
        private XmlAttributeCollection attributes;
        private XmlLinkedNode lastChild;
        private System.Xml.XmlName name;

        internal XmlElement(System.Xml.XmlName name, bool empty, XmlDocument doc) : base(doc)
        {
            base.parentNode = null;
            if (!doc.IsLoading)
            {
                XmlDocument.CheckName(name.Prefix);
                XmlDocument.CheckName(name.LocalName);
            }
            if (name.LocalName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("Xdom_Empty_LocalName"));
            }
            this.name = name;
            if (empty)
            {
                this.lastChild = this;
            }
        }

        protected internal XmlElement(string prefix, string localName, string namespaceURI, XmlDocument doc) : this(doc.AddXmlName(prefix, localName, namespaceURI, null), true, doc)
        {
        }

        internal override XmlNode AppendChildForLoad(XmlNode newChild, XmlDocument doc)
        {
            XmlNodeChangedEventArgs insertEventArgsForLoad = doc.GetInsertEventArgsForLoad(newChild, this);
            if (insertEventArgsForLoad != null)
            {
                doc.BeforeEvent(insertEventArgsForLoad);
            }
            XmlLinkedNode nextNode = (XmlLinkedNode) newChild;
            if ((this.lastChild == null) || (this.lastChild == this))
            {
                nextNode.next = nextNode;
                this.lastChild = nextNode;
                nextNode.SetParentForLoad(this);
            }
            else
            {
                XmlLinkedNode lastChild = this.lastChild;
                nextNode.next = lastChild.next;
                lastChild.next = nextNode;
                this.lastChild = nextNode;
                if (lastChild.IsText && nextNode.IsText)
                {
                    XmlNode.NestTextNodes(lastChild, nextNode);
                }
                else
                {
                    nextNode.SetParentForLoad(this);
                }
            }
            if (insertEventArgsForLoad != null)
            {
                doc.AfterEvent(insertEventArgsForLoad);
            }
            return nextNode;
        }

        public override XmlNode CloneNode(bool deep)
        {
            XmlDocument ownerDocument = this.OwnerDocument;
            bool isLoading = ownerDocument.IsLoading;
            ownerDocument.IsLoading = true;
            XmlElement element = ownerDocument.CreateElement(this.Prefix, this.LocalName, this.NamespaceURI);
            ownerDocument.IsLoading = isLoading;
            if (element.IsEmpty != this.IsEmpty)
            {
                element.IsEmpty = this.IsEmpty;
            }
            if (this.HasAttributes)
            {
                foreach (XmlAttribute attribute in this.Attributes)
                {
                    XmlAttribute node = (XmlAttribute) attribute.CloneNode(true);
                    if ((attribute is XmlUnspecifiedAttribute) && !attribute.Specified)
                    {
                        ((XmlUnspecifiedAttribute) node).SetSpecified(false);
                    }
                    element.Attributes.InternalAppendAttribute(node);
                }
            }
            if (deep)
            {
                element.CopyChildren(ownerDocument, this, deep);
            }
            return element;
        }

        public virtual string GetAttribute(string name)
        {
            XmlAttribute attributeNode = this.GetAttributeNode(name);
            if (attributeNode != null)
            {
                return attributeNode.Value;
            }
            return string.Empty;
        }

        public virtual string GetAttribute(string localName, string namespaceURI)
        {
            XmlAttribute attributeNode = this.GetAttributeNode(localName, namespaceURI);
            if (attributeNode != null)
            {
                return attributeNode.Value;
            }
            return string.Empty;
        }

        public virtual XmlAttribute GetAttributeNode(string name)
        {
            if (this.HasAttributes)
            {
                return this.Attributes[name];
            }
            return null;
        }

        public virtual XmlAttribute GetAttributeNode(string localName, string namespaceURI)
        {
            if (this.HasAttributes)
            {
                return this.Attributes[localName, namespaceURI];
            }
            return null;
        }

        public virtual XmlNodeList GetElementsByTagName(string name)
        {
            return new XmlElementList(this, name);
        }

        public virtual XmlNodeList GetElementsByTagName(string localName, string namespaceURI)
        {
            return new XmlElementList(this, localName, namespaceURI);
        }

        internal override string GetXPAttribute(string localName, string ns)
        {
            if (ns == this.OwnerDocument.strReservedXmlns)
            {
                return null;
            }
            XmlAttribute attributeNode = this.GetAttributeNode(localName, ns);
            if (attributeNode != null)
            {
                return attributeNode.Value;
            }
            return string.Empty;
        }

        public virtual bool HasAttribute(string name)
        {
            return (this.GetAttributeNode(name) != null);
        }

        public virtual bool HasAttribute(string localName, string namespaceURI)
        {
            return (this.GetAttributeNode(localName, namespaceURI) != null);
        }

        internal override bool IsValidChildType(XmlNodeType type)
        {
            switch (type)
            {
                case XmlNodeType.Element:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.EntityReference:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return true;
            }
            return false;
        }

        public override void RemoveAll()
        {
            base.RemoveAll();
            this.RemoveAllAttributes();
        }

        public virtual void RemoveAllAttributes()
        {
            if (this.HasAttributes)
            {
                this.attributes.RemoveAll();
            }
        }

        internal void RemoveAllChildren()
        {
            base.RemoveAll();
        }

        public virtual void RemoveAttribute(string name)
        {
            if (this.HasAttributes)
            {
                this.Attributes.RemoveNamedItem(name);
            }
        }

        public virtual void RemoveAttribute(string localName, string namespaceURI)
        {
            this.RemoveAttributeNode(localName, namespaceURI);
        }

        public virtual XmlNode RemoveAttributeAt(int i)
        {
            if (this.HasAttributes)
            {
                return this.attributes.RemoveAt(i);
            }
            return null;
        }

        public virtual XmlAttribute RemoveAttributeNode(XmlAttribute oldAttr)
        {
            if (this.HasAttributes)
            {
                return this.Attributes.Remove(oldAttr);
            }
            return null;
        }

        public virtual XmlAttribute RemoveAttributeNode(string localName, string namespaceURI)
        {
            if (this.HasAttributes)
            {
                XmlAttribute attributeNode = this.GetAttributeNode(localName, namespaceURI);
                this.Attributes.Remove(attributeNode);
                return attributeNode;
            }
            return null;
        }

        public virtual void SetAttribute(string name, string value)
        {
            XmlAttribute attributeNode = this.GetAttributeNode(name);
            if (attributeNode == null)
            {
                attributeNode = this.OwnerDocument.CreateAttribute(name);
                attributeNode.Value = value;
                this.Attributes.InternalAppendAttribute(attributeNode);
            }
            else
            {
                attributeNode.Value = value;
            }
        }

        public virtual string SetAttribute(string localName, string namespaceURI, string value)
        {
            XmlAttribute attributeNode = this.GetAttributeNode(localName, namespaceURI);
            if (attributeNode == null)
            {
                attributeNode = this.OwnerDocument.CreateAttribute(string.Empty, localName, namespaceURI);
                attributeNode.Value = value;
                this.Attributes.InternalAppendAttribute(attributeNode);
                return value;
            }
            attributeNode.Value = value;
            return value;
        }

        public virtual XmlAttribute SetAttributeNode(XmlAttribute newAttr)
        {
            if (newAttr.OwnerElement != null)
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Attr_InUse"));
            }
            return (XmlAttribute) this.Attributes.SetNamedItem(newAttr);
        }

        public virtual XmlAttribute SetAttributeNode(string localName, string namespaceURI)
        {
            XmlAttribute attributeNode = this.GetAttributeNode(localName, namespaceURI);
            if (attributeNode == null)
            {
                attributeNode = this.OwnerDocument.CreateAttribute(string.Empty, localName, namespaceURI);
                this.Attributes.InternalAppendAttribute(attributeNode);
            }
            return attributeNode;
        }

        internal override void SetParent(XmlNode node)
        {
            base.parentNode = node;
        }

        public override void WriteContentTo(XmlWriter w)
        {
            for (XmlNode node = this.FirstChild; node != null; node = node.NextSibling)
            {
                node.WriteTo(w);
            }
        }

        public override void WriteTo(XmlWriter w)
        {
            w.WriteStartElement(this.Prefix, this.LocalName, this.NamespaceURI);
            if (this.HasAttributes)
            {
                XmlAttributeCollection attributes = this.Attributes;
                for (int i = 0; i < attributes.Count; i++)
                {
                    attributes[i].WriteTo(w);
                }
            }
            if (this.IsEmpty)
            {
                w.WriteEndElement();
            }
            else
            {
                this.WriteContentTo(w);
                w.WriteFullEndElement();
            }
        }

        public override XmlAttributeCollection Attributes
        {
            get
            {
                if (this.attributes == null)
                {
                    lock (this.OwnerDocument.objLock)
                    {
                        if (this.attributes == null)
                        {
                            this.attributes = new XmlAttributeCollection(this);
                        }
                    }
                }
                return this.attributes;
            }
        }

        public virtual bool HasAttributes
        {
            get
            {
                if (this.attributes == null)
                {
                    return false;
                }
                return (this.attributes.Count > 0);
            }
        }

        public override string InnerText
        {
            get
            {
                return base.InnerText;
            }
            set
            {
                XmlLinkedNode lastNode = this.LastNode;
                if (((lastNode != null) && (lastNode.NodeType == XmlNodeType.Text)) && (lastNode.next == lastNode))
                {
                    lastNode.Value = value;
                }
                else
                {
                    this.RemoveAllChildren();
                    this.AppendChild(this.OwnerDocument.CreateTextNode(value));
                }
            }
        }

        public override string InnerXml
        {
            get
            {
                return base.InnerXml;
            }
            set
            {
                this.RemoveAllChildren();
                new XmlLoader().LoadInnerXmlElement(this, value);
            }
        }

        internal override bool IsContainer
        {
            get
            {
                return true;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (this.lastChild == this);
            }
            set
            {
                if (value)
                {
                    if (this.lastChild != this)
                    {
                        this.RemoveAllChildren();
                        this.lastChild = this;
                    }
                }
                else if (this.lastChild == this)
                {
                    this.lastChild = null;
                }
            }
        }

        internal override XmlLinkedNode LastNode
        {
            get
            {
                if (this.lastChild != this)
                {
                    return this.lastChild;
                }
                return null;
            }
            set
            {
                this.lastChild = value;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.name.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this.name.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.name.NamespaceURI;
            }
        }

        public override XmlNode NextSibling
        {
            get
            {
                if ((base.parentNode != null) && (base.parentNode.LastNode != this))
                {
                    return base.next;
                }
                return null;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return XmlNodeType.Element;
            }
        }

        public override XmlDocument OwnerDocument
        {
            get
            {
                return this.name.OwnerDocument;
            }
        }

        public override XmlNode ParentNode
        {
            get
            {
                return base.parentNode;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.name.Prefix;
            }
            set
            {
                this.name = this.name.OwnerDocument.AddXmlName(value, this.LocalName, this.NamespaceURI, this.SchemaInfo);
            }
        }

        public override IXmlSchemaInfo SchemaInfo
        {
            get
            {
                return this.name;
            }
        }

        internal System.Xml.XmlName XmlName
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        internal override string XPLocalName
        {
            get
            {
                return this.LocalName;
            }
        }

        internal override XPathNodeType XPNodeType
        {
            get
            {
                return XPathNodeType.Element;
            }
        }
    }
}

