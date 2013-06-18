namespace System.ServiceModel.Channels
{
    using System;

    internal abstract class WsrmHeaderInfo
    {
        private MessageHeaderInfo messageHeader;

        protected WsrmHeaderInfo(MessageHeaderInfo messageHeader)
        {
            this.messageHeader = messageHeader;
        }

        public MessageHeaderInfo MessageHeader
        {
            get
            {
                return this.messageHeader;
            }
        }
    }
}

