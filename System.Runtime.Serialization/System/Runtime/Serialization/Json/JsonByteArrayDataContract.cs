namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    internal class JsonByteArrayDataContract : JsonDataContract
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public JsonByteArrayDataContract(ByteArrayDataContract traditionalByteArrayDataContract) : base(traditionalByteArrayDataContract)
        {
        }

        public override object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            if (context != null)
            {
                return JsonDataContract.HandleReadValue(jsonReader.ReadElementContentAsBase64(), context);
            }
            if (!JsonDataContract.TryReadNullAtTopLevel(jsonReader))
            {
                return jsonReader.ReadElementContentAsBase64();
            }
            return null;
        }
    }
}

