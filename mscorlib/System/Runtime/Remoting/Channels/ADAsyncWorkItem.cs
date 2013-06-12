namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    internal class ADAsyncWorkItem
    {
        [SecurityCritical]
        private LogicalCallContext _callCtx;
        private IMessageSink _nextSink;
        private IMessageSink _replySink;
        private IMessage _reqMsg;

        [SecurityCritical]
        internal ADAsyncWorkItem(IMessage reqMsg, IMessageSink nextSink, IMessageSink replySink)
        {
            this._reqMsg = reqMsg;
            this._nextSink = nextSink;
            this._replySink = replySink;
            this._callCtx = CallContext.GetLogicalCallContext();
        }

        [SecurityCritical]
        internal virtual void FinishAsyncWork(object stateIgnored)
        {
            LogicalCallContext callCtx = CallContext.SetLogicalCallContext(this._callCtx);
            IMessage msg = this._nextSink.SyncProcessMessage(this._reqMsg);
            if (this._replySink != null)
            {
                this._replySink.SyncProcessMessage(msg);
            }
            CallContext.SetLogicalCallContext(callCtx);
        }
    }
}

