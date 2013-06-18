namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters;
    using System.Security;
    using System.Security.Permissions;

    internal class XmlObjectSerializerReadContextComplex : XmlObjectSerializerReadContext
    {
        private FormatterAssemblyStyle assemblyFormat;
        private SerializationBinder binder;
        protected IDataContractSurrogate dataContractSurrogate;
        private static Hashtable dataContractTypeCache = new Hashtable();
        private SerializationMode mode;
        private bool preserveObjectReferences;
        private Hashtable surrogateDataContracts;
        private ISurrogateSelector surrogateSelector;

        internal XmlObjectSerializerReadContextComplex(NetDataContractSerializer serializer) : base(serializer)
        {
            this.mode = SerializationMode.SharedType;
            this.preserveObjectReferences = true;
            this.binder = serializer.Binder;
            this.surrogateSelector = serializer.SurrogateSelector;
            this.assemblyFormat = serializer.AssemblyFormat;
        }

        internal XmlObjectSerializerReadContextComplex(DataContractSerializer serializer, DataContract rootTypeDataContract, DataContractResolver dataContractResolver) : base(serializer, rootTypeDataContract, dataContractResolver)
        {
            this.mode = SerializationMode.SharedContract;
            this.preserveObjectReferences = serializer.PreserveObjectReferences;
            this.dataContractSurrogate = serializer.DataContractSurrogate;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal XmlObjectSerializerReadContextComplex(XmlObjectSerializer serializer, int maxItemsInObjectGraph, StreamingContext streamingContext, bool ignoreExtensionDataObject) : base(serializer, maxItemsInObjectGraph, streamingContext, ignoreExtensionDataObject)
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
            return (this.surrogateSelector.GetSurrogate(memberType, base.GetStreamingContext(), out selector) != null);
        }

        internal override int GetArraySize()
        {
            if (!this.preserveObjectReferences)
            {
                return -1;
            }
            return base.attributes.ArraySZSize;
        }

        internal override DataContract GetDataContract(int id, RuntimeTypeHandle typeHandle)
        {
            DataContract contract = null;
            if ((this.mode == SerializationMode.SharedType) && (this.surrogateSelector != null))
            {
                contract = NetDataContractSerializer.GetDataContractFromSurrogateSelector(this.surrogateSelector, base.GetStreamingContext(), typeHandle, null, ref this.surrogateDataContracts);
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
                contract = NetDataContractSerializer.GetDataContractFromSurrogateSelector(this.surrogateSelector, base.GetStreamingContext(), typeHandle, type, ref this.surrogateDataContracts);
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

        internal override object InternalDeserialize(XmlReaderDelegator xmlReader, Type declaredType, string name, string ns)
        {
            if (this.mode != SerializationMode.SharedContract)
            {
                return this.InternalDeserializeInSharedTypeMode(xmlReader, -1, declaredType, name, ns);
            }
            if (this.dataContractSurrogate == null)
            {
                return base.InternalDeserialize(xmlReader, declaredType, name, ns);
            }
            return this.InternalDeserializeWithSurrogate(xmlReader, declaredType, null, name, ns);
        }

        public override object InternalDeserialize(XmlReaderDelegator xmlReader, int declaredTypeID, RuntimeTypeHandle declaredTypeHandle, string name, string ns)
        {
            if (this.mode != SerializationMode.SharedContract)
            {
                return this.InternalDeserializeInSharedTypeMode(xmlReader, declaredTypeID, Type.GetTypeFromHandle(declaredTypeHandle), name, ns);
            }
            if (this.dataContractSurrogate == null)
            {
                return base.InternalDeserialize(xmlReader, declaredTypeID, declaredTypeHandle, name, ns);
            }
            return this.InternalDeserializeWithSurrogate(xmlReader, Type.GetTypeFromHandle(declaredTypeHandle), null, name, ns);
        }

        internal override object InternalDeserialize(XmlReaderDelegator xmlReader, Type declaredType, DataContract dataContract, string name, string ns)
        {
            if (this.mode != SerializationMode.SharedContract)
            {
                return this.InternalDeserializeInSharedTypeMode(xmlReader, -1, declaredType, name, ns);
            }
            if (this.dataContractSurrogate == null)
            {
                return base.InternalDeserialize(xmlReader, declaredType, dataContract, name, ns);
            }
            return this.InternalDeserializeWithSurrogate(xmlReader, declaredType, dataContract, name, ns);
        }

        private object InternalDeserializeInSharedTypeMode(XmlReaderDelegator xmlReader, int declaredTypeID, Type declaredType, string name, string ns)
        {
            object retObj = null;
            DataContract contract;
            if (base.TryHandleNullOrRef(xmlReader, declaredType, name, ns, ref retObj))
            {
                return retObj;
            }
            string clrAssembly = base.attributes.ClrAssembly;
            string clrType = base.attributes.ClrType;
            if ((clrAssembly != null) && (clrType != null))
            {
                Assembly assembly;
                Type type;
                contract = this.ResolveDataContractInSharedTypeMode(clrAssembly, clrType, out assembly, out type);
                if (contract == null)
                {
                    if (assembly == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("AssemblyNotFound", new object[] { clrAssembly })));
                    }
                    if (type == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ClrTypeNotFound", new object[] { assembly.FullName, clrType })));
                    }
                }
                if ((declaredType != null) && declaredType.IsArray)
                {
                    contract = (declaredTypeID < 0) ? base.GetDataContract(declaredType) : this.GetDataContract(declaredTypeID, declaredType.TypeHandle);
                }
            }
            else
            {
                if (clrAssembly != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(XmlObjectSerializer.TryAddLineInfo(xmlReader, System.Runtime.Serialization.SR.GetString("AttributeNotFound", new object[] { "http://schemas.microsoft.com/2003/10/Serialization/", "Type", xmlReader.NodeType, xmlReader.NamespaceURI, xmlReader.LocalName }))));
                }
                if (clrType != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(XmlObjectSerializer.TryAddLineInfo(xmlReader, System.Runtime.Serialization.SR.GetString("AttributeNotFound", new object[] { "http://schemas.microsoft.com/2003/10/Serialization/", "Assembly", xmlReader.NodeType, xmlReader.NamespaceURI, xmlReader.LocalName }))));
                }
                if (declaredType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(XmlObjectSerializer.TryAddLineInfo(xmlReader, System.Runtime.Serialization.SR.GetString("AttributeNotFound", new object[] { "http://schemas.microsoft.com/2003/10/Serialization/", "Type", xmlReader.NodeType, xmlReader.NamespaceURI, xmlReader.LocalName }))));
                }
                contract = (declaredTypeID < 0) ? base.GetDataContract(declaredType) : this.GetDataContract(declaredTypeID, declaredType.TypeHandle);
            }
            return this.ReadDataContractValue(contract, xmlReader);
        }

        private object InternalDeserializeWithSurrogate(XmlReaderDelegator xmlReader, Type declaredType, DataContract surrogateDataContract, string name, string ns)
        {
            DataContract dataContract = surrogateDataContract ?? base.GetDataContract(DataContractSurrogateCaller.GetDataContractType(this.dataContractSurrogate, declaredType));
            if (this.IsGetOnlyCollection && (dataContract.UnderlyingType != declaredType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser", new object[] { DataContract.GetClrTypeFullName(declaredType) })));
            }
            this.ReadAttributes(xmlReader);
            string objectId = base.GetObjectId();
            object obj2 = base.InternalDeserialize(xmlReader, name, ns, declaredType, ref dataContract);
            object newObj = DataContractSurrogateCaller.GetDeserializedObject(this.dataContractSurrogate, obj2, dataContract.UnderlyingType, declaredType);
            base.ReplaceDeserializedObject(objectId, obj2, newObj);
            return newObj;
        }

        protected override DataContract ResolveDataContractFromTypeName()
        {
            if (this.mode == SerializationMode.SharedContract)
            {
                return base.ResolveDataContractFromTypeName();
            }
            if ((base.attributes.ClrAssembly != null) && (base.attributes.ClrType != null))
            {
                Assembly assembly;
                Type type;
                return this.ResolveDataContractInSharedTypeMode(base.attributes.ClrAssembly, base.attributes.ClrType, out assembly, out type);
            }
            return null;
        }

        private DataContract ResolveDataContractInSharedTypeMode(string assemblyName, string typeName, out Assembly assembly, out Type type)
        {
            type = this.ResolveDataContractTypeInSharedTypeMode(assemblyName, typeName, out assembly);
            if (type != null)
            {
                return base.GetDataContract(type);
            }
            return null;
        }

        private Type ResolveDataContractTypeInSharedTypeMode(string assemblyName, string typeName, out Assembly assembly)
        {
            assembly = null;
            Type type = null;
            if (this.binder != null)
            {
                type = this.binder.BindToType(assemblyName, typeName);
            }
            if (type != null)
            {
                return type;
            }
            XmlObjectDataContractTypeKey key = new XmlObjectDataContractTypeKey(assemblyName, typeName);
            XmlObjectDataContractTypeInfo info = (XmlObjectDataContractTypeInfo) dataContractTypeCache[key];
            if (info == null)
            {
                if (this.assemblyFormat == FormatterAssemblyStyle.Full)
                {
                    if (assemblyName == "0")
                    {
                        assembly = Globals.TypeOfInt.Assembly;
                    }
                    else
                    {
                        assembly = Assembly.Load(assemblyName);
                    }
                    if (assembly != null)
                    {
                        type = assembly.GetType(typeName);
                    }
                }
                else
                {
                    assembly = ResolveSimpleAssemblyName(assemblyName);
                    if (assembly != null)
                    {
                        try
                        {
                            type = assembly.GetType(typeName);
                        }
                        catch (TypeLoadException)
                        {
                        }
                        catch (FileNotFoundException)
                        {
                        }
                        catch (FileLoadException)
                        {
                        }
                        catch (BadImageFormatException)
                        {
                        }
                        if (type == null)
                        {
                            type = Type.GetType(typeName, new Func<AssemblyName, Assembly>(XmlObjectSerializerReadContextComplex.ResolveSimpleAssemblyName), new Func<Assembly, string, bool, Type>(new TopLevelAssemblyTypeResolver(assembly).ResolveType), false);
                        }
                    }
                }
                if (type == null)
                {
                    return type;
                }
                info = new XmlObjectDataContractTypeInfo(assembly, type);
                lock (dataContractTypeCache)
                {
                    if (!dataContractTypeCache.ContainsKey(key))
                    {
                        dataContractTypeCache[key] = info;
                    }
                    return type;
                }
            }
            assembly = info.Assembly;
            return info.Type;
        }

        private static Assembly ResolveSimpleAssemblyName(AssemblyName assemblyName)
        {
            return ResolveSimpleAssemblyName(assemblyName.FullName);
        }

        private static Assembly ResolveSimpleAssemblyName(string assemblyName)
        {
            if (assemblyName == "0")
            {
                return Globals.TypeOfInt.Assembly;
            }
            Assembly assembly = Assembly.LoadWithPartialName(assemblyName);
            if (assembly == null)
            {
                AssemblyName name = new AssemblyName(assemblyName) {
                    Version = null
                };
                assembly = Assembly.LoadWithPartialName(name.FullName);
            }
            return assembly;
        }

        internal override SerializationMode Mode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.mode;
            }
        }

        private sealed class TopLevelAssemblyTypeResolver
        {
            private Assembly topLevelAssembly;

            public TopLevelAssemblyTypeResolver(Assembly topLevelAssembly)
            {
                this.topLevelAssembly = topLevelAssembly;
            }

            public Type ResolveType(Assembly assembly, string simpleTypeName, bool ignoreCase)
            {
                if (assembly == null)
                {
                    assembly = this.topLevelAssembly;
                }
                return assembly.GetType(simpleTypeName, false, ignoreCase);
            }
        }

        private class XmlObjectDataContractTypeInfo
        {
            private System.Reflection.Assembly assembly;
            private System.Type type;

            public XmlObjectDataContractTypeInfo(System.Reflection.Assembly assembly, System.Type type)
            {
                this.assembly = assembly;
                this.type = type;
            }

            public System.Reflection.Assembly Assembly
            {
                get
                {
                    return this.assembly;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this.type;
                }
            }
        }

        private class XmlObjectDataContractTypeKey
        {
            private string assemblyName;
            private string typeName;

            public XmlObjectDataContractTypeKey(string assemblyName, string typeName)
            {
                this.assemblyName = assemblyName;
                this.typeName = typeName;
            }

            public override bool Equals(object obj)
            {
                if (!object.ReferenceEquals(this, obj))
                {
                    XmlObjectSerializerReadContextComplex.XmlObjectDataContractTypeKey key = obj as XmlObjectSerializerReadContextComplex.XmlObjectDataContractTypeKey;
                    if (key == null)
                    {
                        return false;
                    }
                    if (this.assemblyName != key.assemblyName)
                    {
                        return false;
                    }
                    if (this.typeName != key.typeName)
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                int hashCode = 0;
                if (this.assemblyName != null)
                {
                    hashCode = this.assemblyName.GetHashCode();
                }
                if (this.typeName != null)
                {
                    hashCode ^= this.typeName.GetHashCode();
                }
                return hashCode;
            }
        }
    }
}

