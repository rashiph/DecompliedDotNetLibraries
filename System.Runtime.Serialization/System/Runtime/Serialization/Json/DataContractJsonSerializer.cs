namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public sealed class DataContractJsonSerializer : XmlObjectSerializer
    {
        private bool alwaysEmitTypeInformation;
        private IDataContractSurrogate dataContractSurrogate;
        private bool ignoreExtensionDataObject;
        internal Dictionary<XmlQualifiedName, DataContract> knownDataContracts;
        private ReadOnlyCollection<Type> knownTypeCollection;
        internal IList<Type> knownTypeList;
        private int maxItemsInObjectGraph;
        private DataContract rootContract;
        private XmlDictionaryString rootName;
        private bool rootNameRequiresMapping;
        private Type rootType;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DataContractJsonSerializer(Type type) : this(type, (IEnumerable<Type>) null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DataContractJsonSerializer(Type type, string rootName) : this(type, rootName, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DataContractJsonSerializer(Type type, XmlDictionaryString rootName) : this(type, rootName, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DataContractJsonSerializer(Type type, IEnumerable<Type> knownTypes) : this(type, knownTypes, 0x7fffffff, false, null, false)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DataContractJsonSerializer(Type type, string rootName, IEnumerable<Type> knownTypes) : this(type, rootName, knownTypes, 0x7fffffff, false, null, false)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DataContractJsonSerializer(Type type, XmlDictionaryString rootName, IEnumerable<Type> knownTypes) : this(type, rootName, knownTypes, 0x7fffffff, false, null, false)
        {
        }

        public DataContractJsonSerializer(Type type, IEnumerable<Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, IDataContractSurrogate dataContractSurrogate, bool alwaysEmitTypeInformation)
        {
            this.Initialize(type, knownTypes, maxItemsInObjectGraph, ignoreExtensionDataObject, dataContractSurrogate, alwaysEmitTypeInformation);
        }

        public DataContractJsonSerializer(Type type, string rootName, IEnumerable<Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, IDataContractSurrogate dataContractSurrogate, bool alwaysEmitTypeInformation)
        {
            XmlDictionary dictionary = new XmlDictionary(2);
            this.Initialize(type, dictionary.Add(rootName), knownTypes, maxItemsInObjectGraph, ignoreExtensionDataObject, dataContractSurrogate, alwaysEmitTypeInformation);
        }

        public DataContractJsonSerializer(Type type, XmlDictionaryString rootName, IEnumerable<Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, IDataContractSurrogate dataContractSurrogate, bool alwaysEmitTypeInformation)
        {
            this.Initialize(type, rootName, knownTypes, maxItemsInObjectGraph, ignoreExtensionDataObject, dataContractSurrogate, alwaysEmitTypeInformation);
        }

        private void AddCollectionItemTypeToKnownTypes(Type knownType)
        {
            Type type;
            for (Type type2 = knownType; CollectionDataContract.IsCollection(type2, out type); type2 = type)
            {
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == Globals.TypeOfKeyValue))
                {
                    type = Globals.TypeOfKeyValuePair.MakeGenericType(type.GetGenericArguments());
                }
                this.knownTypeList.Add(type);
            }
        }

        internal static bool CheckIfJsonNameRequiresMapping(string jsonName)
        {
            if (jsonName != null)
            {
                if (!DataContract.IsValidNCName(jsonName))
                {
                    return true;
                }
                for (int i = 0; i < jsonName.Length; i++)
                {
                    if (XmlJsonWriter.CharacterNeedsEscaping(jsonName[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool CheckIfJsonNameRequiresMapping(XmlDictionaryString jsonName)
        {
            return ((jsonName != null) && CheckIfJsonNameRequiresMapping(jsonName.Value));
        }

        internal static void CheckIfTypeIsReference(DataContract dataContract)
        {
            if (dataContract.IsReference)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("JsonUnsupportedForIsReference", new object[] { DataContract.GetClrTypeFullName(dataContract.UnderlyingType), dataContract.IsReference })));
            }
        }

        internal static bool CheckIfXmlNameRequiresMapping(string xmlName)
        {
            return ((xmlName != null) && CheckIfJsonNameRequiresMapping(ConvertXmlNameToJsonName(xmlName)));
        }

        internal static bool CheckIfXmlNameRequiresMapping(XmlDictionaryString xmlName)
        {
            return ((xmlName != null) && CheckIfXmlNameRequiresMapping(xmlName.Value));
        }

        internal static string ConvertXmlNameToJsonName(string xmlName)
        {
            return XmlConvert.DecodeName(xmlName);
        }

        internal static XmlDictionaryString ConvertXmlNameToJsonName(XmlDictionaryString xmlName)
        {
            if (xmlName != null)
            {
                return new XmlDictionary().Add(ConvertXmlNameToJsonName(xmlName.Value));
            }
            return null;
        }

        internal static DataContract GetDataContract(DataContract declaredTypeContract, Type declaredType, Type objectType)
        {
            DataContract dataContract = DataContractSerializer.GetDataContract(declaredTypeContract, declaredType, objectType);
            CheckIfTypeIsReference(dataContract);
            return dataContract;
        }

        internal override Type GetDeserializeType()
        {
            return this.rootType;
        }

        internal override Type GetSerializeType(object graph)
        {
            if (graph != null)
            {
                return graph.GetType();
            }
            return this.rootType;
        }

        private void Initialize(Type type, IEnumerable<Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, IDataContractSurrogate dataContractSurrogate, bool alwaysEmitTypeInformation)
        {
            XmlObjectSerializer.CheckNull(type, "type");
            this.rootType = type;
            if (knownTypes != null)
            {
                this.knownTypeList = new List<Type>();
                foreach (Type type2 in knownTypes)
                {
                    this.knownTypeList.Add(type2);
                    if (type2 != null)
                    {
                        this.AddCollectionItemTypeToKnownTypes(type2);
                    }
                }
            }
            if (maxItemsInObjectGraph < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxItemsInObjectGraph", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            this.maxItemsInObjectGraph = maxItemsInObjectGraph;
            this.ignoreExtensionDataObject = ignoreExtensionDataObject;
            this.dataContractSurrogate = dataContractSurrogate;
            this.alwaysEmitTypeInformation = alwaysEmitTypeInformation;
        }

        private void Initialize(Type type, XmlDictionaryString rootName, IEnumerable<Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, IDataContractSurrogate dataContractSurrogate, bool alwaysEmitTypeInformation)
        {
            this.Initialize(type, knownTypes, maxItemsInObjectGraph, ignoreExtensionDataObject, dataContractSurrogate, alwaysEmitTypeInformation);
            this.rootName = ConvertXmlNameToJsonName(rootName);
            this.rootNameRequiresMapping = CheckIfJsonNameRequiresMapping(this.rootName);
        }

        internal override bool InternalIsStartObject(XmlReaderDelegator reader)
        {
            return (base.IsRootElement(reader, this.RootContract, this.RootName, XmlDictionaryString.Empty) || IsJsonLocalName(reader, this.RootName.Value));
        }

        internal override object InternalReadObject(XmlReaderDelegator xmlReader, bool verifyObjectName)
        {
            if (this.MaxItemsInObjectGraph == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ExceededMaxItemsQuota", new object[] { this.MaxItemsInObjectGraph })));
            }
            if (verifyObjectName)
            {
                if (!this.InternalIsStartObject(xmlReader))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationExceptionWithReaderDetails(System.Runtime.Serialization.SR.GetString("ExpectingElement", new object[] { XmlDictionaryString.Empty, this.RootName }), xmlReader));
                }
            }
            else if (!base.IsStartElement(xmlReader))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationExceptionWithReaderDetails(System.Runtime.Serialization.SR.GetString("ExpectingElementAtDeserialize", new object[] { XmlNodeType.Element }), xmlReader));
            }
            DataContract rootContract = this.RootContract;
            if (rootContract.IsPrimitive && object.ReferenceEquals(rootContract.UnderlyingType, this.rootType))
            {
                return ReadJsonValue(rootContract, xmlReader, null);
            }
            return XmlObjectSerializerReadContextComplexJson.CreateContext(this, rootContract).InternalDeserialize(xmlReader, this.rootType, rootContract, null, null);
        }

        internal override void InternalWriteEndObject(XmlWriterDelegator writer)
        {
            writer.WriteEndElement();
        }

        internal override void InternalWriteObject(XmlWriterDelegator writer, object graph)
        {
            this.InternalWriteStartObject(writer, graph);
            this.InternalWriteObjectContent(writer, graph);
            this.InternalWriteEndObject(writer);
        }

        internal override void InternalWriteObjectContent(XmlWriterDelegator writer, object graph)
        {
            if (this.MaxItemsInObjectGraph == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ExceededMaxItemsQuota", new object[] { this.MaxItemsInObjectGraph })));
            }
            DataContract rootContract = this.RootContract;
            Type underlyingType = rootContract.UnderlyingType;
            Type objType = (graph == null) ? underlyingType : graph.GetType();
            if (this.dataContractSurrogate != null)
            {
                graph = DataContractSerializer.SurrogateToDataContractType(this.dataContractSurrogate, graph, underlyingType, ref objType);
            }
            if (graph == null)
            {
                WriteJsonNull(writer);
            }
            else if (underlyingType == objType)
            {
                if (rootContract.CanContainReferences)
                {
                    XmlObjectSerializerWriteContextComplexJson json = XmlObjectSerializerWriteContextComplexJson.CreateContext(this, rootContract);
                    json.OnHandleReference(writer, graph, true);
                    json.SerializeWithoutXsiType(rootContract, writer, graph, underlyingType.TypeHandle);
                }
                else
                {
                    WriteJsonValue(JsonDataContract.GetJsonDataContract(rootContract), writer, graph, null, underlyingType.TypeHandle);
                }
            }
            else
            {
                XmlObjectSerializerWriteContextComplexJson json2 = XmlObjectSerializerWriteContextComplexJson.CreateContext(this, this.RootContract);
                rootContract = GetDataContract(rootContract, underlyingType, objType);
                if (rootContract.CanContainReferences)
                {
                    json2.OnHandleReference(writer, graph, true);
                    json2.SerializeWithXsiTypeAtTopLevel(rootContract, writer, graph, underlyingType.TypeHandle, objType);
                }
                else
                {
                    json2.SerializeWithoutXsiType(rootContract, writer, graph, underlyingType.TypeHandle);
                }
            }
        }

        internal override void InternalWriteStartObject(XmlWriterDelegator writer, object graph)
        {
            if (this.rootNameRequiresMapping)
            {
                writer.WriteStartElement("a", "item", "item");
                writer.WriteAttributeString(null, "item", null, this.RootName.Value);
            }
            else
            {
                writer.WriteStartElement(this.RootName, XmlDictionaryString.Empty);
            }
        }

        internal static bool IsJsonLocalName(XmlReaderDelegator reader, string elementName)
        {
            string str;
            return (XmlObjectSerializerReadContextComplexJson.TryGetJsonLocalName(reader, out str) && (elementName == str));
        }

        public override bool IsStartObject(XmlDictionaryReader reader)
        {
            return base.IsStartObjectHandleExceptions(new JsonReaderDelegator(reader));
        }

        public override bool IsStartObject(XmlReader reader)
        {
            return base.IsStartObjectHandleExceptions(new JsonReaderDelegator(reader));
        }

        internal static object ReadJsonValue(DataContract contract, XmlReaderDelegator reader, XmlObjectSerializerReadContextComplexJson context)
        {
            return JsonDataContract.GetJsonDataContract(contract).ReadJsonValue(reader, context);
        }

        public override object ReadObject(Stream stream)
        {
            XmlObjectSerializer.CheckNull(stream, "stream");
            return this.ReadObject(JsonReaderWriterFactory.CreateJsonReader(stream, XmlDictionaryReaderQuotas.Max));
        }

        public override object ReadObject(XmlDictionaryReader reader)
        {
            return base.ReadObjectHandleExceptions(new JsonReaderDelegator(reader), true);
        }

        public override object ReadObject(XmlReader reader)
        {
            return base.ReadObjectHandleExceptions(new JsonReaderDelegator(reader), true);
        }

        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
        {
            return base.ReadObjectHandleExceptions(new JsonReaderDelegator(reader), verifyObjectName);
        }

        public override object ReadObject(XmlReader reader, bool verifyObjectName)
        {
            return base.ReadObjectHandleExceptions(new JsonReaderDelegator(reader), verifyObjectName);
        }

        public override void WriteEndObject(XmlDictionaryWriter writer)
        {
            base.WriteEndObjectHandleExceptions(new JsonWriterDelegator(writer));
        }

        public override void WriteEndObject(XmlWriter writer)
        {
            base.WriteEndObjectHandleExceptions(new JsonWriterDelegator(writer));
        }

        internal static void WriteJsonNull(XmlWriterDelegator writer)
        {
            writer.WriteAttributeString(null, "type", null, "null");
        }

        internal static void WriteJsonValue(JsonDataContract contract, XmlWriterDelegator writer, object graph, XmlObjectSerializerWriteContextComplexJson context, RuntimeTypeHandle declaredTypeHandle)
        {
            contract.WriteJsonValue(writer, graph, context, declaredTypeHandle);
        }

        public override void WriteObject(Stream stream, object graph)
        {
            XmlObjectSerializer.CheckNull(stream, "stream");
            XmlDictionaryWriter writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, false);
            this.WriteObject(writer, graph);
            writer.Flush();
        }

        public override void WriteObject(XmlDictionaryWriter writer, object graph)
        {
            base.WriteObjectHandleExceptions(new JsonWriterDelegator(writer), graph);
        }

        public override void WriteObject(XmlWriter writer, object graph)
        {
            base.WriteObjectHandleExceptions(new JsonWriterDelegator(writer), graph);
        }

        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            base.WriteObjectContentHandleExceptions(new JsonWriterDelegator(writer), graph);
        }

        public override void WriteObjectContent(XmlWriter writer, object graph)
        {
            base.WriteObjectContentHandleExceptions(new JsonWriterDelegator(writer), graph);
        }

        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
        {
            base.WriteStartObjectHandleExceptions(new JsonWriterDelegator(writer), graph);
        }

        public override void WriteStartObject(XmlWriter writer, object graph)
        {
            base.WriteStartObjectHandleExceptions(new JsonWriterDelegator(writer), graph);
        }

        internal bool AlwaysEmitTypeInformation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.alwaysEmitTypeInformation;
            }
        }

        public IDataContractSurrogate DataContractSurrogate
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dataContractSurrogate;
            }
        }

        public bool IgnoreExtensionDataObject
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.ignoreExtensionDataObject;
            }
        }

        internal override Dictionary<XmlQualifiedName, DataContract> KnownDataContracts
        {
            get
            {
                if ((this.knownDataContracts == null) && (this.knownTypeList != null))
                {
                    this.knownDataContracts = XmlObjectSerializerContext.GetDataContractsForKnownTypes(this.knownTypeList);
                }
                return this.knownDataContracts;
            }
        }

        public ReadOnlyCollection<Type> KnownTypes
        {
            get
            {
                if (this.knownTypeCollection == null)
                {
                    if (this.knownTypeList != null)
                    {
                        this.knownTypeCollection = new ReadOnlyCollection<Type>(this.knownTypeList);
                    }
                    else
                    {
                        this.knownTypeCollection = new ReadOnlyCollection<Type>(Globals.EmptyTypeArray);
                    }
                }
                return this.knownTypeCollection;
            }
        }

        public int MaxItemsInObjectGraph
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.maxItemsInObjectGraph;
            }
        }

        private DataContract RootContract
        {
            get
            {
                if (this.rootContract == null)
                {
                    this.rootContract = DataContract.GetDataContract((this.dataContractSurrogate == null) ? this.rootType : DataContractSerializer.GetSurrogatedType(this.dataContractSurrogate, this.rootType));
                    CheckIfTypeIsReference(this.rootContract);
                }
                return this.rootContract;
            }
        }

        private XmlDictionaryString RootName
        {
            get
            {
                return (this.rootName ?? JsonGlobals.rootDictionaryString);
            }
        }
    }
}

