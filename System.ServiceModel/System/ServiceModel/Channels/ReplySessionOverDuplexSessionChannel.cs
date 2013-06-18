namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class ReplySessionOverDuplexSessionChannel : ReplyOverDuplexChannelBase<IDuplexSessionChannel>, IReplySessionChannel, IReplyChannel, IChannel, ICommunicationObject, ISessionChannel<IInputSession>
    {
        private ReplySessionOverDuplexSession session;

        public ReplySessionOverDuplexSessionChannel(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel) : base(channelManager, innerChannel)
        {
            this.session = new ReplySessionOverDuplexSession(innerChannel.Session);
        }

        public IInputSession Session
        {
            get
            {
                return this.session;
            }
        }

        private class ReplySessionOverDuplexSession : IInputSession, ISession
        {
            private IDuplexSession innerSession;

            public ReplySessionOverDuplexSession(IDuplexSession innerSession)
            {
                if (innerSession == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerSession");
                }
                this.innerSession = innerSession;
            }

            public string Id
            {
                get
                {
                    return this.innerSession.Id;
                }
            }
        }
    }
}

