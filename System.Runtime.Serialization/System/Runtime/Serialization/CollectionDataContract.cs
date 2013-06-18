namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using System.Xml;

    internal sealed class CollectionDataContract : DataContract
    {
        [SecurityCritical]
        private XmlDictionaryString childElementNamespace;
        [SecurityCritical]
        private XmlDictionaryString collectionItemName;
        [SecurityCritical]
        private CollectionDataContractCriticalHelper helper;
        [SecurityCritical]
        private DataContract itemContract;

        [SecuritySafeCritical]
        internal CollectionDataContract(CollectionKind kind) : base(new CollectionDataContractCriticalHelper(kind))
        {
            this.InitCollectionDataContract(this);
        }

        [SecuritySafeCritical]
        internal CollectionDataContract(Type type) : base(new CollectionDataContractCriticalHelper(type))
        {
            this.InitCollectionDataContract(this);
        }

        [SecuritySafeCritical]
        internal CollectionDataContract(Type type, DataContract itemContract) : base(new CollectionDataContractCriticalHelper(type, itemContract))
        {
            this.InitCollectionDataContract(this);
        }

        [SecuritySafeCritical]
        private CollectionDataContract(Type type, string invalidCollectionInSharedContractMessage) : base(new CollectionDataContractCriticalHelper(type, invalidCollectionInSharedContractMessage))
        {
            this.InitCollectionDataContract(this.GetSharedTypeContract(type));
        }

        [SecuritySafeCritical]
        private CollectionDataContract(Type type, CollectionKind kind, Type itemType, MethodInfo getEnumeratorMethod, MethodInfo addMethod, ConstructorInfo constructor) : base(new CollectionDataContractCriticalHelper(type, kind, itemType, getEnumeratorMethod, addMethod, constructor))
        {
            this.InitCollectionDataContract(this.GetSharedTypeContract(type));
        }

        [SecuritySafeCritical]
        private CollectionDataContract(Type type, CollectionKind kind, Type itemType, MethodInfo getEnumeratorMethod, MethodInfo addMethod, ConstructorInfo constructor, bool isConstructorCheckRequired) : base(new CollectionDataContractCriticalHelper(type, kind, itemType, getEnumeratorMethod, addMethod, constructor, isConstructorCheckRequired))
        {
            this.InitCollectionDataContract(this.GetSharedTypeContract(type));
        }

        [SecuritySafeCritical]
        internal override DataContract BindGenericParameters(DataContract[] paramContracts, Dictionary<DataContract, DataContract> boundContracts)
        {
            DataContract contract;
            if (boundContracts.TryGetValue(this, out contract))
            {
                return contract;
            }
            CollectionDataContract contract2 = new CollectionDataContract(this.Kind);
            boundContracts.Add(this, contract2);
            contract2.ItemContract = this.ItemContract.BindGenericParameters(paramContracts, boundContracts);
            contract2.IsItemTypeNullable = !contract2.ItemContract.IsValueType;
            contract2.ItemName = this.ItemNameSetExplicit ? this.ItemName : contract2.ItemContract.StableName.Name;
            contract2.KeyName = this.KeyName;
            contract2.ValueName = this.ValueName;
            contract2.StableName = DataContract.CreateQualifiedName(DataContract.ExpandGenericParameters(XmlConvert.DecodeName(base.StableName.Name), new GenericNameProvider(DataContract.GetClrTypeFullName(base.UnderlyingType), paramContracts)), IsCollectionDataContract(base.UnderlyingType) ? base.StableName.Namespace : DataContract.GetCollectionNamespace(contract2.ItemContract.StableName.Namespace));
            return contract2;
        }

        [SecuritySafeCritical]
        private void CheckConstructor()
        {
            if (this.Constructor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("CollectionTypeDoesNotHaveDefaultCtor", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType) })));
            }
            this.IsConstructorCheckRequired = false;
        }

        internal static bool CreateGetOnlyCollectionDataContract(Type type, out DataContract dataContract)
        {
            Type type2;
            if (type.IsArray)
            {
                dataContract = new CollectionDataContract(type);
                return true;
            }
            return IsCollectionOrTryCreate(type, true, out dataContract, out type2, false);
        }

        internal override bool Equals(object other, Dictionary<DataContractPairKey, object> checkedContracts)
        {
            if (base.IsEqualOrChecked(other, checkedContracts))
            {
                return true;
            }
            if (base.Equals(other, checkedContracts))
            {
                CollectionDataContract contract = other as CollectionDataContract;
                if (contract != null)
                {
                    bool flag = (this.ItemContract != null) && !this.ItemContract.IsValueType;
                    bool flag2 = (contract.ItemContract != null) && !contract.ItemContract.IsValueType;
                    return (((this.ItemName == contract.ItemName) && ((this.IsItemTypeNullable || flag) == (contract.IsItemTypeNullable || flag2))) && this.ItemContract.Equals(contract.ItemContract, checkedContracts));
                }
            }
            return false;
        }

        private static void FindCollectionMethodsOnInterface(Type type, Type interfaceType, ref MethodInfo addMethod, ref MethodInfo getEnumeratorMethod)
        {
            InterfaceMapping interfaceMap = type.GetInterfaceMap(interfaceType);
            for (int i = 0; i < interfaceMap.TargetMethods.Length; i++)
            {
                if (interfaceMap.InterfaceMethods[i].Name == "Add")
                {
                    addMethod = interfaceMap.InterfaceMethods[i];
                }
                else if (interfaceMap.InterfaceMethods[i].Name == "GetEnumerator")
                {
                    getEnumeratorMethod = interfaceMap.InterfaceMethods[i];
                }
            }
        }

        private static void GetCollectionMethods(Type type, Type interfaceType, Type[] addMethodTypeArray, bool addMethodOnInterface, out MethodInfo getEnumeratorMethod, out MethodInfo addMethod)
        {
            addMethod = (MethodInfo) (getEnumeratorMethod = null);
            if (addMethodOnInterface)
            {
                addMethod = type.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null, addMethodTypeArray, null);
                if ((addMethod == null) || (addMethod.GetParameters()[0].ParameterType != addMethodTypeArray[0]))
                {
                    FindCollectionMethodsOnInterface(type, interfaceType, ref addMethod, ref getEnumeratorMethod);
                    if (addMethod == null)
                    {
                        foreach (Type type2 in interfaceType.GetInterfaces())
                        {
                            if (IsKnownInterface(type2))
                            {
                                FindCollectionMethodsOnInterface(type, type2, ref addMethod, ref getEnumeratorMethod);
                                if (addMethod == null)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                addMethod = type.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, addMethodTypeArray, null);
                if (addMethod == null)
                {
                    return;
                }
            }
            if (getEnumeratorMethod == null)
            {
                getEnumeratorMethod = type.GetMethod("GetEnumerator", BindingFlags.Public | BindingFlags.Instance, null, Globals.EmptyTypeArray, null);
                if ((getEnumeratorMethod == null) || !Globals.TypeOfIEnumerator.IsAssignableFrom(getEnumeratorMethod.ReturnType))
                {
                    Type typeOfIEnumerable = interfaceType.GetInterface("System.Collections.Generic.IEnumerable*");
                    if (typeOfIEnumerable == null)
                    {
                        typeOfIEnumerable = Globals.TypeOfIEnumerable;
                    }
                    getEnumeratorMethod = GetTargetMethodWithName("GetEnumerator", type, typeOfIEnumerable);
                }
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private static string GetInvalidCollectionMessage(string message, string nestedMessage, string param)
        {
            if (param != null)
            {
                return System.Runtime.Serialization.SR.GetString(message, new object[] { nestedMessage, param });
            }
            return System.Runtime.Serialization.SR.GetString(message, new object[] { nestedMessage });
        }

        private DataContract GetSharedTypeContract(Type type)
        {
            if (type.IsDefined(Globals.TypeOfCollectionDataContractAttribute, false))
            {
                return this;
            }
            if (!type.IsSerializable && !type.IsDefined(Globals.TypeOfDataContractAttribute, false))
            {
                return null;
            }
            return new ClassDataContract(type);
        }

        internal static MethodInfo GetTargetMethodWithName(string name, Type type, Type interfaceType)
        {
            InterfaceMapping interfaceMap = type.GetInterfaceMap(interfaceType);
            for (int i = 0; i < interfaceMap.TargetMethods.Length; i++)
            {
                if (interfaceMap.InterfaceMethods[i].Name == name)
                {
                    return interfaceMap.InterfaceMethods[i];
                }
            }
            return null;
        }

        internal override DataContract GetValidContract()
        {
            if (this.IsConstructorCheckRequired)
            {
                this.CheckConstructor();
            }
            return this;
        }

        internal override DataContract GetValidContract(SerializationMode mode)
        {
            if (mode == SerializationMode.SharedType)
            {
                if (this.SharedTypeContract == null)
                {
                    DataContract.ThrowTypeNotSerializable(base.UnderlyingType);
                }
                return this.SharedTypeContract;
            }
            this.ThrowIfInvalid();
            return this;
        }

        private static bool HandleIfInvalidCollection(Type type, bool tryCreate, bool hasCollectionDataContract, bool createContractWithException, string message, string param, ref DataContract dataContract)
        {
            if (hasCollectionDataContract)
            {
                if (tryCreate)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(GetInvalidCollectionMessage(message, System.Runtime.Serialization.SR.GetString("InvalidCollectionDataContract", new object[] { DataContract.GetClrTypeFullName(type) }), param)));
                }
                return true;
            }
            if (!createContractWithException)
            {
                return false;
            }
            if (tryCreate)
            {
                dataContract = new CollectionDataContract(type, GetInvalidCollectionMessage(message, System.Runtime.Serialization.SR.GetString("InvalidCollectionType", new object[] { DataContract.GetClrTypeFullName(type) }), param));
            }
            return true;
        }

        [SecurityCritical]
        private void InitCollectionDataContract(DataContract sharedTypeContract)
        {
            this.helper = base.Helper as CollectionDataContractCriticalHelper;
            this.collectionItemName = this.helper.CollectionItemName;
            if ((this.helper.Kind == CollectionKind.Dictionary) || (this.helper.Kind == CollectionKind.GenericDictionary))
            {
                this.itemContract = this.helper.ItemContract;
            }
            this.helper.SharedTypeContract = sharedTypeContract;
        }

        private void InitSharedTypeContract()
        {
        }

        internal static bool IsCollection(Type type)
        {
            Type type2;
            return IsCollection(type, out type2);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static bool IsCollection(Type type, out Type itemType)
        {
            return IsCollectionHelper(type, out itemType, true);
        }

        internal static bool IsCollection(Type type, bool constructorRequired)
        {
            Type type2;
            return IsCollectionHelper(type, out type2, constructorRequired);
        }

        internal static bool IsCollectionDataContract(Type type)
        {
            return type.IsDefined(Globals.TypeOfCollectionDataContractAttribute, false);
        }

        private static bool IsCollectionHelper(Type type, out Type itemType, bool constructorRequired)
        {
            DataContract contract;
            if (type.IsArray && (DataContract.GetBuiltInDataContract(type) == null))
            {
                itemType = type.GetElementType();
                return true;
            }
            return IsCollectionOrTryCreate(type, false, out contract, out itemType, constructorRequired);
        }

        internal static bool IsCollectionInterface(Type type)
        {
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }
            return ((IList<Type>) KnownInterfaces).Contains(type);
        }

        private static bool IsCollectionOrTryCreate(Type type, bool tryCreate, out DataContract dataContract, out Type itemType, bool constructorRequired)
        {
            MethodInfo method;
            MethodInfo info2;
            Type[] typeArray5;
            dataContract = null;
            itemType = Globals.TypeOfObject;
            if (DataContract.GetBuiltInDataContract(type) != null)
            {
                return HandleIfInvalidCollection(type, tryCreate, false, false, "CollectionTypeCannotBeBuiltIn", null, ref dataContract);
            }
            bool hasCollectionDataContract = IsCollectionDataContract(type);
            Type baseType = type.BaseType;
            bool createContractWithException = (((baseType != null) && (baseType != Globals.TypeOfObject)) && ((baseType != Globals.TypeOfValueType) && (baseType != Globals.TypeOfUri))) ? IsCollection(baseType) : false;
            if (type.IsDefined(Globals.TypeOfDataContractAttribute, false))
            {
                return HandleIfInvalidCollection(type, tryCreate, hasCollectionDataContract, createContractWithException, "CollectionTypeCannotHaveDataContract", null, ref dataContract);
            }
            if (Globals.TypeOfIXmlSerializable.IsAssignableFrom(type))
            {
                return false;
            }
            if (!Globals.TypeOfIEnumerable.IsAssignableFrom(type))
            {
                return HandleIfInvalidCollection(type, tryCreate, hasCollectionDataContract, createContractWithException, "CollectionTypeIsNotIEnumerable", null, ref dataContract);
            }
            if (type.IsInterface)
            {
                Type type3 = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                Type[] knownInterfaces = KnownInterfaces;
                for (int i = 0; i < knownInterfaces.Length; i++)
                {
                    if (knownInterfaces[i] == type3)
                    {
                        method = null;
                        if (type.IsGenericType)
                        {
                            Type[] genericArguments = type.GetGenericArguments();
                            if (type3 == Globals.TypeOfIDictionaryGeneric)
                            {
                                itemType = Globals.TypeOfKeyValue.MakeGenericType(genericArguments);
                                method = type.GetMethod("Add");
                                info2 = Globals.TypeOfIEnumerableGeneric.MakeGenericType(new Type[] { Globals.TypeOfKeyValuePair.MakeGenericType(genericArguments) }).GetMethod("GetEnumerator");
                            }
                            else
                            {
                                itemType = genericArguments[0];
                                if ((type3 == Globals.TypeOfICollectionGeneric) || (type3 == Globals.TypeOfIListGeneric))
                                {
                                    method = Globals.TypeOfICollectionGeneric.MakeGenericType(new Type[] { itemType }).GetMethod("Add");
                                }
                                info2 = Globals.TypeOfIEnumerableGeneric.MakeGenericType(new Type[] { itemType }).GetMethod("GetEnumerator");
                            }
                        }
                        else
                        {
                            if (type3 == Globals.TypeOfIDictionary)
                            {
                                itemType = typeof(KeyValue<object, object>);
                                method = type.GetMethod("Add");
                            }
                            else
                            {
                                itemType = Globals.TypeOfObject;
                                if (type3 == Globals.TypeOfIList)
                                {
                                    method = Globals.TypeOfIList.GetMethod("Add");
                                }
                            }
                            info2 = Globals.TypeOfIEnumerable.GetMethod("GetEnumerator");
                        }
                        if (tryCreate)
                        {
                            dataContract = new CollectionDataContract(type, (CollectionKind) ((byte) (i + 1)), itemType, info2, method, null);
                        }
                        return true;
                    }
                }
            }
            ConstructorInfo constructor = null;
            if (!type.IsValueType)
            {
                constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Globals.EmptyTypeArray, null);
                if ((constructor == null) && constructorRequired)
                {
                    return HandleIfInvalidCollection(type, tryCreate, hasCollectionDataContract, createContractWithException, "CollectionTypeDoesNotHaveDefaultCtor", null, ref dataContract);
                }
            }
            Type interfaceType = null;
            CollectionKind none = CollectionKind.None;
            bool flag3 = false;
            foreach (Type type5 in type.GetInterfaces())
            {
                Type type6 = type5.IsGenericType ? type5.GetGenericTypeDefinition() : type5;
                Type[] typeArray4 = KnownInterfaces;
                for (int j = 0; j < typeArray4.Length; j++)
                {
                    if (typeArray4[j] == type6)
                    {
                        CollectionKind kind2 = (CollectionKind) ((byte) (j + 1));
                        if ((none == CollectionKind.None) || (kind2 < none))
                        {
                            none = kind2;
                            interfaceType = type5;
                            flag3 = false;
                        }
                        else if (((byte) (none & kind2)) == kind2)
                        {
                            flag3 = true;
                        }
                        break;
                    }
                }
            }
            switch (none)
            {
                case CollectionKind.None:
                    return HandleIfInvalidCollection(type, tryCreate, hasCollectionDataContract, createContractWithException, "CollectionTypeIsNotIEnumerable", null, ref dataContract);

                case CollectionKind.Enumerable:
                case CollectionKind.Collection:
                case CollectionKind.GenericEnumerable:
                    if (flag3)
                    {
                        interfaceType = Globals.TypeOfIEnumerable;
                    }
                    itemType = interfaceType.IsGenericType ? interfaceType.GetGenericArguments()[0] : Globals.TypeOfObject;
                    GetCollectionMethods(type, interfaceType, new Type[] { itemType }, false, out info2, out method);
                    if (method == null)
                    {
                        return HandleIfInvalidCollection(type, tryCreate, hasCollectionDataContract, createContractWithException, "CollectionTypeDoesNotHaveAddMethod", DataContract.GetClrTypeFullName(itemType), ref dataContract);
                    }
                    if (tryCreate)
                    {
                        dataContract = new CollectionDataContract(type, none, itemType, info2, method, constructor, !constructorRequired);
                    }
                    goto Label_04F3;

                default:
                    if (flag3)
                    {
                        return HandleIfInvalidCollection(type, tryCreate, hasCollectionDataContract, createContractWithException, "CollectionTypeHasMultipleDefinitionsOfInterface", KnownInterfaces[((int) none) - 1].Name, ref dataContract);
                    }
                    typeArray5 = null;
                    switch (none)
                    {
                        case CollectionKind.GenericDictionary:
                        {
                            typeArray5 = interfaceType.GetGenericArguments();
                            bool flag4 = interfaceType.IsGenericTypeDefinition || (typeArray5[0].IsGenericParameter && typeArray5[1].IsGenericParameter);
                            itemType = flag4 ? Globals.TypeOfKeyValue : Globals.TypeOfKeyValue.MakeGenericType(typeArray5);
                            goto Label_04CC;
                        }
                        case CollectionKind.Dictionary:
                            typeArray5 = new Type[] { Globals.TypeOfObject, Globals.TypeOfObject };
                            itemType = Globals.TypeOfKeyValue.MakeGenericType(typeArray5);
                            goto Label_04CC;

                        case CollectionKind.GenericList:
                        case CollectionKind.GenericCollection:
                            typeArray5 = interfaceType.GetGenericArguments();
                            itemType = typeArray5[0];
                            goto Label_04CC;

                        case CollectionKind.List:
                            itemType = Globals.TypeOfObject;
                            typeArray5 = new Type[] { itemType };
                            goto Label_04CC;
                    }
                    break;
            }
        Label_04CC:
            if (tryCreate)
            {
                GetCollectionMethods(type, interfaceType, typeArray5, true, out info2, out method);
                dataContract = new CollectionDataContract(type, none, itemType, info2, method, constructor, !constructorRequired);
            }
        Label_04F3:
            return true;
        }

        private static bool IsKnownInterface(Type type)
        {
            Type type2 = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            foreach (Type type3 in KnownInterfaces)
            {
                if (type2 == type3)
                {
                    return true;
                }
            }
            return false;
        }

        internal override bool IsValidContract(SerializationMode mode)
        {
            if (mode == SerializationMode.SharedType)
            {
                return (this.SharedTypeContract != null);
            }
            return (this.InvalidCollectionInSharedContractMessage == null);
        }

        public override object ReadXmlValue(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context)
        {
            xmlReader.Read();
            object obj2 = null;
            if (context.IsGetOnlyCollection)
            {
                context.IsGetOnlyCollection = false;
                this.XmlFormatGetOnlyCollectionReaderDelegate(xmlReader, context, this.CollectionItemName, this.Namespace, this);
            }
            else
            {
                obj2 = this.XmlFormatReaderDelegate(xmlReader, context, this.CollectionItemName, this.Namespace, this);
            }
            xmlReader.ReadEndElement();
            return obj2;
        }

        internal bool RequiresMemberAccessForRead(SecurityException securityException)
        {
            if (!DataContract.IsTypeVisible(base.UnderlyingType))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustCollectionContractTypeNotPublic", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType) }), securityException));
                }
                return true;
            }
            if ((this.ItemType != null) && !DataContract.IsTypeVisible(this.ItemType))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustCollectionContractTypeNotPublic", new object[] { DataContract.GetClrTypeFullName(this.ItemType) }), securityException));
                }
                return true;
            }
            if (DataContract.ConstructorRequiresMemberAccess(this.Constructor))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustCollectionContractNoPublicConstructor", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType) }), securityException));
                }
                return true;
            }
            if (!DataContract.MethodRequiresMemberAccess(this.AddMethod))
            {
                return false;
            }
            if (securityException != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustCollectionContractAddMethodNotPublic", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType), this.AddMethod.Name }), securityException));
            }
            return true;
        }

        internal bool RequiresMemberAccessForWrite(SecurityException securityException)
        {
            if (!DataContract.IsTypeVisible(base.UnderlyingType))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustCollectionContractTypeNotPublic", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType) }), securityException));
                }
                return true;
            }
            if ((this.ItemType == null) || DataContract.IsTypeVisible(this.ItemType))
            {
                return false;
            }
            if (securityException != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustCollectionContractTypeNotPublic", new object[] { DataContract.GetClrTypeFullName(this.ItemType) }), securityException));
            }
            return true;
        }

        private void ThrowIfInvalid()
        {
            if (this.InvalidCollectionInSharedContractMessage != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(this.InvalidCollectionInSharedContractMessage));
            }
        }

        internal static bool TryCreate(Type type, out DataContract dataContract)
        {
            Type type2;
            return IsCollectionOrTryCreate(type, true, out dataContract, out type2, true);
        }

        public override void WriteXmlValue(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context)
        {
            context.IsGetOnlyCollection = false;
            this.XmlFormatWriterDelegate(xmlWriter, obj, context, this);
        }

        internal MethodInfo AddMethod
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.AddMethod;
            }
        }

        public XmlDictionaryString ChildElementNamespace
        {
            [SecuritySafeCritical]
            get
            {
                if (this.childElementNamespace == null)
                {
                    lock (this)
                    {
                        if (this.childElementNamespace == null)
                        {
                            if ((this.helper.ChildElementNamespace == null) && !this.IsDictionary)
                            {
                                XmlDictionaryString str = ClassDataContract.GetChildNamespaceToDeclare(this, this.ItemType, new XmlDictionary());
                                Thread.MemoryBarrier();
                                this.helper.ChildElementNamespace = str;
                            }
                            this.childElementNamespace = this.helper.ChildElementNamespace;
                        }
                    }
                }
                return this.childElementNamespace;
            }
        }

        public XmlDictionaryString CollectionItemName
        {
            [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.collectionItemName;
            }
        }

        internal ConstructorInfo Constructor
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.Constructor;
            }
        }

        internal MethodInfo GetEnumeratorMethod
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.GetEnumeratorMethod;
            }
        }

        internal string InvalidCollectionInSharedContractMessage
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.InvalidCollectionInSharedContractMessage;
            }
        }

        internal bool IsConstructorCheckRequired
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.IsConstructorCheckRequired;
            }
            [SecurityCritical]
            set
            {
                this.helper.IsConstructorCheckRequired = value;
            }
        }

        internal bool IsDictionary
        {
            get
            {
                return (this.KeyName != null);
            }
        }

        internal bool IsItemTypeNullable
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.IsItemTypeNullable;
            }
            [SecurityCritical]
            set
            {
                this.helper.IsItemTypeNullable = value;
            }
        }

        public DataContract ItemContract
        {
            [SecuritySafeCritical]
            get
            {
                return (this.itemContract ?? this.helper.ItemContract);
            }
            [SecurityCritical]
            set
            {
                this.itemContract = value;
                this.helper.ItemContract = value;
            }
        }

        internal string ItemName
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.ItemName;
            }
            [SecurityCritical]
            set
            {
                this.helper.ItemName = value;
            }
        }

        private bool ItemNameSetExplicit
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.ItemNameSetExplicit;
            }
        }

        internal Type ItemType
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.ItemType;
            }
        }

        internal string KeyName
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.KeyName;
            }
            [SecurityCritical]
            set
            {
                this.helper.KeyName = value;
            }
        }

        internal CollectionKind Kind
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.Kind;
            }
        }

        internal override Dictionary<XmlQualifiedName, DataContract> KnownDataContracts
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.KnownDataContracts;
            }
            [SecurityCritical]
            set
            {
                this.helper.KnownDataContracts = value;
            }
        }

        private static Type[] KnownInterfaces
        {
            [SecuritySafeCritical]
            get
            {
                return CollectionDataContractCriticalHelper.KnownInterfaces;
            }
        }

        internal DataContract SharedTypeContract
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.SharedTypeContract;
            }
        }

        internal string ValueName
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.ValueName;
            }
            [SecurityCritical]
            set
            {
                this.helper.ValueName = value;
            }
        }

        internal System.Runtime.Serialization.XmlFormatGetOnlyCollectionReaderDelegate XmlFormatGetOnlyCollectionReaderDelegate
        {
            [SecuritySafeCritical]
            get
            {
                if (this.helper.XmlFormatGetOnlyCollectionReaderDelegate == null)
                {
                    lock (this)
                    {
                        if (this.helper.XmlFormatGetOnlyCollectionReaderDelegate == null)
                        {
                            if (base.OriginalUnderlyingType.IsInterface && (((this.Kind == CollectionKind.Enumerable) || (this.Kind == CollectionKind.Collection)) || (this.Kind == CollectionKind.GenericEnumerable)))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("GetOnlyCollectionMustHaveAddMethod", new object[] { DataContract.GetClrTypeFullName(base.OriginalUnderlyingType) })));
                            }
                            System.Runtime.Serialization.XmlFormatGetOnlyCollectionReaderDelegate delegate2 = new XmlFormatReaderGenerator().GenerateGetOnlyCollectionReader(this);
                            Thread.MemoryBarrier();
                            this.helper.XmlFormatGetOnlyCollectionReaderDelegate = delegate2;
                        }
                    }
                }
                return this.helper.XmlFormatGetOnlyCollectionReaderDelegate;
            }
        }

        internal XmlFormatCollectionReaderDelegate XmlFormatReaderDelegate
        {
            [SecuritySafeCritical]
            get
            {
                if (this.helper.XmlFormatReaderDelegate == null)
                {
                    lock (this)
                    {
                        if (this.helper.XmlFormatReaderDelegate == null)
                        {
                            XmlFormatCollectionReaderDelegate delegate2 = new XmlFormatReaderGenerator().GenerateCollectionReader(this);
                            Thread.MemoryBarrier();
                            this.helper.XmlFormatReaderDelegate = delegate2;
                        }
                    }
                }
                return this.helper.XmlFormatReaderDelegate;
            }
        }

        internal XmlFormatCollectionWriterDelegate XmlFormatWriterDelegate
        {
            [SecuritySafeCritical]
            get
            {
                if (this.helper.XmlFormatWriterDelegate == null)
                {
                    lock (this)
                    {
                        if (this.helper.XmlFormatWriterDelegate == null)
                        {
                            XmlFormatCollectionWriterDelegate delegate2 = new XmlFormatWriterGenerator().GenerateCollectionWriter(this);
                            Thread.MemoryBarrier();
                            this.helper.XmlFormatWriterDelegate = delegate2;
                        }
                    }
                }
                return this.helper.XmlFormatWriterDelegate;
            }
        }

        [SecurityCritical(SecurityCriticalScope.Everything)]
        private class CollectionDataContractCriticalHelper : DataContract.DataContractCriticalHelper
        {
            private static Type[] _knownInterfaces;
            private readonly MethodInfo addMethod;
            private XmlDictionaryString childElementNamespace;
            private XmlDictionaryString collectionItemName;
            private readonly ConstructorInfo constructor;
            private readonly MethodInfo getEnumeratorMethod;
            private string invalidCollectionInSharedContractMessage;
            private bool isConstructorCheckRequired;
            private bool isItemTypeNullable;
            private bool isKnownTypeAttributeChecked;
            private DataContract itemContract;
            private string itemName;
            private bool itemNameSetExplicit;
            private Type itemType;
            private string keyName;
            private CollectionKind kind;
            private Dictionary<XmlQualifiedName, DataContract> knownDataContracts;
            private DataContract sharedTypeContract;
            private string valueName;
            private System.Runtime.Serialization.XmlFormatGetOnlyCollectionReaderDelegate xmlFormatGetOnlyCollectionReaderDelegate;
            private XmlFormatCollectionReaderDelegate xmlFormatReaderDelegate;
            private XmlFormatCollectionWriterDelegate xmlFormatWriterDelegate;

            internal CollectionDataContractCriticalHelper(CollectionKind kind)
            {
                this.Init(kind, null, null);
            }

            internal CollectionDataContractCriticalHelper(Type type) : base(type)
            {
                if (type == Globals.TypeOfArray)
                {
                    type = Globals.TypeOfObjectArray;
                }
                if (type.GetArrayRank() > 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("SupportForMultidimensionalArraysNotPresent")));
                }
                base.StableName = DataContract.GetStableName(type);
                this.Init(CollectionKind.Array, type.GetElementType(), null);
            }

            internal CollectionDataContractCriticalHelper(Type type, DataContract itemContract) : base(type)
            {
                if (type.GetArrayRank() > 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("SupportForMultidimensionalArraysNotPresent")));
                }
                base.StableName = DataContract.CreateQualifiedName("ArrayOf" + itemContract.StableName.Name, itemContract.StableName.Namespace);
                this.itemContract = itemContract;
                this.Init(CollectionKind.Array, type.GetElementType(), null);
            }

            internal CollectionDataContractCriticalHelper(Type type, string invalidCollectionInSharedContractMessage) : base(type)
            {
                this.Init(CollectionKind.Collection, null, null);
                this.invalidCollectionInSharedContractMessage = invalidCollectionInSharedContractMessage;
            }

            internal CollectionDataContractCriticalHelper(Type type, CollectionKind kind, Type itemType, MethodInfo getEnumeratorMethod, MethodInfo addMethod, ConstructorInfo constructor) : base(type)
            {
                CollectionDataContractAttribute attribute;
                if (getEnumeratorMethod == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("CollectionMustHaveGetEnumeratorMethod", new object[] { DataContract.GetClrTypeFullName(type) })));
                }
                if ((addMethod == null) && !type.IsInterface)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("CollectionMustHaveAddMethod", new object[] { DataContract.GetClrTypeFullName(type) })));
                }
                if (itemType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("CollectionMustHaveItemType", new object[] { DataContract.GetClrTypeFullName(type) })));
                }
                base.StableName = DataContract.GetCollectionStableName(type, itemType, out attribute);
                this.Init(kind, itemType, attribute);
                this.getEnumeratorMethod = getEnumeratorMethod;
                this.addMethod = addMethod;
                this.constructor = constructor;
            }

            internal CollectionDataContractCriticalHelper(Type type, CollectionKind kind, Type itemType, MethodInfo getEnumeratorMethod, MethodInfo addMethod, ConstructorInfo constructor, bool isConstructorCheckRequired) : this(type, kind, itemType, getEnumeratorMethod, addMethod, constructor)
            {
                this.isConstructorCheckRequired = isConstructorCheckRequired;
            }

            private void Init(CollectionKind kind, Type itemType, CollectionDataContractAttribute collectionContractAttribute)
            {
                this.kind = kind;
                if (itemType != null)
                {
                    this.itemType = itemType;
                    this.isItemTypeNullable = DataContract.IsTypeNullable(itemType);
                    bool flag = (kind == CollectionKind.Dictionary) || (kind == CollectionKind.GenericDictionary);
                    string str = null;
                    string str2 = null;
                    string str3 = null;
                    if (collectionContractAttribute != null)
                    {
                        if (collectionContractAttribute.IsItemNameSetExplicit)
                        {
                            if ((collectionContractAttribute.ItemName == null) || (collectionContractAttribute.ItemName.Length == 0))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidCollectionContractItemName", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType) })));
                            }
                            str = DataContract.EncodeLocalName(collectionContractAttribute.ItemName);
                            this.itemNameSetExplicit = true;
                        }
                        if (collectionContractAttribute.IsKeyNameSetExplicit)
                        {
                            if ((collectionContractAttribute.KeyName == null) || (collectionContractAttribute.KeyName.Length == 0))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidCollectionContractKeyName", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType) })));
                            }
                            if (!flag)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidCollectionContractKeyNoDictionary", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType), collectionContractAttribute.KeyName })));
                            }
                            str2 = DataContract.EncodeLocalName(collectionContractAttribute.KeyName);
                        }
                        if (collectionContractAttribute.IsValueNameSetExplicit)
                        {
                            if ((collectionContractAttribute.ValueName == null) || (collectionContractAttribute.ValueName.Length == 0))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidCollectionContractValueName", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType) })));
                            }
                            if (!flag)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidCollectionContractValueNoDictionary", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType), collectionContractAttribute.ValueName })));
                            }
                            str3 = DataContract.EncodeLocalName(collectionContractAttribute.ValueName);
                        }
                    }
                    XmlDictionary dictionary = flag ? new XmlDictionary(5) : new XmlDictionary(3);
                    base.Name = dictionary.Add(base.StableName.Name);
                    base.Namespace = dictionary.Add(base.StableName.Namespace);
                    this.itemName = str ?? DataContract.GetStableName(DataContract.UnwrapNullableType(itemType)).Name;
                    this.collectionItemName = dictionary.Add(this.itemName);
                    if (flag)
                    {
                        this.keyName = str2 ?? "Key";
                        this.valueName = str3 ?? "Value";
                    }
                }
                if (collectionContractAttribute != null)
                {
                    base.IsReference = collectionContractAttribute.IsReference;
                }
            }

            internal MethodInfo AddMethod
            {
                get
                {
                    return this.addMethod;
                }
            }

            public XmlDictionaryString ChildElementNamespace
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.childElementNamespace;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.childElementNamespace = value;
                }
            }

            public XmlDictionaryString CollectionItemName
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.collectionItemName;
                }
            }

            internal ConstructorInfo Constructor
            {
                get
                {
                    return this.constructor;
                }
            }

            internal MethodInfo GetEnumeratorMethod
            {
                get
                {
                    return this.getEnumeratorMethod;
                }
            }

            internal string InvalidCollectionInSharedContractMessage
            {
                get
                {
                    return this.invalidCollectionInSharedContractMessage;
                }
            }

            internal bool IsConstructorCheckRequired
            {
                get
                {
                    return this.isConstructorCheckRequired;
                }
                set
                {
                    this.isConstructorCheckRequired = value;
                }
            }

            internal bool IsDictionary
            {
                get
                {
                    return (this.KeyName != null);
                }
            }

            internal bool IsItemTypeNullable
            {
                get
                {
                    return this.isItemTypeNullable;
                }
                set
                {
                    this.isItemTypeNullable = value;
                }
            }

            internal DataContract ItemContract
            {
                get
                {
                    if ((this.itemContract == null) && (base.UnderlyingType != null))
                    {
                        if (this.IsDictionary)
                        {
                            if (string.CompareOrdinal(this.KeyName, this.ValueName) == 0)
                            {
                                DataContract.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("DupKeyValueName", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType), this.KeyName }), base.UnderlyingType);
                            }
                            this.itemContract = ClassDataContract.CreateClassDataContractForKeyValue(this.ItemType, base.Namespace, new string[] { this.KeyName, this.ValueName });
                            DataContract.GetDataContract(this.ItemType);
                        }
                        else
                        {
                            this.itemContract = DataContract.GetDataContract(this.ItemType);
                        }
                    }
                    return this.itemContract;
                }
                set
                {
                    this.itemContract = value;
                }
            }

            internal string ItemName
            {
                get
                {
                    return this.itemName;
                }
                set
                {
                    this.itemName = value;
                }
            }

            internal bool ItemNameSetExplicit
            {
                get
                {
                    return this.itemNameSetExplicit;
                }
            }

            internal Type ItemType
            {
                get
                {
                    return this.itemType;
                }
            }

            internal string KeyName
            {
                get
                {
                    return this.keyName;
                }
                set
                {
                    this.keyName = value;
                }
            }

            internal CollectionKind Kind
            {
                get
                {
                    return this.kind;
                }
            }

            internal override Dictionary<XmlQualifiedName, DataContract> KnownDataContracts
            {
                get
                {
                    if (!this.isKnownTypeAttributeChecked && (base.UnderlyingType != null))
                    {
                        lock (this)
                        {
                            if (!this.isKnownTypeAttributeChecked)
                            {
                                this.knownDataContracts = DataContract.ImportKnownTypeAttributes(base.UnderlyingType);
                                Thread.MemoryBarrier();
                                this.isKnownTypeAttributeChecked = true;
                            }
                        }
                    }
                    return this.knownDataContracts;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.knownDataContracts = value;
                }
            }

            internal static Type[] KnownInterfaces
            {
                get
                {
                    if (_knownInterfaces == null)
                    {
                        _knownInterfaces = new Type[] { Globals.TypeOfIDictionaryGeneric, Globals.TypeOfIDictionary, Globals.TypeOfIListGeneric, Globals.TypeOfICollectionGeneric, Globals.TypeOfIList, Globals.TypeOfIEnumerableGeneric, Globals.TypeOfICollection, Globals.TypeOfIEnumerable };
                    }
                    return _knownInterfaces;
                }
            }

            internal DataContract SharedTypeContract
            {
                get
                {
                    return this.sharedTypeContract;
                }
                set
                {
                    this.sharedTypeContract = value;
                }
            }

            internal string ValueName
            {
                get
                {
                    return this.valueName;
                }
                set
                {
                    this.valueName = value;
                }
            }

            internal System.Runtime.Serialization.XmlFormatGetOnlyCollectionReaderDelegate XmlFormatGetOnlyCollectionReaderDelegate
            {
                get
                {
                    return this.xmlFormatGetOnlyCollectionReaderDelegate;
                }
                set
                {
                    this.xmlFormatGetOnlyCollectionReaderDelegate = value;
                }
            }

            internal XmlFormatCollectionReaderDelegate XmlFormatReaderDelegate
            {
                get
                {
                    return this.xmlFormatReaderDelegate;
                }
                set
                {
                    this.xmlFormatReaderDelegate = value;
                }
            }

            internal XmlFormatCollectionWriterDelegate XmlFormatWriterDelegate
            {
                get
                {
                    return this.xmlFormatWriterDelegate;
                }
                set
                {
                    this.xmlFormatWriterDelegate = value;
                }
            }
        }

        public class DictionaryEnumerator : IEnumerator<KeyValue<object, object>>, IDisposable, IEnumerator
        {
            private IDictionaryEnumerator enumerator;

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public DictionaryEnumerator(IDictionaryEnumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return this.enumerator.MoveNext();
            }

            public void Reset()
            {
                this.enumerator.Reset();
            }

            public KeyValue<object, object> Current
            {
                get
                {
                    return new KeyValue<object, object>(this.enumerator.Key, this.enumerator.Value);
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }

        public class GenericDictionaryEnumerator<K, V> : IEnumerator<KeyValue<K, V>>, IDisposable, IEnumerator
        {
            private IEnumerator<KeyValuePair<K, V>> enumerator;

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public GenericDictionaryEnumerator(IEnumerator<KeyValuePair<K, V>> enumerator)
            {
                this.enumerator = enumerator;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return this.enumerator.MoveNext();
            }

            public void Reset()
            {
                this.enumerator.Reset();
            }

            public KeyValue<K, V> Current
            {
                get
                {
                    KeyValuePair<K, V> current = this.enumerator.Current;
                    return new KeyValue<K, V>(current.Key, current.Value);
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }
    }
}

