namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting;
    using System.Security;

    internal class MCMDictionary : MessageDictionary
    {
        internal IMethodCallMessage _mcmsg;
        public static string[] MCMkeys = new string[] { "__Uri", "__MethodName", "__MethodSignature", "__TypeName", "__Args", "__CallContext" };

        public MCMDictionary(IMethodCallMessage msg, IDictionary idict) : base(MCMkeys, idict)
        {
            this._mcmsg = msg;
        }

        [SecurityCritical]
        private LogicalCallContext FetchLogicalCallContext()
        {
            Message message = this._mcmsg as Message;
            if (message != null)
            {
                return message.GetLogicalCallContext();
            }
            MethodCall call = this._mcmsg as MethodCall;
            if (call == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
            }
            return call.GetLogicalCallContext();
        }

        [SecuritySafeCritical]
        internal override object GetMessageValue(int i)
        {
            switch (i)
            {
                case 0:
                    return this._mcmsg.Uri;

                case 1:
                    return this._mcmsg.MethodName;

                case 2:
                    return this._mcmsg.MethodSignature;

                case 3:
                    return this._mcmsg.TypeName;

                case 4:
                    return this._mcmsg.Args;

                case 5:
                    return this.FetchLogicalCallContext();
            }
            throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
        }

        [SecurityCritical]
        internal override void SetSpecialKey(int keyNum, object value)
        {
            Message message = this._mcmsg as Message;
            MethodCall call = this._mcmsg as MethodCall;
            switch (keyNum)
            {
                case 0:
                    if (message == null)
                    {
                        if (call == null)
                        {
                            throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
                        }
                        call.Uri = (string) value;
                        return;
                    }
                    message.Uri = (string) value;
                    return;

                case 1:
                    if (message == null)
                    {
                        throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
                    }
                    message.SetLogicalCallContext((LogicalCallContext) value);
                    return;
            }
            throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
        }
    }
}

