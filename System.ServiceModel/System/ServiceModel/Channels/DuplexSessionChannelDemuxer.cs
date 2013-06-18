namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class DuplexSessionChannelDemuxer : SessionChannelDemuxer<IDuplexSessionChannel, Message>
    {
        public DuplexSessionChannelDemuxer(BindingContext context, TimeSpan peekTimeout, int maxPendingSessions) : base(context, peekTimeout, maxPendingSessions)
        {
        }

        protected override void AbortItem(Message message)
        {
            TypedChannelDemuxer.AbortMessage(message);
        }

        protected override IAsyncResult BeginReceive(IDuplexSessionChannel channel, AsyncCallback callback, object state)
        {
            return channel.BeginReceive(callback, state);
        }

        protected override IAsyncResult BeginReceive(IDuplexSessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return channel.BeginReceive(timeout, callback, state);
        }

        protected override IDuplexSessionChannel CreateChannel(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel, Message firstMessage)
        {
            return new DuplexSessionChannelWrapper(channelManager, innerChannel, firstMessage);
        }

        protected override void EndpointNotFound(IDuplexSessionChannel channel, Message message)
        {
            bool flag = true;
            try
            {
                if (base.DemuxFailureHandler != null)
                {
                    try
                    {
                        DuplexSessionDemuxFailureAsyncResult result = new DuplexSessionDemuxFailureAsyncResult(base.DemuxFailureHandler, channel, message, Fx.ThunkCallback(new AsyncCallback(this.EndpointNotFoundCallback)), new ChannelAndMessageAsyncState(channel, message));
                        result.Start();
                        if (!result.CompletedSynchronously)
                        {
                            flag = false;
                        }
                        else
                        {
                            DuplexSessionDemuxFailureAsyncResult.End(result);
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
                    this.AbortItem(message);
                    channel.Abort();
                }
            }
        }

        private void EndpointNotFoundCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ChannelAndMessageAsyncState asyncState = (ChannelAndMessageAsyncState) result.AsyncState;
                bool flag = true;
                try
                {
                    DuplexSessionDemuxFailureAsyncResult.End(result);
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
                        this.AbortItem(asyncState.message);
                        asyncState.channel.Abort();
                    }
                }
            }
        }

        protected override Message EndReceive(IDuplexSessionChannel channel, IAsyncResult result)
        {
            return channel.EndReceive(result);
        }

        protected override Message GetMessage(Message message)
        {
            return message;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ChannelAndMessageAsyncState
        {
            public IChannel channel;
            public Message message;
            public ChannelAndMessageAsyncState(IChannel channel, Message message)
            {
                this.channel = channel;
                this.message = message;
            }
        }
    }
}

