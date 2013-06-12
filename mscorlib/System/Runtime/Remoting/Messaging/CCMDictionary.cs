namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Security;

    internal class CCMDictionary : MessageDictionary
    {
        internal IConstructionCallMessage _ccmsg;
        public static string[] CCMkeys = new string[] { "__Uri", "__MethodName", "__MethodSignature", "__TypeName", "__Args", "__CallContext", "__CallSiteActivationAttributes", "__ActivationType", "__ContextProperties", "__Activator", "__ActivationTypeName" };

        public CCMDictionary(IConstructionCallMessage msg, IDictionary idict) : base(CCMkeys, idict)
        {
            this._ccmsg = msg;
        }

        [SecurityCritical]
        private LogicalCallContext FetchLogicalCallContext()
        {
            ConstructorCallMessage message = this._ccmsg as ConstructorCallMessage;
            if (message != null)
            {
                return message.GetLogicalCallContext();
            }
            if (!(this._ccmsg is ConstructionCall))
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
            }
            return ((MethodCall) this._ccmsg).GetLogicalCallContext();
        }

        [SecuritySafeCritical]
        internal override object GetMessageValue(int i)
        {
            switch (i)
            {
                case 0:
                    return this._ccmsg.Uri;

                case 1:
                    return this._ccmsg.MethodName;

                case 2:
                    return this._ccmsg.MethodSignature;

                case 3:
                    return this._ccmsg.TypeName;

                case 4:
                    return this._ccmsg.Args;

                case 5:
                    return this.FetchLogicalCallContext();

                case 6:
                    return this._ccmsg.CallSiteActivationAttributes;

                case 7:
                    return null;

                case 8:
                    return this._ccmsg.ContextProperties;

                case 9:
                    return this._ccmsg.Activator;

                case 10:
                    return this._ccmsg.ActivationTypeName;
            }
            throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
        }

        [SecurityCritical]
        internal override void SetSpecialKey(int keyNum, object value)
        {
            switch (keyNum)
            {
                case 0:
                    ((ConstructorCallMessage) this._ccmsg).Uri = (string) value;
                    return;

                case 1:
                    ((ConstructorCallMessage) this._ccmsg).SetLogicalCallContext((LogicalCallContext) value);
                    return;
            }
            throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
        }
    }
}

