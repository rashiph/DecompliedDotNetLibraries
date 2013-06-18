namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal static class MessagingVersionHelper
    {
        public static System.ServiceModel.Channels.AddressingVersion AddressingVersion(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(MessagingVersionHelper), "AddressingVersion");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return System.ServiceModel.Channels.AddressingVersion.WSAddressingAugust2004;

                case ProtocolVersion.Version11:
                    return System.ServiceModel.Channels.AddressingVersion.WSAddressing10;
            }
            return null;
        }

        public static System.ServiceModel.Channels.MessageVersion MessageVersion(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(MessagingVersionHelper), "MessageVersion");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return System.ServiceModel.Channels.MessageVersion.Soap11WSAddressingAugust2004;

                case ProtocolVersion.Version11:
                    return System.ServiceModel.Channels.MessageVersion.Soap11WSAddressing10;
            }
            return null;
        }

        public static MessageSecurityVersion SecurityVersion(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(MessagingVersionHelper), "SecurityVersion");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11;

                case ProtocolVersion.Version11:
                    return MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12;
            }
            return null;
        }

        public static void SetReplyAddress(Message message, EndpointAddress replyTo, ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(MessagingVersionHelper), "SetReplyAddress");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    if (message.Headers.ReplyTo == null)
                    {
                        message.Headers.ReplyTo = replyTo;
                    }
                    if (message.Headers.MessageId == null)
                    {
                        message.Headers.MessageId = new UniqueId();
                    }
                    return;

                case ProtocolVersion.Version11:
                    if (message.Headers.From == null)
                    {
                        message.Headers.From = replyTo;
                    }
                    return;
            }
        }
    }
}

