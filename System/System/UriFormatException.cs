namespace System
{
    using System.Runtime.Serialization;

    [Serializable]
    public class UriFormatException : FormatException, ISerializable
    {
        public UriFormatException()
        {
        }

        public UriFormatException(string textString) : base(textString)
        {
        }

        protected UriFormatException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public UriFormatException(string textString, Exception e) : base(textString, e)
        {
        }

        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
        }
    }
}

