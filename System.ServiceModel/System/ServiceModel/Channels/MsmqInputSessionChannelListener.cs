namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Transactions;

    internal sealed class MsmqInputSessionChannelListener : MsmqChannelListenerBase<IInputSessionChannel>
    {
        private MsmqReceiveContextLockManager receiveContextManager;
        private System.ServiceModel.Channels.MsmqReceiveHelper receiver;

        internal MsmqInputSessionChannelListener(MsmqBindingElementBase bindingElement, BindingContext context, MsmqReceiveParameters receiveParameters) : base(bindingElement, context, receiveParameters, TransportDefaults.GetDefaultMessageEncoderFactory())
        {
            base.SetSecurityTokenAuthenticator(MsmqUri.NetMsmqAddressTranslator.Scheme, context);
            this.receiver = new System.ServiceModel.Channels.MsmqReceiveHelper(base.ReceiveParameters, this.Uri, new MsmqInputMessagePool((base.ReceiveParameters as MsmqTransportReceiveParameters).MaxPoolSize), null, this);
            if (base.ReceiveParameters.ReceiveContextSettings.Enabled)
            {
                this.receiveContextManager = new MsmqReceiveContextLockManager(base.ReceiveParameters.ReceiveContextSettings, this.receiver.Queue);
            }
        }

        public override IInputSessionChannel AcceptChannel()
        {
            return this.AcceptChannel(this.DefaultReceiveTimeout);
        }

        public override IInputSessionChannel AcceptChannel(TimeSpan timeout)
        {
            IInputSessionChannel channel;
            if (base.DoneReceivingInCurrentState())
            {
                return null;
            }
            if (!base.ReceiveParameters.ReceiveContextSettings.Enabled && (Transaction.Current == null))
            {
                base.Fault();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqTransactionRequired")));
            }
            MsmqInputMessage msmqMessage = this.receiver.TakeMessage();
            try
            {
                MsmqMessageProperty property;
                if (!this.receiver.TryReceive(msmqMessage, timeout, MsmqTransactionMode.CurrentOrThrow, out property))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                }
                if (property != null)
                {
                    return MsmqDecodeHelper.DecodeTransportSessiongram(this, msmqMessage, property, this.receiveContextManager);
                }
                if (CommunicationState.Opened == base.State)
                {
                    base.Fault();
                }
                channel = null;
            }
            catch (MsmqException exception)
            {
                if (exception.FaultReceiver)
                {
                    base.Fault();
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Normalized);
            }
            finally
            {
                this.receiver.ReturnMessage(msmqMessage);
            }
            return channel;
        }

        public override IAsyncResult BeginAcceptChannel(AsyncCallback callback, object state)
        {
            return this.BeginAcceptChannel(this.DefaultReceiveTimeout, callback, state);
        }

        public override IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (base.DoneReceivingInCurrentState())
            {
                return new DoneReceivingAsyncResult(callback, state);
            }
            if (!base.ReceiveParameters.ReceiveContextSettings.Enabled && (Transaction.Current == null))
            {
                base.Fault();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqTransactionRequired")));
            }
            MsmqInputMessage msmqMessage = this.receiver.TakeMessage();
            return this.receiver.BeginTryReceive(msmqMessage, timeout, MsmqTransactionMode.CurrentOrThrow, callback, state);
        }

        public override IInputSessionChannel EndAcceptChannel(IAsyncResult result)
        {
            IInputSessionChannel channel;
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            DoneReceivingAsyncResult result2 = result as DoneReceivingAsyncResult;
            if (result2 != null)
            {
                DoneReceivingAsyncResult.End(result2);
                return null;
            }
            MsmqInputMessage msmqMessage = null;
            MsmqMessageProperty msmqProperty = null;
            try
            {
                if (!this.receiver.EndTryReceive(result, out msmqMessage, out msmqProperty))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                }
                if (msmqProperty != null)
                {
                    return MsmqDecodeHelper.DecodeTransportSessiongram(this, msmqMessage, msmqProperty, this.receiveContextManager);
                }
                if (CommunicationState.Opened == base.State)
                {
                    base.Fault();
                }
                channel = null;
            }
            catch (MsmqException exception)
            {
                if (exception.FaultReceiver)
                {
                    base.Fault();
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Normalized);
            }
            finally
            {
                if (msmqMessage != null)
                {
                    this.receiver.ReturnMessage(msmqMessage);
                }
            }
            return channel;
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (base.DoneReceivingInCurrentState())
            {
                return new DoneAsyncResult(true, callback, state);
            }
            return this.receiver.BeginWaitForMessage(timeout, callback, state);
        }

        protected override void OnCloseCore(bool aborting)
        {
            if (this.receiver != null)
            {
                this.receiver.Close();
            }
            if (this.receiveContextManager != null)
            {
                this.receiveContextManager.Dispose();
            }
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            bool flag;
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            if (result is DoneAsyncResult)
            {
                return CompletedAsyncResult<bool>.End(result);
            }
            try
            {
                flag = this.receiver.EndWaitForMessage(result);
            }
            catch (MsmqException exception)
            {
                if (exception.FaultReceiver)
                {
                    base.Fault();
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Normalized);
            }
            return flag;
        }

        protected override void OnFaulted()
        {
            this.OnCloseCore(true);
            base.OnFaulted();
        }

        protected override void OnOpenCore(TimeSpan timeout)
        {
            base.OnOpenCore(timeout);
            try
            {
                this.receiver.Open();
            }
            catch (MsmqException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Normalized);
            }
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            bool flag;
            if (base.DoneReceivingInCurrentState())
            {
                return true;
            }
            try
            {
                flag = this.receiver.WaitForMessage(timeout);
            }
            catch (MsmqException exception)
            {
                if (exception.FaultReceiver)
                {
                    base.Fault();
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Normalized);
            }
            return flag;
        }

        internal System.ServiceModel.Channels.MsmqReceiveHelper MsmqReceiveHelper
        {
            get
            {
                return this.receiver;
            }
        }

        private class DoneAsyncResult : CompletedAsyncResult<bool>
        {
            internal DoneAsyncResult(bool data, AsyncCallback callback, object state) : base(data, callback, state)
            {
            }
        }
    }
}

