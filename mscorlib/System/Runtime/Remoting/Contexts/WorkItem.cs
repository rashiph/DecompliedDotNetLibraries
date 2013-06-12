namespace System.Runtime.Remoting.Contexts
{
    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Threading;

    internal class WorkItem
    {
        [SecurityCritical]
        internal LogicalCallContext _callCtx;
        internal Context _ctx;
        internal int _flags;
        internal IMessageSink _nextSink;
        internal IMessage _replyMsg;
        internal IMessageSink _replySink;
        internal IMessage _reqMsg;
        internal static InternalCrossContextDelegate _xctxDel = new InternalCrossContextDelegate(WorkItem.ExecuteCallback);
        private const int FLG_ASYNC = 4;
        private const int FLG_DUMMY = 8;
        private const int FLG_SIGNALED = 2;
        private const int FLG_WAITING = 1;

        [SecurityCritical]
        internal WorkItem(IMessage reqMsg, IMessageSink nextSink, IMessageSink replySink)
        {
            this._reqMsg = reqMsg;
            this._replyMsg = null;
            this._nextSink = nextSink;
            this._replySink = replySink;
            this._ctx = Thread.CurrentContext;
            this._callCtx = CallContext.GetLogicalCallContext();
        }

        [SecurityCritical]
        internal virtual void Execute()
        {
            Thread.CurrentThread.InternalCrossContextCallback(this._ctx, _xctxDel, new object[] { this });
        }

        [SecurityCritical]
        internal static object ExecuteCallback(object[] args)
        {
            WorkItem item = (WorkItem) args[0];
            if (item.IsAsync())
            {
                item._nextSink.AsyncProcessMessage(item._reqMsg, item._replySink);
            }
            else if (item._nextSink != null)
            {
                item._replyMsg = item._nextSink.SyncProcessMessage(item._reqMsg);
            }
            return null;
        }

        internal virtual bool IsAsync()
        {
            return ((this._flags & 4) == 4);
        }

        internal virtual bool IsDummy()
        {
            return ((this._flags & 8) == 8);
        }

        internal virtual bool IsSignaled()
        {
            return ((this._flags & 2) == 2);
        }

        internal virtual bool IsWaiting()
        {
            return ((this._flags & 1) == 1);
        }

        internal virtual void SetAsync()
        {
            this._flags |= 4;
        }

        internal virtual void SetDummy()
        {
            this._flags |= 8;
        }

        internal virtual void SetSignaled()
        {
            this._flags |= 2;
        }

        internal virtual void SetWaiting()
        {
            this._flags |= 1;
        }

        internal virtual IMessage ReplyMessage
        {
            get
            {
                return this._replyMsg;
            }
        }
    }
}

