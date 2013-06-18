namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    internal class XmlObjectSerializerWriteContextComplex : XmlObjectSerializerWriteContext
    {
        private SerializationBinder binder;
        protected IDataContractSurrogate dataContractSurrogate;
        private SerializationMode mode;
        private StreamingContext streamingContext;
        private Hashtable surrogateDataContracts;
        private ISurrogateSelector surrogateSelector;

        internal XmlObjectSerializerWriteContextComplex(NetDataContractSerializer serializer, Hashtable surrogateDataContracts) : base(serializer)
        {
            this.mode = SerializationMode.SharedType;
            base.preserveObjectReferences = true;
            this.streamingContext = serializer.Context;
            this.binder = serializer.Binder;
            this.surrogateSelector = serializer.SurrogateSelector;
            this.surrogateDataContracts = surrogateDataContracts;
        }

        internal XmlObjectSerializerWriteContextComplex(DataContractSerializer serializer, DataContract rootTypeDataContract, DataContractResolver dataContractResolver) : base(serializer, rootTypeDataContract, dataContractResolver)
        {
            this.mode = SerializationMode.SharedContract;
            base.preserveObjectReferences = serializer.PreserveObjectReferences;
            this.dataContractSurrogate = serializer.DataContractSurrogate;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal XmlObjectSerializerWriteContextComplex(XmlObjectSerializer serializer, int maxItemsInObjectGraph, StreamingContext streamingContext, bool ignoreExtensionDataObject) : base(serializer, maxItemsInObjectGraph, streamingContext, ignoreExtensionDataObject)
        {
        }

        internal override void CheckIfTypeSerializable(Type memberType, bool isMemberTypeSerializable)
        {
            if (((this.mode != SerializationMode.SharedType) || (this.surrogateSelector == null)) || !this.CheckIfTypeSerializableForSharedTypeMode(memberType))
            {
                if (this.dataContractSurrogate != null)
                {
                    while (memberType.IsArray)
                    {
                        memberType = memberType.GetElementType();
                    }
                    memberType = DataContractSurrogateCaller.GetDataContractType(this.dataContractSurrogate, memberType);
                    if (!DataContract.IsTypeSerializable(memberType))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("TypeNotSerializable", new object[] { memberType })));
                    }
                }
                else
                {
                    base.CheckIfTypeSerializable(memberType, isMemberTypeSerializable);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        private bool CheckIfTypeSerializableForSharedTypeMode(Type memberType)
        {
            ISurrogateSelector selector;
            return (this.surrogateSelector.GetSurrogate(memberType, this.streamingContext, out selector) != null);
        }

        internal override DataContract GetDataContract(int id, RuntimeTypeHandle typeHandle)
        {
            DataContract contract = null;
            if ((this.mode == SerializationMode.SharedType) && (this.surrogateSelector != null))
            {
                contract = NetDataContractSerializer.GetDataContractFromSurrogateSelector(this.surrogateSelector, this.streamingContext, typeHandle, null, ref this.surrogateDataContracts);
            }
            if (contract == null)
            {
                return base.GetDataContract(id, typeHandle);
            }
            if (this.IsGetOnlyCollection && (contract is SurrogateDataContract))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser", new object[] { DataContract.GetClrTypeFullName(contract.UnderlyingType) })));
            }
            return contract;
        }

        internal override DataContract GetDataContract(RuntimeTypeHandle typeHandle, Type type)
        {
            DataContract contract = null;
            if ((this.mode == SerializationMode.SharedType) && (this.surrogateSelector != null))
            {
                contract = NetDataContractSerializer.GetDataContractFromSurrogateSelector(this.surrogateSelector, this.streamingContext, typeHandle, type, ref this.surrogateDataContracts);
            }
            if (contract == null)
            {
                return base.GetDataContract(typeHandle, type);
            }
            if (this.IsGetOnlyCollection && (contract is SurrogateDataContract))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser", new object[] { DataContract.GetClrTypeFullName(contract.UnderlyingType) })));
            }
            return contract;
        }

        internal override DataContract GetDataContractSkipValidation(int typeId, RuntimeTypeHandle typeHandle, Type type)
        {
            DataContract contract = null;
            if ((this.mode == SerializationMode.SharedType) && (this.surrogateSelector != null))
            {
                contract = NetDataContractSerializer.GetDataContractFromSurrogateSelector(this.surrogateSelector, this.streamingContext, typeHandle, null, ref this.surrogateDataContracts);
            }
            if (contract == null)
            {
                return base.GetDataContractSkipValidation(typeId, typeHandle, type);
            }
            if (this.IsGetOnlyCollection && (contract is SurrogateDataContract))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser", new object[] { DataContract.GetClrTypeFullName(contract.UnderlyingType) })));
            }
            return contract;
        }

        internal override Type GetSurrogatedType(Type type)
        {
            if (this.dataContractSurrogate == null)
            {
                return base.GetSurrogatedType(type);
            }
            type = DataContract.UnwrapNullableType(type);
            Type surrogatedType = DataContractSerializer.GetSurrogatedType(this.dataContractSurrogate, type);
            if (this.IsGetOnlyCollection && (surrogatedType != type))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser", new object[] { DataContract.GetClrTypeFullName(type) })));
            }
            return surrogatedType;
        }

        public override void InternalSerialize(XmlWriterDelegator xmlWriter, object obj, bool isDeclaredType, bool writeXsiType, int declaredTypeID, RuntimeTypeHandle declaredTypeHandle)
        {
            if (this.dataContractSurrogate == null)
            {
                base.InternalSerialize(xmlWriter, obj, isDeclaredType, writeXsiType, declaredTypeID, declaredTypeHandle);
            }
            else
            {
                this.InternalSerializeWithSurrogate(xmlWriter, obj, isDeclaredType, writeXsiType, declaredTypeID, declaredTypeHandle);
            }
        }

        private void InternalSerializeWithSurrogate(XmlWriterDelegator xmlWriter, object obj, bool isDeclaredType, bool writeXsiType, int declaredTypeID, RuntimeTypeHandle declaredTypeHandle)
        {
            RuntimeTypeHandle typeHandle = isDeclaredType ? declaredTypeHandle : Type.GetTypeHandle(obj);
            object oldObj = obj;
            int oldObjId = 0;
            Type typeFromHandle = Type.GetTypeFromHandle(typeHandle);
            Type surrogatedType = this.GetSurrogatedType(Type.GetTypeFromHandle(declaredTypeHandle));
            declaredTypeHandle = surrogatedType.TypeHandle;
            obj = DataContractSerializer.SurrogateToDataContractType(this.dataContractSurrogate, obj, surrogatedType, ref typeFromHandle);
            typeHandle = typeFromHandle.TypeHandle;
            if (oldObj != obj)
            {
                oldObjId = base.SerializedObjects.ReassignId(0, oldObj, obj);
            }
            if (writeXsiType)
            {
                surrogatedType = Globals.TypeOfObject;
                this.SerializeWithXsiType(xmlWriter, obj, typeHandle, typeFromHandle, -1, surrogatedType.TypeHandle, surrogatedType);
            }
            else if (declaredTypeHandle.Equals(typeHandle))
            {
                DataContract dataContract = this.GetDataContract(typeHandle, typeFromHandle);
                base.SerializeWithoutXsiType(dataContract, xmlWriter, obj, declaredTypeHandle);
            }
            else
            {
                this.SerializeWithXsiType(xmlWriter, obj, typeHandle, typeFromHandle, -1, declaredTypeHandle, surrogatedType);
            }
            if (oldObj != obj)
            {
                base.SerializedObjects.ReassignId(oldObjId, obj, oldObj);
            }
        }

        internal override void OnEndHandleReference(XmlWriterDelegator xmlWriter, object obj, bool canContainCyclicReference)
        {
            if (!base.preserveObjectReferences || this.IsGetOnlyCollection)
            {
                base.OnEndHandleReference(xmlWriter, obj, canContainCyclicReference);
            }
        }

        internal override bool OnHandleReference(XmlWriterDelegator xmlWriter, object obj, bool canContainCyclicReference)
        {
            if (!base.preserveObjectReferences || this.IsGetOnlyCollection)
            {
                return base.OnHandleReference(xmlWriter, obj, canContainCyclicReference);
            }
            bool newId = true;
            int id = base.SerializedObjects.GetId(obj, ref newId);
            if (newId)
            {
                xmlWriter.WriteAttributeInt("z", DictionaryGlobals.IdLocalName, DictionaryGlobals.SerializationNamespace, id);
            }
            else
            {
                xmlWriter.WriteAttributeInt("z", DictionaryGlobals.RefLocalName, DictionaryGlobals.SerializationNamespace, id);
                xmlWriter.WriteAttributeBool("i", DictionaryGlobals.XsiNilLocalName, DictionaryGlobals.SchemaInstanceNamespace, true);
            }
            return !newId;
        }

        public override void WriteAnyType(XmlWriterDelegator xmlWriter, object value)
        {
            if (!this.OnHandleReference(xmlWriter, value, false))
            {
                xmlWriter.WriteAnyType(value);
            }
        }

        internal override void WriteArraySize(XmlWriterDelegator xmlWriter, int size)
        {
            if (base.preserveObjectReferences && (size > -1))
            {
                xmlWriter.WriteAttributeInt("z", DictionaryGlobals.ArraySizeLocalName, DictionaryGlobals.SerializationNamespace, size);
            }
        }

        public override void WriteBase64(XmlWriterDelegator xmlWriter, byte[] value)
        {
            if (!this.OnHandleReference(xmlWriter, value, false))
            {
                xmlWriter.WriteBase64(value);
            }
        }

        public override void WriteBase64(XmlWriterDelegator xmlWriter, byte[] value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (value == null)
            {
                base.WriteNull(xmlWriter, typeof(byte[]), true, name, ns);
            }
            else
            {
                xmlWriter.WriteStartElementPrimitive(name, ns);
                if (!this.OnHandleReference(xmlWriter, value, false))
                {
                    xmlWriter.WriteBase64(value);
                }
                xmlWriter.WriteEndElementPrimitive();
            }
        }

        internal override bool WriteClrTypeInfo(XmlWriterDelegator xmlWriter, DataContract dataContract)
        {
            if (this.mode == SerializationMode.SharedType)
            {
                NetDataContractSerializer.WriteClrTypeInfo(xmlWriter, dataContract, this.binder);
                return true;
            }
            return false;
        }

        internal override bool WriteClrTypeInfo(XmlWriterDelegator xmlWriter, Type dataContractType, SerializationInfo serInfo)
        {
            if (this.mode == SerializationMode.SharedType)
            {
                NetDataContractSerializer.WriteClrTypeInfo(xmlWriter, dataContractType, this.binder, serInfo);
                return true;
            }
            return false;
        }

        internal override bool WriteClrTypeInfo(XmlWriterDelegator xmlWriter, Type dataContractType, string clrTypeName, string clrAssemblyName)
        {
            if (this.mode == SerializationMode.SharedType)
            {
                NetDataContractSerializer.WriteClrTypeInfo(xmlWriter, dataContractType, this.binder, clrTypeName, clrAssemblyName);
                return true;
            }
            return false;
        }

        public override void WriteQName(XmlWriterDelegator xmlWriter, XmlQualifiedName value)
        {
            if (!this.OnHandleReference(xmlWriter, value, false))
            {
                xmlWriter.WriteQName(value);
            }
        }

        public override void WriteQName(XmlWriterDelegator xmlWriter, XmlQualifiedName value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (value == null)
            {
                base.WriteNull(xmlWriter, typeof(XmlQualifiedName), true, name, ns);
            }
            else
            {
                if (((ns != null) && (ns.Value != null)) && (ns.Value.Length > 0))
                {
                    xmlWriter.WriteStartElement("q", name, ns);
                }
                else
                {
                    xmlWriter.WriteStartElement(name, ns);
                }
                if (!this.OnHandleReference(xmlWriter, value, false))
                {
                    xmlWriter.WriteQName(value);
                }
                xmlWriter.WriteEndElement();
            }
        }

        public override void WriteString(XmlWriterDelegator xmlWriter, string value)
        {
            if (!this.OnHandleReference(xmlWriter, value, false))
            {
                xmlWriter.WriteString(value);
            }
        }

        public override void WriteString(XmlWriterDelegator xmlWriter, string value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (value == null)
            {
                base.WriteNull(xmlWriter, typeof(string), true, name, ns);
            }
            else
            {
                xmlWriter.WriteStartElementPrimitive(name, ns);
                if (!this.OnHandleReference(xmlWriter, value, false))
                {
                    xmlWriter.WriteString(value);
                }
                xmlWriter.WriteEndElementPrimitive();
            }
        }

        public override void WriteUri(XmlWriterDelegator xmlWriter, Uri value)
        {
            if (!this.OnHandleReference(xmlWriter, value, false))
            {
                xmlWriter.WriteUri(value);
            }
        }

        public override void WriteUri(XmlWriterDelegator xmlWriter, Uri value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (value == null)
            {
                base.WriteNull(xmlWriter, typeof(Uri), true, name, ns);
            }
            else
            {
                xmlWriter.WriteStartElementPrimitive(name, ns);
                if (!this.OnHandleReference(xmlWriter, value, false))
                {
                    xmlWriter.WriteUri(value);
                }
                xmlWriter.WriteEndElementPrimitive();
            }
        }

        internal override SerializationMode Mode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.mode;
            }
        }
    }
}

