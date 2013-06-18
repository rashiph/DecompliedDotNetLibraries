namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    public class ChannelDispatcherCollection : SynchronizedCollection<ChannelDispatcherBase>
    {
        private ServiceHostBase service;

        internal ChannelDispatcherCollection(ServiceHostBase service, object syncRoot) : base(syncRoot)
        {
            if (service == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("service");
            }
            this.service = service;
        }

        protected override void ClearItems()
        {
            ChannelDispatcherBase[] array = new ChannelDispatcherBase[base.Count];
            base.CopyTo(array, 0);
            base.ClearItems();
            if (this.service != null)
            {
                foreach (ChannelDispatcherBase base2 in array)
                {
                    this.service.OnRemoveChannelDispatcher(base2);
                }
            }
        }

        protected override void InsertItem(int index, ChannelDispatcherBase item)
        {
            if (this.service != null)
            {
                if (this.service.State == CommunicationState.Closed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.service.GetType().ToString()));
                }
                this.service.OnAddChannelDispatcher(item);
            }
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            ChannelDispatcherBase channelDispatcher = base.Items[index];
            base.RemoveItem(index);
            if (this.service != null)
            {
                this.service.OnRemoveChannelDispatcher(channelDispatcher);
            }
        }

        protected override void SetItem(int index, ChannelDispatcherBase item)
        {
            ChannelDispatcherBase base2;
            if ((this.service != null) && (this.service.State == CommunicationState.Closed))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.service.GetType().ToString()));
            }
            if (this.service != null)
            {
                this.service.OnAddChannelDispatcher(item);
            }
            lock (base.SyncRoot)
            {
                base2 = base.Items[index];
                base.SetItem(index, item);
            }
            if (this.service != null)
            {
                this.service.OnRemoveChannelDispatcher(base2);
            }
        }
    }
}

