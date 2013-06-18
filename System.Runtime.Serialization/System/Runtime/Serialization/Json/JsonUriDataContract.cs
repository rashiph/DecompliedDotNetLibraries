namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    internal class JsonUriDataContract : JsonDataContract
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public JsonUriDataContract(UriDataContract traditionalUriDataContract) : base(traditionalUriDataContract)
        {
        }

        public override object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            if (context != null)
            {
                return JsonDataContract.HandleReadValue(jsonReader.ReadElementContentAsUri(), context);
            }
            if (!JsonDataContract.TryReadNullAtTopLevel(jsonReader))
            {
                return jsonReader.ReadElementContentAsUri();
            }
            return null;
        }
    }
}

