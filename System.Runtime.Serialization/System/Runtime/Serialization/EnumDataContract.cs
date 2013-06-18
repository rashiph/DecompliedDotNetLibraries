namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Security;
    using System.Threading;
    using System.Xml;

    internal sealed class EnumDataContract : DataContract
    {
        [SecurityCritical]
        private EnumDataContractCriticalHelper helper;

        [SecuritySafeCritical]
        internal EnumDataContract() : base(new EnumDataContractCriticalHelper())
        {
            this.helper = base.Helper as EnumDataContractCriticalHelper;
        }

        [SecuritySafeCritical]
        internal EnumDataContract(Type type) : base(new EnumDataContractCriticalHelper(type))
        {
            this.helper = base.Helper as EnumDataContractCriticalHelper;
        }

        internal override bool Equals(object other, Dictionary<DataContractPairKey, object> checkedContracts)
        {
            if (base.IsEqualOrChecked(other, checkedContracts))
            {
                return true;
            }
            if (base.Equals(other, null))
            {
                EnumDataContract contract = other as EnumDataContract;
                if (contract != null)
                {
                    if ((this.Members.Count != contract.Members.Count) || (this.Values.Count != contract.Values.Count))
                    {
                        return false;
                    }
                    string[] array = new string[this.Members.Count];
                    string[] strArray2 = new string[this.Members.Count];
                    for (int i = 0; i < this.Members.Count; i++)
                    {
                        array[i] = this.Members[i].Name;
                        strArray2[i] = contract.Members[i].Name;
                    }
                    Array.Sort<string>(array);
                    Array.Sort<string>(strArray2);
                    for (int j = 0; j < this.Members.Count; j++)
                    {
                        if (array[j] != strArray2[j])
                        {
                            return false;
                        }
                    }
                    return (this.IsFlags == contract.IsFlags);
                }
            }
            return false;
        }

        [SecuritySafeCritical]
        internal static XmlQualifiedName GetBaseContractName(Type type)
        {
            return EnumDataContractCriticalHelper.GetBaseContractName(type);
        }

        [SecuritySafeCritical]
        internal static Type GetBaseType(XmlQualifiedName baseContractName)
        {
            return EnumDataContractCriticalHelper.GetBaseType(baseContractName);
        }

        internal long GetEnumValueFromString(string value)
        {
            if (this.IsULong)
            {
                return (long) XmlConverter.ToUInt64(value);
            }
            return XmlConverter.ToInt64(value);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal string GetStringFromEnumValue(long value)
        {
            if (this.IsULong)
            {
                return XmlConvert.ToString((ulong) value);
            }
            return XmlConvert.ToString(value);
        }

        internal object ReadEnumValue(XmlReaderDelegator reader)
        {
            string str = reader.ReadElementContentAsString();
            long num = 0L;
            int num2 = 0;
            if (!this.IsFlags)
            {
                if (str.Length == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("InvalidEnumValueOnRead", new object[] { str, DataContract.GetClrTypeFullName(base.UnderlyingType) })));
                }
                num = this.ReadEnumValue(str, 0, str.Length);
            }
            else
            {
                while (num2 < str.Length)
                {
                    if (str[num2] != ' ')
                    {
                        break;
                    }
                    num2++;
                }
                int index = num2;
                int count = 0;
                while (num2 < str.Length)
                {
                    if (str[num2] == ' ')
                    {
                        count = num2 - index;
                        if (count > 0)
                        {
                            num |= this.ReadEnumValue(str, index, count);
                        }
                        num2++;
                        while (num2 < str.Length)
                        {
                            if (str[num2] != ' ')
                            {
                                break;
                            }
                            num2++;
                        }
                        index = num2;
                        if (num2 == str.Length)
                        {
                            break;
                        }
                    }
                    num2++;
                }
                count = num2 - index;
                if (count > 0)
                {
                    num |= this.ReadEnumValue(str, index, count);
                }
            }
            if (this.IsULong)
            {
                return Enum.ToObject(base.UnderlyingType, (ulong) num);
            }
            return Enum.ToObject(base.UnderlyingType, num);
        }

        private long ReadEnumValue(string value, int index, int count)
        {
            for (int i = 0; i < this.Members.Count; i++)
            {
                string name = this.Members[i].Name;
                if ((name.Length == count) && (string.CompareOrdinal(value, index, name, 0, count) == 0))
                {
                    return this.Values[i];
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("InvalidEnumValueOnRead", new object[] { value.Substring(index, count), DataContract.GetClrTypeFullName(base.UnderlyingType) })));
        }

        public override object ReadXmlValue(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context)
        {
            object obj2 = this.ReadEnumValue(xmlReader);
            if (context != null)
            {
                context.AddNewObject(obj2);
            }
            return obj2;
        }

        internal void WriteEnumValue(XmlWriterDelegator writer, object value)
        {
            long num = this.IsULong ? ((long) ((IConvertible) value).ToUInt64(null)) : ((IConvertible) value).ToInt64(null);
            for (int i = 0; i < this.Values.Count; i++)
            {
                if (num == this.Values[i])
                {
                    writer.WriteString(this.ChildElementNames[i].Value);
                    return;
                }
            }
            if (!this.IsFlags)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("InvalidEnumValueOnWrite", new object[] { value, DataContract.GetClrTypeFullName(base.UnderlyingType) })));
            }
            int index = -1;
            bool flag = true;
            for (int j = 0; j < this.Values.Count; j++)
            {
                long num5 = this.Values[j];
                if (num5 == 0L)
                {
                    index = j;
                }
                else
                {
                    if (num == 0L)
                    {
                        break;
                    }
                    if ((num5 & num) == num5)
                    {
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            writer.WriteString(DictionaryGlobals.Space.Value);
                        }
                        writer.WriteString(this.ChildElementNames[j].Value);
                        num &= ~num5;
                    }
                }
            }
            if (num != 0L)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("InvalidEnumValueOnWrite", new object[] { value, DataContract.GetClrTypeFullName(base.UnderlyingType) })));
            }
            if (flag && (index >= 0))
            {
                writer.WriteString(this.ChildElementNames[index].Value);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public override void WriteXmlValue(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context)
        {
            this.WriteEnumValue(xmlWriter, obj);
        }

        internal XmlQualifiedName BaseContractName
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.BaseContractName;
            }
            [SecurityCritical]
            set
            {
                this.helper.BaseContractName = value;
            }
        }

        internal override bool CanContainReferences
        {
            get
            {
                return false;
            }
        }

        private XmlDictionaryString[] ChildElementNames
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.ChildElementNames;
            }
        }

        internal bool IsFlags
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.IsFlags;
            }
            [SecurityCritical]
            set
            {
                this.helper.IsFlags = value;
            }
        }

        internal bool IsULong
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.IsULong;
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

        internal List<long> Values
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.Values;
            }
            [SecurityCritical]
            set
            {
                this.helper.Values = value;
            }
        }

        [SecurityCritical(SecurityCriticalScope.Everything)]
        private class EnumDataContractCriticalHelper : DataContract.DataContractCriticalHelper
        {
            private XmlQualifiedName baseContractName;
            private XmlDictionaryString[] childElementNames;
            private bool hasDataContract;
            private bool isFlags;
            private bool isULong;
            private List<DataMember> members;
            private static Dictionary<XmlQualifiedName, Type> nameToType = new Dictionary<XmlQualifiedName, Type>();
            private static Dictionary<Type, XmlQualifiedName> typeToName = new Dictionary<Type, XmlQualifiedName>();
            private List<long> values;

            static EnumDataContractCriticalHelper()
            {
                Add(typeof(sbyte), "byte");
                Add(typeof(byte), "unsignedByte");
                Add(typeof(short), "short");
                Add(typeof(ushort), "unsignedShort");
                Add(typeof(int), "int");
                Add(typeof(uint), "unsignedInt");
                Add(typeof(long), "long");
                Add(typeof(ulong), "unsignedLong");
            }

            internal EnumDataContractCriticalHelper()
            {
                base.IsValueType = true;
            }

            internal EnumDataContractCriticalHelper(Type type) : base(type)
            {
                DataContractAttribute attribute;
                base.StableName = DataContract.GetStableName(type, out this.hasDataContract);
                Type underlyingType = Enum.GetUnderlyingType(type);
                this.baseContractName = GetBaseContractName(underlyingType);
                this.ImportBaseType(underlyingType);
                this.IsFlags = type.IsDefined(Globals.TypeOfFlagsAttribute, false);
                this.ImportDataMembers();
                XmlDictionary dictionary = new XmlDictionary(2 + this.Members.Count);
                base.Name = dictionary.Add(base.StableName.Name);
                base.Namespace = dictionary.Add(base.StableName.Namespace);
                this.childElementNames = new XmlDictionaryString[this.Members.Count];
                for (int i = 0; i < this.Members.Count; i++)
                {
                    this.childElementNames[i] = dictionary.Add(this.Members[i].Name);
                }
                if (DataContract.TryGetDCAttribute(type, out attribute) && attribute.IsReference)
                {
                    DataContract.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("EnumTypeCannotHaveIsReference", new object[] { DataContract.GetClrTypeFullName(type), attribute.IsReference, false }), type);
                }
            }

            internal static void Add(Type type, string localName)
            {
                XmlQualifiedName name = DataContract.CreateQualifiedName(localName, "http://www.w3.org/2001/XMLSchema");
                typeToName.Add(type, name);
                nameToType.Add(name, type);
            }

            internal static XmlQualifiedName GetBaseContractName(Type type)
            {
                XmlQualifiedName name = null;
                typeToName.TryGetValue(type, out name);
                return name;
            }

            internal static Type GetBaseType(XmlQualifiedName baseContractName)
            {
                Type type = null;
                nameToType.TryGetValue(baseContractName, out type);
                return type;
            }

            private void ImportBaseType(Type baseType)
            {
                this.isULong = baseType == Globals.TypeOfULong;
            }

            private void ImportDataMembers()
            {
                Type underlyingType = base.UnderlyingType;
                FieldInfo[] fields = underlyingType.GetFields(BindingFlags.Public | BindingFlags.Static);
                Dictionary<string, DataMember> memberNamesTable = new Dictionary<string, DataMember>();
                List<DataMember> members = new List<DataMember>(fields.Length);
                List<long> list2 = new List<long>(fields.Length);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo memberInfo = fields[i];
                    bool flag = false;
                    if (this.hasDataContract)
                    {
                        object[] customAttributes = memberInfo.GetCustomAttributes(Globals.TypeOfEnumMemberAttribute, false);
                        if ((customAttributes != null) && (customAttributes.Length > 0))
                        {
                            if (customAttributes.Length > 1)
                            {
                                base.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("TooManyEnumMembers", new object[] { DataContract.GetClrTypeFullName(memberInfo.DeclaringType), memberInfo.Name }));
                            }
                            EnumMemberAttribute attribute = (EnumMemberAttribute) customAttributes[0];
                            DataMember memberContract = new DataMember(memberInfo);
                            if (attribute.IsValueSetExplicit)
                            {
                                if ((attribute.Value == null) || (attribute.Value.Length == 0))
                                {
                                    base.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidEnumMemberValue", new object[] { memberInfo.Name, DataContract.GetClrTypeFullName(underlyingType) }));
                                }
                                memberContract.Name = attribute.Value;
                            }
                            else
                            {
                                memberContract.Name = memberInfo.Name;
                            }
                            ClassDataContract.CheckAndAddMember(members, memberContract, memberNamesTable);
                            flag = true;
                        }
                        object[] objArray2 = memberInfo.GetCustomAttributes(Globals.TypeOfDataMemberAttribute, false);
                        if ((objArray2 != null) && (objArray2.Length > 0))
                        {
                            base.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("DataMemberOnEnumField", new object[] { DataContract.GetClrTypeFullName(memberInfo.DeclaringType), memberInfo.Name }));
                        }
                    }
                    else if (!memberInfo.IsNotSerialized)
                    {
                        DataMember member2 = new DataMember(memberInfo) {
                            Name = memberInfo.Name
                        };
                        ClassDataContract.CheckAndAddMember(members, member2, memberNamesTable);
                        flag = true;
                    }
                    if (flag)
                    {
                        object obj2 = memberInfo.GetValue(null);
                        if (this.isULong)
                        {
                            list2.Add((long) ((IConvertible) obj2).ToUInt64(null));
                        }
                        else
                        {
                            list2.Add(((IConvertible) obj2).ToInt64(null));
                        }
                    }
                }
                Thread.MemoryBarrier();
                this.members = members;
                this.values = list2;
            }

            internal XmlQualifiedName BaseContractName
            {
                get
                {
                    return this.baseContractName;
                }
                set
                {
                    this.baseContractName = value;
                    Type baseType = GetBaseType(this.baseContractName);
                    if (baseType == null)
                    {
                        base.ThrowInvalidDataContractException(System.Runtime.Serialization.SR.GetString("InvalidEnumBaseType", new object[] { value.Name, value.Namespace, base.StableName.Name, base.StableName.Namespace }));
                    }
                    this.ImportBaseType(baseType);
                }
            }

            internal XmlDictionaryString[] ChildElementNames
            {
                get
                {
                    return this.childElementNames;
                }
            }

            internal bool IsFlags
            {
                get
                {
                    return this.isFlags;
                }
                set
                {
                    this.isFlags = value;
                }
            }

            internal bool IsULong
            {
                get
                {
                    return this.isULong;
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

            internal List<long> Values
            {
                get
                {
                    return this.values;
                }
                set
                {
                    this.values = value;
                }
            }
        }
    }
}

