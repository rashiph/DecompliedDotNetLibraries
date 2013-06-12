namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.Remoting;
    using System.Security;

    internal class ClientAsyncReplyTerminatorSink : IMessageSink
    {
        internal IMessageSink _nextSink;

        internal ClientAsyncReplyTerminatorSink(IMessageSink nextSink)
        {
            this._nextSink = nextSink;
        }

        [SecurityCritical]
        public virtual IMessageCtrl AsyncProcessMessage(IMessage replyMsg, IMessageSink replySink)
        {
            return null;
        }

        [SecurityCritical]
        public virtual IMessage SyncProcessMessage(IMessage replyMsg)
        {
            Guid empty = Guid.Empty;
            if (RemotingServices.CORProfilerTrackRemotingCookie())
            {
                object obj2 = replyMsg.Properties["CORProfilerCookie"];
                if (obj2 != null)
                {
                    empty = (Guid) obj2;
                }
            }
            RemotingServices.CORProfilerRemotingClientReceivingReply(empty, true);
            return this._nextSink.SyncProcessMessage(replyMsg);
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

