namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;

    internal class JsonEnumDataContract : JsonDataContract
    {
        [SecurityCritical]
        private JsonEnumDataContractCriticalHelper helper;

        [SecuritySafeCritical]
        public JsonEnumDataContract(EnumDataContract traditionalDataContract) : base(new JsonEnumDataContractCriticalHelper(traditionalDataContract))
        {
            this.helper = base.Helper as JsonEnumDataContractCriticalHelper;
        }

        public override object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            object obj2;
            if (this.IsULong)
            {
                obj2 = Enum.ToObject(base.TraditionalDataContract.UnderlyingType, jsonReader.ReadElementContentAsUnsignedLong());
            }
            else
            {
                obj2 = Enum.ToObject(base.TraditionalDataContract.UnderlyingType, jsonReader.ReadElementContentAsLong());
            }
            if (context != null)
            {
                context.AddNewObject(obj2);
            }
            return obj2;
        }

        public override void WriteJsonValueCore(XmlWriterDelegator jsonWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, RuntimeTypeHandle declaredTypeHandle)
        {
            if (this.IsULong)
            {
                jsonWriter.WriteUnsignedLong(((IConvertible) obj).ToUInt64(null));
            }
            else
            {
                jsonWriter.WriteLong(((IConvertible) obj).ToInt64(null));
            }
        }

        public bool IsULong
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.IsULong;
            }
        }

        [SecurityCritical(SecurityCriticalScope.Everything)]
        private class JsonEnumDataContractCriticalHelper : JsonDataContract.JsonDataContractCriticalHelper
        {
            private bool isULong;

            public JsonEnumDataContractCriticalHelper(EnumDataContract traditionalEnumDataContract) : base(traditionalEnumDataContract)
            {
                this.isULong = traditionalEnumDataContract.IsULong;
            }

            public bool IsULong
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.isULong;
                }
            }
        }
    }
}

