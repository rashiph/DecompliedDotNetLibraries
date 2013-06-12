namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml.Schema;

    internal class XmlNodeReaderNavigator
    {
        private int attrIndex;
        private bool bCreatedOnAttribute;
        private bool bLogOnAttrVal;
        private bool bOnAttrVal;
        private XmlNode curNode;
        internal VirtualAttribute[] decNodeAttributes = new VirtualAttribute[] { new VirtualAttribute(null, null), new VirtualAttribute(null, null), new VirtualAttribute(null, null) };
        private XmlDocument doc;
        internal VirtualAttribute[] docTypeNodeAttributes = new VirtualAttribute[] { new VirtualAttribute(null, null), new VirtualAttribute(null, null) };
        private XmlNode elemNode;
        private int logAttrIndex;
        private XmlNode logNode;
        private XmlNameTable nameTable;
        private int nAttrInd;
        private int nDeclarationAttrCount;
        private int nDocTypeAttrCount;
        private int nLogAttrInd;
        private int nLogLevel;
        private const string strEncoding = "encoding";
        private const string strPublicID = "PUBLIC";
        private const string strStandalone = "standalone";
        private const string strSystemID = "SYSTEM";
        private const string strVersion = "version";

        public XmlNodeReaderNavigator(XmlNode node)
        {
            this.curNode = node;
            this.logNode = node;
            XmlNodeType nodeType = this.curNode.NodeType;
            if (nodeType == XmlNodeType.Attribute)
            {
                this.elemNode = null;
                this.attrIndex = -1;
                this.bCreatedOnAttribute = true;
            }
            else
            {
                this.elemNode = node;
                this.attrIndex = -1;
                this.bCreatedOnAttribute = false;
            }
            if (nodeType == XmlNodeType.Document)
            {
                this.doc = (XmlDocument) this.curNode;
            }
            else
            {
                this.doc = node.OwnerDocument;
            }
            this.nameTable = this.doc.NameTable;
            this.nAttrInd = -1;
            this.nDeclarationAttrCount = -1;
            this.nDocTypeAttrCount = -1;
            this.bOnAttrVal = false;
            this.bLogOnAttrVal = false;
        }

        private void CheckIndexCondition(int attributeIndex)
        {
            if ((attributeIndex < 0) || (attributeIndex >= this.AttributeCount))
            {
                throw new ArgumentOutOfRangeException("attributeIndex");
            }
        }

        internal string DefaultLookupNamespace(string prefix)
        {
            if (!this.bCreatedOnAttribute)
            {
                if (prefix == "xmlns")
                {
                    return this.nameTable.Add("http://www.w3.org/2000/xmlns/");
                }
                if (prefix == "xml")
                {
                    return this.nameTable.Add("http://www.w3.org/XML/1998/namespace");
                }
                if (prefix == string.Empty)
                {
                    return this.nameTable.Add(string.Empty);
                }
            }
            return null;
        }

        public string GetAttribute(int attributeIndex)
        {
            if (this.bCreatedOnAttribute)
            {
                return null;
            }
            switch (this.curNode.NodeType)
            {
                case XmlNodeType.Element:
                    this.CheckIndexCondition(attributeIndex);
                    return ((XmlElement) this.curNode).Attributes[attributeIndex].Value;

                case XmlNodeType.Attribute:
                    this.CheckIndexCondition(attributeIndex);
                    return ((XmlElement) this.elemNode).Attributes[attributeIndex].Value;

                case XmlNodeType.DocumentType:
                    this.CheckIndexCondition(attributeIndex);
                    return this.GetDocumentTypeAttr(attributeIndex);

                case XmlNodeType.XmlDeclaration:
                    this.CheckIndexCondition(attributeIndex);
                    return this.GetDeclarationAttr(attributeIndex);
            }
            throw new ArgumentOutOfRangeException("attributeIndex");
        }

        public string GetAttribute(string name)
        {
            if (!this.bCreatedOnAttribute)
            {
                switch (this.curNode.NodeType)
                {
                    case XmlNodeType.Element:
                        return this.GetAttributeFromElement((XmlElement) this.curNode, name);

                    case XmlNodeType.Attribute:
                        return this.GetAttributeFromElement((XmlElement) this.elemNode, name);

                    case XmlNodeType.DocumentType:
                        return this.GetDocumentTypeAttr((XmlDocumentType) this.curNode, name);

                    case XmlNodeType.XmlDeclaration:
                        return this.GetDeclarationAttr((XmlDeclaration) this.curNode, name);
                }
            }
            return null;
        }

        public string GetAttribute(string name, string ns)
        {
            if (!this.bCreatedOnAttribute)
            {
                switch (this.curNode.NodeType)
                {
                    case XmlNodeType.Element:
                        return this.GetAttributeFromElement((XmlElement) this.curNode, name, ns);

                    case XmlNodeType.Attribute:
                        return this.GetAttributeFromElement((XmlElement) this.elemNode, name, ns);

                    case XmlNodeType.DocumentType:
                        if (ns.Length != 0)
                        {
                            return null;
                        }
                        return this.GetDocumentTypeAttr((XmlDocumentType) this.curNode, name);

                    case XmlNodeType.XmlDeclaration:
                        if (ns.Length != 0)
                        {
                            return null;
                        }
                        return this.GetDeclarationAttr((XmlDeclaration) this.curNode, name);
                }
            }
            return null;
        }

        private string GetAttributeFromElement(XmlElement elem, string name)
        {
            XmlAttribute attributeNode = elem.GetAttributeNode(name);
            if (attributeNode != null)
            {
                return attributeNode.Value;
            }
            return null;
        }

        private string GetAttributeFromElement(XmlElement elem, string name, string ns)
        {
            XmlAttribute attributeNode = elem.GetAttributeNode(name, ns);
            if (attributeNode != null)
            {
                return attributeNode.Value;
            }
            return null;
        }

        public int GetDecAttrInd(string name)
        {
            if (this.nDeclarationAttrCount == -1)
            {
                this.InitDecAttr();
            }
            for (int i = 0; i < this.nDeclarationAttrCount; i++)
            {
                if (this.decNodeAttributes[i].name == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public string GetDeclarationAttr(int i)
        {
            if (this.nDeclarationAttrCount == -1)
            {
                this.InitDecAttr();
            }
            return this.decNodeAttributes[i].value;
        }

        public string GetDeclarationAttr(XmlDeclaration decl, string name)
        {
            if (name == "version")
            {
                return decl.Version;
            }
            if (name == "encoding")
            {
                return decl.Encoding;
            }
            if (name == "standalone")
            {
                return decl.Standalone;
            }
            return null;
        }

        public int GetDocTypeAttrInd(string name)
        {
            if (this.nDocTypeAttrCount == -1)
            {
                this.InitDocTypeAttr();
            }
            for (int i = 0; i < this.nDocTypeAttrCount; i++)
            {
                if (this.docTypeNodeAttributes[i].name == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public string GetDocumentTypeAttr(int i)
        {
            if (this.nDocTypeAttrCount == -1)
            {
                this.InitDocTypeAttr();
            }
            return this.docTypeNodeAttributes[i].value;
        }

        public string GetDocumentTypeAttr(XmlDocumentType docType, string name)
        {
            if (name == "PUBLIC")
            {
                return docType.PublicId;
            }
            if (name == "SYSTEM")
            {
                return docType.SystemId;
            }
            return null;
        }

        internal IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (!this.bCreatedOnAttribute)
            {
                XmlNode curNode = this.curNode;
                while (curNode != null)
                {
                    if (curNode.NodeType == XmlNodeType.Element)
                    {
                        XmlElement element = (XmlElement) curNode;
                        if (element.HasAttributes)
                        {
                            XmlAttributeCollection attributes = element.Attributes;
                            for (int i = 0; i < attributes.Count; i++)
                            {
                                XmlAttribute attribute = attributes[i];
                                if ((attribute.LocalName == "xmlns") && (attribute.Prefix.Length == 0))
                                {
                                    if (!dictionary.ContainsKey(string.Empty))
                                    {
                                        dictionary.Add(this.nameTable.Add(string.Empty), this.nameTable.Add(attribute.Value));
                                    }
                                }
                                else if (attribute.Prefix == "xmlns")
                                {
                                    string localName = attribute.LocalName;
                                    if (!dictionary.ContainsKey(localName))
                                    {
                                        dictionary.Add(this.nameTable.Add(localName), this.nameTable.Add(attribute.Value));
                                    }
                                }
                            }
                        }
                        if (scope == XmlNamespaceScope.Local)
                        {
                            break;
                        }
                    }
                    else if (curNode.NodeType == XmlNodeType.Attribute)
                    {
                        curNode = ((XmlAttribute) curNode).OwnerElement;
                        continue;
                    }
                    curNode = curNode.ParentNode;
                }
                if (scope != XmlNamespaceScope.Local)
                {
                    if (dictionary.ContainsKey(string.Empty) && (dictionary[string.Empty] == string.Empty))
                    {
                        dictionary.Remove(string.Empty);
                    }
                    if (scope == XmlNamespaceScope.All)
                    {
                        dictionary.Add(this.nameTable.Add("xml"), this.nameTable.Add("http://www.w3.org/XML/1998/namespace"));
                    }
                }
            }
            return dictionary;
        }

        private void InitDecAttr()
        {
            int index = 0;
            string version = this.doc.Version;
            if ((version != null) && (version.Length != 0))
            {
                this.decNodeAttributes[index].name = "version";
                this.decNodeAttributes[index].value = version;
                index++;
            }
            version = this.doc.Encoding;
            if ((version != null) && (version.Length != 0))
            {
                this.decNodeAttributes[index].name = "encoding";
                this.decNodeAttributes[index].value = version;
                index++;
            }
            version = this.doc.Standalone;
            if ((version != null) && (version.Length != 0))
            {
                this.decNodeAttributes[index].name = "standalone";
                this.decNodeAttributes[index].value = version;
                index++;
            }
            this.nDeclarationAttrCount = index;
        }

        private void InitDocTypeAttr()
        {
            int index = 0;
            XmlDocumentType documentType = this.doc.DocumentType;
            if (documentType == null)
            {
                this.nDocTypeAttrCount = 0;
            }
            else
            {
                string publicId = documentType.PublicId;
                if (publicId != null)
                {
                    this.docTypeNodeAttributes[index].name = "PUBLIC";
                    this.docTypeNodeAttributes[index].value = publicId;
                    index++;
                }
                publicId = documentType.SystemId;
                if (publicId != null)
                {
                    this.docTypeNodeAttributes[index].name = "SYSTEM";
                    this.docTypeNodeAttributes[index].value = publicId;
                    index++;
                }
                this.nDocTypeAttrCount = index;
            }
        }

        private bool IsLocalNameEmpty(XmlNodeType nt)
        {
            switch (nt)
            {
                case XmlNodeType.None:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Comment:
                case XmlNodeType.Document:
                case XmlNodeType.DocumentFragment:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.EndElement:
                case XmlNodeType.EndEntity:
                    return true;

                case XmlNodeType.Element:
                case XmlNodeType.Attribute:
                case XmlNodeType.EntityReference:
                case XmlNodeType.Entity:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.DocumentType:
                case XmlNodeType.Notation:
                case XmlNodeType.XmlDeclaration:
                    return false;
            }
            return true;
        }

        public void LogMove(int level)
        {
            this.logNode = this.curNode;
            this.nLogLevel = level;
            this.nLogAttrInd = this.nAttrInd;
            this.logAttrIndex = this.attrIndex;
            this.bLogOnAttrVal = this.bOnAttrVal;
        }

        public string LookupNamespace(string prefix)
        {
            if (!this.bCreatedOnAttribute)
            {
                string str;
                if (prefix == "xmlns")
                {
                    return this.nameTable.Add("http://www.w3.org/2000/xmlns/");
                }
                if (prefix == "xml")
                {
                    return this.nameTable.Add("http://www.w3.org/XML/1998/namespace");
                }
                if (prefix == null)
                {
                    prefix = string.Empty;
                }
                if (prefix.Length == 0)
                {
                    str = "xmlns";
                }
                else
                {
                    str = "xmlns:" + prefix;
                }
                XmlNode curNode = this.curNode;
                while (curNode != null)
                {
                    if (curNode.NodeType == XmlNodeType.Element)
                    {
                        XmlElement element = (XmlElement) curNode;
                        if (element.HasAttributes)
                        {
                            XmlAttribute attributeNode = element.GetAttributeNode(str);
                            if (attributeNode != null)
                            {
                                return attributeNode.Value;
                            }
                        }
                    }
                    else if (curNode.NodeType == XmlNodeType.Attribute)
                    {
                        curNode = ((XmlAttribute) curNode).OwnerElement;
                        continue;
                    }
                    curNode = curNode.ParentNode;
                }
                if (prefix.Length == 0)
                {
                    return string.Empty;
                }
            }
            return null;
        }

        internal string LookupPrefix(string namespaceName)
        {
            if (!this.bCreatedOnAttribute && (namespaceName != null))
            {
                if (namespaceName == "http://www.w3.org/2000/xmlns/")
                {
                    return this.nameTable.Add("xmlns");
                }
                if (namespaceName == "http://www.w3.org/XML/1998/namespace")
                {
                    return this.nameTable.Add("xml");
                }
                if (namespaceName == string.Empty)
                {
                    return string.Empty;
                }
                XmlNode curNode = this.curNode;
                while (curNode != null)
                {
                    if (curNode.NodeType == XmlNodeType.Element)
                    {
                        XmlElement element = (XmlElement) curNode;
                        if (element.HasAttributes)
                        {
                            XmlAttributeCollection attributes = element.Attributes;
                            for (int i = 0; i < attributes.Count; i++)
                            {
                                XmlAttribute attribute = attributes[i];
                                if (attribute.Value == namespaceName)
                                {
                                    if ((attribute.Prefix.Length == 0) && (attribute.LocalName == "xmlns"))
                                    {
                                        if (this.LookupNamespace(string.Empty) == namespaceName)
                                        {
                                            return string.Empty;
                                        }
                                    }
                                    else if (attribute.Prefix == "xmlns")
                                    {
                                        string localName = attribute.LocalName;
                                        if (this.LookupNamespace(localName) == namespaceName)
                                        {
                                            return this.nameTable.Add(localName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (curNode.NodeType == XmlNodeType.Attribute)
                    {
                        curNode = ((XmlAttribute) curNode).OwnerElement;
                        continue;
                    }
                    curNode = curNode.ParentNode;
                }
            }
            return null;
        }

        public void MoveToAttribute(int attributeIndex)
        {
            if (!this.bCreatedOnAttribute)
            {
                XmlAttribute attribute = null;
                switch (this.curNode.NodeType)
                {
                    case XmlNodeType.Element:
                        this.CheckIndexCondition(attributeIndex);
                        attribute = ((XmlElement) this.curNode).Attributes[attributeIndex];
                        if (attribute != null)
                        {
                            this.elemNode = this.curNode;
                            this.curNode = attribute;
                            this.attrIndex = attributeIndex;
                        }
                        return;

                    case XmlNodeType.Attribute:
                        this.CheckIndexCondition(attributeIndex);
                        attribute = ((XmlElement) this.elemNode).Attributes[attributeIndex];
                        if (attribute != null)
                        {
                            this.curNode = attribute;
                            this.attrIndex = attributeIndex;
                        }
                        return;

                    case XmlNodeType.DocumentType:
                    case XmlNodeType.XmlDeclaration:
                        this.CheckIndexCondition(attributeIndex);
                        this.nAttrInd = attributeIndex;
                        return;
                }
            }
        }

        public bool MoveToAttribute(string name)
        {
            return this.MoveToAttribute(name, string.Empty);
        }

        public bool MoveToAttribute(string name, string namespaceURI)
        {
            if (!this.bCreatedOnAttribute)
            {
                XmlNodeType nodeType = this.curNode.NodeType;
                switch (nodeType)
                {
                    case XmlNodeType.Element:
                        return this.MoveToAttributeFromElement((XmlElement) this.curNode, name, namespaceURI);

                    case XmlNodeType.Attribute:
                        return this.MoveToAttributeFromElement((XmlElement) this.elemNode, name, namespaceURI);
                }
                if ((nodeType == XmlNodeType.XmlDeclaration) && (namespaceURI.Length == 0))
                {
                    this.nAttrInd = this.GetDecAttrInd(name);
                    if (this.nAttrInd != -1)
                    {
                        this.bOnAttrVal = false;
                        return true;
                    }
                }
                else if (((nodeType == XmlNodeType.DocumentType) && (namespaceURI.Length == 0)) && ((this.nAttrInd = this.GetDocTypeAttrInd(name)) != -1))
                {
                    this.bOnAttrVal = false;
                    return true;
                }
            }
            return false;
        }

        private bool MoveToAttributeFromElement(XmlElement elem, string name, string ns)
        {
            XmlAttribute node = null;
            if (ns.Length == 0)
            {
                node = elem.GetAttributeNode(name);
            }
            else
            {
                node = elem.GetAttributeNode(name, ns);
            }
            if (node != null)
            {
                this.bOnAttrVal = false;
                this.elemNode = elem;
                this.curNode = node;
                this.attrIndex = elem.Attributes.FindNodeOffsetNS(node);
                if (this.attrIndex != -1)
                {
                    return true;
                }
            }
            return false;
        }

        public bool MoveToElement()
        {
            if (!this.bCreatedOnAttribute)
            {
                XmlNodeType nodeType = this.curNode.NodeType;
                if (nodeType != XmlNodeType.Attribute)
                {
                    if (((nodeType == XmlNodeType.DocumentType) || (nodeType == XmlNodeType.XmlDeclaration)) && (this.nAttrInd != -1))
                    {
                        this.nAttrInd = -1;
                        return true;
                    }
                }
                else if (this.elemNode != null)
                {
                    this.curNode = this.elemNode;
                    this.attrIndex = -1;
                    return true;
                }
            }
            return false;
        }

        public bool MoveToFirstChild()
        {
            XmlNode firstChild = this.curNode.FirstChild;
            if (firstChild == null)
            {
                return false;
            }
            this.curNode = firstChild;
            if (!this.bOnAttrVal)
            {
                this.attrIndex = -1;
            }
            return true;
        }

        public bool MoveToNext()
        {
            if (this.curNode.NodeType != XmlNodeType.Attribute)
            {
                return this.MoveToNextSibling(this.curNode);
            }
            return this.MoveToNextSibling(this.elemNode);
        }

        public bool MoveToNextAttribute(ref int level)
        {
            if (!this.bCreatedOnAttribute)
            {
                switch (this.curNode.NodeType)
                {
                    case XmlNodeType.Attribute:
                        if (this.attrIndex >= (this.elemNode.Attributes.Count - 1))
                        {
                            return false;
                        }
                        this.curNode = this.elemNode.Attributes[++this.attrIndex];
                        return true;

                    case XmlNodeType.Element:
                        if (this.curNode.Attributes.Count > 0)
                        {
                            level++;
                            this.elemNode = this.curNode;
                            this.curNode = this.curNode.Attributes[0];
                            this.attrIndex = 0;
                            return true;
                        }
                        break;

                    case XmlNodeType.XmlDeclaration:
                        if (this.nDeclarationAttrCount == -1)
                        {
                            this.InitDecAttr();
                        }
                        this.nAttrInd++;
                        if (this.nAttrInd < this.nDeclarationAttrCount)
                        {
                            if (this.nAttrInd == 0)
                            {
                                level++;
                            }
                            this.bOnAttrVal = false;
                            return true;
                        }
                        this.nAttrInd--;
                        break;

                    case XmlNodeType.DocumentType:
                        if (this.nDocTypeAttrCount == -1)
                        {
                            this.InitDocTypeAttr();
                        }
                        this.nAttrInd++;
                        if (this.nAttrInd < this.nDocTypeAttrCount)
                        {
                            if (this.nAttrInd == 0)
                            {
                                level++;
                            }
                            this.bOnAttrVal = false;
                            return true;
                        }
                        this.nAttrInd--;
                        break;
                }
            }
            return false;
        }

        private bool MoveToNextSibling(XmlNode node)
        {
            XmlNode nextSibling = node.NextSibling;
            if (nextSibling == null)
            {
                return false;
            }
            this.curNode = nextSibling;
            if (!this.bOnAttrVal)
            {
                this.attrIndex = -1;
            }
            return true;
        }

        public bool MoveToParent()
        {
            XmlNode parentNode = this.curNode.ParentNode;
            if (parentNode == null)
            {
                return false;
            }
            this.curNode = parentNode;
            if (!this.bOnAttrVal)
            {
                this.attrIndex = 0;
            }
            return true;
        }

        public bool ReadAttributeValue(ref int level, ref bool bResolveEntity, ref XmlNodeType nt)
        {
            if (this.nAttrInd != -1)
            {
                if (!this.bOnAttrVal)
                {
                    this.bOnAttrVal = true;
                    level++;
                    nt = XmlNodeType.Text;
                    return true;
                }
                return false;
            }
            if (this.curNode.NodeType == XmlNodeType.Attribute)
            {
                XmlNode firstChild = this.curNode.FirstChild;
                if (firstChild != null)
                {
                    this.curNode = firstChild;
                    nt = this.curNode.NodeType;
                    level++;
                    this.bOnAttrVal = true;
                    return true;
                }
            }
            else if (this.bOnAttrVal)
            {
                XmlNode nextSibling = null;
                if ((this.curNode.NodeType == XmlNodeType.EntityReference) && bResolveEntity)
                {
                    this.curNode = this.curNode.FirstChild;
                    nt = this.curNode.NodeType;
                    level++;
                    bResolveEntity = false;
                    return true;
                }
                nextSibling = this.curNode.NextSibling;
                if (nextSibling == null)
                {
                    XmlNode parentNode = this.curNode.ParentNode;
                    if ((parentNode != null) && (parentNode.NodeType == XmlNodeType.EntityReference))
                    {
                        this.curNode = parentNode;
                        nt = XmlNodeType.EndEntity;
                        level--;
                        return true;
                    }
                }
                if (nextSibling != null)
                {
                    this.curNode = nextSibling;
                    nt = this.curNode.NodeType;
                    return true;
                }
                return false;
            }
            return false;
        }

        public void ResetMove(ref int level, ref XmlNodeType nt)
        {
            this.LogMove(level);
            if (!this.bCreatedOnAttribute)
            {
                if (this.nAttrInd != -1)
                {
                    if (this.bOnAttrVal)
                    {
                        level--;
                        this.bOnAttrVal = false;
                    }
                    this.nLogAttrInd = this.nAttrInd;
                    level--;
                    this.nAttrInd = -1;
                    nt = this.curNode.NodeType;
                }
                else
                {
                    if (this.bOnAttrVal && (this.curNode.NodeType != XmlNodeType.Attribute))
                    {
                        this.ResetToAttribute(ref level);
                    }
                    if (this.curNode.NodeType == XmlNodeType.Attribute)
                    {
                        this.curNode = ((XmlAttribute) this.curNode).OwnerElement;
                        this.attrIndex = -1;
                        level--;
                        nt = XmlNodeType.Element;
                    }
                    if (this.curNode.NodeType == XmlNodeType.Element)
                    {
                        this.elemNode = this.curNode;
                    }
                }
            }
        }

        public void ResetToAttribute(ref int level)
        {
            if (!this.bCreatedOnAttribute && this.bOnAttrVal)
            {
                if (!this.IsOnDeclOrDocType)
                {
                    while ((this.curNode.NodeType != XmlNodeType.Attribute) && ((this.curNode = this.curNode.ParentNode) != null))
                    {
                        level--;
                    }
                }
                else
                {
                    level -= 2;
                }
                this.bOnAttrVal = false;
            }
        }

        public void RollBackMove(ref int level)
        {
            this.curNode = this.logNode;
            level = this.nLogLevel;
            this.nAttrInd = this.nLogAttrInd;
            this.attrIndex = this.logAttrIndex;
            this.bOnAttrVal = this.bLogOnAttrVal;
        }

        public int AttributeCount
        {
            get
            {
                if (this.bCreatedOnAttribute)
                {
                    return 0;
                }
                XmlNodeType nodeType = this.curNode.NodeType;
                if (nodeType == XmlNodeType.Element)
                {
                    return ((XmlElement) this.curNode).Attributes.Count;
                }
                if ((nodeType == XmlNodeType.Attribute) || ((this.bOnAttrVal && (nodeType != XmlNodeType.XmlDeclaration)) && (nodeType != XmlNodeType.DocumentType)))
                {
                    return this.elemNode.Attributes.Count;
                }
                if (nodeType == XmlNodeType.XmlDeclaration)
                {
                    if (this.nDeclarationAttrCount == -1)
                    {
                        this.InitDecAttr();
                    }
                    return this.nDeclarationAttrCount;
                }
                if (nodeType != XmlNodeType.DocumentType)
                {
                    return 0;
                }
                if (this.nDocTypeAttrCount == -1)
                {
                    this.InitDocTypeAttr();
                }
                return this.nDocTypeAttrCount;
            }
        }

        public string BaseURI
        {
            get
            {
                return this.curNode.BaseURI;
            }
        }

        internal bool CreatedOnAttribute
        {
            get
            {
                return this.bCreatedOnAttribute;
            }
        }

        public XmlDocument Document
        {
            get
            {
                return this.doc;
            }
        }

        public bool HasValue
        {
            get
            {
                if ((this.nAttrInd == -1) && ((this.curNode.Value == null) && (this.curNode.NodeType != XmlNodeType.DocumentType)))
                {
                    return false;
                }
                return true;
            }
        }

        public bool IsDefault
        {
            get
            {
                return ((this.curNode.NodeType == XmlNodeType.Attribute) && !((XmlAttribute) this.curNode).Specified);
            }
        }

        public bool IsEmptyElement
        {
            get
            {
                return ((this.curNode.NodeType == XmlNodeType.Element) && ((XmlElement) this.curNode).IsEmpty);
            }
        }

        internal bool IsOnAttrVal
        {
            get
            {
                return this.bOnAttrVal;
            }
        }

        private bool IsOnDeclOrDocType
        {
            get
            {
                XmlNodeType nodeType = this.curNode.NodeType;
                if (nodeType != XmlNodeType.XmlDeclaration)
                {
                    return (nodeType == XmlNodeType.DocumentType);
                }
                return true;
            }
        }

        public string LocalName
        {
            get
            {
                if (this.nAttrInd != -1)
                {
                    return this.Name;
                }
                if (this.IsLocalNameEmpty(this.curNode.NodeType))
                {
                    return string.Empty;
                }
                return this.curNode.LocalName;
            }
        }

        public string Name
        {
            get
            {
                if (this.nAttrInd != -1)
                {
                    if (this.bOnAttrVal)
                    {
                        return string.Empty;
                    }
                    if (this.curNode.NodeType == XmlNodeType.XmlDeclaration)
                    {
                        return this.decNodeAttributes[this.nAttrInd].name;
                    }
                    return this.docTypeNodeAttributes[this.nAttrInd].name;
                }
                if (this.IsLocalNameEmpty(this.curNode.NodeType))
                {
                    return string.Empty;
                }
                return this.curNode.Name;
            }
        }

        public string NamespaceURI
        {
            get
            {
                return this.curNode.NamespaceURI;
            }
        }

        public XmlNameTable NameTable
        {
            get
            {
                return this.nameTable;
            }
        }

        public XmlNodeType NodeType
        {
            get
            {
                XmlNodeType nodeType = this.curNode.NodeType;
                if (this.nAttrInd == -1)
                {
                    return nodeType;
                }
                if (this.bOnAttrVal)
                {
                    return XmlNodeType.Text;
                }
                return XmlNodeType.Attribute;
            }
        }

        internal XmlNode OwnerElementNode
        {
            get
            {
                if (this.bCreatedOnAttribute)
                {
                    return null;
                }
                return this.elemNode;
            }
        }

        public string Prefix
        {
            get
            {
                return this.curNode.Prefix;
            }
        }

        public IXmlSchemaInfo SchemaInfo
        {
            get
            {
                return this.curNode.SchemaInfo;
            }
        }

        public string Value
        {
            get
            {
                string internalSubset = null;
                XmlNodeType nodeType = this.curNode.NodeType;
                if (this.nAttrInd != -1)
                {
                    if (this.curNode.NodeType == XmlNodeType.XmlDeclaration)
                    {
                        return this.decNodeAttributes[this.nAttrInd].value;
                    }
                    return this.docTypeNodeAttributes[this.nAttrInd].value;
                }
                switch (nodeType)
                {
                    case XmlNodeType.DocumentType:
                        internalSubset = ((XmlDocumentType) this.curNode).InternalSubset;
                        break;

                    case XmlNodeType.XmlDeclaration:
                    {
                        StringBuilder builder = new StringBuilder(string.Empty);
                        if (this.nDeclarationAttrCount == -1)
                        {
                            this.InitDecAttr();
                        }
                        for (int i = 0; i < this.nDeclarationAttrCount; i++)
                        {
                            builder.Append(this.decNodeAttributes[i].name + "=\"" + this.decNodeAttributes[i].value + "\"");
                            if (i != (this.nDeclarationAttrCount - 1))
                            {
                                builder.Append(" ");
                            }
                        }
                        internalSubset = builder.ToString();
                        break;
                    }
                    default:
                        internalSubset = this.curNode.Value;
                        break;
                }
                if (internalSubset != null)
                {
                    return internalSubset;
                }
                return string.Empty;
            }
        }

        public string XmlLang
        {
            get
            {
                return this.curNode.XmlLang;
            }
        }

        public System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this.curNode.XmlSpace;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct VirtualAttribute
        {
            internal string name;
            internal string value;
            internal VirtualAttribute(string name, string value)
            {
                this.name = name;
                this.value = value;
            }
        }
    }
}

