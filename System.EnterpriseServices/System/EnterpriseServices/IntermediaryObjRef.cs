namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Serialization;

    internal class IntermediaryObjRef : ObjRef
    {
        private ObjRef _custom;

        public IntermediaryObjRef(MarshalByRefObject mbro, Type reqtype, RealProxy pxy) : base(mbro, reqtype)
        {
            this._custom = pxy.CreateObjRef(reqtype);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            object data = CallContext.GetData("__ClientIsClr");
            if ((data != null) && ((bool) data))
            {
                base.GetObjectData(info, ctx);
            }
            else
            {
                this._custom.GetObjectData(info, ctx);
            }
        }
    }
}

