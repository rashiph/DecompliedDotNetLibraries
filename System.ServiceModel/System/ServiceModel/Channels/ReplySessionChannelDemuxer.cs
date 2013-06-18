namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class ReplySessionChannelDemuxer : SessionChannelDemuxer<IReplySessionChannel, RequestContext>
    {
        public ReplySessionChannelDemuxer(BindingContext context, TimeSpan peekTimeout, int maxPendingSessions) : base(context, peekTimeout, maxPendingSessions)
        {
        }

        protected override void AbortItem(RequestContext request)
        {
            TypedChannelDemuxer.AbortMessage(request.RequestMessage);
            request.Abort();
        }

        protected override IAsyncResult BeginReceive(IReplySessionChannel channel, AsyncCallback callback, object state)
        {
            return channel.BeginReceiveRequest(callback, state);
        }

        protected override IAsyncResult BeginReceive(IReplySessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return channel.BeginReceiveRequest(timeout, callback, state);
        }

        protected override IReplySessionChannel CreateChannel(ChannelManagerBase channelManager, IReplySessionChannel innerChannel, RequestContext firstRequest)
        {
            return new ReplySessionChannelWrapper(channelManager, innerChannel, firstRequest);
        }

        protected override void EndpointNotFound(IReplySessionChannel channel, RequestContext request)
        {
            bool flag = true;
            try
            {
                if (base.DemuxFailureHandler != null)
                {
                    try
                    {
                        ReplySessionDemuxFailureAsyncResult result = new ReplySessionDemuxFailureAsyncResult(base.DemuxFailureHandler, request, channel, Fx.ThunkCallback(new AsyncCallback(this.EndpointNotFoundCallback)), new ChannelAndRequestAsyncState(channel, request));
                        result.Start();
                        if (!result.CompletedSynchronously)
                        {
                            flag = false;
                        }
                        else
                        {
                            ReplySessionDemuxFailureAsyncResult.End(result);
                            flag = false;
                        }
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
                    catch (ObjectDisposedException exception3)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                        }
                    }
                    catch (Exception exception4)
                    {
                        if (Fx.IsFatal(exception4))
                        {
                            throw;
                        }
                        base.HandleUnknownException(exception4);
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    this.AbortItem(request);
                    channel.Abort();
                }
            }
        }

        private void EndpointNotFoundCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ChannelAndRequestAsyncState asyncState = (ChannelAndRequestAsyncState) result.AsyncState;
                bool flag = true;
                try
                {
                    ReplySessionDemuxFailureAsyncResult.End(result);
                    flag = false;
                }
                catch (TimeoutException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }
                catch (CommunicationException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                }
                catch (ObjectDisposedException exception3)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                    }
                }
                catch (Exception exception4)
                {
                    if (Fx.IsFatal(exception4))
                    {
                        throw;
                    }
                    base.HandleUnknownException(exception4);
                }
                finally
                {
                    if (flag)
                    {
                        this.AbortItem(asyncState.request);
                        asyncState.channel.Abort();
                    }
                }
            }
        }

        protected override RequestContext EndReceive(IReplySessionChannel channel, IAsyncResult result)
        {
            return channel.EndReceiveRequest(result);
        }

        protected override Message GetMessage(RequestContext request)
        {
            return request.RequestMessage;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ChannelAndRequestAsyncState
        {
            public IChannel channel;
            public RequestContext request;
            public ChannelAndRequestAsyncState(IChannel channel, RequestContext request)
            {
                this.channel = channel;
                this.request = request;
            }
        }
    }
}

