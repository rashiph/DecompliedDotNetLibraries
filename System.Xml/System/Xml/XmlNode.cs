namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml.Schema;
    using System.Xml.XPath;

    [DebuggerDisplay("{debuggerDisplayProxy}")]
    public abstract class XmlNode : ICloneable, IEnumerable, IXPathNavigable
    {
        internal XmlNode parentNode;

        internal XmlNode()
        {
        }

        internal XmlNode(XmlDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentException(Res.GetString("Xdom_Node_Null_Doc"));
            }
            this.parentNode = doc;
        }

        internal virtual void AfterEvent(XmlNodeChangedEventArgs args)
        {
            if (args != null)
            {
                this.OwnerDocument.AfterEvent(args);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal bool AncestorNode(XmlNode node)
        {
            for (XmlNode node2 = this.ParentNode; (node2 != null) && (node2 != this); node2 = node2.ParentNode)
            {
                if (node2 == node)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual XmlNode AppendChild(XmlNode newChild)
        {
            XmlDocument ownerDocument = this.OwnerDocument;
            if (ownerDocument == null)
            {
                ownerDocument = this as XmlDocument;
            }
            if (!this.IsContainer)
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Node_Insert_Contain"));
            }
            if ((this == newChild) || this.AncestorNode(newChild))
            {
                throw new ArgumentException(Res.GetString("Xdom_Node_Insert_Child"));
            }
            if (newChild.ParentNode != null)
            {
                newChild.ParentNode.RemoveChild(newChild);
            }
            XmlDocument document2 = newChild.OwnerDocument;
            if (((document2 != null) && (document2 != ownerDocument)) && (document2 != this))
            {
                throw new ArgumentException(Res.GetString("Xdom_Node_Insert_Context"));
            }
            if (newChild.NodeType == XmlNodeType.DocumentFragment)
            {
                XmlNode nextSibling;
                XmlNode firstChild = newChild.FirstChild;
                for (XmlNode node2 = firstChild; node2 != null; node2 = nextSibling)
                {
                    nextSibling = node2.NextSibling;
                    newChild.RemoveChild(node2);
                    this.AppendChild(node2);
                }
                return firstChild;
            }
            if (!(newChild is XmlLinkedNode) || !this.IsValidChildType(newChild.NodeType))
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Node_Insert_TypeConflict"));
            }
            if (!this.CanInsertAfter(newChild, this.LastChild))
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Node_Insert_Location"));
            }
            string oldValue = newChild.Value;
            XmlNodeChangedEventArgs args = this.GetEventArgs(newChild, newChild.ParentNode, this, oldValue, oldValue, XmlNodeChangedAction.Insert);
            if (args != null)
            {
                this.BeforeEvent(args);
            }
            XmlLinkedNode lastNode = this.LastNode;
            XmlLinkedNode nextNode = (XmlLinkedNode) newChild;
            if (lastNode == null)
            {
                nextNode.next = nextNode;
                this.LastNode = nextNode;
                nextNode.SetParent(this);
            }
            else
            {
                nextNode.next = lastNode.next;
                lastNode.next = nextNode;
                this.LastNode = nextNode;
                nextNode.SetParent(this);
                if (lastNode.IsText && nextNode.IsText)
                {
                    NestTextNodes(lastNode, nextNode);
                }
            }
            if (args != null)
            {
                this.AfterEvent(args);
            }
            return nextNode;
        }

        internal virtual XmlNode AppendChildForLoad(XmlNode newChild, XmlDocument doc)
        {
            XmlNodeChangedEventArgs insertEventArgsForLoad = doc.GetInsertEventArgsForLoad(newChild, this);
            if (insertEventArgsForLoad != null)
            {
                doc.BeforeEvent(insertEventArgsForLoad);
            }
            XmlLinkedNode lastNode = this.LastNode;
            XmlLinkedNode nextNode = (XmlLinkedNode) newChild;
            if (lastNode == null)
            {
                nextNode.next = nextNode;
                this.LastNode = nextNode;
                nextNode.SetParentForLoad(this);
            }
            else
            {
                nextNode.next = lastNode.next;
                lastNode.next = nextNode;
                this.LastNode = nextNode;
                if (lastNode.IsText && nextNode.IsText)
                {
                    NestTextNodes(lastNode, nextNode);
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

        private void AppendChildText(StringBuilder builder)
        {
            for (XmlNode node = this.FirstChild; node != null; node = node.NextSibling)
            {
                if (node.FirstChild == null)
                {
                    if (((node.NodeType == XmlNodeType.Text) || (node.NodeType == XmlNodeType.CDATA)) || ((node.NodeType == XmlNodeType.Whitespace) || (node.NodeType == XmlNodeType.SignificantWhitespace)))
                    {
                        builder.Append(node.InnerText);
                    }
                }
                else
                {
                    node.AppendChildText(builder);
                }
            }
        }

        internal virtual void BeforeEvent(XmlNodeChangedEventArgs args)
        {
            if (args != null)
            {
                this.OwnerDocument.BeforeEvent(args);
            }
        }

        internal virtual bool CanInsertAfter(XmlNode newChild, XmlNode refChild)
        {
            return true;
        }

        internal virtual bool CanInsertBefore(XmlNode newChild, XmlNode refChild)
        {
            return true;
        }

        public virtual XmlNode Clone()
        {
            return this.CloneNode(true);
        }

        public abstract XmlNode CloneNode(bool deep);
        internal virtual void CopyChildren(XmlDocument doc, XmlNode container, bool deep)
        {
            for (XmlNode node = container.FirstChild; node != null; node = node.NextSibling)
            {
                this.AppendChildForLoad(node.CloneNode(deep), doc);
            }
        }

        public virtual XPathNavigator CreateNavigator()
        {
            XmlDocument document = this as XmlDocument;
            if (document != null)
            {
                return document.CreateNavigator(this);
            }
            return this.OwnerDocument.CreateNavigator(this);
        }

        internal virtual XmlNode FindChild(XmlNodeType type)
        {
            for (XmlNode node = this.FirstChild; node != null; node = node.NextSibling)
            {
                if (node.NodeType == type)
                {
                    return node;
                }
            }
            return null;
        }

        public IEnumerator GetEnumerator()
        {
            return new XmlChildEnumerator(this);
        }

        internal virtual XmlNodeChangedEventArgs GetEventArgs(XmlNode node, XmlNode oldParent, XmlNode newParent, string oldValue, string newValue, XmlNodeChangedAction action)
        {
            XmlDocument ownerDocument = this.OwnerDocument;
            if (ownerDocument == null)
            {
                return null;
            }
            if (!ownerDocument.IsLoading && (((newParent != null) && newParent.IsReadOnly) || ((oldParent != null) && oldParent.IsReadOnly)))
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Node_Modify_ReadOnly"));
            }
            return ownerDocument.GetEventArgs(node, oldParent, newParent, oldValue, newValue, action);
        }

        public virtual string GetNamespaceOfPrefix(string prefix)
        {
            string namespaceOfPrefixStrict = this.GetNamespaceOfPrefixStrict(prefix);
            if (namespaceOfPrefixStrict == null)
            {
                return string.Empty;
            }
            return namespaceOfPrefixStrict;
        }

        internal string GetNamespaceOfPrefixStrict(string prefix)
        {
            XmlDocument document = this.Document;
            if (document != null)
            {
                prefix = document.NameTable.Get(prefix);
                if (prefix == null)
                {
                    return null;
                }
                XmlNode parentNode = this;
                while (parentNode != null)
                {
                    if (parentNode.NodeType == XmlNodeType.Element)
                    {
                        XmlElement element = (XmlElement) parentNode;
                        if (element.HasAttributes)
                        {
                            XmlAttributeCollection attributes = element.Attributes;
                            if (prefix.Length == 0)
                            {
                                for (int i = 0; i < attributes.Count; i++)
                                {
                                    XmlAttribute attribute = attributes[i];
                                    if ((attribute.Prefix.Length == 0) && Ref.Equal(attribute.LocalName, document.strXmlns))
                                    {
                                        return attribute.Value;
                                    }
                                }
                            }
                            else
                            {
                                for (int j = 0; j < attributes.Count; j++)
                                {
                                    XmlAttribute attribute2 = attributes[j];
                                    if (Ref.Equal(attribute2.Prefix, document.strXmlns))
                                    {
                                        if (Ref.Equal(attribute2.LocalName, prefix))
                                        {
                                            return attribute2.Value;
                                        }
                                    }
                                    else if (Ref.Equal(attribute2.Prefix, prefix))
                                    {
                                        return attribute2.NamespaceURI;
                                    }
                                }
                            }
                        }
                        if (Ref.Equal(parentNode.Prefix, prefix))
                        {
                            return parentNode.NamespaceURI;
                        }
                        parentNode = parentNode.ParentNode;
                    }
                    else if (parentNode.NodeType == XmlNodeType.Attribute)
                    {
                        parentNode = ((XmlAttribute) parentNode).OwnerElement;
                    }
                    else
                    {
                        parentNode = parentNode.ParentNode;
                    }
                }
                if (Ref.Equal(document.strXml, prefix))
                {
                    return document.strReservedXml;
                }
                if (Ref.Equal(document.strXmlns, prefix))
                {
                    return document.strReservedXmlns;
                }
            }
            return null;
        }

        public virtual string GetPrefixOfNamespace(string namespaceURI)
        {
            string prefixOfNamespaceStrict = this.GetPrefixOfNamespaceStrict(namespaceURI);
            if (prefixOfNamespaceStrict == null)
            {
                return string.Empty;
            }
            return prefixOfNamespaceStrict;
        }

        internal string GetPrefixOfNamespaceStrict(string namespaceURI)
        {
            XmlDocument document = this.Document;
            if (document != null)
            {
                namespaceURI = document.NameTable.Add(namespaceURI);
                XmlNode parentNode = this;
                while (parentNode != null)
                {
                    if (parentNode.NodeType == XmlNodeType.Element)
                    {
                        XmlElement element = (XmlElement) parentNode;
                        if (element.HasAttributes)
                        {
                            XmlAttributeCollection attributes = element.Attributes;
                            for (int i = 0; i < attributes.Count; i++)
                            {
                                XmlAttribute attribute = attributes[i];
                                if (attribute.Prefix.Length == 0)
                                {
                                    if (Ref.Equal(attribute.LocalName, document.strXmlns) && (attribute.Value == namespaceURI))
                                    {
                                        return string.Empty;
                                    }
                                }
                                else if (Ref.Equal(attribute.Prefix, document.strXmlns))
                                {
                                    if (attribute.Value == namespaceURI)
                                    {
                                        return attribute.LocalName;
                                    }
                                }
                                else if (Ref.Equal(attribute.NamespaceURI, namespaceURI))
                                {
                                    return attribute.Prefix;
                                }
                            }
                        }
                        if (Ref.Equal(parentNode.NamespaceURI, namespaceURI))
                        {
                            return parentNode.Prefix;
                        }
                        parentNode = parentNode.ParentNode;
                    }
                    else if (parentNode.NodeType == XmlNodeType.Attribute)
                    {
                        parentNode = ((XmlAttribute) parentNode).OwnerElement;
                    }
                    else
                    {
                        parentNode = parentNode.ParentNode;
                    }
                }
                if (Ref.Equal(document.strReservedXml, namespaceURI))
                {
                    return document.strXml;
                }
                if (Ref.Equal(document.strReservedXmlns, namespaceURI))
                {
                    return document.strXmlns;
                }
            }
            return null;
        }

        internal virtual string GetXPAttribute(string localName, string namespaceURI)
        {
            return string.Empty;
        }

        internal static bool HasReadOnlyParent(XmlNode n)
        {
            while (n != null)
            {
                switch (n.NodeType)
                {
                    case XmlNodeType.Attribute:
                    {
                        n = ((XmlAttribute) n).OwnerElement;
                        continue;
                    }
                    case XmlNodeType.EntityReference:
                    case XmlNodeType.Entity:
                        return true;
                }
                n = n.ParentNode;
            }
            return false;
        }

        public virtual XmlNode InsertAfter(XmlNode newChild, XmlNode refChild)
        {
            if ((this == newChild) || this.AncestorNode(newChild))
            {
                throw new ArgumentException(Res.GetString("Xdom_Node_Insert_Child"));
            }
            if (refChild == null)
            {
                return this.PrependChild(newChild);
            }
            if (!this.IsContainer)
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Node_Insert_Contain"));
            }
            if (refChild.ParentNode != this)
            {
                throw new ArgumentException(Res.GetString("Xdom_Node_Insert_Path"));
            }
            if (newChild == refChild)
            {
                return newChild;
            }
            XmlDocument ownerDocument = newChild.OwnerDocument;
            XmlDocument document2 = this.OwnerDocument;
            if (((ownerDocument != null) && (ownerDocument != document2)) && (ownerDocument != this))
            {
                throw new ArgumentException(Res.GetString("Xdom_Node_Insert_Context"));
            }
            if (!this.CanInsertAfter(newChild, refChild))
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Node_Insert_Location"));
            }
            if (newChild.ParentNode != null)
            {
                newChild.ParentNode.RemoveChild(newChild);
            }
            if (newChild.NodeType == XmlNodeType.DocumentFragment)
            {
                XmlNode nextSibling;
                XmlNode node = refChild;
                XmlNode firstChild = newChild.FirstChild;
                for (XmlNode node3 = firstChild; node3 != null; node3 = nextSibling)
                {
                    nextSibling = node3.NextSibling;
                    newChild.RemoveChild(node3);
                    this.InsertAfter(node3, node);
                    node = node3;
                }
                return firstChild;
            }
            if (!(newChild is XmlLinkedNode) || !this.IsValidChildType(newChild.NodeType))
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Node_Insert_TypeConflict"));
            }
            XmlLinkedNode nextNode = (XmlLinkedNode) newChild;
            XmlLinkedNode prevNode = (XmlLinkedNode) refChild;
            string oldValue = newChild.Value;
            XmlNodeChangedEventArgs args = this.GetEventArgs(newChild, newChild.ParentNode, this, oldValue, oldValue, XmlNodeChangedAction.Insert);
            if (args != null)
            {
                this.BeforeEvent(args);
            }
            if (prevNode == this.LastNode)
            {
                nextNode.next = prevNode.next;
                prevNode.next = nextNode;
                this.LastNode = nextNode;
                nextNode.SetParent(this);
                if (prevNode.IsText && nextNode.IsText)
                {
                    NestTextNodes(prevNode, nextNode);
                }
            }
            else
            {
                XmlLinkedNode next = prevNode.next;
                nextNode.next = next;
                prevNode.next = nextNode;
                nextNode.SetParent(this);
                if (prevNode.IsText)
                {
                    if (nextNode.IsText)
                    {
                        NestTextNodes(prevNode, nextNode);
                        if (next.IsText)
                        {
                            NestTextNodes(nextNode, next);
                        }
                    }
                    else if (next.IsText)
                    {
                        UnnestTextNodes(prevNode, next);
                    }
                }
                else if (nextNode.IsText && next.IsText)
                {
                    NestTextNodes(nextNode, next);
                }
            }
            if (args != null)
            {
                this.AfterEvent(args);
            }
            return nextNode;
        }

        public virtual XmlNode InsertBefore(XmlNode newChild, XmlNode refChild)
        {
            if ((this == newChild) || this.AncestorNode(newChild))
            {
                throw new ArgumentException(Res.GetString("Xdom_Node_Insert_Child"));
            }
            if (refChild == null)
            {
                return this.AppendChild(newChild);
            }
            if (!this.IsContainer)
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Node_Insert_Contain"));
            }
            if (refChild.ParentNode != this)
            {
                throw new ArgumentException(Res.GetString("Xdom_Node_Insert_Path"));
            }
            if (newChild == refChild)
            {
                return newChild;
            }
            XmlDocument ownerDocument = newChild.OwnerDocument;
            XmlDocument document2 = this.OwnerDocument;
            if (((ownerDocument != null) && (ownerDocument != document2)) && (ownerDocument != this))
            {
                throw new ArgumentException(Res.GetString("Xdom_Node_Insert_Context"));
            }
            if (!this.CanInsertBefore(newChild, refChild))
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Node_Insert_Location"));
            }
            if (newChild.ParentNode != null)
            {
                newChild.ParentNode.RemoveChild(newChild);
            }
            if (newChild.NodeType == XmlNodeType.DocumentFragment)
            {
                XmlNode firstChild = newChild.FirstChild;
                XmlNode oldChild = firstChild;
                if (oldChild != null)
                {
                    newChild.RemoveChild(oldChild);
                    this.InsertBefore(oldChild, refChild);
                    this.InsertAfter(newChild, oldChild);
                }
                return firstChild;
            }
            if (!(newChild is XmlLinkedNode) || !this.IsValidChildType(newChild.NodeType))
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Node_Insert_TypeConflict"));
            }
            XmlLinkedNode prevNode = (XmlLinkedNode) newChild;
            XmlLinkedNode nextNode = (XmlLinkedNode) refChild;
            string oldValue = newChild.Value;
            XmlNodeChangedEventArgs args = this.GetEventArgs(newChild, newChild.ParentNode, this, oldValue, oldValue, XmlNodeChangedAction.Insert);
            if (args != null)
            {
                this.BeforeEvent(args);
            }
            if (nextNode == this.FirstChild)
            {
                prevNode.next = nextNode;
                this.LastNode.next = prevNode;
                prevNode.SetParent(this);
                if (prevNode.IsText && nextNode.IsText)
                {
                    NestTextNodes(prevNode, nextNode);
                }
            }
            else
            {
                XmlLinkedNode previousSibling = (XmlLinkedNode) nextNode.PreviousSibling;
                prevNode.next = nextNode;
                previousSibling.next = prevNode;
                prevNode.SetParent(this);
                if (previousSibling.IsText)
                {
                    if (prevNode.IsText)
                    {
                        NestTextNodes(previousSibling, prevNode);
                        if (nextNode.IsText)
                        {
                            NestTextNodes(prevNode, nextNode);
                        }
                    }
                    else if (nextNode.IsText)
                    {
                        UnnestTextNodes(previousSibling, nextNode);
                    }
                }
                else if (prevNode.IsText && nextNode.IsText)
                {
                    NestTextNodes(prevNode, nextNode);
                }
            }
            if (args != null)
            {
                this.AfterEvent(args);
            }
            return prevNode;
        }

        internal bool IsConnected()
        {
            XmlNode parentNode = this.ParentNode;
            while ((parentNode != null) && (parentNode.NodeType != XmlNodeType.Document))
            {
                parentNode = parentNode.ParentNode;
            }
            return (parentNode != null);
        }

        internal virtual bool IsValidChildType(XmlNodeType type)
        {
            return false;
        }

        internal static void NestTextNodes(XmlNode prevNode, XmlNode nextNode)
        {
            nextNode.parentNode = prevNode;
        }

        public virtual void Normalize()
        {
            XmlNode firstNode = null;
            XmlNode nextSibling;
            StringBuilder builder = new StringBuilder();
            for (XmlNode node2 = this.FirstChild; node2 != null; node2 = nextSibling)
            {
                nextSibling = node2.NextSibling;
                switch (node2.NodeType)
                {
                    case XmlNodeType.Element:
                        node2.Normalize();
                        break;

                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                    {
                        builder.Append(node2.Value);
                        if (this.NormalizeWinner(firstNode, node2) == firstNode)
                        {
                            this.RemoveChild(node2);
                        }
                        else
                        {
                            if (firstNode != null)
                            {
                                this.RemoveChild(firstNode);
                            }
                            firstNode = node2;
                        }
                        continue;
                    }
                }
                if (firstNode != null)
                {
                    firstNode.Value = builder.ToString();
                    firstNode = null;
                }
                builder.Remove(0, builder.Length);
            }
            if ((firstNode != null) && (builder.Length > 0))
            {
                firstNode.Value = builder.ToString();
            }
        }

        private XmlNode NormalizeWinner(XmlNode firstNode, XmlNode secondNode)
        {
            if (firstNode == null)
            {
                return secondNode;
            }
            if (firstNode.NodeType == XmlNodeType.Text)
            {
                return firstNode;
            }
            if (secondNode.NodeType == XmlNodeType.Text)
            {
                return secondNode;
            }
            if (firstNode.NodeType == XmlNodeType.SignificantWhitespace)
            {
                return firstNode;
            }
            if (secondNode.NodeType == XmlNodeType.SignificantWhitespace)
            {
                return secondNode;
            }
            if (firstNode.NodeType == XmlNodeType.Whitespace)
            {
                return firstNode;
            }
            if (secondNode.NodeType == XmlNodeType.Whitespace)
            {
                return secondNode;
            }
            return null;
        }

        public virtual XmlNode PrependChild(XmlNode newChild)
        {
            return this.InsertBefore(newChild, this.FirstChild);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual void RemoveAll()
        {
            XmlNode firstChild = this.FirstChild;
            XmlNode nextSibling = null;
            while (firstChild != null)
            {
                nextSibling = firstChild.NextSibling;
                this.RemoveChild(firstChild);
                firstChild = nextSibling;
            }
        }

        public virtual XmlNode RemoveChild(XmlNode oldChild)
        {
            if (!this.IsContainer)
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Node_Remove_Contain"));
            }
            if (oldChild.ParentNode != this)
            {
                throw new ArgumentException(Res.GetString("Xdom_Node_Remove_Child"));
            }
            XmlLinkedNode node = (XmlLinkedNode) oldChild;
            string oldValue = node.Value;
            XmlNodeChangedEventArgs args = this.GetEventArgs(node, this, null, oldValue, oldValue, XmlNodeChangedAction.Remove);
            if (args != null)
            {
                this.BeforeEvent(args);
            }
            XmlLinkedNode lastNode = this.LastNode;
            if (node == this.FirstChild)
            {
                if (node == lastNode)
                {
                    this.LastNode = null;
                    node.next = null;
                    node.SetParent(null);
                }
                else
                {
                    XmlLinkedNode next = node.next;
                    if (next.IsText && node.IsText)
                    {
                        UnnestTextNodes(node, next);
                    }
                    lastNode.next = next;
                    node.next = null;
                    node.SetParent(null);
                }
            }
            else if (node == lastNode)
            {
                XmlLinkedNode previousSibling = (XmlLinkedNode) node.PreviousSibling;
                previousSibling.next = node.next;
                this.LastNode = previousSibling;
                node.next = null;
                node.SetParent(null);
            }
            else
            {
                XmlLinkedNode prevNode = (XmlLinkedNode) node.PreviousSibling;
                XmlLinkedNode nextNode = node.next;
                if (nextNode.IsText)
                {
                    if (prevNode.IsText)
                    {
                        NestTextNodes(prevNode, nextNode);
                    }
                    else if (node.IsText)
                    {
                        UnnestTextNodes(node, nextNode);
                    }
                }
                prevNode.next = nextNode;
                node.next = null;
                node.SetParent(null);
            }
            if (args != null)
            {
                this.AfterEvent(args);
            }
            return oldChild;
        }

        public virtual XmlNode ReplaceChild(XmlNode newChild, XmlNode oldChild)
        {
            XmlNode nextSibling = oldChild.NextSibling;
            this.RemoveChild(oldChild);
            this.InsertBefore(newChild, nextSibling);
            return oldChild;
        }

        public XmlNodeList SelectNodes(string xpath)
        {
            XPathNavigator navigator = this.CreateNavigator();
            if (navigator == null)
            {
                return null;
            }
            return new XPathNodeList(navigator.Select(xpath));
        }

        public XmlNodeList SelectNodes(string xpath, XmlNamespaceManager nsmgr)
        {
            XPathNavigator navigator = this.CreateNavigator();
            if (navigator == null)
            {
                return null;
            }
            XPathExpression expr = navigator.Compile(xpath);
            expr.SetContext(nsmgr);
            return new XPathNodeList(navigator.Select(expr));
        }

        public XmlNode SelectSingleNode(string xpath)
        {
            XmlNodeList list = this.SelectNodes(xpath);
            if (list == null)
            {
                return null;
            }
            return list[0];
        }

        public XmlNode SelectSingleNode(string xpath, XmlNamespaceManager nsmgr)
        {
            XPathNavigator navigator = this.CreateNavigator();
            if (navigator == null)
            {
                return null;
            }
            XPathExpression expr = navigator.Compile(xpath);
            expr.SetContext(nsmgr);
            return new XPathNodeList(navigator.Select(expr))[0];
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal virtual void SetParent(XmlNode node)
        {
            if (node == null)
            {
                this.parentNode = this.OwnerDocument;
            }
            else
            {
                this.parentNode = node;
            }
        }

        internal virtual void SetParentForLoad(XmlNode node)
        {
            this.parentNode = node;
        }

        internal static void SplitName(string name, out string prefix, out string localName)
        {
            int index = name.IndexOf(':');
            if (((-1 == index) || (index == 0)) || ((name.Length - 1) == index))
            {
                prefix = string.Empty;
                localName = name;
            }
            else
            {
                prefix = name.Substring(0, index);
                localName = name.Substring(index + 1);
            }
        }

        public virtual bool Supports(string feature, string version)
        {
            if ((string.Compare("XML", feature, StringComparison.OrdinalIgnoreCase) != 0) || (((version != null) && !(version == "1.0")) && !(version == "2.0")))
            {
                return false;
            }
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new XmlChildEnumerator(this);
        }

        object ICloneable.Clone()
        {
            return this.CloneNode(true);
        }

        internal static void UnnestTextNodes(XmlNode prevNode, XmlNode nextNode)
        {
            nextNode.parentNode = prevNode.ParentNode;
        }

        public abstract void WriteContentTo(XmlWriter w);
        public abstract void WriteTo(XmlWriter w);

        public virtual XmlAttributeCollection Attributes
        {
            get
            {
                return null;
            }
        }

        public virtual string BaseURI
        {
            get
            {
                for (XmlNode node = this.ParentNode; node != null; node = node.ParentNode)
                {
                    switch (node.NodeType)
                    {
                        case XmlNodeType.EntityReference:
                            return ((XmlEntityReference) node).ChildBaseURI;

                        case XmlNodeType.Document:
                        case XmlNodeType.Entity:
                        case XmlNodeType.Attribute:
                            return node.BaseURI;
                    }
                }
                return string.Empty;
            }
        }

        public virtual XmlNodeList ChildNodes
        {
            get
            {
                return new XmlChildNodes(this);
            }
        }

        private object debuggerDisplayProxy
        {
            get
            {
                return new DebuggerDisplayXmlNodeProxy(this);
            }
        }

        internal XmlDocument Document
        {
            get
            {
                if (this.NodeType == XmlNodeType.Document)
                {
                    return (XmlDocument) this;
                }
                return this.OwnerDocument;
            }
        }

        public virtual XmlNode FirstChild
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                XmlLinkedNode lastNode = this.LastNode;
                if (lastNode != null)
                {
                    return lastNode.next;
                }
                return null;
            }
        }

        public virtual bool HasChildNodes
        {
            get
            {
                return (this.LastNode != null);
            }
        }

        public virtual string InnerText
        {
            get
            {
                XmlNode firstChild = this.FirstChild;
                if (firstChild == null)
                {
                    return string.Empty;
                }
                if (firstChild.NextSibling == null)
                {
                    switch (firstChild.NodeType)
                    {
                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Whitespace:
                        case XmlNodeType.SignificantWhitespace:
                            return firstChild.Value;
                    }
                }
                StringBuilder builder = new StringBuilder();
                this.AppendChildText(builder);
                return builder.ToString();
            }
            set
            {
                XmlNode firstChild = this.FirstChild;
                if (((firstChild != null) && (firstChild.NextSibling == null)) && (firstChild.NodeType == XmlNodeType.Text))
                {
                    firstChild.Value = value;
                }
                else
                {
                    this.RemoveAll();
                    this.AppendChild(this.OwnerDocument.CreateTextNode(value));
                }
            }
        }

        public virtual string InnerXml
        {
            get
            {
                StringWriter w = new StringWriter(CultureInfo.InvariantCulture);
                XmlDOMTextWriter writer2 = new XmlDOMTextWriter(w);
                try
                {
                    this.WriteContentTo(writer2);
                }
                finally
                {
                    writer2.Close();
                }
                return w.ToString();
            }
            set
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Set_InnerXml"));
            }
        }

        internal virtual bool IsContainer
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsReadOnly
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                XmlDocument ownerDocument = this.OwnerDocument;
                return HasReadOnlyParent(this);
            }
        }

        internal virtual bool IsText
        {
            get
            {
                return false;
            }
        }

        public virtual XmlElement this[string name]
        {
            get
            {
                for (XmlNode node = this.FirstChild; node != null; node = node.NextSibling)
                {
                    if ((node.NodeType == XmlNodeType.Element) && (node.Name == name))
                    {
                        return (XmlElement) node;
                    }
                }
                return null;
            }
        }

        public virtual XmlElement this[string localname, string ns]
        {
            get
            {
                for (XmlNode node = this.FirstChild; node != null; node = node.NextSibling)
                {
                    if (((node.NodeType == XmlNodeType.Element) && (node.LocalName == localname)) && (node.NamespaceURI == ns))
                    {
                        return (XmlElement) node;
                    }
                }
                return null;
            }
        }

        public virtual XmlNode LastChild
        {
            get
            {
                return this.LastNode;
            }
        }

        internal virtual XmlLinkedNode LastNode
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public abstract string LocalName { get; }

        public abstract string Name { get; }

        public virtual string NamespaceURI
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual XmlNode NextSibling
        {
            get
            {
                return null;
            }
        }

        public abstract XmlNodeType NodeType { get; }

        public virtual string OuterXml
        {
            get
            {
                StringWriter w = new StringWriter(CultureInfo.InvariantCulture);
                XmlDOMTextWriter writer2 = new XmlDOMTextWriter(w);
                try
                {
                    this.WriteTo(writer2);
                }
                finally
                {
                    writer2.Close();
                }
                return w.ToString();
            }
        }

        public virtual XmlDocument OwnerDocument
        {
            get
            {
                if (this.parentNode.NodeType == XmlNodeType.Document)
                {
                    return (XmlDocument) this.parentNode;
                }
                return this.parentNode.OwnerDocument;
            }
        }

        public virtual XmlNode ParentNode
        {
            get
            {
                if (this.parentNode.NodeType != XmlNodeType.Document)
                {
                    return this.parentNode;
                }
                XmlLinkedNode firstChild = this.parentNode.FirstChild as XmlLinkedNode;
                if (firstChild != null)
                {
                    XmlLinkedNode next = firstChild;
                    do
                    {
                        if (next == this)
                        {
                            return this.parentNode;
                        }
                        next = next.next;
                    }
                    while ((next != null) && (next != firstChild));
                }
                return null;
            }
        }

        public virtual string Prefix
        {
            get
            {
                return string.Empty;
            }
            set
            {
            }
        }

        public virtual XmlNode PreviousSibling
        {
            get
            {
                return null;
            }
        }

        internal virtual XmlNode PreviousText
        {
            get
            {
                return null;
            }
        }

        public virtual IXmlSchemaInfo SchemaInfo
        {
            get
            {
                return XmlDocument.NotKnownSchemaInfo;
            }
        }

        public virtual string Value
        {
            get
            {
                return null;
            }
            set
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Res.GetString("Xdom_Node_SetVal"), new object[] { this.NodeType.ToString() }));
            }
        }

        internal virtual string XmlLang
        {
            get
            {
                XmlNode parentNode = this;
                XmlElement element = null;
                do
                {
                    element = parentNode as XmlElement;
                    if ((element != null) && element.HasAttribute("xml:lang"))
                    {
                        return element.GetAttribute("xml:lang");
                    }
                    parentNode = parentNode.ParentNode;
                }
                while (parentNode != null);
                return string.Empty;
            }
        }

        internal virtual System.Xml.XmlSpace XmlSpace
        {
            get
            {
                XmlNode parentNode = this;
                XmlElement element = null;
                do
                {
                    string str;
                    element = parentNode as XmlElement;
                    if (((element != null) && element.HasAttribute("xml:space")) && ((str = XmlConvert.TrimString(element.GetAttribute("xml:space"))) != null))
                    {
                        if (str == "default")
                        {
                            return System.Xml.XmlSpace.Default;
                        }
                        if (str == "preserve")
                        {
                            return System.Xml.XmlSpace.Preserve;
                        }
                    }
                    parentNode = parentNode.ParentNode;
                }
                while (parentNode != null);
                return System.Xml.XmlSpace.None;
            }
        }

        internal virtual string XPLocalName
        {
            get
            {
                return string.Empty;
            }
        }

        internal virtual XPathNodeType XPNodeType
        {
            get
            {
                return ~XPathNodeType.Root;
            }
        }
    }
}

