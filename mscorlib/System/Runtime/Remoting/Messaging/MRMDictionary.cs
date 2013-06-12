namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting;
    using System.Security;

    internal class MRMDictionary : MessageDictionary
    {
        internal IMethodReturnMessage _mrmsg;
        internal bool fault;
        public static string[] MCMkeysFault = new string[] { "__CallContext" };
        public static string[] MCMkeysNoFault = new string[] { "__Uri", "__MethodName", "__MethodSignature", "__TypeName", "__Return", "__OutArgs", "__CallContext" };

        [SecurityCritical]
        public MRMDictionary(IMethodReturnMessage msg, IDictionary idict) : base((msg.Exception != null) ? MCMkeysFault : MCMkeysNoFault, idict)
        {
            this.fault = msg.Exception != null;
            this._mrmsg = msg;
        }

        [SecurityCritical]
        private LogicalCallContext FetchLogicalCallContext()
        {
            ReturnMessage message = this._mrmsg as ReturnMessage;
            if (message != null)
            {
                return message.GetLogicalCallContext();
            }
            MethodResponse response = this._mrmsg as MethodResponse;
            if (response != null)
            {
                return response.GetLogicalCallContext();
            }
            StackBasedReturnMessage message2 = this._mrmsg as StackBasedReturnMessage;
            if (message2 == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
            }
            return message2.GetLogicalCallContext();
        }

        [SecuritySafeCritical]
        internal override object GetMessageValue(int i)
        {
            switch (i)
            {
                case 0:
                    if (!this.fault)
                    {
                        return this._mrmsg.Uri;
                    }
                    return this.FetchLogicalCallContext();

                case 1:
                    return this._mrmsg.MethodName;

                case 2:
                    return this._mrmsg.MethodSignature;

                case 3:
                    return this._mrmsg.TypeName;

                case 4:
                    if (!this.fault)
                    {
                        return this._mrmsg.ReturnValue;
                    }
                    return this._mrmsg.Exception;

                case 5:
                    return this._mrmsg.Args;

                case 6:
                    return this.FetchLogicalCallContext();
            }
            throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
        }

        [SecurityCritical]
        internal override void SetSpecialKey(int keyNum, object value)
        {
            ReturnMessage message = this._mrmsg as ReturnMessage;
            MethodResponse response = this._mrmsg as MethodResponse;
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

