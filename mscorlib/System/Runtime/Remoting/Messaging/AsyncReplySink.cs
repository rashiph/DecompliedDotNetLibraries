namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.Remoting.Contexts;
    using System.Security;
    using System.Threading;

    internal class AsyncReplySink : IMessageSink
    {
        private Context _cliCtx;
        private IMessageSink _replySink;

        internal AsyncReplySink(IMessageSink replySink, Context cliCtx)
        {
            this._replySink = replySink;
            this._cliCtx = cliCtx;
        }

        [SecurityCritical]
        public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
        {
            throw new NotSupportedException();
        }

        [SecurityCritical]
        public virtual IMessage SyncProcessMessage(IMessage reqMsg)
        {
            IMessage message = null;
            if (this._replySink != null)
            {
                object[] args = new object[] { reqMsg, this._replySink };
                InternalCrossContextDelegate ftnToCall = new InternalCrossContextDelegate(System.Runtime.Remoting.Messaging.AsyncReplySink.SyncProcessMessageCallback);
                message = (IMessage) Thread.CurrentThread.InternalCrossContextCallback(this._cliCtx, ftnToCall, args);
            }
            return message;
        }

        [SecurityCritical]
        internal static object SyncProcessMessageCallback(object[] args)
        {
            IMessage msg = (IMessage) args[0];
            IMessageSink sink = (IMessageSink) args[1];
            Thread.CurrentContext.NotifyDynamicSinks(msg, true, false, true, true);
            return sink.SyncProcessMessage(msg);
        }

        public IMessageSink NextSink
        {
            [SecurityCritical]
            get
            {
                return this._replySink;
            }
        }
    }
}

