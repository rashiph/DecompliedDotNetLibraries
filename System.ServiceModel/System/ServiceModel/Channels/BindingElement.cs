namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    public abstract class BindingElement
    {
        protected BindingElement()
        {
        }

        protected BindingElement(BindingElement elementToBeCloned)
        {
        }

        public virtual IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return context.BuildInnerChannelListener<TChannel>();
        }

        public virtual bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public virtual bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        public abstract BindingElement Clone();
        internal T GetIndividualProperty<T>() where T: class
        {
            return this.GetProperty<T>(new BindingContext(new CustomBinding(), new BindingParameterCollection()));
        }

        public abstract T GetProperty<T>(BindingContext context) where T: class;
        internal virtual bool IsMatch(BindingElement b)
        {
            return false;
        }
    }
}

