namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;
    using System.Xml;

    internal class XmlObjectSerializerReadContextComplexJson : XmlObjectSerializerReadContextComplex
    {
        private string extensionDataValueType;

        public XmlObjectSerializerReadContextComplexJson(DataContractJsonSerializer serializer, DataContract rootTypeDataContract) : base(serializer, serializer.MaxItemsInObjectGraph, new StreamingContext(StreamingContextStates.All), serializer.IgnoreExtensionDataObject)
        {
            base.rootTypeDataContract = rootTypeDataContract;
            base.serializerKnownTypeList = serializer.knownTypeList;
            base.dataContractSurrogate = serializer.DataContractSurrogate;
        }

        internal static XmlObjectSerializerReadContextComplexJson CreateContext(DataContractJsonSerializer serializer, DataContract rootTypeDataContract)
        {
            return new XmlObjectSerializerReadContextComplexJson(serializer, rootTypeDataContract);
        }

        protected override XmlReaderDelegator CreateReaderDelegatorForReader(XmlReader xmlReader)
        {
            return new JsonReaderDelegator(xmlReader);
        }

        internal override int GetArraySize()
        {
            return -1;
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

        public int GetJsonMemberIndex(XmlReaderDelegator xmlReader, XmlDictionaryString[] memberNames, int memberIndex, ExtensionDataObject extensionData)
        {
            int length = memberNames.Length;
            if (length != 0)
            {
                string str;
                int num2 = 0;
                for (int i = (memberIndex + 1) % length; num2 < length; i = (i + 1) % length)
                {
                    if (xmlReader.IsStartElement(memberNames[i], XmlDictionaryString.Empty))
                    {
                        return i;
                    }
                    num2++;
                }
                if (TryGetJsonLocalName(xmlReader, out str))
                {
                    int num4 = 0;
                    for (int j = (memberIndex + 1) % length; num4 < length; j = (j + 1) % length)
                    {
                        if (memberNames[j].Value == str)
                        {
                            return j;
                        }
                        num4++;
                    }
                }
            }
            base.HandleMemberNotFound(xmlReader, extensionData, memberIndex);
            return length;
        }

        [SecuritySafeCritical]
        private static bool IsBitSet(byte[] bytes, int bitIndex)
        {
            return BitFlagsGenerator.IsBitSet(bytes, bitIndex);
        }

        protected override bool IsReadingClassExtensionData(XmlReaderDelegator xmlReader)
        {
            return (xmlReader.GetAttribute("type") == "object");
        }

        protected override bool IsReadingCollectionExtensionData(XmlReaderDelegator xmlReader)
        {
            return (xmlReader.GetAttribute("type") == "array");
        }

        internal override void ReadAttributes(XmlReaderDelegator xmlReader)
        {
            if (base.attributes == null)
            {
                base.attributes = new Attributes();
            }
            base.attributes.Reset();
            if (xmlReader.MoveToAttribute("type") && (xmlReader.Value == "null"))
            {
                base.attributes.XsiNil = true;
            }
            else if (xmlReader.MoveToAttribute("__type"))
            {
                XmlQualifiedName name = JsonReaderDelegator.ParseQualifiedName(xmlReader.Value);
                base.attributes.XsiTypeName = name.Name;
                string str = name.Namespace;
                if (!string.IsNullOrEmpty(str))
                {
                    switch (str[0])
                    {
                        case '#':
                            str = "http://schemas.datacontract.org/2004/07/" + str.Substring(1);
                            break;

                        case '\\':
                            if (str.Length >= 2)
                            {
                                switch (str[1])
                                {
                                    case '#':
                                    case '\\':
                                        str = str.Substring(1);
                                        break;
                                }
                            }
                            break;
                    }
                }
                base.attributes.XsiTypeNamespace = str;
            }
            xmlReader.MoveToElement();
        }

        protected override object ReadDataContractValue(DataContract dataContract, XmlReaderDelegator reader)
        {
            return DataContractJsonSerializer.ReadJsonValue(dataContract, reader, this);
        }

        private IDataNode ReadNumericalPrimitiveExtensionDataValue(XmlReaderDelegator xmlReader)
        {
            TypeCode code;
            object obj2 = JsonObjectDataContract.ParseJsonNumber(xmlReader.ReadContentAsString(), out code);
            switch (code)
            {
                case TypeCode.SByte:
                    return new DataNode<sbyte>((sbyte) obj2);

                case TypeCode.Byte:
                    return new DataNode<byte>((byte) obj2);

                case TypeCode.Int16:
                    return new DataNode<short>((short) obj2);

                case TypeCode.UInt16:
                    return new DataNode<ushort>((ushort) obj2);

                case TypeCode.Int32:
                    return new DataNode<int>((int) obj2);

                case TypeCode.UInt32:
                    return new DataNode<uint>((uint) obj2);

                case TypeCode.Int64:
                    return new DataNode<long>((long) obj2);

                case TypeCode.UInt64:
                    return new DataNode<ulong>((ulong) obj2);

                case TypeCode.Single:
                    return new DataNode<float>((float) obj2);

                case TypeCode.Double:
                    return new DataNode<double>((double) obj2);

                case TypeCode.Decimal:
                    return new DataNode<decimal>((decimal) obj2);
            }
            throw Fx.AssertAndThrow("JsonObjectDataContract.ParseJsonNumber shouldn't return a TypeCode that we're not expecting");
        }

        protected override IDataNode ReadPrimitiveExtensionDataValue(XmlReaderDelegator xmlReader, string dataContractName, string dataContractNamespace)
        {
            IDataNode node;
            string str;
            if (((str = this.extensionDataValueType) != null) && !(str == "string"))
            {
                if (str != "boolean")
                {
                    if (str != "number")
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("JsonUnexpectedAttributeValue", new object[] { this.extensionDataValueType })));
                    }
                    node = this.ReadNumericalPrimitiveExtensionDataValue(xmlReader);
                }
                else
                {
                    node = new DataNode<bool>(xmlReader.ReadContentAsBoolean());
                }
            }
            else
            {
                node = new DataNode<string>(xmlReader.ReadContentAsString());
            }
            xmlReader.ReadEndElement();
            return node;
        }

        protected override void StartReadExtensionDataValue(XmlReaderDelegator xmlReader)
        {
            this.extensionDataValueType = xmlReader.GetAttribute("type");
        }

        public static void ThrowDuplicateMemberException(object obj, XmlDictionaryString[] memberNames, int memberIndex)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.Runtime.Serialization.SR.GetString("JsonDuplicateMemberInInput", new object[] { DataContract.GetClrTypeFullName(obj.GetType()), memberNames[memberIndex] })));
        }

        public static void ThrowMissingRequiredMembers(object obj, XmlDictionaryString[] memberNames, byte[] expectedElements, byte[] requiredElements)
        {
            StringBuilder builder = new StringBuilder();
            int num = 0;
            for (int i = 0; i < memberNames.Length; i++)
            {
                if (IsBitSet(expectedElements, i) && IsBitSet(requiredElements, i))
                {
                    if (builder.Length != 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(memberNames[i]);
                    num++;
                }
            }
            if (num == 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.Runtime.Serialization.SR.GetString("JsonOneRequiredMemberNotFound", new object[] { DataContract.GetClrTypeFullName(obj.GetType()), builder.ToString() })));
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.Runtime.Serialization.SR.GetString("JsonRequiredMembersNotFound", new object[] { DataContract.GetClrTypeFullName(obj.GetType()), builder.ToString() })));
        }

        internal static bool TryGetJsonLocalName(XmlReaderDelegator xmlReader, out string name)
        {
            if (xmlReader.IsStartElement(JsonGlobals.itemDictionaryString, JsonGlobals.itemDictionaryString) && xmlReader.MoveToAttribute("item"))
            {
                name = xmlReader.Value;
                return true;
            }
            name = null;
            return false;
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

