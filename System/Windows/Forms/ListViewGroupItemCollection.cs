namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;

    internal class ListViewGroupItemCollection : ListView.ListViewItemCollection.IInnerList
    {
        private ListViewGroup group;
        private ArrayList items;

        public ListViewGroupItemCollection(ListViewGroup group)
        {
            this.group = group;
        }

        public ListViewItem Add(ListViewItem value)
        {
            this.CheckListViewItem(value);
            this.MoveToGroup(value, this.group);
            this.Items.Add(value);
            return value;
        }

        public void AddRange(ListViewItem[] items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                this.CheckListViewItem(items[i]);
            }
            this.Items.AddRange(items);
            for (int j = 0; j < items.Length; j++)
            {
                this.MoveToGroup(items[j], this.group);
            }
        }

        private void CheckListViewItem(ListViewItem item)
        {
            if ((item.ListView != null) && (item.ListView != this.group.ListView))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("OnlyOneControl", new object[] { item.Text }), "item");
            }
        }

        public void Clear()
        {
            for (int i = 0; i < this.Count; i++)
            {
                this.MoveToGroup(this[i], null);
            }
            this.Items.Clear();
        }

        public bool Contains(ListViewItem item)
        {
            return this.Items.Contains(item);
        }

        public void CopyTo(Array dest, int index)
        {
            this.Items.CopyTo(dest, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        public int IndexOf(ListViewItem item)
        {
            return this.Items.IndexOf(item);
        }

        public ListViewItem Insert(int index, ListViewItem item)
        {
            this.CheckListViewItem(item);
            this.MoveToGroup(item, this.group);
            this.Items.Insert(index, item);
            return item;
        }

        private void MoveToGroup(ListViewItem item, ListViewGroup newGroup)
        {
            ListViewGroup group = item.Group;
            if (group != newGroup)
            {
                item.group = newGroup;
                if (group != null)
                {
                    group.Items.Remove(item);
                }
                this.UpdateNativeListViewItem(item);
            }
        }

        public void Remove(ListViewItem item)
        {
            this.Items.Remove(item);
            if (item.group == this.group)
            {
                item.group = null;
                this.UpdateNativeListViewItem(item);
            }
        }

        public void RemoveAt(int index)
        {
            this.Remove(this[index]);
        }

        private void UpdateNativeListViewItem(ListViewItem item)
        {
            if (((item.ListView != null) && item.ListView.IsHandleCreated) && !item.ListView.InsertingItemsNatively)
            {
                item.UpdateStateToListView(item.Index);
            }
        }

        public int Count
        {
            get
            {
                return this.Items.Count;
            }
        }

        public ListViewItem this[int index]
        {
            get
            {
                return (ListViewItem) this.Items[index];
            }
            set
            {
                if (value != this.Items[index])
                {
                    this.MoveToGroup((ListViewItem) this.Items[index], null);
                    this.Items[index] = value;
                    this.MoveToGroup((ListViewItem) this.Items[index], this.group);
                }
            }
        }

        private ArrayList Items
        {
            get
            {
                if (this.items == null)
                {
                    this.items = new ArrayList();
                }
                return this.items;
            }
        }

        public bool OwnerIsDesignMode
        {
            get
            {
                if (this.group.ListView == null)
                {
                    return false;
                }
                ISite site = this.group.ListView.Site;
                return ((site != null) && site.DesignMode);
            }
        }

        public bool OwnerIsVirtualListView
        {
            get
            {
                return ((this.group.ListView != null) && this.group.ListView.VirtualMode);
            }
        }
    }
}

