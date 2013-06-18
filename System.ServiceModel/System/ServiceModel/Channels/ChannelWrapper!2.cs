namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Threading;

    internal abstract class ChannelWrapper<TChannel, TItem> : LayeredChannel<TChannel> where TChannel: class, IChannel where TItem: class, IDisposable
    {
        private TItem firstItem;

        public ChannelWrapper(ChannelManagerBase channelManager, TChannel innerChannel, TItem firstItem) : base(channelManager, innerChannel)
        {
            this.firstItem = firstItem;
        }

        protected abstract void CloseFirstItem(TimeSpan timeout);
        protected TItem GetFirstItem()
        {
            return Interlocked.Exchange<TItem>(ref this.firstItem, default(TItem));
        }

        protected bool HaveFirstItem()
        {
            return (this.firstItem != null);
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            this.CloseFirstItem(TimeSpan.Zero);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.CloseFirstItem(helper.RemainingTime());
            return base.OnBeginClose(helper.RemainingTime(), callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.CloseFirstItem(helper.RemainingTime());
            base.OnClose(helper.RemainingTime());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            base.OnEndClose(result);
        }

        protected class ReceiveAsyncResult : AsyncResult
        {
            private TItem item;

            public ReceiveAsyncResult(TItem item, AsyncCallback callback, object state) : base(callback, state)
            {
                this.item = item;
                base.Complete(true);
            }

            public static TItem End(IAsyncResult result)
            {
                return AsyncResult.End<ChannelWrapper<TChannel, TItem>.ReceiveAsyncResult>(result).item;
            }
        }

        protected class WaitAsyncResult : AsyncResult
        {
            public WaitAsyncResult(AsyncCallback callback, object state) : base(callback, state)
            {
                base.Complete(true);
            }

            public static bool End(IAsyncResult result)
            {
                AsyncResult.End<ChannelWrapper<TChannel, TItem>.WaitAsyncResult>(result);
                return true;
            }
        }
    }
}

