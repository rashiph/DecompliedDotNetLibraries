namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;

    internal abstract class TransportChannelFactory<TChannel> : ChannelFactoryBase<TChannel>, ITransportFactorySettings, IDefaultCommunicationTimeouts
    {
        private System.ServiceModel.Channels.BufferManager bufferManager;
        private bool manualAddressing;
        private long maxBufferPoolSize;
        private long maxReceivedMessageSize;
        private System.ServiceModel.Channels.MessageEncoderFactory messageEncoderFactory;
        private System.ServiceModel.Channels.MessageVersion messageVersion;

        protected TransportChannelFactory(TransportBindingElement bindingElement, BindingContext context) : this(bindingElement, context, TransportDefaults.GetDefaultMessageEncoderFactory())
        {
        }

        protected TransportChannelFactory(TransportBindingElement bindingElement, BindingContext context, System.ServiceModel.Channels.MessageEncoderFactory defaultMessageEncoderFactory) : base(context.Binding)
        {
            this.manualAddressing = bindingElement.ManualAddressing;
            this.maxBufferPoolSize = bindingElement.MaxBufferPoolSize;
            this.maxReceivedMessageSize = bindingElement.MaxReceivedMessageSize;
            Collection<MessageEncodingBindingElement> collection = context.BindingParameters.FindAll<MessageEncodingBindingElement>();
            if (collection.Count > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MultipleMebesInParameters")));
            }
            if (collection.Count == 1)
            {
                this.messageEncoderFactory = collection[0].CreateMessageEncoderFactory();
                context.BindingParameters.Remove<MessageEncodingBindingElement>();
            }
            else
            {
                this.messageEncoderFactory = defaultMessageEncoderFactory;
            }
            if (this.messageEncoderFactory != null)
            {
                this.messageVersion = this.messageEncoderFactory.MessageVersion;
            }
            else
            {
                this.messageVersion = System.ServiceModel.Channels.MessageVersion.None;
            }
        }

        internal virtual int GetMaxBufferSize()
        {
            if (this.MaxReceivedMessageSize > 0x7fffffffL)
            {
                return 0x7fffffff;
            }
            return (int) this.MaxReceivedMessageSize;
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(System.ServiceModel.Channels.MessageVersion))
            {
                return (T) this.MessageVersion;
            }
            if (!(typeof(T) == typeof(FaultConverter)))
            {
                return base.GetProperty<T>();
            }
            if (this.MessageEncoderFactory == null)
            {
                return default(T);
            }
            return this.MessageEncoderFactory.Encoder.GetProperty<T>();
        }

        protected override void OnAbort()
        {
            this.OnCloseOrAbort();
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnCloseOrAbort();
            return base.OnBeginClose(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.OnCloseOrAbort();
            base.OnClose(timeout);
        }

        private void OnCloseOrAbort()
        {
            if (this.bufferManager != null)
            {
                this.bufferManager.Clear();
            }
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            this.bufferManager = System.ServiceModel.Channels.BufferManager.CreateBufferManager(this.MaxBufferPoolSize, this.GetMaxBufferSize());
        }

        internal void ValidateScheme(Uri via)
        {
            if ((via.Scheme != this.Scheme) && (string.Compare(via.Scheme, this.Scheme, StringComparison.OrdinalIgnoreCase) != 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("via", System.ServiceModel.SR.GetString("InvalidUriScheme", new object[] { via.Scheme, this.Scheme }));
            }
        }

        public System.ServiceModel.Channels.BufferManager BufferManager
        {
            get
            {
                return this.bufferManager;
            }
        }

        public bool ManualAddressing
        {
            get
            {
                return this.manualAddressing;
            }
        }

        public long MaxBufferPoolSize
        {
            get
            {
                return this.maxBufferPoolSize;
            }
        }

        public long MaxReceivedMessageSize
        {
            get
            {
                return this.maxReceivedMessageSize;
            }
        }

        public System.ServiceModel.Channels.MessageEncoderFactory MessageEncoderFactory
        {
            get
            {
                return this.messageEncoderFactory;
            }
        }

        public System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        public abstract string Scheme { get; }

        System.ServiceModel.Channels.BufferManager ITransportFactorySettings.BufferManager
        {
            get
            {
                return this.BufferManager;
            }
        }

        bool ITransportFactorySettings.ManualAddressing
        {
            get
            {
                return this.ManualAddressing;
            }
        }

        long ITransportFactorySettings.MaxReceivedMessageSize
        {
            get
            {
                return this.MaxReceivedMessageSize;
            }
        }

        System.ServiceModel.Channels.MessageEncoderFactory ITransportFactorySettings.MessageEncoderFactory
        {
            get
            {
                return this.MessageEncoderFactory;
            }
        }
    }
}

