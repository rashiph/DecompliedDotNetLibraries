namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class ReplyOneWayChannelListener : LayeredChannelListener<IInputChannel>
    {
        private IChannelListener<IReplyChannel> innerChannelListener;
        private bool packetRoutable;

        public ReplyOneWayChannelListener(OneWayBindingElement bindingElement, BindingContext context) : base(context.Binding, context.BuildInnerChannelListener<IReplyChannel>())
        {
            this.packetRoutable = bindingElement.PacketRoutable;
        }

        protected override IInputChannel OnAcceptChannel(TimeSpan timeout)
        {
            IReplyChannel innerChannel = this.innerChannelListener.AcceptChannel(timeout);
            return this.WrapInnerChannel(innerChannel);
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelListener.BeginAcceptChannel(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelListener.BeginWaitForChannel(timeout, callback, state);
        }

        protected override IInputChannel OnEndAcceptChannel(IAsyncResult result)
        {
            IReplyChannel innerChannel = this.innerChannelListener.EndAcceptChannel(result);
            return this.WrapInnerChannel(innerChannel);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return this.innerChannelListener.EndWaitForChannel(result);
        }

        protected override void OnOpening()
        {
            this.innerChannelListener = (IChannelListener<IReplyChannel>) this.InnerChannelListener;
            base.OnOpening();
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return this.innerChannelListener.WaitForChannel(timeout);
        }

        private IInputChannel WrapInnerChannel(IReplyChannel innerChannel)
        {
            if (innerChannel == null)
            {
                return null;
            }
            return new ReplyOneWayInputChannel(this, innerChannel);
        }

        private class ReplyOneWayInputChannel : LayeredChannel<IReplyChannel>, IInputChannel, IChannel, ICommunicationObject
        {
            private bool validateHeader;

            public ReplyOneWayInputChannel(ReplyOneWayChannelListener listener, IReplyChannel innerChannel) : base(listener, innerChannel)
            {
                this.validateHeader = listener.packetRoutable;
            }

            public IAsyncResult BeginReceive(AsyncCallback callback, object state)
            {
                return this.BeginReceive(base.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ReceiveAsyncResult(base.InnerChannel, timeout, this.validateHeader, callback, state);
            }

            public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new TryReceiveAsyncResult(base.InnerChannel, timeout, this.validateHeader, callback, state);
            }

            public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.InnerChannel.BeginWaitForRequest(timeout, callback, state);
            }

            public Message EndReceive(IAsyncResult result)
            {
                return ReceiveAsyncResult.End(result);
            }

            public bool EndTryReceive(IAsyncResult result, out Message message)
            {
                return TryReceiveAsyncResult.End(result, out message);
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return base.InnerChannel.EndWaitForRequest(result);
            }

            private Message ProcessContext(RequestContext context, TimeSpan timeout)
            {
                if (context == null)
                {
                    return null;
                }
                bool flag = false;
                Message requestMessage = null;
                try
                {
                    requestMessage = context.RequestMessage;
                    requestMessage.Properties.Add(RequestContextMessageProperty.Name, new RequestContextMessageProperty(context));
                    if (this.validateHeader)
                    {
                        PacketRoutableHeader.ValidateMessage(requestMessage);
                    }
                    try
                    {
                        context.Reply(null, new TimeoutHelper(timeout).RemainingTime());
                        flag = true;
                    }
                    catch (CommunicationException exception)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                        }
                    }
                    catch (TimeoutException exception2)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                        }
                    }
                }
                finally
                {
                    if (!flag)
                    {
                        context.Abort();
                        if (requestMessage != null)
                        {
                            requestMessage.Close();
                            requestMessage = null;
                        }
                    }
                }
                return requestMessage;
            }

            public Message Receive()
            {
                return this.Receive(base.DefaultReceiveTimeout);
            }

            public Message Receive(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                RequestContext context = base.InnerChannel.ReceiveRequest(helper.RemainingTime());
                return this.ProcessContext(context, helper.RemainingTime());
            }

            public bool TryReceive(TimeSpan timeout, out Message message)
            {
                RequestContext context;
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if (base.InnerChannel.TryReceiveRequest(helper.RemainingTime(), out context))
                {
                    message = this.ProcessContext(context, helper.RemainingTime());
                    return true;
                }
                message = null;
                return false;
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return base.InnerChannel.WaitForRequest(timeout);
            }

            public EndpointAddress LocalAddress
            {
                get
                {
                    return base.InnerChannel.LocalAddress;
                }
            }

            private class ReceiveAsyncResult : ReplyOneWayChannelListener.ReplyOneWayInputChannel.ReceiveAsyncResultBase
            {
                public ReceiveAsyncResult(IReplyChannel innerChannel, TimeSpan timeout, bool validateHeader, AsyncCallback callback, object state) : base(innerChannel, timeout, validateHeader, callback, state)
                {
                }

                public static Message End(IAsyncResult result)
                {
                    return AsyncResult.End<ReplyOneWayChannelListener.ReplyOneWayInputChannel.ReceiveAsyncResult>(result).Message;
                }

                protected override IAsyncResult OnBeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    return base.InnerChannel.BeginReceiveRequest(timeout, callback, state);
                }

                protected override RequestContext OnEndReceiveRequest(IAsyncResult result)
                {
                    return base.InnerChannel.EndReceiveRequest(result);
                }
            }

            private abstract class ReceiveAsyncResultBase : AsyncResult
            {
                private RequestContext context;
                private IReplyChannel innerChannel;
                private System.ServiceModel.Channels.Message message;
                private static AsyncCallback onReceiveRequest = Fx.ThunkCallback(new AsyncCallback(ReplyOneWayChannelListener.ReplyOneWayInputChannel.ReceiveAsyncResultBase.OnReceiveRequest));
                private static AsyncCallback onReply = Fx.ThunkCallback(new AsyncCallback(ReplyOneWayChannelListener.ReplyOneWayInputChannel.ReceiveAsyncResultBase.OnReply));
                private TimeoutHelper timeoutHelper;
                private bool validateHeader;

                protected ReceiveAsyncResultBase(IReplyChannel innerChannel, TimeSpan timeout, bool validateHeader, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.innerChannel = innerChannel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.validateHeader = validateHeader;
                    IAsyncResult result = this.OnBeginReceiveRequest(this.timeoutHelper.RemainingTime(), onReceiveRequest, this);
                    if (result.CompletedSynchronously && this.HandleReceiveRequestComplete(result))
                    {
                        base.Complete(true);
                    }
                }

                private bool HandleReceiveRequestComplete(IAsyncResult result)
                {
                    this.context = this.OnEndReceiveRequest(result);
                    if (this.context == null)
                    {
                        return true;
                    }
                    bool flag = false;
                    IAsyncResult result2 = null;
                    try
                    {
                        this.message = this.context.RequestMessage;
                        this.message.Properties.Add(RequestContextMessageProperty.Name, new RequestContextMessageProperty(this.context));
                        if (this.validateHeader)
                        {
                            PacketRoutableHeader.ValidateMessage(this.message);
                        }
                        try
                        {
                            result2 = this.context.BeginReply(null, this.timeoutHelper.RemainingTime(), onReply, this);
                            flag = true;
                        }
                        catch (CommunicationException exception)
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                            }
                        }
                        catch (TimeoutException exception2)
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                            }
                        }
                    }
                    finally
                    {
                        if (!flag)
                        {
                            this.context.Abort();
                            if (this.message != null)
                            {
                                this.message.Close();
                                this.message = null;
                            }
                        }
                    }
                    return ((result2 == null) || (result2.CompletedSynchronously && this.HandleReplyComplete(result2)));
                }

                private bool HandleReplyComplete(IAsyncResult result)
                {
                    bool flag = true;
                    try
                    {
                        this.context.EndReply(result);
                        flag = false;
                    }
                    catch (CommunicationException exception)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                        }
                    }
                    catch (TimeoutException exception2)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                        }
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.context.Abort();
                        }
                    }
                    return true;
                }

                protected abstract IAsyncResult OnBeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state);
                protected abstract RequestContext OnEndReceiveRequest(IAsyncResult result);
                private static void OnReceiveRequest(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        bool flag;
                        ReplyOneWayChannelListener.ReplyOneWayInputChannel.ReceiveAsyncResultBase asyncState = (ReplyOneWayChannelListener.ReplyOneWayInputChannel.ReceiveAsyncResultBase) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            flag = asyncState.HandleReceiveRequestComplete(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            flag = true;
                            exception = exception2;
                        }
                        if (flag)
                        {
                            asyncState.Complete(false, exception);
                        }
                    }
                }

                private static void OnReply(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        bool flag;
                        ReplyOneWayChannelListener.ReplyOneWayInputChannel.ReceiveAsyncResultBase asyncState = (ReplyOneWayChannelListener.ReplyOneWayInputChannel.ReceiveAsyncResultBase) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            flag = asyncState.HandleReplyComplete(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            flag = true;
                            exception = exception2;
                        }
                        if (flag)
                        {
                            asyncState.Complete(false, exception);
                        }
                    }
                }

                protected IReplyChannel InnerChannel
                {
                    get
                    {
                        return this.innerChannel;
                    }
                }

                protected System.ServiceModel.Channels.Message Message
                {
                    get
                    {
                        return this.message;
                    }
                }
            }

            private class TryReceiveAsyncResult : ReplyOneWayChannelListener.ReplyOneWayInputChannel.ReceiveAsyncResultBase
            {
                private bool tryResult;

                public TryReceiveAsyncResult(IReplyChannel innerChannel, TimeSpan timeout, bool validateHeader, AsyncCallback callback, object state) : base(innerChannel, timeout, validateHeader, callback, state)
                {
                }

                public static bool End(IAsyncResult result, out Message message)
                {
                    ReplyOneWayChannelListener.ReplyOneWayInputChannel.TryReceiveAsyncResult result2 = AsyncResult.End<ReplyOneWayChannelListener.ReplyOneWayInputChannel.TryReceiveAsyncResult>(result);
                    message = result2.Message;
                    return result2.tryResult;
                }

                protected override IAsyncResult OnBeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    return base.InnerChannel.BeginTryReceiveRequest(timeout, callback, state);
                }

                protected override RequestContext OnEndReceiveRequest(IAsyncResult result)
                {
                    RequestContext context;
                    this.tryResult = base.InnerChannel.EndTryReceiveRequest(result, out context);
                    return context;
                }
            }
        }
    }
}

