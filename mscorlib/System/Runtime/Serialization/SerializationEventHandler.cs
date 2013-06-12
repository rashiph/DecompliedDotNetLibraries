namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.CompilerServices;

    [Serializable]
    internal delegate void SerializationEventHandler(StreamingContext context);
}

