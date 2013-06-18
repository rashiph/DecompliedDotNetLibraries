namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal static class ReliableChannelBinderHelper
    {
        internal static IAsyncResult BeginCloseDuplexSessionChannel(ReliableChannelBinder<IDuplexSessionChannel> binder, IDuplexSessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseDuplexSessionChannelAsyncResult(binder, channel, timeout, callback, state);
        }

        internal static IAsyncResult BeginCloseReplySessionChannel(ReliableChannelBinder<IReplySessionChannel> binder, IReplySessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseReplySessionChannelAsyncResult(binder, channel, timeout, callback, state);
        }

        internal static void CloseDuplexSessionChannel(ReliableChannelBinder<IDuplexSessionChannel> binder, IDuplexSessionChannel channel, TimeSpan timeout)
        {
            Message message;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            channel.Session.CloseOutputSession(helper.RemainingTime());
            binder.WaitForPendingOperations(helper.RemainingTime());
            TimeSpan span = helper.RemainingTime();
            bool flag = span == TimeSpan.Zero;
        Label_003B:
            message = null;
            bool flag2 = true;
            try
            {
                bool flag3 = channel.TryReceive(span, out message);
                flag2 = false;
                if (flag3 && (message == null))
                {
                    channel.Close(helper.RemainingTime());
                    return;
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (!flag2)
                {
                    throw;
                }
                if (!MaskHandled(binder.DefaultMaskingMode) || !binder.IsHandleable(exception))
                {
                    throw;
                }
                flag2 = false;
            }
            finally
            {
                if (message != null)
                {
                    message.Close();
                }
                if (flag2)
                {
                    channel.Abort();
                }
            }
            if (!flag && (channel.State == CommunicationState.Opened))
            {
                span = helper.RemainingTime();
                flag = span == TimeSpan.Zero;
                goto Label_003B;
            }
            channel.Abort();
        }

        internal static void CloseReplySessionChannel(ReliableChannelBinder<IReplySessionChannel> binder, IReplySessionChannel channel, TimeSpan timeout)
        {
            RequestContext context;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            binder.WaitForPendingOperations(helper.RemainingTime());
            TimeSpan span = helper.RemainingTime();
            bool flag = span == TimeSpan.Zero;
        Label_0029:
            context = null;
            bool flag2 = true;
            try
            {
                bool flag3 = channel.TryReceiveRequest(span, out context);
                flag2 = false;
                if (flag3 && (context == null))
                {
                    channel.Close(helper.RemainingTime());
                    return;
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (!flag2)
                {
                    throw;
                }
                if (!MaskHandled(binder.DefaultMaskingMode) || !binder.IsHandleable(exception))
                {
                    throw;
                }
                flag2 = false;
            }
            finally
            {
                if (context != null)
                {
                    context.RequestMessage.Close();
                    context.Close();
                }
                if (flag2)
                {
                    channel.Abort();
                }
            }
            if (!flag && (channel.State == CommunicationState.Opened))
            {
                span = helper.RemainingTime();
                flag = span == TimeSpan.Zero;
                goto Label_0029;
            }
            channel.Abort();
        }

        internal static void EndCloseDuplexSessionChannel(IDuplexSessionChannel channel, IAsyncResult result)
        {
            CloseDuplexSessionChannelAsyncResult.End(result);
        }

        internal static void EndCloseReplySessionChannel(IReplySessionChannel channel, IAsyncResult result)
        {
            CloseReplySessionChannelAsyncResult.End(result);
        }

        internal static bool MaskHandled(MaskingMode maskingMode)
        {
            return ((maskingMode & MaskingMode.Handled) == MaskingMode.Handled);
        }

        internal static bool MaskUnhandled(MaskingMode maskingMode)
        {
            return ((maskingMode & MaskingMode.Unhandled) == MaskingMode.Unhandled);
        }

        private sealed class CloseDuplexSessionChannelAsyncResult : ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<IDuplexSessionChannel, Message>
        {
            private static AsyncCallback onCloseOutputSessionCompleteStatic = Fx.ThunkCallback(new AsyncCallback(ReliableChannelBinderHelper.CloseDuplexSessionChannelAsyncResult.OnCloseOutputSessionCompleteStatic));

            public CloseDuplexSessionChannelAsyncResult(ReliableChannelBinder<IDuplexSessionChannel> binder, IDuplexSessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state) : base(binder, channel, timeout, callback, state)
            {
                bool flag = false;
                IAsyncResult result = base.Channel.Session.BeginCloseOutputSession(base.RemainingTime, onCloseOutputSessionCompleteStatic, this);
                if (result.CompletedSynchronously)
                {
                    flag = this.HandleCloseOutputSessionComplete(result);
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            protected override IAsyncResult BeginTryInput(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.Channel.BeginTryReceive(timeout, callback, state);
            }

            protected override void DisposeItem(Message item)
            {
                item.Close();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ReliableChannelBinderHelper.CloseDuplexSessionChannelAsyncResult>(result);
            }

            protected override bool EndTryInput(IAsyncResult result, out Message item)
            {
                return base.Channel.EndTryReceive(result, out item);
            }

            private bool HandleCloseOutputSessionComplete(IAsyncResult result)
            {
                base.Channel.Session.EndCloseOutputSession(result);
                return base.Begin();
            }

            private static void OnCloseOutputSessionCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ReliableChannelBinderHelper.CloseDuplexSessionChannelAsyncResult asyncState = (ReliableChannelBinderHelper.CloseDuplexSessionChannelAsyncResult) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        flag = asyncState.HandleCloseOutputSessionComplete(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (flag || (exception != null))
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }
        }

        private abstract class CloseInputSessionChannelAsyncResult<TChannel, TItem> : AsyncResult where TChannel: class, IChannel where TItem: class
        {
            private ReliableChannelBinder<TChannel> binder;
            private TChannel channel;
            private bool lastReceive;
            private static AsyncCallback onChannelCloseCompleteStatic;
            private static AsyncCallback onInputCompleteStatic;
            private static AsyncCallback onWaitForPendingOperationsCompleteStatic;
            private TimeoutHelper timeoutHelper;

            static CloseInputSessionChannelAsyncResult()
            {
                ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<TChannel, TItem>.onChannelCloseCompleteStatic = Fx.ThunkCallback(new AsyncCallback(ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<TChannel, TItem>.OnChannelCloseCompleteStatic));
                ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<TChannel, TItem>.onInputCompleteStatic = Fx.ThunkCallback(new AsyncCallback(ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<TChannel, TItem>.OnInputCompleteStatic));
                ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<TChannel, TItem>.onWaitForPendingOperationsCompleteStatic = Fx.ThunkCallback(new AsyncCallback(ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<TChannel, TItem>.OnWaitForPendingOperationsCompleteStatic));
            }

            protected CloseInputSessionChannelAsyncResult(ReliableChannelBinder<TChannel> binder, TChannel channel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.binder = binder;
                this.channel = channel;
                this.timeoutHelper = new TimeoutHelper(timeout);
            }

            protected bool Begin()
            {
                bool flag = false;
                IAsyncResult result = this.binder.BeginWaitForPendingOperations(this.RemainingTime, ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<TChannel, TItem>.onWaitForPendingOperationsCompleteStatic, this);
                if (result.CompletedSynchronously)
                {
                    flag = this.HandleWaitForPendingOperationsComplete(result);
                }
                return flag;
            }

            protected abstract IAsyncResult BeginTryInput(TimeSpan timeout, AsyncCallback callback, object state);
            protected abstract void DisposeItem(TItem item);
            protected abstract bool EndTryInput(IAsyncResult result, out TItem item);
            private void HandleChannelCloseComplete(IAsyncResult result)
            {
                this.channel.EndClose(result);
            }

            private bool HandleInputComplete(IAsyncResult result, out bool gotEof)
            {
                bool flag3;
                TItem item = default(TItem);
                bool flag = true;
                gotEof = false;
                try
                {
                    bool flag2 = false;
                    flag2 = this.EndTryInput(result, out item);
                    flag = false;
                    if (!flag2 || (item != null))
                    {
                        if (this.lastReceive || (this.channel.State != CommunicationState.Opened))
                        {
                            this.channel.Abort();
                            return true;
                        }
                        return false;
                    }
                    gotEof = true;
                    result = this.channel.BeginClose(this.RemainingTime, ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<TChannel, TItem>.onChannelCloseCompleteStatic, this);
                    if (result.CompletedSynchronously)
                    {
                        this.HandleChannelCloseComplete(result);
                        return true;
                    }
                    flag3 = false;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (!flag)
                    {
                        throw;
                    }
                    if (!ReliableChannelBinderHelper.MaskHandled(this.binder.DefaultMaskingMode) || !this.binder.IsHandleable(exception))
                    {
                        throw;
                    }
                    if (this.lastReceive || (this.channel.State != CommunicationState.Opened))
                    {
                        this.channel.Abort();
                        return true;
                    }
                    flag3 = false;
                }
                finally
                {
                    if (item != null)
                    {
                        this.DisposeItem(item);
                    }
                    if (flag)
                    {
                        this.channel.Abort();
                    }
                }
                return flag3;
            }

            private bool HandleWaitForPendingOperationsComplete(IAsyncResult result)
            {
                this.binder.EndWaitForPendingOperations(result);
                return this.WaitForEof();
            }

            private static void OnChannelCloseCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<TChannel, TItem> asyncState = (ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<TChannel, TItem>) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.HandleChannelCloseComplete(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    asyncState.Complete(false, exception);
                }
            }

            private static void OnInputCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<TChannel, TItem> asyncState = (ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<TChannel, TItem>) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        bool flag2;
                        flag = asyncState.HandleInputComplete(result, out flag2);
                        if (!flag && !flag2)
                        {
                            flag = asyncState.WaitForEof();
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (flag || (exception != null))
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private static void OnWaitForPendingOperationsCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<TChannel, TItem> asyncState = (ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<TChannel, TItem>) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        flag = asyncState.HandleWaitForPendingOperationsComplete(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (flag || (exception != null))
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private bool WaitForEof()
            {
                TimeSpan remainingTime = this.RemainingTime;
                this.lastReceive = remainingTime == TimeSpan.Zero;
                while (true)
                {
                    IAsyncResult result = null;
                    try
                    {
                        result = this.BeginTryInput(remainingTime, ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<TChannel, TItem>.onInputCompleteStatic, this);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        if (!ReliableChannelBinderHelper.MaskHandled(this.binder.DefaultMaskingMode) || !this.binder.IsHandleable(exception))
                        {
                            throw;
                        }
                    }
                    if (result != null)
                    {
                        bool flag;
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        bool flag2 = this.HandleInputComplete(result, out flag);
                        if (flag2 || flag)
                        {
                            return flag2;
                        }
                    }
                    if (this.lastReceive || (this.channel.State != CommunicationState.Opened))
                    {
                        this.channel.Abort();
                        return true;
                    }
                    remainingTime = this.RemainingTime;
                    this.lastReceive = remainingTime == TimeSpan.Zero;
                }
            }

            protected TChannel Channel
            {
                get
                {
                    return this.channel;
                }
            }

            protected TimeSpan RemainingTime
            {
                get
                {
                    return this.timeoutHelper.RemainingTime();
                }
            }
        }

        private sealed class CloseReplySessionChannelAsyncResult : ReliableChannelBinderHelper.CloseInputSessionChannelAsyncResult<IReplySessionChannel, RequestContext>
        {
            public CloseReplySessionChannelAsyncResult(ReliableChannelBinder<IReplySessionChannel> binder, IReplySessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state) : base(binder, channel, timeout, callback, state)
            {
                if (base.Begin())
                {
                    base.Complete(true);
                }
            }

            protected override IAsyncResult BeginTryInput(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.Channel.BeginTryReceiveRequest(timeout, callback, state);
            }

            protected override void DisposeItem(RequestContext item)
            {
                item.RequestMessage.Close();
                item.Close();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ReliableChannelBinderHelper.CloseReplySessionChannelAsyncResult>(result);
            }

            protected override bool EndTryInput(IAsyncResult result, out RequestContext item)
            {
                return base.Channel.EndTryReceiveRequest(result, out item);
            }
        }
    }
}

