namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;

    public abstract class PeerMessagePropagationFilter
    {
        protected PeerMessagePropagationFilter()
        {
        }

        public abstract PeerMessagePropagation ShouldMessagePropagate(Message message, PeerMessageOrigination origination);
    }
}

