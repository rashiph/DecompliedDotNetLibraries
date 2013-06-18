namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Security;
    using System.Xml;

    internal class XmlObjectSerializerContext
    {
        private System.Runtime.Serialization.DataContractResolver dataContractResolver;
        [SecurityCritical]
        private bool demandedMemberAccessPermission;
        [SecurityCritical]
        private bool demandedSerializationFormatterPermission;
        private bool ignoreExtensionDataObject;
        private static MethodInfo incrementItemCountMethod;
        private bool isSerializerKnownDataContractsSetExplicit;
        private int itemCount;
        private KnownTypeDataContractResolver knownTypeResolver;
        private int maxItemsInObjectGraph;
        protected DataContract rootTypeDataContract;
        internal ScopedKnownTypes scopedKnownTypes;
        protected XmlObjectSerializer serializer;
        protected Dictionary<XmlQualifiedName, DataContract> serializerKnownDataContracts;
        protected IList<Type> serializerKnownTypeList;
        private StreamingContext streamingContext;

        internal XmlObjectSerializerContext(NetDataContractSerializer serializer) : this(serializer, serializer.MaxItemsInObjectGraph, serializer.Context, serializer.IgnoreExtensionDataObject)
        {
        }

        internal XmlObjectSerializerContext(DataContractSerializer serializer, DataContract rootTypeDataContract, System.Runtime.Serialization.DataContractResolver dataContractResolver) : this(serializer, serializer.MaxItemsInObjectGraph, new StreamingContext(StreamingContextStates.All), serializer.IgnoreExtensionDataObject, dataContractResolver)
        {
            this.rootTypeDataContract = rootTypeDataContract;
            this.serializerKnownTypeList = serializer.knownTypeList;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal XmlObjectSerializerContext(XmlObjectSerializer serializer, int maxItemsInObjectGraph, StreamingContext streamingContext, bool ignoreExtensionDataObject) : this(serializer, maxItemsInObjectGraph, streamingContext, ignoreExtensionDataObject, null)
        {
        }

        internal XmlObjectSerializerContext(XmlObjectSerializer serializer, int maxItemsInObjectGraph, StreamingContext streamingContext, bool ignoreExtensionDataObject, System.Runtime.Serialization.DataContractResolver dataContractResolver)
        {
            this.scopedKnownTypes = new ScopedKnownTypes();
            this.serializer = serializer;
            this.itemCount = 1;
            this.maxItemsInObjectGraph = maxItemsInObjectGraph;
            this.streamingContext = streamingContext;
            this.ignoreExtensionDataObject = ignoreExtensionDataObject;
            this.dataContractResolver = dataContractResolver;
        }

        internal virtual void CheckIfTypeSerializable(Type memberType, bool isMemberTypeSerializable)
        {
            if (!isMemberTypeSerializable)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("TypeNotSerializable", new object[] { memberType })));
            }
        }

        [SecuritySafeCritical]
        public void DemandMemberAccessPermission()
        {
            if (!this.demandedMemberAccessPermission)
            {
                Globals.MemberAccessPermission.Demand();
                this.demandedMemberAccessPermission = true;
            }
        }

        [SecuritySafeCritical]
        public void DemandSerializationFormatterPermission()
        {
            if (!this.demandedSerializationFormatterPermission)
            {
                Globals.SerializationFormatterPermission.Demand();
                this.demandedSerializationFormatterPermission = true;
            }
        }

        internal DataContract GetDataContract(Type type)
        {
            return this.GetDataContract(type.TypeHandle, type);
        }

        internal virtual DataContract GetDataContract(int id, RuntimeTypeHandle typeHandle)
        {
            if (this.IsGetOnlyCollection)
            {
                return DataContract.GetGetOnlyCollectionDataContract(id, typeHandle, null, this.Mode);
            }
            return DataContract.GetDataContract(id, typeHandle, this.Mode);
        }

        internal virtual DataContract GetDataContract(RuntimeTypeHandle typeHandle, Type type)
        {
            if (this.IsGetOnlyCollection)
            {
                return DataContract.GetGetOnlyCollectionDataContract(DataContract.GetId(typeHandle), typeHandle, type, this.Mode);
            }
            return DataContract.GetDataContract(typeHandle, type, this.Mode);
        }

        private DataContract GetDataContractFromSerializerKnownTypes(XmlQualifiedName qname)
        {
            DataContract contract;
            Dictionary<XmlQualifiedName, DataContract> serializerKnownDataContracts = this.SerializerKnownDataContracts;
            if (serializerKnownDataContracts == null)
            {
                return null;
            }
            if (!serializerKnownDataContracts.TryGetValue(qname, out contract))
            {
                return null;
            }
            return contract;
        }

        internal static Dictionary<XmlQualifiedName, DataContract> GetDataContractsForKnownTypes(IList<Type> knownTypeList)
        {
            if (knownTypeList == null)
            {
                return null;
            }
            Dictionary<XmlQualifiedName, DataContract> nameToDataContractTable = new Dictionary<XmlQualifiedName, DataContract>();
            Dictionary<Type, Type> typesChecked = new Dictionary<Type, Type>();
            for (int i = 0; i < knownTypeList.Count; i++)
            {
                Type type = knownTypeList[i];
                if (type == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("NullKnownType", new object[] { "knownTypes" })));
                }
                DataContract.CheckAndAdd(type, typesChecked, ref nameToDataContractTable);
            }
            return nameToDataContractTable;
        }

        internal virtual DataContract GetDataContractSkipValidation(int typeId, RuntimeTypeHandle typeHandle, Type type)
        {
            if (this.IsGetOnlyCollection)
            {
                return DataContract.GetGetOnlyCollectionDataContractSkipValidation(typeId, typeHandle, type);
            }
            return DataContract.GetDataContractSkipValidation(typeId, typeHandle, type);
        }

        public StreamingContext GetStreamingContext()
        {
            return this.streamingContext;
        }

        internal virtual Type GetSurrogatedType(Type type)
        {
            return type;
        }

        public void IncrementItemCount(int count)
        {
            if (count > (this.maxItemsInObjectGraph - this.itemCount))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ExceededMaxItemsQuota", new object[] { this.maxItemsInObjectGraph })));
            }
            this.itemCount += count;
        }

        internal bool IsKnownType(DataContract dataContract, Type declaredType)
        {
            DataContract contract = this.ResolveDataContractFromKnownTypes(dataContract.StableName.Name, dataContract.StableName.Namespace, null, declaredType);
            return ((contract != null) && (contract.UnderlyingType == dataContract.UnderlyingType));
        }

        internal bool IsKnownType(DataContract dataContract, Dictionary<XmlQualifiedName, DataContract> knownDataContracts, Type declaredType)
        {
            bool flag = false;
            if (knownDataContracts != null)
            {
                this.scopedKnownTypes.Push(knownDataContracts);
                flag = true;
            }
            bool flag2 = this.IsKnownType(dataContract, declaredType);
            if (flag)
            {
                this.scopedKnownTypes.Pop();
            }
            return flag2;
        }

        private DataContract ResolveDataContractFromDataContractResolver(XmlQualifiedName typeName, Type declaredType)
        {
            Type type = this.DataContractResolver.ResolveName(typeName.Name, typeName.Namespace, declaredType, this.KnownTypeResolver);
            if (type == null)
            {
                return null;
            }
            return this.GetDataContract(type);
        }

        private DataContract ResolveDataContractFromKnownTypes(XmlQualifiedName typeName)
        {
            DataContract primitiveDataContract = PrimitiveDataContract.GetPrimitiveDataContract(typeName.Name, typeName.Namespace);
            if (primitiveDataContract == null)
            {
                if (((typeName.Name == "SafeSerializationManager") && (typeName.Namespace == "http://schemas.datacontract.org/2004/07/System.Runtime.Serialization")) && (Globals.TypeOfSafeSerializationManager != null))
                {
                    return this.GetDataContract(Globals.TypeOfSafeSerializationManager);
                }
                primitiveDataContract = this.scopedKnownTypes.GetDataContract(typeName);
                if (primitiveDataContract == null)
                {
                    primitiveDataContract = this.GetDataContractFromSerializerKnownTypes(typeName);
                }
            }
            return primitiveDataContract;
        }

        protected DataContract ResolveDataContractFromKnownTypes(string typeName, string typeNs, DataContract memberTypeContract, Type declaredType)
        {
            DataContract contract;
            XmlQualifiedName name = new XmlQualifiedName(typeName, typeNs);
            if (this.DataContractResolver == null)
            {
                contract = this.ResolveDataContractFromKnownTypes(name);
            }
            else
            {
                contract = this.ResolveDataContractFromDataContractResolver(name, declaredType);
            }
            if (contract == null)
            {
                DataContract dataContract;
                if (((memberTypeContract != null) && !memberTypeContract.UnderlyingType.IsInterface) && (memberTypeContract.StableName == name))
                {
                    contract = memberTypeContract;
                }
                if ((contract != null) || (this.rootTypeDataContract == null))
                {
                    return contract;
                }
                if (this.rootTypeDataContract.StableName == name)
                {
                    return this.rootTypeDataContract;
                }
                for (CollectionDataContract contract2 = this.rootTypeDataContract as CollectionDataContract; contract2 != null; contract2 = dataContract as CollectionDataContract)
                {
                    dataContract = this.GetDataContract(this.GetSurrogatedType(contract2.ItemType));
                    if (dataContract.StableName == name)
                    {
                        return dataContract;
                    }
                }
            }
            return contract;
        }

        internal Type ResolveNameFromKnownTypes(XmlQualifiedName typeName)
        {
            DataContract contract = this.ResolveDataContractFromKnownTypes(typeName);
            if (contract == null)
            {
                return null;
            }
            return contract.OriginalUnderlyingType;
        }

        protected System.Runtime.Serialization.DataContractResolver DataContractResolver
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dataContractResolver;
            }
        }

        internal bool IgnoreExtensionDataObject
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.ignoreExtensionDataObject;
            }
        }

        internal static MethodInfo IncrementItemCountMethod
        {
            get
            {
                if (incrementItemCountMethod == null)
                {
                    incrementItemCountMethod = typeof(XmlObjectSerializerContext).GetMethod("IncrementItemCount", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return incrementItemCountMethod;
            }
        }

        internal virtual bool IsGetOnlyCollection
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        protected KnownTypeDataContractResolver KnownTypeResolver
        {
            get
            {
                if (this.knownTypeResolver == null)
                {
                    this.knownTypeResolver = new KnownTypeDataContractResolver(this);
                }
                return this.knownTypeResolver;
            }
        }

        internal virtual SerializationMode Mode
        {
            get
            {
                return SerializationMode.SharedContract;
            }
        }

        internal int RemainingItemCount
        {
            get
            {
                return (this.maxItemsInObjectGraph - this.itemCount);
            }
        }

        private Dictionary<XmlQualifiedName, DataContract> SerializerKnownDataContracts
        {
            get
            {
                if (!this.isSerializerKnownDataContractsSetExplicit)
                {
                    this.serializerKnownDataContracts = this.serializer.KnownDataContracts;
                    this.isSerializerKnownDataContractsSetExplicit = true;
                }
                return this.serializerKnownDataContracts;
            }
        }
    }
}

