namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    public abstract class MessageEncodingBindingElement : BindingElement
    {
        protected MessageEncodingBindingElement()
        {
        }

        protected MessageEncodingBindingElement(MessageEncodingBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
        }

        internal virtual bool CheckEncodingVersion(EnvelopeVersion version)
        {
            return false;
        }

        public abstract MessageEncoderFactory CreateMessageEncoderFactory();
        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(System.ServiceModel.Channels.MessageVersion))
            {
                return (T) this.MessageVersion;
            }
            return context.GetInnerProperty<T>();
        }

        internal IChannelFactory<TChannel> InternalBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        internal IChannelListener<TChannel> InternalBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        internal bool InternalCanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        internal bool InternalCanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        internal override bool IsMatch(BindingElement b)
        {
            return ((b != null) && (b is MessageEncodingBindingElement));
        }

        internal virtual bool IsWsdlExportable
        {
            get
            {
                return true;
            }
        }

        public abstract System.ServiceModel.Channels.MessageVersion MessageVersion { get; set; }
    }
}

