namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal interface IPeerNodeMessageHandling
    {
        PeerMessagePropagation DetermineMessagePropagation(Message message, PeerMessageOrigination origination);
        void HandleIncomingMessage(MessageBuffer messageBuffer, PeerMessagePropagation propagateFlags, int index, MessageHeader header, Uri via, Uri to);
        bool IsKnownVia(Uri via);
        bool IsNotSeenBefore(Message message, out byte[] id, out int cacheMiss);
        bool ValidateIncomingMessage(ref Message data, Uri via);

        MessageEncodingBindingElement EncodingBindingElement { get; }

        bool HasMessagePropagation { get; }
    }
}

