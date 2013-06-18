namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal sealed class ReliableOutputSessionChannelOverDuplex : ReliableOutputSessionChannel
    {
        private static AsyncCallback onReceiveCompleted = Fx.ThunkCallback(new AsyncCallback(ReliableOutputSessionChannelOverDuplex.OnReceiveCompletedStatic));

        public ReliableOutputSessionChannelOverDuplex(ChannelManagerBase factory, IReliableFactorySettings settings, IClientReliableChannelBinder binder, FaultHelper faultHelper, LateBoundChannelParameterCollection channelParameters) : base(factory, settings, binder, faultHelper, channelParameters)
        {
        }

        protected override ReliableRequestor CreateRequestor()
        {
            return new SendWaitReliableRequestor();
        }

        protected override IAsyncResult OnConnectionBeginSend(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException, AsyncCallback callback, object state)
        {
            ReliableBinderSendAsyncResult result = new ReliableBinderSendAsyncResult(callback, state) {
                Binder = base.Binder,
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
            ReliableBinderSendAsyncResult result = new ReliableBinderSendAsyncResult(callback, state) {
                Binder = base.Binder,
                MaskingMode = MaskingMode.Unhandled,
                Message = message
            };
            result.Begin(timeout);
            return result;
        }

        protected override void OnConnectionEndSend(IAsyncResult result)
        {
            Exception exception;
            ReliableBinderSendAsyncResult.End(result, out exception);
            ReliableBinderSendAsyncResult result2 = (ReliableBinderSendAsyncResult) result;
            if (result2.MessageAttemptInfo.RetryCount == base.Settings.MaxRetryCount)
            {
                base.MaxRetryCountException = exception;
            }
        }

        protected override void OnConnectionEndSendMessage(IAsyncResult result)
        {
            ReliableBinderSendAsyncResult.End(result);
        }

        protected override void OnConnectionSend(Message message, TimeSpan timeout, bool saveHandledException, bool maskUnhandledException)
        {
            MaskingMode maskingMode = maskUnhandledException ? MaskingMode.Unhandled : MaskingMode.None;
            if (saveHandledException)
            {
                try
                {
                    base.Binder.Send(message, timeout, maskingMode);
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
            base.Binder.Send(message, timeout, maskingMode);
        }

        protected override void OnConnectionSendMessage(Message message, TimeSpan timeout, MaskingMode maskingMode)
        {
            base.Binder.Send(message, timeout, maskingMode);
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            if (Thread.CurrentThread.IsThreadPoolThread)
            {
                try
                {
                    this.StartReceiving();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    base.ReliableSession.OnUnknownException(exception);
                }
            }
            else
            {
                ActionItem.Schedule(new Action<object>(ReliableOutputSessionChannelOverDuplex.StartReceiving), this);
            }
        }

        private void OnReceiveCompleted(IAsyncResult result)
        {
            RequestContext context;
            if (base.Binder.EndTryReceive(result, out context))
            {
                if (context != null)
                {
                    using (context)
                    {
                        Message requestMessage = context.RequestMessage;
                        base.ProcessMessage(requestMessage);
                        context.Close(this.DefaultCloseTimeout);
                    }
                    base.Binder.BeginTryReceive(TimeSpan.MaxValue, onReceiveCompleted, this);
                }
                else if (!base.Connection.Closed && (base.Binder.State == CommunicationState.Opened))
                {
                    Exception e = new CommunicationException(System.ServiceModel.SR.GetString("EarlySecurityClose"));
                    base.ReliableSession.OnLocalFault(e, (Message) null, null);
                }
            }
            else
            {
                base.Binder.BeginTryReceive(TimeSpan.MaxValue, onReceiveCompleted, this);
            }
        }

        private static void OnReceiveCompletedStatic(IAsyncResult result)
        {
            ReliableOutputSessionChannelOverDuplex asyncState = (ReliableOutputSessionChannelOverDuplex) result.AsyncState;
            try
            {
                asyncState.OnReceiveCompleted(result);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                asyncState.ReliableSession.OnUnknownException(exception);
            }
        }

        protected override WsrmFault ProcessRequestorResponse(ReliableRequestor requestor, string requestName, WsrmMessageInfo info)
        {
            if (requestor != null)
            {
                requestor.SetInfo(info);
                return null;
            }
            string faultReason = System.ServiceModel.SR.GetString("ReceivedResponseBeforeRequestFaultString", new object[] { requestName });
            string exceptionMessage = System.ServiceModel.SR.GetString("ReceivedResponseBeforeRequestExceptionString", new object[] { requestName });
            return SequenceTerminatedFault.CreateProtocolFault(base.ReliableSession.OutputID, faultReason, exceptionMessage);
        }

        private void StartReceiving()
        {
            base.Binder.BeginTryReceive(TimeSpan.MaxValue, onReceiveCompleted, this);
        }

        private static void StartReceiving(object state)
        {
            ReliableOutputSessionChannelOverDuplex duplex = (ReliableOutputSessionChannelOverDuplex) state;
            try
            {
                duplex.StartReceiving();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                duplex.ReliableSession.OnUnknownException(exception);
            }
        }

        protected override bool RequestAcks
        {
            get
            {
                return true;
            }
        }
    }
}

