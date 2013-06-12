namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization;
    using System.Security;

    internal class RemotingSurrogate : ISerializationSurrogate
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
            if (RemotingServices.IsTransparentProxy(obj))
            {
                RemotingServices.GetRealProxy(obj).GetObjectData(info, context);
            }
            else
            {
                RemotingServices.GetObjectData(obj, info, context);
            }
        }

        [SecurityCritical]
        public virtual object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_PopulateData"));
        }
    }
}

