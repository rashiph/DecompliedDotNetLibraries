namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Security;

    internal class RedirectionProxy : MarshalByRefObject, IMessageSink
    {
        private WellKnownObjectMode _objectMode;
        private MarshalByRefObject _proxy;
        [SecurityCritical]
        private RealProxy _realProxy;
        private Type _serverType;

        [SecurityCritical]
        internal RedirectionProxy(MarshalByRefObject proxy, Type serverType)
        {
            this._proxy = proxy;
            this._realProxy = RemotingServices.GetRealProxy(this._proxy);
            this._serverType = serverType;
            this._objectMode = WellKnownObjectMode.Singleton;
        }

        [SecurityCritical]
        public virtual IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            IMessage message = null;
            message = this.SyncProcessMessage(msg);
            if (replySink != null)
            {
                replySink.SyncProcessMessage(message);
            }
            return null;
        }

        [SecurityCritical]
        public virtual IMessage SyncProcessMessage(IMessage msg)
        {
            try
            {
                msg.Properties["__Uri"] = this._realProxy.IdentityObject.URI;
                if (this._objectMode == WellKnownObjectMode.Singleton)
                {
                    return this._realProxy.Invoke(msg);
                }
                MarshalByRefObject proxy = (MarshalByRefObject) Activator.CreateInstance(this._serverType, true);
                return RemotingServices.GetRealProxy(proxy).Invoke(msg);
            }
            catch (Exception exception)
            {
                return new ReturnMessage(exception, msg as IMethodCallMessage);
            }
        }

        public IMessageSink NextSink
        {
            [SecurityCritical]
            get
            {
                return null;
            }
        }

        public WellKnownObjectMode ObjectMode
        {
            set
            {
                this._objectMode = value;
            }
        }
    }
}

