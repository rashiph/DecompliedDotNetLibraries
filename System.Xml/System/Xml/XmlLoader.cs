namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Xml.Schema;

    internal class XmlLoader
    {
        private XmlDocument doc;
        private bool preserveWhitespace;
        private XmlReader reader;

        private XmlReader CreateInnerXmlReader(string xmlFragment, XmlNodeType nt, XmlParserContext context, XmlDocument doc)
        {
            XmlNodeType fragType = nt;
            switch (fragType)
            {
                case XmlNodeType.Entity:
                case XmlNodeType.EntityReference:
                    fragType = XmlNodeType.Element;
                    break;
            }
            XmlTextReaderImpl reader = new XmlTextReaderImpl(xmlFragment, fragType, context) {
                XmlValidatingReaderCompatibilityMode = true
            };
            if (doc.HasSetResolver)
            {
                reader.XmlResolver = doc.GetResolver();
            }
            if (!doc.ActualLoadingStatus)
            {
                reader.DisableUndeclaredEntityCheck = true;
            }
            XmlDocumentType documentType = doc.DocumentType;
            if (documentType != null)
            {
                reader.Namespaces = documentType.ParseWithNamespaces;
                if (documentType.DtdSchemaInfo != null)
                {
                    reader.SetDtdInfo(documentType.DtdSchemaInfo);
                }
                else
                {
                    IDtdParser parser = DtdParser.Create();
                    XmlTextReaderImpl.DtdParserProxy adapter = new XmlTextReaderImpl.DtdParserProxy(reader);
                    IDtdInfo newDtdInfo = parser.ParseFreeFloatingDtd(context.BaseURI, context.DocTypeName, context.PublicId, context.SystemId, context.InternalSubset, adapter);
                    documentType.DtdSchemaInfo = newDtdInfo as SchemaInfo;
                    reader.SetDtdInfo(newDtdInfo);
                }
            }
            if ((nt == XmlNodeType.Entity) || (nt == XmlNodeType.EntityReference))
            {
                reader.Read();
                reader.ResolveEntity();
            }
            return reader;
        }

        private string EntitizeName(string name)
        {
            return ("&" + name + ";");
        }

        internal void ExpandEntity(XmlEntity ent)
        {
            this.ParsePartialContent(ent, this.EntitizeName(ent.Name), XmlNodeType.Entity);
        }

        internal void ExpandEntityReference(XmlEntityReference eref)
        {
            this.doc = eref.OwnerDocument;
            bool isLoading = this.doc.IsLoading;
            this.doc.IsLoading = true;
            switch (eref.Name)
            {
                case "lt":
                    eref.AppendChildForLoad(this.doc.CreateTextNode("<"), this.doc);
                    this.doc.IsLoading = isLoading;
                    return;

                case "gt":
                    eref.AppendChildForLoad(this.doc.CreateTextNode(">"), this.doc);
                    this.doc.IsLoading = isLoading;
                    return;

                case "amp":
                    eref.AppendChildForLoad(this.doc.CreateTextNode("&"), this.doc);
                    this.doc.IsLoading = isLoading;
                    return;

                case "apos":
                    eref.AppendChildForLoad(this.doc.CreateTextNode("'"), this.doc);
                    this.doc.IsLoading = isLoading;
                    return;

                case "quot":
                    eref.AppendChildForLoad(this.doc.CreateTextNode("\""), this.doc);
                    this.doc.IsLoading = isLoading;
                    return;
            }
            foreach (XmlEntity entity in this.doc.Entities)
            {
                if (Ref.Equal(entity.Name, eref.Name))
                {
                    this.ParsePartialContent(eref, this.EntitizeName(eref.Name), XmlNodeType.EntityReference);
                    return;
                }
            }
            if (!this.doc.ActualLoadingStatus)
            {
                eref.AppendChildForLoad(this.doc.CreateTextNode(""), this.doc);
                this.doc.IsLoading = isLoading;
            }
            else
            {
                this.doc.IsLoading = isLoading;
                throw new XmlException("Xml_UndeclaredParEntity", eref.Name);
            }
        }

        private XmlParserContext GetContext(XmlNode node)
        {
            string xmlLang = null;
            XmlSpace none = XmlSpace.None;
            XmlDocumentType documentType = this.doc.DocumentType;
            string baseURI = this.doc.BaseURI;
            Hashtable hashtable = new Hashtable();
            XmlNameTable nameTable = this.doc.NameTable;
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(nameTable);
            bool flag = false;
            while ((node != null) && (node != this.doc))
            {
                if ((node is XmlElement) && ((XmlElement) node).HasAttributes)
                {
                    nsMgr.PushScope();
                    foreach (XmlAttribute attribute in ((XmlElement) node).Attributes)
                    {
                        if ((attribute.Prefix == this.doc.strXmlns) && !hashtable.Contains(attribute.LocalName))
                        {
                            hashtable.Add(attribute.LocalName, attribute.LocalName);
                            nsMgr.AddNamespace(attribute.LocalName, attribute.Value);
                        }
                        else if ((!flag && (attribute.Prefix.Length == 0)) && (attribute.LocalName == this.doc.strXmlns))
                        {
                            nsMgr.AddNamespace(string.Empty, attribute.Value);
                            flag = true;
                        }
                        else if (((none == XmlSpace.None) && (attribute.Prefix == this.doc.strXml)) && (attribute.LocalName == this.doc.strSpace))
                        {
                            if (attribute.Value == "default")
                            {
                                none = XmlSpace.Default;
                            }
                            else if (attribute.Value == "preserve")
                            {
                                none = XmlSpace.Preserve;
                            }
                        }
                        else if (((xmlLang == null) && (attribute.Prefix == this.doc.strXml)) && (attribute.LocalName == this.doc.strLang))
                        {
                            xmlLang = attribute.Value;
                        }
                    }
                }
                node = node.ParentNode;
            }
            return new XmlParserContext(nameTable, nsMgr, (documentType == null) ? null : documentType.Name, (documentType == null) ? null : documentType.PublicId, (documentType == null) ? null : documentType.SystemId, (documentType == null) ? null : documentType.InternalSubset, baseURI, xmlLang, none);
        }

        internal void Load(XmlDocument doc, XmlReader reader, bool preserveWhitespace)
        {
            this.doc = doc;
            if (reader.GetType() == typeof(XmlTextReader))
            {
                this.reader = ((XmlTextReader) reader).Impl;
            }
            else
            {
                this.reader = reader;
            }
            this.preserveWhitespace = preserveWhitespace;
            if (doc == null)
            {
                throw new ArgumentException(Res.GetString("Xdom_Load_NoDocument"));
            }
            if (reader == null)
            {
                throw new ArgumentException(Res.GetString("Xdom_Load_NoReader"));
            }
            doc.SetBaseURI(reader.BaseURI);
            if ((reader.Settings != null) && (reader.Settings.ValidationType == ValidationType.Schema))
            {
                doc.Schemas = reader.Settings.Schemas;
            }
            if ((this.reader.ReadState == ReadState.Interactive) || this.reader.Read())
            {
                this.LoadDocSequence(doc);
            }
        }

        private XmlAttribute LoadAttributeNode()
        {
            XmlReader reader = this.reader;
            if (reader.IsDefault)
            {
                return this.LoadDefaultAttribute();
            }
            XmlAttribute attribute = this.doc.CreateAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI);
            IXmlSchemaInfo schemaInfo = reader.SchemaInfo;
            if (schemaInfo != null)
            {
                attribute.XmlName = this.doc.AddAttrXmlName(attribute.Prefix, attribute.LocalName, attribute.NamespaceURI, schemaInfo);
            }
            while (reader.ReadAttributeValue())
            {
                XmlNode node;
                switch (reader.NodeType)
                {
                    case XmlNodeType.Text:
                        node = this.doc.CreateTextNode(reader.Value);
                        break;

                    case XmlNodeType.EntityReference:
                        node = this.doc.CreateEntityReference(reader.LocalName);
                        if (reader.CanResolveEntity)
                        {
                            reader.ResolveEntity();
                            this.LoadAttributeValue(node, false);
                            if (node.FirstChild == null)
                            {
                                node.AppendChildForLoad(this.doc.CreateTextNode(string.Empty), this.doc);
                            }
                        }
                        break;

                    default:
                        throw UnexpectedNodeType(reader.NodeType);
                }
                attribute.AppendChildForLoad(node, this.doc);
            }
            return attribute;
        }

        private XmlAttribute LoadAttributeNodeDirect()
        {
            XmlReader reader = this.reader;
            if (reader.IsDefault)
            {
                XmlUnspecifiedAttribute attribute2 = new XmlUnspecifiedAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI, this.doc);
                this.LoadAttributeValue(attribute2, true);
                attribute2.SetSpecified(false);
                return attribute2;
            }
            XmlAttribute parent = new XmlAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI, this.doc);
            this.LoadAttributeValue(parent, true);
            return parent;
        }

        private void LoadAttributeValue(XmlNode parent, bool direct)
        {
            XmlReader reader = this.reader;
            while (reader.ReadAttributeValue())
            {
                XmlNode node;
                switch (reader.NodeType)
                {
                    case XmlNodeType.Text:
                        node = direct ? new XmlText(reader.Value, this.doc) : this.doc.CreateTextNode(reader.Value);
                        break;

                    case XmlNodeType.EntityReference:
                        node = direct ? new XmlEntityReference(this.reader.LocalName, this.doc) : this.doc.CreateEntityReference(this.reader.LocalName);
                        if (reader.CanResolveEntity)
                        {
                            reader.ResolveEntity();
                            this.LoadAttributeValue(node, direct);
                            if (node.FirstChild == null)
                            {
                                node.AppendChildForLoad(direct ? new XmlText(string.Empty) : this.doc.CreateTextNode(string.Empty), this.doc);
                            }
                        }
                        break;

                    case XmlNodeType.EndEntity:
                        return;

                    default:
                        throw UnexpectedNodeType(reader.NodeType);
                }
                parent.AppendChildForLoad(node, this.doc);
            }
        }

        private XmlDeclaration LoadDeclarationNode()
        {
            string version = null;
            string encoding = null;
            string standalone = null;
            while (this.reader.MoveToNextAttribute())
            {
                string name = this.reader.Name;
                if (name != null)
                {
                    if (!(name == "version"))
                    {
                        if (name == "encoding")
                        {
                            goto Label_004E;
                        }
                        if (name == "standalone")
                        {
                            goto Label_005C;
                        }
                    }
                    else
                    {
                        version = this.reader.Value;
                    }
                }
                continue;
            Label_004E:
                encoding = this.reader.Value;
                continue;
            Label_005C:
                standalone = this.reader.Value;
            }
            if (version == null)
            {
                ParseXmlDeclarationValue(this.reader.Value, out version, out encoding, out standalone);
            }
            return this.doc.CreateXmlDeclaration(version, encoding, standalone);
        }

        private XmlAttribute LoadDefaultAttribute()
        {
            XmlReader reader = this.reader;
            XmlAttribute parent = this.doc.CreateDefaultAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI);
            IXmlSchemaInfo schemaInfo = reader.SchemaInfo;
            if (schemaInfo != null)
            {
                parent.XmlName = this.doc.AddAttrXmlName(parent.Prefix, parent.LocalName, parent.NamespaceURI, schemaInfo);
            }
            this.LoadAttributeValue(parent, false);
            XmlUnspecifiedAttribute attribute2 = parent as XmlUnspecifiedAttribute;
            if (attribute2 != null)
            {
                attribute2.SetSpecified(false);
            }
            return parent;
        }

        private void LoadDocSequence(XmlDocument parentDoc)
        {
            XmlNode newChild = null;
            while ((newChild = this.LoadNode(true)) != null)
            {
                parentDoc.AppendChildForLoad(newChild, parentDoc);
                if (!this.reader.Read())
                {
                    return;
                }
            }
        }

        private void LoadDocumentType(IDtdInfo dtdInfo, XmlDocumentType dtNode)
        {
            SchemaInfo info = dtdInfo as SchemaInfo;
            if (info == null)
            {
                throw new XmlException("Xml_InternalError", string.Empty);
            }
            dtNode.DtdSchemaInfo = info;
            if (info != null)
            {
                this.doc.DtdSchemaInfo = info;
                if (info.Notations != null)
                {
                    foreach (SchemaNotation notation in info.Notations.Values)
                    {
                        dtNode.Notations.SetNamedItem(new XmlNotation(notation.Name.Name, notation.Pubid, notation.SystemLiteral, this.doc));
                    }
                }
                if (info.GeneralEntities != null)
                {
                    foreach (SchemaEntity entity in info.GeneralEntities.Values)
                    {
                        XmlEntity node = new XmlEntity(entity.Name.Name, entity.Text, entity.Pubid, entity.Url, entity.NData.IsEmpty ? null : entity.NData.Name, this.doc);
                        node.SetBaseURI(entity.DeclaredURI);
                        dtNode.Entities.SetNamedItem(node);
                    }
                }
                if (info.ParameterEntities != null)
                {
                    foreach (SchemaEntity entity3 in info.ParameterEntities.Values)
                    {
                        XmlEntity entity4 = new XmlEntity(entity3.Name.Name, entity3.Text, entity3.Pubid, entity3.Url, entity3.NData.IsEmpty ? null : entity3.NData.Name, this.doc);
                        entity4.SetBaseURI(entity3.DeclaredURI);
                        dtNode.Entities.SetNamedItem(entity4);
                    }
                }
                this.doc.Entities = dtNode.Entities;
                IDictionaryEnumerator enumerator = info.ElementDecls.GetEnumerator();
                if (enumerator != null)
                {
                    enumerator.Reset();
                    while (enumerator.MoveNext())
                    {
                        SchemaElementDecl decl = (SchemaElementDecl) enumerator.Value;
                        if (decl.AttDefs != null)
                        {
                            IDictionaryEnumerator enumerator2 = decl.AttDefs.GetEnumerator();
                            while (enumerator2.MoveNext())
                            {
                                SchemaAttDef def = (SchemaAttDef) enumerator2.Value;
                                if (def.Datatype.TokenizedType == XmlTokenizedType.ID)
                                {
                                    this.doc.AddIdInfo(this.doc.AddXmlName(decl.Prefix, decl.Name.Name, string.Empty, null), this.doc.AddAttrXmlName(def.Prefix, def.Name.Name, string.Empty, null));
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        }

        private XmlDocumentType LoadDocumentTypeNode()
        {
            string publicId = null;
            string systemId = null;
            string internalSubset = this.reader.Value;
            string localName = this.reader.LocalName;
            while (this.reader.MoveToNextAttribute())
            {
                string name = this.reader.Name;
                if (name != null)
                {
                    if (!(name == "PUBLIC"))
                    {
                        if (name == "SYSTEM")
                        {
                            goto Label_005A;
                        }
                    }
                    else
                    {
                        publicId = this.reader.Value;
                    }
                }
                continue;
            Label_005A:
                systemId = this.reader.Value;
            }
            XmlDocumentType dtNode = this.doc.CreateDocumentType(localName, publicId, systemId, internalSubset);
            IDtdInfo dtdInfo = this.reader.DtdInfo;
            if (dtdInfo != null)
            {
                this.LoadDocumentType(dtdInfo, dtNode);
                return dtNode;
            }
            this.ParseDocumentType(dtNode);
            return dtNode;
        }

        private XmlEntityReference LoadEntityReferenceNode(bool direct)
        {
            XmlEntityReference reference = direct ? new XmlEntityReference(this.reader.Name, this.doc) : this.doc.CreateEntityReference(this.reader.Name);
            if (this.reader.CanResolveEntity)
            {
                this.reader.ResolveEntity();
                while (this.reader.Read() && (this.reader.NodeType != XmlNodeType.EndEntity))
                {
                    XmlNode newChild = direct ? this.LoadNodeDirect() : this.LoadNode(false);
                    if (newChild != null)
                    {
                        reference.AppendChildForLoad(newChild, this.doc);
                    }
                }
                if (reference.LastChild == null)
                {
                    reference.AppendChildForLoad(this.doc.CreateTextNode(string.Empty), this.doc);
                }
            }
            return reference;
        }

        internal void LoadInnerXmlAttribute(XmlAttribute node, string innerxmltext)
        {
            this.ParsePartialContent(node, innerxmltext, XmlNodeType.Attribute);
        }

        internal void LoadInnerXmlElement(XmlElement node, string innerxmltext)
        {
            XmlNamespaceManager mgr = this.ParsePartialContent(node, innerxmltext, XmlNodeType.Element);
            if (node.ChildNodes.Count > 0)
            {
                this.RemoveDuplicateNamespace(node, mgr, false);
            }
        }

        private XmlNode LoadNode(bool skipOverWhitespace)
        {
            XmlElement element;
            IXmlSchemaInfo schemaInfo;
            XmlNode node2;
            XmlReader reader = this.reader;
            XmlNode parentNode = null;
        Label_0009:
            node2 = null;
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                {
                    bool isEmptyElement = reader.IsEmptyElement;
                    element = this.doc.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                    element.IsEmpty = isEmptyElement;
                    if (reader.MoveToFirstAttribute())
                    {
                        XmlAttributeCollection attributes = element.Attributes;
                        do
                        {
                            XmlAttribute attribute = this.LoadAttributeNode();
                            attributes.Append(attribute);
                        }
                        while (reader.MoveToNextAttribute());
                        reader.MoveToElement();
                    }
                    if (isEmptyElement)
                    {
                        schemaInfo = reader.SchemaInfo;
                        if (schemaInfo != null)
                        {
                            element.XmlName = this.doc.AddXmlName(element.Prefix, element.LocalName, element.NamespaceURI, schemaInfo);
                        }
                        node2 = element;
                        break;
                    }
                    if (parentNode != null)
                    {
                        parentNode.AppendChildForLoad(element, this.doc);
                    }
                    parentNode = element;
                    goto Label_025B;
                }
                case XmlNodeType.Attribute:
                    node2 = this.LoadAttributeNode();
                    break;

                case XmlNodeType.Text:
                    node2 = this.doc.CreateTextNode(reader.Value);
                    break;

                case XmlNodeType.CDATA:
                    node2 = this.doc.CreateCDataSection(reader.Value);
                    break;

                case XmlNodeType.EntityReference:
                    node2 = this.LoadEntityReferenceNode(false);
                    break;

                case XmlNodeType.ProcessingInstruction:
                    node2 = this.doc.CreateProcessingInstruction(reader.Name, reader.Value);
                    break;

                case XmlNodeType.Comment:
                    node2 = this.doc.CreateComment(reader.Value);
                    break;

                case XmlNodeType.DocumentType:
                    node2 = this.LoadDocumentTypeNode();
                    break;

                case XmlNodeType.Whitespace:
                    if (!this.preserveWhitespace)
                    {
                        if ((parentNode == null) && !skipOverWhitespace)
                        {
                            return null;
                        }
                        goto Label_025B;
                    }
                    node2 = this.doc.CreateWhitespace(reader.Value);
                    break;

                case XmlNodeType.SignificantWhitespace:
                    node2 = this.doc.CreateSignificantWhitespace(reader.Value);
                    break;

                case XmlNodeType.EndElement:
                    if (parentNode != null)
                    {
                        schemaInfo = reader.SchemaInfo;
                        if (schemaInfo != null)
                        {
                            element = parentNode as XmlElement;
                            if (element != null)
                            {
                                element.XmlName = this.doc.AddXmlName(element.Prefix, element.LocalName, element.NamespaceURI, schemaInfo);
                            }
                        }
                        if (parentNode.ParentNode == null)
                        {
                            return parentNode;
                        }
                        parentNode = parentNode.ParentNode;
                        goto Label_025B;
                    }
                    return null;

                case XmlNodeType.EndEntity:
                    return null;

                case XmlNodeType.XmlDeclaration:
                    node2 = this.LoadDeclarationNode();
                    break;

                default:
                    throw UnexpectedNodeType(reader.NodeType);
            }
            if (parentNode != null)
            {
                parentNode.AppendChildForLoad(node2, this.doc);
            }
            else
            {
                return node2;
            }
        Label_025B:
            if (reader.Read())
            {
                goto Label_0009;
            }
            if (parentNode != null)
            {
                while (parentNode.ParentNode != null)
                {
                    parentNode = parentNode.ParentNode;
                }
            }
            return parentNode;
        }

        private XmlNode LoadNodeDirect()
        {
            XmlNode node2;
            XmlReader reader = this.reader;
            XmlNode parentNode = null;
        Label_0009:
            node2 = null;
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                {
                    bool isEmptyElement = this.reader.IsEmptyElement;
                    XmlElement newChild = new XmlElement(this.reader.Prefix, this.reader.LocalName, this.reader.NamespaceURI, this.doc) {
                        IsEmpty = isEmptyElement
                    };
                    if (this.reader.MoveToFirstAttribute())
                    {
                        XmlAttributeCollection attributes = newChild.Attributes;
                        do
                        {
                            XmlAttribute attribute = this.LoadAttributeNodeDirect();
                            attributes.Append(attribute);
                        }
                        while (reader.MoveToNextAttribute());
                    }
                    if (!isEmptyElement)
                    {
                        parentNode.AppendChildForLoad(newChild, this.doc);
                        parentNode = newChild;
                        goto Label_01FC;
                    }
                    node2 = newChild;
                    break;
                }
                case XmlNodeType.Attribute:
                    node2 = this.LoadAttributeNodeDirect();
                    break;

                case XmlNodeType.Text:
                    node2 = new XmlText(this.reader.Value, this.doc);
                    break;

                case XmlNodeType.CDATA:
                    node2 = new XmlCDataSection(this.reader.Value, this.doc);
                    break;

                case XmlNodeType.EntityReference:
                    node2 = this.LoadEntityReferenceNode(true);
                    break;

                case XmlNodeType.ProcessingInstruction:
                    node2 = new XmlProcessingInstruction(this.reader.Name, this.reader.Value, this.doc);
                    break;

                case XmlNodeType.Comment:
                    node2 = new XmlComment(this.reader.Value, this.doc);
                    break;

                case XmlNodeType.Whitespace:
                    if (!this.preserveWhitespace)
                    {
                        goto Label_01FC;
                    }
                    node2 = new XmlWhitespace(this.reader.Value, this.doc);
                    break;

                case XmlNodeType.SignificantWhitespace:
                    node2 = new XmlSignificantWhitespace(this.reader.Value, this.doc);
                    break;

                case XmlNodeType.EndElement:
                    if (parentNode.ParentNode != null)
                    {
                        parentNode = parentNode.ParentNode;
                        goto Label_01FC;
                    }
                    return parentNode;

                case XmlNodeType.EndEntity:
                    goto Label_01FC;

                default:
                    throw UnexpectedNodeType(this.reader.NodeType);
            }
            if (parentNode != null)
            {
                parentNode.AppendChildForLoad(node2, this.doc);
            }
            else
            {
                return node2;
            }
        Label_01FC:
            if (reader.Read())
            {
                goto Label_0009;
            }
            return null;
        }

        internal void ParseDocumentType(XmlDocumentType dtNode)
        {
            XmlDocument ownerDocument = dtNode.OwnerDocument;
            if (ownerDocument.HasSetResolver)
            {
                this.ParseDocumentType(dtNode, true, ownerDocument.GetResolver());
            }
            else
            {
                this.ParseDocumentType(dtNode, false, null);
            }
        }

        private void ParseDocumentType(XmlDocumentType dtNode, bool bUseResolver, XmlResolver resolver)
        {
            this.doc = dtNode.OwnerDocument;
            XmlParserContext context = new XmlParserContext(null, new XmlNamespaceManager(this.doc.NameTable), null, null, null, null, this.doc.BaseURI, string.Empty, XmlSpace.None);
            XmlTextReaderImpl reader = new XmlTextReaderImpl("", XmlNodeType.Element, context) {
                Namespaces = dtNode.ParseWithNamespaces
            };
            if (bUseResolver)
            {
                reader.XmlResolver = resolver;
            }
            IDtdParser parser = DtdParser.Create();
            XmlTextReaderImpl.DtdParserProxy adapter = new XmlTextReaderImpl.DtdParserProxy(reader);
            IDtdInfo dtdInfo = parser.ParseFreeFloatingDtd(this.doc.BaseURI, dtNode.Name, dtNode.PublicId, dtNode.SystemId, dtNode.InternalSubset, adapter);
            this.LoadDocumentType(dtdInfo, dtNode);
        }

        internal XmlNamespaceManager ParsePartialContent(XmlNode parentNode, string innerxmltext, XmlNodeType nt)
        {
            this.doc = parentNode.OwnerDocument;
            XmlParserContext context = this.GetContext(parentNode);
            this.reader = this.CreateInnerXmlReader(innerxmltext, nt, context, this.doc);
            try
            {
                this.preserveWhitespace = true;
                bool isLoading = this.doc.IsLoading;
                this.doc.IsLoading = true;
                if (nt == XmlNodeType.Entity)
                {
                    XmlNode newChild = null;
                    while (this.reader.Read() && ((newChild = this.LoadNodeDirect()) != null))
                    {
                        parentNode.AppendChildForLoad(newChild, this.doc);
                    }
                }
                else
                {
                    XmlNode node2 = null;
                    while (this.reader.Read() && ((node2 = this.LoadNode(true)) != null))
                    {
                        parentNode.AppendChildForLoad(node2, this.doc);
                    }
                }
                this.doc.IsLoading = isLoading;
            }
            finally
            {
                this.reader.Close();
            }
            return context.NamespaceManager;
        }

        internal static void ParseXmlDeclarationValue(string strValue, out string version, out string encoding, out string standalone)
        {
            version = null;
            encoding = null;
            standalone = null;
            XmlTextReaderImpl impl = new XmlTextReaderImpl(strValue, null);
            try
            {
                impl.Read();
                if (impl.MoveToAttribute("version"))
                {
                    version = impl.Value;
                }
                if (impl.MoveToAttribute("encoding"))
                {
                    encoding = impl.Value;
                }
                if (impl.MoveToAttribute("standalone"))
                {
                    standalone = impl.Value;
                }
            }
            finally
            {
                impl.Close();
            }
        }

        internal XmlNode ReadCurrentNode(XmlDocument doc, XmlReader reader)
        {
            this.doc = doc;
            this.reader = reader;
            this.preserveWhitespace = true;
            if (doc == null)
            {
                throw new ArgumentException(Res.GetString("Xdom_Load_NoDocument"));
            }
            if (reader == null)
            {
                throw new ArgumentException(Res.GetString("Xdom_Load_NoReader"));
            }
            if (reader.ReadState == ReadState.Initial)
            {
                reader.Read();
            }
            if (reader.ReadState != ReadState.Interactive)
            {
                return null;
            }
            XmlNode node = this.LoadNode(true);
            if (node.NodeType != XmlNodeType.Attribute)
            {
                reader.Read();
            }
            return node;
        }

        private void RemoveDuplicateNamespace(XmlElement elem, XmlNamespaceManager mgr, bool fCheckElemAttrs)
        {
            mgr.PushScope();
            XmlAttributeCollection attributes = elem.Attributes;
            int count = attributes.Count;
            if (fCheckElemAttrs && (count > 0))
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    XmlAttribute attribute = attributes[i];
                    if (attribute.Prefix == this.doc.strXmlns)
                    {
                        string str = mgr.LookupNamespace(attribute.LocalName);
                        if (str == null)
                        {
                            mgr.AddNamespace(attribute.LocalName, attribute.Value);
                        }
                        else if (attribute.Value == str)
                        {
                            elem.Attributes.RemoveNodeAt(i);
                        }
                    }
                    else if ((attribute.Prefix.Length == 0) && (attribute.LocalName == this.doc.strXmlns))
                    {
                        string defaultNamespace = mgr.DefaultNamespace;
                        if (defaultNamespace != null)
                        {
                            if (attribute.Value == defaultNamespace)
                            {
                                elem.Attributes.RemoveNodeAt(i);
                            }
                        }
                        else
                        {
                            mgr.AddNamespace(attribute.LocalName, attribute.Value);
                        }
                    }
                }
            }
            for (XmlNode node = elem.FirstChild; node != null; node = node.NextSibling)
            {
                XmlElement element = node as XmlElement;
                if (element != null)
                {
                    this.RemoveDuplicateNamespace(element, mgr, true);
                }
            }
            mgr.PopScope();
        }

        internal static Exception UnexpectedNodeType(XmlNodeType nodetype)
        {
            return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Res.GetString("Xml_UnexpectedNodeType"), new object[] { nodetype.ToString() }));
        }
    }
}

