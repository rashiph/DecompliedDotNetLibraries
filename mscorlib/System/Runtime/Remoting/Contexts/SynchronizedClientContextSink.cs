namespace System.Runtime.Remoting.Contexts
{
    using System;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    internal class SynchronizedClientContextSink : InternalSink, IMessageSink
    {
        internal IMessageSink _nextSink;
        [SecurityCritical]
        internal SynchronizationAttribute _property;

        [SecurityCritical]
        internal SynchronizedClientContextSink(SynchronizationAttribute prop, IMessageSink nextSink)
        {
            this._property = prop;
            this._nextSink = nextSink;
        }

        [SecurityCritical]
        public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
        {
            if (!this._property.IsReEntrant)
            {
                LogicalCallContext context = (LogicalCallContext) reqMsg.Properties[Message.CallContextKey];
                string newLogicalCallID = Identity.GetNewLogicalCallID();
                context.RemotingData.LogicalCallID = newLogicalCallID;
                this._property.AsyncCallOutLCIDList.Add(newLogicalCallID);
            }
            AsyncReplySink sink = new AsyncReplySink(replySink, this._property);
            return this._nextSink.AsyncProcessMessage(reqMsg, sink);
        }

        [SecuritySafeCritical]
        ~SynchronizedClientContextSink()
        {
            this._property.Dispose();
        }

        [SecurityCritical]
        public virtual IMessage SyncProcessMessage(IMessage reqMsg)
        {
            IMessage message;
            if (this._property.IsReEntrant)
            {
                this._property.HandleThreadExit();
                message = this._nextSink.SyncProcessMessage(reqMsg);
                this._property.HandleThreadReEntry();
                return message;
            }
            LogicalCallContext context = (LogicalCallContext) reqMsg.Properties[Message.CallContextKey];
            string logicalCallID = context.RemotingData.LogicalCallID;
            bool flag = false;
            if (logicalCallID == null)
            {
                logicalCallID = Identity.GetNewLogicalCallID();
                context.RemotingData.LogicalCallID = logicalCallID;
                flag = true;
            }
            bool flag2 = false;
            if (this._property.SyncCallOutLCID == null)
            {
                this._property.SyncCallOutLCID = logicalCallID;
                flag2 = true;
            }
            message = this._nextSink.SyncProcessMessage(reqMsg);
            if (flag2)
            {
                this._property.SyncCallOutLCID = null;
                if (flag)
                {
                    LogicalCallContext context2 = (LogicalCallContext) message.Properties[Message.CallContextKey];
                    context2.RemotingData.LogicalCallID = null;
                }
            }
            return message;
        }

        public IMessageSink NextSink
        {
            [SecurityCritical]
            get
            {
                return this._nextSink;
            }
        }

        internal class AsyncReplySink : IMessageSink
        {
            internal IMessageSink _nextSink;
            [SecurityCritical]
            internal SynchronizationAttribute _property;

            [SecurityCritical]
            internal AsyncReplySink(IMessageSink nextSink, SynchronizationAttribute prop)
            {
                this._nextSink = nextSink;
                this._property = prop;
            }

            [SecurityCritical]
            public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
            {
                throw new NotSupportedException();
            }

            [SecurityCritical]
            public virtual IMessage SyncProcessMessage(IMessage reqMsg)
            {
                WorkItem work = new WorkItem(reqMsg, this._nextSink, null);
                this._property.HandleWorkRequest(work);
                if (!this._property.IsReEntrant)
                {
                    this._property.AsyncCallOutLCIDList.Remove(((LogicalCallContext) reqMsg.Properties[Message.CallContextKey]).RemotingData.LogicalCallID);
                }
                return work.ReplyMessage;
            }

            public IMessageSink NextSink
            {
                [SecurityCritical]
                get
                {
                    return this._nextSink;
                }
            }
        }
    }
}

