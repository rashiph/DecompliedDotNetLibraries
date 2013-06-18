namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.Serialization.Json;
    using System.Security;
    using System.Xml;

    internal static class JsonFormatGeneratorStatics
    {
        [SecurityCritical]
        private static MethodInfo boxPointer;
        [SecurityCritical]
        private static PropertyInfo collectionItemNameProperty;
        [SecurityCritical]
        private static ConstructorInfo extensionDataObjectCtor;
        [SecurityCritical]
        private static PropertyInfo extensionDataProperty;
        [SecurityCritical]
        private static MethodInfo getItemContractMethod;
        [SecurityCritical]
        private static MethodInfo getJsonDataContractMethod;
        [SecurityCritical]
        private static MethodInfo getJsonMemberIndexMethod;
        [SecurityCritical]
        private static MethodInfo getRevisedItemContractMethod;
        [SecurityCritical]
        private static MethodInfo getUninitializedObjectMethod;
        [SecurityCritical]
        private static MethodInfo ienumeratorGetCurrentMethod;
        [SecurityCritical]
        private static MethodInfo ienumeratorMoveNextMethod;
        [SecurityCritical]
        private static MethodInfo isStartElementMethod0;
        [SecurityCritical]
        private static MethodInfo isStartElementMethod2;
        [SecurityCritical]
        private static PropertyInfo nodeTypeProperty;
        [SecurityCritical]
        private static MethodInfo onDeserializationMethod;
        [SecurityCritical]
        private static MethodInfo readJsonValueMethod;
        [SecurityCritical]
        private static ConstructorInfo serializationExceptionCtor;
        [SecurityCritical]
        private static Type[] serInfoCtorArgs;
        [SecurityCritical]
        private static MethodInfo throwDuplicateMemberExceptionMethod;
        [SecurityCritical]
        private static MethodInfo throwMissingRequiredMembersMethod;
        [SecurityCritical]
        private static PropertyInfo typeHandleProperty;
        [SecurityCritical]
        private static MethodInfo unboxPointer;
        [SecurityCritical]
        private static MethodInfo writeAttributeStringMethod;
        [SecurityCritical]
        private static MethodInfo writeEndElementMethod;
        [SecurityCritical]
        private static MethodInfo writeJsonISerializableMethod;
        [SecurityCritical]
        private static MethodInfo writeJsonNameWithMappingMethod;
        [SecurityCritical]
        private static MethodInfo writeJsonValueMethod;
        [SecurityCritical]
        private static MethodInfo writeStartElementMethod;

        public static MethodInfo BoxPointer
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

        public static PropertyInfo CollectionItemNameProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (collectionItemNameProperty == null)
                {
                    collectionItemNameProperty = typeof(XmlObjectSerializerWriteContextComplexJson).GetProperty("CollectionItemName", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return collectionItemNameProperty;
            }
        }

        public static ConstructorInfo ExtensionDataObjectCtor
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

        public static PropertyInfo ExtensionDataProperty
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

        public static MethodInfo GetCurrentMethod
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

        public static MethodInfo GetItemContractMethod
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

        public static MethodInfo GetJsonDataContractMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getJsonDataContractMethod == null)
                {
                    getJsonDataContractMethod = typeof(JsonDataContract).GetMethod("GetJsonDataContract", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getJsonDataContractMethod;
            }
        }

        public static MethodInfo GetJsonMemberIndexMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getJsonMemberIndexMethod == null)
                {
                    getJsonMemberIndexMethod = typeof(XmlObjectSerializerReadContextComplexJson).GetMethod("GetJsonMemberIndex", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getJsonMemberIndexMethod;
            }
        }

        public static MethodInfo GetRevisedItemContractMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getRevisedItemContractMethod == null)
                {
                    getRevisedItemContractMethod = typeof(XmlObjectSerializerWriteContextComplexJson).GetMethod("GetRevisedItemContract", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return getRevisedItemContractMethod;
            }
        }

        public static MethodInfo GetUninitializedObjectMethod
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

        public static MethodInfo IsStartElementMethod0
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

        public static MethodInfo IsStartElementMethod2
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

        public static MethodInfo MoveNextMethod
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

        public static PropertyInfo NodeTypeProperty
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

        public static MethodInfo OnDeserializationMethod
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

        public static MethodInfo ReadJsonValueMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (readJsonValueMethod == null)
                {
                    readJsonValueMethod = typeof(DataContractJsonSerializer).GetMethod("ReadJsonValue", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return readJsonValueMethod;
            }
        }

        public static ConstructorInfo SerializationExceptionCtor
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

        public static Type[] SerInfoCtorArgs
        {
            [SecuritySafeCritical]
            get
            {
                if (serInfoCtorArgs == null)
                {
                    serInfoCtorArgs = new Type[] { typeof(SerializationInfo), typeof(StreamingContext) };
                }
                return serInfoCtorArgs;
            }
        }

        public static MethodInfo ThrowDuplicateMemberExceptionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (throwDuplicateMemberExceptionMethod == null)
                {
                    throwDuplicateMemberExceptionMethod = typeof(XmlObjectSerializerReadContextComplexJson).GetMethod("ThrowDuplicateMemberException", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return throwDuplicateMemberExceptionMethod;
            }
        }

        public static MethodInfo ThrowMissingRequiredMembersMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (throwMissingRequiredMembersMethod == null)
                {
                    throwMissingRequiredMembersMethod = typeof(XmlObjectSerializerReadContextComplexJson).GetMethod("ThrowMissingRequiredMembers", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return throwMissingRequiredMembersMethod;
            }
        }

        public static PropertyInfo TypeHandleProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (typeHandleProperty == null)
                {
                    typeHandleProperty = typeof(Type).GetProperty("TypeHandle");
                }
                return typeHandleProperty;
            }
        }

        public static MethodInfo UnboxPointer
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

        public static MethodInfo WriteAttributeStringMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeAttributeStringMethod == null)
                {
                    writeAttributeStringMethod = typeof(XmlWriterDelegator).GetMethod("WriteAttributeString", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) }, null);
                }
                return writeAttributeStringMethod;
            }
        }

        public static MethodInfo WriteEndElementMethod
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

        public static MethodInfo WriteJsonISerializableMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeJsonISerializableMethod == null)
                {
                    writeJsonISerializableMethod = typeof(XmlObjectSerializerWriteContextComplexJson).GetMethod("WriteJsonISerializable", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return writeJsonISerializableMethod;
            }
        }

        public static MethodInfo WriteJsonNameWithMappingMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeJsonNameWithMappingMethod == null)
                {
                    writeJsonNameWithMappingMethod = typeof(XmlObjectSerializerWriteContextComplexJson).GetMethod("WriteJsonNameWithMapping", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return writeJsonNameWithMappingMethod;
            }
        }

        public static MethodInfo WriteJsonValueMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeJsonValueMethod == null)
                {
                    writeJsonValueMethod = typeof(DataContractJsonSerializer).GetMethod("WriteJsonValue", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                return writeJsonValueMethod;
            }
        }

        public static MethodInfo WriteStartElementMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeStartElementMethod == null)
                {
                    writeStartElementMethod = typeof(XmlWriterDelegator).GetMethod("WriteStartElement", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new Type[] { typeof(XmlDictionaryString), typeof(XmlDictionaryString) }, null);
                }
                return writeStartElementMethod;
            }
        }
    }
}

