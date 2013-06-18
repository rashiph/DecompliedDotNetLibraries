namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal sealed class ReliableOutputSessionChannelOverRequest : ReliableOutputSessionChannel
    {
        private IClientReliableChannelBinder binder;

        public ReliableOutputSessionChannelOverRequest(ChannelManagerBase factory, IReliableFactorySettings settings, IClientReliableChannelBinder binder, FaultHelper faultHelper, LateBoundChannelParameterCollection channelParameters) : base(factory, settings, binder, faultHelper, channelParameters)
        {
            this.binder = binder;
        }

        protected override ReliableRequestor CreateRequestor()
        {
            return new RequestReliableRequestor();
        }

        protected override IAsyncResult OnConnectionBeginSend(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException, AsyncCallback callback, object state)
        {
            ReliableBinderRequestAsyncResult result = new ReliableBinderRequestAsyncResult(callback, state) {
                Binder = this.binder,
                MessageAttemptInfo = attemptInfo,
                MaskingMode = maskUnhandledException ? MaskingMode.Unhandled : MaskingMode.None
            };
            if (attemptInfo.RetryCount < base.Settings.MaxRetryCount)
            {
                result.MaskingMode |= MaskingMode.Handled;
                result.SaveHandledException = false;
            }
            else
            {
                result.SaveHandledException = true;
            }
            result.Begin(timeout);
            return result;
        }

        protected override IAsyncResult OnConnectionBeginSendMessage(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            ReliableBinderRequestAsyncResult result = new ReliableBinderRequestAsyncResult(callback, state) {
                Binder = this.binder,
                MaskingMode = MaskingMode.Handled,
                Message = message
            };
            result.Begin(timeout);
            return result;
        }

        protected override void OnConnectionEndSend(IAsyncResult result)
        {
            Exception exception;
            Message message = ReliableBinderRequestAsyncResult.End(result, out exception);
            ReliableBinderRequestAsyncResult result2 = (ReliableBinderRequestAsyncResult) result;
            if (result2.MessageAttemptInfo.RetryCount == base.Settings.MaxRetryCount)
            {
                base.MaxRetryCountException = exception;
            }
            if (message != null)
            {
                base.ProcessMessage(message);
            }
        }

        protected override void OnConnectionEndSendMessage(IAsyncResult result)
        {
            Message message = ReliableBinderRequestAsyncResult.End(result);
            if (message != null)
            {
                base.ProcessMessage(message);
            }
        }

        protected override void OnConnectionSend(Message message, TimeSpan timeout, bool saveHandledException, bool maskUnhandledException)
        {
            MaskingMode maskingMode = maskUnhandledException ? MaskingMode.Unhandled : MaskingMode.None;
            Message message2 = null;
            if (saveHandledException)
            {
                try
                {
                    message2 = this.binder.Request(message, timeout, maskingMode);
                    return;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (!base.Binder.IsHandleable(exception))
                    {
                        throw;
                    }
                    base.MaxRetryCountException = exception;
                    return;
                }
            }
            maskingMode |= MaskingMode.Handled;
            message2 = this.binder.Request(message, timeout, maskingMode);
            if (message2 != null)
            {
                base.ProcessMessage(message2);
            }
        }

        protected override void OnConnectionSendMessage(Message message, TimeSpan timeout, MaskingMode maskingMode)
        {
            Message message2 = this.binder.Request(message, timeout, maskingMode);
            if (message2 != null)
            {
                base.ProcessMessage(message2);
            }
        }

        protected override WsrmFault ProcessRequestorResponse(ReliableRequestor requestor, string requestName, WsrmMessageInfo info)
        {
            string faultReason = System.ServiceModel.SR.GetString("ReceivedResponseBeforeRequestFaultString", new object[] { requestName });
            string exceptionMessage = System.ServiceModel.SR.GetString("ReceivedResponseBeforeRequestExceptionString", new object[] { requestName });
            return SequenceTerminatedFault.CreateProtocolFault(base.ReliableSession.OutputID, faultReason, exceptionMessage);
        }

        protected override bool RequestAcks
        {
            get
            {
                return false;
            }
        }
    }
}

