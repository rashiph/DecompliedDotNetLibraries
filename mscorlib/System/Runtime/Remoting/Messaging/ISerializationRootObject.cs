namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    internal interface ISerializationRootObject
    {
        [SecurityCritical]
        void RootSetObjectData(SerializationInfo info, StreamingContext ctx);
    }
}

