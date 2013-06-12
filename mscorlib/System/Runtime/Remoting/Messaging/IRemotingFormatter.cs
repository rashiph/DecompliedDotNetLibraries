namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [ComVisible(true)]
    public interface IRemotingFormatter : IFormatter
    {
        object Deserialize(Stream serializationStream, HeaderHandler handler);
        void Serialize(Stream serializationStream, object graph, Header[] headers);
    }
}

