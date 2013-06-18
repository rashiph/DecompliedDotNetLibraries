namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Serialization;

    [Serializable]
    internal class ServicedComponentMarshaler : ObjRef
    {
        private bool _marshalled;
        private RealProxy _rp;
        private Type _rt;
        private SCUnMarshaler _um;

        private ServicedComponentMarshaler()
        {
        }

        internal ServicedComponentMarshaler(MarshalByRefObject o, Type requestedType) : base(o, requestedType)
        {
            this._rp = RemotingServices.GetRealProxy(o);
            this._rt = requestedType;
        }

        protected ServicedComponentMarshaler(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            byte[] buffer = null;
            Type type = null;
            bool flag = false;
            ComponentServices.InitializeRemotingChannels();
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Name.Equals("servertype"))
                {
                    type = (Type) enumerator.Value;
                }
                else
                {
                    if (enumerator.Name.Equals("dcomInfo"))
                    {
                        buffer = (byte[]) enumerator.Value;
                        continue;
                    }
                    if (enumerator.Name.Equals("fIsMarshalled"))
                    {
                        int num = 0;
                        object obj2 = enumerator.Value;
                        if (obj2.GetType() == typeof(string))
                        {
                            num = ((IConvertible) obj2).ToInt32(null);
                        }
                        else
                        {
                            num = (int) obj2;
                        }
                        if (num == 0)
                        {
                            flag = true;
                        }
                    }
                }
            }
            if (!flag)
            {
                this._marshalled = true;
            }
            this._um = new SCUnMarshaler(type, buffer);
            this._rt = type;
            if (base.IsFromThisProcess() && !ServicedComponentInfo.IsTypeEventSource(type))
            {
                this._rp = RemotingServices.GetRealProxy(base.GetRealObject(context));
            }
            else
            {
                if (ServicedComponentInfo.IsTypeEventSource(type))
                {
                    this.TypeInfo = new SCMTypeName(type);
                }
                object realObject = base.GetRealObject(context);
                this._rp = RemotingServices.GetRealProxy(realObject);
            }
            this._um.Dispose();
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
            if (!this.IsMarshaledObject)
            {
                return this;
            }
            if (base.IsFromThisProcess() && !ServicedComponentInfo.IsTypeEventSource(this._rt))
            {
                object realObject = base.GetRealObject(context);
                ((ServicedComponent) realObject).DoSetCOMIUnknown(IntPtr.Zero);
                return realObject;
            }
            if (this._rp == null)
            {
                this._rp = this._um.GetRealProxy();
            }
            return this._rp.GetTransparentProxy();
        }

        private bool IsMarshaledObject
        {
            get
            {
                return this._marshalled;
            }
        }
    }
}

