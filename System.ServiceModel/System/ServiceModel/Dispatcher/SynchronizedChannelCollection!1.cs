namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;

    internal class SynchronizedChannelCollection<TChannel> : SynchronizedCollection<TChannel> where TChannel: IChannel
    {
        private EventHandler onChannelClosed;
        private EventHandler onChannelFaulted;

        internal SynchronizedChannelCollection(object syncRoot) : base(syncRoot)
        {
            this.onChannelClosed = new EventHandler(this.OnChannelClosed);
            this.onChannelFaulted = new EventHandler(this.OnChannelFaulted);
        }

        private void AddingChannel(TChannel channel)
        {
            channel.Faulted += this.onChannelFaulted;
            channel.Closed += this.onChannelClosed;
        }

        protected override void ClearItems()
        {
            List<TChannel> items = base.Items;
            for (int i = 0; i < items.Count; i++)
            {
                this.RemovingChannel(items[i]);
            }
            base.ClearItems();
        }

        protected override void InsertItem(int index, TChannel item)
        {
            this.AddingChannel(item);
            base.InsertItem(index, item);
        }

        private void OnChannelClosed(object sender, EventArgs args)
        {
            TChannel item = (TChannel) sender;
            base.Remove(item);
        }

        private void OnChannelFaulted(object sender, EventArgs args)
        {
            TChannel item = (TChannel) sender;
            base.Remove(item);
        }

        protected override void RemoveItem(int index)
        {
            TChannel channel = base.Items[index];
            base.RemoveItem(index);
            this.RemovingChannel(channel);
        }

        private void RemovingChannel(TChannel channel)
        {
            channel.Faulted -= this.onChannelFaulted;
            channel.Closed -= this.onChannelClosed;
        }

        protected override void SetItem(int index, TChannel item)
        {
            TChannel channel = base.Items[index];
            this.AddingChannel(item);
            base.SetItem(index, item);
            this.RemovingChannel(channel);
        }
    }
}

