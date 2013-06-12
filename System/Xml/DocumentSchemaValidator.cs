namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml.Schema;
    using System.Xml.XPath;

    internal sealed class DocumentSchemaValidator : IXmlNamespaceResolver
    {
        private XmlSchemaInfo attributeSchemaInfo;
        private XmlNode currentNode;
        private ArrayList defaultAttributes;
        private XmlDocument document;
        private ValidationEventHandler eventHandler;
        private ValidationEventHandler internalEventHandler;
        private bool isPartialTreeValid;
        private bool isValid;
        private XmlNameTable nameTable;
        private XmlNode[] nodeSequenceToValidate;
        private XmlValueGetter nodeValueGetter;
        private XmlNamespaceManager nsManager;
        private string NsXmlNs;
        private string NsXsi;
        private bool psviAugmentation;
        private XmlSchemaInfo schemaInfo;
        private XmlSchemaSet schemas;
        private XmlNode startNode;
        private XmlSchemaValidator validator;
        private string XsiNil;
        private string XsiType;

        public DocumentSchemaValidator(XmlDocument ownerDocument, XmlSchemaSet schemas, ValidationEventHandler eventHandler)
        {
            this.schemas = schemas;
            this.eventHandler = eventHandler;
            this.document = ownerDocument;
            this.internalEventHandler = new ValidationEventHandler(this.InternalValidationCallBack);
            this.nameTable = this.document.NameTable;
            this.nsManager = new XmlNamespaceManager(this.nameTable);
            this.nodeValueGetter = new XmlValueGetter(this.GetNodeValue);
            this.psviAugmentation = true;
            this.NsXmlNs = this.nameTable.Add("http://www.w3.org/2000/xmlns/");
            this.NsXsi = this.nameTable.Add("http://www.w3.org/2001/XMLSchema-instance");
            this.XsiType = this.nameTable.Add("type");
            this.XsiNil = this.nameTable.Add("nil");
        }

        private bool AncestorTypeHasWildcard(XmlSchemaObject ancestorType)
        {
            XmlSchemaComplexType complexType = this.GetComplexType(ancestorType);
            return ((ancestorType != null) && complexType.HasWildCard);
        }

        private void CheckNodeSequenceCapacity(int currentIndex)
        {
            if (this.nodeSequenceToValidate == null)
            {
                this.nodeSequenceToValidate = new XmlNode[4];
            }
            else if (currentIndex >= (this.nodeSequenceToValidate.Length - 1))
            {
                XmlNode[] destinationArray = new XmlNode[this.nodeSequenceToValidate.Length * 2];
                Array.Copy(this.nodeSequenceToValidate, 0, destinationArray, 0, this.nodeSequenceToValidate.Length);
                this.nodeSequenceToValidate = destinationArray;
            }
        }

        private XmlSchemaValidator CreateTypeFinderValidator(XmlSchemaObject partialValidationType)
        {
            XmlSchemaValidator validator = new XmlSchemaValidator(this.document.NameTable, this.document.Schemas, this.nsManager, XmlSchemaValidationFlags.None);
            validator.ValidationEventHandler += new ValidationEventHandler(this.TypeFinderCallBack);
            if (partialValidationType != null)
            {
                validator.Initialize(partialValidationType);
                return validator;
            }
            validator.Initialize();
            return validator;
        }

        private void CreateValidator(XmlSchemaObject partialValidationType, XmlSchemaValidationFlags validationFlags)
        {
            this.validator = new XmlSchemaValidator(this.nameTable, this.schemas, this.NamespaceResolver, validationFlags);
            this.validator.SourceUri = XmlConvert.ToUri(this.document.BaseURI);
            this.validator.XmlResolver = null;
            this.validator.ValidationEventHandler += this.internalEventHandler;
            this.validator.ValidationEventSender = this;
            if (partialValidationType != null)
            {
                this.validator.Initialize(partialValidationType);
            }
            else
            {
                this.validator.Initialize();
            }
        }

        private XmlSchemaAttribute FindSchemaInfo(XmlAttribute attributeToValidate)
        {
            XmlElement ownerElement = attributeToValidate.OwnerElement;
            XmlSchemaObject schemaObject = this.FindSchemaInfo(ownerElement);
            XmlSchemaComplexType complexType = this.GetComplexType(schemaObject);
            if (complexType == null)
            {
                return null;
            }
            XmlQualifiedName qname = new XmlQualifiedName(attributeToValidate.LocalName, attributeToValidate.NamespaceURI);
            XmlSchemaAttribute attribute = complexType.AttributeUses[qname] as XmlSchemaAttribute;
            if (attribute == null)
            {
                XmlSchemaAnyAttribute attributeWildcard = complexType.AttributeWildcard;
                if ((attributeWildcard != null) && attributeWildcard.NamespaceList.Allows(qname))
                {
                    attribute = this.schemas.GlobalAttributes[qname] as XmlSchemaAttribute;
                }
            }
            return attribute;
        }

        private XmlSchemaObject FindSchemaInfo(XmlElement elementToValidate)
        {
            this.isPartialTreeValid = true;
            XmlNode node = elementToValidate;
            IXmlSchemaInfo schemaInfo = null;
            int currentIndex = 0;
            XmlNode parentNode = node.ParentNode;
            do
            {
                schemaInfo = parentNode.SchemaInfo;
                if ((schemaInfo.SchemaElement != null) || (schemaInfo.SchemaType != null))
                {
                    break;
                }
                this.CheckNodeSequenceCapacity(currentIndex);
                this.nodeSequenceToValidate[currentIndex++] = parentNode;
                parentNode = parentNode.ParentNode;
            }
            while (parentNode != null);
            if (parentNode == null)
            {
                currentIndex--;
                this.nodeSequenceToValidate[currentIndex] = null;
                return this.GetTypeFromAncestors(elementToValidate, null, currentIndex);
            }
            this.CheckNodeSequenceCapacity(currentIndex);
            this.nodeSequenceToValidate[currentIndex++] = parentNode;
            XmlSchemaObject schemaElement = schemaInfo.SchemaElement;
            if (schemaElement == null)
            {
                schemaElement = schemaInfo.SchemaType;
            }
            return this.GetTypeFromAncestors(elementToValidate, schemaElement, currentIndex);
        }

        private XmlSchemaComplexType GetComplexType(XmlSchemaObject schemaObject)
        {
            if (schemaObject == null)
            {
                return null;
            }
            XmlSchemaElement element = schemaObject as XmlSchemaElement;
            if (element != null)
            {
                return (element.ElementSchemaType as XmlSchemaComplexType);
            }
            return (schemaObject as XmlSchemaComplexType);
        }

        private string GetDefaultPrefix(string attributeNS)
        {
            IDictionary<string, string> namespacesInScope = this.NamespaceResolver.GetNamespacesInScope(XmlNamespaceScope.All);
            string key = null;
            attributeNS = this.nameTable.Add(attributeNS);
            foreach (KeyValuePair<string, string> pair in namespacesInScope)
            {
                if (object.ReferenceEquals(this.nameTable.Add(pair.Value), attributeNS))
                {
                    key = pair.Key;
                    if (key.Length != 0)
                    {
                        return key;
                    }
                }
            }
            return key;
        }

        public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
        {
            IDictionary<string, string> namespacesInScope = this.nsManager.GetNamespacesInScope(scope);
            if (scope != XmlNamespaceScope.Local)
            {
                XmlNode startNode = this.startNode;
                while (startNode != null)
                {
                    XmlAttributeCollection attributes;
                    int num;
                    XmlAttribute attribute;
                    switch (startNode.NodeType)
                    {
                        case XmlNodeType.Element:
                        {
                            XmlElement element = (XmlElement) startNode;
                            if (!element.HasAttributes)
                            {
                                goto Label_00E4;
                            }
                            attributes = element.Attributes;
                            num = 0;
                            goto Label_00D7;
                        }
                        case XmlNodeType.Attribute:
                        {
                            startNode = ((XmlAttribute) startNode).OwnerElement;
                            continue;
                        }
                        default:
                            goto Label_00FB;
                    }
                Label_005C:
                    attribute = attributes[num];
                    if (Ref.Equal(attribute.NamespaceURI, this.document.strReservedXmlns))
                    {
                        if (attribute.Prefix.Length == 0)
                        {
                            if (!namespacesInScope.ContainsKey(string.Empty))
                            {
                                namespacesInScope.Add(string.Empty, attribute.Value);
                            }
                        }
                        else if (!namespacesInScope.ContainsKey(attribute.LocalName))
                        {
                            namespacesInScope.Add(attribute.LocalName, attribute.Value);
                        }
                    }
                    num++;
                Label_00D7:
                    if (num < attributes.Count)
                    {
                        goto Label_005C;
                    }
                Label_00E4:
                    startNode = startNode.ParentNode;
                    continue;
                Label_00FB:
                    startNode = startNode.ParentNode;
                }
            }
            return namespacesInScope;
        }

        private object GetNodeValue()
        {
            return this.currentNode.Value;
        }

        private XmlSchemaObject GetTypeFromAncestors(XmlElement elementToValidate, XmlSchemaObject ancestorType, int ancestorsCount)
        {
            this.validator = this.CreateTypeFinderValidator(ancestorType);
            this.schemaInfo = new XmlSchemaInfo();
            int num = ancestorsCount - 1;
            bool flag = this.AncestorTypeHasWildcard(ancestorType);
            for (int i = num; i >= 0; i--)
            {
                XmlNode parentNode = this.nodeSequenceToValidate[i];
                XmlElement elementNode = parentNode as XmlElement;
                this.ValidateSingleElement(elementNode, false, this.schemaInfo);
                if (!flag)
                {
                    elementNode.XmlName = this.document.AddXmlName(elementNode.Prefix, elementNode.LocalName, elementNode.NamespaceURI, this.schemaInfo);
                    flag = this.AncestorTypeHasWildcard(this.schemaInfo.SchemaElement);
                }
                this.validator.ValidateEndOfAttributes(null);
                if (i > 0)
                {
                    this.ValidateChildrenTillNextAncestor(parentNode, this.nodeSequenceToValidate[i - 1]);
                }
                else
                {
                    this.ValidateChildrenTillNextAncestor(parentNode, elementToValidate);
                }
            }
            this.ValidateSingleElement(elementToValidate, false, this.schemaInfo);
            XmlSchemaObject schemaElement = null;
            if (this.schemaInfo.SchemaElement != null)
            {
                schemaElement = this.schemaInfo.SchemaElement;
            }
            else
            {
                schemaElement = this.schemaInfo.SchemaType;
            }
            if (schemaElement == null)
            {
                if (this.validator.CurrentProcessContents == XmlSchemaContentProcessing.Skip)
                {
                    if (this.isPartialTreeValid)
                    {
                        return XmlSchemaComplexType.AnyTypeSkip;
                    }
                    return schemaElement;
                }
                if (this.validator.CurrentProcessContents == XmlSchemaContentProcessing.Lax)
                {
                    return XmlSchemaComplexType.AnyType;
                }
            }
            return schemaElement;
        }

        private void InternalValidationCallBack(object sender, ValidationEventArgs arg)
        {
            if (arg.Severity == XmlSeverityType.Error)
            {
                this.isValid = false;
            }
            XmlSchemaValidationException exception = arg.Exception as XmlSchemaValidationException;
            exception.SetSourceObject(this.currentNode);
            if (this.eventHandler != null)
            {
                this.eventHandler(sender, arg);
            }
            else if (arg.Severity == XmlSeverityType.Error)
            {
                throw exception;
            }
        }

        public string LookupNamespace(string prefix)
        {
            string namespaceOfPrefixStrict = this.nsManager.LookupNamespace(prefix);
            if (namespaceOfPrefixStrict == null)
            {
                namespaceOfPrefixStrict = this.startNode.GetNamespaceOfPrefixStrict(prefix);
            }
            return namespaceOfPrefixStrict;
        }

        public string LookupPrefix(string namespaceName)
        {
            string prefix = this.nsManager.LookupPrefix(namespaceName);
            if (prefix == null)
            {
                prefix = this.startNode.GetPrefixOfNamespaceStrict(namespaceName);
            }
            return prefix;
        }

        private void SetDefaultAttributeSchemaInfo(XmlSchemaAttribute schemaAttribute)
        {
            this.attributeSchemaInfo.Clear();
            this.attributeSchemaInfo.IsDefault = true;
            this.attributeSchemaInfo.IsNil = false;
            this.attributeSchemaInfo.SchemaType = schemaAttribute.AttributeSchemaType;
            this.attributeSchemaInfo.SchemaAttribute = schemaAttribute;
            SchemaAttDef attDef = schemaAttribute.AttDef;
            if (attDef.Datatype.Variety == XmlSchemaDatatypeVariety.Union)
            {
                XsdSimpleValue defaultValueTyped = attDef.DefaultValueTyped as XsdSimpleValue;
                this.attributeSchemaInfo.MemberType = defaultValueTyped.XmlType;
            }
            this.attributeSchemaInfo.Validity = XmlSchemaValidity.Valid;
        }

        private void TypeFinderCallBack(object sender, ValidationEventArgs arg)
        {
            if (arg.Severity == XmlSeverityType.Error)
            {
                this.isPartialTreeValid = false;
            }
        }

        public bool Validate(XmlNode nodeToValidate)
        {
            XmlSchemaObject partialValidationType = null;
            XmlSchemaValidationFlags allowXmlAttributes = XmlSchemaValidationFlags.AllowXmlAttributes;
            this.startNode = nodeToValidate;
            switch (nodeToValidate.NodeType)
            {
                case XmlNodeType.Element:
                {
                    IXmlSchemaInfo schemaInfo = nodeToValidate.SchemaInfo;
                    XmlSchemaElement schemaElement = schemaInfo.SchemaElement;
                    if (schemaElement == null)
                    {
                        partialValidationType = schemaInfo.SchemaType;
                        if (partialValidationType == null)
                        {
                            if (nodeToValidate.ParentNode.NodeType != XmlNodeType.Document)
                            {
                                partialValidationType = this.FindSchemaInfo(nodeToValidate as XmlElement);
                                if (partialValidationType == null)
                                {
                                    throw new XmlSchemaValidationException("XmlDocument_NoNodeSchemaInfo", null, nodeToValidate);
                                }
                            }
                            else
                            {
                                nodeToValidate = nodeToValidate.ParentNode;
                            }
                        }
                    }
                    else if (schemaElement.RefName.IsEmpty)
                    {
                        partialValidationType = schemaElement;
                    }
                    else
                    {
                        partialValidationType = this.schemas.GlobalElements[schemaElement.QualifiedName];
                    }
                    goto Label_0110;
                }
                case XmlNodeType.Attribute:
                    if (nodeToValidate.XPNodeType != XPathNodeType.Namespace)
                    {
                        partialValidationType = nodeToValidate.SchemaInfo.SchemaAttribute;
                        if (partialValidationType == null)
                        {
                            partialValidationType = this.FindSchemaInfo(nodeToValidate as XmlAttribute);
                            if (partialValidationType == null)
                            {
                                throw new XmlSchemaValidationException("XmlDocument_NoNodeSchemaInfo", null, nodeToValidate);
                            }
                        }
                        goto Label_0110;
                    }
                    break;

                case XmlNodeType.Document:
                    allowXmlAttributes |= XmlSchemaValidationFlags.ProcessIdentityConstraints;
                    goto Label_0110;

                case XmlNodeType.DocumentFragment:
                    goto Label_0110;
            }
            throw new InvalidOperationException(Res.GetString("XmlDocument_ValidateInvalidNodeType", (object[]) null));
        Label_0110:
            this.isValid = true;
            this.CreateValidator(partialValidationType, allowXmlAttributes);
            if (this.psviAugmentation)
            {
                if (this.schemaInfo == null)
                {
                    this.schemaInfo = new XmlSchemaInfo();
                }
                this.attributeSchemaInfo = new XmlSchemaInfo();
            }
            this.ValidateNode(nodeToValidate);
            this.validator.EndValidation();
            return this.isValid;
        }

        private void ValidateAttributes(XmlElement elementNode)
        {
            XmlAttributeCollection attributes = elementNode.Attributes;
            XmlAttribute node = null;
            for (int i = 0; i < attributes.Count; i++)
            {
                node = attributes[i];
                this.currentNode = node;
                if (!Ref.Equal(node.NamespaceURI, this.NsXmlNs))
                {
                    this.validator.ValidateAttribute(node.LocalName, node.NamespaceURI, this.nodeValueGetter, this.attributeSchemaInfo);
                    if (this.psviAugmentation)
                    {
                        node.XmlName = this.document.AddAttrXmlName(node.Prefix, node.LocalName, node.NamespaceURI, this.attributeSchemaInfo);
                    }
                }
            }
            if (this.psviAugmentation)
            {
                if (this.defaultAttributes == null)
                {
                    this.defaultAttributes = new ArrayList();
                }
                else
                {
                    this.defaultAttributes.Clear();
                }
                this.validator.GetUnspecifiedDefaultAttributes(this.defaultAttributes);
                XmlSchemaAttribute schemaAttribute = null;
                node = null;
                for (int j = 0; j < this.defaultAttributes.Count; j++)
                {
                    schemaAttribute = this.defaultAttributes[j] as XmlSchemaAttribute;
                    XmlQualifiedName qualifiedName = schemaAttribute.QualifiedName;
                    node = this.document.CreateDefaultAttribute(this.GetDefaultPrefix(qualifiedName.Namespace), qualifiedName.Name, qualifiedName.Namespace);
                    this.SetDefaultAttributeSchemaInfo(schemaAttribute);
                    node.XmlName = this.document.AddAttrXmlName(node.Prefix, node.LocalName, node.NamespaceURI, this.attributeSchemaInfo);
                    node.AppendChild(this.document.CreateTextNode(schemaAttribute.AttDef.DefaultValueRaw));
                    attributes.Append(node);
                    XmlUnspecifiedAttribute attribute3 = node as XmlUnspecifiedAttribute;
                    if (attribute3 != null)
                    {
                        attribute3.SetSpecified(false);
                    }
                }
            }
        }

        private void ValidateChildrenTillNextAncestor(XmlNode parentNode, XmlNode childToStopAt)
        {
            for (XmlNode node = parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                if (node == childToStopAt)
                {
                    return;
                }
                switch (node.NodeType)
                {
                    case XmlNodeType.Element:
                    {
                        this.ValidateSingleElement(node as XmlElement, true, null);
                        continue;
                    }
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    {
                        this.validator.ValidateText(node.Value);
                        continue;
                    }
                    case XmlNodeType.EntityReference:
                    {
                        this.ValidateChildrenTillNextAncestor(node, childToStopAt);
                        continue;
                    }
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    {
                        continue;
                    }
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                    {
                        this.validator.ValidateWhitespace(node.Value);
                        continue;
                    }
                }
                throw new InvalidOperationException(Res.GetString("Xml_UnexpectedNodeType", new string[] { this.currentNode.NodeType.ToString() }));
            }
        }

        private void ValidateElement()
        {
            this.nsManager.PushScope();
            XmlElement currentNode = this.currentNode as XmlElement;
            XmlAttributeCollection attributes = currentNode.Attributes;
            XmlAttribute attribute = null;
            string xsiNil = null;
            string xsiType = null;
            for (int i = 0; i < attributes.Count; i++)
            {
                attribute = attributes[i];
                string namespaceURI = attribute.NamespaceURI;
                string localName = attribute.LocalName;
                if (Ref.Equal(namespaceURI, this.NsXsi))
                {
                    if (Ref.Equal(localName, this.XsiType))
                    {
                        xsiType = attribute.Value;
                    }
                    else if (Ref.Equal(localName, this.XsiNil))
                    {
                        xsiNil = attribute.Value;
                    }
                }
                else if (Ref.Equal(namespaceURI, this.NsXmlNs))
                {
                    this.nsManager.AddNamespace((attribute.Prefix.Length == 0) ? string.Empty : attribute.LocalName, attribute.Value);
                }
            }
            this.validator.ValidateElement(currentNode.LocalName, currentNode.NamespaceURI, this.schemaInfo, xsiType, xsiNil, null, null);
            this.ValidateAttributes(currentNode);
            this.validator.ValidateEndOfAttributes(this.schemaInfo);
            for (XmlNode node = currentNode.FirstChild; node != null; node = node.NextSibling)
            {
                this.ValidateNode(node);
            }
            this.currentNode = currentNode;
            this.validator.ValidateEndElement(this.schemaInfo);
            if (this.psviAugmentation)
            {
                currentNode.XmlName = this.document.AddXmlName(currentNode.Prefix, currentNode.LocalName, currentNode.NamespaceURI, this.schemaInfo);
                if (this.schemaInfo.IsDefault)
                {
                    XmlText newChild = this.document.CreateTextNode(this.schemaInfo.SchemaElement.ElementDecl.DefaultValueRaw);
                    currentNode.AppendChild(newChild);
                }
            }
            this.nsManager.PopScope();
        }

        private void ValidateNode(XmlNode node)
        {
            this.currentNode = node;
            switch (this.currentNode.NodeType)
            {
                case XmlNodeType.Element:
                    this.ValidateElement();
                    return;

                case XmlNodeType.Attribute:
                {
                    XmlAttribute currentNode = this.currentNode as XmlAttribute;
                    this.validator.ValidateAttribute(currentNode.LocalName, currentNode.NamespaceURI, this.nodeValueGetter, this.attributeSchemaInfo);
                    if (this.psviAugmentation)
                    {
                        currentNode.XmlName = this.document.AddAttrXmlName(currentNode.Prefix, currentNode.LocalName, currentNode.NamespaceURI, this.attributeSchemaInfo);
                    }
                    return;
                }
                case XmlNodeType.Text:
                    this.validator.ValidateText(this.nodeValueGetter);
                    return;

                case XmlNodeType.CDATA:
                    this.validator.ValidateText(this.nodeValueGetter);
                    return;

                case XmlNodeType.EntityReference:
                case XmlNodeType.DocumentFragment:
                    for (XmlNode node2 = node.FirstChild; node2 != null; node2 = node2.NextSibling)
                    {
                        this.ValidateNode(node2);
                    }
                    return;

                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                    return;

                case XmlNodeType.Document:
                {
                    XmlElement documentElement = ((XmlDocument) node).DocumentElement;
                    if (documentElement == null)
                    {
                        throw new InvalidOperationException(Res.GetString("Xml_InvalidXmlDocument", new object[] { Res.GetString("Xdom_NoRootEle") }));
                    }
                    this.ValidateNode(documentElement);
                    return;
                }
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    this.validator.ValidateWhitespace(this.nodeValueGetter);
                    return;
            }
            throw new InvalidOperationException(Res.GetString("Xml_UnexpectedNodeType", new string[] { this.currentNode.NodeType.ToString() }));
        }

        private void ValidateSingleElement(XmlElement elementNode, bool skipToEnd, XmlSchemaInfo newSchemaInfo)
        {
            this.nsManager.PushScope();
            XmlAttributeCollection attributes = elementNode.Attributes;
            XmlAttribute attribute = null;
            string xsiNil = null;
            string xsiType = null;
            for (int i = 0; i < attributes.Count; i++)
            {
                attribute = attributes[i];
                string namespaceURI = attribute.NamespaceURI;
                string localName = attribute.LocalName;
                if (Ref.Equal(namespaceURI, this.NsXsi))
                {
                    if (Ref.Equal(localName, this.XsiType))
                    {
                        xsiType = attribute.Value;
                    }
                    else if (Ref.Equal(localName, this.XsiNil))
                    {
                        xsiNil = attribute.Value;
                    }
                }
                else if (Ref.Equal(namespaceURI, this.NsXmlNs))
                {
                    this.nsManager.AddNamespace((attribute.Prefix.Length == 0) ? string.Empty : attribute.LocalName, attribute.Value);
                }
            }
            this.validator.ValidateElement(elementNode.LocalName, elementNode.NamespaceURI, newSchemaInfo, xsiType, xsiNil, null, null);
            if (skipToEnd)
            {
                this.validator.ValidateEndOfAttributes(newSchemaInfo);
                this.validator.SkipToEndElement(newSchemaInfo);
                this.nsManager.PopScope();
            }
        }

        private IXmlNamespaceResolver NamespaceResolver
        {
            get
            {
                if (this.startNode == this.document)
                {
                    return this.nsManager;
                }
                return this;
            }
        }

        public bool PsviAugmentation
        {
            get
            {
                return this.psviAugmentation;
            }
            set
            {
                this.psviAugmentation = value;
            }
        }
    }
}

