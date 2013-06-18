namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal abstract class ReliableListenerOverDatagram<TChannel, TReliableChannel, TInnerChannel, TItem> : ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel> where TChannel: class, IChannel where TReliableChannel: class, IChannel where TInnerChannel: class, IChannel where TItem: class, IDisposable
    {
        private Action<object> asyncHandleReceiveComplete;
        private ChannelTracker<TInnerChannel, object> channelTracker;
        private AsyncCallback onTryReceiveComplete;

        protected ReliableListenerOverDatagram(ReliableSessionBindingElement binding, BindingContext context) : base(binding, context)
        {
            this.asyncHandleReceiveComplete = new Action<object>(this.AsyncHandleReceiveComplete);
            this.onTryReceiveComplete = Fx.ThunkCallback(new AsyncCallback(this.OnTryReceiveComplete));
            this.channelTracker = new ChannelTracker<TInnerChannel, object>();
        }

        protected override void AbortInnerListener()
        {
            base.AbortInnerListener();
            this.channelTracker.Abort();
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
                if ((item != null) && this.HandleReceiveComplete(item, asyncState))
                {
                    this.StartReceiving(asyncState, true);
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

        protected override IAsyncResult BeginCloseInnerListener(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.BeginCloseInnerListener), new ChainedEndHandler(this.EndCloseInnerListener), new ChainedBeginHandler(this.channelTracker.BeginClose), new ChainedEndHandler(this.channelTracker.EndClose));
        }

        private bool BeginProcessItem(TItem item, WsrmMessageInfo info, TInnerChannel channel, out TReliableChannel reliableChannel, out bool newChannel, out bool dispatch)
        {
            Message faultReply;
            dispatch = false;
            reliableChannel = default(TReliableChannel);
            newChannel = false;
            if (info.FaultReply != null)
            {
                faultReply = info.FaultReply;
            }
            else if (info.CreateSequenceInfo == null)
            {
                UniqueId id;
                reliableChannel = base.GetChannel(info, out id);
                if (((TReliableChannel) reliableChannel) != null)
                {
                    return true;
                }
                if (id == null)
                {
                    this.DisposeItem(item);
                    return true;
                }
                faultReply = new UnknownSequenceFault(id).CreateMessage(base.MessageVersion, base.ReliableMessagingVersion);
            }
            else
            {
                reliableChannel = base.ProcessCreateSequence(info, channel, out dispatch, out newChannel);
                if (((TReliableChannel) reliableChannel) != null)
                {
                    return true;
                }
                faultReply = info.FaultReply;
            }
            try
            {
                this.SendReply(faultReply, channel, item);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (!base.HandleException(exception, channel))
                {
                    channel.Abort();
                    return false;
                }
            }
            finally
            {
                faultReply.Close();
                this.DisposeItem(item);
            }
            return true;
        }

        protected abstract IAsyncResult BeginTryReceiveItem(TInnerChannel channel, AsyncCallback callback, object state);
        protected override void CloseInnerListener(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.CloseInnerListener(helper.RemainingTime());
            this.channelTracker.Close(helper.RemainingTime());
        }

        protected abstract void DisposeItem(TItem item);
        protected override void EndCloseInnerListener(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        private void EndProcessItem(TItem item, WsrmMessageInfo info, TReliableChannel channel, bool dispatch)
        {
            this.ProcessSequencedItem(channel, item, info);
            if (dispatch)
            {
                base.Dispatch();
            }
        }

        protected abstract void EndTryReceiveItem(TInnerChannel channel, IAsyncResult result, out TItem item);
        protected abstract Message GetMessage(TItem item);
        private bool HandleReceiveComplete(TItem item, TInnerChannel channel)
        {
            Message message = null;
            TReliableChannel local;
            bool flag;
            bool flag2;
            try
            {
                message = this.GetMessage(item);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (!base.HandleException(exception, this))
                {
                    throw;
                }
                item.Dispose();
                return true;
            }
            WsrmMessageInfo info = WsrmMessageInfo.Get(base.MessageVersion, base.ReliableMessagingVersion, channel, null, message);
            if (info.ParsingException != null)
            {
                this.DisposeItem(item);
                return true;
            }
            if (!this.BeginProcessItem(item, info, channel, out local, out flag, out flag2))
            {
                return false;
            }
            if (local == null)
            {
                this.DisposeItem(item);
                return true;
            }
            if (flag2 || !flag)
            {
                this.StartReceiving(channel, false);
                this.EndProcessItem(item, info, local, flag2);
                return false;
            }
            this.EndProcessItem(item, info, local, flag2);
            return true;
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.channelTracker.BeginOpen), new ChainedEndHandler(this.channelTracker.EndOpen), new ChainedBeginHandler(this.OnBeginOpen), new ChainedEndHandler(this.OnEndOpen));
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        protected override void OnInnerChannelAccepted(TInnerChannel channel)
        {
            base.OnInnerChannelAccepted(channel);
            this.channelTracker.PrepareChannel(channel);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.channelTracker.Open(helper.RemainingTime());
            base.OnOpen(helper.RemainingTime());
        }

        private void OnTryReceiveComplete(IAsyncResult result)
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
                    if ((item != null) && this.HandleReceiveComplete(item, asyncState))
                    {
                        this.StartReceiving(asyncState, true);
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
                this.channelTracker.Add(channel, null);
                this.StartReceiving(channel, false);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                base.Fault(exception);
            }
        }

        protected abstract void ProcessSequencedItem(TReliableChannel reliableChannel, TItem item, WsrmMessageInfo info);
        protected abstract void SendReply(Message reply, TInnerChannel channel, TItem item);
        private void StartReceiving(TInnerChannel channel, bool canBlock)
        {
            TItem local;
            do
            {
                local = default(TItem);
                try
                {
                    IAsyncResult state = this.BeginTryReceiveItem(channel, this.onTryReceiveComplete, channel);
                    if (!state.CompletedSynchronously)
                    {
                        break;
                    }
                    if (!canBlock)
                    {
                        ActionItem.Schedule(this.asyncHandleReceiveComplete, state);
                        break;
                    }
                    this.EndTryReceiveItem(channel, state, out local);
                    if (local == null)
                    {
                        break;
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (!base.HandleException(exception, channel))
                    {
                        channel.Abort();
                        break;
                    }
                }
            }
            while ((local == null) || this.HandleReceiveComplete(local, channel));
        }
    }
}

