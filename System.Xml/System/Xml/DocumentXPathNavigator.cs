namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml.Schema;
    using System.Xml.XPath;

    internal sealed class DocumentXPathNavigator : XPathNavigator, IHasXmlNode
    {
        private int attributeIndex;
        private XmlDocument document;
        private XmlElement namespaceParent;
        private XmlNode source;

        public DocumentXPathNavigator(DocumentXPathNavigator other)
        {
            this.document = other.document;
            this.source = other.source;
            this.attributeIndex = other.attributeIndex;
            this.namespaceParent = other.namespaceParent;
        }

        public DocumentXPathNavigator(XmlDocument document, XmlNode node)
        {
            this.document = document;
            this.ResetPosition(node);
        }

        public override XmlWriter AppendChild()
        {
            switch (this.source.NodeType)
            {
                case XmlNodeType.Document:
                case XmlNodeType.DocumentFragment:
                case XmlNodeType.Element:
                {
                    DocumentXmlWriter writer = new DocumentXmlWriter(DocumentXmlWriterType.AppendChild, this.source, this.document) {
                        NamespaceManager = GetNamespaceManager(this.source, this.document)
                    };
                    return new XmlWellFormedWriter(writer, writer.Settings);
                }
            }
            throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
        }

        private void CalibrateText()
        {
            for (XmlNode node = this.PreviousText(this.source); node != null; node = this.PreviousText(node))
            {
                this.ResetPosition(node);
            }
        }

        private static bool CheckAttributePosition(XmlAttribute attribute, out XmlAttributeCollection attributes, int index)
        {
            XmlElement ownerElement = attribute.OwnerElement;
            if (ownerElement != null)
            {
                attributes = ownerElement.Attributes;
                if (((index >= 0) && (index < attributes.Count)) && (attribute == attributes[index]))
                {
                    return true;
                }
            }
            else
            {
                attributes = null;
            }
            return false;
        }

        public override bool CheckValidity(XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
        {
            XmlDocument source;
            if (this.source.NodeType == XmlNodeType.Document)
            {
                source = (XmlDocument) this.source;
            }
            else
            {
                source = this.source.OwnerDocument;
                if (schemas != null)
                {
                    throw new ArgumentException(Res.GetString("XPathDocument_SchemaSetNotAllowed", (object[]) null));
                }
            }
            if ((schemas == null) && (source != null))
            {
                schemas = source.Schemas;
            }
            if ((schemas == null) || (schemas.Count == 0))
            {
                throw new InvalidOperationException(Res.GetString("XmlDocument_NoSchemaInfo"));
            }
            DocumentSchemaValidator validator = new DocumentSchemaValidator(source, schemas, validationEventHandler) {
                PsviAugmentation = false
            };
            return validator.Validate(this.source);
        }

        public override XPathNavigator Clone()
        {
            return new DocumentXPathNavigator(this);
        }

        private XmlNodeOrder Compare(XmlNode node1, XmlNode node2)
        {
            if (node1.XPNodeType == XPathNodeType.Attribute)
            {
                if (node2.XPNodeType != XPathNodeType.Attribute)
                {
                    return XmlNodeOrder.Before;
                }
                XmlElement ownerElement = ((XmlAttribute) node1).OwnerElement;
                if (ownerElement.HasAttributes)
                {
                    XmlAttributeCollection attributes = ownerElement.Attributes;
                    for (int i = 0; i < attributes.Count; i++)
                    {
                        XmlAttribute attribute = attributes[i];
                        if (attribute == node1)
                        {
                            return XmlNodeOrder.Before;
                        }
                        if (attribute == node2)
                        {
                            return XmlNodeOrder.After;
                        }
                    }
                }
                return XmlNodeOrder.Unknown;
            }
            if (node2.XPNodeType == XPathNodeType.Attribute)
            {
                return XmlNodeOrder.After;
            }
            XmlNode nextSibling = node1.NextSibling;
            while ((nextSibling != null) && (nextSibling != node2))
            {
                nextSibling = nextSibling.NextSibling;
            }
            if (nextSibling == null)
            {
                return XmlNodeOrder.After;
            }
            return XmlNodeOrder.Before;
        }

        public override XmlNodeOrder ComparePosition(XPathNavigator other)
        {
            DocumentXPathNavigator navigator = other as DocumentXPathNavigator;
            if (navigator != null)
            {
                this.CalibrateText();
                navigator.CalibrateText();
                if ((this.source == navigator.source) && (this.namespaceParent == navigator.namespaceParent))
                {
                    return XmlNodeOrder.Same;
                }
                if ((this.namespaceParent != null) || (navigator.namespaceParent != null))
                {
                    return base.ComparePosition(other);
                }
                XmlNode source = this.source;
                XmlNode node = navigator.source;
                XmlNode node3 = OwnerNode(source);
                XmlNode node4 = OwnerNode(node);
                if (node3 == node4)
                {
                    if (node3 == null)
                    {
                        return XmlNodeOrder.Unknown;
                    }
                    return this.Compare(source, node);
                }
                int depth = GetDepth(source);
                int num2 = GetDepth(node);
                if (num2 > depth)
                {
                    while ((node != null) && (num2 > depth))
                    {
                        node = OwnerNode(node);
                        num2--;
                    }
                    if (source == node)
                    {
                        return XmlNodeOrder.Before;
                    }
                    node4 = OwnerNode(node);
                }
                else if (depth > num2)
                {
                    while ((source != null) && (depth > num2))
                    {
                        source = OwnerNode(source);
                        depth--;
                    }
                    if (source == node)
                    {
                        return XmlNodeOrder.After;
                    }
                    node3 = OwnerNode(source);
                }
                while ((node3 != null) && (node4 != null))
                {
                    if (node3 == node4)
                    {
                        return this.Compare(source, node);
                    }
                    source = node3;
                    node = node4;
                    node3 = OwnerNode(source);
                    node4 = OwnerNode(node);
                }
            }
            return XmlNodeOrder.Unknown;
        }

        public override XmlWriter CreateAttributes()
        {
            if (this.source.NodeType != XmlNodeType.Element)
            {
                throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
            }
            DocumentXmlWriter writer = new DocumentXmlWriter(DocumentXmlWriterType.AppendAttribute, this.source, this.document) {
                NamespaceManager = GetNamespaceManager(this.source, this.document)
            };
            return new XmlWellFormedWriter(writer, writer.Settings);
        }

        private static void DeleteAttribute(XmlAttribute attribute, int index)
        {
            XmlAttributeCollection attributes;
            if (!CheckAttributePosition(attribute, out attributes, index) && !ResetAttributePosition(attribute, attributes, out index))
            {
                throw new InvalidOperationException(Res.GetString("Xpn_MissingParent"));
            }
            if (attribute.IsReadOnly)
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Node_Modify_ReadOnly"));
            }
            attributes.RemoveAt(index);
        }

        public override void DeleteRange(XPathNavigator lastSiblingToDelete)
        {
            XmlNode node3;
            DocumentXPathNavigator navigator = lastSiblingToDelete as DocumentXPathNavigator;
            if (navigator == null)
            {
                if (lastSiblingToDelete == null)
                {
                    throw new ArgumentNullException("lastSiblingToDelete");
                }
                throw new NotSupportedException();
            }
            this.CalibrateText();
            navigator.CalibrateText();
            XmlNode source = this.source;
            XmlNode node = navigator.source;
            if (source != node)
            {
                if (node.IsText)
                {
                    node = navigator.TextEnd(node);
                }
                if (!IsFollowingSibling(source, node))
                {
                    throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
                }
                XmlNode node4 = OwnerNode(source);
                DeleteToFollowingSibling(source, node);
                if (node4 != null)
                {
                    this.ResetPosition(node4);
                }
                return;
            }
            switch (source.NodeType)
            {
                case XmlNodeType.Element:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                    break;

                case XmlNodeType.Attribute:
                {
                    XmlAttribute attribute = (XmlAttribute) source;
                    if (attribute.IsNamespace)
                    {
                        goto Label_00E1;
                    }
                    node3 = OwnerNode(attribute);
                    DeleteAttribute(attribute, this.attributeIndex);
                    if (node3 != null)
                    {
                        this.ResetPosition(node3);
                    }
                    return;
                }
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    node = navigator.TextEnd(node);
                    break;

                default:
                    goto Label_00E1;
            }
            node3 = OwnerNode(source);
            DeleteToFollowingSibling(source, node);
            if (node3 != null)
            {
                this.ResetPosition(node3);
            }
            return;
        Label_00E1:
            throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
        }

        public override void DeleteSelf()
        {
            XmlNode node3;
            XmlNode source = this.source;
            XmlNode end = source;
            switch (source.NodeType)
            {
                case XmlNodeType.Element:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                    break;

                case XmlNodeType.Attribute:
                {
                    XmlAttribute node = (XmlAttribute) source;
                    if (node.IsNamespace)
                    {
                        goto Label_00AF;
                    }
                    node3 = OwnerNode(node);
                    DeleteAttribute(node, this.attributeIndex);
                    if (node3 != null)
                    {
                        this.ResetPosition(node3);
                    }
                    return;
                }
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    this.CalibrateText();
                    source = this.source;
                    end = this.TextEnd(source);
                    break;

                default:
                    goto Label_00AF;
            }
            node3 = OwnerNode(source);
            DeleteToFollowingSibling(source, end);
            if (node3 != null)
            {
                this.ResetPosition(node3);
            }
            return;
        Label_00AF:
            throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
        }

        internal static void DeleteToFollowingSibling(XmlNode node, XmlNode end)
        {
            XmlNode parentNode = node.ParentNode;
            if (parentNode == null)
            {
                throw new InvalidOperationException(Res.GetString("Xpn_MissingParent"));
            }
            if (node.IsReadOnly || end.IsReadOnly)
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Node_Modify_ReadOnly"));
            }
            while (node != end)
            {
                XmlNode oldChild = node;
                node = node.NextSibling;
                parentNode.RemoveChild(oldChild);
            }
            parentNode.RemoveChild(node);
        }

        private XmlNode FirstChild(XmlNode node)
        {
            XmlNode firstChild = node.FirstChild;
            if (!this.document.HasEntityReferences)
            {
                return firstChild;
            }
            return this.FirstChildTail(firstChild);
        }

        private XmlNode FirstChildTail(XmlNode child)
        {
            while ((child != null) && (child.NodeType == XmlNodeType.EntityReference))
            {
                child = child.FirstChild;
            }
            return child;
        }

        public override string GetAttribute(string localName, string namespaceURI)
        {
            return this.source.GetXPAttribute(localName, namespaceURI);
        }

        private static int GetDepth(XmlNode node)
        {
            int num = 0;
            for (XmlNode node2 = OwnerNode(node); node2 != null; node2 = OwnerNode(node2))
            {
                num++;
            }
            return num;
        }

        public override string GetNamespace(string name)
        {
            XmlNode source = this.source;
            while ((source != null) && (source.NodeType != XmlNodeType.Element))
            {
                XmlAttribute attribute = source as XmlAttribute;
                if (attribute != null)
                {
                    source = attribute.OwnerElement;
                }
                else
                {
                    source = source.ParentNode;
                }
            }
            XmlElement parentNode = source as XmlElement;
            if (parentNode != null)
            {
                string strXmlns;
                if ((name != null) && (name.Length != 0))
                {
                    strXmlns = name;
                }
                else
                {
                    strXmlns = this.document.strXmlns;
                }
                string strReservedXmlns = this.document.strReservedXmlns;
                do
                {
                    XmlAttribute attributeNode = parentNode.GetAttributeNode(strXmlns, strReservedXmlns);
                    if (attributeNode != null)
                    {
                        return attributeNode.Value;
                    }
                    parentNode = parentNode.ParentNode as XmlElement;
                }
                while (parentNode != null);
            }
            if (name == this.document.strXml)
            {
                return this.document.strReservedXml;
            }
            if (name == this.document.strXmlns)
            {
                return this.document.strReservedXmlns;
            }
            return string.Empty;
        }

        private static XmlNamespaceManager GetNamespaceManager(XmlNode node, XmlDocument document)
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(document.NameTable);
            List<XmlElement> list = new List<XmlElement>();
            while (node != null)
            {
                XmlElement item = node as XmlElement;
                if ((item != null) && item.HasAttributes)
                {
                    list.Add(item);
                }
                node = node.ParentNode;
            }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                manager.PushScope();
                XmlAttributeCollection attributes = list[i].Attributes;
                for (int j = 0; j < attributes.Count; j++)
                {
                    XmlAttribute attribute = attributes[j];
                    if (attribute.IsNamespace)
                    {
                        string prefix = (attribute.Prefix.Length == 0) ? string.Empty : attribute.LocalName;
                        manager.AddNamespace(prefix, attribute.Value);
                    }
                }
            }
            return manager;
        }

        public override XmlWriter InsertAfter()
        {
            XmlNode source = this.source;
            switch (source.NodeType)
            {
                case XmlNodeType.Attribute:
                case XmlNodeType.Document:
                case XmlNodeType.DocumentFragment:
                    throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));

                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    source = this.TextEnd(source);
                    break;
            }
            DocumentXmlWriter writer = new DocumentXmlWriter(DocumentXmlWriterType.InsertSiblingAfter, source, this.document) {
                NamespaceManager = GetNamespaceManager(source.ParentNode, this.document)
            };
            return new XmlWellFormedWriter(writer, writer.Settings);
        }

        public override XmlWriter InsertBefore()
        {
            switch (this.source.NodeType)
            {
                case XmlNodeType.Attribute:
                case XmlNodeType.Document:
                case XmlNodeType.DocumentFragment:
                    throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));

                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    this.CalibrateText();
                    break;
            }
            DocumentXmlWriter writer = new DocumentXmlWriter(DocumentXmlWriterType.InsertSiblingBefore, this.source, this.document) {
                NamespaceManager = GetNamespaceManager(this.source.ParentNode, this.document)
            };
            return new XmlWellFormedWriter(writer, writer.Settings);
        }

        public override bool IsDescendant(XPathNavigator other)
        {
            DocumentXPathNavigator navigator = other as DocumentXPathNavigator;
            return ((navigator != null) && IsDescendant(this.source, navigator.source));
        }

        private static bool IsDescendant(XmlNode top, XmlNode bottom)
        {
            do
            {
                XmlNode parentNode = bottom.ParentNode;
                if (parentNode == null)
                {
                    XmlAttribute attribute = bottom as XmlAttribute;
                    if (attribute == null)
                    {
                        goto Label_0027;
                    }
                    parentNode = attribute.OwnerElement;
                    if (parentNode == null)
                    {
                        goto Label_0027;
                    }
                }
                bottom = parentNode;
            }
            while (top != bottom);
            return true;
        Label_0027:
            return false;
        }

        internal static bool IsFollowingSibling(XmlNode left, XmlNode right)
        {
            do
            {
                left = left.NextSibling;
                if (left == null)
                {
                    return false;
                }
            }
            while (left != right);
            return true;
        }

        public override bool IsSamePosition(XPathNavigator other)
        {
            DocumentXPathNavigator navigator = other as DocumentXPathNavigator;
            if (navigator == null)
            {
                return false;
            }
            this.CalibrateText();
            navigator.CalibrateText();
            return ((this.source == navigator.source) && (this.namespaceParent == navigator.namespaceParent));
        }

        private static bool IsValidChild(XmlNode parent, XmlNode child)
        {
            switch (parent.NodeType)
            {
                case XmlNodeType.Document:
                    switch (child.NodeType)
                    {
                        case XmlNodeType.ProcessingInstruction:
                        case XmlNodeType.Comment:
                        case XmlNodeType.Element:
                            return true;
                    }
                    break;

                case XmlNodeType.DocumentFragment:
                    switch (child.NodeType)
                    {
                        case XmlNodeType.Element:
                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                        case XmlNodeType.ProcessingInstruction:
                        case XmlNodeType.Comment:
                        case XmlNodeType.Whitespace:
                        case XmlNodeType.SignificantWhitespace:
                            return true;
                    }
                    break;

                case XmlNodeType.Element:
                    return true;
            }
            return false;
        }

        public override string LookupNamespace(string prefix)
        {
            string array = base.LookupNamespace(prefix);
            if (array != null)
            {
                array = this.NameTable.Add(array);
            }
            return array;
        }

        public override bool MoveTo(XPathNavigator other)
        {
            DocumentXPathNavigator navigator = other as DocumentXPathNavigator;
            if ((navigator != null) && (this.document == navigator.document))
            {
                this.source = navigator.source;
                this.attributeIndex = navigator.attributeIndex;
                this.namespaceParent = navigator.namespaceParent;
                return true;
            }
            return false;
        }

        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            XmlElement source = this.source as XmlElement;
            if ((source != null) && source.HasAttributes)
            {
                XmlAttributeCollection attributes = source.Attributes;
                for (int i = 0; i < attributes.Count; i++)
                {
                    XmlAttribute attribute = attributes[i];
                    if ((attribute.LocalName == localName) && (attribute.NamespaceURI == namespaceURI))
                    {
                        if (!attribute.IsNamespace)
                        {
                            this.source = attribute;
                            this.attributeIndex = i;
                            return true;
                        }
                        return false;
                    }
                }
            }
            return false;
        }

        public override bool MoveToChild(XPathNodeType type)
        {
            if (this.source.NodeType != XmlNodeType.Attribute)
            {
                XmlNode node = this.FirstChild(this.source);
                if (node != null)
                {
                    int contentKindMask = XPathNavigator.GetContentKindMask(type);
                    if (contentKindMask == 0)
                    {
                        return false;
                    }
                    do
                    {
                        if (((((int) 1) << node.XPNodeType) & contentKindMask) != 0)
                        {
                            this.source = node;
                            return true;
                        }
                        node = this.NextSibling(node);
                    }
                    while (node != null);
                }
            }
            return false;
        }

        public override bool MoveToChild(string localName, string namespaceUri)
        {
            if (this.source.NodeType != XmlNodeType.Attribute)
            {
                for (XmlNode node = this.FirstChild(this.source); node != null; node = this.NextSibling(node))
                {
                    if (((node.NodeType == XmlNodeType.Element) && (node.LocalName == localName)) && (node.NamespaceURI == namespaceUri))
                    {
                        this.source = node;
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool MoveToFirst()
        {
            if (this.source.NodeType == XmlNodeType.Attribute)
            {
                return false;
            }
            XmlNode node = this.ParentNode(this.source);
            if (node == null)
            {
                return false;
            }
            XmlNode child = this.FirstChild(node);
            while (!IsValidChild(node, child))
            {
                child = this.NextSibling(child);
                if (child == null)
                {
                    return false;
                }
            }
            this.source = child;
            return true;
        }

        public override bool MoveToFirstAttribute()
        {
            XmlElement source = this.source as XmlElement;
            if ((source != null) && source.HasAttributes)
            {
                XmlAttributeCollection attributes = source.Attributes;
                for (int i = 0; i < attributes.Count; i++)
                {
                    XmlAttribute attribute = attributes[i];
                    if (!attribute.IsNamespace)
                    {
                        this.source = attribute;
                        this.attributeIndex = i;
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool MoveToFirstChild()
        {
            XmlNode node;
            switch (this.source.NodeType)
            {
                case XmlNodeType.Document:
                case XmlNodeType.DocumentFragment:
                    node = this.FirstChild(this.source);
                    if (node != null)
                    {
                        while (!IsValidChild(this.source, node))
                        {
                            node = this.NextSibling(node);
                            if (node == null)
                            {
                                return false;
                            }
                        }
                        break;
                    }
                    return false;

                case XmlNodeType.Element:
                    node = this.FirstChild(this.source);
                    if (node != null)
                    {
                        break;
                    }
                    return false;

                default:
                    return false;
            }
            this.source = node;
            return true;
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope scope)
        {
            XmlElement source = this.source as XmlElement;
            if (source != null)
            {
                XmlAttributeCollection attributes;
                int index = 0x7fffffff;
                switch (scope)
                {
                    case XPathNamespaceScope.All:
                        attributes = source.Attributes;
                        if (MoveToFirstNamespaceGlobal(ref attributes, ref index))
                        {
                            this.source = attributes[index];
                            this.attributeIndex = index;
                        }
                        else
                        {
                            this.source = this.document.NamespaceXml;
                        }
                        this.namespaceParent = source;
                        goto Label_0115;

                    case XPathNamespaceScope.ExcludeXml:
                        attributes = source.Attributes;
                        if (MoveToFirstNamespaceGlobal(ref attributes, ref index))
                        {
                            XmlAttribute attribute = attributes[index];
                            while (Ref.Equal(attribute.LocalName, this.document.strXml))
                            {
                                if (!MoveToNextNamespaceGlobal(ref attributes, ref index))
                                {
                                    return false;
                                }
                                attribute = attributes[index];
                            }
                            this.source = attribute;
                            this.attributeIndex = index;
                            this.namespaceParent = source;
                            goto Label_0115;
                        }
                        return false;

                    case XPathNamespaceScope.Local:
                        if (source.HasAttributes)
                        {
                            attributes = source.Attributes;
                            if (!MoveToFirstNamespaceLocal(attributes, ref index))
                            {
                                return false;
                            }
                            this.source = attributes[index];
                            this.attributeIndex = index;
                            this.namespaceParent = source;
                            goto Label_0115;
                        }
                        return false;
                }
            }
            return false;
        Label_0115:
            return true;
        }

        private static bool MoveToFirstNamespaceGlobal(ref XmlAttributeCollection attributes, ref int index)
        {
            if (MoveToFirstNamespaceLocal(attributes, ref index))
            {
                return true;
            }
            for (XmlElement element = attributes.parent.ParentNode as XmlElement; element != null; element = element.ParentNode as XmlElement)
            {
                if (element.HasAttributes)
                {
                    attributes = element.Attributes;
                    if (MoveToFirstNamespaceLocal(attributes, ref index))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool MoveToFirstNamespaceLocal(XmlAttributeCollection attributes, ref int index)
        {
            for (int i = attributes.Count - 1; i >= 0; i--)
            {
                XmlAttribute attribute = attributes[i];
                if (attribute.IsNamespace)
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToFollowing(XPathNodeType type, XPathNavigator end)
        {
            XmlNode node = null;
            XmlNode node3;
            XmlNode node4;
            DocumentXPathNavigator navigator = end as DocumentXPathNavigator;
            if (navigator != null)
            {
                if (this.document != navigator.document)
                {
                    return false;
                }
                if (navigator.source.NodeType == XmlNodeType.Attribute)
                {
                    navigator = (DocumentXPathNavigator) navigator.Clone();
                    if (!navigator.MoveToNonDescendant())
                    {
                        return false;
                    }
                }
                node = navigator.source;
            }
            int contentKindMask = XPathNavigator.GetContentKindMask(type);
            if (contentKindMask == 0)
            {
                return false;
            }
            XmlNode source = this.source;
            switch (source.NodeType)
            {
                case XmlNodeType.Attribute:
                    source = ((XmlAttribute) source).OwnerElement;
                    if (source != null)
                    {
                        break;
                    }
                    return false;

                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    source = this.TextEnd(source);
                    break;
            }
        Label_00A8:
            node3 = source.FirstChild;
            if (node3 != null)
            {
                source = node3;
                goto Label_00DD;
            }
        Label_00B9:
            node4 = source.NextSibling;
            if (node4 != null)
            {
                source = node4;
            }
            else
            {
                XmlNode parentNode = source.ParentNode;
                if (parentNode != null)
                {
                    source = parentNode;
                    goto Label_00B9;
                }
                return false;
            }
        Label_00DD:
            if (source == node)
            {
                return false;
            }
            if (((((int) 1) << source.XPNodeType) & contentKindMask) == 0)
            {
                goto Label_00A8;
            }
            this.source = source;
            return true;
        }

        public override bool MoveToFollowing(string localName, string namespaceUri, XPathNavigator end)
        {
            XmlNode node = null;
            XmlNode node3;
            XmlNode node4;
            DocumentXPathNavigator navigator = end as DocumentXPathNavigator;
            if (navigator != null)
            {
                if (this.document != navigator.document)
                {
                    return false;
                }
                if (navigator.source.NodeType == XmlNodeType.Attribute)
                {
                    navigator = (DocumentXPathNavigator) navigator.Clone();
                    if (!navigator.MoveToNonDescendant())
                    {
                        return false;
                    }
                }
                node = navigator.source;
            }
            XmlNode source = this.source;
            if (source.NodeType == XmlNodeType.Attribute)
            {
                source = ((XmlAttribute) source).OwnerElement;
                if (source == null)
                {
                    return false;
                }
            }
        Label_006C:
            node3 = source.FirstChild;
            if (node3 != null)
            {
                source = node3;
                goto Label_009E;
            }
        Label_007A:
            node4 = source.NextSibling;
            if (node4 != null)
            {
                source = node4;
            }
            else
            {
                XmlNode parentNode = source.ParentNode;
                if (parentNode != null)
                {
                    source = parentNode;
                    goto Label_007A;
                }
                return false;
            }
        Label_009E:
            if (source == node)
            {
                return false;
            }
            if (((source.NodeType != XmlNodeType.Element) || (source.LocalName != localName)) || (source.NamespaceURI != namespaceUri))
            {
                goto Label_006C;
            }
            this.source = source;
            return true;
        }

        public override bool MoveToId(string id)
        {
            XmlElement elementById = this.document.GetElementById(id);
            if (elementById != null)
            {
                this.source = elementById;
                this.namespaceParent = null;
                return true;
            }
            return false;
        }

        public override bool MoveToNamespace(string name)
        {
            if (name != this.document.strXmlns)
            {
                XmlElement source = this.source as XmlElement;
                if (source != null)
                {
                    string strXmlns;
                    if ((name != null) && (name.Length != 0))
                    {
                        strXmlns = name;
                    }
                    else
                    {
                        strXmlns = this.document.strXmlns;
                    }
                    string strReservedXmlns = this.document.strReservedXmlns;
                    do
                    {
                        XmlAttribute attributeNode = source.GetAttributeNode(strXmlns, strReservedXmlns);
                        if (attributeNode != null)
                        {
                            this.namespaceParent = (XmlElement) this.source;
                            this.source = attributeNode;
                            return true;
                        }
                        source = source.ParentNode as XmlElement;
                    }
                    while (source != null);
                    if (name == this.document.strXml)
                    {
                        this.namespaceParent = (XmlElement) this.source;
                        this.source = this.document.NamespaceXml;
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool MoveToNext()
        {
            XmlNode node = this.NextSibling(this.source);
            if (node == null)
            {
                return false;
            }
            if (node.IsText && this.source.IsText)
            {
                node = this.NextSibling(this.TextEnd(node));
                if (node == null)
                {
                    return false;
                }
            }
            XmlNode parent = this.ParentNode(node);
            while (!IsValidChild(parent, node))
            {
                node = this.NextSibling(node);
                if (node == null)
                {
                    return false;
                }
            }
            this.source = node;
            return true;
        }

        public override bool MoveToNext(XPathNodeType type)
        {
            XmlNode node = this.NextSibling(this.source);
            if (node != null)
            {
                if (node.IsText && this.source.IsText)
                {
                    node = this.NextSibling(this.TextEnd(node));
                    if (node == null)
                    {
                        return false;
                    }
                }
                int contentKindMask = XPathNavigator.GetContentKindMask(type);
                if (contentKindMask == 0)
                {
                    return false;
                }
                do
                {
                    if (((((int) 1) << node.XPNodeType) & contentKindMask) != 0)
                    {
                        this.source = node;
                        return true;
                    }
                    node = this.NextSibling(node);
                }
                while (node != null);
            }
            return false;
        }

        public override bool MoveToNext(string localName, string namespaceUri)
        {
            for (XmlNode node = this.NextSibling(this.source); node != null; node = this.NextSibling(node))
            {
                if (((node.NodeType == XmlNodeType.Element) && (node.LocalName == localName)) && (node.NamespaceURI == namespaceUri))
                {
                    this.source = node;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToNextAttribute()
        {
            XmlAttributeCollection attributes;
            XmlAttribute source = this.source as XmlAttribute;
            if (((source != null) && !source.IsNamespace) && (CheckAttributePosition(source, out attributes, this.attributeIndex) || ResetAttributePosition(source, attributes, out this.attributeIndex)))
            {
                for (int i = this.attributeIndex + 1; i < attributes.Count; i++)
                {
                    source = attributes[i];
                    if (!source.IsNamespace)
                    {
                        this.source = source;
                        this.attributeIndex = i;
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope scope)
        {
            XmlAttribute source = this.source as XmlAttribute;
            if ((source != null) && source.IsNamespace)
            {
                XmlAttributeCollection attributes;
                int attributeIndex = this.attributeIndex;
                if (CheckAttributePosition(source, out attributes, attributeIndex) || ResetAttributePosition(source, attributes, out attributeIndex))
                {
                    switch (scope)
                    {
                        case XPathNamespaceScope.All:
                            do
                            {
                                if (!MoveToNextNamespaceGlobal(ref attributes, ref attributeIndex))
                                {
                                    if (this.PathHasDuplicateNamespace(null, this.namespaceParent, this.document.strXml))
                                    {
                                        return false;
                                    }
                                    this.source = this.document.NamespaceXml;
                                    return true;
                                }
                                source = attributes[attributeIndex];
                            }
                            while (this.PathHasDuplicateNamespace(source.OwnerElement, this.namespaceParent, source.LocalName));
                            this.source = source;
                            this.attributeIndex = attributeIndex;
                            goto Label_014A;

                        case XPathNamespaceScope.ExcludeXml:
                            string localName;
                            do
                            {
                                if (!MoveToNextNamespaceGlobal(ref attributes, ref attributeIndex))
                                {
                                    return false;
                                }
                                source = attributes[attributeIndex];
                                localName = source.LocalName;
                            }
                            while (this.PathHasDuplicateNamespace(source.OwnerElement, this.namespaceParent, localName) || Ref.Equal(localName, this.document.strXml));
                            this.source = source;
                            this.attributeIndex = attributeIndex;
                            goto Label_014A;

                        case XPathNamespaceScope.Local:
                            if (source.OwnerElement == this.namespaceParent)
                            {
                                if (!MoveToNextNamespaceLocal(attributes, ref attributeIndex))
                                {
                                    return false;
                                }
                                this.source = attributes[attributeIndex];
                                this.attributeIndex = attributeIndex;
                                goto Label_014A;
                            }
                            return false;
                    }
                }
            }
            return false;
        Label_014A:
            return true;
        }

        private static bool MoveToNextNamespaceGlobal(ref XmlAttributeCollection attributes, ref int index)
        {
            if (MoveToNextNamespaceLocal(attributes, ref index))
            {
                return true;
            }
            for (XmlElement element = attributes.parent.ParentNode as XmlElement; element != null; element = element.ParentNode as XmlElement)
            {
                if (element.HasAttributes)
                {
                    attributes = element.Attributes;
                    if (MoveToFirstNamespaceLocal(attributes, ref index))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool MoveToNextNamespaceLocal(XmlAttributeCollection attributes, ref int index)
        {
            for (int i = index - 1; i >= 0; i--)
            {
                XmlAttribute attribute = attributes[i];
                if (attribute.IsNamespace)
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToParent()
        {
            XmlNode node = this.ParentNode(this.source);
            if (node != null)
            {
                this.source = node;
                return true;
            }
            XmlAttribute source = this.source as XmlAttribute;
            if (source != null)
            {
                node = source.IsNamespace ? this.namespaceParent : source.OwnerElement;
                if (node != null)
                {
                    this.source = node;
                    this.namespaceParent = null;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToPrevious()
        {
            XmlNode node = this.PreviousSibling(this.source);
            if (node == null)
            {
                return false;
            }
            if (node.IsText)
            {
                if (this.source.IsText)
                {
                    node = this.PreviousSibling(this.TextStart(node));
                    if (node == null)
                    {
                        return false;
                    }
                }
                else
                {
                    node = this.TextStart(node);
                }
            }
            XmlNode parent = this.ParentNode(node);
            while (!IsValidChild(parent, node))
            {
                node = this.PreviousSibling(node);
                if (node == null)
                {
                    return false;
                }
            }
            this.source = node;
            return true;
        }

        public override void MoveToRoot()
        {
            while (true)
            {
                XmlNode parentNode = this.source.ParentNode;
                if (parentNode == null)
                {
                    XmlAttribute source = this.source as XmlAttribute;
                    if (source == null)
                    {
                        break;
                    }
                    parentNode = source.IsNamespace ? this.namespaceParent : source.OwnerElement;
                    if (parentNode == null)
                    {
                        break;
                    }
                }
                this.source = parentNode;
            }
            this.namespaceParent = null;
        }

        private XmlNode NextSibling(XmlNode node)
        {
            XmlNode nextSibling = node.NextSibling;
            if (!this.document.HasEntityReferences)
            {
                return nextSibling;
            }
            return this.NextSiblingTail(node, nextSibling);
        }

        private XmlNode NextSiblingTail(XmlNode node, XmlNode sibling)
        {
            while (sibling == null)
            {
                node = node.ParentNode;
                if ((node == null) || (node.NodeType != XmlNodeType.EntityReference))
                {
                    return null;
                }
                sibling = node.NextSibling;
            }
            while ((sibling != null) && (sibling.NodeType == XmlNodeType.EntityReference))
            {
                sibling = sibling.FirstChild;
            }
            return sibling;
        }

        private static XmlNode OwnerNode(XmlNode node)
        {
            XmlNode parentNode = node.ParentNode;
            if (parentNode != null)
            {
                return parentNode;
            }
            XmlAttribute attribute = node as XmlAttribute;
            if (attribute != null)
            {
                return attribute.OwnerElement;
            }
            return null;
        }

        private XmlNode ParentNode(XmlNode node)
        {
            XmlNode parentNode = node.ParentNode;
            if (!this.document.HasEntityReferences)
            {
                return parentNode;
            }
            return this.ParentNodeTail(parentNode);
        }

        private XmlNode ParentNodeTail(XmlNode parent)
        {
            while ((parent != null) && (parent.NodeType == XmlNodeType.EntityReference))
            {
                parent = parent.ParentNode;
            }
            return parent;
        }

        private bool PathHasDuplicateNamespace(XmlElement top, XmlElement bottom, string localName)
        {
            string strReservedXmlns = this.document.strReservedXmlns;
            while ((bottom != null) && (bottom != top))
            {
                if (bottom.GetAttributeNode(localName, strReservedXmlns) != null)
                {
                    return true;
                }
                bottom = bottom.ParentNode as XmlElement;
            }
            return false;
        }

        public override XmlWriter PrependChild()
        {
            switch (this.source.NodeType)
            {
                case XmlNodeType.Document:
                case XmlNodeType.DocumentFragment:
                case XmlNodeType.Element:
                {
                    DocumentXmlWriter writer = new DocumentXmlWriter(DocumentXmlWriterType.PrependChild, this.source, this.document) {
                        NamespaceManager = GetNamespaceManager(this.source, this.document)
                    };
                    return new XmlWellFormedWriter(writer, writer.Settings);
                }
            }
            throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
        }

        private XmlNode PreviousSibling(XmlNode node)
        {
            XmlNode previousSibling = node.PreviousSibling;
            if (!this.document.HasEntityReferences)
            {
                return previousSibling;
            }
            return this.PreviousSiblingTail(node, previousSibling);
        }

        private XmlNode PreviousSiblingTail(XmlNode node, XmlNode sibling)
        {
            while (sibling == null)
            {
                node = node.ParentNode;
                if ((node == null) || (node.NodeType != XmlNodeType.EntityReference))
                {
                    return null;
                }
                sibling = node.PreviousSibling;
            }
            while ((sibling != null) && (sibling.NodeType == XmlNodeType.EntityReference))
            {
                sibling = sibling.LastChild;
            }
            return sibling;
        }

        private XmlNode PreviousText(XmlNode node)
        {
            XmlNode previousText = node.PreviousText;
            if (!this.document.HasEntityReferences)
            {
                return previousText;
            }
            return this.PreviousTextTail(node, previousText);
        }

        private XmlNode PreviousTextTail(XmlNode node, XmlNode text)
        {
            if (text != null)
            {
                return text;
            }
            if (node.IsText)
            {
                XmlNode previousSibling = node.PreviousSibling;
                while (previousSibling == null)
                {
                    node = node.ParentNode;
                    if ((node == null) || (node.NodeType != XmlNodeType.EntityReference))
                    {
                        return null;
                    }
                    previousSibling = node.PreviousSibling;
                }
                while (previousSibling != null)
                {
                    switch (previousSibling.NodeType)
                    {
                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                            return previousSibling;

                        case XmlNodeType.EntityReference:
                        {
                            previousSibling = previousSibling.LastChild;
                            continue;
                        }
                        case XmlNodeType.Whitespace:
                        case XmlNodeType.SignificantWhitespace:
                            return previousSibling;
                    }
                    return null;
                }
            }
            return null;
        }

        public override XmlWriter ReplaceRange(XPathNavigator lastSiblingToReplace)
        {
            DocumentXPathNavigator navigator = lastSiblingToReplace as DocumentXPathNavigator;
            if (navigator == null)
            {
                if (lastSiblingToReplace == null)
                {
                    throw new ArgumentNullException("lastSiblingToReplace");
                }
                throw new NotSupportedException();
            }
            this.CalibrateText();
            navigator.CalibrateText();
            XmlNode source = this.source;
            XmlNode node = navigator.source;
            if (source == node)
            {
                switch (source.NodeType)
                {
                    case XmlNodeType.Attribute:
                    case XmlNodeType.Document:
                    case XmlNodeType.DocumentFragment:
                        throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        node = navigator.TextEnd(node);
                        break;
                }
            }
            else
            {
                if (node.IsText)
                {
                    node = navigator.TextEnd(node);
                }
                if (!IsFollowingSibling(source, node))
                {
                    throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
                }
            }
            DocumentXmlWriter writer = new DocumentXmlWriter(DocumentXmlWriterType.ReplaceToFollowingSibling, source, this.document) {
                NamespaceManager = GetNamespaceManager(source.ParentNode, this.document),
                Navigator = this,
                EndNode = node
            };
            return new XmlWellFormedWriter(writer, writer.Settings);
        }

        private static bool ResetAttributePosition(XmlAttribute attribute, XmlAttributeCollection attributes, out int index)
        {
            if (attributes != null)
            {
                for (int i = 0; i < attributes.Count; i++)
                {
                    if (attribute == attributes[i])
                    {
                        index = i;
                        return true;
                    }
                }
            }
            index = 0;
            return false;
        }

        internal void ResetPosition(XmlNode node)
        {
            this.source = node;
            XmlAttribute attribute = node as XmlAttribute;
            if (attribute != null)
            {
                XmlElement ownerElement = attribute.OwnerElement;
                if (ownerElement != null)
                {
                    ResetAttributePosition(attribute, ownerElement.Attributes, out this.attributeIndex);
                    if (attribute.IsNamespace)
                    {
                        this.namespaceParent = ownerElement;
                    }
                }
            }
        }

        public override XPathNodeIterator SelectDescendants(XPathNodeType nt, bool includeSelf)
        {
            if (nt != XPathNodeType.Element)
            {
                return base.SelectDescendants(nt, includeSelf);
            }
            XmlNodeType nodeType = this.source.NodeType;
            if ((nodeType != XmlNodeType.Document) && (nodeType != XmlNodeType.Element))
            {
                return new DocumentXPathNodeIterator_Empty(this);
            }
            if (includeSelf)
            {
                return new DocumentXPathNodeIterator_AllElemChildren_AndSelf(this);
            }
            return new DocumentXPathNodeIterator_AllElemChildren(this);
        }

        public override XPathNodeIterator SelectDescendants(string localName, string namespaceURI, bool matchSelf)
        {
            string nsAtom = this.document.NameTable.Get(namespaceURI);
            if ((nsAtom == null) || (this.source.NodeType == XmlNodeType.Attribute))
            {
                return new DocumentXPathNodeIterator_Empty(this);
            }
            string localNameAtom = this.document.NameTable.Get(localName);
            if (localNameAtom == null)
            {
                return new DocumentXPathNodeIterator_Empty(this);
            }
            if (localNameAtom.Length == 0)
            {
                if (matchSelf)
                {
                    return new DocumentXPathNodeIterator_ElemChildren_AndSelf_NoLocalName(this, nsAtom);
                }
                return new DocumentXPathNodeIterator_ElemChildren_NoLocalName(this, nsAtom);
            }
            if (matchSelf)
            {
                return new DocumentXPathNodeIterator_ElemChildren_AndSelf(this, localNameAtom, nsAtom);
            }
            return new DocumentXPathNodeIterator_ElemChildren(this, localNameAtom, nsAtom);
        }

        public override void SetValue(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            XmlNode source = this.source;
            switch (source.NodeType)
            {
                case XmlNodeType.Element:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                    break;

                case XmlNodeType.Attribute:
                    if (((XmlAttribute) source).IsNamespace)
                    {
                        goto Label_00B8;
                    }
                    source.InnerText = value;
                    return;

                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                {
                    this.CalibrateText();
                    source = this.source;
                    XmlNode end = this.TextEnd(source);
                    if (source != end)
                    {
                        if (source.IsReadOnly)
                        {
                            throw new InvalidOperationException(Res.GetString("Xdom_Node_Modify_ReadOnly"));
                        }
                        DeleteToFollowingSibling(source.NextSibling, end);
                        break;
                    }
                    break;
                }
                default:
                    goto Label_00B8;
            }
            source.InnerText = value;
            return;
        Label_00B8:
            throw new InvalidOperationException(Res.GetString("Xpn_BadPosition"));
        }

        XmlNode IHasXmlNode.GetNode()
        {
            return this.source;
        }

        private XmlNode TextEnd(XmlNode node)
        {
            XmlNode node2;
            do
            {
                node2 = node;
                node = this.NextSibling(node);
            }
            while ((node != null) && node.IsText);
            return node2;
        }

        private XmlNode TextStart(XmlNode node)
        {
            XmlNode node2;
            do
            {
                node2 = node;
                node = this.PreviousSibling(node);
            }
            while ((node != null) && node.IsText);
            return node2;
        }

        public override string BaseURI
        {
            get
            {
                return this.source.BaseURI;
            }
        }

        public override bool CanEdit
        {
            get
            {
                return true;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                XmlElement source = this.source as XmlElement;
                if ((source != null) && source.HasAttributes)
                {
                    XmlAttributeCollection attributes = source.Attributes;
                    for (int i = 0; i < attributes.Count; i++)
                    {
                        XmlAttribute attribute = attributes[i];
                        if (!attribute.IsNamespace)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public override bool HasChildren
        {
            get
            {
                switch (this.source.NodeType)
                {
                    case XmlNodeType.Document:
                    case XmlNodeType.DocumentFragment:
                    {
                        XmlNode child = this.FirstChild(this.source);
                        if (child != null)
                        {
                            while (!IsValidChild(this.source, child))
                            {
                                child = this.NextSibling(child);
                                if (child == null)
                                {
                                    return false;
                                }
                            }
                            return true;
                        }
                        return false;
                    }
                    case XmlNodeType.Element:
                        if (this.FirstChild(this.source) == null)
                        {
                            return false;
                        }
                        return true;
                }
                return false;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                XmlElement source = this.source as XmlElement;
                return ((source != null) && source.IsEmpty);
            }
        }

        public override string LocalName
        {
            get
            {
                return this.source.XPLocalName;
            }
        }

        public override string Name
        {
            get
            {
                switch (this.source.NodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.ProcessingInstruction:
                        return this.source.Name;

                    case XmlNodeType.Attribute:
                    {
                        if (!((XmlAttribute) this.source).IsNamespace)
                        {
                            return this.source.Name;
                        }
                        string localName = this.source.LocalName;
                        if (!Ref.Equal(localName, this.document.strXmlns))
                        {
                            return localName;
                        }
                        return string.Empty;
                    }
                }
                return string.Empty;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                XmlAttribute source = this.source as XmlAttribute;
                if ((source != null) && source.IsNamespace)
                {
                    return string.Empty;
                }
                return this.source.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.document.NameTable;
            }
        }

        public override XPathNodeType NodeType
        {
            get
            {
                this.CalibrateText();
                return this.source.XPNodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                XmlAttribute source = this.source as XmlAttribute;
                if ((source != null) && source.IsNamespace)
                {
                    return string.Empty;
                }
                return this.source.Prefix;
            }
        }

        public override IXmlSchemaInfo SchemaInfo
        {
            get
            {
                return this.source.SchemaInfo;
            }
        }

        public override object UnderlyingObject
        {
            get
            {
                this.CalibrateText();
                return this.source;
            }
        }

        public override string Value
        {
            get
            {
                switch (this.source.NodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.DocumentFragment:
                        return this.source.InnerText;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        return this.ValueText;

                    case XmlNodeType.Document:
                        return this.ValueDocument;
                }
                return this.source.Value;
            }
        }

        private string ValueDocument
        {
            get
            {
                XmlElement documentElement = this.document.DocumentElement;
                if (documentElement != null)
                {
                    return documentElement.InnerText;
                }
                return string.Empty;
            }
        }

        private string ValueText
        {
            get
            {
                this.CalibrateText();
                string str = this.source.Value;
                XmlNode node = this.NextSibling(this.source);
                if ((node == null) || !node.IsText)
                {
                    return str;
                }
                StringBuilder builder = new StringBuilder(str);
                do
                {
                    builder.Append(node.Value);
                    node = this.NextSibling(node);
                }
                while ((node != null) && node.IsText);
                return builder.ToString();
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.source.XmlLang;
            }
        }
    }
}

