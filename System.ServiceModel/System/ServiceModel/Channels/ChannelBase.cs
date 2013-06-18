namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    public abstract class ChannelBase : CommunicationObject, IChannel, ICommunicationObject, IDefaultCommunicationTimeouts
    {
        private ChannelManagerBase channelManager;

        protected ChannelBase(ChannelManagerBase channelManager)
        {
            if (channelManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelManager");
            }
            this.channelManager = channelManager;
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x4001f, System.ServiceModel.SR.GetString("TraceCodeChannelCreated", new object[] { TraceUtility.CreateSourceString(this) }), this);
            }
        }

        public virtual T GetProperty<T>() where T: class
        {
            IChannelFactory channelManager = this.channelManager as IChannelFactory;
            if (channelManager != null)
            {
                return channelManager.GetProperty<T>();
            }
            IChannelListener listener = this.channelManager as IChannelListener;
            if (listener != null)
            {
                return listener.GetProperty<T>();
            }
            return default(T);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x40020, System.ServiceModel.SR.GetString("TraceCodeChannelDisposed", new object[] { TraceUtility.CreateSourceString(this) }), this);
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                return ((IDefaultCommunicationTimeouts) this.channelManager).CloseTimeout;
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                return ((IDefaultCommunicationTimeouts) this.channelManager).OpenTimeout;
            }
        }

        protected TimeSpan DefaultReceiveTimeout
        {
            get
            {
                return ((IDefaultCommunicationTimeouts) this.channelManager).ReceiveTimeout;
            }
        }

        protected TimeSpan DefaultSendTimeout
        {
            get
            {
                return ((IDefaultCommunicationTimeouts) this.channelManager).SendTimeout;
            }
        }

        protected ChannelManagerBase Manager
        {
            get
            {
                return this.channelManager;
            }
        }

        TimeSpan IDefaultCommunicationTimeouts.CloseTimeout
        {
            get
            {
                return this.DefaultCloseTimeout;
            }
        }

        TimeSpan IDefaultCommunicationTimeouts.OpenTimeout
        {
            get
            {
                return this.DefaultOpenTimeout;
            }
        }

        TimeSpan IDefaultCommunicationTimeouts.ReceiveTimeout
        {
            get
            {
                return this.DefaultReceiveTimeout;
            }
        }

        TimeSpan IDefaultCommunicationTimeouts.SendTimeout
        {
            get
            {
                return this.DefaultSendTimeout;
            }
        }
    }
}

