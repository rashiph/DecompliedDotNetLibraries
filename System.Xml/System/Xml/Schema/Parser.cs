namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal sealed class Parser
    {
        private XmlNamespaceManager annotationNSManager;
        private SchemaBuilder builder;
        private XmlDocument dummyDocument;
        private ValidationEventHandler eventHandler;
        private bool isProcessNamespaces;
        private int markupDepth;
        private XmlNamespaceManager namespaceManager;
        private XmlNameTable nameTable;
        private XmlNode parentNode;
        private PositionInfo positionInfo;
        private bool processMarkup;
        private XmlReader reader;
        private System.Xml.Schema.XmlSchema schema;
        private SchemaNames schemaNames;
        private SchemaType schemaType;
        private int schemaXmlDepth;
        private SchemaInfo xdrSchema;
        private XmlCharType xmlCharType = XmlCharType.Instance;
        private string xmlns;
        private System.Xml.XmlResolver xmlResolver;

        public Parser(SchemaType schemaType, XmlNameTable nameTable, SchemaNames schemaNames, ValidationEventHandler eventHandler)
        {
            this.schemaType = schemaType;
            this.nameTable = nameTable;
            this.schemaNames = schemaNames;
            this.eventHandler = eventHandler;
            this.xmlResolver = new XmlUrlResolver();
            this.processMarkup = true;
            this.dummyDocument = new XmlDocument();
        }

        private bool CheckSchemaRoot(SchemaType rootType, out string code)
        {
            code = null;
            if (this.schemaType == SchemaType.None)
            {
                this.schemaType = rootType;
            }
            switch (rootType)
            {
                case SchemaType.None:
                case SchemaType.DTD:
                    code = "Sch_SchemaRootExpected";
                    if (this.schemaType == SchemaType.XSD)
                    {
                        code = "Sch_XSDSchemaRootExpected";
                    }
                    return false;

                case SchemaType.XDR:
                    if (this.schemaType != SchemaType.XSD)
                    {
                        if (this.schemaType != SchemaType.XDR)
                        {
                            code = "Sch_MixSchemaTypes";
                            return false;
                        }
                        break;
                    }
                    code = "Sch_XSDSchemaOnly";
                    return false;

                case SchemaType.XSD:
                    if (this.schemaType == SchemaType.XSD)
                    {
                        break;
                    }
                    code = "Sch_MixSchemaTypes";
                    return false;
            }
            return true;
        }

        private XmlAttribute CreateXmlNsAttribute(string prefix, string value)
        {
            XmlAttribute attribute;
            if (prefix.Length == 0)
            {
                attribute = this.dummyDocument.CreateAttribute(string.Empty, this.xmlns, "http://www.w3.org/2000/xmlns/");
            }
            else
            {
                attribute = this.dummyDocument.CreateAttribute(this.xmlns, prefix, "http://www.w3.org/2000/xmlns/");
            }
            attribute.AppendChild(this.dummyDocument.CreateTextNode(value));
            this.annotationNSManager.AddNamespace(prefix, value);
            return attribute;
        }

        public SchemaType FinishParsing()
        {
            return this.schemaType;
        }

        private XmlAttribute LoadAttributeNode()
        {
            XmlReader reader = this.reader;
            XmlAttribute attribute = this.dummyDocument.CreateAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI);
            while (reader.ReadAttributeValue())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Text:
                    {
                        attribute.AppendChild(this.dummyDocument.CreateTextNode(reader.Value));
                        continue;
                    }
                    case XmlNodeType.EntityReference:
                    {
                        attribute.AppendChild(this.LoadEntityReferenceInAttribute());
                        continue;
                    }
                }
                throw XmlLoader.UnexpectedNodeType(reader.NodeType);
            }
            return attribute;
        }

        private XmlElement LoadElementNode(bool root)
        {
            XmlReader reader = this.reader;
            bool isEmptyElement = reader.IsEmptyElement;
            XmlElement newChild = this.dummyDocument.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
            newChild.IsEmpty = isEmptyElement;
            if (root)
            {
                this.parentNode = newChild;
                return newChild;
            }
            XmlAttributeCollection attributes = newChild.Attributes;
            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    if (Ref.Equal(reader.NamespaceURI, this.schemaNames.NsXmlNs))
                    {
                        this.annotationNSManager.AddNamespace((reader.Prefix.Length == 0) ? string.Empty : this.reader.LocalName, this.reader.Value);
                    }
                    XmlAttribute node = this.LoadAttributeNode();
                    attributes.Append(node);
                }
                while (reader.MoveToNextAttribute());
            }
            reader.MoveToElement();
            string str = this.annotationNSManager.LookupNamespace(reader.Prefix);
            if (str == null)
            {
                XmlAttribute attribute2 = this.CreateXmlNsAttribute(reader.Prefix, this.namespaceManager.LookupNamespace(reader.Prefix));
                attributes.Append(attribute2);
            }
            else if (str.Length == 0)
            {
                string str2 = this.namespaceManager.LookupNamespace(reader.Prefix);
                if (str2 != string.Empty)
                {
                    XmlAttribute attribute3 = this.CreateXmlNsAttribute(reader.Prefix, str2);
                    attributes.Append(attribute3);
                }
            }
            while (reader.MoveToNextAttribute())
            {
                if ((reader.Prefix.Length != 0) && (this.annotationNSManager.LookupNamespace(reader.Prefix) == null))
                {
                    XmlAttribute attribute4 = this.CreateXmlNsAttribute(reader.Prefix, this.namespaceManager.LookupNamespace(reader.Prefix));
                    attributes.Append(attribute4);
                }
            }
            reader.MoveToElement();
            this.parentNode.AppendChild(newChild);
            if (!reader.IsEmptyElement)
            {
                this.parentNode = newChild;
            }
            return newChild;
        }

        private XmlEntityReference LoadEntityReferenceInAttribute()
        {
            XmlEntityReference reference = this.dummyDocument.CreateEntityReference(this.reader.LocalName);
            if (this.reader.CanResolveEntity)
            {
                this.reader.ResolveEntity();
                while (this.reader.ReadAttributeValue())
                {
                    switch (this.reader.NodeType)
                    {
                        case XmlNodeType.Text:
                        {
                            reference.AppendChild(this.dummyDocument.CreateTextNode(this.reader.Value));
                            continue;
                        }
                        case XmlNodeType.EntityReference:
                        {
                            reference.AppendChild(this.LoadEntityReferenceInAttribute());
                            continue;
                        }
                        case XmlNodeType.EndEntity:
                            if (reference.ChildNodes.Count == 0)
                            {
                                reference.AppendChild(this.dummyDocument.CreateTextNode(string.Empty));
                            }
                            return reference;
                    }
                    throw XmlLoader.UnexpectedNodeType(this.reader.NodeType);
                }
            }
            return reference;
        }

        public SchemaType Parse(XmlReader reader, string targetNamespace)
        {
            this.StartParsing(reader, targetNamespace);
            while (this.ParseReaderNode() && reader.Read())
            {
            }
            return this.FinishParsing();
        }

        public bool ParseReaderNode()
        {
            if (this.reader.Depth > this.markupDepth)
            {
                if (this.processMarkup)
                {
                    this.ProcessAppInfoDocMarkup(false);
                }
                return true;
            }
            if (this.reader.NodeType == XmlNodeType.Element)
            {
                if (this.builder.ProcessElement(this.reader.Prefix, this.reader.LocalName, this.reader.NamespaceURI))
                {
                    this.namespaceManager.PushScope();
                    if (this.reader.MoveToFirstAttribute())
                    {
                        do
                        {
                            this.builder.ProcessAttribute(this.reader.Prefix, this.reader.LocalName, this.reader.NamespaceURI, this.reader.Value);
                            if (Ref.Equal(this.reader.NamespaceURI, this.schemaNames.NsXmlNs) && this.isProcessNamespaces)
                            {
                                this.namespaceManager.AddNamespace((this.reader.Prefix.Length == 0) ? string.Empty : this.reader.LocalName, this.reader.Value);
                            }
                        }
                        while (this.reader.MoveToNextAttribute());
                        this.reader.MoveToElement();
                    }
                    this.builder.StartChildren();
                    if (this.reader.IsEmptyElement)
                    {
                        this.namespaceManager.PopScope();
                        this.builder.EndChildren();
                        if (this.reader.Depth == this.schemaXmlDepth)
                        {
                            return false;
                        }
                    }
                    else if (!this.builder.IsContentParsed())
                    {
                        this.markupDepth = this.reader.Depth;
                        this.processMarkup = true;
                        if (this.annotationNSManager == null)
                        {
                            this.annotationNSManager = new XmlNamespaceManager(this.nameTable);
                            this.xmlns = this.nameTable.Add("xmlns");
                        }
                        this.ProcessAppInfoDocMarkup(true);
                    }
                }
                else if (!this.reader.IsEmptyElement)
                {
                    this.markupDepth = this.reader.Depth;
                    this.processMarkup = false;
                }
            }
            else if (this.reader.NodeType == XmlNodeType.Text)
            {
                if (!this.xmlCharType.IsOnlyWhitespace(this.reader.Value))
                {
                    this.builder.ProcessCData(this.reader.Value);
                }
            }
            else if (((this.reader.NodeType == XmlNodeType.EntityReference) || (this.reader.NodeType == XmlNodeType.SignificantWhitespace)) || (this.reader.NodeType == XmlNodeType.CDATA))
            {
                this.builder.ProcessCData(this.reader.Value);
            }
            else if (this.reader.NodeType == XmlNodeType.EndElement)
            {
                if (this.reader.Depth == this.markupDepth)
                {
                    if (this.processMarkup)
                    {
                        XmlNodeList childNodes = this.parentNode.ChildNodes;
                        XmlNode[] markup = new XmlNode[childNodes.Count];
                        for (int i = 0; i < childNodes.Count; i++)
                        {
                            markup[i] = childNodes[i];
                        }
                        this.builder.ProcessMarkup(markup);
                        this.namespaceManager.PopScope();
                        this.builder.EndChildren();
                    }
                    this.markupDepth = 0x7fffffff;
                }
                else
                {
                    this.namespaceManager.PopScope();
                    this.builder.EndChildren();
                }
                if (this.reader.Depth == this.schemaXmlDepth)
                {
                    return false;
                }
            }
            return true;
        }

        private void ProcessAppInfoDocMarkup(bool root)
        {
            XmlNode newChild = null;
            switch (this.reader.NodeType)
            {
                case XmlNodeType.Element:
                    this.annotationNSManager.PushScope();
                    newChild = this.LoadElementNode(root);
                    return;

                case XmlNodeType.Text:
                    newChild = this.dummyDocument.CreateTextNode(this.reader.Value);
                    break;

                case XmlNodeType.CDATA:
                    newChild = this.dummyDocument.CreateCDataSection(this.reader.Value);
                    break;

                case XmlNodeType.EntityReference:
                    newChild = this.dummyDocument.CreateEntityReference(this.reader.Name);
                    break;

                case XmlNodeType.ProcessingInstruction:
                    newChild = this.dummyDocument.CreateProcessingInstruction(this.reader.Name, this.reader.Value);
                    break;

                case XmlNodeType.Comment:
                    newChild = this.dummyDocument.CreateComment(this.reader.Value);
                    break;

                case XmlNodeType.Whitespace:
                case XmlNodeType.EndEntity:
                    return;

                case XmlNodeType.SignificantWhitespace:
                    newChild = this.dummyDocument.CreateSignificantWhitespace(this.reader.Value);
                    break;

                case XmlNodeType.EndElement:
                    this.annotationNSManager.PopScope();
                    this.parentNode = this.parentNode.ParentNode;
                    return;
            }
            this.parentNode.AppendChild(newChild);
        }

        public void StartParsing(XmlReader reader, string targetNamespace)
        {
            string str;
            this.reader = reader;
            this.positionInfo = PositionInfo.GetPositionInfo(reader);
            this.namespaceManager = reader.NamespaceManager;
            if (this.namespaceManager == null)
            {
                this.namespaceManager = new XmlNamespaceManager(this.nameTable);
                this.isProcessNamespaces = true;
            }
            else
            {
                this.isProcessNamespaces = false;
            }
            while ((reader.NodeType != XmlNodeType.Element) && reader.Read())
            {
            }
            this.markupDepth = 0x7fffffff;
            this.schemaXmlDepth = reader.Depth;
            SchemaType rootType = this.schemaNames.SchemaTypeFromRoot(reader.LocalName, reader.NamespaceURI);
            if (!this.CheckSchemaRoot(rootType, out str))
            {
                throw new XmlSchemaException(str, reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition);
            }
            if (this.schemaType == SchemaType.XSD)
            {
                this.schema = new System.Xml.Schema.XmlSchema();
                this.schema.BaseUri = new Uri(reader.BaseURI, UriKind.RelativeOrAbsolute);
                this.builder = new XsdBuilder(reader, this.namespaceManager, this.schema, this.nameTable, this.schemaNames, this.eventHandler);
            }
            else
            {
                this.xdrSchema = new SchemaInfo();
                this.xdrSchema.SchemaType = SchemaType.XDR;
                this.builder = new XdrBuilder(reader, this.namespaceManager, this.xdrSchema, targetNamespace, this.nameTable, this.schemaNames, this.eventHandler);
                ((XdrBuilder) this.builder).XmlResolver = this.xmlResolver;
            }
        }

        public SchemaInfo XdrSchema
        {
            get
            {
                return this.xdrSchema;
            }
        }

        internal System.Xml.XmlResolver XmlResolver
        {
            set
            {
                this.xmlResolver = value;
            }
        }

        public System.Xml.Schema.XmlSchema XmlSchema
        {
            get
            {
                return this.schema;
            }
        }
    }
}

