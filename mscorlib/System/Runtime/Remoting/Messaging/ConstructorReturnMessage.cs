namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Security;
    using System.Threading;

    [SecurityCritical]
    internal class ConstructorReturnMessage : ReturnMessage, IConstructionReturnMessage, IMethodReturnMessage, IMethodMessage, IMessage
    {
        private int _iFlags;
        private MarshalByRefObject _o;
        private const int Intercept = 1;

        public ConstructorReturnMessage(Exception e, IConstructionCallMessage ccm) : base(e, ccm)
        {
        }

        public ConstructorReturnMessage(MarshalByRefObject o, object[] outArgs, int outArgsCount, LogicalCallContext callCtx, IConstructionCallMessage ccm) : base(o, outArgs, outArgsCount, callCtx, ccm)
        {
            this._o = o;
            this._iFlags = 1;
        }

        internal object GetObject()
        {
            return this._o;
        }

        public override IDictionary Properties
        {
            [SecurityCritical]
            get
            {
                if (base._properties == null)
                {
                    object obj2 = new CRMDictionary(this, new Hashtable());
                    Interlocked.CompareExchange(ref this._properties, obj2, null);
                }
                return (IDictionary) base._properties;
            }
        }

        public override object ReturnValue
        {
            [SecurityCritical]
            get
            {
                if (this._iFlags == 1)
                {
                    return RemotingServices.MarshalInternal(this._o, null, null);
                }
                return base.ReturnValue;
            }
        }
    }
}

