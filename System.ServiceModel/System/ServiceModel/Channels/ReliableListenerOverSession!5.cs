namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    internal abstract class ReliableListenerOverSession<TChannel, TReliableChannel, TInnerChannel, TInnerSession, TItem> : ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel> where TChannel: class, IChannel where TReliableChannel: class, IChannel where TInnerChannel: class, IChannel, ISessionChannel<TInnerSession> where TInnerSession: ISession where TItem: IDisposable
    {
        private Action<object> asyncHandleReceiveComplete;
        private AsyncCallback onReceiveComplete;

        protected ReliableListenerOverSession(ReliableSessionBindingElement binding, BindingContext context) : base(binding, context)
        {
            this.asyncHandleReceiveComplete = new Action<object>(this.AsyncHandleReceiveComplete);
            this.onReceiveComplete = Fx.ThunkCallback(new AsyncCallback(this.OnReceiveComplete));
        }

        private void AsyncHandleReceiveComplete(object state)
        {
            try
            {
                IAsyncResult result = (IAsyncResult) state;
                TInnerChannel asyncState = (TInnerChannel) result.AsyncState;
                TItem item = default(TItem);
                try
                {
                    this.EndTryReceiveItem(asyncState, result, out item);
                    if (item == null)
                    {
                        asyncState.Close();
                        return;
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (!base.HandleException(exception, asyncState))
                    {
                        asyncState.Abort();
                        return;
                    }
                }
                if (item != null)
                {
                    this.HandleReceiveComplete(item, asyncState);
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                base.Fault(exception2);
            }
        }

        protected abstract IAsyncResult BeginTryReceiveItem(TInnerChannel channel, AsyncCallback callback, object state);
        protected abstract void DisposeItem(TItem item);
        protected abstract void EndTryReceiveItem(TInnerChannel channel, IAsyncResult result, out TItem item);
        protected abstract Message GetMessage(TItem item);
        private void HandleReceiveComplete(TItem item, TInnerChannel channel)
        {
            WsrmMessageInfo info = WsrmMessageInfo.Get(base.MessageVersion, base.ReliableMessagingVersion, channel, channel.Session as ISecureConversationSession, this.GetMessage(item));
            if (info.ParsingException != null)
            {
                this.DisposeItem(item);
                channel.Abort();
            }
            else
            {
                TReliableChannel reliableChannel = default(TReliableChannel);
                bool dispatch = false;
                bool newChannel = false;
                Message reply = null;
                if (info.FaultReply != null)
                {
                    reply = info.FaultReply;
                }
                else if (info.CreateSequenceInfo == null)
                {
                    UniqueId id;
                    reliableChannel = base.GetChannel(info, out id);
                    if ((reliableChannel == null) && (id == null))
                    {
                        this.DisposeItem(item);
                        channel.Abort();
                        return;
                    }
                    if (reliableChannel == null)
                    {
                        reply = new UnknownSequenceFault(id).CreateMessage(base.MessageVersion, base.ReliableMessagingVersion);
                    }
                }
                else
                {
                    reliableChannel = base.ProcessCreateSequence(info, channel, out dispatch, out newChannel);
                    if (reliableChannel == null)
                    {
                        reply = info.FaultReply;
                    }
                }
                if (reliableChannel != null)
                {
                    this.ProcessSequencedItem(channel, item, reliableChannel, info, newChannel);
                    if (dispatch)
                    {
                        base.Dispatch();
                    }
                }
                else
                {
                    try
                    {
                        this.SendReply(reply, channel, item);
                        channel.Close();
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        if (DiagnosticUtility.ShouldTraceError)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                        }
                        channel.Abort();
                    }
                    finally
                    {
                        reply.Close();
                        this.DisposeItem(item);
                    }
                }
            }
        }

        private void OnReceiveComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                try
                {
                    TInnerChannel asyncState = (TInnerChannel) result.AsyncState;
                    TItem item = default(TItem);
                    try
                    {
                        this.EndTryReceiveItem(asyncState, result, out item);
                        if (item == null)
                        {
                            asyncState.Close();
                            return;
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        if (!base.HandleException(exception, asyncState))
                        {
                            asyncState.Abort();
                            return;
                        }
                    }
                    if (item != null)
                    {
                        this.HandleReceiveComplete(item, asyncState);
                    }
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    base.Fault(exception2);
                }
            }
        }

        protected override void ProcessChannel(TInnerChannel channel)
        {
            try
            {
                IAsyncResult state = this.BeginTryReceiveItem(channel, this.onReceiveComplete, channel);
                if (state.CompletedSynchronously)
                {
                    ActionItem.Schedule(this.asyncHandleReceiveComplete, state);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (DiagnosticUtility.ShouldTraceError)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                }
                channel.Abort();
            }
        }

        protected abstract void ProcessSequencedItem(TInnerChannel channel, TItem item, TReliableChannel reliableChannel, WsrmMessageInfo info, bool newChannel);
        protected abstract void SendReply(Message reply, TInnerChannel channel, TItem item);
    }
}

