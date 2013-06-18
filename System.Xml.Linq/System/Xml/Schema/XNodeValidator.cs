namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Linq;

    internal class XNodeValidator
    {
        private bool addSchemaInfo;
        private ArrayList defaultAttributes;
        private XmlNamespaceManager namespaceManager;
        private Dictionary<XmlSchemaInfo, XmlSchemaInfo> schemaInfos;
        private XmlSchemaSet schemas;
        private XObject source;
        private ValidationEventHandler validationEventHandler;
        private XmlSchemaValidator validator;
        private XName xsiNilName;
        private XName xsiTypeName;

        public XNodeValidator(XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
        {
            this.schemas = schemas;
            this.validationEventHandler = validationEventHandler;
            XNamespace namespace2 = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            this.xsiTypeName = namespace2.GetName("type");
            this.xsiNilName = namespace2.GetName("nil");
        }

        private XmlSchemaInfo GetDefaultAttributeSchemaInfo(XmlSchemaAttribute sa)
        {
            XmlSchemaInfo info = new XmlSchemaInfo {
                IsDefault = true,
                IsNil = false,
                SchemaAttribute = sa
            };
            XmlSchemaSimpleType attributeSchemaType = sa.AttributeSchemaType;
            info.SchemaType = attributeSchemaType;
            if (attributeSchemaType.Datatype.Variety == XmlSchemaDatatypeVariety.Union)
            {
                string defaultValue = this.GetDefaultValue(sa);
                foreach (XmlSchemaSimpleType type2 in ((XmlSchemaSimpleTypeUnion) attributeSchemaType.Content).BaseMemberTypes)
                {
                    object obj2 = null;
                    try
                    {
                        obj2 = type2.Datatype.ParseValue(defaultValue, this.schemas.NameTable, this.namespaceManager);
                    }
                    catch (XmlSchemaException)
                    {
                    }
                    if (obj2 != null)
                    {
                        info.MemberType = type2;
                        break;
                    }
                }
            }
            info.Validity = XmlSchemaValidity.Valid;
            return info;
        }

        private string GetDefaultValue(XmlSchemaAttribute sa)
        {
            XmlQualifiedName refName = sa.RefName;
            if (!refName.IsEmpty)
            {
                sa = this.schemas.GlobalAttributes[refName] as XmlSchemaAttribute;
                if (sa == null)
                {
                    return null;
                }
            }
            string fixedValue = sa.FixedValue;
            if (fixedValue != null)
            {
                return fixedValue;
            }
            return sa.DefaultValue;
        }

        private string GetDefaultValue(XmlSchemaElement se)
        {
            XmlQualifiedName refName = se.RefName;
            if (!refName.IsEmpty)
            {
                se = this.schemas.GlobalElements[refName] as XmlSchemaElement;
                if (se == null)
                {
                    return null;
                }
            }
            string fixedValue = se.FixedValue;
            if (fixedValue != null)
            {
                return fixedValue;
            }
            return se.DefaultValue;
        }

        private void PushAncestorsAndSelf(XElement e)
        {
            while (e != null)
            {
                XAttribute lastAttr = e.lastAttr;
                if (lastAttr != null)
                {
                    do
                    {
                        lastAttr = lastAttr.next;
                        if (lastAttr.IsNamespaceDeclaration)
                        {
                            string localName = lastAttr.Name.LocalName;
                            if (localName == "xmlns")
                            {
                                localName = string.Empty;
                            }
                            if (!this.namespaceManager.HasNamespace(localName))
                            {
                                this.namespaceManager.AddNamespace(localName, lastAttr.Value);
                            }
                        }
                    }
                    while (lastAttr != e.lastAttr);
                }
                e = e.parent as XElement;
            }
        }

        private void PushElement(XElement e, ref string xsiType, ref string xsiNil)
        {
            this.namespaceManager.PushScope();
            XAttribute lastAttr = e.lastAttr;
            if (lastAttr != null)
            {
                do
                {
                    lastAttr = lastAttr.next;
                    if (lastAttr.IsNamespaceDeclaration)
                    {
                        string localName = lastAttr.Name.LocalName;
                        if (localName == "xmlns")
                        {
                            localName = string.Empty;
                        }
                        this.namespaceManager.AddNamespace(localName, lastAttr.Value);
                    }
                    else
                    {
                        XName name = lastAttr.Name;
                        if (name == this.xsiTypeName)
                        {
                            xsiType = lastAttr.Value;
                        }
                        else if (name == this.xsiNilName)
                        {
                            xsiNil = lastAttr.Value;
                        }
                    }
                }
                while (lastAttr != e.lastAttr);
            }
        }

        private void ReplaceSchemaInfo(XObject o, XmlSchemaInfo schemaInfo)
        {
            if (this.schemaInfos == null)
            {
                this.schemaInfos = new Dictionary<XmlSchemaInfo, XmlSchemaInfo>(new XmlSchemaInfoEqualityComparer());
            }
            XmlSchemaInfo key = o.Annotation<XmlSchemaInfo>();
            if (key != null)
            {
                if (!this.schemaInfos.ContainsKey(key))
                {
                    this.schemaInfos.Add(key, key);
                }
                o.RemoveAnnotations<XmlSchemaInfo>();
            }
            if (!this.schemaInfos.TryGetValue(schemaInfo, out key))
            {
                key = schemaInfo;
                this.schemaInfos.Add(key, key);
            }
            o.AddAnnotation(key);
        }

        public void Validate(XObject source, XmlSchemaObject partialValidationType, bool addSchemaInfo)
        {
            this.source = source;
            this.addSchemaInfo = addSchemaInfo;
            XmlSchemaValidationFlags allowXmlAttributes = XmlSchemaValidationFlags.AllowXmlAttributes;
            XmlNodeType nodeType = source.NodeType;
            switch (nodeType)
            {
                case XmlNodeType.Element:
                    goto Label_009B;

                case XmlNodeType.Attribute:
                    if (!((XAttribute) source).IsNamespaceDeclaration)
                    {
                        if (source.Parent == null)
                        {
                            throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_MissingParent"));
                        }
                        goto Label_009B;
                    }
                    break;

                case XmlNodeType.Document:
                    source = ((XDocument) source).Root;
                    if (source == null)
                    {
                        throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_MissingRoot"));
                    }
                    allowXmlAttributes |= XmlSchemaValidationFlags.ProcessIdentityConstraints;
                    goto Label_009B;
            }
            throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_BadNodeType", new object[] { nodeType }));
        Label_009B:
            this.namespaceManager = new XmlNamespaceManager(this.schemas.NameTable);
            this.PushAncestorsAndSelf(source.Parent);
            this.validator = new XmlSchemaValidator(this.schemas.NameTable, this.schemas, this.namespaceManager, allowXmlAttributes);
            this.validator.ValidationEventHandler += new ValidationEventHandler(this.ValidationCallback);
            this.validator.XmlResolver = null;
            if (partialValidationType != null)
            {
                this.validator.Initialize(partialValidationType);
            }
            else
            {
                this.validator.Initialize();
            }
            if (nodeType == XmlNodeType.Attribute)
            {
                this.ValidateAttribute((XAttribute) source);
            }
            else
            {
                this.ValidateElement((XElement) source);
            }
            this.validator.EndValidation();
        }

        private void ValidateAttribute(XAttribute a)
        {
            XmlSchemaInfo schemaInfo = this.addSchemaInfo ? new XmlSchemaInfo() : null;
            this.source = a;
            this.validator.ValidateAttribute(a.Name.LocalName, a.Name.NamespaceName, a.Value, schemaInfo);
            if (this.addSchemaInfo)
            {
                this.ReplaceSchemaInfo(a, schemaInfo);
            }
        }

        private void ValidateAttributes(XElement e)
        {
            XAttribute lastAttr = e.lastAttr;
            if (lastAttr != null)
            {
                do
                {
                    lastAttr = lastAttr.next;
                    if (!lastAttr.IsNamespaceDeclaration)
                    {
                        this.ValidateAttribute(lastAttr);
                    }
                }
                while (lastAttr != e.lastAttr);
                this.source = e;
            }
            if (this.addSchemaInfo)
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
                foreach (XmlSchemaAttribute attribute2 in this.defaultAttributes)
                {
                    lastAttr = new XAttribute(XNamespace.Get(attribute2.QualifiedName.Namespace).GetName(attribute2.QualifiedName.Name), this.GetDefaultValue(attribute2));
                    this.ReplaceSchemaInfo(lastAttr, this.GetDefaultAttributeSchemaInfo(attribute2));
                    e.Add(lastAttr);
                }
            }
        }

        private void ValidateElement(XElement e)
        {
            XmlSchemaInfo schemaInfo = this.addSchemaInfo ? new XmlSchemaInfo() : null;
            string xsiType = null;
            string xsiNil = null;
            this.PushElement(e, ref xsiType, ref xsiNil);
            this.source = e;
            this.validator.ValidateElement(e.Name.LocalName, e.Name.NamespaceName, schemaInfo, xsiType, xsiNil, null, null);
            this.ValidateAttributes(e);
            this.validator.ValidateEndOfAttributes(schemaInfo);
            this.ValidateNodes(e);
            this.validator.ValidateEndElement(schemaInfo);
            if (this.addSchemaInfo)
            {
                if ((schemaInfo.Validity == XmlSchemaValidity.Valid) && schemaInfo.IsDefault)
                {
                    e.Value = this.GetDefaultValue(schemaInfo.SchemaElement);
                }
                this.ReplaceSchemaInfo(e, schemaInfo);
            }
            this.namespaceManager.PopScope();
        }

        private void ValidateNodes(XElement e)
        {
            XNode content = e.content as XNode;
            if (content != null)
            {
                do
                {
                    content = content.next;
                    XElement element = content as XElement;
                    if (element != null)
                    {
                        this.ValidateElement(element);
                    }
                    else
                    {
                        XText text = content as XText;
                        if (text != null)
                        {
                            string elementValue = text.Value;
                            if (elementValue.Length > 0)
                            {
                                this.validator.ValidateText(elementValue);
                            }
                        }
                    }
                }
                while (content != e.content);
                this.source = e;
            }
            else
            {
                string str2 = e.content as string;
                if ((str2 != null) && (str2.Length > 0))
                {
                    this.validator.ValidateText(str2);
                }
            }
        }

        private void ValidationCallback(object sender, ValidationEventArgs e)
        {
            if (this.validationEventHandler != null)
            {
                this.validationEventHandler(this.source, e);
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                throw e.Exception;
            }
        }
    }
}

