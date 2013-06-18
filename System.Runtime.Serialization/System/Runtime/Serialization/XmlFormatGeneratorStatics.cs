namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security;
    using System.Xml;

    internal static class XmlFormatGeneratorStatics
    {
        [SecurityCritical]
        private static MethodInfo addNewObjectMethod;
        [SecurityCritical]
        private static MethodInfo addNewObjectWithIdMethod;
        [SecurityCritical]
        private static MethodInfo boxPointer;
        [SecurityCritical]
        private static MethodInfo checkEndOfArrayMethod;
        [SecurityCritical]
        private static PropertyInfo childElementNamespaceProperty;
        [SecurityCritical]
        private static PropertyInfo childElementNamespacesProperty;
        [SecurityCritical]
        private static PropertyInfo collectionItemNameProperty;
        [SecurityCritical]
        private static FieldInfo contractNamespacesField;
        [SecurityCritical]
        private static MethodInfo createUnexpectedStateExceptionMethod;
        [SecurityCritical]
        private static MethodInfo demandMemberAccessPermissionMethod;
        [SecurityCritical]
        private static MethodInfo demandSerializationFormatterPermissionMethod;
        [SecurityCritical]
        private static ConstructorInfo dictionaryEnumeratorCtor;
        [SecurityCritical]
        private static MethodInfo ensureArraySizeMethod;
        [SecurityCritical]
        private static ConstructorInfo extensionDataObjectCtor;
        [SecurityCritical]
        private static PropertyInfo extensionDataProperty;
        [SecurityCritical]
        private static MethodInfo extensionDataSetExplicitMethodInfo;
        [SecurityCritical]
        private static MethodInfo getArrayLengthMethod;
        [SecurityCritical]
        private static MethodInfo getArraySizeMethod;
        [SecurityCritical]
        private static MethodInfo getCollectionMemberMethod;
        [SecurityCritical]
        private static MethodInfo getDateTimeOffsetAdapterMethod;
        [SecurityCritical]
        private static MethodInfo getDateTimeOffsetMethod;
        [SecurityCritical]
        private static MethodInfo getDefaultValueMethod;
        [SecurityCritical]
        private static MethodInfo getExistingObjectMethod;
        [SecurityCritical]
        private static MethodInfo getHasValueMethod;
        [SecurityCritical]
        private static MethodInfo getItemContractMethod;
        [SecurityCritical]
        private static MethodInfo getMemberIndexMethod;
        [SecurityCritical]
        private static MethodInfo getMemberIndexWithRequiredMembersMethod;
        [SecurityCritical]
        private static MethodInfo getNullableValueMethod;
        [SecurityCritical]
        private static MethodInfo getObjectIdMethod;
        [SecurityCritical]
        private static MethodInfo getRealObjectMethod;
        [SecurityCritical]
        private static MethodInfo getStreamingContextMethod;
        [SecurityCritical]
        private static MethodInfo getUninitializedObjectMethod;
        [SecurityCritical]
        private static ConstructorInfo hashtableCtor;
        [SecurityCritical]
        private static MethodInfo ienumeratorGetCurrentMethod;
        [SecurityCritical]
        private static MethodInfo ienumeratorMoveNextMethod;
        [SecurityCritical]
        private static MethodInfo incrementArrayCountMethod;
        [SecurityCritical]
        private static MethodInfo incrementCollectionCountGenericMethod;
        [SecurityCritical]
        private static MethodInfo incrementCollectionCountMethod;
        [SecurityCritical]
        private static MethodInfo incrementItemCountMethod;
        [SecurityCritical]
        private static MethodInfo internalDeserializeMethod;
        [SecurityCritical]
        private static MethodInfo internalSerializeMethod;
        [SecurityCritical]
        private static MethodInfo internalSerializeReferenceMethod;
        [SecurityCritical]
        private static MethodInfo isStartElementMethod0;
        [SecurityCritical]
        private static MethodInfo isStartElementMethod2;
        [SecurityCritical]
        private static FieldInfo memberNamesField;
        [SecurityCritical]
        private static MethodInfo moveToNextElementMethod;
        [SecurityCritical]
        private static PropertyInfo namespaceProperty;
        [SecurityCritical]
        private static PropertyInfo nodeTypeProperty;
        [SecurityCritical]
        private static MethodInfo onDeserializationMethod;
        [SecurityCritical]
        private static MethodInfo readAttributesMethod;
        [SecurityCritical]
        private static MethodInfo readIfNullOrRefMethod;
        [SecurityCritical]
        private static MethodInfo readMethod;
        [SecurityCritical]
        private static MethodInfo readSerializationInfoMethod;
        [SecurityCritical]
        private static MethodInfo readXmlValueMethod;
        [SecurityCritical]
        private static MethodInfo replaceDeserializedObjectMethod;
        [SecurityCritical]
        private static MethodInfo resetAttributesMethod;
        [SecurityCritical]
        private static ConstructorInfo serializationExceptionCtor;
        [SecurityCritical]
        private static MethodInfo skipUnknownElementMethod;
        [SecurityCritical]
        private static MethodInfo storeCollectionMemberInfoMethod;
        [SecurityCritical]
        private static MethodInfo storeIsGetOnlyCollectionMethod;
        private static MethodInfo throwArrayExceededSizeExceptionMethod;
        [SecurityCritical]
        private static MethodInfo throwNullValueReturnedForGetOnlyCollectionExceptionMethod;
        [SecurityCritical]
        private static MethodInfo throwRequiredMemberMissingExceptionMethod;
        [SecurityCritical]
        private static MethodInfo throwRequiredMemberMustBeEmittedMethod;
        [SecurityCritical]
        private static MethodInfo throwTypeNotSerializableMethod;
        [SecurityCritical]
        private static MethodInfo traceInstructionMethod;
        [SecurityCritical]
        private static MethodInfo trimArraySizeMethod;
        [SecurityCritical]
        private static MethodInfo unboxPointer;
        [SecurityCritical]
        private static MethodInfo writeEndElementMethod;
        [SecurityCritical]
        private static MethodInfo writeExtensionDataMethod;
        [SecurityCritical]
        private static MethodInfo writeISerializableMethod;
        [SecurityCritical]
        private static MethodInfo writeNamespaceDeclMethod;
        [SecurityCritical]
        private static MethodInfo writeNullMethod;
        [SecurityCritical]
        private static MethodInfo writeStartElementMethod2;
        [SecurityCritical]
        private static MethodInfo writeStartElementMethod3;
        [SecurityCritical]
        private static MethodInfo writeXmlValueMethod;

        internal static MethodInfo AddNewObjectMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (addNewObjectMethod == null)
                {
                    addNewObjectMethod = typeof(XmlObjectSerializerReadContext).GetMethod("AddNewObject", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return addNewObjectMethod;
            }
        }

        internal static MethodInfo AddNewObjectWithIdMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (addNewObjectWithIdMethod == null)
                {
                    addNewObjectWithIdMethod = typeof(XmlObjectSerializerReadContext).GetMethod("AddNewObjectWithId", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return addNewObjectWithIdMethod;
            }
        }

        internal static MethodInfo BoxPointer
        {
            [SecuritySafeCritical]
            get
            {
                if (boxPointer == null)
                {
                    boxPointer = typeof(Pointer).GetMethod("Box");
                }
                return boxPointer;
            }
        }

        internal static MethodInfo CheckEndOfArrayMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (checkEndOfArrayMethod == null)
                {
                    checkEndOfArrayMethod = typeof(XmlObjectSerializerReadContext).GetMethod("CheckEndOfArray", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return checkEndOfArrayMethod;
            }
        }

        internal static PropertyInfo ChildElementNamespaceProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (childElementNamespaceProperty == null)
                {
                    childElementNamespaceProperty = typeof(CollectionDataContract).GetProperty("ChildElementNamespace", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return childElementNamespaceProperty;
            }
        }

        internal static PropertyInfo ChildElementNamespacesProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (childElementNamespacesProperty == null)
                {
                    childElementNamespacesProperty = typeof(ClassDataContract).GetProperty("ChildElementNamespaces", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return childElementNamespacesProperty;
            }
        }

        internal static PropertyInfo CollectionItemNameProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (collectionItemNameProperty == null)
                {
                    collectionItemNameProperty = typeof(CollectionDataContract).GetProperty("CollectionItemName", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return collectionItemNameProperty;
            }
        }

        internal static FieldInfo ContractNamespacesField
        {
            [SecuritySafeCritical]
            get
            {
                if (contractNamespacesField == null)
                {
                    contractNamespacesField = typeof(ClassDataContract).GetField("ContractNamespaces", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return contractNamespacesField;
            }
        }

        internal static MethodInfo CreateUnexpectedStateExceptionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (createUnexpectedStateExceptionMethod == null)
                {
                    createUnexpectedStateExceptionMethod = typeof(XmlObjectSerializerReadContext).GetMethod("CreateUnexpectedStateException", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { typeof(XmlNodeType), typeof(XmlReaderDelegator) }, null);
                }
                return createUnexpectedStateExceptionMethod;
            }
        }

        internal static MethodInfo DemandMemberAccessPermissionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (demandMemberAccessPermissionMethod == null)
                {
                    demandMemberAccessPermissionMethod = typeof(XmlObjectSerializerContext).GetMethod("DemandMemberAccessPermission", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return demandMemberAccessPermissionMethod;
            }
        }

        internal static MethodInfo DemandSerializationFormatterPermissionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (demandSerializationFormatterPermissionMethod == null)
                {
                    demandSerializationFormatterPermissionMethod = typeof(XmlObjectSerializerContext).GetMethod("DemandSerializationFormatterPermission", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return demandSerializationFormatterPermissionMethod;
            }
        }

        internal static ConstructorInfo DictionaryEnumeratorCtor
        {
            [SecuritySafeCritical]
            get
            {
                if (dictionaryEnumeratorCtor == null)
                {
                    dictionaryEnumeratorCtor = Globals.TypeOfDictionaryEnumerator.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { Globals.TypeOfIDictionaryEnumerator }, null);
                }
                return dictionaryEnumeratorCtor;
            }
        }

        internal static MethodInfo EnsureArraySizeMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (ensureArraySizeMethod == null)
                {
                    ensureArraySizeMethod = typeof(XmlObjectSerializerReadContext).GetMethod("EnsureArraySize", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return ensureArraySizeMethod;
            }
        }

        internal static ConstructorInfo ExtensionDataObjectCtor
        {
            [SecuritySafeCritical]
            get
            {
                if (extensionDataObjectCtor == null)
                {
                    extensionDataObjectCtor = typeof(ExtensionDataObject).GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[0], null);
                }
                return extensionDataObjectCtor;
            }
        }

        internal static PropertyInfo ExtensionDataProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (extensionDataProperty == null)
                {
                    extensionDataProperty = typeof(IExtensibleDataObject).GetProperty("ExtensionData");
                }
                return extensionDataProperty;
            }
        }

        internal static MethodInfo ExtensionDataSetExplicitMethodInfo
        {
            [SecuritySafeCritical]
            get
            {
                if (extensionDataSetExplicitMethodInfo == null)
                {
                    extensionDataSetExplicitMethodInfo = typeof(IExtensibleDataObject).GetMethod("set_ExtensionData");
                }
                return extensionDataSetExplicitMethodInfo;
            }
        }

        internal static MethodInfo GetArrayLengthMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getArrayLengthMethod == null)
                {
                    getArrayLengthMethod = Globals.TypeOfArray.GetProperty("Length").GetGetMethod();
                }
                return getArrayLengthMethod;
            }
        }

        internal static MethodInfo GetArraySizeMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getArraySizeMethod == null)
                {
                    getArraySizeMethod = typeof(XmlObjectSerializerReadContext).GetMethod("GetArraySize", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getArraySizeMethod;
            }
        }

        internal static MethodInfo GetCollectionMemberMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getCollectionMemberMethod == null)
                {
                    getCollectionMemberMethod = typeof(XmlObjectSerializerReadContext).GetMethod("GetCollectionMember", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getCollectionMemberMethod;
            }
        }

        internal static MethodInfo GetCurrentMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (ienumeratorGetCurrentMethod == null)
                {
                    ienumeratorGetCurrentMethod = typeof(IEnumerator).GetProperty("Current").GetGetMethod();
                }
                return ienumeratorGetCurrentMethod;
            }
        }

        internal static MethodInfo GetDateTimeOffsetAdapterMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getDateTimeOffsetAdapterMethod == null)
                {
                    getDateTimeOffsetAdapterMethod = typeof(DateTimeOffsetAdapter).GetMethod("GetDateTimeOffsetAdapter", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getDateTimeOffsetAdapterMethod;
            }
        }

        internal static MethodInfo GetDateTimeOffsetMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getDateTimeOffsetMethod == null)
                {
                    getDateTimeOffsetMethod = typeof(DateTimeOffsetAdapter).GetMethod("GetDateTimeOffset", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getDateTimeOffsetMethod;
            }
        }

        internal static MethodInfo GetDefaultValueMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getDefaultValueMethod == null)
                {
                    getDefaultValueMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("GetDefaultValue", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getDefaultValueMethod;
            }
        }

        internal static MethodInfo GetExistingObjectMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getExistingObjectMethod == null)
                {
                    getExistingObjectMethod = typeof(XmlObjectSerializerReadContext).GetMethod("GetExistingObject", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getExistingObjectMethod;
            }
        }

        internal static MethodInfo GetHasValueMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getHasValueMethod == null)
                {
                    getHasValueMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("GetHasValue", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getHasValueMethod;
            }
        }

        internal static MethodInfo GetItemContractMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getItemContractMethod == null)
                {
                    getItemContractMethod = typeof(CollectionDataContract).GetProperty("ItemContract", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).GetGetMethod(true);
                }
                return getItemContractMethod;
            }
        }

        internal static MethodInfo GetMemberIndexMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getMemberIndexMethod == null)
                {
                    getMemberIndexMethod = typeof(XmlObjectSerializerReadContext).GetMethod("GetMemberIndex", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getMemberIndexMethod;
            }
        }

        internal static MethodInfo GetMemberIndexWithRequiredMembersMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getMemberIndexWithRequiredMembersMethod == null)
                {
                    getMemberIndexWithRequiredMembersMethod = typeof(XmlObjectSerializerReadContext).GetMethod("GetMemberIndexWithRequiredMembers", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getMemberIndexWithRequiredMembersMethod;
            }
        }

        internal static MethodInfo GetNullableValueMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getNullableValueMethod == null)
                {
                    getNullableValueMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("GetNullableValue", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getNullableValueMethod;
            }
        }

        internal static MethodInfo GetObjectIdMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getObjectIdMethod == null)
                {
                    getObjectIdMethod = typeof(XmlObjectSerializerReadContext).GetMethod("GetObjectId", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getObjectIdMethod;
            }
        }

        internal static MethodInfo GetRealObjectMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getRealObjectMethod == null)
                {
                    getRealObjectMethod = typeof(XmlObjectSerializerReadContext).GetMethod("GetRealObject", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getRealObjectMethod;
            }
        }

        internal static MethodInfo GetStreamingContextMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getStreamingContextMethod == null)
                {
                    getStreamingContextMethod = typeof(XmlObjectSerializerContext).GetMethod("GetStreamingContext", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getStreamingContextMethod;
            }
        }

        internal static MethodInfo GetUninitializedObjectMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getUninitializedObjectMethod == null)
                {
                    getUninitializedObjectMethod = typeof(XmlFormatReaderGenerator).GetMethod("UnsafeGetUninitializedObject", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { typeof(int) }, null);
                }
                return getUninitializedObjectMethod;
            }
        }

        internal static ConstructorInfo HashtableCtor
        {
            [SecuritySafeCritical]
            get
            {
                if (hashtableCtor == null)
                {
                    hashtableCtor = Globals.TypeOfHashtable.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, Globals.EmptyTypeArray, null);
                }
                return hashtableCtor;
            }
        }

        internal static MethodInfo IncrementArrayCountMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (incrementArrayCountMethod == null)
                {
                    incrementArrayCountMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("IncrementArrayCount", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return incrementArrayCountMethod;
            }
        }

        internal static MethodInfo IncrementCollectionCountGenericMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (incrementCollectionCountGenericMethod == null)
                {
                    incrementCollectionCountGenericMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("IncrementCollectionCountGeneric", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return incrementCollectionCountGenericMethod;
            }
        }

        internal static MethodInfo IncrementCollectionCountMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (incrementCollectionCountMethod == null)
                {
                    incrementCollectionCountMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("IncrementCollectionCount", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { typeof(XmlWriterDelegator), typeof(ICollection) }, null);
                }
                return incrementCollectionCountMethod;
            }
        }

        internal static MethodInfo IncrementItemCountMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (incrementItemCountMethod == null)
                {
                    incrementItemCountMethod = typeof(XmlObjectSerializerContext).GetMethod("IncrementItemCount", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return incrementItemCountMethod;
            }
        }

        internal static MethodInfo InternalDeserializeMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (internalDeserializeMethod == null)
                {
                    internalDeserializeMethod = typeof(XmlObjectSerializerReadContext).GetMethod("InternalDeserialize", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { typeof(XmlReaderDelegator), typeof(int), typeof(RuntimeTypeHandle), typeof(string), typeof(string) }, null);
                }
                return internalDeserializeMethod;
            }
        }

        internal static MethodInfo InternalSerializeMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (internalSerializeMethod == null)
                {
                    internalSerializeMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("InternalSerialize", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return internalSerializeMethod;
            }
        }

        internal static MethodInfo InternalSerializeReferenceMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (internalSerializeReferenceMethod == null)
                {
                    internalSerializeReferenceMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("InternalSerializeReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return internalSerializeReferenceMethod;
            }
        }

        internal static MethodInfo IsStartElementMethod0
        {
            [SecuritySafeCritical]
            get
            {
                if (isStartElementMethod0 == null)
                {
                    isStartElementMethod0 = typeof(XmlReaderDelegator).GetMethod("IsStartElement", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[0], null);
                }
                return isStartElementMethod0;
            }
        }

        internal static MethodInfo IsStartElementMethod2
        {
            [SecuritySafeCritical]
            get
            {
                if (isStartElementMethod2 == null)
                {
                    isStartElementMethod2 = typeof(XmlReaderDelegator).GetMethod("IsStartElement", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { typeof(XmlDictionaryString), typeof(XmlDictionaryString) }, null);
                }
                return isStartElementMethod2;
            }
        }

        internal static FieldInfo MemberNamesField
        {
            [SecuritySafeCritical]
            get
            {
                if (memberNamesField == null)
                {
                    memberNamesField = typeof(ClassDataContract).GetField("MemberNames", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return memberNamesField;
            }
        }

        internal static MethodInfo MoveNextMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (ienumeratorMoveNextMethod == null)
                {
                    ienumeratorMoveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");
                }
                return ienumeratorMoveNextMethod;
            }
        }

        internal static MethodInfo MoveToNextElementMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (moveToNextElementMethod == null)
                {
                    moveToNextElementMethod = typeof(XmlObjectSerializerReadContext).GetMethod("MoveToNextElement", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return moveToNextElementMethod;
            }
        }

        internal static PropertyInfo NamespaceProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (namespaceProperty == null)
                {
                    namespaceProperty = typeof(DataContract).GetProperty("Namespace", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return namespaceProperty;
            }
        }

        internal static PropertyInfo NodeTypeProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (nodeTypeProperty == null)
                {
                    nodeTypeProperty = typeof(XmlReaderDelegator).GetProperty("NodeType", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return nodeTypeProperty;
            }
        }

        internal static MethodInfo OnDeserializationMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (onDeserializationMethod == null)
                {
                    onDeserializationMethod = typeof(IDeserializationCallback).GetMethod("OnDeserialization");
                }
                return onDeserializationMethod;
            }
        }

        internal static MethodInfo ReadAttributesMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (readAttributesMethod == null)
                {
                    readAttributesMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ReadAttributes", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return readAttributesMethod;
            }
        }

        internal static MethodInfo ReadIfNullOrRefMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (readIfNullOrRefMethod == null)
                {
                    readIfNullOrRefMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ReadIfNullOrRef", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { typeof(XmlReaderDelegator), typeof(Type), typeof(bool) }, null);
                }
                return readIfNullOrRefMethod;
            }
        }

        internal static MethodInfo ReadMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (readMethod == null)
                {
                    readMethod = typeof(XmlObjectSerializerReadContext).GetMethod("Read", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return readMethod;
            }
        }

        internal static MethodInfo ReadSerializationInfoMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (readSerializationInfoMethod == null)
                {
                    readSerializationInfoMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ReadSerializationInfo", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return readSerializationInfoMethod;
            }
        }

        internal static MethodInfo ReadXmlValueMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (readXmlValueMethod == null)
                {
                    readXmlValueMethod = typeof(DataContract).GetMethod("ReadXmlValue", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return readXmlValueMethod;
            }
        }

        internal static MethodInfo ReplaceDeserializedObjectMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (replaceDeserializedObjectMethod == null)
                {
                    replaceDeserializedObjectMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ReplaceDeserializedObject", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return replaceDeserializedObjectMethod;
            }
        }

        internal static MethodInfo ResetAttributesMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (resetAttributesMethod == null)
                {
                    resetAttributesMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ResetAttributes", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return resetAttributesMethod;
            }
        }

        internal static ConstructorInfo SerializationExceptionCtor
        {
            [SecuritySafeCritical]
            get
            {
                if (serializationExceptionCtor == null)
                {
                    serializationExceptionCtor = typeof(SerializationException).GetConstructor(new Type[] { typeof(string) });
                }
                return serializationExceptionCtor;
            }
        }

        internal static MethodInfo SkipUnknownElementMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (skipUnknownElementMethod == null)
                {
                    skipUnknownElementMethod = typeof(XmlObjectSerializerReadContext).GetMethod("SkipUnknownElement", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return skipUnknownElementMethod;
            }
        }

        internal static MethodInfo StoreCollectionMemberInfoMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (storeCollectionMemberInfoMethod == null)
                {
                    storeCollectionMemberInfoMethod = typeof(XmlObjectSerializerReadContext).GetMethod("StoreCollectionMemberInfo", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { typeof(object) }, null);
                }
                return storeCollectionMemberInfoMethod;
            }
        }

        internal static MethodInfo StoreIsGetOnlyCollectionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (storeIsGetOnlyCollectionMethod == null)
                {
                    storeIsGetOnlyCollectionMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("StoreIsGetOnlyCollection", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return storeIsGetOnlyCollectionMethod;
            }
        }

        internal static MethodInfo ThrowArrayExceededSizeExceptionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (throwArrayExceededSizeExceptionMethod == null)
                {
                    throwArrayExceededSizeExceptionMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ThrowArrayExceededSizeException", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return throwArrayExceededSizeExceptionMethod;
            }
        }

        internal static MethodInfo ThrowNullValueReturnedForGetOnlyCollectionExceptionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (throwNullValueReturnedForGetOnlyCollectionExceptionMethod == null)
                {
                    throwNullValueReturnedForGetOnlyCollectionExceptionMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ThrowNullValueReturnedForGetOnlyCollectionException", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return throwNullValueReturnedForGetOnlyCollectionExceptionMethod;
            }
        }

        internal static MethodInfo ThrowRequiredMemberMissingExceptionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (throwRequiredMemberMissingExceptionMethod == null)
                {
                    throwRequiredMemberMissingExceptionMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ThrowRequiredMemberMissingException", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return throwRequiredMemberMissingExceptionMethod;
            }
        }

        internal static MethodInfo ThrowRequiredMemberMustBeEmittedMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (throwRequiredMemberMustBeEmittedMethod == null)
                {
                    throwRequiredMemberMustBeEmittedMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("ThrowRequiredMemberMustBeEmitted", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return throwRequiredMemberMustBeEmittedMethod;
            }
        }

        internal static MethodInfo ThrowTypeNotSerializableMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (throwTypeNotSerializableMethod == null)
                {
                    throwTypeNotSerializableMethod = typeof(DataContract).GetMethod("ThrowTypeNotSerializable", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return throwTypeNotSerializableMethod;
            }
        }

        internal static MethodInfo TraceInstructionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (traceInstructionMethod == null)
                {
                    traceInstructionMethod = typeof(SerializationTrace).GetMethod("TraceInstruction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return traceInstructionMethod;
            }
        }

        internal static MethodInfo TrimArraySizeMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (trimArraySizeMethod == null)
                {
                    trimArraySizeMethod = typeof(XmlObjectSerializerReadContext).GetMethod("TrimArraySize", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return trimArraySizeMethod;
            }
        }

        internal static MethodInfo UnboxPointer
        {
            [SecuritySafeCritical]
            get
            {
                if (unboxPointer == null)
                {
                    unboxPointer = typeof(Pointer).GetMethod("Unbox");
                }
                return unboxPointer;
            }
        }

        internal static MethodInfo WriteEndElementMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeEndElementMethod == null)
                {
                    writeEndElementMethod = typeof(XmlWriterDelegator).GetMethod("WriteEndElement", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[0], null);
                }
                return writeEndElementMethod;
            }
        }

        internal static MethodInfo WriteExtensionDataMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeExtensionDataMethod == null)
                {
                    writeExtensionDataMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("WriteExtensionData", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return writeExtensionDataMethod;
            }
        }

        internal static MethodInfo WriteISerializableMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeISerializableMethod == null)
                {
                    writeISerializableMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("WriteISerializable", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return writeISerializableMethod;
            }
        }

        internal static MethodInfo WriteNamespaceDeclMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeNamespaceDeclMethod == null)
                {
                    writeNamespaceDeclMethod = typeof(XmlWriterDelegator).GetMethod("WriteNamespaceDecl", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { typeof(XmlDictionaryString) }, null);
                }
                return writeNamespaceDeclMethod;
            }
        }

        internal static MethodInfo WriteNullMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeNullMethod == null)
                {
                    writeNullMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("WriteNull", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { typeof(XmlWriterDelegator), typeof(Type), typeof(bool) }, null);
                }
                return writeNullMethod;
            }
        }

        internal static MethodInfo WriteStartElementMethod2
        {
            [SecuritySafeCritical]
            get
            {
                if (writeStartElementMethod2 == null)
                {
                    writeStartElementMethod2 = typeof(XmlWriterDelegator).GetMethod("WriteStartElement", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { typeof(XmlDictionaryString), typeof(XmlDictionaryString) }, null);
                }
                return writeStartElementMethod2;
            }
        }

        internal static MethodInfo WriteStartElementMethod3
        {
            [SecuritySafeCritical]
            get
            {
                if (writeStartElementMethod3 == null)
                {
                    writeStartElementMethod3 = typeof(XmlWriterDelegator).GetMethod("WriteStartElement", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(XmlDictionaryString), typeof(XmlDictionaryString) }, null);
                }
                return writeStartElementMethod3;
            }
        }

        internal static MethodInfo WriteXmlValueMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeXmlValueMethod == null)
                {
                    writeXmlValueMethod = typeof(DataContract).GetMethod("WriteXmlValue", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return writeXmlValueMethod;
            }
        }
    }
}

