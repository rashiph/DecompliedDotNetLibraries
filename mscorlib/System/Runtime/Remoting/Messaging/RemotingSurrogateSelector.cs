namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [SecurityCritical, ComVisible(true), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    public class RemotingSurrogateSelector : ISurrogateSelector
    {
        private MessageSurrogateFilter _filter;
        private ISerializationSurrogate _messageSurrogate;
        private ISurrogateSelector _next;
        private ObjRefSurrogate _objRefSurrogate = new ObjRefSurrogate();
        private RemotingSurrogate _remotingSurrogate = new RemotingSurrogate();
        private object _rootObj;
        private static Type s_IMethodCallMessageType = typeof(IMethodCallMessage);
        private static Type s_IMethodReturnMessageType = typeof(IMethodReturnMessage);
        private static Type s_ObjRefType = typeof(ObjRef);

        public RemotingSurrogateSelector()
        {
            this._messageSurrogate = new MessageSurrogate(this);
        }

        [SecurityCritical]
        public virtual void ChainSelector(ISurrogateSelector selector)
        {
            this._next = selector;
        }

        [SecurityCritical]
        public virtual ISurrogateSelector GetNextSelector()
        {
            return this._next;
        }

        public object GetRootObject()
        {
            return this._rootObj;
        }

        [SecurityCritical]
        public virtual ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector ssout)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (type.IsMarshalByRef)
            {
                ssout = this;
                return this._remotingSurrogate;
            }
            if (s_IMethodCallMessageType.IsAssignableFrom(type) || s_IMethodReturnMessageType.IsAssignableFrom(type))
            {
                ssout = this;
                return this._messageSurrogate;
            }
            if (s_ObjRefType.IsAssignableFrom(type))
            {
                ssout = this;
                return this._objRefSurrogate;
            }
            if (this._next != null)
            {
                return this._next.GetSurrogate(type, context, out ssout);
            }
            ssout = null;
            return null;
        }

        public void SetRootObject(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            this._rootObj = obj;
            SoapMessageSurrogate surrogate = this._messageSurrogate as SoapMessageSurrogate;
            if (surrogate != null)
            {
                surrogate.SetRootObject(this._rootObj);
            }
        }

        public virtual void UseSoapFormat()
        {
            this._messageSurrogate = new SoapMessageSurrogate(this);
            ((SoapMessageSurrogate) this._messageSurrogate).SetRootObject(this._rootObj);
        }

        public MessageSurrogateFilter Filter
        {
            get
            {
                return this._filter;
            }
            set
            {
                this._filter = value;
            }
        }
    }
}

