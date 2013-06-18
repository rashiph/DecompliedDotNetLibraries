namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Diagnostics;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    internal class XmlObjectSerializerReadContext : XmlObjectSerializerContext
    {
        internal Attributes attributes;
        private Attributes attributesInXmlData;
        private HybridObjectCache deserializedObjects;
        private XmlReaderDelegator extensionDataReader;
        private object getOnlyCollectionValue;
        private bool isGetOnlyCollection;
        private XmlDocument xmlDocument;
        private XmlSerializableReader xmlSerializableReader;

        protected XmlObjectSerializerReadContext(NetDataContractSerializer serializer) : base(serializer)
        {
            this.attributes = new Attributes();
        }

        internal XmlObjectSerializerReadContext(DataContractSerializer serializer, DataContract rootTypeDataContract, DataContractResolver dataContractResolver) : base(serializer, rootTypeDataContract, dataContractResolver)
        {
            this.attributes = new Attributes();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal XmlObjectSerializerReadContext(XmlObjectSerializer serializer, int maxItemsInObjectGraph, StreamingContext streamingContext, bool ignoreExtensionDataObject) : base(serializer, maxItemsInObjectGraph, streamingContext, ignoreExtensionDataObject)
        {
        }

        private System.Xml.XmlAttribute AddNamespaceDeclaration(string prefix, string ns)
        {
            System.Xml.XmlAttribute attribute = ((prefix == null) || (prefix.Length == 0)) ? this.Document.CreateAttribute(null, "xmlns", "http://www.w3.org/2000/xmlns/") : this.Document.CreateAttribute("xmlns", prefix, "http://www.w3.org/2000/xmlns/");
            attribute.Value = ns;
            return attribute;
        }

        public void AddNewObject(object obj)
        {
            this.AddNewObjectWithId(this.attributes.Id, obj);
        }

        public void AddNewObjectWithId(string id, object obj)
        {
            if (id != Globals.NewObjectId)
            {
                this.DeserializedObjects.Add(id, obj);
            }
            if (this.extensionDataReader != null)
            {
                this.extensionDataReader.UnderlyingExtensionDataReader.SetDeserializedValue(obj);
            }
        }

        public void CheckEndOfArray(XmlReaderDelegator xmlReader, int arraySize, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (xmlReader.NodeType != XmlNodeType.EndElement)
            {
                while (xmlReader.IsStartElement())
                {
                    if (xmlReader.IsStartElement(itemName, itemNamespace))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ArrayExceededSizeAttribute", new object[] { arraySize, itemName.Value, itemNamespace.Value })));
                    }
                    this.SkipUnknownElement(xmlReader);
                }
                if (xmlReader.NodeType != XmlNodeType.EndElement)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.EndElement, xmlReader));
                }
            }
        }

        internal static XmlObjectSerializerReadContext CreateContext(NetDataContractSerializer serializer)
        {
            return new XmlObjectSerializerReadContextComplex(serializer);
        }

        internal static XmlObjectSerializerReadContext CreateContext(DataContractSerializer serializer, DataContract rootTypeDataContract, DataContractResolver dataContractResolver)
        {
            if (!serializer.PreserveObjectReferences && (serializer.DataContractSurrogate == null))
            {
                return new XmlObjectSerializerReadContext(serializer, rootTypeDataContract, dataContractResolver);
            }
            return new XmlObjectSerializerReadContextComplex(serializer, rootTypeDataContract, dataContractResolver);
        }

        protected virtual XmlReaderDelegator CreateReaderDelegatorForReader(XmlReader xmlReader)
        {
            return new XmlReaderDelegator(xmlReader);
        }

        internal XmlReaderDelegator CreateReaderOverChildNodes(IList<System.Xml.XmlAttribute> xmlAttributes, IList<System.Xml.XmlNode> xmlChildNodes)
        {
            System.Xml.XmlNode node = CreateWrapperXmlElement(this.Document, xmlAttributes, xmlChildNodes, null, null, null);
            XmlReaderDelegator xmlReader = this.CreateReaderDelegatorForReader(new XmlNodeReader(node));
            xmlReader.MoveToContent();
            Read(xmlReader);
            return xmlReader;
        }

        public static Exception CreateUnexpectedStateException(XmlNodeType expectedState, XmlReaderDelegator xmlReader)
        {
            return XmlObjectSerializer.CreateSerializationExceptionWithReaderDetails(System.Runtime.Serialization.SR.GetString("ExpectingState", new object[] { expectedState }), xmlReader);
        }

        internal static System.Xml.XmlNode CreateWrapperXmlElement(XmlDocument document, IList<System.Xml.XmlAttribute> xmlAttributes, IList<System.Xml.XmlNode> xmlChildNodes, string prefix, string localName, string ns)
        {
            localName = localName ?? "wrapper";
            ns = ns ?? string.Empty;
            System.Xml.XmlNode node = document.CreateElement(prefix, localName, ns);
            if (xmlAttributes != null)
            {
                for (int i = 0; i < xmlAttributes.Count; i++)
                {
                    node.Attributes.Append(xmlAttributes[i]);
                }
            }
            if (xmlChildNodes != null)
            {
                for (int j = 0; j < xmlChildNodes.Count; j++)
                {
                    node.AppendChild(xmlChildNodes[j]);
                }
            }
            return node;
        }

        private object DeserializeFromExtensionData(IDataNode dataNode, Type type, string name, string ns)
        {
            ExtensionDataReader underlyingExtensionDataReader;
            if (this.extensionDataReader == null)
            {
                underlyingExtensionDataReader = new ExtensionDataReader(this);
                this.extensionDataReader = this.CreateReaderDelegatorForReader(underlyingExtensionDataReader);
            }
            else
            {
                underlyingExtensionDataReader = this.extensionDataReader.UnderlyingExtensionDataReader;
            }
            underlyingExtensionDataReader.SetDataNode(dataNode, name, ns);
            object obj2 = this.InternalDeserialize(this.extensionDataReader, type, name, ns);
            dataNode.Clear();
            underlyingExtensionDataReader.Reset();
            return obj2;
        }

        public static T[] EnsureArraySize<T>(T[] array, int index)
        {
            if (array.Length <= index)
            {
                if (index == 0x7fffffff)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("MaxArrayLengthExceeded", new object[] { 0x7fffffff, DataContract.GetClrTypeFullName(typeof(T)) })));
                }
                int num = (index < 0x3fffffff) ? (index * 2) : 0x7fffffff;
                T[] destinationArray = new T[num];
                Array.Copy(array, 0, destinationArray, 0, array.Length);
                array = destinationArray;
            }
            return array;
        }

        internal virtual int GetArraySize()
        {
            return -1;
        }

        internal object GetCollectionMember()
        {
            return this.getOnlyCollectionValue;
        }

        public object GetExistingObject(string id, Type type, string name, string ns)
        {
            object obj2 = this.DeserializedObjects.GetObject(id);
            if (obj2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("DeserializedObjectWithIdNotFound", new object[] { id })));
            }
            if (!(obj2 is IDataNode))
            {
                return obj2;
            }
            IDataNode dataNode = (IDataNode) obj2;
            return (((dataNode.Value != null) && dataNode.IsFinalValue) ? dataNode.Value : this.DeserializeFromExtensionData(dataNode, type, name, ns));
        }

        private object GetExistingObjectOrExtensionData(string id)
        {
            object obj2 = this.DeserializedObjects.GetObject(id);
            if (obj2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("DeserializedObjectWithIdNotFound", new object[] { id })));
            }
            return obj2;
        }

        public int GetMemberIndex(XmlReaderDelegator xmlReader, XmlDictionaryString[] memberNames, XmlDictionaryString[] memberNamespaces, int memberIndex, ExtensionDataObject extensionData)
        {
            for (int i = memberIndex + 1; i < memberNames.Length; i++)
            {
                if (xmlReader.IsStartElement(memberNames[i], memberNamespaces[i]))
                {
                    return i;
                }
            }
            this.HandleMemberNotFound(xmlReader, extensionData, memberIndex);
            return memberNames.Length;
        }

        public int GetMemberIndexWithRequiredMembers(XmlReaderDelegator xmlReader, XmlDictionaryString[] memberNames, XmlDictionaryString[] memberNamespaces, int memberIndex, int requiredIndex, ExtensionDataObject extensionData)
        {
            for (int i = memberIndex + 1; i < memberNames.Length; i++)
            {
                if (xmlReader.IsStartElement(memberNames[i], memberNamespaces[i]))
                {
                    if (requiredIndex < i)
                    {
                        ThrowRequiredMemberMissingException(xmlReader, memberIndex, requiredIndex, memberNames);
                    }
                    return i;
                }
            }
            this.HandleMemberNotFound(xmlReader, extensionData, memberIndex);
            return memberNames.Length;
        }

        public string GetObjectId()
        {
            return this.attributes.Id;
        }

        public object GetRealObject(IObjectReference obj, string id)
        {
            object realObject = SurrogateDataContract.GetRealObject(obj, base.GetStreamingContext());
            if (realObject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("GetRealObjectReturnedNull", new object[] { DataContract.GetClrTypeFullName(obj.GetType()) })));
            }
            this.ReplaceDeserializedObject(id, obj, realObject);
            return realObject;
        }

        protected void HandleMemberNotFound(XmlReaderDelegator xmlReader, ExtensionDataObject extensionData, int memberIndex)
        {
            xmlReader.MoveToContent();
            if (xmlReader.NodeType != XmlNodeType.Element)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.Element, xmlReader));
            }
            if (base.IgnoreExtensionDataObject || (extensionData == null))
            {
                this.SkipUnknownElement(xmlReader);
            }
            else
            {
                this.HandleUnknownElement(xmlReader, extensionData, memberIndex);
            }
        }

        internal void HandleUnknownElement(XmlReaderDelegator xmlReader, ExtensionDataObject extensionData, int memberIndex)
        {
            if (extensionData.Members == null)
            {
                extensionData.Members = new List<ExtensionDataMember>();
            }
            extensionData.Members.Add(this.ReadExtensionDataMember(xmlReader, memberIndex));
        }

        protected void InitializeExtensionDataNode(IDataNode dataNode, string dataContractName, string dataContractNamespace)
        {
            dataNode.DataContractName = dataContractName;
            dataNode.DataContractNamespace = dataContractNamespace;
            dataNode.ClrAssemblyName = this.attributes.ClrAssembly;
            dataNode.ClrTypeName = this.attributes.ClrType;
            this.AddNewObject(dataNode);
            dataNode.Id = this.attributes.Id;
        }

        internal virtual object InternalDeserialize(XmlReaderDelegator xmlReader, Type declaredType, string name, string ns)
        {
            DataContract dataContract = base.GetDataContract(declaredType);
            return this.InternalDeserialize(xmlReader, name, ns, declaredType, ref dataContract);
        }

        public virtual object InternalDeserialize(XmlReaderDelegator xmlReader, int id, RuntimeTypeHandle declaredTypeHandle, string name, string ns)
        {
            DataContract dataContract = this.GetDataContract(id, declaredTypeHandle);
            return this.InternalDeserialize(xmlReader, name, ns, Type.GetTypeFromHandle(declaredTypeHandle), ref dataContract);
        }

        protected object InternalDeserialize(XmlReaderDelegator reader, string name, string ns, Type declaredType, ref DataContract dataContract)
        {
            object retObj = null;
            if (this.TryHandleNullOrRef(reader, dataContract.UnderlyingType, name, ns, ref retObj))
            {
                return retObj;
            }
            bool knownTypesAddedInCurrentScope = false;
            if (dataContract.KnownDataContracts != null)
            {
                this.scopedKnownTypes.Push(dataContract.KnownDataContracts);
                knownTypesAddedInCurrentScope = true;
            }
            if (this.attributes.XsiTypeName != null)
            {
                dataContract = base.ResolveDataContractFromKnownTypes(this.attributes.XsiTypeName, this.attributes.XsiTypeNamespace, dataContract, declaredType);
                if (dataContract == null)
                {
                    if (base.DataContractResolver == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(XmlObjectSerializer.TryAddLineInfo(reader, System.Runtime.Serialization.SR.GetString("DcTypeNotFoundOnDeserialize", new object[] { this.attributes.XsiTypeNamespace, this.attributes.XsiTypeName, reader.NamespaceURI, reader.LocalName }))));
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(XmlObjectSerializer.TryAddLineInfo(reader, System.Runtime.Serialization.SR.GetString("DcTypeNotResolvedOnDeserialize", new object[] { this.attributes.XsiTypeNamespace, this.attributes.XsiTypeName, reader.NamespaceURI, reader.LocalName }))));
                }
                knownTypesAddedInCurrentScope = this.ReplaceScopedKnownTypesTop(dataContract.KnownDataContracts, knownTypesAddedInCurrentScope);
            }
            if (dataContract.IsISerializable && (this.attributes.FactoryTypeName != null))
            {
                DataContract contract = base.ResolveDataContractFromKnownTypes(this.attributes.FactoryTypeName, this.attributes.FactoryTypeNamespace, dataContract, declaredType);
                if (contract != null)
                {
                    if (!contract.IsISerializable)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("FactoryTypeNotISerializable", new object[] { DataContract.GetClrTypeFullName(contract.UnderlyingType), DataContract.GetClrTypeFullName(dataContract.UnderlyingType) })));
                    }
                    dataContract = contract;
                    knownTypesAddedInCurrentScope = this.ReplaceScopedKnownTypesTop(dataContract.KnownDataContracts, knownTypesAddedInCurrentScope);
                }
                else if (DiagnosticUtility.ShouldTraceWarning)
                {
                    Dictionary<string, string> dictionary = new Dictionary<string, string>(2);
                    dictionary["FactoryType"] = this.attributes.FactoryTypeNamespace + ":" + this.attributes.FactoryTypeName;
                    dictionary["ISerializableType"] = dataContract.StableName.Namespace + ":" + dataContract.StableName.Name;
                    TraceUtility.Trace(TraceEventType.Warning, 0x30011, System.Runtime.Serialization.SR.GetString("TraceCodeFactoryTypeNotFound"), new DictionaryTraceRecord(dictionary));
                }
            }
            if (knownTypesAddedInCurrentScope)
            {
                object obj3 = this.ReadDataContractValue(dataContract, reader);
                this.scopedKnownTypes.Pop();
                return obj3;
            }
            return this.ReadDataContractValue(dataContract, reader);
        }

        internal virtual object InternalDeserialize(XmlReaderDelegator xmlReader, Type declaredType, DataContract dataContract, string name, string ns)
        {
            if (dataContract == null)
            {
                base.GetDataContract(declaredType);
            }
            return this.InternalDeserialize(xmlReader, name, ns, declaredType, ref dataContract);
        }

        private bool IsContentNode(XmlNodeType nodeType)
        {
            switch (nodeType)
            {
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                case XmlNodeType.DocumentType:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return false;
            }
            return true;
        }

        protected virtual bool IsReadingClassExtensionData(XmlReaderDelegator xmlReader)
        {
            return false;
        }

        protected virtual bool IsReadingCollectionExtensionData(XmlReaderDelegator xmlReader)
        {
            return (this.attributes.ArraySZSize != -1);
        }

        public static bool MoveToNextElement(XmlReaderDelegator xmlReader)
        {
            return (xmlReader.MoveToContent() != XmlNodeType.EndElement);
        }

        internal static void ParseQualifiedName(string qname, XmlReaderDelegator xmlReader, out string name, out string ns, out string prefix)
        {
            int index = qname.IndexOf(':');
            prefix = "";
            if (index >= 0)
            {
                prefix = qname.Substring(0, index);
            }
            name = qname.Substring(index + 1);
            ns = xmlReader.LookupNamespace(prefix);
        }

        public static void Read(XmlReaderDelegator xmlReader)
        {
            if (!xmlReader.Read())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("UnexpectedEndOfFile")));
            }
        }

        private IDataNode ReadAndResolveUnknownXmlData(XmlReaderDelegator xmlReader, IDictionary<string, string> namespaces, string dataContractName, string dataContractNamespace)
        {
            XmlNodeType type;
            bool flag = true;
            bool flag2 = true;
            bool flag3 = true;
            string strA = null;
            string str2 = null;
            IList<System.Xml.XmlNode> xmlChildNodes = new List<System.Xml.XmlNode>();
            IList<System.Xml.XmlAttribute> xmlAttributes = null;
            if (namespaces != null)
            {
                xmlAttributes = new List<System.Xml.XmlAttribute>();
                foreach (KeyValuePair<string, string> pair in namespaces)
                {
                    xmlAttributes.Add(this.AddNamespaceDeclaration(pair.Key, pair.Value));
                }
            }
            while ((type = xmlReader.NodeType) != XmlNodeType.EndElement)
            {
                if (type == XmlNodeType.Element)
                {
                    string namespaceURI = xmlReader.NamespaceURI;
                    string localName = xmlReader.LocalName;
                    if (flag)
                    {
                        flag = namespaceURI.Length == 0;
                    }
                    if (flag2)
                    {
                        if (str2 == null)
                        {
                            str2 = localName;
                            strA = namespaceURI;
                        }
                        else
                        {
                            flag2 = (string.CompareOrdinal(str2, localName) == 0) && (string.CompareOrdinal(strA, namespaceURI) == 0);
                        }
                    }
                }
                else
                {
                    if (xmlReader.EOF)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("UnexpectedEndOfFile")));
                    }
                    if (this.IsContentNode(xmlReader.NodeType))
                    {
                        flag3 = flag = flag2 = false;
                    }
                }
                if (this.attributesInXmlData == null)
                {
                    this.attributesInXmlData = new Attributes();
                }
                this.attributesInXmlData.Read(xmlReader);
                System.Xml.XmlNode item = this.Document.ReadNode(xmlReader.UnderlyingReader);
                xmlChildNodes.Add(item);
                if (namespaces == null)
                {
                    if (this.attributesInXmlData.XsiTypeName != null)
                    {
                        item.Attributes.Append(this.AddNamespaceDeclaration(this.attributesInXmlData.XsiTypePrefix, this.attributesInXmlData.XsiTypeNamespace));
                    }
                    if (this.attributesInXmlData.FactoryTypeName != null)
                    {
                        item.Attributes.Append(this.AddNamespaceDeclaration(this.attributesInXmlData.FactoryTypePrefix, this.attributesInXmlData.FactoryTypeNamespace));
                    }
                }
            }
            xmlReader.ReadEndElement();
            if ((str2 != null) && flag2)
            {
                return this.ReadUnknownCollectionData(this.CreateReaderOverChildNodes(xmlAttributes, xmlChildNodes), dataContractName, dataContractNamespace);
            }
            if (flag)
            {
                return this.ReadUnknownISerializableData(this.CreateReaderOverChildNodes(xmlAttributes, xmlChildNodes), dataContractName, dataContractNamespace);
            }
            if (flag3)
            {
                return this.ReadUnknownClassData(this.CreateReaderOverChildNodes(xmlAttributes, xmlChildNodes), dataContractName, dataContractNamespace);
            }
            XmlDataNode dataNode = new XmlDataNode();
            this.InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);
            dataNode.OwnerDocument = this.Document;
            dataNode.XmlChildNodes = xmlChildNodes;
            dataNode.XmlAttributes = xmlAttributes;
            return dataNode;
        }

        internal virtual void ReadAttributes(XmlReaderDelegator xmlReader)
        {
            if (this.attributes == null)
            {
                this.attributes = new Attributes();
            }
            this.attributes.Read(xmlReader);
        }

        protected virtual object ReadDataContractValue(DataContract dataContract, XmlReaderDelegator reader)
        {
            return dataContract.ReadXmlValue(reader, this);
        }

        private ExtensionDataMember ReadExtensionDataMember(XmlReaderDelegator xmlReader, int memberIndex)
        {
            ExtensionDataMember member = new ExtensionDataMember {
                Name = xmlReader.LocalName,
                Namespace = xmlReader.NamespaceURI,
                MemberIndex = memberIndex
            };
            if (xmlReader.UnderlyingExtensionDataReader != null)
            {
                member.Value = xmlReader.UnderlyingExtensionDataReader.GetCurrentNode();
                return member;
            }
            member.Value = this.ReadExtensionDataValue(xmlReader);
            return member;
        }

        public IDataNode ReadExtensionDataValue(XmlReaderDelegator xmlReader)
        {
            this.ReadAttributes(xmlReader);
            base.IncrementItemCount(1);
            IDataNode dataNode = null;
            if (this.attributes.Ref != Globals.NewObjectId)
            {
                xmlReader.Skip();
                object existingObjectOrExtensionData = this.GetExistingObjectOrExtensionData(this.attributes.Ref);
                dataNode = (existingObjectOrExtensionData is IDataNode) ? ((IDataNode) existingObjectOrExtensionData) : new DataNode<object>(existingObjectOrExtensionData);
                dataNode.Id = this.attributes.Ref;
                return dataNode;
            }
            if (this.attributes.XsiNil)
            {
                xmlReader.Skip();
                return null;
            }
            string dataContractName = null;
            string dataContractNamespace = null;
            if (this.attributes.XsiTypeName != null)
            {
                dataContractName = this.attributes.XsiTypeName;
                dataContractNamespace = this.attributes.XsiTypeNamespace;
            }
            if (this.IsReadingCollectionExtensionData(xmlReader))
            {
                Read(xmlReader);
                return this.ReadUnknownCollectionData(xmlReader, dataContractName, dataContractNamespace);
            }
            if (this.attributes.FactoryTypeName != null)
            {
                Read(xmlReader);
                return this.ReadUnknownISerializableData(xmlReader, dataContractName, dataContractNamespace);
            }
            if (this.IsReadingClassExtensionData(xmlReader))
            {
                Read(xmlReader);
                return this.ReadUnknownClassData(xmlReader, dataContractName, dataContractNamespace);
            }
            DataContract contract = this.ResolveDataContractFromTypeName();
            if (contract == null)
            {
                return this.ReadExtensionDataValue(xmlReader, dataContractName, dataContractNamespace);
            }
            if (contract is XmlDataContract)
            {
                return this.ReadUnknownXmlData(xmlReader, dataContractName, dataContractNamespace);
            }
            if (contract.IsISerializable)
            {
                Read(xmlReader);
                return this.ReadUnknownISerializableData(xmlReader, dataContractName, dataContractNamespace);
            }
            if (contract is PrimitiveDataContract)
            {
                if (this.attributes.Id == Globals.NewObjectId)
                {
                    Read(xmlReader);
                    xmlReader.MoveToContent();
                    dataNode = this.ReadUnknownPrimitiveData(xmlReader, contract.UnderlyingType, dataContractName, dataContractNamespace);
                    xmlReader.ReadEndElement();
                    return dataNode;
                }
                dataNode = new DataNode<object>(xmlReader.ReadElementContentAsAnyType(contract.UnderlyingType));
                this.InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);
                return dataNode;
            }
            if (contract is EnumDataContract)
            {
                dataNode = new DataNode<object>(((EnumDataContract) contract).ReadEnumValue(xmlReader));
                this.InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);
                return dataNode;
            }
            if (contract is ClassDataContract)
            {
                Read(xmlReader);
                return this.ReadUnknownClassData(xmlReader, dataContractName, dataContractNamespace);
            }
            if (contract is CollectionDataContract)
            {
                Read(xmlReader);
                dataNode = this.ReadUnknownCollectionData(xmlReader, dataContractName, dataContractNamespace);
            }
            return dataNode;
        }

        private IDataNode ReadExtensionDataValue(XmlReaderDelegator xmlReader, string dataContractName, string dataContractNamespace)
        {
            this.StartReadExtensionDataValue(xmlReader);
            if (this.attributes.UnrecognizedAttributesFound)
            {
                return this.ReadUnknownXmlData(xmlReader, dataContractName, dataContractNamespace);
            }
            IDictionary<string, string> namespacesInScope = xmlReader.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);
            Read(xmlReader);
            xmlReader.MoveToContent();
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    if (!xmlReader.NamespaceURI.StartsWith("http://schemas.datacontract.org/2004/07/", StringComparison.Ordinal))
                    {
                        return this.ReadAndResolveUnknownXmlData(xmlReader, namespacesInScope, dataContractName, dataContractNamespace);
                    }
                    return this.ReadUnknownClassData(xmlReader, dataContractName, dataContractNamespace);

                case XmlNodeType.Text:
                    return this.ReadPrimitiveExtensionDataValue(xmlReader, dataContractName, dataContractNamespace);

                case XmlNodeType.EndElement:
                {
                    IDataNode node = this.ReadUnknownPrimitiveData(xmlReader, Globals.TypeOfObject, dataContractName, dataContractNamespace);
                    xmlReader.ReadEndElement();
                    node.IsFinalValue = false;
                    return node;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.Element, xmlReader));
        }

        public string ReadIfNullOrRef(XmlReaderDelegator xmlReader, Type memberType, bool isMemberTypeSerializable)
        {
            if (this.attributes.Ref != Globals.NewObjectId)
            {
                this.CheckIfTypeSerializable(memberType, isMemberTypeSerializable);
                xmlReader.Skip();
                return this.attributes.Ref;
            }
            if (this.attributes.XsiNil)
            {
                this.CheckIfTypeSerializable(memberType, isMemberTypeSerializable);
                xmlReader.Skip();
                return null;
            }
            return Globals.NewObjectId;
        }

        internal object ReadIXmlSerializable(XmlReaderDelegator xmlReader, XmlDataContract xmlDataContract, bool isMemberType)
        {
            if (this.xmlSerializableReader == null)
            {
                this.xmlSerializableReader = new XmlSerializableReader();
            }
            return ReadIXmlSerializable(this.xmlSerializableReader, xmlReader, xmlDataContract, isMemberType);
        }

        internal static object ReadIXmlSerializable(XmlSerializableReader xmlSerializableReader, XmlReaderDelegator xmlReader, XmlDataContract xmlDataContract, bool isMemberType)
        {
            object obj2 = null;
            xmlSerializableReader.BeginRead(xmlReader);
            if (isMemberType && !xmlDataContract.HasRoot)
            {
                xmlReader.Read();
                xmlReader.MoveToContent();
            }
            if (xmlDataContract.UnderlyingType == Globals.TypeOfXmlElement)
            {
                if (!xmlReader.IsStartElement())
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.Element, xmlReader));
                }
                XmlDocument document = new XmlDocument();
                obj2 = (XmlElement) document.ReadNode(xmlSerializableReader);
            }
            else if (xmlDataContract.UnderlyingType == Globals.TypeOfXmlNodeArray)
            {
                obj2 = XmlSerializableServices.ReadNodes(xmlSerializableReader);
            }
            else
            {
                IXmlSerializable serializable = xmlDataContract.CreateXmlSerializableDelegate();
                serializable.ReadXml(xmlSerializableReader);
                obj2 = serializable;
            }
            xmlSerializableReader.EndRead();
            return obj2;
        }

        protected virtual IDataNode ReadPrimitiveExtensionDataValue(XmlReaderDelegator xmlReader, string dataContractName, string dataContractNamespace)
        {
            Type valueType = xmlReader.ValueType;
            if (valueType == Globals.TypeOfString)
            {
                IDataNode dataNode = new DataNode<object>(xmlReader.ReadContentAsString());
                this.InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);
                dataNode.IsFinalValue = false;
                xmlReader.ReadEndElement();
                return dataNode;
            }
            IDataNode node2 = this.ReadUnknownPrimitiveData(xmlReader, valueType, dataContractName, dataContractNamespace);
            xmlReader.ReadEndElement();
            return node2;
        }

        internal static object ReadRootIXmlSerializable(XmlReaderDelegator xmlReader, XmlDataContract xmlDataContract, bool isMemberType)
        {
            return ReadIXmlSerializable(new XmlSerializableReader(), xmlReader, xmlDataContract, isMemberType);
        }

        public SerializationInfo ReadSerializationInfo(XmlReaderDelegator xmlReader, Type type)
        {
            XmlNodeType type2;
            SerializationInfo info = new SerializationInfo(type, XmlObjectSerializer.FormatterConverter);
            while ((type2 = xmlReader.MoveToContent()) != XmlNodeType.EndElement)
            {
                if (type2 != XmlNodeType.Element)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.Element, xmlReader));
                }
                if (xmlReader.NamespaceURI.Length != 0)
                {
                    this.SkipUnknownElement(xmlReader);
                }
                else
                {
                    object obj2;
                    string name = XmlConvert.DecodeName(xmlReader.LocalName);
                    base.IncrementItemCount(1);
                    this.ReadAttributes(xmlReader);
                    if (this.attributes.Ref != Globals.NewObjectId)
                    {
                        xmlReader.Skip();
                        obj2 = this.GetExistingObject(this.attributes.Ref, null, name, string.Empty);
                    }
                    else if (this.attributes.XsiNil)
                    {
                        xmlReader.Skip();
                        obj2 = null;
                    }
                    else
                    {
                        obj2 = this.InternalDeserialize(xmlReader, Globals.TypeOfObject, name, string.Empty);
                    }
                    info.AddValue(name, obj2);
                }
            }
            return info;
        }

        private ClassDataNode ReadUnknownClassData(XmlReaderDelegator xmlReader, string dataContractName, string dataContractNamespace)
        {
            XmlNodeType type;
            ClassDataNode dataNode = new ClassDataNode();
            this.InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);
            int num = 0;
            while ((type = xmlReader.MoveToContent()) != XmlNodeType.EndElement)
            {
                if (type != XmlNodeType.Element)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.Element, xmlReader));
                }
                if (dataNode.Members == null)
                {
                    dataNode.Members = new List<ExtensionDataMember>();
                }
                dataNode.Members.Add(this.ReadExtensionDataMember(xmlReader, num++));
            }
            xmlReader.ReadEndElement();
            return dataNode;
        }

        private CollectionDataNode ReadUnknownCollectionData(XmlReaderDelegator xmlReader, string dataContractName, string dataContractNamespace)
        {
            XmlNodeType type;
            CollectionDataNode dataNode = new CollectionDataNode();
            this.InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);
            int arraySZSize = this.attributes.ArraySZSize;
            while ((type = xmlReader.MoveToContent()) != XmlNodeType.EndElement)
            {
                if (type != XmlNodeType.Element)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.Element, xmlReader));
                }
                if (dataNode.ItemName == null)
                {
                    dataNode.ItemName = xmlReader.LocalName;
                    dataNode.ItemNamespace = xmlReader.NamespaceURI;
                }
                if (xmlReader.IsStartElement(dataNode.ItemName, dataNode.ItemNamespace))
                {
                    if (dataNode.Items == null)
                    {
                        dataNode.Items = new List<IDataNode>();
                    }
                    dataNode.Items.Add(this.ReadExtensionDataValue(xmlReader));
                }
                else
                {
                    this.SkipUnknownElement(xmlReader);
                }
            }
            xmlReader.ReadEndElement();
            if (arraySZSize != -1)
            {
                dataNode.Size = arraySZSize;
                if (dataNode.Items == null)
                {
                    if (dataNode.Size > 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ArraySizeAttributeIncorrect", new object[] { arraySZSize, 0 })));
                    }
                    return dataNode;
                }
                if (dataNode.Size != dataNode.Items.Count)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ArraySizeAttributeIncorrect", new object[] { arraySZSize, dataNode.Items.Count })));
                }
                return dataNode;
            }
            if (dataNode.Items != null)
            {
                dataNode.Size = dataNode.Items.Count;
                return dataNode;
            }
            dataNode.Size = 0;
            return dataNode;
        }

        private ISerializableDataNode ReadUnknownISerializableData(XmlReaderDelegator xmlReader, string dataContractName, string dataContractNamespace)
        {
            XmlNodeType type;
            ISerializableDataNode dataNode = new ISerializableDataNode();
            this.InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);
            dataNode.FactoryTypeName = this.attributes.FactoryTypeName;
            dataNode.FactoryTypeNamespace = this.attributes.FactoryTypeNamespace;
            while ((type = xmlReader.MoveToContent()) != XmlNodeType.EndElement)
            {
                if (type != XmlNodeType.Element)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.Element, xmlReader));
                }
                if (xmlReader.NamespaceURI.Length != 0)
                {
                    this.SkipUnknownElement(xmlReader);
                }
                else
                {
                    ISerializableDataMember item = new ISerializableDataMember {
                        Name = xmlReader.LocalName,
                        Value = this.ReadExtensionDataValue(xmlReader)
                    };
                    if (dataNode.Members == null)
                    {
                        dataNode.Members = new List<ISerializableDataMember>();
                    }
                    dataNode.Members.Add(item);
                }
            }
            xmlReader.ReadEndElement();
            return dataNode;
        }

        private IDataNode ReadUnknownPrimitiveData(XmlReaderDelegator xmlReader, Type type, string dataContractName, string dataContractNamespace)
        {
            IDataNode dataNode = xmlReader.ReadExtensionData(type);
            this.InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);
            return dataNode;
        }

        private IDataNode ReadUnknownXmlData(XmlReaderDelegator xmlReader, string dataContractName, string dataContractNamespace)
        {
            XmlDataNode dataNode = new XmlDataNode();
            this.InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);
            dataNode.OwnerDocument = this.Document;
            if (xmlReader.NodeType != XmlNodeType.EndElement)
            {
                IList<System.Xml.XmlAttribute> list = null;
                IList<System.Xml.XmlNode> list2 = null;
                if (xmlReader.MoveToContent() != XmlNodeType.Text)
                {
                    while (xmlReader.MoveToNextAttribute())
                    {
                        string namespaceURI = xmlReader.NamespaceURI;
                        if ((namespaceURI != "http://schemas.microsoft.com/2003/10/Serialization/") && (namespaceURI != "http://www.w3.org/2001/XMLSchema-instance"))
                        {
                            if (list == null)
                            {
                                list = new List<System.Xml.XmlAttribute>();
                            }
                            list.Add((System.Xml.XmlAttribute) this.Document.ReadNode(xmlReader.UnderlyingReader));
                        }
                    }
                    Read(xmlReader);
                }
                while (xmlReader.MoveToContent() != XmlNodeType.EndElement)
                {
                    if (xmlReader.EOF)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("UnexpectedEndOfFile")));
                    }
                    if (list2 == null)
                    {
                        list2 = new List<System.Xml.XmlNode>();
                    }
                    list2.Add(this.Document.ReadNode(xmlReader.UnderlyingReader));
                }
                xmlReader.ReadEndElement();
                dataNode.XmlAttributes = list;
                dataNode.XmlChildNodes = list2;
            }
            return dataNode;
        }

        public void ReplaceDeserializedObject(string id, object oldObj, object newObj)
        {
            if (!object.ReferenceEquals(oldObj, newObj))
            {
                if (id != Globals.NewObjectId)
                {
                    if (this.DeserializedObjects.IsObjectReferenced(id))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("FactoryObjectContainsSelfReference", new object[] { DataContract.GetClrTypeFullName(oldObj.GetType()), DataContract.GetClrTypeFullName(newObj.GetType()), id })));
                    }
                    this.DeserializedObjects.Remove(id);
                    this.DeserializedObjects.Add(id, newObj);
                }
                if (this.extensionDataReader != null)
                {
                    this.extensionDataReader.UnderlyingExtensionDataReader.SetDeserializedValue(newObj);
                }
            }
        }

        private bool ReplaceScopedKnownTypesTop(Dictionary<XmlQualifiedName, DataContract> knownDataContracts, bool knownTypesAddedInCurrentScope)
        {
            if (knownTypesAddedInCurrentScope)
            {
                this.scopedKnownTypes.Pop();
                knownTypesAddedInCurrentScope = false;
            }
            if (knownDataContracts != null)
            {
                this.scopedKnownTypes.Push(knownDataContracts);
                knownTypesAddedInCurrentScope = true;
            }
            return knownTypesAddedInCurrentScope;
        }

        public void ResetAttributes()
        {
            if (this.attributes != null)
            {
                this.attributes.Reset();
            }
        }

        protected virtual DataContract ResolveDataContractFromTypeName()
        {
            if (this.attributes.XsiTypeName != null)
            {
                return base.ResolveDataContractFromKnownTypes(this.attributes.XsiTypeName, this.attributes.XsiTypeNamespace, null, null);
            }
            return null;
        }

        public void SkipUnknownElement(XmlReaderDelegator xmlReader)
        {
            this.ReadAttributes(xmlReader);
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.Trace(TraceEventType.Verbose, 0x30007, System.Runtime.Serialization.SR.GetString("TraceCodeElementIgnored"), new StringTraceRecord("Element", xmlReader.NamespaceURI + ":" + xmlReader.LocalName));
            }
            xmlReader.Skip();
        }

        protected virtual void StartReadExtensionDataValue(XmlReaderDelegator xmlReader)
        {
        }

        internal void StoreCollectionMemberInfo(object collectionMember)
        {
            this.getOnlyCollectionValue = collectionMember;
            this.isGetOnlyCollection = true;
        }

        internal static void ThrowArrayExceededSizeException(int arraySize, Type type)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ArrayExceededSize", new object[] { arraySize, DataContract.GetClrTypeFullName(type) })));
        }

        internal static void ThrowNullValueReturnedForGetOnlyCollectionException(Type type)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("NullValueReturnedForGetOnlyCollection", new object[] { DataContract.GetClrTypeFullName(type) })));
        }

        public static void ThrowRequiredMemberMissingException(XmlReaderDelegator xmlReader, int memberIndex, int requiredIndex, XmlDictionaryString[] memberNames)
        {
            StringBuilder builder = new StringBuilder();
            if (requiredIndex == memberNames.Length)
            {
                requiredIndex--;
            }
            for (int i = memberIndex + 1; i <= requiredIndex; i++)
            {
                if (builder.Length != 0)
                {
                    builder.Append(" | ");
                }
                builder.Append(memberNames[i].Value);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(XmlObjectSerializer.TryAddLineInfo(xmlReader, System.Runtime.Serialization.SR.GetString("UnexpectedElementExpectingElements", new object[] { xmlReader.NodeType, xmlReader.LocalName, xmlReader.NamespaceURI, builder.ToString() }))));
        }

        public static T[] TrimArraySize<T>(T[] array, int size)
        {
            if (size != array.Length)
            {
                T[] destinationArray = new T[size];
                Array.Copy(array, 0, destinationArray, 0, size);
                array = destinationArray;
            }
            return array;
        }

        protected bool TryHandleNullOrRef(XmlReaderDelegator reader, Type declaredType, string name, string ns, ref object retObj)
        {
            this.ReadAttributes(reader);
            if (this.attributes.Ref != Globals.NewObjectId)
            {
                if (this.isGetOnlyCollection)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("IsReferenceGetOnlyCollectionsNotSupported", new object[] { this.attributes.Ref, DataContract.GetClrTypeFullName(declaredType) })));
                }
                retObj = this.GetExistingObject(this.attributes.Ref, declaredType, name, ns);
                reader.Skip();
                return true;
            }
            if (this.attributes.XsiNil)
            {
                reader.Skip();
                return true;
            }
            return false;
        }

        private HybridObjectCache DeserializedObjects
        {
            get
            {
                if (this.deserializedObjects == null)
                {
                    this.deserializedObjects = new HybridObjectCache();
                }
                return this.deserializedObjects;
            }
        }

        private XmlDocument Document
        {
            get
            {
                if (this.xmlDocument == null)
                {
                    this.xmlDocument = new XmlDocument();
                }
                return this.xmlDocument;
            }
        }

        internal override bool IsGetOnlyCollection
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isGetOnlyCollection;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.isGetOnlyCollection = value;
            }
        }
    }
}

