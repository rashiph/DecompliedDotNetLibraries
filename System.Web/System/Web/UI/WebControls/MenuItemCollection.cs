namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Web.UI;

    public sealed class MenuItemCollection : ICollection, IEnumerable, IStateManager
    {
        private bool _isTrackingViewState;
        private List<MenuItem> _list;
        private List<LogItem> _log;
        private MenuItem _owner;
        private int _version;

        public MenuItemCollection() : this(null)
        {
        }

        public MenuItemCollection(MenuItem owner)
        {
            this._owner = owner;
            this._list = new List<MenuItem>();
        }

        public void Add(MenuItem child)
        {
            this.AddAt(this._list.Count, child);
        }

        public void AddAt(int index, MenuItem child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }
            if ((child.Owner != null) && (child.Parent == null))
            {
                child.Owner.Items.Remove(child);
            }
            if (child.Parent != null)
            {
                child.Parent.ChildItems.Remove(child);
            }
            if (this._owner != null)
            {
                child.SetParent(this._owner);
                child.SetOwner(this._owner.Owner);
            }
            this._list.Insert(index, child);
            this._version++;
            if (this._isTrackingViewState)
            {
                ((IStateManager) child).TrackViewState();
                child.SetDirty();
            }
            this.Log.Add(new LogItem(LogItemType.Insert, index, this._isTrackingViewState));
        }

        public void Clear()
        {
            if (this.Count != 0)
            {
                if (this._owner != null)
                {
                    Menu owner = this._owner.Owner;
                    if (owner != null)
                    {
                        for (MenuItem item = owner.SelectedItem; item != null; item = item.Parent)
                        {
                            if (this.Contains(item))
                            {
                                owner.SetSelectedItem(null);
                                break;
                            }
                        }
                    }
                }
                foreach (MenuItem item2 in this._list)
                {
                    item2.SetParent(null);
                }
                this._list.Clear();
                this._version++;
                if (this._isTrackingViewState)
                {
                    this.Log.Clear();
                }
                this.Log.Add(new LogItem(LogItemType.Clear, 0, this._isTrackingViewState));
            }
        }

        public bool Contains(MenuItem c)
        {
            return this._list.Contains(c);
        }

        public void CopyTo(Array array, int index)
        {
            if (!(array is MenuItem[]))
            {
                throw new ArgumentException(System.Web.SR.GetString("MenuItemCollection_InvalidArrayType"), "array");
            }
            this._list.CopyTo((MenuItem[]) array, index);
        }

        public void CopyTo(MenuItem[] array, int index)
        {
            this._list.CopyTo(array, index);
        }

        internal MenuItem FindItem(string[] path, int pos)
        {
            if (pos == path.Length)
            {
                return this._owner;
            }
            string str = TreeView.UnEscape(path[pos]);
            for (int i = 0; i < this.Count; i++)
            {
                MenuItem item = this._list[i];
                if (item.Value == str)
                {
                    return item.ChildItems.FindItem(path, pos + 1);
                }
            }
            return null;
        }

        public IEnumerator GetEnumerator()
        {
            return new MenuItemCollectionEnumerator(this);
        }

        public int IndexOf(MenuItem value)
        {
            return this._list.IndexOf(value);
        }

        public void Remove(MenuItem value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            int index = this._list.IndexOf(value);
            if (index != -1)
            {
                this.RemoveAt(index);
            }
        }

        public void RemoveAt(int index)
        {
            MenuItem item = this._list[index];
            Menu owner = item.Owner;
            if (owner != null)
            {
                for (MenuItem item2 = owner.SelectedItem; item2 != null; item2 = item2.Parent)
                {
                    if (item2 == item)
                    {
                        owner.SetSelectedItem(null);
                        break;
                    }
                }
            }
            item.SetParent(null);
            this._list.RemoveAt(index);
            this._version++;
            this.Log.Add(new LogItem(LogItemType.Remove, index, this._isTrackingViewState));
        }

        internal void SetDirty()
        {
            foreach (LogItem item in this.Log)
            {
                item.Tracked = true;
            }
            for (int i = 0; i < this.Count; i++)
            {
                this[i].SetDirty();
            }
        }

        void IStateManager.LoadViewState(object state)
        {
            object[] objArray = (object[]) state;
            if (objArray != null)
            {
                if (objArray[0] != null)
                {
                    string[] strArray = ((string) objArray[0]).Split(new char[] { ',' });
                    for (int j = 0; j < strArray.Length; j++)
                    {
                        string[] strArray2 = strArray[j].Split(new char[] { ':' });
                        LogItemType type = (LogItemType) int.Parse(strArray2[0], CultureInfo.InvariantCulture);
                        int index = int.Parse(strArray2[1], CultureInfo.InvariantCulture);
                        switch (type)
                        {
                            case LogItemType.Insert:
                                this.AddAt(index, new MenuItem());
                                break;

                            case LogItemType.Remove:
                                this.RemoveAt(index);
                                break;

                            case LogItemType.Clear:
                                this.Clear();
                                break;
                        }
                    }
                }
                for (int i = 0; i < (objArray.Length - 1); i++)
                {
                    if ((objArray[i + 1] != null) && (this[i] != null))
                    {
                        ((IStateManager) this[i]).LoadViewState(objArray[i + 1]);
                    }
                }
            }
        }

        object IStateManager.SaveViewState()
        {
            object[] objArray = new object[this.Count + 1];
            bool flag = false;
            if ((this._log != null) && (this._log.Count > 0))
            {
                StringBuilder builder = new StringBuilder();
                int num = 0;
                for (int j = 0; j < this._log.Count; j++)
                {
                    LogItem item = this._log[j];
                    if (item.Tracked)
                    {
                        builder.Append((int) item.Type);
                        builder.Append(":");
                        builder.Append(item.Index);
                        if (j < (this._log.Count - 1))
                        {
                            builder.Append(",");
                        }
                        num++;
                    }
                }
                if (num > 0)
                {
                    objArray[0] = builder.ToString();
                    flag = true;
                }
            }
            for (int i = 0; i < this.Count; i++)
            {
                objArray[i + 1] = ((IStateManager) this[i]).SaveViewState();
                if (objArray[i + 1] != null)
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                return null;
            }
            return objArray;
        }

        void IStateManager.TrackViewState()
        {
            this._isTrackingViewState = true;
            for (int i = 0; i < this.Count; i++)
            {
                ((IStateManager) this[i]).TrackViewState();
            }
        }

        public int Count
        {
            get
            {
                return this._list.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return ((ICollection) this._list).IsSynchronized;
            }
        }

        public MenuItem this[int index]
        {
            get
            {
                return this._list[index];
            }
        }

        private List<LogItem> Log
        {
            get
            {
                if (this._log == null)
                {
                    this._log = new List<LogItem>();
                }
                return this._log;
            }
        }

        public object SyncRoot
        {
            get
            {
                return ((ICollection) this._list).SyncRoot;
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this._isTrackingViewState;
            }
        }

        private class LogItem
        {
            private int _index;
            private bool _tracked;
            private MenuItemCollection.LogItemType _type;

            public LogItem(MenuItemCollection.LogItemType type, int index, bool tracked)
            {
                this._type = type;
                this._index = index;
                this._tracked = tracked;
            }

            public int Index
            {
                get
                {
                    return this._index;
                }
            }

            public bool Tracked
            {
                get
                {
                    return this._tracked;
                }
                set
                {
                    this._tracked = value;
                }
            }

            public MenuItemCollection.LogItemType Type
            {
                get
                {
                    return this._type;
                }
            }
        }

        private enum LogItemType
        {
            Insert,
            Remove,
            Clear
        }

        private class MenuItemCollectionEnumerator : IEnumerator
        {
            private MenuItem currentElement;
            private int index;
            private MenuItemCollection list;
            private int version;

            internal MenuItemCollectionEnumerator(MenuItemCollection list)
            {
                this.list = list;
                this.index = -1;
                this.version = list._version;
            }

            public bool MoveNext()
            {
                if (this.version != this.list._version)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ListEnumVersionMismatch"));
                }
                if (this.index < (this.list.Count - 1))
                {
                    this.index++;
                    this.currentElement = this.list[this.index];
                    return true;
                }
                this.index = this.list.Count;
                return false;
            }

            public void Reset()
            {
                if (this.version != this.list._version)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ListEnumVersionMismatch"));
                }
                this.currentElement = null;
                this.index = -1;
            }

            public MenuItem Current
            {
                get
                {
                    if (this.index == -1)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("ListEnumCurrentOutOfRange"));
                    }
                    if (this.index >= this.list.Count)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("ListEnumCurrentOutOfRange"));
                    }
                    return this.currentElement;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }
    }
}

