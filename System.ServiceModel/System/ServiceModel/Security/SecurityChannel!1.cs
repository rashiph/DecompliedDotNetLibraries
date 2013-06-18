namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal abstract class SecurityChannel<TChannel> : LayeredChannel<TChannel> where TChannel: class, IChannel
    {
        private System.ServiceModel.Security.SecurityProtocol securityProtocol;

        protected SecurityChannel(ChannelManagerBase channelManager, TChannel innerChannel) : this(channelManager, innerChannel, null)
        {
        }

        protected SecurityChannel(ChannelManagerBase channelManager, TChannel innerChannel, System.ServiceModel.Security.SecurityProtocol securityProtocol) : base(channelManager, innerChannel)
        {
            this.securityProtocol = securityProtocol;
        }

        private IAsyncResult BeginCloseSecurityProtocol(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.securityProtocol != null)
            {
                return this.securityProtocol.BeginClose(timeout, callback, state);
            }
            return new NullSecurityProtocolCloseAsyncResult<TChannel>(callback, state);
        }

        private void EndCloseSecurityProtocol(IAsyncResult result)
        {
            if (result is NullSecurityProtocolCloseAsyncResult<TChannel>)
            {
                NullSecurityProtocolCloseAsyncResult<TChannel>.End(result);
            }
            else
            {
                this.securityProtocol.EndClose(result);
            }
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(FaultConverter))
            {
                return (new SecurityChannelFaultConverter(base.InnerChannel) as T);
            }
            return base.GetProperty<T>();
        }

        protected override void OnAbort()
        {
            if (this.securityProtocol != null)
            {
                this.securityProtocol.Close(true, TimeSpan.Zero);
            }
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.BeginCloseSecurityProtocol), new ChainedEndHandler(this.EndCloseSecurityProtocol), new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose));
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.securityProtocol != null)
            {
                this.securityProtocol.Close(false, helper.RemainingTime());
            }
            base.OnClose(helper.RemainingTime());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        protected void ThrowIfDisposedOrNotOpen(Message message)
        {
            base.ThrowIfDisposedOrNotOpen();
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
        }

        public System.ServiceModel.Security.SecurityProtocol SecurityProtocol
        {
            get
            {
                return this.securityProtocol;
            }
            protected set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.securityProtocol = value;
            }
        }

        private class NullSecurityProtocolCloseAsyncResult : CompletedAsyncResult
        {
            public NullSecurityProtocolCloseAsyncResult(AsyncCallback callback, object state) : base(callback, state)
            {
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SecurityChannel<TChannel>.NullSecurityProtocolCloseAsyncResult>(result);
            }
        }

        protected sealed class OutputChannelSendAsyncResult : ApplySecurityAndSendAsyncResult<IOutputChannel>
        {
            public OutputChannelSendAsyncResult(Message message, SecurityProtocol binding, IOutputChannel channel, TimeSpan timeout, AsyncCallback callback, object state) : base(binding, channel, timeout, callback, state)
            {
                base.Begin(message, null);
            }

            protected override IAsyncResult BeginSendCore(IOutputChannel channel, Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginSend(message, timeout, callback, state);
            }

            internal static void End(IAsyncResult result)
            {
                SecurityChannel<TChannel>.OutputChannelSendAsyncResult self = result as SecurityChannel<TChannel>.OutputChannelSendAsyncResult;
                ApplySecurityAndSendAsyncResult<IOutputChannel>.OnEnd(self);
            }

            protected override void EndSendCore(IOutputChannel channel, IAsyncResult result)
            {
                channel.EndSend(result);
            }

            protected override void OnSendCompleteCore(TimeSpan timeout)
            {
            }
        }
    }
}

