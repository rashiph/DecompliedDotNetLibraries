namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;

    [ListBindable(false)]
    public class ListViewGroupCollection : IList, ICollection, IEnumerable
    {
        private ArrayList list;
        private ListView listView;

        internal ListViewGroupCollection(ListView listView)
        {
            this.listView = listView;
        }

        public int Add(ListViewGroup group)
        {
            if (this.Contains(group))
            {
                return -1;
            }
            this.CheckListViewItems(group);
            group.ListViewInternal = this.listView;
            int num = this.List.Add(group);
            if (this.listView.IsHandleCreated)
            {
                this.listView.InsertGroupInListView(this.List.Count, group);
                this.MoveGroupItems(group);
            }
            return num;
        }

        public ListViewGroup Add(string key, string headerText)
        {
            ListViewGroup group = new ListViewGroup(key, headerText);
            this.Add(group);
            return group;
        }

        public void AddRange(ListViewGroup[] groups)
        {
            for (int i = 0; i < groups.Length; i++)
            {
                this.Add(groups[i]);
            }
        }

        public void AddRange(ListViewGroupCollection groups)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                this.Add(groups[i]);
            }
        }

        private void CheckListViewItems(ListViewGroup group)
        {
            for (int i = 0; i < group.Items.Count; i++)
            {
                ListViewItem item = group.Items[i];
                if ((item.ListView != null) && (item.ListView != this.listView))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("OnlyOneControl", new object[] { item.Text }));
                }
            }
        }

        public void Clear()
        {
            if (this.listView.IsHandleCreated)
            {
                for (int j = 0; j < this.Count; j++)
                {
                    this.listView.RemoveGroupFromListView(this[j]);
                }
            }
            for (int i = 0; i < this.Count; i++)
            {
                this[i].ListViewInternal = null;
            }
            this.List.Clear();
            this.listView.UpdateGroupView();
        }

        public bool Contains(ListViewGroup value)
        {
            return this.List.Contains(value);
        }

        public void CopyTo(Array array, int index)
        {
            this.List.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.List.GetEnumerator();
        }

        public int IndexOf(ListViewGroup value)
        {
            return this.List.IndexOf(value);
        }

        public void Insert(int index, ListViewGroup group)
        {
            if (!this.Contains(group))
            {
                group.ListViewInternal = this.listView;
                this.List.Insert(index, group);
                if (this.listView.IsHandleCreated)
                {
                    this.listView.InsertGroupInListView(index, group);
                    this.MoveGroupItems(group);
                }
            }
        }

        private void MoveGroupItems(ListViewGroup group)
        {
            foreach (ListViewItem item in group.Items)
            {
                if (item.ListView == this.listView)
                {
                    item.UpdateStateToListView(item.Index);
                }
            }
        }

        public void Remove(ListViewGroup group)
        {
            group.ListViewInternal = null;
            this.List.Remove(group);
            if (this.listView.IsHandleCreated)
            {
                this.listView.RemoveGroupFromListView(group);
            }
        }

        public void RemoveAt(int index)
        {
            this.Remove(this[index]);
        }

        int IList.Add(object value)
        {
            if (!(value is ListViewGroup))
            {
                throw new ArgumentException("value");
            }
            return this.Add((ListViewGroup) value);
        }

        bool IList.Contains(object value)
        {
            return ((value is ListViewGroup) && this.Contains((ListViewGroup) value));
        }

        int IList.IndexOf(object value)
        {
            if (value is ListViewGroup)
            {
                return this.IndexOf((ListViewGroup) value);
            }
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            if (value is ListViewGroup)
            {
                this.Insert(index, (ListViewGroup) value);
            }
        }

        void IList.Remove(object value)
        {
            if (value is ListViewGroup)
            {
                this.Remove((ListViewGroup) value);
            }
        }

        public int Count
        {
            get
            {
                return this.List.Count;
            }
        }

        public ListViewGroup this[int index]
        {
            get
            {
                return (ListViewGroup) this.List[index];
            }
            set
            {
                if (!this.List.Contains(value))
                {
                    this.List[index] = value;
                }
            }
        }

        public ListViewGroup this[string key]
        {
            get
            {
                if (this.list != null)
                {
                    for (int i = 0; i < this.list.Count; i++)
                    {
                        if (string.Compare(key, this[i].Name, false, CultureInfo.CurrentCulture) == 0)
                        {
                            return this[i];
                        }
                    }
                }
                return null;
            }
            set
            {
                int num = -1;
                if (this.list != null)
                {
                    for (int i = 0; i < this.list.Count; i++)
                    {
                        if (string.Compare(key, this[i].Name, false, CultureInfo.CurrentCulture) == 0)
                        {
                            num = i;
                            break;
                        }
                    }
                    if (num != -1)
                    {
                        this.list[num] = value;
                    }
                }
            }
        }

        private ArrayList List
        {
            get
            {
                if (this.list == null)
                {
                    this.list = new ArrayList();
                }
                return this.list;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return true;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                if (value is ListViewGroup)
                {
                    this[index] = (ListViewGroup) value;
                }
            }
        }
    }
}

