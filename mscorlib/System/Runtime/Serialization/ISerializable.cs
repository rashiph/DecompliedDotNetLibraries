namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface ISerializable
    {
        [SecurityCritical]
        void GetObjectData(SerializationInfo info, StreamingContext context);
    }
}

