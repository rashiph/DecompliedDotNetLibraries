namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    internal class ServerAsyncReplyTerminatorSink : IMessageSink
    {
        internal IMessageSink _nextSink;

        internal ServerAsyncReplyTerminatorSink(IMessageSink nextSink)
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
            Guid guid;
            RemotingServices.CORProfilerRemotingServerSendingReply(out guid, true);
            if (RemotingServices.CORProfilerTrackRemotingCookie())
            {
                replyMsg.Properties["CORProfilerCookie"] = guid;
            }
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

