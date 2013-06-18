namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    internal class DataContractSet
    {
        private Dictionary<XmlQualifiedName, DataContract> contracts;
        private IDataContractSurrogate dataContractSurrogate;
        private Dictionary<XmlQualifiedName, DataContract> knownTypesForObject;
        private Dictionary<DataContract, object> processedContracts;
        private ICollection<Type> referencedCollectionTypes;
        private Dictionary<XmlQualifiedName, object> referencedCollectionTypesDictionary;
        private ICollection<Type> referencedTypes;
        private Dictionary<XmlQualifiedName, object> referencedTypesDictionary;
        private Hashtable surrogateDataTable;

        internal DataContractSet(DataContractSet dataContractSet)
        {
            if (dataContractSet == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("dataContractSet"));
            }
            this.dataContractSurrogate = dataContractSet.dataContractSurrogate;
            this.referencedTypes = dataContractSet.referencedTypes;
            this.referencedCollectionTypes = dataContractSet.referencedCollectionTypes;
            foreach (KeyValuePair<XmlQualifiedName, DataContract> pair in dataContractSet)
            {
                this.Add(pair.Key, pair.Value);
            }
            if (dataContractSet.processedContracts != null)
            {
                foreach (KeyValuePair<DataContract, object> pair2 in dataContractSet.processedContracts)
                {
                    this.ProcessedContracts.Add(pair2.Key, pair2.Value);
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DataContractSet(IDataContractSurrogate dataContractSurrogate) : this(dataContractSurrogate, null, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DataContractSet(IDataContractSurrogate dataContractSurrogate, ICollection<Type> referencedTypes, ICollection<Type> referencedCollectionTypes)
        {
            this.dataContractSurrogate = dataContractSurrogate;
            this.referencedTypes = referencedTypes;
            this.referencedCollectionTypes = referencedCollectionTypes;
        }

        private void Add(DataContract dataContract)
        {
            this.Add(dataContract.StableName, dataContract);
        }

        internal void Add(Type type)
        {
            DataContract dataContract = this.GetDataContract(type);
            EnsureTypeNotGeneric(dataContract.UnderlyingType);
            this.Add(dataContract);
        }

        public void Add(XmlQualifiedName name, DataContract dataContract)
        {
            if (!dataContract.IsBuiltInDataContract)
            {
                this.InternalAdd(name, dataContract);
            }
        }

        private void AddClassDataContract(ClassDataContract classDataContract)
        {
            if (classDataContract.BaseContract != null)
            {
                this.Add(classDataContract.BaseContract.StableName, classDataContract.BaseContract);
            }
            if (!classDataContract.IsISerializable && (classDataContract.Members != null))
            {
                for (int i = 0; i < classDataContract.Members.Count; i++)
                {
                    DataMember dataMember = classDataContract.Members[i];
                    DataContract memberTypeDataContract = this.GetMemberTypeDataContract(dataMember);
                    if ((this.dataContractSurrogate != null) && (dataMember.MemberInfo != null))
                    {
                        object obj2 = DataContractSurrogateCaller.GetCustomDataToExport(this.dataContractSurrogate, dataMember.MemberInfo, memberTypeDataContract.UnderlyingType);
                        if (obj2 != null)
                        {
                            this.SurrogateDataTable.Add(dataMember, obj2);
                        }
                    }
                    this.Add(memberTypeDataContract.StableName, memberTypeDataContract);
                }
            }
            this.AddKnownDataContracts(classDataContract.KnownDataContracts);
        }

        private void AddCollectionDataContract(CollectionDataContract collectionDataContract)
        {
            if (collectionDataContract.IsDictionary)
            {
                ClassDataContract itemContract = collectionDataContract.ItemContract as ClassDataContract;
                this.AddClassDataContract(itemContract);
            }
            else
            {
                DataContract itemTypeDataContract = this.GetItemTypeDataContract(collectionDataContract);
                if (itemTypeDataContract != null)
                {
                    this.Add(itemTypeDataContract.StableName, itemTypeDataContract);
                }
            }
            this.AddKnownDataContracts(collectionDataContract.KnownDataContracts);
        }

        private void AddKnownDataContracts(Dictionary<XmlQualifiedName, DataContract> knownDataContracts)
        {
            if (knownDataContracts != null)
            {
                foreach (DataContract contract in knownDataContracts.Values)
                {
                    this.Add(contract);
                }
            }
        }

        private void AddReferencedType(Dictionary<XmlQualifiedName, object> referencedTypes, Type type)
        {
            if (IsTypeReferenceable(type))
            {
                object obj2;
                XmlQualifiedName stableName = this.GetStableName(type);
                if (referencedTypes.TryGetValue(stableName, out obj2))
                {
                    Type type2 = obj2 as Type;
                    if (type2 == null)
                    {
                        List<Type> list2 = (List<Type>) obj2;
                        if (!list2.Contains(type))
                        {
                            list2.Add(type);
                        }
                    }
                    else if (type2 != type)
                    {
                        referencedTypes.Remove(stableName);
                        List<Type> list = new List<Type> {
                            type2,
                            type
                        };
                        referencedTypes.Add(stableName, list);
                    }
                }
                else
                {
                    referencedTypes.Add(stableName, type);
                }
            }
        }

        private void AddXmlDataContract(XmlDataContract xmlDataContract)
        {
            this.AddKnownDataContracts(xmlDataContract.KnownDataContracts);
        }

        internal static void EnsureTypeNotGeneric(Type type)
        {
            if (type.ContainsGenericParameters)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("GenericTypeNotExportable", new object[] { type })));
            }
        }

        internal ContractCodeDomInfo GetContractCodeDomInfo(DataContract dataContract)
        {
            object obj2;
            if (this.ProcessedContracts.TryGetValue(dataContract, out obj2))
            {
                return (ContractCodeDomInfo) obj2;
            }
            return null;
        }

        internal DataContract GetDataContract(Type clrType)
        {
            if (this.dataContractSurrogate == null)
            {
                return DataContract.GetDataContract(clrType);
            }
            DataContract builtInDataContract = DataContract.GetBuiltInDataContract(clrType);
            if (builtInDataContract == null)
            {
                Type dataContractType = DataContractSurrogateCaller.GetDataContractType(this.dataContractSurrogate, clrType);
                builtInDataContract = DataContract.GetDataContract(dataContractType);
                if (!this.SurrogateDataTable.Contains(builtInDataContract))
                {
                    object obj2 = DataContractSurrogateCaller.GetCustomDataToExport(this.dataContractSurrogate, clrType, dataContractType);
                    if (obj2 != null)
                    {
                        this.SurrogateDataTable.Add(builtInDataContract, obj2);
                    }
                }
            }
            return builtInDataContract;
        }

        public IEnumerator<KeyValuePair<XmlQualifiedName, DataContract>> GetEnumerator()
        {
            return this.Contracts.GetEnumerator();
        }

        internal DataContract GetItemTypeDataContract(CollectionDataContract collectionContract)
        {
            if (collectionContract.ItemType != null)
            {
                return this.GetDataContract(collectionContract.ItemType);
            }
            return collectionContract.ItemContract;
        }

        internal DataContract GetMemberTypeDataContract(DataMember dataMember)
        {
            if (dataMember.MemberInfo == null)
            {
                return dataMember.MemberTypeContract;
            }
            Type memberType = dataMember.MemberType;
            if (!dataMember.IsGetOnlyCollection)
            {
                return this.GetDataContract(memberType);
            }
            if ((this.dataContractSurrogate != null) && (DataContractSurrogateCaller.GetDataContractType(this.dataContractSurrogate, memberType) != memberType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("SurrogatesWithGetOnlyCollectionsNotSupported", new object[] { DataContract.GetClrTypeFullName(memberType), DataContract.GetClrTypeFullName(dataMember.MemberInfo.DeclaringType), dataMember.MemberInfo.Name })));
            }
            return DataContract.GetGetOnlyCollectionDataContract(DataContract.GetId(memberType.TypeHandle), memberType.TypeHandle, memberType, SerializationMode.SharedContract);
        }

        private Dictionary<XmlQualifiedName, object> GetReferencedCollectionTypes()
        {
            if (this.referencedCollectionTypesDictionary == null)
            {
                this.referencedCollectionTypesDictionary = new Dictionary<XmlQualifiedName, object>();
                if (this.referencedCollectionTypes != null)
                {
                    foreach (Type type in this.referencedCollectionTypes)
                    {
                        if (type == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("ReferencedCollectionTypesCannotContainNull")));
                        }
                        this.AddReferencedType(this.referencedCollectionTypesDictionary, type);
                    }
                }
                XmlQualifiedName stableName = DataContract.GetStableName(Globals.TypeOfDictionaryGeneric);
                if (!this.referencedCollectionTypesDictionary.ContainsKey(stableName) && this.GetReferencedTypes().ContainsKey(stableName))
                {
                    this.AddReferencedType(this.referencedCollectionTypesDictionary, Globals.TypeOfDictionaryGeneric);
                }
            }
            return this.referencedCollectionTypesDictionary;
        }

        private Dictionary<XmlQualifiedName, object> GetReferencedTypes()
        {
            if (this.referencedTypesDictionary == null)
            {
                this.referencedTypesDictionary = new Dictionary<XmlQualifiedName, object>();
                this.referencedTypesDictionary.Add(DataContract.GetStableName(Globals.TypeOfNullable), Globals.TypeOfNullable);
                if (this.referencedTypes != null)
                {
                    foreach (Type type in this.referencedTypes)
                    {
                        if (type == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("ReferencedTypesCannotContainNull")));
                        }
                        this.AddReferencedType(this.referencedTypesDictionary, type);
                    }
                }
            }
            return this.referencedTypesDictionary;
        }

        internal XmlQualifiedName GetStableName(Type clrType)
        {
            if (this.dataContractSurrogate != null)
            {
                return DataContract.GetStableName(DataContractSurrogateCaller.GetDataContractType(this.dataContractSurrogate, clrType));
            }
            return DataContract.GetStableName(clrType);
        }

        internal object GetSurrogateData(object key)
        {
            return this.SurrogateDataTable[key];
        }

        internal void InternalAdd(XmlQualifiedName name, DataContract dataContract)
        {
            DataContract contract = null;
            if (this.Contracts.TryGetValue(name, out contract))
            {
                if (!contract.Equals(dataContract))
                {
                    if ((dataContract.UnderlyingType == null) || (contract.UnderlyingType == null))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("DupContractInDataContractSet", new object[] { dataContract.StableName.Name, dataContract.StableName.Namespace })));
                    }
                    bool flag = DataContract.GetClrTypeFullName(dataContract.UnderlyingType) == DataContract.GetClrTypeFullName(contract.UnderlyingType);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("DupTypeContractInDataContractSet", new object[] { flag ? dataContract.UnderlyingType.AssemblyQualifiedName : DataContract.GetClrTypeFullName(dataContract.UnderlyingType), flag ? contract.UnderlyingType.AssemblyQualifiedName : DataContract.GetClrTypeFullName(contract.UnderlyingType), dataContract.StableName.Name, dataContract.StableName.Namespace })));
                }
            }
            else
            {
                this.Contracts.Add(name, dataContract);
                if (dataContract is ClassDataContract)
                {
                    this.AddClassDataContract((ClassDataContract) dataContract);
                }
                else if (dataContract is CollectionDataContract)
                {
                    this.AddCollectionDataContract((CollectionDataContract) dataContract);
                }
                else if (dataContract is XmlDataContract)
                {
                    this.AddXmlDataContract((XmlDataContract) dataContract);
                }
            }
        }

        internal bool IsContractProcessed(DataContract dataContract)
        {
            return this.ProcessedContracts.ContainsKey(dataContract);
        }

        private static bool IsTypeReferenceable(Type type)
        {
            Type type2;
            if (((!type.IsSerializable && !type.IsDefined(Globals.TypeOfDataContractAttribute, false)) && (!Globals.TypeOfIXmlSerializable.IsAssignableFrom(type) || type.IsGenericTypeDefinition)) && !CollectionDataContract.IsCollection(type, out type2))
            {
                return ClassDataContract.IsNonAttributedTypeValidForSerialization(type);
            }
            return true;
        }

        public bool Remove(XmlQualifiedName key)
        {
            if (DataContract.GetBuiltInDataContract(key.Name, key.Namespace) != null)
            {
                return false;
            }
            return this.Contracts.Remove(key);
        }

        internal void SetContractCodeDomInfo(DataContract dataContract, ContractCodeDomInfo info)
        {
            this.ProcessedContracts.Add(dataContract, info);
        }

        internal void SetContractProcessed(DataContract dataContract)
        {
            this.ProcessedContracts.Add(dataContract, dataContract);
        }

        internal void SetSurrogateData(object key, object surrogateData)
        {
            this.SurrogateDataTable[key] = surrogateData;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal bool TryGetReferencedCollectionType(XmlQualifiedName stableName, DataContract dataContract, out Type type)
        {
            return this.TryGetReferencedType(stableName, dataContract, true, out type);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal bool TryGetReferencedType(XmlQualifiedName stableName, DataContract dataContract, out Type type)
        {
            return this.TryGetReferencedType(stableName, dataContract, false, out type);
        }

        private bool TryGetReferencedType(XmlQualifiedName stableName, DataContract dataContract, bool useReferencedCollectionTypes, out Type type)
        {
            object obj2;
            Dictionary<XmlQualifiedName, object> dictionary = useReferencedCollectionTypes ? this.GetReferencedCollectionTypes() : this.GetReferencedTypes();
            if (dictionary.TryGetValue(stableName, out obj2))
            {
                type = obj2 as Type;
                if (type != null)
                {
                    return true;
                }
                List<Type> list = (List<Type>) obj2;
                StringBuilder builder = new StringBuilder();
                bool isGenericTypeDefinition = false;
                for (int i = 0; i < list.Count; i++)
                {
                    Type clrType = list[i];
                    if (!isGenericTypeDefinition)
                    {
                        isGenericTypeDefinition = clrType.IsGenericTypeDefinition;
                    }
                    builder.AppendFormat("{0}\"{1}\" ", Environment.NewLine, clrType.AssemblyQualifiedName);
                    if (dataContract != null)
                    {
                        DataContract contract = this.GetDataContract(clrType);
                        builder.Append(System.Runtime.Serialization.SR.GetString(((contract != null) && contract.Equals(dataContract)) ? "ReferencedTypeMatchingMessage" : "ReferencedTypeNotMatchingMessage"));
                    }
                }
                if (isGenericTypeDefinition)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString(useReferencedCollectionTypes ? "AmbiguousReferencedCollectionTypes1" : "AmbiguousReferencedTypes1", new object[] { builder.ToString() })));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString(useReferencedCollectionTypes ? "AmbiguousReferencedCollectionTypes3" : "AmbiguousReferencedTypes3", new object[] { XmlConvert.DecodeName(stableName.Name), stableName.Namespace, builder.ToString() })));
            }
            type = null;
            return false;
        }

        private Dictionary<XmlQualifiedName, DataContract> Contracts
        {
            get
            {
                if (this.contracts == null)
                {
                    this.contracts = new Dictionary<XmlQualifiedName, DataContract>();
                }
                return this.contracts;
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

        public DataContract this[XmlQualifiedName key]
        {
            get
            {
                DataContract builtInDataContract = DataContract.GetBuiltInDataContract(key.Name, key.Namespace);
                if (builtInDataContract == null)
                {
                    this.Contracts.TryGetValue(key, out builtInDataContract);
                }
                return builtInDataContract;
            }
        }

        internal Dictionary<XmlQualifiedName, DataContract> KnownTypesForObject
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.knownTypesForObject;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.knownTypesForObject = value;
            }
        }

        private Dictionary<DataContract, object> ProcessedContracts
        {
            get
            {
                if (this.processedContracts == null)
                {
                    this.processedContracts = new Dictionary<DataContract, object>();
                }
                return this.processedContracts;
            }
        }

        private Hashtable SurrogateDataTable
        {
            get
            {
                if (this.surrogateDataTable == null)
                {
                    this.surrogateDataTable = new Hashtable();
                }
                return this.surrogateDataTable;
            }
        }
    }
}

