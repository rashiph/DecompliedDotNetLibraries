namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    internal sealed class ComSurrogateSelector : ISurrogateSelector, ISerializationSurrogate
    {
        private ISurrogateSelector _deleg = new RemotingSurrogateSelector();

        public void ChainSelector(ISurrogateSelector next)
        {
            this._deleg.ChainSelector(next);
        }

        public ISurrogateSelector GetNextSelector()
        {
            return this._deleg.GetNextSelector();
        }

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext ctx)
        {
            if (!obj.GetType().IsCOMObject)
            {
                throw new NotSupportedException();
            }
            info.SetType(typeof(ComObjRef));
            info.AddValue("buffer", ComponentServices.GetDCOMBuffer(obj));
        }

        public ISerializationSurrogate GetSurrogate(Type type, StreamingContext ctx, out ISurrogateSelector selector)
        {
            selector = null;
            if (type.IsCOMObject)
            {
                selector = this;
                return this;
            }
            return this._deleg.GetSurrogate(type, ctx, out selector);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext ctx, ISurrogateSelector sel)
        {
            throw new NotSupportedException();
        }
    }
}

