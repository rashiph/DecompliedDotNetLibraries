namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public abstract class ChannelDispatcherBase : CommunicationObject
    {
        protected ChannelDispatcherBase()
        {
        }

        protected virtual void Attach(ServiceHostBase host)
        {
        }

        internal void AttachInternal(ServiceHostBase host)
        {
            this.Attach(host);
        }

        public virtual void CloseInput()
        {
        }

        internal virtual void CloseInput(TimeSpan timeout)
        {
            this.CloseInput();
        }

        protected virtual void Detach(ServiceHostBase host)
        {
        }

        internal void DetachInternal(ServiceHostBase host)
        {
            this.Detach(host);
        }

        public abstract ServiceHostBase Host { get; }

        public abstract IChannelListener Listener { get; }
    }
}

