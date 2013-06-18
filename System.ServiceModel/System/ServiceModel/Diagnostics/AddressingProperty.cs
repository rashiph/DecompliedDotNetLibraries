namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class AddressingProperty
    {
        private string action;
        private UniqueId messageId;
        private EndpointAddress replyTo;
        private Uri to;

        public AddressingProperty(MessageHeaders headers)
        {
            this.action = headers.Action;
            this.to = headers.To;
            this.replyTo = headers.ReplyTo;
            this.messageId = headers.MessageId;
        }

        public string Action
        {
            get
            {
                return this.action;
            }
        }

        public UniqueId MessageId
        {
            get
            {
                return this.messageId;
            }
        }

        public static string Name
        {
            get
            {
                return "Addressing";
            }
        }

        public EndpointAddress ReplyTo
        {
            get
            {
                return this.replyTo;
            }
        }

        public Uri To
        {
            get
            {
                return this.to;
            }
        }
    }
}

