namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Security;
    using System.Xml;

    internal class ExtensionDataReader : XmlReader
    {
        private int attributeCount;
        private int attributeIndex = -1;
        private Hashtable cache = new Hashtable();
        private XmlObjectSerializerReadContext context;
        private int depth;
        private Queue<IDataNode> deserializedDataNodes;
        private ElementData element;
        private ElementData[] elements;
        private ExtensionDataNodeType internalNodeType;
        private string localName;
        private ElementData nextElement;
        private XmlNodeType nodeType;
        private string ns;
        [SecurityCritical]
        private static Dictionary<string, string> nsToPrefixTable = new Dictionary<string, string>();
        private string prefix;
        [SecurityCritical]
        private static Dictionary<string, string> prefixToNsTable = new Dictionary<string, string>();
        private System.Xml.ReadState readState;
        private string value;
        private XmlNodeReader xmlNodeReader;

        [SecuritySafeCritical]
        static ExtensionDataReader()
        {
            AddPrefix("i", "http://www.w3.org/2001/XMLSchema-instance");
            AddPrefix("z", "http://schemas.microsoft.com/2003/10/Serialization/");
            AddPrefix(string.Empty, string.Empty);
        }

        internal ExtensionDataReader(XmlObjectSerializerReadContext context)
        {
            this.context = context;
        }

        private void AddDeserializedDataNode(IDataNode node)
        {
            if ((node.Id != Globals.NewObjectId) && ((node.Value == null) || !node.IsFinalValue))
            {
                if (this.deserializedDataNodes == null)
                {
                    this.deserializedDataNodes = new Queue<IDataNode>();
                }
                this.deserializedDataNodes.Enqueue(node);
            }
        }

        [SecuritySafeCritical]
        private static void AddPrefix(string prefix, string ns)
        {
            nsToPrefixTable.Add(ns, prefix);
            prefixToNsTable.Add(prefix, ns);
        }

        private bool CheckIfNodeHandled(IDataNode node)
        {
            bool flag = false;
            if (node.Id != Globals.NewObjectId)
            {
                flag = this.cache[node] != null;
                if (flag)
                {
                    if (this.nextElement == null)
                    {
                        this.nextElement = this.GetNextElement();
                    }
                    this.nextElement.attributeCount = 0;
                    this.nextElement.AddAttribute("z", "http://schemas.microsoft.com/2003/10/Serialization/", "Ref", node.Id.ToString(NumberFormatInfo.InvariantInfo));
                    this.nextElement.AddAttribute("i", "http://www.w3.org/2001/XMLSchema-instance", "nil", "true");
                    this.internalNodeType = ExtensionDataNodeType.ReferencedElement;
                    return flag;
                }
                this.cache.Add(node, node);
            }
            return flag;
        }

        public override void Close()
        {
            if (this.IsXmlDataNode)
            {
                this.xmlNodeReader.Close();
            }
            else
            {
                this.Reset();
                this.readState = System.Xml.ReadState.Closed;
            }
        }

        public override string GetAttribute(int i)
        {
            if (this.IsXmlDataNode)
            {
                return this.xmlNodeReader.GetAttribute(i);
            }
            return null;
        }

        public override string GetAttribute(string name)
        {
            if (this.IsXmlDataNode)
            {
                return this.xmlNodeReader.GetAttribute(name);
            }
            return null;
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            if (this.IsXmlDataNode)
            {
                return this.xmlNodeReader.GetAttribute(name, namespaceURI);
            }
            for (int i = 0; i < this.element.attributeCount; i++)
            {
                AttributeData data = this.element.attributes[i];
                if ((data.localName == name) && (data.ns == namespaceURI))
                {
                    return data.value;
                }
            }
            return null;
        }

        internal IDataNode GetCurrentNode()
        {
            IDataNode dataNode = this.element.dataNode;
            this.Skip();
            return dataNode;
        }

        private ElementData GetNextElement()
        {
            int index = this.depth + 1;
            if (((this.elements != null) && (this.elements.Length > index)) && (this.elements[index] != null))
            {
                return this.elements[index];
            }
            return new ElementData();
        }

        [SecuritySafeCritical]
        internal static string GetPrefix(string ns)
        {
            string str;
            ns = ns ?? string.Empty;
            if (!nsToPrefixTable.TryGetValue(ns, out str))
            {
                lock (nsToPrefixTable)
                {
                    if (!nsToPrefixTable.TryGetValue(ns, out str))
                    {
                        str = ((ns == null) || (ns.Length == 0)) ? string.Empty : ("p" + nsToPrefixTable.Count);
                        AddPrefix(str, ns);
                    }
                }
            }
            return str;
        }

        private void GrowElementsIfNeeded()
        {
            if (this.elements == null)
            {
                this.elements = new ElementData[8];
            }
            else if (this.elements.Length == this.depth)
            {
                ElementData[] destinationArray = new ElementData[this.elements.Length * 2];
                Array.Copy(this.elements, 0, destinationArray, 0, this.elements.Length);
                this.elements = destinationArray;
            }
        }

        private bool IsElementNode(ExtensionDataNodeType nodeType)
        {
            if ((nodeType != ExtensionDataNodeType.Element) && (nodeType != ExtensionDataNodeType.ReferencedElement))
            {
                return (nodeType == ExtensionDataNodeType.NullElement);
            }
            return true;
        }

        [SecuritySafeCritical]
        public override string LookupNamespace(string prefix)
        {
            string str;
            if (this.IsXmlDataNode)
            {
                return this.xmlNodeReader.LookupNamespace(prefix);
            }
            if (!prefixToNsTable.TryGetValue(prefix, out str))
            {
                return null;
            }
            return str;
        }

        private void MoveNext(IDataNode dataNode)
        {
            switch (this.internalNodeType)
            {
                case ExtensionDataNodeType.Text:
                case ExtensionDataNodeType.ReferencedElement:
                case ExtensionDataNodeType.NullElement:
                    this.internalNodeType = ExtensionDataNodeType.EndElement;
                    return;
            }
            Type dataType = dataNode.DataType;
            if (dataType == Globals.TypeOfClassDataNode)
            {
                this.MoveNextInClass((ClassDataNode) dataNode);
            }
            else if (dataType == Globals.TypeOfCollectionDataNode)
            {
                this.MoveNextInCollection((CollectionDataNode) dataNode);
            }
            else if (dataType == Globals.TypeOfISerializableDataNode)
            {
                this.MoveNextInISerializable((ISerializableDataNode) dataNode);
            }
            else if (dataType == Globals.TypeOfXmlDataNode)
            {
                this.MoveNextInXml((XmlDataNode) dataNode);
            }
            else
            {
                if (dataNode.Value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.Runtime.Serialization.SR.GetString("InvalidStateInExtensionDataReader")));
                }
                this.MoveToDeserializedObject(dataNode);
            }
        }

        private void MoveNextInClass(ClassDataNode dataNode)
        {
            if ((dataNode.Members != null) && (this.element.childElementIndex < dataNode.Members.Count))
            {
                if (this.element.childElementIndex == 0)
                {
                    this.context.IncrementItemCount(-dataNode.Members.Count);
                }
                ExtensionDataMember member = dataNode.Members[this.element.childElementIndex++];
                this.SetNextElement(member.Value, member.Name, member.Namespace, GetPrefix(member.Namespace));
            }
            else
            {
                this.internalNodeType = ExtensionDataNodeType.EndElement;
                this.element.childElementIndex = 0;
            }
        }

        private void MoveNextInCollection(CollectionDataNode dataNode)
        {
            if ((dataNode.Items != null) && (this.element.childElementIndex < dataNode.Items.Count))
            {
                if (this.element.childElementIndex == 0)
                {
                    this.context.IncrementItemCount(-dataNode.Items.Count);
                }
                IDataNode node = dataNode.Items[this.element.childElementIndex++];
                this.SetNextElement(node, dataNode.ItemName, dataNode.ItemNamespace, GetPrefix(dataNode.ItemNamespace));
            }
            else
            {
                this.internalNodeType = ExtensionDataNodeType.EndElement;
                this.element.childElementIndex = 0;
            }
        }

        private void MoveNextInISerializable(ISerializableDataNode dataNode)
        {
            if ((dataNode.Members != null) && (this.element.childElementIndex < dataNode.Members.Count))
            {
                if (this.element.childElementIndex == 0)
                {
                    this.context.IncrementItemCount(-dataNode.Members.Count);
                }
                ISerializableDataMember member = dataNode.Members[this.element.childElementIndex++];
                this.SetNextElement(member.Value, member.Name, string.Empty, string.Empty);
            }
            else
            {
                this.internalNodeType = ExtensionDataNodeType.EndElement;
                this.element.childElementIndex = 0;
            }
        }

        private void MoveNextInXml(XmlDataNode dataNode)
        {
            if (this.IsXmlDataNode)
            {
                this.xmlNodeReader.Read();
                if (this.xmlNodeReader.Depth == 0)
                {
                    this.internalNodeType = ExtensionDataNodeType.EndElement;
                    this.xmlNodeReader = null;
                }
            }
            else
            {
                this.internalNodeType = ExtensionDataNodeType.Xml;
                if (this.element == null)
                {
                    this.element = this.nextElement;
                }
                else
                {
                    this.PushElement();
                }
                System.Xml.XmlNode node = XmlObjectSerializerReadContext.CreateWrapperXmlElement(dataNode.OwnerDocument, dataNode.XmlAttributes, dataNode.XmlChildNodes, this.element.prefix, this.element.localName, this.element.ns);
                for (int i = 0; i < this.element.attributeCount; i++)
                {
                    AttributeData data = this.element.attributes[i];
                    System.Xml.XmlAttribute attribute = dataNode.OwnerDocument.CreateAttribute(data.prefix, data.localName, data.ns);
                    attribute.Value = data.value;
                    node.Attributes.Append(attribute);
                }
                this.xmlNodeReader = new XmlNodeReader(node);
                this.xmlNodeReader.Read();
            }
        }

        public override void MoveToAttribute(int index)
        {
            if (this.IsXmlDataNode)
            {
                this.xmlNodeReader.MoveToAttribute(index);
            }
            else
            {
                if ((index < 0) || (index >= this.attributeCount))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("InvalidXmlDeserializingExtensionData")));
                }
                this.nodeType = XmlNodeType.Attribute;
                AttributeData data = this.element.attributes[index];
                this.localName = data.localName;
                this.ns = data.ns;
                this.prefix = data.prefix;
                this.value = data.value;
                this.attributeIndex = index;
            }
        }

        public override bool MoveToAttribute(string name)
        {
            return (this.IsXmlDataNode && this.xmlNodeReader.MoveToAttribute(name));
        }

        public override bool MoveToAttribute(string name, string namespaceURI)
        {
            if (this.IsXmlDataNode)
            {
                return this.xmlNodeReader.MoveToAttribute(name, this.ns);
            }
            for (int i = 0; i < this.element.attributeCount; i++)
            {
                AttributeData data = this.element.attributes[i];
                if ((data.localName == name) && (data.ns == namespaceURI))
                {
                    this.MoveToAttribute(i);
                    return true;
                }
            }
            return false;
        }

        private void MoveToDeserializedObject(IDataNode dataNode)
        {
            Type dataType = dataNode.DataType;
            bool isTypedNode = true;
            if (dataType == Globals.TypeOfObject)
            {
                dataType = dataNode.Value.GetType();
                if (dataType == Globals.TypeOfObject)
                {
                    this.internalNodeType = ExtensionDataNodeType.EndElement;
                    return;
                }
                isTypedNode = false;
            }
            if (!this.MoveToText(dataType, dataNode, isTypedNode))
            {
                if (!dataNode.IsFinalValue)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("InvalidDataNode", new object[] { DataContract.GetClrTypeFullName(dataType) })));
                }
                this.internalNodeType = ExtensionDataNodeType.EndElement;
            }
        }

        public override bool MoveToElement()
        {
            if (this.IsXmlDataNode)
            {
                return this.xmlNodeReader.MoveToElement();
            }
            if (this.nodeType != XmlNodeType.Attribute)
            {
                return false;
            }
            this.SetElement();
            return true;
        }

        public override bool MoveToFirstAttribute()
        {
            if (this.IsXmlDataNode)
            {
                return this.xmlNodeReader.MoveToFirstAttribute();
            }
            if (this.attributeCount == 0)
            {
                return false;
            }
            this.MoveToAttribute(0);
            return true;
        }

        public override bool MoveToNextAttribute()
        {
            if (this.IsXmlDataNode)
            {
                return this.xmlNodeReader.MoveToNextAttribute();
            }
            if ((this.attributeIndex + 1) >= this.attributeCount)
            {
                return false;
            }
            this.MoveToAttribute((int) (this.attributeIndex + 1));
            return true;
        }

        private bool MoveToText(Type type, IDataNode dataNode, bool isTypedNode)
        {
            bool flag = true;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<bool>) dataNode).GetValue() : ((bool) dataNode.Value));
                    break;

                case TypeCode.Char:
                    this.value = XmlConvert.ToString(isTypedNode ? ((int) ((DataNode<char>) dataNode).GetValue()) : ((char) dataNode.Value));
                    break;

                case TypeCode.SByte:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<sbyte>) dataNode).GetValue() : ((sbyte) dataNode.Value));
                    break;

                case TypeCode.Byte:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<byte>) dataNode).GetValue() : ((byte) dataNode.Value));
                    break;

                case TypeCode.Int16:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<short>) dataNode).GetValue() : ((short) dataNode.Value));
                    break;

                case TypeCode.UInt16:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<ushort>) dataNode).GetValue() : ((ushort) dataNode.Value));
                    break;

                case TypeCode.Int32:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<int>) dataNode).GetValue() : ((int) dataNode.Value));
                    break;

                case TypeCode.UInt32:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<uint>) dataNode).GetValue() : ((uint) dataNode.Value));
                    break;

                case TypeCode.Int64:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<long>) dataNode).GetValue() : ((long) dataNode.Value));
                    break;

                case TypeCode.UInt64:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<ulong>) dataNode).GetValue() : ((ulong) dataNode.Value));
                    break;

                case TypeCode.Single:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<float>) dataNode).GetValue() : ((float) dataNode.Value));
                    break;

                case TypeCode.Double:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<double>) dataNode).GetValue() : ((double) dataNode.Value));
                    break;

                case TypeCode.Decimal:
                    this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<decimal>) dataNode).GetValue() : ((decimal) dataNode.Value));
                    break;

                case TypeCode.DateTime:
                    this.value = (isTypedNode ? ((DataNode<DateTime>) dataNode).GetValue() : ((DateTime) dataNode.Value)).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK", DateTimeFormatInfo.InvariantInfo);
                    break;

                case TypeCode.String:
                    this.value = isTypedNode ? ((DataNode<string>) dataNode).GetValue() : ((string) dataNode.Value);
                    break;

                default:
                    if (type == Globals.TypeOfByteArray)
                    {
                        byte[] inArray = isTypedNode ? ((DataNode<byte[]>) dataNode).GetValue() : ((byte[]) dataNode.Value);
                        this.value = (inArray == null) ? string.Empty : Convert.ToBase64String(inArray);
                    }
                    else if (type == Globals.TypeOfTimeSpan)
                    {
                        this.value = XmlConvert.ToString(isTypedNode ? ((DataNode<TimeSpan>) dataNode).GetValue() : ((TimeSpan) dataNode.Value));
                    }
                    else if (type == Globals.TypeOfGuid)
                    {
                        this.value = (isTypedNode ? ((DataNode<Guid>) dataNode).GetValue() : ((Guid) dataNode.Value)).ToString();
                    }
                    else if (type == Globals.TypeOfUri)
                    {
                        this.value = (isTypedNode ? ((DataNode<Uri>) dataNode).GetValue() : ((Uri) dataNode.Value)).GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
                    }
                    else
                    {
                        flag = false;
                    }
                    break;
            }
            if (flag)
            {
                this.internalNodeType = ExtensionDataNodeType.Text;
            }
            return flag;
        }

        private void PopElement()
        {
            this.prefix = this.element.prefix;
            this.localName = this.element.localName;
            this.ns = this.element.ns;
            if (this.depth != 0)
            {
                this.depth--;
                if (this.elements != null)
                {
                    this.element = this.elements[this.depth];
                }
            }
        }

        private void PushElement()
        {
            this.GrowElementsIfNeeded();
            this.elements[this.depth++] = this.element;
            if (this.nextElement == null)
            {
                this.element = this.GetNextElement();
            }
            else
            {
                this.element = this.nextElement;
                this.nextElement = null;
            }
        }

        public override bool Read()
        {
            if ((this.nodeType != XmlNodeType.Attribute) || !this.MoveToNextAttribute())
            {
                this.MoveNext(this.element.dataNode);
                switch (this.internalNodeType)
                {
                    case ExtensionDataNodeType.None:
                        if (this.depth != 0)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("InvalidXmlDeserializingExtensionData")));
                        }
                        this.nodeType = XmlNodeType.None;
                        this.prefix = string.Empty;
                        this.ns = string.Empty;
                        this.localName = string.Empty;
                        this.value = string.Empty;
                        this.attributeCount = 0;
                        this.readState = System.Xml.ReadState.EndOfFile;
                        return false;

                    case ExtensionDataNodeType.Element:
                    case ExtensionDataNodeType.ReferencedElement:
                    case ExtensionDataNodeType.NullElement:
                        this.PushElement();
                        this.SetElement();
                        break;

                    case ExtensionDataNodeType.EndElement:
                        this.nodeType = XmlNodeType.EndElement;
                        this.prefix = string.Empty;
                        this.ns = string.Empty;
                        this.localName = string.Empty;
                        this.value = string.Empty;
                        this.attributeCount = 0;
                        this.attributeIndex = -1;
                        this.PopElement();
                        break;

                    case ExtensionDataNodeType.Text:
                        this.nodeType = XmlNodeType.Text;
                        this.prefix = string.Empty;
                        this.ns = string.Empty;
                        this.localName = string.Empty;
                        this.attributeCount = 0;
                        this.attributeIndex = -1;
                        break;

                    case ExtensionDataNodeType.Xml:
                        break;

                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.Runtime.Serialization.SR.GetString("InvalidStateInExtensionDataReader")));
                }
                this.readState = System.Xml.ReadState.Interactive;
            }
            return true;
        }

        public override bool ReadAttributeValue()
        {
            return (this.IsXmlDataNode && this.xmlNodeReader.ReadAttributeValue());
        }

        internal void Reset()
        {
            this.localName = null;
            this.ns = null;
            this.prefix = null;
            this.value = null;
            this.attributeCount = 0;
            this.attributeIndex = -1;
            this.depth = 0;
            this.element = null;
            this.nextElement = null;
            this.elements = null;
            this.deserializedDataNodes = null;
        }

        public override void ResolveEntity()
        {
            if (this.IsXmlDataNode)
            {
                this.xmlNodeReader.ResolveEntity();
            }
        }

        internal void SetDataNode(IDataNode dataNode, string name, string ns)
        {
            this.SetNextElement(dataNode, name, ns, null);
            this.element = this.nextElement;
            this.nextElement = null;
            this.SetElement();
        }

        internal void SetDeserializedValue(object obj)
        {
            IDataNode node = ((this.deserializedDataNodes == null) || (this.deserializedDataNodes.Count == 0)) ? null : this.deserializedDataNodes.Dequeue();
            if ((node != null) && !(obj is IDataNode))
            {
                node.Value = obj;
                node.IsFinalValue = true;
            }
        }

        private void SetElement()
        {
            this.nodeType = XmlNodeType.Element;
            this.localName = this.element.localName;
            this.ns = this.element.ns;
            this.prefix = this.element.prefix;
            this.value = string.Empty;
            this.attributeCount = this.element.attributeCount;
            this.attributeIndex = -1;
        }

        private void SetNextElement(IDataNode node, string name, string ns, string prefix)
        {
            this.internalNodeType = ExtensionDataNodeType.Element;
            this.nextElement = this.GetNextElement();
            this.nextElement.localName = name;
            this.nextElement.ns = ns;
            this.nextElement.prefix = prefix;
            if (node == null)
            {
                this.nextElement.attributeCount = 0;
                this.nextElement.AddAttribute("i", "http://www.w3.org/2001/XMLSchema-instance", "nil", "true");
                this.internalNodeType = ExtensionDataNodeType.NullElement;
            }
            else if (!this.CheckIfNodeHandled(node))
            {
                this.AddDeserializedDataNode(node);
                node.GetData(this.nextElement);
                if (node is XmlDataNode)
                {
                    this.MoveNextInXml((XmlDataNode) node);
                }
            }
        }

        public override void Skip()
        {
            if (this.IsXmlDataNode)
            {
                this.xmlNodeReader.Skip();
            }
            else if (this.ReadState == System.Xml.ReadState.Interactive)
            {
                this.MoveToElement();
                if (this.IsElementNode(this.internalNodeType))
                {
                    int num = 1;
                    while (num != 0)
                    {
                        if (!this.Read())
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("InvalidXmlDeserializingExtensionData")));
                        }
                        if (this.IsElementNode(this.internalNodeType))
                        {
                            num++;
                        }
                        else if (this.internalNodeType == ExtensionDataNodeType.EndElement)
                        {
                            this.ReadEndElement();
                            num--;
                        }
                    }
                }
                else
                {
                    this.Read();
                }
            }
        }

        public override int AttributeCount
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return this.attributeCount;
                }
                return this.xmlNodeReader.AttributeCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                if (this.IsXmlDataNode)
                {
                    return this.xmlNodeReader.BaseURI;
                }
                return string.Empty;
            }
        }

        public override int Depth
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return this.depth;
                }
                return this.xmlNodeReader.Depth;
            }
        }

        public override bool EOF
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return (this.readState == System.Xml.ReadState.EndOfFile);
                }
                return this.xmlNodeReader.EOF;
            }
        }

        public override bool HasValue
        {
            get
            {
                return (this.IsXmlDataNode && this.xmlNodeReader.HasValue);
            }
        }

        public override bool IsDefault
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return base.IsDefault;
                }
                return this.xmlNodeReader.IsDefault;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return false;
                }
                return this.xmlNodeReader.IsEmptyElement;
            }
        }

        private bool IsXmlDataNode
        {
            get
            {
                return (this.internalNodeType == ExtensionDataNodeType.Xml);
            }
        }

        public override string this[int i]
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return this.GetAttribute(i);
                }
                return this.xmlNodeReader[i];
            }
        }

        public override string this[string name]
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return this.GetAttribute(name);
                }
                return this.xmlNodeReader[name];
            }
        }

        public override string this[string name, string namespaceURI]
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return this.GetAttribute(name, namespaceURI);
                }
                return this.xmlNodeReader[name, namespaceURI];
            }
        }

        public override string LocalName
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return this.localName;
                }
                return this.xmlNodeReader.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                if (this.IsXmlDataNode)
                {
                    return this.xmlNodeReader.Name;
                }
                return string.Empty;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return this.ns;
                }
                return this.xmlNodeReader.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                if (this.IsXmlDataNode)
                {
                    return this.xmlNodeReader.NameTable;
                }
                return null;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return this.nodeType;
                }
                return this.xmlNodeReader.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return this.prefix;
                }
                return this.xmlNodeReader.Prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return base.QuoteChar;
                }
                return this.xmlNodeReader.QuoteChar;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return this.readState;
                }
                return this.xmlNodeReader.ReadState;
            }
        }

        public override string Value
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return this.value;
                }
                return this.xmlNodeReader.Value;
            }
        }

        public override string XmlLang
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return base.XmlLang;
                }
                return this.xmlNodeReader.XmlLang;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                if (!this.IsXmlDataNode)
                {
                    return base.XmlSpace;
                }
                return this.xmlNodeReader.XmlSpace;
            }
        }

        private enum ExtensionDataNodeType
        {
            None,
            Element,
            EndElement,
            Text,
            Xml,
            ReferencedElement,
            NullElement
        }
    }
}

