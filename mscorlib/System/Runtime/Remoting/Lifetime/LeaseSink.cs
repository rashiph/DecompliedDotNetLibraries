namespace System.Runtime.Remoting.Lifetime
{
    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    internal class LeaseSink : IMessageSink
    {
        private Lease lease;
        private IMessageSink nextSink;

        public LeaseSink(Lease lease, IMessageSink nextSink)
        {
            this.lease = lease;
            this.nextSink = nextSink;
        }

        [SecurityCritical]
        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            this.lease.RenewOnCall();
            return this.nextSink.AsyncProcessMessage(msg, replySink);
        }

        [SecurityCritical]
        public IMessage SyncProcessMessage(IMessage msg)
        {
            this.lease.RenewOnCall();
            return this.nextSink.SyncProcessMessage(msg);
        }

        public IMessageSink NextSink
        {
            [SecurityCritical]
            get
            {
                return this.nextSink;
            }
        }
    }
}

