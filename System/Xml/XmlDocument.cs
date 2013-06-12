namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml.Schema;
    using System.Xml.XPath;

    public class XmlDocument : XmlNode
    {
        private bool actualLoadingStatus;
        internal string baseURI;
        internal bool bSetResolver;
        private DomNameTable domNameTable;
        internal static System.Xml.EmptyEnumerator EmptyEnumerator = new System.Xml.EmptyEnumerator();
        private XmlNamedNodeMap entities;
        internal bool fCDataNodesPresent;
        internal bool fEntRefNodesPresent;
        private Hashtable htElementIDAttrDecl;
        private Hashtable htElementIdMap;
        private XmlImplementation implementation;
        internal static IXmlSchemaInfo InvalidSchemaInfo = new XmlSchemaInfo(XmlSchemaValidity.Invalid);
        private bool isLoading;
        private XmlLinkedNode lastChild;
        private XmlAttribute namespaceXml;
        internal static IXmlSchemaInfo NotKnownSchemaInfo = new XmlSchemaInfo(XmlSchemaValidity.NotKnown);
        internal object objLock;
        private bool preserveWhitespace;
        private bool reportValidity;
        private System.Xml.XmlResolver resolver;
        private System.Xml.Schema.SchemaInfo schemaInfo;
        private XmlSchemaSet schemas;
        internal string strCDataSectionName;
        internal string strCommentName;
        internal string strDocumentFragmentName;
        internal string strDocumentName;
        internal string strEmpty;
        internal string strEntityName;
        internal string strID;
        internal string strLang;
        internal string strNonSignificantWhitespaceName;
        internal string strReservedXml;
        internal string strReservedXmlns;
        internal string strSignificantWhitespaceName;
        internal string strSpace;
        internal string strTextName;
        internal string strXml;
        internal string strXmlns;
        internal static IXmlSchemaInfo ValidSchemaInfo = new XmlSchemaInfo(XmlSchemaValidity.Valid);

        public event XmlNodeChangedEventHandler NodeChanged;

        public event XmlNodeChangedEventHandler NodeChanging;

        public event XmlNodeChangedEventHandler NodeInserted;

        public event XmlNodeChangedEventHandler NodeInserting;

        public event XmlNodeChangedEventHandler NodeRemoved;

        public event XmlNodeChangedEventHandler NodeRemoving;

        public XmlDocument() : this(new XmlImplementation())
        {
        }

        protected internal XmlDocument(XmlImplementation imp)
        {
            this.implementation = imp;
            this.domNameTable = new DomNameTable(this);
            XmlNameTable nameTable = this.NameTable;
            nameTable.Add(string.Empty);
            this.strDocumentName = nameTable.Add("#document");
            this.strDocumentFragmentName = nameTable.Add("#document-fragment");
            this.strCommentName = nameTable.Add("#comment");
            this.strTextName = nameTable.Add("#text");
            this.strCDataSectionName = nameTable.Add("#cdata-section");
            this.strEntityName = nameTable.Add("#entity");
            this.strID = nameTable.Add("id");
            this.strNonSignificantWhitespaceName = nameTable.Add("#whitespace");
            this.strSignificantWhitespaceName = nameTable.Add("#significant-whitespace");
            this.strXmlns = nameTable.Add("xmlns");
            this.strXml = nameTable.Add("xml");
            this.strSpace = nameTable.Add("space");
            this.strLang = nameTable.Add("lang");
            this.strReservedXmlns = nameTable.Add("http://www.w3.org/2000/xmlns/");
            this.strReservedXml = nameTable.Add("http://www.w3.org/XML/1998/namespace");
            this.strEmpty = nameTable.Add(string.Empty);
            this.baseURI = string.Empty;
            this.objLock = new object();
        }

        public XmlDocument(XmlNameTable nt) : this(new XmlImplementation(nt))
        {
        }

        internal XmlName AddAttrXmlName(string prefix, string localName, string namespaceURI, IXmlSchemaInfo schemaInfo)
        {
            XmlName name = this.AddXmlName(prefix, localName, namespaceURI, schemaInfo);
            if (!this.IsLoading)
            {
                object obj2 = name.Prefix;
                object obj3 = name.NamespaceURI;
                object obj4 = name.LocalName;
                if (((obj2 == this.strXmlns) || ((obj2 == this.strEmpty) && (obj4 == this.strXmlns))) ^ (obj3 == this.strReservedXmlns))
                {
                    throw new ArgumentException(Res.GetString("Xdom_Attr_Reserved_XmlNS", new object[] { namespaceURI }));
                }
            }
            return name;
        }

        internal void AddDefaultAttributes(XmlElement elem)
        {
            System.Xml.Schema.SchemaInfo dtdSchemaInfo = this.DtdSchemaInfo;
            SchemaElementDecl schemaElementDecl = this.GetSchemaElementDecl(elem);
            if ((schemaElementDecl != null) && (schemaElementDecl.AttDefs != null))
            {
                IDictionaryEnumerator enumerator = schemaElementDecl.AttDefs.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    SchemaAttDef attdef = (SchemaAttDef) enumerator.Value;
                    if ((attdef.Presence == SchemaDeclBase.Use.Default) || (attdef.Presence == SchemaDeclBase.Use.Fixed))
                    {
                        string attrPrefix = string.Empty;
                        string name = attdef.Name.Name;
                        string attrNamespaceURI = string.Empty;
                        if (dtdSchemaInfo.SchemaType == SchemaType.DTD)
                        {
                            attrPrefix = attdef.Name.Namespace;
                        }
                        else
                        {
                            attrPrefix = attdef.Prefix;
                            attrNamespaceURI = attdef.Name.Namespace;
                        }
                        XmlAttribute newAttr = this.PrepareDefaultAttribute(attdef, attrPrefix, name, attrNamespaceURI);
                        elem.SetAttributeNode(newAttr);
                    }
                }
            }
        }

        internal void AddElementWithId(string id, XmlElement elem)
        {
            if ((this.htElementIdMap == null) || !this.htElementIdMap.Contains(id))
            {
                if (this.htElementIdMap == null)
                {
                    this.htElementIdMap = new Hashtable();
                }
                ArrayList list = new ArrayList();
                list.Add(new WeakReference(elem));
                this.htElementIdMap.Add(id, list);
            }
            else
            {
                ArrayList elementList = (ArrayList) this.htElementIdMap[id];
                if (this.GetElement(elementList, elem) == null)
                {
                    elementList.Add(new WeakReference(elem));
                }
            }
        }

        internal bool AddIdInfo(XmlName eleName, XmlName attrName)
        {
            if ((this.htElementIDAttrDecl != null) && (this.htElementIDAttrDecl[eleName] != null))
            {
                return false;
            }
            if (this.htElementIDAttrDecl == null)
            {
                this.htElementIDAttrDecl = new Hashtable();
            }
            this.htElementIDAttrDecl.Add(eleName, attrName);
            return true;
        }

        internal XmlName AddXmlName(string prefix, string localName, string namespaceURI, IXmlSchemaInfo schemaInfo)
        {
            return this.domNameTable.AddName(prefix, localName, namespaceURI, schemaInfo);
        }

        internal override void AfterEvent(XmlNodeChangedEventArgs args)
        {
            if (args != null)
            {
                switch (args.Action)
                {
                    case XmlNodeChangedAction.Insert:
                        if (this.onNodeInsertedDelegate == null)
                        {
                            break;
                        }
                        this.onNodeInsertedDelegate(this, args);
                        return;

                    case XmlNodeChangedAction.Remove:
                        if (this.onNodeRemovedDelegate == null)
                        {
                            break;
                        }
                        this.onNodeRemovedDelegate(this, args);
                        return;

                    case XmlNodeChangedAction.Change:
                        if (this.onNodeChangedDelegate != null)
                        {
                            this.onNodeChangedDelegate(this, args);
                        }
                        break;

                    default:
                        return;
                }
            }
        }

        internal override XmlNode AppendChildForLoad(XmlNode newChild, XmlDocument doc)
        {
            if (!this.IsValidChildType(newChild.NodeType))
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Node_Insert_TypeConflict"));
            }
            if (!this.CanInsertAfter(newChild, this.LastChild))
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Node_Insert_Location"));
            }
            XmlNodeChangedEventArgs insertEventArgsForLoad = this.GetInsertEventArgsForLoad(newChild, this);
            if (insertEventArgsForLoad != null)
            {
                this.BeforeEvent(insertEventArgsForLoad);
            }
            XmlLinkedNode node = (XmlLinkedNode) newChild;
            if (this.lastChild == null)
            {
                node.next = node;
            }
            else
            {
                node.next = this.lastChild.next;
                this.lastChild.next = node;
            }
            this.lastChild = node;
            node.SetParentForLoad(this);
            if (insertEventArgsForLoad != null)
            {
                this.AfterEvent(insertEventArgsForLoad);
            }
            return node;
        }

        internal override void BeforeEvent(XmlNodeChangedEventArgs args)
        {
            if (args != null)
            {
                switch (args.Action)
                {
                    case XmlNodeChangedAction.Insert:
                        if (this.onNodeInsertingDelegate == null)
                        {
                            break;
                        }
                        this.onNodeInsertingDelegate(this, args);
                        return;

                    case XmlNodeChangedAction.Remove:
                        if (this.onNodeRemovingDelegate == null)
                        {
                            break;
                        }
                        this.onNodeRemovingDelegate(this, args);
                        return;

                    case XmlNodeChangedAction.Change:
                        if (this.onNodeChangingDelegate != null)
                        {
                            this.onNodeChangingDelegate(this, args);
                        }
                        break;

                    default:
                        return;
                }
            }
        }

        internal override bool CanInsertAfter(XmlNode newChild, XmlNode refChild)
        {
            if (refChild == null)
            {
                refChild = this.LastChild;
            }
            if (refChild == null)
            {
                return true;
            }
            switch (newChild.NodeType)
            {
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return true;

                case XmlNodeType.DocumentType:
                    return !this.HasNodeTypeInPrevSiblings(XmlNodeType.Element, refChild);

                case XmlNodeType.Element:
                    return !this.HasNodeTypeInNextSiblings(XmlNodeType.DocumentType, refChild.NextSibling);
            }
            return false;
        }

        internal override bool CanInsertBefore(XmlNode newChild, XmlNode refChild)
        {
            if (refChild == null)
            {
                refChild = this.FirstChild;
            }
            if (refChild == null)
            {
                return true;
            }
            switch (newChild.NodeType)
            {
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                    return (refChild.NodeType != XmlNodeType.XmlDeclaration);

                case XmlNodeType.DocumentType:
                    if (refChild.NodeType == XmlNodeType.XmlDeclaration)
                    {
                        break;
                    }
                    return !this.HasNodeTypeInPrevSiblings(XmlNodeType.Element, refChild.PreviousSibling);

                case XmlNodeType.Element:
                    if (refChild.NodeType != XmlNodeType.XmlDeclaration)
                    {
                        return !this.HasNodeTypeInNextSiblings(XmlNodeType.DocumentType, refChild);
                    }
                    break;

                case XmlNodeType.XmlDeclaration:
                    return (refChild == this.FirstChild);
            }
            return false;
        }

        internal static void CheckName(string name)
        {
            int invCharIndex = ValidateNames.ParseNmtoken(name, 0);
            if (invCharIndex < name.Length)
            {
                throw new XmlException("Xml_BadNameChar", XmlException.BuildCharExceptionArgs(name, invCharIndex));
            }
        }

        public override XmlNode CloneNode(bool deep)
        {
            XmlDocument toNode = this.Implementation.CreateDocument();
            toNode.SetBaseURI(this.baseURI);
            if (deep)
            {
                toNode.ImportChildren(this, toNode, deep);
            }
            return toNode;
        }

        internal XmlNodeType ConvertToNodeType(string nodeTypeString)
        {
            if (nodeTypeString == "element")
            {
                return XmlNodeType.Element;
            }
            if (nodeTypeString == "attribute")
            {
                return XmlNodeType.Attribute;
            }
            if (nodeTypeString == "text")
            {
                return XmlNodeType.Text;
            }
            if (nodeTypeString == "cdatasection")
            {
                return XmlNodeType.CDATA;
            }
            if (nodeTypeString == "entityreference")
            {
                return XmlNodeType.EntityReference;
            }
            if (nodeTypeString == "entity")
            {
                return XmlNodeType.Entity;
            }
            if (nodeTypeString == "processinginstruction")
            {
                return XmlNodeType.ProcessingInstruction;
            }
            if (nodeTypeString == "comment")
            {
                return XmlNodeType.Comment;
            }
            if (nodeTypeString == "document")
            {
                return XmlNodeType.Document;
            }
            if (nodeTypeString == "documenttype")
            {
                return XmlNodeType.DocumentType;
            }
            if (nodeTypeString == "documentfragment")
            {
                return XmlNodeType.DocumentFragment;
            }
            if (nodeTypeString == "notation")
            {
                return XmlNodeType.Notation;
            }
            if (nodeTypeString == "significantwhitespace")
            {
                return XmlNodeType.SignificantWhitespace;
            }
            if (nodeTypeString != "whitespace")
            {
                throw new ArgumentException(Res.GetString("Xdom_Invalid_NT_String", new object[] { nodeTypeString }));
            }
            return XmlNodeType.Whitespace;
        }

        public XmlAttribute CreateAttribute(string name)
        {
            string prefix = string.Empty;
            string localName = string.Empty;
            string namespaceURI = string.Empty;
            XmlNode.SplitName(name, out prefix, out localName);
            this.SetDefaultNamespace(prefix, localName, ref namespaceURI);
            return this.CreateAttribute(prefix, localName, namespaceURI);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public XmlAttribute CreateAttribute(string qualifiedName, string namespaceURI)
        {
            string prefix = string.Empty;
            string localName = string.Empty;
            XmlNode.SplitName(qualifiedName, out prefix, out localName);
            return this.CreateAttribute(prefix, localName, namespaceURI);
        }

        public virtual XmlAttribute CreateAttribute(string prefix, string localName, string namespaceURI)
        {
            return new XmlAttribute(this.AddAttrXmlName(prefix, localName, namespaceURI, null), this);
        }

        public virtual XmlCDataSection CreateCDataSection(string data)
        {
            this.fCDataNodesPresent = true;
            return new XmlCDataSection(data, this);
        }

        public virtual XmlComment CreateComment(string data)
        {
            return new XmlComment(data, this);
        }

        protected internal virtual XmlAttribute CreateDefaultAttribute(string prefix, string localName, string namespaceURI)
        {
            return new XmlUnspecifiedAttribute(prefix, localName, namespaceURI, this);
        }

        public virtual XmlDocumentFragment CreateDocumentFragment()
        {
            return new XmlDocumentFragment(this);
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        public virtual XmlDocumentType CreateDocumentType(string name, string publicId, string systemId, string internalSubset)
        {
            return new XmlDocumentType(name, publicId, systemId, internalSubset, this);
        }

        public XmlElement CreateElement(string name)
        {
            string prefix = string.Empty;
            string localName = string.Empty;
            XmlNode.SplitName(name, out prefix, out localName);
            return this.CreateElement(prefix, localName, string.Empty);
        }

        public XmlElement CreateElement(string qualifiedName, string namespaceURI)
        {
            string prefix = string.Empty;
            string localName = string.Empty;
            XmlNode.SplitName(qualifiedName, out prefix, out localName);
            return this.CreateElement(prefix, localName, namespaceURI);
        }

        public virtual XmlElement CreateElement(string prefix, string localName, string namespaceURI)
        {
            XmlElement elem = new XmlElement(this.AddXmlName(prefix, localName, namespaceURI, null), true, this);
            if (!this.IsLoading)
            {
                this.AddDefaultAttributes(elem);
            }
            return elem;
        }

        public virtual XmlEntityReference CreateEntityReference(string name)
        {
            return new XmlEntityReference(name, this);
        }

        public override XPathNavigator CreateNavigator()
        {
            return this.CreateNavigator(this);
        }

        protected internal virtual XPathNavigator CreateNavigator(XmlNode node)
        {
            XmlNode parentNode;
            switch (node.NodeType)
            {
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.SignificantWhitespace:
                    parentNode = node.ParentNode;
                    if (parentNode == null)
                    {
                        break;
                    }
                Label_005B:
                    switch (parentNode.NodeType)
                    {
                        case XmlNodeType.Attribute:
                            return null;

                        case XmlNodeType.EntityReference:
                            parentNode = parentNode.ParentNode;
                            if (parentNode != null)
                            {
                                goto Label_005B;
                            }
                            goto Label_0076;
                    }
                    break;

                case XmlNodeType.EntityReference:
                case XmlNodeType.Entity:
                case XmlNodeType.DocumentType:
                case XmlNodeType.Notation:
                case XmlNodeType.XmlDeclaration:
                    return null;

                case XmlNodeType.Whitespace:
                    XmlNodeType type2;
                    parentNode = node.ParentNode;
                    if (parentNode == null)
                    {
                        goto Label_00AB;
                    }
                Label_008B:
                    type2 = parentNode.NodeType;
                    switch (type2)
                    {
                        case XmlNodeType.Document:
                        case XmlNodeType.Attribute:
                            return null;
                    }
                    if (type2 == XmlNodeType.EntityReference)
                    {
                        parentNode = parentNode.ParentNode;
                        if (parentNode != null)
                        {
                            goto Label_008B;
                        }
                    }
                    goto Label_00AB;

                default:
                    goto Label_00B4;
            }
        Label_0076:
            node = this.NormalizeText(node);
            goto Label_00B4;
        Label_00AB:
            node = this.NormalizeText(node);
        Label_00B4:
            return new DocumentXPathNavigator(this, node);
        }

        public virtual XmlNode CreateNode(string nodeTypeString, string name, string namespaceURI)
        {
            return this.CreateNode(this.ConvertToNodeType(nodeTypeString), name, namespaceURI);
        }

        public virtual XmlNode CreateNode(XmlNodeType type, string name, string namespaceURI)
        {
            return this.CreateNode(type, null, name, namespaceURI);
        }

        public virtual XmlNode CreateNode(XmlNodeType type, string prefix, string name, string namespaceURI)
        {
            switch (type)
            {
                case XmlNodeType.Element:
                    if (prefix == null)
                    {
                        return this.CreateElement(name, namespaceURI);
                    }
                    return this.CreateElement(prefix, name, namespaceURI);

                case XmlNodeType.Attribute:
                    if (prefix == null)
                    {
                        return this.CreateAttribute(name, namespaceURI);
                    }
                    return this.CreateAttribute(prefix, name, namespaceURI);

                case XmlNodeType.Text:
                    return this.CreateTextNode(string.Empty);

                case XmlNodeType.CDATA:
                    return this.CreateCDataSection(string.Empty);

                case XmlNodeType.EntityReference:
                    return this.CreateEntityReference(name);

                case XmlNodeType.ProcessingInstruction:
                    return this.CreateProcessingInstruction(name, string.Empty);

                case XmlNodeType.Comment:
                    return this.CreateComment(string.Empty);

                case XmlNodeType.Document:
                    return new XmlDocument();

                case XmlNodeType.DocumentType:
                    return this.CreateDocumentType(name, string.Empty, string.Empty, string.Empty);

                case XmlNodeType.DocumentFragment:
                    return this.CreateDocumentFragment();

                case XmlNodeType.Whitespace:
                    return this.CreateWhitespace(string.Empty);

                case XmlNodeType.SignificantWhitespace:
                    return this.CreateSignificantWhitespace(string.Empty);

                case XmlNodeType.XmlDeclaration:
                    return this.CreateXmlDeclaration("1.0", null, null);
            }
            throw new ArgumentException(Res.GetString("Arg_CannotCreateNode", new object[] { type }));
        }

        public virtual XmlProcessingInstruction CreateProcessingInstruction(string target, string data)
        {
            return new XmlProcessingInstruction(target, data, this);
        }

        public virtual XmlSignificantWhitespace CreateSignificantWhitespace(string text)
        {
            return new XmlSignificantWhitespace(text, this);
        }

        public virtual XmlText CreateTextNode(string text)
        {
            return new XmlText(text, this);
        }

        public virtual XmlWhitespace CreateWhitespace(string text)
        {
            return new XmlWhitespace(text, this);
        }

        public virtual XmlDeclaration CreateXmlDeclaration(string version, string encoding, string standalone)
        {
            return new XmlDeclaration(version, encoding, standalone, this);
        }

        internal XmlAttribute GetDefaultAttribute(XmlElement elem, string attrPrefix, string attrLocalname, string attrNamespaceURI)
        {
            System.Xml.Schema.SchemaInfo dtdSchemaInfo = this.DtdSchemaInfo;
            SchemaElementDecl schemaElementDecl = this.GetSchemaElementDecl(elem);
            if ((schemaElementDecl != null) && (schemaElementDecl.AttDefs != null))
            {
                IDictionaryEnumerator enumerator = schemaElementDecl.AttDefs.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    SchemaAttDef attdef = (SchemaAttDef) enumerator.Value;
                    if ((((attdef.Presence == SchemaDeclBase.Use.Default) || (attdef.Presence == SchemaDeclBase.Use.Fixed)) && (attdef.Name.Name == attrLocalname)) && (((dtdSchemaInfo.SchemaType == SchemaType.DTD) && (attdef.Name.Namespace == attrPrefix)) || ((dtdSchemaInfo.SchemaType != SchemaType.DTD) && (attdef.Name.Namespace == attrNamespaceURI))))
                    {
                        return this.PrepareDefaultAttribute(attdef, attrPrefix, attrLocalname, attrNamespaceURI);
                    }
                }
            }
            return null;
        }

        private WeakReference GetElement(ArrayList elementList, XmlElement elem)
        {
            ArrayList list = new ArrayList();
            foreach (WeakReference reference in elementList)
            {
                if (!reference.IsAlive)
                {
                    list.Add(reference);
                }
                else if (((XmlElement) reference.Target) == elem)
                {
                    return reference;
                }
            }
            foreach (WeakReference reference2 in list)
            {
                elementList.Remove(reference2);
            }
            return null;
        }

        public virtual XmlElement GetElementById(string elementId)
        {
            if (this.htElementIdMap != null)
            {
                ArrayList list = (ArrayList) this.htElementIdMap[elementId];
                if (list != null)
                {
                    foreach (WeakReference reference in list)
                    {
                        XmlElement target = (XmlElement) reference.Target;
                        if ((target != null) && target.IsConnected())
                        {
                            return target;
                        }
                    }
                }
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

        internal XmlEntity GetEntityNode(string name)
        {
            if (this.DocumentType != null)
            {
                XmlNamedNodeMap entities = this.DocumentType.Entities;
                if (entities != null)
                {
                    return (XmlEntity) entities.GetNamedItem(name);
                }
            }
            return null;
        }

        internal override XmlNodeChangedEventArgs GetEventArgs(XmlNode node, XmlNode oldParent, XmlNode newParent, string oldValue, string newValue, XmlNodeChangedAction action)
        {
            this.reportValidity = false;
            switch (action)
            {
                case XmlNodeChangedAction.Insert:
                    if ((this.onNodeInsertingDelegate != null) || (this.onNodeInsertedDelegate != null))
                    {
                        break;
                    }
                    return null;

                case XmlNodeChangedAction.Remove:
                    if ((this.onNodeRemovingDelegate != null) || (this.onNodeRemovedDelegate != null))
                    {
                        break;
                    }
                    return null;

                case XmlNodeChangedAction.Change:
                    if ((this.onNodeChangingDelegate != null) || (this.onNodeChangedDelegate != null))
                    {
                        break;
                    }
                    return null;
            }
            return new XmlNodeChangedEventArgs(node, oldParent, newParent, oldValue, newValue, action);
        }

        internal XmlName GetIDInfoByElement(XmlName eleName)
        {
            if (this.htElementIDAttrDecl == null)
            {
                return null;
            }
            return this.GetIDInfoByElement_(eleName);
        }

        private XmlName GetIDInfoByElement_(XmlName eleName)
        {
            XmlName name = this.GetXmlName(eleName.Prefix, eleName.LocalName, string.Empty, null);
            if (name != null)
            {
                return (XmlName) this.htElementIDAttrDecl[name];
            }
            return null;
        }

        internal XmlNodeChangedEventArgs GetInsertEventArgsForLoad(XmlNode node, XmlNode newParent)
        {
            if ((this.onNodeInsertingDelegate == null) && (this.onNodeInsertedDelegate == null))
            {
                return null;
            }
            string oldValue = node.Value;
            return new XmlNodeChangedEventArgs(node, null, newParent, oldValue, oldValue, XmlNodeChangedAction.Insert);
        }

        internal System.Xml.XmlResolver GetResolver()
        {
            return this.resolver;
        }

        private SchemaElementDecl GetSchemaElementDecl(XmlElement elem)
        {
            System.Xml.Schema.SchemaInfo dtdSchemaInfo = this.DtdSchemaInfo;
            if (dtdSchemaInfo != null)
            {
                SchemaElementDecl decl;
                XmlQualifiedName key = new XmlQualifiedName(elem.LocalName, (dtdSchemaInfo.SchemaType == SchemaType.DTD) ? elem.Prefix : elem.NamespaceURI);
                if (dtdSchemaInfo.ElementDecls.TryGetValue(key, out decl))
                {
                    return decl;
                }
            }
            return null;
        }

        internal XmlName GetXmlName(string prefix, string localName, string namespaceURI, IXmlSchemaInfo schemaInfo)
        {
            return this.domNameTable.GetName(prefix, localName, namespaceURI, schemaInfo);
        }

        private bool HasNodeTypeInNextSiblings(XmlNodeType nt, XmlNode refNode)
        {
            for (XmlNode node = refNode; node != null; node = node.NextSibling)
            {
                if (node.NodeType == nt)
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasNodeTypeInPrevSiblings(XmlNodeType nt, XmlNode refNode)
        {
            if (refNode != null)
            {
                XmlNode firstChild = null;
                if (refNode.ParentNode != null)
                {
                    firstChild = refNode.ParentNode.FirstChild;
                }
                while (firstChild != null)
                {
                    if (firstChild.NodeType == nt)
                    {
                        return true;
                    }
                    if (firstChild == refNode)
                    {
                        break;
                    }
                    firstChild = firstChild.NextSibling;
                }
            }
            return false;
        }

        private void ImportAttributes(XmlNode fromElem, XmlNode toElem)
        {
            int count = fromElem.Attributes.Count;
            for (int i = 0; i < count; i++)
            {
                if (fromElem.Attributes[i].Specified)
                {
                    toElem.Attributes.SetNamedItem(this.ImportNodeInternal(fromElem.Attributes[i], true));
                }
            }
        }

        private void ImportChildren(XmlNode fromNode, XmlNode toNode, bool deep)
        {
            for (XmlNode node = fromNode.FirstChild; node != null; node = node.NextSibling)
            {
                toNode.AppendChild(this.ImportNodeInternal(node, deep));
            }
        }

        public virtual XmlNode ImportNode(XmlNode node, bool deep)
        {
            return this.ImportNodeInternal(node, deep);
        }

        private XmlNode ImportNodeInternal(XmlNode node, bool deep)
        {
            XmlNode toElem = null;
            if (node == null)
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Import_NullNode"));
            }
            switch (node.NodeType)
            {
                case XmlNodeType.Element:
                    toElem = this.CreateElement(node.Prefix, node.LocalName, node.NamespaceURI);
                    this.ImportAttributes(node, toElem);
                    if (deep)
                    {
                        this.ImportChildren(node, toElem, deep);
                    }
                    return toElem;

                case XmlNodeType.Attribute:
                    toElem = this.CreateAttribute(node.Prefix, node.LocalName, node.NamespaceURI);
                    this.ImportChildren(node, toElem, true);
                    return toElem;

                case XmlNodeType.Text:
                    return this.CreateTextNode(node.Value);

                case XmlNodeType.CDATA:
                    return this.CreateCDataSection(node.Value);

                case XmlNodeType.EntityReference:
                    return this.CreateEntityReference(node.Name);

                case XmlNodeType.ProcessingInstruction:
                    return this.CreateProcessingInstruction(node.Name, node.Value);

                case XmlNodeType.Comment:
                    return this.CreateComment(node.Value);

                case XmlNodeType.DocumentType:
                {
                    XmlDocumentType type = (XmlDocumentType) node;
                    return this.CreateDocumentType(type.Name, type.PublicId, type.SystemId, type.InternalSubset);
                }
                case XmlNodeType.DocumentFragment:
                    toElem = this.CreateDocumentFragment();
                    if (deep)
                    {
                        this.ImportChildren(node, toElem, deep);
                    }
                    return toElem;

                case XmlNodeType.Whitespace:
                    return this.CreateWhitespace(node.Value);

                case XmlNodeType.SignificantWhitespace:
                    return this.CreateSignificantWhitespace(node.Value);

                case XmlNodeType.XmlDeclaration:
                {
                    XmlDeclaration declaration = (XmlDeclaration) node;
                    return this.CreateXmlDeclaration(declaration.Version, declaration.Encoding, declaration.Standalone);
                }
            }
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Res.GetString("Xdom_Import"), new object[] { node.NodeType.ToString() }));
        }

        internal static bool IsTextNode(XmlNodeType nt)
        {
            switch (nt)
            {
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return true;
            }
            return false;
        }

        internal override bool IsValidChildType(XmlNodeType type)
        {
            switch (type)
            {
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return true;

                case XmlNodeType.DocumentType:
                    if (this.DocumentType != null)
                    {
                        throw new InvalidOperationException(Res.GetString("Xdom_DualDocumentTypeNode"));
                    }
                    return true;

                case XmlNodeType.XmlDeclaration:
                    if (this.Declaration != null)
                    {
                        throw new InvalidOperationException(Res.GetString("Xdom_DualDeclarationNode"));
                    }
                    return true;

                case XmlNodeType.Element:
                    if (this.DocumentElement != null)
                    {
                        throw new InvalidOperationException(Res.GetString("Xdom_DualDocumentElementNode"));
                    }
                    return true;
            }
            return false;
        }

        public virtual void Load(Stream inStream)
        {
            XmlTextReader reader = this.SetupReader(new XmlTextReader(inStream, this.NameTable));
            try
            {
                this.Load(reader);
            }
            finally
            {
                reader.Impl.Close(false);
            }
        }

        public virtual void Load(TextReader txtReader)
        {
            XmlTextReader reader = this.SetupReader(new XmlTextReader(txtReader, this.NameTable));
            try
            {
                this.Load(reader);
            }
            finally
            {
                reader.Impl.Close(false);
            }
        }

        public virtual void Load(string filename)
        {
            XmlTextReader reader = this.SetupReader(new XmlTextReader(filename, this.NameTable));
            try
            {
                this.Load(reader);
            }
            finally
            {
                reader.Close();
            }
        }

        public virtual void Load(XmlReader reader)
        {
            try
            {
                this.IsLoading = true;
                this.actualLoadingStatus = true;
                this.RemoveAll();
                this.fEntRefNodesPresent = false;
                this.fCDataNodesPresent = false;
                this.reportValidity = true;
                new XmlLoader().Load(this, reader, this.preserveWhitespace);
            }
            finally
            {
                this.IsLoading = false;
                this.actualLoadingStatus = false;
                this.reportValidity = true;
            }
        }

        public virtual void LoadXml(string xml)
        {
            XmlTextReader reader = this.SetupReader(new XmlTextReader(new StringReader(xml), this.NameTable));
            try
            {
                this.Load(reader);
            }
            finally
            {
                reader.Close();
            }
        }

        private XmlNode NormalizeText(XmlNode n)
        {
            XmlNode node = null;
            while (IsTextNode(n.NodeType))
            {
                node = n;
                n = n.PreviousSibling;
                if (n == null)
                {
                    XmlNode parentNode = node;
                    do
                    {
                        if ((parentNode.ParentNode == null) || (parentNode.ParentNode.NodeType != XmlNodeType.EntityReference))
                        {
                            break;
                        }
                        if (parentNode.ParentNode.PreviousSibling != null)
                        {
                            n = parentNode.ParentNode.PreviousSibling;
                            break;
                        }
                        parentNode = parentNode.ParentNode;
                    }
                    while (parentNode != null);
                }
                if (n != null)
                {
                    goto Label_005C;
                }
                return node;
            Label_0054:
                n = n.LastChild;
            Label_005C:
                if (n.NodeType == XmlNodeType.EntityReference)
                {
                    goto Label_0054;
                }
            }
            return node;
        }

        private XmlAttribute PrepareDefaultAttribute(SchemaAttDef attdef, string attrPrefix, string attrLocalname, string attrNamespaceURI)
        {
            this.SetDefaultNamespace(attrPrefix, attrLocalname, ref attrNamespaceURI);
            XmlAttribute attribute = this.CreateDefaultAttribute(attrPrefix, attrLocalname, attrNamespaceURI);
            attribute.InnerXml = attdef.DefaultValueRaw;
            XmlUnspecifiedAttribute attribute2 = attribute as XmlUnspecifiedAttribute;
            if (attribute2 != null)
            {
                attribute2.SetSpecified(false);
            }
            return attribute;
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        public virtual XmlNode ReadNode(XmlReader reader)
        {
            XmlNode node = null;
            try
            {
                this.IsLoading = true;
                node = new XmlLoader().ReadCurrentNode(this, reader);
            }
            finally
            {
                this.IsLoading = false;
            }
            return node;
        }

        internal void RemoveElementWithId(string id, XmlElement elem)
        {
            if ((this.htElementIdMap != null) && this.htElementIdMap.Contains(id))
            {
                ArrayList elementList = (ArrayList) this.htElementIdMap[id];
                WeakReference element = this.GetElement(elementList, elem);
                if (element != null)
                {
                    elementList.Remove(element);
                    if (elementList.Count == 0)
                    {
                        this.htElementIdMap.Remove(id);
                    }
                }
            }
        }

        public virtual void Save(Stream outStream)
        {
            XmlDOMTextWriter w = new XmlDOMTextWriter(outStream, this.TextEncoding);
            if (!this.preserveWhitespace)
            {
                w.Formatting = Formatting.Indented;
            }
            this.WriteTo(w);
            w.Flush();
        }

        public virtual void Save(TextWriter writer)
        {
            XmlDOMTextWriter w = new XmlDOMTextWriter(writer);
            if (!this.preserveWhitespace)
            {
                w.Formatting = Formatting.Indented;
            }
            this.Save(w);
        }

        public virtual void Save(string filename)
        {
            if (this.DocumentElement == null)
            {
                throw new XmlException("Xml_InvalidXmlDocument", Res.GetString("Xdom_NoRootEle"));
            }
            XmlDOMTextWriter w = new XmlDOMTextWriter(filename, this.TextEncoding);
            try
            {
                if (!this.preserveWhitespace)
                {
                    w.Formatting = Formatting.Indented;
                }
                this.WriteTo(w);
                w.Flush();
            }
            finally
            {
                w.Close();
            }
        }

        public virtual void Save(XmlWriter w)
        {
            XmlNode firstChild = this.FirstChild;
            if (firstChild != null)
            {
                if (w.WriteState == WriteState.Start)
                {
                    if (firstChild is XmlDeclaration)
                    {
                        if (this.Standalone.Length == 0)
                        {
                            w.WriteStartDocument();
                        }
                        else if (this.Standalone == "yes")
                        {
                            w.WriteStartDocument(true);
                        }
                        else if (this.Standalone == "no")
                        {
                            w.WriteStartDocument(false);
                        }
                        firstChild = firstChild.NextSibling;
                    }
                    else
                    {
                        w.WriteStartDocument();
                    }
                }
                while (firstChild != null)
                {
                    firstChild.WriteTo(w);
                    firstChild = firstChild.NextSibling;
                }
                w.Flush();
            }
        }

        internal void SetBaseURI(string inBaseURI)
        {
            this.baseURI = inBaseURI;
        }

        internal void SetDefaultNamespace(string prefix, string localName, ref string namespaceURI)
        {
            if ((prefix == this.strXmlns) || ((prefix.Length == 0) && (localName == this.strXmlns)))
            {
                namespaceURI = this.strReservedXmlns;
            }
            else if (prefix == this.strXml)
            {
                namespaceURI = this.strReservedXml;
            }
        }

        private XmlTextReader SetupReader(XmlTextReader tr)
        {
            tr.XmlValidatingReaderCompatibilityMode = true;
            tr.EntityHandling = EntityHandling.ExpandCharEntities;
            if (this.HasSetResolver)
            {
                tr.XmlResolver = this.GetResolver();
            }
            return tr;
        }

        public void Validate(ValidationEventHandler validationEventHandler)
        {
            this.Validate(validationEventHandler, this);
        }

        public void Validate(ValidationEventHandler validationEventHandler, XmlNode nodeToValidate)
        {
            if ((this.schemas == null) || (this.schemas.Count == 0))
            {
                throw new InvalidOperationException(Res.GetString("XmlDocument_NoSchemaInfo"));
            }
            if (nodeToValidate.Document != this)
            {
                throw new ArgumentException(Res.GetString("XmlDocument_NodeNotFromDocument", new object[] { "nodeToValidate" }));
            }
            if (nodeToValidate == this)
            {
                this.reportValidity = false;
            }
            new DocumentSchemaValidator(this, this.schemas, validationEventHandler).Validate(nodeToValidate);
            if (nodeToValidate == this)
            {
                this.reportValidity = true;
            }
        }

        public override void WriteContentTo(XmlWriter xw)
        {
            foreach (XmlNode node in this)
            {
                node.WriteTo(xw);
            }
        }

        public override void WriteTo(XmlWriter w)
        {
            this.WriteContentTo(w);
        }

        internal bool ActualLoadingStatus
        {
            get
            {
                return this.actualLoadingStatus;
            }
            set
            {
                this.actualLoadingStatus = value;
            }
        }

        public override string BaseURI
        {
            get
            {
                return this.baseURI;
            }
        }

        internal bool CanReportValidity
        {
            get
            {
                return this.reportValidity;
            }
        }

        internal virtual XmlDeclaration Declaration
        {
            get
            {
                if (this.HasChildNodes)
                {
                    return (this.FirstChild as XmlDeclaration);
                }
                return null;
            }
        }

        public XmlElement DocumentElement
        {
            get
            {
                return (XmlElement) this.FindChild(XmlNodeType.Element);
            }
        }

        public virtual XmlDocumentType DocumentType
        {
            get
            {
                return (XmlDocumentType) this.FindChild(XmlNodeType.DocumentType);
            }
        }

        internal System.Xml.Schema.SchemaInfo DtdSchemaInfo
        {
            get
            {
                return this.schemaInfo;
            }
            set
            {
                this.schemaInfo = value;
            }
        }

        internal string Encoding
        {
            get
            {
                XmlDeclaration declaration = this.Declaration;
                if (declaration != null)
                {
                    return declaration.Encoding;
                }
                return null;
            }
        }

        internal XmlNamedNodeMap Entities
        {
            get
            {
                if (this.entities == null)
                {
                    this.entities = new XmlNamedNodeMap(this);
                }
                return this.entities;
            }
            set
            {
                this.entities = value;
            }
        }

        internal bool HasEntityReferences
        {
            get
            {
                return this.fEntRefNodesPresent;
            }
        }

        internal bool HasSetResolver
        {
            get
            {
                return this.bSetResolver;
            }
        }

        public XmlImplementation Implementation
        {
            get
            {
                return this.implementation;
            }
        }

        public override string InnerText
        {
            set
            {
                throw new InvalidOperationException(Res.GetString("Xdom_Document_Innertext"));
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
                this.LoadXml(value);
            }
        }

        internal override bool IsContainer
        {
            get
            {
                return true;
            }
        }

        internal bool IsLoading
        {
            get
            {
                return this.isLoading;
            }
            set
            {
                this.isLoading = value;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        internal override XmlLinkedNode LastNode
        {
            get
            {
                return this.lastChild;
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
                return this.strDocumentName;
            }
        }

        public override string Name
        {
            get
            {
                return this.strDocumentName;
            }
        }

        internal XmlAttribute NamespaceXml
        {
            get
            {
                if (this.namespaceXml == null)
                {
                    this.namespaceXml = new XmlAttribute(this.AddAttrXmlName(this.strXmlns, this.strXml, this.strReservedXmlns, null), this);
                    this.namespaceXml.Value = this.strReservedXml;
                }
                return this.namespaceXml;
            }
        }

        public XmlNameTable NameTable
        {
            get
            {
                return this.implementation.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return XmlNodeType.Document;
            }
        }

        public override XmlDocument OwnerDocument
        {
            get
            {
                return null;
            }
        }

        public override XmlNode ParentNode
        {
            get
            {
                return null;
            }
        }

        public bool PreserveWhitespace
        {
            get
            {
                return this.preserveWhitespace;
            }
            set
            {
                this.preserveWhitespace = value;
            }
        }

        public override IXmlSchemaInfo SchemaInfo
        {
            get
            {
                if (this.reportValidity)
                {
                    XmlElement documentElement = this.DocumentElement;
                    if (documentElement != null)
                    {
                        switch (documentElement.SchemaInfo.Validity)
                        {
                            case XmlSchemaValidity.Valid:
                                return ValidSchemaInfo;

                            case XmlSchemaValidity.Invalid:
                                return InvalidSchemaInfo;
                        }
                    }
                }
                return NotKnownSchemaInfo;
            }
        }

        public XmlSchemaSet Schemas
        {
            get
            {
                if (this.schemas == null)
                {
                    this.schemas = new XmlSchemaSet(this.NameTable);
                }
                return this.schemas;
            }
            set
            {
                this.schemas = value;
            }
        }

        internal string Standalone
        {
            get
            {
                XmlDeclaration declaration = this.Declaration;
                if (declaration != null)
                {
                    return declaration.Standalone;
                }
                return null;
            }
        }

        internal System.Text.Encoding TextEncoding
        {
            get
            {
                if (this.Declaration != null)
                {
                    string encoding = this.Declaration.Encoding;
                    if (encoding.Length > 0)
                    {
                        return System.Text.Encoding.GetEncoding(encoding);
                    }
                }
                return null;
            }
        }

        internal string Version
        {
            get
            {
                XmlDeclaration declaration = this.Declaration;
                if (declaration != null)
                {
                    return declaration.Version;
                }
                return null;
            }
        }

        public virtual System.Xml.XmlResolver XmlResolver
        {
            set
            {
                if (value != null)
                {
                    try
                    {
                        new NamedPermissionSet("FullTrust").Demand();
                    }
                    catch (SecurityException exception)
                    {
                        throw new SecurityException(Res.GetString("Xml_UntrustedCodeSettingResolver"), exception);
                    }
                }
                this.resolver = value;
                if (!this.bSetResolver)
                {
                    this.bSetResolver = true;
                }
                XmlDocumentType documentType = this.DocumentType;
                if (documentType != null)
                {
                    documentType.DtdSchemaInfo = null;
                }
            }
        }

        internal override XPathNodeType XPNodeType
        {
            get
            {
                return XPathNodeType.Root;
            }
        }
    }
}

