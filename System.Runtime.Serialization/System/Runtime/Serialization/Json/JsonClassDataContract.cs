namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;
    using System.Xml;

    internal class JsonClassDataContract : JsonDataContract
    {
        [SecurityCritical]
        private JsonClassDataContractCriticalHelper helper;

        [SecuritySafeCritical]
        public JsonClassDataContract(ClassDataContract traditionalDataContract) : base(new JsonClassDataContractCriticalHelper(traditionalDataContract))
        {
            this.helper = base.Helper as JsonClassDataContractCriticalHelper;
        }

        public override object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            jsonReader.Read();
            object obj2 = this.JsonFormatReaderDelegate(jsonReader, context, XmlDictionaryString.Empty, this.MemberNames);
            jsonReader.ReadEndElement();
            return obj2;
        }

        public override void WriteJsonValueCore(XmlWriterDelegator jsonWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, RuntimeTypeHandle declaredTypeHandle)
        {
            jsonWriter.WriteAttributeString(null, "type", null, "object");
            this.JsonFormatWriterDelegate(jsonWriter, obj, context, this.TraditionalClassDataContract, this.MemberNames);
        }

        internal JsonFormatClassReaderDelegate JsonFormatReaderDelegate
        {
            [SecuritySafeCritical]
            get
            {
                if (this.helper.JsonFormatReaderDelegate == null)
                {
                    lock (this)
                    {
                        if (this.helper.JsonFormatReaderDelegate == null)
                        {
                            JsonFormatClassReaderDelegate delegate2 = new JsonFormatReaderGenerator().GenerateClassReader(this.TraditionalClassDataContract);
                            Thread.MemoryBarrier();
                            this.helper.JsonFormatReaderDelegate = delegate2;
                        }
                    }
                }
                return this.helper.JsonFormatReaderDelegate;
            }
        }

        internal JsonFormatClassWriterDelegate JsonFormatWriterDelegate
        {
            [SecuritySafeCritical]
            get
            {
                if (this.helper.JsonFormatWriterDelegate == null)
                {
                    lock (this)
                    {
                        if (this.helper.JsonFormatWriterDelegate == null)
                        {
                            JsonFormatClassWriterDelegate delegate2 = new JsonFormatWriterGenerator().GenerateClassWriter(this.TraditionalClassDataContract);
                            Thread.MemoryBarrier();
                            this.helper.JsonFormatWriterDelegate = delegate2;
                        }
                    }
                }
                return this.helper.JsonFormatWriterDelegate;
            }
        }

        internal XmlDictionaryString[] MemberNames
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.MemberNames;
            }
        }

        private ClassDataContract TraditionalClassDataContract
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.TraditionalClassDataContract;
            }
        }

        internal override string TypeName
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.TypeName;
            }
        }

        [SecurityCritical(SecurityCriticalScope.Everything)]
        private class JsonClassDataContractCriticalHelper : JsonDataContract.JsonDataContractCriticalHelper
        {
            private JsonFormatClassReaderDelegate jsonFormatReaderDelegate;
            private JsonFormatClassWriterDelegate jsonFormatWriterDelegate;
            private XmlDictionaryString[] memberNames;
            private ClassDataContract traditionalClassDataContract;
            private string typeName;

            public JsonClassDataContractCriticalHelper(ClassDataContract traditionalDataContract) : base(traditionalDataContract)
            {
                this.typeName = string.IsNullOrEmpty(traditionalDataContract.Namespace.Value) ? traditionalDataContract.Name.Value : (traditionalDataContract.Name.Value + ":" + XmlObjectSerializerWriteContextComplexJson.TruncateDefaultDataContractNamespace(traditionalDataContract.Namespace.Value));
                this.traditionalClassDataContract = traditionalDataContract;
                this.CopyMembersAndCheckDuplicateNames();
            }

            private void CopyMembersAndCheckDuplicateNames()
            {
                if (this.traditionalClassDataContract.MemberNames != null)
                {
                    int length = this.traditionalClassDataContract.MemberNames.Length;
                    Dictionary<string, object> dictionary = new Dictionary<string, object>(length);
                    XmlDictionaryString[] strArray = new XmlDictionaryString[length];
                    for (int i = 0; i < length; i++)
                    {
                        if (dictionary.ContainsKey(this.traditionalClassDataContract.MemberNames[i].Value))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.Runtime.Serialization.SR.GetString("JsonDuplicateMemberNames", new object[] { DataContract.GetClrTypeFullName(this.traditionalClassDataContract.UnderlyingType), this.traditionalClassDataContract.MemberNames[i].Value })));
                        }
                        dictionary.Add(this.traditionalClassDataContract.MemberNames[i].Value, null);
                        strArray[i] = DataContractJsonSerializer.ConvertXmlNameToJsonName(this.traditionalClassDataContract.MemberNames[i]);
                    }
                    this.memberNames = strArray;
                }
            }

            internal JsonFormatClassReaderDelegate JsonFormatReaderDelegate
            {
                get
                {
                    return this.jsonFormatReaderDelegate;
                }
                set
                {
                    this.jsonFormatReaderDelegate = value;
                }
            }

            internal JsonFormatClassWriterDelegate JsonFormatWriterDelegate
            {
                get
                {
                    return this.jsonFormatWriterDelegate;
                }
                set
                {
                    this.jsonFormatWriterDelegate = value;
                }
            }

            internal XmlDictionaryString[] MemberNames
            {
                get
                {
                    return this.memberNames;
                }
            }

            internal ClassDataContract TraditionalClassDataContract
            {
                get
                {
                    return this.traditionalClassDataContract;
                }
            }
        }
    }
}

