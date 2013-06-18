namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal abstract class OutputChannel : ChannelBase, IOutputChannel, IChannel, ICommunicationObject
    {
        protected OutputChannel(ChannelManagerBase manager) : base(manager)
        {
        }

        protected virtual void AddHeadersTo(Message message)
        {
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, base.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowIfDisposedOrNotOpen();
            this.AddHeadersTo(message);
            this.EmitTrace(message);
            return this.OnBeginSend(message, timeout, callback, state);
        }

        protected virtual TraceRecord CreateSendTrace(Message message)
        {
            return MessageTransmitTraceRecord.CreateSendTraceRecord(message, this.RemoteAddress);
        }

        private void EmitTrace(Message message)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40014, System.ServiceModel.SR.GetString("TraceCodeMessageSent"), this.CreateSendTrace(message), this, null);
            }
        }

        public void EndSend(IAsyncResult result)
        {
            this.OnEndSend(result);
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IOutputChannel))
            {
                return (T) this;
            }
            T property = base.GetProperty<T>();
            if (property != null)
            {
                return property;
            }
            return default(T);
        }

        protected abstract IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract void OnEndSend(IAsyncResult result);
        protected abstract void OnSend(Message message, TimeSpan timeout);
        public void Send(Message message)
        {
            this.Send(message, base.DefaultSendTimeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowIfDisposedOrNotOpen();
            this.AddHeadersTo(message);
            this.EmitTrace(message);
            this.OnSend(message, timeout);
        }

        public abstract EndpointAddress RemoteAddress { get; }

        public abstract Uri Via { get; }
    }
}

