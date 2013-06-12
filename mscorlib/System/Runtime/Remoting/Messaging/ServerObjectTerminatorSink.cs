namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.Remoting.Contexts;
    using System.Security;

    [Serializable]
    internal class ServerObjectTerminatorSink : InternalSink, IMessageSink
    {
        internal StackBuilderSink _stackBuilderSink;

        internal ServerObjectTerminatorSink(MarshalByRefObject srvObj)
        {
            this._stackBuilderSink = new StackBuilderSink(srvObj);
        }

        [SecurityCritical]
        public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
        {
            IMessageCtrl ctrl = null;
            IMessage msg = InternalSink.ValidateMessage(reqMsg);
            if (msg != null)
            {
                if (replySink != null)
                {
                    replySink.SyncProcessMessage(msg);
                }
                return ctrl;
            }
            IMessageSink serverObject = this._stackBuilderSink.ServerObject as IMessageSink;
            if (serverObject != null)
            {
                return serverObject.AsyncProcessMessage(reqMsg, replySink);
            }
            return this._stackBuilderSink.AsyncProcessMessage(reqMsg, replySink);
        }

        [SecurityCritical]
        public virtual IMessage SyncProcessMessage(IMessage reqMsg)
        {
            IMessage message2;
            IMessage message = InternalSink.ValidateMessage(reqMsg);
            if (message != null)
            {
                return message;
            }
            ArrayWithSize serverSideDynamicSinks = InternalSink.GetServerIdentity(reqMsg).ServerSideDynamicSinks;
            if (serverSideDynamicSinks != null)
            {
                DynamicPropertyHolder.NotifyDynamicSinks(reqMsg, serverSideDynamicSinks, false, true, false);
            }
            IMessageSink serverObject = this._stackBuilderSink.ServerObject as IMessageSink;
            if (serverObject != null)
            {
                message2 = serverObject.SyncProcessMessage(reqMsg);
            }
            else
            {
                message2 = this._stackBuilderSink.SyncProcessMessage(reqMsg);
            }
            if (serverSideDynamicSinks != null)
            {
                DynamicPropertyHolder.NotifyDynamicSinks(message2, serverSideDynamicSinks, false, false, false);
            }
            return message2;
        }

        public IMessageSink NextSink
        {
            [SecurityCritical]
            get
            {
                return null;
            }
        }
    }
}

