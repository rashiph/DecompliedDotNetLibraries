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

    public sealed class TreeNodeCollection : ICollection, IEnumerable, IStateManager
    {
        private bool _isTrackingViewState;
        private List<TreeNode> _list;
        private List<LogItem> _log;
        private TreeNode _owner;
        private bool _updateParent;
        private int _version;

        public TreeNodeCollection() : this(null, true)
        {
        }

        public TreeNodeCollection(TreeNode owner) : this(owner, true)
        {
        }

        internal TreeNodeCollection(TreeNode owner, bool updateParent)
        {
            this._owner = owner;
            this._list = new List<TreeNode>();
            this._updateParent = updateParent;
        }

        public void Add(TreeNode child)
        {
            this.AddAt(this.Count, child);
        }

        public void AddAt(int index, TreeNode child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }
            if (this._updateParent)
            {
                if ((child.Owner != null) && (child.Parent == null))
                {
                    child.Owner.Nodes.Remove(child);
                }
                if (child.Parent != null)
                {
                    child.Parent.ChildNodes.Remove(child);
                }
                if (this._owner != null)
                {
                    child.SetParent(this._owner);
                    child.SetOwner(this._owner.Owner);
                }
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
                    TreeView owner = this._owner.Owner;
                    if (owner != null)
                    {
                        if (owner.CheckedNodes.Count != 0)
                        {
                            owner.CheckedNodes.Clear();
                        }
                        for (TreeNode node = owner.SelectedNode; node != null; node = node.Parent)
                        {
                            if (this.Contains(node))
                            {
                                owner.SetSelectedNode(null);
                                break;
                            }
                        }
                    }
                }
                foreach (TreeNode node2 in this._list)
                {
                    node2.SetParent(null);
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

        public bool Contains(TreeNode c)
        {
            return this._list.Contains(c);
        }

        public void CopyTo(TreeNode[] nodeArray, int index)
        {
            ((ICollection) this).CopyTo(nodeArray, index);
        }

        internal TreeNode FindNode(string[] path, int pos)
        {
            if (pos == path.Length)
            {
                return this._owner;
            }
            string str = TreeView.UnEscape(path[pos]);
            for (int i = 0; i < this.Count; i++)
            {
                TreeNode node = this[i];
                if (node.Value == str)
                {
                    return node.ChildNodes.FindNode(path, pos + 1);
                }
            }
            return null;
        }

        public IEnumerator GetEnumerator()
        {
            return new TreeNodeCollectionEnumerator(this);
        }

        public int IndexOf(TreeNode value)
        {
            return this._list.IndexOf(value);
        }

        public void Remove(TreeNode value)
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
            TreeNode node = this._list[index];
            if (this._updateParent)
            {
                TreeView owner = node.Owner;
                if (owner != null)
                {
                    if (owner.CheckedNodes.Count != 0)
                    {
                        UnCheckUnSelectRecursive(node);
                    }
                    else
                    {
                        for (TreeNode node2 = owner.SelectedNode; node2 != null; node2 = node2.Parent)
                        {
                            if (node2 == node)
                            {
                                owner.SetSelectedNode(null);
                                break;
                            }
                        }
                    }
                }
                node.SetParent(null);
            }
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

        void ICollection.CopyTo(Array array, int index)
        {
            if (!(array is TreeNode[]))
            {
                throw new ArgumentException(System.Web.SR.GetString("TreeNodeCollection_InvalidArrayType"), "array");
            }
            this._list.CopyTo((TreeNode[]) array, index);
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
                            case LogItemType.Remove:
                                this.RemoveAt(index);
                                break;

                            case LogItemType.Clear:
                                this.Clear();
                                break;

                            case LogItemType.Insert:
                                if ((this._owner != null) && (this._owner.Owner != null))
                                {
                                    this.AddAt(index, this._owner.Owner.CreateNode());
                                }
                                else
                                {
                                    this.AddAt(index, new TreeNode());
                                }
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

        private static void UnCheckUnSelectRecursive(TreeNode node)
        {
            TreeNodeCollection checkedNodes = node.Owner.CheckedNodes;
            if (node.Checked)
            {
                checkedNodes.Remove(node);
            }
            TreeNode selectedNode = node.Owner.SelectedNode;
            if (node == selectedNode)
            {
                node.Owner.SetSelectedNode(null);
                selectedNode = null;
            }
            if ((selectedNode != null) || (checkedNodes.Count != 0))
            {
                foreach (TreeNode node3 in node.ChildNodes)
                {
                    UnCheckUnSelectRecursive(node3);
                }
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

        public TreeNode this[int index]
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
            private TreeNodeCollection.LogItemType _type;

            public LogItem(TreeNodeCollection.LogItemType type, int index, bool tracked)
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

            public TreeNodeCollection.LogItemType Type
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

        private class TreeNodeCollectionEnumerator : IEnumerator
        {
            private TreeNode currentElement;
            private int index;
            private TreeNodeCollection list;
            private int version;

            internal TreeNodeCollectionEnumerator(TreeNodeCollection list)
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

            public TreeNode Current
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

