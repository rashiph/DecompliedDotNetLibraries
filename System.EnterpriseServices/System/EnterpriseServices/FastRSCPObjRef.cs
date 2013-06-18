namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Serialization;

    [Serializable]
    internal class FastRSCPObjRef : ObjRef
    {
        private IntPtr _pUnk;
        private RealProxy _rp;
        private Type _serverType;

        internal FastRSCPObjRef(IntPtr pUnk, Type serverType, string uri)
        {
            this._pUnk = pUnk;
            this._serverType = serverType;
            this.URI = uri;
            this.TypeInfo = new SCMTypeName(serverType);
            this.ChannelInfo = new SCMChannelInfo();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ComponentServices.InitializeRemotingChannels();
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            object data = CallContext.GetData("__ClientIsClr");
            if ((data != null) && ((bool) data))
            {
                RemoteServicedComponentProxy proxy = this._rp as RemoteServicedComponentProxy;
                if (proxy != null)
                {
                    RemotingServices.Marshal((MarshalByRefObject) proxy.RemotingIntermediary.GetTransparentProxy(), null, null).GetObjectData(info, context);
                }
                else
                {
                    base.GetObjectData(info, context);
                }
            }
            else
            {
                base.GetObjectData(info, context);
                info.SetType(typeof(ServicedComponentMarshaler));
                info.AddValue("servertype", this._rp.GetProxiedType());
                byte[] dCOMBuffer = ComponentServices.GetDCOMBuffer((MarshalByRefObject) this._rp.GetTransparentProxy());
                if (dCOMBuffer != null)
                {
                    info.AddValue("dcomInfo", dCOMBuffer);
                }
            }
        }

        public override object GetRealObject(StreamingContext context)
        {
            RealProxy proxy = new RemoteServicedComponentProxy(this._serverType, this._pUnk, false);
            this._rp = proxy;
            return (MarshalByRefObject) proxy.GetTransparentProxy();
        }
    }
}

