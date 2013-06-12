namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml.Schema;

    internal class XsdValidatingReader : XmlReader, IXmlSchemaInfo, IXmlLineInfo, IXmlNamespaceResolver
    {
        private object atomicValue;
        private int attributeCount;
        private AttributePSVIInfo attributePSVI;
        private AttributePSVIInfo[] attributePSVINodes;
        private ValidatingReaderNodeData cachedNode;
        private XsdCachingReader cachingReader;
        private XmlReader coreReader;
        private int coreReaderAttributeCount;
        private XmlNameTable coreReaderNameTable;
        private IXmlNamespaceResolver coreReaderNSResolver;
        private int currentAttrIndex;
        private ArrayList defaultAttributes;
        private const int InitialAttributeCount = 8;
        private System.Xml.Schema.Parser inlineSchemaParser;
        private IXmlLineInfo lineInfo;
        private bool manageNamespaces;
        private XmlNamespaceManager nsManager;
        private string NsXmlNs;
        private string NsXs;
        private string NsXsi;
        private string originalAtomicValueString;
        private bool processInlineSchema;
        private ReadContentAsBinaryHelper readBinaryHelper;
        private bool replayCache;
        private ValidatingReaderState savedState;
        private ValidatingReaderNodeData textNode;
        private IXmlNamespaceResolver thisNSResolver;
        private static Type TypeOfString;
        private ValidationEventHandler validationEvent;
        private ValidatingReaderState validationState;
        private XmlSchemaValidator validator;
        private XmlValueGetter valueGetter;
        private XmlCharType xmlCharType;
        private XmlResolver xmlResolver;
        private XmlSchemaInfo xmlSchemaInfo;
        private string XsdSchema;
        private string XsiNil;
        private string XsiNoNamespaceSchemaLocation;
        private string XsiSchemaLocation;
        private string XsiType;

        internal XsdValidatingReader(XmlReader reader, XmlResolver xmlResolver, XmlReaderSettings readerSettings) : this(reader, xmlResolver, readerSettings, null)
        {
        }

        internal XsdValidatingReader(XmlReader reader, XmlResolver xmlResolver, XmlReaderSettings readerSettings, XmlSchemaObject partialValidationType)
        {
            this.xmlCharType = XmlCharType.Instance;
            this.coreReader = reader;
            this.coreReaderNSResolver = reader as IXmlNamespaceResolver;
            this.lineInfo = reader as IXmlLineInfo;
            this.coreReaderNameTable = this.coreReader.NameTable;
            if (this.coreReaderNSResolver == null)
            {
                this.nsManager = new XmlNamespaceManager(this.coreReaderNameTable);
                this.manageNamespaces = true;
            }
            this.thisNSResolver = this;
            this.xmlResolver = xmlResolver;
            this.processInlineSchema = (readerSettings.ValidationFlags & XmlSchemaValidationFlags.ProcessInlineSchema) != XmlSchemaValidationFlags.None;
            this.Init();
            this.SetupValidator(readerSettings, reader, partialValidationType);
            this.validationEvent = readerSettings.GetEventHandler();
        }

        private AttributePSVIInfo AddAttributePSVI(int attIndex)
        {
            AttributePSVIInfo info = this.attributePSVINodes[attIndex];
            if (info != null)
            {
                info.Reset();
                return info;
            }
            if (attIndex >= (this.attributePSVINodes.Length - 1))
            {
                AttributePSVIInfo[] destinationArray = new AttributePSVIInfo[this.attributePSVINodes.Length * 2];
                Array.Copy(this.attributePSVINodes, 0, destinationArray, 0, this.attributePSVINodes.Length);
                this.attributePSVINodes = destinationArray;
            }
            info = this.attributePSVINodes[attIndex];
            if (info == null)
            {
                info = new AttributePSVIInfo();
                this.attributePSVINodes[attIndex] = info;
            }
            return info;
        }

        internal void CachingCallBack(XsdCachingReader cachingReader)
        {
            this.coreReader = cachingReader.GetCoreReader();
            this.lineInfo = cachingReader.GetLineInfo();
            this.replayCache = false;
        }

        private void ClearAttributesInfo()
        {
            this.attributeCount = 0;
            this.coreReaderAttributeCount = 0;
            this.currentAttrIndex = -1;
            this.defaultAttributes.Clear();
            this.attributePSVI = null;
        }

        public override void Close()
        {
            this.coreReader.Close();
            this.validationState = ValidatingReaderState.ReaderClosed;
        }

        internal ValidatingReaderNodeData CreateDummyTextNode(string attributeValue, int depth)
        {
            if (this.textNode == null)
            {
                this.textNode = new ValidatingReaderNodeData(XmlNodeType.Text);
            }
            this.textNode.Depth = depth;
            this.textNode.RawValue = attributeValue;
            return this.textNode;
        }

        public override string GetAttribute(int i)
        {
            if (this.attributeCount == 0)
            {
                return null;
            }
            if (i < this.coreReaderAttributeCount)
            {
                return this.coreReader.GetAttribute(i);
            }
            int num = i - this.coreReaderAttributeCount;
            ValidatingReaderNodeData data = (ValidatingReaderNodeData) this.defaultAttributes[num];
            return data.RawValue;
        }

        public override string GetAttribute(string name)
        {
            string attribute = this.coreReader.GetAttribute(name);
            if ((attribute == null) && (this.attributeCount > 0))
            {
                ValidatingReaderNodeData defaultAttribute = this.GetDefaultAttribute(name, false);
                if (defaultAttribute != null)
                {
                    attribute = defaultAttribute.RawValue;
                }
            }
            return attribute;
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            string attribute = this.coreReader.GetAttribute(name, namespaceURI);
            if ((attribute == null) && (this.attributeCount > 0))
            {
                namespaceURI = (namespaceURI == null) ? string.Empty : this.coreReaderNameTable.Get(namespaceURI);
                name = this.coreReaderNameTable.Get(name);
                if ((name == null) || (namespaceURI == null))
                {
                    return null;
                }
                ValidatingReaderNodeData data = this.GetDefaultAttribute(name, namespaceURI, false);
                if (data != null)
                {
                    return data.RawValue;
                }
            }
            return attribute;
        }

        private AttributePSVIInfo GetAttributePSVI(string name)
        {
            string str;
            string str2;
            string str3;
            if (this.inlineSchemaParser != null)
            {
                return null;
            }
            ValidateNames.SplitQName(name, out str2, out str);
            str2 = this.coreReaderNameTable.Add(str2);
            str = this.coreReaderNameTable.Add(str);
            if (str2.Length == 0)
            {
                str3 = string.Empty;
            }
            else
            {
                str3 = this.thisNSResolver.LookupNamespace(str2);
            }
            return this.GetAttributePSVI(str, str3);
        }

        private AttributePSVIInfo GetAttributePSVI(string localName, string ns)
        {
            AttributePSVIInfo info = null;
            for (int i = 0; i < this.coreReaderAttributeCount; i++)
            {
                info = this.attributePSVINodes[i];
                if (((info != null) && Ref.Equal(localName, info.localName)) && Ref.Equal(ns, info.namespaceUri))
                {
                    this.currentAttrIndex = i;
                    return info;
                }
            }
            return null;
        }

        private XsdCachingReader GetCachingReader()
        {
            if (this.cachingReader == null)
            {
                this.cachingReader = new XsdCachingReader(this.coreReader, this.lineInfo, new CachingEventHandler(this.CachingCallBack));
            }
            else
            {
                this.cachingReader.Reset(this.coreReader);
            }
            this.lineInfo = this.cachingReader;
            return this.cachingReader;
        }

        private ValidatingReaderNodeData GetDefaultAttribute(string name, bool updatePosition)
        {
            string str;
            string str2;
            string str3;
            ValidateNames.SplitQName(name, out str2, out str);
            str2 = this.coreReaderNameTable.Add(str2);
            str = this.coreReaderNameTable.Add(str);
            if (str2.Length == 0)
            {
                str3 = string.Empty;
            }
            else
            {
                str3 = this.thisNSResolver.LookupNamespace(str2);
            }
            return this.GetDefaultAttribute(str, str3, updatePosition);
        }

        private ValidatingReaderNodeData GetDefaultAttribute(string attrLocalName, string ns, bool updatePosition)
        {
            ValidatingReaderNodeData data = null;
            for (int i = 0; i < this.defaultAttributes.Count; i++)
            {
                data = (ValidatingReaderNodeData) this.defaultAttributes[i];
                if (Ref.Equal(data.LocalName, attrLocalName) && Ref.Equal(data.Namespace, ns))
                {
                    if (updatePosition)
                    {
                        this.currentAttrIndex = this.coreReader.AttributeCount + i;
                    }
                    return data;
                }
            }
            return null;
        }

        private void GetIsDefault()
        {
            if (!(this.coreReader is XsdCachingReader) && this.xmlSchemaInfo.HasDefaultValue)
            {
                this.coreReader = this.GetCachingReader();
                if (this.xmlSchemaInfo.IsUnionType && !this.xmlSchemaInfo.IsNil)
                {
                    this.ReadAheadForMemberType();
                }
                else if (this.coreReader.Read())
                {
                    switch (this.coreReader.NodeType)
                    {
                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                            this.validator.ValidateText(new XmlValueGetter(this.GetStringValue));
                            break;

                        case XmlNodeType.Whitespace:
                        case XmlNodeType.SignificantWhitespace:
                            this.validator.ValidateWhitespace(new XmlValueGetter(this.GetStringValue));
                            break;

                        case XmlNodeType.EndElement:
                            this.atomicValue = this.validator.ValidateEndElement(this.xmlSchemaInfo);
                            this.originalAtomicValueString = this.GetOriginalAtomicValueStringOfElement();
                            if (this.xmlSchemaInfo.IsDefault)
                            {
                                this.cachingReader.SwitchTextNodeAndEndElement(this.xmlSchemaInfo.XmlType.ValueConverter.ToString(this.atomicValue), this.originalAtomicValueString);
                            }
                            break;
                    }
                }
                this.cachingReader.SetToReplayMode();
                this.replayCache = true;
            }
        }

        private void GetMemberType()
        {
            if (((this.xmlSchemaInfo.MemberType == null) && (this.atomicValue != this)) && ((!(this.coreReader is XsdCachingReader) && this.xmlSchemaInfo.IsUnionType) && !this.xmlSchemaInfo.IsNil))
            {
                this.coreReader = this.GetCachingReader();
                this.ReadAheadForMemberType();
                this.cachingReader.SetToReplayMode();
                this.replayCache = true;
            }
        }

        private string GetOriginalAtomicValueStringOfElement()
        {
            if (!this.xmlSchemaInfo.IsDefault)
            {
                return this.validator.GetConcatenatedValue();
            }
            XmlSchemaElement schemaElement = this.xmlSchemaInfo.SchemaElement;
            if (schemaElement == null)
            {
                return string.Empty;
            }
            if (schemaElement.DefaultValue == null)
            {
                return schemaElement.FixedValue;
            }
            return schemaElement.DefaultValue;
        }

        private object GetStringValue()
        {
            return this.coreReader.Value;
        }

        public bool HasLineInfo()
        {
            return true;
        }

        private void Init()
        {
            this.validationState = ValidatingReaderState.Init;
            this.defaultAttributes = new ArrayList();
            this.currentAttrIndex = -1;
            this.attributePSVINodes = new AttributePSVIInfo[8];
            this.valueGetter = new XmlValueGetter(this.GetStringValue);
            TypeOfString = typeof(string);
            this.xmlSchemaInfo = new XmlSchemaInfo();
            this.NsXmlNs = this.coreReaderNameTable.Add("http://www.w3.org/2000/xmlns/");
            this.NsXs = this.coreReaderNameTable.Add("http://www.w3.org/2001/XMLSchema");
            this.NsXsi = this.coreReaderNameTable.Add("http://www.w3.org/2001/XMLSchema-instance");
            this.XsiType = this.coreReaderNameTable.Add("type");
            this.XsiNil = this.coreReaderNameTable.Add("nil");
            this.XsiSchemaLocation = this.coreReaderNameTable.Add("schemaLocation");
            this.XsiNoNamespaceSchemaLocation = this.coreReaderNameTable.Add("noNamespaceSchemaLocation");
            this.XsdSchema = this.coreReaderNameTable.Add("schema");
        }

        private object InternalReadContentAsObject()
        {
            return this.InternalReadContentAsObject(false);
        }

        private object InternalReadContentAsObject(bool unwrapTypedValue)
        {
            string str;
            return this.InternalReadContentAsObject(unwrapTypedValue, out str);
        }

        private object InternalReadContentAsObject(bool unwrapTypedValue, out string originalStringValue)
        {
            switch (this.NodeType)
            {
                case XmlNodeType.Attribute:
                    originalStringValue = this.Value;
                    if ((this.attributePSVI == null) || (this.attributePSVI.typedAttributeValue == null))
                    {
                        return this.Value;
                    }
                    if (this.validationState == ValidatingReaderState.OnDefaultAttribute)
                    {
                        XmlSchemaAttribute schemaAttribute = this.attributePSVI.attributeSchemaInfo.SchemaAttribute;
                        originalStringValue = (schemaAttribute.DefaultValue != null) ? schemaAttribute.DefaultValue : schemaAttribute.FixedValue;
                    }
                    return this.ReturnBoxedValue(this.attributePSVI.typedAttributeValue, this.AttributeSchemaInfo.XmlType, unwrapTypedValue);

                case XmlNodeType.EndElement:
                    if (this.atomicValue != null)
                    {
                        originalStringValue = this.originalAtomicValueString;
                        return this.atomicValue;
                    }
                    originalStringValue = string.Empty;
                    return string.Empty;
            }
            if (this.validator.CurrentContentType == XmlSchemaContentType.TextOnly)
            {
                object obj2 = this.ReturnBoxedValue(this.ReadTillEndElement(), this.xmlSchemaInfo.XmlType, unwrapTypedValue);
                originalStringValue = this.originalAtomicValueString;
                return obj2;
            }
            XsdCachingReader coreReader = this.coreReader as XsdCachingReader;
            if (coreReader != null)
            {
                originalStringValue = coreReader.ReadOriginalContentAsString();
                return originalStringValue;
            }
            originalStringValue = base.InternalReadContentAsString();
            return originalStringValue;
        }

        private object InternalReadElementContentAsObject(out XmlSchemaType xmlType)
        {
            return this.InternalReadElementContentAsObject(out xmlType, false);
        }

        private object InternalReadElementContentAsObject(out XmlSchemaType xmlType, bool unwrapTypedValue)
        {
            string str;
            return this.InternalReadElementContentAsObject(out xmlType, unwrapTypedValue, out str);
        }

        private object InternalReadElementContentAsObject(out XmlSchemaType xmlType, bool unwrapTypedValue, out string originalString)
        {
            object atomicValue = null;
            xmlType = null;
            if (this.IsEmptyElement)
            {
                if (this.xmlSchemaInfo.ContentType == XmlSchemaContentType.TextOnly)
                {
                    atomicValue = this.ReturnBoxedValue(this.atomicValue, this.xmlSchemaInfo.XmlType, unwrapTypedValue);
                }
                else
                {
                    atomicValue = this.atomicValue;
                }
                originalString = this.originalAtomicValueString;
                xmlType = this.ElementXmlType;
                this.Read();
                return atomicValue;
            }
            this.Read();
            if (this.NodeType == XmlNodeType.EndElement)
            {
                if (this.xmlSchemaInfo.IsDefault)
                {
                    if (this.xmlSchemaInfo.ContentType == XmlSchemaContentType.TextOnly)
                    {
                        atomicValue = this.ReturnBoxedValue(this.atomicValue, this.xmlSchemaInfo.XmlType, unwrapTypedValue);
                    }
                    else
                    {
                        atomicValue = this.atomicValue;
                    }
                    originalString = this.originalAtomicValueString;
                }
                else
                {
                    atomicValue = string.Empty;
                    originalString = string.Empty;
                }
            }
            else
            {
                if (this.NodeType == XmlNodeType.Element)
                {
                    throw new XmlException("Xml_MixedReadElementContentAs", string.Empty, this);
                }
                atomicValue = this.InternalReadContentAsObject(unwrapTypedValue, out originalString);
                if (this.NodeType != XmlNodeType.EndElement)
                {
                    throw new XmlException("Xml_MixedReadElementContentAs", string.Empty, this);
                }
            }
            xmlType = this.ElementXmlType;
            this.Read();
            return atomicValue;
        }

        private bool IsXSDRoot(string localName, string ns)
        {
            return (Ref.Equal(ns, this.NsXs) && Ref.Equal(localName, this.XsdSchema));
        }

        public override string LookupNamespace(string prefix)
        {
            return this.thisNSResolver.LookupNamespace(prefix);
        }

        public override void MoveToAttribute(int i)
        {
            if ((i < 0) || (i >= this.attributeCount))
            {
                throw new ArgumentOutOfRangeException("i");
            }
            this.currentAttrIndex = i;
            if (i < this.coreReaderAttributeCount)
            {
                this.coreReader.MoveToAttribute(i);
                if (this.inlineSchemaParser == null)
                {
                    this.attributePSVI = this.attributePSVINodes[i];
                }
                else
                {
                    this.attributePSVI = null;
                }
                this.validationState = ValidatingReaderState.OnAttribute;
            }
            else
            {
                int num = i - this.coreReaderAttributeCount;
                this.cachedNode = (ValidatingReaderNodeData) this.defaultAttributes[num];
                this.attributePSVI = this.cachedNode.AttInfo;
                this.validationState = ValidatingReaderState.OnDefaultAttribute;
            }
            if (this.validationState == ValidatingReaderState.OnReadBinaryContent)
            {
                this.readBinaryHelper.Finish();
                this.validationState = this.savedState;
            }
        }

        public override bool MoveToAttribute(string name)
        {
            if (this.coreReader.MoveToAttribute(name))
            {
                this.validationState = ValidatingReaderState.OnAttribute;
                this.attributePSVI = this.GetAttributePSVI(name);
            }
            else
            {
                if (this.attributeCount > 0)
                {
                    ValidatingReaderNodeData defaultAttribute = this.GetDefaultAttribute(name, true);
                    if (defaultAttribute != null)
                    {
                        this.validationState = ValidatingReaderState.OnDefaultAttribute;
                        this.attributePSVI = defaultAttribute.AttInfo;
                        this.cachedNode = defaultAttribute;
                        goto Label_0057;
                    }
                }
                return false;
            }
        Label_0057:
            if (this.validationState == ValidatingReaderState.OnReadBinaryContent)
            {
                this.readBinaryHelper.Finish();
                this.validationState = this.savedState;
            }
            return true;
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            name = this.coreReaderNameTable.Get(name);
            ns = (ns != null) ? this.coreReaderNameTable.Get(ns) : string.Empty;
            if ((name == null) || (ns == null))
            {
                return false;
            }
            if (this.coreReader.MoveToAttribute(name, ns))
            {
                this.validationState = ValidatingReaderState.OnAttribute;
                if (this.inlineSchemaParser == null)
                {
                    this.attributePSVI = this.GetAttributePSVI(name, ns);
                }
                else
                {
                    this.attributePSVI = null;
                }
            }
            else
            {
                ValidatingReaderNodeData data = this.GetDefaultAttribute(name, ns, true);
                if (data != null)
                {
                    this.attributePSVI = data.AttInfo;
                    this.cachedNode = data;
                    this.validationState = ValidatingReaderState.OnDefaultAttribute;
                }
                else
                {
                    return false;
                }
            }
            if (this.validationState == ValidatingReaderState.OnReadBinaryContent)
            {
                this.readBinaryHelper.Finish();
                this.validationState = this.savedState;
            }
            return true;
        }

        public override bool MoveToElement()
        {
            if (!this.coreReader.MoveToElement() && (this.validationState >= ValidatingReaderState.None))
            {
                return false;
            }
            this.currentAttrIndex = -1;
            this.validationState = ValidatingReaderState.ClearAttributes;
            return true;
        }

        public override bool MoveToFirstAttribute()
        {
            if (this.coreReader.MoveToFirstAttribute())
            {
                this.currentAttrIndex = 0;
                if (this.inlineSchemaParser == null)
                {
                    this.attributePSVI = this.attributePSVINodes[0];
                }
                else
                {
                    this.attributePSVI = null;
                }
                this.validationState = ValidatingReaderState.OnAttribute;
            }
            else if (this.defaultAttributes.Count > 0)
            {
                this.cachedNode = (ValidatingReaderNodeData) this.defaultAttributes[0];
                this.attributePSVI = this.cachedNode.AttInfo;
                this.currentAttrIndex = 0;
                this.validationState = ValidatingReaderState.OnDefaultAttribute;
            }
            else
            {
                return false;
            }
            if (this.validationState == ValidatingReaderState.OnReadBinaryContent)
            {
                this.readBinaryHelper.Finish();
                this.validationState = this.savedState;
            }
            return true;
        }

        public override bool MoveToNextAttribute()
        {
            if ((this.currentAttrIndex + 1) < this.coreReaderAttributeCount)
            {
                this.coreReader.MoveToNextAttribute();
                this.currentAttrIndex++;
                if (this.inlineSchemaParser == null)
                {
                    this.attributePSVI = this.attributePSVINodes[this.currentAttrIndex];
                }
                else
                {
                    this.attributePSVI = null;
                }
                this.validationState = ValidatingReaderState.OnAttribute;
            }
            else if ((this.currentAttrIndex + 1) < this.attributeCount)
            {
                int num = ++this.currentAttrIndex - this.coreReaderAttributeCount;
                this.cachedNode = (ValidatingReaderNodeData) this.defaultAttributes[num];
                this.attributePSVI = this.cachedNode.AttInfo;
                this.validationState = ValidatingReaderState.OnDefaultAttribute;
            }
            else
            {
                return false;
            }
            if (this.validationState == ValidatingReaderState.OnReadBinaryContent)
            {
                this.readBinaryHelper.Finish();
                this.validationState = this.savedState;
            }
            return true;
        }

        private void ProcessElementEvent()
        {
            if ((this.processInlineSchema && this.IsXSDRoot(this.coreReader.LocalName, this.coreReader.NamespaceURI)) && (this.coreReader.Depth > 0))
            {
                this.xmlSchemaInfo.Clear();
                this.attributeCount = this.coreReaderAttributeCount = this.coreReader.AttributeCount;
                if (!this.coreReader.IsEmptyElement)
                {
                    this.inlineSchemaParser = new System.Xml.Schema.Parser(SchemaType.XSD, this.coreReaderNameTable, this.validator.SchemaSet.GetSchemaNames(this.coreReaderNameTable), this.validationEvent);
                    this.inlineSchemaParser.StartParsing(this.coreReader, null);
                    this.inlineSchemaParser.ParseReaderNode();
                    this.validationState = ValidatingReaderState.ParseInlineSchema;
                }
                else
                {
                    this.validationState = ValidatingReaderState.ClearAttributes;
                }
            }
            else
            {
                this.atomicValue = null;
                this.originalAtomicValueString = null;
                this.xmlSchemaInfo.Clear();
                if (this.manageNamespaces)
                {
                    this.nsManager.PushScope();
                }
                string xsiSchemaLocation = null;
                string xsiNoNamespaceSchemaLocation = null;
                string xsiNil = null;
                string xsiType = null;
                if (this.coreReader.MoveToFirstAttribute())
                {
                    do
                    {
                        string namespaceURI = this.coreReader.NamespaceURI;
                        string localName = this.coreReader.LocalName;
                        if (Ref.Equal(namespaceURI, this.NsXsi))
                        {
                            if (Ref.Equal(localName, this.XsiSchemaLocation))
                            {
                                xsiSchemaLocation = this.coreReader.Value;
                            }
                            else if (Ref.Equal(localName, this.XsiNoNamespaceSchemaLocation))
                            {
                                xsiNoNamespaceSchemaLocation = this.coreReader.Value;
                            }
                            else if (Ref.Equal(localName, this.XsiType))
                            {
                                xsiType = this.coreReader.Value;
                            }
                            else if (Ref.Equal(localName, this.XsiNil))
                            {
                                xsiNil = this.coreReader.Value;
                            }
                        }
                        if (this.manageNamespaces && Ref.Equal(this.coreReader.NamespaceURI, this.NsXmlNs))
                        {
                            this.nsManager.AddNamespace((this.coreReader.Prefix.Length == 0) ? string.Empty : this.coreReader.LocalName, this.coreReader.Value);
                        }
                    }
                    while (this.coreReader.MoveToNextAttribute());
                    this.coreReader.MoveToElement();
                }
                this.validator.ValidateElement(this.coreReader.LocalName, this.coreReader.NamespaceURI, this.xmlSchemaInfo, xsiType, xsiNil, xsiSchemaLocation, xsiNoNamespaceSchemaLocation);
                this.ValidateAttributes();
                this.validator.ValidateEndOfAttributes(this.xmlSchemaInfo);
                if (this.coreReader.IsEmptyElement)
                {
                    this.ProcessEndElementEvent();
                }
                this.validationState = ValidatingReaderState.ClearAttributes;
            }
        }

        private void ProcessEndElementEvent()
        {
            this.atomicValue = this.validator.ValidateEndElement(this.xmlSchemaInfo);
            this.originalAtomicValueString = this.GetOriginalAtomicValueStringOfElement();
            if (this.xmlSchemaInfo.IsDefault)
            {
                int depth = this.coreReader.Depth;
                this.coreReader = this.GetCachingReader();
                this.cachingReader.RecordTextNode(this.xmlSchemaInfo.XmlType.ValueConverter.ToString(this.atomicValue), this.originalAtomicValueString, depth + 1, 0, 0);
                this.cachingReader.RecordEndElementNode();
                this.cachingReader.SetToReplayMode();
                this.replayCache = true;
            }
            else if (this.manageNamespaces)
            {
                this.nsManager.PopScope();
            }
        }

        private void ProcessInlineSchema()
        {
            if (this.coreReader.Read())
            {
                if (this.coreReader.NodeType == XmlNodeType.Element)
                {
                    this.attributeCount = this.coreReaderAttributeCount = this.coreReader.AttributeCount;
                }
                else
                {
                    this.ClearAttributesInfo();
                }
                if (!this.inlineSchemaParser.ParseReaderNode())
                {
                    this.inlineSchemaParser.FinishParsing();
                    XmlSchema xmlSchema = this.inlineSchemaParser.XmlSchema;
                    this.validator.AddSchema(xmlSchema);
                    this.inlineSchemaParser = null;
                    this.validationState = ValidatingReaderState.Read;
                }
            }
        }

        private void ProcessReaderEvent()
        {
            if (!this.replayCache)
            {
                switch (this.coreReader.NodeType)
                {
                    case XmlNodeType.Element:
                        this.ProcessElementEvent();
                        return;

                    case XmlNodeType.Attribute:
                    case XmlNodeType.Entity:
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.Document:
                    case XmlNodeType.DocumentFragment:
                    case XmlNodeType.Notation:
                        return;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        this.validator.ValidateText(new XmlValueGetter(this.GetStringValue));
                        return;

                    case XmlNodeType.EntityReference:
                        throw new InvalidOperationException();

                    case XmlNodeType.DocumentType:
                        this.validator.SetDtdSchemaInfo(this.coreReader.DtdInfo);
                        return;

                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        this.validator.ValidateWhitespace(new XmlValueGetter(this.GetStringValue));
                        return;

                    case XmlNodeType.EndElement:
                        this.ProcessEndElementEvent();
                        return;
                }
            }
        }

        public override bool Read()
        {
            switch (this.validationState)
            {
                case ValidatingReaderState.OnReadAttributeValue:
                case ValidatingReaderState.OnDefaultAttribute:
                case ValidatingReaderState.OnAttribute:
                case ValidatingReaderState.ClearAttributes:
                    this.ClearAttributesInfo();
                    if (this.inlineSchemaParser == null)
                    {
                        this.validationState = ValidatingReaderState.Read;
                        break;
                    }
                    this.validationState = ValidatingReaderState.ParseInlineSchema;
                    goto Label_007C;

                case ValidatingReaderState.Init:
                    this.validationState = ValidatingReaderState.Read;
                    if (this.coreReader.ReadState != System.Xml.ReadState.Interactive)
                    {
                        break;
                    }
                    this.ProcessReaderEvent();
                    return true;

                case ValidatingReaderState.Read:
                    break;

                case ValidatingReaderState.ParseInlineSchema:
                    goto Label_007C;

                case ValidatingReaderState.ReadAhead:
                    this.ClearAttributesInfo();
                    this.ProcessReaderEvent();
                    this.validationState = ValidatingReaderState.Read;
                    return true;

                case ValidatingReaderState.OnReadBinaryContent:
                    this.validationState = this.savedState;
                    this.readBinaryHelper.Finish();
                    return this.Read();

                case ValidatingReaderState.ReaderClosed:
                case ValidatingReaderState.EOF:
                    return false;

                default:
                    return false;
            }
            if (this.coreReader.Read())
            {
                this.ProcessReaderEvent();
                return true;
            }
            this.validator.EndValidation();
            if (this.coreReader.EOF)
            {
                this.validationState = ValidatingReaderState.EOF;
            }
            return false;
        Label_007C:
            this.ProcessInlineSchema();
            return true;
        }

        private void ReadAheadForMemberType()
        {
            while (this.coreReader.Read())
            {
                switch (this.coreReader.NodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.Attribute:
                    case XmlNodeType.EntityReference:
                    case XmlNodeType.Entity:
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.Document:
                    case XmlNodeType.DocumentType:
                    case XmlNodeType.DocumentFragment:
                    case XmlNodeType.Notation:
                    {
                        continue;
                    }
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    {
                        this.validator.ValidateText(new XmlValueGetter(this.GetStringValue));
                        continue;
                    }
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                    {
                        this.validator.ValidateWhitespace(new XmlValueGetter(this.GetStringValue));
                        continue;
                    }
                    case XmlNodeType.EndElement:
                        this.atomicValue = this.validator.ValidateEndElement(this.xmlSchemaInfo);
                        this.originalAtomicValueString = this.GetOriginalAtomicValueStringOfElement();
                        if (this.atomicValue != null)
                        {
                            break;
                        }
                        this.atomicValue = this;
                        return;

                    default:
                    {
                        continue;
                    }
                }
                if (!this.xmlSchemaInfo.IsDefault)
                {
                    break;
                }
                this.cachingReader.SwitchTextNodeAndEndElement(this.xmlSchemaInfo.XmlType.ValueConverter.ToString(this.atomicValue), this.originalAtomicValueString);
                return;
            }
        }

        public override bool ReadAttributeValue()
        {
            if (this.validationState == ValidatingReaderState.OnReadBinaryContent)
            {
                this.readBinaryHelper.Finish();
                this.validationState = this.savedState;
            }
            if (this.NodeType != XmlNodeType.Attribute)
            {
                return false;
            }
            if (this.validationState == ValidatingReaderState.OnDefaultAttribute)
            {
                this.cachedNode = this.CreateDummyTextNode(this.cachedNode.RawValue, this.cachedNode.Depth + 1);
                this.validationState = ValidatingReaderState.OnReadAttributeValue;
                return true;
            }
            return this.coreReader.ReadAttributeValue();
        }

        public override object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            string str;
            object obj3;
            if (!XmlReader.CanReadContentAs(this.NodeType))
            {
                throw base.CreateReadContentAsException("ReadContentAs");
            }
            object obj2 = this.InternalReadContentAsObject(false, out str);
            XmlSchemaType type = (this.NodeType == XmlNodeType.Attribute) ? this.AttributeXmlType : this.ElementXmlType;
            try
            {
                if (type != null)
                {
                    if ((returnType == typeof(DateTimeOffset)) && (type.Datatype is Datatype_dateTimeBase))
                    {
                        obj2 = str;
                    }
                    return type.ValueConverter.ChangeType(obj2, returnType);
                }
                obj3 = XmlUntypedConverter.Untyped.ChangeType(obj2, returnType, namespaceResolver);
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", returnType.ToString(), exception, this);
            }
            catch (InvalidCastException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", returnType.ToString(), exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", returnType.ToString(), exception3, this);
            }
            return obj3;
        }

        public override int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.validationState != ValidatingReaderState.OnReadBinaryContent)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
                this.savedState = this.validationState;
            }
            this.validationState = this.savedState;
            int num = this.readBinaryHelper.ReadContentAsBase64(buffer, index, count);
            this.savedState = this.validationState;
            this.validationState = ValidatingReaderState.OnReadBinaryContent;
            return num;
        }

        public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.validationState != ValidatingReaderState.OnReadBinaryContent)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
                this.savedState = this.validationState;
            }
            this.validationState = this.savedState;
            int num = this.readBinaryHelper.ReadContentAsBinHex(buffer, index, count);
            this.savedState = this.validationState;
            this.validationState = ValidatingReaderState.OnReadBinaryContent;
            return num;
        }

        public override bool ReadContentAsBoolean()
        {
            bool flag;
            if (!XmlReader.CanReadContentAs(this.NodeType))
            {
                throw base.CreateReadContentAsException("ReadContentAsBoolean");
            }
            object obj2 = this.InternalReadContentAsObject();
            XmlSchemaType type = (this.NodeType == XmlNodeType.Attribute) ? this.AttributeXmlType : this.ElementXmlType;
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToBoolean(obj2);
                }
                flag = XmlUntypedConverter.Untyped.ToBoolean(obj2);
            }
            catch (InvalidCastException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Boolean", exception, this);
            }
            catch (FormatException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Boolean", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Boolean", exception3, this);
            }
            return flag;
        }

        public override DateTime ReadContentAsDateTime()
        {
            DateTime time;
            if (!XmlReader.CanReadContentAs(this.NodeType))
            {
                throw base.CreateReadContentAsException("ReadContentAsDateTime");
            }
            object obj2 = this.InternalReadContentAsObject();
            XmlSchemaType type = (this.NodeType == XmlNodeType.Attribute) ? this.AttributeXmlType : this.ElementXmlType;
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToDateTime(obj2);
                }
                time = XmlUntypedConverter.Untyped.ToDateTime(obj2);
            }
            catch (InvalidCastException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "DateTime", exception, this);
            }
            catch (FormatException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "DateTime", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "DateTime", exception3, this);
            }
            return time;
        }

        public override decimal ReadContentAsDecimal()
        {
            decimal num;
            if (!XmlReader.CanReadContentAs(this.NodeType))
            {
                throw base.CreateReadContentAsException("ReadContentAsDecimal");
            }
            object obj2 = this.InternalReadContentAsObject();
            XmlSchemaType type = (this.NodeType == XmlNodeType.Attribute) ? this.AttributeXmlType : this.ElementXmlType;
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToDecimal(obj2);
                }
                num = XmlUntypedConverter.Untyped.ToDecimal(obj2);
            }
            catch (InvalidCastException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Decimal", exception, this);
            }
            catch (FormatException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Decimal", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Decimal", exception3, this);
            }
            return num;
        }

        public override double ReadContentAsDouble()
        {
            double num;
            if (!XmlReader.CanReadContentAs(this.NodeType))
            {
                throw base.CreateReadContentAsException("ReadContentAsDouble");
            }
            object obj2 = this.InternalReadContentAsObject();
            XmlSchemaType type = (this.NodeType == XmlNodeType.Attribute) ? this.AttributeXmlType : this.ElementXmlType;
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToDouble(obj2);
                }
                num = XmlUntypedConverter.Untyped.ToDouble(obj2);
            }
            catch (InvalidCastException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Double", exception, this);
            }
            catch (FormatException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Double", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Double", exception3, this);
            }
            return num;
        }

        public override float ReadContentAsFloat()
        {
            float num;
            if (!XmlReader.CanReadContentAs(this.NodeType))
            {
                throw base.CreateReadContentAsException("ReadContentAsFloat");
            }
            object obj2 = this.InternalReadContentAsObject();
            XmlSchemaType type = (this.NodeType == XmlNodeType.Attribute) ? this.AttributeXmlType : this.ElementXmlType;
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToSingle(obj2);
                }
                num = XmlUntypedConverter.Untyped.ToSingle(obj2);
            }
            catch (InvalidCastException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Float", exception, this);
            }
            catch (FormatException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Float", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Float", exception3, this);
            }
            return num;
        }

        public override int ReadContentAsInt()
        {
            int num;
            if (!XmlReader.CanReadContentAs(this.NodeType))
            {
                throw base.CreateReadContentAsException("ReadContentAsInt");
            }
            object obj2 = this.InternalReadContentAsObject();
            XmlSchemaType type = (this.NodeType == XmlNodeType.Attribute) ? this.AttributeXmlType : this.ElementXmlType;
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToInt32(obj2);
                }
                num = XmlUntypedConverter.Untyped.ToInt32(obj2);
            }
            catch (InvalidCastException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Int", exception, this);
            }
            catch (FormatException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Int", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Int", exception3, this);
            }
            return num;
        }

        public override long ReadContentAsLong()
        {
            long num;
            if (!XmlReader.CanReadContentAs(this.NodeType))
            {
                throw base.CreateReadContentAsException("ReadContentAsLong");
            }
            object obj2 = this.InternalReadContentAsObject();
            XmlSchemaType type = (this.NodeType == XmlNodeType.Attribute) ? this.AttributeXmlType : this.ElementXmlType;
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToInt64(obj2);
                }
                num = XmlUntypedConverter.Untyped.ToInt64(obj2);
            }
            catch (InvalidCastException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Long", exception, this);
            }
            catch (FormatException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Long", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Long", exception3, this);
            }
            return num;
        }

        public override object ReadContentAsObject()
        {
            if (!XmlReader.CanReadContentAs(this.NodeType))
            {
                throw base.CreateReadContentAsException("ReadContentAsObject");
            }
            return this.InternalReadContentAsObject(true);
        }

        public override string ReadContentAsString()
        {
            string str;
            if (!XmlReader.CanReadContentAs(this.NodeType))
            {
                throw base.CreateReadContentAsException("ReadContentAsString");
            }
            object obj2 = this.InternalReadContentAsObject();
            XmlSchemaType type = (this.NodeType == XmlNodeType.Attribute) ? this.AttributeXmlType : this.ElementXmlType;
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToString(obj2);
                }
                str = obj2 as string;
            }
            catch (InvalidCastException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "String", exception, this);
            }
            catch (FormatException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "String", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "String", exception3, this);
            }
            return str;
        }

        public override object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            XmlSchemaType type;
            string str;
            object obj3;
            if (this.NodeType != XmlNodeType.Element)
            {
                throw base.CreateReadElementContentAsException("ReadElementContentAs");
            }
            object obj2 = this.InternalReadElementContentAsObject(out type, false, out str);
            try
            {
                if (type != null)
                {
                    if ((returnType == typeof(DateTimeOffset)) && (type.Datatype is Datatype_dateTimeBase))
                    {
                        obj2 = str;
                    }
                    return type.ValueConverter.ChangeType(obj2, returnType, namespaceResolver);
                }
                obj3 = XmlUntypedConverter.Untyped.ChangeType(obj2, returnType, namespaceResolver);
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", returnType.ToString(), exception, this);
            }
            catch (InvalidCastException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", returnType.ToString(), exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", returnType.ToString(), exception3, this);
            }
            return obj3;
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.validationState != ValidatingReaderState.OnReadBinaryContent)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
                this.savedState = this.validationState;
            }
            this.validationState = this.savedState;
            int num = this.readBinaryHelper.ReadElementContentAsBase64(buffer, index, count);
            this.savedState = this.validationState;
            this.validationState = ValidatingReaderState.OnReadBinaryContent;
            return num;
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return 0;
            }
            if (this.validationState != ValidatingReaderState.OnReadBinaryContent)
            {
                this.readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(this.readBinaryHelper, this);
                this.savedState = this.validationState;
            }
            this.validationState = this.savedState;
            int num = this.readBinaryHelper.ReadElementContentAsBinHex(buffer, index, count);
            this.savedState = this.validationState;
            this.validationState = ValidatingReaderState.OnReadBinaryContent;
            return num;
        }

        public override bool ReadElementContentAsBoolean()
        {
            XmlSchemaType type;
            bool flag;
            if (this.NodeType != XmlNodeType.Element)
            {
                throw base.CreateReadElementContentAsException("ReadElementContentAsBoolean");
            }
            object obj2 = this.InternalReadElementContentAsObject(out type);
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToBoolean(obj2);
                }
                flag = XmlUntypedConverter.Untyped.ToBoolean(obj2);
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Boolean", exception, this);
            }
            catch (InvalidCastException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Boolean", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Boolean", exception3, this);
            }
            return flag;
        }

        public override DateTime ReadElementContentAsDateTime()
        {
            XmlSchemaType type;
            DateTime time;
            if (this.NodeType != XmlNodeType.Element)
            {
                throw base.CreateReadElementContentAsException("ReadElementContentAsDateTime");
            }
            object obj2 = this.InternalReadElementContentAsObject(out type);
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToDateTime(obj2);
                }
                time = XmlUntypedConverter.Untyped.ToDateTime(obj2);
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "DateTime", exception, this);
            }
            catch (InvalidCastException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "DateTime", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "DateTime", exception3, this);
            }
            return time;
        }

        public override decimal ReadElementContentAsDecimal()
        {
            XmlSchemaType type;
            decimal num;
            if (this.NodeType != XmlNodeType.Element)
            {
                throw base.CreateReadElementContentAsException("ReadElementContentAsDecimal");
            }
            object obj2 = this.InternalReadElementContentAsObject(out type);
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToDecimal(obj2);
                }
                num = XmlUntypedConverter.Untyped.ToDecimal(obj2);
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Decimal", exception, this);
            }
            catch (InvalidCastException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Decimal", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Decimal", exception3, this);
            }
            return num;
        }

        public override double ReadElementContentAsDouble()
        {
            XmlSchemaType type;
            double num;
            if (this.NodeType != XmlNodeType.Element)
            {
                throw base.CreateReadElementContentAsException("ReadElementContentAsDouble");
            }
            object obj2 = this.InternalReadElementContentAsObject(out type);
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToDouble(obj2);
                }
                num = XmlUntypedConverter.Untyped.ToDouble(obj2);
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Double", exception, this);
            }
            catch (InvalidCastException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Double", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Double", exception3, this);
            }
            return num;
        }

        public override float ReadElementContentAsFloat()
        {
            XmlSchemaType type;
            float num;
            if (this.NodeType != XmlNodeType.Element)
            {
                throw base.CreateReadElementContentAsException("ReadElementContentAsFloat");
            }
            object obj2 = this.InternalReadElementContentAsObject(out type);
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToSingle(obj2);
                }
                num = XmlUntypedConverter.Untyped.ToSingle(obj2);
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Float", exception, this);
            }
            catch (InvalidCastException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Float", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Float", exception3, this);
            }
            return num;
        }

        public override int ReadElementContentAsInt()
        {
            XmlSchemaType type;
            int num;
            if (this.NodeType != XmlNodeType.Element)
            {
                throw base.CreateReadElementContentAsException("ReadElementContentAsInt");
            }
            object obj2 = this.InternalReadElementContentAsObject(out type);
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToInt32(obj2);
                }
                num = XmlUntypedConverter.Untyped.ToInt32(obj2);
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Int", exception, this);
            }
            catch (InvalidCastException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Int", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Int", exception3, this);
            }
            return num;
        }

        public override long ReadElementContentAsLong()
        {
            XmlSchemaType type;
            long num;
            if (this.NodeType != XmlNodeType.Element)
            {
                throw base.CreateReadElementContentAsException("ReadElementContentAsLong");
            }
            object obj2 = this.InternalReadElementContentAsObject(out type);
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToInt64(obj2);
                }
                num = XmlUntypedConverter.Untyped.ToInt64(obj2);
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Long", exception, this);
            }
            catch (InvalidCastException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Long", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Long", exception3, this);
            }
            return num;
        }

        public override object ReadElementContentAsObject()
        {
            XmlSchemaType type;
            if (this.NodeType != XmlNodeType.Element)
            {
                throw base.CreateReadElementContentAsException("ReadElementContentAsObject");
            }
            return this.InternalReadElementContentAsObject(out type, true);
        }

        public override string ReadElementContentAsString()
        {
            XmlSchemaType type;
            string str;
            if (this.NodeType != XmlNodeType.Element)
            {
                throw base.CreateReadElementContentAsException("ReadElementContentAsString");
            }
            object obj2 = this.InternalReadElementContentAsObject(out type);
            try
            {
                if (type != null)
                {
                    return type.ValueConverter.ToString(obj2);
                }
                str = obj2 as string;
            }
            catch (InvalidCastException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "String", exception, this);
            }
            catch (FormatException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "String", exception2, this);
            }
            catch (OverflowException exception3)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "String", exception3, this);
            }
            return str;
        }

        private object ReadTillEndElement()
        {
            if (this.atomicValue != null)
            {
                if (this.atomicValue == this)
                {
                    this.atomicValue = null;
                }
                this.SwitchReader();
            }
            else
            {
                while (this.coreReader.Read())
                {
                    if (!this.replayCache)
                    {
                        switch (this.coreReader.NodeType)
                        {
                            case XmlNodeType.Element:
                                this.ProcessReaderEvent();
                                goto Label_010B;

                            case XmlNodeType.Text:
                            case XmlNodeType.CDATA:
                                this.validator.ValidateText(new XmlValueGetter(this.GetStringValue));
                                break;

                            case XmlNodeType.Whitespace:
                            case XmlNodeType.SignificantWhitespace:
                                this.validator.ValidateWhitespace(new XmlValueGetter(this.GetStringValue));
                                break;

                            case XmlNodeType.EndElement:
                                this.atomicValue = this.validator.ValidateEndElement(this.xmlSchemaInfo);
                                this.originalAtomicValueString = this.GetOriginalAtomicValueStringOfElement();
                                if (this.manageNamespaces)
                                {
                                    this.nsManager.PopScope();
                                }
                                goto Label_010B;
                        }
                    }
                }
            }
        Label_010B:
            return this.atomicValue;
        }

        public override void ResolveEntity()
        {
            throw new InvalidOperationException();
        }

        private object ReturnBoxedValue(object typedValue, XmlSchemaType xmlType, bool unWrap)
        {
            if (typedValue != null)
            {
                if (unWrap && (xmlType.Datatype.Variety == XmlSchemaDatatypeVariety.List))
                {
                    Datatype_List datatype = xmlType.Datatype as Datatype_List;
                    if (datatype.ItemType.Variety == XmlSchemaDatatypeVariety.Union)
                    {
                        typedValue = xmlType.ValueConverter.ChangeType(typedValue, xmlType.Datatype.ValueType, this.thisNSResolver);
                    }
                }
                return typedValue;
            }
            typedValue = this.validator.GetConcatenatedValue();
            return typedValue;
        }

        private void SetupValidator(XmlReaderSettings readerSettings, XmlReader reader, XmlSchemaObject partialValidationType)
        {
            this.validator = new XmlSchemaValidator(this.coreReaderNameTable, readerSettings.Schemas, this.thisNSResolver, readerSettings.ValidationFlags);
            this.validator.XmlResolver = this.xmlResolver;
            this.validator.SourceUri = XmlConvert.ToUri(reader.BaseURI);
            this.validator.ValidationEventSender = this;
            this.validator.ValidationEventHandler += readerSettings.GetEventHandler();
            this.validator.LineInfoProvider = this.lineInfo;
            if (this.validator.ProcessSchemaHints)
            {
                this.validator.SchemaSet.ReaderSettings.DtdProcessing = readerSettings.DtdProcessing;
            }
            this.validator.SetDtdSchemaInfo(reader.DtdInfo);
            if (partialValidationType != null)
            {
                this.validator.Initialize(partialValidationType);
            }
            else
            {
                this.validator.Initialize();
            }
        }

        public override void Skip()
        {
            int depth = this.Depth;
            switch (this.NodeType)
            {
                case XmlNodeType.Element:
                    break;

                case XmlNodeType.Attribute:
                    this.MoveToElement();
                    break;

                default:
                    goto Label_0089;
            }
            if (!this.coreReader.IsEmptyElement)
            {
                bool flag = true;
                if ((this.xmlSchemaInfo.IsUnionType || this.xmlSchemaInfo.IsDefault) && (this.coreReader is XsdCachingReader))
                {
                    flag = false;
                }
                this.coreReader.Skip();
                this.validationState = ValidatingReaderState.ReadAhead;
                if (flag)
                {
                    this.validator.SkipToEndElement(this.xmlSchemaInfo);
                }
            }
        Label_0089:
            this.Read();
        }

        private void SwitchReader()
        {
            XsdCachingReader coreReader = this.coreReader as XsdCachingReader;
            if (coreReader != null)
            {
                this.coreReader = coreReader.GetCoreReader();
            }
            this.replayCache = false;
        }

        IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
        {
            if (this.coreReaderNSResolver != null)
            {
                return this.coreReaderNSResolver.GetNamespacesInScope(scope);
            }
            return this.nsManager.GetNamespacesInScope(scope);
        }

        string IXmlNamespaceResolver.LookupNamespace(string prefix)
        {
            if (this.coreReaderNSResolver != null)
            {
                return this.coreReaderNSResolver.LookupNamespace(prefix);
            }
            return this.nsManager.LookupNamespace(prefix);
        }

        string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
        {
            if (this.coreReaderNSResolver != null)
            {
                return this.coreReaderNSResolver.LookupPrefix(namespaceName);
            }
            return this.nsManager.LookupPrefix(namespaceName);
        }

        private void ValidateAttributes()
        {
            this.attributeCount = this.coreReaderAttributeCount = this.coreReader.AttributeCount;
            int attIndex = 0;
            bool flag = false;
            if (this.coreReader.MoveToFirstAttribute())
            {
                do
                {
                    string localName = this.coreReader.LocalName;
                    string namespaceURI = this.coreReader.NamespaceURI;
                    AttributePSVIInfo info = this.AddAttributePSVI(attIndex);
                    info.localName = localName;
                    info.namespaceUri = namespaceURI;
                    if (namespaceURI == this.NsXmlNs)
                    {
                        attIndex++;
                    }
                    else
                    {
                        info.typedAttributeValue = this.validator.ValidateAttribute(localName, namespaceURI, this.valueGetter, info.attributeSchemaInfo);
                        if (!flag)
                        {
                            flag = info.attributeSchemaInfo.Validity == XmlSchemaValidity.Invalid;
                        }
                        attIndex++;
                    }
                }
                while (this.coreReader.MoveToNextAttribute());
            }
            this.coreReader.MoveToElement();
            if (flag)
            {
                this.xmlSchemaInfo.Validity = XmlSchemaValidity.Invalid;
            }
            this.validator.GetUnspecifiedDefaultAttributes(this.defaultAttributes, true);
            this.attributeCount += this.defaultAttributes.Count;
        }

        public override int AttributeCount
        {
            get
            {
                return this.attributeCount;
            }
        }

        private XmlSchemaInfo AttributeSchemaInfo
        {
            get
            {
                return this.attributePSVI.attributeSchemaInfo;
            }
        }

        private XmlSchemaType AttributeXmlType
        {
            get
            {
                if (this.attributePSVI != null)
                {
                    return this.AttributeSchemaInfo.XmlType;
                }
                return null;
            }
        }

        public override string BaseURI
        {
            get
            {
                return this.coreReader.BaseURI;
            }
        }

        public override bool CanReadBinaryContent
        {
            get
            {
                return true;
            }
        }

        public override int Depth
        {
            get
            {
                if (this.validationState < ValidatingReaderState.None)
                {
                    return this.cachedNode.Depth;
                }
                return this.coreReader.Depth;
            }
        }

        private XmlSchemaType ElementXmlType
        {
            get
            {
                return this.xmlSchemaInfo.XmlType;
            }
        }

        public override bool EOF
        {
            get
            {
                return this.coreReader.EOF;
            }
        }

        public override bool HasValue
        {
            get
            {
                return ((this.validationState < ValidatingReaderState.None) || this.coreReader.HasValue);
            }
        }

        public override bool IsDefault
        {
            get
            {
                return ((this.validationState == ValidatingReaderState.OnDefaultAttribute) || this.coreReader.IsDefault);
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this.coreReader.IsEmptyElement;
            }
        }

        public int LineNumber
        {
            get
            {
                if (this.lineInfo != null)
                {
                    return this.lineInfo.LineNumber;
                }
                return 0;
            }
        }

        public int LinePosition
        {
            get
            {
                if (this.lineInfo != null)
                {
                    return this.lineInfo.LinePosition;
                }
                return 0;
            }
        }

        public override string LocalName
        {
            get
            {
                if (this.validationState < ValidatingReaderState.None)
                {
                    return this.cachedNode.LocalName;
                }
                return this.coreReader.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                if (this.validationState != ValidatingReaderState.OnDefaultAttribute)
                {
                    return this.coreReader.Name;
                }
                string defaultAttributePrefix = this.validator.GetDefaultAttributePrefix(this.cachedNode.Namespace);
                if ((defaultAttributePrefix != null) && (defaultAttributePrefix.Length != 0))
                {
                    return ((defaultAttributePrefix + ":" + this.cachedNode.LocalName));
                }
                return this.cachedNode.LocalName;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                if (this.validationState < ValidatingReaderState.None)
                {
                    return this.cachedNode.Namespace;
                }
                return this.coreReader.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.coreReaderNameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                if (this.validationState < ValidatingReaderState.None)
                {
                    return this.cachedNode.NodeType;
                }
                XmlNodeType nodeType = this.coreReader.NodeType;
                if ((nodeType != XmlNodeType.Whitespace) || ((this.validator.CurrentContentType != XmlSchemaContentType.TextOnly) && (this.validator.CurrentContentType != XmlSchemaContentType.Mixed)))
                {
                    return nodeType;
                }
                return XmlNodeType.SignificantWhitespace;
            }
        }

        public override string Prefix
        {
            get
            {
                if (this.validationState < ValidatingReaderState.None)
                {
                    return this.cachedNode.Prefix;
                }
                return this.coreReader.Prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return this.coreReader.QuoteChar;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                if (this.validationState != ValidatingReaderState.Init)
                {
                    return this.coreReader.ReadState;
                }
                return System.Xml.ReadState.Initial;
            }
        }

        public override IXmlSchemaInfo SchemaInfo
        {
            get
            {
                return this;
            }
        }

        public override XmlReaderSettings Settings
        {
            get
            {
                XmlReaderSettings settings = this.coreReader.Settings;
                if (settings != null)
                {
                    settings = settings.Clone();
                }
                if (settings == null)
                {
                    settings = new XmlReaderSettings();
                }
                settings.Schemas = this.validator.SchemaSet;
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags = this.validator.ValidationFlags;
                settings.ReadOnly = true;
                return settings;
            }
        }

        bool IXmlSchemaInfo.IsDefault
        {
            get
            {
                switch (this.NodeType)
                {
                    case XmlNodeType.Element:
                        if (!this.coreReader.IsEmptyElement)
                        {
                            this.GetIsDefault();
                        }
                        return this.xmlSchemaInfo.IsDefault;

                    case XmlNodeType.Attribute:
                        if (this.attributePSVI == null)
                        {
                            break;
                        }
                        return this.AttributeSchemaInfo.IsDefault;

                    case XmlNodeType.EndElement:
                        return this.xmlSchemaInfo.IsDefault;
                }
                return false;
            }
        }

        bool IXmlSchemaInfo.IsNil
        {
            get
            {
                XmlNodeType nodeType = this.NodeType;
                if ((nodeType != XmlNodeType.Element) && (nodeType != XmlNodeType.EndElement))
                {
                    return false;
                }
                return this.xmlSchemaInfo.IsNil;
            }
        }

        XmlSchemaSimpleType IXmlSchemaInfo.MemberType
        {
            get
            {
                switch (this.NodeType)
                {
                    case XmlNodeType.Element:
                        if (!this.coreReader.IsEmptyElement)
                        {
                            this.GetMemberType();
                        }
                        return this.xmlSchemaInfo.MemberType;

                    case XmlNodeType.Attribute:
                        if (this.attributePSVI == null)
                        {
                            return null;
                        }
                        return this.AttributeSchemaInfo.MemberType;

                    case XmlNodeType.EndElement:
                        return this.xmlSchemaInfo.MemberType;
                }
                return null;
            }
        }

        XmlSchemaAttribute IXmlSchemaInfo.SchemaAttribute
        {
            get
            {
                if ((this.NodeType == XmlNodeType.Attribute) && (this.attributePSVI != null))
                {
                    return this.AttributeSchemaInfo.SchemaAttribute;
                }
                return null;
            }
        }

        XmlSchemaElement IXmlSchemaInfo.SchemaElement
        {
            get
            {
                if ((this.NodeType != XmlNodeType.Element) && (this.NodeType != XmlNodeType.EndElement))
                {
                    return null;
                }
                return this.xmlSchemaInfo.SchemaElement;
            }
        }

        XmlSchemaType IXmlSchemaInfo.SchemaType
        {
            get
            {
                switch (this.NodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.EndElement:
                        return this.xmlSchemaInfo.SchemaType;

                    case XmlNodeType.Attribute:
                        if (this.attributePSVI == null)
                        {
                            return null;
                        }
                        return this.AttributeSchemaInfo.SchemaType;
                }
                return null;
            }
        }

        XmlSchemaValidity IXmlSchemaInfo.Validity
        {
            get
            {
                switch (this.NodeType)
                {
                    case XmlNodeType.Element:
                        if (!this.coreReader.IsEmptyElement)
                        {
                            if (this.xmlSchemaInfo.Validity == XmlSchemaValidity.Valid)
                            {
                                return XmlSchemaValidity.NotKnown;
                            }
                            return this.xmlSchemaInfo.Validity;
                        }
                        return this.xmlSchemaInfo.Validity;

                    case XmlNodeType.Attribute:
                        if (this.attributePSVI == null)
                        {
                            break;
                        }
                        return this.AttributeSchemaInfo.Validity;

                    case XmlNodeType.EndElement:
                        return this.xmlSchemaInfo.Validity;
                }
                return XmlSchemaValidity.NotKnown;
            }
        }

        public override string Value
        {
            get
            {
                if (this.validationState < ValidatingReaderState.None)
                {
                    return this.cachedNode.RawValue;
                }
                return this.coreReader.Value;
            }
        }

        public override Type ValueType
        {
            get
            {
                switch (this.NodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.EndElement:
                        if (this.xmlSchemaInfo.ContentType == XmlSchemaContentType.TextOnly)
                        {
                            return this.xmlSchemaInfo.SchemaType.Datatype.ValueType;
                        }
                        break;

                    case XmlNodeType.Attribute:
                        if ((this.attributePSVI == null) || (this.AttributeSchemaInfo.ContentType != XmlSchemaContentType.TextOnly))
                        {
                            break;
                        }
                        return this.AttributeSchemaInfo.SchemaType.Datatype.ValueType;
                }
                return TypeOfString;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.coreReader.XmlLang;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this.coreReader.XmlSpace;
            }
        }

        private enum ValidatingReaderState
        {
            ClearAttributes = 4,
            EOF = 9,
            Error = 10,
            Init = 1,
            None = 0,
            OnAttribute = 3,
            OnDefaultAttribute = -1,
            OnReadAttributeValue = -2,
            OnReadBinaryContent = 7,
            ParseInlineSchema = 5,
            Read = 2,
            ReadAhead = 6,
            ReaderClosed = 8
        }
    }
}

