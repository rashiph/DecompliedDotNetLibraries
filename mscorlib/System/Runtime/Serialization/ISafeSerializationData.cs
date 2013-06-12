namespace System.Runtime.Serialization
{
    using System;

    public interface ISafeSerializationData
    {
        void CompleteDeserialization(object deserialized);
    }
}

