namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections;
    using System.ServiceModel;

    internal class CommunicationObjectManager<ItemType> : LifetimeManager where ItemType: class, ICommunicationObject
    {
        private bool inputClosed;
        private Hashtable table;

        public CommunicationObjectManager(object mutex) : base(mutex)
        {
            this.table = new Hashtable();
        }

        public void Add(ItemType item)
        {
            bool flag = false;
            lock (base.ThisLock)
            {
                if ((base.State == LifetimeState.Opened) && !this.inputClosed)
                {
                    if (this.table.ContainsKey(item))
                    {
                        return;
                    }
                    this.table.Add(item, item);
                    base.IncrementBusyCountWithoutLock();
                    item.Closed += new EventHandler(this.OnItemClosed);
                    flag = true;
                }
            }
            if (!flag)
            {
                item.Abort();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().ToString()));
            }
        }

        public void CloseInput()
        {
            this.inputClosed = true;
        }

        public void DecrementActivityCount()
        {
            base.DecrementBusyCount();
        }

        public void IncrementActivityCount()
        {
            this.IncrementBusyCount();
        }

        private void OnItemClosed(object sender, EventArgs args)
        {
            this.Remove((ItemType) sender);
        }

        public void Remove(ItemType item)
        {
            lock (base.ThisLock)
            {
                if (!this.table.ContainsKey(item))
                {
                    return;
                }
                this.table.Remove(item);
            }
            item.Closed -= new EventHandler(this.OnItemClosed);
            base.DecrementBusyCount();
        }

        public ItemType[] ToArray()
        {
            lock (base.ThisLock)
            {
                int num = 0;
                ItemType[] localArray = new ItemType[this.table.Keys.Count];
                foreach (ItemType local in this.table.Keys)
                {
                    localArray[num++] = local;
                }
                return localArray;
            }
        }
    }
}

