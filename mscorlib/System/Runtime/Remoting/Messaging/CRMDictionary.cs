namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Security;

    internal class CRMDictionary : MessageDictionary
    {
        internal IConstructionReturnMessage _crmsg;
        public static string[] CRMkeysFault = new string[] { "__Uri", "__MethodName", "__MethodSignature", "__TypeName", "__CallContext" };
        public static string[] CRMkeysNoFault = new string[] { "__Uri", "__MethodName", "__MethodSignature", "__TypeName", "__Return", "__OutArgs", "__CallContext" };
        internal bool fault;

        [SecurityCritical]
        public CRMDictionary(IConstructionReturnMessage msg, IDictionary idict) : base((msg.Exception != null) ? CRMkeysFault : CRMkeysNoFault, idict)
        {
            this.fault = msg.Exception != null;
            this._crmsg = msg;
        }

        [SecurityCritical]
        private LogicalCallContext FetchLogicalCallContext()
        {
            ReturnMessage message = this._crmsg as ReturnMessage;
            if (message != null)
            {
                return message.GetLogicalCallContext();
            }
            MethodResponse response = this._crmsg as MethodResponse;
            if (response == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
            }
            return response.GetLogicalCallContext();
        }

        [SecuritySafeCritical]
        internal override object GetMessageValue(int i)
        {
            switch (i)
            {
                case 0:
                    return this._crmsg.Uri;

                case 1:
                    return this._crmsg.MethodName;

                case 2:
                    return this._crmsg.MethodSignature;

                case 3:
                    return this._crmsg.TypeName;

                case 4:
                    if (this.fault)
                    {
                        return this.FetchLogicalCallContext();
                    }
                    return this._crmsg.ReturnValue;

                case 5:
                    return this._crmsg.Args;

                case 6:
                    return this.FetchLogicalCallContext();
            }
            throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
        }

        [SecurityCritical]
        internal override void SetSpecialKey(int keyNum, object value)
        {
            ReturnMessage message = this._crmsg as ReturnMessage;
            MethodResponse response = this._crmsg as MethodResponse;
            switch (keyNum)
            {
                case 0:
                    if (message == null)
                    {
                        if (response == null)
                        {
                            throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
                        }
                        response.Uri = (string) value;
                        return;
                    }
                    message.Uri = (string) value;
                    return;

                case 1:
                    if (message == null)
                    {
                        if (response == null)
                        {
                            throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
                        }
                        response.SetLogicalCallContext((LogicalCallContext) value);
                        return;
                    }
                    message.SetLogicalCallContext((LogicalCallContext) value);
                    return;
            }
            throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
        }
    }
}

