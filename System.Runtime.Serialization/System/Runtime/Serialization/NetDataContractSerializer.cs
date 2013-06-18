namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization.Formatters;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    public sealed class NetDataContractSerializer : XmlObjectSerializer, IFormatter
    {
        private FormatterAssemblyStyle assemblyFormat;
        private SerializationBinder binder;
        private DataContract cachedDataContract;
        private StreamingContext context;
        private bool ignoreExtensionDataObject;
        private int maxItemsInObjectGraph;
        private XmlDictionaryString rootName;
        private XmlDictionaryString rootNamespace;
        private ISurrogateSelector surrogateSelector;
        private static Hashtable typeNameCache = new Hashtable();

        public NetDataContractSerializer() : this(new StreamingContext(StreamingContextStates.All))
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public NetDataContractSerializer(StreamingContext context) : this(context, 0x7fffffff, false, FormatterAssemblyStyle.Full, null)
        {
        }

        public NetDataContractSerializer(string rootName, string rootNamespace) : this(rootName, rootNamespace, new StreamingContext(StreamingContextStates.All), 0x7fffffff, false, FormatterAssemblyStyle.Full, null)
        {
        }

        public NetDataContractSerializer(XmlDictionaryString rootName, XmlDictionaryString rootNamespace) : this(rootName, rootNamespace, new StreamingContext(StreamingContextStates.All), 0x7fffffff, false, FormatterAssemblyStyle.Full, null)
        {
        }

        public NetDataContractSerializer(StreamingContext context, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, FormatterAssemblyStyle assemblyFormat, ISurrogateSelector surrogateSelector)
        {
            this.Initialize(context, maxItemsInObjectGraph, ignoreExtensionDataObject, assemblyFormat, surrogateSelector);
        }

        public NetDataContractSerializer(string rootName, string rootNamespace, StreamingContext context, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, FormatterAssemblyStyle assemblyFormat, ISurrogateSelector surrogateSelector)
        {
            XmlDictionary dictionary = new XmlDictionary(2);
            this.Initialize(dictionary.Add(rootName), dictionary.Add(DataContract.GetNamespace(rootNamespace)), context, maxItemsInObjectGraph, ignoreExtensionDataObject, assemblyFormat, surrogateSelector);
        }

        public NetDataContractSerializer(XmlDictionaryString rootName, XmlDictionaryString rootNamespace, StreamingContext context, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, FormatterAssemblyStyle assemblyFormat, ISurrogateSelector surrogateSelector)
        {
            this.Initialize(rootName, rootNamespace, context, maxItemsInObjectGraph, ignoreExtensionDataObject, assemblyFormat, surrogateSelector);
        }

        public object Deserialize(Stream stream)
        {
            return base.ReadObject(stream);
        }

        internal DataContract GetDataContract(object obj, ref Hashtable surrogateDataContracts)
        {
            return this.GetDataContract((obj == null) ? Globals.TypeOfObject : obj.GetType(), ref surrogateDataContracts);
        }

        internal DataContract GetDataContract(Type type, ref Hashtable surrogateDataContracts)
        {
            return this.GetDataContract(type.TypeHandle, type, ref surrogateDataContracts);
        }

        internal DataContract GetDataContract(RuntimeTypeHandle typeHandle, Type type, ref Hashtable surrogateDataContracts)
        {
            DataContract contract = GetDataContractFromSurrogateSelector(this.surrogateSelector, this.Context, typeHandle, type, ref surrogateDataContracts);
            if (contract != null)
            {
                return contract;
            }
            if (this.cachedDataContract == null)
            {
                contract = DataContract.GetDataContract(typeHandle, type, SerializationMode.SharedType);
                this.cachedDataContract = contract;
                return contract;
            }
            DataContract cachedDataContract = this.cachedDataContract;
            if (cachedDataContract.UnderlyingType.TypeHandle.Equals(typeHandle))
            {
                return cachedDataContract;
            }
            return DataContract.GetDataContract(typeHandle, type, SerializationMode.SharedType);
        }

        internal static DataContract GetDataContractFromSurrogateSelector(ISurrogateSelector surrogateSelector, StreamingContext context, RuntimeTypeHandle typeHandle, Type type, ref Hashtable surrogateDataContracts)
        {
            if (surrogateSelector == null)
            {
                return null;
            }
            if (type == null)
            {
                type = Type.GetTypeFromHandle(typeHandle);
            }
            DataContract builtInDataContract = DataContract.GetBuiltInDataContract(type);
            if (builtInDataContract != null)
            {
                return builtInDataContract;
            }
            if (surrogateDataContracts != null)
            {
                DataContract contract2 = (DataContract) surrogateDataContracts[type];
                if (contract2 != null)
                {
                    return contract2;
                }
            }
            DataContract contract3 = null;
            ISerializationSurrogate serializationSurrogate = GetSurrogate(type, surrogateSelector, context);
            if (serializationSurrogate != null)
            {
                contract3 = new SurrogateDataContract(type, serializationSurrogate);
            }
            else if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                DataContract itemContract = GetDataContractFromSurrogateSelector(surrogateSelector, context, elementType.TypeHandle, elementType, ref surrogateDataContracts);
                if (itemContract == null)
                {
                    itemContract = DataContract.GetDataContract(elementType.TypeHandle, elementType, SerializationMode.SharedType);
                }
                contract3 = new CollectionDataContract(type, itemContract);
            }
            if (contract3 == null)
            {
                return null;
            }
            if (surrogateDataContracts == null)
            {
                surrogateDataContracts = new Hashtable();
            }
            surrogateDataContracts.Add(type, contract3);
            return contract3;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        private static ISerializationSurrogate GetSurrogate(Type type, ISurrogateSelector surrogateSelector, StreamingContext context)
        {
            ISurrogateSelector selector;
            return surrogateSelector.GetSurrogate(type, context, out selector);
        }

        internal static TypeInformation GetTypeInformation(Type type)
        {
            TypeInformation information = null;
            object obj2 = typeNameCache[type];
            if (obj2 == null)
            {
                information = new TypeInformation(DataContract.GetClrTypeFullNameUsingTypeForwardedFromAttribute(type), DataContract.GetClrAssemblyName(type));
                lock (typeNameCache)
                {
                    typeNameCache[type] = information;
                    return information;
                }
            }
            return (TypeInformation) obj2;
        }

        private void Initialize(StreamingContext context, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, FormatterAssemblyStyle assemblyFormat, ISurrogateSelector surrogateSelector)
        {
            this.context = context;
            if (maxItemsInObjectGraph < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxItemsInObjectGraph", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            this.maxItemsInObjectGraph = maxItemsInObjectGraph;
            this.ignoreExtensionDataObject = ignoreExtensionDataObject;
            this.surrogateSelector = surrogateSelector;
            this.AssemblyFormat = assemblyFormat;
        }

        private void Initialize(XmlDictionaryString rootName, XmlDictionaryString rootNamespace, StreamingContext context, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, FormatterAssemblyStyle assemblyFormat, ISurrogateSelector surrogateSelector)
        {
            this.Initialize(context, maxItemsInObjectGraph, ignoreExtensionDataObject, assemblyFormat, surrogateSelector);
            this.rootName = rootName;
            this.rootNamespace = rootNamespace;
        }

        internal override bool InternalIsStartObject(XmlReaderDelegator reader)
        {
            return base.IsStartElement(reader);
        }

        internal override object InternalReadObject(XmlReaderDelegator xmlReader, bool verifyObjectName)
        {
            if (this.MaxItemsInObjectGraph == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ExceededMaxItemsQuota", new object[] { this.MaxItemsInObjectGraph })));
            }
            if (!base.IsStartElement(xmlReader))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationExceptionWithReaderDetails(System.Runtime.Serialization.SR.GetString("ExpectingElementAtDeserialize", new object[] { XmlNodeType.Element }), xmlReader));
            }
            return XmlObjectSerializerReadContext.CreateContext(this).InternalDeserialize(xmlReader, null, null, null);
        }

        internal override void InternalWriteEndObject(XmlWriterDelegator writer)
        {
            writer.WriteEndElement();
        }

        internal override void InternalWriteObject(XmlWriterDelegator writer, object graph)
        {
            Hashtable surrogateDataContracts = null;
            DataContract dataContract = this.GetDataContract(graph, ref surrogateDataContracts);
            this.InternalWriteStartObject(writer, graph, dataContract);
            this.InternalWriteObjectContent(writer, graph, dataContract, surrogateDataContracts);
            this.InternalWriteEndObject(writer);
        }

        internal override void InternalWriteObjectContent(XmlWriterDelegator writer, object graph)
        {
            Hashtable surrogateDataContracts = null;
            DataContract dataContract = this.GetDataContract(graph, ref surrogateDataContracts);
            this.InternalWriteObjectContent(writer, graph, dataContract, surrogateDataContracts);
        }

        private void InternalWriteObjectContent(XmlWriterDelegator writer, object graph, DataContract contract, Hashtable surrogateDataContracts)
        {
            if (this.MaxItemsInObjectGraph == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ExceededMaxItemsQuota", new object[] { this.MaxItemsInObjectGraph })));
            }
            if (base.IsRootXmlAny(this.rootName, contract))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("IsAnyNotSupportedByNetDataContractSerializer", new object[] { contract.UnderlyingType })));
            }
            if (graph == null)
            {
                XmlObjectSerializer.WriteNull(writer);
            }
            else
            {
                Type type = graph.GetType();
                if (contract.UnderlyingType != type)
                {
                    contract = this.GetDataContract(graph, ref surrogateDataContracts);
                }
                XmlObjectSerializerWriteContext context = null;
                if (contract.CanContainReferences)
                {
                    context = XmlObjectSerializerWriteContext.CreateContext(this, surrogateDataContracts);
                    context.HandleGraphAtTopLevel(writer, graph, contract);
                }
                WriteClrTypeInfo(writer, contract, this.binder);
                contract.WriteXmlValue(writer, graph, context);
            }
        }

        internal override void InternalWriteStartObject(XmlWriterDelegator writer, object graph)
        {
            Hashtable surrogateDataContracts = null;
            DataContract dataContract = this.GetDataContract(graph, ref surrogateDataContracts);
            this.InternalWriteStartObject(writer, graph, dataContract);
        }

        private void InternalWriteStartObject(XmlWriterDelegator writer, object graph, DataContract contract)
        {
            base.WriteRootElement(writer, contract, this.rootName, this.rootNamespace, base.CheckIfNeedsContractNsAtRoot(this.rootName, this.rootNamespace, contract));
        }

        public override bool IsStartObject(XmlDictionaryReader reader)
        {
            return base.IsStartObjectHandleExceptions(new XmlReaderDelegator(reader));
        }

        public override bool IsStartObject(XmlReader reader)
        {
            return base.IsStartObjectHandleExceptions(new XmlReaderDelegator(reader));
        }

        public override object ReadObject(XmlReader reader)
        {
            return base.ReadObjectHandleExceptions(new XmlReaderDelegator(reader), true);
        }

        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
        {
            return base.ReadObjectHandleExceptions(new XmlReaderDelegator(reader), verifyObjectName);
        }

        public override object ReadObject(XmlReader reader, bool verifyObjectName)
        {
            return base.ReadObjectHandleExceptions(new XmlReaderDelegator(reader), verifyObjectName);
        }

        public void Serialize(Stream stream, object graph)
        {
            base.WriteObject(stream, graph);
        }

        internal static void WriteClrTypeInfo(XmlWriterDelegator writer, DataContract dataContract, SerializationBinder binder)
        {
            if (!dataContract.IsISerializable && !(dataContract is SurrogateDataContract))
            {
                TypeInformation typeInformation = null;
                string typeName = null;
                string assemblyName = null;
                if (binder != null)
                {
                    binder.BindToName(dataContract.OriginalUnderlyingType, out assemblyName, out typeName);
                }
                if (typeName == null)
                {
                    typeInformation = GetTypeInformation(dataContract.OriginalUnderlyingType);
                    typeName = typeInformation.FullTypeName;
                }
                if (assemblyName == null)
                {
                    assemblyName = (typeInformation == null) ? GetTypeInformation(dataContract.OriginalUnderlyingType).AssemblyString : typeInformation.AssemblyString;
                }
                WriteClrTypeInfo(writer, typeName, assemblyName);
            }
        }

        private static void WriteClrTypeInfo(XmlWriterDelegator writer, string clrTypeName, string clrAssemblyName)
        {
            if (clrTypeName != null)
            {
                writer.WriteAttributeString("z", DictionaryGlobals.ClrTypeLocalName, DictionaryGlobals.SerializationNamespace, DataContract.GetClrTypeString(clrTypeName));
            }
            if (clrAssemblyName != null)
            {
                writer.WriteAttributeString("z", DictionaryGlobals.ClrAssemblyLocalName, DictionaryGlobals.SerializationNamespace, DataContract.GetClrTypeString(clrAssemblyName));
            }
        }

        internal static void WriteClrTypeInfo(XmlWriterDelegator writer, Type dataContractType, SerializationBinder binder, SerializationInfo serInfo)
        {
            TypeInformation typeInformation = null;
            string typeName = null;
            string assemblyName = null;
            if (binder != null)
            {
                binder.BindToName(dataContractType, out assemblyName, out typeName);
            }
            if (typeName == null)
            {
                if (serInfo.IsFullTypeNameSetExplicit)
                {
                    typeName = serInfo.FullTypeName;
                }
                else
                {
                    typeInformation = GetTypeInformation(serInfo.ObjectType);
                    typeName = typeInformation.FullTypeName;
                }
            }
            if (assemblyName == null)
            {
                if (serInfo.IsAssemblyNameSetExplicit)
                {
                    assemblyName = serInfo.AssemblyName;
                }
                else
                {
                    assemblyName = (typeInformation == null) ? GetTypeInformation(serInfo.ObjectType).AssemblyString : typeInformation.AssemblyString;
                }
            }
            WriteClrTypeInfo(writer, typeName, assemblyName);
        }

        internal static void WriteClrTypeInfo(XmlWriterDelegator writer, Type dataContractType, SerializationBinder binder, string defaultClrTypeName, string defaultClrAssemblyName)
        {
            string typeName = null;
            string assemblyName = null;
            if (binder != null)
            {
                binder.BindToName(dataContractType, out assemblyName, out typeName);
            }
            if (typeName == null)
            {
                typeName = defaultClrTypeName;
            }
            if (assemblyName == null)
            {
                assemblyName = defaultClrAssemblyName;
            }
            WriteClrTypeInfo(writer, typeName, assemblyName);
        }

        public override void WriteEndObject(XmlDictionaryWriter writer)
        {
            base.WriteEndObjectHandleExceptions(new XmlWriterDelegator(writer));
        }

        public override void WriteEndObject(XmlWriter writer)
        {
            base.WriteEndObjectHandleExceptions(new XmlWriterDelegator(writer));
        }

        public override void WriteObject(XmlWriter writer, object graph)
        {
            base.WriteObjectHandleExceptions(new XmlWriterDelegator(writer), graph);
        }

        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            base.WriteObjectContentHandleExceptions(new XmlWriterDelegator(writer), graph);
        }

        public override void WriteObjectContent(XmlWriter writer, object graph)
        {
            base.WriteObjectContentHandleExceptions(new XmlWriterDelegator(writer), graph);
        }

        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
        {
            base.WriteStartObjectHandleExceptions(new XmlWriterDelegator(writer), graph);
        }

        public override void WriteStartObject(XmlWriter writer, object graph)
        {
            base.WriteStartObjectHandleExceptions(new XmlWriterDelegator(writer), graph);
        }

        public FormatterAssemblyStyle AssemblyFormat
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.assemblyFormat;
            }
            set
            {
                if ((value != FormatterAssemblyStyle.Full) && (value != FormatterAssemblyStyle.Simple))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("InvalidAssemblyFormat", new object[] { value })));
                }
                this.assemblyFormat = value;
            }
        }

        public SerializationBinder Binder
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.binder;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.binder = value;
            }
        }

        public StreamingContext Context
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.context;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.context = value;
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

        public int MaxItemsInObjectGraph
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.maxItemsInObjectGraph;
            }
        }

        public ISurrogateSelector SurrogateSelector
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.surrogateSelector;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.surrogateSelector = value;
            }
        }
    }
}

