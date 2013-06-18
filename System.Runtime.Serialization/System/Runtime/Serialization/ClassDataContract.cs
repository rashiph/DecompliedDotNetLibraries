namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using System.Xml;

    internal sealed class ClassDataContract : DataContract
    {
        [SecurityCritical]
        private XmlDictionaryString[] childElementNamespaces;
        public XmlDictionaryString[] ContractNamespaces;
        [SecurityCritical]
        private ClassDataContractCriticalHelper helper;
        public XmlDictionaryString[] MemberNames;
        public XmlDictionaryString[] MemberNamespaces;

        [SecuritySafeCritical]
        internal ClassDataContract() : base(new ClassDataContractCriticalHelper())
        {
            this.InitClassDataContract();
        }

        [SecuritySafeCritical]
        internal ClassDataContract(Type type) : base(new ClassDataContractCriticalHelper(type))
        {
            this.InitClassDataContract();
        }

        [SecuritySafeCritical]
        private ClassDataContract(Type type, XmlDictionaryString ns, string[] memberNames) : base(new ClassDataContractCriticalHelper(type, ns, memberNames))
        {
            this.InitClassDataContract();
        }

        [SecuritySafeCritical]
        internal override DataContract BindGenericParameters(DataContract[] paramContracts, Dictionary<DataContract, DataContract> boundContracts)
        {
            Type underlyingType = base.UnderlyingType;
            if (!underlyingType.IsGenericType || !underlyingType.ContainsGenericParameters)
            {
                return this;
            }
            lock (this)
            {
                DataContract contract;
                XmlQualifiedName stableName;
                object[] objArray;
                if (boundContracts.TryGetValue(this, out contract))
                {
                    return contract;
                }
                ClassDataContract contract2 = new ClassDataContract();
                boundContracts.Add(this, contract2);
                if (underlyingType.IsGenericTypeDefinition)
                {
                    stableName = base.StableName;
                    objArray = paramContracts;
                }
                else
                {
                    stableName = DataContract.GetStableName(underlyingType.GetGenericTypeDefinition());
                    Type[] genericArguments = underlyingType.GetGenericArguments();
                    objArray = new object[genericArguments.Length];
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        Type type2 = genericArguments[i];
                        if (type2.IsGenericParameter)
                        {
                            objArray[i] = paramContracts[type2.GenericParameterPosition];
                        }
                        else
                        {
                            objArray[i] = type2;
                        }
                    }
                }
                contract2.StableName = DataContract.CreateQualifiedName(DataContract.ExpandGenericParameters(XmlConvert.DecodeName(stableName.Name), new GenericNameProvider(DataContract.GetClrTypeFullName(base.UnderlyingType), objArray)), stableName.Namespace);
                if (this.BaseContract != null)
                {
                    contract2.BaseContract = (ClassDataContract) this.BaseContract.BindGenericParameters(paramContracts, boundContracts);
                }
                contract2.IsISerializable = this.IsISerializable;
                contract2.IsValueType = base.IsValueType;
                if (this.Members != null)
                {
                    contract2.Members = new List<DataMember>(this.Members.Count);
                    foreach (DataMember member in this.Members)
                    {
                        contract2.Members.Add(member.BindGenericParameters(paramContracts, boundContracts));
                    }
                }
                return contract2;
            }
        }

        internal static void CheckAndAddMember(List<DataMember> members, DataMember memberContract, Dictionary<string, DataMember> memberNamesTable)
        {
            DataMember member;
            if (memberNamesTable.TryGetValue(memberContract.Name, out member))
            {
                Type declaringType = memberContract.MemberInfo.DeclaringType;
                DataContract.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString(declaringType.IsEnum ? "DupEnumMemberValue" : "DupMemberName", new object[] { member.MemberInfo.Name, memberContract.MemberInfo.Name, DataContract.GetClrTypeFullName(declaringType), memberContract.Name }), declaringType);
            }
            memberNamesTable.Add(memberContract.Name, memberContract);
            members.Add(memberContract);
        }

        private XmlDictionaryString[] CreateChildElementNamespaces()
        {
            if (this.Members == null)
            {
                return null;
            }
            XmlDictionaryString[] sourceArray = null;
            if (this.BaseContract != null)
            {
                sourceArray = this.BaseContract.ChildElementNamespaces;
            }
            int num = (sourceArray != null) ? sourceArray.Length : 0;
            XmlDictionaryString[] destinationArray = new XmlDictionaryString[this.Members.Count + num];
            if (num > 0)
            {
                Array.Copy(sourceArray, 0, destinationArray, 0, sourceArray.Length);
            }
            XmlDictionary dictionary = new XmlDictionary();
            for (int i = 0; i < this.Members.Count; i++)
            {
                destinationArray[i + num] = GetChildNamespaceToDeclare(this, this.Members[i].MemberType, dictionary);
            }
            return destinationArray;
        }

        internal static ClassDataContract CreateClassDataContractForKeyValue(Type type, XmlDictionaryString ns, string[] memberNames)
        {
            return new ClassDataContract(type, ns, memberNames);
        }

        [SecuritySafeCritical]
        private void EnsureMethodsImported()
        {
            this.helper.EnsureMethodsImported();
        }

        internal override bool Equals(object other, Dictionary<DataContractPairKey, object> checkedContracts)
        {
            if (base.IsEqualOrChecked(other, checkedContracts))
            {
                return true;
            }
            if (base.Equals(other, checkedContracts))
            {
                ClassDataContract contract = other as ClassDataContract;
                if (contract != null)
                {
                    if (this.IsISerializable)
                    {
                        if (!contract.IsISerializable)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (contract.IsISerializable)
                        {
                            return false;
                        }
                        if (this.Members == null)
                        {
                            if ((contract.Members != null) && !this.IsEveryDataMemberOptional(contract.Members))
                            {
                                return false;
                            }
                        }
                        else if (contract.Members == null)
                        {
                            if (!this.IsEveryDataMemberOptional(this.Members))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            Dictionary<string, DataMember> dictionary = new Dictionary<string, DataMember>(this.Members.Count);
                            List<DataMember> dataMembers = new List<DataMember>();
                            for (int i = 0; i < this.Members.Count; i++)
                            {
                                dictionary.Add(this.Members[i].Name, this.Members[i]);
                            }
                            for (int j = 0; j < contract.Members.Count; j++)
                            {
                                DataMember member;
                                if (dictionary.TryGetValue(contract.Members[j].Name, out member))
                                {
                                    if (!member.Equals(contract.Members[j], checkedContracts))
                                    {
                                        return false;
                                    }
                                    dictionary.Remove(member.Name);
                                }
                                else
                                {
                                    dataMembers.Add(contract.Members[j]);
                                }
                            }
                            if (!this.IsEveryDataMemberOptional(dictionary.Values))
                            {
                                return false;
                            }
                            if (!this.IsEveryDataMemberOptional(dataMembers))
                            {
                                return false;
                            }
                        }
                    }
                    if (this.BaseContract == null)
                    {
                        return (contract.BaseContract == null);
                    }
                    if (contract.BaseContract == null)
                    {
                        return false;
                    }
                    return this.BaseContract.Equals(contract.BaseContract, checkedContracts);
                }
            }
            return false;
        }

        internal static XmlDictionaryString GetChildNamespaceToDeclare(DataContract dataContract, Type childType, XmlDictionary dictionary)
        {
            childType = DataContract.UnwrapNullableType(childType);
            if ((!childType.IsEnum && !Globals.TypeOfIXmlSerializable.IsAssignableFrom(childType)) && ((DataContract.GetBuiltInDataContract(childType) == null) && (childType != Globals.TypeOfDBNull)))
            {
                string str = DataContract.GetStableName(childType).Namespace;
                if ((str.Length > 0) && (str != dataContract.Namespace.Value))
                {
                    return dictionary.Add(str);
                }
            }
            return null;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SecuritySafeCritical]
        internal ConstructorInfo GetISerializableConstructor()
        {
            return this.helper.GetISerializableConstructor();
        }

        [SecuritySafeCritical]
        internal ConstructorInfo GetNonAttributedTypeConstructor()
        {
            return this.helper.GetNonAttributedTypeConstructor();
        }

        [SecurityCritical]
        private void InitClassDataContract()
        {
            this.helper = base.Helper as ClassDataContractCriticalHelper;
            this.ContractNamespaces = this.helper.ContractNamespaces;
            this.MemberNames = this.helper.MemberNames;
            this.MemberNamespaces = this.helper.MemberNamespaces;
        }

        private bool IsEveryDataMemberOptional(IEnumerable<DataMember> dataMembers)
        {
            foreach (DataMember member in dataMembers)
            {
                if (member.IsRequired)
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsNonAttributedTypeValidForSerialization(Type type)
        {
            if (type.IsArray)
            {
                return false;
            }
            if (type.IsEnum)
            {
                return false;
            }
            if (type.IsGenericParameter)
            {
                return false;
            }
            if (Globals.TypeOfIXmlSerializable.IsAssignableFrom(type))
            {
                return false;
            }
            if (type.IsPointer)
            {
                return false;
            }
            if (type.IsDefined(Globals.TypeOfCollectionDataContractAttribute, false))
            {
                return false;
            }
            foreach (Type type2 in type.GetInterfaces())
            {
                if (CollectionDataContract.IsCollectionInterface(type2))
                {
                    return false;
                }
            }
            if (type.IsSerializable)
            {
                return false;
            }
            if (Globals.TypeOfISerializable.IsAssignableFrom(type))
            {
                return false;
            }
            if (type.IsDefined(Globals.TypeOfDataContractAttribute, false))
            {
                return false;
            }
            if (type == Globals.TypeOfExtensionDataObject)
            {
                return false;
            }
            if (type.IsValueType)
            {
                return type.IsVisible;
            }
            return (type.IsVisible && (type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Globals.EmptyTypeArray, null) != null));
        }

        public override object ReadXmlValue(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context)
        {
            xmlReader.Read();
            object obj2 = this.XmlFormatReaderDelegate(xmlReader, context, this.MemberNames, this.MemberNamespaces);
            xmlReader.ReadEndElement();
            return obj2;
        }

        internal bool RequiresMemberAccessForRead(SecurityException securityException)
        {
            this.EnsureMethodsImported();
            if (!DataContract.IsTypeVisible(base.UnderlyingType))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustDataContractTypeNotPublic", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType) }), securityException));
                }
                return true;
            }
            if ((this.BaseContract != null) && this.BaseContract.RequiresMemberAccessForRead(securityException))
            {
                return true;
            }
            if (DataContract.ConstructorRequiresMemberAccess(this.GetISerializableConstructor()))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustISerializableNoPublicConstructor", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType) }), securityException));
                }
                return true;
            }
            if (DataContract.ConstructorRequiresMemberAccess(this.GetNonAttributedTypeConstructor()))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustNonAttributedSerializableTypeNoPublicConstructor", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType) }), securityException));
                }
                return true;
            }
            if (DataContract.MethodRequiresMemberAccess(this.OnDeserializing))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustDataContractOnDeserializingNotPublic", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType), this.OnDeserializing.Name }), securityException));
                }
                return true;
            }
            if (DataContract.MethodRequiresMemberAccess(this.OnDeserialized))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustDataContractOnDeserializedNotPublic", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType), this.OnDeserialized.Name }), securityException));
                }
                return true;
            }
            if (this.Members != null)
            {
                for (int i = 0; i < this.Members.Count; i++)
                {
                    if (this.Members[i].RequiresMemberAccessForSet())
                    {
                        if (securityException == null)
                        {
                            return true;
                        }
                        if (this.Members[i].MemberInfo is FieldInfo)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustDataContractFieldSetNotPublic", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType), this.Members[i].MemberInfo.Name }), securityException));
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustDataContractPropertySetNotPublic", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType), this.Members[i].MemberInfo.Name }), securityException));
                    }
                }
            }
            return false;
        }

        internal bool RequiresMemberAccessForWrite(SecurityException securityException)
        {
            this.EnsureMethodsImported();
            if (!DataContract.IsTypeVisible(base.UnderlyingType))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustDataContractTypeNotPublic", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType) }), securityException));
                }
                return true;
            }
            if ((this.BaseContract != null) && this.BaseContract.RequiresMemberAccessForWrite(securityException))
            {
                return true;
            }
            if (DataContract.MethodRequiresMemberAccess(this.OnSerializing))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustDataContractOnSerializingNotPublic", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType), this.OnSerializing.Name }), securityException));
                }
                return true;
            }
            if (DataContract.MethodRequiresMemberAccess(this.OnSerialized))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustDataContractOnSerializedNotPublic", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType), this.OnSerialized.Name }), securityException));
                }
                return true;
            }
            if (this.Members != null)
            {
                for (int i = 0; i < this.Members.Count; i++)
                {
                    if (this.Members[i].RequiresMemberAccessForGet())
                    {
                        if (securityException == null)
                        {
                            return true;
                        }
                        if (this.Members[i].MemberInfo is FieldInfo)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustDataContractFieldGetNotPublic", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType), this.Members[i].MemberInfo.Name }), securityException));
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.Runtime.Serialization.SR.GetString("PartialTrustDataContractPropertyGetNotPublic", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType), this.Members[i].MemberInfo.Name }), securityException));
                    }
                }
            }
            return false;
        }

        public override void WriteXmlValue(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context)
        {
            this.XmlFormatWriterDelegate(xmlWriter, obj, context, this);
        }

        internal ClassDataContract BaseContract
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.BaseContract;
            }
            [SecurityCritical]
            set
            {
                this.helper.BaseContract = value;
            }
        }

        public XmlDictionaryString[] ChildElementNamespaces
        {
            [SecuritySafeCritical]
            get
            {
                if (this.childElementNamespaces == null)
                {
                    lock (this)
                    {
                        if (this.childElementNamespaces == null)
                        {
                            if (this.helper.ChildElementNamespaces == null)
                            {
                                XmlDictionaryString[] strArray = this.CreateChildElementNamespaces();
                                Thread.MemoryBarrier();
                                this.helper.ChildElementNamespaces = strArray;
                            }
                            this.childElementNamespaces = this.helper.ChildElementNamespaces;
                        }
                    }
                }
                return this.childElementNamespaces;
            }
        }

        internal MethodInfo ExtensionDataSetMethod
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.ExtensionDataSetMethod;
            }
        }

        internal bool HasDataContract
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.HasDataContract;
            }
        }

        internal bool HasExtensionData
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.HasExtensionData;
            }
        }

        internal override bool IsISerializable
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.IsISerializable;
            }
            [SecurityCritical]
            set
            {
                this.helper.IsISerializable = value;
            }
        }

        internal bool IsNonAttributedType
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.IsNonAttributedType;
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

        internal List<DataMember> Members
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.Members;
            }
            [SecurityCritical]
            set
            {
                this.helper.Members = value;
            }
        }

        internal MethodInfo OnDeserialized
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.OnDeserialized;
            }
        }

        internal MethodInfo OnDeserializing
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.OnDeserializing;
            }
        }

        internal MethodInfo OnSerialized
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.OnSerialized;
            }
        }

        internal MethodInfo OnSerializing
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.OnSerializing;
            }
        }

        internal XmlFormatClassReaderDelegate XmlFormatReaderDelegate
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
                            XmlFormatClassReaderDelegate delegate2 = new XmlFormatReaderGenerator().GenerateClassReader(this);
                            Thread.MemoryBarrier();
                            this.helper.XmlFormatReaderDelegate = delegate2;
                        }
                    }
                }
                return this.helper.XmlFormatReaderDelegate;
            }
        }

        internal XmlFormatClassWriterDelegate XmlFormatWriterDelegate
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
                            XmlFormatClassWriterDelegate delegate2 = new XmlFormatWriterGenerator().GenerateClassWriter(this);
                            Thread.MemoryBarrier();
                            this.helper.XmlFormatWriterDelegate = delegate2;
                        }
                    }
                }
                return this.helper.XmlFormatWriterDelegate;
            }
        }

        [SecurityCritical(SecurityCriticalScope.Everything)]
        private class ClassDataContractCriticalHelper : DataContract.DataContractCriticalHelper
        {
            private ClassDataContract baseContract;
            private XmlDictionaryString[] childElementNamespaces;
            public XmlDictionaryString[] ContractNamespaces;
            private MethodInfo extensionDataSetMethod;
            private bool hasDataContract;
            private bool hasExtensionData;
            private bool isISerializable;
            private bool isKnownTypeAttributeChecked;
            private bool isMethodChecked;
            private bool isNonAttributedType;
            private Dictionary<XmlQualifiedName, DataContract> knownDataContracts;
            public XmlDictionaryString[] MemberNames;
            public XmlDictionaryString[] MemberNamespaces;
            private List<DataMember> members;
            private MethodInfo onDeserialized;
            private MethodInfo onDeserializing;
            private MethodInfo onSerialized;
            private MethodInfo onSerializing;
            private static Type[] serInfoCtorArgs;
            private XmlFormatClassReaderDelegate xmlFormatReaderDelegate;
            private XmlFormatClassWriterDelegate xmlFormatWriterDelegate;

            internal ClassDataContractCriticalHelper()
            {
            }

            internal ClassDataContractCriticalHelper(Type type) : base(type)
            {
                XmlQualifiedName stableNameAndSetHasDataContract = this.GetStableNameAndSetHasDataContract(type);
                if (type == Globals.TypeOfDBNull)
                {
                    base.StableName = stableNameAndSetHasDataContract;
                    this.members = new List<DataMember>();
                    XmlDictionary dictionary = new XmlDictionary(2);
                    base.Name = dictionary.Add(base.StableName.Name);
                    base.Namespace = dictionary.Add(base.StableName.Namespace);
                    this.ContractNamespaces = this.MemberNames = this.MemberNamespaces = new XmlDictionaryString[0];
                    this.EnsureMethodsImported();
                }
                else
                {
                    Type baseType = type.BaseType;
                    this.isISerializable = Globals.TypeOfISerializable.IsAssignableFrom(type);
                    this.SetIsNonAttributedType(type);
                    if (this.isISerializable)
                    {
                        if (this.HasDataContract)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("ISerializableCannotHaveDataContract", new object[] { DataContract.GetClrTypeFullName(type) })));
                        }
                        if ((baseType != null) && (!baseType.IsSerializable || !Globals.TypeOfISerializable.IsAssignableFrom(baseType)))
                        {
                            baseType = null;
                        }
                    }
                    base.IsValueType = type.IsValueType;
                    if (((baseType != null) && (baseType != Globals.TypeOfObject)) && ((baseType != Globals.TypeOfValueType) && (baseType != Globals.TypeOfUri)))
                    {
                        DataContract dataContract = DataContract.GetDataContract(baseType);
                        if (dataContract is CollectionDataContract)
                        {
                            this.BaseContract = ((CollectionDataContract) dataContract).SharedTypeContract as ClassDataContract;
                        }
                        else
                        {
                            this.BaseContract = dataContract as ClassDataContract;
                        }
                        if (((this.BaseContract != null) && this.BaseContract.IsNonAttributedType) && !this.isNonAttributedType)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("AttributedTypesCannotInheritFromNonAttributedSerializableTypes", new object[] { DataContract.GetClrTypeFullName(type), DataContract.GetClrTypeFullName(baseType) })));
                        }
                    }
                    else
                    {
                        this.BaseContract = null;
                    }
                    this.hasExtensionData = Globals.TypeOfIExtensibleDataObject.IsAssignableFrom(type);
                    if ((this.hasExtensionData && !this.HasDataContract) && !this.IsNonAttributedType)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("OnlyDataContractTypesCanHaveExtensionData", new object[] { DataContract.GetClrTypeFullName(type) })));
                    }
                    if (this.isISerializable)
                    {
                        base.SetDataContractName(stableNameAndSetHasDataContract);
                    }
                    else
                    {
                        base.StableName = stableNameAndSetHasDataContract;
                        this.ImportDataMembers();
                        XmlDictionary dictionary2 = new XmlDictionary(2 + this.Members.Count);
                        base.Name = dictionary2.Add(base.StableName.Name);
                        base.Namespace = dictionary2.Add(base.StableName.Namespace);
                        int length = 0;
                        int num2 = 0;
                        if (this.BaseContract == null)
                        {
                            this.MemberNames = new XmlDictionaryString[this.Members.Count];
                            this.MemberNamespaces = new XmlDictionaryString[this.Members.Count];
                            this.ContractNamespaces = new XmlDictionaryString[1];
                        }
                        else
                        {
                            length = this.BaseContract.MemberNames.Length;
                            this.MemberNames = new XmlDictionaryString[this.Members.Count + length];
                            Array.Copy(this.BaseContract.MemberNames, this.MemberNames, length);
                            this.MemberNamespaces = new XmlDictionaryString[this.Members.Count + length];
                            Array.Copy(this.BaseContract.MemberNamespaces, this.MemberNamespaces, length);
                            num2 = this.BaseContract.ContractNamespaces.Length;
                            this.ContractNamespaces = new XmlDictionaryString[1 + num2];
                            Array.Copy(this.BaseContract.ContractNamespaces, this.ContractNamespaces, num2);
                        }
                        this.ContractNamespaces[num2] = base.Namespace;
                        for (int i = 0; i < this.Members.Count; i++)
                        {
                            this.MemberNames[i + length] = dictionary2.Add(this.Members[i].Name);
                            this.MemberNamespaces[i + length] = base.Namespace;
                        }
                    }
                    this.EnsureMethodsImported();
                }
            }

            internal ClassDataContractCriticalHelper(Type type, XmlDictionaryString ns, string[] memberNames) : base(type)
            {
                base.StableName = new XmlQualifiedName(this.GetStableNameAndSetHasDataContract(type).Name, ns.Value);
                this.ImportDataMembers();
                XmlDictionary dictionary = new XmlDictionary(1 + this.Members.Count);
                base.Name = dictionary.Add(base.StableName.Name);
                base.Namespace = ns;
                this.ContractNamespaces = new XmlDictionaryString[] { base.Namespace };
                this.MemberNames = new XmlDictionaryString[this.Members.Count];
                this.MemberNamespaces = new XmlDictionaryString[this.Members.Count];
                for (int i = 0; i < this.Members.Count; i++)
                {
                    this.Members[i].Name = memberNames[i];
                    this.MemberNames[i] = dictionary.Add(this.Members[i].Name);
                    this.MemberNamespaces[i] = base.Namespace;
                }
                this.EnsureMethodsImported();
            }

            private void EnsureIsReferenceImported(Type type)
            {
                DataContractAttribute attribute;
                bool flag = false;
                bool flag2 = DataContract.TryGetDCAttribute(type, out attribute);
                if (this.BaseContract != null)
                {
                    if (flag2 && attribute.IsReferenceSetExplicit)
                    {
                        bool isReference = this.BaseContract.IsReference;
                        if ((isReference && !attribute.IsReference) || (!isReference && attribute.IsReference))
                        {
                            DataContract.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("InconsistentIsReference", new object[] { DataContract.GetClrTypeFullName(type), attribute.IsReference, DataContract.GetClrTypeFullName(this.BaseContract.UnderlyingType), this.BaseContract.IsReference }), type);
                        }
                        else
                        {
                            flag = attribute.IsReference;
                        }
                    }
                    else
                    {
                        flag = this.BaseContract.IsReference;
                    }
                }
                else if (flag2 && attribute.IsReference)
                {
                    flag = attribute.IsReference;
                }
                if (flag && type.IsValueType)
                {
                    DataContract.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("ValueTypeCannotHaveIsReference", new object[] { DataContract.GetClrTypeFullName(type), true, false }), type);
                }
                else
                {
                    base.IsReference = flag;
                }
            }

            internal void EnsureMethodsImported()
            {
                if (!this.isMethodChecked && (base.UnderlyingType != null))
                {
                    lock (this)
                    {
                        if (!this.isMethodChecked)
                        {
                            foreach (MethodInfo info in base.UnderlyingType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                            {
                                Type prevAttributeType = null;
                                ParameterInfo[] parameters = info.GetParameters();
                                if (this.HasExtensionData && this.IsValidExtensionDataSetMethod(info, parameters))
                                {
                                    if ((info.Name == "System.Runtime.Serialization.IExtensibleDataObject.set_ExtensionData") || !info.IsPublic)
                                    {
                                        this.extensionDataSetMethod = XmlFormatGeneratorStatics.ExtensionDataSetExplicitMethodInfo;
                                    }
                                    else
                                    {
                                        this.extensionDataSetMethod = info;
                                    }
                                }
                                if (IsValidCallback(info, parameters, Globals.TypeOfOnSerializingAttribute, this.onSerializing, ref prevAttributeType))
                                {
                                    this.onSerializing = info;
                                }
                                if (IsValidCallback(info, parameters, Globals.TypeOfOnSerializedAttribute, this.onSerialized, ref prevAttributeType))
                                {
                                    this.onSerialized = info;
                                }
                                if (IsValidCallback(info, parameters, Globals.TypeOfOnDeserializingAttribute, this.onDeserializing, ref prevAttributeType))
                                {
                                    this.onDeserializing = info;
                                }
                                if (IsValidCallback(info, parameters, Globals.TypeOfOnDeserializedAttribute, this.onDeserialized, ref prevAttributeType))
                                {
                                    this.onDeserialized = info;
                                }
                            }
                            Thread.MemoryBarrier();
                            this.isMethodChecked = true;
                        }
                    }
                }
            }

            internal ConstructorInfo GetISerializableConstructor()
            {
                if (!this.IsISerializable)
                {
                    return null;
                }
                ConstructorInfo info = base.UnderlyingType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, SerInfoCtorArgs, null);
                if (info == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("SerializationInfo_ConstructorNotFound", new object[] { DataContract.GetClrTypeFullName(base.UnderlyingType) })));
                }
                return info;
            }

            internal ConstructorInfo GetNonAttributedTypeConstructor()
            {
                if (!this.IsNonAttributedType)
                {
                    return null;
                }
                Type underlyingType = base.UnderlyingType;
                if (underlyingType.IsValueType)
                {
                    return null;
                }
                ConstructorInfo info = underlyingType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Globals.EmptyTypeArray, null);
                if (info == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(System.Runtime.Serialization.SR.GetString("NonAttributedSerializableTypesMustHaveDefaultConstructor", new object[] { DataContract.GetClrTypeFullName(underlyingType) })));
                }
                return info;
            }

            [SecuritySafeCritical]
            private XmlQualifiedName GetStableNameAndSetHasDataContract(Type type)
            {
                return DataContract.GetStableName(type, out this.hasDataContract);
            }

            private void ImportDataMembers()
            {
                MemberInfo[] infoArray;
                Type underlyingType = base.UnderlyingType;
                this.EnsureIsReferenceImported(underlyingType);
                List<DataMember> members = new List<DataMember>();
                Dictionary<string, DataMember> memberNamesTable = new Dictionary<string, DataMember>();
                if (this.isNonAttributedType)
                {
                    infoArray = underlyingType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                }
                else
                {
                    infoArray = underlyingType.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                }
                for (int i = 0; i < infoArray.Length; i++)
                {
                    FieldInfo info9;
                    MemberInfo memberInfo = infoArray[i];
                    if (this.HasDataContract)
                    {
                        object[] objArray = memberInfo.GetCustomAttributes(typeof(DataMemberAttribute), false);
                        if ((objArray != null) && (objArray.Length > 0))
                        {
                            if (objArray.Length > 1)
                            {
                                base.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("TooManyDataMembers", new object[] { DataContract.GetClrTypeFullName(memberInfo.DeclaringType), memberInfo.Name }));
                            }
                            DataMember member = new DataMember(memberInfo);
                            if (memberInfo.MemberType == MemberTypes.Property)
                            {
                                PropertyInfo info2 = (PropertyInfo) memberInfo;
                                MethodInfo method = info2.GetGetMethod(true);
                                if ((method != null) && IsMethodOverriding(method))
                                {
                                    continue;
                                }
                                MethodInfo info4 = info2.GetSetMethod(true);
                                if ((info4 != null) && IsMethodOverriding(info4))
                                {
                                    continue;
                                }
                                if (method == null)
                                {
                                    base.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("NoGetMethodForProperty", new object[] { info2.DeclaringType, info2.Name }));
                                }
                                if ((info4 == null) && !this.SetIfGetOnlyCollection(member))
                                {
                                    base.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("NoSetMethodForProperty", new object[] { info2.DeclaringType, info2.Name }));
                                }
                                if (method.GetParameters().Length > 0)
                                {
                                    base.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("IndexedPropertyCannotBeSerialized", new object[] { info2.DeclaringType, info2.Name }));
                                }
                            }
                            else if (memberInfo.MemberType != MemberTypes.Field)
                            {
                                base.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidMember", new object[] { DataContract.GetClrTypeFullName(underlyingType), memberInfo.Name }));
                            }
                            DataMemberAttribute attribute = (DataMemberAttribute) objArray[0];
                            if (attribute.IsNameSetExplicit)
                            {
                                if ((attribute.Name == null) || (attribute.Name.Length == 0))
                                {
                                    base.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidDataMemberName", new object[] { memberInfo.Name, DataContract.GetClrTypeFullName(underlyingType) }));
                                }
                                member.Name = attribute.Name;
                            }
                            else
                            {
                                member.Name = memberInfo.Name;
                            }
                            member.Name = DataContract.EncodeLocalName(member.Name);
                            member.IsNullable = DataContract.IsTypeNullable(member.MemberType);
                            member.IsRequired = attribute.IsRequired;
                            if (attribute.IsRequired && base.IsReference)
                            {
                                DataContract.DataContractCriticalHelper.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("IsRequiredDataMemberOnIsReferenceDataContractType", new object[] { DataContract.GetClrTypeFullName(memberInfo.DeclaringType), memberInfo.Name, true }), underlyingType);
                            }
                            member.EmitDefaultValue = attribute.EmitDefaultValue;
                            member.Order = attribute.Order;
                            ClassDataContract.CheckAndAddMember(members, member, memberNamesTable);
                        }
                        continue;
                    }
                    if (!this.isNonAttributedType)
                    {
                        goto Label_04BA;
                    }
                    FieldInfo info5 = memberInfo as FieldInfo;
                    PropertyInfo info6 = memberInfo as PropertyInfo;
                    if (((info5 == null) && (info6 == null)) || ((info5 != null) && info5.IsInitOnly))
                    {
                        continue;
                    }
                    object[] customAttributes = memberInfo.GetCustomAttributes(typeof(IgnoreDataMemberAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                    {
                        if (customAttributes.Length <= 1)
                        {
                            continue;
                        }
                        base.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("TooManyIgnoreDataMemberAttributes", new object[] { DataContract.GetClrTypeFullName(memberInfo.DeclaringType), memberInfo.Name }));
                    }
                    DataMember memberContract = new DataMember(memberInfo);
                    if (info6 == null)
                    {
                        goto Label_0486;
                    }
                    MethodInfo getMethod = info6.GetGetMethod();
                    if (((getMethod == null) || IsMethodOverriding(getMethod)) || (getMethod.GetParameters().Length > 0))
                    {
                        continue;
                    }
                    MethodInfo setMethod = info6.GetSetMethod(true);
                    if (setMethod == null)
                    {
                        if (this.SetIfGetOnlyCollection(memberContract))
                        {
                            goto Label_0455;
                        }
                        continue;
                    }
                    if (!setMethod.IsPublic || IsMethodOverriding(setMethod))
                    {
                        continue;
                    }
                Label_0455:
                    if ((this.hasExtensionData && (memberContract.MemberType == Globals.TypeOfExtensionDataObject)) && (memberInfo.Name == "ExtensionData"))
                    {
                        continue;
                    }
                Label_0486:
                    memberContract.Name = DataContract.EncodeLocalName(memberInfo.Name);
                    memberContract.IsNullable = DataContract.IsTypeNullable(memberContract.MemberType);
                    ClassDataContract.CheckAndAddMember(members, memberContract, memberNamesTable);
                    continue;
                Label_04BA:
                    info9 = memberInfo as FieldInfo;
                    if ((info9 != null) && !info9.IsNotSerialized)
                    {
                        DataMember member3 = new DataMember(memberInfo) {
                            Name = DataContract.EncodeLocalName(memberInfo.Name)
                        };
                        object[] objArray3 = info9.GetCustomAttributes(Globals.TypeOfOptionalFieldAttribute, false);
                        if ((objArray3 == null) || (objArray3.Length == 0))
                        {
                            if (base.IsReference)
                            {
                                DataContract.DataContractCriticalHelper.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("NonOptionalFieldMemberOnIsReferenceSerializableType", new object[] { DataContract.GetClrTypeFullName(memberInfo.DeclaringType), memberInfo.Name, true }), underlyingType);
                            }
                            member3.IsRequired = true;
                        }
                        member3.IsNullable = DataContract.IsTypeNullable(member3.MemberType);
                        ClassDataContract.CheckAndAddMember(members, member3, memberNamesTable);
                    }
                }
                if (members.Count > 1)
                {
                    members.Sort(ClassDataContract.DataMemberComparer.Singleton);
                }
                this.SetIfMembersHaveConflict(members);
                Thread.MemoryBarrier();
                this.members = members;
            }

            private static bool IsMethodOverriding(MethodInfo method)
            {
                return (method.IsVirtual && ((method.Attributes & MethodAttributes.NewSlot) == MethodAttributes.PrivateScope));
            }

            private static bool IsValidCallback(MethodInfo method, ParameterInfo[] parameters, Type attributeType, MethodInfo currentCallback, ref Type prevAttributeType)
            {
                if (!method.IsDefined(attributeType, false))
                {
                    return false;
                }
                if (currentCallback != null)
                {
                    DataContract.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("DuplicateCallback", new object[] { method, currentCallback, DataContract.GetClrTypeFullName(method.DeclaringType), attributeType }), method.DeclaringType);
                }
                else if (prevAttributeType != null)
                {
                    DataContract.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("DuplicateAttribute", new object[] { prevAttributeType, attributeType, DataContract.GetClrTypeFullName(method.DeclaringType), method }), method.DeclaringType);
                }
                else if (method.IsVirtual)
                {
                    DataContract.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("CallbacksCannotBeVirtualMethods", new object[] { method, DataContract.GetClrTypeFullName(method.DeclaringType), attributeType }), method.DeclaringType);
                }
                else
                {
                    if (method.ReturnType != Globals.TypeOfVoid)
                    {
                        DataContract.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("CallbackMustReturnVoid", new object[] { DataContract.GetClrTypeFullName(method.DeclaringType), method }), method.DeclaringType);
                    }
                    if (((parameters == null) || (parameters.Length != 1)) || (parameters[0].ParameterType != Globals.TypeOfStreamingContext))
                    {
                        DataContract.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("CallbackParameterInvalid", new object[] { DataContract.GetClrTypeFullName(method.DeclaringType), method, Globals.TypeOfStreamingContext }), method.DeclaringType);
                    }
                    prevAttributeType = attributeType;
                }
                return true;
            }

            private bool IsValidExtensionDataSetMethod(MethodInfo method, ParameterInfo[] parameters)
            {
                if (!(method.Name == "System.Runtime.Serialization.IExtensibleDataObject.set_ExtensionData") && !(method.Name == "set_ExtensionData"))
                {
                    return false;
                }
                if (this.extensionDataSetMethod != null)
                {
                    base.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("DuplicateExtensionDataSetMethod", new object[] { method, this.extensionDataSetMethod, DataContract.GetClrTypeFullName(method.DeclaringType) }));
                }
                if (method.ReturnType != Globals.TypeOfVoid)
                {
                    DataContract.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("ExtensionDataSetMustReturnVoid", new object[] { DataContract.GetClrTypeFullName(method.DeclaringType), method }), method.DeclaringType);
                }
                if (((parameters == null) || (parameters.Length != 1)) || (parameters[0].ParameterType != Globals.TypeOfExtensionDataObject))
                {
                    DataContract.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("ExtensionDataSetParameterInvalid", new object[] { DataContract.GetClrTypeFullName(method.DeclaringType), method, Globals.TypeOfExtensionDataObject }), method.DeclaringType);
                }
                return true;
            }

            private bool SetIfGetOnlyCollection(DataMember memberContract)
            {
                if (CollectionDataContract.IsCollection(memberContract.MemberType, false) && !memberContract.MemberType.IsValueType)
                {
                    memberContract.IsGetOnlyCollection = true;
                    return true;
                }
                return false;
            }

            private void SetIfMembersHaveConflict(List<DataMember> members)
            {
                if (this.BaseContract != null)
                {
                    int baseTypeIndex = 0;
                    List<Member> list = new List<Member>();
                    foreach (DataMember member in members)
                    {
                        list.Add(new Member(member, base.StableName.Namespace, baseTypeIndex));
                    }
                    for (ClassDataContract contract = this.BaseContract; contract != null; contract = contract.BaseContract)
                    {
                        baseTypeIndex++;
                        foreach (DataMember member2 in contract.Members)
                        {
                            list.Add(new Member(member2, contract.StableName.Namespace, baseTypeIndex));
                        }
                    }
                    IComparer<Member> singleton = DataMemberConflictComparer.Singleton;
                    list.Sort(singleton);
                    for (int i = 0; i < (list.Count - 1); i++)
                    {
                        int num3 = i;
                        int num4 = i;
                        bool flag = false;
                        while (((num4 < (list.Count - 1)) && (string.CompareOrdinal(list[num4].member.Name, list[num4 + 1].member.Name) == 0)) && (string.CompareOrdinal(list[num4].ns, list[num4 + 1].ns) == 0))
                        {
                            list[num4].member.ConflictingMember = list[num4 + 1].member;
                            if (!flag)
                            {
                                if (list[num4 + 1].member.HasConflictingNameAndType)
                                {
                                    flag = true;
                                }
                                else
                                {
                                    flag = list[num4].member.MemberType != list[num4 + 1].member.MemberType;
                                }
                            }
                            num4++;
                        }
                        if (flag)
                        {
                            for (int j = num3; j <= num4; j++)
                            {
                                list[j].member.HasConflictingNameAndType = true;
                            }
                        }
                        i = num4 + 1;
                    }
                }
            }

            private void SetIsNonAttributedType(Type type)
            {
                this.isNonAttributedType = (!type.IsSerializable && !this.hasDataContract) && ClassDataContract.IsNonAttributedTypeValidForSerialization(type);
            }

            internal ClassDataContract BaseContract
            {
                get
                {
                    return this.baseContract;
                }
                set
                {
                    this.baseContract = value;
                    if ((this.baseContract != null) && base.IsValueType)
                    {
                        base.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("ValueTypeCannotHaveBaseType", new object[] { base.StableName.Name, base.StableName.Namespace, this.baseContract.StableName.Name, this.baseContract.StableName.Namespace }));
                    }
                }
            }

            public XmlDictionaryString[] ChildElementNamespaces
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.childElementNamespaces;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.childElementNamespaces = value;
                }
            }

            internal MethodInfo ExtensionDataSetMethod
            {
                get
                {
                    this.EnsureMethodsImported();
                    return this.extensionDataSetMethod;
                }
            }

            internal bool HasDataContract
            {
                get
                {
                    return this.hasDataContract;
                }
            }

            internal bool HasExtensionData
            {
                get
                {
                    return this.hasExtensionData;
                }
            }

            internal override bool IsISerializable
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.isISerializable;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.isISerializable = value;
                }
            }

            internal bool IsNonAttributedType
            {
                get
                {
                    return this.isNonAttributedType;
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

            internal List<DataMember> Members
            {
                get
                {
                    return this.members;
                }
                set
                {
                    this.members = value;
                }
            }

            internal MethodInfo OnDeserialized
            {
                get
                {
                    this.EnsureMethodsImported();
                    return this.onDeserialized;
                }
            }

            internal MethodInfo OnDeserializing
            {
                get
                {
                    this.EnsureMethodsImported();
                    return this.onDeserializing;
                }
            }

            internal MethodInfo OnSerialized
            {
                get
                {
                    this.EnsureMethodsImported();
                    return this.onSerialized;
                }
            }

            internal MethodInfo OnSerializing
            {
                get
                {
                    this.EnsureMethodsImported();
                    return this.onSerializing;
                }
            }

            private static Type[] SerInfoCtorArgs
            {
                get
                {
                    if (serInfoCtorArgs == null)
                    {
                        serInfoCtorArgs = new Type[] { typeof(SerializationInfo), typeof(StreamingContext) };
                    }
                    return serInfoCtorArgs;
                }
            }

            internal XmlFormatClassReaderDelegate XmlFormatReaderDelegate
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

            internal XmlFormatClassWriterDelegate XmlFormatWriterDelegate
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

            internal class DataMemberConflictComparer : IComparer<ClassDataContract.ClassDataContractCriticalHelper.Member>
            {
                internal static ClassDataContract.ClassDataContractCriticalHelper.DataMemberConflictComparer Singleton = new ClassDataContract.ClassDataContractCriticalHelper.DataMemberConflictComparer();

                public int Compare(ClassDataContract.ClassDataContractCriticalHelper.Member x, ClassDataContract.ClassDataContractCriticalHelper.Member y)
                {
                    int num = string.CompareOrdinal(x.ns, y.ns);
                    if (num != 0)
                    {
                        return num;
                    }
                    int num2 = string.CompareOrdinal(x.member.Name, y.member.Name);
                    if (num2 != 0)
                    {
                        return num2;
                    }
                    return (x.baseTypeIndex - y.baseTypeIndex);
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct Member
            {
                internal DataMember member;
                internal string ns;
                internal int baseTypeIndex;
                internal Member(DataMember member, string ns, int baseTypeIndex)
                {
                    this.member = member;
                    this.ns = ns;
                    this.baseTypeIndex = baseTypeIndex;
                }
            }
        }

        internal class DataMemberComparer : IComparer<DataMember>
        {
            internal static ClassDataContract.DataMemberComparer Singleton = new ClassDataContract.DataMemberComparer();

            public int Compare(DataMember x, DataMember y)
            {
                int num = x.Order - y.Order;
                if (num != 0)
                {
                    return num;
                }
                return string.CompareOrdinal(x.Name, y.Name);
            }
        }
    }
}

