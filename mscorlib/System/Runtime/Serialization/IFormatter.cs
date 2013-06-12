namespace System.Runtime.Serialization
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IFormatter
    {
        object Deserialize(Stream serializationStream);
        void Serialize(Stream serializationStream, object graph);

        SerializationBinder Binder { get; set; }

        StreamingContext Context { get; set; }

        ISurrogateSelector SurrogateSelector { get; set; }
    }
}

