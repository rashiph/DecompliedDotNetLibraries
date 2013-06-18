namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal abstract class WsrmRequestInfo
    {
        private UniqueId messageId;
        private EndpointAddress replyTo;

        protected WsrmRequestInfo()
        {
        }

        protected void SetMessageId(MessageVersion messageVersion, MessageHeaders headers)
        {
            this.messageId = headers.MessageId;
            if (this.messageId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(System.ServiceModel.SR.GetString("MissingMessageIdOnWsrmRequest", new object[] { this.RequestName }), messageVersion.Addressing.Namespace, "MessageID", false));
            }
        }

        protected void SetReplyTo(MessageVersion messageVersion, MessageHeaders headers)
        {
            this.replyTo = headers.ReplyTo;
            if ((messageVersion.Addressing == AddressingVersion.WSAddressing10) && (this.replyTo == null))
            {
                this.replyTo = EndpointAddress.AnonymousAddress;
            }
            if (this.replyTo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(System.ServiceModel.SR.GetString("MissingReplyToOnWsrmRequest", new object[] { this.RequestName }), messageVersion.Addressing.Namespace, "ReplyTo", false));
            }
        }

        public UniqueId MessageId
        {
            get
            {
                return this.messageId;
            }
        }

        public EndpointAddress ReplyTo
        {
            get
            {
                return this.replyTo;
            }
        }

        public abstract string RequestName { get; }
    }
}

