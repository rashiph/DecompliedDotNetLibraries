namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class ListenerChannel
    {
        private IChannelBinder binder;
        private ServiceThrottle throttle;

        public ListenerChannel(IChannelBinder binder)
        {
            this.binder = binder;
        }

        public IChannelBinder Binder
        {
            get
            {
                return this.binder;
            }
        }

        public ServiceThrottle Throttle
        {
            get
            {
                return this.throttle;
            }
            set
            {
                this.throttle = value;
            }
        }
    }
}

