namespace System.Runtime.Remoting.Contexts
{
    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    internal class SynchronizedServerContextSink : InternalSink, IMessageSink
    {
        internal IMessageSink _nextSink;
        [SecurityCritical]
        internal SynchronizationAttribute _property;

        [SecurityCritical]
        internal SynchronizedServerContextSink(SynchronizationAttribute prop, IMessageSink nextSink)
        {
            this._property = prop;
            this._nextSink = nextSink;
        }

        [SecurityCritical]
        public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
        {
            WorkItem work = new WorkItem(reqMsg, this._nextSink, replySink);
            work.SetAsync();
            this._property.HandleWorkRequest(work);
            return null;
        }

        [SecuritySafeCritical]
        ~SynchronizedServerContextSink()
        {
            this._property.Dispose();
        }

        [SecurityCritical]
        public virtual IMessage SyncProcessMessage(IMessage reqMsg)
        {
            WorkItem work = new WorkItem(reqMsg, this._nextSink, null);
            this._property.HandleWorkRequest(work);
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

