namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization;
    using System.Security;

    internal class ObjRefSurrogate : ISerializationSurrogate
    {
        [SecurityCritical]
        public virtual void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            ((ObjRef) obj).GetObjectData(info, context);
            info.AddValue("fIsMarshalled", 0);
        }

        [SecurityCritical]
        public virtual object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_PopulateData"));
        }
    }
}

