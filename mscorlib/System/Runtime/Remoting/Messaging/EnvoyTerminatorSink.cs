namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Security;
    using System.Threading;

    [Serializable]
    internal class EnvoyTerminatorSink : InternalSink, IMessageSink
    {
        private static EnvoyTerminatorSink messageSink;
        private static object staticSyncObject = new object();

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
            return Thread.CurrentContext.GetClientContextChain().AsyncProcessMessage(reqMsg, replySink);
        }

        [SecurityCritical]
        public virtual IMessage SyncProcessMessage(IMessage reqMsg)
        {
            IMessage message = InternalSink.ValidateMessage(reqMsg);
            if (message != null)
            {
                return message;
            }
            return Thread.CurrentContext.GetClientContextChain().SyncProcessMessage(reqMsg);
        }

        internal static IMessageSink MessageSink
        {
            get
            {
                if (messageSink == null)
                {
                    EnvoyTerminatorSink sink = new EnvoyTerminatorSink();
                    lock (staticSyncObject)
                    {
                        if (messageSink == null)
                        {
                            messageSink = sink;
                        }
                    }
                }
                return messageSink;
            }
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

