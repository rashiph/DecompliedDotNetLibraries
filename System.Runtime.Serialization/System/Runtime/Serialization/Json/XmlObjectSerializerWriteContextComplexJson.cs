namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Xml;

    internal class XmlObjectSerializerWriteContextComplexJson : XmlObjectSerializerWriteContextComplex
    {
        private bool alwaysEmitXsiType;
        private bool perCallXsiTypeAlreadyEmitted;

        public XmlObjectSerializerWriteContextComplexJson(DataContractJsonSerializer serializer, DataContract rootTypeDataContract) : base(serializer, serializer.MaxItemsInObjectGraph, new StreamingContext(StreamingContextStates.All), serializer.IgnoreExtensionDataObject)
        {
            this.alwaysEmitXsiType = serializer.AlwaysEmitTypeInformation;
            base.rootTypeDataContract = rootTypeDataContract;
            base.serializerKnownTypeList = serializer.knownTypeList;
            base.dataContractSurrogate = serializer.DataContractSurrogate;
        }

        internal static XmlObjectSerializerWriteContextComplexJson CreateContext(DataContractJsonSerializer serializer, DataContract rootTypeDataContract)
        {
            return new XmlObjectSerializerWriteContextComplexJson(serializer, rootTypeDataContract);
        }

        internal override DataContract GetDataContract(int id, RuntimeTypeHandle typeHandle)
        {
            DataContract dataContract = base.GetDataContract(id, typeHandle);
            DataContractJsonSerializer.CheckIfTypeIsReference(dataContract);
            return dataContract;
        }

        internal override DataContract GetDataContract(RuntimeTypeHandle typeHandle, Type type)
        {
            DataContract dataContract = base.GetDataContract(typeHandle, type);
            DataContractJsonSerializer.CheckIfTypeIsReference(dataContract);
            return dataContract;
        }

        internal override DataContract GetDataContractSkipValidation(int typeId, RuntimeTypeHandle typeHandle, Type type)
        {
            DataContract dataContract = base.GetDataContractSkipValidation(typeId, typeHandle, type);
            DataContractJsonSerializer.CheckIfTypeIsReference(dataContract);
            return dataContract;
        }

        internal static DataContract GetRevisedItemContract(DataContract oldItemContract)
        {
            if (((oldItemContract != null) && oldItemContract.UnderlyingType.IsGenericType) && (oldItemContract.UnderlyingType.GetGenericTypeDefinition() == Globals.TypeOfKeyValue))
            {
                return ClassDataContract.CreateClassDataContractForKeyValue(oldItemContract.UnderlyingType, oldItemContract.Namespace, new string[] { "Key", "Value" });
            }
            return oldItemContract;
        }

        private void HandleCollectionAssignedToObject(Type declaredType, ref DataContract dataContract, ref object obj, ref bool verifyKnownType)
        {
            if ((declaredType != dataContract.UnderlyingType) && (dataContract is CollectionDataContract))
            {
                if (verifyKnownType)
                {
                    this.VerifyType(dataContract, declaredType);
                    verifyKnownType = false;
                }
                if (((CollectionDataContract) dataContract).Kind == CollectionKind.Dictionary)
                {
                    IDictionary dictionary = obj as IDictionary;
                    Dictionary<object, object> dictionary2 = new Dictionary<object, object>();
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        dictionary2.Add(entry.Key, entry.Value);
                    }
                    obj = dictionary2;
                }
                dataContract = base.GetDataContract(Globals.TypeOfIEnumerable);
            }
        }

        private static bool RequiresJsonTypeInfo(DataContract contract)
        {
            return (contract is ClassDataContract);
        }

        protected override void SerializeWithXsiType(XmlWriterDelegator xmlWriter, object obj, RuntimeTypeHandle objectTypeHandle, Type objectType, int declaredTypeID, RuntimeTypeHandle declaredTypeHandle, Type declaredType)
        {
            DataContract dataContract;
            bool verifyKnownType = false;
            bool isInterface = declaredType.IsInterface;
            if (isInterface && CollectionDataContract.IsCollectionInterface(declaredType))
            {
                dataContract = this.GetDataContract(declaredTypeHandle, declaredType);
            }
            else if (declaredType.IsArray)
            {
                dataContract = this.GetDataContract(declaredTypeHandle, declaredType);
            }
            else
            {
                dataContract = this.GetDataContract(objectTypeHandle, objectType);
                DataContract declaredContract = (declaredTypeID >= 0) ? this.GetDataContract(declaredTypeID, declaredTypeHandle) : this.GetDataContract(declaredTypeHandle, declaredType);
                verifyKnownType = this.WriteTypeInfo(xmlWriter, dataContract, declaredContract);
                this.HandleCollectionAssignedToObject(declaredType, ref dataContract, ref obj, ref verifyKnownType);
            }
            if (isInterface)
            {
                VerifyObjectCompatibilityWithInterface(dataContract, obj, declaredType);
            }
            base.SerializeAndVerifyType(dataContract, xmlWriter, obj, verifyKnownType, declaredType.TypeHandle, declaredType);
        }

        internal override void SerializeWithXsiTypeAtTopLevel(DataContract dataContract, XmlWriterDelegator xmlWriter, object obj, RuntimeTypeHandle originalDeclaredTypeHandle, Type graphType)
        {
            bool verifyKnownType = false;
            Type underlyingType = base.rootTypeDataContract.UnderlyingType;
            bool isInterface = underlyingType.IsInterface;
            if ((!isInterface || !CollectionDataContract.IsCollectionInterface(underlyingType)) && !underlyingType.IsArray)
            {
                verifyKnownType = this.WriteTypeInfo(xmlWriter, dataContract, base.rootTypeDataContract);
                this.HandleCollectionAssignedToObject(underlyingType, ref dataContract, ref obj, ref verifyKnownType);
            }
            if (isInterface)
            {
                VerifyObjectCompatibilityWithInterface(dataContract, obj, underlyingType);
            }
            base.SerializeAndVerifyType(dataContract, xmlWriter, obj, verifyKnownType, underlyingType.TypeHandle, underlyingType);
        }

        internal static string TruncateDefaultDataContractNamespace(string dataContractNamespace)
        {
            if (!string.IsNullOrEmpty(dataContractNamespace))
            {
                if (dataContractNamespace[0] == '#')
                {
                    return (@"\" + dataContractNamespace);
                }
                if (dataContractNamespace[0] == '\\')
                {
                    return (@"\" + dataContractNamespace);
                }
                if (dataContractNamespace.StartsWith("http://schemas.datacontract.org/2004/07/", StringComparison.Ordinal))
                {
                    return ("#" + dataContractNamespace.Substring(JsonGlobals.DataContractXsdBaseNamespaceLength));
                }
            }
            return dataContractNamespace;
        }

        private static void VerifyObjectCompatibilityWithInterface(DataContract contract, object graph, Type declaredType)
        {
            Type type = contract.GetType();
            if ((type == typeof(XmlDataContract)) && !Globals.TypeOfIXmlSerializable.IsAssignableFrom(declaredType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("XmlObjectAssignedToIncompatibleInterface", new object[] { graph.GetType(), declaredType })));
            }
            if ((type == typeof(CollectionDataContract)) && !CollectionDataContract.IsCollectionInterface(declaredType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("CollectionAssignedToIncompatibleInterface", new object[] { graph.GetType(), declaredType })));
            }
        }

        private void VerifyType(DataContract dataContract, Type declaredType)
        {
            bool flag = false;
            if (dataContract.KnownDataContracts != null)
            {
                this.scopedKnownTypes.Push(dataContract.KnownDataContracts);
                flag = true;
            }
            if (!base.IsKnownType(dataContract, declaredType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("DcTypeNotFoundOnSerialize", new object[] { DataContract.GetClrTypeFullName(dataContract.UnderlyingType), dataContract.StableName.Name, dataContract.StableName.Namespace })));
            }
            if (flag)
            {
                this.scopedKnownTypes.Pop();
            }
        }

        internal override void WriteArraySize(XmlWriterDelegator xmlWriter, int size)
        {
        }

        internal override bool WriteClrTypeInfo(XmlWriterDelegator xmlWriter, DataContract dataContract)
        {
            return false;
        }

        internal override bool WriteClrTypeInfo(XmlWriterDelegator xmlWriter, Type dataContractType, string clrTypeName, string clrAssemblyName)
        {
            return false;
        }

        protected override void WriteDataContractValue(DataContract dataContract, XmlWriterDelegator xmlWriter, object obj, RuntimeTypeHandle declaredTypeHandle)
        {
            JsonDataContract jsonDataContract = JsonDataContract.GetJsonDataContract(dataContract);
            if ((this.alwaysEmitXsiType && !this.perCallXsiTypeAlreadyEmitted) && RequiresJsonTypeInfo(dataContract))
            {
                this.WriteTypeInfo(xmlWriter, jsonDataContract.TypeName);
            }
            this.perCallXsiTypeAlreadyEmitted = false;
            DataContractJsonSerializer.WriteJsonValue(jsonDataContract, xmlWriter, obj, this, declaredTypeHandle);
        }

        internal override void WriteExtensionDataTypeInfo(XmlWriterDelegator xmlWriter, IDataNode dataNode)
        {
            Type dataType = dataNode.DataType;
            if ((dataType == Globals.TypeOfClassDataNode) || (dataType == Globals.TypeOfISerializableDataNode))
            {
                xmlWriter.WriteAttributeString(null, "type", null, "object");
                base.WriteExtensionDataTypeInfo(xmlWriter, dataNode);
            }
            else if (dataType == Globals.TypeOfCollectionDataNode)
            {
                xmlWriter.WriteAttributeString(null, "type", null, "array");
            }
            else if ((dataType != Globals.TypeOfXmlDataNode) && (((dataType == Globals.TypeOfObject) && (dataNode.Value != null)) && RequiresJsonTypeInfo(base.GetDataContract(dataNode.Value.GetType()))))
            {
                base.WriteExtensionDataTypeInfo(xmlWriter, dataNode);
            }
        }

        internal void WriteJsonISerializable(XmlWriterDelegator xmlWriter, ISerializable obj)
        {
            Type type = obj.GetType();
            SerializationInfo info = new SerializationInfo(type, XmlObjectSerializer.FormatterConverter);
            obj.GetObjectData(info, base.GetStreamingContext());
            if (DataContract.GetClrTypeFullName(type) != info.FullTypeName)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ChangingFullTypeNameNotSupported", new object[] { info.FullTypeName, DataContract.GetClrTypeFullName(type) })));
            }
            base.WriteSerializationInfo(xmlWriter, type, info);
        }

        internal static void WriteJsonNameWithMapping(XmlWriterDelegator xmlWriter, XmlDictionaryString[] memberNames, int index)
        {
            xmlWriter.WriteStartElement("a", "item", "item");
            xmlWriter.WriteAttributeString(null, "item", null, memberNames[index].Value);
        }

        protected override void WriteNull(XmlWriterDelegator xmlWriter)
        {
            DataContractJsonSerializer.WriteJsonNull(xmlWriter);
        }

        private void WriteTypeInfo(XmlWriterDelegator writer, string typeInformation)
        {
            writer.WriteAttributeString(null, "__type", null, typeInformation);
        }

        protected override bool WriteTypeInfo(XmlWriterDelegator writer, DataContract contract, DataContract declaredContract)
        {
            if (((object.ReferenceEquals(contract.Name, declaredContract.Name) && object.ReferenceEquals(contract.Namespace, declaredContract.Namespace)) || ((contract.Name.Value == declaredContract.Name.Value) && (contract.Namespace.Value == declaredContract.Namespace.Value))) || !(contract.UnderlyingType != Globals.TypeOfObjectArray))
            {
                return false;
            }
            if (RequiresJsonTypeInfo(contract))
            {
                this.perCallXsiTypeAlreadyEmitted = true;
                this.WriteTypeInfo(writer, contract.Name.Value, contract.Namespace.Value);
            }
            else if (declaredContract.UnderlyingType == typeof(Enum))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.Runtime.Serialization.SR.GetString("EnumTypeNotSupportedByDataContractJsonSerializer", new object[] { declaredContract.UnderlyingType })));
            }
            return true;
        }

        protected override void WriteTypeInfo(XmlWriterDelegator writer, string dataContractName, string dataContractNamespace)
        {
            if (string.IsNullOrEmpty(dataContractNamespace))
            {
                this.WriteTypeInfo(writer, dataContractName);
            }
            else
            {
                this.WriteTypeInfo(writer, dataContractName + ":" + TruncateDefaultDataContractNamespace(dataContractNamespace));
            }
        }

        internal XmlDictionaryString CollectionItemName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return JsonGlobals.itemDictionaryString;
            }
        }

        internal IList<Type> SerializerKnownTypeList
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return base.serializerKnownTypeList;
            }
        }
    }
}

